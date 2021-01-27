const initialDataAuditToolPage = () => {
    Selection2('drpTeamGroup');
    Selection2('drpTeam');
    getBuyerList();
    setDateRangePicker4('#txtDateRange');

    //event selection dropdown list on page
    eventSelectionDropdownlist();

    bindDataToGridStyle(null, null, null, null, 'uni0037');
    bindDataToJqGridBom(null, null, null, null);
    bindDataToJqGridOpPlan(null, null, null, null, null);
    bindDataToJqGridOpPlanDetail(null, null, null, null, null, null, null);
    bindDataToJqGridMbom(null, null, null, null);
}

//#region get data master
const getBuyerList = () => {
    getBuyers($('#drpTeam').val(), response => FillDataToDropDownlist("drpBuyer", response, "SubCode", "CodeName"));
}
//#endregion

//#region functions
const eventSelectionStyleRow = (rowData) => {
    const styleCode = rowData.StyleCode;
    const styleSize = rowData.StyleSize;
    const styleColorSerial = rowData.StyleColorSerial;
    const revNo = rowData.RevNo;
    reloadGridBom(styleCode, styleSize, styleColorSerial, revNo);
    reloadGridOpPlan(styleCode, styleSize, styleColorSerial, revNo, '');
    reloadGridMbom(styleCode, styleSize, styleColorSerial, revNo);
}
//#endregion

//#region bind data on gridview

function bindDataToGridStyle(buyer, startDate, endDate, aoNumber, styleInfo) {
    $('#tbStyle').jqGrid({
        pager: "#tbStylePager",
        sortname: "STYLECODE",
        sortorder: "DESC",
        page: 1,
        rowNum: 40,
        rowList: [40, 60, 80, 20],
        scroll: false,
        viewrecords: true,
        scrollrows: true,
        shrinkToFit: false,
        width: null,
        gridview: true,
        height: 180,
        url: "/DataAuditTool/SearchStyle",
        caption: "Style",
        datatype: "json",
        postData: {
            //string buyer, string startDate, string endDate, string aoNumber, string styleInfo
            buyer: buyer, startDate: startDate, endDate: endDate, aoNumber: aoNumber, styleInfo: styleInfo
        },
        colModel: [
            { name: "StyleCode", index: "StyleCode", label: arrColname.STYLECODE, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 100 },
            { name: "StyleName", index: "StyleName", label: arrColname.STYLENAME, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "BuyerName", index: "BuyerName", label: arrColname.BuyerName, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "BuyerStyleCode", index: "BuyerStyleCode", label: arrColname.BUYERSTYLECODE, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 100 },
            { name: "BuyerStyleName", index: "BuyerStyleName", label: arrColname.BUYERSTYLENAME, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "StyleSize", index: "StyleSize", label: arrColname.STYLESIZE, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 80 },
            { name: "StyleColorSerial", index: "StyleColorSerial", label: arrColname.STYLECOLORSERIAL, hidden: true },
            { name: "StyleColorWays", index: "StyleColorWays", label: arrColname.STYLECOLORSERIAL, search: true, width: 120 },
            { name: "RevNo", index: "RevNo", label: arrColname.REVNO, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 80 },
            { name: "StaTus", index: "StaTus", label: arrColname.STATUS, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "RegistryDate", index: "RegistryDate", label: arrColname.REGISTRYDATE, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "Register", index: "Register", label: arrColname.REGISTER_NAME, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "AdConfirm", index: "AdConfirm", label: arrColname.AD_CONFIRM, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 140 },
            { name: "AdDevSale", index: "AdDevSale", label: arrColname.AD_DEV_SALES, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "Have", index: "Have", hidden: true },
            { name: "AdDevSale", index: "AdDevSale", label: arrColname.AD_DEV_SALES, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'StyleGroup', index: 'StyleGroup', hidden: true },
            { name: 'SubGroup', index: 'SubGroup', hidden: true },
            { name: 'SubSubGroup', index: 'SubSubGroup', hidden: true },
            { name: 'Buyer', index: 'Buyer', hidden: true }
        ],
        onSelectRow: function (rowid) {
            const rowdata = $('#tbStyle').jqGrid("getRowData", rowid);
            eventSelectionStyleRow(rowdata);
        },
        loadComplete: function () {
            updatePagerIcons();
        },
        onPaging: function (pgButton) {
            //if (pgButton === "records") {
            //    SetPaging(myJqgrid, tableNavName);
            //}
        },
        ajaxGridOptions: { async: true } 
    }).jqGrid("navGrid", "#tbStylePager", {
        cloneToTop: true,
        edit: false,
        add: false,
        del: false,
        search: false,
        searchicon: "ace-icon fa fa-search orange",
        searchtext: "",
        refresh: true, refreshicon: "ace-icon fa fa-refresh green", refreshtext: 'Refresh'
    });
    //$("#" + tableNavName).find("option[value=20]").text(arrButtonAction.all);
    //merge header 2 column button Save Delete
    //SearchFilter(myJqgrid);
}

