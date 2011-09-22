using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.FullText.Schema
{
    public class DefaultIndexSchema
        : BaseIndexSchema, IConfigurationSerializable
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

        #region IConfigurationSerializable Members

        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode schemaObj = context.NextSubject;
            context.Graph.Assert(schemaObj, context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), context.Graph.CreateUriNode(new Uri(FullTextHelper.ClassSchema)));
            context.Graph.Assert(schemaObj, ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType), context.Graph.CreateLiteralNode(this.GetType().Name + ", dotNetRDF.Query.FullText"));
        }

        #endregion
    }
}
