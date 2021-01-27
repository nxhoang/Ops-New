//#region Variables
"use strict";

var opsGroups = [], opsNodes = [], opsEdges = [], CurrentOpmt = {}, CurrentStyle = {}, CurrentMesPackage = {},
    opsData = {
        "groups": opsGroups,
        "nodes": opsNodes,
        "edges": opsEdges
    }, MesProcesses = [], ProcessGroups = [];
var jsPlumbGroup, defaultGroupMode = "OpGroup", OpsLanguage, currentLang, selectedOpLanguage, opNameList = [],
    defaultProcessHeight = 190, defaultProcessWidth = 120, defaultCanvasHeight = 650, defaultFontSize = 13, dfNodeTop = 24,
    dfNodeLeft = 2, dfGroupTop = 60, dfGroupLeft = 20, systemLang = $("#flagSelected").attr("value");
var msgInformData, msgLostData, msgPermission, msgNoGroup, msgSelectOpFirstly, msgExistGroup, msgSelectColor, msgError,
    msgSave, msgConfirmDelete, msgDeleted, msgInvalid, msgLimitedLength, msgAcceptLineFlow, msgConfirmed, msgSelectFactory,
    msgOpConfirmed, enableRefreshAT = true, CurrentGroupMode = defaultGroupMode, RefreshTime = 900000,
    hasRefreshCanvas = false, moduleList = [];
const BtnCreateOpmt = "btnCreateOpmt", BtnDeleteOpmt = "btnDeleteOpmt", GetOpdtsUri = `/OpsLayout/GetOpdts`,
    GetMpdtUri = `/OpsLayout/GetMpdtByMxPackage`, ProcessModal = "modalProcess", ProcessModalId = `#${ProcessModal}`;
var mesOpApp = angular.module("opsApp", ["$jsPlumb", "ngAnimate", "ngSanitize", "ui.bootstrap", "ui.router", "ui",
    "long2know", "colorpicker.module"]);
//#endregion

//#region Javascript functions
function changeModalHeaderBgColor() {
    const layoutTabHeader = document.getElementById("layoutTabHeader");

    // Changing background color for process detail modal by tab background color
    if (layoutTabHeader.currentStyle) {
        console.log(layoutTabHeader.currentStyle["backgroundColor"]);
    } else {
        const thbgcl = document.defaultView.getComputedStyle(layoutTabHeader, null)["backgroundColor"];
        console.log(thbgcl);
        document.getElementById("opdtModalHeader").style.backgroundColor = thbgcl;
    }
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
    console.log("BeforeSelectRowGridOpMaster");

    const msg = getMsgByLang(msgLostData);
    const r = confirm(msg.value);

    return r;
}

function FunctionCallBackSavingProcess(addStatus, objOpdt) {
    if (addStatus) {
        //callback(objOpdt);
    }
}

function CreateGridOpsDetail() { }

