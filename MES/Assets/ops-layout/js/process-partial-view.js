//#region Ready
(() => {
    //Get user role, include role for operation management and factory role
    window.UserRoleOpm = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionPdm));
    window.UserRoleFom = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionAom));
    window.UserRoleMes = GetUserRoleInfo(SystemIdMes, GetMenuIdByEdition(editionMes));

    ClickButtonOkInputOpTime();
    LoadOpNameByGetTemplate();
    EventClickPaintingButton();
    $("#btnRemoveVideo").click(() => {
        RemovePreviewVideo();
    });
})();
//#endregion

//#region Variables
const OpNameTable = "tbAllOpName", OpNameTableId = `#${OpNameTable}`, BtnSaveOpdt = "btnSaveProcess",
    BtnUpdateOpdt = "btnUpdateProcess";

var StatusUpdateProcess = 1, ProcessNameTemp = "mdlProcessNameTemplate", LayoutPage, DisplayColor, LayoutTopY, LayoutLeftX,
    LayoutGroupMode, IsLayoutEvent, ModuleTypeConst = "ModuleType", OpGroupConst = "OpGroup", MachineTypeConst = "MachineType",
    TableProcessCloneId = "#tblProcessClone", TableProcessCloneName = "tblProcessClone",
    TableProcessNameTemplateId = "#tbProcessNameTemplate", TableProcessNameTemplateName = "tbProcessNameTemplate",
    TableProcessNameDetail = "#tbProcessTemplate", ProcessTemplateDetail = "mdlProcessTemplateDetail",
    ProcessTemplate = "mdlProccesTemplate", CompareProcessName = false, ListOldProcessName = [], SelectedObjOpnt,
    ListObjOpnt, IOTTYPE = { Assembly: "SA", FinalAssembly: "FA", Qa: "QA" },
    IsUpdateProcess = false, IsInsertProcess = false, ListPaintingTime = null;
//#endregion

// #region Get data

//Get process name
function GetOpName2(languageId) {
    var config = ObjectConfigAjaxPost("/Ops/GetOpName", true, JSON.stringify({ languageId: languageId }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropdownOpName(respone);
    });
}

//Get tool category
function GetMasterCodeTool(isMachine, toolCategories) {
    const config = ObjectConfigAjaxPost("/Ops/GetCategorysMachineTool", true, JSON.stringify({ isMachine: isMachine }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropdownCategory(respone, "drpCategoryTool", "1");
        //const categories = [];
        $('#drpCategoryTool').multiselect('select', [], true);

        if (toolCategories) {
            $('#drpCategoryTool').multiselect('select', toolCategories, true);
        }
    });
}

//Get machine category
function GetMasterCodeMachine(isMachine, categories) {
    var config = ObjectConfigAjaxPost("/Ops/GetCategorysMachineTool", true, JSON.stringify({ isMachine: isMachine }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropdownCategory(respone, "drpCategoryMachine", "0");
        //Load default machine
        const arrCategory = ["CSW", "SEW"];
        $('#drpCategoryMachine').multiselect('select', arrCategory, true);
        if (categories) {
            $('#drpCategoryMachine').multiselect('select', categories, true);
        }
    });
}

//Get process group
function GetMasterCodeOpGroup(OpGroup) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: OpGroup }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpProcessGroup", respone, "SubCode", "CodeName");
    });
}

//Get job type
function GetMasterCodeOpType(OpType, selectedJobType) {
    const config = ObjectConfigAjaxPost("/Ops/GetMasterCode", false, JSON.stringify({ mCode: OpType }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpJobCode", respone, "SubCode", "CodeName");
        if (selectedJobType) {
            $("#drpJobCode").val(selectedJobType).trigger("change");
        }
    });
}

function GetActionCodeOpType(OpType, selectedActionCode) {
    const config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: OpType }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpActionCode", respone, "SubCode", "CodeName");
        if (selectedActionCode) {
            $("#drpActionCode").val(selectedActionCode).trigger("change");
        }
    });
}

function GetFactory(selectedFactory) {
    const config = ObjectConfigAjaxPost("/Ops/GetFactory", true);
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("selFactory", respone, "FactoryId", "FactoryName");
        if (selectedFactory) {
            $("#selFactory").val(selectedFactory).trigger("change");
        }
    });
}

//Get module
function GetModulesByStyleCode(styleCode, selectedModule) {
    const config = ObjectConfigAjaxPost("/Ops/GetModulesListByStyleCode", true, JSON.stringify({ styleCode: styleCode }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpModule", respone, "ModuleId", "ModuleName");

        if (selectedModule) {
            $("#drpModule").val(selectedModule).trigger("change");
        }
    });
}

function GetOpNameByCode(languageId, moduleId, actionCode) {
    var config = ObjectConfigAjaxPost("/Ops/GetOpNameByCode", true, JSON.stringify({ languageId: languageId, moduleId: moduleId, actionCode: actionGroup }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropdownOpName(respone);
    });
}

//Get list ops detail
function GetListOpToolLinking(opTool) {
    var lst;
    $.ajax({
        url: "/Ops/GetToolLinkingByCode",
        async: false,
        type: "POST",
        data: JSON.stringify({ opTool: opTool }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            lst = res;
        },
        error: function (jqXhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error,
                err.message);
        }
    });

    return lst;
}

function GetListProcessNameDetail(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, languageId, callBack) {
    var config = ObjectConfigAjaxPost("/Ops/GetListProcessNameDetail", true,
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
    //$.blockUI({ message: "<h3>Loading process name...</h3>" });
    AjaxPostCommon(config, function (respone) {
        callBack(respone);
        //$.unblockUI();
    });
}

//start Ha

//Get Template
function GetTempName(actionCode) {
    const config = ObjectConfigAjaxPost("/Ops/GetTempName", true, JSON.stringify({ actionCode: actionCode }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpTemplate", respone, "TempId", "TempName");
    });
}

//Get Main Level
function GetActionItemMainLevel() {
    var config = ObjectConfigAjaxPost("/Ops/GetItemMainLevel", true, JSON.stringify({}));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpMainLevel", respone, "MainLevel", "MainLevelName");
    });
}

function EventButtonClickTemplate() {
    $("#btnGetProNameTemp").click(function () {
        ShowModal(ProcessTemplate);
    });
}

function LoadOpNameByGetTemplate() {
    $("#drpActionCode").change(function () {
        const actioncode = $("#drpActionCode").val();
        GetTempName(actioncode);

        $("#drpTemplate").change(function () {
            var actioncode = $("#drpActionCode").val();
            var tempid = $("#drpTemplate").val();
            var language = CurrentOpmt.Language;
            var data = { opActionCode: actioncode, opTempId: tempid, opLanguage: language };

            ReloadJqGrid2LoCal("tbProcessTemplate", data);
        });
    });
}

function ButtonClickProcessDetail(rowid) {
    //Edit
    if (StatusUpdateProcess == 1) {
        ClearDataProcessDetailOpName();

        var listObjDetail = jQuery(TableInputOpNameId).getGridParam('data');
        var objDetail = jQuery(TableInputOpNameId).jqGrid('getRowData', rowid);

        SelectedObjOpnt = objDetail;

        ShowModal(ProcessTemplateDetail);

        $('#selectMachineTypeSubDetail').append('<option value=' + objDetail.MachineType + '>' + objDetail.MachineName + '</option>');
        $("#txtMachineCountSubDetail").val(objDetail.MachineCount);
        $("#txtRemarksSubDetail").val(objDetail.Remarks);
        $("#txtMaxTimeSubDetail").val(objDetail.MaxTime);
        $("#txtManCountSubDetail").val(objDetail.ManCount);
        $("#drpJobTypeSubDetail").val(objDetail.JobType).trigger('change');
        $('#selectToolIdSubDetail').append('<option value=' + objDetail.ToolId + '>' + objDetail.ToolName + '</option>');
        $('#drpActionCodeSubDetail').val(objDetail.ActionCode).trigger('change');
    }
    else { //Add new
        ListObjOpnt = jQuery(TableInputOpNameId).getGridParam('data');

        var obj = jQuery(TableInputOpNameId).jqGrid('getRowData', rowid);

        SelectedObjOpnt = obj;
        ShowModal(ProcessTemplateDetail);
        ClearDataProcessDetailOpName();
    }
}

function EventButtonOkSubProcessDetail() {
    $("#btnSaveProcessDetail").click(function () {
        //Edit
        if (StatusUpdateProcess == 1) {

            ListObjOpnt = jQuery(TableInputOpNameId).getGridParam('data');

            let obj = SelectedObjOpnt;

            obj.MachineType = $("#selectMachineTypeSubDetail").val();
            obj.MachineCount = $("#txtMachineCountSubDetail").val();
            obj.Remarks = $("#txtRemarksSubDetail").val();
            obj.MaxTime = $("#txtMaxTimeSubDetail").val();
            obj.ManCount = $("#txtManCountSubDetail").val();
            obj.JobType = $("#drpJobTypeSubDetail").val();
            obj.ToolId = $('#selectToolIdSubDetail').val();
            obj.ActionCode = $('#drpActionCodeSubDetail').val();

            HideModal(ProcessTemplateDetail);

            let gridData = ListObjOpnt;

            for (var i = 0; i < gridData.length; i++) {

                let objProcess = gridData[i];

                if (obj.OpNameId == objProcess.OpNameId && obj.StyleCode == objProcess.StyleCode && obj.StyleSize == objProcess.StyleSize
                    && obj.StyleColorSerial == objProcess.StyleColorSerial && obj.RevNo == objProcess.RevNo && obj.OpRevNo == objProcess.OpRevNo
                    && obj.Edition == objProcess.Edition && obj.OpSerial == objProcess.OpSerial) {

                    objProcess.MachineType = obj.MachineType;
                    objProcess.MachineCount = obj.MachineCount;
                    objProcess.Remarks = obj.Remarks;
                    objProcess.MaxTime = obj.MaxTime;
                    objProcess.ManCount = obj.ManCount;
                    objProcess.JobType = obj.JobType;
                    objProcess.ToolId = obj.ToolId;
                    objProcess.ActionCode = obj.ActionCode;

                    return objProcess;
                }
            }
            ReloadJqGridLocal(TableInputOpNameName, gridData);
        }

        else { //Add new
            var obj = SelectedObjOpnt;

            obj.MachineType = $("#selectMachineTypeSubDetail").val();
            obj.MachineCount = $("#txtMachineCountSubDetail").val();
            obj.Remarks = $("#txtRemarksSubDetail").val();
            obj.MaxTime = $("#txtMaxTimeSubDetail").val();
            obj.ManCount = $("#txtManCountSubDetail").val();
            obj.JobType = $("#drpJobTypeSubDetail").val();
            obj.ToolId = $('#selectToolIdSubDetail').val();
            obj.ActionCode = $('#drpActionCodeSubDetail').val();

            HideModal(ProcessTemplateDetail);

            let gridData = ListObjOpnt;

            for (let i = 0; i < gridData.length; i++) {

                var objProcess = gridData[i];

                if (obj.OpNameId == objProcess.OpNameId && obj.StyleCode == objProcess.StyleCode && obj.StyleSize == objProcess.StyleSize
                    && obj.StyleColorSerial == objProcess.StyleColorSerial && obj.RevNo == objProcess.RevNo && obj.OpRevNo == objProcess.OpRevNo
                    && obj.Edition == objProcess.Edition && obj.OpSerial == objProcess.OpSerial) {

                    objProcess.MachineType = obj.MachineType;
                    objProcess.MachineCount = obj.MachineCount;
                    objProcess.Remarks = obj.Remarks;
                    objProcess.MaxTime = obj.MaxTime;
                    objProcess.ManCount = obj.ManCount;
                    objProcess.JobType = obj.JobType;
                    objProcess.ToolId = obj.ToolId;
                    objProcess.ActionCode = obj.ActionCode;

                    return objProcess;
                    //break;
                }
            }
            ReloadJqGridLocal(TableInputOpNameName, gridData);
        }
    });
}

