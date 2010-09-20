using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LinqToRdf;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("TestHarness")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Readify Pty Ltd")]
[assembly: AssemblyProduct("TestHarness")]
[assembly: AssemblyCopyright("Copyright © Readify Pty Ltd 2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("ab68556d-70b4-45a2-b7c9-9148beafd9fb")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
/*
[assembly: Ontology(
    BaseUri = "http://xmlns.com/foaf/0.1/",
    Name = "foaf",
    Prefix = "foaf",
    UrlOfOntology = "http://xmlns.com/foaf/0.1/")]
[assembly: Ontology(
    BaseUri = "http://purl.org/vocab/frbr/core#",
    Name = "frbr",
    Prefix = "frbr",
    UrlOfOntology = "http://purl.org/vocab/frbr/core#")]
[assembly: Ontology(
    BaseUri = "http://purl.org/ontology/mo/",
    Name = "music",
    Prefix = "music",
    UrlOfOntology = "http://purl.org/ontology/mo/")]

[assembly: Ontology(
    BaseUri = "http://www.w3.org/2003/01/geo/wgs84_pos#",
    Name = "space",
    Prefix = "space",
    UrlOfOntology = "http://www.w3.org/2003/01/geo/wgs84_pos#")]
[assembly: Ontology(
    BaseUri = "http://www.w3.org/2006/time#",
    Name = "time",
    Prefix = "time",
    UrlOfOntology = "http://www.w3.org/2006/time#")]
*/
[assembly: Ontology(
    BaseUri = "http://aabs.purl.org/ontologies/2007/04/music#",
    Name = "music",
    Prefix = "music",
    UrlOfOntology = "http://aabs.purl.org/ontologies/2007/04/music#")]

