/*

Copyright Robert Vesse 2009-12
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Connections
{
    class ConnectionInfo
    {
        private IGenericIOManager _manager;

        public ConnectionInfo(IGenericIOManager manager)
        {
            this._manager = manager;
        }

        [Category("Basic"),Description("The name of the connection")]
        public String Name
        {
            get
            {
                return this._manager.ToString();
            }
        }

        [Category("Basic"), Description("The .Net type of the connection")]
        public String Type
        {
            get
            {
                return this._manager.GetType().AssemblyQualifiedName;
            }
        }

        [Category("Basic"), Description("Is the connection read-only?")]
        public bool IsReadOnly
        {
            get
            {
                return this._manager.IsReadOnly;
            }
        }

        [Category("Basic"), Description("Is the connection ready for use?")]
        public bool IsReady
        {
            get
            {
                return this._manager.IsReady;
            }
        }

        [Category("Basic"), Description("The actual connection object, may be expanded to see implementation specific additional information"),
         TypeConverter(typeof(ExpandableObjectConverter))]
        public IGenericIOManager Connection
        {
            get
            {
                return this._manager;
            }
        }

        [Category("Features"),Description("Is SPARQL Query supported by this connection?")]
        public bool IsQueryable
        {
            get
            {
                return (this._manager is IQueryableGenericIOManager);
            }
        }

        [Category("Features"),Description("Is SPARQL Update supported by this connection and if so how?  Possible values are Native, Approximated and Not Supported")]
        public String UpdateMode
        {
            get
            {
                if (this._manager is IUpdateableGenericIOManager)
                {
                    return "Native";
                }
                else if (!this._manager.IsReadOnly)
                {
                    return "Approximated";
                }
                else
                {
                    return "Not Supported";
                }
            }
        }

        [Category("Features"),Description("Is the connection capable of listing graphs?")]
        public bool CanListGraphs
        {
            get
            {
                return this._manager.ListGraphsSupported;
            }
        }

        [Category("Features"),Description("Is the connection capable of deleting graphs?")]
        public bool CanDeleteGraphs
        {
            get
            {
                return this._manager.DeleteSupported;
            }
        }

        [Category("Features"),Description("When a data import is performed with Store Manager will the imported data be appended to any existing data in the target graph(s) or will it overwrite it?  If this property is false exact import behaviour can be determined by checking the SaveToDefaultBehaviour and SaveToNamedBehaviour properties.")]
        public bool WillImportsAppend
        {
            get
            {
                return !this._manager.IsReadOnly && this._manager.UpdateSupported;
            }
        }

        [Category("IO Capabilities"),Description("Shows if the connection works in Triples or Quads")]
        public String StorageType
        {
            get
            {
                if ((this._manager.IOBehaviour & IOBehaviour.IsTripleStore) != 0)
                {
                    return "Triples";
                }
                else if ((this._manager.IOBehaviour & IOBehaviour.IsQuadStore) != 0)
                {
                    return "Quads";
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        [Category("IO Capabilities"),Description("Does the connection support an explicit default graph?")]
        public bool IsDefaultGraphSupported
        {
            get
            {
                return ((this._manager.IOBehaviour & IOBehaviour.HasDefaultGraph) != 0);
            }
        }

        [Category("IO Capabilities"),Description("Does the connection support named graphs?")]
        public bool AreNamedGraphsSupported
        {
            get
            {
                return ((this._manager.IOBehaviour & IOBehaviour.HasNamedGraphs) != 0);
            }
        }

        [Category("IO Capabilities"),Description("How does the connection handle saving graph data to the default graph?  Possible values are Overwrite, Append, N/A (Read Only) and Unknown.  Please note that when using the Import feature Store Manager will always attempt to append data unless the connection does not allow this, please see the WillImportsAppend property to see what the behaviour for this connection will be when used within Store Manager.  Note - If that property is false then this behaviour applies for triples in the default graph.")]
        public String SaveToDefaultBehaviour
        {
            get
            {
                if (this._manager.IsReadOnly)
                {
                    return "N/A (Read Only)";
                }
                else if ((this._manager.IOBehaviour & IOBehaviour.OverwriteDefault) != 0)
                {
                    return "Overwrite";
                }
                else if ((this._manager.IOBehaviour & IOBehaviour.AppendToDefault) != 0)
                {
                    return "Append";
                }
                else
                {
                    return "Unknown";
                }
            }
        }

        [Category("IO Capabilities"), Description("How does the connection handle saving graph data to named graphs?  Possible values are Overwrite, Append, N/A (Read Only) and Unknown.  Please note that when using the Import feature Store Manager will always attempt to append data unless the connection does not allow this, please see the WillImportsAppend property to see what the behaviour for this connection will be when used within Store Manager.  Note - If that property is false then this behaviour applies for triples in named graphs.")]
        public String SaveToNamedBehaviour
        {
            get
            {
                if (this._manager.IsReadOnly)
                {
                    return "N/A (Read Only)";
                }
                else if ((this._manager.IOBehaviour & IOBehaviour.OverwriteNamed) != 0)
                {
                    return "Overwrite";
                }
                else if ((this._manager.IOBehaviour & IOBehaviour.AppendToNamed) != 0)
                {
                    return "Append";
                }
                else
                {
                    return "Unknown";
                }
            }
        }
    }
}
