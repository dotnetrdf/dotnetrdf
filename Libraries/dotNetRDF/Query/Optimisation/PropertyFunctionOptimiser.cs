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

using System.Collections.Generic;
using System.Linq;
using VDS.Common.References;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Update;

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// An algebra optimiser that looks for property functions specified by simple triple patterns in BGPs and replaces them with actual property function patterns
    /// </summary>
    public class PropertyFunctionOptimiser
        : IAlgebraOptimiser
    {
        private ThreadIsolatedReference<IEnumerable<IPropertyFunctionFactory>> _factories = new ThreadIsolatedReference<IEnumerable<IPropertyFunctionFactory>>(() => Enumerable.Empty<IPropertyFunctionFactory>());

        /// <summary>
        /// Optimises the algebra to include property functions
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <returns></returns>
        public ISparqlAlgebra Optimise(ISparqlAlgebra algebra)
        {
            if (algebra is IBgp)
            {
                IBgp current = (IBgp)algebra;
                if (current.PatternCount == 0) return current;

                List<ITriplePattern> ps = current.TriplePatterns.ToList();
                List<IPropertyFunctionPattern> propFuncs = PropertyFunctionHelper.ExtractPatterns(ps, _factories.Value);
                if (propFuncs.Count == 0) return current;

                // Remove raw Triple Patterns pertaining to extracted property functions
                foreach (IPropertyFunctionPattern propFunc in propFuncs)
                {
                    // Track where we need to insert the property function back into the BGP
                    ITriplePattern first = propFunc.OriginalPatterns.First();
                    int origLocation = ps.FindIndex(p => p.Equals(first));
                    foreach (ITriplePattern tp in propFunc.OriginalPatterns)
                    {
                        int location = ps.FindIndex(p => p.Equals(tp));
                        if (location >= 0)
                        {
                            if (location < origLocation) origLocation--;
                            ps.RemoveAt(location);
                        }
                        else
                        {
                            throw new RdfQueryException("Bad Property Function extraction");
                        }
                    }

                    // Make the insert
                    if (origLocation >= ps.Count || origLocation < 0 || ps.Count == 0)
                    {
                        ps.Add(propFunc);
                    }
                    else
                    {
                        ps.Insert(origLocation, propFunc);
                    }
                }

                // Return a new BGP
                return new Bgp(ps);
            }
            else if (algebra is ITerminalOperator)
            {
                return algebra;
            }
            else if (algebra is IAbstractJoin)
            {
                return ((IAbstractJoin)algebra).Transform(this);
            }
            else if (algebra is IUnaryOperator)
            {
                return ((IUnaryOperator)algebra).Transform(this);
            }
            else
            {
                return algebra;
            }
        }

        /// <summary>
        /// Returns that the optimiser is applicable
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlQuery q)
        {
            _factories.Value = q.PropertyFunctionFactories;
            return _factories.Value.Any() || PropertyFunctionFactory.FactoryCount > 0;
        }

        /// <summary>
        /// Returns that the optimiser is applicable
        /// </summary>
        /// <param name="cmds">Update Commands</param>
        /// <returns></returns>
        public bool IsApplicable(SparqlUpdateCommandSet cmds)
        {
            return PropertyFunctionFactory.FactoryCount > 0;
        }
    }
}
