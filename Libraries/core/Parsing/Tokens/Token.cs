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

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// Static Class which defines the Integer Constants used for Token Types
    /// </summary>
    public static class Token
    {
        /// <summary>
        /// Constants defining Token Types
        /// </summary>
        public const int UNKNOWN = -1,
                          BOF = 0,

                          PREFIXDIRECTIVE = 1,
                          PREFIX = 2,
                          BASEDIRECTIVE = 3,
                          KEYWORDDIRECTIVE = 4,
                          FORALL = 5,
                          FORSOME = 6,

                          URI = 10,
                          QNAME = 11,

                          LITERAL = 20,
                          LONGLITERAL = 21,
                          LANGSPEC = 22,
                          DATATYPE = 23,
                          PLAINLITERAL = 24,
                          LITERALWITHLANG = 25,
                          LITERALWITHDT = 26,
                          GRAPHLITERAL = 27,

                          BLANKNODE = 30,
                          BLANKNODEWITHID = 31,
                          BLANKNODECOLLECTION = 32,

                          IMPLIES = 40,
                          IMPLIEDBY = 41,
                          EQUALS = 42,

                          AT = 50,
                          DOT = 51,
                          SEMICOLON = 52,
                          COMMA = 53,
                          HASH = 54,
                          UNDERSCORE = 55,
                          HATHAT = 56,
                          LEFTSQBRACKET = 57,
                          RIGHTSQBRACKET = 58,
                          LEFTBRACKET = 59,
                          RIGHTBRACKET = 60,
                          LEFTCURLYBRACKET = 61,
                          RIGHTCURLYBRACKET = 62,
                          HAT = 63,
                          EXCLAMATION = 64,
                          ASSIGNMENT = 65,
                          QUESTION = 66,
                          PATH = 67,

                          KEYWORDA = 70,
                          KEYWORDIS = 71,
                          KEYWORDOF = 72,
                          KEYWORDCUSTOM = 73,
                          KEYWORDDEF = 74,

                          VARIABLE = 80,

                          COMMENT = 90,

                          ASK = 100,
                          CONSTRUCT = 101,
                          DESCRIBE = 102,
                          SELECT = 103,

                          ORDERBY = 110,
                          LIMIT = 111,
                          OFFSET = 112,
                          DISTINCT = 113,
                          REDUCED = 114,
                          FROM = 115,
                          NAMED = 116,
                          FROMNAMED = 117,
                          WHERE = 118,
                          GRAPH = 119,
                          OPTIONAL = 120,
                          FILTER = 121,
                          ALL = 122,
                          UNION = 123,
                          ASC = 124,
                          DESC = 125,
                          AS = 126,
                          HAVING = 127,
                          GROUPBY = 128,
                          EXISTS = 129,
                          NOTEXISTS = 130,
                          LET = 131,
                          UNSAID = 132,
                          MINUS_P = 133,
                          SERVICE = 134,
                          BINDINGS = 135,
                          UNDEF = 136,

                          STR = 150,
                          LANG = 151,
                          LANGMATCHES = 152,
                          SPARQLDATATYPE = 153,
                          BOUND = 154,
                          SAMETERM = 155,
                          ISURI = 156,
                          ISIRI = 157,
                          ISLITERAL = 158,
                          REGEX = 159,
                          DATATYPEFUNC = 160,
                          ISBLANK = 161,
                          IN = 162,
                          NOTIN = 163,
                          STRLANG = 164,
                          STRDT = 165,
                          IRI = 166,
                          COALESCE = 167,
                          IF = 168,
                          URIFUNC = 169,

                          PLUS = 200,
                          MINUS = 201,
                          NOT = 202,
                          DIVIDE = 203,
                          MULTIPLY = 204,
                          GREATERTHAN = 205,
                          LESSTHAN = 206,
                          GREATERTHANOREQUALTO = 207,
                          LESSTHANOREQUALTO = 208,
                          NOTEQUALS = 209,
                          NEGATION = 210,
                          AND = 211,
                          OR = 212,
                          BITWISEOR = 213,

                          COUNT = 170,
                          SUM = 171,
                          AVG = 172,
                          MIN = 173,
                          MAX = 174,
                          NMIN = 175,
                          NMAX = 176,
                          MEDIAN = 177,
                          MODE = 178,
                          GROUPCONCAT = 179,
                          SAMPLE = 180,
                          SEPARATOR = 181,

                          LENGTH = 190,
                                                    
                          INSERT = 200,
                          DELETE = 201,
                          DATA = 202,
                          LOAD = 203,
                          CLEAR = 204,
                          CREATE = 205,
                          DROP = 206,
                          SILENT = 207,
                          INTO = 208,
                          WITH = 209,
                          USING = 210,
                          DEFAULT = 211,

                          EOF = 1000;
    }
}
