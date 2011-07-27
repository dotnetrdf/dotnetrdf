using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;

namespace VDS.OWL.Term.Entity
{
    public interface IDataAnd
        : IAnd<IDataType>, IDataType
    {

    }

    public interface IDataNot
        : INot<IDataType>, IDataType
    {

    }

    public interface IDataOneOf
        : IOneOf<ILiteralExpression>, IDataType
    {

    }

    public interface IDataOr
        : IOr<IDataType>, IDataType
    {

    }

    public interface IDataTypeVariable
        : INamedTerm, IDataType, IVariable
    {

    }


    public interface INamedDataType
        : IDataType, INamedEntity
    {
        IDataTypeDefinition CreateEquivalentTo(IDataType definition);

        IRestrictedDataType CreateLanguageRange(ILiteral value);

        IRestrictedDataType CreateLength(ILiteral value);

        IRestrictedDataType CreateMaxExclusive(ILiteral value);

        IRestrictedDataType CreateMaxInclusive(ILiteral value);

        IRestrictedDataType CreateMaxLength(ILiteral value);

        IRestrictedDataType CreateMinExclusive(ILiteral value);

        IRestrictedDataType CreateMinInclusive(ILiteral value);

        IRestrictedDataType CreateMinLength(ILiteral value);

        IRestrictedDataType CreatePattern(ILiteral value);

        IRestrictedDataType CreateRestriction(IFacet facet, ILiteral value);
    }

    public interface IRestrictedDataType
        : IDataType
    {
        INamedDataType DataType
        {
            get;
        }

        ITermSet<IFacetRestriction> Restrictions
        {
            get;
        }

        IRestrictedDataType CreateLanguageRange(ILiteral value);

        IRestrictedDataType CreateLength(ILiteral value);

        IRestrictedDataType CreateMaxExclusive(ILiteral value);

        IRestrictedDataType CreateMaxInclusive(ILiteral value);

        IRestrictedDataType CreateMaxLength(ILiteral value);

        IRestrictedDataType CreateMinExclusive(ILiteral value);

        IRestrictedDataType CreateMinInclusive(ILiteral value);

        IRestrictedDataType CreateMinLength(ILiteral value);

        IRestrictedDataType CreatePattern(ILiteral value);

        IRestrictedDataType CreateRestriction(IFacet facet, ILiteral value);
    }
}
