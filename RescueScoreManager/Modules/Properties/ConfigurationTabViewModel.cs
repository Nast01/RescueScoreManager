using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Properties;

public partial class ConfigurationTabViewModel : ObservableObject
{
    private readonly ILogger<ConfigurationTabViewModel> _logger;
    private readonly IXMLService _xmlService;

    [ObservableProperty]
    private ObservableCollection<RaceFormatConfiguration> _raceFormatConfigurations = new();

    [ObservableProperty]
    private RaceFormatConfiguration? _selectedRaceFormatConfiguration;

    public ConfigurationTabViewModel(
        ILogger<ConfigurationTabViewModel> logger,
        IXMLService xmlService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));

        var configurations = _xmlService.GetRaceFormatConfigurations();
        _raceFormatConfigurations = new ObservableCollection<RaceFormatConfiguration>(configurations);
    }

    #region Public Methods
    public async Task InitializeAsync()
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing race format configuration view");
        }
    }
    #endregion

    #region Private Methods
    private async Task LoadDataAsync()
    {
        await Task.Run(() =>
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RaceFormatConfigurations.Clear();
                var raceConfigurations = _xmlService.GetRaceFormatConfigurations();
                foreach (var raceConfig in raceConfigurations)
                {
                    RaceFormatConfigurations.Add(raceConfig);
                }
            });
        });
    }
    #endregion
}