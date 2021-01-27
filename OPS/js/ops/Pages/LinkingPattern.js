//==================
var iniTialpattern = "000";
var _rowOpened = null;
var _rowOpenedParent = null;
var _gridOldEdit = null;
var max = 0;
var linked = "Linked";
var bgLinked = "#FAAC58";
var notLinked = "Not yet Linked";
var prots = [];
var currChild = [];
var dataSubGrid = [];
var _bomExpand = [];
var _opExpand = [];
var _opExpandP = [];
var _bomep = false;
var _opep = false;
var _opepp = false;
var rowDropTo;
var checkDragg = false;
var ck = false;
var _rowOpen = [];
var _subGridProt = [];
//======================
var tempGrid = "DataTempGrid";
var Operation_Grid = $("#Operation_Grid");
var Operation_Grid_TB = "Operation_Grid";
var Pattern_Grid_TB = "Pattern_Grid";
var multiTitle = "Multiple selected";
var htmlMuilty = "<div id='dragItemID' class='DraggAllItem'><span class='rowdragg'>" + multiTitle + "</span></div>";
var bomt = "Bomt";
var pattern = "Pattern";
var btnUpdateText = "Update";
var btnAddText = "Link";
arrColNameOpsDetail = {
    STYLECODE: "Style Code",
    SYTLESIZE: "Style Size",
    STYLECOLORSERIAL: "Color",
    REVNO: "RevNo",
    OPREVNO: "Op No",
    OPSERIAL: "Op Serial",
    OPNUM: "OpNum",
    OPNAME: "Operation Name",
    OPTIME: "Operation Time",
    OPPRICE: "Cost",
    FACTORY: "Factory",
    MANCOUNT: "Workers",
    MACHINETYPE: "MachineType",
    MACHINECOUNT: "Machines",
    OFFEROPPRICE: "Offer Price",
    MAXTIME: "Max Time",
    THREADCOLOR: "Thread Color",
    BENCHMARKTIME: "Benchmark Time",
    OPGROUPNAME: "Group Name",
    MACHINETYPENAME: "Machine Type",
    MACHINEFILE: "Machine File",
    VIDEO: "Video",
    UPLOADFILE: "Upload file",
    PLAYVIDEO: "Play Video"
};

arrPatternCollName = {
    MainItemCode: "Main Item",
    ItemCode: "Item Code",
    ItemName: "Item Name",
    STYLESIZE: "Item Size",
    MainItemColorSerial: "Item Color",
    REVNO: "Pattern Name",
    OPREVNO: "Pattern Serial",
    OPTIME: "Pattern Uniquire",
    OPPRICE: "OP Price",
    MACHINECOUNT: "Register Date",
    CONFIRMCHK: "Status",
    OPCOUNT: "OP Count",
    MANCOUNT: "Workers",
    REMARKS: "Remarks",
    StyleColorSerial: "Color Serial",
    Qty: "Qty"
};

arrPatternChilName = {
    Url: "Image",
    Width: "Width",
    Height: "Height",
    PatternSerial: "Serial",
    PieceQty: "Piece Qty",
    EndWise: "End Wise"
};

arrOpChilName = {

};

arrButtonName = {
    edittext: "Edit",
    addtext: "Add",
    deltext: "Delete",
    searchtext: "Search",
    refreshtext: "Refresh"
};

//function ======================================================
// operation

arrOpsColname = {
    EDITION: 'Edition',
    STYLECODE: 'Style Code',
    STYLESIZE: 'Style Size',
    STYLECOLORSERIAL: 'Color',
    REVNO: 'Revision',
    OPREVNO: 'OP Revision',
    OPTIME: 'Ops Time',
    OPPRICE: 'OP Price',
    MACHINECOUNT: 'Machine count',
    CONFIRMCHK: 'Status',
    OPCOUNT: 'OP Count',
    MANCOUNT: 'Workers',
    LASTUPDATEDATE: 'Date update',
    REMARKS: 'Remarks'
};

//======================
function CreateOperationGrid(styleCode, styleSize, styleColorSerial, revNo, edition, languageId) {
    Operation_Grid.jqGrid({
        datatype: "json",
        height: 450,
        width: null,
        shrinkToFit: false,
        rowNum: 100000,
        rownumbers: true,
        gridview: false,
        //==========================================
        url: "/OpsLink/GetOpDetail",
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
                search: false,
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
        subGridRowExpanded: ShowGridchildOperation,
        subGridOptions: {
            plusicon: "ace-icon fa fa-plus",
            minusicon: "ace-icon fa fa-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: true
        },
        gridComplete: function () {
            var x;
            
            $('td[aria-describedby="Operation_Grid_NewPrevNo"]').each(function () {
                if (!$(this).is(".jqgroup,.jqgfirstrow,.ui-subgrid")) {
                    $(this).parent().addClass("ui-droppable");
                }
                x = $(this).html();
                if (x === "&nbsp;") {
                    $(this).parent().find('td[aria-describedby="Operation_Grid_subgrid"]').unbind("click").html("");
                }
            });
            if (_opep) {
                x = CheckEpandRowDrop(rowDropTo, _opExpand);
                ExpandOperation();
                if (!x) {
                    Operation_Grid.find('tr[id="' + rowDropTo + '"]').find('td[aria-describedby="Operation_Grid_subgrid"]').trigger("click");
                }
            }
            DroppRowGrid();
            x = Operation_Grid.jqGrid("getGridParam", "records");
            ShowHideButtonByTab(x, "#patternLinking");
        }
    });
}

//==============Grid linking Parten_Grid
function CreatePatternGrid(styleCode, styleSize, styleColorSerial, revNo) {
    $("#" + Pattern_Grid_TB).jqGrid({
        datatype: "json",
        height: 250,
        shrinkToFit: false,
        width: null,
        rownumbers: true,
        multiselect: true,
        pginput: false,
        rowNum: 100000,
        //==========================================
        url: "/OPSLink/GetBom",
        postData: {
            styleCode: styleCode, size: styleSize, serial: styleColorSerial, revNo: revNo
        },
        colModel: [
            //{ name: "Status", index: "Status", label: arrPatternCollName.Status,search: false, sort: false},
            {
                name: "MainItemCode", index: "MainItemCode",
                label: arrPatternCollName.MainItemCode
            },
            {
                name: "ItemCode", index: "ItemCode",
                label: arrPatternCollName.ItemCode
            },
            {
                name: "ItemName", index: "ItemName",
                label: arrPatternCollName.ItemName,
                width:200
            },
            {
                name: "ItemColorWays", index: "ItemColorWays",
                label: arrPatternCollName.MainItemColorSerial
            },
            { label: arrPatternCollName.StyleColorSerial, name: "StyleColorSerial", index: "StyleColorSerial", align: "center", width: 60 },
            { label: arrPatternCollName.UnitConsumption, name: "UnitConsumption", index: "UnitConsumption", hidden: false },
            { label: arrPatternCollName.ConsumpUnit, name: "ConsumpUnit", index: "ConsumpUnit", hidden: false },
            { label: arrPatternCollName.Qty, name: "Qty", index: "Qty", align: "center", width: 60 },
            { name: "RegistryDate", index: "RegistryDate", hidden: false },
            { name: "CurrCode", index: "CurrCode", hidden: true },
            { name: "StyleCode", index: "StyleCode", hidden: true },
            { name: "StyleSize", index: "StyleSize", hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "RevNo", index: "RevNo", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: "PatternCode", index: "PatternCode", hidden: true }
        ],
        afterInsertRow: function (rowid, rowdata) {
            $("#" + rowid, $("#" + Pattern_Grid_TB)).addClass("Itemrow-draggable");
        },
        subGrid: true,
        subGridOptions: {
            plusicon: "ui-icon-plus",
            minusicon: "ui-icon-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: true
        },
        subGridRowExpanded: ShowGridchildPattern,
        gridComplete: function () {
            $('td[aria-describedby="Pattern_Grid_PatternCode"]').each(function () {
                var x = $(this).html();
                if (x === "&nbsp;") {
                    $(this).parent().find('td[aria-describedby="Pattern_Grid_subgrid"]').unbind("click").html("");
                }
            });
            if (_bomep) {
                ExpandBomt();
            }
            DragRow();
            var record = $("#" + Pattern_Grid_TB).jqGrid("getGridParam", "records");
            ShowHideButtonByTab(record, "#patternLinking");
        }
    });
}

