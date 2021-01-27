function Notify(vContent, vType, vFrom, vAlign) {
	if (!vFrom) vFrom = 'top';
	if (!vAlign) vAlign = 'right';
	if (!vType) vType = 'info';

	$.notify({
        message: vContent
        
	},
		{
			type: vType.toLowerCase() /* type_list:  inverse, info , success , warning , danger */ 
            //, timer: 1000 * 300
            , placement: {
				from: vFrom.toLowerCase(), /*top;bottom */
				align: vAlign.toLowerCase() /*left;center;right*/
            } 
            , z_index: 9999
		});
}
