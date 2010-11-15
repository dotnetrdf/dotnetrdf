using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace VDS.Web
{
    [RunInstaller(true)]
    public partial class HttpServerInstaller : Installer
    {
        public HttpServerInstaller()
        {
            InitializeComponent();
        }
    }
}
