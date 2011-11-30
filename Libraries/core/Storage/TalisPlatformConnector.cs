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

If this license is not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Writing;

#if !NO_STORAGE

namespace VDS.RDF.Storage
{

    /// <summary>
    /// Possible results of a Talis Update Operation
    /// </summary>
    public enum TalisUpdateResult
    {
        /// <summary>
        /// No Update was Required
        /// </summary>
        NotRequired,
        /// <summary>
        /// Unversioned Update was Done
        /// </summary>
        Done,
        /// <summary>
        /// Versioned Update was Done Synchronously
        /// </summary>
        Synchronous,
        /// <summary>
        /// Unversioned Update was Done Asychronously
        /// </summary>
        Asynchronous,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Class for connecting to the Talis Platform
    /// </summary>
    /// <remarks>
    /// The Talis platform automatically converts all Blank Nodes input into it into Uri nodes.  This means that data saved to Talis and then retrieved may lose it's Blank Nodes or have them assigned different IDs (different IDs is perfectly acceptable behaviour for any RDF based application since Blank Node IDs are only ever scoped to a given serialization).
    /// </remarks>
    public class TalisPlatformConnector
        : IQueryableGenericIOManager, IConfigurationSerializable 
    {
        private String _storename, _username, _password;
        private String _baseuri;
        private bool _hasCredentials = true;

        #region Constants

        /// <summary>
        /// Namespace for the Talis Changeset Ontology
        /// </summary>
        public const String TalisChangeSetNamespace = "http://purl.org/vocab/changeset/schema#";
        /// <summary>
        /// Formatting String used to turn Date Times into ChangeSet IDs on URIs
        /// </summary>
        public const String TalisChangeSetIDFormat = "yyyyMMdd-HHmmss";
        /// <summary>
        /// MIME Type for Talis ChangeSets
        /// </summary>
        public const String TalisChangeSetMIMEType = "application/vnd.talis.changeset+xml";
        /// <summary>
        /// Base Uri for the Talis Platform API
        /// </summary>
        public const String TalisAPIBaseURI = "http://api.talis.com/stores/";

        #endregion

        /// <summary>
        /// Creates a new Talis Platform Connector which manages access to the services provided by the Talis platform
        /// </summary>
        /// <param name="storeName">Name of the Store</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <remarks>This Constructor creates a Connector which provides authentication details when making requests to the Talis Platform.  Note that this does not guarentee that operations suceed since the account you apply requires certain capabilities in order for operations to be permitted.</remarks>
        public TalisPlatformConnector(String storeName, String username, String password)
        {
            this._storename = storeName;
            this._username = username;
            this._password = password;
            this._hasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));

            this._baseuri = TalisAPIBaseURI + this._storename + "/";
        }

        /// <summary>
        /// Creates a new Talis Platform Connector which manages access to the services provided by the Talis platform
        /// </summary>
        /// <param name="storeName">Name of the Store</param>
        /// <remarks>This Constructor creates a Connector which does not provide authentication details when making requests to the Talis Platform.  This means that any operations that require capabilities not available to unauthenticated users will fail.</remarks>
        public TalisPlatformConnector(String storeName)
        {
            this._storename = storeName;
            this._baseuri = TalisAPIBaseURI + this._storename + "/";
            this._hasCredentials = false;
        }

        #region Describe

        /// <summary>
        /// Gets the Graph describing the given resource from the Store
        /// </summary>
        /// <param name="resourceUri">URI of Resource to Describe</param>
        /// <param name="g">Graph to load the description into</param>
        public void Describe(IGraph g, String resourceUri)
        {
            if (!resourceUri.Equals(String.Empty))
            {
                this.DescribeInternal(g, resourceUri, "meta");
            }
        }

        /// <summary>
        /// Gets the Graph describing the given resource from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="resourceUri">URI of Resource to Describe</param>
        public void Describe(IRdfHandler handler, String resourceUri)
        {
            if (!resourceUri.Equals(String.Empty))
            {
                this.DescribeInternal(handler, resourceUri, "meta");
            }
        }

