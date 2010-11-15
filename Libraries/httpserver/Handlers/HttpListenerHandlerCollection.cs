using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web.Handlers
{
    public class HttpListenerHandlerCollection : VDS.Web.Handlers.IHttpListenerHandlerCollection
    {
        private List<HttpRequestMapping> _mappings = new List<HttpRequestMapping>();
        private Dictionary<Type, IHttpListenerHandler> _cachedHandlers = new Dictionary<Type, IHttpListenerHandler>();

        public int Count
        {
            get
            {
                return this._mappings.Count;
            }
        }

        public void AddMapping(HttpRequestMapping mapping)
        {
            this._mappings.Add(mapping);
        }

        public void InsertMapping(HttpRequestMapping mapping, int insertAt)
        {
            this._mappings.Insert(insertAt, mapping);
        }

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
    }
}
