using System.ComponentModel.DataAnnotations;

namespace EventBookingSystemV1.DTOs
{
    /// <summary>
    /// Validation attribute that only passes if the boolean property is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return value is bool b && b;
        }
    }
}
