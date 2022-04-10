using System.ComponentModel.DataAnnotations;

namespace Suggestions.Common.Validation
{
    public class GreaterThanZero : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value != null && int.TryParse(value.ToString(), out var i) && i > 0;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"Значение {name} должно быть больше 0";
        }
    }
}
