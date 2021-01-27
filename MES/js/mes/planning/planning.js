//#region Variable
var PPLINES;
var OBJDRAGMESPKG;
var OBJDRAGPROPKG;
var DAYPILOTMES;
var OBJEVENTMTOPSEL = [];
//var ISDRAGEVENT = true;
var LIST_PACKAGE_GROUP = []; //Keep list of package group when creating MES package
var LIST_PP_PACKAGE_GROUP = [];
var LIST_DELETED_MES_PACKAGE = [];
var QCO_CURRENT_LIST = [];
var LIST_MES_SEQ = []; //ADD) SON (2019.09.17) - 17 September 2019 - add list mes package sequence

var _listWorkingTime = [];
var _endDateDpMes = new Date();

var ISUPDATED = true;

var CREATEMESQCO = false;
var SELECTEDROWQCO = [];

var COMBINATIONSTATUS = "N";

var dpMtop;
var dpMes;
var _dpMtopModule;//ADD - SON) 24/Nov/2020
var _dpMesModule;//ADD - SON) 24/Nov/2020
var _currentTab = 1;//ADD - SON) 24/Nov/2020

var CUR_PACKAGE_SEQ = 0; //Current package sequence
//var CUR_PACKAGE_GROUP = ""; //Current package group
var CUR_NUMBER_MES_PKG = 0; //The current number of mes packages

var EVENTCOLOR = ["#00ace6", "#00cccc", "#00cc7a", "#009933", "#990099", "#cc3300"
    , "#666699", "#993366", "#00b38f", "#cc0088", "#cc00cc", "#0099cc"];

//#region jqgrid

var tableMesPackageId = "#tbMesPackage";
var tableMesPackageName = "tbMesPackage";
var paperMesPackageId = "#divMesPackagePage";
var paperMesPackageName = "divMesPackagePage";

var tableQCOPPKGId = "#tbQCOPPKG";
var tableQCOPPKGName = "tbQCOPPKG";
var paperQCOPPKGId = "#divQCOPPKGPage";
var paperQCOPPKGName = "divQCOPPKGPage";
//#endregion

//#endregion

function initPage() {
    //Init partial view package group
    initPackageGroupPartialView();

    //Init date range of production package
    InitDateRangePp();

    //Init date range of MES package
    InitDateRangeMES();

    //Init start date to distribute MES package
    InitStartDateSingle();

    //Testing function
    //initPageTest();

    //COMBINATIONSTATUS = GetCombinationByCorpAndFactory("1002", "ALL");
    COMBINATIONSTATUS = GetCombinationByCorpAndFactory();

    //Init grid mes package
    bindDataToJqGridMesPackage(null);

    //Set factory base on factory of user role.
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    //Get list of factory
    GetFactories("drpFactoryMtop", null);
    GetFactories("drpFactoryMes", null);
    GetFactories("drpFactoryQco", null);

    //Get list of Buyer
    //GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);

    GetMasterCodes("drpBuyerMtop", BuyerMasterCode, StatusOkMasterCode);
    GetMasterCodes("drpBuyerQco", BuyerMasterCode, StatusOkMasterCode);

    //var dpMes = new DayPilot.Scheduler("dpMes");

    initDayPilot();
    initDayPilotMes();

    ////Load line no
    //FillDataMultipleSelectLineNoMdl("drpLineNo", PPLINES, "id", "name");

    //Event divide production package
    eventDivideProPackage();

    //Set factory base on factory of user role.
    $("#drpFactoryMtop").val($("#hdFactoryUser").val()).trigger('change');
    $("#drpFactoryMes").val($("#hdFactoryUser").val()).trigger('change');
    $("#drpFactoryQco").val($("#hdFactoryUser").val()).trigger('change');

    //Event click search production package.
    eventClickBtnSearchPp();

    eventClickSearchMESPackage();

    //Event click add mes package
    eventAddMesPackage();
    //Event click button edit MES package
    eventEditMesPacakge();

    //Event Save MES package
    eventSaveMesPackage();

    clickCancelModalDistributionMes();

    clickTabProductionpackage();

    clickSearchQcoPackage();

    bindDataToJqGridProductionPackageByQco(null, null, null, null, null, null);

    $("#txtDailyTarQty").val("400");

    //Init slider for filitering ranking
    QCOSlider(1, 100);

    checkInitalRole($("#lblRole").text(), "MES", "PPL", function (role) {
        if (role.IsAdd === "1") {
            DisabledButton("btnAddMesPkg", false);
        } else {
            DisabledButton("btnAddMesPkg", true);
        }

        if (role.IsUpdate === "1") {
            DisabledButton("btnEditMesPkg", false);
        } else {
            DisabledButton("btnEditMesPkg", true);
        }
    });

    //START ADD - SON) 24/Nov/2020
    //initial schedule of aomtop packages for module scheduling
    //initDayPilotAomtopsModule();
    //initDayPilotMesModule();
    //eventClickOnButtonModuleTab();
    //END ADD - SON) 24/Nov/2020
}

function checkInitalRole(roleId, systemId, menuId, callBack) {

    let config = ObjectConfigAjaxPost("../Masterdata/GetRoleMySql", false, JSON.stringify({ roleId: roleId, systemId: systemId, menuId: menuId }));
    AjaxPostCommon(config, function (role) {
        callBack(role);
    });
}

function initPageTest() {

    //factoryId = "P2C1";
    //startDate = "20150705";
    //endDate = "20150720";
    //var buyer = null;
    //var styleInfo = null;
    //var aoNo = "AD-LFM-0034";

    $("#txtDailyTarQty").val("400");
    $("#txtAoNumber").val("AD-LFM-0034");
    $("#txtStartDate").val("2015/07/11");

    $("#drpFactory").val("P2A1").trigger("change");

    //$("#drpFactoryMtop").val("P2C1").trigger();

    $('#txtDateRangePp').data('daterangepicker').setStartDate("2015/07/05");
    $('#txtDateRangePp').data('daterangepicker').setEndDate("2015/07/27");

    $('#txtDateRange').data('daterangepicker').setStartDate("2018/11/20");
    $('#txtDateRange').data('daterangepicker').setEndDate("2018/11/26");

}

function InitDateRangePp() {
    var origStyle = "";
    $('#txtDateRangePp').daterangepicker(
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
        origStyle = $($('#txtDateRangePp').data('daterangepicker').container[0]).attr('style');
        let neworigStyle = origStyle + "opacity: 1 ; transform : scale(1);";
        $($('#txtDateRangePp').data('daterangepicker').container[0]).removeAttr('style');
        $($('#txtDateRangePp').data('daterangepicker').container[0]).attr('style', neworigStyle);
    }).on('hide.daterangepicker', function (e) {
        $($('#txtDateRangePp').data('daterangepicker').container[0]).removeAttr('style');
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
        $($('#txtDateRangePp').data('daterangepicker').container[0]).attr('style', neworigStyle);
    });
    $('#txtDateRangePp').data('daterangepicker').setStartDate(getCurrentDate(0));
    $('#txtDateRangePp').data('daterangepicker').setEndDate(getCurrentDate(20));

    //$('#txtDateRangePp').data('daterangepicker').setStartDate("2020/12/11");
    //$('#txtDateRangePp').data('daterangepicker').setEndDate("2021/01/31");
}

function InitDateRangeMES() {
    var origStyle = "";
    $('#txtDateRangeMes').daterangepicker(
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
        origStyle = $($('#txtDateRangeMes').data('daterangepicker').container[0]).attr('style');
        let neworigStyle = origStyle + "opacity: 1 ; transform : scale(1);";
        $($('#txtDateRangeMes').data('daterangepicker').container[0]).removeAttr('style');
        $($('#txtDateRangeMes').data('daterangepicker').container[0]).attr('style', neworigStyle);
    }).on('hide.daterangepicker', function (e) {
        $($('#txtDateRangeMes').data('daterangepicker').container[0]).removeAttr('style');
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
        $($('#txtDateRangeMes').data('daterangepicker').container[0]).attr('style', neworigStyle);
    });
    $('#txtDateRangeMes').data('daterangepicker').setStartDate(getCurrentDate(0));
    $('#txtDateRangeMes').data('daterangepicker').setEndDate(getCurrentDate(20));

}

function InitStartDateSingle() {
    //$("#txtStartDate").val(getCurrentDate(0));

    $("#txtStartDate").daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        "setDate": new Date(),
        locale: {
            format: 'YYYY/MM/DD'
        }
        //minYear: 1901,
        //maxYear: parseInt(moment().format('YYYY'), 10)
    });

}

//#region Redifine functions
function SelectedRowPackageGroup(dataRow) {

}

function BeforeSelectRowPackageGv(rowid, e) {
    return true;
}

//Redifine function search package group
function eventSearchPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo) {
    //Reload production package and mes package    
    ReloadJqGrid2LoCal(tableMesPackageName, null);
}

//#endregion

//#region Day Pilot

