using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Services;

public class ProgramService : IProgramService
{
    private readonly IXMLService _xmlService;
    private readonly ILogger<ProgramService> _logger;
    private readonly Timer? _autoSaveTimer;
    private Program? _pendingAutoSave;
    private readonly SemaphoreSlim _autoSaveSemaphore = new(1, 1);
    
    public bool IsAutoSaveEnabled { get; set; } = true;

    public event EventHandler<Program>? ProgramPublished;
    public event EventHandler<Program>? ProgramUpdated;
    public event EventHandler<ProgramMeeting>? MeetingUpdated;

    public ProgramService(IXMLService xmlService, ILogger<ProgramService> logger)
    {
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Auto-save every 30 seconds
        _autoSaveTimer = new Timer(AutoSaveCallback, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    #region Program Management

    public async Task<Program> CreateProgramAsync(string description = "")
    {
        return await Task.Run(() =>
        {
            try
            {
                var newProgram = new Program
                {
                    Id = 1, // Single program always has ID 1
                    Description = description,
                    Status = ScheduleStatus.Draft,
                    CreatedDate = DateTime.Now,
                    Version = 1,
                    Sites = new List<Site>()
                };

                _xmlService.SetCurrentProgram(newProgram);

                _logger.LogInformation("Created new program: {ProgramId} - {Description}", newProgram.Id, newProgram.Description);
                return newProgram;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating program: {Description}", description);
                throw;
            }
        });
    }

    public async Task<Program?> GetProgramAsync(int id)
    {
        return await Task.Run(() =>
        {
            try
            {
                var program = _xmlService.GetProgram();
                _logger.LogDebug("Retrieved program: {ProgramId}", id);
                return program.Id == id ? program : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program: {ProgramId}", id);
                return null;
            }
        });
    }

    public async Task<IEnumerable<Program>> GetAllProgramsAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var program = _xmlService.GetProgram();
                _logger.LogDebug("Retrieved program: {ProgramId}", program.Id);
                return new List<Program> { program };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program");
                return new List<Program>();
            }
        });
    }

    public async Task<Program?> GetCurrentProgramAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var currentProgram = _xmlService.GetProgram();
                _logger.LogDebug("Retrieved current program: {ProgramId}", currentProgram?.Id);
                return currentProgram;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current program");
                return null;
            }
        });
    }

    public async Task<Program> UpdateProgramAsync(Program program)
    {
        return await Task.Run(() =>
        {
            try
            {
                _xmlService.SetCurrentProgram(program);

                // Trigger auto-save
                if (IsAutoSaveEnabled)
                {
                    _pendingAutoSave = program;
                }

                ProgramUpdated?.Invoke(this, program);
                _logger.LogInformation("Updated program: {ProgramId} - {Description}", program.Id, program.Description);
                return program;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating program: {ProgramId}", program.Id);
                throw;
            }
        });
    }

    public async Task<bool> DeleteProgramAsync(int id)
    {
        return await Task.Run(() =>
        {
            try
            {
                var programToDelete = _xmlService.GetProgram();
                
                if (programToDelete.Id != id)
                {
                    _logger.LogWarning("Program not found for deletion: {ProgramId}", id);
                    return false;
                }

                if (programToDelete.IsPublished)
                {
                    _logger.LogWarning("Cannot delete published program: {ProgramId}", id);
                    return false;
                }

                // Cannot delete the single program - reset to default instead
                var defaultProgram = Program.CreateDefault();
                _xmlService.SetCurrentProgram(defaultProgram);

                _logger.LogInformation("Deleted program: {ProgramId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting program: {ProgramId}", id);
                return false;
            }
        });
    }

    public async Task SetCurrentProgramAsync(Program program)
    {
        await Task.Run(() =>
        {
            try
            {
                _xmlService.SetCurrentProgram(program);
                _logger.LogInformation("Set current program: {ProgramId} - {Description}", program.Id, program.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current program: {ProgramId}", program.Id);
                throw;
            }
        });
    }

    #endregion

    #region Program Meeting Management

    public async Task<ProgramMeeting> CreateProgramMeetingAsync(int programId, string name, DateTime programDate, 
        DateTime beginHour, DateTime endHour, int? siteId = null, string description = "")
    {
        return await Task.Run(() =>
        {
            try
            {
                var site = siteId.HasValue ? _xmlService.GetSites().FirstOrDefault(s => s.Id == siteId.Value) : null;
                
                var meeting = new ProgramMeeting
                {
                    Id = GenerateNewId(),
                    ProgramId = programId,
                    Name = name,
                    Description = description,
                    ProgramDate = programDate,
                    BeginHour = beginHour,
                    EndHour = endHour
                };

                var meetings = _xmlService.GetProgramMeetings().ToList();
                meetings.Add(meeting);
                _xmlService.UpdateProgramMeetings(meetings);

                _logger.LogInformation("Created program meeting: {MeetingId} - {Name}", meeting.Id, meeting.Name);
                return meeting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating program meeting: {Name}", name);
                throw;
            }
        });
    }

    public async Task<ProgramMeeting> UpdateProgramMeetingAsync(ProgramMeeting meeting)
    {
        return await Task.Run(() =>
        {
            try
            {
                var meetings = _xmlService.GetProgramMeetings().ToList();
                int existingIndex = meetings.FindIndex(m => m.Id == meeting.Id);
                
                if (existingIndex >= 0)
                {
                    meetings[existingIndex] = meeting;
                }
                else
                {
                    meetings.Add(meeting);
                }

                _xmlService.UpdateProgramMeetings(meetings);

                MeetingUpdated?.Invoke(this, meeting);
                _logger.LogInformation("Updated program meeting: {MeetingId} - {Name}", meeting.Id, meeting.Name);
                return meeting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating program meeting: {MeetingId}", meeting.Id);
                throw;
            }
        });
    }

    public async Task<bool> DeleteProgramMeetingAsync(int meetingId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var meetings = _xmlService.GetProgramMeetings().ToList();
                var meetingToDelete = meetings.FirstOrDefault(m => m.Id == meetingId);
                
                if (meetingToDelete == null)
                {
                    _logger.LogWarning("Program meeting not found for deletion: {MeetingId}", meetingId);
                    return false;
                }

                meetings.Remove(meetingToDelete);
                _xmlService.UpdateProgramMeetings(meetings);

                _logger.LogInformation("Deleted program meeting: {MeetingId}", meetingId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting program meeting: {MeetingId}", meetingId);
                return false;
            }
        });
    }

    public async Task<IEnumerable<ProgramMeeting>> GetProgramMeetingsAsync(int programId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var meetings = _xmlService.GetProgramMeetings().Where(m => m.ProgramId == programId);
                _logger.LogDebug("Retrieved {Count} meetings for program: {ProgramId}", meetings.Count(), programId);
                return meetings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving meetings for program: {ProgramId}", programId);
                return new List<ProgramMeeting>();
            }
        });
    }

    #endregion

    #region Program Slot Management

    public async Task<ProgramSlot> CreateProgramSlotAsync(int meetingId, string name, DateTime beginHour, 
        DateTime endHour, int raceFormatDetailId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var meetings = _xmlService.GetProgramMeetings().ToList();
                var meeting = meetings.FirstOrDefault(m => m.Id == meetingId);
                
                if (meeting == null)
                {
                    throw new ArgumentException($"Program meeting not found: {meetingId}");
                }

                var raceFormatDetail = _xmlService.GetRaceFormatConfigurations()
                    .SelectMany(config => config.RaceFormatDetails)
                    .FirstOrDefault(detail => detail.Id == raceFormatDetailId);

                if (raceFormatDetail == null)
                {
                    throw new ArgumentException($"Race format detail not found: {raceFormatDetailId}");
                }

                var slot = new ProgramSlot
                {
                    Id = GenerateNewId(),
                    ProgramMeetingId = meetingId,
                    Name = name,
                    BeginHour = beginHour,
                    EndHour = endHour,
                    RaceFormatDetailId = raceFormatDetailId,
                    RaceFormatDetail = raceFormatDetail
                };

                meeting.ProgramSlots.Add(slot);
                _xmlService.UpdateProgramMeetings(meetings);

                _logger.LogInformation("Created program slot: {SlotId} - {Name}", slot.Id, slot.Name);
                return slot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating program slot: {Name}", name);
                throw;
            }
        });
    }

    public async Task<ProgramSlot> UpdateProgramSlotAsync(ProgramSlot slot)
    {
        return await Task.Run(() =>
        {
            try
            {
                var meetings = _xmlService.GetProgramMeetings().ToList();
                var meeting = meetings.FirstOrDefault(m => m.Id == slot.ProgramMeetingId);
                
                if (meeting == null)
                {
                    throw new ArgumentException($"Program meeting not found: {slot.ProgramMeetingId}");
                }

                int existingSlotIndex = meeting.ProgramSlots.ToList().FindIndex(s => s.Id == slot.Id);
                if (existingSlotIndex >= 0)
                {
                    var slotsList = meeting.ProgramSlots.ToList();
                    slotsList[existingSlotIndex] = slot;
                    meeting.ProgramSlots = slotsList;
                }
                else
                {
                    meeting.ProgramSlots.Add(slot);
                }

                _xmlService.UpdateProgramMeetings(meetings);

                _logger.LogInformation("Updated program slot: {SlotId} - {Name}", slot.Id, slot.Name);
                return slot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating program slot: {SlotId}", slot.Id);
                throw;
            }
        });
    }

    public async Task<bool> DeleteProgramSlotAsync(int slotId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var meetings = _xmlService.GetProgramMeetings().ToList();
                
                foreach (var meeting in meetings)
                {
                    var slotToDelete = meeting.ProgramSlots.FirstOrDefault(s => s.Id == slotId);
                    if (slotToDelete != null)
                    {
                        var slotsList = meeting.ProgramSlots.ToList();
                        slotsList.Remove(slotToDelete);
                        meeting.ProgramSlots = slotsList;
                        
                        _xmlService.UpdateProgramMeetings(meetings);
                        _logger.LogInformation("Deleted program slot: {SlotId}", slotId);
                        return true;
                    }
                }

                _logger.LogWarning("Program slot not found for deletion: {SlotId}", slotId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting program slot: {SlotId}", slotId);
                return false;
            }
        });
    }

    public async Task<ProgramSlot> CreateManualEventAsync(int meetingId, string name, DateTime beginHour, 
        int durationMinutes, string eventType = "Manual")
    {
        return await Task.Run(() =>
        {
            try
            {
                var endHour = beginHour.AddMinutes(durationMinutes);
                
                // Create a dummy race format detail for manual events
                var manualRaceFormatDetail = new RaceFormatDetail
                {
                    Id = GenerateNewId(),
                    Label = $"Manual - {eventType}",
                    Order = 0
                };

                var slot = new ProgramSlot
                {
                    Id = GenerateNewId(),
                    ProgramMeetingId = meetingId,
                    Name = name,
                    BeginHour = beginHour,
                    EndHour = endHour,
                    RaceFormatDetailId = manualRaceFormatDetail.Id,
                    RaceFormatDetail = manualRaceFormatDetail
                };

                var meetings = _xmlService.GetProgramMeetings().ToList();
                var meeting = meetings.FirstOrDefault(m => m.Id == meetingId);
                
                if (meeting == null)
                {
                    throw new ArgumentException($"Program meeting not found: {meetingId}");
                }

                meeting.ProgramSlots.Add(slot);
                _xmlService.UpdateProgramMeetings(meetings);

                _logger.LogInformation("Created manual event slot: {SlotId} - {Name}", slot.Id, slot.Name);
                return slot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual event: {Name}", name);
                throw;
            }
        });
    }

    #endregion

    #region Publishing

    public async Task<Program> PublishProgramAsync(int programId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var program = _xmlService.GetProgram();
                
                if (program.Id != programId)
                {
                    throw new ArgumentException($"Program not found: {programId}");
                }

                if (!program.CanBePublished)
                {
                    throw new InvalidOperationException($"Program cannot be published in its current state: {program.Status}");
                }

                program.Publish();
                _xmlService.SetCurrentProgram(program);

                ProgramPublished?.Invoke(this, program);
                _logger.LogInformation("Published program: {ProgramId} - Version {Version}", program.Id, program.Version);
                return program;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing program: {ProgramId}", programId);
                throw;
            }
        });
    }

    public async Task<Program> UnpublishProgramAsync(int programId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var program = _xmlService.GetProgram();
                
                if (program.Id != programId)
                {
                    throw new ArgumentException($"Program not found: {programId}");
                }

                program.Status = ScheduleStatus.Draft;
                _xmlService.SetCurrentProgram(program);

                _logger.LogInformation("Unpublished program: {ProgramId}", program.Id);
                return program;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing program: {ProgramId}", programId);
                throw;
            }
        });
    }

    public async Task<Program> ArchiveProgramAsync(int programId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var program = _xmlService.GetProgram();
                
                if (program.Id != programId)
                {
                    throw new ArgumentException($"Program not found: {programId}");
                }

                program.Archive();
                _xmlService.SetCurrentProgram(program);

                _logger.LogInformation("Archived program: {ProgramId}", program.Id);
                return program;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving program: {ProgramId}", programId);
                throw;
            }
        });
    }

    #endregion

    #region Auto-save and Bulk Operations

    public async Task AutoSaveProgramAsync(Program program)
    {
        if (!IsAutoSaveEnabled)
            { return; }

        await _autoSaveSemaphore.WaitAsync();
        try
        {
            _xmlService.Save();
            _logger.LogDebug("Auto-saved program: {ProgramId}", program.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-save for program: {ProgramId}", program.Id);
        }
        finally
        {
            _autoSaveSemaphore.Release();
        }
    }

    public async Task<IEnumerable<ProgramMeeting>> BulkUpdateMeetingsAsync(IEnumerable<ProgramMeeting> meetings)
    {
        return await Task.Run(() =>
        {
            try
            {
                var allMeetings = _xmlService.GetProgramMeetings().ToList();
                
                foreach (var meeting in meetings)
                {
                    int existingIndex = allMeetings.FindIndex(m => m.Id == meeting.Id);
                    if (existingIndex >= 0)
                    {
                        allMeetings[existingIndex] = meeting;
                    }
                    else
                    {
                        allMeetings.Add(meeting);
                    }
                }

                _xmlService.UpdateProgramMeetings(allMeetings);
                _logger.LogInformation("Bulk updated {Count} meetings", meetings.Count());
                return meetings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk update of meetings");
                throw;
            }
        });
    }

    public async Task<bool> BulkDeleteMeetingsAsync(IEnumerable<int> meetingIds)
    {
        return await Task.Run(() =>
        {
            try
            {
                var allMeetings = _xmlService.GetProgramMeetings().ToList();
                var idsToDelete = meetingIds.ToHashSet();
                
                allMeetings.RemoveAll(m => idsToDelete.Contains(m.Id));
                _xmlService.UpdateProgramMeetings(allMeetings);

                _logger.LogInformation("Bulk deleted {Count} meetings", meetingIds.Count());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk delete of meetings");
                return false;
            }
        });
    }

    public async Task<Program> CreateNewVersionAsync(int programId, string description = "")
    {
        return await Task.Run(() =>
        {
            try
            {
                var sourceProgram = _xmlService.GetProgram();
                if (sourceProgram.Id != programId)
                {
                    throw new ArgumentException($"Source program not found: {programId}");
                }

                var newProgram = new Program
                {
                    Id = sourceProgram.Id, // Keep same ID for single program
                    Description = string.IsNullOrEmpty(description) ? sourceProgram.Description : description,
                    Status = ScheduleStatus.Draft,
                    CreatedDate = DateTime.Now,
                    Version = sourceProgram.Version + 1,
                    Sites = sourceProgram.Sites
                };

                _xmlService.SetCurrentProgram(newProgram);

                _logger.LogInformation("Created new version of program: {SourceId} -> {NewId}", programId, newProgram.Id);
                return newProgram;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version of program: {ProgramId}", programId);
                throw;
            }
        });
    }

    public async Task<IEnumerable<Program>> GetProgramVersionsAsync(int baseProgramId)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Return the current program as the only version available
                var program = _xmlService.GetProgram();
                _logger.LogDebug("Retrieved program versions for base: {BaseProgramId}", baseProgramId);
                return new List<Program> { program };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving program versions: {BaseProgramId}", baseProgramId);
                return new List<Program>();
            }
        });
    }

    #endregion

    #region Private Methods

    private void AutoSaveCallback(object? state)
    {
        if (_pendingAutoSave != null && IsAutoSaveEnabled)
        {
            Task.Run(async () => await AutoSaveProgramAsync(_pendingAutoSave));
            _pendingAutoSave = null;
        }
    }

    private int GenerateNewId()
    {
        // Simple ID generation - in a real application, this would be handled by the database
        var random = new Random();
        return random.Next(10000, 99999);
    }

    public void Dispose()
    {
        _autoSaveTimer?.Dispose();
        _autoSaveSemaphore?.Dispose();
    }

    #endregion
}
