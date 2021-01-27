var OperationGrid = $("#OperationGrid");
var OperationGridTb = "OperationGrid";
var ToolsGrid = "ToolsGrid";
var rowDrop = null;
var toolck = false;
var tools = [];
var _ep = false;
var _epp = false;
var _expandP = [];
var _expand = [];
var arrTool = {
    ItemCode: "Tool Code",
    ItemName: "Tool Name",
    ImagePath: "Tool Image",
    CategId: "Category",
    Category: "Category",
    Buyer: "Buyer",
    Brand: "Brand",
    Machine: "Machine",
    MainTool: "Main Tool"
};

function CreateOperationToolsGrid(styleCode, styleSize, styleColorSerial, revNo, edition, languageId) {
    OperationGrid.jqGrid({
        datatype: "json",
        height: 450,
        width: null,
        shrinkToFit: false,
        rowNum: 100000,
        gridview: false,
        rownumbers: true,
        //==========================================
        url: "/OpsLink/GetOpDetailTools",
        caption: "Operation",
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
                name: "ToolName",
                index: "ToolName",
                width: 250,
                label: "Main Tool",
                align: "center",
                search: false,
                sort: false
            },
            {
                name: "ToolCount",
                index: "ToolCount",
                width: 80,
                label: "Tools",
                align: "center",
                search: false,
                sort: false
            },
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
            { name: "ToolId", index: "ToolId", hidden: true },
            { name: "MachineType", index: "MachineType", hidden: true },
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
        subGridRowExpanded: ShowGridchildTools,
        subGridOptions: {
            plusicon: "ace-icon fa fa-plus",
            minusicon: "ace-icon fa fa-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: true
        },
        gridComplete: function () {
            $('td[aria-describedby="OperationGrid_NewPrevNo"]').each(function () {
                var x = $(this).html();
                if (!$(this).is(".jqgroup,.jqgfirstrow,.ui-subgrid")) {
                    $(this).parent().addClass("tools-droppable");
                }
                if (x === "&nbsp;") {
                    $(this).parent().find('td[aria-describedby="OperationGrid_subgrid"]').unbind("click").html("");
                }
            });
            if (_ep) {
                var x = CheckEpandRowDrop(rowDrop, _expand);
                Expand();
                if (!x) {
                    OperationGrid.find('tr[id="' + rowDrop + '"]').find('td[aria-describedby="OperationGrid_subgrid"]').trigger("click");
                }
            }
            DroppRowTools();
        }
    });
}

function CreateToolsGrid(gId) {
    $("#" + ToolsGrid).jqGrid({
        datatype: "json",
        height: 250,
        shrinkToFit: false,
        width: null,
        rownumbers: true,
        multiselect: true,
        pginput: false,
        rowNum: 100000,
        //==========================================
        url: "/OpsLink/GetOtmts",
        postData: {
            gId: gId
        },
        //mtype: 'POST',
        colModel: [
            { name: "CategId", index: "CategId", label: arrTool.CategId, width: 100, hidden: true },
            {
                name: 'MainTool', index: "MainTool", label: arrTool.MainTool, width: 50,
                formatter: function (cellValue, option) {
                    return '<input type="radio" name="radio_' + option.gid + '"  />';
                }
            },
            { name: "Category", index: "Category", label: arrTool.Category, width: 300, search: false, sort: false },
            { name: "ItemCode", index: "ItemCode", label: arrTool.ItemCode },
            { name: "ItemName", index: "ItemName", label: arrTool.ItemName, width: 300 },
            {
                name: "ImagePath", index: "ImagePath", label: arrTool.ImagePath, align: "center", formatter: function (cellvalue, options) {
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
            { name: "Brand", index: "Brand", label: arrTool.Brand, hidden: true }
        ],
        onCellSelect: function (id, cellidx) {
            var cm = $("#" + ToolsGrid).jqGrid("getGridParam", "colModel");
            var colNameAttr = cm[cellidx].name;
            if (colNameAttr === "ImagePath") {
                toolck = $("#jqg_" + ToolsGrid + "_" + id).is(":checked");
                setTimeout(function () {
                    $("#jqg_" + ToolsGrid + "_" + id).prop("checked", toolck);
                }, 1);
            } else {
                toolck = false;
            }
        },
        afterInsertRow: function (rowid, rowdata) {
            $("#" + rowid, $("#" + ToolsGrid)).addClass("tools-draggable");
        },
        gridComplete: function () {
            DragRowTools();
        }
    });
}

function DroppRowTools() {
    $(".tools-droppable").droppable({
        accept: ".tools-draggable",
        activeClass: "dropactive",
        hoverClass: "drophover",
        tolerance: "pointer",
        drop: function () {
            if (!CheckDragg()) {
                return false;
            }
            rowDrop = $(this).attr("id");
            SaveToSession();
            return true;
        }
    });

}

function ToolsChildDraggAble(id) {
    $(".childtools-droppable").droppable({
        accept: ".tools-draggable",
        activeClass: "dropactive",
        hoverClass: "drophover",
        tolerance: "pointer",
        drop: function () {
            if (!CheckDragg()) {
                return false;
            }
            rowDrop = id;
            SaveToSession();
            return true;
        }
    });
}

function SelectMain(thing, event, rowid, itemCode, itemName) {
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
                var rowData = OperationGrid.jqGrid("getRowData", rowid);
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
                    Main: "1",
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
                        OperationGrid.jqGrid("setCell", rowid, "ToolId", main.ItemCode);
                        _checkEdit = true;
                        OperationGrid.jqGrid("setCell", rowid, "MachineName", main.ItemName);
                    }
                });
            },
            function () {
                event.preventDefault();
                //$(thing).prop('checked', false);
                return;
            }, "Tool?"
            );//12
    event.preventDefault();
    return false;
}

