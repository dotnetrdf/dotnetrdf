/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions)
        {
            this._name = name;
            this._defFile = definitionFile;
            this._extensions = fileExtensions;
        }

        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfReader defaultParser)
            : this(name, definitionFile, fileExtensions)
        {
            this._parser = defaultParser;
        }

        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfReader defaultParser, IRdfWriter defaultWriter)
            : this(name, definitionFile, fileExtensions, defaultParser)
        {
            this._writer = defaultWriter;
        }

        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfWriter defaultWriter)
            : this(name, definitionFile, fileExtensions)
        {
            this._writer = defaultWriter;
        }

        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfReader defaultParser, IRdfWriter defaultWriter, ISyntaxValidator validator)
            : this(name, definitionFile, fileExtensions, defaultParser, defaultWriter)
        {
            this._validator = validator;
        }

        public SyntaxDefinition(String name, String definitionFile, String[] fileExtensions, IRdfWriter defaultWriter, ISyntaxValidator validator)
            : this(name, definitionFile, fileExtensions, defaultWriter)
        {
            this._validator = validator;
        }

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
