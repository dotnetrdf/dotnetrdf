using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace rdfServer
{
    [RunInstaller(true)]
    public partial class RdfServerServiceInstaller : Installer
    {
        public RdfServerServiceInstaller(String name)
        {
            InitializeComponent();
            this.svcRdfServerService.ServiceName = name;
            this.svcRdfServerService.DisplayName = name;

        }
    }
}
