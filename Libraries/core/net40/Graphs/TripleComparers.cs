/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

namespace VDS.RDF.Graphs
{
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
        /// <summary>
        /// Creates a new Full Triple comparer
        /// </summary>
        public FullTripleComparer()
            : base() { }

        /// <summary>
        /// Creates a new Full Triple comparer that uses a specific Node comparer
        /// </summary>
        /// <param name="nodeComparer">Node comparer</param>
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
        /// <summary>
        /// Creates a new Subject comparer
        /// </summary>
        public SubjectComparer()
            : base() { }

        /// <summary>
        /// Creates a new Subject comparer using the provided Node comparer
        /// </summary>
        /// <param name="nodeComparer">Node comparer</param>
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
        /// <summary>
        /// Creates a new Predicate comparer
        /// </summary>
        public PredicateComparer()
            : base() { }

        /// <summary>
        /// Creates a new Predicate comparer using the provided Node comparer
        /// </summary>
        /// <param name="nodeComparer">Node Comparer</param>
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
        /// <summary>
        /// Creates a new Object comparer
        /// </summary>
        public ObjectComparer()
            : base() { }

        /// <summary>
        /// Creates a new Object comparer using the provided Node comparer
        /// </summary>
        /// <param name="nodeComparer">Node comparer</param>
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
        /// <summary>
        /// Creates a new Subject Predicate comparer
        /// </summary>
        public SubjectPredicateComparer()
            : base() { }

        /// <summary>
        /// Creates a new Subject Predicate comparer using the provided Node comparer
        /// </summary>
        /// <param name="nodeComparer">Node Comparer</param>
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
        /// <summary>
        /// Creates a new Subject Object comparer
        /// </summary>
        public SubjectObjectComparer()
            : base() { }

        /// <summary>
        /// Creates a new Subject Object comparer using the provided Node comparer
        /// </summary>
        /// <param name="nodeComparer">Node comparer</param>
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
        /// <summary>
        /// Creates a new Predicate Object comparer
        /// </summary>
        public PredicateObjectComparer()
            : base() { }

        /// <summary>
        /// Creates a new Predicate Object comparer using the provided Node comparer
        /// </summary>
        /// <param name="nodeComparer">Node comparer</param>
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
        /// <summary>
        /// Creates a new Object Subject comparer
        /// </summary>
        public ObjectSubjectComparer()
            : base() { }

        /// <summary>
        /// Creates a new Object Subject comparer using the provided Node comparer
        /// </summary>
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
