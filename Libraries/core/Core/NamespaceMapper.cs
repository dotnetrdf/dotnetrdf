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
using System.Threading;

namespace VDS.RDF
{
    /// <summary>
    /// Delegate Type for the Events of the Namespace Mapper
    /// </summary>
    /// <param name="prefix">Namespace Prefix</param>
    /// <param name="uri">Namespace Uri</param>
    public delegate void NamespaceChanged(String prefix, Uri uri);

    /// <summary>
    /// Class for representing Mappings between Prefixes and Namespace URIs
    /// </summary>
    public class NamespaceMapper : INamespaceMapper
    {
        /// <summary>
        /// Constant Uri for the RDF Namespace
        /// </summary>
        public const string RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        /// <summary>
        /// Constant Uri for the RDF Scheme Namespace
        /// </summary>
        public const string RDFS = "http://www.w3.org/2000/01/rdf-schema#";
        /// <summary>
        /// Constant Uri for the XML Scheme Namespace
        /// </summary>
        public const string XMLSCHEMA = "http://www.w3.org/2001/XMLSchema#";
        /// <summary>
        /// Constant Uri for the OWL Namespace
        /// </summary>
        public const string OWL = "http://www.w3.org/2002/07/owl#";

        /// <summary>
        /// Mapping of Prefixes to URIs
        /// </summary>
        protected Dictionary<String, Uri> _uris;
        /// <summary>
        /// Mapping of URIs to Prefixes
        /// </summary>
        protected Dictionary<int, String> _prefixes;

        /// <summary>
        /// Constructs a new Namespace Map
        /// </summary>
        /// <remarks>The Prefixes rdf, rdfs and xsd are automatically defined</remarks>
        public NamespaceMapper()
            : this(false) { }

        /// <summary>
        /// Constructs a new Namespace Map which is optionally empty
        /// </summary>
        /// <param name="empty">Whether the Namespace Map should be empty, if set to false the Prefixes rdf, rdfs and xsd are automatically defined</param>
        public NamespaceMapper(bool empty)
        {
            this._uris = new Dictionary<string, Uri>();
            this._prefixes = new Dictionary<int, string>();

            if (!empty)
            {
                //Add Standard Namespaces
                this.AddNamespace("rdf", new Uri(RDF));
                this.AddNamespace("rdfs", new Uri(RDFS));
                this.AddNamespace("xsd", new Uri(XMLSCHEMA));
            }
        }

        /// <summary>
        /// Constructs a new Namespace Map which is based on an existing map
        /// </summary>
        /// <param name="nsmapper"></param>
        protected internal NamespaceMapper(INamespaceMapper nsmapper)
            : this(true)
        {
            this.Import(nsmapper);
        }

        /// <summary>
        /// Returns the Prefix associated with the given Namespace URI
        /// </summary>
        /// <param name="uri">The Namespace URI to lookup the Prefix for</param>
        /// <returns>String prefix for the Namespace</returns>
        public virtual String GetPrefix(Uri uri)
        {
            int hash = uri.GetEnhancedHashCode();
            if (this._prefixes.ContainsKey(hash))
            {
                return this._prefixes[hash];
            }
            else
            {
                throw new RdfException("The Prefix for the given URI '" + uri.ToString() + "' is not known by the in-scope NamespaceMapper");
            }
        }

        /// <summary>
        /// Returns the Namespace URI associated with the given Prefix
        /// </summary>
        /// <param name="prefix">The Prefix to lookup the Namespace URI for</param>
        /// <returns>URI for the Namespace</returns>
        public virtual Uri GetNamespaceUri(String prefix) 
        {
            if (this._uris.ContainsKey(prefix))
            {
                return this._uris[prefix];
            }
            else
            {
                throw new RdfException("The Namespace URI for the given Prefix '" + prefix + "' is not known by the in-scope NamespaceMapper");
            }
        }

