//#region bind data to gridview
const bindDataToJqGridOpNameDetail = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, languageId, isLinking) => {
    jQuery("#tbOpNameDetail").jqGrid({
        url: '/PlanManagement/GetListOpNameDetail',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            opRevNo: opRevNo,
            opSerial: opSerial,
            edition: edition,
            languageId: languageId,
            isLinking: isLinking
        },
        datatype: "json",
        height: 400,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        gridview: true,
        //multiselect: true,
        caption: "Linked Items",
        colModel: [
            // { name: 'DeleteItemEle', index: 'DeleteItemEle', label: " ", width: 50, align: "center", formatter: formatterDeleteItem },
            { name: 'OpNameSerial', index: 'OpNameSerial', label: "OpName Serial", classes: 'pointer', formatter: function (cellValue, option, rowData) { return `OpName ${rowData.OpnSerial}`; } },
            { name: 'OpName', index: 'OpName', label: "Op Name", width: 650, classes: 'pointer' },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'OpnSerial', index: 'OpnSerial', hiden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'OpNameId', index: 'OpNameId', hidden: true },
            { name: 'OpType', index: 'OpType', hidden: true },
            { name: 'HasBom', index: 'HasBom', hidden: true }
        ],
        gridComplete: function () {
            let ids = jQuery("#tbOpNameDetail").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbOpNameDetail").jqGrid("getRowData", i);
                if (rowdata.HasBom !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbOpNameDetail_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbOpNameDetail_subgrid]").unbind('click');
                }
            }

            //expand row
            expandRowJqgrid('#tbOpNameDetail', _listExpandedRowIdOpNameDt)

            //drop 
            $('td[aria-describedby="tbOpNameDetail_OpType"]').each(function () {
                if (!$(this).is(".jqgroup,.jqgfirstrow,.ui-subgrid")) {
                    $(this).parent().addClass("ui-droppable");
                }
            });
            DroppRowGrid();
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
        subGridRowExpanded: subGridviewLinkedBom
    });
}

const subGridviewLinkedBom = (subgridDivId, rowId) => {
    //get current selected linked item row data
    let rowData = $('#tbOpNameDetail').jqGrid('getRowData', rowId);
    //clear delete element
    rowData.DeleteItemEle = '';
    let subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/PlanManagement/GetLinkedBom',
        postData: {
            styleCode: rowData.StyleCode,
            styleSize: rowData.StyleSize,
            styleColorSerial: rowData.StyleColorSerial,
            revNo: rowData.RevNo,
            opRevNo: rowData.OpRevNo,
            opSerial: rowData.OpSerial,
            opnSerial: rowData.OpnSerial,
            edition: rowData.Edition,
            isLinking: true
        },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        gridview: true,
        //multiselect: true,
        colModel: [
            { name: 'DeleteItemEle', index: 'DeleteItemEle', label: " ", width: 50, align: "center", formatter: formatterDeleteItemOpnDt },
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer' },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 250, classes: 'pointer' },
            { name: 'ItemColorWays', index: 'ItemColorWays', label: "Item Color", width: 150, classes: 'pointer' },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", width: 100, align: 'center', classes: 'pointer' },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Cons.Unit", width: 70, align: 'center' },
            { name: 'MainItemCode', index: 'MainItemCode', hidden: true },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'OpnSerial', index: 'OpnSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'PatternSerial', index: 'PatternSerial', hidden: true },
            { name: 'OpType', index: 'OpType', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }
        ],
        gridComplete: function () {
            //tbOpNameDetail_1_t
            let ids = jQuery(`#${subgridTableId}`).jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $(`#${subgridTableId}`).jqGrid("getRowData", i);
                if (rowdata.HasPattern !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=" + subgridTableId + "_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $(`tr[id=${i}]>td[aria-describedby$=${subgridTableId}_subgrid]`).unbind('click');
                }
            }

            //expand row
            expandRowSubJqgrid(_listExpandedRowIdSubOpNameDt)
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
        subGridRowExpanded: subGridviewLinkedPatterns
    });
}

