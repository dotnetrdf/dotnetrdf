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

using System.Collections.Generic;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Engine.Algebra
{
    /// <summary>
    /// Interface for algebra executors
    /// </summary>
    public interface IAlgebraExecutor
    {
        /// <summary>
        /// Executes the algebra using a new default context
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns>Enumerable of solutions</returns>
        IEnumerable<ISolution> Execute(IAlgebra algebra);

        /// <summary>
        /// Executes the algebra using the given context (or a new default context if it is null)
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Execution context</param>
        /// <returns>Enumberable of solutions</returns>
        IEnumerable<ISolution> Execute(IAlgebra algebra, IExecutionContext context);

        IEnumerable<ISolution> Execute(Bgp bgp, IExecutionContext context);

        IEnumerable<ISolution> Execute(Slice slice, IExecutionContext context);

        IEnumerable<ISolution> Execute(Union union, IExecutionContext context);

        IEnumerable<ISolution> Execute(NamedGraph namedGraph, IExecutionContext context);

        IEnumerable<ISolution> Execute(Filter filter, IExecutionContext context);

        IEnumerable<ISolution> Execute(Table table, IExecutionContext context);

        IEnumerable<ISolution> Execute(Join join, IExecutionContext context);

        IEnumerable<ISolution> Execute(LeftJoin join, IExecutionContext context);

        IEnumerable<ISolution> Execute(Minus minus, IExecutionContext context);

        IEnumerable<ISolution> Execute(Distinct distinct, IExecutionContext context);

        IEnumerable<ISolution> Execute(Reduced reduced, IExecutionContext context);

        IEnumerable<ISolution> Execute(Project project, IExecutionContext context);

        IEnumerable<ISolution> Execute(OrderBy orderBy, IExecutionContext context);

        IEnumerable<ISolution> Execute(Extend extend, IExecutionContext context);

        IEnumerable<ISolution> Execute(GroupBy groupBy, IExecutionContext context);

        IEnumerable<ISolution> Execute(Service service, IExecutionContext context);

        IEnumerable<ISolution> Execute(PropertyPath path, IExecutionContext context);

        IEnumerable<ISolution> Execute(TopN topN, IExecutionContext context);

        IEnumerable<ISolution> Execute(PropertyFunction propertyFunction, IExecutionContext context);
    }
}
