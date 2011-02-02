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

namespace VDS.RDF
{
    /// <summary>
    /// Class for representing URI Nodes
    /// </summary>
    public class UriNode : BaseNode, IComparable<UriNode>
    {
        private Uri _uri;
        protected String _stringUri;

        /// <summary>
        /// Internal Only Constructor for Uri Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="uri">Uri for the Node</param>
        protected internal UriNode(IGraph g, Uri uri)
            : base(g, NodeType.Uri)
        {
            this._uri = uri;
            this._stringUri = this._uri.ToString();

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Internal Only Constructor for Uri Nodes
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="qname">QName for the Node</param>
        /// <remarks>
        /// This Constructor tries to resolve the QName using the NamespaceMapper and Base Uri of the Graph it is in.  Exceptions may occur if we cannot resolve the QName correctly.
        /// </remarks>
        protected internal UriNode(IGraph g, String qname)
            : base(g, NodeType.Uri)
        {
            if (qname.Contains(':'))
            {
                if (this._graph != null)
                {
                    this._uri = new Uri(Tools.ResolveQName(qname, this._graph.NamespaceMap, this._graph.BaseUri));
                }
                else
                {
                    throw new RdfException("Cannot create a URI Node using a QName in a Null Graph");
                }
            }
            else
            {
                throw new RdfException("Cannot create a URI Node since the QName '" + qname + "' appears to be invalid");
            }

            this._stringUri = this._uri.ToString();

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Protected Constructor for derived classes which want to control the URI more closely
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="uri">A form of the URI (derived classes may override Uri property as desired)</param>
        /// <param name="stringUri">String form of the URI (may be unormalized/relative etc)</param>
        protected UriNode(IGraph g, Uri uri, String stringUri)
            : base(g, NodeType.Uri)
        {
            this._uri = uri;
            this._stringUri = stringUri;

            //Compute Hash Code
            this._hashcode = (this._nodetype + this.ToString()).GetHashCode();
        }

        /// <summary>
        /// Gets the Uri for this Node
        /// </summary>
        public virtual Uri Uri
        {
            get
            {
                return this._uri;
            }
        }

        /// <summary>
        /// Gets the Uri for this Node as a String
        /// </summary>
        /// <remarks>Computed at instantiation for efficiency, used in Equals implementation</remarks>
        protected internal String StringUri
        {
            get
            {
                return this._stringUri;
            }
        }

        /// <summary>
        /// Implementation of Equality for Uri Nodes
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns></returns>
        /// <remarks>
        /// URI Nodes are considered equal if the string form of their URIs match using Ordinal string comparison
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            if (obj is INode)
            {
                return this.Equals((INode)obj);
            }
            else
            {
                //Can only be equal to other Nodes
                return false;
            }
        }

        /// <summary>
        /// Implementation of Equality for Uri Nodes
        /// </summary>
        /// <param name="other">Object to compare with</param>
        /// <returns></returns>
        /// <remarks>
        /// URI Nodes are considered equal if the string form of their URIs match using Ordinal string comparison
        /// </remarks>
        public override bool Equals(INode other)
        {
            if ((Object)other == null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.NodeType == NodeType.Uri)
            {
                UriNode temp = (UriNode)other;

                //Switched to using straight ordinal string comparison
                return this.StringUri.Equals(temp.StringUri, StringComparison.Ordinal);
            }
            else
            {
                //Can only be equal to UriNodes
                return false;
            }
        }

        /// <summary>
        /// Gets a String representation of a Uri as a plain text Uri
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._stringUri;
        }

        /// <summary>
        /// Implementation of Compare To for Uri Nodes
        /// </summary>
        /// <param name="other">Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Uri Nodes are greater than Blank Nodes and Nulls, they are less than Literal Nodes and Graph Literal Nodes.
        /// <br /><br />
        /// Uri Nodes are ordered based upon lexical ordering of the string value of their URIs
        /// </remarks>
        public override int CompareTo(INode other)
        {
            if (other == null)
            {
                //Everything is greater than a null
                //Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.Blank || other.NodeType == NodeType.Variable)
            {
                //Uri Nodes are greater than Blank and Variable Nodes
                //Return a 1 to indicate this
                return 1;
            }
            else if (other.NodeType == NodeType.Uri)
            {
                if (ReferenceEquals(this, other)) return 0;

                //Uri Nodes are ordered lexically
                //Return the result of CompareTo on the string values of the URIs
                UriNode u = (UriNode)other;
                return this._stringUri.CompareTo(u.StringUri);
            }
            else
            {
                //Anything else is considered greater than a Uri Node
                //Return -1 to indicate this
                return -1;
            }
        }

        /// <summary>
        /// Implementation of Compare To for URI Nodes
        /// </summary>
        /// <param name="other">URI Node to Compare To</param>
        /// <returns></returns>
        /// <remarks>
        /// Simply invokes the more general implementation of this method
        /// </remarks>
        public int CompareTo(UriNode other)
        {
            return this.CompareTo((INode)other);
        }
    }

    ///// <summary>
    ///// Class for representing URI Nodes where the URI may be non-normalized
    ///// </summary>
    //class NonNormalizedUriNode : UriNode
    //{
    //    protected internal NonNormalizedUriNode(IGraph g, String uri)
    //        : base(g, new Uri(uri), uri)
    //    {

    //    }
    //}
}
