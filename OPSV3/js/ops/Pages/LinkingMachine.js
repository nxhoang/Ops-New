var OperationGridMc = $("#OperationGridMc");
var OperationGridTbMc = "OperationGridMc";
var MachineGrid = "MachineGrid";
var rowDropMc = null;
var toolckMc = false;
var machine = [];
var _epMc = false;
var _eppMc = false;
var _expandPMc = [];
var _expandMc = [];
var arrMc = {
    ItemCode: "Machine Code",
    ItemName: "Machine Name",
    ImagePath: "Machine Image",
    CategId: "Category",
    Category: "Category",
    Buyer: "Buyer",
    Brand: "Brand",
    Machine: "Machine",
    MainTool: "Main Machine"
};
function CreateOperationMcGrid(styleCode, styleSize, styleColorSerial, revNo, edition, languageId) {
    OperationGridMc.jqGrid({
        datatype: "json",
        height: 450,
        width: null,
        shrinkToFit: false,
        rowNum: 100000,
        rownumbers: true,
        gridview: false,
        //==========================================
        url: "/OpsLink/GetOpDetailMachine",
        caption: "Machines",
        postData: {
            styleCode: styleCode, size: styleSize, serial: styleColorSerial, revNo: revNo, edition: edition, languageId: languageId
        },
        //mtype: 'POST',
        colModel: [
            {
                name: "OpGroupName",
                index: "OpGroupName",
                label: arrColNameOpsDetail.OPGROUPNAME,
                search: false,
                sort: false
            },
            {
                name: 'ModuleName',
                index: 'ModuleName',
                label: arrColNameOpsDetail.MODULENAME,
                hidden: true,
                searchoptions: { sopt: ['cn', 'eq', 'ne'] },
                classes: 'pointer'
            },
            {
                name: "OpNum",
                index: "OpNum",
                width: 70,
                label: arrColNameOpsDetail.OPNUM,
                search: false,
                sort: false
            },
            {
                name: "OpNameLan",
                index: "OpNameLan",
                width: 250,
                label: arrColNameOpsDetail.OPNAME,
                search: false,
                sort: false
            },
            {
                name: "OpTime",
                index: "OpTime",
                width: 130,
                label: arrColNameOpsDetail.OPTIME,
                align: "center"
            },
            {
                name: "OpPrice",
                index: "OpPrice",
                width: 80,
                label: arrColNameOpsDetail.OPPRICE,
                align: "center",
                search: false,
                sort: false,
                hidden: true //Ha add
            },
            {
                name: "MachineName",
                index: "MachineName",
                width: 250,
                label: arrColNameOpsDetail.MACHINETYPENAME,
                align: "center",
                search: false,
                sort: false
            },
            {
                name: "MachineCount",
                index: "MachineCount",
                width: 80,
                label: arrColNameOpsDetail.MACHINECOUNT,
                align: "center",
                search: false,
                sort: false
            },
            //START ADD - SON) 14/Mar/2020
            {
                name: "Remarks",
                index: "Remarks",
                width: 250,
                label: "Remarks",
                search: false,
                sort: false
            },
            //END ADD - SON) 14/Mar/2020
            {
                name: "OfferPrice",
                index: "OfferPrice",
                width: 135,
                label: arrColNameOpsDetail.OFFEROPPRICE,
                align: "center",
                search: false,
                sort: false,
                hidden: true //Ha add
            },
            {
                name: "Maxtime",
                index: "Maxtime",
                width: 75,
                label: arrColNameOpsDetail.MAXTIME,
                align: "center",
                search: false,
                sort: false
            },
            {
                name: "BenchmarkTime",
                index: "BenchmarkTime",
                width: 150,
                label: arrColNameOpsDetail.BENCHMARKTIME,
                align: "center",
                search: false,
                sort: false
            },
            { name: "StyleCode", index: "StyleCode", hidden: true },
            { name: "StyleSize", index: "StyleSize", hidden: true },
            { name: "StyleColorSerial", index: "StyleColorSerial", hidden: true },
            { name: "RevNo", index: "RevNo", hidden: true },
            { name: "OpRevNo", index: "OpRevNo", hidden: true },
            { name: "OpSerial", index: "OpSerial", hidden: true },
            { name: "OpType", index: "OpType", hidden: true },
            { name: "MachineType", index: "MachineType", hidden: true },
            { name: "ToolId", index: "ToolId", hidden: true },
            { name: "NewPrevNo", index: "NewPrevNo", hidden: true }
        ],
        grouping: true,
        groupingView: {
            groupField: ["OpGroupName"],
            groupColumnShow: [false],
            groupText: ["Group Name: {0} - {1} Item(s)"],
            groupCollapse: true,
            plusicon: "ace-icon fa fa-plus",
            minusicon: "ace-icon fa fa-minus"
        },
        subGrid: true,
        subGridRowExpanded: ShowGridchildMc,
        subGridOptions: {
            plusicon: "ace-icon fa fa-plus",
            minusicon: "ace-icon fa fa-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: true
        },
        gridComplete: function () {
            $('td[aria-describedby="OperationGridMc_NewPrevNo"]').each(function () {
                var x = $(this).html();
                if (!$(this).is(".jqgroup,.jqgfirstrow,.ui-subgrid")) {
                    $(this).parent().addClass("machine-droppable");
                }
                if (x === "&nbsp;") {
                    $(this).parent().find('td[aria-describedby="OperationGridMc_subgrid"]').unbind("click").html("");
                }
            });
            if (_epMc) {
                var x = CheckEpandRowDrop(rowDropMc, _expandMc);
                ExpandMc();
                if (!x) {
                    OperationGridMc.find('tr[id="' + rowDropMc + '"]').find('td[aria-describedby="OperationGridMc_subgrid"]').trigger("click");
                }
            }
            DroppRowMc();
        }
    });
}

