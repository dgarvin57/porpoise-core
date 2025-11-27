// Porpoise.Core/Services/CoverPlatinumDecrypt.cs — FINAL, EXACT, WORKING
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Porpoise.Core.Services;

public static class CoverPlatinumDecrypt
{
    // PROFILE KEY = Brown + Orange + Green + Violet (32 bytes)
    private static readonly byte[] ProfileKey = new byte[]
    {
        0x13, 0xBF, 0x42, 0xFE, // Brown
        0xA1, 0xF2, 0xE7, 0xD6, // Orange
        0xE1, 0xD6, 0x41, 0x77, // Green
        0xAF, 0x82, 0x9D, 0xFB, // Violet
        0x13, 0xBF, 0x42, 0xFE, // repeat
        0xA1, 0xF2, 0xE7, 0xD6,
        0xE1, 0xD6, 0x41, 0x77,
        0xAF, 0x82, 0x9D, 0xFB
    };

    // RESPONSE IV = Black + Red + Yellow + Blue (16 bytes)
    private static readonly byte[] ResponseIV = new byte[]
    {
        0xBA, 0x3F, 0x23, 0x8E, // Black
        0x0F, 0x1E, 0x92, 0xFD, // Red
        0xEA, 0xFB, 0x7A, 0x50, // Yellow
        0x49, 0x3B, 0x4C, 0xF2  // Blue
    };

    public static string DecryptFile(string path)
    {
        var allBytes = File.ReadAllBytes(path);

        // Skip the 29-byte header for binary files
        const int HeaderLength = 29;

        if (allBytes.Length <= HeaderLength)
            throw new InvalidOperationException("File too small");

        var encryptedData = new byte[allBytes.Length - HeaderLength];
        Array.Copy(allBytes, HeaderLength, encryptedData, 0, encryptedData.Length);

#pragma warning disable SYSLIB0022 // Type or member is obsolete
        using var rijndael = new RijndaelManaged
        {
            Key = ProfileKey,
            IV = ResponseIV,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        };
#pragma warning restore SYSLIB0022 // Type or member is obsolete

        Console.WriteLine("[1] ProfileKey: " + ProfileKey);
        Console.WriteLine("[2] ResponseIV: " + ResponseIV);

        using var decryptor = rijndael.CreateDecryptor();
        using var memoryStream = new MemoryStream(encryptedData);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

        var plainText = new byte[encryptedData.Length];
        var decryptedCount = cryptoStream.Read(plainText, 0, plainText.Length);

        Console.WriteLine("decryptedCount: " +decryptedCount);
        Console.WriteLine("decryptedText: " + (decryptedCount > 300 ? Encoding.Unicode.GetString(plainText, 0, 300) : Encoding.Unicode.GetString(plainText, 0, decryptedCount)));

        return Encoding.Unicode.GetString(plainText, 0, decryptedCount);
    }
}