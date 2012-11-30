<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output indent="no" method="html" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" />
  <xsl:template match="/">
    <html xmlns="http://www.w3.org/1999/xhtml">
      <head>
        <style>
          body {
            font-family: Verdana;
          }
          table {
            border-spacing: 0;
            padding: 0;
            margin: 0;
          }
          td {
            border: 0px solid #000000;
            font-size: 10pt;
            vertical-align: top;
            padding: 0;
            border-spacing: 0;
            padding: 0;
            margin: 0;
          }
          .collapseWidth, .collapsable {
            width: 1em ! important; 
            text-align: center;
            padding: 0;
            color: #ff0000;
          }
          .collapsable, .collapsableRow {
            cursor: pointer;
          }

          .collapsed .collapsableContent {
            display: none;
          }

          .marker {
            color: #0000ff;
          }
          .namespace, .namespacename {
            color: #ff0000;
          }
          .namespace {
            font-weight: bold;
          }
          .elementname, .attributename {
            color: #990000;
          }
          .comment {
            color: #888888;
          }
          .attributevalue {
            color: #000000;
            font-weight: bold;
          }
          .cdata {
            font-family: Courier New;
            color: #000000;
          }
          .cdatavalue {
            color: #000000;
            font-weight: bold;
          }
        </style>
        <script>
        <xsl:text>
        function expandCollapse(elementClicked) {
            var element = elementClicked;
            if (element.className == 'collapsableRow') {
                element = elementClicked.parentNode.parentNode;
            }

            if (element.parentNode.className == 'collapsed') {
              element.parentNode.className = '';
              element.parentNode.firstChild.innerHTML = '-';
            } else {
              element.parentNode.className = 'collapsed';
              element.parentNode.firstChild.innerHTML = '+';
            }
        }
        function ShowError() {
            if (location.search) {
                var anchorString = location.search.substring(1, location.search.length);
                var anchorStringArray = anchorString.split(',');
                
                var i = 0;
                for (; i != anchorStringArray.length; i++) {
                    var anchor = document.getElementById('a'+anchorStringArray[i]);
                    if (anchor) {
                        anchor.style.backgroundColor = '#ffff00';
                    }
                    /*
                    anchor = document.getElementById('a-end'+anchorStringArray[i]);
                    if (anchor) {
                        anchor.style.backgroundColor = '#ffff00';
                    }
                    */
                }
            }
        }
        </xsl:text>
        </script>
      </head>
      <body onload="ShowError()">
        <table cellpadding="0" cellspacing="0">
          <tr>
            <td><div class="collapseWidth" /></td>
            <td>
              <span class="marker">&lt;?xml version="1.0"?&gt;</span>
            </td>
          </tr>
        </table>
        <xsl:apply-templates/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="*">
    <xsl:variable name="linenumber"><xsl:number level="any" count="//*"/></xsl:variable>
    <table width="100%" cellpadding="0" cellspacing="0">
      <tr>
        <td class="collapseWidth" width="5px">
        <div class="collapseWidth" /></td>
        <td><a name="a{$linenumber}" id="a{$linenumber}">
            <span class="marker">&lt;</span>
            <span class="elementname"><xsl:value-of select="name()" /></span>
            <xsl:call-template name="findNamespace" />
            <xsl:apply-templates select="@*" />
            <span class="marker"> /&gt;</span>
            </a>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="*[text()]">
    <xsl:variable name="linenumber"><xsl:number level="any" count="//*"/></xsl:variable>
    <table width="100%" cellpadding="0" cellspacing="0">
      <tr>
        <td class="collapseWidth" width="5px">
        <div class="collapseWidth" /></td>
        <td><a name="a{$linenumber}" id="a{$linenumber}">
            <span class="marker">&lt;</span>
            <span class="elementname"><xsl:value-of select="name()" /></span>
            <xsl:call-template name="findNamespace" />
            <xsl:apply-templates select="@*" />
            <span class="marker">&gt;</span>
            <xsl:apply-templates/>
            <span class="marker">&lt;/</span>
            <span class="elementname"><xsl:value-of select="name()" /></span>
            <span class="marker">&gt;</span>
            </a>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="* [ * or comment() or processing-instruction() or string-length(text()) > 64 ]">
    <xsl:variable name="linenumber"><xsl:number level="any" count="//*"/></xsl:variable>
    <table width="100%" cellpadding="0" cellspacing="0">
      <tr>
        <td class="collapsable" width="5px" onclick="expandCollapse(this);">-
        <div class="collapseWidth" /></td>
        <td><a name="a{$linenumber}" id="a{$linenumber}">
            <div class="collapsableRow" onclick="expandCollapse(this);">
            <span class="marker">&lt;</span>
            <span class="elementname"><xsl:value-of select="name()" /></span>
            <xsl:call-template name="findNamespace" />
            <xsl:apply-templates select="@*" />
            <span class="marker">&gt;</span>
            </div>
            </a>
            <div class="collapsableContent">
                <xsl:apply-templates/>
            </div>
            <a name="a-end{$linenumber}" id="a-end{$linenumber}">
            <span class="marker">&lt;/</span>
            <span class="elementname"><xsl:value-of select="name()" /></span>
            <span class="marker">&gt;</span>
            </a>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="comment() [ string-length() > 64 ]">
    <xsl:variable name="linenumber"><xsl:number level="any" count="//*"/></xsl:variable>
    <table width="100%" cellpadding="0" cellspacing="0">
      <tr>
        <td class="collapsable" width="5px" onclick="expandCollapse(this)">-
        <div class="collapseWidth" /></td>
        <td><a name="a{$linenumber}" id="a{$linenumber}">
          <span class="marker">&lt;!--</span>
          <div class="collapsableContent">
            <span class="comment">
              <xsl:value-of select="current()" />
            </span>
          </div>
          <span class="marker">--&gt;</span>
          </a>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="comment()">
    <xsl:variable name="linenumber"><xsl:number level="any" count="//*"/></xsl:variable>
    <table width="100%" cellpadding="0" cellspacing="0">
      <tr>
        <td class="collapseWidth" width="5px">
        <div class="collapseWidth" /></td>
        <td><a name="a{$linenumber}" id="a{$linenumber}">
          <span class="marker">&lt;!--</span>
          <span class="comment">
            <xsl:value-of select="current()" />
          </span>
          <span class="marker">--&gt;</span>
          </a>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="@*">
    <xsl:text> </xsl:text>
    <span class="attributename"><xsl:value-of select="name()" /></span>
    <span class="marker">=&quot;</span>
    <span class="attributevalue">
      <xsl:call-template name="replaceAmpersands">
        <xsl:with-param name="inputString" select="string(current())" />
      </xsl:call-template>
    </span>
    <span class="marker">&quot;</span>
  </xsl:template>

  <xsl:template match="text()">
    <span class="cdatavalue">
      <xsl:call-template name="replaceAmpersands">
        <xsl:with-param name="inputString" select="string(current())" />
      </xsl:call-template>
    </span>
  </xsl:template>

  <xsl:template match="processing-instruction()">
    <xsl:variable name="linenumber"><xsl:number level="any" count="//*"/></xsl:variable>
      <table cellpadding="0" cellspacing="0">
        <tr>
          <td><div class="collapseWidth" /></td>
          <td><a name="a{$linenumber}" id="a{$linenumber}">
            <span class="marker">&lt;?<xsl:value-of select="name()" />
              <xsl:text> </xsl:text>
              <xsl:value-of select="current()" />?&gt;</span>
          </a>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="replaceAmpersands">
    <xsl:param name="inputString" />
    <xsl:variable name="ampString">&amp;</xsl:variable>
 
    <xsl:choose>
      <xsl:when test="contains($inputString, $ampString)">
        <xsl:value-of select="substring-before($inputString, $ampString)" />
        <xsl:value-of select="concat($ampString, 'amp;')" />
        <xsl:call-template name="replaceAmpersands">
          <xsl:with-param name="inputString" select="substring-after($inputString, $ampString)" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$inputString" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="findNamespace">
    <xsl:variable name="curnode" select="." />

    <xsl:for-each select=".|@*">
      <xsl:variable name="vName" select="substring-before(name($curnode), ':')"/>
      <xsl:variable name="vUri" select="namespace-uri($curnode)"/>

      <xsl:variable name="vAncestNamespace">
        <xsl:call-template name="findAncNamespace">
          <xsl:with-param name="pName" select="$vName"/>
          <xsl:with-param name="pUri" select="$vUri"/>
          <xsl:with-param name="pNode" select="$curnode" />
        </xsl:call-template>
      </xsl:variable>

      <xsl:if test="not(number($vAncestNamespace))">
        <xsl:if test="$curnode/parent::* or namespace-uri($curnode) or contains(name($curnode), ':')">
          <xsl:text> </xsl:text>
          <span class="namespacename">
            <xsl:value-of select="'xmlns'"/>
            <xsl:if test="contains(name($curnode), ':')">
              <xsl:value-of select="concat(':', $vName)"/>
            </xsl:if>
          </span>
          <span class="markup">="</span>
          <span class="namespace">
            <xsl:value-of select="namespace-uri($curnode)"/>
          </span>
          <span class="markup">"</span>
        </xsl:if>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="findAncNamespace">
    <xsl:param name="pNode" select="."/>
    <xsl:param name="pName" select="substring-before(name(), ':')"/>
    <xsl:param name="pUri" select="namespace-uri(.)"/>

    <xsl:choose>
      <xsl:when test="not($pNode/parent::*)">0</xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="not(($pName = substring-before(name($pNode/..), ':')
                          and $pUri  = namespace-uri($pNode/..))
                          or $pNode/../@*[$pName = substring-before(name(), ':')
                          and $pUri = namespace-uri()]
                          )">
            <xsl:call-template name="findAncNamespace">
              <xsl:with-param name="pNode" select="$pNode/.."/>
              <xsl:with-param name="pName" select="$pName"/>
              <xsl:with-param name="pUri" select="$pUri"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>1</xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>