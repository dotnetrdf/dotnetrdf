/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;

namespace VDS.RDF.Test.Configuration
{
    [TestClass]
    public class DefaultTypeTests
    {
        private void TestDefaultType(String typeUri, String expectedType)
        {
            String actualType = ConfigurationLoader.GetDefaultType(typeUri);
            Assert.AreEqual(expectedType, actualType);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeGraph()
        {
            this.TestDefaultType(ConfigurationLoader.ClassGraph, ConfigurationLoader.DefaultTypeGraph);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeGraphCollection()
        {
            this.TestDefaultType(ConfigurationLoader.ClassGraphCollection, ConfigurationLoader.DefaultTypeGraphCollection);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeSparqlHttpProtocolProcessor()
        {
            this.TestDefaultType(ConfigurationLoader.ClassSparqlHttpProtocolProcessor, ConfigurationLoader.DefaultTypeSparqlHttpProtocolProcessor);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeSparqlQueryProcessor()
        {
            this.TestDefaultType(ConfigurationLoader.ClassSparqlQueryProcessor, ConfigurationLoader.DefaultTypeSparqlQueryProcessor);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeSparqlUpdateProcessor()
        {
            this.TestDefaultType(ConfigurationLoader.ClassSparqlUpdateProcessor, ConfigurationLoader.DefaultTypeSparqlUpdateProcessor);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeTripleCollection()
        {
            this.TestDefaultType(ConfigurationLoader.ClassTripleCollection, ConfigurationLoader.DefaultTypeTripleCollection);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeTripleStore()
        {
            this.TestDefaultType(ConfigurationLoader.ClassTripleStore, ConfigurationLoader.DefaultTypeTripleStore);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeUser()
        {
            this.TestDefaultType(ConfigurationLoader.ClassUser, typeof(System.Net.NetworkCredential).AssemblyQualifiedName);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeUserGroup()
        {
            this.TestDefaultType(ConfigurationLoader.ClassUserGroup, ConfigurationLoader.DefaultTypeUserGroup);
        }

        [TestMethod]
        public void ConfigurationDefaultTypeProxy()
        {
            this.TestDefaultType(ConfigurationLoader.ClassProxy, typeof(System.Net.WebProxy).AssemblyQualifiedName);
        }
    }
}
