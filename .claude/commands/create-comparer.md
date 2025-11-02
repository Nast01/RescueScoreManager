# Create Comparer

Creates a custom comparer class following RescueScoreManager patterns for sorting entities with proper null handling and performance optimization.

## Parameters
- `comparer_name` (required): Name of the comparer (e.g., "AthletePerformance", "EventPriority", "SiteCapacity")
- `entity_type` (required): Type being compared (e.g., "Athlete", "Race", "Site", "Competition")
- `sort_criteria` (required): Comma-separated list of sort criteria (e.g., "Name,Score:desc,Date")
- `comparison_logic` (optional): Description of special comparison logic

## Usage
```
@create-comparer comparer_name="AthletePerformance" entity_type="Athlete" sort_criteria="Score:desc,Name,Age" comparison_logic="Score descending, then name alphabetically, then age ascending"
```

## Implementation Steps

1. **Create Comparer Class**: `RescueScoreManager/Comparer/{comparer_name}Comparer.cs`
   - Implement IComparer<T> interface
   - Add proper null handling
   - Include multi-level sorting logic
   - Add performance optimizations

2. **Add Unit Tests**: Create test class for the comparer
   - Test null handling
   - Test sort criteria
   - Test edge cases

3. **Update Documentation**: Add usage examples

## Template Structure

### Comparer Class Template:
```csharp
using System;
using System.Collections.Generic;
using RescueScoreManager.Data;

namespace RescueScoreManager.Comparer
{
    /// <summary>
    /// Comparer for {entity_type} entities based on {comparison_logic}
    /// Sort criteria: {sort_criteria}
    /// </summary>
    public class {comparer_name}Comparer : IComparer<{entity_type}>
    {
        private readonly bool _descending;
        private readonly StringComparison _stringComparison;

        /// <summary>
        /// Initializes a new instance of the {comparer_name}Comparer class
        /// </summary>
        /// <param name="descending">Whether to sort in descending order</param>
        /// <param name="stringComparison">String comparison method for text fields</param>
        public {comparer_name}Comparer(bool descending = false, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            _descending = descending;
            _stringComparison = stringComparison;
        }

        /// <summary>
        /// Compares two {entity_type} instances
        /// </summary>
        /// <param name="x">First {entity_type} to compare</param>
        /// <param name="y">Second {entity_type} to compare</param>
        /// <returns>Comparison result</returns>
        public int Compare({entity_type}? x, {entity_type}? y)
        {
            // Handle null cases first
            if (x == null && y == null) return 0;
            if (x == null) return _descending ? 1 : -1;
            if (y == null) return _descending ? -1 : 1;

            int result = CompareInternal(x, y);
            return _descending ? -result : result;
        }

        /// <summary>
        /// Internal comparison logic
        /// </summary>
        private int CompareInternal({entity_type} x, {entity_type} y)
        {
            // Multi-level comparison based on sort_criteria
            
            // Primary sort criterion (example: Score)
            int result = CompareScores(x, y);
            if (result != 0) return result;

            // Secondary sort criterion (example: Name)
            result = CompareNames(x, y);
            if (result != 0) return result;

            // Tertiary sort criterion (example: Age)
            result = CompareAges(x, y);
            if (result != 0) return result;

            // Final fallback - compare by ID for consistent ordering
            return x.Id.CompareTo(y.Id);
        }

        /// <summary>
        /// Compares entities by score (or primary numeric criterion)
        /// </summary>
        private int CompareScores({entity_type} x, {entity_type} y)
        {
            // Example for numeric comparison (Score property)
            if (x.Score != y.Score)
            {
                return x.Score.CompareTo(y.Score);
            }
            return 0;
        }

        /// <summary>
        /// Compares entities by name (or primary string criterion)
        /// </summary>
        private int CompareNames({entity_type} x, {entity_type} y)
        {
            // Handle null names
            if (string.IsNullOrEmpty(x.Name) && string.IsNullOrEmpty(y.Name)) return 0;
            if (string.IsNullOrEmpty(x.Name)) return 1;
            if (string.IsNullOrEmpty(y.Name)) return -1;

            return string.Compare(x.Name, y.Name, _stringComparison);
        }

        /// <summary>
        /// Compares entities by age (or secondary numeric criterion)
        /// </summary>
        private int CompareAges({entity_type} x, {entity_type} y)
        {
            // Example for date/age comparison
            if (x.Age != y.Age)
            {
                return x.Age.CompareTo(y.Age);
            }
            return 0;
        }
    }
}
```

