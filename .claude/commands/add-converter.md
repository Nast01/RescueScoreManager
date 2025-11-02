# Add Converter

Creates a WPF value converter following RescueScoreManager converter patterns with proper registration and usage examples.

## Parameters
- `converter_name` (required): Name of the converter (e.g., "StatusToColor", "CountToVisibility", "DateToText")
- `source_type` (required): Source data type (e.g., "bool", "int", "string", "DateTime", "enum")
- `target_type` (required): Target UI type (e.g., "Visibility", "Brush", "string", "double")
- `conversion_logic` (optional): Description of conversion logic
- `supports_convert_back` (optional): Set to "true" if two-way conversion needed (default: false)

## Usage
```
@add-converter converter_name="StatusToColor" source_type="string" target_type="Brush" conversion_logic="Active=Green,Inactive=Gray,Error=Red" supports_convert_back="false"
```

## Implementation Steps

1. **Create Converter Class**: `RescueScoreManager/Converter/{converter_name}Converter.cs`
   - Implement IValueConverter interface
   - Add conversion logic in Convert method
   - Implement ConvertBack if two-way conversion needed
   - Include proper error handling

2. **Register in App.xaml**: Add converter to application resources
   - Add to App.xaml resource dictionary
   - Use proper x:Key naming

3. **Update Documentation**: Add usage examples and patterns

## Template Structure

### Converter Class Template:
```csharp
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RescueScoreManager.Converter
{
    /// <summary>
    /// Converts {source_type} to {target_type}
    /// {conversion_logic}
    /// </summary>
    public class {converter_name}Converter : IValueConverter
    {
        /// <summary>
        /// Converts {source_type} to {target_type}
        /// </summary>
        /// <param name="value">The {source_type} value to convert</param>
        /// <param name="targetType">Target type (should be {target_type})</param>
        /// <param name="parameter">Optional parameter for conversion</param>
        /// <param name="culture">Culture info for conversion</param>
        /// <returns>Converted {target_type} value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                // Handle null values
                if (value == null)
                    return GetDefaultValue();

                // Convert based on source type
                switch (value)
                {
                    case {source_type} sourceValue:
                        return ConvertValue(sourceValue, parameter);
                    
                    default:
                        return GetDefaultValue();
                }
            }
            catch (Exception)
            {
                // Return default value on conversion error
                return GetDefaultValue();
            }
        }

        /// <summary>
        /// Converts {target_type} back to {source_type}
        /// </summary>
        /// <param name="value">The {target_type} value to convert back</param>
        /// <param name="targetType">Target type (should be {source_type})</param>
        /// <param name="parameter">Optional parameter for conversion</param>
        /// <param name="culture">Culture info for conversion</param>
        /// <returns>Converted {source_type} value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Only implement if supports_convert_back is true
            #if SUPPORTS_CONVERT_BACK
            try
            {
                if (value == null)
                    return GetDefaultSourceValue();

                switch (value)
                {
                    case {target_type} targetValue:
                        return ConvertBackValue(targetValue, parameter);
                    
                    default:
                        return GetDefaultSourceValue();
                }
            }
            catch (Exception)
            {
                return GetDefaultSourceValue();
            }
            #else
            throw new NotSupportedException("ConvertBack is not supported by this converter");
            #endif
        }

        /// <summary>
        /// Performs the actual conversion logic
        /// </summary>
        private {target_type} ConvertValue({source_type} value, object? parameter)
        {
            // TODO: Implement specific conversion logic based on converter type
            
            // Example for bool to Visibility
            if (typeof({target_type}) == typeof(Visibility))
            {
                return (value as bool? ?? false) ? Visibility.Visible : Visibility.Collapsed;
            }
            
            // Example for string to Brush
            if (typeof({target_type}) == typeof(Brush))
            {
                return value?.ToString()?.ToLower() switch
                {
                    "active" => new SolidColorBrush(Colors.Green),
                    "inactive" => new SolidColorBrush(Colors.Gray),
                    "error" => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            
            // Example for int to string
            if (typeof({target_type}) == typeof(string))
            {
                return value?.ToString() ?? string.Empty;
            }
            
            return GetDefaultValue();
        }

        #if SUPPORTS_CONVERT_BACK
        /// <summary>
        /// Performs the actual back conversion logic
        /// </summary>
        private {source_type} ConvertBackValue({target_type} value, object? parameter)
        {
            // TODO: Implement specific back conversion logic
            return GetDefaultSourceValue();
        }

        /// <summary>
        /// Gets the default source value
        /// </summary>
        private {source_type} GetDefaultSourceValue()
        {
            if (typeof({source_type}) == typeof(bool))
                return false;
            if (typeof({source_type}) == typeof(int))
                return 0;
            if (typeof({source_type}) == typeof(string))
                return string.Empty;
            
            return default({source_type});
        }
        #endif

        /// <summary>
        /// Gets the default target value for error cases
        /// </summary>
        private {target_type} GetDefaultValue()
        {
            if (typeof({target_type}) == typeof(Visibility))
                return Visibility.Collapsed;
            if (typeof({target_type}) == typeof(Brush))
                return new SolidColorBrush(Colors.Transparent);
            if (typeof({target_type}) == typeof(string))
                return string.Empty;
            if (typeof({target_type}) == typeof(double))
                return 0.0;
            
            return default({target_type});
        }
    }
}
```

