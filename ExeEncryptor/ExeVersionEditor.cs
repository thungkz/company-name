using System;
using System.IO;
using System.Text;

namespace ExeEncryptor;

public static class ExeVersionEditor
{
    /// <summary>
    /// Port of C++ SearchMemory logic.
    /// Searches for target version key and reads or updates its value in the byte buffer.
    /// </summary>
    public static bool SearchMemory(
        byte[] text,
        int nBytesRead,
        string csEntry,
        ref string csRet,
        bool unicode,
        bool update,
        string? updateName,
        int offset,
        int ploc)
    {
        bool bUpdate = false;
        byte[] sz_oldIP = new byte[1024];
        byte[] sz_newIP = new byte[1024];

        int len = csEntry.Length;
        int j = 0;
        for (int i = 0; i <= len; i++)
        {
            char ch = (i < len) ? csEntry[i] : '\0';
            sz_oldIP[j++] = (byte)ch;
            if (unicode)
            {
                sz_oldIP[j++] = 0;
            }
        }
        len = j - offset;

        j = 0;
        for (int i = 0; i < nBytesRead; i++)
        {
            if (text[i] == sz_oldIP[j])
            {
                if (j == len)
                {
                    if (update)
                    {
                        byte[] tmp = new byte[1000];
                        if (!string.IsNullOrEmpty(updateName))
                        {
                            byte[] updateBytes = Encoding.ASCII.GetBytes(updateName);
                            Array.Copy(updateBytes, tmp, Math.Min(updateBytes.Length, 1000));
                        }

                        int tmpIdx = 0;
                        for (int k = 0; k < 100; k += 2)
                        {
                            if (i + k + 2 - ploc >= text.Length) break;

                            byte ch = tmp[tmpIdx++];
                            text[i + k + 2 - ploc] = ch;
                            if (ch == 0) break;
                        }
                        csRet = updateName ?? string.Empty;
                    }
                    else
                    {
                        int tmpIdx = 0;
                        for (int k = 0; k < 100; k += 2)
                        {
                            if (i + k + 2 - ploc >= text.Length) break;

                            byte ch = text[i + k + 2 - ploc];
                            sz_newIP[tmpIdx++] = ch;
                            if (ch == 0) break;
                        }
                        // Trim trailing nulls and convert to string
                        int validLen = 0;
                        while (validLen < tmpIdx && sz_newIP[validLen] != 0)
                        {
                            validLen++;
                        }
                        csRet = Encoding.ASCII.GetString(sz_newIP, 0, validLen);
                    }
                    return true;
                }
                j++;
            }
            else
            {
                j = 0;
            }
        }
        return bUpdate;
    }

    /// <summary>
    /// Retrieves or updates a version property in the specified EXE file.
    /// </summary>
    public static string GetVersionInfo(
        string filePath,
        string csEntry,
        bool unicode = true,
        bool update = false,
        string? updateName = null,
        int offset = 0,
        int ploc = 0)
    {
        string csRet = "";
        if (!File.Exists(filePath)) return csRet;

        try
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            bool isUpdated = SearchMemory(
                fileBytes,
                fileBytes.Length,
                csEntry,
                ref csRet,
                unicode,
                update,
                updateName,
                offset,
                ploc);

            if (isUpdated && update)
            {
                File.WriteAllBytes(filePath, fileBytes);
            }
        }
        catch (Exception ex)
        {
            csRet = $"Error: {ex.Message}";
        }

        return csRet;
    }

    /// <summary>
    /// Renames a physical file on disk.
    /// </summary>
    public static bool RenamePhysicalFile(string oldPath, string newName, out string newPath, out string errorMessage)
    {
        newPath = oldPath;
        errorMessage = string.Empty;

        if (!File.Exists(oldPath))
        {
            errorMessage = "Source file does not exist.";
            return false;
        }

        try
        {
            string? directory = Path.GetDirectoryName(oldPath);
            newPath = Path.Combine(directory ?? "", newName);

            if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
            {
                return true; // No change needed
            }

            if (File.Exists(newPath))
            {
                errorMessage = "A file with the new name already exists.";
                return false;
            }

            File.Move(oldPath, newPath);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