        /// <summary>
        /// Gets the Graph describing the given resource from the Store
        /// </summary>
        /// <param name="resourceUri">Uri of Resource to Describe</param>
        /// <param name="g">Graph to load the description into</param>
        public void Describe(IGraph g, Uri resourceUri)
        {
            if (resourceUri != null)
            {
                this.Describe(g, resourceUri.ToString());
            }
        }

        /// <summary>
        /// Gets the Graph describing the given resource from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="resourceUri">URI of Resource to Describe</param>
        public void Describe(IRdfHandler handler, Uri resourceUri)
        {
            if (!resourceUri.Equals(String.Empty))
            {
                this.DescribeInternal(handler, resourceUri.ToSafeString(), "meta");
            }
        }

        /// <summary>
        /// Gets the Graph describing the given resource from a particular Private Graph in the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="privateGraphID">ID of the Private Graph</param>
        /// <param name="resourceUri">URI of the Resource to Describe</param>
        public void Describe(IGraph g, String privateGraphID, String resourceUri)
        {
            if (!resourceUri.Equals(String.Empty))
            {
                this.DescribeInternal(g, resourceUri, "meta/graphs/" + privateGraphID);
            }
        }

        /// <summary>
        /// Gets the Graph describing the given resource from a particular Private Graph in the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="privateGraphID">ID of the Private Graph</param>
        /// <param name="resourceUri">URI of the Resource to Describe</param>
        public void Describe(IRdfHandler handler, String privateGraphID, String resourceUri)
        {
            if (!resourceUri.Equals(String.Empty))
            {
                this.DescribeInternal(handler, resourceUri, "meta/graphs/" + privateGraphID);
            }
        }

        /// <summary>
        /// Gets the Graph describing the given resource from a particular Private Graph in the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="privateGraphID">ID of the Private Graph</param>
        /// <param name="resourceUri">Uri of the Resource to Describe</param>
        public void Describe(IGraph g, String privateGraphID, Uri resourceUri)
        {
            if (resourceUri != null)
            {
                this.Describe(g, privateGraphID, resourceUri.ToString());
            }
        }

        /// <summary>
        /// Gets the Graph describing the given resource from a particular Private Graph in the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="privateGraphID">ID of the Private Graph</param>
        /// <param name="resourceUri">URI of the Resource to Describe</param>
        public void Describe(IRdfHandler handler, String privateGraphID, Uri resourceUri)
        {
            if (resourceUri != null)
            {
                this.Describe(handler, privateGraphID, resourceUri.ToString());
            }
        }        

        /// <summary>
        /// Internal implementation of the Describe function which can describe a resource at various service paths
        /// </summary>
        /// <param name="g">Graph to load description into</param>
        /// <param name="resourceUri">Uri of resource to describe</param>
        /// <param name="servicePath">Service to get the resource from</param>
        private void DescribeInternal(IGraph g, String resourceUri, String servicePath)
        {
            if (g.IsEmpty) g.BaseUri = new Uri(resourceUri);
            this.DescribeInternal(new GraphHandler(g), resourceUri, servicePath);
        }

        private void DescribeInternal(IRdfHandler handler, String resourceUri, String servicePath)
        {
            //Single about param
            Dictionary<String, String> ps = new Dictionary<string, string>();
            ps.Add("about", resourceUri);

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                //Get the Request Object
                request = this.CreateRequest(servicePath, ps);
                request.Method = "GET";
                request.Accept = MimeTypesHelper.HttpAcceptHeader;

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif

                    //Get the relevant Parser
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    //Got a Response so we can analyse the Response Code
                    response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    throw Error(code, webEx);
                }
                //Didn't get a Response
                throw;
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Adds a Graph to the Store
        /// </summary>
        /// <param name="g">Graph to add</param>
        public void Add(IGraph g)
        {
            this.AddInternal(g, "meta");
        }

        /// <summary>
        /// Adds a Graph to a Private Graph in the Store
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="privateGraphID">Private Graph ID</param>
        public void Add(IGraph g, String privateGraphID)
        {
            this.AddInternal(g, "meta/graphs/" + privateGraphID);
        }

