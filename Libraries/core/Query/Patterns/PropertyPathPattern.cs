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
        private String _lengthVar = String.Empty;

        /// <summary>
        /// Creates a new Property Path Pattern
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="path">Property Path</param>
        /// <param name="obj">Object</param>
        /// <param name="lengthVar">Variable to bind the path length to</param>
        public PropertyPathPattern(PatternItem subj, ISparqlPath path, PatternItem obj, String lengthVar)
        {
            this._subj = subj;
            this._path = path;
            this._obj = obj;
            if (lengthVar.StartsWith("?") || lengthVar.StartsWith("$")) lengthVar = lengthVar.Substring(1);
            this._lengthVar = lengthVar;

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
        /// Creates a new Property Path Pattern
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="path">Property Path</param>
        /// <param name="obj">Object</param>
        public PropertyPathPattern(PatternItem subj, ISparqlPath path, PatternItem obj)
            : this(subj, path, obj, String.Empty) { }

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
            //if (this._path.IsSimple)
            //{
                try
                {
                    //Try and generate an Algebra expression
                    PathTransformContext transformContext = new PathTransformContext(this._subj, this._obj);
                    //this._path.ToAlgebra(transformContext);
                    //ISparqlAlgebra algebra = transformContext.ToAlgebra();
                    ISparqlAlgebra algebra = this._path.ToAlgebraOperator(transformContext);

                    //Now we can evaluate the resulting algebra
                    BaseMultiset initialInput = context.InputMultiset;
                    BaseMultiset result = algebra.Evaluate(context);

                    ////Bind Length Variable if required
                    //if (!this._lengthVar.Equals(String.Empty))
                    //{
                    //    result.AddVariable(this._lengthVar);
                    //    foreach (Set s in result.Sets)
                    //    {
                    //        s.Add(this._lengthVar, new LiteralNode(null, ((IBgp)algebra).PatternCount.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
                    //    }
                    //}

                    context.OutputMultiset = initialInput.Join(result);

                    //If we reach here we've successfully evaluated the simple pattern and can return
                    return;
                }
                catch (RdfQueryException)
                {
                    //Path was non-simple or couldn't be transformed
                    throw;
                }
            //}
            //else
            //{
            //    //It is a complex path so we have to evaluate it properly
            //    PathEvaluationContext evalContext = new PathEvaluationContext(context, this._subj, this._obj);
            //    try
            //    {
            //        this._path.Evaluate(evalContext);
            //    }
            //    catch (RdfQueryPathFoundException)
            //    {
            //        //Ignore this error as this is just an optimisation
            //    }

            //    //Now we take the results and generate an output multiset from them
            //    bool subjVar = (this._subj.VariableName != null);
            //    bool objVar = (this._obj.VariableName != null);
            //    bool lengthVar = (!this._lengthVar.Equals(String.Empty));
            //    if (subjVar || objVar)
            //    {
            //        //We only need to do this if the path had variables as the subject and/or object
            //        foreach (PotentialPath p in evalContext.CompletePaths)
            //        {
            //            if (p.IsComplete && !p.IsDeadEnd)
            //            {
            //                Set s = new Set();
            //                if (subjVar)
            //                {
            //                    s.Add(this._subj.VariableName, p.Start);
            //                }
            //                if (objVar)
            //                {
            //                    s.Add(this._obj.VariableName, p.Current);
            //                }
            //                if (lengthVar)
            //                {
            //                    s.Add(this._lengthVar, new LiteralNode(null, p.Length.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger)));
            //                }

            //                context.OutputMultiset.Add(s);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //If the path didn't have variables we'll return either an Identity/Null Multiset
            //        //depending on whether any paths were found
            //        if (evalContext.CompletePaths.Any(p => p.IsComplete && !p.IsDeadEnd))
            //        {
            //            context.OutputMultiset = new IdentityMultiset();
            //        }
            //        else
            //        {
            //            context.OutputMultiset = new NullMultiset();
            //        }
            //    }
            //}
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
            if (!this._lengthVar.Equals(String.Empty))
            {
                output.Append("LENGTH ?");
                output.Append(this._lengthVar);
                output.Append(' ');
            }
            output.Append(this._obj.ToString());
            return output.ToString();
        }
    }
}