function CreateMcGrid(gId) {
    $("#" + MachineGrid).jqGrid({
        datatype: "json",
        height: 250,
        shrinkToFit: false,
        width: null,
        rownumbers: true,
        multiselect: true,
        pginput: false,
        rowNum: 100000,
        //==========================================
        url: "/OpsLink/GetOtmtsMc",
        postData: {
            gId: gId
        },
        //mtype: 'POST',
        colModel: [
            {name: "CategId", index: "CategId", label: arrMc.CategId, width: 100, hidden: true},
            {
                name: 'MainTool', index: "MainTool", label: arrMc.MainTool, width: 50,
                formatter: function (cellValue, option) {
                    return '<input type="radio" name="radio_' + option.gid + '"  />';
                }
            },
            { name: "Category", index: "Category", label: arrMc.Category, width: 300, search: false, sort: false },
            { name: "ItemCode", index: "ItemCode", label: arrMc.ItemCode, search: false, sort: false },
            { name: "ItemName", index: "ItemName", label: arrMc.ItemName, width: 300 },
            {
                name: "ImagePath", index: "ImagePath", label: arrMc.ImagePath, align: "center", formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return "<img id='" + id +
                            "' class='imgpattern' onclick = 'ShowPatternImage(\"" + cellvalue +
                            "\");' src='" +
                            cellvalue +
                            "' onerror='imgError(this);'/>";
                    return "";
                }
            },
            {
                name: "Img", index: "Img", hidden: true
                , formatter: function (cellvalue, options, rowObject) {
                    return rowObject.ImagePath;
                }
            },
            { name: "Brand", index: "Brand", label: arrMc.Brand, hidden: true }
        ],
        onCellSelect: function (id, cellidx) {
            var cm = $("#" + MachineGrid).jqGrid("getGridParam", "colModel");
            var colNameAttr = cm[cellidx].name;
            if (colNameAttr === "ImagePath") {
                toolckMc = $("#jqg_" + MachineGrid + "_" + id).is(":checked");
                setTimeout(function () {
                    $("#jqg_" + MachineGrid + "_" + id).prop("checked", toolckMc);
                }, 1);
            } else {
                toolckMc = false;
            }
        },
        afterInsertRow: function (rowid, rowdata) {
            $("#" + rowid, $("#" + MachineGrid)).addClass("machine-draggable");
        },
        gridComplete: function () {
            DragRowMc();
        }
    });
}

function SelectRowMachine(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId) {
    OperationGridMc.jqGrid("setGridParam", {
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColor: styleColorSerial, revNo: revNo, opRevNo: opRevNo, edition: edition, languageId: languageId
        }
    }).trigger("reloadGrid");

    $("#" + MachineGrid).jqGrid("setGridParam", {
        postData: {
            gId: $("#cbMachine").val()
        }
    }).trigger("reloadGrid");
}