        /// <summary>
        /// Internal implementation of adding a Graphs content to the Store
        /// </summary>
        /// <param name="g">Graph to add to the Store</param>
        /// <param name="servicePath">Service at the Store to add to</param>
        private void AddInternal(IGraph g, String servicePath)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                //Create the Request
                request = this.CreateRequest(servicePath, new Dictionary<string, string>());
                request.Method = "POST";
                request.ContentType = MimeTypesHelper.RdfXml[0];

                //Write the RDF/XML to the Request Stream
                RdfXmlWriter writer = new RdfXmlWriter();
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif
                
                //Make the Request
                using (response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //OK if we get here!
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    //Got a Response so we can analyse the Response Code
                    response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    throw Error(code, webEx);
                }
                //Didn't get a Response
                throw;
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the Store
        /// </summary>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public TalisUpdateResult Update(IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            return this.UpdateInternal(additions, removals, "meta");
        }

        /// <summary>
        /// Updates a Private Graph in the Store
        /// </summary>
        /// <param name="privateGraphID">Private Graph ID</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public TalisUpdateResult Update(String privateGraphID, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            return this.UpdateInternal(additions, removals, "meta/graphs/" + privateGraphID);
        }

        /// <summary>
        /// Updates the Store using a Versioned Update
        /// </summary>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public TalisUpdateResult UpdateVersioned(IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            return this.UpdateInternal(additions, removals, "meta/changesets");
        }

        /// <summary>
        /// Updates a Private Graph in the Store using a Versioned Update
        /// </summary>
        /// <param name="privateGraphID">Private Graph ID</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public TalisUpdateResult UpdateVersioned(String privateGraphID, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            return this.UpdateInternal(additions, removals, "meta/graphs/" + privateGraphID + "/changesets");
        }

        /// <summary>
        /// Internal implementation of Updating a Store by POSTing a ChangeSet to it
        /// </summary>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="servicePath">Service to post ChangeSet to</param>
        /// <returns></returns>
        private TalisUpdateResult UpdateInternal(IEnumerable<Triple> additions, IEnumerable<Triple> removals, String servicePath)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                //Generate the ChangeSet Batch
                IGraph g = this.GenerateChangeSet(additions, removals);
                if (g == null) return TalisUpdateResult.NotRequired; //Null so no changes need persisting
                if (g.IsEmpty) return TalisUpdateResult.NotRequired; //Empty so no changes need persisting

                //Create the Request
                request = this.CreateRequest(servicePath, new Dictionary<string, string>());
                request.Method = "POST";
                request.ContentType = TalisChangeSetMIMEType;

                //Write the RDF/XML to the Request Stream
                RdfXmlWriter writer = new RdfXmlWriter();
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                //Make the Request
                using (response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif

                    //What sort of Update Result did we get?
                    int code = (int)response.StatusCode;
                    response.Close();
                    if (code == 200 || code == 201)
                    {
                        return TalisUpdateResult.Synchronous;
                    }
                    else if (code == 202)
                    {
                        return TalisUpdateResult.Asynchronous;
                    }
                    else if (code == 204)
                    {
                        return TalisUpdateResult.Done;
                    }
                    else
                    {
                        return TalisUpdateResult.Unknown;
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    //Got a Response so we can analyse the Response Code
                    response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    throw Error(code, webEx);
                }
                //Didn't get a Response
                throw;
            }
        }

        /// <summary>
        /// Takes lists of Triples added and removed and generates a ChangeSet Batch Graph for these
        /// </summary>
        /// <param name="additions">Triple added</param>
        /// <param name="removals">Triples removed</param>
        /// <returns>Null if there are no Changes to be persisted</returns>
        private IGraph GenerateChangeSet(IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            //Ensure there are no duplicates in the lists
            List<Triple> toAdd = (additions == null) ? new List<Triple>() : additions.Distinct().ToList();
            List<Triple> toRemove = (removals == null) ? new List<Triple>() : removals.Distinct().ToList();

            //Eliminate any additions that have also been retracted
            List<Triple> temp = new List<Triple>();
            foreach (Triple t in toAdd)
            {
                if (toRemove.Contains(t))
                {
                    temp.Add(t);
                }
            }
            //If it was in both lists we don't need to persist the Change as it has effectively not happened
            toAdd.RemoveAll(t => temp.Contains(t));
            toRemove.RemoveAll(t => temp.Contains(t));

            //Nothing to do if both lists are now empty
            if (toAdd.Count == 0 && toRemove.Count == 0) return null;

            //Now we need to build a ChangeSet Graph
            Graph g = new Graph();
            g.BaseUri = new Uri("http://www.dotnetrdf.org/");
            g.NamespaceMap.AddNamespace("cs", new Uri(TalisChangeSetNamespace));

            //Make all the Nodes we need
            IUriNode rdfType = g.CreateUriNode("rdf:type");
            IUriNode changeSet = g.CreateUriNode("cs:ChangeSet");
            IUriNode subjOfChange = g.CreateUriNode("cs:subjectOfChange");
            IUriNode createdDate = g.CreateUriNode("cs:createdDate");
            ILiteralNode now = g.CreateLiteralNode(DateTime.Now.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat));
            IUriNode creator = g.CreateUriNode("cs:creatorName");
            ILiteralNode dotNetRDF = g.CreateLiteralNode("dotNetRDF");
            IUriNode changeReason = g.CreateUriNode("cs:changeReason");
            ILiteralNode dotNetRDFUpdate = g.CreateLiteralNode("Updates to the store were requested by a dotNetRDF powered application");
            IUriNode precedingChangeset = g.CreateUriNode("cs:precedingChangeSet");
            IUriNode removal = g.CreateUriNode("cs:removal");
            IUriNode addition = g.CreateUriNode("cs:addition");
            IUriNode rdfStmt = g.CreateUriNode("rdf:Statement");
            IUriNode rdfSubj = g.CreateUriNode("rdf:subject");
            IUriNode rdfPred = g.CreateUriNode("rdf:predicate");
            IUriNode rdfObj = g.CreateUriNode("rdf:object");

            //Find the Distinct Subjects from the list
            IEnumerable<INode> subjects = (from t in toAdd select t.Subject).Concat(from t in toRemove select t.Subject).Distinct();
            foreach (INode subj in subjects)
            {
                //Create a ChangeSet for this Subject
                IUriNode report = g.CreateUriNode(new Uri(Tools.ResolveUri(subj.GetHashCode() + "/changes/" + DateTime.Now.ToString(TalisChangeSetIDFormat), g.BaseUri.ToString())));
                g.Assert(new Triple(report, rdfType, changeSet));
                g.Assert(new Triple(report, subjOfChange, Tools.CopyNode(subj, g)));
                g.Assert(new Triple(report, createdDate, now));
                g.Assert(new Triple(report, creator, dotNetRDF));
                g.Assert(new Triple(report, changeReason, dotNetRDFUpdate));

                //Add Additions to this ChangeSet
                foreach (Triple t in toAdd.Where(t2 => t2.Subject.Equals(subj)))
                {
                    IBlankNode b = g.CreateBlankNode();
                    g.Assert(new Triple(report, addition, b));
                    g.Assert(new Triple(b, rdfType, rdfStmt));
                    g.Assert(new Triple(b, rdfSubj, Tools.CopyNode(t.Subject, g)));
                    g.Assert(new Triple(b, rdfPred, Tools.CopyNode(t.Predicate, g)));
                    g.Assert(new Triple(b, rdfObj, Tools.CopyNode(t.Object, g)));
                }

                //Add Removals to this ChangeSet
                foreach (Triple t in toRemove.Where(t2 => t2.Subject.Equals(subj)))
                {
                    IBlankNode b = g.CreateBlankNode();
                    g.Assert(new Triple(report, removal, b));
                    g.Assert(new Triple(b, rdfType, rdfStmt));
                    g.Assert(new Triple(b, rdfSubj, Tools.CopyNode(t.Subject, g)));
                    g.Assert(new Triple(b, rdfPred, Tools.CopyNode(t.Predicate, g)));
                    g.Assert(new Triple(b, rdfObj, Tools.CopyNode(t.Object, g)));
                }
            }

            return g;
        }

        #endregion

        #region Query

        /// <summary>
        /// Makes a SPARQL query against the Talis Store Metabox using the Store SPARQL Service
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns>Either a Result Set or a Graph depending on the type of Sparql Query</returns>
        /// <remarks>
        /// The SPARQL Query will be parsed locally by the libararies <see cref="SparqlQueryParser">SparqlQueryParser</see> to ensure that it is a valid Query, this adds some extra overhead but potentially saves the overhead of submitting a malformed Sparql query via a <see cref="HttpWebRequest">HttpWebRequest</see> to the Talis Platform
        /// </remarks>
        public Object Query(String query)
        {
            return this.QueryInternal(query, "services/sparql");
        }

        /// <summary>
        /// Makes a SPARQL query against the Talis Store Metabox using the Store SPARQL Service processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <remarks>
        /// The SPARQL Query will be parsed locally by the libraries <see cref="SparqlQueryParser">SparqlQueryParser</see> to ensure that it is a valid Query, this adds some extra overhead but potentially saves the overhead of submitting a malformed Sparql query via a <see cref="HttpWebRequest">HttpWebRequest</see> to the Talis Platform
        /// </remarks>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query)
        {
            this.QueryInternal(rdfHandler, resultsHandler, query, "services/sparql");
        }

        /// <summary>
        /// Makes a SPARQL query against the Talis Store Metabox and Private Graphs using the Store Multi-SPARQL Service
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <returns>Either a Result Set or a Graph depending on the type of Sparql Query</returns>
        /// <remarks>
        /// The SPARQL Query will be parsed locally by the libraries <see cref="SparqlQueryParser">SparqlQueryParser</see> to ensure that it is a valid Query, this adds some extra overhead but potentially saves the overhead of submitting a malformed Sparql query via a <see cref="HttpWebRequest">HttpWebRequest</see> to the Talis Platform
        /// </remarks>
        public Object QueryAll(String query)
        {
            return this.QueryInternal(query, "services/multisparql");
        }

        /// <summary>
        /// Makes a SPARQL query against the Talis Store Metabox and Private Graphs using the Store SPARQL Service processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query</param>
        /// <remarks>
        /// The SPARQL Query will be parsed locally by the libraries <see cref="SparqlQueryParser">SparqlQueryParser</see> to ensure that it is a valid Query, this adds some extra overhead but potentially saves the overhead of submitting a malformed Sparql query via a <see cref="HttpWebRequest">HttpWebRequest</see> to the Talis Platform
        /// </remarks>
        public void QueryAll(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query)
        {
            this.QueryInternal(rdfHandler, resultsHandler, query, "services/multisparql");
        }

        /// <summary>
        /// Internal implementation of querying the Store 
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <param name="servicePath">Service to Query</param>
        /// <returns></returns>
        private Object QueryInternal(String query, String servicePath)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.QueryInternal(new GraphHandler(g), new ResultSetHandler(results), query, servicePath);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }
            else
            {
                return g;
            }
        }

        private void QueryInternal(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query, String servicePath)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                //Single Query parameter
                Dictionary<String, String> ps = new Dictionary<string, string>();
                ps.Add("query", query);

                //Parse the query locally to validate it and so we can decide what to do
                //when we receive the Response more easily as we'll know the query type
                //This also saves us wasting a HttpWebRequest on a malformed query
                SparqlQueryParser qparser = new SparqlQueryParser();
                SparqlQuery q = qparser.ParseFromString(query);

                //Get the Request
                request = this.CreateRequest(servicePath, ps);
                request.Method = "GET";

                switch (q.QueryType)
                {
                    case SparqlQueryType.Ask:
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        //Some kind of Sparql Result Set
                        request.Accept = MimeTypesHelper.HttpSparqlAcceptHeader;

#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugRequest(request);
                        }
