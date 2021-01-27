var styleCode = "";
var styleSize = "";
var styleColorSerial = "";
var styleColorSerial = "";
var revNo = "";
var tab = "#patternLinking";
var data = null;
var opmasterRow = false;
var alertResult = false;
var UserRole;
var oldTab = "";
var _checkEdit = false;
var editionRow = "";
var confChk = "";
var grid_selector = "#gridOpsTable";
var pager_selector = "#gridOpsPaper";


function ShowConTrucGrid() {
    var selLanId = $("#drpLanguages").val();
    styleCode = getUrlParameter("styleCode");
    styleSize = getUrlParameter("styleSize");
    styleColorSerial = getUrlParameter("serial");
    revNo = getUrlParameter("revNo");
    if (styleCode === "styleCode") {
        // OPS Style search
        var StyleMasterGlobal = JSON.parse(localStorage.getItem(StyleMasterInfo));
        if (!$.isEmptyObject(StyleMasterGlobal)) {
            styleCode = StyleMasterGlobal.StyleCode;
            styleSize = StyleMasterGlobal.StyleSize;
            styleColorSerial = StyleMasterGlobal.StyleColorSerial;
            revNo = StyleMasterGlobal.RevNo;
            editionRow = !isEmpty(StyleMasterGlobal.Edition) ? StyleMasterGlobal.Edition.slice(0, 1).toUpperCase() : "";
            confChk = StyleMasterGlobal.ConfirmChk;
            opmasterRow = true;
        }
    }
    CreatePatternGrid("UNI0037", "LRG", "001", "001");
    CreateOperationGrid("UNI0037", "LRG", "001", "001", "1", selLanId);
    //Tools linking==========
    CreateOperationToolsGrid("UNI0037", "LRG", "001", "001", "1", selLanId);
    CreateToolsGrid($("#cbTool").val());
    // Machine linking
    CreateOperationMcGrid("UNI0037", "LRG", "001", "001", "1", selLanId);
    CreateMcGrid($("#cbMachine").val());
    LoadOpsMasterGrid();
    CreateGridTemp();
    //$(gridOpsTableId).setGridHeight(200);
}

function SetTiTle() {
    if (!$.isEmptyObject(data)) {
        //Operation_Grid.jqGrid('setCaption', " OPS Detail - Style: " + data.StyleCode + " | Size: " + data.StyleSize
        //                       + " | Color: " + data.StyleColorSerial + " | Revision: " + data.RevNo + "|  Edtion: " + data.Edition2 + " | Op Revision:" + data.OpRevNo);
        //OperationGrid.jqGrid('setCaption', " OPS Detail - Style: " + data.StyleCode + " | Size: " + data.StyleSize
        //                       + " | Color: " + data.StyleColorSerial + " | Revision: " + data.RevNo + "|  Edtion: " + data.Edition2 + " | Op Revision:" + data.OpRevNo);
        //OperationGridMc.jqGrid('setCaption', " OPS Detail - Style: " + data.StyleCode + " | Size: " + data.StyleSize
        //                       + " | Color: " + data.StyleColorSerial + " | Revision: " + data.RevNo + "|  Edtion: " + data.Edition2 + " | Op Revision:" + data.OpRevNo);

        Operation_Grid.jqGrid('setCaption', data.StyleCode + ' - ' + data.BuyerStyleName + " | " + data.StyleSize
            + ' | ' + data.StyleColorWays + " | REV " + data.RevNo + " | OPREV - " + data.OpRevNo + ' | Edition - ' + data.Edition2);
        OperationGrid.jqGrid('setCaption', data.StyleCode + ' - ' + data.BuyerStyleName + " | " + data.StyleSize
            + ' | ' + data.StyleColorWays + " | REV " + data.RevNo + " | OPREV - " + data.OpRevNo + ' | Edition - ' + data.Edition2);
        OperationGridMc.jqGrid('setCaption', data.StyleCode + ' - ' + data.BuyerStyleName + " | " + data.StyleSize
            + ' | ' + data.StyleColorWays + " | REV " + data.RevNo + " | OPREV - " + data.OpRevNo + ' | Edition - ' + data.Edition2);
    }
}

