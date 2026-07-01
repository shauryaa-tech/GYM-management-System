using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GymManagement.Helpers
{
    public class CallbackUrlAttribute : ValidationAttribute, IClientModelValidator
    {
        public CallbackUrlAttribute()
            : base("Enter a valid URL or path starting with /")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var error = GetValidationError(value?.ToString());
            return error == null
                ? ValidationResult.Success
                : new ValidationResult(error);
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-callbackurl", ErrorMessage ?? FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        }

        private static string? GetValidationError(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            url = url.Trim();

            if (url.StartsWith('/'))
                return null;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return null;
            }

            return "Enter a valid URL or path starting with /";
        }

        private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
                return false;

            attributes.Add(key, value);
            return true;
        }
    }
}