#endif

                        using (response = (HttpWebResponse)request.GetResponse())
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
                            ISparqlResultsReader resultsParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                            resultsParser.Load(resultsHandler, new StreamReader(response.GetResponseStream()));
                            response.Close();
                        }
                        break;

                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        //Some kind of Graph
                        //HACK: Have to send only RDF/XML as the accept header due to a known issue with Talis Platform
                        request.Accept = MimeTypesHelper.RdfXml[0];//MimeTypesHelper.HttpAcceptHeader;

#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugRequest(request);
                        }
#endif

                        using (response = (HttpWebResponse)request.GetResponse())
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
                            IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                            parser.Load(rdfHandler, new StreamReader(response.GetResponseStream()));
                            response.Close();
                        }
                        break;

                    default:
                        //Error
                        throw new RdfQueryException("Unknown Query Type was used, unable to determine how to process the response from Talis");
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    //Got a Response so we can analyse the Response Code
                    response = (HttpWebResponse)webEx.Response;
                    int code = (int)response.StatusCode;
                    throw Error(code, webEx);
                }
                //Didn't get a Response
                throw;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method which sets up the basic Request to the Talis Platform
        /// </summary>
        /// <param name="servicePath">Path to the service relative to the Base Uri for the Store</param>
        /// <param name="serviceParams">Querystring parameters to be added to the Request</param>
        /// <returns>A HTTP Request to the appropriate Talis service with authentication credentials added, no Method or other Headers are set so the calling function needs to configure the Request to perform the correct type of HTTP operation</returns>
        protected HttpWebRequest CreateRequest(String servicePath, Dictionary<String, String> serviceParams)
        {
            //Build the Request Uri
            String requestUri = this._baseuri + servicePath;
            if (serviceParams.Count > 0)
            {
                requestUri += "?";
                foreach (String p in serviceParams.Keys) 
                {
                    requestUri += p + "=" + Uri.EscapeDataString(serviceParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length-1);
            }

            //Create the Request Object
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);

            //Create the Request Credentials
            if (this._hasCredentials)
            {
                NetworkCredential credentials = new NetworkCredential(this._username, this._password);
                request.Credentials = credentials;
            }

            return request;
        }

        /// <summary>
        /// Helper method for determining the reason for an error received from the Talis platform based on the HTTP Response Code
        /// </summary>
        /// <param name="code">HTTP Response Code</param>
        /// <param name="ex">Exception to be handled</param>
        /// <returns></returns>
        protected TalisException Error(int code, WebException ex)
        {
#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugResponse((HttpWebResponse)ex.Response);
            }
