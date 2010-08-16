dotNetRDF Virtuoso Sample
=========================

A very simple demo application of a Graph/Description browser for RDF stored in a Virtuoso Quad Store.

The connection to Virtuoso is configured in the application config file which is dotNetRDFVirtuosoSample.exe.config
In the <applicationSettings> section change the settings so that they point to the server you have Virtuoso
installed on and a valid user account.
Once configured you can run the dotNetRDFVirtuosoSample.exe file from the bin/Debug directory

Robert Vesse 2009
rvesse@vdesign-studios.com


Acknowledgements
----------------

Uses code (3rd Party Libraries) from the following sources
-MySQL Connector.Net from MySQL AB http://www.mysql.org
-JSON.Net from James Newton-King http://james.newtonking.com
-Virtuoso ADO.Net Provider from OpenLink Software http://www.openlinksw.com

Thanks to the following people who have helped in the development process or whose suggestions have led to 
improvements in the code:
-Eamon Nerbonne for the BlockingStreamReader fix (http://eamon.nerbonne.org/) which is much nicer than the
 alternative of pre-caching in a MemoryStream
-Hugh Williams and Jacqui Hand of OpenLink Software (http://www.openlinksw.com) for helping me resolve some
 issues with their ADO.Net provider including promptly providing me with a fixed version once the issue
 I'd identified had been traced to it's cause and extending my evaluation license so I could build the code
-Toby Inkster (http://tobyinkster.co.uk/) for providing me with some TriX extensibility stylesheets that I 
 could use to test my TriX parser