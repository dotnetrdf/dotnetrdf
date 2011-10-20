using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Utilities.Editor.Syntax;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Utilities.Editor.AutoComplete.Data;
using VDS.RDF.Utilities.Editor.AutoComplete.Vocabularies;

namespace VDS.RDF.Utilities.Editor.AutoComplete
{
    public class AutoCompleteManager
    {
        private static bool _init = false;

        private static List<VocabularyDefinition> _builtInVocabs = new List<VocabularyDefinition>()
        {
            //Standard Prefixes
            new VocabularyDefinition("rdf", NamespaceMapper.RDF, "RDF Namespace"),
            new VocabularyDefinition("rdfs", NamespaceMapper.RDFS, "RDF Schema Namespace"),
            new VocabularyDefinition("owl", NamespaceMapper.OWL, "OWL Namespace"),
            new VocabularyDefinition("xsd", NamespaceMapper.XMLSCHEMA, "XML Schema Datatypes Namespace"),

            //Common Prefixes
            new VocabularyDefinition("dc", "http://purl.org/dc/elements/1.1/", "Dublin Core Elements Namespace"),
            new VocabularyDefinition("dcterms", "http://www.purl.org/dc/terms/", "Dublin Core Terms Namespace"),
            new VocabularyDefinition("foaf", "http://xmlns.com/foaf/0.1/", "Friend of a Friend Namespace for describing social networks"),
            new VocabularyDefinition("vcard", "http://www.w3.org/2006/vcard/ns#", "VCard Namespace"),
            new VocabularyDefinition("gr", "http://purl.org/goodrelations/v1#", "Good Relations Namespace for describing eCommerce"),
            new VocabularyDefinition("sioc", "http://rdfs.org/sioc/ns#", "Semantically Interlinked Online Communities Namespaces for describing social networks and online communitities"),
            new VocabularyDefinition("doap", "http://usefulinc.com/ns/doap#", "Description of a Project namespaces for describing projects"),
            new VocabularyDefinition("vann", "http://purl.org/vocab/vann/", "Vocabulary Annotation Namespace for annothing vocabularies and ontologies with descriptive information"),
            new VocabularyDefinition("vs", "http://www.w3.org/2003/06/sw-vocab-status/ns#", "Vocabulary Status Namespace for annotating vocabularies with term status information"),
            new VocabularyDefinition("skos", "http://www.w3.org/2004/02/skos/core#", "Simple Knowledge Organisation System for categorising things in hierarchies"),
            new VocabularyDefinition("geo", "http://www.w3.org/2003/01/geo/wgs84_pos#", "WGS84 Geo Positing Namespace for describing positions according to WGS84"),

            //Useful SPARQL Prefixes
            new VocabularyDefinition("fn", XPathFunctionFactory.XPathFunctionsNamespace, "XPath Functions Namespace used to refer to functions by URI in SPARQL queries"),
            new VocabularyDefinition("lfn", LeviathanFunctionFactory.LeviathanFunctionsNamespace, "Leviathan Functions Namespace used to refer to functions by URI in SPARQL queries"),
            new VocabularyDefinition("afn", ArqFunctionFactory.ArqFunctionsNamespace, "ARQ Functions Namespace used to refer to functions by URI in SPARQL queries"),

            //Other dotNetRDF Prefixes
            new VocabularyDefinition("dnr", ConfigurationLoader.ConfigurationNamespace, "dotNetRDF Configuration Namespace for specifying configuration files"),
            new VocabularyDefinition("dnr-ft", ConfigurationLoader.ConfigurationNamespace, "dotNetRDF Configuration Namespace for Query.FullText extensions")
        };

        private static List<AutoCompleteDefinition> _builtinCompleters = new List<AutoCompleteDefinition>()
        {
            new AutoCompleteDefinition("NTriples", typeof(NTriplesAutoCompleter<>)),
            new AutoCompleteDefinition("Turtle", typeof(TurtleAutoCompleter<>)),
            new AutoCompleteDefinition("Notation3", typeof(Notation3AutoCompleter<>)),

            new AutoCompleteDefinition("SparqlQuery10", typeof(Sparql10AutoCompleter<>)),
            new AutoCompleteDefinition("SparqlQuery11", typeof(Sparql11AutoCompleter<>)),

            new AutoCompleteDefinition("SparqlUpdate11", typeof(SparqlUpdateAutoCompleter<>)),

            //new AutoCompleteDefinition("TriG", new TurtleAutoCompleter())

        };

        private static List<NamespaceTerm> _terms = new List<NamespaceTerm>();
        private static HashSet<String> _loadedNamespaces = new HashSet<String>();
        private static LoadNamespaceTermsDelegate _namespaceLoader = new LoadNamespaceTermsDelegate(AutoCompleteManager.LoadNamespaceTerms);

        public static void Initialise()
        {
            if (_init) return;
            _init = true;

            //Have to intialise the Syntax Manager first
            SyntaxManager.Initialise();

            //Then start lazy loading Namespace Terms
            foreach (VocabularyDefinition vocab in _builtInVocabs)
            {
                _namespaceLoader.BeginInvoke(vocab.NamespaceUri, InitialiseNamepaceTerms, null);
            }
        }