function ShowGridchildOperation(subgridId, rowId) {
    var row = Operation_Grid.getRowData(rowId);
    var sCode = row.StyleCode;
    var sSize = row.StyleSize;
    var sColorSerial = row.StyleColorSerial;
    var rNo = row.RevNo;
    var opRevNo = row.OpRevNo;
    var opSerial = row.OpSerial;
    var opType = row.OpType;
    var selRowId = $(grid_selector).jqGrid("getGridParam", "selrow");
    var edition = $(grid_selector).jqGrid("getCell", selRowId, "Edition").charAt(0);
    var subgridTableId = subgridId + "_t";
    $("#" + subgridId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: "/OpsLink/GetBomByOp",
        datatype: "json",
        postData: {
            styleCode: sCode, styleSize: sSize, styleColor: sColorSerial, revNo: rNo, opRevNo: opRevNo,
            opSerial: opSerial, edition: edition
        },
        page: 1,
        colModel: [
            { name: "", index: "", width: 30, sortable: false, formatter: RemoveBom, align: "center" },
            { label: arrPatternCollName.ItemCode, width: 200, name: "ItemCode", index: "ItemCode" },
            { label: arrPatternCollName.ItemName, width: 400, name: "ItemName", index: "ItemName" },
            { label: arrOpChilName.OpType, name: "OpType", index: "OpType", width: 200, align: "center" },
            { label: arrOpChilName.UnitConsumption, name: "UnitConsumption", index: "UnitConsumption", width: 120 },
            { label: arrOpChilName.ConsumpUnit, name: "ConsumpUnit", index: "ConsumpUnit", width: 120 },
            { name: "OpRevNo", index: "OpRevNo", width: 80, hidden: true },
            { name: "OpSerial", index: "OpSerial", width: 80, hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "MainItemCode", index: "MainItemCode", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: "BomOrPattern", index: "BomOrPattern", hidden: true },
            { name: "PatternSerial", index: "PatternSerial", hidden: true },
            { name: "Edition", index: "Edition", hidden: true },
            { name: "CurPatternSerial", index: "CurPatternSerial", hidden: true }
        ],
        viewrecords: true,
        loadonce: false,
        height: "100%",
        rowNum: 100000,
        rownumbers: true,
        //autowidth: true,
        subGrid: true,
        subGridRowExpanded: ShowGridchildOpPattern,
        subGridRowColapsed: function () {
            var currentID = $(this).attr("id");
            currentID = currentID + "_subgrid";
            var index = _rowOpen.indexOf(currentID);
            if (index > -1) {
                _rowOpen.splice(index, 1);
            }
        },
        subGridOptions: {
            plusicon: "ace-icon fa fa-plus",
            minusicon: "ace-icon fa fa-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: true
        },
        ondblClickRow: function (rowid) {
            if (CheckDragg()) {
                var patternSerial = $("#" + subgridId + "_t").jqGrid("getCell", rowid, "PatternSerial");
                if (patternSerial != iniTialpattern) {
                    // this row is not linked yes
                    return;
                }               
                ShowBoxUpdate(subgridId, rowid);
            }
        },
        gridComplete: function() {
            $("#" + subgridId + "_t_").html("<a title ='Remove' class='memberAction' style='padding-left: 7px;'  onclick=\"RemoveAll(this,'" + rowId + "')\">x</a>");
            $("#" + subgridId).addClass("child-droppable");
            $('td[aria-describedby="' + subgridId + '_t_BomOrPattern"]').each(function () {
                x = $(this).html();
                if (x !== pattern) {
                    $(this).parent().find('td[aria-describedby="' + subgridId + '_t_subgrid"]').unbind("click").html("");
                }
            });
            ChildDropAble();
            _rowOpen.forEach(function (a) {
                if (!CheckOpen(a, subgridId)) {
                    $('td[aria-describedby="' + a + '"]').trigger("click");
                }
            });
        },
        pager: "#jqGridPager" + "_" + subgridId
    });
}

function CheckOpen(idOpen, grid) {
    var x = idOpen.split("_");
    var gridCompare = "Operation_Grid_" + x[2];
    if (gridCompare != grid) {
        return true;
    }
    return $('td[aria-describedby="' + idOpen + '"]').hasClass('sgexpanded');
}

function ShowGridchildOpPattern(subgridId, rowId) {
    var x = $(this).attr("id");
    var currentID = x + "_subgrid";
    var index = _rowOpen.indexOf(currentID);
    if (index <= -1) {
        _rowOpen.push(currentID);
    }
    
    console.log(x + " | " + subgridId + " | " + rowId);
    var rowparrent = x.split("_")[2];
    var row = Operation_Grid.getRowData(rowparrent);
    var sCode = row.StyleCode;
    var sSize = row.StyleSize;
    var sColorSerial = row.StyleColorSerial;
    var rNo = row.RevNo;
    var opRevNo = row.OpRevNo;
    var opSerial = row.OpSerial;
    var opType = row.OpType;
    var selRowId = $(grid_selector).jqGrid("getGridParam", "selrow");
    var edition = $(grid_selector).jqGrid("getCell", selRowId, "Edition").charAt(0);
    var rowBom = $("#" + x).getRowData(rowId);
    var subgridTableId = subgridId + "_t";
    $("#" + subgridId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: "/OpsLink/GetPatternByBom",
        datatype: "json",
        //ITEMCODE, ITEMCOLORSERIAL, MAINITEMCODE
        postData: {
            styleCode: sCode, styleSize: sSize, styleColor: sColorSerial, revNo: rNo, opRevNo: opRevNo,
            opSerial: opSerial, edition: edition, mainItemColorSerial: rowBom.MainItemColorSerial,
            itemCode: rowBom.ItemCode, itemColorSerial: rowBom.ItemColorSerial, mainItemCode: rowBom.MainItemCode
        },
        page: 1,
        colModel: [
            { name: "", index: "", width: 30, sortable: false, formatter: Remove, align: "center" },
            {
                label: arrPatternChilName.Url,
                name: "Url",
                index: "Url",
                align: "center",
                width: 120,
                formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return "<img id='" +
                            id +
                            "' class='imgpattern' onclick = ShowPatternImage('" +
                            cellvalue +
                            "'); src='" +
                            cellvalue +
                            "' onerror='imgError(this);'/>";
                    return "";
                }
            },
            { label: arrOpChilName.Piece, name: "Piece", index: "Piece", width: 300 },
            { label: arrOpChilName.OpType, name: "OpType", index: "OpType", width: 80 },
            { label: arrPatternChilName.Width, name: "Width", index: "Width", align: "center" },
            { label: arrPatternChilName.Height, name: "Height", index: "Height", align: "center" },
            { label: arrOpChilName.PieceQty, name: "PieceQty", index: "PieceQty", width: 80 },
            { label: arrOpChilName.UnitConsumption, name: "UnitConsumption", index: "UnitConsumption", width: 80, hidden: true },
            { label: arrOpChilName.ConsumpUnit, name: "ConsumpUnit", index: "ConsumpUnit", width: 80, hidden: true },
            { label: arrPatternChilName.PieceUnique, name: "PieceUnique", index: "PieceUnique", align: "center" },
            { name: "ItemCode", index: "ItemCode", hidden: true },
            { name: "OpRevNo", index: "OpRevNo", width: 80, hidden: true },
            { name: "OpSerial", index: "OpSerial", width: 80, hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "MainItemCode", index: "MainItemCode", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: "Edition", index: "Edition", hidden: true },
            { name: "PatternSerial", index: "PatternSerial", hidden: true },
            { name: "CurPatternSerial", index: "CurPatternSerial", hidden: true }
        ],
        viewrecords: true,
        loadonce: false,
        height: "100%",
        rowNum: 100000,
        rownumbers: true,
        gridComplete: function () {
            $("#" + subgridId + "_t_").html("<a title ='Remove' class='memberAction' style='padding-left: 7px;'  onclick=\"RemoveAllPattern(this,'" + rowId + "')\">x</a>");
            $("#" + subgridId).addClass("child-droppable");
            ChildDropAble();
        },
        ondblClickRow: function (rowid, iRow, iCol, e) {
            if (CheckDragg()) {
                ShowBoxUpdate(subgridId, rowid);
                e.stopPropagation();
            }
        },
        pager: "#jqGridPager" + "_" + subgridId
    });
}

