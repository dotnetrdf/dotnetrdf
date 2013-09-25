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
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    public class Quad
    {
        private readonly Triple _t;
        private readonly INode _graph;
        private readonly int _hashCode;

        /// <summary>
        /// Creates a new quad
        /// </summary>
        /// <param name="t">Triple</param>
        /// <param name="g">Graph name</param>
        public Quad(Triple t, INode g)
        {
            if (t == null) throw new ArgumentNullException("t");
            this._t = t;
            this._graph = g;

            this._hashCode = this.ToString().GetHashCode();
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

        /// <summary>
        /// Gets the subject of the quad
        /// </summary>
        public INode Subject
        {
            get
            {
                return this._t.Subject;
            }
        }

        /// <summary>
        /// Gets the predicate of the quad
        /// </summary>
        public INode Predicate
        {
            get
            {
                return this._t.Predicate;
            }
        }

        /// <summary>
        /// Gets the object of the quad
        /// </summary>
        public INode Object
        {
            get
            {
                return this._t.Object;
            }
        }

        /// <summary>
        /// Gets the graph name for the quad
        /// </summary>
        public INode Graph
        {
            get
            {
                return this._graph;
            }
        }

        /// <summary>
        /// Returns whether the quad is grounded i.e. does not contain any blank/variable nodes
        /// </summary>
        public bool IsGroundQuad
        {
            get
            {
                return this._t.IsGroundTriple;
            }
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
            return this._t;
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
            if (this._graph.Equals(graph)) return this;
            return new Quad(this._t, graph);
        }

        /// <summary>
        /// Determines whether this quad is equal to some other object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>True if this quad is equal to the other object, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Quad)
            {
                Quad other = (Quad)obj;

                //Graph, subject, predicate and object must all be equal
                return (this._graph.Equals(other.Graph) && this._t.Subject.Equals(other.Subject) && this._t.Predicate.Equals(other.Predicate) && this._t.Object.Equals(other.Object));
            }
            else
            {
                //Can only be equal to another Quad
                return false;
            }
        }

        /// <summary>
        /// Gets a human readable representation of the quad, this representation is intended for debugging purposes and does not represent a round trippable serialization of a quad.  For that use the <see cref="ToString(IQuadFormatter)"/> overload
        /// </summary>
        /// <returns>String representation of the quad</returns>
        public override string ToString()
        {
            StringBuilder outString = new StringBuilder();
            outString.Append(this._t.ToString());
            if (this._graph == null)
            {
                outString.Append(" in Default Graph");
            }
            else
            {
                outString.Append(" in Graph " + this._graph.ToString());
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
