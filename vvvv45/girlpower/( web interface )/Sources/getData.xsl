<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes"/>
	<xsl:template name="Split">
		<!--This template will recursively break apart a comma-delimited string into child elements-->
		<xsl:param name="strInput" select="&apos;&apos;"/>
		<xsl:param name="strDelimiter" select="&apos;,&apos;"/>
		<xsl:variable name="strNextItem" select="substring-before($strInput, $strDelimiter)"/>
		<xsl:variable name="strOutput" select="substring-after($strInput, $strDelimiter)"/>
		<xsl:variable name="strLen" select="string-length($strNextItem)"/>
		<xsl:choose>
			<xsl:when test="contains($strInput,$strDelimiter)">
				<SLICE>
					<xsl:value-of select="$strNextItem"/>
				</SLICE>
				<!-- At this point, the template will recursively call itself until the last comma is found -->
				<xsl:call-template name="Split">
					<xsl:with-param name="strInput" select="$strOutput"/>
					<xsl:with-param name="strDelimiter" select="$strDelimiter"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<!-- The otherwise clause will be reached when a comma is not located using contains() -->
				<SLICE>
					<xsl:value-of select="$strInput" />
				</SLICE>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="/PATCH">
		<PATCH>
			<xsl:attribute name="name"><xsl:value-of select="@nodename"/></xsl:attribute>
			<xsl:for-each select="NODE[starts-with (@nodename,'IOBox') and PIN[@pinname='Descriptive Name']/@values!='||' ]/PIN[@pinname='Descriptive Name']/..">
				<xsl:variable name="id" select="@id"/>
				<xsl:variable name="Datatype">
					<xsl:if test="substring-after(@nodename,'IOBox ')='(String)' ">String</xsl:if>
					<xsl:if test="substring-after(@nodename,'IOBox ')='(Value Advanced)' ">Value</xsl:if>
					<xsl:if test="substring-after(@nodename,'IOBox ')='(Color)' ">Color</xsl:if>
					<xsl:if test="substring-after(@nodename,'IOBox ')='(Enumerations)' ">Enum</xsl:if>
				</xsl:variable>
				<xsl:if test="../LINK[@dstnodeid=$id]=false()">
					<BOX>
						<xsl:attribute name="name"><xsl:value-of select="PIN[@pinname='Descriptive Name']/@values"/></xsl:attribute>
						<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
						<xsl:attribute name="datatype"><xsl:value-of select="$Datatype"/></xsl:attribute>
						<xsl:attribute name="pinname">
							<xsl:if test="$Datatype = 'String' ">Input String</xsl:if>
							<xsl:if test="$Datatype = 'Value' ">Y Input Value</xsl:if>
							<xsl:if test="$Datatype = 'Color' ">Color Input</xsl:if>
							<xsl:if test="$Datatype = 'Enum' ">Input Enum</xsl:if>
						</xsl:attribute>
						
						<GROUP>
							<xsl:choose>
								<xsl:when test="contains(PIN[@pinname='Descriptive Name']/@values, ':')">
									<xsl:value-of select="translate(substring-before(PIN[@pinname='Descriptive Name']/@values, ':'),'|','') "/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="PIN[@pinname='Descriptive Name']/@values"/>
								</xsl:otherwise>
							</xsl:choose>
						</GROUP>
						
						<SLICECOUNT>
							<xsl:choose>
								<xsl:when test="PIN[contains(@pinname,'Input')]/@slicecount">
									<xsl:value-of select="PIN[contains(@pinname,'Input')]/@slicecount"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="PIN[contains(@pinname,'Input')]">0</xsl:when>
										<xsl:otherwise>1</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</SLICECOUNT>
						<VALUES>
							<xsl:choose>
								<xsl:when test="PIN[contains(@pinname,'Input')]/@pinname">
									<xsl:if test="$Datatype = 'String'">
										<xsl:value-of select="PIN[contains(@pinname,'Input')]/@values"/>
									</xsl:if>
									<xsl:if test="$Datatype = 'Value'">
										<xsl:call-template name="Split">
											<xsl:with-param name="strInput" select="PIN[contains(@pinname,'Input')]/@values"/>
											<xsl:with-param name="strDelimiter" select="','"/>
										</xsl:call-template>
									</xsl:if>
									<xsl:if test="$Datatype = 'Color'">
										<xsl:value-of select="PIN[contains(@pinname,'Input')]/@values"/>
									</xsl:if>
									<xsl:if test="$Datatype = 'Enum'">
										<xsl:value-of select="PIN[contains(@pinname,'Input')]/@values"/>
									</xsl:if>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$Datatype = 'String'">
										<SLICE/>
									</xsl:if>
									<xsl:if test="$Datatype = 'Value'">
										<SLICE>0</SLICE>
									</xsl:if>
									<xsl:if test="$Datatype = 'Color'">
										<SLICE>|0.00000,1.00000,0.00000,1.00000|</SLICE>
									</xsl:if>
									<xsl:if test="$Datatype = 'Enum'">
										<SLICE>nil</SLICE>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</VALUES>
						<MIN>
							<xsl:choose>
								<xsl:when test="PIN[@pinname='Minimum']">
									<xsl:value-of select="PIN[@pinname='Minimum']/@values"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="substring-after(@nodename,'IOBox ')='(Value Advanced)' ">-1</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</MIN>
						<MAX>
							<xsl:choose>
								<xsl:when test="PIN[@pinname='Maximum']">
									<xsl:value-of select="PIN[@pinname='Maximum']/@values"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="substring-after(@nodename,'IOBox ')='(Value Advanced)' ">1</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</MAX>
					</BOX>
				</xsl:if>
			</xsl:for-each>
		</PATCH>
	</xsl:template>
</xsl:stylesheet>
