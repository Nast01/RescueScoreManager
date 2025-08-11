using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RescueScoreManager.Modules.Forfeit;

/// <summary>
/// Logique d'interaction pour ForfeitView.xaml
/// </summary>
public partial class ForfeitView : UserControl
{
    public ForfeitView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ForfeitViewModel viewModel)
        {
            await viewModel.InitializeAsync();

            // Set focus to the first search box for better UX
            SearchNameTextBox.Focus();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ForfeitViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }

    private void OnSearchTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Update the binding source first
            if (sender is TextBox textBox)
            {
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            }

            // Then trigger the search
            if (DataContext is ForfeitViewModel viewModel && viewModel.SearchFiltersCommand.CanExecute(null))
            {
                viewModel.SearchFiltersCommand.Execute(null);
            }

            e.Handled = true;
        }
    }
}
