/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF
{
    /// <summary>
    /// Represents the definition of a MIME Type including mappings to relevant readers and writers
    /// </summary>
    public class MimeTypeDefinition
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
            this._name = syntaxName;
            foreach (String type in mimeTypes)
            {
                this.CheckValidMimeType(type);
            }
            this._mimeTypes.AddRange(mimeTypes);

            foreach (String ext in fileExtensions)
            {
                this._fileExtensions.Add(this.CheckFileExtension(ext));
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
            this._formatUri = formatUri;
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
            this.RdfParserType = rdfParserType;
            this.RdfDatasetParserType = rdfDatasetParserType;
            this.SparqlResultsParserType = sparqlResultsParserType;
            this.RdfWriterType = rdfWriterType;
            this.RdfDatasetWriterType = rdfDatasetWriterType;
            this.SparqlResultsWriterType = sparqlResultsWriterType;
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
            this._formatUri = formatUri;
        }

        /// <summary>
        /// Gets the name of the Syntax to which this MIME Type Definition relates
        /// </summary>
        public String SyntaxName
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets the Format URI as defined by the <a href="http://www.w3.org/ns/formats/">W3C</a> (where applicable)
        /// </summary>
        public String FormatUri
        {
            get
            {
                return this._formatUri;
            }
        }

        /// <summary>
        /// Gets the Encoding that should be used for reading and writing this Syntax
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                if (this._encoding != null)
                {
                    return this._encoding;
                }
                else
                {
                    return Encoding.UTF8;
                }
            }
            set
            {
                this._encoding = value;
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
                return this._mimeTypes;
            }
        }

        /// <summary>
        /// Checks that MIME Types are valid
        /// </summary>
        /// <param name="type">Type</param>
        public void CheckValidMimeType(String type)
        {
            if (!Regex.IsMatch(type, MimeTypesHelper.ValidMimeTypePattern))
            {
                throw new RdfException(type + " is not a valid MIME Type");
            }
        }

        /// <summary>
        /// Adds a MIME Type to this definition
        /// </summary>
        /// <param name="type">MIME Type</param>
        public void AddMimeType(String type)
        {
            if (!this._mimeTypes.Contains(type))
            {
                this._mimeTypes.Add(type);
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
                else if (this._mimeTypes.Count > 0)
                {
                    return this._mimeTypes.First();
                }
                else
                {
                    throw new RdfException("No MIME Types are defined for " + this._name);
                }
            }
            set
            {
                if (value == null)
                {
                    this._canonicalType = value;
                }
                else if (this._mimeTypes.Contains(value))
                {
                    this._canonicalType = value;
                }
                else
                {
                    throw new RdfException("Cannot set the Canonical MIME Type for " + this._name + " to " + value + " as this is no such MIME Type listed in this definition.  Use AddMimeType to add a MIME Type prior to setting the CanonicalType.");
                }
            }
        }

        /// <summary>
        /// Determines whether the Definition supports a particular MIME type
        /// </summary>
        /// <param name="mimeType">MIME Type</param>
        /// <returns></returns>
        public bool SupportsMimeType(String mimeType)
        {
            return this._mimeTypes.Contains(mimeType) || mimeType.Equals(MimeTypesHelper.Any);
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
                return this._fileExtensions;
            }
        }

        /// <summary>
        /// Adds a File Extension for this Syntax
        /// </summary>
        /// <param name="ext">File Extension</param>
        public void AddFileExtension(String ext)
        {
            if (!this._fileExtensions.Contains(this.CheckFileExtension(ext)))
            {
                this._fileExtensions.Add(this.CheckFileExtension(ext));
            }
        }

        private String CheckFileExtension(String ext)
        {
            if (ext.StartsWith(".")) return ext.Substring(1);
            return ext;
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
                else if (this._fileExtensions.Count > 0)
                {
                    return this._fileExtensions.First();
                }
                else
                {
                    throw new RdfException("No File Extensions are defined for " + this._name);
                }
            }
            set
            {
                if (value == null)
                {
                    this._canonicalExt = value;
                }
                else if (this._fileExtensions.Contains(this.CheckFileExtension(value)))
                {
                    this._fileExtensions.Add(this.CheckFileExtension(value));
                } 
                else 
                {
                    throw new RdfException("Cannot set the Canonical File Extension for " + this._name + " to " + value + " as this is no such File Extension listed in this definition.  Use AddFileExtension to add a File Extension prior to setting the CanonicalFileExtension.");
                }
            }
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
                if (i.IsGenericType)
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
                return this._rdfParserType;
            }
            set
            {
                if (value == null)
                {
                    this._rdfParserType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Parser", value, typeof(IRdfReader)))
                    {
                        this._rdfParserType = value;
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
                return this._rdfDatasetParserType;
            }
            set
            {
                if (value == null)
                {
                    this._rdfDatasetParserType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Dataset Parser", value, typeof(IStoreReader)))
                    {
                        this._rdfDatasetParserType = value;
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
                return this._sparqlResultsParserType;
            }
            set
            {
                if (value == null)
                {
                    this._sparqlResultsParserType = value;
                }
                else
                {
                    if (EnsureInterface("SPARQL Results Parser", value, typeof(ISparqlResultsReader)))
                    {
                        this._sparqlResultsParserType = value;
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
                return this._rdfWriterType;
            }
            set
            {
                if (value == null)
                {
                    this._rdfWriterType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Writer", value, typeof(IRdfWriter)))
                    {
                        this._rdfWriterType = value;
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
                return this._rdfDatasetWriterType;
            }
            set
            {
                if (value == null)
                {
                    this._rdfDatasetWriterType = value;
                }
                else
                {
                    if (EnsureInterface("RDF Dataset Writer", value, typeof(IStoreWriter)))
                    {
                        this._rdfDatasetWriterType = value;
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
                return this._sparqlResultsWriterType;
            }
            set
            {
                if (value == null)
                {
                    this._sparqlResultsWriterType = value;
                }
                else
                {
                    if (EnsureInterface("SPARQL Results Writer", value, typeof(ISparqlResultsWriter)))
                    {
                        this._sparqlResultsWriterType = value;
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
                return (this._rdfParserType != null);
            }
        }

        /// <summary>
        /// Gets whether this definition can instantiate a Parser that can parse RDF Datasets
        /// </summary>
        public bool CanParseRdfDatasets
        {
            get
            {
                return (this._rdfDatasetParserType != null);
            }
        }

        /// <summary>
        /// Gets whether this definition can instantiate a Parser that can parse SPARQL Results
        /// </summary>
        public bool CanParseSparqlResults
        {
            get
            {
                return (this._sparqlResultsParserType != null);
            }
        }

        /// <summary>
        /// Gets whether the definition provides a RDF Writer
        /// </summary>
        public bool CanWriteRdf
        {
            get
            {
                return (this._rdfWriterType != null);
            }
        }

        /// <summary>
        /// Gets whether the Definition provides a RDF Dataset Writer
        /// </summary>
        public bool CanWriteRdfDatasets
        {
            get
            {
                return (this._rdfDatasetWriterType != null);
            }
        }

        /// <summary>
        /// Gets whether the Definition provides a SPARQL Results Writer
        /// </summary>
        public bool CanWriteSparqlResults
        {
            get
            {
                return (this._sparqlResultsWriterType != null);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF parser
        /// </summary>
        /// <returns></returns>
        public IRdfReader GetRdfParser()
        {
            if (this._rdfParserType != null)
            {
                return (IRdfReader)Activator.CreateInstance(this._rdfParserType);
            }
            else
            {
                throw new RdfParserSelectionException("There is no RDF Parser available for the Syntax " + this._name);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF writer
        /// </summary>
        /// <returns></returns>
        public IRdfWriter GetRdfWriter()
        {
            if (this._rdfWriterType != null)
            {
                return (IRdfWriter)Activator.CreateInstance(this._rdfWriterType);
            }
            else
            {
                throw new RdfWriterSelectionException("There is no RDF Writer available for the Syntax " + this._name);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF Dataset parser
        /// </summary>
        /// <returns></returns>
        public IStoreReader GetRdfDatasetParser()
        {
            if (this._rdfDatasetParserType != null)
            {
                return (IStoreReader)Activator.CreateInstance(this._rdfDatasetParserType);
            }
            else
            {
                throw new RdfParserSelectionException("There is no RDF Dataset Parser available for the Syntax " + this._name);
            }
        }

        /// <summary>
        /// Gets an instance of a RDF Dataset writer
        /// </summary>
        /// <returns></returns>
        public IStoreWriter GetRdfDatasetWriter()
        {
            if (this._rdfDatasetWriterType != null)
            {
                return (IStoreWriter)Activator.CreateInstance(this._rdfDatasetWriterType);
            }
            else
            {
                throw new RdfWriterSelectionException("There is no RDF Dataset Writer available for the Syntax " + this._name);
            }
        }

        /// <summary>
        /// Gets an instance of a SPARQL Results parser
        /// </summary>
        /// <returns></returns>
        public ISparqlResultsReader GetSparqlResultsParser()
        {
            if (this._sparqlResultsParserType != null)
            {
                return (ISparqlResultsReader)Activator.CreateInstance(this._sparqlResultsParserType);
            }
            else if (this._rdfParserType != null)
            {
                return new SparqlRdfParser((IRdfReader)Activator.CreateInstance(this._rdfParserType));
            }
            else
            {
                throw new RdfParserSelectionException("There is no SPARQL Results Parser available for the Syntax " + this._name);
            }
        }

        /// <summary>
        /// Gets an instance of a SPARQL Results writer
        /// </summary>
        /// <returns></returns>
        public ISparqlResultsWriter GetSparqlResultsWriter()
        {
            if (this._sparqlResultsWriterType != null)
            {
                return (ISparqlResultsWriter)Activator.CreateInstance(this._sparqlResultsWriterType);
            }
            else if (this._rdfWriterType != null)
            {
                return new SparqlRdfWriter((IRdfWriter)Activator.CreateInstance(this._rdfWriterType));
            }
            else
            {
                throw new RdfWriterSelectionException("There is no SPARQL Results Writer available for the Syntax " + this._name);
            }
        }

        /// <summary>
        /// Gets whether a particular Type of Object can be parsed
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <returns></returns>
        public bool CanParseObject<T>()
        {
            return this._objectParserTypes.ContainsKey(typeof(T));
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
            if (this._objectParserTypes.TryGetValue(t, out result))
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
            if (this._objectParserTypes.ContainsKey(t))
            {
                if (parserType == null)
                {
                    this._objectParserTypes.Remove(t);
                }
                else
                {
                    if (this.EnsureObjectParserInterface(parserType, t))
                    {
                        this._objectParserTypes[t] = parserType;
                    }
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
            if (this._objectParserTypes.ContainsKey(typeof(T)))
            {
                Type parserType = this._objectParserTypes[typeof(T)];
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
                return this._objectParserTypes;
            }
        }

        #endregion

    }
}
