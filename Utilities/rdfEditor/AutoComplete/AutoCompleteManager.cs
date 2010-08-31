using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using rdfEditor.Syntax;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;

namespace rdfEditor.AutoComplete
{
    public class AutoCompleteManager
    {
        private static bool _init = false;

        private static List<AutoCompleteDefinition> _builtinCompleters = new List<AutoCompleteDefinition>()
        {
            new AutoCompleteDefinition("Turtle", new TurtleAutoCompleter()),
            new AutoCompleteDefinition("Notation3", new TurtleAutoCompleter()),
            new AutoCompleteDefinition("SparqlQuery10", new SparqlAutoCompleter(SparqlQuerySyntax.Sparql_1_0))
        };

        private static List<ICompletionData> _builtinPrefixes = new List<ICompletionData>()
        {
            //Standard Prefixes
            new PrefixCompletionData("rdf", NamespaceMapper.RDF, "RDF Namespace"),
            new PrefixCompletionData("rdfs", NamespaceMapper.RDFS, "RDF Schema Namespace"),
            new PrefixCompletionData("owl", NamespaceMapper.OWL, "OWL Namespace"),
            new PrefixCompletionData("xsd", NamespaceMapper.XMLSCHEMA, "XML Schema Datatypes Namespace"),

            //Common Prefixes
            new PrefixCompletionData("dc", "http://purl.org/dc/elements/1.1/", "Dublin Core Elements Namespace"),
            new PrefixCompletionData("dcterms", "http://www.purl.org/dc/terms/", "Dublin Core Terms Namespace"),
            new PrefixCompletionData("foaf", "http://xmlns.com/foaf/0.1/", "Friend of a Friend Namespace for describing social networks"),
            new PrefixCompletionData("vcard", "http://www.w3.org/2006/vcard/ns#", "VCard Namespace"),
            new PrefixCompletionData("gr", "http://purl.org/goodrelations/v1#", "Good Relations Namespace for describing eCommerce"),

            //Useful SPARQL Prefixes
            new PrefixCompletionData("fn", XPathFunctionFactory.XPathFunctionsNamespace, "XPath Functions Namespace used to refer to functions by URI in SPARQL queries"),
            new PrefixCompletionData("lfn", LeviathanFunctionFactory.LeviathanFunctionsNamespace, "Leviathan Functions Namespace used to refer to functions by URI in SPARQL queries"),
            new PrefixCompletionData("afn", ArqFunctionFactory.ArqFunctionsNamespace, "ARQ Functions Namespace used to refer to functions by URI in SPARQL queries"),

            //Other dotNetRDF Prefixes
            new PrefixCompletionData("dnr", ConfigurationLoader.ConfigurationNamespace, "dotNetRDF Configuration Namespace for specifying configuration files")
        };

        private static List<NamespaceTerm> _terms = new List<NamespaceTerm>();
        private static HashSet<String> _loadedNamespaces = new HashSet<string>();
        private static LoadNamespaceTermsDelegate _namespaceLoader = new LoadNamespaceTermsDelegate(AutoCompleteManager.LoadNamespaceTerms);

        public static void Initialise()
        {
            if (_init) return;

            //Have to intialise the Syntax Manager first
            SyntaxManager.Initialise();

            //Then register the auto-completers
            foreach (AutoCompleteDefinition def in _builtinCompleters)
            {
                SyntaxDefinition syntaxDef = SyntaxManager.GetDefinition(def.Name);
                if (syntaxDef != null)
                {
                    syntaxDef.AutoCompleter = def.AutoCompleter;
                }
            }

            //Then start lazy loading Namespace Terms
            foreach (PrefixCompletionData prefix in _builtinPrefixes.OfType<PrefixCompletionData>())
            {
                _namespaceLoader.BeginInvoke(prefix.NamespaceUri, InitialiseNamepaceTerms, null);
            }

            _init = true;
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

        public static IEnumerable<ICompletionData> PrefixData
        {
            get
            {
                return _builtinPrefixes;                       
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
                UriLoader.Load(g, new Uri(namespaceUri));

                UriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                UriNode rdfsClass = g.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class"));
                UriNode rdfProperty = g.CreateUriNode(new Uri(NamespaceMapper.RDF + "Property"));

                List<NamespaceTerm> terms = new List<NamespaceTerm>();
                String termUri;
                foreach (Triple t in g.GetTriplesWithPredicateObject(rdfType, rdfsClass))
                {
                    if (t.Subject.NodeType == NodeType.Uri)
                    {
                        termUri = t.Subject.ToString();
                        if (termUri.StartsWith(namespaceUri))
                        {
                            _terms.Add(new NamespaceTerm(namespaceUri, termUri.Substring(namespaceUri.Length)));
                        }
                    }
                }
                foreach (Triple t in g.GetTriplesWithPredicateObject(rdfType, rdfProperty))
                {
                    if (t.Subject.NodeType == NodeType.Uri)
                    {
                        termUri = t.Subject.ToString();
                        if (termUri.StartsWith(namespaceUri))
                        {
                            _terms.Add(new NamespaceTerm(namespaceUri, termUri.Substring(namespaceUri.Length)));
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
                //If an exception happens then we can't get namespace terms for whatever reason
                //We still count the namespace as loaded so we don't try this again
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            _loadedNamespaces.Add(namespaceUri);
            return GetNamespaceTerms(namespaceUri);
        }

        public static IEnumerable<NamespaceTerm> GetNamespaceTerms(String namespaceUri)
        {
            return (from t in _terms
                    where t.NamespaceUri.Equals(namespaceUri, StringComparison.OrdinalIgnoreCase)
                    select t);
        }
    }
}
