using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LinqToRdf;
using System.Data.Linq;

namespace Some.Namespace
{
[assembly: Ontology(
    BaseUri = "http://purl.org/ontology/mo/",
    Name = "MyOntology",
    Prefix = "MyOntology",
    UrlOfOntology = "http://purl.org/ontology/mo/")]


    public partial class MyOntologyDataContext : RdfDataContext
    {
        public MyOntologyDataContext(TripleStore store) : base(store)
        {
        }
        public MyOntologyDataContext(string store) : base(new TripleStore(store))
        {
        }

		        public IQueryable<Record> Records
		        {
		            get
		            {
		                return ForType<Record>();
		            }
		        }
		
		        public IQueryable<Track> Tracks
		        {
		            get
		            {
		                return ForType<Track>();
		            }
		        }
		
		        public IQueryable<MusicArtist> MusicArtists
		        {
		            get
		            {
		                return ForType<MusicArtist>();
		            }
		        }
		
		        public IQueryable<SoloMusicArtist> SoloMusicArtists
		        {
		            get
		            {
		                return ForType<SoloMusicArtist>();
		            }
		        }
		
		        public IQueryable<MusicGroup> MusicGroups
		        {
		            get
		            {
		                return ForType<MusicGroup>();
		            }
		        }
		
		        public IQueryable<CorporateBody> CorporateBodys
		        {
		            get
		            {
		                return ForType<CorporateBody>();
		            }
		        }
		
		        public IQueryable<Label> Labels
		        {
		            get
		            {
		                return ForType<Label>();
		            }
		        }
		
		        public IQueryable<MusicalWork> MusicalWorks
		        {
		            get
		            {
		                return ForType<MusicalWork>();
		            }
		        }
		
		        public IQueryable<Movement> Movements
		        {
		            get
		            {
		                return ForType<Movement>();
		            }
		        }
		
		        public IQueryable<MusicalExpression> MusicalExpressions
		        {
		            get
		            {
		                return ForType<MusicalExpression>();
		            }
		        }
		
		        public IQueryable<Sound> Sounds
		        {
		            get
		            {
		                return ForType<Sound>();
		            }
		        }
		
		        public IQueryable<Signal> Signals
		        {
		            get
		            {
		                return ForType<Signal>();
		            }
		        }
		
		        public IQueryable<AnalogSignal> AnalogSignals
		        {
		            get
		            {
		                return ForType<AnalogSignal>();
		            }
		        }
		
		        public IQueryable<DigitalSignal> DigitalSignals
		        {
		            get
		            {
		                return ForType<DigitalSignal>();
		            }
		        }
		
		        public IQueryable<Score> Scores
		        {
		            get
		            {
		                return ForType<Score>();
		            }
		        }
		
		        public IQueryable<Lyrics> Lyricss
		        {
		            get
		            {
		                return ForType<Lyrics>();
		            }
		        }
		
		        public IQueryable<Libretto> Librettos
		        {
		            get
		            {
		                return ForType<Libretto>();
		            }
		        }
		
		        public IQueryable<Performance> Performances
		        {
		            get
		            {
		                return ForType<Performance>();
		            }
		        }
		
		        public IQueryable<Performer> Performers
		        {
		            get
		            {
		                return ForType<Performer>();
		            }
		        }
		
		        public IQueryable<SoundEngineer> SoundEngineers
		        {
		            get
		            {
		                return ForType<SoundEngineer>();
		            }
		        }
		
		        public IQueryable<Listener> Listeners
		        {
		            get
		            {
		                return ForType<Listener>();
		            }
		        }
		
		        public IQueryable<Conductor> Conductors
		        {
		            get
		            {
		                return ForType<Conductor>();
		            }
		        }
		
		        public IQueryable<Composer> Composers
		        {
		            get
		            {
		                return ForType<Composer>();
		            }
		        }
		
		        public IQueryable<Composition> Compositions
		        {
		            get
		            {
		                return ForType<Composition>();
		            }
		        }
		
		        public IQueryable<Arranger> Arrangers
		        {
		            get
		            {
		                return ForType<Arranger>();
		            }
		        }
		
		        public IQueryable<Arrangement> Arrangements
		        {
		            get
		            {
		                return ForType<Arrangement>();
		            }
		        }
		
		        public IQueryable<Recording> Recordings
		        {
		            get
		            {
		                return ForType<Recording>();
		            }
		        }
		
		        public IQueryable<Instrumentation> Instrumentations
		        {
		            get
		            {
		                return ForType<Instrumentation>();
		            }
		        }
		
		        public IQueryable<Orchestration> Orchestrations
		        {
		            get
		            {
		                return ForType<Orchestration>();
		            }
		        }
		
		        public IQueryable<PublishedScore> PublishedScores
		        {
		            get
		            {
		                return ForType<PublishedScore>();
		            }
		        }
		
