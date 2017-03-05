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
using System.Threading;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Union which will be evaluated in parallel
    /// </summary>
    public class ParallelUnion 
        : IUnion
    {
        private ISparqlAlgebra _lhs, _rhs;

        /// <summary>
        /// Creates a new Union
        /// </summary>
        /// <param name="lhs">LHS Pattern</param>
        /// <param name="rhs">RHS Pattern</param>
        public ParallelUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            this._lhs = lhs;
            this._rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Union
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Create a copy of the evaluation context for the RHS
            SparqlEvaluationContext context2 = new SparqlEvaluationContext(context.Query, context.Data, context.Processor);
            if (!(context.InputMultiset is IdentityMultiset))
            {
                context2.InputMultiset = new Multiset();
                foreach (ISet s in context.InputMultiset.Sets)
                {
                    context2.InputMultiset.Add(s.Copy());
                }
            }

            List<Uri> activeGraphs = context.Data.ActiveGraphUris.ToList();
            List<Uri> defaultGraphs = context.Data.DefaultGraphUris.ToList();

            ParallelEvaluateDelegate d = new ParallelEvaluateDelegate(this.ParallelEvaluate);
            IAsyncResult lhs = d.BeginInvoke(this._lhs, context, activeGraphs, defaultGraphs, null, null);
            IAsyncResult rhs = d.BeginInvoke(this._rhs, context2, activeGraphs, defaultGraphs, null, null);

            WaitHandle.WaitAll(new WaitHandle[] { lhs.AsyncWaitHandle, rhs.AsyncWaitHandle });

            bool rhsOk = false;
            try 
            {
                BaseMultiset lhsResult = d.EndInvoke(lhs);
                rhsOk = true;
                BaseMultiset rhsResult = d.EndInvoke(rhs);
                context.CheckTimeout();

                context.OutputMultiset = lhsResult.Union(rhsResult);
                context.CheckTimeout();

                context.InputMultiset = context.OutputMultiset;
                return context.OutputMultiset;
            }
            catch 
            {
                if (!rhsOk)
                {
                    //Clean up the RHS evaluation call if the LHS has errored
                    try
                    {
                        d.EndInvoke(rhs);
                    }
                    catch
                    {
                        //Ignore this error as we're already going to throw the other error
                    }
                }
                throw;
            }
        }

        private delegate BaseMultiset ParallelEvaluateDelegate(ISparqlAlgebra algebra, SparqlEvaluationContext context, IEnumerable<Uri> activeGraphs, IEnumerable<Uri> defGraphs);

        private BaseMultiset ParallelEvaluate(ISparqlAlgebra algebra, SparqlEvaluationContext context, IEnumerable<Uri> activeGraphs, IEnumerable<Uri> defGraphs)
        {
            bool activeGraphOk = false, defaultGraphOk = false;
            try
            {
                //Set the Active Graph
                if (activeGraphs.Any())
                {
                    context.Data.SetActiveGraph(activeGraphs);
                    activeGraphOk = true;
                }
                //Set the Default Graph
                if (defGraphs.Any())
                {
                    context.Data.SetDefaultGraph(defGraphs);
                    defaultGraphOk = true;
                }

                //Evaluate the algebra and return the result
                return context.Evaluate(algebra);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (defaultGraphOk)
                {
                    try
                    {
                        context.Data.ResetDefaultGraph();
                    }
                    catch
                    {
                        //Ignore reset exceptions
                    }
                }
                if (activeGraphOk)
                {
                    try
                    {
                        context.Data.ResetActiveGraph();
                    }
                    catch
                    {
                        //Ignore reset exceptions
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Floating variables are those not fixed
                HashSet<String> fixedVars = new HashSet<string>(this.FixedVariables);
                return this.Variables.Where(v => !fixedVars.Contains(v));
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables
        {
            get
            {
                // Fixed variables are those fixed on both sides
                return this._lhs.FixedVariables.Intersect(this._rhs.FixedVariables);
            }
        }

        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return this._lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return this._rhs;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ParallelUnion(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = new GraphPattern();
            p.IsUnion = true;
            p.AddGraphPattern(this._lhs.ToGraphPattern());
            p.AddGraphPattern(this._rhs.ToGraphPattern());
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new ParallelUnion(optimiser.Optimise(this._lhs), optimiser.Optimise(this._rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new ParallelUnion(optimiser.Optimise(this._lhs), this._rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new ParallelUnion(this._lhs, optimiser.Optimise(this._rhs));
        }
    }

    /// <summary>
    /// Represents a Join which will be evaluated in parallel
    /// </summary>
    public class ParallelJoin : IJoin
    {
        private readonly ISparqlAlgebra _lhs, _rhs;
        private BaseMultiset _rhsResult;
        private Exception _rhsError;
        private readonly ParallelEvaluateDelegate _d;

        /// <summary>
        /// Creates a new Join
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <param name="rhs">Right Hand Side</param>
        public ParallelJoin(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            if (!lhs.Variables.IsDisjoint(rhs.Variables)) throw new RdfQueryException("Cannot create a ParallelJoin between two algebra operators which are not distinct");
            this._lhs = lhs;
            this._rhs = rhs;
            this._d = new ParallelEvaluateDelegate(this.ParallelEvaluate);
        }

        /// <summary>
        /// Evalutes a Join
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Create a copy of the evaluation context for the RHS
            SparqlEvaluationContext context2 = new SparqlEvaluationContext(context.Query, context.Data, context.Processor);
            if (!(context.InputMultiset is IdentityMultiset))
            {
                context2.InputMultiset = new Multiset();
                foreach (ISet s in context.InputMultiset.Sets)
                {
                    context2.InputMultiset.Add(s.Copy());
                }
            }

            List<Uri> activeGraphs = context.Data.ActiveGraphUris.ToList();
            List<Uri> defaultGraphs = context.Data.DefaultGraphUris.ToList();

            //Start both executing asynchronously
            IAsyncResult lhs = this._d.BeginInvoke(this._lhs, context, activeGraphs, defaultGraphs, null, null);
            IAsyncResult rhs = this._d.BeginInvoke(this._rhs, context2, activeGraphs, defaultGraphs, new AsyncCallback(this.RhsCallback), null);

            //Wait on the LHS
            if (context.RemainingTimeout > 0)
            {
                lhs.AsyncWaitHandle.WaitOne(new TimeSpan(0, 0, 0, 0, (int)context.RemainingTimeout));
            }
            else
            {
                lhs.AsyncWaitHandle.WaitOne();
            }
            context.CheckTimeout();

            //Get the LHS result
            BaseMultiset lhsResult;
            try
            {
                lhsResult = this._d.EndInvoke(lhs);
            }
            catch
            {
                throw;
            }

            //If LHS came back as null/empty no need to wait for RHS to complete
            if (lhsResult is NullMultiset)
            {
                context.OutputMultiset = lhsResult;
            }
            else if (lhsResult.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                //Wait for RHS to complete
                if (!rhs.IsCompleted)
                {
                    if (context.RemainingTimeout > 0)
                    {
                        rhs.AsyncWaitHandle.WaitOne(new TimeSpan(0, 0, 0, 0, (int)context.RemainingTimeout));
                    }
                    else
                    {
                        rhs.AsyncWaitHandle.WaitOne();
                    }
                    context.CheckTimeout();
                }

                if (this._rhsResult == null)
                {
                    if (this._rhsError != null) throw this._rhsError;
#if !PORTABLE // No Thread.Sleep() in PCL
                    Thread.Sleep(10);
#endif
                }
                if (this._rhsResult == null) throw new RdfQueryException("Unknown error in parallel join evaluation, RHS is reported completed without errors but no result is available");

                //Compute the product of the two sides
                context.OutputMultiset = lhsResult.Product(this._rhsResult);
            }
            return context.OutputMultiset;
        }

        private delegate BaseMultiset ParallelEvaluateDelegate(ISparqlAlgebra algebra, SparqlEvaluationContext context, IEnumerable<Uri> activeGraphs, IEnumerable<Uri> defGraphs);

        private BaseMultiset ParallelEvaluate(ISparqlAlgebra algebra, SparqlEvaluationContext context, IEnumerable<Uri> activeGraphs, IEnumerable<Uri> defGraphs)
        {
            bool activeGraphOk = false, defaultGraphOk = false;
            try
            {
                //Set the Active Graph
                if (activeGraphs.Any())
                {
                    context.Data.SetActiveGraph(activeGraphs);
                    activeGraphOk = true;
                }
                //Set the Default Graph
                if (defGraphs.Any())
                {
                    context.Data.SetDefaultGraph(defGraphs);
                    defaultGraphOk = true;
                }

                //Evaluate the algebra and return the result
                return context.Evaluate(algebra);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (defaultGraphOk)
                {
                    try
                    {
                        context.Data.ResetDefaultGraph();
                    }
                    catch
                    {
                        //Ignore reset exceptions
                    }
                }
                if (activeGraphOk)
                {
                    try
                    {
                        context.Data.ResetActiveGraph();
                    }
                    catch
                    {
                        //Ignore reset exceptions
                    }
                }
            }
        }

        private void RhsCallback(IAsyncResult result)
        {
            try
            {
                this._rhsResult = this._d.EndInvoke(result);
            }
            catch (Exception ex)
            {
                this._rhsError = ex;
                this._rhsResult = null;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (this._lhs.Variables.Concat(this._rhs.Variables)).Distinct();
            }
        }
        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FloatingVariables
        {
            get
            {
                // Floating variables are those floating on either side which are not fixed
                IEnumerable<String> floating = this._lhs.FloatingVariables.Concat(this._rhs.FloatingVariables).Distinct();
                HashSet<String> fixedVars = new HashSet<string>(this.FixedVariables);
                return floating.Where(v => !fixedVars.Contains(v));
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value
        /// </summary>
        public IEnumerable<String> FixedVariables
        {
            get
            {
                // Fixed variables are those fixed on either side
                return this._lhs.FixedVariables.Concat(this._rhs.FixedVariables).Distinct();
            }
        }

        /// <summary>
        /// Gets the LHS of the Join
        /// </summary>
        public ISparqlAlgebra Lhs
        {
            get
            {
                return this._lhs;
            }
        }

        /// <summary>
        /// Gets the RHS of the Join
        /// </summary>
        public ISparqlAlgebra Rhs
        {
            get
            {
                return this._rhs;
            }
        }

        /// <summary>
        /// Gets the String representation of the Join
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ParallelJoin(" + this._lhs.ToString() + ", " + this._rhs.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            GraphPattern p = this._lhs.ToGraphPattern();
            p.AddGraphPattern(this._rhs.ToGraphPattern());
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new ParallelJoin(optimiser.Optimise(this._lhs), optimiser.Optimise(this._rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new ParallelJoin(optimiser.Optimise(this._lhs), this._rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given Optimiser
        /// </summary>
        /// <param name="optimiser">Optimser</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new ParallelJoin(this._lhs, optimiser.Optimise(this._rhs));
        }
    }
}
