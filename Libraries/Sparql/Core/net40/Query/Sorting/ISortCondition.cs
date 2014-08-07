using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Sorting
{
    /// <summary>
    /// Interface for sort conditions
    /// </summary>
    public interface ISortCondition
        : IEquatable<ISortCondition>
    {
        /// <summary>
        /// Gets whether this is an ascending sort condition
        /// </summary>
        /// <returns>True if an ascending sort condition, false if a descending sort condition</returns>
        bool IsAscending { get; }

        /// <summary>
        /// Gets the expression used for sorting
        /// </summary>
        IExpression Expression { get; }

        IComparer<ISolution> CreateComparer(IExpressionContext context);
            
        String ToString();

        String ToString(IAlgebraFormatter formatter);

        String ToPrefixString();

        String ToPrefixString(IAlgebraFormatter formatter);
    }
}
