using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using VDS.RDF;
using VDS.RDF.Parsing.Validation;
using VDS.RDF.Utilities.Editor.AutoComplete;

namespace VDS.RDF.Utilities.Editor.Syntax
{
    /// <summary>
    /// Represents a Syntax Definition
    /// </summary>
    public class SyntaxDefinition
    {
        private String _name;
        private IHighlightingDefinition _def;
        private String _defFile;
        private String[] _extensions;
        private IRdfReader _parser;
        private IRdfWriter _writer;
        private ISyntaxValidator _validator;
        private String _singleLineComment, _multiLineCommentStart, _multiLineCommentEnd;
        private bool _isXml = false;

        #region Constructors which take an explicit Highlighting Definition

        public SyntaxDefinition(String name, IHighlightingDefinition definition, String[] fileExtensions)
        {
            this._name = name;
            this._def = definition;
            this._extensions = fileExtensions;
        }

        public SyntaxDefinition(String name, IHighlightingDefinition definition, String[] fileExtensions, IRdfReader defaultParser)
            : this(name, definition, fileExtensions)
        {
            this._parser = defaultParser;
        }

        public SyntaxDefinition(String name, IHighlightingDefinition definition, String[] fileExtensions, IRdfReader defaultParser, IRdfWriter defaultWriter)
            : this(name, definition, fileExtensions, defaultParser)
        {
            this._writer = defaultWriter;
        }

        public SyntaxDefinition(String name, IHighlightingDefinition definition, String[] fileExtensions, IRdfWriter defaultWriter)
            : this(name, definition, fileExtensions)
        {
            this._writer = defaultWriter;
        }

        public SyntaxDefinition(String name, IHighlightingDefinition definition, String[] fileExtensions, IRdfReader defaultParser, IRdfWriter defaultWriter, ISyntaxValidator validator)
            : this(name, definition, fileExtensions, defaultParser, defaultWriter)
        {
            this._validator = validator;
        }

        public SyntaxDefinition(String name, IHighlightingDefinition definition, String[] fileExtensions, IRdfWriter defaultWriter, ISyntaxValidator validator)
            : this(name, definition, fileExtensions, defaultWriter)
        {
            this._validator = validator;
        }

        public SyntaxDefinition(String name, IHighlightingDefinition definition, String[] fileExtensions, ISyntaxValidator validator)
            : this(name, definition, fileExtensions)
        {
            this._validator = validator;
        }

        #endregion

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
        /// Gets the Syntax Highlighting Definition
        /// </summary>
        public IHighlightingDefinition Highlighter
        {
            get
            {
                if (this._def == null)
                {
                    this._def = SyntaxManager.LoadHighlighting(this._defFile, true);
                }
                return this._def;
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
