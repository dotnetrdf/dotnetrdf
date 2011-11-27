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
                          TAB = 68,
                          EOL = 69,

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
                          BIND = 137,

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
                          ISNUMERIC = 170,
                          STRLEN = 171,
                          SUBSTR = 172,
                          UCASE = 173,
                          LCASE = 174,
                          STRSTARTS = 175,
                          STRENDS = 176,
                          CONTAINS = 177,
                          ENCODEFORURI = 178,
                          CONCAT = 179,
                          ABS = 180,
                          ROUND = 181,
                          CEIL = 182,
                          FLOOR = 183,
                          NOW = 184,
                          YEAR = 185,
                          MONTH = 186,
                          DAY = 187,
                          HOURS = 188,
                          MINUTES = 189,
                          SECONDS = 190,
                          TIMEZONE = 191,
                          TZ = 192,
                          MD5 = 193,
                          SHA1 = 194,
                          SHA224 = 195,
                          SHA256 = 196,
                          SHA384 = 197,
                          SHA512 = 198,
                          BNODE = 199,
                          RAND = 200,
                          STRAFTER = 201,
                          STRBEFORE = 202,
                          REPLACE = 203,

                          PLUS = 250,
                          MINUS = 251,
                          NOT = 252,
                          DIVIDE = 253,
                          MULTIPLY = 254,
                          GREATERTHAN = 255,
                          LESSTHAN = 256,
                          GREATERTHANOREQUALTO = 257,
                          LESSTHANOREQUALTO = 258,
                          NOTEQUALS = 259,
                          NEGATION = 260,
                          AND = 261,
                          OR = 262,
                          BITWISEOR = 263,

                          COUNT = 300,
                          SUM = 301,
                          AVG = 302,
                          MIN = 303,
                          MAX = 304,
                          NMIN = 305,
                          NMAX = 306,
                          MEDIAN = 307,
                          MODE = 308,
                          GROUPCONCAT = 309,
                          SAMPLE = 310,
                          SEPARATOR = 311,

                          LENGTH = 350,
                                                    
                          INSERT = 500,
                          DELETE = 501,
                          DATA = 502,
                          LOAD = 503,
                          CLEAR = 504,
                          CREATE = 505,
                          DROP = 506,
                          SILENT = 507,
                          INTO = 508,
                          WITH = 509,
                          USING = 510,
                          DEFAULT = 511,
                          ALLWORD = 512,
                          ADD = 513,
                          COPY = 514,
                          MOVE = 515,
                          TO = 516,

                          EOF = 1000;
    }
}