const subGridviewLinkedPatterns = (subgridDivId, rowId) => {
    const parentTableId = subgridDivId.substring(0, subgridDivId.lastIndexOf("_"));
    //get current selected linked item row data
    let rowData = $(`#${parentTableId}`).jqGrid('getRowData', rowId);
    //clear delete element
    rowData.DeleteItemEle = '';
    let subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/PlanManagement/GetLinkedPatternsOpnDt',
        postData: { objProt: JSON.stringify(rowData) },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        gridview: true,
        //multiselect: true,
        colModel: [
            { name: 'DeletePatternEle', index: 'DeletePatternEle', label: " ", width: 50, align: "center", formatter: formatterDeletePatternOpnDt },
            {
                label: " ",
                name: "ImageLink",
                index: "ImageLink",
                align: "center",
                width: 120,
                formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return `<img id='${id}' class='imgpattern' onclick = showPatternImage('${cellvalue}'); src='${cellvalue}' onerror='imgError(this);'/>`;
                    return "";
                }
            },
            { name: "Piece", index: "Piece", label: "Piece", width: 300 },
            { name: "Width", index: "Width", label: "Width", align: "center", width: 50 },
            { name: "Height", index: "Height", label: "Height", align: "center", width: 50 },
            { name: "EndWise", index: "EndWise", label: "End Wise", align: "center", width: 80 },
            { name: "PieceQty", index: "PieceQty", label: "Qty", width: 50 },
            { name: "ConsumpUnit", index: "ConsumpUnit", label: "Cons.Unit", width: 80 },
            { name: "PatternSerial", index: "PatternSerial", label: "Serial", width: 80 },
            { name: "PieceUnique", index: "PieceUnique", label: "Unique", align: "center", width: 80 },
            { name: "ItemCode", index: "ItemCode", hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "MainItemCode", index: "MainItemCode", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'OpnSerial', index: 'OpnSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'OpType', index: 'OpType', hidden: true }
        ]
    });
}

//#region fortmat column on grid

const formatterDeleteItemOpnDt = (cellValue, option, rowData) => {
    const { OpSerial, OpnSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial, PieceQty } = rowData;
    return `<a id='ach_${option.gid}_${option.rowId}' onclick='deleteBomPatternLinkingOpnDt(${JSON.stringify({ OpSerial, OpnSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial, PieceQty })}, ${true})'>X</a>`;
}

const formatterDeletePatternOpnDt = (cellValue, option, rowData) => {
    const { OpSerial, OpnSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial, PieceQty } = rowData;
    return `<a id='ach_${option.gid}_${option.rowId}' onclick='deleteBomPatternLinkingOpnDt(${JSON.stringify({ OpSerial, OpnSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial, PieceQty })}, ${false})'>X</a>`;
}
//#endregion

//#endregion

//#region reload gridview
const reloadGridTbOpNameDetail = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, languageId, isLinking) => {
    //reload gridview BOM
    var data = {
        styleCode: styleCode,
        styleSize: styleSize,
        styleColorSerial: styleColorSerial,
        revNo: revNo,
        opRevNo: opRevNo,
        opSerial: opSerial,
        edition: edition,
        languageId: languageId,
        isLinking: isLinking
    };
    ReloadJqGrid2LoCal("tbOpNameDetail", data);
}
//#endregion

//#region event
const reloadDataGridviewTbBomPatternLinkingOpnDt = () => {
    //get selected row in sub gridview
    _listTempLinkedPattern = getSelectedRowsSubGrid('#tbBom');
    //get selected row in gridview
    let selectedRowBom = GetSelectedMultipleRowsData('#tbBom');

    //get edition from selected op master
    const selOpMt = GetSelectedOneRowData('#gridOpsTable');
    let opSerial = getOpSerial();
    //get selected row.operation name detail
    const selectedOpNameDt = GetSelectedOneRowData('#tbOpNameDetail');
    _listTempOpNameDt = selectedOpNameDt;
    if ((_listTempLinkedPattern.length === 0 && selectedRowBom === null) || $.isEmptyObject(selectedOpNameDt)) {
        ShowMessage("Linking Bom & Pattern", 'Please select Bom or process', MessageType.Warning);
        return;
    }
    //get op name detail serial
    const opnSerial = selectedOpNameDt.OpnSerial;
    //get all rows of grid bom
    const gridData = jQuery('#tbBom').jqGrid("getRowData");
    //set selectedRowBom with empty array if it is null
    if (selectedRowBom === null) {
        selectedRowBom = [];
    }
    else {
        //update has pattern to N, and set pattern serial is 000
        selectedRowBom.forEach((bom) => {
            bom.HasPattern = 'N';
            bom.PatternSerial = '000';
            bom.OpType = "I";
            bom.OpRevNo = selOpMt.OpRevNo;
            bom.Edition = selOpMt.Edition;
            bom.OpSerial = opSerial;
            bom.OpnSerial = opnSerial;
        });
    }

    //get item of linked patterns
    _listTempLinkedPattern.forEach((pt) => {
        //add total piece qty
        pt.TotalPieceQty = pt.PieceQty;
        pt.OpRevNo = selOpMt.OpRevNo;
        pt.Edition = selOpMt.Edition;
        pt.OpType = "I";
        pt.OpSerial = opSerial;
        pt.OpnSerial = opnSerial;
        //find patterns of pattern
        let item = selectedRowBom.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
        //if pattern doesn't have parents then get it from BOM
        if (item === null || typeof (item) === "undefined") {
            let parentsItem = gridData.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
            parentsItem.HasPattern = 'Y';
            parentsItem.OpType = "I";
            parentsItem.OpRevNo = selOpMt.OpRevNo;
            parentsItem.Edition = selOpMt.Edition;
            selectedRowBom.push(parentsItem);
        } else {
            item.HasPattern = 'Y';
        }
    });

    if (!checkLinkedPatternByOpnSerial(_listTempLinkedPattern)) return;

    ShowModal('mdlBomPatternLinking');
    //reload bom and pattern linking modal
    ReloadJqGridLocal('tbBomPatternLinking', selectedRowBom);
}

