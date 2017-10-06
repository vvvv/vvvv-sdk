function mouseEnterEvent(obj)
{
	// this function sends two key-value pairs as an XElement to VVVV
	
	window.vvvvSend({ button : obj.innerHTML, action: "enter" })
}

function mouseLeaveEvent(obj)
{
	window.vvvvSend({ button : obj.innerHTML, action: "leave" })
}

function clickEvent(obj)
{
	window.vvvvSend({ button : obj.innerHTML, action: "click" })
}