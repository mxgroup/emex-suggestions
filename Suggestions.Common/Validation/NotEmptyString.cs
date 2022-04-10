using System.ComponentModel.DataAnnotations;

namespace Suggestions.Common.Validation
{
    public class NotEmptyString : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return (value as string)?.Length > 0;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"Значение {name} должно быть непустой строкой";
        }
    }
}
