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

namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using VDS.RDF.Query;
    using Xunit;

    public class DynamicSparqlResultTests
    {
        [Fact]
        public void Constructor_requires_original()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamicSparqlResult(null));
        }

        [Fact]
        public void Get_index_requires_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Throws<ArgumentNullException>(() =>
                d[null]);
        }

        [Fact]
        public void Get_index_returns_native_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Equal(UriFactory.Create("urn:s"), d["s"]);
        }

        [Fact]
        public void Set_index_requires_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Throws<ArgumentNullException>(() =>
                d[null] = null);
        }

        [Fact]
        public void Set_index_binds_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);
            var s2 = UriFactory.Create("urn:s2");
            var expected = new NodeFactory().CreateUriNode(s2);

            d["s"] = s2;

            Assert.Equal(expected, result["s"]);
        }

        [Fact]
        public void Keys_are_variables()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Equal(results.Variables, d.Keys);
        }

        [Fact]
        public void Values_are_native_bindings()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");
            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Equal(result.Select(pair => ((IUriNode)pair.Value).Uri), d.Values.Cast<Uri>());
        }

        [Fact]
        public void Counts_bound_variables()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Equal(result.Count, d.Count);
        }

        [Fact]
        public void Is_writable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.False(d.IsReadOnly);
        }

        [Fact]
        public void Add_requires_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Throws<ArgumentNullException>(() =>
                d.Add(null, null));
        }

        [Fact]
        public void Add_binds_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");
            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            d.Add("x", "y");

            Assert.Equal("y", ((ILiteralNode)result["x"]).Value);
        }

        [Fact]
        public void Add_handles_pairs()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = (ICollection<KeyValuePair<string, object>>)new DynamicSparqlResult(result);

            d.Add(new KeyValuePair<string, object>("x", "y"));

            Assert.Equal("y", ((ILiteralNode)result["x"]).Value);
        }

        [Fact]
        public void Clear_unbinds_all_variables()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            d.Clear();

            Assert.Empty(result);
        }

        [Fact]
        public void Contains_rejects_missing_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.DoesNotContain(new KeyValuePair<string, object>("x", null), d);
        }

        [Fact]
        public void Contains_matches_native_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.Contains(new KeyValuePair<string, object>("s", UriFactory.Create("urn:s")), d);
        }

        [Fact]
        public void ContainsKey_rejects_null_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.False(d.ContainsKey(null));
        }

        [Fact]
        public void ContainsKey_matches_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.True(d.ContainsKey("s"));
        }

        [Fact]
        public void Copies_pairs_with_variable_key_and_native_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = (ICollection<KeyValuePair<string, object>>)new DynamicSparqlResult(result);

            var objects = new KeyValuePair<string, object>[5]; // +2 for padding on each side
            d.CopyTo(objects, 1); // start at the second item at destination

            Assert.Equal(
                new[]
                {
                    default,
                    new KeyValuePair<string, object>("s", UriFactory.Create("urn:s")),
                    new KeyValuePair<string, object>("p", UriFactory.Create("urn:p")),
                    new KeyValuePair<string, object>("o", UriFactory.Create("urn:o")),
                    default
                },
                objects);
        }

        [Fact]
        public void Enumerates_pairs_with_variable_key_and_native_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = (ICollection<KeyValuePair<string, object>>)new DynamicSparqlResult(result);

            using (var actual = d.GetEnumerator())
            {
                using (var expected = result.GetEnumerator())
                {
                    while (expected.MoveNext() | actual.MoveNext())
                    {
                        Assert.Equal(
                            new KeyValuePair<string, object>(
                                expected.Current.Key,
                                ((IUriNode)expected.Current.Value).Uri),
                            actual.Current);
                    }
                }
            }
        }

        [Fact]
        public void IEnumerable_enumerates_pairs_with_variable_key_and_native_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            var actual = d.GetEnumerator();
            using (var expected = result.GetEnumerator())
            {
                while (expected.MoveNext() | actual.MoveNext())
                {
                    Assert.Equal(
                        new KeyValuePair<string, object>(
                            expected.Current.Key,
                            ((IUriNode)expected.Current.Value).Uri),
                        actual.Current);
                }
            }
        }

        [Fact]
        public void Remove_rejects_null_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.False(d.Remove(null));
        }

        [Fact]
        public void Remove_rejects_missing_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.False(d.Remove("x"));
        }

        [Fact]
        public void Remove_unbinds_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.True(d.Remove("s"));
            Assert.False(result.HasValue("s"));
        }

        [Fact]
        public void Remove_pair_rejects_missing_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = (ICollection<KeyValuePair<string, object>>)new DynamicSparqlResult(result);

            Assert.False(d.Remove(new KeyValuePair<string, object>("x", null)));
        }

        [Fact]
        public void Remove_pair_rejects_missing_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = (ICollection<KeyValuePair<string, object>>)new DynamicSparqlResult(result);

            Assert.False(d.Remove(new KeyValuePair<string, object>("s", UriFactory.Create("urn:x"))));
            Assert.True(result.HasValue("s"));
        }

        [Fact]
        public void Remove_pair_unbinds_variable_with_existing_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = (ICollection<KeyValuePair<string, object>>)new DynamicSparqlResult(result);

            Assert.True(d.Remove(new KeyValuePair<string, object>("s", UriFactory.Create("urn:s"))));
            Assert.False(result.HasValue("s"));
        }

        [Fact]
        public void TryGetVaue_rejects_null_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.False(d.TryGetValue(null, out var value));
            Assert.Null(value);
        }

        [Fact]
        public void TryGetVaue_rejects_missing_variable()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.False(d.TryGetValue("x", out var value));
            Assert.Null(value);
        }

        [Fact]
        public void TryGetVaue_outputs_native_binding()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);

            Assert.True(d.TryGetValue("s", out var value));
            Assert.Equal(UriFactory.Create("urn:s"), value);
        }

        [Fact]
        public void Provides_dictionary_meta_object()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var results = (SparqlResultSet)g.ExecuteQuery(@"
SELECT *
WHERE {
    ?s ?p ?o .
}
");

            var result = results.Single();
            var d = new DynamicSparqlResult(result);
            var p = (IDynamicMetaObjectProvider)d;
            var mo = p.GetMetaObject(Expression.Empty());

            Assert.NotNull(mo);
            Assert.IsType<DictionaryMetaObject>(mo);
        }
    }
}
