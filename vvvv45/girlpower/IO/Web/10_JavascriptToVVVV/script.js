function mouseEnterEvent(obj)
{
	// this function sends two key-value pairs as an XElement to VVVV
	
	window.vvvvSend({ button : obj.innerHTML, action: "Enter" })
}

function mouseLeaveEvent(obj)
{
	window.vvvvSend({ button : obj.innerHTML, action: "Leave" })
}

function clickEvent(obj)
{
	window.vvvvSend({ button : obj.innerHTML, action: "Click" })
}