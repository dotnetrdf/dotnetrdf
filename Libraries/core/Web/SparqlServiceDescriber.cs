/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Web.Configuration;
using VDS.RDF.Web.Configuration.Query;
using VDS.RDF.Web.Configuration.Server;
using VDS.RDF.Web.Configuration.Update;

namespace VDS.RDF.Web
{
    //REQ: Implement SPARQL Service Descriptions
    //Q: Are multiple service descriptions in a single file permitted?

    /// <summary>
    /// Static Helper class responsible for generating SPARQL Service Description Graphs based on a given Configuration object
    /// </summary>
    public static class SparqlServiceDescriber
    {
        public const String ServiceDescriptionNamespace = "http://www.w3.org/ns/sparql-service-description#";

        private const String ClassService = "Service",
                             ClassLanguage = "Language",
                             ClassFunction = "Function",
                             ClassAggregate = "Aggregate",
                             ClassEntailmentRegime = "EntailmentRegime",
                             ClassEntailmentProfile = "EntailmentProfile",
                             ClassGraphCollection = "GraphCollection",
                             ClassDataset = "Dataset",
                             ClassGraph = "Graph",
                             ClassNamedGraph = "NamedGraph";

        private const String InstanceSparql10Query = "SPARQL10Query",
                             InstanceSparql11Query = "SPARQL11Query",
                             InstanceSparql11Update = "SPARQL11Update",
                             InstanceDereferencesURIs = "DereferencesURIs",
                             InstanceUnionDefaultGraph = "UnionDefaultGraph",
                             InstanceRequiresDataset = "RequiresDataset",
                             InstanceEmptyGraphs = "EmptyGraphs";

        private const String PropertyUrl = "url",
                             PropertyFeatures = "feature",
                             PropertyDefaultEntailmentRegime = "defaultEntailmentRegime",
                             PropertySupportedEntailmentRegime = "supportedEntailmentRegime",
                             PropertyExtensionFunction = "extensionFunction",
                             PropertyExtensionAggregate = "extensionAggregate",
                             PropertyLanguageExtension = "languageExtension",
                             PropertySupportedLanguage = "supportedLanguage",
                             PropertyPropertyFeature = "propertyFeature",
                             PropertyDefaultDatasetDescription = "defaultDatasetDescription",
                             PropertyAvailableGraphDescriptions = "availableGraphDescriptions",
                             PropertyResultFormat = "resultFormat",
                             PropertyInputFormat = "inputFormat",
                             PropertyDefaultGraph = "defaultGraph",
                             PropertyNamedGraph = "namedGraph",
                             PropertyName = "name",
                             PropertyGraph = "graph";

        private static IGraph GetNewGraph()
        {
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("sd", new Uri(SparqlServiceDescriber.ServiceDescriptionNamespace));
            g.NamespaceMap.AddNamespace("void", new Uri("http://rdfs.org/ns/void#"));
            g.NamespaceMap.AddNamespace("scovo", new Uri("http://purl.org/NET/scovo#"));

            return g;
        }


        public static IGraph GetServiceDescription(BaseQueryHandlerConfiguration config, Uri descripUri)
        {
            IGraph g = SparqlServiceDescriber.GetNewGraph();

            UriNode descrip = g.CreateUriNode(descripUri);
            UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            UriNode service = g.CreateUriNode("sd:" + ClassService);
            
            throw new NotImplementedException();
        }

        public static IGraph GetServiceDescription(BaseSparqlServerConfiguration config, Uri descripUri)
        {
            throw new NotImplementedException();
        }

        public static IGraph GetServiceDescription(BaseUpdateHandlerConfiguration config, Uri descripUri)
        {
            throw new NotImplementedException();
        }
    }
}
