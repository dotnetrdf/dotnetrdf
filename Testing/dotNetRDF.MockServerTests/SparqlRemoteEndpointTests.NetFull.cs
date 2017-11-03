/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2017 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System.Threading;
using Xunit;
using VDS.RDF.Query;
using FluentAssertions;

namespace dotNetRDF.MockServerTests
{
    public partial class SparqlRemoteEndpointTests
    {
        const int AsyncTimeout = 45000;

        
        [Fact]
        public void SparqlRemoteEndpointAsyncApiQueryWithResultSet()
        {
            RegisterSelectQueryGetHandler();
            SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
            ManualResetEvent signal = new ManualResetEvent(false);
            endpoint.QueryWithResultSet("SELECT * WHERE {?s ?p ?o}", (r, s) =>
            {
                signal.Set();
                signal.Close();
            }, null);

            signal.WaitOne(1000);
            signal.SafeWaitHandle.IsClosed.Should().BeTrue();
        }

        
        [Fact]
        public void SparqlRemoteEndpointAsyncApiQueryWithResultGraph()
        {
            RegisterConstructQueryGetHandler();
            SparqlRemoteEndpoint endpoint = GetQueryEndpoint();
            ManualResetEvent signal = new ManualResetEvent(false);
            endpoint.QueryWithResultGraph("CONSTRUCT WHERE { ?s ?p ?o }", (r, s) =>
            {
                signal.Set();
                signal.Close();
            }, null);

            signal.WaitOne(1000);
            signal.SafeWaitHandle.IsClosed.Should().BeTrue();
        }

        
    }
}
