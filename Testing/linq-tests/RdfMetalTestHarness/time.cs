using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqToRdf;
using System.Data.Linq;
using System;

namespace RdfMetal.Time
{
[assembly: Ontology(
    BaseUri = "http://www.w3.org/2006/time#",
    Name = "time",
    Prefix = "time",
    UrlOfOntology = "http://www.w3.org/2006/time#")]


    public partial class timeDataContext : RdfDataContext
    {
        public timeDataContext(TripleStore store) : base(store)
        {
        }
        public timeDataContext(string store) : base(new TripleStore(store))
        {
        }

		        public IQueryable<Interval> Intervals
		        {
		            get
		            {
		                return ForType<Interval>();
		            }
		        }
		
		        public IQueryable<Instant> Instants
		        {
		            get
		            {
		                return ForType<Instant>();
		            }
		        }
		

    }

[OwlResource(OntologyName="time", RelativeUriReference="Interval")]
public partial class Interval : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "time", RelativeUriReference = "intervalDuring")]
public string intervalDuringUri { get; set; }

private EntityRef<Interval> _intervalDuring { get; set; }

[OwlResource(OntologyName = "time", RelativeUriReference = "intervalDuring")]
public Interval intervalDuring
{
    get
    {
        if (_intervalDuring.HasLoadedOrAssignedValue)
            return _intervalDuring.Entity;
        if (DataContext != null)
        {
            var ctx = (timeDataContext)DataContext;
            _intervalDuring = new EntityRef<Interval>(from x in ctx.Intervals where x.HasInstanceUri(intervalDuringUri) select x);
            return _intervalDuring.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "time", RelativeUriReference = "intervalMeets")]
public string intervalMeetsUri { get; set; }

private EntityRef<Interval> _intervalMeets { get; set; }

[OwlResource(OntologyName = "time", RelativeUriReference = "intervalMeets")]
public Interval intervalMeets
{
    get
    {
        if (_intervalMeets.HasLoadedOrAssignedValue)
            return _intervalMeets.Entity;
        if (DataContext != null)
        {
            var ctx = (timeDataContext)DataContext;
            _intervalMeets = new EntityRef<Interval>(from x in ctx.Intervals where x.HasInstanceUri(intervalMeetsUri) select x);
            return _intervalMeets.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "time", RelativeUriReference = "intervalBefore")]
public string intervalBeforeUri { get; set; }

private EntityRef<Interval> _intervalBefore { get; set; }

[OwlResource(OntologyName = "time", RelativeUriReference = "intervalBefore")]
public Interval intervalBefore
{
    get
    {
        if (_intervalBefore.HasLoadedOrAssignedValue)
            return _intervalBefore.Entity;
        if (DataContext != null)
        {
            var ctx = (timeDataContext)DataContext;
            _intervalBefore = new EntityRef<Interval>(from x in ctx.Intervals where x.HasInstanceUri(intervalBeforeUri) select x);
            return _intervalBefore.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="time", RelativeUriReference="Instant")]
public partial class Instant : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}



}