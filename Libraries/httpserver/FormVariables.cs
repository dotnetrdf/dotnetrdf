using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web
{
    public class FormVariables
    {
        private Dictionary<String, List<String>> _values = new Dictionary<String, List<String>>();
        private bool _valid = true;

        /// <summary>
        /// MIME Type for URL Encoded WWW Form Content used when POSTing over HTTP
        /// </summary>
        private const String WWWFormURLEncoded = "application/x-www-form-urlencoded";

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
                                this._values.Add(kvp[0], new List<String>());
                            }
                            this._values[kvp[0]].Add(kvp[1]);
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

        public bool IsValid
        {
            get
            {
                return this._valid;
            }
        }

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
