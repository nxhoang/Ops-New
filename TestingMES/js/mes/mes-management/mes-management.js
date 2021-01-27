function InitPage() {

    //Init functions in partial view
    initPackageGroupPartialView();

    //Init grid mes package
    bindDataToJqGridMesPackage(null);

    //Init grid production package
    bindDataToJqGridProPackage(null);

    //Event check list readiness
    eventClickButtonCheckListReadiness();

    //Event reset or start mes package
    eventResetStartProductionReadiness();

    //Set factory base on factory of user role.
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    $(tableGroupPackageId).showCol("Complete");

    //Get MES line, module and process group for MES package line detail
    getProcessGroups();
    GetLineByFactoryId("drpLineDt", $("#hdFactoryUser").val());

    eventLineDetail();

    initProductionDate("txtProDateLineDt");

    //Init gridview line detail
    bindDataToJqGridLineDetail(null);
    //testPage();

}

function testPage() {

    $('#txtDateRange').data('daterangepicker').setStartDate("2015/07/05");
    $('#txtDateRange').data('daterangepicker').setEndDate("2015/07/30");

    $("#btnSearchExePackage").trigger("click");
    //$("#btnSearchExePackage").click(); 
}

//#region Redifine functions
function SelectedRowPackageGroup(dataRow) {
    //Get module by style code
    GetModulesByStyleCodeMySql("drpModuleLineDt", dataRow.StyleCode);
}

function BeforeSelectRowPackageGv(rowid, e) {
    return true;
}

//Redifine function search package group
function eventSearchPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo) {

}

//#endregion

//#region Variable

//var tableGroupPackageId = "#tbGroupPackage";
//var tableGroupPackageName = "tbGroupPackage";
//var paperGroupPackageId = "#divGroupPackagePage";
//var paperGroupPackageName = "divGroupPackagePage";

var tableMesPackageId = "#tbMesPackage";
var tableMesPackageName = "tbMesPackage";
var paperMesPackageId = "#divMesPackagePage";
var paperMesPackageName = "divMesPackagePage";

var tablePPackageId = "#tbPPackage";
var tablePPackageName = "tbPPackage";
var paperPPackageId = "#divPPackagePage";
var paperPPackageName = "divPPackagePage";

var tableLineDetailId = "tbLineDetail";

var ArrExecPackName = {
    FACTORY: "Factory",
    AONO: "AONO",
    STYLE: "Style",
    TARGET: "Target",
    COMPLETED: "Completed",
    STATUS: "Status",
    PLANNEDSTART: "Planned Start",
    PLANNEDFINISHIING: "Planned Finishing"
};

var ProductionReadiness = {
    MESOP: "001",
    BOMPattern: "002",
    PPMeeting: "003",
    MaterialReadiness: "004",
    CuttingStatus: "005",
    Treatment: "006",
    Tools: "007",
    Machines: "008",
    JIG: "009",
    LineSetup: "010"
};

//#endregion

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
    $('#txtDateRange').data('daterangepicker').setEndDate(getCurrentDate(10));
}

//#region Bindata to gridview packages

////Bind data to grid group packages
//function bindDataToJqGridGroupPackage(factory, plnStartDate, plnEndDate) {

//    jQuery(tableGroupPackageId).jqGrid({
//        url: '/MesManagement/GetGroupPackages',
//        postData: {
//            factory: factory, plnStartDate: plnStartDate, plnEndDate: plnEndDate
//        },
//        datatype: "json",
//        height: 'auto',
//        colModel: [
//            { name: 'MesFactory', index: 'MesFactory', width: 120, label: ArrExecPackName.FACTORY, classes: 'pointer' },
//            { name: 'AoNo', index: 'AoNo', width: 120, label: ArrExecPackName.AONO, align: 'center', classes: 'pointer' },
//            { name: 'StyleCode', index: 'StyleCode', width: 100, label: ArrExecPackName.STYLE, align: 'center', classes: 'pointer' },
//            { name: 'TargetQty', index: 'TargetQty', width: 150, label: ArrExecPackName.TARGET, align: 'center', classes: 'pointer' },
//            { name: 'MadeQty', index: 'MadeQty', width: 150, label: ArrExecPackName.COMPLETED, align: 'center', classes: 'pointer' },
//            { name: 'Status', index: 'Status', width: 150, label: ArrExecPackName.STATUS, align: 'center', classes: 'pointer' },
//            { name: 'MesPlnStartDate', index: 'MesPlnStartDate', width: 200, label: ArrExecPackName.PLANNEDSTART, align: 'center' },
//            { name: 'MesPlnEndDate', index: 'MesPlnEndDate', width: 200, label: ArrExecPackName.PLANNEDFINISHIING, align: 'center' },
//            { name: 'PackageGroup', index: 'PackageGroup', hidden: true }
//        ],
//        rowNum: 10,
//        rowList: [10, 20, 30],
//        pager: paperMesPackageId,
//        sortname: 'id',
//        toolbarfilter: true,
//        viewrecords: true,
//        sortorder: "asc",
//        loadonce: true,
//        gridComplete: function () {

