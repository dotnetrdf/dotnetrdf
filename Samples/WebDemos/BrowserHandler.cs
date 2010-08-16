using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace WebDemos
{
    //TODO: Convert into a configurable handler and include in dotNetRDF.LinkedData
    class BrowserHandler : IHttpHandler
    {

        public bool IsReusable
        {
            get 
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            String uri = context.Request.QueryString["uri"];
            if (uri == null)
            {
                context.Response.Write("Bad request");
            }
            else
            {
                //Load the Graph from that URI
                Graph g = new Graph();
                try
                {
                    UriLoader.Load(g, new Uri(uri));
                }
                catch (Exception)
                {
                    //Supress the exception
                    //TODO: Show an error message to the end user
                }

                //Write out as a HTML page
                HtmlWriter writer = new HtmlWriter();
                writer.Stylesheet = "../sparql.css";
                writer.UriPrefix = "?uri=";

                context.Response.ContentType = MimeTypesHelper.Html[0];
                writer.Save(g, context.Response.Output);
            }
        }
    }
}
