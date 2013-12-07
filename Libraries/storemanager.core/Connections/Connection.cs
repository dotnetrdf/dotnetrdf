using System;
using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Container class that holds all the necessary information about a connection
    /// </summary>
    public class Connection
    {
        private String _name;

        /// <summary>
        /// Creates a new connection which was initially be in the closed state
        /// </summary>
        /// <param name="definition">Definition</param>
        public Connection(IConnectionDefinition definition)
            : this(definition, CreateRootUri()) { }

        /// <summary>
        /// Creates a new connection which was initially be in the closed state
        /// </summary>
        /// <param name="definition">Definition</param>
        /// <param name="rootUri">Root URI</param>
        public Connection(IConnectionDefinition definition, Uri rootUri)
        {
            if (ReferenceEquals(definition, null)) throw new ArgumentNullException("definition");
            if (ReferenceEquals(rootUri, null)) throw new ArgumentNullException("rootUri");
            this.Definition = definition;
            this.RootUri = rootUri;
        }

        /// <summary>
        /// Creates a fresh root URI as a UUID based URI
        /// </summary>
        /// <returns>Fresh root URI</returns>
        public static Uri CreateRootUri()
        {
            Guid uuid;
            do
            {
                uuid = Guid.NewGuid();
            } while (uuid.Equals(Guid.Empty));
            return new Uri("urn:uuid:" + uuid.ToString());
        }

        /// <summary>
        /// Gets the connection definition used to create the connection
        /// </summary>
        public IConnectionDefinition Definition { get; private set; }

        /// <summary>
        /// Gets the storage provider
        /// </summary>
        /// <remarks>
        /// Will typically only be available if the connection is currently open
        /// </remarks>
        public IStorageProvider StorageProvider { get; private set; }

        /// <summary>
        /// Gets the connection information
        /// </summary>
        /// <remarks>
        /// Only applicable for open connections, otherwise a <see cref="NotSupportedException"/> is thrown
        /// </remarks>
        public ConnectionInfo Information
        {
            get
            {
                if (!ReferenceEquals(this.StorageProvider, null)) return new ConnectionInfo(this.StorageProvider);
                throw new InvalidOperationException("Cannot access connection information for a closed connection");
            }
        }

        /// <summary>
        /// Root URI used for serializing the connection information
        /// </summary>
        public Uri RootUri { get; private set; }

        /// <summary>
        /// Gets/Sets the friendly name associated with this connection
        /// </summary>
        public String Name
        {
            get
            {
                if (!ReferenceEquals(this._name, null)) return this._name;
                return !ReferenceEquals(this.StorageProvider, null) ? this.StorageProvider.ToString() : this.Definition.StoreName;
            }
            set { this._name = value; }
        }

        /// <summary>
        /// Gets/Sets the created date
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Gets/Sets the last modified date
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        /// <summary>
        /// Gets/Sets the last opened date
        /// </summary>
        public DateTimeOffset LastOpened { get; set; }

        /// <summary>
        /// Gets whether the connection is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return !ReferenceEquals(this.StorageProvider, null); }
        }

        /// <summary>
        /// Opens the connection if it is not already open
        /// </summary>
        public void Open()
        {
            if (!ReferenceEquals(this.StorageProvider, null)) return;
            this.StorageProvider = this.Definition.OpenConnection();
        }

        /// <summary>
        /// Closes the connection if it is not already closed
        /// </summary>
        public void Close()
        {
            if (ReferenceEquals(this.StorageProvider, null)) return;
            this.StorageProvider.Dispose();
            this.StorageProvider = null;
        }

        /// <summary>
        /// Saves the configuration for this connection to the given graph
        /// </summary>
        /// <param name="g">Graph</param>
        public void SaveConfiguration(IGraph g)
        {
            if (ReferenceEquals(this.StorageProvider, null)) throw new InvalidOperationException("Cannot save the configuration of a closed connection");

            IConfigurationSerializable serializable = this.StorageProvider as IConfigurationSerializable;
            if (ReferenceEquals(serializable, null)) throw new InvalidOperationException("The underlying connection does not support serializing its configuration");

            ConfigurationSerializationContext context = new ConfigurationSerializationContext(g);
            INode rootNode = context.Graph.CreateUriNode(this.RootUri);
            // Remove any previous saved configuration
            // Note that this may leave some orphaned configuration information for complex configurations but since that would be linked by blank nodes it won't matter
            context.Graph.Retract(context.Graph.GetTriplesWithSubject(rootNode).ToList());

            // Serialize the new configuration state
            context.NextSubject = rootNode;
            serializable.SerializeConfiguration(context);

            // Add additional information
            context.Graph.NamespaceMap.AddNamespace("rdfs", UriFactory.Create(NamespaceMapper.RDFS));
            INode rdfsLabel = context.Graph.CreateUriNode("rdfs:label");
            context.Graph.Assert(rootNode, rdfsLabel, context.Graph.CreateLiteralNode(this.Name));
            // TODO Serialize date information
        }

        /// <summary>
        /// Loads ancillary information such as name and modified dates from the given graph
        /// </summary>
        /// <param name="g">Graph</param>
        public void LoadInformation(IGraph g)
        {
            throw new NotImplementedException();
        }
    }
}