function ClearDataProcessDetailOpName() {

    $("#selectMachineTypeSubDetail").empty().trigger('change');
    $("#txtMachineCountSubDetail").val('');
    $("#txtRemarksSubDetail").val('');
    $("#txtMaxTimeSubDetail").val('');
    $("#txtManCountSubDetail").val('');
    $("#drpJobTypeSubDetail").val('').trigger('change');
    $('#selectToolIdSubDetail').empty().trigger('change');
    $('#drpActionCodeSubDetail').val('').trigger('change');

}
//end Ha

// #endregion

// #region Bind data to gridview

//start Ha
// Get data to Template table
function jqGridProcessTemplate(actioncode, tempid, languageId) {
    jQuery(TableProcessNameDetail).jqGrid({
        url: '/OPS/GetTemplate',
        datatype: "json",
        postData: {
            opActionCode: actioncode,
            opTempId: tempid,
            opLanguage: languageId
        },
        width: null,
        shrinkToFit: false,
        height: 250,
        colModel: [
            { name: 'OpNameId', index: 'OpNameId', hidden: true },
            { name: 'OpNameLan', index: 'OpNameLan', hidden: true },
            { name: 'OpName', index: 'OpName', label: "Template Name", width: 700, formatter: getTemplateName },
        ],
        rowList: [10, 20, 30],
        pager: '#pager4',
        sortname: 'OpNameLan',
        viewrecords: true,
        loadonce: true,
        multiselect: true,
        sortorder: "asc",
        caption: "Process Template",
        gridview: true,
        autowidth: false,
    });
    function getTemplateName(cellvalue, option, rowObject) {
        return rowObject.OpNameId + ' - ' + rowObject.OpNameLan;
    }
}
//end Ha

//Get data from excel and bind to table
function BindDataToJqGridInputOpTimeModal(opNameData) {
    jQuery(TableInputOpNameId).jqGrid({
        datatype: "local",
        height: 250,
        width: null,
        shrinkToFit: false,
        rowNum: 10000,
        colModel: [
            { name: 'OpnSerial', index: 'OpnSerial', width: 70, label: "Serial" },
            { name: 'OpName', index: 'OpName', width: 650, label: "Operation Name" },
            { name: 'OpNameId', index: 'OpNameId', hidden: true },
            {
                name: 'OpTime', index: 'OpTime', label: "Operation Time", width: 150,
                formatter: function (cellValue, option) {
                    return '<input type="text" id="txtOpTime_' + option.rowId + '" class="form-control" maxlength="3" ' +
                        'onkeypress = "return isNumber(event)" value= "' + cellValue + '" />';
                }
            },
            {
                name: '', index: 'Detail', width: 70, sortable: false, hidden: false,
                formatter: function (cellvalue, option, rowObject) {
                    return "<button type='button' class='btn btn-info btn-modal' style='height:30px;width:60px;' onclick='ButtonClickProcessDetail(" + option.rowId + ")' > Detail </button>";
                }
            },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSeiral', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'MachineType', index: 'MachineType', hidden: true },
            { name: 'MachineName', index: 'MachineName', hidden: true },
            { name: 'MachineCount', index: 'MachineCount', hidden: true },
            { name: 'Remarks', index: 'Remarks', hidden: true },
            { name: 'MaxTime', index: 'MaxTime', hidden: true },
            { name: 'ManCount', index: 'ManCount', hidden: true },
            { name: 'JobType', index: 'JobType', hidden: true },
            { name: 'ToolId', index: 'ToolId', hidden: true },
            { name: 'ToolName', index: 'ToolName', hidden: true },
            { name: 'ActionCode', index: 'ActionCode', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
        }
    });

    for (var i = 0; i <= opNameData.length; i++) {
        jQuery(TableInputOpNameId).jqGrid('addRowData', i + 1, opNameData[i]);
    }
}

//Bind data to Ops detail gridview
function BindDataToJqGridCloneProcess(styleCode, styleSize, styleColor, revNo, opRevNo, edition, lanId) {
    jQuery(TableProcessCloneId).jqGrid({
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
        height: 400,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: true,
        sortname: "ModuleName",
        sortorder: "asc",
        gridview: true,
        multiselect: true,
        caption: "List Of Processes",
        colModel: [
            { label: " ", width: 25, formatter: markHotSpot, sortable: false, search: false },
            { name: 'OpGroupName', index: 'OpGroupName', label: arrColNameOpsDetail.OPGROUPNAME, searchoptions: { sopt: ['cn', 'eq', 'ne'] }, clearSearch: true },
            { name: 'ModuleName', index: 'ModuleName', label: arrColNameOpsDetail.MODULENAME },
            { name: 'OpNum', index: 'OpNum', width: 70, label: arrColNameOpsDetail.OPNUM, align: 'center' },
            { name: 'OpName', index: 'OpName', width: 300, label: arrColNameOpsDetail.OPNAME },
            { name: 'OpNameLan', index: 'OpNameLan', width: 300, label: arrColNameOpsDetail.OPNAME, hidden: true },
            { name: 'OpTime', index: 'OpTime', width: 130, label: arrColNameOpsDetail.OPTIME, align: 'center', },
            { name: 'Factory', index: 'Factory', width: 90, label: arrColNameOpsDetail.FACTORY, align: 'center', hidden: true, },
            { name: 'MachineName', index: 'MachineName', width: 120, label: arrColNameOpsDetail.MACHINENAME, align: 'left' },
            { name: 'MachineCount', index: 'MachineCount', width: 80, label: arrColNameOpsDetail.MACHINECOUNT, align: 'center' },
            { name: 'ManCount', index: 'ManCount', width: 80, label: arrColNameOpsDetail.MANCOUNT, align: 'center' },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSeiral', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'OpGroup', index: 'OpGroup', hidden: true },
            { name: 'MachineType', index: 'MachineType', hidden: true },
            { name: 'DisplayColor', index: 'DisplayColor', hidden: true },
            { name: 'HotSpot', index: 'HotSpot', hidden: true },
        ],
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
        },
        onSelectRow: function (rowid) { },
        loadComplete: function () { },
        ondblClickRow: function (rowid, abc) { },
        ajaxGridOptions: { async: false }
    });

    function markHotSpot(cellvalue, options, rowObject) {
        if (rowObject.HotSpot === "1") {
            return "<i class='fa fa-flag' style='color: red'></i> ";
        }
        return "";
    }

    //Hide checkbox all
    var myGrid = $(TableProcessCloneId);
    $("#cb_" + myGrid[0].id).hide();

    SearchFilter(myGrid);
}

function BindDataToJqGridProcessNameTemplate(actionCode, lanId) {
    jQuery(TableProcessNameTemplateId).jqGrid({
        url: '/OPS/GetProcessNameTemplate',
        postData: {
            actionCode: actionCode,
            lanId: lanId
        },
        datatype: "json",
        height: 200,
        shrinkToFit: false,
        width: null,
        viewrecords: false,
        rowNum: -1,
        rownumbers: false,
        gridview: true,
        multiselect: true,
        colModel: [
            { name: 'OpName', index: 'OpName', width: 700, label: "Process Name" },
            { name: 'OpNameId', index: 'OpNameId', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get process name template", err.message, ObjMessageType.Error);
        },
        onSelectRow: function (rowid) {
            //var row = $(gridOpsDetailId).jqGrid("getRowData", rowid);
        }
    });
}
// #endregion

// #region Fill data to dropdown list

function FillDataToDropdownOpName(arrOpName) {
    $('#drpOpName').multiselect('destroy');
    $("#drpOpName").empty();
    var option = '';
    for (let i = 0; i < arrOpName.length; i++) {
        option += '<option value="' + arrOpName[i]["OpNameId"] + '">' + arrOpName[i]["OpName"] + "</option>";
    }
    $('#drpOpName').append(option);

    $("#drpOpName").multiselect({
        includeSelectAllOption: false,
        enableCaseInsensitiveFiltering: true,
        delimiterText: " | ",
        buttonWidth: "100%",
        maxHeight: 300,
        buttonClass: "btn-multiple-select",
        onDropdownHide: function () {
            $('#drpTemplate').val(null).trigger('change.select2'); //add new

            // Get selected options.
            var selectedVal = $("#drpOpName").val();
            var strText = "";

            //Clear array selected opname.
            ArrSelectedOpname = [];

            //var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
            if (window.CurrentOpmt) {
                const opSerial = $("#txtProcessNo").val();

                if (ArrayListIsNull(selectedVal)) { $("#hdOpNameTex").val(strText); return; };

                let opnSerial = 1;
                for (let i = 0; i < selectedVal.length; i++) {
                    const val = selectedVal[i];
                    const txt = $("#drpOpName option[value='" + val + "']").text();
                    if (i === 0) {
                        strText = ConvertString(txt);
                    } else {
                        strText += " | " + txt;
                    }
                    const objOpName = {
                        StyleCode: CurrentOpmt.StyleCode, StyleSize: CurrentOpmt.StyleSize,
                        StyleColorSerial: CurrentOpmt.StyleColorSerial, RevNo: CurrentOpmt.RevNo,
                        OpRevNo: CurrentOpmt.OpRevNo, OpSerial: opSerial, Edition: CurrentOpmt.Edition, OpNameId: val,
                        OpName: txt, OpTime: '', OpnSerial: opnSerial
                    };
                    opnSerial++;
                    ArrSelectedOpname.push(objOpName);
                }
            }

            $("#hdOpNameTex").val(strText);
        }
    });
}

function FillDataToDropdownMachine(arrDataSource, idDropdownlistTool, idDropdownlistToolMain) {
    //drpMachine
    $('#' + idDropdownlistTool).multiselect('destroy');
    $('#' + idDropdownlistTool).empty();
    var a = $('#' + idDropdownlistTool).val();
    var option = '';
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i]["ItemCode"] + '">' + arrDataSource[i]["ItemName"] + '</option>';
    }

    $('#' + idDropdownlistTool).append(option);

    $("#" + idDropdownlistTool).multiselect({
        //includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: '100%',
        maxHeight: 300,
        buttonClass: 'btn-multiple-select',
        onDropdownHidden: function () {
            var arrMachine = [];
            var arrMachineVal = $("#" + idDropdownlistTool).val();
            ArrSelectedTool = arrMachineVal;
            ArrSelectedMachine = arrMachineVal;
            if (!ArrayListIsNull(arrMachineVal)) {
                for (var i = 0; i < arrMachineVal.length; i++) {

                    var val = arrMachineVal[i];
                    var txt = $("#" + idDropdownlistTool + " option[value='" + val + "']").text();

                    arrMachine.push({
                        'ItemCode': val,
                        'ItemName': txt
                    });
                }
            };

            //drpMachineDefault
            FillDataToDropDownlist(idDropdownlistToolMain, arrMachine, "ItemCode", "ItemName");
        }
    });
}

