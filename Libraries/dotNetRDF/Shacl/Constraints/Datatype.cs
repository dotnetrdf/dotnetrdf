/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Shacl.Constraints
{
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using VDS.RDF.Parsing;
    using VDS.RDF.Query;
    using VDS.RDF.Shacl.Validation;

    internal class Datatype : Constraint
    {
        [DebuggerStepThrough]
        internal Datatype(Shape shape, INode node)
            : base(shape, node)
        {
        }

        internal override INode ConstraintComponent
        {
            get
            {
                return Vocabulary.DatatypeConstraintComponent;
            }
        }

        private Uri DataTypeParameter
        {
            get
            {
                return ((IUriNode)this).Uri;
            }
        }

        internal override bool Validate(INode focusNode, IEnumerable<INode> valueNodes, Report report)
        {
            var invalidValues =
                from valueNode in valueNodes
                where IsInvalid(valueNode)
                select valueNode;

            return ReportValueNodes(focusNode, invalidValues, report);
        }

        private bool IsInvalid(INode n)
        {
            if (n.NodeType != NodeType.Literal)
            {
                return true;
            }

            var literal = (ILiteralNode)n;
            var xsd_string = UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString);

            var rdf_langString = UriFactory.Create(RdfSpecsHelper.RdfLangString);
            var stringDatatype = string.IsNullOrEmpty(literal.Language) ? xsd_string : rdf_langString;
            var datatype = literal.DataType ?? stringDatatype;

            if (!EqualityHelper.AreUrisEqual(datatype, DataTypeParameter))
            {
                return true;
            }

            if (literal.DataType is null)
            {
                return false;
            }

            var supportedDatatypes = SparqlSpecsHelper.SupportedCastFunctions.Union(SparqlSpecsHelper.IntegerDataTypes);

            if (!supportedDatatypes.Contains(literal.DataType.AbsoluteUri))
            {
                return false;
            }

            return IsIllformed(literal);
        }

        private const string Root = "root";

        private static bool IsIllformed(ILiteralNode literal)
        {
            var type = literal.DataType.AbsoluteUri.Replace(XmlSpecsHelper.NamespaceXmlSchema, string.Empty);
            var schemas = GenerateSchema(type);

            var isIllformed = false;
            void handler(object sender, ValidationEventArgs e)
            {
                isIllformed = true;
            }

            var doc = new XDocument(new XElement(Root, literal.Value));
            doc.Validate(schemas, handler);

            return isIllformed;
        }

        // Equivalent to the following:
        // <xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema">
        //    <xsd:element name="{Root}" type="xsd:{type}"/>
        // </schema>
        private static XmlSchemaSet GenerateSchema(string type)
        {
            var schemas = new XmlSchemaSet();
            var schema = new XmlSchema();

            schema.Items.Add(
                new XmlSchemaElement
                {
                    Name = Root,
                    SchemaTypeName = new XmlQualifiedName(type, XmlSchema.Namespace),
                });

            schemas.Add(schema);

            return schemas;
        }
    }
}