### App.xaml Registration Template:
Add to `App.xaml` in the `<Application.Resources>` section:

```xaml
<!-- Add this to App.xaml resources -->
<converter:{converter_name}Converter x:Key="{converter_name}Converter"/>
```

Also add the namespace reference at the top of App.xaml:
```xaml
xmlns:converter="clr-namespace:RescueScoreManager.Converter"
```

## Common Converter Patterns

### Boolean to Visibility:
```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is bool boolValue)
    {
        // Check for invert parameter
        bool invert = parameter?.ToString()?.ToLower() == "invert";
        bool result = invert ? !boolValue : boolValue;
        return result ? Visibility.Visible : Visibility.Collapsed;
    }
    return Visibility.Collapsed;
}
```

### String to Brush:
```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    return (value?.ToString()?.ToLower()) switch
    {
        "success" or "active" => new SolidColorBrush(Colors.Green),
        "warning" or "pending" => new SolidColorBrush(Colors.Orange),
        "error" or "inactive" => new SolidColorBrush(Colors.Red),
        _ => new SolidColorBrush(Colors.Gray)
    };
}
```

### Enum to String:
```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is Enum enumValue)
    {
        // Use localization service if available
        return enumValue.ToString();
    }
    return string.Empty;
}
```

### Count to Visibility:
```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is int count)
    {
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    if (value is ICollection collection)
    {
        return collection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    return Visibility.Collapsed;
}
```

### DateTime to String:
```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    if (value is DateTime dateTime)
    {
        string format = parameter?.ToString() ?? "dd/MM/yyyy";
        return dateTime.ToString(format, culture);
    }
    return string.Empty;
}
```

## Usage Examples

### In XAML Binding:
```xaml
<!-- Basic usage -->
<TextBlock Visibility="{Binding IsVisible, Converter={StaticResource BooleanToVisibilityConverter}}"/>

<!-- With parameter -->
<Border Background="{Binding Status, Converter={StaticResource StatusToColorConverter}}"/>

<!-- With converter parameter -->
<TextBlock Visibility="{Binding IsHidden, Converter={StaticResource BooleanToVisibilityConverter}, ConverterParameter=invert}"/>

<!-- In DataTrigger -->
<Style.Triggers>
    <DataTrigger Binding="{Binding Status, Converter={StaticResource StatusToStringConverter}}" Value="Active">
        <Setter Property="Foreground" Value="Green"/>
    </DataTrigger>
</Style.Triggers>
```

### MultiBinding Usage:
For complex conversions, consider IMultiValueConverter:
```xaml
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource MultiValueConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

## Testing Considerations

### Unit Test Template:
```csharp
[Test]
public void Convert_ValidInput_ReturnsExpectedOutput()
{
    // Arrange
    var converter = new {converter_name}Converter();
    var input = /* test input */;
    var expected = /* expected output */;

    // Act
    var result = converter.Convert(input, typeof({target_type}), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.AreEqual(expected, result);
}

[Test]
public void Convert_NullInput_ReturnsDefaultValue()
{
    // Arrange
    var converter = new {converter_name}Converter();

    // Act
    var result = converter.Convert(null, typeof({target_type}), null, CultureInfo.InvariantCulture);

    // Assert
    Assert.IsNotNull(result);
    // Assert default value is returned
}
```

## Performance Considerations

### Caching for Expensive Conversions:
```csharp
private static readonly Dictionary<string, Brush> _colorCache = new();

public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
{
    string key = value?.ToString() ?? "default";
    
    if (_colorCache.TryGetValue(key, out Brush? cachedBrush))
        return cachedBrush;
    
    var brush = CreateBrush(key);
    _colorCache[key] = brush;
    return brush;
}
```

## Validation Checklist
- [ ] Converter class in Converter/ folder
- [ ] Implements IValueConverter interface
- [ ] Handles null input gracefully
- [ ] Returns appropriate default values
- [ ] Includes XML documentation
- [ ] Registered in App.xaml resources
- [ ] ConvertBack implemented if needed
- [ ] Error handling for invalid inputs
- [ ] Performance optimized for frequent use
- [ ] Unit tests created (recommended)
- [ ] Usage examples documented