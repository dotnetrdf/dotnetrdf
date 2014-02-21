using System;
using System.Collections.Generic;

namespace VDS.RDF.Attributes
{
    /// <summary>
    /// An attribute used to declare a RDF syntax for IO purposes
    /// </summary>
    /// <remarks>
    /// The <see cref="IOManager"/> will use these attributes to detect assemblies which provide support for additional RDF syntaxes beyond the basic ones the IO Core supports
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class RdfIOAttribute
        : Attribute
    {
        /// <summary>
        /// Gets/Sets the friendly syntax name
        /// </summary>
        public String SyntaxName { get; set; }

        /// <summary>
        /// Gets/Sets the syntax URI (if known/applicable)
        /// </summary>
        public String FormatUri { get; set; }

        /// <summary>
        /// Gets/Sets the default character encoding for the syntax
        /// </summary>
        public String Encoding { get; set; }

        /// <summary>
        /// Gets/Sets the file extensions to associate with the syntax
        /// </summary>
        public String[] FileExtensions { get; set; }

        /// <summary>
        /// Gets/Sets the canonical file extension for the syntax
        /// </summary>
        public String CanonicalFileExtension { get; set; }

        /// <summary>
        /// Gets/Sets the MIME types to associate with the syntax
        /// </summary>
        public String[] MimeTypes { get; set; }

        /// <summary>
        /// Gets/Sets the canonical MIME type for the syntax
        /// </summary>
        public String CanonicalMimeType { get; set; }

        /// <summary>
        /// Gets/Sets a parser type for the syntax, the type must implement <see cref="IRdfReader"/> for the attribute to be successfully used
        /// </summary>
        public Type ParserType { get; set; }

        /// <summary>
        /// Gets/Sets a writer type for the syntax, the type must implement <see cref="IRdfWriter"/> for the attribute to be successfully used
        /// </summary>
        public Type WriterType { get; set; }

        /// <summary>
        /// Creates a <see cref="MimeTypeDefinition"/> based on the attribute settings
        /// </summary>
        /// <returns>MIME Type Definition</returns>
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