function FillDataToDropdownCategory(arrDataSource, idDropdownlistCategory, isTool) {
    if ($('#' + idDropdownlistCategory) && $('#' + idDropdownlistCategory)[0]) {
        const opCount = $('#' + idDropdownlistCategory)[0].length;
        if (opCount !== 0) $('#' + idDropdownlistCategory).multiselect('destroy');
    }

    $('#' + idDropdownlistCategory).empty();

    var option = '';
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i]["SubCode"] + '">' + arrDataSource[i]["CodeName"] + '</option>';
    }

    $('#' + idDropdownlistCategory).append(option);

    $("#" + idDropdownlistCategory).multiselect({
        includeSelectAllOption: false,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: '100%',
        maxHeight: 300,
        buttonClass: 'btn-multiple-select',
        onDropdownHidden: function () {
            var arrCat = $("#" + idDropdownlistCategory).val();
            if (!ArrayListIsNull(arrCat)) {
                if (isTool === "1") {
                    // Load list of tool                   
                    GetToolDataAndFillToDropdownList("1", arrCat);

                } else {
                    GetToolDataAndFillToDropdownList("0", arrCat);
                }
            };
        },
        onChange: function (option, checked) {
            // Get selected options.
            var selectedOptions = $("#" + idDropdownlistCategory + " option:selected");
            if (selectedOptions.length >= 10) {
                // Disable all other checkboxes.
                var nonSelectedOptions = $("#" + idDropdownlistCategory + " option").filter(function () {
                    return !$(this).is(':selected');
                });

                nonSelectedOptions.each(function () {
                    var input = $('input[value="' + $(this).val() + '"]');
                    input.prop('disabled', true);
                    input.parent('li').addClass('disabled');
                });
            }
            else {
                // Enable all checkboxes.
                $("#" + idDropdownlistCategory + " option").each(function () {
                    var input = $('input[value="' + $(this).val() + '"]');
                    input.prop('disabled', false);
                    input.parent('li').addClass('disabled');
                });
            }
        }
    });
}

//Fill data to dropdownlist tool and machine
function GetToolDataAndFillToDropdownList(isTool, lstCategoryId) {
    const url = StatusUpdateProcess === 1 ? "/Ops/GetOpMachineMaster2" : "/Ops/GetOpMachineMaster";
    const config = ObjectConfigAjaxPost(url, true, JSON.stringify({ isTool: isTool, lstCategoryId: lstCategoryId }));

    AjaxPostCommon(config, function (respone) {
        if (isTool === "1") {
            FillDataToDropdownMachine(respone, "drpTool", "drpToolMain");
            $('#drpTool').multiselect('select', ArrSelectedTool, true);
        } else {
            FillDataToDropdownMachine(respone, "drpMachine", "drpMachineDefault");
            $('#drpMachine').multiselect('select', ArrSelectedMachine, true);
        }
    });
}

//start Ha
//Fill data to dropdownlist in Process Detail
//Fill data to Machine type
function GetSelectMachineType() {
    $("#selectMachineTypeSubDetail").select2({
        minimumInputLength: 2,
        containerCss: { width: '100%' },
        allowClear: true,
        placeholder: 'Select machine type',
        ajax: {
            url: '/Ops/SearchMachineName',
            delay: 250,
            data: function (params) {
                return {
                    q: params.term, // search term
                };
            },
            processResults: function (data) {

                var dataMod = [];
                if ($.isEmptyObject(data)) {
                    dataMod = { id: "1", text: "2" };
                }
                else {
                    $.each(data.items, function (index, item) {
                        var newItem = { id: item.ItemCode, text: item.ItemName };
                        dataMod.push(newItem);
                    });
                }
                return {
                    results: dataMod
                };
            },
        },
        editrules: { edithidden: true, required: true }

    });
}

//Fill data to Job type
function FillDataToDropDownlistJobType(OpType) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: OpType }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpJobTypeSubDetail", respone, "SubCode", "CodeName");
    });
}

//Fill data to Tool ID
function SelectToolID() {
    $("#selectToolIdSubDetail").select2({
        minimumInputLength: 2,
        containerCss: { width: '100%' },
        allowClear: true,
        placeholder: 'Select tool ID',
        ajax: {
            url: '/Ops/SearchToolId',
            delay: 250,
            data: function (params) {
                return {
                    q: params.term, // search term
                };
            },
            processResults: function (data) {

                var dataMod = [];
                if ($.isEmptyObject(data)) {
                    dataMod = { id: "1", text: "2" };
                }
                else {
                    $.each(data.items, function (index, item) {
                        var newItem = { id: item.ItemCode, text: item.ItemName };
                        dataMod.push(newItem);
                    });
                }
                return {
                    results: dataMod
                };
            },
        },
        editrules: { edithidden: true, required: true }

    });
}

//Fill data to Action code
function FillDataToDropDownlistActionCode(OpType) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: OpType }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpActionCodeSubDetail", respone, "SubCode", "CodeName");
    });
}
//end Ha

// #endregion

// #region Init data
window.InitMasterData = function initMasterData(currentOpmt) {
    if (currentOpmt && currentOpmt.StyleCode) {
        GetModulesByStyleCode(currentOpmt.StyleCode);
        GetMasterCodeTool("0"); //Get list of category tool 
        GetMasterCodeMachine("1"); //Get machine category
        GetMasterCodeOpGroup(OpGroup); //Get process group
        GetMasterCodeOpType(OpType); //Get Job code
        GetActionCodeOpType("ActionCode");
        GetFactory();
        const arrCategory = ["CSW", "SEW"];

        //Get list of tool and machine based on list of categories
        GetToolDataAndFillToDropdownList("0", arrCategory);
        GetToolDataAndFillToDropdownList("1", arrCategory);
        JqGridSelectedProcess("");
    }
};

function InitDataUpdateProcess() {
    //Set status update process is 1.
    StatusUpdateProcess = 1;
    ShowModal(ProcessModal);
    InitDataForProcessModal();

    var menuId = "";
    if (CurrentOpmt.Edition === editionOps || CurrentOpmt.Edition === editionPdm)
        menuId = MenuIdOpm;
    else if (CurrentOpmt.Edition === editionAom)
        menuId = MenuIdAom;

    //Get user role.
    var userRole = GetUserRoleInfo(SystemIdOps, menuId);
    if (!$.isEmptyObject(userRole) || userRole.IsUpdate === "1") {
        $('#btnUpdateProcess').show();
        $('#btnSaveProcess').hide();
    } else {
        $('#btnUpdateProcess').hide();
        $('#btnSaveProcess').hide();
    }
}

function InitDataAddNewProcess() {
    StatusUpdateProcess = 0; //Set status update process is 0.
    //GetMaxOpSerial();
    InitDataForProcessModal();
    $('#btnUpdateProcess').hide();
    $('#btnSaveProcess').show();
}

//Init data for add new process modal
function InitDataForProcessModal() {
    if (StatusUpdateProcess === 1) {
        $("#btnRemoveVideo").prop("disabled", false);
        $("#btnEnterProcess").prop("disabled", false);
    } else if (StatusUpdateProcess === 0) {
        $("#btnRemoveVideo").prop("disabled", true);
        $("#btnEnterProcess").prop("disabled", false);
    } else {
        $("#btnEnterProcess").prop("disabled", true);
    }
    //Check on ops layout screen
    if (IsLayoutEvent === "1") {
        switch (LayoutGroupMode) {
            case ModuleTypeConst:
                $("#drpModule").prop("disabled", true);
                break;
            case OpGroupConst:
                $("#drpProcessGroup").prop("disabled", true);
                break;
            case MachineTypeConst:
                $("#drpMachineDefault").prop("disabled", true);
                break;
        }
    }
}
// #endregion

// #region Method for add or update process
function initMasterDataProcessModal() {
    if (window.CurrentOpmt && window.CurrentOpmt.StyleCode) GetModulesByStyleCode(window.CurrentOpmt.StyleCode);
    GetMasterCodeTool("0"); //Get list of category tool 
    GetMasterCodeMachine("1"); //Get machine category
    GetMasterCodeOpGroup(OpGroup); //Get process group
    GetMasterCodeOpType(OpType); //Get Job code
    GetActionCodeOpType("ActionCode");
    GetFactory();
    const arrCategory = ["CSW", "SEW"];

    //Get list of tool and machine based on list of categories
    GetToolDataAndFillToDropdownList("0", arrCategory);
    GetToolDataAndFillToDropdownList("1", arrCategory);
    JqGridSelectedProcess("");
}

function LayoutLoadListProcessClone(callBack) {
    ShowModal("mdlCloneProcess");
    BindDataToJqGridCloneProcess("", "", "", "", "", "", "");

    var postData = {
        styleCode: CurrentOpmt.StyleCode,
        styleSize: CurrentOpmt.StyleSize,
        styleColor: CurrentOpmt.StyleColorSerial,
        revNo: CurrentOpmt.RevNo,
        opRevNo: CurrentOpmt.OpRevNo,
        edition: CurrentOpmt.Edition,
        languageId: MapLanguageToFlag(CurrentOpmt.Language)
    };
    ReloadJqGrid(TableProcessCloneName, postData);

    CloneProcess(callBack);
}

function LayoutSaveEvent(callBack) {
    ClearDataAddNewProccess(); // Clear all of controls
    ShowModal(ProcessModal);
    IsUpdateProcess = false;
    IsInsertProcess = true;

    IsLayoutEvent = "1";
    $(`#${BtnSaveOpdt}`).show();
    $(`#${BtnUpdateOpdt}`).hide();

    GetMaxOpSerial();

    $("#btnSaveProcess").unbind().click(function () {
        SaveNewProcess(null, callBack);
    });
}

function LayoutUpdateEvent(layoutOpdt, callBack) {
    IsLayoutEvent = "1";
    IsUpdateProcess = true;
    IsInsertProcess = false;

    $(`#${BtnSaveOpdt}`).hide();

    ClearDataAddNewProccess(); // Clear all of controls
    RemoveRedBorder(); // Remove red color border of fields.
    ShowModal(ProcessModal);
    layoutOpdt.StyleCode = CurrentOpmt.StyleCode;
    layoutOpdt.StyleSize = CurrentOpmt.StyleSize;
    layoutOpdt.StyleColorSerial = CurrentOpmt.StyleColorSerial;
    layoutOpdt.RevNo = CurrentOpmt.RevNo;
    layoutOpdt.OpRevNo = CurrentOpmt.OpRevNo;
    layoutOpdt.OpSerial = layoutOpdt.id;
    layoutOpdt.Edition = CurrentOpmt.Edition;
    layoutOpdt.LanguageId = CurrentOpmt.Language;

    window.DisplayColor = layoutOpdt.DisplayColor;

    GetObjectOpsDetail(layoutOpdt, (opdt) => {
        opdt.LanguageId = CurrentOpmt.Language;
        opdt.GroupTitle = layoutOpdt.GroupTitle;

        GetOpdtForModal(opdt);
        BindDataToJqGridInputOpTimeModal([]);

        // Unbind avoid clicking event firing multiples times.
        $("#btnUpdateProcess").unbind().click(() => {
            UpdateProcess(null, callBack, opdt);
        });
    });
}

var ToUpdateOpdt;

function LoadDataOpdtDetail(currentOpdt) {
    currentOpdt.StyleCode = CurrentOpmt.StyleCode;
    currentOpdt.StyleSize = CurrentOpmt.StyleSize;
    currentOpdt.StyleColorSerial = CurrentOpmt.StyleColorSerial;
    currentOpdt.RevNo = CurrentOpmt.RevNo;
    currentOpdt.OpRevNo = CurrentOpmt.OpRevNo;
    currentOpdt.OpSerial = currentOpdt.id;
    currentOpdt.Edition = CurrentOpmt.Edition;
    currentOpdt.LanguageId = CurrentOpmt.Language;

    window.DisplayColor = currentOpdt.DisplayColor;

    const opdt = GetObjectOpsDetail(currentOpdt);
    opdt.LanguageId = CurrentOpmt.Language;
    opdt.GroupTitle = currentOpdt.GroupTitle;
    ToUpdateOpdt = opdt;

    BindDataToJqGridInputOpTimeModal([]);
    GetOpdtForModal(opdt);
}

