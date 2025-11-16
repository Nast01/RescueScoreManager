using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RescueScoreManager.Data;

namespace RescueScoreManager.Modules.Planning.ViewModels;

public partial class SiteViewModel : ObservableObject
{
    private readonly Site _site;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _hasConflicts;

    public int Id => _site.Id;
    public string Name => _site.Name;
    public string Description => _site.Description ?? string.Empty;
    public string Icon => _site.Icon;

    public ObservableCollection<ProgramMeetingViewModel> ProgramMeetings { get; } = new();

    public SiteViewModel(Site site)
    {
        _site = site ?? throw new ArgumentNullException(nameof(site));
    }

    public Site GetModel() => _site;

    public void AddMeeting(ProgramMeetingViewModel meeting)
    {
        if (!ProgramMeetings.Contains(meeting))
        {
            ProgramMeetings.Add(meeting);
        }
    }

    public void RemoveMeeting(ProgramMeetingViewModel meeting)
    {
        ProgramMeetings.Remove(meeting);
    }

    public bool HasTimeConflict(DateTime beginTime, DateTime endTime, int? excludeMeetingId = null)
    {
        return ProgramMeetings.Any(m => 
            m.Id != excludeMeetingId &&
            m.BeginHour < endTime && 
            m.EndHour > beginTime);
    }

    public override bool Equals(object? obj)
    {
        return obj is SiteViewModel other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}