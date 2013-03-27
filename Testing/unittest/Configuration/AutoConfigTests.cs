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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.DateTime;

namespace VDS.RDF.Configuration
{
    [TestClass]
    public class AutoConfigTests
    {
        [TestMethod]
        public void ConfigurationStaticOptionUri1()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#UsePLinqEvaluation");

            Assert.AreEqual("dotnetrdf-configure", optionUri.Scheme);
            Assert.AreEqual(1, optionUri.Segments.Length);
            Assert.AreEqual("VDS.RDF.Options", optionUri.Segments[0]);
            Assert.AreEqual("VDS.RDF.Options", optionUri.AbsolutePath);
            Assert.AreEqual("UsePLinqEvaluation", optionUri.Fragment.Substring(1));
        }

        [TestMethod]
        public void ConfigurationStaticOptionUri2()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options,SomeAssembly#UsePLinqEvaluation");

            Assert.AreEqual("dotnetrdf-configure", optionUri.Scheme);
            Assert.AreEqual(1, optionUri.Segments.Length);
            Assert.AreEqual("VDS.RDF.Options,SomeAssembly", optionUri.Segments[0]);
            Assert.AreEqual("VDS.RDF.Options,SomeAssembly", optionUri.AbsolutePath);
            Assert.AreEqual("UsePLinqEvaluation", optionUri.Fragment.Substring(1));
        }

        private void ApplyStaticOptionsConfigure(Uri option, String value)
        {
            Graph g = new Graph();
            INode configure = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyConfigure));
            g.Assert(g.CreateUriNode(option), configure, g.CreateLiteralNode(value));
            this.ApplyStaticOptionsConfigure(g);
        }

        private void ApplyStaticOptionsConfigure(IGraph g, Uri option, INode value)
        {
            INode configure = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyConfigure));
            g.Assert(g.CreateUriNode(option), configure, value);
            this.ApplyStaticOptionsConfigure(g);
        }

        private void ApplyStaticOptionsConfigure(IGraph g)
        {
            ConfigurationLoader.AutoConfigureStaticOptions(g);
        }

        [TestMethod, ExpectedException(typeof(DotNetRdfConfigurationException))]
        public void ConfigurationStaticOptionsNoFragment()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph");
            this.ApplyStaticOptionsConfigure(optionUri, "");
        }

        [TestMethod, ExpectedException(typeof(DotNetRdfConfigurationException))]
        public void ConfigurationStaticOptionsBadClass()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.NoSuchClass#Property");
            this.ApplyStaticOptionsConfigure(optionUri, "");
        }

        [TestMethod, ExpectedException(typeof(DotNetRdfConfigurationException))]
        public void ConfigurationStaticOptionsBadProperty()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph#NoSuchProperty");
            this.ApplyStaticOptionsConfigure(optionUri, "");
        }

        [TestMethod, ExpectedException(typeof(DotNetRdfConfigurationException))]
        public void ConfigurationStaticOptionsNonStaticProperty()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph#BaseUri");
            this.ApplyStaticOptionsConfigure(optionUri, "http://example.org");
        }

        [TestMethod]
        public void ConfigurationStaticOptionsEnumProperty()
        {
            LiteralEqualityMode current = Options.LiteralEqualityMode;
            try
            {
                Assert.AreEqual(current, Options.LiteralEqualityMode);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#LiteralEqualityMode");
                this.ApplyStaticOptionsConfigure(optionUri, "Loose");

                Assert.AreEqual(LiteralEqualityMode.Loose, Options.LiteralEqualityMode);
            }
            finally
            {
                Options.LiteralEqualityMode = current;
            }
        }

        [TestMethod]
        public void ConfigurationStaticOptionsInt32Property()
        {
            int current = Options.UriLoaderTimeout;
            try
            {
                Assert.AreEqual(current, Options.UriLoaderTimeout);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#UriLoaderTimeout");
                Graph g = new Graph();
                this.ApplyStaticOptionsConfigure(g, optionUri, (99).ToLiteral(g));

                Assert.AreEqual(99, Options.UriLoaderTimeout);
            }
            finally
            {
                Options.UriLoaderTimeout = current;
            }
        }

        [TestMethod]
        public void ConfigurationStaticOptionsInt64Property()
        {
            long current = Options.QueryExecutionTimeout;
            try
            {
                Assert.AreEqual(current, Options.QueryExecutionTimeout);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#QueryExecutionTimeout");
                Graph g = new Graph();
                this.ApplyStaticOptionsConfigure(g, optionUri, (99).ToLiteral(g));

                Assert.AreEqual(99, Options.QueryExecutionTimeout);
            }
            finally
            {
                Options.QueryExecutionTimeout = current;
            }
        }

#if NET40 && !SILVERLIGHT
        [TestMethod]
        public void ConfigurationStaticOptionsBooleanProperty()
        {
            bool current = Options.UsePLinqEvaluation;
            try
            {
                Assert.AreEqual(current, Options.UsePLinqEvaluation);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#UsePLinqEvaluation");
                Graph g = new Graph();
                this.ApplyStaticOptionsConfigure(g, optionUri, (false).ToLiteral(g));

                Assert.IsFalse(Options.UsePLinqEvaluation);
            }
            finally
            {
                Options.UsePLinqEvaluation = current;
            }
        }
#endif

        [TestMethod]
        public void ConfigurationAutoOperators1()
        {
            try
            {
                String data = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:SparqlOperator ;
dnr:type """ + typeof(MockSparqlOperator).AssemblyQualifiedName + @""" .";

                Graph g = new Graph();
                g.LoadFromString(data);

                ConfigurationLoader.AutoConfigureSparqlOperators(g);

                ISparqlOperator op;
                SparqlOperators.TryGetOperator(SparqlOperatorType.Add, out op, null);

                Assert.AreEqual(typeof(MockSparqlOperator), op.GetType());
                SparqlOperators.RemoveOperator(op);
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }

        [TestMethod]
        public void ConfigurationAutoOperators2()
        {
            try
            {
                String data = @"@prefix dnr: <http://www.dotnetrdf.org/configuration#> .
_:a a dnr:SparqlOperator ;
dnr:type ""VDS.RDF.Query.Operators.DateTime.DateTimeAddition"" ;
dnr:enabled false .";

                Graph g = new Graph();
                g.LoadFromString(data);

                ConfigurationLoader.AutoConfigureSparqlOperators(g);

                Assert.IsFalse(SparqlOperators.IsRegistered(new DateTimeAddition()));
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }
    }

    public class MockSparqlOperator
        : ISparqlOperator
    {

        #region ISparqlOperator Members

        public SparqlOperatorType Operator
        {
            get { return SparqlOperatorType.Add; }
        }

        public bool IsApplicable(params IValuedNode[] ns)
        {
            return true;
        }

        public IValuedNode Apply(params Nodes.IValuedNode[] ns)
        {
            return null;
        }

        #endregion
    }
}
