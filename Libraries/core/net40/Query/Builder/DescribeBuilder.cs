using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF.Query.Builder
{
    sealed class DescribeBuilder : QueryTypeSpecificBuilderBase, IDescribeBuilder
    {
        readonly List<IToken> _describeVariables = new List<IToken>();

        /// <summary>
        /// Adds additional <paramref name="variables"/> to DESCRIBE
        /// </summary>
        public IDescribeBuilder And(params string[] variables)
        {
            _describeVariables.AddRange(from variableName in variables select new VariableToken(variableName, 0, 0, 0));
            return this;
        }

        /// <summary>
        /// Adds additional <paramref name="uris"/> to DESCRIBE
        /// </summary>
        public IDescribeBuilder And(params Uri[] uris)
        {
            _describeVariables.AddRange(from uri in uris select new UriToken(string.Format("<{0}>", uri), 0, 0, 0));
            return this;
        }

        public SparqlQuery BuildQuery()
        {
            return CreateQueryBuilder().BuildQuery();
        }

        internal override SparqlQueryType SparqlQueryType
        {
            get { return SparqlQueryType.Describe; }
        }

        internal IEnumerable<IToken> DescribeVariables
        {
            get { return _describeVariables; }
        }

        protected override QueryBuilder CreateQueryBuilder()
        {
            return new QueryBuilder(this);
        }
    }
}