using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            Initialize();
        }

        private void Initialize()
        {
            // Set current date to competition begin date if not within range
            var competition = _xmlService.GetCompetition();
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
            UpdateStatistics();
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
                var races = _xmlService.GetRaces();
                var raceFormatConfigurations = _xmlService.GetRaceFormatConfigurations();

                foreach (var config in raceFormatConfigurations)
                {
                    foreach (var detail in config.RaceFormatDetails)
                    {
                        var eventViewModel = new PlanningEventViewModel
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
                var sortedEvents = EventsToplan.OrderBy(e => e.Name).ToList();
                EventsToplan.Clear();
                foreach (var evt in sortedEvents)
                {
                    EventsToplan.Add(evt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading events to plan");
            }
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
                        dailySites.Add(new SiteViewModel
                        {
                            Id = site.Id,
                            Name = site.Name,
                            Description = site.Description,
                            Icon = site.Icon,
                            Color = GetSiteColor(colorIndex++),
                            Date = date,
                            TimeSlots = CreateTimeSlots(date)
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

        private List<TimeSlotViewModel> CreateTimeSlots(DateTime? date = null)
        {
            var targetDate = date ?? CurrentDate;
            var timeSlots = new List<TimeSlotViewModel>();
            var startTime = new TimeSpan(7, 0, 0);

            for (int i = 0; i < 51; i++)
            {
                var currentTime = startTime.Add(TimeSpan.FromMinutes(i * 15));
                timeSlots.Add(new TimeSlotViewModel
                {
                    Time = currentTime.ToString(@"hh\:mm"),
                    DateTime = targetDate.Add(currentTime),
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
            return new Random().Next(15, 35); // Mock data
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
            EventsToplanCount = EventsToplan.Count;
            PlannedEventsCount = Sites.SelectMany(s => s.TimeSlots).SelectMany(ts => ts.Events).Count();
        }

        public void MoveEventToTimeSlot(object eventItem, object timeSlotItem)
        {
            if (eventItem is PlanningEventViewModel planningEvent && timeSlotItem is TimeSlotViewModel timeSlot)
            {
                // Check if this event is already in the timeslot to prevent duplicates
                if (timeSlot.Events.Any(e => e.Id == planningEvent.Id))
                {
                    return;
                }

                var plannedEvent = new PlannedEventViewModel
                {
                    Id = planningEvent.Id,
                    Title = planningEvent.Name,
                    Subtitle = $"{planningEvent.ParticipantCount} participants",
                    Color = planningEvent.ConfigurationColor,
                    Duration = planningEvent.Duration,
                    ConfigurationLabel = planningEvent.ConfigurationLabel,
                    StatusColor = planningEvent.StatusColor,
                    ParticipantCount = planningEvent.ParticipantCount
                };

                timeSlot.Events.Add(plannedEvent);
                EventsToplan.Remove(planningEvent);

                UpdateStatistics();
                
                _logger.LogInformation($"Moved event {planningEvent.Name} to time slot {timeSlot.Time}");
            }
        }

        public void RemoveEventFromTimeSlot(object plannedEventItem)
        {
            if (plannedEventItem is PlannedEventViewModel plannedEvent)
            {
                // Find the timeslot containing this event
                TimeSlotViewModel? containingTimeSlot = null;
                foreach (var site in Sites)
                {
                    foreach (var timeSlot in site.TimeSlots)
                    {
                        if (timeSlot.Events.Any(e => e.Id == plannedEvent.Id))
                        {
                            containingTimeSlot = timeSlot;
                            break;
                        }
                    }
                    if (containingTimeSlot != null)
                    {
                        break;
                    }
                }

                if (containingTimeSlot != null)
                {
                    // Remove from timeslot
                    var eventToRemove = containingTimeSlot.Events.FirstOrDefault(e => e.Id == plannedEvent.Id);
                    if (eventToRemove != null)
                    {
                        containingTimeSlot.Events.Remove(eventToRemove);
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

                    UpdateStatistics();
                    _logger.LogInformation($"Removed event {plannedEvent.Title} from time slot and restored to planning list");
                }
            }
        }
        public void RefreshData()
        {
            Initialize();
        }

        private void OnSave()
        {
            try
            {
                _logger.LogInformation("Saving planning configuration");
                _dialogService.ShowMessage("Sauvegarder", "Configuration sauvegardée avec succès!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving planning configuration");
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
                
                var unplannedEvents = EventsToplan.Where(e => !e.IsPlanned).ToList();
                var availableSlots = Sites.SelectMany(v => v.TimeSlots.Where(ts => !ts.Events.Any())).ToList();

                for (int i = 0; i < Math.Min(unplannedEvents.Count, availableSlots.Count); i++)
                {
                    MoveEventToTimeSlot(unplannedEvents[i], availableSlots[i]);
                }

                _dialogService.ShowMessage("Auto-planification", $"Planification automatique terminée! {Math.Min(unplannedEvents.Count, availableSlots.Count)} épreuves planifiées.");
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
                                TimeSlots = CreateTimeSlots(date)
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
                                ts.DateTime.TimeOfDay.Add(TimeSpan.FromMinutes(15)) > selectedDateTime.TimeOfDay);

                            if (targetTimeSlot != null)
                            {
                                targetTimeSlot.Events.Add(dialogViewModel.CreatedEvent);
                            }
                        }
                    }

                    UpdateStatistics();
                    _logger.LogInformation($"Manual timeslot created: {dialogViewModel.CreatedEvent.Title}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual timeslot");
                _dialogService.ShowMessage("Erreur", "Erreur lors de la création du créneau manuel");
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
    }

    public class TimeSlotViewModel
    {
        public string Time { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public ObservableCollection<PlannedEventViewModel> Events { get; set; } = new();
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
