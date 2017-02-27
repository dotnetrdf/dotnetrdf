using System.IO;
using VDS.RDF.Query;

namespace VDS.RDF
{
    public static class SparqlResultsWriterExtensions
    {
        public static void Save(this ISparqlResultsWriter resultsWriter, SparqlResultSet resultSet, string filename)
        {
            using (var output = new StreamWriter(filename))
            {
                resultsWriter.Save(resultSet, output);
            }
        }
    }
}
