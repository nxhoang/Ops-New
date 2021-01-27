
const initDayPilotAomtopsModule = () => {
    console.log('initDayPilotAomtopsModule');

    var arrDateRange = $("#txtDateRangePp").val().split('-');
    var d1 = new Date(arrDateRange[0].trim());
    var d2 = new Date(arrDateRange[1].trim());
    var difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    _dpMtopModule = new DayPilot.Scheduler("dpMtopModule");
    // behavior and appearance
    _dpMtopModule.cellWidth = 40;
    _dpMtopModule.eventHeight = 25;
    _dpMtopModule.headerHeight = 25;

    // view
    _dpMtopModule.startDate = arrDateRange[0];
    _dpMtopModule.days = difference + 1;
    _dpMtopModule.scale = "Day";

    _dpMtopModule.rowMarginBottom = 5;

    _dpMtopModule.timeHeaders = [
        { "groupBy": "Month" },
        { "groupBy": 'Week' },
        { "groupBy": "Day", "format": "d" }
    ];

    _dpMtopModule.eventClickHandling = "Select";

    _dpMtopModule.bubble = new DayPilot.Bubble({
        onLoad: function (args) {
            console.log(args);
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
        }
        , animation: "fast" //fast is default, jump, slow 
    });

    _dpMtopModule.treeEnabled = true;
    _dpMtopModule.rowHeaderWidth = 100;

    //Load production package and lines
    _dpMtopModule.resources = [];
    _dpMtopModule.events.list = [];

    _dpMtopModule.eventHoverHandling = "Bubble";
    _dpMtopModule.eventClickHandling = "Select";
    //_dpMtopModule.allowMultiSelect = false; //Do not allow multi select
    _dpMtopModule.allowMultiSelect = COMBINATIONSTATUS === "Y" ? true : false;

    // dp.allowEventOverlap = false;
    _dpMtopModule.allowMultiMove = false;
    _dpMtopModule.multiMoveVerticalMode = "Disabled";

    _dpMtopModule.moveDisabled = true;
    _dpMtopModule.dragOutAllowed = false;

    _dpMtopModule.heightSpec = "Max";
    _dpMtopModule.height = 500;

    //#Note: DayPilot Pro
    _dpMtopModule.cornerHtml = "LINE";

    // event moving
    _dpMtopModule.onEventMove = function (args) {

        eventMove(_dpMtopModule, args);

        console.log("_dpMtopModule - onEventMove:" + args.e.start().toString());
    };

    //Corlor sunday colum
    _dpMtopModule.onBeforeCellRender = function (args) {
        if (args.cell.start.getDayOfWeek() === 0) {
            args.cell.backColor = "#dddddd";
        }
    };

    _dpMtopModule.onEventSelect = function (args) {
        //args.e.data.backColor = "#1a75ff";
        if (args.selected && args.e.text().indexOf("unselectable") !== -1) {  // prevent selecting events that contain the text "unselectable"
            args.preventDefault();
        }
    };

    //Show context menu to delete event
    _dpMtopModule.contextMenu = new DayPilot.Menu({
        items: [
            {
                text: "Create MES package", onclick: function () {

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
                    OBJEVENTMTOPSEL = _dpMtopModule.multiselect.events();
                    if (OBJEVENTMTOPSEL === null || OBJEVENTMTOPSEL.length === 0) {
                        ShowMessage("Create MES Package", "Please select a package to schedule.", ObjMessageType.Info);

                        return false;
                    }

                    //START ADD - SON) 19/Jun/2020 - Checking existing mes packages
                    //if (!CheckExistingMesPkgOnScheduler(OBJEVENTMTOPSEL)) {
                    //    ShowMessage("Create MES Package", "Please select date again to show all MES packages.", ObjMessageType.Info);
                    //    return false;
                    //}
                    //END ADD - SON) 19/Jun/2020

                    const eventFst = _dpMtopModule.multiselect.events()[0].data;
                    const stlInf = eventFst.StyleInf;
                    const firstPackageGroup = eventFst.PackageGroup;

                    let tempPPackage = "";
                    let isSameStyle = true;
                    let isSamePkgGroup = true;
                    OBJEVENTMTOPSEL.forEach(function (e) {
                        let eveData = e.data;
                        //Events are selected must be same style code, size, color and revision.
                        if (stlInf !== eveData.StyleInf) {
                            isSameStyle = false;
                            return false;
                        }

                        //check aomtops packages are the same package group or not
                        if (eveData.PackageGroup !== firstPackageGroup) {
                            isSamePkgGroup = false;
                            return false;
                        }

                        //calculate total plan qty
                        if ($.isEmptyObject(eveData.PackageGroup)) totalQty += eveData.PlanQty;
                        else totalQty = eveData.RemainQty;
                    });

                    //Check if packages are not same style information then return
                    if (!isSameStyle) {
                        ShowMessage("Create MES Package", "Packages must be same style.", ObjMessageType.Info);
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

                    let prdPkg = OBJEVENTMTOPSEL[0].data.PrdPkg;
                    //Set production package to hidden field
                    $("#hdSelectedPrdPkg").text(prdPkg);

                    //Get module by style code
                    getModulesByStyleCode('drpModule', styleCodeFst);

                    //Set total qty on modal
                    $("#txtTotalQty").val(totalQty);
                    showDivideAomtopsPackages();

                    //Get daily target qty from OPS                   
                    let dailyTargetQty = GetDailyTargetQtyFromOPS(facMes, styleCodeFst, styleSizeFst, styleColorSerialFst, styleRevFst);
                    $("#txtDailyTarQty, #txtOPSDailyTarQty").val(dailyTargetQty);
                }
            }
        ]
    });

    _dpMtopModule.init();
    _dpMtopModule.scrollTo(arrDateRange[0]);
}

const initDayPilotMesModule = () => {
    var arrDateRange = $("#txtDateRangePp").val().split('-');

    var d1 = new Date(arrDateRange[0].trim());
    var d2 = new Date(arrDateRange[1].trim());
    var difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    _dpMesModule = new DayPilot.Scheduler("dpMesModule");

    // behavior and appearance
    _dpMesModule.cellWidth = 40;
    _dpMesModule.eventHeight = 25;
    _dpMesModule.headerHeight = 25;

    // view
    _dpMesModule.startDate = arrDateRange[0];
    _dpMesModule.days = difference + 1;
    _dpMesModule.scale = "Day";

    _dpMesModule.rowMarginBottom = 5;

    _dpMesModule.timeHeaders = [
        { "groupBy": "Month" },
        { "groupBy": 'Week' },
        { "groupBy": "Day", "format": "d" }
    ];

    _dpMesModule.eventClickHandling = "Select";

    _dpMesModule.bubble = new DayPilot.Bubble({
        onLoad: function (args) {
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
        }
    });

    _dpMesModule.treeEnabled = true;
    _dpMesModule.rowHeaderWidth = 100;

    //Clear mes package on scheduler
    _dpMesModule.resources = [];
    _dpMesModule.events.list = [];

    _dpMesModule.eventHoverHandling = "Bubble";

    _dpMesModule.dragOutAllowed = false; //must be true same dpMtop
    _dpMesModule.allowMultiSelect = false;

    _dpMesModule.heightSpec = "Max";
    _dpMesModule.height = 500;

    //#Note: DayPilot Pro
    _dpMesModule.cornerHtml = "LINE";

    // event moving
    _dpMesModule.onEventMove = function (args) {

        if (args.e.data.IsNew !== 'Y') {
            if (isScanedMesPackage(args.e.data.Factory, args.e.data.PlnStartDate, args.e.data.MxPackage)) {
                ShowMessage("Delete MES Package", `This MES package (${args.e.data.MxPackage}) scanned IoT.`, ObjMessageType.Info);
                args.preventDefault();
            }
        }

        let curDate = args.newStart.toString('yyyy/MM/dd');

        if (!isWorkingDate(curDate)) {
            ShowMessage("Schedule MES package", "This day does not have working time.", ObjMessageType.Info);
            args.preventDefault();
            console.log("_dpMesModule - onEventMove: This day does not have working time - " + curDate);
        }

    };

    _dpMesModule.onEventMoved = function (args) {
        //_dpMesModule.message("Event " + args.e.text() + " moved.");      
        //console.log("_dpMesModule - onEventMoved 1: " + OBJDRAGPROPKG.data.start);
        //console.log("_dpMesModule - onEventMoved: " + args.e.text());

        //Enable resizeable event
        args.e.data.resizeDisabled = true;
        let isNew = args.e.data.IsNew;
        //console.log("_dpMesModule - onEventMoved - isnew before: " + isNew);
        args.e.data.IsNew = isNew === "Y" ? isNew : "U";
    };

    //Corlor sunday colum
    _dpMesModule.onBeforeCellRender = function (args) {
        if (args.cell.start.getDayOfWeek() === 0) {
            args.cell.backColor = "#dddddd";
        }
    };

    //Show context menu to delete event
    _dpMesModule.contextMenu = new DayPilot.Menu({
        items: [
            {
                text: "Delete", onclick: function () {
                    let e = this.source;
                    //Get package group id of event (MES package)
                    let pkgGroupId = e.data.PackageGroup;

                    if (e.data.IsNew !== 'Y') {
                        if (isScanedMesPackage(e.data.Factory, e.data.PlnStartDate, e.data.MxPackage)) {
                            ShowMessage("Delete MES Package", `This MES package (${e.data.MxPackage}) scanned IoT.`, ObjMessageType.Info);

                            return false;
                        }
                    }

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

                    _dpMesModule.events.remove(e);

                }
            }
        ]
        //cssClassPrefix: "menu_default"
    });

    _dpMesModule.init();
    _dpMesModule.scrollTo(arrDateRange[0]);
}

const showDivideAomtopsPackages = () => {
    //Show modal divide production package quantity
    $('#mdlPpDivide').modal('show');
    //Enable drag modal
    $("#mdlPpDivide").draggable({
        handle: ".modal-header"
    });

    if (_currentTab === 3) {
        $('#divModule').show();
    } else {
        $('#divModule').hide();
    }
}

const loadAomtopsPackagesModule = () => {
    $.blockUI({ baseZ: 1052 });
    setTimeout(() => {

        //Clear data
        OBJDRAGMESPKG = null;
        OBJEVENTMTOPSEL = [];
        LIST_DELETED_MES_PACKAGE = [];
        _listWorkingTime = [];

        //Get information for searching
        let arrDateRange = $("#txtDateRangePp").val().split('-');
        let scrollTo = arrDateRange[0].replace(new RegExp('/', 'g'), '-');
        let startDate = $.trim(arrDateRange[0].replace(new RegExp('/', 'g'), ''));
        let endDate = $.trim(arrDateRange[1].replace(new RegExp('/', 'g'), ''));
        let factoryId = $("#drpFactoryMtop").val();

        let buyer = $("#drpBuyerMtop").val();
        let styleInfo = $("#txtStyleInfo").val();
        let aoNo = $("#txtAoNumber").val();

        //Calculate number of days range
        let d1 = new Date(arrDateRange[0].trim());
        let d2 = new Date(arrDateRange[1].trim());
        let difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

        _endDateDpMes = d2;

        //Get production lines
        GetFactoryLines(factoryId, startDate, endDate, buyer, styleInfo, aoNo, function (newArrLine) {
            _dpMtopModule.resources = newArrLine;
        });

        let prdStartDate = arrDateRange[0];
        //Get production packages
        getAomtopsPackagesModule(factoryId, startDate, endDate, buyer, styleInfo, aoNo, function (lstPp) {
            _dpMtopModule.events.list = lstPp;

            if (lstPp.length > 0) {
                //sort production start date to get minimun date
                lstPp.sort((a, b) => (a.PrdSdat > b.PrdSdat) ? 1 : -1);
                prdStartDate = moment(lstPp[0].PrdSdat).format('YYYY-MM-DD');
                prdStartDate = prdStartDate > arrDateRange[0] ? arrDateRange[0] : prdStartDate;
                console.log(prdStartDate);

                //sort production end date to get maximum date
                lstPp.sort((a, b) => (a.PrdEdat > b.PrdEdat) ? 1 : -1);
                let prdEndDate = moment(lstPp[lstPp.length - 1].PrdEdat).format('YYYY-MM-DD');
                prdEndDate = prdEndDate > arrDateRange[1] ? prdEndDate : arrDateRange[1];
                console.log(prdEndDate);
                //Calculate difference date
                let drpDifferenceDay = Math.floor((new Date(prdEndDate) - new Date(prdStartDate)) / (1000 * 60 * 60 * 24));
                difference = difference > drpDifferenceDay ? difference : drpDifferenceDay;
            }

        });

        _dpMtopModule.days = difference + 1;
        _dpMtopModule.startDate = prdStartDate;
        _dpMtopModule.update();
        _dpMtopModule.scrollTo(scrollTo);

        //get mes packages from local
        loadMesPackagesDailyPilot(factoryId, startDate, endDate, buyer, styleInfo, aoNo, $.trim(arrDateRange[0]), $.trim(arrDateRange[1]), difference);

        $.unblockUI();
    }, 100);
}

const loadMesPackagesDailyPilot = (factoryId, startDate, endDate, buyer, styleInfo, aoNo, prdStartDate, prdEndDate, prdDifferenceDay) => {
    //Get line for MES
    let mesLines = GetMESLinesByFactory(factoryId);
    _dpMesModule.resources = mesLines;

    let mesStartDate = prdStartDate;
    let mesEndDate = prdEndDate;
    //Get production packages
    getMesPackagesModule(factoryId, startDate, endDate, aoNo, buyer, styleInfo, function (listMes) {
        _dpMesModule.events.list = listMes;

        if (listMes.length > 0) {
            //sort list mes packages to get minimun plan start date
            listMes.sort((a, b) => (a.PlnStartDate > b.PlnStartDate) ? 1 : -1);
            mesStartDate = moment(listMes[0].PlnStartDate).format('YYYY-MM-DD');
            mesStartDate = mesStartDate > prdStartDate ? prdStartDate : mesStartDate;
            console.log(mesStartDate);

            //sort list mes pakages to get maximum plan end date
            listMes.sort((a, b) => (a.PlnStartDate > b.PlnStartDate) ? 1 : -1);
            mesEndDate = moment(listMes[listMes.length - 1].PlnStartDate).format('YYYY-MM-DD');
            mesEndDate = mesEndDate > prdEndDate ? mesEndDate : prdEndDate;
            console.log(mesEndDate);

            prdDifferenceDay = Math.floor((new Date(mesEndDate) - new Date(mesStartDate)) / (1000 * 60 * 60 * 24));
        }
    });

    _dpMesModule.days = prdDifferenceDay + 1;
    _dpMesModule.startDate = mesStartDate;
    _dpMesModule.update();
    _dpMesModule.scrollTo(prdStartDate.replace(new RegExp('/', 'g'), '-'));

    //Fill MES line to dropdownlist
    FillDataMultipleSelectLineNoMdl("drpLineNo", mesLines, "id", "name");

    //Get list of working time
    GetWorkingTime(factoryId, startDate, endDate, function (listWrkTime) {
        _listWorkingTime = listWrkTime;
    });
}

//#region get data through API
const getAomtopsPackagesModule = (factoryId, startDate, endDate, buyer, styleInfo, aoNo, callBack) => {

    //Get list prodcttion package from AOMTOP
    var config = ObjectConfigAjaxPost("../Planning/GetAomtopsPackagesModule", false
        , JSON.stringify({ factoryId: factoryId, startDate: startDate, endDate: endDate, buyer: buyer, styleInfo: styleInfo, aoNo: aoNo }));
    AjaxPostCommon(config, function (lstMtopProPkg) {

        var distPP = [];
        lstMtopProPkg.forEach(value => {
            //Check style information
            var stlInf = value.StyleInf;
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
            value.moveDisabled = true;

            if (!$.isEmptyObject(value.PackageGroup)) {
                if (value.RemainQty === 0) {
                    //Disable right click
                    value.rightClickDisabled = true;
                    value.cssClass = "distributed";
                } else {
                    if (value.RemainQty !== value.TargetQty) {
                        value.barHidden = false;
                    }
                }
            }
        })

        callBack(lstMtopProPkg);
    });
}

const getMesPackagesModule = (mesFac, startDate, endDate, aoNo, buyer, styleInf, callBack) => {
    //string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
    var config = ObjectConfigAjaxPost("../Planning/GetMesPackagesModule", false
        , JSON.stringify({ mesFac: mesFac, startDate: startDate, endDate: endDate, aoNo: aoNo, buyer: buyer, styleInf: styleInf }));
    AjaxPostCommon(config, function (listMesPkg) {

        let distPP = [];

        $.each(listMesPkg, function (index, value) {

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
        callBack(listMesPkg);
    });
}

const getModulesByStyleCode = (dropdownId, styleCode) => {
    var config = ObjectConfigAjaxPost("../Planning/GetModulesByStyleCode", true
        , JSON.stringify({ styleCode: styleCode}));
    AjaxPostCommon(config, function (moduleList) {
        FillDataToDropDownlist(dropdownId, moduleList, 'ModuleId', 'ModuleName')
    });
}
//#endregion

const checkDataWhenCreatingMesPackage = (lineNo, dailyTarQty, factoryMes, module) => {
    //Check line no
    if ($.isEmptyObject(lineNo)) {
        ShowMessage("Create MES package", "Please choose line number.", ObjMessageType.Info);
        return false;
    }

    //Check daily quantity
    if ($.isEmptyObject(dailyTarQty) || isNaN(dailyTarQty) || dailyTarQty === 0) {
        ShowMessage("Create MES package", "Please check daily target Qty.", ObjMessageType.Info);
        return false;
    }

    //Check MES factory
    if ($.isEmptyObject(factoryMes)) {
        ShowMessage("Create MES package", "Please select Mes factory.", ObjMessageType.Info);
        return;
    }

    if ($.isEmptyObject(module)) {
        ShowMessage("Create MES package", "Please select module.", ObjMessageType.Info);
        return false;
    }

    //Check production package whether available or not before schedule
    let selPrdPkg = $("#hdSelectedPrdPkg").text();
    if (!isProductionPackageAvailable(factoryMes, selPrdPkg)) {
        ShowMessage("Create MES package", "This production package was scheduled.", ObjMessageType.Info);
        return false;
    }

    let dayDis = $("#chkCreateAll").is(":checked") === false ? 1 : Math.ceil(totalQty / dailyTarQty);
    //Checking working time before scheduling
    if (!CheckWorkingTimeBeforeScheduling(dayDis, $("#txtStartDate").val(), _endDateDpMes)) {
        ShowMessage("Create MES package", "Please synchronize working time sheet.", ObjMessageType.Info);
        return false;
    }

    return true;
}

const createMesPackageByModule = () => {
    let lineNo = $("#drpLineNo").val();
    let dailyTarQty = parseInt($("#txtDailyTarQty").val());
    let numLines = lineNo.length;
    let factoryMes = $("#drpFactoryMes").val();
    let totalQty = parseInt($("#txtTotalQty").val());
    let module = $("#drpModule").val();

    //check data before creating mes package.
    if (!checkDataWhenCreatingMesPackage(lineNo, dailyTarQty, factoryMes, module)) return;

    //Get information of the first selected events
    let objEvent;
    objEvent = OBJEVENTMTOPSEL[0].data;
    let styleInf = objEvent.StyleInf;

    //Get production package
    let proPackageId = objEvent.id;

    //CREATE PACKAGE GROUP.
    //Get current year and month
    let d = new Date();
    let month = (d.getMonth() + 1).toString();
    let mm = month.length === 1 ? "0" + month : month;
    let yy = d.getFullYear().toString();
    let yymm = yy.substr(2, 2) + mm;

    let newPkgGroup;

    

    //Keep mes packge serial
    let mesSerial = 0;
    let seqMes = 0;

    //is new production package
    let isDisNewPPPackage = true;
    //remain quantity of package group
    let oldPPkgRemainQty = 0;



    //check selected production package whether has package group or not.
    //if it has then don't need to create new package group.
    if (!$.isEmptyObject(objEvent.PackageGroup)) {
        newPkgGroup = objEvent.PackageGroup;
        oldPPkgRemainQty = objEvent.RemainQty;
        isDisNewPPPackage = false;
    } else {
        isDisNewPPPackage = true;
        //generate new package group
        //Create new Package group by factory, year and month
        newPkgGroup = GetMaxPackageGroup(factoryMes, yymm);
        //Get number of sequence package group (get last 9 characters of package group) 
        //and set it to the current sequence variable
        CUR_PACKAGE_SEQ = parseInt(newPkgGroup.substr(10, 9));
    }



    //Checking production package to create new group package or not
    //$.each(OBJEVENTMTOPSEL, function (idx, selEvn) {
    //    let selObj = selEvn.data;
    //    //Find package group in current list package group
    //    let pkgGroup = LIST_PP_PACKAGE_GROUP.filter(x => x.PPackage === selObj.PrdPkg); //.map(x => x.PackageGroup);

    //    //If package existed then don't need to create new package group
    //    if (pkgGroup.length !== 0) {
    //        isDisNewPPPackage = false;
    //        newPkgGroup = pkgGroup[0].PackageGroup;

    //        //Get information of production package group in list
    //        let ppGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === newPkgGroup);
    //        oldPPkgRemainQty = ppGroup.length === 0 ? oldPPkgRemainQty : ppGroup[0].RemainQty;
    //        return false;
    //    }
    //});

    //If do not exist production package group then create new package group
    if (isDisNewPPPackage) {
        let listNewPkgGroup = LIST_PACKAGE_GROUP.filter(x => x.IsNew === "Y");
        //If the is no new package group in current list then get max package group from database
        if (listNewPkgGroup.length === 0) {
            //Create new Package group by factory, year and month
            newPkgGroup = GetMaxPackageGroup(factoryMes, yymm);

            //Get number of sequence package group (get last 9 characters of package group) and set it to the current sequent variable
            CUR_PACKAGE_SEQ = parseInt(newPkgGroup.substr(10, 9));
        } else {
            //If in current list has new package group, just increase package group sequence from variable
            //and create package group from this sequence
            CUR_PACKAGE_SEQ++;
            newPkgGroup = factoryMes + "-" + yymm + "-" + ZeroPad(CUR_PACKAGE_SEQ, 9);
        }
    }

    //Get max MES sequence in package group
    let pkgGroup = LIST_PACKAGE_GROUP.filter(x => x.PackageGroup === newPkgGroup);
    if (pkgGroup.length !== 0) {

        //START MOD) SON (2019.09.17) - 17 September 2019 - check maximum mes package sequence
        let listMesSeq = LIST_MES_SEQ.filter(x => x.PackageGroup === newPkgGroup);
        if (listMesSeq.length !== 0) {
            //Get min Seq at the first position
            let maxSeq = listMesSeq[0].Seq;
            //Find maximum mes sequence in the list
            $.each(listMesSeq, function (idx, mesSeq) {
                if (maxSeq < mesSeq.Seq) maxSeq = mesSeq.Seq;
            });
            //Set max MES sequence
            seqMes = maxSeq;
        } else {
            seqMes = pkgGroup[0].MaxSeq;
        }

        //seqMes = pkgGroup[0].MaxSeq;

        //END MOD) SON (2019.09.17) - 17 September 2019 
    } else {

        seqMes = 0;
    }

    let plnQty = 0;
    let totalPlnQty = 0;
    let seqPP = 0;
    //let isSameStyle = true;
    $.each(OBJEVENTMTOPSEL, function (ind, obj) {
        //Production package information
        let ppInf = obj.data;

        //Create new object production package group if it is new.
        if (isDisNewPPPackage) {
            //Sum plan quantity of selected events
            totalPlnQty += obj.data.PlanQty;

            //Add production package group
            seqPP++;
            let objProPPkgGroup = {
                PackageGroup: newPkgGroup,
                SeqNo: seqPP,
                PPackage: ppInf.PrdPkg,
                Factory: ppInf.Factory,
                AONo: ppInf.AoNo,
                OrdQty: ppInf.OrdQty,
                PlanQty: ppInf.PlanQty,
                IsNew: "Y"
            };
            //Add production package into package group
            LIST_PP_PACKAGE_GROUP.push(objProPPkgGroup);
        }
    });

    //Get plan quantity
    plnQty = isDisNewPPPackage ? totalPlnQty : oldPPkgRemainQty;

    //let startDateExe = "2015-07-10"; 
    let startDateExe = $("#txtStartDate").val().replace(/\//g, "-");

    let id = objEvent.id;
    let startDate = startDateExe; //objEvent.start; //
    let endDate = startDateExe; //objEvent.end;
    let aoNo = objEvent.AoNo;
    let styleCode = objEvent.StyleCode;
    let styleSize = objEvent.StyleSize;
    let styleColorSerial = objEvent.StyleColorSerial;
    let revNo = objEvent.RevNo;
    let bgColor = objEvent.backColor;

    //Maximum the line can be distributed
    let maxLineDis = Math.ceil(plnQty / dailyTarQty);

    if (maxLineDis < numLines) {
        ShowMessage("Create MES package", "Please reduce distribution line.", ObjMessageType.Info);
        return;
    }

    //Distribute mes packages
    let remainQty = 0; //Remain quantity
    let totalDis = 0; //Total distribution
    let curPlan = plnQty < dailyTarQty ? plnQty : dailyTarQty;

    //Set start date to distribute mes package
    let dayPiloStart = DayPilot.Date.parse(startDate, "yyyy-MM-dd");

    for (let i = 1; i <= dayDis; i++) {

        while (!isWorkingDate(dayPiloStart.toString('yyyy/MM/dd'))) {
            dayPiloStart = new DayPilot.Date(dayPiloStart).addDays(1);
        }

        $.each(lineNo, function (index, value) {
            if (remainQty > 0 || totalDis === remainQty) {

                seqMes++;

                let newMxPackage = "M" + proPackageId.slice(1) + "_" + ZeroPad(seqMes, 2);

                //let newId = newPkgGroup + "_" + (CUR_NUMBER_MES_PKG + 1);
                CUR_NUMBER_MES_PKG++;

                //Create event object for Mes daypilot
                let e = new DayPilot.Event({
                    start: dayPiloStart,
                    end: dayPiloStart, //Always distribute 1 day only
                    id: newMxPackage, //newId,
                    text: newMxPackage, //newId,
                    resource: value,
                    Factory: factoryMes,
                    LineNo: value,
                    AoNo: aoNo,
                    StyleCode: styleCode,
                    StyleSize: styleSize,
                    StyleColorSerial: styleColorSerial,
                    RevNo: revNo,
                    PlanQty: curPlan,
                    StyleInf: styleInf,

                    PackageGroup: newPkgGroup,
                    SeqNo: seqMes,
                    PrdPkg: proPackageId,
                    MxPackage: newMxPackage,
                    MxTarget: curPlan,
                    PlnStartDate: "",
                    PlnEndDate: "",
                    FinishedQty: 0,
                    TaktTime: 0,
                    WorkingHours: 0,

                    MxStatus: "RO",

                    resizeDisabled: true, //1 MES package produce for 1 date 

                    backColor: bgColor,

                    cssClass: "event-style", //"distributed",
                    barHidden: true,

                    IsNew: "Y"//Status 'Y' is yes, new event was created, 'N' is No, old event load from database
                });
                dpMes.events.add(e);

                totalDis += curPlan;
                //Calculate remain qty again
                remainQty = plnQty - totalDis;
                curPlan = remainQty < dailyTarQty ? remainQty : dailyTarQty;

                //START ADD) SON (2019.09.17) - 17 September 2019 - add max MES sequence
                //Keep list mes seq in temporary memory
                LIST_MES_SEQ.push({ PackageGroup: newPkgGroup, Seq: seqMes, MxPackage: newMxPackage });
                //START ADD) SON (2019.09.17) - 17 September 2019
            }
        });

        dayPiloStart = new DayPilot.Date(dayPiloStart).addDays(1);
    }

    if (isDisNewPPPackage) {
        //Create object package group.   
        //If production package existed in current list of Package Group then no need to create Package group.        
        let objPkgGroup = {
            PackageGroup: newPkgGroup,
            MesFactory: factoryMes,
            StyleCode: styleCode,
            StyleSize: styleSize,
            StyleColorSerial: styleColorSerial,
            RevNo: revNo,
            Buyer: styleCode.substring(0, 3),
            TargetQty: totalQty,
            Status: "RO",
            MesPlnStartDate: "",
            MesPlnEndDate: "",
            Priority: 1,
            MadeQty: 0,
            //Registrar: "",
            StyleInf: styleInf,
            RemainQty: remainQty,
            IsNew: "Y",
            //MesMaxSerial: mesSerial,
            SeqNo: seqMes,
            Registrar: $("#hdUserId").val()
        };
        LIST_PACKAGE_GROUP.push(objPkgGroup);

    } else {
        //Update remain quantity
        let objIndex = LIST_PACKAGE_GROUP.findIndex(obj => obj.PackageGroup === newPkgGroup);
        LIST_PACKAGE_GROUP[objIndex].RemainQty = remainQty;

        //Update max MES serial
        LIST_PACKAGE_GROUP[objIndex].SeqNo = seqMes;
    }

    //Color production package background if remain qty is 0
    if (remainQty === 0) {
        //Remove event and add again
        $.each(OBJEVENTMTOPSEL, function (idx, selEvn) {

            let pp = dpMtop.events.find(selEvn.data.id);

            dpMtop.events.remove(pp);

            //Place css and adding again
            selEvn.data.cssClass = "distributed";
            selEvn.data.rightClickDisabled = true;
            dpMtop.events.add(selEvn);

        });
    }

    //Copy style information
    CopyStyleInfomation(styleCode, styleSize, styleColorSerial, revNo);

    //Hide modal after creating event
    $('#mdlPpDivide').modal('hide');
}