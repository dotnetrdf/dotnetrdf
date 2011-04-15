/*

Copyright Robert Vesse 2009-10
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// Represents an Inverse Path
    /// </summary>
    public class InversePath : BaseUnaryPath
    {
        /// <summary>
        /// Creates a new Inverse Path
        /// </summary>
        /// <param name="path">Path</param>
        public InversePath(ISparqlPath path)
            : base(path) { }

        /// <summary>
        /// Gets whether the path is simple
        /// </summary>
        public override bool IsSimple
        {
            get 
            {
                return this._path.IsSimple;
            }
        }

        /// <summary>
        /// Evaluates the Path in the given Context
        /// </summary>
        /// <param name="context">Path Evaluation Context</param>
        public override void Evaluate(PathEvaluationContext context)
        {
            context.IsReversed = !context.IsReversed;
            this._path.Evaluate(context);
            context.IsReversed = !context.IsReversed;
        }

        /// <summary>
        /// Gets the String representation of the Path
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "^ " + this._path.ToString();
        }

        /// <summary>
        /// Generates the Path transform to an Algebra expression
        /// </summary>
        /// <param name="context">Transform Context</param>
        public override void ToAlgebra(PathTransformContext context)
        {
            if (this.IsSimple)
            {
                //Swap the Subject and Object over
                PatternItem tempObj = context.Object;
                PatternItem tempSubj = context.Subject;
                PatternItem tempEnd = context.End;
                context.Object = tempSubj;
                context.Subject = tempObj;
                context.End = tempSubj;

                //Then transform the path
                this._path.ToAlgebra(context);

                //Then swap the Subject and Object back
                context.Subject = tempSubj;
                context.Object = tempObj;
                context.End = tempEnd;
            }
            else
            {
                throw new RdfQueryException("Cannot transform a non-simple Path to an Algebra expression");
            }
        }

        public override ISparqlAlgebra ToAlgebraOperator(PathTransformContext context)
        {
            this.ToAlgebra(context);
            return context.ToAlgebra();
        }
    }
}
