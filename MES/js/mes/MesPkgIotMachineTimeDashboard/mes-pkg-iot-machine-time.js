//get list factoty
function getListFactory() {
    var config = ObjectConfigAjaxPost(
        "../CorporationDashboard/GetListFactory"
        , false
    );
    AjaxPostCommon(config, function (resData) {
        resData.forEach(function (val, ind) {
            $('#selFactory').append(`<option value="${val.Factory}">${val.Name}</option>`);
        });
    });
}
getListFactory();
Selection2("selFactory");

//set date default
DateRangePicker("#beginDate", true);

//get mes package id
function GetPkgByFacAndDate(factory, plnDate) {
    var list = [];
    if (!factory) return; //user must select the factory

    var config = ObjectConfigAjaxPost(
        "/MesPkgIotMachineTimeDashboard/GetMesPkgByDate",
        false,
        JSON.stringify({ factory: factory, date: plnDate })
    );

    AjaxPostCommon(config, function (lstPackage) {
        if (lstPackage && lstPackage.Result && lstPackage.Result.length > 0) {
            list = lstPackage.Result;
            //let plnStartDate = $("#beginDate").val().replace(/\//g, "-");
            //lstPackage.Result.map((item) => {
            //    //after get list MES Package then get detail of MES Package
            //    let el = getInfoDetailChart(item.MxPackage, plnStartDate);
            //    if (el != null) {
            //        list.push(el);
            //    }
            //})
        }
    });

    return list;
}

function handleChangeFactory() {
    resetTag();
    //show grid list MES package
    $("#list3").jqGrid('setGridState', 'visible');
    getMesPackageId();
    document.getElementById("info-detail-chart").innerHTML = "";
}

//clear select mes package
function clearSelectMesPkg() {
    var select = document.getElementById("mesPkgId");
    var length = select.options.length;
    for (i = length - 1; i >= 0; i--) {
        select.options[i] = null;
    }
}

//handle change date
$("#beginDate").change(function () {
    resetTag();
    //show grid list MES package
    $("#list3").jqGrid('setGridState', 'visible');
    getMesPackageId();
    document.getElementById("info-detail-chart").innerHTML = "";
});