function initDayPilot() {
    var arrDateRange = $("#txtDateRangePp").val().split('-');

    var d1 = new Date(arrDateRange[0].trim());
    var d2 = new Date(arrDateRange[1].trim());
    var difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    //var arrDateRange = $("#txtDateRangePp").val().split('-');
    //var startDate = $.trim(arrDateRange[0].replace(new RegExp('/', 'g'), ''));
    //var endDate = $.trim(arrDateRange[1].replace(new RegExp('/', 'g'), ''));
    //var factoryId = $("#drpFactory").val();

    //var buyer = $("#drpBuyerMtop").val();
    //var styleInfo = $("#txtStyleInfo").val();
    //var aoNo = $("#txtAoNumber").val(); 

    //var dpMtop = new DayPilot.Scheduler("dpMtop");
    dpMtop = new DayPilot.Scheduler("dpMtop");

    //var dpMes = new DayPilot.Scheduler("dpMes");

    // behavior and appearance
    dpMtop.cellWidth = 40;
    dpMtop.eventHeight = 25;
    dpMtop.headerHeight = 25;

    // view
    dpMtop.startDate = arrDateRange[0];
    dpMtop.days = difference + 1;
    dpMtop.scale = "Day";

    dpMtop.rowMarginBottom = 5;

    dpMtop.timeHeaders = [
        { "groupBy": "Month" },
        { "groupBy": 'Week' },
        { "groupBy": "Day", "format": "d" }
    ];

    dpMtop.eventClickHandling = "Select";

    dpMtop.bubble = new DayPilot.Bubble({
        onLoad: function (args) {
            console.log(args);
            //console.log(args.source.data.tag);
            //console.log(JSON.parse(args.source.data.tag));

            //var strHtml = "<strong>Package Information</strong>";
            ////strHtml += "</br> Package: " + args.source.data.id;
            //strHtml += "</br> Package: " + args.source.data.PrdPkg;
            //strHtml += "</br> Factory: " + args.source.data.Factory;
            //strHtml += "</br> Line No: " + args.source.data.LineNo;
            //strHtml += "</br> AO No: " + args.source.data.AoNo;
            //strHtml += "</br> Style Code: " + args.source.data.StyleCode;
            //strHtml += "</br> Style Size: " + args.source.data.StyleSize;
            //strHtml += "</br> Style Color: " + args.source.data.StyleColorSerial;
            //strHtml += "</br> Rev No: " + args.source.data.RevNo;
            //strHtml += "</br> Plan Production Date: " + args.source.data.start.toString("yyyy/M/d") + " - " + args.source.data.end.toString("yyyy/M/d");
            //strHtml += "</br> Plan Qty: " + args.source.data.PlanQty; 
            //args.html = strHtml;

            args.async = true;

            var htmlContent = "";
            $.ajax({
                url: '/QCO/POPStyleSummary/?StyleCode=' + args.source.data.StyleCode
                    + '&StyleSize=' + args.source.data.StyleSize
                    + '&StyleColorSerial=' + args.source.data.StyleColorSerial
                    + '&RevNo=' + args.source.data.RevNo
                    + '&PRDPKG=' + args.source.data.PrdPkg
                    + '&RemainQty=' + (args.source.data.RemainQty == null ? args.source.data.PlanQty : args.source.data.RemainQty)
            })
                .then(function (content) {
                    // Set the tooltip content upon successful retrieval
                    htmlContent = content;
                }, function (xhr, status, error) {
                    console.log(xhr, status, error);
                    // Upon failure... set the tooltip content to error
                    htmlContent = status + ': ' + error;
                }).then(() => {
                    args.html = htmlContent;
                    args.loaded();
                });

            //setTimeout(function () {
            //    args.html = htmlContent;
            //    args.loaded();
            //}, 1000);
        }
        //, showAfter: 0 //show bubble after a time
        //, hideAfter: 0 //hide when click mouse
        , animation: "fast" //fast is default, jump, slow 
    });

    dpMtop.treeEnabled = true;
    dpMtop.rowHeaderWidth = 100;

    //Load prodcution package and lines
    dpMtop.resources = [];
    dpMtop.events.list = [];

    dpMtop.eventHoverHandling = "Bubble";
    dpMtop.eventClickHandling = "Select";
    //dpMtop.allowMultiSelect = false; //Do not allow multi select
    dpMtop.allowMultiSelect = COMBINATIONSTATUS === "Y" ? true : false; //Do not allow multi select

    // dp.allowEventOverlap = false;
    dpMtop.allowMultiMove = false;
    dpMtop.multiMoveVerticalMode = "Disabled";

    dpMtop.moveDisabled = true;
    dpMtop.dragOutAllowed = false;

    dpMtop.heightSpec = "Max";
    dpMtop.height = 500;

    //#Note: DayPilot Pro
    dpMtop.cornerHtml = "LINE";

    // event moving
    dpMtop.onEventMove = function (args) {

        eventMove(dpMtop, args);

        console.log("dpMtop - onEventMove:" + args.e.start().toString());
    };

    dpMtop.onEventMoved = function (args) {
        //dpMtop.message("Event " + args.e.text() + " moved.");

        console.log("dpMtop - onEventMoved:" + args.e.text());
    };

    dpMtop.onEventMoving = function (args) {
        //args.left.enabled = true;
        //args.left.html = args.e.start().toString();
        //OBJDRAGPROPKG = args.e;

        console.log("dpMtop - onEventMoving:" + args.e.start().toString());
    };

    // event resizing
    //dpMtop.onEventResized = function (args) {
    //    dpMtop.message("Resized: " + args.e.text());
    //};

    //Corlor sunday colum
    dpMtop.onBeforeCellRender = function (args) {
        if (args.cell.start.getDayOfWeek() === 0) {
            args.cell.backColor = "#dddddd";
        }
    };

    dpMtop.onEventSelect = function (args) {
        //args.e.data.backColor = "#1a75ff";
        if (args.selected && args.e.text().indexOf("unselectable") !== -1) {  // prevent selecting events that contain the text "unselectable"
            args.preventDefault();
        }
    };

    //Show context menu to delete event
    dpMtop.contextMenu = new DayPilot.Menu({
        items: [
            //{ text: "Delete", onclick: function () { var e = this.source; dpMtop.events.remove(e); } },
            {
                text: "Create MES package", onclick: function () {

                    //set ISDRAGEVENT is false when user click right mouse on event to create MES package
                    //ISDRAGEVENT = false;
                    //deselect all
                    $('#drpLineNo').multiselect('deselectAll', false);
                    $('#drpLineNo').multiselect('updateButtonText');

                    //Check MES factory
                    var facMes = $("#drpFactoryMes").val();
                    if (isEmpty(facMes)) {
                        ShowMessage("Create MES Package", "You do not belong to any factory.", ObjMessageType.Info);

                        return false;
                    }

                    //Sum qty of selected events on MTOP daypilot
                    var totalQty = 0;

                    //Keep selected events
                    OBJEVENTMTOPSEL = dpMtop.multiselect.events();
                    if (OBJEVENTMTOPSEL === null || OBJEVENTMTOPSEL.length === 0) {
                        ShowMessage("Create MES Package", "Please select a package to schedule.", ObjMessageType.Info);

                        return false;
                    }

                    //START ADD - SON) 19/Jun/2020 - Checking existing mes packages
                    if (!CheckExistingMesPkgOnScheduler(OBJEVENTMTOPSEL)) {
                        ShowMessage("Create MES Package", "Please select date again to show all MES packages.", ObjMessageType.Info);
                        return false;
                    }
                    //END ADD - SON) 19/Jun/2020

                    var eventFst = dpMtop.multiselect.events()[0].data;
                    var stlInf = eventFst.StyleInf;

                    //Check events (packages) was distributed or not
                    let countPPDistributed = 0;
                    let contPPNotDistributed = 0;
                    let tempPPackage = "";
                    let isSameStyle = true;
                    let temPackageGroup = '';
                    let isSamePkgGroup = true;
                    OBJEVENTMTOPSEL.forEach(function (e) {
                        let eveData = e.data;

                        //Events are selected must be same style code, size, color and revision.
                        if (stlInf !== eveData.StyleInf) {
                            isSameStyle = false;

                            return false; //Break loop
                        }

                        //Check events (packages) was distributed or not
                        let pppObj = LIST_PP_PACKAGE_GROUP.filter(x => x.PPackage === eveData.PrdPkg);
                        if (pppObj.length !== 0) {
                            countPPDistributed++;
                            tempPPackage = pppObj[0].PPackage;

                            //START ADD - SON) 22/Jun/2020 - check the same package group of production pacakges
                            if (temPackageGroup === '') {
                                temPackageGroup = pppObj[0].PackageGroup;
                            } else {
                                if (temPackageGroup !== pppObj[0].PackageGroup) {
                                    isSamePkgGroup = false;
                                    return false;
                                }
                            }
                            //END ADD - SON) 22/Jun/2020

                        } else {
                            contPPNotDistributed++;
                        }
                    });

                    //Check if packages are not same style information then return
                    if (!isSameStyle) {
                        ShowMessage("Create MES Package", "Packages must be same style.", ObjMessageType.Info);
                        return;
                    }

                    //If selected packages include packages which was distributed and was not distributed then return do not allow distribute
                    if (countPPDistributed !== 0 && contPPNotDistributed !== 0) {
                        ShowMessage("Create MES Package", "There is pacakge which was distributed " + tempPPackage, ObjMessageType.Info);
                        return;
                    }

                    //Show aler produciton packages are not same package group
                    if (!isSamePkgGroup) {
                        ShowMessage("Create MES Package", "Production packages are not same package group " + tempPPackage, ObjMessageType.Info);
                        return;
                    }

                    var styleCodeFst = eventFst.StyleCode;
                    var styleSizeFst = eventFst.StyleSize;
                    var styleColorSerialFst = eventFst.StyleColorSerial;
                    var styleRevFst = eventFst.RevNo;

                    let isRemainZero = false;
                    //Check all selected events whether to be same style information or difference
                    OBJEVENTMTOPSEL.forEach(function (e) {
                        var eveData = e.data;
                        totalQty += eveData.PlanQty;

                        //Check production package whether to be created package group or not.
                        $.each(LIST_PP_PACKAGE_GROUP, function (idx, ppPkgGroup) {
                            if (ppPkgGroup.PPackage === eveData.PrdPkg) {

                                //Checking remain quantity in package group
                                let pkgGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === ppPkgGroup.PackageGroup);

                                if (pkgGroup[0].RemainQty > 0) {
                                    totalQty = pkgGroup[0].RemainQty;

                                    return false;
                                } else {
                                    isRemainZero = true;
                                    tempPPackage = ppPkgGroup.PPackage;

                                    return false; //Break loop
                                }
                            }
                        });
                    });
                    //}

                    //If remain quantity of package group then do not distribute packages
                    if (isRemainZero) {
                        ShowMessage("Create MES Package", "Remain quantity is 0. Cannot distribute package " + tempPPackage, ObjMessageType.Info);
                        return;
                    }

                    let prdPkg = OBJEVENTMTOPSEL[0].data.PrdPkg;
                    //Set production package to hidden field
                    $("#hdSelectedPrdPkg").text(prdPkg);

                    //Set total qty on modal
                    $("#txtTotalQty").val(totalQty);
                    //Show modal divide production package quantity
                    $('#mdlPpDivide').modal('show');
                    //Enable drag modal
                    $("#mdlPpDivide").draggable({
                        handle: ".modal-header"
                    });

                    //Get daily target qty from OPS                   
                    let dailyTargetQty = GetDailyTargetQtyFromOPS(facMes, styleCodeFst, styleSizeFst, styleColorSerialFst, styleRevFst);
                    $("#txtDailyTarQty, #txtOPSDailyTarQty").val(dailyTargetQty);
                }
            }
        ]
        //cssClassPrefix: "menu_default"
    });

    dpMtop.init();
    dpMtop.scrollTo(arrDateRange[0]);
}

