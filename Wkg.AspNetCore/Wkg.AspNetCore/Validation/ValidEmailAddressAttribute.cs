using System.ComponentModel.DataAnnotations;
using Wkg.Data.Validation;

namespace Wkg.AspNetCore.Validation;

/// <summary>
/// Specifies that a data field value must be a valid email address conforming to the email address format specified in RFC 5322.
/// </summary>
/// <remarks>
/// Contrary to <see cref="EmailAddressAttribute"/>, this attribute uses <see cref="DataValidationService.IsEmailAddress(string?)"/> to validate against the email address format, 
/// instead of simply checking for the presence of an <c>@</c> character.
/// </remarks>
public class ValidEmailAddressAttribute : DataTypeAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidEmailAddressAttribute"/> class.
    /// </summary>
    public ValidEmailAddressAttribute() : base(DataType.EmailAddress)
    {
        ErrorMessage ??= "The {0} field is not a valid e-mail address.";
    }

    /// <summary>
    /// Determines whether the specified value conforms to the email address format specified in RFC 5322.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><see langword="true" /> if the specified value is a valid email address or <see langword="null" />; otherwise, <see langword="false" />.</returns>
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
        return DataValidationService.IsEmailAddress(valueAsString);
    }
}
