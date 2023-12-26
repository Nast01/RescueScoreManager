using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer;

public class ClubNameComparer : IComparer<Club>
{
    public int Compare(Club? c1, Club? c2)
    {
        return (c1 != null & c2 != null) ? c1.Name.ToLowerInvariant().CompareTo(c2.Name.ToLowerInvariant()) : 1;
    }
}
