/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    /// <summary>
    /// Represents the definition of a MIME Type including mappings to relevant readers and writers
    /// </summary>
    public sealed class MimeTypeDefinition
    {
        private String _name, _canonicalType, _canonicalExt, _formatUri;
        private Encoding _encoding = Encoding.UTF8;
        private List<String> _mimeTypes = new List<string>();
        private List<String> _fileExtensions = new List<string>();
        private Type _rdfParserType, _rdfDatasetParserType, _sparqlResultsParserType;
        private Type _rdfWriterType, _rdfDatasetWriterType, _sparqlResultsWriterType;
        private Dictionary<Type, Type> _objectParserTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// Creates a new MIME Type Definition
        /// </summary>
        /// <param name="syntaxName">Syntax Name for the Syntax which has this MIME Type definition</param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        public MimeTypeDefinition(String syntaxName, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions)
        {
            if (mimeTypes == null) throw new ArgumentNullException("MIME Types enumeration cannot be null");
            _name = syntaxName;
            _mimeTypes.AddRange(mimeTypes.Select(t => CheckValidMimeType(t)));

            foreach (String ext in fileExtensions)
            {
                _fileExtensions.Add(CheckFileExtension(ext));
            }
        }

        /// <summary>
        /// Creates a new MIME Type Definition
        /// </summary>
        /// <param name="syntaxName">Syntax Name for the Syntax which has this MIME Type definition</param>
        /// <param name="formatUri">Format URI as defined by the <a href="http://www.w3.org/ns/formats/">W3C</a></param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        public MimeTypeDefinition(String syntaxName, String formatUri, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions)
            : this(syntaxName, mimeTypes, fileExtensions)
        {
            _formatUri = formatUri;
        }

        /// <summary>
        /// Creates a new MIME Type Definition
        /// </summary>
        /// <param name="syntaxName">Syntax Name for the Syntax which has this MIME Type definition</param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        /// <param name="rdfParserType">Type to use to parse RDF (or null if not applicable)</param>
        /// <param name="rdfDatasetParserType">Type to use to parse RDF Datasets (or null if not applicable)</param>
        /// <param name="sparqlResultsParserType">Type to use to parse SPARQL Results (or null if not applicable)</param>
        /// <param name="rdfWriterType">Type to use to writer RDF (or null if not applicable)</param>
        /// <param name="rdfDatasetWriterType">Type to use to write RDF Datasets (or null if not applicable)</param>
        /// <param name="sparqlResultsWriterType">Type to use to write SPARQL Results (or null if not applicable)</param>
        public MimeTypeDefinition(String syntaxName, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions, Type rdfParserType, Type rdfDatasetParserType, Type sparqlResultsParserType, Type rdfWriterType, Type rdfDatasetWriterType, Type sparqlResultsWriterType)
            : this(syntaxName, mimeTypes, fileExtensions)
        {
            RdfParserType = rdfParserType;
            RdfDatasetParserType = rdfDatasetParserType;
            SparqlResultsParserType = sparqlResultsParserType;
            RdfWriterType = rdfWriterType;
            RdfDatasetWriterType = rdfDatasetWriterType;
            SparqlResultsWriterType = sparqlResultsWriterType;
        }

        /// <summary>
        /// Creates a new MIME Type Definition
        /// </summary>
        /// <param name="syntaxName">Syntax Name for the Syntax which has this MIME Type definition</param>
        /// <param name="formatUri">Format URI as defined by the <a href="http://www.w3.org/ns/formats/">W3C</a></param>
        /// <param name="mimeTypes">MIME Types</param>
        /// <param name="fileExtensions">File Extensions</param>
        /// <param name="rdfParserType">Type to use to parse RDF (or null if not applicable)</param>
        /// <param name="rdfDatasetParserType">Type to use to parse RDF Datasets (or null if not applicable)</param>
        /// <param name="sparqlResultsParserType">Type to use to parse SPARQL Results (or null if not applicable)</param>
        /// <param name="rdfWriterType">Type to use to writer RDF (or null if not applicable)</param>
        /// <param name="rdfDatasetWriterType">Type to use to write RDF Datasets (or null if not applicable)</param>
        /// <param name="sparqlResultsWriterType">Type to use to write SPARQL Results (or null if not applicable)</param>
        public MimeTypeDefinition(String syntaxName, String formatUri, IEnumerable<String> mimeTypes, IEnumerable<String> fileExtensions, Type rdfParserType, Type rdfDatasetParserType, Type sparqlResultsParserType, Type rdfWriterType, Type rdfDatasetWriterType, Type sparqlResultsWriterType)
            : this(syntaxName, mimeTypes, fileExtensions, rdfParserType, rdfDatasetParserType, sparqlResultsParserType, rdfWriterType, rdfDatasetWriterType, sparqlResultsWriterType)
        {
            _formatUri = formatUri;
        }

        /// <summary>
        /// Gets the name of the Syntax to which this MIME Type Definition relates
        /// </summary>
        public String SyntaxName
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the Format URI as defined by the <a href="http://www.w3.org/ns/formats/">W3C</a> (where applicable)
        /// </summary>
        public String FormatUri
        {
            get
            {
                return _formatUri;
            }
        }

        /// <summary>
        /// Gets the Encoding that should be used for reading and writing this Syntax
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                if (_encoding != null)
                {
                    return _encoding;
                }
                else
                {
                    return Encoding.UTF8;
                }
            }
            set
            {
                _encoding = value;
            }
        }

        #region MIME Type Management

        /// <summary>
        /// Gets the MIME Types defined
        /// </summary>
        public IEnumerable<String> MimeTypes
        {
            get
            {
                return _mimeTypes;
            }
        }

        /// <summary>
        /// Checks that MIME Types are valid
        /// </summary>
        /// <param name="type">Type</param>
        public String CheckValidMimeType(String type)
        {
            type = type.Trim().ToLowerInvariant();
            if (!MimeTypesHelper.IsValidMimeType(type))
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
            if (!_mimeTypes.Contains(CheckValidMimeType(type)))
            {
                _mimeTypes.Add(CheckValidMimeType(type));
            }
        }

        /// <summary>
        /// Gets the Canonical MIME Type that should be used
        /// </summary>
        public String CanonicalMimeType
        {
            get
            {
                if (_canonicalType != null)
                {
                    return _canonicalType;
                }
                else if (_mimeTypes.Count > 0)
                {
                    return _mimeTypes.First();
                }
                else
                {
                    throw new RdfException("No MIME Types are defined for " + _name);
                }
            }
            set
            {
                if (value == null)
                {
                    _canonicalType = value;
                }
                else if (_mimeTypes.Contains(value))
                {
                    _canonicalType = value;
                }
                else
                {
                    throw new RdfException("Cannot set the Canonical MIME Type for " + _name + " to " + value + " as this is no such MIME Type listed in this definition.  Use AddMimeType to add a MIME Type prior to setting the CanonicalType.");
                }
            }
        }

        /// <summary>
        /// Determines whether the Definition supports a particular MIME type
        /// </summary>
        /// <param name="mimeType">MIME Type</param>
        /// <returns></returns>
        [Obsolete("Deprecated in favour of the alternative overload which takes a MimeTypeSelector", false)]
        public bool SupportsMimeType(String mimeType)
        {
            String type = mimeType.ToLowerInvariant();
            type = type.Contains(';') ? type.Substring(0, type.IndexOf(';')) : type;
            return _mimeTypes.Contains(type) || mimeType.Equals(MimeTypesHelper.Any);
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
            if (selector.IsRange)
            {
                if (selector.RangeType == null) return false;
                return _mimeTypes.Any(type => type.StartsWith(selector.RangeType));
            }
            else
            {
                return _mimeTypes.Contains(selector.Type);
            }
        }

        #endregion

        #region File Extension Management

        /// <summary>
        /// Gets the File Extensions associated with this Syntax
        /// </summary>
        public IEnumerable<String> FileExtensions
        {
            get
            {
                return _fileExtensions;
            }
        }

        /// <summary>
        /// Adds a File Extension for this Syntax
        /// </summary>
        /// <param name="ext">File Extension</param>
        public void AddFileExtension(String ext)
        {
            if (!_fileExtensions.Contains(CheckFileExtension(ext)))
            {
                _fileExtensions.Add(CheckFileExtension(ext));
            }
        }

        private String CheckFileExtension(String ext)
        {
            if (ext.StartsWith(".")) return ext.Substring(1);
            return ext.ToLowerInvariant();
        }

        /// <summary>
        /// Gets whether any file extensions are associated with this syntax
        /// </summary>
        public bool HasFileExtensions
        {
            get
            {
                return _canonicalExt != null || _fileExtensions.Count > 0;
            }
        }

        /// <summary>
        /// Gets/Sets the Canonical File Extension for this Syntax
        /// </summary>
        public String CanonicalFileExtension
        {
            get
            {
                if (_canonicalExt != null)
                {
                    return _canonicalExt;
                }
                else if (_fileExtensions.Count > 0)
                {
                    return _fileExtensions.First();
                }
                else
                {
                    throw new RdfException("No File Extensions are defined for " + _name);
                }
            }
            set
            {
                if (value == null)
                {
                    _canonicalExt = value;
                }
                else if (_fileExtensions.Contains(CheckFileExtension(value)))
                {
                    _fileExtensions.Add(CheckFileExtension(value));
                } 
                else 
                {
                    throw new RdfException("Cannot set the Canonical File Extension for " + _name + " to " + value + " as this is no such File Extension listed in this definition.  Use AddFileExtension to add a File Extension prior to setting the CanonicalFileExtension.");
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
            return _fileExtensions.Contains(ext);
        }

        #endregion

        #region Parser and Writer Management

        /// <summary>
        /// Ensures that a given Type implements a required Interface
        /// </summary>
        /// <param name="property">Property to which we are assigning</param>
        /// <param name="t">Type</param>
        /// <param name="interfaceType">Required Interface Type</param>
        private bool EnsureInterface(String property, Type t, Type interfaceType)
        {
            if (!t.GetInterfaces().Any(itype => itype.Equals(interfaceType)))
            {
                throw new RdfException("Cannot use Type " + t.FullName + " for the " + property + " Type as it does not implement the required interface " + interfaceType.FullName);
            }
            else
            {
                return true;
            }
        }

        private bool EnsureObjectParserInterface(Type t, Type obj)
        {
            bool ok = false;
            foreach (Type i in t.GetInterfaces())
            {
#if NETCORE
                if(i.IsGenericType())
#else
                if (i.IsGenericType)
#endif
                {
                    if (i.GetGenericArguments().First().Equals(obj))
                    {
                        ok = true;
                        break;
                    }
                }
            }
            if (!ok)
            {
                throw new RdfException("Cannot use Type " + t.FullName + " as an Object Parser for the Type " + obj.FullName + " as it does not implement the required interface IObjectParser<" + obj.Name + ">");
            }
            return ok;
        }

        /// <summary>
        /// Gets/Sets the Type to use to parse RDF (or null if not applicable)
        /// </summary>
        public Type RdfParserType
        {
            get
            {
                return _rdfParserType;
            }
            set
            {
                if (value == null)
                {
                    _rdfParserType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Parser", value, typeof(IRdfReader)))
                    {
                        _rdfParserType = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Type to use to parse RDF Datasets (or null if not applicable)
        /// </summary>
        public Type RdfDatasetParserType
        {
            get
            {
                return _rdfDatasetParserType;
            }
            set
            {
                if (value == null)
                {
                    _rdfDatasetParserType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Dataset Parser", value, typeof(IStoreReader)))
                    {
                        _rdfDatasetParserType = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Type to use to parse SPARQL Results (or null if not applicable)
        /// </summary>
        public Type SparqlResultsParserType
        {
            get
            {
                return _sparqlResultsParserType;
            }
            set
            {
                if (value == null)
                {
                    _sparqlResultsParserType = value;
                }
                else
                {
                    if (EnsureInterface("SPARQL Results Parser", value, typeof(ISparqlResultsReader)))
                    {
                        _sparqlResultsParserType = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Type to use to writer RDF (or null if not applicable)
        /// </summary>
        public Type RdfWriterType
        {
            get
            {
                return _rdfWriterType;
            }
            set
            {
                if (value == null)
                {
                    _rdfWriterType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Writer", value, typeof(IRdfWriter)))
                    {
                        _rdfWriterType = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Type to use to writer RDF Dataets (or null if not applicable)
        /// </summary>
        public Type RdfDatasetWriterType
        {
            get
            {
                return _rdfDatasetWriterType;
            }
            set
            {
                if (value == null)
                {
                    _rdfDatasetWriterType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Dataset Writer", value, typeof(IStoreWriter)))
                    {
                        _rdfDatasetWriterType = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the Type to use to write SPARQL Results (or null if not applicable)
        /// </summary>
        public Type SparqlResultsWriterType
        {
            get
            {
                return _sparqlResultsWriterType;
            }
            set
            {
                if (value == null)
                {
                    _sparqlResultsWriterType = value;
                }
                else
                {
                    if (EnsureInterface("SPARQL Results Writer", value, typeof(ISparqlResultsWriter)))
                    {
                        _sparqlResultsWriterType = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether this definition can instantiate a Parser that can parse RDF
        /// </summary>
        public bool CanParseRdf
        {
            get
            {
                return (_rdfParserType != null);
            }
        }

        /// <summary>
        /// Gets whether this definition can instantiate a Parser that can parse RDF Datasets
        /// </summary>
        public bool CanParseRdfDatasets
        {
            get
            {
                return (_rdfDatasetParserType != null);
            }
        }

        /// <summary>
        /// Gets whether this definition can instantiate a Parser that can parse SPARQL Results
        /// </summary>
        public bool CanParseSparqlResults
        {
            get
            {
                return (_sparqlResultsParserType != null);
            }
        }

        /// <summary>
        /// Gets whether the definition provides a RDF Writer
        /// </summary>
        public bool CanWriteRdf
        {
            get
            {
                return (_rdfWriterType != null);
            }
        }

        /// <summary>
        /// Gets whether the Definition provides a RDF Dataset Writer
        /// </summary>
        public bool CanWriteRdfDatasets
        {
            get
            {
                return (_rdfDatasetWriterType != null);
            }
        }

        /// <summary>
        /// Gets whether the Definition provides a SPARQL Results Writer
        /// </summary>
        public bool CanWriteSparqlResults
        {
            get
            {
                return (_sparqlResultsWriterType != null);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF parser
        /// </summary>
        /// <returns></returns>
        public IRdfReader GetRdfParser()
        {
            if (_rdfParserType != null)
            {
                return (IRdfReader)Activator.CreateInstance(_rdfParserType);
            }
            else
            {
                throw new RdfParserSelectionException("There is no RDF Parser available for the Syntax " + _name);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF writer
        /// </summary>
        /// <returns></returns>
        public IRdfWriter GetRdfWriter()
        {
            if (_rdfWriterType != null)
            {
                return (IRdfWriter)Activator.CreateInstance(_rdfWriterType);
            }
            else
            {
                throw new RdfWriterSelectionException("There is no RDF Writer available for the Syntax " + _name);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF Dataset parser
        /// </summary>
        /// <returns></returns>
        public IStoreReader GetRdfDatasetParser()
        {
            if (_rdfDatasetParserType != null)
            {
                return (IStoreReader)Activator.CreateInstance(_rdfDatasetParserType);
            }
            else
            {
                throw new RdfParserSelectionException("There is no RDF Dataset Parser available for the Syntax " + _name);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF Dataset writer
        /// </summary>
        /// <returns></returns>
        public IStoreWriter GetRdfDatasetWriter()
        {
            if (_rdfDatasetWriterType != null)
            {
                return (IStoreWriter)Activator.CreateInstance(_rdfDatasetWriterType);
            }
            else
            {
                throw new RdfWriterSelectionException("There is no RDF Dataset Writer available for the Syntax " + _name);
            }
        }

        /// <summary>
        /// Gets an instance of a SPARQL Results parser
        /// </summary>
        /// <returns></returns>
        public ISparqlResultsReader GetSparqlResultsParser()
        {
            if (_sparqlResultsParserType != null)
            {
                return (ISparqlResultsReader)Activator.CreateInstance(_sparqlResultsParserType);
            }
            else if (_rdfParserType != null)
            {
                return new SparqlRdfParser((IRdfReader)Activator.CreateInstance(_rdfParserType));
            }
            else
            {
                throw new RdfParserSelectionException("There is no SPARQL Results Parser available for the Syntax " + _name);
            }
        }

        /// <summary>
        /// Gets an instance of a SPARQL Results writer
        /// </summary>
        /// <returns></returns>
        public ISparqlResultsWriter GetSparqlResultsWriter()
        {
            if (_sparqlResultsWriterType != null)
            {
                return (ISparqlResultsWriter)Activator.CreateInstance(_sparqlResultsWriterType);
            }
            else if (_rdfWriterType != null)
            {
                return new SparqlRdfWriter((IRdfWriter)Activator.CreateInstance(_rdfWriterType));
            }
            else
            {
                throw new RdfWriterSelectionException("There is no SPARQL Results Writer available for the Syntax " + _name);
            }
        }

        /// <summary>
        /// Gets whether a particular Type of Object can be parsed
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <returns></returns>
        public bool CanParseObject<T>()
        {
            return _objectParserTypes.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Gets an Object Parser for the given Type
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <returns></returns>
        public Type GetObjectParserType<T>()
        {
            Type t = typeof(T);
            Type result;
            if (_objectParserTypes.TryGetValue(t, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets an Object Parser for the given Type
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <param name="parserType">Parser Type</param>
        public void SetObjectParserType<T>(Type parserType)
        {
            Type t = typeof(T);
            if (_objectParserTypes.ContainsKey(t))
            {
                if (parserType == null)
                {
                    _objectParserTypes.Remove(t);
                }
                else
                {
                    if (EnsureObjectParserInterface(parserType, t))
                    {
                        _objectParserTypes[t] = parserType;
                    }
                }
            }
            else if (parserType != null)
            {
                if (EnsureObjectParserInterface(parserType, t))
                {
                    _objectParserTypes.Add(t, parserType);
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
            if (_objectParserTypes.ContainsKey(typeof(T)))
            {
                Type parserType = _objectParserTypes[typeof(T)];
                return (IObjectParser<T>)Activator.CreateInstance(parserType);
            }
            else
            {
                throw new RdfParserSelectionException("There is no Object Parser available for the Type " + typeof(T).FullName);
            }
        }

        /// <summary>
        /// Gets the registered Object Parser Types
        /// </summary>
        public IEnumerable<KeyValuePair<Type, Type>> ObjectParserTypes
        {
            get
            {
                return _objectParserTypes;
            }
        }

#endregion

    }

    /// <summary>
    /// Selector used in selecting which MIME type to use
    /// </summary>
    public sealed class MimeTypeSelector
        : IComparable<MimeTypeSelector>
    {
        private String _type, _rangeType, _charset;
        private double _quality = 1.0d;
        private int _order;
        private bool _isSpecific = false, _isRange = false, _isAny = false, _isInvalid = false;

        /// <summary>
        /// Creates a MIME Type selector
        /// </summary>
        /// <param name="contentType">MIME Type</param>
        /// <param name="order">Order the selector appears in the input</param>
        /// <returns></returns>
        public static MimeTypeSelector Create(String contentType, int order)
        {
            if (contentType.Contains(';'))
            {
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
            else
            {
                return new MimeTypeSelector(contentType.Trim().ToLowerInvariant(), null, 1.0d, order);
            }
        }

        /// <summary>
        /// Creates an enumeration of MIME type selectors
        /// </summary>
        /// <param name="ctypes">MIME Types</param>
        /// <returns></returns>
        public static IEnumerable<MimeTypeSelector> CreateSelectors(IEnumerable<String> ctypes)
        {
            List<MimeTypeSelector> selectors = new List<MimeTypeSelector>();

            // Convert types into selectors
            if (ctypes != null)
            {
                int order = 1;
                foreach (String type in ctypes)
                {
                    selectors.Add(Create(type, order));
                    order++;
                }
            }

            // Adjust resulting selectors appropriately
            if (selectors.Count == 0)
            {
                // If no MIME types treat as if a single any selector
                selectors.Add(new MimeTypeSelector(MimeTypesHelper.Any, null, 1.0d, 1));
            }
            else
            {
                // Sort the selectors
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
            if (type == null) throw new ArgumentNullException("type", "Type cannot be null");
            _type = type.Trim().ToLowerInvariant();
            _charset = charset != null ? charset.Trim() : null;
            _quality = quality;
            _order = order;

            // Validate parameters
            if (_quality < 0) _quality = 0;
            if (_quality > 1) _quality = 1;
            if (_order < 1) _order = 1;

            // Check what type of selector this is
            if (!MimeTypesHelper.IsValidMimeType(_type))
            {
                // Invalid
                _isInvalid = true;
            }
            else if (_type.Equals(MimeTypesHelper.Any))
            {
                // Is a */* any
                _isAny = true;
            }
            else if (_type.EndsWith("/*"))
            {
                // Is a blah/* range
                _isRange = true;
                _rangeType = _type.Substring(0, _type.Length - 1);
            }
            else if (_type.Contains('*'))
            {
                // If it contains a * and is not */* or blah/* it is invalid
                _isInvalid = true;
            }
            else
            {
                // Must be a specific type
                _isSpecific = true;
            }
        }

        /// <summary>
        /// Gets the selected type
        /// </summary>
        /// <returns>A type string of the form <strong>type/subtype</strong> assuming the type if valid</returns>
        public String Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the range type if this is a range selector
        /// </summary>
        /// <returns>A type string of the form <strong>type/</strong> if this is a range selector, otherwise null</returns>
        public String RangeType
        {
            get
            {
                return _rangeType;
            }
        }

        /// <summary>
        /// Gets the Charset for the selector (may be null if none specified)
        /// </summary>
        public String Charset
        {
            get
            {
                return _charset;
            }
        }

        /// <summary>
        /// Gets the quality for the selector (range of 0.0-1.0)
        /// </summary>
        public double Quality
        {
            get
            {
                return _quality;
            }
        }

        /// <summary>
        /// Gets the order of apperance for the selector (used as precedence tiebreaker where necessary)
        /// </summary>
        public int Order
        {
            get
            {
                return _order;
            }
        }

        /// <summary>
        /// Gets whether the selector if for a */* pattern i.e. accept any
        /// </summary>
        public bool IsAny
        {
            get
            {
                return _isAny;
            }
        }

        /// <summary>
        /// Gets whether the selector is for a type/* pattern i.e. accept any sub-type of the given type
        /// </summary>
        public bool IsRange
        {
            get
            {
                return _isRange;
            }
        }

        /// <summary>
        /// Gets whether the selector is invalid
        /// </summary>
        public bool IsInvalid
        {
            get
            {
                return _isInvalid;
            }
        }

        /// <summary>
        /// Gets whether the selector is for a specific MIME type e.g. type/sub-type
        /// </summary>
        public bool IsSpecific
        {
            get
            {
                return _isSpecific;
            }
        }

        /// <summary>
        /// Sorts the selector in precedence order according to the content negotiation rules from the relevant RFCs
        /// </summary>
        /// <param name="other">Selector to compare against</param>
        /// <returns></returns>
        public int CompareTo(MimeTypeSelector other)
        {
            if (other == null)
            {
                // We're always greater than a null
                return -1;
            }

            if (_isInvalid)
            {
                if (other.IsInvalid)
                {
                    // If both invalid use order
                    return Order.CompareTo(other.Order);
                }
                else
                {
                    // Invalid types are less than valid types
                    return 1;
                }
            }
            else if (other.IsInvalid)
            {
                // Valid types are greater than invalid types
                return -1;
            }

            if (_isAny)
            {
                if (other.IsAny)
                {
                    // If both Any use quality
                    int c = -1 * Quality.CompareTo(other.Quality);
                    if (c == 0)
                    {
                        // If same quality use order
                        c = Order.CompareTo(other.Order);
                    }
                    return c;
                }
                else
                {
                    // Any is less than range/specific type
                    return 1;
                }
            }
            else if (_isRange)
            {
                if (other.IsAny)
                {
                    // Range types are greater than Any
                    return -1;
                }
                else if (other.IsRange)
                {
                    // If both Range use quality
                    int c = -1 * Quality.CompareTo(other.Quality);
                    if (c == 0)
                    {
                        // If same quality use order
                        c = Order.CompareTo(other.Order);
                    }
                    return c;
                }
                else
                {
                    // Range is less that specific type
                    return 1;
                }
            }
            else
            {
                if (other.IsAny || other.IsRange)
                {
                    // Specific types are greater than Any/Range
                    return -1;
                }
                else
                {
                    // Both specific so use quality
                    int c = -1 * Quality.CompareTo(other.Quality);
                    if (c == 0)
                    {
                        // If same quality use order
                        c = Order.CompareTo(other.Order);
                    }
                    return c;
                }
            }
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
            builder.Append(_type);
            if (_quality != 1.0d)
            {
                builder.Append("; q=" + _quality.ToString("g3"));
            }
            if (_charset != null)
            {
                builder.Append("; charset=" + _charset);
            }
            return builder.ToString();
        }
    }
}
