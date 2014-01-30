using System;
using System.Collections.Generic;
using System.Text;

namespace VDS.RDF.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class RdfIOAttribute
        : Attribute
    {
        public String SyntaxName { get; set; }

        public String FormatUri { get; set; }

        public String Encoding { get; set; }

        public String[] FileExtensions { get; set; }

        public String CanonicalFileExtension { get; set; }

        public String[] MimeTypes { get; set; }

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
            MimeTypeDefinition definition = new MimeTypeDefinition(this.SyntaxName, this.FormatUri, mimeTypes, fileExts, this.ParserType, this.WriterType);
            definition.CanonicalFileExtension = this.CanonicalFileExtension;
            definition.CanonicalMimeType = this.CanonicalMimeType;
            definition.Encoding = System.Text.Encoding.GetEncoding(this.Encoding);
            return definition;
        }
    }
}