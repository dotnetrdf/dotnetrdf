/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System.Collections.Generic;
using VDS.RDF.Parsing;

namespace VDS.RDF.Shacl;

/// <summary>
/// Represents the SHACL vocabulary.
/// </summary>
public static class Vocabulary
{
    /// <summary>
    /// The SHACL base URI.
    /// </summary>
    public const string BaseUri = "http://www.w3.org/ns/shacl#";

    private static readonly NodeFactory Factory = new NodeFactory(new NodeFactoryOptions());

    /// <summary>
    /// Gets a node representing alternativePath.
    /// </summary>
    public static IUriNode AlternativePath { get; } = ShaclNode("alternativePath");

    /// <summary>
    /// Gets a node representing and.
    /// </summary>
    public static IUriNode And { get; } = ShaclNode("and");

    /// <summary>
    /// Gets a node representing AndConstraintComponent.
    /// </summary>
    public static IUriNode AndConstraintComponent { get; } = ShaclNode("AndConstraintComponent");

    /// <summary>
    /// Gets a node representing ask.
    /// </summary>
    public static IUriNode Ask { get; } = ShaclNode("ask");

    /// <summary>
    /// Gets a node representing BlankNode.
    /// </summary>
    public static IUriNode BlankNode { get; } = ShaclNode("BlankNode");

    /// <summary>
    /// Gets a node representing BlankNodeOrIRI.
    /// </summary>
    public static IUriNode BlankNodeOrIri { get; } = ShaclNode("BlankNodeOrIRI");

    /// <summary>
    /// Gets a node representing BlankNodeOrLiteral.
    /// </summary>
    public static IUriNode BlankNodeOrLiteral { get; } = ShaclNode("BlankNodeOrLiteral");

    /// <summary>
    /// Gets a node representing class.
    /// </summary>
    public static IUriNode Class { get; } = ShaclNode("class");

    /// <summary>
    /// Gets a node representing ClassConstraintComponent.
    /// </summary>
    public static IUriNode ClassConstraintComponent { get; } = ShaclNode("ClassConstraintComponent");

    /// <summary>
    /// Gets a node representing closed.
    /// </summary>
    public static IUriNode Closed { get; } = ShaclNode("closed");

    /// <summary>
    /// Gets a node representing ClosedConstraintComponent.
    /// </summary>
    public static IUriNode ClosedConstraintComponent { get; } = ShaclNode("ClosedConstraintComponent");

    /// <summary>
    /// Gets a node representing conforms.
    /// </summary>
    public static IUriNode Conforms { get; } = ShaclNode("conforms");

    /// <summary>
    /// Gets a node representing ConstraintComponent.
    /// </summary>
    public static IUriNode ConstraintComponent { get; } = ShaclNode("ConstraintComponent");

    /// <summary>
    /// Gets a node representing datatype.
    /// </summary>
    public static IUriNode Datatype { get; } = ShaclNode("datatype");

    /// <summary>
    /// Gets a node representing DatatypeConstraintComponent.
    /// </summary>
    public static IUriNode DatatypeConstraintComponent { get; } = ShaclNode("DatatypeConstraintComponent");

    /// <summary>
    /// Gets a node representing deactivated.
    /// </summary>
    public static IUriNode Deactivated { get; } = ShaclNode("deactivated");

    /// <summary>
    /// Gets a node representing declare.
    /// </summary>
    public static IUriNode Declare { get; } = ShaclNode("declare");

    /// <summary>
    /// Gets a node representing disjoint.
    /// </summary>
    public static IUriNode Disjoint { get; } = ShaclNode("disjoint");

    /// <summary>
    /// Gets a node representing DisjointConstraintComponent.
    /// </summary>
    public static IUriNode DisjointConstraintComponent { get; } = ShaclNode("DisjointConstraintComponent");

    /// <summary>
    /// Gets a node representing EqualsConstraintComponent.
    /// </summary>
    public static IUriNode EqualsConstraintComponent { get; } = ShaclNode("EqualsConstraintComponent");

