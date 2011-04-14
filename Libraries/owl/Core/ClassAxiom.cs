using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.OWL
{
    public class ClassAxiom : Axiom
    {
    }

    public class SubClassOfAxiom : ClassAxiom
    {
        public ClassExpression SubClassExpression { get; protected set; }

        public ClassExpression SuperClassExpression { get; protected set; }
    }


    public class EquivalentClassesAxiom : ClassAxiom
    {
        public IEnumerable<ClassExpression> EquivalentClasses { get; protected set; }
    }

    public class DisjointClassesAxiom : ClassAxiom
    {
        public IEnumerable<ClassExpression> DisjointClasses { get; protected set; }
    }

    public class DisjointUnionAxiom : ClassAxiom
    {
        public IEnumerable<ClassExpression> DisjointUnionClasses { get; protected set; }
    }
}
