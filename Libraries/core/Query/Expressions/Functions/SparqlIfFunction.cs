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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Class representing the SPARQL IF ELSE function
    /// </summary>
    public class IfElseFunction : ISparqlExpression
    {
        private ISparqlExpression _condition, _ifBranch, _elseBranch;

        /// <summary>
        /// Creates a new IF ELSE function
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="ifBranch">Expression to evaluate if condition evaluates to true</param>
        /// <param name="elseBranch">Expression to evalaute if condition evaluates to false/error</param>
        public IfElseFunction(ISparqlExpression condition, ISparqlExpression ifBranch, ISparqlExpression elseBranch)
        {
            this._condition = condition;
            this._ifBranch = ifBranch;
            this._elseBranch = elseBranch;
        }

        /// <summary>
        /// Gets the value of the expression as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            bool condition;
            try
            {
                condition = this._condition.EffectiveBooleanValue(context, bindingID);
            }
            catch (RdfQueryException)
            {
                //Condition expression errored so we'll go down the ELSE branch
                condition = false;
            }

            //Condition evaluated without error so we go to the appropriate branch of the IF ELSE
            //depending on whether it evaluated to true or false
            if (condition)
            {
                return this._ifBranch.Value(context, bindingID);
            }
            else
            {
                return this._elseBranch.Value(context, bindingID);
            }
        }

        /// <summary>
        /// Gets the effective boolean value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            throw new RdfQueryException("A SPARQL IF ELSE does not have an effective boolean value");
        }

        /// <summary>
        /// Gets the enumeration of variables used in the expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return this._condition.Variables.Concat(this._ifBranch.Variables).Concat(this._elseBranch.Variables);
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("IF ");
            output.Append(this._condition.ToString());
            output.Append(' ');
            if (this._ifBranch is IfElseFunction)
            {
                output.Append('(');
                output.Append(this._ifBranch.ToString());
                output.Append(')');
            }
            else
            {
                output.Append(this._ifBranch.ToString());
            }
            if (this._elseBranch != null)
            {
                output.Append(" ELSE ");
                if (this._elseBranch is IfElseFunction)
                {
                    output.Append('(');
                    output.Append(this._elseBranch.ToString());
                    output.Append(')');
                }
                else
                {
                    output.Append(this._elseBranch.ToString());
                }
            }
            return output.ToString();
        }
    }
}
