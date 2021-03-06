﻿//#region variables        
window.chartColors = {
    red: 'rgb(255, 99, 132)',
    orange: 'rgb(255, 159, 64)',
    yellow: 'rgb(255, 205, 86)',
    green: 'rgb(75, 192, 192)',
    blue: 'rgb(54, 162, 235)',
    purple: 'rgb(153, 102, 255)',
    grey: 'rgb(201, 203, 207)'
};
//#endregion

function initLineChartDashboardPage() {
    Selection2("drpFactory");

    //SingleDatePicker("#txtDate");
    DateRangePicker("#txtDate", true);

    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    //Event button click
    eventButtonClick();

    //Event dropdownlist selection
    eventDropdownlistSelection();

    //Set height of div tag which wrap canvas
    setLineChartHeight();

    //Event click on chart
    eventClickOnChart();
}

function setLineChartHeight() {
    let hWindow = $(window).height();
    $("#divLineChart").height(hWindow - 250);
}

function getListActiveMesPkg(factoryId, plnStartDate, callbackFunc) {
    var config = ObjectConfigAjaxPost(
        "../LineDashboard/GetListActiveMesPkg"
        , false
        , JSON.stringify({
            factoryId: factoryId, yyyyMMdd: plnStartDate
        })
    );
    AjaxPostCommon(config, function (resData) {
        let listMxPkg;
        if (resData.IsSuccess) {
            listMxPkg = resData.Data;
        } else {
            listMxPkg = null;
        }
        callbackFunc(listMxPkg);
    });
}

//#region Lines Chart

function getStyleInformation(mxPackage, callbackFunc) {
    var config = ObjectConfigAjaxPost(
        "../LineDashboard/GetMxPackageInfo"
        , false
        , JSON.stringify({
            mxPackage: mxPackage
        })
    );
    AjaxPostCommon(config, function (resData) {
        let stlInf;
        if (resData.IsSuccess) {
            stlInf = resData.Data;
        } else {
            stlInf = null;
        }
        callbackFunc(stlInf);
    });
}

function createLineChart(chartLables, completedQty, remainQty, mxPackages, completedIot) {
    //Create chart lables and chart data
    var barChartData = {
        labels: chartLables,
        datasets: [{
            label: 'Completed',
            backgroundColor: window.chartColors.green,
            data: completedQty,
            MxPackage: mxPackages
        },
        {
            label: 'Remaining',
            backgroundColor: window.chartColors.red,
            data: remainQty,
            MxPackage: mxPackages
        },
        {
            label: 'Completed Iot',
            backgroundColor: window.chartColors.gray5,
            data: completedIot,
            MxPackage: mxPackages
        }]
    };

    //Create line chart
    createChart(barChartData);
}

