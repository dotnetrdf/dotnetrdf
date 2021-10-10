#!/bin/sh
cd `dirname "$0"`
cat >index.html <<EOS
<!DOCTYPE html>
<html lang="en">
<title>Minutes of the RDF* calls</title>

<h1>Minutes of the RDF* calls</h1>
<ul>
EOS

ls 2*.html | sort -r | while read filename
do
    date=`basename "$filename" .html`
    echo "<li><a href=\"$filename\">$date</a>" >>index.html
done
echo "</ul>" >>index.html
