using System;
using System.Collections.Generic;

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

        public MimeTypeDefinition GetDefinition()
        {
            List<String> mimeTypes = new List<string>();
            if (!ReferenceEquals(this.CanonicalMimeType, null)) mimeTypes.Add(this.CanonicalMimeType);
            if (!ReferenceEquals(this.MimeTypes, null)) mimeTypes.AddRange(this.MimeTypes);
            List<String> fileExts = new List<string>();
            if (!ReferenceEquals(this.CanonicalFileExtension, null)) fileExts.Add(this.CanonicalFileExtension);
            if (!ReferenceEquals(this.FileExtensions, null)) fileExts.AddRange(this.FileExtensions);
            return new MimeTypeDefinition(this.SyntaxName, this.FormatUri, mimeTypes, fileExts, this.ParserType, this.WriterType);
        }
    }
}