//        },
//        loaderror: function (xhr, status, err) {
//            alert("error - get group package: " + err);
//        },
//        onSelectRow: function (rowid) {
//            var rowdata = $(tableGroupPackageId).jqGrid("getRowData", rowid);
//            var packageGroup = rowdata.PackageGroup;

//            //Reload production package and mes package
//            var params = { packageGroup: packageGroup };
//            ReloadJqGrid2LoCal(tablePPackageName, params);

//            ReloadJqGrid2LoCal(tableMesPackageName, params);

//            //Check package group status
//            if (rowdata.Status === "G") {
//                //If status is going on then disable check list and reset buttons production readiness, 
//                //button start package group
//                DisableButtonsProductionReadiness(true);

//                DisabledButton("btnResetProReadiness", true);
//                DisabledButton("btnStartExecution", true);
//            } else {
//                DisableButtonsProductionReadiness(false);

//                DisabledButton("btnResetProReadiness", false);
//                DisabledButton("btnStartExecution", false);
//            }

//        },
//        loadcomplete: function () {

//        },
//        autowidth: true,

//    });


//    /* Add tooltips */
//    $('.navtable .ui-pg-button').tooltip({
//        container: 'body'
//    });

//    //Custom jqgrid css
//    customJqGridCss();

//    $(window).on('resize.jqGrid', function () {
//        $(tableGroupPackageId).jqGrid('setGridWidth', $("#content").width() - 42); //well: 19, col:13, content: 10
//    });
//}

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
            { name: 'LineName', index: 'LineName', label: "MES Line", width: 100, align: 'center', classes: 'pointer' },
            { name: 'LineNo', index: 'LineNo', label: "Line No", width: 70, align: 'center', classes: 'pointer', hidden: true },
            { name: 'MX_IOT_Completed', index: 'MX_IOT_Completed', label: "Completed Qty", width: 100, align: 'center', classes: 'pointer' },
            { name: 'MxTarget', index: 'MxTarget', label: "Target Qty", width: 70, align: 'center', classes: 'pointer' },
            { name: 'MxPackage', index: 'MxPackage', label: "MES P. Packages", width: 270, classes: 'pointer' },
            { name: 'PlnActStartDate', index: 'StartPlan', label: "Actual Start Plan", formatter: "date", formatoptions: { srcformat: "d-m-Y", newformat: "d-m-Y" } }, //d-m-Y H:i:s
            { name: 'Complete', width: 100, label: "Line Detail", align: 'center', formatter: showMesPackageLineDetail, classes: 'pointer', hidden: false },
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
            var grid = $(tableMesPackageId);
            var colSum = grid.jqGrid('getCol', 'MxTarget', false, 'sum');
            //grid.jqGrid('footerData', 'set', { MX_IOT_Completed: "Total", MxTarget: colSum });

            var colSumComplete = grid.jqGrid('getCol', 'MX_IOT_Completed', false, 'sum');
            grid.jqGrid('footerData', 'set', { LineName: "Total", MX_IOT_Completed: colSumComplete, MxTarget: colSum });
        },
        onSelectRow: function (rowid) {
            var rowdata = $(tableMesPackageId).jqGrid("getRowData", rowid);
            //Get row data of package group
            var objPackageGroup = GetSelectedOneRowData(tableGroupPackageId);
            if (objPackageGroup.Status === STATUSMESPACKAGEOBJ.Confirmed || !$.isEmptyObject($.trim(rowdata.StartPlan))) {
                //Disable all buttons
                DisableButtonsProductionReadiness(true);
                DisabledButton("btnResetProReadiness", true);
                DisabledButton("btnStartExecution", true);
            } else {

                //Check production readiness by checking data in database
                CheckMESProductionRadiness(rowdata, objPackageGroup);

                //Check production readiness confirm manually
                disableButtonsProductionReadinessByMesPackage(rowdata.MxPackage);
                DisabledButton("btnResetProReadiness", false);
                DisabledButton("btnStartExecution", false);
            }

        },
        //autowidth: true,
        width: null,
        shrinkToFit: false,
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
        $(tableMesPackageId).jqGrid('setGridWidth', $("#wid-id-1").width() - 30);
    });

    function showMesPackageLineDetail(cellvalue, options, rowObject) {
        return "<button type='button' class='btn btn-primary' onclick='showModalLineDetail(" + JSON.stringify(rowObject) + ")'> Line DT </button>";
    }
}

