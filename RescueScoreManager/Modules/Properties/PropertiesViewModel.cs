using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace RescueScoreManager.Modules.Properties;

public partial class PropertiesViewModel : ObservableObject
{
    private readonly ILogger<PropertiesViewModel> _logger;

    [ObservableProperty]
    private GeneraleTabViewModel _generaleTabViewModel;

    [ObservableProperty]
    private ConfigurationTabViewModel _configurationTabViewModel;

    public PropertiesViewModel(
        ILogger<PropertiesViewModel> logger,
        GeneraleTabViewModel generaleTabViewModel,
        ConfigurationTabViewModel configurationTabViewModel)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _generaleTabViewModel = generaleTabViewModel ?? throw new ArgumentNullException(nameof(generaleTabViewModel));
        _configurationTabViewModel = configurationTabViewModel ?? throw new ArgumentNullException(nameof(configurationTabViewModel));
    }

    #region Public Methods
    public async Task InitializeAsync()
    {
        try
        {
            await Task.WhenAll(
                GeneraleTabViewModel.InitializeAsync(),
                ConfigurationTabViewModel.InitializeAsync()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing properties view");
        }
    }
    #endregion
}