function getOpNames(lang) {
    let url = "/OpsLayout/GetOpName";

    // Get list of Operation name
    let getOpName = $.post(url, { languageId: lang });
    getOpName.then(function (response) {
        if (response.opNames !== null && response.opNames !== undefined) {
            window.opNameList = response.opNames;
        } else {
            console.log(response.error);
        }
    }, function (error) {
        ShowAjaxError(error, "/OpsLayout/GetOpName");
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
    hasRefreshCanvas = false;
    $.blockUI({ message: "<h3>Loading layout...</h3>" });

    toolkit.clear(); // clear layout immediately
    jsPlumbSurface.refresh();

    // Before reload layout, need to remove all of summaries
    removeAllSummary();

    if (!page) page = 1;

    if (opsMaster === null || opsMaster === undefined) {
        opsMaster = CurrentOpmt;
    }

    opsGroups.length = 0;
    opsNodes.length = 0;
    opsEdges.length = 0;

    if (opsMaster !== null && opsMaster !== undefined) {
        const isOpmtEmpty = !Object.keys(opsMaster).length;

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

                // If user change mode of group, get the value for loading layout.
                // If user select row in operation plan grid view, get GroupMode of opmt. 
                // If GroupMode is empty, mode is group. 
                const opGrpMode = opsMaster.GroupMode === undefined || opsMaster.GroupMode === "" ? defaultGroupMode : opsMaster.GroupMode;
                groupMode = groupMode === undefined ? opGrpMode : groupMode;
                CurrentGroupMode = groupMode;

                // If user change language of opmt, get the value for loading layout.
                // If user select row in operation plan grid view, get language of opmt. 
                // If Language is empty, lang is df (default).
                var languageId = lang === undefined || lang === null ? MapLanguageToFlag(opsMaster.Language) : lang;

                const config = {
                    url: "/OpsLayout/GetOpdts",
                    async: true,
                    postData: JSON.stringify({ opsMaster: opsMaster, groupMode: groupMode, languageId: languageId, page: page })
                };

                AjaxPostCommon(config, (response) => {
                    console.log(response);
                    if (response.opdts && response.opdts.IsSuccess === false) {
                        console.error(response.opdts.Log);
                    } else {
                        const opmt = response.opmt, opdts = response.opdts;
                        if (groupMode === "FirstLoad" && opmt && opmt.GroupMode && opmt.GroupMode !== "") groupMode = opmt.GroupMode;

                        if (opmt) {
                            CurrentOpmt = response.opmt;
                            $(`#${BtnCreateOpmt}`).hide();
                            $(`#${BtnDeleteOpmt}`).show();
                            document.getElementById("wid-op-layout").style.cssText = 'display:block !important';
                            document.getElementById("wid-id-mtop-tap").style.cssText = 'display:block !important';
                        } else {
                            $(`#${BtnCreateOpmt}`).show();
                            $(`#${BtnDeleteOpmt}`).hide();
                            document.getElementById("wid-op-layout").style.cssText = 'display:none !important';
                            document.getElementById("wid-id-mtop-tap").style.cssText = 'display:none !important';
                        }

                        // Assigning data to global variables for "Package Operation Plan" screen.
                        if (response && response.opdts) {
                            if (response.opdts.nodes) window.MesProcesses = response.opdts.nodes;
                        }

                        if (opmt !== null && opmt !== undefined && opdts !== null && opdts !== undefined) {
                            const fontSize = opmt.LayoutFontSize === 0 || opmt.LayoutFontSize === undefined ?
                                defaultFontSize : opmt.LayoutFontSize;
                            const canvasHeight = opmt.CanvasHeight === 0 || opmt.CanvasHeight === undefined ?
                                defaultCanvasHeight : opmt.CanvasHeight;
                            const pWidth = opmt.ProcessWidth === null || opmt.ProcessWidth === undefined ?
                                defaultProcessWidth : opmt.ProcessWidth;
                            const pHeight = opmt.ProcessHeight === null || opmt.ProcessHeight === null ?
                                defaultProcessHeight : opmt.ProcessHeight;
                            let maxPage = 1;

                            if (opdts.nodes.length > 0) {
                                jsPlumbGroup = opdts.groups;

                                //console.log(jsPlumbGroup);

                                let gleft = dfGroupLeft, nLeft = dfNodeLeft, nTop = dfNodeTop;

                                $(jsPlumbGroup).each(function (index, value) {
                                    var group = {}, left = value.X, top = value.Y;

                                    group.id = `g${value.SubCode}`;
                                    group.title = value.CodeName;
                                    group.DisplayColor = "#00000000";
                                    group.BackgroundColor = value.BackgroundColor;
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

                                var tempNodes = opdts.nodes, isNullGroup, topNullGroup, leftNullGroup;

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

                                    $(tempNodes).each((index, value) => {
                                        //console.log(value);
                                        let isDisplay = true;

                                        if (value.Page > maxPage) maxPage = value.Page;

                                        var node = {};
                                        node.id = value.OpSerial.toString();

                                        //var name = value.OpNameLan === null || value.OpNameLan === undefined ?
                                        //    value.OpName : value.OpNameLan;
                                        let opName;

                                        switch (languageId) {
                                            case "vn":
                                                opName = value.VnOpName;
                                                break;
                                            case "gb":
                                                opName = value.GbOpName;
                                                break;
                                            case "mm":
                                                opName = value.MmOpName;
                                                break;
                                            case "id":
                                                opName = value.IdOpName;
                                                break;
                                            case "et":
                                                opName = value.EtOpName;
                                                break;
                                            default:
                                                opName = value.OpName ? value.OpName : "";
                                                break;
                                        }

                                        let machineName = value.MachineName === null || value.MachineName === undefined ? " " :
                                            value.MachineName;

                                        //const opName = name === null || name === undefined ? "" : name;
                                        var opNum = value.OpNum === null || value.OpNum === undefined ? " " : value.OpNum;
                                        node.name = `[${opNum}] ${opName}`;
                                        node.OpTime = value.OpTime;
                                        node.MachineCount = value.MachineCount;
                                        node.MachineName = machineName;
                                        node.ManCount = value.ManCount;
                                        node.OpName = opName;
                                        node.VnOpName = value.VnOpName;
                                        node.GbOpName = value.GbOpName;
                                        node.MmOpName = value.MmOpName;
                                        node.IdOpName = value.IdOpName;
                                        node.EtOpName = value.EtOpName;

                                        node.OpNum = opNum;
                                        let remarks = "";
                                        if (value.Remarks) remarks = value.Remarks;
                                        node.Remarks = remarks;
                                        node.OpGroup = value.OpGroup;
                                        node.MachineType = value.MachineType;
                                        node.ModuleId = value.ModuleId;
                                        node.DisplayColor = value.DisplayColor === null ? "#FFFFFF" : `#${value.DisplayColor.substr(3, 8)}`;
                                        node.ProcessWidth = pWidth;
                                        node.ProcessHeight = pHeight;
                                        node.LayoutFontSize = fontSize;
                                        node.Tools = [];
                                        node.IsDisplay = isDisplay;
                                        node.Page = value.Page === 0 ? 1 : value.Page;
                                        node.CanDelete = window.CanDelete;
                                        //START ADD - SON
                                        node.ShowButtonPlayVideo = $.isEmptyObject(value.VideoFile) ? 0 : 1;
                                        //END ADD - SON
                                        node.Achieve = value.Achieve;
                                        node.Target = CurrentMesPackage.MxTarget;
                                        //node.OpGroupName = value.OpGroupName;

                                        if (value.X === null && value.Y === null) {
                                            // Position of node based on group that the node belongs to.
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
                                                node.left = dfNodeLeft;
                                                node.top = dfNodeTop;
                                            } else {
                                                node.left = (nLeft += 2);
                                                node.top = (nTop += 2);
                                            }
                                        } else {
                                            node.left = value.UiLeft;
                                            node.top = value.UiTop;
                                        }

                                        switch (groupMode) {
                                            case "MachineType":
                                                if (tempNodes[index].MachineType === null || tempNodes[index].MachineType.trim() === "") {
                                                    isNullGroup = true;
                                                    node.group = "emptyGroup";
                                                    if (topNullGroup === undefined) topNullGroup = getTopLeft(tempNodes[index].Y);
                                                    if (leftNullGroup === undefined) leftNullGroup = getTopLeft(tempNodes[index].X);
                                                } else {
                                                    node.group = `g${tempNodes[index].MachineType}`;
                                                }
                                                node.Title = value.MachineName;
                                                break;
                                            case "OpGroup":
                                                if (tempNodes[index].OpGroup === null || tempNodes[index].OpGroup.trim() === "") {
                                                    isNullGroup = true;
                                                    node.group = "emptyGroup";
                                                    if (topNullGroup === undefined) topNullGroup = getTopLeft(tempNodes[index].Y);
                                                    if (leftNullGroup === undefined) leftNullGroup = getTopLeft(tempNodes[index].X);
                                                } else {
                                                    node.group = `g${tempNodes[index].OpGroup}`;
                                                }
                                                node.Title = value.OpGroupName;
                                                break;
                                            default:
                                                if (tempNodes[index].ModuleId === null || tempNodes[index].ModuleId.trim() === "") {
                                                    isNullGroup = true;
                                                    node.group = "emptyGroup";
                                                    if (topNullGroup === undefined) topNullGroup = getTopLeft(tempNodes[index].Y);
                                                    if (leftNullGroup === undefined) leftNullGroup = getTopLeft(tempNodes[index].X);
                                                } else {
                                                    node.group = `g${tempNodes[index].ModuleId}`;
                                                }
                                                node.Title = value.ModuleName;
                                        }

                                        opsNodes.push(node);
                                    });
                                }

                                if (isNullGroup) {
                                    var group = {
                                        id: "emptyGroup", title: "", left: leftNullGroup, top: topNullGroup, BackgroundColor: "white",
                                        isArrange: false, nodes: [], Processes: 0, Machines: 0, Workers: 0, TotalOpTime: 0
                                    };
                                    opsGroups.push(group);
                                }
                                window.ProcessGroups = opsGroups;

                                opsData.groups = opsGroups;
                                opsData.nodes = opsNodes;
                                opsData.edges = opdts.edges;

                                toolkit.load({ data: opsData });
                            } else {
                                const msg = getMsgByLang(msgInformData);
                                ShowMessage(msg.title, msg.value, ObjMessageType.Info);
                            }

                            var appElement = document.querySelector("[ng-app=opsApp]");
                            var $scope = angular.element(appElement).scope();
                            $scope.$apply(function () {
                                if (opdts.groupsToAdd !== null && opdts.groupsToAdd !== undefined) {
                                    $scope.groupsToAdd = opdts.groupsToAdd;
                                }
                                $scope.opdtMode = groupMode;
                                $scope.opsFont.fontSize = parseInt(fontSize);
                                $scope.canvasStyle.height = parseInt(canvasHeight);
                                $scope.pLayoutModifier.pHeight = parseInt(pHeight);
                                $scope.pLayoutModifier.pWidth = parseInt(pWidth);
                                $scope.layoutPage.pageNo = page;
                                $scope.layoutPage.maxPage = response.opdts.totalPage;

                                let flagLang = "df";
                                if (lang) {
                                    flagLang = lang;
                                } else {
                                    if (opmt && opmt.Language) flagLang = MapLanguageToFlag(opmt.Language);
                                }
                                $scope.layoutLang.selectedLang = flagLang;
                                selectedOpLanguage = flagLang;
                                $scope.isCloneMode = false;
                                $scope.isShowGroups = true;

                                // Check user permission to enable delete action of process
                                $scope.savingRule.canSave = window.CanDelete;
                            });

                            // Remove css of clone icon
                            $("#cloneProcess").removeClass("clone-mode");

                            // Initialize master data for process modal.
                            window.InitMasterData(opmt);
                        } else {
                            console.log(response.error);
                        }
                    }

                    $.unblockUI();
                });
            });
        }
    } else {
        MsgInform("Inform", "Please select a MES package", "error", true, true);
    }
}