    /// <summary>
    /// Gets a node representing equals.
    /// </summary>
    public static IUriNode EqualsNode { get; } = ShaclNode("equals");

    /// <summary>
    /// Gets a node representing flags.
    /// </summary>
    public static IUriNode Flags { get; } = ShaclNode("flags");

    /// <summary>
    /// Gets a node representing focusNode.
    /// </summary>
    public static IUriNode FocusNode { get; } = ShaclNode("focusNode");

    /// <summary>
    /// Gets a node representing hasValue.
    /// </summary>
    public static IUriNode HasValue { get; } = ShaclNode("hasValue");

    /// <summary>
    /// Gets a node representing HasValueConstraintComponent.
    /// </summary>
    public static IUriNode HasValueConstraintComponent { get; } = ShaclNode("HasValueConstraintComponent");

    /// <summary>
    /// Gets a node representing ignoredProperties.
    /// </summary>
    public static IUriNode IgnoredProperties { get; } = ShaclNode("ignoredProperties");

    /// <summary>
    /// Gets a node representing in.
    /// </summary>
    public static IUriNode In { get; } = ShaclNode("in");

    /// <summary>
    /// Gets a node representing InConstraintComponent.
    /// </summary>
    public static IUriNode InConstraintComponent { get; } = ShaclNode("InConstraintComponent");

    /// <summary>
    /// Gets a node representing inversePath.
    /// </summary>
    public static IUriNode InversePath { get; } = ShaclNode("inversePath");

    /// <summary>
    /// Gets a node representing IRI.
    /// </summary>
    public static IUriNode Iri { get; } = ShaclNode("IRI");

    /// <summary>
    /// Gets a node representing IRIOrLiteral.
    /// </summary>
    public static IUriNode IriOrLiteral { get; } = ShaclNode("IRIOrLiteral");

    /// <summary>
    /// Gets a node representing languageIn.
    /// </summary>
    public static IUriNode LanguageIn { get; } = ShaclNode("languageIn");

    /// <summary>
    /// Gets a node representing LanguageInConstraintComponent.
    /// </summary>
    public static IUriNode LanguageInConstraintComponent { get; } = ShaclNode("LanguageInConstraintComponent");

    /// <summary>
    /// Gets a node representing lessThan.
    /// </summary>
    public static IUriNode LessThan { get; } = ShaclNode("lessThan");

    /// <summary>
    /// Gets a node representing LessThanConstraintComponent.
    /// </summary>
    public static IUriNode LessThanConstraintComponent { get; } = ShaclNode("LessThanConstraintComponent");

    /// <summary>
    /// Gets a node representing lessThanOrEquals.
    /// </summary>
    public static IUriNode LessThanOrEquals { get; } = ShaclNode("lessThanOrEquals");

    /// <summary>
    /// Gets a node representing LessThanOrEqualsConstraintComponent.
    /// </summary>
    public static IUriNode LessThanOrEqualsConstraintComponent { get; } = ShaclNode("LessThanOrEqualsConstraintComponent");

    /// <summary>
    /// Gets a node representing Literal.
    /// </summary>
    public static IUriNode Literal { get; } = ShaclNode("Literal");

    /// <summary>
    /// Gets a node representing maxCount.
    /// </summary>
    public static IUriNode MaxCount { get; } = ShaclNode("maxCount");

    /// <summary>
    /// Gets a node representing MaxCountConstraintComponent.
    /// </summary>
    public static IUriNode MaxCountConstraintComponent { get; } = ShaclNode("MaxCountConstraintComponent");

    /// <summary>
    /// Gets a node representing maxExclusive.
    /// </summary>
    public static IUriNode MaxExclusive { get; } = ShaclNode("maxExclusive");

    /// <summary>
    /// Gets a node representing MaxExclusiveConstraintComponent.
    /// </summary>
    public static IUriNode MaxExclusiveConstraintComponent { get; } = ShaclNode("MaxExclusiveConstraintComponent");

    /// <summary>
    /// Gets a node representing maxInclusive.
    /// </summary>
    public static IUriNode MaxInclusive { get; } = ShaclNode("maxInclusive");