function ShowGridchildPattern(subgridId, rowId) {
    var row = $("#" + Pattern_Grid_TB).getRowData(rowId);
    var sCode = row.StyleCode;
    var sSize = row.StyleSize;
    var sColorSerial = row.StyleColorSerial;
    var rNo = row.RevNo;
    var itemcode = row.ItemCode;
    var itemColorSerial = row.ItemColorSerial;
    var mainItemCode = row.MainItemCode;
    var mainItemColorSerial = row.MainItemColorSerial;
    var selRowId = $(grid_selector).jqGrid("getGridParam", "selrow");
    var edition = $(grid_selector).jqGrid("getCell", selRowId, "Edition").charAt(0);
    var subgridTableId = subgridId + "_t";
    $("#" + subgridId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: "/OpsLink/GetPatternLink?styleCode=" + sCode + "&styleSize=" + sSize + "&styleColorSerial=" + sColorSerial +
        "&revNo=" + rNo + "&itemcode=" + itemcode + "&itemColorSerial=" + itemColorSerial +
        "&mainItemCode=" + mainItemCode + "&MainItemColorSerial=" + mainItemColorSerial,
        datatype: "json",
        page: 1,
        colModel: [
            { name: "Status", index: "Status", label: arrPatternCollName.Status, search: false, sort: false },
            {
                label: arrPatternChilName.Url, name: "Url", index: "Url", width: 80,
                align: "center",
                formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue) return "<img id='" + id + "' class='imgpattern' onclick = ShowPatternImage('" + cellvalue + "'); src='" + cellvalue + "'  onerror='imgError(this);'/>";
                    return "";
                }
            },
            { label: arrPatternChilName.Piece, name: "Piece", index: "Piece", width: 200 },
            { label: arrPatternChilName.Width, name: "Width", index: "Width", align: "center", width: 80 },
            { label: arrPatternChilName.Height, name: "Height", index: "Height", align: "center", width: 80 },
            //{ label: arrPatternChilName.MainPart, name: "MainPartName", index: "MainPartName", width: 80 },
            { label: arrPatternChilName.EndWise, name: "EndWise", index: "EndWise", align: "center", width: 80 },
            { label: arrPatternChilName.PieceQty, name: "PieceQty", index: "PieceQty", align: "center", width: 80 },
            { label: arrPatternChilName.UnitConsumption, name: "UnitConsumption", index: "UnitConsumption", align: "center", width: 80 },
            { label: arrPatternChilName.PieceUnique, name: "PieceUnique", index: "PieceUnique",align: "center", width: 80 },
            { label: arrPatternChilName.PatternSerial, name: "PatternSerial", index: "PatternSerial", width: 80 },
            { name: "Currcode", index: "Currcode", hidden: true },
            { name: "MainPart", index: "MainPart", hidden: true },
            { name: "SizeUnit", index: "SizeUnit", hidden: true },
            { name: "ConsumpUnit", index: "ConsumpUnit", hidden: true },
            { name: "Area", index: "Area", hidden: true },
            { name: "ItemCode", index: "ItemCode", hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "MainItemCode", index: "MainItemCode", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: "PieceQtyRest", index: "PieceQtyRest", hidden: true },
            { name: "PatternSerial", index: "PatternSerial", hidden: true }
        ],
        viewrecords: true,
        height: "100%",
        rownumbers: true,
        rowNum: 100000, //ADD) SON (2019.10.30) - 30 October 2019
        multiselect: true,
        afterInsertRow: function (rowid, rowdata) {
            if (rowdata.Status !== linked) {
                $("#" + subgridTableId).find('tr[id="' + rowid+'"]').addClass("Child-draggable");
            } else {
                $("#jqg_" + subgridTableId + "_" + rowid).prop("checked", false).attr("disabled", true);
                $("#jqg_" + subgridTableId + "_" + rowid).parent().parent().find('td[aria-describedby="' + subgridTableId+'_Status"]').addClass("Linked");
            }
        },
        onSelectAll: function (id, status) {
            if (status === true) {
                $.each(id, function (index, value) {
                    var stt = $("#" + subgridId+"_t").jqGrid("getCell", value, "Status");
                    if (stt === linked) {
                        $("#jqg_" + subgridId + "_t_" + value).removeAttr("checked");
                    }
                });
            }
            return false;
        },
        onCellSelect: function (id, cellidx) {
            var cm = $("#" + subgridId + "_t").jqGrid("getGridParam", "colModel");
            var colNameAttr = cm[cellidx].name;
            if (colNameAttr === "Url") {
                ck = $("#jqg_" + subgridId + "_t_" + id).is(":checked");
                setTimeout(function () {
                        $("#jqg_" + subgridId + "_t_" + id).prop("checked", ck);
                    },1);
            } else {
                ck = false;
            }
        },
        onSelectRow: function (id) {
            var stt = $("#" + subgridId+"_t").jqGrid("getCell", id, "Status");
            if (stt === linked) {
                $("#jqg_" + subgridId + "_t_" + id).removeAttr("checked");
            }
        },
        gridComplete: function () {
            DraggChil();
        },
        pager: "#jqGridPager" + "_" + subgridId
    });
};

//================dragd and drop

function ShowPatternImage(url) {
    $("#modalImage").modal("show");
    $("#modalShowimage").attr("src", url);
    setTimeout(function () {
        $("#modalImage").modal("hide");
    }, 6000);
}

function CheckMuiltipalSelected(taget,isImage) {
    var currentGrid;
    var idCurent;
    var i = 0;
    if (isImage) {
        idCurent = taget.attr("id");
        currentGrid = taget.parent().parent().attr("id");
    } else {
        currentGrid = taget.offsetParent.id;
        idCurent = taget.id;
    }
    $("#"+Pattern_Grid_TB).find('tr[role="row"]').find("input[type=checkbox]:checked").each(function () {
        i++;
    });
    if (i > 0) {
        // just dragg current checked return false
        var x = $("#" + currentGrid).find("tr[id='" + idCurent + "']").find("input[type=checkbox]:checked");
        if (x.length === 1 && i === 1) return false;
        return true;
    }
    return false;
}

function DragRow() {
    $(".Itemrow-draggable").draggable({
        start: function() {
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
            if ($(this).find('td[aria-describedby="' + Pattern_Grid_TB + '_Status"]').html() === linked) {
                $(this).find("input:checkbox").prop("checked", false);
            } else {
                $(this).find("input:checkbox").prop("checked", true);
            }
            var html;
            var piece = event.delegateTarget.cells[3].textContent;
            var w = event.delegateTarget.cells[4].textContent;
            var h = event.delegateTarget.cells[4].textContent;
            if (!CheckMuiltipalSelected(event.delegateTarget)) {
                html = "<div id='dragItemID' ><table class='table table-bordered'><tr><td>piece</td><td>" + piece + "</td><td>Width</td><td>" + w + "</td><td>Height</td><td>" + h + "</td></tr>";
            }
            else {
                html = htmlMuilty;
            }
            return $(html);
        },
        //stop: function (event, ui) {

        //},
        cursorAt: { left: 10, top: 10 }
    });
}

function DraggChil() {
    $(".Child-draggable").draggable({
        drag: function () {
            return CheckDragg();
        },
        revert: "invalid",
        cursor: "pointer",
        appendTo: "body",
        helper: function (event/*, ui*/) {
            var html;
            if (!CheckDragg()) {
                return $("<div></div>");
            }
            if ($(this).find('td[aria-describedby="*_Status"]').html() === linked) {
                $(this).find("input:checkbox").prop("checked", false);
            } else {
                $(this).find("input:checkbox").prop("checked", true);
            }
            $(this).find("input:checkbox").prop("checked", true);
            if (!CheckMuiltipalSelected(event.delegateTarget)) {
                var src = $(event.delegateTarget.cells[3].innerHTML).attr("src");
                html = "<image id='dragItemID' src='" + src + "'/>";
            }
            else {
                html = htmlMuilty;
            }
            return $(html);
        },
        start: function (event, ui) {
            $(this).css("z-index", 1001);
            if ($(ui)[0].helper.width() > 200) {
                ui.helper.animate({
                    width: 200,
                    height: 250

                });
            }
        },
        cursorAt: { left: 10, top: 10 }
    });
}

function DroppRowGrid() {
    $(".ui-droppable").droppable({
        accept: ".Itemrow-draggable,.Child-draggable",
        activeClass: "dropactive",
        hoverClass: "drophover",
        tolerance: "pointer",
        drop: function (event, ui) {
            if (!CheckDragg()) {
                return false;
            }
            rowDropTo = $(this).attr("id");
            var x = 1;
            var y = 4;
            var checkMuilty = ui.draggable;;
            if (ui.draggable.is(".Child-draggable")) {
                checkMuilty = ui.draggable;
                x = 3;
                y = x;
            }
            if (CheckMuiltipalSelected(checkMuilty, 1)) {
                x = 4;
                $("#txtName").text(multiTitle);
            } else {
                var text = "";
                if (x !== 3) {
                    var rowidBom = ui.draggable.attr("id");
                    text = $("#" + Pattern_Grid_TB).jqGrid("getCell", rowidBom, "ItemName");
                }
                else {
                   var grid = ui.draggable.parent().parent().attr("id");
                   var rowidPt = ui.draggable.attr("id");
                   text = $("#" + grid).jqGrid("getCell", rowidPt, "Piece");
                }
                $("#txtName").text(text);
            }
            ShowBoxDrag(x, ui, y);
            return true;
        }
    });
}

