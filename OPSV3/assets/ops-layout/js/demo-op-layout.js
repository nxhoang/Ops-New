//#region Variables
"use strict";

const TxtVideoLink = "txtVideoLink", JqTxtVideoLink = $(`#${TxtVideoLink}`), VdOpdt = "vdoOpsDetail", JqVdOpdt = $(`#${VdOpdt}`),
    DivOpVideoList = "divOpVideoList", DefaultDisplayColor = "#A9A9A9", DefaultColor = "#FFFFFFFF", DefaultIcon = "settings.svg";

var opsGroups = [], opsNodes = [], opsEdges = [], opsData = {
    "groups": opsGroups,
    "nodes": opsNodes,
    "edges": opsEdges
}, jsPlumbGroup, defaultGroupMode = "OpGroup", OpsLanguage,
    currentLang, opNameList = [], defaultProcessHeight = 190, defaultProcessWidth = 120, defaultCanvasHeight = 650,
    defaultFontSize = 13, dfNodeTop = 24, dfNodeLeft = 2, dfGroupTop = 60, dfGroupLeft = 20, LayoutEvent = false,
    systemLang = $("#flagSelected").attr("value"), msgInformData, msgLostData, msgPermission, msgNoGroup, msgSelectOpFirstly,
    msgExistGroup, msgSelectColor, msgError, msgSave, msgConfirmDelete, msgDeleted, msgInvalid, msgLimitedLength,
    msgAcceptLineFlow, msgConfirmed, msgSelectFactory, msgOpConfirmed, currentMovedGroup, isMovingGroup = false,
    hasRefreshCanvas = false, emptyGroups = [], CurrentOpdts = [], CurrentOpmt, IsDeleteProcess = false;

var app = angular.module("opsApp", ["$jsPlumb", "ngAnimate", "ngSanitize", "ui.bootstrap", "ui.router", "ui", "long2know",
    "colorpicker.module"]);
//#endregion

//#region Javascript functions
function createOpVideoTag(vdLink, isMain) {
    const vdTag = document.createElement("video");
    vdTag.src = vdLink;
    vdTag.classList.add("op-vd-tag");
    if (isMain) vdTag.classList.add("op-vd-tag--main-highlight");
    vdTag.addEventListener("click", (ev) => {
        if (ev.target.currentSrc) {
            document.getElementById(TxtVideoLink).value = ev.target.currentSrc;
            document.getElementById(VdOpdt).src = ev.target.currentSrc;
        }
    });

    return vdTag;
}

function PlayOpVideo(res) {
    // Clearing controls
    document.getElementById(VdOpdt).src = "";
    document.getElementById(TxtVideoLink).value = "";
    document.getElementById(DivOpVideoList).innerHTML = "";

    //Adding main video
    if (res.mainOpdt.VideoOpLink) document.getElementById(DivOpVideoList).appendChild(createOpVideoTag(res.mainOpdt.VideoOpLink, true));

    // Creating video tag then add to the list.
    if (res.opdts) {
        for (let p of res.opdts) {
            if (p.VideoFile) {
                const vdUrl = `${res.httpVideo}${p.VideoFile}`;
                document.getElementById(DivOpVideoList).appendChild(createOpVideoTag(vdUrl, false));
            }
        }
    }

    const l = res.mainOpdt && res.mainOpdt.VideoOpLink && res.mainOpdt.VideoOpLink.includes("pkfile") ? `${window.location.host}\\${res.mainOpdt.VideoOpLink}` : res.mainOpdt.VideoOpLink,
        proInf = `Style Code: ${res.mainOpdt.StyleCode} | Size: ${res.mainOpdt.StyleSize} | Color: ${res.mainOpdt.StyleColorSerial} | RevNo: ${res.mainOpdt.RevNo} | Op Revision: ${res.mainOpdt.OpRevNo} | Op Serial: ${res.mainOpdt.OpSerial}`,
        proName = `Group name: ${res.mainOpdt.OpGroupName} | Process name: ${res.mainOpdt.OpName}`;

    JqTxtVideoLink.val(l);
    if (res.mainOpdt && res.mainOpdt.VideoOpLink) JqVdOpdt.attr("src", res.mainOpdt.VideoOpLink);

    $("#lblProcessName").html(proName);
    $("#lblProcessInfo").html(proInf);

    ShowModal("mdlPlayVideo");
}

function clickOpVideo(ev) {
    console.log(ev);
    //alert(ev.target.currentSrc);
    if (ev.target.currentSrc) {
        document.getElementById(TxtVideoLink).value = ev.target.currentSrc;
        document.getElementById(VdOpdt).src = ev.target.currentSrc;
    }
}

function getOpNameByLang(opdt, lang) {
    let opName;
    switch (lang) {
        case "vn":
            opName = opdt.VnOpName;
            break;
        case "gb":
            opName = opdt.GbOpName;
            break;
        case "mm":
            opName = opdt.MmOpName;
            break;
        case "id":
            opName = opdt.IdOpName;
            break;
        case "et":
            opName = opdt.EtOpName;
            break;
        default:
            opName = opdt.OpName ? opdt.OpName : "";
            break;
    }
    return opName;
}

function GetOpnmByLang(opnm, lang) {
    let opName;
    switch (lang) {
        case "vn":
            opName = opnm.Vietnam;
            break;
        case "gb":
            opName = opnm.English;
            break;
        case "mm":
            opName = opnm.Myanmar;
            break;
        case "id":
            opName = opnm.Indonesia;
            break;
        case "et":
            opName = opnm.Ethiopia;
            break;
        case "kr":
            opName = opnm.Korea;
            break;
        default:
            opName = opnm.English ? opnm.English : "";
            break;
    }
    return opName;
}

function shadeColor(color, percent) {
    var num = parseInt(color, 16),
        amt = Math.round(2.55 * percent),
        R = (num >> 16) + amt,
        G = (num >> 8 & 0x00FF) + amt,
        B = (num & 0x0000FF) + amt;
    return (0x1000000 + (R < 255 ? R < 1 ? 0 : R : 255) * 0x10000 + (G < 255 ? G < 1 ? 0 : G : 255) *
        0x100 + (B < 255 ? B < 1 ? 0 : B : 255)).toString(16).slice(1);
}

function getMsgByLang(msgObj) {
    const lang = $("#flagSelected").attr("value");
    const msg = GetMsgByLang(msgObj, lang);

    return msg;
}

function removeSummary(selector) {
    switch (selector[0].id) {
        case "sumTool":
            $(".ops-node-tool").hide();
            break;
    }

    selector.remove();
}

function removeAllSummary() {
    const els = ["sumTitle", "sumEditable", "sumTool", "sumWorker", "showPattern", "sumAllMachines", "sumMainMachines",
        "sumBom", "jigSum"];
    for (let el of els) {
        if (document.contains(document.getElementById(el))) {
            document.getElementById(el).remove();
        }
    }
}

function BeforeSelectRowGridOpMaster() {
    const msg = getMsgByLang(msgLostData);
    const r = confirm(msg.value);

    return r;
}

function OpsMasterFunction(row) {
    // Hide save popup if user select operation master
    $("#opslayoutModal").hide();

    // Clearing all video processing information.
    if (IsSelectRow) {
        ClearAllVideoInfo1();
        window.IsSelectRow = false;
    }

    if (typeof toolkit !== "undefined") {
        if (LayoutEvent) {
            LayoutEvent = false;
        } else {
            loadLayout(row);
            //document.getElementById("divRightVideoList").style.display = "none";
        }
    }
}

function FunctionCallBackSavingProcess(addStatus, objOpdt) {
    console.log(addStatus);
    console.log(objOpdt);
    if (addStatus) {
        //callback(objOpdt);
    }
}

function CreateGridOpsDetail() { }

function getOpNames(lang) {
    let url = "/OpLayout/GetOpName";

    // Get list of Operation name
    let getOpName = $.post(url, { languageId: lang });
    getOpName.then(function (response) {
        if (response.opNames !== null && response.opNames !== undefined) {
            window.opNameList = response.opNames;
        } else {
            console.log(response.error);
        }
    }, function (error) {
        ShowAjaxError(error, "/OpLayout/GetOpName");
    });
}

function GetStyleMaster() { }

function getTopLeft(param) {
    ///<summary>
    ///Get top, left of group
    /// <param name="param">format groupTop.nodeTop or groupLeft.nodeLeft</param>
    ///</summary >

    if (param === null || param === undefined) {
        return 0;
    }

    const xGr = param.split(".");
    let x;
    if (xGr.length < 1) {
        x = xGr;
    } else {
        x = xGr[0];
    }
    return x;
}

function loadLayout(opsMaster, lang, groupMode, page) {
    //window.location.reload(true);
    hasRefreshCanvas = false;

    $.blockUI(window.BlockUI);

    // Before reload layout, need to remove all of summaries
    removeAllSummary();

    if (!page) page = 1;

    if (opsMaster === null || opsMaster === undefined) {
        opsMaster = GetSelectedOneRowData(gridOpsTableId);
    }

    opsGroups.length = 0;
    opsNodes.length = 0;
    opsEdges.length = 0;

    if (opsMaster !== null && opsMaster !== undefined) {
        var isOpmtEmpty = !Object.keys(opsMaster).length;

        if (!isOpmtEmpty) {
            // Check user permission to enable delete action of process
            checkUserRole(opsMaster.Edition, "Update", (result) => {
                window.CanSave = result;
                if (result === true) {
                    checkUserRole(opsMaster.Edition, "Delete", (rs) => {
                        window.CanDelete = rs;
                    });
                } else {
                    window.CanDelete = false;
                }
            });

            // If user change mode of group, get the value for loading layout.
            // If user select row in operation plan grid view, get GroupMode of opmt. 
            // If GroupMode is empty, mode is group. 
            const opGrpMode = opsMaster.GroupMode === "" ? defaultGroupMode : opsMaster.GroupMode;
            groupMode = groupMode === undefined ? opGrpMode : groupMode;

            // If user change language of opmt, get the value for loading layout.
            // If user select row in operation plan grid view, get language of opmt. 
            // If Language is empty, lang is df (default). 
            var languageId = lang === undefined || lang === null ? MapLanguageToFlag(opsMaster.Language) : lang;

            const config = {
                url: "/OpLayout/GetOpdts",
                async: true,
                postData: JSON.stringify({ opsMaster: opsMaster, groupMode: groupMode, languageId: languageId, page: page })
            };

            AjaxPostCommon(config, (response) => {
                toolkit.clear();
                jsPlumbSurface.refresh();

                if (response.error) {
                    console.log(response.error);
                } else {
                    console.log("💋 data is loaded for layout");
                    console.log(response);

                    const opmt = response.opmt, opdts = response.opdts;
                    CurrentOpdts = response.opdts.nodes;
                    window.CurrentOpmt = response.opmt;
                    window.ProcessIconPath = response.iconUrl;

                    if (opmt !== null && opmt !== undefined && opdts !== null && opdts !== undefined) {
                        const fontSize = opmt.LayoutFontSize === 0 || opmt.LayoutFontSize === undefined ?
                            defaultFontSize : opmt.LayoutFontSize;
                        const canvasHeight = opmt.CanvasHeight === 0 || opmt.CanvasHeight === undefined ?
                            defaultCanvasHeight : opmt.CanvasHeight;
                        const pWidth = opmt.ProcessWidth === null || opmt.ProcessWidth === undefined ?
                            defaultProcessWidth : opmt.ProcessWidth;
                        const pHeight = opmt.ProcessHeight === null || opmt.ProcessHeight === null ?
                            defaultProcessHeight : opmt.ProcessHeight;

                        if (opdts.nodes.length > 0) {
                            jsPlumbGroup = opdts.groups;

                            let gleft = dfGroupLeft;
                            let nLeft = dfNodeLeft;
                            let nTop = dfNodeTop;

                            $(jsPlumbGroup).each(function (index, value) {
                                var group = {};
                                var left = value.X;
                                var top = value.Y;
                                let parCom = value.PartComment === null || typeof value.PartComment === "undefined" ? "" : " (" + value.PartComment + ")";//SON ADD) 30/May/2019

                                group.id = `g${value.SubCode}`;
                                group.title = value.CodeName + parCom; //SON ADD) 30/May/2019
                                group.DisplayColor = "#00000000";
                                group.Processes = 0;
                                group.Machines = 0;
                                group.Workers = 0;
                                group.TotalOpTime = 0;

                                if (top === null && left === null) {
                                    if (index === 0) {
                                        group.left = gleft;
                                    } else {
                                        group.left = gleft += 200;
                                    }

                                    group.top = dfGroupTop;
                                } else {
                                    group.left = left;
                                    group.top = top;
                                }

                                group.isArrange = false;

                                opsGroups.push(group);
                            });

                            var tempNodes = opdts.nodes, isNullGroup,
                                topNullGroup, leftNullGroup;

                            if (tempNodes !== null && tempNodes !== undefined && tempNodes.length > 0) {
                                var lastGroup;

                                switch (groupMode) {
                                    case "MachineType":
                                        lastGroup = tempNodes[0].MachineType;
                                        break;
                                    case "OpGroup":
                                        lastGroup = tempNodes[0].OpGroup;
                                        break;
                                    default:
                                        lastGroup = tempNodes[0].ModuleId;
                                }

                                $(tempNodes).each(function (index, value) {
                                    let isDisplay = true, machineName = value.MachineName === null || value.MachineName === undefined ? " " : value.MachineName,
                                        hotSpot = value.HotSpot && value.HotSpot === "1" ? true : false,
                                        opsState = value.OpsState && value.OpsState === "1" ? true : false,
                                        remarks = value.Remarks ? value.Remarks : "",
                                        opName, group, title, top, left;
                                    opName = getOpNameByLang(value, lang);

                                    const displayColor = value.DisplayColor === null || value.DisplayColor === "" || value.DisplayColor === DefaultColor ? DefaultDisplayColor : `#${value.DisplayColor.substr(3, 8)}`,
                                        page = value.Page === 0 ? 1 : value.Page,
                                        showButtonPlayVideo = $.isEmptyObject(value.VideoFile) ? 0 : 1,
                                        opNum = value.OpNum === null || value.OpNum === undefined ? " " : value.OpNum;

                                    if (value.X === null && value.Y === null) {
                                        // Position of node based on group that the node belong to.
                                        // We have sorted list of nodes by group. If last group of node changed, 
                                        // we need to reset top and left.
                                        switch (groupMode) {
                                            case "MachineType":
                                                if (lastGroup !== value.MachineType) {
                                                    nLeft = dfNodeLeft;
                                                    nTop = dfNodeTop;
                                                    lastGroup = value.MachineType;
                                                }
                                                break;
                                            case "OpGroup":
                                                if (lastGroup !== value.OpGroup) {
                                                    nLeft = dfNodeLeft;
                                                    nTop = dfNodeTop;
                                                    lastGroup = value.OpGroup;
                                                }
                                                break;
                                            default:
                                                if (lastGroup !== value.ModuleId) {
                                                    nLeft = dfNodeLeft;
                                                    nTop = dfNodeTop;
                                                    lastGroup = value.ModuleId;
                                                }
                                        }

                                        if (index === 0) {
                                            left = dfNodeLeft;
                                            top = dfNodeTop;
                                        } else {
                                            left = nLeft += 2;
                                            top = nTop += 2;
                                        }
                                    } else {
                                        left = value.UiLeft;
                                        top = value.UiTop;
                                    }

                                    switch (groupMode) {
                                        case "MachineType":
                                            if (tempNodes[index].MachineType === null) {
                                                isNullGroup = true;
                                                group = "emptyGroup";
                                                if (topNullGroup === undefined) topNullGroup = getTopLeft(tempNodes[index].Y);
                                                if (leftNullGroup === undefined) leftNullGroup = getTopLeft(tempNodes[index].X);
                                            } else {
                                                group = `g${tempNodes[index].MachineType}`;
                                            }
                                            title = value.MachineName;
                                            break;
                                        case "OpGroup":
                                            if (tempNodes[index].OpGroup === null) {
                                                isNullGroup = true;
                                                group = "emptyGroup";
                                                if (topNullGroup === undefined) topNullGroup = getTopLeft(tempNodes[index].Y);
                                                if (leftNullGroup === undefined) leftNullGroup = getTopLeft(tempNodes[index].X);
                                            } else {
                                                group = `g${tempNodes[index].OpGroup}`;
                                            }
                                            title = value.OpGroupName;
                                            break;
                                        default:
                                            if (tempNodes[index].ModuleId === null) {
                                                isNullGroup = true;
                                                group = "emptyGroup";
                                                if (topNullGroup === undefined) topNullGroup = getTopLeft(tempNodes[index].Y);
                                                if (leftNullGroup === undefined) leftNullGroup = getTopLeft(tempNodes[index].X);
                                            } else {
                                                group = `g${tempNodes[index].ModuleId}`;
                                            }
                                            title = value.ModuleName;
                                    }

                                    // Getting icon
                                    let iconNameArr = [];
                                    //if (value.MainProcessArr) {
                                    //    // ex: MainProcessArr: "1-0-0" and IconNames: "3485.svg-3486.svg-347.svg", pick up 3485.svg (main process is 1)
                                    //    const tempMainProcessArr = value.MainProcessArr.split("-");
                                    //    if (tempMainProcessArr && tempMainProcessArr.length > 0 && value.IconNames) {
                                    //        const tempIconNameArr = value.IconNames.split("-");

                                    //        for (let i = 0; i < tempIconNameArr.length; i++) {
                                    //            if (tempMainProcessArr[i] === "1" && tempIconNameArr[i].trim() !== "_")
                                    //                iconNameArr.push(tempIconNameArr[i]);
                                    //        }
                                    //        //node.data.IconNameArr = iconNameArr;
                                    //        //node.data.IconUrl = response.iconUrl;
                                    //    }
                                    //}

                                    //if (iconNameArr.length === 0) iconNameArr.push("settings.svg");
                                    const iconName = value.IconName ? value.IconName : DefaultIcon;

                                    const layoutProcess = new LayoutProcess(value.OpSerial.toString(), `[${opNum}] ${opName}`, value.OpTime, value.MachineCount,
                                        machineName, value.ManCount, opName, value.VnOpName, value.GbOpName, value.MmOpName, value.IdOpName, value.EtOpName,
                                        value.Codes, value.IconNames, value.MainProcessArr, hotSpot, opsState, opNum, remarks, value.OpGroup, value.MachineType,
                                        value.ModuleId, displayColor, pWidth, pHeight, fontSize, [], isDisplay, page, window.CanDelete, showButtonPlayVideo,
                                        left, top, group, title, iconNameArr, response.iconUrl, iconName);

                                    opsNodes.push(layoutProcess);
                                });
                            }

                            if (isNullGroup) {
                                var emptyGroup = {
                                    id: "emptyGroup",
                                    title: "",
                                    left: leftNullGroup,
                                    top: topNullGroup,
                                    isArrange: false,
                                    nodes: [],
                                    Processes: 0,
                                    Machines: 0,
                                    Workers: 0,
                                    TotalOpTime: 0
                                };
                                opsGroups.push(emptyGroup);
                            }

                            opsData.groups = opsGroups;
                            opsData.nodes = opsNodes;
                            opsData.edges = opdts.edges;

                            toolkit.load({
                                data: opsData
                            });
                        } else {
                            const msg = getMsgByLang(msgInformData);
                            ShowMessage(msg.title, msg.value, ObjMessageType.Info);
                        }

                        console.log("total page...");
                        console.log(response.opdts.totalPage);

                        var appElement = document.querySelector("[ng-app=opsApp]");
                        var $scope = angular.element(appElement).scope();
                        $scope.$apply(function () {
                            if (opdts.groupsToAdd !== null && opdts.groupsToAdd !== undefined) {
                                $scope.groupsToAdd = opdts.groupsToAdd;
                            }
                            $scope.Opdts = opsNodes;
                            $scope.opdtMode = groupMode;
                            $scope.opsFont.fontSize = parseInt(fontSize);
                            $scope.canvasStyle.height = parseInt(canvasHeight);
                            $scope.pLayoutModifier.pHeight = parseInt(pHeight);
                            $scope.pLayoutModifier.pWidth = parseInt(pWidth);
                            $scope.layoutPage.pageNo = page;
                            $scope.layoutPage.maxPage = response.opdts.totalPage;
                            $scope.layoutLang.selectedLang = languageId;
                            $scope.isCloneMode = false;
                            $scope.isShowGroups = true;
                            $scope.layoutViewType.selectedViewType = new OpViewType("OpName", "1", "letter-t.svg");

                            $scope.OpVideo1 = []; // Clearing list of process videos.

                            // Check user permission to enable delete action of process
                            $scope.savingRule.canSave = window.CanDelete;

                            if (response.iconUrl) $scope.IconUrl = response.iconUrl;
                        });

                        // Remove css of clone icon
                        $("#cloneProcess").removeClass("clone-mode");

                        // Displaying layout
                        document.getElementById("opsDiv").style.display = "block";
                        //alert("loaded data");
                    } else {
                        console.log(response.error);
                    }
                }
            });
        }
    }

    // Oanh add change color card 26Jan2021
    setTimeout(function () {
        setBackgroundColor_Layout_Load();
    }, 1000);

    $.unblockUI();
}

