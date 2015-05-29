/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
        private double _quality = 1.0;

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
        /// Gets/Sets the desired quality for the MIME type
        /// </summary>
        public double Quality
        {
            get { return this._quality; }
            set
            {
                if (value < 0d)
                {
                    this._quality = 0d;
                } 
                else if (value > 1d)
                {
                    this._quality = 1.0d;
                }
                else
                {
                    this._quality = value;
                }
            }
        }

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
            MimeTypeDefinition definition = new MimeTypeDefinition
                                                {
                                                    SyntaxName = this.SyntaxName,
                                                    FormatUri = this.FormatUri,
                                                    MimeTypes = mimeTypes,
                                                    FileExtensions = fileExts,
                                                    RdfParserType = this.ParserType,
                                                    RdfWriterType = this.WriterType,
                                                    CanonicalFileExtension = this.CanonicalFileExtension,
                                                    CanonicalMimeType = this.CanonicalMimeType,
                                                    Encoding = System.Text.Encoding.GetEncoding(this.Encoding)
                                                };
            return definition;
        }
    }
}