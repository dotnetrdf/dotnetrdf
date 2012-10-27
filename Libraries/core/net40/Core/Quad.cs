using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    public class Quad
    {
        private readonly Triple _t;
        private readonly Uri _graphUri;
        private readonly int _hashCode;

        public Quad(Triple t, Uri graphUri)
        {
            if (t == null) throw new ArgumentNullException("t");
            this._t = t;
            this._graphUri = graphUri;

            this._hashCode = this.ToString().GetHashCode();
        }

        public Quad(INode subj, INode pred, INode obj, Uri graphUri)
            : this(new Triple(subj, pred, obj), graphUri) { }

        public INode Subject
        {
            get
            {
                return this._t.Subject;
            }
        }

        public INode Predicate
        {
            get
            {
                return this._t.Predicate;
            }
        }

        public INode Object
        {
            get
            {
                return this._t.Object;
            }
        }

        public Uri Graph
        {
            get
            {
                return this._graphUri;
            }
        }

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
        /// Makes a copy of the Quad which is the Quad with a different Graph field
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// Returns this quad if the Graph URI matches that already set for this Quad, otherwise a new Quad is returned
        /// </remarks>
        public Quad CopyTo(Uri graphUri)
        {
            if (EqualityHelper.AreUrisEqual(this._graphUri, graphUri)) return this;
            return new Quad(this._t, graphUri);
        }

        public override bool Equals(object obj)
        {
            if (obj is Quad)
            {
                Quad other = (Quad)obj;

                //Graph URI and subject, predicate and object must all be equal
                return (EqualityHelper.AreUrisEqual(this._graphUri, other.Graph) && this._t.Subject.Equals(other.Subject) && this._t.Predicate.Equals(other.Predicate) && this._t.Object.Equals(other.Object));
            }
            else
            {
                //Can only be equal to another Quad
                return false;
            }
        }

        public override string ToString()
        {
            StringBuilder outString = new StringBuilder();
            outString.Append(this._t.ToString());
            if (this._graphUri == null)
            {
                outString.Append(" in Default Graph");
            }
            else
            {
                outString.Append(" in Graph " + this._graphUri.AbsoluteUri);
            }

            return outString.ToString();
        }

        public override int GetHashCode()
        {
            return this._hashCode;
        }
    }
}
