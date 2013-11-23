/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Specifications;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued Node representing a Date value
    /// </summary>
    public class DateNode
        : DateTimeNode
    {
        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateNode(DateTimeOffset value)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateNode(DateTimeOffset value, String lexicalValue)
            : base(value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        public DateNode(DateTime value)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }

        /// <summary>
        /// Creates a new Date valued node
        /// </summary>
        /// <param name="value">Date Time value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public DateNode(DateTime value, String lexicalValue)
            : base(value, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDate)) { }
    }
}