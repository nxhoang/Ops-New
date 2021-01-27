
//Get list of company
var LABELS_CHART = [];
var lstFactoryCode = [];
var CHART_DATA = [];
var CUR_YEAR;
var CUR_MONTH;
var CUR_DATE;
var CUR_WEEK_NO;

function InitPage() {

    //GetCompanyCorporation(function (lstCom) {
    //    $.each(lstCom, function (index, value) {
    //        LABELS_CHART.push(value.ShortName);
    //        lstFactoryCode.push(value.DeptCode);
    //    });

    //    loadChart();
    //});

    DateRangePicker("#txtYearMonth");

    let curDate = new Date();
    CUR_MONTH = curDate.getMonth() + 1;
    CUR_YEAR = curDate.getFullYear();
    CUR_WEEK_NO = getWeekNumber(curDate)[1];
    CUR_DATE = curDate.getDate() < 10 ? '0' + curDate.getDate() : curDate.getDate();
    let showType = $("#drpShowType").val();

    GetMaxMachineCountByMonth(CUR_YEAR, CUR_MONTH, CUR_DATE, CUR_WEEK_NO, showType, function (resData) {
        createChartData(resData);   
    });

    //Fill list weekno to dropdownlist
    FillDataToDropDownlist("drpWeekNo", CreateWeekNo(), "WeekNo", "WeekName");

    Selection2("drpYear");
    Selection2("drpMonth");
    Selection2("drpShowBy");
    Selection2("drpShowType");

    eventSelectDropdownlist();
            
    $("#drpYear").val(CUR_YEAR).trigger('change');
    $("#drpMonth").val(CUR_MONTH).trigger('change');
    $("#drpWeekNo").val(CUR_WEEK_NO).trigger('change');
}

function createChartData(resData) {
    if (resData.IsSuccess) {
        let labelsChart = [];
        let mesMc = [];
        let dgsMc = [];
        let pkgMc = [];
        $.each(resData.Data, function (idx, mccn) {
            labelsChart.push(mccn.FACTORY);
            mesMc.push(mccn.MACHINE_COUNT_MES);
            dgsMc.push(mccn.MACHINE_COUNT_DGS);
            pkgMc.push(mccn.MACHINE_COUNT_PKG);
        });

        CHART_DATA = createChartDataSet(labelsChart, mesMc, dgsMc, pkgMc);
    }

    loadChart();

}

// Get number of machines of factory from MES, DGS and package
function GetMaxMachineCountByMonth(yyyy, mm, dd, weekNo, showType, callBack) {
    var config = ObjectConfigAjaxPost("../Dashboard/GetMaxMachineCountByMonth", false, JSON.stringify({ yyyy: yyyy, mm: mm, dd, weekNo, showType: showType }));
    AjaxPostCommon(config, function (resData) {
        callBack(resData);
    });
}

function GetMaxMachineCountByWeek(yyyy, mm, dd, weekNo, showType, callBack) {
    var config = ObjectConfigAjaxPost("../Dashboard/GetMaxMachineCountByWeek", false, JSON.stringify({ yyyy: yyyy, mm: mm, dd: dd, weekNo: weekNo, showType: showType }));
    AjaxPostCommon(config, function (resData) {
        callBack(resData);
    });
}

function eventSelectDropdownlist() {
    $("#drpShowBy").on("change", function () {
        if (this.value === "1") {
            //Montly
            $("#divMonth").show();
            $("#divWeekNo").hide();

        } else {
            //Weekly
            $("#divMonth").hide();
            $("#divWeekNo").show();

        }
    });
}

function showMachineCount() {
    let yyyy = $("#drpYear").val();
    let showType = $("#drpShowType").val();

    if ($("#drpShowType").val() === "1") {        
        //No of machine
        if ($("#drpShowBy").val() === "1") {
            //Monthly            
            let mm = $("#drpMonth").val();

            GetMaxMachineCountByMonth(yyyy, mm, CUR_DATE, CUR_WEEK_NO, showType, function (resData) {
                createChartData(resData);
            });
        } else {
            //weekly
            let weekNo = $("#drpWeekNo").val();

            GetMaxMachineCountByWeek(yyyy, CUR_MONTH, CUR_DATE, weekNo, showType, function (resData) {
                createChartData(resData);
            });
        }
    } else {
       
        //No of Machine
        if ($("#drpShowBy").val() === "1") {
            //Monthly
            let mm = $("#drpMonth").val();

            GetMaxMachineCountByMonth(yyyy, mm, CUR_DATE, CUR_WEEK_NO, showType, function (resData) {
                createChartData(resData);
            });
        } else {
            //weekly
            let weekNo = $("#drpWeekNo").val();

            GetMaxMachineCountByWeek(yyyy, CUR_MONTH, CUR_DATE, weekNo, showType, function (resData) {
                createChartData(resData);
            });
        }
    }
}

