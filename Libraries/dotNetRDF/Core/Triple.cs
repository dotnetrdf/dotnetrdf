/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Writing.Serialization;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing RDF Triples in memory
    /// </summary>
#if !NETCORE
    [Serializable,XmlRoot(ElementName="triple")]
#endif
    public sealed class Triple
        : IComparable<Triple>
#if !NETCORE
        , ISerializable, IXmlSerializable
#endif
    {
        private INode _subject, _predicate, _object;
        private ITripleContext _context = null;
        private Uri _u = null;
        private IGraph _g = null;
        private int _hashcode;

        /// <summary>
        /// Constructs a Triple from Nodes that belong to the same Graph/Node Factory
        /// </summary>
        /// <param name="subj">Subject of the Triple</param>
        /// <param name="pred">Predicate of the Triple</param>
        /// <param name="obj">Object of the Triple</param>
        /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory</remarks>
        /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory</exception>
        public Triple(INode subj, INode pred, INode obj)
        {
            // Require that all Nodes belong to the same Graph
            if (!ReferenceEquals(subj.Graph, pred.Graph) || !ReferenceEquals(pred.Graph, obj.Graph))
            {
                throw new RdfException("Subject, Predicate and Object Nodes must all come from the same Graph/Node Factory - use Tools.CopyNode() to transfer nodes between Graphs");
            }
            else
            {
                // Set the Graph property from the Subject
                _g = subj.Graph;

                // Store the Three Nodes of the Triple
                _subject = subj;
                _predicate = pred;
                _object = obj;

                // Compute Hash Code
                _hashcode = (_subject.GetHashCode().ToString() + _predicate.GetHashCode().ToString() + _object.GetHashCode().ToString()).GetHashCode();
            }
        }

        /// <summary>
        /// Constructs a Triple from Nodes that belong to the same Graph/Node Factory and associates this Triple with the given Graph (doesn't assert the Triple)
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <param name="g">Graph</param>
        /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory</remarks>
        /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory</exception>
        public Triple(INode subj, INode pred, INode obj, IGraph g)
            : this(subj, pred, obj)
        {
            _g = g;
        }

        /// <summary>
        /// Constructs a Triple from Nodes that belong to the same Graph/Node Factory with some Context
        /// </summary>
        /// <param name="subj">Subject of the Triple</param>
        /// <param name="pred">Predicate of the Triple</param>
        /// <param name="obj">Object of the Triple</param>
        /// <param name="context">Context Information for the Triple</param>
        /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory</remarks>
        /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory</exception>
        public Triple(INode subj, INode pred, INode obj, ITripleContext context)
            : this(subj, pred, obj)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a Triple and associates it with the given Graph URI permanently (though not with a specific Graph as such)
        /// </summary>
        /// <param name="subj">Subject of the Triple</param>
        /// <param name="pred">Predicate of the Triple</param>
        /// <param name="obj">Object of the Triple</param>
        /// <param name="graphUri">Graph URI</param>
        /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory</remarks>
        /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory</exception>
        public Triple(INode subj, INode pred, INode obj, Uri graphUri)
            : this(subj, pred, obj)
        {
            _u = graphUri;
        }

        /// <summary>
        /// Constructs a Triple from Nodes that belong to the same Graph/Node Factory with some Context
        /// </summary>
        /// <param name="subj">Subject of the Triple</param>
        /// <param name="pred">Predicate of the Triple</param>
        /// <param name="obj">Object of the Triple</param>
        /// <param name="context">Context Information for the Triple</param>
        /// <param name="graphUri">Graph URI</param>
        /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph/Node Factory</remarks>
        /// <exception cref="RdfException">Thrown if the Nodes aren't all from the same Graph/Node Factory</exception>
        public Triple(INode subj, INode pred, INode obj, ITripleContext context, Uri graphUri)
            : this(subj, pred, obj, graphUri)
        {
            _context = context;
        }

        private Triple()
        { }

#if !NETCORE
        private Triple(SerializationInfo info, StreamingContext context)
        {
            _subject = (INode)info.GetValue("s", typeof(INode));
            _predicate = (INode)info.GetValue("p", typeof(INode));
            _object = (INode)info.GetValue("o", typeof(INode));

            // Compute Hash Code
            _hashcode = (_subject.GetHashCode().ToString() + _predicate.GetHashCode().ToString() + _object.GetHashCode().ToString()).GetHashCode();
        }
#endif

        /// <summary>
        /// Gets the Subject of the Triple
        /// </summary>
        public INode Subject
        {
            get
            {
                return _subject;
            }
        }

        /// <summary>
        /// Gets the Predicate of the Triple
        /// </summary>
        public INode Predicate
        {
            get
            {
                return _predicate;
            }
        }

        /// <summary>
        /// Gets the Object of the Triple
        /// </summary>
        public INode Object
        {
            get
            {
                return _object;
            }
        }

        /// <summary>
        /// Gets the Graph this Triple was created for
        /// </summary>
        /// <remarks>This is not necessarily the actual Graph this Triple is asserted in since this property is set from the Subject of the Triple when it is created and it is possible to create a Triple without asserting it into an actual Graph or to then assert it into a different Graph.</remarks>
        public IGraph Graph
        {
            get
            {
                return _g;
            }
        }

        /// <summary>
        /// Gets the Uri of the Graph this Triple was created for
        /// </summary>
        /// <remarks>This is not necessarily the actual Graph Uri of the Graph this Triple is asserted in since this property is set from the Subject of the Triple when it is created and it is possible to create a Triple without asserting it into an actual Graph or to then assert it into a different Graph.</remarks>
        public Uri GraphUri
        {
            get
            {
                if (_u != null)
                {
                    return _u;
                }
                else if (_g == null)
                {
                    return null;
                }
                else
                {
                    return _g.BaseUri;
                }
            }
        }

        /// <summary>
        /// Gets the Context Information for this Triple
        /// </summary>
        /// <remarks>
        /// Context may be null where no Context for the Triple has been defined
        /// </remarks>
        public ITripleContext Context
        {
            get
            {
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        /// <summary>
        /// Gets an enumeration of the Nodes in the Triple
        /// </summary>
        /// <remarks>
        /// Returned as subject, predicate, object
        /// </remarks>
        public IEnumerable<INode> Nodes
        {
            get
            {
                return new List<INode> { _subject, _predicate, _object };
            }
        }

        /// <summary>
        /// Gets whether the Triple is a Ground Triple
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <strong>Ground Triple</strong> is any Triple considered to state a single fixed fact.  In practise this means that the Triple does not contain any Blank Nodes.
        /// </para>
        /// </remarks>
        public bool IsGroundTriple
        {
            get
            {
                return (_subject.NodeType != NodeType.Blank && _predicate.NodeType != NodeType.Blank && _object.NodeType != NodeType.Blank);
            }
        }

        /// <summary>
        /// Checks whether the Triple involves a given Node
        /// </summary>
        /// <param name="n">The Node to test upon</param>
        /// <returns>True if the Triple contains the given Node</returns>
        public bool Involves(INode n)
        {
            return (_subject.Equals(n) || _predicate.Equals(n) || _object.Equals(n));
        }

        /// <summary>
        /// Checks whether the Triple involves a given Uri
        /// </summary>
        /// <param name="uri">The Uri to test upon</param>
        /// <returns>True if the Triple has a UriNode with the given Uri</returns>
        public bool Involves(Uri uri)
        {
            IUriNode temp = new UriNode(null, uri);

            // Does the Subject involve this Uri?
            if (_subject.Equals(temp)) return true;
            // Does the Predicate involve this Uri?
            if (_predicate.Equals(temp)) return true;
            // Does the Object involve this Uri?
            if (_object.Equals(temp)) return true;
            // Not Involved!
            return false;
        }

        /// <summary>
        /// Indicates whether the Triple has the given Node as the Subject
        /// </summary>
        /// <param name="n">Node to test upon</param>
        /// <returns></returns>
        public bool HasSubject(INode n)
        {
            // return this._subject.GetHashCode().Equals(n.GetHashCode());
            return _subject.Equals(n);
        }

        /// <summary>
        /// Indicates whether the Triple has the given Node as the Predicate
        /// </summary>
        /// <param name="n">Node to test upon</param>
        /// <returns></returns>
        public bool HasPredicate(INode n)
        {
            // return this._predicate.GetHashCode().Equals(n.GetHashCode());
            return _predicate.Equals(n);
        }

        /// <summary>
        /// Indicates whether the Triple has the given Node as the Object
        /// </summary>
        /// <param name="n">Node to test upon</param>
        /// <returns></returns>
        public bool HasObject(INode n)
        {
            // return this._object.GetHashCode().Equals(n.GetHashCode());
            return _object.Equals(n);
        }

        /// <summary>
        /// Implementation of Equality for Triples
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns></returns>
        /// <remarks>
        /// Triples are considered equal on the basis of two things:
        /// <ol>
        /// <li>The Hash Codes of the Triples are identical</li>
        /// <li>The logical conjunction (AND) of the equality of the Subject, Predicate and Object is true.  Each pair of Nodes must either be Equal using Node Equality or are both Blank Nodes and have identical Node IDs (i.e. are indistinguishable for equality purposes on a single Triple level)</li>
        /// </ol>
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj is Triple)
            {
                Triple temp = (Triple)obj;

                // Subject, Predicate and Object must all be equal
                // Either the Nodes must be directly equal or they must both be Blank Nodes with identical Node IDs
                // Use lazy evaluation as far as possible
                return (_subject.Equals(temp.Subject) || (_subject.NodeType == NodeType.Blank && temp.Subject.NodeType == NodeType.Blank && _subject.ToString().Equals(temp.Subject.ToString())))
                       && (_predicate.Equals(temp.Predicate) || (_predicate.NodeType == NodeType.Blank && temp.Predicate.NodeType == NodeType.Blank && _predicate.ToString().Equals(temp.Predicate.ToString())))
                       && (_object.Equals(temp.Object) || (_object.NodeType == NodeType.Blank && temp.Object.NodeType == NodeType.Blank && _object.ToString().Equals(temp.Object.ToString())));

             }
            else
            {
                // Can only be equal to other Triples
                return false;
            }
        }

        /// <summary>
        /// Implementation of Hash Codes for Triples
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Returns the Hash Code of the Triple which is calculated as the Hash Code of the String formed by concatenating the Hash Codes of its constituent Nodes.  This Hash Code is precomputed in the Constructor of a Triple since it will be used a lot (in Triple Equality calculation, Triple Collections etc)
        /// </para>
        /// <para>
        /// Since Hash Codes are based on a String representation there is no guarantee of uniqueness though the same Triple will always give the same Hash Code (on a given Platform - see the MSDN Documentation for <see cref="string.GetHashCode">string.GetHashCode()</see> for further details)
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            return _hashcode;
        }

        /// <summary>
        /// Gets a String representation of a Triple in the form 'Subject , Predicate , Object'
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder outString = new StringBuilder();
            outString.Append(_subject.ToString());
            outString.Append(" , ");
            outString.Append(_predicate.ToString());
            outString.Append(" , ");
            outString.Append(_object.ToString());

            return outString.ToString();
        }

        /// <summary>
        /// Gets a String representation of a Triple in the form 'Subject , Predicate , Object' with optional compression of URIs to QNames
        /// </summary>
        /// <param name="compress">Controls whether URIs will be compressed to QNames in the String representation</param>
        /// <returns></returns>
        public string ToString(bool compress)
        {
            if (!compress || _g == null)
            {
                return ToString();
            }
            else
            {
                TurtleFormatter formatter = new TurtleFormatter(_g.NamespaceMap);
                return formatter.Format(this);
            }
        }

        /// <summary>
        /// Gets the String representation of a Triple using the given Triple Formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <returns></returns>
        public string ToString(ITripleFormatter formatter)
        {
            return formatter.Format(this);
        }

        /// <summary>
        /// Implementation of CompareTo for Triples which allows Triples to be sorted
        /// </summary>
        /// <param name="other">Triple to compare to</param>
        /// <returns></returns>
        /// <remarks>Triples are Ordered by Subjects, Predicates and then Objects.  Triples are only partially orderable since the CompareTo methods on Nodes only define a partial ordering over Nodes</remarks>
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

#if !NETCORE

        #region ISerializable Members

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serilization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("s", _subject);
            info.AddValue("p", _predicate);
            info.AddValue("o", _object);
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Gets the schema for XML serialization
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads the data for XML deserialization
        /// </summary>
        /// <param name="reader">XML Reader</param>
        public void ReadXml(XmlReader reader)
        {
            reader.Read();
            _subject = reader.DeserializeNode();
            _predicate = reader.DeserializeNode();
            _object = reader.DeserializeNode();

            // Compute Hash Code
            _hashcode = (_subject.GetHashCode().ToString() + _predicate.GetHashCode().ToString() + _object.GetHashCode().ToString()).GetHashCode();
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public void WriteXml(XmlWriter writer)
        {
            _subject.SerializeNode(writer);
            _predicate.SerializeNode(writer);
            _object.SerializeNode(writer);
        }

        #endregion

#endif
    }
}
