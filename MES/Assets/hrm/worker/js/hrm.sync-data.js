const UrlBulkInsertAttEmps = "/MesLineAllocation/BulkInsertDeptTeam",
    UrlGetDeptTeam = "/Employee/GetDeptTeams",
    UrlBulkInsertEmpByDepts = "/Employee/BulkInsertEmployeeByDept",
    UrlSyncEmpImg = "/Employee/SyncEmpImg",
    UrlSyncEmpImgByCond = "/Employee/SyncEmpImgByCondition",
    SyncDeptTeamMes = "<h3>Synchronizing department team...</h3>",
    SyncEmpByDeptMes = "<h3>Synchronizing employees by department team...</h3>",
    SyncEmpImgMes = "<h3>Synchronizing employee images...</h3>",
    AjaxMsgSyncEmpNfcId = "<h3>Synchronizing employee NFC...</h3>",
    UrlSyncEmpNfcId = "/Employee/SynchronizeEmpNfcId",
    UrlGetEmpNoImage = "/Employee/GetEmpNoImage",
    BtnSyncUserImg = "btnSyncUserImg",
    BtnSyncUserImgByCondition = "btnSyncUserImgByCondition",
    BtnSyncEmpNfcId = "btnSyncEmpNfcId";

function GetEmpNoImage() {
    ///<summary>
    /// If there are some employees do not have image, enable "synchronize image" button.
    ///</summary >

    const config = new AjaxShortHandConfig(AjaxWaitMes, UrlGetEmpNoImage);
    AjaxPostShortHand(config, (res) => {
        console.log(res);

        if (!res.IsSuccess) {
            console.log(res.Log);
        }
        if (res.IsSuccess) {
            document.getElementById(BtnSyncUserImg).style.display = parseInt(res.Data) === 0 ? "none" : "block";
        }
    });
}

function SyncEmpImgByCond(data) {
    ///<summary>
    ///1. Loading list of employee image urls.
    ///2. Saving the images to local.
    ///</summary >
    
    console.log(data);

    const config = new AjaxConfig(UrlSyncEmpImgByCond, true, JSON.stringify(data));
    AjaxPostCommon(config, (res) => {
        console.log(res);

        if (!res.IsSuccess) {
            console.log(res.Log);
        }
        if (res.IsSuccess) {
            // Reload grid after synchronize
            getEmployees(currentCorp, window.SelectedSections);

            MsgInform("Inform", "Synchronized images successfully.", "inform", true, true);
            document.getElementById(BtnSyncUserImg).style.display = "none";
        }
    }).always(() => { $.unblockUI();});
}

function SyncEmpImg() {
    ///<summary>
    ///1. Loading list of employee image urls.
    ///2. Saving the images to local.
    ///</summary >

    const config = new AjaxShortHandConfig(SyncEmpImgMes, UrlSyncEmpImg);
    AjaxPostShortHand(config, (res) => {
        console.log(res);

        if (!res.IsSuccess) {
            console.log(res.Log);
        }
        if (res.IsSuccess) {
            // console.log(res);
            MsgInform("Inform", "Synchronized images successfully.", "inform", true, true);
            document.getElementById(BtnSyncUserImg).style.display = "none";
        }
    });
}

function SyncEmpNfcId() {
    ///<summary>
    ///1. Loading list of employees from API.
    ///2. Updating NFC column in db.
    ///</summary >

    console.log("What is Near field communication.");

    const config = new AjaxShortHandConfig(AjaxMsgSyncEmpNfcId, UrlSyncEmpNfcId);
    AjaxPostShortHand(config, (res) => {
        console.log(res);

        if (!res.IsSuccess) {
            console.log(res.Log);
        }
        if (res.IsSuccess) {
            // console.log(res);
            MsgInform("Inform", "Synchronized employee NFC successfully.", "inform", true, true);
            document.getElementById(BtnSyncEmpNfcId).style.display = "none";
        }
    });

    //const config = new AjaxConfig(UrlSyncEmpNfcId, true);
    //AjaxPostCommon(config, (res) => {
    //    console.log(res);
    //});
}

function BulkInsertDeptTeam() {
    ///<summary>
    ///1. Loading list of department teams. If there are no data
    ///2. Bulk inserting list of department teams from K - API to database
    ///then loading list of department teams</summary >

    $.blockUI({ message: SyncDeptTeamMes });

    $.getJSON(UrlBulkInsertAttEmps).done((res) => {
        console.log(res);
        if (!res.IsSuccess) {
            console.log(res.Log);
        }
        if (res.IsSuccess && res.Result) {
            // console.log(res);
            //MsgInform("Inform", "Synchronized successfully.", "inform", true, true);
            BulkInsertEmpByDepts();
        }
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always(() => {
            //$.unblockUI();
        });
}

function BulkInsertEmpByDepts() {
    ///<summary>
    ///1. Loading list of departments then
    ///2. Bulk inserting list of departments from K - API to database
    ///</summary >

    $.blockUI({ message: SyncEmpByDeptMes });

    $.getJSON(UrlBulkInsertEmpByDepts).done((res) => {
        console.log(res);

        if (!res.IsSuccess) {
            console.log(res.Log);
        }
        if (res.IsSuccess) {
            // console.log(res);
            MsgInform("Inform", "Synchronized successfully.", "inform", true, true);
        }
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always(() => {
            $.unblockUI();
        });
}

function GetDeptTeam() {
    const config = new AjaxShortHandConfig(AjaxLoadMdMes, UrlGetDeptTeam);
    AjaxPostShortHand(config, (res) => {
        console.log(res);

        if (!res.IsSuccess) {
            console.log(res.Log);
        }
        if (res.IsSuccess && res.Result) {
            //document.getElementById(BtnSyncData).style.display = res.Result.length > 0 ? "none" : "block";
        }
    });
}