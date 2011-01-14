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
        protected List<ISparqlExpression> _expressions = new List<ISparqlExpression>();

        /// <summary>
        /// Creates a new SPARQL Set function
        /// </summary>
        /// <param name="varTerm">Variable Expression Term</param>
        /// <param name="set">Set</param>
        public SparqlSetFunction(VariableExpressionTerm varTerm, IEnumerable<ISparqlExpression> set)
        {
            this._varTerm = varTerm;
            foreach (ISparqlExpression e in set)
            {
                this._expressions.Add(e);
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

        public abstract bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID);

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

        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.SetOperator;
            }
        }

        public abstract String Functor
        {
            get;
        }

        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return this._varTerm.AsEnumerable<ISparqlExpression>().Concat(this._expressions);
            }
        }

        public abstract override string ToString();
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
        public SparqlInFunction(VariableExpressionTerm varTerm, IEnumerable<ISparqlExpression> set)
            : base(varTerm, set) { }

        /// <summary>
        /// Gets the effective boolean value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._varTerm.Value(context, bindingID);
            if (result != null)
            {
                if (this._expressions.Count == 0) return false;

                //Have to use SPARQL Value Equality here
                //If any expressions error and nothing in the set matches then an error is thrown
                bool errors = false;
                foreach (ISparqlExpression expr in this._expressions)
                {
                    try
                    {
                        INode temp = expr.Value(context, bindingID);
                        if (SparqlSpecsHelper.Equality(result, temp)) return true;
                    }
                    catch
                    {
                        errors = true;
                    }
                }

                if (errors)
                {
                    throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordIn; 
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(this._varTerm.ToString());
            output.Append(" IN (");
            for (int i = 0; i < this._expressions.Count; i++)
            {
                output.Append(this._expressions[i].ToString());
                if (i < this._expressions.Count - 1)
                {
                    output.Append(" , ");
                }
            }
            output.Append(")");
            return output.ToString();
        }
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
        public SparqlNotInFunction(VariableExpressionTerm varTerm, IEnumerable<ISparqlExpression> set)
            : base(varTerm, set) { }

        /// <summary>
        /// Gets the effective boolean value of the function as evaluated for a given Binding in the given Context
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._varTerm.Value(context, bindingID);
            if (result != null)
            {
                if (this._expressions.Count == 0) return true;

                //Have to use SPARQL Value Equality here
                //If any expressions error and nothing in the set matches then an error is thrown
                bool errors = false;
                foreach (ISparqlExpression expr in this._expressions)
                {
                    try
                    {
                        INode temp = expr.Value(context, bindingID);
                        if (SparqlSpecsHelper.Equality(result, temp)) return false;
                    }
                    catch
                    {
                        errors = true;
                    }
                }

                if (errors)
                {
                    throw new RdfQueryException("One/more expressions in a Set function failed to evaluate");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        public override string Functor
        {
            get 
            {
                return SparqlSpecsHelper.SparqlKeywordNotIn; 
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append(this._varTerm.ToString());
            output.Append(" NOT IN (");
            for (int i = 0; i < this._expressions.Count; i++)
            {
                output.Append(this._expressions[i].ToString());
                if (i < this._expressions.Count - 1)
                {
                    output.Append(" , ");
                }
            }
            output.Append(")");
            return output.ToString();
        }
    }
}
