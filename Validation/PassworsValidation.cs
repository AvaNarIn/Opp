using System.ComponentModel.DataAnnotations;

public class PasswordAttribute : ValidationAttribute {
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
        if (value == null || value is string { Length: 0 }) {
            return ValidationResult.Success;
        }

        if (value is not string password) {
            return new ValidationResult("invalid password");
        }

        if (password.Length < 8) {
            return new ValidationResult("must be at least 8 characters");
        }

        if (!password.Any(char.IsLetter)
            || !password.Any(char.IsDigit)) {

            return new ValidationResult(
                "must contain at least one letter and one digit");
        }
        
        return ValidationResult.Success;
    }
}