## Common Comparison Patterns

### String Comparison:
```csharp
private int CompareNames({entity_type} x, {entity_type} y)
{
    // Null handling
    if (string.IsNullOrEmpty(x.Name) && string.IsNullOrEmpty(y.Name)) return 0;
    if (string.IsNullOrEmpty(x.Name)) return 1;
    if (string.IsNullOrEmpty(y.Name)) return -1;

    // Culture-aware comparison
    return string.Compare(x.Name, y.Name, _stringComparison);
}
```

### Numeric Comparison:
```csharp
private int CompareScores({entity_type} x, {entity_type} y)
{
    return x.Score.CompareTo(y.Score);
}

// For nullable numerics
private int CompareOptionalScores({entity_type} x, {entity_type} y)
{
    if (x.Score == null && y.Score == null) return 0;
    if (x.Score == null) return 1;
    if (y.Score == null) return -1;
    
    return x.Score.Value.CompareTo(y.Score.Value);
}
```

### DateTime Comparison:
```csharp
private int CompareDates({entity_type} x, {entity_type} y)
{
    return x.CreatedDate.CompareTo(y.CreatedDate);
}

// For nullable DateTime
private int CompareOptionalDates({entity_type} x, {entity_type} y)
{
    if (x.EventDate == null && y.EventDate == null) return 0;
    if (x.EventDate == null) return 1;
    if (y.EventDate == null) return -1;
    
    return x.EventDate.Value.CompareTo(y.EventDate.Value);
}
```

### Boolean Comparison:
```csharp
private int CompareActiveStatus({entity_type} x, {entity_type} y)
{
    // True values come first
    return y.IsActive.CompareTo(x.IsActive);
}
```

### Enum Comparison:
```csharp
private int CompareStatus({entity_type} x, {entity_type} y)
{
    return x.Status.CompareTo(y.Status);
}
```

### Complex Object Comparison:
```csharp
private int CompareClubs({entity_type} x, {entity_type} y)
{
    // Handle null clubs
    if (x.Club == null && y.Club == null) return 0;
    if (x.Club == null) return 1;
    if (y.Club == null) return -1;

    // Compare by club name
    return string.Compare(x.Club.Name, y.Club.Name, _stringComparison);
}
```

## Performance Optimizations

### Caching for Expensive Operations:
```csharp
private readonly Dictionary<int, string> _sortKeyCache = new();

private string GetSortKey({entity_type} entity)
{
    if (_sortKeyCache.TryGetValue(entity.Id, out string? cached))
        return cached;

    // Generate expensive sort key
    string sortKey = $"{entity.Name?.ToLower()}_{entity.Score:D10}";
    _sortKeyCache[entity.Id] = sortKey;
    return sortKey;
}
```

### Multiple Comparers for Different Scenarios:
```csharp
public static class {entity_type}Comparers
{
    public static IComparer<{entity_type}> ByName { get; } = new {comparer_name}NameComparer();
    public static IComparer<{entity_type}> ByScore { get; } = new {comparer_name}ScoreComparer();
    public static IComparer<{entity_type}> ByDate { get; } = new {comparer_name}DateComparer();
}
```

## Usage Examples

### In Collections:
```csharp
// Sort a list
var sortedAthletes = athletes.OrderBy(a => a, new {comparer_name}Comparer()).ToList();

// Sort with LINQ
var sorted = athletes.OrderBy(a => a, new {comparer_name}Comparer(descending: true));

// Sort in place
athletesList.Sort(new {comparer_name}Comparer());
```

### In XAML with CollectionViewSource:
```xaml
<CollectionViewSource x:Key="SortedItems" Source="{Binding Items}">
    <CollectionViewSource.SortDescriptions>
        <componentModel:SortDescription PropertyName="Score" Direction="Descending"/>
        <componentModel:SortDescription PropertyName="Name" Direction="Ascending"/>
    </CollectionViewSource.SortDescriptions>
</CollectionViewSource>
```

### In ViewModel:
```csharp
public ObservableCollection<{entity_type}> SortedItems
{
    get
    {
        var sorted = _items.OrderBy(item => item, new {comparer_name}Comparer()).ToList();
        return new ObservableCollection<{entity_type}>(sorted);
    }
}
```

