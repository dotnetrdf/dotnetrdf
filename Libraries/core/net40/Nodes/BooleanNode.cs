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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued Node representing boolean values
    /// </summary>
    public class BooleanNode
        : LiteralNode, IValuedNode
    {
        private bool _value;

        /// <summary>
        /// Creates a new boolean valued node
        /// </summary>
        /// <param name="g">Graph the node belong to</param>
        /// <param name="value">Boolean Value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public BooleanNode(bool value, String lexicalValue)
            : base(lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
        {
            this._value = value;
        }

        /// <summary>
        /// Creates a new boolean valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Boolean Value</param>
        public BooleanNode(bool value)
            : this(value, value.ToString().ToLower()) { }

        /// <summary>
        /// Gets the string value of the boolean
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to integers
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new NodeValueException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to decimals
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new NodeValueException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to floats
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new NodeValueException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to doubles
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new NodeValueException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Gets the boolean value
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            return this._value;
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to date times
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            throw new NodeValueException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to date times
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new NodeValueException("Cannot case Boolean to other types");
        }
        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public String EffectiveType
        {
            get
            {
                return XmlSpecsHelper.XmlSchemaDataTypeBoolean;
            }
        }


        /// <summary>
        /// Gets the numeric type of the node
        /// </summary>
        public SparqlNumericType NumericType
        {
            get 
            { 
                return SparqlNumericType.NaN; 
            }
        }
    }
}
