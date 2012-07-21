/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.IO;
using System.Reflection;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Utilities.Web.Deploy
{
    public class Test
    {
        private const String UnknownVocabularyTermsTest = "SELECT DISTINCT ?term WHERE { {{[] a ?term} UNION {[] ?term []} FILTER (IsUri(?term) && fn:starts-with(STR(?term), \"http://www.dotnetrdf.org/configuration\")) } MINUS {GRAPH <http://www.dotnetrdf.org/configuration#> { {?term a rdfs:Class} UNION {?term a rdf:Property} } } }";
        private const String MissingConfigurationTypeTest = "SELECT DISTINCT * WHERE { ?s a ?type . MINUS { ?s dnr:type ?dotnetType } FILTER (?type != dnr:User && ?type != dnr:Proxy) }";
        private const String InvalidTypePropertyTest = "SELECT DISTINCT * WHERE { ?s dnr:type ?type . FILTER(!IsLiteral(?type)) }";
        private const String MultipleTypeTest = "SELECT DISTINCT ?s WHERE {?s dnr:type ?type} GROUP BY ?s HAVING (COUNT(?type) > 1)";
        private const String LibraryTypesTest = "SELECT DISTINCT * WHERE {?s dnr:type ?type . FILTER(IsLiteral(?type) && fn:starts-with(?type, \"VDS.RDF\")) }";
        private const String BadHandlerUriTest = "SELECT DISTINCT * WHERE {?s a dnr:HttpHandler . FILTER(!IsUri(?s) || !fn:starts-with(STR(?s), \"dotnetrdf\")) }";
        private const String MissingHandlerTypeTest = "SELECT DISTINCT * WHERE { ?s a dnr:HttpHandler . MINUS { ?s dnr:type ?dotnetType } }";
        private const String InvalidHandlerTypeTest = "SELECT DISTINCT * WHERE { ?s a dnr:HttpHandler ; dnr:type ?type . FILTER(IsLiteral(?type) && fn:starts-with(?type, \"VDS.RDF\")) }";
        private const String InvalidRangeTest = "SELECT DISTINCT * WHERE { ?property a rdf:Property ; rdfs:range ?range . ?s ?property ?obj . OPTIONAL { ?obj a ?type }. FILTER (ISURI(?range) && ?range != ?type && !(IsLiteral(?obj) && DATATYPE(?obj) = ?range)) }";
        private const String ValidRangeTest = "ASK WHERE { @property a rdf:Property ; rdfs:range ?range . @s @property ?obj . OPTIONAL { ?obj a ?type }. FILTER (?range = ?type || (IsLiteral(?obj) && DATATYPE(?obj) = ?range)) }";
        private const String InvalidDomainTest = "SELECT DISTINCT * WHERE { ?property a rdf:Property ; rdfs:domain ?domain . ?s ?property ?obj ; a ?type FILTER(ISURI(?domain) && ?domain != ?type) }";
        private const String ValidDomainTest = "ASK WHERE { @property a rdf:Property ; rdfs:domain ?domain . @s @property ?obj ; a ?type FILTER(?domain = ?type) }";
        private const String ClearTextPasswordTest = "SELECT DISTINCT ?s WHERE {?s dnr:password ?password . FILTER(ISLITERAL(?password)) }";

        public void RunTest(String[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: 2 Arguments are required in order to use the -test mode");
                return;
            }

            if (!File.Exists(args[1]))
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot test " + args[1] + " since the file does not exist");
                return;
            }

            Graph g = new Graph();
            try
            {
                FileLoader.Load(g, args[1]);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot parse the configuration file");
                Console.Error.WriteLine("rdfWebDeploy: Error: " + ex.Message);
                return;
            }
            Console.WriteLine("rdfWebDeploy: Opened the configuration file successfully");

            Graph vocab = new Graph();
            try
            {
                vocab.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                vocab.LoadFromEmbeddedResource("VDS.RDF.Query.FullText.ttl, dotNetRDF.Query.FullText");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfWebDeploy: Error: Cannot parse the configuration vocabulary");
                Console.Error.WriteLine("rdfWebDeploy: Error: " + ex.Message);
                return;
            }
            Console.WriteLine("rdfWebDeploy: Loaded the configuration vocabulary successfully");
            Console.WriteLine();
            Console.WriteLine("rdfWebDeploy: Tests Started...");
            Console.WriteLine();

            //Now make some tests against it
            int warnings = 0, errors = 0;
            IInMemoryQueryableStore store = new TripleStore();
            store.Add(vocab);
            if (g.BaseUri == null) g.BaseUri = new Uri("dotnetrdf:config");
            store.Add(g);
            Object results;
            SparqlResultSet rset;

            //Unknown vocabulary term test
            Console.WriteLine("rdfWebDeploy: Testing for URIs in the vocabulary namespace which are unknown");
            results = store.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + UnknownVocabularyTermsTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: The configuration file uses the URI '" + r["term"] + "' for a property/type and this does not appear to be a valid term in the Configuration vocabulary");
                    errors++;
                }
            }
            Console.WriteLine();

            #region General dnr:type Tests

            //Missing dnr:type test
            Console.WriteLine("rdfWebDeploy: Testing for missing dnr:type properties");
            results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + MissingConfigurationTypeTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Warning: The Node '" + r["s"].ToString() + "' has an rdf:type but no dnr:type which may be needed by the Configuration Loader");
                    warnings++;
                }
            }
            Console.WriteLine();

            //Invalid dnr:type test
            Console.WriteLine("rdfWebDeploy: Testing that values given for dnr:type property are literals");
            results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + InvalidTypePropertyTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: The node '" + r["s"].ToString() + "' has a dnr:type of '" + r["type"].ToString() + "' which is not given as a string literal");
                    errors++;
                }
            }
            Console.WriteLine();

            //Multiple dnr:type test
            Console.WriteLine("rdfWebDeploy: Testing that no object has multiple dnr:type values");
            results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + MultipleTypeTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: The node '" + r["s"].ToString() + "' has multiple dnr:type values which is not valid");
                    errors++;
                }
            }
            Console.WriteLine();

            //Unknown Library Types test
            Console.WriteLine("rdfWebDeploy: Testing that values given for dnr:type property are valid");
            results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + LibraryTypesTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                Assembly dotnetrdf = Assembly.GetAssembly(typeof(IGraph));

                foreach (SparqlResult r in rset)
                {
                    String typeName = ((ILiteralNode)r["type"]).Value;
                    Type t = typeName.Contains(",") ? Type.GetType(typeName) : dotnetrdf.GetType(typeName);

                    if (t == null)
                    {
                        Console.Error.WriteLine("rdfWebDeploy: Error: The node '" + r["s"].ToString() + "' has a dnr:type of '" + r["type"].ToString() + "' which does not appear to be a valid type in an available DLL");
                        errors++;
                    }
                    else
                    {
                        if (typeName.Contains(","))
                        {
                            String assm = typeName.Substring(typeName.IndexOf(",") + 1);
                            assm = assm.Trim();

                            switch (assm)
                            {
                                case "dotNetRDF.Data.Sql":
                                    Console.Error.WriteLine("rdfWebDeploy: Warning: The node '" + r["s"].ToString() + "' has a dnr:type of '" + r["type"].ToString() + "' which is in the dotNetRDF.Data.Sql library, please ensure you use the -sql option when deploying or if deploying manually include the Data.Sql library and its dependencies");
                                    warnings++;
                                    break;
                                case "dotNetRDF.Data.Virtuoso":
                                    Console.Error.WriteLine("rdfWebDeploy: Warning: The node '" + r["s"].ToString() + "' has a dnr:type of '" + r["type"].ToString() + "' which is in the dotNetRDF.Data.Virtuoso library, please ensure you use the -virtuoso option when deploying or if deploying manually include the Data.Virtuoso library and its dependencies");
                                    warnings++;
                                    break;
                                case "dotNetRDF.Query.FullText":
                                    Console.Error.WriteLine("rdfWebDeploy: Warning: The node '" + r["s"].ToString() + "' has a dnr:type of '" + r["type"].ToString() + "' which is in the dotNetRDF.Query.FullText library, please ensure you use the -fulltext option when deploying or if deploying manually include the Query.FullText library and its dependencies");
                                    warnings++;
                                    break;
                                default:
                                    Console.Error.WriteLine("rdfWebDeploy: Warning: The node '" + r["s"].ToString() + "' has a dnr:type of '" + r["type"].ToString() + "' which is in the " + assm + " library, please ensure that when deploying you manually include this library and any dependencies");
                                    warnings++;
                                    break;
                            }
                        }
                    }
                }
            }
            Console.WriteLine();

            #endregion

            #region dnr:HttpHandler Tests including specific dnr:type Tests

            //Bad Handler URI test
            Console.WriteLine("rdfWebDeploy: Testing for bad URIs given the rdf:type of dnr:HttpHandler");
            results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + BadHandlerUriTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: The Handler node '" + r["s"].ToString() + "' is not a URI of the form <dotnetrdf:/path> which is required for correct detection of handler configuration");
                    errors++;
                }
            }
            Console.WriteLine();

            //Missing Handler type test
            Console.WriteLine("rdfWebDeploy: Testing for missing dnr:type for dnr:HttpHandler objects");
            results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + MissingHandlerTypeTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Error: The Handler node '" + r["s"].ToString() + "' has an rdf:type but no dnr:type which is requiring for automated deployment via this tool");
                    errors++;
                }
            }
            Console.WriteLine();

            //Invalid Handler Type test
            Console.WriteLine("rdfWebDeploy: Testing that values given for dnr:type for dnr:HttpHandler objects in the VDS.RDF namespace are valid IHttpHandlers");
            results = g.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + InvalidHandlerTypeTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                Assembly dotnetrdf = Assembly.GetAssembly(typeof(IGraph));
                foreach (SparqlResult r in rset)
                {
                    Type t = dotnetrdf.GetType(((ILiteralNode)r["type"]).Value);
                    if (t != null)
                    {
                        if (!t.GetInterfaces().Any(i => i.Equals(typeof(System.Web.IHttpHandler))))
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: The node '" + r["s"].ToString() + "' has a dnr:type of '" + r["type"].ToString() + "' which does not appear to be a valid IHttpHandler implementation in dotNetRDF");
                            errors++;
                        }
                    }
                }
            }
            Console.WriteLine();

            #endregion

            #region Property Tests

            //Range test
            Console.WriteLine("rdfWebDeploy: Testing for bad ranges for properties");
            results = store.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + InvalidRangeTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    SparqlParameterizedString hasValidRange = new SparqlParameterizedString();
                    hasValidRange.CommandText = RdfWebDeployHelper.NamespacePrefixes + ValidRangeTest;
                    hasValidRange.SetParameter("property", r["property"]);
                    hasValidRange.SetParameter("s", r["s"]);
                    Object temp = store.ExecuteQuery(hasValidRange.ToString());
                    if (temp is SparqlResultSet)
                    {
                        if (!((SparqlResultSet)temp).Result)
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: The Node '" + r["s"].ToString() + "' has a value for the property '" + r["property"].ToString() + "' which is '" + r["obj"].ToString() + "' which does not appear to be valid for the range of this property which is '" + r["range"].ToString() + "'");
                            errors++;
                        }
                    }
                }
            }
            Console.WriteLine();

            //Domain test
            Console.WriteLine("rdfWebDeploy: Testing for bad domains for properties");
            results = store.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + InvalidDomainTest);
            if (results is SparqlResultSet)
            {
                rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    SparqlParameterizedString hasValidDomain = new SparqlParameterizedString();
                    hasValidDomain.CommandText = RdfWebDeployHelper.NamespacePrefixes + ValidDomainTest;
                    hasValidDomain.SetParameter("property", r["property"]);
                    hasValidDomain.SetParameter("s", r["s"]);
                    Object temp = store.ExecuteQuery(hasValidDomain.ToString());
                    if (temp is SparqlResultSet)
                    {
                        if (!((SparqlResultSet)temp).Result)
                        {
                            Console.Error.WriteLine("rdfWebDeploy: Error: The Node '" + r["s"].ToString() + "' has a value for the property '" + r["property"].ToString() + "' and the type given is '" + r["type"].ToString() + "' which does not match the domain of this property which is '" + r["domain"].ToString() + "'");
                            errors++;
                        }
                    }
                }
            }
            Console.WriteLine();

            #endregion

            #region Clear Text Password Tests

            Console.WriteLine("rdfWebDeploy: Testing for clear text passwords used with dnr:password property");
            results = store.ExecuteQuery(RdfWebDeployHelper.NamespacePrefixes + ClearTextPasswordTest);
            if (results is SparqlResultSet)
            {
                foreach (SparqlResult r in (SparqlResultSet)results)
                {
                    Console.Error.WriteLine("rdfWebDeploy: Warning: The Node '" + r["s"].ToString() + "' has a value for the dnr:password property specified as a Literal in clear text.  It is recommended that you specify any passwords as AppSetting URIs e.g. <appsetting:MyPassword> and then create an AppSetting in the <appSettings> section of your Web.config file to store your password.  The Web.config file can then have the <appSettings> section encrypted to help protect your password");
                    warnings++;
                }
            }
            Console.WriteLine();

            #endregion

            //Show Test Results
            Console.WriteLine("rdfWedDeploy: Tests Completed - " + warnings + " Warning(s) - " + errors + " Error(s)");
        }
    }
}
