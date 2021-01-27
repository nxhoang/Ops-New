
//var chartColors = {
//    red: "rgb(255, 99, 132)",
//    orange: "rgb(255, 159, 64)",
//    yellow: "rgb(255, 205, 86)",
//    green: "rgb(75, 192, 192)",
//    blue: "rgb(54, 162, 235)",
//    purple: "rgb(153, 102, 255)",
//    grey: "rgb(201, 203, 207)"
//};

var chartName = "chartProcess";
var tempDataRow = [];
var tempDataChart = [];
var tempCategories = [];
var lstSelectedData = [];

// #region Chart js

var chartProcess;

//Generate process chart.
function GenerateProcessChart(chartType, showType) {
       
    //ChartTypeDataConfig(chartType);
   
    //var arrDataOpTime = []; 
    //var arrLabelOpName = [];
   
    //var gridData = jQuery(gridOpsDetailId).jqGrid("getRowData");
    //for (var i = 0; i < gridData.length; i++) {
    //    var opTime = gridData[i].OpTime;
    //    var opName = gridData[i].OpName;
    //    if (showType === "1") {
    //        if (!isEmpty(opTime) && opTime !== "0") {               
    //            arrDataOpTime.push(opTime);
    //            arrLabelOpName.push(opName);
    //        }
    //    } else {
    //        arrDataOpTime.push(opTime);
    //        arrLabelOpName.push(opName);
    //    }
    //}
    //if (chartProcess != null) {
    //    chartProcess.destroy();

    //    //Create new chart
    //    var ctx = document.getElementById("cvsProcessChart").getContext("2d");
    //    chartProcess = new Chart(ctx, config);
    //    var data = GetDataToCreateProcessChart(arrLabelOpName, arrDataOpTime);
    //    chartProcess.config.data = data;
    //    chartProcess.update();

    //    //Update new data
    //    //var newData = GetDataToCreateProcessChart(arrLabelOpName, arrDataOpTime);
    //    //chartProcess.config.data = newData;
    //    //chartProcess.update();
    //} else {
    //    //Create new chart
    //    var ctx2 = document.getElementById("cvsProcessChart").getContext("2d");
    //    chartProcess = new Chart(ctx2, config);
    //    var data2 = GetDataToCreateProcessChart(arrLabelOpName, arrDataOpTime);
    //    chartProcess.config.data = data2;
    //    chartProcess.update();
        
    //}
}

function GetDataToCreateProcessChart(arrLable, arrOptime) {
    //var newData = {
    //    labels: arrLable,
    //    datasets: [
    //        {
    //            label: "Process time",
    //            backgroundColor: chartColors.blue,
    //            borderColor: chartColors.blue,
    //            data: arrOptime,
    //            fill: false
    //        }
    //    ]
    //};

    //return newData;
}

var config;
function ChartTypeDataConfig(typeChart) {
    //if (isEmpty(typeChart)) typeChart = "line";

    //config = {
    //    type: typeChart,
    //    data: {
    //        labels: "",
    //        datasets: [{
    //            label: "Process time",
    //            backgroundColor: chartColors.red,
    //            borderColor: chartColors.red,
    //            data: [],
    //            fill: false,
    //            pointBackgroundColor: ["#FF0000", "#FF0000", "#FF0000", "#FF0000"]
    //        }]
    //    },
    //    options: {
    //        responsive: true,
    //        title: {
    //            display: true
    //            //text: "Process Chart"
    //        },
    //        tooltips: {
    //            mode: "index",
    //            intersect: false
    //        },
    //        hover: {
    //            mode: "nearest",
    //            intersect: true
    //        },
    //        scales: {
    //            xAxes: [{
    //                display: false,
    //                scaleLabel: {
    //                    display: true,
    //                    labelString: "Proccess"
    //                }
    //            }],
    //            yAxes: [{
    //                display: true,
    //                scaleLabel: {
    //                    display: true,
    //                    labelString: "Time in seconds"
    //                }
    //            }]
    //        }
    //    }
    //};

}

// #endregion

//#region Fusionchart

function RemoveFusioinChart(chartId) {
    // We procure the chart JavaScript object by referring to the Id
    // we used to create the chart.
    var revenueChart = FusionCharts(chartId);

    if (revenueChart !== undefined) {
        // Dispose the chart using the dispose function
        revenueChart.dispose();       
    }

}

function ChartProperties() {
    var chart = {
        "xAxisName": "Process",
        "yAxisName": "Process time",
        "lineThickness": "3",
        "theme": "fint",
        "showLabels": "0",        
        "showValues": "0",
        "placeValuesInside": "0",        
        "rotateValues": "0",
        "valueFontColor": "#000000",
        "palettecolors": "#0075c2" //Color bar chart
    };

    return chart;
}

