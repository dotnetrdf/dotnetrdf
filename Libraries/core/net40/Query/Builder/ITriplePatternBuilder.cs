using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for building triple patterns
    /// </summary>
    public interface ITriplePatternBuilder
    {
        /// <summary>
        /// Sets a variable as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        TriplePatternPredicatePart Subject(string subjectVariableName);
        /// <summary>
        /// Depending on the generic parameter type, sets a literal, a QName or a blank node as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        /// <param name="subject">Either a variable name, a literal, a QName or a blank node identifier</param>
        /// <remarks>A relevant prefix/base URI must be added to <see cref="ICommonQueryBuilder.Prefixes"/> to accept a QName</remarks>
        TriplePatternPredicatePart Subject<TNode>(string subject) where TNode : INode;
        /// <summary>
        /// Depending on the <paramref name="subjectNode"/>'s type, sets a literal, a QName or a blank node as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        /// <remarks>A relevant prefix/base URI must be added to <see cref="ICommonQueryBuilder.Prefixes"/> to accept a QName</remarks>
        TriplePatternPredicatePart Subject(INode subjectNode);
        /// <summary>
        /// Sets a <see cref="Uri"/> as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        TriplePatternPredicatePart Subject(Uri subject);
        /// <summary>
        /// Sets a <see cref="PatternItem"/> as <see cref="IMatchTriplePattern.Subject"/>
        /// </summary>
        TriplePatternPredicatePart Subject(PatternItem subject);
    }
}