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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to any store that implements the SPARQL Graph Store HTTP Protocol for Managing Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <a href="http://www.w3.org/TR/sparql11-http-rdf-update/">SPARQL Graph Store HTTP Protocol</a> is defined as part of SPARQL 1.1 and is currently a working draft so implementations are not guaranteed to be fully compliant with the draft and the protocol may change in the future.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> While this connector supports the update of a Graph the Graph Store HTTP Protocol only allows for the addition of data to an existing Graph and not the removal of data, therefore any calls to <see cref="SparqlHttpProtocolConnector.UpdateGraph">UpdateGraph()</see> that would require the removal of Triple(s) will result in an error.
    /// </para>
    /// </remarks>
    public class SparqlHttpProtocolConnector 
        : IGenericIOManager, IConfigurationSerializable
    {
        /// <summary>
        /// URI of the Protocol Server
        /// </summary>
        protected String _serviceUri;

        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Connector
        /// </summary>
        /// <param name="serviceUri">URI of the Protocol Server</param>
        public SparqlHttpProtocolConnector(String serviceUri)
        {
            if (serviceUri == null) throw new ArgumentNullException("serviceUri", "Cannot create a connection to a Graph Store HTTP Protocol store if the Service URI is null");
            if (serviceUri.Equals(String.Empty)) throw new ArgumentException("Cannot create a connection to a Graph Store HTTP Protocol store if the Service URI is null/empty", "serviceUri");

            this._serviceUri = serviceUri;
        }

        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Connector
        /// </summary>
        /// <param name="serviceUri">URI of the Protocol Server</param>
        public SparqlHttpProtocolConnector(Uri serviceUri)
            : this(serviceUri.ToSafeString()) { }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IGraph g, String graphUri)
        {
            Uri origUri = g.BaseUri;
            if (origUri == null && g.IsEmpty && graphUri != null && !graphUri.Equals(String.Empty))
            {
                origUri = new Uri(graphUri);
            }
            this.LoadGraph(new GraphHandler(g), graphUri);
            g.BaseUri = origUri;
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, String graphUri)
        {
            String retrievalUri = this._serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                retrievalUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                retrievalUri += "?default";
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(retrievalUri);
                request.Method = "GET";
                request.Accept = MimeTypesHelper.HttpAcceptHeader;

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //Parse the retrieved RDF
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));

                    //If we get here then it was OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                //If the error is a 404 then return false
                //Any other error caused the function to throw an error
                if (webEx.Response != null)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                }
                throw new RdfStorageException("A HTTP Error occurred while trying to load a Graph from the Store", webEx);
            }
        }

        /// <summary>
        /// Sends a HEAD Command to the Protocol Server to determine whether a given Graph exists
        /// </summary>
        /// <param name="graphUri">URI of the Graph to check for</param>
        public virtual bool GraphExists(Uri graphUri)
        {
            return this.GraphExists(graphUri.ToSafeString());
        }

        /// <summary>
        /// Sends a HEAD Command to the Protocol Server to determine whether a given Graph exists
        /// </summary>
        /// <param name="graphUri">URI of the Graph to check for</param>
        public virtual bool GraphExists(String graphUri)
        {
            String lookupUri = this._serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                lookupUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                lookupUri += "?default";
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lookupUri);
                request.Method = "HEAD";

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we get here then it was OK
                    response.Close();
                    return true;
                }
            }
            catch (WebException webEx)
            {
                //If the error is a 404 then return false
                //Any other error caused the function to throw an error
                if (webEx.Response != null)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }
                throw new RdfStorageException("A HTTP Error occurred while trying to check whether a Graph exists in the Store", webEx);
            }
        }

        /// <summary>
        /// Saves a Graph to the Protocol Server
        /// </summary>
        /// <param name="g">Graph to save</param>
        public virtual void SaveGraph(IGraph g)
        {
            String saveUri = this._serviceUri;
            if (g.BaseUri != null)
            {
                saveUri += "?graph=" + Uri.EscapeDataString(g.BaseUri.ToString());
            }
            else
            {
                saveUri += "?default";
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(saveUri));
                request.Method = "PUT";
                request.ContentType = MimeTypesHelper.RdfXml[0];
                RdfXmlWriter writer = new RdfXmlWriter();
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we get here then it was OK
                    response.Close();
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
                throw new RdfStorageException("A HTTP Error occurred while trying to save a Graph to the Store", webEx);
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of SPARQL Graph Store protocol based stores
        /// </summary>
        public virtual IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.IsQuadStore | IOBehaviour.HasDefaultGraph | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteDefault | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateAddTriples;
            }
        }

        /// <summary>
        /// Updates a Graph on the Protocol Server
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// <strong>Note:</strong> The SPARQL Graph Store HTTP Protocol for Graph Management only supports the addition of Triples to a Graph and does not support removal of Triples from a Graph.  If you attempt to remove Triples then an <see cref="RdfStorageException">RdfStorageException</see> will be thrown
        /// </remarks>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        /// <summary>
        /// Updates a Graph on the Protocol Server
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// <strong>Note:</strong> The SPARQL Graph Store HTTP Protocol for Graph Management only supports the addition of Triples to a Graph and does not support removal of Triples from a Graph.  If you attempt to remove Triples then an <see cref="RdfStorageException">RdfStorageException</see> will be thrown
        /// </remarks>
        public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (removals != null && removals.Any()) throw new RdfStorageException("Unable to Update a Graph since this update requests that Triples be removed from the Graph which the SPARQL Graph Store HTTP Protocol for Graph Management does not support");

            if (additions == null || !additions.Any()) return;

            String updateUri = this._serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                updateUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                updateUri += "?default";
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(updateUri));
                request.Method = "POST";
                request.ContentType = MimeTypesHelper.RdfXml[0];
                RdfXmlWriter writer = new RdfXmlWriter();
                Graph g = new Graph();
                g.Assert(additions);
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we get here then it was OK
                    response.Close();
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
                throw new RdfStorageException("A HTTP Error occurred while trying to update a Graph in the Store", webEx);
            }
        }

        /// <summary>
        /// Gets that Updates are supported
        /// </summary>
        public virtual bool UpdateSupported
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Deletes a Graph from the store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(String graphUri)
        {
            String deleteUri = this._serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                deleteUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                deleteUri += "?default";
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(deleteUri));
                request.Method = "DELETE";

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we get here then it was OK
                    response.Close();
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

                //Don't throw the error if we get a 404 - this means we couldn't do a delete as the graph didn't exist to start with
                if (webEx.Response == null || (webEx.Response != null && ((HttpWebResponse)webEx.Response).StatusCode != HttpStatusCode.NotFound))
                {
                    throw new RdfStorageException("A HTTP Error occurred while trying to delete a Graph from the Store", webEx);
                }
            }
        }

        /// <summary>
        /// Returns that deleting Graphs is supported
        /// </summary>
        public virtual bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Throws an exception as listing graphs in a SPARQL Graph Store HTTP Protocol does not support listing graphs
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since SPARQL Graph Store HTTP Protocol does not support listing graphs</exception>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            throw new NotSupportedException("SPARQL HTTP Protocol Connector does not support listing Graphs");
        }

        /// <summary>
        /// Returns that listing Graphs is not supported
        /// </summary>
        public virtual bool ListGraphsSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets that the Store is ready
        /// </summary>
        public virtual bool IsReady
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Gets that the Store is not read-only
        /// </summary>
        public virtual bool IsReadOnly
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes of the Connection
        /// </summary>
        public virtual void Dispose()
        {
            //Nothing to dispose of
        }

        /// <summary>
        /// Gets a String representation of the connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[SPARQL Graph Store HTTP Protocol] " + this._serviceUri;
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._serviceUri)));
        }
    }
}

#endif