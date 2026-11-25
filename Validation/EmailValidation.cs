using System.ComponentModel.DataAnnotations;

public class EmailValidationAttribute : ValidationAttribute
{
    public EmailValidationAttribute()
    {
        ErrorMessage = "invalid email format";
    }

    public override bool IsValid(object? value)
    {
        if (value == null || value is string text && text.Length == 0)
        {
            return true;
        }

        var email = new EmailAddressAttribute();

        var pattern = new RegularExpressionAttribute(
            @"[^\s@.]+(?:\.[^\s@.]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}");

        return email.IsValid(value) && pattern.IsValid(value);
    }
}