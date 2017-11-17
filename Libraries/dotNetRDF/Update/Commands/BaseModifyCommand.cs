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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Abstract Base class for classes that represent SPARQL Update INSERT, DELETE and INSERT/DELETE commands
    /// </summary>
    public abstract class BaseModificationCommand
        : SparqlUpdateCommand
    {
        /// <summary>
        /// URI from the WITH statement
        /// </summary>
        protected Uri _graphUri;
        /// <summary>
        /// URIs for the USING clauses
        /// </summary>
        protected List<Uri> _usingUris;
        /// <summary>
        /// URIS for the USING NAMED clauses
        /// </summary>
        protected List<Uri> _usingNamedUris;

        /// <summary>
        /// Creates a new Base Modification Command
        /// </summary>
        /// <param name="type">Update Command Type</param>
        public BaseModificationCommand(SparqlUpdateCommandType type)
            : base(type) { }

        /// <summary>
        /// Gets the URIs specified in USING clauses
        /// </summary>
        public IEnumerable<Uri> UsingUris
        {
            get
            {
                if (_usingUris == null)
                {
                    return Enumerable.Empty<Uri>();
                }
                else
                {
                    return _usingUris;
                }
            }
        }

        /// <summary>
        /// Gets the URIs specified in USING NAMED clauses
        /// </summary>
        public IEnumerable<Uri> UsingNamedUris
        {
            get
            {
                if (_usingNamedUris == null)
                {
                    return Enumerable.Empty<Uri>();
                }
                else
                {
                    return _usingNamedUris;
                }
            }
        }

        /// <summary>
        /// Gets the URI of the Graph specified in the WITH clause
        /// </summary>
        public Uri GraphUri
        {
            get
            {
                return _graphUri;
            }
            internal set
            {
                _graphUri = value;
            }
        }

        /// <summary>
        /// Adds a new USING URI
        /// </summary>
        /// <param name="u">URI</param>
        public void AddUsingUri(Uri u)
        {
            if (_usingUris == null) _usingUris = new List<Uri>();
            _usingUris.Add(u);
        }

        /// <summary>
        /// Adds a new USING NAMED URI
        /// </summary>
        /// <param name="u">URI</param>
        public void AddUsingNamedUri(Uri u)
        {
            if (_usingNamedUris == null) _usingNamedUris = new List<Uri>();
            _usingNamedUris.Add(u);
        }

        /// <summary>
        /// Determines whether a Graph Pattern is valid for use in an DELETE pattern
        /// </summary>
        /// <param name="p">Graph Pattern</param>
        /// <param name="top">Is this the top level pattern?</param>
        /// <returns></returns>
        protected bool IsValidDeletePattern(GraphPattern p, bool top)
        {
            if (p.IsGraph)
            {
                // If a GRAPH clause then all triple patterns must be constructable and have no Child Graph Patterns
                return !p.HasChildGraphPatterns && p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoBlankVariables);
            }
            else if (p.IsExists || p.IsMinus || p.IsNotExists || p.IsOptional || p.IsService || p.IsSubQuery || p.IsUnion)
            {
                // EXISTS/MINUS/NOT EXISTS/OPTIONAL/SERVICE/Sub queries/UNIONs are not permitted
                return false;
            }
            else
            {
                // For other patterns all Triple patterns must be constructable with no blank variables
                // If top level then any Child Graph Patterns must be valid
                // Otherwise must have no Child Graph Patterns
                return p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoBlankVariables) && ((top && p.ChildGraphPatterns.All(gp => IsValidDeletePattern(gp, false))) || !p.HasChildGraphPatterns);
            }
        }
    }
}
