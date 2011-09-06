/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
            //Try and generate an Algebra expression
            //Make sure we don't generate clashing temporary variable IDs over the life of the
            //Evaluation
            PathTransformContext transformContext = new PathTransformContext(this.PathStart, this.PathEnd);
            if (context["PathTransformID"] != null)
            {
                transformContext.NextID = (int)context["PathTransformID"];
            }
            ISparqlAlgebra algebra = this.Path.ToAlgebra(transformContext);
            context["PathTransformID"] = transformContext.NextID;

            //Now we can evaluate the resulting algebra
            //Note: We may need to preserve Blank Node variables across evaluations
            //which we usually don't do BUT because of the way we translate only part of the path
            //into an algebra at a time and may need to do further nested translate calls we do
            //need to do this here
            BaseMultiset initialInput = context.InputMultiset;
            bool trimMode = context.TrimTemporaryVariables;
            try
            {
                context.TrimTemporaryVariables = false;
                BaseMultiset result = context.Evaluate(algebra);//algebra.Evaluate(context);
                //Also note that we don't trim temporary variables here even if we've set the setting back
                //to enabled since a Trim will be done at the end of whatever BGP we are being evaluated in

                //Once we have our results can join then into our input
                context.OutputMultiset = initialInput.Join(result);
            }
            finally
            {
                context.TrimTemporaryVariables = trimMode;
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
            gp.AddTriplePattern(new PropertyPathPattern(this.PathStart, this.Path, this.PathEnd));
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