function GetOpNamesFromJqgrid() {
    const gridPro = $("#tbOpTime").getGridParam('data');

    //Check length of grid process detail
    if (IsInsertProcess && gridPro.length <= 0) {
        $.unblockUI();
        ColorButtonBorder("btnEnterProcess", "error-border");
        ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
        return [];
    }

    for (let i = 0; i < gridPro.length; i++) {
        const opTime = $("#txtOpntTime_" + (i + 1)).val(); //Get operation time in textbox

        //Set operation time again.
        gridPro[i].OpTime = opTime;
        gridPro[i].Edition = "M";
    }

    return gridPro;
}

//Add new process to database
function SaveNewProcess(callBackFunc, oploCallBack) {
    if (CheckDataAddNewProccess()) {
        if ($.isEmptyObject(window.CurrentOpmt)) {
            ShowMessageOk("004", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }
        const objOpDetail = CreateObjectOpDetail(window.CurrentOpmt);

        UploadImageProcess(window.CurrentOpmt, (imgRes) => {
            if (imgRes !== Fail) {
                UploadVideoProcess(window.CurrentOpmt, (videoRes) => {
                    if (videoRes !== Fail) {
                        const lstMachine = CreateObjectMachine();
                        const lstTool = CreateObjectTool();

                        // If imgRes = 3, it means there is not video for uploading. No need to set image name.
                        if (imgRes === 3) {
                            objOpDetail.ImageName = null;
                        } else {
                            if (imgRes !== Success) objOpDetail.ImageName = imgRes;
                        }

                        // If videoRes = 3 mean there is not video for uploading. No need to set video name.
                        if (videoRes === 3) {
                            objOpDetail.VideoFile = null;
                        } else {
                            if (videoRes && videoRes.length > 0) objOpDetail.VideoFile = videoRes[0];
                        }

                        const opNames = GetOpNamesFromJqgrid();
                        AddNewProcess(objOpDetail, lstMachine, lstTool, opNames, (addRes) => {
                            if (addRes === true) {
                                if (callBackFunc === null) {
                                    HideModal(ProcessModal);
                                    objOpDetail.X = LayoutLeftX;
                                    objOpDetail.Y = LayoutTopY;
                                    const node = CreateObjectForLayout(window.CurrentOpmt, objOpDetail);

                                    // Mapping objOpDetail to node
                                    oploCallBack(node);
                                } else {
                                    callBackFunc(addRes);
                                }

                                // Clear grid by null data
                                ReloadJqGridLocal(TableInputOpNameName, []);
                            }
                        });
                    } else {
                        $.unblockUI();
                        MsgInform("Error", "Could not upload process video.", "error", true, true);
                    }
                });
            } else {
                $.unblockUI();
                MsgInform("Error", "Could not upload process image.", "error", true, true);
            }
        });
    }
}

function CreateObjectForLayout(objOpMaster, objOpdt) {
    var node = {};

    node.id = objOpdt.OpSerial.toString();
    var machineName = objOpdt.MachineName === null || objOpdt.MachineName === undefined ? " " : objOpdt.MachineName;
    const name = objOpdt.OpName;
    const opNum = objOpdt.OpNum === null || objOpdt.OpNum === undefined ? " " : objOpdt.OpNum;
    node.name = `[${opNum}] ${name}`;
    node.OpTime = objOpdt.OpTime;
    node.MachineCount = objOpdt.MachineCount;
    node.MachineName = machineName;
    node.ManCount = objOpdt.ManCount;
    node.OpNum = objOpdt.OpNum;
    node.OpName = name;
    let remarks = "";
    if (objOpdt.Remarks) remarks = objOpdt.Remarks;
    node.Remarks = remarks;
    node.OpGroup = objOpdt.OpGroup;
    node.ModuleId = objOpdt.ModuleId;
    node.MachineType = objOpdt.MachineType;
    node.left = objOpdt.X;
    node.top = objOpdt.Y;
    node.DisplayColor = objOpdt.DisplayColor === null || objOpMaster.DisplayColor === undefined || objOpdt.DisplayColor === "" ? "#FFFFFF" : `#${objOpdt.DisplayColor.substring(3, 8)}`;
    node.ProcessWidth = objOpMaster.ProcessWidth === null || objOpMaster.ProcessWidth === undefined || objOpMaster.ProcessWidth === "" ? defaultProcessWidth : objOpMaster.ProcessWidth;
    node.ProcessHeight = objOpMaster.ProcessHeight === null || objOpMaster.ProcessHeight === undefined || objOpMaster.ProcessHeight === "" ? defaultProcessHeight : objOpMaster.ProcessHeight;
    node.LayoutFontSize = objOpMaster.LayoutFontSize === 0 || objOpMaster.LayoutFontSize === "0" ? defaultFontSize : objOpMaster.LayoutFontSize;
    node.Page = objOpdt.Page === 0 ? 1 : objOpdt.Page;
    node.IsDisplay = true;
    node.CanDelete = window.CanSave;
    node.ShowButtonPlayVideo = $.isEmptyObject(objOpdt.VideoFile) ? 0 : 1;
    let achieve = 0;
    if (objOpdt.Achieve) achieve = objOpdt.Achieve;
    node.Achieve = achieve;
    node.Target = CurrentMpmt.TargetQty;

    return node;
}

function UpdateProcess(callBackFunc, oploCallBack, opdt) {
    console.log("UpdateProcess...");
    if (!CheckDataAddNewProccess()) return;
    const objOpDetail = CreateObjectOpDetail(window.CurrentOpmt, opdt);

    UploadImageProcess(window.CurrentOpmt, (imgRes) => {
        if (imgRes !== Fail) {
            UploadVideoProcess(window.CurrentOpmt, (videoRes) => {
                if (videoRes !== Fail) {
                    const lstMachine = CreateObjectMachine();
                    const lstTool = CreateObjectTool();

                    //Set name of media files as previous name
                    objOpDetail.ImageName = $("#hdImageName").val();
                    objOpDetail.VideoFile = $("#hdVideoName").val();

                    // If imgRes = 3, it means there is not video for uploading. No need to set image name.
                    if (imgRes === 3) {
                        objOpDetail.ImageName = null;
                    } else {
                        if (imgRes !== Success) objOpDetail.ImageName = imgRes;
                    }

                    // If videoRes = 3 mean there is not video for uploading. No need to set video name.
                    if (videoRes === 3) {
                        objOpDetail.VideoFile = null;
                    } else {
                        if (videoRes && videoRes.length > 0) objOpDetail.VideoFile = videoRes[0];
                    }

                    const opNames = GetOpNamesFromJqgrid();
                    UpdateOpDetail(objOpDetail, lstMachine, lstTool, opNames, (updateRes) => {
                        if (updateRes === true) {
                            if (callBackFunc === null) {
                                HideModal(ProcessModal);
                                const node = CreateObjectForLayout(CurrentOpmt, objOpDetail);
                                oploCallBack(node);
                                CurrentOpmt.Edition = $("#drpOpsMasterEdition").val();
                            } else {
                                callBackFunc(addRes);
                            }
                        } else {
                            console.log(updateRes);
                        }
                    });
                } else {
                    $.unblockUI();
                    MsgInform("Error", "Could not upload process video.", "error", true, true);
                }
            });
        } else {
            $.unblockUI();
            MsgInform("Error", "Could not upload process image.", "error", true, true);
        }
    });
}

//Create object tool linking
function CreateObjectMachine() {
    var lstMachine = [];
    var arrMachine = $("#drpMachine").val();
    var selMainMachine = $("#drpMachineDefault").val();

    if ($.isEmptyObject(CurrentOpmt) || $.isEmptyObject(arrMachine)) {
        return null;
    }
    arrMachine.forEach(function (machineCode) {
        var mainMachine = machineCode === selMainMachine ? "1" : "0";
        var objMachine = {
            StyleCode: CurrentOpmt.StyleCode,
            StyleColorSerial: CurrentOpmt.StyleColorSerial,
            StyleSize: CurrentOpmt.StyleSize,
            RevNo: CurrentOpmt.RevNo,
            OpRevNo: CurrentOpmt.OpRevNo,
            OpSerial: $("#txtProcessNo").val(),
            ItemCode: machineCode,
            Machine: "1",
            MainTool: mainMachine,
            Edition: "M"
        };

        lstMachine.push(objMachine);
    });

    return lstMachine;
}

//Create object tool linking
function CreateObjectTool() {
    var lstTool = [];
    var arrTool = $("#drpTool").val();
    var selMainTool = $("#drpToolMain").val();

    if ($.isEmptyObject(CurrentOpmt) || $.isEmptyObject(arrTool)) {
        return null;
    }
    arrTool.forEach(function (toolCode) {
        var mainTool = toolCode === selMainTool ? "1" : "0";
        var objMachine = {
            StyleCode: CurrentOpmt.StyleCode,
            StyleColorSerial: CurrentOpmt.StyleColorSerial,
            StyleSize: CurrentOpmt.StyleSize,
            RevNo: CurrentOpmt.RevNo,
            OpRevNo: CurrentOpmt.OpRevNo,
            OpSerial: $("#txtProcessNo").val(),
            ItemCode: toolCode,
            Machine: "0",
            MainTool: mainTool,
            Edition: "M"
        };

        lstTool.push(objMachine);
    });

    return lstTool;
}

//Clear data on "Add New Process" modal
function ClearDataAddNewProccess() {
    //Clear temporary selected tool and machine.
    window.ArrSelectedTool = [];
    window.ArrSelectedMachine = [];
    window.ArrSelectedOpname = [];
    CompareProcessName = false;

    $("#hdOpNameTex").val("");
    $("#drpTemplate").val("").trigger('change'); //Ha add
    $("#txtProcessNo").val("");
    $("#txtProcessNumber").val("");
    $("#drpProcessGroup").val("").trigger('change');
    $('#drpTool option:selected').each(function () {
        $(this).prop('selected', false);
    });
    $('#drpTool').multiselect('refresh');
    $('#drpMachine option:selected').each(function () {
        $(this).prop('selected', false);
    });
    $('#drpMachine').multiselect('refresh');

    //Clear machine type.
    $("#drpToolMain").val("").trigger('change');
    $("#drpMachineDefault").val("").trigger('change');
    $("#txtMachineCount").val("1");
    $("#txtWorker").val("1");
    $("#txtMaxTime").val("");
    $("#txtProcessTime").val("");
    $("#txtStitchCount").val("");
    $("#txtRemarks").val("");
    $("#chkHotSpot").prop("checked", false);
    $("#chkOutsourcing").prop("checked", false);
    $("#rdAssembly").prop("checked", false);
    $("#rdFinalAssembly").prop("checked", false);
    $("#rdQa").prop("checked", false);
    $("#rdNormal").prop("checked", false);

    RemovePreviewVideo();
    RemovePreviewImage();
    RemoveRedBorder();

    $("#drpModule").prop("disabled", false);
    $("#drpProcessGroup").prop("disabled", false);
    $("#drpMachineDefault").prop("disabled", false);
    $("#drpJobCode").val(null).trigger("change");
    $("#drpActionCode").val(null).trigger("change");
    $("#drpModule").val(null).trigger("change");
    $("#selFactory").val(null).trigger("change");
    $("#drpProcessGroup").val(null).trigger("change");
    $("#txtCostingGroup").val("");
    $("#txtOfferPrice").val("");
    $("#txtProcessCost").val("");
    $("#txtBenchmarkTime").val("");

    //Reload grid input process with no data
    ReloadJqGridLocal("tbOpTime", []);

    //Set textbox process name is empty
    $("#txtProcessName").val("");
}

function RemoveRedBorder() {
    //Remove class normal-error
    RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpProcessGroup", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpJobCode", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpMachineDefault", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpToolMain", "error-border");
    RemoveClass("txtMachineCount", "error-border");
    RemoveClass("txtWorker", "error-border");
    RemoveClass("txtMaxTime", "error-border");
    RemoveClass("txtProcessTime", "error-border");
    RemoveClass("btnOpTime", "error-border");
    RemoveClass("btnEnterProcess", "error-border");
}

