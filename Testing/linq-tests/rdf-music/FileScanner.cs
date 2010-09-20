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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using ID3Lib;
using VDS.RDF.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace RdfMusic
{
    public class MusicStore
    {
        private static int nscount = 1;
        Dictionary<string, string> namespaces = new Dictionary<string, string>();
        private IInMemoryQueryableStore store;

        public IInMemoryQueryableStore TripleStore
        {
            get
            {
                return store;
            }
        }

        private enum StoreFormat
        {
            N3,
            RDF,
            Unknown
        } ;

        public void InitialiseStore(string storeLocation)
        {
            TripleStore store = new TripleStore();
            //store.AddReasoner(new Euler(new N3Reader(MusicConstants.OntologyURL)));
            if (File.Exists(storeLocation))
            {
                Graph g = new Graph();
                FileLoader.Load(g, storeLocation);
                store.Add(g);
            }
        }

        public void WriteStoreToFile(string storeLocation)
        {
            IRdfWriter rxw = GetRdfWriter(storeLocation);
            //foreach (string key in namespaces.Keys)
            //{
            //    rxw.Namespaces.AddNamespace(namespaces[key], key);
            //}
            rxw.Save(store.GetDefaultGraph(), storeLocation);
        }

        private static IRdfWriter GetRdfWriter(string storeLocation)
        {
            return MimeTypesHelper.GetWriter(MimeTypesHelper.GetMimeType(Path.GetExtension(storeLocation)));
        }

        private static StoreFormat GetStoreFormat(string location)
        {
            string[] sa = location.Split(new char[] { '.' });
            if (sa[sa.Length - 1] == "n3")
            {
                return StoreFormat.N3;
            }
            else
            {
                return StoreFormat.RDF;
            }
        }

        public void Add(Track t)
        {
            string uri = OwlClassSupertype.GetOntologyBaseUri(typeof(Track));
            if (!namespaces.ContainsValue(uri))
            {
                namespaces.Add("generatedNamespaceChar" + nscount++, uri);
            }
            store.Add(t);
        }
    }

    public class FileScanner
    {
        public class NewFileScannedEventArgs : EventArgs
        {
            public string FileLocation
            {
                get { return fileLocation; }
            }

            private readonly string fileLocation;

            public NewFileScannedEventArgs(string fileLocation):base()
            {
                this.fileLocation = fileLocation;
            }
        }
        public event EventHandler<NewFileScannedEventArgs> newFileScanned;

        public void ScanFiles(string directoryLocation, MusicStore store)
        {

            foreach (Track t in GetAllTracks(directoryLocation))
            {
                t.InstanceUri = GenTrackName(t);
                store.Add(t);
                if (newFileScanned != null) newFileScanned(this, new NewFileScannedEventArgs(t.InstanceUri));
            }
        }

        private IEnumerable<Track> GetAllTracks(string location)
        {
            MP3File mp3f = new MP3File();
            foreach (FileInfo info in RdfMusic.IO.FsIter.FilesByExtension(".mp3", new DirectoryInfo(location)))
            {
                TagHandler th = null;
                try
                {
                    th = new TagHandler(mp3f.Read(info.FullName));
                }
                catch { }
                if (th != null)
                    yield return new Track(th, info.FullName);
            }
        }

        private string GenTrackName(Track track)
        {
            return OwlClassSupertype.GetInstanceBaseUri(typeof(Track)) + "_" + HttpUtility.UrlEncode(track.FileLocation.GetHashCode().ToString());
        }

    }
}
