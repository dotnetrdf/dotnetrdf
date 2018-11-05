namespace VDS.RDF.Dynamic
{
    using System.Collections;

    public interface IRdfCollection : IEnumerable { }

    public class RdfCollection : IRdfCollection
    {
        private readonly IEnumerable original;

        public RdfCollection(IEnumerable original)
        {
            if (original is string)
            {
                this.original = original.AsEnumerable();
            }
            else
            {
                this.original = original;
            }
        }

        public RdfCollection(params object[] stuff)
        {
            this.original = stuff;
        }

        public IEnumerator GetEnumerator() => original.GetEnumerator();
    }

    public static class RdfCollectionExtensions
    {
        public static IRdfCollection AsRdfCollection(this IEnumerable original)
        {
            return new RdfCollection(original);
        }
    }
}
