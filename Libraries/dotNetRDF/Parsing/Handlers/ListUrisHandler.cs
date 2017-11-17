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
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A Results Handler which extracts URIs from one/more variables in a Result Set
    /// </summary>
    public class ListUrisHandler 
        : BaseResultsHandler
    {
        private List<Uri> _uris;
        private HashSet<String> _vars = new HashSet<String>();

        /// <summary>
        /// Creates a new List URIs Handler
        /// </summary>
        /// <param name="var">Variable to build the list from</param>
        public ListUrisHandler(String var)
        {
            _vars.Add(var);
        }

        /// <summary>
        /// Creates a new List URIs Handler
        /// </summary>
        /// <param name="vars">Variables to build the list from</param>
        public ListUrisHandler(IEnumerable<String> vars)
        {
            foreach (String var in vars)
            {
                _vars.Add(var);
            }
        }

        /// <summary>
        /// Gets the URIs
        /// </summary>
        public IEnumerable<Uri> Uris
        {
            get
            {
                return _uris;
            }
        }

        /// <summary>
        /// Starts handling results
        /// </summary>
        protected override void StartResultsInternal()
        {
            _uris = new List<Uri>();
        }

        /// <summary>
        /// Handles boolean results
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            // Nothing to do
        }

        /// <summary>
        /// Handles variable declarations
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            // Nothing to do
            return true;
        }

        /// <summary>
        /// Handles results by extracting any URI values from the relevant variables
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            foreach (String var in result.Variables)
            {
                if (_vars.Contains(var) && result.HasValue(var))
                {
                    INode value = result[var];
                    if (value.NodeType == NodeType.Uri)
                    {
                        _uris.Add(((IUriNode)value).Uri);
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// A Results Handler which extracts Literals from one/more variables in a Result Set
    /// </summary>
    public class ListStringsHandler
        : BaseResultsHandler
    {
        private List<String> _values;
        private HashSet<String> _vars = new HashSet<String>();

        /// <summary>
        /// Creates a new List Strings handler
        /// </summary>
        /// <param name="var">Variable to build the list from</param>
        public ListStringsHandler(String var)
        {
            _vars.Add(var);
        }

        /// <summary>
        /// Creates a new List Strings handler
        /// </summary>
        /// <param name="vars">Variables to build the list from</param>
        public ListStringsHandler(IEnumerable<String> vars)
        {
            foreach (String var in vars)
            {
                _vars.Add(var);
            }
        }

        /// <summary>
        /// Gets the Strings
        /// </summary>
        public IEnumerable<String> Strings
        {
            get
            {
                return _values;
            }
        }

        /// <summary>
        /// Starts handling results
        /// </summary>
        protected override void StartResultsInternal()
        {
            _values = new List<string>();
        }

        /// <summary>
        /// Handles boolean results
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            // Nothing to do
        }

        /// <summary>
        /// Handles variable declarations
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            // Nothing to do
            return true;
        }

        /// <summary>
        /// Handles results by extracting strings from relevant variables
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(SparqlResult result)
        {
            foreach (String var in result.Variables)
            {
                if (_vars.Contains(var) && result.HasValue(var))
                {
                    INode value = result[var];
                    if (value.NodeType == NodeType.Literal)
                    {
                        _values.Add(((ILiteralNode)value).Value);
                    }
                }
            }
            return true;
        }
    }
}
