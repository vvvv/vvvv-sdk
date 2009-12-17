using System;
using System.Collections.Generic;
using System.Text;


namespace VVVV.Webinterface.Utilities
{
    class JSToolkit
    {

        #region constructor
        public JSToolkit()
        {

        }
        #endregion constructor






        public static string XmlHttpRequest()
        {
            string temp = "var http_request = false;" + Environment.NewLine +


                            "function macheRequest(url,form,name) {" + Environment.NewLine +
                                "http_request = false;" + Environment.NewLine + Environment.NewLine +
                                "var Element = document.getElementById(name);" + Environment.NewLine +

                                "if (window.XMLHttpRequest) " + Environment.NewLine +
                                "{" + Environment.NewLine + 
                                    "http_request = new XMLHttpRequest();" + Environment.NewLine + 
                                    "if (http_request.overrideMimeType) " + Environment.NewLine + 
                                    "{" + 
                                        "http_request.overrideMimeType('text/xml');" + 
                                     "}" + 
                                 "}" + Environment.NewLine + Environment.NewLine +
                    		
                                "if (!http_request)"+ Environment.NewLine + 
                                 "{" +  Environment.NewLine +
                                    "alert('Ende :( Kann keine XMLHTTP-Instanz erzeugen');" + Environment.NewLine  + 
                                    "return false;" + Environment.NewLine + 
                                "}" + Environment.NewLine + Environment.NewLine +
                    		
                                //"http_request.onreadystatechange = HandleResponse;" + Environment.NewLine +
                                "http_request.open('POST', url, true);" + Environment.NewLine + 
                                "http_request.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');" + Environment.NewLine +
                                "http_request.send('?' + form + '=' + name + '=' + Element.value );" + Environment.NewLine +
                            "}" + Environment.NewLine + Environment.NewLine;

            return temp;
        }


        



        public static string MakeRequest()
        {
            return @"        
                   function makeRequest(pId,content) {
                   var httpRequest;
                    if (window.XMLHttpRequest) { // Mozilla, Safari, ...
                        httpRequest = new XMLHttpRequest();
                        if (httpRequest.overrideMimeType) {
                            httpRequest.overrideMimeType('text/xml');
                            // See note below about this line
                        }
                    } 
                    else if (window.ActiveXObject) { // IE
                        try {
                            httpRequest = new ActiveXObject('Msxml2.XMLHTTP');
                        } 
                        catch (e) {
                            try {
                                httpRequest = new ActiveXObject('Microsoft.XMLHTTP');
                            } 
                            catch (e) {}
                        }
                    }

                    if (!httpRequest) {
                        alert('Giving up :( Cannot create an XMLHTTP instance');
                        return false;
                    }
                    httpRequest.onreadystatechange = function() { HandleResponse(httpRequest); };
                    httpRequest.open('POST', 'ToVVVV.xml', true);
                    httpRequest.send('?' + pId + '=' + content);
                    }
                    ";
        }


        public static string Comet()
        {
            IFrame tIFrame = new IFrame();
            return tIFrame.Text;
        }



        public static string ResizeBrowser(string pWidth, string pHeight)
        {
            string tCode = @"
<script>
$(document).ready(function(){{  
    window.resizeTo({0},{1}); 
}}); 
</script>" + Environment.NewLine;
            return String.Format(tCode, pWidth, pHeight); ;
        }
    }
}
