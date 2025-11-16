using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RescueScoreManager.Data;
using RescueScoreManager.Services;

namespace RescueScoreManager.Modules.Planning.ViewModels;

public partial class ProgramSlotViewModel : ObservableObject
{
    private readonly IProgramService _programService;
    private readonly IProgramValidationService _validationService;
    private readonly ILocalizationService _localizationService;

    [ObservableProperty]
    private ProgramSlot _slot;

    [ObservableProperty]
    private bool _hasConflicts;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isDragging;

    [ObservableProperty]
    private ProgramValidationResult? _validationResult;

    public int Id => Slot.Id;
    
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private DateTime _beginHour;
    
    [ObservableProperty]
    private DateTime _endHour;

    public TimeSpan Duration => EndHour - BeginHour;
    public string DurationText => Duration.ToString(@"hh\:mm");
    public string TimeRangeText => $"{BeginHour:HH:mm} - {EndHour:HH:mm}";
    
    public RaceFormatDetail? RaceFormatDetail => Slot.RaceFormatDetail;
    public bool IsManualEvent => RaceFormatDetail == null;
    public string EventType => IsManualEvent ? "Manual" : RaceFormatDetail?.Label ?? "Unknown";

    public ICommand EditSlotCommand { get; }
    public ICommand DeleteSlotCommand { get; }
    public ICommand ValidateSlotCommand { get; }

    public ProgramSlotViewModel(
        ProgramSlot slot,
        IProgramService programService,
        IProgramValidationService validationService,
        ILocalizationService localizationService)
    {
        _slot = slot ?? throw new ArgumentNullException(nameof(slot));
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        // Initialize properties from slot
        _name = slot.Name;
        _beginHour = slot.BeginHour;
        _endHour = slot.EndHour;

        EditSlotCommand = new AsyncRelayCommand(OnEditSlotAsync);
        DeleteSlotCommand = new AsyncRelayCommand(OnDeleteSlotAsync);
        ValidateSlotCommand = new AsyncRelayCommand(OnValidateSlotAsync);
    }

    public void UpdateFromModel(ProgramSlot slot)
    {
        Slot = slot;
        Name = slot.Name;
        BeginHour = slot.BeginHour;
        EndHour = slot.EndHour;
        
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(TimeRangeText));
        OnPropertyChanged(nameof(RaceFormatDetail));
        OnPropertyChanged(nameof(IsManualEvent));
        OnPropertyChanged(nameof(EventType));
    }

    public async Task UpdateSlotAsync()
    {
        // Update the slot model with current properties
        Slot.Name = Name;
        Slot.BeginHour = BeginHour;
        Slot.EndHour = EndHour;

        try
        {
            await _programService.UpdateProgramSlotAsync(Slot);
        }
        catch (Exception ex)
        {
            // Log error and revert changes
            UpdateFromModel(Slot);
            throw;
        }
    }

    public ProgramSlot GetModel() => Slot;

    public bool CanMoveToTime(DateTime newBeginTime)
    {
        var duration = Duration;
        var newEndTime = newBeginTime + duration;
        
        // Basic time validation
        return newBeginTime >= DateTime.Today && newEndTime > newBeginTime;
    }

    public async Task<bool> MoveToTimeAsync(DateTime newBeginTime)
    {
        if (!CanMoveToTime(newBeginTime))
            return false;

        var duration = Duration;
        var newEndTime = newBeginTime + duration;
        
        try
        {
            // Validate the move
            var validationResult = await _validationService.ValidateTimeSlotChangeAsync(
                Slot, newBeginTime, newEndTime);
            
            if (!validationResult.IsValid)
            {
                ValidationResult = validationResult;
                HasConflicts = true;
                return false;
            }
            
            // Update times
            BeginHour = newBeginTime;
            EndHour = newEndTime;
            await UpdateSlotAsync();
            
            HasConflicts = false;
            return true;
        }
        catch (Exception ex)
        {
            // Handle error
            return false;
        }
    }

    public async Task<bool> ResizeAsync(DateTime newEndTime)
    {
        if (newEndTime <= BeginHour)
            return false;

        try
        {
            // Validate the resize
            var validationResult = await _validationService.ValidateTimeSlotChangeAsync(
                Slot, BeginHour, newEndTime);
            
            if (!validationResult.IsValid)
            {
                ValidationResult = validationResult;
                HasConflicts = true;
                return false;
            }
            
            // Update end time
            EndHour = newEndTime;
            await UpdateSlotAsync();
            
            HasConflicts = false;
            return true;
        }
        catch (Exception ex)
        {
            // Handle error
            return false;
        }
    }

    private async Task OnEditSlotAsync()
    {
        // This would typically open an edit dialog
        // For now, just validate the slot
        await OnValidateSlotAsync();
    }

    private async Task OnDeleteSlotAsync()
    {
        try
        {
            await _programService.DeleteProgramSlotAsync(Slot.Id);
        }
        catch (Exception ex)
        {
            // Handle error
        }
    }

    private async Task OnValidateSlotAsync()
    {
        try
        {
            ValidationResult = await _validationService.ValidateProgramSlotAsync(Slot);
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
        _ = Task.Run(async () => await UpdateSlotAsync());
    }

    partial void OnBeginHourChanged(DateTime value)
    {
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(TimeRangeText));
        _ = Task.Run(async () => await UpdateSlotAsync());
    }

    partial void OnEndHourChanged(DateTime value)
    {
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(DurationText));
        OnPropertyChanged(nameof(TimeRangeText));
        _ = Task.Run(async () => await UpdateSlotAsync());
    }

    public override bool Equals(object? obj)
    {
        return obj is ProgramSlotViewModel other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name} ({TimeRangeText})";
    }
}