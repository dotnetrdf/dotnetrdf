/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
    public class BindPattern
        : BaseTriplePattern, IComparable<BindPattern>, IAssignmentPattern
    {
        private readonly String _var;
        private readonly ISparqlExpression _expr;

        /// <summary>
        /// Creates a new BIND Pattern
        /// </summary>
        /// <param name="var">Variable to assign to</param>
        /// <param name="expr">Expression which generates a value which will be assigned to the variable</param>
        public BindPattern(String var, ISparqlExpression expr)
        {
            this._var = var;
            this._expr = expr;
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
                    INode temp = this._expr.Evaluate(context, 0);
                    s.Add(this._var, temp);
                }
                catch
                {
                    // No assignment if there's an error
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
                        // Make a new assignment
                        INode temp = this._expr.Evaluate(context, id);
                        s.Add(this._var, temp);
                    }
                    catch
                    {
                        // No assignment if there's an error but the solution is preserved
                    }
                    context.OutputMultiset.Add(s);
                }
            }
        }

        /// <summary>
        /// Gets the Pattern Type
        /// </summary>
        public override TriplePatternType PatternType
        {
            get
            {
                return TriplePatternType.BindAssignment;
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
        /// Returns an empty enumeration as any evaluation error will result in an unbound value so we can't guarantee any variables are bound
        /// </summary>
        public override IEnumerable<string> FixedVariables
        {
            get { return Enumerable.Empty<String>(); }
        }

        /// <summary>
        /// Returns the variable being assigned to as any evaluation error will result in an unbound value so we can't guarantee it is bound
        /// </summary>
        public override IEnumerable<string> FloatingVariables
        {
            get { return this._var.AsEnumerable(); }
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
        /// Returns true as a BIND can never contain a Blank Variable
        /// </summary>
        public override bool HasNoBlankVariables
        {
            get
            {
                return true;
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
