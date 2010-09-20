using System.Collections.Generic;
using Antlr.StringTemplate;

namespace rdfMetal
{
    public class CodeGenerator
    {
		EmbeddedResourceTemplateLoader templateLoader;
		StringTemplateGroup stringTemplateGroup;
		private static readonly string TemplateNamespace = "rdfMetal.template";
		
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
            template.SetAttribute("handle", opts.ontologyPrefix);
            template.SetAttribute("uri", opts.ontologyNamespace);
            template.SetAttribute("opts", opts);
            template.SetAttribute("refs", opts.namespaceReferences.Split(','));
            return template.ToString();
        }
    }
}