const reloadDataGridviewTbLinkedItemAfterLinkingOpnDt = () => {
    let opSerial = getOpSerial();
    let listLinkedPattern = getSubGridJqGrid('#tbBomPatternLinking');
    let listLinkedItem = GetAllRowsDataJqGrid('#tbBomPatternLinking');
    
    //get selected opname detail
    //const selectedOpNameDt = GetSelectedOneRowData('#tbOpNameDetail');
    const selectedOpNameDt = _listTempOpNameDt;
    //get op name detail serial
    const opnSerial = selectedOpNameDt.OpnSerial;
    //update opserial for item and pattern linking
    listLinkedItem.forEach(item => { item.OpSerial = opSerial; item.OpnSerial = opnSerial });
    listLinkedPattern.forEach(pt => { pt.OpSerial = opSerial; pt.OpnSerial = opnSerial});
    //checking pattern quantity whether greater than remain quantity or not
    let isValidPieceQty = true;
    let pieceName = '';
    listLinkedPattern.forEach(pt => {
        if (pt.PieceQty > pt.PieceQtyRest) {
            isValidPieceQty = false;
            pieceName = pt.Piece;
            return false;
        }
    });
    if (!isValidPieceQty) {
        ShowMessage("Linking Bom & Pattern", `Please input a valid quantity for pattern: ${pieceName}`, MessageType.Error);
        return;
    }

    linkingBomAndPatternOpnDt(listLinkedItem, listLinkedPattern);

    HideModal('mdlBomPatternLinking');
    reloadOpNameDetailAndBomOpnDt(true);
}

const checkLinkedPatternByOpnSerial = (listPattern) => {
    let isLinked = true
    var config = {
        url: "/PlanManagement/CheckLinkedPatternByOpnSerial",
        postData: JSON.stringify({ listPattern: listPattern }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            isLinked = true;
        } else {
            console.log("Error Linking Bom & Pattern: " + response.Log);
            ShowMessage("Linking Bom & Pattern", response.Log, MessageType.Error);
            isLinked = false;
        }
    });

    return isLinked;
}

const saveBomAndPatternsLinkingOpnDt = () => {
    const opSerial = getOpSerial();
    const selOpmt = GetSelectedOneRowData('#gridOpsTable');

    var config = {
        url: "/PlanManagement/SaveLinkingBomPatternOpnDt",
        postData: JSON.stringify({
            styleCode: selOpmt.StyleCode,
            styleSize: selOpmt.StyleSize,
            styleColorSerial: selOpmt.StyleColorSerial,
            revNo: selOpmt.RevNo,
            opRevNo: selOpmt.OpRevNo,
            opSerial: opSerial,
            edition: selOpmt.Edition
        }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            reloadOpNameDetailAndBomOpnDt(false);
            ShowMessage("Save Bom & Pattern", response.Result, MessageType.Success);
        } else {
            console.log("Error save BOM & Patterns: " + response.Log);
            ShowMessage("Save Bom & Pattern", response.Log, MessageType.Error);
        }
    });
}
//#endregion

