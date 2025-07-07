using System.ComponentModel.DataAnnotations;

namespace PetLoveCommunity.API.Services;

/// <summary>
/// Service interface for validating configuration objects using Data Annotations.
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>
    /// Validates an object using Data Annotations and throws if validation fails.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="configurationName">The name of the configuration for error messages.</param>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    void ValidateAndThrow<T>(T instance, string configurationName) where T : class;
    
    /// <summary>
    /// Validates an object using Data Annotations and returns validation results.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    /// <param name="instance">The instance to validate.</param>
    /// <returns>A collection of validation results. Empty if validation passes.</returns>
    IEnumerable<ValidationResult> Validate<T>(T instance) where T : class;
}