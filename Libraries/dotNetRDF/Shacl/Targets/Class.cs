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

namespace VDS.RDF.Shacl.Targets
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using VDS.RDF.Parsing;

    internal class Class : Target
    {
        private static readonly NodeFactory Factory = new NodeFactory();
        private static readonly INode RdfsSubClassOf = Factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#subClassOf"));
        private static readonly INode RdfType = Factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        [DebuggerStepThrough]
        internal Class(INode node)
            : base(node)
        {
        }

        internal override IEnumerable<INode> SelectFocusNodes(IGraph dataGragh)
        {
            return
                InferSubclasses(this)
                .SelectMany(c =>
                    dataGragh.GetTriplesWithPredicateObject(RdfType, c)
                    .Select(t => t.Subject));
        }

        private static IEnumerable<INode> InferSubclasses(INode node, HashSet<INode> seen = null)
        {
            if (seen is null)
            {
                seen = new HashSet<INode>();
            }

            if (seen.Add(node))
            {
                yield return node;

                foreach (var subclass in RdfsSubClassOf.SubjectsOf(node))
                {
                    foreach (var inferred in InferSubclasses(subclass, seen))
                    {
                        yield return inferred;
                    }
                }
            }
        }
    }
}