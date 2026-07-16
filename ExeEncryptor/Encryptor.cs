using System;
using System.Collections.Generic;
using System.Text;

public static class Encryptor
{
    private static readonly string ArrayStr = "0123456789abcdef";
    private static readonly string MixStr = "FEDCBA9876543210G";
    private static readonly string Mix2Str = "EDCBA9876543210G";

    // Helper: MixTable maps hex digit to custom mixed character
    private static byte MixTable(byte ch)
    {
        int index = ArrayStr.IndexOf((char)ch);
        if (index >= 0)
        {
            return (byte)MixStr[index + 1];
        }
        return ch;
    }

    // Helper: MixTable2 maps custom mixed character back to index (0-15)
    private static int MixTable2(byte ch)
    {
        return Mix2Str.IndexOf((char)ch);
    }

    // Validate if the string is valid company-specific hex representation
    public static bool IsUrlHex(string str)
    {
        if (string.IsNullOrEmpty(str)) return false;
        foreach (char c in str)
        {
            if (MixTable2((byte)c) < 0) return false;
        }
        return true;
    }

    // Encrypt: map bytes to company's custom hex string (each byte becomes 2 bytes)
    public static byte[] Encrypt(byte[] data)
    {
        if (data == null || data.Length == 0) return Array.Empty<byte>();

        byte[] result = new byte[data.Length * 2];
        int cx = 0;

        for (int bx = 0; bx < data.Length; ++bx)
        {
            byte aa = data[bx];

            // Low nibble
            byte lo = (byte)(aa & 0x0F);
            byte dLo = (byte)(lo > 9 ? lo + 0x37 + 0x20 : lo + 0x30);
            result[cx++] = MixTable(dLo);

            // High nibble
            byte hi = (byte)(aa >> 4);
            byte dHi = (byte)(hi > 9 ? hi + 0x37 + 0x20 : hi + 0x30);
            result[cx++] = MixTable(dHi);
        }

        return result;
    }

    // Decrypt: map company's custom hex string back to original bytes (halves the size)
    public static byte[] Decrypt(byte[] data)
    {
        if (data == null || data.Length == 0) return Array.Empty<byte>();

        int len = data.Length / 2;
        byte[] result = new byte[len];
        int cx = 0;

        for (int bx = 0; bx < data.Length - 1; bx += 2)
        {
            byte a0 = data[bx];
            byte a1 = data[bx + 1];

            int lo = MixTable2(a0);
            int hi = MixTable2(a1);

            if (lo < 0 || hi < 0)
            {
                result[cx++] = 0; // Fallback for corrupted characters
                continue;
            }

            result[cx++] = (byte)(hi * 16 + lo);
        }

        return result;
    }

    // EncryptText: Convert normal text string to company's custom hex representation
    public static string EncryptText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        byte[] data = Encoding.UTF8.GetBytes(text);
        byte[] encrypted = Encrypt(data);
        return Encoding.ASCII.GetString(encrypted);
    }

    // DecryptText: Convert company's custom hex representation back to normal text
    public static string DecryptText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Remove spaces or formatting if any was introduced
        text = text.Replace(" ", "").Replace("\r", "").Replace("\n", "");

        if (!IsUrlHex(text))
        {
            throw new ArgumentException("The input string contains invalid characters for decryption.");
        }

        byte[] data = Encoding.ASCII.GetBytes(text);
        byte[] decrypted = Decrypt(data);
        return Encoding.UTF8.GetString(decrypted).TrimEnd('\0');
    }
}
