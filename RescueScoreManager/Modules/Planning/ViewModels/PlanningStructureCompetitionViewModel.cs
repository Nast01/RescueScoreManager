using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public class PlanningStructureCompetitionViewModel : ObservableObject
    {
        private bool _isConfigurationModalOpen;
        private EventViewModel _selectedEvent;

        public ObservableCollection<EventViewModel> Events { get; }

        public bool IsConfigurationModalOpen
        {
            get => _isConfigurationModalOpen;
            set => SetProperty(ref _isConfigurationModalOpen, value);
        }

        public EventViewModel SelectedEvent
        {
            get => _selectedEvent;
            set => SetProperty(ref _selectedEvent, value);
        }

        public ICommand ConfigureEventCommand { get; }
        public ICommand CloseConfigurationCommand { get; }
        public ICommand SaveConfigurationCommand { get; }

        public PlanningStructureCompetitionViewModel()
        {
            Events = new ObservableCollection<EventViewModel>
            {
                new EventViewModel
                {
                    Name = "100m Nage Libre Hommes",
                    ParticipantCount = 24,
                    Category = "Senior",
                    ConfigurationStatus = "2/3 configurés"
                }
            };

            ConfigureEventCommand = new RelayCommand<EventViewModel>(OnConfigureEvent);
            CloseConfigurationCommand = new RelayCommand(OnCloseConfiguration);
            SaveConfigurationCommand = new RelayCommand(OnSaveConfiguration);
        }

        private void OnConfigureEvent(EventViewModel eventViewModel)
        {
            SelectedEvent = eventViewModel;
            IsConfigurationModalOpen = true;
        }

        private void OnCloseConfiguration()
        {
            IsConfigurationModalOpen = false;
            SelectedEvent = null;
        }

        private void OnSaveConfiguration()
        {
            IsConfigurationModalOpen = false;
        }
    }

    public class EventViewModel : ObservableObject
    {
        private string _name;
        private int _participantCount;
        private string _category;
        private string _configurationStatus;
        private bool _isConfigurationByCategory = true;
        private int _seniorParticipants = 6;
        private string _seniorAge = "14-17 ans";
        private int _juniorParticipants = 6;
        private int _selectedSeries = 1;
        private int _participantsPerSeries = 6;
        private string _qualificationMode = "N/A (pas de DF)";
        private int _finalists = 6;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int ParticipantCount
        {
            get => _participantCount;
            set => SetProperty(ref _participantCount, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string ConfigurationStatus
        {
            get => _configurationStatus;
            set => SetProperty(ref _configurationStatus, value);
        }

        public bool IsConfigurationByCategory
        {
            get => _isConfigurationByCategory;
            set => SetProperty(ref _isConfigurationByCategory, value);
        }

        public int SeniorParticipants
        {
            get => _seniorParticipants;
            set => SetProperty(ref _seniorParticipants, value);
        }

        public string SeniorAge
        {
            get => _seniorAge;
            set => SetProperty(ref _seniorAge, value);
        }

        public int JuniorParticipants
        {
            get => _juniorParticipants;
            set => SetProperty(ref _juniorParticipants, value);
        }

        public int SelectedSeries
        {
            get => _selectedSeries;
            set => SetProperty(ref _selectedSeries, value);
        }

        public int ParticipantsPerSeries
        {
            get => _participantsPerSeries;
            set => SetProperty(ref _participantsPerSeries, value);
        }

        public string QualificationMode
        {
            get => _qualificationMode;
            set => SetProperty(ref _qualificationMode, value);
        }

        public int Finalists
        {
            get => _finalists;
            set => SetProperty(ref _finalists, value);
        }

        public ObservableCollection<string> AvailableSeries { get; }
        public ObservableCollection<string> AvailableParticipantsPerSeries { get; }
        public ObservableCollection<string> AvailableQualificationModes { get; }
        public ObservableCollection<string> AvailableFinalists { get; }

        public EventViewModel()
        {
            AvailableSeries = new ObservableCollection<string> { "1 série", "2 séries", "3 séries" };
            AvailableParticipantsPerSeries = new ObservableCollection<string> { "4 participants", "6 participants", "8 participants" };
            AvailableQualificationModes = new ObservableCollection<string> { "N/A (pas de DF)", "Temps", "Places" };
            AvailableFinalists = new ObservableCollection<string> { "4 finalistes", "6 finalistes", "8 finalistes" };
        }
    }
}
