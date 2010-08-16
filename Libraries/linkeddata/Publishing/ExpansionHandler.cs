using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace VDS.RDF.LinkedData.Publishing
{
    public class ExpansionHandler : IHttpHandler
    {
        private ExpansionLoader _loader;
        private String _cacheDir = "~/App_Data/expansion_cache/";

        public ExpansionHandler()
        {
            String cacheDir = ConfigurationManager.AppSettings["ExpansionHandlerCache"];
            if (cacheDir != null)
            {
                this._cacheDir = cacheDir;
            }
        }

        public bool IsReusable
        {
            get 
            {
                return true; 
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            //Turn on Response Buffering
            context.Response.Buffer = true;

            //Prepare the Cache Directories
            if (!Path.IsPathRooted(this._cacheDir))
            {
                this._cacheDir = context.Server.MapPath(this._cacheDir);
            }
            if (this._loader == null) this._loader = new ExpansionLoader(this._cacheDir);

            //Add our Custom Headers
            try
            {
                context.Response.Headers.Add("X-dotNetRDF-Version", Assembly.GetAssembly(typeof(VDS.RDF.IGraph)).GetName().Version.ToString());
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("X-dotNetRDF-Version", Assembly.GetAssembly(typeof(VDS.RDF.IGraph)).GetName().Version.ToString());
            }

            try
            {

                //Retrieve the desired URI and Profile URI from Querystring parameters
                String uri = context.Request.QueryString["uri"];
                String profile = context.Request.QueryString["profile"];

                if (uri == null)
                {
                    if (context.Request.Url.Query.Equals(String.Empty))
                    {
                        throw new ArgumentNullException("uri", "Required uri parameter used to designate the URI you wish to expand was not found.  Your request must use a URI of the form " + context.Request.Url.ToString() + "?uri=" + Uri.EscapeDataString("http://example.org"));
                    }
                    else
                    {
                        throw new ArgumentNullException("uri", "Required uri parameter used to designate the URI you wish to expand was not found.  Your request must use a URI of the form " + context.Request.Url.ToString().Replace(context.Request.Url.Query, String.Empty) + "?uri=" + Uri.EscapeDataString("http://example.org"));
                    }
                }

                //Note that the ExpansionLoader class automatically handles all the Caching for us
                IInMemoryQueryableStore store;
                String uriHash = new Uri(uri).GetSha256Hash();
                if (profile == null)
                {
                    //Use Default Profile
                    store = this._loader.Load(new Uri(uri));
                }
                else
                {
                    //Use Custom Profile
                    store = this._loader.Load(new Uri(uri), new Uri(profile));
                }

                String ctype;
                IStoreWriter writer = MimeTypesHelper.GetStoreWriter(context.Request.AcceptTypes, out ctype);
                context.Response.ContentType = ctype;
                writer.Save(store, new StreamParams(context.Response.OutputStream));
            }
            catch (ArgumentNullException argNull)
            {
                HandleErrors(context, "Missing Argument", argNull);
            }
            catch (RdfParseException parseEx)
            {
                HandleErrors(context, "RDF Parser Error", parseEx);
            }
            catch (RdfException rdfEx)
            {
                HandleErrors(context, "RDF Error", rdfEx);
            }
            catch (Exception ex)
            {
                HandleErrors(context, "Error", ex);
            }
        }

        /// <summary>
        /// Handles errors in processing Sparql Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleErrors(HttpContext context, String title, Exception ex)
        {
            //Clear any existing Response
            context.Response.Clear();

            //if (this._config != null)
            //{
            //    if (!this._config.ShowErrors)
            //    {
            //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //        return;
            //    }
            //}

            //Set to Plain Text output and report the error
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";

            //Error Title
            context.Response.Write(title + "\n");
            context.Response.Write(new String('-', title.Length) + "\n\n");

            //Error Message
            context.Response.Write(ex.Message + "\n");

#if DEBUG
            //Stack Trace only when Debug build
            context.Response.Write(ex.StackTrace + "\n\n");
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                context.Response.Write(ex.Message + "\n");
                context.Response.Write(ex.StackTrace + "\n\n");
            }
#endif
        }

    }
}
