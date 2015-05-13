/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Spin.Model;

namespace VDS.RDF.Query.Spin.Utility
{
    public static class RDFHelper
    {
        public static String NON_CONFLICT_PREFIX = Guid.NewGuid().ToString().Substring(0, 4) + "_";
        private static long _tempVariablesCount = 0;

        private readonly static FastNodeComparer _nodeComparer = new FastNodeComparer();
        internal readonly static UriComparer uriComparer = new UriComparer();
        internal readonly static IEqualityComparer<Triple> tripleEqualityComparer = new TripleEqualityComparer();
        internal readonly static NodeFactory nodeFactory = new NodeFactory();

        #region Aliasing utility

        public static String NewVarName(String baseName = "")
        {
            return NON_CONFLICT_PREFIX + baseName + (_tempVariablesCount++).ToString();
        }

        #endregion Aliasing utility

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

        #endregion UriComparison shortcuts

        #region NodeFactory shortcuts

        //// TODO check whether we still need this
        //internal static List<HashSet<Uri>> GetPropertyPathItems(ISparqlPath path)
        //{
        //    List<HashSet<Uri>> predicates = new List<HashSet<Uri>>() { new HashSet<Uri>(uriComparer), new HashSet<Uri>(uriComparer) };
        //    if (path is AlternativePath)
        //    {
        //        AlternativePath altPath = (AlternativePath)path;
        //        List<HashSet<Uri>> results = GetPropertyPathItems(altPath.LhsPath);
        //        predicates[0].UnionWith(results[0]);
        //        predicates[1].UnionWith(results[1]);
        //        results = GetPropertyPathItems(altPath.RhsPath);
        //        predicates[0].UnionWith(results[0]);
        //        predicates[1].UnionWith(results[1]);
        //    }
        //    else if (path is NegatedSet)
        //    {
        //        NegatedSet negSet = (NegatedSet)path;
        //        predicates[1].UnionWith(negSet.Properties.Select(ppty => ((IUriNode)ppty.Predicate).Uri).Union(negSet.InverseProperties.Select(ppty => ((IUriNode)ppty.Predicate).Uri)));
        //    }
        //    else if (path is SequencePath)
        //    {
        //        SequencePath stepPath = (SequencePath)path;
        //        List<HashSet<Uri>> results = GetPropertyPathItems(stepPath.LhsPath);
        //        predicates[0].UnionWith(results[0]);
        //        predicates[1].UnionWith(results[1]);
        //        results = GetPropertyPathItems(stepPath.RhsPath);
        //        predicates[0].UnionWith(results[0]);
        //        predicates[1].UnionWith(results[1]);
        //    }
        //    else if (path is Property)
        //    {
        //        Property singlePath = (Property)path;
        //        predicates[0].Add(((IUriNode)singlePath.Predicate).Uri);
        //    }
        //    else if (path is BaseUnaryPath)
        //    {
        //        BaseUnaryPath unaryPath = (BaseUnaryPath)path;
        //        List<HashSet<Uri>> results = GetPropertyPathItems(unaryPath.Path);
        //        predicates[0].UnionWith(results[0]);
        //        predicates[1].UnionWith(results[1]);
        //    }
        //    return predicates;
        //}

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
                return CreateVariableNode("_bNodeVar_" + item.VariableName.Replace("_:", ""));
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

        public static IVariableNode CreateTempVariableNode(String name = "")
        {
            return nodeFactory.CreateVariableNode(NewVarName(name));
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

        #endregion NodeFactory shortcuts

        // TODO complete Literal helper functions

        #region Literal nodes utility

        /// <summary>
        /// Takes a <see cref="INode">INode</see> and converts it to a <see cref="IValuedNode">IValuedNode</see> if it is not already an instance that implements the interface
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns>Valued Node</returns>
        public static Object AsPrimitive(this INode n)
        {
            if (n == null) return null;

            switch (n.NodeType)
            {
                case NodeType.Blank:
                    IBlankNode b = (IBlankNode)n;
                    return b;
                case NodeType.GraphLiteral:
                    IGraphLiteralNode glit = (IGraphLiteralNode)n;
                    return glit.SubGraph.BaseUri;
                case NodeType.Literal:
                    ILiteralNode lit = (ILiteralNode)n;
                    //Decide what kind of valued node to produce based on node datatype
                    if (lit.DataType != null)
                    {
                        String dt = lit.DataType.AbsoluteUri;
                        switch (dt)
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                                bool bVal;
                                if (Boolean.TryParse(lit.Value, out bVal))
                                {
                                    return bVal;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeByte:
                                //xsd:byte actually maps to SignedByte in .Net
                                sbyte sbVal;
                                if (sbyte.TryParse(lit.Value, out sbVal))
                                {
                                    return sbVal;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                DateTime date;
                                if (DateTime.TryParse(lit.Value, null, DateTimeStyles.AdjustToUniversal, out date))
                                {
                                    return date;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                DateTime dateTime;
                                if (DateTime.TryParse(lit.Value, null, DateTimeStyles.AdjustToUniversal, out dateTime))
                                {
                                    return dateTime;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration:
                            case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                                TimeSpan timeSpan;
                                try
                                {
                                    timeSpan = XmlConvert.ToTimeSpan(lit.Value);
                                    return timeSpan;
                                }
                                catch
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDecimal:
                                Decimal dec;
                                if (Decimal.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
                                {
                                    return dec;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                                Double dbl;
                                if (Double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                                {
                                    return dbl;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                                Single flt;
                                if (Single.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out flt))
                                {
                                    return flt;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeInt:
                            case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                            case XmlSpecsHelper.XmlSchemaDataTypeLong:
                            case XmlSpecsHelper.XmlSchemaDataTypeShort:
                                long lng;
                                if (Int64.TryParse(lit.Value, out lng))
                                {
                                    return lng;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                            case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                                //Must be below zero
                                long neglng;
                                if (Int64.TryParse(lit.Value, out neglng) && neglng < 0)
                                {
                                    return neglng;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                            case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                                //Must be above zero
                                long poslng;
                                if (Int64.TryParse(lit.Value, out poslng) && poslng >= 0)
                                {
                                    return poslng;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                                //xsd:unsignedByte actually maps to Byte in .Net
                                byte byVal;
                                if (byte.TryParse(lit.Value, out byVal))
                                {
                                    return byVal;
                                }
                                else
                                {
                                    return null;
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                                //Must be unsigned
                                ulong ulng;
                                if (UInt64.TryParse(lit.Value, out ulng))
                                {
                                    return ulng;
                                }
                                else
                                {
                                    return null;
                                }
                            default:
                                if (SparqlSpecsHelper.IntegerDataTypes.Contains(dt))
                                {
                                    long l;
                                    if (Int64.TryParse(lit.Value, out l))
                                    {
                                        return l;
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    return null;
                                }
                        }
                    }
                    else
                    {
                        return lit.Value;
                    }
                case NodeType.Uri:
                    IUriNode u = (IUriNode)n;
                    return u.Uri;
                case NodeType.Variable:
                    IVariableNode v = (IVariableNode)n;
                    return null;
                default:
                    throw new RdfQueryException("Cannot create a native value node for an unknown node type");
            }
        }

        #endregion Literal nodes utility
    }
}