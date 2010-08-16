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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Abstract base class for SPARQL Functions which operate on Sets
    /// </summary>
    public abstract class SparqlSetFunction : ISparqlExpression
    {
        /// <summary>
        /// Variable Expression Term that the Set function applies to
        /// </summary>
        protected VariableExpressionTerm _varTerm;
        /// <summary>
        /// Set that is used in the function
        /// </summary>
        protected HashSet<INode> _set = new HashSet<INode>();

        /// <summary>
        /// Creates a new SPARQL Set function
        /// </summary>
        /// <param name="varTerm">Variable Expression Term</param>
        /// <param name="set">Set</param>
        public SparqlSetFunction(VariableExpressionTerm varTerm, IEnumerable<INode> set)
        {
            this._varTerm = varTerm;
            foreach (INode n in set)
            {
                this._set.Add(n);
            }
        }

        /// <summary>
        /// Gets the value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return new LiteralNode(null, this.EffectiveBooleanValue(context, bindingID).ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets the effective boolean value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public virtual bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._varTerm.Value(context, bindingID);
            if (result != null)
            {
                return this._set.Contains(result);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the Variable the function applies to
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return this._varTerm.Variables; 
            }
        }
    }

    /// <summary>
    /// Class representing the SPARQL IN set function
    /// </summary>
    public class SparqlInFunction : SparqlSetFunction
    {
        /// <summary>
        /// Creates a new SPARQL IN function
        /// </summary>
        /// <param name="varTerm">Variable Expression Term</param>
        /// <param name="set">Set</param>
        public SparqlInFunction(VariableExpressionTerm varTerm, IEnumerable<INode> set)
            : base(varTerm, set) { }
    }

    /// <summary>
    /// Class representing the SPARQL NOT IN set function
    /// </summary>
    public class SparqlNotInFunction : SparqlSetFunction
    {
        /// <summary>
        /// Creates a new SPARQL NOT IN function
        /// </summary>
        /// <param name="varTerm">Variable Expression Term</param>
        /// <param name="set">Set</param>
        public SparqlNotInFunction(VariableExpressionTerm varTerm, IEnumerable<INode> set)
            : base(varTerm, set) { }

        /// <summary>
        /// Gets the effective boolean value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return !base.EffectiveBooleanValue(context, bindingID);
        }
    }
}
