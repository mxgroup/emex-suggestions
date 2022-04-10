using System;

namespace Suggestions.Common.Exceptions
{
    public class BadOperationException : Exception
    {
        public BadOperationException(string message) : base(message)
        {
        }
    }
}