using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.FullText.Schema
{
    public class DefaultIndexSchema
        : BaseIndexSchema
    {
        public const String DefaultIndexField = "nodeIndex",
                            DefaultHashField = "nodeIndexHash",
                            DefaultNodeTypeField = "nodeType",
                            DefaultNodeValueField = "nodeValue",
                            DefaultNodeMetaField = "nodeMeta";

        public DefaultIndexSchema()
        {
            this.IndexField = DefaultIndexField;
            this.HashField = DefaultHashField;
            this.NodeMetaField = DefaultNodeMetaField;
            this.NodeTypeField = DefaultNodeTypeField;
            this.NodeValueField = DefaultNodeValueField;
        }
    }
}
