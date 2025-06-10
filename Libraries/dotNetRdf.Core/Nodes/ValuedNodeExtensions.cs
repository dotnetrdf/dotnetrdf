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

using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Nodes;

/// <summary>
/// Extension Methods related to valued nodes.
/// </summary>
public static class ValuedNodeExtensions
{
    /// <summary>
    /// Takes a <see cref="INode">INode</see> and converts it to a <see cref="IValuedNode">IValuedNode</see> if it is not already an instance that implements the interface.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <returns>Valued Node.</returns>
    public static IValuedNode AsValuedNode(this INode n)
    {
        if (n == null) return null;
        if (n is IValuedNode valuedNode) return valuedNode;

        switch (n.NodeType)
        {
            case NodeType.Blank:
                var b = (IBlankNode)n;
                return new BlankNode(b.InternalID);
            case NodeType.GraphLiteral:
                var glit = (IGraphLiteralNode)n;
                return new GraphLiteralNode(glit.SubGraph);
            case NodeType.Literal:
                var lit = (ILiteralNode)n;
                // Decide what kind of valued node to produce based on node datatype
                if (lit.DataType != null)
                {
                    var dt = lit.DataType.AbsoluteUri;
                    switch (dt)
                    {
                        case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                            if (lit.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                lit.Value.Equals("1", StringComparison.OrdinalIgnoreCase))
                            {
                                return new BooleanNode(true);
                            }

                            if (lit.Value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                                lit.Value.Equals("0", StringComparison.OrdinalIgnoreCase))
                            {
                                return new BooleanNode(false);
                            }

                            return new StringNode( lit.Value, lit.DataType);
                        case XmlSpecsHelper.XmlSchemaDataTypeByte:
                            // xsd:byte actually maps to SignedByte in .Net
                            sbyte sbVal;
                            if (sbyte.TryParse(lit.Value, out sbVal))
                            {
                                return new SignedByteNode(sbVal, lit.Value);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeDate:
                            DateTime date;
                            if (DateTime.TryParse(lit.Value, null, DateTimeStyles.AdjustToUniversal, out date))
                            {
                                return new DateNode(date, lit.Value);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                            DateTime dateTime;
                            if (DateTime.TryParse(lit.Value, null, DateTimeStyles.AdjustToUniversal, out dateTime))
                            {
                                return new DateTimeNode(dateTime, lit.Value);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration:
                        case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                            TimeSpan timeSpan;
                            try
                            {
                                timeSpan = XmlConvert.ToTimeSpan(lit.Value);
                                return new TimeSpanNode(timeSpan, lit.Value, lit.DataType);
                            }
                            catch
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeDecimal:
                            decimal dec;
                            if (decimal.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
                            {
                                return new DecimalNode(dec, lit.Value);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                            double dbl;
                            if (double.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                            {
                                return new DoubleNode(dbl, lit.Value);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                            float flt;
                            if (float.TryParse(lit.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out flt))
                            {
                                return new FloatNode(flt, lit.Value);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeInt:
                        case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                        case XmlSpecsHelper.XmlSchemaDataTypeLong:
                        case XmlSpecsHelper.XmlSchemaDataTypeShort:
                            long lng;
                            if (long.TryParse(lit.Value, out lng))
                            {
                                return new LongNode(lng, lit.Value, lit.DataType);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                        case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                            // Must be below zero
                            long neglng;
                            if (long.TryParse(lit.Value, out neglng) && neglng < 0)
                            {
                                return new LongNode(neglng, lit.Value, lit.DataType);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                        case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                            // Must be above zero
                            long poslng;
                            if (long.TryParse(lit.Value, out poslng) && poslng >= 0)
                            {
                                return new LongNode(poslng, lit.Value, lit.DataType);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                            // xsd:unsignedByte actually maps to Byte in .Net
                            byte byVal;
                            if (byte.TryParse(lit.Value, out byVal))
                            {
                                return new ByteNode(byVal, lit.Value);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                        case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                        case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                            // Must be unsigned
                            ulong ulng;
                            if (ulong.TryParse(lit.Value, out ulng))
                            {
                                return new UnsignedLongNode(ulng, lit.Value, lit.DataType);
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                        case RdfSpecsHelper.RdfLangString:
                            return !lit.Language.Equals(string.Empty)
                                ? new StringNode(lit.Value, lit.Language)
                                : new StringNode(lit.Value);
                        default:
                            if (NumericTypesHelper.IntegerDataTypes.Contains(dt))
                            {
                                long l;
                                if (long.TryParse(lit.Value, out l))
                                {
                                    return new LongNode(l, lit.Value, lit.DataType);
                                }
                                else
                                {
                                    return new StringNode(lit.Value, lit.DataType);
                                }
                            }
                            else
                            {
                                return new StringNode(lit.Value, lit.DataType);
                            }
                    }
                }
                else if (!lit.Language.Equals(string.Empty))
                {
                    return new StringNode(lit.Value, lit.Language);
                }
                else
                {
                    return new StringNode(lit.Value);
                }
            case NodeType.Uri:
                var u = (IUriNode)n;
                return new UriNode(u.Uri);
            case NodeType.Variable:
                var v = (IVariableNode)n;
                return new VariableNode(v.VariableName);
            case NodeType.Triple:
                var tn = (ITripleNode)n;
                return new TripleNode(new Triple(tn.Triple.Subject.AsValuedNode(),
                    tn.Triple.Predicate.AsValuedNode(), tn.Triple.Object.AsValuedNode()));
            default:
                throw new RdfQueryException("Cannot create a valued node for an unknown node type");
        }
    }

    /// <summary>
    /// Tries to get the result of calling <see cref="IValuedNode.AsBoolean()">AsBoolean()</see> on a node throwing an error if the node is null.
    /// </summary>
    /// <param name="n">Node.</param>
    /// <exception cref="RdfQueryException">Thrown if the input is null of the specific valued node cannot be cast to a boolean.</exception>
    /// <returns></returns>
    public static bool AsSafeBoolean(this IValuedNode n)
    {
        if (n == null) throw new RdfQueryException("Cannot cast a null to a boolean");
        return n.AsBoolean();
    }
}
