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
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents an EXIST/NOT EXIST clause used as a Function in an Expression
    /// </summary>
    public class ExistsFunction : ISparqlExpression
    {
        private GraphPattern _pattern;
        private bool _mustExist;
        private BaseMultiset _result;

        /// <summary>
        /// Creates a new EXISTS/NOT EXISTS function
        /// </summary>
        /// <param name="pattern">Graph Pattern</param>
        /// <param name="mustExist">Whether this is an EXIST</param>
        public ExistsFunction(GraphPattern pattern, bool mustExist)
        {
            this._pattern = pattern;
            this._mustExist = mustExist;
        }

        /// <summary>
        /// Gets the Value of this function which is a Boolean as a Literal Node
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public INode Value(SparqlEvaluationContext context, int bindingID)
        {
            return new LiteralNode(null, this.EffectiveBooleanValue(context, bindingID).ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeBoolean));
        }

        /// <summary>
        /// Gets whether a given Binding from the Input has joinable Bindings in the results of the EXISTS/NOT EXISTS pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            if (this._result == null) this.EvaluateInternal(context);

            Set x = context.InputMultiset[bindingID];
            List<String> joinVars = x.Variables.Where(v => this._result.ContainsVariable(v)).ToList();

            IEnumerable<Set> ys = from joinVar in joinVars
                                  where x[joinVar] != null
                                  from s in this._result.Sets
                                  where x[joinVar].Equals(s[joinVar])
                                  select s;

            if (this._mustExist)
            {
                return ys.Any();
            }
            else
            {
                return (ys.Count() == 0);
            }
        }

        /// <summary>
        /// Internal method which evaluates the Graph Pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <remarks>
        /// We only ever need to evaluate the Graph Pattern once to get the Results
        /// </remarks>
        private void EvaluateInternal(SparqlEvaluationContext context)
        {
            if (this._result != null) return;

            ISparqlAlgebra existsClause = this._pattern.ToAlgebra();
            BaseMultiset initialInput = context.InputMultiset;
            this._result = existsClause.Evaluate(context);
            context.InputMultiset = initialInput;
        }

        /// <summary>
        /// Gets the Variables used in this Expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            { 
                return (from p in this._pattern.TriplePatterns
                        from v in p.Variables
                        select v).Distinct();
            }
        }

        /// <summary>
        /// Gets the String representation of the Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this._mustExist)
            {
                output.Append("EXISTS ");
            }
            else
            {
                output.Append("NOT EXISTS ");
            }
            output.Append(this._pattern.ToString());
            return output.ToString();
        }
    }
}
