namespace VDS.RDF.Query.Algebra
{
    public interface IAlgebraVisitor
    {
        void Visit(Bgp bgp);

        void Visit(Slice bgp);
    }
}
