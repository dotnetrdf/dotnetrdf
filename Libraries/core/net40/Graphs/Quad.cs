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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Graphs
{
    /// <summary>
    /// Represents a RDF quad which is a RDF triple with an additional graph name field
    /// </summary>
    public sealed class Quad
        : IEquatable<Quad>
    {
        /// <summary>
        /// Special node instance which represents the default graph
        /// </summary>
        public static readonly INode DefaultGraphNode = new UriNode(new Uri("dotnetrdf:default-graph"));

        private readonly int _hashCode;

        /// <summary>
        /// Creates a new quad
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="g">Graph name</param>
        public Quad(Triple t, INode g)
        {
            if (t == null) throw new ArgumentNullException("t");
            this.Triple = t;
            this.Graph = ReferenceEquals(g, null) ? DefaultGraphNode : g;

            this._hashCode = Tools.CombineHashCodes(this.Triple, this.Graph);
        }

        /// <summary>
        /// Creates a new quad
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <param name="graph">graph name</param>
        public Quad(INode subj, INode pred, INode obj, INode graph)
            : this(new Triple(subj, pred, obj), graph) { }

        private Triple Triple { get; set; }

        /// <summary>
        /// Gets the subject of the quad
        /// </summary>
        public INode Subject
        {
            get
            {
                return this.Triple.Subject;
            }
        }

        /// <summary>
        /// Gets the predicate of the quad
        /// </summary>
        public INode Predicate
        {
            get
            {
                return this.Triple.Predicate;
            }
        }

        /// <summary>
        /// Gets the object of the quad
        /// </summary>
        public INode Object
        {
            get
            {
                return this.Triple.Object;
            }
        }

        /// <summary>
        /// Gets the graph name for the quad
        /// </summary>
        public INode Graph { get; private set; }

        /// <summary>
        /// Returns whether the quad is grounded i.e. does not contain any blank/variable nodes
        /// </summary>
        public bool IsGroundQuad
        {
            get
            {
                return this.Triple.IsGroundTriple && (this.Graph.NodeType != NodeType.Blank && this.Graph.NodeType != NodeType.Variable);
            }
        }

        /// <summary>
        /// Gets whether this quad belongs to the unnamed default graph
        /// </summary>
        /// <remarks>
        /// This is indicated by the Graph property having the value <see cref="Quad.DefaultGraphNode"/>
        /// </remarks>
        public bool InDefaultGraph
        {
            get { return DefaultGraphNode.Equals(this.Graph); }
        }

        /// <summary>
        /// Converts a Quad into a Triple
        /// </summary>
        /// <returns>Triple form of the Quad</returns>
        /// <remarks>
        /// <strong>Warning:</strong> This is a lossy information, a Triple does not store any reference to a Graph
        /// </remarks>
        public Triple AsTriple()
        {
            return this.Triple;
        }

        /// <summary>
        /// Makes a copy of the Quad which is the Quad with a different Graph name
        /// </summary>
        /// <param name="graph">Graph name</param>
        /// <returns></returns>
        /// <remarks>
        /// Returns this quad if the Graph name matches that already set for this Quad, otherwise a new Quad is returned
        /// </remarks>
        public Quad CopyTo(INode graph)
        {
            return this.Graph.Equals(graph) ? this : new Quad(this.Triple, graph);
        }

        /// <summary>
        /// Determines whether this quad is equal to some other object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>True if this quad is equal to the other object, false otherwise</returns>
        public override bool Equals(object obj)
        {
            return obj is Quad && this.Equals((Quad) obj);
        }

        /// <summary>
        /// Determines whether this quad is equal to another quad
        /// </summary>
        /// <param name="other">Other quad</param>
        /// <returns>True if this quad is equal to the other, false otherwise</returns>
        public bool Equals(Quad other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;

            //Graph, subject, predicate and object must all be equal
            return (this.Graph.Equals(other.Graph) && this.Triple.Subject.Equals(other.Subject) && this.Triple.Predicate.Equals(other.Predicate) && this.Triple.Object.Equals(other.Object));
        }

        /// <summary>
        /// Gets a human readable representation of the quad, this representation is intended for debugging purposes and does not represent a round trippable serialization of a quad.  For that use the <see cref="ToString(IQuadFormatter)"/> overload
        /// </summary>
        /// <returns>String representation of the quad</returns>
        public override string ToString()
        {
            StringBuilder outString = new StringBuilder();
            outString.Append(this.Triple.ToString());
            if (this.Graph == null)
            {
                outString.Append(" in Default Graph");
            }
            else
            {
                outString.Append(" in Graph " + this.Graph.ToString());
            }

            return outString.ToString();
        }

        /// <summary>
        /// Gets the string representation of the quad as formatted by the given formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <returns>String representation of the quad</returns>
        public string ToString(IQuadFormatter formatter)
        {
            return formatter.Format(this);
        }

        /// <summary>
        /// Gets the hash code of the quad
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return this._hashCode;
        }
    }
}
