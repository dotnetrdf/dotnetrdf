using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test.Core
{
    [TestClass]
    public class LiteralNodeTests
    {
        [TestMethod]
        public void ShouldCreateLiteralNodeByCultureInvariantToString()
        {
            // given
            INodeFactory nodeFactory = new NodeFactory();

            // when
            Thread.CurrentThread.CurrentCulture = new CultureInfo("pl");

            // then
            Assert.AreEqual("5.5", 5.5.ToLiteral(nodeFactory).Value);
            Assert.AreEqual("7.5", 7.5f.ToLiteral(nodeFactory).Value);
            Assert.AreEqual("15.5", 15.5m.ToLiteral(nodeFactory).Value);

            // when
            CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            // Make a writable clone
            culture = (CultureInfo)culture.Clone();
            culture.NumberFormat.NegativeSign = "!";
            Thread.CurrentThread.CurrentCulture = culture;

            // then
            Assert.AreEqual("-1", (-1).ToLiteral(nodeFactory).Value);
            Assert.AreEqual("-1", ((short)-1).ToLiteral(nodeFactory).Value);
            Assert.AreEqual("-1", ((long)-1).ToLiteral(nodeFactory).Value);
        }
    }
}