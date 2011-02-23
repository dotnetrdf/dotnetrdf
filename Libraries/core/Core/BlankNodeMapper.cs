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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Mapper class which creates Blank Node IDs and ensures that auto-assigned and user specified IDs don't collide
    /// </summary>
    public class BlankNodeMapper
    {
        private Dictionary<String, BlankNodeIDAssigment> _idmap = new Dictionary<string, BlankNodeIDAssigment>();
        private Dictionary<String, String> _remappings = new Dictionary<string, string>();
        private int _nextid = 1;
        private int _nextremap = 1;
        private String _prefix = "autos";

        /// <summary>
        /// Creates a new Blank Node Mapper
        /// </summary>
        public BlankNodeMapper()
        { }

        /// <summary>
        /// Creates a new Blank Node Mapper that uses a custom Prefix
        /// </summary>
        /// <param name="prefix">Prefix</param>
        public BlankNodeMapper(String prefix)
        {
            if (prefix == null || prefix.EndsWith(String.Empty)) prefix = "autos";
            this._prefix = prefix;
        }

        /// <summary>
        /// Gets the next available auto-assigned Blank Node ID
        /// </summary>
        /// <returns></returns>
        public String GetNextID()
        {
            String id = this._prefix + this._nextid;

            //Check it's not in use
            while (this._idmap.ContainsKey(id))
            {
                this._nextid++;
                id = this._prefix + this._nextid;
            }
            this._nextid++;

            //Add to ID Map
            this._idmap.Add(id, new BlankNodeIDAssigment(id, true));

            return id;
        }

        /// <summary>
        /// Checks that an ID can be used as a Blank Node ID remapping it to another ID if necessary
        /// </summary>
        /// <param name="id">ID to be checked</param>
        /// <remarks>
        /// If the ID is not known it is added to the ID maps.  If the ID is known but is user-assigned then this can be used fine.  If the ID is known and was auto-assigned then it has to be remapped to a different ID.
        /// </remarks>
        public void CheckID(ref String id)
        {
            if (this._remappings.ContainsKey(id))
            {
                //Is remapped to something else
                id = this._remappings[id];
            } 
            else if (this._idmap.ContainsKey(id))
            {
                BlankNodeIDAssigment idinfo = this._idmap[id];
                if (idinfo.AutoAssigned)
                {
                    //This ID has been auto-assigned so remap to something else
                    String newid = "remapped" + this._nextremap;
                    while (this._idmap.ContainsKey(newid))
                    {
                        this._nextremap++;
                        newid = "remapped" + this._nextremap;
                    }
                    this._nextremap++;

                    //Add to ID Map
                    this._idmap.Add(newid, new BlankNodeIDAssigment(newid, false));
                    this._remappings.Add(id, newid);
                    id = newid;
                }
                //Otherwise this ID can be used fine
            }
            else
            {
                //Register the ID
                this._idmap.Add(id, new BlankNodeIDAssigment(id, false));
            }
        }
    }

    /// <summary>
    /// Mapper class which remaps Blank Node IDs which aren't valid as-is in a given serialization to a new ID
    /// </summary>
    /// <remarks>
    /// This also has to take care of the fact that it's possible that these remapped IDs then collide with existing valid IDs in which case these also have to be remapped
    /// </remarks>
    public class BlankNodeOutputMapper
    {
        private Func<String, bool> _validator;
        private Dictionary<String, BlankNodeIDAssigment> _remappings = new Dictionary<string, BlankNodeIDAssigment>();
        private int _nextid = 1;

        /// <summary>
        /// Creates a new Blank Node ID mapper
        /// </summary>
        /// <param name="validator">Function which determines whether IDs are valid or not</param>
        public BlankNodeOutputMapper(Func<String, bool> validator)
        {
            this._validator = validator;
        }

        /// <summary>
        /// Takes a ID, validates it and returns either the ID or an appropriate remapped ID
        /// </summary>
        /// <param name="id">ID to map</param>
        /// <returns></returns>
        public String GetOutputID(String id)
        {
            if (this._validator(id))
            {
                //A Valid ID for outputting
                if (!this._remappings.ContainsKey(id))
                {
                    //Check that our Value hasn't been used as the remapping of an invalid ID
                    if (!this._remappings.ContainsValue(new BlankNodeIDAssigment(id, true)))
                    {
                        //We're OK
                        this._remappings.Add(id, new BlankNodeIDAssigment(id, false));
                        return id;
                    }
                    else
                    {
                        //Our ID has already been remapped from another ID so we need to remap ourselves
                        String remappedID = this.GetNextID();
                        this._remappings.Add(id, new BlankNodeIDAssigment(remappedID, false));
                        return remappedID;
                    }
                }
                else 
                {
                    return this._remappings[id].ID;
                }

            }
            else if (this._remappings.ContainsKey(id))
            {
                //Already validated/remapped
                return this._remappings[id].ID;
            } 
            else
            {
                //Not valid for outputting so need to remap
                String remappedID = this.GetNextID();
                this._remappings.Add(id, new BlankNodeIDAssigment(remappedID, true));
                return remappedID;
            }
        }

        /// <summary>
        /// Internal Helper function which generates the new IDs
        /// </summary>
        /// <returns></returns>
        private String GetNextID()
        {
            String nextID = "autos" + this._nextid;
            while (this._remappings.ContainsKey(nextID))
            {
                this._nextid++;
                nextID = "autos" + this._nextid;
            }
            this._nextid++;

            return nextID;
        }
    }

    /// <summary>
    /// Records Blank Node assigments
    /// </summary>
    struct BlankNodeIDAssigment
    {
        private String _id;
        private bool _auto;

        /// <summary>
        /// Creates a new Blank Node ID Assigment Record
        /// </summary>
        /// <param name="id">ID to assign</param>
        /// <param name="auto">Was the ID auto-assigned</param>
        public BlankNodeIDAssigment(String id, bool auto)
        {
            this._id = id;
            this._auto = auto;
        }

        /// <summary>
        /// Assigned ID
        /// </summary>
        public String ID
        {
            get
            {
                return this._id;
            }
        }

        /// <summary>
        /// Whether the ID is auto-assigned
        /// </summary>
        public bool AutoAssigned
        {
            get
            {
                return this._auto;
            }
        }

        /// <summary>
        /// Returns whether a given Object is equal to this Blank Node ID assignment
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is BlankNodeIDAssigment)
            {
                BlankNodeIDAssigment other = (BlankNodeIDAssigment)obj;
                return (other.ID.Equals(this._id) && other.AutoAssigned == this._auto);
            }
            else
            {
                return false;
            }
        }
    }
}
