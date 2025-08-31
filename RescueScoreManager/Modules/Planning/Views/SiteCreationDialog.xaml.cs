using System.Windows;

using Microsoft.Extensions.DependencyInjection;

using RescueScoreManager.Modules.Planning.ViewModels;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.Views
{
    public partial class SiteCreationDialog : Window
    {
        public SiteCreationDialog()
        {
            InitializeComponent();
            InitializeViewModel();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void InitializeViewModel()
        {
            try
            {
                var serviceProvider = App.ServiceProvider;
                var localizationService = serviceProvider?.GetService<ILocalizationService>();
                var xmlService = serviceProvider?.GetService<IXMLService>();

                if (localizationService != null && xmlService != null)
                {
                    SiteCreationDialogViewModel viewModel = new SiteCreationDialogViewModel(xmlService, localizationService);
                    viewModel.DialogWindow = this;

                    DataContext = viewModel;
                }
            }
            catch (Exception)
            {
                // Fallback to empty ViewModel if services are not available
                // This prevents the dialog from crashing
            }
        }
    }
}
