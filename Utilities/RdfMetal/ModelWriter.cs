using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace rdfMetal
{
    public class ModelWriter
    {
        public void Write(string output, IEnumerable<OntologyClass> classes)
        {
            var serializer = new XmlSerializer(typeof (OntologyClass[]));
            var fs = new FileStream(output, FileMode.Create);
            TextWriter writer = new StreamWriter(fs, new UTF8Encoding());
            // Serialize using the XmlTextWriter.
            serializer.Serialize(writer, classes.ToArray());
            writer.Close();
        }

        public IEnumerable<OntologyClass> Read(string path)
        {
            var serializer = new XmlSerializer(typeof (OntologyClass[]));
            // Create a TextReader to read the file. 
            var fs = new FileStream(path, FileMode.OpenOrCreate);
            TextReader reader = new StreamReader(fs);
            return (OntologyClass[]) serializer.Deserialize(reader);
        }
    }
}