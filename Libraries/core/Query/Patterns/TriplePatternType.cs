using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Possible Types of Triple Pattern
    /// </summary>
    public enum TriplePatternType
    {
        /// <summary>
        /// Simple pattern matching
        /// </summary>
        Match,
        /// <summary>
        /// FILTER application
        /// </summary>
        Filter,
        /// <summary>
        /// BIND assignment
        /// </summary>
        BindAssignment,
        /// <summary>
        /// LET assignment
        /// </summary>
        LetAssignment,
        /// <summary>
        /// Sub-query
        /// </summary>
        SubQuery,
        /// <summary>
        /// Property Path
        /// </summary>
        Path,
        /// <summary>
        /// Property Function
        /// </summary>
        PropertyFunction
    }

    /// <summary>
    /// Comparer for Triple Pattern Types
    /// </summary>
    public class TriplePatternTypeComparer
        : IComparer<TriplePatternType>
    {
        /// <summary>
        /// Compares two triple pattern types
        /// </summary>
        /// <param name="x">Pattern Type</param>
        /// <param name="y">Pattern Type</param>
        /// <returns></returns>
        public int Compare(TriplePatternType x, TriplePatternType y)
        {
            return ((int)x).CompareTo((int)y);
        }
    }
}
