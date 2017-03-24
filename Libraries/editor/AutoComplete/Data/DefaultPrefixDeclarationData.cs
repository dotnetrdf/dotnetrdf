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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor.AutoComplete.Data
{
    /// <summary>
    /// Abstract completion data for default prefix declarations
    /// </summary>
    public abstract class BaseDefaultPrefixDeclarationData 
        : BaseCompletionData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="postfix">Postfix</param>
        public BaseDefaultPrefixDeclarationData(String prefix, String postfix)
            : base("<New Default Prefix Declaration>", prefix + ": <Enter Default Namespace URI here>" + postfix, "Inserts a new Default Prefix declaration", 100.0d) { }
    }

    /// <summary>
    /// Completion data for Turtle style default prefix declaration
    /// </summary>
    public class TurtleStyleDefaultPrefixDeclarationData
        : BaseDefaultPrefixDeclarationData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        public TurtleStyleDefaultPrefixDeclarationData()
            : base("@prefix ", ".") { }
    }

    /// <summary>
    /// Completion data for SPARQL style default prefix declaration
    /// </summary>
    public class SparqlStyleDefaultPrefixDeclarationData
        : BaseDefaultPrefixDeclarationData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        public SparqlStyleDefaultPrefixDeclarationData()
            : base("PREFIX ", String.Empty) { }
    }
}
