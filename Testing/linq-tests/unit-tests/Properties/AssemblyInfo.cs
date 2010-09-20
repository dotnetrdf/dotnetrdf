using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VDS.RDF.Linq;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RdfSerialisationTest")]
[assembly: AssemblyDescription("Unit tests for ontology LINQ query provider for access to RDF triple stores using SPARQL")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Andrew Matthews")]
[assembly: AssemblyProduct("RdfSerialisationTest")]
[assembly: AssemblyCopyright("Copyright ©  2008 Andrew Matthews")]
[assembly: AssemblyTrademark("LinqToRdf")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM componenets.  If you need to access ontology type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c4b907c7-6f6c-41c9-88d9-4e5c30c59f4e")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("0.8.0.0")]
[assembly: AssemblyFileVersion("0.8.0.0")]

[assembly: Ontology(
    BaseUri = "http://aabs.purl.org/ontologies/2007/11/tasks#",
    Name = "Tasks",
    Prefix = "tasks",
    UrlOfOntology = "file:///C:/etc/dev/semantic-web/linqtordf/src/unit-testing/standard-test-data/tasks.n3")]
[assembly: Ontology(
    BaseUri = "http://xmlns.com/foaf/0.1/",
    Name = "MyOntology",
    Prefix = "MyOntology",
    UrlOfOntology = "http://xmlns.com/foaf/0.1/")]
