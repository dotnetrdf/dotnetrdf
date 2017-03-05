/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF
{
    /// <summary>
    /// A Namespace Mapper which has an explicit notion of Nesting
    /// </summary>
    public class NestedNamespaceMapper : INestedNamespaceMapper
    {
        private Dictionary<String, List<NestedMapping>> _uris = new Dictionary<string, List<NestedMapping>>();
        private Dictionary<int, List<NestedMapping>> _prefixes = new Dictionary<int, List<NestedMapping>>();
        private int _level = 0;

        /// <summary>
        /// Constructs a new Namespace Map
        /// </summary>
        /// <remarks>The Prefixes rdf, rdfs and xsd are automatically defined</remarks>
        public NestedNamespaceMapper()
            : this(false) { }

        /// <summary>
        /// Constructs a new Namespace Map which is optionally empty
        /// </summary>
        /// <param name="empty">Whether the Namespace Map should be empty, if set to false the Prefixes rdf, rdfs and xsd are automatically defined</param>
        protected internal NestedNamespaceMapper(bool empty)
        {
            if (!empty)
            {
                //Add Standard Namespaces
                this.AddNamespace("rdf", UriFactory.Create(NamespaceMapper.RDF));
                this.AddNamespace("rdfs", UriFactory.Create(NamespaceMapper.RDFS));
                this.AddNamespace("xsd", UriFactory.Create(NamespaceMapper.XMLSCHEMA));
            }
        }

        /// <summary>
        /// Adds a Namespace at the Current Nesting Level
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="uri">Namespace URI</param>
        public void AddNamespace(string prefix, Uri uri)
        {
            if (uri == null) throw new ArgumentNullException("Cannot set a prefix to the null URI");
            NestedMapping mapping = new NestedMapping(prefix, uri, this._level);
            if (!this._prefixes.ContainsKey(uri.GetEnhancedHashCode())) this._prefixes.Add(uri.GetEnhancedHashCode(), new List<NestedMapping>());

            if (this._uris.ContainsKey(prefix))
            {
                //Is it defined on the current nesting level?
                if (this._uris[prefix].Any(m => m.Level == this._level))
                {
                    //If it is then we override it
                    this._uris[prefix].RemoveAll(m => m.Level == this._level);
                    this._prefixes[uri.GetEnhancedHashCode()].RemoveAll(m => m.Level == this._level);

                    this._uris[prefix].Add(mapping);
                    this._prefixes[uri.GetEnhancedHashCode()].Add(mapping);
                    this.RaiseNamespaceModified(prefix, uri);
                }
                else
                {
                    //If not we simply add it
                    this._uris[prefix].Add(mapping);
                    this._prefixes[uri.GetEnhancedHashCode()].Add(mapping);
                    this.RaiseNamespaceAdded(prefix, uri);
                }
            }
            else
            {
                //Not yet defined so add it
                this._uris.Add(prefix, new List<NestedMapping>());
                this._uris[prefix].Add(mapping);
                this._prefixes[uri.GetEnhancedHashCode()].Add(mapping);
                this.RaiseNamespaceAdded(prefix, uri);
            }
        }

        /// <summary>
        /// Clears the Namespace Map
        /// </summary>
        public void Clear()
        {
            this._uris.Clear();
            this._prefixes.Clear();
        }

        /// <summary>
        /// Gets the Namespace URI for the given Prefix at the current Nesting Level
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        public Uri GetNamespaceUri(string prefix)
        {
            if (this._uris.ContainsKey(prefix))
            {
                return this._uris[prefix].Last().Uri;
            }
            else
            {
                throw new RdfException("The Namespace URI for the given Prefix '" + prefix + "' is not known by the in-scope NamespaceMapper");
            }
        }

        /// <summary>
        /// Gets the Namespace Prefix for the given URI at the current Nesting Level
        /// </summary>
        /// <param name="uri">Namespace URI</param>
        /// <returns></returns>
        public string GetPrefix(Uri uri)
        {
            int hash = uri.GetEnhancedHashCode();
            if (this._prefixes.ContainsKey(hash))
            {
                return this._prefixes[hash].Last().Prefix;
            }
            else
            {
                throw new RdfException("The Prefix for the given URI '" + uri.AbsoluteUri + "' is not known by the in-scope NamespaceMapper");
            }
        }

        /// <summary>
        /// Gets the Nesting Level at which the given Namespace is definition is defined
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        public int GetNestingLevel(String prefix)
        {
            if (this._uris.ContainsKey(prefix))
            {
                return this._uris[prefix].Last().Level;
            }
            else
            {
                throw new RdfException("The Nesting Level for the given Prefix '" + prefix + "' is not known by the in-scope NamespaceMapper");
            }
        }

        /// <summary>
        /// Gets whether the given Namespace exists
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <returns></returns>
        public bool HasNamespace(string prefix)
        {
            return this._uris.ContainsKey(prefix);
        }

        /// <summary>
        /// Imports another Namespace Map into this one
        /// </summary>
        /// <param name="nsmap">Namespace Map</param>
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
                    if (!this.GetNamespaceUri(prefix).AbsoluteUri.Equals(nsmap.GetNamespaceUri(prefix).AbsoluteUri, StringComparison.Ordinal))
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
        /// Increments the Nesting Level
        /// </summary>
        public void IncrementNesting()
        {
            this._level++;
        }

        /// <summary>
        /// Decrements the Nesting Level
        /// </summary>
        /// <remarks>
        /// When the Nesting Level is decremented any Namespaces defined at a greater Nesting Level are now out of scope and so are removed from the Mapper
        /// </remarks>
        public void DecrementNesting()
        {
            this._level--;
            if (this._level > 0)
            {
                foreach (String prefix in this._uris.Keys)
                {
                    this._uris[prefix].RemoveAll(m => m.Level > this._level);
                }
                foreach (int u in this._prefixes.Keys)
                {
                    this._prefixes[u].RemoveAll(m => m.Level > this._level);
                }
                foreach (KeyValuePair<String, List<NestedMapping>> mapping in this._uris.ToList())
                {
                    if (mapping.Value.Count == 0) this._uris.Remove(mapping.Key);
                }
                foreach (KeyValuePair<int, List<NestedMapping>> mapping in this._prefixes.ToList())
                {
                    if (mapping.Value.Count == 0) this._prefixes.Remove(mapping.Key);
                }
            }
        }

        /// <summary>
        /// Gets the current Nesting Level
        /// </summary>
        public int NestingLevel
        {
            get
            {
                return this._level;
            }
        }

        /// <summary>
        /// Event which occurs when a Namespace is added
        /// </summary>
        public event NamespaceChanged NamespaceAdded;

        /// <summary>
        /// Event which occurs when a Namespace is modified
        /// </summary>
        public event NamespaceChanged NamespaceModified;

        /// <summary>
        /// Event which occurs when a Namespace is removed
        /// </summary>
        public event NamespaceChanged NamespaceRemoved;

        /// <summary>
        /// Internal Helper for the NamespaceAdded Event which raises it only when a Handler is registered
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        protected virtual void RaiseNamespaceAdded(String prefix, Uri uri)
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
        protected virtual void RaiseNamespaceModified(String prefix, Uri uri)
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

        /// <summary>
        /// Gets the Namespace Prefixes
        /// </summary>
        public IEnumerable<string> Prefixes
        {
            get 
            {
                return this._uris.Keys;
            }
        }

        /// <summary>
        /// Tries to reduce a URI to a QName using this Namespace Map
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="qname">Resulting QName</param>
        /// <returns></returns>
        public bool ReduceToQName(string uri, out string qname)
        {
            foreach (Uri u in this._uris.Values.Select(l => l.Last().Uri))
            {
                String baseuri = u.AbsoluteUri;

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

        /// <summary>
        /// Removes a Namespace provided that Namespace is defined on the current Nesting Level
        /// </summary>
        /// <param name="prefix">Prefix</param>
        public void RemoveNamespace(string prefix)
        {
            if (this.HasNamespace(prefix))
            {
                if (this.GetNestingLevel(prefix) == this._level)
                {
                    //If it's registered on this nesting level will be the last thing registered
                    this._uris[prefix].RemoveAt(this._uris[prefix].Count - 1);
                    if (this._uris[prefix].Count == 0) this._uris.Remove(prefix);
                    Uri nsUri = this.GetNamespaceUri(prefix);
                    int hash = nsUri.GetEnhancedHashCode();
                    this._prefixes[hash].RemoveAt(this._prefixes[hash].Count - 1);
                    if (this._prefixes[hash].Count == 0) this._prefixes.Remove(hash);
                    this.OnNamespaceRemoved(prefix, nsUri);
                }
            }
        }

        /// <summary>
        /// Disposes of the Namespace Map
        /// </summary>
        public void Dispose()
        {
            this._prefixes.Clear();
            this._uris.Clear();
            this._level = 0;
        }
    }

    /// <summary>
    /// Class used to hold Nested Namespace definition information
    /// </summary>
    class NestedMapping
    {
        private int _level;
        private String _prefix;
        private Uri _uri;

        /// <summary>
        /// Creates a new Nested Mapping
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="uri">Namespace URI</param>
        /// <param name="level">Nesting Level</param>
        public NestedMapping(String prefix, Uri uri, int level)
        {
            this._prefix = prefix;
            this._uri = uri;
            this._level = level;
        }

        /// <summary>
        /// Creates a new Nested Mapping
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="uri">Namespace URI</param>
        public NestedMapping(String prefix, Uri uri)
            : this(prefix, uri, 0) { }

        /// <summary>
        /// Gets the Nesting Level
        /// </summary>
        public int Level
        {
            get 
            {
                return this._level;
            }
        }

        /// <summary>
        /// Gets the Namespace Prefix
        /// </summary>
        public String Prefix
        {
            get 
            {
                return this._prefix;
            }
        }

        /// <summary>
        /// Gets the Namespace URI
        /// </summary>
        public Uri Uri
        {
            get
            {
                return this._uri;
            }
        }
    }
}