function CheckProcessNameAndOpTime() {
    let check = true;
    const valOpName = $("#drpOpName").val();
    const valTemplate = $("#drpTemplate").val();

    if ($.isEmptyObject(valOpName) && isEmpty($("#hdOpNameTex").val()) && $.isEmptyObject(valTemplate)) {
        ColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
        check = false;
    }
    else if (!$.isEmptyObject(valTemplate)) {
        RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
        const dataRows = GetAllRowsDataJqGrid(TableInputOpNameId);
        if ($.isEmptyObject(dataRows)) {
            ColorButtonBorder("btnOpTime", "error-border");
            check = false;
        }
        else {
            for (let i = 0; i < dataRows.length; i++) {
                const opTime = $("#txtOpTime_" + (i + 1)).val();
                if (isEmptyOrWhiteSpace(opTime)) {
                    ColorButtonBorder("btnOpTime", "error-border");
                    check = false;
                }
            }
        }
    }
    else {
        RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");

        if (valOpName && valOpName.length > 1) {
            //Check list optime
            const dataRows = GetAllRowsDataJqGrid(TableInputOpNameId);
            if ($.isEmptyObject(dataRows) && !$.isEmptyObject($("#drpOpName").val())) {
                ColorButtonBorder("btnOpTime", "error-border");

                check = false;
            } else {
                for (let i = 0; i < dataRows.length; i++) {
                    const opTime = $("#txtOpTime_" + (i + 1)).val();
                    if (isEmptyOrWhiteSpace(opTime)) {
                        ColorButtonBorder("btnOpTime", "error-border");

                        check = false;
                    }
                }
            }
        }
        if (check) RemoveClass("btnOpTime", "error-border");
    }
    return check;
}

//Check data before add new process
function CheckDataAddNewProccess() {
    let check = true;
    if ($.isEmptyObject(window.CurrentOpmt)) {
        check = false;
    }

    if (isEmpty($("#txtProcessNo").val())) {
        ColorTextbox("txtProcessNo", "error-border");
        check = false;
    } else {
        RemoveClass("txtProcessNo", "error-border");
    }

    if (!$.isEmptyObject($("#drpMachine").val()) && LayoutGroupMode !== MachineTypeConst) {
        if (isEmpty($("#drpMachineDefault").val())) {
            ColorBorderDropdownlistSelect2("drpMachineDefault", "error-border");
            check = false;
        } else {
            RemoveColorBorderDropdownlistSelect2("drpMachineDefault", "error-border");
        }
    } else {
        RemoveColorBorderDropdownlistSelect2("drpMachineDefault", "error-border");
    }

    if (!$.isEmptyObject($("#drpTool").val())) {
        if (isEmpty($("#drpToolMain").val())) {
            ColorBorderDropdownlistSelect2("drpToolMain", "error-border");
            check = false;
        } else {
            RemoveColorBorderDropdownlistSelect2("drpToolMain", "error-border");
        }
    } else {
        RemoveColorBorderDropdownlistSelect2("drpToolMain", "error-border");
    }

    if (isEmpty($("#txtMachineCount").val())) {
        ColorTextbox("txtMachineCount", "error-border");
        check = false;
    } else {
        RemoveClass("txtMachineCount", "error-border");
    }

    if (isEmpty($("#txtWorker").val())) {
        ColorTextbox("txtWorker", "error-border");
        check = false;
    } else {
        RemoveClass("txtWorker", "error-border");
    }

    if (isEmpty($("#txtMaxTime").val())) {
        ColorTextbox("txtMaxTime", "error-border");
        check = false;
    } else {
        RemoveClass("txtMaxTime", "error-border");
    }

    if (isEmpty($("#txtProcessTime").val())) {
        ColorTextbox("txtProcessTime", "error-border");
        check = false;
    } else {
        RemoveClass("txtProcessTime", "error-border");
    }

    //Checking process name
    if (isEmptyOrWhiteSpace($("#txtProcessName").val())) {
        ColorButtonBorder("btnEnterProcess", "error-border");
        check = false;
    } else {
        RemoveClass("btnOpTime", "error-border");
    }

    if (HasFile($("#flProcessVideo")[0].files)) {
        if (!CheckTypeOfVideo($("#flProcessVideo")[0].files)) {
            ShowMessageOk("008", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, ArrVideoType.toString());
            return false;
        }
    }

    if (check === false) {
        ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
    }
    return check;
}

function OraAddProcess(opdt, machines, tools, opnts) {
    $.post(OraAddOpdt, { opdt, machines, tools, opnts }).done((response) => {
        console.log(response);
    });
}

//Add new process
function AddNewProcess(objOpDetail, lstMachine, lstTool, lstOpnt, callBack) {
    $.ajax({
        url: "/Ops/AddNewProcess",
        async: true,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail, lstOpMachine: lstMachine, lstOpTool: lstTool, lstOpnt: lstOpnt }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                callBack(true);
                ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add, ObjMessageType.Info);
            } else {
                callBack(false);
                ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);
            }
        },
        beforeSend: () => {
            $.blockUI({ message: "<h3>Saving data...</h3>" });
        },
        complete: () => {
            $.unblockUI();
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, ObjMessageType.Error,
                errorThrown.message);
            callBack(false);
        }
    });
}

function UpdateOpDetail(objOpDetail, lstMachine, lstTool, lstOpnt, callBack) {
    $.ajax({
        url: "/Ops/UpdateOpDetail",
        async: true,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail, lstMachine: lstMachine, lstTool: lstTool, lstOpnt: lstOpnt }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, ObjMessageType.Info);
                callBack(true);
            } else {
                ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);
                callBack(false);
            }
        },
        beforeSend: () => {
            $.blockUI({ message: "<h3>Updating data...</h3>" });
        },
        complete: () => {
            $.unblockUI();
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);
            callBack(false);
        }
    });
}

//Update filename upload
function UpdateFilenameUpload(fileType, objOpDetail, lstFileName, fncCallBack) {
    $.ajax({
        url: "/Ops/UpdateFileNameUpload",
        async: false,
        type: "POST",
        data: JSON.stringify({ fileType: fileType, opDetail: objOpDetail, lstFileName: lstFileName }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            fncCallBack(res);
        },
        error: function (xhr, status, error) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);

        }
    });
}

//Create object opeartion plan detail
function CreateObjectOpDetail(objOpsMaster, opdt) {
    if ($.isEmptyObject(objOpsMaster)) return null;
    let opGroup, machineType, moduleId, iotType = "", processX = "0", processY = "0", ptDt = GetMaterialAndPaintingType();
    var machineName = $("#drpMachineDefault option:selected").text();
    machineType = $("#drpMachineDefault").val();
    moduleId = $("#drpModule").val();

    if ($("#rdAssembly").is(':checked')) {
        iotType = "SA";
    } else if ($("#rdFinalAssembly").is(':checked')) {
        iotType = "FA";
    } else if ($("#rdQa").is(':checked')) {
        iotType = "QA";
    } else if ($("#rdNormal").is(':checked')) {
        iotType = "";
    }
    if (opdt && opdt.OpGroup) opGroup = opdt.OpGroup;
    if (opdt) processX = window.LayoutLeftX;
    if (opdt) processY = window.LayoutTopY;

    const objOpDetail = {
        Edition: "M",
        StyleCode: objOpsMaster.StyleCode,
        StyleSize: objOpsMaster.StyleSize,
        StyleColorSerial: objOpsMaster.StyleColorSerial,
        RevNo: objOpsMaster.RevNo,
        OpRevNo: objOpsMaster.OpRevNo,
        OpSerial: $("#txtProcessNo").val(),
        OpNum: $("#txtProcessNumber").val(),
        OpGroup: opGroup,
        OpName: $("#txtProcessName").val(),
        OpTime: $("#txtProcessTime").val(),
        MachineType: machineType,
        MachineName: machineName,
        OpPrice: $("#txtProcessCost").val(),
        OfferOpPrice: $("#txtOfferPrice").val(),
        MachineCount: $("#txtMachineCount").val(),
        Remarks: $("#txtRemarks").val(),
        MaxTime: $("#txtMaxTime").val(),
        ManCount: $("#txtWorker").val(),
        ImageName: $("#flProcessImage").val().split('\\').pop(),
        VideoFile: $("#flProcessVideo").val().split('\\').pop(),
        JobType: $("#drpJobCode").val(),
        BenchmarkTime: $("#txtBenchmarkTime").val(),
        ModuleId: moduleId,
        HotSpot: $("#chkHotSpot").is(":checked") === true ? "1" : "0",
        ToolId: $("#drpToolMain").val(),
        OpsState: $("#chkOutsourcing").is(":checked") === true ? "1" : "0",
        OpTimeMax: objOpsMaster.OpTime,
        Page: isEmpty(LayoutPage) === true ? "" : LayoutPage,
        DisplayColor: isEmpty(DisplayColor) === true ? "" : DisplayColor,
        OpTimeBalancing: $("#txtProcessTime").val(),
        ActionCode: $("#drpActionCode").val(),
        Factory: $("#selFactory").val(),
        StitchCount: $("#txtStitchCount").val(),
        IotType: iotType,
        MxPackage: CurrentMesPackage.MxPackage,
        X: processX,
        Y: processY,
        PaintingType: ptDt.PaintingType,
        MaterialType: ptDt.MaterialType,
        DryingTime: $("#drpDryingTime").val(),
        Temperature: $("#drpTemperature").val(),
        CoolingTime: $("#drpCoolingTime").val()
    };

    return objOpDetail;
}

