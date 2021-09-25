<!-- 

This transformation allows for CURIEs to be used within TriX files.
For example:

	<triple xmlns:foaf="http://xmlns.com/foaf/0.1/">
		<id>toby</id>
		<curie>foaf:name</curie>
		<plainLiteral>Toby Inkster</plainLiteral>
	</triple>

-->


<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:trix="http://www.w3.org/2004/03/trix/trix-1/"
    xmlns="http://www.w3.org/2004/03/trix/trix-1/"
>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="trix:curie">

		<xsl:variable name="short" select="substring-before(text(),':')" />
	  
		<xsl:choose>
			<xsl:when test="namespace::node()[name(.)=$short]">
				<uri> 
					<xsl:value-of select="namespace::node()[name(.)=$short]"/> 
					<xsl:value-of select="substring-after(text(),':')"/> 
				</uri> 
			</xsl:when>
			<xsl:otherwise>
				<error />
			</xsl:otherwise>
		</xsl:choose>
	  
	</xsl:template> 

</xsl:stylesheet>