function ChildDropAble() {
    $(".child-droppable").droppable({
        accept: ".Itemrow-draggable,.Child-draggable",
        activeClass: "dropactive",
        hoverClass: "drophover",
        tolerance: "pointer",
        drop: function (event, ui) {
            if (!CheckDragg()) {
                return false;
            }
            var id = $(this).attr("id");
            rowDropTo = id.split("_")[2];
            var x = 1;
            var y = 4;
            var checkMuilty = ui.draggable;;
            if (ui.draggable.is(".Child-draggable")) {
                checkMuilty = ui.draggable;
                x = 3;
                y = x;
            }
            if (CheckMuiltipalSelected(checkMuilty, 1)) {
                x = 4;
                $("#txtName").text(multiTitle);
            } else {
                var text = "";
                if (x !== 3) {
                    var rowidBom = ui.draggable.attr("id");
                    text = $("#" + Pattern_Grid_TB).jqGrid("getCell", rowidBom, "ItemName");
                }
                else {
                    var grid = ui.draggable.parent().parent().attr("id");
                    var rowidPt = ui.draggable.attr("id");
                    text = $("#" + grid).jqGrid("getCell", rowidPt, "Piece");
                }
                $("#txtName").text(text);
            }
            ShowBoxDrag(x, ui, y);
            return true;
        }
    });
}

function RemoveError() {
    $("#txtQty").removeClass("error");
    $("#txtUnitConsumption").removeClass("error");
    $(".select2-selection--single").removeClass("error");
    $("#validateUpdate").html("");
    $("#validateUpdate").hide();
}

function RemoveValue(type) {
    $("#txtQty").val("");
    $("#txtQty").val("");
    $("#cbConsumpUnit").val("Cm").trigger("change");
    $("#txtUnitConsumption").val("");
}

function ShowBoxUpdate(gridId, rowid) {
    $("#divUpdate").show();
    $("#divLink").hide();
    RemoveError();
    $("#btnAddPattern").html(btnUpdateText);
    $("#GridDragg").val(gridId);
    $("#rowSelect").val(rowid);
    var parrentId = gridId.split("_")[2];
    var rowBomtData = Operation_Grid.jqGrid("getRowData", parrentId)
    var itemcode = Operation_Grid.jqGrid("getCell", rowid, "OpNum");
    var sbItemName = Operation_Grid.jqGrid("getCell", parrentId, "OpNameLan");
    var opGroupName = Operation_Grid.jqGrid("getCell", parrentId, "OpGroupName");
    var rowChildData = $("#" + gridId + "_t").jqGrid("getRowData", rowid);
    var conSumUnit = rowChildData.ConsumpUnit;
    var unitConsumption = rowChildData.UnitConsumption;
    var patternSerial = rowChildData.PatternSerial;
    var pieceQty = rowChildData.PieceQty;
    var opType = rowChildData.OpType;
    if (patternSerial === iniTialpattern) {
        $("#txtName").text(rowChildData.ItemName);
        ShowHideInPut(1);
        RemoveValue();
        $("#cbConsumpUnit").val(conSumUnit).trigger("change");
        $("#txtUnitConsumption").val(unitConsumption);
    } else {
        $("#txtName").text(rowChildData.Piece);
        ShowHideInPut(2);
        $("#txtQty").val(pieceQty);
        //$("#cbConsumpUnit").val(conSumUnit).trigger("change");
        //$("#txtUnitConsumption").val(unitConsumption);
    }
    $("#sbItemCode").text(itemcode);
    $("#sbItemName2").text(sbItemName);
    $("#opGroupName").text(opGroupName);
    //=======================
    if (opType === "I") {
        $("#cbInput").prop("checked", true);
        $("#cbOutput").prop("checked", false);
    } else {
        $("#cbInput").prop("checked", false);
        $("#cbOutput").prop("checked", true);
    }
    $("#PatternSerial").val(patternSerial);
    $("#PatternModel").modal("show");
}

function ShowBoxDrag(target, ui, who) {
    $("#divUpdate").hide();
    $("#divLink").show();
    $("#btnAddPattern").html(btnAddText);
    RemoveError();
    //var conSumUnit = "";
    var unitConsumption = "1";
    var qty = "1";
    var grid;
    var rowidPt = 0;
    var rowidBom;
    var chek = 1;
    var patternSerial = "";
    var rowdragg;
    var checkMulti;
    var showPopup = 1;
    if (target === 1) {
        //Bomt dragg
        grid = Pattern_Grid_TB;
        rowidBom = ui.draggable.attr("id");
        patternSerial = iniTialpattern;
        showPopup = 1;
        rowdragg = rowidBom;
        checkMulti = 0;
    } else if (target === 3) {
        // pattern dragg
        grid = ui.draggable.parent().parent().attr("id");
        rowidPt = ui.draggable.attr("id");
        rowidBom = grid.split("_")[2];
        patternSerial = $("#" + grid).jqGrid("getCell", rowidPt, "PatternSerial");
        chek = 23;
        showPopup = 2;
        rowdragg = rowidPt;
        checkMulti = 0;
    } else {
        // muilty select
        checkMulti = 1;
        grid = Pattern_Grid_TB;
        rowidBom = ui.draggable.attr("id");
        rowdragg = rowidBom;
        if (who === 3) {
            grid = ui.draggable.parent().parent().attr("id");
            patternSerial = $("#" + grid).jqGrid("getCell", rowdragg, "PatternSerial");
        } else {
            patternSerial = iniTialpattern;
        }
        if (CheckHaveChildInList()) {
            showPopup = 3;
        } else {
            showPopup = 1;
        }
    }
    if (chek === 23) {
        qty = $("#" + grid).jqGrid("getCell", rowidPt, "PieceQtyRest");
        conSumUnit = $("#" + Pattern_Grid_TB).jqGrid("getCell", rowidBom, "ConsumpUnit");
        unitConsumption = $("#" + Pattern_Grid_TB).jqGrid("getCell", rowidBom, "UnitConsumption");
    } else {
        conSumUnit = $("#" + Pattern_Grid_TB).jqGrid("getCell", rowidBom, "ConsumpUnit");
        unitConsumption = $("#"+Pattern_Grid_TB).jqGrid("getCell", rowidBom, "UnitConsumption");
    }
    //Save grid dragg
    $("#GridDragg").val(grid);
    // bomt ConsumpUnit
    var itemcode = Operation_Grid.jqGrid("getCell", rowDropTo, "OpNum");
    var sbItemName = Operation_Grid.jqGrid("getCell", rowDropTo, "OpNameLan");
    //var opGroupName = Operation_Grid.jqGrid("getCell", rowDropTo, "OpGroupName");
    var rowPid = $(grid_selector).jqGrid("getGridParam", "selrow");
    var itemColorSerial = $("#"+Pattern_Grid_TB).jqGrid("getCell", rowidBom, "ItemColorSerial");
    var mainItemColorSerial = $("#"+Pattern_Grid_TB).jqGrid("getCell", rowidBom, "MainItemColorSerial");
    $("#sbItemCode").text(itemcode);
    $("#sbItemName").text(sbItemName);
    //$("#opGroupName").text(opGroupName);
    ShowHideInPut(showPopup);
    if (showPopup === 3) {
        RemoveValue();
    } else if(showPopup === 1){
        $("#txtQty").val(qty);
        $("#cbConsumpUnit").val(conSumUnit).trigger("change");
        $("#txtUnitConsumption").val(unitConsumption);
    } else {
        $("#txtQty").val(qty);
        $("#cbConsumpUnit").val("").trigger("change");
        $("#txtUnitConsumption").val("");
    }
    //=======================
    $("#ItemColorSerial").val(itemColorSerial);
    $("#MainItemColorSerial").val(mainItemColorSerial);
    $("#PatternSerial").val(patternSerial);
    $("#PatternModel").modal("show");
    AddToTempGrid();
}

