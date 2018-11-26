namespace VDS.RDF.Dynamic
{
    using System.Collections;
    using Xunit;

    public class RdfCollectionTests
    {
        [Fact]
        public void Enumerables_are_unmodified()
        {
            var original = new object[] { 0, "a", 'a' };
            var collection = new RdfCollection(original);

            Assert.Equal<IEnumerable>(original, collection);
        }

        [Fact]
        public void Strings_are_wrapped()
        {
            var original = string.Empty;
            var collection = new RdfCollection(original);

            Assert.Equal<object>(new[] { original }, collection);
        }

        [Fact]
        public void Params_are_deconstructed()
        {
            var original = new object[] { 0, "a", 'a' };
            var collection = new RdfCollection(original[0], original[1], original[2]);

            Assert.Equal<IEnumerable>(original, collection);
        }
    }
}
