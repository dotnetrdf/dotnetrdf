/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using VDS.RDF.Namespaces;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Exposes method for assigning a name to an expression variable
    /// </summary>
    public interface IAssignmentVariableNamePart<out T>
    {
        /// <summary>
        /// Set the expression's variable name
        /// </summary>
        /// <returns>the parent query or graph pattern builder</returns>
        T As(string variableName);

        //{
        //    var expressionBuilder = new ExpressionBuilder(_prefixes);
        //    var assignment = _buildAssignmentExpression(expressionBuilder);
        //    var bindPattern = new BindPattern(variableName, assignment.Expression);
        //    if (_selectBuilder != null)
        //    {
        //        _selectBuilder.And(new SparqlVariable(variableName, assignment.Expression));
        //        return (T)_selectBuilder;
        //    }

        //    if (_graphPatternBuilder != null)
        //    {
        //        _graphPatternBuilder.Where(bindPattern);
        //        return (T)_graphPatternBuilder;
        //    }

        //    // todo: refactor as lookup table
        //    throw new InvalidOperationException(string.Format("Invalid type of T for creating assignment: {0}", typeof(T)));
        //}
    }

    internal abstract class AssignmentVariableNamePart
    {
        private readonly Func<ExpressionBuilder, SparqlExpression> _buildAssignmentExpression;

        protected AssignmentVariableNamePart(Func<ExpressionBuilder, SparqlExpression> buildAssignmentExpression)
        {
            _buildAssignmentExpression = buildAssignmentExpression;
        }

        protected ISparqlExpression BuildAssignmentExpression(INamespaceMapper prefixes)
        {
            var expressionBuilder = new ExpressionBuilder(prefixes);
            var assignment = _buildAssignmentExpression(expressionBuilder);
            return assignment.Expression;
        }
    }
}