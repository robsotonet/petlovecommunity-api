using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.API.Services;

/// <summary>
/// Service for validating configuration objects using Data Annotations.
/// </summary>
public class ConfigurationValidator : IConfigurationValidator
{
    /// <summary>
    /// Validates an object using Data Annotations and throws if validation fails.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="configurationName">The name of the configuration for error messages.</param>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public void ValidateAndThrow<T>(T instance, string configurationName) where T : class
    {
        var validationResults = Validate(instance);
        
        if (validationResults.Any())
        {
            var errorMessages = validationResults.Select(vr => vr.ErrorMessage);
            var combinedMessage = string.Join(", ", errorMessages);
            
            throw new InvalidOperationException(
                $"{configurationName} configuration validation failed: {combinedMessage}");
        }
    }
    
    /// <summary>
    /// Validates an object using Data Annotations and returns validation results.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A collection of validation results. Empty if validation passes.</returns>
    public IEnumerable<ValidationResult> Validate<T>(T instance) where T : class
    {
        var validationContext = new ValidationContext(instance);
        var validationResults = new List<ValidationResult>();
        
        Validator.TryValidateObject(instance, validationContext, validationResults, validateAllProperties: true);
        
        return validationResults;
    }
}