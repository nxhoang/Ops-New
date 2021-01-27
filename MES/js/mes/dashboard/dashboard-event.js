function eventSelectDropdownlist() {
    $("#drpShowBy").on("change", function () {
        if (this.value === "1") {
            //Montly
            $("#divYear").show();
            $("#divMonth").show();
            $("#divWeekNo").hide();

        } else if (this.value === "2") {
            //Weekly
            $("#divYear").show();
            $("#divMonth").hide();
            $("#divWeekNo").show();

        } else {
            //Today
            $("#divYear").hide();
            $("#divMonth").hide();
            $("#divWeekNo").hide();
        }
    });
}

function showMachineCount() {
    let yyyy = $("#drpYear").val();
    let showType = $("#drpShowType").val();
    let showBy = $("#drpShowBy").val();

    if ($("#drpShowType").val() === "1") {
        //No of machine
        if (showBy === "1") {
            //Monthly            
            let mm = $("#drpMonth").val();

            GetMaxMachineCountByMonth(yyyy, mm, CUR_DATE, CUR_WEEK_NO, showType, function (resData) {
                createChartData(resData);
            });
        } else if (showBy === "2") {
            //weekly
            let weekNo = $("#drpWeekNo").val();

            GetMaxMachineCountByWeek(yyyy, CUR_MONTH, CUR_DATE, weekNo, showType, function (resData) {
                createChartData(resData);
            });
        } else {
            //today
            GetMaxMachineCountByDay(yyyy, CUR_MONTH, CUR_DATE, CUR_WEEK_NO, showType, function (resData) {
                createChartData(resData);
            });
        }
    } else {
        //No of Machine
        if (showBy === "1") {
            //Monthly
            let mm = $("#drpMonth").val();

            GetMaxMachineCountByMonth(yyyy, mm, CUR_DATE, CUR_WEEK_NO, showType, function (resData) {
                createChartData(resData);
            });
        } else if (showBy === "2") {
            //weekly
            let weekNo = $("#drpWeekNo").val();

            GetMaxMachineCountByWeek(yyyy, CUR_MONTH, CUR_DATE, weekNo, showType, function (resData) {
                createChartData(resData);
            });
        } else {
            //today
            GetMaxMachineCountByDay(yyyy, CUR_MONTH, CUR_DATE, CUR_WEEK_NO, showType, function (resData) {
                createChartData(resData);
            });
        }
    }
}
