using VDS.RDF.Query.Spin.Core;
using VDS.RDF.Query.Spin.OntologyHelpers;

namespace VDS.RDF.Query.Spin.Model
{

    public class AttributeImpl : AbstractAttributeImpl, IAttributeResource
    {

        public AttributeImpl(INode node, SpinProcessor spinModel)
            : base(node, spinModel)
        {
        }


        public bool IsOptional()
        {
            return getMinCount() == 0;
        }


        public INode getDefaultValue()
        {
            return GetObject(SPL.PropertyDefaultValue);
        }


        public int? getMaxCount()
        {
            int? value = (int?)GetLong(SPL.PropertyMaxCount);
            if (value != null)
            {
                return value;
            }
            else
            {
                return null;
            }
        }


        public int getMinCount()
        {
            int? value = (int?)GetLong(SPL.PropertyMaxCount);
            if (value != null)
            {
                return (int)value;
            }
            else
            {
                return 0;
            }
        }
    }
}