    /// <summary>
    /// Gets a node representing MaxInclusiveConstraintComponent.
    /// </summary>
    public static IUriNode MaxInclusiveConstraintComponent { get; } = ShaclNode("MaxInclusiveConstraintComponent");

    /// <summary>
    /// Gets a node representing maxLength.
    /// </summary>
    public static IUriNode MaxLength { get; } = ShaclNode("maxLength");

    /// <summary>
    /// Gets a node representing MaxLengthConstraintComponent.
    /// </summary>
    public static IUriNode MaxLengthConstraintComponent { get; } = ShaclNode("MaxLengthConstraintComponent");

    /// <summary>
    /// Gets a node representing message.
    /// </summary>
    public static IUriNode Message { get; } = ShaclNode("message");

    /// <summary>
    /// Gets a node representing minCount.
    /// </summary>
    public static IUriNode MinCount { get; } = ShaclNode("minCount");

    /// <summary>
    /// Gets a node representing MinCountConstraintComponent.
    /// </summary>
    public static IUriNode MinCountConstraintComponent { get; } = ShaclNode("MinCountConstraintComponent");

    /// <summary>
    /// Gets a node representing minExclusive.
    /// </summary>
    public static IUriNode MinExclusive { get; } = ShaclNode("minExclusive");

    /// <summary>
    /// Gets a node representing MinExclusiveConstraintComponent.
    /// </summary>
    public static IUriNode MinExclusiveConstraintComponent { get; } = ShaclNode("MinExclusiveConstraintComponent");

    /// <summary>
    /// Gets a node representing minInclusive.
    /// </summary>
    public static IUriNode MinInclusive { get; } = ShaclNode("minInclusive");

    /// <summary>
    /// Gets a node representing MinInclusiveConstraintComponent.
    /// </summary>
    public static IUriNode MinInclusiveConstraintComponent { get; } = ShaclNode("MinInclusiveConstraintComponent");

    /// <summary>
    /// Gets a node representing minLength.
    /// </summary>
    public static IUriNode MinLength { get; } = ShaclNode("minLength");

    /// <summary>
    /// Gets a node representing MinLengthConstraintComponent.
    /// </summary>
    public static IUriNode MinLengthConstraintComponent { get; } = ShaclNode("MinLengthConstraintComponent");

    /// <summary>
    /// Gets a node representing namespace.
    /// </summary>
    public static IUriNode Namespace { get; } = ShaclNode("namespace");

    /// <summary>
    /// Gets a node representing node.
    /// </summary>
    public static IUriNode Node { get; } = ShaclNode("node");

    /// <summary>
    /// Gets a node representing NodeConstraintComponent.
    /// </summary>
    public static IUriNode NodeConstraintComponent { get; } = ShaclNode("NodeConstraintComponent");

    /// <summary>
    /// Gets a node representing nodeKind.
    /// </summary>
    public static IUriNode NodeKind { get; } = ShaclNode("nodeKind");

    /// <summary>
    /// Gets a node representing NodeKindConstraintComponent.
    /// </summary>
    public static IUriNode NodeKindConstraintComponent { get; } = ShaclNode("NodeKindConstraintComponent");

    /// <summary>
    /// Gets a node representing NodeShape.
    /// </summary>
    public static IUriNode NodeShape { get; } = ShaclNode("NodeShape");

    /// <summary>
    /// Gets a node representing nodeValidator.
    /// </summary>
    public static IUriNode NodeValidator { get; } = ShaclNode("nodeValidator");

    /// <summary>
    /// Gets a node representing not.
    /// </summary>
    public static IUriNode Not { get; } = ShaclNode("not");

    /// <summary>
    /// Gets a node representing NotConstraintComponent.
    /// </summary>
    public static IUriNode NotConstraintComponent { get; } = ShaclNode("NotConstraintComponent");

    /// <summary>
    /// Gets a node representing oneOrMorePath.
    /// </summary>
    public static IUriNode OneOrMorePath { get; } = ShaclNode("oneOrMorePath");

