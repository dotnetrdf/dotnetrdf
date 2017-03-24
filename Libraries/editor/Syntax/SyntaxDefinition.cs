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
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing.Validation;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    /// <summary>
    /// Represents a Syntax Definition
    /// </summary>
    public class SyntaxDefinition
    {
        private String _name;
        private String _defFile;
        private String[] _extensions;
        private IRdfReader _parser;
        private IRdfWriter _writer;
        private ISyntaxValidator _validator;
        private String _singleLineComment, _multiLineCommentStart, _multiLineCommentEnd;
        private bool _isXml = false;

        #region Constructors which lazily load the Highlighting Definition

        /// <summary>
        /// Creates a new syntax definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definitionFile">Definition File</param>
        /// <param name="fileExtensions">Associated File Extensions</param>
        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions)
        {
            this._name = name;
            this._defFile = definitionFile;
            this._extensions = fileExtensions;
        }

        /// <summary>
        /// Creates a new syntax definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definitionFile">Definition File</param>
        /// <param name="fileExtensions">Associated File Extensions</param>
        /// <param name="defaultParser">Default RDF parser</param>
        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfReader defaultParser)
            : this(name, definitionFile, fileExtensions)
        {
            this._parser = defaultParser;
        }

        /// <summary>
        /// Creates a new syntax definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definitionFile">Definition File</param>
        /// <param name="fileExtensions">Associated File Extensions</param>
        /// <param name="defaultParser">Default RDF parser</param>
        /// <param name="defaultWriter">Default RDF writer</param>
        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfReader defaultParser, IRdfWriter defaultWriter)
            : this(name, definitionFile, fileExtensions, defaultParser)
        {
            this._writer = defaultWriter;
        }

        /// <summary>
        /// Creates a new syntax definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definitionFile">Definition File</param>
        /// <param name="fileExtensions">Associated File Extensions</param>
        /// <param name="defaultWriter">Default RDF writer</param>
        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfWriter defaultWriter)
            : this(name, definitionFile, fileExtensions)
        {
            this._writer = defaultWriter;
        }

        /// <summary>
        /// Creates a new syntax definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definitionFile">Definition File</param>
        /// <param name="fileExtensions">Associated File Extensions</param>
        /// <param name="defaultParser">Default RDF parser</param>
        /// <param name="defaultWriter">Default RDF writer</param>
        /// <param name="validator">Syntax Validator</param>
        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfReader defaultParser, IRdfWriter defaultWriter, ISyntaxValidator validator)
            : this(name, definitionFile, fileExtensions, defaultParser, defaultWriter)
        {
            this._validator = validator;
        }

        /// <summary>
        /// Creates a new syntax definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definitionFile">Definition File</param>
        /// <param name="fileExtensions">Associated File Extensions</param>
        /// <param name="defaultWriter">Default RDF writer</param>
        /// <param name="validator">Syntax Validator</param>
        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfWriter defaultWriter, ISyntaxValidator validator)
            : this(name, definitionFile, fileExtensions, defaultWriter)
        {
            this._validator = validator;
        }

        /// <summary>
        /// Creates a new syntax definition
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="definitionFile">Definition File</param>
        /// <param name="fileExtensions">Associated File Extensions</param>
        /// <param name="validator">Syntax Validator</param>
        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, ISyntaxValidator validator)
            : this(name, definitionFile, fileExtensions)
        {
            this._validator = validator;
        }

        #endregion

        /// <summary>
        /// Gets the Name of the Syntax
        /// </summary>
        public String Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets the Filename of the file that defines how to highlight this syntax
        /// </summary>
        public String DefinitionFile
        {
            get
            {
                return this._defFile;
            }
        }

        /// <summary>
        /// Gets the File Extensions associated with the Syntax
        /// </summary>
        public String[] FileExtensions
        {
            get
            {
                return this._extensions;
            }
        }

        /// <summary>
        /// Gets the Default RDF Parser for the format
        /// </summary>
        public IRdfReader DefaultParser
        {
            get
            {
                return this._parser;
            }
        }

        /// <summary>
        /// Gets the Default RDF Writer for the format
        /// </summary>
        public IRdfWriter DefaultWriter
        {
            get
            {
                return this._writer;
            }
        }

        /// <summary>
        /// Gets the Syntax Validator for the format
        /// </summary>
        public ISyntaxValidator Validator
        {
            get
            {
                return this._validator;
            }
        }

        /// <summary>
        /// Gets/Sets single line comment character
        /// </summary>
        public String SingleLineComment
        {
            get
            {
                return this._singleLineComment;
            }
            set
            {
                this._singleLineComment = value;
            }
        }

        /// <summary>
        /// Gets/Sets multi-line comment start characters
        /// </summary>
        public String MultiLineCommentStart
        {
            get
            {
                return this._multiLineCommentStart;
            }
            set
            {
                this._multiLineCommentStart = value;
            }
        }

        /// <summary>
        /// Gets/Sets multi-line comment end characters
        /// </summary>
        public String MultiLineCommentEnd
        {
            get
            {
                return this._multiLineCommentEnd;
            }
            set
            {
                this._multiLineCommentEnd = value;
            }
        }

        /// <summary>
        /// Gets whether the Syntax supports comments
        /// </summary>
        public bool CanComment
        {
            get
            {
                return this._singleLineComment != null || (this._multiLineCommentStart != null && this._multiLineCommentEnd != null);
            }
        }

        /// <summary>
        /// Gets/Sets whether the format uses XML based highlighting
        /// </summary>
        public bool IsXmlFormat
        {
            get
            {
                return this._isXml;
            }
            set
            {
                this._isXml = value;
            }
        }
    }
}
