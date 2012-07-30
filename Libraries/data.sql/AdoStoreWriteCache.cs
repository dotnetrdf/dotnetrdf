/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Linq;
using System.Text;
using System.Threading;
using VDS.Common;

namespace VDS.RDF.Storage
{
    [Obsolete("The Data.Sql Library is being deprecated in favour of the many open source and commercial triple stores supported by the core library which are far more performant.  Please switch over your code to an alternative triple store, we will no longer support/distribute this library after the 0.7.x series of releases", true)]
    class AdoStoreWriteCache
    {
        private HashTable<int, AdoStoreNodeID> _cache = new HashTable<int, AdoStoreNodeID>(1);

        public AdoStoreNodeID GetNodeID(INode n)
        {
            AdoStoreNodeID id = new AdoStoreNodeID(n);
            if (this._cache.TryGetValue(n.GetHashCode(), out id))
            {
                return id;
            }
            else
            {
                return new AdoStoreNodeID(n);
            }
        }

        public void AddNodeID(AdoStoreNodeID id)
        {
            this._cache.Add(id.Node.GetHashCode(), id);
        }
    }

    [Obsolete("The Data.Sql Library is being deprecated in favour of the many open source and commercial triple stores supported by the core library which are far more performant.  Please switch over your code to an alternative triple store, we will no longer support/distribute this library after the 0.7.x series of releases", true)]
    class AdoStoreNodeID
    {
        private INode _n;
        private int _id = 0;

        public AdoStoreNodeID(INode n)
            : this(n, 0) { }

        public AdoStoreNodeID(INode n, int id)
        {
            this._n = n;
            this._id = id;
        }

        public INode Node
        {
            get
            {
                return this._n;
            }
        }

        public int ID
        {
            get
            {
                return this._id;
            }
            set
            {
                if (this._id <= 0)
                {
                    this._id = value;
                }
                else
                {
                    throw new InvalidOperationException("Cannot change the ID once it has been set");
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is AdoStoreNodeID)
            {
                return this._n.Equals(((AdoStoreNodeID)obj).Node);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this._n.GetHashCode();
        }
    }
}
