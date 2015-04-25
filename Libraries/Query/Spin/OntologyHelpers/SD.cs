/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using VDS.RDF.Query.Spin.Utility;

namespace VDS.RDF.Query.Spin.OntologyHelpers
{
    /**
     * Vocabulary for http://www.w3.org/TR/sparql11-service-description/
     */

    public static class SD
    {
        public readonly static String NS_URI = "http://www.w3.org/ns/sparql-service-description#";

        public readonly static IUriNode ClassDataset = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Dataset"));
        public readonly static IUriNode ClassFeature = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Feature"));
        public readonly static IUriNode ClassGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "Graph"));
        public readonly static IUriNode ClassGraphCollection = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "GraphCollection"));

        public readonly static IUriNode PropertyAvailableGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "availableGraph"));
        public readonly static IUriNode PropertyHasFeature = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "hasFeature"));
        public readonly static IUriNode PropertyNamedGraph = RDFHelper.CreateUriNode(UriFactory.Create(NS_URI + "namedGraph"));
    }
}