function SaveMachine(isAlert) {
    if (!CheckDragg()) {
        return;
    }
    $.ajax({
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST",
        url: "/OpsLink/SaveMachine",
        data: {},
        success: function (data) {
            _checkEdit = false;
            if (isAlert) {
                if (data === 1 || data === 2) {
                    ShowValidateByItem("001", SmsFunction.Add, MessageType.Success, MessageContext.Update, Type.Success);// 17
                } else {
                    ShowValidateByItem("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, Type.Error);//14
                }
                GetParentMcExpand();
                GetMcExpand();
                OperationGridMc.trigger("reloadGrid");
            }
        },
        err: function () {
            ShowValidateByItem("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, Type.Error);//14
        }
    });
    machine = [];
}

function ShowGridchildMc(subgridId, rowId) {
    var row = OperationGridMc.getRowData(rowId);
    var MachineType = row.MachineType;
    var sCode = row.StyleCode;
    var sSize = row.StyleSize;
    var sColorSerial = row.StyleColorSerial;
    var rNo = row.RevNo;
    var opRevNo = row.OpRevNo;
    var opSerial = row.OpSerial;
    var selRowId = $(grid_selector).jqGrid("getGridParam", "selrow");
    var edition = $(grid_selector).jqGrid("getCell", selRowId, "Edition").charAt(0);
    var subgridTableId = subgridId + "_t";
    $("#" + subgridId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: "/OpsLink/GetOptlMc?styleCode=" + sCode + "&styleSize=" + sSize +
            "&styleColor=" + sColorSerial + "&revNo=" + rNo +
        "&opRevNo=" + opRevNo + "&opSerial=" + opSerial + "&edition=" + edition,
        datatype: "json",
        page: 1,
        colModel: [
            { name: "", index: "", width: 30, sortable: false, formatter: RemoveIconMc, align: "center" },
            {
                name: 'MainTool', index: "MainTool", label: arrMc.MainTool, width: 50,
                formatter: function (cellvalue, options, rowObject) {
                    let checked = "";
                    if (MachineType === rowObject.ItemCode) {
                        checked = 'checked = "checked"';
                    } else {
                        checked = "";
                    }
                    return '<input  onclick="SelectMainMc(this, event,' + rowId + ',\'' + rowObject.ItemCode + '\',\'' + rowObject.ItemName + '\');"'
                            + checked + 'type="radio"  name="radio_' + options.gid + '"  />';
                }
            },
            { label: arrMc.CategId, name: "CategId", index: "CategId", width: 90, hidden: true },
            { name: "Category", index: "Category", label: arrMc.Category, width: 180, sort: false },
            { label: arrMc.ItemCode, name: "ItemCode", index: "ItemCode", width: 120 },
            { label: arrMc.ItemName, name: "ItemName", index: "ItemName", width: 300 },
            {
                label: arrMc.ImagePath,
                name: "ImagePath",
                index: "ImagePath",
                width: 100,
                align: "center",
                formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return "<img id='" +
                            id +
                            "' class='imgpattern' onclick = 'ShowPatternImage(\"" + cellvalue +
                            "\");'; src='" +
                            cellvalue +
                            "'  onerror='imgError(this);'/>";
                    return "";
                }
            },
            { name: "Machine", index: "Machine", hidden: true },
            { name: "ItemCode", index: "ItemCode", hidden: true },
            { name: "Edition", index: "Edition", hidden: true }
        ],
        viewrecords: true,
        loadonce: false,
        height: "100%",
        rowNum: 100000,
        rownumbers: true,
        gridComplete: function () {
            $("#" + subgridId + "_t_").html("<a title ='Remove' class='memberAction' style='padding-left: 7px;'  onclick=\"RemoveAllMc(this,'" + rowId + "')\">x</a>");
            $("#" + subgridId).addClass("machineChild-droppable");
            McChildDraggAble(rowId);
        },
        pager: "#jqGridPager" + "_" + subgridId
    });
}

function RemoveIconMc(cellvalue, options, rowObject) {
    var id = options.rowId;
    var refNo = rowObject.OpSerial;
    var html = "<a title ='Remove'  id='" + id + "' onclick=\"RemoveRowMc(this,'" + id + "')\" class='memberAction' name='" + refNo + "' type='button'>X</a>";
    return html;
}