function updateMachineCount() {

    $.blockUI({ css: BLOCKUI_CSS });

    let selDate = $("#txtYearMonth").val();
    let yyyy = selDate.substr(0, 4);
    let mm = selDate.substr(5, 2);
    let dd = selDate.substr(8, 2);

    //Get week number
    let weekNo = getWeekNumber(new Date(selDate))[1];

    var config = ObjectConfigAjaxPost("../Dashboard/UpdateMachineCount", true, JSON.stringify({ yyyy: yyyy, mm: mm, dd: dd, weekNo: weekNo }));
    AjaxPostCommon(config, function (updSta) {
        $.unblockUI;
        if (updSta.IsSuccess) {            
            ShowMessage("Update machine operation", updSta.Data, ObjMessageType.Info);
        } else {
            ShowMessage("Update machine operation", updSta.Message, ObjMessageType.Info);
        }
    });
}

//#region chart.js

function createChartDataSet(LABELS_CHART, mesMc, dgsMc, pkgMc) {
    let barChart = {
        labels: LABELS_CHART,
        datasets: [
            {
                label: 'MES',
                backgroundColor: convertColorHexToRgbOpacity(chartColors.gray1, 1),
                borderColor: chartColors.gray1,
                borderWidth: 1,
                hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.gray1, 0.7),
                data: mesMc
                //factory: lstFactoryCode

            },
            {
                label: 'DGS',
                backgroundColor: convertColorHexToRgbOpacity(chartColors.gray2, 1),
                borderColor: chartColors.gray2,
                borderWidth: 1,
                hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.gray2, 0.7),
                data: dgsMc
                //factory: lstFactoryCode
            },
            {
                label: 'Package',
                backgroundColor: convertColorHexToRgbOpacity(chartColors.gray3, 1),
                borderColor: chartColors.gray3,
                borderWidth: 1,
                hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.gray3, 0.7),
                data: pkgMc
                //factory: lstFactoryCode
            }
        ]


    };

    return barChart;
}

var barChartData = {
    labels: LABELS_CHART, //['PKBT', 'PKS2', 'PKS3'],
    datasets: [
        {
            label: 'Daily target',
            backgroundColor: convertColorHexToRgbOpacity(chartColors.yellow, 0.5),
            borderColor: chartColors.yellow,
            borderWidth: 1,
            hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.yellow, 0.8),
            data: [10000, 300, 800, 500, 110, 2330, 4560, 4320, 4230, 4230, 4230],
            factory: lstFactoryCode

        },
        {
            label: 'Completed',
            backgroundColor: convertColorHexToRgbOpacity(chartColors.green, 0.5),
            borderColor: chartColors.green,
            borderWidth: 1,
            hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.green, 0.8),
            data: [9000, 250, 600, 500, 110, 2330, 4560, 4320, 4230, 4230, 4230],
            factory: lstFactoryCode
        },
        {
            label: 'Remain',
            backgroundColor: convertColorHexToRgbOpacity(chartColors.red, 0.5),
            borderColor: chartColors.red,
            borderWidth: 1,
            hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.red, 0.8),
            data: [5000, 50, 200, 500, 40, 100, 2732, 2232, 1234, 3455, 2345],
            factory: lstFactoryCode
        }
    ]

};

function loadChart() {

    if (window.myBar !== undefined) {
        window.myBar.destroy();
    }
    var ctx = document.getElementById('cvCorp').getContext('2d');

    var ctx2 = document.getElementById("cvCorp");
    ctx2.height = 110;

    window.myBar = new Chart(ctx, {
        type: 'bar',
        data: CHART_DATA,
        options: {
            tooltips: {
                enabled: false
            },
            responsive: true,
            legend: {
                position: 'top'  
            }, 
            //title: {
            //    display: true,
            //    text: 'Corporation Factories'
            //},
            onClick: function (c, i) {
                //e = i[0];
                //if (typeof e !== "undefined") {
                //    console.log('e: ' + e._index);
                //    var x_value = this.data.labels[e._index];
                //    var y_value = this.data.datasets[0].data[e._index];
                //    console.log('x value: ' + x_value);
                //    console.log('y value: ' + y_value);
                //    var fac = this.data.datasets[0].factory[e._index];
                //    console.log('y value: ' + fac);
                //    window.location = "/MesManagement/FactorySummary?corp=" + fac;
                //}

            },
            scales: {
                yAxes: [{
                    ticks: {
                        callback: function (value, index, values) {
                            return value;
                        }
                    }
                }]
            },
            plugins: {
                datalabels: {
                    color: 'black',
                    display: function (context) {
                        return context.dataset.data[context.dataIndex] > 0;
                    },
                    font: {
                        weight: 'bold'
                    },
                    formatter: Math.round,
                    anchor: 'end',
                    offset: 0,
                    align: 'end'
                }
            }
        }
    });
    
}

//#endregion