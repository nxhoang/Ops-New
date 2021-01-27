//set date default
//DateRangePicker("#beginDate", true);

//get list factoty
function getListFactory() {
    var config = ObjectConfigAjaxPost(
        "../CorporationDashboard/GetListFactory"
        , false
    );
    AjaxPostCommon(config, function (resData) {
        if (resData.length > 0) {
            resData.forEach(function (val, ind) {
                $('#selFactory').append(`<option value="${val.Factory}">${val.Name}</option>`);
            });
        }
    });
}
getListFactory();
//set date default
InitDateRange();
Selection2("selFactory");
Selection2("selLine");
Selection2("selPackage");

function handleChangeFactory() {
    //set again value line
    $('#selLine')
        .empty()
        .append('<option selected="selected" value="">---</option>');
    //set again value package
    $('#selPackage')
        .empty()
        .append('<option selected="selected" value="">---</option>');

    let factoryId = $("#selFactory").val();
    var config = ObjectConfigAjaxPost("../UtilizationDefect/GetLineByFactory", false
        , JSON.stringify({ factoryId: factoryId }));
    AjaxPostCommon(config, function (res) {
        if (res.IsSuccess && res.Result.length > 0) {
            console.log(res);
            res.Result.forEach(function (val, ind) {
                $('#selLine').append(`<option value="${val.LineSerial}">${val.LineName}</option>`);
            });
        }
    });

    getList();
}

//handle change date
$("#txtDateRange").change(function () {
    getList();
});