		        public IQueryable<MusicalManifestation> MusicalManifestations
		        {
		            get
		            {
		                return ForType<MusicalManifestation>();
		            }
		        }
		
		        public IQueryable<PublishedLibretto> PublishedLibrettos
		        {
		            get
		            {
		                return ForType<PublishedLibretto>();
		            }
		        }
		
		        public IQueryable<PublishedLyrics> PublishedLyricss
		        {
		            get
		            {
		                return ForType<PublishedLyrics>();
		            }
		        }
		
		        public IQueryable<Festival> Festivals
		        {
		            get
		            {
		                return ForType<Festival>();
		            }
		        }
		
		        public IQueryable<Show> Shows
		        {
		            get
		            {
		                return ForType<Show>();
		            }
		        }
		
		        public IQueryable<ReleaseType> ReleaseTypes
		        {
		            get
		            {
		                return ForType<ReleaseType>();
		            }
		        }
		
		        public IQueryable<MusicalItem> MusicalItems
		        {
		            get
		            {
		                return ForType<MusicalItem>();
		            }
		        }
		
		        public IQueryable<Medium> Mediums
		        {
		            get
		            {
		                return ForType<Medium>();
		            }
		        }
		
		        public IQueryable<Dat> Dats
		        {
		            get
		            {
		                return ForType<Dat>();
		            }
		        }
		
		        public IQueryable<Dcc> Dccs
		        {
		            get
		            {
		                return ForType<Dcc>();
		            }
		        }
		
		        public IQueryable<Cd> Cds
		        {
		            get
		            {
		                return ForType<Cd>();
		            }
		        }
		
		        public IQueryable<Md> Mds
		        {
		            get
		            {
		                return ForType<Md>();
		            }
		        }
		
		        public IQueryable<Dvda> Dvdas
		        {
		            get
		            {
		                return ForType<Dvda>();
		            }
		        }
		
		        public IQueryable<Sacd> Sacds
		        {
		            get
		            {
		                return ForType<Sacd>();
		            }
		        }
		
		        public IQueryable<Vinyl> Vinyls
		        {
		            get
		            {
		                return ForType<Vinyl>();
		            }
		        }
		
		        public IQueryable<Magnetictape> Magnetictapes
		        {
		            get
		            {
		                return ForType<Magnetictape>();
		            }
		        }
		
		        public IQueryable<Stream> Streams
		        {
		            get
		            {
		                return ForType<Stream>();
		            }
		        }
		
		        public IQueryable<ReleaseStatus> ReleaseStatuss
		        {
		            get
		            {
		                return ForType<ReleaseStatus>();
		            }
		        }
		
		        public IQueryable<Instrument> Instruments
		        {
		            get
		            {
		                return ForType<Instrument>();
		            }
		        }
		
		        public IQueryable<String> Strings
		        {
		            get
		            {
		                return ForType<String>();
		            }
		        }
		
		        public IQueryable<Woodwind> Woodwinds
		        {
		            get
		            {
		                return ForType<Woodwind>();
		            }
		        }
		
		        public IQueryable<Brass> Brasss
		        {
		            get
		            {
		                return ForType<Brass>();
		            }
		        }
		
		        public IQueryable<Percussion> Percussions
		        {
		            get
		            {
		                return ForType<Percussion>();
		            }
		        }
		
		        public IQueryable<Keyboard> Keyboards
		        {
		            get
		            {
		                return ForType<Keyboard>();
		            }
		        }
		
		        public IQueryable<Digital> Digitals
		        {
		            get
		            {
		                return ForType<Digital>();
		            }
		        }
		
		        public IQueryable<Genre> Genres
		        {
		            get
		            {
		                return ForType<Genre>();
		            }
		        }
		
		        public IQueryable<Classical> Classicals
		        {
		            get
		            {
		                return ForType<Classical>();
		            }
		        }
		
		        public IQueryable<Rock> Rocks
		        {
		            get
		            {
		                return ForType<Rock>();
		            }
		        }
		
		        public IQueryable<Jazz> Jazzs
		        {
		            get
		            {
		                return ForType<Jazz>();
		            }
		        }
		
		        public IQueryable<World> Worlds
		        {
		            get
		            {
		                return ForType<World>();
		            }
		        }
		
		        public IQueryable<Hiphop> Hiphops
		        {
		            get
		            {
		                return ForType<Hiphop>();
		            }
		        }
		
		        public IQueryable<Country> Countrys
		        {
		            get
		            {
		                return ForType<Country>();
		            }
		        }
		
		        public IQueryable<Blues> Bluess
		        {
		            get
		            {
		                return ForType<Blues>();
		            }
		        }
		
		        public IQueryable<Electronica> Electronicas
		        {
		            get
		            {
		                return ForType<Electronica>();
		            }
		        }
		
		        public IQueryable<Gospel> Gospels
		        {
		            get
		            {
		                return ForType<Gospel>();
		            }
		        }
		
