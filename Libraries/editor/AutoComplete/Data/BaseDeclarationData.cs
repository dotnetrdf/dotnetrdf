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
    /// Completion data for Base URI declarations
    /// </summary>
    public abstract class BaseDeclarationData 
        : BaseCompletionData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        /// <param name="prefix">Prefix text</param>
        /// <param name="postfix">Postfix text</param>
        public BaseDeclarationData(String prefix, String postfix)
            : base("<New Base URI Declaration>", prefix + "<Enter Base URI here>" + postfix, "Inserts a new Base URI declaration") { }
    }

    /// <summary>
    /// Completion data for Turtle Style Base URI declarations
    /// </summary>
    public class TurtleStyleBaseDeclarationData 
        : BaseDeclarationData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        public TurtleStyleBaseDeclarationData()
            : base("@base ", ".") { }
    }

    /// <summary>
    /// Completion data for SPARQL style Base URI declarations
    /// </summary>
    public class SparqlStyleBaseDeclarationData 
        : BaseDeclarationData
    {
        /// <summary>
        /// Creates new completion data
        /// </summary>
        public SparqlStyleBaseDeclarationData()
            : base("BASE", String.Empty) { }
    }
}
