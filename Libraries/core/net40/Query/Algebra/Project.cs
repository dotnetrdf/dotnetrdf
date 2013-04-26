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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents the Selection step of Query Evaluation
    /// </summary>
    /// <remarks>
    /// Selection trims variables from the Multiset that are not needed in the final output.  This is separate from <see cref="Project">Project</see> so that all Variables are available for Ordering and Having clauses
    /// </remarks>
    public class Select
        : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;
        private List<SparqlVariable> _variables = new List<SparqlVariable>();
        private bool _all = false;

        /// <summary>
        /// Creates a new Select
        /// </summary>
        /// <param name="pattern">Inner Pattern</param>
        /// <param name="variables">Variables to Select</param>
        public Select(ISparqlAlgebra pattern, bool selectAll, IEnumerable<SparqlVariable> variables)
        {
            this._pattern = pattern;
            this._variables.AddRange(variables);
            this._all = selectAll;
        }

        public bool IsSelectAll
        {
            get
            {
                return this._all;
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Trims the Results of evaluating the inner pattern to remove Variables which are not Result Variables
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            try
            {
                context.InputMultiset = context.Evaluate(this._pattern);
            }
            catch (RdfQueryTimeoutException)
            {
                //If not partial results throw the error
                if (context.Query == null || !context.Query.PartialResultsOnTimeout) throw;
            }

            //Ensure expected variables are present
            HashSet<SparqlVariable> vars = new HashSet<SparqlVariable>(this._variables);
            if (context.InputMultiset is NullMultiset)
            {
                context.InputMultiset = new Multiset(vars.Select(v => v.Name));
            }
            else if (context.InputMultiset is IdentityMultiset)
            {
                context.InputMultiset = new Multiset(vars.Select(v => v.Name));
                context.InputMultiset.Add(new Set());
            }
            else if (context.InputMultiset.IsEmpty)
            {
                foreach (SparqlVariable var in vars)
                {
                    context.InputMultiset.AddVariable(var.Name);
                }
            }

            //Trim Variables that aren't being SELECTed
            if (!this._all)
            {
                foreach (String var in context.InputMultiset.Variables.ToList())
                {
                    if (!vars.Any(v => v.Name.Equals(var) && v.IsResultVariable))
                    {
                        //If not a Result variable then trim from results
                        context.InputMultiset.Trim(var);
                    }
                }
            }

            //Ensure all SELECTed variables are present
            foreach (SparqlVariable var in vars)
            {
                if (!context.InputMultiset.ContainsVariable(var.Name))
                {
                    context.InputMultiset.AddVariable(var.Name);
                }
            }

            context.OutputMultiset = context.InputMultiset;

            //Apply variable ordering if applicable
            if (!this._all && (context.Query == null || SparqlSpecsHelper.IsSelectQuery(context.Query.QueryType)))
            {
                context.OutputMultiset.SetVariableOrder(context.Query.Variables.Where(v => v.IsResultVariable).Select(v => v.Name));
            }
            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the SPARQL Variables used
        /// </summary>
        /// <remarks>
        /// If the Query supplied in the <see cref="SparqlEvaluationContext">SparqlEvaluationContext</see> is non-null then it's Variables are used rather than these
        /// </remarks>
        public IEnumerable<SparqlVariable> SparqlVariables
        {
            get
            {
                return this._variables;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Select(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            foreach (SparqlVariable var in this._variables)
            {
                q.AddVariable(var);
            }
            if (this._variables.All(v => v.IsResultVariable))
            {
                q.QueryType = SparqlQueryType.SelectAll;
            }
            else
            {
                q.QueryType = SparqlQueryType.Select;
            }
            return q;
        }

        /// <summary>
        /// Throws an error as a Select() cannot be converted back to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since a Select() cannot be converted back to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Select() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Select(optimiser.Optimise(this._pattern), this._all, this._variables);
        }
    }

    /// <summary>
    /// Represents the Ask step of Query Evaluation
    /// </summary>
    /// <remarks>
    /// Used only for ASK queries.  Turns the final Multiset into either an <see cref="IdentityMultiset">IdentityMultiset</see> if the ASK succeeds or a <see cref="NullMultiset">NullMultiset</see> if the ASK fails
    /// </remarks>
    public class Ask 
        : IUnaryOperator
    {
        private ISparqlAlgebra _pattern;

        /// <summary>
        /// Creates a new ASK
        /// </summary>
        /// <param name="pattern">Inner Pattern</param>
        public Ask(ISparqlAlgebra pattern)
        {
            this._pattern = pattern;
        }

        /// <summary>
        /// Evaluates the ASK by turning the Results of evaluating the Inner Pattern to either an Identity/Null Multiset depending on whether there were any Results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            try
            {
                context.InputMultiset = context.Evaluate(this._pattern);
            }
            catch (RdfQueryTimeoutException)
            {
                if (!context.Query.PartialResultsOnTimeout) throw;
            }

            if (context.InputMultiset is IdentityMultiset || context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
            }
            else
            {
                if (context.InputMultiset.IsEmpty)
                {
                    context.OutputMultiset = new NullMultiset();
                }
                else
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
            }

            return context.OutputMultiset;
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the String representation of the Ask
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Ask(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            q.QueryType = SparqlQueryType.Ask;
            return q;
        }

        /// <summary>
        /// Throws an exception since an Ask() cannot be converted to a Graph Pattern
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since an Ask() cannot be converted to a Graph Pattern</exception>
        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("An Ask() cannot be converted to a GraphPattern");
        }

        /// <summary>
        /// Transforms the Inner Algebra using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimiser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Ask(optimiser.Optimise(this._pattern));
        }
    }
}
