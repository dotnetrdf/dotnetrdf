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
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Query;

namespace VDS.RDF
{
    /// <summary>
    /// Possible Literal Equality Mode Settings
    /// </summary>
    public enum LiteralEqualityMode
    {
        /// <summary>
        /// Strict Mode compares Literals according to the official W3C RDF Specification
        /// </summary>
        /// <remarks>
        /// This means Literals are equal if and only if:
        /// <ol>
        /// <li>The Lexical Values are identical when compared using Ordinal Comparison</li>
        /// <li>The Language Tags if present are identical</li>
        /// <li>The Datatypes if present are identical</li>
        /// </ol>
        /// </remarks>
        Strict,
        /// <summary>
        /// Loose Mode compares Literals based on values (if they have known Datatypes)
        /// </summary>
        /// <remarks>
        /// This means Literals can be equal if they have lexically different values which are equivalent when converted to the Datatype.
        /// <br /><br />
        /// Literals without Datatypes and those whose Datatypes are unknown or not handled by the Library will be compared using lexical equivalence as with <see cref="LiteralEqualityMode.Strict">Strict</see> mode.
        /// </remarks>
        Loose
    }

    /// <summary>
    /// Configures Global Static Options for the Library
    /// </summary>
    /// <remarks>
    /// Some of these are Debug Build only, please see the Remarks on individual members for more detail
    /// </remarks>
    public static class Options
    {
        private static LiteralEqualityMode _litEqualityMode = LiteralEqualityMode.Strict;
        private static bool _litNormalization = false;
        private static long _queryExecutionTimeout = 180000, _updateExecutionTimeout = 180000;
        private static int _defaultCompressionLevel = WriterCompressionLevel.More;
        private static bool _fullIndexing = true;
        private static bool _queryOptimisation = true, _algebraOptimisation = true, _unsafeOptimisation = false;
        private static SparqlQuerySyntax _queryDefaultSyntax = SparqlQuerySyntax.Sparql_1_1;
        private static bool _queryAllowUnknownFunctions = true;
        private static bool _uriLoaderCaching = true;
        private static int _uriLoaderTimeout = 15000;
        private static bool _utf8Bom = false;
        private static bool _useDTDs = true;
        private static bool _multiThreadedWriting = false;
        private static bool _internUris = true;
        private static bool _rigorousQueryEvaluation = false, _strictOperators = false;
        private static bool _forceBlockingIO = false;
        private static bool _forceHttpBasicAuth = false;

#if NET40 && !SILVERLIGHT
        private static bool _usePLinq = true;
#endif

#if DEBUG
        //Debug Build Only
        private static bool _httpDebug = false;
        private static bool _httpFullDebug = false;
#endif

