using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test.Core
{
    [TestClass]
    public class UriTests
    {
        [TestMethod]
        public void UriAbsoluteUriWithQuerystring()
        {
            Uri u = new Uri("http://example.org/?test");
            Assert.AreEqual("http://example.org/?test", u.AbsoluteUri);
        }

        [TestMethod]
        public void UriAbsoluteUriWithFragment()
        {
            Uri u = new Uri("http://example.org/#test");
            Assert.AreEqual("http://example.org/#test", u.AbsoluteUri);
        }
    }
}
