//#region variable
var tableMesPackageId = "tbMesPackage";
var tableWIPId = "tbWIP";

var WIPTYPE = {
    Module: "1",
    OpGroup: "2"
};

//#endregion

function initPageWIP() {
    //Init partial view package group
    initPackageGroupPartialView();

    //Init mes gridview
    bindDataToJqGridMesPackage(null);

    //Init work in process gridview.
    binDatatoJqGridWIP(null);

    //Init event click on button in WIP
    eventClickButtonWIP();
}

//#region redefine function

//Event click on package group row.
function eventSelectedRowOnPackageGroupGrid(rowdata) {

    //Reload production package and mes package
    var params = { packageGroup: rowdata.PackageGroup };
    ReloadJqGrid2LoCal(tableMesPackageId, params);

    //Clear work in process gridview
    ReloadJqGrid2LoCal(tableWIPId, null);
}

function SelectedRowPackageGroup() {

}

function BeforeSelectRowPackageGv() {
    return true;
}

//Redifine function search package group
function eventSearchPackageGroup() {
    //factoryId, startDate, endDate, buyer, buyerInfo, aoNo

    //Clear mes package gridview
    ReloadJqGrid2LoCal(tableMesPackageId, null);

    //Clear work in process gridview
    ReloadJqGrid2LoCal(tableWIPId, null);
}

//#endregion

//#region bind data to gridview
function binDatatoJqGridWIP(mesPkg, getType) {
    jQuery("#" + tableWIPId).jqGrid({
        url: '/WIP/GetWorkInProcessByMesPkg',
        postData: {
            mesPkg: mesPkg, getType: getType
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'IoTType', index: 'IoTType', label: "Assembly",  align: 'center', classes: 'pointer' },
            { name: 'MxPackage', index: 'MxPackage', label: "MxPackage",  align: 'center', classes: 'pointer', hidden: true },
            { name: 'Factory', index: 'Factory', label: "Factory", align: 'center', classes: 'pointer' },
            { name: 'ModuleName', index: 'ModuleName', label: "Module Name",  align: 'center', classes: 'pointer' },
            { name: 'ModuleId', index: 'ModuleId', label: "ModuleId",  classes: 'pointer' },
            { name: 'OpGroupName', index: 'OpGroupName', label: "Op Group", classes: 'pointer'}, 
            { name: 'MxTarget', index: 'MxTarget', label: "Target", classes: 'pointer'},
            { name: 'Issued', index: 'Issued', label: "ModuleId", classes: 'pointer' },
            { name: 'TotalMade', index: 'TotalMade', label: "Total Made", classes: 'pointer' },
            { name: 'Balance', index: 'Balance', label: "Balance", classes: 'pointer' },
            { name: 'Shipped', index: 'Shipped', label: "Shipped", classes: 'pointer' }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {},
        loadComplete: function () {},
        onSelectRow: function () {},
        autowidth: true
        //width: null,
        //shrinkToFit: false,
        //Footer
        //footerrow: true,
        //useDataOnFooter: true
    });

    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    $(window).on('resize.jqGrid', function () {
        $("#" + tableWIPId).jqGrid('setGridWidth', $("#wid-id-1.widget-body").width() );
    });
}

function bindDataToJqGridMesPackage(packageGroup) {

    jQuery("#" + tableMesPackageId).jqGrid({
        url: '/MesManagement/GetMesPackages',
        postData: {
            packageGroup: packageGroup
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'LineName', index: 'LineName', label: "MES Line", width: 100, align: 'center', classes: 'pointer' },
            { name: 'LineNo', index: 'LineNo', label: "Line No", width: 70, align: 'center', classes: 'pointer', hidden: true },
            { name: 'MX_IOT_Completed', index: 'MX_IOT_Completed', label: "Completed Qty", width: 100, align: 'center', classes: 'pointer' },
            { name: 'MxTarget', index: 'MxTarget', label: "Target Qty", width: 70, align: 'center', classes: 'pointer' },
            { name: 'MxPackage', index: 'MxPackage', label: "MES P. Packages", width: 270, classes: 'pointer' },
            { name: 'PlnActStartDate', index: 'StartPlan', label: "Actual Start Plan", formatter: "date", formatoptions: { srcformat: "d-m-Y", newformat: "d-m-Y" } }, //d-m-Y H:i:s
            { name: 'PackageGroup', index: 'PackageGroup', hidden: true },
            { name: 'SeqNo', index: 'SeqNo', hidden: true },
            { name: 'MxStatus', index: 'MxStatus', hidden: true },
            { name: 'LineSerial', index: 'LineSerial', hidden: true }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        //pager: paperMesPackageId,
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {

        },
        loadComplete: function () {
            var grid = $("#" + tableMesPackageId);
            var colSum = grid.jqGrid('getCol', 'MxTarget', false, 'sum');
            //grid.jqGrid('footerData', 'set', { MX_IOT_Completed: "Total", MxTarget: colSum });

            var colSumComplete = grid.jqGrid('getCol', 'MX_IOT_Completed', false, 'sum');
            grid.jqGrid('footerData', 'set', { LineName: "Total", MX_IOT_Completed: colSumComplete, MxTarget: colSum });
        },
        onSelectRow: function (rowid) {
            var rowdata = $("#" + tableMesPackageId).jqGrid("getRowData", rowid);

            //Get work in process
            let getType = $("#rdModuleWIP").is(':checked') === true ? WIPTYPE.Module : WIPTYPE.OpGroup;

            //Reload work in process by mes package
            var params = { mesPkg: rowdata.MxPackage, getType: getType };
            ReloadJqGrid2LoCal(tableWIPId, params);
        },
        autowidth: true,
        //width: null,
        //shrinkToFit: false,
        //Footer
        footerrow: true,
        useDataOnFooter: true
    });

    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    $(window).on('resize.jqGrid', function () {
        $("#" + tableMesPackageId).jqGrid('setGridWidth', $("#content").width());
    });
}
//#endregion