//START ADD - Son Nguyen Cao
function BeforeSelectRowGridOpMaster() {
    return true;
}
//END ADD - Son Nguyen Cao

function OpsMasterFunction(parentdata) {
    SetValueForLanguage("drpLanguages", MapLanguageToFlag(parentdata.Language));
    data = parentdata;
    editionRow = !isEmpty(data.Edition) ? data.Edition.slice(0, 1).toUpperCase() : "";
    confChk = data.ConfirmChk;
    var type;
    if (tab === "#patternLinking") {
        type = "1";
    } else if (tab === "#toolLinking") {
        type = "2";
    } else {
        type = "3";
    }
    var param = JSON.stringify({
        styleCode: data.StyleCode,
        styleSize: data.StyleSize,
        styleColor: data.StyleColorSerial,
        revNo: data.RevNo,
        opRevNo: data.OpRevNo,
        edition: data.Edition,
        type: type
    });
    if (CheckEdit()) {
        ShowYesNoConFirmItem("001", SmsFunction.Add, MessageType.Confirm, MessageContext.Confirm,//5
            function () {
                if (tab === "#patternLinking") {
                    SaveProt(false);
                } else if (tab === "#toolLinking") {
                    SaveTools(false);
                } else {
                    SaveMachine(false);
                }
                RemoveAllThing(param);
            },
            function () {
                RemoveAllThing(param);
                return;
            }
        );//5
    }
    else {
        RemoveAllThing(param);
    }

}

function RemoveAllThing(param) {
    $.ajax({
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST",
        url: "/OpsLink/RemoveAll",
        data: param,
        success: function () {
            CheckRoleCurrent(EditOnly);
            ReLoadGrid(tab);
            ResetVariable();
            ResetVariableTools();
        }
    });
}

function RegisterTabClick() {
    $('a[data-toggle="tab"]').on('shown.bs.tab',
        function (e) {
            var target = $(e.target).attr("href"); // activated tab
            tab = target;
            UpdateChangeTab();
            ReLoadGrid(tab);
        });
}

function ShowHideTab() {
    $("#jsPanel-Tool,#jsPanel-Tool-min").hide();
    $("#jsPanel-Bom,#jsPanel-Bom-min").hide();
    $("#jsPanel-Machine,#jsPanel-Machine-min").hide();
    if (tab === "#patternLinking") {
        $("#jsPanel-Bom,#jsPanel-Bom-min").show();
        if (!$.isEmptyObject(data)) {
            var param = JSON.stringify({
                styleCode: data.StyleCode,
                styleSize: data.StyleSize,
                styleColor: data.StyleColorSerial,
                revNo: data.RevNo,
                opRevNo: data.OpRevNo,
                edition: data.Edition,
                type: "1"
            });
            $.ajax({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                type: "POST",
                url: "/OpsLink/RemoveAll",
                data: param,
                success: function () {
                    SelectRow(data.StyleCode, data.StyleSize, data.StyleColorSerial, data.RevNo, data.OpRevNo, data.Edition);
                }
            });
        }
    } else if (tab === "#toolLinking") {
        $("#jsPanel-Tool,#jsPanel-Tool-min").show();
        if (!$.isEmptyObject(data)) {
            SelectRowTools(data.StyleCode, data.StyleSize, data.StyleColorSerial, data.RevNo, data.OpRevNo, data.Edition);
        }
    } else {
        $("#jsPanel-Machine,#jsPanel-Machine-min").show();
        if (data !== null) {
            SelectRowMachine(data.StyleCode, data.StyleSize, data.StyleColorSerial, data.RevNo, data.OpRevNo, data.Edition);
        }
    }
    /// need to remove all thing??
}

