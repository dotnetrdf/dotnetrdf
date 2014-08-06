using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Elements;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query.Compiler
{
    /// <summary>
    /// An element visitor that compiles elements into query algebra
    /// </summary>
    public class CompilingElementVisitor
        : IElementVisitor
    {
        public CompilingElementVisitor()
            : this(Table.CreateUnit()) {}

        public CompilingElementVisitor(IAlgebra initiaAlgebra)
        {
            if (initiaAlgebra == null) throw new ArgumentNullException("initiaAlgebra");
            this.Algebras = new Stack<IAlgebra>();
            this.Algebras.Push(initiaAlgebra);
        }

        private Stack<IAlgebra> Algebras { get; set; }

        /// <summary>
        /// Compiles the given element into algebra
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>Compiled Algebra</returns>
        public IAlgebra Compile(IElement element)
        {
            element.Accept(this);
            if (this.Algebras.Count != 1) throw new RdfQueryException(String.Format("Element compilation failed, expected to produce 1 algebra but produced {0}", this.Algebras.Count));
            return this.Algebras.Pop();
        }

        public void Visit(BindElement bind)
        {
            this.Algebras.Push(Extend.Create(this.Algebras.Pop(), bind.Assignments));
        }

        public void Visit(DataElement data)
        {
            this.Algebras.Push(Join.Create(this.Algebras.Pop(), new Table(CompileInlineData(data.Data))));
        }

        public void Visit(FilterElement filter)
        {
            this.Algebras.Push(Filter.Create(this.Algebras.Pop(), filter.Expressions));
        }

        public void Visit(GroupElement group)
        {
            IList<FilterElement> filters = group.Elements.OfType<FilterElement>().ToList();

            IList<IElement> elements = group.Elements.Where(e => !(e is FilterElement)).ToList();

            IAlgebra algebra = Table.CreateUnit();
            foreach (IElement e in elements)
            {
                CompilingElementVisitor compiler = new CompilingElementVisitor(algebra);
                algebra = compiler.Compile(e);
            }
            this.Algebras.Push(Join.Create(this.Algebras.Pop(), algebra));

            // Apply filters
            foreach (FilterElement filter in filters)
            {
                filter.Accept(this);
            }
        }

        public void Visit(MinusElement minus)
        {
            CompilingElementVisitor compiler = new CompilingElementVisitor();
            IAlgebra rhs = compiler.Compile(minus.Element);
            this.Algebras.Push(new Minus(this.Algebras.Pop(), rhs));
        }

        public void Visit(NamedGraphElement namedGraph)
        {
            namedGraph.Element.Accept(this);
            this.Algebras.Push(new NamedGraph(namedGraph.Graph, this.Algebras.Pop()));
        }

        public void Visit(OptionalElement optional)
        {
            CompilingElementVisitor compiler = new CompilingElementVisitor();
            IAlgebra rhs = compiler.Compile(optional.Element);
            this.Algebras.Push(new LeftJoin(this.Algebras.Pop(), rhs, optional.Expressions));
        }

        public void Visit(PathBlockElement pathBlock)
        {
            // Handle empty path blocks
            if (pathBlock.Paths.Count == 0)
            {
                this.Algebras.Push(Join.Create(this.Algebras.Pop(), Table.CreateUnit()));
                return;
            }

            // Build up the algebra based on the paths
            List<Triple> currentTriples = new List<Triple>();
            IAlgebra current = Table.CreateUnit();
            foreach (TriplePath triplePath in pathBlock.Paths)
            {
                if (triplePath.IsTriple)
                {
                    currentTriples.Add(triplePath.AsTriple());
                }
                else
                {
                    // Add current group of triples if relevant
                    if (currentTriples.Count > 0)
                    {
                        current = Join.Create(current, new Bgp(currentTriples));
                        currentTriples.Clear();
                    }
                    // Wrap with path
                    current = new PropertyPath(current, triplePath);
                }
            }
            // Remember to add final group of triples if relevant
            if (currentTriples.Count > 0) current = Join.Create(current, new Bgp(currentTriples));

            // Finally join to existing algebra
            this.Algebras.Push(Join.Create(this.Algebras.Pop(), current));
        }

        public void Visit(ServiceElement service)
        {
            service.InnerElement.Accept(this);
            this.Algebras.Push(new Service(this.Algebras.Pop(), service.EndpointUri, service.IsSilent));
        }

        public void Visit(SubQueryElement subQuery)
        {
            DefaultQueryCompiler compiler = new DefaultQueryCompiler();
            IAlgebra innerAlgebra = compiler.Compile(subQuery.SubQuery);
            this.Algebras.Push(Join.Create(this.Algebras.Pop(), innerAlgebra));
        }

        public void Visit(TripleBlockElement tripleBlock)
        {
// ReSharper disable RedundantCast
            IAlgebra bgp = tripleBlock.Triples.Count > 0 ? (IAlgebra) new Bgp(tripleBlock.Triples) : (IAlgebra) Table.CreateUnit();
// ReSharper restore RedundantCast
            this.Algebras.Push(Join.Create(this.Algebras.Pop(), bgp));
        }

        public void Visit(UnionElement union)
        {
            // Firstly convert all the elements
            foreach (IElement element in union.Elements)
            {
                CompilingElementVisitor compiler = new CompilingElementVisitor();
                this.Algebras.Push(compiler.Compile(element));
            }

            // Then union together the results
            IAlgebra current = this.Algebras.Pop();
            for (int i = 1; i < union.Elements.Count; i++)
            {
                current = new Union(this.Algebras.Pop(), current);
            }
            this.Algebras.Push(Join.Create(this.Algebras.Pop(), current));
        }

        /// <summary>
        /// Compiles the result rows provided for inline data into the format needed for algebra
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <returns>Inline Data</returns>
        public static IEnumerable<ISolution> CompileInlineData(IEnumerable<IResultRow> rows)
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