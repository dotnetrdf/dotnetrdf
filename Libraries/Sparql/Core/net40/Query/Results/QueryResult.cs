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

using VDS.RDF.Graphs;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Basic implementation of query results
    /// </summary>
    public class QueryResult
        : IQueryResult
    {
        public QueryResult(bool result)
        {
            this.IsBoolean = true;
            this.IsGraph = false;
            this.IsTabular = false;
            this.Boolean = result;
        }

        public QueryResult(ITabularResults results)
        {
            this.IsBoolean = false;
            this.IsGraph = false;
            this.IsTabular = true;
            this.Table = results;
        }

        public QueryResult(IGraph g)
        {
            this.IsBoolean = false;
            this.IsGraph = true;
            this.IsTabular = false;
            this.Graph = g;
        }

        public bool IsTabular { get; private set; }
        public bool IsGraph { get; private set; }
        public bool IsBoolean { get; private set; }
        public ITabularResults Table { get; private set; }
        public IGraph Graph { get; private set; }
        public bool? Boolean { get; private set; }
    }
}