function SetMenuActionMode() { }

function GetOpmtFromLocalStorage() {
    const { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo } = JSON.parse(localStorage.getItem(OpsMasterInfo));
    const opmt = new Opmt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo);

    return opmt;
}

function checkUserRole(edition, action, callBack) {
    const getUrConfig = {
        url: "/OpLayout/GetUserRole",
        async: false
    };

    AjaxGetCommon(getUrConfig, (response) => {
        let result = false;
        if (response) {
            const userRole = response;
            let userRoleCheck = {};

            switch (edition) {
                // The selected plan edition is PDM or OPS 
                case "O":
                case "P":
                    //Return false if R&D role is null
                    if (userRole.rdRole === null) result = false;
                    userRoleCheck = userRole.rdRole;
                    break;
                case "A":
                    //Return false if R&D role is null
                    if (userRole.facRole === null) result = false;
                    userRoleCheck = userRole.facRole;
                    break;
                case "M":
                    //Return false if MES role is null
                    if (userRole.mesRole === null) result = false;
                    userRoleCheck = userRole.mesRole;
                    break;
                default:
                    result = false;
                    break;
            }

            //If user role is not null then check action permission
            if (!$.isEmptyObject(userRoleCheck)) {
                if (action === "Add") {
                    result = userRoleCheck.IsAdd === "1";
                } else if (action === "Update") {
                    result = userRoleCheck.IsUpdate === "1";
                } else if (action === "Delete") {
                    result = userRoleCheck.IsDelete === "1";
                } else if (action === "Confirm") {
                    result = userRoleCheck.IsConfirm === "1";
                } else if (action === "Export") {
                    result = userRoleCheck.IsExport === "1";
                }
            } else {
                result = false;
            }

        } else {
            console.log("Could not get User Role");
            result = false;
        }
        callBack(result);
    });
}

function removeEmptyGroups() {
    const grs = document.getElementsByTagName("group");
    for (let e of grs) {
        const pNodes = e.getElementsByTagName("node");
        if (pNodes.length === 0) {
            //e.style.display = "none";
            e.remove();
        }
    }
}

(function () {
    //#region Get messages
    GetMessageById("001", SystemIdOps, MenuIdAom, SmsFunction.Confirm, MessageType.Confirm, MessageContext.Confirm,
        (msg) => { msgLostData = msg; });
    GetMessageById("001", SystemIdOps, MenuIdAom, SmsFunction.Confirm, MessageType.Warning, MessageContext.NoData,
        (msg) => { msgInformData = msg; });
    GetMessageById("001", SystemIdOps, MenuIdSms, SmsFunction.Generic, MessageType.Error, MessageContext.Error,
        (msg) => { msgPermission = msg; });
    GetMessageById("001", SystemIdOps, MenuIdAom, SmsFunction.Add, MessageType.Error, MessageContext.Error,
        (msg) => { msgNoGroup = msg; });
    GetMessageById("001", SystemIdOps, MenuIdAom, SmsFunction.Generic, MessageType.Error, MessageContext.Error,
        (msg) => { msgSelectOpFirstly = msg; });
    GetMessageById("002", SystemIdOps, MenuIdOpm, SmsFunction.Add, MessageType.Error, MessageContext.Error,
        (msg) => { msgExistGroup = msg; });
    GetMessageById("002", SystemIdOps, MenuIdAom, SmsFunction.Update, MessageType.Error, MessageContext.Error,
        (msg) => { msgSelectColor = msg; });
    GetMessageById("002", SystemIdOps, MenuIdAom, SmsFunction.Generic, MessageType.Error, MessageContext.Error,
        (msg) => { msgError = msg; });
    GetMessageById("001", SystemIdOps, MenuIdOpm, SmsFunction.Add, MessageType.Success, MessageContext.Update,
        (msg) => { msgSave = msg; });
    GetMessageById("002", SystemIdOps, MenuIdOpm, SmsFunction.Delete, MessageType.Confirm, MessageContext.DeleteConfirm,
        (msg) => { msgConfirmDelete = msg; });
    GetMessageById("001", SystemIdOps, MenuIdOpm, SmsFunction.Delete, MessageType.Success, MessageContext.Delete,
        (msg) => { msgDeleted = msg; });
    GetMessageById("001", SystemIdOps, MenuIdAom, SmsFunction.Generic, MessageType.Error, MessageContext.InvalidData,
        (msg) => { msgInvalid = msg; });
    GetMessageById("002", SystemIdOps, MenuIdAom, SmsFunction.Add, MessageType.Error, MessageContext.Error,
        (msg) => { msgLimitedLength = msg; });
    GetMessageById("003", SystemIdOps, MenuIdAom, SmsFunction.Add, MessageType.Error, MessageContext.Error,
        (msg) => { msgAcceptLineFlow = msg; });
    GetMessageById("001", SystemIdOps, MenuIdOpm, SmsFunction.Confirm, MessageType.Success, MessageContext.Confirmed,
        (msg) => { msgConfirmed = msg; });
    GetMessageById("001", SystemIdOps, MenuIdAom, SmsFunction.Update, MessageType.Error, MessageContext.Error,
        (msg) => { msgSelectFactory = msg; });
    GetMessageById("002", SystemIdOps, MenuIdSms, SmsFunction.Generic, MessageType.Error, MessageContext.Error,
        (msg) => { msgOpConfirmed = msg; });

    //#endregion

    // #region Modal add/edit process
    // For demo
    //eventClickOnButtonOld();

    //Event on modal
    //ClickButtonRemoveImg();
    //ClickButtonRemoveVideo();
    //flProcessImageChange();

    //ClickButtonOpTime();
    //ClickLoadAllProcessName();
    //ClickButtonOk();

    //START ADD) SON (2019.08.29) - 29 August 2019
    //InitModalProcessPartialView();
    //END ADD) SON (2019.08.29) - 29 August 2019

    //ClickButtonGetProNameTemplate();

    //SelectModule();

    BindDataToJqGridInputOpTimeModal([]);
    //Init grid process template
    BindDataToJqGridProcessNameTemplate("", "");
    // #endregion

    var currentOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    var isOpmtEmpty = !Object.keys(currentOpsMaster).length;

    LoadOpsMasterGrid();

    if (currentOpsMaster === null || currentOpsMaster === undefined || isOpmtEmpty) {
        currentLang = "df";
    } else {
        currentLang = MapLanguageToFlag(currentOpsMaster.Language);
    }

    getOpNames(currentLang);

    // Block popup by right click of mouse
    const blockContextMenu = evt => { evt.preventDefault(); };
    window.addEventListener("contextmenu", blockContextMenu);

    $("#opsmodal").draggable();
    $("#opslayoutModal").draggable();

    window.onbeforeunload = function () {
        var message = "Important: Please click on 'Save' button to leave this page.";

        if (window.isChange && window.CanSave) return message;
    };

    //currentMovedGroup.click(() => {
    //    console.log("Hello world...");
    //});
})();

function currentMovedGroupClick() {
    console.log("So funny...");
}

function toggleGroups(isShow) {
    const groups = document.getElementsByTagName("group");

    if (isShow) {
        for (let g of groups) {
            g.style.border = "1px solid #1d5987";
            g.style.width = "170px";
            g.style.height = "110px";
        }
    } else {
        for (let g of groups) {
            g.style.border = "none";
            g.style.width = "0";
            g.style.height = "0";
        }
    }
}

function toggleDeleteFlowIcon(isShowDeleteFlow) {
    const delElements = document.getElementsByClassName("delete-connection");
    for (let e of delElements) {
        if (isShowDeleteFlow) {
            $(e).show();
        } else {
            $(e).hide();
        }
    }
}

// Intersecting rectangles
function intersectRect(r1, r2) {
    return !(r2.left > r1.right || r2.right < r1.left || r2.top > r1.bottom || r2.bottom < r1.top);
}

// Removing process group
function removeOpGroup() {
    const grs = document.getElementsByTagName("group");
    console.log(grs);
    for (let g of grs) {
        //console.log(g);
        g.remove();
    }
}
//#endregion

//#region Angular directives
app.directive("node", function (jsPlumbFactory) {
    return jsPlumbFactory.node({
        templateUrl: "node_template.tpl",
        inherit: ["removeProcess", "editTodo", "doneEditing", "updateNode", "changeNodePage", "playVideoProcess"] //SON MOD - Adding playVideoProcess
    });
});

app.directive("group", function (jsPlumbFactory) {
    return jsPlumbFactory.group({
        inherit: ["remove", "toggleGroup"],
        templateUrl: "group_template.tpl"
    });
});

