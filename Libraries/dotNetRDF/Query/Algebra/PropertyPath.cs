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

using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents an arbitrary property path in the algebra (only used when strict algebra is generated)
    /// </summary>
    public class PropertyPath
        : BasePathOperator, ITerminalOperator
    {
        /// <summary>
        /// Creates a new Property Path operator
        /// </summary>
        /// <param name="start">Path Start</param>
        /// <param name="path">Path Expression</param>
        /// <param name="end">Path End</param>
        public PropertyPath(PatternItem start, ISparqlPath path, PatternItem end)
            : base(start, path, end) { }

        /// <summary>
        /// Evaluates the Path in the given context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public override BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            // Try and generate an Algebra expression
            // Make sure we don't generate clashing temporary variable IDs over the life of the
            // Evaluation
            PathTransformContext transformContext = new PathTransformContext(PathStart, PathEnd);
            if (context["PathTransformID"] != null)
            {
                transformContext.NextID = (int)context["PathTransformID"];
            }
            ISparqlAlgebra algebra = Path.ToAlgebra(transformContext);
            context["PathTransformID"] = transformContext.NextID;

            // Now we can evaluate the resulting algebra
            BaseMultiset initialInput = context.InputMultiset;
            bool trimMode = context.TrimTemporaryVariables;
            bool rigMode = Options.RigorousEvaluation;
            try
            {
                // Must enable rigorous evaluation or we get incorrect interactions between property and non-property path patterns
                Options.RigorousEvaluation = true;

                // Note: We may need to preserve Blank Node variables across evaluations
                // which we usually don't do BUT because of the way we translate only part of the path
                // into an algebra at a time and may need to do further nested translate calls we do
                // need to do this here
                context.TrimTemporaryVariables = false;
                BaseMultiset result = context.Evaluate(algebra);

                // Also note that we don't trim temporary variables here even if we've set the setting back
                // to enabled since a Trim will be done at the end of whatever BGP we are being evaluated in

                // Once we have our results can join then into our input
                context.OutputMultiset = initialInput.Join(result);
            }
            finally
            {
                context.TrimTemporaryVariables = trimMode;
                Options.RigorousEvaluation = rigMode;
            }

            return context.OutputMultiset;
        }

        /// <summary>
        /// Converts the algebra back into a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public override GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            gp.AddTriplePattern(new PropertyPathPattern(PathStart, Path, PathEnd));
            return gp;
        }

        /// <summary>
        /// Gets the string representation of the algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "PropertyPath()";
        }
    }
}