const bindDataToJqGridBom = (styleCode, styleSize, styleColorSerial, revNo) => {
    jQuery("#tbBom").jqGrid({
        url: '/DataAuditTool/GetBomt',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo
        },
        datatype: "json",
        height: 340,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        //gridview: true,
        //multiselect: true,
        caption: "BOM & Patterns",
        colModel: [
            { name: 'MainItemCode', index: 'MainItemCode', label: "Main Item", width: 150, classes: 'pointer' },
            { name: 'MainItemName', index: 'MainItemName', label: "Main Item Name", width: 150, classes: 'pointer', hidden: true },
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 250, classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'ItemColorWays', index: 'ItemColorWays', label: "Item Color", width: 150, classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", width: 100, align: 'center', classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Cons.Unit", width: 70, align: 'center', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'Qty', index: 'Qty', label: "Qty", width: 50, align: 'center', classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'RegistryDate', index: 'RegistryDate', label: "Registry Date", width: 100, align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" }, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }
        ],
        afterInsertRow: function (rowid, rowdata) {
            $("#" + rowid, "#tbBom").addClass("Itemrow-draggable");
        },
        gridComplete: function () {
            let ids = jQuery("#tbBom").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbBom").jqGrid("getRowData", i);
                if (rowdata.HasPattern !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbBom_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbBom_subgrid]").unbind('click');
                }
            }
        },
        onSelectRow: function (rowid) {
            const rowdata = $('#tbBom').jqGrid("getRowData", rowid);
            reloadGridMbom(rowdata.StyleCode, rowdata.StyleSize, rowdata.StyleColorSerial, rowdata.RevNo);
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
        subGridRowExpanded: subGridviewPattern
    });
}

const subGridviewPattern = (subgridDivId, rowId) => {
    var rowData = $('#tbBom').jqGrid('getRowData', rowId);
    var subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/DataAuditTool/GetPatterns',
        postData: {
            styleCode: rowData.StyleCode,
            styleSize: rowData.StyleSize,
            styleColorSerial: rowData.StyleColorSerial,
            revNo: rowData.RevNo,
            itemCode: rowData.ItemCode,
            itemColorSerial: rowData.ItemColorSerial,
            mainItemCode: rowData.MainItemCode,
            mainItemColorSerial: rowData.MainItemColorSerial
        },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        //gridview: true,
        //multiselect: true,
        colModel: [
            {
                label: " ",
                name: "Url",
                index: "Url",
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
            { name: "PieceQtyRest", index: "PieceQtyRest", label: "Remain Qty", width: 100 },
            { name: "UnitConsumption", index: "UnitConsumption", label: "Unit.Cons", width: 80 },
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
            { name: 'RevNo', index: 'RevNo', hidden: true }
        ]
    });
}

