using Opts = Mono.GetOptions.Options;
using Opt = Mono.GetOptions.OptionAttribute;

namespace rdfMetal
{
    public class Options : Opts
    {
        [Opt("The SPARQL endpoint to query.", 'e', "endpoint")]
        public string EndpointUri = "";

        [Opt("The Default Graph of the SPARQL Endpoint", 'd')]
        public string DefaultGraphUri = "";

        [Opt("An RDF File to query (Ignored if -e or -endpoint is used)", 'f')]
        public string SourceFile = "";

        [Opt("The XML Namespace to extract classes from.", 'n', "namespace")]
        public string OntologyNamespace = "";

        [Opt("Where to place the generated code.", 'o', "output")]
        public string GeneratedSourceLocation = "DomainModel.cs";

        [Opt("Where to place/get the collected metadata.", 'm', "metadata")]
        public string GeneratedMetadataLocation = "";

        [Opt("Ignore BNodes. Use this if you only want to generate code for named classes.", 'i', "ignorebnodes")]
        public bool IgnoreBlankNodes = false;

        [Opt("Extract RDFS classes instead of OWL Classes", 'r', "rdfs")]
        public bool ExtractRdfsClasses = false;

        [Opt("The ontology name to be used in LinqToRdf for prefixing URIs and disambiguating class names and properties.", 'h', "handle")]
        public string OntologyPrefix = "MyOntology";

        [Opt("The .NET namespace to place the generated source in.", 'N', "netnamespace")]
        public string DotNetNamespace = "MyOntology";

        [Opt("A comma separated list of namespaces to reference in the generated source.", 'r', "references")]
        public string DotNetNamespaceReferences = "System";

        public Options()
        {
            base.ParsingMode = Mono.GetOptions.OptionsParsingMode.Both;
        }

    }
}