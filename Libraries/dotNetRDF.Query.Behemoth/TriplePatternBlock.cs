using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query.Patterns;

namespace dotNetRDF.Query.Behemoth
{
    class TriplePatternBlock : IEvaluationBlock
    {
        private readonly TriplePattern _pattern;
        private readonly BehemothEvaluationContext _context;
        public TriplePatternBlock(TriplePattern triplePattern, BehemothEvaluationContext context)
        {
            _pattern = triplePattern;
            _context = context;
        }

        public IEnumerable<Bindings> Evaluate(Bindings input)
        {
            var subjVar = _pattern.Subject.VariableName;
            var predVar = _pattern.Predicate.VariableName;
            var objVar = _pattern.Object.VariableName;
            var boundSubj = subjVar != null && input.ContainsVariable(subjVar);
            var boundPred = predVar != null && input.ContainsVariable(predVar);
            var boundObj = objVar != null && input.ContainsVariable(objVar);
            var boundPattern = new TriplePattern(
                boundSubj ? new NodeMatchPattern(input[subjVar]) : _pattern.Subject,
                boundPred ? new NodeMatchPattern(input[predVar]) : _pattern.Predicate,
                boundObj ? new NodeMatchPattern(input[objVar]) : _pattern.Object);

            switch (boundPattern.IndexType)
            {
                case TripleIndexType.NoVariables:
                    if (_context.Data.ContainsTriple(new Triple(
                        ((NodeMatchPattern)_pattern.Subject).Node,
                        ((NodeMatchPattern)_pattern.Predicate).Node,
                        ((NodeMatchPattern)_pattern.Object).Node
                    )))
                    {
                        return input.AsEnumerable();
                    }

                    break;

                case TripleIndexType.None:
                    return _context.Data.Triples.Select(t => input.Extend(
                        new KeyValuePair<string, INode>(subjVar, t.Subject),
                        new KeyValuePair<string, INode>(predVar, t.Predicate),
                        new KeyValuePair<string, INode>(objVar, t.Object)
                    ));
                
                case TripleIndexType.Object:
                    return _context.Data.GetTriplesWithObject(((NodeMatchPattern)boundPattern.Object).Node)
                        .Select(t => input.Extend(
                            new KeyValuePair<string, INode>(subjVar, t.Subject),
                            new KeyValuePair<string, INode>(predVar, t.Predicate)));
                
                case TripleIndexType.Predicate:
                    return _context.Data.GetTriplesWithPredicate(((NodeMatchPattern)boundPattern.Predicate).Node)
                        .Select(t => input.Extend(
                            new KeyValuePair<string, INode>(subjVar, t.Subject),
                            new KeyValuePair<string, INode>(objVar, t.Object)));
                
                case TripleIndexType.PredicateObject:
                    return _context.Data.GetTriplesWithPredicateObject(
                            ((NodeMatchPattern)boundPattern.Predicate).Node,
                            ((NodeMatchPattern)boundPattern.Object).Node)
                        .Select(t => input.Extend(
                            new KeyValuePair<string, INode>(subjVar, t.Subject)));
                
                case TripleIndexType.Subject:
                    return _context.Data.GetTriplesWithSubject(
                            ((NodeMatchPattern)boundPattern.Subject).Node)
                        .Select(t => input.Extend(
                            new KeyValuePair<string, INode>(predVar, t.Predicate),
                            new KeyValuePair<string, INode>(objVar, t.Object)));
                
                case TripleIndexType.SubjectObject:
                    return _context.Data.GetTriplesWithSubjectObject(((NodeMatchPattern)boundPattern.Subject).Node,
                            ((NodeMatchPattern)boundPattern.Object).Node)
                        .Select(t => input.Extend(
                            new KeyValuePair<string, INode>(predVar, t.Predicate)));
                
                case TripleIndexType.SubjectPredicate:
                    return _context.Data.GetTriplesWithSubjectPredicate(
                            ((NodeMatchPattern)boundPattern.Subject).Node,
                            ((NodeMatchPattern)boundPattern.Predicate).Node)
                        .Select(t => input.Extend(
                            new KeyValuePair<string, INode>(objVar, t.Object)));
                
            }
            return Enumerable.Empty<Bindings>();
        }
    }
}
