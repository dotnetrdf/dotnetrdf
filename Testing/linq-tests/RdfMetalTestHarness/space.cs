using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqToRdf;
using System.Data.Linq;
using System;

namespace RdfMetal.Space
{
[assembly: Ontology(
    BaseUri = "http://www.w3.org/2003/01/geo/wgs84_pos#",
    Name = "space",
    Prefix = "space",
    UrlOfOntology = "http://www.w3.org/2003/01/geo/wgs84_pos#")]


    public partial class spaceDataContext : RdfDataContext
    {
        public spaceDataContext(TripleStore store) : base(store)
        {
        }
        public spaceDataContext(string store) : base(new TripleStore(store))
        {
        }

		        public IQueryable<SpatialThing> SpatialThings
		        {
		            get
		            {
		                return ForType<SpatialThing>();
		            }
		        }
		
		        public IQueryable<Point> Points
		        {
		            get
		            {
		                return ForType<Point>();
		            }
		        }
		

    }

[OwlResource(OntologyName="space", RelativeUriReference="SpatialThing")]
public partial class SpatialThing : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="space", RelativeUriReference="Point")]
public partial class Point : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}



}