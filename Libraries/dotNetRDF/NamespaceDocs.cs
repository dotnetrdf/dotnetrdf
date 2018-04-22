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

namespace VDS.RDF
{
    /// <summary>
    /// <para>
    /// Top Level Namespace for the <strong>dotNetRDF Library</strong> which embodies a simple but powerful API for working with RDF and SPARQL.   
    /// </para>
    /// <para>
    /// Specific Namespaces within the Hierarchy provide <see cref="VDS.RDF.Parsing">Parsing</see> and <see cref="VDS.RDF.Writing">Serialization</see> functionality along with a host of related classes to support these functions.
    /// </para>
    /// <para>
    /// Support for querying RDF is provided in the <see cref="VDS.RDF.Query">Query</see> namespace which includes SPARQL Query, limited reasoning support in the <see cref="VDS.RDF.Query.Inference">Query.Inference</see> namespace and a Pellet Server client in the <see cref="VDS.RDF.Query.Inference.Pellet">Query.Inference.Pellet</see> namespace.
    /// </para>
    /// <para>
    /// Support for updating RDF based on the SPARQL 1.1 Update and Graph Store HTTP Protocol for RDF Graph Management is provided in the <see cref="VDS.RDF.Update">Update</see> and <see cref="VDS.RDF.Update.Protocol">Update.Protocol</see> namespaces.
    /// </para>
    /// <h3>Third Party Storage</h3>
    /// <para>For communicating with arbitrary Triple Stores we have a dedicated <see cref="VDS.RDF.Storage">Storage</see> namespace.  As of this release we support the following Triple Stores:
    /// <ul>
    ///     <li>AllegroGraph</li>
    ///     <li>Dydra</li>
    ///     <li>4store</li>
    ///     <li>Fuseki</li>
    ///     <li>Any Sesame HTTP Protocol compliant store e.g. Sesame, OWLIM</li>
    ///     <li>Any SPARQL Graph Store HTTP Protocol for RDF Graph Management compliant stores</li>
    ///     <li>Any SPARQL store that exposes a Query and/or Update endpoint</li>
    ///     <li>Stardog</li>
    ///     <li>Virtuoso</li>
    /// </ul>
    /// </para>
    /// <h3>ASP.Net Integration</h3>
    /// <para>
    /// For those building ASP.Net based websites the <see cref="VDS.RDF.Web">Web</see> namespace is dedicated to providing classes for integrating RDF into ASP.Net applications.
    /// </para>
    /// <h3>Ontology API</h3>
    /// <para>
    /// There is also an <see cref="VDS.RDF.Ontology">Ontology</see> namespace which provides a more resource and ontology centric API for working with RDF than the standard Graph and Triple centric APIs
    /// </para>
    /// <h3>Configuration API</h3>
    /// <para>
    /// We provide a <see cref="Configuration">Configuration</see> API which provides for encoding configuration in RDF Graphs.  This configuration system is used extensively as part of the ASP.Net support as it allows for much more expressive and flexible configurations than were previously possible.  See the <a href="http://www.dotnetrdf.org/content.asp?pageID=Configuration%20API">documentation</a> on the main website for many detailed examples.  This is primarily intended as an easy way to help deploy configurations for ASP.Net applications though you can make use of the API to describe the configuration of various types of objects in other applications, for example we use it in our Store Manager utility to store connection details.
    /// </para>
    /// <h3>Notes</h3>
    /// <para>
    /// dotNetRDF 1.0.0 is now considered a stable release, this means it should be stable for production scenarios.  However it is open source software and despite our best efforts there may still be bugs.  Please help us improve this library by emailing us when you find a bug, you can use the <a href="mailto:dotnetrdf-bugs@lists.sourceforge.net">Bug Reports list</a> to report bugs, the <a href="mailto:dotnetrdf-support@lists.sourceforge.net">Support list</a> to ask questions and the <a href="mailto:dotnetrdf-develop@lists.sourceforge.net">Developer list</a> to request new features or discuss development plans (all these are SourceForge mailing lists which require subscription).
    /// </para>
    /// <h4>Alternative Builds</h4>
    /// <h5>Mono Build</h5>
    /// <para>
    /// There is no separate build for Mono since dotNetRDF can run directly under Mono.  Note that there may still be some features of .Net we use that Mono does not fully support, see the <a href="http://www.dotnetrdf.org/content.asp?pageID=Mono%20Issues">Mono Issues</a> page for more details.  We recommend Mono 2.10 or higher though the library should run on recent 2.6/2.8 releases.
    /// </para>
    /// <h5>Client Profile Build</h5>
    /// <para>
    /// The Client Profile build omits the reference to <see cref="System.Web">System.Web</see> so lacks the ASP.Net integration and some other features that rely on this dependency but is otherwise a fairly complete build of the library.
    /// </para>
    /// <h5>Silverlight/Windows Phone 7 Build</h5>
    /// <para>
    /// The Silverlight and Windows Phone 7 builds of dotNetRDF (<em>dotNetRDF.Silverlight.dll</em> and <em>dotNetRDF.WindowsPhone.dll</em>) are experimental builds that receive limited internal testing so please be aware that these are not as stable as the standard .Net builds.  These build runs on Silverlight 4/Windows Phone 7 and omits the following features since they can't be supported on these platforms:
    /// </para>
    /// <ul>
    ///     <li>Most of the <see cref="VDS.RDF.Web">Web</see> namespaces</li>
    ///     <li>Does not include parts of the <see cref="VDS.RDF.Storage">Storage</see> namespace that would require synchronous HTTP</li>
    ///     <li>No String normalization support</li>
    ///     <li>No <see cref="VDS.RDF.Parsing.UriLoader">UriLoader</see> caching support</li>
    ///     <li>No multi-threaded support where System.Threading.ReaderWriteLockSlim is used</li>
    ///     <li>Various writers and parsers use streaming rather than DOM based XML parsing</li>
    ///     <li>No support for XSL in TriX files</li>
    ///     <li>Synchronous HTTP Request Features - For most of these there are asynchronous callback driven versions of these features available from the 0.5.0 release onwards</li>
    /// </ul>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration Classes which are used for dynamic loading of Configuration serialized as RDF Graphs.
    /// </para>
    /// <para>
    /// This API which provides for encoding dotNetRDF centric configuration in RDF Graphs though it can be extended to serialize and deserialize arbitrary objects if desired.  This configuration API is used extensively with our ASP.Net support as it allows for highly expressive and flexible configurations.  See the <a href="http://www.dotnetrdf.org/content.asp?pageID=Configuration%20API">documentation</a> on the main website for many detailed examples.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Configuration.Permissions
{
    /// <summary>
    /// <para>
    /// Namespace for classes related to configuring Permissions
    /// </para>
    /// <para>
    /// <strong>Warning:</strong> The API here is experimental and may changed/be removed in future releases
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Namespace for specialised node implementations and the <see cref="IValuedNode"/> interface, this is an extension of the <see cref="INode"/> interface that provides strongly typed access to the value of a node.
    /// <para>
    /// These implementations are primarily used internally in the SPARQL engine, however as these all derive from the standard <see cref="INode"/> implementations they can be used interchangeably with those if desired.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Ontology
{
    /// <summary>
    /// <para>
    /// The Ontology Namespace is based upon <a href="http://jena.sourceforge.net/ontology/">Jena's Ontology API</a>.  It allows for a more ontology-centric way of manipulating RDF graphs within the dotNetRDF API.
    /// </para>
    /// <para>
    /// The <see cref="OntologyResource">OntologyResource</see> is the base class of resources and allows for the retrieval and manipulation of various common properties of a resource.  More specialised classes like <see cref="OntologyClass">OntologyClass</see> and <see cref="OntologyProperty">OntologyProperty</see> are used to work with classes and properties etc.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// <para>
    /// Namespace for Parsing classes and variety of supporting Classes.
    /// </para>
    /// <para>
    /// Classes here are primarily implementations of <see cref="IRdfReader">IRdfReader</see> with some implementations of <see cref="IStoreReader">IStoreReader</see> and a few other specialised classes.
    /// </para>
    /// <para>
    /// Has child namespaces <see cref="VDS.RDF.Parsing.Events">Events</see> and <see cref="VDS.RDF.Parsing.Tokens">Tokens</see> for supporting Event and Token based Parsing.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// <para>
    /// Namespace for Parser Context classes, these are classes that are used internally by parsers to store their state.  This allows parsers to be safely used in a multi-threaded scenario so the parsing of one Graph/Store cannot affect the parsing of another.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Parsing.Events
{
    /// <summary>
    /// Namespace for Event classes which are used to support Event Based parsing of RDF syntaxes
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// <para>
    /// Namespace for RDF and SPARQL Results Handlers
    /// </para>
    /// <para>
    /// Handlers are a powerful low level part of the parsers API, they allow you to parse RDF, RDF Datasets and SPARQL Results in such a way that you can take arbitrary actions with the data and choose to end parsing as soon as desired.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Parsing.Tokens
{
    /// <summary>
    /// <para>
    /// Namespace for Token classes which are used to support Token Based parsing of RDF syntaxes
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Parsing.Validation
{
    /// <summary>
    /// <para>
    /// Namespace for Validator classes that can be used to validate various forms of syntax
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query
{
    /// <summary>
    /// <para>
    /// Namespace for Query Classes which provide querying capabilities on RDF.
    /// </para>
    /// <para>
    /// Query capabilities are centered around support for the SPARQL standard.  You can execute full SPARQL 1.1 queries over in-memory data or submit queries to remote SPARQL endpoints.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Aggregates
{
    /// <summary>
    /// <para>
    /// Namespace for Aggregate classes which implement Aggregate functions for SPARQL
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Aggregates.Leviathan
{
    /// <summary>
    /// Namespace for aggregates provided by the Leviathan function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Namespace for the built-in SPARQL aggregates
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Aggregates.XPath
{
    /// <summary>
    /// Namespace for aggregates provided by the XPath function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// <para>
    /// Contains the classes which model the mapping of SPARQL queries into the SPARQL Algebra.  This namespace is a key component of the new <strong>Leviathan</strong> SPARQL engine introduced in the 0.2.x builds of dotNetRDF
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Construct
{
    /// <summary>
    /// <para>
    /// Namespace for classes used in executing CONSTRUCT queries
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }

}

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// <para>
    /// Namespace for classes used to define a Dataset over which SPARQL Queries and Updates evaluated using the Leviathan engine operate
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Describe
{
    /// <summary>
    /// <para>
    /// Namespace for classes which implement algorithms for executing DESCRIBE queries
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions
{
    /// <summary>
    /// <para>
    /// Namespace containing all the classes related to the execution of expressions in SPARQL queries.  Any valid expression should be able to be modelled and executed using these clases.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Arithmetic
{
    /// <summary>
    /// Namespace containing expression classes pertaining to arithmetic operations
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Comparison
{
    /// <summary>
    /// Namespace containing expression classes pertaining to comparison operations
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Conditional
{
    /// <summary>
    /// Namespace containing expression classes pertaining to conditional operations
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// <para>
    /// Namespace containing expression classes which model functions in SPARQL expressions
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Namespace containing expression classes which provide the ARQ function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Leviathan
{
    /// <summary>
    /// Namespace containing expression classes which provide the Leviathan function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Hash
{
    /// <summary>
    /// Namespace containing expression classes which provide the hash functions from the Leviathan function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Namespace containing expression classes which provide the numeric functions from the Leviathan function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Namespace containing expression classes which provide the trigonometric functions from the Leviathan function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in functions
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in functions which have boolean results
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in functions which construct new terms
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql.DateTime
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in functions pertaining to date times
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Hash
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in functions pertaining to hash algorithms
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Numeric
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in numeric functions
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in functions pertaining to sets (IN and NOT IN)
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String
{
    /// <summary>
    /// Namespace containing expression classes which provide the SPARQL built-in functions pertaining to string manipulation
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.XPath
{
    /// <summary>
    /// Namespace containing expression classes which provide functions from the XPath function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast
{
    /// <summary>
    /// Namespace containing expression classes which provide cast functions from the XPath function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.XPath.DateTime
{
    /// <summary>
    /// Namespace containing expression classes which provide date time functions from the XPath function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.XPath.Numeric
{
    /// <summary>
    /// Namespace containing expression classes which provide numeric functions from the XPath function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Namespace containing expression classes which provide string functions from the XPath function library
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Expressions.Primary
{
    /// <summary>
    /// Namespace containing expression classes representing primary constructs in SPARQL expression trees i.e. constants, modifiers and variables
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Filters
{
    /// <summary>
    /// <para>
    /// Namespace containing classes pertaining to the filtering of the results of SPARQL queries
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Grouping
{
    /// <summary>
    /// <para>
    /// Namespace containing classes used to apply GROUP BY clauses to SPARQL queries
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Inference
{
    /// <summary>
    /// <para>
    /// Namespace for Inference Classes which provide Inferencing capabilities on RDF - these features are currently experimental and may not work as expected.
    /// </para>
    /// <para>
    /// Classes which implement reasoning must implement the <see cref="IInferenceEngine">IInferenceEngine</see> interface, these can then be attached to classes which implement the <see cref="IInferencingTripleStore">IInferencingTripleStore</see> interface or they can be used to apply inference to any <see cref="IGraph">IGraph</see> implementation with the inferred Triples optionally output to a separate Graph.
    /// </para>
    /// <para>
    /// OWL reasoning currently has extremely limited support, we provide a Pellet client in the <see cref="Pellet">Pellet</see> namespace which can be used to connect to a Pellet Server but that currently only provides reasoning on external knowledge bases on the Pellet Server
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Inference.Pellet
{
    /// <summary>
    /// <para>
    /// Namespace which provides a client for interacting with a Pellet Server
    /// </para>
    /// <para>
    /// Due to Pellet Server being a relatively new product it is currently only possible to reason over external knowledge bases on a Pellet Server and not to use Pellet to reason over in-memory data.  As Pellet Server is updated in the future this client will be updated to take advantage of those updates and to eventually provide for in-memory reasoning.  You may also want to consider using the <see cref="VDS.RDF.Storage.StardogConnector"/> which is the triple store from the same people who developed Pellet and which integrates some Pellet capabilities.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Inference.Pellet.Services
{
    /// <summary>
    /// <para>
    /// Namespace which provides classes which represent the Services offered by a Pellet Server knowledge base
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Operators
{
    /// <summary>
    /// <para>
    /// Namespace which provides classes which represent the implementation of various operators in SPARQL.  This allows for some of the basic operators like + and - to be extended to allow functionality beyond the SPARQL specification such as date time arithmetic.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Operators.DateTime
{
    /// <summary>
    /// <para>
    /// Namespace which provides implementations of <see cref="ISparqlOperator"/> which allow for embedding date time arithmetic into SPARQL queries
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Operators.Numeric
{
    /// <summary>
    /// <para>
    /// Namespace which provides implementations of <see cref="ISparqlOperator"/> which provide the default numeric implementations of operators as required by the SPARQL specification
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Optimisation
{
    /// <summary>
    /// <para>
    /// Namespace containing classes that are used in the Optimisation of SPARQL Queries.  Includes the interfaces <see cref="IQueryOptimiser">IQueryOptimiser</see> and <see cref="IAlgebraOptimiser">IAlgebraOptimiser</see> which can be used to implement custom query optimisation. 
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Ordering
{
    /// <summary>
    /// <para>
    /// Namespace containing classes used to order the results of SPARQL queries
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Paths
{
    /// <summary>
    /// <para>
    /// Contains the classes which model property paths in SPARQL, they can be used to both represent and evaluate a property path as part of a SPARQL query.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Namespace for Pattern Classes that are used in the Graph and Triple matching process for executing SPARQL queries on <see cref="IInMemoryQueryableStore">IInMemoryQueryableStore</see> objects
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Query.PropertyFunctions
{
    /// <summary>
    /// Namespace which provide classes relating to the property function extension point of SPARQL
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Skos
{
    /// <summary>
    /// Namespace containing classes implementing the Simple Knowledge Organization System (SKOS)
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Storage
{
    /// <summary>
    /// <para>
    /// Namespace for storage classes which provide support for using arbitrary backing Stores
    /// </para>
    /// <para>
    /// Storage is managed via the <see cref="IStorageProvider">IStorageProvider</see> interface, see the <a href="http://www.dotnetrdf.org/content.asp?pageID=Triple%20Store%20Integration">Triple Store Integration</a> documentation on the main website for more detail.
    /// </para>
    /// <h3>Data Provider Libraries</h3>
    /// <para>
    /// From the 0.5.0 release onwards any triple store integration that requires additional dependencies are provided with their own library to reduce dependencies in the Core library and allow that functionality to be optional.  The following stores are currently provided in separate libraries:
    /// </para>
    /// <ul>
    ///     <li>Virtuoso - Virtuoso support can be found in the <strong>dotNetRDF.Data.Virtuoso.dll</strong> library and requires one additional dependency.</li>
    /// </ul>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Storage.Management
{
    /// <summary>
    /// <para>
    /// Namespace for storage classes which provide support for managing servers that provide multiple backing Stores
    /// </para>
    /// <para>
    /// Servers are managed via the <see cref="IStorageServer"/> interface, a server can provide lists of available stores, retrieve a reference to a store, create new stores and delete existing stores.  The exact capabilites may depend on the implementation and may be inspected via the <see cref="IStorageServer.IOBehaviour"/> property.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Storage.Management.Provisioning
{
    /// <summary>
    /// <para>
    /// Namespace for storage classes which provide support for creating new stores in conjunction with a <see cref="IStorageServer"/>
    /// </para>
    /// <para>
    /// In order for an <see cref="IStorageServer"/> to create a new store it requires an instance of the <see cref="IStoreTemplate"/> interface from this namespace.  The basic interface provides only a Store ID, specific implementations may provide many more customizable properties to allow new stores to be created that take advantage of the capabilties of the server the store is being created on.  A <see cref="IStorageServer"/> provides methods to generate the basic templates that it accepts and should be used in preference to creating any of the implementations directly.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Storage.Management.Provisioning.Sesame
{
    /// <summary>
    /// <para>
    /// Namespace containing implementations of <see cref="IStoreTemplate"/> which provide templates for creating new stores on Sesame servers
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Storage.Management.Provisioning.Stardog
{
    /// <summary>
    /// <para>
    /// Namespace containing implementations of <see cref="IStoreTemplate"/> which provide templates for creating new stores on Stardog servers
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Update
{
    /// <summary>
    /// <para>
    /// Namespace for performing updates on Triple Stores using SPARQL Update
    /// </para>
    /// <para>
    /// This is a new part of the API introduced in the 0.3.0 release and adds support for using SPARQL to update Triple Stores.  SPARQL Update is part of the new SPARQL 1.1 standard and provides syntax for inserting, modifying and deleting data as well as managing graphs in a store.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Update.Commands
{
    /// <summary>
    /// <para>
    /// Namespace containing classes which model SPARQL Update Commands.  These can be used both to represent SPARQL Updates and to execute them over in-memory stores.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// <para>
    /// Namespaces containing classes which implement the SPARQL Graph Store HTTP Protocol for RDF Graph Management
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Web
{
    /// <summary>
    /// <para>
    /// Namespace for Classes designed to aid the deployment of Linked Data, SPARQL Endpoints and other Semantic Web technologies as part of ASP.Net web applications.
    /// </para>
    /// <para>
    /// The ASP.Net support leverages the <see cref="VDS.RDF.Configuration">Configuration API</see> heavily and so only requires only 1 &lt;appSetting&gt; like so:
    /// <code>
    /// &lt;add key="dotNetRDFConfig" value="~/App_Data/config.ttl" /&gt;
    /// </code>
    /// This setting provides a pointer to an RDF configuration graph that uses the <a href="http://www.dotnetrdf.org/configuration#">Configuration Vocabulary</a> to express the configuration of HTTP Handlers for your ASP.Net application.  We also now provide a command line tool <a href="http://www.dotnetrdf.org/content.asp?pageID=rdfWedDeploy">rdfWebDeploy</a> which can be used to automate the testing and deployment of this configuration.  See documentation on the <a href="http://www.dotnetrdf.org/content.asp?pageID=Configuration%20API">Configuration API</a> for more detail.  Individual handler documentation gives basic examples of Handler configurations.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Web.Configuration
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration classes which are used to load and store the configuration settings for HTTP Handlers provided as part of the <strong>Web</strong> namespace.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Web.Configuration.Protocol
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration classes which are used to load and store the configuration settings for SPARQL Graph Store HTTP Protocol
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Web.Configuration.Query
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration classes which are used to load and store the configuration settings for SPARQL Query handlers
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Web.Configuration.Resource
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration classes which are used to load and store the configuration settings for handlers which serve resources such as Graphs
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Web.Configuration.Server
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration classes which are used to load and store the configuration settings for SPARQL Servers
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Web.Configuration.Update
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration classes which are used to load and store the configuration settings for SPARQL Update handlers
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Writing
{
    /// <summary>
    /// <para>
    /// Namespace for Writing Classes which provide the means to Serialize RDF Graphs as concrete RDF syntaxes or graphical representations.
    /// </para>
    /// <para>
    /// Also contains classes that can be used to save Graphs and Triple Stores to arbitrary database backed storage using classes from the <see cref="VDS.RDF.Storage">Storage</see> namespace.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// <para>
    /// Namespace for Writer Context classes, these are classes that are used internally by writers to store their state.  This allows writers to be safely used in a multi-threaded scenario since the writing of one Graph/Store cannot affect the writing of another.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Writing.Formatting
{
    /// <summary>
    /// <para>
    /// Namespace for Formatter Classes which can be used to format <see cref="VDS.RDF.Triple">Triples</see>, <see cref="VDS.RDF.INode">Nodes</see> and <see cref="System.Uri">URIs</see> among other types.
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Writing.Serialization
{
    /// <summary>
    /// <para>
    /// Namespace for classes related to .Net serialization integration in the library
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

