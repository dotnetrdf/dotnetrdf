<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://schemas.microsoft.com/wix/2006/wi">

  <xsl:template match="node()|@*">
    <xsl:choose>
      <xsl:when test="name() = 'File'">
        <xsl:call-template name="File" />
      </xsl:when>
      <xsl:when test="name() = 'Component'">
        <xsl:call-template name="Component" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy>
          <xsl:apply-templates select="node()|@*" />
        </xsl:copy>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="Component">
    <xsl:element name="Component">
      <xsl:attribute name="Id">
        <xsl:value-of select="../@Id"/>_<xsl:value-of select="@Id" />
      </xsl:attribute>
      <xsl:attribute name="Directory">
        <xsl:value-of select="@Directory"/>
      </xsl:attribute>
      <xsl:attribute name="Guid">
        <xsl:value-of select="@Guid"/>
      </xsl:attribute>
      <xsl:apply-templates select="node()" />
    </xsl:element>
  </xsl:template>

  <xsl:template name="File">
    <xsl:element name="File">
      <xsl:attribute name="Id">
          <xsl:value-of select="../../@Id"/>_<xsl:value-of select="@Id"/>
      </xsl:attribute>
      <xsl:attribute name="KeyPath">
        <xsl:value-of select="@KeyPath"/>
      </xsl:attribute>
      <xsl:attribute name="Source">
        <xsl:value-of select="@Source"/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>