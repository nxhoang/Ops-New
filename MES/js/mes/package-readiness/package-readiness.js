
var JIG_STATUS = {
    Ready: '1',
    Requested: '2',
    Processing: '3',
    Cancel: '4'
};

function initPackageReadinessPage() {
    
    SelectDateRangePicker('#txtDateRange');

    //Get list of factory
    GetFactories("drpFactory", null);

    //Get list of Buyer
    GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);

    //Set factory base on factory of user role.
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    $("#drpBuyer").val('OSP').trigger('change');

    //Init gridview Mtop package
    bindDataToJqGridMtopPackage(null, null, null, null, null, null, null);

    eventClickOnButton();
}

function getPackageReadiness(prdPkg, callBack) {
    var config = ObjectConfigAjaxPost(
        "../PackageReadiness/GetPackageReadiness"
        , false
        , JSON.stringify({
            prdPkg: prdPkg
        })
    );
    AjaxPostCommon(config, function (resData) {
        let pkgReadiness = null;
        if (resData.IsSuccess) {
            pkgReadiness = resData.Result;
        } 

        callBack(pkgReadiness);

    });
}

//Set package readiness status on screen
function setPkgReadinessStatus(pkgReadiness) {
    if (pkgReadiness !== null) {
        if (pkgReadiness.MOULD === '1') {
            $('#lblMould').text('Ready');
        } else {
            $('#lblMould').text('Not Ready');
        }

        if (pkgReadiness.SOP === '1') {
            $('#lblSop').text('Ready');
        } else {
            $('#lblSop').text('Not Ready');
        }

        //Disable button send Jig request
        $('#btnSendRequestJig').prop('disabled', true);

        switch (pkgReadiness.JIG) {
            case JIG_STATUS.Ready:
                $('#lblJig').text('Ready');
                break;
            case JIG_STATUS.Requested:
                $('#lblJig').text('Requested');
                break;
            case JIG_STATUS.Processing:
                $('#lblJig').text('Processing');
                break;
            case JIG_STATUS.Cancel:
                $('#lblJig').text('Cancel');
                break;
            default:
                $('#lblJig').text('Not Ready');
                $('#btnSendRequestJig').prop('disabled', false);
        }
    } else {
        $('#lblJig').text('Not Ready');
        $('#lblMould').text('Not Ready');
        $('#lblSop').text('Not Ready');

        //Clear data on Jig request modal
        clearJigRequestModal();

        //Enable button send jig request
        $('#btnSendRequestJig').prop('disabled', false);
    }
}

function checkJigRequestData() {
    if (isEmpty($('#lblRequestJigId').text())) {
        ShowMessage("Send Jig request", 'Jig request Id cannot be empty', ObjMessageType.Info);
        return false;
    }

    if (isEmpty($('#txtJigCode').val())) {
        ShowMessage("Send Jig request", 'Jig code cannot be empty', ObjMessageType.Info);
        return false;
    }

    if (isEmpty($('#txtAO').val())) {
        ShowMessage("Send Jig request", 'AO cannot be empty', ObjMessageType.Info);
        return false;
    }

    if (isEmpty($('#txtStyleCode').val())) {
        ShowMessage("Send Jig request", 'Style code cannot be empty', ObjMessageType.Info);
        return false;
    }

    if (isEmpty($('#txtJigQty').text())) {
        ShowMessage("Send Jig request", 'Jig quantity cannot be empty', ObjMessageType.Info);
        return false;
    }

    return true;
}

function clearJigRequestModal() {
    $('#lblMtopPkgMdl').text('');
    $('#lblRequestJigId').text('');
    $('#txtJigCode').val('');
    $('#txtJigName').val('');
    $('#txtAO').val('');
    $('#txtAOQty').val('');
    $('#txtStyleCode').val('');
    $('#txtJigQty').val('');
}

//#region bind data to grid mtop package
function bindDataToJqGridMtopPackage(factory, plnStartDate, plnEndDate, buyer, styleInfo, aoNo) {
    
    jQuery('#tbMtopPackage').jqGrid({
        url: '/Planning/GetProductionPackage',
        postData: {
            factoryId: factory, startDate: plnStartDate, endDate: plnEndDate, buyer: buyer, styleInfo: styleInfo, aoNo: aoNo
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'PrdPkg', index: 'PrdPkg', width: 170, label: "Package" },
            { name: 'AoNo', index: 'AoNo', width: 100, label: "AO", align: 'center' },
            { name: 'StyleName', index: 'StyleName', width: 150, label: "Style Name" },
            { name: 'StyleSize', index: 'StyleSize', width: 50, label: "Style Size", align: 'center' },
            { name: 'StyleColorways', index: 'StyleColorways', width: 120, label: "Color" },
            { name: 'RevNo', index: 'RevNo', width: 50, label: "Rev No", align: 'center' },
            { name: 'PlanQty', index: 'PlanQty', width: 70, label: "Plan Qty", align: 'center' },
            { name: 'PrdSdat', index: 'PrdSdat', width: 70, label: "Start Date", align: 'center' },
            { name: 'PrdEdat', index: 'PrdEdat', width: 70, label: "End Date", align: 'center' },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'LineNo', index: 'LineNo', width: 70, label: "Line No", hidden: true }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        pager: '#pgMtopPackage',
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {

        },
        loaderror: function (xhr, status, err) {
            alert("error - get production package: " + err);
        },
        onSelectRow: function (rowid) {
            const mtopPkgData = $('#tbMtopPackage').jqGrid("getRowData", rowid);
            getPackageReadiness(mtopPkgData.PrdPkg, (pkgReadiness) => {
                setPkgReadinessStatus(pkgReadiness);
            });
        },
        loadcomplete: function () {

        },
        //width: null,
        //shrinkToFit: false
        autowidth: true
    });

    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    $(window).on('resize.jqGrid', function () {

        $('#tbMtopPackage').jqGrid('setGridWidth', $("#content").width());
    });

}

function reloadGridMtopPackage(factory, plnStartDate, plnEndDate, buyer, buyerInfo, aoNo) {

    var params = { factoryId: factory, startDate: plnStartDate, endDate: plnEndDate, buyer: buyer, buyerInfo: buyerInfo, aoNo: aoNo };
    ReloadJqGrid2LoCal('tbMtopPackage', params);
}

//#endregion