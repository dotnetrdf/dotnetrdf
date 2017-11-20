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

namespace VDS.RDF
{

    public class EventTests
    {
        private bool _graphAdded, _graphRemoved, _graphChanged;

        [Fact]
        public void GraphEventBubbling()
        {
            this._graphAdded = false;
            this._graphRemoved = false;
            this._graphChanged = false;

            //Create Store and Graph add attach handlers to Store
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            store.GraphAdded += this.HandleGraphAdded;
            store.GraphRemoved += this.HandleGraphRemoved;
            store.GraphChanged += this.HandleGraphChanged;

            //Add the Graph to the Store which should fire the GraphAdded event
            store.Add(g);
            Assert.True(this._graphAdded, "GraphAdded event of the Triple Store should have fired");

            //Assert a Triple
            INode s = g.CreateBlankNode();
            INode p = g.CreateUriNode("rdf:type");
            INode o = g.CreateUriNode("rdfs:Class");
            Triple t = new Triple(s, p, o);
            g.Assert(t);
            Assert.True(this._graphChanged, "GraphChanged event of the Triple Store should have fired");

            //Retract the Triple
            this._graphChanged = false;
            g.Retract(t);
            Assert.True(this._graphChanged, "GraphChanged event of the Triple Store should have fired");

            //Remove the Graph from the Store which should fire the GraphRemoved event
            store.Remove(g.BaseUri);
            Assert.True(this._graphRemoved, "GraphRemoved event of the Triple Store should have fired");
        }

        protected void HandleGraphAdded(Object sender, TripleStoreEventArgs args)
        {
            this._graphAdded = true;
            Console.WriteLine("GraphAdded event occurred on a " + sender.GetType().Name + " instance");
            TestTools.ShowGraph(args.GraphEvent.Graph);
            Console.WriteLine();
        }

        protected void HandleGraphRemoved(Object sender, TripleStoreEventArgs args)
        {
            this._graphRemoved = true;
            Console.WriteLine("GraphRemoved event occurred on a " + sender.GetType().Name + " instance");
            TestTools.ShowGraph(args.GraphEvent.Graph);
            Console.WriteLine();
        }

        protected void HandleGraphChanged(Object sender, TripleStoreEventArgs args)
        {
            this._graphChanged = true;
            Console.WriteLine("GraphChanged event occurred on a " + sender.GetType().Name + " instance");
            if (args.GraphEvent != null && args.GraphEvent.TripleEvent != null)
            {
                Console.WriteLine("  Event was a Triple Event");
                Console.WriteLine("  " + args.GraphEvent.TripleEvent.Triple.ToString());
                Console.WriteLine((args.GraphEvent.TripleEvent.WasAsserted ? "  Triple was asserted" : "  Triple was retracted"));
            }
            Console.WriteLine();
        }
    }
}
