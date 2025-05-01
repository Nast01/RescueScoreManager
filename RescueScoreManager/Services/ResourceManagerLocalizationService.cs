using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Services;

/// <summary>
/// Implementation of localization service using resource files
/// </summary>
public class ResourceManagerLocalizationService : ILocalizationService
{
    private static ResourceManagerLocalizationService _instance;
    private ResourceManager _resourceManager;
    private LanguageModel _currentLanguage;

    public ObservableCollection<LanguageModel> AvailableLanguages { get; private set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public LanguageModel CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage?.CultureCode != value?.CultureCode)
            {
                _currentLanguage = value;

                // Update thread culture
                Thread.CurrentThread.CurrentUICulture = value.Culture;

                // Clear resource cache
                ResourceManagerClearCache();

                // Notify UI that all translations should be updated
                OnPropertyChanged(string.Empty);

                // Save selected language to app settings
                SaveSelectedLanguage(value.CultureCode);
            }
        }
    }

    public ResourceManagerLocalizationService()
    {
        // Initialize resource manager with default resource file
        _resourceManager = new ResourceManager("RescueScoreManager.Properties.Resources", typeof(App).Assembly);

        // Initialize available languages
        InitializeAvailableLanguages();

        // Load saved language or use system default
        LoadSavedLanguage();
    }

    public static ResourceManagerLocalizationService Instance =>
        _instance ??= new ResourceManagerLocalizationService();

    private void InitializeAvailableLanguages()
    {
        // Create list of supported languages
        AvailableLanguages = new ObservableCollection<LanguageModel>
            {
                new LanguageModel { DisplayName = "Français", CultureCode = "fr-FR" },
                new LanguageModel { DisplayName = "English", CultureCode = "en-US" },
                // Add more languages as needed
            };
    }

    private void LoadSavedLanguage()
    {
        try
        {
            // Try to load from settings (could use any persistence mechanism)
            string savedCulture = Properties.Settings.Default.PreferredLanguage;

            if (string.IsNullOrEmpty(savedCulture))
            {
                // Use system language if no saved preference
                _currentLanguage = FindLanguageModel(CultureInfo.CurrentUICulture.Name)
                    ?? AvailableLanguages[0]; // Default to first language if not found
            }
            else
            {
                _currentLanguage = FindLanguageModel(savedCulture)
                    ?? AvailableLanguages[0]; // Default to first language if not found
            }

            // Set thread culture to match
            Thread.CurrentThread.CurrentUICulture = _currentLanguage.Culture;
        }
        catch
        {
            // Fallback to first language on any error
            _currentLanguage = AvailableLanguages[0];
            Thread.CurrentThread.CurrentUICulture = _currentLanguage.Culture;
        }
    }

    private LanguageModel FindLanguageModel(string cultureCode)
    {
        return AvailableLanguages.FirstOrDefault(l =>
            string.Equals(l.CultureCode, cultureCode, comparisonType: StringComparison.OrdinalIgnoreCase));
    }

    private void SaveSelectedLanguage(string cultureCode)
    {
        try
        {
            Properties.Settings.Default.PreferredLanguage = cultureCode;
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Log error but continue - saving preference is not critical
        }
    }

    public string GetString(string key)
    {
        try
        {
            return _resourceManager.GetString(key, _currentLanguage.Culture) ?? $"[{key}]";
        }
        catch
        {
            return $"[{key}]";
        }
    }

    public string GetString(string key, params object[] args)
    {
        try
        {
            string format = GetString(key);
            return string.Format(format, args);
        }
        catch
        {
            return $"[{key}]";
        }
    }

    private void ResourceManagerClearCache()
    {
        try
        {
            // Use reflection to clear ResourceManager cache
            var resourceSets = typeof(ResourceManager)
                .GetProperty("ResourceSets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_resourceManager);

            resourceSets?.GetType().GetMethod("Clear")?.Invoke(resourceSets, null);
        }
        catch
        {
            // If reflection fails, recreate the resource manager
            _resourceManager = new ResourceManager("MyApp.Properties.Resources", typeof(App).Assembly);
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
