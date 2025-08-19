using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Properties;

public partial class GeneraleTabViewModel : ObservableObject
{
    private readonly ILogger<GeneraleTabViewModel> _logger;
    private readonly IXMLService _xmlService;
    private AppSetting _appSetting = new();

    public GeneraleTabViewModel(
        ILogger<GeneraleTabViewModel> logger,
        IXMLService xmlService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
    }

    #region Wrapper Properties
    public int NumberOfLanes
    {
        get => _appSetting.NumberOfLanes;
        set
        {
            if (_appSetting.NumberOfLanes != value)
            {
                _appSetting.NumberOfLanes = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public bool IsRankingByTime
    {
        get => _appSetting.IsRankingByTime;
        set
        {
            if (value && !_appSetting.IsRankingByTime)
            {
                _appSetting.IsRankingByTime = true;
                _appSetting.IsRankingByFinal = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRankingByFinal));
                _ = SaveSettingsAsync();
            }
        }
    }

    public bool IsRankingByFinal
    {
        get => _appSetting.IsRankingByFinal;
        set
        {
            if (value && !_appSetting.IsRankingByFinal)
            {
                _appSetting.IsRankingByFinal = true;
                _appSetting.IsRankingByTime = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRankingByTime));
                _ = SaveSettingsAsync();
            }
        }
    }

    public int NumberOfAthleteComputedByClub
    {
        get => _appSetting.NumberOfAthleteComputedByClub;
        set
        {
            if (_appSetting.NumberOfAthleteComputedByClub != value)
            {
                _appSetting.NumberOfAthleteComputedByClub = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public int NumberOfRelayComputedByClub
    {
        get => _appSetting.NumberOfRelayComputedByClub;
        set
        {
            if (_appSetting.NumberOfRelayComputedByClub != value)
            {
                _appSetting.NumberOfRelayComputedByClub = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public bool HasAres
    {
        get => _appSetting.HasAres;
        set
        {
            if (_appSetting.HasAres != value)
            {
                _appSetting.HasAres = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public string ChronoPath
    {
        get => _appSetting.ChronoPath;
        set
        {
            if (_appSetting.ChronoPath != value)
            {
                _appSetting.ChronoPath = value ?? string.Empty;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public bool IsRelayByHeatFinal
    {
        get => _appSetting.IsRelayByHeatFinal;
        set
        {
            if (_appSetting.IsRelayByHeatFinal != value)
            {
                _appSetting.IsRelayByHeatFinal = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public int NumberOfForeignAthleteInFinal
    {
        get => _appSetting.NumberOfForeignAthleteInFinal;
        set
        {
            if (_appSetting.NumberOfForeignAthleteInFinal != value)
            {
                _appSetting.NumberOfForeignAthleteInFinal = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public int NumberOfAthleteInFinal
    {
        get => _appSetting.NumberOfAthleteInFinal;
        set
        {
            if (_appSetting.NumberOfAthleteInFinal != value)
            {
                _appSetting.NumberOfAthleteInFinal = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public int NumberMinOfAthleteInFinalByHeat
    {
        get => _appSetting.NumberMinOfAthleteInFinalByHeat;
        set
        {
            if (_appSetting.NumberMinOfAthleteInFinalByHeat != value)
            {
                _appSetting.NumberMinOfAthleteInFinalByHeat = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }

    public bool HasPointForeignAthlete
    {
        get => _appSetting.HasPointForeignAthlete;
        set
        {
            if (_appSetting.HasPointForeignAthlete != value)
            {
                _appSetting.HasPointForeignAthlete = value;
                OnPropertyChanged();
                _ = SaveSettingsAsync();
            }
        }
    }
    #endregion

    #region Public Methods
    public async Task InitializeAsync()
    {
        try
        {
            await LoadGeneralSettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing general settings");
        }
    }
    #endregion

    #region Private Methods
    private async Task LoadGeneralSettingsAsync()
    {
        await Task.Run(() =>
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var settings = _xmlService.GetSetting();
                if (settings != null)
                {
                    _appSetting = settings;
                }
                else
                {
                    _appSetting = new AppSetting();
                }
                
                // Ensure at least one ranking option is selected
                if (!_appSetting.IsRankingByTime && !_appSetting.IsRankingByFinal)
                {
                    _appSetting.IsRankingByTime = true;
                }
                
                // Notify all wrapper properties that their values may have changed
                OnPropertyChanged(nameof(NumberOfLanes));
                OnPropertyChanged(nameof(IsRankingByTime));
                OnPropertyChanged(nameof(IsRankingByFinal));
                OnPropertyChanged(nameof(NumberOfAthleteComputedByClub));
                OnPropertyChanged(nameof(NumberOfRelayComputedByClub));
                OnPropertyChanged(nameof(HasAres));
                OnPropertyChanged(nameof(ChronoPath));
                OnPropertyChanged(nameof(IsRelayByHeatFinal));
                OnPropertyChanged(nameof(NumberOfForeignAthleteInFinal));
                OnPropertyChanged(nameof(NumberOfAthleteInFinal));
                OnPropertyChanged(nameof(NumberMinOfAthleteInFinalByHeat));
                OnPropertyChanged(nameof(HasPointForeignAthlete));
            });
        });
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                bool success = _xmlService.Save();
                if (success)
                {
                    _logger.LogInformation("AppSettings saved successfully");
                }
                else
                {
                    _logger.LogError("Failed to save AppSettings");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving AppSettings");
        }
    }


    #endregion
}