function GetOpdtForModal(opdt) {
    $("#txtProcessNo").val(ZeroPad(opdt.OpSerial, 3));
    $("#txtProcessNumber").val(opdt.OpNum);
    document.getElementById("txtGroupTitle").value = opdt.GroupTitle;
    $("#txtProcessTime").val(opdt.OpTime);
    $("#hdOpNameTex").val(opdt.OpName);
    $("#txtProcessName").val(opdt.OpName);
    $("#txtOfferPrice").val(opdt.OfferOpPrice);
    $("#txtMachineCount").val(opdt.MachineCount);
    $("#txtRemarks").val(opdt.Remarks);
    $("#txtMaxTime").val(opdt.MaxTime);
    $("#txtWorker").val(opdt.ManCount);
    $("#txtStitchCount").val(opdt.StitchCount);
    $("#txtBenchmarkTime").val(opdt.BenchmarkTime);
    $("#txtProcessCost").val(opdt.OpPrice);
    $("#chkHotSpot").prop('checked', StringToBoolean(opdt.HotSpot));
    $("#chkOutsourcing").prop('checked', StringToBoolean(opdt.OpsState));

    $.unblockUI();

    //Get machine and tool
    var lstTool = GetListOpToolLinking(opdt);

    //Get process name detail 
    GetListProcessNameDetail(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
        opdt.RevNo, opdt.OpRevNo, opdt.OpSerial, opdt.LanguageId, function (lstOpnts) {
            //Reload OpNameInput gridview.
            ReloadJqGridLocal("tbOpTime", lstOpnts);
        });
    var arrCategory = [], arrTool = [], arrMc = [], arrSelectedTool = [], arrSelectedMachine = [];

    $.each(lstTool, function (index, value) {
        //Get tool and machine
        if (value.Machine === "0") {
            const objTool = { ItemCode: value.ItemCode, ItemName: value.ItemName };
            arrTool.push(objTool);
            arrSelectedTool.push(value.ItemCode);
        } else {
            const objMachine = { ItemCode: value.ItemCode, ItemName: value.ItemName };
            arrMc.push(objMachine);
            arrSelectedMachine.push(value.ItemCode);
        }
        //Get list category
        if (!IsInArray(value.CategId, arrCategory)) {
            arrCategory.push(value.CategId.trim());
        }
    });

    //Add machine type to selected array - old machine
    if ($.isEmptyObject(lstTool) && !$.isEmptyObject(opdt.MachineType)) {
        var objOldMachine = { ItemCode: opdt.MachineType, ItemName: opdt.MachineName };
        arrMc.push(objOldMachine);
        arrSelectedMachine.push(opdt.MachineType);
    }
    window.ArrSelectedTool = arrSelectedTool;
    window.ArrSelectedMachine = arrSelectedMachine;

    //Get list of tool and machine base on list of category
    arrCategory = $.isEmptyObject(arrCategory) ? ["CSW", "SEW"] : arrCategory;
    GetToolDataAndFillToDropdownList("0", arrCategory);
    GetToolDataAndFillToDropdownList("1", arrCategory);

    if (arrCategory) {
        $('#drpCategoryTool').multiselect('select', arrCategory, true);
        $('#drpCategoryMachine').multiselect('select', arrCategory, true);
    }

    let selectedModule, selectedJobType, selectedActionCode, selectedFactory;
    if (opdt && opdt.ModuleId) selectedModule = opdt.ModuleId;
    if (selectedModule) $("#drpModule").val(selectedModule).trigger("change");
    if (opdt && opdt.JobType) selectedJobType = opdt.JobType;
    if (selectedJobType) $("#drpJobCode").val(selectedJobType).trigger("change");
    if (opdt && opdt.ActionCode) selectedActionCode = opdt.ActionCode;
    GetActionCodeOpType("ActionCode", selectedActionCode);
    if (opdt && opdt.Factory) selectedFactory = opdt.Factory;
    if (selectedFactory) $("#selFactory").val(selectedFactory).trigger("change");

    FillDataToDropDownlist("drpMachineDefault", arrMc, "ItemCode", "ItemName");
    $("#drpMachineDefault").val(opdt.MachineType).trigger("change");

    //Load main tool
    FillDataToDropDownlist("drpToolMain", arrTool, "ItemCode", "ItemName");
    $("#drpToolMain").val(opdt.ToolId).trigger("change");

    let iotType = opdt.IotType, assembly = false, finalAssembly = false, qa = false;
    if (iotType === IOTTYPE.Assembly) {
        assembly = true;
    } else if (iotType === IOTTYPE.FinalAssembly) {
        finalAssembly = true;
    } else if (iotType === IOTTYPE.Qa) {
        qa = true;
    }

    $("#rdAssembly").prop('checked', assembly);
    $("#rdFinalAssembly").prop('checked', finalAssembly);
    $("#rdQa").prop('checked', qa);

    //Get image name and video name
    $("#hdImageName").val(opdt.ImageName);
    $("#hdVideoName").val(opdt.VideoFile);
    $("#txtRemarks").val(opdt.Remarks);

    var imageName = opdt.ImageName, imgPath = "/img/no-image.png";
    if (!isEmpty(imageName)) imgPath = opdt.ImageLink;
    $("#imgPreview").attr("src", imgPath);

    //Loading video
    var videoFilename = opdt.VideoFile, posterPath = "/img/no-video.png";
    let srtSrc = "";
    if (!isEmpty(videoFilename)) {
        srtSrc = opdt.VideoOpLink;
        posterPath = "";
    }
    $("#vidPreview").attr("src", srtSrc);
    $("#vidPreview").attr("poster", posterPath);

    LoadPtdt(opdt); // Loading painting detail

    //START ADD - SON) 14/Jul/2020
    let mesRow = GetSelectedOneRowData(tableMesPackageId);
    $("#lblMxPackageId").text(mesRow.MxPackage);
    //END ADD - SON) 14/Jul/2020
}

function EventKeyUpMachineCount() {
    $("#txtMachineCount").keyup(function () {
        var maxTime = CalculateMaxTime($("#txtProcessTime").val(), $("#txtWorker").val(), $("#txtMachineCount").val());
        $("#txtMaxTime").val(maxTime);
    });
}

function EventKeyUpManCount() {
    $("#txtWorker").keyup(function () {
        var maxTime = CalculateMaxTime($("#txtProcessTime").val(), $("#txtWorker").val(), $("#txtMachineCount").val());
        $("#txtMaxTime").val(maxTime);
    });
}

// #endregion

// #region Add new process event - Modal

//Event save ops process
function ClickButtonSaveProcess() {
    IsLayoutEvent = "0";
    $("#btnSaveProcess").click(function () {
        SaveNewProcess(FunctionCallBackSavingProcess);
    });
}

function ClickButtonUpdateProcess() {
    IsLayoutEvent = "0";
    $("#btnUpdateProcess").click(function () {
        UpdateProcess(FunctionCallBackUpdateProcess);
    });
}

//Clear image upload
function ClickButtonRemoveImg() {
    $("#btnRemoveImg").click(function () {
        RemovePreviewImage();
    });
}

//Clear video upload
function ClickButtonRemoveVideo() {
    $("#btnRemoveVideo").click(function () {
        DeleteProcessVideo();
    });
}

function flProcessImageChange() {
    $("#flProcessImage").change(function (evt) {
        var fileName = evt.target.files[0].name;
        var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, Megabyte);
        if (fileSize > 4) {
            ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }

        $("#lblImageName").text(fileName + " ( " + fileSize + " MB )");

        //Preview image before upload to FTP
        readURL(this, "#imgPreview");
    });
}

$(document).on("change", "#flProcessVideo", function (evt) {
    //Clear source of video preview
    $("#vidPreview").removeAttr("src");
    $("#vidPreview").removeAttr("poster");

    //Preview video before upload
    var $source = $('#videoPreview');
    $source[0].src = URL.createObjectURL(this.files[0]);
    $source.parent()[0].load();

    //Get file name
    var fileName = evt.target.files[0].name;
    var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, "MB");
    $("#lblVideoName").text(fileName + " ( " + fileSize + " MB )");
});

//Remove video preview
function ClickButtonRemoveFileOpDetail() {
    $("#btnRemoveFileOpDetail").click(function () {
        RemoveOpDetailFilePreview();
    });
}

function SelectModule() {
    $("#drpModule, #drpActionCode").change(function () {
        if (StatusUpdateProcess === 1) return; //Load all process name.

        //Clear data process name
        $("#hdOpNameTex").val("");

        var langId = MapLanguageToFlag(CurrentOpmt.Language);
        var moduleId = $("#drpModule").val();
        var actionCode = $("#drpActionCode").val();
        var config = ObjectConfigAjaxPost("/Ops/GetOpNameByCode", true, JSON.stringify({
            languageId: langId,
            moduleId: moduleId, actionCode: actionCode, buyer: window.CurrentOpmt.Buyer
        }));
        AjaxPostCommon(config, function (response) {
            FillDataToDropdownOpName(response);
        });
    });
}

function ClickButtonOpTime() {
    $("#btnOpTime").click(function () {
        setTimeout(function () {
            ShowModal(ModalInputOpTimeName);
            //Compare current process name array with process name in database.
            var processNameVal = [];
            $.each(ListOldProcessName, function (index, value) {
                processNameVal.push(String(value.OpNameId));
            });
            CompareProcessName = Compare2Arrays($('#drpOpName').val(), processNameVal);
            if (CompareProcessName) {
                //Reload data for load data the second time
                ReloadJqGridLocal(TableInputOpNameName, ListOldProcessName);
            } else {
                //Reload data for load data the second time
                ReloadJqGridLocal(TableInputOpNameName, ArrSelectedOpname);
            }

        }, 100);
    });
}

function ClickButtonOk() {
    $("#btnGetOptime").click(function () {
        var gridData = $(TableInputOpNameId).getGridParam('data');
        var totalProcessTime = 0;

        for (var i = 0; i < gridData.length; i++) {
            var opTime = $("#txtOpTime_" + (i + 1)).val();

            if (isEmptyOrWhiteSpace(opTime)) {
                ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error,
                    "(Row " + (i + 1) + ")");
                return;
            }
            totalProcessTime += parseInt(opTime);

            //Set optime to array selected process name.
            ArrSelectedOpname[i].OpTime = opTime;

            //start Ha
            var rowdata = gridData[i];
            ArrSelectedOpname[i].MachineType = rowdata.MachineType;
            ArrSelectedOpname[i].MachineCount = rowdata.MachineCount;
            ArrSelectedOpname[i].Remarks = rowdata.Remarks;
            ArrSelectedOpname[i].MaxTime = rowdata.MaxTime;
            ArrSelectedOpname[i].ManCount = rowdata.ManCount;
            ArrSelectedOpname[i].JobType = rowdata.JobType;
            ArrSelectedOpname[i].ToolId = rowdata.ToolId;
            ArrSelectedOpname[i].ActionCode = rowdata.ActionCode;
            //end Ha
        }

        var maxTime = CalculateMaxTime(totalProcessTime, $("#txtWorker").val(), $("#txtMachineCount").val());
        $("#txtMaxTime").val(maxTime);

        $("#txtProcessTime").val(totalProcessTime);
        HideModal("mdlInputOpTime");
    });
}

//start HA NGUYEN

function EventChangeTemplate() {
    $("#drpTemplate").change(function () {
        //Unselect process name
        $('#drpOpName option:selected').each(function () {
            $(this).prop('selected', false);
        });
        $('#drpOpName').multiselect('refresh');

        ArrSelectedOpname = [];
    });
}

function ClickButtonOkTemplate() {
    $("#btnAddTemplate").click(function () {
        var lstProTemp = GetSelectedMultipleRowsData(TableProcessNameDetail);
        if ($.isEmptyObject(lstProTemp)) {
            ShowMessageOk("013", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Info, "process name template");
            return false;
        } else {
            var strText = "";
            //Clear array selected opname.
            ArrSelectedOpname = [];

            //var objOpsMaster = CurrentOpmt;
            var opSerial = $("#txtProcessNo").val();

            var opnSerial = 1;

            $.each(lstProTemp, function (idx, obj) {

                var opNameId = obj.OpNameId;
                var opName = obj.OpNameLan;

                if (idx === 0) {
                    strText = ConvertString(opName);
                } else {
                    strText += " | " + opName;
                }

                var objOpName = {
                    StyleCode: CurrentOpmt.StyleCode
                    , StyleSize: CurrentOpmt.StyleSize
                    , StyleColorSerial: CurrentOpmt.StyleColorSerial
                    , RevNo: CurrentOpmt.RevNo
                    , OpRevNo: CurrentOpmt.OpRevNo
                    , OpSerial: opSerial
                    , Edition: CurrentOpmt.Edition
                    , OpNameId: opNameId
                    , OpName: opName
                    , OpTime: ''
                    , OpnSerial: opnSerial
                    , MachineType: ''
                    , MachineCount: ''
                    , Remarks: ''
                    , MaxTime: ''
                    , ManCount: ''
                    , JobType: ''
                    , ToolId: ''
                    , ActionCode: ''
                };
                opnSerial++;
                ArrSelectedOpname.push(objOpName);
            });

            $("#hdOpNameTex").val(strText);

            HideModal(ProcessTemplate);
        }
    });
}
//end HA NGUYEN

