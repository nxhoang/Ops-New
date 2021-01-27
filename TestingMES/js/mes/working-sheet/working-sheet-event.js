function ShowModalSyncWeeklyWorkingSheet() {
    $("#mdlSyncWorkSheet").modal("show");
}
 
function eventButtonsSync() {
    $("#btnSyncFacWrkSheet").click(function () {
        let FactorySync = $("#txtFactorySync").val();
        let yymmSync = $("#txtYearMonthSync").val();
        let SourceSync = $("#drpSourceSync").val(); 
        //let yyyy = yymmSync.substr(0, 4);

        //let factory = $("#Factory").val();
        let weekNo = '';//$("#drpWeekNoSync").val();
        console.log(yymmSync + " - " + FactorySync);
        
        var config = ObjectConfigAjaxPost("../Factory/SynchronizeWorkingTime", false
            , JSON.stringify({ factory: FactorySync.toString(), yyyyMM: yymmSync }));

        //var config = ObjectConfigAjaxPost("../Factory/SynchronizeWorkingSheet", false
        //    , JSON.stringify({ factory: FactorySync, yyyy: yymmSync, weekNo: weekNo, sourceSync: SourceSync }));

        AjaxPostCommon(config, function (syncRes) {
            if (syncRes.IsSuccess) {
                console.log(syncRes.Data);
            } else {
                alert(syncRes.retMsg);
            }
        });
    });

    //ShowConfirmYesNo(
    //    "Synchronize Working Sheet"
    //    , "This process take a while to complete. Are you sure to synchronize working sheet?"
    //    , function () {

    //    }
    //    , function () { }
    //);
    //});
}
