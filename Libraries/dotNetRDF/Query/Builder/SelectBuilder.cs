/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Builder.Expressions;

namespace VDS.RDF.Query.Builder
{
    internal sealed class SelectBuilder : QueryBuilder, ISelectBuilder
    {
        private readonly IList<Func<INamespaceMapper, SparqlVariable>> _buildSelectVariables = new List<Func<INamespaceMapper, SparqlVariable>>();

        internal SelectBuilder(SparqlQueryType queryType) : base(queryType)
        {
        }

        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        public ISelectBuilder And(params SparqlVariable[] variables)
        {
            foreach (var sparqlVariable in variables)
            {
                SparqlVariable variablelocalCopy = sparqlVariable;
                _buildSelectVariables.Add(prefixes => EnsureIsResultVariable(variablelocalCopy));
            }
            return this;
        }

        internal ISelectBuilder And(Func<INamespaceMapper, SparqlVariable> buildTriplePatternFunc)
        {
            _buildSelectVariables.Add(buildTriplePatternFunc);
            return this;
        }

        /// <summary>
        /// Adds additional SELECT expression
        /// </summary>
        public IAssignmentVariableNamePart<ISelectBuilder> And<TExpression>(Func<IExpressionBuilder, PrimaryExpression<TExpression>> buildAssignmentExpression)
        {
            return new SelectAssignmentVariableNamePart<TExpression>(this, buildAssignmentExpression);
        }

        private static SparqlVariable EnsureIsResultVariable(SparqlVariable sparqlVariable)
        {
            if (sparqlVariable.IsResultVariable)
            {
                return sparqlVariable;
            }

            if (sparqlVariable.IsAggregate)
            {
                return new SparqlVariable(sparqlVariable.Name, sparqlVariable.Aggregate);
            }

            if (sparqlVariable.IsProjection)
            {
                return new SparqlVariable(sparqlVariable.Name, sparqlVariable.Projection);
            }

            return new SparqlVariable(sparqlVariable.Name, true);
        }

        /// <summary>
        /// Adds additional SELECT <paramref name="variables"/>
        /// </summary>
        public ISelectBuilder And(params string[] variables)
        {
            return And(variables.Select(var => new SparqlVariable(var, true)).ToArray());
        }

        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        public ISelectBuilder Distinct()
        {
            switch (QueryType)
            {
                case SparqlQueryType.Select:
                    QueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAll:
                    QueryType = SparqlQueryType.SelectAllDistinct;
                    break;
                case SparqlQueryType.SelectReduced:
                    QueryType = SparqlQueryType.SelectDistinct;
                    break;
                case SparqlQueryType.SelectAllReduced:
                    QueryType = SparqlQueryType.SelectAllDistinct;
                    break;
            }
            return this;
        }

        protected override SparqlQuery BuildQuery(SparqlQuery query)
        {
            BuildReturnVariables(query);

            return base.BuildQuery(query);
        }

        private void BuildReturnVariables(SparqlQuery query)
        {
            var variables = _buildSelectVariables.Select(buildSelectVariable => buildSelectVariable(Prefixes));

            foreach (var selectVariable in variables)
            {
                query.AddVariable(selectVariable);
            }
        }
    }
}