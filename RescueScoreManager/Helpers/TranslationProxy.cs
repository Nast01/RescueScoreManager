using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RescueScoreManager.Services;

namespace RescueScoreManager.Helpers;

/// <summary>
/// Helper class that implements indexing for translation lookup
/// </summary>
public class TranslationProxy : INotifyPropertyChanged
{
    private readonly ILocalizationService _localizationService;

    public event PropertyChangedEventHandler PropertyChanged;

    public TranslationProxy()
    {
        // Get localization service
        _localizationService = ResourceManagerLocalizationService.Instance;

        // Subscribe to language changes
        _localizationService.PropertyChanged += (s, e) =>
        {
            // Notify that all indexed values may have changed
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        };
    }

    // Indexer to get translated strings
    public string this[string key]
    {
        get => _localizationService.GetString(key);
    }
}