//Bind data to grid mes package
function bindDataToJqGridProPackage(packageGroup) {

    jQuery(tablePPackageId).jqGrid({
        url: '/MesManagement/GetProPackages',
        postData: {
            packageGroup: packageGroup
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'PPackage', index: 'PPackage', label: "AOMTOPS P. Packages", width: 250, classes: 'pointer' },
            { name: 'PlanQty', index: 'PlanQty', label: "Qty", align: 'center', width: 50, classes: 'pointer' }
            , { name: 'AoNo', index: 'AoNo', label: "AONO", align: 'left', hidden: true } /* 2019-06-18 Tai Le (Thomas)*/
            , { name: 'Factory', index: 'Factory', label: "FACTORY", align: 'left', hidden: true } /* 2019-06-18 Tai Le (Thomas)*/
            , { name: 'RemainQty', index: 'RemainQty', label: "Remain Qty", align: 'left', width: 100 }
            , { name: 'LATESTQCOTIME', index: 'LATESTQCOTIME', label: "Latest QCO Calc. Time", align: 'right', hidden: true } /* 2019-06-18 Tai Le (Thomas)*/
            , { name: 'NORMALIZEDPERCENT', index: 'NORMALIZEDPERCENT', label: "Readiness (%)", align: 'right', width: 90 } /* 2019-06-18 Tai Le (Thomas)*/

            , { name: 'QCOYear', index: 'QCOYear', label: "QCOYear", align: 'right', width: 50, hidden: true } /* 2019-07-22 Tai Le (Thomas)*/
            , { name: 'QCOWeekNo', index: 'QCOWeekNo', label: "QCOWeekNo", align: 'right', width: 50, hidden: true } /* 2019-07-22 Tai Le (Thomas)*/
            , { name: 'LINENO', index: 'LINENO', label: "LINENO", align: 'right', width: 50, hidden: true } /* 2019-07-22 Tai Le (Thomas)*/
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
            var grid = $(tablePPackageId);
            var colSum = grid.jqGrid('getCol', 'PlanQty', false, 'sum');
            grid.jqGrid('footerData', 'set', { PPackage: "Total", PlanQty: colSum });
        },
        //autowidth: true, //Fix with of gridview
        width: null,
        shrinkToFit: false,
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
        $(tablePPackageId).jqGrid('setGridWidth', $("#wid-id-0").width() - 30);
    });
}

function bindDataToJqGridLineDetail(mesPackage) {
    /*Get MES package line detail and bind it on gridview*/
    jQuery("#" + tableLineDetailId).jqGrid({
        url: '/MesManagement/GetLineDetailByMESPkg',
        postData: {
            mesPackage: mesPackage
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'LineSerial', index: 'LineSerial', label: "Line No", width: 70, classes: 'pointer' },
            { name: 'LineName', index: 'LineName', label: "MES Line", width: 100, classes: 'pointer' },
            { name: 'ModuleName', index: 'ModuleName', label: "Module", width: 150,  classes: 'pointer' },
            { name: 'ProcessGroupName', index: 'ProcessGroupName', label: "Process Group", width: 150, classes: 'pointer' },
            { name: 'ProDate', index: 'ProDate', label: "Production Date", formatter: "date", formatoptions: { srcformat: "d-m-Y", newformat: "d-m-Y" } },
            { name: 'RegisterId', index: 'RegisterId', label: "Register", width: 100, classes: 'pointer' },
            { name: 'RegistryDate', index: 'RegistryDate', label: "Registry Date", formatter: "date", formatoptions: { srcformat: "d-m-Y", newformat: "d-m-Y" } }, //d-m-Y H:i:s
            { name: 'ModuleId', index: 'ModuleId', hidden: true },
            { name: 'ProcessGroup', index: 'ProcessGroup', hidden: true },
            { name: 'MxPackage', index: 'MxPackage', hidden: true }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        sortname: 'id',
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {

        },
        loadComplete: function () {
        },
        onSelectRow: function (rowid) {
            
        },
        width: null,
        shrinkToFit: false
    });


    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    //$(window).on('resize.jqGrid', function () {
    //    $("#" + tableLineDetailId).jqGrid('setGridWidth', $("#wid-id-1").width() - 30);
    //});
}
//#endregion