//#region functions
const linkingBomAndPatternOpnDt = (listLinkedItem, listLinkedPattern) => {
    var config = {
        url: "/PlanManagement/LinkingBomAndPatternOpnDt",
        postData: JSON.stringify({ listLinkedItem: listLinkedItem, listLinkedPattern: listLinkedPattern }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            ShowMessage("Linking Bom & Pattern", response.Result, MessageType.Success);
        } else {
            console.log("Error save BOM & Patterns: " + response.Log);
            ShowMessage("Linking Bom & Pattern", response.Log, MessageType.Error);
        }
    });
}

const reloadOpNameDetailAndBomOpnDt = (isLinking) => {
    _listExpandedRowIdOpNameDt = getExpandedRowIdsManySubGridJqGrid('#tbOpNameDetail');

    //get list current expanded row
    //_listExpandedRowIdOpNameDt = getExpandedRowIdsJqGrid('#tbOpNameDetail');
    _listExpandedRowIdTbBom = getExpandedRowIdsJqGrid('#tbBom');

    _listExpandedRowIdSubOpNameDt = getExpandedRowIdsSubGridJqGrid('#tbOpNameDetail', _listExpandedRowIdOpNameDt);

    let opSerial = getOpSerial();
    let selOpmt = GetSelectedOneRowData('#gridOpsTable');
    //reload linked bom
    reloadGridTbOpNameDetail(selOpmt.StyleCode, selOpmt.StyleSize, selOpmt.StyleColorSerial, selOpmt.RevNo, selOpmt.OpRevNo, opSerial, selOpmt.Edition, localStorage.getItem(LanguageId), isLinking);
    //reload grid bom
    reloadGridBom(selOpmt.StyleCode, selOpmt.StyleSize, selOpmt.StyleColorSerial, selOpmt.RevNo, selOpmt.OpRevNo, selOpmt.Edition, isLinking);
}

const deleteBomPatternLinkingOpnDt = (rowData, isItem = true) => {
    var config = {
        url: "/PlanManagement/DeleteBomPatternLinkingOpnDt",
        postData: JSON.stringify({ prot: rowData, isItem: isItem }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            reloadOpNameDetailAndBomOpnDt(true);
            ShowMessage("Delete Bom & Pattern Linking", response.Result, MessageType.Success);
        } else {
            console.log("Delete Bom & Pattern Linking: " + response.Log);
            ShowMessage("Delete Bom & Pattern Linking", response.Log, MessageType.Error);
        }
    });
}
//#endregion

