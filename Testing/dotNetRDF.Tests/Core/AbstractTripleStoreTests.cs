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
using Xunit;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract set of Triple Stores tests which can be used to test any <see cref="ITripleStore"/> implementation
    /// </summary>

    public abstract class AbstractTripleStoreTests
    {
        /// <summary>
        /// Method which derived tests should implement to provide a fresh instance that can be used for testing
        /// </summary>
        /// <returns></returns>
        protected abstract ITripleStore GetInstance();

        [Fact]
        public void TripleStoreIsEmpty01()
        {
            ITripleStore store = this.GetInstance();

            Assert.True(store.IsEmpty);
        }

        [Fact]
        public void TripleStoreIsEmpty02()
        {
            ITripleStore store = this.GetInstance();
            store.Add(new Graph());

            Assert.False(store.IsEmpty);
        }

        [Fact]
        public void TripleStoreAdd01()
        {
            ITripleStore store = this.GetInstance();

            Graph g = new Graph();
            store.Add(g);

            Assert.False(store.IsEmpty);
            Assert.True(store.HasGraph(g.BaseUri));
        }

        [Fact]
        public void TripleStoreAdd02()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);

            Assert.False(store.IsEmpty);
            Assert.True(store.HasGraph(g.BaseUri));
        }

        [Fact]
        public void TripleStoreHasGraph01()
        {
            ITripleStore store = this.GetInstance();

            Assert.False(store.HasGraph(new Uri("http://thereisnosuchdomain.com:1234/graph")));
        }

        [Fact]
        public void TripleStoreHasGraph02()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            store.Add(g);

            Assert.True(store.HasGraph(null));
        }

        [Fact]
        public void TripleStoreHasGraph03()
        {
            ITripleStore store = this.GetInstance();

            IGraph g = new Graph();
            g.BaseUri = new Uri("http://nosuchdomain.com/graph");
            store.Add(g);

            Assert.True(store.HasGraph(g.BaseUri));
        }
    }


    public class TripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new TripleStore();
        }
    }


    public class ThreadSafeTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new TripleStore(new ThreadSafeGraphCollection());
        }
    }

    public class WebDemandTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new WebDemandTripleStore();
        }
    }

    public class DiskDemandTripleStoreTests
        : AbstractTripleStoreTests
    {
        protected override ITripleStore GetInstance()
        {
            return new DiskDemandTripleStore();
        }
    }
}