//#region MES package line detail
function showModalLineDetail(dataRow) {   
    //Reload line detail gridview
    reloadLineDetail(dataRow.MxPackage);

    //Show modal MES package line
    ShowModalDragable("mdlMesPackageLineDt");    
}

function getProcessGroups() {
    GetMasterCodes("drpProGroupLineDt", "OPGroup", null);
}

//Create object line detail
function createObjectLineDetail() {

    let selMesPkgRow = GetSelectedOneRowData(tableMesPackageId);

    let lnDt = {
        MxPackage: selMesPkgRow.MxPackage,
        LineSerial: $("#drpLineDt").val(),
        ModuleId: $("#drpModuleLineDt").val(),
        ProcessGroup: $("#drpProGroupLineDt").val(),
        ProDate: $("#txtProDateLineDt").val(),
        RegisterId: $("#hdUserId").val()
    };

    return lnDt;
}

function initProductionDate(dateId) {
    /*Init date picker*/
    $("#" + dateId).daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        "setDate": new Date(),
        locale: {
            format: 'YYYY/MM/DD'
        }        
    });

}

function reloadLineDetail(mesPackage) {
    //Reload line detail gridview
    var params = { mesPackage: mesPackage };
    ReloadJqGrid2LoCal(tableLineDetailId, params);
}
//#endregion

//#region Check list readinsess

function disableButtonsProductionReadinessByMesPackage(mxPackage) {
    GetProductionReadinessCheckList(mxPackage, function (lstMpcl) {

        //Enable all buttons
        DisableButtonsProductionReadiness(false);

        $.each(lstMpcl, function (index, value) {

            var checkListId = value.CheckListId;

            switch (checkListId) {
                case ProductionReadiness.MESOP:
                    DisabledButton("btnMesOpCheck", true);
                    $("#btnMesOpCheck").prop('value', '100%');
                    break;
                case ProductionReadiness.BOMPattern:
                    DisabledButton("btnBOMPatterns", true);
                    $("#btnBOMPatterns").prop('value', '100%');
                    break;
                case ProductionReadiness.PPMeeting:
                    DisabledButton("btnPPMeetingCnf", true);
                    $("#btnPPMeetingCnf").prop('value', '100%');
                    break;
                case ProductionReadiness.MaterialReadiness:
                    DisabledButton("btnMatReadiness", true);
                    $("#btnMatReadiness").prop('value', '100%');
                    break;
                case ProductionReadiness.CuttingStatus:
                    DisabledButton("btnCuttingStatus", true);
                    $("#btnCuttingStatus").prop('value', '100%');
                    break;
                case ProductionReadiness.Treatment:
                    DisabledButton("btnTreatments", true);
                    $("#btnTreatments").prop('value', '100%');
                    break;
                case ProductionReadiness.Tools:
                    DisabledButton("btnToolReadiness", true);
                    $("#btnToolReadiness").prop('value', '100%');
                    break;
                case ProductionReadiness.Machines:
                    DisabledButton("btnMachineReadiness", true);
                    $("#btnMachineReadiness").prop('value', '100%');
                    break;
                case ProductionReadiness.JIG:
                    DisabledButton("btnJigRegistered", true);
                    $("#btnJigRegistered").prop('value', '100%');
                    break;
                default:
                    DisabledButton("btnLineSetup", true);
                    $("#btnLineSetup").prop('value', '100%');
                    break;
            }
        });
    });
}