function CreateDataSourceProperies(chartProp, arrCategories, arrData) {
    var datasource = {
        "chart": chartProp,
        "categories": arrCategories,
        "dataset": [{
            "allowDrag": "1",
            "data": arrData
        }]
    };

    return datasource;
}

function DataChartProperties(chartType, hotSpot, dataVal) {
    if (hotSpot === "1") {
        dataVal.HotSpot = "1";
        if (chartType === "dragcolumn2d" || chartType === "column2d") {
            delete dataVal.anchorRadius;
            delete dataVal.anchorBorderColor;
            delete dataVal.anchorBgColor;

            dataVal.color = "ff0000";            
        } else {
            delete dataVal.color;

            dataVal.anchorRadius = "5";
            dataVal.anchorBorderColor = "0372AB";
            dataVal.anchorBgColor = "ff0000";
        }
    }    

    return dataVal;
}

function GetProcessTimeDataForChart(data, chartType, showType, balancing) {
    var arrValue = [];
    var arrCategories = [];

    for (var i = 0; i < data.length; i++) {
        //var opTime = data[i].OpTime;
        var opTime = balancing === "1" ? data[i].OpTimeBalancing : data[i].OpTime;
        var opName = data[i].OpName;
        var hotSpot = data[i].HotSpot;
        var opSerial = data[i].OpSerial;

        if (showType === "1") {
            if (!isEmpty(opTime) && opTime !== "0") {
                var val = { "opserial": opSerial, "value": opTime, "label": opName  };
                val = DataChartProperties(chartType, hotSpot, val);

                arrValue.push(val);
                arrCategories.push({ "label": opName });
            }
        } else {
            var val2 = { "opserial": opSerial, "value": opTime, "label": opName  };
            val2 = DataChartProperties(chartType, hotSpot, val2);

            arrValue.push(val2);
            arrCategories.push({ "label": opName });
        }
    }
    
    var categories = [{ "category": arrCategories }];
    
    var arrData = { "data": arrValue, "categories": categories };

    tempDataChart = arrValue;
    tempCategories = categories;

    return arrData;
}

function CreateDatasource() {
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    var objOpsKey = {
        styleCode: objOpsMaster.StyleCode,
        styleSize: objOpsMaster.StyleSize,
        styleColor: objOpsMaster.StyleColorSerial,
        revNo: objOpsMaster.RevNo,
        opRevNo: objOpsMaster.OpRevNo,
        edition: objOpsMaster.Edition,
        languageId: objOpsMaster.Language
    };

    //Clear data in temporary.
    tempDataChart = [];
    tempCategories = [];
    lstSelectedData = [];

    var chartType = GetChartType(); // $("#drpChartType").val();
    var showType = $("#drpShowChart").val();

    var charProp = ChartProperties();
    charProp.showValues = $("#chkShowValue").is(":checked") ? "1" : "0";
    var balancing = $("#drpShowTime").val() === "2" ? "1" : "0";   
    var gridData = GetListOpsDetail(objOpsKey); //jQuery(gridOpsDetailId).jqGrid("getRowData");
    var lstData = GetProcessTimeDataForChart(gridData, chartType, showType, balancing);
    var arrData = lstData.data;
    var arrCategories = lstData.categories;
    var datasource = CreateDataSourceProperies(charProp, arrCategories, arrData);

    //Save data and categories to temporary memory using for showing value on chart.
    tempDataChart = arrData;
    tempCategories = arrCategories;

    return datasource;
   
}

