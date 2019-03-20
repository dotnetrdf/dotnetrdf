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

    public class Shacl
    {
        public const string BaseUri = "http://www.w3.org/ns/shacl#";
        private static NodeFactory factory = new NodeFactory();

        public static IUriNode Path => ShaclNode("path");

        public static IUriNode Deactivated => ShaclNode("deactivated");

        public static IUriNode Severity => ShaclNode("severity");

        public static IUriNode Message => ShaclNode("message");

        public static IUriNode Conforms => ShaclNode("conforms");

        #region Targets
        public static IUriNode TargetClass => ShaclNode("targetClass");

        public static IUriNode TargetNode => ShaclNode("targetNode");

        public static IUriNode TargetObjectsOf => ShaclNode("targetObjectsOf");

        public static IUriNode TargetSubjectsOf => ShaclNode("targetSubjectsOf");
        #endregion

        #region Constraints
        public static IUriNode Class => ShaclNode("class");

        public static IUriNode Node => ShaclNode("node");

        public static IUriNode Property => ShaclNode("property");

        public static IUriNode Datatype => ShaclNode("datatype");

        public static IUriNode And => ShaclNode("and");

        public static IUriNode Or => ShaclNode("or");

        public static IUriNode Not => ShaclNode("not");

        public static IUriNode Xone => ShaclNode("xone");

        public static IUriNode NodeKind => ShaclNode("nodeKind");

        public static IUriNode MinLength => ShaclNode("minLength");

        public static IUriNode MaxLength => ShaclNode("maxLength");

        public static IUriNode LanguageIn => ShaclNode("languageIn");

        public static IUriNode In => ShaclNode("in");

        public static IUriNode MinCount => ShaclNode("minCount");

        public static IUriNode MaxCount => ShaclNode("maxCount");

        public static IUriNode UniqueLang => ShaclNode("uniqueLang");

        public static IUriNode HasValue => ShaclNode("hasValue");

        public static IUriNode Pattern => ShaclNode("pattern");

        public static IUriNode Flags => ShaclNode("flags");

        public static IUriNode Equals => ShaclNode("equals");

        public static IUriNode Disjoint => ShaclNode("disjoint");

        public static IUriNode LessThan => ShaclNode("lessThan");

        public static IUriNode LessThanOrEquals => ShaclNode("lessThanOrEquals");

        public static IUriNode MinExclusive => ShaclNode("minExclusive");

        public static IUriNode MinInclusive => ShaclNode("minInclusive");

        public static IUriNode MaxExclusive => ShaclNode("maxExclusive");

        public static IUriNode MaxInclusive => ShaclNode("maxInclusive");

        public static IUriNode QualifiedMinCount => ShaclNode("qualifiedMinCount");

        public static IUriNode QualifiedMaxCount => ShaclNode("qualifiedMaxCount");

        public static IUriNode QualifiedValueShape => ShaclNode("qualifiedValueShape");

        public static IUriNode QualifiedValueShapesDisjoint => ShaclNode("qualifiedValueShapesDisjoint");

        public static IUriNode Closed => ShaclNode("closed");

        public static IUriNode IgnoredProperties => ShaclNode("ignoredProperties");
        #endregion

        #region Constraint components
        public static IUriNode ClassConstraintComponent => ShaclNode("ClassConstraintComponent");

        public static IUriNode NodeConstraintComponent => ShaclNode("NodeConstraintComponent");

        public static IUriNode PropertyConstraintComponent => ShaclNode("PropertyConstraintComponent");

        public static IUriNode DatatypeConstraintComponent => ShaclNode("DatatypeConstraintComponent");

        public static IUriNode AndConstraintComponent => ShaclNode("AndConstraintComponent");

        public static IUriNode OrConstraintComponent => ShaclNode("OrConstraintComponent");

        public static IUriNode NotConstraintComponent => ShaclNode("NotConstraintComponent");

        public static IUriNode XoneConstraintComponent => ShaclNode("XoneConstraintComponent");

        public static IUriNode NodeKindConstraintComponent => ShaclNode("NodeKindConstraintComponent");

        public static IUriNode MinLengthConstraintComponent => ShaclNode("MinLengthConstraintComponent");

        public static IUriNode MaxLengthConstraintComponent => ShaclNode("MaxLengthConstraintComponent");

        public static IUriNode LanguageInConstraintComponent => ShaclNode("LanguageInConstraintComponent");

        public static IUriNode InConstraintComponent => ShaclNode("InConstraintComponent");

        public static IUriNode MinCountConstraintComponent => ShaclNode("MinCountConstraintComponent");

        public static IUriNode MaxCountConstraintComponent => ShaclNode("MaxCountConstraintComponent");

        public static IUriNode UniqueLangConstraintComponent => ShaclNode("UniqueLangConstraintComponent");

        public static IUriNode HasValueConstraintComponent => ShaclNode("HasValueConstraintComponent");

        public static IUriNode PatternConstraintComponent => ShaclNode("PatternConstraintComponent");

        public static IUriNode EqualsConstraintComponent => ShaclNode("EqualsConstraintComponent");

        public static IUriNode DisjointConstraintComponent => ShaclNode("DisjointConstraintComponent");

        public static IUriNode LessThanConstraintComponent => ShaclNode("LessThanConstraintComponent");

        public static IUriNode LessThanOrEqualsConstraintComponent => ShaclNode("LessThanOrEqualsConstraintComponent");

        public static IUriNode MinExclusiveConstraintComponent => ShaclNode("MinExclusiveConstraintComponent");

        public static IUriNode MinInclusiveConstraintComponent => ShaclNode("MinInclusiveConstraintComponent");

        public static IUriNode MaxExclusiveConstraintComponent => ShaclNode("MaxExclusiveConstraintComponent");

        public static IUriNode MaxInclusiveConstraintComponent => ShaclNode("MaxInclusiveConstraintComponent");

        public static IUriNode QualifiedMinCountConstraintComponent => ShaclNode("QualifiedMinCountConstraintComponent");

        public static IUriNode QualifiedMaxCountConstraintComponent => ShaclNode("QualifiedMaxCountConstraintComponent");

        public static IUriNode ClosedConstraintComponent => ShaclNode("ClosedConstraintComponent");
        #endregion

        #region Shapes
        public static IUriNode NodeShape => ShaclNode("NodeShape");

        public static IUriNode PropertyShape => ShaclNode("PropertyShape");
        #endregion

        #region Paths
        public static IUriNode AlternativePath => ShaclNode("alternativePath");

        public static IUriNode InversePath => ShaclNode("inversePath");

        public static IUriNode OneOrMorePath => ShaclNode("oneOrMorePath");

        public static IUriNode ZeroOrMorePath => ShaclNode("zeroOrMorePath");

        public static IUriNode ZeroOrOnePath => ShaclNode("zeroOrOnePath");
        #endregion

        #region Node kinds
        public static IUriNode BlankNode => ShaclNode("BlankNode");

        public static IUriNode Iri => ShaclNode("IRI");

        public static IUriNode Literal => ShaclNode("Literal");

        public static IUriNode BlankNodeOrIri => ShaclNode("BlankNodeOrIRI");

        public static IUriNode BlankNodeOrLiteral => ShaclNode("BlankNodeOrLiteral");

        public static IUriNode IriOrLiteral => ShaclNode("IRIOrLiteral");
        #endregion

        #region Report
        public static IUriNode Result => ShaclNode("result");

        public static IUriNode ValidationReport => ShaclNode("ValidationReport");

        public static IUriNode ValidationResult => ShaclNode("ValidationResult");

        public static IUriNode FocusNode => ShaclNode("focusNode");

        public static IUriNode Value => ShaclNode("value");

        public static IUriNode SourceShape => ShaclNode("sourceShape");

        public static IUriNode SourceConstraintComponent => ShaclNode("sourceConstraintComponent");

        public static IUriNode ResultSeverity => ShaclNode("resultSeverity");

        public static IUriNode Violation => ShaclNode("Violation");

        public static IUriNode ResultPath => ShaclNode("resultPath");

        public static IUriNode ResultMessage => ShaclNode("resultMessage");
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

        #endregion

        private static IUriNode ShaclNode(string name) => factory.CreateUriNode(UriFactory.Create($"{BaseUri}{name}"));
    }
}
