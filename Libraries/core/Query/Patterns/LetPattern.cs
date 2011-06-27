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
    /// Class for representing LET Patterns in SPARQL Queries
    /// </summary>
    public class LetPattern : BaseTriplePattern, IComparable<LetPattern>, IComparable<IAssignmentPattern>, IAssignmentPattern
    {
        private String _var;
        private ISparqlExpression _expr;

        /// <summary>
        /// Creates a new LET Pattern
        /// </summary>
        /// <param name="var">Variable to assign to</param>
        /// <param name="expr">Expression which generates a value which will be assigned to the variable</param>
        public LetPattern(String var, ISparqlExpression expr)
        {
            this._var = var;
            this._expr = expr;
            this._indexType = TripleIndexType.SpecialAssignment;
            this._vars = this._var.AsEnumerable().Concat(this._expr.Variables).Distinct().ToList();
            this._vars.Sort();
        }

        /// <summary>
        /// Evaluates a LET assignment in the given Evaluation Context
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
                Set s = new Set();
                try
                {
                    INode temp = this._expr.Value(context, 0);
                    s.Add(this._var, temp);
                    context.OutputMultiset.Add(s);
                }
                catch
                {
                    //No assignment if there's an error
                }
            }
            else
            {
                foreach (int id in context.InputMultiset.SetIDs.ToList())
                {
                    ISet s = context.InputMultiset[id];
                    if (s.ContainsVariable(this._var))
                    {
                        try
                        {
                            //A value already exists so see if the two values match
                            INode current = s[this._var];
                            INode temp = this._expr.Value(context, id);
                            if (current != temp)
                            {
                                //Where the values aren't equal the solution is eliminated
                                context.InputMultiset.Remove(id);
                            }
                        }
                        catch
                        {
                            //If an error occurs the solution is eliminated
                            context.InputMultiset.Remove(id);
                        }
                    }
                    else
                    {
                        context.InputMultiset.AddVariable(this._var);
                        try
                        {
                            //Make a new assignment
                            INode temp = this._expr.Value(context, id);
                            s.Add(this._var, temp);
                        }
                        catch
                        {
                            //If an error occurs no assignment happens
                        }
                    }
                }
                context.OutputMultiset = new IdentityMultiset();
            }
        }

        /// <summary>
        /// Returns that this is not an accept all since it is a LET assignment
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
            output.Append("LET(");
            output.Append("?");
            output.Append(this._var);
            output.Append(" := ");
            output.Append(this._expr.ToString());
            output.Append(")");

            return output.ToString();
        }

        /// <summary>
        /// Compares this Let to another Let
        /// </summary>
        /// <param name="other">Let to compare to</param>
        /// <returns>Just calls the base compare method since that implements all the logic we need</returns>
        public int CompareTo(LetPattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares this Let to another Let
        /// </summary>
        /// <param name="other">Let to compare to</param>
        /// <returns>Just calls the base compare method since that implements all the logic we need</returns>
        public int CompareTo(IAssignmentPattern other)
        {
            return base.CompareTo(other);
        }
    }
}