function ClickButtonUsingTeplate() {
    $("#btnUsingProTemp").click(function () {
        var lstProTemp = GetSelectedMultipleRowsData(TableProcessNameTemplateId);
        if ($.isEmptyObject(lstProTemp)) {
            ShowMessageOk("013", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Info, "process name template");
            return false;
        } else {
            var strText = "";
            //Clear array selected opname.
            ArrSelectedOpname = [];

            var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
            var opSerial = $("#txtProcessNo").val();
            var opnSerial = 1;

            $.each(lstProTemp, function (idx, obj) {

                var opNameId = obj.OpNameId;
                var opName = obj.OpName;

                if (idx === 0) {
                    strText = ConvertString(opName);
                } else {
                    strText += " | " + opName;
                }

                var objOpName = {
                    StyleCode: objOpsMaster.StyleCode
                    , StyleSize: objOpsMaster.StyleSize
                    , StyleColorSerial: objOpsMaster.StyleColorSerial
                    , RevNo: objOpsMaster.RevNo
                    , OpRevNo: objOpsMaster.OpRevNo
                    , OpSerial: opSerial
                    , Edition: objOpsMaster.Edition
                    , OpNameId: opNameId
                    , OpName: opName
                    , OpTime: ''
                    , OpnSerial: opnSerial
                    , MachineType: ''
                    , MachineCount: ''
                    , Remarks: ''
                    , MaxTime: ''
                    , ManCount: ''
                    , JobType: ''
                    , ToolId: ''
                    , ActionCode: ''
                };
                opnSerial++;
                ArrSelectedOpname.push(objOpName);
            });
            $("#hdOpNameTex").val(strText);

            HideModal(ProcessNameTemp);

            //Unselect process name
            $('#drpOpName option:selected').each(function () {
                $(this).prop('selected', false);
            });
            $('#drpOpName').multiselect('refresh');
        }
    });
}

// #region Upload machine file

function UploadFileAlert(objResult) {
    if (objResult.Result === Success) {
        //Show alert.
        ShowMessageOk("002", SmsFunction.Upload, MessageType.Success, MessageContext.Update, ObjMessageType.Info);

        //Close modal
        HideModal("modalUploadFileOpDetail");

        //Reload data ops detail gridview.
        ReloadJqGrid(gridOpsDetailName, CurrentOpmt);
    } else {
        ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error, objResult.Content);

    }
}

// #endregion

// #endregion

// #region Clone Process Event
function CloneProcess(callBack) {

    $("#btnCloneProcess").unbind().click(function () {

        var lstOpDetail = GetSelectedMultipleRowsData(TableProcessCloneId);
        if (lstOpDetail === null) {
            ShowMessage("Clone Process", "Please select process to clone.", MessageTypeInfo);
            return;
        }

        var config = ObjectConfigAjaxPost("/OpsLayout/CloneProcess", true, JSON.stringify({ lstOpDetail: lstOpDetail }));
        AjaxPostCommon(config, function (respone) {
            if (respone.Result === Success) {
                //Create node for loading on layout.
                var lstNewOpdt = respone.Content;
                var arrLstOpdt = [];
                $.each(lstNewOpdt, function (i, val) {
                    val.X = $.isEmptyObject(val.X) ? null : val.X.split('.')[1];
                    val.Y = $.isEmptyObject(val.Y) ? null : val.Y.split('.')[1];
                    var node = CreateObjectForLayout(CurrentOpmt, val);
                    arrLstOpdt.push(node);
                });
                callBack(arrLstOpdt[0]);
                HideModal("mdlCloneProcess");
                //Update summary
                UpdateSummary();
                //Reload ops master gridview.      
                CurrentOpmt.Edition = $("#drpOpsMasterEdition").val();
                //ReloadJqGrid(gridOpsTableName, opMaster);
                ReloadJqGrid2LoCal(gridOpsTableName, CurrentOpmt);
                ShowMessage("Clone Process", "Cloned", MessageTypeInfo);
            } else {
                ShowMessage("Clone Process", respone.Content, MessageTypeInfo);
            }
        });
    });
}

function UpdateSummary() {
    var sumTit = $("#sumTitle").is(":visible");
    var sumEdi = $("#sumEditable").is(":visible");
    var sumMac = $("#sumMainMachines").is(":visible");
    var sumToo = $("#sumTool").is(":visible");
    var sumWor = $("#sumWorker").is(":visible");

    if (sumTit) {

        var titData = { top: $("#sumTitle").css("top"), left: $("#sumTitle").css("left") };
        console.log('top: ' + titData.top + '; left: ' + titData.left);
        $("#sumTitle").remove();
        setTimeout(function () {
            createSummary("sumTitle", titData);
        }, 0);
    }

    if (sumEdi) {
        var ediData = { top: $("#sumEditable").css("top"), left: $("#sumEditable").css("left") };
        console.log('top: ' + ediData.top + '; left: ' + ediData.left);
        $("#sumEditable").remove();
        setTimeout(function () {
            createSummary("sumEditable", ediData);
        }, 0);
    }

    if (sumMac) {
        var macData = { top: $("#sumMainMachines").css("top"), left: $("#sumMainMachines").css("left") };
        console.log('top: ' + macData.top + '; left: ' + macData.left);
        $("#sumMainMachines").remove();
        setTimeout(function () {
            createSummary("sumMainMachines", macData);
        }, 0);

    }

    if (sumToo) {
        var tooData = { top: $("#sumTool").css("top"), left: $("#sumTool").css("left") };
        console.log('top: ' + tooData.top + '; left: ' + tooData.left);
        $("#sumTool").remove();
        setTimeout(function () {
            createSummary("sumTool", tooData);
        }, 0);

    }

    if (sumWor) {
        var worData = { top: $("#sumWorker").css("top"), left: $("#sumWorker").css("left") };
        console.log('top: ' + worData.top + '; left: ' + worData.left);
        $("#sumWorker").remove();
        setTimeout(function () {
            createSummary("sumWorker", worData);
        }, 0);

    }
}
// #endregion

//#region Process name modal
function ButtonClickEnterProcess() {
    $("#btnEnterProcess").click(function () {
        ShowModal("mdlProcess");
    });
}

function jqGridProcessName() {
    jQuery("#tbAllOpName").jqGrid({
        url: '/Ops/GetOpName',
        datatype: "json",
        mtype: "POST",
        postData: {
            languageId: window.CurLang
        },
        serializeGridData: function (postData) {
            return JSON.stringify(postData);
        },
        ajaxGridOptions: { contentType: "application/json; charset=utf-8" },
        jsonReader: {
            page: function (obj) { return obj.page; },
            total: function (obj) {
                return obj.total;
            },
            records: function (obj) { return obj.records; },
            root: function (obj) { return obj.rows; },
            repeatitems: false,
            id: "0"
        },
        width: null,
        shrinkToFit: false,
        height: $(window).height() * 30 / 100,
        colModel: [
            { name: 'OpNameId', index: 'OpNameId', label: "Name ID", width: 100 },
            { name: 'OpName', index: 'OpName', label: "Op Name", width: 550 },
            {
                name: '', index: '', label: "", width: 100, sortable: false, hidden: false,
                formatter: function (cellvalue, option, rowObject) {
                    return "<button type='button' class='btn btn-info btn-modal' style='height:30px;width:80px;' onclick='ButtonClickAddProcess(" + option.rowId + ")' > Add </button>";
                }
            }
        ],
        rowList: [50, 100, 150],
        rowNum: 50,
        pager: '#pagerOpName',
        sortname: 'OpNameId',
        viewrecords: true,
        loadonce: false,
        multiselect: false,
        sortorder: "asc",
        caption: "All Process name",
        gridview: true,
        autowidth: false,
        ignoreCase: true //using for filter ignore case
    });
    SearchFilter($("#tbAllOpName"));

    $(window).on('resize.jqGrid', function () {
        const height = $(window).height() * 30 / 100;

        $("#tbAllOpName").jqGrid('setGridHeight', height);
    });
}

function ButtonClickAddProcess(rowid) {
    //let lstObjOpName = jQuery("#tbAllOpName").getGridParam('data');

    //Get selected process from the full list process name base on rowid.
    let objSeldProcess = jQuery("#tbAllOpName").jqGrid('getRowData', rowid);

    //Get current list selected process.
    let gridSelPro = jQuery("#tbOpTime").getGridParam('data');

    //Increase operation name serial
    let opnSerial = gridSelPro.length + 1;

    //Check if the number of process greater then 10 then show inform to user
    if (gridSelPro.length >= 10) {
        ShowMessageOk("012", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
        return false;
    }

    for (var i = 0; i < gridSelPro.length; i++) {
        if (objSeldProcess.OpNameId === gridSelPro[i].OpNameId) {
            ShowMessageOk("014", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return false;
        }
    }
    let opSerial = $("#txtProcessNo").val();
    let opNameId = objSeldProcess.OpNameId;
    let objOpNameDt = {
        StyleCode: window.CurrentOpmt.StyleCode
        , StyleSize: window.CurrentOpmt.StyleSize
        , StyleColorSerial: window.CurrentOpmt.StyleColorSerial
        , RevNo: window.CurrentOpmt.RevNo
        , OpRevNo: window.CurrentOpmt.OpRevNo
        , OpSerial: opSerial
        , Edition: window.CurrentOpmt.Edition
        , OpNameId: opNameId
        , OpName: objSeldProcess.OpName
        , OpTime: ''
        , OpnSerial: opnSerial
    };
    gridSelPro.push(objOpNameDt);
    ReloadJqGridLocal("tbOpTime", gridSelPro);
}

function ButtonClickProcessDelete(rowid) {
    //Delete a row on gridview base row id
    jQuery("#tbOpTime").jqGrid('delRowData', rowid);

    //Get current list selected process after delete row.
    let gridSelPro = jQuery("#tbOpTime").getGridParam('data');

    //Set operation name serial again
    $.each(gridSelPro, function (idx, obj) {
        obj.OpnSerial = idx + 1;
    });

    //Reload grid selected process
    ReloadJqGridLocal("tbOpTime", gridSelPro);
}

function JqGridSelectedProcess() {
    $("#tbOpTime").jqGrid({
        datatype: "local",
        loadonce: true,
        height: 150,
        width: null,
        shrinkToFit: false,
        rowNum: 10000,
        colModel: [
            { name: 'OpnSerial', index: 'OpnSerial', width: 100, label: "Serial" },
            { name: 'OpName', index: 'OpName', width: 430, label: "Operation Name" },
            { name: 'OpNameId', index: 'OpNameId', hidden: true },
            {
                name: 'OpTime', index: 'OpTime', label: "Operation Time", width: 150,
                formatter: function (cellValue, option) {
                    return '<input type="text" id="txtOpntTime_' + option.rowId + '" class="form-control" maxlength="3" ' +
                        'onkeypress = "return isNumber(event)" value= "' + cellValue + '" />';
                }
            },
            {
                name: '', index: 'Detail', width: 100, sortable: false, hidden: false,
                formatter: function (cellvalue, option, rowObject) {
                    return "<button type='button' class='btn btn-info btn-modal' style='height:30px;width:80px;' onclick='clickDetailButtonOfProcess(" + option.rowId + ")' > Detail </button>";
                }
            },
            {
                name: '', index: 'Delete', width: 100, sortable: false, hidden: false,
                formatter: function (cellvalue, option, rowObject) {
                    return "<button type='button' class='btn btn-info btn-modal' style='height:30px;width:80px;' onclick='ButtonClickProcessDelete(" + option.rowId + ")' > Delete </button>";
                }
            },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSeiral', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'MachineType', index: 'MachineType', hidden: true },
            { name: 'MachineName', index: 'MachineName', hidden: true },
            { name: 'MachineCount', index: 'MachineCount', hidden: true },
            { name: 'Remarks', index: 'Remarks', hidden: true },
            { name: 'MaxTime', index: 'MaxTime', hidden: true },
            { name: 'ManCount', index: 'ManCount', hidden: true },
            { name: 'JobType', index: 'JobType', hidden: true },
            { name: 'ToolId', index: 'ToolId', hidden: true },
            { name: 'ToolName', index: 'ToolName', hidden: true },
            { name: 'ActionCode', index: 'ActionCode', hidden: true },
        ],
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error,
                err.message);
        },
        gridComplete: function () {
            setTimeout(function () {
                updatePagerIcons();
            }, 0);
        }
    });
}

