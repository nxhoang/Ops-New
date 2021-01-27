//#region varialbles
var tbDefectCodeId = "#tbDefectCode";
var paperDefectCodeId = "#divDefectCodePage";
//#endregion


function InitPage() {
    bindDataToJqGridDefectCode(null);
    GetMasterCodes("drpDefectCat", "DefectCat", null);
    selectedCategory();
}

//#region bin data to gridview
//Bind data to grid defect code
function bindDataToJqGridDefectCode(defectCat) {

    jQuery(tbDefectCodeId).jqGrid({
        url: '/DefectCode/GetListDefectCodeByCat',
        postData: {
            defectCat: defectCat
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'DefectCode', index: 'DefectCode', width: 50, label: "Defect Code", classes: 'pointer' },
            { name: 'DefectDesc', index: 'DefectDesc', label: "Defect Description", classes: 'pointer' },
            { name: 'DefectCat', index: 'DefectCat', hidden: true }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        pager: paperDefectCodeId,
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {

        },
        loaderror: function (xhr, status, err) {
            alert("error - get list of defect code: " + err);
        },
        beforeSelectRow: function (rowid, e) {
        },
        onSelectRow: function (rowid) {
        },
        loadcomplete: function () {

        },
        autowidth: true
    });

    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    $(window).on('resize.jqGrid', function () {
        //$(tableGroupPackageId).jqGrid('setGridWidth', $("#content").width() - 32); //well: 19, col:13, content: 10
        $(tableGroupPackageId).jqGrid('setGridWidth', $("#content").width());
    });
        
}
//#endregion