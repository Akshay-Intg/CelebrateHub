using System.ComponentModel.DataAnnotations;

namespace CelebrateHub.Services.Attributes
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _age;

        public MinimumAgeAttribute(int age)
        {
            _age = age;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var dob = (DateTime)value;
            var today = DateTime.Today;

            var age = today.Year - dob.Year;

            if (dob.Date > today.AddYears(-age))
                age--;

            if (age < _age)
                return new ValidationResult($"You must be at least {_age} years old.");

            return ValidationResult.Success;
        }
    }
}