function CheckEdit() {
    if (CheckDragg()) {
        return _checkEdit;
    }
    else {
        return false;
    }
}

function ChangePage() {
    //$("#linkOfOpsLink").removeAttr("href");
    var newhref = "/OpsLink/index";
    $("a.linkOfOpsLink").on('click', function (e) {
        if (CheckEdit()) {
            newhref = $(this).attr("href");
            e.preventDefault();
            ShowYesNoConFirmItem("001", SmsFunction.Add, MessageType.Confirm, MessageContext.Confirm,
                function () {
                    if (tab === "#patternLinking") {
                        SaveProt(false);
                    } else if (tab === "#toolLinking") {
                        SaveTools(false);
                    } else {
                        SaveMachine(false);
                    }
                    ChangeLink(newhref);
                },
                function () {
                    ChangeLink(newhref);
                    return;
                }
            );
        }

    });
}

function ChangeLink(link) {
    setTimeout(function () {
        window.location = link;
    }, 500);
}

function UpdateChangeTab() {

    if (CheckEdit()) {
        ShowYesNoConFirmItem("001", SmsFunction.Add, MessageType.Confirm, MessageContext.Confirm,
            function () {
                if (oldTab === "#patternLinking") {
                    // save old tools or machine
                    SaveProt(false);
                } else if (oldTab === "#toolLinking") {
                    SaveTools(false);
                } else {
                    SaveMachine(false);
                }
                ShowHideTab();
                ResetVariable();
                ResetVariableTools();
            },
            function () {
                ShowHideTab();
                ResetVariable();
                ResetVariableTools();
            }
        );
    }
    else {
        ShowHideTab();
        ResetVariable();
        ResetVariableTools();
    }
    if (!$.isEmptyObject(data)) {
        CheckRoleCurrent(EditOnly);
    } else {
        CheckRoleCurrent(EditOnly);
    }

}

function ReLoadGrid(tabCurrent) {
    var languageId = $("#drpLanguages").val();
    setTimeout(function () {
        if (!$.isEmptyObject(data)) {
            if (tabCurrent === "#patternLinking") {
                SelectRow(data.StyleCode,
                    data.StyleSize,
                    data.StyleColorSerial,
                    data.RevNo,
                    data.OpRevNo,
                    data.Edition, languageId);
            } else if (tabCurrent === "#toolLinking") {
                SelectRowTools(data.StyleCode,
                    data.StyleSize,
                    data.StyleColorSerial,
                    data.RevNo,
                    data.OpRevNo,
                    data.Edition, languageId);
            } else {
                SelectRowMachine(data.StyleCode,
                    data.StyleSize,
                    data.StyleColorSerial,
                    data.RevNo,
                    data.OpRevNo,
                    data.Edition, languageId);
            }
        }
    }, 200);
    SetTiTle();
}

function CreateGridOpsDetail() { }

//function GetKeyCodeStyle(styleCode, styleSize, styleColorSerial, revNo) {
//    alertResult = true;
//   // ReloadParentGrid(styleCode, styleSize, styleColorSerial, revNo);
//}

//function ReloadParentGrid(styleCode, styleSize, styleColorSerial, revNo) {
//    $(gridOpsTableId).jqGrid("setGridParam", {
//        postData: {
//            styleCode: styleCode, styleSize: styleSize
//            , styleColor: styleColorSerial, revNo: revNo
//        }
//    }).trigger("reloadGrid");
//}

function GetStyleMaster() { }

function ShowObjectLinking() {
    ShowBomPattern();
    ShowTools();
    ShowMachine();
    // hidden Tools for first ready
    $("#jsPanel-Tool,#jsPanel-Tool-min").hide();
    $("#jsPanel-Machine,#jsPanel-Machine-min").hide();
}

