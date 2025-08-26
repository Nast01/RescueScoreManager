using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public class PlanningFinalValidationViewModel : ObservableObject
    {
        private bool _isAllEventsConfigured = true;
        private bool _isScheduleComplete = true;
        private bool _areVenuesAssigned = true;
        private int _totalEvents = 5;
        private int _configuredEvents = 5;
        private int _totalParticipants = 76;

        public bool IsAllEventsConfigured
        {
            get => _isAllEventsConfigured;
            set => SetProperty(ref _isAllEventsConfigured, value);
        }

        public bool IsScheduleComplete
        {
            get => _isScheduleComplete;
            set => SetProperty(ref _isScheduleComplete, value);
        }

        public bool AreVenuesAssigned
        {
            get => _areVenuesAssigned;
            set => SetProperty(ref _areVenuesAssigned, value);
        }

        public int TotalEvents
        {
            get => _totalEvents;
            set => SetProperty(ref _totalEvents, value);
        }

        public int ConfiguredEvents
        {
            get => _configuredEvents;
            set => SetProperty(ref _configuredEvents, value);
        }

        public int TotalParticipants
        {
            get => _totalParticipants;
            set => SetProperty(ref _totalParticipants, value);
        }

        public bool CanExport => IsAllEventsConfigured && IsScheduleComplete && AreVenuesAssigned;

        public ObservableCollection<ValidationItemViewModel> ValidationItems { get; }
        public ObservableCollection<ExportFormatViewModel> ExportFormats { get; }

        public ICommand ExportCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand SaveDraftCommand { get; }

        public PlanningFinalValidationViewModel()
        {
            ValidationItems = new ObservableCollection<ValidationItemViewModel>
            {
                new ValidationItemViewModel { Title = "Épreuves configurées", Status = "5/5 complètes", IsValid = true },
                new ValidationItemViewModel { Title = "Planification horaires", Status = "Complète", IsValid = true },
                new ValidationItemViewModel { Title = "Attribution des lieux", Status = "Complète", IsValid = true },
                new ValidationItemViewModel { Title = "Participants inscrits", Status = "76 participants", IsValid = true }
            };

            ExportFormats = new ObservableCollection<ExportFormatViewModel>
            {
                new ExportFormatViewModel { Name = "PDF - Planning complet", IsSelected = true },
                new ExportFormatViewModel { Name = "Excel - Feuilles de résultats", IsSelected = false },
                new ExportFormatViewModel { Name = "JSON - Données brutes", IsSelected = false }
            };

            ExportCommand = new RelayCommand(OnExport, () => CanExport);
            PreviewCommand = new RelayCommand(OnPreview);
            SaveDraftCommand = new RelayCommand(OnSaveDraft);
        }

        private void OnExport()
        {
            // Export logic
        }

        private void OnPreview()
        {
            // Preview logic
        }

        private void OnSaveDraft()
        {
            // Save draft logic
        }
    }

    public class ValidationItemViewModel
    {
        public string Title { get; set; }
        public string Status { get; set; }
        public bool IsValid { get; set; }
    }

    public class ExportFormatViewModel : ObservableObject
    {
        private string _name;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
