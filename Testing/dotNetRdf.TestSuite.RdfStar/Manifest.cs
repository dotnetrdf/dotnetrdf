using System;
using System.IO;

namespace VDS.RDF.TestSuite.RdfStar
{
    public class Manifest
    {
        public Uri BaseUri { get; }
        public string LocalDirectory { get; }
        public Uri LocalManifestUri { get; }
        public IGraph Graph { get; }
        public Manifest(IGraph manifestGraph, string localFilePath)
        {
            Graph = manifestGraph;
            BaseUri = manifestGraph.BaseUri;
            LocalDirectory = Path.GetDirectoryName(localFilePath);
            LocalManifestUri = new Uri(new Uri("file://"), Path.GetFullPath(localFilePath));
        }

        public string ResolveResourcePath(Uri resourcePath)
        {
            var relPath = BaseUri.MakeRelativeUri(resourcePath);
            return new Uri(LocalManifestUri, relPath).LocalPath;
        }
    }
}