function CreateGridTemp() {
    $("#" + tempGrid).jqGrid({
        datatype: "local",
        //scroll: false,
        viewrecords: true,
        editurl: "clientArray",
        //scrollrows: true,
        //shrinkToFit: false,
        //width: null,
        //gridview: true,
        colModel: [
            {name: "MainItemCode", index: "MainItemCode", label: arrPatternCollName.MainItemCode},
            {name: "ItemCode", index: "ItemCode",label: arrPatternCollName.ItemCode},
            {name: "ItemName", index: "ItemName",label: arrPatternCollName.ItemName, width: 300},
            //{name: "ItemColorWays", index: "ItemColorWays",label: arrPatternCollName.MainItemColorSerial},
            { label: arrPatternCollName.StyleColorSerial, name: "StyleColorSerial", index: "StyleColorSerial", align: "center" },
            {
                label: arrPatternCollName.UnitConsumption, name: "UnitConsumption", index: "UnitConsumption", editable: true
                , editoptions: {
                    maxlength: 8,
                    dataInit: function(element) {
                        $(element).keypress(function (e) {
                            return isDecimalNumber(e);
                        })
                    }
                }
            },
            {
                label: arrPatternCollName.ConsumpUnit, name: "ConsumpUnit", index: "ConsumpUnit", editable: true, edittype: 'select',
                editoptions: {
                    value: '0:Please select',
                    dataInit: function (elem) {
                        var selectVal = $(elem).parent().parent().find('td[aria-describedby="' + tempGrid + '_ConsumpUnit"]').attr("title");
                        $(elem).html($("#cbConsumpUnit").html()).val(selectVal).trigger("change");
                    }
                }
            },         
            //{ label: arrPatternCollName.Qty, name: "Qty", index: "Qty", align: "center", width: 60 },
            { name: "CurrCode", index: "CurrCode", hidden: true },
            { name: "StyleCode", index: "StyleCode", hidden: true },
            { name: "StyleSize", index: "StyleSize", hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "RevNo", index: "RevNo", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: "PatternCode", index: "PatternCode", hidden: true },
            { name: "Parent", index: "Parent", hidden: true },
            { name: "BomOrPattern", index: "BomOrPattern", hidden: true },
            { name: "PatternSerial", index: "PatternSerial", hidden: true }
        ],
        onCellSelect: function (rowid, cellidx) {
            var cm = $("#" + tempGrid).jqGrid("getGridParam", "colModel");
            var colNameAttr = cm[cellidx].name;
            if (colNameAttr !== "subgrid") {
                OpenRowEdit(tempGrid, rowid);
            }
        },
        
        subGrid: true,
        //subGridWidth:"100%",
        subGridOptions: {
            plusicon: "ui-icon-plus",
            minusicon: "ui-icon-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: false
        },
       
        subGridRowExpanded: function (subgridId, rowId) {
            var rowData = $("#" + tempGrid).getRowData(rowId);
            var subgridTableId = subgridId + "_t";
            $("#" + subgridId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
            $("#" + subgridTableId).jqGrid({
                datatype: "local",
                data: GetDatFromSub(rowData),
                colModel: [
                    {
                        label: arrPatternChilName.Url,name: "Url",index: "Url",align: "center",width: 100,
                        formatter: function (cellvalue, options) {
                            var id = options.rowId;
                            if (cellvalue)
                                return "<img id='" +
                                    id +
                                    "' class='imgpattern' onclick = ShowPatternImage('" +
                                    cellvalue +
                                    "'); src='" +
                                    cellvalue +
                                    "'/>";
                            return "";
                        }
                    },
                    { label: arrPatternChilName.Piece, name: "Piece", index: "Piece", width:300 },
                    { label: arrPatternChilName.Width, name: "Width", index: "Width", align: "center" },
                    { label: arrPatternChilName.Height, name: "Height", index: "Height", align: "center" },
                    //{ label: arrPatternChilName.MainPart, name: "MainPartName", index: "MainPartName", width: 80 },
                    //{ label: arrPatternChilName.EndWise, name: "EndWise", index: "EndWise", align: "center", width: 80 },
                    {
                        label: arrPatternChilName.PieceQty, name: "PieceQty", index: "PieceQty", editable: true,
                        align: "center", width: 100, closeOnEscape: true
                        , editoptions: {
                            maxlength: 3,
                            dataInit: function (element) {
                                $(element).keypress(function (e) {
                                    return isNumber(e);
                                })
                            }
                        }
                    },
                    { label: arrPatternCollName.PieceUnique, name: "PieceUnique", index: "PieceUnique" },
                    //{
                    //    label: arrPatternChilName.UnitConsumption, name: "UnitConsumption", index: "UnitConsumption", editable: true
                    //    , align: "center", width: 120, closeOnEscape: true
                    //},
                    //{ label: arrPatternChilName.PieceUnique, name: "PieceUnique", index: "PieceUnique", align: "center", width: 120 },
                   // { label: arrPatternChilName.PatternSerial, name: "PatternSerial", index: "PatternSerial", width: 80 },
                    { name: "Currcode", index: "Currcode", hidden: true },
                    { name: "MainPart", index: "MainPart", hidden: true },
                    { name: "SizeUnit", index: "SizeUnit", hidden: true },
                    { name: "ConsumpUnit", index: "ConsumpUnit", hidden: true },
                    { name: "Area", index: "Area", hidden: true },
                    { name: "ItemCode", index: "ItemCode", hidden: true },
                    { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
                    { name: "MainItemCode", index: "MainItemCode", hidden: true },
                    { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
                    { name: "PieceQtyRest", index: "PieceQtyRest", hidden: true },
                    { name: "Parent", index: "Parent", hidden: true },
                    { name: "BomOrPattern", index: "BomOrPattern", hidden: true },
                    { name: "ItemName", index: "ItemName", hidden: true },
                    { name: "PatternSerial", index: "PatternSerial", hidden: true }
                ],
                onSelectRow: function (rowid) {
                    OpenRowEdit(subgridTableId, rowid);
                },
                viewrecords: true,
                //autowidth: true, 
                //shrinkToFit: false,
                height: "100%",
                rownumbers: true,
                pager: "#jqGridPager" + "_" + subgridId
            });
        }
    });
    
}

function OpenRowEdit(gridId, rowid) {
    if (_rowOpened !== null) {
        if (_gridOldEdit !== gridId || _rowOpened != rowid) {
            var result = SaveRow(_gridOldEdit, _rowOpened);
            if (!result) {
                return;
            }
        }
    }
    $("#" + gridId).jqGrid("editRow", rowid, false);
    _gridOldEdit = gridId;
    _rowOpened = rowid;
}

function SaveRow(gridId, rowid) {
    if (gridId === null) return true;
    var ConsumpUnit = "";
    RemoveError();
    if (gridId != tempGrid) {
        var qty = $("#" + rowid + "_PieceQty").val();
        var qtyCanlink = $("#" + gridId).jqGrid("getCell", rowid, "PieceQtyRest");
        if (qty.length === 0 || qty <= 0) {
            $("#" + rowid + "_PieceQty").addClass("error");
            ShowValidateByItem("001", SmsFunction.Add, MessageType.Error, MessageContext.Communication, Type.Error, qtyCanlink);//7
            return false;
        } else if (qty > qtyCanlink) {
            ShowValidateByItem("001", SmsFunction.Add, MessageType.Error, MessageContext.Communication, Type.Error, qtyCanlink);//7
            return false;
        }
    }
    if (gridId != null) {
        jQuery("#" + gridId).saveRow(rowid, {
            successfunc: function () {
                return true;
            }
        }, null);
    }
    else {
        if (_gridOldEdit !== null && _rowOpened !== null) {
            SaveRow(_gridOldEdit, _rowOpened);
        }
    }
    if (gridId === tempGrid) {
        $("#" + gridId).jqGrid("setCell", rowid, "ConsumpUnit", ConsumpUnit);
    }
    _gridOldEdit = null;
    _rowOpened = null;
    return true;
}

function AddToTempGrid() {
    $("#" + tempGrid).jqGrid("clearGridData");
    _subGridProt = [];
    var rowPid = $(grid_selector).jqGrid("getGridParam", "selrow");
    var gridDrag = $("#GridDragg").val();
    var rowOp = $(grid_selector).jqGrid("getRowData", rowPid);
    var edition = (rowOp.Edition).charAt(0);
    var listBomt = [];
    var i = 1;
    $("#" + Pattern_Grid_TB).find('tr[role="row"]:not(.ui-jqgrid-labels)').find("input:checkbox:checked")
        .each(function () {
        if ($(this).parent().parent().parent().is(".ui-jqgrid-labels")) return true;
        i++;
        var id = $(this).attr("id");
        var idSplit = id.split("_");
        var l = idSplit.length;
        var rowid;
        var prot = {
            StyleCode: rowOp.StyleCode,
            StyleSize: rowOp.StyleSize,
            StyleColorSerial: rowOp.StyleColorSerial,
            RevNo: rowOp.RevNo,
            ItemCode: "",
            ItemColorSerial: "",
            MainItemCode: "",
            MainItemColorSerial: "",
            PatternSerial: "",
            OpRevNo: Operation_Grid.jqGrid("getCell", rowDropTo, "OpRevNo"),
            OpSerial: Operation_Grid.jqGrid("getCell", rowDropTo, "OpSerial"),
            OpType: "",
            Status: "",
            ConsumpUnit: $("#cbConsumpUnit").val(),
            UnitConsumption: rowOp.UnitConsumption,
            Edition: edition,
            PieceQty: "",
            Url: "",
            BomOrPattern: "",
            PieceQtyRest: "",
            PieceUnique: "",
            Parent: "None",
            Width: "",
            Height: ""
        };
        if (l === 4) {
            prot.PatternSerial = iniTialpattern;
            prot.BomOrPattern = bomt;
        } else {
            var gridchild = idSplit[1] + "_" + idSplit[2] + "_" + idSplit[3] + "_" + idSplit[4];
            rowid = idSplit[5];
            var childData = $("#" + gridchild).jqGrid("getRowData", rowid);
            prot.Piece = childData.Piece;
            prot.PatternSerial = childData.PatternSerial;
            prot.UnitConsumption = childData.UnitConsumption;
            prot.BomOrPattern = pattern;
            prot.PieceQty = childData.PieceQtyRest;
            prot.Url = $("#" + gridchild).find('tr[id="' + rowid + '"]').find('td[aria-describedby="' + gridchild + '_Url"]').children().attr("src");
            prot.Width = childData.Width;
            prot.Height = childData.Height;
            prot.PieceQtyRest = childData.PieceQtyRest;
            prot.PieceUnique = childData.PieceUnique;
        }
        rowid = idSplit[3];
        var currentRowData = $("#" + Pattern_Grid_TB).jqGrid("getRowData", rowid);
        prot.ItemCode = currentRowData.ItemCode;
        prot.ItemName = currentRowData.ItemName;
        prot.ItemColorSerial = currentRowData.ItemColorSerial;
        prot.MainItemCode = currentRowData.MainItemCode;
        prot.MainItemColorSerial = currentRowData.MainItemColorSerial;
        //
        if (prot.PatternSerial === iniTialpattern) {
            $("#" + tempGrid).jqGrid("addRowData", i, prot);
            listBomt.push(prot);
        }
        else {
            // if it is not have parent, get 1 row insert and set it is parent
            // how can i do it?
        if (!ChekHaveChild(prot, listBomt) || listBomt.length === 0) {
                // do some thing here it mean append perent prot
                var newprot = jQuery.extend({}, prot);
                newprot.Parent = "Yes";
                $("#" + tempGrid).jqGrid("addRowData", i, newprot);
                listBomt.push(newprot);
            }
            _subGridProt.push(prot);
        }
        return true;
        });
    UnbinClick();
    $('#' + tempGrid).find('tr td[aria-describedby="' + tempGrid + '_subgrid"]').trigger('click');
    _gridOldEdit = null;
    _rowOpened = null;
}

function UnbinClick() {
    var rows = $("#" + tempGrid).getDataIDs();
    for (let a = 0; a < rows.length; a++) {
        row = $("#" + tempGrid).getRowData(rows[a]);
        var rowid = rows[a];
        var check = ChekHaveChild(row,_subGridProt);
        if (!check) {
            $("#" + tempGrid).find('tr[id="' + rowid + '"]').find('td[aria-describedby="' + tempGrid + '_subgrid"]').unbind("click").html("");
        }
    }
}

function GetDatFromSub(data) {
    var newprots = [];
    _subGridProt.forEach(function (a) {
        if (ChekChild(data, a)) {
            newprots.push(a);
        }
    });
    return newprots;
}

function ChekChild(bomt, prot) {
    if (bomt.StyleCode === prot.StyleCode &&
        bomt.StyleSize === prot.StyleSize &&
        bomt.StyleColorSerial === prot.StyleColorSerial &&
        bomt.RevNo === prot.RevNo &&
        bomt.ItemCode === prot.ItemCode &&
        bomt.ItemColorSerial === prot.ItemColorSerial &&
        bomt.MainItemCode === prot.MainItemCode &&
        bomt.ItemColorSerial === prot.ItemColorSerial
        )
        return true;
    return false;
}

function ChekHaveChild(rowData, listProd) {
    if (listProd.length === 0) {
        return false;
    }
    var result = false;
    listProd.forEach(function (a) {
        if (ChekChild(rowData, a)) {
            result = true;
            return false;
        }
    });
    return result;
}

function AddProtToSession() {
    //validate
    RemoveError();
    var btnText = $("#btnAddPattern").html();
    var isAdd = true
    if (btnText !== btnUpdateText) {
        isAdd = true;
    } else isAdd = false;
    var arr = [];
    _checkEdit = true;
    var rowPid = $(grid_selector).jqGrid("getGridParam", "selrow");
    var gridDrag = $("#GridDragg").val();
    var serial = $("#PatternSerial").val();
    var opType = $("#cbInput").is(":checked") ? "I" : "O";
    var rowOp = $(grid_selector).jqGrid("getRowData", rowPid);
    var rowOpDetail = $(Operation_Grid).jqGrid("getRowData", rowDropTo);
    var edition = (rowOp.Edition).charAt(0);
    var unitConSumtion = parseFloat($("#txtUnitConsumption").val());
    if (!unitConSumtion) {
        unitConSumtion = "";
    }
    /// Save row before change to server
    var rowEditing = $("#" + tempGrid).jqGrid("getGridParam", "selrow");
    if (isAdd) {
        var rsult = SaveRow(_gridOldEdit, _rowOpened);
        if (!rsult) {
            return false;
        }
        arr = ValidateAdd();
        if (arr.length > 0) {         
            ShowValidateByItem("001", SmsFunction.BeforeChange, MessageType.Warning, MessageContext.IgnoreChanges, Type.Error, " PieceQty");// 2
            return false;
        }
        $("#" + tempGrid).find('tr[role="row"]:not(.ui-jqgrid-labels, .jqgfirstrow, .ui-subgrid)').each(function () {
            var rowid = $(this).attr("id");
            var rowUpdate;
            var whereRow = $(this).find('td[aria-describedby$="_Parent"]').html();
            var gridId = tempGrid;
            if (whereRow === "None")
            {
                gridId = $(this).parent().parent().attr("id");
            }else {
                return true;
            }
            rowUpdate = $("#" + gridId).jqGrid("getRowData", rowid);
            var prot = {
                    StyleCode: rowOp.StyleCode,
                    StyleSize: rowOp.StyleSize,
                    StyleColorSerial: rowOp.StyleColorSerial,
                    Revno: rowOp.RevNo,
                    ItemCode: rowUpdate.ItemCode,
                    ItemName: rowUpdate.ItemName,
                    ItemColorSerial: rowUpdate.ItemColorSerial,
                    MainItemCode: rowUpdate.MainItemCode,
                    MainItemColorSerial: rowUpdate.MainItemColorSerial,
                    PatternSerial: rowUpdate.PatternSerial,
                    OpRevNo: rowOpDetail.OpRevNo,
                    OpSerial: rowOpDetail.OpSerial,
                    OpType: opType,
                    Status: "",
                    ConsumpUnit: rowUpdate.ConsumpUnit,
                    UnitConsumption: rowUpdate.UnitConsumption,
                    Edition: edition,
                    Piece: rowUpdate.Piece,
                    PieceQty: rowUpdate.PieceQty,
                    Url: $("#" + gridId).find('tr[id="' + rowid + '"]').find('td[aria-describedby="' + gridId + '_Url"]').children().attr("src"),
                    Width: rowUpdate.Width,
                    Height: rowUpdate.Height,
                    PieceUnique: rowUpdate.PieceUnique,
                    CurPatternSerial: rowUpdate.CurPatternSerial,
                    BomOrPattern: rowUpdate.BomOrPattern
                };
                prots.push(prot);
                return true;
            });
        var lprot = JSON.stringify({ 'prots': prots });
        $("#PatternModel").modal("hide");
        prots = [];
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/AddSessionProts",
            data: lprot,
            success: function(data) {
                GetBomtExpand();
                GetOperationParentExpand();
                GetOperationExpand();
                Operation_Grid.trigger("reloadGrid");
                $("#" + Pattern_Grid_TB).trigger("reloadGrid");
            },
            err: function(response) {
                console.log(response);
            }
        });
    } else {
        //===validate 20171214===================
        var arrEro = ValidateForm(btnText);
        RemoveError();
        if (arrEro.length > 0) {
            arrEro.forEach(function (a) {
                if (a === 1) {
                    $("#txtQty").addClass("error");
                    ShowValidateByItem("001", SmsFunction.BeforeChange, MessageType.Warning, MessageContext.IgnoreChanges, Type.Error, " Piece Qty > 0");//2
                }
            });
            return;
        }
        //============================
        var rowSelect = $("#rowSelect").val();
        var rowCurrentData = $("#" + gridDrag + "_t").jqGrid("getRowData", rowSelect);
        var prot = {
            styleCode: rowOp.StyleCode,
            styleSize: rowOp.StyleSize,
            styleColorSerial: rowOp.StyleColorSerial,
            revno: rowOp.RevNo,
            ItemCode: rowCurrentData.ItemCode,
            ItemColorSerial: rowCurrentData.ItemColorSerial,
            MainItemCode: rowCurrentData.MainItemCode,
            MainItemColorSerial: rowCurrentData.MainItemColorSerial,
            PatternSerial: serial === iniTialpattern ? iniTialpattern : serial,
            OpRevNo: rowCurrentData.OpRevNo,
            OpSerial: rowCurrentData.OpSerial,
            OpType: rowCurrentData.OpType, // will compare for OpType
            Status: "",
            ConsumpUnit: $("#cbConsumpUnit").val(),
            UnitConsumption: unitConSumtion,
            Edition: rowCurrentData.Edition,
            PieceQty: $("#txtQty").val(),
            Url: rowCurrentData.Url,
            CurPatternSerial: rowCurrentData.CurPatternSerial,
            BomOrPattern: opType // will update for OpType
        };
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/UpdateSessionProts",
            data: JSON.stringify({ 'prot': prot }),
            success: function (data) {
                if (data !== 0) {
                    $("#txtQty").addClass("error");
                    ShowValidateByItem("001", SmsFunction.Add, MessageType.Error, MessageContext.Communication, Type.Error, data);//7
                }
                else {
                    // update when susses
                    $("#PatternModel").modal("hide");
                    $("#" + gridDrag + "_t").jqGrid("setCell", rowSelect, "OpType", prot.OpType);
                    $("#" + gridDrag + "_t").jqGrid("setCell", rowSelect, "ConsumpUnit", prot.ConsumpUnit);
                    $("#" + gridDrag + "_t").jqGrid("setCell", rowSelect, "UnitConsumption", prot.UnitConsumption);
                    $("#" + gridDrag + "_t").jqGrid("setCell", rowSelect, "PieceQty", prot.PieceQty);
                    $("#" + gridDrag + "_t").jqGrid("setCell", rowSelect, "OpType", prot.BomOrPattern);
                    GetBomtExpand();
                    $("#" + Pattern_Grid_TB).trigger("reloadGrid");
                }
            },
            err: function (response) {
                console.log(response);
            }
        });
    }
}

