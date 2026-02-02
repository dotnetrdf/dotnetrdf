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
using VDS.RDF.Query;
using VDS.RDF.Update;

namespace VDS.RDF.LDF;

internal static class Queries
{
    private static readonly string Preamble = """
        PREFIX hydra: <http://www.w3.org/ns/hydra/core#>
        PREFIX void:  <http://rdfs.org/ns/void#>
        """;

    private static readonly string Shape = """
        ?fragment ^void:subset   ?dataset  .
        ?page     ^void:subset   ?fragment .
        ?search   ^hydra:search  ?dataset  .
        ?mapping  ^hydra:mapping ?search   .
        """;

    internal static SparqlQuery Select { get; } = new SparqlQueryParser().ParseFromString($$"""
            {{Preamble}}

            SELECT DISTINCT ?page ?search
            WHERE {
                {{Shape}}
            }
            """);

    internal static SparqlUpdateCommandSet Delete { get; } = new SparqlUpdateParser().ParseFromString($$"""
            {{Preamble}}
                        
            DELETE {
            	?datasetContainer ?datasetInverseProperty ?dataset       .
            	?dataset          ?datasetProperty        ?datasetValue  .
            	?fragment         ?fragmentProperty       ?fragmentValue .
            	?page             ?pageProperty           ?pageValue     .
            	?search           ?searchProperty         ?searchValue   .
            	?mapping          ?mappingProperty        ?mappingValue  .
            }
            WHERE {
                {{Shape}}
                        
            	?dataset  ?datasetProperty  ?datasetValue  .
            	?fragment ?fragmentProperty ?fragmentValue .
            	?search   ?searchProperty   ?searchValue   .
            	?mapping  ?mappingProperty  ?mappingValue  .
            
            	OPTIONAL { ?datasetContainer ?datasetInverseProperty ?dataset   . }
            	OPTIONAL { ?page             ?pageProperty           ?pageValue . }
            }
            """);
}
