using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExeEncryptor;

public partial class Form1 : Form
{
    private TextBox? lastFocusedTextBox;
    private List<string> _scannedDecodedCompanyNames = new();
    // Byte ranges (offset + length + encoding) of spurious company codes found during last scan.
    // These are physically zeroed out of the binary during rename to prevent ghost results on future scans.
    private List<(int Offset, int ByteLength, string Encoding)> _scannedSpuriousRanges = new();

    public Form1()
    {
        InitializeComponent();
        InitTextBoxEvents();
        InitTitleChangerDragDrop();
    }

    private void InitTextBoxEvents()
    {
        txtTextInput.Enter += (s, e) => lastFocusedTextBox = txtTextInput;
        txtTextOutput.Enter += (s, e) => lastFocusedTextBox = txtTextOutput;
    }

    private void ApplyTransform(Func<string, string> transform)
    {
        var tb = lastFocusedTextBox ?? txtTextInput;
        tb.Text = transform(tb.Text);
        tb.Focus();
    }

    // ── Text Transform Button Handlers ─────────────────────────────────────

    private void BtnUppercase_Click(object sender, EventArgs e)
        => ApplyTransform(s => s.ToUpper());

    private void BtnLowercase_Click(object sender, EventArgs e)
        => ApplyTransform(s => s.ToLower());

