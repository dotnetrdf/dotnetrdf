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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Algebra that represents the application of a Property Function
    /// </summary>
    public class PropertyFunction
        : IUnaryOperator
    {
        private readonly ISparqlPropertyFunction _function;
        private readonly ISparqlAlgebra _algebra;

        /// <summary>
        /// Creates a new Property function algebra
        /// </summary>
        /// <param name="algebra">Inner algebra</param>
        /// <param name="function">Property Function</param>
        public PropertyFunction(ISparqlAlgebra algebra, ISparqlPropertyFunction function)
        {
            _function = function;
            _algebra = algebra;
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get 
            {
                return _algebra;
            }
        }

        /// <summary>
        /// Transforms this algebra with the given optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new PropertyFunction(optimiser.Optimise(_algebra), _function);
        }

        /// <summary>
        /// Evaluates the algebra in the given context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            context.InputMultiset = context.Evaluate(_algebra);
            return _function.Evaluate(context);
        }

        /// <summary>
        /// Gets the variables used in the algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return _algebra.Variables.Concat(_function.Variables).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Floating variables includes those of the property function that aren't themselves fixed
                HashSet<String> fixedVars = new HashSet<string>(FixedVariables);
                return _algebra.FloatingVariables.Concat(_function.Variables.Where(v => !fixedVars.Contains(v))).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return _algebra.FixedVariables; } }

        /// <summary>
        /// Throws an error because property functions cannot be converted back to queries
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            throw new NotSupportedException("Property Functions cannot be converted back into queries");
        }

        /// <summary>
        /// Throws an error because property functions cannot be converted back to graph patterns
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("Property Functions cannot be converted back into Graph Patterns");
        }

        /// <summary>
        /// Gets the string representation of the algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "PropertyFunction(" + _algebra.ToString() + "," + _function.FunctionUri + ")";
        }
    }
}
