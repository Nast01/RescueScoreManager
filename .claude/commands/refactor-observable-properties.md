# Refactor Observable Properties

Modernizes ViewModels to use Community Toolkit's [ObservableProperty] pattern instead of manual INotifyPropertyChanged implementation.

## Parameters
- `target_file` (required): Path to the ViewModel file to refactor
- `preserve_validation` (optional): Set to "true" to preserve custom validation logic (default: true)
- `backup_original` (optional): Set to "true" to create backup before refactoring (default: true)

## Usage
```
@refactor-observable-properties target_file="RescueScoreManager/Modules/Planning/ViewModels/PlanningViewModel.cs" preserve_validation="true" backup_original="true"
```

## Refactoring Steps

### 1. **Analysis Phase**
- Read the target ViewModel file
- Identify properties using manual INotifyPropertyChanged
- Check for custom validation logic
- Identify backing fields and property patterns
- Note any special property change handlers

### 2. **Backup Phase** (if backup_original="true")
- Create backup copy with .bak extension
- Preserve original for rollback if needed

### 3. **Refactoring Phase**
- Update class inheritance to ObservableObject
- Convert properties to [ObservableProperty] pattern
- Remove manual PropertyChanged implementations
- Preserve custom validation and change handlers
- Update using statements

## Refactoring Patterns

### Manual INotifyPropertyChanged Pattern (Before):
```csharp
public class MyViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _count;
    private bool _isEnabled;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
                // Custom logic
                ValidateName();
            }
        }
    }

    public int Count
    {
        get => _count;
        set
        {
            if (_count != value)
            {
                _count = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public string DisplayText => $"{Name} ({Count})";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void ValidateName()
    {
        // Custom validation logic
    }
}
```

### Community Toolkit Pattern (After):
```csharp
public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private bool _isEnabled;

    public string DisplayText => $"{Name} ({Count})";

    partial void OnNameChanged(string value)
    {
        // Custom logic preserved
        ValidateName();
    }

    partial void OnCountChanged(int value)
    {
        // Notify dependent properties
        OnPropertyChanged(nameof(DisplayText));
    }

    private void ValidateName()
    {
        // Custom validation logic preserved
    }
}
```

## Conversion Rules

### 1. **Class Declaration**
```csharp
// Before
public class MyViewModel : INotifyPropertyChanged
public class MyViewModel : BaseViewModel

// After
public partial class MyViewModel : ObservableObject
```

### 2. **Using Statements**
```csharp
// Add if not present
using CommunityToolkit.Mvvm.ComponentModel;

// Remove if no longer needed
using System.ComponentModel;
using System.Runtime.CompilerServices;
```

### 3. **Property Patterns**

#### Simple Property:
```csharp
// Before
private string _name = string.Empty;
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}

// After
[ObservableProperty]
private string _name = string.Empty;
```

#### Property with Custom Logic:
```csharp
// Before
private string _name = string.Empty;
public string Name
{
    get => _name;
    set
    {
        if (SetProperty(ref _name, value))
        {
            ValidateName();
            OnPropertyChanged(nameof(DisplayName));
        }
    }
}

// After
[ObservableProperty]
private string _name = string.Empty;

partial void OnNameChanged(string value)
{
    ValidateName();
    OnPropertyChanged(nameof(DisplayName));
}
```

#### Property with Validation:
```csharp
// Before
private string _email = string.Empty;
public string Email
{
    get => _email;
    set
    {
        if (SetProperty(ref _email, value))
        {
            IsEmailValid = IsValidEmail(value);
        }
    }
}

// After
[ObservableProperty]
private string _email = string.Empty;

partial void OnEmailChanged(string value)
{
    IsEmailValid = IsValidEmail(value);
}
```

### 4. **Remove Manual Implementation**
```csharp
// Remove these if they exist
public event PropertyChangedEventHandler? PropertyChanged;

protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
{
    if (EqualityComparer<T>.Default.Equals(field, value))
        return false;

    field = value;
    OnPropertyChanged(propertyName);
    return true;
}
```

## Special Cases

### 1. **Computed Properties**
```csharp
// These remain unchanged - no backing field needed
public string FullName => $"{FirstName} {LastName}";
public bool HasItems => Items.Count > 0;
```

### 2. **Properties with Complex Setters**
```csharp
// Before
public DateTime SelectedDate
{
    get => _selectedDate;
    set
    {
        if (SetProperty(ref _selectedDate, value))
        {
            // Complex logic
            UpdateAvailableSlots();
            RefreshCalendar();
            NotifyExternalService();
        }
    }
}

// After - Keep as regular property if logic is very complex
[ObservableProperty]
private DateTime _selectedDate;

partial void OnSelectedDateChanged(DateTime value)
{
    // Complex logic preserved
    UpdateAvailableSlots();
    RefreshCalendar();
    NotifyExternalService();
}
```