function getList() {
    let factoryId = $("#selFactory").val();
    let lineId = $("#selLine").val();
    let arrDateRange = $('#txtDateRange').val().split('-');
    let startDate = $.trim(arrDateRange[0].replace(/\//g, ''));
    let endDate = $.trim(arrDateRange[1].replace(/\//g, ''));
    let package = $("#selPackage").val();

    var config = ObjectConfigAjaxPost(
        "../UtilizationDefect/GetListTotalDefect"
        , false,
        JSON.stringify({ factory: factoryId, lineId: lineId, startDate: startDate, endDate: endDate, package: package })
    );
    AjaxPostCommon(config, function (res) {
        loadChart(res.Result);
    });
}

function loadChart(list) {
    document.getElementById('chart-machine-iot').innerHTML = "";
    //add element canvas before draw chart
    var chartMachineIot = document.getElementById('chart-machine-iot');
    var x = document.createElement(`CANVAS`);
    x.id = 'maingroupdefect';
    chartMachineIot.appendChild(x);

    var obj = {};
    listLabel = [];
    listQtyDefect = [];

    try {
        list.forEach(function (item) {
            listLabel.push(item.DEFECTCATNAME);
            listQtyDefect.push(item.DEFQTY);
        });
    }
    catch (ex) {
        console.log(ex);
    }

    obj.listLabel = listLabel;
    obj.listQtyDefect = listQtyDefect;

    drawChart(obj);
}

function drawChart(obj) {
    let lineId = $("#selLine").val();
    let package = $("#selPackage").val();
    let arrDateRange = $('#txtDateRange').val().split('-');
    let startDate = $.trim(arrDateRange[0].replace(/\//g, ''));
    let endDate = $.trim(arrDateRange[1].replace(/\//g, ''));
    var canvas = document.getElementById("maingroupdefect");
    var ctx = canvas.getContext("2d");
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: obj.listLabel,
            datasets: [{
                label: 'MAIN DEFECT',
                backgroundColor: "#2D8F90",
                data: obj.listQtyDefect, 
            }],
            
        },
        options: { 
            tooltips: {
                // Disable the on-canvas tooltip
                enabled: false,
                custom: function (tooltipModel) {
                    //var listTime = this._chart.data.listTime;
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
                            innerHtml += '<tr><th style="padding-bottom: 17px">' + title + '</th></tr>';
                        });
                        innerHtml += '</thead><tbody>';
                        //Add style information
                        //innerHtml += `<tr> <td style="width: 100%; text-align: center;" > <img width="150" src="${imageLink}" /> </td></tr>`;
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
                //callbacks: {
                //    title: function (tooltipItem, data) {
                //        //console.log(data);
                //        //console.log(tooltipItem);
                //        let index = tooltipItem[0].index;
                //        mesPackage = listMxPackage[index];
                //        getStyleInformation(mesPackage, function (stlInf) {
                //            if (stlInf !== null) {
                //                imageLink = stlInf.ImageLink;
                //            }
                //        });
                        
                //        return tooltipsLabel[tooltipItem[0].index];
                //    }
                //}

            },
            scales: {
                xAxes: [{
                    stacked: true,
                    gridLines: {
                        display: false,
                    },
                    ticks: {
                        autoSkip: false,
                        fontSize: 12,
                        //callback: function (value, index, values) {
                        //    //convert value truc x
                        //    console.log(value, index, values);
                        //    return '$' + value;
                        //}
                    }
                }],
                yAxes: [{
                    stacked: true,
                    //format lables y
                    ticks: {
                        callback: function (label, index, labels) {
                            //return formatIntToTime(label);
                        },
                        //beginAtZero: true,
                        //padding: 50,
                    },
                    type: 'linear',
                    
                }]
            },
            responsive: true,
            maintainAspectRatio: false,
            legend: {
                display: true,
                //labels: {
                //    fontColor: 'rgb(255, 99, 132)'
                //}
            },
            plugins: {
                datalabels: {
                    //color: '#fff',
                    color: '#36A2EB',
                    font: {
                        size: 17,
                        weight: '600',
                    },
                    labels: {
                        title: {
                            font: {
                                weight: 'bold'
                            }
                        },
                        value: {
                            color: 'green'
                        }
                    },
                    display: false
                }
            },

            "hover": {
                "animationDuration": 0
            },
            "animation": {
                "duration": 1,
                "onComplete": function () {
                    var chartInstance = this.chart,
                        ctx = chartInstance.ctx;

                    ctx.font = Chart.helpers.fontString(Chart.defaults.global.defaultFontSize, Chart.defaults.global.defaultFontStyle, Chart.defaults.global.defaultFontFamily);
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'bottom';

                    this.data.datasets.forEach(function (dataset, i) {
                        var meta = chartInstance.controller.getDatasetMeta(i);
                        meta.data.forEach(function (bar, index) {
                            var data = dataset.data[index];
                            ctx.fillText(data, bar._model.x, bar._model.y - 5);
                        });
                    });
                }
            },
        }
    });

    canvas.onclick = function (evt) {
        
        var activePoints = myChart.getElementsAtEvent(evt);
        if (activePoints[0]) {
            var chartData = activePoints[0]['_chart'].config.data;
            var idx = activePoints[0]['_index'];

            var label = chartData.labels[idx];
            //var value = chartData.datasets[0].data[idx];

            var title = $("#exampleModalLabel").text(label);
            //console.log(title);

            var res = label.split("-");
            //get data detail when click one column
            var config = ObjectConfigAjaxPost(
                "../UtilizationDefect/GetDetailDefect"
                , false
                , JSON.stringify({
                    defectCat: res[0],
                    startDate: startDate,
                    endDate: endDate,
                    lineId: lineId,
                    package: package
                })
            );
            AjaxPostCommon(config, function (r) {
                if (r.IsSuccess) {
                    loadChartModal(r.Result);
                } 
            });

            $('#exampleModal').modal('show');
        }
    };
}

//when change screen auto draw agian
window.addEventListener('resize', getList);

//draw chart after 5 minute
window.setInterval(function () {
    /// call your function here
    let factoryId = $("#selFactory").val();
    if (factoryId != 0) {
        getList();
    }

}, 300000);

function loadChartModal(list) {
    document.getElementById('modal-chart').innerHTML = "";
    //add element canvas before draw chart
    var chartMachineIot = document.getElementById('modal-chart');
    var x = document.createElement(`CANVAS`);
    x.id = 'modaldefect';
    chartMachineIot.appendChild(x);

    var obj = {};
    listLabel = [];
    listQtyDefect = [];

    try {
        list.forEach(function (item) {
            listLabel.push(item.DEFECTDESC);
            listQtyDefect.push(item.DEFQTY);
        });
    }
    catch (ex) {
        console.log(ex);
    }

    obj.listLabel = listLabel;
    obj.listQtyDefect = listQtyDefect;

    drawChartModal(obj);
}

function drawChartModal(obj) {
    //console.log(obj);
    var canvas = document.getElementById("modaldefect");
    var ctx = canvas.getContext("2d");
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: obj.listLabel,
            datasets: [{
                label: 'SUB DEFECT',
                backgroundColor: "#90EE7E",
                data: obj.listQtyDefect,
            }],

        },
        options: {
            tooltips: {
                // Disable the on-canvas tooltip
                enabled: false,
                custom: function (tooltipModel) {
                    //var listTime = this._chart.data.listTime;
                    // Tooltip Element
                    var tooltipEl = document.getElementById('chartjs-tooltip2');

                    // Create element on first render
                    if (!tooltipEl) {
                        tooltipEl = document.createElement('div');
                        tooltipEl.id = 'chartjs-tooltip2';
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
                            innerHtml += '<tr><th style="padding-bottom: 17px">' + title + '</th></tr>';
                        });
                        innerHtml += '</thead><tbody>';
                        //Add style information
                        //innerHtml += `<tr> <td style="width: 100%; text-align: center;" > <img width="150" src="${imageLink}" /> </td></tr>`;
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
                            //return formatIntToTime(label);
                        }
                    },
                    type: 'linear',
                }]
            },
            responsive: true,
            maintainAspectRatio: false,
            legend: {
                display: true,
            },
            plugins: {
                datalabels: {
                    color: '#fff',
                    font: {
                        size: 14,
                        weight: '600',
                    },
                    labels: {
                        title: {
                            
                        },
                        font: {
                            weight: '600',
                            size: 65
                        }
                    },
                    display: false
                }
            },
            "hover": {
                "animationDuration": 0
            },
            "animation": {
                "duration": 1,
                "onComplete": function () {
                    var chartInstance = this.chart,
                        ctx = chartInstance.ctx;

                    ctx.font = Chart.helpers.fontString(Chart.defaults.global.defaultFontSize, Chart.defaults.global.defaultFontStyle, Chart.defaults.global.defaultFontFamily);
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'bottom';

                    this.data.datasets.forEach(function (dataset, i) {
                        var meta = chartInstance.controller.getDatasetMeta(i);
                        meta.data.forEach(function (bar, index) {
                            var data = dataset.data[index];
                            ctx.fillText(data, bar._model.x, bar._model.y - 5);
                        });
                    });
                }
            },
            
        }
    });

    
}

function handleChangeLine() {
    let factoryId = $("#selFactory").val();
    let lineId = $("#selLine").val();
    let arrDateRange = $('#txtDateRange').val().split('-');
    let startDate = $.trim(arrDateRange[0].replace(/\//g, ''));
    let endDate = $.trim(arrDateRange[1].replace(/\//g, ''));

    //set again value package
    $('#selPackage')
        .empty()
        .append('<option selected="selected" value="">---</option>');

    var config = ObjectConfigAjaxPost("../UtilizationDefect/GetPackageByFactoryLine", false
        , JSON.stringify({ factoryId: factoryId, lineId: lineId, startDate: startDate, endDate: endDate }));
    AjaxPostCommon(config, function (res) {
        console.log(res);
        if (res.IsSuccess && res.Result.length > 0) {
            res.Result.forEach(function (val, ind) {
                $('#selPackage').append(`<option value="${val.MxPackage}">${val.MxPackage}</option>`);
            });
        }
    });

    getList();
}

function handleChangePackage() {
    getList();
}

//Init date range and set date default
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
    $('#txtDateRange').data('daterangepicker').setEndDate(getCurrentDate(6));

}