namespace VDS.RDF
{
    using System;

    [Serializable]
    public class MockWrapperNode : WrapperNode
    {
        public MockWrapperNode(INode node) : base(node) { }
    }
}