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
using System.IO;
using System.Linq;
using System.Net;

namespace VDS.Web
{
    /// <summary>
    /// Represents the set of Form Variables because the <see cref="HttpListenerContext">HttpListenerContext</see> does not do this for us
    /// </summary>
    public class FormVariables
    {
        private Dictionary<String, List<String>> _values = new Dictionary<String, List<String>>();
        private bool _valid = true;

        /// <summary>
        /// MIME Type for URL Encoded WWW Form Content used when POSTing over HTTP
        /// </summary>
        private const String WWWFormURLEncoded = "application/x-www-form-urlencoded";

        /// <summary>
        /// Creates a new set of Form Variables
        /// </summary>
        /// <param name="context">Context</param>
        public FormVariables(HttpServerContext context)
            : this(context.InnerContext) { }

        /// <summary>
        /// Creates a new set of Form Variables
        /// </summary>
        /// <param name="context">Context</param>
        public FormVariables(HttpListenerContext context)
        {
            if (context.Request.HasEntityBody && context.Request.ContentType.Equals(WWWFormURLEncoded))
            {
                String data = String.Empty;
                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                {
                    data = reader.ReadToEnd();
                    reader.Close();
                }

                if (data.Length > 0)
                {
                    String[] pairs;
                    if (data.Contains('&'))
                    {
                        pairs = data.Split('&');
                    }
                    else
                    {
                        pairs = new String[] { data };
                    }

                    foreach (String pair in pairs)
                    {
                        if (pair.Contains('='))
                        {
                            String[] kvp = pair.Split(new char[] { '=' }, 2);
                            if (!this._values.ContainsKey(kvp[0]))
                            {
                                this._values.Add(Uri.UnescapeDataString(kvp[0]), new List<String>());
                            }
                            this._values[kvp[0]].Add(Uri.UnescapeDataString(kvp[1]));
                        }
                        else
                        {
                            this._valid = false;
                            this._values.Clear();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets whether the form variables were well formed
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this._valid;
            }
        }

        /// <summary>
        /// Gets a Variable
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <returns></returns>
        /// <remarks>
        /// If there are multiple values for this variable only the first is returned by this method
        /// </remarks>
        public String this[String name]
        {
            get
            {
                if (this._values.ContainsKey(name))
                {
                    return this._values[name].FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets all values for a Variable
        /// </summary>
        /// <param name="name">Variable</param>
        /// <returns></returns>
        public String[] GetValues(String name)
        {
            if (this._values.ContainsKey(name))
            {
                return this._values[name].ToArray();
            }
            else
            {
                return Enumerable.Empty<String>().ToArray();
            }
        }
    }
}
