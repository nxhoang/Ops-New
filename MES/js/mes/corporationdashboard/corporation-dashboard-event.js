function eventChangedate() {
    $("#txtDate").change(function () {
        $.blockUI();
        $.blockUI({ css: { backgroundColor: '#fff', color: '#5192ca' } });
        setTimeout(function () {
            document.getElementById('chartCorporation').innerHTML = "";
            loadLineChart();
            $.unblockUI();
        }, 100);
    });
}

function eventLoadCharTheFirst() {
    $.blockUI();
    $.blockUI({ css: { backgroundColor: '#fff', color: '#5192ca' } });
    setTimeout(function () {
        loadLineChart();
        $.unblockUI();
    }, 100);
}

function getFactoryHaveData(resFactory) {
    let plnStartDate = $("#txtDate").val().replace(/\//g, "");
    var listFactory = [];
    resFactory.forEach(function (item) {

        getListActiveMesPkg(item, plnStartDate, function (listPkg) {
            if (listPkg.length > 0) {
                $.each(listPkg, function (idx, pkg) {
                    if (pkg.MxTarget > 0) {
                        listFactory.push({ factoryId: item, factoryName: pkg.factoryName });
                        //listFactory.push(item);
                        return false;
                    } else {
                        return false;
                    }
                });
            }
        });
    });

    return listFactory;
}

function loadLineChart() {
    if (typeof lineBarChart !== "undefined") {
        //Clear bar chart
        lineBarChart.destroy();
    }
    //let listFactory = getListFactory();
    var listFactory = $('#selFactory').val();
    var listFactoryName = $('#selFactory').text();

    var listFactoryHaveData = getFactoryHaveData(listFactory);

    var countFactory = listFactoryHaveData.length;

    var chartCorporation = document.getElementById('chartCorporation');

    listFactoryHaveData.forEach(function (item) {
        var div = document.createElement("DIV");
        if (countFactory == 1) {
            div.className = "col-sm-12";
        } else if (countFactory <= 4) {
            div.className = "col-sm-6";
        } else if (countFactory > 4 && countFactory <= 6) {
            div.className = "col-sm-4";
        } else if (countFactory > 6 && countFactory <= 8) {
            div.className = "col-sm-3";
        } else {
            div.className = "col-sm-2";
        }

        var divLineChart = document.createElement('DIV');
        divLineChart.className = "divLineChart";

        var x = document.createElement(`CANVAS`);
        x.id = item.factoryId;

        divLineChart.appendChild(x);
        div.appendChild(divLineChart);
        chartCorporation.appendChild(div);

        drawLineChart(item.factoryId, item.factoryName);

    });

    setLineChartHeight(countFactory);
}

function drawLineChart(factoryId, factoryName) {

    let plnStartDate = $("#txtDate").val().replace(/\//g, "");

    getListActiveMesPkg(factoryId, plnStartDate, function (listPkg) {
        let chartLables = [];
        let completedQty = [];
        let remainQty = [];
        let mxPackages = [];
        let completedIot = [];

        //Iterate list active mes package to create chart data
        $.each(listPkg, function (idx, pkg) {
            let iotCompleted = pkg.MX_IOT_Completed - pkg.MxTarget;

            //Calculate remain quantity
            let calRemainQty = pkg.MxTarget - pkg.MX_IOT_Completed;
            calRemainQty = calRemainQty <= 0 ? 0 : calRemainQty;

            chartLables.push(pkg.LineName);
            completedQty.push(iotCompleted >= 0 ? pkg.MxTarget : pkg.MX_IOT_Completed);
            remainQty.push(calRemainQty);
            mxPackages.push({ MxPackage: pkg.MxPackage, MxTarget: pkg.MxTarget });

            //Calculate iot completed
            let calCompletedIot = iotCompleted >= 0 ? iotCompleted : 0;
            completedIot.push(calCompletedIot);
        });

        createLineChart(chartLables, completedQty, remainQty, mxPackages, completedIot, factoryId, factoryName);

    });
}

//function getListFactory() {
//    var listFactory = [];
//    var config = ObjectConfigAjaxPost(
//        "../CorporationDashboard/GetListFactory"
//        , false
//    );
//    AjaxPostCommon(config, function (resData) {
//        if (resData.length > 0) {
//            listFactory = getFactoryHaveData(resData);
//        } else {
//            listFactory = null;
//        }
//    });

//    return listFactory;
//}

function handleClickButtonDisplay() {
    $("#btnDisplayCD").click(function () {
        $.blockUI();
        $.blockUI({ css: { backgroundColor: '#fff', color: '#5192ca' } });
        setTimeout(function () {
            document.getElementById('chartCorporation').innerHTML = "";
            loadLineChart();
            $.unblockUI();
        }, 100);
    });
}
