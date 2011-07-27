using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;
using VDS.OWL.Term.Query;

namespace VDS.OWL.Term.Entity
{
    public interface IIndividualExpression
        : INamedTerm, IValue
    {
        IDifferentIndividuals CreateDifferentFrom(IIndividualExpression ind);

        IDirectTypeAssertion CreateDirectType(IClassExpression c);

        IDataPropertyAssertion CreateFact(IDataProperty p, ILiteralExpression value);

        IObjectPropertyAssertion CreateFact(IObjectProperty p, IIndividualExpression ind);

        bool IsAnonymous
        {
            get;
        }

        INegativeDataPropertyAssertion CreateNegativeFact(IDataProperty p, ILiteralExpression value);

        INegativeObjectPropertyAssertion CreateNegativeFact(IObjectProperty p, IIndividualExpression ind);

        ISameIndividual CreateSameAs(IIndividualExpression ind);

        ITypeAssertion CreateType(IClassExpression c);
    }

    public interface IIndividual
        : IIndividualExpression, INamedEntity
    {
    }

    public interface IIndividualVariable
        : IIndividualExpression, IVariable
    {

    }
}