function ShowBomPattern() {
    $.jsPanel({
        id: "jsPanel-Bom",
        setstatus: "minimize", //
        headerControls: { close: "disable", maximize: "remove" },
        theme: "rebeccapurple",
        contentSize: { width: 1000, height: 350 },
        headerTitle: "BOM & PATTERNS",
        content: "<table id='Pattern_Grid' ></table>",
        callback: function () {
            this.content.css("padding", "15px");
        },
        onnormalized: function () {
            ResizeGridOnpanel("jsPanel-Bom", Pattern_Grid_TB);
        },
        resizable: {
            stop: function () {
                ResizeGridOnpanel("jsPanel-Bom", Pattern_Grid_TB);
            }
        },
        onmaximized: function () {
            ResizeGridOnpanel("jsPanel-Bom", Pattern_Grid_TB);
        }
    });
}

function ShowTools() {
    $.jsPanel({
        id: "jsPanel-Tool",
        setstatus: "minimize", //
        headerControls: { close: "disable", maximize: "remove" },
        theme: "rebeccapurple",
        contentSize: { width: 900, height: 350 },
        headerTitle: "Tools",
        content: "<div><select id='cbTool' class='form-control' data-placeholder='Select category'></select></div><table id='ToolsGrid' ></table>",
        callback: function () {
            this.content.css("padding", "15px");
        },
        resizable: {
            stop: function () {
                ResizeGridOnpanel("jsPanel-Tool", ToolsGrid);
            }
        },
        onnormalized: function () {
            ResizeGridOnpanel("jsPanel-Tool", ToolsGrid);
        },
        onmaximized: function () {
            ResizeGridOnpanel("jsPanel-Tool", ToolsGrid);
        }
    });
}

function ShowMachine() {
    $.jsPanel({
        id: "jsPanel-Machine",
        setstatus: "minimize", //
        headerControls: { close: "disable", maximize: "remove" },
        theme: "rebeccapurple",
        contentSize: { width: 900, height: 350 },
        headerTitle: "Machines",
        content: "<div><select id='cbMachine' class='form-control' data-placeholder='Select category'></select></div><table id='MachineGrid' ></table>",
        callback: function () {
            this.content.css("padding", "15px");
        },
        resizable: {
            stop: function () {
                ResizeGridOnpanel("jsPanel-Machine", MachineGrid);
            }
        },
        onnormalized: function () {
            ResizeGridOnpanel("jsPanel-Machine", MachineGrid);
        },
        onmaximized: function () {
            ResizeGridOnpanel("jsPanel-Machine", MachineGrid);
        }
    });
}

function ResizeGridOnpanel(toolName, gridName) {
    var h = $("#" + toolName).height() - 150;
    if (h > 250) {
        $("#" + gridName).setGridHeight(h);
    } else {
        $("#" + gridName).setGridHeight(250);
    }
}

function UpdateProt() {
    $("#dvPatternLink .btnEdit").click(function () {

        oldTab = tab;
        window.checkDragg = true;
        window.SetMenuActionMode("dvPatternLink", Update, UserRole);
    });
    $("#dvPatternLink .btnSave").click(function () {
        if (tab === "#patternLinking") {
            SaveProt(true);
        } else if (tab === "#toolLinking") {
            SaveTools(true);
        } else {
            SaveMachine(true);
        }
    });
    $("#dvPatternLink .btnCancel").click(function () {
        ShowYesNoConFirmItem("002", SmsFunction.Confirm, MessageType.Confirm, MessageContext.Confirm,//18
            function () {
                _checkEdit = false;
                $.ajax({
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    type: "POST",
                    url: "/OpsLink/RemoveAll",
                    data: {},
                    success: function () {
                        if (tab === "#patternLinking") {
                            GetBomtExpand();
                            GetOperationParentExpand();
                            GetOperationExpand();
                            Operation_Grid.trigger("reloadGrid");
                            $("#" + Pattern_Grid_TB).trigger("reloadGrid");
                            ResetVariable();
                        } else if (tab === "#toolLinking") {
                            GetParentExpand();
                            GetExpand();
                            OperationGrid.trigger("reloadGrid");
                            //$("#" + ToolsGrid).trigger("reloadGrid");
                            ResetVariableTools();
                        }
                        else {
                            GetParentMcExpand();
                            GetMcExpand();
                            OperationGridMc.trigger("reloadGrid");
                            ResetVariableMachine();
                        }

                    }
                });
                window.SetMenuActionMode("dvPatternLink", EditOnly, UserRole);
            },
            function () {
                return;
            }
        );
    });
}

