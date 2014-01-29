using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public abstract class RdfIOAttribute
        : Attribute
    {
        public String SyntaxName { get; set; }

        public String FormatUri { get; set; }

        public String Encoding { get; set; }

        public IEnumerable<String> FileExtensions { get; set; }

        public String CanonicalFileExtension { get; set; }

        public IEnumerable<String> MimeTypes { get; set; }

        public String CanonicalMimeType { get; set; }

        public Type ParserType { get; set; }

        public Type WriterType { get; set; }
    }
}