		        public IQueryable<Funk> Funks
		        {
		            get
		            {
		                return ForType<Funk>();
		            }
		        }
		
		        public IQueryable<Pop> Pops
		        {
		            get
		            {
		                return ForType<Pop>();
		            }
		        }
		
		        public IQueryable<Melodic> Melodics
		        {
		            get
		            {
		                return ForType<Melodic>();
		            }
		        }
		
		        public IQueryable<Reggae> Reggaes
		        {
		            get
		            {
		                return ForType<Reggae>();
		            }
		        }
		
		        public IQueryable<DAT> DATs
		        {
		            get
		            {
		                return ForType<DAT>();
		            }
		        }
		
		        public IQueryable<DCC> DCCs
		        {
		            get
		            {
		                return ForType<DCC>();
		            }
		        }
		
		        public IQueryable<CD> CDs
		        {
		            get
		            {
		                return ForType<CD>();
		            }
		        }
		
		        public IQueryable<MD> MDs
		        {
		            get
		            {
		                return ForType<MD>();
		            }
		        }
		
		        public IQueryable<DVDA> DVDAs
		        {
		            get
		            {
		                return ForType<DVDA>();
		            }
		        }
		
		        public IQueryable<SACD> SACDs
		        {
		            get
		            {
		                return ForType<SACD>();
		            }
		        }
		
		        public IQueryable<MagneticTape> MagneticTapes
		        {
		            get
		            {
		                return ForType<MagneticTape>();
		            }
		        }
		
		        public IQueryable<Torrent> Torrents
		        {
		            get
		            {
		                return ForType<Torrent>();
		            }
		        }
		
		        public IQueryable<ED2K> ED2Ks
		        {
		            get
		            {
		                return ForType<ED2K>();
		            }
		        }
		
		        public IQueryable<AudioFile> AudioFiles
		        {
		            get
		            {
		                return ForType<AudioFile>();
		            }
		        }
		

    }

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Record")]
public partial class Record : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "has_track")]
public string has_trackUri { get; set; }

private EntityRef<Track> _has_track { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "has_track")]
public Track has_track
{
    get
    {
        if (_has_track.HasLoadedOrAssignedValue)
            return _has_track.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _has_track = new EntityRef<Track>(from x in ctx.Tracks where x.HasInstanceUri(has_trackUri) select x);
            return _has_track.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "track")]
public string trackUri { get; set; }

private EntityRef<Track> _track { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "track")]
public Track track
{
    get
    {
        if (_track.HasLoadedOrAssignedValue)
            return _track.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _track = new EntityRef<Track>(from x in ctx.Tracks where x.HasInstanceUri(trackUri) select x);
            return _track.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Track")]
public partial class Track : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "trackNum")]
  public string trackNum {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "track_number")]
  public int track_number {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "olga")]
  public Document olga {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MusicArtist")]
public partial class MusicArtist : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "fanpage")]
  public Document fanpage {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "biography")]
  public Document biography {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "discography")]
  public Document discography {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remixed")]
public string remixedUri { get; set; }

private EntityRef<Signal> _remixed { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remixed")]
public Signal remixed
{
    get
    {
        if (_remixed.HasLoadedOrAssignedValue)
            return _remixed.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _remixed = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(remixedUri) select x);
            return _remixed.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampled")]
public string sampledUri { get; set; }

private EntityRef<Signal> _sampled { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampled")]
public Signal sampled
{
    get
    {
        if (_sampled.HasLoadedOrAssignedValue)
            return _sampled.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _sampled = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(sampledUri) select x);
            return _sampled.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "compiled")]
public string compiledUri { get; set; }

private EntityRef<MusicalManifestation> _compiled { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "compiled")]
public MusicalManifestation compiled
{
    get
    {
        if (_compiled.HasLoadedOrAssignedValue)
            return _compiled.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _compiled = new EntityRef<MusicalManifestation>(from x in ctx.MusicalManifestations where x.HasInstanceUri(compiledUri) select x);
            return _compiled.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "djmixed")]
public string djmixedUri { get; set; }

private EntityRef<Signal> _djmixed { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "djmixed")]
public Signal djmixed
{
    get
    {
        if (_djmixed.HasLoadedOrAssignedValue)
            return _djmixed.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _djmixed = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(djmixedUri) select x);
            return _djmixed.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "supporting_musician")]
public string supporting_musicianUri { get; set; }

private EntityRef<MusicArtist> _supporting_musician { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "supporting_musician")]
public MusicArtist supporting_musician
{
    get
    {
        if (_supporting_musician.HasLoadedOrAssignedValue)
            return _supporting_musician.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _supporting_musician = new EntityRef<MusicArtist>(from x in ctx.MusicArtists where x.HasInstanceUri(supporting_musicianUri) select x);
            return _supporting_musician.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="SoloMusicArtist")]