function CheckHaveChildInList() {
    var result = false;
    $("#" + Pattern_Grid_TB).find('tr[role="row"]:not(.ui-jqgrid-labels)').find("input:checkbox:checked")
        .each(function () {
            var id = $(this).attr("id");
            var idSplit = id.split("_");
            var l = idSplit.length;
            if (l !== 4) {
                result = true;
            }
        });
    return result;
}

//==============================================
function SelectRow(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId) {
    Operation_Grid.jqGrid("setGridParam", {
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColor: styleColorSerial, revNo: revNo, opRevNo: opRevNo, edition: edition, languageId: languageId
        }
    }).trigger("reloadGrid");

    $("#Pattern_Grid").jqGrid("setGridParam", {
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo, opRevNo: opRevNo, edition: edition
        }
    }).trigger("reloadGrid");
}

function AppendMcmt() {
    FillDataToDropDownlist("cbConsumpUnit", GetMasterCode("UnitCode"), "SubCode", "CodeName");
    setTimeout(function () {
        $("#cbConsumpUnit").val("Cm").trigger("change");
    },800);
}

function RemoveBom(cellvalue, options, rowObject) {
    var id = options.rowId;
    var refNo = rowObject.OpSerial;
    var bom = rowObject.PatternSerial;
    var html = "";
    if (bom === iniTialpattern) {
        html = "<a id='" + id + "' title ='Remove' onclick=\"RemoveRow(this,'" + id + "','" + iniTialpattern + "')\" class='memberAction' name='" + refNo + "' type='button'>X</a>";
    }
    return html;
}
function Remove(cellvalue, options, rowObject) {
    var id = options.rowId;
    var refNo = rowObject.OpSerial;
    var html = "<a id='" + id + "' title ='Remove' onclick=\"RemoveRow(this,'" + id + "')\" class='memberAction' name='" + refNo + "' type='button'>X</a>";
    return html;
}
//action button=================