//get mes package by factoryId and date
function getMesPackageId() {
    //clearSelectMesPkg();
    let factoryId = $("#selFactory").val();
    let plnStartDate = $("#beginDate").val().replace(/\//g, "");

    let res = GetPkgByFacAndDate(factoryId, plnStartDate);
    if (res.length > 0) {
        displayListMesPackage(res);
        //$('#mesPkgId').append('<option value="">---</option>');
        //res.forEach(function (val, ind) {
        //    $('#mesPkgId').append(`<option value="${val.MxPackage}">${val.MxPackage}</option>`);
        //});
    } else {
        var e = document.getElementById("selFactory");
        var strText = e.options[e.selectedIndex].text;
        let mesTitle = 'Warning';
        let mesContent = 'Factory ' + strText + ' has no package on ' + $("#beginDate").val() + '.<br />Please select other factory or other date';
        ShowMessage(mesTitle, mesContent, 'warning');
        resetGrid();
    }
    //Selection2("mesPkgId");
}

// handle change mes package
function handleChangeMesPkg() {
    resetTag();
    $.blockUI();
    $.blockUI({ css: { backgroundColor: '#fff', color: '#5192ca' } });
    setTimeout(function () {
        getDataToDisplayChart();
        $.unblockUI();
    }, 100);

}

function getDataToDisplayChart(mesPackage) {
    //let mesPackage = $("#mesPkgId").val();
    let plnStartDate = $("#beginDate").val().replace(/\//g, "-");
    if (!mesPackage) return; //user must select the factory

    var config = ObjectConfigAjaxPost(
        "/MesPkgIotMachineTimeDashboard/DisplayDashboard",
        false,
        JSON.stringify({ mesPkg: mesPackage, date: plnStartDate })
    );

    AjaxPostCommon(config, function (lstPackage) {
        if (lstPackage && lstPackage.IsSuccess && lstPackage.Result.length > 0) {
            list = lstPackage.Result;
            loadChart(list, mesPackage, plnStartDate);
        } else {
            let mesTitle = 'Warning';
            let mesContent = 'Mes Package not has Iot. Please select other mes package or other factory or other date.';
            ShowMessage(mesTitle, mesContent, 'warning');
        }
    });
}

function loadChart(list, mesPackage, plnStartDate) {
    //console.log(mesPackage);
    listIot = [];
    listOpGrp = [];
    listPowerTime = [];
    listMotoTime = [];
    listMcRunTume = [];
    listTime = [];
    listOpName = [];

    var obj = {};

    try {
        list.forEach(function (item) {
            var time = {};
            var timeMaxMin = {};
            let max_actime = item.MaxActTime - item.MaxMotoTime;
            let max_powerTime = item.MaxPowerTime - (item.MaxActTime > 0 ? item.MaxActTime : item.MaxMotoTime);

            listIot.push(item.VanCount > 1 ? item.MachineId + ' (' + item.VanCount + ')' : item.MachineId);
            listOpGrp.push(item.OpGroupName);
            listOpName.push(item.OpName);
            listPowerTime.push(max_powerTime > 0 ? max_powerTime : null);
            listMotoTime.push(item.MaxMotoTime > 0 ? item.MaxMotoTime : null);
            listMcRunTume.push(max_actime > 0 ? max_actime : null);
            //add time min and max to object 
            timeMaxMin.MinDate = item.MinDate;
            timeMaxMin.MaxDate = item.MaxDate;
            timeMaxMin.MinPowerTime = item.MinPowerTime;
            timeMaxMin.MaxPowerTime = item.MaxPowerTime;
            //add machineId and time min max to object 
            time.OpName = item.OpName;
            time.timeMaxMin = timeMaxMin;
            time.OpGroupName = item.OpGroupName;

            listTime.push(time);
        });
    } catch (ex) {
        console.log(ex);
    }
    obj.listIot = { opGroupName: listOpGrp, opName: listOpName, lstIot: listIot };
    obj.listPowerTime = listPowerTime;
    obj.listMotoTime = listMotoTime;
    obj.listMcRunTume = listMcRunTume;
    obj.listTime = listTime;

    document.getElementById('chart-machine-iot').innerHTML = "";
    //add element canvas before draw chart
    var chartMachineIot = document.getElementById('chart-machine-iot');
    var x = document.createElement(`CANVAS`);
    x.id = 'myChart4';
    chartMachineIot.appendChild(x);
    drawChart(obj);
    $("#list3").jqGrid('setGridState', 'hidden');
    //displayInfoDetailChart(mesPackage, plnStartDate);
    displayEfficiency(list, mesPackage);
}

function drawChart(obj) {
    var tooltipsLabel = obj.listIot.opName;
    var ctx = document.getElementById("myChart4").getContext('2d');
    var startTime = '';
    var endTime = '';
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: obj.listIot.lstIot,
            datasets: [{
                label: 'Motor time',
                backgroundColor: "#F55B5B",
                data: obj.listMotoTime,
            }, {
                label: 'MC run time',
                backgroundColor: "#90EE7E",
                data: obj.listMcRunTume,
            }, {
                label: 'Power time',
                backgroundColor: "#2D8F90",
                data: obj.listPowerTime,
                }],
            listTime: obj.listTime
        },
        options: {
           
            tooltips: {
                // Disable the on-canvas tooltip
                enabled: false,
                custom: function (tooltipModel) {
                    var listTime = this._chart.data.listTime;
                    // Tooltip Element
                    var tooltipEl = document.getElementById('chartjs-tooltip');

                    // Create element on first render
                    if (!tooltipEl) {
                        tooltipEl = document.createElement('div');
                        tooltipEl.id = 'chartjs-tooltip';
                        tooltipEl.innerHTML = '<table></table>';
                        document.body.appendChild(tooltipEl);
                    }

                    // Hide if no tooltip
                    if (tooltipModel.opacity === 0) {
                        tooltipEl.style.opacity = 0;
                        return;
                    }

                    // Set caret Position
                    tooltipEl.classList.remove('above', 'below', 'no-transform');
                    if (tooltipModel.yAlign) {
                        tooltipEl.classList.add(tooltipModel.yAlign);
                    } else {
                        tooltipEl.classList.add('no-transform');
                    }

                    function getBody(bodyItem) {
                        return bodyItem.lines;
                    }

                    // Set Text
                    if (tooltipModel.body) {
                        var titleLines = tooltipModel.title || [];
                        var innerHtml = '<thead>';

                        titleLines.forEach(function (title) {
                            
                            innerHtml += '<tr><th colspan="3">' + 'OpName - ' + title + '</th></tr>';

                        });
                        innerHtml += '</thead><tbody>';

                        //Add style information
                        innerHtml += `<tr> <td>Time start: </td> <td class="tooltip-pad-left" id="start-time" > ${startTime} </td> </tr>`;
                        innerHtml += `<tr> <td>Time end: </td> <td class="tooltip-pad-left"> ${endTime} </td> </tr>`;


                        innerHtml += '</tbody>';

                        var tableRoot = tooltipEl.querySelector('table');
                        tableRoot.innerHTML = innerHtml;
                    }

                    // `this` will be the overall tooltip
                    var position = this._chart.canvas.getBoundingClientRect();

                    // Display, position, and set styles for font
                    tooltipEl.style.opacity = 1;
                    tooltipEl.style.position = 'absolute';
                    tooltipEl.style.left = position.left + window.pageXOffset + tooltipModel.caretX + 'px';
                    tooltipEl.style.top = position.top + window.pageYOffset + tooltipModel.caretY + 'px';
                    tooltipEl.style.fontFamily = tooltipModel._bodyFontFamily;
                    tooltipEl.style.fontSize = tooltipModel.bodyFontSize + 'px';
                    tooltipEl.style.fontStyle = tooltipModel._bodyFontStyle;
                    tooltipEl.style.padding = tooltipModel.yPadding + 'px ' + tooltipModel.xPadding + 'px';
                    tooltipEl.style.pointerEvents = 'none';
                },
                callbacks: {
                    title: function (tooltipItem, data) {
                        listTime.map((item) => {
                            if (item.OpName == tooltipsLabel[tooltipItem[0].index]) {
                                //console.log(item);
                                var hms = item.timeMaxMin.MinDate.substr(-8);   // input string
                                var a = hms.split(':'); // split it at the colons
                                var seconds = (+a[0]) * 60 * 60 + (+a[1]) * 60 + (+a[2]); 
                                start_sec = seconds - item.timeMaxMin.MinPowerTime;
                                startTime = formatIntToTime(start_sec);
                                endTime = item.timeMaxMin.MaxDate.substr(-8);
                                //console.log(start_sec);
                            }
                        })
                        return tooltipsLabel[tooltipItem[0].index];
                    }
                }

            },
            scales: {
                xAxes: [{
                    stacked: true,
                    gridLines: {
                        display: false,
                    },
                    ticks: {
                        autoSkip: false
                    }
                }],
                yAxes: [{
                    stacked: true,
                    //format lables y
                    ticks: {
                        callback: function (label, index, labels) {
                            return formatIntToTime(label);
                        }
                    },
                    type: 'linear',
                }]
            },
            responsive: true,
            maintainAspectRatio: false,
            //legend: { position: 'bottom' },
            plugins: {
                datalabels: {
                    color: 'black',
                    labels: {
                        title: {
                            font: {
                                weight: '600',
                                size: 13
                            }
                        }
                    },
                    //format value display column chart
                    formatter: function (value, context) {
                        retV = 0;
                        switch (context.datasetIndex) {
                            
                            case 0:
                                retV = value;
                                return formatIntToTime(retV);

                            case 1:
                                retV = value + context.chart.data.datasets[context.datasetIndex - 1].data[context.dataIndex];
                                return formatIntToTime(retV);

                            case 2:
                                retV = value + context.chart.data.datasets[context.datasetIndex - 2].data[context.dataIndex] + context.chart.data.datasets[context.datasetIndex - 1].data[context.dataIndex];
                                return formatIntToTime(retV);
                        }

                    }
                }
            },
        }
    });
}

