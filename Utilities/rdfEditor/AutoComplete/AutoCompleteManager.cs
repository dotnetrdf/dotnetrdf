using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Query.Expressions;
using rdfEditor.Syntax;

namespace rdfEditor.AutoComplete
{
    public enum AutoCompleteState
    {
        Disabled,
        None,
        QName,
        Prefix,
        URI,
        Keyword,
        KeywordOrQName,
        Inserted,
        BNode
    }

    public class AutoCompleteManager
    {
        private static bool _init = false;

        private static List<AutoCompleteDefinition> _builtinCompleters = new List<AutoCompleteDefinition>()
        {
            new AutoCompleteDefinition("Turtle", new TurtleAutoCompleter()),
            new AutoCompleteDefinition("Notation3", new TurtleAutoCompleter())
        };

        private static List<ICompletionData> _builtinPrefixes = new List<ICompletionData>()
        {
            //Standard Prefixes
            new PrefixCompletionData("rdf", NamespaceMapper.RDF, "RDF Namespace"),
            new PrefixCompletionData("rdfs", NamespaceMapper.RDFS, "RDF Schema Namespace"),
            new PrefixCompletionData("owl", NamespaceMapper.OWL, "OWL Namespace"),
            new PrefixCompletionData("xsd", NamespaceMapper.XMLSCHEMA, "XML Schema Datatypes Namespace"),

            //Common Prefixes
            new PrefixCompletionData("dct", "http://purl.org/dc/elements/1.1/", "Dublin Core Elements Namespace"),
            new PrefixCompletionData("dct", "http://www.purl.org/dc/terms/", "Dublin Core Terms Namespace"),
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

            _init = true;
        }

        public static IEnumerable<ICompletionData> PrefixData
        {
            get
            {
                return _builtinPrefixes;                       
            }
        }
    }
}
