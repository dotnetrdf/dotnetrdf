using System;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// todo: extract interface?
    /// </summary>
    public sealed class TriplePatternObjectPart
    {
        public ITriplePatternBuilder Object(string variableName)
        {
            throw new System.NotImplementedException();
        }

        public ITriplePatternBuilder Object<TNode>(string @object) where TNode : INode
        {
            throw new System.NotImplementedException();
        }

        public ITriplePatternBuilder Object(INode objectNode)
        {
            throw new NotImplementedException();
        }
    }
}