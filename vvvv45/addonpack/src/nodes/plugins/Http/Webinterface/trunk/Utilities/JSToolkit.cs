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


        public static string ReloadPageFuntkionContent()
        {
            return "window.location.reload ();" + Environment.NewLine;
        }



        public static string ReloadPageVar()
        {
            return  "var aktiv = window.setInterval(\"reload_window()\", 600);" + Environment.NewLine;
                            
        }



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



        public static string Alert()
        {
            string temp = "function HandleResponse() {" + Environment.NewLine +

                                //"if (http_request.readyState == 4)" + Environment.NewLine +
                                //"{" + Environment.NewLine +
                                //     "if (http_request.status == 200)" + Environment.NewLine +
                                //     "{" + Environment.NewLine +
                                //         //"alert(http_request.responseText);" + Environment.NewLine +
                                //     "}" + Environment.NewLine +
                                //     "else" + Environment.NewLine +
                                //     "{" + Environment.NewLine +
                                //        //"var xmldoc = http_request.responseText;" + Environment.NewLine +
                                //        //"alert(xmldoc);" + Environment.NewLine + 
                                //     "}" + Environment.NewLine +
                                //"}" + Environment.NewLine +
                            "}" + Environment.NewLine + Environment.NewLine;
            return temp;
        }



        public static string MouseMove()
        {
            return "window.captureEvents(Event.MOUSEMOVE);" + Environment.NewLine + 
                   "function nsmouse(evnt){ " + Environment.NewLine +
                   "xpos = evnt.pageX + 20;" +  Environment.NewLine +
                   "ypos = evnt.pageY + 20;" + Environment.NewLine + 
                   "macheRequest('mouse1','mouseY',xpos + '&' + ypos);" + Environment.NewLine + 
                   "}" + Environment.NewLine +
                   "window.onMouseMove = nsmouse;" + Environment.NewLine; 
        }



        public static string ButtonToggle()
        {
                return
                @"
                var getvalue = document.getElementById(pId).getAttribute('value')
                var element = document.getElementById(pId);

                

                var elementChildren = element.childNodes;
                for(var i = 0; i < elementChildren.length; i++)
                {
                    if(elementChildren.item(i).id == 'ButtonInlay')
                    {
                        var buttonInlay = elementChildren.item(i);
                    }
                }   

                if(getvalue == '0') {
                    document.getElementById(pId).setAttribute('value','1')             
                    buttonInlay.style.background = '#808080';
                    getvalue = '1';
                } 
                else {
                    document.getElementById(pId).setAttribute('value','0')
                    buttonInlay.removeAttribute('style'); 
                    getvalue = '0';
                }
                makeRequest(pId,getvalue);
                ";

        }



        public static string ButtonBang()
        {
            return
                @"
                    var element = document.getElementById(pId);
                    var elementChildren = element.childNodes;
                    var buttonInlay;
                    for(var i = 0; i < elementChildren.length; i++)
                    {
                        if(elementChildren.item(i).id == 'ButtonInlay')
                        {
                            buttonInlay = elementChildren.item(i);
                            buttonInlay.style.background = '#808080';
                        }
                    }   
                    makeRequest(pId,1);
                    var counter;
                    for(var i = 0; i < 100000; i++)
                    {
                        counter ++;
                    }
                    buttonInlay.removeAttribute('style'); 
                ";
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



        public static string TextfieldSendData()
        {
            return
            @"
            var getvalue = document.getElementById(pId).value
            makeRequest(pId, getvalue);
            ";
        }



        public static string Comet()
        {
            IFrame tIFrame = new IFrame();
            return tIFrame.Text;
        }



        public static string Polling(string IntervalTime, string ValuesToSent)
        {
            string temp = @"
<script>
    $(document).ready(function(){{
        $(document).everyTime({0},function(i) {{
            processChunk(i);    
        }}, 0);
    }});

    function processChunk(i){{ 
        $.ajax({{
            data: {1},
        }});
    }};   
</script>" + Environment.NewLine;

            return String.Format(temp, IntervalTime, ValuesToSent);
        }



        public static string ErrorMessage(string pFilename)
        {

            return
            String.Format(@"
            <!DOCTYPE HTML PUBLIC ""-//IETF//DTD HTML 2.0//EN"">
            <html><head>
            <title>404 Not Found</title>
            </head><body>
            <h1>Not Found</h1>
            <p>The requested File /{0} was not found on this server.</p>
            <hr>
            </body></html>
            ", pFilename);
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
