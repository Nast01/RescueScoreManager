# Analyze Planning Issue

Specialized command for debugging Planning module issues, particularly focused on PlanningProgramView drag & drop, data binding, and event handling problems.

## Parameters
- `issue_type` (required): Type of issue - "drag_drop", "binding", "events", "ui_update", "performance"
- `component` (optional): Specific component - "program_meeting", "time_slot", "site", "event"
- `symptoms` (optional): Description of observed symptoms
- `error_messages` (optional): Any error messages or exceptions

## Usage
```
@analyze-planning-issue issue_type="drag_drop" component="program_meeting" symptoms="Events not visible after drop" error_messages="None"
```

## Analysis Steps

### 1. **Examine Current State**
- Read PlanningProgramView.xaml and PlanningProgramView.xaml.cs
- Read PlanningProgramViewModel.cs
- Check recent changes and git history
- Identify patterns and potential root causes

### 2. **Issue-Specific Analysis**

#### For `drag_drop` Issues:
- Check drag & drop event handlers (OnProgramMeetingDrop, OnTimeSlotDrop)
- Verify DataContext binding in drop targets
- Validate drag data format and content
- Check ObservableCollection updates
- Verify UI thread operations

#### For `binding` Issues:
- Check DataContext setup in code-behind
- Verify ViewModel property notifications
- Check [ObservableProperty] attributes
- Validate binding paths in XAML
- Check converter usage and registration

#### For `events` Issues:
- Check event handler registration
- Verify command binding
- Check RelayCommand setup
- Validate parameter passing
- Check async/await patterns

#### For `ui_update` Issues:
- Check ObservableCollection usage
- Verify PropertyChanged notifications
- Check UI thread marshalling
- Validate template selectors and triggers
- Check visibility converters

#### For `performance` Issues:
- Check collection sizes and operations
- Verify virtualization usage
- Check binding complexity
- Validate memory leaks
- Check excessive notifications

### 3. **Common Problem Patterns**

#### Drag & Drop Issues:
```csharp
// Check if sender DataContext is correct
private void OnProgramMeetingDrop(object sender, DragEventArgs e)
{
    System.Diagnostics.Debug.WriteLine($"Sender type: {sender?.GetType().Name}");
    
    object programMeeting = null;
    if (sender is GroupBox groupBox)
    {
        programMeeting = groupBox.DataContext;
        System.Diagnostics.Debug.WriteLine($"DataContext type: {programMeeting?.GetType().Name}");
    }
    
    if (programMeeting is ProgramMeetingViewModel vm)
    {
        System.Diagnostics.Debug.WriteLine($"Events count before: {vm.Events.Count}");
        // ... perform operation
        System.Diagnostics.Debug.WriteLine($"Events count after: {vm.Events.Count}");
    }
}
```

#### Binding Issues:
```xaml
<!-- Check binding paths -->
<TextBlock Text="{Binding Path=PropertyName, FallbackValue='BINDING ERROR'}" />

<!-- Add debug converter -->
<TextBlock Text="{Binding Path=PropertyName, Converter={StaticResource DebugConverter}}" />
```

#### ViewModel Issues:
```csharp
// Check property notifications
[ObservableProperty]
private string _searchText = string.Empty;

// Verify command setup
public ICommand SaveCommand { get; }

public ViewModel()
{
    SaveCommand = new RelayCommand(OnSave, CanSave);
}
```

### 4. **Diagnostic Checklist**

#### For Drag & Drop:
- [ ] Drop event handlers are registered correctly
- [ ] Sender DataContext is the expected type
- [ ] Drag data contains expected format ("PlanningEvent" or "PlannedEvent")
- [ ] ObservableCollection.Add() is called on UI thread
- [ ] No duplicate additions or race conditions
- [ ] Events are not being removed elsewhere
- [ ] ItemsControl ItemsSource binding is correct

#### For Data Binding:
- [ ] DataContext is set correctly in code-behind
- [ ] ViewModel properties use [ObservableProperty]
- [ ] Binding paths match property names exactly
- [ ] Converters are registered in resources
- [ ] Two-way bindings use UpdateSourceTrigger appropriately
- [ ] Collections implement INotifyCollectionChanged

#### For UI Updates:
- [ ] UI changes happen on UI thread (Dispatcher.Invoke if needed)
- [ ] ObservableCollection is used for dynamic lists
- [ ] ItemsControl templates are correctly defined
- [ ] Visibility converters work as expected
- [ ] Style triggers are properly configured

### 5. **Quick Fixes and Workarounds**

#### Force UI Refresh:
```csharp
// Force binding refresh
var bindingExpression = BindingOperations.GetBindingExpression(targetElement, targetProperty);
bindingExpression?.UpdateTarget();

// Force collection refresh
OnPropertyChanged(nameof(CollectionProperty));
```

#### Debug Binding:
```xaml
<!-- Add to Window/UserControl resources -->
<converter:DebugConverter x:Key="DebugConverter"/>

<!-- Use in suspicious bindings -->
<TextBlock Text="{Binding SuspiciousProperty, Converter={StaticResource DebugConverter}}" />
```

