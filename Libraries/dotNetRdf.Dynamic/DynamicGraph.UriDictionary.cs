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

namespace VDS.RDF.Dynamic;

public partial class DynamicGraph : IDictionary<Uri, object>
{
    /// <inheritdoc/>
    ICollection<Uri> IDictionary<Uri, object>.Keys
    {
        get
        {
            return UriPairs.Keys;
        }
    }

    private IDictionary<Uri, object> UriPairs
    {
        get
        {
            return UriNodes
                .ToDictionary(
                    subject => subject.Uri,
                    subject => this[subject]);
        }
    }

    /// <summary>
    /// Gets nodes equivalent to <paramref name="node"/> or sets statements with subject equivalent to <paramref name="node"/> and predicate and objects equivalent to <paramref name="value"/>.
    /// </summary>
    /// <param name="node">The node to wrap dynamically.</param>
    /// <returns>A <see cref="DynamicNode"/> wrapped around the <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="node"/> is null.</exception>
    public object this[Uri node]
    {
        get
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return this[Convert(node)];
        }

        set
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            this[Convert(node)] = value;
        }
    }

    /// <summary>
    /// Asserts statements equivalent to the parameters.
    /// </summary>
    /// <param name="subject">The subject to assert.</param>
    /// <param name="predicateAndObjects">An object with public properties or a dictionary representing predicates and objects to assert.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="subject"/>.</exception>
    public void Add(Uri subject, object predicateAndObjects)
    {
        if (subject is null)
        {
            throw new ArgumentNullException(nameof(subject));
        }

        Add(Convert(subject), predicateAndObjects);
    }

    /// <inheritdoc/>
    void ICollection<KeyValuePair<Uri, object>>.Add(KeyValuePair<Uri, object> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// Checks whether statements exist equivalent to the parameters.
    /// </summary>
    /// <param name="subject">The subject to check.</param>
    /// <param name="predicateAndObjects">An object with public properties or a dictionary representing predicates and objects to check.</param>
    /// <returns>Whether statements exist equivalent to the parameters.</returns>
    public bool Contains(Uri subject, object predicateAndObjects)
    {
        if (subject is null)
        {
            return false;
        }

        return Contains(Convert(subject), predicateAndObjects);
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<Uri, object>>.Contains(KeyValuePair<Uri, object> item)
    {
        return Contains(item.Key, item.Value);
    }

    /// <summary>
    /// Checks whether a URI node equivalent to <paramref name="key"/> exists.
    /// </summary>
    /// <param name="key">The node to check.</param>
    /// <returns>Whether a URI node equivalent to <paramref name="key"/> exists.</returns>
    public bool ContainsKey(Uri key)
    {
        if (key is null)
        {
            return false;
        }

        return ContainsKey(Convert(key));
    }

    /// <inheritdoc/>
    void ICollection<KeyValuePair<Uri, object>>.CopyTo(KeyValuePair<Uri, object>[] array, int arrayIndex)
    {
        UriPairs.ToArray().CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<Uri, object>> IEnumerable<KeyValuePair<Uri, object>>.GetEnumerator()
    {
        return UriPairs.GetEnumerator();
    }

    /// <summary>
    /// Retracts statements with <paramref name="subject"/>.
    /// </summary>
    /// <param name="subject">The subject to retract.</param>
    /// <returns>Whether any statements were retracted.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="subject"/> is null.</exception>
    public bool Remove(Uri subject)
    {
        if (subject is null)
        {
            throw new ArgumentNullException(nameof(subject));
        }

        return Remove(Convert(subject));
    }

    /// <summary>
    /// Retracts statements equivalent to  parameters.
    /// </summary>
    /// <param name="subject">The subject to retract.</param>
    /// <param name="predicateAndObjects">An object with public properties or a dictionary representing predicates and objects to retract.</param>
    /// <returns>Whether any statements were retracted.</returns>
    public bool Remove(Uri subject, object predicateAndObjects)
    {
        if (subject is null)
        {
            return false;
        }

        return Remove(Convert(subject), predicateAndObjects);
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<Uri, object>>.Remove(KeyValuePair<Uri, object> item)
    {
        return Remove(item.Key, item.Value);
    }

    /// <summary>
    /// Tries to get a node from the graph.
    /// </summary>
    /// <param name="node">The node to try.</param>
    /// <param name="value">A <see cref="DynamicNode"/> wrapped around the <paramref name="node"/>.</param>
    /// <returns>A value representing whether a <paramref name="value"/> was set or not.</returns>
    public bool TryGetValue(Uri node, out object value)
    {
        value = null;

        if (node is null)
        {
            return false;
        }

        return TryGetValue(Convert(node), out value);
    }

    private INode Convert(Uri subject)
    {
        return subject.AsUriNode(this, SubjectBaseUri);
    }
}
