using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Messages;
using RescueScoreManager.Services;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Modules.Planning.ViewModels;

public partial class ProgramViewModel : ObservableObject
{
    private readonly IProgramService _programService;
    private readonly IProgramValidationService _validationService;
    private readonly IXMLService _xmlService;
    private readonly ILocalizationService _localizationService;
    private readonly IDialogService _dialogService;
    private readonly IMessenger _messenger;
    private readonly ILogger<ProgramViewModel> _logger;

    [ObservableProperty]
    private Program? _currentProgram;

    [ObservableProperty]
    private ProgramValidationResult? _lastValidationResult;

    [ObservableProperty]
    private bool _isValidating;

    [ObservableProperty]
    private bool _hasUnsavedChanges;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<ProgramMeetingViewModel> ProgramMeetings { get; } = new();
    public ObservableCollection<SiteViewModel> AvailableSites { get; } = new();
    public ObservableCollection<RaceFormatDetailViewModel> AvailableEvents { get; } = new();

    public ICommand CreateProgramCommand { get; }
    public ICommand SaveProgramCommand { get; }
    public ICommand PublishProgramCommand { get; }
    public ICommand ValidateProgramCommand { get; }
    public ICommand CreateMeetingCommand { get; }
    public ICommand CreateManualEventCommand { get; }

