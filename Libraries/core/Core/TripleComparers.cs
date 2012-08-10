/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
    /// A Node Comparer which does faster comparisons since it only does lexical comparisons for literals rather than value comparisons
    /// </summary>
    public class FastNodeComparer
        : IComparer<INode>
    {
        /// <summary>
        /// Compares two Nodes
        /// </summary>
        /// <param name="x">Node</param>
        /// <param name="y">Node</param>
        /// <returns></returns>
        public int Compare(INode x, INode y)
        {
            if (ReferenceEquals(x, y)) return 0;

            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else if (y == null)
            {
                return 1;
            }

            if (x.NodeType == NodeType.Literal && y.NodeType == NodeType.Literal)
            {
                //Use faster comparison for literals - standard comparison is valued based 
                //and so gets slower as amount of nodes to compare gets larger

                //Sort order for literals is as follows
                //plain literals < language spec'd literals < typed literals
                //Within a category ordering is lexical on modifier, then lexical on lexical value
                ILiteralNode a = (ILiteralNode)x;
                ILiteralNode b = (ILiteralNode)y;

                if (a.DataType != null)
                {
                    if (b.DataType != null)
                    {
                        //Compare datatypes
                        int c = ComparisonHelper.CompareUris(a.DataType, b.DataType);
                        if (c == 0)
                        {
                            //Same datatype so compare lexical values
                            return a.Value.CompareTo(b.Value);
                        }
                        //Different datatypes
                        return c;
                    }
                    else
                    {
                        //y is untyped literal so x is greater than y
                        return 1;
                    }
                }
                else if (!a.Language.Equals(String.Empty))
                {
                    if (!b.Language.Equals(String.Empty))
                    {
                        //Compare language specifiers
                        int c = a.Language.CompareTo(b.Language);
                        if (c == 0)
                        {
                            //Same language so compare lexical values
                            return a.Value.CompareTo(b.Value);
                        }
                        //Different language specifiers
                        return c;
                    }
                    else
                    {
                        //y in plain literal so x is greater then y
                        return 1;
                    }
                }
                else
                {
                    //Plain literals so just compare lexical value
                    return a.Value.CompareTo(b.Value);
                }
            }
            else
            {
                //Non-literal nodes use their normal IComparable implementations
                return x.CompareTo(y);
            }
        }
    }

    /// <summary>
    /// Abstract base class for Triple Comparers which provide for comparisons using different node comparers
    /// </summary>
    public abstract class BaseTripleComparer
        : IComparer<Triple>
    {
        /// <summary>
        /// Node Comparer
        /// </summary>
        protected readonly IComparer<INode> _nodeComparer;

        /// <summary>
        /// Creates a new Triple Comparer
        /// </summary>
        public BaseTripleComparer()
            : this(Comparer<INode>.Default) { }

        /// <summary>
        /// Creates a new Triple Comparer
        /// </summary>
        /// <param name="nodeComparer">Node Comparer to use</param>
        public BaseTripleComparer(IComparer<INode> nodeComparer)
        {
            if (nodeComparer == null) throw new ArgumentNullException("nodeComparer");
            this._nodeComparer = nodeComparer;
        }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public abstract int Compare(Triple x, Triple y);
    }

    /// <summary>
    /// Triple comparer which compares on subjects, then predicates and finally objects
    /// </summary>
    public class FullTripleComparer
        : BaseTripleComparer
    {
        public FullTripleComparer()
            : base() { }

        public FullTripleComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            int c = this._nodeComparer.Compare(x.Subject, y.Subject);
            if (c == 0)
            {
                c = this._nodeComparer.Compare(x.Predicate, y.Predicate);
                if (c == 0)
                {
                    c = this._nodeComparer.Compare(x.Object, y.Object);
                }
            }
            return c;
        }
    }

    /// <summary>
    /// Triple comparer which compares only on subjects
    /// </summary>
    public class SubjectComparer
        : BaseTripleComparer
    {
        public SubjectComparer()
            : base() { }

        public SubjectComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            return this._nodeComparer.Compare(x.Subject, y.Subject);
        }
    }

    /// <summary>
    /// Triple comparer which compares only on predicates
    /// </summary>
    public class PredicateComparer
        : BaseTripleComparer
    {

        public PredicateComparer()
            : base() { }

        public PredicateComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            return this._nodeComparer.Compare(x.Predicate, y.Predicate);
        }
    }

    /// <summary>
    /// Triple comparer which compares only on objects
    /// </summary>
    public class ObjectComparer 
        : BaseTripleComparer
    {
        public ObjectComparer()
            : base() { }

        public ObjectComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            return this._nodeComparer.Compare(x.Object, y.Object);
        }
    }

    /// <summary>
    /// Triple comparer which compares on subjects and then predicates
    /// </summary>
    public class SubjectPredicateComparer
        : BaseTripleComparer
    {
        public SubjectPredicateComparer()
            : base() { }

        public SubjectPredicateComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            int c = this._nodeComparer.Compare(x.Subject, y.Subject);
            if (c == 0)
            {
                c = this._nodeComparer.Compare(x.Predicate, y.Predicate);
            }
            return c;
        }
    }

    /// <summary>
    /// Triple comparer which compares on subjects and then objects
    /// </summary>
    public class SubjectObjectComparer
        : BaseTripleComparer
    {
        public SubjectObjectComparer()
            : base() { }

        public SubjectObjectComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            int c = this._nodeComparer.Compare(x.Subject, y.Subject);
            if (c == 0)
            {
                c = this._nodeComparer.Compare(x.Object, y.Object);
            }
            return c;
        }
    }

    /// <summary>
    /// Triple comparer which compares on predicates and then objects
    /// </summary>
    public class PredicateObjectComparer
        : BaseTripleComparer
    {
        public PredicateObjectComparer()
            : base() { }

        public PredicateObjectComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            int c = this._nodeComparer.Compare(x.Predicate, y.Predicate);
            if (c == 0)
            {
                c = this._nodeComparer.Compare(x.Object, y.Object);
            }
            return c;
        }
    }

    /// <summary>
    /// Triple comparer which compares on objects and then subjects
    /// </summary>
    public class ObjectSubjectComparer
        : BaseTripleComparer
    {
        public ObjectSubjectComparer()
            : base() { }

        public ObjectSubjectComparer(IComparer<INode> nodeComparer)
            : base(nodeComparer) { }

        /// <summary>
        /// Compares two Triples
        /// </summary>
        /// <param name="x">Triple</param>
        /// <param name="y">Triple</param>
        /// <returns></returns>
        public override int Compare(Triple x, Triple y)
        {
            int c = this._nodeComparer.Compare(x.Object, y.Object);
            if (c == 0)
            {
                c = this._nodeComparer.Compare(x.Subject, y.Subject);
            }
            return c;
        }
    }
}
