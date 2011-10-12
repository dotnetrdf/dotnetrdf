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
using System.Text;
using System.Threading;
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to 4store
    /// </summary>
    /// <remarks>
    /// <para>
    /// Depending on the version of <a href="http://librdf.org/rasqal/">RASQAL</a> used for your 4store instance and the options it was built with some kinds of queries may not suceed or return unexpected results.
    /// </para>
    /// <para>
    /// Prior to the 1.x releases 4store did not permit the saving of unamed Graphs to the Store or Triple level updates.  There was a branch of 4store that supports Triple level updates and you could tell the connector if your 4store instance supports this when you instantiate it.  From the 0.4.0 release of the library onwards this support was enabled by default since the 1.x builds of 4store have this feature integrated into them by default.
    /// </para>
    /// </remarks>
    public class FourStoreConnector 
        : IQueryableGenericIOManager, IUpdateableGenericIOManager, IConfigurationSerializable
    {
        private String _baseUri;
        private SparqlRemoteEndpoint _endpoint;
        private SparqlRemoteUpdateEndpoint _updateEndpoint;
        private bool _updatesEnabled = true;
        private SparqlFormatter _formatter = new SparqlFormatter();

#if !NO_RWLOCK
        ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim();
#endif

        /// <summary>
        /// Creates a new 4store connector which manages access to the services provided by a 4store server
        /// </summary>
        /// <param name="baseUri">Base Uri of the 4store</param>
        /// <remarks>
        /// <strong>Note:</strong> As of the 0.4.0 release 4store support defaults to Triple Level updates enabled as all recent 4store releases have supported this.  You can still optionally disable this with the two argument version of the constructor
        /// </remarks>
        public FourStoreConnector(String baseUri)
        {
            //Determine the appropriate actual Base Uri
            if (baseUri.EndsWith("sparql/"))
            {
                this._baseUri = baseUri.Substring(0, baseUri.IndexOf("sparql/"));
            }
            else if (baseUri.EndsWith("data/"))
            {
                this._baseUri = baseUri.Substring(0, baseUri.IndexOf("data/"));
            }
            else if (!baseUri.EndsWith("/"))
            {
                this._baseUri = baseUri + "/";
            }
            else
            {
                this._baseUri = baseUri;
            }

            this._endpoint = new SparqlRemoteEndpoint(new Uri(this._baseUri + "sparql/"));
            this._updateEndpoint = new SparqlRemoteUpdateEndpoint(new Uri(this._baseUri + "update/"));
            this._endpoint.Timeout = 60000;
            this._updateEndpoint.Timeout = 60000;
        }

        /// <summary>
        /// Creates a new 4store connector which manages access to the services provided by a 4store server
        /// </summary>
        /// <param name="baseUri">Base Uri of the 4store</param>
        /// <param name="enableUpdateSupport">Indicates to the connector that you are using a 4store instance that supports Triple level updates</param>
        /// <remarks>
        /// If you enable Update support but are using a 4store instance that does not support Triple level updates then you will almost certainly experience errors while using the connector.
        /// </remarks>
        public FourStoreConnector(String baseUri, bool enableUpdateSupport)
            : this(baseUri)
        {
            this._updatesEnabled = enableUpdateSupport;
        }

        /// <summary>
        /// Loads a Graph from the 4store instance
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the 4store instance using an RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the 4store instance
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, String graphUri)
        {
            if (g.IsEmpty && graphUri != null & !graphUri.Equals(String.Empty))
            {
                g.BaseUri = new Uri(graphUri);
            }
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the 4store instance
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            try
            {
#if !NO_RWLOCK
                this._lockManager.EnterReadLock();
#endif

                if (!graphUri.Equals(String.Empty))
                {
                    this._endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } FROM <" + graphUri.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }");
                }
                else
                {
                    throw new RdfStorageException("Cannot retrieve a Graph from 4store without specifying a Graph URI");
                }
            }
            finally
            {
#if !NO_RWLOCK
                this._lockManager.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Saves a Graph to a 4store instance (Warning: Completely replaces any existing Graph with the same URI)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// <para>
        /// Completely replaces any existing Graph with the same Uri in the store
        /// </para>
        /// <para>
        /// Attempting to save a Graph which doesn't have a Base Uri will result in an error
        /// </para>
        /// </remarks>
        /// <exception cref="RdfStorageException">Thrown if you try and save a Graph without a Base Uri or if there is an error communicating with the 4store instance</exception>
        public void SaveGraph(IGraph g)
        {
            try
            {
#if !NO_RWLOCK
                this._lockManager.EnterWriteLock();
#endif

                //Set up the Request
                HttpWebRequest request;
                if (g.BaseUri != null)
                {
                    request = (HttpWebRequest)WebRequest.Create(this._baseUri + "data/" + Uri.EscapeUriString(g.BaseUri.ToString()));
                }
                else
                {
                    throw new RdfStorageException("Cannot save a Graph without a Base URI to a 4store Server");
                }
                request.Method = "PUT";
                request.ContentType = MimeTypesHelper.Turtle[0];

                //Write the Graph as Turtle to the Request Stream
                CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

                #if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
                #endif

                //Make the Request
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
                throw new RdfStorageException("A HTTP error occurred while communicating with the 4store Server", webEx);
            }
            finally
            {
#if !NO_RWLOCK
                this._lockManager.ExitWriteLock();
#endif
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of 4store
        /// </summary>
        public IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.IsQuadStore | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Updates a Graph in the store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// May throw an error since the default builds of 4store don't support Triple level updates.  There are builds that do support this and the user can instantiate the connector with support for this enabled if they wish, if they do so and the underlying 4store doesn't support updates errors will occur when updates are attempted.
        /// </remarks>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null)
            {
                throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
            }
            else
            {
                this.UpdateGraph(graphUri.ToString(), additions, removals);
            }
        }

        /// <summary>
        /// Updates a Graph in the store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// May throw an error since the default builds of 4store don't support Triple level updates.  There are builds that do support this and the user can instantiate the connector with support for this enabled if they wish, if they do so and the underlying 4store doesn't support updates errors will occur when updates are attempted.
        /// </remarks>
        public void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (!this._updatesEnabled)
            {
                throw new RdfStorageException("4store does not support Triple level updates");
            }
            else if (graphUri.Equals(String.Empty))
            {
                throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
            }
            else
            {
                try
                {
#if !NO_RWLOCK
                    this._lockManager.EnterWriteLock();
#endif
                    //TODO: Change this to use the SparqlRemoteUpdateEndpoint 

                    if (removals != null)
                    {
                        if (removals.Any())
                        {
                            //Build up the DELETE command and execute
                            StringBuilder delete = new StringBuilder();
                            delete.AppendLine("DELETE DATA");
                            delete.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in removals)
                            {
                                delete.AppendLine(t.ToString(this._formatter));
                            }
                            delete.AppendLine("}}");

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._baseUri + "update/");
                            request.Method = "POST";
                            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                            StreamWriter postWriter = new StreamWriter(request.GetRequestStream());
                            postWriter.Write("update=");
                            postWriter.Write(HttpUtility.UrlEncode(delete.ToString()));
                            postWriter.Close();

                            #if DEBUG
                                if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
                            #endif

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
#if DEBUG
                                if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif
                                //If we get here then it was OK
                                response.Close();
                            }
                        }
                    }

                    if (additions != null)
                    {
                        if (additions.Any())
                        {
                            //Build up the INSERT command and execute
                            StringBuilder insert = new StringBuilder();
                            insert.AppendLine("INSERT DATA");
                            insert.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in additions)
                            {
                                insert.AppendLine(t.ToString(this._formatter));
                            }
                            insert.AppendLine("}}");

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._baseUri + "update/");
                            request.Method = "POST";
                            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                            StreamWriter postWriter = new StreamWriter(request.GetRequestStream());
                            postWriter.Write("update=");
                            postWriter.Write(HttpUtility.UrlEncode(insert.ToString()));
                            postWriter.Close();

                            #if DEBUG
                                if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
                            #endif

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
#if DEBUG
                                if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif
                                //If we get here then it was OK
                                response.Close();
                            }
                        }
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
                    throw new RdfStorageException("A HTTP error occurred while communicating with the 4store Server", webEx);
                }
                finally
                {
#if !NO_RWLOCK
                    this._lockManager.ExitWriteLock();
#endif
                }
            }
        }

        /// <summary>
        /// Returns whether this connector has been instantiated with update support or not
        /// </summary>
        /// <remarks>
        /// If this property returns true it does not guarantee that the 4store instance actually supports updates it simply indicates that the user has enabled updates on the connector.  If Updates are enabled and the 4store server being connected to does not support updates then errors will occur.
        /// </remarks>
        public bool UpdateSupported
        {
            get 
            {
                return false; 
            }
        }

        /// <summary>
        /// Returns that the Connection is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Connection is not read-only
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying 4store Instance
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns>A <see cref="Graph">Graph</see> or a <see cref="SparqlResultSet">SparqlResultSet</see></returns>
        /// <remarks>
        /// Depending on the version of <a href="http://librdf.org/rasqal/">RASQAL</a> used and the options it was built with some kinds of queries may not suceed or return unexpected results.
        /// </remarks>
        public Object Query(String sparqlQuery)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }
            else
            {
                return g;
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying 4store Instance processing the results with the appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            HttpWebResponse response = this._endpoint.QueryRaw(sparqlQuery);
            StreamReader data = new StreamReader(response.GetResponseStream());
            try
            {
                //Is the Content Type referring to a Sparql Result Set format?
                ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(response.ContentType);
                resreader.Load(resultsHandler, data);
                response.Close();
            }
            catch (RdfParserSelectionException)
            {
                //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                //Is the Content Type referring to a RDF format?
                IRdfReader rdfreader = MimeTypesHelper.GetParser(response.ContentType);
                rdfreader.Load(rdfHandler, data);
                response.Close();
            }
        }

        /// <summary>
        /// Deletes a Graph from the 4store server
        /// </summary>
        /// <param name="graphUri">Uri of Graph to delete</param>
        public void DeleteGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                throw new RdfStorageException("You must specify a valid URI in order to delete a Graph from 4store");
            }
            else
            {
                this.DeleteGraph(graphUri.ToString());
            }
        }

        /// <summary>
        /// Deletes a Graph from the 4store server
        /// </summary>
        /// <param name="graphUri">Uri of Graph to delete</param>
        public void DeleteGraph(String graphUri)
        {
            try
            {
#if !NO_RWLOCK
                this._lockManager.EnterWriteLock();
#endif

                //Set up the Request
                HttpWebRequest request;
                if (!graphUri.Equals(String.Empty))
                {
                    request = (HttpWebRequest)WebRequest.Create(this._baseUri + "data/" + Uri.EscapeUriString(graphUri));
                }
                else
                {
                    throw new RdfStorageException("Cannot delete a Graph without a Base URI from a 4store Server");
                }
                request.Method = "DELETE";

                #if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
                #endif

                //Make the Request
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    response.Close();
                }

                //If we get here then it's OK
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP error occurred while communicating with the 4store Server", webEx);
            }
            finally
            {
#if !NO_RWLOCK
                this._lockManager.ExitWriteLock();
#endif
            }
        }

        /// <summary>
        /// Returns that deleting Graph is supported
        /// </summary>
        public bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Lists the Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Uri> ListGraphs()
        {
            try
            {
                Object results = this.Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
                if (results is SparqlResultSet)
                {
                    List<Uri> graphs = new List<Uri>();
                    foreach (SparqlResult r in ((SparqlResultSet)results))
                    {
                        if (r.HasValue("g"))
                        {
                            INode temp = r["g"];
                            if (temp.NodeType == NodeType.Uri)
                            {
                                graphs.Add(((IUriNode)temp).Uri);
                            }
                        }
                    }
                    return graphs;
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
            }
        }

        /// <summary>
        /// Returns that Listing Graphs is supported
        /// </summary>
        public bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Applies a SPARQL Update against 4store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <remarks>
        /// <strong>Note:</strong> Please be aware that some valid SPARQL Updates may not be accepted by 4store since the SPARQL parser used by 4store does not support some of the latest editors draft syntax changes.
        /// </remarks>
        public void Update(String sparqlUpdate)
        {
            this._updateEndpoint.Update(sparqlUpdate);
        }

        /// <summary>
        /// Disposes of a 4store connection
        /// </summary>
        public void Dispose()
        {
            //No Dispose actions needed
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[4store] " + this._baseUri;
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context"></param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode enableUpdates = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEnableUpdates);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            context.Graph.Assert(new Triple(manager, enableUpdates, this._updatesEnabled.ToLiteral(context.Graph)));
        }
    }
}

#endif