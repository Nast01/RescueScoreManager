# Create Data Entity

Creates a new data entity class following RescueScoreManager data patterns with Entity Framework support and XML serialization.

## Parameters
- `entity_name` (required): Name of the entity (e.g., "Equipment", "Judge", "Certificate")
- `properties` (required): Comma-separated list of properties with types (e.g., "Name:string,Count:int,IsActive:bool")
- `relationships` (optional): Navigation properties (e.g., "Competition:one,Athletes:many")
- `include_xml` (optional): Set to "true" to add XML serialization support (default: true)

## Usage
```
@create-data-entity entity_name="Equipment" properties="Name:string,Description:string,Count:int,IsActive:bool,CreatedDate:DateTime" relationships="Competition:one" include_xml="true"
```

## Implementation Steps

1. **Create Entity Class**: `RescueScoreManager/Data/{entity_name}.cs`
   - Add properties with proper types
   - Include navigation properties for relationships
   - Add validation attributes
   - Configure Entity Framework relationships

2. **Update DbContext**: Add DbSet to `RescueScoreManagerContext.cs`
   - Add DbSet property
   - Configure entity in OnModelCreating if needed

3. **Update XMLService**: Add entity support to `XMLService.cs`
   - Add private collection field
   - Add public getter method
   - Add CRUD methods
   - Include in Load/Save operations

4. **Generate Migration**: Create EF migration for the new entity

## Template Structure

### Entity Class Template:
```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RescueScoreManager.Data
{
    /// <summary>
    /// Represents a {entity_name} entity
    /// </summary>
    public class {entity_name}
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets whether this entity is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last modified date
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        // Add based on 'relationships' parameter

        /// <summary>
        /// Gets or sets the associated Competition ID
        /// </summary>
        public int? CompetitionId { get; set; }

        /// <summary>
        /// Gets or sets the associated Competition
        /// </summary>
        [ForeignKey("CompetitionId")]
        public virtual Competition? Competition { get; set; }

        // Example many relationship
        /// <summary>
        /// Gets or sets the collection of related entities
        /// </summary>
        public virtual ICollection<RelatedEntity> RelatedEntities { get; set; } = new List<RelatedEntity>();

        /// <summary>
        /// Returns a string representation of the {entity_name}
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current {entity_name}
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is {entity_name} other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the current {entity_name}
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
```

### DbContext Update Template:
Add to `RescueScoreManagerContext.cs`:

```csharp
/// <summary>
/// Gets or sets the {entity_name} entities
/// </summary>
public DbSet<{entity_name}> {entity_name}s { get; set; }
```

In `OnModelCreating` method:
```csharp
// {entity_name} configuration
modelBuilder.Entity<{entity_name}>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Description).HasMaxLength(500);
    entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
    
    // Configure relationships
    entity.HasOne(e => e.Competition)
          .WithMany() // or .WithMany(c => c.{entity_name}s) if back-reference exists
          .HasForeignKey(e => e.CompetitionId)
          .OnDelete(DeleteBehavior.SetNull);
    
    // Add indexes for performance
    entity.HasIndex(e => e.Name);
    entity.HasIndex(e => e.CompetitionId);
});
```

### XMLService Update Template:
Add to `XMLService.cs`:

```csharp
// Add private field
private readonly List<{entity_name}> _{entity_name.ToLower()}s = new();

// Add public getter
/// <summary>
/// Gets the collection of {entity_name}s
/// </summary>
public IReadOnlyList<{entity_name}> Get{entity_name}s()
{
    var sorted{entity_name}s = _{entity_name.ToLower()}s.ToList();
    sorted{entity_name}s.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
    return sorted{entity_name}s.AsReadOnly();
}

/// <summary>
/// Gets a {entity_name} by ID
/// </summary>
public {entity_name}? Get{entity_name}ById(int id)
{
    return _{entity_name.ToLower()}s.FirstOrDefault(x => x.Id == id);
}

/// <summary>
/// Adds a new {entity_name}
/// </summary>
public void Add{entity_name}({entity_name} {entity_name.ToLower()})
{
    if ({entity_name.ToLower()} == null)
        throw new ArgumentNullException(nameof({entity_name.ToLower()}));
    
    if (_{entity_name.ToLower()}s.Any(x => x.Id == {entity_name.ToLower()}.Id))
        throw new InvalidOperationException($"{entity_name} with ID {{{entity_name.ToLower()}.Id}} already exists");
    
    _{entity_name.ToLower()}s.Add({entity_name.ToLower()});
    _logger.LogInformation("{entity_name} added: {{Name}}", {entity_name.ToLower()}.Name);
}

/// <summary>
/// Updates an existing {entity_name}
/// </summary>
public void Update{entity_name}({entity_name} {entity_name.ToLower()})
{
    if ({entity_name.ToLower()} == null)
        throw new ArgumentNullException(nameof({entity_name.ToLower()}));
    
    var existingIndex = _{entity_name.ToLower()}s.FindIndex(x => x.Id == {entity_name.ToLower()}.Id);
    if (existingIndex == -1)
        throw new InvalidOperationException($"{entity_name} with ID {{{entity_name.ToLower()}.Id}} not found");
    
    {entity_name.ToLower()}.ModifiedDate = DateTime.UtcNow;
    _{entity_name.ToLower()}s[existingIndex] = {entity_name.ToLower()};
    _logger.LogInformation("{entity_name} updated: {{Name}}", {entity_name.ToLower()}.Name);
}

/// <summary>
/// Removes a {entity_name}
/// </summary>
public void Remove{entity_name}(int id)
{
    var {entity_name.ToLower()} = _{entity_name.ToLower()}s.FirstOrDefault(x => x.Id == id);
    if ({entity_name.ToLower()} != null)
    {
        _{entity_name.ToLower()}s.Remove({entity_name.ToLower()});
        _logger.LogInformation("{entity_name} removed: {{Name}}", {entity_name.ToLower()}.Name);
    }
}

/// <summary>
/// Updates the {entity_name} collection
/// </summary>
public void Update{entity_name}s(IEnumerable<{entity_name}> {entity_name.ToLower()}s)
{
    if ({entity_name.ToLower()}s == null)
        throw new ArgumentNullException(nameof({entity_name.ToLower()}s));
    
    _{entity_name.ToLower()}s.Clear();
    _{entity_name.ToLower()}s.AddRange({entity_name.ToLower()}s);
    _logger.LogInformation("{entity_name}s collection updated with {{Count}} items", _{entity_name.ToLower()}s.Count);
}
```

