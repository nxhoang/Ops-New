//#region Variable

var tableGroupPackageId = "#tbGroupPackage";
var tableGroupPackageName = "tbGroupPackage";
var paperGroupPackageId = "#divGroupPackagePage";
var paperGroupPackageName = "divGroupPackagePage";

//#endregion

//Init functions for package group partial view.
function initPackageGroupPartialView() {
    InitDateRange();

    //Get list of factory
    GetFactories("drpFactory", null);

    //Get list of Buyer
    GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);

    //Init grid execution package
    bindDataToJqGridGroupPackage(null, null, null, null, null, null, null);

    //Event click button search package group
    eventClickBtnSearchPkgGroup();

    Selection2("drpFilterBy");
        
}

//Init date range
function InitDateRange() {
    var origStyle = "";
    $('#txtDateRange').daterangepicker(
        {
			/*singleDatePicker: true, 
			"minDate": new Date(today),*/
            showDropdowns: true,
            "setDate": new Date(),
            "autoclose": true,
            locale: {
                format: 'YYYY/MM/DD'
            },
            autoApply: true
        }
    ).on('show.daterangepicker', function (e) {
        origStyle = $($('#txtDateRange').data('daterangepicker').container[0]).attr('style');
        let neworigStyle = origStyle + "opacity: 1 ; transform : scale(1);";
        $($('#txtDateRange').data('daterangepicker').container[0]).removeAttr('style');
        $($('#txtDateRange').data('daterangepicker').container[0]).attr('style', neworigStyle);
    }).on('hide.daterangepicker', function (e) {
        $($('#txtDateRange').data('daterangepicker').container[0]).removeAttr('style');
        let neworigStyle = "";
        let arr_neworigStyle = origStyle.split(';');
        for (var i = 0; i < arr_neworigStyle.length; i++) {
            let current = arr_neworigStyle[i];
            if (current.length > 0) {
                if (current.includes('display')) {
                    neworigStyle += "display:none;";
                } else {
                    neworigStyle += current + ';';
                }
            }
        }
        $($('#txtDateRange').data('daterangepicker').container[0]).attr('style', neworigStyle);
    });
    $('#txtDateRange').data('daterangepicker').setStartDate(getCurrentDate(0));
    $('#txtDateRange').data('daterangepicker').setEndDate(getCurrentDate(20));

}

