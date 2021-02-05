// #region Variables
const TxtProcessNumber = "txtProcessNumber",
    TxtProcessNo = "txtProcessNo",
    JqTxtProcessNumber = $(`#${TxtProcessNumber}`),
    JqTxtProcessNo = $(`#${TxtProcessNo}`),
    ModalProcessDetail = "mdlProcessDetail",
    BtnSaveProcess = "btnSaveProcess",
    JqBtnSaveProcess = $(`#${BtnSaveProcess}`);

var UserRoleOpm, UserRoleFom, UserRoleMes, ImageHttpLink;
// #endregion Variables

// #region Ready
(() => {
    UserRoleOpm = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionPdm));
    UserRoleFom = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionAom));
    UserRoleMes = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionAom));

    // Ready events plus button to show sub-process
    //eventClickButtonProcessDetail();

    //eventSelectRadioButton();

    // Image and video events.
    //eventSelectFileChange();

    // Loading pickup/dispose and sub-process to combox boxes.
    //loadDataOnOpDetailModal();

    // Initialize data for modal
    window.InitDataForProcessModal();

    // Changed sub-process dropdown list events.
    //eventOnChangeSelection();

    // Main process radio events.
    cbMainProcessOnchanged();

    // Loading main image
    //flProcessImageChange();
    // Image, video, file events
    window.ImageVideoFileEvents();

    getReasonOperationPlan(res => {
        console.log("Loading op reason...");
        console.log(res);

        FillDataToDropDownlist("drpReasonOp", res, "SubCode", "CodeName");
    });
})();
// #endregion Ready

//#region Functions
function LayoutSaveEvent(callBack) {
    // Displaying process-modal
    ShowModal(ModalProcessDetail);

    // Clearing/resetting all controls.
    clearDataOnProcessDtModal();

    // Going to db to get max of OpSerial
    AsyncGetMaxOpSerial((opSerial) => {
        if (opSerial && opSerial.trim() !== "") {
            console.log("Assign OpSerial to textbox");
            console.log(`OpSerial: ${opSerial}`);

            JqTxtProcessNo.val(opSerial);
            JqTxtProcessNumber.val(Number(opSerial));
        } else {
            console.log("Could not get max of OpSerial");
        }
    });

    JqBtnSaveProcess.unbind().click(() => {
        if (window.StatusUpdateProcess === 1) {
            console.log("Updating process");
            UpdateProcess_New(null, callBack);
        } else {
            console.log("Creating new process");
            SaveNewProcess_New(null, callBack);
        }
    });
}

function LayoutUpdateEvent(opdt, callBack) {
    console.log("Layout updating opdt event.");
    // Displaying process-modal
    ShowModal('mdlProcessDetail');

    //unbindSaveProcess(window.StatusUpdateProcess, callBack);

    const opmt = GetSelectedOneRowData(gridOpsTableId);

    opdt.StyleCode = opmt.StyleCode;
    opdt.StyleSize = opmt.StyleSize;
    opdt.StyleColorSerial = opmt.StyleColorSerial;
    opdt.RevNo = opmt.RevNo;
    opdt.OpRevNo = opmt.OpRevNo;
    opdt.OpSerial = opdt.id;
    opdt.Edition = opmt.Edition;

    window.DisplayColor = opdt.DisplayColor;

    GetOpdtAsync(opdt, (dbOpdt) => {
        dbOpdt.LanguageId = opmt.Language;

        console.log(dbOpdt);

        InitDataUpdateProcess(dbOpdt);
    });

    JqBtnSaveProcess.unbind().click(() => {
        if (window.StatusUpdateProcess === 1) {
            console.log("Updating a process");
            UpdateProcess_New(null, callBack);
        } else {
            console.log("Creating new process");
            SaveNewProcess_New(null, callBack);
        }
    });
}

function cbMainProcessOnchanged() {
    const rds = document.getElementsByName("mainProcess");
    for (let i = 0; i < rds.length; i++) {
        rds[i].addEventListener('change', (ev) => {
            for (let opnt of _listOpnt) {
                if (opnt.OpnSerial.toString() === ev.currentTarget.value) {
                    opnt.MainProcess = "1";
                } else {
                    opnt.MainProcess = "0";
                }
            }
        });
    }
}

function GetOpnts(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, languageId, callBack) {
    /// <summary>This function to get list of opnts with image, video url based on GetListProcessNameDetail function</summary>
    /// <returns type="opnts">List of opnts</returns>

    const config = ObjectConfigAjaxPost("/Ops/GetOpnts", true,
        JSON.stringify({
            edition: edition,
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            opRevNo: opRevNo,
            opSerial: opSerial,
            languageId: languageId
        }));
    AjaxPostCommon(config, (res) => {
        callBack(res);
    });
}

