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

using System;
using System.Collections.Generic;

namespace VDS.RDF.Parsing;

/// <summary>
/// A collection of statically defined RDFa contexts.
/// </summary>
public static class StaticRdfAContexts
{
    /// <summary>
    /// A static definition of the RDFa 1.1 Core context.
    /// </summary>
    public static RdfAContext RdfACoreContext = new(
        null,
        new List<KeyValuePair<string, string>>
        {
            new ("describedby", "http://www.w3.org/2007/05/powder-s#describedby"),
            new ("license", "http://www.w3.org/1999/xhtml/vocab#license"),
            new ("role", "http://www.w3.org/1999/xhtml/vocab#role"),
        },
        new List<KeyValuePair<string, string>>
        {
            new("dcat", "http://www.w3.org/ns/dcat#"),
            new("grddl", "http://www.w3.org/2003/g/data-view#"),
            new("as", "https://www.w3.org/ns/activitystreams#"),
            new("duv", "https://www.w3.org/ns/duv#"),
            new("csvw", "http://www.w3.org/ns/csvw#"),
            new("odrl", "http://www.w3.org/ns/odrl/2/"),
            new("oa", "http://www.w3.org/ns/oa#"),
            new("ma", "http://www.w3.org/ns/ma-ont#"),
            new("dqv", "http://www.w3.org/ns/dqv#"),
            new("org", "http://www.w3.org/ns/org#"),
            new("prov", "http://www.w3.org/ns/prov#"),
            new("ldp", "http://www.w3.org/ns/ldp#"),
            new("qb", "http://purl.org/linked-data/cube#"),
            new("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#"),
            new("owl", "http://www.w3.org/2002/07/owl#"),
            new("rdfa", "http://www.w3.org/ns/rdfa#"),
            new("sd", "http://www.w3.org/ns/sparql-service-description#"),
            new("jsonld", "http://www.w3.org/ns/json-ld#"),
            new("skosxl", "http://www.w3.org/2008/05/skos-xl#"),
            new("time", "http://www.w3.org/2006/time#"),
            new("sosa", "http://www.w3.org/ns/sosa/"),
            new("ssn", "http://www.w3.org/ns/ssn/"),
            new("void", "http://rdfs.org/ns/void#"),
            new("wdr", "http://www.w3.org/2007/05/powder#"),
            new("wdrs", "http://www.w3.org/2007/05/powder-s#"),
            new("xml", "http://www.w3.org/XML/1998/namespace"),
            new("xsd", "http://www.w3.org/2001/XMLSchema#"),
            new("xhv", "http://www.w3.org/1999/xhtml/vocab#"),
            new("rr", "http://www.w3.org/ns/r2rml#"),
            new("rdfs", "http://www.w3.org/2000/01/rdf-schema#"),
            new("dc", "http://purl.org/dc/terms/"),
            new("cc", "http://creativecommons.org/ns#"),
            new("ctag", "http://commontag.org/ns#"),
            new("dcterms", "http://purl.org/dc/terms/"),
            new("dc11", "http://purl.org/dc/elements/1.1/"),
            new("gr", "http://purl.org/goodrelations/v1#"),
            new("rev", "http://purl.org/stuff/rev#"),
            new("foaf", "http://xmlns.com/foaf/0.1/"),
            new("v", "http://rdf.data-vocabulary.org/#"),
            new("og", "http://ogp.me/ns#"),
            new("sioc", "http://rdfs.org/sioc/ns#"),
            new("schema", "http://schema.org/"),
            new("ical", "http://www.w3.org/2002/12/cal/icaltzd#"),
            new("vcard", "http://www.w3.org/2006/vcard/ns#"),
            new("skos", "http://www.w3.org/2004/02/skos/core#"),
            new("rif", "http://www.w3.org/2007/rif#"),
        });