function SaveToSession() {
    _checkEdit = true;
    var rowid = $(grid_selector).jqGrid("getGridParam", "selrow");
    var Edition = $(grid_selector).jqGrid("getCell", rowid, "Edition");
    var rowData = OperationGrid.jqGrid("getRowData", rowDrop);
    var toolId = rowData.ToolId;
    $("#" + ToolsGrid).find('tr[role="row"]:not(.ui-jqgrid-labels)').find("input:checkbox:checked")
        .each(function () {
            var rowPid = $(this).parent().parent().attr("id");
            var ChildData = $("#" + ToolsGrid).jqGrid("getRowData", rowPid);
            var rd = $(this).parent().parent().find("input[name='radio_ToolsGrid']");

            var tool = {
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
                ImagePath: ChildData.Img,
                MainTool: rd.is(":checked") ? "1" : "0",
                Edition: Edition
            };
            tools.push(tool);
            return true;
        });
    var len = tools.length;
    if ($.isEmptyObject(toolId) && len > 1) {
        if (!CheckMain(tools)) {
            ShowValidateByItem("001", SmsFunction.Update, MessageType.Warning, MessageContext.IgnoreChanges, Type.Warning, " tool");//8
            tools = [];
            return false;
        }
    }
    if (len === 1 && $.isEmptyObject(toolId)) {
        tools[0].MainTool = "1";
    }
    var ltool = JSON.stringify({ 'tools': tools });
    tools = [];
    $.ajax({
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST",
        url: "/OpsLink/AddSessionOptl",
        data: ltool,
        success: function (data) {
            GetParentExpand();
            GetExpand();
            OperationGrid.trigger("reloadGrid");
            $("#" + ToolsGrid).trigger("reloadGrid");
        },
        err: function (response) {
            console.log(response);
        }
    });
}

