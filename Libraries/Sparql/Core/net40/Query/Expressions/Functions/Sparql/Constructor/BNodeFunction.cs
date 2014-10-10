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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor
{
    /// <summary>
    /// Class representing the SPARQL BNODE() function
    /// </summary>
    public class BNodeFunction 
        : BaseNAryExpression
    {
        private IExpressionContext _currentContext = null;
        private IBlankNodeGenerator _generator;

        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        public BNodeFunction()
            : base(Enumerable.Empty<IExpression>()) { }

        /// <summary>
        /// Creates a new BNode Function
        /// </summary>
        /// <param name="expr">Argument Expression</param>
        public BNodeFunction(IExpression expr)
            : base(expr.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> arguments = args.ToList();
            return arguments.Count > 0 ? new BNodeFunction(arguments[0]) : new BNodeFunction();
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in a given Context for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(ISolution solution, IExpressionContext context)
        {
            // Clear the cache if the expression context has changed
            if (!ReferenceEquals(context, this._currentContext))
            {
                this._currentContext = context;
                this._generator = null;
            }

            // Get the generator
            // We use a random derived generator which is seeded from the reference hash of the current context
            // This ensures that BNode generation is aligned across all instances of a function for a given context and
            // that functions in different contexts are unlikely to collide hash codes even when given the same inputs
            if (this._generator == null)
            {
                this._generator = new RandomDerivedBlankNodeGenerator(RuntimeHelpers.GetHashCode(this._currentContext));
            }

            if (this.Arguments.Count == 0)
            {
                //If no argument then always a fresh BNode
                return new BlankNode(Guid.NewGuid());
            }

            // Otherwise a single argument whose value is used to derive a GUID for the Blank Node ID
            IValuedNode n = this.Arguments[0].Evaluate(solution, context);
            if (n == null) throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to null");
            if (n.NodeType != NodeType.Literal) throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a non-literal node");
            if (n.HasDataType) throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a typed literal node");
            if (n.HasLanguage) throw new RdfQueryException("Cannot create a Blank Node when the argument Expression evaluates to a lanuage specified literal");

            return new BlankNode(this._generator.GetGuid(n.Value));
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordBNode;
            }
        }
    }
}
