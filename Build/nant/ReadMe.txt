dotNetRDF
=========

A Library for RDF manipulation and parsing in .Net using C# 3.0

Robert Vesse 2009-10
rvesse@vdesign-studios.com

NAnt Usage
----------

Run build-nightly.bat to compile and update the bin/nightly directory


Build Files
===========

dotnetrdf.build
-----------------------

Contains the following Targets:

build-nightly - Copies the build from the Libraries/core/bin/Debug directory to the root bin/nightly directory

clean-nightly - Cleans the root bin/nightly directory

build-stable - Copies the build from the Libraries/core/bin/Debug directory to the root bin/stable directory

clean-stable - Cleans the root bin/stable directory

compile - Compiles Libaries/core/dotNetRDF.csproj

compile-all - Compiles the entire dotNetRDF solution