function SaveTools(isAlert) {
    if (!CheckDragg()) {
        return;
    }
    $.ajax({
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST",
        url: "/OpsLink/SaveTools",
        data: {},
        success: function (data) {
            _checkEdit = false;
            if (isAlert) {
                if (data === 1 || data === 2) {
                    ShowValidateByItem("001", SmsFunction.Add, MessageType.Success, MessageContext.Update, Type.Success);// 17
                } else {
                    ShowValidateByItem("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, Type.Error);//14
                }
                GetParentExpand();
                GetExpand();
                OperationGrid.trigger("reloadGrid");
                $("#" + ToolsGrid).trigger("reloadGrid");
            }
        },
        err: function () {
            ShowValidateByItem("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, Type.Error);//14
        }
    });
    tools = [];
}

function GetParentExpand() {
    _epp = true;
    //ui-icon ace-icon fa fa-plus
    //ui-icon ace-icon fa fa-minus
    OperationGrid.find("tr.jqgroup").find("span.fa-minus").each(function () {
        var id = $(this).parent().parent().attr("id");
        _expandP.push(id);
    });
}

function GetExpand() {
    _ep = true;
    OperationGrid.find('td.sgexpanded[aria-describedby="OperationGrid_subgrid"]').each(function () {
        var id = $(this).parent().attr("id");
        _expand.push(id);
    });
}

function Expand() {
    if (_ep) {
        _expandP.forEach(function (a) {
            $("#" + a).find("span").trigger("click");
        });
        _expand.forEach(function (a) {
            OperationGrid.find('tr[id="' + a + '"]').find('td[aria-describedby="' + OperationGridTb + '_subgrid"]').trigger("click");
        });
    }
    _expandP = [];
    _expand = [];
    _epp = false;
    _ep = false;
}

function DragRowTools() {
    $(".tools-draggable").draggable({
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
                html = "<div id='dragItemID' ><table class='table table-bordered'><tr><td>" + arrTool.ItemCode + "</td><td>" + itemcode + "</td><td>" + arrTool.ItemName + "</td><td>" + itemname + "</td></tr>";
            }
            else {
                html = htmlMuilty;
            }
            return $(html);
        },
        cursorAt: { left: 10, top: 10 }
    });
}

function CheckMuiltiple() {
    var i = 0;
    $("#" + ToolsGrid).find('tr[role="row"]').find("input[type=checkbox]:checked").each(function () {
        i++;
    });
    if (i > 1) {
        return true;
    }
    return false;
}

function SelectRowTools(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId) {
    OperationGrid.jqGrid("setGridParam", {
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColor: styleColorSerial, revNo: revNo, opRevNo: opRevNo, edition: edition, languageId: languageId
        }
    }).trigger("reloadGrid");

    $("#ToolsGrid").jqGrid("setGridParam", {
        postData: {
            gId: $("#cbTool").val()
        }
    }).trigger("reloadGrid");
}

function ShowGridchildTools(subgridId, rowId) {
    var row = OperationGrid.getRowData(rowId);
    var ToolId = row.ToolId;
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
        url: "/OpsLink/GetOptl?styleCode=" + sCode + "&styleSize=" + sSize +
            "&styleColor=" + sColorSerial + "&revNo=" + rNo +
        "&opRevNo=" + opRevNo + "&opSerial=" + opSerial + "&edition=" + edition,
        datatype: "json",
        page: 1,
        colModel: [
            { name: "", index: "", width: 30, sortable: false, formatter: RemoveIcon, align: "center" },
            {
                name: 'MainTool', index: "MainTool", label: arrTool.MainTool, width: 50,
                formatter: function (cellvalue, options, rowObject) {
                    let checked = "";
                    if (ToolId === rowObject.ItemCode) {
                        checked = 'checked = "checked"';
                    } else {
                        checked = "";
                    }
                    return '<input  onclick="SelectMain(this, event,' + rowId + ',\'' + rowObject.ItemCode + '\',\'' + rowObject.ItemName + '\');"'
                            + checked + 'type="radio"  name="radio_' + options.gid + '"  />';
                }
            },
            { label: arrTool.CategId, name: "CategId", index: "CategId", width: 90, hidden: true },
            { name: "Category", index: "Category", label: arrTool.Category, width: 180, sort: false },
            { label: arrTool.ItemCode, name: "ItemCode", index: "ItemCode", width: 120 },
            { label: arrTool.ItemName, name: "ItemName", index: "ItemName", width: 300 },
            {
                label: arrTool.ImagePath,
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
            $("#" + subgridId + "_t_").html("<a title ='Remove' class='memberAction' style='padding-left: 7px;'  onclick=\"RemoveAllTools(this,'" + rowId + "')\">x</a>");
            $("#" + subgridId).addClass("childtools-droppable");
            ToolsChildDraggAble(rowId);
        },
        pager: "#jqGridPager" + "_" + subgridId
    });
}