//Disable or enable all buttons production readiness
function DisableButtonsProductionReadiness(blDis) {
    DisabledButton("btnMesOpCheck", blDis);
    DisabledButton("btnBOMPatterns", blDis);
    DisabledButton("btnPPMeetingCnf", blDis);
    //DisabledButton("btnMatReadiness", blDis);
    DisabledButton("btnCuttingStatus", blDis);
    DisabledButton("btnTreatments", blDis);
    DisabledButton("btnToolReadiness", blDis);
    DisabledButton("btnMachineReadiness", blDis);
    DisabledButton("btnJigRegistered", blDis);
    DisabledButton("btnLineSetup", blDis);

    //Set value 0% for button
    $("#btnMesOpCheck").prop('value', '0%');
    $("#btnBOMPatterns").prop('value', '0%');
    $("#btnPPMeetingCnf").prop('value', '0%');
    $("#btnCuttingStatus").prop('value', '0%');
    $("#btnTreatments").prop('value', '0%');
    $("#btnToolReadiness").prop('value', '0%');
    $("#btnMachineReadiness").prop('value', '0%');
    $("#btnJigRegistered").prop('value', '0%');
    $("#btnLineSetup").prop('value', '0%');
}

function InsertReadinessCheckList(mpcl, type = '') {
    var config = ObjectConfigAjaxPost(
        "../MesManagement/InsertReadniessCheckList"
        , false
        , JSON.stringify({
            mpcl: mpcl
        })
    );

    if (type == 'MGCL') {
        config = ObjectConfigAjaxPost(
            "../MesManagement/InsertGroupReadniessCheckList"
            , false
            , JSON.stringify({
                mgcl: mpcl
            })
        );
    }

    AjaxPostCommon(config, function (resIns) {
        if (resIns === Success) {
            //Recheck production readiness buttons
            disableButtonsProductionReadinessByMesPackage(mpcl.MxPackage);
            ShowMessage("Production Readiness", "Susscess", ObjMessageType.Info);
        } else if (resIns === NoAuthority) {
            ShowMessage("Production Readiness", "You have no authority.", ObjMessageType.Info);
        } else {
            ShowMessage("Production Readiness", resIns, ObjMessageType.Error);
        }
    });
}

function RecalculateMaterialReadiness(pQCOQueue) {
    var config = ObjectConfigAjaxPost(
        "../QCO/GetLatestQCOMaterialReadiness"
        , false
        , JSON.stringify({
            qcoQueue: pQCOQueue
        })
    );
    AjaxPostCommon(config, function (resIns) {
        console.log(resIns);

        if (resIns === Success) {
            ShowMessage("Re-calculate Readiness", "Re-calculate Readiness take 5-10 minute to complete.", ObjMessageType.Info);
        } else if (resIns === NoAuthority) {
            ShowMessage("Re-calculate Readiness", "You have no authority.", ObjMessageType.Info);
        } else {
            ShowMessage("Re-calculate Readiness", resIns, ObjMessageType.Error);
        }
    });
}

function resetReadinessCheckList(mxPackage) {
    var config = ObjectConfigAjaxPost(
        "../MesManagement/DeleteReadniessCheckList"
        , false
        , JSON.stringify({
            mxPackage: mxPackage
        })
    );
    AjaxPostCommon(config, function (resIns) {
        if (resIns === Success) {
            //Recheck production readiness buttons
            disableButtonsProductionReadinessByMesPackage(mxPackage);
            ShowMessage("Production Readiness", "Susscess", ObjMessageType.Info);
        } else if (resIns === NoAuthority) {
            ShowMessage("Production Readiness", "You have no authority.", ObjMessageType.Info);
        } else {
            ShowMessage("Production Readiness", resIns, ObjMessageType.Error);
        }
    });
}

function GetProductionReadinessCheckList(mxPackage, callBack) {
    var config = ObjectConfigAjaxPost(
        "../MesManagement/GetProductionReadiness"
        , false
        , JSON.stringify({
            mxPackage: mxPackage
        })
    );
    AjaxPostCommon(config, function (lstMpcl) {
        callBack(lstMpcl);
    });
}

function GetMaterialReadinessCheckList(packageGroup, callBack) {
    var config = ObjectConfigAjaxPost(
        "../MesManagement/GetMaterialReadiness"
        , false
        , JSON.stringify({
            packageGroup: packageGroup
        })
    );
    AjaxPostCommon(config, function (lstMgcl) {
        callBack(lstMgcl);
    });
}

