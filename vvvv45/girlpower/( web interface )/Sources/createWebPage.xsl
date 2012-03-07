<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" version="1.0" encoding="UTF-8" indent="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"/>
	
	
	<xsl:key name="PATCH" match="BOX" use="GROUP"/>
	
	
	
	<xsl:template match="/PATCH">
		<!-- 	
		Begin  HTML
		Output
		 -->
		<html xmlns="http://www.w3.org/1999/xhtml" lang="de">
		<head>
		<link rel="stylesheet" type="text/css" href="styles.css" />
		<title>VVVV Patch Remoting</title>
			<script src="scripts.js" type="text/javascript"></script>
		</head>
		<body>			
		<div class="container">
			<div class="header"><xsl:value-of select="/PATCH/@name"/></div>
			<div class="content">
				<h1>Welcome </h1>
				<p>This is a web-interface for a vvvv patch! You need valid username and a valid password to change values.</p>
				<hr />
				<h1 style="margin-top: 20px;">Web Interface</h1>
				<form action="" method="post">
					 
					<!-- Here we start generating output. 
					For every Group we create one HTML table -->
					<xsl:for-each select="//BOX[generate-id(.)=generate-id(key('PATCH',GROUP))]">					 
					 <table cellspacing="0">
						 <tr>
							<th colspan="5"><xsl:value-of select="GROUP"/></th>
						 </tr>
						 
						 <!--Go through the IOBoxes and Sort at the  secondary key " IOBox Name",  sorted by name-->	
						 <xsl:for-each select="key('PATCH',GROUP)">
							<xsl:sort select="@name"/> 
							<xsl:variable name="slicecount" select="SLICECOUNT"/>
							<xsl:variable name="datatype" select="@datatype"/>
							
								<xsl:for-each select="VALUES/SLICE">
								<tr>
									<xsl:if test="position()=1">
										<td class="ioboxName" rowspan="{$slicecount}">
											<xsl:choose>
											<xsl:when test="contains(../../@name, ':')">
												<xsl:value-of select="translate(substring-after(../../@name, ':'),'|','') "/>
											</xsl:when>
											<xsl:otherwise>
												<xsl:value-of select="../../@name"/>
											</xsl:otherwise>					
										</xsl:choose>		
										 (<xsl:value-of select="../../@datatype"/>)
										</td>
									</xsl:if>
									<td class="sliceindex"><xsl:value-of select="position()"/></td>
									<td class="value">
									
										<xsl:if test="$datatype='Color'">
											Alpha: <input name="{../../@name}---{position()}-alpha" id="{../../@name}" value="{ALPHA}" maxlength="100" class="alpha" onblur="return chkValues(this.id,0,1)" />
											<input name="{../../@name}---{position()}" id="{../../@name}---{position()}" value="{HEX}" maxlength="100" class="color" style="background-color: {HEX};" onblur="return updateColor(this.id)"  /><span></span>
											<a href="javascript:openColorPickerAdv('{../../@name}---{position()}');"><img src="pick.gif" /></a>
										</xsl:if>
										
										<xsl:if test="$datatype='String'">
											<input name="{../../@name}---{position()}" id="{../../@name}---{position()}"  maxlength="100" class="string" value="{.}" />
										</xsl:if>
										
										<xsl:if test="$datatype='Value'">
											<input name="{../../@name}---{position()}" id="{../../@name}---{position()}"  maxlength="100" class="Value" value="{.}"/>
										</xsl:if>
										
										</td>
								</tr>
							</xsl:for-each>
							  
			  
			  
			  
					 
						</xsl:for-each>
					 

  		</table>
		<br/><br/>
	</xsl:for-each>					 
					 
					 
					 
					 
					 	 
					  
					 
					Username: <input name="username"  maxlength="100" class="value" value="" /><br /><br />
					Password: <input name="password"  maxlength="100" class="value" value="" type="password" /><br />
					<input type="submit" value="submit" class="submit" />
				</form>
				</div>
			<div class="footer"></div>
		</div>
		</body>
		</html>





	</xsl:template>
</xsl:stylesheet>
