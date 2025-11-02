# Project Overview

This is a C# WPF application using .NET 9.0
- MVVM pattern with Community Toolkit
- Uses MaterialDesignInXamlToolkit preferentially, but you can use other necessary UI libraries

# Prerequisites
- Visual Studio 2022 17.8+ or VS Code with C# extension
- .NET 9.0 SDK
- Git for version control

# Dependencies
- MaterialDesignInXamlToolkit
- CommunityToolkit.Mvvm
- Entity Framework Core
- Microsoft.Extensions.DependencyInjection

# Getting Started
1. Clone the repository
2. Open RescueScoreManager.sln in Visual Studio
3. Restore NuGet packages
4. Build solution (Ctrl+Shift+B)
5. Run application (F5)

# Common Commands
- Build: `dotnet build`
- Test: `dotnet test`
- Run: `dotnet run --project RescueScoreManager`

# Project Structure

## Root Directory
- **RescueScoreManager.sln** - Main Visual Studio solution file
- **Directory.Build.props/targets** - MSBuild configuration files for project-wide settings
- **Directory.Packages.props** - Centralized NuGet package management
- **NuGet.config** - NuGet package source configuration
- **README.md** - Project documentation and setup instructions

## RescueScoreManager/ (Main Application)
- **App.xaml/cs** - Application entry point and global resources
- **MainWindow.xaml/cs** - Main application window and navigation
- **MainWindowViewModel.cs** - Main window view model with navigation logic

### Assets/
- **Documents/** - Template documents for competition management (Excel, Word, PDF templates)
- **Images/** - Application icons and logos (FFSS branding)

### Comparer/
- Custom comparison logic - Sorting entities with custom comparers

### Constants/
- **AppConstants.cs** - Application-wide constant values

### Converter/
- **WPF Value Converters** - Data binding converters for UI display logic (visibility, formatting, localization)

### Data/
- **Entity Models** - Data models representing competition entities (Athletes, Clubs, Races, etc.)
- **EnumRSM.cs** - Application-specific enumerations
- **RescueScoreManagerContext.cs** - Entity Framework database context

### Helpers/
- **Utility Classes** - Helper functions for JSON, passwords, text processing, time manipulation, and translations

### Localization/
- **TranslateExtension.cs** - WPF markup extension for multilingual support

### Migrations/
- **Entity Framework Migrations** - Database schema versioning and updates

### Modules/
- **Forfeit/** - Forfeit/disqualification management functionality
- **Home/** - Dashboard and home screen modules
- **Login/** - User authentication interface
- **Planning/** - Competition planning and scheduling system (ViewModels & Views)
- **Properties/** - Application configuration and settings management
- **SelectNewCompetition/** - Competition selection and creation interface

### Services/
- **API Services** - External API communication and authentication
- **Business Services** - Core application services (Excel export, XML data, validation, storage)
- **Infrastructure Services** - Cross-cutting concerns (dialog management, image handling, localization)

### Resources/
- **Application Resources** - Icons, images, and static assets

## RescueScoreManager.Tests/
- **Unit Tests** - Test project for application components

## Configuration Files
- **Settings.XamlStyler** - XAML code formatting rules
- **exclusions.dic** - Spell check exclusions

# Coding Standards

- Use async/await for all I/O operations
- ViewModels should inherit from ObservableObject and use [ObservableProperty] attributes.
  Implement IDisposable if necessary.
- Follow naming: PascalCase for properties, camelCase for private fields
- XAML: Use x:Name for controls that need code-behind access

# Common Patterns

## Architecture Patterns

### MVVM (Model-View-ViewModel)
- **Models**: Data entities in Data/ folder (Competition, Race, Athlete, etc.)
- **Views**: XAML files in Modules/*/Views/ folders
- **ViewModels**: Corresponding ViewModel classes in Modules/*/ViewModels/ folders
- **Base Classes**: BaseViewModel.cs provides common functionality

### Dependency Injection
- Service interfaces defined with I prefix (e.g., IXMLService, IDialogService)
- Concrete implementations without prefix (e.g., XMLService, DialogService)
- Services registered in App.xaml.cs and injected into ViewModels

### Repository Pattern
- XMLService acts as data repository for XML persistence
- ApiService handles external API data access
- Entity Framework context (RescueScoreManagerContext) for database operations

## Code Organization Patterns

### Feature-Based Modules
```
Modules/
├── Planning/
│   ├── ViewModels/
│   └── Views/
├── Properties/
│   ├── ViewModels/
│   └── Views/
└── Home/
    ├── ViewModels/
    └── Views/
```

### Separation of Concerns
- **Data Layer**: Data/ folder with entities and context
- **Business Logic**: Services/ folder with business services
- **UI Layer**: Modules/ with Views and ViewModels
- **Utilities**: Helpers/, Converter/, Comparer/ folders

### Configuration Management
- Centralized constants in Constants/AppConstants.cs
- Application settings in Properties/Settings.settings

## WPF-Specific Patterns

### Value Converters
- Named with descriptive suffixes (e.g., ToVisibilityConverter, ToBrushConverter)
- Located in dedicated Converter/ folder
- Handle data transformation for UI binding

### Resource Management
- Global resources in App.xaml
- Localized resources in Properties/Resources.*.resx
- Asset organization in Assets/ and Resources/ folders

### Dialog Pattern
- Dialog views suffixed with Dialog.xaml
- Corresponding ViewModels with DialogViewModel suffix
- Managed through IDialogService abstraction

## Data Patterns

### Entity Relationships
- Competition → Races → Heats → Results hierarchy
- Club → Athletes relationship
- Program → Meetings → Slots structure

### Data Persistence
- XML serialization through XMLService
- Entity Framework for database operations
- API integration for external data sync

### Data Validation
- IValidationService for business rule validation
- Entity validation attributes
- Input validation in ViewModels

## Naming Conventions

### Files and Classes
- **Views**: *View.xaml (e.g., PlanningView.xaml)
- **ViewModels**: *ViewModel.cs (e.g., PlanningViewModel.cs)
- **Services**: *Service.cs with interface I*Service.cs
- **Dialogs**: *Dialog.xaml (e.g., SiteCreationDialog.xaml)

### Properties and Methods
- Observable properties with OnPropertyChanged pattern
- Command methods prefixed with On (e.g., OnSaveCommand)
- Private fields prefixed with underscore (e.g., _xmlService)

## Error Handling Patterns

### Service Layer
- Try-catch blocks with logging in service methods
- IExceptionHandlingService for centralized error management
- Graceful degradation with user-friendly messages

### UI Layer
- Error dialogs through IDialogService
- Validation feedback in ViewModels
- Loading states and error indicators

## Localization Patterns

### Resource-Based Localization
- Resources.resx and Resources.en-US.resx for multi-language support
- TranslateExtension for XAML markup localization
- ILocalizationService for programmatic translations

### Cultural Formatting
- LocalizedFormatConverter for culture-specific data display
- Date/time formatting through TimeHelper
- Number and currency formatting considerations

## Testing Patterns

### Unit Testing
- Separate test project (RescueScoreManager.Tests)
- ViewModel testing with mocked services
- Naming convention: *Tests.cs (e.g., MainWindowViewModelTests.cs)

## Configuration Patterns

### Build Configuration
- Directory.Build.props for project-wide MSBuild properties
- Directory.Packages.props for centralized package management
- Environment-specific configurations

### Styling and Theming
- XAML styling with Settings.XamlStyler configuration
- Consistent color schemes and typography
- Reusable styles and templates