        /// <summary>
        /// Gets/Sets the Mode used to compute Literal Equality (Default is <see cref="LiteralEqualityMode.Strict">Strict</see> which enforces the W3C RDF Specification)
        /// </summary>
        public static LiteralEqualityMode LiteralEqualityMode
        {
            get
            {
                return _litEqualityMode;
            }
            set
            {
                _litEqualityMode = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Literal Values should be normalized
        /// </summary>
        public static bool LiteralValueNormalization
        {
            get
            {
                return _litNormalization;
            }
            set
            {
                _litNormalization = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Hard Timeout limit for SPARQL Query Execution (in milliseconds)
        /// </summary>
        /// <remarks>
        /// This is used to stop SPARQL queries running away and never completing execution, it defaults to 3 mins (180,000 milliseconds)
        /// </remarks>
        public static long QueryExecutionTimeout
        {
            get
            {
                return _queryExecutionTimeout;
            }
            set
            {
                _queryExecutionTimeout = Math.Max(value, 0);
            }
        }

        /// <summary>
        /// Gets/Sets whether Query Optimisation should be used
        /// </summary>
        public static bool QueryOptimisation
        {
            get
            {
                return _queryOptimisation;
            }
            set
            {
                _queryOptimisation = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Algebra Optimisation should be used
        /// </summary>
        public static bool AlgebraOptimisation
        {
            get
            {
                return _algebraOptimisation;
            }
            set
            {
                _algebraOptimisation = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether some Optimisations considered unsafe can be used
        /// </summary>
        /// <remarks>
        /// <para>
        /// The notion of unsafe optimisations refers to optimisations that can make significant performance improvements to some types of queries but are disabled normally because they may lead to behaviour which does not strictly align with the SPARQL specification.
        /// </para>
        /// <para>
        /// One example of such an optimisation is an implicit join where the optimiser cannot be sure that the variables involved don't represent literals.
        /// </para>
        /// </remarks>
        public static bool UnsafeOptimisation
        {
            get
            {
                return _unsafeOptimisation;
            }
            set
            {
                _unsafeOptimisation = value;
            }
        }

        /// <summary>
        /// Gets/Sets the default syntax used for parsing SPARQL queries
        /// </summary>
        /// <remarks>
        /// The default is SPARQL 1.1 unless you use this property to change it
        /// </remarks>
        public static SparqlQuerySyntax QueryDefaultSyntax
        {
            get
            {
                return _queryDefaultSyntax;
            }
            set
            {
                _queryDefaultSyntax = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether functions that can't be parsed into Expressions should be represented by the <see cref="VDS.RDF.Query.Expressions.Functions.UnknownFunction">UnknownFunction</see>
        /// </summary>
        /// <remarks>When set to false a Parser Error will be thrown if the Function cannot be parsed into an Expression</remarks>
        public static bool QueryAllowUnknownFunctions
        {
            get
            {
                return _queryAllowUnknownFunctions;
            }
            set
            {
                _queryAllowUnknownFunctions = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to use rigorous query evaluation
        /// </summary>
        /// <remarks>
        /// <para>
        /// Rigorous Query evaluation applies more checks to the triples produced by datasets to ensure they actually match the patterns being scanned.  If the underlying index structures are able to guarantee this then rigorous evaluation may be turned off for faster evaluation which it is by default since our default <see cref="TreeIndexedTripleCollection"/> and <see cref="TripleCollection"/> implementations will guarantee this.
        /// </para>
        /// </remarks>
        public static bool RigorousEvaluation
        {
            get
            {
                return _rigorousQueryEvaluation;
            }
            set
            {
                _rigorousQueryEvaluation = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether to use strict operators
        /// </summary>
        /// <remarks>
        /// <para>
        /// Strict Operators refers to the interpretation of certian operators like + and - in SPARQL expression evaluation.  If enabled then the operators will function only as specified in the SPARQL specification, if disabled (which is the default) then certain extensions (which the SPARQL specification allows an implementation to provide) will be allowed e.g. date time arithmetic.
        /// </para>
        /// <para>
        /// The only time you may want to disable this is if you are developing queries locally which you want to ensure are portable to other systems or when running the SPARQL compliance tests.
        /// </para>
        /// </remarks>
        public static bool StrictOperators
        {
            get
            {
                return _strictOperators;
            }
            set
            {
                _strictOperators = value;
            }
        }

#if NET40 && !SILVERLIGHT

        /// <summary>
        /// Gets/Sets whether the query engine will try to use PLinq where applicable to evaluate suitable SPARQL constructs in parallel
        /// </summary>
        /// <remarks>
        /// For the 0.6.1 release onwards this was an experimental feature and disabled by default, from 0.7.0 onwards this is enabled by default
        /// </remarks>
        public static bool UsePLinqEvaluation
        {
            get
            {
                return _usePLinq;
            }
            set
            {
                _usePLinq = value;
            }
        }

#endif

        /// <summary>
        /// Gets/Sets the Hard Timeout limit for SPARQL Update Execution (in milliseconds)
        /// </summary>
        /// <remarks>
        /// This is used to stop SPARQL Updates running away and never completing execution, it defaults to 3 mins (180,000 milliseconds)
        /// </remarks>
        public static long UpdateExecutionTimeout
        {
            get
            {
                return _updateExecutionTimeout;
            }
            set
            {
                _updateExecutionTimeout = Math.Max(0, value);
            }
        }

        /// <summary>
        /// Gets/Sets the Default Compression Level used for Writers returned by the <see cref="MimeTypesHelper">MimeTypesHelper</see> class when the writers implement <see cref="ICompressingWriter">ICompressingWriter</see>
        /// </summary>
        public static int DefaultCompressionLevel
        {
            get
            {
                return _defaultCompressionLevel;
            }
            set
            {
                _defaultCompressionLevel = value;
            }
        }

        /// <summary>
        /// Controls whether the indexed triple collections will create full indexes for the Triples inserted into it
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default indexes triple collections creates indexes on Triples based upon Subjects, Predicates and Objects alone.  When full indexing is enabled it also creates indexes based on Subject-Predicate, Predicate-Object and Subject-Object pairs which may improve query speed but will use additional memory.
        /// </para>
        /// <para>
        /// Default setting for Full Indexing is enabled, enabling/disabling it only has an effect on indexed triple collection instances instantiated after full indexing was enabled/disabled i.e. existing Graphs in memory using the indexed triple collections continue to use the full indexing setting that was present when they were instantiated.
        /// </para>
        /// </remarks>
        public static bool FullTripleIndexing
        {
            get
            {
                return _fullIndexing;
            }
            set
            {
                _fullIndexing = value;
            }
        }

#if !NO_URICACHE

        /// <summary>
        /// Gets/Sets whether the <see cref="UriLoader">UriLoader</see> uses caching
        /// </summary>
        public static bool UriLoaderCaching
        {
            get
            {
                return _uriLoaderCaching;
            }
            set
            {
                _uriLoaderCaching = value;
            }
        }
#endif

        /// <summary>
        /// Gets/Sets the Timeout for URI Loader requests (Defaults to 15 seconds)
        /// </summary>
        public static int UriLoaderTimeout
        {
            get
            {
                return _uriLoaderTimeout;
            }
            set
            {
                if (value > 0)
                {
                    _uriLoaderTimeout = value;
                }
            }
        }

        /// <summary>
        /// Gets/Sets whether a UTF-8 BOM is used for UTF-8 Streams created by dotNetRDF (this does not affect Streams passed directly to methods as open streams cannot have their encoding changed)
        /// </summary>
        public static bool UseBomForUtf8
        {
            get
            {
                return _utf8Bom;
            }
            set
            {
                _utf8Bom = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Blocking IO should be forced
        /// </summary>
        /// <remarks>
        /// Blocking IO refers to how the parsing sub-system reads in inputs, it will use Blocking/Non-Blocking IO depending on the input source.  In most cases the detection of which to use should never cause an issue but theoretically in some rare cases using non-blocking IO may lead to incorrect parsing errors being thrown (premature end of input detected), if you suspect this is the case try enabling this setting.  If you still experience this problem with this setting enabled then there is some other issue with your input.
        /// </remarks>
        public static bool ForceBlockingIO
        {
            get
            {
                return _forceBlockingIO;
            }
            set
            {
                _forceBlockingIO = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether Basic HTTP authentication should be forced
        /// </summary>
        /// <remarks>
        /// <para>
        /// There have been reported problems where some servers don't cope nicely with the HTTP authentication challenge response procedure resulting in failed HTTP requests.  If the server only uses Basic HTTP authentication then you can opt to force dotNetRDF to always include the HTTP basic authentication header in requests and thus workaround this problem.
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> Under Silverlight this will only work correctly if usernames and passwords are composed only of characters within the ASCII range.
        /// </para>
        /// </remarks>
        public static bool ForceHttpBasicAuth
        {
            get
            {
                return _forceHttpBasicAuth;
            }
            set
            {
                _forceHttpBasicAuth = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether a DTD should be used for some XML formats to compress output
        /// </summary>
        public static bool UseDtd
        {
            get
            {
                return _useDTDs;
            }
            set
            {
                _useDTDs = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether multi-theaded writing is permitted
        /// </summary>
        /// <remarks>
        /// In some contexts multi-threaded writing may not even work due to restrictions on thread types since we use the <see cref="System.Threading.WaitAll">WaitAll()</see> method which is only valid in <strong>MTA</strong> contexts.
        /// </remarks>
        public static bool AllowMultiThreadedWriting
        {
            get
            {
                return _multiThreadedWriting;
            }
            set
            {
                _multiThreadedWriting = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the library will attempt to intern URIs to reduce memory usage
        /// </summary>
        public static bool InternUris
        {
            get
            {
                return _internUris;
            }
            set
            {
                _internUris = value;
            }
        }

        #region Debug Build Only Options

        #if DEBUG

        /// <summary>
        /// Gets/Sets whether HTTP Request and Response Information should be output to the Console Standard Out for Debugging purposes
        /// </summary>
        /// <remarks>
        /// <strong>Only available in Debug builds</strong>
        /// <br /><br />
        /// This does not guarentee that this information is output to the Console Standard Out, most code that makes HTTP requests should do this but it may vary depending on the Library build and the exact classes used.
        /// </remarks>
        public static bool HttpDebugging {
            get
            {
                return _httpDebug;
            }
            set
            {
                _httpDebug = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the HTTP Response Stream should be output to the Console Standard Output for Debugging purposes
        /// </summary>
        /// <remarks>
        /// <strong>Only available in Debug builds</strong>
        /// <br /><br />
        /// Only applies if <see cref="HttpDebugging">HttpDebugging</see> is enabled
        /// </remarks>
        public static bool HttpFullDebugging
        {
            get
            {
                return _httpFullDebug;
            }
            set
            {
                _httpFullDebug = value;
            }
        }

        #endif

        #endregion
    }
}
