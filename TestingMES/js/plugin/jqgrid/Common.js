
function CreateJqGridObjectWithRowFooter(vElemID, vURL, vDataType, vcolModel, vMultiselect = false, vMultikey = '', vrowNum = 0) {

	var LcMultikey = '', LcrowNum = 20;
	if (typeof vMultikey === "string")
		if (vMultikey.length > 0)
			LcMultikey = vMultikey;

	if (typeof vrowNum === "number")
		if (vrowNum > 0)
			LcrowNum = vrowNum;

	if (typeof vElemID == "string" && typeof vURL == "string" && typeof vDataType == "string" && typeof vcolModel == 'object') {
		try {
			var $grid = jQuery("#" + vElemID);

			$grid.jqGrid({
				mtype: 'GET'
				, url: vURL
				, datatype: vDataType
				, colModel: vcolModel

				, ajaxGridOptions: { contentType: 'application/json; charset=utf-8' }
				, loadonce: false
				, loadui: 'block'

				, autowidth: true
				, shrinkToFit: false
				, forceFit: false
				, height: window.innerHeight * 0.5

				, multiselect: vMultiselect
				, 'multiselectWidth': 35
				, multikey: LcMultikey /*shiftKey;altKey;ctrlKey*/

				, iconSet: "fontAwesome"
				, rowNum: LcrowNum //20
				, rowList: [20, 25, 30, 35, 40, 50, 100, 200, 500, "-1"]
				, footerrow: true
				, pager: '#' + vElemID + 'Pager'

				, viewrecords: true
				, rownumbers: true
				, rownumWidth: 30

				, loadComplete: function (xhr) {
					console.log(xhr);

					var selfgridID = this.id;

					var sumPLANQUANTITY = $('#' + selfgridID).jqGrid('getCol', 'PLANQUANTITY', false, 'sum');
					var sumREQUESTQTY = $('#' + selfgridID).jqGrid('getCol', 'REQUESTQTY', false, 'sum');

					console.log(sumPLANQUANTITY, sumREQUESTQTY);

					$('#' + selfgridID).jqGrid('footerData', 'set',
						{
							ITEMCOLORSERIAL: 'Material Readiness', REQUESTQTY: (sumPLANQUANTITY / sumREQUESTQTY) * 100
						});
				}

				, gridComplete: function () {
					$("#" + vElemID + "Pager option[value=-1]").text('All');

					var selfgridID = this.id;
					var ids = jQuery("#" + selfgridID).jqGrid('getDataIDs');
					var modvalue;

					for (var i = 0; i < ids.length; i++) {
						var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', ids[i]);
						modvalue = (i + 1) % 2;

						/*#Noted: Set row background color for chẵn and lẻ row */
						var backgroundColor;
						if (modvalue === 0) {
							backgroundColor = 'jqgrid-row-even';
						} else {
							backgroundColor = 'jqgrid-row-odd';
						}

						if (rowData.PLANQUANTITY == 0)
							backgroundColor = "jqgrid-row-Warning";
						else if (rowData.PLANQUANTITY < rowData.REQUESTQTY && rowData.PLANQUANTITY > 0)
							backgroundColor = "jqgrid-row-WIP";
						else if (rowData.PLANQUANTITY >= rowData.REQUESTQTY)
							backgroundColor = "jqgrid-row-Complete";


						//jQuery("#" + selfgridID).jqGrid('setRowData', ids[i], {}, { 'background': 'yellow' });
						jQuery("#" + this.id).jqGrid('setRowData', ids[i], {}, backgroundColor);
					}
				}
				, loadError: function (xhr, status, error) {
					if (xhr.responseText.length > 0)
						alert(xhr.responseText);
				}
			});

			$grid.jqGrid('navGrid', '#' + vElemID + 'Pager'
				, { add: true, edit: true, del: true, search: true }
				, { /*edit option*/ }
				, { /*add option*/ }
				, { /*delete option*/ }
				, {/*search option*/ }
				, {/*View option*/ }
			);

			return $grid;

		} catch (err) {
			alert(err.message);
			return null;
		}


	} else {
		alert("Invalid Input");
		return null;
	}
}


