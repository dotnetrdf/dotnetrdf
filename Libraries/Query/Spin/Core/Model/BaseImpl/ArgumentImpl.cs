using System;
using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{
    public class ArgumentImpl : AbstractAttributeImpl, IArgumentResource
    {

        public ArgumentImpl(INode node, SpinModel spinModel)
            : base(node, spinModel)
        {
        }


        public int? getArgIndex()
        {
            String varName = getVarName();
            if (varName != null)
            {
                return SP.getArgPropertyIndex(varName);
            }
            else
            {
                return null;
            }
        }


        public INode getDefaultValue()
        {
            return GetObject(SPL.PropertyDefaultValue);
        }


        public String getVarName()
        {
            IResource argProperty = getPredicate();
            if (argProperty != null)
            {
                return argProperty.Uri.ToString().Replace(SP.NS_URI, "");
            }
            else
            {
                return null;
            }
        }


        public bool IsOptional()
        {
            return (bool)GetBoolean(SPL.PropertyOptional);
        }
    }
}