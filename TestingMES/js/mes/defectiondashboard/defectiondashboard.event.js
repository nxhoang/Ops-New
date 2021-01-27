
function eventDateControlClick() {
    
}

function eventSearchBtnControlClick() {
    document.getElementById('btnSearch').onclick = function () {
        ReloadPackageDataSource();
    }
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

        if (choosedPackage)
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