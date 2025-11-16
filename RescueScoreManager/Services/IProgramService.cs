using RescueScoreManager.Data;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Services;

public interface IProgramService
{
    // Program Management
    Task<Program> CreateProgramAsync(string description = "");
    Task<Program?> GetProgramAsync(int id);
    Task<IEnumerable<Program>> GetAllProgramsAsync();
    Task<Program?> GetCurrentProgramAsync();
    Task<Program> UpdateProgramAsync(Program program);
    Task<bool> DeleteProgramAsync(int id);
    Task SetCurrentProgramAsync(Program program);

    // Program Meeting Management
    Task<ProgramMeeting> CreateProgramMeetingAsync(int programId, string name, DateTime programDate, 
        DateTime beginHour, DateTime endHour, int? siteId = null, string description = "");
    Task<ProgramMeeting> UpdateProgramMeetingAsync(ProgramMeeting meeting);
    Task<bool> DeleteProgramMeetingAsync(int meetingId);
    Task<IEnumerable<ProgramMeeting>> GetProgramMeetingsAsync(int programId);

    // Program Slot Management
    Task<ProgramSlot> CreateProgramSlotAsync(int meetingId, string name, DateTime beginHour, 
        DateTime endHour, int raceFormatDetailId);
    Task<ProgramSlot> UpdateProgramSlotAsync(ProgramSlot slot);
    Task<bool> DeleteProgramSlotAsync(int slotId);

    // Manual Events (Special Program Slots)
    Task<ProgramSlot> CreateManualEventAsync(int meetingId, string name, DateTime beginHour, 
        int durationMinutes, string eventType = "Manual");

    // Publishing
    Task<Program> PublishProgramAsync(int programId);
    Task<Program> UnpublishProgramAsync(int programId);
    Task<Program> ArchiveProgramAsync(int programId);

    // Auto-save functionality
    Task AutoSaveProgramAsync(Program program);
    bool IsAutoSaveEnabled { get; set; }

    // Bulk operations
    Task<IEnumerable<ProgramMeeting>> BulkUpdateMeetingsAsync(IEnumerable<ProgramMeeting> meetings);
    Task<bool> BulkDeleteMeetingsAsync(IEnumerable<int> meetingIds);

    // Program versioning
    Task<Program> CreateNewVersionAsync(int programId, string description = "");
    Task<IEnumerable<Program>> GetProgramVersionsAsync(int baseProgramId);

    // Events
    event EventHandler<Program>? ProgramPublished;
    event EventHandler<Program>? ProgramUpdated;
    event EventHandler<ProgramMeeting>? MeetingUpdated;
}