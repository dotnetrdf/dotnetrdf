using System;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    internal class PatternItemFactory
    {
        private readonly NodeFactory _nodeFactory = new NodeFactory();
        private readonly INamespaceMapper _namespaceMapper;

        public PatternItemFactory(INamespaceMapper namespaceMapper)
        {
            _namespaceMapper = namespaceMapper;
        }

        internal PatternItem CreateVariablePattern(string variableName)
        {
            return new VariablePattern(variableName);
        }

        internal PatternItem CreateNodeMatchPattern(string qName)
        {
            var qNameResolved = Tools.ResolveQName(qName, _namespaceMapper, null);
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

        public PatternItem CreatePatternItem(Type nodeType, string patternString)
        {
            if(nodeType == typeof(IUriNode))
            {
                return CreateNodeMatchPattern(patternString);
            }
            if(nodeType == typeof(IBlankNode))
            {
                return CreateBlankNodeMatchPattern(patternString);
            }

            throw new ArgumentException(string.Format("Invalid node type {0}", nodeType));
        }
    }
}