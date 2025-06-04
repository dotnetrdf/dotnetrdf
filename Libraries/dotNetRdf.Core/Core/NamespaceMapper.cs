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
using System.Linq;
using System.Threading;
using VDS.Common.Collections;

namespace VDS.RDF;

/// <summary>
/// Delegate Type for the Events of the Namespace Mapper.
/// </summary>
/// <param name="prefix">Namespace Prefix.</param>
/// <param name="uri">Namespace Uri.</param>
public delegate void NamespaceChanged(string prefix, Uri uri);

/// <summary>
/// Class for representing Mappings between Prefixes and Namespace URIs.
/// </summary>
public class NamespaceMapper : INamespaceMapper
{
    /// <summary>
    /// Constant Uri for the RDF Namespace.
    /// </summary>
    public const string RDF = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    /// <summary>
    /// Constant Uri for the RDF Scheme Namespace.
    /// </summary>
    public const string RDFS = "http://www.w3.org/2000/01/rdf-schema#";
    /// <summary>
    /// Constant Uri for the XML Scheme Namespace.
    /// </summary>
    public const string XMLSCHEMA = "http://www.w3.org/2001/XMLSchema#";
    /// <summary>
    /// Constant Uri for the OWL Namespace.
    /// </summary>
    public const string OWL = "http://www.w3.org/2002/07/owl#";

    /// <summary>
    /// Mapping of Prefixes to URIs.
    /// </summary>
    protected Dictionary<string, Uri> _uris;

    /// <summary>
    /// Mapping of URIs to Prefixes.
    /// </summary>
    protected Dictionary<int, List<string>> _prefixes;

    /// <summary>
    /// URI factory to use.
    /// </summary>
    protected readonly IUriFactory _uriFactory;

    /// <summary>
    /// Constructs a new Namespace Map.
    /// </summary>
    /// <remarks>The Prefixes rdf, rdfs and xsd are automatically defined.</remarks>
    public NamespaceMapper()
        : this(UriFactory.Root, false) { }

    /// <summary>
    /// Constructs a new Namespace Map which is optionally empty.
    /// </summary>
    /// <param name="empty">Whether the Namespace Map should be empty, if set to false the Prefixes rdf, rdfs and xsd are automatically defined.</param>
    public NamespaceMapper(bool empty) 
        : this(UriFactory.Root, empty) { }

    /// <summary>
    /// Constructs a new namespace map with the specified URI factory.
    /// </summary>
    /// <param name="uriFactory">The URI factory for the namespace map to use.</param>
    /// <param name="empty">Whether the namespace map should be empty. If set to false, the prefixes rdf, rdfs and xsd are automatically defined.</param>
    public NamespaceMapper(IUriFactory uriFactory, bool empty = false)
    {
        _uriFactory = uriFactory ?? UriFactory.Root;
        _uris = new Dictionary<string, Uri>();
        _prefixes = new Dictionary<int, List<string>>();

        if (!empty)
        {
            // Add Standard Namespaces
            AddNamespace("rdf", _uriFactory.Create(RDF));
            AddNamespace("rdfs", _uriFactory.Create(RDFS));
            AddNamespace("xsd", _uriFactory.Create(XMLSCHEMA));
        }
    }
    /// <summary>
    /// Constructs a new Namespace Map which is based on an existing map.
    /// </summary>
    /// <param name="nsmapper"></param>
    /// <param name="uriFactory">The factory to use internally when creating new Uri intances. If not specified, defaults to <see cref="UriFactory.Root"/>.</param>
    protected internal NamespaceMapper(INamespaceMapper nsmapper, IUriFactory uriFactory = null)
        : this(uriFactory, true)
    {
        Import(nsmapper);
    }

    /// <summary>
    /// Returns the Prefix associated with the given Namespace URI.
    /// </summary>
    /// <param name="uri">The Namespace URI to lookup the Prefix for.</param>
    /// <returns>String prefix for the Namespace.</returns>
    public virtual string GetPrefix(Uri uri)
    {
        var hash = uri.GetEnhancedHashCode();
        if (_prefixes.ContainsKey(hash))
        {
            return _prefixes[hash][0];
        }
        else
        {
            throw new RdfException("The Prefix for the given URI '" + uri.AbsoluteUri + "' is not known by the in-scope NamespaceMapper");
        }
    }