//Bind data to grid group packages
function bindDataToJqGridGroupPackage(factory, plnStartDate, plnEndDate, buyer, buyerInfo, AONo, filterStartDate) {

    jQuery(tableGroupPackageId).jqGrid({
        url: '/MesManagement/GetGroupPackages',
        postData: {
            factory: factory, plnStartDate: plnStartDate, plnEndDate: plnEndDate, buyer: buyer, buyerInfo: buyerInfo, AONo: AONo, filterStartDate: filterStartDate
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { width: 160, label: "Package Group", title: false, formatter: formatPackageGroup },/*2020-08-06 Tai Le(Thomas): add {title: false} */
            { name: 'MATReadiness', index: 'MATReadiness', width: 70, label: "MAT. Readiness", classes: 'pointer' , align: 'right' },/*2020-08-06 Tai Le(Thomas): Material Readiness(%)*/
            { name: 'FactoryName', index: 'FactoryName', width: 100, label: "Factory", classes: 'pointer', align: 'center' },
            { name: 'AONo', index: 'AONo', width: 120, label: "AO No", align: 'center', classes: 'pointer' },
            { name: 'BuyerStyleCode', index: 'BuyerStyleCode', width: 130, label: "Buyer Code", align: 'left', classes: 'pointer' },
            { name: 'StyleCode', index: 'StyleCode', width: 100, label: "Style Code", align: 'center', classes: 'pointer' },
            { name: 'StyleName', index: 'StyleName', width: 100, label: "Style Name", align: 'left', classes: 'pointer' },
            { name: 'StyleSize', index: 'StyleSize', width: 80, label: "Style Size", align: 'center', classes: 'pointer' },
            { name: 'StyleColorways', index: 'StyleColorways', width: 150, label: "Color", align: 'left', classes: 'pointer' },
            { name: 'RevNo', index: 'RevNo', width: 80, label: "Rev No", align: 'center', classes: 'pointer' },
            { name: 'TargetQty', index: 'TargetQty', width: 80, label: "Target", align: 'center', classes: 'pointer' },
            { name: 'TotalMadeQty', index: 'TotalMadeQty', width: 80, label: "Completed", align: 'center', classes: 'pointer' },
            //{ name: 'MadeQty', index: 'MadeQty', width: 100, label: "Completed", align: 'center', classes: 'pointer' },
            { name: 'StatusName', index: 'StatusName', width: 100, label: "Status", align: 'center', classes: 'pointer' },
            { name: 'MesPlnStartDate', index: 'MesPlnStartDate', width: 110, label: "Plan Start", align: 'center' },
            { name: 'MesPlnEndDate', index: 'MesPlnEndDate', width: 110, label: "Plan Finish", align: 'center' },
            { name: 'Complete', width: 100, label: "Complete", align: 'center', formatter: formatCompletePackageGroup, classes: 'pointer', hidden: true },
            //{ name: 'PackageGroup', index: 'PackageGroup', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'PackageGroup', index: 'PackageGroup', hidden: true },
            { name: 'Status', index: 'Status', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'MesFactory', index: 'MesFactory', hidden: true },
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        pager: paperGroupPackageId,
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {
            
        },
        loaderror: function (xhr, status, err) {
            alert("error - get group package: " + err);
        },
        beforeSelectRow: function (rowid, e) {
            return BeforeSelectRowPackageGv(rowid, e);
        },
        onSelectRow: function (rowid) {
            const dataRow = $(tableGroupPackageId).jqGrid("getRowData", rowid);

            //let chkMesPkg = $("#chkMESPackage").is(":checked");

            //Reload MES package 
            //eventSelectedRowOnPackageGroupGrid(dataRow, chkMesPkg);
            eventSelectedRowOnPackageGroupGrid(dataRow);

            SelectedRowPackageGroup(dataRow);

            window.CurrentStyle = {
                StyleCode: dataRow.StyleCode,
                StyleColorSerial: dataRow.StyleColorSerial,
                StyleSize: dataRow.StyleSize,
                RevNo: dataRow.RevNo
            };
            window.CurrentMpmt = dataRow;
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

    function formatCompletePackageGroup(cellvalue, options, rowObject) {
       
        let pkgGroupStatus = rowObject.Status;
        let strButtonColor = "";
        let disabled = "";
        if (pkgGroupStatus === "CL") {
            strButtonColor = "btn btn-success";
            disabled = "disabled";
        } else {
            strButtonColor = "btn btn-primary";
            disabled = "";
        }

        return "<button type='button' class='" + strButtonColor + "' " + disabled + " onclick='CompletePackageGroup(" + JSON.stringify(rowObject) + ")'> Complete </button>";

        //<i class='glyphicon glyphicon-lock'></i>
    }

    function formatPackageGroup(cellvalue, options, rowObject) {
        return '<a href="#" class="qtip-pkgGroup" data-role="quickSummary" data-summaryurl="/Planning/GetStyleSummary/?PackageGroup=' + rowObject.PackageGroup + '" >' + rowObject.PackageGroup + '</a>';
    }
}

function filterMESPackage(gridMesPkgId) {

    let filVal = $("#drpFilterBy").val();

    if (filVal === "2") {
        let arrDateRange = $("#txtDateRange").val().split('-');
        let startDate = $.trim(arrDateRange[0].replace(new RegExp('/', 'g'), ''));

        //Get array id of gridview
        let arrIds = $("#" + gridMesPkgId).jqGrid('getDataIDs');
        $.each(arrIds, function (idx, rowId) {
            let rData = $("#" + gridMesPkgId).jqGrid('getRowData', rowId);
            let plnStartDate = rData.PlnStartDate.replace(new RegExp('-', 'g'), '');
            if (plnStartDate < startDate) {
                $("#" + gridMesPkgId).jqGrid('delRowData', rowId);
            }
        });
    }
}