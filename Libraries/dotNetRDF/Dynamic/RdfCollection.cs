namespace VDS.RDF.Dynamic
{
    using System.Collections;

    public class RdfCollection : IRdfCollection
    {
        private readonly IEnumerable original;

        public RdfCollection(params object[] originals) : this((IEnumerable)originals) { }

        public RdfCollection(IEnumerable original)
        {
            if (original is string)
            {
                original = original.AsEnumerable();
            }

            this.original = original;
        }

        public IEnumerator GetEnumerator() => original.GetEnumerator();
    }
}