function RemoveIcon(cellvalue, options, rowObject) {
    var id = options.rowId;
    var refNo = rowObject.OpSerial;
    var html = "<a title ='Remove' id='" + id + "' onclick=\"RemoveRowTools(this,'" + id + "')\" class='memberAction' name='" + refNo + "' type='button'>X</a>";
    return html;
}

function RemoveRowTools(thing, id) {
    if (!CheckDragg()) {
        return;
    }
    var main = null;
    var grid = $(thing).parent().parent().parent().parent().attr("id");
    var arr = grid.split("_");
    var rowParrent = arr[1];
    var checkmain = $("#" + grid).find("tr[id='" + id + "']").find("input[type='radio']:checked").val();
    var record = jQuery("#" + grid).jqGrid('getGridParam', 'records');
    var rowid = $(grid_selector).jqGrid("getGridParam", "selrow");
    // if remove main tools
    if (checkmain === "on") {
        if (record > 2) {
            ShowValidateByItem("001", SmsFunction.Delete, MessageType.Error, MessageContext.Communication, Type.Success, "tool");//9
            return;
        }
        if (record === 1 || record === 2) {
            var mainTool = record === 1 ? "0" : "1";
            var rowData = OperationGrid.jqGrid("getRowData", rowParrent);
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
                        MainTool: mainTool,
                        Edition: $(grid_selector).jqGrid("getCell", rowid, "Edition")
                    };
                    return false;
                }
            });
        }
    }
    ConfirmRemove(function () {
        _checkEdit = true;
        var rowDataP = OperationGrid.jqGrid("getRowData", rowParrent);
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
        tools.push(tool);
        var ltool = JSON.stringify({ 'tools': tools });
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/RemoveSessionTools",
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
                        GetParentExpand();
                        GetExpand();
                        OperationGrid.trigger("reloadGrid");
                        main = null;
                    });
                } else {
                    GetParentExpand();
                    GetExpand();
                    OperationGrid.trigger("reloadGrid");
                }
                _checkEdit = true;
            },
            err: function () {
            }
        });
        tools = [];
    });
}

function RemoveAllTools(thing, id) {
    if (!CheckDragg()) {
        return;
    }
    ConfirmRemove(function () {
        _checkEdit = true;
        var rowid = $(grid_selector).jqGrid("getGridParam", "selrow");
        var Edition = $(grid_selector).jqGrid("getCell", rowid, "Edition");
        var rowDataP = OperationGrid.jqGrid("getRowData", id);
        var grid = OperationGridTb + "_" + id + "_t";
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
            tools.push(tool);
        });
        //Close before remove
        OperationGrid.find('tr[id="' + id + '"]').find('td[aria-describedby="OperationGrid_subgrid"]').trigger("click");
        var ltool = JSON.stringify({ 'tools': tools });
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/RemoveSessionTools",
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
                        Main: "0",
                        Edition: Edition
                    };
                    var opToolConfig = {
                        url: "/OpsLink/AddMainRemove",
                        postData: JSON.stringify(main)
                    };
                    AjaxPostCommon(opToolConfig, function (response) {
                        GetParentExpand();
                        GetExpand();
                        OperationGrid.trigger("reloadGrid");
                    });
                } else {
                    GetParentExpand();
                    GetExpand();
                    OperationGrid.trigger("reloadGrid");
                }
            }
        });
        tools = [];
    });
}

function AppendMcmtByCode(name, isMachine) {
    $.ajax({
        url: "/OpsLink/GetMasterCodeByCode",
        async: false, //run sequence
        type: "POST",
        data: JSON.stringify({ isMachine: isMachine }),
        dataType: "json",
        contentType: "application/json",
        success: function (arrRes) {
            FillDataToDropDownlist2(name, arrRes, "SubCode", "CodeName");
        },
        error: function (err) {
            console.log(err);
            FillDataToDropDownlist2("cbTool", [], "SubCode", "CodeName");
        }
    });
}

function ResetVariableTools() {
    checkDragg = false;
    tools = [];
    _expand = [];
    _expandP = [];
    _ep = false;
    _epp = false;
    _checkEdit = false;
}