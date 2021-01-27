function fancyConfirm(msg, callback) {
	var ret;
	$.fancybox.open(
		"<div class=\"fancyConfirm\" ><p>" + msg + "</p>" +
		"<div class=\"fancyButtons\">" +
		"   <input class=\" btn btn-success\" id=\"fancyConfirm_ok\" type=\"button\" value=\"Ok\">" +
		"   <input class=\"btn btn-link\" id=\"fancyConfirm_cancel\" type=\"button\" value=\"Cancel\">" +
		"</div></div>",
		{
			//'src': ,
			type: 'html',
			'modal': true,
			smallBtn: false,
			afterShow: function () {
				$("#fancyConfirm_cancel").click(function () {
					ret = false;
					$.fancybox.close();
				});
				$("#fancyConfirm_ok").click(function () {
					ret = true;
					$.fancybox.close();
				});
			},
			afterClose: function () {
				if (typeof callback == 'function') {
					callback.call(this, ret);
				};
			}
		});
}

function fancyAlert(msg) {
	$.fancybox.open('<div class="fancyboxmessage">' + msg + '</div>');
}