/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using ID3Lib;
using VDS.RDF.Linq;

namespace RdfMusic
{
  public class MusicDataContext : RdfDataContext
  {
    public MusicDataContext(LinqTripleStore store) : base(store){}

    public MusicDataContext(string store) : base(new LinqTripleStore(store)) { }

    public IQueryable<Album> Albums
    {
      get { return ForType<Album>(); }
    }

    public IQueryable<Track> Tracks
    {
      get { return ForType<Track>(); }
    }
  }

  [OwlResource(OntologyName = "Music", RelativeUriReference = "Track")]
  public class Track : OwlInstanceSupertype
  {
    public Track(TagHandler th, string fileLocation)
    {
      FileLocation = fileLocation;
      Title = th.Track;
      ArtistName = th.Artist;
      AlbumName = th.Album;
      Year = th.Year;
      GenreName = th.Genere;
      Comment = th.Comment;
    }

    public Track()
    {
    }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "title")]
    public string Title { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "artistName")]
    public string ArtistName { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "albumName")]
    public string AlbumName { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "year")]
    public string Year { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "genreName")]
    public string GenreName { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "comment")]
    public string Comment { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "fileLocation")]
    public string FileLocation { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "rating", XmlSchemaDatatype = "integer")]
    public int Rating { get; set; }

    private EntityRef<Album> _Album { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "isTrackOn")]
    public Album Album
    {
      get
      {
        if (_Album.HasLoadedOrAssignedValue)
          return _Album.Entity;
        if (DataContext != null)
        {
          var ctx = (MusicDataContext) DataContext;
          string trackUri = InstanceUri;
          string trackPredicateUri = this.PredicateUriForProperty(MethodBase.GetCurrentMethod());
          _Album = new EntityRef<Album>(
            from r in ((MusicDataContext) DataContext).Albums
            where r.StmtObjectWithSubjectAndPredicate(trackUri, trackPredicateUri)
            select r);

          return _Album.Entity;
        }
        return null;
      }
    }
  }

  [OwlResource(OntologyName = "Music", RelativeUriReference = "Album")]
  public class Album : OwlInstanceSupertype
  {
    private readonly EntitySet<Track> _Tracks = new EntitySet<Track>();

    [OwlResource(OntologyName = "Music", RelativeUriReference = "name")]
    public string Name { get; set; }

    [OwlResource(OntologyName = "Music", RelativeUriReference = "isTrackOn")]
    public EntitySet<Track> Tracks
    {
      get
      {
        if (_Tracks.HasLoadedOrAssignedValues)
          return _Tracks;
        if (DataContext != null)
        {
          string recordUri = InstanceUri;
          string trackPredicateUri = this.PredicateUriForProperty(MethodBase.GetCurrentMethod());
          _Tracks.SetSource(
            from t in ((MusicDataContext) DataContext).Tracks
            where t.StmtSubjectWithObjectAndPredicate(recordUri, trackPredicateUri)
            select t);
        }
        return _Tracks;
      }
    }
  }
}