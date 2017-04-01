using System;

namespace VDS.RDF.XunitExtensions
{
    public class SkipTestException : Exception
    {
        public SkipTestException(string reason)
            : base(reason) { }
    }
}