function ClickButtonOkInputOpTime() {
    $("#btnGetProDt").click(function () {
        //Get data on grid selected process detail
        let proSelList = jQuery("#tbOpTime").getGridParam('data');

        //var strOpNameId = "";
        let strProName = "";
        let totalProcessTime = 0;

        for (var i = 0; i < proSelList.length; i++) {
            var opTime = $("#txtOpntTime_" + (i + 1)).val();

            //Check process time is null or not
            if (isEmptyOrWhiteSpace(opTime)) {
                ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, "(Row " + (i + 1) + ")");
                return;
            }

            //Calculate total process time
            totalProcessTime += parseInt(opTime);

            //Combine string from each process on gridview.
            //Check the last process
            if (i === 0) {
                strProName += proSelList[i].OpName;
            } else {
                strProName += " | " + proSelList[i].OpName;
            }
        }

        var maxTime = CalculateMaxTime(totalProcessTime, $("#txtWorker").val(), $("#txtMachineCount").val());
        $("#txtMaxTime").val(maxTime);

        $("#txtProcessTime").val(totalProcessTime);
        $("#txtProcessName").val(strProName);

        HideModal("mdlProcess");
    });
}

function loadTooltipForProcessNameTextbox() {
    $("#txtProcessName").mouseenter(function () {
        let proName = $(this).val();
        $(this).attr('title', proName);
    });
}

//START ADD) SON - 2019.03.1.0 - 11/Mar/2019 - event click Detail button of process detail
function clickDetailButtonOfProcess(rowid) {
    //Edit
    if (StatusUpdateProcess === 1) {
        ClearDataProcessDetailOpName();
        const objDetail = jQuery("#tbOpTime").jqGrid('getRowData', rowid);

        SelectedObjOpnt = objDetail;
        ShowModal(ProcessTemplateDetail);

        $('#selectMachineTypeSubDetail').append('<option value=' + objDetail.MachineType + '>' + objDetail.MachineName + '</option>');
        $("#txtMachineCountSubDetail").val(objDetail.MachineCount);
        $("#txtRemarksSubDetail").val(objDetail.Remarks);
        $("#txtMaxTimeSubDetail").val(objDetail.MaxTime);
        $("#txtManCountSubDetail").val(objDetail.ManCount);
        $("#drpJobTypeSubDetail").val(objDetail.JobType).trigger('change');
        $('#selectToolIdSubDetail').append('<option value=' + objDetail.ToolId + '>' + objDetail.ToolName + '</option>');
        $('#drpActionCodeSubDetail').val(objDetail.ActionCode).trigger('change');
    }
    else {
        SelectedObjOpnt = jQuery("#tbOpTime").jqGrid('getRowData', rowid);
        ShowModal(ProcessTemplateDetail);
        ClearDataProcessDetailOpName();
    }
}
//END ADD) SON - 2019.03.1.0 - 11/Mar/2019

//Close button of Enter Process Modal
function CloseOpTime() {
    $("#btnCloseOpTime, #btnCloseOpTime1").click(function () {
        ShowConfirmYesNoMessage("001", SmsFunction.Generic, MessageType.Warning, MessageContext.IgnoreChanges, function () {
            //Set process name is empty
            $("#txtProcessName").val("");

            ReloadJqGridLocal("tbOpTime", []);
            HideModal("mdlProcess");
        }, function () { }, "");
    });
}
//#endregion

//#region Painting
var MATERIALTYPE = {
    Normal: "001",
    HeatSensitive: "002"
};

var PAINTINGTYE = {
    Promoter: "001",
    Primer: "002",
    Paint: "003"
};

function EventClickPaintingButton() {
    $("#btnShowPainting").click(function () {
        if (ListPaintingTime === null) {
            $.blockUI();
            //Get painting type time range
            GetPaintingTimeRange(null, null, function (resList) {
                ListPaintingTime = resList;
                ShowModal("mdlPaintingDetail");
                $.unblockUI();
            });
        } else {
            ShowModal("mdlPaintingDetail");
        }
    });

    $("#btnPaintingDtCancel, #btnClosePaintingDtMdl").click(function () {
        ShowConfirmYesNoMessage("001", SmsFunction.Generic, MessageType.Warning, MessageContext.IgnoreChanges, function () {
            //Clear data on painting modal
            ClearDataOnPaintingDetailModal();

            //Hide painting modal detail
            HideModal("mdlPaintingDetail");
        }, function () { }, "");
    });

    $("#rdNormalMaterial, #rdHeatSenMaterial, #rdPromoter, #rdPrimer, #rdPaint").change(function () {
        GetPtrBySelection();
    });
}

function GetPaintingTimeRange(paintingType, materialType, callBack) {
    const config = ObjectConfigAjaxPost("/Ops/GetPaintingTimeRange", true,
        JSON.stringify({ paintingType: paintingType, materialType: materialType }));
    AjaxPostCommon(config, function (resList) {
        callBack(resList);
    });
}

function ClearDataOnPaintingDetailModal() {
    //Type of material
    $("#rdNormalMaterial").prop("checked", false);
    $("#rdHeatSenMaterial").prop("checked", false);

    //Painting Type
    $("#rdPromoter").prop("checked", false);
    $("#rdPrimer").prop("checked", false);
    $("#rdPaint").prop("checked", false);

    $("#lblTemperatureRange").text("");
    $("#lblDryingTimeRange").text("");
    $("#lblCoolingTimeRange").text("");

    FillDataToDropDownlist("drpTemperature", [], "RangeValue", "RangeText");
    FillDataToDropDownlist("drpDryingTime", [], "RangeValue", "RangeText");
    FillDataToDropDownlist("drpCoolingTime", [], "RangeValue", "RangeText");
}

function GetPtrBySelection() {
    //Get material type and painting type on modal painting detail.
    const obj = GetMaterialAndPaintingType(), matType = obj.MaterialType, paiType = obj.PaintingType,
        objPt = $.grep(ListPaintingTime, (pt) => {
            return pt.PaintingType === paiType && pt.MaterialType === matType;
        });

    if (objPt.length === 0) {
        //Temperature time
        $("#drpTemperature").val('').trigger('change');

        //Drying time
        $("#drpDryingTime").val('').trigger('change');

        //Cooling time
        $("#drpCoolingTime").val('').trigger('change');

        $("#lblTemperatureRange").text("");
        $("#lblDryingTimeRange").text("");
        $("#lblCoolingTimeRange").text("");

        return;
    }
    const temperature = CreatePaintingRangeVaule(objPt[0].MinTemperature, objPt[0].MaxTemperature),
        dryingTime = CreatePaintingRangeVaule(objPt[0].MinDryingTime, objPt[0].MaxDryingTime),
        coolingTime = CreatePaintingRangeVaule(objPt[0].MinCoolingTime, objPt[0].MaxCoolingTime);

    $("#lblTemperatureRange").text(objPt[0].MinTemperature + " - " + objPt[0].MaxTemperature);
    $("#lblDryingTimeRange").text(objPt[0].MinDryingTime + " - " + objPt[0].MaxDryingTime);
    $("#lblCoolingTimeRange").text(objPt[0].MinCoolingTime + " - " + objPt[0].MaxCoolingTime);

    FillDataToDropDownlist("drpTemperature", temperature, "RangeValue", "RangeText");
    FillDataToDropDownlist("drpDryingTime", dryingTime, "RangeValue", "RangeText");
    FillDataToDropDownlist("drpCoolingTime", coolingTime, "RangeValue", "RangeText");
}

function CreatePaintingRangeVaule(min, max) {
    const rangeTime = [];

    //If min is 0 then get max value only
    if (min === 0) {
        const obj = { RangeText: max, RangeValue: max };
        rangeTime.push(obj);
        return rangeTime;
    }

    for (let i = min; i <= max; i++) {
        const obj = { RangeText: i, RangeValue: i };
        rangeTime.push(obj);
    }
    return rangeTime;
}

//Get material type and paiting type on modal paiting detail.
function GetMaterialAndPaintingType() {
    let matType = null;
    let paiType = null;

    if ($("#rdNormalMaterial").is(':checked')) matType = MATERIALTYPE.Normal;
    if ($("#rdHeatSenMaterial").is(':checked')) matType = MATERIALTYPE.HeatSensitive;
    if ($("#rdPromoter").is(':checked')) paiType = PAINTINGTYE.Promoter;
    if ($("#rdPrimer").is(':checked')) paiType = PAINTINGTYE.Primer;
    if ($("#rdPaint").is(':checked')) paiType = PAINTINGTYE.Paint;
    const obj = { MaterialType: matType, PaintingType: paiType };

    return obj;
}

function LoadPtdt(opdt) {
    const matType = opdt.MaterialType, paiType = opdt.PaintingType;

    //If material type and painting type is null then return, do not load data.
    if (isEmpty(matType) || isEmpty(paiType)) return;

    //Select radio button of material type and painting type.
    CheckedPaintingRadios(matType, paiType);

    //Get paiting type range of termperature, drying time and cooling time.
    GetPaintingTimeRange(null, null, function (resList) {
        ListPaintingTime = resList;
        GetPtrBySelection();
        $("#drpTemperature").val(opdt.Temperature).trigger('change');
        $("#drpDryingTime").val(opdt.DryingTime).trigger('change');
        $("#drpCoolingTime").val(opdt.CoolingTime).trigger('change');
    });
}

function CheckedPaintingRadios(matType, paiType) {
    switch (matType) {
        case MATERIALTYPE.Normal:
            $("#rdNormalMaterial").prop("checked", true);
            break;
        default:
            $("#rdHeatSenMaterial").prop("checked", true);
    }

    switch (paiType) {
        case PAINTINGTYE.Promoter:
            $("#rdPromoter").prop("checked", true);
            break;
        case PAINTINGTYE.Primer:
            $("#rdPrimer").prop("checked", true);
            break;
        default:
            $("#rdPaint").prop("checked", true);
    }
}
//#endregion Painting