function SetMenuActionMode() { }

function GetOpmtFromLocalStorage() {
    const { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo } = JSON.parse(localStorage.getItem(OpsMasterInfo));
    const opmt = new Opmt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo);

    return opmt;
}

function checkUserRole(edition, action, callBack) {
    const getUrConfig = {
        url: "/OpsLayout/GetUserRole",
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

function openCreatureOpmtModal() {
    $("#btnCreateOpmt").on("click", () => {
        checkUserRole("M", "Add", (result) => {
            if (result !== true) {
                const msg = getMsgByLang(msgPermission);
                ShowMessage(msg.title, msg.value);
                return;
            } else {
                const mesPpRowId = $("#tbMesPackage").jqGrid("getGridParam", "selrow");
                if (mesPpRowId) {
                    ClickButtonAddOpsMaster();
                    $(IdTableOpsModal).jqGrid("setGridParam", {
                        postData: {
                            styleCode: window.CurrentStyle.StyleCode,
                            styleSize: window.CurrentStyle.StyleSize,
                            styleColor: window.CurrentStyle.StyleColorSerial,
                            revNo: window.CurrentStyle.RevNo,
                            edition: "M"
                        }
                    }).trigger("reloadGrid");

                    $("#opsModal").modal('show');
                } else {
                    MsgInform("Error", "Please select a MES P.Package.", ObjMessageType.Error, false, true);
                }
            }
        });
    });
}

function SelectedRowMesPackageGv(r) {
    console.log(r);

    ActiveOpLayoutTab();
    window.CurrentMesPackage = r;
    $.blockUI();

    // Clearing all video processing information.
    ClearAllVideoInfo();

    LoadOpAndLineLayout(r);
    getModuleByStyleCode(r.StyleCode);
}

function getModuleByStyleCode(styleCode) {
    //Get colors
    $.post("/OpsLayout/GetModulesByCode", { styleCode }).then((response) => {
        console.log(response);
        if (response && response.modules) {
            //$scope.modules = response.data.modules;
            window.moduleList = response.modules;
            //$scope.modules.unshift({ ModuleId: "", ModuleName: "" });
            //window.moduleList.unshift({ ModuleId: "", ModuleName: "" });
            if (response.modules.length === 0) console.log("GetModulesByCode: no module");
        } else {
            console.log(response);
        }
    });
}

function ActiveOpLayoutTab() {
    document.getElementById(TabLiOpLayout).classList.add("active");
    document.getElementById(TabLiOpLineMapping).classList.remove("active");
    document.getElementById(TabLiEmployeeOpMapping).classList.remove("active");

    document.getElementById(TabDivMesOpLayout).classList.add("active", "in");
    document.getElementById(TabDivOpLineMapping).classList.remove("active", "in");
    document.getElementById(TabOpEmployeeMapping).classList.remove("active", "in");
}

function BeforeSelectRowPackageGv(rowid, e) {
    const isDisable = document.getElementById('btnSaveChanges').disabled;

    if (!isDisable) {
        const conf = confirm("Changes you made may not be saved.");
        if (conf) {
            document.getElementById('btnSaveChanges').disabled = true;
            return true;
        } else {
            return false;
        }
    } else {
        return true;
    }
}

function LoadOpAndLineLayout(r) {
    document.getElementById("selOpView").selectedIndex = "0";
    document.getElementById("wid-op-layout").classList.add("hide");

    r.OpRevNo = "001";
    r.Edition = "M";
    r.GroupMode = "FirstLoad";

    console.log("Style information");
    console.log(r);

    // Hide save popup if user select operation master
    $("#opslayoutModal").hide();

    if (typeof toolkit !== "undefined") {
        $(`#${BtnCreateOpmt}`).hide();
        $(`#${BtnDeleteOpmt}`).show();
        loadLayout(r);
    }
}

function RefreshAchieveTarget() {
    setInterval(function () {
        if (CurrentMesPackage && CurrentMesPackage.MxPackage) {
            if (enableRefreshAT) {
                GetMpdt(CurrentMesPackage.MxPackage, (mpdt) => {
                    if (window.CurrentOpmt && CurrentGroupMode && selectedOpLanguage) {
                        GetOpdts(window.CurrentOpmt, CurrentGroupMode, selectedOpLanguage, window.processPage, (res) => {
                            toolkit.eachNode((index, node) => {
                                //console.log(node);
                                for (let p of res.opdts.nodes) {
                                    if (p.OpSerial.toString() === node.id) {
                                        node.data.Achieve = p.Achieve;
                                        node.data.Target = mpdt.MxTarget;
                                        toolkit.updateNode(node);
                                    }
                                }
                            });
                        });
                    } else {
                        console.log(window.CurrentOpmt);
                        console.log(CurrentGroupMode);
                        console.log(selectedOpLanguage);
                    }
                });
            }
        }
        else {
            console.log(CurrentMesPackage.MxPackage);
        }
    }, RefreshTime);
}

function GetOpdts(opsMaster, groupMode, languageId, page, callBack) {
    $.post(GetOpdtsUri, { opsMaster, groupMode, languageId, page }).done((response) => {
        if (response && response.error) {
            console.log(response.error);
        } else {
            callBack(response);
        }
    });
}

function GetMpdt(mxPackage, callBack) {
    $.post(GetMpdtUri, { mxPackage }).done((response) => {
        //console.log(response.result);
        if (response && response.result) callBack(response.result);
    });
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

    // #region Modal add new process

    //Envent on modal
    ClickButtonRemoveImg();
    //ClickButtonRemoveVideo();
    flProcessImageChange();

    ClickButtonOpTime();
    //ClickLoadAllProcessName();
    ClickButtonOk();

    //ClickButtonGetProNameTemplate();

    SelectModule();

    BindDataToJqGridInputOpTimeModal([]);
    var isOpmtEmpty = !Object.keys(CurrentOpmt).length;

    if (CurrentOpmt === null || CurrentOpmt === undefined || isOpmtEmpty) {
        currentLang = "df";
    } else {
        currentLang = MapLanguageToFlag(CurrentOpmt.Language);
    }

    getOpNames(currentLang);
    window.CurLang = currentLang;
    jqGridProcessName();

    // Block popup by right click of mouse
    const blockContextMenu = evt => { evt.preventDefault(); };
    window.addEventListener("contextmenu", blockContextMenu);

    $("#opsmodal").draggable();
    $("#opslayoutModal").draggable();

    window.onbeforeunload = function () {
        var message = "Important: Please click on 'Save' button to leave this page.";

        if (window.isChange && window.CanSave) return message;
    };

    //#endregion

    //#region Operation plan modal
    openCreatureOpmtModal();

    $(ProcessModalId).on('shown.bs.modal', () => {
        if (window.IsInsertProcess === true) $.unblockUI();
    });
    //#endregion

    RefreshAchieveTarget();
})();

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
//#endregion

//#region Angular directives
mesOpApp.directive("node", function (jsPlumbFactory) {
    return jsPlumbFactory.node({
        templateUrl: "node_template.tpl",
        inherit: ["removeProcess", "editTodo", "doneEditing", "updateNode", "changeNodePage", "playVideoProcess"] //SON MOD - Adding playVideoProcess
    });
});

mesOpApp.directive("group", function (jsPlumbFactory) {
    return jsPlumbFactory.group({
        inherit: ["remove", "toggleGroup"],
        templateUrl: "group_template.tpl"
    });
});

mesOpApp.directive("appFilereader", function ($q, $http) {
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
                            let { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo } = CurrentOpmt;
                            let opdt = new Opdt(Edition, "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, scope.$ctrlNode.opsProcess.OpSerial);
                            var imgFile = values[0].file,
                                imgData = window.SetDataToUploadFile(imgFile, opdt);

                            if (values[0].isImage) {
                                ngModel.$setViewValue({
                                    src: "../img/uploading.gif",
                                    file: scope.$ctrlNode.fileInfo
                                });

                                var config = scope.$ctrlNode.postConfig("/OpsLayout/UploadImageProcess", imgData);

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
                                    ShowAjaxError(error, "/OpsLayout/UploadImageProcess");
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
                                        url: "/OpsLayout/UploadVideoProcess",
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

mesOpApp.directive("floatingPanel", function () {
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
//#endregion

//#region Angular services
mesOpApp.service("sharedService", function () {
    const getUrConfig = {
        url: "/OpsLayout/GetUserRole",
        async: true
    };

    function getUserRole(callback) {
        return AjaxGetCommon(getUrConfig, callback);
    }

    const service = { getUserRole: getUserRole };

    return service;
});
//#endregion

//#region Angular filter
mesOpApp.filter("trustUrl", ["$sce", function ($sce) {
    return function (recordingUrl) {
        return $sce.trustAsResourceUrl(recordingUrl);
    };
}]);
//#endregion

//#region Angular controllers
mesOpApp.controller("OpsLayoutController", function ($uibModal, $log, $scope, $http, jsPlumbService, sharedService) {
    //#region JsPlumb
    var ctrl = this;
    var toolkit;
    var surface;
    window.jsps = jsPlumbService;
    window.ctrl = this;
    $scope.draggableTypes = [{ label: "Group", type: "group", group: true }, { label: "Process", type: "node" },
    { label: "Summary", type: "sumTitle", group: true }, { label: "Summary", type: "sumEditable", group: true },
    { label: "Summary", type: "sumMainMachines", group: true }, { label: "Summary", type: "sumAllMachines", group: true },
    { label: "Summary", type: "sumTool", group: true }, { label: "Summary", type: "sumWorker", group: true },
    { label: "Summary", type: "showPattern", group: true }, { label: "Summary", type: "sumBom", group: true },
    { label: "Summary", type: "jigSum", group: true }, { label: "GrpModule", type: "grpModule", group: true }];

    // Show the modal to add new or update a process

    ctrl.toolkitParams = {
        groupFactory: function (type, data, callback) {
            if (CurrentOpmt.ConfirmChk === "Y" && type === "group") {
                MsgInform("Alert", "This operation plan has already been confirmed.", "alert", true, true);
                return;
            }

            if (data.type !== "group") {
                let pos = {
                    top: `${data.top}px`,
                    left: `${data.left}px`
                };

                const lang = $scope.layoutLang.selectedLang;
                createSummary(data.type, pos, lang);
            } else {
                //console.log(type);

                const curMesPackage = GetSelectedOneRowData("#tbMesPackage"),
                    isOpmtEmpty = !Object.keys(CurrentOpmt).length,
                    isMesPackageEmpty = !Object.keys(curMesPackage).length;
                //console.log(curMesPackage);

                if (!isMesPackageEmpty && !isOpmtEmpty) {
                    checkUserRole("M", "Add", (result) => {
                        if (result !== true) {
                            const msg = getMsgByLang(msgPermission);
                            ShowMessage(msg.title, msg.value);
                            return;
                        } else {
                            if ($scope.groupsToAdd.length > 0) {
                                const modalInstance = $uibModal.open({
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
                    MsgInform("Inform", "Please select a MES package.", "alert", true, true);
                }
            }
        },
        nodeFactory: function (type, data, callback) {
            if (type) {
                if (CurrentOpmt.ConfirmChk === "Y") {
                    MsgInform("Alert", "This operation plan has already been confirmed.", "alert", true, true);
                    return;
                }
                $.blockUI({ message: "<h3>Initializing data...</h3>" });
                const isOpmtEmpty = !Object.keys(CurrentOpmt).length;

                switch (data.type) {
                    case "node":
                        data.name = toolkit.getNodeCount() + 1;
                        if (!isOpmtEmpty) {
                            checkUserRole("M", "Add", (result) => {
                                if (result !== true) {
                                    const msg = getMsgByLang(msgPermission);
                                    ShowMessage(msg.title, msg.value);
                                    $.unblockUI();
                                    return;
                                } else {
                                    window.LayoutPage = $scope.layoutPage.pageNo;
                                    window.LayoutGroupMode = $scope.opdtMode;

                                    // Assign data to global variables in process-partial-view.js
                                    window.LayoutLeftX = data.left;
                                    window.LayoutTopY = data.top;

                                    // Changing background color for process detail modal by tab background color
                                    changeModalHeaderBgColor();

                                    LayoutSaveEvent(callback);
                                }
                            });
                        } else {
                            MsgInform("Inform", "Please select a MES package.", "alert", true, true);
                        }

                        break;
                }
            } else {
                console.log("no type");
            }
        }
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

                                    window.isChange = true;
                                }
                            }
                            // Clone (copy) process
                            if ($scope.isCloneMode) {
                                const isOpmtEmpty = !Object.keys(CurrentOpmt).length;

                                if (!isOpmtEmpty) {
                                    checkUserRole("M", "Add", (result) => {
                                        if (result !== true) {
                                            const msg = getMsgByLang(msgPermission);
                                            ShowMessage(msg.title, msg.value);
                                            return;
                                        } else {
                                            if (CurrentOpmt.ConfirmChk === "Y") {
                                                const msg = getMsgByLang(msgOpConfirmed);
                                                ShowMessage(msg.title, msg.value);
                                                return;
                                            }

                                            const n = params.node.data;
                                            const opNum = n.name.split("]")[0].split("[");
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
                                                opdt = new Opdt("M", "MES", CurrentOpmt.StyleCode, CurrentOpmt.StyleColorSerial, CurrentOpmt.StyleSize,
                                                    CurrentOpmt.RevNo, CurrentOpmt.OpRevNo, n.id, n.OpName, n.OpGroup, n.MachineType, n.ModuleId,
                                                    null, n.Page, xG + "." + xN.toString(), yG + "." + yN.toString(), colorToSave),
                                                ajaxConfig = new AjaxConfig("/OpsLayout/CloneSingleProcess", true, JSON.stringify({ opdt }));

                                            AjaxPostCommon(ajaxConfig, (response) => {
                                                if (response.error === null || response.error === undefined) {
                                                    const opdtResult = response.result;
                                                    if (opdtResult !== "false") {
                                                        const node = CreateObjectForLayout(CurrentOpmt, opdtResult);
                                                        node.DisplayColor = n.DisplayColor;
                                                        node.left = parseInt(n.left) + 2;
                                                        node.top = parseInt(n.top) + 2;
                                                        node.group = n.group;
                                                        node.MachineName = n.MachineName;
                                                        node.CanDelete = window.CanDelete;
                                                        node.Title = n.Title;
                                                        toolkit.addNode(node);
                                                    } else {
                                                        console.log("An error occurred, could not clone this process.");
                                                    }
                                                } else {
                                                    console.log(response.error);
                                                }
                                            });
                                        }
                                    });
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
                            // These prevent displaying modal (to move processes of group to another page).
                            params.e.preventDefault();
                            params.e.stopPropagation();

                            if (CurrentOpmt.ConfirmChk === "Y") {
                                //MsgInform("Alert", "This operation plan has already been confirmed.", "alert", true, true);
                                //return;
                                // Allow displaying process detail, however disable updating button.
                                $("#btnUpdateProcess").hide();
                            } else {
                                $("#btnUpdateProcess").show();
                            }
                            $.blockUI({ message: "<h3>Loading data...</h3>" });

                            checkUserRole("M", "Add", (result) => {
                                if (result === false) {
                                    const msg = getMsgByLang(msgPermission);
                                    ShowMessage(msg.title, msg.value);
                                } else {
                                    let process = params.node.data, grpTop = 0, grpLeft = 0;
                                    if (params.node.group && params.node.group.data) {
                                        if (params.node.group.data.title) process.GroupTitle = params.node.group.data.title;
                                        if (params.node.group.data.top) grpTop = getTopLeft(params.node.group.data.top.toString());
                                        if (params.node.group.data.left) grpLeft = getTopLeft(params.node.group.data.left.toString());
                                    }
                                    window.LayoutPage = $scope.layoutPage.pageNo;
                                    window.LayoutGroupMode = $scope.opdtMode;

                                    const processTop = getTopLeft(process.top.toString()),
                                        processLeft = getTopLeft(process.left.toString());

                                    window.LayoutLeftX = `${grpLeft}.${processLeft}`;
                                    window.LayoutTopY = `${grpTop}.${processTop}`;

                                    // Changing background color for process detail modal by tab background color
                                    changeModalHeaderBgColor();

                                    LayoutUpdateEvent(process, function (node) {
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
                                        process.MachineName = node.MachineName;
                                        process.MachineType = node.MachineType;
                                        process.MachineCount = node.MachineCount;
                                        toolkit.updateNode(process);

                                        window.isChange = true;
                                    });
                                }
                            });
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
                    revert: false,
                    constrain: false,
                    events: {
                        click: (params) => {
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
                            if (CurrentOpmt.ConfirmChk === "Y") {
                                MsgInform("Alert", "This operation plan has already been confirmed.", "alert", true, true);
                            } else {
                                // Check user permission to enable delete action of process
                                checkUserRole("M", "Update", (result) => {
                                    if (result === true) {
                                        checkUserRole("M", "Delete", (rs) => {
                                            if (rs) {
                                                const isShowBtnUpdateModule = $scope.opdtMode === "ModuleType" ? false : true,
                                                    modalConfig = {
                                                        templateUrl: "changePageNoModalContent.html",
                                                        controller: "ChangeNodePageModalCtrl",
                                                        controllerAs: "$ctrl",
                                                        modalData: { maxPage: $scope.layoutPage.maxPage, modules: window.moduleList, isShowBtnUpdateModule }
                                                    };
                                                modalInstance(modalConfig, (modalData) => {
                                                    const nodes = params.node.getNodes(), opdts = [];

                                                    // If there are selected page, updating page
                                                    if (modalData.pageNo) {
                                                        for (let n of nodes) {
                                                            const opdt = new Opdt("M", "", CurrentOpmt.StyleCode, CurrentOpmt.StyleColorSerial, CurrentOpmt.StyleSize,
                                                                CurrentOpmt.RevNo, CurrentOpmt.OpRevNo, n.id, n.data.OpName,
                                                                n.data.OpGroup, n.data.MachineType, n.data.ModuleId, null, modalData.pageNo);
                                                            opdts.push(opdt);
                                                        }
                                                        const cf = new AjaxConfig("/OpsLayout/UpdatePage", true, JSON.stringify({ opdts }));
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
                                                                const opdt = new Opdt("M", "MES", CurrentOpmt.StyleCode, CurrentOpmt.StyleColorSerial,
                                                                    CurrentOpmt.StyleSize, CurrentOpmt.RevNo, CurrentOpmt.OpRevNo, n.id, n.data.OpName,
                                                                    n.data.OpGroup, n.data.MachineType, modalData.selectedModule.ModuleId, null, n.data.Page);
                                                                opdts.push(opdt);
                                                            }

                                                            const cf = new AjaxConfig("/OpsLayout/UpdateModule", true, JSON.stringify({ opdts }));
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
                                            } else {
                                                MsgInform("Alert", "No right to perform this action.", "alert", true, true);
                                            }
                                        });
                                    } else {
                                        MsgInform("Alert", "No right to perform this action.", "alert", true, true);
                                    }
                                });
                            }
                        }
                    }
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
                create: function (component) {
                    if (component.element === undefined) {
                        let isShow = $scope.savingRule.canSave && $scope.isShowDeleteFlow ? "block" : "none";

                        const isSourceVisible = $(component.source).is(":visible"),
                            isTargetVisible = $(component.target).is(":visible");

                        if (!isSourceVisible || !isTargetVisible) isShow = "none";
                        if (CurrentOpmt.ConfirmChk === "Y") isShow = "none";

                        return $(`<img style="display: ${isShow}; background-color:transparent;"` +
                            `src="../../assets/ops-layout/img/del-icon.png" alt="Delete connection">`);
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
            }
        },
        events: {
            canvasClick: function () {

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

            toolkit.eachEdge(function (index, value) {
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

            return result;
        };

        toolkit.beforeStartConnect = () => {
            if (hasRefreshCanvas === false) {
                surface.refresh();
                hasRefreshCanvas = true;
            }
        };
    };
    ctrl.typeExtractor = el => el.getAttribute("jtk-node-type");
    //#endregion

    //#region Variables
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

    $scope.groupsToAdd = [];
    $scope.animationsEnabled = true;

    //const opmt = CurrentOpmt;
    var isOpmtEmpty = !Object.keys(CurrentOpmt).length;

    const { GroupMode, CanvasHeight, ProcessHeight, ProcessWidth, LayoutFontSize, ConfirmChk } = CurrentOpmt !== null && !isOpmtEmpty ?
        CurrentOpmt : {
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
    $http.post("/OpsLayout/GetColorByTheme", { theme: "PKStandard" }).then(function (response) {
        var lstColor = [];
        $.each(response.data, function (index, value) {
            var opColor = new OpColor(value.ColorDesc, "#" + value.HexaValue);
            lstColor.push(opColor);
        });
        $scope.opsColors = lstColor;
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
        var isOpmtEmpty = !Object.keys(CurrentOpmt).length;
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
        if (response.rdRole !== null || response.facRole !== null || response.mesRole !== null) {
            $scope.userRole = response;
            $scope.showSaveData.isShowSaveData = true;
        } else {
            console.log("Could not get User Role");
        }
    });

    $scope.disableButtons = {
        btnSave: true,
        btnSaveAsNewOpRev: true,
        btnSaveAsFacEdition: true,
        btnSaveAndConfirm: true
    };
    $scope.isShowFacCb = false;
    $scope.isShowGetMxPk = false;

    $scope.showSaveModal = (isClosedButton) => {
        if (!isClosedButton) {
            $("#opslayoutModal").toggle();

            var isOpmtEmpty = !Object.keys(CurrentOpmt).length;
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

                let { Edition, ConfirmChk } = CurrentOpmt;
                Edition = "M";

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
                            //if ($scope.userRole.facRole.IsAdd === "1") $scope.disableButtons.btnSaveAsFacEdition = false;
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
                            if (ConfirmChk === "" || ConfirmChk === null || ConfirmChk === undefined) {
                                if ($scope.userRole.mesRole.IsUpdate === "1") $scope.disableButtons.btnSave = false;
                                //if ($scope.userRole.mesRole.IsAdd === "1") $scope.disableButtons.btnSaveAsNewOpRev = false;
                                if ($scope.userRole.mesRole.IsConfirm === "1") $scope.disableButtons.btnSaveAndConfirm = false;
                            }

                            $scope.isShowGetMxPk = false;
                        }

                        // User has sufficient rights by Mes menu and role
                        if ($scope.userRole.facRole !== null) {
                            //if ($scope.userRole.facRole.IsAdd === "1") $scope.disableButtons.btnSaveAsFacEdition = false;
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

    $scope.saveOp = (url, isSaveAsFom, isSaveAsNew) => {
        $.blockUI(window.BlockUI);
        const { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, Remarks, Factory } = CurrentOpmt;
        const lang = MapFlagValueToLanguage($scope.layoutLang.selectedLang);
        const opsViewMode = $scope.opdtMode;
        let remarks = $scope.opmt.remarks;
        if (remarks === null || remarks === undefined || remarks.trim() === "") remarks = Remarks;

        const factory = window.CurrentMpmt.MesFactory;
        if (factory === undefined || factory === null || factory === "") {
            const msg = getMsgByLang(msgSelectFactory);
            MsgInform(msg.title, msg.value, ObjMessageType.Error, false, true);
            $.unblockUI();
            return;
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
            $.unblockUI();
            var result = response;

            if (result === true) {
                // Clear remarks
                $scope.opmt.remarks = null;

                window.isChange = false;

                const msg = getMsgByLang(msgSave);
                MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);

                // Syncing data to PKMES
                opmt.MxPackage = CurrentMesPackage.MxPackage;
                SyncOperationPlan(opmt);

                $("#opslayoutModal").hide();
            } else {
                console.log(response.error);
                const msg = getMsgByLang(msgError);
                ShowMessage(msg.title, msg.value, ObjMessageType.Error);
            }
        });
    };

    $scope.confirmOpmt = () => {
        const config = {
            url: "/OpsLayout/ConfirmOpmt",
            async: true,
            postData: JSON.stringify({ opmt: CurrentOpmt })
        };
        AjaxPostCommon(config, (response) => {
            var result = response;

            if (result === true) {
                $scope.$apply(function () {
                    $scope.disableButtons = {
                        btnSave: true,
                        btnSaveAsNewOpRev: true,
                        btnSaveAsFacEdition: true,
                        btnSaveAndConfirm: true
                    };
                });
                CurrentOpmt.ConfirmChk = "Y";

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

    function modalInstance(initialObj, callBack) {
        var modal = $uibModal.open({
            animation: $scope.animationsEnabled,
            ariaLabelledBy: "modal-title",
            ariaDescribedBy: "modal-body",
            templateUrl: initialObj.templateUrl,
            controller: initialObj.controller,
            controllerAs: initialObj.controllerAs,
            windowClass: "modal-custom",
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

            node.data.name = `[${node.data.OpNum}] ${opName}`;
            toolkit.updateNode(node);
        });
    }

    $scope.selectLang = (lang) => {
        if (window.isChange) {
            const msg = getMsgByLang(msgLostData);
            const r = confirm(msg.value);
            if (r === true) {
                $scope.layoutLang.selectedLang = lang;

                // Assign language for loading process name
                selectedOpLanguage = lang;
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
            selectedOpLanguage = lang;
        }
    };

    $scope.isCloneMode = false;
    $scope.changeToCloneMode = () => {
        if (CurrentOpmt.ConfirmChk === "Y") {
            MsgInform("Alert", "This operation plan has already been confirmed.", "alert", true, true);
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

    };
    $scope.nextPage = function (value) {
        if (value === $scope.layoutPage.maxPage) return;
        $scope.layoutPage.pageNo = value + 1;
        window.processPage = value + 1;

        loadLayout(null, $scope.layoutLang.selectedLang, $scope.opdtMode, $scope.layoutPage.pageNo);
    };
    $scope.newPage = () => {
        if (CurrentOpmt.ConfirmChk === "Y") {
            const msg = getMsgByLang(msgOpConfirmed);
            ShowMessage(msg.title, msg.value);
            return;
        }
        toolkit.clear();
        const maxPage = $scope.layoutPage.maxPage;
        $scope.layoutPage.maxPage = maxPage + 1;
        $scope.layoutPage.pageNo = $scope.layoutPage.maxPage;

        //toolkit.eachNode((index, n) => {
        //    if (n.data.Page === $scope.layoutPage.maxPage) {
        //        $scope.layoutPage.maxPage = maxPage + 1;
        //        $scope.layoutPage.pageNo = $scope.layoutPage.maxPage;
        //        pageChangedEvent($scope.layoutPage.maxPage, maxPage);

        //        return;
        //    }
        //});
    };

    $scope.showProcessModal = (() => {
        $("#panel-process").toggle();
    });
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

    $scope.changeProcessHeight = ((value) => {
        if (value !== null && value !== undefined) {
            toolkit.eachNode((index, node) => {
                node.data.ProcessHeight = value;
                toolkit.updateNode(node);
            });
        } else {
            const msg = getMsgByLang(msgInvalid);
            ShowMessage(msg.title, msg.value, ObjMessageType.Error);
        }
    });

    $scope.changeProcessWidth = ((value) => {
        if (value !== null && value !== undefined) {
            toolkit.eachNode((index, node) => {
                node.data.ProcessWidth = value;
                toolkit.updateNode(node);
            });
        } else {
            const msg = getMsgByLang(msgInvalid);
            ShowMessage(msg.title, msg.value, ObjMessageType.Error);
        }
    });

    $scope.removeProcess = function (node) {
        const { StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, ConfirmChk } = CurrentOpmt;
        if (ConfirmChk === "Y") {
            const msg = getMsgByLang(msgOpConfirmed);
            ShowMessage(msg.title, msg.value);
        } else {
            checkUserRole("M", "Delete", (result) => {
                if (result !== true) {
                    const msg = getMsgByLang(msgPermission);
                    ShowMessage(msg.title, msg.value);
                    return;
                }
            });
            const smsg = getMsgByLang(msgConfirmDelete);

            ConfirmYesNo(smsg.title, smsg.value, function () {
                $.blockUI();
                window.isChange = true;

                const opdt = new Opdt("M", "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, node.id);

                //START ADD - Son Nguyen Cao
                //Update next process.
                var objConnection = toolkit.getNode(node.id).getEdges();
                var lstOpdtNextOp = [];
                $.each(objConnection, function (index, vaule) {
                    if (opdt.OpSerial !== vaule.source.id) {
                        var newOpdt = new Opdt("M", "", StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo, vaule.source.id);
                        lstOpdtNextOp.push(newOpdt);
                    }
                });
                //END ADD - Son Nguyen Cao.

                let deleteProcess = $http.post("/OpsLayout/DeleteProcess", { opdt: opdt });
                deleteProcess.then(function (response) {
                    if (response && response.data && response.data.error) {
                        console.log(response.data.error);
                    } else {
                        if (response && response.data) {
                            toolkit.removeNode(node);

                            // Removing empty group also
                            const grp = toolkit.getGroup(node.group);
                            const pcs = grp.getNodes();
                            if (pcs && pcs.length === 0) toolkit.removeGroup(grp, true);

                            const msg = getMsgByLang(msgDeleted);
                            MsgInform(msg.title, msg.value, ObjMessageType.Info, false, true);

                            $.unblockUI();

                            //START ADD - Son Nguyen Cao
                            //Update next process.                                                        
                            $http.post("/OpsLayout/UpdateNextOp", { lstOpdt: lstOpdtNextOp });
                            //END ADD - Son Nguyen Cao
                        } else {
                            const msg = getMsgByLang(msgError);
                            MsgInform(msg.title, msg.value, ObjMessageType.Error, false, false);
                        }
                    }
                }, function (error) {
                    ShowAjaxError(error, "/OpsLayout/DeleteProcess");
                });
            });
        }
    };

    //START ADD - SON
    $scope.showPrintingLine = false;

    $scope.showVerticalLine = function () {
        $scope.showPrintingLine = !$scope.showPrintingLine;
    };

    $scope.playVideoProcess = function (node) {
        const opDetail = {
            StyleCode: CurrentOpmt.StyleCode,
            StyleSize: CurrentOpmt.StyleSize,
            StyleColorSerial: CurrentOpmt.StyleColorSerial,
            RevNo: CurrentOpmt.RevNo,
            OpRevNo: CurrentOpmt.OpRevNo,
            OpSerial: node.id,
            Edition: "M"
        };

        //Get colors
        $http.post("/Ops/GetOpDetailByCode", { opDetail: opDetail }).then(function (response) {
            const opdt = response.data;
            if ($.isEmptyObject(opdt.VideoOpLink)) {
                alert("This process does not have video.");
            } else {
                PlayVideo(opdt);
            }
        });
    };
    //END ADD - SON

    $scope.showAchieveTarget = function () {
        $(".mes__layout--achieve-taget").toggle();
        enableRefreshAT = false;
    };

    $scope.changeNodePage = function ($event, node) {
        if (CurrentOpmt.ConfirmChk === "Y") {
            MsgInform("Alert", "This operation plan has already been confirmed.", "alert", true, true);
        } else {
            // Check user permission to enable delete action of process
            checkUserRole("M", "Update", (result) => {
                if (result === true) {
                    checkUserRole("M", "Delete", (rs) => {
                        if (rs) {
                            const isShowBtnUpdateModule = $scope.opdtMode === "ModuleType" ? false : true,
                                modalConfig = {
                                    templateUrl: "changePageNoModalContent.html",
                                    controller: "ChangeNodePageModalCtrl",
                                    controllerAs: "$ctrl",
                                    modalData: { maxPage: $scope.layoutPage.maxPage, modules: window.moduleList, isShowBtnUpdateModule }
                                };
                            modalInstance(modalConfig, (modalData) => {
                                //let { Edition, StyleCode, StyleColorSerial, StyleSize, RevNo, OpRevNo } = GetSelectedOneRowData(gridOpsTableId);
                                const n = toolkit.getNode(node.id),
                                    opdt = new Opdt("M", "MES", CurrentOpmt.StyleCode, CurrentOpmt.StyleColorSerial, CurrentOpmt.StyleSize,
                                        CurrentOpmt.RevNo, CurrentOpmt.OpRevNo, n.id, n.data.OpName,
                                        n.data.OpGroup, n.data.MachineType, n.data.ModuleId, null, n.data.Page),
                                    opdts = [];

                                // If there are selected page, updating page
                                if (modalData.pageNo) {
                                    opdt.Page = modalData.pageNo;
                                    opdts.push(opdt);

                                    const cf = new AjaxConfig("/OpsLayout/UpdatePage", true, JSON.stringify({ opdts }));
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

                                        const cf = new AjaxConfig("/OpsLayout/UpdateModule", true, JSON.stringify({ opdts }));

                                        if ($scope.opdtMode === "ModuleType") {
                                            const grp = toolkit.getGroup(`g${opdt.ModuleId}`);
                                            if (grp) {
                                                AjaxPostCommon(cf, (res) => {
                                                    if (res) {
                                                        n.ModuleId = modalData.selectedModule.ModuleId;
                                                        n.Title = modalData.selectedModule.ModuleName;
                                                        n.group = `g${opdt.ModuleId}`;
                                                        toolkit.updateNode(n);
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
                        } else {
                            MsgInform("Alert", "No right to perform this action.", "alert", true, true);
                        }
                    });
                } else {
                    MsgInform("Alert", "No right to perform this action.", "alert", true, true);
                }
            });
        }
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

            if (CurrentOpmt.ConfirmChk === "Y") {
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
                window.CurrentProcessViewMode = 1;
                toolkit.eachNode((index, node) => {
                    node.data.name = node.data.Remarks !== "" ? `[${node.data.OpNum}] ${node.data.OpName} (${node.data.Remarks})` :
                        `[${node.data.OpNum}] ${node.data.OpName}`;
                    toolkit.updateNode(node);
                });
                break;
            case "2":
                $scope.rdOpName = "2";
                window.CurrentProcessViewMode = 2;
                toolkit.eachNode((index, node) => {
                    node.data.name = `[${node.data.OpNum}] ${node.data.OpName}`;
                    toolkit.updateNode(node);
                });
                break;
            case "3":
                $scope.rdOpName = "3";
                window.CurrentProcessViewMode = 3;
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
                n.data.DisplayColor = "#fff";
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
    }

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
    };

    $scope.changeColorFlow = ((color) => {
        changeFlowColor(color);
    });

    $scope.colorFlowChanged = ((color) => {
        $scope.opsColor.flow = color;
        $scope.showFlowCp = false;

        changeFlowColor(color);
    });

    $scope.changeColorAllNodes = (color => {
        $(".ops-node").css("font-size", $scope.opsFont.fontSize);
        changeColor($(".ops-node"), color, "background", "node-all-print");
    });

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

        if (CurrentOpmt.ConfirmChk === "Y") {
            MsgInform("Alert", "This operation plan has already been confirmed.", "alert", true, true);
        } else {
            // Check user permission to enable delete action of process
            checkUserRole("M", "Update", (result) => {
                if (result === true) {
                    checkUserRole("M", "Delete", (rs) => {
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
        }
    };

    $scope.showProcessVideoModal = () => {
        //const currentOpmt = GetSelectedOneRowData(gridOpsTableId);
        if (CurrentOpmt.ConfirmChk === "Y") {
            const msg = getMsgByLang(msgOpConfirmed);
            ShowMessage(msg.title, msg.value);
        } else {
            const isOpmtEmpty = !Object.keys(CurrentOpmt).length;

            if (!isOpmtEmpty) {
                checkUserRole("M", "Add", (result) => {
                    if (result !== true) {
                        const msg = getMsgByLang(msgPermission);
                        ShowMessage(msg.title, msg.value);
                        return;
                    } else {
                        if (HideOpVideoModal) {
                            $('#vdpModal').show();
                        } else {
                            $('#vdpModal').modal('show');
                        }
                    }
                });
            }
        }
    };

    $scope.closeRightShortListVideo = () => {
        $("#divRightVideoList").hide("slide", { direction: "right" }, 1000);
        $("#vdpModal").show("slide", { direction: "left" }, 1000);
        $(".modal-backdrop.fade.in").show("slide", { direction: "left" }, 1000);
    };

    $scope.saveProcessVideo = () => {
        console.log("About to split video...");
        SaveProcessVideo();
    };
    //#endregion
});

mesOpApp.controller("GroupModalCtrl", function ($uibModalInstance, items) {
    var $ctrl = this;
    $ctrl.items = items;
    //$ctrl.selectedGroup = $ctrl.items[0];

    $ctrl.ok = function () {
        if ($ctrl.selectedGroup) {
            $uibModalInstance.close({ id: `g${$ctrl.selectedGroup.SubCode}`, title: $ctrl.selectedGroup.CodeName });
        }
    };

    $ctrl.cancel = function () {
        $uibModalInstance.dismiss("cancel");
    };
});

mesOpApp.controller("ChangeNodePageModalCtrl", function ($scope, $uibModalInstance, modalData) {
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
        $uibModalInstance.dismiss("cancel");
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
//#endregion