function formatIntToTime(sec_num) {
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = sec_num - (hours * 3600) - (minutes * 60);

    if (hours < 10) { hours = "0" + hours; }
    if (minutes < 10) { minutes = "0" + minutes; }
    if (seconds < 10) { seconds = "0" + seconds; }

    return hours + ':' + minutes + ':' + seconds;
}

//display info detail of mespackage
function displayInfoDetailChart(mesPackage, dateStart) {
    
    var config = ObjectConfigAjaxPost(
        "/MesPkgIotMachineTimeDashboard/GetInfoDetailchart",
        false,
        JSON.stringify({ mesPkg: mesPackage, date: dateStart })
    );

    AjaxPostCommon(config, function (res) {
        if (res.IsSuccess && res.Data) {
            var data = res.Data[0];

            var info = document.getElementById("info-detail-chart");
            info.innerHTML = "";

            var tbl = document.createElement("TABLE");
            tbl.setAttribute("id", "table-info");
            info.appendChild(tbl);

            var table = document.getElementById("table-info");

            var row1 = table.insertRow(0);
            var cell0 = row1.insertCell(0);
            cell0.id = 'rowspan-mes-pkg';
            var cell1 = row1.insertCell(1);
            var cell2 = row1.insertCell(2);
            var cell3 = row1.insertCell(3);
            var cell4 = row1.insertCell(4);
            var cell5 = row1.insertCell(5);
            var cell6 = row1.insertCell(6);
            var cell7 = row1.insertCell(7);
            var cell8 = row1.insertCell(8);
            cell0.innerHTML = data.MXPACKAGE;
            cell1.innerHTML = "Line";
            cell2.innerHTML = "Style code";
            cell3.innerHTML = "Style name";
            cell4.innerHTML = "Style size";
            cell5.innerHTML = "Style color";
            cell6.innerHTML = "Color name";
            cell7.innerHTML = "Revision";
            cell8.innerHTML = "Target";

            var row2 = table.insertRow(1);
            row2.id = 'row-value-mes-pkg';
            var cell2_1 = row2.insertCell(0);
            var cell2_2 = row2.insertCell(1);
            var cell2_3 = row2.insertCell(2);
            var cell2_4 = row2.insertCell(3);
            var cell2_5 = row2.insertCell(4);
            var cell2_6 = row2.insertCell(5);
            var cell2_7 = row2.insertCell(6);
            var cell2_8 = row2.insertCell(7);
            cell2_1.innerHTML = data.LINENAME;
            cell2_2.innerHTML = data.STYLECODE;
            cell2_3.innerHTML = data.STYLENAME;
            cell2_4.innerHTML = data.STYLESIZE;
            cell2_5.innerHTML = data.STYLECOLORSERIAL;
            cell2_6.innerHTML = data.STYLECOLORWAYS;
            cell2_7.innerHTML = data.REVNO;
            cell2_8.innerHTML = data.MXTARGET;

            document.getElementById("buyer-style-name").innerHTML = data.BUYERSTYLENAME;
            document.getElementById("label-buyer-st-name").innerHTML = data.BUYERSTYLENAME;
            
            document.getElementById("rowspan-mes-pkg").rowSpan = "2";

            getImageByPackage(mesPackage);

        } else {
            console.log('No data');
        }
    });
}

