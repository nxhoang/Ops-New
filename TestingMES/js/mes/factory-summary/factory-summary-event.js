
function EventSelectCorporation() {
    $("#drpCorporation").change(function () {

        var corpId = $("#drpCorporation option:checked").val();
        GetChartDataLabels(corpId, function (lstFact) {
            var chartData = CreateChartData(lstFact);
            var configChart2 = CreateChartConfig(chartData);

            if (typeof chartFac !== "undefined") {
                chartFac.destroy();
            }

            chartFac = new Chart(ctx, configChart2);

        });


    });
}
