using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;
using VDS.OWL.Term.Query;

namespace VDS.OWL.Term.Entity
{
    public interface IDataProperty
        : IProperty
    {
        IDirectSubDataPropertyOf CreateDirectSubPropertyOf(IDataProperty p);

        IDisjointDataProperties CreateDisjointWith(IDataProperty p);

        IDataPropertyDomain CreateDomain(IClassExpression domain);

        IEquivalentDataProperties CreateEquivalentTo(IDataProperty p);

        IDataCardinality CreateExactCardinality(int cardinality);

        IDataCardinality CreateExactCardinality(int cardinality, IDataType t);

        IDataFunctional CreateFunctional();

        IDataMax CreateMaxCardinality(int cardinality);

        IDataMax CreateMaxCardinality(int cardinality, IDataType t);

        IDataMin CreateMinCardinality(int cardinality);

        IDataMin CreateMinCardinality(int cardinality, IDataType t);

        IDataAll CreateOnly(IDataType t);

        IDataPropertyRange CreateRange(IDataType range);

        IDataSome CreateSome(IDataType c);

        IStrictSubDataPropertyOf CreateStrictSubPropertyOf(IDataProperty p);

        ISubDataPropertyOf CreateSubPropertyof(IDataProperty p);

        IDataHasValue CreateValue(ILiteralExpression value);
    }

    public interface IDataPropertyVariable
        : INamedTerm, IDataProperty, IVariable
    {

    }
}
