using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Token which represents the SPARQL Update ADD Keyword
    /// </summary>
    public class AddKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new ADD Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public AddKeywordToken(int line, int pos) : base(Token.ADD, "ADD", line, line, pos, pos + 3) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update CLEAR Keyword
    /// </summary>
    public class ClearKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new CLEAR Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ClearKeywordToken(int line, int pos) : base(Token.CLEAR, "CLEAR", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update COPY Keyword
    /// </summary>
    public class CopyKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new COPY Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public CopyKeywordToken(int line, int pos) : base(Token.COPY, "COPY", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update CREATE Keyword
    /// </summary>
    public class CreateKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new CREATE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public CreateKeywordToken(int line, int pos) : base(Token.CREATE, "CREATE", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update DATA Keyword
    /// </summary>
    public class DataKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DATA Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DataKeywordToken(int line, int pos) : base(Token.DATA, "DATA", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update DEFAULT Keyword
    /// </summary>
    public class DefaultKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DEFAULT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DefaultKeywordToken(int line, int pos) : base(Token.DEFAULT, "DEFAULT", line, line, pos, pos + 7) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update DELETE Keyword
    /// </summary>
    public class DeleteKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DELETE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DeleteKeywordToken(int line, int pos) : base(Token.DELETE, "DELETE", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update DROP Keyword
    /// </summary>
    public class DropKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new DROP Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public DropKeywordToken(int line, int pos) : base(Token.DROP, "DROP", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update INSERT Keyword
    /// </summary>
    public class InsertKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new INSERT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public InsertKeywordToken(int line, int pos) : base(Token.INSERT, "INSERT", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update INTO Keyword
    /// </summary>
    public class IntoKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new INTO Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public IntoKeywordToken(int line, int pos) : base(Token.INTO, "INTO", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update LOAD Keyword
    /// </summary>
    public class LoadKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new LOAD Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public LoadKeywordToken(int line, int pos) : base(Token.LOAD, "LOAD", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update MOVE Keyword
    /// </summary>
    public class MoveKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new MOVE Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public MoveKeywordToken(int line, int pos) : base(Token.MOVE, "MOVE", line, line, pos, pos + 4) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update SILENT Keyword
    /// </summary>
    public class SilentKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new SILENT Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public SilentKeywordToken(int line, int pos) : base(Token.SILENT, "SILENT", line, line, pos, pos + 6) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update TO Keyword
    /// </summary>
    public class ToKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new TO Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public ToKeywordToken(int line, int pos) : base(Token.TO, "TO", line, line, pos, pos + 2) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update USING Keyword
    /// </summary>
    public class UsingKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new USING Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public UsingKeywordToken(int line, int pos) : base(Token.USING, "USING", line, line, pos, pos + 5) { }
    }

    /// <summary>
    /// Token which represents the SPARQL Update WITH Keyword
    /// </summary>
    public class WithKeywordToken : BaseToken
    {
        /// <summary>
        /// Creates a new WITH Keyword Token
        /// </summary>
        /// <param name="line">Line the Keyword occurs on</param>
        /// <param name="pos">Position the Keyword occurs at</param>
        public WithKeywordToken(int line, int pos) : base(Token.WITH, "WITH", line, line, pos, pos + 4) { }
    }
}
