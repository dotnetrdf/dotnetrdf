/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

namespace VDS.RDF.Skos
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Parsing;

    public class SkosGraph : WrapperGraph
    {
        public SkosGraph() : base()
        {
            this.InitializeNamespaceMap();
        }

        public SkosGraph(IGraph g) : base(g)
        {
            this.InitializeNamespaceMap();
        }

        private void InitializeNamespaceMap()
        {
            this.NamespaceMap.AddNamespace(SkosHelper.Prefix, UriFactory.Create(SkosHelper.Namespace));
        }

        public IEnumerable<SkosConceptScheme> ConceptSchemes
        {
            get
            {
                return this.GetInstances(SkosHelper.ConceptScheme).Cast<SkosConceptScheme>();
            }
        }

        public IEnumerable<SkosConcept> Concepts
        {
            get
            {
                return this.GetInstances(SkosHelper.Concept).Cast<SkosConcept>();
            }
        }

        public IEnumerable<SkosCollection> Collections
        {
            get
            {
                return this.GetInstances(SkosHelper.Collection).Cast<SkosCollection>();
            }
        }

        public IEnumerable<SkosOrderedCollection> OrderedCollections
        {
            get
            {
                return this.GetInstances(SkosHelper.OrderedCollection).Cast<SkosOrderedCollection>();
            }
        }

        private IEnumerable<INode> GetInstances(string typeUri)
        {
            var a = this.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            var type = this.CreateUriNode(UriFactory.Create(typeUri));

            return this
                .GetTriplesWithPredicateObject(a, type)
                .Select(t => t.Subject);
        }
    }
}
