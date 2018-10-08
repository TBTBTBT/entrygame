
window.onload = function(){
	//DialogOpen();
}
function DialogBack(){
	var element_bg = document.createElement("div");
	var style = element_bg.style;
	style.position = "fixed";
	style.left   = "0px";
	style.right  = "0px";
	style.top    = "0px";
	style.bottom = "0px";
	style.backgroundColor = "#000";
	var opacity = 0.5;
	style.opacity = opacity;
	style.filter = "alpha(opacity=" + (opacity * 100) + ")";
	document.body.appendChild(element_bg);
}
function DialogOpen(){
	
}
