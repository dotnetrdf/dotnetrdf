using System;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Class responsible for creating <see cref="PatternItem"/>s
    /// </summary>
    internal class PatternItemFactory
    {
        private readonly NodeFactory _nodeFactory = new NodeFactory();

        internal PatternItem CreateVariablePattern(string variableName)
        {
            return new VariablePattern(variableName);
        }

        internal PatternItem CreateNodeMatchPattern(string qName, INamespaceMapper namespaceMapper)
        {
            var qNameResolved = Tools.ResolveQName(qName, namespaceMapper, null);
            return CreateNodeMatchPattern(new Uri(qNameResolved));
        }

        internal PatternItem CreateNodeMatchPattern(Uri uri)
        {
            return CreateNodeMatchPattern(_nodeFactory.CreateUriNode(uri));
        }

        internal PatternItem CreateNodeMatchPattern(INode node)
        {
            return new NodeMatchPattern(node);
        }

        private PatternItem CreateBlankNodeMatchPattern(string blankNodeIdentifier)
        {
            return new BlankNodePattern(blankNodeIdentifier);
        }

        internal PatternItem CreatePatternItem(Type nodeType, string patternString, INamespaceMapper namespaceMapper)
        {
            if (nodeType == typeof(IUriNode))
            {
                return CreateNodeMatchPattern(patternString, namespaceMapper);
            }
            if (nodeType == typeof(IBlankNode))
            {
                return CreateBlankNodeMatchPattern(patternString);
            }
            if (nodeType == typeof(ILiteralNode))
            {
                return CreateLiteralNodeMatchPattern(patternString);
            }
            if(nodeType == typeof(IVariableNode))
            {
                return CreateVariablePattern(patternString);
            }

            throw new ArgumentException(string.Format("Invalid node type {0}", nodeType));
        }

        internal PatternItem CreateLiteralNodeMatchPattern(object literal)
        {
            var literalString = GetLiteralString(literal);

            return new NodeMatchPattern(_nodeFactory.CreateLiteralNode(literalString));
        }

        internal PatternItem CreateLiteralNodeMatchPattern(object literal, Uri datatype)
        {
            var literalString = GetLiteralString(literal);

            return new NodeMatchPattern(_nodeFactory.CreateLiteralNode(literalString, datatype));
        }

        internal PatternItem CreateLiteralNodeMatchPattern(object literal, string langSpec)
        {
            var literalString = GetLiteralString(literal);

            return new NodeMatchPattern(_nodeFactory.CreateLiteralNode(literalString, langSpec));
        }

        private static string GetLiteralString(object literal)
        {
            string literalString = literal.ToString();
            if (literal is DateTimeOffset)
            {
                literalString = GetDatetimeString((DateTimeOffset) literal);
            }
            else if (literal is DateTime)
            {
                literalString = GetDatetimeString((DateTime) literal);
            }
            return literalString;
        }

        private static string GetDatetimeString(DateTimeOffset literal)
        {
            var datetimeString = literal.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
            return datetimeString;
        }

        private static string GetDatetimeString(DateTime literal)
        {
            var datetimeString = literal.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat);
            return datetimeString;
        }
    }
}