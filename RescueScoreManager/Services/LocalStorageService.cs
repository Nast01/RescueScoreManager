using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using System.Security.Cryptography;
using System.Text;

namespace RescueScoreManager.Services;

public class LocalStorageService : IStorageService
{
    private readonly string? _filePath;
    private readonly ILogger<LocalStorageService> _logger;
    private readonly string _encryptionKey;

    public LocalStorageService(ILogger<LocalStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FFSSAuthApp");

        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _filePath = Path.Combine(appDataPath, "auth.json");
        _encryptionKey = GenerateOrGetEncryptionKey(appDataPath);
    }

    public AuthenticationInfo LoadAuthenticationInfo()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _logger.LogInformation("Authentication file not found, returning empty auth info");
                return new AuthenticationInfo();
            }

            string encryptedContent = File.ReadAllText(_filePath);
            string decryptedContent = DecryptString(encryptedContent);
            var authInfo = JsonSerializer.Deserialize<AuthenticationInfo>(decryptedContent);

            _logger.LogInformation("Authentication info loaded successfully");
            return authInfo ?? new AuthenticationInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading authentication info from {FilePath}", _filePath);
            return new AuthenticationInfo();
        }
    }

    public void SaveAuthenticationInfo(AuthenticationInfo authInfo)
    {
        if (authInfo == null)
        {
            _logger.LogWarning("Attempted to save null authentication info");
            return;
        }

        try
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(authInfo, jsonOptions);
            string encryptedContent = EncryptString(jsonString);

            File.WriteAllText(_filePath, encryptedContent);
            _logger.LogInformation("Authentication info saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving authentication info to {FilePath}", _filePath);
            throw new InvalidOperationException("Failed to save authentication information", ex);
        }
    }

    private string GenerateOrGetEncryptionKey(string appDataPath)
    {
        string keyPath = Path.Combine(appDataPath, "key.dat");

        if (File.Exists(keyPath))
        {
            return File.ReadAllText(keyPath);
        }

        string key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        File.WriteAllText(keyPath, key);
        return key;
    }

    private string EncryptString(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_encryptionKey);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);

        swEncrypt.Write(plainText);
        swEncrypt.Close();

        var encrypted = msEncrypt.ToArray();
        var result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }

    private string DecryptString(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(_encryptionKey);

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

}
