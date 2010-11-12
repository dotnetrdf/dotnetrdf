using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web
{
    public abstract class HttpListenerHandlerCollection
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

        public IHttpListenerHandler GetHandler(HttpListenerContext context)
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
                            throw new NoHandlerException("The Handler Type " + t.FullName + " which would have accepted the request failed to instantiate correctly", ex);
                        }
                    }
                }
            }

            throw new NoHandlerException("There are No Handlers registered which can process this request");
        }

        /// <summary>
        /// Allows for the Handler Collection to optionally read/write values from the Server State for use by its contained Handlers
        /// </summary>
        /// <param name="state">HTTP Server State</param>
        public virtual void Initialise(HttpServerState state)
        {

        }
    }
}