function RemoveRow(thing, id, bom) {
    if (!CheckDragg()) {
        return;
    }
    ConfirmRemove(function () {
        _checkEdit = true;
        var grid = $(thing).parent().parent().parent().parent().attr("id");
        var arr = grid.split("_");
        var rowParrent = arr[2];
        var rowDataP = Operation_Grid.jqGrid("getRowData", rowParrent);
        var rowDataC = $("#" + grid).jqGrid("getRowData", id);
        var pt = (bom === iniTialpattern ? iniTialpattern : rowDataC.PatternSerial);
        var pr = {
            styleCode: rowDataP.StyleCode,
            styleSize: rowDataP.StyleSize,
            styleColorSerial: rowDataP.StyleColorSerial,
            revno: rowDataP.RevNo,
            ItemCode: rowDataC.ItemCode,
            ItemColorSerial: rowDataC.ItemColorSerial,
            MainItemCode: rowDataC.MainItemCode,
            MainItemColorSerial: rowDataC.MainItemColorSerial,
            PatternSerial: pt,
            OpRevNo: rowDataP.OpRevNo,
            OpSerial: rowDataP.OpSerial,
            OpType: rowDataC.OpType,
            Edition: rowDataC.Edition,
            Status: "",
            CurPatternSerial: rowDataC.CurPatternSerial,
            BomOrPattern: bom === iniTialpattern ? bomt : pattern
        };
        prots.push(pr);
        var lprot = JSON.stringify({ 'prots': prots });
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/RemoveSessionProts",
            data: lprot,
            success: function (data) {
                GetBomtExpand();
                GetOperationParentExpand();
                GetOperationExpand();
                Operation_Grid.trigger("reloadGrid");
                $("#" + Pattern_Grid_TB).trigger("reloadGrid");
            },
            err: function () {
            }
        });
        prots = [];
    });
}

function RemoveAll(thing, id) {
    if (!CheckDragg()) {
        return;
    }
    _checkEdit = true;
    ConfirmRemove(function () {
        var grid = $(thing).parent().attr("id");
        var arr = grid.split("_");
        grid = arr[0] + "_" + arr[1] + "_" + arr[2] + "_t";
        var rowOpData = Operation_Grid.jqGrid("getRowData", id);
        var patternSerial = rowOpData.MainItemColorSerial;
        $("#" + grid).find('tr[role="row"]').each(function () {
            if ($(this).is(".jqgfirstrow")) {
                return;
            }
            var curentid = $(this).attr("id");
            var curentData = $("#" + grid).jqGrid("getRowData", curentid);
            var pr = {
                styleCode: rowOpData.StyleCode,
                styleSize: rowOpData.StyleSize,
                styleColorSerial: rowOpData.StyleColorSerial,
                revno: rowOpData.RevNo,
                ItemCode: curentData.ItemCode,
                ItemColorSerial: curentData.ItemColorSerial,
                MainItemCode: curentData.MainItemCode,
                MainItemColorSerial: curentData.MainItemColorSerial,
                PatternSerial: curentData.PatternSerial,
                OpRevNo: rowOpData.OpRevNo,
                OpSerial: rowOpData.OpSerial,
                OpType: curentData.OpType,
                Edition: curentData.Edition,
                Status: "",
                CurPatternSerial: curentData.CurPatternSerial,
                BomOrPattern: patternSerial === iniTialpattern ? bomt : pattern
            };
            prots.push(pr);
        });
        //Close before remove
        Operation_Grid.find('tr[id="' + id + '"]').find('td[aria-describedby="Operation_Grid_subgrid"]').trigger("click");
        var lprot = JSON.stringify({ 'prots': prots });
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/RemoveSessionProts",
            data: lprot,
            success: function (data) {
                GetBomtExpand();
                GetOperationParentExpand();
                GetOperationExpand();
                Operation_Grid.trigger("reloadGrid");
                $("#" + Pattern_Grid_TB).trigger("reloadGrid");
            }
        });
        prots = [];
    });
}

