/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a One or More Path (e.g. rdf:type+) in the SPARQL Algebra.
    /// </summary>
    public class OneOrMorePath : BaseArbitraryLengthPathOperator
    {
        /// <summary>
        /// Creates a new One or More Path.
        /// </summary>
        /// <param name="start">Path Start.</param>
        /// <param name="end">Path End.</param>
        /// <param name="path">Path.</param>
        public OneOrMorePath(PatternItem start, PatternItem end, ISparqlPath path)
            : base(start, end, path) { }


        /// <summary>
        /// Gets the String representation of the Algebra.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "OneOrMorePath(" + PathStart + ", " + Path.ToString() + ", " + PathEnd + ")";
        }

        public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
        {
            return visitor.VisitOneOrMorePath(this);
        }

        public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
        {
            return processor.ProcessOneOrMorePath(this, context);
        }

        /// <summary>
        /// Transforms the Algebra back into a Graph Pattern.
        /// </summary>
        /// <returns></returns>
        public override GraphPattern ToGraphPattern()
        {
            var gp = new GraphPattern();
            var pp = new PropertyPathPattern(PathStart, new OneOrMore(Path), PathEnd);
            gp.AddTriplePattern(pp);
            return gp;
        }
    }
}
