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
using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF;

/// <summary>
/// Class for representing RDF Triples in memory.
/// </summary>
public sealed class Triple
    : IComparable<Triple>
{
    // private readonly IRefNode _graphName;
    private readonly int _hashcode;

    /// <summary>
    /// Constructs a Triple from Nodes that belong to the same Graph/Node Factory.
    /// </summary>
    /// <param name="subj">Subject of the Triple.</param>
    /// <param name="pred">Predicate of the Triple.</param>
    /// <param name="obj">Object of the Triple.</param>
    /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory.</remarks>
    /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory.</exception>
    public Triple(INode subj, INode pred, INode obj)
    {
        // Store the Three Nodes of the Triple
        Subject = subj;
        Predicate = pred;
        Object = obj;

        // Compute Hash Code
        _hashcode = Tools.CombineHashCodes(Subject, Predicate, Object);
    }

    /// <summary>
    /// Constructs a Triple from Nodes that belong to the same Graph/Node Factory and associates this Triple with the given Graph (doesn't assert the Triple).
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <param name="g">Graph.</param>
    /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory.</remarks>
    /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory.</exception>
    [Obsolete("This constructor is obsolete and will be removed in a future version.")]
    public Triple(INode subj, INode pred, INode obj, IGraph g)
        : this(subj, pred, obj)
    {
        // Graph = g;
    }

    /// <summary>
    /// Constructs a Triple from Nodes that belong to the same Graph/Node Factory with some Context.
    /// </summary>
    /// <param name="subj">Subject of the Triple.</param>
    /// <param name="pred">Predicate of the Triple.</param>
    /// <param name="obj">Object of the Triple.</param>
    /// <param name="context">Context Information for the Triple.</param>
    /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory.</remarks>
    /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory.</exception>
    public Triple(INode subj, INode pred, INode obj, ITripleContext context)
        : this(subj, pred, obj)
    {
        Context = context;
    }

    ///// <summary>
    ///// Creates a Triple and associates it with the given Graph URI permanently (though not with a specific Graph as such).
    ///// </summary>
    ///// <param name="subj">Subject of the Triple.</param>
    ///// <param name="pred">Predicate of the Triple.</param>
    ///// <param name="obj">Object of the Triple.</param>
    ///// <param name="graphUri">Graph URI.</param>
    ///// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory.</remarks>
    ///// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory.</exception>
    //[Obsolete("This constructor is obsolete and will be remove in a future version.")]
    //public Triple(INode subj, INode pred, INode obj, Uri graphUri)
    //    : this(subj, pred, obj)
    //{
    //    // _graphName = graphUri == null ? null : new UriNode(graphUri);
    //}

    ///// <summary>
    ///// Constructs a Triple from Nodes that belong to the same Graph/Node Factory with some Context.
    ///// </summary>
    ///// <param name="subj">Subject of the Triple.</param>
    ///// <param name="pred">Predicate of the Triple.</param>
    ///// <param name="obj">Object of the Triple.</param>
    ///// <param name="context">Context Information for the Triple.</param>
    ///// <param name="graphUri">Graph URI.</param>
    ///// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory.</remarks>
    ///// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory.</exception>
    //[Obsolete("This constructor is obsolete and will be remove in a future version.")]
    //public Triple(INode subj, INode pred, INode obj, ITripleContext context, Uri graphUri)
    //    : this(subj, pred, obj, graphUri)
    //{
    //    Context = context;
    //}

    ///// <summary>
    ///// Constructs a triple associated with the specified graph.
    ///// </summary>
    ///// <param name="subj">Subject of the Triple.</param>
    ///// <param name="pred">Predicate of the Triple.</param>
    ///// <param name="obj">Object of the Triple.</param>
    ///// <param name="graph">Name of the graph the triple is associated with</param>
    //[Obsolete("This constructor is obsolete and will be remove in a future version.")]
    //public Triple(IRefNode subj, IRefNode pred, INode obj, IRefNode graph):
    //    this(subj, pred, obj)
    //{
    //    _graphName = graph;
    //}

    /// <summary>
    /// Gets the Subject of the Triple.
    /// </summary>
    public INode Subject { get; private set; }

    /// <summary>
    /// Gets the Predicate of the Triple.
    /// </summary>
    public INode Predicate { get; private set; }

    /// <summary>
    /// Gets the Object of the Triple.
    /// </summary>
    public INode Object { get; private set; }

    /// <summary>
    /// Gets the Context Information for this Triple.
    /// </summary>
    /// <remarks>
    /// Context may be null where no Context for the Triple has been defined.
    /// </remarks>
    public ITripleContext Context { get; set; } = null;

    /// <summary>
    /// Gets an enumeration of the Nodes in the Triple.
    /// </summary>
    /// <remarks>
    /// Returned as subject, predicate, object.
    /// </remarks>
    public IEnumerable<INode> Nodes => new List<INode> { Subject, Predicate, Object };

    /// <summary>
    /// Gets whether the Triple is a Ground Triple.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <strong>Ground Triple</strong> is any Triple considered to state a single fixed fact.  In practice this means that the Triple does not contain any Blank Nodes, and does not quote any triples containing blank nodes.
    /// </para>
    /// </remarks>
    public bool IsGroundTriple => Subject.NodeType != NodeType.Blank && Predicate.NodeType != NodeType.Blank &&
                                  Object.NodeType != NodeType.Blank &&
                                  (Subject is not ITripleNode stn || stn.Triple.IsGroundTriple) &&
                                  (Object is not ITripleNode otn || otn.Triple.IsGroundTriple);

    /// <summary>
    /// Checks whether the Triple involves a given Node.
    /// </summary>
    /// <param name="n">The Node to test upon.</param>
    /// <returns>True if the Triple contains the given node or quotes a triple that involves the given node.</returns>
    public bool Involves(INode n)
    {
        return Subject.Equals(n) || Predicate.Equals(n) || Object.Equals(n) ||
               (Subject is ITripleNode stn && stn.Triple.Involves(n)) ||
               (Object is ITripleNode otn && otn.Triple.Involves(n));
    }

    /// <summary>
    /// Checks whether the Triple involves a given Uri.
    /// </summary>
    /// <param name="uri">The Uri to test upon.</param>
    /// <returns>True if the Triple has a UriNode with the given Uri.</returns>
    public bool Involves(Uri uri)
    {
        IUriNode temp = new UriNode(uri);
        return Involves(temp);
    }

    /// <summary>
    /// Indicates whether the Triple has the given Node as the Subject.
    /// </summary>
    /// <param name="n">Node to test upon.</param>
    /// <returns></returns>
    public bool HasSubject(INode n)
    {
        // return this._subject.GetHashCode().Equals(n.GetHashCode());
        return Subject.Equals(n);
    }

    /// <summary>
    /// Indicates whether the Triple has the given Node as the Predicate.
    /// </summary>
    /// <param name="n">Node to test upon.</param>
    /// <returns></returns>
    public bool HasPredicate(INode n)
    {
        // return this._predicate.GetHashCode().Equals(n.GetHashCode());
        return Predicate.Equals(n);
    }

    /// <summary>
    /// Indicates whether the Triple has the given Node as the Object.
    /// </summary>
    /// <param name="n">Node to test upon.</param>
    /// <returns></returns>
    public bool HasObject(INode n)
    {
        // return this._object.GetHashCode().Equals(n.GetHashCode());
        return Object.Equals(n);
    }

    /// <summary>
    /// Implementation of Equality for Triples.
    /// </summary>
    /// <param name="obj">Object to compare with.</param>
    /// <returns></returns>
    /// <remarks>
    /// Triples are considered equal on the basis of two things:.
    /// <ol>
    /// <li>The Hash Codes of the Triples are identical</li>
    /// <li>The logical conjunction (AND) of the equality of the Subject, Predicate and Object is true.  Each pair of Nodes must either be Equal using Node Equality or are both Blank Nodes and have identical Node IDs (i.e. are indistinguishable for equality purposes on a single Triple level)</li>
    /// </ol>
    /// </remarks>
    public override bool Equals(object obj)
    {
        if (obj is Triple other)
        {
            // Subject, Predicate and Object must all be equal
            // Either the Nodes must be directly equal or they must both be Blank Nodes with identical Node IDs
            // Use lazy evaluation as far as possible
            return (Subject.Equals(other.Subject) || (Subject.NodeType == NodeType.Blank &&
                                                     other.Subject.NodeType == NodeType.Blank &&
                                                     Subject.ToString().Equals(other.Subject.ToString())))
                   && (Predicate.Equals(other.Predicate) || (Predicate.NodeType == NodeType.Blank &&
                                                            other.Predicate.NodeType == NodeType.Blank &&
                                                            Predicate.ToString().Equals(other.Predicate.ToString())))
                   && (Object.Equals(other.Object) || (Object.NodeType == NodeType.Blank &&
                                                      other.Object.NodeType == NodeType.Blank &&
                                                      Object.ToString().Equals(other.Object.ToString())));

        }

        // Can only be equal to other Triples
        return false;
    }

    /// <summary>
    /// Implementation of Hash Codes for Triples.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Returns the Hash Code of the Triple which is calculated as the Hash Code of the String formed by concatenating the Hash Codes of its constituent Nodes.  This Hash Code is precomputed in the Constructor of a Triple since it will be used a lot (in Triple Equality calculation, Triple Collections etc).
    /// </para>
    /// <para>
    /// Since Hash Codes are based on a String representation there is no guarantee of uniqueness though the same Triple will always give the same Hash Code (on a given Platform - see the MSDN Documentation for <see cref="string.GetHashCode">string.GetHashCode()</see> for further details).
    /// </para>
    /// </remarks>
    public override int GetHashCode()
    {
        return _hashcode;
    }

    /// <summary>
    /// Gets a String representation of a Triple in the form 'Subject , Predicate , Object'.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var outString = new StringBuilder();
        outString.Append(Subject.ToString());
        outString.Append(" , ");
        outString.Append(Predicate.ToString());
        outString.Append(" , ");
        outString.Append(Object.ToString());

        return outString.ToString();
    }

    /// <summary>
    /// Gets a String representation of a Triple in the form 'Subject , Predicate , Object' with optional compression of URIs to QNames.
    /// </summary>
    /// <param name="compress">Controls whether URIs will be compressed to QNames in the String representation.</param>
    /// <returns></returns>
    /// <remarks>As of dotNetRdf 3.0, this method is obsolete and does not perform an URI to QName compression. To produce a compressed string, use
    /// the <see cref="ToString(ITripleFormatter)"/> overload passing in a formatter configured with the desired <see cref="INamespaceMapper"/>.</remarks>
    [Obsolete("This method is obsolete and will be removed in a future version. Use ToString() or ToString(ITripleFormatter) instead.")]
    public string ToString(bool compress)
    {
        //if (!compress || Graph == null)
        //{
        //    return ToString();
        //}
        //else
        //{
        //    var formatter = new TurtleFormatter(Graph.NamespaceMap);
        //    return formatter.Format(this);
        //}
        return ToString();
    }

    /// <summary>
    /// Gets the String representation of a Triple using the given Triple Formatter.
    /// </summary>
    /// <param name="formatter">Formatter.</param>
    /// <returns></returns>
    public string ToString(ITripleFormatter formatter)
    {
        return formatter.Format(this);
    }

    /// <summary>
    /// Implementation of CompareTo for Triples which allows Triples to be sorted.
    /// </summary>
    /// <param name="other">Triple to compare to.</param>
    /// <returns></returns>
    /// <remarks>Triples are Ordered by Subjects, Predicates and then Objects.  Triples are only partially orderable since the CompareTo methods on Nodes only define a partial ordering over Nodes.</remarks>
    public int CompareTo(Triple other)
    {
        if (other == null)
        {
            // Everything is greater than a null
            // Return a 1 to indicate this
            return 1;
        }
        else
        {
            int s, p;

            // Compare Subjects
            s = Subject.CompareTo(other.Subject);
            if (s == 0)
            {
                // Compare Predicates
                p = Predicate.CompareTo(other.Predicate);
                if (p == 0)
                {
                    // Compare Objects
                    return Object.CompareTo(other.Object);
                }
                else
                {
                    return p;
                }
            }
            else
            {
                return s;
            }
        }
    }
}