/**
 * @param {string} vElemID
 * @param {string} vURL
 * @param {string} vDataType 
 */
function CreateJqGridObjectWithoutColModel(vElemID, vURL, vDataType) {
	if (typeof vElemID == "string" && typeof vURL == "string" && typeof vDataType == "string") {
		try {
			var $grid = jQuery("#" + vElemID);

			$grid.jqGrid({
				url: vURL
				, datatype: vDataType

				, mtype: 'GET'
				, ajaxGridOptions: { contentType: 'application/json; charset=utf-8' }
				, loadonce: false

				, autowidth: true
				, shrinkToFit: false
				, forceFit: false
				, height: window.innerHeight * 0.72

				, iconSet: "fontAwesome"
				, rowNum: 20
				, rowList: [20, 25, 30, 35, 40, 50, 100, 200, 500]
				, pager: '#' + vElemID + 'Pager'

				, viewrecords: true
				, rownumbers: true

				, gridComplete: function () {
					var selfgridID = this.id;
					var ids = jQuery("#" + selfgridID).jqGrid('getDataIDs');
					var modvalue;

					for (var i = 0; i < ids.length; i++) {
						var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', ids[i]);
						modvalue = (i + 1) % 2;

						/*#Noted: Set row background color for chẵn and lẻ row */
						var backgroundColor;
						if (modvalue === 0) {
							backgroundColor = 'jqgrid-row-even';
						} else {
							backgroundColor = 'jqgrid-row-odd';
						}

						jQuery("#" + selfgridID).jqGrid('setRowData', ids[i], {}, backgroundColor);
					}
				}
				, loadError: function (xhr, status, error) {
					console.log(xhr);
					console.log(status);
					console.log(error);
					/*if (xhr.responseText.length > 0)
						alert(xhr.responseText);*/
				}
			});

			$grid.jqGrid('navGrid', '#' + vElemID + 'Pager'
				, { add: true, edit: true, del: true, search: true }
				, { /*edit option*/ }
				, { /*add option*/ }
				, { /*delete option*/ }
				, {/*search option*/ }
				, {/*View option*/ }
			);

			return $grid;

		} catch (err) {
			alert(err.message);
			console.log('Error at CreateJqGridObjectWithDynamicColModel(): ' + err.message);
			return null;
		}

	} else {
		alert("Invalid Input");
		return null;
	}
}

/**
 * @param {string} vElemID
 * @param {string} vURL
 * @param {string} vDataType
 * @param {array} vcolModel
 */
function CreateJqGridObject(vElemID, vURL, vDataType, vcolModel, vMultiselect = false, vMultikey = '', vrowNum = 0) {

	var LcMultikey = '', LcrowNum = 20;
	if (typeof vMultikey === "string")
		if (vMultikey.length > 0)
			LcMultikey = vMultikey;
	 
	if (typeof vrowNum === "number")
		if (vrowNum > 0)
			if (vrowNum > 500)
				LcrowNum = 500;
			else
				LcrowNum = vrowNum;
	 

	if (typeof vElemID == "string" && typeof vURL == "string" && typeof vDataType == "string" && typeof vcolModel == 'object') {
		try {
			var $grid = jQuery("#" + vElemID);

			$grid.jqGrid({
				mtype: 'GET'
				, url: vURL
				, datatype: vDataType
				, colModel: vcolModel

				, ajaxGridOptions: { contentType: 'application/json; charset=utf-8' }
				, loadonce: false

				//, autowidth: false
				, width: null
				, shrinkToFit: false
				//, forceFit: false
				, height: window.innerHeight * 0.7
                 
				, multiselect: vMultiselect
				, 'multiselectWidth': 35
				, multikey: LcMultikey /*shiftKey;altKey;ctrlKey*/

				, iconSet: "fontAwesome"
				, rowNum: LcrowNum //20
				, rowList: [20, 25, 30, 35, 40, 50, 100, 200, 500, "-1"]
				, pager: '#' + vElemID + 'Pager'
				, viewrecords: true

				//, rownumbers: true

				, loadComplete: function () { }
				, gridComplete: function () {
					$("#" + vElemID + "Pager option[value=-1]").text('All');

					var selfgridID = this.id;
					var ids = jQuery("#" + selfgridID).jqGrid('getDataIDs');
					var modvalue;

					for (var i = 0; i < ids.length; i++) {
						var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', ids[i]);
						modvalue = (i + 1) % 2;

						/*#Noted: Set row background color for chẵn and lẻ row */
						var backgroundColor;
						if (modvalue === 0) {
							backgroundColor = 'jqgrid-row-even';
						} else {
							backgroundColor = 'jqgrid-row-odd';
						}

						jQuery("#" + selfgridID).jqGrid('setRowData', ids[i], {}, backgroundColor);
					}
				}
				, loadError: function (xhr, status, error) {
					if (xhr.responseText.length > 0)
						alert(xhr.responseText);
				}
			});

			//$grid.jqGrid('navGrid', '#' + vElemID + 'Pager'
			//	, { add: true, edit: true, del: true, search: true }
			//	, { /*edit option*/ }
			//	, { /*add option*/ }
			//	, { /*delete option*/ }
			//	, {/*search option*/ }
			//	, {/*View option*/ }
			//);

			return $grid;

		} catch (err) {
			alert(err.message);
			return null;
		}


	} else {
		alert("Invalid Input");
		return null;
	}
}


