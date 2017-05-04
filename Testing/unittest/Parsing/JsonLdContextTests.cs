using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using VDS.RDF.JsonLd;

namespace VDS.RDF.Parsing
{
    public class JsonLdContextTests
    {

        public JsonLdContextTests() { }

        [Fact]
        public void EmptyContextDefaultsToNullBaseIri()
        {
            var context = new JsonLdContext();
            Assert.Null(context.Base);
        }

        [Fact]
        public void BaseIriCanBeSpecifiedInContextObject()
        {
            var processor = new JsonLdProcessor(new JsonLdProcessorOptions());
            var contextObject = JObject.Parse(@"{'@base': 'http://example.org/'}");
            var context = processor.ProcessContext(new JsonLdContext(), contextObject);
            Assert.Equal(new Uri("http://example.org/"), context.Base);
        }

        [Fact]
        public void BaseIriCanBeSpecifiedInOptions()
        {
            var processor = new JsonLdProcessor(new JsonLdProcessorOptions { Base = new Uri("http://contoso.com/") });
            var contextObject = JObject.Parse("{}");
            var context = processor.ProcessContext(new JsonLdContext(), null);
            Assert.Equal(new Uri("http://contoso.com/"), context.Base);
        }

        [Fact]
        public void OptionsOverridesBaseInContextObject()
        {
            var processor = new JsonLdProcessor(new JsonLdProcessorOptions { Base = new Uri("http://contoso.com/") });
            var contextObject = JObject.Parse("{'@base': 'http://example.org/'}");
            var context = processor.ProcessContext(new JsonLdContext(), contextObject);
            context = processor.ProcessContext(context, null);
            Assert.Equal(new Uri("http://contoso.com/"), context.Base);
        }

    }
}