    /// <summary>
    /// Returns the Namespace URI associated with the given Prefix.
    /// </summary>
    /// <param name="prefix">The Prefix to lookup the Namespace URI for.</param>
    /// <returns>URI for the Namespace.</returns>
    public virtual Uri GetNamespaceUri(string prefix) 
    {
        if (_uris.ContainsKey(prefix))
        {
            return _uris[prefix];
        }
        else
        {
            throw new RdfException("The Namespace URI for the given Prefix '" + prefix + "' is not known by the in-scope NamespaceMapper.  Did you forget to define a namespace for this prefix?");
        }
    }

    /// <summary>
    /// Adds a Namespace to the Namespace Map.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="uri">Namespace Uri.</param>
    public virtual void AddNamespace(string prefix, Uri uri)
    {
        if (uri == null) throw new ArgumentNullException("Cannot set a prefix to the null URI");
        var hash = uri.GetEnhancedHashCode();
        if (!_uris.ContainsKey(prefix))
        {
            // Add a New Prefix
            _uris.Add(prefix, uri);
            if (!_prefixes.ContainsKey(hash))
            {
                _prefixes[hash] = new List<string> {prefix};
            }
            else
            {
                _prefixes[hash].Add(prefix);
            }
            OnNamespaceAdded(prefix, uri);
            /*
            if (!_prefixes.ContainsKey(hash))
            {
                // Add a New Uri
                _prefixes.Add(hash, prefix);
                OnNamespaceAdded(prefix, uri);
            }
            else
            {
                // Check whether the Namespace Uri is actually being changed
                // If the existing Uri is the same as the old one then we change the prefix
                // but we don't raise the OnNamespaceModified event
                _prefixes[hash] = prefix;
                if (!_uris[prefix].AbsoluteUri.Equals(uri.AbsoluteUri, StringComparison.Ordinal))
                {
                    // Raise modified event
                    OnNamespaceModified(prefix, uri);
                }
            }*/
        }
        else
        {
            // Check whether the Namespace is actually being changed
            // If the existing Uri is the same as the old one no change is needed
            if (!_uris[prefix].AbsoluteUri.Equals(uri.AbsoluteUri, StringComparison.Ordinal))
            {
                Uri oldUri = _uris[prefix];

                // Overwrite the prefix-to-uri mapping
                _uris[prefix] = uri;

                // Remove the old uri-to-prefix mapping
                var oldUriHash = oldUri.GetEnhancedHashCode();
                _prefixes[oldUriHash].Remove(prefix);
                if (_prefixes[oldUriHash].Count == 0)
                {
                    _prefixes.Remove(oldUriHash);
                }

                // Insert the new uri-to-prefix mapping
                if (_prefixes.ContainsKey(hash))
                {
                    _prefixes[hash].Add(prefix);
                }
                else
                {
                    _prefixes[hash] = new List<string> {prefix};
                }

                // Raise the modified event
                OnNamespaceModified(prefix, uri);
            }
        }
    }

    /// <summary>
    /// Removes a Namespace from the NamespaceMapper.
    /// </summary>
    /// <param name="prefix">Namespace Prefix of the Namespace to remove.</param>
    public virtual void RemoveNamespace(string prefix)
    {
        // Check the Namespace is defined
        if (_uris.ContainsKey(prefix))
        {
            Uri u = _uris[prefix];

            // Remove the Prefix to Uri Mapping
            _uris.Remove(prefix);

            // Remove the corresponding Uri to Prefix Mapping
            var hash = u.GetEnhancedHashCode();
            if (_prefixes.ContainsKey(hash))
            {
                _prefixes[hash].Remove(prefix);
                if (_prefixes[hash].Count == 0) _prefixes.Remove(hash);
            }

            // Raise the Event
            OnNamespaceRemoved(prefix, u);
        }
    }

    /// <summary>
    /// Method which checks whether a given Namespace Prefix is defined.
    /// </summary>
    /// <param name="prefix">Prefix to test.</param>
    /// <returns></returns>
    public virtual bool HasNamespace(string prefix)
    {
        return _uris.ContainsKey(prefix);
    }

    /// <summary>
    /// Method which checks whether a given Namespace is defined.
    /// </summary>
    /// <param name="ns">Namespace to test.</param>
    public virtual bool HasNamespace(Uri ns)
    {
        var hash = ns.GetEnhancedHashCode();
        return _prefixes.ContainsKey(hash);
    }

    /// <summary>
    /// Clears the Namespace Map.
    /// </summary>
    public void Clear()
    {
        _prefixes.Clear();
        _uris.Clear();
    }