/**
 * @param {any} vElemID
 * @param {any} vURL
 * @param {any} vDataType
 * @param {any} vcolModel
 * @param {any} vMultiselect
 * @param {any} vSubgrgid_URL
 * @param {any} vSubgrid_colModel
 */
function CreateJqGridObjectWithSimpleSubgrid(vElemID, vURL, vDataType, vcolModel, vMultiselect = true, vSubgrgid_URL, vSubgrid_colModel) {
	if (typeof vElemID == "string" && typeof vURL == "string" && typeof vDataType == "string" && typeof vcolModel == 'object') {
		try {
			var $grid = jQuery("#" + vElemID);

			$grid.jqGrid({
				mtype: 'GET'
				, url: vURL
				, datatype: vDataType
				, colModel: vcolModel

				, ajaxGridOptions: { contentType: 'application/json; charset=utf-8' }
				, loadonce: false

				, autowidth: true
				, shrinkToFit: false
				, forceFit: false
				, height: window.innerHeight * 0.72


				, multiselect: vMultiselect
				, 'multiselectWidth': 35

				, iconSet: "fontAwesome"
				, rowNum: 10
				, rowList: [10, 15, 20, 25, 30, 35, 40, 50, 100, 500, -1]
				, pager: '#' + vElemID + 'Pager'
				, viewrecords: true
				, loadComplete: function () { }

				/* subGrid Part */
				, subGrid: true
				, subGridUrl: vSubgrgid_URL
				, subGridModel: vSubgrid_colModel
				, subGridBeforeExpand: function (pID, id) {
					var selfgridID = this.id;
					var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', id);
					$(this).jqGrid("setGridParam", { subGridUrl: vSubgrgid_URL + '&MachineID=' + rowData.MACHINEID });
				}
				, gridComplete: function () {
					var selfgridID = this.id;
					var ids = jQuery("#" + selfgridID).jqGrid('getDataIDs');
					var modvalue;

					for (var i = 0; i < ids.length; i++) {
						var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', ids[i]);
						modvalue = (i + 1) % 2;

						/*#Noted: Set row background color for chẵn and lẻ row */
						var backgroundColor;
						if (modvalue === 0) {
							backgroundColor = 'jqgrid-row-even';
						} else {
							backgroundColor = 'jqgrid-row-odd';
						}

						jQuery("#" + selfgridID).jqGrid('setRowData', ids[i], {}, backgroundColor);
					}
				}
				, loadError: function (xhr, status, error) {
					if (xhr.responseText.length > 0)
						alert(xhr.responseText);
				}
			});

			$grid.jqGrid('navGrid', '#' + vElemID + 'Pager'
				, { add: true, edit: true, del: true, search: true }
				, { /*edit option*/ }
				, { /*add option*/ }
				, { /*delete option*/ }
				, {/*search option*/ }
				, {/*View option*/ }
			);

			return $grid;

		} catch (err) {
			alert(err.message);
			return null;
		}


	} else {
		alert("Invalid Input");
		return null;
	}
}

