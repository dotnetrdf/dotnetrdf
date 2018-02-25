namespace VDS.RDF.Skos
{
    using System.Collections.Generic;
    using System.Linq;

    public class SkosCollection : SkosMember
    {
        public SkosCollection(INode resource) : base(resource) { }

        public IEnumerable<SkosMember> Member
        {
            get
            {
                return this
                    .GetObjects(SkosHelper.Member)
                    .Select(SkosMember.Create);
            }
        }
    }
}
