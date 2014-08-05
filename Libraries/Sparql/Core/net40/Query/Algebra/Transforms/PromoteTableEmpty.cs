namespace VDS.RDF.Query.Algebra.Transforms
{
    public class PromoteTableEmpty
        : TransformCopy
    {
        public override IAlgebra Transform(Join join, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            // If either side is table empty then join will produce empty result
            if (IsTableEmpty(transformedLhs) || IsTableEmpty(transformedRhs)) return Table.CreateEmpty();

            return base.Transform(join, transformedLhs, transformedRhs);
        }

        public override IAlgebra Transform(LeftJoin leftJoin, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            // If LHS is table empty whole join will produce empty result
            if (IsTableEmpty(transformedLhs)) return Table.CreateEmpty();

            // If RHS is table empty then only preserve LHS
            if (IsTableEmpty(transformedRhs)) return transformedLhs;

            return base.Transform(leftJoin, transformedLhs, transformedRhs);
        }

        public override IAlgebra Transform(Union union, IAlgebra transformedLhs, IAlgebra transformedRhs)
        {
            // If LHS is table empty then may only need to preserve RHS
            if (IsTableEmpty(transformedLhs))
            {
                // If RHS is also table empty then union will produce empty result
                if (IsTableEmpty(transformedRhs)) return Table.CreateEmpty();

                // Otherwise just preserve RHS
                return transformedRhs;
            }
            
            // If RHS is table empty then only need to preserve LHS
            if (IsTableEmpty(transformedRhs)) return transformedLhs;

            return base.Transform(union, transformedLhs, transformedRhs);
        }

        private static bool IsTableEmpty(IAlgebra algebra)
        {
            Table t = algebra as Table;
            return t != null && t.IsEmpty;
        }
    }
}