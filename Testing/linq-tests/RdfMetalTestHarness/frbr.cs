using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqToRdf;
using System.Data.Linq;
using System;

namespace RdfMetal.Frbr
{
[assembly: Ontology(
    BaseUri = "http://purl.org/vocab/frbr/core#",
    Name = "frbr",
    Prefix = "frbr",
    UrlOfOntology = "http://purl.org/vocab/frbr/core#")]


    public partial class frbrDataContext : RdfDataContext
    {
        public frbrDataContext(TripleStore store) : base(store)
        {
        }
        public frbrDataContext(string store) : base(new TripleStore(store))
        {
        }

		        public IQueryable<Work> Works
		        {
		            get
		            {
		                return ForType<Work>();
		            }
		        }
		
		        public IQueryable<Expression> Expressions
		        {
		            get
		            {
		                return ForType<Expression>();
		            }
		        }
		
		        public IQueryable<Manifestation> Manifestations
		        {
		            get
		            {
		                return ForType<Manifestation>();
		            }
		        }
		
		        public IQueryable<Item> Items
		        {
		            get
		            {
		                return ForType<Item>();
		            }
		        }
		

    }

[OwlResource(OntologyName="frbr", RelativeUriReference="Work")]
public partial class Work : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="frbr", RelativeUriReference="Expression")]
public partial class Expression : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="frbr", RelativeUriReference="Manifestation")]
public partial class Manifestation : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="frbr", RelativeUriReference="Item")]
public partial class Item : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}



}