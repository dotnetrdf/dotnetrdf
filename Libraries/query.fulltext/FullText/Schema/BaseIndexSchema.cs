using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText.Schema
{
    public abstract class BaseIndexSchema
        : IFullTextIndexSchema
    {
        public string IndexField
        {
            get;
            protected set;
        }

        public String HashField
        {
            get;
            protected set;
        }

        public string NodeTypeField
        {
            get;
            protected set;
        }

        public string NodeValueField
        {
            get;
            protected set;
        }

        public string NodeMetaField
        {
            get;
            protected set;
        }
    }
}
