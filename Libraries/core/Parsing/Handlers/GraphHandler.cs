/*

Copyright Robert Vesse 2009-11
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

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A RDF Handler which asserts Triples into a Graph
    /// </summary>
    public class GraphHandler : BaseRdfHandler
    {
        private IGraph _target;
        private IGraph _g;

        /// <summary>
        /// Creates a new Graph Handler
        /// </summary>
        /// <param name="g">Graph</param>
        public GraphHandler(IGraph g)
            : base(g)
        {
            if (g == null) throw new ArgumentNullException("graph");
            this._g = g;
        }

        /// <summary>
        /// Gets the Base URI of the Graph currently being parsed into
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                if (this._target != null)
                {
                    return this._target.BaseUri;
                }
                else
                {
                    return this._g.BaseUri;
                }
            }
        }

        /// <summary>
        /// Gets the Graph that this handler wraps
        /// </summary>
        protected IGraph Graph
        {
            get
            {
                return this._g;
            }
        }

        /// <summary>
        /// Starts Handling RDF ensuring that if the target Graph is non-empty RDF is handling into a temporary Graph until parsing completes successfully
        /// </summary>
        protected override void StartRdfInternal()
        {
            if (this._g.IsEmpty)
            {
                this._target = this._g;
            }
            else
            {
                this._target = new Graph(true);
                this._target.NamespaceMap.Import(this._g.NamespaceMap);
                this._target.BaseUri = this._g.BaseUri;
            }
            this.NodeFactory = this._target;
        }

        /// <summary>
        /// Ends Handling RDF discarding the handled Triples if parsing failed (indicated by false for the <paramref name="ok">ok</paramref> parameter) and otherwise merging the handled triples from the temporary graph into the target graph if necessary
        /// </summary>
        /// <param name="ok">Indicates whether parsing completed OK</param>
        protected override void EndRdfInternal(bool ok)
        {
            if (ok)
            {
                //If the Target Graph was different from the Destination Graph then do a Merge
                if (!ReferenceEquals(this._g, this._target))
                {
                    this._g.Merge(this._target);
                    this._g.NamespaceMap.Import(this._target.NamespaceMap);
                    if (this._g.BaseUri == null) this._g.BaseUri = this._target.BaseUri;
                }
                else
                {
                    //The Target was the Graph so we want to set our reference to it to be null so we don't
                    //clear it in the remainder of our clean up step
                    this._target = null;
                }
            }
            else
            {
                //Discard the Parsed Triples if parsing failed
                if (ReferenceEquals(this._g, this._target))
                {
                    this._g.Clear();
                    this._target = null;
                }
            }

            //Always throw away the target afterwards if not already done so
            if (this._target != null)
            {
                this._target.Clear();
                this._target = null;
            }
        }

        /// <summary>
        /// Handles Namespace Declarations by adding them to the Graphs Namespace Map
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns></returns>
        protected override bool HandleNamespaceInternal(string prefix, Uri namespaceUri)
        {
            this._target.NamespaceMap.AddNamespace(prefix, namespaceUri);
            return true;
        }

        /// <summary>
        /// Handles Base URI Declarations by setting the Graphs Base URI
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns></returns>
        protected override bool HandleBaseUriInternal(Uri baseUri)
        {
            this._target.BaseUri = baseUri;
            return true;
        }

        /// <summary>
        /// Handles Triples by asserting them in the Graph
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected override bool HandleTripleInternal(Triple t)
        {
            this._target.Assert(t);
            return true;
        }

        /// <summary>
        /// Gets that this Handler accepts all Triples
        /// </summary>
        public override bool AcceptsAll
        {
            get 
            {
                return true; 
            }
        }
    }
}
