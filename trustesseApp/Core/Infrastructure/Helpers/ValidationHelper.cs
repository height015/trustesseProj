using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace trustesseApp.Core.Infrastructure.Helpers;

using System.ComponentModel.DataAnnotations;
using System.Text;


public static class ValidationHelper
{
    public static bool TryValidate(this object? model, out string msg)
    {
        if (model == null)
        {
            msg = "The model cannot be null.";
            return false;
        }

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, serviceProvider: null, items: null);

        bool isValid = Validator.TryValidateObject(
            model,
            context,
            validationResults,
            validateAllProperties: true
        );

        if (!isValid)
        {
            var errorDetail = new StringBuilder();
            errorDetail.AppendLine("Validation Error(s):");

            for (int i = 0; i < validationResults.Count; i++)
            {
                var error = validationResults[i];
                errorDetail.Append("- " + error.ErrorMessage);

                if (i < validationResults.Count - 1)
                {
                    errorDetail.Append("<br />");
                }
            }

            msg = errorDetail.ToString();
        }
        else
        {
            msg = string.Empty;
        }

        return isValid;
    }
}
