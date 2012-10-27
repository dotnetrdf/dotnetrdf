/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

namespace VDS.RDF
{
    /// <summary>
    /// Interface for Handlers which handle the RDF produced by parsers
    /// </summary>
    public interface IRdfHandler 
        : INodeFactory
    {
        /// <summary>
        /// Start the Handling of RDF
        /// </summary>
        /// <exception cref="RdfParseException">May be thrown if the Handler is already in use and the implementation is not thread-safe</exception>
        void StartRdf();

        /// <summary>
        /// End the Handling of RDF
        /// </summary>
        /// <param name="ok">Whether parsing finished without error</param>
        void EndRdf(bool ok);

        /// <summary>
        /// Handles a Namespace Definition
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="namespaceUri">Namespace URI</param>
        /// <returns>Should return <strong>true</strong> if parsing should continue or <strong>false</strong> if it should be terminated</returns>
        bool HandleNamespace(String prefix, Uri namespaceUri);

        /// <summary>
        /// Handles a Base URI Definition
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <returns>Should return <strong>true</strong> if parsing should continue or <strong>false</strong> if it should be terminated</returns>
        bool HandleBaseUri(Uri baseUri);

        /// <summary>
        /// Handles a Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns>Should return <strong>true</strong> if parsing should continue or <strong>false</strong> if it should be terminated</returns>
        bool HandleTriple(Triple t);

        /// <summary>
        /// Handles a Quad
        /// </summary>
        /// <param name="q">Quad</param>
        /// <returns>Should return <strong>true</strong> if parsing should continue or <strong>false</strong> if it should be terminated</returns>
        bool HandleQuad(Quad q);

        /// <summary>
        /// Gets whether the Handler will always handle all data (i.e. won't terminate parsing early)
        /// </summary>
        bool AcceptsAll
        {
            get;
        }
    }

    /// <summary>
    /// Interface for Handlers which wrap other Handlers
    /// </summary>
    public interface IWrappingRdfHandler 
        : IRdfHandler
    {
        /// <summary>
        /// Gets the Inner Handlers used by this Handler
        /// </summary>
        IEnumerable<IRdfHandler> InnerHandlers
        {
            get;
        }
    }
}
