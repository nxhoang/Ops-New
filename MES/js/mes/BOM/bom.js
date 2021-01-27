function initBOMPage() {
    //Init partial view package group
    initPackageGroupPartialView();

    //Set factory base on factory of user role.
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    bindDataToJqGridEngineeringBOM(null, null, null, null);

    bindDataToJqGridModuleBOM(null, null, null, null);
}


//#region Redefine functions for partial view package group
function SelectedRowPackageGroup(dataRow) {

}

//Redefine event selected row
function eventSelectedRowOnPackageGroupGrid(rowData) {

    //Reload BOM
    var params = { styleCode: rowData.StyleCode, styleSize: rowData.StyleSize, styleColorSerial: rowData.StyleColorSerial, revNo: rowData.RevNo};
    ReloadJqGrid2LoCal("tbEngineeringBOM", params);

    //Reload module BOM
    ReloadJqGrid2LoCal("tbModuleBOM", params);
}

function BeforeSelectRowPackageGv(rowid, e) {
    return true;
}

//Redifine function search package group
function eventSearchPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo) {
   
}
//#endregion

//#region Bind data to gridview
function bindDataToJqGridEngineeringBOM(styleCode, styleSize, styleColorSerial, revNo) {

    jQuery("#tbEngineeringBOM").jqGrid({
        url: '/BOM/GetBOM',
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo
        },
        datatype: "json",
        height: 500,
        width: null,
        shrinkToFit: false,
        colModel: [
            { name: 'MainItemCode', index: 'MainItemCode', label: "Main Item", width: 150, classes: 'pointer', hidden: true },
            { name: 'MainItemName', index: 'MainItemName', label: "Main Item Name", width: 150, classes: 'pointer', hidden: true },
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer' },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 150, classes: 'pointer' },
            { name: 'ItemColor', index: 'ItemColor', label: "Item Color", width: 150, classes: 'pointer' },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", align: 'center', classes: 'pointer' },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Consumption Unit" },
            { name: 'Qty', index: 'Qty', label: "Qty", width: 50, align: 'center', classes: 'pointer' },
            { name: 'StdPrice', index: 'StdPrice', label: "Standard Price", width: 100, align: 'center', classes: 'pointer'},
            { name: 'CurrCode', index: 'CurrCode', label: "Currency", width: 70, align: 'center', classes: 'pointer'},
            { name: 'RegistryDate', index: 'RegistryDate', label: "Registry Date", width: 100, align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" }},
            { name: 'RegisterName', index: 'RegisterName', label: "Register Name", width: 100, align: 'center', classes: 'pointer' },
            { name: 'PATTERNCONS', index: 'PATTERNCONS', label: "Pattern Cons", width: 100, align: 'center', classes: 'pointer' },
            { name: 'POLYCONS', index: 'POLYCONS', label: "Poly Consumption", width: 120, align: 'center', classes: 'pointer' },
            { name: 'MARKERCONS', index: 'MARKERCONS', label: "Auto Cons", width: 80, align: 'center', classes: 'pointer' },
            { name: 'MARKERCONSUNIT', index: 'MARKERCONSUNIT', label: "Auto Cons Unit", width: 100, align: 'center', classes: 'pointer' },
            { name: 'CAD_MATERIAL', index: 'CAD_MATERIAL', label: "Cad Material", width: 100, align: 'center', classes: 'pointer' },
            { name: 'SosName', index: 'SosName', label: "SOS", width: 100, align: 'center', classes: 'pointer' },
            { name: 'GENNAME', index: 'GENNAME', label: "Gen Name", width: 70, align: 'center', classes: 'pointer' },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }

        ],
        rowNum: 1000,
        sortname: 'id',
        sortorder: "asc",
        gridComplete: function () {
            let ids = jQuery("#tbEngineeringBOM").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbEngineeringBOM").jqGrid("getRowData", i);               
                if (rowdata.HasPattern !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbEngineeringBOM_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbEngineeringBOM_subgrid]").unbind('click');
                                        
                }
            }
        },
        loadComplete: function () {
         
        },
        onSelectRow: function (rowid) {
            //$("#tbEngineeringBOM").expandSubGridRow(rowid); 
            //Get row data
            //var rowdata = $(tableMesPackageId).jqGrid("getRowData", rowid);
        },       
        subGrid: true,       
        subGridRowExpanded: subGridviewEngineeringBOM        
    });
    
    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();
}

