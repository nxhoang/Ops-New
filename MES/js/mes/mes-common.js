var BuyerMasterCode = "Buyer";
var StatusOkMasterCode = "OK";

var STATUSMESPACKAGEOBJ = {
    Open: "RO",
    Confirmed: "CF"
};

var STATUSPACKAGEGROUPOBJ = {
    Open: "RO",
    PartiallyConfirmed: "PC",
    Confirmed: "CF",
    Started: "ST",
    PauseCancel: "PC",
    Close: "CL"
};

//Get factory
function GetFactories(controlId, factoryId) {
    var config = ObjectConfigAjaxPost("../MasterData/GetFactoriesMySql", false
        , JSON.stringify({ factoryId: factoryId }));
    AjaxPostCommon(config, function (lstFac) {      
        FillDataToDropDownlist(controlId, lstFac, "Factory", "Name");
    });
}

//Get master code by master code and status
function GetLineByFactoryId(controlId, factoryId) {
    var config = ObjectConfigAjaxPost("../MasterData/GetLineByFactoryMySql", false
        , JSON.stringify({ factoryId: factoryId }));
    AjaxPostCommon(config, function (listLine) {       
        FillDataToDropDownlist(controlId, listLine, "LineSerial", "LineName");
    });
}

//Get master code by master code and status
function GetMasterCodes(controlId, mCode, codeStatus) {
    var config = ObjectConfigAjaxPost("../MasterData/GetMasterCodesMySql", false
        , JSON.stringify({ mCode: mCode, codeStatus: codeStatus }));
    AjaxPostCommon(config, function (lstMtCode) {
        $.each(lstMtCode, function (index, value) {
            value.CodeName = value.SubCode + " - " + value.CodeName;
        });
        FillDataToDropDownlist(controlId, lstMtCode, "SubCode", "CodeName");
    });
}

//Get master code by master code and status in ERP database
function GetMasterCodesErp(controlId, mCode, codeStatus) {
    var config = ObjectConfigAjaxPost("../MasterData/GetMasterCodes", false
        , JSON.stringify({ mCode: mCode, codeStatus: codeStatus }));
    AjaxPostCommon(config, function (lstMtCode) {
        $.each(lstMtCode, function (index, value) {
            value.CodeName = value.SubCode + " - " + value.CodeName;
        });
        FillDataToDropDownlist(controlId, lstMtCode, "SubCode", "CodeName");
    });
}

//Get list of master code with code description and code detail.
function GetMasterCodeOracle(controlId, masterCode, subCode, codeDesc, codeDetail) {
    var config = ObjectConfigAjaxPost("../MasterData/GetMasterCodeOracle", false
        , JSON.stringify({ masterCode: masterCode, subCode: subCode, codeDesc: codeDesc, codeDetail: codeDetail }));
    AjaxPostCommon(config, function (lstMtCode) {
        $.each(lstMtCode, function (index, value) {
            value.CodeName = value.SubCode + " - " + value.CodeName;
        });
        FillDataToDropDownlist(controlId, lstMtCode, "SubCode", "CodeName");
    });
}

function GetCategoryMachineTool(controlId, isMachine) {
    var config = ObjectConfigAjaxPost("../MasterData/GetCategoryMachineTool", false
        , JSON.stringify({ isMachine: isMachine }));
    AjaxPostCommon(config, function (lstMtCode) {
        $.each(lstMtCode, function (index, value) {
            value.CodeName = value.SubCode + " - " + value.CodeName;
        });
        FillDataToDropDownlist(controlId, lstMtCode, "SubCode", "CodeName");
    });
}

//Get list of company of corporation
function GetCompanyCorporation(callBack) {
    var config = ObjectConfigAjaxPost("../MasterData/GetCompanyListCoporation", false, JSON.stringify({}));
    AjaxPostCommon(config, function (lstDcmt) {       
        callBack(lstDcmt);
    });
}

