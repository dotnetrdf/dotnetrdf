/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test
{
    [TestClass]
    public class EventTests
    {
        private bool _graphAdded, _graphRemoved, _graphChanged;

        [TestMethod]
        public void GraphEventBubbling()
        {
            try
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
                Assert.IsTrue(this._graphAdded, "GraphAdded event of the Triple Store should have fired");

                //Assert a Triple
                INode s = g.CreateBlankNode();
                INode p = g.CreateUriNode("rdf:type");
                INode o = g.CreateUriNode("rdfs:Class");
                Triple t = new Triple(s, p, o);
                g.Assert(t);
                Assert.IsTrue(this._graphChanged, "GraphChanged event of the Triple Store should have fired");

                //Retract the Triple
                this._graphChanged = false;
                g.Retract(t);
                Assert.IsTrue(this._graphChanged, "GraphChanged event of the Triple Store should have fired");

                //Remove the Graph from the Store which should fire the GraphRemoved event
                store.Remove(g.BaseUri);
                Assert.IsTrue(this._graphRemoved, "GraphRemoved event of the Triple Store should have fired");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void HandleGraphAdded(Object sender, TripleStoreEventArgs args)
        {
            this._graphAdded = true;
            Console.WriteLine("GraphAdded event occurred on a " + sender.GetType().Name + " instance");
            TestTools.ShowGraph(args.GraphEvent.Graph);
            Console.WriteLine();
        }

        public void HandleGraphRemoved(Object sender, TripleStoreEventArgs args)
        {
            this._graphRemoved = true;
            Console.WriteLine("GraphRemoved event occurred on a " + sender.GetType().Name + " instance");
            TestTools.ShowGraph(args.GraphEvent.Graph);
            Console.WriteLine();
        }

        public void HandleGraphChanged(Object sender, TripleStoreEventArgs args)
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
