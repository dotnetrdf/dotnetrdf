/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Text;
using VDS.Web;
using VDS.Web.Modules;

namespace VDS.RDF.Utilities.Server
{
    public class StartupModule
        : IHttpListenerModule
    {
        public void Dispose()
        {
            // No op
        }

        public void PreStateChange(HttpServer server, ServerState target)
        {
            if (target != ServerState.Starting) return;

            // Report the path wiring
            foreach (HttpRequestMapping mapping in server.Handlers)
            {
                StringBuilder builder = new StringBuilder();
                switch (mapping.AcceptedPathMode)
                {
                    case PathMode.All:
                        builder.Append("*");
                        break;
                    case PathMode.Extension:
                        builder.Append("*." + mapping.AcceptedPath);
                        break;
                    case PathMode.WildcardPath:
                        builder.Append(mapping.AcceptedPath + "*");
                        break;
                    case PathMode.FixedPath:
                        builder.Append(mapping.AcceptedPath);
                        break;
                }

                builder.Append('\t');
                builder.Append(String.Join(",", mapping.AcceptedVerbs));
                builder.Append('\t');
                builder.Append(mapping.HandlerType.FullName);

                server.Console.Information(builder.ToString());
            }
        }

        public void PostStateChange(HttpServer server)
        {
            // No op
        }

        public bool ProcessRequest(HttpServerContext context)
        {
            // Continue processing
            return true;
        }
    }
}