    /// <summary>
    /// Gets a node representing optional.
    /// </summary>
    public static IUriNode Optional { get; } = ShaclNode("optional");

    /// <summary>
    /// Gets a node representing or.
    /// </summary>
    public static IUriNode Or { get; } = ShaclNode("or");

    /// <summary>
    /// Gets a node representing OrConstraintComponent.
    /// </summary>
    public static IUriNode OrConstraintComponent { get; } = ShaclNode("OrConstraintComponent");

    /// <summary>
    /// Gets a node representing parameter.
    /// </summary>
    public static IUriNode Parameter { get; } = ShaclNode("parameter");

    /// <summary>
    /// Gets a node representing path.
    /// </summary>
    public static IUriNode Path { get; } = ShaclNode("path");

    /// <summary>
    /// Gets a node representing pattern.
    /// </summary>
    public static IUriNode Pattern { get; } = ShaclNode("pattern");

    /// <summary>
    /// Gets a node representing PatternConstraintComponent.
    /// </summary>
    public static IUriNode PatternConstraintComponent { get; } = ShaclNode("PatternConstraintComponent");

    /// <summary>
    /// Gets a node representing prefix.
    /// </summary>
    public static IUriNode Prefix { get; } = ShaclNode("prefix");

    /// <summary>
    /// Gets a node representing prefixes.
    /// </summary>
    public static IUriNode Prefixes { get; } = ShaclNode("prefixes");

    /// <summary>
    /// Gets a node representing property.
    /// </summary>
    public static IUriNode Property { get; } = ShaclNode("property");

    /// <summary>
    /// Gets a node representing PropertyShapeComponent.
    /// </summary>
    // See https://github.com/w3c/data-shapes/issues/103 (spec says sh:PropertyShapeComponent)
    public static IUriNode PropertyConstraintComponent { get; } = ShaclNode("PropertyShapeComponent");

    /// <summary>
    /// Gets a node representing PropertyShape.
    /// </summary>
    public static IUriNode PropertyShape { get; } = ShaclNode("PropertyShape");

    /// <summary>
    /// Gets a node representing propertyValidator.
    /// </summary>
    public static IUriNode PropertyValidator { get; } = ShaclNode("propertyValidator");

    /// <summary>
    /// Gets a node representing qualifiedMaxCount.
    /// </summary>
    public static IUriNode QualifiedMaxCount { get; } = ShaclNode("qualifiedMaxCount");

    /// <summary>
    /// Gets a node representing QualifiedMaxCountConstraintComponent.
    /// </summary>
    public static IUriNode QualifiedMaxCountConstraintComponent { get; } = ShaclNode("QualifiedMaxCountConstraintComponent");

    /// <summary>
    /// Gets a node representing qualifiedMinCount.
    /// </summary>
    public static IUriNode QualifiedMinCount { get; } = ShaclNode("qualifiedMinCount");

    /// <summary>
    /// Gets a node representing QualifiedMinCountConstraintComponent.
    /// </summary>
    public static IUriNode QualifiedMinCountConstraintComponent { get; } = ShaclNode("QualifiedMinCountConstraintComponent");

    /// <summary>
    /// Gets a node representing qualifiedValueShape.
    /// </summary>
    public static IUriNode QualifiedValueShape { get; } = ShaclNode("qualifiedValueShape");

    /// <summary>
    /// Gets a node representing qualifiedValueShapesDisjoint.
    /// </summary>
    public static IUriNode QualifiedValueShapesDisjoint { get; } = ShaclNode("qualifiedValueShapesDisjoint");

    /// <summary>
    /// Gets a node representing result.
    /// </summary>
    public static IUriNode Result { get; } = ShaclNode("result");

    /// <summary>
    /// Gets a node representing resultMessage.
    /// </summary>
    public static IUriNode ResultMessage { get; } = ShaclNode("resultMessage");

    /// <summary>
    /// Gets a node representing resultPath.
    /// </summary>
    public static IUriNode ResultPath { get; } = ShaclNode("resultPath");

