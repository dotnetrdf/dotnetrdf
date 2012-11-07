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
using System.Reflection;
using System.Text;
using VDS.Web.Handlers;

namespace VDS.Web
{
    /// <summary>
    /// Possible Modes for Paths
    /// </summary>
    public enum PathMode
    {
        /// <summary>
        /// Unknown - if Path Mode is this a Handler Mapping will not accept any requests
        /// </summary>
        Unknown,
        /// <summary>
        /// The Handler processes all Requests regardless of Path provided they meet the Verb criteria
        /// </summary>
        All,
        /// <summary>
        /// The Handler processes all Requests to a specific file extension
        /// </summary>
        Extension,
        /// <summary>
        /// The Handler processes all Requests to a fixed path
        /// </summary>
        FixedPath,
        /// <summary>
        /// The Handler processes all Requests under a path
        /// </summary>
        WildcardPath
    }


    /// <summary>
    /// Mapping from a Request Verb and Path to a <see cref="IHttpListenerHandler">IHttpListenerHandler</see>
    /// </summary>
    public class HttpRequestMapping
    {
        private String _verb = "*";
        private String[] _verbs;
        private String _path = "/";
        private PathMode _mode = PathMode.Unknown;
        private Type _handlerType;

        /// <summary>
        /// Constant for specifying that the mapping applies for All HTTP Verbs
        /// </summary>
        public const String AllVerbs = "*";
        /// <summary>
        /// Constant for specifying that the mapping applies to Any Path
        /// </summary>
        public const String AnyPath = "*";
        /// <summary>
        /// Constant for specifying that the mapping is never directly applied (use when the Handler may be invoked via <see cref="HttpServer.RemapHandler">HttpServer.RemapHandler()</see> but should not be invoked normally
        /// </summary>
        public const String NoVerbs = "";

        /// <summary>
        /// Creates a new Request Mapping
        /// </summary>
        /// <param name="verb">HTTP Verbs (comma separated)</param>
        /// <param name="path">URL Path</param>
        /// <param name="handlerType">Handler Type</param>
        public HttpRequestMapping(String verb, String path, Type handlerType)
        {
            this._verb = verb.ToUpper();
            if (this._verb.Equals(AllVerbs))
            {
                this._verbs = new String[] { AllVerbs };
            }
            else if (this._verb.Equals(NoVerbs))
            {
                this._verbs = new String[] { };
            }
            else if (!this._verb.Contains(','))
            {
                this._verbs = new String[] { this._verb };
            }
            else
            {
                this._verbs = this._verb.Split(',');
            }

            //Select Path Mode
            this._path = path;
            if (this._path.Equals(AnyPath))
            {
                this._mode = PathMode.All;
            }
            else if (this._path.StartsWith("*."))
            {
                this._mode = PathMode.Extension;
                this._path = this._path.Substring(1);
            } 
            else if (this._path.EndsWith("*"))
            {
                this._mode = PathMode.WildcardPath;
                this._path = this._path.Substring(0, this._path.Length - 1);
            } 
            else 
            {
                this._mode = PathMode.FixedPath;
            }

            //Check Handler Type implements IHttpListenerHandler
            if (handlerType == null) throw new ArgumentNullException("handlerType", "Handler Type cannot be null");
            if (!handlerType.GetInterfaces().Contains(typeof(IHttpListenerHandler)))
            {
                throw new ArgumentException("The Type given for the Handler type must implement the IHttpListenerHandler interface");
            }
            this._handlerType = handlerType;
        }

        /// <summary>
        /// Gets the Type of the Handler
        /// </summary>
        public Type HandlerType
        {
            get
            {
                return this._handlerType;
            }
        }

        /// <summary>
        /// Gets the Accepted Verbs
        /// </summary>
        public String[] AcceptedVerbs
        {
            get
            {
                return this._verbs;
            }
        }

        /// <summary>
        /// Gets the Accepted Path
        /// </summary>
        public String AcceptedPath
        {
            get
            {
                return this._path;
            }
        }

        /// <summary>
        /// Gets the Accepted Path Mode
        /// </summary>
        public PathMode AcceptedPathMode
        {
            get
            {
                return this._mode;
            }
        }

        /// <summary>
        /// Creates a new instance of the Handler this mapping refers to
        /// </summary>
        /// <returns></returns>
        public IHttpListenerHandler CreateHandlerInstance()
        {
            Object temp = Activator.CreateInstance(this._handlerType);
            if (temp is IHttpListenerHandler)
            {
                return (IHttpListenerHandler)temp;
            }
            else
            {
                throw new HttpServerException("Unable to create an instance of the Handler Type " + this._handlerType.FullName + " as it does not implement the IHttpListenerHandler interface");
            }
        }

        /// <summary>
        /// Determines whether this mapping accepts the request
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        public bool AcceptsRequest(HttpServerContext context)
        {
            if (this._mode == PathMode.Unknown) return false;

            if (this.AcceptsVerb(context.Request.HttpMethod))
            {
                String requestPath = context.Request.Url.AbsolutePath;
                switch (this._mode)
                {
                    case PathMode.All:
                        return true;
                    case PathMode.Extension:
                        return (requestPath.EndsWith(this._path));
                    case PathMode.FixedPath:
                        return requestPath.Equals(this._path);
                    case PathMode.WildcardPath:
                        return requestPath.StartsWith(this._path);
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether this mapping accepts the HTTP verb
        /// </summary>
        /// <param name="verb">HTTP verb</param>
        /// <returns></returns>
        private bool AcceptsVerb(String verb)
        {
            if (this._verb.Equals(AllVerbs))
            {
                return true;
            }
            else if (this._verb.Equals(NoVerbs))
            {
                return false;
            }
            else
            {
                return this._verbs.Contains(verb.ToUpper());
            }
        }
    }
}
