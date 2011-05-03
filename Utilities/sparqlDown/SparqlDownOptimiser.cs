using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Optimisation;

namespace sparqlDown
{
    class SparqlDownOptimiser : BaseAlgebraOptimiser
    {
        private IToken _endpointSpec;

        public SparqlDownOptimiser(Uri endpointUri)
        {
            this._endpointSpec = new UriToken("<" + endpointUri.ToString() + ">", 0, 0, 0);
        }

        protected override ISparqlAlgebra OptimiseInternal(ISparqlAlgebra algebra, int depth)
        {
            try
            {
                if (algebra is IAbstractJoin)
                {
                    IAbstractJoin join = (IAbstractJoin)algebra;
                    if (algebra is IMinus)
                    {
                        return new Minus(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                    }
                    else if (algebra is IExistsJoin)
                    {
                        IExistsJoin exists = (IExistsJoin)algebra;
                        return new ExistsJoin(this.OptimiseInternal(exists.Lhs, depth + 1), this.OptimiseInternal(exists.Rhs, depth + 1), exists.MustExist);
                    }
                    else
                    {
                        bool lhs = join.Lhs.IsSparql10();
                        bool rhs = join.Rhs.IsSparql10();

                        if (algebra is IJoin)
                        {
                            if (lhs)
                            {
                                if (rhs)
                                {
                                    return new Service(this._endpointSpec, join.ToGraphPattern());
                                }
                                else
                                {
                                    return new Join(new Service(this._endpointSpec, join.Lhs.ToGraphPattern()), this.OptimiseInternal(join.Rhs, depth + 1));
                                }
                            }
                            else if (rhs)
                            {
                                return new Join(this.OptimiseInternal(join.Lhs, depth + 1), new Service(this._endpointSpec, join.Rhs.ToGraphPattern()));
                            }
                            else
                            {
                                return new Join(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                            }
                        }
                        else if (algebra is ILeftJoin)
                        {
                            if (lhs)
                            {
                                if (rhs)
                                {
                                    return new Service(this._endpointSpec, join.ToGraphPattern());
                                }
                                else
                                {
                                    return new LeftJoin(new Service(this._endpointSpec, join.Lhs.ToGraphPattern()), this.OptimiseInternal(join.Rhs, depth + 1));
                                }
                            }
                            else if (rhs)
                            {
                                return new LeftJoin(this.OptimiseInternal(join.Lhs, depth + 1), new Service(this._endpointSpec, join.Rhs.ToGraphPattern()));
                            }
                            else
                            {
                                return new LeftJoin(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                            }
                        }
                        else if (algebra is IUnion)
                        {
                            if (lhs)
                            {
                                if (rhs)
                                {
                                    return new Service(this._endpointSpec, join.ToGraphPattern());
                                }
                                else
                                {
                                    return new Union(new Service(this._endpointSpec, join.Lhs.ToGraphPattern()), this.OptimiseInternal(join.Rhs, depth + 1));
                                }
                            }
                            else if (rhs)
                            {
                                return new Union(this.OptimiseInternal(join.Lhs, depth + 1), new Service(this._endpointSpec, join.Rhs.ToGraphPattern()));
                            }
                            else
                            {
                                return new Union(this.OptimiseInternal(join.Lhs, depth + 1), this.OptimiseInternal(join.Rhs, depth + 1));
                            }
                        }
                        else
                        {
                            throw new RdfParseException("Unable to transform an unknown IAbstractJoin implementation '" + join.GetType().ToString() + "' for evaluation in a mixed mode SPARQL environment");
                        }
                    }
                }
                else if (algebra is IBgp)
                {
                    if (algebra.IsSparql10())
                    {
                        return new Service(this._endpointSpec, algebra.ToGraphPattern());
                    }
                    else
                    {
                        return algebra;
                    }
                }
                else if (algebra is IUnaryOperator)
                {
                    if (algebra.IsSparql10())
                    {
                        try
                        {
                            return new Service(this._endpointSpec, algebra.ToGraphPattern());
                        }
                        catch (NotSupportedException)
                        {
                            //Ignore
                        }
                    }

                    //If fails to convert due to a NotSupportedException convert manually
                    IUnaryOperator op = (IUnaryOperator)algebra;
                    if (algebra is Bindings)
                    {
                        return new Bindings(((Bindings)algebra).BindingsPattern, this.OptimiseInternal(op.InnerAlgebra, depth + 1));
                    }
                    else if (algebra is Distinct)
                    {
                        return new Distinct(this.OptimiseInternal(op.InnerAlgebra, depth + 1));
                    }
                    else if (algebra is Filter)
                    {
                        return new Filter(this.OptimiseInternal(op.InnerAlgebra, depth + 1), ((Filter)algebra).SparqlFilter);
                    }
                    else if (algebra is Graph)
                    {
                        return new Graph(this.OptimiseInternal(op.InnerAlgebra, depth + 1), ((Graph)algebra).GraphSpecifier);
                    }
                    else if (algebra is GroupBy)
                    {
                        return new GroupBy(this.OptimiseInternal(op.InnerAlgebra, depth + 1), ((GroupBy)algebra).Grouping);
                    }
                    else if (algebra is Having)
                    {
                        return new Having(this.OptimiseInternal(op.InnerAlgebra, depth + 1), ((Having)algebra).HavingClause);
                    }
                    else if (algebra is OrderBy)
                    {
                        return new OrderBy(this.OptimiseInternal(op.InnerAlgebra, depth + 1), ((OrderBy)algebra).Ordering);
                    }
                    else if (algebra is Project)
                    {
                        return new Project(this.OptimiseInternal(op.InnerAlgebra, depth + 1), ((Project)algebra).SparqlVariables);
                    }
                    else if (algebra is Select)
                    {
                        return new Select(this.OptimiseInternal(op.InnerAlgebra, depth + 1), ((Select)algebra).SparqlVariables);
                    }
                    else if (algebra is Slice)
                    {
                        Slice s = (Slice)algebra;
                        return new Slice(this.OptimiseInternal(op.InnerAlgebra, depth + 1), s.Limit, s.Offset);
                    }
                    else
                    {
                        throw new RdfQueryException("Unable to transform an unknown IUnaryOperator implementation '" + op.GetType().ToString() + "' for evaluation in a mixed mode SPARQL environment");
                    }
                }

                return algebra;
            }
            catch (RdfQueryException)
            {
                    throw;
            }
            catch (Exception ex)
            {
                throw new RdfQueryException("Unable to transform the SPARQL Algebra for evaluation in mixed mode SPARQL environment, see Inner Exception for more detail", ex);
            }
        }

        public override bool IsApplicable(SparqlQuery q)
        {
            return true;
        }
    }
}
