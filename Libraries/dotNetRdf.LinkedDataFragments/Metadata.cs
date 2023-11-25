/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.LDF.Hydra;

namespace VDS.RDF.LDF
{
    internal class Metadata : WrapperGraph
    {
        private readonly Uri uri;

        internal Metadata(IGraph original, Uri uri)
            : base(original)
        {
            this.uri = uri;
            this.Fragment = new GraphWrapperNode(this.CreateUriNode(this.uri), this);
        }

        internal IriTemplate Search
        {
            get
            {
                return this.GetTriplesWithPredicate(Vocabulary.Hydra.Search).Select(t => new IriTemplate(t.Object, this)).SingleOrDefault();
            }
        }

        internal Uri NextPageUri
        {
            get
            {
                return Vocabulary.Hydra.Next.ObjectsOf(this.Fragment).Cast<IUriNode>().SingleOrDefault()?.Uri;
            }
        }

        internal long? TripleCount
        {
            get
            {
                return Vocabulary.Void.Triples.ObjectsOf(this.Fragment).SingleOrDefault()?.AsValuedNode().AsInteger();
            }
        }

        private GraphWrapperNode Fragment { get; set; }
    }
}