Add to Load method:
```csharp
// Load {entity_name}s
var {entity_name.ToLower()}Elements = doc.Descendants("{entity_name}");
foreach (var element in {entity_name.ToLower()}Elements)
{
    var {entity_name.ToLower()} = new {entity_name}
    {
        Id = int.Parse(element.Attribute("Id")?.Value ?? "0"),
        Name = element.Attribute("Name")?.Value ?? string.Empty,
        Description = element.Attribute("Description")?.Value,
        IsActive = bool.Parse(element.Attribute("IsActive")?.Value ?? "true"),
        CreatedDate = DateTime.Parse(element.Attribute("CreatedDate")?.Value ?? DateTime.UtcNow.ToString()),
        CompetitionId = int.TryParse(element.Attribute("CompetitionId")?.Value, out var compId) ? compId : null
    };
    _{entity_name.ToLower()}s.Add({entity_name.ToLower()});
}
```

Add to Save method:
```csharp
// Save {entity_name}s
foreach (var {entity_name.ToLower()} in _{entity_name.ToLower()}s)
{
    var {entity_name.ToLower()}Element = new XElement("{entity_name}",
        new XAttribute("Id", {entity_name.ToLower()}.Id),
        new XAttribute("Name", {entity_name.ToLower()}.Name),
        new XAttribute("IsActive", {entity_name.ToLower()}.IsActive),
        new XAttribute("CreatedDate", {entity_name.ToLower()}.CreatedDate.ToString("O")));
    
    if (!string.IsNullOrEmpty({entity_name.ToLower()}.Description))
        {entity_name.ToLower()}Element.Add(new XAttribute("Description", {entity_name.ToLower()}.Description));
    
    if ({entity_name.ToLower()}.CompetitionId.HasValue)
        {entity_name.ToLower()}Element.Add(new XAttribute("CompetitionId", {entity_name.ToLower()}.CompetitionId.Value));
    
    if ({entity_name.ToLower()}.ModifiedDate.HasValue)
        {entity_name.ToLower()}Element.Add(new XAttribute("ModifiedDate", {entity_name.ToLower()}.ModifiedDate.Value.ToString("O")));
    
    root.Add({entity_name.ToLower()}Element);
}
```

## Property Type Mappings

### Common Types:
- `string` → `string` (with StringLength attribute)
- `int` → `int`
- `bool` → `bool`
- `DateTime` → `DateTime`
- `decimal` → `decimal` (for currency/precision)
- `double` → `double` (for measurements)

### Validation Attributes:
- `[Required]` for mandatory fields
- `[StringLength(n)]` for string limits
- `[Range(min, max)]` for numeric ranges
- `[EmailAddress]` for email fields
- `[Phone]` for phone numbers
- `[Url]` for URLs

## Relationship Patterns

### One-to-Many:
```csharp
// Parent entity
public virtual ICollection<Child> Children { get; set; } = new List<Child>();

// Child entity
public int ParentId { get; set; }
public virtual Parent Parent { get; set; }
```

### Many-to-Many:
```csharp
// Use join entity for explicit control
public virtual ICollection<EntityJoin> JoinEntities { get; set; } = new List<EntityJoin>();
```

## Validation Checklist
- [ ] Entity class in Data/ folder
- [ ] Proper validation attributes
- [ ] Navigation properties configured
- [ ] DbSet added to context
- [ ] Entity configuration in OnModelCreating
- [ ] XMLService methods added
- [ ] Load/Save logic implemented
- [ ] Migration generated
- [ ] Relationships properly defined
- [ ] XML documentation added