const bindDataToJqGridOpPlan = (styleCode, styleSize, styleColor, revNo, edition) => {
    jQuery('#tbOpPlan').jqGrid({
        url: '/OPS/GetOpMaster',
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColor: styleColor, revNo: revNo, edition: edition
        },
        datatype: "json",
        width: null,
        height: 300,
        shrinkToFit: false,
        scroll: false,
        deepempty: true,
        ignoreCase: true,
        viewrecords: true,
        rowNum: 10,
        rowList: [10, 20, 30, 40],
        pager: '#tbOpPlanPager',
        gridview: true,
        caption: "Operation Plan",
        colModel: [
            { name: 'Edition2', index: 'Edition2', width: 110, label: arrOpsColname.EDITION, align: 'center', classes: 'pointer', sortable: false },
            { name: 'StyleCode', index: 'StyleCode', width: 90, label: arrOpsColname.STYLECODE, classes: 'pointer' },
            { name: 'StyleColorWays', index: 'StyleColorWays', width: 200, label: arrOpsColname.STYLECOLORSERIAL, classes: 'pointer' },
            { name: 'BuyerStyleCode', index: 'BuyerStyleCode', width: 120, label: arrOpsColname.BUYERSTYLECODE, classes: 'pointer' },
            { name: 'BuyerStyleName', index: 'BuyerStyleName', width: 250, label: arrOpsColname.BUYERSTYLENAME, classes: 'pointer' },
            { name: 'StyleSize', index: 'StyleSize', width: 90, label: arrOpsColname.STYLESIZE, classes: 'pointer' },
            { name: 'RevNo', index: 'RevNo', width: 90, label: arrOpsColname.REVNO, align: 'center', classes: 'pointer' },
            { name: 'OpRevNo', index: 'OpRevNo', width: 90, label: arrOpsColname.OPREVNO, align: 'center', classes: 'pointer' },
            { name: 'OpTime', index: 'OpTime', width: 90, label: arrOpsColname.OPTIME, align: 'center', classes: 'pointer' },
            { name: 'TotalOpTime', index: 'TotalOpTime', width: 90, label: "Total Time", align: 'center', classes: 'pointer' },
            { name: 'OpPrice', index: 'OpPrice', width: 90, label: arrOpsColname.OPPRICE, align: 'center', classes: 'pointer', hidden: true },
            { name: 'MachineCount', index: 'MachineCount', width: 115, label: arrOpsColname.MACHINECOUNT, align: 'center', classes: 'pointer' },
            { width: 60, label: arrOpsColname.CONFIRMCHK, align: 'center', classes: 'pointer', formatter: showIconConfirmed },
            { name: 'OpCount', index: 'OpCount', width: 90, label: arrOpsColname.OPCOUNT, align: 'center', classes: 'pointer' },
            { name: 'ManCount', index: 'ManCount', width: 90, label: arrOpsColname.MANCOUNT, align: 'center', classes: 'pointer' },
            { name: 'Factory', index: 'Factory', width: 90, label: arrOpsColname.FACTORY, align: 'center', classes: 'pointer' },
            { name: 'LastUpdateTime', index: 'LastUpdateTime', width: 150, label: arrOpsColname.LASTUPDATEDATE, align: 'left', classes: 'pointer', formatter: convertDateToString },
            { name: 'Remarks', index: 'Remarks', width: 250, label: arrOpsColname.REMARKS, align: 'left', classes: 'pointer' },
            { name: 'MxPackage', index: 'MxPackage', width: 250, label: arrOpsColname.MXPACKAGE, align: 'left', classes: 'pointer' }, //ADD) SON - 1/Jul/2019
            { name: 'Edition', index: 'Edition', width: 90, hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'Language', index: 'Language', hidden: true },
            { name: 'ProcessWidth', index: 'ProcessWidth', hidden: true },
            { name: 'ProcessHeight', index: 'ProcessHeight', hidden: true },
            { name: 'GroupMode', index: 'GroupMode', hidden: true },
            { name: 'CanvasHeight', index: 'CanvasHeight', hidden: true },
            { name: 'Buyer', index: 'Buyer', hidden: true },
            { name: 'LayoutFontSize', index: 'LayoutFontSize', hidden: true },
            { name: 'StyleGroup', index: 'StyleGroup', hidden: true },
            { name: 'SubGroup', index: 'SubGroup', hidden: true },
            { name: 'SubSubGroup', index: 'SubSubGroup', hidden: true },
            { name: 'ConfirmChk', index: 'ConfirmChk', hidden: true },
            { name: 'RegisterId', index: 'RegisterId', hidden: true },
            { name: 'ConfirmedId', index: 'ConfirmedId', hidden: true },
        ],
        loadComplete: function () {
            setTimeout(function () {
                updatePagerIcons();
            }, 0);
        },
        onPaging: function (pgButton) {
            if (pgButton === "records") {
                SetPaging($('#tbOpPlan'), 'tbOpPlanPager');
            }
        },
        onSelectRow: function (rowid) {
            const rowData = $('#tbOpPlan').jqGrid("getRowData", rowid);
            reloadGridOpPlanDetail(rowData.StyleCode, rowData.StyleSize, rowData.StyleColorSerial, rowData.RevNo, rowData.OpRevNo, rowData.Edition, 'en');
        },
        ajaxGridOptions: { async: false },
        loadonce: true,
    });

    //Add filter edition on header of gridview
    jQuery('#tbOpPlan').jqGrid('setLabel', 'Edition2',
        "<select id= 'drpOpPlanEdition' >" +
        "<option value=''>All</option >" +
        "<option value='P'>PDM</option >" +
        "<option value='O'>OPS</option >" +
        "<option value='A'>AOMTOPS</option>" +
        "<option value='M'>MES</option>" +
        "</select> ");
    //navButtons
    jQuery('#tbOpPlan').jqGrid('navGrid', '#tbOpPlanPager', {
        //navbar options
        view: true,
        viewicon: 'ace-icon fa fa-search-plus grey',
        edit: false,
        del: false,
        search: true,
        searchicon: 'ace-icon fa fa-search orange',
        refresh: true,
        refreshicon: 'ace-icon fa fa-refresh green'
    });

    $("#pg_tbOpPlanPager option[value=40]").text(arrButtonAction.all);

    function showIconConfirmed(cellValue, options, rowObject) {
        if (rowObject.ConfirmChk === ConfirmCheck) {
            return "<label><i class='fa fa-lock'></i></label>";
        }
        return "";
    }

    function convertDateToString(cellValue, options, rowObject) {
        if (!$.isEmptyObject(rowObject.LastUpdateTime)) {
            var newDate = eval(("new " + rowObject.LastUpdateTime).replace(/\//g, ""))
            return newDate;
        }
        return "";
    }
}

const bindDataToJqGridOpPlanDetail = (styleCode, styleSize, styleColor, revNo, opRevNo, edition, lanId) => {

    jQuery('#tbOpPlanDt').jqGrid({
        url: '/OPS/GetOpDetail',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColor: styleColor,
            revNo: revNo,
            opRevNo: opRevNo,
            edition: edition,
            languageId: lanId
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
        caption: "Plan Detail",
        colModel: [
            { name: 'HotSpot', index: 'HotSpot', label: " ", width: 25, classes: 'pointer', formatter: markHotSpot },
            { name: 'OpGroupName', index: 'OpGroupName', label: arrColNameOpsDetail.OPGROUPNAME, hidden: true, classes: 'pointer' },
            { name: 'ModuleName', index: 'ModuleName', label: arrColNameOpsDetail.MODULENAME, hidden: false, classes: 'pointer' },
            { name: 'OpNum', index: 'OpNum', width: 70, label: arrColNameOpsDetail.OPNUM, align: 'left', classes: 'pointer' },
            { name: 'OpName', index: 'OpName', width: 250, label: arrColNameOpsDetail.OPNAME, classes: 'pointer' },
            { name: 'OpNameLan', index: 'OpNameLan', width: 250, label: arrColNameOpsDetail.OPNAME, hidden: true, classes: 'pointer' },
            { name: 'OpTime', index: 'OpTime', width: 130, label: arrColNameOpsDetail.OPTIME, align: 'center', classes: 'pointer' },
            { name: 'OpPrice', index: 'OpPrice', width: 80, label: arrColNameOpsDetail.OPPRICE, align: 'center', classes: 'pointer', hidden: true }, //Ha add
            { name: 'Factory', index: 'Factory', width: 90, label: arrColNameOpsDetail.FACTORY, align: 'center', hidden: true, classes: 'pointer' },
            { name: 'FactoryName', index: 'FactoryName', width: 120, label: arrColNameOpsDetail.FACTORY, align: 'left', classes: 'pointer' },

            { name: 'ManCount', index: 'ManCount', width: 70, label: arrColNameOpsDetail.MANCOUNT, align: 'center', classes: 'pointer' },
            { name: 'MachineName', index: 'MachineName', width: 120, label: arrColNameOpsDetail.MACHINENAME, align: 'left', classes: 'pointer' },
            { name: 'MachineCount', index: 'MachineCount', width: 80, label: arrColNameOpsDetail.MACHINECOUNT, align: 'center', classes: 'pointer' },
            { name: 'OfferOpPrice', index: 'OfferOpPrice', width: 100, label: arrColNameOpsDetail.OFFEROPPRICE, align: 'center', classes: 'pointer', hidden: true }, //Ha add
            { name: 'MaxTime', index: 'MaxTime', width: 75, label: arrColNameOpsDetail.MAXTIME, align: 'center', classes: 'pointer' },
            { name: 'StitchCount', index: 'StitchCount', hidden: true }, // HA ADD
            {
                name: 'OrgFileName', index: 'OrgFileName', width: 70, label: 'Jig Image', align: 'center', classes: 'pointer'
                , formatter: function (cellvalue, options) {
                    if (cellvalue)
                        return "<img style='width:60px;height:20px' src='" + cellvalue + "' onclick=ShowImageDetail('" + options.rowId + "'); />";
                    return "";
                }
            },
            { name: 'Remarks', index: 'Remark', width: 250, label: "Remarks", classes: 'pointer' },
            { name: 'VideoFile', index: 'VideoFile', width: 150, label: arrColNameOpsDetail.VIDEO, align: 'center', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', width: 100, label: arrColNameOpsDetail.STYLECODE, hidden: true },
            { name: 'StyleSize', index: 'StyleSize', width: 100, label: arrColNameOpsDetail.STYLESIZE, hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSeiral', width: 100, label: arrColNameOpsDetail.STYLECOLORSERIAL, hidden: true },
            { name: 'RevNo', index: 'RevNo', width: 100, label: arrColNameOpsDetail.REVNO, hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', width: 100, label: arrColNameOpsDetail.OPREVNO, hidden: true },
            { name: 'OpSerial', index: 'OpSerial', width: 100, label: arrColNameOpsDetail.OPSERIAL, hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'HotSpot', index: 'HotSpot', hidden: true },
            { name: 'OpTimeBalancing', index: 'OpTimeBalancing', hidden: true },
            { name: 'ToolId', index: 'ToolId', hidden: true },
            { name: 'MachineType', index: 'MachineType', hidden: true },
            { name: 'OpGroup', index: 'OpGroup', hidden: true },
            { name: 'ModuleId', index: 'ModuleId', hidden: true },
            { name: 'ActionCode', index: 'ActionCode', hidden: true },
            { name: 'VideoOpLink', index: 'VideoOpLink', hidden: true },
            { name: 'ImageLink', index: 'ImageLink', hidden: true },
            { name: 'HasFile', index: 'HasFile', hidden: true },
            { name: 'HasManyFiles', index: 'HasManyFiles', hidden: true },
            { name: 'FileNameOpfl', index: 'FileNameOpfl', hidden: true },
            { name: 'NoneStd', index: 'NoneStd', hidden: true } //ADD) SON - 21/Jun/2019 - remark process is standard name
        ],
        grouping: true,
        groupingView: {
            groupField: ['ModuleName'],
            groupColumnShow: [false],
            groupText: ["Module Name: {0} - {1} Item(s)"],
            groupCollapse: false,
            plusicon: "ace-icon fa fa-plus",
            minusicon: "ace-icon fa fa-minus"
        },
        ajaxGridOptions: { async: false }
    });

    function markHotSpot(cellvalue, options, rowObject) {
        if (cellvalue === "1") {
            return "<i class='fa fa-flag' style='color: red'></i> ";
        }
        return "";
    }
}

const bindDataToJqGridMbom = (styleCode, styleSize, styleColorSerial, revNo) => {
    jQuery("#tbModule").jqGrid({
        url: '/DataAuditTool/GetModules',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo
        },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1,
        rownumbers: false,
        caption: "MBOM",
        colModel: [
            { name: 'ModuleId', index: 'ModuleId', label: "Part Id", width: 150 },
            { name: 'ModuleName', index: 'ModuleName', label: 'Part Name', width: 150 },
            { name: 'FinalAssembly', index: 'FinalAssembly', label: 'Final Assembly', width: 100, align: 'center'},
            { name: 'ItemCount', index: 'ItemCount', width: 100, label: "No Of Items", align: 'center' },
            { name: 'RegistryDate', index: 'RegistryDate', width: 170, label: "RegistryDate" },
            { name: 'HasItem', index: 'HasPattern', hidden: true }
        ],
        gridComplete: function () {
            let ids = jQuery("#tbModule").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbModule").jqGrid("getRowData", i);
                if (rowdata.HasItem !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbModule_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbModule_subgrid]").unbind('click');
                }
            }
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
        subGridRowExpanded: subGridModuleMbom,
        ajaxGridOptions: { async: false }
    });
}

const subGridModuleMbom = (subgridDivId, rowId) => {
    //get current selected linked item row data
    let rowData = $('#tbModule').jqGrid('getRowData', rowId);
    const rowDataStyle = GetSelectedOneRowData('#tbStyle');
    let subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/DataAuditTool/GetMBom',
        postData: {
            styleCode: rowDataStyle.StyleCode,
            styleSize: rowDataStyle.StyleSize,
            styleColorSerial: rowDataStyle.StyleColorSerial,
            revNo: rowDataStyle.RevNo,
            moduleId: rowData.ModuleId
        },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1,
        rownumbers: false,
        gridview: true,
        colModel: [
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer' },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 250, classes: 'pointer' },
            { name: 'ItemColorways', index: 'ItemColorways', label: "Item Color", width: 150, classes: 'pointer' },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", width: 100, align: 'center', classes: 'pointer' },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Cons.Unit", width: 70, align: 'center' },
            { name: 'MainItemCode', index: 'MainItemCode', hidden: true },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'ModuleItemCode', index: 'ModuleItemCode', hidden: true },
            { name: 'PatternSerial', index: 'PatternSerial', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }
        ],
        gridComplete: function () {
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
        subGridRowExpanded: subGridviewMbomPatterns
    });
}

