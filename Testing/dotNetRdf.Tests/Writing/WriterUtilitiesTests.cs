using System.Collections.Generic;
using VDS.RDF.Writing.Contexts;
using Xunit;

namespace VDS.RDF.Writing;

public class WriterUtilitiesTests
{
    
        [Fact]
        public void AListThatStartsWithAnEmptyNodeIsNotAnImplicitCollection()
        {
            var g = new Graph();
            INode b1 = g.CreateBlankNode("b1");
            INode b2 = g.CreateBlankNode("b2");
            INode b3 = g.CreateBlankNode("b3");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(b1, rdfRest, b2);
            g.Assert(b2, rdfFirst, b3);
            g.Assert(b2, rdfRest, rdfNil);
            
            Assert.Empty(FindCollections(g, CollectionSearchMode.ImplicitOnly));
        }
        
        [Fact]
        public void AListThatContainsAnEmptyNodeIsNotAnImplicitCollection()
        {
            var g = new Graph();
            INode b1 = g.CreateBlankNode("b1");
            INode b2 = g.CreateBlankNode("b2");
            INode b3 = g.CreateBlankNode("b3");
            INode b4 = g.CreateBlankNode("b4");
            INode b5 = g.CreateBlankNode("b5");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(b1, rdfFirst, b2);
            g.Assert(b1, rdfRest, b3);
            g.Assert(b3, rdfRest, b4);
            g.Assert(b4, rdfFirst, b5);
            g.Assert(b4, rdfRest, rdfNil);
            
            Assert.Empty(FindCollections(g, CollectionSearchMode.ImplicitOnly));
        }

        [Fact]
        public void AListThatEndsWithAnEmptyNodeIsNotAnImplicitCollection()
        {
            var g = new Graph();
            INode b1 = g.CreateBlankNode("b1");
            INode b2 = g.CreateBlankNode("b2");
            INode b3 = g.CreateBlankNode("b3");
            INode b4 = g.CreateBlankNode("b4");
            INode b5 = g.CreateBlankNode("b5");
            INode rdfFirst = g.CreateUriNode("rdf:first");
            INode rdfRest = g.CreateUriNode("rdf:rest");
            INode rdfNil = g.CreateUriNode("rdf:nil");

            g.Assert(b1, rdfFirst, b2);
            g.Assert(b1, rdfRest, b3);
            g.Assert(b3, rdfFirst, b4);
            g.Assert(b3, rdfRest, b5);
            g.Assert(b5, rdfRest, rdfNil);
            
            Assert.Empty(FindCollections(g, CollectionSearchMode.ImplicitOnly));
        }

        private Dictionary<INode, OutputRdfCollection> FindCollections(IGraph g, CollectionSearchMode mode = CollectionSearchMode.All)
        {
            var sw = new System.IO.StringWriter();
            var context = new CompressingTurtleWriterContext(g, sw);
            WriterHelper.FindCollections(context, mode);
            return context.Collections;
        }
}