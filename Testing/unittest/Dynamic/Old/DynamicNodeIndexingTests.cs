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

namespace VDS.RDF.Dynamic.Old
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CSharp.RuntimeBinder;
    using Xunit;

    public class DynamicNodeIndexingTests
    {
        public void Indexing_requires_base_uri_for_relative_uri()
        {
            var dynamic_s = new Graph().CreateBlankNode().AsDynamic();

            Assert.Throws<InvalidOperationException>(() =>
                dynamic_s["p"]
            );
        }

        public void Indexing_requires_known_index_type()
        {
            var dynamic_s = new Graph().CreateBlankNode().AsDynamic();

            Assert.Throws<RuntimeBinderException>(() =>
                dynamic_s[null]
            );
        }

        public void Indexing_requires_valid_uri()
        {
            var dynamic_s = new Graph().CreateBlankNode().AsDynamic();

            Assert.Throws<FormatException>(() =>
                dynamic_s["http:///"]
            );
        }

        public void Indexing_supports_absolute_uri_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_p = UriFactory.Create("http://example.com/p");
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[example_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_absolute_uri_string_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["http://example.com/p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_dynamic_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var dynamic_p = g.Triples.Single().Predicate.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[dynamic_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_empty_string_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var baseUri = UriFactory.Create("http://example.com/p");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[string.Empty] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_hash_base_uri_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/#s> <http://example.com/#p> <http://example.com/#o> .");

            var baseUri = UriFactory.Create("http://example.com/#");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_node_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_p = g.Triples.Single().Predicate;
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[example_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_qname_index()
        {
            var baseUri = UriFactory.Create("http://example.com/");

            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace("ex", baseUri);

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["ex:p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_qname_index_with_empty_prefix()
        {
            var baseUri = UriFactory.Create("http://example.com/");

            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");
            g.NamespaceMap.AddNamespace(string.Empty, baseUri);

            var dynamic_s = g.Triples.Single().Subject.AsDynamic();
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[":p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_relative_uri_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var baseUri = UriFactory.Create("http://example.com/");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_p = new Uri("p", UriKind.Relative);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s[example_p] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_supports_relative_uri_string_index()
        {
            var g = new Graph();
            g.LoadFromString("<http://example.com/s> <http://example.com/p> <http://example.com/o> .");

            var baseUri = UriFactory.Create("http://example.com/");
            var dynamic_s = g.Triples.Single().Subject.AsDynamic(baseUri);
            var example_o = g.Triples.Single().Object;

            var objects = dynamic_s["p"] as IEnumerable<object>;
            var result = objects.Single();

            Assert.Equal(example_o, result);
        }

        public void Indexing_setter_requires_index()
        {
            var dynamic_s = new Graph().CreateBlankNode().AsDynamic();

            Assert.Throws<ArgumentNullException>(() =>
                dynamic_s[null as string] = null
            );
        }
    }
}
