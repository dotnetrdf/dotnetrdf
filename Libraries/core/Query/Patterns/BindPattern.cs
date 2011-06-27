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
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Class for representing BIND assignments in SPARQL Queries
    /// </summary>
    public class BindPattern : BaseTriplePattern, IComparable<BindPattern>, IComparable<IAssignmentPattern>, IAssignmentPattern
    {
        private String _var;
        private ISparqlExpression _expr;

        /// <summary>
        /// Creates a new BIND Pattern
        /// </summary>
        /// <param name="var">Variable to assign to</param>
        /// <param name="expr">Expression which generates a value which will be assigned to the variable</param>
        public BindPattern(String var, ISparqlExpression expr)
        {
            this._var = var;
            this._expr = expr;
            this._indexType = TripleIndexType.SpecialAssignment;
            this._vars = this._var.AsEnumerable().Concat(this._expr.Variables).Distinct().ToList();
            this._vars.Sort();
        }

        /// <summary>
        /// Evaluates a BIND assignment in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            if (context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
            }
            else if (context.InputMultiset is IdentityMultiset)
            {
                context.OutputMultiset.AddVariable(this._var);
                Set s = new Set();
                try
                {
                    INode temp = this._expr.Value(context, 0);
                    s.Add(this._var, temp);
                }
                catch
                {
                    //No assignment if there's an error
                    s.Add(this._var, null);
                }
                context.OutputMultiset.Add(s);
            }
            else
            {
                if (context.InputMultiset.ContainsVariable(this._var))
                {
                    throw new RdfQueryException("Cannot use a BIND assigment to BIND to a variable that has previously been used in the Query");
                }

                context.OutputMultiset.AddVariable(this._var);
                foreach (int id in context.InputMultiset.SetIDs.ToList())
                {
                    ISet s = context.InputMultiset[id].Copy();
                    try
                    {
                        //Make a new assignment
                        INode temp = this._expr.Value(context, id);
                        s.Add(this._var, temp);
                    }
                    catch
                    {
                        //No assignment if there's an error but the solution is preserved
                    }
                    context.OutputMultiset.Add(s);
                }
            }
        }

        /// <summary>
        /// Returns that this is not an accept all since it is a BIND assignment
        /// </summary>
        public override bool IsAcceptAll
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Gets the Expression that is used to generate values to be assigned
        /// </summary>
        public ISparqlExpression AssignExpression
        {
            get
            {
                return this._expr;
            }
        }

        /// <summary>
        /// Gets the Name of the Variable to which values will be assigned
        /// </summary>
        public String VariableName
        {
            get
            {
                return this._var;
            }
        }

        /// <summary>
        /// Gets whether the Pattern uses the Default Dataset
        /// </summary>
        public override bool UsesDefaultDataset
        {
            get
            {
                return this._expr.UsesDefaultDataset();
            }
        }

        /// <summary>
        /// Gets the string representation of the LET assignment
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("BIND(");
            output.Append(this._expr.ToString());
            output.Append(" AS ?");
            output.Append(this._var);
            output.Append(")");

            return output.ToString();
        }

        /// <summary>
        /// Compares this Bind to another Bind
        /// </summary>
        /// <param name="other">Bind to compare to</param>
        /// <returns>Just calls the base compare method since that implements all the logic we need</returns>
        public int CompareTo(BindPattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares this Bind to another Bind
        /// </summary>
        /// <param name="other">Bind to compare to</param>
        /// <returns>Just calls the base compare method since that implements all the logic we need</returns>
        public int CompareTo(IAssignmentPattern other)
        {
            return base.CompareTo(other);
        }
    }
}
