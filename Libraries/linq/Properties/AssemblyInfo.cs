using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Resources;
using VDS.RDF.Linq;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dotNetRDF.Linq")]
[assembly: AssemblyDescription("A LINQ query provider for access to RDF triple stores using SPARQL (Port of LinqToRdf to dotNetRDF)")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Visual Design Studios")]
[assembly: AssemblyProduct("dotNetRDF.Linq")]
[assembly: AssemblyCopyright("Copyright ©  2008 Andrew Matthews and Copyright ©  2010 Robert Vesse")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access ontology type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9FDA129F-5CDD-4f39-B442-D01BE8C9816C")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("0.1.0.*")]
[assembly: AssemblyFileVersion("0.1.0.0")]

//[assembly: IsolatedStorageFilePermission(SecurityAction.RequestMinimum, UserQuota = 1048576)]
//[assembly: SecurityPermission(SecurityAction.RequestRefuse, UnmanagedCode = true)]
//[assembly: FileIOPermission(SecurityAction.RequestOptional, Unrestricted = true)]
[assembly: Ontology(
    Prefix = "rdf",
    BaseUri = "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
    Name = "RDF")]
[assembly: Ontology(
    Prefix = "rdfs",
    BaseUri = "http://www.w3.org/2000/01/rdf-schema#",
    Name = "RDFS")]
[assembly: Ontology(
    Prefix = "xsd",
    BaseUri = "http://www.w3.org/2001/XMLSchema#",
    Name = "Data Types")]
[assembly: Ontology(
    Prefix = "fn",
    BaseUri = "http://www.w3.org/2005/xpath-functions#",
    Name = "XPath Functions")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
