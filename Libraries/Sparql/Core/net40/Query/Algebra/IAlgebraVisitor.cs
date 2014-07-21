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

        void Visit(Join join);

        void Visit(LeftJoin leftJoin);

        void Visit(Minus minus);

        void Visit(Distinct distinct);

        void Visit(Reduced reduced);

        void Visit(Project project);

        void Visit(OrderBy orderBy);

        void Visit(Extend extend);

        void Visit(GroupBy groupBy);

        void Visit(Service service);

        void Visit(PropertyPath path);

        void Visit(TopN topN);

        void Visit(PropertyFunction propertyFunction);

        void Visit(IndexJoin indexJoin);
    }
}
