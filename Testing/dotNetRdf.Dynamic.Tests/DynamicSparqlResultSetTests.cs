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
    using System.Collections;
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF;
    using VDS.RDF.Query;
    using Xunit;

    public class DynamicSparqlResultSetTests
    {
        [Fact]
        public void Requires_original()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamicSparqlResultSet(null));
        }

        [Fact]
        public void Enumerates_dynamic_results()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> <urn:o> .");

            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var d = new DynamicSparqlResultSet(results);

            foreach (var x in d)
            {
                Assert.NotNull(x);
            }
        }

        [Fact]
        public void IEnumerable_Enumerates_dynamic_results()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> <urn:o> .");

            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var d = (IEnumerable)new DynamicSparqlResultSet(results);

            foreach (DynamicSparqlResult x in d)
            {
                Assert.NotNull(x);
            }
        }

        [Fact]
        public void Provides_enumerable_meta_object()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> <urn:o> .");

            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var d = new DynamicSparqlResultSet(results);

            var p = (IDynamicMetaObjectProvider)d;
            var mo = p.GetMetaObject(Expression.Empty());

            Assert.NotNull(mo);
            Assert.IsType<EnumerableMetaObject>(mo);
        }
    }
}
