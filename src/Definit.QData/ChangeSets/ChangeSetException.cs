using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Definit.QData.ChangeSets
{
    [Serializable]
    public class ChangeSetException : Exception
    {
        public ChangeSetException()
        {
        }

        public ChangeSetException(string message) : base(message)
        {
        }

        public ChangeSetException(string message, Exception inner) : base(message, inner)
        {
        }

    }

    [Serializable]
    public class ChangeSetInvalidException : ChangeSetException
    {
        public ChangeSetInvalidException()
        {
        }

        public ChangeSetInvalidException(string message) : base(message)
        {
        }

        public ChangeSetInvalidException(string message, Exception inner) : base(message, inner)
        {
        }

    }

    [Serializable]
    public class ChangeSetConcurrencyException : ChangeSetException
    {
        public ChangeSetConcurrencyException()
        {
        }

        public ChangeSetConcurrencyException(string message) : base(message)
        {
        }

        public ChangeSetConcurrencyException(string message, Exception inner) : base(message, inner)
        {
        }

    }
}
