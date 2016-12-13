// Functions for the Color Picker
function colorPicker_callBack(strColor) {
	document.getElementById(id).style.backgroundColor =  strColor;
	document.getElementById(id).value = strColor;
}

function colorView_CB(strColor) {
	document.getElementById(id).style.backgroundColor = strColor;
	document.getElementById(id).value = strColor;
}

function openColorPickerAdv(strId) {
	document.getElementById(strId).nextSibling.innerHTML = "<div id='colorPickerAdvDiv' style='margin-left:200px;margin-top:-20px;visibility:hidden;padding:0;position:absolute;z-index:1'><iframe id='colorPickerAdv'  width='323' src='./colorPickerAdv.html' height='120' style='border-style:outset;border-width:2px;' marginwidth='0' marginheight='0' noresize frameborder='0' border='0'></iframe></div>";
	if (document.getElementById('colorPickerAdvDiv').style.visibility != 'visible') {
		document.getElementById('colorPickerAdvDiv').style.visibility = 'visible';
		id = strId;
	}
}
function closeColorPickerAdv() {
	document.getElementById('colorPickerAdvDiv').parentNode.innerHTML = "";
}

function updateColor(id) {
  var curr = document.getElementById(id).value;
  document.getElementById(id).style.backgroundColor = curr;
}
// Functions for Value IOBoxes
function chkValues (a,minimum,maximum) {
  var chkZ = 1;
  var toSmall = 0;
  var toHigh = 0;
  document.getElementById(a).nextSibling.innerText = "";
  document.getElementById(a).style.backgroundColor = "#FFFFFF";
  if (document.getElementById(a).value < minimum ) { chkZ = -1; toSmall = 1}
  if (document.getElementById(a).value > maximum ) { chkZ = -1; toHigh = 1} 
  for (i = 0; i < document.getElementById(a).value.length; ++i)
    if (document.getElementById(a).value.charAt(i) != "." &&
		document.getElementById(a).value.charAt(i) < "0" ||
        document.getElementById(a).value.charAt(i) > "9")
	  chkZ = -1;
	  
  if (chkZ == -1) {
    document.getElementById(a).focus();
	document.getElementById(a).style.backgroundColor = "#ff0000";
	if (toSmall == 1) document.getElementById(a).nextSibling.innerText = "Not allowed: to small!";
	if (toHigh == 1) document.getElementById(a).nextSibling.innerText = "Not allowed: to big!";
    return false;
  }
}