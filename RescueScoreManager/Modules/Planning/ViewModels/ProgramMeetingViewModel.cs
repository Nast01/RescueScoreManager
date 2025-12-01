using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels;

public partial class ProgramMeetingViewModel : ObservableObject
{
    private readonly IProgramService _programService;
    private readonly IProgramValidationService _validationService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ProgramMeeting _meeting;

    [ObservableProperty]
    private SiteViewModel? _assignedSite;

    [ObservableProperty]
    private bool _hasConflicts;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isDragOver;

    [ObservableProperty]
    private ProgramValidationResult? _validationResult;

    public int Id => Meeting.Id;
    
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private string _description;
    
    [ObservableProperty]
    private DateTime _programDate;
    
    [ObservableProperty]
    private DateTime _beginHour;
    
    [ObservableProperty]
    private DateTime _endHour;

    public TimeSpan Duration => EndHour - BeginHour;
    public string DurationText => Duration.ToString(@"hh\:mm");

    public ObservableCollection<ProgramSlotViewModel> ProgramSlots { get; } = new();

    public ICommand EditMeetingCommand { get; }
    public ICommand DeleteMeetingCommand { get; }
    public ICommand AddSlotCommand { get; }
    public ICommand ValidateMeetingCommand { get; }

    public ProgramMeetingViewModel(
        ProgramMeeting meeting, 
        IProgramService programService,
        IProgramValidationService validationService,
        ILocalizationService localizationService)
    {
        _meeting = meeting ?? throw new ArgumentNullException(nameof(meeting));
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        // Initialize properties from meeting
        _name = meeting.Name;
        _description = meeting.Description;
        _programDate = meeting.ProgramDate;
        _beginHour = meeting.BeginHour;
        _endHour = meeting.EndHour;

        EditMeetingCommand = new AsyncRelayCommand(OnEditMeetingAsync);
        DeleteMeetingCommand = new AsyncRelayCommand(OnDeleteMeetingAsync);
        AddSlotCommand = new AsyncRelayCommand(OnAddSlotAsync);
        ValidateMeetingCommand = new AsyncRelayCommand(OnValidateMeetingAsync);

        LoadProgramSlots();
    }

    private void LoadProgramSlots()
    {
        ProgramSlots.Clear();
        foreach (var slot in Meeting.ProgramSlots)
        {
            var slotViewModel = new ProgramSlotViewModel(slot, _programService, _validationService, _localizationService);
            ProgramSlots.Add(slotViewModel);
        }
    }

    public void UpdateFromModel(ProgramMeeting meeting)
    {
        Meeting = meeting;
        Name = meeting.Name;
        Description = meeting.Description;
        ProgramDate = meeting.ProgramDate;
        BeginHour = meeting.BeginHour;
        EndHour = meeting.EndHour;
        
        LoadProgramSlots();
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(DurationText));
    }

    public async Task UpdateMeetingAsync()
    {
        // Update the meeting model with current properties
        Meeting.Name = Name;
        Meeting.Description = Description;
        Meeting.ProgramDate = ProgramDate;
        Meeting.BeginHour = BeginHour;
        Meeting.EndHour = EndHour;

        try
        {
            await _programService.UpdateProgramMeetingAsync(Meeting);
        }
        catch (Exception ex)
        {
            // Log error and revert changes
            UpdateFromModel(Meeting);
            throw;
        }
    }

    public bool CanAcceptDrop(object dragData)
    {
        // Check if the dragged data is compatible with this meeting
        return dragData is RaceFormatDetailViewModel || dragData is ProgramSlotViewModel;
    }

    public async Task<bool> HandleDropAsync(object dragData, DateTime? targetTime = null)
    {
        try
        {
            if (dragData is RaceFormatDetailViewModel raceFormatDetail)
            {
                return await HandleRaceFormatDetailDropAsync(raceFormatDetail, targetTime);
            }
            else if (dragData is ProgramSlotViewModel slot)
            {
                return await HandleSlotMoveAsync(slot, targetTime);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            // Log error
            return false;
        }
    }

    private async Task<bool> HandleRaceFormatDetailDropAsync(RaceFormatDetailViewModel raceFormatDetail, DateTime? targetTime)
    {
        var slotBeginTime = targetTime ?? BeginHour;
        var slotEndTime = slotBeginTime.AddMinutes(30); // Default 30 minutes, could be calculated from race format
        
        // Validate the time slot
        var validationResult = await _validationService.ValidateTimeSlotChangeAsync(
            new ProgramSlot(), slotBeginTime, slotEndTime);
        
        if (!validationResult.IsValid)
        {
            return false;
        }

        // Create new program slot
        var newSlot = await _programService.CreateProgramSlotAsync(
            Meeting.Id, 
            raceFormatDetail.Label, 
            slotBeginTime, 
            slotEndTime, 
            raceFormatDetail.Id);

        var slotViewModel = new ProgramSlotViewModel(newSlot, _programService, _validationService, _localizationService);
        ProgramSlots.Add(slotViewModel);
        
        return true;
    }

    private async Task<bool> HandleSlotMoveAsync(ProgramSlotViewModel slot, DateTime? targetTime)
    {
        if (targetTime.HasValue)
        {
            var duration = slot.EndHour - slot.BeginHour;
            var newBeginTime = targetTime.Value;
            var newEndTime = newBeginTime + duration;
            
            // Validate the move
            var validationResult = await _validationService.ValidateTimeSlotChangeAsync(
                slot.GetModel(), newBeginTime, newEndTime);
            
            if (!validationResult.IsValid)
            {
                return false;
            }
            
            // Update slot times
            slot.BeginHour = newBeginTime;
            slot.EndHour = newEndTime;
            await slot.UpdateSlotAsync();
        }
        
        return true;
    }

    private async Task OnEditMeetingAsync()
    {
        // This would typically open an edit dialog
        // For now, just validate the meeting
        await OnValidateMeetingAsync();
    }

    private async Task OnDeleteMeetingAsync()
    {
        try
        {
            await _programService.DeleteProgramMeetingAsync(Meeting.Id);
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }

    private async Task OnAddSlotAsync()
    {
        try
        {
            // Add a default manual slot
            var slotName = _localizationService.GetString("NewSlot") ?? "New Slot";
            var slotBeginTime = BeginHour;
            var slotEndTime = BeginHour.AddMinutes(30);
            
            var newSlot = await _programService.CreateManualEventAsync(
                Meeting.Id, slotName, slotBeginTime, 30);
            
            var slotViewModel = new ProgramSlotViewModel(newSlot, _programService, _validationService, _localizationService);
            ProgramSlots.Add(slotViewModel);
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }

    private async Task OnValidateMeetingAsync()
    {
        try
        {
            ValidationResult = await _validationService.ValidateProgramMeetingAsync(Meeting);
            HasConflicts = !ValidationResult.IsValid;
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }

    // Property change handlers to trigger auto-save
    partial void OnNameChanged(string value)
    {
        _ = Task.Run(async () => await UpdateMeetingAsync());
    }

    partial void OnDescriptionChanged(string value)
    {
        _ = Task.Run(async () => await UpdateMeetingAsync());
    }

    partial void OnBeginHourChanged(DateTime value)
    {
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(DurationText));
        _ = Task.Run(async () => await UpdateMeetingAsync());
    }

    partial void OnEndHourChanged(DateTime value)
    {
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(DurationText));
        _ = Task.Run(async () => await UpdateMeetingAsync());
    }

    public override bool Equals(object? obj)
    {
        return obj is ProgramMeetingViewModel other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}