using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.OWL.Term.Axiom;
using VDS.OWL.Term.Entity;
using VDS.OWL.Term.Function;
using VDS.OWL.Term.Query;
using VDS.OWL.Term.Rule;

namespace VDS.OWL.Term.Visitor
{
    public interface ITermVisitor<T>
        where T : ITerm
    {
        T Visit(IAnnotationPropertyDomain axiom);

        T Visit(IAnnotationPropertyRange axiom);

        T Visit(IAnnotationPropertyVariable var);

        T Visit(IAnonymousIndividual ind);

        T Visit(IAnonymousIndividualVariable var);

        T Visit(IAsymmetric axiom);

        T Visit(IAxiomAnnotation annotation);

        T Visit(IClassVariable var);

        T Visit(IDataAll all);

        T Visit(IDataAnd and);

        T Visit(IDataCardinality card);

        T Visit(IDataFunctional axiom);

        T Visit(IDataHasValue value);

        T Visit(IDataMax max);

        T Visit(IDataMin min);

        T Visit(IDataNAryAll all);

        T Visit(IDataNot not);

        T Visit(IDataOneOf oneOf);

        T Visit(IDataOr or);

        T Visit(IDataPropertyAssertion axiom);

        T Visit(IDataPropertyDomain axiom);

        T Visit(IDataPropertyList p);

        T Visit(IDataPropertyRange axiom);

        T Visit(IDataPropertyVariable var);

        T Visit(IDataSome some);

        T Visit(IDataTypeDefinition axiom);

        T Visit(IDataTypeVariable var);

        T Visit(IDeclaration declaration);

        T Visit(IDifferentIndividuals axiom);

        T Visit(IDirectSubClassOf query);

        T Visit(IDirectSubDataPropertyOf query);

        T Visit(IDirectSubObjectPropertyOf query);

        T Visit(IDirectTypeAssertion query);

        T Visit(IDisjointClasses axiom);

        T Visit(IDisjointDataProperties axiom);

        T Visit(IDisjointObjectProperties axiom);

        T Visit(IDisjointUnion axiom);

        T Visit(IEntityAnnotation annotation);

        T Visit(IEquivalentClasses axiom);

        T Visit(IEquivalentDataProperties axiom);

        T Visit(IEquivalentObjectProperties axiom);

        T Visit(IFacet facet);

        T Visit(IFacetRestriction fr);

        T Visit(IFunction function);

        T Visit(IFunctionAtom axiom);

        T Visit(IIndividualVariable var);

        T Visit(IInverseFunctional axiom);

        T Visit(IInverseObjectProperty p);

        T Visit(IInverseProperties axiom);

        T Visit(IIrreflexive axiom);

        T Visit(ILiteralVariable var);

        T Visit(INamedAnnotationProperty p);

        T Visit(INamedClass c);

        T Visit(INamedDataProperty p);

        T Visit(INamedDataType d);

        T Visit(INamedIndividual ind);

        T Visit(INamedObjectProperty p);

        T Visit(INAryDataType d);

        T Visit(INegativeDataPropertyAssertion axiom);

        T Visit(INegativeObjectPropertyAssertion axiom);

        T Visit(IObjectAll all);

        T Visit(IObjectAnd and);

        T Visit(IObjectCardinality card);

        T Visit(IObjectFunctional axiom);

        T Visit(IObjectHasValue value);

        T Visit(IObjectMax max);

        T Visit(IObjectMin min);

        T Visit(IObjectNot not);

        T Visit(IObjectOneOf oneOf);

        T Visit(IObjectOr or);

        T Visit(IObjectPropertyAssertion axiom);

        T Visit(IObjectPropertyDomain axiom);

        T Visit(IObjectPropertyList p);

        T Visit(IObjectPropertyRange axiom);

        T Visit(IObjectPropertyVariable var);

        T Visit(IObjectSelf self);

        T Visit(IObjectSome some);

        T Visit(IOntologyAnnotation annotation);

        T Visit(IPlainLiteral literal);

        T Visit(IQueryAnd query);

        T Visit(IQueryNot query);

        T Visit(IQueryOr query);

        T Visit(IReflexive axiom);

        T Visit(IRestrictedDataType d);

        T Visit(IRule axiom);

        T Visit(ISameIndividual axiom);

        T Visit(IStrictSubClassOf query);

        T Visit(IStrictSubDataPropertyOf query);

        T Visit(IStrictSubObjectPropertyOf query);

        T Visit(ISubAnnotationPropertyOf axiom);

        T Visit(ISubClassOf axiom);

        T Visit(ISubDataPropertyOf axiom);

        T Visit(ISubObjectPropertyChain axiom);

        T Visit(ISubObjectPropertyOf axiom);

        T Visit(ISymmetric axiom);

        T Visit(ITransitive axiom);

        T Visit(ITypeAssertion axiom);

        T Visit(ITypedLiteral literal); 
    }