public partial class SoloMusicArtist : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MusicGroup")]
public partial class MusicGroup : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="CorporateBody")]
public partial class CorporateBody : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Label")]
public partial class Label : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MusicalWork")]
public partial class MusicalWork : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "opus")]
  public string opus {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usedInPerformance")]
public string usedInPerformanceUri { get; set; }

private EntityRef<Performance> _usedInPerformance { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usedInPerformance")]
public Performance usedInPerformance
{
    get
    {
        if (_usedInPerformance.HasLoadedOrAssignedValue)
            return _usedInPerformance.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _usedInPerformance = new EntityRef<Performance>(from x in ctx.Performances where x.HasInstanceUri(usedInPerformanceUri) select x);
            return _usedInPerformance.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "productOfComposition")]
public string productOfCompositionUri { get; set; }

private EntityRef<Composition> _productOfComposition { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "productOfComposition")]
public Composition productOfComposition
{
    get
    {
        if (_productOfComposition.HasLoadedOrAssignedValue)
            return _productOfComposition.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _productOfComposition = new EntityRef<Composition>(from x in ctx.Compositions where x.HasInstanceUri(productOfCompositionUri) select x);
            return _productOfComposition.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "composed_in")]
public string composed_inUri { get; set; }

private EntityRef<Composition> _composed_in { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "composed_in")]
public Composition composed_in
{
    get
    {
        if (_composed_in.HasLoadedOrAssignedValue)
            return _composed_in.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _composed_in = new EntityRef<Composition>(from x in ctx.Compositions where x.HasInstanceUri(composed_inUri) select x);
            return _composed_in.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "arranged_in")]
public string arranged_inUri { get; set; }

private EntityRef<Arrangement> _arranged_in { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "arranged_in")]
public Arrangement arranged_in
{
    get
    {
        if (_arranged_in.HasLoadedOrAssignedValue)
            return _arranged_in.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _arranged_in = new EntityRef<Arrangement>(from x in ctx.Arrangements where x.HasInstanceUri(arranged_inUri) select x);
            return _arranged_in.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "movement")]
public string movementUri { get; set; }

private EntityRef<Movement> _movement { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "movement")]
public Movement movement
{
    get
    {
        if (_movement.HasLoadedOrAssignedValue)
            return _movement.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _movement = new EntityRef<Movement>(from x in ctx.Movements where x.HasInstanceUri(movementUri) select x);
            return _movement.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Movement")]
public partial class Movement : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "movementNum")]
  public string movementNum {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "movement_number")]
  public int movement_number {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MusicalExpression")]
public partial class MusicalExpression : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "hasManifestation")]
public string hasManifestationUri { get; set; }

private EntityRef<MusicalManifestation> _hasManifestation { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "hasManifestation")]
public MusicalManifestation hasManifestation
{
    get
    {
        if (_hasManifestation.HasLoadedOrAssignedValue)
            return _hasManifestation.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _hasManifestation = new EntityRef<MusicalManifestation>(from x in ctx.MusicalManifestations where x.HasInstanceUri(hasManifestationUri) select x);
            return _hasManifestation.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "manifestation")]
public string manifestationUri { get; set; }

