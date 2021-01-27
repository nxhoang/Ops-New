//#region Variable
var tableMesPackageId = "#tbMesPackage";
var tableMesPackageName = "tbMesPackage";
var paperMesPackageId = "#divMesPackagePage";
var paperMesPackageName = "divMesPackagePage";
//#endregion

//Redifine event selected row
function eventSelectedRowOnPackageGroupGrid(rowData) {
    var packageGroup = rowData.PackageGroup;

    //Reload production package and mes package
    var params = { packageGroup: packageGroup };

    ReloadJqGrid2LoCal(tableMesPackageName, params);
}

//Redifine function search package group
function eventSearchPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo) {

}

//#region bind data to gridview

//function OpsMasterFunction(row) {}

//Bind data to grid mes package
function bindDataToJqGridMesPackage(packageGroup) {
    jQuery(tableMesPackageId).jqGrid({
        url: '/MesManagement/GetMesPackages',
        postData: {
            packageGroup: packageGroup
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'MxPackage', index: 'MxPackage', label: "MES P. Packages", width: 200, classes: 'pointer' },
            { name: 'StatusName', index: 'StatusName', label: "Status", width: 30, align: 'center', classes: 'pointer' },
            { name: 'MxStatus', index: 'MxStatus', hidden: true },
            { name: 'LineNo', index: 'LineNo', label: "Line No", width: 70, align: 'center', classes: 'pointer' , hidden: true },
            { name: 'LineName', index: 'LineName', label: "MES Line", width: 70, align: 'center', classes: 'pointer' },
            //{ name: 'StartPlan', index: 'StartPlan', label: "Start Plan", formatter: "date", formatoptions: { srcformat: "d-m-Y H:i:s", newformat: "d-m-Y H:i:s" } },
            { name: 'PlnStartDate', index: 'PlnStartDate', label: "Start Plan"}, //ADD) SON - 6/Jul/2019 - add start plan date
            { name: 'MxTarget', index: 'MxTarget', label: "Target Qty", width: 70, align: 'center', classes: 'pointer' },
            { name: 'PackageGroup', index: 'PackageGroup', hidden: true },
            { name: 'SeqNo', index: 'SeqNo', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'LineSerial', index: 'LineSerial', hidden: true }
        ],
        rowNum: 1000,
        //rowList: [10, 20, 30],
        //pager: paperMesPackageId,
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {

        },
        loadComplete: function () {

            //Filter mes package by plan start date
            filterMESPackage("tbMesPackage");

            var grid = $(tableMesPackageId);
            var colSum = grid.jqGrid('getCol', 'MxTarget', false, 'sum');
            grid.jqGrid('footerData', 'set', { MxPackage: "Total", MxTarget: colSum });
        },
        beforeSelectRow: function (rowid, e) {
            return BeforeSelectRowPackageGv(rowid, e);
        },
        onSelectRow: function (rowid) {
            //Get row data
            const dataRow = $(tableMesPackageId).jqGrid("getRowData", rowid);
            console.log("Selected package");

            SelectedRowMesPackageGv(dataRow);
        },
        autowidth: true,
        //width: null,
        //shrinkToFit: false,
        //scroll: false,

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
        $(tableMesPackageId).jqGrid('setGridWidth', $("#divMesPackage").width());
    });
}
//#endregion