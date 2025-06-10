/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage;

/// <summary>
/// Class for connecting to any SPARQL server that provides both a query and update endpoint.
/// </summary>
/// <remarks>
/// <para>
/// This class is a wrapper around a <see cref="SparqlRemoteEndpoint"/> and a <see cref="SparqlRemoteUpdateEndpoint"/>.  The former is used for the query functionality while the latter is used for the update functionality.  As updates happen via SPARQL the behaviour with respects to adding and removing blank nodes will be somewhat up to the underlying SPARQL implementation.  This connector is <strong>not</strong> able to carry out <see cref="IStorageProvider.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple})"/> operations which attempt to delete blank nodes and cannot guarantee that added blank nodes bear any relation to existing blank nodes in the store.
/// </para>
/// <para>
/// Unlike other HTTP based connectors this connector does not derive from <see cref="BaseAsyncHttpConnector">BaseHttpConnector</see> - if you need to specify proxy information you should do so on the SPARQL Endpoint you are wrapping either by providing endpoint instance pre-configured with the proxy settings or by accessing the endpoint via the <see cref="ReadWriteSparqlConnector">Endpoint</see> and <see cref="ReadWriteSparqlConnector.UpdateEndpoint">UpdateEndpoint</see> properties and programmatically adding the settings.
/// </para>
/// </remarks>
public class ReadWriteSparqlConnector
    : SparqlConnector, IUpdateableStorage
{
    private readonly TurtleFormatter _formatter = new ();
#pragma warning disable 618
    private readonly SparqlRemoteUpdateEndpoint _updateEndpoint;
#pragma warning restore 618

    /// <summary>
    /// Creates a new connector.
    /// </summary>
    /// <param name="queryClient">The SPARQL query client to use.</param>
    /// <param name="updateClient">The SPARQL update client to use.</param>
    /// <param name="loadMethod">The method to use for loading graphs.</param>
    public ReadWriteSparqlConnector(SparqlQueryClient queryClient, SparqlUpdateClient updateClient,
        SparqlConnectorLoadMethod loadMethod = SparqlConnectorLoadMethod.Construct) : base(queryClient, loadMethod)
    {
        UpdateClient = updateClient ??
                       throw new ArgumentNullException(nameof(updateClient), "Update client must not be null.");
    }
    
    /// <summary>
    /// Creates a new connection.
    /// </summary>
    /// <param name="queryEndpoint">Query Endpoint.</param>
    /// <param name="updateEndpoint">Update Endpoint.</param>
    /// <param name="mode">Method for loading graphs.</param>
    [Obsolete("Replaced by ReadWriteSparqlConnector(SparqlQueryClient, SparqlUpdateClient, SparqlConnectorLoadMethod")]
    public ReadWriteSparqlConnector(SparqlRemoteEndpoint queryEndpoint, SparqlRemoteUpdateEndpoint updateEndpoint, SparqlConnectorLoadMethod mode)
        : base(queryEndpoint, mode)
    {
        _updateEndpoint = updateEndpoint ?? throw new ArgumentNullException(nameof(updateEndpoint), "Update Endpoint cannot be null, if you require a read-only SPARQL connector use the base class SparqlConnector instead");
    }

    /// <summary>
    /// Creates a new connection.
    /// </summary>
    /// <param name="queryEndpoint">Query Endpoint.</param>
    /// <param name="updateEndpoint">Update Endpoint.</param>
    [Obsolete("Replaced by ReadWriteSparqlConnector(SparqlQueryClient, SparqlUpdateClient, SparqlConnectorLoadMethod")]
    public ReadWriteSparqlConnector(SparqlRemoteEndpoint queryEndpoint, SparqlRemoteUpdateEndpoint updateEndpoint)
        : this(queryEndpoint, updateEndpoint, SparqlConnectorLoadMethod.Construct) { }

    /// <summary>
    /// Creates a new connection.
    /// </summary>
    /// <param name="queryEndpoint">Query Endpoint.</param>
    /// <param name="updateEndpoint">Update Endpoint.</param>
    /// <param name="mode">Method for loading graphs.</param>
    public ReadWriteSparqlConnector(Uri queryEndpoint, Uri updateEndpoint, SparqlConnectorLoadMethod mode)
        : this(new SparqlQueryClient(new HttpClient(), queryEndpoint), new SparqlUpdateClient(new HttpClient(), updateEndpoint), mode) { }

    /// <summary>
    /// Creates a new connection.
    /// </summary>
    /// <param name="queryEndpoint">Query Endpoint.</param>
    /// <param name="updateEndpoint">Update Endpoint.</param>
    public ReadWriteSparqlConnector(Uri queryEndpoint, Uri updateEndpoint)
        : this(queryEndpoint, updateEndpoint, SparqlConnectorLoadMethod.Construct) { }

    /// <summary>
    /// Gets the underlying <see cref="SparqlRemoteUpdateEndpoint">SparqlRemoteUpdateEndpoint</see> which this class is a wrapper around.
    /// </summary>
    [Description("The Remote Update Endpoint to which queries are sent using HTTP."), TypeConverter(typeof(ExpandableObjectConverter))]
    [Obsolete]
    public SparqlRemoteUpdateEndpoint UpdateEndpoint
    {
        get
        {
            return _updateEndpoint;
        }
    }

    /// <summary>
    /// Gets the underlying <see cref="SparqlUpdateClient"/> which this class is a wrapper around.
    /// </summary>
    public SparqlUpdateClient UpdateClient { get; }
    
    /// <summary>
    /// Gets/Sets the HTTP Timeout in milliseconds used for communicating with the SPARQL Endpoint.
    /// </summary>
    [Obsolete("This property is only used by the obsolete SparqlRemoteEndpoint-backed implementation.")]
    public override int Timeout
    {
        get
        {
            return _timeout;
        }
        set
        {
            _timeout = value;
            _endpoint.Timeout = value;
            _updateEndpoint.Timeout = value;
        }
    }

    /// <summary>
    /// Gets that deleting graphs is supported.
    /// </summary>
    public override bool DeleteSupported
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Gets that the store is not read-only.
    /// </summary>
    public override bool IsReadOnly
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the IO behaviour for the store.
    /// </summary>
    public override IOBehaviour IOBehaviour
    {
        get
        {
            return IOBehaviour.IsQuadStore | IOBehaviour.HasDefaultGraph | IOBehaviour.HasNamedGraphs | IOBehaviour.CanUpdateTriples | IOBehaviour.OverwriteTriples | IOBehaviour.OverwriteDefault | IOBehaviour.OverwriteNamed;
        }
    }

    /// <summary>
    /// Gets that triple level updates are supported, see the remarks section of the <see cref="ReadWriteSparqlConnector"/> for exactly what is and isn't supported.
    /// </summary>
    public override bool UpdateSupported
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Deletes a graph from the store.
    /// </summary>
    /// <param name="graphUri">URI of the graph to delete.</param>
    public override void DeleteGraph(string graphUri)
    {
        DeleteGraph(graphUri.ToSafeUri());
    }

    /// <summary>
    /// Deletes a graph from the store.
    /// </summary>
    /// <param name="graphUri">URI of the graph to delete.</param>
    public override void DeleteGraph(Uri graphUri)
    {
        if (graphUri == null)
        {
            Update("DROP DEFAULT");
        }
        else
        {
            Update("DROP GRAPH <" + _formatter.FormatUri(graphUri) + ">");
        }
    }

    /// <summary>
    /// Saves a graph to the store.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    public override void SaveGraph(IGraph g)
    {
        var updates = new StringBuilder();

        // Saving a Graph overwrites a previous graph so start with a CLEAR SILENT GRAPH
        if (g.BaseUri == null)
        {
            updates.AppendLine("CLEAR SILENT DEFAULT;");
        }
        else
        {
            updates.AppendLine("CLEAR SILENT GRAPH <" + _formatter.FormatUri(g.BaseUri) + ">;");
        }

        // Insert preamble
        // Note that we use INSERT { } WHERE { } rather than INSERT DATA { } so we can insert blank nodes
        if (g.BaseUri != null)
        {
            updates.AppendLine("WITH <" + _formatter.FormatUri(g.BaseUri) + ">");
        }
        updates.AppendLine("INSERT");
        updates.AppendLine("{");

        // Serialize triples
        foreach (Triple t in g.Triples)
        {
            updates.AppendLine(" " + _formatter.Format(t));
        }

        // End
        updates.AppendLine("} WHERE { }");

        // Save the graph
        Update(updates.ToString());
    }

    /// <summary>
    /// Updates a graph in the store.
    /// </summary>
    /// <param name="graphUri">URI of the graph to update.</param>
    /// <param name="additions">Triples to add.</param>
    /// <param name="removals">Triples to remove.</param>
    public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        UpdateGraph(graphUri.ToSafeUri(), additions, removals);
    }

    /// <summary>
    /// Updates a graph in the store.
    /// </summary>
    /// <param name="graphUri">URI of the graph to update.</param>
    /// <param name="additions">Triples to add.</param>
    /// <param name="removals">Triples to remove.</param>
    public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        var updates = new StringBuilder();

        if (additions != null)
        {
            if (additions.Any())
            {
                // Insert preamble
                // Note that we use INSERT { } WHERE { } rather than INSERT DATA { } so we can insert blank nodes
                if (graphUri != null)
                {
                    updates.AppendLine("WITH <" + _formatter.FormatUri(graphUri) + ">");
                }
                updates.AppendLine("INSERT");
                updates.AppendLine("{");

                // Serialize triples
                foreach (Triple t in additions)
                {
                    updates.AppendLine(" " + _formatter.Format(t));
                }

                // End
                updates.AppendLine("} WHERE { }");
                if (removals != null && removals.Any()) updates.AppendLine(";");
            }
        }
        if (removals != null)
        {
            if (removals.Any())
            {
                // Insert preamble
                // Note that we use DELETE DATA { } for deletes so we don't support deleting blank nodes
                updates.AppendLine("DELETE DATA");
                updates.AppendLine("{");

                if (graphUri != null)
                {
                    updates.AppendLine("GRAPH <" + _formatter.FormatUri(graphUri) + "> {");
                }

                // Serialize triples
                foreach (Triple t in removals)
                {
                    if (!t.IsGroundTriple) throw new RdfStorageException("The ReadWriteSparqlConnector does not support the deletion of blank node containing triples");
                    updates.AppendLine("  " + _formatter.Format(t));
                }

                // End
                if (graphUri != null) updates.AppendLine(" }");
                updates.AppendLine("}");
            }
        }

        // Make an update if necessary
        if (updates.Length > 0)
        {
            Update(updates.ToString());
        }
    }

    /// <summary>
    /// Makes a SPARQL Update against the store.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    public void Update(string sparqlUpdate)
    {
        if (UpdateClient != null)
        {
            UpdateClient.UpdateAsync(sparqlUpdate).Wait();
        }
        else
        {
            _updateEndpoint.Update(sparqlUpdate);
        }
    }

    /// <summary>
    /// Gets a String which gives details of the Connection.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "[SPARQL Query & Update] Query: " + _endpoint.Uri.AbsoluteUri + " Update: " + _updateEndpoint.Uri.AbsoluteUri;
    }

    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        // Call base SerializeConfiguration() first
        INode manager = context.NextSubject;
        context.NextSubject = manager;
        base.SerializeConfiguration(context);
        context.NextSubject = manager;

        if (UpdateClient != null)
        {
            // Use the indirect serialization method

            // Serialize the Endpoints Configuration
            INode endpoint = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpoint));
            INode endpointObj = context.Graph.CreateBlankNode();
            context.NextSubject = endpointObj;
            UpdateClient.SerializeConfiguration(context);

            // Link that serialization to our serialization
            context.Graph.Assert(new Triple(manager, endpoint, endpointObj));
        }
        else if (_updateEndpoint != null)
        {
            // Use the indirect serialization method

            // Serialize the Endpoints Configuration
            INode endpoint = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpoint));
            INode endpointObj = context.Graph.CreateBlankNode();
            context.NextSubject = endpointObj;
            ((IConfigurationSerializable)_updateEndpoint).SerializeConfiguration(context);

            // Link that serialization to our serialization
            context.Graph.Assert(new Triple(manager, endpoint, endpointObj));
        }
        else
        {
            // Use the direct serialization method
            INode endpointUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri));

            context.Graph.Assert(new Triple(manager, endpointUri, context.Graph.CreateLiteralNode(_endpoint.Uri.AbsoluteUri)));
        }
    }
}