function CreateLayoutProcess(opdt, callBack) {
    $.post("/OpLayout/GetOpdt", { opdt }).done((res) => {
        console.log(res);
        if (res.IsSuccess) {
            HideModal(ProcessModal);

            // Getting icon
            let iconNameArr = [];
            if (res.Result.p.MainProcessArr) {
                // ex: MainProcessArr: "1-0-0" and IconNames: "3485.svg-3486.svg-347.svg", pick up 3485.svg (main process is 1)
                const tempMainProcessArr = res.Result.p.MainProcessArr.split("-");
                if (tempMainProcessArr && tempMainProcessArr.length > 0 && res.Result.p.IconNames) {
                    const tempIconNameArr = res.Result.p.IconNames.split("-");

                    for (let i = 0; i < tempIconNameArr.length; i++) {
                        if (tempMainProcessArr[i] === "1" && tempIconNameArr[i].trim() !== "_") iconNameArr.push(tempIconNameArr[i]);
                    }
                }
            }

            if (iconNameArr.length === 0) iconNameArr.push("settings.svg");

            const bgColorLayout = JSON.parse(localStorage.getItem('bgColorLayout'));
            let bgImage = bgColorLayout ? bgColorLayout.BgImage : "linear-gradient(to right, #fc6586, #53bbfd)",
                btFontColor = bgColorLayout ? bgColorLayout.FontColor : '#ffffff';

            const machineName = res.Result.p.MachineName ? res.Result.p.MachineName : " ",
                iconName = res.Result.p.IconName ? res.Result.p.IconName : "settings.svg",
                hotSpot = res.Result.p.HotSpot && res.Result.p.HotSpot === "1" ? true : false,
                opsState = res.Result.p.OpsState && res.Result.p.OpsState === "1" ? true : false,
                remarks = res.Result.p.Remarks ? res.Result.p.Remarks : "",
                displayColor = res.Result.p.DisplayColor ? `#${res.Result.p.DisplayColor.substr(3, 8)}` : "#A9A9A9",
                showButtonPlayVideo = $.isEmptyObject(res.Result.p.VideoFile) ? 0 : 1,
                opNum = res.Result.p.OpNum ? res.Result.p.OpNum : " ",
                opName = getOpNameByLang(opdt, window.currentLang),
                layoutProcess = new LayoutProcess(res.Result.p.OpSerial.toString(), `[${opNum}] ${opName}`,
                    res.Result.p.OpTime, res.Result.p.MachineCount, machineName, res.Result.p.ManCount, opName,
                    res.Result.p.VnOpName, res.Result.p.GbOpName, res.Result.p.MmOpName, res.Result.p.IdOpName,
                    res.Result.p.EtOpName, res.Result.p.Codes, res.Result.p.IconNames, res.Result.p.MainProcessArr,
                    hotSpot, opsState, opNum, remarks, res.Result.p.OpGroup, res.Result.p.MachineType, res.Result.p.ModuleId,
                    displayColor, window.ProcessWidth, window.ProcessHeight, window.LayoutFontSize, [], true,
                    window.LayoutPage, window.CanDelete, showButtonPlayVideo, LayoutLeftX, LayoutTopY, "emptyGroup", "",
                    iconNameArr, res.Result.iconUrl, iconName, bgImage, btFontColor);
            callBack(layoutProcess);
        }
    });
}

function ToLayoutProcess() {
    // Getting icon
    let iconNameArr = [];
    //if (opdt.p.MainProcessArr) {
    //    // ex: MainProcessArr: "1-0-0" and IconNames: "3485.svg-3486.svg-347.svg", pick up 3485.svg (main process is 1)
    //    const tempMainProcessArr = opdt.p.MainProcessArr.split("-");
    //    if (tempMainProcessArr && tempMainProcessArr.length > 0 && opdt.p.IconNames) {
    //        const tempIconNameArr = opdt.p.IconNames.split("-");

    //        for (let i = 0; i < tempIconNameArr.length; i++) {
    //            if (tempMainProcessArr[i] === "1" && tempIconNameArr[i].trim() !== "_") iconNameArr.push(tempIconNameArr[i]);
    //        }
    //    }
    //}

    //if (iconNameArr.length === 0) iconNameArr.push("settings.svg");
    const iconName = opdt.p.IconName ? opdt.p.IconName : "settings.svg",
        machineName = opdt.p.MachineName ? opdt.p.MachineName : " ",
        hotSpot = opdt.p.HotSpot && opdt.p.HotSpot === "1" ? true : false,
        opsState = opdt.p.OpsState && opdt.p.OpsState === "1" ? true : false,
        remarks = opdt.p.Remarks ? opdt.p.Remarks : "",
        displayColor = opdt.p.DisplayColor ? `#${opdt.p.DisplayColor.substr(3, 8)}` : "#A9A9A9",
        showButtonPlayVideo = $.isEmptyObject(opdt.p.VideoFile) ? 0 : 1,
        opNum = opdt.p.OpNum ? opdt.p.OpNum : " ",
        opName = getOpNameByLang(objOpDetail, window.currentLang),
        layoutProcess = new LayoutProcess(opdt.p.OpSerial.toString(), `[${opNum}] ${opName}`, opdt.p.OpTime,
            opdt.p.MachineCount, machineName, opdt.p.ManCount, opName, opdt.p.VnOpName, opdt.p.GbOpName, opdt.p.MmOpName,
            opdt.p.IdOpName, opdt.p.EtOpName, opdt.p.Codes, opdt.p.IconNames, opdt.p.MainProcessArr, hotSpot, opsState,
            opNum, remarks, opdt.p.OpGroup, opdt.p.MachineType, opdt.p.ModuleId, displayColor, window.ProcessWidth,
            window.ProcessHeight, window.LayoutFontSize, [], true, window.LayoutPage, window.CanDelete,
            showButtonPlayVideo, LayoutLeftX, LayoutTopY, "emptyGroup", "", iconNameArr, opdt.iconUrl, iconName);

    return layoutProcess;
}
//#endregion