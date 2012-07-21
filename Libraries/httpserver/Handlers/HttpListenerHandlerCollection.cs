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
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web.Handlers
{
    /// <summary>
    /// Class representing a collection of mappings to Handlers
    /// </summary>
    public class HttpListenerHandlerCollection
        : IHttpListenerHandlerCollection
    {
        private List<HttpRequestMapping> _mappings = new List<HttpRequestMapping>();
        private Dictionary<Type, IHttpListenerHandler> _cachedHandlers = new Dictionary<Type, IHttpListenerHandler>();

        /// <summary>
        /// Gets the size of the collection
        /// </summary>
        public int Count
        {
            get
            {
                return this._mappings.Count;
            }
        }

        /// <summary>
        /// Adds a mapping
        /// </summary>
        /// <param name="mapping">Mapping</param>
        public void AddMapping(HttpRequestMapping mapping)
        {
            this._mappings.Add(mapping);
        }

        /// <summary>
        /// Inserts a mapping
        /// </summary>
        /// <param name="mapping">Mapping</param>
        /// <param name="insertAt">Index to insert at</param>
        public void InsertMapping(HttpRequestMapping mapping, int insertAt)
        {
            this._mappings.Insert(insertAt, mapping);
        }

        /// <summary>
        /// Removes a Mapping
        /// </summary>
        /// <param name="mapping">Mapping</param>
        public void RemoveMapping(HttpRequestMapping mapping)
        {
            this._mappings.Remove(mapping);
        }

        /// <summary>
        /// Removes a Mapping
        /// </summary>
        /// <param name="removeAt">Index to remove at</param>
        public void RemoveMapping(int removeAt)
        {
            this._mappings.RemoveAt(removeAt);
        }

        /// <summary>
        /// Gets a Handler based on the given server context
        /// </summary>
        /// <param name="context">Server Context</param>
        /// <returns></returns>
        public IHttpListenerHandler GetHandler(HttpServerContext context)
        {
            for (int i = 0; i < this._mappings.Count; i++)
            {
                if (this._mappings[i].AcceptsRequest(context))
                {
                    Type t = this._mappings[i].HandlerType;

                    if (this._cachedHandlers.ContainsKey(t))
                    {
                        return this._cachedHandlers[t];
                    }
                    else
                    {
                        //Get a new instance and Cache if reusable
                        try
                        {
                            IHttpListenerHandler handler = this._mappings[i].CreateHandlerInstance();
                            if (handler.IsReusable)
                            {
                                this._cachedHandlers.Add(t, handler);
                            }
                            return handler;
                        }
                        catch (Exception ex)
                        {
                            throw new NoHandlerException("The Handler Type " + t.FullName + " which would have processed the request failed to instantiate correctly", ex);
                        }
                    }
                }
            }

            throw new NoHandlerException("There are No Handlers registered which can process this request");
        }

        /// <summary>
        /// Gets a Handler based on the given Type
        /// </summary>
        /// <param name="handlerType">Handler Type</param>
        /// <returns></returns>
        public IHttpListenerHandler GetHandler(Type handlerType)
        {
            if (this._cachedHandlers.ContainsKey(handlerType))
            {
                return this._cachedHandlers[handlerType];
            }
            else
            {
                //Get a new instance and Cache if reusable
                try
                {
                    IHttpListenerHandler handler = this._mappings.FirstOrDefault(m => m.HandlerType.Equals(handlerType)).CreateHandlerInstance();
                    if (handler.IsReusable)
                    {
                        this._cachedHandlers.Add(handlerType, handler);
                    }
                    return handler;
                }
                catch (Exception ex)
                {
                    throw new NoHandlerException("The Handler Type " + handlerType.FullName + " which would have processed the request failed to instantiate correctly", ex);
                }
            }
        }

        /// <summary>
        /// Gets an enumerator over the mappings
        /// </summary>
        /// <returns></returns>
        public IEnumerator<HttpRequestMapping> GetEnumerator()
        {
            return this._mappings.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator over the mappings
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
