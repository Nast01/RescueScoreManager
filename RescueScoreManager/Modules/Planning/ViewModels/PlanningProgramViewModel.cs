using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Modules.Planning.Views;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class PlanningProgramViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<PlanningProgramViewModel> _logger;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private DateTime _currentDate = DateTime.Today;

        [ObservableProperty]
        private string _currentDateDisplay = string.Empty;

        [ObservableProperty]
        private int _eventsToplanCount;

        [ObservableProperty]
        private int _plannedEventsCount;

        public ObservableCollection<PlanningEventViewModel> EventsToplan { get; }
        public ObservableCollection<PlanningEventViewModel> FilteredEventsToplan { get; }
        public ObservableCollection<SiteViewModel> Sites { get; }
        public ObservableCollection<RaceFormatDetail> RaceFormatDetails { get; }
        
        private readonly Dictionary<DateTime, List<SiteViewModel>> _sitesByDate = new();

        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand PreviousDayCommand { get; }
        public ICommand NextDayCommand { get; }
        public ICommand AutoPlanCommand { get; }
        public ICommand AddSiteCommand { get; }
        public ICommand RemoveSiteCommand { get; }
        public ICommand CreateManualTimeSlotCommand { get; }
        public ICommand CreateProgramMeetingCommand { get; }

        public PlanningProgramViewModel(
            IXMLService xmlService,
            ILocalizationService localizationService,
            IDialogService dialogService,
            ILogger<PlanningProgramViewModel> logger)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            EventsToplan = new ObservableCollection<PlanningEventViewModel>();
            FilteredEventsToplan = new ObservableCollection<PlanningEventViewModel>();
            Sites = new ObservableCollection<SiteViewModel>();
            RaceFormatDetails = new ObservableCollection<RaceFormatDetail>();

            SaveCommand = new RelayCommand(OnSave);
            ExportCommand = new RelayCommand(OnExport);
            PreviousDayCommand = new RelayCommand(OnPreviousDay);
            NextDayCommand = new RelayCommand(OnNextDay);
            AutoPlanCommand = new RelayCommand(OnAutoPlan);
            AddSiteCommand = new RelayCommand(OnAddSite);
            RemoveSiteCommand = new RelayCommand<SiteViewModel>(OnRemoveSite);
            CreateManualTimeSlotCommand = new RelayCommand(OnCreateManualTimeSlot);
            CreateProgramMeetingCommand = new RelayCommand(OnCreateProgramMeeting);

            Initialize();
        }
        
        partial void OnSearchTextChanged(string value)
        {
            FilterEvents();
        }

        private void Initialize()
        {
            // Set current date to competition begin date if not within range
            Competition? competition = _xmlService.GetCompetition();
            if (competition != null)
            {
                if (CurrentDate < competition.BeginDate || CurrentDate > competition.EndDate)
                {
                    CurrentDate = competition.BeginDate;
                }
            }
            
            UpdateCurrentDateDisplay();
            LoadEventsToplan();
            LoadSites();
            LoadRaceFormatDetails();
            LoadPlanningData();
            UpdateStatistics();
            FilterEvents();
        }

        private void UpdateCurrentDateDisplay()
        {
            CurrentDateDisplay = $"{GetDayName(CurrentDate.DayOfWeek)} {CurrentDate.Day} {GetMonthName(CurrentDate.Month)} {CurrentDate.Year}";
        }

        private string GetDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Lundi",
                DayOfWeek.Tuesday => "Mardi",
                DayOfWeek.Wednesday => "Mercredi",
                DayOfWeek.Thursday => "Jeudi",
                DayOfWeek.Friday => "Vendredi",
                DayOfWeek.Saturday => "Samedi",
                DayOfWeek.Sunday => "Dimanche",
                _ => dayOfWeek.ToString()
            };
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Janvier",
                2 => "Février",
                3 => "Mars",
                4 => "Avril",
                5 => "Mai",
                6 => "Juin",
                7 => "Juillet",
                8 => "Août",
                9 => "Septembre",
                10 => "Octobre",
                11 => "Novembre",
                12 => "Décembre",
                _ => month.ToString()
            };
        }

        private void LoadEventsToplan()
        {
            EventsToplan.Clear();

            try
            {
                IReadOnlyList<Race> races = _xmlService.GetRaces();
                IReadOnlyList<RaceFormatConfiguration> raceFormatConfigurations = _xmlService.GetRaceFormatConfigurations();

                foreach (RaceFormatConfiguration config in raceFormatConfigurations)
                {
                    foreach (RaceFormatDetail detail in config.RaceFormatDetails)
                    {
                        PlanningEventViewModel eventViewModel = new PlanningEventViewModel
                        {
                            Id = detail.Id,
                            Name = detail.Label,
                            ConfigurationLabel = config.Label,
                            ConfigurationColor = GetConfigurationColor(config.Discipline),
                            StatusColor = GetStatusColor(detail),
                            ParticipantCount = GetParticipantCount(detail),
                            Duration = detail.NumberOfRun * 10,
                            IsPlanned = false
                        };

                        EventsToplan.Add(eventViewModel);
                    }
                }

                // Sort events by name
                List<PlanningEventViewModel> sortedEvents = EventsToplan.OrderBy(e => e.Name).ToList();
                EventsToplan.Clear();
                foreach (PlanningEventViewModel evt in sortedEvents)
                {
                    EventsToplan.Add(evt);
                }
                
                FilterEvents();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading events to plan");
            }
        }
        
        private void FilterEvents()
        {
            FilteredEventsToplan.Clear();
            
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (PlanningEventViewModel evt in EventsToplan)
                {
                    FilteredEventsToplan.Add(evt);
                }
            }
            else
            {
                List<PlanningEventViewModel> filtered = EventsToplan.Where(e => 
                    e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.ConfigurationLabel.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                foreach (var evt in filtered)
                {
                    FilteredEventsToplan.Add(evt);
                }
            }
            
            EventsToplanCount = FilteredEventsToplan.Count;
        }

        private void LoadSites()
        {
            Sites.Clear();
            _sitesByDate.Clear();

            var sites = _xmlService.GetSites();
            var competition = _xmlService.GetCompetition();
            
            if (sites.Count > 0 && competition != null)
            {
                // Create site instances for each day of the competition
                for (var date = competition.BeginDate.Date; date <= competition.EndDate.Date; date = date.AddDays(1))
                {
                    var dailySites = new List<SiteViewModel>();
                    int colorIndex = 0;
                    
                    foreach (var site in sites)
                    {
                        string siteColor = GetSiteColor(colorIndex++);
                        dailySites.Add(new SiteViewModel
                        {
                            Id = site.Id,
                            Name = site.Name,
                            Description = site.Description,
                            Icon = site.Icon,
                            Color = siteColor,
                            Date = date,
                            TimeSlots = CreateTimeSlots(date, siteColor)
                        });
                    }
                    
                    _sitesByDate[date] = dailySites;
                }
            }
            
            // Load sites for current date
            UpdateSitesForCurrentDate();
        }

        private void UpdateSitesForCurrentDate()
        {
            Sites.Clear();
            
            if (_sitesByDate.TryGetValue(CurrentDate.Date, out var dailySites))
            {
                foreach (var site in dailySites)
                {
                    Sites.Add(site);
                }
            }
        }

        private List<TimeSlotViewModel> CreateTimeSlots(DateTime? date = null, string siteColor = "#F9FAFB")
        {
            var targetDate = date ?? CurrentDate;
            var timeSlots = new List<TimeSlotViewModel>();
            var startTime = new TimeSpan(7, 30, 0);

            for (int i = 0; i < 70; i++)
            {
                var currentTime = startTime.Add(TimeSpan.FromMinutes(i * 10));
                timeSlots.Add(new TimeSlotViewModel
                {
                    Time = currentTime.ToString(@"hh\:mm"),
                    DateTime = targetDate.Add(currentTime),
                    SiteColor = siteColor,
                    Events = new ObservableCollection<PlannedEventViewModel>()
                });
            }

            return timeSlots;
        }

        private string GetConfigurationColor(int discipline)
        {
            return discipline switch
            {
                1 => "#3B82F6", // EauPlate
                2 => "#F59E0B", // Cotier
                3 => "#8B5CF6", // Mixte
                _ => "#6B7280"
            };
        }

        private string GetSiteColor(int index)
        {
            string[] colors = new[]
            {
                "#3B82F6", // Blue
                "#10B981", // Green
                "#F59E0B", // Orange
                "#8B5CF6", // Purple
                "#EF4444", // Red
                "#06B6D4", // Cyan
                "#84CC16", // Lime
                "#F97316", // Orange alt
                "#EC4899", // Pink
                "#6366F1"  // Indigo
            };
            
            return colors[index % colors.Length];
        }

        private string GetStatusColor(RaceFormatDetail detail)
        {
            return "#F59E0B"; // Orange for pending
        }

        private int GetParticipantCount(RaceFormatDetail detail)
        {
            try
            {
                // Get the parent RaceFormatConfiguration
                var raceFormatConfiguration = detail.RaceFormatConfiguration;
                
                if (raceFormatConfiguration == null)
                {
                    // If not accessible directly, find it from the XML service
                    var allConfigurations = _xmlService.GetRaceFormatConfigurations();
                    raceFormatConfiguration = allConfigurations.FirstOrDefault(config => 
                        config.RaceFormatDetails.Any(d => d.Id == detail.Id));
                }
                
                if (raceFormatConfiguration != null)
                {
                    // Get all races from XML service
                    var allRaces = _xmlService.GetRaces();
                    
                    // Find races that match the configuration criteria
                    var matchingRaces = allRaces.Where(race => 
                        race.Discipline == raceFormatConfiguration.Discipline &&
                        race.Gender == raceFormatConfiguration.Gender &&
                        raceFormatConfiguration.Categories.Any(configCat => 
                            race.Categories.Any(raceCat => raceCat.Id == configCat.Id))
                    ).ToList();
                    
                    // Calculate total participant count from all matching races
                    return matchingRaces.Sum(race => race.GetAvailableTeams().Count);
                }
                
                return 0; // Return 0 if configuration not found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating participant count for RaceFormatDetail {DetailId}", detail.Id);
                return 0;
            }
        }

        private int GetParticipantCountNullable(RaceFormatDetail? detail)
        {
            return detail == null ? 0 : GetParticipantCount(detail);
        }

        private void LoadRaceFormatDetails()
        {
            RaceFormatDetails.Clear();

            try
            {
                var raceFormatConfigurations = _xmlService.GetRaceFormatConfigurations();
                foreach (var config in raceFormatConfigurations)
                {
                    foreach (var detail in config.RaceFormatDetails)
                    {
                        RaceFormatDetails.Add(detail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading race format details");
            }
        }

        private void UpdateStatistics()
        {
            PlannedEventsCount = Sites.SelectMany(s => s.TimeSlots).SelectMany(ts => ts.Events).Count();
        }

        private int GetRequiredSlotCount(int durationInMinutes)
        {
            // Calculate slots needed based on 10-minute slots as specified
            return (int)Math.Ceiling(durationInMinutes / 10.0);
        }

        private List<TimeSlotViewModel> GetConsecutiveAvailableSlots(SiteViewModel site, TimeSlotViewModel startingSlot, int requiredSlots, PlannedEventViewModel? excludeEvent = null)
        {
            int startIndex = site.TimeSlots.IndexOf(startingSlot);
            if (startIndex == -1 || startIndex + requiredSlots > site.TimeSlots.Count)
            {
                return new List<TimeSlotViewModel>();
            }

            var consecutiveSlots = new List<TimeSlotViewModel>();
            for (int i = startIndex; i < startIndex + requiredSlots && i < site.TimeSlots.Count; i++)
            {
                var slot = site.TimeSlots[i];
                // Check if slot has any events that would conflict (excluding the specified event if provided)
                var conflictingEvents = excludeEvent != null 
                    ? slot.Events.Where(e => e != excludeEvent).ToList()
                    : slot.Events.ToList();
                
                if (conflictingEvents.Any())
                {
                    return new List<TimeSlotViewModel>(); // Slot is occupied by other events
                }
                consecutiveSlots.Add(slot);
            }

            return consecutiveSlots.Count == requiredSlots ? consecutiveSlots : new List<TimeSlotViewModel>();
        }

        public void MoveEventToTimeSlot(object eventItem, object timeSlotItem)
        {
            if (eventItem is PlanningEventViewModel planningEvent && timeSlotItem is TimeSlotViewModel startingTimeSlot)
            {
                // Check if this event is already in the timeslot to prevent duplicates
                if (startingTimeSlot.Events.Any(e => e.Id == planningEvent.Id))
                {
                    return;
                }

                // Find the site containing this time slot
                var targetSite = Sites.FirstOrDefault(s => s.TimeSlots.Contains(startingTimeSlot));
                if (targetSite == null)
                {
                    return;
                }

                // Calculate required slots and get consecutive available slots
                int requiredSlots = GetRequiredSlotCount(planningEvent.Duration);
                List<TimeSlotViewModel> availableSlots = GetConsecutiveAvailableSlots(targetSite, startingTimeSlot, requiredSlots);

                if (availableSlots.Count != requiredSlots)
                {
                    _logger.LogWarning($"Cannot place event {planningEvent.Name}: not enough consecutive available slots (need {requiredSlots}, found {availableSlots.Count})");
                    return;
                }

                // Create the planned event
                var plannedEvent = new PlannedEventViewModel
                {
                    Id = planningEvent.Id,
                    Title = planningEvent.Name,
                    Subtitle = $"{planningEvent.ParticipantCount} participants",
                    Color = startingTimeSlot.SiteColor,
                    Duration = planningEvent.Duration,
                    ConfigurationLabel = planningEvent.ConfigurationLabel,
                    StatusColor = planningEvent.StatusColor,
                    ParticipantCount = planningEvent.ParticipantCount
                };

                // Add the event to all required consecutive slots
                foreach (var slot in availableSlots)
                {
                    slot.Events.Add(plannedEvent);
                }

                EventsToplan.Remove(planningEvent);
                FilterEvents();

                UpdateStatistics();
                SavePlanningData();
                
                _logger.LogInformation($"Moved event {planningEvent.Name} to {requiredSlots} time slots starting at {startingTimeSlot.Time}");
            }
        }

        public void RemoveEventFromTimeSlot(object plannedEventItem)
        {
            if (plannedEventItem is PlannedEventViewModel plannedEvent)
            {
                // Find all timeslots containing this exact event instance
                var containingTimeSlots = new List<TimeSlotViewModel>();
                foreach (var site in Sites)
                {
                    foreach (var timeSlot in site.TimeSlots)
                    {
                        if (timeSlot.Events.Contains(plannedEvent))
                        {
                            containingTimeSlots.Add(timeSlot);
                        }
                    }
                }

                if (containingTimeSlots.Any())
                {
                    // Remove the exact event instance from all timeslots it occupies
                    foreach (var timeSlot in containingTimeSlots)
                    {
                        timeSlot.Events.Remove(plannedEvent);
                    }

                    // Convert PlannedEventViewModel back to PlanningEventViewModel
                    var restoredEvent = new PlanningEventViewModel
                    {
                        Id = plannedEvent.Id,
                        Name = plannedEvent.Title,
                        ConfigurationLabel = plannedEvent.ConfigurationLabel,
                        ConfigurationColor = plannedEvent.Color,
                        StatusColor = plannedEvent.StatusColor,
                        ParticipantCount = plannedEvent.ParticipantCount,
                        Duration = plannedEvent.Duration,
                        IsPlanned = false
                    };

                    EventsToplan.Add(restoredEvent);

                    // Sort events by name after adding
                    var sortedEvents = EventsToplan.OrderBy(e => e.Name).ToList();
                    EventsToplan.Clear();
                    foreach (var evt in sortedEvents)
                    {
                        EventsToplan.Add(evt);
                    }

                    FilterEvents();
                    UpdateStatistics();
                    SavePlanningData();
                    _logger.LogInformation($"Removed event {plannedEvent.Title} from {containingTimeSlots.Count} time slots and restored to planning list");
                }
            }
        }

        public void MovePlannedEventToTimeSlot(object plannedEventItem, object timeSlotItem)
        {
            if (plannedEventItem is PlannedEventViewModel plannedEvent && timeSlotItem is TimeSlotViewModel targetTimeSlot)
            {
                // Find all source timeslots containing this exact event instance
                var sourceTimeSlots = new List<TimeSlotViewModel>();
                SiteViewModel? sourceSite = null;
                foreach (var site in Sites)
                {
                    foreach (var timeSlot in site.TimeSlots)
                    {
                        if (timeSlot.Events.Contains(plannedEvent))
                        {
                            sourceTimeSlots.Add(timeSlot);
                            if (sourceSite == null)
                            {
                                sourceSite = site;
                            }
                        }
                    }
                }

                // Find the target site containing the target time slot
                var targetSite = Sites.FirstOrDefault(s => s.TimeSlots.Contains(targetTimeSlot));
                if (targetSite == null)
                {
                    return;
                }

                if (sourceTimeSlots.Any() && !sourceTimeSlots.Contains(targetTimeSlot))
                {
                    // Check if this event is already in the target timeslot to prevent duplicates
                    if (targetTimeSlot.Events.Any(e => e.Id == plannedEvent.Id))
                    {
                        return;
                    }

                    // Calculate required slots and get consecutive available slots (excluding the current event)
                    int requiredSlots = GetRequiredSlotCount(plannedEvent.Duration);
                    var availableSlots = GetConsecutiveAvailableSlots(targetSite, targetTimeSlot, requiredSlots, plannedEvent);

                    if (availableSlots.Count != requiredSlots)
                    {
                        _logger.LogWarning($"Cannot move event {plannedEvent.Title}: not enough consecutive available slots (need {requiredSlots}, found {availableSlots.Count})");
                        return;
                    }

                    // Remove from all source timeslots
                    foreach (var sourceSlot in sourceTimeSlots)
                    {
                        sourceSlot.Events.Remove(plannedEvent);
                    }

                    // Create a new event with the target site's color
                    var movedEvent = new PlannedEventViewModel
                    {
                        Id = plannedEvent.Id,
                        Title = plannedEvent.Title,
                        Subtitle = plannedEvent.Subtitle,
                        Color = targetTimeSlot.SiteColor,
                        Duration = plannedEvent.Duration,
                        ConfigurationLabel = plannedEvent.ConfigurationLabel,
                        StatusColor = plannedEvent.StatusColor,
                        ParticipantCount = plannedEvent.ParticipantCount
                    };

                    // Add to all required target timeslots
                    foreach (var slot in availableSlots)
                    {
                        slot.Events.Add(movedEvent);
                    }

                    UpdateStatistics();
                    SavePlanningData();
                    
                    _logger.LogInformation($"Moved planned event {plannedEvent.Title} from {sourceTimeSlots.Count} slots to {requiredSlots} slots starting at {targetTimeSlot.Time}");
                }
            }
        }
        public void RefreshData()
        {
            Initialize();
        }

        public void MoveEventToProgramMeeting(object eventItem, object programMeetingItem)
        {
            if (eventItem is PlanningEventViewModel planningEvent && programMeetingItem is ProgramMeetingViewModel programMeeting)
            {
                try
                {
                    // Create the planned event for the program meeting
                    var plannedEvent = new PlannedEventViewModel
                    {
                        Id = planningEvent.Id,
                        Title = planningEvent.Name,
                        Subtitle = $"{planningEvent.ParticipantCount} participants",
                        Color = programMeeting.SiteColor,
                        Duration = planningEvent.Duration,
                        ConfigurationLabel = planningEvent.ConfigurationLabel,
                        StatusColor = planningEvent.StatusColor,
                        ParticipantCount = planningEvent.ParticipantCount
                    };

                    // Add to program meeting
                    programMeeting.Events.Add(plannedEvent);
                    
                    // Update program meeting end time based on events
                    UpdateProgramMeetingEndTime(programMeeting);

                    // Remove from events to plan
                    EventsToplan.Remove(planningEvent);
                    FilterEvents();
                    UpdateStatistics();
                    
                    // Save the changes
                    SaveProgramMeetingData(programMeeting);
                    
                    _logger.LogInformation($"Moved event {planningEvent.Name} to program meeting {programMeeting.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error moving event to program meeting");
                }
            }
        }

        public void MovePlannedEventToProgramMeeting(object plannedEventItem, object programMeetingItem)
        {
            if (plannedEventItem is PlannedEventViewModel plannedEvent && programMeetingItem is ProgramMeetingViewModel programMeeting)
            {
                try
                {
                    // Remove from current location (time slots or other program meetings)
                    RemoveEventFromCurrentLocation(plannedEvent);

                    // Update color to match new program meeting
                    plannedEvent.Color = programMeeting.SiteColor;

                    // Add to new program meeting
                    programMeeting.Events.Add(plannedEvent);
                    
                    // Update program meeting end time
                    UpdateProgramMeetingEndTime(programMeeting);
                    
                    UpdateStatistics();
                    
                    // Save the changes
                    SaveProgramMeetingData(programMeeting);
                    
                    _logger.LogInformation($"Moved planned event {plannedEvent.Title} to program meeting {programMeeting.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error moving planned event to program meeting");
                }
            }
        }

        private void RemoveEventFromCurrentLocation(PlannedEventViewModel plannedEvent)
        {
            // Remove from time slots
            foreach (var site in Sites)
            {
                foreach (var timeSlot in site.TimeSlots)
                {
                    if (timeSlot.Events.Contains(plannedEvent))
                    {
                        timeSlot.Events.Remove(plannedEvent);
                    }
                }
                
                // Remove from other program meetings
                foreach (var programMeeting in site.ProgramMeetings)
                {
                    if (programMeeting.Events.Contains(plannedEvent))
                    {
                        programMeeting.Events.Remove(plannedEvent);
                        UpdateProgramMeetingEndTime(programMeeting);
                        SaveProgramMeetingData(programMeeting);
                    }
                }
            }
        }

        private void UpdateProgramMeetingEndTime(ProgramMeetingViewModel programMeeting)
        {
            if (programMeeting.Events.Any())
            {
                // Calculate end time based on the latest event end time
                var latestEndTime = programMeeting.BeginHour;
                foreach (var evt in programMeeting.Events)
                {
                    var eventEndTime = programMeeting.BeginHour.AddMinutes(evt.Duration);
                    if (eventEndTime > latestEndTime)
                    {
                        latestEndTime = eventEndTime;
                    }
                }
                programMeeting.EndHour = latestEndTime;
            }
            else
            {
                // Default 1 hour duration if no events
                programMeeting.EndHour = programMeeting.BeginHour.AddHours(1);
            }
        }

        private void SaveProgramMeetingData(ProgramMeetingViewModel programMeetingViewModel)
        {
            try
            {
                var allMeetings = _xmlService.GetProgramMeetings().ToList();
                var existingMeeting = allMeetings.FirstOrDefault(m => m.Id == programMeetingViewModel.Id);
                
                if (existingMeeting != null)
                {
                    // Update existing meeting
                    existingMeeting.EndHour = programMeetingViewModel.EndHour;
                    
                    // Clear existing slots and recreate them
                    existingMeeting.ProgramSlots.Clear();
                    
                    int slotId = 1;
                    foreach (var evt in programMeetingViewModel.Events)
                    {
                        var raceFormatDetail = _xmlService.GetRaceFormatConfigurations()
                            .SelectMany(config => config.RaceFormatDetails)
                            .FirstOrDefault(detail => detail.Id == evt.Id);

                        if (raceFormatDetail != null)
                        {
                            var programSlot = new ProgramSlot
                            {
                                Id = slotId++,
                                Name = evt.Title,
                                BeginHour = programMeetingViewModel.BeginHour,
                                EndHour = programMeetingViewModel.BeginHour.AddMinutes(evt.Duration),
                                RaceFormatDetailId = evt.Id,
                                RaceFormatDetail = raceFormatDetail,
                                ProgramMeetingId = existingMeeting.Id,
                                ProgramMeeting = existingMeeting,
                                ProgramRuns = new List<ProgramRun>()
                            };

                            // Find site name from description or current site
                            string siteName = "Default Site";
                            if (existingMeeting.Description != null && existingMeeting.Description.StartsWith("Site:"))
                            {
                                var parts = existingMeeting.Description.Split('|');
                                if (parts.Length >= 1)
                                {
                                    siteName = parts[0].Substring(5);
                                }
                            }

                            var programRun = new ProgramRun
                            {
                                Id = slotId,
                                Name = evt.Title,
                                Site = siteName,
                                Status = Data.EnumRSM.ProgramStatus.Pending,
                                BeginHour = programSlot.BeginHour,
                                EndHour = programSlot.EndHour,
                                ProgramSlotId = programSlot.Id,
                                ProgramSlot = programSlot
                            };

                            programSlot.ProgramRuns.Add(programRun);
                            existingMeeting.ProgramSlots.Add(programSlot);
                        }
                    }
                    
                    _xmlService.UpdateProgramMeetings(allMeetings);
                    _xmlService.Save();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving program meeting data");
            }
        }

        private void SavePlanningData()
        {
            try
            {
                var programMeetings = new List<ProgramMeeting>();
                int meetingId = 1;

                foreach (var site in Sites)
                {
                    string meetingName = $"{site.Name} - {site.Date:yyyy-MM-dd}";
                    var programMeeting = new ProgramMeeting
                    {
                        Id = meetingId++,
                        Name = meetingName,
                        Description = site.Description,
                        ProgramDate = site.Date,
                        BeginHour = site.Date.AddHours(7),
                        EndHour = site.Date.AddHours(20),
                        ProgramSlots = new List<ProgramSlot>()
                    };

                    int slotId = 1;
                    foreach (var timeSlot in site.TimeSlots.Where(ts => ts.Events.Any()))
                    {
                        foreach (var plannedEvent in timeSlot.Events)
                        {
                            var raceFormatDetail = _xmlService.GetRaceFormatConfigurations()
                                .SelectMany(config => config.RaceFormatDetails)
                                .FirstOrDefault(detail => detail.Id == plannedEvent.Id);

                            if (raceFormatDetail != null)
                            {
                                var programSlot = new ProgramSlot
                                {
                                    Id = slotId++,
                                    Name = plannedEvent.Title,
                                    BeginHour = timeSlot.DateTime,
                                    EndHour = timeSlot.DateTime.AddMinutes(plannedEvent.Duration),
                                    RaceFormatDetailId = raceFormatDetail.Id,
                                    RaceFormatDetail = raceFormatDetail,
                                    ProgramMeetingId = programMeeting.Id,
                                    ProgramMeeting = programMeeting,
                                    ProgramRuns = new List<ProgramRun>()
                                };

                                var programRun = new ProgramRun
                                {
                                    Id = slotId,
                                    Name = plannedEvent.Title,
                                    Site = site.Name,
                                    Status = Data.EnumRSM.ProgramStatus.Pending,
                                    BeginHour = timeSlot.DateTime,
                                    EndHour = timeSlot.DateTime.AddMinutes(plannedEvent.Duration),
                                    ProgramSlotId = programSlot.Id,
                                    ProgramSlot = programSlot
                                };

                                programSlot.ProgramRuns.Add(programRun);
                                programMeeting.ProgramSlots.Add(programSlot);
                            }
                            else
                            {
                                var programSlot = new ProgramSlot
                                {
                                    Id = slotId++,
                                    Name = plannedEvent.Title,
                                    BeginHour = timeSlot.DateTime,
                                    EndHour = timeSlot.DateTime.AddMinutes(plannedEvent.Duration),
                                    ProgramMeetingId = programMeeting.Id,
                                    ProgramMeeting = programMeeting,
                                    ProgramRuns = new List<ProgramRun>()
                                };


                                var programRun = new ProgramRun
                                {
                                    Id = slotId,
                                    Name = plannedEvent.Title,
                                    Site = site.Name,
                                    Status = Data.EnumRSM.ProgramStatus.Pending,
                                    BeginHour = timeSlot.DateTime,
                                    EndHour = timeSlot.DateTime.AddMinutes(plannedEvent.Duration),
                                    ProgramSlotId = programSlot.Id,
                                    ProgramSlot = programSlot
                                };


                                programSlot.ProgramRuns.Add(programRun);
                                programMeeting.ProgramSlots.Add(programSlot);
                            }
                        }
                    }

                    if (programMeeting.ProgramSlots.Any())
                    {
                        programMeetings.Add(programMeeting);
                    }
                }

                _xmlService.UpdateProgramMeetings(programMeetings);
                _xmlService.Save();
                _logger.LogInformation("Planning data saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving planning data");
            }
        }

        private void LoadPlanningData()
        {
            try
            {
                var programMeetings = _xmlService.GetProgramMeetings();
                
                // First, clear any existing program meetings from sites and time slots
                foreach (var siteList in _sitesByDate.Values)
                {
                    foreach (var site in siteList)
                    {
                        site.ProgramMeetings.Clear();
                        foreach (var timeSlot in site.TimeSlots)
                        {
                            timeSlot.ProgramMeeting = null;
                            timeSlot.IsFirstSlotOfMeeting = false;
                            timeSlot.IsLastSlotOfMeeting = false;
                            timeSlot.IsMiddleSlotOfMeeting = false;
                        }
                    }
                }
                
                foreach (var programMeeting in programMeetings)
                {
                    var meetingDate = programMeeting.ProgramDate.Date;
                    
                    if (_sitesByDate.TryGetValue(meetingDate, out var dailySites))
                    {
                        // Find the appropriate site for this program meeting
                        string? targetSiteName = null;
                        SiteViewModel? targetSite = null;
                        
                        // First try to get site from program runs (for meetings with content)
                        targetSiteName = programMeeting.ProgramSlots.FirstOrDefault()?.ProgramRuns.FirstOrDefault()?.Site;
                        if (!string.IsNullOrEmpty(targetSiteName))
                        {
                            targetSite = dailySites.FirstOrDefault(s => s.Name == targetSiteName);
                        }
                        
                        // If no site found and it's a new meeting, try to parse from description
                        if (targetSite == null && programMeeting.Description != null && programMeeting.Description.StartsWith("Site:"))
                        {
                            var parts = programMeeting.Description.Split('|');
                            if (parts.Length >= 2)
                            {
                                targetSiteName = parts[0].Substring(5); // Remove "Site:" prefix
                                if (int.TryParse(parts[1], out int siteId))
                                {
                                    targetSite = dailySites.FirstOrDefault(s => s.Id == siteId);
                                }
                                if (targetSite == null)
                                {
                                    targetSite = dailySites.FirstOrDefault(s => s.Name == targetSiteName);
                                }
                            }
                        }
                        
                        if (targetSite != null)
                        {
                            // Create ProgramMeetingViewModel
                            var programMeetingViewModel = new ProgramMeetingViewModel
                            {
                                Id = programMeeting.Id,
                                Name = programMeeting.Name,
                                Description = programMeeting.Description ?? "",
                                BeginHour = programMeeting.BeginHour,
                                EndHour = programMeeting.EndHour,
                                SiteColor = targetSite.Color
                            };
                            
                            // Find the corresponding time slots for the entire duration
                            var startTimeSlot = targetSite.TimeSlots.FirstOrDefault(ts => 
                                Math.Abs((ts.DateTime - programMeeting.BeginHour).TotalMinutes) < 7.5);
                            var endTimeSlot = targetSite.TimeSlots.FirstOrDefault(ts => 
                                Math.Abs((ts.DateTime - programMeeting.EndHour).TotalMinutes) < 7.5);
                            
                            if (startTimeSlot != null)
                            {
                                int startIndex = targetSite.TimeSlots.IndexOf(startTimeSlot);
                                int endIndex = endTimeSlot != null ? targetSite.TimeSlots.IndexOf(endTimeSlot) : startIndex;
                                
                                programMeetingViewModel.StartSlotIndex = startIndex;
                                
                                // Mark all slots in the range as part of this meeting
                                for (int i = startIndex; i <= endIndex && i < targetSite.TimeSlots.Count; i++)
                                {
                                    var slot = targetSite.TimeSlots[i];
                                    slot.ProgramMeeting = programMeetingViewModel;
                                    
                                    if (startIndex == endIndex)
                                    {
                                        // Single slot meeting: both first and last
                                        slot.IsFirstSlotOfMeeting = true;
                                        slot.IsLastSlotOfMeeting = true;
                                    }
                                    else if (i == startIndex)
                                    {
                                        // First slot: contains the meeting object and header
                                        slot.IsFirstSlotOfMeeting = true;
                                    }
                                    else if (i == endIndex)
                                    {
                                        // Last slot: only mark as last, not middle
                                        slot.IsLastSlotOfMeeting = true;
                                    }
                                    else
                                    {
                                        // Middle slots: just part of the meeting
                                        slot.IsMiddleSlotOfMeeting = true;
                                    }
                                }
                            }
                            
                            // Add events to the program meeting
                            foreach (var programSlot in programMeeting.ProgramSlots)
                            {
                                foreach (var programRun in programSlot.ProgramRuns)
                                {
                                    var plannedEvent = new PlannedEventViewModel
                                    {
                                        Id = programSlot.RaceFormatDetailId,
                                        Title = programRun.Name,
                                        Subtitle = $"Site: {programRun.Site}",
                                        Color = targetSite.Color,
                                        Duration = (int)(programRun.EndHour - programRun.BeginHour).TotalMinutes,
                                        ConfigurationLabel = programSlot.RaceFormatDetail?.Label ?? "",
                                        StatusColor = "#10B981",
                                        ParticipantCount = GetParticipantCountNullable(programSlot.RaceFormatDetail)
                                    };

                                    programMeetingViewModel.Events.Add(plannedEvent);
                                    
                                    // Remove from events to plan
                                    var eventToRemove = EventsToplan.FirstOrDefault(e => e.Id == programSlot.RaceFormatDetailId);
                                    if (eventToRemove != null)
                                    {
                                        EventsToplan.Remove(eventToRemove);
                                    }
                                }
                            }
                            
                            targetSite.ProgramMeetings.Add(programMeetingViewModel);
                        }
                    }
                }
                
                // Update sites for current date to reflect changes
                UpdateSitesForCurrentDate();

                FilterEvents();
                UpdateStatistics();
                _logger.LogInformation("Planning data loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading planning data");
            }
        }

        private void OnSave()
        {
            try
            {
                _logger.LogInformation("Saving planning configuration");
                SavePlanningData();
                _xmlService.Save();
                _dialogService.ShowMessage("Sauvegarder", "Configuration sauvegardée avec succès!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving planning configuration");
                _dialogService.ShowMessage("Erreur", "Erreur lors de la sauvegarde");
            }
        }

        private void OnExport()
        {
            try
            {
                _logger.LogInformation("Exporting planning");
                _dialogService.ShowMessage("Exporter", "Planning exporté avec succès!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting planning");
            }
        }

        private void OnPreviousDay()
        {
            var competition = _xmlService.GetCompetition();
            if (competition != null && CurrentDate > competition.BeginDate)
            {
                CurrentDate = CurrentDate.AddDays(-1);
                UpdateCurrentDateDisplay();
                UpdateSitesForCurrentDate();
            }
        }

        private void OnNextDay()
        {
            var competition = _xmlService.GetCompetition();
            if (competition != null && CurrentDate < competition.EndDate)
            {
                CurrentDate = CurrentDate.AddDays(1);
                UpdateCurrentDateDisplay();
                UpdateSitesForCurrentDate();
            }
        }

        private void OnAutoPlan()
        {
            try
            {
                _logger.LogInformation("Auto-planning events for current day");
                
                var unplannedEvents = EventsToplan.Where(e => !e.IsPlanned).OrderBy(e => e.Duration).ToList();
                int plannedCount = 0;

                foreach (var evt in unplannedEvents)
                {
                    int requiredSlots = GetRequiredSlotCount(evt.Duration);
                    bool placed = false;

                    // Try to place the event in each site
                    foreach (var site in Sites)
                    {
                        if (placed)
                        {
                            break;
                        }

                        // Try each starting slot in the site
                        for (int i = 0; i <= site.TimeSlots.Count - requiredSlots; i++)
                        {
                            var startingSlot = site.TimeSlots[i];
                            var availableSlots = GetConsecutiveAvailableSlots(site, startingSlot, requiredSlots);

                            if (availableSlots.Count == requiredSlots)
                            {
                                MoveEventToTimeSlot(evt, startingSlot);
                                plannedCount++;
                                placed = true;
                                break;
                            }
                        }
                    }
                }

                _dialogService.ShowMessage("Auto-planification", $"Planification automatique terminée! {plannedCount} épreuves planifiées.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during auto-planning");
            }
        }

        private void OnAddSite()
        {
            try
            {
                SiteCreationDialog dialog = new SiteCreationDialog();
                bool? result = dialog.ShowDialog();
                
                if (result == true && dialog.DataContext is SiteCreationDialogViewModel viewModel && viewModel.CreatedSite != null)
                {
                    // Add the new site to the XML collection
                    var sites = _xmlService.GetSites().ToList();
                    sites.Add(viewModel.CreatedSite);
                    _xmlService.UpdateSites(sites);
                    
                    // Add the new site to all competition dates
                    var competition = _xmlService.GetCompetition();
                    if (competition != null)
                    {
                        int colorIndex = sites.Count - 1; // Use index for consistent coloring
                        
                        for (var date = competition.BeginDate.Date; date <= competition.EndDate.Date; date = date.AddDays(1))
                        {
                            if (!_sitesByDate.ContainsKey(date))
                            {
                                _sitesByDate[date] = new List<SiteViewModel>();
                            }
                            
                            var newSiteViewModel = new SiteViewModel
                            {
                                Id = viewModel.CreatedSite.Id,
                                Name = viewModel.CreatedSite.Name,
                                Description = viewModel.CreatedSite.Description ?? string.Empty,
                                Icon = viewModel.CreatedSite.Icon,
                                Color = GetSiteColor(colorIndex),
                                Date = date,
                                TimeSlots = CreateTimeSlots(date, GetSiteColor(colorIndex))
                            };
                            
                            _sitesByDate[date].Add(newSiteViewModel);
                        }
                    }
                    
                    // Update current day's sites in the UI
                    UpdateSitesForCurrentDate();
                    
                    // Save changes
                    _xmlService.Save();
                    
                    _logger.LogInformation($"New site created: {viewModel.CreatedSite.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new site");
                _dialogService.ShowMessage("Erreur", "Erreur lors de la création du site");
            }
        }

        private void OnRemoveSite(SiteViewModel? siteViewModel)
        {
            if (siteViewModel == null)
            {
                return;
            }

            try
            {
                bool confirmed = _dialogService.ShowConfirmation("Supprimer le site", 
                    $"Êtes-vous sûr de vouloir supprimer le site '{siteViewModel.Name}' de tous les jours de la compétition ?");

                if (confirmed)
                {
                    // Remove from XML service
                    var sites = _xmlService.GetSites().ToList();
                    var siteToRemove = sites.FirstOrDefault(s => s.Id == siteViewModel.Id);
                    if (siteToRemove != null)
                    {
                        sites.Remove(siteToRemove);
                        _xmlService.UpdateSites(sites);
                        _xmlService.Save();
                    }

                    // Remove from all dates in the dictionary
                    var datesToUpdate = _sitesByDate.Keys.ToList();
                    foreach (var date in datesToUpdate)
                    {
                        var dailySites = _sitesByDate[date];
                        var siteToRemoveFromDay = dailySites.FirstOrDefault(s => s.Id == siteViewModel.Id);
                        if (siteToRemoveFromDay != null)
                        {
                            dailySites.Remove(siteToRemoveFromDay);
                        }
                    }

                    // Update current day's sites in the UI
                    UpdateSitesForCurrentDate();

                    _logger.LogInformation($"Site removed: {siteViewModel.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing site");
                _dialogService.ShowMessage("Erreur", "Erreur lors de la suppression du site");
            }
        }

        private void OnCreateManualTimeSlot()
        {
            try
            {
                var dialog = new ManualTimeSlotDialog();
                var dialogViewModel = new ManualTimeSlotDialogViewModel(_xmlService, CurrentDate);
                dialog.DataContext = dialogViewModel;

                bool? result = dialog.ShowDialog();

                if (result == true && dialogViewModel.CreatedEvent != null)
                {
                    var selectedSites = dialogViewModel.GetSelectedSites();
                    var selectedDateTime = dialogViewModel.GetSelectedDateTime();

                    foreach (var selectedSite in selectedSites)
                    {
                        // Find the corresponding site for the selected date
                        var targetSite = Sites.FirstOrDefault(s => s.Id == selectedSite.Id && s.Date.Date == selectedDateTime.Date);
                        if (targetSite != null)
                        {
                            // Find the appropriate timeslot
                            var targetTimeSlot = targetSite.TimeSlots.FirstOrDefault(ts => 
                                ts.DateTime.TimeOfDay <= selectedDateTime.TimeOfDay &&
                                ts.DateTime.TimeOfDay.Add(TimeSpan.FromMinutes(10)) > selectedDateTime.TimeOfDay);

                            if (targetTimeSlot != null)
                            {
                                // Create a copy of the event with the site's color
                                var siteSpecificEvent = new PlannedEventViewModel
                                {
                                    Id = dialogViewModel.CreatedEvent.Id,
                                    Title = dialogViewModel.CreatedEvent.Title,
                                    Subtitle = dialogViewModel.CreatedEvent.Subtitle,
                                    Color = targetSite.Color,
                                    Duration = dialogViewModel.CreatedEvent.Duration,
                                    ConfigurationLabel = dialogViewModel.CreatedEvent.ConfigurationLabel,
                                    StatusColor = dialogViewModel.CreatedEvent.StatusColor,
                                    ParticipantCount = dialogViewModel.CreatedEvent.ParticipantCount
                                };
                                
                                targetTimeSlot.Events.Add(siteSpecificEvent);
                            }
                        }
                    }

                    UpdateStatistics();
                    SavePlanningData();
                    _logger.LogInformation($"Manual timeslot created: {dialogViewModel.CreatedEvent.Title}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual timeslot");
                _dialogService.ShowMessage("Erreur", "Erreur lors de la création du créneau manuel");
            }
        }

        private void OnCreateProgramMeeting()
        {
            try
            {
                var dialog = new ProgramMeetingCreationDialog(CurrentDate, Sites);
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.DataContext is ProgramMeetingCreationDialogViewModel viewModel && viewModel.CreatedProgramMeeting != null)
                {
                    // Refresh the data to show the new ProgramMeeting
                    LoadPlanningData();
                    UpdateStatistics();
                    
                    // Update sites for current date to show changes immediately
                    UpdateSitesForCurrentDate();
                    
                    _logger.LogInformation($"ProgramMeeting created: {viewModel.CreatedProgramMeeting.Name}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ProgramMeeting");
                _dialogService.ShowMessage("Erreur", "Erreur lors de la création du groupe de programme");
            }
        }
    }

    public class PlanningEventViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ConfigurationLabel { get; set; } = string.Empty;
        public string ConfigurationColor { get; set; } = "#6B7280";
        public string StatusColor { get; set; } = "#6B7280";
        public int ParticipantCount { get; set; }
        public int Duration { get; set; }
        public bool IsPlanned { get; set; }
    }

    public class SiteViewModel
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "#6B7280";
        public DateTime Date { get; set; }
        public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
        public ObservableCollection<ProgramMeetingViewModel> ProgramMeetings { get; set; } = new();
    }

    public class ProgramMeetingViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime BeginHour { get; set; }
        public DateTime EndHour { get; set; }
        public string TimeRange => $"{BeginHour:HH:mm} - {EndHour:HH:mm}";
        public string SiteColor { get; set; } = "#6B7280";
        public ObservableCollection<PlannedEventViewModel> Events { get; set; } = new();
        public int StartSlotIndex { get; set; }
        public int SlotCount => (int)Math.Ceiling((EndHour - BeginHour).TotalMinutes / 10.0);
        
        // Visual properties for grouping
        public bool IsExpanded { get; set; } = true;
        public string GroupColor => SiteColor;
        public string BorderColor => SiteColor;
    }

    public class TimeSlotViewModel
    {
        public string Time { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string SiteColor { get; set; } = "#F9FAFB";
        public ObservableCollection<PlannedEventViewModel> Events { get; set; } = new();
        public ProgramMeetingViewModel? ProgramMeeting { get; set; }
        public bool HasProgramMeeting => ProgramMeeting != null;
        public bool IsFirstSlotOfMeeting { get; set; }
        public bool IsLastSlotOfMeeting { get; set; }
        public bool IsMiddleSlotOfMeeting { get; set; }
        public bool IsPartOfMeeting => HasProgramMeeting || IsMiddleSlotOfMeeting;
    }

    public class PlannedEventViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Color { get; set; } = "#6B7280";
        public int Duration { get; set; }
        public string ConfigurationLabel { get; set; } = string.Empty;
        public string StatusColor { get; set; } = "#6B7280";
        public int ParticipantCount { get; set; }
    }
}
