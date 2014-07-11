namespace VDS.RDF.Query.Elements
{
    /// <summary>
    /// Interface for element visitors
    /// </summary>
    public interface IElementVisitor
    {
        void Visit(BindElement bind);

        void Visit(DataElement bind);

        void Visit(FilterElement bind);

        void Visit(GroupElement bind);

        void Visit(MinusElement bind);

        void Visit(NamedGraphElement bind);

        void Visit(OptionalElement bind);

        void Visit(PathBlockElement bind);

        void Visit(ServiceElement bind);

        void Visit(SubQueryElement bind);

        void Visit(TripleBlockElement bind);

        void Visit(UnionElement bind);
    }
}