//Get list of factory by corporation
function GetFactoriesByCorporation(corporationCode, callBack) {
    var config = ObjectConfigAjaxPost("../MasterData/GetFactoriesByCorporation", false, JSON.stringify({ corporationCode: corporationCode}));
    AjaxPostCommon(config, function (lstDcmt) {        
        callBack(lstDcmt);
    });
}

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

function GetOperationPlanMES(styleCode, styleSize, styleColorSerial, revNo, mxPackage) {
    let listOp = [];
    let config = ObjectConfigAjaxPost(
        "../MesManagement/GetMesOperationPlan", false
        , JSON.stringify({
            styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo, mxPackage: mxPackage
        })
    );
    AjaxPostCommon(config, function (resIns) {
        if (resIns.IsSuccess) {
            listOp = resIns.Data;
        }
    });

    return listOp;
}

/*
 * Add days to the date 
 */
function AddDays(date, days) {
    var result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}

function GetModulesByStyleCodeMySql(controlId, styleCode) {
    /*Get list of module by style code*/
    var config = ObjectConfigAjaxPost("../MasterData/GetListModulesByStyleCode", false
        , JSON.stringify({ styleCode: styleCode }));
    AjaxPostCommon(config, function (listModules) {
        FillDataToDropDownlist(controlId, listModules, "ModuleId", "ModuleName");
    });
}

function DateRangePicker(controlId, blSingleDate) {
    $(controlId).daterangepicker({
        singleDatePicker: blSingleDate,
        showDropdowns: true,
        locale: {
            format: 'YYYY/MM/DD'
        },
        minYear: 1901,
        maxYear: parseInt(moment().format('YYYY'), 10)
    }).on('show.daterangepicker', function (e) {
        origStyle = $($(controlId).data('daterangepicker').container[0]).attr('style');
        let neworigStyle = origStyle + "opacity: 1 ; transform : scale(1);";
        $($(controlId).data('daterangepicker').container[0]).removeAttr('style');
        $($(controlId).data('daterangepicker').container[0]).attr('style', neworigStyle);
    }).on('hide.daterangepicker', function (e) {
        $($(controlId).data('daterangepicker').container[0]).removeAttr('style');
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
        $($(controlId).data('daterangepicker').container[0]).attr('style', neworigStyle);
    });
}

function SelectDateRangePicker(dateRangeId) {
    var origStyle = "";
    $(dateRangeId).daterangepicker(
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
        origStyle = $($(dateRangeId).data('daterangepicker').container[0]).attr('style');
        let neworigStyle = origStyle + "opacity: 1 ; transform : scale(1);";
        $($(dateRangeId).data('daterangepicker').container[0]).removeAttr('style');
        $($(dateRangeId).data('daterangepicker').container[0]).attr('style', neworigStyle);
    }).on('hide.daterangepicker', function (e) {
        $($(dateRangeId).data('daterangepicker').container[0]).removeAttr('style');
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
        $($(dateRangeId).data('daterangepicker').container[0]).attr('style', neworigStyle);
    });
    $(dateRangeId).data('daterangepicker').setStartDate(getCurrentDate(0));
    $(dateRangeId).data('daterangepicker').setEndDate(getCurrentDate(20));
}

//Create list week number
function CreateWeekNo() {
    let listWeekNo = [];
    for (let i = 1; i < 54; i++) {
        //Add zero character before number less than 10
        let wn = i < 10 ? '0' + i : i;

        let objWeek = {
            WeekNo: wn,
            WeekName: wn
        };

        listWeekNo.push(objWeek);
    }

    return listWeekNo;
}

//Get list working time sheet by plan year, month and day
function GetWorkingTimeSheet(factoryId, planYear, planMonth, planDay, callBack) {   
    var config = ObjectConfigAjaxPost("../CommonAPI/GetWorkingTimeSheet", false
        , JSON.stringify({ factoryId: factoryId, planYear: planYear, planMonth: planMonth, planDay: planDay}));
    AjaxPostCommon(config, function (listWts) {
        callBack(listWts);
    });
}