function getImageByPackage(mes_package) {
    var config = ObjectConfigAjaxPost(
        "/MesPkgIotMachineTimeDashboard/GetImageByMesPacKage",
        false,
        JSON.stringify({ mesPkg: mes_package })
    );

    AjaxPostCommon(config, function (res) {
        //Set source for image
        $("#imgMesIot").attr("src", res.ImageLink);
    });
}

function resetTag() {
    document.getElementById('chart-machine-iot').innerHTML = "";
    //document.getElementById('info-detail-chart').innerHTML = "";
    document.getElementById("buyer-style-name").innerHTML = "";
    //document.getElementById("label-buyer-st-name").innerHTML = "";
    //$("#imgMesIot").attr("src", "/img/no-image.png");
}

//display grid list MES package 
function displayListMesPackage(res) {
    //clear grid data
    resetGrid();
    let factoryText = $("#selFactory option:selected").text();
    //console.log(res)
    $("#list3").jqGrid({
        data: res,
        datatype: "local",
        colNames: ['MES Package ID', 'AONo', 'Line', 'Style code', 'Buyer Style Name', 'Style size', 'Style color', 'Color name', 'RevNo', 'Target'],
        colModel: [
            { name: 'MxPackage', index: 'MxPackage'},
            { name: 'AoNo', index: 'AoNo', width: 80 },
            { name: 'LineName', index: 'LineName', width: 70 },
            { name: 'StyleCode', index: 'StyleCode', width: 80 },
            { name: 'BuyerStyleName', index: 'BuyerStyleName', formatter: formatMesPackage},
            //{ name: 'StyleName', index: 'StyleName'},
            { name: 'StyleSize', index: 'StyleSize', width: 70 },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', width: 60, align: 'right' },
            { name: 'StyleColorWays', index: 'StyleColorWays', width: 100 },
            { name: 'RevNo', index: 'RevNo', width: 60, align: 'right' },
            { name: 'MxTarget', index: 'MxTarget', width: 60, align: 'right' }
        ],
        setGridWidth: '500px',
        rowNum: 10,
        rowList: [10, 20, 30, 50, 100],
        pager: '#pager3',
        sortname: 'id',
        viewrecords: true,
        sortorder: "desc",
        loadonce: true,
        caption: factoryText + " Packages",
        onSelectRow: function (id) {
            var rowId = $("#list3").getRowData(id);
            //console.log(rowId);
            var MxPackage = rowId['MxPackage']; //($(rowId['MxPackage'])).html();
            //var htmlObject = $(MxPackage);
            //console.log(htmlObject.html());
            //get data and draw chart
            resetTag();
            $.blockUI();
            $.blockUI({ css: { backgroundColor: '#fff', color: '#5192ca' } });
            setTimeout(function () {
                getDataToDisplayChart(MxPackage);
                $.unblockUI();
            }, 100);

             //set BuyerStyleName for title chart
            document.getElementById("buyer-style-name").innerHTML = rowId['BuyerStyleName'];
            //document.getElementById("label-buyer-st-name").innerHTML = rowId['BuyerStyleName'];
            //set url image product
            //getImageByPackage(MxPackage);
        },
    });

    
    var DataGrid = $('#list3');
    DataGrid.jqGrid('setGridWidth', parseInt($(window).width()) - 20);

    //handles the grid resize on window resize
    $(window).resize(function () {
        DataGrid.jqGrid('setGridWidth', parseInt($(window).width()) - 20);
    });

    //set data when change date or factory
    $("#list3")
        .jqGrid('setGridParam',
            {
                datatype: 'local',
                data: res
            })
        .trigger("reloadGrid");

    var $grid = $('#list3');
    $grid.jqGrid('setCaption', factoryText.split(']').pop() + ' Packages');
}

