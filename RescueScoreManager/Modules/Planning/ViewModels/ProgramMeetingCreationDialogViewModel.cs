using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class ProgramMeetingCreationDialogViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;
        private readonly DateTime _currentDate;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private SiteViewModel? _selectedSite;

        [ObservableProperty]
        private int _selectedHour = 8;

        [ObservableProperty]
        private int _selectedMinute = 0;

        public ObservableCollection<SiteViewModel> AvailableSites { get; }
        public ObservableCollection<int> AvailableHours { get; }
        public ObservableCollection<int> AvailableMinutes { get; }

        public ICommand CreateCommand { get; }

        public ProgramMeeting? CreatedProgramMeeting { get; private set; }

        public event EventHandler? ProgramMeetingCreated;

        public ProgramMeetingCreationDialogViewModel(
            IXMLService xmlService,
            ILocalizationService localizationService,
            DateTime currentDate,
            IEnumerable<SiteViewModel> availableSites)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _currentDate = currentDate;

            AvailableSites = new ObservableCollection<SiteViewModel>(availableSites);
            AvailableHours = new ObservableCollection<int>();
            AvailableMinutes = new ObservableCollection<int>();

            CreateCommand = new RelayCommand(OnCreate, CanCreate);

            InitializeTimeOptions();

            // Set default site if available
            if (AvailableSites.Any())
            {
                SelectedSite = AvailableSites.First();
            }
        }

        private void InitializeTimeOptions()
        {
            // Hours from 6 AM to 10 PM
            for (int hour = 6; hour <= 22; hour++)
            {
                AvailableHours.Add(hour);
            }

            // Minutes in 10-minute increments
            for (int minute = 0; minute < 60; minute += 10)
            {
                AvailableMinutes.Add(minute);
            }
        }

        private bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(Name) && SelectedSite != null;
        }

        private void OnCreate()
        {
            try
            {
                if (SelectedSite == null)
                    return;

                // Create the start time
                var startTime = _currentDate.Date.AddHours(SelectedHour).AddMinutes(SelectedMinute);

                // Generate a new ID
                var existingMeetings = _xmlService.GetProgramMeetings();
                int newId = existingMeetings.Any() ? existingMeetings.Max(m => m.Id) + 1 : 1;

                // Create the ProgramMeeting with site information in description
                var programMeeting = new ProgramMeeting
                {
                    Id = newId,
                    Name = Name.Trim(),
                    Description = $"Site:{SelectedSite.Name}|{SelectedSite.Id}", // Store site info for retrieval
                    ProgramDate = _currentDate,
                    BeginHour = startTime,
                    EndHour = startTime.AddHours(1), // Default duration, will be updated based on content
                    ProgramSlots = new List<ProgramSlot>()
                };

                // Save to XML
                var allMeetings = existingMeetings.ToList();
                allMeetings.Add(programMeeting);
                _xmlService.UpdateProgramMeetings(allMeetings);
                _xmlService.Save();

                CreatedProgramMeeting = programMeeting;
                ProgramMeetingCreated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // Log error or show message
                System.Diagnostics.Debug.WriteLine($"Error creating ProgramMeeting: {ex.Message}");
            }
        }

        partial void OnNameChanged(string value)
        {
            ((RelayCommand)CreateCommand).NotifyCanExecuteChanged();
        }

        partial void OnSelectedSiteChanged(SiteViewModel? value)
        {
            ((RelayCommand)CreateCommand).NotifyCanExecuteChanged();
        }
    }
}