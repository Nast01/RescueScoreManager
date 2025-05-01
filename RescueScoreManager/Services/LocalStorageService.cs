using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using RescueScoreManager.Data;
using System.Text.Json;

namespace RescueScoreManager.Services;

public class LocalStorageService : IStorageService
{
    private readonly string? _filePath;

    public LocalStorageService()
    {
        // Store in AppData of the user
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FFSSAuthApp");

        // Create Folder if not existing
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _filePath = Path.Combine(appDataPath, "auth.json");
    }

    public AuthenticationInfo LoadAuthenticationInfo()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string jsonString = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AuthenticationInfo>(jsonString);
            }
        }
        catch (Exception ex)
        {
            // Gérer les erreurs de lecture
            Console.WriteLine($"Erreur lors du chargement des données d'authentification: {ex.Message}");
        }

        return new AuthenticationInfo();
    }

    void IStorageService.SaveAuthenticationInfo(AuthenticationInfo authInfo)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(authInfo, jsonOptions);
            File.WriteAllText(_filePath, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("SaveTokenError")}: {ex.Message}");
        }
    }



}
