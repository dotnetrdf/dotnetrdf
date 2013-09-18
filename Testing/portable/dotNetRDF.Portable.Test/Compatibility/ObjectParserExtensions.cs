using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    public static class ObjectParserExtensions
    {
        public static T ParseFromFile<T>(this IObjectParser<T> parser, string fileName)
        {
            using (var streamReader = new StreamReader(fileName))
            {
                return parser.Parse(streamReader);
            }
        }
    }
}