## Unit Test Template:
```csharp
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RescueScoreManager.Comparer;
using RescueScoreManager.Data;

namespace RescueScoreManager.Tests.Comparer
{
    [TestFixture]
    public class {comparer_name}ComparerTests
    {
        private {comparer_name}Comparer _comparer;

        [SetUp]
        public void SetUp()
        {
            _comparer = new {comparer_name}Comparer();
        }

        [Test]
        public void Compare_BothNull_ReturnsZero()
        {
            var result = _comparer.Compare(null, null);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Compare_FirstNull_ReturnsNegative()
        {
            var entity = new {entity_type} { Name = "Test" };
            var result = _comparer.Compare(null, entity);
            Assert.Less(result, 0);
        }

        [Test]
        public void Compare_SecondNull_ReturnsPositive()
        {
            var entity = new {entity_type} { Name = "Test" };
            var result = _comparer.Compare(entity, null);
            Assert.Greater(result, 0);
        }

        [Test]
        public void Compare_DifferentScores_SortsCorrectly()
        {
            var entity1 = new {entity_type} { Score = 100, Name = "A" };
            var entity2 = new {entity_type} { Score = 200, Name = "B" };

            var result = _comparer.Compare(entity1, entity2);
            Assert.Less(result, 0); // entity1 should come before entity2
        }

        [Test]
        public void Compare_SameScore_SortsByName()
        {
            var entity1 = new {entity_type} { Score = 100, Name = "B" };
            var entity2 = new {entity_type} { Score = 100, Name = "A" };

            var result = _comparer.Compare(entity1, entity2);
            Assert.Greater(result, 0); // entity2 should come before entity1
        }

        [Test]
        public void Sort_MultipleItems_ProducesCorrectOrder()
        {
            var items = new List<{entity_type}>
            {
                new {entity_type} { Score = 100, Name = "C" },
                new {entity_type} { Score = 200, Name = "A" },
                new {entity_type} { Score = 100, Name = "B" },
                new {entity_type} { Score = 300, Name = "D" }
            };

            items.Sort(_comparer);

            Assert.AreEqual("C", items[0].Name); // Score 100, Name C
            Assert.AreEqual("B", items[1].Name); // Score 100, Name B  
            Assert.AreEqual("A", items[2].Name); // Score 200, Name A
            Assert.AreEqual("D", items[3].Name); // Score 300, Name D
        }

        [Test]
        public void Compare_DescendingOrder_ReversesResult()
        {
            var descendingComparer = new {comparer_name}Comparer(descending: true);
            var entity1 = new {entity_type} { Score = 100 };
            var entity2 = new {entity_type} { Score = 200 };

            var result = descendingComparer.Compare(entity1, entity2);
            Assert.Greater(result, 0); // Higher score should come first
        }
    }
}
```

## Specialized Comparer Examples

### Multi-Key Comparer:
```csharp
public class CompetitionEventComparer : IComparer<Race>
{
    public int Compare(Race? x, Race? y)
    {
        if (x == null || y == null) return HandleNulls(x, y);

        // 1. Competition date
        int result = x.Competition.BeginDate.CompareTo(y.Competition.BeginDate);
        if (result != 0) return result;

        // 2. Event category
        result = string.Compare(x.Category?.Name, y.Category?.Name, StringComparison.OrdinalIgnoreCase);
        if (result != 0) return result;

        // 3. Distance
        result = x.Distance.CompareTo(y.Distance);
        if (result != 0) return result;

        // 4. Final fallback
        return x.Id.CompareTo(y.Id);
    }
}
```

### Performance-Oriented Comparer:
```csharp
public class FastNameComparer : IComparer<Club>
{
    private static readonly Dictionary<string, string> _normalizedCache = new();

    public int Compare(Club? x, Club? y)
    {
        if (x == null || y == null) return HandleNulls(x, y);

        string normalizedX = GetNormalizedName(x.Name);
        string normalizedY = GetNormalizedName(y.Name);

        return string.Compare(normalizedX, normalizedY, StringComparison.Ordinal);
    }

    private static string GetNormalizedName(string name)
    {
        if (_normalizedCache.TryGetValue(name, out string? cached))
            return cached;

        string normalized = name?.Trim().ToUpperInvariant() ?? string.Empty;
        _normalizedCache[name] = normalized;
        return normalized;
    }
}
```

## Validation Checklist
- [ ] Comparer class in Comparer/ folder
- [ ] Implements IComparer<T> interface
- [ ] Handles null values correctly
- [ ] Multi-level sorting implemented
- [ ] Performance considerations addressed
- [ ] String comparison uses appropriate culture
- [ ] Consistent ordering (transitive property)
- [ ] Unit tests created and passing
- [ ] XML documentation added
- [ ] Usage examples documented
- [ ] Follows project naming conventions