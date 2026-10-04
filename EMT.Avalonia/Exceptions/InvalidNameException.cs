using System;
using System.Collections.Generic;

namespace EMT.Exceptions
{
    public class InvalidNameException : Exception
    {
        public IReadOnlyList<string> Problems { get; }

        public InvalidNameException(IReadOnlyList<string> problems)
            : base("Some keys can't be used:\n" + string.Join("\n", problems))
        {
            Problems = problems;
        }
    }
}
