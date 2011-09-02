/*

Copyright Robert Vesse 2009-10
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
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using VDS.RDF.Parsing;
#if !NO_WEB
using System.Web;
#endif

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// Represents the Explain Service provided by a Pellet Server
    /// </summary>
    public class ExplainService 
        : PelletService
    {
        private String _explainUri;
        /// <summary>
        /// Base Query for use with the Explain Service
        /// </summary>
        protected SparqlParameterizedString _baseQuery = new SparqlParameterizedString("SELECT * WHERE { @s @p @o . }");

        /// <summary>
        /// Creates a new Explain Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ExplainService(String name, JObject obj)
            : base(name, obj) 
        {
            if (!this.Endpoint.Uri.EndsWith("explain"))
            {
                this._explainUri = this.Endpoint.Uri.Substring(0, this.Endpoint.Uri.IndexOf("explain") + 7);
            }
            else
            {
                this._explainUri = this.Endpoint.Uri;
            }
        }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a Graph explaining the result of the SPARQL Query
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public IGraph Explain(String sparqlQuery)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._explainUri + "?query=" + HttpUtility.UrlEncode(sparqlQuery));
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes, MimeTypesHelper.SupportedRdfMimeTypes);

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    Graph g = new Graph();
                    parser.Load(g, new StreamReader(response.GetResponseStream()));

                    response.Close();
                    return g;
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfReasoningException("A HTTP error occurred while communicating with the Pellet Server", webEx);
            }
        }

#endif

        /// <summary>
        /// Gets a Graph explaining the result of the SPARQL Query
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public void Explain(String sparqlQuery, GraphCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._explainUri + "?query=" + HttpUtility.UrlEncode(sparqlQuery));
            request.Method = this.Endpoint.HttpMethods.First();
            request.Accept = MimeTypesHelper.CustomHttpAcceptHeader(this.MimeTypes, MimeTypesHelper.SupportedRdfMimeTypes);

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            request.BeginGetResponse(result =>
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif
                        IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                        Graph g = new Graph();
                        parser.Load(g, new StreamReader(response.GetResponseStream()));

                        response.Close();
                        callback(g, state);
                    }
                }, null);
        }
    }

    /// <summary>
    /// Represents the Explan Unsatisfiable Service provided by a Pellet Server
    /// </summary>
    public class ExplainUnsatService 
        : ExplainService
    {
        /// <summary>
        /// Creates a new Explain Unsatisfiable Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ExplainUnsatService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a Graph explaining why a Class is unsatisfiable
        /// </summary>
        /// <param name="cls">Class</param>
        /// <returns></returns>
        public IGraph ExplainUnsatisfiable(INode cls)
        {
            this._baseQuery.SetParameter("s", cls);
            this._baseQuery.SetUri("p", new Uri(NamespaceMapper.RDFS + "subClassOf"));
            this._baseQuery.SetUri("o", new Uri(OwlHelper.OwlNothing));

            return base.Explain(this._baseQuery.ToString());
        }

#endif

        /// <summary>
        /// Gets a Graph explaining why a Class is unsatisfiable
        /// </summary>
        /// <param name="cls">Class</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public void ExplainUnsatisfiable(INode cls, GraphCallback callback, Object state)
        {
            this._baseQuery.SetParameter("s", cls);
            this._baseQuery.SetUri("p", new Uri(NamespaceMapper.RDFS + "subClassOf"));
            this._baseQuery.SetUri("o", new Uri(OwlHelper.OwlNothing));

            base.Explain(this._baseQuery.ToString(), callback, state);
        }
    }

    /// <summary>
    /// Represents the Explain Instance Service provided by a Pellet Server
    /// </summary>
    public class ExplainInstanceService
        : ExplainService
    {
        /// <summary>
        /// Creates a new Explain Instance Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ExplainInstanceService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a Graph explaining why an Instance is of the given Class
        /// </summary>
        /// <param name="instance">Instance</param>
        /// <param name="cls">Class</param>
        /// <returns></returns>
        public IGraph ExplainInstance(INode instance, INode cls)
        {
            this._baseQuery.SetParameter("s", instance);
            this._baseQuery.SetUri("p", new Uri(RdfSpecsHelper.RdfType));
            this._baseQuery.SetParameter("o", cls);

            return base.Explain(this._baseQuery.ToString());
        }

#endif

        /// <summary>
        /// Gets a Graph explaining why an Instance is of the given Class
        /// </summary>
        /// <param name="instance">Instance</param>
        /// <param name="cls">Class</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public void ExplainInstance(INode instance, INode cls, GraphCallback callback, Object state)
        {
            this._baseQuery.SetParameter("s", instance);
            this._baseQuery.SetUri("p", new Uri(RdfSpecsHelper.RdfType));
            this._baseQuery.SetParameter("o", cls);

            base.Explain(this._baseQuery.ToString(), callback, state);
        }
    }

    /// <summary>
    /// Represents the Explain Subclass Service provided by a Pellet Server
    /// </summary>
    public class ExplainSubclassService 
        : ExplainService
    {
        /// <summary>
        /// Creates a new Explain Subclass Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ExplainSubclassService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a Graph explaining why the given Class is a subclass of the given Super Class
        /// </summary>
        /// <param name="subclass">Class</param>
        /// <param name="superclass">Super Class</param>
        /// <returns></returns>
        public IGraph ExplainSubclass(INode subclass, INode superclass)
        {
            this._baseQuery.SetParameter("s", subclass);
            this._baseQuery.SetUri("p", new Uri(NamespaceMapper.RDFS + "subClassOf"));
            this._baseQuery.SetParameter("o", superclass);

            return base.Explain(this._baseQuery.ToString());
        }

#endif

        /// <summary>
        /// Gets a Graph explaining why the given Class is a subclass of the given Super Class
        /// </summary>
        /// <param name="subclass">Class</param>
        /// <param name="superclass">Super Class</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public void ExplainSubclass(INode subclass, INode superclass, GraphCallback callback, Object state)
        {
            this._baseQuery.SetParameter("s", subclass);
            this._baseQuery.SetUri("p", new Uri(NamespaceMapper.RDFS + "subClassOf"));
            this._baseQuery.SetParameter("o", superclass);

            base.Explain(this._baseQuery.ToString(), callback, state);
        }
    }

    /// <summary>
    /// Represents the Explain Inconsistent Service provided by a Pellet Server
    /// </summary>
    public class ExplainInconsistentService
        : ExplainService
    {
        /// <summary>
        /// Creates a new Explain Inconsistent Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ExplainInconsistentService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a Graph explaining why the Knowledge Base is inconsistent
        /// </summary>
        /// <returns></returns>
        public IGraph ExplainInconsistent()
        {
            return base.Explain(String.Empty);
        }

#endif

        /// <summary>
        /// Gets a Graph explaining why the Knowledge Base is inconsistent
        /// </summary>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public void ExplainInconsistent(GraphCallback callback, Object state)
        {
            base.Explain(String.Empty, callback, state);
        }
    }

    /// <summary>
    /// Represents the Explain Property Service provided by a Pellet Server
    /// </summary>
    public class ExplainPropertyService
        : ExplainService
    {
        /// <summary>
        /// Creates a new Explain Property Service
        /// </summary>
        /// <param name="name">Service Name</param>
        /// <param name="obj">JSON Object</param>
        internal ExplainPropertyService(String name, JObject obj)
            : base(name, obj) { }

#if !SILVERLIGHT

        /// <summary>
        /// Gets a Graph explaining why the given Triple was derived
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IGraph ExplainProperty(INode subj, INode pred, INode obj)
        {
            this._baseQuery.SetParameter("s", subj);
            this._baseQuery.SetParameter("p", pred);
            this._baseQuery.SetParameter("o", obj);

            return base.Explain(this._baseQuery.ToString());
        }

        /// <summary>
        /// Gets a Graph explaining why the given Triple was derived
        /// </summary>
        /// <param name="t">Triple</param>
        public IGraph ExplainProperty(Triple t)
        {
            return this.ExplainProperty(t.Subject, t.Predicate, t.Object);
        }

#endif

        /// <summary>
        /// Gets a Graph explaining why the given Triple was derived
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public void ExplainProperty(INode subj, INode pred, INode obj, GraphCallback callback, Object state)
        {
            this._baseQuery.SetParameter("s", subj);
            this._baseQuery.SetParameter("p", pred);
            this._baseQuery.SetParameter("o", obj);

            base.Explain(this._baseQuery.ToString(), callback, state);
        }

        /// <summary>
        /// Gets a Graph explaining why the given Triple was derived
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="callback">Callback to invoke when the operation completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void ExplainProprety(Triple t, GraphCallback callback, Object state)
        {
            this.ExplainProperty(t.Subject, t.Predicate, t.Object, callback, state);
        }
    }
}