function RemoveAllPattern(thing, id) {
    if (!CheckDragg()) {
        return;
    }
    ConfirmRemove(function () {
        _checkEdit = true;
        var grid = $(thing).parent().attr("id");
        var arr = grid.split("_");
        grid = arr[0] + "_" + arr[1] + "_" + arr[2] + "_t_" + arr[4] + "_t";
        var rowOpid = arr[2];
        var rowOpData = Operation_Grid.jqGrid("getRowData", rowOpid);
        $("#" + grid).find('tr[role="row"]').each(function () {
            if ($(this).is(".jqgfirstrow")) {
                return;
            }
            var curentid = $(this).attr("id");
            var curentData = $("#" + grid).jqGrid("getRowData", curentid);
            var pr = {
                styleCode: rowOpData.StyleCode,
                styleSize: rowOpData.StyleSize,
                styleColorSerial: rowOpData.StyleColorSerial,
                revno: rowOpData.RevNo,
                ItemCode: curentData.ItemCode,
                ItemColorSerial: curentData.ItemColorSerial,
                MainItemCode: curentData.MainItemCode,
                MainItemColorSerial: curentData.MainItemColorSerial,
                PatternSerial: curentData.PatternSerial,
                OpRevNo: rowOpData.OpRevNo,
                OpSerial: rowOpData.OpSerial,
                OpType: curentData.OpType,
                Edition: curentData.Edition,
                Status: "",
                BomOrPattern: curentData.OpSerial === iniTialpattern ? bomt : pattern
            };
            prots.push(pr);
        });
        var lprot = JSON.stringify({ 'prots': prots });
        $.ajax({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            type: "POST",
            url: "/OpsLink/RemoveSessionProts",
            data: lprot,
            success: function (data) {
                GetBomtExpand();
                GetOperationParentExpand();
                GetOperationExpand();
                Operation_Grid.trigger("reloadGrid");
                $("#" + Pattern_Grid_TB).trigger("reloadGrid");
            }
        });
        prots = [];
    });
}

function GetBomtExpand() {
    _bomep = true;
    $("#" + Pattern_Grid_TB).find('td.sgexpanded[aria-describedby="Pattern_Grid_subgrid"]').each(function() {
        var id = $(this).parent().attr("id");
        _bomExpand.push(id);
    });
}

function GetOperationParentExpand() {
    _opepp = true;
    //ui-icon ace-icon fa fa-plus
    //ui-icon ace-icon fa fa-minus
    Operation_Grid.find("tr.jqgroup").find("span.fa-minus").each(function () {
        var id = $(this).parent().parent().attr("id");
        _opExpandP.push(id);
    });
}

function GetOperationExpand() {
    _opep = true;
    Operation_Grid.find('td.sgexpanded[aria-describedby="Operation_Grid_subgrid"]').each(function () {
        var id = $(this).parent().attr("id");
        _opExpand.push(id);
    });
}

function ExpandBomt() {
    if (_bomep) {
        _bomExpand.forEach(function(a) {
            $("#" + Pattern_Grid_TB).find('tr[id="' + a + '"]').find('td[aria-describedby="' + Pattern_Grid_TB + '_subgrid"]').trigger("click");
        });
    }
    _bomep = false;
    _bomExpand = [];
}

function ExpandOperation() {
    if (_opep) {
        _opExpandP.forEach(function (a) {
            $("#" + a).find("span").trigger("click");
        });
        _opExpand.forEach(function (a) {
            Operation_Grid.find('tr[id="' + a + '"]').find('td[aria-describedby="' + Operation_Grid_TB + '_subgrid"]').trigger("click");
        });
    }
    _opExpandP = [];
    _opExpand = [];
    _opepp = false;
    _opep = false;
}

function CheckEpandRowDrop(row, arr) {
    var value = false;
    if (arr.length === 0) {
        value = false;
    }
    arr.forEach(function(x) {
        if (x === row) {
            value = true;
        }
    });
    return value;
}

function SaveProt(isAlert) {
    if (!CheckDragg()) {
        return;
    }
    $.ajax({
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST",
        url: "/OpsLink/Save",
        data: {},
        success: function (data) {
            _checkEdit = false;
            if (isAlert) {
                if (data === 1 || data === 2) {
                    ShowValidateByItem("001", SmsFunction.Add, MessageType.Success, MessageContext.Save, Type.Success);//4
                } else {
                    ShowValidateByItem("001", SmsFunction.Update, MessageType.Error, MessageContext.Save, Type.Error);//14
                }
                GetBomtExpand();
                GetOperationParentExpand();
                GetOperationExpand();
                Operation_Grid.trigger("reloadGrid");
                $("#" + Pattern_Grid_TB).trigger("reloadGrid");
            }
        },
        err: function () {
            ShowValidateByItem("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, Type.Error);//14
        }
    });
    prots = [];
 }

function CheckDragg() {
    return checkDragg;
}

function RemoveDraggAble(gridId, rowId) {
    $("#" + gridId).jqGrid("setCell", rowId, "Status", linked, { background: bgLinked });
    if (gridId === Pattern_Grid_TB) {
        $('tr[id="' + rowId + '"]').find('td[aria-describedby="' + gridId +'_Status"]').parent().removeClass("Itemrow-draggable");
    } else {
        $('tr[id="' + rowId + '"]').find('td[aria-describedby="' + gridId + '_Status"]').parent().removeClass("Child-draggable");
    }
    $("#jqg_" + gridId + "_" + rowId).prop("checked", false).attr("disabled", true);
}

function ResetVariable() {
      checkDragg = false;
      prots = [];
     _bomExpand = [];
     _opExpand = [];
     _opExpandP = [];
     _bomep = false;
     _opep = false;
     _opepp = false;
     _checkEdit = false;
}

function ExpandSubGrid(gridId,expandedRowId) {
    //$("#" + gridId).jqGrid("expandSubGridRow", expandedRowId);
    $("#" + gridId+"_" + expandedRowId+"_t").trigger("reloadGrid");
}

function ShowHideInPut(isBomt) {
    if (isBomt == 1) {
        // Bomt
        $("#txtQty").prop("disabled", true);
        $("#cbConsumpUnit").prop("disabled", false);
        $("#txtUnitConsumption").prop("disabled", false);
    } else if(isBomt == 2){
        // Prot
        $("#txtQty").prop("disabled", false);
        $("#cbConsumpUnit").prop("disabled", true);
        $("#txtUnitConsumption").prop("disabled", true);
    } else {
        // bom and pattern
        $("#txtQty").prop("disabled", false);
        $("#cbConsumpUnit").prop("disabled",false);
        $("#txtUnitConsumption").prop("disabled", false);
    }
}

function ShowPoupUp(title, value) {
    $("#warning-title").text(title);
    $("#waring-body").text(value);
    $("#warning").modal("show");
    setTimeout(function () {
        $("#warning").modal("hide");
    },2000);
}

//============Validate
function ValidateCheckBox() {
    var cbIp = $("#cbInput").is(":checked");
    var cbOp = $("#cbOutput").is(":checked");
    if (!cbIp && !cbOp) return false;
    return true;
}

function ValidateAdd() {
    var arr = [];
    $("#" + tempGrid).find('tr[role="row"]:not(.ui-jqgrid-labels, .jqgfirstrow, .ui-subgrid)').each(function () {
        var rowid = $(this).attr("id");
        var rowUpdate;
        var whereRow = $(this).find('td[aria-describedby$="_Parent"]').html();
        if (whereRow !== "None") {
            return true;
        } 
        var gridId = $(this).parent().parent().attr("id");
        rowUpdate = $("#" + gridId).jqGrid("getRowData", rowid);
        if (gridId === tempGrid) {
            /*
            if (!rowUpdate.ConsumpUnit) {
                // red row
                $("#" + tempGrid).find('tr[id="' + rowid + '"]').find('td[aria-describedby="' + tempGrid + '_ConsumpUnit"]').addClass("alert-danger");
                arr.push(1);
                return false;
            } else if (!rowUpdate.UnitConsumption) {
                // red row
                $("#" + tempGrid).find('tr[id="' + rowid + '"]').find('td[aria-describedby="' + tempGrid + '_UnitConsumption"]').addClass("alert-danger");
                arr.push(2);
                return false;
            }
            */
        }
        else {
            if (!rowUpdate.PieceQty) {
                $("#" + gridId).find('tr[id="' + rowid + '"]').find('td[aria-describedby="' + gridId + '_PieceQty"]').addClass("alert-danger");
                arr.push(3);
                return false;
            }
        }
    });
    return arr;

    //var whereRow = $(this).find('td[aria-describedby$="_Parent"]').html();
    //var gridId = tempGrid;
    //if (whereRow === "None") {
    //    gridId = $(this).parent().parent().attr("id");
    //} else {
    //    return true;
    //}
}

function ValidateForm(buttonName) {
    var arr = [];
    if ($('#txtQty').is(':disabled')) {
        // this is bomt
        /*
        var unCon = $("#txtUnitConsumption").val();
        var conSum = $("#cbConsumpUnit").val();
        if (unCon.length === 0 || unCon <= 0) {
            arr.push(2);
        }
        if (conSum === null || conSum === "") {
            arr.push(3);
        }
        */
    } else {
        // this is pattern
        var qty = $("#txtQty").val();
        if (qty.length === 0 || qty <= 0) {
            arr.push(1);
        }
    }
    return arr;
}

function ResizeGridChild(gridParent, GridChild) {
    var wParent = $("#" + gridParent).Width()-10;
    $("#" + gridName).setGridWidth(w);
}