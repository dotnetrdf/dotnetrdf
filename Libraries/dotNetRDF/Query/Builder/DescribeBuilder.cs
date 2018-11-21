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
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Query.Builder
{
    internal sealed class DescribeBuilder : QueryBuilder, IDescribeBuilder
    {
        readonly List<IToken> _describeVariables = new List<IToken>();

        internal DescribeBuilder(SparqlQueryType sparqlQueryType) : base(sparqlQueryType)
        {
        }

        /// <summary>
        /// Adds additional <paramref name="variables"/> to DESCRIBE
        /// </summary>
        public IDescribeBuilder And(params string[] variables)
        {
            foreach (var variableName in variables)
            {
                _describeVariables.Add(new VariableToken(variableName, 0, 0, 0));   
            }
            return this;
        }

        /// <summary>
        /// Adds additional <paramref name="uris"/> to DESCRIBE
        /// </summary>
        public IDescribeBuilder And(params Uri[] uris)
        {
            foreach (var uri in uris)
            {
                _describeVariables.Add(new UriToken(string.Format("<{0}>", uri), 0, 0, 0));
            }
            return this;
        }

        protected override SparqlQuery BuildQuery(SparqlQuery query)
        {
            foreach (var describeVariable in _describeVariables)
            {
                query.AddDescribeVariable(describeVariable);
            }

            return base.BuildQuery(query);
        }
    }
}