app.directive("appFilereader", function ($q, $http) {
    var slice = Array.prototype.slice;

    return {
        restrict: "A",
        require: "?ngModel",
        link: function (scope, element, attrs, ngModel) {
            if (!ngModel) return;

            ngModel.$render = function () { };
            element.bind("change", function (e) {
                var element = e.target;
                var acceptedFile = element.accept;

                if (!element.value) return;

                element.disabled = true;
                $q.all(slice.call(element.files, 0).map(readFile))
                    .then(function (values) {
                        if (element.multiple) {
                            ngModel.$setViewValue(values);
                        } else {
                            let { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo } = GetSelectedOneRowData(gridOpsTableId);
                            let opdt = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, scope.$ctrlNode.opsProcess.OpSerial);
                            var imgFile = values[0].file,
                                imgData = window.SetDataToUploadFile(imgFile, opdt);

                            if (values[0].isImage) {
                                ngModel.$setViewValue({
                                    src: "../img/uploading.gif",
                                    file: scope.$ctrlNode.fileInfo
                                });

                                var config = scope.$ctrlNode.postConfig("/OpLayout/UploadImageProcess", imgData);

                                $http(config).then(function (response) {
                                    if (response.data !== "fail") {
                                        ngModel.$setViewValue({
                                            src: values[0].src,
                                            file: imgFile
                                        });

                                        scope.$ctrlNode.opsProcess.ImageName = response.data;
                                    } else {
                                        ngModel.$setViewValue({
                                            src: "../img/upload-failed.jpg",
                                            file: scope.$ctrlNode.fileInfo
                                        });
                                    }
                                }, function (error) {
                                    ShowAjaxError(error, "/OpLayout/UploadImageProcess");
                                });
                            }
                            if (values[0].isVideo) {
                                ngModel.$setViewValue({
                                    src: "",
                                    poster: "../img/uploading.gif",
                                    file: scope.$ctrlNode.fileInfo
                                });

                                var opsVideoFileName = "";
                                var uploadResult = true;
                                var uploadRequest = UploadProcessVideo(imgFile, function (chunk, filePartName, index) {
                                    var videoData = window.SetDataToUploadFile(chunk, opdt);
                                    videoData.append("FileName", filePartName);
                                    let config = {
                                        async: true,
                                        url: "/OpLayout/UploadVideoProcess",
                                        postData: videoData
                                    };
                                    let request = AjaxUploadFile(config, function (response) {
                                        if (response === StatusResult.Fail) {
                                            uploadResult = false;
                                        } else {
                                            if (index === 0) {
                                                opsVideoFileName = response;
                                            }
                                        }
                                    });

                                    request.fail(function () {
                                        uploadResult = false;
                                    });

                                    return request;
                                });

                                $.when.apply(null, uploadRequest).done(function () {
                                    if (uploadResult) {
                                        ngModel.$setViewValue({
                                            src: values[0].src,
                                            poster: "",
                                            file: values[0].file
                                        });
                                        scope.$ctrlNode.opsProcess.VideoFile = opsVideoFileName;
                                    } else {
                                        ngModel.$setViewValue({
                                            src: "",
                                            poster: "../img/upload-failed.jpg",
                                            file: scope.$ctrlNode.fileInfo
                                        });
                                    }
                                });
                            }
                        }
                        element.value = null;
                        element.disabled = false;
                    }, function () {
                        element.disabled = false;
                    });

                function readFile(file) {
                    var deferred = $q.defer();
                    var reader = new FileReader();
                    reader.onload = function () {
                    };
                    reader.onerror = function () {
                        deferred.reject();
                    };
                    reader.onloadend = function (e) {
                        var header = GetFileHeader(e.target.result);
                        var fileUrl = URL.createObjectURL(file);

                        //Check the file signature
                        switch (acceptedFile) {
                            case "image/*":
                                var isImage = IsImageFile(header);
                                if (isImage) {
                                    deferred.resolve({ src: fileUrl, file: file, isImage: isImage });
                                } else {
                                    ShowMessage("Error ", "Please select image file (png or jpg).", ObjMessageType.Error);
                                    ngModel.$setViewValue({
                                        src: "../img/no-image.png",
                                        file: scope.$ctrlNode.fileInfo
                                    });
                                    deferred.reject();
                                }
                                break;
                            case "video/*":
                                var isVideo = IsVideoFile(header);
                                if (isVideo) {
                                    deferred.resolve({ src: fileUrl, file: file, isVideo: isVideo });
                                } else {
                                    ShowMessage("Error ", "Please select video file (mp4 or mov).", ObjMessageType.Error);
                                    deferred.reject();
                                    ngModel.$setViewValue({
                                        src: "",
                                        poster: "../img/no-video.png",
                                        file: scope.$ctrlNode.fileInfo
                                    });
                                }
                                break;
                        }
                    };

                    reader.readAsArrayBuffer(file);

                    return deferred.promise;
                }
            });
        }
    };
});

app.directive("floatingPanel", function () {
    return {
        restrict: "A",
        scope: {
            parentTag: "@",
            title: "@",
            content: "@",
            htmlTag: "@",
            theme: "@"
        },
        link: function (scope, elem, attrs) {
            const content = scope.htmlTag === undefined ? scope.content : $("#" + scope.htmlTag);
            const title = scope.title === undefined ? "" : scope.title;
            const header = attrs.removeContent ? content : title;

            const config =
            {
                id: attrs.panelId,
                headerTitle: header,
                position: "center",
                headerControls: { controls: "closeonly" },
                content: attrs.removeContent ? "" : content,
                theme: scope.theme === undefined ? "primary filled" : scope.theme,
                callback: function (panel) {
                    if (attrs.removeContent) this.content.remove();
                    $("i", this.header.title).css({ 'font-size': "2rem", 'margin-right': "8px", 'cursor': "pointer" });
                    $("div.jsPanel-controlbar").css({
                        'position': "absolute",
                        'top': "5px",
                        'right': "5px"
                    });
                    panel.hide();
                },
                onbeforeclose: function (panel) {
                    panel.hide();

                    if (attrs.panelId === "panel-colorpicker") {
                        $("group").css({ 'cursor': "pointer" });
                        $("node").css({ 'cursor': "move" });
                    }

                    return false;
                }
            };

            if (scope.parentTag !== undefined) {
                const element = $("#" + scope.parentTag);
                config.contentSize = { width: element.width(), height: element.height() };
            }

            $.jsPanel({ config });
        }
    };
});

app.directive("someVideo", () => {
    return {
        scope: {
            //videoCurrentTime: "=videoCurrentTime"
        },
        controller: ($scope, $element) => {
            //console.log("💋 directive-someVideo: controller");
            //console.log($scope.$parent.selectedVideo);

            $scope.onTimeUpdate = () => {
                //$scope.videoCurrentTime = $element[0].currentTime;
                //console.log("ontimeUpdate angular directive");
                //console.log($scope.$parent.selectedVideo);
                //console.log($element[0].currentTime);
                //console.log($element);

                // If current time is equally ended time of part-video, pause video.
                if ($element && $element[0] && $element[0].currentTime >= convertToSecond1($scope.$parent.selectedVideo.EndTime)) {
                    $element[0].pause();
                }

                $scope.$apply();
            };
        },
        link: (scope, elm) => {
            //scope.$watch("videoCurrentTime", (newVar) => {
            //    elm[0].currentTime = newVar;
            //});
            console.log("😍 directive-someVideo: link");
            //console.log(elm);
            //console.log(scope);
            //console.log(scope.$parent.selectedVideo);

            if (elm && elm[0]) elm[0].currentTime = scope.$parent.selectedVideo.StartedDuration;

            scope.$parent.$watch("selectedVideo", (newVar) => {
                if (elm && elm[0]) elm[0].currentTime = newVar.StartedDuration;
            });

            elm.bind("timeupdate", scope.onTimeUpdate);
        }
    };
});
//#endregion

//#region Angular services
app.service("sharedService", function () {
    const getUrConfig = {
        url: "/OpLayout/GetUserRole",
        async: true
    };

    function getUserRole(callback) {
        return AjaxGetCommon(getUrConfig, callback);
    }

    const getFacConfig = {
        url: "/OpLayout/GetFactoryByTypeAndStatus"
    };

    function getFactory(callback) {
        return AjaxGetCommon(getFacConfig, callback);
    }

    const service = { getUserRole: getUserRole, getFactory: getFactory };

    return service;
});
//#endregion

