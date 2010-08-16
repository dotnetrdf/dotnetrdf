/*

Copyright Robert Vesse 2009-10
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

#if !NO_WEB && !NO_ASP

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using VDS.RDF.Web.Configuration.Resource;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// A Disk Resource Handler translates a Uri it is invoked with into a filesystem path and attempts to return the file at that location
    /// </summary>
    /// <remarks>
    /// <para>
    /// This Handler supports only one handler of this type being registered (multiple Handlers can be registered but their configuration will be shared if they get it from the same Web.config file).  The Handler is designed to be configured with a wildcard path so any path using the Base Uri of the Handler will be translated into a filesystem path and the requested resource (if it exists) returned.
    /// </para>
    /// <para>
    /// Each Handler registered in Web.config may have a prefix for their Configuration variables set by adding a AppSetting key using the type of the handler like so:
    /// <code>&lt;add key="VDS.RDF.Web.DiskResourceHandler" value="ABC" /&gt;</code>
    /// Then when the Handler at that path is invoked it will look for Configuration variables prefixed with that name.
    /// </para>
    /// <para>
    /// This Handler supports the following Configuration Variables
    /// </para>
    /// <ul>
    /// <li><strong>BaseUri</strong> (<em>Required</em>) - Sets the Base Uri at which your Handler is installed, used to obtain the portion of the Uri that will be translated into a filesystem path for retrieving the resource</li>
    /// <li><strong>DataFolder</strong> (<em>Required</em>) - Sets the root Data Folder into which URIs will be mapped as filesystem paths</li>
    /// <li><strong>DataExtension</strong> (<em>Optional</em>) - Sets the extension of the data files contained in the data folder.  Defaults to <strong>.rdf</strong></li>
    /// <li><strong>DataMIMEType</strong> (<em>Optional</em>) - Sets the MIME Type of your data files, if you set a <strong>DataExtension</strong> the MIME Type will be determined from the extension unless you set it explicitly.  Defaults to <strong>application/rdf+xml</strong></li>
    /// <li><strong>DefaultFile</strong> (<em>Optional</em>) - Sets the Default File that will be retrieved if a client accesses the handler using the Base Uri.  Defaults to no default file.</li>
    /// <li><strong>AllowFormatTranslation</strong> (<em>Optional</em>) - Sets whether the Handler can automatically translate your data files into formats accepted by the Client if the client does not accept the MIME Type of your data.  Defaults to <strong>False</strong></li>
    /// </ul>
    /// <para>
    /// The following is an example of how this configuration can be used.  First we have the Web.config settings added to the &lt;appSettings&gt; section:
    /// </para>
    /// <code>
    /// &lt;appSettings&gt;
    ///     &lt;add key="VDS.RDF.Web.DiskResourceHandler" value="Disk"/&gt;
    ///     &lt;add key="DiskBaseURI" value="http://example.org/resource/"/&gt;
    ///     &lt;add key="DiskDataFolder" value="~/App_Data"/&gt;
    ///     &lt;add key="DiskDataExtension" value=".ttl"/&gt;
    ///     &lt;add key="DiskDefaultFile" value="default"/&gt;
    ///     &lt;add key="DiskAllowFormatTranslation" value="true"/&gt;
    /// &lt;/appSettings&gt;
    /// </code>
    /// <para>
    /// Then in the &lt;handlers&gt; section of &lt;system.webServer&gt; we have to register our handler (this assumes you're using IIS 7):
    /// </para>
    /// <code>
    /// &lt;handlers&gt;
    ///     &lt;remove name="ResourceHandler" /&gt;
    ///     &lt;add name="ResourceHandler" verb="*" path="resource/*" type="VDS.RDF.Web.DiskResourceHandler" /&gt;
    /// &lt;/handlers&gt;
    /// </code>
    /// <para>
    /// <strong>Note for IIS 6 Users:</strong> Simply add the &lt;add&gt; tag to your &lt;httpHandlers&gt; section of &lt;system.web&gt; - just remember to remove the <em>name</em> attribute
    /// </para>
    /// <para>
    /// Now you've done this you'll need to place some data files under the <strong>App_Data</strong> folder of your application with the Turtle extension <em>.ttl</em>
    /// <br />
    /// Once this is done you can start accessing resources at the Uri, the following table gives you an idea of the mapping from URIs to Files (the example assumes the application is installed in the filesystem at <strong>D:\example.org\www\</strong>
    /// </para>
    /// <table class="dtTable" cellspacing="0">
    /// <tr>
    ///     <th>Uri</th>
    ///     <th>File</th>
    /// </tr>
    /// <tr>
    ///     <td>http://example.org/resource/</td>
    ///     <td>D:\example.org\www\App_Data\default.ttl</td>
    /// </tr>
    /// <tr>
    ///     <td>http://example.org/resource/something</td>
    ///     <td>D:\example.org\www\App_Data\something.ttl</td>
    /// </tr>
    /// <tr>
    ///     <td>http://example.org/resource/thing</td>
    ///     <td>D:\example.org\www\App_Data\thing.ttl</td>
    /// </tr>
    /// <tr>
    ///     <td>http://example.org/resource/subdirectory/thing</td>
    ///     <td>D:\example.org\www\App_Data\subdirectory\thing.ttl</td>
    /// </tr>
    /// <tr>
    ///     <td>http://example.org/resource/a/b/c/thing</td>
    ///     <td>D:\example.org\www\App_Data\a\b\c\thing.ttl</td>
    /// </tr>
    /// </table>
    /// <para>
    /// If the Uri maps to a file that doesn't exist then the Handler will return a HTTP 404 error to the client.
    /// </para>
    /// </remarks>
    [Obsolete("This class is considered obsolete - it is recommended that you use a ProtocolHandler instead to achieve similar functionality", true)]
    public class DiskResourceHandler : BaseResourceHandler
    {
        private String _cacheKey = String.Empty;
        private DiskResourceHandlerConfiguration _config;

        /// <summary>
        /// Processes a request for a resource by mapping the Uri to a filesystem path and returning the RDF document there
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        public override void ProcessRequest(HttpContext context)
        {
            //Turn on Response Buffering
            context.Response.Buffer = true;

            //Add our Custom Headers
            try
            {
                context.Response.Headers.Add("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            this.LoadConfig(context);

            //Get the Uri and convert to a Path
            String uri = this.RewriteURI(context.Request.Url.ToString());
            if (uri.StartsWith(this._config.BaseURI))
            {
                if (this._config.BaseURI.Length < uri.Length)
                {
                    uri = uri.Substring(this._config.BaseURI.Length);
                }
                else
                {
                    uri = this._config.DefaultFile;
                }
                
                //Swap forwards slashes to backslashes and create the filesystem path
                uri = uri.Replace('/','\\');
                String filePath = this._config.DataFolder + uri + this._config.DataExtension;

                if (File.Exists(filePath))
                {
                    context.Response.Clear();

                    //Does the Client accept the format our Data is in?
                    bool clientAcceptsFormat = false;
                    if (context.Request.AcceptTypes.Length > 0)
                    {
                        clientAcceptsFormat = context.Request.AcceptTypes.Any(ctype => ctype.Contains(this._config.DataMIMEType));
                    }

                    if (this._config.AllowFormatTranslation && !clientAcceptsFormat)
                    {
                        //Translate the format into one more appropriate for the Client
                        String ctype;
                        IRdfWriter writer;
                        if (context.Request.AcceptTypes != null)
                        {
                            writer = MimeTypesHelper.GetWriter(context.Request.AcceptTypes, out ctype);
                        }
                        else
                        {
                            //Default To RDF/XML if no accept header
                            writer = new FastRdfXmlWriter();
                            ctype = MimeTypesHelper.RdfXml[0];
                        }
                        String destExt = MimeTypesHelper.GetFileExtension(writer);

                        //Clear Response and Set Content Type
                        context.Response.Clear();
                        context.Response.ContentType = ctype;

                        //Translate to the desired format
                        IRdfReader parser = MimeTypesHelper.GetParser(this._config.DataMIMEType);
                        Graph temp = new Graph();
                        parser.Load(temp, filePath);
                        writer.Save(temp, context.Response.Output);
                    }
                    else
                    {
                        //Send in the Format we have since we're not allowed to do Format Translation
                        context.Response.ContentType = this._config.DataMIMEType;
                        context.Response.WriteFile(filePath);
                    }
                }
                else
                {
                    context.Response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
            }
            else
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
        }

        /// <summary>
        /// Loads the Configuration for the Handler
        /// </summary>
        /// <param name="context">Context of the HTTP Web Request</param>
        /// <param name="cacheKey">Cache Key</param>
        /// <param name="prefix">Config Variable Prefix</param>
        protected override void LoadConfigInternal(HttpContext context, string cacheKey, string prefix)
        {
            if (context.Cache[cacheKey] == null)
            {
                //No Config Cached so create a new Config object which will load the Config
                this._config = new DiskResourceHandlerConfiguration(context, cacheKey, prefix);
                context.Cache.Add(cacheKey, this._config, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 15, 0), System.Web.Caching.CacheItemPriority.Normal, null);
            }
            else
            {
                //Retrieve from Cache
                this._config = (DiskResourceHandlerConfiguration)context.Cache[cacheKey];
            }
        }
    }
}

#endif