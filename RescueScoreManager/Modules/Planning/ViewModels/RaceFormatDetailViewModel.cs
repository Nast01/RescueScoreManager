using CommunityToolkit.Mvvm.ComponentModel;
using RescueScoreManager.Data;
using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Modules.Planning.ViewModels;

public partial class RaceFormatDetailViewModel : ObservableObject
{
    private readonly RaceFormatDetail _raceFormatDetail;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isDragging;

    [ObservableProperty]
    private bool _isScheduled;

    public int Id => _raceFormatDetail.Id;
    public int Order => _raceFormatDetail.Order;
    public string Label => _raceFormatDetail.Label;
    public string FullLabel => _raceFormatDetail.FullLabel;
    public string LevelLabel => _raceFormatDetail.LevelLabel;
    public HeatLevel Level => _raceFormatDetail.Level;
    public int NumberOfRun => _raceFormatDetail.NumberOfRun;
    public QualificationType QualificationMethod => _raceFormatDetail.QualificationMethod;
    public string QualificationMethodLabel => _raceFormatDetail.QualificationMethodLabel;
    public int SpotsPerRace => _raceFormatDetail.SpotsPerRace;
    public int QualifyingSpots => _raceFormatDetail.QualifyingSpots;
    public RaceFormatConfiguration RaceFormatConfiguration => _raceFormatDetail.RaceFormatConfiguration;

    // UI-specific properties
    public string DisplayText => $"{Label} ({NumberOfRun} run{(NumberOfRun > 1 ? "s" : "")})";
    public string ShortDisplayText => LevelLabel;
    public string ToolTipText => $"{FullLabel}\n{QualificationMethodLabel}: {QualifyingSpots} spots\n{SpotsPerRace} per race";
    
    public string LevelColor => Level switch
    {
        HeatLevel.Heat => "#2196F3",      // Blue
        HeatLevel.Quarter => "#FF9800",    // Orange
        HeatLevel.Semi => "#F44336",       // Red
        HeatLevel.Final => "#4CAF50",      // Green
        _ => "#9E9E9E"                     // Gray
    };

    public string GenderColor => RaceFormatConfiguration.Gender switch
    {
        Gender.Men => "#1976D2",           // Dark Blue
        Gender.Woman => "#E91E63",         // Pink
        Gender.Mixte => "#9C27B0",         // Purple
        _ => "#9E9E9E"                     // Gray
    };

    public int EstimatedDurationMinutes => CalculateEstimatedDuration();

    public RaceFormatDetailViewModel(RaceFormatDetail raceFormatDetail)
    {
        _raceFormatDetail = raceFormatDetail ?? throw new ArgumentNullException(nameof(raceFormatDetail));
    }

    public RaceFormatDetail GetModel() => _raceFormatDetail;

    private int CalculateEstimatedDuration()
    {
        // Basic estimation based on race type and number of runs
        // This could be made more sophisticated based on actual race data
        
        var baseTime = Level switch
        {
            HeatLevel.Heat => 5,        // 5 minutes per heat
            HeatLevel.Quarter => 8,     // 8 minutes per quarter
            HeatLevel.Semi => 10,       // 10 minutes per semi
            HeatLevel.Final => 15,      // 15 minutes per final
            _ => 5
        };
        
        // Multiply by number of runs and add setup time
        var totalTime = (baseTime * NumberOfRun) + 5; // 5 minutes setup
        
        return Math.Max(totalTime, 10); // Minimum 10 minutes
    }

    public bool CanBeScheduledAt(DateTime startTime, DateTime endTime)
    {
        // Basic validation - could be enhanced with more business rules
        var requiredDuration = TimeSpan.FromMinutes(EstimatedDurationMinutes);
        var availableDuration = endTime - startTime;
        
        return availableDuration >= requiredDuration;
    }

    public DateTime GetProposedEndTime(DateTime startTime)
    {
        return startTime.AddMinutes(EstimatedDurationMinutes);
    }

    public override bool Equals(object? obj)
    {
        return obj is RaceFormatDetailViewModel other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return DisplayText;
    }
}