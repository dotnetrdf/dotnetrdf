/*

Copyright Robert Vesse 2009-11
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

using System;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Flags Enumeration which is used to express the IO Behaviour of a specific Store
    /// </summary>
    [Flags]
    public enum IOBehaviour
    {
        /// <summary>
        /// Indicates whether the Store is Read Only i.e. Saving is not supported
        /// </summary>
        IsReadOnly = 0,

        /// <summary>
        /// Indicates that the Store is a Triple Store
        /// </summary>
        IsTripleStore = 1,
        /// <summary>
        /// Indicates that the Store is a Quad (Graph) Store
        /// </summary>
        IsQuadStore = 2,

        /// <summary>
        /// Indicates whether the Store has an explicit unnamed default graph
        /// </summary>
        HasDefaultGraph = 4,
        /// <summary>
        /// Indicates whether the Store has named graphs
        /// </summary>
        HasNamedGraphs = 8,

        /// <summary>
        /// Indicates that a Triple Store appends Triples when the SaveGraph() method is used
        /// </summary>
        AppendTriples = 16,
        /// <summary>
        /// Indicates that a Triple Store overwrites Triples when the SaveGraph() method is used
        /// </summary>
        OverwriteTriples = 32,

        /// <summary>
        /// Indicates that Graph data written to the Default Graph is always appended when the SaveGraph() method is used
        /// </summary>
        AppendToDefault = 64,
        /// <summary>
        /// Indicates that Graph data written to the Default Graph overwrites existing data when the SaveGraph() method is used
        /// </summary>
        OverwriteDefault = 128,

        /// <summary>
        /// Indicates that Graph data written to Named Graphs is always appended when the SaveGraph() method is used
        /// </summary>
        AppendToNamed = 256,
        /// <summary>
        /// Indicates that Graph data written to Named Graphs overwrites existing data when the SaveGraph() method is used
        /// </summary>
        OverwriteNamed = 512,

        /// <summary>
        /// Indicates a Store that can do Triple Level additions on existing Graphs using the UpdateGraph() method
        /// </summary>
        CanUpdateAddTriples = 1024,
        /// <summary>
        /// Indicates a Store that can do Triple Level removals on existing Graphs using the UpdateGraph() method
        /// </summary>
        CanUpdateDeleteTriples = 2048,

        /// <summary>
        /// Indicates a Store that can do Triple Level additions and removals on existing Graphs using the UpdateGraph() method
        /// </summary>
        CanUpdateTriples = CanUpdateAddTriples | CanUpdateDeleteTriples,

        /// <summary>
        /// Default Behaviour for Read Only Triple Stores
        /// </summary>
        ReadOnlyTripleStore = IsReadOnly | IsTripleStore,
        /// <summary>
        /// Default Behaviour for Read Only Quad (Graph) Stores
        /// </summary>
        ReadOnlyGraphStore = IsReadOnly | IsQuadStore | HasDefaultGraph | HasNamedGraphs,

        /// <summary>
        /// Default Behaviour for Triple Stores
        /// </summary>
        /// <remarks>
        /// Default Behaviour is considered to be a Triple Store where data is appended
        /// </remarks>
        TripleStore = IsTripleStore | AppendTriples,
        /// <summary>
        /// Default Behaviour for Quad (Graph) Stores
        /// </summary>
        /// <remarks>
        /// Default Behaviour is considered to be Quad Store with Default and Named Graphs, data is appended to the default graph and overwrites named graphs
        /// </remarks>
        GraphStore = IsQuadStore | HasDefaultGraph | HasNamedGraphs | AppendToDefault | OverwriteNamed,
    }
}
