/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;

namespace VDS.RDF.Query.FullText.Schema
{
    /// <summary>
    /// Default Index Schema
    /// </summary>
    public class DefaultIndexSchema
        : BaseIndexSchema, IConfigurationSerializable
    {
        /// <summary>
        /// Constants for the Field Names used by the Default Index Schema
        /// </summary>
        public const String DefaultIndexField = "nodeIndex",
                            DefaultGraphField = "nodeGraph",
                            DefaultHashField = "nodeIndexHash",
                            DefaultNodeTypeField = "nodeType",
                            DefaultNodeValueField = "nodeValue",
                            DefaultNodeMetaField = "nodeMeta";

        /// <summary>
        /// Creates a new Default Index Schema
        /// </summary>
        public DefaultIndexSchema()
        {
            this.IndexField = DefaultIndexField;
            this.GraphField = DefaultGraphField;
            this.HashField = DefaultHashField;
            this.NodeMetaField = DefaultNodeMetaField;
            this.NodeTypeField = DefaultNodeTypeField;
            this.NodeValueField = DefaultNodeValueField;
        }

        /// <summary>
        /// Serializes the Schemas Configuration
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            context.EnsureObjectFactory(typeof(FullTextObjectFactory));

            INode schemaObj = context.NextSubject;
            context.Graph.Assert(schemaObj, context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.ClassSchema)));
            context.Graph.Assert(schemaObj, context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType)), context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Query.FullText"));
        }
    }
}
