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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    /// <summary>
    /// Auto-completer implementation for SPARQL Update
    /// </summary>
    /// <typeparam name="T">Control Type</typeparam>
    public class SparqlUpdateAutoCompleter<T>
        : SparqlAutoCompleter<T>
    {
        /// <summary>
        /// Creates a new auto-completer
        /// </summary>
        /// <param name="editor">Text Editor</param>
        public SparqlUpdateAutoCompleter(ITextEditorAdaptor<T> editor)
            : base(editor, SparqlQuerySyntax.Sparql_1_1)
        {
            foreach (String keyword in SparqlSpecsHelper.SparqlUpdate11Keywords)
            {
                this._keywords.Add(new KeywordData(keyword));
                this._keywords.Add(new KeywordData(keyword.ToLower()));
            }
        }
    }
}
