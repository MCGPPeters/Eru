using System;

namespace Eru.ErrorHandling
{
    public class Error<TErrorIdentifier>
    {
        public Error(TErrorIdentifier identifier, Exception exception)
        {
            Identifier = identifier;
            Exception = exception;
        }

        public Exception Exception { get; private set; }
        public TErrorIdentifier Identifier { get; private set; }
    }
}