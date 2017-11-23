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
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    sealed class BindAssignmentVariableNamePart :
        AssignmentVariableNamePart<ISparqlExpression>,
        IAssignmentVariableNamePart<IGraphPatternBuilder>,
        IAssignmentVariableNamePart<IQueryBuilder>
    {
        private readonly GraphPatternBuilder _graphPatternBuilder;
        private readonly QueryBuilder _queryBuilder;

        internal BindAssignmentVariableNamePart(QueryBuilder queryBuilder, Func<IExpressionBuilder, PrimaryExpression<ISparqlExpression>> buildAssignmentExpression)
            : this(queryBuilder.RootGraphPatternBuilder, buildAssignmentExpression)
        {
            _queryBuilder = queryBuilder;
        }

        internal BindAssignmentVariableNamePart(GraphPatternBuilder graphPatternBuilder, Func<IExpressionBuilder, PrimaryExpression<ISparqlExpression>> buildAssignmentExpression)
            :base(buildAssignmentExpression)
        {
            _graphPatternBuilder = graphPatternBuilder;
        }

#if NET35
        internal BindAssignmentVariableNamePart(QueryBuilder queryBuilder, Func<INonAggregateExpressionBuilder, PrimaryExpression<ISparqlExpression>> buildAssignmentExpression, bool nonAggregate)
            : this(queryBuilder.RootGraphPatternBuilder, buildAssignmentExpression, true)
        {
            _queryBuilder = queryBuilder;
        }

        internal BindAssignmentVariableNamePart(GraphPatternBuilder graphPatternBuilder, Func<INonAggregateExpressionBuilder, PrimaryExpression<ISparqlExpression>> buildAssignmentExpression, bool nonAggregate)
            : base(buildAssignmentExpression)
        {
            _graphPatternBuilder = graphPatternBuilder;
        }
#endif

        IGraphPatternBuilder IAssignmentVariableNamePart<IGraphPatternBuilder>.As(string variableName)
        {
            _graphPatternBuilder.Where(mapper => new ITriplePattern[] { new BindPattern(variableName, BuildAssignmentExpression(mapper)) });

            return _graphPatternBuilder;
        }

        IQueryBuilder IAssignmentVariableNamePart<IQueryBuilder>.As(string variableName)
        {
            _queryBuilder.RootGraphPatternBuilder.Where(mapper => new ITriplePattern[] { new BindPattern(variableName, BuildAssignmentExpression(mapper)) });

            return _queryBuilder;
        }
    }
}