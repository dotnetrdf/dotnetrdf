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
