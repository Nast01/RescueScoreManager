using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer;

public class LicenseeFullNameComparer : IComparer<Licensee>
{
    public int Compare(Licensee? lic1, Licensee? lic2)
    {
        return (lic1 != null & lic2 != null) ? lic1.FullName.ToLowerInvariant().CompareTo(lic2.FullName.ToLowerInvariant()) : 1;
    }
}
