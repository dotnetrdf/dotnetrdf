cd ../../Libraries/core/
gmcs -out:bin/Debug/dotNetRDF.Mono.dll -t:library -v -recurse:*.cs -lib:bin/Debug -r:System.Data,System.Web,System.Configuration,Newtonsoft.Json.Net35,MySql.Data,HtmlAgilityPack -nowarn:0659,0628,0169,0414,0219,0162 -define:MONO,DEBUG > mono-compile-log.txt 2>&1
