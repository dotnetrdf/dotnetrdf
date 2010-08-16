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

#if !NO_WEB && !NO_ASP

namespace VDS.RDF.Web
{
    /// <summary>
    /// Possible Database Types for Handlers which use a SQL Based Store
    /// </summary>
    public enum HandlerDBTypes
    {
        /// <summary>
        /// Connect to a Microsoft SQL Server based Store
        /// </summary>
        MSSQL,
        /// <summary>
        /// Connect to a MySQL based Store
        /// </summary>
        MySQL,
        /// <summary>
        /// Connect to a non-native Virtuoso based Store (eg Microsoft SQL via Virtuoso)
        /// </summary>
        Virtuoso
    }

    /// <summary>
    /// Possible Store Types for Handlers which use a Native Triple Store
    /// </summary>
    public enum HandlerStoreTypes
    {
        /// <summary>
        /// Connect to a Talis Platform Store
        /// </summary>
        Talis,
        /// <summary>
        /// Connect to a Virtuoso Universal Server native Quad Store
        /// </summary>
        Virtuoso,
        /// <summary>
        /// Connect to a 4store Server
        /// </summary>
        FourStore,
        /// <summary>
        /// Connect to any server supporting the Sesame 2 HTTP Protocol
        /// </summary>
        Sesame2HTTP,
        /// <summary>
        /// Connect to an AllegroGraph Server
        /// </summary>
        AllegroGraph
    }

    /// <summary>
    /// Possible data Loading Modes for the <see cref="SparqlHandler">SparqlHandler</see>
    /// </summary>
    public enum SparqlLoadMode
    {
        /// <summary>
        /// Graphs are loaded On Demand as required by the Query (Default)
        /// </summary>
        /// <remarks>Graphs are loaded only if they are mentioned in a FROM, FROM NAMED or GRAPH clause</remarks>
        OnDemand,
        /// <summary>
        /// Graphs are loaded On Demand as required by the Query, Query is analysed prior to execution in order that additional pertinent Graphs can be loaded.
        /// </summary>
        /// <remarks>
        /// Any Uri mentioned in the query that does not correspond to a widely used Namespaces (RDF, RDFS and XML Schema plus User defined exclusions) will be looked up in the Store.  A number of Graphs using that Uri will be loaded, this may be all the Graphs that use that Uri but the Handler will decide based on the amount of work to be done in loading the Graphs.
        /// </remarks>
        OnDemandEnhanced,
        /// <summary>
        /// Graphs are loaded On Demand as required by the Query, Query is analysed prior to execution in order that additional pertinent Graphs can be loaded.
        /// </summary>
        /// <remarks>As <see cref="SparqlLoadMode.OnDemandEnhanced">OnDemandEnhanced</see> except that all URIs are used regardless of their Namespace</remarks>
        OnDemandAggressive,
        /// <summary>
        /// The entire Triple Store must be loaded into memory before the Sparql Handler will function
        /// </summary>
        PreloadAll,
        /// <summary>
        /// The entire Triple Store will be loaded into memory asynchronously allowing the Sparql Handler to function immediately albeit with incomplete data
        /// </summary>
        PreloadAllAsync
    }

#if !NO_DATA && !NO_STORAGE

    /// <summary>
    /// Possible Resource Lookup Modes for the <see cref="SqlResourceHandler">SqlResourceHandler</see>
    /// </summary>
    public enum SQLResourceLookupMode
    {
        /// <summary>
        /// URIs must correspond to a Graph Uri in order that a resource can be retrieved
        /// </summary>
        Graph,
        /// <summary>
        /// URIs may correspond to either a Graph Uri or must occur as a Subject in some Triples in order that a resource can be retrieved
        /// </summary>
        GraphOrDescribe,
        /// <summary>
        /// URIs must occur as the Subject of some Triples in order that a resource can be retrieved
        /// </summary>
        Describe
    }

#endif
}

#endif