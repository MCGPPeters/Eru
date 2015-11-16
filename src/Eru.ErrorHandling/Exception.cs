using System;

namespace Eru.ErrorHandling
{
    public class Exception<TExceptionIdentifier>
    {
        public Exception(TExceptionIdentifier identifier, Exception ex)
        {
            Identifier = identifier;
            Ex = ex;
        }

        public Exception Ex { get; private set; }
        public TExceptionIdentifier Identifier { get; private set; }
    }
}