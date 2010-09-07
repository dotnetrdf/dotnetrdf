using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace rdfEditor
{
    public class OffsetScopedNamespaceMapper : INamespaceMapper
    {
        private Dictionary<String, List<OffsetMapping>> _uris = new Dictionary<string, List<OffsetMapping>>();
        private Dictionary<int, List<OffsetMapping>> _prefixes = new Dictionary<int, List<OffsetMapping>>();
        private int _offset = 0;

        /// <summary>
        /// Constructs a new Namespace Map
        /// </summary>
        /// <remarks>The Prefixes rdf, rdfs and xsd are automatically defined</remarks>
        public OffsetScopedNamespaceMapper()
            : this(false) { }

        /// <summary>
        /// Constructs a new Namespace Map which is optionally empty
        /// </summary>
        /// <param name="empty">Whether the Namespace Map should be empty, if set to false the Prefixes rdf, rdfs and xsd are automatically defined</param>
        protected internal OffsetScopedNamespaceMapper(bool empty)
        {
            if (!empty)
            {
                //Add Standard Namespaces
                this.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
                this.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
                this.AddNamespace("xsd", new Uri(NamespaceMapper.XMLSCHEMA));
            }
        }

        public void AddNamespace(string prefix, Uri uri)
        {
            OffsetMapping mapping = new OffsetMapping(prefix, uri, this._offset);
            if (!this._prefixes.ContainsKey(uri.GetEnhancedHashCode())) this._prefixes.Add(uri.GetEnhancedHashCode(), new List<OffsetMapping>());

            if (this._uris.ContainsKey(prefix))
            {
                //Is it defined at the current offset level?
                if (this._uris[prefix].Any(m => m.Offset == this._offset))
                {
                    //If it is then we override it
                    this._uris[prefix].RemoveAll(m => m.Offset == this._offset);
                    this._prefixes[uri.GetEnhancedHashCode()].RemoveAll(m => m.Offset == this._offset);

                    this._uris[prefix].Add(mapping);
                    this._prefixes[uri.GetEnhancedHashCode()].Add(mapping);
                    this.OnNamespaceModified(prefix, uri);
                }
                else
                {
                    //If not we simply add it
                    this._uris[prefix].Add(mapping);
                    this._prefixes[uri.GetEnhancedHashCode()].Add(mapping);
                    this.OnNamespaceAdded(prefix, uri);
                }
            }
            else
            {
                //Not yet defined so add it
                this._uris.Add(prefix, new List<OffsetMapping>());
                this._uris[prefix].Add(mapping);
                this._prefixes[uri.GetEnhancedHashCode()].Add(mapping);
                this.OnNamespaceAdded(prefix, uri);
            }
        }

        public void Clear()
        {
            this._uris.Clear();
            this._prefixes.Clear();
        }

        public Uri GetNamespaceUri(string prefix)
        {
            if (this._uris.ContainsKey(prefix))
            {
                if (this._uris[prefix].Any(m => m.Offset < this._offset))
                {
                    return this._uris[prefix].Last(m => m.Offset < this._offset).Uri;
                }
                else
                {
                    throw new RdfException("The Namespace URI for the given Prefix '" + prefix + "' is not in-scope at the current Offset");
                }
            }
            else
            {
                throw new RdfException("The Namespace URI for the given Prefix '" + prefix + "' is not known by the in-scope NamespaceMapper");
            }
        }

        public string GetPrefix(Uri uri)
        {
            int hash = uri.GetEnhancedHashCode();
            if (this._prefixes.ContainsKey(hash))
            {
                if (this._prefixes[hash].Any(m => m.Offset < this._offset))
                {
                    return this._prefixes[hash].Last(m => m.Offset < this._offset).Prefix;
                }
                else
                {
                    throw new RdfException("The Prefix for the given URI '" + uri.ToString() + "' is not in-scope at the current Offset");
                }
            }
            else
            {
                throw new RdfException("The Prefix for the given URI '" + uri.ToString() + "' is not known by the in-scope NamespaceMapper");
            }
        }

        public bool HasNamespace(string prefix)
        {
            if (this._uris.ContainsKey(prefix))
            {
                return this._uris[prefix].Any(m => m.Offset < this._offset);
            }
            else
            {
                return false;
            }
        }

        public void Import(INamespaceMapper nsmap)
        {
            String tempPrefix = "ns0";
            int tempPrefixID = 0;
            foreach (String prefix in nsmap.Prefixes)
            {
                if (!this._uris.ContainsKey(prefix))
                {
                    //Non-colliding Namespaces get copied across
                    this.AddNamespace(prefix, nsmap.GetNamespaceUri(prefix));
                }
                else
                {
                    //Colliding Namespaces get remapped to new prefixes
                    //Assuming the prefixes aren't already used for the same Uri
                    if (!this.GetNamespaceUri(prefix).ToString().Equals(nsmap.GetNamespaceUri(prefix).ToString(), StringComparison.Ordinal))
                    {
                        while (this._uris.ContainsKey(tempPrefix))
                        {
                            tempPrefixID++;
                            tempPrefix = "ns" + tempPrefixID;
                        }
                        this.AddNamespace(tempPrefix, nsmap.GetNamespaceUri(prefix));
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Current Offset
        /// </summary>
        public int CurrentOffset
        {
            get
            {
                return this._offset;
            }
            set
            {
                this._offset = value;
            }
        }

        public event NamespaceChanged NamespaceAdded;

        public event NamespaceChanged NamespaceModified;

        public event NamespaceChanged NamespaceRemoved;

        /// <summary>
        /// Internal Helper for the NamespaceAdded Event which raises it only when a Handler is registered
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        protected virtual void OnNamespaceAdded(String prefix, Uri uri)
        {
            NamespaceChanged handler = this.NamespaceAdded;
            if (handler != null)
            {
                handler(prefix, uri);
            }
        }

        /// <summary>
        /// Internal Helper for the NamespaceModified Event which raises it only when a Handler is registered
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        protected virtual void OnNamespaceModified(String prefix, Uri uri)
        {
            NamespaceChanged handler = this.NamespaceModified;
            if (handler != null)
            {
                handler(prefix, uri);
            }
        }

        /// <summary>
        /// Internal Helper for the NamespaceRemoved Event which raises it only when a Handler is registered
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        protected virtual void OnNamespaceRemoved(String prefix, Uri uri)
        {
            NamespaceChanged handler = this.NamespaceRemoved;
            if (handler != null)
            {
                handler(prefix, uri);
            }
        }

        public IEnumerable<string> Prefixes
        {
            get 
            {
                return (from prefix in this._uris.Keys
                        where this.HasNamespace(prefix)
                        select prefix);
            }
        }

        public bool ReduceToQName(string uri, out string qname)
        {
            foreach (Uri u in this._uris.Values.Select(l => l.Last(m => m.Offset < this._offset).Uri))
            {
                String baseuri = u.ToString();

                //Does the Uri start with the Base Uri
                if (uri.StartsWith(baseuri))
                {
                    //Remove the Base Uri from the front of the Uri
                    qname = uri.Substring(baseuri.Length);
                    //Add the Prefix back onto the front plus the colon to give a QName
                    qname = this.GetPrefix(u) + ":" + qname;
                    if (qname.Equals(":")) continue;
                    if (qname.Contains("/") || qname.Contains("#")) continue;
                    return true;
                }
            }

            //Failed to find a Reduction
            qname = String.Empty;
            return false;
        }

        public void RemoveNamespace(string prefix)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this._prefixes.Clear();
            this._uris.Clear();
        }
    }

    public class OffsetMapping
    {
        private int _offset;
        private String _prefix;
        private Uri _uri;

        public OffsetMapping(String prefix, Uri uri, int offset)
        {
            this._prefix = prefix;
            this._uri = uri;
            this._offset = offset;
        }

        public OffsetMapping(String prefix, Uri uri)
            : this(prefix, uri, 0) { }

        public int Offset
        {
            get 
            {
                return this._offset;
            }
        }

        public String Prefix
        {
            get 
            {
                return this._prefix;
            }
        }

        public Uri Uri
        {
            get
            {
                return this._uri;
            }
        }
    }
}
