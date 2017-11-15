/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
#if !NETCOREAPP2_0 // Parallel query evaluation is currently unsupported in .NET Core
using System;
using System.Threading;
using Xunit;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Parsing;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{
    public partial class QueryThreadSafety
    {
        [Fact]
        public void SparqlQueryThreadSafeEvaluation()
        {
            TestTools.TestInMTAThread(this.SparqlQueryThreadSafeEvaluationActual);
        }

        [Fact]
        public void SparqlQueryAndUpdateThreadSafeEvaluation()
        {
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine("Run #" + i);
                TestTools.TestInMTAThread(this.SparqlQueryAndUpdateThreadSafeEvaluationActual);
                Console.WriteLine();
            }
        }
    }
}
#endif