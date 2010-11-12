using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web
{
    public class HttpServerState
    {
        private Dictionary<String, Object> _state = new Dictionary<string, object>();

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
