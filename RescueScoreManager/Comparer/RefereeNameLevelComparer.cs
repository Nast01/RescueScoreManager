using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer;

public class RefereeNameLevelComparer : IComparer<Referee>
{
    public int Compare(Referee? ref1, Referee? ref2)
    {
        string key1 = (ref1.RefereeLevel + ref1.FullName).ToLower();
        string key2 = (ref2.RefereeLevel + ref2.FullName).ToLower();
        return key1.CompareTo(key2);
    }
}