/**
 * @param {any} vElemID
 * @param {any} vURL
 * @param {any} vDataType
 * @param {any} vcolModel
 * @param {any} vMultiselect
 * @param {any} vSubgrgid_URL
 * @param {any} vSubgrid_colModel
 */
function CreateJqGridObjectWithComplexSubgrid(vElemID, vURL, vDataType, vcolModel, vMultiselect = true, vSubgrgid_URL, vSubgrid_colModel) {
	if (typeof vElemID == "string" && typeof vURL == "string" && typeof vDataType == "string" && typeof vcolModel == 'object') {
		try {
			var $grid = jQuery("#" + vElemID);

			$grid.jqGrid({
				mtype: 'GET'
				, url: vURL
				, datatype: vDataType
				, colModel: vcolModel
				, ajaxGridOptions: { contentType: 'application/json; charset=utf-8' }
				, autowidth: true
				, shrinkToFit: false
				, forceFit: false
				, height: window.innerHeight * 0.72

				, multiselect: vMultiselect
				, 'multiselectWidth': 35

				, iconSet: "fontAwesome"
				, rowNum: 20
				, rowList: [10, 15, 20, 25, 30, 35, 40, 50, 100, 500]
				, pager: '#' + vElemID + 'Pager'
				, viewrecords: true

				, loadComplete: function () { }

				/* subGrid Part */
				, subGrid: true
				, subGridOptions: {
					hasSubgrid: function (options) {
						/*var Res = options.data.HASPATTERN;*/
						return true;
					}
				}
				, subGridBeforeExpand: function (pID, id) {
					var selfgridID = this.id;
					var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', id);

					$(this).jqGrid("setGridParam", { subGridUrl: vSubgrgid_URL + '&MachineID=' + rowData.MACHINEID });
				}
				, subGridRowExpanded: function (subgrid_id, row_id) {
					// we pass two parameters
					// subgrid_id is a id of the div tag created within a table
					// the row_id is the id of the row
					// If we want to pass additional parameters to the url we can use
					// the method getRowData(row_id) - which returns associative array in type name-value
					// here we can easy construct the following
					var dt = jQuery("#" + this.id).jqGrid('getRowData', row_id);

					var subgrid_table_id = subgrid_id + "_t";
					jQuery("#" + subgrid_id).html("<table id='" + subgrid_table_id + "' class='scroll'></table>");

					jQuery("#" + subgrid_table_id).jqGrid({
						url: vSubgrgid_URL + '&MachineID=' + dt.MACHINEID,
						datatype: "json",
						colModel: vSubgrid_colModel,
						//autowidth: true,
						height: 'auto',
						gridComplete: function () {
							var selfgridID = this.id;

							if (!vMultiselect) {
								$('#cb_' + selfgridID).attr('disabled', 'disabled'); /*Hide the All-Pick checkbox on Header */
								$('#cb_' + selfgridID).hide(); /*Hide the All-Pick checkbox on Header */
							}

							var ids = jQuery("#" + selfgridID).jqGrid('getDataIDs');
							var modvalue = 0;

							for (var i = 0; i < ids.length; i++) {
								var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', ids[i]);
								modvalue = (i + 1) % 2;

								/*#Noted: Set row background color for chẵn and lẻ row */
								var backgroundColor;
								if (modvalue === 0) {
									backgroundColor = 'jqgrid-row-even';
								} else {
									backgroundColor = 'jqgrid-row-odd';
								}

								jQuery("#" + selfgridID).jqGrid('setRowData', ids[i], {}, backgroundColor);


							}
						},
						loadError: function (xhr, status, error) {
							if (xhr.responseText.length > 0)
								alert(xhr.responseText);
						}
					});
				}

				, gridComplete: function () {
					var selfgridID = this.id;
					var ids = jQuery("#" + selfgridID).jqGrid('getDataIDs');
					var modvalue;

					for (var i = 0; i < ids.length; i++) {
						var rowData = jQuery("#" + selfgridID).jqGrid('getRowData', ids[i]);
						modvalue = (i + 1) % 2;

						/*#Noted: Set row background color for chẵn and lẻ row */
						var backgroundColor;
						if (modvalue === 0) {
							backgroundColor = 'jqgrid-row-even';
						} else {
							backgroundColor = 'jqgrid-row-odd';
						}

						jQuery("#" + selfgridID).jqGrid('setRowData', ids[i], {}, backgroundColor);
					}
				}
				, loadError: function (xhr, status, error) {
					if (xhr.responseText.length > 0)
						alert(xhr.responseText);
				}
			});

			$grid.jqGrid('navGrid', '#' + vElemID + 'Pager'
				, { add: true, edit: true, del: true, search: true }
				, { /*edit option*/ }
				, { /*add option*/ }
				, { /*delete option*/ }
				, {/*search option*/ }
				, {/*View option*/ }
			);

			return $grid;

		} catch (err) {
			alert(err.message);
			return null;
		}


	} else {
		alert("Invalid Input");
		return null;
	}
}