function initDayPilotMes() {
    var arrDateRange = $("#txtDateRangePp").val().split('-');

    var d1 = new Date(arrDateRange[0].trim());
    var d2 = new Date(arrDateRange[1].trim());
    var difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    dpMes = new DayPilot.Scheduler("dpMes");
    //var dpMes = new DayPilot.Scheduler("dpMes");

    // behavior and appearance
    dpMes.cellWidth = 40;
    dpMes.eventHeight = 25;
    dpMes.headerHeight = 25;

    // view
    dpMes.startDate = arrDateRange[0];
    dpMes.days = difference + 1;
    dpMes.scale = "Day";

    dpMes.rowMarginBottom = 5;

    dpMes.timeHeaders = [
        { "groupBy": "Month" },
        { "groupBy": 'Week' },
        { "groupBy": "Day", "format": "d" }
    ];

    dpMes.eventClickHandling = "Select";

    dpMes.bubble = new DayPilot.Bubble({
        onLoad: function (args) {
            console.log(args);


            //var strHtml = "<strong>Package Information</strong>";
            //strHtml += "</br> Package Group: " + args.source.data.PackageGroup;
            //strHtml += "</br> MxPackage: " + args.source.data.MxPackage;
            //strHtml += "</br> Factory: " + args.source.data.Factory;
            //strHtml += "</br> Line No: " + args.source.data.LineName; //LineNo;
            //strHtml += "</br> AO No: " + args.source.data.AoNo;
            //strHtml += "</br> Style Code: " + args.source.data.StyleCode;
            //strHtml += "</br> Style Size: " + args.source.data.StyleSize;
            //strHtml += "</br> Style Color: " + args.source.data.StyleColorSerial;
            //strHtml += "</br> Rev No: " + args.source.data.RevNo;
            //strHtml += "</br> Stauts: " + args.source.data.MxStatus;
            //strHtml += "</br> Plan Production Date: " + args.source.data.start.toString("yyyy/M/d") + " - " + args.source.data.end.toString("yyyy/M/d");
            //strHtml += "</br> Plan Qty: " + args.source.data.PlanQty; 
            //args.html = strHtml;


            args.async = true;

            var htmlContent = "";
            $.ajax({
                url: '/QCO/POPStyleSummaryMESPackage/?StyleCode=' + args.source.data.StyleCode
                    + '&StyleSize=' + args.source.data.StyleSize
                    + '&StyleColorSerial=' + args.source.data.StyleColorSerial
                    + '&RevNo=' + args.source.data.RevNo
                    + '&MxPackage=' + args.source.data.MxPackage
                    + '&MxTarget=' + args.source.data.MxTarget
            })
                .then(function (content) {
                    //console.log(content);
                    // Set the tooltip content upon successful retrieval
                    htmlContent = content;
                }, function (xhr, status, error) {
                    console.log(xhr, status, error);
                    // Upon failure... set the tooltip content to error
                    htmlContent = status + ': ' + error;
                }).then(() => {
                    args.html = htmlContent;
                    args.loaded();
                });

            //setTimeout(function () {
            //    args.html = htmlContent;
            //    args.loaded();
            //}, 1000);
        }
    });

    dpMes.treeEnabled = true;
    dpMes.rowHeaderWidth = 100;

    //Clear mes package on scheduler
    dpMes.resources = [];
    dpMes.events.list = [];

    dpMes.eventHoverHandling = "Bubble";

    // dp.allowEventOverlap = false;
    //dpMes.allowMultiMove = true;
    //dpMes.multiMoveVerticalMode = "Disabled";

    dpMes.dragOutAllowed = false; //must be true same dpMtop
    dpMes.allowMultiSelect = false;

    dpMes.heightSpec = "Max";
    dpMes.height = 500;

    //#Note: DayPilot Pro
    dpMes.cornerHtml = "LINE";

    // event resizing
    dpMes.onEventResized = function (args) {
        dpMes.message("Resized: " + args.e.text());
    };

    // event moving
    dpMes.onEventMove = function (args) {

        //START ADD - SON) 16/Jul/2020
        if (args.e.data.IsNew !== 'Y') {
            if (isScanedMesPackage(args.e.data.Factory, args.e.data.PlnStartDate, args.e.data.MxPackage)) {
                ShowMessage("Delete MES Package", `This MES package (${args.e.data.MxPackage}) scanned IoT.`, ObjMessageType.Info);
                args.preventDefault();
            }
        }
        //END ADD - SON) 16/Jul/2020

        let curDate = args.newStart.toString('yyyy/MM/dd');

        if (!isWorkingDate(curDate)) {
            ShowMessage("Schedule MES package", "This day does not have working time.", ObjMessageType.Info);

            args.preventDefault();

            console.log("dpMes - onEventMove: This day does not have working time - " + curDate);
        }



        //#region event move package from dpMtop to dpMes
        //eventMove(dpMes, args);

        //console.log("dpMes - onEventMove: " + args.e.calendar);

        //if (args.e.calendar.id === "dpMtop") {

        //    var eData = args.e.data;

        //    //OBJDRAGMESPKG = eData;

        //    //Calculate remain qty
        //    let isRemainZero = false;
        //    let totalQty = 0;

        //    //Check all selected events whether to be same style information or difference
        //    totalQty += eData.PlanQty;

        //    //Check production package whether to be created package group or not.
        //    $.each(LIST_PP_PACKAGE_GROUP, function (idx, ppPkgGroup) {
        //        if (ppPkgGroup.PPackage === eData.PrdPkg) {

        //            //Checking remain quantity in package group
        //            let pkgGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === ppPkgGroup.PackageGroup);

        //            if (pkgGroup[0].RemainQty > 0) {
        //                totalQty = pkgGroup[0].RemainQty;

        //                return false;
        //            } else {
        //                isRemainZero = true;
        //                tempPPackage = ppPkgGroup.PPackage;

        //                return false; //Break loop
        //            }
        //        }
        //    });

        //    //If remain quantity of package group then do not distribute packages
        //    if (isRemainZero) {
        //        removeTemporaryMesPkg();
        //        ShowMessage("Create MES Package", "Remain quantity is 0. Cannot distribute package " + tempPPackage, ObjMessageType.Info);

        //        return;
        //    }

        //    //deselect all
        //    $('#drpLineNo').multiselect('deselectAll', false);
        //    $('#drpLineNo').multiselect('updateButtonText');
        //    $("#txtTotalQty").val(totalQty);
        //    //ISDRAGEVENT = true;
        //    $('#mdlPpDivide').modal('show');
        //    $("#mdlPpDivide").draggable({
        //        handle: ".modal-header"
        //    });
        //}
        //#endregion

    };

    dpMes.onEventMoving = function (args) {
        //args.left.enabled = true;
        //args.left.html = args.e.start().toString();

        //Change background color event on dpMtop and dpMes
        //args.e.data.backColor = "#1a75ff";
        //args.e.data.fontColor = "#ffffff";

        //if (args.overlapping) {
        //    args.right.enabled = true;
        //    args.right.html = "Conflict with an existing event!";
        //}

        //let curDate = args.e.data.start.toString('yyyy/MM/dd');
        //let dropDate = args.data.start.toString('yyyy/MM/dd');
        //console.log('dropDate: ' + dropDate);
        //if (!isWorkingDate(dropDate)) {
        //    args.right.html = "You can't move package to this date";
        //    args.allowed = false;
        //}

        //if (args.e.resource() === "A" && args.resource === "B") {
        //    args.left.enabled = false;
        //    args.right.enabled = true;
        //    args.right.html = "You can't move an event from resource A to B";

        //    args.allowed = false;
        //}

        //console.log("dpMes - onEventMoving: " + args.e.start().toString());
    };

    dpMes.onEventMoved = function (args) {
        //dpMes.message("Event " + args.e.text() + " moved.");      
        //console.log("dpMes - onEventMoved 1: " + OBJDRAGPROPKG.data.start);
        //console.log("dpMes - onEventMoved: " + args.e.text());

        //Enable resizeable event
        args.e.data.resizeDisabled = true;
        let isNew = args.e.data.IsNew;
        //console.log("dpMes - onEventMoved - isnew before: " + isNew);
        args.e.data.IsNew = isNew === "Y" ? isNew : "U";

        //console.log("dpMes - onEventMoved - isnew after: " + args.e.data.IsNew);

        ////Get current distribution mes package
        //let curMesPkg = dpMes.events.find(OBJDRAGMESPKG.id);
        //let lineNo = curMesPkg.data.resource;
        //let strDate = curMesPkg.data.start.toString("yyyy/MM/dd");
        //let endDate = curMesPkg.data.end.toString("yyyyMMdd");
        //$('#drpLineNo').multiselect('select', lineNo);
        ////$("#txtStartDate").val("2015/07/11");
        //$("#txtStartDate").val(strDate);


        //Update original start date and end date of dragged production package
        //OBJDRAGPROPKG.data.start = "2015/07/10"; //str.substring(0, 1);
        //OBJDRAGPROPKG.data.end = "2015/07/10";
        //OBJDRAGPROPKG.data.resource = OBJDRAGPROPKG.data.resource.LineNo;

        //args.e.data.backColor = "#1a75ff";

    };

    //Corlor sunday colum
    dpMes.onBeforeCellRender = function (args) {
        if (args.cell.start.getDayOfWeek() === 0) {
            args.cell.backColor = "#dddddd";
        }
    };

    //Show context menu to delete event
    dpMes.contextMenu = new DayPilot.Menu({
        items: [
            {
                text: "Delete", onclick: function () {
                    let e = this.source;
                    //Get package group id of event (MES package)
                    let pkgGroupId = e.data.PackageGroup;

                    //START ADD - SON) 16/Jul/2020 - don't let user delete scaned MES package
                    if (e.data.IsNew !== 'Y') {
                        if (isScanedMesPackage(e.data.Factory, e.data.PlnStartDate, e.data.MxPackage)) {
                            ShowMessage("Delete MES Package", `This MES package (${e.data.MxPackage}) scanned IoT.`, ObjMessageType.Info);

                            return false;
                        }
                    }
                    //END ADD - SON) 16/Jul/2020

                    //Check event delete whether is creating by QCO or not
                    if (CREATEMESQCO) {
                        eventDeleteMESPackageQco(e);
                    } else {
                        //START change background color of 
                        //Get list of production package in MTOP scheduler
                        let listPpMtop = dpMtop.events.list;

                        //List event need to remove from scheduler
                        let listRemPpMtop = []; //Get list pp on MTOP scheduler to change its background color
                        let listPpByPkgGroup = []; //Keep list production package by package group id
                        for (let i = 0; i < LIST_PP_PACKAGE_GROUP.length; i++) {
                            //Get list production package in current Produciton Package Group list by package group Id.
                            if (LIST_PP_PACKAGE_GROUP[i].PackageGroup === pkgGroupId) {
                                listPpByPkgGroup.push(LIST_PP_PACKAGE_GROUP[i].PPackage);
                            }
                        }

                        //Get list of production packages which were distributed by comparing with current list production package
                        $.each(listPpMtop, function (idx, mtopPkg) {
                            //Find all production package which were deleted on MES schedule.
                            $.each(listPpByPkgGroup, function (idx, pPkg) {
                                if (mtopPkg.PrdPkg === pPkg) {
                                    //Find Mtop event by id
                                    let pp = dpMtop.events.find(pPkg);
                                    listRemPpMtop.push(pp);
                                }
                            });
                        });

                        //Remove mtop production package and add it again after changing background color
                        $.each(listRemPpMtop, function (idx, mtopPkg) {
                            dpMtop.events.remove(mtopPkg);

                            //Put css and adding again
                            mtopPkg.data.cssClass = "event-style";
                            mtopPkg.data.rightClickDisabled = false;
                            dpMtop.events.add(mtopPkg);

                        });
                        //END change background color of

                        //Remove all production package in package group
                        let removePp = false;

                        //Recalculate remain quantity and remove package group in temporary list.
                        $.each(LIST_PACKAGE_GROUP, function (idx, pkgGroup) {
                            if (pkgGroup.PackageGroup === pkgGroupId) {
                                pkgGroup.RemainQty = pkgGroup.RemainQty + e.data.PlanQty;

                                //If remain qty and target qty are same then remove package in temporary list
                                if (pkgGroup.RemainQty === pkgGroup.TargetQty) {
                                    //Remove object package group from the list
                                    LIST_PACKAGE_GROUP.splice(idx, 1);
                                    //Reduce package group sequence
                                    CUR_PACKAGE_SEQ--;

                                    //Set remove production package if delete all MES package in 1 package group on MES scheduler
                                    removePp = true;
                                }

                                //Reduce mes package group sequence
                                CUR_NUMBER_MES_PKG--;

                                return false;
                            }
                        });

                        let listRevPP = [];
                        if (removePp === true) {
                            //Add list of production package group to the list which needs to remove.                      
                            for (let i = 0; i < LIST_PP_PACKAGE_GROUP.length; i++) {
                                if (LIST_PP_PACKAGE_GROUP[i].PackageGroup === pkgGroupId) {
                                    listRevPP.push(LIST_PP_PACKAGE_GROUP[i].PPackage);
                                }
                            }

                            //Remove production package group in current list
                            $.each(listRevPP, function (idx, ppId) {
                                $.each(LIST_PP_PACKAGE_GROUP, function (idex, pp) {
                                    if (pp.PPackage === ppId) {
                                        LIST_PP_PACKAGE_GROUP.splice(idex, 1);
                                        return false;
                                    }
                                });
                            });
                        }

                        //Keep list of deleted mes packages. If event is new (Y) then don't need to add to list deleting
                        if (e.data.IsNew !== "Y") {
                            LIST_DELETED_MES_PACKAGE.push(e.data);
                        }

                        //START ADD) SON (2019.09.17) - 18 September 2019 - remove MES sequence
                        $.each(LIST_MES_SEQ, function (idx, mesSeq) {
                            if (mesSeq.MxPackage === e.data.MxPackage) {
                                //Remove mes sequence in current list.
                                LIST_MES_SEQ.splice(idx, 1);

                                //Return after removing object in the list
                                return false;
                            }
                        });
                        //END ADD) SON (2019.09.17) - 18 September 2019 -

                        dpMes.events.remove(e);
                    }
                }
            }
        ]
        //cssClassPrefix: "menu_default"
    });

    dpMes.init();
    dpMes.scrollTo(arrDateRange[0]);
}

