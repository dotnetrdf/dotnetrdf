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
    using System.Linq;
    using VDS.RDF.Parsing;

    /// <summary>
    /// Represents SKOS resources that can be members of collections (concepts and collections)
    /// </summary>
    public abstract class SkosMember : SkosResource
    {
        internal SkosMember(INode resource) : base(resource) { }

        internal static SkosMember Create(INode node)
        {
            var a = node.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            var typeStatements = node.Graph.GetTriplesWithSubjectPredicate(node, a);

            var skosOrderedCollection = node.Graph.CreateUriNode(UriFactory.Create(SkosHelper.OrderedCollection));
            if (typeStatements.WithObject(skosOrderedCollection).Any())
            {
                return new SkosOrderedCollection(node);
            }

            var skosCollection = node.Graph.CreateUriNode(UriFactory.Create(SkosHelper.Collection));
            if (typeStatements.WithObject(skosCollection).Any())
            {
                return new SkosCollection(node);
            }

            return new SkosConcept(node);
        }
    }
}
