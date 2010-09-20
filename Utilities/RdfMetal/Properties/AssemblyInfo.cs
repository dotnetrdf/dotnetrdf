using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mono;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("RdfMetal")]
[assembly: AssemblyDescription("A code generator for LinqToRdf")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Andrew Matthews")]
[assembly: AssemblyProduct("RdfMetal")]
[assembly: AssemblyCopyright("Copyright © Andrew Matthews 2008")]
[assembly: AssemblyTrademark("LinqToRdf, RdfMetal")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("08fa4e28-7239-4ee3-9208-0bfc73ddcbe1")]

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
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

[assembly: UsageComplement("is used with the following options")]
// This is text that goes after " [options]" in help output.

// Attributes visible in " -V"
[assembly: Mono.About("RdfMetal allows you to generate LinqTordf compatible domain models  by querying a remote RDF triple store using SPARQL. It is a command line tool that may be either added to your build process or used as a RAD tool to kickstart development.")]
[assembly: Mono.Author("Andrew Matthews")]