    /// <summary>
    /// A static definition of the RDFa 1.1 + XHTML context.
    /// </summary>
    public static RdfAContext XhtmlRdfAContext = new (
        RdfACoreContext,
        //RdfAParser.XHtmlVocabNamespace,
        null,
        new List<KeyValuePair<string, string>>
        {
            new ("alternate",RdfAParser.XHtmlVocabNamespace+"alternate"),
            new ("appendix", RdfAParser.XHtmlVocabNamespace+"appendix"),
            new ("bookmark", RdfAParser.XHtmlVocabNamespace + "bookmark"),
            new ("cite",RdfAParser.XHtmlVocabNamespace+"cite"),
            new ("chapter",RdfAParser.XHtmlVocabNamespace+"chapter"),
            new ("contents",RdfAParser.XHtmlVocabNamespace+"contents"),
            new ("copyright",RdfAParser.XHtmlVocabNamespace+"copyright"),
            new ("first",RdfAParser.XHtmlVocabNamespace+"first"),
            new ("glossary",RdfAParser.XHtmlVocabNamespace+"glossary"),
            new ("help",RdfAParser.XHtmlVocabNamespace+"help"),
            new ("icon",RdfAParser.XHtmlVocabNamespace+"icon"),
            new ("index",RdfAParser.XHtmlVocabNamespace+"index"),
            new ("last",RdfAParser.XHtmlVocabNamespace+"last"),
            new ("license",RdfAParser.XHtmlVocabNamespace+"license"),
            new ("meta",RdfAParser.XHtmlVocabNamespace+"meta"),
            new ("next",RdfAParser.XHtmlVocabNamespace+"next"),
            new ("p3pv1",RdfAParser.XHtmlVocabNamespace+"p3pv1"),
            new ("prev",RdfAParser.XHtmlVocabNamespace+"prev"),
            new ("role",RdfAParser.XHtmlVocabNamespace+"role"),
            new ("section",RdfAParser.XHtmlVocabNamespace+"section"),
            new ("stylesheet",RdfAParser.XHtmlVocabNamespace+"stylesheet"),
            new ("subsection",RdfAParser.XHtmlVocabNamespace+"subsection"),
            new ("start",RdfAParser.XHtmlVocabNamespace+"start"),
            new ("top",RdfAParser.XHtmlVocabNamespace+"top"),
            new ("up",RdfAParser.XHtmlVocabNamespace+"up"),
        },
        new List<KeyValuePair<string, Uri>>
        {
            new( "dcat", new Uri("http://www.w3.org/ns/dcat#")),
            new( "grddl", new Uri("http://www.w3.org/2003/g/data-view#")),
            new( "as", new Uri("https://www.w3.org/ns/activitystreams#")),
            new( "duv", new Uri("https://www.w3.org/ns/duv#")),
            new( "csvw", new Uri("http://www.w3.org/ns/csvw#")),
            new( "odrl", new Uri("http://www.w3.org/ns/odrl/2/")),
            new( "oa", new Uri("http://www.w3.org/ns/oa#")),
            new( "ma", new Uri("http://www.w3.org/ns/ma-ont#")),
            new( "dqv", new Uri("http://www.w3.org/ns/dqv#")),
            new( "org", new Uri("http://www.w3.org/ns/org#")),
            new( "prov", new Uri("http://www.w3.org/ns/prov#")),
            new( "ldp", new Uri("http://www.w3.org/ns/ldp#")),
            new( "qb", new Uri("http://purl.org/linked-data/cube#")),
            new( "rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#")),
            new( "owl", new Uri("http://www.w3.org/2002/07/owl#")),
            new( "rdfa", new Uri("http://www.w3.org/ns/rdfa#")),
            new( "sd", new Uri("http://www.w3.org/ns/sparql-service-description#")),
            new( "jsonld", new Uri("http://www.w3.org/ns/json-ld#")),
            new( "skosxl", new Uri("http://www.w3.org/2008/05/skos-xl#")),
            new( "time", new Uri("http://www.w3.org/2006/time#")),
            new( "sosa", new Uri("http://www.w3.org/ns/sosa/")),
            new( "ssn", new Uri("http://www.w3.org/ns/ssn/")),
            new( "void", new Uri("http://rdfs.org/ns/void#")),
            new( "wdr", new Uri("http://www.w3.org/2007/05/powder#")),
            new( "wdrs", new Uri("http://www.w3.org/2007/05/powder-s#")),
            new( "xml", new Uri("http://www.w3.org/XML/1998/namespace")),
            new( "xsd", new Uri("http://www.w3.org/2001/XMLSchema#")),
            new( "xhv", new Uri("http://www.w3.org/1999/xhtml/vocab#")),
            new( "rr", new Uri("http://www.w3.org/ns/r2rml#")),
            new( "rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#")),
            new( "dc", new Uri("http://purl.org/dc/terms/")),
            new( "cc", new Uri("http://creativecommons.org/ns#")),
            new( "ctag", new Uri("http://commontag.org/ns#")),
            new( "dcterms", new Uri("http://purl.org/dc/terms/")),
            new( "dc11", new Uri("http://purl.org/dc/elements/1.1/")),
            new( "gr", new Uri("http://purl.org/goodrelations/v1#")),
            new( "rev", new Uri("http://purl.org/stuff/rev#")),
            new( "foaf", new Uri("http://xmlns.com/foaf/0.1/")),
            new( "v", new Uri("http://rdf.data-vocabulary.org/#")),
            new( "og", new Uri("http://ogp.me/ns#")),
            new( "sioc", new Uri("http://rdfs.org/sioc/ns#")),
            new( "schema", new Uri("http://schema.org/")),
            new( "ical", new Uri("http://www.w3.org/2002/12/cal/icaltzd#")),
            new( "vcard", new Uri("http://www.w3.org/2006/vcard/ns#")),
            new( "skos", new Uri("http://www.w3.org/2004/02/skos/core#")),
            new( "rif", new Uri("http://www.w3.org/2007/rif#")),
        });
}