function eventDeleteMESPackageQco(e) {
    let pkgGroupId = e.data.PackageGroup;
    let mesQty = e.data.PlanQty;
    //START change background color of 
    //Get list of production package
    let listPpMtop = QCO_CURRENT_LIST;

    let mxPkg = e.data.MxPackage;
    let ppkg = mxPkg.substring(0, 32);
    ppkg = "P" + ppkg.substr(1, 31);

    //let ppQco = QCO_CURRENT_LIST.filter(x => x.PrdPkg === ppkg);
    let ppQco = QCO_CURRENT_LIST.filter(x => x.PrdPkg === ppkg);
    ppQco[0].RemainQty += mesQty;

    //console.log("mxpkg: " + mxPkg + " - mesQty: " + mesQty); 
    //console.log("ppkg: " + ppkg + " - New Qty: " + ppQco.PlanQty);

    ////List event need to remove from scheduler
    //let listRemPpMtop = [];
    //let listPpByPkgGroup = [];
    //for (let i = 0; i < LIST_PP_PACKAGE_GROUP.length; i++) {
    //    if (LIST_PP_PACKAGE_GROUP[i].PackageGroup === pkgGroupId) {
    //        listPpByPkgGroup.push(LIST_PP_PACKAGE_GROUP[i].PPackage);
    //    }
    //}

    ////Get list of production packages which were distributed
    //$.each(listPpMtop, function (idx, mtopPkg) {
    //    //Find all production package which were deleted on MES schedule.
    //    $.each(listPpByPkgGroup, function (idx, pPkg) {
    //        if (mtopPkg.PrdPkg === pPkg) {               
    //            listRemPpMtop.push(pPkg);
    //        }
    //    });
    //});

    ////Remove mtop production package and add it again after changing background color
    //$.each(listRemPpMtop, function (idx, mtopPkg) {
    //    dpMtop.events.remove(mtopPkg);

    //    //Put css and adding again
    //    mtopPkg.data.cssClass = "event-style";
    //    mtopPkg.data.rightClickDisabled = false;
    //    dpMtop.events.add(mtopPkg);

    //});
    ////END change background color of

    //Remove all production package in package group
    let removePp = false;

    //Recalculate remain quantity and remove package group in temporary list.
    $.each(LIST_PACKAGE_GROUP, function (idx, pkgGroup) {
        if (pkgGroup.PackageGroup === pkgGroupId) {
            pkgGroup.RemainQty = pkgGroup.RemainQty + e.data.PlanQty;

            //If remain qty and target qty are same then remove package in temporary list
            if (pkgGroup.RemainQty === pkgGroup.TargetQty) {
                //Remove object package group from the list
                LIST_PACKAGE_GROUP.splice(idx, 1);
                //Reduce package group sequence
                CUR_PACKAGE_SEQ--;

                removePp = true;
            }

            //Reduce mes package group sequence
            CUR_NUMBER_MES_PKG--;

            return false;
        }
    });

    let listRevPP = [];
    if (removePp === true) {
        //Add list of production package group                       
        for (let i = 0; i < LIST_PP_PACKAGE_GROUP.length; i++) {
            if (LIST_PP_PACKAGE_GROUP[i].PackageGroup === pkgGroupId) {
                listRevPP.push(LIST_PP_PACKAGE_GROUP[i].PPackage);
            }
        }

        //Remove production package group
        $.each(listRevPP, function (idx, ppId) {
            $.each(LIST_PP_PACKAGE_GROUP, function (idex, pp) {
                if (pp.PPackage === ppId) {
                    LIST_PP_PACKAGE_GROUP.splice(idex, 1);
                    return false;
                }
            });
        });
    }

    //Keep list of deleted mes packages
    if (e.data.IsNew !== "Y") {
        LIST_DELETED_MES_PACKAGE.push(e.data);
    }

    dpMes.events.remove(e);

    //START MOD) SON (2019.09.17) - 18 September 2019 - reload QCO gridview base on slider
    ReloadQCOGridBySlider();

    //Reload production package by QCO
    //ReloadJqGridLocal(tableQCOPPKGName, QCO_CURRENT_LIST);
    //START MOD) SON () - 18 September 2019 
}

