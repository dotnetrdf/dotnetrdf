namespace VDS.RDF.Query.Algebra.Transforms
{
    /// <summary>
    /// Interface for algebra transforms
    /// </summary>
    public interface IAlgebraTransform
    {
        IAlgebra Transform(Bgp bgp);

        IAlgebra Transform(Table table);

        IAlgebra Transform(Slice slice, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(NamedGraph namedGraph, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Filter filter, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Distinct distinct, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Reduced reduced, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Project project, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(OrderBy orderBy, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Extend extend, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(GroupBy groupBy, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Service service, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(PropertyPath path, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(TopN topN, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(PropertyFunction propertyFunction, IAlgebra transformedInnerAlgebra);

        IAlgebra Transform(Union union, IAlgebra transformedLhs, IAlgebra transformedRhs);

        IAlgebra Transform(Join join, IAlgebra transformedLhs, IAlgebra transformedRhs);

        IAlgebra Transform(LeftJoin leftJoin, IAlgebra transformedLhs, IAlgebra transformedRhs);

        IAlgebra Transform(Minus minus, IAlgebra transformedLhs, IAlgebra transformedRhs);
    }
}