//#region Angular filter
app.filter("trustUrl", ["$sce", function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);
//#endregion

//#region Angular controllers
app.controller("OpsLayoutController", function ($uibModal, $log, $document, $scope, $http, jsPlumbService, sharedService) {
    //#region JsPlumb
    var ctrl = this;
    let toolkit;
    var surface;
    window.jsps = jsPlumbService;
    window.ctrl = this;
    $scope.draggableTypes = [{ label: "Group", type: "group", group: true }, { label: "Process", type: "node" },
    { label: "Summary", type: "sumTitle", group: true }, { label: "Summary", type: "sumEditable", group: true },
    { label: "Summary", type: "sumMainMachines", group: true }, { label: "Summary", type: "sumAllMachines", group: true },
    { label: "Summary", type: "sumTool", group: true }, { label: "Summary", type: "sumWorker", group: true },
    { label: "Summary", type: "showPattern", group: true }, { label: "Summary", type: "sumBom", group: true },
    { label: "Summary", type: "jigSum", group: true }];

    $scope.OpVideo1 = [];

    ctrl.toolkitParams = {
        groupFactory: function (type, data, callback) {
            if (data.type !== "group") {
                let pos = {
                    top: `${data.top}px`,
                    left: `${data.left}px`
                };

                const lang = $scope.layoutLang.selectedLang;
                createSummary(data.type, pos, lang);
            } else {
                const currentOpmt = GetSelectedOneRowData(gridOpsTableId);
                if (currentOpmt.ConfirmChk === "Y") {
                    const msg = getMsgByLang(msgOpConfirmed);
                    ShowMessage(msg.title, msg.value);
                    return;
                }

                const isOpmtEmpty = !Object.keys(currentOpmt).length;
                if (!isOpmtEmpty) {
                    checkUserRole(currentOpmt.Edition, "Add", (result) => {
                        if (result !== true) {
                            const msg = getMsgByLang(msgPermission);
                            ShowMessage(msg.title, msg.value);
                            return;
                        } else {
                            if ($scope.groupsToAdd.length > 0) {
                                var modalInstance = $uibModal.open({
                                    animation: $scope.animationsEnabled,
                                    ariaLabelledBy: "modal-title",
                                    ariaDescribedBy: "modal-body",
                                    templateUrl: "grModalContent.html",
                                    controller: "GroupModalCtrl",
                                    controllerAs: "$ctrl",
                                    windowClass: "modal-custom",
                                    appendTo: undefined,
                                    resolve: {
                                        items: function () {
                                            return $scope.groupsToAdd;
                                        }
                                    }
                                });

                                modalInstance.result.then(function (selectedItem) {
                                    const group = toolkit.getGroup(selectedItem.id);
                                    if (group !== null && group !== undefined) {
                                        let g = $(`group[data-jtk-group-id='${group.data.id}']`);
                                        let miniviewG = $(`div.jtk-miniview-group-element[jtk-node-id='${group.data.id}']`);
                                        miniviewG.width(170).height(240);

                                        miniviewG.show();
                                        g.show();

                                        const msg = getMsgByLang(msgExistGroup);

                                        MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);
                                    } else {
                                        data.title = selectedItem.title;
                                        data.id = selectedItem.id;
                                        console.log(data);
                                        callback(data);
                                        toggleGroups(true);
                                        $scope.isShowGroups = true;
                                    }
                                }, function () {
                                    $log.info("Modal dismissed at: " + new Date());
                                });
                            } else {
                                const msg = getMsgByLang(msgNoGroup);
                                ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                            }
                        }
                    });
                }
                else {
                    const msg = getMsgByLang(msgSelectOpFirstly);
                    ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                }
            }
        },
        nodeFactory: function (type, data, callback) {
            const currentOpmt = GetSelectedOneRowData(gridOpsTableId);
            const isOpmtEmpty = !Object.keys(currentOpmt).length;

            switch (data.type) {
                case "node":
                    data.name = toolkit.getNodeCount() + 1;
                    if (!isOpmtEmpty) {
                        if (currentOpmt.ConfirmChk === "Y") {
                            const msg = getMsgByLang(msgOpConfirmed);
                            ShowMessage(msg.title, msg.value);
                            return;
                        } else {
                            checkUserRole(currentOpmt.Edition, "Add", (result) => {
                                if (result !== true) {
                                    const msg = getMsgByLang(msgPermission);
                                    ShowMessage(msg.title, msg.value);
                                    return;
                                } else {
                                    window.LayoutPage = $scope.layoutPage.pageNo;
                                    window.LayoutGroupMode = $scope.opdtMode;
                                    window.LayoutTopY = data.top;
                                    window.LayoutLeftX = data.left;
                                    window.ProcessHeight = $scope.pLayoutModifier.pHeight;
                                    window.ProcessWidth = $scope.pLayoutModifier.pWidth;
                                    window.LayoutFontSize = $scope.opsFont.fontSize;

                                    window.LayoutEvent = true;

                                    // If group mode is Module Type, disable Module dropdown list.
                                    document.getElementById("drpModule").disabled = $scope.opdtMode === "ModuleType";

                                    // For creating new process
                                    window.StatusUpdateProcess = 0;
                                    LayoutSaveEvent(callback);
                                }
                            });
                        }

                    } else {
                        const msg = getMsgByLang(msgSelectOpFirstly);
                        ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                    }

                    break;
            }
        }
        //beforeStartConnect: (node, edgeType) => {
        //    console.log("beforeStartConnect");
        //    return { label: "..." };
        //}
    };

    ctrl.renderParams = {
        view: {
            nodes: {
                "default": {
                    template: "node",
                    events: {
                        click: function (params) {
                            params.e.stopPropagation();
                            const isDisplayColorpicker = $("#panel-colorpicker").is(":visible");
                            if (isDisplayColorpicker) {
                                const color = $scope.opsColor.hexPicker;

                                if (color === "") {
                                    const msg = getMsgByLang(msgSelectColor);
                                    MsgInform(msg.title, msg.value, ObjMessageType.Error, true, false);
                                } else {
                                    params.node.data.DisplayColor = color;
                                    toolkit.updateNode(params.node);

                                    window.window.isChange = true;
                                }
                            }
                            if ($scope.isCloneMode) {
                                const opmt = GetSelectedOneRowData(gridOpsTableId);
                                const isOpmtEmpty = !Object.keys(opmt).length;

                                if (!isOpmtEmpty) {
                                    if (opmt.ConfirmChk === "Y") {
                                        const msg = getMsgByLang(msgOpConfirmed);
                                        ShowMessage(msg.title, msg.value);
                                        return;
                                    } else {
                                        checkUserRole(opmt.Edition, "Add", (result) => {
                                            if (result !== true) {
                                                const msg = getMsgByLang(msgPermission);
                                                ShowMessage(msg.title, msg.value);
                                                return;
                                            } else {
                                                const n = params.node.data, opNum = n.name.split("]")[0].split("[");
                                                //console.log(n);

                                                if (opNum[1].length >= 9) {
                                                    const msg = getMsgByLang(msgLimitedLength);
                                                    ShowMessage(msg.title, `${msg.value}<b> [${opNum[1]}] </b>.`, ObjMessageType.Error);
                                                    return;
                                                }
                                                const grp = toolkit.getGroup(n.group),
                                                    xN = parseInt(n.left.toString()) + 2,
                                                    yN = parseInt(n.top.toString()) + 2,
                                                    xG = grp ? getTopLeft(grp.data.left.toString()) : null,
                                                    yG = grp ? getTopLeft(grp.data.top.toString()) : null,
                                                    colorToSave = n.DisplayColor.slice(0, 1) + "FF" + n.DisplayColor.slice(1, 7),
                                                    opdt = new Opdt(opmt.Edition, "", opmt.StyleCode, opmt.StyleColorSerial, opmt.StyleSize, opmt.RevNo, opmt.OpRevNo, n.id, n.OpName, n.OpGroup, n.MachineType, n.ModuleId, null, n.Page, xG + "." + xN.toString(), yG + "." + yN.toString(), colorToSave),
                                                    ajaxConfig = new AjaxConfig("/OpLayout/CloneSingleProcess", true, JSON.stringify({ opdt: opdt }));

                                                AjaxPostCommon(ajaxConfig, (response) => {
                                                    if (response.error === null || response.error === undefined) {
                                                        const opdtRs = response.result;

                                                        console.log(opdtRs);

                                                        if (opdtRs !== "false") {
                                                            //const node = CreateObjectForLayout(opmt, opdtResult);
                                                            //node.DisplayColor = n.DisplayColor;
                                                            //node.left = parseInt(n.left) + 2;
                                                            //node.top = parseInt(n.top) + 2;
                                                            //node.group = n.group;
                                                            //node.MachineName = n.MachineName;
                                                            //node.CanDelete = window.CanDelete;
                                                            //node.Title = n.Title;

                                                            const layoutProcess = new LayoutProcess(opdtRs.OpSerial.toString(), `[${opdtRs.OpNum}] ${n.OpName}`, n.OpTime,
                                                                n.MachineCount, n.MachineName, n.ManCount, n.OpName, n.VnOpName, n.GbOpName, n.MmOpName, n.IdOpName, n.EtOpName,
                                                                n.Codes, n.IconNames, n.MainProcessArr, n.HotSpot, n.OpsState, opdtRs.OpNum, n.Remarks, n.OpGroup, n.MachineType,
                                                                n.ModuleId, n.DisplayColor, n.ProcessWidth, n.ProcessHeight, n.LayoutFontSize, n.Tools, n.IsDisplay, n.Page,
                                                                n.CanDelete, n.ShowButtonPlayVideo, parseInt(n.left) + 2, parseInt(n.top) + 2, n.group, n.Title, n.IconNameArr,
                                                                n.IconUrl, n.IconName);

                                                            toolkit.addNode(layoutProcess);
                                                        } else {
                                                            console.log("An error occurred, could not clone this process.");
                                                        }
                                                    } else {
                                                        console.log(response.error);
                                                    }
                                                });
                                            }
                                        });
                                    }
                                } else {
                                    const msg = getMsgByLang(msgSelectOpFirstly);
                                    ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                                }
                            }
                        },
                        dblclick: (params) => {
                            const grp = toolkit.getGroup(params.node.data.group);
                            const pcs = grp.getNodes();

                            for (let i = 0; i < pcs.length; i++) {
                                if (pcs[i].data.id === params.node.data.id) {
                                    const deletedItem = pcs.splice(i, 1)[0];
                                    pcs.push(deletedItem);
                                }
                            }
                        },
                        contextmenu: params => {
                            console.log("Displaying opdt.");

                            const process = params.node.data;
                            window.LayoutPage = $scope.layoutPage.pageNo;
                            window.LayoutGroupMode = $scope.opdtMode;
                            window.LayoutTopY = process.top;
                            window.LayoutLeftX = process.left;
                            LayoutEvent = true;
                            window.StatusUpdateProcess = 0;

                            //LayoutUpdateEvent(process, (node) => {
                            //    toolkit.updateNode(node);
                            //});

                            // Disable module selection
                            document.getElementById("drpModule").disabled = $scope.opdtMode === "ModuleType";

                            LayoutUpdateEvent(process, (node) => {
                                console.log(node);

                                const rdOpName = $scope.rdOpName;
                                process.Remarks = node.Remarks;
                                switch (rdOpName) {
                                    case "1":
                                        process.name = node.Remarks !== "" ? `[${node.OpNum}] ${node.OpName} (${node.Remarks})` :
                                            `[${node.OpNum}] ${node.OpName}`;
                                        break;
                                    case "3":
                                        process.name = `[${node.OpNum}] ${node.Remarks}`;
                                        break;
                                    default:
                                        process.name = `[${node.OpNum}] ${node.OpName}`;
                                        break;
                                }
                                process.OpTime = node.OpTime;
                                process.OpNum = node.OpNum;
                                process.OpName = node.OpName;
                                process.Remarks = node.Remarks;
                                process.OpTime = node.OpTime;
                                process.MachineName = node.MachineName;
                                process.MachineType = node.MachineType;
                                process.MachineCount = node.MachineCount;
                                process.OpsState = node.OpsState;
                                process.HotSpot = node.HotSpot;
                                process.IconName = node.IconName;

                                toolkit.updateNode(process);

                                window.isChange = true;
                            });

                            params.e.preventDefault();
                            params.e.stopPropagation();
                        }
                    },
                    dragOptions: {
                        start: () => {
                            if (!$scope.isShowGroups) toggleGroups(true);
                        },
                        stop: (n) => {
                            if (!$scope.isShowGroups) toggleGroups(false);
                            if (n.el.jtk.node.data.group) {
                                const grp = toolkit.getGroup(n.el.jtk.node.data.group);
                                n.el.jtk.node.data.Title = grp.data.title;
                                toolkit.updateNode(n.el.jtk.node);
                            } else {
                                console.log("This process does not belong to a group currently.");
                                console.log("Actually it's bug of jsplumb");

                                const r1 = n.el.getBoundingClientRect(),
                                    grs = surface.getGroups();

                                for (let g of grs) {
                                    const grt = document.querySelectorAll(`[data-jtk-group-id="${g.id}"]`),
                                        r2 = grt[0].getBoundingClientRect();

                                    if (intersectRect(r1, r2)) {
                                        n.el.jtk.node.data.Title = g.data.title;
                                        n.el.jtk.node.data.group = g.data.id;
                                        toolkit.updateNode(n.el.jtk.node);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            },
            groups: {
                "default": {
                    template: "group",
                    endpoint: "Blank",
                    anchor: "Continuous",
                    revert: false, // if drop outside the group, the position will be revert.
                    constrain: false, // only allowing drop inside the group.
                    events: {
                        click: (params) => {
                            console.log("click group");
                            //surface.repaintEverything();
                            //surface.getJsPlumb().repaintEverything();
                            //surface.refresh();

                            const isDisplayColorpicker = $("#panel-colorpicker").is(":visible");
                            if (isDisplayColorpicker) {
                                const color = $scope.opsColor.hexPicker;
                                if (color === "") {
                                    const msg = getMsgByLang(msgSelectColor);
                                    MsgInform(msg.title, msg.value, ObjMessageType.Error, true, false);
                                } else {
                                    window.isChange = true;

                                    processChangedColor($(params.el), color, "background", `node-bygroup-print-${params.node.data.id}`);
                                    params.node.data.DisplayColor = color;
                                    const nodesByGroup = params.node.getNodes();
                                    for (let n of nodesByGroup) {
                                        n.data.DisplayColor = color;
                                        toolkit.updateNode(n);
                                    }
                                }
                            }
                        },
                        contextmenu: params => {
                            const isShowBtnUpdateModule = $scope.opdtMode === "ModuleType" ? false : true,
                                modalConfig = {
                                    templateUrl: "changePageNoModalContent.html",
                                    controller: "ChangeNodePageModalCtrl",
                                    controllerAs: "$ctrl",
                                    modalData: { maxPage: $scope.layoutPage.maxPage, modules: $scope.modules, isShowBtnUpdateModule }
                                };
                            modalInstance(modalConfig, (modalData) => {
                                const nodes = params.node.getNodes(), opdts = [];
                                let { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo } = GetSelectedOneRowData(gridOpsTableId);

                                // If there are selected page, updating page
                                if (modalData.pageNo) {
                                    for (let n of nodes) {
                                        const opdt = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, n.id, n.data.OpName,
                                            n.data.OpGroup, n.data.MachineType, n.data.ModuleId, null, modalData.pageNo);
                                        opdts.push(opdt);
                                    }
                                    const cf = new AjaxConfig("/OpLayout/UpdatePage", true, JSON.stringify({ opdts }));
                                    AjaxPostCommon(cf,
                                        (res) => {
                                            if (res) {
                                                toolkit.removeGroup(params.node, true); // set true for removing all of children.
                                            } else {
                                                console.log(res);
                                            }
                                        });
                                } else {
                                    // If there are selected module, updating module
                                    if (modalData.selectedModule) {
                                        for (let n of nodes) {
                                            const opdt = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, n.id, n.data.OpName,
                                                n.data.OpGroup, n.data.MachineType, modalData.selectedModule.ModuleId, null, n.data.Page);
                                            opdts.push(opdt);
                                        }

                                        const cf = new AjaxConfig("/OpLayout/UpdateModule", true, JSON.stringify({ opdts }));
                                        AjaxPostCommon(cf, (res) => {
                                            if (res) {
                                                for (let n of nodes) {
                                                    n.ModuleId = modalData.selectedModule.ModuleId;
                                                    toolkit.updateNode(n);
                                                }
                                            } else {
                                                console.log(res);
                                            }
                                        });
                                    }
                                }

                                surface.refresh();
                            });
                        }
                    }
                    //dragOptions: {
                    //    start: (g) => {
                    //        console.log(g);
                    //        surface.repaintEverything();
                    //        surface.getJsPlumb().repaintEverything();
                    //        surface.refresh();
                    //    },
                    //    stop: (g) => {
                    //        //console.log(g);
                    //        //surface.repaintEverything();
                    //        //surface.getJsPlumb().repaintEverything();
                    //        //surface.refresh();
                    //    }
                    //}
                },
                constrained: {
                    parent: "default",
                    constrain: true
                }
            },
            edges: {
                "default": {
                    events: {
                        tap: (params) => {
                            let isDisplayColorpicker = $("#panel-colorpicker").is(":visible");
                            let color = $scope.opsColor.flow;

                            if (isDisplayColorpicker && color !== "") {
                                params.connection.setPaintStyle({
                                    stroke: `${color}`,
                                    strokeWidth: 1
                                }, true);
                            }

                            window.isChange = true;
                        }
                    }
                }
            }
        },
        layout: {
            type: "Absolute"
        },
        jsPlumb: {
            Anchor: "Continuous",
            Endpoint: "Blank",
            Connector: ["Flowchart", {
                cssClass: "connectorClass",
                hoverClass: "connectorHoverClass"
            }],
            PaintStyle: {
                strokeWidth: 2,
                stroke: "#527085"
            },
            HoverPaintStyle: {
                stroke: "orange"
            },
            Overlays: [["Arrow", {
                fill: "#09098e",
                width: 15,
                length: 15,
                location: 1
            }],
            ["Custom", {
                create: (component) => {
                    if (component.element === undefined) {
                        let isShow = $scope.savingRule.canSave && $scope.isShowDeleteFlow ? "block" : "none";

                        const isSourceVisible = $(component.source).is(":visible");
                        const isTargetVisible = $(component.target).is(":visible");

                        if (!isSourceVisible || !isTargetVisible) isShow = "none";
                        //isShow = $scope.isShowDeleteFlow ? "block" : "none";

                        return $(`<img width="25px" height="25px" style="display: ${isShow}; background-color:transparent;"` +
                            `src="../../assets/ops-layout/img/remove-flow.svg" alt="Delete connection">`);
                    } else {
                        return $("<div></div>");
                    }
                },
                location: 0.5,
                cssClass: "delete-connection",
                events: {
                    click: (params) => {
                        const smsg = getMsgByLang(msgConfirmDelete);
                        ConfirmYesNo(smsg.title, smsg.value, () => {
                            params.component.getAttachedElements()[0].deleteEveryConnection();
                        });
                    }
                }
            }]]
        },
        lassoFilter: ".controls, .controls *, .miniview, .miniview *",
        dragOptions: {
            filter: ".delete *, .group-connect *",
            magnetize: false,
            stop: (nodeOrGroup) => {
                if (nodeOrGroup.el.nodeName === "NODE") {
                    summarizeProcessByGroup();
                }

                window.isChange = true;

                //console.log(nodeOrGroup);
            }
        },
        events: {
            canvasClick: function () {
                console.log("canvasClick");
            },
            modeChanged: function () {
                //alert(n);
            },
            edgeAdded: (e) => {
                if (e.addedByMouse) {
                    window.isChange = true;
                }
            },
            edgeRemoved: (e) => {
                if (e.addedByMouse) {
                    window.isChange = true;
                }
            }
        },
        consumeRightClick: false,
        lassoInvert: true,
        wheelReverse: true
    };
    var datasetContainer = document.querySelector(".jtk-ops-dataset");

    // ---------------- update data set -------------------------
    var syntaxHighlight = function (json) {
        json = json.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
        return "<pre>" +
            json
                .replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,
                    function (match) {
                        var cls = "number";
                        if (/^"/.test(match)) {
                            if (/:$/.test(match)) {
                                cls = "key";
                            } else {
                                cls = "string";
                            }
                        } else if (/true|false/.test(match)) {
                            cls = "boolean";
                        } else if (/null/.test(match)) {
                            cls = "null";
                        }
                        return '<span class="' + cls + '">' + match + "</span>";
                    }) +
            "</pre>";
    };
    var updateDataset = function () {
        datasetContainer.innerHTML = syntaxHighlight(JSON.stringify(toolkit.exportData(), null, 4));
        //alert("when is it executed?");

        console.log("Data is updated.");
    };

    // element is the DOM element into which the toolkit was rendered
    ctrl.init = function (scope, element) {
        toolkit = window.toolkit = scope.toolkit;

        surface = window.jsPlumbSurface = jsPlumbService.getSurface("opsLayOutSurface");

        var controls = element[0].querySelector("#controls-mode");

        // listener for mode change on renderer.
        surface.bind("modeChanged", function (mode) {
            jsPlumb.removeClass(controls.querySelectorAll("[mode]"), "selected-mode");
            jsPlumb.addClass(controls.querySelectorAll("[mode='" + mode + "']"), "selected-mode");
        });

        // pan mode/select mode
        jsPlumb.on(controls, "tap", "[mode]", function () {
            surface.setMode(this.getAttribute("mode"));
        });

        // on home button click, zoom content to fit.
        jsPlumb.on(controls, "tap", "[reset]", function () {
            toolkit.clearSelection();
            surface.zoomToFit();
        });

        // any operation that caused a data update (and would have caused an autosave), fires a dataUpdated event.
        toolkit.bind("dataUpdated", updateDataset);

        toolkit.beforeConnect = function (source, target) {
            var result = true;
            //surface.repaintEverything();
            //surface.getJsPlumb().repaintEverything();
            //surface.refresh();

            toolkit.eachEdge((index, value) => {
                if (source.id === value.source.id) {
                    const msg = getMsgByLang(msgAcceptLineFlow);
                    ShowMessage(msg.title, msg.value, ObjMessageType.Error);
                }
                if (source.id === value.source.id || source.id === target.id ||
                    source.id === value.target.id && target.id === value.source.id) {
                    result = false;
                    return;
                }
            });

            //console.log(result);
            return result;
        };

        toolkit.beforeStartConnect = (s, t) => {
            //console.log("acquaintance");
            if (hasRefreshCanvas === false) {
                surface.refresh();
                hasRefreshCanvas = true;

                console.log("refreshed canvas before connect");
            }

            //console.log("not work");
        };

        loadLayout();
    };
    ctrl.typeExtractor = el => el.getAttribute("jtk-node-type");
    //#endregion

    //#region Variables
    $scope.Opdts = [];
    $scope.rdOpName = "2";
    $scope.sharedService = sharedService;
    window.processPage = 1;
    $scope.layoutPage = {
        pageNo: 1,
        maxPage: 1
    };
    $scope.layoutLang = {
        showLangCb: false,
        selectedLang: "df",
        opsLanguages: [
            new OpLanguage("Default Lang", "df", "pk-flag.png"),
            new OpLanguage("Tiếng Việt", "vn", "blank.gif"),
            new OpLanguage("English", "gb", "blank.gif"),
            new OpLanguage("မြန်မာဘာသာ", "mm", "myanmar-flag-icon-16.png"),
            new OpLanguage("Bahasa", "id", "blank.gif"),
            new OpLanguage("Amharic", "et", "blank.gif")
        ]
    };

    $scope.layoutViewType = {
        showLayoutViewCb: false,
        selectedViewType: new OpViewType("OpName", "1", "letter-t.svg"),
        layoutViewTypes: [
            new OpViewType("OpName", "1", "letter-t.svg"),
            new OpViewType("Code", "2", "code-p.svg")
            //new OpViewType("Icon", "3", "image.svg")
        ]
    };

    $scope.groupsToAdd = [];
    $scope.animationsEnabled = true;

    const opmt = GetSelectedOneRowData(gridOpsTableId);
    var isOpmtEmpty = !Object.keys(opmt).length;

    const { GroupMode, CanvasHeight, ProcessHeight, ProcessWidth, LayoutFontSize, ConfirmChk } = opmt !== null && !isOpmtEmpty ?
        opmt : {
            GroupMode: defaultGroupMode, CanvasHeight: defaultCanvasHeight, ProcessHeight: defaultProcessHeight,
            ProcessWidth: defaultProcessWidth, LayoutFontSize: defaultFontSize
        };

    const canvasHeight = CanvasHeight === "0" ? defaultCanvasHeight : CanvasHeight;
    const fontSize = LayoutFontSize === "0" ? defaultFontSize : LayoutFontSize;
    const pHeight = ProcessHeight === "" ? defaultProcessHeight : ProcessHeight;
    const pWidth = ProcessWidth === "" ? defaultProcessWidth : ProcessWidth;

    $scope.canvasStyle = { "height": parseInt(canvasHeight) };

    $scope.pLayoutModifier = {
        noProcessInRow: 1,
        pHeight: parseInt(pHeight),
        pWidth: parseInt(pWidth),
        pVerticalDistance: 30,
        pHorizontalDistance: 30
    };

    $scope.opdtMode = GroupMode === "" ? defaultGroupMode : GroupMode;
    $scope.opsColor = {
        hexPicker: "#FFFFFF",
        selectedGroup: "#FFFFFF",
        processText: "#FFFFFF",
        groupText: "#FFFFFF",
        flow: "#FFFFFF"
    };

    //Get colors
    $http.post("/OpLayout/GetColour").then((response) => {
        const lstColor = [];
        $.each(response.data, (index, value) => {
            const opColor = new OpColor(value.ColorDesc, "#" + value.HexaValue, value.OpGroup, value.Module);
            lstColor.push(opColor);
        });
        $scope.opsColors = lstColor;
    });

    const curOpmt = GetSelectedOneRowData(gridOpsTableId);
    //Get colors
    $http.post("/OpLayout/GetModulesByCode", { styleCode: curOpmt.StyleCode }).then((response) => {
        $scope.modules = response.data.modules;
        $scope.modules.unshift({ ModuleId: "", ModuleName: "" });
    });

    $scope.opsFont = {
        fontSize: parseInt(fontSize)
    };

    $scope.showColorList = false;
    $scope.showSgrCp = false;
    $scope.showNodeTextCp = false;
    $scope.showFlowCp = false;
    $scope.showOpsModal = {
        isShowSummaries: false
    };

    $scope.showOptions = {
        isShowOpt: false
    };

    $scope.opmt = {
        remarks: null
    };
    $scope.savingRule = { canSave: false };
    $scope.isShowDeleteFlow = true;
    $scope.isShowFlow = true;
    $scope.isShowGroups = true;
    $scope.selectedVideo = {};
    $scope.SplitVideoUrl = "";
    //$scope.selectedProcessVideo = {};

    //#endregion

    //#region Summaries
    function summarizeProcessByGroup() {
        const isShowSumGrp = $(".grp-sum").is(":visible");

        if (isShowSumGrp) {
            toolkit.eachGroup(function (index, value) {
                const nodes = value.getNodes();
                const totalProcesses = nodes.length;
                let totalMachines = 0;
                let totalWorkers = 0;
                let totalTime = 0;

                nodes.forEach(function (item) {
                    totalMachines += item.data.MachineCount;
                    totalWorkers += item.data.ManCount;
                    totalTime += item.data.OpTime;
                });

                value.data.Processes = totalProcesses;
                value.data.Machines = totalMachines;
                value.data.Workers = totalWorkers;
                value.data.TotalOpTime = totalTime;

                toolkit.updateNode(value);
            });
        }
    }

    $scope.showSummaryModal = (isShow) => {
        var currentOpsMaster = GetSelectedOneRowData(gridOpsTableId);
        var isOpmtEmpty = !Object.keys(currentOpsMaster).length;
        if (isOpmtEmpty) {
            $scope.showOpsModal.isShowSummaries = false;
        } else {
            $scope.showOpsModal.isShowSummaries = !isShow;
        }
    };

    $scope.showOptionModal = (isShow) => {
        $scope.showOptions.isShowOpt = !isShow;
    };

    $scope.summarizeByGroup = () => {
        $(".grp-sum").toggle();

        summarizeProcessByGroup();
    };
    $scope.showOrdinalTools = false;
    //#endregion

    //#region Printing
    var inlineMediaStyle = null;
    var head = document.getElementsByTagName("head")[0];
    var newStyle = document.createElement("style");
    newStyle.setAttribute("type", "text/css");
    newStyle.setAttribute("media", "print");

    $scope.isFirst = false;

    $scope.exportPdf = function () {
        window.print();
    };

    //#endregion

    //#region Saving
    $scope.userRole = {
        rdRole: null,
        facRole: null
    };
    sharedService.getUserRole(function (response) {
        if (response.rdRole !== null || response.facRole !== null) {
            $scope.userRole = response;
            $scope.showSaveData.isShowSaveData = true;
        } else {
            console.log("Could not get User Role");
        }
    });

    sharedService.getFactory(function (response) {
        const factories = response.result;
        if (factories !== null && factories !== undefined) {
            $scope.factoryArr = factories;
        } else {
            console.log("Could not get factory for saving as Factory Edition");
        }
    });

    $scope.disableButtons = {
        btnSave: true,
        btnSaveAsNewOpRev: true,
        btnSaveAsFacEdition: true,
        btnSaveAndConfirm: true
    };
    $scope.factoryArr = [];
    $scope.factory = null;
    $scope.isShowFacCb = false;
    $scope.isShowGetMxPk = false;

    $scope.showSaveModal = (isClosedButton) => {
        $scope.factory = null;
        if (!isClosedButton) {
            $("#opslayoutModal").toggle();

            var currentOpsMaster = GetSelectedOneRowData(gridOpsTableId);
            var isOpmtEmpty = !Object.keys(currentOpsMaster).length;
            if (isOpmtEmpty) {
                $("#opslayoutModal").hide();
            } else {
                $scope.isShowFacCb = false;
                $scope.isNewForFom = false;

                // Disable all of buttons before checking enable whether
                $scope.disableButtons = {
                    btnSave: true,
                    btnSaveAsNewOpRev: true,
                    btnSaveAsFacEdition: true,
                    btnSaveAndConfirm: true
                };

                const { Edition, ConfirmChk } = GetSelectedOneRowData(gridOpsTableId);

                switch (Edition) {
                    case "O":
                    case "P":
                        // There are sufficient rights by OPM Menu by role
                        if ($scope.userRole.rdRole !== null) {
                            // If not confirmed yet
                            if (ConfirmChk === "") {
                                // IsUpdate equals 1 that means able to save.
                                if ($scope.userRole.rdRole.IsUpdate === "1") $scope.disableButtons.btnSave = false;

                                // IsAdd equals 1 that means able to copy.
                                if ($scope.userRole.rdRole.IsAdd === "1") $scope.disableButtons.btnSaveAsNewOpRev = false;

                                // IsConfirm equals 1 that means able to confirm.
                                if ($scope.userRole.rdRole.IsConfirm === "1") $scope.disableButtons.btnSaveAndConfirm = false;
                            } else {
                                if ($scope.userRole.rdRole.IsAdd === "1") $scope.disableButtons.btnSaveAsNewOpRev = false;
                            }
                        }

                        // User has sufficient rights by FOM Menu and role
                        if ($scope.userRole.facRole !== null) {
                            if ($scope.userRole.facRole.IsAdd === "1") $scope.disableButtons.btnSaveAsFacEdition = false;
                            $scope.isShowFacCb = true;
                        }
                        break;
                    case "A":
                        // If there are sufficient rights by FOM Menu by role
                        if ($scope.userRole.facRole !== null) {
                            if (ConfirmChk === "") {
                                if ($scope.userRole.facRole.IsUpdate === "1") $scope.disableButtons.btnSave = false;
                                if ($scope.userRole.facRole.IsAdd === "1") $scope.disableButtons.btnSaveAsNewOpRev = false;
                                if ($scope.userRole.facRole.IsConfirm === "1") $scope.disableButtons.btnSaveAndConfirm = false;
                            } else {
                                if ($scope.userRole.facRole.IsAdd === "1") $scope.disableButtons.btnSaveAsNewOpRev = false;
                            }

                            // Save as new OpRevNo also is save as factory edition in this case, so need to show the combobox
                            $scope.isShowFacCb = true;
                            $scope.isNewForFom = true;
                        }
                        break;
                    case "M":
                        // If User has sufficient rights by MES Menu by role
                        if ($scope.userRole.mesRole !== null) {
                            if (ConfirmChk === "") {
                                if ($scope.userRole.mesRole.IsUpdate === "1") $scope.disableButtons.btnSave = false;
                                if ($scope.userRole.mesRole.IsAdd === "1") $scope.disableButtons.btnSaveAsNewOpRev = false;
                                if ($scope.userRole.mesRole.IsConfirm === "1") $scope.disableButtons.btnSaveAndConfirm = false;
                            } else {
                                if ($scope.userRole.mesRole.IsAdd === "1") $scope.disableButtons.btnSaveAsNewOpRev = false;
                            }

                            $scope.isShowGetMxPk = false;
                        }

                        // User has sufficient rights by Mes menu and role
                        if ($scope.userRole.facRole !== null) {
                            if ($scope.userRole.facRole.IsAdd === "1") $scope.disableButtons.btnSaveAsFacEdition = false;
                            $scope.isShowFacCb = true;
                        }
                        break;
                }
            }
        } else {
            $("#opslayoutModal").hide();
        }
    };

    $scope.showSaveData = {
        isShowSaveData: false
    };

    //START ADD) SON - 06/Mar/2019
    function saveAsNewOp(url, isSaveAsFom, isSaveAsNew) {
        $.blockUI(window.BlockUI);
        LayoutEvent = true;
        let { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, Remarks, Factory } = GetSelectedOneRowData(gridOpsTableId);
        const lang = MapFlagValueToLanguage($scope.layoutLang.selectedLang);
        const opsViewMode = $scope.opdtMode;
        let remarks = $scope.opmt.remarks;
        if (remarks === null || remarks === undefined || remarks.trim() === "") remarks = Remarks;

        let factory = $scope.factory;
        if (isSaveAsFom || $scope.isNewForFom && isSaveAsNew || Factory === "" && Edition === "M") {
            if (factory === undefined || factory === null) {
                const msg = getMsgByLang(msgSelectFactory);
                MsgInform(msg.title, msg.value, ObjMessageType.Error, false, true);
                $.unblockUI();
                return;
            }
        }

        let edition2 = "";
        if (isSaveAsFom) edition2 = 'A';

        const opmt = new Opmt(Edition, edition2, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, lang, opsViewMode,
            $scope.pLayoutModifier.pWidth, $scope.pLayoutModifier.pHeight, $scope.opsFont.fontSize,
            $scope.canvasStyle.height, factory, remarks);

        var data = toolkit.exportData({
            type: "json",
            parameters: {
                importantNumber: 34,
                somePrefix: "foo-"
            }
        });
        var nodeArray = [];

        $(data.nodes).each(function (i, n) {
            let xN = getTopLeft(n.left.toString());
            let yN = getTopLeft(n.top.toString());
            const colorToSave = n.DisplayColor.slice(0, 1) + "FF" + n.DisplayColor.slice(1, 7);
            const opName = n.OpName.length > 200 ? n.OpName.substr(0, 200) : n.OpName;

            // If there are no group, pick up first one node for position of null group
            if (data.groups.length === 0) {
                let opNext;
                let grpX = 0;
                let grpY = 0;
                if (i === 0) {
                    grpX = xN;
                    grpY = yN;
                    xN = dfNodeLeft;
                    yN = dfNodeTop;
                }

                $(data.edges).each(function (k, e) {
                    if (e.source === n.id) {
                        opNext = e.target;
                    }
                });

                const node = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, n.id, opName,
                    null, null, null, opNext, n.Page, grpX + "." + xN, grpY + "." + yN, colorToSave);

                nodeArray.push(node);
            }

            $(data.groups).each(function (j, g) {
                var xG = getTopLeft(g.left.toString());
                var yG = getTopLeft(g.top.toString());

                if (n.group === g.id) {
                    var opNext = null;
                    $(data.edges).each(function (k, e) {
                        if (e.source === n.id) {
                            opNext = e.target;
                        }
                    });

                    var opGroup = n.OpGroup;
                    var machineType = n.MachineType;
                    var moduleId = n.ModuleId;

                    switch (opsViewMode) {
                        case "OpGroup":
                            opGroup = n.group.substring(1);
                            if (g.id === "emptyGroup") {
                                opGroup = null;
                            }
                            break;
                        case "MachineType":
                            machineType = g.id.substring(1);
                            if (g.id === "emptyGroup") {
                                machineType = null;
                            }
                            break;
                        default:
                            moduleId = g.id.substring(1);
                            if (g.id === "emptyGroup") {
                                moduleId = null;
                            }
                    }
                    const node = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, n.id, opName,
                        opGroup, machineType, moduleId, opNext, n.Page, xG + "." + xN, yG + "." + yN, colorToSave);

                    nodeArray.push(node);
                }
            });
        });

        const config = {
            url: url,
            async: true,
            postData: JSON.stringify({ opdts: nodeArray, opmt: opmt })
        };
        AjaxPostCommon(config, (response) => {
            var result = response;

            if (result === true) {
                // Reload Opmt gridview
                let data = CreateObjStyleKeyCode(StyleCode, StyleSize, StyleColorSerial, RevNo);
                const opEdition = $("#drpOpsMasterEdition").val();
                data.edition = opEdition;
                //ReloadJqGrid(gridOpsTableName, data);
                ReloadJqGrid2LoCal(gridOpsTableName, data);
                // Clear remarks
                $scope.opmt.remarks = null;

                window.isChange = false;

                const msg = getMsgByLang(msgSave);
                MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);
            } else {
                const msg = getMsgByLang(msgError);
                ShowMessage(msg.title, msg.value, ObjMessageType.Error);
            }
        });

        $.unblockUI();
    }
    //END ADD) SON - 06/Mar/2019

    $scope.saveOp = (url, isSaveAsFom, isSaveAsNew) => {
        saveAsNewOp(url, isSaveAsFom, isSaveAsNew);

        ////START ADD) 06/Mar/2019: Save as new operation plan
        //if (isSaveAsNew) {
        //    let { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, Remarks, Factory, Language, OpCount } = GetSelectedOneRowData(gridOpsTableId);
        //    var countPro = countProcessesWithStandardName(Edition, Language, StyleCode, StyleSize, StyleColorSerial, RevNo, OpRevNo);
        //    if (countPro < OpCount) {
        //        ShowConfirmYesNoMessage("003", SmsFunction.Confirm, MessageType.Confirm, MessageContext.Confirm, function () {

        //            saveAsNewOp(url, isSaveAsFom, isSaveAsNew);

        //        }, function () { }, countPro);
        //    } else { //Copy Operation Plan normally
        //        saveAsNewOp(url, isSaveAsFom, isSaveAsNew);
        //    }
        //} else {
        //    saveAsNewOp(url, isSaveAsFom, isSaveAsNew);
        //}
        ////END ADD) 06/Mar/2019
    };

    $scope.confirmOpmt = () => {
        const opmt = GetSelectedOneRowData(gridOpsTableId);
        const config = {
            url: "/OpLayout/ConfirmOpmt",
            async: true,
            postData: JSON.stringify({ opmt: opmt })
        };
        AjaxPostCommon(config, (response) => {
            var result = response;

            if (result === true) {
                ReloadJqGrid2LoCal(gridOpsTableName, opmt);
                const msg = getMsgByLang(msgConfirmed);
                MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);
            } else {
                const msg = getMsgByLang(msgError);
                ShowMessage(msg.title, msg.value, ObjMessageType.Error);
            }
        });
    };
    //#endregion

    //#region Functions

    // #region Check User Role
    //START ADD - Son Nguyen Cao
    function CheckUserRole(edition, action) {
        var userRoleCheck;
        //Get user role.
        if (edition !== "M" || !$.isEmptyObject(edition)) {
            // The selected plan edition is PDM or OPS 
            if (edition === "O" || edition === "P") {
                //Return false if R&D role is null
                if ($scope.userRole.rdRole === null) return false;
                userRoleCheck = $scope.userRole.rdRole;
            } else if (edition === "A") {
                //Return false if R&D role is null
                if ($scope.userRole.facRole === null) return false;
                userRoleCheck = $scope.userRole.facRole;
            }
        } else {
            return false;
        }

        //If user role is not null then check action permission
        if (!$.isEmptyObject(userRoleCheck)) {
            if (action === AddRole) {
                return userRoleCheck.IsAdd === "1";
            } else if (action === UpdateRole) {
                return userRoleCheck.IsUpdate === "1";
            } else if (action === DeleteRole) {
                return userRoleCheck.IsDelete === "1";
            } else if (action === ConfirmRole) {
                return userRoleCheck.IsConfirm === "1";
            } else if (action === ExportRole) {
                return userRoleCheck.IsExport === "1";
            }
        }

        return false;
    }
    //END ADD - Son Nguyen Cao
    // #endregion

    //#region General functions
    window.OpenReadySearch();

    function modalInstance(initialObj, callBack) {
        var modal = $uibModal.open({
            animation: $scope.animationsEnabled,
            ariaLabelledBy: "modal-title",
            ariaDescribedBy: "modal-body",
            templateUrl: initialObj.templateUrl,
            controller: initialObj.controller,
            controllerAs: initialObj.controllerAs,
            windowClass: "modal-custom op__modal-change-page",
            appendTo: undefined,
            resolve: {
                modalData: function () {
                    return initialObj.modalData;
                }
            }
        });

        modal.result.then(function (data) {
            callBack(data);
        }, function () {
            $log.info("Modal dismissed at: " + new Date());
        });
    }

    $scope.showLoadLayoutPanel = () => {
        $("#panel-load-layout").toggle();
    };

    function processChangedColor(selector, color, prop, printClass) {
        selector.css(prop, color, "!important");
        let textNode = document.createTextNode(`.${printClass} { ${prop}: ${color} !important;}`);
        newStyle.appendChild(textNode);

        if (inlineMediaStyle !== null) {
            head.replaceChild(newStyle, inlineMediaStyle);
        }
        else {
            head.appendChild(newStyle);
        }
        inlineMediaStyle = newStyle;

        selector.addClass(`${printClass}`);
    }

    function changeColor(selector, color, prop, printClass) {
        if (color === "") {
            const msg = getMsgByLang(msgSelectColor);
            MsgInform(msg.title, msg.value, ObjMessageType.Error, true, false);
        } else {
            processChangedColor(selector, color, prop, printClass);
        }
    }

    function changeLanguage(language) {
        $scope.layoutLang.selectedLang = language;

        toolkit.eachNode((index, node) => {
            let opName;

            switch (language) {
                case "vn":
                    opName = node.data.VnOpName;
                    break;
                case "gb":
                    opName = node.data.GbOpName;
                    break;
                case "mm":
                    opName = node.data.MmOpName;
                    break;
                case "id":
                    opName = node.data.IdOpName;
                    break;
                case "et":
                    opName = node.data.EtOpName;
                    break;
                default:
                    opName = node.data.OpName ? node.data.OpName : "";
                    break;
            }

            //console.log(node.data);
            //console.log(opName);

            // Clearing icon
            //node.data.IconNameArr = [];
            node.data.name = `[${node.data.OpNum}] ${opName}`;
            toolkit.updateNode(node);
        });
    }

    $scope.selectLang = (lang) => {
        if (window.isChange) {
            const msg = getMsgByLang(msgLostData);
            const r = confirm(msg.value);
            if (r === true) {
                // Assign language for loading process name
                //selectedOpLanguage = lang;
                currentLang = lang;

                // Re-load layout by language
                //loadLayout(null, lang, $scope.opdtMode, $scope.layoutPage.pageNo);
                changeLanguage(lang);

                window.isChange = false;
            } else {
                return;
            }
        } else {
            // Re-load layout by language
            //loadLayout(null, lang, $scope.opdtMode, $scope.layoutPage.pageNo);
            changeLanguage(lang);

            // Assign language for loading process name
            //selectedOpLanguage = lang;
        }

        console.log($scope.layoutLang.selectedLang);
        console.log(lang);
    };

    //$scope.IconNames = [];

    function changeViewType(viewType) {
        console.log("Change view type.");
        //console.log($scope.IconUrl);

        toolkit.eachNode((index, node) => {
            let pText = "", tempIconNameArr = [], tempMainProcessArr = [], iconNameArr = [], opNum = "";

            //console.log(viewType);

            switch (viewType) {
                case "1":
                    changeLanguage($scope.layoutLang.selectedLang);
                    break;
                case "2":
                    if (node.data.OpNum) {
                        opNum = node.data.OpNum;
                    }
                    pText = `[${opNum}] [${node.data.Codes}]`;
                    node.data.IconNameArr = [];

                    if (node.data) {
                        if (node.data.MainProcessArr) {
                            tempMainProcessArr = node.data.MainProcessArr.split("-");
                            if (tempMainProcessArr && tempMainProcessArr.length > 0 && node.data.IconNames) {
                                tempIconNameArr = node.data.IconNames.split("-");

                                for (let i = 0; i < tempIconNameArr.length; i++) {
                                    if (tempMainProcessArr[i] === "1" && tempIconNameArr[i].trim() !== "_") iconNameArr.push(tempIconNameArr[i]);
                                }

                                node.data.IconUrl = $scope.IconUrl;
                            }
                        }
                        if (iconNameArr.length === 0) iconNameArr.push(DefaultIcon);
                        //console.log(iconNameArr);

                        node.data.IconNameArr = iconNameArr;
                    }
                    break;
                case "3":
                    if (node.data) {
                        if (node.data.MainProcessArr) {
                            tempMainProcessArr = node.data.MainProcessArr.split("-");
                            if (tempMainProcessArr && tempMainProcessArr.length > 0 && node.data.IconNames) {
                                tempIconNameArr = node.data.IconNames.split("-");

                                for (let i = 0; i < tempIconNameArr.length; i++) {
                                    if (tempMainProcessArr[i] === "1" && tempIconNameArr[i].trim() !== "_") iconNameArr.push(tempIconNameArr[i]);
                                }

                                node.data.IconUrl = $scope.IconUrl;
                            }
                        }
                        if (iconNameArr.length === 0) iconNameArr.push(DefaultIcon);
                        //console.log(iconNameArr);

                        node.data.IconNameArr = iconNameArr;
                    }
                    break;
                default:
                    changeLanguage($scope.layoutLang.selectedLang);
                    break;
            }

            //console.log(pText);
            node.data.name = pText;
            toolkit.updateNode(node);
        });
    }

    $scope.selectViewType = (vt) => {
        console.log(vt);

        $scope.layoutViewType.selectedViewType = vt;
        changeViewType(vt.value);
    };

    $scope.isCloneMode = false;
    $scope.changeToCloneMode = () => {
        const opmt = GetSelectedOneRowData(gridOpsTableId);
        if (opmt.ConfirmChk === "Y") {
            const msg = getMsgByLang(msgOpConfirmed);
            ShowMessage(msg.title, msg.value);
            return;
        }

        $scope.isCloneMode = !$scope.isCloneMode;

        if ($scope.isCloneMode) {
            $("node").css({ 'cursor': "url(../../../img/cursor/cursorCopy.cur), default" });
            $("#panel-colorpicker").hide();
            $("#cloneProcess").addClass("clone-mode");
        } else {
            $("#cloneProcess").removeClass("clone-mode");
            $("node").css({ 'cursor': "move" });
        }
    };
    //#endregion

    //#region Layout events
    $scope.opMode = [{ name: "ModuleType", title: "Load processes by Module type", id: "ModuleType" },
    { name: "OpGroup", title: "Load processes by Operation group", id: "OpGroup" },
    { name: "MachineType", title: "Load processes by Machine type", id: "MachineType" }];

    $scope.changeOpdtMode = function (newValue, oldValue) {
        if (window.isChange) {
            const msg = getMsgByLang(msgLostData);
            const r = confirm(msg.value);
            if (r === true) {
                const lang = $scope.layoutLang.selectedLang;
                loadLayout(null, lang, newValue, $scope.layoutPage.pageNo);
                window.window.isChange = false;
            } else {
                $scope.opdtMode = oldValue;
            }
        } else {
            const lang = $scope.layoutLang.selectedLang;
            loadLayout(null, lang, newValue, $scope.layoutPage.pageNo);
        }
    };

    $scope.previousPage = function (value) {
        if (value - 1 === 0) return;
        $scope.layoutPage.pageNo = value - 1;
        window.processPage = value - 1;

        loadLayout(null, $scope.layoutLang.selectedLang, $scope.opdtMode, $scope.layoutPage.pageNo);

        //currentMovedGroup.click = currentMovedGroupClick;
    };
    $scope.nextPage = function (value) {
        if (value === $scope.layoutPage.maxPage) return;
        $scope.layoutPage.pageNo = value + 1;
        window.processPage = value + 1;

        loadLayout(null, $scope.layoutLang.selectedLang, $scope.opdtMode, $scope.layoutPage.pageNo);
    };
    $scope.newPage = () => {
        const currentOpmt = GetSelectedOneRowData(gridOpsTableId);

        if (currentOpmt.ConfirmChk === "Y") {
            const msg = getMsgByLang(msgOpConfirmed);
            ShowMessage(msg.title, msg.value);
            return;
        } else {
            checkUserRole(currentOpmt.Edition, "Add", (result) => {
                if (result !== true) {
                    const msg = getMsgByLang(msgPermission);
                    ShowMessage(msg.title, msg.value);
                    return;
                } else {
                    toolkit.clear();
                    surface.refresh();
                    //jsPlumbGroup = [];
                    //toolkit.load({
                    //    data: {
                    //        "groups": [],
                    //        "nodes": [],
                    //        "edges": []
                    //    }
                    //});

                    const maxPage = $scope.layoutPage.maxPage;

                    $scope.layoutPage.maxPage = maxPage + 1;
                    $scope.layoutPage.pageNo = $scope.layoutPage.maxPage;
                }
            });
        }
    };

    $scope.showProcessModal = () => {
        $("#panel-process").toggle();
    };
    $scope.changeCanvasHeight = function (value) {
        if (value !== null && value !== undefined) {
            window.isChange = true;

            $scope.canvasStyle.height = value;
        } else {
            const msg = getMsgByLang(msgInvalid);
            ShowMessage(msg.title, msg.value, ObjMessageType.Error);
        }
    };

    $scope.changeFontSize = (value) => {
        if (value !== null && value !== undefined) {
            toolkit.eachNode((index, node) => {
                node.data.LayoutFontSize = value;
                toolkit.updateNode(node);
            });
        } else {
            const msg = getMsgByLang(msgInvalid);
            ShowMessage(msg.title, msg.value, ObjMessageType.Error);
        }
    };

    $scope.changeProcessHeight = (value) => {
        if (value !== null && value !== undefined) {
            toolkit.eachNode((index, node) => {
                node.data.ProcessHeight = value;
                toolkit.updateNode(node);
            });
        } else {
            const msg = getMsgByLang(msgInvalid);
            ShowMessage(msg.title, msg.value, ObjMessageType.Error);
        }
    };

    $scope.changeProcessWidth = (value) => {
        if (value !== null && value !== undefined) {
            toolkit.eachNode((index, node) => {
                node.data.ProcessWidth = value;
                toolkit.updateNode(node);
            });
        } else {
            const msg = getMsgByLang(msgInvalid);
            ShowMessage(msg.title, msg.value, ObjMessageType.Error);
        }
    };

    $scope.removeProcess = function (node) {
        const { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, ConfirmChk } = GetSelectedOneRowData(gridOpsTableId);
        const hasConfirmed = ConfirmChk !== "" ? true : false;
        if (!hasConfirmed) {
            checkUserRole(Edition, "Delete", (result) => {
                if (result !== true) {
                    const msg = getMsgByLang(msgPermission);
                    ShowMessage(msg.title, msg.value);
                    return;
                }
            });

            const smsg = getMsgByLang(msgConfirmDelete);

            ConfirmYesNo(smsg.title, smsg.value, () => {
                window.isChange = true;

                const opdt = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, node.id);

                //START ADD - Son Nguyen Cao
                //Update next process.
                if (toolkit.getNode(node.id)) {
                    const objConnection = toolkit.getNode(node.id).getEdges();
                    var lstOpdtNextOp = [];
                    $.each(objConnection, (index, value) => {
                        if (opdt.OpSerial !== value.source.id) {
                            var newOpdt = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, value.source.id);
                            lstOpdtNextOp.push(newOpdt);
                        }
                    });

                    let deleteProcess = $http.post("/OpLayout/DeleteProcess", { opdt: opdt });
                    deleteProcess.then(function (response) {
                        var result = response;

                        if (result) {
                            IsDeleteProcess = true; // to ignore function ClearAllVideoInfo1()

                            toolkit.removeNode(node);

                            // Removing empty group also
                            const grp = toolkit.getGroup(node.group);
                            if (grp) {
                                const pcs = grp.getNodes();
                                if (pcs && pcs.length === 0) toolkit.removeGroup(grp, true);
                            }

                            const msg = getMsgByLang(msgDeleted);
                            MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);

                            //START ADD - Son Nguyen Cao
                            //Update next process.                                                        
                            let updateNextOp = $http.post("/OpLayout/UpdateNextOp", { lstOpdt: lstOpdtNextOp });
                            updateNextOp.then(function (response) {
                                if (response.data === Success) {
                                    LayoutEvent = true;
                                    //Reload operation master gird
                                    var postData = {
                                        styleCode: opdt.StyleCode, styleSize: opdt.StyleSize, styleColor: opdt.StyleColorSerial
                                        , revNo: opdt.RevNo, edition: $("#drpOpsMasterEdition").val()
                                    };
                                    //ReloadJqGrid(gridOpsTableName, postData);
                                    ReloadJqGrid2LoCal(gridOpsTableName, postData);
                                    //ShowMessage("Update next opeartion plan", "Cannot update next operation plan.", MessageTypeAlert);
                                }
                            });

                            //toolkit.removeNode(node);
                            //MsgInform("Inform", "The process is deleted successfully.", ObjMessageType.Info, false, true);

                            //END ADD - Son Nguyen Cao
                        } else {
                            const msg = getMsgByLang(msgError);
                            MsgInform(msg.title, msg.value, ObjMessageType.Error, false, false);
                        }
                    }, function (error) {
                        ShowAjaxError(error, "/OpLayout/DeleteProcess");
                    });
                }
                //END ADD - Son Nguyen Cao.
            });
        } else {
            const msg = getMsgByLang(msgOpConfirmed);
            ShowMessage(msg.title, msg.value);
        }
    };

    //START ADD - SON
    $scope.showPrintingLine = false;

    $scope.showVerticalLine = function () {
        $scope.showPrintingLine = !$scope.showPrintingLine;
    };

    $scope.playVideoProcess = (node) => {
        const selOpRow = GetSelectedOneRowData(gridOpsTableId),
            opdt = {
                StyleCode: selOpRow.StyleCode,
                StyleSize: selOpRow.StyleSize,
                StyleColorSerial: selOpRow.StyleColorSerial,
                RevNo: selOpRow.RevNo,
                OpRevNo: selOpRow.OpRevNo,
                OpSerial: node.id,
                Edition: selOpRow.Edition
            };

        $http.post("/OpLayout/GetVideos", { opdt }).then((res) => {
            console.log(res.data.Result);

            if ($.isEmptyObject(res.data.Result.mainOpdt)) {
                alert("This process does not have video.");
            } else {
                PlayOpVideo(res.data.Result);
            }
        });
    };
    //END ADD - SON

    $scope.changeNodePage = function ($event, node) {
        const isShowBtnUpdateModule = $scope.opdtMode === "ModuleType" ? false : true,
            modalConfig = {
                templateUrl: "changePageNoModalContent.html",
                controller: "ChangeNodePageModalCtrl",
                controllerAs: "$ctrl",
                modalData: { maxPage: $scope.layoutPage.maxPage, modules: $scope.modules, isShowBtnUpdateModule }
            };
        modalInstance(modalConfig, (modalData) => {
            let { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo } = GetSelectedOneRowData(gridOpsTableId);
            const n = toolkit.getNode(node.id),
                opdt = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, n.id, n.data.OpName,
                    n.data.OpGroup, n.data.MachineType, n.data.ModuleId, null, n.data.Page),
                opdts = [];

            // If there are selected page, updating page
            if (modalData.pageNo) {
                opdt.Page = modalData.pageNo;
                opdts.push(opdt);

                const cf = new AjaxConfig("/OpLayout/UpdatePage", true, JSON.stringify({ opdts }));
                AjaxPostCommon(cf,
                    (res) => {
                        if (res) {
                            toolkit.removeNode(node);
                        } else {
                            console.log(res);
                        }
                    });
            } else {
                // If there are selected module, updating module
                if (modalData.selectedModule) {
                    console.log("updating module");

                    opdt.ModuleId = modalData.selectedModule.ModuleId;
                    opdts.push(opdt);

                    const cf = new AjaxConfig("/OpLayout/UpdateModule", true, JSON.stringify({ opdts }));

                    if ($scope.opdtMode === "ModuleType") {
                        const grp = toolkit.getGroup(`g${opdt.ModuleId}`);
                        if (grp) {
                            AjaxPostCommon(cf, (res) => {
                                if (res) {
                                    n.ModuleId = modalData.selectedModule.ModuleId;
                                    n.Title = modalData.selectedModule.ModuleName;
                                    n.group = `g${opdt.ModuleId}`;
                                    toolkit.updateNode(n);

                                    //const grp = toolkit.getGroup(n.group), pcs = grp.getNodes();
                                    //if (pcs && pcs.length === 0) {
                                    //    console.log("There are no process anymore.");
                                    //}
                                    //surface.repaintEverything();
                                    //surface.getJsPlumb().repaintEverything();
                                    //surface.refresh();
                                } else {
                                    console.log(res);
                                }
                            });
                            console.log("Already existed");
                        } else {
                            MsgInform("Alert", "Please insert the module group ahead.", "alert", true, true);
                        }
                    } else {
                        AjaxPostCommon(cf, (res) => {
                            if (res) {
                                node.ModuleId = modalData.selectedModule.ModuleId;
                                toolkit.updateNode(node);
                            } else {
                                console.log(res);
                            }
                        });
                    }

                    surface.refresh();
                } else {
                    console.log("Nothing changes");
                }
            }
        });
    };

    $scope.toggleGroup = function ($event, group) {
        const grp = toolkit.getGroup(group.id), pcs = grp.getNodes(), el = $($event.currentTarget);
        window.isChange = true;

        el.text().trim() === el.data("text-swap") ? el.text(el.data("text-original")) : el.text(el.data("text-swap"));

        if (!group.isArrange) {
            $scope.setNodePos(pcs);
            group.isArrange = true;
        } else {
            $scope.summonNodePos(pcs);
            group.isArrange = false;
        }
        surface.repaintEverything();
        surface.getJsPlumb().repaintEverything();
        surface.refresh();
    };

    $scope.isVertical = false;

    $scope.setNodePos = function (nodes) {
        const isVertical = $scope.isVertical;

        var t = dfNodeTop;
        var l = dfNodeLeft;
        var n = $scope.pLayoutModifier.noProcessInRow;
        var j = 0;
        const horizontalDistance = $scope.pLayoutModifier.pHeight + 2 + $scope.pLayoutModifier.pHorizontalDistance; // 2 is border
        const verticalDistance = $scope.pLayoutModifier.pWidth + 2 + $scope.pLayoutModifier.pVerticalDistance;

        if (isVertical) {
            $(nodes).each(function (index, value) {
                if (value.data.IsDisplay) {
                    if (n === 1) {
                        // Increase top if second node up to
                        if (index > 0) {
                            t += horizontalDistance;
                        }
                        surface.setPosition(value.data.id, l, t);
                        toolkit.updateNode(value.data.id, { left: l, top: t });
                    } else {
                        surface.setPosition(value.data.id, l, t);
                        toolkit.updateNode(value.data.id, { left: l, top: t });

                        j += 1;

                        // Increase left till number of process is expected then increase top
                        if (j === n) {
                            l = dfNodeLeft;
                            t += horizontalDistance;
                            j = 0;
                        } else {
                            l += verticalDistance;
                        }
                    }
                }
                //jsPlumb.revalidate(value.data.id);
            });
        } else {
            $(nodes).each(function (index, value) {
                if (value.data.IsDisplay) {
                    if (n === 1) {
                        // Increase top if second node up to
                        if (index > 0) {
                            l += verticalDistance;
                        }
                        surface.setPosition(value.data.id, l, t);
                        toolkit.updateNode(value.data.id, { left: l, top: t });
                        jsPlumb.repaintEverything();
                    } else {
                        surface.setPosition(value.data.id, l, t);
                        toolkit.updateNode(value.data.id, { left: l, top: t });
                        jsPlumb.repaintEverything();
                        j += 1;

                        // Increase top till number of process is expected then increase left
                        if (j === n) {
                            t = dfNodeTop;
                            l += verticalDistance;
                            j = 0;
                        } else {
                            t += horizontalDistance;
                        }
                    }
                }
            });
        }
    };
    $scope.summonNodePos = function (nodes) {
        var t = dfNodeTop;
        var l = dfNodeLeft;
        $(nodes).each(function (index, value) {
            if (value.data.IsDisplay) {
                if (index > 0) {
                    t += 2;
                    l += 2;
                }
                surface.setPosition(value.data.id, l, t);
                toolkit.updateNode(value.data.id, { left: l, top: t });
            }
        });
    };
    $scope.showHideConnection = (value, isIcon) => {
        if (isIcon) $scope.isShowFlow = !value;
        $(".jtk-endpoint-connected").remove();
        if ($scope.isShowFlow) {
            $(".jtk-connector").show();
            const currentOpmt = GetSelectedOneRowData(gridOpsTableId);
            if (currentOpmt.ConfirmChk === "Y") {
                console.log("This operation plan has already been confirmed.");
            } else {
                // Check user permission to enable delete action of process
                checkUserRole("M", "Update", (result) => {
                    if (result === true) {
                        checkUserRole("M", "Delete", (rs) => {
                            if (rs) {
                                $(".delete-connection").show();
                            } else {
                                console.log("No right to show delete icon.");
                            }
                        });
                    } else {
                        console.log("No right to show delete icon.");
                    }
                });
            }
        } else {
            $(".jtk-connector").hide();
            $(".delete-connection").hide();
        }
    };

    // show/hide groups
    $scope.showHideGroups = (v, isIcon) => {
        if (isIcon) $scope.isShowGroups = !v;
        toggleGroups($scope.isShowGroups);
    };

    $scope.zoomToFit = function () {
        toolkit.clearSelection();
        surface.zoomToFit();
    };

    $scope.selectOpNameRadio = function (value) {
        switch (value) {
            case "1":
                $scope.rdOpName = "1";
                toolkit.eachNode((index, node) => {
                    node.data.name = node.data.Remarks !== "" ? `[${node.data.OpNum}] ${node.data.OpName} (${node.data.Remarks})` :
                        `[${node.data.OpNum}] ${node.data.OpName}`;

                    toolkit.updateNode(node);
                });
                break;
            case "2":
                $scope.rdOpName = "2";
                toolkit.eachNode((index, node) => {
                    node.data.name = `[${node.data.OpNum}] ${node.data.OpName}`;
                    toolkit.updateNode(node);
                });
                break;
            case "3":
                $scope.rdOpName = "3";
                toolkit.eachNode((index, node) => {
                    node.data.name = `[${node.data.OpNum}] ${node.data.Remarks}`;
                    toolkit.updateNode(node);
                });
                break;
            default:
                break;
        }
    };

    $scope.applyStandardColor = () => {
        window.isChange = true;

        for (let n of toolkit.getNodes()) {
            const sc = $scope.opsColors.find(x => x.opgroup === n.data.OpGroup);
            if (sc) {
                //console.log(sc);

                n.data.DisplayColor = sc.value;
                toolkit.updateNode(n);
            } else {
                n.data.DisplayColor = DefaultDisplayColor;
                toolkit.updateNode(n);
            }
        }
    };
    //#endregion

    //#region Color picker
    $scope.selectColor = (color) => {
        $scope.opsColor.hexPicker = color;
        $scope.showColorList = false;
    };
    $scope.colorIndexChanged = (color) => {
        $scope.opsColor.selectedGroup = color;
        $scope.showSgrCp = false;

        changeBgcNodes(color);
    };

    $scope.showColorPicker = () => {
        $("#panel-colorpicker").toggle();
        const isVisible = $("#panel-colorpicker").is(":visible");
        if (isVisible) {
            $scope.isCloneMode = false;

            $("group").css({ 'cursor': "url(../../../img/cursor/redpntbrsh.cur), default" });
            $("node").css({ 'cursor': "url(../../../img/cursor/bluepntbrsh.cur), default" });

            $("#cloneProcess").removeClass("clone-mode");
        } else {
            $("group").css({ 'cursor': "pointer" });
            $("node").css({ 'cursor': "move" });
        }
    };

    $scope.changeColorNodeText = (color) => {
        changeColor($(".jtk-node .name span"), color, "color", "node-text-print");
    };

    $scope.colorNodeTextChanged = (color) => {
        $scope.opsColor.processText = color;
        $scope.showNodeTextCp = false;

        changeColor($(".jtk-node .name span"), color, "color", "node-text-print");
    };

    $scope.changeColorGroupText = (color) => {
        changeColor($("group .group-title"), color, "color", "group-text-print");
    };

    $scope.colorGroupTextChanged = (color) => {
        $scope.opsColor.groupText = color;
        $scope.showGroupTextCp = false;

        changeColor($("group .group-title"), color, "color", "group-text-print");
    };

    function changeFlowColor(color) {
        let isDisplayColorpicker = $("#panel-colorpicker").is(":visible");

        if (isDisplayColorpicker && color !== "") {
            let conns = surface.getJsPlumb().getAllConnections();
            for (let c of conns) {
                c.setPaintStyle({
                    stroke: `${color}`,
                    strokeWidth: 1
                }, true);
            }

            surface.getJsPlumb().repaintEverything();
        }
    }

    $scope.changeColorFlow = (color) => {
        changeFlowColor(color);
    };

    $scope.colorFlowChanged = (color) => {
        $scope.opsColor.flow = color;
        $scope.showFlowCp = false;

        changeFlowColor(color);
    };

    $scope.changeColorAllNodes = color => {
        $(".ops-node").css("font-size", $scope.opsFont.fontSize);
        changeColor($(".ops-node"), color, "background", "node-all-print");
    };

    function changeBgcNodes(color) {
        let selection = toolkit.getSelection();
        let groups = selection.getGroups();

        if (color === "") {
            const msg = getMsgByLang(msgSelectColor);
            MsgInform(msg.title, msg.value, ObjMessageType.Error, true, false);
        } else {
            if (!Number.isNaN(groups.length) && groups.length !== 0) {
                selection.eachGroup(function (index, value) {
                    let grElement = $(`group[data-jtk-group-id='${value.id}']`);
                    grElement.find(".group-title").css("background", color);

                    processChangedColor(grElement, color, "background", `node-bygroup-print-${value.id}`);
                    let nodes = value.getNodes();
                    nodes.forEach(function (item) {
                        item.data.DisplayColor = color;
                        toolkit.updateNode(item);
                    });
                });
            }
        }
    }

    $scope.changeColorNodesByGroup = function (color) {
        changeBgcNodes(color);
    };
    //#endregion

    $scope.showDeleteFlow = (isShowDeleteFlow) => {
        $scope.isShowDeleteFlow = !isShowDeleteFlow;

        const opsMaster = GetSelectedOneRowData(gridOpsTableId);
        // Check user permission to enable delete action of process
        checkUserRole(opsMaster.Edition, "Update", (result) => {
            if (result === true) {
                checkUserRole(opsMaster.Edition, "Delete", (rs) => {
                    if (rs) {
                        toggleDeleteFlowIcon($scope.isShowDeleteFlow);
                    } else {
                        const msg = getMsgByLang(msgPermission);
                        ShowMessage(msg.title, msg.value);
                    }
                });
            } else {
                const msg = getMsgByLang(msgPermission);
                ShowMessage(msg.title, msg.value);
            }
        });
    };

    //$scope.showProcessVideoModal = () => {
    //    const currentOpmt = GetSelectedOneRowData(gridOpsTableId);
    //    if (currentOpmt.ConfirmChk === "Y") {
    //        const msg = getMsgByLang(msgOpConfirmed);
    //        ShowMessage(msg.title, msg.value);
    //    } else {
    //        const isOpmtEmpty = !Object.keys(currentOpmt).length;

    //        if (!isOpmtEmpty) {
    //            checkUserRole(currentOpmt.Edition, "Add", (result) => {
    //                if (result !== true) {
    //                    const msg = getMsgByLang(msgPermission);
    //                    ShowMessage(msg.title, msg.value);
    //                    return;
    //                } else {
    //                    if (HideOpVideoModal) {
    //                        $('#vdpModal').show();
    //                    } else {
    //                        $('#vdpModal').modal('show');
    //                    }
    //                }
    //            });
    //        }
    //    }
    //};

    $scope.FixedModal = {
        IsShow: false
    };

    $scope.showProcessVideoModal = () => {
        const currentOpmt = GetSelectedOneRowData(gridOpsTableId);
        if (currentOpmt.ConfirmChk === "Y") {
            const msg = getMsgByLang(msgOpConfirmed);
            ShowMessage(msg.title, msg.value);
        } else {
            const isOpmtEmpty = !Object.keys(currentOpmt).length;

            if (!isOpmtEmpty) {
                checkUserRole(currentOpmt.Edition, "Add", (result) => {
                    if (result !== true) {
                        const msg = getMsgByLang(msgPermission);
                        ShowMessage(msg.title, msg.value);
                        return;
                    } else {
                        //_FixedVpModalCtr.style.display = _FixedVpModalCtr.style.display === "none" ? "block" : "none";
                        //console.log(JqModalFixedVp);
                        //$("#modalFixedVp").show("slide", { direction: "bottom" }, 1000);
                        if ($scope.FixedModal.IsShow) {
                            //$("#modalFixedVp").hide(1000);
                            $("#modalFixedVp").slideUp("slow");
                        } else {
                            $("#modalFixedVp").slideDown("slow");
                        }
                        $scope.FixedModal.IsShow = !$scope.FixedModal.IsShow;
                    }
                });
            }
        }
    };

    $scope.CloseFixedVpModal = () => {
        if ($scope.FixedModal.IsShow) {
            //$("#modalFixedVp").hide(1000);
            $("#modalFixedVp").slideUp("slow");
        } else {
            $("#modalFixedVp").slideDown("slow");
        }
        $scope.FixedModal.IsShow = !$scope.FixedModal.IsShow;
    };

    $scope.CloseFixedVpModal1 = () => {
        //$("#modalFixedVp1").slideUp("slow");
        $("#modalFixedVp2").slideUp("slow");
        $scope.FixedModal.IsShow = !$scope.FixedModal.IsShow;
    };

    $scope.MinimizeVideomodal = () => {
        $("#modalFixedVp2").slideUp("slow");

        setTimeout(() => {
            $("#modalFixedVp").slideDown("slow");
        }, 500);
        //$("#modalFixedVp").slideDown("slow");
    };

    $scope.closeRightShortListVideo = () => {
        //document.getElementById(DivRightVideoList).style.display = "none";
        $("#divRightVideoList").hide("slide", { direction: "right" }, 1000);
        $("#vdpModal").show("slide", { direction: "left" }, 1000);
    };

    $scope.clickThumbnailVideo = (v) => {
        console.log("clicked a thumbnail");
        //console.log(document.getElementById("vdProcess1").src);

        $scope.SplitVideoUrl = document.getElementById("vdProcess1").src;

        console.log(v);

        //const vdpPreview = document.getElementById("vdpPreview");
        //vdpPreview.onloadeddata = () => {
        //    console.log("video is loaded");
        //};

        $scope.selectedVideo = v;
    };

    $scope.saveProcessVideo = () => {
        console.log("About to split video...");
        SaveProcessVideo1();
    };

    //#endregion
});

