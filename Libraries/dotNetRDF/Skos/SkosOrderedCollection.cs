namespace VDS.RDF.Skos
{
    using System.Collections.Generic;
    using System.Linq;

    public class SkosOrderedCollection : SkosCollection
    {
        public SkosOrderedCollection(INode resource) : base(resource) { }

        public IEnumerable<SkosMember> MemberList
        {
            get
            {
                return this.resource.Graph
                    .GetListItems(
                        this.GetObjects(SkosHelper.Member)
                        .Single())
                    .Select(SkosMember.Create);
            }
        }
    }
}
