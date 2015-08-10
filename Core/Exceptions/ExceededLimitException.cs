using System;

namespace Core.Exceptions
{
    public class ExceededLimitException : Exception
    {
        public ExceededLimitException(string message)
            : base(message)
        {
        }

        public ExceededLimitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}