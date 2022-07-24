# Deploying with rdfWebDeploy 

Integration of dotNetRDF into ASP.Net applications is configured using the [Configuration API](configuration/index.md) and can be deployed using [rdfWebDeploy](/tools/rdfwebdeploy.md). This guide teaches you how to deploy configuration using rdfWebDeploy.

# Problem 

In this example we want to create a SPARQL Query endpoint which has a base Graph as it's initial dataset but which users can introduce additional graphs into using `FROM/FROM NAMED` clauses and using Default/Named Graph URIs in the requests. We want this endpoint to be available at our website under the URI `/sparql`

## Step 1 - Define Configuration 

To start with we need to create a configuration file which defines this information - see the [Configuration API](configuration/index.md) documentation for guidance on this, this will look like the following:

```turtle
@prefix dnr: <http://www.dotnetrdf.org/configuration#> .

# Firstly note that our Handler must have a subject which is a special dotNetRDF URI as discussed in Configuration API - HTTP Handlers
<dotnetrdf:/sparql> a dnr:HttpHandler ;
  dnr:type "VDS.RDF.Web.QueryHandler" ; # States that we're using the QueryHandler
  dnr:queryProcessor _:proc .

# Now we must define our query processor, we want to use the in-memory Leviathan SPARQL Engine
_:proc a dnr:SparqlQueryProcessor ;
  dnr:type "VDS.RDF.Query.LeviathanQueryProcessor" ;
  dnr:usingStore _:store .

# Now we define the initial dataset
_:store a dnr:TripleStore ;
  dnr:type "VDS.RDF.WebDemandTripleStore" ; # The WebDemandTripleStore can dynamically load Graphs from the web as required
  dnr:usingGraph _:initGraph .

# This defines out initial Graph which comes from a file in our App_Data directory
_:initGraph a dnr:Graph ;
  dnr:type "VDS.RDF.Graph" ;
  dnr:fromFile "~/App_Data/example.rdf" .
```

Here we've defined out configuration file in Turtle but any RDF serialization which dotNetRDF understands can be used, we'll refer to this file as `config.ttl` for the rest of the document but you can name this file whatever you want. Please ensure you use an appropriate file extension for the RDF serialization used to ensure correct behaviour.

We recommend that you create your configuration file in your applications `App_Data/` directory

## Step 2 - Test Configuration 

Next step is to test your Configuration for errors and mistakes by issuing the following command at the command line (assumes you have the Tools package installed and on your path):

```dos
X:\example.com\www>rdfWebDeploy -test App_Data\config.ttl
```

This will test your configuration file and produce output like the following:

```dos
rdfWebDeploy: Opened the configuration file successfully
rdfWebDeploy: Loaded the configuration vocabulary successfully

rdfWebDeploy: Tests Started...

rdfWebDeploy: Testing for URIs in the vocabulary namespace which are unknown

rdfWebDeploy: Testing for missing dnr:type properties

rdfWebDeploy: Testing that values given for dnr:type property are literals

rdfWebDeploy: Testing that no object has multiple dnr:type values

rdfWebDeploy: Testing that values given for dnr:type property in the VDS.RDF namespace are valid

rdfWebDeploy: Testing for bad URIs given the rdf:type of dnr:HttpHandler

rdfWebDeploy: Testing for missing dnr:type for dnr:HttpHandler objects

rdfWebDeploy: Testing that values given for dnr:type for dnr:HttpHandler objects in the VDS.RDF namespace are valid IHttpHandlers

rdfWebDeploy: Testing for bad ranges for properties

rdfWebDeploy: Testing for bad domains for properties

rdfWedDeploy: Tests Completed - 0 Warning(s) - 0 Error(s)
```

If you see any error messages at this stage you should correct this errors now.

## Step 3 - Deploying to Web.config 

You now need to deploy this configuration to your `Web.config` file, don't worry if your application doesn't have one as this step will create one if necessary. The command to be issued at the command line now varies depending on whether the application you are deploying to is on a local IIS server instance or whether it is a local copy of an application you plan to upload to your web hosting.

