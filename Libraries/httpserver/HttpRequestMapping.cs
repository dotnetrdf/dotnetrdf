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
