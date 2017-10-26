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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class FormattingTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        [Fact]
        public void SparqlFormattingOptionalAtRoot()
        {
            SparqlQuery q = new SparqlQuery { QueryType = SparqlQueryType.Select };
            q.AddVariable(new SparqlVariable("s", true));

            GraphPattern gp = new GraphPattern();
            gp.IsOptional = true;
            gp.AddTriplePattern(new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o")));
            q.RootGraphPattern = gp;

            String toStr = q.ToString();
            Console.WriteLine("ToString() Form:");
            Console.WriteLine(toStr);
            Assert.Equal(2, toStr.ToCharArray().Where(c => c == '{').Count());
            Assert.Equal(2, toStr.ToCharArray().Where(c => c == '}').Count());
            Console.WriteLine();

            SparqlFormatter formatter = new SparqlFormatter();
            String fmtStr = formatter.Format(q);
            Console.WriteLine("SparqlFormatter Form:");
            Console.WriteLine(fmtStr);
            Assert.Equal(2, fmtStr.ToCharArray().Where(c => c == '{').Count());
            Assert.Equal(2, fmtStr.ToCharArray().Where(c => c == '}').Count());
        }

    }
}
