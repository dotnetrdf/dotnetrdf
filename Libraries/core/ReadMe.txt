dotNetRDF
=========

A Library for RDF manipulation and parsing in .Net using C# 3.0

This package contains the Core Library for the following four .Net profiles:
- .Net 3.5
- .Net 3.5 Client Profile
- Silverlight 4
- Silverlight 4 for Windows Phone 7

It also contains the following Data providers for .Net 3.5 only:
- Sql - provides our ADO Store backend
- Virtuoso - provides support for OpenLink Virtuoso

And it contains the following Query plugins for .Net 3.5 only:
- FullText - provides full text SPARQL query support

Please see the Release Notes for usage details and Known Issues

Robert Vesse 2009-11
rvesse@vdesign-studios.com

License
-------

dotNetRDF is licensed under the GNU GPL Version 3

Alternatively you may use it under the GNU Lesser GPL Version 3 or under the MIT license

If none of these licenses are suitable for your intended use please contact us to discuss
alternative terms

Security
--------

This Project creates a Strong Name signed assembly for the Core Library only.

The Key file for this requires a Password which is as follows: VDSOpenSource2009

For security conscious deployment scenarios we suggest that you build the project from source and use your own key file

Acknowledgements
----------------

Acknowledgements can be found in Acknowledgements.txt