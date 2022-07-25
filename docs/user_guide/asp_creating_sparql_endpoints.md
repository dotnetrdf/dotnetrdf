# Creating SPARQL Endpoints 

dotNetRDF provides a set of HTTP Handlers that can be used to deploy various forms of RDF and SPARQL endpoints as part of your ASP.Net website.

## Automated Configuration 

[Configuration API - HTTP Handlers](configuration/http_handlers.md) gives a guide to the available handlers and [Deploying with rdfWebDeploy](asp_deploying_with_rdfwebdeploy.md) walks you through the automated process of deploying them.

## Manual Configuration 

Continue reading for information on how to manually set up your `Web.config` file without using rdfWebDeploy

### Configuration Settings

All Handlers are configured by using `Web.config` to register them in your web application, the library supports registering multiple handlers in a single application each of which can have different settings. Each HTTP Handler registered in your `Web.config` file must have corresponding configuration information stored in your Configuration Graph.

Your Configuration Graph is registered using the `<appSettings>` section of your `Web.config` file like so:

```xml

<appSettings>
  <add key="dotNetRDFConfig" value="~/App_Data/config.ttl" />
</appSettings>
```

Each Handler is associated with some Configuration data in your Configuration Graph by using special URIs of the form `<dotnetrdf:/path>` as described in [Configuration API - HTTP Handlers](configuration/http_handlers.md)

### Handler Registration 

#### IIS 5x 

Under IIS 5x you cannot map arbitrary paths so you'll have to define a custom extension (we recommend you define `.sparql`) which is done as follows:

1. Open IIS Manager
1. Browse to your website/virtual directory and right click, select Properties
1. On to the Virtual Directory tab click the Configuration button
1. Click the Add button in the Application Configuration dialog that appears
1. Click the Browse button and browse to and select aspnet_isapi.dll which is typically located in `C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\`
1. In the extension box type `.sparql` (you must include the dot at the start!), uncheck the Check that file exists box
1. Click OK to add the mapping (if the OK button is not enabled try clicking in the Executable box to get Windows to expand the path and then try to click OK)

You've now configured IIS to route any request for a file with the `.sparql` extension through ASP.Net which means any HTTP Handlers you define will be applied correctly. You can now configure a ASP.Net Handler by adding a line to the `<httpHandlers>` section of the `<system.web>` section of your `Web.config` file:

```xml

<add verb="*" path="/query.sparql" validate="false" type="VDS.RDF.Web.QueryHandler" />
```

Then provided you have entered all the relevant configuration settings for your Handler in your Configuration Graph you can access the endpoint by pointing your browser to `http://www.yourdomain.com/query.sparql`

#### IIS 6x 

On IIS 6x you configure a ASP.Net Handler by adding a line to the `<httpHandlers>` section of the `<system.web>` section of your `Web.config` file:

```xml
<add verb="*" path="/sparql" validate="false" type="VDS.RDF.Web.QueryHandler" />
```

Then provided you have entered all the relevant configuration settings for your Handler in your Configuration Graph you can access the endpoint by pointing your browser to `http://www.yourdomain.com/sparql`

Under IIS 6x you will need to get IIS to serve the request even when the physical file doesn't exist. To do this you can either create a dummy file/folder with the appropriate name or use the IIS 5x configuration approach (see [Microsoft TechNet article](http://www.microsoft.com/technet/prodtechnol/WindowsServer2003/Library/IIS/4c840252-fab7-427e-a197-7facb6649106.mspx?mfr=true) for IIS 6x)

>[!NOTE]
> In theory it should be possible to configure under IIS 6 without the previous step but I don't currently have access to an appropriate machine to verify this.

#### IIS 7x

Configuring a SPARQL Endpoint under IIS 7x is very easy if you are using ASP.Net's integrated pipeline mode (if not follow the instructions for IIS 6x). Simply add a line like the following to the `<handlers>` section of the `<system.webServer>` section of your `Web.config` file:

```xml

<add name="/sparql" verb="*" path="/sparql" type="VDS.RDF.Web.QueryHandler" />
```

Then provided you have entered all the relevant configuration settings for your Handler in your Configuration Graph you can access the endpoint by pointing your browser to `http://www.yourdomain.com/sparql`

### Other Web.config tweaks 

You may need to make some additional changes to `Web.config` in order to get handlers working correctly.

If you are creating SPARQL endpoints you may need to add the following to the `<system.web>` section of your config if you are using .Net 4.0+ as otherwise the ASP.Net engine may reject queries and updates as potentially dangerous requests:

```xml
<httpRuntime requestValidationMode="2.0" />
```

You may also need to add the following to your `<system.webServer>` configuration to get your `Web.config` to be loaded:

```xml
<validation validateIntegratedModeConfiguration="false" />
```

# General IIS Configuration 

As a general rule it is advisable to register the following MIME types with IIS server to ensure that it serves responses correctly:

* text/turtle (*.ttl)
* application/rdf+xml (*.rdf)
* text/n3 (*.n3)
* text/plain (*.nt)
* application/rdf+json (*.rj)
* application/sparql-results+xml (*.srx)
* application/sparql-json+xml (*.srj)