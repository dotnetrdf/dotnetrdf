using System;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// todo: extract interface?
    /// </summary>
    public sealed class TriplePatternPredicatePart
    {
        public TriplePatternObjectPart Predicate(string variableName)
        {
            throw new System.NotImplementedException();
        }

        public TriplePatternObjectPart Predicate(Uri predicateUri)
        {
            throw new NotImplementedException();
        }

        public TriplePatternObjectPart PredicateUri(string predicateQName)
        {
            throw new NotImplementedException();
        }

        public TriplePatternObjectPart Predicate(INode predicateNode)
        {
            throw new NotImplementedException();
        }
    }
}