function subGridviewEngineeringBOM(subgridDivId, rowId) {
    var rowData = $('#tbEngineeringBOM').jqGrid('getRowData', rowId);
    var subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/BOM/GetPatterns',
        postData: {
            styleCode: rowData.StyleCode, styleSize: rowData.StyleSize, styleColorSerial: rowData.StyleColorSerial, revNo: rowData.RevNo, itemCode: rowData.ItemCode, itemColorSerial: rowData.ItemColorSerial
        },
        datatype: "json",
        width: null,
        shrinkToFit: false,
        colModel: [
            { name: '', index: '', label: "", width: 30, classes: 'pointer', formatter: patternImage},
            { name: 'Piece', index: 'Piece', label: "Description", width: 250, classes: 'pointer'},
            { name: 'Height', index: 'Height', label: "Height", width: 50, classes: 'pointer'},
            { name: 'Width', index: 'ItemCode', label: "Width", width: 50, classes: 'pointer' },
            { name: '', index: '', label: "Pattern Area", width: 100, classes: 'pointer' },
            { name: 'Area', index: 'Area', label: "Polygon Area", width: 50, classes: 'pointer' },
            { name: 'MainPartName', index: 'MainPartName', label: "Main Part", width: 150, align: 'center', classes: 'pointer' },
            { name: 'SizeUnit', index: 'SizeUnit', label: "Size Unit", width: 70 },
            { name: 'EndWise', index: 'EndWise', label: "End Wise", width: 70, align: 'center', classes: 'pointer' },
            { name: 'PieceQty', index: 'PieceQty', label: "Piece Qty", width: 70, align: 'center', classes: 'pointer' },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Unit Cons", width: 70, align: 'center', classes: 'pointer' },
            { name: 'CurrCode', index: 'CurrCode', label: "Currentcy", width: 70, align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" } },
            { name: 'PieceUnique', index: 'PieceUnique', label: "Unique", width: 70, align: 'center', classes: 'pointer' },
            { name: 'PatternSerial', index: 'PatternSerial', label: "Serial", width: 50, align: 'center', classes: 'pointer' },
            { name: 'Url', index: 'Url', hidden: true }

        ],
        rowNum: 1000,
        height: '100%'       
    });    
}

function patternImage(cellvalue, options, rowObject) {
    return "<img src='" + rowObject.Url + "' width='20' height='10' />";
}

function bindDataToJqGridModuleBOM(styleCode, styleSize, styleColorSerial, revNo) {

    jQuery("#tbModuleBOM").jqGrid({
        url: '/BOM/GetModule',
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo
        },
        datatype: "json",
        height: 500,
        width: null,
        shrinkToFit: false,
        colModel: [           
            { name: 'ModuleId', index: 'ModuleId', label: "Part Id", width: 200, classes: 'pointer' },
            { name: 'ModuleName', index: 'ModuleName', label: "ModuleName", width: 350, classes: 'pointer' },
            { name: 'FinalAssembly', index: 'FinalAssembly', label: " Final Assembly", width: 150, classes: 'pointer' },
            { name: 'RegistryDate', index: 'RegistryDate', label: "Registry Date", width: 150, classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" } },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'PartId', index: 'PartId', hidden: true },            
            { name: 'HasItem', index: 'HasItem', hidden: true }

        ],
        rowNum: 1000,
        sortname: 'id',
        sortorder: "asc",
        gridComplete: function () {
            let ids = jQuery("#tbModuleBOM").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbModuleBOM").jqGrid("getRowData", i);
               
                if (rowdata.HasItem !== "Y") {
                    //Hide plus icon if item has no item
                    $("tr[id=" + i + "]>td[aria-describedby$=tbModuleBOM_subgrid]").html("&nbsp;");
                }
            }
        },
        loadComplete: function () {

        },
        onSelectRow: function (rowid) {
            //Get row data
            //var rowdata = $(tableMesPackageId).jqGrid("getRowData", rowid);
        },
        subGrid: true,
        subGridRowExpanded: subGridviewModuleBOM
    });

    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

}