Both of these methods will create/update your `Web.config` file so that all the relevant Handlers are registered at the appropriate paths and the `<appSettings>` setting which points to your configuration file is set. They will also copy all required DLLs into the `bin/` directory of your application (creating it if necessary).

With either of these methods you can add the options `-nointreg` to not register handlers in the integrated mode section of your `Web.config` file or `-noclassicreg` to not register handlers in the classic mode section of your `Web.config` file.

> [!WARNING]
> This tool was originally designed for .Net 3.5 but dotNetRDF now also supports .Net 4.0, as the `Web.config` format has some differences between ASP.Net 3.5 and ASP.Net 4.0 we strongly recommend you ensure that you have a pre-existing `Web.config` before running this step as we cannot guarantee that a created `Web.config` will be compatible with your environment whereas updating an existing `Web.config` file should work fine.

### Local IIS Method 

If your application is configured on a local IIS server instance then you can issue the following command:

```dos
X:\example.com\www>rdfWebDeploy -deploy /appVirtualPath config.ttl
```

With this method you specify the virtual path to your application and then the name of the configuration file. If this configuration file is located in the current directory it is copied to the `App_Data/` directory, otherwise it is checked for in the `App_Data/` directory.

If your application is not on the default site of your IIS instance then you would use the `-site` option like so:

```dos
X:\example.com\www>rdfWebDeploy -deploy /appVirtualPath config.ttl -site "www.example.com"
```

### No IIS Method 

If you intend to upload your application to web hosting and it is not configured on a local IIS server instance then you should issue the following command:

```
X:\example.com\www>rdfWebDeploy -xmldeploy "X:\example.com\www\Web.Config" config.ttl
```

With this method you specify the path to the `Web.Config` file (or the root folder) of your application and the name of the configuration file. If this configuration file is located in the current directory it is copied to the `App_Data/` directory, otherwise it is checked for in the `App_Data/` directory.

### Additional Features 

For either of the above methods you can add the `-negotiate` option to install the Negotiate by File Extension module. This module allows all our HTTP Handlers to transparently negotiate based on the file extension in the URL. So for our example here if the `-negotiate` option is used you'd be able to browse to `/sparql.srj?query=SELECT * WHERE { ?s ?p ?o}` to force the results to be returned as SPARQL Results JSON.

## Step 4 - Upload 

If your application is being used on remote web hosting now upload your application to your web hosting provider. Your provider must support ASP.Net 3.5 for this to function correctly, IIS 7+ based hosting is recommended for best support.

## Step 5 - Testing 

Navigate to the URL `/sparql` at your site and you should now have a working SPARQL Endpoint.

# Common Errors 

There are two common error conditions you may run into:

You get a [DotNetRdfConfigurationException](xref:VDS.RDF.Configuration.DotNetRdfConfigurationException) - this generally indicates that your Configuration file is invalid (note that `rdfWebDeploy -test` can only ensure general syntactic correctness and not validity). If you have debugging turned on you should normally see an informative message detailed what is wrong with your configuration. Correct the errors and retry - you may need to restart the IIS site to force the updated file to be read in some cases.

You get an `Unable to load type VDS.RDF.Web.QueryHandler` or similar error. This is usually due to a failure to upload the required DLLs into the `bin/` directory of your site. If you have done this ensure that you are running as an IIS Application rather than a virtual directory. If you are running as a virtual directory (and cannot change to running as an IIS application) then move the DLLs from the `bin/` directory in your virtual folder to the `bin/` directory of the application root

Depending on your version of ASP.Net you may also need to make some manual tweaks to the `Web.config` - see [Creating SPARQL Endpoints](asp_creating_sparql_endpoints.md) for more details.

# Further Reading 

For more information on setting up SPARQL endpoints you may wish to look at the following topics:

* [Configuration API](configuration/index.md)
* [Configuration API - HTTP Handlers](configuration/http_handlers.md)
* [Configuration Vocabulary](http://www.dotnetrdf.org/configuration#)
* [rdfWebDeploy](/tools/rdfwebdeploy.md)