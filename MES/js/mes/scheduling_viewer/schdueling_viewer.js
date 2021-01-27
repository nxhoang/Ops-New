var dpMes;
var EVENTCOLOR = ["#00ace6", "#00cccc", "#00cc7a", "#009933", "#990099", "#cc3300"
    , "#666699", "#993366", "#00b38f", "#cc0088", "#cc00cc", "#0099cc"];

$(() => {

    initDateRange('#txtDateRange');

    //Fill factory to dropdownlist
    GetFactories("drpFactory", null);

    //Fill buyer to dropdownlist
    GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);

    //Set factory base on factory of user role.
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    initDayPilotMes();

    eventClickButtonSchdulingViewer();

    //Remove empty option of factory
    $('#drpFactory option').filter(function () {
        return ($(this).val().trim() === "" && $(this).text().trim() === "");
    }).remove();
});

const initDateRange = (dateRangeId) => {
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
    $(dateRangeId).data('daterangepicker').setEndDate(getCurrentDate(30));

}

//#region daily pilot
function initDayPilotMes() {
    var arrDateRange = $("#txtDateRange").val().split('-');

    var d1 = new Date(arrDateRange[0].trim());
    var d2 = new Date(arrDateRange[1].trim());
    var difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

    dpMes = new DayPilot.Scheduler("dpMes");

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

    dpMes.dragOutAllowed = false; //must be true same dpMtop
    dpMes.allowMultiSelect = false;

    dpMes.heightSpec = "Max";
    dpMes.height = 500;

    //#Note: DayPilot Pro
    dpMes.cornerHtml = "LINE";


    //Corlor sunday colum
    dpMes.onBeforeCellRender = function (args) {
        if (args.cell.start.getDayOfWeek() === 0) {
            args.cell.backColor = "#dddddd";
        }
    };

    dpMes.init();
    dpMes.scrollTo(arrDateRange[0]);
}

function loadMESPackage() {
    $.blockUI();
    setTimeout(() => {
        //Get information for searching
        let arrDateRange = $("#txtDateRange").val().split('-');
        let scrollTo = arrDateRange[0].replace(new RegExp('/', 'g'), '-');
        let startDate = $.trim(arrDateRange[0].replace(new RegExp('/', 'g'), ''));
        let endDate = $.trim(arrDateRange[1].replace(new RegExp('/', 'g'), ''));
        let factoryId = $("#drpFactory").val();

        let buyer = $("#drpBuyer").val();
        let styleInfo = $("#txtStyleInfo").val();
        let aoNo = $("#txtAoNumber").val();

        //Calculate number of days range
        let d1 = new Date(arrDateRange[0].trim());
        let d2 = new Date(arrDateRange[1].trim());
        let difference = Math.floor((d2 - d1) / (1000 * 60 * 60 * 24));

        //Get line for MES
        let mesLines = GetMESLinesByFactory(factoryId);
        dpMes.resources = mesLines;

        //Get production packages
        GetMesPackages(factoryId, startDate, endDate, factoryId, aoNo, buyer, styleInfo, function (listMes) {
            dpMes.events.list = listMes;
        });

        dpMes.days = difference + 1;
        dpMes.startDate = arrDateRange[0];
        dpMes.update();
        dpMes.scrollTo(scrollTo);

        $.unblockUI();
    }, 100);
}
//#endregion

//#region functions get data from API
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

function GetMesPackages(mesFac, startDate, endDate, ppFactory, aoNo, buyer, styleInf, callBack) {
    //string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
    var config = ObjectConfigAjaxPost("../SchedulingViewer/GetMESPackage", false
        , JSON.stringify({ mesFac: mesFac, startDate: startDate, endDate: endDate, ppFactory: ppFactory, aoNo: aoNo, buyer: buyer, styleInf: styleInf }));
    AjaxPostCommon(config, function (listMpmt) {

        let distPP = [];
        let indPp = 0; //Set index to get color

        //Get list of Mes package in list of group package
        let listMes = [];
        $.each(listMpmt, function (idx, mpmt) {
            $.each(mpmt.ListMpdt, function (idex, mpdt) {
                listMes.push(mpdt);
            });
        });

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
            value.moveDisabled = true; //Prevent moving event
            value.rightClickDisabled = true; //Disable right click
            value.cssClass = "event-style";
        });

        callBack(listMes);
    });
}
//#endregion