private EntityRef<MusicalManifestation> _manifestation { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "manifestation")]
public MusicalManifestation manifestation
{
    get
    {
        if (_manifestation.HasLoadedOrAssignedValue)
            return _manifestation.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _manifestation = new EntityRef<MusicalManifestation>(from x in ctx.MusicalManifestations where x.HasInstanceUri(manifestationUri) select x);
            return _manifestation.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Sound")]
public partial class Sound : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usedInRecording")]
public string usedInRecordingUri { get; set; }

private EntityRef<Recording> _usedInRecording { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usedInRecording")]
public Recording usedInRecording
{
    get
    {
        if (_usedInRecording.HasLoadedOrAssignedValue)
            return _usedInRecording.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _usedInRecording = new EntityRef<Recording>(from x in ctx.Recordings where x.HasInstanceUri(usedInRecordingUri) select x);
            return _usedInRecording.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recorded_in")]
public string recorded_inUri { get; set; }

private EntityRef<Recording> _recorded_in { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recorded_in")]
public Recording recorded_in
{
    get
    {
        if (_recorded_in.HasLoadedOrAssignedValue)
            return _recorded_in.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _recorded_in = new EntityRef<Recording>(from x in ctx.Recordings where x.HasInstanceUri(recorded_inUri) select x);
            return _recorded_in.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Signal")]
public partial class Signal : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "channels")]
  public int channels {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "trmid")]
  public string trmid {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "puid")]
  public string puid {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "signalTime")]
  public TemporalEntity signalTime {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "time")]
  public TemporalEntity time {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "records")]
public string recordsUri { get; set; }

private EntityRef<Performance> _records { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "records")]
public Performance records
{
    get
    {
        if (_records.HasLoadedOrAssignedValue)
            return _records.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _records = new EntityRef<Performance>(from x in ctx.Performances where x.HasInstanceUri(recordsUri) select x);
            return _records.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remixer")]
public string remixerUri { get; set; }

private EntityRef<MusicArtist> _remixer { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remixer")]
public MusicArtist remixer
{
    get
    {
        if (_remixer.HasLoadedOrAssignedValue)
            return _remixer.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _remixer = new EntityRef<MusicArtist>(from x in ctx.MusicArtists where x.HasInstanceUri(remixerUri) select x);
            return _remixer.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampler")]
public string samplerUri { get; set; }

private EntityRef<MusicArtist> _sampler { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampler")]
public MusicArtist sampler
{
    get
    {
        if (_sampler.HasLoadedOrAssignedValue)
            return _sampler.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _sampler = new EntityRef<MusicArtist>(from x in ctx.MusicArtists where x.HasInstanceUri(samplerUri) select x);
            return _sampler.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "contains_sample_from")]
public string contains_sample_fromUri { get; set; }

private EntityRef<Signal> _contains_sample_from { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "contains_sample_from")]
public Signal contains_sample_from
{
    get
    {
        if (_contains_sample_from.HasLoadedOrAssignedValue)
            return _contains_sample_from.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _contains_sample_from = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(contains_sample_fromUri) select x);
            return _contains_sample_from.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "djmixed_by")]
public string djmixed_byUri { get; set; }

private EntityRef<MusicArtist> _djmixed_by { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "djmixed_by")]
public MusicArtist djmixed_by
{
    get
    {
        if (_djmixed_by.HasLoadedOrAssignedValue)
            return _djmixed_by.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _djmixed_by = new EntityRef<MusicArtist>(from x in ctx.MusicArtists where x.HasInstanceUri(djmixed_byUri) select x);
            return _djmixed_by.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remix_of")]
public string remix_ofUri { get; set; }

private EntityRef<Signal> _remix_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remix_of")]
public Signal remix_of
{
    get
    {
        if (_remix_of.HasLoadedOrAssignedValue)
            return _remix_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _remix_of = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(remix_ofUri) select x);
            return _remix_of.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "medley_of")]
public string medley_ofUri { get; set; }

private EntityRef<Signal> _medley_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "medley_of")]
public Signal medley_of
{
    get
    {
        if (_medley_of.HasLoadedOrAssignedValue)
            return _medley_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _medley_of = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(medley_ofUri) select x);
            return _medley_of.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "djmix_of")]
public string djmix_ofUri { get; set; }

private EntityRef<Signal> _djmix_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "djmix_of")]
public Signal djmix_of
{
    get
    {
        if (_djmix_of.HasLoadedOrAssignedValue)
            return _djmix_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _djmix_of = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(djmix_ofUri) select x);
            return _djmix_of.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remaster_of")]
public string remaster_ofUri { get; set; }

private EntityRef<Signal> _remaster_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "remaster_of")]
public Signal remaster_of
{
    get
    {
        if (_remaster_of.HasLoadedOrAssignedValue)
            return _remaster_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _remaster_of = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(remaster_ofUri) select x);
            return _remaster_of.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "mashup_of")]
public string mashup_ofUri { get; set; }

private EntityRef<Signal> _mashup_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "mashup_of")]
public Signal mashup_of
{
    get
    {
        if (_mashup_of.HasLoadedOrAssignedValue)
            return _mashup_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _mashup_of = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(mashup_ofUri) select x);
            return _mashup_of.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="AnalogSignal")]
public partial class AnalogSignal : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampled_version")]
public string sampled_versionUri { get; set; }