    /// <summary>
    /// Gets a node representing resultSeverity.
    /// </summary>
    public static IUriNode ResultSeverity { get; } = ShaclNode("resultSeverity");

    /// <summary>
    /// Gets a node representing select.
    /// </summary>
    public static IUriNode Select { get; } = ShaclNode("select");

    /// <summary>
    /// Gets a node representing severity.
    /// </summary>
    public static IUriNode Severity { get; } = ShaclNode("severity");

    /// <summary>
    /// Gets a node representing sourceConstraint.
    /// </summary>
    public static IUriNode SourceConstraint { get; } = ShaclNode("sourceConstraint");

    /// <summary>
    /// Gets a node representing sourceConstraintComponent.
    /// </summary>
    public static IUriNode SourceConstraintComponent { get; } = ShaclNode("sourceConstraintComponent");

    /// <summary>
    /// Gets a node representing sourceShape.
    /// </summary>
    public static IUriNode SourceShape { get; } = ShaclNode("sourceShape");

    /// <summary>
    /// Gets a node representing sparql.
    /// </summary>
    public static IUriNode Sparql { get; } = ShaclNode("sparql");

    /// <summary>
    /// Gets a node representing SPARQLAskValidator.
    /// </summary>
    public static IUriNode SparqlAskValidato { get; } = ShaclNode("SPARQLAskValidator");

    /// <summary>
    /// Gets a node representing SPARQLConstraintComponent.
    /// </summary>
    public static IUriNode SparqlConstraintComponent { get; } = ShaclNode("SPARQLConstraintComponent");

    /// <summary>
    /// Gets a node representing targetClass.
    /// </summary>
    public static IUriNode TargetClass { get; } = ShaclNode("targetClass");

    /// <summary>
    /// Gets a node representing targetNode.
    /// </summary>
    public static IUriNode TargetNode { get; } = ShaclNode("targetNode");

    /// <summary>
    /// Gets a node representing targetObjectsOf.
    /// </summary>
    public static IUriNode TargetObjectsOf { get; } = ShaclNode("targetObjectsOf");

    /// <summary>
    /// Gets a node representing targetSubjectsOf.
    /// </summary>
    public static IUriNode TargetSubjectsOf { get; } = ShaclNode("targetSubjectsOf");

    /// <summary>
    /// Gets a node representing uniqueLang.
    /// </summary>
    public static IUriNode UniqueLang { get; } = ShaclNode("uniqueLang");

    /// <summary>
    /// Gets a node representing UniqueLangConstraintComponent.
    /// </summary>
    public static IUriNode UniqueLangConstraintComponent { get; } = ShaclNode("UniqueLangConstraintComponent");

    /// <summary>
    /// Gets a node representing ValidationReport.
    /// </summary>
    public static IUriNode ValidationReport { get; } = ShaclNode("ValidationReport");

    /// <summary>
    /// Gets a node representing ValidationResult.
    /// </summary>
    public static IUriNode ValidationResult { get; } = ShaclNode("ValidationResult");

    /// <summary>
    /// Gets a node representing validator.
    /// </summary>
    public static IUriNode Validator { get; } = ShaclNode("validator");

    /// <summary>
    /// Gets a node representing value.
    /// </summary>
    public static IUriNode Value { get; } = ShaclNode("value");

    /// <summary>
    /// Gets a node representing Violation.
    /// </summary>
    public static IUriNode Violation { get; } = ShaclNode("Violation");

    /// <summary>
    /// Gets a node representing xone.
    /// </summary>
    public static IUriNode Xone { get; } = ShaclNode("xone");

    /// <summary>
    /// Gets a node representing XoneConstraintComponent.
    /// </summary>
    public static IUriNode XoneConstraintComponent { get; } = ShaclNode("XoneConstraintComponent");

    /// <summary>
    /// Gets a node representing zeroOrMorePath.
    /// </summary>
    public static IUriNode ZeroOrMorePath { get; } = ShaclNode("zeroOrMorePath");

    /// <summary>
    /// Gets a node representing zeroOrOnePath.
    /// </summary>
    public static IUriNode ZeroOrOnePath { get; } = ShaclNode("zeroOrOnePath");

