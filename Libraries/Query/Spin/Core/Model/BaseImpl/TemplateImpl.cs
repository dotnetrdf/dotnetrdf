using System;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{

    public class TemplateImpl : ModuleImpl, ITemplateResource
    {

        public TemplateImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {

        }


        public String getLabelTemplate()
        {
            return GetString(SPIN.PropertyLabelTemplate);
        }
    }
}