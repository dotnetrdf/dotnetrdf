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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// Summary description for WriteToStoreHandlerTests
    /// </summary>
    public partial class WriteToStoreHandlerTests
    {
#if !NO_VIRTUOSO
        [SkippableFact]
        public void ParsingWriteToStoreHandlerVirtuoso()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestWriteToStoreHandler(virtuoso);
        }

        [SkippableFact]
        public void ParsingWriteToStoreHandlerDatasetsVirtuoso()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestWriteToStoreDatasetsHandler(virtuoso);
        }

        [SkippableFact]
        public void ParsingWriteToStoreHandlerBNodesAcrossBatchesVirtuoso()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestWriteToStoreHandlerWithBNodes(virtuoso);
        }
#endif
        [SkippableFact]
        public void ParsingWriteToStoreHandlerAllegroGraph()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            this.TestWriteToStoreHandler(agraph);
        }

        [SkippableFact]
        public void ParsingWriteToStoreHandlerFuseki()
        {
            try
            {
                Options.UriLoaderCaching = false;
                FusekiConnector fuseki = FusekiTest.GetConnection();
                this.TestWriteToStoreHandler(fuseki);
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [SkippableFact]
        public void ParsingWriteToStoreHandlerBNodesAcrossBatchesAllegroGraph()
        {
            AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
            this.TestWriteToStoreHandlerWithBNodes(agraph);
        }

        [SkippableFact]
        public void ParsingWriteToStoreHandlerBNodesAcrossBatchesFuseki()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestWriteToStoreHandlerWithBNodes(fuseki);
        }

    }
}
