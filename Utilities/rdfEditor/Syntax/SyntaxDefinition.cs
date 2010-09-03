using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using VDS.RDF;
using VDS.RDF.Parsing.Validation;
using rdfEditor.AutoComplete;

namespace rdfEditor.Syntax
{
    public class SyntaxDefinition
    {
        private String _name;
        private IHighlightingDefinition _def;
        private String _defFile;
        private String[] _extensions;
        private IRdfReader _parser;
        private IRdfWriter _writer;
        private ISyntaxValidator _validator;

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

        public String Name
        {
            get
            {
                return this._name;
            }
        }

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

        public String[] FileExtensions
        {
            get
            {
                return this._extensions;
            }
        }

        public IRdfReader DefaultParser
        {
            get
            {
                return this._parser;
            }
        }

        public IRdfWriter DefaultWriter
        {
            get
            {
                return this._writer;
            }
        }

        public ISyntaxValidator Validator
        {
            get
            {
                return this._validator;
            }
        }
    }
}