//Update package group statusi
function UpdateMesStartPlan(packageGroup, seqNo) {
    var config = ObjectConfigAjaxPost(
        "../MesManagement/UpdateMesStartPlan"
        , false
        , JSON.stringify({
            packageGroup: packageGroup
            , seqNo: seqNo
        })
    );
    AjaxPostCommon(config, function (resIns) {
        if (resIns === Success) {
            //Reload production package and mes package           
            var params = { packageGroup: packageGroup };
            ReloadJqGrid2LoCal(tableMesPackageName, params);

            ShowMessage("Production Status", "Susscess", ObjMessageType.Info);
        } else if (resIns === NoAuthority) {
            ShowMessage("Production Status", "You have no authority.", ObjMessageType.Info);
        } else {
            ShowMessage("Production Status", resIns, ObjMessageType.Error);
        }
    });
}

function CheckMESProductionRadiness(rowdata, pkgGroupData) {
    let styleCode = pkgGroupData.StyleCode;
    let styleSize = pkgGroupData.StyleSize;
    let styleColorSerial = pkgGroupData.StyleColorSerial;
    let revNo = pkgGroupData.RevNo;
    let mxPackage = pkgGroupData.MxPackage;
    let edition = "M";
    //Get MES operation plan
    let opmt = GetOperationPlanMES(styleCode, styleSize, styleColorSerial, revNo, mxPackage);
    if (opmt != null) {
        $("#btnMesOpCheck").prop('100%');
        DisabledButton("btnMesOpCheck", true);

        //Get list BOM and patterns
        let listBOMPts = GetListBOMPatternMySql(styleCode, styleSize, styleColorSerial, revNo, opmt.opRevNo, edition);
        if (listBOMPts.length > 0) { //If the list of BOM pattern is not empty
            DisabledButton("btnBOMPatterns", false);
            $("#btnBOMPatterns").prop('value', '100%');
        } else {
            DisabledButton("btnBOMPatterns", true);
            $("#btnBOMPatterns").prop('value', '0%');
        }
    } else {
        //Disable button MES Operation Plan status
        $("#btnMesOpCheck").prop('value', '0%');
        DisabledButton("btnMesOpCheck", false);

        //Disable button BOM patterns status
        DisabledButton("btnBOMPatterns", false);
        $("#btnBOMPatterns").prop('value', '0%');
    }

}

//Get list BOM and pattern from MySQL
function GetListBOMPatternMySql(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition) {
    let listBOMPts = [];
    let config = ObjectConfigAjaxPost(
        "../MesManagement/GetMesBOMPatternMySql", false
        , JSON.stringify({
            styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo, opRevNo: opRevNo, edition: edition
        })
    );
    AjaxPostCommon(config, function (resGet) {
        if (resGet.IsSuccess) {
            listBOMPts = resGet.Data;
        }
    });

    return listBOMPts;
}

//#endregion

function GetMesPackagesById(packageGroup, seqNo) {
    var config = ObjectConfigAjaxPost(
        "../MesManagement/GetMesPackages"
        , false
        , JSON.stringify({
            packageGroup: packageGroup, seqNo: seqNo
        })
    );
    AjaxPostCommon(config, function (mpcl) {
        return mpcl;
    });
}

function ConpleteProducitonPackageGroup(pkgGroupRow) {
    ShowConfirmYesNo(
        "Complete production package group"
        , "Are you sure?"
        , function () {
            if (pkgGroupRow.Status === STATUSPACKAGEGROUPOBJ.Open) {

                let pkgGroup = pkgGroupRow.PackageGroup;
                let pkgGroupStatus = STATUSPACKAGEGROUPOBJ.Confirmed;
                let completedId = $("#hdUserId").val();

                var config = ObjectConfigAjaxPost("../MesManagement/UpdateMesPackageStatus", false
                    , JSON.stringify({ packageGroup: pkgGroup, seqNo: seqNo, mesStatus: mesStatus, confirmedId: confirmedId }));
                AjaxPostCommon(config, function (resUpd) {

                    if (resUpd === Success) {
                        var params = { packageGroup: pkgGroup };
                        ReloadJqGrid2LoCal(tableMesPackageName, params);
                        ShowMessage("Confirm MES package", "Successfully", ObjMessageType.Info);
                    } else {
                        ShowMessage("Confirm MES package", resUpd, ObjMessageType.Error);
                    }

                });
            }
        }
        , function () { }
    );
}

