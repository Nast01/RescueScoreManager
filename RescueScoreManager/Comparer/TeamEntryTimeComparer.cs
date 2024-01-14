using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer;

public class TeamEntryTimeComparer : IComparer<Team>
{
    public int Compare(Team? team1, Team? team2)
    {
        return team1.EntryTime.CompareTo(team2.EntryTime);
    }
}
