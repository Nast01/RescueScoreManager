using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

using Microsoft.Extensions.DependencyInjection;

using RescueScoreManager.Data;
using RescueScoreManager.Modules.Planning.ViewModels;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class RaceConfigurationDialog : Window
    {
        public RaceConfigurationDialog()
        {
            InitializeComponent();
        }

        public RaceConfigurationDialog(string title) : this()
        {
            Title = title;
        }

        public RaceConfigurationDialog(string title, List<Race> races) : this()
        {
            Title = title;
            InitializeViewModel(races);
        }

        private void InitializeViewModel(List<Race> races)
        {
            try
            {
                var serviceProvider = App.ServiceProvider;
                var localizationService = serviceProvider?.GetService<ILocalizationService>();
                var xmlService = serviceProvider?.GetService<IXMLService>();

                if (localizationService != null && xmlService != null)
                {
                    RaceConfigurationDialogViewModel viewModel = new RaceConfigurationDialogViewModel(localizationService, xmlService, races);

                    DataContext = viewModel;
                }
            }
            catch (Exception)
            {
                // Fallback to empty ViewModel if services are not available
                // This prevents the dialog from crashing
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow numeric input (0-9)
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
