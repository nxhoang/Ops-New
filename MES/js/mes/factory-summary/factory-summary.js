
var ctx = document.getElementById('cvFactory').getContext('2d');
var chartFac;

function InitPage() {

    var corpId = getUrlParameter("corp");
    
    GetChartDataLabels(corpId, function (lstFact) {
        var chartData = CreateChartData(lstFact);
        var configChart = CreateChartConfig(chartData);


        var ctx2 = document.getElementById("cvFactory");
        ctx2.height = 100;

        chartFac = new Chart(ctx, configChart);
    });

    //Get list of corporation
    GetFactoryByCorporation("drpCorporation");

    //Event change corporation
    EventSelectCorporation();
}

//#region Get master data
function GetFactoryByCorporation(controlId) {
    GetCompanyCorporation(function (lstFac) {
       
        FillDataToDropDownlist(controlId, lstFac, "DeptCode", "ShortName");
    });
}

//#endregion

//#region chart.js

function GetChartDataLabels(corpId, callBackFunc) {
    var lstFac = [];
    GetFactoriesByCorporation(corpId, function (lstCom) {
        $.each(lstCom, function (index, value) {
            lstFac.push(value.Name);
        });

        callBackFunc(lstFac);
    });

    
}

function CreateChartData(labelsChart) {
    var barChartData2 = {
        labels: labelsChart,
        datasets: [
            {
                label: 'Daily target',
                backgroundColor: convertColorHexToRgbOpacity(chartColors.yellow, 0.5),
                borderColor: chartColors.yellow,
                borderWidth: 1,
                hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.yellow, 0.8),
                data: [10000, 300, 800, 0, 0, 0, 0, 0, 0, 0, 0]
            },
            {
                label: 'Completed',
                backgroundColor: convertColorHexToRgbOpacity(chartColors.green, 0.5),
                borderColor: chartColors.green,
                borderWidth: 1,
                hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.green, 0.8),
                data: [9000, 250, 600, 0, 0, 0, 0, 0, 0, 0, 0]
            },
            {
                label: 'Remain',
                backgroundColor: convertColorHexToRgbOpacity(chartColors.red, 0.5),
                borderColor: chartColors.red,
                borderWidth: 1,
                hoverBackgroundColor: convertColorHexToRgbOpacity(chartColors.red, 0.8),
                data: [5000, 50, 200, 0, 0, 0, 0, 0, 0, 0, 0]
            }
        ]

    };

    return barChartData2;
}

function CreateChartConfig(chartData) {

    var configChart = {
        type: 'bar',
        data: chartData,
        options: {
            tooltips: {
                enabled: false
            },
            responsive: true,
            legend: {
                position: 'top',
            },
            title: {
                display: true,
                text: 'Corporation Factories'
            },
            onClick: function (c, i) {
                e = i[0];
                if (typeof e !== "undefined") {
                    console.log(e._index);
                    var x_value = this.data.labels[e._index];
                    var y_value = this.data.datasets[0].data[e._index];
                    console.log(x_value);
                    console.log(y_value);

                    //window.location = "http://localhost:60275?factory=" + x_value;
                    //window.location = "/dashboard.aspx?factory=" + x_value;
                }

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
                        return context.dataset.data[context.dataIndex] > 15;
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
    };

    return configChart;
}

//#endregion