using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using trustesseApp.Infrastructure.Extentions;

namespace trustesseApp.Core;

 [AttributeUsage(AttributeTargets.Property)]
    public class CheckNumberAttribute : ValidationAttribute
    {
        private readonly int _compareValue;

        public CheckNumberAttribute(int compareValue)
        {
            _compareValue = compareValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is int num)
            {
                if (num <= _compareValue)
                {
                    return new ValidationResult(ErrorMessage);
                }

                return ValidationResult.Success;
            }

            if (value is long num2)
            {
                if (num2 <= _compareValue)
                {
                    return new ValidationResult(ErrorMessage);
                }

                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

      [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class CheckNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return new ValidationResult(ErrorMessage ?? "The name cannot be empty.");
                }

                if (!Regex.IsMatch(name, "^[a-zA-Z.' -]{4,80}$", RegexOptions.Compiled))
                {
                    return new ValidationResult(ErrorMessage ?? "The name is not in a valid format.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? "The value is not a valid string.");
        }
    }


[AttributeUsage(AttributeTargets.Property)]
public class CheckEmailAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        string text = value as string;
        if (string.IsNullOrEmpty(text))
        {
            return ValidationResult.Success;
        }
        if (!text.IsValidEmail())
        {
            return new ValidationResult(base.ErrorMessageString);
        }
        return ValidationResult.Success;
    }
}
