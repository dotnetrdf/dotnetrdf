using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VDS.Web.Configuration
{
    public interface IConfigurationLoader
    {
        void Load(XmlNode node, HttpServer server);
    }
}