### 3. **Properties with Validation Attributes**
```csharp
// Before
[Required]
[StringLength(100)]
public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);
}

// After - Attributes go on the property, not the field
[ObservableProperty]
private string _name = string.Empty;

// The generated property will have the attributes
```

## Command Refactoring

### 1. **RelayCommand Properties**
```csharp
// Before
private ICommand? _saveCommand;
public ICommand SaveCommand => _saveCommand ??= new RelayCommand(OnSave, CanSave);

// After - Use [RelayCommand] attribute
[RelayCommand(CanExecute = nameof(CanSave))]
private void OnSave()
{
    // Implementation
}

private bool CanSave()
{
    return !string.IsNullOrEmpty(Name);
}
```

### 2. **Async Commands**
```csharp
// Before
private ICommand? _loadCommand;
public ICommand LoadCommand => _loadCommand ??= new AsyncRelayCommand(OnLoadAsync);

// After
[RelayCommand]
private async Task OnLoadAsync()
{
    // Implementation
}
```

## Validation Preservation

### 1. **Custom Validation Logic**
```csharp
// Preserve in partial methods
partial void OnEmailChanged(string value)
{
    // Preserve existing validation
    if (string.IsNullOrEmpty(value))
    {
        EmailError = "Email is required";
        return;
    }
    
    if (!IsValidEmail(value))
    {
        EmailError = "Invalid email format";
        return;
    }
    
    EmailError = null;
}
```

### 2. **Property Dependencies**
```csharp
// Preserve property change notifications
partial void OnFirstNameChanged(string value)
{
    OnPropertyChanged(nameof(FullName));
}

partial void OnLastNameChanged(string value)
{
    OnPropertyChanged(nameof(FullName));
}
```

## Testing After Refactoring

### 1. **Verify Property Changes**
```csharp
[Test]
public void PropertyChange_NotificationWorks()
{
    var viewModel = new MyViewModel();
    bool propertyChanged = false;
    
    viewModel.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(MyViewModel.Name))
            propertyChanged = true;
    };
    
    viewModel.Name = "Test";
    
    Assert.IsTrue(propertyChanged);
}
```

### 2. **Verify Custom Logic**
```csharp
[Test]
public void CustomLogic_StillExecutes()
{
    var viewModel = new MyViewModel();
    
    viewModel.Email = "invalid-email";
    
    Assert.IsNotNull(viewModel.EmailError);
    
    viewModel.Email = "valid@example.com";
    
    Assert.IsNull(viewModel.EmailError);
}
```

## Common Issues and Solutions

### 1. **Missing partial Keyword**
```csharp
// Error: class must be partial
public class MyViewModel : ObservableObject  // ❌

// Fix: add partial keyword
public partial class MyViewModel : ObservableObject  // ✅
```

### 2. **Property Name Conflicts**
```csharp
// Error: property name conflicts with generated property
[ObservableProperty]
private string _name;
public string Name { get; set; }  // ❌

// Fix: remove manual property
[ObservableProperty]
private string _name;  // ✅ (Name property will be generated)
```

### 3. **Backing Field Naming**
```csharp
// Error: incorrect backing field name
[ObservableProperty]
private string name;  // ❌ (should start with underscore)

// Fix: use underscore prefix
[ObservableProperty]
private string _name;  // ✅
```

## Rollback Strategy

If issues arise after refactoring:

1. **Restore from backup**
2. **Check compilation errors**
3. **Verify property bindings in XAML**
4. **Test UI functionality**
5. **Run unit tests**

## Performance Benefits

After refactoring:
- **Reduced boilerplate code** (60-80% less property code)
- **Better performance** (source generators vs reflection)
- **Compile-time safety** (no string-based property names)
- **Easier maintenance** (less code to maintain)

## Validation Checklist
- [ ] Class marked as partial
- [ ] Inherits from ObservableObject
- [ ] All properties converted to [ObservableProperty]
- [ ] Custom logic preserved in partial methods
- [ ] Commands converted to [RelayCommand] if applicable
- [ ] Manual PropertyChanged implementation removed
- [ ] Using statements updated
- [ ] Backup created (if requested)
- [ ] Code compiles without errors
- [ ] UI bindings still work
- [ ] Unit tests pass
- [ ] Custom validation still functions