    public interface ITermVisitorVoid
    {
        void Visit(IAnnotationPropertyDomain axiom);

        void Visit(IAnnotationPropertyRange axiom);

        void Visit(IAnnotationPropertyVariable var);

        void Visit(IAnonymousIndividual ind);

        void Visit(IAnonymousIndividualVariable var);

        void Visit(IAsymmetric axiom);

        void Visit(IAxiomAnnotation annotation);

        void Visit(IClassVariable var);

        void Visit(IDataAll all);

        void Visit(IDataAnd and);

        void Visit(IDataCardinality card);

        void Visit(IDataFunctional axiom);

        void Visit(IDataHasValue value);

        void Visit(IDataMax max);

        void Visit(IDataMin min);

        void Visit(IDataNAryAll all);

        void Visit(IDataNot not);

        void Visit(IDataOneOf oneOf);

        void Visit(IDataOr or);

        void Visit(IDataPropertyAssertion axiom);

        void Visit(IDataPropertyDomain axiom);

        void Visit(IDataPropertyList p);

        void Visit(IDataPropertyRange axiom);

        void Visit(IDataPropertyVariable var);

        void Visit(IDataSome some);

        void Visit(IDataTypeDefinition axiom);

        void Visit(IDataTypeVariable var);

        void Visit(IDeclaration declaration);

        void Visit(IDifferentIndividuals axiom);

        void Visit(IDirectSubClassOf query);

        void Visit(IDirectSubDataPropertyOf query);

        void Visit(IDirectSubObjectPropertyOf query);

        void Visit(IDirectTypeAssertion query);

        void Visit(IDisjointClasses axiom);

        void Visit(IDisjointDataProperties axiom);

        void Visit(IDisjointObjectProperties axiom);

        void Visit(IDisjointUnion axiom);

        void Visit(IEntityAnnotation annotation);

        void Visit(IEquivalentClasses axiom);

        void Visit(IEquivalentDataProperties axiom);

        void Visit(IEquivalentObjectProperties axiom);

        void Visit(IFacet facet);

        void Visit(IFacetRestriction fr);

        void Visit(IFunction function);

        void Visit(IFunctionAtom axiom);

        void Visit(IIndividualVariable var);

        void Visit(IInverseFunctional axiom);

        void Visit(IInverseObjectProperty p);

        void Visit(IInverseProperties axiom);

        void Visit(IIrreflexive axiom);

        void Visit(ILiteralVariable var);

        void Visit(INamedAnnotationProperty p);

        void Visit(INamedClass c);

        void Visit(INamedDataProperty p);

        void Visit(INamedDataType d);

        void Visit(INamedIndividual ind);

        void Visit(INamedObjectProperty p);

        void Visit(INAryDataType d);

        void Visit(INegativeDataPropertyAssertion axiom);

        void Visit(INegativeObjectPropertyAssertion axiom);

        void Visit(IObjectAll all);

        void Visit(IObjectAnd and);

        void Visit(IObjectCardinality card);

        void Visit(IObjectFunctional axiom);

        void Visit(IObjectHasValue value);

        void Visit(IObjectMax max);

        void Visit(IObjectMin min);

        void Visit(IObjectNot not);

        void Visit(IObjectOneOf oneOf);

        void Visit(IObjectOr or);

        void Visit(IObjectPropertyAssertion axiom);

        void Visit(IObjectPropertyDomain axiom);

        void Visit(IObjectPropertyList p);

        void Visit(IObjectPropertyRange axiom);

        void Visit(IObjectPropertyVariable var);

        void Visit(IObjectSelf self);

        void Visit(IObjectSome some);

        void Visit(IOntologyAnnotation annotation);

        void Visit(IPlainLiteral literal);

        void Visit(IQueryAnd query);

        void Visit(IQueryNot query);

        void Visit(IQueryOr query);

        void Visit(IReflexive axiom);

        void Visit(IRestrictedDataType d);

        void Visit(IRule axiom);

        void Visit(ISameIndividual axiom);

        void Visit(IStrictSubClassOf query);

        void Visit(IStrictSubDataPropertyOf query);

        void Visit(IStrictSubObjectPropertyOf query);

        void Visit(ISubAnnotationPropertyOf axiom);

        void Visit(ISubClassOf axiom);

        void Visit(ISubDataPropertyOf axiom);

        void Visit(ISubObjectPropertyChain axiom);

        void Visit(ISubObjectPropertyOf axiom);

        void Visit(ISymmetric axiom);

        void Visit(ITransitive axiom);

        void Visit(ITypeAssertion axiom);

        void Visit(ITypedLiteral literal); 
    }
}
