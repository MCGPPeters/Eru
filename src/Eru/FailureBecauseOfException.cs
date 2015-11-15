using System;
using System.Collections.Generic;

namespace Eru
{
    public class FailureBecauseOfException<T> : Failure<T>
    {
        public FailureBecauseOfException(IEnumerable<T> causeIdentifiers, Exception exception)
        {
            CauseIdentifiers = causeIdentifiers;
            Exception = exception;
        }

        public Exception Exception { get; set; }
    }
}