const subGridviewMbomPatterns = (subgridDivId, rowId) => {
    const parentTableId = subgridDivId.substring(0, subgridDivId.lastIndexOf("_"));
    //get current selected linked item row data
    let rowData = $(`#${parentTableId}`).jqGrid('getRowData', rowId);
    const { StyleCode, StyleSize, StyleColorSerial, RevNo, ModuleItemCode, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial } = rowData;

    //clear delete element
    rowData.DeleteItemEle = '';
    let subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/DataAuditTool/GetMbomPatterns',
        postData: {
            styleCode: StyleCode,
            styleSize: StyleSize,
            styleColorSerial: StyleColorSerial,
            revNo: RevNo,
            moduleId: ModuleItemCode,
            itemCode: ItemCode,
            itemColorSerial: ItemColorSerial,
            mainItemCode: MainItemCode,
            mainItemColorSerial: MainItemColorSerial
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
            {
                label: " ",
                name: "IMAGELINK",
                index: "IMAGELINK",
                align: "center",
                width: 120,
                formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return `<img id='${id}' class='imgpattern' onclick = showPatternImage('${cellvalue}'); src='${cellvalue}' onerror='imgError(this);'/>`;
                    return "";
                }
            },
            { name: "PIECE", index: "PIECE", label: "Piece", width: 300 },
            { name: "WIDTH", index: "WIDTH", label: "Width", align: "center", width: 50 },
            { name: "HEIGHT", index: "HEIGHT", label: "Height", align: "center", width: 50 },
            { name: "ENDWISE", index: "ENDWISE", label: "End Wise", align: "center", width: 80 },
            { name: "PIECEQTY", index: "PIECEQTY", label: "Qty", width: 50 },
            { name: "CONSUMPUNIT", index: "CONSUMPUNIT", label: "Cons.Unit", width: 80 },
            { name: "PATTERNSERIAL", index: "PATTERNSERIAL", label: "Serial", width: 80 },
            { name: "PIECEUNIQUE", index: "PIECEUNIQUE", label: "Unique", align: "center", width: 80 },
            { name: "ITEMCODE", index: "ITEMCODE", hidden: true },
            { name: "ITEMCOLORSERIAL", index: "ITEMCOLORSERIAL", hidden: true },
            { name: "MAINITEMCODE", index: "MAINITEMCODE", hidden: true },
            { name: "MAINITEMCOLORSERIAL", index: "MAINITEMCOLORSERIAL", hidden: true },
            { name: 'STYLECODE', index: 'STYLECODE', hidden: true },
            { name: 'STYLESIZE', index: 'STYLESIZE', hidden: true },
            { name: 'STYLECOLORSERIAL', index: 'STYLECOLORSERIAL', hidden: true },
            { name: 'REVNO', index: 'REVNO', hidden: true }            
        ]
    });
}
//#endregion

//#region reload gridview
const reloadGridBom = (styleCode, styleSize, styleColorSerial, revNo) => {
    const postData = { styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo };
    ReloadJqGrid2LoCal('tbBom', postData);
}

const reloadGridOpPlan = (styleCode, styleSize, styleColorSerial, revNo, edition) => {
    const postData = { styleCode: styleCode, styleSize: styleSize, styleColor: styleColorSerial, revNo: revNo, edition: edition };
    ReloadJqGrid2LoCal('tbOpPlan', postData);
}

const reloadGridOpPlanDetail = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId) => {
    const postData = { styleCode: styleCode, styleSize: styleSize, styleColor: styleColorSerial, revNo: revNo, opRevNo: opRevNo, edition: edition, languageId: languageId };
    ReloadJqGrid2LoCal('tbOpPlanDt', postData);
}

const reloadGridMbom = (styleCode, styleSize, styleColorSerial, revNo) => {
    const postData = { styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo };
    ReloadJqGrid2LoCal('tbModule', postData);
}
//#endregion