# Create Dialog Pair

Creates matching Dialog.xaml and DialogViewModel.cs files following RescueScoreManager dialog patterns.

## Parameters
- `dialog_name` (required): Name of the dialog (e.g., "EventCreation", "SiteConfiguration")
- `namespace_path` (required): Namespace path (e.g., "Planning", "Properties")
- `fields` (optional): Comma-separated list of field names (e.g., "Name,Description,StartTime")
- `result_type` (optional): Type of result object (default: "created object")

## Usage
```
@create-dialog-pair dialog_name="EventCreation" namespace_path="Planning" fields="Name,Description,StartTime,Duration" result_type="Event"
```

## Implementation Steps

1. **Create DialogViewModel**: `RescueScoreManager/Modules/{namespace_path}/ViewModels/{dialog_name}DialogViewModel.cs`
   - Inherit from `ObservableObject`
   - Use `[ObservableProperty]` for form fields
   - Implement validation logic
   - Add `CreateCommand` and proper event handling
   - Include result property for created object

2. **Create Dialog XAML**: `RescueScoreManager/Modules/{namespace_path}/Views/{dialog_name}Dialog.xaml`
   - Use Window as base with standard dialog properties
   - Center screen positioning, no resize
   - Standard button layout (Cancel/Create)
   - Form fields with proper styling

3. **Create Code-behind**: `RescueScoreManager/Modules/{namespace_path}/Views/{dialog_name}Dialog.xaml.cs`
   - Handle dialog result setting
   - Implement Cancel/Create button clicks
   - Set up DataContext with DI

## Template Structure

### DialogViewModel Template:
```csharp
using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.{namespace_path}.ViewModels
{
    public partial class {dialog_name}DialogViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        public ICommand CreateCommand { get; }

        public {result_type}? Created{result_type} { get; private set; }

        public event EventHandler? {result_type}Created;

        public {dialog_name}DialogViewModel(
            IXMLService xmlService,
            ILocalizationService localizationService)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

            CreateCommand = new RelayCommand(OnCreate, CanCreate);
        }

        private bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private void OnCreate()
        {
            try
            {
                // Create your object here
                var new{result_type} = new {result_type}
                {
                    Name = Name.Trim(),
                    Description = Description?.Trim() ?? string.Empty,
                    // Add other properties as needed
                };

                // Save to service if needed
                // _xmlService.Add{result_type}(new{result_type});
                // _xmlService.Save();

                Created{result_type} = new{result_type};
                {result_type}Created?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Log error and show message
                System.Diagnostics.Debug.WriteLine($"Error creating {result_type}: {ex.Message}");
            }
        }

        partial void OnNameChanged(string value)
        {
            ((RelayCommand)CreateCommand).NotifyCanExecuteChanged();
        }
    }
}
```

### Dialog XAML Template:
```xaml
<Window x:Class="RescueScoreManager.Modules.{namespace_path}.Views.{dialog_name}Dialog"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:loc="clr-namespace:RescueScoreManager.Localization"
        Title="Créer {result_type}" Height="400" Width="500"
        WindowStartupLocation="CenterScreen"
        ResizeMode="NoResize"
        Background="White">

    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <TextBlock Grid.Row="0" Text="Créer un nouveau {result_type}" 
                   FontSize="18" FontWeight="Bold" 
                   Foreground="#1F2937" Margin="0,0,0,20"/>

        <!-- Form Content -->
        <StackPanel Grid.Row="1" Spacing="15">
            <!-- Name Field -->
            <StackPanel>
                <TextBlock Text="Nom :" FontWeight="SemiBold" FontSize="14" Margin="0,0,0,8"/>
                <TextBox Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}" 
                         Background="#F9FAFB" BorderBrush="#E5E7EB" 
                         Padding="12" Height="40" FontSize="14"/>
            </StackPanel>

            <!-- Description Field -->
            <StackPanel>
                <TextBlock Text="Description :" FontWeight="SemiBold" FontSize="14" Margin="0,0,0,8"/>
                <TextBox Text="{Binding Description, UpdateSourceTrigger=PropertyChanged}" 
                         Background="#F9FAFB" BorderBrush="#E5E7EB" 
                         Padding="12" Height="80" FontSize="14"
                         TextWrapping="Wrap" AcceptsReturn="True"/>
            </StackPanel>

            <!-- Add additional fields based on 'fields' parameter -->
        </StackPanel>

        <!-- Footer Buttons -->
        <Grid Grid.Row="2" Margin="0,20,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <Button Grid.Column="1" Content="Annuler" 
                    Background="Transparent" BorderBrush="#E5E7EB" BorderThickness="1" 
                    Padding="20,10" Margin="0,0,10,0"
                    Click="Cancel_Click">
                <Button.Template>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}" 
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}"
                                CornerRadius="6" 
                                Padding="{TemplateBinding Padding}">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                    </ControlTemplate>
                </Button.Template>
            </Button>

            <Button Grid.Column="2" Content="Créer" 
                    Background="#2563EB" Foreground="White" BorderThickness="0"
                    Padding="20,10"
                    Command="{Binding CreateCommand}"
                    Click="Create_Click">
                <Button.Template>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}" 
                                CornerRadius="6" 
                                Padding="{TemplateBinding Padding}">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                    </ControlTemplate>
                </Button.Template>
            </Button>
        </Grid>
    </Grid>
</Window>
```

### Code-behind Template:
```csharp
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RescueScoreManager.Modules.{namespace_path}.ViewModels;

namespace RescueScoreManager.Modules.{namespace_path}.Views
{
    public partial class {dialog_name}Dialog : Window
    {
        public {dialog_name}Dialog()
        {
            InitializeComponent();
            
            // Set up DataContext via DI
            var viewModel = App.ServiceProvider?.GetService<{dialog_name}DialogViewModel>();
            DataContext = viewModel;
            
            // Subscribe to creation event
            if (viewModel != null)
            {
                viewModel.{result_type}Created += OnObjectCreated;
            }
        }

        private void OnObjectCreated(object? sender, EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Command will handle the creation
            // Dialog result will be set in OnObjectCreated
        }
    }
}
```

## Validation Checklist
- [ ] DialogViewModel inherits from ObservableObject
- [ ] Uses [ObservableProperty] for form fields
- [ ] Implements CreateCommand with CanExecute validation
- [ ] Dialog uses Window with proper properties
- [ ] Has standard Cancel/Create button layout
- [ ] Code-behind handles dialog result properly
- [ ] Includes proper error handling
- [ ] Follows project styling patterns
- [ ] ViewModel registered in DI container