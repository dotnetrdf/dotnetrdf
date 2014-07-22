using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Elements;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query.Compiler
{
    public class DefaultQueryCompiler
        : IQueryCompiler, IElementVisitor
    {
        private readonly Stack<IAlgebra> _algebras = new Stack<IAlgebra>();

        public virtual IAlgebra Compile(IQuery query)
        {
            this._algebras.Clear();

            // Firstly visit the where clause
            if (query.WhereClause != null)
            {
                query.WhereClause.Accept(this);
            }
            else
            {
                // If no WHERE clause then use Table Unit
                this._algebras.Push(Table.CreateUnit());
            }

            // Then visit the modifiers in the appropriate order

            // GROUP BY

            // Project Expressions
            if (query.Projections.Any(kvp => kvp.Value != null))
            {
                // TODO Must handle replacing aggregates with their temporary variables
                // TODO Provide an Extend.Create() method to concatenate Extend instances together
                this._algebras.Push(new Extend(this._algebras.Pop(), query.Projections.Where(kvp => kvp.Value != null)));
            }

            // HAVING
            if (query.HavingConditions.Any())
            {
                this._algebras.Push(new Filter(this._algebras.Pop(), query.HavingConditions));
            }

            // VALUES
            if (query.ValuesClause != null)
            {
                this._algebras.Push(new Join(this._algebras.Pop(), new Table(this.CompileInlineData(query.ValuesClause))));
            }

            // ORDER BY
            if (query.SortConditions.Any())
            {
                this._algebras.Push(new OrderBy(this._algebras.Pop(), query.SortConditions));
            }

            // PROJECT
            if (query.Projections.Any(kvp => kvp.Value == null))
            {
                this._algebras.Push(new Project(this._algebras.Pop(), query.Projections.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key)));
            }

            // DISTINCT/REDUCED
            switch (query.QueryType)
            {
                case QueryType.SelectAllDistinct:
                case QueryType.SelectDistinct:
                    this._algebras.Push(new Distinct(this._algebras.Pop()));
                    break;
                case QueryType.SelectAllReduced:
                case QueryType.SelectReduced:
                    this._algebras.Push(new Reduced(this._algebras.Pop()));
                    break;
            }

            // LIMIT and OFFSET
            if (query.HasLimit || query.HasOffset)
            {
                this._algebras.Push(new Slice(this._algebras.Pop(), query.Limit, query.Offset));
            }

            // Return the final algebra
            if (this._algebras.Count != 1) throw new RdfQueryException(String.Format("Query compilation failed, expected to produce 1 algebra but produced {0}", this._algebras.Count));
            return this._algebras.Pop();
        }

        public void Visit(BindElement bind)
        {
            throw new NotImplementedException();
        }

        public void Visit(DataElement data)
        {
            this._algebras.Push(new Table(this.CompileInlineData(data.Data)));
        }

        public void Visit(FilterElement filter)
        {
            // A standalone filter applies over Table Unit
            this._algebras.Push(new Filter(Table.CreateUnit(), filter.Expressions));
        }

        public void Visit(GroupElement group)
        {
            throw new NotImplementedException();
        }

        public void Visit(MinusElement minus)
        {
            throw new NotImplementedException();
        }

        public void Visit(NamedGraphElement namedGraph)
        {
            namedGraph.Element.Accept(this);
            this._algebras.Push(new NamedGraph(namedGraph.Graph, this._algebras.Pop()));
        }

        public void Visit(OptionalElement optional)
        {
            throw new NotImplementedException();
        }

        public void Visit(PathBlockElement pathBlock)
        {
            throw new NotImplementedException();
        }

        public void Visit(ServiceElement service)
        {
            service.InnerElement.Accept(this);
            this._algebras.Push(new Service(this._algebras.Pop(), service.EndpointUri, service.IsSilent));
        }

        public void Visit(SubQueryElement subQuery)
        {
            throw new NotImplementedException();
        }

        public void Visit(TripleBlockElement tripleBlock)
        {
            this._algebras.Push(new Bgp(tripleBlock.Triples));
        }

        public void Visit(UnionElement union)
        {
            union.Lhs.Accept(this);
            union.Rhs.Accept(this);

            IAlgebra rhs = this._algebras.Pop();
            IAlgebra lhs = this._algebras.Pop();

            this._algebras.Push(new Union(lhs, rhs));
        }

        protected IEnumerable<ISolution> CompileInlineData(IEnumerable<IResultRow> rows)
        {
            foreach (IResultRow r in rows)
            {
                Solution s = new Solution();
                foreach (String var in r.Variables)
                {
                    INode n;
                    if (r.TryGetBoundValue(var, out n)) s.Add(var, n);
                }
                yield return s;
            }
        }
    }
}