        /// <summary>
        /// Adds a Namespace to the Namespace Map
        /// </summary>
        /// <param name="prefix">Namespace Prefix</param>
        /// <param name="uri">Namespace Uri</param>
        public virtual void AddNamespace(String prefix, Uri uri)
        {
            int hash = uri.GetEnhancedHashCode();
            if (!this._uris.ContainsKey(prefix))
            {
                //Add a New Prefix
                this._uris.Add(prefix, uri);

                if (!this._prefixes.ContainsKey(hash))
                {
                    //Add a New Uri
                    this._prefixes.Add(hash, prefix);
                    this.OnNamespaceAdded(prefix, uri);
                }
                else
                {
                    //Check whether the Namespace Uri is actually being changed
                    //If the existing Uri is the same as the old one then we change the prefix
                    //but we don't raise the OnNamespaceModified event
                    this._prefixes[hash] = prefix;
                    if (!this._uris[prefix].ToString().Equals(uri.ToString(), StringComparison.Ordinal))
                    {
                        //Raise modified event
                        this.OnNamespaceModified(prefix, uri);
                    }
                }
            }
            else
            {
                //Check whether the Namespace is actually being changed
                //If the existing Uri is the same as the old one no change is needed
                if (!this._uris[prefix].ToString().Equals(uri.ToString(), StringComparison.Ordinal))
                {
                    //Update the existing Prefix
                    this._uris[prefix] = uri;
                    this._prefixes[hash] = prefix;
                    this.OnNamespaceModified(prefix, uri);
                }
            }
        }

        /// <summary>
        /// Removes a Namespace from the NamespaceMapper
        /// </summary>
        /// <param name="prefix">Namespace Prefix of the Namespace to remove</param>
        public virtual void RemoveNamespace(String prefix)
        {
            //Check the Namespace is defined
            if (this._uris.ContainsKey(prefix))
            {
                Uri u = this._uris[prefix];

                //Remove the Prefix to Uri Mapping
                this._uris.Remove(prefix);

                //Remove the corresponding Uri to Prefix Mapping
                int hash = u.GetEnhancedHashCode();
                if (this._prefixes.ContainsKey(hash))
                {
                    this._prefixes.Remove(hash);
                }

                //Raise the Event
                this.OnNamespaceRemoved(prefix, u);
            }
        }

        /// <summary>
        /// Method which checks whether a given Namespace Prefix is defined
        /// </summary>
        /// <param name="prefix">Prefix to test</param>
        /// <returns></returns>
        public virtual bool HasNamespace(String prefix)
        {
            return this._uris.ContainsKey(prefix);
        }

        /// <summary>
        /// Clears the Namespace Map
        /// </summary>
        public void Clear()
        {
            this._prefixes.Clear();
            this._uris.Clear();
        }

        /// <summary>
        /// Gets a Enumerator of all the Prefixes
        /// </summary>
        public IEnumerable<String> Prefixes
        {
            get
            {
                return this._uris.Keys;
            }
        }

