using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class SiteCreationDialogViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _selectedIcon = "🏊";

        public ICommand CreateCommand { get; }

        public static List<string> AvailableIcons => new() { "🏊", "🏃", "🏄" };

        public Site? CreatedSite { get; private set; }
        
        public Window? DialogWindow { get; set; }

        public SiteCreationDialogViewModel(IXMLService xmlService, ILocalizationService localizationService)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            CreateCommand = new RelayCommand(OnCreate);
        }

        private bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private void OnCreate()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return;
            }

            try
            {
                // Get next available ID
                var sites = _xmlService.GetSites();
                int nextId = sites.Any() ? sites.Max(s => s.Id) + 1 : 1;

                // Create new Site
                CreatedSite = new Site(nextId, Name.Trim(), Description.Trim(), SelectedIcon);
                
                // Close the dialog with success result
                if (DialogWindow != null)
                {
                    DialogWindow.DialogResult = true;
                    DialogWindow.Close();
                }
            }
            catch (Exception ex)
            {
                // Handle error - could log or show message
                CreatedSite = null;
                throw;
            }
        }
    }
}
