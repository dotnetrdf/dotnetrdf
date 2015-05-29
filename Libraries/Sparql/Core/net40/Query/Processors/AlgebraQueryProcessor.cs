/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Compiler;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query.Processors
{
    public class AlgebraQueryProcessor
        : IQueryProcessor
    {
        public AlgebraQueryProcessor(IQueryCompiler compiler, IAlgebraExecutor executor)
        {
            if (compiler == null) throw new ArgumentNullException("compiler");
            if (executor == null) throw new ArgumentNullException("executor");
            this.Compiler = compiler;
            this.Executor = executor;
        }

        public IQueryCompiler Compiler { get; private set; }

        public IAlgebraExecutor Executor { get; private set; }

        public virtual IEnumerable<INode> DatasetDefaultGraphs { get { return Quad.DefaultGraphNode.AsEnumerable(); } }

        public virtual IEnumerable<INode> DatasetNamedGraphs { get { return Enumerable.Empty<INode>(); } } 

        public IQueryResult Execute(IQuery query)
        {
            IAlgebra algebra = this.Compile(query);
            var context = CreateExecutionContext(query);
            IEnumerable<ISolution> solutions = this.Execute(algebra, context);

            switch (query.QueryType)
            {
                case QueryType.Ask:
                    return new QueryResult(solutions.Any());
                case QueryType.Construct:
                    // TODO Implement CONSTRUCT template processing
                    throw new NotImplementedException("CONSTRUCT queries are not yet supported");
                case QueryType.Describe:
                case QueryType.DescribeAll:
                    // TODO Implement DESCRIBE processing
                    throw new NotImplementedException("DESCRIBE queries are not yet supported");
                case QueryType.Select:
                case QueryType.SelectAll:
                case QueryType.SelectAllDistinct:
                case QueryType.SelectAllReduced:
                case QueryType.SelectDistinct:
                case QueryType.SelectReduced:
                    return new QueryResult(new StreamingTabularResults(new AlgebraExecutionResultStream(algebra, solutions)));
                default:
                    throw new RdfQueryException("Unexpected query type encountered");
            }
        }

        public void Execute(IQuery query, QueryCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an execution context for the query
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>Execution Context</returns>
        protected virtual IExecutionContext CreateExecutionContext(IQuery query)
        {
            IExecutionContext context = new QueryExecutionContext(query, this.DatasetDefaultGraphs, this.DatasetNamedGraphs);
            return context;
        }

        /// <summary>
        /// Compiles the query into algebra
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>Algebra</returns>
        protected virtual IAlgebra Compile(IQuery query)
        {
            return this.Compiler.Compile(query);
        }

        /// <summary>
        /// Executes the algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Execution Context</param>
        /// <returns>Query solutions</returns>
        protected virtual IEnumerable<ISolution> Execute(IAlgebra algebra, IExecutionContext context)
        {
            return this.Executor.Execute(algebra, context);
        }
    }
}
