using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface ILocalizationService : INotifyPropertyChanged
{
    ObservableCollection<LanguageModel> AvailableLanguages { get; }
    LanguageModel CurrentLanguage { get; set; }
    string GetString(string key);
    string GetString(string key, params object[] args);
}
