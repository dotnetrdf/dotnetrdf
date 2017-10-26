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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{

    public class JoinTests
    {
        private NodeFactory _factory = new NodeFactory();

        [Fact]
        public void SparqlAlgebraJoinSingleVariable1()
        {
            ISet x = new Set();
            x.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x);
            BaseMultiset rhs = new Multiset();
            rhs.Add(x);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.Equal(1, joined.Count);
        }

        [Fact]
        public void SparqlAlgebraJoinSingleVariable2()
        {
            ISet x = new Set();
            x.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.Equal(2, joined.Count);
        }

        [Fact]
        public void SparqlAlgebraJoinMultiVariable1()
        {
            ISet x = new Set();
            x.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            x.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.Equal(2, joined.Count);
        }

        [Fact]
        public void SparqlAlgebraJoinMultiVariable2()
        {
            ISet x1 = new Set();
            x1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            x1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet x2 = new Set();
            x2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            x2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x1);
            lhs.Add(x2);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.Equal(2, joined.Count);
        }

        [Fact]
        public void SparqlAlgebraJoinMultiVariable3()
        {
            ISet x1 = new Set();
            x1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            x1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet x2 = new Set();
            x2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            x2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x1);
            lhs.Add(x2);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.Equal(2, joined.Count);
        }

        [Fact]
        public void SparqlAlgebraJoinMultiVariable4()
        {
            ISet x1 = new Set();
            x1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            x1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));
            ISet x2 = new Set();
            x2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            x2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));

            ISet y1 = new Set();
            y1.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x1")));
            y1.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y2")));
            ISet y2 = new Set();
            y2.Add("a", this._factory.CreateUriNode(UriFactory.Create("http://x2")));
            y2.Add("b", this._factory.CreateUriNode(UriFactory.Create("http://y1")));

            BaseMultiset lhs = new Multiset();
            lhs.Add(x1);
            lhs.Add(x2);
            BaseMultiset rhs = new Multiset();
            rhs.Add(y1);
            rhs.Add(y2);

            BaseMultiset joined = lhs.Join(rhs);

            Assert.Equal(0, joined.Count);
        }
    }
}
