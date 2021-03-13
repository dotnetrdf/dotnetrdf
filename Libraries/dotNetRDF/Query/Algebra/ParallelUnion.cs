/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a Union which will be evaluated in parallel.
    /// </summary>
    public class ParallelUnion 
        : IUnion
    {

        /// <summary>
        /// Creates a new Union.
        /// </summary>
        /// <param name="lhs">LHS Pattern.</param>
        /// <param name="rhs">RHS Pattern.</param>
        public ParallelUnion(ISparqlAlgebra lhs, ISparqlAlgebra rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        /// <summary>
        /// Evaluates the Union.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            // Create a copy of the evaluation context for the RHS
            var context2 = new SparqlEvaluationContext(context.Query, context.Data, context.Processor);
            if (!(context.InputMultiset is IdentityMultiset))
            {
                context2.InputMultiset = new Multiset();
                foreach (var s in context.InputMultiset.Sets)
                {
                    context2.InputMultiset.Add(s.Copy());
                }
            }

            var activeGraphs = context.Data.ActiveGraphUris.ToList();
            var defaultGraphs = context.Data.DefaultGraphUris.ToList();

            var lhsTask = Task.Factory.StartNew(() => ParallelEvaluate(Lhs, context, activeGraphs, defaultGraphs));
            var rhsTask = Task.Factory.StartNew(() => ParallelEvaluate(Rhs, context2, activeGraphs, defaultGraphs));
            Task[] evaluationTasks = {lhsTask, rhsTask};
            try
            {
                Task.WaitAll(evaluationTasks);
                context.CheckTimeout();
                var lhsResult = lhsTask.Result;
                var rhsResult = rhsTask.Result;
                context.OutputMultiset = lhsResult.Union(rhsResult);
                context.CheckTimeout();
                context.InputMultiset = context.OutputMultiset;
                return context.OutputMultiset;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Any())
                {
                    throw ex.InnerExceptions.First();
                }

                throw;
            }
        }

        private static BaseMultiset ParallelEvaluate(ISparqlAlgebra algebra, SparqlEvaluationContext context, IEnumerable<Uri> activeGraphs, IEnumerable<Uri> defGraphs)
        {
            bool activeGraphOk = false, defaultGraphOk = false;
            try
            {
                // Set the Active Graph
                if (activeGraphs.Any())
                {
                    context.Data.SetActiveGraph(activeGraphs);
                    activeGraphOk = true;
                }
                // Set the Default Graph
                if (defGraphs.Any())
                {
                    context.Data.SetDefaultGraph(defGraphs);
                    defaultGraphOk = true;
                }

                // Evaluate the algebra and return the result
                return context.Evaluate(algebra);
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
                        // Ignore reset exceptions
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
                        // Ignore reset exceptions
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra.
        /// </summary>
        public IEnumerable<string> Variables => (Lhs.Variables.Concat(Rhs.Variables)).Distinct();

        /// <summary>
        /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
        /// </summary>
        public IEnumerable<string> FloatingVariables
        {
            get
            {
                // Floating variables are those not fixed
                HashSet<string> fixedVars = new HashSet<string>(FixedVariables);
                return Variables.Where(v => !fixedVars.Contains(v));
            }
        }

        /// <summary>
        /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
        /// </summary>
        public IEnumerable<string> FixedVariables => Lhs.FixedVariables.Intersect(Rhs.FixedVariables);

        /// <summary>
        /// Gets the LHS of the Join.
        /// </summary>
        public ISparqlAlgebra Lhs { get; }

        /// <summary>
        /// Gets the RHS of the Join.
        /// </summary>
        public ISparqlAlgebra Rhs { get; }

        /// <summary>
        /// Gets the String representation of the Algebra.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ParallelUnion(" + Lhs + ", " + Rhs + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query.
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            var q = new SparqlQuery {RootGraphPattern = ToGraphPattern()};
            q.Optimise();
            return q;
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query.
        /// </summary>
        /// <returns></returns>
        public GraphPattern ToGraphPattern()
        {
            var p = new GraphPattern {IsUnion = true};
            p.AddGraphPattern(Lhs.ToGraphPattern());
            p.AddGraphPattern(Rhs.ToGraphPattern());
            return p;
        }

        /// <summary>
        /// Transforms both sides of the Join using the given optimiser.
        /// </summary>
        /// <param name="optimiser">optimser.</param>
        /// <returns></returns>
        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new ParallelUnion(optimiser.Optimise(Lhs), optimiser.Optimise(Rhs));
        }

        /// <summary>
        /// Transforms the LHS of the Join using the given optimiser.
        /// </summary>
        /// <param name="optimiser">optimser.</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformLhs(IAlgebraOptimiser optimiser)
        {
            return new ParallelUnion(optimiser.Optimise(Lhs), Rhs);
        }

        /// <summary>
        /// Transforms the RHS of the Join using the given optimiser.
        /// </summary>
        /// <param name="optimiser">optimser.</param>
        /// <returns></returns>
        public ISparqlAlgebra TransformRhs(IAlgebraOptimiser optimiser)
        {
            return new ParallelUnion(Lhs, optimiser.Optimise(Rhs));
        }
    }
}