using System;

namespace Eru.ErrorHandling
{
    public class Exception<TExceptionIdentifier> : Failure<TExceptionIdentifier>
    {
        public Exception(TExceptionIdentifier identifier, Exception ex) : base(identifier)
        {
            Identifier = identifier;
            Ex = ex;
        }

        public Exception Ex { get; private set; }
        public TExceptionIdentifier Identifier { get; private set; }
    }
}