private EntityRef<DigitalSignal> _sampled_version { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampled_version")]
public DigitalSignal sampled_version
{
    get
    {
        if (_sampled_version.HasLoadedOrAssignedValue)
            return _sampled_version.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _sampled_version = new EntityRef<DigitalSignal>(from x in ctx.DigitalSignals where x.HasInstanceUri(sampled_versionUri) select x);
            return _sampled_version.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="DigitalSignal")]
public partial class DigitalSignal : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampleRate")]
  public string sampleRate {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "bitsPerSample")]
  public int bitsPerSample {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sample_rate")]
  public float sample_rate {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampledVersionOf")]
public string sampledVersionOfUri { get; set; }

private EntityRef<AnalogSignal> _sampledVersionOf { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampledVersionOf")]
public AnalogSignal sampledVersionOf
{
    get
    {
        if (_sampledVersionOf.HasLoadedOrAssignedValue)
            return _sampledVersionOf.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _sampledVersionOf = new EntityRef<AnalogSignal>(from x in ctx.AnalogSignals where x.HasInstanceUri(sampledVersionOfUri) select x);
            return _sampledVersionOf.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampled_version_of")]
public string sampled_version_ofUri { get; set; }

private EntityRef<AnalogSignal> _sampled_version_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "sampled_version_of")]
public AnalogSignal sampled_version_of
{
    get
    {
        if (_sampled_version_of.HasLoadedOrAssignedValue)
            return _sampled_version_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _sampled_version_of = new EntityRef<AnalogSignal>(from x in ctx.AnalogSignals where x.HasInstanceUri(sampled_version_ofUri) select x);
            return _sampled_version_of.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Score")]
public partial class Score : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Lyrics")]
public partial class Lyrics : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Libretto")]
public partial class Libretto : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Performance")]
public partial class Performance : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "performer")]
  public Agent performer {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "conductor")]
  public Agent conductor {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "listener")]
  public Agent listener {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usesWork")]
public string usesWorkUri { get; set; }

private EntityRef<MusicalWork> _usesWork { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usesWork")]
public MusicalWork usesWork
{
    get
    {
        if (_usesWork.HasLoadedOrAssignedValue)
            return _usesWork.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _usesWork = new EntityRef<MusicalWork>(from x in ctx.MusicalWorks where x.HasInstanceUri(usesWorkUri) select x);
            return _usesWork.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "producesSound")]
public string producesSoundUri { get; set; }

private EntityRef<Sound> _producesSound { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "producesSound")]
public Sound producesSound
{
    get
    {
        if (_producesSound.HasLoadedOrAssignedValue)
            return _producesSound.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _producesSound = new EntityRef<Sound>(from x in ctx.Sounds where x.HasInstanceUri(producesSoundUri) select x);
            return _producesSound.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recordedAs")]
public string recordedAsUri { get; set; }

private EntityRef<Signal> _recordedAs { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recordedAs")]
public Signal recordedAs
{
    get
    {
        if (_recordedAs.HasLoadedOrAssignedValue)
            return _recordedAs.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _recordedAs = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(recordedAsUri) select x);
            return _recordedAs.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_sound")]
public string produced_soundUri { get; set; }

private EntityRef<Sound> _produced_sound { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_sound")]
public Sound produced_sound
{
    get
    {
        if (_produced_sound.HasLoadedOrAssignedValue)
            return _produced_sound.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _produced_sound = new EntityRef<Sound>(from x in ctx.Sounds where x.HasInstanceUri(produced_soundUri) select x);
            return _produced_sound.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recorded_as")]
public string recorded_asUri { get; set; }

private EntityRef<Signal> _recorded_as { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recorded_as")]
public Signal recorded_as
{
    get
    {
        if (_recorded_as.HasLoadedOrAssignedValue)
            return _recorded_as.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _recorded_as = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(recorded_asUri) select x);
            return _recorded_as.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "instrument")]
public string instrumentUri { get; set; }

private EntityRef<Instrument> _instrument { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "instrument")]
public Instrument instrument
{
    get
    {
        if (_instrument.HasLoadedOrAssignedValue)
            return _instrument.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _instrument = new EntityRef<Instrument>(from x in ctx.Instruments where x.HasInstanceUri(instrumentUri) select x);
            return _instrument.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Performer")]
public partial class Performer : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="SoundEngineer")]
public partial class SoundEngineer : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Listener")]
public partial class Listener : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Conductor")]
public partial class Conductor : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Composer")]
public partial class Composer : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Composition")]
public partial class Composition : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "composer")]
  public Agent composer {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "producesWork")]
public string producesWorkUri { get; set; }

private EntityRef<MusicalWork> _producesWork { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "producesWork")]
public MusicalWork producesWork
{
    get
    {
        if (_producesWork.HasLoadedOrAssignedValue)
            return _producesWork.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _producesWork = new EntityRef<MusicalWork>(from x in ctx.MusicalWorks where x.HasInstanceUri(producesWorkUri) select x);
            return _producesWork.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_work")]
public string produced_workUri { get; set; }

private EntityRef<MusicalWork> _produced_work { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_work")]
public MusicalWork produced_work
{
    get
    {
        if (_produced_work.HasLoadedOrAssignedValue)
            return _produced_work.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _produced_work = new EntityRef<MusicalWork>(from x in ctx.MusicalWorks where x.HasInstanceUri(produced_workUri) select x);
            return _produced_work.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Arranger")]
public partial class Arranger : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Arrangement")]
public partial class Arrangement : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_score")]
public string produced_scoreUri { get; set; }

private EntityRef<Score> _produced_score { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_score")]
public Score produced_score
{
    get
    {
        if (_produced_score.HasLoadedOrAssignedValue)
            return _produced_score.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _produced_score = new EntityRef<Score>(from x in ctx.Scores where x.HasInstanceUri(produced_scoreUri) select x);
            return _produced_score.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "arrangement_of")]
public string arrangement_ofUri { get; set; }

private EntityRef<MusicalWork> _arrangement_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "arrangement_of")]
public MusicalWork arrangement_of
{
    get
    {
        if (_arrangement_of.HasLoadedOrAssignedValue)
            return _arrangement_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _arrangement_of = new EntityRef<MusicalWork>(from x in ctx.MusicalWorks where x.HasInstanceUri(arrangement_ofUri) select x);
            return _arrangement_of.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Recording")]
public partial class Recording : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usesSound")]
public string usesSoundUri { get; set; }

private EntityRef<Sound> _usesSound { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "usesSound")]
public Sound usesSound
{
    get
    {
        if (_usesSound.HasLoadedOrAssignedValue)
            return _usesSound.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _usesSound = new EntityRef<Sound>(from x in ctx.Sounds where x.HasInstanceUri(usesSoundUri) select x);
            return _usesSound.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "producesSignal")]
public string producesSignalUri { get; set; }

private EntityRef<Signal> _producesSignal { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "producesSignal")]
public Signal producesSignal
{
    get
    {
        if (_producesSignal.HasLoadedOrAssignedValue)
            return _producesSignal.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _producesSignal = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(producesSignalUri) select x);
            return _producesSignal.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recording_of")]
public string recording_ofUri { get; set; }

private EntityRef<Sound> _recording_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "recording_of")]
public Sound recording_of
{
    get
    {
        if (_recording_of.HasLoadedOrAssignedValue)
            return _recording_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _recording_of = new EntityRef<Sound>(from x in ctx.Sounds where x.HasInstanceUri(recording_ofUri) select x);
            return _recording_of.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_signal")]
public string produced_signalUri { get; set; }

private EntityRef<Signal> _produced_signal { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "produced_signal")]
public Signal produced_signal
{
    get
    {
        if (_produced_signal.HasLoadedOrAssignedValue)
            return _produced_signal.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _produced_signal = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(produced_signalUri) select x);
            return _produced_signal.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Instrumentation")]
public partial class Instrumentation : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Orchestration")]
public partial class Orchestration : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="PublishedScore")]
public partial class PublishedScore : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MusicalManifestation")]
public partial class MusicalManifestation : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "encoding")]
  public string encoding {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "producer")]
  public Agent producer {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "publishingLocation")]
  public SpatialThing publishingLocation {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "availableAs")]
  public Item availableAs {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "publisher")]
  public Agent publisher {get;set;} // 
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "publishing_location")]
  public SpatialThing publishing_location {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "compiler")]
