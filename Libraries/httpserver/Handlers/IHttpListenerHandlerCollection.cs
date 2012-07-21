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
using VDS.Web;

namespace VDS.Web.Handlers
{
    /// <summary>
    /// Interface represents a collection of mappings for handlers
    /// </summary>
    public interface IHttpListenerHandlerCollection
        : IEnumerable<HttpRequestMapping>
    {
        /// <summary>
        /// Adds a mapping
        /// </summary>
        /// <param name="mapping">Mapping</param>
        void AddMapping(HttpRequestMapping mapping);

        /// <summary>
        /// Gets the size of the collection
        /// </summary>
        int Count 
        {
            get;
        }

        /// <summary>
        /// Gets a Handler based on the current server context
        /// </summary>
        /// <param name="context">Server Context</param>
        /// <returns></returns>
        IHttpListenerHandler GetHandler(HttpServerContext context);

        /// <summary>
        /// Gets a Handler based on the given type
        /// </summary>
        /// <param name="handlerType">Handler Type</param>
        /// <returns></returns>
        IHttpListenerHandler GetHandler(Type handlerType);

        /// <summary>
        /// Inserts a mapping into the collection
        /// </summary>
        /// <param name="mapping">Mapping</param>
        /// <param name="insertAt">Index to insert at</param>
        void InsertMapping(HttpRequestMapping mapping, int insertAt);

        /// <summary>
        /// Removes a mapping from the collection
        /// </summary>
        /// <param name="mapping">Mapping</param>
        void RemoveMapping(HttpRequestMapping mapping);

        /// <summary>
        /// Removes a mapping at the given index
        /// </summary>
        /// <param name="removeAt">Index to remove at</param>
        void RemoveMapping(int removeAt);
    }
}
