using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using System.Diagnostics;

namespace dotNetRDFTest
{
    class RemoteSparqlTestSuite
    {
        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);
        }

        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("RemoteSPARQLTestSuite.txt",false,Encoding.UTF8);
            String[] skipTests = { };

            Console.SetOut(output);

            output.WriteLine("### Running Remote SPARQL Endpoint Tests");
            output.WriteLine();
            output.WriteLine("Accept Header for SPARQL SELECT and ASK: " + MimeTypesHelper.HttpSparqlAcceptHeader);
            output.WriteLine("Accept Header for SPARQL DESCRIBE and CONSTRUCT: " + MimeTypesHelper.HttpAcceptHeader);
            output.WriteLine();

            //Options.HttpDebugging = true;
            try
            {
                SparqlRemoteEndpoint dbpedia = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
                SparqlResultSet results = new SparqlResultSet();

                String[] queries = new String[3];
                queries[0] = "select distinct ?Concept where {[] a ?Concept} limit 10";
                queries[1] = "select distinct ?Concept where {[] a ?Concept} limit 10 offset 5";
                queries[2] = "prefix skos: <http://www.w3.org/2004/02/skos/core#> select distinct ?City where {?City skos:subject <http://dbpedia.org/resource/Category:Cities_in_England>}";

                foreach (String query in queries)
                {
                    output.WriteLine("## Making a SPARQL SELECT Query");
                    output.WriteLine("# Query");
                    output.WriteLine();
                    output.WriteLine(query);
                    output.WriteLine();
                    output.WriteLine("# Results");

                    results = dbpedia.QueryWithResultSet(query);
                    foreach (SparqlResult result in results)
                    {
                        output.WriteLine(result.ToString());
                    }
                    output.WriteLine();
                }

                //Options.HttpFullDebugging = true;
                String gquery = "DESCRIBE <http://dbpedia.org/resource/Southampton>";
                output.WriteLine("## Making a SPARQL DESCRIBE Query");
                output.WriteLine("# Query");
                output.WriteLine();
                output.WriteLine(gquery);
                output.WriteLine();
                output.WriteLine("# Results");
                Graph g = dbpedia.QueryWithResultGraph(gquery);

                foreach (Triple t in g.Triples)
                {
                    output.WriteLine(t.ToString());
                }
            }
            catch (XmlException xmlEx)
            {
                reportError(output, "XML Exception", xmlEx);
            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }
            //Options.HttpDebugging = false;

            output.WriteLine();
            output.WriteLine("### Running Federated SPARQL Test");

            try
            {
                SparqlRemoteEndpoint dbpedia = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
                SparqlRemoteEndpoint bbcProgs = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://www.bbc.co.uk/programmes");
                SparqlRemoteEndpoint books = new SparqlRemoteEndpoint(new Uri("http://sparql.org/books"));

                String fedQuery = "SELECT * WHERE {?s a ?type} LIMIT 10";

                FederatedSparqlRemoteEndpoint fedEndpoint = new FederatedSparqlRemoteEndpoint(new SparqlRemoteEndpoint[] { dbpedia, bbcProgs/*, books*/ });
                fedEndpoint.MaxSimultaneousRequests = 1;
                SparqlResultSet results = fedEndpoint.QueryWithResultSet(fedQuery);
                foreach (SparqlResult result in results)
                {
                    output.WriteLine(result.ToString());
                }
                output.WriteLine();
            }
            catch (XmlException xmlEx)
            {
                reportError(output, "XML Exception", xmlEx);
            }
            catch (IOException ioEx)
            {
                reportError(output, "IO Exception", ioEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parsing Exception", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Exception", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }

            output.WriteLine();
            output.WriteLine("### Running Result Set Parser Tests");

            try
            {
                int testsPassed = 0;
                int testsFailed = 0;
                String[] files = Directory.GetFiles("sparql_tests");
                bool passed, passDesired;
                SparqlResultSet results = new SparqlResultSet();
                SparqlXmlParser parser = new SparqlXmlParser();

                foreach (String file in files)
                {
                    if (skipTests.Contains(Path.GetFileName(file)))
                    {
                        output.WriteLine("## Skipping Test of File " + Path.GetFileName(file));
                        output.WriteLine();
                        continue;
                    }

                    if (Path.GetExtension(file) != ".srx")
                    {
                        continue;
                    }

                    Debug.WriteLine("Testing File " + Path.GetFileName(file));
                    output.WriteLine("## Testing File " + Path.GetFileName(file));
                    output.WriteLine("# Test Started at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));

                    passed = false;
                    passDesired = true;

                    try
                    {
                        if (Path.GetFileNameWithoutExtension(file).StartsWith("bad"))
                        {
                            passDesired = false;
                            output.WriteLine("# Desired Result = Parsing Failed");
                        }
                        else
                        {
                            output.WriteLine("# Desired Result = Parses OK");
                        }

                        results = new SparqlResultSet();
                        parser.Load(results, file);

                        passed = true;
                        output.WriteLine("Parsed OK");
                    }
                    catch (XmlException xmlEx)
                    {
                        reportError(output, "XML Exception", xmlEx);
                    }
                    catch (IOException ioEx)
                    {
                        reportError(output, "IO Exception", ioEx);
                    }
                    catch (RdfParseException parseEx)
                    {
                        reportError(output, "Parsing Exception", parseEx);
                    }
                    catch (RdfException rdfEx)
                    {
                        reportError(output, "RDF Exception", rdfEx);
                    }
                    catch (Exception ex)
                    {
                        reportError(output, "Other Exception", ex);
                    }
                    finally
                    {
                        if (passed && passDesired)
                        {
                            //Passed and we wanted to Pass
                            testsPassed++;
                            output.WriteLine("# Result = Test Passed");
                        }
                        else if (!passed && passDesired)
                        {
                            //Failed when we should have Passed
                            testsFailed++;
                            output.WriteLine("# Result = Test Failed");
                        }
                        else if (passed && !passDesired)
                        {
                            //Passed when we should have Failed
                            testsFailed++;
                            output.WriteLine("# Result = Test Failed");
                        }
                        else
                        {
                            //Failed and we wanted to Fail
                            testsPassed++;
                            output.WriteLine("# Result = Test Passed");
                        }

                        output.WriteLine("# Results Generated = " + results.Count);
                        output.WriteLine("# Query Result was " + results.Result);
                        output.WriteLine("# Test Ended at " + DateTime.Now.ToString(TestSuite.TestSuiteTimeFormat));
                    }

                    output.WriteLine();
                }

                output.WriteLine(testsPassed + " Tests Passed");
                output.WriteLine(testsFailed + " Tests Failed");

            }
            catch (Exception ex)
            {
                reportError(output, "Other Exception", ex);
            }

            Console.SetOut(Console.Out);
            Console.WriteLine("Done");
            Debug.WriteLine("Finished");

            output.Close();

        }
    }
}
