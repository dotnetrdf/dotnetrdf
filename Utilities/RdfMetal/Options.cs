using Opts = Mono.GetOptions.Options;
using Opt = Mono.GetOptions.OptionAttribute;

namespace rdfMetal
{
    public class Options : Opts
    {
        [Opt("The SPARQL endpoint to query.", 'e', "endpoint")]
        public string endpoint = "";

        [Opt("The Default Graph of the SPARQL Endpoint", 'd')]
        public string defaultGraphUri = "";

        [Opt("An RDF File to query (Ignored if -e or -endpoint is used)", 'f')]
        public string sourceFile = "";

        [Opt("The XML Namespace to extract classes from.", 'n', "namespace")]
        public string ontologyNamespace = "";

        [Opt("Where to place the generated code.", 'o', "output")]
        public string sourceLocation = "DomainModel.cs";

        [Opt("Where to place/get the collected metadata.", 'm', "metadata")]
        public string metadataLocation = "";

        [Opt("Ignore BNodes. Use this if you only want to generate code for named classes.", 'i', "ignorebnodes")]
        public bool ignoreBlankNodes = false;

        [Opt("The ontology name to be used in LinqToRdf for prefixing URIs and disambiguating class names and properties.", 'h', "handle")]
        public string ontologyPrefix = "MyOntology";

        [Opt("The .NET namespace to place the generated source in.", 'N', "netnamespace")]
        public string dotnetNamespace = "MyOntology";

        [Opt("A comma separated list of namespaces to reference in the generated source.", 'r', "references")]
        public string namespaceReferences = "System";

        public Options()
        {
            base.ParsingMode = Mono.GetOptions.OptionsParsingMode.Both;
        }

    }
}