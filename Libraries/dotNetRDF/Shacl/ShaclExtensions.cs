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

namespace VDS.RDF.Shacl
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Parsing;

    internal static class ShaclExtensions
    {
        private static readonly NodeFactory factory = new NodeFactory();
        private static readonly INode rdfs_subClassOf = factory.CreateUriNode(UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#subClassOf"));
        private static readonly INode rdf_type = factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        internal static IEnumerable<INode> SubjectsOf(this INode predicate, INode @object) =>
            from t in @object.Graph.GetTriplesWithPredicateObject(predicate, @object)
            select t.Subject;

        internal static IEnumerable<INode> ObjectsOf(this INode predicate, INode subject) =>
            from t in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
            select t.Object;

        internal static IEnumerable<INode> InstancesOf(this IGraph g, INode @class) =>
            rdf_type.SubjectsOf(@class.CopyNode(g));

        internal static IEnumerable<INode> ShaclInstancesOf(this IGraph g, INode @class) =>
            InferSubclasses(@class).SelectMany(c => g.InstancesOf(c));

        internal static bool IsShaclInstance(this INode @class, INode node) =>
            InferSubclasses(@class).Any(c => c.IsInstance(node));

        private static IEnumerable<INode> InferSubclasses(INode node, HashSet<INode> seen = null)
        {
            if (seen is null)
            {
                seen = new HashSet<INode>();
            }

            if (seen.Add(node))
            {
                yield return node;

                foreach (var subclass in rdfs_subClassOf.SubjectsOf(node))
                {
                    foreach (var inferred in InferSubclasses(subclass, seen))
                    {
                        yield return inferred;
                    }
                }
            }
        }

        internal static bool IsInstance(this INode @class, INode node)
        {
            return node.Graph.GetTriplesWithSubjectPredicate(node, rdf_type).WithObject(@class).Any();
        }
    }
}
