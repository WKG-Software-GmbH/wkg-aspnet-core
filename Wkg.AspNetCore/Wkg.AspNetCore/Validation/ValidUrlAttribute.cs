using System.ComponentModel.DataAnnotations;
using Wkg.Data.Validation;

namespace Wkg.AspNetCore.Validation;

/// <summary>
/// Specifies that a data field value must be a valid HTTP, HTTPS, or FTP URL as defined by RFC 3986.
/// </summary>
/// <remarks>
/// Contrary to <see cref="UrlAttribute"/>, this attribute uses <see cref="DataValidationService.IsUrl(string?)"/> to validate against the URL format,
/// instead of simply checking whether the value starts with <c>http://</c>, <c>https://</c>, or <c>ftp://</c>.
/// </remarks>
public class ValidUrlAttribute : DataTypeAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidUrlAttribute"/> class.
    /// </summary>
    public ValidUrlAttribute() : base(DataType.Url)
    {
        ErrorMessage ??= "The {0} field is not a valid URL.";
    }

    /// <summary>
    /// Determines whether the specified value conforms to the URL format as defined by RFC 3986.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns><see langword="true" /> if the specified value is a valid URL or <see langword="null" />; otherwise, <see langword="false" />.</returns>
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
        return DataValidationService.IsUrl(valueAsString);
    }
}
