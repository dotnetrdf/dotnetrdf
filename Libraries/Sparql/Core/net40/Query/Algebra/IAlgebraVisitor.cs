namespace VDS.RDF.Query.Algebra
{
    public interface IAlgebraVisitor
    {
        void Visit(Bgp bgp);

        void Visit(Slice slice);

        void Visit(Union union);

        void Visit(NamedGraph namedGraph);

        void Visit(Filter filter);

        void Visit(Table table);
    }
}
