using System.ComponentModel.DataAnnotations;
using Wkg.Data.Validation;

namespace Wkg.AspNetCore.Validation;

/// <summary>
/// Specifies that a data field value must be a valid phone number.
/// </summary>
/// <remarks>
/// This attribute uses <see cref="DataValidationService.IsPhoneNumber(string?)"/> to validate against the phone number format.
/// </remarks>
public class ValidPhoneNumberAttribute : DataTypeAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidPhoneNumberAttribute"/> class.
    /// </summary>
    public ValidPhoneNumberAttribute() : base(DataType.PhoneNumber)
    {
        ErrorMessage ??= "The {0} field is not a valid phone number.";
    }

    /// <summary>
    /// Determines whether the specified value conforms to the phone number format.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><see langword="true" /> if the specified value is a valid phone number or <see langword="null" />; otherwise, <see langword="false" />.</returns>
    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is not string valueAsString)
        {
            return false;
        }
        return DataValidationService.IsPhoneNumber(valueAsString);
    }
}
