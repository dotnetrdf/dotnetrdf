dotNetRDF
=========

A Library for RDF manipulation and parsing in .Net using C# 3.0

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

This Project creates a Strong Name signed assembly.  The Key file for this requires a Password which is as follows:
VDSOpenSource2009

For security conscious deployment scenarios we suggest that you build the project from source and use your own key file

Acknowledgements
----------------

Uses code (3rd Party Libraries) from the following sources:
-MySQL Connector.Net from MySQL AB http://www.mysql.org
-JSON.Net from James Newton-King http://james.newtonking.com
-Virtuoso ADO.Net Provider from OpenLink Software http://www.openlinksw.com
-HtmlAgilityPack from Simon Mourier http://www.codeplex.com/htmlagilitypack

Uses code (embedded in the library) from the following sources:
-HashLib http://hashlib.codeplex.com/

Thanks to the following people who have helped in the development process or whose suggestions have led to 
improvements in the code:
- Eamon Nerbonne for the BlockingStreamReader fix (http://eamon.nerbonne.org/) which is much nicer than the
  alternative of pre-caching in a MemoryStream
- Hugh Williams and Jacqui Hand of OpenLink Software (http://www.openlinksw.com) for helping me resolve some
  issues with their ADO.Net provider including promptly providing me with a fixed version once the issue
  I'd identified had been traced to it's cause and extending my evaluation license so I could build the code
- Toby Inkster (http://tobyinkster.co.uk/) for providing me with some TriX extensibility stylesheets that I 
  could use to test my TriX parser
- Marek Safar from the Mono project for fixing the bug in gmcs I identified which meant I couldn't compile
  a Mono build of dotNetRDF
- Andy Seaborne and Steve Harris for excellent answers to various SPARQL and ARQ function library related questions 
  which have contributed to resolving various issues in the Leviathan engine and adding the ARQ function library support
- Peter Kahle for his efforts in producing a Windows Phone 7 version of the library
- Paul Hermans for various suggestions related to the improvement of the Toolkit
- The SPARQL Working Group for useful feedback and responses to my comments
- Graham Moore for excellent input on needed changes in the API to better support 3rd party developers
  plugging their stuff into the API
- Laurent Lefort for feedback and suggestions regarding rdfEditor
- The following people who have contributed bug reports, patches, ideas etc on the mailing lists:
 - Tana Isaac
 - Koos Strydoom
 - Alexander Sidorov
 - Michael Friis
 - Bob DuCharme
 - Alexander Zapirov
 - Sergey Novikov
 - Jeen Broekstra
 - Robert P DeCarlo
 - Clive Emberey
 - Anton Andreev
 - Steve Fraleigh
 - Felipe Santos
 - Bob Morris