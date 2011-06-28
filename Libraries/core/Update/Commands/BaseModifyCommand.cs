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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// Abstract Base class for classes that represent SPARQL Update INSERT, DELETE and INSERT/DELETE commands
    /// </summary>
    public abstract class BaseModificationCommand : SparqlUpdateCommand
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
                if (this._usingUris == null)
                {
                    return Enumerable.Empty<Uri>();
                }
                else
                {
                    return this._usingUris;
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
                if (this._usingNamedUris == null)
                {
                    return Enumerable.Empty<Uri>();
                }
                else
                {
                    return this._usingNamedUris;
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
                return this._graphUri;
            }
            internal set
            {
                this._graphUri = value;
            }
        }

        /// <summary>
        /// Adds a new USING URI
        /// </summary>
        /// <param name="u">URI</param>
        internal void AddUsingUri(Uri u)
        {
            if (this._usingUris == null) this._usingUris = new List<Uri>();
            this._usingUris.Add(u);
        }

        /// <summary>
        /// Adds a new USING NAMED URI
        /// </summary>
        /// <param name="u">URI</param>
        internal void AddUsingNamedUri(Uri u)
        {
            if (this._usingNamedUris == null) this._usingNamedUris = new List<Uri>();
            this._usingNamedUris.Add(u);
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
                //If a GRAPH clause then all triple patterns must be constructable and have no Child Graph Patterns
                return !p.HasChildGraphPatterns && p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoBlankVariables);
            }
            else if (p.IsExists || p.IsMinus || p.IsNotExists || p.IsOptional || p.IsService || p.IsSubQuery || p.IsUnion)
            {
                //EXISTS/MINUS/NOT EXISTS/OPTIONAL/SERVICE/Sub queries/UNIONs are not permitted
                return false;
            }
            else
            {
                //For other patterns all Triple patterns must be constructable with no blank variables
                //If top level then any Child Graph Patterns must be valid
                //Otherwise must have no Child Graph Patterns
                return p.TriplePatterns.All(tp => tp is IConstructTriplePattern && ((IConstructTriplePattern)tp).HasNoBlankVariables) && ((top && p.ChildGraphPatterns.All(gp => IsValidDeletePattern(gp, false))) || !p.HasChildGraphPatterns);
            }
        }
    }
}
