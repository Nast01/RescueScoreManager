using System.Windows;
using System.Windows.Controls;

namespace RescueScoreManager.Modules.SelectNewCompetition;

/// <summary>
/// Logique d'interaction pour SelectNewCompetitionView.xaml
/// </summary>
public partial class SelectNewCompetitionView : UserControl
{
    public SelectNewCompetitionView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Initialize the view model when the view is loaded
        if (DataContext is SelectNewCompetitionViewModel viewModel)
        {
            await viewModel.InitializeAsync();

            // Set focus to the date picker for better UX
            BeginDatePicker.Focus();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // Clean up resources when view is unloaded
        if (DataContext is IDisposable disposableViewModel)
        {
            disposableViewModel.Dispose();
        }
    }
}