function subGridviewModuleBOM(subgridDivId, rowId) {
    //Get row data from group package
    var rowData = GetSelectedOneRowData("#tbGroupPackage");
    //Get row data from module
    var mdlData = $('#tbModuleBOM').jqGrid('getRowData', rowId);

    var subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/BOM/GetMBOM',
        postData: {
            styleCode: rowData.StyleCode, styleSize: rowData.StyleSize, styleColorSerial: rowData.StyleColorSerial, revNo: rowData.RevNo, moduleId: mdlData.ModuleId
        },
        datatype: "json",
        width: null,
        shrinkToFit: false,
        colModel: [
            { name: 'MainItemCode', index: 'MainItemCode', label: "Main Item", width: 150, classes: 'pointer', hidden: true },
            { name: 'MainItemName', index: 'MainItemName', label: "Main Item Name", width: 150, classes: 'pointer', hidden: true },
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer' },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 150, classes: 'pointer' },
            { name: 'ItemColor', index: 'ItemColor', label: "Item Color", width: 150, classes: 'pointer' },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", align: 'center', classes: 'pointer' },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Consumption Unit" },
            { name: 'Qty', index: 'Qty', label: "Qty", width: 50, align: 'center', classes: 'pointer' },
            { name: 'StdPrice', index: 'StdPrice', label: "Standard Price", width: 100, align: 'center', classes: 'pointer' },
            { name: 'CurrCode', index: 'CurrCode', label: "Currency", width: 70, align: 'center', classes: 'pointer' },
            { name: 'RegistryDate', index: 'RegistryDate', label: "Registry Date", width: 100, align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" } },
            { name: 'RegisterName', index: 'RegisterName', label: "Register Name", width: 100, align: 'center', classes: 'pointer' },
            { name: 'PatternCons', index: 'PatternCons', label: "Pattern Cons", width: 100, align: 'center', classes: 'pointer' },
            { name: 'PolyCons', index: 'PolyCons', label: "Poly Consumption", width: 120, align: 'center', classes: 'pointer' },
            { name: 'MarkerCons', index: 'MarkerCons', label: "Auto Cons", width: 80, align: 'center', classes: 'pointer' },
            { name: 'MarkerConsUnit', index: 'MARKERCONSUNIT', label: "Auto Cons Unit", width: 100, align: 'center', classes: 'pointer' },
            { name: 'Cad_Material', index: 'Cad_Material', label: "Cad Material", width: 100, align: 'center', classes: 'pointer' },
            { name: 'GenName', index: 'GenName', label: "Gen Name", width: 70, align: 'center', classes: 'pointer' },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }
        ],
        rowNum: 1000,
        height: 200,
        subGrid: true,
        subGridRowExpanded: subGridviewMBOMPattern,
        gridComplete: function() {
            //tbModuleBOM_5_t
            let ids = jQuery("#" + subgridTableId).jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#" + subgridTableId).jqGrid("getRowData", i);
               
                if (rowdata.HasPattern !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=" + subgridTableId +"_subgrid]").html("&nbsp;");
                }
            }
        }
    });

    $("#tbModuleBOM").parents('div.ui-jqgrid-bdiv').css("max-height", "300px");
}

function subGridviewMBOMPattern(subgridDivId, rowId) {
    //Remove the last character from sub grid div Id
    let position = subgridDivId.lastIndexOf("_");
    let paSubGridId = "#" + subgridDivId.substr(0, position);
    var rowData = $(paSubGridId).jqGrid('getRowData', rowId);

    var subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/BOM/GetPatterns',
        postData: {
            styleCode: rowData.StyleCode, styleSize: rowData.StyleSize, styleColorSerial: rowData.StyleColorSerial, revNo: rowData.RevNo, itemCode: rowData.ItemCode, itemColorSerial: rowData.ItemColorSerial
        },
        datatype: "json",
        width: null,
        shrinkToFit: false,
        colModel: [
            { name: '', index: '', label: "", width: 30, classes: 'pointer', formatter: patternImage },
            { name: 'Piece', index: 'Piece', label: "Description", width: 250, classes: 'pointer' },
            { name: 'Height', index: 'Height', label: "Height", width: 50, classes: 'pointer' },
            { name: 'Width', index: 'ItemCode', label: "Width", width: 50, classes: 'pointer' },
            { name: '', index: '', label: "Pattern Area", width: 100, classes: 'pointer' },
            { name: 'Area', index: 'Area', label: "Polygon Area", width: 50, classes: 'pointer' },
            { name: 'MainPartName', index: 'MainPartName', label: "Main Part", width: 150, align: 'center', classes: 'pointer' },
            { name: 'SizeUnit', index: 'SizeUnit', label: "Size Unit", width: 70 },
            { name: 'EndWise', index: 'EndWise', label: "End Wise", width: 70, align: 'center', classes: 'pointer' },
            { name: 'PieceQty', index: 'PieceQty', label: "Piece Qty", width: 70, align: 'center', classes: 'pointer' },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Unit Cons", width: 70, align: 'center', classes: 'pointer' },
            { name: 'CurrCode', index: 'CurrCode', label: "Currentcy", width: 70, align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" } },
            { name: 'PieceUnique', index: 'PieceUnique', label: "Unique", width: 70, align: 'center', classes: 'pointer' },
            { name: 'PatternSerial', index: 'PatternSerial', label: "Serial", width: 50, align: 'center', classes: 'pointer' },
            { name: 'Url', index: 'Url', hidden: true }
        ],
        rowNum: 1000,
        height: '100%'        
    });
}
//#endregion