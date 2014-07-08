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
        private readonly ISparqlAlgebra _inner;
        private readonly String _var;
        private readonly ISparqlExpression _expr;

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
            if (optimiser is IExpressionTransformer)
            {
                return new Extend(optimiser.Optimise(this._inner), ((IExpressionTransformer)optimiser).Transform(this._expr), this._var);
            }
            else
            {
                return new Extend(optimiser.Optimise(this._inner), this._expr, this._var);
            }
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
                    INode temp = this._expr.Evaluate(context, 0);
                    s.Add(this._var, temp);
                }
                catch
                {
                    //No assignment if there's an error
                    s.Add(this._var, null);
                }
                context.OutputMultiset.Add(s.Copy());
            }
            else
            {
                if (results.ContainsVariable(this._var))
                {
                    throw new RdfQueryException("Cannot assign to the variable ?" + this._var + "since it has previously been used in the Query");
                }

                context.InputMultiset = results;
                context.OutputMultiset.AddVariable(this._var);
#if NET40 && !SILVERLIGHT
                if (Options.UsePLinqEvaluation && this._expr.CanParallelise)
                {
                    results.SetIDs.AsParallel().ForAll(id => EvalExtend(context, results, id));
                }
                else
                {
#endif
                    foreach (int id in results.SetIDs)
                    {
                        EvalExtend(context, results, id);
                    }
#if NET40 && !SILVERLIGHT
                }
#endif
            }

            return context.OutputMultiset;
        }

        private void EvalExtend(SparqlEvaluationContext context, BaseMultiset results, int id)
        {
            ISet s = results[id].Copy();
            try
            {
                //Make a new assignment
                INode temp = this._expr.Evaluate(context, id);
                s.Add(this._var, temp);
            }
            catch
            {
                //No assignment if there's an error but the solution is preserved
            }
            context.OutputMultiset.Add(s);
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
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables { get { return this._inner.FloatingVariables.Concat(this._var.AsEnumerable()); } }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables { get { return this._inner.FixedVariables; } }

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
            return "Extend(" + this._inner.ToSafeString() + ", " + this._expr.ToString() + " AS ?" + this._var + ")";
        }
    }
}
