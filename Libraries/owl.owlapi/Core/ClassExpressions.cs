using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class ObjectUnionOf : ClassExpression
    {
        public IEnumerable<ClassExpression> UnionOf { get; protected set; }
    }

    public class ObjectComplementOf : ClassExpression
    {
        public ClassExpression ComplementOf { get; protected set; }
    }

    public class ObjectOneOf : ClassExpression
    {
        public IEnumerable<IIndividual> Individuals { get; protected set; }
    }

    public class ObjectIntersectionOf : ClassExpression
    {
        public IEnumerable<ClassExpression> IntersectionOf { get; protected set; }
    }
}
