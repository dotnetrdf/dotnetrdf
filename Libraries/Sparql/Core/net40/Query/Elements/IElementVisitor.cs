namespace VDS.RDF.Query.Elements
{
    /// <summary>
    /// Interface for element visitors
    /// </summary>
    public interface IElementVisitor
    {
        void Visit(BindElement bind);

        void Visit(DataElement data);

        void Visit(FilterElement filter);

        void Visit(GroupElement group);

        void Visit(MinusElement minus);

        void Visit(NamedGraphElement namedGraph);

        void Visit(OptionalElement optional);

        void Visit(PathBlockElement pathBlock);

        void Visit(ServiceElement service);

        void Visit(SubQueryElement subQuery);

        void Visit(TripleBlockElement tripleBlock);

        void Visit(UnionElement union);
    }
}
