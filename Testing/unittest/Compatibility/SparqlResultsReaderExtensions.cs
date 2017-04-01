using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Query;

namespace VDS.RDF
{
    public static class SparqlResultsReaderExtensions
    {
        public static void Load(this ISparqlResultsReader reader, SparqlResultSet resultSet, string filename)
        {
            using (var input = new StreamReader(filename))
            {
                reader.Load(resultSet, input);
            }
        }

        public static void Load(this ISparqlResultsReader reader, ISparqlResultsHandler handler, string filename)
        {
            using (var input = new StreamReader(filename))
            {
                reader.Load(handler, input);
            }
        }
    }
}
