using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RescueScoreManager.Modules.Planning.ViewModels;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class ProgramMeetingCreationDialog : Window
    {
        public ProgramMeetingCreationDialog()
        {
            InitializeComponent();
        }

        public ProgramMeetingCreationDialog(DateTime currentDate, IEnumerable<SiteViewModel> availableSites) : this()
        {
            InitializeViewModel(currentDate, availableSites);
        }

        private void InitializeViewModel(DateTime currentDate, IEnumerable<SiteViewModel> availableSites)
        {
            try
            {
                var serviceProvider = App.ServiceProvider;
                var xmlService = serviceProvider?.GetService<IXMLService>();
                var localizationService = serviceProvider?.GetService<ILocalizationService>();

                if (xmlService != null && localizationService != null)
                {
                    var viewModel = new ProgramMeetingCreationDialogViewModel(xmlService, localizationService, currentDate, availableSites);
                    DataContext = viewModel;

                    // Subscribe to the creation event
                    viewModel.ProgramMeetingCreated += (sender, e) =>
                    {
                        DialogResult = true;
                        Close();
                    };
                }
            }
            catch (Exception)
            {
                // Fallback to prevent dialog crash
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}