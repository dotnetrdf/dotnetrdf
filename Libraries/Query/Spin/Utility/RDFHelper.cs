using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Spin.OntologyHelpers;
using VDS.RDF.Query.Spin.Model;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Paths;

namespace VDS.RDF.Query.Spin.Utility
{
    public static class RDFHelper
    {

        public static String UI_TEMPVAR_PREFIX = "spinVar_" + Guid.NewGuid().ToString().Substring(0, 4) + "_";
        private static long _tempVariablesCount = 0;

        private readonly static FastNodeComparer _nodeComparer = new FastNodeComparer();
        internal readonly static UriComparer uriComparer = new UriComparer();
        internal readonly static IEqualityComparer<Triple> tripleEqualityComparer = new TripleEqualityComparer();
        internal readonly static NodeFactory nodeFactory = new NodeFactory();

        #region UriComparison shortcuts

        internal static bool SameTerm(Uri uri1, Uri uri2)
        {
            return uriComparer.Equals(uri1, uri2);
        }

        internal static bool SameTerm(Uri uri1, INode uri2)
        {
            if (uri1 == null) return false;
            if (uri2 is IResource) uri2 = ((IResource)uri2).AsNode();
            IUriNode node1 = nodeFactory.CreateUriNode(uri1);
            return _nodeComparer.Equals(node1, uri2);
        }

        internal static bool SameTerm(INode uri1, Uri uri2)
        {
            if (uri2 == null) return false;
            if (uri1 is IResource) uri1 = ((IResource)uri1).AsNode();
            IUriNode node2 = nodeFactory.CreateUriNode(uri2);
            return _nodeComparer.Equals(uri1, node2);
        }

        internal static bool SameTerm(INode node1, INode node2)
        {
            if (node1 is IResource) node1 = ((IResource)node1).AsNode();
            if (node2 is IResource) node2 = ((IResource)node2).AsNode();
            return _nodeComparer.Equals(node1, node2);
        }

        #endregion

        #region NodeFactory shortcuts

        // TODO return a couple of direct and negated Uris
        internal static List<HashSet<Uri>> GetPropertyPathItems(ISparqlPath path)
        {
            List<HashSet<Uri>> predicates = new List<HashSet<Uri>>() { new HashSet<Uri>(uriComparer), new HashSet<Uri>(uriComparer) };
             if (path is AlternativePath)
             {
                 AlternativePath altPath = (AlternativePath)path;
                 List<HashSet<Uri>> results = GetPropertyPathItems(altPath.LhsPath);
                 predicates[0].UnionWith(results[0]);
                 predicates[1].UnionWith(results[1]);
                 results = GetPropertyPathItems(altPath.RhsPath);
                 predicates[0].UnionWith(results[0]);
                 predicates[1].UnionWith(results[1]);
             }
             else if (path is NegatedSet) {
                 NegatedSet negSet = (NegatedSet)path;
                 predicates[1].UnionWith(negSet.Properties.Select(ppty => ((IUriNode)ppty.Predicate).Uri).Union(negSet.InverseProperties.Select(ppty => ((IUriNode)ppty.Predicate).Uri)));
             }
             else if (path is SequencePath) {
                 SequencePath stepPath = (SequencePath)path;
                 List<HashSet<Uri>> results = GetPropertyPathItems(stepPath.LhsPath);
                 predicates[0].UnionWith(results[0]);
                 predicates[1].UnionWith(results[1]);
                 results = GetPropertyPathItems(stepPath.RhsPath);
                 predicates[0].UnionWith(results[0]);
                 predicates[1].UnionWith(results[1]);
             }
             else if (path is Property)
             {
                 Property singlePath = (Property)path;
                 predicates[0].Add(((IUriNode)singlePath.Predicate).Uri);
             }
             else if (path is BaseUnaryPath)
             {
                 BaseUnaryPath unaryPath = (BaseUnaryPath)path;
                 List<HashSet<Uri>> results = GetPropertyPathItems(unaryPath.Path);
                 predicates[0].UnionWith(results[0]);
                 predicates[1].UnionWith(results[1]);
             }
             return predicates;
         }

        public static INode GetNode(PatternItem item)
        {
            if (item is VariablePattern)
            {
                return CreateVariableNode(item.VariableName);
            }
            else if (item is NodeMatchPattern)
            {
                return ((NodeMatchPattern)item).Node;
            }
            else if (item is BlankNodePattern)
            {
                return CreateVariableNode("_bNodeVar_"+ item.VariableName.Replace("_:", ""));
            }
            return null;
        }

        public static IUriNode CreateUriNode(Uri uri)
        {
            return nodeFactory.CreateUriNode(uri);
        }

        public static IVariableNode CreateVariableNode(String name)
        {
            return nodeFactory.CreateVariableNode(name);
        }

        public static IVariableNode CreateTempVariableNode()
        {
            return nodeFactory.CreateVariableNode(UI_TEMPVAR_PREFIX + (_tempVariablesCount++).ToString());
        }

        public static IBlankNode CreateBlankNode()
        {
            return nodeFactory.CreateBlankNode();
        }

        public static IBlankNode CreateBlankNode(String nodeId)
        {
            return nodeFactory.CreateBlankNode(nodeId.Replace("_:", ""));
        }

