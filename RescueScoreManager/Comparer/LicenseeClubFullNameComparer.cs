using RescueScoreManager.Data;
using RescueScoreManager.Helpers;

namespace RescueScoreManager.Comparer;

public class LicenseeClubFullNameComparer : IComparer<Licensee>
{
    public int Compare(Licensee? lic1, Licensee? lic2)
    {
        string key1 = TextHelper.RemoveDiacritics(lic1.Club.Name.ToLowerInvariant() + lic1.FullName.ToLowerInvariant());
        string key2 = TextHelper.RemoveDiacritics(lic2.Club.Name.ToLowerInvariant() + lic2.FullName.ToLowerInvariant());

        return key1.CompareTo(key2);
    }
}