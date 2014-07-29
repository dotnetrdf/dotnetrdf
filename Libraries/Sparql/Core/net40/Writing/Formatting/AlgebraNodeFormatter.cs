using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// Node formatter used for formatting algebra as part of <strong>ToString()</strong> implementations for <see cref="IAlgebra"/> implementations
    /// </summary>
    public class AlgebraNodeFormatter
        // TODO Would be nicer to extend another formatter that uses some syntax compressions
        : NTriples11Formatter, IAlgebraFormatter
    {
        protected override string FormatVariableNode(INode v, QuadSegment? segment)
        {
            return String.Format("?{0}", v.VariableName);
        }
    }
}
