using System.Collections.Generic;
using Antlr.StringTemplate;

namespace VDS.RDF.Utilities.Linq.Metal
{
    public class CodeGenerator
    {
		EmbeddedResourceTemplateLoader templateLoader;
		StringTemplateGroup stringTemplateGroup;
		private static readonly string TemplateNamespace = "VDS.RDF.Utilities.Linq.Metal.template";
		
//		private readonly StringTemplateGroup group = new StringTemplateGroup("myGroup", @"C:\shared.datastore\repository\personal\dev\projects\semantic-web\LinqToRdf.Prototypes\RdfMetal\template");
		
		public CodeGenerator()
		{
			templateLoader = new EmbeddedResourceTemplateLoader(GetType().Assembly, TemplateNamespace);
			stringTemplateGroup = new StringTemplateGroup("rdfMetal", templateLoader);
		}

        public string Generate(IEnumerable<OntologyClass> classes, Options opts)
        {
			StringTemplate template = stringTemplateGroup.GetInstanceOf("classes");
            template.SetAttribute("classes", classes);
            template.SetAttribute("handle", opts.OntologyPrefix);
            template.SetAttribute("uri", opts.OntologyNamespace);
            template.SetAttribute("opts", opts);
            template.SetAttribute("refs", opts.DotNetNamespaceReferences.Split(','));
            return template.ToString();
        }
    }
}