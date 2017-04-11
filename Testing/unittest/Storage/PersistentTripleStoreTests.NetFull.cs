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
using System.Threading;
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Storage
{
    public partial class PersistentTripleStoreTests
    {
        #region Contains Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiContains()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestContains(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoContains()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestContains(virtuoso);
        }

        #endregion

        #region Get Graph Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiGetGraph()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestGetGraph(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoGetGraph()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestGetGraph(virtuoso);
        }

        #endregion

        #region Add Triples Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiAddTriplesFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddTriplesFlushed(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoAddTriplesFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddTriplesFlushed(virtuoso);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiAddTriplesDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddTriplesDiscarded(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoAddTriplesDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddTriplesDiscarded(virtuoso);
        }

        #endregion

        #region Remove Triples Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiRemoveTriplesFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveTriplesFlushed(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoRemoveTriplesFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveTriplesFlushed(virtuoso);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiRemoveTriplesDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveTriplesDiscarded(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoRemoveTriplesDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveTriplesDiscarded(virtuoso);
        }

        #endregion

        #region Add Graph Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiAddGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddGraphFlushed(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoAddGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddGraphFlushed(virtuoso);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiAddGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddGraphDiscarded(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoAddGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddGraphDiscarded(virtuoso);
        }

        #endregion

        #region Remove Graph Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiRemoveGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveGraphFlushed(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoRemoveGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveGraphFlushed(virtuoso);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiRemoveGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveGraphDiscarded(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoRemoveGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveGraphDiscarded(virtuoso);
        }

        #endregion

        #region Add then Remove Graph Sequencing Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddThenRemoveGraphFlushed(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoAddThenRemoveGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddThenRemoveGraphFlushed(virtuoso);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiAddThenRemoveGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestAddThenRemoveGraphDiscarded(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoAddThenRemoveGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestAddThenRemoveGraphDiscarded(virtuoso);
        }

        #endregion

        #region Remove then Add Graph Sequencing Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphFlushed()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveThenAddGraphFlushed(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoRemoveThenAddGraphFlushed()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveThenAddGraphFlushed(virtuoso);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiRemoveThenAddGraphDiscarded()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestRemoveThenAddGraphDiscarded(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoRemoveThenAddGraphDiscarded()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestRemoveThenAddGraphDiscarded(virtuoso);
        }

        #endregion

        #region SPARQL Query Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiQueryUnsynced()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            Assert.Throws<RdfQueryException>(() => this.TestQueryUnsynced(fuseki));
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoQueryUnsynced()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            Assert.Throws<RdfQueryException>(() => this.TestQueryUnsynced(virtuoso));
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiQuerySelect()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQuerySelect(fuseki, "SELECT * WHERE { ?s a ?type }");
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoQuerySelect()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQuerySelect(virtuoso, "SELECT * WHERE { ?s a ?type }");
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiQueryAsk()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQueryAsk(fuseki, "ASK WHERE { GRAPH ?g { ?s a ?type } }", true);
            this.TestQueryAsk(fuseki, "ASK WHERE { GRAPH ?g { ?s <http://example.org/noSuchThing> ?o } }", false);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoQueryAsk()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQueryAsk(virtuoso, "ASK WHERE { ?s a ?type }", true);
            this.TestQueryAsk(virtuoso, "ASK WHERE { ?s <http://example.org/noSuchThing> ?o }", false);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiQueryConstruct()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQueryConstruct(fuseki, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoQueryConstruct()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQueryConstruct(virtuoso, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiQueryDescribe()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestQueryDescribe(fuseki, "DESCRIBE ?type WHERE { GRAPH ?g { ?s a ?type } } LIMIT 5");
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoQueryDescribe()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestQueryDescribe(virtuoso, "DESCRIBE ?type WHERE { ?s a ?type } LIMIT 5");
        }

        #endregion

        #region SPARQL Update Tests

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiUpdateUnsynced()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            Assert.Throws<SparqlUpdateException>(() => this.TestUpdateUnsynced(fuseki));
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoUpdateUnsynced()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            Assert.Throws<SparqlUpdateException>(() => this.TestUpdateUnsynced(virtuoso));
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreMemUpdate()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestUpdate(manager);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreFusekiUpdate()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();
            this.TestUpdate(fuseki);
        }

        [SkippableFact]
        public void StoragePersistentTripleStoreVirtuosoUpdate()
        {
            VirtuosoManager virtuoso = VirtuosoTest.GetConnection();
            this.TestUpdate(virtuoso);
        }

        #endregion
    }
}