//#region drag and drop
function CheckMuiltipalSelected(taget, isImage) {
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
    $("#tbBom").find('tr[role="row"]').find("input[type=checkbox]:checked").each(function () {
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

const getSelectedBomAndPatternForLinking = () => {
    //clear temporary data
    _listTempLinkedPattern = [];
    _listTempLinkedBom = [];
    _listTempOpNameDt = [];
    //get selected row in sub gridview
    _listTempLinkedPattern = getSelectedRowsSubGrid('#tbBom');
    //get selected Bom row in gridview
    let selectedRowBom = GetSelectedMultipleRowsData('#tbBom');

    if (selectedRowBom !== null) _listTempLinkedBom = selectedRowBom;

    //if linked bom and pattern is null then return false
    if ((_listTempLinkedPattern.length === 0 && selectedRowBom === null)) return false;

    return true;
}

const isSelectedMultipleBomAndPatterns = () => {
    if (_listTempLinkedBom.length > 1 || _listTempLinkedPattern.length > 1) return true;

    if (_listTempLinkedBom.length === 1 && _listTempLinkedPattern.length === 1) return true;

    return false;
}

const updateBomAndPatternForLinking = (opNameDtDropRowData) => {
    //assign dropped opname detail row data to temporary variable
    _listTempOpNameDt = opNameDtDropRowData;
    //get edition from selected op master
    const selOpMt = GetSelectedOneRowData('#gridOpsTable');
    let opSerial = getOpSerial();
    //get op name detail serial
    const opnSerial = opNameDtDropRowData.OpnSerial;
    //get all rows of grid bom
    const gridData = jQuery('#tbBom').jqGrid("getRowData");
    //update has pattern to N, and set pattern serial is 000
    _listTempLinkedBom.forEach((bom) => {
        bom.HasPattern = 'N';
        bom.PatternSerial = '000';
        bom.OpType = "I";
        bom.OpRevNo = selOpMt.OpRevNo;
        bom.Edition = selOpMt.Edition;
        bom.OpSerial = opSerial;
        bom.OpnSerial = opnSerial;
    });

    //get item of linked patterns
    _listTempLinkedPattern.forEach((pt) => {
        //add total piece qty
        pt.TotalPieceQty = pt.PieceQty;
        pt.OpRevNo = selOpMt.OpRevNo;
        pt.Edition = selOpMt.Edition;
        pt.OpType = "I";
        pt.OpSerial = opSerial;
        pt.OpnSerial = opnSerial;
        //find patterns of pattern
        let item = _listTempLinkedBom.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
        //if pattern doesn't have parents then get it from BOM
        if (item === null || typeof (item) === "undefined") {
            let parentsItem = gridData.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
            parentsItem.HasPattern = 'Y';
            parentsItem.OpType = "I";
            parentsItem.OpRevNo = selOpMt.OpRevNo;
            parentsItem.Edition = selOpMt.Edition;
            _listTempLinkedBom.push(parentsItem);
        } else {
            item.HasPattern = 'Y';
        }
    });    
}

function DragRow() {
    $(".Itemrow-draggable").draggable({
        start: function () {
            $(this).css("z-index", 9999999);
        },
        drag: function () {
            return true;
        },
        revert: "invalid",
        cursor: "pointer",
        appendTo: "body",
        helper: function (event/*, ui*/) {
            //if cannot get selected Bom and pattern for linking then return
            if (!getSelectedBomAndPatternForLinking()) return $("<div></div>");

            let html;

            //if (!CheckMuiltipalSelected(event.delegateTarget)) {
            if (!isSelectedMultipleBomAndPatterns()) {
                const itemCode = event.delegateTarget.cells[4].textContent;
                const itemName = event.delegateTarget.cells[5].textContent;
                html = "<div id='dragItemID' style='z-index:9999999;'><table class='table table-bordered'><tr style='background-color:#c7d3a9'><td>Item Code</td><td>" + itemCode + "</td><td>Item Name</td><td>" + itemName + "</td></tr>";
            }
            else {
                html = htmlMuilty;
            }
            return $(html);
        },
        cursorAt: { left: 10, top: 10 }
    });
}

function DraggChil() {
    $(".Child-draggable").draggable({
        drag: function () {
            return _isDragable;
        },
        revert: "invalid",
        cursor: "pointer",
        appendTo: "body",
        helper: function (event/*, ui*/) {
            //if cannot get selected Bom and pattern for linking then return
            if (!getSelectedBomAndPatternForLinking()) return $("<div></div>");;

            //$(this).find("input:checkbox").prop("checked", true);
            //if (!CheckMuiltipalSelected(event.delegateTarget)) {
            if (!isSelectedMultipleBomAndPatterns()) {
                var src = $(event.delegateTarget.cells[2].innerHTML).attr("src");
                html = "<image id='dragItemID' style='z-index:9999999' src='" + src + "'/>";
            }
            else {
                html = htmlMuilty;
            }
            return $(html);
        },
        start: function (event, ui) {
            $(this).css("z-index", 9999999);
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
            //var rowidBom = ui.draggable.attr("id");
            //text = $("#tbBom").jqGrid("getCell", rowidBom, "ItemName");

            if (!checkLinkedPatternByOpnSerial(_listTempLinkedPattern)) return false;

            //get dropped row id and row data
            const droppedRowId = $(this).attr("id");
            var droppedRowData = $('#tbOpNameDetail').jqGrid("getRowData", droppedRowId);
            showModalLinkingBomAndPatterns(droppedRowData)
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
            if (!_isDragable) {
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
                    text = $("#tbBom").jqGrid("getCell", rowidBom, "ItemName");
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

const showModalLinkingBomAndPatterns = (droppedRowData) => {
    //console.log('showModalLinkingBomAndPatterns');
    //console.log(_listTempLinkedPattern);
    //console.log(_listTempLinkedBom);
    //console.log(droppedRowData);

    updateBomAndPatternForLinking(droppedRowData)

    ShowModal('mdlBomPatternLinking');
    //reload bom and pattern linking modal
    ReloadJqGridLocal('tbBomPatternLinking', _listTempLinkedBom);
}

function ShowBoxDrag(target, ui, who) {

    console.log('drop success');
    console.log(_listTempLinkedPattern);
    console.log(_listTempLinkedBom);

}
//#endregion