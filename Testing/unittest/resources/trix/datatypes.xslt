<!-- 

This transformation allows for XML Schema 2 built-in primative types
(and <integer> for convenience) to be used within TriX files. For example:

	<triple>
		<uri>http://example.com/foo</uri>
		<uri>http://example.com/bytesLength</uri>
		<decimal>277</decimal>
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
 
<!-- There is probably a much more efficient way of doing this... -->

<xsl:template match="trix:duration">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#duration">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:dateTime">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#dateTime">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:time">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#time">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:date">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#date">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:gYearMonth">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#gYearMonth">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:gYear">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#gYear">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:gMonth">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#gMonth">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:gMonthDay">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#gMonthDay">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:gDay">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#gDay">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:string">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#string">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:boolean">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#boolean">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:base64binary">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#base64binary">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:hexBinary">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#hexBinary">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:float">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#float">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:decimal">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#decimal">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:double">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#double">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:anyURI">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#anyURI">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:QName">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#QName">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:NOTATION">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#NOTATION">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

<xsl:template match="trix:integer">
  <typedLiteral datatype="http://www.w3.org/2001/XMLSchema#integer">
    <xsl:value-of select="normalize-space(text())"/>
  </typedLiteral>
</xsl:template>

</xsl:stylesheet>