function GenerationFusionChart(chartType, showType) {

    RemoveFusioinChart(chartName);
       
    FusionCharts.ready(function () {
        var fusioncharts = new FusionCharts({
            type: chartType, //'dragcolumn2d', //'dragline', //
            renderAt: 'chart-container',
            width: '100%',
            height: '600',
            dataFormat: 'json',
            id: chartName,
            dataSource: CreateDatasource(chartType, showType),
            events: {
                "dataPlotClick": function (eventObj, dataObj) {
                    /*
                    var index = dataObj.dataIndex;
                    var revenueChart = eventObj.sender;
                    var jsonData = revenueChart.getJSONData();
                    var curChartType = GetChartType();
                    var objSelectedData = {};
                    var hotSpot = tempDataChart[index].HotSpot;
                    if (curChartType === "dragcolumn2d" || curChartType === "column2d") {

                        var bgColor = tempDataChart[index].color;
                        console.log("column2d bgcolor: " + bgColor);

                        if (bgColor === "ffff00" && hotSpot === "1") {
                            bgColor = "ff0000";

                            //remove selected data from selected data list.
                            var i;
                            for (i = 0; i < lstSelectedData.length; i++) {
                                if (lstSelectedData[i].index === index) {
                                    lstSelectedData.splice(i, 1); break;
                                }
                            }
                        }
                        else if (bgColor === "ffff00") {//selected background color

                            bgColor = "0075c2";

                            //remove selected data from selected data list.
                            var i;
                            for (i = 0; i < lstSelectedData.length; i++) {
                                if (lstSelectedData[i].index === index) {
                                    lstSelectedData.splice(i, 1); break;
                                }
                            }

                        } else {//normal background color
                            if (lstSelectedData.length >= 2) {
                                alert("You selected maximum processes (2).");
                            } else {
                                bgColor = "ffff00";
                                objSelectedData.index = index;
                                objSelectedData.selectedData = tempDataChart[index];
                                lstSelectedData.push(objSelectedData);
                            }                                
                        }
                        tempDataChart[index].color = bgColor;
                    } else {

                        var bgColor = tempDataChart[index].anchorBgColor;
                        var radius = tempDataChart[index].anchorRadius;
                        var borderColor = tempDataChart[index].anchorBorderColor;
                      
                        if (bgColor === "ffff00" && hotSpot === "1") { //plot is hot spot
                            radius = "5";
                            bgColor = "ff0000";
                            borderColor = "0372AB";

                            //remove selected data from selected data list.
                            var i;
                            for (i = 0; i < lstSelectedData.length; i++) {
                                if (lstSelectedData[i].index === index) {
                                    lstSelectedData.splice(i, 1); break;
                                }
                            }

                        } else if (bgColor === "ffff00") { //selected plot
                            //remove selected data from selected data list.
                            var i;
                            for (i = 0; i < lstSelectedData.length; i++) {
                                if (lstSelectedData[i].index === index) {
                                    lstSelectedData.splice(i, 1); break;
                                }
                            }
                            bgColor = "ffffff";
                            radius = "3";
                            borderColor = "0372AB";
                        } else { //normal plot
                            if (lstSelectedData.length >= 2) {
                                alert("You selected maximum processes (2).");
                            } else {
                                bgColor = "ffff00";
                                radius = "5";
                                borderColor = "ff3300";

                                objSelectedData.index = index;
                                objSelectedData.selectedData = tempDataChart[index];
                                lstSelectedData.push(objSelectedData);
                            }                            
                        }
                        tempDataChart[index].anchorBgColor = bgColor;
                        tempDataChart[index].anchorRadius = radius;
                        tempDataChart[index].anchorBorderColor = borderColor;
                    }

                    var chartProp = ChartProperties();
                    chartProp.showValues = chartProp.showValues = $("#chkShowValue").is(":checked") ? "1" : "0";
                    var datasource = CreateDataSourceProperies(chartProp, tempCategories, tempDataChart);

                    $("#chart-container").updateFusionCharts({ dataSource: datasource, dataFormat: 'json' });
                    //$('#chart-container').updateFusionCharts({
                    //    'type': 'dragcolumn2d'
                    //}); 
                    */
                },
                'renderComplete': function (evt, arg) {
                    //function showData() {
                    //    var chartIns = evt && evt.sender,
                    //        data = chartIns && chartIns.getJSONData();
                    //    //alert(JSON.stringify(data));
                    //    var dt1 = " Q1: " + data.dataset[0].data[0].value;
                    //    var dt2 = "; Q2: " + data.dataset[0].data[1].value;
                    //    var dt3 = "; Q3: " + data.dataset[0].data[2].value;
                    //    var dt4 = "; Q4: " + data.dataset[0].data[3].value;
                    //    alert("samsung:" + dt1 + dt2 + dt3 + dt4);
                    //}
                    //document.getElementById("btnSaveBalancingTime").addEventListener("click", showData);
                },
                'datarestored': function (evtObj) {
                    var ds1Values = tempDataChart;//["300", "500", "400", "200"],                           
                    update = function (arr, rowNum) {
                        var i = 0,
                            arrLen = arr.length;

                        for (i; i < arrLen; i += 1) {
                            val = arr[i];
                            document.getElementById(rowNum + '-' + (i + 1)).innerHTML = val;
                        }
                    };
                    update(ds1Values, 1);
                }
            }
        });
        fusioncharts.render();
    });
}

function UpdateOpTimeBalancing(lstOpDetail) {
    var config = ObjectConfigAjaxPost("/Ops/UpdateOptimeBalancing", true, JSON.stringify({ lstOpDetail: lstOpDetail }));
    AjaxPostCommon(config, function (respone) {
        if (respone === Success) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, ObjMessageType.Info);
        } else {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error);
        }
    });
}

//#endregion


      

