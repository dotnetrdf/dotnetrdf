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
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing property patterns in SPARQL Queries
    /// </summary>
    public class PropertyPathPattern : BaseTriplePattern
    {
        private PatternItem _subj, _obj;
        private ISparqlPath _path;

        /// <summary>
        /// Creates a new Property Path Pattern
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="path">Property Path</param>
        /// <param name="obj">Object</param>
        public PropertyPathPattern(PatternItem subj, ISparqlPath path, PatternItem obj)
        {
            this._subj = subj;
            this._path = path;
            this._obj = obj;

            //Build our list of Variables
            if (this._subj.VariableName != null)
            {
                this._vars.Add(this._subj.VariableName);
            }
            if (this._obj.VariableName != null)
            {
                if (!this._vars.Contains(this._obj.VariableName)) this._vars.Add(this._obj.VariableName);
            }
            this._vars.Sort();
        }

        /// <summary>
        /// Gets the Subject of the Property Path
        /// </summary>
        public PatternItem Subject
        {
            get
            {
                return this._subj;
            }
        }

        /// <summary>
        /// Gets the Property Path
        /// </summary>
        public ISparqlPath Path
        {
            get
            {
                return this._path;
            }
        }

        /// <summary>
        /// Gets the Object of the Property Path
        /// </summary>
        public PatternItem Object
        {
            get
            {
                return this._obj;
            }
        }

        /// <summary>
        /// Evaluates a property path pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            //Try and generate an Algebra expression
            //Make sure we don't generate clashing temporary variable IDs over the life of the
            //Evaluation
            PathTransformContext transformContext = new PathTransformContext(this._subj, this._obj);
            if (context["PathTransformID"] != null)
            {
                transformContext.NextID = (int)context["PathTransformID"];
            }
            ISparqlAlgebra algebra = this._path.ToAlgebra(transformContext);
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

                //If we reach here we've successfully evaluated the simple pattern and can return
                return;
            }
            finally
            {
                context.TrimTemporaryVariables = trimMode;
                //context.RigorousEvaluation = rigMode;
            }
        }

        /// <summary>
        /// Gets whether the Pattern accepts all Triple Patterns
        /// </summary>
        public override bool IsAcceptAll
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Gets the String representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(this._subj.ToString());
            output.Append(' ');
            output.Append(this._path.ToString());
            output.Append(' ');
            output.Append(this._obj.ToString());
            return output.ToString();
        }
    }
}
