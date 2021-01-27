
function eventDateControlClick() {
    document.getElementById('btnNextDate').onclick = function () {
        var currentDate = getChoosedDate();
        var nextDate = AddDays(currentDate, 1);
        setChoosedDate(nextDate);

        ReloadPackageDataSource();
    }

    document.getElementById('btnPreDate').onclick = function () {
        var currentDate = getChoosedDate();
        var preDate = AddDays(currentDate, -1);
        setChoosedDate(preDate);

        ReloadPackageDataSource();
    }
}

function eventPackageSelected() {
    $("#drpPackage").change(function () {
        resetPackage();//clear old selected

        var mespackage = $(this).val();

        choosedPackage = mespackage;
        //START MOD - SON) 28/Aug/2020
        //if (choosedPackage)
        //    GetPackageInfoAndTryConnectToMqtt();

        if (choosedPackage) {
            GetPackageInfoAndTryConnectToMqtt();
            GetStyleInformation(mespackage);
        }
        //END MOD - SON) 28/Aug/2020
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

        //START ADD - SON) 5/Sep/2020
        //Get line by factory
        GetLineByFactoryId('drpLine', $(this).val());
        //END ADD - SON) 5/Sep/2020
    });
}

function eventDateSelected() {
    $("#chooseDate").change(function () {
        ReloadPackageDataSource();
    });
}

//START ADD - SON) 5/Sep/2020
function eventLineSelection() {
    $("#drpLine").change(function () {
        //Get list of MES package by line and date
        let choosedDate = getChoosedDate();
        let fac = $("#drpFactory").val();
        let line = $("#drpLine").val();

        GetPackagesByLine(fac, line, choosedDate);
    });
}
//END ADD - SON) 5/Sep/2020


//2020-12-17 Tai Le(Thomas)
function eventReportBySelected() {
    $('#ddlReportBy a').click(function () { 
        let _this = $(this); 
        ReportBy = $(this).attr('id');
        if (ReportBy == "FA") ReportByDesc = `By Final Assembly <i class="fa fa-caret-down"></i>`;
        if (ReportBy == "QA") ReportByDesc = `By End line QC <i class="fa fa-caret-down"></i>`;
        _this.closest('.btn-group').find('button')[0].innerHTML = ReportByDesc;

        RefreshData();
    });
}

//