#### Debug DataContext:
```csharp
private void DebugDataContext(FrameworkElement element)
{
    System.Diagnostics.Debug.WriteLine($"Element: {element.GetType().Name}");
    System.Diagnostics.Debug.WriteLine($"DataContext: {element.DataContext?.GetType().Name ?? "NULL"}");
    if (element.Parent is FrameworkElement parent)
    {
        System.Diagnostics.Debug.WriteLine($"Parent DataContext: {parent.DataContext?.GetType().Name ?? "NULL"}");
    }
}
```

### 6. **Investigation Questions**

Answer these questions during analysis:

1. **When did the issue start?** (recent changes, new features, refactoring)
2. **Is it reproducible?** (always, sometimes, specific conditions)
3. **Which components are affected?** (all sites, specific program meetings, certain events)
4. **Are there error messages?** (console output, logs, exceptions)
5. **What user actions trigger it?** (drag & drop, button clicks, navigation)

### 7. **Common Solutions**

#### ObservableCollection Not Updating UI:
```csharp
// Ensure collection is bound to UI thread
public ObservableCollection<ItemType> Items { get; } = new();

// Add items on UI thread
Application.Current.Dispatcher.Invoke(() =>
{
    Items.Add(newItem);
});
```

#### DataContext Not Set:
```csharp
public partial class MyView : UserControl
{
    public MyView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext == null)
        {
            var viewModel = App.ServiceProvider?.GetService<MyViewModel>();
            DataContext = viewModel;
        }
    }
}
```

#### Drag & Drop DataContext Issues:
```xaml
<!-- Ensure drop target has correct DataContext -->
<GroupBox DataContext="{Binding ProgramMeeting}"
          AllowDrop="True"
          Drop="OnProgramMeetingDrop">
    <!-- Content -->
</GroupBox>
```

### 8. **Testing Steps**

After implementing fixes:

1. **Clear Debug Output** and reproduce the issue
2. **Check Debug Messages** for expected flow
3. **Verify UI Updates** happen immediately
4. **Test Edge Cases** (empty collections, null values)
5. **Performance Test** with multiple operations
6. **Cross-Component Test** (different sites, meetings, events)

### 9. **Prevention Strategies**

#### Add Comprehensive Logging:
```csharp
private void OnDrop(object sender, DragEventArgs e)
{
    _logger.LogInformation("Drop operation started. Sender: {SenderType}", sender?.GetType().Name);
    
    try
    {
        // ... operation logic
        _logger.LogInformation("Drop operation completed successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Drop operation failed");
        throw;
    }
}
```

#### Add Debug Assertions:
```csharp
#if DEBUG
private void ValidateState()
{
    Debug.Assert(DataContext is PlanningProgramViewModel, "DataContext should be PlanningProgramViewModel");
    Debug.Assert(Sites.Count > 0, "Sites collection should not be empty");
}
#endif
```

#### Add Unit Tests:
```csharp
[Test]
public void MoveEventToProgramMeeting_ValidEvent_AddsToCollection()
{
    // Arrange
    var viewModel = new PlanningProgramViewModel(/* dependencies */);
    var programMeeting = new ProgramMeetingViewModel();
    var planningEvent = new PlanningEventViewModel();

    // Act
    viewModel.MoveEventToProgramMeeting(planningEvent, programMeeting);

    // Assert
    Assert.AreEqual(1, programMeeting.Events.Count);
    Assert.Contains(planningEvent.Id, programMeeting.Events.Select(e => e.Id));
}
```

## Quick Diagnostic Commands

### Check Current State:
```csharp
// Add to ViewModel for debugging
public void DiagnosePlanningState()
{
    _logger.LogInformation("=== Planning State Diagnosis ===");
    _logger.LogInformation("Sites count: {Count}", Sites.Count);
    _logger.LogInformation("Events to plan: {Count}", EventsToplan.Count);
    
    foreach (var site in Sites)
    {
        _logger.LogInformation("Site {Name}: {MeetingCount} meetings", site.Name, site.ProgramMeetings.Count);
        foreach (var meeting in site.ProgramMeetings)
        {
            _logger.LogInformation("  Meeting {Name}: {EventCount} events", meeting.Name, meeting.Events.Count);
        }
    }
}
```

### Force Refresh:
```csharp
public void ForceRefresh()
{
    OnPropertyChanged(nameof(Sites));
    OnPropertyChanged(nameof(EventsToplan));
    OnPropertyChanged(nameof(FilteredEventsToplan));
}
```

## Validation Checklist
- [ ] Identified root cause of the issue
- [ ] Implemented appropriate solution
- [ ] Added logging/debugging for future issues
- [ ] Tested fix with various scenarios
- [ ] Verified no regression in other functionality
- [ ] Updated documentation if needed
- [ ] Considered adding unit tests
- [ ] Performance impact assessed