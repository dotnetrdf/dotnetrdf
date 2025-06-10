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
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes;

/// <summary>
/// Valued Node representing a Time Span value.
/// </summary>
public class TimeSpanNode
    : LiteralNode, IValuedNode
{
    private TimeSpan _value;

    /// <summary>
    /// Creates a new Time span node.
    /// </summary>
    /// <param name="value">Time Span.</param>
    public TimeSpanNode(TimeSpan value)
        : this(value, XmlConvert.ToString(value), UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration)) { }

    /// <summary>
    /// Creates a new Time span node. 
    /// </summary>
    /// <param name="value">Time Span.</param>
    /// <param name="lexicalValue">Lexical value.</param>
    public TimeSpanNode(TimeSpan value, string lexicalValue)
        : this(value, lexicalValue, UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration)) { }

    /// <summary>
    /// Creates a new Time span node.
    /// </summary>
    /// <param name="value">Time Span.</param>
    /// <param name="lexicalValue">Lexical value.</param>
    /// <param name="dtUri">Data type URI.</param>
    /// <param name="normalizeLiteralValue">Whether to perform unicode normalization on <paramref name="lexicalValue"/>.</param>
    public TimeSpanNode(TimeSpan value, string lexicalValue, Uri dtUri, bool normalizeLiteralValue = false)
        : base(lexicalValue, dtUri, normalizeLiteralValue)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the date time value as a string.
    /// </summary>
    /// <returns></returns>
    public string AsString()
    {
        return Value;
    }

    /// <summary>
    /// Throws an error as date times cannot be converted to integers.
    /// </summary>
    /// <returns></returns>
    public long AsInteger()
    {
        throw new RdfQueryException("Cannot convert Time Spans to other types");
    }

    /// <summary>
    /// Throws an error as date times cannot be converted to decimals.
    /// </summary>
    /// <returns></returns>
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Cannot convert Time Spans to other types");
    }

    /// <summary>
    /// Throws an error as date times cannot be converted to floats.
    /// </summary>
    /// <returns></returns>
    public float AsFloat()
    {
        throw new RdfQueryException("Cannot convert Time Spans to other types");
    }

    /// <summary>
    /// Throws an error as date times cannot be converted to doubles.
    /// </summary>
    /// <returns></returns>
    public double AsDouble()
    {
        throw new RdfQueryException("Cannot convert Time Spans to other types");
    }

    /// <summary>
    /// Throws an error as date times cannot be converted to booleans.
    /// </summary>
    /// <returns></returns>
    public bool AsBoolean()
    {
        throw new RdfQueryException("Cannot convert Time Spans to other types");
    }

    /// <summary>
    /// Gets the date time value of the node.
    /// </summary>
    /// <returns></returns>
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Cannot convert Time Spans to other types");
    }

    /// <summary>
    /// Gets the date time value of the node.
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset AsDateTimeOffset()
    {
        throw new RdfQueryException("Cannot convert Time Spans to other types");
    }

    /// <summary>
    /// Gets the time span value of the node.
    /// </summary>
    /// <returns></returns>
    public TimeSpan AsTimeSpan()
    {
        return _value;
    }

    /// <summary>
    /// Gets the URI of the datatype this valued node represents as a String.
    /// </summary>
    public string EffectiveType
    {
        get
        {
            return DataType.AbsoluteUri;
        }
    }

    /// <summary>
    /// Gets the numeric type of the node.
    /// </summary>
    public SparqlNumericType NumericType
    {
        get
        {
            return SparqlNumericType.NaN;
        }
    }
}
