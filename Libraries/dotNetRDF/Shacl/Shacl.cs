/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Shacl
{
    using System.Collections.Generic;
    using VDS.RDF;

    public static class Shacl
    {
        public const string BaseUri = "http://www.w3.org/ns/shacl#";
        private static readonly NodeFactory factory = new NodeFactory();

        public static IUriNode Path { get; } = ShaclNode("path");

        public static IUriNode Deactivated { get; } = ShaclNode("deactivated");

        public static IUriNode Severity { get; } = ShaclNode("severity");

        public static IUriNode Message { get; } = ShaclNode("message");

        public static IUriNode Conforms { get; } = ShaclNode("conforms");

        #region Targets
        public static IUriNode TargetClass { get; } = ShaclNode("targetClass");

        public static IUriNode TargetNode { get; } = ShaclNode("targetNode");

        public static IUriNode TargetObjectsOf { get; } = ShaclNode("targetObjectsOf");

        public static IUriNode TargetSubjectsOf { get; } = ShaclNode("targetSubjectsOf");
        #endregion

        #region Constraints
        public static IUriNode Class { get; } = ShaclNode("class");

        public static IUriNode Node { get; } = ShaclNode("node");

        public static IUriNode Property { get; } = ShaclNode("property");

        public static IUriNode Datatype { get; } = ShaclNode("datatype");

        public static IUriNode And { get; } = ShaclNode("and");

        public static IUriNode Or { get; } = ShaclNode("or");

        public static IUriNode Not { get; } = ShaclNode("not");

        public static IUriNode Xone { get; } = ShaclNode("xone");

        public static IUriNode NodeKind { get; } = ShaclNode("nodeKind");

        public static IUriNode MinLength { get; } = ShaclNode("minLength");

        public static IUriNode MaxLength { get; } = ShaclNode("maxLength");

        public static IUriNode LanguageIn { get; } = ShaclNode("languageIn");

        public static IUriNode In { get; } = ShaclNode("in");

        public static IUriNode MinCount { get; } = ShaclNode("minCount");

        public static IUriNode MaxCount { get; } = ShaclNode("maxCount");

        public static IUriNode UniqueLang { get; } = ShaclNode("uniqueLang");

        public static IUriNode HasValue { get; } = ShaclNode("hasValue");

        public static IUriNode Pattern { get; } = ShaclNode("pattern");

        public static IUriNode Flags { get; } = ShaclNode("flags");

        public static IUriNode Equals { get; } = ShaclNode("equals");

        public static IUriNode Disjoint { get; } = ShaclNode("disjoint");

        public static IUriNode LessThan { get; } = ShaclNode("lessThan");

        public static IUriNode LessThanOrEquals { get; } = ShaclNode("lessThanOrEquals");

        public static IUriNode MinExclusive { get; } = ShaclNode("minExclusive");

        public static IUriNode MinInclusive { get; } = ShaclNode("minInclusive");

        public static IUriNode MaxExclusive { get; } = ShaclNode("maxExclusive");

        public static IUriNode MaxInclusive { get; } = ShaclNode("maxInclusive");

        public static IUriNode QualifiedMinCount { get; } = ShaclNode("qualifiedMinCount");

        public static IUriNode QualifiedMaxCount { get; } = ShaclNode("qualifiedMaxCount");

        public static IUriNode QualifiedValueShape { get; } = ShaclNode("qualifiedValueShape");

        public static IUriNode QualifiedValueShapesDisjoint { get; } = ShaclNode("qualifiedValueShapesDisjoint");

        public static IUriNode Closed { get; } = ShaclNode("closed");

        public static IUriNode IgnoredProperties { get; } = ShaclNode("ignoredProperties");
        #endregion

        #region Constraint components
        public static IUriNode ClassConstraintComponent { get; } = ShaclNode("ClassConstraintComponent");

        public static IUriNode NodeConstraintComponent { get; } = ShaclNode("NodeConstraintComponent");

        public static IUriNode PropertyShapeComponent { get; } = ShaclNode("PropertyConstraintComponent");

        public static IUriNode DatatypeConstraintComponent { get; } = ShaclNode("DatatypeConstraintComponent");

        public static IUriNode AndConstraintComponent { get; } = ShaclNode("AndConstraintComponent");

        public static IUriNode OrConstraintComponent { get; } = ShaclNode("OrConstraintComponent");

        public static IUriNode NotConstraintComponent { get; } = ShaclNode("NotConstraintComponent");

        public static IUriNode XoneConstraintComponent { get; } = ShaclNode("XoneConstraintComponent");

        public static IUriNode NodeKindConstraintComponent { get; } = ShaclNode("NodeKindConstraintComponent");

        public static IUriNode MinLengthConstraintComponent { get; } = ShaclNode("MinLengthConstraintComponent");

        public static IUriNode MaxLengthConstraintComponent { get; } = ShaclNode("MaxLengthConstraintComponent");

        public static IUriNode LanguageInConstraintComponent { get; } = ShaclNode("LanguageInConstraintComponent");

        public static IUriNode InConstraintComponent { get; } = ShaclNode("InConstraintComponent");

        public static IUriNode MinCountConstraintComponent { get; } = ShaclNode("MinCountConstraintComponent");

        public static IUriNode MaxCountConstraintComponent { get; } = ShaclNode("MaxCountConstraintComponent");

        public static IUriNode UniqueLangConstraintComponent { get; } = ShaclNode("UniqueLangConstraintComponent");

        public static IUriNode HasValueConstraintComponent { get; } = ShaclNode("HasValueConstraintComponent");

        public static IUriNode PatternConstraintComponent { get; } = ShaclNode("PatternConstraintComponent");

        public static IUriNode EqualsConstraintComponent { get; } = ShaclNode("EqualsConstraintComponent");

        public static IUriNode DisjointConstraintComponent { get; } = ShaclNode("DisjointConstraintComponent");

        public static IUriNode LessThanConstraintComponent { get; } = ShaclNode("LessThanConstraintComponent");

        public static IUriNode LessThanOrEqualsConstraintComponent { get; } = ShaclNode("LessThanOrEqualsConstraintComponent");

        public static IUriNode MinExclusiveConstraintComponent { get; } = ShaclNode("MinExclusiveConstraintComponent");

        public static IUriNode MinInclusiveConstraintComponent { get; } = ShaclNode("MinInclusiveConstraintComponent");

        public static IUriNode MaxExclusiveConstraintComponent { get; } = ShaclNode("MaxExclusiveConstraintComponent");

        public static IUriNode MaxInclusiveConstraintComponent { get; } = ShaclNode("MaxInclusiveConstraintComponent");

        public static IUriNode QualifiedMinCountConstraintComponent { get; } = ShaclNode("QualifiedMinCountConstraintComponent");

        public static IUriNode QualifiedMaxCountConstraintComponent { get; } = ShaclNode("QualifiedMaxCountConstraintComponent");

        public static IUriNode ClosedConstraintComponent { get; } = ShaclNode("ClosedConstraintComponent");
        #endregion

        #region Shapes
        public static IUriNode NodeShape { get; } = ShaclNode("NodeShape");

        public static IUriNode PropertyShape { get; } = ShaclNode("PropertyShape");
        #endregion

        #region Paths
        public static IUriNode AlternativePath { get; } = ShaclNode("alternativePath");

        public static IUriNode InversePath { get; } = ShaclNode("inversePath");

        public static IUriNode OneOrMorePath { get; } = ShaclNode("oneOrMorePath");

        public static IUriNode ZeroOrMorePath { get; } = ShaclNode("zeroOrMorePath");

        public static IUriNode ZeroOrOnePath { get; } = ShaclNode("zeroOrOnePath");
        #endregion

        #region Node kinds
        public static IUriNode BlankNode { get; } = ShaclNode("BlankNode");

        public static IUriNode Iri { get; } = ShaclNode("IRI");

        public static IUriNode Literal { get; } = ShaclNode("Literal");

        public static IUriNode BlankNodeOrIri { get; } = ShaclNode("BlankNodeOrIRI");

        public static IUriNode BlankNodeOrLiteral { get; } = ShaclNode("BlankNodeOrLiteral");

        public static IUriNode IriOrLiteral { get; } = ShaclNode("IRIOrLiteral");
        #endregion

        #region Report
        public static IUriNode Result { get; } = ShaclNode("result");

        public static IUriNode ValidationReport { get; } = ShaclNode("ValidationReport");

        public static IUriNode ValidationResult { get; } = ShaclNode("ValidationResult");

        public static IUriNode FocusNode { get; } = ShaclNode("focusNode");

        public static IUriNode Value { get; } = ShaclNode("value");

        public static IUriNode SourceShape { get; } = ShaclNode("sourceShape");

        public static IUriNode SourceConstraintComponent { get; } = ShaclNode("sourceConstraintComponent");

        public static IUriNode ResultSeverity { get; } = ShaclNode("resultSeverity");

        public static IUriNode Violation { get; } = ShaclNode("Violation");

        public static IUriNode ResultPath { get; } = ShaclNode("resultPath");

        public static IUriNode ResultMessage { get; } = ShaclNode("resultMessage");
        #endregion

        #region Collections

        public static IEnumerable<IUriNode> Shapes
        {
            get
            {
                yield return NodeShape;
                yield return PropertyShape;
            }
        }

        public static IEnumerable<IUriNode> Targets
        {
            get
            {
                yield return TargetClass;
                yield return TargetNode;
                yield return TargetObjectsOf;
                yield return TargetSubjectsOf;
            }
        }

        public static IEnumerable<IUriNode> Constraints
        {
            get
            {
                yield return Class;
                yield return Node;
                yield return Property;
                yield return Datatype;
                yield return And;
                yield return Or;
                yield return Not;
                yield return Xone;
                yield return NodeKind;
                yield return MinLength;
                yield return MaxLength;
                yield return LanguageIn;
                yield return In;
                yield return MinCount;
                yield return MaxCount;
                yield return UniqueLang;
                yield return HasValue;
                yield return Pattern;
                yield return Equals;
                yield return Disjoint;
                yield return LessThan;
                yield return LessThanOrEquals;
                yield return MinExclusive;
                yield return MinInclusive;
                yield return MaxExclusive;
                yield return MaxInclusive;
                yield return QualifiedMinCount;
                yield return QualifiedMaxCount;
                yield return Closed;
            }
        }

        public static IEnumerable<IUriNode> BlankNodeKinds
        {
            get
            {
                yield return Shacl.BlankNode;
                yield return Shacl.BlankNodeOrIri;
                yield return Shacl.BlankNodeOrLiteral;
            }
        }

        public static IEnumerable<IUriNode> LiteralNodeKinds
        {
            get
            {
                yield return Shacl.Literal;
                yield return Shacl.BlankNodeOrLiteral;
                yield return Shacl.IriOrLiteral;
            }
        }

        public static IEnumerable<IUriNode> IriNodeKinds
        {
            get
            {
                yield return Shacl.Iri;
                yield return Shacl.IriOrLiteral;
                yield return Shacl.BlankNodeOrIri;
            }
        }

        public static IEnumerable<IUriNode> Paths
        {
            get
            {
                yield return AlternativePath;
                yield return InversePath;
                yield return OneOrMorePath;
                yield return ZeroOrMorePath;
                yield return ZeroOrOnePath;

            }
        }

        #endregion

        private static IUriNode ShaclNode(string name) => factory.CreateUriNode(UriFactory.Create($"{BaseUri}{name}"));
    }
}