public string compilerUri { get; set; }

private EntityRef<MusicArtist> _compiler { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "compiler")]
public MusicArtist compiler
{
    get
    {
        if (_compiler.HasLoadedOrAssignedValue)
            return _compiler.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _compiler = new EntityRef<MusicArtist>(from x in ctx.MusicArtists where x.HasInstanceUri(compilerUri) select x);
            return _compiler.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "compilation_of")]
public string compilation_ofUri { get; set; }

private EntityRef<Signal> _compilation_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "compilation_of")]
public Signal compilation_of
{
    get
    {
        if (_compilation_of.HasLoadedOrAssignedValue)
            return _compilation_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _compilation_of = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(compilation_ofUri) select x);
            return _compilation_of.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "other_release_of")]
public string other_release_ofUri { get; set; }

private EntityRef<MusicalManifestation> _other_release_of { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "other_release_of")]
public MusicalManifestation other_release_of
{
    get
    {
        if (_other_release_of.HasLoadedOrAssignedValue)
            return _other_release_of.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _other_release_of = new EntityRef<MusicalManifestation>(from x in ctx.MusicalManifestations where x.HasInstanceUri(other_release_ofUri) select x);
            return _other_release_of.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "tribute_to")]
public string tribute_toUri { get; set; }

private EntityRef<MusicArtist> _tribute_to { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "tribute_to")]
public MusicArtist tribute_to
{
    get
    {
        if (_tribute_to.HasLoadedOrAssignedValue)
            return _tribute_to.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _tribute_to = new EntityRef<MusicArtist>(from x in ctx.MusicArtists where x.HasInstanceUri(tribute_toUri) select x);
            return _tribute_to.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "releaseType")]
public string releaseTypeUri { get; set; }

private EntityRef<ReleaseType> _releaseType { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "releaseType")]
public ReleaseType releaseType
{
    get
    {
        if (_releaseType.HasLoadedOrAssignedValue)
            return _releaseType.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _releaseType = new EntityRef<ReleaseType>(from x in ctx.ReleaseTypes where x.HasInstanceUri(releaseTypeUri) select x);
            return _releaseType.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "releaseStatus")]
public string releaseStatusUri { get; set; }

private EntityRef<ReleaseStatus> _releaseStatus { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "releaseStatus")]
public ReleaseStatus releaseStatus
{
    get
    {
        if (_releaseStatus.HasLoadedOrAssignedValue)
            return _releaseStatus.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _releaseStatus = new EntityRef<ReleaseStatus>(from x in ctx.ReleaseStatuss where x.HasInstanceUri(releaseStatusUri) select x);
            return _releaseStatus.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "available_as")]
public string available_asUri { get; set; }

