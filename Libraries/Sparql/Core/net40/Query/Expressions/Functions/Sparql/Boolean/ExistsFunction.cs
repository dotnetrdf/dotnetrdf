/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Represents an EXIST/NOT EXISTS clause used as a Function in an Expression
    /// </summary>
    public class ExistsFunction
        : BaseAlgebraExpression
    {
        private readonly bool _mustExist;

        /// <summary>
        /// Creates a new EXISTS/NOT EXISTS function
        /// </summary>
        /// <param name="algebra">Algebra expression</param>
        /// <param name="mustExist">Whether this is an EXIST</param>
        public ExistsFunction(IAlgebra algebra, bool mustExist)
            : base(algebra)
        {
            this._mustExist = mustExist;
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel
        /// </summary>
        public override bool CanParallelise
        {
            get { return false; }
        }

        public override bool IsDeterministic
        {
            get { return true; }
        }

        public override bool IsConstant
        {
            get { return false; }
        }

        public override IValuedNode Evaluate(ISolution set, IExpressionContext context)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get { return this._mustExist ? SparqlSpecsHelper.SparqlKeywordExists : SparqlSpecsHelper.SparqlKeywordNotExists; }
        }

        public override IExpression Copy(IAlgebra algebra)
        {
            return new ExistsFunction(algebra, this._mustExist);
        }
    }
}