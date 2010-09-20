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
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Web.Configuration;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Permissions Model for UAC in dotNetRDF's ASP.Net integration
    /// </summary>
    /// <remarks>
    /// Highly experimental, unfinished and currently non-functional
    /// </remarks>
    public class PermissionsModule : IHttpModule
    {
        private HttpContext _current;
        private List<UserGroup> _userGroups = new List<UserGroup>();

        /// <summary>
        /// Disposes of the Module
        /// </summary>
        public void Dispose()
        {
            //Nothing to do
        }

        /// <summary>
        /// Initialises the HTTP Module
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            //this._current = context.Context;
            //context.PostMapRequestHandler += new EventHandler(context_PostMapRequestHandler);
        }

        void context_PostMapRequestHandler(object sender, EventArgs e)
        {
            //First need to get whether this request is pointing to a Handler

            //Check the Configuration File is specified
            String configFile = this._current.Server.MapPath(ConfigurationManager.AppSettings["dotNetRDFConfig"]);
            if (configFile == null) throw new DotNetRdfConfigurationException("Unable to load Protocol Handler Configuration as the Web.Config file does not specify a 'dotNetRDFConfig' AppSetting to specify the RDF configuration file to use");
            IGraph g = WebConfigurationLoader.LoadConfigurationGraph(this._current, configFile);

            //Then check there is configuration associated with the expected URI
            String objUri = "dotnetrdf:" + this._current.Request.Path;
            INode objNode = g.GetUriNode(new Uri(objUri));
            if (objNode == null)
            {
                //See if there is wildcard configuration associated with the expected URI
                String path;
                objNode = WebConfigurationLoader.FindObject(g, this._current.Request.Url, out path);
                if (objNode == null) return; //No object so no need to authenticate
            }

            //Find the User Groups that relate to this path
            IEnumerable<INode> groups = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:userGroup"));
            foreach (INode group in groups)
            {
                Object temp = ConfigurationLoader.LoadObject(g, group);
                if (temp is UserGroup)
                {
                    this._userGroups.Add((UserGroup)temp);
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load Handler Configuration as the RDF Configuration file specifies a value for the Handlers dnr:userGroup property which cannot be loaded as an object which is a UserGroup");
                }
            }

            //Check for Authentication
            if (this._userGroups.Count > 0)
            {
                HandlerHelper.IsAuthenticated(this._current, this._userGroups);
            }
        }
    }
}

#endif