#endif
            switch (code)
            {
                case 400:
                    //Response should contain a message telling us what was wrong
                    String data = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    return new TalisException("Required Parameter Missing/Malformed: " + data, ex);
                case 403:
                    return new TalisException("Access to this service is not permitted using the provided Credentials", ex);
                case 405:
                    return new TalisException("Service doesn't support the HTTP Method used", ex);
                case 406:
                    return new TalisException("Requested Content Types are not available from this Service", ex);
                case 413:
                    return new TalisException("Request was too large for this Service and was rejected", ex);
                case 415:
                    return new TalisException("Content submitted to Talis was in an unsupported format", ex);
                case 422:
                    return new TalisException("RDF submitted to Talis is not valid", ex);
                case 500:
                    return new TalisException("Talis reported an Internal Server Error, your request may have timed out", ex);
                case 507:
                    return new TalisException("Talis Platform has insufficient storage to process the Request at this time", ex);
                default:
                    return new TalisException("Unknown Error accessing the Talis Platform", ex);
            }
        }

        #endregion

        #region Generic IO Manager Implementation

        /// <summary>
        /// Loads a Graph which is the Description of the given URI from the Metabox of the Talis Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If there are no Triples with the given URI as the Subject then no Triples will be loaded into the given Graph
        /// <br /><br />
        /// Equivalent to calling the <see cref="TalisPlatformConnector.Describe">Describe()</see> method
        /// </remarks>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph which is the Description of the given URI from the Metabox of the Talis Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If there are no Triples with the given URI as the Subject then no Triples will be loaded into the given Graph
        /// <br /><br />
        /// Equivalent to calling the <see cref="TalisPlatformConnector.Describe">Describe()</see> method
        /// </remarks>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph which is the Description of the given URI from the Metabox of the Talis Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If there are no Triples with the given URI as the Subject then no Triples will be loaded into the given Graph
        /// <br /><br />
        /// Equivalent to calling the <see cref="TalisPlatformConnector.Describe">Describe()</see> method
        /// </remarks>
        public void LoadGraph(IGraph g, String graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty)) throw new TalisException("Cannot load the Description of a null/empty URI");
            this.Describe(g, graphUri);
        }

        /// <summary>
        /// Loads a Graph which is the Description of the given URI from the Metabox of the Talis Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If there are no Triples with the given URI as the Subject then no Triples will be loaded into the given Graph
        /// <br /><br />
        /// Equivalent to calling the <see cref="TalisPlatformConnector.Describe">Describe()</see> method
        /// </remarks>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty)) throw new TalisException("Cannot load the Description of a null/empty URI");
            this.Describe(handler, graphUri);
        }

        /// <summary>
        /// Saves a Graph into the Metabox of the Talis Store
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// The Metabox of a Talis Store is a single Graph so the contents of the given Graph will be added to the existing content of the Metabox
        /// <br /><br />
        /// Equivalent to calling the <see cref="TalisPlatformConnector.Add">Add()</see> method
        /// </remarks>
        public void SaveGraph(IGraph g)
        {
            this.Add(g);
        }

        /// <summary>
        /// Gets the IO Behaviour of the Store
        /// </summary>
        public IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.TripleStore | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Updates the Metabox of the Talis Store using unversioned update
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// The Metabox of a Talis Store is a single Graph so the updates apply to the Metabox and not to a specific Graph i.e. the <paramref name="graphUri"/> parameter is ignored
        /// <br /><br />
        /// Equivalent to calling the <see cref="TalisPlatformConnector.Update">Update()</see> method
        /// </remarks>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.Update(additions, removals);
        }

        /// <summary>
        /// Updates the Metabox of the Talis Store using unversioned update
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// The Metabox of a Talis Store is a single Graph so the updates apply to the Metabox and not to a specific Graph i.e. the <paramref name="graphUri"/> parameter is ignored
        /// <br /><br />
        /// Equivalent to calling the <see cref="TalisPlatformConnector.Update">Update()</see> method
        /// </remarks>
        public void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.Update(additions, removals);
        }

        /// <summary>
        /// Indicates that Updates are supported by the Talis Platform
        /// </summary>
        public bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Throws an exception since you cannot delete a Graph from a Talis Store as it is not a named graph store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph from the Talis Platform as it is not a named graph store</exception>
        public void DeleteGraph(Uri graphUri)
        {
            throw new RdfStorageException("The TalisConnector does not support deletion of Graphs since the Talis platform is not a named graph store");
        }

        /// <summary>
        /// Throws an exception since you cannot delete a Graph from a Talis Store as it is not a named graph store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since you cannot delete a Graph from the Talis Platform as it is not a named graph store</exception>
        public void DeleteGraph(String graphUri)
        {
            throw new RdfStorageException("The TalisConnector does not support deletion of Graphs since the Talis platform is not a named graph store");
        }

        /// <summary>
        /// Returns that deleting a Graph is not supported
        /// </summary>
        public bool DeleteSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Throws an exception since listing graphs from the Talis Platform is not supported as it is not a named graph store
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Uri> ListGraphs()
        {
            throw new NotSupportedException("Talis Platform does not support listing Graphs");
        }

        /// <summary>
        /// Returns that listing graphs is not supported
        /// </summary>
        public bool ListGraphsSupported
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

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the Talis Platform Connector
        /// </summary>
        public void Dispose()
        {
            //No dispose needed
        }

        #endregion

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Talis] " + this._storename;
        }


        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode store = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyStore);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._storename)));

            if (this._username != null && this._password != null)
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._password)));
            }
        }
    }
}

#endif