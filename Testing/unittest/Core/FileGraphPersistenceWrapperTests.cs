using System;
using System.IO;
using Xunit;

namespace VDS.RDF
{
    public class FileGraphPersistenceWrapperTests : IDisposable
    {
        private readonly string _persistenceFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ttl");

        [Fact]
        public void ItCanPersistANewGraph()
        {
            var wrapper = new FileGraphPersistenceWrapper(_persistenceFile);
            wrapper.Assert(wrapper.CreateUriNode(new Uri("urn:s")), wrapper.CreateUriNode(new Uri("urn:p")),
                wrapper.CreateUriNode(new Uri("urn:o")));
            wrapper.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Dispose(bool disposing)
        {
            File.Delete(_persistenceFile);
        }
#pragma warning restore xUnit1013 // Public method should be marked as test
    }
}
