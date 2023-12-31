using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer;

public class CategoryComparer : IComparer<Category>
{
    public int Compare(Category? c1, Category? c2)
    {
        return (c1 != null & c2 != null) ? c1.AgeMin.CompareTo(c2.AgeMin) : 1;
    }
}
