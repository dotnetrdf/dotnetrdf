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

namespace VDS.Web
{
    /// <summary>
    /// Represents the state of the server, essentially provides a global Key Value collection to the server
    /// </summary>
    public class HttpServerState
    {
        private Dictionary<String, Object> _state = new Dictionary<string, object>();

        /// <summary>
        /// Gets/Sets an item in the state
        /// </summary>
        /// <param name="name">Key</param>
        /// <returns>Value or null</returns>
        public Object this[String name]
        {
            get
            {
                if (this._state.ContainsKey(name))
                {
                    return this._state[name];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                lock (this._state)
                {
                    if (this._state.ContainsKey(name))
                    {
                        this._state[name] = value;
                    }
                    else
                    {
                        this._state.Add(name, value);
                    }
                }
            }
        }
    }
}
