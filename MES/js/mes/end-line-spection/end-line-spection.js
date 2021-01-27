//set date default
DateRangePicker("#beginDate", true);

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
Selection2("selOutputTarget");

function handleChangeFactory() {
    getList();

}

//handle change date
$("#beginDate").change(function () {
    getList();
});

function getList() {
    let factoryId = $("#selFactory").val();
    let plnStartDate = $("#beginDate").val().replace(/\//g, "");

    var config = ObjectConfigAjaxPost(
        "../UtilizationDefect/GetListEndLineSpection"
        , false,
        JSON.stringify({ factory: factoryId, date: plnStartDate })
    );
    AjaxPostCommon(config, function (res) {
        loadChart(res.Result);
    });
}

function loadChart(list) {
    console.log(list);
    document.getElementById('chart-machine-iot').innerHTML = "";
    //add element canvas before draw chart
    var chartMachineIot = document.getElementById('chart-machine-iot');
    var x = document.createElement(`CANVAS`);
    x.id = 'endline';
    chartMachineIot.appendChild(x);

    var obj = {};
    listLabel = [];
    listTotalOutPut = [];
    listDefect = [];
    listRft = [];
    listTotal = [];
    listMxTarget = [];
    listMxPackage = [];
    let category = $("#selOutputTarget").val();

    try {
        list.forEach(function (item) {
            let temp = 0;
            if (item.Output == 0 && category == "output") {
                return false;
            } else {
                //console.log(item)
            }

            if (category == "target") {
                temp = item.MxTarget;
            } else {
                temp = item.Output;
            }
            
            if (item.LineName.length > 0) {
                listLabel.push(item.LineName);
            } else {
                listLabel.push("Line temp");
            }
            let total = item.TotalDefect + temp + (temp - item.TotalDefect);
            let def = item.TotalDefect * 100 / total;
            let rft = (temp - item.TotalDefect) * 100 / total;
            listTotalOutPut.push(100 - def - rft);
            listDefect.push(def);
            listRft.push(rft);
            listTotal.push(total);
            listMxTarget.push(temp);
            listMxPackage.push(item.MxPackage);
        });
    }
    catch (ex) {
        console.log(ex);
    }

    obj.listLabel = listLabel;
    obj.listTotalOutPut = listTotalOutPut;
    obj.listDefect = listDefect;
    obj.listRft = listRft;
    obj.listTotal = listTotal;
    obj.listMxTarget = listMxTarget;
    obj.listMxPackage = listMxPackage;

    drawChart(obj);
}

function drawChart(obj) {
    //console.log(obj);
    var tooltipsLabel = obj.listLabel;
    var ctx = document.getElementById("endline").getContext('2d');
    var listTotal = obj.listTotal;
    var listMxTarget = obj.listMxTarget;
    var listMxPackage = obj.listMxPackage;
    var imageLink = "";
    var mesPackage = "";
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: obj.listLabel,
            datasets: [{
                label: 'TOTAL OUTPUT',
                //backgroundColor: "#FF0066",
                backgroundColor: "#90EE7E",
                data: obj.listTotalOutPut, 
            }, {
                label: 'DEFECT %',
                backgroundColor: "#F55B5B",
                data: obj.listDefect,
            }, {
                label: 'RFT',
                backgroundColor: "#2D8F90",
                data: obj.listRft,
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
                            innerHtml += '<tr><th style="padding-bottom: 17px">' + title + ' - ' + mesPackage + '</th></tr>';
                        });
                        innerHtml += '</thead><tbody>';
                        //Add style information
                        //innerHtml += `<tr> <td > ${mesPackage} </td> </tr>`;
                        innerHtml += `<tr> <td style="width: 100%; text-align: center;" > <img width="150" src="${imageLink}" /> </td></tr>`;
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
                        //console.log(data);
                        //console.log(tooltipItem);
                        let index = tooltipItem[0].index;
                        mesPackage = listMxPackage[index];
                        getStyleInformation(mesPackage, function (stlInf) {
                            if (stlInf !== null) {
                                imageLink = stlInf.ImageLink;
                            }
                        });
                        
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
                            //return formatIntToTime(label);
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
                    color: '#000',
                    font: {
                        size: 14
                    },
                    labels: {
                        title: {
                            font: {
                                weight: '600',
                                size: 25
                            }
                        }
                    },
                    //format value display column chart
                    formatter: function (value, context) {
                        retV = 0;
                        switch (context.datasetIndex) {
                            case 0:
                                retV = Math.round((value * listTotal[context.dataIndex] / 100) * 100) / 100;
                                return retV.toString();

                            case 1:
                                retV = Math.round(((value * listTotal[context.dataIndex] / 100) / listMxTarget[context.dataIndex] * 100) * 100) / 100;
                                return retV.toString() + '%';

                            case 2:
                                retV = Math.round(((value * listTotal[context.dataIndex] / 100) / listMxTarget[context.dataIndex] * 100) * 100) / 100;
                                return retV.toString() + '%';
                        }

                    }
                }
            },
        }
    });
}


function handleChangeOutputTarget() {
    getList();
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