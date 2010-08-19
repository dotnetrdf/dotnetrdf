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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    public class NestedNamespaceMapper : INamespaceMapper
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
                this.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
                this.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
                this.AddNamespace("xsd", new Uri(NamespaceMapper.XMLSCHEMA));
            }
        }

        public void AddNamespace(string prefix, Uri uri)
        {
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
                this._uris.Add(prefix, new List<NestedMapping>());
                this._uris[prefix].Add(mapping);
                this._prefixes[uri.GetEnhancedHashCode()].Add(mapping);
                this.OnNamespaceAdded(prefix, uri);
            }
        }

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

        public string GetPrefix(Uri uri)
        {
            int hash = uri.GetEnhancedHashCode();
            if (this._prefixes.ContainsKey(hash))
            {
                return this._prefixes[hash].Last().Prefix;
            }
            else
            {
                throw new RdfException("The Prefix for the given URI '" + uri.ToString() + "' is not known by the in-scope NamespaceMapper");
            }
        }

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

        public bool HasNamespace(string prefix)
        {
            return this._uris.ContainsKey(prefix);
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

        public void IncrementNesting()
        {
            this._level++;
        }

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

        public int NestingLevel
        {
            get
            {
                return this._level;
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
                return this._uris.Keys;
            }
        }

        public bool ReduceToQName(string uri, out string qname)
        {
            foreach (Uri u in this._uris.Values.Select(l => l.Last().Uri))
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
            this._level = 0;
        }
    }

    public class NestedMapping
    {
        private int _level;
        private String _prefix;
        private Uri _uri;

        public NestedMapping(String prefix, Uri uri, int level)
        {
            this._prefix = prefix;
            this._uri = uri;
            this._level = level;
        }

        public NestedMapping(String prefix, Uri uri)
            : this(prefix, uri, 0) { }

        public int Level
        {
            get 
            {
                return this._level;
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