var customTootip = function (tooltipModel) {
    //let stlImgLink = "http://203.113.151.204:8080/PKPDM/style/UNI/UNI0037/Images/UNI003714.jpg";
    let stlImgLink = "";
    let mxPkg = "";
    let styleInf = null;
    let mxTarget = 0;
    let completedQty = 0;
    let remainQty = 0;
    if (typeof tooltipModel.dataPoints !== 'undefined') {
        //Get the whole chart data
        var meta = lineBarChart.getDatasetMeta(0);

        //Get data point index (position of column which is hovered)
        let dataPoint = tooltipModel.dataPoints[0].index;

        //data[dataPoint]: data of column in stacked chart which is hovered
        //datasets[0]: Completed Qty data
        //MxPackage[dataPoint]: MxPackage of the column which is hovered
        mxPkg = meta.data[dataPoint]._chart.data.datasets[0].MxPackage[dataPoint].MxPackage;

        completedQty = meta.data[dataPoint]._chart.data.datasets[0].data[dataPoint];
        remainQty = meta.data[dataPoint]._chart.data.datasets[1].data[dataPoint];
        //Get completed qty
        mxTarget = meta.data[dataPoint]._chart.data.datasets[0].MxPackage[dataPoint].MxTarget;

        //console.log(mxPkg);

        //Get style information and get the style image link
        getStyleInformation(mxPkg, function (stlInf) {
            if (stlInf !== null) {
                stlImgLink = stlInf.ImageLink;
                styleInf = stlInf;
            }
        });
    }

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
        //var bodyLines = tooltipModel.body.map(getBody);

        var innerHtml = '<thead>';

        titleLines.forEach(function (title) {
            //innerHtml += '<tr><th>' + title + ' - ' + mxPkg + '</th></tr>';
            innerHtml += '<tr><th colspan="3">' + title + ' - ' + mxPkg + '</th></tr>';
        });
        innerHtml += '</thead><tbody>';

        //Add style information
        innerHtml += `<tr> <td style="white-space: nowrap;">Buyer Style Code: </td> <td style="white-space: nowrap;" class="tooltip-pad-left">${styleInf.BuyerStyleCode}</td> <td style="padding-left:5px" rowspan="9"><img class="img-responsive" src="${stlImgLink}" alt="Style image" style="max-width: 200px; max-height:150px; margin: 0 auto;"></td> </tr>`;
        innerHtml += `<tr> <td>Buyer Style Name: </td> <td style="white-space: nowrap;" class="tooltip-pad-left">${styleInf.BuyerStyleName}</td> </tr>`;
        innerHtml += `<tr> <td>PK Style Code: </td> <td class="tooltip-pad-left">${styleInf.StyleCode}</td> </tr>`;
        innerHtml += `<tr> <td>Style Size: </td> <td class="tooltip-pad-left">${styleInf.StyleSize}</td> </tr>`;
        innerHtml += `<tr> <td>Style Color: </td> <td class="tooltip-pad-left">${styleInf.StyleColorways}</td> </tr>`;
        innerHtml += `<tr> <td>RevNo: </td> <td class="tooltip-pad-left">${styleInf.RevNo}</td> </tr>`;
        innerHtml += `<tr> <td>Target: </td> <td class="tooltip-pad-left">${mxTarget}</td> </tr>`;
        innerHtml += `<tr> <td>Completed: </td> <td class="tooltip-pad-left">${completedQty}</td> </tr>`;
        innerHtml += `<tr> <td>Remaining: </td> <td class="tooltip-pad-left">${remainQty}</td> </tr>`;

        innerHtml += '</tbody>';

        var tableRoot = tooltipEl.querySelector('table');
        tableRoot.innerHTML = innerHtml;
    }

    // `this` will be the overall tooltip
    var position = this._chart.canvas.getBoundingClientRect();

    //console.log('---------------');
    //console.log('positionx: ' + position.x + 'position y: ' + position.y + 'position w: ' + position.width + 'position h: ' + position.height + 'position top: ' + position.top + 'position right: ' + position.right + 'position left: ' + position.left + 'position bottom: ' + position.bottom);
    //console.log('window width: ' + $(window).width());
    //console.log('position.left: ' + position.left);
    //console.log('tooltipModel.caretX: ' + tooltipModel.caretX); 
    //console.log('window.pageXOffset: ' + window.pageXOffset); 

    let wWindow = $(window).width();
    //Adjust position of x and y of tooltip
    let posX = 100;
    let posY = 100;

    //Check width of widow and position of column on chart
    if (tooltipModel.caretX + 350 > wWindow) {
        posX -= 270;
    }

    // Display, position, and set styles for font
    tooltipEl.style.opacity = 1;
    tooltipEl.style.position = 'absolute';
    tooltipEl.style.left = position.left + window.pageXOffset + tooltipModel.caretX + posX + 'px';
    tooltipEl.style.top = position.top + window.pageYOffset + tooltipModel.caretY - posY + 'px';
    //console.log('top pos: ' + (position.top + window.pageYOffset + tooltipModel.caretY));
    //console.log('top pos: ' + position.top + ', YOffset: ' + window.pageYOffset+ ', carteY: ' + tooltipModel.caretY);
    tooltipEl.style.fontFamily = tooltipModel._bodyFontFamily;
    tooltipEl.style.fontSize = tooltipModel.bodyFontSize + 'px';
    tooltipEl.style.fontStyle = tooltipModel._bodyFontStyle;
    tooltipEl.style.padding = tooltipModel.yPadding + 'px ' + tooltipModel.xPadding + 'px';
    tooltipEl.style.pointerEvents = 'none';
};

