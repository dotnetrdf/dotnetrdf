/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents an Extend operation which is the formal algebraic form of the BIND operation
    /// </summary>
    public class Extend
        : IUnaryOperator
    {
        private ISparqlAlgebra _inner;
        private String _var;
        private ISparqlExpression _expr;

        /// <summary>
        /// Creates a new Extend operator
        /// </summary>
        /// <param name="pattern">Pattern</param>
        /// <param name="expr">Expression</param>
        /// <param name="var">Variable to bind to</param>
        public Extend(ISparqlAlgebra pattern, ISparqlExpression expr, String var)
        {
            this._inner = pattern;
            this._expr = expr;
            this._var = var;

            if (this._inner.Variables.Contains(this._var))
            {
                throw new RdfQueryException("Cannot create an Extend() operator which extends the results of the inner algebra with a variable that is already used in the inner algebra");
            }
        }

        /// <summary>
        /// Gets the Variable Name to be bound
        /// </summary>
        public String VariableName
        {
            get
            {
                return this._var;
            }
        }

        /// <summary>
        /// Gets the Assignment Expression
        /// </summary>
        public ISparqlExpression AssignExpression
        {
            get
            {
                return this._expr;
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get 
            { 
                return this._inner; 
            }
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Extend(optimiser.Optimise(this._inner), this._expr, this._var);
        }

        /// <summary>
        /// Evaluates the Algebra in the given context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //First evaluate the inner algebra
            BaseMultiset results = context.Evaluate(this._inner);
            context.OutputMultiset = new Multiset();

            if (results is NullMultiset)
            {
                context.OutputMultiset = results;
            }
            else if (results is IdentityMultiset)
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
                if (results.ContainsVariable(this._var))
                {
                    throw new RdfQueryException("Cannot use a BIND assigment to BIND to a variable that has previously been used in the Query");
                }

                context.OutputMultiset.AddVariable(this._var);
                foreach (int id in results.SetIDs.ToList())
                {
                    ISet s = results[id].Copy();
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

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the variables used in the algebra
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return this._inner.Variables.Concat(this._var.AsEnumerable()); 
            }
        }

        /// <summary>
        /// Converts the Algebra to a Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            return q;
        }

        /// <summary>
        /// Converts the Algebra to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern gp = this._inner.ToGraphPattern();
            if (gp.HasModifier)
            {
                GraphPattern p = new GraphPattern();
                p.AddGraphPattern(gp);
                p.AddAssignment(new BindPattern(this._var, this._expr));
                return p;
            }
            else
            {
                gp.AddAssignment(new BindPattern(this._var, this._expr));
                return gp;
            }
        }

        /// <summary>
        /// Gets the String representation of the Extend
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Extend(" + this._inner.ToSafeString() + ")";
        }
    }
}
