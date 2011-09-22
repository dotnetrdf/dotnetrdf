using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.FullText.Search;

namespace VDS.RDF.Query.FullText
{
    /// <summary>
    /// <para>
    /// This namespace provides Full Text Query capabilities that can be used with our Leviathan SPARQL Engine.  Currently only Lucene.Net based indexing is supported though the library is fully extensible so you can create your own indexing providers as desired.
    /// </para>
    /// <h3>Usage</h3>
    /// <para>
    /// To use these features you simply need to add a <see cref="FullTextOptimiser">FullTextOptimiser</see> as an optimiser on your SPARQL Queries, this takes a <see cref="IFullTextSearchProvider">IFullTextSearchProvider</see> instance which performs the actual full text querying.  Once this is done you then simply need to use the relevant property in your SPARQL queries e.g.
    /// </para>
    /// <pre>
    /// PREFIX pf: &lt;http://jena.hpl.hp.com/ARQ/property#&gt;
    /// SELECT * WHERE { ?s pf:textMatch "text" }
    /// </pre>
    /// <para>
    /// Those who are familiar will note that this is the same syntax as used by <a href="http://jena.sourceforge.net/ARQ/lucene-arq.html">LARQ</a> and all the syntactic variations from LARQ such as retrieving scores, applying thresholds and limits are also supported by our full text query.
    /// </para>
    /// <para>
    /// Search Text can be a simple textual search term or it may be any valid query as supported by the underlying full text query provider:
    /// </para>
    /// <ul>
    ///     <li>Lucene.Net - See the <a href="http://lucene.apache.org/java/2_9_2/queryparsersyntax.html">Lucene 2.9.2 Query Syntax</a> documentation</li>
    /// </ul>
    /// <h4>Important</h4>
    /// <para>
    /// The <strong>FullText</strong> namespace is provided by the plugin library <strong>dotNetRDF.Query.FullText.dll</strong>
    /// </para>
    /// </summary>
    public class NamespaceDoc
    {
    }
}

namespace VDS.RDF.Query.FullText.Indexing
{
    /// <summary>
    /// This namespace provides Full Text Indexing functionality through the <see cref="IFullTextIndexer">IFullTextIndexer</see> interface
    /// </summary>
    public class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.FullText.Indexing.Lucene
{
    /// <summary>
    /// This namespace provides implementations of the <see cref="IFullTextIndexer">IFullTextIndexer</see> interface which use Lucene.Net to create indexes
    /// </summary>
    public class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.FullText.Schema
{
    /// <summary>
    /// <para>
    /// This namespace contains classes pertaining to controlling how indexers encode the indexed information onto fields on index documents
    /// </para>
    /// <para>
    /// Typically there should be no need to use anything other than the <see cref="DefaultIndexSchema">DefaultIndexSchema</see> unless you are creating a custom indexer
    /// </para>
    /// </summary>
    public class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.FullText.Search
{
    /// <summary>
    /// This namespace provides Full Text Query functionality through the <see cref="IFullTextSearchProvider">IFullTextSearchProvider</see> and <see cref="IFullTextSearchResult">IFullTextSearchResult</see> interfaces
    /// </summary>
    public class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    /// <summary>
    /// This namespace provides implementations of the <see cref="IFullTextSearchProvider">IFullTextSearchProvider</see> interface which use Lucene.Net to make full text queries
    /// </summary>
    public class NamespaceDoc
    {

    }
}