function CheckedChekBox() {
    $("#cbInput").change(function () {
        if (this.checked) {
            $("#cbOutput").prop("checked", false);
        }
    });
    $("#cbOutput").change(function () {
        if (this.checked) {
            $("#cbInput").prop("checked", false);
        }
    });
}

function ConfirmRemove(funct) {
    ShowYesNoConFirmItem("002", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges,
        funct,
        function () {
            return;
        }
    );//15
}

function ChangeCbTools() {
    $("#cbTool").change(function () {
        jQuery("#ToolsGrid").jqGrid("clearGridData");
        $("#ToolsGrid").jqGrid("setGridParam",
            {
                postData: {
                    gId: $(this).val()
                }
            }).trigger("reloadGrid");
    });
    $("#cbMachine").change(function () {
        jQuery("#MachineGrid").jqGrid("clearGridData");
        $("#MachineGrid").jqGrid("setGridParam",
            {
                postData: {
                    gId: $(this).val()
                }
            }).trigger("reloadGrid");
    });
}

function ShowHideButtonByTab(count, value) {
    if (count <= 0 && value === tab) {
        // window.SetMenuActionMode("dvPatternLink", ReadOnly, UserRole);
    } else {
        // window.SetMenuActionMode("dvPatternLink", EditOnly, UserRole);
    }
}

function isNumberKey(keyid) {
    var value = $("#" + keyid).val().length;
    if (keyid === "txtUnitConsumption") {
        if (value >= 8) {
            return false;
        }
    } else {
        if (value >= 3)
            return false;
    }
    return true;
}
//================================
//change group
function SelectChangeGroupShow() {
    $("#drpGroupShow").change(function () {
        var showVal = $(this).val();
        if (CheckEdit()) {
            ShowYesNoConFirmItem("001", SmsFunction.Add, MessageType.Confirm, MessageContext.Confirm,//5
                function () {
                    if (tab === "#patternLinking") {
                        SaveProt(true);
                    } else if (tab === "#toolLinking") {
                        SaveTools(true);
                    } else {
                        SaveMachine(true);
                    }
                },
                function () {
                    return;
                }
            );
        }
        ChangeGroupingJqGrid("#" + OperationGridTbMc, showVal);
        ChangeGroupingJqGrid("#" + Operation_Grid_TB, showVal);
        ChangeGroupingJqGrid("#" + OperationGridTb, showVal);
    });
}

///

function ShowYesNoConFirmItem(id, event, type, context, funct, funtNo, strMsg) {
    GetMsg(id, Ops, Obl, event, type, context, language, function (result) {
        var value = ReplaceStr(result.value, strMsg);
        ShowConfirmYesNo(result.title, value,
            funct,
            funtNo
        );
    });
}

// show msg

function ShowValidateByItem(id, event, mtype, context, msgtype, strMsg) {
    GetMsg(id, Ops, Obl, event, mtype, context, language, function (result) {
        var value = ReplaceStr(result.value, strMsg);
        ShowAlertByTime(result.title, value, msgtype, 3000);
    });
}

function CheckMain(arrTool) {
    var rs = false;
    arrTool.forEach(function (e) {
        if (e.MainTool === "1") {
            rs = true;
            return false;
        }
    });
    return rs;
}