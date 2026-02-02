/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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

using VDS.RDF.Parsing;

namespace VDS.RDF.LDF;

internal static class Vocabulary
{
    private static readonly NodeFactory Factory = new();

    private static IUriNode Node(string name) => Factory.CreateUriNode(UriFactory.Create(name));

    private static IUriNode Node(string baseUri, string name) => Node($"{baseUri}{name}");

    internal static class Hydra
    {
        private const string BaseUri = "http://www.w3.org/ns/hydra/core#";

        internal static IUriNode Next { get; } = Node(BaseUri, "next");

        internal static IUriNode Variable { get; } = Node(BaseUri, "variable");

        internal static IUriNode Property { get; } = Node(BaseUri, "property");

        internal static IUriNode Mapping { get; } = Node(BaseUri, "mapping");

        internal static IUriNode Template { get; } = Node(BaseUri, "template");
    }

    internal static class Void
    {
        private const string BaseUri = "http://rdfs.org/ns/void#";

        internal static IUriNode Triples { get; } = Node(BaseUri, "triples");
    }

    internal static class Rdf
    {
        internal static IUriNode Subject { get; } = Node(RdfSpecsHelper.RdfSubject);

        internal static IUriNode Predicate { get; } = Node(RdfSpecsHelper.RdfPredicate);

        internal static IUriNode Object { get; } = Node(RdfSpecsHelper.RdfObject);
    }
}
