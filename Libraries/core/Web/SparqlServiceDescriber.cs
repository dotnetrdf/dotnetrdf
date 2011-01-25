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

#if !NO_ASP && !NO_WEB

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;
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
        /// <summary>
        /// Namespace URI for SPARQL Service Description 1.1
        /// </summary>
        public const String ServiceDescriptionNamespace = "http://www.w3.org/ns/sparql-service-description#";

        /// <summary>
        /// Constants for SPARQL Service Description Classes
        /// </summary>
        public const String ClassService = "Service",
                            ClassLanguage = "Language",
                            ClassFunction = "Function",
                            ClassAggregate = "Aggregate",
                            ClassEntailmentRegime = "EntailmentRegime",
                            ClassEntailmentProfile = "EntailmentProfile",
                            ClassGraphCollection = "GraphCollection",
                            ClassDataset = "Dataset",
                            ClassGraph = "Graph",
                            ClassNamedGraph = "NamedGraph";

        /// <summary>
        /// Constants for SPARQL Service Description Instances
        /// </summary>
        public const String InstanceSparql10Query = "SPARQL10Query",
                            InstanceSparql11Query = "SPARQL11Query",
                            InstanceSparql11Update = "SPARQL11Update",
                            InstanceDereferencesURIs = "DereferencesURIs",
                            InstanceUnionDefaultGraph = "UnionDefaultGraph",
                            InstanceRequiresDataset = "RequiresDataset",
                            InstanceEmptyGraphs = "EmptyGraphs";

        /// <summary>
        /// Constants for SPARQL Service Description Properties
        /// </summary>
        public const String PropertyUrl = "url",
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


        public static IGraph GetServiceDescription(HttpContext context, BaseQueryHandlerConfiguration config, Uri descripUri)
        {
            IGraph g = SparqlServiceDescriber.GetNewGraph();

            //Add the Top Level Node representing the Service
            UriNode descrip = g.CreateUriNode(descripUri);
            UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            UriNode service = g.CreateUriNode("sd:" + ClassService);
            g.Assert(descrip, rdfType, service);

            //Add its sd:url
            UriNode url = g.CreateUriNode("sd:" + PropertyUrl);
            g.Assert(descrip, url, descrip);

            //Add the sd:supportedLanguage - Requires Query Language to be configurable through the Configuration API
            UriNode supportedLang = g.CreateUriNode("sd:" + PropertySupportedLanguage);
            UriNode lang;
            switch (config.Syntax)
            {
                case SparqlQuerySyntax.Extended:
                case SparqlQuerySyntax.Sparql_1_1:
                    lang = g.CreateUriNode("sd:" + InstanceSparql11Query);
                    break;
                default:
                    lang = g.CreateUriNode("sd:" + InstanceSparql10Query);
                    break;
            }
            g.Assert(descrip, supportedLang, lang);

            //Add the Result Formats
            UriNode resultFormat = g.CreateUriNode("sd:" + PropertyResultFormat);
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (definition.CanWriteRdf || definition.CanWriteSparqlResults)
                {
                    if (definition.FormatUri != null)
                    {
                        g.Assert(descrip, resultFormat, g.CreateUriNode(new Uri(definition.FormatUri)));
                    }
                }
            }

            //Add Features and Dataset Description
            //First add descriptions for Global Expression Factories
            UriNode extensionFunction = g.CreateUriNode("sd:" + PropertyExtensionFunction);
            UriNode extensionAggregate = g.CreateUriNode("sd:" + PropertyExtensionAggregate);
            foreach (ISparqlCustomExpressionFactory factory in SparqlExpressionFactory.Factories)
            {
                foreach (Uri u in factory.AvailableExtensionFunctions)
                {
                    g.Assert(descrip, extensionFunction, g.CreateUriNode(u));
                }
                foreach (Uri u in factory.AvailableExtensionAggregates)
                {
                    g.Assert(descrip, extensionAggregate, g.CreateUriNode(u));
                }
            }

            //Then get the Configuration Object to add any other Feature Descriptions it wishes to
            config.AddFeatureDescription(g, descrip);

            return g;
        }

        public static IGraph GetServiceDescription(HttpContext context, BaseSparqlServerConfiguration config, Uri descripUri)
        {
            IGraph g = SparqlServiceDescriber.GetNewGraph();

            INode queryNode, updateNode, protocolNode;

            //Query Service Description
            if (config.QueryProcessor != null)
            {
                //Add the Top Level Node representing the Query Service
                queryNode = g.CreateUriNode(new Uri(descripUri, "query"));
                UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                UriNode service = g.CreateUriNode("sd:" + ClassService);
                g.Assert(queryNode, rdfType, service);

                //Add its sd:url
                UriNode url = g.CreateUriNode("sd:" + PropertyUrl);
                g.Assert(queryNode, url, queryNode);

                //Add the sd:supportedLanguage - Requires Query Language to be configurable through the Configuration API
                UriNode supportedLang = g.CreateUriNode("sd:" + PropertySupportedLanguage);
                UriNode lang;
                switch (config.QuerySyntax)
                {
                    case SparqlQuerySyntax.Extended:
                    case SparqlQuerySyntax.Sparql_1_1:
                        lang = g.CreateUriNode("sd:" + InstanceSparql11Query);
                        break;
                    default:
                        lang = g.CreateUriNode("sd:" + InstanceSparql10Query);
                        break;
                }
                g.Assert(queryNode, supportedLang, lang);

                //Add the Result Formats
                UriNode resultFormat = g.CreateUriNode("sd:" + PropertyResultFormat);
                foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
                {
                    if (definition.CanWriteRdf || definition.CanWriteSparqlResults)
                    {
                        if (definition.FormatUri != null)
                        {
                            g.Assert(queryNode, resultFormat, g.CreateUriNode(new Uri(definition.FormatUri)));
                        }
                    }
                }

                //Add Features and Dataset Description
                //First add descriptions for Global Expression Factories
                UriNode extensionFunction = g.CreateUriNode("sd:" + PropertyExtensionFunction);
                UriNode extensionAggregate = g.CreateUriNode("sd:" + PropertyExtensionAggregate);
                foreach (ISparqlCustomExpressionFactory factory in SparqlExpressionFactory.Factories)
                {
                    foreach (Uri u in factory.AvailableExtensionFunctions)
                    {
                        g.Assert(queryNode, extensionFunction, g.CreateUriNode(u));
                    }
                    foreach (Uri u in factory.AvailableExtensionAggregates)
                    {
                        g.Assert(queryNode, extensionAggregate, g.CreateUriNode(u));
                    }
                }
            }
            else
            {
                queryNode = null;
            }

            //Update Service Description
            if (config.UpdateProcessor != null)
            {
                //TODO: Implement SPARQL Update Service Description
                updateNode = null;
            }
            else
            {
                updateNode = null;
            }

            //Uniform HTTP Protocol Description
            //Nothing happens here as there is no stuff in SPARQL Service Description 1.1 for describing such endpoints
            protocolNode = null;

            //Finally get the Configuration Node to add additional feature and dataset descriptions
            config.AddFeatureDescription(g, queryNode, updateNode, protocolNode);

            return g;
        }

        public static IGraph GetServiceDescription(HttpContext context, BaseUpdateHandlerConfiguration config, Uri descripUri)
        {
            throw new NotImplementedException();
        }
    }
}

#endif