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

using System;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Interface for Valued Nodes
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface extends the basic <see cref="INode">INode</see> interface with methods related to turning the lexical value into a strongly typed .Net value.  It is intended primarily for use within SPARQL expression evaluation where we need to do a lot of value conversions and currently waste a lot of effort (and thus performance) doing that.
    /// </para>
    /// </remarks>
    public interface IValuedNode 
        : INode
    {
        /// <summary>
        /// Gets the String value of the Node
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is distinct from <strong>ToString()</strong> because that method will typically include additional information like language specifier/datatype as appropriate whereas this method is used to produce a string as would be produced by applying the <strong>STR()</strong> function from SPARQL
        /// </remarks>
        String AsString();

        /// <summary>
        /// Gets the Long value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Long</exception>
        long AsInteger();

        /// <summary>
        /// Gets the Decimal value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Decimal</exception>
        decimal AsDecimal();

        /// <summary>
        /// Gets the Float value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Float</exception>
        float AsFloat();

        /// <summary>
        /// Gets the Double value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Double</exception>
        double AsDouble();

        /// <summary>
        /// Gets the Boolean value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Boolean</exception>
        bool AsBoolean();

        /// <summary>
        /// Gets the Date Time value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Date Time</exception>
        DateTime AsDateTime();

        /// <summary>
        /// Gets the Date Time Offset value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Date Time Offset</exception>
        DateTimeOffset AsDateTimeOffset();

        /// <summary>
        /// Gets the Time Span value of the Node
        /// </summary>
        /// <returns></returns>
        TimeSpan AsTimeSpan();

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        /// <remarks>
        /// Either String.Empty if no type or the string form of the type URI
        /// </remarks>
        String EffectiveType
        {
            get;
        }

        /// <summary>
        /// Gets the Numeric Type of the Node
        /// </summary>
        SparqlNumericType NumericType
        {
            get;
        }
    }
}
