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
    /// <summary>
    /// Static Helper class for the SKOS API
    /// </summary>
    public static class SkosHelper
    {
        /// <summary>
        /// SKOS namespace and prefix
        /// </summary>
        public const string
            Prefix = "skos",
            Namespace = "http://www.w3.org/2004/02/skos/core#";

        /// <summary>
        /// Constant URIs for classes and properties exposed by the SKOS API and its derived classes
        /// </summary>
        public const string
            Concept = Namespace + "Concept",
            ConceptScheme = Namespace + "ConceptScheme",
            InScheme = Namespace + "inScheme",
            HasTopConcept = Namespace + "hasTopConcept",
            TopConceptOf = Namespace + "topConceptOf",
            PrefLabel = Namespace + "prefLabel",
            AltLabel = Namespace + "altLabel",
            HiddenLabel = Namespace + "hiddenLabel",
            Notation = Namespace + "notation",
            Note = Namespace + "note",
            ChangeNote = Namespace + "changeNote",
            Definition = Namespace + "definition",
            EditorialNote = Namespace + "editorialNote",
            Example = Namespace + "example",
            HistoryNote = Namespace + "historyNote",
            ScopeNote = Namespace + "scopeNote",
            SemanticRelation = Namespace + "semanticRelation",
            Broader = Namespace + "broader",
            Narrower = Namespace + "narrower",
            Related = Namespace + "related",
            BroaderTransitive = Namespace + "broaderTransitive",
            NarrowerTransitive = Namespace + "narrowerTransitive",
            Collection = Namespace + "Collection",
            OrderedCollection = Namespace + "OrderedCollection",
            Member = Namespace + "member",
            MemberList = Namespace + "memberList",
            MappingRelation = Namespace + "mappingRelation",
            CloseMatch = Namespace + "closeMatch",
            ExactMatch = Namespace + "exactMatch",
            BroadMatch = Namespace + "broadMatch",
            NarrowMatch = Namespace + "narrowMatch",
            RelatedMatch = Namespace + "relatedMatch";
    }
}
