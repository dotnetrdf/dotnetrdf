using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

public class PullEvaluationContext : IPatternEvaluationContext
{
    public ISparqlDataset Data { get; }
    public bool RigorousEvaluation { get; }

    public PullEvaluationContext(ISparqlDataset data)
    {
        Data = data;
        RigorousEvaluation = true;
    }

    private INode? GetNode(PatternItem patternItem, ISet? inputBindings)
    {
        if (patternItem is NodeMatchPattern nodeMatchPattern) { return nodeMatchPattern.Node; }

        if (inputBindings != null && patternItem is VariablePattern variablePattern &&
            inputBindings.ContainsVariable(variablePattern.VariableName))
        {
            return inputBindings[variablePattern.VariableName];
        }

        return null;
    }

    internal IEnumerable<Triple> GetTriples(IMatchTriplePattern triplePattern, ISet? inputBindings)
    {
        // Expand quoted triple patterns in subject or object position of the triple pattern
        if (triplePattern.Subject is QuotedTriplePattern subjectTriplePattern)
        {
            return GetQuotedTriples(subjectTriplePattern).SelectMany(tn =>
                GetTriples(new TriplePattern(new NodeMatchPattern(tn), triplePattern.Predicate,
                    triplePattern.Object), inputBindings));
        }

        if (triplePattern.Object is QuotedTriplePattern objectTriplePattern)
        {
            return GetQuotedTriples(objectTriplePattern).SelectMany(tn =>
                GetTriples(new TriplePattern(triplePattern.Subject, triplePattern.Predicate,
                    new NodeMatchPattern(tn)), inputBindings));
        }

        INode? subj = GetNode(triplePattern.Subject, inputBindings);
        INode? pred = GetNode(triplePattern.Predicate, inputBindings);
        INode? obj = GetNode(triplePattern.Object, inputBindings);
        if (subj != null)
        {
            if (pred != null)
            {
                if (obj != null)
                {
                    // Return if the triple exists
                    var t = new Triple(subj, pred, obj);
                    return Data.ContainsTriple(t) ? t.AsEnumerable() : Enumerable.Empty<Triple>();
                }
                return Data.GetTriplesWithSubjectPredicate(subj, pred);
            }

            return obj != null ? Data.GetTriplesWithSubjectObject(subj, obj) : Data.GetTriplesWithSubject(subj);
        }

        if (pred != null)
        {
            return obj != null ? Data.GetTriplesWithPredicateObject(pred, obj) : Data.GetTriplesWithPredicate(pred);
        }

        return obj != null ? Data.GetTriplesWithObject(obj) : Data.Triples;
    }

    private IEnumerable<ITripleNode> GetQuotedTriples(QuotedTriplePattern qtp)
    {
        TriplePattern triplePattern = qtp.QuotedTriple;
        INode s, p, o;
        switch (triplePattern.IndexType)
        {
            case TripleIndexType.Subject:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                return Data.GetQuotedWithSubject(s).Select(t => new TripleNode(t));

            case TripleIndexType.Predicate:
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                return Data.GetQuotedWithPredicate(p).Select(t => new TripleNode(t));

            case TripleIndexType.Object:
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                return Data.GetQuotedWithObject(o).Select(t => new TripleNode(t));

            case TripleIndexType.SubjectPredicate:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                return Data.GetQuotedWithSubjectPredicate(s, p).Select(t => new TripleNode(t));

            case TripleIndexType.SubjectObject:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                return Data.GetQuotedWithSubjectObject(s, o).Select(t => new TripleNode(t));

            case TripleIndexType.PredicateObject:
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                return Data.GetQuotedWithPredicateObject(p, o).Select(t => new TripleNode(t));

            case TripleIndexType.NoVariables:
                s = ((NodeMatchPattern)triplePattern.Subject).Node;
                p = ((NodeMatchPattern)triplePattern.Predicate).Node;
                o = ((NodeMatchPattern)triplePattern.Object).Node;
                var t = new Triple(s, p, o);
                if (Data.ContainsQuotedTriple(t))
                {
                    return new[] { new TripleNode(t) };
                }
                else
                {
                    return Enumerable.Empty<ITripleNode>();
                }
            case TripleIndexType.None:
                return Data.QuotedTriples.Select(t => new TripleNode(t));

        }

        return Enumerable.Empty<ITripleNode>();
    }

    public bool ContainsVariable(string varName)
    {
        return true;
    }

    public bool ContainsValue(string varName, INode value)
    {
        return true;
    }
}