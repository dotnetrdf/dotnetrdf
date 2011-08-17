using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText.Schema
{
    public interface IFullTextIndexSchema
    {
        String IndexField
        {
            get;
        }

        String HashField
        {
            get;
        }

        String NodeTypeField
        {
            get;
        }

        String NodeValueField
        {
            get;
        }

        String NodeMetaField
        {
            get;
        }
    }
}