//get info detail of mespackage
function getInfoDetailChart(mesPackage, dateStart) {
    var data = null;
    var config = ObjectConfigAjaxPost(
        "/MesPkgIotMachineTimeDashboard/GetInfoDetailchart",
        false,
        JSON.stringify({ mesPkg: mesPackage, date: dateStart })
    );

    AjaxPostCommon(config, function (res) {
        if (res.IsSuccess && res.Data) {
            data = res.Data[0];
        }
    });

    return data;
}

//clear grid data
function resetGrid() {
    var numberOfRecords = $("#list3").getGridParam("records");
    if (numberOfRecords) {
        $('#list3').jqGrid("clearGridData");
    }
}

function formatMesPackage(cellvalue, options, rowObject) {
    return '<a href="#" class="qtip-pkgGroup" data-role="quickSummary" data-summaryurl="/MesPkgIotMachineTimeDashboard/GetImageByMesPacKage/?mesPkg=' + rowObject.MxPackage + '" >' + rowObject.BuyerStyleName + '</a>';
}

function displayEfficiency(arr, mesPackage) {
    var info = document.getElementById("info-detail-chart");
    info.innerHTML = "";

    var tbl = document.createElement("TABLE");
    tbl.setAttribute("id", "table-info");
    info.appendChild(tbl);

    var table = document.getElementById("table-info");
    var row0 = table.insertRow(0);
    row0.id = 'machineId';
    var row1 = table.insertRow(1);
    row1.id = 'operation';
    var row2 = table.insertRow(2);
    row2.id = 'efficiency';
    var row3 = table.insertRow(3);
    row3.id = 'cycletime';
    var totalPercents = 0;
    var countAvg = arr.length;

    arr.forEach(function (item, idx) {

        var _cell0 = 'cell' + idx;
        _cell0 = row0.insertCell(idx);
        _cell0.innerHTML = item.MachineId;

        var _cell1 = 'cell1' + idx;
        _cell1 = row1.insertCell(idx);
        _cell1.innerHTML = item.OpName;

        var _cell2 = 'cell2' + idx;
        _cell2 = row2.insertCell(idx);
        _cell2.classList.add("right");
        if (item.MaxPowerTime == 0 || item.MaxActTime == 0) {
            _cell2.innerHTML = '0%';
        } else {
            let precent = (item.MaxActTime * 100 / item.MaxPowerTime).toFixed(2);
            _cell2.innerHTML = precent + '%';
            totalPercents = totalPercents + parseFloat(precent);
        }

        var qty = Math.max(item.LastIotData, item.LastIotDataDgs);
        var _cell3 = 'cell3' + idx;
        _cell3 = row3.insertCell(idx);
        _cell3.classList.add("right");
        if (qty == 0) {
            _cell3.innerHTML = 0;
        } else {
            _cell3.innerHTML = (item.MaxActTime / qty).toFixed(0);
        }
        

    });

    var machineId = document.getElementById("machineId");
    var x = machineId.insertCell(0);
    x.classList.add("width150");
    x.innerHTML = "MachineId";

    var operation = document.getElementById("operation");
    var a = operation.insertCell(0);
    a.classList.add("first-opname");
    a.innerHTML = "Operation name";

    var efficiency = document.getElementById("efficiency");
    var y = efficiency.insertCell(0);
    y.innerHTML = "Efficiency(%)";
    let avgPercents = Math.round((totalPercents / countAvg) * 100) / 100;
    var y1 = efficiency.insertCell(-1);
    y1.classList.add("avg");
    y1.innerHTML = avgPercents + "%";
    var y2 = efficiency.insertCell(-1);
    y2.classList.add("text-last");
    y2.innerHTML = "Average efficiency";

    var cycletime = document.getElementById("cycletime");
    var z = cycletime.insertCell(0);
    z.innerHTML = "Cycle Time(sec)";
    let lobRate = getLobRate(mesPackage);
    var z1 = cycletime.insertCell(-1);
    z1.classList.add("avg");
    z1.innerHTML = Math.round(lobRate * 100) / 100;
    var z2 = cycletime.insertCell(-1);
    z2.classList.add("text-last");
    z2.innerHTML = "LOB rate";

}

//get LOB rate
function getLobRate(mesPackage) {
    var data = null;
    var config = ObjectConfigAjaxPost(
        "/MesPkgIotMachineTimeDashboard/getLobRate",
        false,
        JSON.stringify({ mesPkg: mesPackage })
    );

    AjaxPostCommon(config, function (res) {
        if (res.IsSuccess && res.Result) {
            data = res.Result[0].LOB;
        }
    });

    return data;
}





