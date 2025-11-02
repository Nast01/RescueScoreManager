# Add Service Interface

Creates a service interface and implementation pair following RescueScoreManager dependency injection patterns.

## Parameters
- `service_name` (required): Name of the service (e.g., "Email", "Report", "Validation")
- `service_type` (required): Type of service - "business", "infrastructure", or "data"
- `methods` (optional): Comma-separated list of method names (e.g., "Send,Validate,Process")
- `dependencies` (optional): Comma-separated list of dependencies (e.g., "ILogger,IXMLService")

## Usage
```
@add-service-interface service_name="Email" service_type="infrastructure" methods="Send,SendBulk,ValidateAddress" dependencies="ILogger,ILocalizationService"
```

## Implementation Steps

1. **Create Interface**: `RescueScoreManager/Services/I{service_name}Service.cs`
   - Define contract with clear method signatures
   - Include XML documentation
   - Follow async patterns where appropriate

2. **Create Implementation**: `RescueScoreManager/Services/{service_name}Service.cs`
   - Implement interface
   - Add dependency injection constructor
   - Include proper error handling and logging
   - Follow project patterns

3. **Register in DI**: Add registration to `App.xaml.cs` service configuration

## Template Structure

### Interface Template:
```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RescueScoreManager.Services
{
    /// <summary>
    /// Service interface for {service_name} operations
    /// </summary>
    public interface I{service_name}Service
    {
        /// <summary>
        /// Example method - replace with actual methods
        /// </summary>
        /// <param name="input">Input parameter</param>
        /// <returns>Result of operation</returns>
        Task<bool> ProcessAsync(string input);

        /// <summary>
        /// Example synchronous method
        /// </summary>
        /// <param name="data">Data to process</param>
        /// <returns>Processed result</returns>
        string Process(string data);

        /// <summary>
        /// Example method with collections
        /// </summary>
        /// <returns>Collection of results</returns>
        IEnumerable<string> GetResults();
    }
}
```

### Implementation Template:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RescueScoreManager.Services
{
    /// <summary>
    /// Implementation of I{service_name}Service
    /// </summary>
    public class {service_name}Service : I{service_name}Service
    {
        private readonly ILogger<{service_name}Service> _logger;
        // Add other dependencies based on 'dependencies' parameter

        public {service_name}Service(
            ILogger<{service_name}Service> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Initialize other dependencies
        }

        /// <inheritdoc/>
        public async Task<bool> ProcessAsync(string input)
        {
            try
            {
                _logger.LogInformation("Processing {service_name} operation with input: {Input}", input);
                
                // TODO: Implement actual logic here
                await Task.Delay(100); // Remove this - placeholder for async work
                
                _logger.LogInformation("{service_name} operation completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {service_name} operation with input: {Input}", input);
                throw;
            }
        }

        /// <inheritdoc/>
        public string Process(string data)
        {
            try
            {
                _logger.LogInformation("Processing {service_name} data");
                
                if (string.IsNullOrWhiteSpace(data))
                {
                    throw new ArgumentException("Data cannot be null or empty", nameof(data));
                }

                // TODO: Implement actual logic here
                var result = data.Trim().ToUpperInvariant();
                
                _logger.LogInformation("{service_name} data processed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing {service_name} data");
                throw;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetResults()
        {
            try
            {
                _logger.LogInformation("Getting {service_name} results");
                
                // TODO: Implement actual logic here
                var results = new List<string> { "Result1", "Result2", "Result3" };
                
                _logger.LogInformation("Retrieved {Count} {service_name} results", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {service_name} results");
                throw;
            }
        }
    }
}
```

### DI Registration Template:
Add to `App.xaml.cs` in the service configuration section:

```csharp
// Add this line in the service registration area
services.AddScoped<I{service_name}Service, {service_name}Service>();
```

## Service Type Guidelines

### Business Services
- Core application logic
- Domain-specific operations
- Business rule validation
- Examples: `ICompetitionService`, `IRaceManagementService`

### Infrastructure Services
- Cross-cutting concerns
- External system integration
- Technical utilities
- Examples: `IEmailService`, `IFileService`, `IReportService`

### Data Services
- Data access and persistence
- Repository patterns
- Data transformation
- Examples: `IXMLService`, `IExportService`, `IImportService`

## Method Pattern Examples

### Async Operations (I/O, Network, File):
```csharp
Task<TResult> OperationAsync(TInput input, CancellationToken cancellationToken = default);
```

### Synchronous Operations (Pure logic):
```csharp
TResult Operation(TInput input);
```

### Validation Methods:
```csharp
bool IsValid(TInput input);
ValidationResult Validate(TInput input);
```

### Collection Operations:
```csharp
IEnumerable<T> GetItems();
IReadOnlyList<T> GetItemsList();
```

## Error Handling Patterns

### For Business Services:
- Log errors with context
- Throw business-specific exceptions
- Include user-friendly messages

### For Infrastructure Services:
- Log technical details
- Wrap external exceptions
- Provide fallback behaviors where appropriate

### For Data Services:
- Handle connection issues
- Validate data integrity
- Support transaction patterns

## Validation Checklist
- [ ] Interface follows naming convention (I{Name}Service)
- [ ] Implementation follows naming convention ({Name}Service)
- [ ] Includes proper dependency injection
- [ ] Has comprehensive error handling
- [ ] Includes logging for operations
- [ ] Methods have XML documentation
- [ ] Async methods follow Task patterns
- [ ] Registered in DI container
- [ ] Follows single responsibility principle
- [ ] Includes appropriate unit tests (recommended)