    public ProgramViewModel(
        IProgramService programService,
        IProgramValidationService validationService,
        IXMLService xmlService,
        ILocalizationService localizationService,
        IDialogService dialogService,
        IMessenger messenger,
        ILogger<ProgramViewModel> logger)
    {
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        CreateProgramCommand = new AsyncRelayCommand(OnCreateProgramAsync);
        SaveProgramCommand = new AsyncRelayCommand(OnSaveProgramAsync, CanSaveProgram);
        PublishProgramCommand = new AsyncRelayCommand(OnPublishProgramAsync, CanPublishProgram);
        ValidateProgramCommand = new AsyncRelayCommand(OnValidateProgramAsync);
        CreateMeetingCommand = new AsyncRelayCommand(OnCreateMeetingAsync);
        CreateManualEventCommand = new AsyncRelayCommand(OnCreateManualEventAsync);

        // Subscribe to service events
        _programService.ProgramUpdated += OnProgramUpdated;
        _programService.MeetingUpdated += OnMeetingUpdated;

        // Enable auto-save
        _programService.IsAutoSaveEnabled = true;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing Program module");
            
            await LoadAvailableSitesAsync();
            await LoadAvailableEventsAsync();
            await LoadCurrentProgramAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Program module");
            StatusMessage = _localizationService.GetString("InitializationError");
        }
    }

    private async Task LoadAvailableSitesAsync()
    {
        var sites = _xmlService.GetSites();
        AvailableSites.Clear();
        
        foreach (var site in sites)
        {
            AvailableSites.Add(new SiteViewModel(site));
        }
    }

    private async Task LoadAvailableEventsAsync()
    {
        var raceFormatConfigurations = _xmlService.GetRaceFormatConfigurations();
        AvailableEvents.Clear();
        
        foreach (var config in raceFormatConfigurations)
        {
            foreach (var detail in config.RaceFormatDetails)
            {
                AvailableEvents.Add(new RaceFormatDetailViewModel(detail));
            }
        }
    }

    private async Task LoadCurrentProgramAsync()
    {
        var currentProgram = await _programService.GetCurrentProgramAsync();
        if (currentProgram != null)
        {
            CurrentProgram = currentProgram;
            await LoadProgramMeetingsAsync();
        }
    }

    private async Task LoadProgramMeetingsAsync()
    {
        if (CurrentProgram == null) return;

        var meetings = await _programService.GetProgramMeetingsAsync(CurrentProgram.Id);
        ProgramMeetings.Clear();
        
        foreach (var meeting in meetings)
        {
            var meetingViewModel = new ProgramMeetingViewModel(meeting, _programService, _validationService, _localizationService);
            ProgramMeetings.Add(meetingViewModel);
        }
    }

    private async Task OnCreateProgramAsync()
    {
        try
        {
            var description = _localizationService.GetString("NewProgramDescription") ?? "New Competition Program";
            var newProgram = await _programService.CreateProgramAsync(description);
            
            CurrentProgram = newProgram;
            await _programService.SetCurrentProgramAsync(newProgram);
            
            ProgramMeetings.Clear();
            HasUnsavedChanges = false;
            StatusMessage = _localizationService.GetString("ProgramCreated");
            
            _messenger.Send(new SnackMessage(_localizationService.GetString("ProgramCreatedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new program");
            StatusMessage = _localizationService.GetString("ErrorCreatingProgram");
        }
    }

    private async Task OnSaveProgramAsync()
    {
        if (CurrentProgram == null) return;

        try
        {
            await _programService.UpdateProgramAsync(CurrentProgram);
            HasUnsavedChanges = false;
            StatusMessage = _localizationService.GetString("ProgramSaved");
            
            _messenger.Send(new SnackMessage(_localizationService.GetString("ProgramSavedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving program");
            StatusMessage = _localizationService.GetString("ErrorSavingProgram");
        }
    }

    private async Task OnPublishProgramAsync()
    {
        if (CurrentProgram == null) return;

        try
        {
            // Validate before publishing
            await OnValidateProgramAsync();
            
            if (LastValidationResult?.IsValid != true)
            {
                var canPublish = await _validationService.CanPublishProgramAsync(CurrentProgram);
                if (!canPublish)
                {
                    var message = await _validationService.GetPublishValidationMessageAsync(CurrentProgram);
                    _dialogService.ShowMessage(_localizationService.GetString("PublishError"), message);
                    return;
                }
            }

            var confirmed = _dialogService.ShowConfirmation(
                _localizationService.GetString("ConfirmPublish"),
                _localizationService.GetString("ConfirmPublishMessage"));

            if (!confirmed) return;

            var publishedProgram = await _programService.PublishProgramAsync(CurrentProgram.Id);
            CurrentProgram = publishedProgram;
            
            StatusMessage = _localizationService.GetString("ProgramPublished");
            _messenger.Send(new SnackMessage(_localizationService.GetString("ProgramPublishedSuccessfully")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing program");
            StatusMessage = _localizationService.GetString("ErrorPublishingProgram");
        }
    }

    private async Task OnValidateProgramAsync()
    {
        if (CurrentProgram == null) return;

        try
        {
            IsValidating = true;
            var validationResult = await _validationService.ValidateProgramAsync(CurrentProgram);
            LastValidationResult = validationResult;
            
            var conflictCount = validationResult.Conflicts.Count;
            var warningCount = validationResult.Warnings.Count;
            
            if (conflictCount == 0 && warningCount == 0)
            {
                StatusMessage = _localizationService.GetString("ValidationPassed");
            }
            else
            {
                StatusMessage = string.Format(_localizationService.GetString("ValidationResults"), 
                    conflictCount, warningCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating program");
            StatusMessage = _localizationService.GetString("ErrorValidatingProgram");
        }
        finally
        {
            IsValidating = false;
        }
    }

    private async Task OnCreateMeetingAsync()
    {
        if (CurrentProgram == null) return;

        try
        {
            // This would typically open a dialog to get meeting details
            var meetingName = "New Meeting";
            var programDate = DateTime.Today;
            var beginHour = DateTime.Today.AddHours(9);
            var endHour = DateTime.Today.AddHours(17);

            var newMeeting = await _programService.CreateProgramMeetingAsync(
                CurrentProgram.Id, meetingName, programDate, beginHour, endHour);
            
            var meetingViewModel = new ProgramMeetingViewModel(newMeeting, _programService, _validationService, _localizationService);
            ProgramMeetings.Add(meetingViewModel);
            
            HasUnsavedChanges = true;
            StatusMessage = _localizationService.GetString("MeetingCreated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meeting");
            StatusMessage = _localizationService.GetString("ErrorCreatingMeeting");
        }
    }

    private async Task OnCreateManualEventAsync()
    {
        // Implementation for creating manual events like warm-up, break, ceremonies
        // This would open a dialog to specify event details
        await Task.CompletedTask;
    }

    private bool CanSaveProgram() => CurrentProgram != null && HasUnsavedChanges;
    
    private bool CanPublishProgram() => CurrentProgram != null && CurrentProgram.Status == ScheduleStatus.Draft;

    private void OnProgramUpdated(object? sender, Program program)
    {
        if (CurrentProgram?.Id == program.Id)
        {
            CurrentProgram = program;
        }
    }

    private void OnMeetingUpdated(object? sender, ProgramMeeting meeting)
    {
        var meetingViewModel = ProgramMeetings.FirstOrDefault(m => m.Id == meeting.Id);
        if (meetingViewModel != null)
        {
            meetingViewModel.UpdateFromModel(meeting);
        }
        HasUnsavedChanges = true;
    }

    partial void OnCurrentProgramChanged(Program? value)
    {
        ((AsyncRelayCommand)SaveProgramCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)PublishProgramCommand).NotifyCanExecuteChanged();
    }

    partial void OnHasUnsavedChangesChanged(bool value)
    {
        ((AsyncRelayCommand)SaveProgramCommand).NotifyCanExecuteChanged();
    }
}