function RemoveRowMc(thing, id) {
    if (!CheckDragg()) {
        return;
    }
    var main = null;
    var grid = $(thing).parent().parent().parent().parent().attr("id");
    var arr = grid.split("_");
    var rowParrent = arr[1];
    var rowDataP = OperationGridMc.jqGrid("getRowData", rowParrent);
    var checkmain = $("#" + grid).find("tr[id='" + id + "']").find("input[type='radio']:checked").val();
    var record = jQuery("#" + grid).jqGrid('getGridParam', 'records');
    var rowid = $(grid_selector).jqGrid("getGridParam", "selrow");
    
    if (checkmain === "on") {
        if (record > 2) {
            ShowValidateByItem("001", SmsFunction.Delete, MessageType.Error, MessageContext.Communication, Type.Success, "machine");//9
            return;
        }
        if (record === 1 || record === 2) {
            var mainMc = record === 1 ? "0" : "1";
            var rowData = OperationGridMc.jqGrid("getRowData", rowParrent);
            $("#" + grid).find('tr[role="row"]').find("input[type='radio']").each(function () {
                    var check = $(this).val();
                    if (!$(this).is(':checked')) {
                        var rowPid = $(this).parent().parent().attr("id");
                        var ChildData = $("#" + grid).jqGrid("getRowData", rowPid);
                        main = {
                            styleCode: rowData.StyleCode,
                            styleSize: rowData.StyleSize,
                            styleColorSerial: rowData.StyleColorSerial,
                            revno: rowData.RevNo,
                            OpRevNo: rowData.OpRevNo,
                            OpSerial: rowData.OpSerial,
                            Machine: "0",
                            ItemCode: ChildData.ItemCode,
                            ItemName: ChildData.ItemName,
                            CategId: ChildData.CategId,
                            Category: ChildData.Category,
                            MainTool: mainMc,
                            Edition: $(grid_selector).jqGrid("getCell", rowid, "Edition")
                        };
                        return false;
                    }
                });
        }
    }

    ConfirmRemove(function () {
        _checkEdit = true;
        var rowChild = $("#" + grid).jqGrid("getRowData", id);
        var tool = {
            styleCode: rowDataP.StyleCode,
            styleSize: rowDataP.StyleSize,
            styleColorSerial: rowDataP.StyleColorSerial,
            revno: rowDataP.RevNo,
            OpRevNo: rowDataP.OpRevNo,
            OpSerial: rowDataP.OpSerial,
            ItemCode: rowChild.ItemCode,
            Edition: rowChild.Edition
        };
        machine.push(tool);
        var ltool = JSON.stringify({ 'tools': machine });
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/RemoveSessionMc",
            data: ltool,
            success: function (data) {
                if (main) {
                    // save main
                    var urlUpdate = "/OpsLink/SaveMainToSession";
                    if (record === 1) {
                        urlUpdate = "/OpsLink/AddMainRemove";
                    }
                    var opToolConfig = {
                        url: urlUpdate,
                        postData: JSON.stringify(main)
                    };
                    AjaxPostCommon(opToolConfig, function (response) {
                        GetParentMcExpand();
                        GetMcExpand();
                        OperationGridMc.trigger("reloadGrid");
                        main = null;
                    });
                } else {
                    GetParentMcExpand();
                    GetMcExpand();
                    OperationGridMc.trigger("reloadGrid");
                }
                _checkEdit = true;
            }
        });
        machine = [];
    });
}

