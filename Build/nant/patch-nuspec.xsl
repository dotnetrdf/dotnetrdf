<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl nuget"
                xmlns:nuget="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd" >

    <xsl:param name="version" />
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>

    <xsl:template match="nuget:dependency[@id='dotNetRDF']" xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
        <dependency id="dotNetRDF" version="{version}" />
    </xsl:template>
</xsl:stylesheet>
