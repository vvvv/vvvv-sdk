function XmlHttpRequest(message)
{
	var XmlHttp = null;
	try 
	{
		// Mozilla, Opera, Safari sowie Internet Explorer (ab v7)
    XmlHttp = new XMLHttpRequest();
	} catch(e) 
	{
		try 
		{
			// MS Internet Explorer (ab v6)
			XmlHttp  = new ActiveXObject("Microsoft.xmlHttp");
		} catch(e) 
		{
			try 
			{
				// MS Internet Explorer (ab v5)
				XmlHttp  = new ActiveXObject("Msxml2.xmlHttp");
			} catch(e) 
			{
				XmlHttp  = null;
			}
		}
	}
	
	if (XmlHttp) 
	{
		XmlHttp.open('POST', 'ToVVVV.xml', true);
		XmlHttp.onreadystatechange = function () 
		{
			if (XmlHttp.readyState == 4) {
				//console.log(xmlHttp.responseText);
			}
		};
		XmlHttp.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
		XmlHttp.setRequestHeader('Cache-Control', 'no-cache');
		XmlHttp.send(message);
	}

}