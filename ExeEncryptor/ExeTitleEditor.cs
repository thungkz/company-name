using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace ExeEncryptor;

public static class ExeTitleEditor
{
    private const int RT_DIALOG = 5;
    private const int RT_STRING = 6;
    private const int RT_VERSION = 16;

    private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
    private const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hLibModule);

    private delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, EnumResNameProc lpEnumFunc, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LockResource(IntPtr hResData);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

    public class StringTableBlock
    {
        public string[] Strings { get; } = new string[16];

        public static StringTableBlock Parse(byte[] data)
        {
            var block = new StringTableBlock();
            int offset = 0;
            for (int i = 0; i < 16; i++)
            {
                if (offset + 2 > data.Length) break;
                ushort len = BitConverter.ToUInt16(data, offset);
                offset += 2;
                if (len == 0)
                {
                    block.Strings[i] = string.Empty;
                }
                else
                {
                    if (offset + len * 2 > data.Length) break;
                    block.Strings[i] = Encoding.Unicode.GetString(data, offset, len * 2);
                    offset += len * 2;
                }
            }
            return block;
        }

        public byte[] ToBytes()
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                for (int i = 0; i < 16; i++)
                {
                    string s = Strings[i] ?? string.Empty;
                    bw.Write((ushort)s.Length);
                    if (s.Length > 0)
                    {
                        bw.Write(Encoding.Unicode.GetBytes(s));
                    }
                }
            }
            return ms.ToArray();
        }
    }

    public struct DetectedTitle
    {
        public string Source { get; set; }
        public string Title { get; set; }
        public object? ExtraData { get; set; }
    }

    private static byte[]? GetResourceBytes(IntPtr hModule, IntPtr name, IntPtr type)
    {
        IntPtr hResInfo = FindResource(hModule, name, type);
        if (hResInfo == IntPtr.Zero) return null;

        IntPtr hResData = LoadResource(hModule, hResInfo);
        if (hResData == IntPtr.Zero) return null;

        IntPtr pData = LockResource(hResData);
        if (pData == IntPtr.Zero) return null;

        uint size = SizeofResource(hModule, hResInfo);
        if (size == 0) return null;

        byte[] data = new byte[size];
        Marshal.Copy(pData, data, 0, (int)size);
        return data;
    }

    private static string ParseDialogTitle(byte[] data)
    {
        if (data.Length < 18) return string.Empty;

        ushort dlgVer = BitConverter.ToUInt16(data, 0);
        ushort signature = BitConverter.ToUInt16(data, 2);

        int offset = 0;
        if (dlgVer == 1 && signature == 0xFFFF)
        {
            offset = 26;
        }
        else
        {
            offset = 18;
        }

        if (offset + 2 > data.Length) return string.Empty;
        ushort menuType = BitConverter.ToUInt16(data, offset);
        if (menuType == 0x0000)
        {
            offset += 2;
        }
        else if (menuType == 0xFFFF)
        {
            offset += 4;
        }
        else
        {
            while (offset + 2 <= data.Length)
            {
                ushort ch = BitConverter.ToUInt16(data, offset);
                offset += 2;
                if (ch == 0) break;
            }
        }

        if (offset + 2 > data.Length) return string.Empty;
        ushort classType = BitConverter.ToUInt16(data, offset);
        if (classType == 0x0000)
        {
            offset += 2;
        }
        else if (classType == 0xFFFF)
        {
            offset += 4;
        }
        else
        {
            while (offset + 2 <= data.Length)
            {
                ushort ch = BitConverter.ToUInt16(data, offset);
                offset += 2;
                if (ch == 0) break;
            }
        }

        int titleStart = offset;
        while (offset + 2 <= data.Length)
        {
            ushort ch = BitConverter.ToUInt16(data, offset);
            offset += 2;
            if (ch == 0) break;
        }

        int titleLenBytes = offset - titleStart - 2;
        if (titleLenBytes > 0)
        {
            return Encoding.Unicode.GetString(data, titleStart, titleLenBytes);
        }

        return string.Empty;
    }

    public static List<DetectedTitle> DetectTitles(string exePath)
    {
        var list = new List<DetectedTitle>();
        if (!File.Exists(exePath)) return list;

        try
        {
            var titleBar = ExeVersionEditor.GetVersionInfo(exePath, "TitleBar", offset: 0);
            if (!string.IsNullOrWhiteSpace(titleBar) && !titleBar.StartsWith("Error:") && !titleBar.StartsWith("Version info key"))
            {
                list.Add(new DetectedTitle { Source = "VersionInfo (TitleBar)", Title = titleBar, ExtraData = "TitleBar" });
            }
            var fileDesc = ExeVersionEditor.GetVersionInfo(exePath, "FileDescription", offset: 0);
            if (!string.IsNullOrWhiteSpace(fileDesc) && !fileDesc.StartsWith("Error:") && !fileDesc.StartsWith("Version info key"))
            {
                list.Add(new DetectedTitle { Source = "VersionInfo (FileDescription)", Title = fileDesc, ExtraData = "FileDescription" });
            }
            var prodName = ExeVersionEditor.GetVersionInfo(exePath, "ProductName", offset: 0);
            if (!string.IsNullOrWhiteSpace(prodName) && !prodName.StartsWith("Error:") && !prodName.StartsWith("Version info key"))
            {
                list.Add(new DetectedTitle { Source = "VersionInfo (ProductName)", Title = prodName, ExtraData = "ProductName" });
            }
        }
        catch { }

        IntPtr hModule = LoadLibraryEx(exePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        if (hModule != IntPtr.Zero)
        {
            try
            {
                EnumResourceNames(hModule, (IntPtr)RT_STRING, (h, type, name, param) =>
                {
                    int blockId = (int)name;
                    byte[]? data = GetResourceBytes(h, name, type);
                    if (data != null)
                    {
                        var block = StringTableBlock.Parse(data);
                        int startId = (blockId - 1) * 16;
                        for (int i = 0; i < 16; i++)
                        {
                            string s = block.Strings[i];
                            if (!string.IsNullOrWhiteSpace(s) && s.Length >= 2 && s.Length <= 120)
                            {
                                list.Add(new DetectedTitle
                                {
                                    Source = $"StringTable (ID {startId + i})",
                                    Title = s.Trim(),
                                    ExtraData = new int[] { RT_STRING, startId + i, blockId, i }
                                });
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);

                EnumResourceNames(hModule, (IntPtr)RT_DIALOG, (h, type, name, param) =>
                {
                    if (((long)name >> 16) == 0)
                    {
                        int dlgId = (int)name;
                        byte[]? data = GetResourceBytes(h, name, type);
                        if (data != null)
                        {
                            string dlgTitle = ParseDialogTitle(data);
                            if (!string.IsNullOrWhiteSpace(dlgTitle) && dlgTitle.Length >= 2 && dlgTitle.Length <= 120)
                            {
                                list.Add(new DetectedTitle
                                {
                                    Source = $"Dialog (ID {dlgId})",
                                    Title = dlgTitle.Trim(),
                                    ExtraData = new object[] { RT_DIALOG, dlgId }
                                });
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch { }
            finally
            {
                FreeLibrary(hModule);
            }
        }

        try
        {
            var binaryTitles = ScanBinaryForTitleStrings(exePath);
            foreach (var t in binaryTitles)
                list.Add(t);
        }
        catch { }

        return list;
    }

    private static List<DetectedTitle> ScanBinaryForTitleStrings(string exePath)
    {
        var result = new List<DetectedTitle>();
        byte[] bytes = File.ReadAllBytes(exePath);

        var skipKeywords = new[] {
            "api-ms-win", "kernel32", "user32", "ntdll", "msvcrt", "ucrtbase",
            "System.", "Microsoft.", "Windows.", ".dll", ".exe", ".pdb",
            "http://", "https://", "\\\\", "Exception", "ERROR", "WARNING",
            "InitializeComponent", "SuspendLayout", "ResumeLayout",
            "AutoScaleDimensions", "AutoScaleMode", "ClientSize", "Location",
            "__", "->", "::", "0x", "\\n", "\\t"
        };

        var seen = new System.Collections.Generic.HashSet<string>();

        // 1. Scan Unicode (UTF-16) Strings
        for (int i = 0; i <= bytes.Length - 4; i += 2)
        {
            int strLen = 0;
            bool ok = true;
            for (int k = i; k <= bytes.Length - 2 && k < i + 180; k += 2)
            {
                ushort ch = BitConverter.ToUInt16(bytes, k);
                if (ch == 0x0000) { strLen = (k - i) / 2; break; }
                if (!((ch >= 0x0020 && ch <= 0x007E) ||
                      (ch >= 0x00A0 && ch <= 0x00FF) ||
                      ch == 0x2019 || ch == 0x2018 || ch == 0x2014 || ch == 0x2013))
                {
                    ok = false; break;
                }
            }

            if (!ok || strLen < 2 || strLen > 80) continue;

            string s = Encoding.Unicode.GetString(bytes, i, strLen * 2).Trim();
            if (s.Length < 2 || seen.Contains(s)) continue;

            bool skip = false;
            foreach (var kw in skipKeywords)
                if (s.Contains(kw))
                {
                    skip = true; break;
                }
            if (skip) continue;

            if (s.All(c => char.IsDigit(c) || c == '.' || c == '-' || c == '_')) continue;
            if (s.Contains("\\") || s.Contains("/") || s.StartsWith(".")) continue;
            if (!s.Any(char.IsLetter)) continue;

            seen.Add(s);
            result.Add(new DetectedTitle
            {
                Source = $"Binary Unicode (offset 0x{i:X8})",
                Title = s,
                ExtraData = null
            });
        }

        // 2. Scan ASCII / ANSI Strings
        for (int i = 0; i <= bytes.Length - 2; i++)
        {
            int strLen = 0;
            bool ok = true;
            for (int k = i; k < bytes.Length && k < i + 90; k++)
            {
                byte b = bytes[k];
                if (b == 0x00) { strLen = k - i; break; }
                if (b < 0x20 || b > 0x7E)
                {
                    ok = false; break;
                }
            }

            if (!ok || strLen < 2 || strLen > 80) continue;

            string s = Encoding.ASCII.GetString(bytes, i, strLen).Trim();
            if (s.Length < 2 || seen.Contains(s)) continue;

            bool skip = false;
            foreach (var kw in skipKeywords)
                if (s.Contains(kw))
                {
                    skip = true; break;
                }
            if (skip) continue;

            if (s.All(c => char.IsDigit(c) || c == '.' || c == '-' || c == '_')) continue;
            if (s.Contains("\\") || s.Contains("/") || s.StartsWith(".")) continue;
            if (!s.Any(char.IsLetter)) continue;

            seen.Add(s);
            result.Add(new DetectedTitle
            {
                Source = $"Binary ASCII (offset 0x{i:X8})",
                Title = s,
                ExtraData = null
            });
        }

        return result;
    }

    public static string DetectCurrentTitle(string exePath)
    {
        var titles = DetectTitles(exePath);
        if (titles.Count > 0)
        {
            foreach (var t in titles)
                if (t.Source.StartsWith("VersionInfo (TitleBar)")) return t.Title;

            foreach (var t in titles)
                if (t.Source.StartsWith("StringTable (ID 57344)") || t.Source.StartsWith("StringTable (ID 128)")) return t.Title;

            foreach (var t in titles)
                if (t.Source.StartsWith("StringTable")) return t.Title;

            foreach (var t in titles)
                if (t.Source.StartsWith("Dialog")) return t.Title;

            DetectedTitle best = default;
            int bestLen = 0;
            foreach (var t in titles)
            {
                if (t.Source.StartsWith("Binary") && t.Title.Length > bestLen)
                {
                    best = t;
                    bestLen = t.Title.Length;
                }
            }
            if (bestLen > 0) return best.Title;

            return titles[0].Title;
        }

        return string.Empty;
    }

    private static bool WriteResource(string exePath, IntPtr name, IntPtr type, byte[] lpData)
    {
        IntPtr hUpdate = BeginUpdateResource(exePath, false);
        if (hUpdate == IntPtr.Zero) return false;

        bool success = UpdateResource(hUpdate, type, name, 0, lpData, (uint)lpData.Length);
        if (!EndUpdateResource(hUpdate, !success))
        {
            return false;
        }
        return success;
    }

    public static bool UpdateStringTableEntry(string exePath, int stringId, string newTitle)
    {
        int blockId = (stringId >> 4) + 1;
        int indexInBlock = stringId % 16;

        IntPtr hModule = LoadLibraryEx(exePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        if (hModule == IntPtr.Zero) return false;

        byte[]? data = null;
        try
        {
            data = GetResourceBytes(hModule, (IntPtr)blockId, (IntPtr)RT_STRING);
        }
        finally
        {
            FreeLibrary(hModule);
        }

        StringTableBlock block;
        if (data != null)
        {
            block = StringTableBlock.Parse(data);
        }
        else
        {
            block = new StringTableBlock();
        }

        block.Strings[indexInBlock] = newTitle;
        byte[] newData = block.ToBytes();

        return WriteResource(exePath, (IntPtr)blockId, (IntPtr)RT_STRING, newData);
    }

    public static bool UpdateDialogTitle(string exePath, int dialogId, string newTitle)
    {
        IntPtr hModule = LoadLibraryEx(exePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_AS_IMAGE_RESOURCE);
        if (hModule == IntPtr.Zero) return false;

        byte[]? data = null;
        try
        {
            data = GetResourceBytes(hModule, (IntPtr)dialogId, (IntPtr)RT_DIALOG);
        }
        finally
        {
            FreeLibrary(hModule);
        }

        if (data == null) return false;

        ushort dlgVer = BitConverter.ToUInt16(data, 0);
        ushort signature = BitConverter.ToUInt16(data, 2);
        int offset = (dlgVer == 1 && signature == 0xFFFF) ? 26 : 18;

        if (offset + 2 > data.Length) return false;
        ushort menuType = BitConverter.ToUInt16(data, offset);
        if (menuType == 0x0000) offset += 2;
        else if (menuType == 0xFFFF) offset += 4;
        else
        {
            while (offset + 2 <= data.Length)
            {
                ushort ch = BitConverter.ToUInt16(data, offset);
                offset += 2;
                if (ch == 0) break;
            }
        }

        if (offset + 2 > data.Length) return false;
        ushort classType = BitConverter.ToUInt16(data, offset);
        if (classType == 0x0000) offset += 2;
        else if (classType == 0xFFFF) offset += 4;
        else
        {
            while (offset + 2 <= data.Length)
            {
                ushort ch = BitConverter.ToUInt16(data, offset);
                offset += 2;
                if (ch == 0) break;
            }
        }

        int titleStart = offset;
        while (offset + 2 <= data.Length)
        {
            ushort ch = BitConverter.ToUInt16(data, offset);
            offset += 2;
            if (ch == 0) break;
        }
        int titleEnd = offset;

        byte[] beforeTitle = new byte[titleStart];
        Array.Copy(data, 0, beforeTitle, 0, titleStart);

        byte[] newTitleBytes = Encoding.Unicode.GetBytes(newTitle + "\0");

        int afterLen = data.Length - titleEnd;
        byte[] afterTitle = new byte[afterLen];
        Array.Copy(data, titleEnd, afterTitle, 0, afterLen);

        byte[] newData = new byte[beforeTitle.Length + newTitleBytes.Length + afterTitle.Length];
        Array.Copy(beforeTitle, 0, newData, 0, beforeTitle.Length);
        Array.Copy(newTitleBytes, 0, newData, beforeTitle.Length, newTitleBytes.Length);
        Array.Copy(afterTitle, 0, newData, beforeTitle.Length + newTitleBytes.Length, afterTitle.Length);

        return WriteResource(exePath, (IntPtr)dialogId, (IntPtr)RT_DIALOG, newData);
    }

    public static bool PatchBinaryString(string exePath, string oldTitle, string newTitle, out string resultMessage)
    {
        resultMessage = "";
        if (string.IsNullOrEmpty(oldTitle))
        {
            resultMessage = "Old title is empty.";
            return false;
        }

        // ✅ 第一步：尝试直接扩展
        if (TryPatchInPlace(exePath, oldTitle, newTitle, out resultMessage, extended: true))
        {
            return true;
        }

        // ✅ 第二步：直接扩展失败，用"两阶段"策略（独特临时字符串）
        if (oldTitle.Length > 1)
        {
            string tempTitle = "~~~T~~~";
            if (TryPatchInPlace(exePath, oldTitle, tempTitle, out string stage1Msg, extended: true))
            {
                if (TryPatchInPlace(exePath, tempTitle, newTitle, out string stage2Msg, extended: true))
                {
                    resultMessage = $"Two-stage successful! S1: {stage1Msg} | S2: {stage2Msg}";
                    return true;
                }
                else
                {
                    resultMessage = $"Stage1 OK, Stage2 failed: {stage2Msg}";
                    return false;
                }
            }
            else
            {
                resultMessage = $"Stage1 failed: {stage1Msg}";
                return false;
            }
        }

        return false;
    }

    private static bool TryPatchInPlace(string exePath, string oldTitle, string newTitle,
                                         out string resultMessage, bool extended)
    {
        resultMessage = "";
        try
        {
            byte[] fileBytes = File.ReadAllBytes(exePath);
            byte[] oldUnicode = Encoding.Unicode.GetBytes(oldTitle);
            byte[] newUnicode = Encoding.Unicode.GetBytes(newTitle);

            bool anyPatched = false;
            int unicodeCount = 0;
            int asciiCount = 0;

            for (int i = 0; i <= fileBytes.Length - oldUnicode.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < oldUnicode.Length; j++)
                {
                    if (fileBytes[i + j] != oldUnicode[j]) { match = false; break; }
                }
                if (!match) continue;

                int availableSpace = oldUnicode.Length;
                if (extended)
                {
                    int k = i + oldUnicode.Length;
                    int maxLookAhead = oldUnicode.Length + 2048; // Expanded from 128 to 2048 to prevent truncating long strings
                    int limit = Math.Min(i + maxLookAhead, fileBytes.Length);

                    while (k < limit && fileBytes[k] == 0x00)
                    {
                        availableSpace += 2;
                        k += 2;
                    }
                }

                int clearEnd = i + availableSpace;
                if (clearEnd > fileBytes.Length) clearEnd = fileBytes.Length;
                for (int m = i; m < clearEnd; m++)
                    fileBytes[m] = 0x00;

                int bytesToCopy = newUnicode.Length;
                if (bytesToCopy > availableSpace)
                {
                    // Truncate to fit perfectly within available space
                    bytesToCopy = (availableSpace / 2) * 2; // Keep unicode character boundary
                }

                Array.Copy(newUnicode, 0, fileBytes, i, bytesToCopy);

                // Zero out the entire remainder of the available space to wipe out old text remnants completely
                int bytesCopied = bytesToCopy;
                int remainingStartIndex = i + bytesCopied;
                int remainingLength = availableSpace - bytesCopied;
                if (remainingLength > 0 && remainingStartIndex + remainingLength <= fileBytes.Length)
                {
                    for (int fillIdx = 0; fillIdx < remainingLength; fillIdx++)
                    {
                        fileBytes[remainingStartIndex + fillIdx] = 0x00;
                    }
                }

                int newTotalBytes = bytesToCopy + 1;
                bool isUsHeapEntry = i > 0
                    && (oldUnicode.Length + 1) < 128
                    && fileBytes[i - 1] == (byte)(oldUnicode.Length + 1);

                if (isUsHeapEntry && newTotalBytes < 128)
                {
                    fileBytes[i - 1] = (byte)newTotalBytes;
                }

                unicodeCount++;
                anyPatched = true;
                i += availableSpace - 1;
            }

            byte[] oldAscii = Encoding.ASCII.GetBytes(oldTitle);
            byte[] newAscii = Encoding.ASCII.GetBytes(newTitle);

            if (oldAscii.Length <= fileBytes.Length)
            {
                for (int i = 0; i <= fileBytes.Length - oldAscii.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < oldAscii.Length; j++)
                    {
                        if (fileBytes[i + j] != oldAscii[j]) { match = false; break; }
                    }
                    if (!match) continue;

                    int availableSpace = oldAscii.Length;
                    if (extended)
                    {
                        int k = i + oldAscii.Length;
                        int maxLookAhead = oldAscii.Length + 2048; // Expanded from 64 to 2048 to prevent truncating long strings
                        int limit = Math.Min(i + maxLookAhead, fileBytes.Length);
                        while (k < limit && fileBytes[k] == 0x00)
                        {
                            availableSpace++;
                            k++;
                        }
                    }

                    int clearEnd = i + availableSpace;
                    if (clearEnd > fileBytes.Length) clearEnd = fileBytes.Length;
                    for (int m = i; m < clearEnd; m++)
                        fileBytes[m] = 0x00;

                    int bytesToCopy = newAscii.Length;
                    if (bytesToCopy > availableSpace)
                    {
                        bytesToCopy = availableSpace;
                    }
                    Array.Copy(newAscii, 0, fileBytes, i, bytesToCopy);

                    // Zero out the entire remainder of the available space to wipe out old text remnants completely
                    int bytesCopied = bytesToCopy;
                    int remainingStartIndex = i + bytesCopied;
                    int remainingLength = availableSpace - bytesCopied;
                    if (remainingLength > 0 && remainingStartIndex + remainingLength <= fileBytes.Length)
                    {
                        for (int fillIdx = 0; fillIdx < remainingLength; fillIdx++)
                        {
                            fileBytes[remainingStartIndex + fillIdx] = 0x00;
                        }
                    }

                    asciiCount++;
                    anyPatched = true;
                    i += availableSpace - 1;
                }
            }

            if (anyPatched)
            {
                File.WriteAllBytes(exePath, fileBytes);
                resultMessage = $"Patched {unicodeCount} Unicode, {asciiCount} ASCII.";
                return true;
            }

            resultMessage = "Title string not found in binary data.";
            return false;
        }
        catch (Exception ex)
        {
            resultMessage = $"Patch error: {ex.Message}";
            return false;
        }
    }

    public static bool ChangeTitle(string exePath, string oldTitle, string newTitle, out string status)
    {
        status = "";
        if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
        {
            status = "Please select a valid EXE file.";
            return false;
        }

        bool resourceUpdated = false;
        var detected = DetectTitles(exePath);

        foreach (var item in detected)
        {
            if (item.Title == oldTitle)
            {
                if (item.Source.StartsWith("VersionInfo") && item.ExtraData is string entry)
                {
                    int offset = (entry == "Comments" || entry == "ProductVersion") ? 2 : 0;
                    ExeVersionEditor.GetVersionInfo(exePath, entry, unicode: true, update: true, updateName: newTitle, offset: offset);
                    resourceUpdated = true;
                }
                else if (item.Source.StartsWith("StringTable") && item.ExtraData is int[] info)
                {
                    if (UpdateStringTableEntry(exePath, info[1], newTitle))
                    {
                        resourceUpdated = true;
                    }
                }
                else if (item.Source.StartsWith("Dialog") && item.ExtraData is object[] infoObj)
                {
                    if (UpdateDialogTitle(exePath, (int)infoObj[1], newTitle))
                    {
                        resourceUpdated = true;
                    }
                }
            }
        }

        string patchMsg = "";
        bool binaryPatched = false;
        if (!string.IsNullOrEmpty(oldTitle))
        {
            binaryPatched = PatchBinaryString(exePath, oldTitle, newTitle, out patchMsg);
        }

        if (resourceUpdated || binaryPatched)
        {
            status = $"Resources updated: {resourceUpdated}. {patchMsg}";
            return true;
        }

        status = "Could not find current title in resources or binary.";
        return false;
    }
}