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
using NUnit.Framework;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract set of Triple Stores tests which can be used to test any <see cref="ITripleStore"/> implementation
    /// </summary>
    [TestFixture]
    public abstract class AbstractTripleStoreTests
    {
        /// <summary>
        /// Method which derived tests should implement to provide a fresh instance that can be used for testing
        /// </summary>
        /// <returns></returns>
        protected abstract ITripleStore GetInstance();

        [Test]
        public void TripleStoreIsEmpty01()
        {
            ITripleStore store = this.GetInstance();

            Assert.IsTrue(store.IsEmpty);
        }

        [Test]
        public void TripleStoreIsEmpty02()
        {
            ITripleStore store = this.GetInstance();
            store.Add(new Graph());

            Assert.IsFalse(store.IsEmpty);
        }

        [Test]
        public void TripleStoreAdd01()
        {
            ITripleStore store = this.GetInstance();

            Graph g = new Graph();
            store.Add(g);

            Assert.IsFalse(store.IsEmpty);
            Assert.IsTrue(store.HasGraph(g.BaseUri));
        }

        [Test]
        public void TripleStoreAdd02()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);

            Assert.IsFalse(store.IsEmpty);
            Assert.IsTrue(store.HasGraph(g.BaseUri));
        }

        [Test]
        public void TripleStoreHasGraph01()
        {
            ITripleStore store = this.GetInstance();

            Assert.IsFalse(store.HasGraph(new Uri("http://thereisnosuchdomain.com/graph")));
        }

        [Test]
        public void TripleStoreHasGraph02()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            store.Add(g);

            Assert.IsTrue(store.HasGraph(null));
        }

        [Test]
        public void TripleStoreHasGraph03()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            g.BaseUri = new Uri("http://nosuchdomain.com/graph");
            store.Add(g);

            Assert.IsTrue(store.HasGraph(g.BaseUri));
        }
    }

    [TestFixture]
    public class TripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new TripleStore();
        }
    }

    [TestFixture]
    public class ThreadSafeTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new TripleStore(new ThreadSafeGraphCollection());
        }
    }

#if !SILVERLIGHT
    [TestFixture]
    public class WebDemandTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new WebDemandTripleStore();
        }
    }
#endif

#if !NO_FILE
    [TestFixture]
    public class DiskDemandTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new DiskDemandTripleStore();
        }
    }
#endif
}