    /// <summary>
    /// Gets a Enumerator of all the Prefixes.
    /// </summary>
    public IEnumerable<string> Prefixes
    {
        get
        {
            return _uris.Keys;
        }
    }

    /// <summary>
    /// A Function which attempts to reduce a Uri to a QName.
    /// </summary>
    /// <param name="uri">The Uri to attempt to reduce.</param>
    /// <param name="qname">The value to output the QName to if possible.</param>
    /// <param name="validationFunction">A validation function to use to validate the QName. Only a QName value for which the validation function return true will be returned.</param>
    /// <returns></returns>
    /// <remarks>This function will return a Boolean indicated whether it succeeded in reducing the Uri to a QName.  If it did then the out parameter qname will contain the reduction, otherwise it will be the empty string.</remarks>
    public virtual bool ReduceToQName(string uri, out string qname, Func<string, bool> validationFunction = null)
    {
        validationFunction ??= DefaultQNameValidationFunction;
        foreach (Uri u in _uris.Values)
        {
            var baseuri = u.AbsoluteUri;

            // Does the Uri start with the Base Uri
            if (uri.StartsWith(baseuri))
            {
                // Remove the Base Uri from the front of the Uri
                qname = uri.Substring(baseuri.Length);
                // Add the Prefix back onto the front plus the colon to give a QName
                qname = _prefixes[u.GetEnhancedHashCode()][0] + ":" + qname;
                if (qname.Equals(":")) continue;
                if (!validationFunction(qname)) continue;
                return true;
            }
        }

        // Failed to find a Reduction
        qname = string.Empty;
        return false;
    }

    /// <summary>
    /// Provides a default validation function for QNames generated by the <see cref="ReduceToQName"/> method.
    /// </summary>
    /// <param name="qname">The QName to be validated.</param>
    /// <returns>False if the QName contains a '#' or '/' character, true otherwise.</returns>
    public static bool DefaultQNameValidationFunction(string qname)
    {
        return !(qname.Contains("/") || qname.Contains("#"));
    }

