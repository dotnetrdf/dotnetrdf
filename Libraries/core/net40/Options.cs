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
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Writing;
using VDS.RDF.Query;
using System.Globalization;
using System.Threading;

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
        static Options()
        {
            Rdf11 = true;
            InternUris = true;
            LiteralEqualityMode = LiteralEqualityMode.Strict;
            FullTripleIndexing = true;
            DefaultComparisonOptions = CompareOptions.Ordinal;
            DefaultCulture = CultureInfo.InvariantCulture;
            HttpFullDebugging = false;
            HttpDebugging = false;
            ForceHttpBasicAuth = false;
            ValidateIris = false;
            LiteralValueNormalization = false;
        }

        /// <summary>
        /// Gets/Sets whether RDF 1.1 mode is enabled (defaults to enabled)
        /// </summary>
        /// <remarks>
        /// <para>
        /// RDF 1.1 is the family of updated RDF specifications formally published as W3C recommendations in/around late 2013.  They may some changes to the semantics of RDF which have user noticeable effects, the most visible of these is that all literals are now implicitly typed i.e. there are no untyped literals in RDF 1.1
        /// </para>
        /// <para>
        /// With RDF 1.1 mode enabled there are a number of behavioural differences:
        /// </para>
        /// <ul>
        /// <li>All literals are implicitly typed, literals with no type/language specifier are typed as <code>xsd:string</code> while those with language specifiers are typed as <code>rdf:langString</code></li>
        /// 
        /// </ul>
        /// </remarks>
        public static bool Rdf11 { get; set; }

        /// <summary>
        /// Gets/Sets the Mode used to compute Literal Equality (Default is <see cref="VDS.RDF.LiteralEqualityMode.Strict">Strict</see> which enforces the W3C RDF Specification)
        /// </summary>
        public static LiteralEqualityMode LiteralEqualityMode { get; set; }

        /// <summary>
        /// Gets/Sets whether Literal Values should be normalized (defaults to disabled)
        /// </summary>
        public static bool LiteralValueNormalization { get; set; }

        /// <summary>
        /// Controls whether the indexed triple collections will create full indexes for the Triples inserted into it (defaults to enabled)
        /// </summary>
        /// <remarks>
        /// <para>
        /// An indexed triple collections typically creates indexes at least based upon Subjects, Predicates and Objects.  When full indexing is enabled it may also create indexes based on Subject-Predicate, Predicate-Object and Subject-Object pairs which may improve query speed but will use additional memory.  Actual implementations may create more/less indexes depending on their underlying data structures but they should respect this setting when deciding how many indexes to create.
        /// </para>
        /// <para>
        /// Default setting for Full Indexing is enabled, enabling/disabling it only has an effect on indexed triple collection instances instantiated after full indexing was enabled/disabled i.e. existing Graphs in memory using the indexed triple collections continue to use the full indexing setting that was present when they were instantiated.
        /// </para>
        /// </remarks>
        public static bool FullTripleIndexing { get; set; }

        /// <summary>
        /// Gets/Sets whether IRIs are validated by parsers which support this functionality (defaults to disabled)
        /// </summary>
        /// <remarks>
        /// When enabled certain parsers will validate all IRIs they see to ensure that they are valid and throw a parser error if they are not.  Since there is a performance penalty associated with this and many older RDF standards were written pre-IRIs (thus enforcing IRI validity would reject data considered valid by those specifications) this feature is disabled by default.
        /// </remarks>
        public static bool ValidateIris { get; set; }

        /// <summary>
        /// Gets/Sets whether Basic HTTP authentication should be forced (defaults to disabled)
        /// </summary>
        /// <remarks>
        /// <para>
        /// There have been reported problems where some servers don't cope nicely with the HTTP authentication challenge response procedure resulting in failed HTTP requests.  If the server only uses Basic HTTP authentication then you can opt to force dotNetRDF to always include the HTTP basic authentication header in requests and thus workaround this problem.
        /// </para>
        /// <para>
        /// <strong>Warning:</strong> Under Silverlight this will only work correctly if usernames and passwords are composed only of characters within the ASCII range.
        /// </para>
        /// </remarks>
        public static bool ForceHttpBasicAuth { get; set; }

        /// <summary>
        /// Gets/Sets whether the library will attempt to intern URIs to reduce memory usage (defaults to enabled)
        /// </summary>
        /// <remarks>
        /// <para>
        /// Interning URIs trades off memory usage for performance, by interning URIs so the same URI strings are represented by the same <see cref="Uri"/> instances we make a lot of equality checks reference equality checks which can substantially boost performance of some operations.
        /// </para>
        /// </remarks>
        public static bool InternUris { get; set; }

        /// <summary>
        /// Gets/Sets whether HTTP Request and Response Information should be output to the Console Standard Out for Debugging purposes (defaults to disabled)
        /// </summary>
        public static bool HttpDebugging { get; set; }

        /// <summary>
        /// Gets/Sets whether the HTTP Response Stream should be output to the Console Standard Output for Debugging purposes (defaults to disabled)
        /// </summary>
        public static bool HttpFullDebugging { get; set; }

        /// <summary>
        /// Gets/Sets the default culture literal comparison when literals are string or not implicitely comparable (different types, parse/cast error...)
        /// </summary>
        /// <remarks>
        /// The default is set to the invariant culture to preserve behavioural backwards compatibility with past versions of dotNetRDF
        /// </remarks>
        public static CultureInfo DefaultCulture { get; set; }

        /// <summary>
        /// Gets/Sets the default collation for literal comparison when literals are string or not implicitely comparable (different types, parse/cast error...)
        /// </summary>
        /// <remarks>
        /// The default is set to <see cref="CompareOptions.Ordinal"/> to preserve behavioural backwards compatibility with past versions of dotNetRDF
        /// </remarks>
        public static CompareOptions DefaultComparisonOptions { get; set; }
    }
}
