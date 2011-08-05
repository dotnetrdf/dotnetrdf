dotNetRDF.2010.sln Readme
=========================

The dotNetRDF.2010.sln solution file is a a Visual Studio 2010 solution that can be used to compile
and test the parts of dotNetRDF that are not buildable under prior versions.

Currently this includes the following:
- Silverlight 4 build of the core library
- Windows Phone 7 build of the core library

Note that we do not actively maintain project files directly for the above so when you open this solution
you may find that the relevant projects are not loaded.  To fix this do the following:

1 - Ensure you have NAnt installed on your PC
2 - Open a command line window (Hit Win+R, type cmd and hit enter) and navigate to Trunk/Build/nant/ in your
    local working copy of the source code
3 - Type nant projectgen-windowsphone
4 - NAnt will now run and generate the project files for the Windows Phone and Silverlight builds of the library

Right click on the projects in Solution Explorer and select Reload Project, they should now be available.