app.controller("GroupModalCtrl", function ($uibModalInstance, items) {
    var $ctrl = this;
    $ctrl.items = items;
    $ctrl.selectedGroup = $ctrl.items[0];

    $ctrl.ok = function () {
        const id = $ctrl.selectedGroup.SubCode;
        $uibModalInstance.close({ id: `g${id}`, title: $ctrl.selectedGroup.CodeName });
    };

    $ctrl.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("NodeModalCtrl", function ($scope, $http, $uibModalInstance, sendData) {
    var $ctrlNode = this;
    $ctrlNode.postConfig = function (url, data) {
        var config = {
            url: url,
            method: "POST",
            data: data,
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        };
        return config;
    };

    //$ctrlNode.node = sendData;
    $ctrlNode.opLongName = "";
    $ctrlNode.maxOpSerial = 0;
    $ctrlNode.opNameArr = [];
    $ctrlNode.selectedOpName = [];
    $ctrlNode.opTypeArr = [];
    $ctrlNode.opToolCat = [];
    $ctrlNode.selectedOpToolCat = [];
    $ctrlNode.opToolArr = [];
    $ctrlNode.selectedOpMachineCat = [];
    $ctrlNode.opMachineCat = [];
    $ctrlNode.hotSpot = false;
    $ctrlNode.fileInfo = {
        name: "",
        size: "",
        type: ""
    };
    $ctrlNode.processImage = {
        src: "../img/no-image.png",
        file: $ctrlNode.fileInfo
    };
    $ctrlNode.processVideo = {
        src: "",
        poster: "../img/no-video.png",
        file: $ctrlNode.fileInfo
    };
    $ctrlNode.machines = [];
    $ctrlNode.tools = [];

    var opdt = GetOpmtFromLocalStorage();
    var getOpSerialConfig = {
        async: true,
        url: "/OpLayout/GetMaxOpSerial",
        postData: JSON.stringify({ opdt: opdt })
    };

    // Initilize Process object
    $ctrlNode.opsProcess = {
        OpSerial: 0,
        OpName: null,
        RefOpName: null,
        JobType: null,
        MachineType: null,
        ToolId: null,
        MachineCount: 1,
        ManCount: 1,
        MaxTime: null,
        OpTime: null,
        BenchmarkTime: null,
        OpPrice: null,
        OfferOpPrice: null,
        OpGroup: null,
        OpsState: null,
        MdCode: null,
        HotSpot: "0",
        ImageName: null,
        VideoFile: null,
        Remarks: null
    };
    AjaxPostCommon(getOpSerialConfig, function (response) {
        $ctrlNode.opsProcess.OpSerial = response;
    });

    // Because the list of operation name too big, 
    // need to wait for rendering already then bind data to dropdown list
    $uibModalInstance.rendered.then(function () {
        setTimeout(function () {
            $ctrlNode.opNameArr = window.opNameList;
        }, 1000);

        // Get list of Action Process
        if (window.opTypeArr === undefined || window.opTypeArr === null) {
            GetMasterCodes("/OpLayout/GetMasterCodes", OpType, function (list) {
                $ctrlNode.opTypeArr = window.opTypeArr = list;
            });
        } else {
            $ctrlNode.opTypeArr = window.opTypeArr;
        }

        // Get Categories of Operation Machine
        if (window.opMachineCat === undefined || window.opMachineCat === null) {
            var opMachineCatConfig = {
                async: true,
                url: "/OpLayout/GetMachineCategories",
                postData: JSON.stringify({ masterCode: "", subCode: "", codeDesc: MachineCodeDesc })
            };
            AjaxPostCommon(opMachineCatConfig, function (response) {
                $ctrlNode.opMachineCat = response;
            });
        } else {
            $ctrlNode.opMachineCat = window.opMachineCat;
        }

        // Get Categories of Operation Tool
        if (window.opToolCat === undefined || window.opToolCat === null) {
            GetMasterCodes("/OpLayout/GetMasterCodes", OpTool, function (list) {
                $ctrlNode.opToolCat = window.opToolCat = list;
            });
        } else {
            $ctrlNode.opToolCat = window.opToolCat;
        }

        // Get list of Process Group
        if (window.processGroupArr === undefined || window.processGroupArr === null) {
            GetMasterCodes("/OpLayout/GetMasterCodes", OpGroup, function (list) {
                $ctrlNode.processGroupArr = window.processGroupArr = list;
            });
        } else {
            $ctrlNode.processGroupArr = window.processGroupArr;
        }

        // Get list of Outsource Group
        if (window.outsourceGroupArr === undefined || window.outsourceGroupArr === null) {
            GetMasterCodes("/OpLayout/GetMasterCodes", OpGroup, function (list) {
                $ctrlNode.outsourceGroupArr = window.outsourceGroupArr = list;
            });
        } else {
            $ctrlNode.outsourceGroupArr = window.outsourceGroupArr;
        }

        // Watch dropdown openning event
        var drOpMachineCat = "#drOpMachineCat ul";
        var drOpToolCat = "#drOpToolCat ul";
        var drOpName = "#drOpName ul";
        $scope.$watch(function () {
            return angular.element(drOpMachineCat).is(":visible");
        },
            function (newVal) {
                if (!newVal) {
                    var categoryArr = $ctrlNode.selectedOpMachineCat;
                    if (categoryArr.length !== 0) {
                        var opToolMachineConfig = {
                            async: true,
                            url: "/OpLayout/GetOpMachineMasters",
                            postData: JSON.stringify({ isTool: false, categoryIds: categoryArr })
                        };
                        AjaxPostCommon(opToolMachineConfig, function (response) {
                            $ctrlNode.opMachineArr = response;
                        });
                    } else {
                        $ctrlNode.opMachineArr = [];
                    }
                }
            });
        $scope.$watch(function () {
            return angular.element(drOpToolCat).is(":visible");
        },
            function (newVal) {
                if (!newVal) {
                    var categoryArr = $ctrlNode.selectedOpToolCat;

                    if (categoryArr.length !== 0) {
                        var opToolCatConfig = {
                            async: true,
                            url: "/OpLayout/GetOpMachineMasters",
                            postData: JSON.stringify({ isTool: true, categoryIds: categoryArr })
                        };
                        AjaxPostCommon(opToolCatConfig, function (response) {
                            $ctrlNode.opToolArr = response;
                        });
                    } else {
                        $ctrlNode.opToolArr = [];
                    }
                }
            });
        $scope.$watch(function () {
            return angular.element(drOpName).is(":visible");
        },
            function (newVal) {
                if (!newVal) {
                    $ctrlNode.opsProcess.OpName = "";
                    $ctrlNode.opsProcess.RefOpName = "";
                    for (let j = 0; j < $ctrlNode.selectedOpName.length; j++) {
                        $ctrlNode.opsProcess.OpName += $ctrlNode.selectedOpName[j].OpName;
                        $ctrlNode.opsProcess.RefOpName += $ctrlNode.selectedOpName[j].OpNameId;

                        if (j < $ctrlNode.selectedOpName.length - 1) {
                            $ctrlNode.opsProcess.OpName += "|";
                            $ctrlNode.opsProcess.RefOpName += ",";
                        }
                    }
                }
            });
    });

    $ctrlNode.changeHotSpot = function () {
        $ctrlNode.opsProcess = $ctrlNode.hotSpot ? "1" : "0";
    };
    $ctrlNode.removeFile = function (fileType) {
        opdt.OpSerial = $ctrlNode.opsProcess.OpSerial;
        var dataToDel = { fileName: "", fileType: "", opdt: opdt };

        switch (fileType) {
            case "image":
                if ($ctrlNode.opsProcess.ImageName !== null) {
                    dataToDel.fileName = $ctrlNode.opsProcess.ImageName;
                    dataToDel.fileType = "Image";
                    $http.post("/OpLayout/DeleteProcessFile", dataToDel).then(function (response) {
                        if (response.data === "fail") {
                            ShowMessage("Error", "Could not delete the image file.", ObjMessageType.Error);
                        } else {
                            $ctrlNode.processImage = {
                                src: "../img/no-image.png",
                                file: $ctrlNode.fileInfo
                            };

                            $ctrlNode.opsProcess.ImageName = null;
                            ShowMessage("Inform", "The image is deleted.", ObjMessageType.Info);
                        }
                    }, function (error) {
                        ShowAjaxError(error, "/OpLayout/DeleteProcessFile");
                    });
                }

                break;
            case "video":
                if ($ctrlNode.opsProcess.VideoFile !== null) {
                    dataToDel.fileName = $ctrlNode.opsProcess.VideoFile;
                    dataToDel.fileType = "Video";
                    $http.post("/OpLayout/DeleteProcessFile", dataToDel).then(function (response) {
                        if (response.data === "fail") {
                            ShowMessage("Error", "Could not delete the video file.", ObjMessageType.Error);
                        } else {
                            $ctrlNode.processVideo = {
                                src: "../img/no-video.png",
                                poster: "../img/no-video.png",
                                file: $ctrlNode.fileInfo
                            };
                            $ctrlNode.opsProcess.VideoFile = null;
                            ShowMessage("Inform", "The video is deleted.", ObjMessageType.Info);
                        }
                    }, function (error) {
                        window.ShowAjaxError(error, "/OpLayout/DeleteProcessFile");
                    });
                }
                break;
        }
    };
    $ctrlNode.submitProcess = function () {
        let process = Object.assign($ctrlNode.opsProcess, opdt);
        let machines = $ctrlNode.machines;
        let tools = $ctrlNode.tools;

        process.X = sendData.left;
        process.Y = sendData.top;
        process.Page = window.processPage;

        let postData = { opdt: process, machines: machines, tools: tools };

        // Add new process into database
        let addPost = $http.post("/OpLayout/AddNewProcess", JSON.stringify(postData));
        addPost.then(function (response) {
            if (response.data.process !== null && response.data.process !== undefined) {
                sendData.id = response.data.process.OpSerial.toString();
                sendData.name = `[${response.data.process.OpSerial}] ${response.data.process.OpName}`;
                sendData.OpName = response.data.process.OpName;
                sendData.OpGroup = response.data.process.OpGroup;
                sendData.MachineType = response.data.process.MachineType;

                $uibModalInstance.close(sendData);
            } else {
                ShowMessage("Error", response.data.error, ObjMessageType.Error);
            }
        }, function (error) {
            ShowAjaxError(error, "/OpLayout/AddNewProcess");
        });
    };

    $ctrlNode.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

app.controller("ChangeNodePageModalCtrl", function ($scope, $uibModalInstance, modalData) {
    var $ctrl = this;

    // Assigning data to the scope.
    $ctrl.processPage = {
        maxPage: modalData.maxPage,
        pageNo: 1
    };
    $ctrl.modules = modalData.modules;
    $ctrl.isShowBtnUpdateModule = modalData.isShowBtnUpdateModule;

    $ctrl.updatePage = () => {
        if ($ctrl.processPage.pageNo !== window.processPage) {
            $uibModalInstance.close({ pageNo: $ctrl.processPage.pageNo });
        }
    };
    $ctrl.updateModule = () => {
        if ($ctrl.selectedModule && $ctrl.selectedModule !== "") {
            $uibModalInstance.close({ selectedModule: $ctrl.selectedModule });
        }
    };
    $ctrl.cancel = () => {
        $uibModalInstance.dismiss("Cancel modal changing node page.");
    };
});

//#endregion

//#region Classes

// Operation plan color
class OpColor {
    constructor(name, value, opgroup, module) {
        this.name = name;
        this.value = value;
        this.opgroup = opgroup;
        this.module = module;
    }
}

// Operation plan language
class OpLanguage {
    constructor(name, value, imgSrc) {
        this.name = name;
        this.value = value;
        this.imgSrc = imgSrc;
    }
}

// Operation plan view type
class OpViewType {
    constructor(name, value, imgSrc) {
        this.name = name;
        this.value = value;
        this.imgSrc = imgSrc;
    }
}

class LayoutProcess {
    constructor(id, name, opTime, machineCount, machineName, manCount, opName, vnOpName, gbOpName, mmOpName, idOpName,
        etOpName, codes, iconNames, mainProcessArr, hotSpot, opsState, opNum, remarks, opGroup, machineType, moduleId,
        displayColor, processWidth, processHeight, layoutFontSize, tools, isDisplay, page, canDelete, showButtonPlayVideo,
        left, top, group, title, iconNameArr, iconUrl, iconName) {
        this.id = id;
        this.name = name;
        this.OpTime = opTime;
        this.MachineCount = machineCount;
        this.MachineName = machineName;
        this.ManCount = manCount;
        this.OpName = opName;
        this.VnOpName = vnOpName;
        this.GbOpName = gbOpName;
        this.MmOpName = mmOpName;
        this.IdOpName = idOpName;
        this.EtOpName = etOpName;
        this.Codes = codes;
        this.IconNames = iconNames;
        this.MainProcessArr = mainProcessArr;
        this.HotSpot = hotSpot;
        this.OpsState = opsState;
        this.OpNum = opNum;
        this.Remarks = remarks;
        this.OpGroup = opGroup;
        this.MachineType = machineType;
        this.ModuleId = moduleId;
        this.DisplayColor = displayColor;
        this.ProcessWidth = processWidth;
        this.ProcessHeight = processHeight;
        this.LayoutFontSize = layoutFontSize;
        this.Tools = tools;
        this.IsDisplay = isDisplay;
        this.Page = page;
        this.CanDelete = canDelete;
        this.ShowButtonPlayVideo = showButtonPlayVideo;
        this.left = left;
        this.top = top;
        this.group = group;
        this.Title = title;
        this.IconNameArr = iconNameArr;
        this.IconUrl = iconUrl;
        this.IconName = iconName;
    }
}
//#endregion