function eventDateControlClick() {
    $("#btnPreDate").click(function () {
        var currentDate = getChoosedDate();
        var preDate = AddDays(currentDate, -1);

        setChoosedDate(preDate);

        ReloadPackageDataSource();
    });

    $("#btnNextDate").click(function () {
        var currentDate = getChoosedDate();
        var nextDate = AddDays(currentDate, 1);

        setChoosedDate(nextDate);

        ReloadPackageDataSource();
    });
}

function eventPackageSelected() {
    $("#drpPackage").change(function () {
        resetPackage();//clear old selected

        var mespackage = $(this).val();

        choosedPackage = mespackage;
        if (choosedPackage)
            GetPackageInfoAndTryConnectToMqtt();
    });
}

function eventDataSourceSelected() {
    $("#drpDataSource").change(function () {
        dataSource = $(this).val();
        
        if(choosedPackage)
            GetPackageInfoAndTryConnectToMqtt();
    });
}

function eventFactorySelected() {

    $("#drpFactory").change(function () {
        ReloadPackageDataSource();
    });
}


function eventDateSelected() {

    $("#chooseDate").change(function () {
        ReloadPackageDataSource();
    });
}

