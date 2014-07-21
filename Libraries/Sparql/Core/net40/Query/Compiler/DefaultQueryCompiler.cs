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

            // TODO visit modifiers


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
            this._algebras.Push(new Table(((IEnumerable<IResultRow>) data.Data).Select(r =>
            {
                Solution s = new Solution();
                foreach (String var in r.Variables)
                {
                    INode n;
                    if (r.TryGetBoundValue(var, out n)) s.Add(var, n);
                }
                return s;
            })));
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
            throw new NotImplementedException();
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
    }
}