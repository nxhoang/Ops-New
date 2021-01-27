var dpMtop;
var EVENTCOLOR = ["#00ace6", "#00cccc", "#00cc7a", "#009933", "#990099", "#cc3300"
    , "#666699", "#993366", "#00b38f", "#cc0088", "#cc00cc", "#0099cc"];

$(() => {

    SelectDateRangePicker('#txtDateRange');

    //Fill factory to dropdownlist
    GetFactories("drpFactory", null);

    //Fill buyer to dropdownlist
    GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);

    //Set factory base on factory of user role.
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    initDayPilotMtop();

    eventClickButtonAoMtopsSchedule();

    //Remove empty option of factory
    $('#drpFactory option').filter(function () {
        return ($(this).val().trim() === "" && $(this).text().trim() === "");
    }).remove();
});

const initDayPilotMtop = () => {
    var arrDateRange = $("#txtDateRange").val().split('-');

    var d1 = new Date(arrDateRange[0].trim());
    var d2 = new Date(arrDateRange[1].trim());
    var difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    dpMtop = new DayPilot.Scheduler("dpMtop");

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
    dpMtop.allowMultiSelect = false; //Do not allow multi select

    // dp.allowEventOverlap = false;
    dpMtop.allowMultiMove = false;
    dpMtop.multiMoveVerticalMode = "Disabled";

    dpMtop.moveDisabled = true;
    dpMtop.dragOutAllowed = false;

    dpMtop.heightSpec = "Max";
    dpMtop.height = 500;

    //#Note: DayPilot Pro
    dpMtop.cornerHtml = "LINE";

    //Corlor sunday colum
    dpMtop.onBeforeCellRender = function (args) {
        if (args.cell.start.getDayOfWeek() === 0) {
            args.cell.backColor = "#dddddd";
        }
    };

    dpMtop.onEventSelect = function (args) {
        if (args.selected && args.e.text().indexOf("unselectable") !== -1) {
            // prevent selecting events that contain the text "unselectable"
            args.preventDefault();
        }
    };

    dpMtop.init();
    dpMtop.scrollTo(arrDateRange[0]);
}

function loadProductionAndMesScheduler() {
    $.blockUI();
    setTimeout(() => {
        //Get information for searching
        let arrDateRange = $("#txtDateRange").val().split('-');
        let scrollTo = arrDateRange[0].replace(new RegExp('/', 'g'), '-');
        let startDate = $.trim(arrDateRange[0].replace(new RegExp('/', 'g'), ''));
        let endDate = $.trim(arrDateRange[1].replace(new RegExp('/', 'g'), ''));
        let factoryId = $("#drpFactory").val();
        let searchType = $("#drpPlannedOrders").val();

        let buyer = $("#drpBuyer").val();
        let styleInfo = $("#txtStyleInfo").val();
        let aoNo = $("#txtAoNumber").val();

        //Calculate number of days range
        let d1 = new Date(arrDateRange[0].trim());
        let d2 = new Date(arrDateRange[1].trim());
        let difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

        //Get production lines
        GetFactoryLines(factoryId, startDate, endDate, buyer, styleInfo, aoNo, function (newArrLine) {
            dpMtop.resources = newArrLine;
        });

        //Get production packages
        GetProductionPackage(factoryId, startDate, endDate, buyer, styleInfo, aoNo, searchType, function (lstPp) {
            dpMtop.events.list = lstPp;
        });

        dpMtop.days = difference + 1;
        dpMtop.startDate = arrDateRange[0];
        dpMtop.update();
        dpMtop.scrollTo(scrollTo);
        $.unblockUI();
    }, 100);
}

//#region get data through API
const GetFactoryLines = (factoryId, startDate, endDate, buyer, styleInfo, aoNo, callBack) => {
    var newArrLine = [];

    var config = ObjectConfigAjaxPost("../AOMTOPSSchedule/GetLinesByProPackage", false
        , JSON.stringify({ factoryId: factoryId, startDate: startDate, endDate: endDate, buyer: buyer, styleInfo: styleInfo, aoNo: aoNo }));
    AjaxPostCommon(config, function (lstLine) {
        $.each(lstLine, function (index, value) {
            var line = { name: value.LineNo, id: value.LineNo };

            newArrLine.push(line);
        });

        callBack(newArrLine);
    });
}

const GetProductionPackage = (factoryId, startDate, endDate, buyer, styleInfo, aoNo, searchType, callBack) => {

    //Get list prodcttion package from AOMTOP
    var config = ObjectConfigAjaxPost("../AOMTOPSSchedule/GetProductionPackage", false
        , JSON.stringify({ factoryId: factoryId, startDate: startDate, endDate: endDate, buyer: buyer, styleInfo: styleInfo, aoNo: aoNo, searchType: searchType }));
    AjaxPostCommon(config, function (lstMtopProPkg) {

        var distPP = [];
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
        });

        callBack(lstMtopProPkg);
    });
}
//#endregion