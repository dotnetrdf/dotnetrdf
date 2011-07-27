using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;
using VDS.OWL.Term.Query;

namespace VDS.OWL.Term.Entity
{
    public interface IClassExpression
        : IType
    {
        IObjectAnd CreateObjectAnd(IEnumerable<IClassExpression> classes);

        IObjectAnd CreateObjectAnd(IClassExpression c);

        IDirectSubClassOf CreateDirectSubClassOf(IClassExpression c);

        IDisjointClasses CreateDisjointWith(IClassExpression c);

        IEquivalentClasses CreateEquivalentTo(IClassExpression c);

        IObjectOr CreateObjectOr(IEnumerable<IClassExpression> classes);

        IObjectOr CreateObjectOr(IClassExpression c);

        IStrictSubClassOf CreateStrictSubClassOf(IClassExpression c);

        ISubClassOf CreateSubClassOf(IClassExpression c);
    }

    public interface IClassVariable
        : INamedTerm, IClassExpression, IVariable
    {

    }
}
