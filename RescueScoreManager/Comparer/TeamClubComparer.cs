using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer;

public class TeamClubComparer : IComparer<Team>
{
    public int Compare(Team? team1, Team? team2)
    {
        string key1 = "";
        string key2 = "";

        if (team1 != null && team2 != null)
        {
            if (team1 is IndividualTeam && team2 is IndividualTeam)
            {
                key1 = (team1 as IndividualTeam).Athlete.Club.Name.ToLower();
                key2 = (team2 as IndividualTeam).Athlete.Club.Name.ToLower();
            }
            else if (team1 is RelayTeam && team2 is RelayTeam)
            {
                key1 = (team1 as RelayTeam).GetClub().Name.ToLower();
                key2 = (team2 as RelayTeam).GetClub().Name.ToLower();
            }
        }
        return key1.CompareTo(key2);
    }
}