private EntityRef<MusicalItem> _available_as { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "available_as")]
public MusicalItem available_as
{
    get
    {
        if (_available_as.HasLoadedOrAssignedValue)
            return _available_as.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _available_as = new EntityRef<MusicalItem>(from x in ctx.MusicalItems where x.HasInstanceUri(available_asUri) select x);
            return _available_as.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "item")]
public string itemUri { get; set; }

private EntityRef<MusicalItem> _item { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "item")]
public MusicalItem item
{
    get
    {
        if (_item.HasLoadedOrAssignedValue)
            return _item.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _item = new EntityRef<MusicalItem>(from x in ctx.MusicalItems where x.HasInstanceUri(itemUri) select x);
            return _item.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "preview")]
public string previewUri { get; set; }

private EntityRef<MusicalItem> _preview { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "preview")]
public MusicalItem preview
{
    get
    {
        if (_preview.HasLoadedOrAssignedValue)
            return _preview.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _preview = new EntityRef<MusicalItem>(from x in ctx.MusicalItems where x.HasInstanceUri(previewUri) select x);
            return _preview.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "release_status")]
public string release_statusUri { get; set; }

private EntityRef<ReleaseStatus> _release_status { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "release_status")]
public ReleaseStatus release_status
{
    get
    {
        if (_release_status.HasLoadedOrAssignedValue)
            return _release_status.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _release_status = new EntityRef<ReleaseStatus>(from x in ctx.ReleaseStatuss where x.HasInstanceUri(release_statusUri) select x);
            return _release_status.Entity;
        }
        return null;
    }
}
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "release_type")]
public string release_typeUri { get; set; }

private EntityRef<ReleaseType> _release_type { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "release_type")]
public ReleaseType release_type
{
    get
    {
        if (_release_type.HasLoadedOrAssignedValue)
            return _release_type.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _release_type = new EntityRef<ReleaseType>(from x in ctx.ReleaseTypes where x.HasInstanceUri(release_typeUri) select x);
            return _release_type.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="PublishedLibretto")]
public partial class PublishedLibretto : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="PublishedLyrics")]
public partial class PublishedLyrics : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Festival")]
public partial class Festival : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Show")]
public partial class Show : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="ReleaseType")]
public partial class ReleaseType : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MusicalItem")]
public partial class MusicalItem : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "encodes")]
public string encodesUri { get; set; }

private EntityRef<Signal> _encodes { get; set; }

[OwlResource(OntologyName = "MyOntology", RelativeUriReference = "encodes")]
public Signal encodes
{
    get
    {
        if (_encodes.HasLoadedOrAssignedValue)
            return _encodes.Entity;
        if (DataContext != null)
        {
            var ctx = (MyOntologyDataContext)DataContext;
            _encodes = new EntityRef<Signal>(from x in ctx.Signals where x.HasInstanceUri(encodesUri) select x);
            return _encodes.Entity;
        }
        return null;
    }
}

#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Medium")]
public partial class Medium : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Dat")]
public partial class Dat : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Dcc")]
public partial class Dcc : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Cd")]
public partial class Cd : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Md")]
public partial class Md : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Dvda")]
public partial class Dvda : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Sacd")]
public partial class Sacd : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Vinyl")]
public partial class Vinyl : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Magnetictape")]
public partial class Magnetictape : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Stream")]
public partial class Stream : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="ReleaseStatus")]
public partial class ReleaseStatus : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Instrument")]
public partial class Instrument : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="String")]
public partial class String : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Woodwind")]
public partial class Woodwind : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Brass")]
public partial class Brass : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Percussion")]
public partial class Percussion : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Keyboard")]
public partial class Keyboard : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Digital")]
public partial class Digital : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Genre")]
public partial class Genre : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Classical")]
public partial class Classical : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Rock")]
public partial class Rock : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Jazz")]
public partial class Jazz : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="World")]
public partial class World : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Hiphop")]
public partial class Hiphop : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Country")]
public partial class Country : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Blues")]
public partial class Blues : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Electronica")]
public partial class Electronica : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Gospel")]
public partial class Gospel : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Funk")]
public partial class Funk : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Pop")]
public partial class Pop : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Melodic")]
public partial class Melodic : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Reggae")]
public partial class Reggae : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="DAT")]
public partial class DAT : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="DCC")]
public partial class DCC : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="CD")]
public partial class CD : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MD")]
public partial class MD : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="DVDA")]
public partial class DVDA : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="SACD")]
public partial class SACD : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="MagneticTape")]
public partial class MagneticTape : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="Torrent")]
public partial class Torrent : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="ED2K")]
public partial class ED2K : OwlInstanceSupertype
{
#region Datatype properties
#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}

[OwlResource(OntologyName="MyOntology", RelativeUriReference="AudioFile")]
public partial class AudioFile : OwlInstanceSupertype
{
#region Datatype properties
  [OwlResource(OntologyName = "MyOntology", RelativeUriReference = "encoding")]
  public string encoding {get;set;} // 

#endregion

#region Incoming relationships properties
#endregion

#region Object properties
#endregion
}



}