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

namespace VDS.RDF
{
    /// <summary>
    /// <para>
    /// Top Level Namespace for the <strong>dotNetRDF Library</strong> which embodies a simple but powerful API for manipulating RDF.   
    /// </para>
    /// <para>
    /// Specific Namespaces within the Hierarchy provide <see cref="VDS.RDF.Parsing">Parsing</see>, <see cref="VDS.RDF.Writing">Serialization</see> and <see cref="VDS.RDF.Translation">Translation</see> functionality along with a host of related classes to support these functions.
    /// </para>
    /// <para>
    /// Support for querying RDF is provided in the <see cref="Query">Query</see> namespace which includes SPARQL Query, limited reasoning support in the <see cref="Query.Inference">Query.Inference</see> namespace and a Pellet client in the <see cref="Query.Inference.Pellet">Query.Inference.Pellet</see> namespace.
    /// </para>
    /// <para>
    /// Support for updating RDF based on the SPARQL 1.1 Update and Uniform HTTP Protocol for RDF Graph Management is provided in the <see cref="Update">Update</see> and <see cref="Update.Protocol">Update.Protocol</see> namespaces.
    /// </para>
    /// <para>For communicating with arbitrary Triple Stores we have a dedicated <see cref="VDS.RDF.Storage">Storage</see> namespace.  As of this release we support the following Triple Stores:
    /// <ul>
    ///     <li>AllegroGraph</li>
    ///     <li>4store</li>
    ///     <li>Joseki</li>
    ///     <li>Any Sesame HTTP Protocl compliant store</li>
    ///     <li>Any SPARQL Uniform HTTP Protocol for RDF Graph Management compliant stores</li>
    ///     <li>Talis Platform</li>
    ///     <li>Virtuoso</li>
    /// </ul>
    /// There is also support for using SQL backed stores (only recommended for small scale testing and development) and a couple of other forms of read-only store namely RDF dataset files and SPARQL query endpoints.
    /// </para>
    /// <para>
    /// For those building ASP.Net based websites the <see cref="VDS.RDF.Web">Web</see> namespace is dedicated to providing classes for integrating RDF into ASP.Net applications.  If you've used dotNetRDF for ASP.Net applications prior to Version 0.3.0 please be aware that most of the existing classes were deprecated in favour of new classes which take advantage of the new Configuration API.
    /// </para>
    /// <para>
    /// There is also a fairly new and experimental <see cref="VDS.RDF.Ontology">Ontology</see> namespace which provides a more resource and ontology centric API for working with RDF which was introduced in the 0.2.2 release
    /// </para>
    /// <h4>Configuration</h4>
    /// <para>
    /// As of the 0.3.0 release we introduced a new <see cref="Configuration">Configuration</see> API which provides for encoding configuration in RDF Graphs.  This configuration system has been used as part of a complete refresh of the ASP.Net support as it allows for much more expressive and flexible configurations than were previously possible.  See the <a href="http://www.dotnetrdf.org/content.asp?pageID=Configuration%20API">documentation</a> on the main website for many detailed examples.
    /// </para>
    /// <h4>Notes</h4>
    /// <para>
    /// Currently dotNetRDF is still considered to be in Alpha, this means it may not be suitable for production scenarios and that the API is subject to change in subsequent releases should we feel it necessary.  As it is an Alpha release users should be aware that the software will not be bug free.  While we continually work to improve the quality of this library and to eliminate bugs as we find them we are at the same time attempting to enhance the library by adding more functionality so it is inevitable that some bugs will persist.  Please help us improve this library by emailing us when you find a bug, you can use the <a href="mailto:dotnetrdf-bugs@lists.sourceforge.net">Bug Reports list</a> to report bugs, the <a href="mailto:dotnetrdf-support@lists.sourceforge.net">Support list</a> to ask questions and the <a href="mailto:dotnetrdf-develop@lists.sourceforge.net">Developer list</a> to request new features or discuss development plans (all these are SourceForge mailing lists which require subscription).
    /// </para>
    /// <para>
    /// The 0.3.x release makes very few breaking API changes compared to the 0.2.x API and it is planned that the API should continue to remain relatively stable other than the introduction of new features and the removal of the now deprecated older ASP.Net classes.  Further 0.3.x releases will focus on bringing continued improvements in the ASP.Net support and increased support for reasoning.
    /// </para>
    /// <h4>Alternative Builds</h4>
    /// <h5>Mono Build</h5>
    /// <para>
    /// We provide a Mono build of dotNetRDF (<em>dotNetRDF.Mono.dll</em>) which is currently targeted at Mono 2.6 - this port is highly experimental and has received little/no testing.  There are some known incompatabilities with Mono mostly regarding the 3rd party libraries that dotNetRDF uses - Virtuoso and MySQL support is likely not to function under Mono.  As far as we are aware all other features should work normally, in terms of roadmap the Mono build is not our main priority currently and we will conduct full testing of the Mono build in the future and make an announcement once we consider that build to be stable or have had time to adapt the build appropriately.
    /// </para>
    /// <h5>Silverlight Build</h5>
    /// <para>
    /// We provide a Silverlight build of dotNetRDF (<em>dotNetRDF.Silverlight.dll</em>) which is an experimental build which has been tested by external users but not internally as we don't do any Silverlight development currently.  This build runs on Silverlight 4 and omits the following features since they can't be supported under Silverlight:
    /// </para>
    /// <ul>
    /// <li>Most of the <see cref="VDS.RDF.Storage">Storage</see> namespace and the <see cref="VDS.RDF.Web">Web</see> namespaces</li>
    /// <li>No String normalization support</li>
    /// <li>No <see cref="VDS.RDF.Parsing.UriLoader">UriLoader</see> caching support</li>
    /// <li>No multi-threaded support where <see cref="System.Threading.ReaderWriteLockSlim">ReaderWriterLockSlim</see> is used</li>
    /// <li>Various writers and parsers use streaming rather than DOM based XML parsing</li>
    /// <li>No support for XSL in TriX files</li>
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
    /// As of the 0.3.0 release we introduced this new API which provides for encoding configuration in RDF Graphs.  This configuration system has been used as part of a complete refresh of the ASP.Net support as it allows for much more expressive and flexible configurations than were previously possible.  See the <a href="http://www.dotnetrdf.org/content.asp?pageID=Configuration%20API">documentation</a> on the main website for many detailed examples.
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

namespace VDS.RDF.Ontology
{
    /// <summary>
    /// <para>
    /// The Ontology Namespace is based upon <a href="http://jena.sourceforge.net/ontology/">Jena's Ontology API</a> and is an experimental part of the library.  It allows for a more resource-centric way of manipulating RDF graphs within the dotNetRDF API.
    /// </para>
    /// <para>
    /// The <see cref="OntologyResource">OntologyResource</see> is the base class of resources and allows for the retrieval and manipulation of various common properties of a resource.  More specialised classes like <see cref="OntologyClass">OntologyClass</see> and <see cref="OntologyProperty">OntologyProperty</see> are used to work with classes and properties etc.
    /// </para>
    /// <para>
    /// One key feature of this new part of the API is the <see cref="ReasonerGraph">ReasonerGraph</see> which allows you to wrap an existing Graph with a reasoner to get a unified view over the original Triples and materialised inferences without modifying your original Graph.
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
    /// Namespace for Parsing Classes and variety of supporting Classes.
    /// </para>
    /// <para>
    /// Classes here are primarily implementations of <see cref="ITokeniser">ITokeniser</see> and <see cref="IRdfReader">IRdfReader</see> with some implementations of <see cref="IStoreReader">IStoreReader</see> and a few other specialised classes.
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
    /// Namespace for Parser Context classes, these are classes that are used internally by parsers to store their state.  This allows parsers to be safely used in a multi-threaded scenario since the parsing of one Graph/Store cannot affect the parsing of another.
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
    /// Namespace for Query Classes which provide Querying capabilities on RDF Graphs
    /// </para>
    /// <para>
    /// Query capabilities are provided for two forms of Query:
    /// <ol>
    ///     <li>Basic Graph pattern matching which is implemented via the <see cref="ISelector">ISelector</see> interface</li>
    ///     <li>Sparql Queries
    ///         <ul>
    ///             <li>Full SPARQL over local in-memory Triple Stores</li>
    ///             <li>Full SPARQL over remote endpoints</li>
    ///         </ul>
    ///     </li>
    /// </ol>
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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// <para>
    /// Namespace containing Expression classes which model functions in SPARQL expressions
    /// </para>
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
    /// Due to Pellet Server being a relatively new product it is currently only possible to reason over external knowledge bases on a Pellet Server and not to use Pellet to reason over in-memory data.  As Pellet Server is updated in the future this client will be updated to take advantage of those updates and to eventually provide for in-memory reasoning.
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

namespace VDS.RDF.Storage
{
    /// <summary>
    /// <para>
    /// Namespace for Storage Classes which provide support for using arbitrary backing Stores
    /// </para>
    /// <para>
    /// There are two main kinds of Store supported:
    /// <ol>
    ///     <li>SQL Database based Stores as backing storage for RDF using classes such as <see cref="VDS.RDF.SqlGraph">SqlGraph</see> with implementations of the <see cref="ISqlIOManager">ISqlIOManager</see> interface.</li>
    ///     <li>Arbitrary Stores as backing storage using classes such as <see cref="VDS.RDF.StoreGraph">StoreGraph</see> with implementations of the <see cref="IGenericIOManager">IGenericIOManager</see> interface.</li>
    /// </ol>
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

namespace VDS.RDF.Storage.Params
{
    /// <summary>
    /// Namespace for Parameter Classes which provide Parameter information to <see cref="IStoreReader">IStoreReader</see> and <see cref="IStoreWriter">IStoreWriter</see> implementations
    /// </summary>
    class NamespaceDoc
    {
    }
}

namespace VDS.RDF.Translation
{
    /// <summary>
    /// <para>
    /// Namespace for Translation Classes which provide the ability to translate RDF directly from one format to another
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
    /// Namespaces containing classes which implement the SPARQL Uniform HTTP Protocol for RDF Graph Management
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
    /// As of the 0.3.0 release the ASP.Net support has been heavily rewritten, as opposed to the previous system which required many &lt;appSettings&gt; defining the new system now requires only 1 &lt;appSetting&gt; like so:
    /// <code>
    /// &lt;add key="dotNetRDFConfig" value="~/App_Data/config.ttl" /&gt;
    /// </code>
    /// This setting provides a pointer to an RDF configuration graph that uses the <a href="http://www.dotnetrdf.org/configuration#">Configuration Vocabulary</a> to express the configuration of HTTP Handlers for your ASP.Net application.  We also now provide a command line tool <a href="http://www.dotnetrdf.org/content.asp?pageID=rdfWedDeploy">rdfWebDeploy</a> which can be used to automate the testing and deployment of this configuration.  See documentation on the <a href="http://www.dotnetrdf.org/content.asp?pageID=Configuration%20API">Configuration API</a> for more detail.  Individual handler documentation gives basic examples of Handler configurations.
    /// </para>
    /// <para>
    /// <strong>Note: </strong> As can be seen the old ASP.Net handlers have now all been marked as obsolete, they are left in the library currently to provide compatability with existing applications but we strongly recommend that applications using these migrate to the new Handlers and all new development should use the new Handlers.
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
    /// Namespace for Configuration classes which are used to load and store the configuration settings for SPARQL Uniform HTTP Protocol for RDF Graph Management handlers
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

namespace VDS.RDF.Web.Configuration.Resources
{
    /// <summary>
    /// <para>
    /// Namespace for Configuration classes which are used to load and store the configuration settings for handlers which serve resources like Graphs
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
    /// Namespace for Formatter Classes which can be used to format <see cref="VDS.RDF.Triple">Triples</see>, <see cref="VDS.RDF.INode">Nodes</see> and <see cref="System.Uri">URIs</see>
    /// </para>
    /// </summary>
    class NamespaceDoc
    {

    }
}

