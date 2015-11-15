using System;

namespace Eru.ExceptionHandling
{
    public class Error<TErrorIdentifier>
    {
        public Exception Exception { get; private set; }
        public TErrorIdentifier Identifier { get; private set; }

        public Error(TErrorIdentifier identifier, Exception exception)
        {
            Identifier = identifier;
            Exception = exception;
        }
    }
}