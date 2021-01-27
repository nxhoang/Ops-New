//#region varialbles
var tbDefectCodeId = "#tbDefectCode";
var paperDefectCodeId = "#divDefectCodePage";

const _languageId = {
    Vietnamese: 'vn',
    Bahasa: 'id',
    Burmese: 'mm',
    Amharic: 'et'
};
//#endregion


function InitPage() {
    bindDataToJqGridDefectCode(null);
    GetMasterCodes("drpDefectCat", "DefectCat", null);
    selectedCategory();

    Selection2("drpLanguage");
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
            { name: 'DefectCode', index: 'DefectCode', width: 50, label: "Defect Code" },
            { name: 'DefectDesc', index: 'DefectDesc', label: "Defect Description" },
            { name: 'DefectCat', index: 'DefectCat', hidden: true },
            { name: 'Vietnamese', index: 'Vietnamese' },
            { name: 'Bahasa', index: 'Bahasa', hidden: true },
            { name: 'Burmese', index: 'Burmese', hidden: true },
            { name: 'Amharic', index: 'Amharic', hidden: true },
            { name: 'HasBuyerDefect', index: 'HasBuyerDefect', hidden: true }
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
            let ids = jQuery(tbDefectCodeId).jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $(tbDefectCodeId).jqGrid("getRowData", i);
                if (rowdata.HasBuyerDefect !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbDefectCode_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbDefectCode_subgrid]").unbind('click');

                }
            }
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
        autowidth: true,
        subGrid: true,
        subGridRowExpanded: subGridviewBuyerDefect 
    });

    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    $(window).on('resize.jqGrid', function () {
        //$(tableGroupPackageId).jqGrid('setGridWidth', $("#content").width() - 32); //well: 19, col:13, content: 10
        $(tbDefectCodeId).jqGrid('setGridWidth', $("#content").width());
    });

}

function resizeDefectCodeJqgrid() {
    $(tbDefectCodeId).jqGrid('setGridWidth', $("#content").width());
}

function subGridviewBuyerDefect(subgridDivId, rowId) {

    var rowData = $(tbDefectCodeId).jqGrid('getRowData', rowId);
    var subgridTableId = subgridDivId + "_t";

    //Declare pager id
    let pager_id = "p_" + subgridTableId;

    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table><div id='" + pager_id + "' class='scroll'></div>");
    $("#" + subgridTableId).jqGrid({
        url: '/DefectCode/GetBuyerDefect',
        postData: { pkDefectCode: rowData.DefectCode },
        datatype: "json",
        width: null,
        shrinkToFit: false,
        colModel: [
            { name: 'BUYER', index: 'BUYER', label: "BUYER", width: 100, align: "center" },
            { name: 'BUYERNAME', index: 'BUYERNAME', label: "BUYER NAME", width: 250},
            { name: 'BUYERDEFECTCODE', index: 'BUYERDEFECTCODE', label: "DEFECT CODE", width: 150 },
            { name: 'BUYERDEFECTDESC', index: 'BUYERDEFECTDESC', label: "DEFECT DESC", width: 350 }
        ],
        rowNum: 1000,
        height: '100%',
        rowNum: 10,
        rowList: [10, 20, 30],
        pager: pager_id
    });
}
//#endregion