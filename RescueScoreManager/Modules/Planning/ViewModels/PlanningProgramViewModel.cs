using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

using RescueScoreManager.Data;
using RescueScoreManager.Services;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Modules.Planning.ViewModels
{
    public partial class PlanningProgramViewModel : ObservableObject
    {
        private readonly IXMLService _xmlService;
        private readonly ILocalizationService _localizationService;
        private readonly IDialogService _dialogService;
        private readonly IProgramService _programService;
        private readonly IProgramValidationService _validationService;
        private readonly ILogger<PlanningProgramViewModel> _logger;

        #region Observable Properties

        [ObservableProperty]
        private Program? _currentProgram;

        private Competition? _currentCompetition;

        public Competition? CurrentCompetition
        {
            get => _currentCompetition;
            set
            {
                SetProperty(ref _currentCompetition, value);
                RefreshCommandStates();
            }
        }

        [ObservableProperty]
        private ObservableCollection<RaceFormatDetail> _availableRaceFormats = new();

        // Keep track of all race formats (original list before filtering)
        private List<RaceFormatDetail> _allRaceFormats = new();

        [ObservableProperty]
        private ObservableCollection<Site> _availableSites = new();

        [ObservableProperty]
        private ObservableCollection<ProgramMeeting> _programMeetings = new();

        [ObservableProperty]
        private ProgramValidationResult _validationResult = new();

        private DateTime _selectedDate = DateTime.Today;

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                // Validate date range against competition dates
                if (CurrentCompetition != null)
                {
                    if (value < CurrentCompetition.BeginDate.Date)
                    {
                        value = CurrentCompetition.BeginDate.Date;
                        _logger.LogWarning("Selected date adjusted to competition begin date: {Date}", value);
                    }
                    else if (value > CurrentCompetition.EndDate.Date)
                    {
                        value = CurrentCompetition.EndDate.Date;
                        _logger.LogWarning("Selected date adjusted to competition end date: {Date}", value);
                    }
                }

                SetProperty(ref _selectedDate, value);
                RefreshCommandStates();
                OnPropertyChanged(nameof(SitesForSelectedDate));
            }
        }

        [ObservableProperty]
        private string _selectedViewMode = "By Time";

        [ObservableProperty]
        private ProgramSlot? _selectedEvent;

        [ObservableProperty]
        private bool _canPublish = false;

        [ObservableProperty]
        private string _validationSummary = "No validation performed";

        [ObservableProperty]
        private Brush _validationStatusColor = Brushes.Gray;

        [ObservableProperty]
        private Brush _statusColor = Brushes.Blue;

        [ObservableProperty]
        private int _eventCount = 0;

        [ObservableProperty]
        private int _conflictCount = 0;

        [ObservableProperty]
        private DateTime _lastValidationTime = DateTime.Now;

        [ObservableProperty]
        private string _autoSaveStatus = "Auto-save enabled";

        #endregion

        #region Computed Properties

        /// <summary>
        /// Gets the available sites with only the program meetings for the selected date
        /// </summary>
        public IEnumerable<Site> SitesForSelectedDate
        {
            get
            {
                return AvailableSites.Select(site =>
                {
                    var clonedSite = new Site(site.Id, site.Name, site.Description, site.Icon);
                    clonedSite.ProgramMeetings = site.ProgramMeetings
                        .Where(meeting => meeting.ProgramDate.Date == SelectedDate.Date)
                        .ToList();
                    return clonedSite;
                }).Where(site => site.ProgramMeetings.Any()); // Only show sites that have meetings for the selected date
            }
        }

        #endregion

        #region Commands

        public ICommand AddSiteCommand { get; }
        public ICommand AddMeetingCommand { get; }
        public ICommand ValidateCommand { get; }
        public ICommand PublishCommand { get; }
        public ICommand PreviousDayCommand { get; }
        public ICommand NextDayCommand { get; }
        public ICommand TodayCommand { get; }
        public ICommand AddManualEventCommand { get; }
        public ICommand AutoArrangeCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand AddManualSlotCommand { get; }
        public ICommand DeleteSlotCommand { get; }

        #endregion

        public PlanningProgramViewModel(
            IXMLService xmlService,
            ILocalizationService localizationService,
            IDialogService dialogService,
            IProgramService programService,
            IProgramValidationService validationService,
            ILogger<PlanningProgramViewModel> logger)
        {
            _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _programService = programService ?? throw new ArgumentNullException(nameof(programService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize commands
            AddSiteCommand = new RelayCommand(async () => await AddSiteAsync());
            AddMeetingCommand = new RelayCommand(async () => await AddMeetingAsync());
            ValidateCommand = new RelayCommand(async () => await ValidateScheduleAsync());
            PublishCommand = new RelayCommand(async () => await PublishProgramAsync(), () => CanPublish);
            PreviousDayCommand = new RelayCommand(() => SelectedDate = SelectedDate.AddDays(-1), CanGoToPreviousDay);
            NextDayCommand = new RelayCommand(() => SelectedDate = SelectedDate.AddDays(1), CanGoToNextDay);
            TodayCommand = new RelayCommand(() => SelectedDate = DateTime.Today, CanGoToToday);
            AddManualEventCommand = new RelayCommand(async () => await AddManualEventAsync());
            AutoArrangeCommand = new RelayCommand(async () => await AutoArrangeEventsAsync());
            ExportCommand = new RelayCommand(async () => await ExportScheduleAsync());
            AddManualSlotCommand = new RelayCommand<ProgramMeeting>(async (meeting) => await AddManualSlotAsync(meeting));
            DeleteSlotCommand = new RelayCommand<ProgramSlot>(async (slot) => await DeleteSlotAsync(slot));

            Initialize();
        }

        #region Initialization

        private async void Initialize()
        {
            try
            {
                _logger.LogInformation("Initializing PlanningProgram module");

                await LoadCurrentCompetitionAsync();
                await LoadCurrentProgramAsync();
                await LoadAvailableRaceFormatsAsync();
                await LoadAvailableSitesAsync();
                await LoadProgramMeetingsAsync();
                await ValidateScheduleAsync();

                _logger.LogInformation("PlanningProgram module initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing PlanningProgram module");
            }
        }

        private async Task LoadCurrentCompetitionAsync()
        {
            try
            {
                CurrentCompetition = await Task.Run(() => _xmlService.GetCompetition());
                if (CurrentCompetition != null)
                {
                    // Set initial SelectedDate to competition begin date if current date is outside range
                    var today = DateTime.Today;
                    if (today < CurrentCompetition.BeginDate.Date || today > CurrentCompetition.EndDate.Date)
                    {
                        SelectedDate = CurrentCompetition.BeginDate.Date;
                    }
                    else
                    {
                        SelectedDate = today;
                    }

                    _logger.LogDebug("Loaded current competition: {CompetitionName} ({BeginDate} - {EndDate})",
                        CurrentCompetition.Name,
                        CurrentCompetition.BeginDate.ToShortDateString(),
                        CurrentCompetition.EndDate.ToShortDateString());
                }
                else
                {
                    _logger.LogWarning("No current competition found");
                    SelectedDate = DateTime.Today;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading current competition");
                SelectedDate = DateTime.Today;
            }
        }

        private async Task LoadCurrentProgramAsync()
        {
            try
            {
                CurrentProgram = await _programService.GetCurrentProgramAsync();
                if (CurrentProgram == null)
                {
                    // Create a new program if none exists
                    CurrentProgram = await _programService.CreateProgramAsync("New Competition Schedule");
                    await _programService.SetCurrentProgramAsync(CurrentProgram);
                }

                UpdateStatusIndicators();
                _logger.LogDebug("Loaded current program: {ProgramId}", CurrentProgram?.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading current program");
            }
        }

        private async Task LoadAvailableRaceFormatsAsync()
        {
            try
            {
                // Get data from services (can be done on background thread)
                var raceFormatConfigurations = await Task.Run(() => _xmlService.GetRaceFormatConfigurations());
                _logger.LogDebug("Found {ConfigCount} race format configurations", raceFormatConfigurations.Count);

                var raceFormats = await Task.Run(() => raceFormatConfigurations
                    .SelectMany(config => config.RaceFormatDetails)
                    .OrderBy(detail => detail.RaceFormatConfiguration.Discipline)
                    .ThenBy(detail => detail.RaceFormatConfiguration.DisciplineLabel)
                    .ThenBy(detail => detail.Order)
                    .ToList());

                _logger.LogDebug("Found {DetailCount} race format details", raceFormats.Count);

                // Store all race formats and update UI collection on UI thread
                _allRaceFormats = raceFormats;
                AvailableRaceFormats.Clear();
                foreach (var format in raceFormats)
                {
                    AvailableRaceFormats.Add(format);
                    _logger.LogDebug("Added race format: {Label} from discipline {Discipline}",
                        format.Label, format.RaceFormatConfiguration.DisciplineLabel);
                }

                // If no race formats found, create some sample data for testing
                if (raceFormats.Count == 0)
                {
                    _logger.LogWarning("No race format configurations found, creating sample data for testing");
                    CreateSampleRaceFormats();
                }

                _logger.LogInformation("Loaded {Count} available race formats sorted by discipline", AvailableRaceFormats.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available race formats");
            }
        }

        private async Task LoadAvailableSitesAsync()
        {
            try
            {
                var sites = await Task.Run(() => _xmlService.GetSites().ToList());

                // Update UI collection on UI thread
                AvailableSites.Clear();
                foreach (var site in sites)
                {
                    AvailableSites.Add(site);
                }

                _logger.LogDebug("Loaded {Count} available sites", AvailableSites.Count);
                OnPropertyChanged(nameof(SitesForSelectedDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available sites");
            }
        }

        private async Task LoadProgramMeetingsAsync()
        {
            try
            {
                if (CurrentProgram == null)
                {
                    return;
                }

                var meetings = await _programService.GetProgramMeetingsAsync(CurrentProgram.Id);

                // Update UI collection on UI thread
                ProgramMeetings.Clear();
                foreach (var meeting in meetings)
                {
                    ProgramMeetings.Add(meeting);
                }

                UpdateEventCount();
                OnPropertyChanged(nameof(SitesForSelectedDate));

                // Refresh available race formats to exclude used ones
                RefreshAvailableRaceFormats();

                _logger.LogDebug("Loaded {Count} program meetings", ProgramMeetings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading program meetings");
            }
        }

        #endregion

        #region Command Implementations

        private async Task AddSiteAsync()
        {
            try
            {
                var dialog = new Views.SiteCreationDialog();
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.DataContext is ViewModels.SiteCreationDialogViewModel viewModel && viewModel.CreatedSite != null)
                {
                    var newSite = viewModel.CreatedSite;

                    // Save to XML
                    var sites = _xmlService.GetSites().ToList();
                    sites.Add(newSite);
                    _xmlService.UpdateSites(sites);
                    _xmlService.Save();

                    // Add to available sites collection
                    AvailableSites.Add(newSite);
                    OnPropertyChanged(nameof(SitesForSelectedDate));
                    _logger.LogInformation("Added new site: {SiteName}", newSite.Name);
                }
                else
                {
                    _logger.LogDebug("Site creation cancelled by user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new site");
            }
        }

        private async Task AddMeetingAsync()
        {
            try
            {
                if (CurrentProgram == null)
                {
                    _logger.LogWarning("Cannot add meeting: CurrentProgram is null");
                    return;
                }

                // Open the ProgramMeetingCreationDialog
                var dialog = new Views.ProgramMeetingCreationDialog(SelectedDate, AvailableSites);
                bool? result = dialog.ShowDialog();

                if (result == true && dialog.DataContext is ViewModels.ProgramMeetingCreationDialogViewModel viewModel && viewModel.CreatedProgramMeeting != null)
                {
                    var newMeeting = viewModel.CreatedProgramMeeting;

                    // Set the program reference properly
                    newMeeting.ProgramId = CurrentProgram.Id;
                    newMeeting.Program = CurrentProgram;

                    // Find the site for this meeting from description
                    string? targetSiteId = newMeeting.Description?.Split('|').LastOrDefault();
                    var targetSite = AvailableSites.FirstOrDefault(s => s.Id.ToString() == targetSiteId);
                    if (targetSite != null)
                    {
                        // Add the meeting to the site's meetings collection
                        if (!targetSite.ProgramMeetings.Any(m => m.Id == newMeeting.Id))
                        {
                            targetSite.ProgramMeetings.Add(newMeeting);
                        }

                        // Update the program's sites collection
                        var sites = _xmlService.GetSites().ToList();
                        int siteIndex = sites.FindIndex(s => s.Id == targetSite.Id);
                        if (siteIndex >= 0)
                        {
                            sites[siteIndex] = targetSite;
                        }
                        else
                        {
                            sites.Add(targetSite);
                        }

                        // Save the updated sites and program to XML
                        _xmlService.UpdateSites(sites);
                        _xmlService.Save();

                        // Update the UI collections
                        ProgramMeetings.Add(newMeeting);
                        OnPropertyChanged(nameof(SitesForSelectedDate));
                        UpdateEventCount();
                        await ValidateScheduleAsync();

                        _logger.LogInformation("Added new meeting: {MeetingName} to site: {SiteName}", newMeeting.Name, targetSite.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Could not find target site for meeting: {MeetingName}", newMeeting.Name);
                    }
                }
                else
                {
                    _logger.LogDebug("Meeting creation cancelled by user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new meeting");
            }
        }

        private async Task ValidateScheduleAsync()
        {
            try
            {
                if (CurrentProgram == null)
                { return; }

                ValidationResult = await _validationService.ValidateProgramAsync(CurrentProgram);
                LastValidationTime = DateTime.Now;

                ConflictCount = ValidationResult.Conflicts.Count;
                CanPublish = await _validationService.CanPublishProgramAsync(CurrentProgram);
                ValidationSummary = await _validationService.GetPublishValidationMessageAsync(CurrentProgram);

                ValidationStatusColor = ValidationResult.IsValid ? Brushes.Green : Brushes.Red;

                _logger.LogInformation("Schedule validation completed: Valid={IsValid}, Conflicts={ConflictCount}",
                    ValidationResult.IsValid, ConflictCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating schedule");
                ValidationSummary = "Validation error occurred";
                ValidationStatusColor = Brushes.Red;
            }
        }

        private async Task PublishProgramAsync()
        {
            try
            {
                if (CurrentProgram == null)
                { return; }

                // Validate before publishing
                await ValidateScheduleAsync();

                if (!CanPublish)
                {
                    _logger.LogWarning("Cannot publish program due to validation issues");
                    return;
                }

                CurrentProgram = await _programService.PublishProgramAsync(CurrentProgram.Id);
                UpdateStatusIndicators();

                _logger.LogInformation("Program published successfully: {ProgramId} - Version {Version}",
                    CurrentProgram.Id, CurrentProgram.Version);

                // Show success message (this would typically be a dialog)
                ValidationSummary = "Program published successfully!";
                ValidationStatusColor = Brushes.Green;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing program");
                ValidationSummary = "Error occurred during publishing";
                ValidationStatusColor = Brushes.Red;
            }
        }

        private async Task AddManualEventAsync()
        {
            try
            {
                var firstMeeting = ProgramMeetings.FirstOrDefault();
                if (firstMeeting == null)
                {
                    _logger.LogWarning("No meetings available to add manual event");
                    return;
                }

                var manualSlot = await _programService.CreateManualEventAsync(
                    meetingId: firstMeeting.Id,
                    name: "Manual Event",
                    beginHour: DateTime.Now.Date.AddHours(12), // Noon
                    durationMinutes: 30,
                    eventType: "Break"
                );

                firstMeeting.ProgramSlots.Add(manualSlot);
                UpdateEventCount();
                await ValidateScheduleAsync();

                _logger.LogInformation("Added manual event: {EventName}", manualSlot.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding manual event");
            }
        }

        private async Task AutoArrangeEventsAsync()
        {
            try
            {
                // This would implement automatic event arrangement logic
                _logger.LogInformation("Auto-arrange events feature not yet implemented");
                await Task.Delay(100); // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-arranging events");
            }
        }

        private async Task ExportScheduleAsync()
        {
            try
            {
                // This would implement schedule export functionality
                _logger.LogInformation("Export schedule feature not yet implemented");
                await Task.Delay(100); // Placeholder
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting schedule");
            }
        }

        private async Task AddManualSlotAsync(ProgramMeeting? programMeeting)
        {
            try
            {
                if (programMeeting == null)
                {
                    _logger.LogWarning("Cannot add manual slot: ProgramMeeting is null");
                    return;
                }

                // Open the ManualTimeSlotDialog
                var dialog = new Views.ManualTimeSlotDialog();

                // Initialize with current context
                var viewModel = new ManualTimeSlotDialogViewModel(_xmlService, programMeeting.ProgramDate);
                dialog.DataContext = viewModel;

                bool? result = dialog.ShowDialog();

                if (result == true && viewModel.CreatedEvent != null)
                {
                    // Create a new ProgramSlot
                    var startTime = viewModel.GetSelectedDateTime();
                    var endTime = startTime.AddMinutes(viewModel.Duration);

                    var newSlot = new ProgramSlot
                    {
                        Id = GetNextProgramSlotId(),
                        Name = viewModel.Name,
                        BeginHour = startTime,
                        EndHour = endTime,
                        RaceFormatDetailId = -1, // Use -1 for manual slots
                        RaceFormatDetail = CreateManualRaceFormatDetail(),
                        ProgramMeetingId = programMeeting.Id,
                        ProgramMeeting = programMeeting
                    };

                    // Add directly to the ProgramMeeting (this updates both the in-memory object and XML since they reference the same object)
                    programMeeting.ProgramSlots.Add(newSlot);

                    _xmlService.Save();

                    // Refresh UI
                    OnPropertyChanged(nameof(SitesForSelectedDate));
                    UpdateEventCount();
                    await ValidateScheduleAsync();

                    _logger.LogInformation("Added manual slot: {SlotName} to meeting: {MeetingName}", newSlot.Name, programMeeting.Name);
                }
                else
                {
                    _logger.LogDebug("Manual slot creation cancelled by user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding manual slot to meeting: {MeetingName}", programMeeting?.Name ?? "Unknown");
            }
        }

        private int GetNextProgramSlotId()
        {
            var existingIds = ProgramMeetings.SelectMany(m => m.ProgramSlots).Select(s => s.Id).ToList();
            return existingIds.Any() ? existingIds.Max() + 1 : 1;
        }

        private RaceFormatDetail CreateManualRaceFormatDetail()
        {
            return new RaceFormatDetail
            {
                Id = -1,
                Label = "Manual Event",
                FullLabel = "Manual Time Slot",
                LevelLabel = "Manual",
                Level = HeatLevel.Heat,
                Order = 999,
                NumberOfRun = 1,
                RaceFormatConfiguration = new RaceFormatConfiguration
                {
                    Id = -1,
                    Label = "Manual Events",
                    FullLabel = "Manual Time Slots",
                    Gender = Gender.Mixte,
                    Discipline = 999,
                    DisciplineLabel = "Manual",
                    Categories = new List<Category>()
                }
            };
        }

        private async Task DeleteSlotAsync(ProgramSlot? programSlot)
        {
            try
            {
                if (programSlot == null)
                {
                    _logger.LogWarning("Cannot delete slot: ProgramSlot is null");
                    return;
                }

                // Show confirmation dialog
                var result = System.Windows.MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer le slot '{programSlot.Name}' ?",
                    "Confirmation de suppression",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    _logger.LogDebug("Slot deletion cancelled by user");
                    return;
                }

                // Find and remove from Program structure
                var program = _xmlService.GetProgram();
                bool slotFound = false;

                foreach (var site in program.Sites)
                {
                    foreach (var meeting in site.ProgramMeetings)
                    {
                        var slotToRemove = meeting.ProgramSlots.FirstOrDefault(s => s.Id == programSlot.Id);
                        if (slotToRemove != null)
                        {
                            meeting.ProgramSlots.Remove(slotToRemove);
                            slotFound = true;
                            break;
                        }
                    }
                    if (slotFound)
                    {
                        break;
                    }
                }

                if (slotFound)
                {
                    // Restore race format to available list if it was a race format slot (not manual)
                    if (programSlot.RaceFormatDetailId > 0 && programSlot.RaceFormatDetail != null)
                    {
                        _logger.LogInformation("Restoring race format to available list: {Label} (ID: {Id})",
                            programSlot.RaceFormatDetail.Label, programSlot.RaceFormatDetail.Id);
                        RestoreRaceFormatToAvailable(programSlot.RaceFormatDetail);
                    }
                    else
                    {
                        _logger.LogDebug("Slot being deleted is manual (ID: {SlotId}), no race format to restore", programSlot.Id);
                    }

                    // Refresh the available race formats to apply discipline-based filtering
                    RefreshAvailableRaceFormats();

                    // Save to XML
                    _xmlService.Save();

                    // Refresh UI
                    OnPropertyChanged(nameof(SitesForSelectedDate));
                    UpdateEventCount();
                    await ValidateScheduleAsync();

                    _logger.LogInformation("Deleted slot: {SlotName} (ID: {SlotId})", programSlot.Name, programSlot.Id);
                }
                else
                {
                    _logger.LogWarning("Could not find slot to delete: {SlotName} (ID: {SlotId})", programSlot.Name, programSlot.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting slot: {SlotName}", programSlot?.Name ?? "Unknown");

                // Show error message to user
                System.Windows.MessageBox.Show(
                    "Une erreur s'est produite lors de la suppression du slot. Veuillez réessayer.",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region Drag and Drop Support

        public async void HandleEventDrop(object raceFormatDetail, DateTime timeSlot)
        {
            try
            {
                if (raceFormatDetail is not RaceFormatDetail formatDetail)
                {
                    return;
                }
                var targetMeeting = ProgramMeetings.FirstOrDefault(m =>
                    m.ProgramDate.Date == timeSlot.Date &&
                    timeSlot >= m.BeginHour && timeSlot <= m.EndHour);

                if (targetMeeting == null)
                {
                    _logger.LogWarning("No suitable meeting found for time slot: {TimeSlot}", timeSlot);
                    return;
                }

                var newSlot = await _programService.CreateProgramSlotAsync(
                    meetingId: targetMeeting.Id,
                    name: formatDetail.Label,
                    beginHour: timeSlot,
                    endHour: timeSlot.AddMinutes(30), // Default 30-minute duration
                    raceFormatDetailId: formatDetail.Id
                );

                targetMeeting.ProgramSlots.Add(newSlot);
                UpdateEventCount();
                await ValidateScheduleAsync();

                _logger.LogInformation("Added event from drag-drop: {EventName} at {TimeSlot}",
                    formatDetail.Label, timeSlot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event drop");
            }
        }

        public async void HandleRaceFormatDrop(RaceFormatDetail raceFormatDetail, ProgramMeeting programMeeting)
        {
            try
            {
                _logger.LogInformation("Handling race format drop: {RaceFormat} into meeting: {MeetingName}",
                    raceFormatDetail.Label, programMeeting.Name);

                // Calculate start time - end hour of the previous slot or meeting begin hour
                var startTime = programMeeting.ProgramSlots.Any()
                    ? programMeeting.ProgramSlots.Max(s => s.EndHour)
                    : programMeeting.BeginHour;

                // Calculate duration based on NumberOfRun, interval, and race entries
                double durationMinutes = await CalculateSlotDuration(raceFormatDetail);
                var endTime = startTime.AddMinutes(durationMinutes);

                // Create new ProgramSlot
                var newSlot = new ProgramSlot
                {
                    Id = GetNextProgramSlotId(),
                    Name = raceFormatDetail.Label,
                    BeginHour = startTime,
                    EndHour = endTime,
                    RaceFormatDetailId = raceFormatDetail.Id,
                    RaceFormatDetail = raceFormatDetail,
                    ProgramMeetingId = programMeeting.Id,
                    ProgramMeeting = programMeeting
                };

                // Add to meeting
                programMeeting.ProgramSlots.Add(newSlot);

                // Update meeting end hour to max of all slots
                UpdateMeetingEndHour(programMeeting);

                // Save to XML
                _xmlService.Save();

                // Remove from available race formats
                RemoveRaceFormatFromAvailable(raceFormatDetail);

                // Refresh UI
                OnPropertyChanged(nameof(SitesForSelectedDate));
                UpdateEventCount();
                await ValidateScheduleAsync();

                _logger.LogInformation("Added race format slot: {SlotName} ({StartTime:HH:mm} - {EndTime:HH:mm}) to meeting: {MeetingName}",
                    newSlot.Name, newSlot.BeginHour, newSlot.EndHour, programMeeting.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling race format drop: {RaceFormat}", raceFormatDetail?.Label);
            }
        }

        private async Task<double> CalculateSlotDuration(RaceFormatDetail raceFormatDetail)
        {
            try
            {
                if (raceFormatDetail?.Races == null || !raceFormatDetail.Races.Any())
                {
                    // Fallback: use a default duration if no races available
                    return raceFormatDetail?.NumberOfRun * 5.0 ?? 30.0; // 5 minutes per run, or 30 minutes default
                }

                // Get app settings to get NumberOfLanes
                var appSettings = _xmlService.GetSetting();
                int numberOfLanes = appSettings?.NumberOfLanes ?? 8; // Default to 8 lanes if not found

                // Base duration minutes - could be configurable, using a default of 5 minutes
                double baseDurationMinutes = 5.0;

                // Collect all teams from all races and sort by ascending EntryTime
                var allTeams = new List<Team>();
                await Task.Run(() =>
                {
                    foreach (var race in raceFormatDetail.Races)
                    {
                        var availableTeams = race.GetAvailableTeams();
                        allTeams.AddRange(availableTeams);
                    }
                    
                    // Sort teams by ascending EntryTime
                    allTeams = allTeams.OrderBy(team => team.EntryTime).ToList();
                });

                // Calculate duration for each possible heat
                double totalHeatDurationMinutes = 0.0;
                int teamIndex = 0;
                
                while (teamIndex < allTeams.Count)
                {
                    // For this heat, take every NumberOfLanes element and find the max EntryTime
                    double maxEntryTimeForHeat = 0.0;
                    int teamsInThisHeat = 0;
                    
                    for (int laneIndex = 0; laneIndex < numberOfLanes && (teamIndex + laneIndex) < allTeams.Count; laneIndex++)
                    {
                        var team = allTeams[teamIndex + laneIndex];
                        double teamEntryTimeMinutes = team.EntryTime / 100.0 / 60.0; // Convert centiseconds to minutes
                        
                        if (teamEntryTimeMinutes > maxEntryTimeForHeat)
                        {
                            maxEntryTimeForHeat = teamEntryTimeMinutes;
                        }
                        
                        teamsInThisHeat++;
                    }
                    
                    // Add the max entry time for this heat to the total duration
                    totalHeatDurationMinutes += maxEntryTimeForHeat;
                    
                    // Move to the next group of teams
                    teamIndex += numberOfLanes;
                }

                // Add base duration to the calculated heat duration
                double totalDurationMinutes = totalHeatDurationMinutes + baseDurationMinutes;

                _logger.LogDebug("Calculated slot duration for {RaceFormatDetail}: {Duration} minutes " +
                    "(Heat durations: {HeatDuration}, Base: {BaseDuration}, Teams: {TeamCount})", 
                    raceFormatDetail.Label, totalDurationMinutes, totalHeatDurationMinutes, baseDurationMinutes, allTeams.Count);

                return Math.Max(totalDurationMinutes, 5.0); // Minimum 5 minutes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating slot duration for RaceFormatDetail: {RaceFormatDetailId}", 
                    raceFormatDetail?.Id);
                // Fallback: use a default duration
                return raceFormatDetail?.NumberOfRun * 5.0 ?? 60.0; // 5 minutes per run or 60 minutes fallback
            }
        }

        private void UpdateMeetingEndHour(ProgramMeeting programMeeting)
        {
            if (programMeeting.ProgramSlots.Any())
            {
                var maxEndHour = programMeeting.ProgramSlots.Max(s => s.EndHour);
                if (maxEndHour > programMeeting.EndHour)
                {
                    programMeeting.EndHour = maxEndHour;
                    _logger.LogDebug("Updated meeting {MeetingName} end hour to {EndHour:HH:mm}",
                        programMeeting.Name, programMeeting.EndHour);
                }
            }
        }

        private void RemoveRaceFormatFromAvailable(RaceFormatDetail raceFormatDetail)
        {
            var formatToRemove = AvailableRaceFormats.FirstOrDefault(f => f.Id == raceFormatDetail.Id);
            if (formatToRemove != null)
            {
                AvailableRaceFormats.Remove(formatToRemove);
                _logger.LogDebug("Removed race format from available list: {Label}", raceFormatDetail.Label);
            }
        }

        private void RestoreRaceFormatToAvailable(RaceFormatDetail raceFormatDetail)
        {
            try
            {
                _logger.LogDebug("Attempting to restore race format: {Label} (ID: {Id})",
                    raceFormatDetail.Label, raceFormatDetail.Id);

                // Check if it's already in the available list
                if (!AvailableRaceFormats.Any(f => f.Id == raceFormatDetail.Id))
                {
                    // Check if the discipline is still in use by other slots
                    string? disciplineLabel = raceFormatDetail.RaceFormatConfiguration?.DisciplineLabel?.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(disciplineLabel))
                    {
                        bool isDisciplineStillUsed = false;
                        foreach (var site in AvailableSites)
                        {
                            foreach (var meeting in site.ProgramMeetings)
                            {
                                foreach (var slot in meeting.ProgramSlots)
                                {
                                    if (slot.RaceFormatDetailId > 0 &&
                                        slot.RaceFormatDetail != null &&
                                        slot.RaceFormatDetail.Id != raceFormatDetail.Id &&  // Don't count the slot we just deleted
                                        slot.RaceFormatDetail.RaceFormatConfiguration?.DisciplineLabel?.ToLowerInvariant() == disciplineLabel)
                                    {
                                        isDisciplineStillUsed = true;
                                        break;
                                    }
                                }
                                if (isDisciplineStillUsed)
                                { break; }
                            }
                            if (isDisciplineStillUsed)
                            { break; }
                        }

                        if (isDisciplineStillUsed)
                        {
                            _logger.LogDebug("Cannot restore race format {Label} - discipline {Discipline} is still in use by other slots",
                                raceFormatDetail.Label, disciplineLabel);
                            return;
                        }
                    }

                    // Find the correct position to insert (maintain original order)
                    RaceFormatDetail? originalFormat = _allRaceFormats.FirstOrDefault(f => f.Id == raceFormatDetail.Id);
                    if (originalFormat != null)
                    {
                        int insertIndex = 0;
                        for (int i = 0; i < _allRaceFormats.Count; i++)
                        {
                            if (_allRaceFormats[i].Id == raceFormatDetail.Id)
                            {
                                // Count how many formats before this one are still in AvailableRaceFormats
                                insertIndex = _allRaceFormats.Take(i).Count(f => AvailableRaceFormats.Any(af => af.Id == f.Id));
                                break;
                            }
                        }

                        AvailableRaceFormats.Insert(insertIndex, originalFormat);
                        _logger.LogInformation("Successfully restored race format to available list at index {Index}: {Label}",
                            insertIndex, raceFormatDetail.Label);
                    }
                    else
                    {
                        _logger.LogWarning("Could not find original race format in _allRaceFormats: {Label} (ID: {Id})",
                            raceFormatDetail.Label, raceFormatDetail.Id);
                    }
                }
                else
                {
                    _logger.LogDebug("Race format already exists in available list: {Label} (ID: {Id})",
                        raceFormatDetail.Label, raceFormatDetail.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring race format to available list: {Label}", raceFormatDetail?.Label);
            }
        }

        private void RefreshAvailableRaceFormats()
        {
            // Get all used race format IDs and disciplines from current program slots
            var usedRaceFormatIds = new HashSet<int>();
            var usedDisciplines = new HashSet<string>();

            foreach (var site in AvailableSites)
            {
                foreach (var meeting in site.ProgramMeetings)
                {
                    foreach (var slot in meeting.ProgramSlots)
                    {
                        if (slot.RaceFormatDetailId > 0 && slot.RaceFormatDetail != null) // Exclude manual slots (-1)
                        {
                            usedRaceFormatIds.Add(slot.RaceFormatDetailId);

                            // Add discipline to used set
                            string? disciplineLabel = slot.RaceFormatDetail.RaceFormatConfiguration?.DisciplineLabel?.ToLowerInvariant();
                            if (!string.IsNullOrEmpty(disciplineLabel))
                            {
                                usedDisciplines.Add(disciplineLabel);
                            }
                        }
                    }
                }
            }

            // Rebuild AvailableRaceFormats excluding:
            // 1. Already used race format IDs
            // 2. Race formats with disciplines that are already used (like 'eau-plate')
            AvailableRaceFormats.Clear();
            foreach (var format in _allRaceFormats.Where(f =>
                !usedRaceFormatIds.Contains(f.Id) &&
                !usedDisciplines.Contains(f.RaceFormatConfiguration?.DisciplineLabel?.ToLowerInvariant() ?? "")))
            {
                AvailableRaceFormats.Add(format);
            }

            _logger.LogDebug("Refreshed available race formats. Used disciplines: {UsedDisciplines}",
                string.Join(", ", usedDisciplines));
        }

        #endregion

        #region Helper Methods

        private bool CanGoToPreviousDay()
        {
            if (CurrentCompetition == null)
            {
                return true;
            }
            return SelectedDate.AddDays(-1) >= CurrentCompetition.BeginDate.Date;
        }

        private bool CanGoToNextDay()
        {
            if (CurrentCompetition == null)
            {
                return true;
            }
            return SelectedDate.AddDays(1) <= CurrentCompetition.EndDate.Date;
        }

        private bool CanGoToToday()
        {
            if (CurrentCompetition == null)
            {
                return true;
            }
            var today = DateTime.Today;
            return today >= CurrentCompetition.BeginDate.Date && today <= CurrentCompetition.EndDate.Date;
        }

        private void RefreshCommandStates()
        {
            // Force command CanExecute evaluation by calling NotifyCanExecuteChanged
            if (PreviousDayCommand is RelayCommand previousCmd)
            { previousCmd.NotifyCanExecuteChanged(); }
            if (NextDayCommand is RelayCommand nextCmd)
            { nextCmd.NotifyCanExecuteChanged(); }
            if (TodayCommand is RelayCommand todayCmd)
            { todayCmd.NotifyCanExecuteChanged(); }
        }

        private void UpdateStatusIndicators()
        {
            if (CurrentProgram == null)
            { return; }

            StatusColor = CurrentProgram.Status switch
            {
                ScheduleStatus.Draft => Brushes.Blue,
                ScheduleStatus.Published => Brushes.Green,
                ScheduleStatus.Archived => Brushes.Gray,
                _ => Brushes.Black
            };
        }

        private void UpdateEventCount()
        {
            EventCount = ProgramMeetings.SelectMany(m => m.ProgramSlots).Count();
        }

        public string Duration => SelectedEvent != null
            ? $"{(SelectedEvent.EndHour - SelectedEvent.BeginHour).TotalMinutes:F0} minutes"
            : "N/A";

        private void CreateSampleRaceFormats()
        {
            try
            {
                // Create sample race format configurations for testing
                var swimConfig = new RaceFormatConfiguration
                {
                    Id = 1,
                    Label = "Swimming Events",
                    FullLabel = "Pool Swimming Competitions",
                    Gender = Gender.Men,
                    Discipline = 1,
                    DisciplineLabel = "Swimming",
                    Categories = new List<Category>()
                };

                var beachConfig = new RaceFormatConfiguration
                {
                    Id = 2,
                    Label = "Beach Events",
                    FullLabel = "Beach Lifesaving Events",
                    Gender = Gender.Woman,
                    Discipline = 2,
                    DisciplineLabel = "Beach",
                    Categories = new List<Category>()
                };

                var mixedConfig = new RaceFormatConfiguration
                {
                    Id = 3,
                    Label = "Mixed Events",
                    FullLabel = "Mixed Gender Events",
                    Gender = Gender.Mixte,
                    Discipline = 3,
                    DisciplineLabel = "Mixed",
                    Categories = new List<Category>()
                };

                // Create sample race format details
                var sampleFormats = new List<RaceFormatDetail>
                {
                    new RaceFormatDetail
                    {
                        Id = 1,
                        Label = "50m Freestyle",
                        FullLabel = "50 meter Freestyle Individual",
                        LevelLabel = "Heat",
                        Level = HeatLevel.Heat,
                        Order = 1,
                        NumberOfRun = 1,
                        RaceFormatConfiguration = swimConfig
                    },
                    new RaceFormatDetail
                    {
                        Id = 2,
                        Label = "100m Rescue Medley",
                        FullLabel = "100 meter Individual Rescue Medley",
                        LevelLabel = "Heat",
                        Level = HeatLevel.Heat,
                        Order = 2,
                        NumberOfRun = 1,
                        RaceFormatConfiguration = swimConfig
                    },
                    new RaceFormatDetail
                    {
                        Id = 3,
                        Label = "Beach Sprint",
                        FullLabel = "Beach Sprint 90m",
                        LevelLabel = "Heat",
                        Level = HeatLevel.Heat,
                        Order = 1,
                        NumberOfRun = 1,
                        RaceFormatConfiguration = beachConfig
                    },
                    new RaceFormatDetail
                    {
                        Id = 4,
                        Label = "Beach Flags",
                        FullLabel = "Beach Flags Competition",
                        LevelLabel = "Heat",
                        Level = HeatLevel.Heat,
                        Order = 2,
                        NumberOfRun = 1,
                        RaceFormatConfiguration = beachConfig
                    },
                    new RaceFormatDetail
                    {
                        Id = 5,
                        Label = "Mixed Relay",
                        FullLabel = "4x50m Mixed Rescue Relay",
                        LevelLabel = "Final",
                        Level = HeatLevel.Final,
                        Order = 1,
                        NumberOfRun = 1,
                        RaceFormatConfiguration = mixedConfig
                    }
                };

                var orderedSampleFormats = sampleFormats.OrderBy(f => f.RaceFormatConfiguration.Discipline).ThenBy(f => f.Order).ToList();
                _allRaceFormats = orderedSampleFormats;
                foreach (var format in orderedSampleFormats)
                {
                    AvailableRaceFormats.Add(format);
                }

                _logger.LogInformation("Created {Count} sample race formats for testing", sampleFormats.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample race formats");
            }
        }

        #endregion

        #region Public Methods

        public async void RefreshData()
        {
            try
            {
                _logger.LogInformation("Refreshing PlanningProgram data");

                await LoadCurrentCompetitionAsync();
                await LoadCurrentProgramAsync();
                await LoadAvailableRaceFormatsAsync();
                await LoadAvailableSitesAsync();
                await LoadProgramMeetingsAsync();
                await ValidateScheduleAsync();

                _logger.LogInformation("PlanningProgram data refreshed successfully. Available race formats: {Count}", AvailableRaceFormats.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing PlanningProgram data");
            }
        }

        #endregion
    }
}