function createChart(lineChartData) {
    var ctx = document.getElementById('canvas').getContext('2d');

    window.lineBarChart = new Chart(ctx, {
        type: 'bar',
        data: lineChartData,
        options: {
            title: {
                display: false,
                text: 'Line Chart Dashboard'
            },
            tooltips: {
                mode: 'index',
                intersect: false,
                //#region custom tooltips
                // Disable the on-canvas tooltip
                enabled: false,
                custom: customTootip
                //#endregion
                //#region custom tooltip
                //// Disable the on-canvas tooltip
                //enabled: false,
                //custom: function (tooltipModel) {
                //    // Tooltip Element
                //    var tooltipEl = document.getElementById('chartjs-tooltip');

                //    // Create element on first render
                //    if (!tooltipEl) {
                //        tooltipEl = document.createElement('div');
                //        tooltipEl.id = 'chartjs-tooltip';
                //        tooltipEl.innerHTML = '<table></table>';
                //        document.body.appendChild(tooltipEl);
                //    }

                //    // Hide if no tooltip
                //    if (tooltipModel.opacity === 0) {
                //        tooltipEl.style.opacity = 0;
                //        return;
                //    }

                //    // Set caret Position
                //    tooltipEl.classList.remove('above', 'below', 'no-transform');
                //    if (tooltipModel.yAlign) {
                //        tooltipEl.classList.add(tooltipModel.yAlign);
                //    } else {
                //        tooltipEl.classList.add('no-transform');
                //    }

                //    function getBody(bodyItem) {
                //        return bodyItem.lines;
                //    }

                //    // Set Text
                //    if (tooltipModel.body) {
                //        var titleLines = tooltipModel.title || [];
                //        var bodyLines = tooltipModel.body.map(getBody);

                //        var innerHtml = '<thead>';

                //        titleLines.forEach(function (title) {
                //            innerHtml += '<tr><th>' + title + '</th></tr>';
                //        });
                //        innerHtml += '</thead><tbody>';

                //        bodyLines.forEach(function (body, i) {
                //            var colors = tooltipModel.labelColors[i];
                //            var style = 'background:' + colors.backgroundColor;
                //            style += '; border-color:' + colors.borderColor;
                //            style += '; border-width: 2px';
                //            var span = '<span style="' + style + '"></span>';
                //            innerHtml += '<tr><td>' + span + body + '</td></tr>';
                //        });
                //        innerHtml += '</tbody>';

                //        var tableRoot = tooltipEl.querySelector('table');
                //        tableRoot.innerHTML = innerHtml;
                //    }

                //    // `this` will be the overall tooltip
                //    var position = this._chart.canvas.getBoundingClientRect();

                //    // Display, position, and set styles for font
                //    tooltipEl.style.opacity = 1;
                //    tooltipEl.style.position = 'absolute';
                //    tooltipEl.style.left = position.left + window.pageXOffset + tooltipModel.caretX + 'px';
                //    tooltipEl.style.top = position.top + window.pageYOffset + tooltipModel.caretY + 'px';
                //    tooltipEl.style.fontFamily = tooltipModel._bodyFontFamily;
                //    tooltipEl.style.fontSize = tooltipModel.bodyFontSize + 'px';
                //    tooltipEl.style.fontStyle = tooltipModel._bodyFontStyle;
                //    tooltipEl.style.padding = tooltipModel.yPadding + 'px ' + tooltipModel.xPadding + 'px';
                //    tooltipEl.style.pointerEvents = 'none';
                //}
                //#endregion
            },
            responsive: true,
            scales: {
                xAxes: [{
                    stacked: true
                }],
                yAxes: [{
                    stacked: true
                }]
            },
            maintainAspectRatio: false,
            plugins: {
                datalabels: {
                    color: 'black',
                    labels: {
                        title: {
                            font: {
                                weight: '600',
                                size: 20
                            }
                        }
                    },
                    formatter: function (value) {
                        return value === 0 ? "" : value;
                    }

                }
            },
            //onClick: function (c, i) {
            //    //console.log(i);
            //    e = i[0];
            //    console.log(e);

            //    if (typeof e !== "undefined") {
            //        console.log('e: ' + e._index);
            //        var x_value = this.data.labels[e._index];
            //        var y_value = this.data.datasets[0].data[e._index];
            //        console.log('x value: ' + x_value);
            //        console.log('y value: ' + y_value);                    
            //        //window.location = "/MesManagement/FactorySummary?corp=" + fac;
            //    }

            //}
        }
    });
}

function eventClickOnChart() {
    var canvas = document.getElementById('canvas');

    canvas.onclick = function (evt) {
        var activePoint = lineBarChart.getElementAtEvent(evt)[0];
        if (typeof activePoint !== "undefined") {
            var data = activePoint._chart.data;
            //Get index where mouse click
            //var datasetIndex = activePoint._datasetIndex;

            //datasetIndex has 2 values 0 and 1. 0 is compledted, 1 is remain
            var labelCompleted = data.datasets[0].label;
            var labelRemain = data.datasets[1].label;
            var mxPkg = data.datasets[1].MxPackage;
            var valueCompleted = data.datasets[0].data[activePoint._index];
            var valueRemain = data.datasets[1].data[activePoint._index];

            console.log(labelCompleted + ": " + valueCompleted);
            console.log(labelRemain + ": " + valueRemain);
            console.log("MxPkg: " + mxPkg[activePoint._index]);

        }
    };
}

//document.getElementById('randomizeData').addEventListener('click', function () {
//    barChartData.datasets.forEach(function (dataset) {
//        dataset.data = dataset.data.map(function () {
//            return randomScalingFactor();
//        });
//    });
//    window.myBar.update();
//});

//#endregion
