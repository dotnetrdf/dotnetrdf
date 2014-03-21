/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Globalization;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    /// <summary>
    /// Represents the definition of a MIME Type including mappings to relevant readers and writers
    /// </summary>
    public class MimeTypeDefinition
    {
        protected double _quality = 1d;
        protected String _canonicalType, _canonicalExt;
        protected Encoding _encoding = Encoding.UTF8;
        protected Type _rdfParserType, _rdfWriterType;
        protected List<String> _mimeTypes = new List<string>();
        protected List<String> _fileExtensions = new List<string>();
        protected Dictionary<Type, Type> _objectParserTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// Gets the name of the Syntax to which this MIME Type Definition relates
        /// </summary>
        public string SyntaxName { get; set; }

        /// <summary>
        /// Gets the Format URI as defined by the <a href="http://www.w3.org/ns/formats/">W3C</a> (where applicable)
        /// </summary>
        public string FormatUri { get; set; }

        /// <summary>
        /// Gets the Encoding that should be used for reading and writing this Syntax
        /// </summary>
        public Encoding Encoding
        {
            get { return this._encoding ?? Encoding.UTF8; }
            set { this._encoding = value; }
        }

        /// <summary>
        /// Gets/Sets the desired quality for this MIME type
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
                    this._quality = 1d;
                }
                else
                {
                    this._quality = value;
                }
            }
        }

        #region MIME Type Management

        /// <summary>
        /// Gets the MIME Types defined
        /// </summary>
        public IEnumerable<String> MimeTypes
        {
            get { return this._mimeTypes; }
            set
            {
                if (value == null) throw new ArgumentNullException("value", "MIME Types enumeration cannot be null");
                this._mimeTypes.Clear();
                this._mimeTypes.AddRange(value.Select(t => CheckValidMimeType(t)));
            }
        }

        /// <summary>
        /// Checks that MIME Types are valid
        /// </summary>
        /// <param name="type">Type</param>
        public static String CheckValidMimeType(String type)
        {
            type = type.Trim().ToLowerInvariant();
            if (!IOManager.IsValidMimeType(type))
            {
                throw new RdfException(type + " is not a valid MIME Type");
            }
            return type;
        }

        /// <summary>
        /// Adds a MIME Type to this definition
        /// </summary>
        /// <param name="type">MIME Type</param>
        public void AddMimeType(String type)
        {
            if (!this._mimeTypes.Contains(CheckValidMimeType(type)))
            {
                this._mimeTypes.Add(CheckValidMimeType(type));
            }
        }

        /// <summary>
        /// Gets the Canonical MIME Type that should be used
        /// </summary>
        public String CanonicalMimeType
        {
            get
            {
                if (this._canonicalType != null)
                {
                    return this._canonicalType;
                }
                if (this._mimeTypes.Count > 0)
                {
                    return this._mimeTypes.First();
                }
                throw new RdfException("No MIME Types are defined for " + this.SyntaxName);
            }
            set
            {
                if (value == null)
                {
                    this._canonicalType = null;
                }
                else if (this._mimeTypes.Contains(value))
                {
                    this._canonicalType = value;
                }
                else
                {
                    throw new RdfException("Cannot set the Canonical MIME Type for " + this.SyntaxName + " to " + value + " as this is no such MIME Type listed in this definition.  Use AddMimeType to add a MIME Type prior to setting the CanonicalType.");
                }
            }
        }

        /// <summary>
        /// Determines whether the definition supports the MIME type specified by the selector
        /// </summary>
        /// <param name="selector">MIME Type selector</param>
        /// <returns></returns>
        public bool SupportsMimeType(MimeTypeSelector selector)
        {
            if (selector.IsInvalid) return false;
            if (selector.IsAny) return true;
            if (!selector.IsRange)
            {
                return this._mimeTypes.Contains(selector.Type);
            }
            return selector.RangeType != null && this._mimeTypes.Any(type => type.StartsWith(selector.RangeType));
        }

        #endregion

        #region File Extension Management

        /// <summary>
        /// Gets the File Extensions associated with this Syntax
        /// </summary>
        public IEnumerable<String> FileExtensions
        {
            get { return this._fileExtensions; }
            set
            {
                if (value == null) return;
                this._fileExtensions.Clear();
                this._fileExtensions.AddRange(value.Select(e => CheckFileExtension(e)));
            }
        }

        /// <summary>
        /// Adds a File Extension for this Syntax
        /// </summary>
        /// <param name="ext">File Extension</param>
        public void AddFileExtension(String ext)
        {
            if (!this._fileExtensions.Contains(CheckFileExtension(ext)))
            {
                this._fileExtensions.Add(CheckFileExtension(ext));
            }
        }

        private static String CheckFileExtension(String ext)
        {
            return ext.StartsWith(".") ? ext.Substring(1) : ext.ToLowerInvariant();
        }

        /// <summary>
        /// Gets whether any file extensions are associated with this syntax
        /// </summary>
        public bool HasFileExtensions
        {
            get { return this._canonicalExt != null || this._fileExtensions.Count > 0; }
        }

        /// <summary>
        /// Gets/Sets the Canonical File Extension for this Syntax
        /// </summary>
        public String CanonicalFileExtension
        {
            get
            {
                if (this._canonicalExt != null)
                {
                    return this._canonicalExt;
                }
                if (this._fileExtensions.Count > 0)
                {
                    return this._fileExtensions.First();
                }
                throw new RdfException("No File Extensions are defined for " + this.SyntaxName);
            }
            set
            {
                if (value == null)
                {
                    this._canonicalExt = null;
                }
                else if (this._fileExtensions.Contains(CheckFileExtension(value)))
                {
                    this._fileExtensions.Add(CheckFileExtension(value));
                }
                else
                {
                    throw new RdfException("Cannot set the Canonical File Extension for " + this.SyntaxName + " to " + value + " as this is no such File Extension listed in this definition.  Use AddFileExtension to add a File Extension prior to setting the CanonicalFileExtension.");
                }
            }
        }

        /// <summary>
        /// Determines whether the Definition supports a particular File Extension
        /// </summary>
        /// <param name="ext">File Extension</param>
        /// <returns></returns>
        public bool SupportsFileExtension(String ext)
        {
            ext = ext.ToLowerInvariant();
            if (ext.StartsWith(".")) ext = ext.Substring(1);
            return this._fileExtensions.Contains(ext);
        }

        #endregion

        #region Parser and Writer Management

        /// <summary>
        /// Ensures that a given Type implements a required Interface
        /// </summary>
        /// <param name="property">Property to which we are assigning</param>
        /// <param name="t">Type</param>
        /// <param name="interfaceType">Required Interface Type</param>
        private static bool EnsureInterface(String property, Type t, Type interfaceType)
        {
            if (t.GetInterfaces().All(itype => itype != interfaceType))
            {
                throw new RdfException("Cannot use Type " + t.FullName + " for the " + property + " Type as it does not implement the required interface " + interfaceType.FullName);
            }
            return true;
        }

        private bool EnsureObjectParserInterface(Type t, Type obj)
        {
            bool ok = t.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericArguments().First() == obj);
            if (!ok)
            {
                throw new RdfException("Cannot use Type " + t.FullName + " as an Object Parser for the Type " + obj.FullName + " as it does not implement the required interface IObjectParser<" + obj.Name + ">");
            }
            return true;
        }

        /// <summary>
        /// Gets/Sets the Type to use to parse RDF (or null if not applicable)
        /// </summary>
        public Type RdfParserType
        {
            get { return this._rdfParserType; }
            set
            {
                if (value == null)
                {
                    this._rdfParserType = null;
                }
                else if (EnsureInterface("RDF Parser", value, typeof (IRdfReader)))
                {
                    this._rdfParserType = value;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Type to use to writer RDF (or null if not applicable)
        /// </summary>
        public Type RdfWriterType
        {
            get { return this._rdfWriterType; }
            set
            {
                if (value == null)
                {
                    this._rdfWriterType = null;
                }
                else if (EnsureInterface("RDF Writer", value, typeof (IRdfWriter)))
                {
                    this._rdfWriterType = value;
                }
            }
        }

        /// <summary>
        /// Gets whether this definition can instantiate a Parser that can parse RDF
        /// </summary>
        public bool CanParseRdf
        {
            get { return (this._rdfParserType != null); }
        }

        /// <summary>
        /// Gets whether the definition provides a RDF Writer
        /// </summary>
        public bool CanWriteRdf
        {
            get { return (this._rdfWriterType != null); }
        }

        /// <summary>
        /// Gets an instance of a RDF parser
        /// </summary>
        /// <returns></returns>
        public IRdfReader GetRdfParser()
        {
            if (this._rdfParserType != null)
            {
                return (IRdfReader) Activator.CreateInstance(this._rdfParserType);
            }
            throw new RdfParserSelectionException("There is no RDF Parser available for the Syntax " + this.SyntaxName);
        }

        /// <summary>
        /// Gets an instance of a RDF writer
        /// </summary>
        /// <returns></returns>
        public IRdfWriter GetRdfWriter()
        {
            if (this._rdfWriterType != null)
            {
                return (IRdfWriter) Activator.CreateInstance(this._rdfWriterType);
            }
            throw new RdfWriterSelectionException("There is no RDF Writer available for the Syntax " + this.SyntaxName);
        }

        /// <summary>
        /// Gets whether a particular Type of Object can be parsed
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <returns></returns>
        public bool CanParseObject<T>()
        {
            return this._objectParserTypes.ContainsKey(typeof (T));
        }

        /// <summary>
        /// Gets an Object Parser for the given Type
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <returns></returns>
        public Type GetObjectParserType<T>()
        {
            Type t = typeof (T);
            Type result;
            return this._objectParserTypes.TryGetValue(t, out result) ? result : null;
        }

        /// <summary>
        /// Sets an Object Parser for the given Type
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="parserType">Parser Type</param>
        public void SetObjectParserType<T>(Type parserType)
        {
            Type t = typeof (T);
            if (this._objectParserTypes.ContainsKey(t))
            {
                if (parserType == null)
                {
                    this._objectParserTypes.Remove(t);
                }
                else if (this.EnsureObjectParserInterface(parserType, t))
                {
                    this._objectParserTypes[t] = parserType;
                }
            }
            else if (parserType != null)
            {
                if (this.EnsureObjectParserInterface(parserType, t))
                {
                    this._objectParserTypes.Add(t, parserType);
                }
            }
        }

        /// <summary>
        /// Gets an Object Parser for the given Type
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <returns></returns>
        public IObjectParser<T> GetObjectParser<T>()
        {
            if (!this._objectParserTypes.ContainsKey(typeof (T)))
            {
                throw new RdfParserSelectionException("There is no Object Parser available for the Type " + typeof (T).FullName);
            }
            Type parserType = this._objectParserTypes[typeof (T)];
            return (IObjectParser<T>) Activator.CreateInstance(parserType);
        }

        /// <summary>
        /// Gets the registered Object Parser Types
        /// </summary>
        public IEnumerable<KeyValuePair<Type, Type>> ObjectParserTypes
        {
            get { return this._objectParserTypes; }
        }

        #endregion

        public override string ToString()
        {
            return ToHttpHeader();
        }

        /// <summary>
        /// Gets the MIME types in HTTP Header format
        /// </summary>
        /// <returns>HTTP header format</returns>
        public String ToHttpHeader()
        {
            StringBuilder output = new StringBuilder();
            String canonicalType = this.CanonicalMimeType;
            output.Append(canonicalType);
            String quality = this.Quality < 1d ? ";q=" + this.Quality.ToString("g3") : String.Empty;
            output.Append(quality);
            // TODO Should we explicitly declare our desired character set here?
            foreach (String type in this.MimeTypes)
            {
                if (ReferenceEquals(canonicalType, type)) continue;
                output.Append(",");
                output.Append(type);
                output.Append(quality);
            }
            return output.ToString();
        }
    }

    /// <summary>
    /// Selector used in selecting which MIME type to use
    /// </summary>
    public sealed class MimeTypeSelector
        : IComparable<MimeTypeSelector>
    {
        /// <summary>
        /// Creates a MIME Type selector
        /// </summary>
        /// <param name="contentType">MIME Type</param>
        /// <param name="order">Order the selector appears in the input</param>
        /// <returns></returns>
        public static MimeTypeSelector Create(String contentType, int order)
        {
            if (!contentType.Contains(';'))
            {
                return new MimeTypeSelector(contentType.Trim().ToLowerInvariant(), null, 1.0d, order);
            }
            String[] parts = contentType.Split(';');
            String type = parts[0].Trim().ToLowerInvariant();

            double quality = 1.0d;
            String charset = null;
            for (int i = 1; i < parts.Length; i++)
            {
                String[] data = parts[i].Split('=');
                if (data.Length == 1) continue;
                switch (data[0].Trim().ToLowerInvariant())
                {
                    case "charset":
                        charset = data[1].Trim();
                        break;
                    case "q":
                        if (!Double.TryParse(data[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out quality))
                        {
                            quality = 1.0d;
                        }
                        break;
                }
            }

            return new MimeTypeSelector(type, charset, quality, order);
        }

        /// <summary>
        /// Creates an enumeration of MIME type selectors
        /// </summary>
        /// <param name="ctypes">MIME Types</param>
        /// <returns></returns>
        public static IEnumerable<MimeTypeSelector> CreateSelectors(IEnumerable<String> ctypes)
        {
            List<MimeTypeSelector> selectors = new List<MimeTypeSelector>();

            //Convert types into selectors
            if (ctypes != null)
            {
                int order = 1;
                foreach (String type in ctypes)
                {
                    selectors.Add(Create(type, order));
                    order++;
                }
            }

            //Adjust resulting selectors appropriately
            if (selectors.Count == 0)
            {
                //If no MIME types treat as if a single any selector
                selectors.Add(new MimeTypeSelector(IOManager.Any, null, 1.0d, 1));
            }
            else
            {
                //Sort the selectors
                selectors.Sort();
            }
            return selectors;
        }

        /// <summary>
        /// Creates a new MIME Type Selector
        /// </summary>
        /// <param name="type">MIME Type to match</param>
        /// <param name="charset">Charset</param>
        /// <param name="quality">Quality (in range 0.0-1.0)</param>
        /// <param name="order">Order of appearance (used as precendence tiebreaker where necessary)</param>
        public MimeTypeSelector(String type, String charset, double quality, int order)
        {
            IsSpecific = false;
            IsInvalid = false;
            IsRange = false;
            IsAny = false;
            Quality = 1.0d;
            if (type == null) throw new ArgumentNullException("type", "Type cannot be null");
            this.Type = type.Trim().ToLowerInvariant();
            this.Charset = charset != null ? charset.Trim() : null;
            this.Quality = quality;
            this.Order = order;

            //Validate parameters
            if (this.Quality < 0) this.Quality = 0;
            if (this.Quality > 1) this.Quality = 1;
            if (this.Order < 1) this.Order = 1;

            //Check what type of selector this is
            if (!IOManager.IsValidMimeType(this.Type))
            {
                //Invalid
                this.IsInvalid = true;
            }
            else if (this.Type.Equals(IOManager.Any))
            {
                //Is a */* any
                this.IsAny = true;
            }
            else if (this.Type.EndsWith("/*"))
            {
                //Is a blah/* range
                this.IsRange = true;
                this.RangeType = this.Type.Substring(0, this.Type.Length - 1);
            }
            else if (this.Type.Contains('*'))
            {
                //If it contains a * and is not */* or blah/* it is invalid
                this.IsInvalid = true;
            }
            else
            {
                //Must be a specific type
                this.IsSpecific = true;
            }
        }

        /// <summary>
        /// Gets the selected type
        /// </summary>
        /// <returns>A type string of the form <strong>type/subtype</strong> assuming the type if valid</returns>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the range type if this is a range selector
        /// </summary>
        /// <returns>A type string of the form <strong>type/</strong> if this is a range selector, otherwise null</returns>
        public string RangeType { get; private set; }

        /// <summary>
        /// Gets the Charset for the selector (may be null if none specified)
        /// </summary>
        public string Charset { get; private set; }

        /// <summary>
        /// Gets the quality for the selector (range of 0.0-1.0)
        /// </summary>
        public double Quality { get; private set; }

        /// <summary>
        /// Gets the order of apperance for the selector (used as precedence tiebreaker where necessary)
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets whether the selector if for a */* pattern i.e. accept any
        /// </summary>
        public bool IsAny { get; private set; }

        /// <summary>
        /// Gets whether the selector is for a type/* pattern i.e. accept any sub-type of the given type
        /// </summary>
        public bool IsRange { get; private set; }

        /// <summary>
        /// Gets whether the selector is invalid
        /// </summary>
        public bool IsInvalid { get; private set; }

        /// <summary>
        /// Gets whether the selector is for a specific MIME type e.g. type/sub-type
        /// </summary>
        public bool IsSpecific { get; private set; }

        /// <summary>
        /// Sorts the selector in precedence order according to the content negotiation rules from the relevant RFCs
        /// </summary>
        /// <param name="other">Selector to compare against</param>
        /// <returns></returns>
        public int CompareTo(MimeTypeSelector other)
        {
            if (other == null)
            {
                //We're always greater than a null
                return -1;
            }

            if (this.IsInvalid)
            {
                return other.IsInvalid ? this.Order.CompareTo(other.Order) : 1;
            }
            if (other.IsInvalid)
            {
                //Valid types are greater than invalid types
                return -1;
            }

            int c;
            if (this.IsAny)
            {
                if (other.IsAny)
                {
                    //If both Any use quality
                    c = -1*this.Quality.CompareTo(other.Quality);
                    if (c == 0)
                    {
                        //If same quality use order
                        c = this.Order.CompareTo(other.Order);
                    }
                    return c;
                }
                //Any is less than range/specific type
                return 1;
            }
            if (this.IsRange)
            {
                if (other.IsAny)
                {
                    //Range types are greater than Any
                    return -1;
                }
                if (other.IsRange)
                {
                    //If both Range use quality
                    c = -1*this.Quality.CompareTo(other.Quality);
                    if (c == 0)
                    {
                        //If same quality use order
                        c = this.Order.CompareTo(other.Order);
                    }
                    return c;
                }
                //Range is less that specific type
                return 1;
            }
            if (other.IsAny || other.IsRange)
            {
                //Specific types are greater than Any/Range
                return -1;
            }
            //Both specific so use quality
            c = -1*this.Quality.CompareTo(other.Quality);
            if (c == 0)
            {
                //If same quality use order
                c = this.Order.CompareTo(other.Order);
            }
            return c;
        }

        /// <summary>
        /// Gets the string representation of the selector as it would appear in an Accept header
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Unless this is an invalid selector this will always be a valid selector that could be appended to a MIME type header
        /// </remarks>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Type);
            if (this.Quality < 1.0d)
            {
                builder.Append("; q=" + this.Quality.ToString("g3"));
            }
            if (this.Charset != null)
            {
                builder.Append("; charset=" + this.Charset);
            }
            return builder.ToString();
        }
    }
}