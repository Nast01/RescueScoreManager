using Microsoft.Extensions.Logging;
using RescueScoreManager.Data;
using System.Diagnostics;

namespace RescueScoreManager.Services;

public class ProgramValidationService : IProgramValidationService
{
    private readonly IXMLService _xmlService;
    private readonly IProgramService _programService;
    private readonly ILogger<ProgramValidationService> _logger;
    
    public TimeSpan LastValidationDuration { get; private set; }
    public int LastValidationEventCount { get; private set; }

    public ProgramValidationService(IXMLService xmlService, IProgramService programService, ILogger<ProgramValidationService> logger)
    {
        _xmlService = xmlService ?? throw new ArgumentNullException(nameof(xmlService));
        _programService = programService ?? throw new ArgumentNullException(nameof(programService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Main Validation Methods

    public async Task<ProgramValidationResult> ValidateProgramAsync(Program program)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ProgramValidationResult();

        try
        {
            _logger.LogDebug("Starting validation for program: {ProgramId}", program.Id);

            // Run all validation checks
            var timeOverlaps = await CheckTimeOverlapsAsync(program);
            var siteConflicts = await CheckSiteConflictsAsync(program);
            var restTimeIssues = await CheckAthleteRestTimeAsync(program);
            var bufferTimeIssues = await CheckMinimumBufferTimeAsync(program);

            // Combine all conflicts
            result.Conflicts.AddRange(timeOverlaps);
            result.Conflicts.AddRange(siteConflicts);
            result.Conflicts.AddRange(restTimeIssues);
            result.Conflicts.AddRange(bufferTimeIssues);

            // Add warnings
            await AddWarningsToResult(program, result);

            // Count events
            var programMeetings = await _programService.GetProgramMeetingsAsync(program.Id);
            result.EventCount = programMeetings
                .SelectMany(m => m.ProgramSlots)
                .Count();

            result.IsValid = !result.Conflicts.Any(c => c.Severity == ConflictSeverity.Critical);

            stopwatch.Stop();
            result.ValidationDuration = stopwatch.Elapsed;
            LastValidationDuration = result.ValidationDuration;
            LastValidationEventCount = result.EventCount;

            _logger.LogInformation("Program validation completed: Valid={IsValid}, Conflicts={ConflictCount}, Warnings={WarningCount}, Duration={Duration}ms", 
                result.IsValid, result.Conflicts.Count, result.Warnings.Count, result.ValidationDuration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during program validation: {ProgramId}", program.Id);
            result.IsValid = false;
            result.Conflicts.Add(new ConflictResult
            {
                Type = ConflictType.MissingRequiredData,
                Message = "Validation error occurred",
                DetailedMessage = ex.Message,
                Severity = ConflictSeverity.Critical
            });
            return result;
        }
    }

    public async Task<ProgramValidationResult> ValidateProgramMeetingAsync(ProgramMeeting meeting)
    {
        return await Task.Run(() =>
        {
            var result = new ProgramValidationResult();

            try
            {
                // Check meeting time validity
                if (meeting.BeginHour >= meeting.EndHour)
                {
                    result.Conflicts.Add(new ConflictResult
                    {
                        Type = ConflictType.InvalidTimeRange,
                        Message = "Meeting end time must be after begin time",
                        DetailedMessage = $"Meeting '{meeting.Name}' has invalid time range: {meeting.BeginHour:HH:mm} - {meeting.EndHour:HH:mm}",
                        Meeting1 = meeting,
                        Severity = ConflictSeverity.Critical
                    });
                }

                // Check if slots fit within meeting timeframe
                foreach (var slot in meeting.ProgramSlots)
                {
                    if (slot.BeginHour < meeting.BeginHour || slot.EndHour > meeting.EndHour)
                    {
                        result.Conflicts.Add(new ConflictResult
                        {
                            Type = ConflictType.InvalidTimeRange,
                            Message = "Slot time extends beyond meeting timeframe",
                            DetailedMessage = $"Slot '{slot.Name}' ({slot.BeginHour:HH:mm}-{slot.EndHour:HH:mm}) extends beyond meeting '{meeting.Name}' ({meeting.BeginHour:HH:mm}-{meeting.EndHour:HH:mm})",
                            Meeting1 = meeting,
                            Slot1 = slot,
                            Severity = ConflictSeverity.High
                        });
                    }
                }

                result.IsValid = !result.Conflicts.Any(c => c.Severity == ConflictSeverity.Critical);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating program meeting: {MeetingId}", meeting.Id);
                result.IsValid = false;
                return result;
            }
        });
    }

    public async Task<ProgramValidationResult> ValidateProgramSlotAsync(ProgramSlot slot)
    {
        return await Task.Run(() =>
        {
            var result = new ProgramValidationResult();

            try
            {
                // Check slot time validity
                if (slot.BeginHour >= slot.EndHour)
                {
                    result.Conflicts.Add(new ConflictResult
                    {
                        Type = ConflictType.InvalidTimeRange,
                        Message = "Slot end time must be after begin time",
                        DetailedMessage = $"Slot '{slot.Name}' has invalid time range: {slot.BeginHour:HH:mm} - {slot.EndHour:HH:mm}",
                        Slot1 = slot,
                        Severity = ConflictSeverity.Critical
                    });
                }

                // Check minimum duration (e.g., 5 minutes)
                var duration = slot.EndHour - slot.BeginHour;
                if (duration.TotalMinutes < 5)
                {
                    result.Warnings.Add(new WarningResult
                    {
                        Type = WarningType.ShortRestTime,
                        Message = "Very short event duration",
                        Slot = slot
                    });
                }

                // Check maximum duration (e.g., 4 hours)
                if (duration.TotalHours > 4)
                {
                    result.Warnings.Add(new WarningResult
                    {
                        Type = WarningType.LongEvent,
                        Message = "Very long event duration",
                        Slot = slot
                    });
                }

                result.IsValid = !result.Conflicts.Any(c => c.Severity == ConflictSeverity.Critical);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating program slot: {SlotId}", slot.Id);
                result.IsValid = false;
                return result;
            }
        });
    }

    #endregion

    #region Specific Validation Checks

    public async Task<IEnumerable<ConflictResult>> CheckTimeOverlapsAsync(Program program)
    {
        var conflicts = new List<ConflictResult>();

        try
        {
            var programMeetings = await _programService.GetProgramMeetingsAsync(program.Id);
            var allSlots = programMeetings
                .SelectMany(m => m.ProgramSlots.Select(s => new { Meeting = m, Slot = s }))
                .OrderBy(x => x.Slot.BeginHour)
                .ToList();

                for (int i = 0; i < allSlots.Count; i++)
                {
                    for (int j = i + 1; j < allSlots.Count; j++)
                    {
                        var slot1 = allSlots[i];
                        var slot2 = allSlots[j];

                        // Check if slots overlap in time
                        if (slot1.Slot.BeginHour < slot2.Slot.EndHour && slot2.Slot.BeginHour < slot1.Slot.EndHour)
                        {
                            conflicts.Add(new ConflictResult
                            {
                                Type = ConflictType.TimeOverlap,
                                Message = "Time overlap between events",
                                DetailedMessage = $"'{slot1.Slot.Name}' ({slot1.Slot.BeginHour:HH:mm}-{slot1.Slot.EndHour:HH:mm}) overlaps with '{slot2.Slot.Name}' ({slot2.Slot.BeginHour:HH:mm}-{slot2.Slot.EndHour:HH:mm})",
                                Meeting1 = slot1.Meeting,
                                Meeting2 = slot2.Meeting,
                                Slot1 = slot1.Slot,
                                Slot2 = slot2.Slot,
                                ConflictStart = new[] { slot1.Slot.BeginHour, slot2.Slot.BeginHour }.Max(),
                                ConflictEnd = new[] { slot1.Slot.EndHour, slot2.Slot.EndHour }.Min(),
                                Severity = ConflictSeverity.High
                            });
                        }
                    }
                }

            return conflicts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking time overlaps for program: {ProgramId}", program.Id);
            return conflicts;
        }
    }

    public async Task<IEnumerable<ConflictResult>> CheckSiteConflictsAsync(Program program)
    {
        var conflicts = new List<ConflictResult>();

        try
        {
            var programMeetings = await _programService.GetProgramMeetingsAsync(program.Id);
            // Note: Site-based validation removed as ProgramMeeting no longer has Site dependency
            // Simplified validation for meeting time conflicts
            var meetings = programMeetings.OrderBy(m => m.BeginHour).ToList();
            
            for (int i = 0; i < meetings.Count; i++)
            {
                for (int j = i + 1; j < meetings.Count; j++)
                {
                    var meeting1 = meetings[i];
                    var meeting2 = meetings[j];

                    // Check if meetings have overlapping time slots on the same date
                    if (meeting1.ProgramDate.Date == meeting2.ProgramDate.Date &&
                        meeting1.BeginHour < meeting2.EndHour && meeting2.BeginHour < meeting1.EndHour)
                    {
                        conflicts.Add(new ConflictResult
                        {
                            Type = ConflictType.TimeConflict,
                            Message = "Time conflict",
                            DetailedMessage = $"Time conflict: '{meeting1.Name}' ({meeting1.BeginHour:HH:mm}-{meeting1.EndHour:HH:mm}) and '{meeting2.Name}' ({meeting2.BeginHour:HH:mm}-{meeting2.EndHour:HH:mm}) have overlapping times on {meeting1.ProgramDate:dd/MM/yyyy}",
                            Meeting1 = meeting1,
                            Meeting2 = meeting2,
                            ConflictStart = new[] { meeting1.BeginHour, meeting2.BeginHour }.Max(),
                            ConflictEnd = new[] { meeting1.EndHour, meeting2.EndHour }.Min(),
                            Severity = ConflictSeverity.Critical
                        });
                    }
                }
            }

            return conflicts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking site conflicts for program: {ProgramId}", program.Id);
            return conflicts;
        }
    }

    public async Task<IEnumerable<ConflictResult>> CheckAthleteRestTimeAsync(Program program, int minimumRestMinutes = 15)
    {
        return await Task.Run(() =>
        {
            var conflicts = new List<ConflictResult>();

            try
            {
                // This would require athlete participation data, which isn't available in the current structure
                // For now, return empty conflicts - this can be enhanced when athlete scheduling is implemented
                _logger.LogDebug("Athlete rest time validation not implemented - requires athlete participation data");
                return conflicts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking athlete rest time for program: {ProgramId}", program.Id);
                return conflicts;
            }
        });
    }

    public async Task<IEnumerable<ConflictResult>> CheckMinimumBufferTimeAsync(Program program, int bufferMinutes = 5)
    {
        var conflicts = new List<ConflictResult>();

        try
        {
            var programMeetings = await _programService.GetProgramMeetingsAsync(program.Id);
            // Note: Buffer time validation simplified without Site dependency
            var meetings = programMeetings.OrderBy(m => m.BeginHour).ToList();

            for (int i = 0; i < meetings.Count - 1; i++)
            {
                var currentMeeting = meetings[i];
                var nextMeeting = meetings[i + 1];

                // Only check buffer time for meetings on the same date
                if (currentMeeting.ProgramDate.Date == nextMeeting.ProgramDate.Date)
                {
                    var timeBetween = nextMeeting.BeginHour - currentMeeting.EndHour;
                    if (timeBetween.TotalMinutes < bufferMinutes && timeBetween.TotalMinutes >= 0)
                    {
                        conflicts.Add(new ConflictResult
                        {
                            Type = ConflictType.InsufficientBufferTime,
                            Message = $"Insufficient buffer time ({timeBetween.TotalMinutes:F0} minutes)",
                            DetailedMessage = $"Only {timeBetween.TotalMinutes:F0} minutes between '{currentMeeting.Name}' and '{nextMeeting.Name}' (minimum {bufferMinutes} minutes required)",
                            Meeting1 = currentMeeting,
                            Meeting2 = nextMeeting,
                            ConflictStart = currentMeeting.EndHour,
                            ConflictEnd = nextMeeting.BeginHour,
                            Severity = ConflictSeverity.Medium
                        });
                    }
                }
            }

            return conflicts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking buffer time for program: {ProgramId}", program.Id);
            return conflicts;
        }
    }

    #endregion

    #region Real-time Validation

    public async Task<ProgramValidationResult> ValidateTimeSlotChangeAsync(ProgramSlot slot, DateTime newBeginTime, DateTime newEndTime)
    {
        return await Task.Run(() =>
        {
            var result = new ProgramValidationResult();

            try
            {
                // Create a temporary slot with new times for validation
                var tempSlot = new ProgramSlot
                {
                    Id = slot.Id,
                    Name = slot.Name,
                    BeginHour = newBeginTime,
                    EndHour = newEndTime,
                    ProgramMeetingId = slot.ProgramMeetingId,
                    RaceFormatDetailId = slot.RaceFormatDetailId
                };

                // Validate the temporary slot
                return ValidateProgramSlotAsync(tempSlot).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating time slot change for slot: {SlotId}", slot.Id);
                result.IsValid = false;
                return result;
            }
        });
    }

    public async Task<ProgramValidationResult> ValidateMeetingMoveAsync(ProgramMeeting meeting, DateTime newBeginTime, DateTime newEndTime)
    {
        return await Task.Run(() =>
        {
            var result = new ProgramValidationResult();

            try
            {
                // Create a temporary meeting with new times for validation
                var tempMeeting = new ProgramMeeting
                {
                    Id = meeting.Id,
                    Name = meeting.Name,
                    BeginHour = newBeginTime,
                    EndHour = newEndTime,
                    ProgramDate = meeting.ProgramDate,
                    ProgramSlots = meeting.ProgramSlots
                };

                // Validate the temporary meeting
                return ValidateProgramMeetingAsync(tempMeeting).Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating meeting move for meeting: {MeetingId}", meeting.Id);
                result.IsValid = false;
                return result;
            }
        });
    }

    public async Task<ProgramValidationResult> ValidateSiteChangeAsync(ProgramMeeting meeting, int newSiteId)
    {
        return await Task.Run(() =>
        {
            var result = new ProgramValidationResult();

            try
            {
                // Site validation removed - meetings no longer have site dependency
                result.Warnings.Add(new WarningResult
                {
                    Type = WarningType.DeprecatedFeature,
                    Message = "Site change validation is deprecated",
                    Meeting = meeting
                });

                result.IsValid = !result.Conflicts.Any(c => c.Severity == ConflictSeverity.Critical);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating site change for meeting: {MeetingId}", meeting.Id);
                result.IsValid = false;
                return result;
            }
        });
    }

    #endregion

    #region Business Rules

    public async Task<bool> CanPublishProgramAsync(Program program)
    {
        var validationResult = await ValidateProgramAsync(program);
        return validationResult.IsValid && !validationResult.Conflicts.Any(c => c.Severity == ConflictSeverity.Critical);
    }

    public async Task<string> GetPublishValidationMessageAsync(Program program)
    {
        var validationResult = await ValidateProgramAsync(program);
        
        if (validationResult.IsValid)
        {
            return "Program is ready to publish.";
        }

        var criticalIssues = validationResult.Conflicts.Where(c => c.Severity == ConflictSeverity.Critical).ToList();
        if (criticalIssues.Any())
        {
            return $"Cannot publish: {criticalIssues.Count} critical issue(s) must be resolved.";
        }

        var highIssues = validationResult.Conflicts.Where(c => c.Severity == ConflictSeverity.High).ToList();
        if (highIssues.Any())
        {
            return $"Warning: {highIssues.Count} high-priority issue(s) should be resolved before publishing.";
        }

        return "Program can be published with warnings.";
    }

    #endregion

    #region Private Methods

    private async Task AddWarningsToResult(Program program, ProgramValidationResult result)
    {
        try
        {
            var programMeetings = await _programService.GetProgramMeetingsAsync(program.Id);
            // Site assignment warnings removed - meetings no longer require sites

            // Check for very short or very long meetings
            foreach (var meeting in programMeetings)
                {
                    var duration = meeting.EndHour - meeting.BeginHour;
                    
                    if (duration.TotalMinutes < 30)
                    {
                        result.Warnings.Add(new WarningResult
                        {
                            Type = WarningType.ShortRestTime,
                            Message = "Very short meeting duration",
                            Meeting = meeting
                        });
                    }
                    else if (duration.TotalHours > 8)
                    {
                        result.Warnings.Add(new WarningResult
                        {
                            Type = WarningType.LongEvent,
                            Message = "Very long meeting duration",
                            Meeting = meeting
                        });
                    }
                }

            // Check for empty meetings (no slots)
            foreach (var meeting in programMeetings.Where(m => !m.ProgramSlots.Any()))
                {
                    result.Warnings.Add(new WarningResult
                    {
                        Type = WarningType.MissingParticipants,
                        Message = "Meeting has no program slots",
                        Meeting = meeting
                    });
                }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding warnings to validation result");
        }
    }

    #endregion
}