        /// <summary>
        /// A Function which attempts to reduce a Uri to a QName
        /// </summary>
        /// <param name="uri">The Uri to attempt to reduce</param>
        /// <param name="qname">The value to output the QName to if possible</param>
        /// <returns></returns>
        /// <remarks>This function will return a Boolean indicated whether it succeeded in reducing the Uri to a QName.  If it did then the out parameter qname will contain the reduction, otherwise it will be the empty string.</remarks>
        public virtual bool ReduceToQName(String uri, out String qname)
        {
            foreach (Uri u in this._uris.Values)
            {
                String baseuri = u.ToString();

                //Does the Uri start with the Base Uri
                if (uri.StartsWith(baseuri))
                {
                    //Remove the Base Uri from the front of the Uri
                    qname = uri.Substring(baseuri.Length);
                    //Add the Prefix back onto the front plus the colon to give a QName
                    qname = this._prefixes[u.GetEnhancedHashCode()] + ":" + qname;
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
        /// Imports the contents of another Namespace Map into this Namespace Map
        /// </summary>
        /// <param name="nsmap">Namespace Map to import</param>
        /// <remarks>
        /// Prefixes in the imported Map which are already defined in this Map are ignored, this may change in future releases.
        /// </remarks>
        public virtual void Import(INamespaceMapper nsmap)
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
                    if (!this._uris[prefix].ToString().Equals(nsmap.GetNamespaceUri(prefix).ToString(), StringComparison.Ordinal))
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
        /// Event which is raised when a Namespace is Added
        /// </summary>
        public event NamespaceChanged NamespaceAdded;

        /// <summary>
        /// Event which is raised when a Namespace is Modified
        /// </summary>
        public event NamespaceChanged NamespaceModified;

        /// <summary>
        /// Event which is raised when a Namespace is Removed
        /// </summary>
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

        #region IDisposable Members

        /// <summary>
        /// Disposes of a Namespace Map
        /// </summary>
        public void Dispose()
        {
            this._prefixes.Clear();
            this._uris.Clear();
        }

        #endregion
    }

    /// <summary>
    /// Class for representing Mappings from URIs to QNames
    /// </summary>
    /// <remarks>
    /// Used primarily in outputting RDF syntax
    /// </remarks>
    public class QNameOutputMapper : NamespaceMapper
    {
        /// <summary>
        /// Mapping of URIs to QNames
        /// </summary>
        protected HashTable<int, QNameMapping> _mapping = new HashTable<int, QNameMapping>();
        /// <summary>
        /// Next available Temporary Namespace ID
        /// </summary>
        protected int _nextNamespaceID = 0;

        /// <summary>
        /// Creates a new QName Output Mapper using the given Namespace Map
        /// </summary>
        /// <param name="nsmapper">Namespace Map</param>
        public QNameOutputMapper(INamespaceMapper nsmapper)
            : base(nsmapper) { }

        /// <summary>
        /// Creates a new QName Output Mapper which has an empty Namespace Map
        /// </summary>
        public QNameOutputMapper()
            : base(true) { }

        /// <summary>
        /// A Function which attempts to reduce a Uri to a QName
        /// </summary>
        /// <param name="uri">The Uri to attempt to reduce</param>
        /// <param name="qname">The value to output the QName to if possible</param>
        /// <returns></returns>
        /// <remarks>This function will return a Boolean indicated whether it succeeded in reducing the Uri to a QName.  If it did then the out parameter qname will contain the reduction, otherwise it will be the empty string.</remarks>
        public override bool ReduceToQName(string uri, out string qname)
        {
            //See if we've cached this mapping
            QNameMapping mapping = new QNameMapping(uri);
            if (this._mapping.Contains(uri.GetHashCode(), mapping))
            {
                qname = this._mapping[uri.GetHashCode()].QName;
                return true;
            }

            foreach (Uri u in this._uris.Values)
            {
                String baseuri = u.ToString();

                //Does the Uri start with the Base Uri
                if (uri.StartsWith(baseuri))
                {
                    //Remove the Base Uri from the front of the Uri
                    qname = uri.Substring(baseuri.Length);
                    //Add the Prefix back onto the front plus the colon to give a QName
                    if (this._prefixes.ContainsKey(u.GetEnhancedHashCode()))
                    {
                        qname = this._prefixes[u.GetEnhancedHashCode()] + ":" + qname;
                        if (qname.Equals(":")) continue;
                        if (qname.Contains("/") || qname.Contains("#")) continue;
                        //Cache the Mapping
                        mapping.QName = qname;
                        this.AddToCache(uri.GetHashCode(), mapping);
                        return true;
                    }
                }
            }

            //Failed to find a Reduction
            qname = String.Empty;
            return false;
        }

        /// <summary>
        /// A Function which attempts to reduce a Uri to a QName and issues a Temporary Namespace if required
        /// </summary>
        /// <param name="uri">The Uri to attempt to reduce</param>
        /// <param name="qname">The value to output the QName to if possible</param>
        /// <param name="tempNamespace">The Temporary Namespace issued (if any)</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This function will always returns a possible QName for the URI if the format of the URI permits it.  It doesn't guarentee that the QName will be valid for the syntax it is being written to - it is up to implementers of writers to validate the QNames returned.
        /// </para>
        /// <para>
        /// Where necessary a Temporary Namespace will be issued and the <paramref name="tempNamespace">tempNamespace</paramref> parameter will be set to the prefix of the new temporary namespace
        /// </para>
        /// </remarks>
        public bool ReduceToQName(String uri, out String qname, out String tempNamespace)
        {
            tempNamespace = String.Empty;

            //See if we've cached this mapping
            QNameMapping mapping = new QNameMapping(uri);
            if (this._mapping.Contains(uri.GetHashCode(), mapping))
            {
                qname = this._mapping[uri.GetHashCode()].QName;
                return true;
            }

            //Try and find a Namespace URI that is the prefix of the URI
            foreach (Uri u in this._uris.Values)
            {
                String baseuri = u.ToString();

                //Does the Uri start with the Base Uri
                if (uri.StartsWith(baseuri))
                {
                    //Remove the Base Uri from the front of the Uri
                    qname = uri.Substring(baseuri.Length);
                    //Add the Prefix back onto the front plus the colon to give a QName
                    if (this._prefixes.ContainsKey(u.GetEnhancedHashCode()))
                    {
                        qname = this._prefixes[u.GetEnhancedHashCode()] + ":" + qname;
                        if (qname.Equals(":")) continue;
                        if (qname.Contains("/") || qname.Contains("#")) continue;
                        //Cache the Mapping
                        mapping.QName = qname;
                        this.AddToCache(uri.GetHashCode(), mapping);
                        return true;
                    }
                }
            }

            //Try and issue a Temporary Namespace
            String nsUri, nsPrefix;
            if (uri.Contains('#'))
            {
                nsUri = uri.Substring(0, uri.LastIndexOf('#') + 1);
                nsPrefix = this.GetNextTemporaryNamespacePrefix();
            }
            else if (uri.LastIndexOf('/') > 8)
            {
                nsUri = uri.Substring(0, uri.LastIndexOf('/') + 1);
                nsPrefix = this.GetNextTemporaryNamespacePrefix();
            }
            else
            {

                //Failed to find a Reduction and unable to issue a Temporary Namespace
                qname = String.Empty;
                return false;
            }

            //Add to Namespace Map
            this.AddNamespace(nsPrefix, new Uri(nsUri));

            //Cache mapping and return
            mapping.QName = nsPrefix + ":" + uri.Replace(nsUri, String.Empty);
            this.AddToCache(uri.GetHashCode(), mapping);
            qname = mapping.QName;
            tempNamespace = nsPrefix;
            return true;
        }

        /// <summary>
        /// Adds a URI to QName Mapping to the Cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected virtual void AddToCache(int key, QNameMapping value)
        {
            this._mapping.Add(key, value);
        }

        /// <summary>
        /// Gets the next available Temporary Namespace ID
        /// </summary>
        /// <returns></returns>
        private String GetNextTemporaryNamespacePrefix()
        {
            String nextPrefixBase = "ns";
            while (this._uris.ContainsKey(nextPrefixBase + this._nextNamespaceID))
            {
                this._nextNamespaceID++;
            }
            return nextPrefixBase + this._nextNamespaceID;
        }
    }

    /// <summary>
    /// Thread Safe version of the <see cref="QNameOutputMapper">QNameOutputMapper</see>
    /// </summary>
    public class ThreadSafeQNameOutputMapper : QNameOutputMapper
    {
        /// <summary>
        /// Creates a new Thread Safe QName Output Mapper
        /// </summary>
        /// <param name="nsmapper">Namespace Mapper</param>
        public ThreadSafeQNameOutputMapper(INamespaceMapper nsmapper)
            : base(nsmapper) { }

        /// <summary>
        /// Adds a QName Mapping to the Cache in a Thread Safe way
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        protected override void AddToCache(int key, QNameMapping value)
        {
            try
            {
                Monitor.Enter(this._mapping);
                base.AddToCache(key, value);
            }
            finally
            {
                Monitor.Exit(this._mapping);
            }
        }

        /// <summary>
        /// Adds a Namespace to the QName Output Mapper
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="uri">Namespace URI</param>
        public override void AddNamespace(string prefix, Uri uri)
        {
            try
            {
                Monitor.Enter(this._prefixes);
                Monitor.Enter(this._uris);
                base.AddNamespace(prefix, uri);
            }
            finally
            {
                Monitor.Exit(this._prefixes);
                Monitor.Exit(this._uris);
            }
        }
    }

    /// <summary>
    /// Represents a mapping from a URI to a QName
    /// </summary>
    public class QNameMapping 
    {
        private String _u;
        private String _qname;

        /// <summary>
        /// Creates a new QName Mapping
        /// </summary>
        /// <param name="u">URI</param>
        public QNameMapping(String u)
        {
            this._u = u;
        }

        /// <summary>
        /// URI this is a mapping for
        /// </summary>
        public String Uri
        {
            get
            {
                return this._u;
            }
        }

        /// <summary>
        /// QName this URI maps to
        /// </summary>
        public String QName
        {
            get
            {
                return this._qname;
            }
            set
            {
                this._qname = value;
            }
        }

        /// <summary>
        /// Gets the String representation of the URI
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._u.ToString();
        }

        /// <summary>
        /// Checks whether this is equal to another Object
        /// </summary>
        /// <param name="obj">Object to test against</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is QNameMapping)
            {
                return this.ToString().Equals(obj.ToString(), StringComparison.Ordinal);
            }
            else
            {
                return false;
            }
        }
    }
}
