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