        public static IAutoCompleter<T> GetAutoCompleter<T>(String name, ITextEditorAdaptor<T> editor)
        {
            foreach (AutoCompleteDefinition def in _builtinCompleters)
            {
                if (def.Name.Equals(name))
                {
                    try
                    {
                        Type ctype = def.Type;
                        Type target = ctype.MakeGenericType(new Type[] { typeof(T) });
                        IAutoCompleter<T> completer = (IAutoCompleter<T>)Activator.CreateInstance(target, new Object[] { editor });
                        return completer;
                    }
                    catch (Exception ex)
                    {
                        //Ignore errors as we'll try further definitions (if applicable)
                        //or return null at the end
                    }
                }
            }
            return null;
        }

        private static void InitialiseNamepaceTerms(IAsyncResult result)
        {
            try
            {
                _namespaceLoader.EndInvoke(result);
            }
            catch
            {
                //Ignore exceptions
            }
        }

        public static IEnumerable<VocabularyDefinition> Vocabularies
        {
            get
            {
                return _builtInVocabs;                       
            }
        }

        private delegate IEnumerable<NamespaceTerm> LoadNamespaceTermsDelegate(String namespaceUri);

        public static IEnumerable<NamespaceTerm> LoadNamespaceTerms(String namespaceUri)
        {
            //Don't load if already loaded
            if (_loadedNamespaces.Contains(namespaceUri)) return GetNamespaceTerms(namespaceUri);

            try
            {
                Graph g = new Graph();
                try
                {
                    UriLoader.Load(g, new Uri(namespaceUri));
                    if (g.Triples.Count == 0) throw new Exception("Did not appear to receive an RDF Format from Namespace URI " + namespaceUri);
                }
                catch
                {
                    //Try and load from our local copy if there is one
                    String prefix = GetDefaultPrefix(namespaceUri);
                    if (!prefix.Equals(String.Empty))
                    {
                        Stream localCopy = Assembly.GetExecutingAssembly().GetManifestResourceStream("VDS.RDF.Utilities.Editor.AutoComplete.Vocabularies." + prefix + ".ttl");
                        if (localCopy != null)
                        {
                            TurtleParser ttlparser = new TurtleParser();
                            ttlparser.Load(g, new StreamReader(localCopy));
                        }
                    }
                }
                List<NamespaceTerm> terms = new List<NamespaceTerm>();
                String termUri;

                //UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                IUriNode rdfsClass = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class"));
                IUriNode rdfsLabel = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
                IUriNode rdfsComment = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "comment"));
                IUriNode rdfProperty = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "Property"));
                IUriNode rdfsDatatype = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Datatype"));

                SparqlParameterizedString queryString = new SparqlParameterizedString();
                queryString.CommandText = "SELECT ?term (STR(?label) AS ?RawLabel) (STR(?comment) AS ?RawComment) WHERE { {{?term a @class} UNION {?term a @property} UNION {?term a @datatype}} OPTIONAL {?term @label ?label} OPTIONAL {?term @comment ?comment} }";
                queryString.SetParameter("class", rdfsClass);
                queryString.SetParameter("property", rdfProperty);
                queryString.SetParameter("datatype", rdfsDatatype);
                queryString.SetParameter("label", rdfsLabel);
                queryString.SetParameter("comment", rdfsComment);

                Object results = g.ExecuteQuery(queryString.ToString());
                if (results is SparqlResultSet)
                {
                    foreach (SparqlResult r in ((SparqlResultSet)results))
                    {
                        termUri = r["term"].ToString();
                        if (termUri.StartsWith(namespaceUri))
                        {
                            //Use the Comment as the label if available
                            if (r.HasValue("RawComment"))
                            {
                                if (r["RawComment"] != null)
                                {
                                    terms.Add(new NamespaceTerm(namespaceUri, termUri.Substring(namespaceUri.Length), r["RawComment"].ToString()));
                                    continue;
                                }
                            }

                            //Use the Label as the label if available
                            if (r.HasValue("RawLabel"))
                            {
                                if (r["RawLabel"] != null)
                                {
                                    terms.Add(new NamespaceTerm(namespaceUri, termUri.Substring(namespaceUri.Length), r["RawLabel"].ToString()));
                                    continue;
                                }
                            }

                            //Otherwise no label
                            terms.Add(new NamespaceTerm(namespaceUri, termUri.Substring(namespaceUri.Length)));
                        }
                    }
                }

                lock (_terms)
                {
                    terms.RemoveAll(t => _terms.Contains(t));
                    _terms.AddRange(terms.Distinct());
                }
            }
            catch (Exception ex)
            {
                //Ignore Exceptions - just means we won't have those namespace terms available
            }
            try
            {
                _loadedNamespaces.Add(namespaceUri);
            }
            catch (NullReferenceException)
            {
                //For some reason .Net sometimes throws a NullReferenceException here which we shall ignore
            }
            return GetNamespaceTerms(namespaceUri);
        }

        public static IEnumerable<NamespaceTerm> GetNamespaceTerms(String namespaceUri)
        {
            return (from t in _terms
                    where t.NamespaceUri.Equals(namespaceUri, StringComparison.Ordinal)
                    select t);
        }

        public static String GetDefaultPrefix(String namespaceUri)
        {
            foreach (VocabularyDefinition vocab in _builtInVocabs)
            {
                if (vocab.NamespaceUri.Equals(namespaceUri, StringComparison.Ordinal)) return vocab.Prefix;
            }
            return String.Empty;
        }
    }
}
