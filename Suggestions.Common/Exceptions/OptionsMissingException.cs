using System;

namespace Suggestions.Common.Exceptions
{
    public class OptionsMissingException : Exception
    {
        public OptionsMissingException(string parameter) : base("Отсутствует значение параметра " + parameter)
        {
        }
    }
}