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

using System;
using System.Collections.Generic;
using System.Threading;

namespace VDS.RDF
{
    /// <summary>
    /// Mapper class which creates Blank Node IDs and ensures that auto-assigned and user specified IDs don't collide
    /// </summary>
    public class BlankNodeMapper
    {
        private Dictionary<String, BlankNodeIDAssigment> _idmap = new Dictionary<string, BlankNodeIDAssigment>();
        private Dictionary<String, String> _remappings = new Dictionary<string, string>();
        private static long _nextid = 0;
        private static long _nextremap = 0;
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
            _prefix = prefix;
        }

        /// <summary>
        /// Gets the next available auto-assigned Blank Node ID
        /// </summary>
        /// <returns></returns>
        public String GetNextID()
        {
            String id = _prefix + Interlocked.Increment(ref _nextid);

            // Check it's not in use
            while (_idmap.ContainsKey(id))
            {
                id = _prefix + Interlocked.Increment(ref _nextid);
            }

            // Add to ID Map
            _idmap.Add(id, new BlankNodeIDAssigment(id, true));

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
            if (_remappings.ContainsKey(id))
            {
                // Is remapped to something else
                id = _remappings[id];
            } 
            else if (_idmap.ContainsKey(id))
            {
                BlankNodeIDAssigment idinfo = _idmap[id];
                if (idinfo.AutoAssigned)
                {
                    // This ID has been auto-assigned so remap to something else
                    String newid = "remapped" + Interlocked.Increment(ref _nextremap);
                    while (_idmap.ContainsKey(newid))
                    {
                        newid = "remapped" + Interlocked.Increment(ref _nextremap);
                    }

                    // Add to ID Map
                    _idmap.Add(newid, new BlankNodeIDAssigment(newid, false));
                    _remappings.Add(id, newid);
                    id = newid;
                }
                // Otherwise this ID can be used fine
            }
            else
            {
                // Register the ID
                _idmap.Add(id, new BlankNodeIDAssigment(id, false));
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
            _validator = validator;
        }

        /// <summary>
        /// Takes a ID, validates it and returns either the ID or an appropriate remapped ID
        /// </summary>
        /// <param name="id">ID to map</param>
        /// <returns></returns>
        public String GetOutputID(String id)
        {
            if (_validator(id))
            {
                // A Valid ID for outputting
                if (!_remappings.ContainsKey(id))
                {
                    // Check that our Value hasn't been used as the remapping of an invalid ID
                    if (!_remappings.ContainsValue(new BlankNodeIDAssigment(id, true)))
                    {
                        // We're OK
                        _remappings.Add(id, new BlankNodeIDAssigment(id, false));
                        return id;
                    }
                    else
                    {
                        // Our ID has already been remapped from another ID so we need to remap ourselves
                        String remappedID = GetNextID();
                        _remappings.Add(id, new BlankNodeIDAssigment(remappedID, false));
                        return remappedID;
                    }
                }
                else 
                {
                    return _remappings[id].ID;
                }

            }
            else if (_remappings.ContainsKey(id))
            {
                // Already validated/remapped
                return _remappings[id].ID;
            } 
            else
            {
                // Not valid for outputting so need to remap
                String remappedID = GetNextID();
                _remappings.Add(id, new BlankNodeIDAssigment(remappedID, true));
                return remappedID;
            }
        }

        /// <summary>
        /// Internal Helper function which generates the new IDs
        /// </summary>
        /// <returns></returns>
        private String GetNextID()
        {
            String nextID = "autos" + _nextid;
            while (_remappings.ContainsKey(nextID))
            {
                _nextid++;
                nextID = "autos" + _nextid;
            }
            _nextid++;

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
            _id = id;
            _auto = auto;
        }

        /// <summary>
        /// Assigned ID
        /// </summary>
        public String ID
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Whether the ID is auto-assigned
        /// </summary>
        public bool AutoAssigned
        {
            get
            {
                return _auto;
            }
        }

        /// <summary>
        /// Returns whether a given Object is equal to this Blank Node ID assignment
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is BlankNodeIDAssigment other)
            {
                return (other.ID.Equals(_id) && other.AutoAssigned == _auto);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
