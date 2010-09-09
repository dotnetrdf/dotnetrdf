
#if !NO_WEB && !NO_ASP

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Web.Configuration;

namespace VDS.RDF.Web
{
    public class PermissionsModule : IHttpModule
    {
        private HttpContext _current;
        private List<UserGroup> _userGroups = new List<UserGroup>();

        public void Dispose()
        {
            //Nothing to do
        }

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
