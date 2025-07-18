using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RescueScoreManager.Services;
public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(ILogger<ValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValidationResult ValidateObject<T>(T obj) where T : class
    {
        if (obj == null)
        {
            return new ValidationResult(false, new[] { "Object cannot be null" });
        }

        var context = new ValidationContext(obj);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        bool isValid = Validator.TryValidateObject(obj, context, results, true);

        var errors = results.Select(r => r.ErrorMessage ?? "Unknown validation error").ToList();

        _logger.LogDebug("Validation for {ObjectType}: {IsValid}, Errors: {ErrorCount}",
            typeof(T).Name, isValid, errors.Count);

        return new ValidationResult(isValid, errors);
    }

    public bool IsValid<T>(T obj) where T : class
    {
        return ValidateObject(obj).IsValid;
    }

    public IEnumerable<string> GetValidationErrors<T>(T obj) where T : class
    {
        return ValidateObject(obj).Errors;
    }
}

public record ValidationResult(bool IsValid, IEnumerable<string> Errors);