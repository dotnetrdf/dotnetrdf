/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Globalization;
using System.Linq;
using System.Xml;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Extension Methods related to valued nodes
    /// </summary>
    public static class ValuedNodeExtensions
    {
        /// <summary>
        /// Takes a <see cref="INode">INode</see> and converts it to a <see cref="IValuedNode">IValuedNode</see> if it is not already an instance that implements the interface
        /// </summary>
        /// <param name="n">Node</param>
        /// <returns>Valued Node</returns>
        public static IValuedNode AsValuedNode(this INode n)
        {
            if (n == null) return null;
            if (n is IValuedNode) return (IValuedNode)n;

            switch (n.NodeType)
            {
                case NodeType.Blank:
                    return new BlankNode(n.AnonID);
                case NodeType.GraphLiteral:
                    return new GraphLiteralNode(n.SubGraph);
                case NodeType.Literal:
                    //Decide what kind of valued node to produce based on node datatype
                    if (n.HasLanguage)
                    {
                        return new StringNode(n.Value, n.Language);
                    }
                    else if (n.HasDataType)
                    {
                        String dt = n.DataType.AbsoluteUri;
                        switch (dt)
                        {
                            case XmlSpecsHelper.XmlSchemaDataTypeBoolean:
                                bool bVal;
                                if (Boolean.TryParse(n.Value, out bVal))
                                {
                                    return new BooleanNode(bVal);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeByte:
                                //xsd:byte actually maps to SignedByte in .Net
                                sbyte sbVal;
                                if (sbyte.TryParse(n.Value, out sbVal))
                                {
                                    return new SignedByteNode(sbVal, n.Value);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDate:
                                DateTime date;
                                if (DateTime.TryParse(n.Value, null, DateTimeStyles.AdjustToUniversal, out date))
                                {
                                    return new DateNode(date, n.Value);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDateTime:
                                DateTime dateTime;
                                if (DateTime.TryParse(n.Value, null, DateTimeStyles.AdjustToUniversal, out dateTime))
                                {
                                    return new DateTimeNode(dateTime, n.Value);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration:
                            case XmlSpecsHelper.XmlSchemaDataTypeDuration:
                                TimeSpan timeSpan;
                                try
                                {
                                    timeSpan = XmlConvert.ToTimeSpan(n.Value);
                                    return new TimeSpanNode(timeSpan, n.Value, n.DataType);
                                }
                                catch
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDecimal:
                                Decimal dec;
                                if (Decimal.TryParse(n.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
                                {
                                    return new DecimalNode(dec, n.Value);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeDouble:
                                Double dbl;
                                if (Double.TryParse(n.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                                {
                                    return new DoubleNode(dbl, n.Value);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeFloat:
                                Single flt;
                                if (Single.TryParse(n.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out flt))
                                {
                                    return new FloatNode(flt, n.Value);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeInt:
                            case XmlSpecsHelper.XmlSchemaDataTypeInteger:
                            case XmlSpecsHelper.XmlSchemaDataTypeLong:
                            case XmlSpecsHelper.XmlSchemaDataTypeShort:
                                long lng;
                                if (Int64.TryParse(n.Value, out lng))
                                {
                                    return new LongNode(lng, n.Value, n.DataType);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeNegativeInteger:
                            case XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger:
                                //Must be below zero
                                long neglng;
                                if (Int64.TryParse(n.Value, out neglng) && neglng < 0)
                                {
                                    return new LongNode(neglng, n.Value, n.DataType);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger:
                            case XmlSpecsHelper.XmlSchemaDataTypePositiveInteger:
                                //Must be above zero
                                long poslng;
                                if (Int64.TryParse(n.Value, out poslng) && poslng >= 0)
                                {
                                    return new LongNode(poslng, n.Value, n.DataType);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte:
                                //xsd:unsignedByte actually maps to Byte in .Net
                                byte byVal;
                                if (byte.TryParse(n.Value, out byVal))
                                {
                                    return new ByteNode(byVal, n.Value);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt:
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedLong:
                            case XmlSpecsHelper.XmlSchemaDataTypeUnsignedShort:
                                //Must be unsigned
                                ulong ulng;
                                if (UInt64.TryParse(n.Value, out ulng))
                                {
                                    return new UnsignedLongNode(ulng, n.Value, n.DataType);
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                            default:
                                if (XmlSpecsHelper.IntegerDataTypes.Contains(dt))
                                {
                                    long l;
                                    if (Int64.TryParse(n.Value, out l))
                                    {
                                        return new LongNode(l, n.Value, n.DataType);
                                    }
                                    else
                                    {
                                        return new StringNode(n.Value, n.DataType);
                                    }
                                }
                                else
                                {
                                    return new StringNode(n.Value, n.DataType);
                                }
                        }
                    }
                    else
                    {
                        return new StringNode(n.Value);
                    }
                case NodeType.Uri:
                    return new UriNode(n.Uri);
                case NodeType.Variable:
                    return new VariableNode(n.VariableName);
                default:
                    throw new NodeValueException("Cannot create a valued node for an unknown node type");
            }
        }

        /// <summary>
        /// Tries to get the result of calling <see cref="IValuedNode.AsBoolean()">AsBoolean()</see> on a node throwing an error if the node is null
        /// </summary>
        /// <param name="n">Node</param>
        /// <exception cref="NodeValueException">Thrown if the input is null of the specific valued node cannot be cast to a boolean</exception>
        /// <returns></returns>
        public static bool AsSafeBoolean(this IValuedNode n)
        {
            if (n == null) throw new NodeValueException("Cannot cast a null to a boolean");
            return n.AsBoolean();
        }
    }
}