/**
 * @param {any} jqGrid Object
 */
function pimpHeader(gridObj) {
    //var _arrGrid = [];
    //if (!Array.isArray(gridObj)){
    //    _arrGrid.push(gridObj);
    //}    
    var $gridArray = Array.isArray(gridObj) ? gridObj : [gridObj];
     
    $gridArray.forEach(function (element) {
        var cm = element.jqGrid("getGridParam", "colModel");
        for (var i = 0; i < cm.length; i++) {
            element.jqGrid('setLabel', cm[i].name, '',
                { 'text-align': (cm[i].align || 'left') },
                (cm[i].titletext ? { 'title': cm[i].titletext } : {}));
        }
    }); 
}



/** custom CSS for SmartAdmin   ONLY 
 */
function customJqGridCss() { 
    // remove classes
    $(".ui-jqgrid").removeClass("ui-widget ui-widget-content");
    $(".ui-jqgrid-view").children().removeClass("ui-widget-header ui-state-default");
    $(".ui-jqgrid-labels, .ui-search-toolbar").children().removeClass("ui-state-default ui-th-column ui-th-ltr");
    $(".ui-jqgrid-pager").removeClass("ui-state-default");
    $(".ui-jqgrid").removeClass("ui-widget-content");

    // add classes
    $(".ui-jqgrid-htable").addClass("table table-bordered table-hover");
    $(".ui-jqgrid-btable").addClass("table table-bordered table-striped");

    $(".ui-pg-div").removeClass().addClass("btn btn-sm btn-primary");
    $(".ui-icon.ui-icon-plus").removeClass().addClass("fa fa-plus");
    $(".ui-icon.ui-icon-pencil").removeClass().addClass("fa fa-pencil");
    $(".ui-icon.ui-icon-trash").removeClass().addClass("fa fa-trash-o");
    $(".ui-icon.ui-icon-search").removeClass().addClass("fa fa-search");
    $(".ui-icon.ui-icon-refresh").removeClass().addClass("fa fa-refresh");
    $(".ui-icon.ui-icon-disk").removeClass().addClass("fa fa-save").parent(".btn-primary").removeClass("btn-primary").addClass("btn-success");
    $(".ui-icon.ui-icon-cancel").removeClass().addClass("fa fa-times").parent(".btn-primary").removeClass("btn-primary").addClass("btn-danger");

    $(".ui-icon.ui-icon-seek-prev").wrap("<div class='btn btn-sm btn-default'></div>");
    $(".ui-icon.ui-icon-seek-prev").removeClass().addClass("fa fa-backward");

    $(".ui-icon.ui-icon-seek-first").wrap("<div class='btn btn-sm btn-default'></div>");
    $(".ui-icon.ui-icon-seek-first").removeClass().addClass("fa fa-fast-backward");

    $(".ui-icon.ui-icon-seek-next").wrap("<div class='btn btn-sm btn-default'></div>");
    $(".ui-icon.ui-icon-seek-next").removeClass().addClass("fa fa-forward");

    $(".ui-icon.ui-icon-seek-end").wrap("<div class='btn btn-sm btn-default'></div>");
    $(".ui-icon.ui-icon-seek-end").removeClass().addClass("fa fa-fast-forward");
}
