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
using System.Linq;
using System.Xml.Linq;

namespace VDS.RDF.Writing;

public class GraphMLFixture
{
    internal ITripleStore Input { get; private set; }

    internal XDocument Output { get; private set; }

    public GraphMLFixture()
    {
        Output = new XDocument();
        Input = GraphMLFixture.Load();

        using (var outputWriter = Output.CreateWriter())
        {
            new GraphMLWriter().Save(Input, outputWriter);
        }
    }

    private static ITripleStore Load()
    {
        var store = new TripleStore();

        var graph1 = new Graph();
        graph1.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(graph1);

        var graph2 = new Graph(new UriNode(new Uri("http://example.com/graph2")));
        graph2.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(graph2);

        return store;
    }

    internal XElement GraphElementByBaseUri(Uri baseUri)
    {
        var graphElements = Output.Descendants(XName.Get(GraphMLSpecsHelper.Graph, GraphMLSpecsHelper.NS));

        if (baseUri == null)
        {
            return graphElements.Single(element =>
                element.Elements().All(child => child.Name.LocalName != GraphMLSpecsHelper.Data));
            //return graphElements.Single(element => element.Attribute(GraphMLSpecsHelper.Id) == null);
        }
        else
        {
            return graphElements.Single(element => element.Elements().Any(child =>
                child.Name.LocalName == GraphMLSpecsHelper.Data &&
                child.Attribute("key").Value.Equals(GraphMLSpecsHelper.GraphLabel) && 
                child.Value.Equals(baseUri.AbsoluteUri)));
            //return graphElements.Single(element => element.Attribute(GraphMLSpecsHelper.Id)?.Value == baseUri.AbsoluteUri);
        }
    }

}
