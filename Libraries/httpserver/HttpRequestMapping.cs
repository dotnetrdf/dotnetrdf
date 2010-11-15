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

    public class HttpRequestMapping
    {
        private String _verb = "*";
        private String[] _verbs;
        private String _path = "/";
        private PathMode _mode = PathMode.Unknown;
        private Type _handlerType;

        public const String AllVerbs = "*";

        public const String AnyPath = "*";

        public HttpRequestMapping(String verb, String path, Type handlerType)
        {
            this._verb = verb.ToUpper();
            if (this._verb.Equals(AllVerbs))
            {
                this._verbs = new String[] { AllVerbs };
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

        public Type HandlerType
        {
            get
            {
                return this._handlerType;
            }
        }

        public String[] AcceptedVerb
        {
            get
            {
                return this._verbs;
            }
        }

        public String AcceptedPath
        {
            get
            {
                return this._path;
            }
        }

        public PathMode AcceptedPathMode
        {
            get
            {
                return this._mode;
            }
        }

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

        private bool AcceptsVerb(String verb)
        {
            if (this._verb.Equals(AllVerbs))
            {
                return true;
            }
            else
            {
                return this._verbs.Contains(verb.ToUpper());
            }
        }
    }
}