        public static ILiteralNode CreateLiteralNode(String literal)
        {
            return nodeFactory.CreateLiteralNode(literal);
        }

        public static ILiteralNode CreateLiteralNode(String literal, String langspec)
        {
            return nodeFactory.CreateLiteralNode(literal, langspec);
        }

        public static ILiteralNode CreateLiteralNode(String literal, Uri datatype)
        {
            return nodeFactory.CreateLiteralNode(literal, datatype);
        }

        internal static ILiteralNode FALSE = CreateLiteralNode("false", UriFactory.Create(NamespaceMapper.XMLSCHEMA + "boolean"));
        internal static ILiteralNode TRUE = CreateLiteralNode("true", UriFactory.Create(NamespaceMapper.XMLSCHEMA + "boolean"));

        internal static HashSet<Uri> numericDatatypeURIs = new HashSet<Uri>(uriComparer);

        internal static HashSet<Uri> otherDatatypeURIs = new HashSet<Uri>(uriComparer);

        private static bool _isFullyInitialized = false;
        private static void Initialize()
        {
            if (_isFullyInitialized) return;
            numericDatatypeURIs.Add(XSD.DatatypeDecimal.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeDuration.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGDay.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGMonth.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGMonthDay.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGYear.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeGYearMonth.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeNegativeInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeNonNegativeInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeNonPositiveInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypePositiveInteger.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedByte.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedInt.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedLong.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeUnsignedShort.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeByte.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeDouble.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeFloat.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeInt.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeLong.Uri);
            numericDatatypeURIs.Add(XSD.DatatypeShort.Uri);

            otherDatatypeURIs.Add(XSD.DatatypeAnySimpleType.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeAnyURI.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeBase64Binary.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeDate.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeDateTime.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeENTITY.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeHexBinary.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeID.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeIDREF.Uri);
            otherDatatypeURIs.Add(XSD.PropertyLanguage.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeName.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNCName.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNMTOKEN.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNormalizedString.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeNOTATION.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeQName.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeTime.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeToken.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeBoolean.Uri);
            otherDatatypeURIs.Add(XSD.DatatypeString.Uri);
            otherDatatypeURIs.Add(RDF.ClassXMLLiteral.Uri);

            _isFullyInitialized = true;
        }

        internal static ILiteralNode CreateInteger(int value)
        {
            return CreateLiteralNode("" + value, XSD.DatatypeInteger.Uri);
        }


        /**
         * Gets a List of all datatype URIs.
         * @return a List the datatype URIs
         */
        internal static List<Uri> GetDatatypeURIs()
        {
            Initialize();
            List<Uri> list = new List<Uri>();
            list.AddRange(otherDatatypeURIs);
            list.AddRange(numericDatatypeURIs);
            list.Add(RDF.ClassPlainLiteral.Uri);
            return list;
        }


        /**
         * Checks if a given URI is a numeric datatype URI.
         * @param datatypeURI  the URI of the datatype to test
         * @return true if so
         */
        internal static bool IsNumeric(Uri datatypeURI)
        {
            Initialize();
            return numericDatatypeURIs.Contains(datatypeURI);
        }


        /**
         * Checks if a given INode represents a system XSD datatype such as xsd:int.
         * Note: this will not return true on user-defined datatypes or rdfs:Literal.
         * @param node  the node to test
         * @return true if node is a datatype
         */
        internal static bool IsSystemDatatype(INode node)
        {
            Initialize();
            if (node is IUriNode)
            {
                return IsNumeric(((IUriNode)node).Uri) || otherDatatypeURIs.Contains(((IUriNode)node).Uri);
            }
            else
            {
                return false;
            }
        }

        #endregion

        // TODO complete Literal helper functions
        #region Literal nodes utility

        public static int? AsInteger(object value, int? defaultValue = null)
        {
            if (value == null) return defaultValue;
            if (value is ILiteralNode) value = ((ILiteralNode)value).AsValuedNode();
            if (!(value is IValuedNode)) return defaultValue;
            return (int)((IValuedNode)value).AsInteger();
        }

        public static long? AsLong(object value, long? defaultValue = null)
        {
            if (value == null) return defaultValue;
            if (value is ILiteralNode) value = ((ILiteralNode)value).AsValuedNode();
            if (!(value is IValuedNode)) return defaultValue;
            return ((IValuedNode)value).AsInteger();
        }

        public static String AsString(object value, String defaultValue = null)
        {
            if (value == null) return defaultValue;
            if (value is ILiteralNode) value = ((ILiteralNode)value).AsValuedNode();
            if (!(value is IValuedNode)) return defaultValue;
            return ((IValuedNode)value).AsString();
        }

        public static bool? AsBoolean(object value, bool? defaultValue=null) {
            if (value==null) return defaultValue;
            if (value is ILiteralNode) value = ((ILiteralNode)value).AsValuedNode();
            if (!(value is IValuedNode)) return defaultValue;
            return ((IValuedNode)value).AsBoolean();
        }

        public static float? AsFloat(object value, float? defaultValue = null)
        {
            if (value == null || !(value is IValuedNode)) return defaultValue;
            if (value is ILiteralNode) value = ((ILiteralNode)value).AsValuedNode();
            if (!(value is IValuedNode)) return defaultValue;
            return ((IValuedNode)value).AsFloat();
        }

        #endregion 
    }
}
