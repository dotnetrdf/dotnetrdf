/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this._vars.Add(var);
        }

        /// <summary>
        /// Creates a new List URIs Handler
        /// </summary>
        /// <param name="vars">Variables to build the list from</param>
        public ListUrisHandler(IEnumerable<String> vars)
        {
            foreach (String var in vars)
            {
                this._vars.Add(var);
            }
        }

        /// <summary>
        /// Gets the URIs
        /// </summary>
        public IEnumerable<Uri> Uris
        {
            get
            {
                return this._uris;
            }
        }

        /// <summary>
        /// Starts handling results
        /// </summary>
        protected override void StartResultsInternal()
        {
            this._uris = new List<Uri>();
        }

        /// <summary>
        /// Handles boolean results
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            //Nothing to do
        }

        /// <summary>
        /// Handles variable declarations
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            //Nothing to do
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
                if (this._vars.Contains(var) && result.HasValue(var))
                {
                    INode value = result[var];
                    if (value.NodeType == NodeType.Uri)
                    {
                        this._uris.Add(((IUriNode)value).Uri);
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
            this._vars.Add(var);
        }

        /// <summary>
        /// Creates a new List Strings handler
        /// </summary>
        /// <param name="vars">Variables to build the list from</param>
        public ListStringsHandler(IEnumerable<String> vars)
        {
            foreach (String var in vars)
            {
                this._vars.Add(var);
            }
        }

        /// <summary>
        /// Gets the Strings
        /// </summary>
        public IEnumerable<String> Strings
        {
            get
            {
                return this._values;
            }
        }

        /// <summary>
        /// Starts handling results
        /// </summary>
        protected override void StartResultsInternal()
        {
            this._values = new List<string>();
        }

        /// <summary>
        /// Handles boolean results
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            //Nothing to do
        }

        /// <summary>
        /// Handles variable declarations
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            //Nothing to do
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
                if (this._vars.Contains(var) && result.HasValue(var))
                {
                    INode value = result[var];
                    if (value.NodeType == NodeType.Literal)
                    {
                        this._values.Add(((ILiteralNode)value).Value);
                    }
                }
            }
            return true;
        }
    }
}
