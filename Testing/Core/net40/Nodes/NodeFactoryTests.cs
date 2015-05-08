using NUnit.Framework;

namespace VDS.RDF.Nodes
{
    [TestFixture]
    public class NodeFactoryTests
        : AbstractNodeFactoryContractTests
    {
        protected override INodeFactory CreateFactoryInstance()
        {
            return new NodeFactory();
        }
    }
}