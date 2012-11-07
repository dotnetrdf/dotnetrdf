/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
