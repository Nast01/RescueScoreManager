using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IProgramValidationService
{
    // Validation Results
    Task<ProgramValidationResult> ValidateProgramAsync(Program program);
    Task<ProgramValidationResult> ValidateProgramMeetingAsync(ProgramMeeting meeting);
    Task<ProgramValidationResult> ValidateProgramSlotAsync(ProgramSlot slot);
    
    // Specific Validation Checks
    Task<IEnumerable<ConflictResult>> CheckTimeOverlapsAsync(Program program);
    Task<IEnumerable<ConflictResult>> CheckSiteConflictsAsync(Program program);
    Task<IEnumerable<ConflictResult>> CheckAthleteRestTimeAsync(Program program, int minimumRestMinutes = 15);
    Task<IEnumerable<ConflictResult>> CheckMinimumBufferTimeAsync(Program program, int bufferMinutes = 5);
    
    // Real-time validation
    Task<ProgramValidationResult> ValidateTimeSlotChangeAsync(ProgramSlot slot, DateTime newBeginTime, DateTime newEndTime);
    Task<ProgramValidationResult> ValidateMeetingMoveAsync(ProgramMeeting meeting, DateTime newBeginTime, DateTime newEndTime);
    Task<ProgramValidationResult> ValidateSiteChangeAsync(ProgramMeeting meeting, int newSiteId);
    
    // Business Rules
    Task<bool> CanPublishProgramAsync(Program program);
    Task<string> GetPublishValidationMessageAsync(Program program);
    
    // Performance tracking
    TimeSpan LastValidationDuration { get; }
    int LastValidationEventCount { get; }
}

public class ProgramValidationResult
{
    public bool IsValid { get; set; }
    public List<ConflictResult> Conflicts { get; set; } = new List<ConflictResult>();
    public List<WarningResult> Warnings { get; set; } = new List<WarningResult>();
    public TimeSpan ValidationDuration { get; set; }
    public int EventCount { get; set; }
}

public class ConflictResult
{
    public ConflictType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string DetailedMessage { get; set; } = string.Empty;
    public ProgramMeeting? Meeting1 { get; set; }
    public ProgramMeeting? Meeting2 { get; set; }
    public ProgramSlot? Slot1 { get; set; }
    public ProgramSlot? Slot2 { get; set; }
    public DateTime ConflictStart { get; set; }
    public DateTime ConflictEnd { get; set; }
    public ConflictSeverity Severity { get; set; }
}

public class WarningResult
{
    public WarningType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public ProgramMeeting? Meeting { get; set; }
    public ProgramSlot? Slot { get; set; }
}

public enum ConflictType
{
    TimeOverlap,
    TimeConflict,
    SiteDoubleBooking,
    InsufficientRestTime,
    InsufficientBufferTime,
    InvalidTimeRange,
    MissingRequiredData
}

public enum WarningType
{
    ShortRestTime,
    LongEvent,
    UnassignedSite,
    MissingParticipants,
    SchedulingRecommendation,
    DeprecatedFeature
}

public enum ConflictSeverity
{
    Critical,   // Prevents publishing
    High,       // Should be resolved
    Medium,     // Warning but publishable
    Low         // Information only
}