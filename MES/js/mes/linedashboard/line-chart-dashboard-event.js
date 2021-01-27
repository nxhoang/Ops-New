
function eventButtonClick() {
    $("#btnDisplay").click(function () {
        loadLineChart();        
    });
}

function loadLineChart() {
    if (typeof lineBarChart !== "undefined") {
        //Clear bar chart
        lineBarChart.destroy();
    }

    let factoryId = $("#drpFactory").val();
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

        createLineChart(chartLables, completedQty, remainQty, mxPackages, completedIot);

    });
}

function eventDropdownlistSelection() {
    $("#drpFactory").change(function () {
        loadLineChart();
    });
}