function RemoveAllMc(thing, id) {
    if (!CheckDragg()) {
        return;
    }
    ConfirmRemove(function () {
        _checkEdit = true;
        var rowid = $(grid_selector).jqGrid("getGridParam", "selrow");
        var Edition = $(grid_selector).jqGrid("getCell", rowid, "Edition");
        var rowDataP = OperationGridMc.jqGrid("getRowData", id);
        var grid = OperationGridTbMc + "_" + id + "_t";
        $("#" + grid).find('tr[role="row"]').each(function () {
            if ($(this).is(".jqgfirstrow")) {
                return;
            }
            var idchild = $(this).attr("id");
            var ChildData = $("#" + grid).jqGrid("getRowData", idchild);
            var tool = {
                styleCode: rowDataP.StyleCode,
                styleSize: rowDataP.StyleSize,
                styleColorSerial: rowDataP.StyleColorSerial,
                revno: rowDataP.RevNo,
                OpRevNo: rowDataP.OpRevNo,
                OpSerial: rowDataP.OpSerial,
                ItemCode: ChildData.ItemCode,
                Edition: ChildData.Edition
            };
            machine.push(tool);
        });
        //Close before remove
        OperationGridMc.find('tr[id="' + id + '"]').find('td[aria-describedby="OperationGridMc_subgrid"]').trigger("click");
        var ltool = JSON.stringify({ 'tools': machine });
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/RemoveSessionMc",
            data: ltool,
            success: function (data) {
                if (data >= 0) {
                    var main = {
                        styleCode: rowDataP.StyleCode,
                        styleSize: rowDataP.StyleSize,
                        styleColorSerial: rowDataP.StyleColorSerial,
                        revno: rowDataP.RevNo,
                        OpRevNo: rowDataP.OpRevNo,
                        OpSerial: rowDataP.OpSerial,
                        Machine: "0",
                        ItemCode: "",
                        ItemName: "",
                        CategId: "",
                        Category: "",
                        MainTool: "0",
                        Edition: Edition
                    };
                    var opToolConfig = {
                        url: "/OpsLink/AddMainRemove",
                        postData: JSON.stringify(main)
                    };
                    AjaxPostCommon(opToolConfig, function (response) {
                        GetParentMcExpand();
                        GetMcExpand();
                        OperationGridMc.trigger("reloadGrid");
                    });
                } else {
                    GetParentMcExpand();
                    GetMcExpand();
                    OperationGridMc.trigger("reloadGrid");
                }
                _checkEdit = true;
            }
        });
        machine = [];
    });
}

function DroppRowMc() {
    $(".machine-droppable").droppable({
        accept: ".machine-draggable",
        activeClass: "dropactive",
        hoverClass: "drophover",
        tolerance: "pointer",
        drop: function () {
            if (!CheckDragg()) {
                return false;
            }
            rowDropMc = $(this).attr("id");
            SaveToSessionMc();
            return true;
        }
    });

}

