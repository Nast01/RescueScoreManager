using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RescueScoreManager.Services;
public interface IValidationService
{
    ValidationResult ValidateObject<T>(T obj) where T : class;
    bool IsValid<T>(T obj) where T : class;
    IEnumerable<string> GetValidationErrors<T>(T obj) where T : class;
}
