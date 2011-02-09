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
using System.Text;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing RDF Triples in memory
    /// </summary>
    public class Triple : IComparable<Triple>
    {
        private INode _subject, _predicate, _object;
        private ITripleContext _context = null;
        private IGraph _g = null;
        private int _hashcode;
        private bool _collides = false;

        /// <summary>
        /// Constructs a Triple from Nodes that belong to the same Graph
        /// </summary>
        /// <param name="subj">Subject of the Triple</param>
        /// <param name="pred">Predicate of the Triple</param>
        /// <param name="obj">Object of the Triple</param>
        /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph</remarks>
        public Triple(INode subj, INode pred, INode obj)
        {
            //Require that all Nodes belong to the same Graph
            if (!ReferenceEquals(subj.Graph, pred.Graph) || !ReferenceEquals(pred.Graph, obj.Graph))
            {
                throw new RdfException("Subject, Predicate and Object Nodes must all come from the same Graph - use Tools.CopyNode() to transfer nodes between Graphs");
            }
            else
            {
                //Set the Graph property from the Subject
                this._g = subj.Graph;

                //Store the Three Nodes of the Triple
                this._subject = subj;
                this._predicate = pred;
                this._object = obj;

                //Compute Hash Code
                this._hashcode = (this._subject.GetHashCode().ToString() + this._predicate.GetHashCode().ToString() + this._object.GetHashCode().ToString()).GetHashCode();
            }
        }

        /// <summary>
        /// Constructs a Triple from Nodes that belong to the same Graph and associates this Triple with the given Graph (doesn't assert the Triple)
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <param name="g">Graph</param>
        public Triple(INode subj, INode pred, INode obj, IGraph g)
            : this(subj, pred, obj)
        {
            this._g = g;
        }

        /// <summary>
        /// Constructs a Triple from Nodes that belong to the same Graph with some Context
        /// </summary>
        /// <param name="subj">Subject of the Triple</param>
        /// <param name="pred">Predicate of the Triple</param>
        /// <param name="obj">Object of the Triple</param>
        /// <param name="con">Context Information for the Triple</param>
        /// <remarks>Will throw an RdfException if the Nodes don't belong to the same Graph</remarks>
        public Triple(INode subj, INode pred, INode obj, ITripleContext con)
            : this(subj, pred, obj)
        {
            this._context = con;
        }

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
                return this._g;
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
                if (this._g == null)
                {
                    return null;
                }
                else
                {
                    return this._g.BaseUri;
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
                return this._context;
            }
            set
            {
                this._context = value;
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
                return new List<INode> { this._subject, this._predicate, this._object };
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
                return (this._subject.NodeType != NodeType.Blank && this._predicate.NodeType != NodeType.Blank && this._object.NodeType != NodeType.Blank);
            }
        }

        /// <summary>
        /// Checks whether the Triple involves a given Node
        /// </summary>
        /// <param name="n">The Node to test upon</param>
        /// <returns>True if the Triple contains the given Node</returns>
        public bool Involves(INode n)
        {
            return (this._subject.Equals(n) || this._predicate.Equals(n) || this._object.Equals(n));
        }

        /// <summary>
        /// Checks whether the Triple involves a given Uri
        /// </summary>
        /// <param name="uri">The Uri to test upon</param>
        /// <returns>True if the Triple has a UriNode with the given Uri</returns>
        public bool Involves(Uri uri)
        {
            UriNode temp = new UriNode(null, uri);

            //Does the Subject involve this Uri?
            if (this._subject.Equals(temp)) return true;
            //Does the Predicate involve this Uri?
            if (this._predicate.Equals(temp)) return true;
            //Does the Object involve this Uri?
            if (this._object.Equals(temp)) return true;
            //Not Involved!
            return false;
        }

        /// <summary>
        /// Indicates whether the Triple has the given Node as the Subject
        /// </summary>
        /// <param name="n">Node to test upon</param>
        /// <returns></returns>
        public bool HasSubject(INode n)
        {
            //return this._subject.GetHashCode().Equals(n.GetHashCode());
            return this._subject.Equals(n);
        }

        /// <summary>
        /// Indicates whether the Triple has the given Node as the Predicate
        /// </summary>
        /// <param name="n">Node to test upon</param>
        /// <returns></returns>
        public bool HasPredicate(INode n)
        {
            //return this._predicate.GetHashCode().Equals(n.GetHashCode());
            return this._predicate.Equals(n);
        }

        /// <summary>
        /// Indicates whether the Triple has the given Node as the Object
        /// </summary>
        /// <param name="n">Node to test upon</param>
        /// <returns></returns>
        public bool HasObject(INode n)
        {
            //return this._object.GetHashCode().Equals(n.GetHashCode());
            return this._object.Equals(n);
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
                //Test Hash Codes to quickly determine if they might be equal
                if (this._hashcode == obj.GetHashCode())
                {
                    Triple temp = (Triple)obj;

                    //Subject, Predicate and Object must all be equal
                    //Either the Nodes must be directly equal or they must both be Blank Nodes with identical Node IDs
                    bool subjEquals = this._subject.Equals(temp.Subject) || (this._subject.NodeType == NodeType.Blank && temp.Subject.NodeType == NodeType.Blank && this._subject.ToString().Equals(temp.Subject.ToString()));
                    bool predEquals = this._predicate.Equals(temp.Predicate) || (this._predicate.NodeType == NodeType.Blank && temp.Predicate.NodeType == NodeType.Blank && this._predicate.ToString().Equals(temp.Predicate.ToString()));
                    bool objEquals = this._object.Equals(temp.Object) || (this._object.NodeType == NodeType.Blank && temp.Object.NodeType == NodeType.Blank && this._object.ToString().Equals(temp.Object.ToString()));

                    //Use conjunction on Node equality to decide this
                    return subjEquals && predEquals && objEquals;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //Can only be equal to other Triples
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
        /// Since Hash Codes are based on a String representation there is no guarantee of uniqueness though the same Triple will always give the same Hash Code (on a given Platform - see the MSDN Documentation for <see cref="System.String.GetHashCode">System.String.GetHashCode()</see> for further details)
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
            outString.Append(this._subject.ToString());
            outString.Append(" , ");
            outString.Append(this._predicate.ToString());
            outString.Append(" , ");
            outString.Append(this._object.ToString());

            return outString.ToString();
        }

        /// <summary>
        /// Gets a String representation of a Triple in the form 'Subject , Predicate , Object' with optional compression of URIs to QNames
        /// </summary>
        /// <param name="compress">Controls whether URIs will be compressed to QNames in the String representation</param>
        /// <returns></returns>
        public string ToString(bool compress)
        {
            if (!compress || this._g == null)
            {
                return this.ToString();
            }
            else
            {
                StringBuilder outString = new StringBuilder();
                String qname;

                //Subject
                if (this._subject.NodeType == NodeType.Uri)
                {
                    if (this._g.NamespaceMap.ReduceToQName(((UriNode)this._subject).Uri.ToString(),out qname)) 
                    {
                        outString.Append(qname);
                    } 
                    else 
                    {
                        outString.Append(this._subject.ToString());
                    }
                }
                else
                {
                    outString.Append(this._subject.ToString());
                }
                outString.Append(" , ");

                //Predicate
                if (this._predicate.NodeType == NodeType.Uri)
                {
                    if (this._g.NamespaceMap.ReduceToQName(((UriNode)this._predicate).Uri.ToString(),out qname)) 
                    {
                        outString.Append(qname);
                    } 
                    else 
                    {
                        outString.Append(this._predicate.ToString());
                    }
                }
                else
                {
                    outString.Append(this._predicate.ToString());
                }
                outString.Append(" , ");

                //Object
                if (this._object.NodeType == NodeType.Uri)
                {
                    if (this._g.NamespaceMap.ReduceToQName(((UriNode)this._object).Uri.ToString(),out qname))
                    {
                        outString.Append(qname);
                    } 
                    else 
                    {
                        outString.Append(this._object.ToString());
                    }
                }
                else
                {
                    outString.Append(this._object.ToString());
                }

                return outString.ToString();
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
                //Everything is greater than a null
                //Return a 1 to indicate this
                return 1;
            }
            else
            {
                int s, p;

                //Compare Subjects
                s = this.Subject.CompareTo(other.Subject);
                if (s == 0)
                {
                    //Compare Predicates
                    p = this.Predicate.CompareTo(other.Predicate);
                    if (p == 0)
                    {
                        //Compare Objects
                        return this.Object.CompareTo(other.Object);
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

        /// <summary>
        /// Gets/Sets whether the Triple collides with other Triples in this Graph
        /// </summary>
        protected internal bool Collides
        {
            get
            {
                return this._collides;
            }
            set
            {
                this._collides = value;
            }
        }
    }
}
