using System.IO;

namespace VDS.RDF
{
    public static class RdfWriterExtensions
    {
        public static void Save(this IRdfWriter writer, IGraph g, string fileName)
        {
            using (var textWriter = new StreamWriter(fileName))
            {
                writer.Save(g, textWriter);
            }
        }
    }
}
