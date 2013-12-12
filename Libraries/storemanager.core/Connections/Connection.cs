/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.ComponentModel;
using System.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    /// <summary>
    /// Container class that holds all the necessary information about a connection
    /// </summary>
    public class Connection
        : IEquatable<Connection>, INotifyPropertyChanged
    {
        private DateTimeOffset _created, _modified;
        private bool _readOnly;

        /// <summary>
        /// Namespace URI for the Store Manager namespace
        /// </summary>
        public const String StoreManagerNamespace = "http://www.dotnetrdf.org/StoreManager#";

        private String _name;

        /// <summary>
        /// Creates a new connection as a copy of the existing connection
        /// </summary>
        /// <param name="connection">Connection</param>
        protected Connection(Connection connection)
        {
            this.Definition = connection.Definition.Copy();
            this.RootUri = CreateRootUri();
            this.Created = DateTimeOffset.UtcNow;
            this.LastModified = this.Created;
            this.LastOpened = null;
            this.IsReadOnly = connection.IsReadOnly;
            this.Name = "Copy of " + connection.Name;
        }

        /// <summary>
        /// Creates a new connection which will initially be in the closed state
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="rootUri">Root URI for the connections information</param>
        public Connection(IGraph g, Uri rootUri)
        {
            if (ReferenceEquals(rootUri, null)) throw new ArgumentNullException("rootUri");
            this.RootUri = rootUri;
            this.LoadConfiguration(g);
        }

        /// <summary>
        /// Creates a new connection which will initially be in the closed state
        /// </summary>
        /// <param name="definition">Definition</param>
        public Connection(IConnectionDefinition definition)
            : this(definition, CreateRootUri())
        {
        }

        /// <summary>
        /// Creates a new connection which will initially be in the closed state
        /// </summary>
        /// <param name="definition">Definition</param>
        /// <param name="rootUri">Root URI</param>
        public Connection(IConnectionDefinition definition, Uri rootUri)
        {
            if (ReferenceEquals(definition, null)) throw new ArgumentNullException("definition");
            if (ReferenceEquals(rootUri, null)) throw new ArgumentNullException("rootUri");
            this.Definition = definition;
            this.RootUri = rootUri;

            this.Created = DateTimeOffset.UtcNow;
            this.LastModified = this.Created;
            this.LastOpened = null;
            this.IsReadOnly = false;
        }

        /// <summary>
        /// Creates a connection which is in the open state
        /// </summary>
        /// <param name="definition">Definition</param>
        /// <param name="provider">Storage Provider</param>
        public Connection(IConnectionDefinition definition, IStorageProvider provider)
            : this(definition, provider, CreateRootUri()) { }

        /// <summary>
        /// Creates a connection which is in the open state
        /// </summary>
        /// <param name="definition">Definition</param>
        /// <param name="provider">Storage Provider</param>
        /// <param name="rootUri">Root URI</param>
        public Connection(IConnectionDefinition definition, IStorageProvider provider, Uri rootUri)
        {
            if (ReferenceEquals(definition, null)) throw new ArgumentNullException("definition");
            if (ReferenceEquals(rootUri, null)) throw new ArgumentNullException("rootUri");
            if (ReferenceEquals(provider, null)) throw new ArgumentNullException("provider");
            this.Definition = definition;
            this.RootUri = rootUri;
            this.StorageProvider = provider;

            this.Created = DateTimeOffset.UtcNow;
            this.LastModified = this.Created;
            this.LastOpened = this.Created;
            this.IsReadOnly = false;

            // Because when created this way the definition may pertain not to this specific connection
            // we need to serialize and re-populate the definition to ensure we have the correct information
            IGraph g = new Graph();
            this.SaveConfiguration(g);
            INode rootNode = g.CreateUriNode(this.RootUri);
            this.Definition.PopulateFrom(g, rootNode);
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
            set
            {
                if (!ReferenceEquals(value, null) && !value.Equals(this.Name)) this.LastModified = DateTimeOffset.UtcNow;
                this._name = value;
                this.RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets/Sets the created date
        /// </summary>
        public DateTimeOffset Created
        {
            get { return this._created; }
            private set
            {
                this._created = value;
                this.RaisePropertyChanged("Created");
            }
        }

        /// <summary>
        /// Gets/Sets the last modified date
        /// </summary>
        public DateTimeOffset LastModified
        {
            get { return this._modified; }
            private set
            {
                this._modified = value;
                this.RaisePropertyChanged("LastModified");
            }
        }

        /// <summary>
        /// Gets/Sets the last opened date
        /// </summary>
        public DateTimeOffset? LastOpened { get; private set; }

        /// <summary>
        /// Gets whether the connection is currently open
        /// </summary>
        public bool IsOpen
        {
            get { return !ReferenceEquals(this.StorageProvider, null); }
        }

        /// <summary>
        /// Gets whether this should be a read-only connection
        /// </summary>
        /// <remarks>
        /// This is used as the default setting when calling the no-argument <see cref="Open()"/> method
        /// </remarks>
        public bool IsReadOnly
        {
            get { return this._readOnly; }
            private set
            {
                if (value == this._readOnly) return;
                this._readOnly = value;
                this.RaisePropertyChanged("IsReadOnly");
            }
        }

        /// <summary>
        /// Opens the connection if it is not already open
        /// </summary>
        public void Open()
        {
            this.Open(this.IsReadOnly);
        }

        /// <summary>
        /// Opens the connection if it is not already open
        /// </summary>
        public void Open(bool readOnly)
        {
            if (!ReferenceEquals(this.StorageProvider, null)) return;
            this.IsReadOnly = readOnly;
            this.StorageProvider = this.Definition.OpenConnection();
            if (this.IsReadOnly && !this.StorageProvider.IsReadOnly)
            {
                if (this.StorageProvider is IQueryableStorage)
                {
                    this.StorageProvider = new QueryableReadOnlyConnector((IQueryableStorage) this.StorageProvider);
                }
                else
                {
                    this.StorageProvider = new ReadOnlyConnector(this.StorageProvider);
                }
            }
            this.LastOpened = DateTimeOffset.UtcNow;
            this.RaisePropertyChanged("LastOpened");
            this.RaisePropertyChanged("IsOpen");
        }

        /// <summary>
        /// Closes the connection if it is not already closed
        /// </summary>
        public void Close()
        {
            if (ReferenceEquals(this.StorageProvider, null)) return;
            this.StorageProvider.Dispose();
            this.StorageProvider = null;
            this.RaisePropertyChanged("IsOpen");
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
            // Friendly Name
            context.Graph.NamespaceMap.AddNamespace("rdfs", UriFactory.Create(NamespaceMapper.RDFS));
            INode rdfsLabel = context.Graph.CreateUriNode("rdfs:label");
            context.Graph.Assert(rootNode, rdfsLabel, context.Graph.CreateLiteralNode(this.Name));
            // Store Manager tracked information
            context.Graph.NamespaceMap.AddNamespace("store", UriFactory.Create(StoreManagerNamespace));
            context.Graph.Assert(rootNode, context.Graph.CreateUriNode("store:created"), this.Created.ToLiteral(context.Graph));
            context.Graph.Assert(rootNode, context.Graph.CreateUriNode("store:lastModified"), this.LastModified.ToLiteral(context.Graph));
            if (this.LastOpened.HasValue) context.Graph.Assert(rootNode, context.Graph.CreateUriNode("store:lastOpened"), this.LastOpened.Value.ToLiteral(context.Graph));
            context.Graph.Assert(rootNode, context.Graph.CreateUriNode("store:definitionType"), this.Definition.GetType().FullName.ToLiteral(context.Graph));
            context.Graph.Assert(rootNode, context.Graph.CreateUriNode("store:readOnly"), this.IsReadOnly.ToLiteral(context.Graph));
        }

        /// <summary>
        /// Loads the connection definition for the connection from the given graph
        /// </summary>
        /// <param name="g">Graph</param>
        public void LoadConfiguration(IGraph g)
        {
            g.NamespaceMap.AddNamespace("store", UriFactory.Create(StoreManagerNamespace));
            g.NamespaceMap.AddNamespace("dnr", UriFactory.Create(ConfigurationLoader.ConfigurationNamespace));
            INode rootNode = g.CreateUriNode(this.RootUri);

            // First off need to find the definition type (if any)
            Triple t = g.GetTriplesWithSubjectPredicate(rootNode, g.CreateUriNode("store:definitionType")).FirstOrDefault();
            if (t != null && t.Object.NodeType == NodeType.Literal)
            {
                Type defType = Type.GetType(((ILiteralNode) t.Object).Value);
                if (defType != null)
                {
                    this.Definition = (IConnectionDefinition) Activator.CreateInstance(defType);
                }
            }
            if (ReferenceEquals(this.Definition, null))
            {
                // Have to figure out the definition type another way
                t = g.GetTriplesWithSubjectPredicate(rootNode, g.CreateUriNode("dnr:type")).FirstOrDefault();
                if (t != null && t.Object.NodeType == NodeType.Literal)
                {
                    Type providerType = Type.GetType(((ILiteralNode) t.Object).Value);
                    IConnectionDefinition temp = ConnectionDefinitionManager.GetDefinitionByTargetType(providerType);
                    if (temp != null)
                    {
                        this.Definition = (IConnectionDefinition) Activator.CreateInstance(temp.GetType());
                    }
                }
            }
            if (ReferenceEquals(this.Definition, null)) throw new ArgumentException("Unable to locate the necessary configuration information to load this connection from the given Graph");

            // Populate information
            this.Definition.PopulateFrom(g, rootNode);
            this.LoadInformation(g);
        }

        /// <summary>
        /// Loads ancillary information such as name and modified dates from the given graph
        /// </summary>
        /// <param name="g">Graph</param>
        public void LoadInformation(IGraph g)
        {
            g.NamespaceMap.AddNamespace("store", UriFactory.Create(StoreManagerNamespace));
            g.NamespaceMap.AddNamespace("dnr", UriFactory.Create(ConfigurationLoader.ConfigurationNamespace));
            INode rootNode = g.CreateUriNode(this.RootUri);

            // Created, Last Modified and Last Opened
            Triple created = g.GetTriplesWithSubjectPredicate(rootNode, g.CreateUriNode("store:created")).FirstOrDefault();
            Triple lastModified = g.GetTriplesWithSubjectPredicate(rootNode, g.CreateUriNode("store:lastModified")).FirstOrDefault();
            Triple lastOpened = g.GetTriplesWithSubjectPredicate(rootNode, g.CreateUriNode("store:lastOpened")).FirstOrDefault();
// ReSharper disable PossibleInvalidOperationException
            // Resharper warnings are incorrect since logic of the GetDate() method guarantees we'll always produce a value
            this.Created = GetDate(created, DateTimeOffset.UtcNow).Value;
            this.LastModified = GetDate(lastModified, this.Created).Value;
// ReSharper restore PossibleInvalidOperationException
            this.LastOpened = GetDate(lastOpened, null);

            // Read-Only?
            this.IsReadOnly = ConfigurationLoader.GetConfigurationBoolean(g, rootNode, g.CreateUriNode("store:readOnly"), false);
        }

        /// <summary>
        /// Makes a copy of this connection which will be in the closed state
        /// </summary>
        /// <returns>Copy</returns>
        public Connection Copy()
        {
            return new Connection(this);
        }

        /// <summary>
        /// Gets the date based on the stored value using the default value if there was no stored value
        /// </summary>
        /// <param name="t">Triple whose object is the stored value</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Date</returns>
        private static DateTimeOffset? GetDate(Triple t, DateTimeOffset? defaultValue)
        {
            if (t == null) return defaultValue;
            INode n = t.Object;
            if (n.NodeType == NodeType.Literal)
            {
                IValuedNode value = n.AsValuedNode();
                try
                {
                    return value.AsDateTimeOffset();
                }
                catch (RdfQueryException)
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Determines whether the connection is equal to another object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>True if equals, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return obj is Connection && this.Equals((Connection) obj);
        }

        /// <summary>
        /// Gets the hash code for a connection
        /// </summary>
        /// <returns>Hash Code</returns>
        public override int GetHashCode()
        {
            return this.RootUri.GetEnhancedHashCode();
        }

        /// <summary>
        /// Gets the string representation of this connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Determines whether the connection is equal to another connection
        /// </summary>
        /// <param name="other">Connection</param>
        /// <returns>True if equals, false otherwise</returns>
        public bool Equals(Connection other)
        {
            return EqualityHelper.AreUrisEqual(this.RootUri, other.RootUri);
        }

        /// <summary>
        /// Event which is raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper method for raising the property changed event
        /// </summary>
        /// <param name="propertyName">Property Name</param>
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}