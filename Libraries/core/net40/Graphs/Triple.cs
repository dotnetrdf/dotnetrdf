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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;
#if !SILVERLIGHT
using VDS.RDF.Writing.Serialization;

#endif

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Class for representing RDF Triples in memory
    /// </summary>
#if !SILVERLIGHT
    [Serializable, XmlRoot(ElementName = "triple")]
#endif
    public sealed class Triple
        : IEquatable<Triple>, IComparable<Triple>
#if !SILVERLIGHT
            , ISerializable, IXmlSerializable
#endif
    {
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
            if (subj == null) throw new ArgumentNullException("subj");
            if (pred == null) throw new ArgumentNullException("pred");
            if (obj == null) throw new ArgumentNullException("obj");

            //Store the Three Nodes of the Triple
            this.Subject = subj;
            this.Predicate = pred;
            this.Object = obj;

            //Compute Hash Code
            this._hashcode = Tools.CreateHashCode(this);
        }

#if !SILVERLIGHT

        /// <summary>
        /// Deserialization only constructor
        /// </summary>
        private Triple() {}

        private Triple(SerializationInfo info, StreamingContext context)
        {
            this.Subject = (INode) info.GetValue("s", typeof (INode));
            this.Predicate = (INode) info.GetValue("p", typeof (INode));
            this.Object = (INode) info.GetValue("o", typeof (INode));

            //Compute Hash Code
            this._hashcode = Tools.CreateHashCode(this);
        }
#endif

        /// <summary>
        /// Gets the Subject of the Triple
        /// </summary>
        public INode Subject { get; private set; }

        /// <summary>
        /// Gets the Predicate of the Triple
        /// </summary>
        public INode Predicate { get; private set; }

        /// <summary>
        /// Gets the Object of the Triple
        /// </summary>
        public INode Object { get; private set; }

        /// <summary>
        /// Gets the Graph this Triple was created for
        /// </summary>
        /// <remarks>This is not necessarily the actual Graph this Triple is asserted in since this property is set from the Subject of the Triple when it is created and it is possible to create a Triple without asserting it into an actual Graph or to then assert it into a different Graph.</remarks>
        [Obsolete("Triples no longer hold a reference to a Graph, use Quad if that is required", true)]
        public IGraph Graph
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets the Uri of the Graph this Triple was created for
        /// </summary>
        /// <remarks>This is not necessarily the actual Graph Uri of the Graph this Triple is asserted in since this property is set from the Subject of the Triple when it is created and it is possible to create a Triple without asserting it into an actual Graph or to then assert it into a different Graph.</remarks>
        [Obsolete("Triples no longer hold a reference to a Graph, use Quad if that is required", true)]
        public Uri GraphUri
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Gets an enumeration of the Nodes in the Triple
        /// </summary>
        /// <remarks>
        /// Returned as subject, predicate, object
        /// </remarks>
        public IEnumerable<INode> Nodes
        {
            get { return new INode[] {this.Subject, this.Predicate, this.Object}; }
        }

        /// <summary>
        /// Gets whether the Triple is a Ground Triple
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <strong>Ground Triple</strong> is any Triple considered to state a single fixed fact.  In practise this means that the Triple does not contain any Blank Nodes/Variables
        /// </para>
        /// </remarks>
        public bool IsGround
        {
            get
            {
                return ((this.Subject.NodeType != NodeType.Blank && this.Subject.NodeType != NodeType.Variable) && (this.Predicate.NodeType != NodeType.Blank && this.Predicate.NodeType != NodeType.Variable) && (this.Object.NodeType != NodeType.Blank && this.Object.NodeType != NodeType.Variable));
                
            }
        }

        /// <summary>
        /// Checks whether the Triple involves a given Node
        /// </summary>
        /// <param name="n">The Node to test upon</param>
        /// <returns>True if the Triple contains the given Node</returns>
        public bool Involves(INode n)
        {
            return (this.Subject.Equals(n) || this.Predicate.Equals(n) || this.Object.Equals(n));
        }

        /// <summary>
        /// Checks whether the Triple involves a given Uri
        /// </summary>
        /// <param name="uri">The Uri to test upon</param>
        /// <returns>True if the Triple has a UriNode with the given Uri</returns>
        public bool Involves(Uri uri)
        {
            INode temp = new UriNode(uri);
            return this.Involves(temp);
        }

        /// <summary>
        /// Converts the Triple into a Quad
        /// </summary>
        /// <param name="graph">Graph name</param>
        /// <returns></returns>
        public Quad AsQuad(INode graph)
        {
            return new Quad(this, graph);
        }

        /// <summary>
        /// Determines whether this triple is equal to some other object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>True if this triple is equal to the other object, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return obj is Triple && this.Equals((Triple) obj);
        }

        /// <summary>
        /// Determines whether this triple is equal to some other object
        /// </summary>
        /// <param name="other">Object</param>
        /// <returns>True if this triple is equal to the other triple, false otherwise</returns>
        public bool Equals(Triple other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;

            //Subject, Predicate and Object must all be equal
            return this.Subject.Equals(other.Subject) && this.Predicate.Equals(other.Predicate) && this.Object.Equals(other.Object);
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
        /// Since Hash Codes are based on a String representation there is no guarantee of uniqueness though the same Triple will always give the same Hash Code (on a given Platform - see the MSDN Documentation for <see cref="string.GetHashCode">string.GetHashCode()</see> for further details) so you should use an appropriate collection to hold them
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            return this._hashcode;
        }

        /// <summary>
        /// Gets a String representation of a Triple in the form 'Subject , Predicate , Object'
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder outString = new StringBuilder();
            outString.Append(this.Subject.ToString());
            outString.Append(" , ");
            outString.Append(this.Predicate.ToString());
            outString.Append(" , ");
            outString.Append(this.Object.ToString());

            return outString.ToString();
        }

        /// <summary>
        /// Gets a String representation of a Triple in the form 'Subject , Predicate , Object' with optional compression of URIs to QNames
        /// </summary>
        /// <param name="compress">Controls whether URIs will be compressed to QNames in the String representation</param>
        /// <returns></returns>
        [Obsolete("Obsolete, no longer supportable since Triple does not hold a reference to a Graph", true)]
        public string ToString(bool compress)
        {
            throw new NotSupportedException();
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
                //Everything is greater than a null
                //Return a 1 to indicate this
                return 1;
            }
            //Compare Subjects
            int s = this.Subject.CompareTo(other.Subject);
            if (s == 0)
            {
                //Compare Predicates
                int p = this.Predicate.CompareTo(other.Predicate);
                return p == 0 ? this.Object.CompareTo(other.Object) : p;
            }
            return s;
        }

#if !SILVERLIGHT

        #region ISerializable Members

        /// <summary>
        /// Gets the data for serialization
        /// </summary>
        /// <param name="info">Serilization Information</param>
        /// <param name="context">Streaming Context</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("s", this.Subject);
            info.AddValue("p", this.Predicate);
            info.AddValue("o", this.Object);
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
            this.Subject = reader.DeserializeNode();
            this.Predicate = reader.DeserializeNode();
            this.Object = reader.DeserializeNode();

            //Compute Hash Code
            this._hashcode = Tools.CreateHashCode(this);
        }

        /// <summary>
        /// Writes the data for XML serialization
        /// </summary>
        /// <param name="writer">XML Writer</param>
        public void WriteXml(XmlWriter writer)
        {
            this.Subject.SerializeNode(writer);
            this.Predicate.SerializeNode(writer);
            this.Object.SerializeNode(writer);
        }

        #endregion

#endif
    }
}