function removeProductionPackage(listPP, PPPkgGroup) {
    $.each(listPP, function (idx, PPPkgGroup) {
        if (PPPkgGroup.PackageGroup === pkgGroupId) {
            listPP.splice(idx, 1);
            return false;
        }
    });
}

function eventMove(target, args) {
    var source = args.e.calendar;
    var srcData = args.e.data;

    if (target !== source) {
        //Must remove event from source and add again to avoid bug
        source.events.remove(args.e);
        //Create new event (get data from original) for dp Mtop
        if (source.id === "dpMtop") {
            var newEvent = new DayPilot.Event({
                start: srcData.start,
                end: srcData.end,
                text: srcData.text,
                resource: srcData.resource,
                id: srcData.id,
                cssClass: "event-style",
                resizeDisabled: true,
                barHidden: true,
                moveDisabled: true,

                AdType: srcData.AdType,
                AdTypeName: srcData.AdTypeName,
                AoNo: srcData.AoNo,
                Buyer: srcData.Buyer,
                DeliveryDate: srcData.DeliveryDate,
                Destination: srcData.Destination,
                Factory: srcData.Factory,
                LineNo: srcData.LineNo,
                OrdQty: srcData.OrdQty,
                PlanQty: srcData.PlanQty,
                PrdEdat: srcData.PrdEdat,
                PrdPkg: srcData.PrdPkg,
                PrdSdat: srcData.PrdSdat,
                Rank: srcData.Rank,
                RevNo: srcData.RevNo,
                StyleCode: srcData.StyleCode,
                StyleColorSerial: srcData.StyleColorSerial,
                StyleSize: srcData.StyleSize,
                StyleInf: srcData.StyleInf,
                backColor: srcData.backColor
            });
            source.events.add(newEvent);
        }
    }
}

/**
 * Remove event on daypilot
 * @param {any} dayPilotId id of schedule
 * @param {any} eveId event id on schedule
 */
function removeEvent(dayPilotId, eveId) {
    if (!$.isEmptyObject(eveId)) {
        var e = dayPilotId.events.find(eveId);
        dayPilotId.events.remove(e).queue();
    }
}

function removeTemporaryMesPkg() {
    if (!$.isEmptyObject(OBJDRAGMESPKG)) {
        var e = dpMes.events.find(OBJDRAGMESPKG.id);
        if (e !== null) {
            dpMes.events.remove(e);
        }

        //Clear temporary mes package
        OBJDRAGMESPKG = null;
        OBJEVENTMTOPSEL = [];
        OBJDRAGPROPKG = null;

    }
}

/**
 * Get max Seq of MES package base on package group
 * @param {any} packageGroupId production package group
 * @param {any} callBack function callback
 */
function GetMaxMesSeqPkgGroup(packageGroupId, callBack) {
    var config = ObjectConfigAjaxPost("../Planning/GetMaxMesSeqPkgGroup", false
        , JSON.stringify({ packageGroupId: packageGroupId }));
    AjaxPostCommon(config, function (maxSeq) {
        callBack(maxSeq);
    });
}

//#endregion get data MES and production packages

//#region Get production package

function GetFactoryLinesByFactoryIdMySql(factoryId, callBack) {
    var newArrLine = [];

    var config = ObjectConfigAjaxPost("../Planning/GetLinesByFactoryIdMySql", false, JSON.stringify({ factoryId: factoryId }));
    AjaxPostCommon(config, function (lstLine) {
        $.each(lstLine, function (index, value) {
            var line = { name: value.LineNo, id: value.LineNo };

            newArrLine.push(line);
        });

        callBack(newArrLine);
    });
}

function GetFactoryLines(factoryId, startDate, endDate, buyer, styleInfo, aoNo, callBack) {
    var newArrLine = [];

    var config = ObjectConfigAjaxPost("../Planning/GetLinesByProPackage", false
        , JSON.stringify({ factoryId: factoryId, startDate: startDate, endDate: endDate, buyer: buyer, styleInfo: styleInfo, aoNo: aoNo }));
    AjaxPostCommon(config, function (lstLine) {
        $.each(lstLine, function (index, value) {
            var line = { name: value.LineNo, id: value.LineNo };

            newArrLine.push(line);
        });

        callBack(newArrLine);
    });
}

function GetMESLinesByFactory(factory) {
    var arrMESLine = [];

    var config = ObjectConfigAjaxPost("../Planning/GetMESLinesByFactoryMySql", false
        , JSON.stringify({ factory: factory }));
    AjaxPostCommon(config, function (lstLine) {
        $.each(lstLine, function (index, value) {
            var line = { name: value.LineName, id: value.LineCombination };

            arrMESLine.push(line);
        });
    });

    return arrMESLine;
}

function GetListOfProductionPackageByAoNoAndFactory(factoryId, startDate, endDate, buyer, styleInfo, aoNo, callBack) {
    var config = ObjectConfigAjaxPost(
        "../Planning/GetListPackageGroupByAoNoFactory",
        false,
        JSON.stringify({
            mesFac: factoryId,
            startDate: startDate,
            endDate: endDate,
            ppFactory: factoryId,
            aoNo: aoNo,
            buyer: buyer,
            styleInfo: styleInfo
        })
    );

    AjaxPostCommon(config, function (lstPp) {
        callBack(lstPp);
    });

}

function GetProductionPackage(factoryId, startDate, endDate, buyer, styleInfo, aoNo, callBack) {

    let listMesProPkg;
    //Get list production pacakge which was scheduled in MySql
    GetListOfProductionPackageByAoNoAndFactory(factoryId, startDate, endDate, buyer, styleInfo, aoNo, function (listMesPP) {
        listMesProPkg = listMesPP;
    });

    //Get list prodcttion package from AOMTOP
    var config = ObjectConfigAjaxPost("../Planning/GetProductionPackage", false
        , JSON.stringify({ factoryId: factoryId, startDate: startDate, endDate: endDate, buyer: buyer, styleInfo: styleInfo, aoNo: aoNo }));
    AjaxPostCommon(config, function (lstMtopProPkg) {

        var distPP = [];
        var indPp = 0; //Set index to get color

        $.each(lstMtopProPkg, function (index, value) {

            //Check style information
            var stlInf = value.StyleInf;
            //var stlKey = value.StyleCode + value.StyleSize + value.StyleColorSerial + value.RevNo;
            var curIdx = distPP.indexOf(stlInf);
            if (curIdx === -1) {
                distPP.push(stlInf);
                curIdx = distPP.length - 1; //Get the last color index
            }

            //Get color in array
            var clo = EVENTCOLOR[curIdx];

            //Check color exist in array or not, if it does not exist then adding color to array
            if (typeof clo === "undefined") {
                clo = getRandomColor();
                EVENTCOLOR.push(clo);
            }

            //console.log(curIdx + " - " + clo + EVENTCOLOR[curIdx] === "undefined");

            value.id = value.PrdPkg;
            value.resource = value.LineNo;
            value.start = value.PrdSdat.substr(0, 4) + "-" + value.PrdSdat.substr(4, 2) + "-" + value.PrdSdat.substr(6, 2);
            value.end = value.PrdEdat.substr(0, 4) + "-" + value.PrdEdat.substr(4, 2) + "-" + value.PrdEdat.substr(6, 2);
            value.text = value.PrdPkg;

            value.moveDisabled = true; //Prevent moving event
            value.resizeDisabled = true; //Prevent resize event
            value.barHidden = true; //Hide bar of event
            value.cssClass = "event-style"; //Set css for event
            value.backColor = clo;

            value.RemainQty = value.PlanQty; //2019-12-14 Tai Le (Thomas)
            //console.log(listPpByAoFac.length);

            $.each(listMesProPkg, function (idx, pp) {
                //console.log(' $.each(listPpByAoFac');
                //console.log(idx, pp);
                //D: Done distributed
                if (pp.PPackage === value.PrdPkg) {
                    if (pp.DistributeStatus === "D") {
                        //Disable right click
                        value.rightClickDisabled = true;
                        value.cssClass = "distributed";
                        value.moveDisabled = true;
                    } else {
                        value.barHidden = false; //Hide bar of event
                        //START ADD) SON - 13 December 2019 - set remail quatity
                        value.RemainQty = pp.RemainQty;
                        //END ADD) SON - 13 December 2019 - set remail quatity 
                    }
                }
            });

        });
        callBack(lstMtopProPkg);
    });
}