    #region Collections

    /// <summary>
    /// Gets the list of shapes.
    /// </summary>
    public static IEnumerable<IUriNode> Shapes
    {
        get
        {
            yield return NodeShape;
            yield return PropertyShape;
        }
    }

    /// <summary>
    /// Gets the list of targets.
    /// </summary>
    public static IEnumerable<IUriNode> Targets
    {
        get
        {
            yield return TargetNode;
            yield return TargetClass;
            yield return TargetSubjectsOf;
            yield return TargetObjectsOf;
        }
    }

    /// <summary>
    /// Gets the list of constraints.
    /// </summary>
    public static IEnumerable<IUriNode> Constraints
    {
        get
        {
            yield return Property;
            yield return MaxCount;
            yield return NodeKind;
            yield return MinCount;
            yield return Node;
            yield return Datatype;
            yield return Closed;
            yield return HasValue;
            yield return Or;
            yield return Class;
            yield return Not;
            yield return Xone;
            yield return In;
            yield return Sparql;
            yield return Pattern;
            yield return MinInclusive;
            yield return MinExclusive;
            yield return MaxExclusive;
            yield return MinLength;
            yield return MaxInclusive;
            yield return And;
            yield return QualifiedMinCount;
            yield return QualifiedMaxCount;
            yield return EqualsNode;
            yield return LanguageIn;
            yield return LessThan;
            yield return Disjoint;
            yield return LessThanOrEquals;
            yield return UniqueLang;
            yield return MaxLength;
        }
    }

    /// <summary>
    /// Gets the list of blank node kinds.
    /// </summary>
    public static IEnumerable<IUriNode> BlankNodeKinds
    {
        get
        {
            yield return Vocabulary.BlankNode;
            yield return Vocabulary.BlankNodeOrIri;
            yield return Vocabulary.BlankNodeOrLiteral;
        }
    }

    /// <summary>
    /// Gets the list of literal node kinds.
    /// </summary>
    public static IEnumerable<IUriNode> LiteralNodeKinds
    {
        get
        {
            yield return Vocabulary.Literal;
            yield return Vocabulary.BlankNodeOrLiteral;
            yield return Vocabulary.IriOrLiteral;
        }
    }

    /// <summary>
    /// Gets the list of IRI node kinds.
    /// </summary>
    public static IEnumerable<IUriNode> IriNodeKinds
    {
        get
        {
            yield return Vocabulary.Iri;
            yield return Vocabulary.IriOrLiteral;
            yield return Vocabulary.BlankNodeOrIri;
        }
    }

    /// <summary>
    /// Gets the list of predicates to expand in validation reports.
    /// </summary>
    public static IEnumerable<INode> PredicatesToExpandInReport
    {
        get
        {
            yield return Result;
            yield return ResultPath;

            yield return RdfRest;
            yield return RdfFirst;

            yield return ZeroOrMorePath;
            yield return OneOrMorePath;
            yield return AlternativePath;
            yield return InversePath;
            yield return ZeroOrOnePath;
        }
    }

    #endregion

    #region Related vocabularies

    internal static IUriNode RdfType { get; } = AnyNode(RdfSpecsHelper.RdfType);

    internal static IUriNode RdfFirst { get; } = AnyNode(RdfSpecsHelper.RdfListFirst);

    internal static IUriNode RdfRest { get; } = AnyNode(RdfSpecsHelper.RdfListRest);

    internal static IUriNode RdfsClass { get; } = AnyNode("http://www.w3.org/2000/01/rdf-schema#Class");

    internal static IUriNode RdfsSubClassOf { get; } = AnyNode("http://www.w3.org/2000/01/rdf-schema#subClassOf");

    internal static IUriNode OwlImports { get; } = AnyNode(NamespaceMapper.OWL + "imports");

    #endregion

    private static IUriNode ShaclNode(string name) => AnyNode($"{BaseUri}{name}");

    private static IUriNode AnyNode(string uri) => Factory.CreateUriNode(Factory.UriFactory.Create(uri));
}
