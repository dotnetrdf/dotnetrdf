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
using Xunit;
using VDS.RDF.Configuration;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Operators;
using VDS.RDF.Query.Operators.DateTime;

namespace VDS.RDF.Configuration
{

    public class AutoConfigTests
    {
        [Fact]
        public void ConfigurationStaticOptionUri1()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#UsePLinqEvaluation");

            Assert.Equal("dotnetrdf-configure", optionUri.Scheme);
            Assert.Single(optionUri.Segments);
            Assert.Equal("VDS.RDF.Options", optionUri.Segments[0]);
            Assert.Equal("VDS.RDF.Options", optionUri.AbsolutePath);
            Assert.Equal("UsePLinqEvaluation", optionUri.Fragment.Substring(1));
        }

        [Fact]
        public void ConfigurationStaticOptionUri2()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options,SomeAssembly#UsePLinqEvaluation");

            Assert.Equal("dotnetrdf-configure", optionUri.Scheme);
            Assert.Single(optionUri.Segments);
            Assert.Equal("VDS.RDF.Options,SomeAssembly", optionUri.Segments[0]);
            Assert.Equal("VDS.RDF.Options,SomeAssembly", optionUri.AbsolutePath);
            Assert.Equal("UsePLinqEvaluation", optionUri.Fragment.Substring(1));
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

        [Fact]
        public void ConfigurationStaticOptionsNoFragment()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph");

            Assert.Throws<DotNetRdfConfigurationException>(() => this.ApplyStaticOptionsConfigure(optionUri, ""));
        }

        [Fact]
        public void ConfigurationStaticOptionsBadClass()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.NoSuchClass#Property");

            Assert.Throws<DotNetRdfConfigurationException>(() => this.ApplyStaticOptionsConfigure(optionUri, ""));
        }

        [Fact]
        public void ConfigurationStaticOptionsBadProperty()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph#NoSuchProperty");

            Assert.Throws<DotNetRdfConfigurationException>(() => this.ApplyStaticOptionsConfigure(optionUri, ""));
        }

        [Fact]
        public void ConfigurationStaticOptionsNonStaticProperty()
        {
            Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Graph#BaseUri");

            Assert.Throws<DotNetRdfConfigurationException>(() => this.ApplyStaticOptionsConfigure(optionUri, "http://example.org"));
        }

        [Fact]
        public void ConfigurationStaticOptionsEnumProperty()
        {
            LiteralEqualityMode current = Options.LiteralEqualityMode;
            try
            {
                Assert.Equal(current, Options.LiteralEqualityMode);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#LiteralEqualityMode");
                this.ApplyStaticOptionsConfigure(optionUri, "Loose");

                Assert.Equal(LiteralEqualityMode.Loose, Options.LiteralEqualityMode);
            }
            finally
            {
                Options.LiteralEqualityMode = current;
            }
        }

        [Fact]
        public void ConfigurationStaticOptionsInt32Property()
        {
            int current = UriLoader.Timeout;
            try
            {
                Assert.Equal(current, UriLoader.Timeout);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#UriLoaderTimeout");
                Graph g = new Graph();
                this.ApplyStaticOptionsConfigure(g, optionUri, (99).ToLiteral(g));

                Assert.Equal(99, UriLoader.Timeout);
            }
            finally
            {
                UriLoader.Timeout = current;
            }
        }

        [Fact]
        public void ConfigurationStaticOptionsInt64Property()
        {
            long current = Options.QueryExecutionTimeout;
            try
            {
                Assert.Equal(current, Options.QueryExecutionTimeout);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#QueryExecutionTimeout");
                Graph g = new Graph();
                this.ApplyStaticOptionsConfigure(g, optionUri, (99).ToLiteral(g));

                Assert.Equal(99, Options.QueryExecutionTimeout);
            }
            finally
            {
                Options.QueryExecutionTimeout = current;
            }
        }

#if NET40
        [Fact]
        public void ConfigurationStaticOptionsBooleanProperty()
        {
            bool current = Options.UsePLinqEvaluation;
            try
            {
                Assert.Equal(current, Options.UsePLinqEvaluation);

                Uri optionUri = new Uri("dotnetrdf-configure:VDS.RDF.Options#UsePLinqEvaluation");
                Graph g = new Graph();
                this.ApplyStaticOptionsConfigure(g, optionUri, (false).ToLiteral(g));

                Assert.False(Options.UsePLinqEvaluation);
            }
            finally
            {
                Options.UsePLinqEvaluation = current;
            }
        }
#endif

        [Fact]
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

                Assert.Equal(typeof(MockSparqlOperator), op.GetType());
                SparqlOperators.RemoveOperator(op);
            }
            finally
            {
                SparqlOperators.Reset();
            }
        }

        [Fact]
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

                Assert.False(SparqlOperators.IsRegistered(new DateTimeAddition()));
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