function GetMesPackages(mesFac, startDate, endDate, ppFactory, aoNo, buyer, styleInf, callBack) {
    //string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
    var config = ObjectConfigAjaxPost("../Planning/GetMESPackage", false
        , JSON.stringify({ mesFac: mesFac, startDate: startDate, endDate: endDate, ppFactory: ppFactory, aoNo: aoNo, buyer: buyer, styleInf: styleInf }));
    AjaxPostCommon(config, function (listMpmt) {

        let distPP = [];
        let indPp = 0; //Set index to get color

        //Get list of Mes package in list of group package
        let listMes = [];
        let listProPkgGroup = [];
        let listPkgGroup = [];
        $.each(listMpmt, function (idx, mpmt) {
            $.each(mpmt.ListMpdt, function (idex, mpdt) {
                listMes.push(mpdt);
            });

            //Get distributed package group
            $.each(mpmt.ListPpkg, function (idx, ppkg) {
                listProPkgGroup.push(ppkg);
            });
        });

        LIST_PACKAGE_GROUP = listMpmt;
        LIST_PP_PACKAGE_GROUP = listProPkgGroup;

        $.each(listMes, function (index, value) {

            //Check style information
            var stlInf = value.StyleInf;
            //var stlKey = value.StyleCode + value.StyleSize + value.StyleColorSerial + value.RevNo;
            var curIdx = distPP.indexOf(stlInf);
            if (curIdx === -1) {
                distPP.push(stlInf);
                curIdx = distPP.length - 1; //Get the last color index
            }

            //Get color in array
            var clo = EVENTCOLOR[curIdx];

            //Check color exist in array or not, if it does not exist then adding color to array
            if (typeof clo === "undefined") {
                clo = getRandomColor();
                EVENTCOLOR.push(clo);
            }

            value.id = value.PackageGroup + "_" + value.SeqNo;
            //value.resource = value.LineNo;
            value.resource = value.LineCombination;
            value.start = value.PlnStartDate.substr(0, 4) + "-" + value.PlnStartDate.substr(4, 2) + "-" + value.PlnStartDate.substr(6, 2);
            value.end = value.PlnEndDate.substr(0, 4) + "-" + value.PlnEndDate.substr(4, 2) + "-" + value.PlnEndDate.substr(6, 2);
            value.text = value.MxPackage;

            value.PlanQty = value.MxTarget;

            value.resizeDisabled = true; //Prevent resize event
            value.barHidden = true; //Hide bar of event
            value.backColor = clo;

            //Check status of MES package
            if (value.MxStatus !== "RO") {
                value.moveDisabled = true; //Prevent moving event
                value.rightClickDisabled = true; //Disable right click
                value.cssClass = "distributed";
            } else {
                value.moveDisabled = ISUPDATED ? false : true; //Prevent moving event
                value.rightClickDisabled = ISUPDATED ? false : true; //Disable right click
                value.cssClass = "event-style"; //Set css for event
            }

        });
        callBack(listMes);
    });
}
//#endregion

//#region bind data to gridview

