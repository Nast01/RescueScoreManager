# Create MVVM Module

Creates a complete MVVM module following RescueScoreManager patterns with View, ViewModel, and proper dependency injection setup.

## Parameters
- `module_name` (required): Name of the module (e.g., "EventManagement")
- `namespace_path` (required): Namespace path (e.g., "Planning", "Properties") 
- `include_dialog` (optional): Set to "true" to include a dialog window
- `base_functionality` (optional): Core functionality description

## Usage
```
@create-mvvm-module module_name="EventManagement" namespace_path="Planning" include_dialog="true" base_functionality="Manage competition events and scheduling"
```

## Implementation Steps

1. **Create ViewModel file**: `RescueScoreManager/Modules/{namespace_path}/ViewModels/{module_name}ViewModel.cs`
   - Inherit from `ObservableObject`
   - Use `[ObservableProperty]` attributes for properties
   - Implement `RelayCommand` for commands
   - Add dependency injection for required services (IXMLService, ILocalizationService, IDialogService, ILogger)
   - Include proper error handling with try-catch and logging
   - Follow naming: OnCommandName for command methods

2. **Create View file**: `RescueScoreManager/Modules/{namespace_path}/Views/{module_name}View.xaml`
   - Use UserControl as base
   - Include proper xmlns declarations
   - Add viewModels namespace reference
   - Use Material Design styling patterns
   - Include BooleanToVisibilityConverter in resources
   - Follow grid-based layout with proper margins

3. **Create Code-behind**: `RescueScoreManager/Modules/{namespace_path}/Views/{module_name}View.xaml.cs`
   - Implement dependency injection in constructor
   - Set DataContext to ViewModel from DI container
   - Add Loaded event handler for initialization
   - Include proper error handling

4. **Update DI Container** (if needed): Add ViewModel registration in `App.xaml.cs`

5. **If include_dialog="true"**: Create additional dialog files following dialog-pair pattern

## Template Structure

### ViewModel Template:
```csharp
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.{namespace_path}.ViewModels
{
    public partial class {module_name}ViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<{module_name}ViewModel> _logger;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public ICommand RefreshCommand { get; }
        public ICommand SaveCommand { get; }

        public {module_name}ViewModel(
            IXMLService xmlService,
            ILocalizationService localizationService,
            IDialogService dialogService,
            ILogger<{module_name}ViewModel> logger)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RefreshCommand = new RelayCommand(OnRefresh);
            SaveCommand = new RelayCommand(OnSave);

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Initialize module data
                _logger.LogInformation("{module_name} module initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing {module_name} module");
            }
        }

        private void OnRefresh()
        {
            try
            {
                IsLoading = true;
                // Refresh logic here
                _logger.LogInformation("{module_name} data refreshed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing {module_name} data");
                _dialogService.ShowMessage("Erreur", "Erreur lors du rafraîchissement des données");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSave()
        {
            try
            {
                // Save logic here
                _xmlService.Save();
                _dialogService.ShowMessage("Succès", "Données sauvegardées avec succès");
                _logger.LogInformation("{module_name} data saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving {module_name} data");
                _dialogService.ShowMessage("Erreur", "Erreur lors de la sauvegarde");
            }
        }
    }
}
```

### View Template:
```xaml
<UserControl x:Class="RescueScoreManager.Modules.{namespace_path}.Views.{module_name}View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:loc="clr-namespace:RescueScoreManager.Localization"
             xmlns:viewModels="clr-namespace:RescueScoreManager.Modules.{namespace_path}.ViewModels"
             Background="White">

  <UserControl.Resources>
    <BooleanToVisibilityConverter x:Key="BooleanToVisibilityConverter"/>
  </UserControl.Resources>

  <Grid Margin="10">
    <Grid.RowDefinitions>
      <RowDefinition Height="Auto"/>
      <RowDefinition Height="*"/>
    </Grid.RowDefinitions>

    <!-- Header -->
    <Grid Grid.Row="0" Margin="0,0,0,10">
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
      </Grid.ColumnDefinitions>

      <StackPanel Grid.Column="0" Orientation="Horizontal" VerticalAlignment="Center">
        <TextBlock Text="📋" FontSize="24" Margin="0,0,10,0" VerticalAlignment="Center"/>
        <TextBlock Text="{module_name}" FontSize="24" FontWeight="Bold" 
                   Foreground="#1F2937" VerticalAlignment="Center"/>
      </StackPanel>

      <StackPanel Grid.Column="1" Orientation="Horizontal" VerticalAlignment="Center">
        <Button Background="#10B981" Foreground="White" BorderThickness="0" 
                Padding="12,8" Margin="0,0,10,0" Cursor="Hand"
                Command="{Binding RefreshCommand}">
          <StackPanel Orientation="Horizontal">
            <TextBlock Text="🔄" FontSize="12" Margin="0,0,5,0"/>
            <TextBlock Text="Actualiser" FontSize="12"/>
          </StackPanel>
        </Button>
        <Button Background="#8B5CF6" Foreground="White" BorderThickness="0" 
                Padding="12,8" Cursor="Hand"
                Command="{Binding SaveCommand}">
          <StackPanel Orientation="Horizontal">
            <TextBlock Text="💾" FontSize="12" Margin="0,0,5,0"/>
            <TextBlock Text="Sauvegarder" FontSize="12"/>
          </StackPanel>
        </Button>
      </StackPanel>
    </Grid>

    <!-- Main Content -->
    <Grid Grid.Row="1">
      <!-- Add your main content here -->
      <TextBlock Text="Module content goes here" 
                 HorizontalAlignment="Center" VerticalAlignment="Center"
                 FontSize="16" Foreground="#6B7280"/>
    </Grid>

    <!-- Loading Overlay -->
    <Grid Grid.Row="0" Grid.RowSpan="2" 
          Background="#80000000" 
          Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}">
      <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
        <ProgressBar IsIndeterminate="True" Width="200" Height="4" Margin="0,0,0,10"/>
        <TextBlock Text="Chargement..." Foreground="White" FontSize="14" HorizontalAlignment="Center"/>
      </StackPanel>
    </Grid>
  </Grid>
</UserControl>
```

## Validation Checklist
- [ ] ViewModel inherits from ObservableObject
- [ ] Uses [ObservableProperty] attributes
- [ ] Includes proper dependency injection
- [ ] Has error handling with logging
- [ ] Follows naming conventions
- [ ] View uses UserControl base
- [ ] Includes proper xmlns references
- [ ] Has loading states and error handling
- [ ] Code-behind sets up DI properly