    /// <summary>
    /// Imports the contents of another Namespace Map into this Namespace Map.
    /// </summary>
    /// <param name="nsmap">Namespace Map to import.</param>
    /// <remarks>
    /// Prefixes in the imported Map which are already defined in this Map are ignored, this may change in future releases.
    /// </remarks>
    public virtual void Import(INamespaceMapper nsmap)
    {
        string tempPrefix = "ns0";
        int tempPrefixID = 0;
        foreach (string prefix in nsmap.Prefixes)
        {
            if (!_uris.ContainsKey(prefix))
            {
                // Non-colliding Namespaces get copied across
                AddNamespace(prefix, nsmap.GetNamespaceUri(prefix));
            }
            else
            {
                // Colliding Namespaces get remapped to new prefixes
                // Assuming the prefixes aren't already used for the same Uri
                if (!_uris[prefix].AbsoluteUri.Equals(nsmap.GetNamespaceUri(prefix).AbsoluteUri, StringComparison.Ordinal))
                {
                    while (_uris.ContainsKey(tempPrefix))
                    {
                        tempPrefixID++;
                        tempPrefix = "ns" + tempPrefixID;
                    }
                    AddNamespace(tempPrefix, nsmap.GetNamespaceUri(prefix));
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
    /// Internal Helper for the NamespaceAdded Event which raises it only when a Handler is registered.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="uri">Namespace Uri.</param>
    protected virtual void OnNamespaceAdded(string prefix, Uri uri)
    {
        NamespaceChanged handler = NamespaceAdded;
        if (handler != null)
        {
            handler(prefix, uri);
        }
    }

    /// <summary>
    /// Internal Helper for the NamespaceModified Event which raises it only when a Handler is registered.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="uri">Namespace Uri.</param>
    protected virtual void OnNamespaceModified(string prefix, Uri uri)
    {
        NamespaceChanged handler = NamespaceModified;
        if (handler != null)
        {
            handler(prefix, uri);
        }
    }

    /// <summary>
    /// Internal Helper for the NamespaceRemoved Event which raises it only when a Handler is registered.
    /// </summary>
    /// <param name="prefix">Namespace Prefix.</param>
    /// <param name="uri">Namespace Uri.</param>
    protected virtual void OnNamespaceRemoved(string prefix, Uri uri)
    {
        NamespaceChanged handler = NamespaceRemoved;
        if (handler != null)
        {
            handler(prefix, uri);
        }
    }

    #region IDisposable Members

    /// <summary>
    /// Disposes of a Namespace Map.
    /// </summary>
    public void Dispose()
    {
        _prefixes.Clear();
        _uris.Clear();
    }

    #endregion
}

/// <summary>
/// Class for representing Mappings from URIs to QNames.
/// </summary>
/// <remarks>
/// Used primarily in outputting RDF syntax.
/// </remarks>
public class QNameOutputMapper 
    : NamespaceMapper
{
    /// <summary>
    /// Mapping of URIs to QNames.
    /// </summary>
    protected readonly MultiDictionary<string, QNameMapping> Mapping = new();

    /// <summary>
    /// Next available Temporary Namespace ID.
    /// </summary>
    private int _nextNamespaceId;

    /// <summary>
    /// Creates a new QName Output Mapper using the given Namespace Map.
    /// </summary>
    /// <param name="nsmapper">Namespace Map.</param>
    /// <param name="uriFactory">The factory to use when creating new Uri instances. If not specified, defaults to <see cref="UriFactory.Root"/>.</param>
    public QNameOutputMapper(INamespaceMapper nsmapper, IUriFactory uriFactory=null)
        : base(nsmapper, uriFactory ?? UriFactory.Root) { }

    /// <summary>
    /// Creates a new QName Output Mapper which has an empty Namespace Map.
    /// </summary>
    /// <param name="uriFactory">The factory to use when creating new Uri instances. If not specified, defaults to <see cref="UriFactory.Root"/>.</param>
    public QNameOutputMapper(IUriFactory uriFactory = null)
        : base(uriFactory ?? UriFactory.Root, true) { }

    /// <summary>
    /// A Function which attempts to reduce a Uri to a QName.
    /// </summary>
    /// <param name="uri">The Uri to attempt to reduce.</param>
    /// <param name="qname">The value to output the QName to if possible.</param>
    /// <param name="validationFunction">OPTIONAL: a function which when applied to a candidate QName string returns true if the string is acceptable, and false otherwise.</param>
    /// <returns></returns>
    /// <remarks>This function will return a Boolean indicated whether it succeeded in reducing the Uri to a QName.  If it did then the out parameter <paramref name="qname"/> will contain the reduction, otherwise it will be the empty string.</remarks>
    public override bool ReduceToQName(string uri, out string qname, Func<string, bool> validationFunction = null)
    {
        // See if we've cached this mapping
        if (Mapping.TryGetValue(uri, out QNameMapping mapping))
        {
            qname = mapping.QName;
            return true;
        }
        mapping = new QNameMapping(uri);

        validationFunction ??= DefaultQNameValidationFunction;
        foreach (Uri u in _uris.Values)
        {
            var baseUri = u.AbsoluteUri;

            // Does the Uri start with the Base Uri
            if (uri.StartsWith(baseUri))
            {
                // Remove the Base Uri from the front of the Uri
                qname = uri.Substring(baseUri.Length);
                // Add the Prefix back onto the front plus the colon to give a QName
                if (_prefixes.ContainsKey(u.GetEnhancedHashCode()))
                {
                    qname = _prefixes[u.GetEnhancedHashCode()][0] + ":" + qname;
                    if (qname.Equals(":")) continue;
                    if (!validationFunction(qname)) continue;
                    // Cache the Mapping
                    mapping.QName = qname;
                    AddToCache(uri, mapping);
                    return true;
                }
            }
        }

        // Failed to find a Reduction
        qname = string.Empty;
        return false;
    }

    /// <summary>
    /// A Function which attempts to reduce a Uri to a QName and issues a Temporary Namespace if required.
    /// </summary>
    /// <param name="uri">The Uri to attempt to reduce.</param>
    /// <param name="qName">The value to output the QName to if possible.</param>
    /// <param name="tempNamespace">The Temporary Namespace issued (if any).</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This function will always returns a possible QName for the URI if the format of the URI permits it.  It doesn't guarantee that the QName will be valid for the syntax it is being written to - it is up to implementers of writers to validate the QNames returned.
    /// </para>
    /// <para>
    /// Where necessary a Temporary Namespace will be issued and the <paramref name="tempNamespace">tempNamespace</paramref> parameter will be set to the prefix of the new temporary namespace.
    /// </para>
    /// </remarks>
    public bool ReduceToQName(string uri, out string qName, out string tempNamespace)
    {
        tempNamespace = string.Empty;

        // See if we've cached this mapping
        if (Mapping.TryGetValue(uri, out QNameMapping mapping))
        {
            qName = mapping.QName;
            return true;
        }
        mapping = new QNameMapping(uri);

        // Try and find a Namespace URI that is the prefix of the URI
        foreach (Uri u in _uris.Values)
        {
            var baseUri = u.AbsoluteUri;

            // Does the Uri start with the Base Uri
            if (uri.StartsWith(baseUri))
            {
                // Remove the Base Uri from the front of the Uri
                qName = uri.Substring(baseUri.Length);
                // Add the Prefix back onto the front plus the colon to give a QName
                if (_prefixes.ContainsKey(u.GetEnhancedHashCode()))
                {
                    qName = _prefixes[u.GetEnhancedHashCode()][0] + ":" + qName;
                    if (qName.Equals(":")) continue;
                    if (qName.Contains("/") || qName.Contains("#")) continue;
                    // Cache the Mapping
                    mapping.QName = qName;
                    AddToCache(uri, mapping);
                    return true;
                }
            }
        }

        // Try and issue a Temporary Namespace
        string nsUri, nsPrefix;
        if (uri.Contains('#'))
        {
            nsUri = uri.Substring(0, uri.LastIndexOf('#') + 1);
            nsPrefix = GetNextTemporaryNamespacePrefix();
        }
        else if (uri.LastIndexOf('/') > 8)
        {
            nsUri = uri.Substring(0, uri.LastIndexOf('/') + 1);
            nsPrefix = GetNextTemporaryNamespacePrefix();
        }
        else
        {
            // Failed to find a Reduction and unable to issue a Temporary Namespace
            qName = string.Empty;
            return false;
        }

        // Add to Namespace Map
        AddNamespace(nsPrefix, _uriFactory.Create(nsUri));

        // Cache mapping and return
        mapping.QName = nsPrefix + ":" + uri.Replace(nsUri, string.Empty);
        AddToCache(uri, mapping);
        qName = mapping.QName;
        tempNamespace = nsPrefix;
        return true;
    }

    /// <summary>
    /// Adds a QName mapping to the cache.
    /// </summary>
    /// <param name="uri">URI.</param>
    /// <param name="mapping">Mapping.</param>
    protected virtual void AddToCache(string uri, QNameMapping mapping)
    {
        Mapping.Add(uri, mapping);
    }

    /// <summary>
    /// Gets the next available Temporary Namespace ID.
    /// </summary>
    /// <returns></returns>
    private string GetNextTemporaryNamespacePrefix()
    {
        const string nextPrefixBase = "ns";
        while (_uris.ContainsKey(nextPrefixBase + _nextNamespaceId))
        {
            _nextNamespaceId++;
        }
        return nextPrefixBase + _nextNamespaceId;
    }
}

/// <summary>
/// Thread Safe version of the <see cref="QNameOutputMapper">QNameOutputMapper</see>.
/// </summary>
public class ThreadSafeQNameOutputMapper
    : QNameOutputMapper
{
    /// <summary>
    /// Creates a new Thread Safe QName Output Mapper.
    /// </summary>
    /// <param name="nsmapper">Namespace Mapper.</param>
    /// <param name="uriFactory">The factory to use when creating new Uri instances. If not specified, defaults to <see cref="UriFactory.Root"/>.</param>
    public ThreadSafeQNameOutputMapper(INamespaceMapper nsmapper, IUriFactory uriFactory = null)
        : base(nsmapper, uriFactory ?? UriFactory.Root) { }

    /// <summary>
    /// Adds a QName Mapping to the Cache in a Thread Safe way.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    protected override void AddToCache(string key, QNameMapping value)
    {
        try
        {
            Monitor.Enter(Mapping);
            base.AddToCache(key, value);
        }
        finally
        {
            Monitor.Exit(Mapping);
        }
    }

    /// <summary>
    /// Adds a Namespace to the QName Output Mapper.
    /// </summary>
    /// <param name="prefix">Prefix.</param>
    /// <param name="uri">Namespace URI.</param>
    public override void AddNamespace(string prefix, Uri uri)
    {
        try
        {
            Monitor.Enter(_prefixes);
            Monitor.Enter(_uris);
            base.AddNamespace(prefix, uri);
        }
        finally
        {
            Monitor.Exit(_prefixes);
            Monitor.Exit(_uris);
        }
    }
}
