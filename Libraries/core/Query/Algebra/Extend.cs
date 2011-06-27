using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    public class Extend : IUnaryOperator
    {
        private ISparqlAlgebra _inner;
        private String _var;
        private ISparqlExpression _expr;

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

        public String VariableName
        {
            get
            {
                return this._var;
            }
        }

        public ISparqlExpression AssignExpression
        {
            get
            {
                return this._expr;
            }
        }

        public ISparqlAlgebra InnerAlgebra
        {
            get 
            { 
                return this._inner; 
            }
        }

        public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
        {
            return new Extend(optimiser.Optimise(this._inner), this._expr, this._var);
        }

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

        public IEnumerable<string> Variables
        {
            get 
            {
                return this._inner.Variables.Concat(this._var.AsEnumerable()); 
            }
        }

        public SparqlQuery ToQuery()
        {
            SparqlQuery q = new SparqlQuery();
            q.RootGraphPattern = this.ToGraphPattern();
            return q;
        }

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

        public override string ToString()
        {
            return "Extend(" + this._inner.ToSafeString() + ")";
        }
    }
}