    private void BtnCapitalize_Click(object sender, EventArgs e)
        => ApplyTransform(s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()));

    // ── Encrypt / Decrypt Button Handler ───────────────────────────────────

    private void BtnConvertText_Click(object sender, EventArgs e)
    {
        string input = txtTextInput.Text;
        if (string.IsNullOrEmpty(input))
        {
            txtTextOutput.Text = string.Empty;
            return;
        }

        try
        {
            if (rbEncrypt.Checked)
            {
                txtTextOutput.Text = Encryptor.EncryptText(input);
            }
            else
            {
                txtTextOutput.Text = Encryptor.DecryptText(input);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Dynamically initialized 2000-char encoded placeholder (decodes to a 1000-char valid company name)
    // This creates a 4000-byte UTF-16 slot in the binary, allowing company names up to 1000 chars.
    // ── Company Codec (must match Encryptor.cs mapping exactly) ─────────────
    // Alphabet index:  0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15
    // Alphabet chars:  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  G
    // nibble n → alphabet[(14-n)&0xF]
    private static readonly string CompanyAlphabet = "0123456789ABCDEG";

    // Large static literal so the binary contains a searchable 2000-char slot
    // for company names up to 1000 characters.  Decodes to "domaindomain...doma"
    // (1000 chars of letters), which passes IsValidCompanyName and is found by
    // the scanner → isCompanyCodeRename = true → uses the large 2000-char slot.
    private static readonly string EmbeddedCompanyCode =
        // "domain" × 166 + "doma" = 1000 decoded chars → passes IsValidCompanyName
        // "A8G818D85808" = encode("domain"); "A8G818D8" = encode("doma")
        // 166 × 12 + 8 = 2000 encoded chars → 1000-char company name slot
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808" +
        "A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D85808A8G818D8";
        // 25 lines × 80 chars = 2000 chars total → 1000-char company name capacity

    private static string EncodeCompanyName(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var sb = new StringBuilder(input.Length * 2);
        foreach (char c in input)
        {
            int lo = c & 0x0F;
            int hi = (c >> 4) & 0x0F;
            sb.Append(CompanyAlphabet[(14 - lo) & 0xF]);
            sb.Append(CompanyAlphabet[(14 - hi) & 0xF]);
        }
        return sb.ToString();
    }

    private static string DecodeCompanyName(string encoded)
    {
        if (encoded.Length % 2 != 0) return "";
        var sb = new StringBuilder(encoded.Length / 2);
        for (int i = 0; i < encoded.Length; i += 2)
        {
            int v1 = CompanyAlphabet.IndexOf(encoded[i]);
            int v2 = CompanyAlphabet.IndexOf(encoded[i + 1]);
            if (v1 < 0 || v2 < 0) return "";        // invalid encoded char → reject whole candidate
            int lo = (14 - v1) & 0xF;
            int hi = (14 - v2) & 0xF;
            char c = (char)((hi << 4) | lo);
            if (c < 0x20 || c > 0x7E) return "";     // non-printable ASCII → reject whole candidate
            sb.Append(c);
        }
        return sb.ToString();
    }

    private static bool IsValidCompanyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        name = name.Trim();
        
        // Allow short names again (at least 2 characters) so real short names work.
        if (name.Length < 2) return false;

        // Specifically block leftover placeholder fragments of "domaindomain..."
        string lower = name.ToLower();
        if (lower == "doma" || lower == "domain" || lower == "domaindo") return false;


        // Aggressively reject any name containing non-printable or garbage chars
        // Safe company characters are: letters, digits, spaces, dots, commas, parentheses, hyphens, and ampersands.
        // Any character outside this white list will trigger immediate rejection.
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c)) continue;
            if (c == ' ' || c == '.' || c == ',' || c == '-' || c == '&' || c == '(' || c == ')') continue;
            return false; // Rejects any symbol noise like ^, ", $, \, !, #, ~, <, {, @, ;, `, etc.
        }

        // Reject names that consist of a single repeated character (e.g. "DDDDDDDD", "UUUUUUUU")
        bool allSame = true;
        for (int idx = 1; idx < name.Length; idx++)
        {
            if (name[idx] != name[0]) { allSame = false; break; }
        }
        if (allSame) return false;

        return true;
    }

    private static int GetOccurrenceCount(string text, string value)
    {
        int count = 0;
        int minIndex = text.IndexOf(value, StringComparison.Ordinal);
        while (minIndex != -1)
        {
            count++;
            if (minIndex + value.Length > text.Length) break;
            minIndex = text.IndexOf(value, minIndex + value.Length, StringComparison.Ordinal);
        }
        return count;
    }

    private static bool IsSubstringOfExisting(string decoded, HashSet<string> seenDecoded)
    {
        string decodedLower = decoded.ToLowerInvariant();
        string decodedNorm = System.Text.RegularExpressions.Regex.Replace(
            decodedLower.Replace(".", ""), @"\s+", " ").Trim();

        foreach (var existing in seenDecoded)
        {
            string existingLower = existing.ToLowerInvariant();
            string existingNorm = System.Text.RegularExpressions.Regex.Replace(
                existingLower.Replace(".", ""), @"\s+", " ").Trim();

            // Direct substring match (normalized)
            if (existingNorm.Contains(decodedNorm) || decodedNorm.Contains(existingNorm))
                return true;

            // Both share the same company suffix keyword (normalized) and the new one is shorter
            // — keep only the first (longer) match, discard the shorter one
            string[] companySuffixes = { "sdn bhd", " ltd", " inc", " corp", " berhad", " plc", " llc" };
            foreach (var suffix in companySuffixes)
            {
                if (existingNorm.Contains(suffix) && decodedNorm.Contains(suffix)
                    && decoded.Length < existing.Length)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static List<(string Encoded, string Decoded, int Offset, string Encoding)> ScanExeForCompanyCodes(
        string exePath, int minLen = 8,
        List<(int Offset, int ByteLength, string Encoding)>? spuriousOut = null)
    {
        var results = new List<(string, string, int, string)>();
        var seenEncoded = new HashSet<string>();
        var seenDecoded = new HashSet<string>();
        var matchedRanges = new List<(int Start, int End)>();

        byte[] data = File.ReadAllBytes(exePath);

        // Valid encoded chars: 0-9 (0x30-0x39), A-E (0x41-0x45), G (0x47)
        bool IsCodeByte(byte b) =>
            (b >= 0x30 && b <= 0x39) ||
            (b >= 0x41 && b <= 0x45) ||
            b == 0x47;

        bool IsSubRange(int start, int end)
        {
            foreach (var r in matchedRanges)
            {
                if (start >= r.Start && end <= r.End)
                    return true;
            }
            return false;
        }

        // Try to decode a candidate encoded string; return true and set decoded if valid
        bool TryDecode(string candidate, out string decoded)
        {
            decoded = DecodeCompanyName(candidate);
            if (!string.IsNullOrEmpty(decoded))
            {
                // Truncate at the first null character in the plaintext decoded result
                int nullIdx = decoded.IndexOf('\0');
                if (nullIdx >= 0)
                {
                    decoded = decoded.Substring(0, nullIdx);
                }
            }
            return !string.IsNullOrEmpty(decoded) && IsValidCompanyName(decoded);
        }

        // 1. Scan ASCII
        int i = 0;
        while (i < data.Length)
        {
            if (!IsCodeByte(data[i])) { i++; continue; }
            int start = i;
            while (i < data.Length && IsCodeByte(data[i])) i++;
            int len = i - start;
            if (len >= minLen)
            {
                bool foundInThisRun = false;
                for (int subLen = len; subLen >= minLen && !foundInThisRun; subLen--)
                {
                    if (subLen % 2 != 0) continue;
                    for (int offset = 0; offset <= len - subLen && !foundInThisRun; offset++)
                    {
                        int byteStart = start + offset;
                        int byteEnd = byteStart + subLen;

                        if (IsSubRange(byteStart, byteEnd)) continue;

                        string encoded = System.Text.Encoding.ASCII.GetString(data, byteStart, subLen);
                        if (TryDecode(encoded, out string decoded))
                        {
                            if (decoded.Trim().Length >= minLen / 2)
                            {
                                if (IsSubstringOfExisting(decoded, seenDecoded))
                                {
                                    spuriousOut?.Add((byteStart, subLen, "ASCII"));
                                    foundInThisRun = true;
                                    continue;
                                }

                                if (seenEncoded.Add(encoded))
                                {
                                    seenDecoded.Add(decoded);
                                    results.Add((encoded, decoded, byteStart, "ASCII"));
                                    matchedRanges.Add((byteStart, byteEnd));
                                    foundInThisRun = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        // 2. Scan UTF-16 (Unicode)
        i = 0;
        while (i < data.Length - 1)
        {
            if (IsCodeByte(data[i]) && data[i + 1] == 0x00)
            {
                int start = i;
                while (i < data.Length - 1 && IsCodeByte(data[i]) && data[i + 1] == 0x00)
                    i += 2;
                int byteLen = i - start;
                int charLen = byteLen / 2;
                if (charLen >= minLen)
                {
                    bool foundInThisRun = false;
                    for (int subLen = charLen; subLen >= minLen && !foundInThisRun; subLen--)
                    {
                        if (subLen % 2 != 0) continue;
                        for (int offset = 0; offset <= charLen - subLen && !foundInThisRun; offset++)
                        {
                            int byteStart = start + offset * 2;
                            int byteEnd = byteStart + subLen * 2;

                            if (IsSubRange(byteStart, byteEnd)) continue;

                            string encoded = System.Text.Encoding.Unicode.GetString(data, byteStart, subLen * 2);
                            if (TryDecode(encoded, out string decoded))
                            {
                                if (decoded.Trim().Length >= minLen / 2)
                                {
                                    if (IsSubstringOfExisting(decoded, seenDecoded))
                                    {
                                        spuriousOut?.Add((byteStart, subLen * 2, "UTF-16"));
                                        foundInThisRun = true;
                                        continue;
                                    }

                                    if (seenEncoded.Add(encoded))
                                    {
                                        seenDecoded.Add(decoded);
                                        results.Add((encoded, decoded, byteStart, "UTF-16"));
                                        matchedRanges.Add((byteStart, byteEnd));
                                        foundInThisRun = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else { i++; }
        }

        // Deduplication
        var finalResults = new List<(string, string, int, string)>();
        var seen = new HashSet<string>();
        foreach (var r in results)
            if (seen.Add(r.Item2)) finalResults.Add(r);

        // Mark embedded-default code as spurious when a custom one exists
        bool hasCustomName = finalResults.Any(r => !string.Equals(r.Item1, EmbeddedCompanyCode, StringComparison.OrdinalIgnoreCase));
        if (hasCustomName)
        {
            foreach (var r in results)
            {
                if (string.Equals(r.Item1, EmbeddedCompanyCode, StringComparison.OrdinalIgnoreCase))
                {
                    int bl = r.Item1.Length * (r.Item4 == "UTF-16" ? 2 : 1);
                    spuriousOut?.Add((r.Item3, bl, r.Item4));
                }
            }
            finalResults.RemoveAll(r => string.Equals(r.Item1, EmbeddedCompanyCode, StringComparison.OrdinalIgnoreCase));
        }

        return finalResults;
    }



    // ── Form Load ──────────────────────────────────────────────────────────
    private void Form1_Load(object sender, EventArgs e)
    {
        // Touch embedded company code to ensure it is retained in the compiled binary
        _ = EmbeddedCompanyCode.Length;

        // Nothing extra needed on load — auto-detect handles compile vs patch automatically.
    }

    // ════════════════════════════════════════════════════════════════════════
    //  EXE VERSION EDITOR — Event Handlers (unchanged)
    // ════════════════════════════════════════════════════════════════════════

    private async void BtnImportVer_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            Title = "Select EXE File to Edit Version Info"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            txtFilePathVer.Text = ofd.FileName;
            this.Text = $"CompanyName - {Path.GetFileNameWithoutExtension(ofd.FileName)}";
            await LoadVersionInfoAsync();
        }
    }

    private async void BtnLoadVer_Click(object sender, EventArgs e) => await LoadVersionInfoAsync();

    // ── Drag-and-Drop: EXE path box ─────────────────────────────────────────
    private void TxtFilePathVer_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private async void TxtFilePathVer_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data == null) return;
        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files == null || files.Length == 0) return;
        string cleaned = files[0].Trim().Trim('"', '\'');
        txtFilePathVer.Text = cleaned;
        this.Text = $"CompanyName - {Path.GetFileNameWithoutExtension(cleaned)}";
        await LoadVersionInfoAsync();
    }

    // ── Drag-and-Drop: Company Name / TextInput box ─────────────────────────
    private void TxtTextInput_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void TxtTextInput_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data == null) return;
        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files == null || files.Length == 0) return;
        txtTextInput.Text = files[0].Trim().Trim('"', '\'');
    }



    private async Task LoadVersionInfoAsync()
    {
        string path = txtFilePathVer.Text.Trim().Trim('"', '\'');
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            lblStatusVer.Text = "Please select a valid EXE file first.";
            return;
        }
        try
        {
            lblStatusVer.Text = "Loading version info...";
            // Heavy I/O on background thread
            var result = await Task.Run(() =>
            {
                var productName = ExeVersionEditor.GetVersionInfo(path, "ProductName", offset: 0);
                var fileDesc = ExeVersionEditor.GetVersionInfo(path, "FileDescription", offset: 0);
                var fileVersion = ExeVersionEditor.GetVersionInfo(path, "FileVersion", offset: 0);
                var productVersion = ExeVersionEditor.GetVersionInfo(path, "ProductVersion", offset: 2);
                var companyName = ExeVersionEditor.GetVersionInfo(path, "CompanyName", offset: 0);
                var comments = ExeVersionEditor.GetVersionInfo(path, "Comments", offset: 2);
                var titleBar = ExeVersionEditor.GetVersionInfo(path, "TitleBar", offset: 0);
                var fileName = Path.GetFileName(path);
                return (productName, fileDesc, fileVersion, productVersion, companyName, comments, titleBar, fileName);
            });

            // Update UI on UI thread
            this.Invoke(new Action(() =>
            {
                txtProductName.Text = result.productName;
                txtFileDescription.Text = result.fileDesc;
                // Update window title to match title bar after loading
                this.Text = $"CompanyName - {result.titleBar}";
                txtFileVersion.Text = result.fileVersion;
                txtProductVersion.Text = result.productVersion;
                txtCompanyName.Text = result.companyName;
                txtComments.Text = result.comments;
                txtTitleBar.Text = result.titleBar;
                txtPhysicalFilename.Text = result.fileName;
                lblStatusVer.Text = "Version info loaded successfully.";
            }));
        }
        catch (Exception ex)
        {
            lblStatusVer.Text = $"Error loading version info: {ex.Message}";
        }
    }

    private void UpdateField(string entryName, string newValue, int offset, int ploc = 0)
    {
        string path = txtFilePathVer.Text.Trim().Trim('"', '\'');
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            lblStatusVer.Text = "Please select a valid EXE file first.";
            return;
        }
        try
        {
            lblStatusVer.Text = $"Updating {entryName}...";
            Application.DoEvents();
            ExeVersionEditor.GetVersionInfo(path, entryName, unicode: true,
                update: true, updateName: newValue, offset: offset, ploc: ploc);
            lblStatusVer.Text = $"Success: '{entryName}' updated to '{newValue}'.";
            _ = LoadVersionInfoAsync(); // async load to avoid blocking UI
        }
        catch (Exception ex)
        {
            lblStatusVer.Text = $"Error updating {entryName}: {ex.Message}";
        }
    }

    private void BtnSaveProductName_Click(object sender, EventArgs e) => UpdateField("ProductName", txtProductName.Text, 0);

    private void BtnSaveFileDescription_Click(object sender, EventArgs e)
    {
        UpdateField("FileDescription", txtFileDescription.Text, 0);
        this.Text = $"CompanyName - {txtFileDescription.Text}";
    }

    private void BtnSaveFileVersion_Click(object sender, EventArgs e) => UpdateField("FileVersion", txtFileVersion.Text, 0);
    private void BtnSaveProductVersion_Click(object sender, EventArgs e) => UpdateField("ProductVersion", txtProductVersion.Text, 2);
    private void BtnSaveCompanyName_Click(object sender, EventArgs e) => UpdateField("CompanyName", txtCompanyName.Text, 0);
    private void BtnSaveComments_Click(object sender, EventArgs e) => UpdateField("Comments", txtComments.Text, 2);
    private void BtnSaveTitleBar_Click(object sender, EventArgs e)
    {
        UpdateField("TitleBar", txtTitleBar.Text, 0);
        // After updating the title bar resource, rename the EXE file to match the new title bar text
        if (!string.IsNullOrWhiteSpace(txtTitleBar.Text) && !string.IsNullOrWhiteSpace(txtFilePathVer.Text))
        {
            string? dir = Path.GetDirectoryName(txtFilePathVer.Text);
            if (string.IsNullOrEmpty(dir))
            {
                lblStatusVer.Text = "Could not determine target folder path.";
                return;
            }
            // Sanitize filename: remove invalid path chars
            string sanitized = string.Concat(txtTitleBar.Text.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                lblStatusVer.Text = "Invalid title for filename.";
                return;
            }
            string newFileName = sanitized.Trim();
            if (!newFileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                newFileName += ".exe";
            string newPath = Path.Combine(dir, newFileName);
            try
            {
                // Copy (overwrite) then delete original
                File.Copy(txtFilePathVer.Text, newPath, true);
                File.Delete(txtFilePathVer.Text);
                txtFilePathVer.Text = newPath;
                this.Text = $"CompanyName - {Path.GetFileNameWithoutExtension(newFileName)}";
                lblStatusVer.Text = $"File renamed to '{newFileName}'.";
            }
            catch (Exception ex)
            {
                lblStatusVer.Text = $"Rename failed: {ex.Message}";
            }
        }
    }

    private void BtnRenamePhysical_Click(object sender, EventArgs e)
    {
        string path = txtFilePathVer.Text.Trim().Trim('"', '\'');
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            lblStatusVer.Text = "Please select a valid EXE file first.";
            return;
        }
        string newName = txtPhysicalFilename.Text.Trim();
        if (string.IsNullOrWhiteSpace(newName)) { lblStatusVer.Text = "Filename cannot be empty."; return; }
        if (string.Equals(Path.GetFileName(path), newName, StringComparison.OrdinalIgnoreCase))
        {
            lblStatusVer.Text = "Filename is already the same.";
            return;
        }
        try
        {
            lblStatusVer.Text = "Renaming file on disk...";
            Application.DoEvents();
            if (ExeVersionEditor.RenamePhysicalFile(path, newName, out string newPath, out string error))
            {
                txtFilePathVer.Text = newPath;
                this.Text = $"CompanyName - {Path.GetFileNameWithoutExtension(newName)}";
                lblStatusVer.Text = $"File renamed successfully to '{newName}'.";
                _ = LoadVersionInfoAsync(); // async load after rename
            }
            else
            {
                lblStatusVer.Text = $"Rename failed: {error}";
            }
        }
        catch (Exception ex)
        {
            lblStatusVer.Text = $"Error renaming: {ex.Message}";
        }
    }

    // ── Title Bar Changer Event Handlers & Helpers ───────────────────────────
    private void InitTitleChangerDragDrop()
    {
        txtExePathChanger.DragEnter += TxtExePathChanger_DragEnter;
        txtExePathChanger.DragDrop += TxtExePathChanger_DragDrop;
    }

    private void TxtExePathChanger_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void TxtExePathChanger_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data == null) return;
        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files == null || files.Length == 0) return;
        string cleaned = files[0].Trim().Trim('"', '\'');
        txtExePathChanger.Text = cleaned;
        txtCompanyCodeTitle.Text = string.Empty;
        txtCompanyNameTitle.Text = string.Empty;
        txtNewTitle.Text = string.Empty;
        ReadTitleFromExe();
    }

    private void BtnBrowseChanger_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            Title = "Select EXE File for Title Bar Changing"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            txtExePathChanger.Text = ofd.FileName;
            txtCompanyCodeTitle.Text = string.Empty;
            txtCompanyNameTitle.Text = string.Empty;
            txtNewTitle.Text = string.Empty;
            ReadTitleFromExe();
        }
    }


    private void ReadTitleFromExe()
    {
        string path = txtExePathChanger.Text.Trim().Trim('"', '\'');
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            lblStatusTitle.ForeColor = System.Drawing.Color.Red;
            lblStatusTitle.Text = "Please select a valid EXE file first.";
            return;
        }

        lblStatusTitle.ForeColor = System.Drawing.Color.DimGray;
        lblStatusTitle.Text = "Scanning company codes and titles...";
        txtCompanyCodeTitle.Text = "Scanning...";
        txtCompanyNameTitle.Text = "Scanning...";
        Application.DoEvents();

        Task.Run(() =>
        {
            string title = "";
            string error = "";

            // ── Read Title Bar from Static Resources instead of Process.Start ──
            try
            {
                var detected = ExeTitleEditor.DetectTitles(path);
                foreach (var item in detected)
                {
                    if (item.Source.StartsWith("VersionInfo"))
                    {
                        title = item.Title;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(title) && detected.Count > 0)
                {
                    title = detected[0].Title;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            // ── Scan Company Codes ──
            var encodedCodes = new List<string>();
            var decodedNames = new List<string>();
            var offsetInfos = new List<string>();
            try
            {
                var spurious = new List<(int Offset, int ByteLength, string Encoding)>();
                var hits = ScanExeForCompanyCodes(path, spuriousOut: spurious);
                this.Invoke(() => { _scannedSpuriousRanges.Clear(); _scannedSpuriousRanges.AddRange(spurious); });
                foreach (var hit in hits)
                {
                    encodedCodes.Add(hit.Encoded);
                    decodedNames.Add(hit.Decoded);
                    offsetInfos.Add($"0x{hit.Offset:X8} ({hit.Encoding})");
                }
            }
            catch { }

            this.Invoke(() =>
            {
                _scannedDecodedCompanyNames.Clear();
                _scannedDecodedCompanyNames.AddRange(decodedNames);

                // Update Encoded Company Code text box
                if (encodedCodes.Count > 0)
                {
                    txtCompanyCodeTitle.Text = string.Join(Environment.NewLine, encodedCodes);
                }
                else
                {
                    txtCompanyCodeTitle.Text = "(None found)";
                }

                // Update Decoded Company Name text box
                if (decodedNames.Count > 0)
                {
                    txtCompanyNameTitle.Text = string.Join(Environment.NewLine, decodedNames);
                }
                else
                {
                    txtCompanyNameTitle.Text = "(None found)";
                }

                // Update dynamically adjust labels/status
                if (decodedNames.Count > 0)
                {
                    lblNewTitle.Text = "New Company Name:";
                    btnRenameTitle.Text = "Rename Company Code";

                    lblStatusTitle.ForeColor = System.Drawing.Color.Green;
                    lblStatusTitle.Text = $"✓ Company code detected: \"{decodedNames[0]}\". Enter a new company name and click Rename.";
                }
                else
                {
                    lblNewTitle.Text = "New Title:";
                    btnRenameTitle.Text = "Rename Title";

                    if (!string.IsNullOrEmpty(error))
                    {
                        lblStatusTitle.ForeColor = System.Drawing.Color.Red;
                        lblStatusTitle.Text = $"Error reading title: {error}";
                    }
                    else if (string.IsNullOrEmpty(title))
                    {
                        lblStatusTitle.ForeColor = System.Drawing.Color.DarkOrange;
                        lblStatusTitle.Text = "Could not read title. Type the title manually in the box above.";
                    }
                    else
                    {
                        lblStatusTitle.ForeColor = System.Drawing.Color.Green;
                        lblStatusTitle.Text = $"✓ Title bar text: \"{title}\". Now enter a new title and click Rename Title.";
                    }
                }
            });
        });
    }


    private async void BtnRenameTitle_Click(object? sender, EventArgs e)
    {
        string path = txtExePathChanger.Text.Trim().Trim('"', '\'');
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            lblStatusTitle.ForeColor = System.Drawing.Color.Red;
            lblStatusTitle.Text = "Please select a valid EXE file first.";
            return;
        }

        // Retrieve oldTitle: prioritize decoded company name, then default placeholder (for first-time init), then detected window title
        string oldTitle = "";
        bool isCompanyCodeRename = false;
        int placeholderMatchIndex = -1;
        bool placeholderIsUnicode = false;

        if (_scannedDecodedCompanyNames.Count > 0)
        {
            oldTitle = _scannedDecodedCompanyNames[0];
            isCompanyCodeRename = true;
        }
        else
        {
            // If no custom code is detected in this EXE, see if the default placeholder is present.
            // This allows writing the first company code to a clean compiled EXE.
            string defaultDecoded = DecodeCompanyName(EmbeddedCompanyCode);
            int nullIdx = defaultDecoded.IndexOf('\0');
            if (nullIdx >= 0) defaultDecoded = defaultDecoded.Substring(0, nullIdx);

            // We look inside the file to verify if the placeholder's encoded bytes are present.
            // EmbeddedCompanyCode starts with "A8G818D85808A8G818D85808" (corresponds to "domaindomain")
            try
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                string prefixString = "A8G818D85808A8G818D85808";
                byte[] prefixAscii = Encoding.ASCII.GetBytes(prefixString);
                byte[] prefixUnicode = Encoding.Unicode.GetBytes(prefixString);
                
                int matchIndex = -1;
                bool isUnicodeMatch = false;

                // 1. Check ASCII
                for (int i = 0; i <= fileBytes.Length - prefixAscii.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < prefixAscii.Length; j++)
                    {
                        if (fileBytes[i + j] != prefixAscii[j]) { match = false; break; }
                    }
                    if (match) { matchIndex = i; isUnicodeMatch = false; break; }
                }

                // 2. Check Unicode if ASCII not found
                if (matchIndex == -1)
                {
                    for (int i = 0; i <= fileBytes.Length - prefixUnicode.Length; i++)
                    {
                        bool match = true;
                        for (int j = 0; j < prefixUnicode.Length; j++)
                        {
                            if (fileBytes[i + j] != prefixUnicode[j]) { match = false; break; }
                        }
                        if (match) { matchIndex = i; isUnicodeMatch = true; break; }
                    }
                }

                if (matchIndex != -1)
                {
                    // Found the placeholder! We set oldTitle to "domaindomain" (corresponds to the matched prefix)
                    // so that PatchBinaryString searches and replaces this exact prefix.
                    oldTitle = "domaindomain";
                    isCompanyCodeRename = true;
                    placeholderMatchIndex = matchIndex;
                    placeholderIsUnicode = isUnicodeMatch;
                }
            }
            catch { }

            if (!isCompanyCodeRename)
            {
                oldTitle = ExeTitleEditor.DetectCurrentTitle(path);
            }
        }


        string newTitle = txtNewTitle.Text.Trim();

        if (string.IsNullOrEmpty(newTitle))
        {
            lblStatusTitle.ForeColor = System.Drawing.Color.Red;
            lblStatusTitle.Text = "New value cannot be empty. Please enter a new value.";
            return;
        }

        if (newTitle.Length < 2)
        {
            MessageBox.Show("The new title must be at least 2 characters long.",
                "Input Too Short", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            lblStatusTitle.ForeColor = System.Drawing.Color.Red;
            lblStatusTitle.Text = "Rename cancelled: name must be at least 2 characters.";
            return;
        }

        if (string.IsNullOrEmpty(oldTitle))
        {
            oldTitle = ShowInputDialog(
                "System could not auto-detect the current company name or title bar text.\n" +
                "Please type in the current name/code of the EXE to search and replace:",
                "Manual Title Input", "");
        }

        if (string.IsNullOrEmpty(oldTitle) || oldTitle.Length < 2)
        {
            lblStatusTitle.ForeColor = System.Drawing.Color.Red;
            lblStatusTitle.Text = "Rename cancelled: original name to search must be at least 2 characters.";
            return;
        }

        // ── Guard: detect if user accidentally typed an encoded code instead of a plain name ──
        if (isCompanyCodeRename)
        {
            const string encAlphabet = "0123456789ABCDEG";
            bool looksEncoded = newTitle.Length % 2 == 0
                && newTitle.All(c => encAlphabet.Contains(c));

            if (looksEncoded)
            {
                string preview = DecodeCompanyName(newTitle);
                string previewMsg = string.IsNullOrEmpty(preview)
                    ? "(could not decode)"
                    : $"\"{preview}\"";

                MessageBox.Show(
                    "❌  Invalid Input Detected!\n\n" +
                    "You entered an encoded company code:\n" +
                    $"\"{newTitle}\"\n\n" +
                    "Please enter a plain, readable company name (e.g. \"MACRO SPEED STATION SDN BHD\") instead.\n" +
                    "The system will encrypt it into a code automatically.",
                    "Input Error — Encoded Code Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatusTitle.ForeColor = System.Drawing.Color.Red;
                lblStatusTitle.Text = "Rename cancelled: please enter a plain company name.";
                return;
            }
        }


        try
        {
            lblStatusTitle.ForeColor = System.Drawing.Color.DimGray;
            lblStatusTitle.Text = "Renaming via Win32 patch, please wait...";
            Application.DoEvents();


            bool anySuccess = false;
            var statusParts = new List<string>();

            string dllPath = Path.ChangeExtension(path, ".dll");
            bool siblingDllExists = File.Exists(dllPath);

            // 1. Update resources (FileDescription, ProductName, CompanyName)
            try
            {
                var updates = new System.Collections.Generic.Dictionary<string, string>
                {
                    ["CompanyName"] = newTitle,
                    ["ProductName"] = newTitle,
                    ["FileDescription"] = newTitle,
                };
                // We use GetVersionInfo with update:true to update these fields
                ExeVersionEditor.GetVersionInfo(path, "CompanyName", unicode: true, update: true, updateName: newTitle);
                ExeVersionEditor.GetVersionInfo(path, "ProductName", unicode: true, update: true, updateName: newTitle);
                ExeVersionEditor.GetVersionInfo(path, "FileDescription", unicode: true, update: true, updateName: newTitle);
                anySuccess = true;
                statusParts.Add("✓ version resource updated");

                if (siblingDllExists)
                {
                    try
                    {
                        ExeVersionEditor.GetVersionInfo(dllPath, "CompanyName", unicode: true, update: true, updateName: newTitle);
                        ExeVersionEditor.GetVersionInfo(dllPath, "ProductName", unicode: true, update: true, updateName: newTitle);
                        ExeVersionEditor.GetVersionInfo(dllPath, "FileDescription", unicode: true, update: true, updateName: newTitle);
                        statusParts.Add("✓ DLL version resource updated");
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                statusParts.Add($"Version resource: {ex.Message}");
            }

            if (isCompanyCodeRename)
            {
                string oldEncoded = EncodeCompanyName(oldTitle);
                string newEncoded = EncodeCompanyName(newTitle);

                // Binary patch the company code in the EXE
                string patchMsg = "";
                bool ok = false;

                if (placeholderMatchIndex != -1)
                {
                    try
                    {
                        // Clean target starts with 2000-char placeholder. Pad new encoded string with null bytes up to 2000 characters.
                        string paddedEncoded = newEncoded.PadRight(2000, '\0');
                        byte[] fileBytes = File.ReadAllBytes(path);
                        byte[] newBytes = placeholderIsUnicode
                            ? Encoding.Unicode.GetBytes(paddedEncoded)
                            : Encoding.ASCII.GetBytes(paddedEncoded);

                        if (placeholderMatchIndex + newBytes.Length <= fileBytes.Length)
                        {
                            Array.Copy(newBytes, 0, fileBytes, placeholderMatchIndex, newBytes.Length);
                            File.WriteAllBytes(path, fileBytes);
                            ok = true;
                            patchMsg = "Initial company code written";
                        }
                        else
                        {
                            ok = false;
                            patchMsg = "Index out of range during first write";
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                        patchMsg = $"Init write error: {ex.Message}";
                    }
                }
                else
                {
                    string paddedEncoded = newEncoded;
                    if (newEncoded.Length < oldEncoded.Length)
                        paddedEncoded = newEncoded + new string('\0', oldEncoded.Length - newEncoded.Length);
                    ok = ExeTitleEditor.PatchBinaryString(path, oldEncoded, paddedEncoded, out patchMsg);
                }

                statusParts.Add(ok ? "✓ Company code patched" : $"Binary code: {patchMsg}");
                if (ok)
                {
                    anySuccess = true;
                    // Zero out only the spurious ghost ranges from the last scan
                    // (e.g. the default "hello" slot in an unmodified copy of this tool).
                    // Do NOT do any extra scanning here — we could accidentally erase what we just wrote.
                    ZeroSpuriousRanges(path, _scannedSpuriousRanges);
                }

                if (siblingDllExists)
                {
                    string dllMsg = "";
                    bool dllOk = false;
                    if (placeholderMatchIndex != -1)
                    {
                        // Sibling DLL clean patch
                        try
                        {
                            // Clean target starts with 2000-char placeholder.
                            // We need to scan sibling DLL for placeholder match since it might be at a different offset.
                            byte[] dllBytes = File.ReadAllBytes(dllPath);
                            string prefixString = "A8G818D85808A8G818D85808";
                            byte[] prefixAscii = Encoding.ASCII.GetBytes(prefixString);
                            byte[] prefixUnicode = Encoding.Unicode.GetBytes(prefixString);
                            int dllMatchIdx = -1;
                            bool dllMatchIsUnicode = false;

                            for (int i = 0; i <= dllBytes.Length - prefixAscii.Length; i++)
                            {
                                bool match = true;
                                for (int j = 0; j < prefixAscii.Length; j++)
                                {
                                    if (dllBytes[i + j] != prefixAscii[j]) { match = false; break; }
                                }
                                if (match) { dllMatchIdx = i; dllMatchIsUnicode = false; break; }
                            }

                            if (dllMatchIdx == -1)
                            {
                                for (int i = 0; i <= dllBytes.Length - prefixUnicode.Length; i++)
                                {
                                    bool match = true;
                                    for (int j = 0; j < prefixUnicode.Length; j++)
                                    {
                                        if (dllBytes[i + j] != prefixUnicode[j]) { match = false; break; }
                                    }
                                    if (match) { dllMatchIdx = i; dllMatchIsUnicode = true; break; }
                                }
                            }

                            if (dllMatchIdx != -1)
                            {
                                string paddedEncoded = newEncoded.PadRight(2000, '\0');
                                byte[] newBytes = dllMatchIsUnicode
                                    ? Encoding.Unicode.GetBytes(paddedEncoded)
                                    : Encoding.ASCII.GetBytes(paddedEncoded);
                                if (dllMatchIdx + newBytes.Length <= dllBytes.Length)
                                {
                                    Array.Copy(newBytes, 0, dllBytes, dllMatchIdx, newBytes.Length);
                                    File.WriteAllBytes(dllPath, dllBytes);
                                    dllOk = true;
                                    dllMsg = "Initial company code written";
                                }
                            }
                        }
                        catch (Exception ex) { dllOk = false; dllMsg = ex.Message; }
                    }
                    else
                    {
                        string paddedEncoded = newEncoded;
                        if (newEncoded.Length < oldEncoded.Length)
                            paddedEncoded = newEncoded + new string('\0', oldEncoded.Length - newEncoded.Length);
                        dllOk = ExeTitleEditor.PatchBinaryString(dllPath, oldEncoded, paddedEncoded, out dllMsg);
                    }
                    if (dllOk) { anySuccess = true; statusParts.Add("✓ DLL code patched"); }
                }

                // Companion plain text patches
                string companionStatus;
                bool companionOk = ExeTitleEditor.ChangeTitle(path, oldTitle, newTitle, out companionStatus);
                if (companionOk) { anySuccess = true; statusParts.Add("✓ Plain text patched"); }

                if (siblingDllExists)
                {
                    string dllCompanionStatus;
                    bool dllCompanionOk = ExeTitleEditor.ChangeTitle(dllPath, oldTitle, newTitle, out dllCompanionStatus);
                    if (dllCompanionOk) { anySuccess = true; statusParts.Add("✓ DLL plain text patched"); }
                }
            }
            else
            {
                // Normal title bar rename only
                string binaryStatus;
                bool ok = ExeTitleEditor.ChangeTitle(path, oldTitle, newTitle, out binaryStatus);
                if (ok) { anySuccess = true; statusParts.Add("✓ Binary strings patched"); }
                else statusParts.Add($"Binary: {binaryStatus}");

                if (siblingDllExists)
                {
                    string dllStatus;
                    bool dllOk = ExeTitleEditor.ChangeTitle(dllPath, oldTitle, newTitle, out dllStatus);
                    if (dllOk) { anySuccess = true; statusParts.Add("✓ DLL patched"); }
                }
            }

            string statusText = string.Join(" | ", statusParts);

            if (anySuccess)
            {
                lblStatusTitle.ForeColor = System.Drawing.Color.Green;
                lblStatusTitle.Text = $"✓ Done! {statusText}";
                MessageBox.Show(
                    $"Renamed successfully!\n\n" +
                    "⚠️  IMPORTANT: You must close and reopen the target EXE for the change to take effect.\n\n" +
                    $"{statusText}" +
                    (siblingDllExists ? $"\nDLL also patched: {Path.GetFileName(dllPath)}" : ""),
                    "Rename Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ReadTitleFromExe();
            }
            else
            {
                lblStatusTitle.ForeColor = System.Drawing.Color.Red;
                lblStatusTitle.Text = $"✗ Failed: {statusText}";
                MessageBox.Show(
                    $"Could not rename.\n\nDetails: {statusText}\n\n" +
                    "Tips:\n" +
                    "• Close the target EXE before renaming.\n" +
                    "• Do not attempt to rename the currently running ExeEncryptor executable directly (copy it first).\n" +
                    "• Run as Administrator if needed.",
                    "Rename Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            lblStatusTitle.ForeColor = System.Drawing.Color.Red;
            lblStatusTitle.Text = $"Error: {ex.Message}";
        }
    }


    /// <summary>
    /// Overwrites spurious / ghost company code byte ranges with zeros in the binary file.
    /// Called after a successful rename so that false-positive codes never reappear on future scans.
    /// </summary>
    private static void ZeroSpuriousRanges(string filePath, List<(int Offset, int ByteLength, string Encoding)> ranges)
    {
        if (ranges.Count == 0) return;
        try
        {
            byte[] data = File.ReadAllBytes(filePath);
            bool changed = false;
            foreach (var (offset, byteLength, _) in ranges)
            {
                if (offset < 0 || offset + byteLength > data.Length) continue;
                for (int k = offset; k < offset + byteLength; k++)
                    data[k] = 0x00;
                changed = true;
            }
            if (changed)
                File.WriteAllBytes(filePath, data);
        }
        catch { /* best-effort: do not fail the rename if cleanup fails */ }
    }

    /// <summary>
    /// Displays a simple dialog prompting the user to type in a text value.
    /// </summary>
    private static string ShowInputDialog(string text, string caption, string defaultValue = "")
    {
        Form prompt = new Form()
        {
            Width = 500,
            Height = 150,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };
        Label textLabel = new Label() { Left = 20, Top = 20, Width = 460, Text = text };
        TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, Text = defaultValue };
        Button confirmation = new Button() { Text = "Ok", Left = 380, Width = 80, Top = 48, DialogResult = DialogResult.OK };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(textLabel);
        prompt.AcceptButton = confirmation;

        return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
    }
}
