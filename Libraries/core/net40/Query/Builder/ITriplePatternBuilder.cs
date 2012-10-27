using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public interface ITriplePatternBuilder
    {
        TriplePatternPredicatePart Subject(string subjectVariableName);
        TriplePatternPredicatePart Subject<TNode>(string subject) where TNode : INode;
        TriplePatternPredicatePart Subject(INode subjectNode);
        TriplePatternPredicatePart Subject(Uri subject);
        TriplePatternPredicatePart Subject(PatternItem subject);
    }
}