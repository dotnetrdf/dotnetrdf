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
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;

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
    }

    internal abstract class AssignmentVariableNamePart<TExpression>
    {
        private readonly Func<IExpressionBuilder, PrimaryExpression<TExpression>> _buildAssignmentExpression;

        protected AssignmentVariableNamePart(Func<IExpressionBuilder, PrimaryExpression<TExpression>> buildAssignmentExpression)
        {
            _buildAssignmentExpression = buildAssignmentExpression;
        }

#if NET35
        private readonly Func<INonAggregateExpressionBuilder, PrimaryExpression<TExpression>> _buildNonAggregateAssignmentExpression;

        protected AssignmentVariableNamePart(Func<INonAggregateExpressionBuilder, PrimaryExpression<TExpression>> buildAssignmentExpression)
        {
            _buildNonAggregateAssignmentExpression = buildAssignmentExpression;
        }

        protected TExpression BuildAssignmentExpression(INamespaceMapper prefixes)
        {
            var expressionBuilder = new ExpressionBuilder(prefixes);

            if(_buildAssignmentExpression != null)
            {
                return _buildAssignmentExpression(expressionBuilder).Expression;
            }
            else
            {
                return _buildNonAggregateAssignmentExpression(expressionBuilder).Expression;
            }
        }
#else
        protected TExpression BuildAssignmentExpression(INamespaceMapper prefixes)
        {
            var expressionBuilder = new ExpressionBuilder(prefixes);

            return _buildAssignmentExpression(expressionBuilder).Expression;
        }
#endif
    }
}