function CompletePackageGroup(pkgGroupRow) {
    ShowConfirmYesNo(
        "Complete production package group"
        , "Are you sure to complete production package?"
        , function () {

            //string packageGroup, string pkgGroupStatus, string completedId
            let packageGroup = pkgGroupRow.PackageGroup;
            let pkgGroupStatus = STATUSPACKAGEGROUPOBJ.Close;
            let completedId = $("#UserId").val();


            var config = ObjectConfigAjaxPost("../MesManagement/CompleteProductionPackageGroup", false, JSON.stringify({ packageGroup: packageGroup, pkgGroupStatus: pkgGroupStatus, completedId: completedId }));
            AjaxPostCommon(config, function (resIns) {
                if (resIns === Success) {

                    ReloadPackageGroup();
                    ShowMessage("Complete Pacakge Group", "Susscess", ObjMessageType.Info);
                } else {
                    ShowMessage("Complete Pacakge Group", resIns, ObjMessageType.Error);
                }
            });
        }
        , function () { }
    );
}

function ReloadPackageGroup() {
    var factoryId = $("#drpFactory").val();
    var arrDateRange = $('#txtDateRange').val().split('-');
    var startDate = $.trim(arrDateRange[0].replace(/\//g, ''));
    var endDate = $.trim(arrDateRange[1].replace(/\//g, ''));
    var buyer = $("#drpBuyer").val();
    var buyerInfo = $("#txtBuyerInfo").val();
    var aoNo = $("#txtBuyerInfo").val();

    reloadGridPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo);
}

function CopyStyleInformation() {
    let pkgGroupRow = GetSelectedOneRowData(tableGroupPackageId);

    let styleCode = pkgGroupRow.StyleCode;
    let styleSize = pkgGroupRow.StyleSize;
    let styleColorSerial = pkgGroupRow.StyleColorSerial;
    let revNo = pkgGroupRow.RevNo;

    var config = ObjectConfigAjaxPost("../MesManagement/CopyStyleInfo", false
        , JSON.stringify({ packageGroup: pkgGroup, seqNo: seqNo, mesStatus: mesStatus, confirmedId: confirmedId }));
    AjaxPostCommon(config, function (resUpd) {


    });
    ShowMessage("Copy style information MES package", styleCode + styleSize + styleColorSerial + revNo, ObjMessageType.Info);

}

function OpenQCODetailDialog($this) {
    //console.log($this);
    //console.log($($this).attr('data-ulr'));

    $.post($($this).attr('data-ulr'))
        .done(function (res) {
            //console.log(res);

            let $mymodal = $("#myModal");

            var buttons = [];
            buttons.push({ id: "ModalConfirmMat", caption: "Confirm Material Readiness", classes: "btn btn-success", type: "button", target: $($this).attr('data-target'), onclickFunc: "ModalConfirmMatClick(this);", datadismiss: '' });
            buttons.push({ id: "ModalCalcMat", caption: "Calculate Material Readiness", classes: "btn btn-primary", type: "button", target: '', onclickFunc: "ModalCalcMatClick(this);", datadismiss: '' });
            buttons.push({ id: "", caption: "Close", classes: "btn btn-default", type: "button", target: '', onclickFunc: "", datadismiss: "modal" });

            //remove modal-header
            let ModalHeaderHtml = "";

            for (let intI = 0; intI < buttons.length; intI++) {
                ModalHeaderHtml +=
                    '<button id="' + buttons[intI].id + '" type="' + buttons[intI].type + '" class="' + buttons[intI].classes + '" data-target="' + buttons[intI].target + '" onclick="' + buttons[intI].onclickFunc + '" data-dismiss="' + buttons[intI].datadismiss + '" >' + buttons[intI].caption + '</button>&nbsp;';
            }

            ModalHeaderHtml = '<div style="text-align:center;">' + ModalHeaderHtml + '</div>';

            $mymodal.find("div.modal-header").html(ModalHeaderHtml);

            //update the modal's body with the response received
            $mymodal.find("div.modal-body").html(res);

            //remove modal-footer
            $mymodal.find("div.modal-footer").remove();

            // Show the modal
            $mymodal.modal("show");
        });
    return false;
}