//Bind data to grid mes package
function bindDataToJqGridMesPackage(packageGroup) {

    jQuery(tableMesPackageId).jqGrid({
        url: '/MesManagement/GetMesPackages',
        postData: {
            packageGroup: packageGroup
        },
        datatype: "json",
        type: 'POST',
        height: 'auto',
        colModel: [
            { name: 'MxPackage', index: 'MxPackage', label: "MES P. Packages", width: 200, classes: 'pointer' },
            { name: 'LineNo', index: 'LineNo', label: "Line No", width: 70, align: 'center', classes: 'pointer', hidden: true },
            { name: 'LineName', index: 'LineName', label: "MES Line", width: 70, align: 'center', classes: 'pointer' },
            { name: 'PlnStartDate', index: 'PlnStartDate', label: "Start Plan" },
            //{ name: 'PlnStartDate', index: 'PlnStartDate', label: "Start Plan", formatter: "date", formatoptions: { srcformat: "d-m-Y H:i:s", newformat: "d-m-Y H:i:s" } },
            { name: 'MxTarget', index: 'MxTarget', label: "Target Qty", width: 70, align: 'center', classes: 'pointer' },
            { width: 100, label: "Confirm", align: 'center', formatter: formatConfirmMesPackage, classes: 'pointer' },
            { name: 'PackageGroup', index: 'PackageGroup', hidden: true },
            { name: 'SeqNo', index: 'SeqNo', hidden: true },
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
        onSelectRow: function (rowid) {
            //Get row data
            //var rowdata = $(tableMesPackageId).jqGrid("getRowData", rowid);

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

    function formatConfirmMesPackage(cellvalue, options, rowObject) {
        let mxStatus = rowObject.MxStatus;
        if (mxStatus !== STATUSMESPACKAGEOBJ.Open) {
            //return mxStatus;
            return rowObject.StatusName;
        }

        return "<button type='button' class='btn btn-primary btn-modal' onclick='ConfirmMesPackage(" + JSON.stringify(rowObject) + ")'>" +
            "<i class='glyphicon glyphicon-lock'></i> Confirm </button>";
    }
}

function ConfirmMesPackage(rowData) {
    //Get package group information
    let objPkgGroup = GetSelectedOneRowData(tableGroupPackageId);
    //Get MES operation plan 
    let opmtMes = GetOperationPlanMES(objPkgGroup.StyleCode, objPkgGroup.StyleSize, objPkgGroup.StyleColorSerial, objPkgGroup.RevNo, rowData.MxPackage);
    if (opmtMes === null) {
        ShowMessage("Confirm MES package", "Please register MES operation plan.", ObjMessageType.Info);
        return;
    }

    ShowConfirmYesNo(
        "Confirm MES package"
        , "Are you sure?"
        , function () {
            if (rowData.MxStatus === STATUSMESPACKAGEOBJ.Open) {

                let pkgGroup = rowData.PackageGroup;
                let seqNo = rowData.SeqNo;
                let mesStatus = STATUSMESPACKAGEOBJ.Confirmed;
                let confirmedId = $("#hdUserId").val();

                var config = ObjectConfigAjaxPost("../MesManagement/UpdateMesPackageStatus", false
                    , JSON.stringify({ packageGroup: pkgGroup, seqNo: seqNo, mesStatus: mesStatus, confirmedId: confirmedId }));
                AjaxPostCommon(config, function (resUpd) {

                    if (resUpd === Success) {
                        var params = { packageGroup: pkgGroup };
                        ReloadJqGrid2LoCal(tableMesPackageName, params);
                        ShowMessage("Confirm MES package", "Confirmed", ObjMessageType.Info);
                    } else {
                        ShowMessage("Confirm MES package", resUpd, ObjMessageType.Error);
                    }

                });
            }
        }
        , function () { }
    );
}

//function GetProductioPkgByQco(qcoFactory, qcoYear, qcoWeekNo, callBack) {
//    var config = ObjectConfigAjaxPost("../Planning/GetProductionPackageByQco", true
//        , JSON.stringify({ qcoFactory: qcoFactory, qcoYear: qcoYear, qcoWeekNo: qcoWeekNo}));
//    AjaxPostCommon(config, function (listQco) {
//        callBack(listQco);
//    });
//}

function GetProductionPackageByQco(qcoFactory, qcoYear, qcoWeekNo, buyer, aoNo, styleInf, callBack) {
    var config = ObjectConfigAjaxPost("../Planning/GetProductionPackageByQco", false
        , JSON.stringify({ qcoFactory: qcoFactory, qcoYear: qcoYear, qcoWeekNo: qcoWeekNo, buyer: buyer, aoNo: aoNo, styleInf: styleInf }));
    AjaxPostCommon(config, function (qcoList) {
        var distPP = [];
        //Start create color for MES package
        $.each(qcoList, function (idx, value) {

            //Check style information
            var stlInf = value.StyleInf;
            var curIdx = distPP.indexOf(stlInf);
            if (curIdx === -1) {
                distPP.push(stlInf);
                curIdx = distPP.length - 1; //Get the last color index
            }

            //Get color in array
            let clo = EVENTCOLOR[curIdx];

            //Check color exist in array or not, if it does not exist then adding color to array
            if (typeof clo === "undefined") {
                clo = getRandomColor();
                EVENTCOLOR.push(clo);
            }

            value.backColor = clo;
        });
        //End create color for MES package
        QCO_CURRENT_LIST = qcoList;
        callBack(qcoList);
    });
}

//Bind data to grid group packages
function bindDataToJqGridProductionPackageByQco(qcoFactory, qcoYear, qcoWeekNo, aoNo, buyer, styleInf) {
    let data;
    GetProductionPackageByQco(qcoFactory, qcoYear, qcoWeekNo, buyer, aoNo, styleInf, function (listQco) {
        data = listQco;
    });

    jQuery(tableQCOPPKGId).jqGrid({
        data: data,
        datatype: "local",
        colModel: [
            { name: 'Schedule', width: 100, label: "Schedule", align: 'center', formatter: formatScheduleProductionPackage, classes: 'pointer' },
            { name: 'QCORank', index: 'QCORank', width: 50, label: "QCO Rank", classes: 'pointer' },
            { name: 'LineNo', index: 'LineNo', width: 50, label: "Line No", classes: 'pointer' },
            { name: 'PrdPkg', index: 'PrdPkg', width: 230, label: "Production package", align: 'center', classes: 'pointer' },
            { name: 'PlanQty', index: 'PlanQty', width: 70, label: "Plan Qty", align: 'center', classes: 'pointer' },
            { name: 'PrdSDat', index: 'PrdSDat', width: 100, label: "Start date", align: 'center', classes: 'pointer' },
            { name: 'PrdEDat', index: 'PrdEDat', width: 100, label: "End date", align: 'center' },
            { name: 'AoNo', index: 'AoNo', width: 100, label: "AO No", align: 'center', classes: 'pointer' },
            { name: 'DeliveryDate', index: 'DeliveryDate', width: 100, label: "Deliery Date", align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "m-d-Y", newformat: "Y-m-d" } },
            { name: 'NormalizedPercent', index: 'NormalizedPercent', width: 100, label: "Material Readiness", align: 'center', classes: 'pointer' },
            { name: 'StyleCode', index: 'StyleCode', width: 70, label: "Style Code", align: 'center', classes: 'pointer' },
            { name: 'StyleSize', index: 'StyleSize', width: 70, label: "Style Size", align: 'center', classes: 'pointer' },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', width: 70, label: "Color", align: 'center', classes: 'pointer' },
            { name: 'RevNo', index: 'RevNo', width: 70, label: "Rev No", align: 'center', classes: 'pointer' },
            { name: 'StyleInf', index: 'StyleInf', hidden: true },
            { name: 'backColor', index: 'backColor', hidden: true },
            { name: 'RemainQty', index: 'RemainQty', hidden: true }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        pager: paperQCOPPKGId,
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        width: null,
        shrinkToFit: false,
        height: "auto",
        onSelectRow: function (rowid) {
            //var rowData = $(this).jqGrid("getLocalRow", rowid), str = "", p;
            //for (p in rowData) {
            //    if (rowData.hasOwnProperty(p)) {
            //        str += "propery \"" + p + "\" + have the value \"" + rowData[p] + "\"\n";
            //    }
            //}
            //alert("all properties of selected row having id=\"" + rowid + "\":\n\n" + str);
        }
    });

    //jQuery(tableQCOPPKGId).jqGrid({
    //    url: '/Planning/GetProductionPackageByQco',
    //    postData: {
    //        qcoFactory: qcoFactory, qcoYear: qcoYear, qcoWeekNo: qcoWeekNo
    //    },
    //    datatype: "json",
    //    height: 'auto',
    //    colModel: [
    //        { name: 'Schedule', width: 100, label: "Schedule", align: 'center', formatter: formatScheduleProductionPackage, classes: 'pointer' },
    //        { name: 'QCORank', index: 'QCORank', width: 50, label: "QCO Rank", classes: 'pointer' },
    //        { name: 'LineNo', index: 'LineNo', width: 50, label: "Line No", classes: 'pointer' },
    //        { name: 'PrdPkg', index: 'PrdPkg', width: 230, label: "Production package", align: 'center', classes: 'pointer' },
    //        { name: 'PlanQty', index: 'PlanQty', width: 70, label: "Plan Qty", align: 'center', classes: 'pointer' },
    //        { name: 'PrdSDat', index: 'PrdSDat', width: 100, label: "Start date", align: 'center', classes: 'pointer' },
    //        { name: 'PrdEDat', index: 'PrdEDat', width: 100, label: "End date", align: 'center' },
    //        { name: 'AoNo', index: 'AoNo', width: 100, label: "AO No", align: 'center', classes: 'pointer' },
    //        { name: 'StyleCode', index: 'StyleCode', width: 70, label: "Style Code", align: 'center', classes: 'pointer' },
    //        { name: 'StyleSize', index: 'StyleSize', width: 70, label: "Style Size", align: 'center', classes: 'pointer' },
    //        { name: 'StyleColorSerial', index: 'StyleColorSerial', width: 70, label: "Color", align: 'center', classes: 'pointer' },
    //        { name: 'RevNo', index: 'RevNo', width: 70, label: "Rev No", align: 'center', classes: 'pointer' },     
    //        { name: 'StyleInf', index: 'StyleInf', hidden: true },     
    //    ],
    //    rowNum: 10,
    //    rowList: [10, 20, 30],
    //    pager: paperQCOPPKGId,
    //    sortname: 'id',
    //    toolbarfilter: true,
    //    viewrecords: true,
    //    sortorder: "asc",
    //    loadonce: true,
    //    width: null,
    //    shrinkToFit: false,
    //    gridComplete: function () {

    //    },
    //    loaderror: function (xhr, status, err) {
    //        alert("error - get group package: " + err);
    //    },
    //    beforeSelectRow: function (rowid, e) {

    //    },
    //    onSelectRow: function (rowid) {
    //        const dataRow = $(tableQCOPPKGId).jqGrid("getRowData", rowid);

    //    },
    //    loadcomplete: function () {

    //    },

    //});


    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    function formatScheduleProductionPackage(cellvalue, options, rowObject) {
        let pPkg = rowObject.PrdPkg;

        if (rowObject.RemainQty === 0) return "";

        return "<button type='button' class='btn btn-primary' onclick='ScheduleProPackageQCO(" + JSON.stringify(rowObject) + ")'> Schedule </button>";

        //<i class='glyphicon glyphicon-lock'></i>
    }

}

function reloadProductionPackageByQco(qcoFactory, qcoYear, qcoWeekNo, buyer, aoNo, styleInf) {

    var params = { qcoFactory: qcoFactory, qcoYear: qcoYear, qcoWeekNo: qcoWeekNo, buyer: buyer, aoNo: aoNo, styleInf: styleInf };
    ReloadJqGrid2LoCal(tableQCOPPKGName, params);
}
//#endregion

//#region Event line no distribute
//Fill data to dropdownlist
function FillDataMultipleSelectLineNoMdl(idDropdownlist, arrDataSource, valueField, textFiled) {
    $('#' + idDropdownlist).multiselect('destroy');
    $('#' + idDropdownlist).empty();
    var option = '';
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i][valueField] + '">' + arrDataSource[i][textFiled] + '</option>';
    }
    $('#' + idDropdownlist).append(option);

    //Format dropdownlist to selection
    MultipleSelectLineNoMdl(idDropdownlist);
}

//Set multiple select for selection
function MultipleSelectLineNoMdl(idDropdownlist) {
    $("#" + idDropdownlist).multiselect({
        includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: '100%',
        maxHeight: 300,
        buttonClass: 'btn-multiple-select',
        onChange: function (option, checked, select) {
            //var disRate = $("#txtLineDisRate").val();
            var dayTar = $("#txtDailyTarQty").val();
            var total = $("#txtTotalQty").val();

            var numLine = $("#" + idDropdownlist).val().length;

            var targetQty = numLine * dayTar;
            if (targetQty > total && targetQty - total >= dayTar) {
                $("#" + idDropdownlist).multiselect('deselect', option[0].value, true);
                ShowMessage("Select Line", "You cannot distribute execed " + (numLine - 1) + " line(s)", ObjMessageType.Info);
                return false;
            }

            //var per = (((numLine * dayTar) / total) * 100).toFixed(2);
            //$("#txtLineDisRate").val(per);
        }
    });
}

function ScheduleProPackageQCO(rowObject) {
    //Clear selected QCO before asign value.
    SELECTEDROWQCO = [];

    SELECTEDROWQCO.push(rowObject);

    //Set production package in hidden field
    $("#hdSelectedPrdPkg").text(rowObject.PrdPkg);

    //Get daily target qty from OPS           
    let dailyTargetQty = GetDailyTargetQtyFromOPS(rowObject.QCOFactory, rowObject.StyleCode, rowObject.StyleSize, rowObject.StyleColorSerial, rowObject.RevNo);
    $("#txtDailyTarQty").val(dailyTargetQty);

    //Set total qty on modal
    $("#txtTotalQty").val(rowObject.RemainQty);
    //Show modal divide production package quantity
    $('#mdlPpDivide').modal('show');
}

function isProductionPackageAvailable(factoryId, productionPackage) {
    let isAvailable;
    var config = ObjectConfigAjaxPost("../Planning/IsProductionPackageAvailable", false, JSON.stringify({ factoryId: factoryId, prdPkg: productionPackage }));
    AjaxPostCommon(config, function (res) {
        isAvailable = res;
    });

    return StringToBoolean(isAvailable);
}

//#endregion

//#region Functions
/**
 * Get max package group base on factory id and current year and month
 * @param {any} factoryId factory of package group
 * @param {any} yymm year and month
 * @returns {any} max package group
 */
function GetMaxPackageGroup(factoryId, yymm) {
    var pkgGroup = "";
    var config = ObjectConfigAjaxPost("../Planning/GetMaxPackageGroup", false
        , JSON.stringify({ factoryId: factoryId, yymm: yymm }));
    AjaxPostCommon(config, function (retPkgGroup) {
        pkgGroup = retPkgGroup;
    });

    return pkgGroup;
}

function ClearDataOnMTopMesScheduler() {
    let arrDateRange = $("#txtDateRangePp").val().split('-');
    //let d1 = new Date(arrDateRange[0].trim());
    //let d2 = new Date(arrDateRange[1].trim());
    //let difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));


    //dpMtop.startDate = arrDateRange[0];
    //dpMtop.days = difference + 1;
    //dpMtop.scale = "Day";

    dpMtop.resources = [];
    dpMtop.events.list = [];
    dpMtop.startDate = arrDateRange[0];
    dpMtop.update();
    //dpMtop.scrollTo("");

    //Daypilot MES
    dpMes.resources = [];
    dpMes.events.list = [];
    dpMes.startDate = arrDateRange[0];
    dpMes.update();
    //dpMes.scrollTo("");

    //Clear grid QCO
    ReloadJqGridLocal(tableQCOPPKGName, []);
}

function ClearTemporaryScheduleMesPackage() {
    OBJEVENTMTOPSEL = [];
    LIST_PACKAGE_GROUP = [];
    LIST_PP_PACKAGE_GROUP = [];
    LIST_DELETED_MES_PACKAGE = [];
    LIST_MES_SEQ = []; //ADD) SON (2019.09.17) - 17 September 2019 - clear list mes sequence

    CUR_NUMBER_MES_PKG = 0;
    CUR_PACKAGE_SEQ = 0;
}

function checkDataCreateMESPackage() {
    let lineNo = $("#drpLineNo").val();
    let dailyTarQty = parseInt($("#txtDailyTarQty").val());
    let numLines = lineNo.length;
    let factoryMes = $("#drpFactoryMes").val();
    let totalQty = parseInt($("#txtTotalQty").val());

    //Check line no
    if ($.isEmptyObject(lineNo)) {
        ShowMessage("Create MES package", "Please choose line number.", ObjMessageType.Info);
        return false;
    }

    //Check daily quantity
    if ($.isEmptyObject(dailyTarQty) || isNaN(dailyTarQty)) {
        ShowMessage("Create MES package", "Please check daily target Qty.", ObjMessageType.Info);
        return false;
    }

    if ($.isEmptyObject(factoryMes)) {
        ShowMessage("Create MES package", "Please select Mes factory.", ObjMessageType.Info);
        return false;
    }

    return true;
}

function GetDailyTargetQtyFromOPS(factory, styleCode, styleSize, styleColorSerial, revNo) {
    let targetQty = 0;
    let config = ObjectConfigAjaxPost("../Planning/GetDailyTargetFromOPS", false
        , JSON.stringify({ factory: factory, styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo }));
    AjaxPostCommon(config, function (dailyTarget) {
        targetQty = dailyTarget;
    });

    return targetQty;
}

function GetCombinationByCorpAndFactory(corpCode, factory) {
    let combineSta = "N";
    //let config = ObjectConfigAjaxPost("../Planning/GetCombinationByCorpAndFactory", false
    //    , JSON.stringify({ corpCode: corpCode, factory: factory }));
    let config = ObjectConfigAjaxPost("../Planning/GetCombinationByCorpAndFactory", false
        , JSON.stringify({}));
    AjaxPostCommon(config, function (comRes) {
        combineSta = comRes;
    });

    return combineSta;
}

function CopyStyleInfomation(styleCode, styleSize, styleColorSerial, revNo) {
    let config = ObjectConfigAjaxPost("../Planning/CopyStyleInfomationAsync", true
        , JSON.stringify({ styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo }));
    AjaxPostCommon(config, function (copyRes) {
        console.log(copyRes);
    });
}

//checking MES package whether it has any changing or not
function isChangingMESPackage() {

    let isChange = false;

    if (LIST_DELETED_MES_PACKAGE.length !== 0) {
        return true;
    }

    //Get list of MES package on Schedule
    var lstMesEve = dpMes.events.list;

    if (lstMesEve.length === 0) {
        isChange = false;
        return false;
    } else {
        //Check MES packages whether it has New or Updated package or not
        $.each(lstMesEve, function (idx, eve) {
            if (eve.IsNew === "Y" || eve.IsNew === "U") {
                isChange = true;
                return true;
            }
        });
    }

    return isChange;
}

function GetListMesPackage(productionPkgId, callBack) {
    let config = ObjectConfigAjaxPost("../Planning/GetListMESPackages", false
        , JSON.stringify({ productionPkgId: productionPkgId }));
    AjaxPostCommon(config, function (listMesPkg) {
        callBack(listMesPkg);
    });
}

function CheckExistingMesPkgOnScheduler(selMtopEvents) {
    let isExisting = true;
    $.each(selMtopEvents, function (idx, mtopPkg) {
        //Get production package id from the first selected event
        let productionPkgId = mtopPkg.data.PrdPkg;

        //Get list mes package in database by production id
        GetListMesPackage(productionPkgId, function (listMesPkgDb) {
            //If production package was scheduled then checking the number of MES package
            // in database and on MES scheduler whether are same or not
            if (listMesPkgDb.length > 0) {
                //Get package group from mes package
                let pkgGroup = listMesPkgDb[0].PackageGroup;
                //Count the number MES package on scheduler
                let countMxPackage = 0;
                $.each(listMesPkgDb, function (idx, mesPkgDb) {
                    //Get list mes package on mes scheduler
                    let curListMesPkg = dpMes.events.list;
                    $.each(curListMesPkg, function (idx, curMesPkg) {
                        //Ignore new MES package
                        if (curMesPkg.IsNew !== 'Y') {
                            //Count mes packages on MES scheduler
                            //If mes package id on MES scheduler is same with mes package id in database then counting
                            if (mesPkgDb.MxPackage === curMesPkg.MxPackage) {
                                countMxPackage++;
                            }
                        }
                    });
                });

                //Filter list deleted mes packaged by package group
                let listDelMesPkg = LIST_DELETED_MES_PACKAGE.filter(x => x.PackageGroup === pkgGroup);
                //Count deleted mes package
                if (listDelMesPkg.length !== 0) {
                    $.each(listDelMesPkg, function () {
                        countMxPackage++;
                    });
                }

                //Compare the list of mes package in database and mes package on MES scheduler.
                if (countMxPackage !== listMesPkgDb.length) {
                    isExisting = false;
                    return false;
                }
            }
        })
    });

    return isExisting;
}

function isScanedMesPackage(factory, startDate, mesPkg) {
    let isScanned = false;
    let config = ObjectConfigAjaxPost("../Planning/IsScannedMesPackage", false
        , JSON.stringify({ factory: factory, startDate: startDate, mesPkg: mesPkg }));
    AjaxPostCommon(config, function (scanStatus) {
        isScanned = scanStatus;
    });

    return isScanned;
}
//#endregion

//#region Slider

function ReloadQCOGridBySlider() {
    //Reload QCO gridview base on slider
    let sliderQCO = document.getElementById('sliderQCO');
    let readSlider = sliderQCO.noUiSlider.get();

    var qcoFilters = $.grep(QCO_CURRENT_LIST, function (qco) {
        return qco.QCORank > readSlider[0] && qco.QCORank < readSlider[1];
    });

    ReloadJqGridLocal(tableQCOPPKGName, qcoFilters);
}

function QCOSlider(min, max) {
    //let slider = $("#sliderQCO"); 
    let slider = document.getElementById("sliderQCO");
    noUiSlider.create(slider, {
        start: [min, max],
        step: 1,
        connect: true,
        //tooltips: [true, true],
        tooltips: true,
        range: {
            min: [min],
            max: [max]
        }
    });

    slider.noUiSlider.on('change.one', function () {
        let sliderQCO = document.getElementById('sliderQCO');
        let readSlider = sliderQCO.noUiSlider.get();

        var qcoFilters = $.grep(QCO_CURRENT_LIST, function (qco) {
            return qco.QCORank > readSlider[0] && qco.QCORank < readSlider[1];
        });

        ReloadJqGridLocal(tableQCOPPKGName, qcoFilters);
    });

}
//#endregion

//#region Working time
/**
 *
 * @param {any} factoryId Factory id
 * @param {any} startDate Start date AOMTOP schedule
 * @param {any} endDate End date to search AOMTOP package schedule
 * @param {any} callBack Callback function
 */
function GetWorkingTime(factoryId, startDate, endDate, callBack) {

    var config = ObjectConfigAjaxPost("../Planning/GetWorkingTime", false
        , JSON.stringify({ factoryId: factoryId, startDate: startDate, endDate: endDate }));
    AjaxPostCommon(config, function (lstWorkingTime) {

        callBack(lstWorkingTime);
    });
}

function CheckWorkingTimeBeforeScheduling(dayDis, startDate, endDateDpMes) {
    let startDateScheule = new Date(startDate);
    for (let i = 0; i < dayDis; i++) {

        //If date schedule greater then date of MES daypialot
        if (startDateScheule > endDateDpMes) return false;

        //Get next date
        startDateScheule.setDate(startDateScheule.getDate() + 1);

        //Check working date of next date
        while (!isWorkingDate(startDateScheule)) {
            //Next date
            startDateScheule.setDate(startDateScheule.getDate() + 1);

            //break while if start date greter then end date
            if (startDateScheule > endDateDpMes) break;
        }
    }

    return true;
}

function isWorkingDate(schedulingDate) {
    let newSchedulingDate = new Date(schedulingDate);

    let dd = newSchedulingDate.getDate();
    let mm = newSchedulingDate.getMonth() + 1;
    let yy = newSchedulingDate.getFullYear();

    //Find working time by year, month and day
    let workingTime = _listWorkingTime.find(x => x.PLANYEAR === yy.toString() && x.PLANMONTH === ZeroPad(mm, 2) && x.PLANDAY === ZeroPad(dd, 2));
    if (typeof workingTime === 'undefined') return false;

    //If morning time and afternoon time is 0 then retur false
    if (workingTime.MORNINGTIME === 0 && workingTime.AFTERNOONTIME === 0) {
        return false;
    }

    return true;
}
//#endregion