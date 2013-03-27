using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF
{
    public static class GraphExtensions
    {
        public static void LoadFromFile(this Graph g, string filename)
        {
            LoadFromFile(g, filename, null);
        }

        public static void LoadFromFile(this Graph g, string filename, IRdfReader parser)
        {
            var path = System.IO.Path.GetFullPath(filename);
            if (g.IsEmpty && g.BaseUri == null)
            {
                if (System.IO.Path.IsPathRooted(filename))
                {
                    g.BaseUri = UriFactory.Create("file:///" + filename);
                }
                else
                {
                    g.BaseUri = UriFactory.Create("file:///" + path);
                }
            }
            using (var stream = System.IO.File.OpenRead(path))
            {
                StreamLoader.Load(g, filename, stream, parser);
            }
        }

        public static void SaveToFile(this IGraph g, string filename)
        {
            using (var w = new StreamWriter(filename))
            {
                g.SaveToStream(filename, w);
            }
        }
    }

}