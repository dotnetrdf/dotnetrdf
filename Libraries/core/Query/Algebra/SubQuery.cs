using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a sub-query as an Algebra operator (only used when strict algebra is generated)
    /// </summary>
    public class SubQuery : ITerminalOperator
    {
        private SparqlQuery _subquery;

        public SubQuery(SparqlQuery q)
        {
            this._subquery = q;
        }

        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Use the same algebra optimisers as the parent query (if any)
            if (context.Query != null)
            {
                this._subquery.AlgebraOptimisers = context.Query.AlgebraOptimisers;
            }

            if (context.InputMultiset is NullMultiset)
            {
                context.OutputMultiset = context.InputMultiset;
            }
            else if (context.InputMultiset.IsEmpty)
            {
                context.OutputMultiset = new NullMultiset();
            }
            else
            {
                SparqlEvaluationContext subcontext = new SparqlEvaluationContext(this._subquery, context.Data, context.Processor);
                subcontext.InputMultiset = context.InputMultiset;

                //Add any Named Graphs to the subquery
                if (context.Query != null)
                {
                    foreach (Uri u in context.Query.NamedGraphs)
                    {
                        this._subquery.AddNamedGraph(u);
                    }
                }

                ISparqlAlgebra query = this._subquery.ToAlgebra();
                try
                {
                    //Evaluate the Subquery
                    context.OutputMultiset = subcontext.Evaluate(query);

                    //If the Subquery contains a GROUP BY it may return a Group Multiset in which case we must flatten this to a Multiset
                    if (context.OutputMultiset is GroupMultiset)
                    {
                        context.OutputMultiset = new Multiset((GroupMultiset)context.OutputMultiset);
                    }

                    //Strip out any Named Graphs from the subquery
                    if (this._subquery.NamedGraphs.Any())
                    {
                        this._subquery.ClearNamedGraphs();
                    }
                }
                catch (RdfQueryException queryEx)
                {
                    throw new RdfQueryException("Query failed due to a failure in Subquery Execution:\n" + queryEx.Message, queryEx);
                }
            }

            return context.OutputMultiset;
        }

        public IEnumerable<string> Variables
        {
            get 
            { 
                return this._subquery.Variables.Where(v => v.IsResultVariable).Select(v => v.Name); 
            }
        }

        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            return q;
        }

        public VDS.RDF.Query.Patterns.GraphPattern ToGraphPattern()
        {
            GraphPattern gp = new GraphPattern();
            gp.TriplePatterns.Add(new SubQueryPattern(this._subquery));
            return gp;
        }

        public override string ToString()
        {
            return "Subquery()";
        }
    }
}