function SelectMainMc(thing, event, rowid, itemCode, itemName) {
    if (!CheckDragg()) {
        event.preventDefault();
        //$(thing).prop('checked', false);
        return;
    }
    ShowYesNoConFirmItem("001", SmsFunction.Update, MessageType.Confirm, MessageContext.Confirm,//12
            function () {
                $(thing).prop('checked', true);
                var prowid = $(grid_selector).jqGrid("getGridParam", "selrow");
                var Edition = $(grid_selector).jqGrid("getCell", prowid, "Edition");
                var rowData = OperationGridMc.jqGrid("getRowData", rowid);
                var main = {
                    styleCode: rowData.StyleCode,
                    styleSize: rowData.StyleSize,
                    styleColorSerial: rowData.StyleColorSerial,
                    revno: rowData.RevNo,
                    OpRevNo: rowData.OpRevNo,
                    OpSerial: rowData.OpSerial,
                    ItemCode: itemCode,
                    ItemName: itemName,
                    CategId: "",
                    Category: "",
                    MainTool: "1",
                    Edition: Edition
                };
                var opToolConfig = {
                    url: "/OpsLink/SaveMainToSession",
                    postData: JSON.stringify(main)
                };
                AjaxPostCommon(opToolConfig, function (response) {
                    if (response === ReportAction.Error) {
                        ShowValidateByItem("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, Type.Error);//14
                    }
                    else {
                        OperationGridMc.jqGrid("setCell", rowid, "MachineType", main.ItemCode);
                        _checkEdit = true;
                        OperationGridMc.jqGrid("setCell", rowid, "MachineName", main.ItemName);
                    }
                });
            },
            function () {
                event.preventDefault();
                return;
            }, " Machine? "
            );//12
    event.preventDefault();
    return false;
}

function SaveToSessionMc() {
    _checkEdit = true;
    var rowid = $(grid_selector).jqGrid("getGridParam", "selrow");
    var Edition = $(grid_selector).jqGrid("getCell", rowid, "Edition");
    var rowData = OperationGridMc.jqGrid("getRowData", rowDropMc);
    var MachineType = rowData.MachineType;
    $("#" + MachineGrid).find('tr[role="row"]:not(.ui-jqgrid-labels)').find("input:checkbox:checked")
        .each(function () {
            var rowPid = $(this).parent().parent().attr("id");
            var ChildData = $("#" + MachineGrid).jqGrid("getRowData", rowPid);
            var rd = $(this).parent().parent().find("input[name='radio_MachineGrid']");
            var tool = {
                styleCode: rowData.StyleCode,
                styleSize: rowData.StyleSize,
                styleColorSerial: rowData.StyleColorSerial,
                revno: rowData.RevNo,
                OpRevNo: rowData.OpRevNo,
                OpSerial: rowData.OpSerial,
                Machine: "1",
                ItemCode: ChildData.ItemCode,
                ItemName: ChildData.ItemName,
                CategId: ChildData.CategId,
                Category: ChildData.Category,
                ImagePath: ChildData.Img,
                MainTool: rd.is(":checked") ? "1" : "0",
                Edition: Edition
            };
            machine.push(tool);
            return true;
        });
    var len = machine.length;
    if ($.isEmptyObject(MachineType) && len > 1) {
        if (!CheckMain(machine)) {
            ShowValidateByItem("001", SmsFunction.Update, MessageType.Warning, MessageContext.IgnoreChanges, Type.Warning, " machine");//8
            machine = [];
            return false;
        }
    }
    if (len === 1 && $.isEmptyObject(MachineType)) {
        machine[0].MainTool = "1";
    }
    var ltool = JSON.stringify({ 'tools': machine });
    machine = [];
    $.ajax({
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST",
        url: "/OpsLink/AddSessionMc",
        data: ltool,
        success: function (data) {
            GetParentMcExpand();
            GetMcExpand();
            OperationGridMc.trigger("reloadGrid");
            _checkEdit = true;
        },
        err: function (response) {
            console.log(response);
        }
    });
}

function DragRowMc() {
    $(".machine-draggable").draggable({
        start: function () {
            $(this).css("z-index", 1001);
        },
        drag: function () {
            return CheckDragg();
        },
        revert: "invalid",
        cursor: "pointer",
        appendTo: "body",
        helper: function (event/*, ui*/) {
            if (!CheckDragg()) {
                return $("<div></div>");
            }
            $(this).find("input:checkbox").prop("checked", true);
            var html;
            var itemcode = event.delegateTarget.cells[3].textContent;
            var itemname = event.delegateTarget.cells[4].textContent;
            if (!CheckMuiltiple()) {
                html = "<div id='dragItemID' ><table class='table table-bordered'><tr><td>" + arrMc.ItemCode + "</td><td>" + itemcode + "</td><td>" + arrMc.ItemName + "</td><td>" + itemname + "</td></tr>";
            }
            else {
                html = htmlMuilty;
            }
            return $(html);
        },
        cursorAt: { left: 10, top: 10 }
    });
}

function McChildDraggAble(id) {
    $(".machineChild-droppable").droppable({
        accept: ".machine-draggable",
        activeClass: "dropactive",
        hoverClass: "drophover",
        tolerance: "pointer",
        drop: function () {
            if (!CheckDragg()) {
                return false;
            }
            rowDropMc = id;
            SaveToSessionMc();
            return true;
        }
    });
}

function GetParentMcExpand() {
    _eppMc = true;
    OperationGridMc.find("tr.jqgroup").find("span.fa-minus").each(function () {
        var id = $(this).parent().parent().attr("id");
        _expandPMc.push(id);
    });
}

function GetMcExpand() {
    _epMc = true;
    OperationGridMc.find('td.sgexpanded[aria-describedby="OperationGridMc_subgrid"]').each(function () {
        var id = $(this).parent().attr("id");
        _expandMc.push(id);
    });
}

function ExpandMc() {
    if (_epMc) {
        _expandPMc.forEach(function (a) {
            $("#" + a).find("span").trigger("click");
        });
        _expandMc.forEach(function (a) {
            OperationGridMc.find('tr[id="' + a + '"]').find('td[aria-describedby="' + OperationGridTbMc + '_subgrid"]').trigger("click");
        });
    }
    _expandPMc = [];
    _expandMc = [];
    _eppMc = false;
    _epMc = false;
}

function ResetVariableMachine() {
    checkDragg = false;
    machine = [];
    _epMc = false;
    _eppMc = false;
    _expandPMc = [];
    _expandMc = [];
    _checkEdit = false;
}