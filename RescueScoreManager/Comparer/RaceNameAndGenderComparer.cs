using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer;

public class RaceNameAndGenderComparer : IComparer<Race>
{
    public int Compare(Race? race1, Race? race2)
    {
        string key1 = ""; 
        string key2 = ""; 

        if(race1.Categories.Count == 1)
        {
            key1 = (race1.Name + race1.Categories.First().Name + race1.Gender.ToString()).ToLower();
        }
        else
        {
            key1 = (race1.Name + race1.Gender.ToString()).ToLower();
        }

        if (race2.Categories.Count == 1)
        {
            key2 = (race2.Name + race2.Categories.First().Name + race2.Gender.ToString()).ToLower();
        }
        else
        {
            key2 = (race2.Name + race2.Gender.ToString()).ToLower();
        }

        return key1.CompareTo(key2);
    }
}
