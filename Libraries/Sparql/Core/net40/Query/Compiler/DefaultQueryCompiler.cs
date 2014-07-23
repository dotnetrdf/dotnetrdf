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

            // Always start from table unit
            this._algebras.Push(Table.CreateUnit());

            // Firstly visit the where clause
            if (query.WhereClause != null)
            {
                query.WhereClause.Accept(this);
            }

            // Then visit the modifiers in the appropriate order

            // GROUP BY

            // Project Expressions
            if (query.Projections != null && query.Projections.Any(kvp => kvp.Value != null))
            {
                // TODO Must handle replacing aggregates with their temporary variables
                this._algebras.Push(Extend.Create(this._algebras.Pop(), query.Projections.Where(kvp => kvp.Value != null)));
            }

            // HAVING
            if (query.HavingConditions != null && query.HavingConditions.Any())
            {
                this._algebras.Push(Filter.Create(this._algebras.Pop(), query.HavingConditions));
            }

            // VALUES
            if (query.ValuesClause != null)
            {
                this._algebras.Push(Join.Create(this._algebras.Pop(), new Table(this.CompileInlineData(query.ValuesClause))));
            }

            // ORDER BY
            if (query.SortConditions != null && query.SortConditions.Any())
            {
                this._algebras.Push(new OrderBy(this._algebras.Pop(), query.SortConditions));
            }

            // PROJECT
            if (query.Projections != null && query.Projections.Any(kvp => kvp.Value == null))
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
            this._algebras.Push(Extend.Create(this._algebras.Pop(), bind.Assignments));
        }

        public void Visit(DataElement data)
        {
            this._algebras.Push(Join.Create(this._algebras.Pop(), new Table(this.CompileInlineData(data.Data))));
        }

        public void Visit(FilterElement filter)
        {
            this._algebras.Push(Filter.Create(this._algebras.Pop(), filter.Expressions));
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
// ReSharper disable RedundantCast
            IAlgebra bgp = tripleBlock.Triples.Count > 0 ? (IAlgebra)new Bgp(tripleBlock.Triples) : (IAlgebra)Table.CreateUnit();
// ReSharper restore RedundantCast
            this._algebras.Push(Join.Create(this._algebras.Pop(), bgp));
        }

        public void Visit(UnionElement union)
        {
            // Firstly convert all the elements
            foreach (IElement element in union.Elements)
            {
                element.Accept(this);
            }

            // Then union together the results
            IAlgebra current = this._algebras.Pop();
            for (int i = 1; i < union.Elements.Count; i++)
            {
                current = new Union(this._algebras.Pop(), current);
            }
            this._algebras.Push(Join.Create(this._algebras.Pop(), current));
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