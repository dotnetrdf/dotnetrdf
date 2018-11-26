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

            Assert.Equal(original as IEnumerable, collection as IEnumerable);
        }

        [Fact]
        public void Strings_are_wrapped()
        {
            var original = string.Empty;
            var collection = new RdfCollection(original);

            Assert.Equal(new[] { original } as object, collection as object);
        }

        [Fact]
        public void Params_are_deconstructed()
        {
            var original = new object[] { 0, "a", 'a' };
            var collection = new RdfCollection(original[0], original[1], original[2]);

            Assert.Equal(original as IEnumerable, collection as IEnumerable);
        }
    }
}
