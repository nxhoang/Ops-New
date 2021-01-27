//#region Variables
//Keep status check event is update or not.
var StatusUpdateProcess = 1;
var ProcessModal = "proccesModal";
var ProcessNameTemp = "mdlProcessNameTemplate";
var LayoutPage;
var DisplayColor;
var LayoutTopY;
var LayoutLeftX;
var LayoutGroupMode;
var IsLayoutEvent;
var ModuleTypeConst = "ModuleType";
var OpGroupConst = "OpGroup";
var MachineTypeConst = "MachineType";

var TableProcessCloneId = "#tblProcessClone";
var TableProcessCloneName = "tblProcessClone";

var TableProcessNameTemplateId = "#tbProcessNameTemplate";
var TableProcessNameTemplateName = "tbProcessNameTemplate";

//start Ha
var TableProcessNameDetail = "#tbProcessTemplate";
var ProcessTemplateDetail = "mdlProcessTemplateDetail";
var ProcessTemplate = "mdlProccesTemplate";

var CompareProcessName = false;
//var ListOldProcessName = [];
var SelectedObjOpnt;
var ListObjOpnt;
var ListPaintingTime = null; //SON ADD) (2019.08.29) - 29 August 2019

var IOTTYPE = {
    Assembly: "SA",
    FinalAssembly: "FA",
    Qa: "QA"
};

var MATERIALTYPE = {
    Normal: "001",
    HeatSensitive: "002"
};

var PAINTINGTYE = {
    Promoter: "001",
    Primer: "002",
    Paint: "003"
};

//end Ha
//#endregion

//START ADD) SON (2019.09.14) - 14 September 2019
function InitModalProcessPartialView() {
    //Event paiting detail
    EventClickPaitingButton();
}
//END ADD) SON (2019.09.14) - 14 September 2019

// #region Get data

//Get process name
function GetOpName2(languageId) {
    var config = ObjectConfigAjaxPost("/Ops/GetOpName", true, JSON.stringify({ languageId: languageId }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropdownOpName(respone);
    });
}

//Get tool category
function GetMasterCodeTool(isMachine) {
    var config = ObjectConfigAjaxPost("/Ops/GetCategorysMachineTool", true, JSON.stringify({ isMachine: isMachine }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropdownCategory(respone, "drpCategoryTool", "1");
    });
}

//Get machine category
function GetMasterCodeMachine(isMachine) {
    var config = ObjectConfigAjaxPost("/Ops/GetCategorysMachineTool", true, JSON.stringify({ isMachine: isMachine }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropdownCategory(respone, "drpCategoryMachine", "0");
        //Load default machine
        var arrCategory = ["CSW", "SEW"];
        $('#drpCategoryMachine').multiselect('select', arrCategory, true);
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
function GetMasterCodeOpType(OpType) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: OpType }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpJobCode", respone, "SubCode", "CodeName");
    });
}

//Get job Action Code and Factory VitHV
function GetActionCodeOpType(OpType) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: OpType }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpActionCode", respone, "SubCode", "CodeName");
    });
}

function GetFactory() {
    var config = ObjectConfigAjaxPost("/Ops/GetFactory", true);
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpFactory", respone, "FactoryId", "FactoryName");
    });
}

//Get module
function GetModulesByStyleCode(styleCode) {
    var config = ObjectConfigAjaxPost("/Ops/GetModulesListByStyleCode", true, JSON.stringify({ styleCode: styleCode }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpModule", respone, "ModuleId", "ModuleName");
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
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
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
    AjaxPostCommon(config, function (respone) {
        callBack(respone);
    });
}

//START ADD) SON (2019.08.29) - 29 August 2019
function GetPaitingTimeRange(paintingType, materialType, callBack) {
    var config = ObjectConfigAjaxPost("/Ops/GetPaitingTimeRange", true, JSON.stringify({ paintingType: paintingType, materialType: materialType }));
    AjaxPostCommon(config, function (resList) {
        callBack(resList);
    });
}
//END ADD) SON (2019.08.29) - 29 August 2019

//start Ha

//Get Template
function GetTempName(actionCode) {
    var config = ObjectConfigAjaxPost("/Ops/GetTempName", true, JSON.stringify({ actionCode: actionCode }));
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
        var actioncode = $("#drpActionCode").val();

        GetTempName(actioncode);

        $("#drpTemplate").change(function () {
            var actioncode = $("#drpActionCode").val();

            var tempid = $("#drpTemplate").val();

            var opMaster = GetSelectedOneRowData(gridOpsTableId);
            var language = opMaster.Language;

            var data = { opActionCode: actioncode, opTempId: tempid, opLanguage: language };

            ReloadJqGrid2LoCal("tbProcessTemplate", data);
        });
    });
}

//START ADD) SON - 2019.03.1.0 - 11/Mar/2019 - event click Detail button of process detail
function clickDetailButtonOfProcess(rowid) {
    //Edit
    if (StatusUpdateProcess == 1) {

        ClearDataProcessDetailOpName();

        var listObjDetail = jQuery("#tbOpTime").getGridParam('data');
        var objDetail = jQuery("#tbOpTime").jqGrid('getRowData', rowid);

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
        $('#txtStitchCountSubDetail').val(objDetail.StitchCount);//ADD) SON - 25 December 2019
    }
    else { //Add new

        //ListObjOpnt = jQuery("#tbOpTime").getGridParam('data');

        SelectedObjOpnt = jQuery("#tbOpTime").jqGrid('getRowData', rowid);

        ShowModal(ProcessTemplateDetail);

        ClearDataProcessDetailOpName();
    }
}
//END ADD) SON - 2019.03.1.0 - 11/Mar/2019

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

            ListObjOpnt = jQuery("#tbOpTime").getGridParam('data');

            var obj = SelectedObjOpnt;

            obj.MachineType = $("#selectMachineTypeSubDetail").val();
            obj.MachineCount = $("#txtMachineCountSubDetail").val();
            obj.Remarks = $("#txtRemarksSubDetail").val();
            obj.MaxTime = $("#txtMaxTimeSubDetail").val();
            obj.ManCount = $("#txtManCountSubDetail").val();
            obj.JobType = $("#drpJobTypeSubDetail").val();
            obj.ToolId = $('#selectToolIdSubDetail').val();
            obj.ActionCode = $('#drpActionCodeSubDetail').val();
            obj.StitchCount = $('#txtStitchCountSubDetail').val();//ADD) SON - 25 December 2019

            HideModal(ProcessTemplateDetail);

            let gridData = jQuery("#tbOpTime").getGridParam('data');

            for (var i = 0; i < gridData.length; i++) {

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
                    objProcess.StitchCount = obj.StitchCount;//ADD) SON - 25 December 2019
                    
                    break;
                }
            }
            ReloadJqGridLocal("#tbOpTime", gridData);

        } else { //Add new
            var obj = SelectedObjOpnt;

            obj.MachineType = $("#selectMachineTypeSubDetail").val();
            obj.MachineCount = $("#txtMachineCountSubDetail").val();
            obj.Remarks = $("#txtRemarksSubDetail").val();
            obj.MaxTime = $("#txtMaxTimeSubDetail").val();
            obj.ManCount = $("#txtManCountSubDetail").val();
            obj.JobType = $("#drpJobTypeSubDetail").val();
            obj.ToolId = $('#selectToolIdSubDetail').val();
            obj.ActionCode = $('#drpActionCodeSubDetail').val();
            obj.StitchCount = $('#txtStitchCountSubDetail').val();//ADD) SON - 25 December 2019

            HideModal(ProcessTemplateDetail);

            let gridData = jQuery("#tbOpTime").getGridParam('data');

            for (var i = 0; i < gridData.length; i++) {

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
                    objProcess.StitchCount = obj.StitchCount;//ADD) SON - 25 December 2019
                  
                    break;
                }
            }
            ReloadJqGridLocal("#tbOpTime", gridData);
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
            { name: 'OpName', index: 'OpName', label: "Template Name", width: 700, formatter: getTemplateName }
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
        autowidth: false
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
        //rownumbers: true,
        //rownumWidth: 100,
        width: null,
        shrinkToFit: false,
        rowNum: 10000,
        colModel: [
            //{//start Ha
            //    name: 'OpMain', index: 'OpMain', label: 'OpMain', width: 70, hidden: true,
            //    formatter: function radio(cellValue, option) {
            //        return '<input type="radio" id="rdIsMain_' + option.rowId + '" name="radio_' + option.gid + '" value=' +
            //               cellValue + '/>';
            //    }
            //}, //end Ha
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
            //start Ha
            {
                name: '', index: 'Detail', width: 70, sortable: false, hidden: false,
                formatter: function (cellvalue, option, rowObject) {
                    return "<button type='button' class='btn btn-info btn-modal' style='height:30px;width:60px;' onclick='ButtonClickProcessDetail(" + option.rowId + ")' > Detail </button>";
                }
            },
            //end Ha
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSeiral', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            //start Ha
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
            //end Ha
        ],
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
        }
    });

    for (var i = 0; i <= opNameData.length; i++)
        jQuery(TableInputOpNameId).jqGrid('addRowData', i + 1, opNameData[i]);

    function addTextbox(cellvalue, options, rowObject) {
        return "<input type='text' id='txtOpTimeMdl' class='form-control' maxlength='3' onkeypress='return isNumber(event)' >";
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
            { name: 'OpTime', index: 'OpTime', width: 130, label: arrColNameOpsDetail.OPTIME, align: 'center' },
            { name: 'Factory', index: 'Factory', width: 90, label: arrColNameOpsDetail.FACTORY, align: 'center', hidden: true },
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
            { name: 'HotSpot', index: 'HotSpot', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);

        },
        onSelectRow: function (rowid) {


        },
        loadComplete: function () {

        },
        ondblClickRow: function (rowid, abc) {

        },
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
        //shrinkToFit: true,
        //autowidth: true,
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
            ShowMessage("Get process name template", err.message, MessageTypeError);
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
    for (var i = 0; i < arrOpName.length; i++) {
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

            var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
            var opSerial = $("#txtProcessNo").val();

            if (ArrayListIsNull(selectedVal)) { $("#hdOpNameTex").val(strText); return; }

            var opnSerial = 1;
            for (var i = 0; i < selectedVal.length; i++) {
                var val = selectedVal[i];
                var txt = $("#drpOpName option[value='" + val + "']").text();
                if (i === 0) {
                    strText = ConvertString(txt);
                } else {
                    strText += " | " + txt;
                }
                var objOpName = {
                    StyleCode: objOpsMaster.StyleCode
                    , StyleSize: objOpsMaster.StyleSize
                    , StyleColorSerial: objOpsMaster.StyleColorSerial
                    , RevNo: objOpsMaster.RevNo
                    , OpRevNo: objOpsMaster.OpRevNo
                    , OpSerial: opSerial
                    , Edition: objOpsMaster.Edition
                    , OpNameId: val
                    , OpName: txt
                    , OpTime: ''
                    , OpnSerial: opnSerial
                };
                opnSerial++;
                ArrSelectedOpname.push(objOpName);
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
            }

            //drpMachineDefault
            FillDataToDropDownlist(idDropdownlistToolMain, arrMachine, "ItemCode", "ItemName");
        }
    });
}

function FillDataToDropdownCategory(arrDataSource, idDropdownlistCategory, isTool) {

    $('#' + idDropdownlistCategory).multiselect('destroy');
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
            }
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
    var url = StatusUpdateProcess === 1 ? "/Ops/GetOpMachineMaster2" : "/Ops/GetOpMachineMaster";

    var config = ObjectConfigAjaxPost(url, true, JSON.stringify({ isTool: isTool, lstCategoryId: lstCategoryId }));
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
                    q: params.term // search term
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
            }
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
                    q: params.term // search term
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
            }
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

function InitDataUpdateProcess(objOpsDetail) {
    //Set status update process is 1.
    StatusUpdateProcess = 1;

    ClearDataAddNewProccess();

    ShowModal(ProcessModal);

    //Get menu id by edition.
    var currentOpmt = GetSelectedOneRowData(gridOpsTableId);

    InitDataForProcessModal();
    setTimeout(function () {
        //objOpsDetail.LanguageId = currentOpmt.Language;
        LoadObjectOpDetailModal(objOpsDetail);
    }, 1000);

    var menuId = "";
    if (currentOpmt.Edition === editionOps || currentOpmt.Edition == editionPdm)
        menuId = MenuIdOpm;
    else if (currentOpmt.Edition === editionAom)
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
    //Set status update process is 0.
    StatusUpdateProcess = 0;

    ShowModal(ProcessModal);

    ClearDataAddNewProccess();

    //$(TableInputOpNameId).remove();

    GetMaxOpSerial();

    InitDataForProcessModal();

    $('#btnUpdateProcess').hide();

    $('#btnSaveProcess').show();
}

//START ADD) SON - 07/Mar/2019 - 2019.03.1.0
/**
 * Init master data for process modal
 */
function initMasterDataProcessModal() {
    //Load operation pland detail follow operation plan master from local Storage
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

    //Get list of category tool
    GetMasterCodeTool("0");

    //Get machine category
    GetMasterCodeMachine("1");

    //Get process group
    GetMasterCodeOpGroup(OpGroup);

    //Get Job code
    GetMasterCodeOpType(OpType);

    //Get ActionCode VitHV and Fact
    GetActionCodeOpType("ActionCode");
    GetFactory();

    //Get module 
    //GetModulesByStyleCode(objOpsMaster.StyleCode);

    var arrCategory = ["CSW", "SEW"];

    //Get list of tool and machine base on list of category
    GetToolDataAndFillToDropdownList("0", arrCategory);
    GetToolDataAndFillToDropdownList("1", arrCategory);

    //ADD) SON - 2019.03.1.0 - 11/Mar/2019 - init gridview process detail
    JqGridSelectedProcess("");
    jqGridProcessName(MapLanguageToFlag(objOpsMaster.Language));
    //Get all process name detail
    //LoadAllProcessName(); // Ha add (7 March 2019)

}
//END ADD) SON - 07/Mar/2019 - 2019.03.1.0

//Init data for add new process modal
function InitDataForProcessModal() {

    //START MOD) SON - 07/Mar/2019 - ver: 2019.03.1.0  
    //Load operation pland detail follow operation plan master from local Storage
    //var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

    ////Get list of category tool
    //GetMasterCodeTool("0");

    ////Get machine category
    //GetMasterCodeMachine("1");

    ////Get process group
    //GetMasterCodeOpGroup(OpGroup);

    ////Get Job code
    //GetMasterCodeOpType(OpType);

    ////Get ActionCode VitHV and Fact
    //GetActionCodeOpType("ActionCode");
    //GetFactory();

    ////Get module 
    //GetModulesByStyleCode(objOpsMaster.StyleCode);

    //var arrCategory = ["CSW", "SEW"];

    ////Get list of tool and machine base on list of category
    //GetToolDataAndFillToDropdownList("0", arrCategory);
    //GetToolDataAndFillToDropdownList("1", arrCategory);

    //if (StatusUpdateProcess === 1) {       

    //    $("#btnRemoveVideo").prop("disabled", false);

    //    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    //    //Load process name.
    //    var opsLanId = MapLanguageToFlag(objOpsMaster.Language);
    //    GetOpName2(opsLanId);

    //} else {
    //    MultipleSelect('drpOpName');
    //    $("#btnRemoveVideo").prop("disabled", true);
    //}
    //END MOD) SON - 07/Mar/2019 - 2019.03.1.0

    if (StatusUpdateProcess === 1) {
        $("#btnRemoveVideo").prop("disabled", false);
        $("#btnEnterProcess").prop("disabled", false);
    } else if (StatusUpdateProcess === 0) {       
        $("#btnRemoveVideo").prop("disabled", true);
        $("#btnEnterProcess").prop("disabled", false);
    } else {
        //StatusUpdateProcess = 3: view mode
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

function LayoutLoadListProcessClone(callBack) {
    ShowModal("mdlCloneProcess");

    var opMaster = GetSelectedOneRowData(gridOpsTableId);
    BindDataToJqGridCloneProcess("", "", "", "", "", "", "");

    var postData = {
        styleCode: opMaster.StyleCode,
        styleSize: opMaster.StyleSize,
        styleColor: opMaster.StyleColorSerial,
        revNo: opMaster.RevNo,
        opRevNo: opMaster.OpRevNo,
        edition: opMaster.Edition,
        languageId: MapLanguageToFlag(opMaster.Language)
    };
    ReloadJqGrid(TableProcessCloneName, postData);

    CloneProcess(callBack);

}

function LayoutSaveEvent(callBack) {
    IsLayoutEvent = "1";

    InitDataAddNewProcess();

    $("#btnSaveProcess").unbind().click(function () {
        SaveNewProcess(null, callBack);
    });
}

function LayoutUpdateEvent(objOpdt, callBack) {
    IsLayoutEvent = "1";

    var opMaster = GetSelectedOneRowData(gridOpsTableId);

    objOpdt.StyleCode = opMaster.StyleCode;
    objOpdt.StyleSize = opMaster.StyleSize;
    objOpdt.StyleColorSerial = opMaster.StyleColorSerial;
    objOpdt.RevNo = opMaster.RevNo;
    objOpdt.OpRevNo = opMaster.OpRevNo;
    objOpdt.OpSerial = objOpdt.id;
    objOpdt.Edition = opMaster.Edition;

    window.DisplayColor = objOpdt.DisplayColor;

    var objOpsDetail = GetObjectOpsDetail(objOpdt);
    objOpsDetail.LanguageId = opMaster.Language;

    InitDataUpdateProcess(objOpsDetail);

    //.unbind advoid clicking event firing multiples times.
    $("#btnUpdateProcess").unbind().click(function () {
        UpdateProcess(null, callBack);
    });
}

//Add new process to database
function SaveNewProcess(callBackFunc, oploCallBack) {

    if (CheckDataAddNewProccess()) {

        //Get data from local storage.
        var opMaster = GetSelectedOneRowData(gridOpsTableId);

        if ($.isEmptyObject(opMaster)) {
            ShowMessageOk("004", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

            return;
        }

        var objOpDetail = CreateObjectOpDetail(opMaster);
        
        $.blockUI(ObjectBlockUICss);
        setTimeout(function () {
            var statusUploadImg = UploadImageProcess(opMaster);
            var statusUploadVideo = UploadVideoProcess(opMaster);

            if (statusUploadImg !== Fail && statusUploadVideo !== Fail) {

                var lstMachine = CreateObjectMachine();
                var lstTool = CreateObjectTool();
                //Set image name and video name.
                if (statusUploadImg !== Success) objOpDetail.ImageName = statusUploadImg;
                if (statusUploadVideo !== Success) objOpDetail.VideoFile = statusUploadVideo;

                //start Ha
                //if ($.isEmptyObject($("#drpTemplate").val())) {
                //    if ($("#drpOpName").val().length === 1) {
                //        ArrSelectedOpname[0].OpTime = $("#txtProcessTime").val();
                //    }
                //}

                //if ($("#drpOpName").val().length === 1) {
                //    ArrSelectedOpname[0].OpTime = $("#txtProcessTime").val();
                //}
                //end Ha

                //START ADD) SON - 2019.03.1.1 - 08/Mar/2019 
                //Get process detail data
                var gridPro = $("#tbOpTime").getGridParam('data');

                //Check length of grid process detail
                if (gridPro.length <= 0) {
                    //unblock UI
                    $.unblockUI();
                    ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
                    return;
                }

                for (var i = 0; i < gridPro.length; i++) {
                    //Get operation time in textbox  
                    var opTime = $("#txtOpntTime_" + (i + 1)).val();
                    //Set operation time again.
                    gridPro[i].OpTime = opTime;

                }

                var blAddPro = AddNewProcess(objOpDetail, lstMachine, lstTool, gridPro);
                //END ADD) SON - 2019.03.1.1 - 08/Mar/2019

                //var blAddPro = AddNewProcess(objOpDetail, lstMachine, lstTool, ArrSelectedOpname);
                if (blAddPro === true) {
                    if (callBackFunc === null) {
                        HideModal(ProcessModal);
                        objOpDetail.X = LayoutLeftX;
                        objOpDetail.Y = LayoutTopY;
                        var node = CreateObjectForLayout(opMaster, objOpDetail);
                        // map objOpDetail to node
                        oploCallBack(node);

                        //Reload operation master gird
                        opMaster.Edition = $("#drpOpsMasterEdition").val();
                        ReloadJqGrid2LoCal(gridOpsTableName, opMaster);

                    } else {
                        callBackFunc(blAddPro);
                    }

                    //Clear grid by null data
                    ReloadJqGridLocal(TableInputOpNameName, []);

                }
            } else {
                ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error);
            }

            //unblock UI
            $.unblockUI();
        }, 100);
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
    node.MachineType = objOpdt.MachineType;
    node.MachineName = machineName;
    node.ManCount = objOpdt.ManCount;
    node.OpNum = objOpdt.OpNum;
    node.OpName = objOpdt.OpName;
    let remarks = "";
    if (objOpdt.Remarks) remarks = objOpdt.Remarks;
    node.Remarks = remarks;
    node.OpGroup = objOpdt.OpGroup;
    node.left = objOpdt.X;
    node.top = objOpdt.Y;
    node.DisplayColor = objOpdt.DisplayColor === null || objOpdt.DisplayColor === "" ? "#FFFFFF" : `#${objOpdt.DisplayColor.substring(3, 8)}`;
    node.ProcessWidth = objOpMaster.ProcessWidth === "" ? defaultProcessWidth : objOpMaster.ProcessWidth;
    node.ProcessHeight = objOpMaster.ProcessHeight === "" ? defaultProcessHeight : objOpMaster.ProcessHeight;
    node.LayoutFontSize = objOpMaster.LayoutFontSize === "0" ? defaultFontSize : objOpMaster.LayoutFontSize;
    node.Page = objOpdt.Page === 0 ? 1 : objOpdt.Page;
    node.IsDisplay = true;
    node.CanDelete = window.CanSave;
    node.ShowButtonPlayVideo = $.isEmptyObject(objOpdt.VideoFile) ? 0 : 1;

    return node;
}

function UpdateProcess(callBackFunc, oploCallBack) {

    //Check data before update
    if (!CheckDataAddNewProccess()) return;

    //Get data from local storage.
    var opMaster = GetSelectedOneRowData(gridOpsTableId);
    $.blockUI(ObjectBlockUICss);
    setTimeout(function () {

        var statusUploadImg = UploadImageProcess(opMaster);
        var statusUploadVideo = UploadVideoProcess(opMaster);
        var objOpDetail = CreateObjectOpDetail(opMaster);

        if (statusUploadImg !== Fail && statusUploadVideo !== Fail) {
            //Set old name of file
            objOpDetail.ImageName = $("#hdImageName").val();
            objOpDetail.VideoFile = $("#hdVideoName").val();

            //Set image name and video name.
            if (statusUploadImg !== Success) objOpDetail.ImageName = statusUploadImg;
            if (statusUploadVideo !== Success) objOpDetail.VideoFile = statusUploadVideo;

            var lstMachine = CreateObjectMachine();
            var lstTool = CreateObjectTool();

            //if ($("#drpOpName").val() !== null && $("#drpOpName").val().length === 1) {
            //    ArrSelectedOpname[0].OpTime = $("#txtProcessTime").val();
            //}

            //START ADD) SON - 2019.03.1.0 - 09/Mar/2019
            //Get process name detail from gridview
            var gridProDt = $("#tbOpTime").getGridParam('data');

            //Check length of grid process detail
            if (gridProDt.length <= 0) {
                //unblock UI
                $.unblockUI();
                //Color button enter process
                ColorButtonBorder("btnEnterProcess", "error-border");
                //Show message inform user enter process
                ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
                return;
            }

            for (var i = 0; i < gridProDt.length; i++) {
                //Get operation time in textbox  
                var opTime = $("#txtOpntTime_" + (i + 1)).val();
                //Set operation time again.
                gridProDt[i].OpTime = opTime;

            }

            var blUpdate = UpdateOpDetail(objOpDetail, lstMachine, lstTool, gridProDt);

            //START ADD) SON - 2019.03.1.0 - 09/Mar/2019

            //var blUpdate = UpdateOpDetail(objOpDetail, lstMachine, lstTool, ArrSelectedOpname);
            if (blUpdate === false) {
                //callBackFunc(blUpdate, objOpDetail);
            } else {
                if (callBackFunc === null) {

                    HideModal(ProcessModal);
                    objOpDetail.X = LayoutLeftX;
                    objOpDetail.Y = LayoutTopY;
                    var node = CreateObjectForLayout(opMaster, objOpDetail);
                    // map objOpDetail to node
                    oploCallBack(node);

                    //Reload operation master gird
                    opMaster.Edition = $("#drpOpsMasterEdition").val();
                    //ReloadJqGrid(gridOpsTableName, opMaster);
                    ReloadJqGrid2LoCal(gridOpsTableName, opMaster);

                } else {
                    callBackFunc(blUpdate);
                }
            }
        } else {
            ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error);
        }
        //unblock UI
        $.unblockUI();
    }, 100);
}

//Create object tool linking
function CreateObjectMachine() {
    var lstMachine = [];
    var arrMachine = $("#drpMachine").val();
    var selMainMachine = $("#drpMachineDefault").val();

    //Get ops master from loacal storage.
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

    if ($.isEmptyObject(objOpsMaster) || $.isEmptyObject(arrMachine)) {
        return null;
    }
    arrMachine.forEach(function (machineCode) {
        var mainMachine = machineCode == selMainMachine ? "1" : "0";
        var objMachine = {
            StyleCode: objOpsMaster.StyleCode,
            StyleColorSerial: objOpsMaster.StyleColorSerial,
            StyleSize: objOpsMaster.StyleSize,
            RevNo: objOpsMaster.RevNo,
            OpRevNo: objOpsMaster.OpRevNo,
            OpSerial: $("#txtProcessNo").val(),
            ItemCode: machineCode,
            Machine: "1",
            MainTool: mainMachine,
            Edition: objOpsMaster.Edition
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

    //Get ops master from loacal storage.
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

    if ($.isEmptyObject(objOpsMaster) || $.isEmptyObject(arrTool)) {
        return null;
    }

    arrTool.forEach(function (toolCode) {
        var mainTool = toolCode == selMainTool ? "1" : "0";
        var objMachine = {
            StyleCode: objOpsMaster.StyleCode,
            StyleColorSerial: objOpsMaster.StyleColorSerial,
            StyleSize: objOpsMaster.StyleSize,
            RevNo: objOpsMaster.RevNo,
            OpRevNo: objOpsMaster.OpRevNo,
            OpSerial: $("#txtProcessNo").val(),
            ItemCode: toolCode,
            Machine: "0",
            MainTool: mainTool,
            Edition: objOpsMaster.Edition
        };

        lstTool.push(objMachine);
    });

    return lstTool;
}

//Clear data on "Add New Process" modal
function ClearDataAddNewProccess() {
    var LayoutPage = null;
    var DisplayColor = null;
    var LayoutTopY = null;
    var LayoutLeftX = null;
    //Clear temporary selected tool and machine.
    ArrSelectedTool = [];
    ArrSelectedMachine = [];
    ArrSelectedOpname = [];
    CompareProcessName = false;
    //ListOldProcessName = [];

    $("#hdOpNameTex").val("");
    $("#drpTemplate").val("").trigger('change'); //Ha add

    $("#txtProcessNo").val("");
    $("#txtProcessNumber").val("");
    //GetOpName2("");
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
    $("#txtRemarks").val("");

    $("#chkHotSpot").prop("checked", false);
    $("#chkOutsourcing").prop("checked", false);

    $("#rdAssembly").prop("checked", false);
    $("#rdFinalAssembly").prop("checked", false);
    $("#rdQa").prop("checked", false);
    $("#rdNormal").prop("checked", false);

    RemovePreviewVideo();
    RemovePreviewImage();

    StyleValidateAddNewProcess();

    $("#drpModule").prop("disabled", false);
    //$("#drpProcessGroup").prop("disabled", false);
    $("#drpMachineDefault").prop("disabled", false);
    $("#drpModule").prop("disabled", false);


    //START ADD) SON - 2019.03.1.0 - 08/Mar/2019 

    $("#drpJobCode").val(null).trigger("change");
    $("#drpActionCode").val(null).trigger("change");
    $("#drpModule").val(null).trigger("change");
    $("#drpFactory").val(null).trigger("change");
    $("#drpProcessGroup").val(null).trigger("change");

    $("#txtCostingGroup").val("");
    $("#txtOfferPrice").val("");
    $("#txtProcessCost").val("");

    //Reload grid input process with no data
    ReloadJqGridLocal("tbOpTime", []);

    //Set textbox process name is empty
    $("#txtProcessName").val("");
    //END ADD) SON - 2019.03.1.0 - 08/Mar/2019

    //START ADD) SON (2019.08.29) - 30 August 2019 - Clear paiting detail data on modal
    ClearDataOnPaitingDetailModal();
    //END ADD) SON (2019.08.29) - 30 August 2019
}

function StyleValidateAddNewProcess() {
    //Remove class normal-error
    //RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpProcessGroup", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpJobCode", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpMachineDefault", "error-border");
    RemoveClass("txtMachineCount", "error-border");
    RemoveClass("txtWorker", "error-border");
    RemoveClass("txtMaxTime", "error-border");
    RemoveClass("txtProcessTime", "error-border");
    //RemoveClass("btnOpTime", "error-border");
    RemoveClass("btnEnterProcess", "error-border");
}

function CheckProcessNameAndOpTime() {
    var check = true;

    var valOpName = $("#drpOpName").val();

    //start Ha
    var valTemplate = $("#drpTemplate").val();

    if ($.isEmptyObject(valOpName) && isEmpty($("#hdOpNameTex").val()) && $.isEmptyObject(valTemplate)) {
        ColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
        check = false;
    }
    else if (!$.isEmptyObject(valTemplate)) {
        RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
        var dataRows = GetAllRowsDataJqGrid(TableInputOpNameId);

        if ($.isEmptyObject(dataRows)) {
            ColorButtonBorder("btnOpTime", "error-border");

            check = false;
        }
        else {
            for (var i = 0; i < dataRows.length; i++) {
                var opTime = $("#txtOpTime_" + (i + 1)).val();
                if (isEmptyOrWhiteSpace(opTime)) {
                    ColorButtonBorder("btnOpTime", "error-border");

                    check = false;
                }
            }
        }
    }
    else {
        RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");

        if (valOpName !== null && valOpName.length > 1) {
            //Check list optime
            var dataRows = GetAllRowsDataJqGrid(TableInputOpNameId);
            if ($.isEmptyObject(dataRows) && !$.isEmptyObject($("#drpOpName").val())) {
                ColorButtonBorder("btnOpTime", "error-border");

                check = false;
            } else {
                for (var i = 0; i < dataRows.length; i++) {
                    var opTime = $("#txtOpTime_" + (i + 1)).val();
                    if (isEmptyOrWhiteSpace(opTime)) {
                        ColorButtonBorder("btnOpTime", "error-border");

                        check = false;
                    }
                }
            }
        }

        if (check) RemoveClass("btnOpTime", "error-border");
    }
    //end Ha

    //Check selected process name and old process name (for old process).
    /*if ($.isEmptyObject(valOpName) && isEmpty($("#hdOpNameTex").val())) {

        //Add css to parent element
        ColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
        check = false;

    } else {
        RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");

        if (valOpName !== null && valOpName.length > 1) {
            //Check list optime
            var dataRows = GetAllRowsDataJqGrid(TableInputOpNameId);
            if ($.isEmptyObject(dataRows) && !$.isEmptyObject($("#drpOpName").val())) {
                ColorButtonBorder("btnOpTime", "error-border");

                check = false;
            } else {
                for (var i = 0; i < dataRows.length; i++) {
                    var opTime = $("#txtOpTime_" + (i + 1)).val();
                    if (isEmptyOrWhiteSpace(opTime)) {
                        ColorButtonBorder("btnOpTime", "error-border");

                        check = false;
                    }
                }
            }
        }

        if (check) RemoveClass("btnOpTime", "error-border");
    }*/

    return check;
}

//Check data before add new process
function CheckDataAddNewProccess() {

    var check = true;

    if (isEmpty($("#hdUsername").val())) {
        ShowMessageOk("011", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

        return false;
    }

    //Get style master from local storage
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    if ($.isEmptyObject(objOpsMaster)) {
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

    //START MOD) SON - 2019.03.1.0 - 09/Mar/2019 

    //Check process name and process time.
    //check = CheckProcessNameAndOpTime();

    // do not check process name from dropdown list
    //if ($("#drpOpName").val() !== null) {
    //    if ($("#drpOpName").val().length > 10) {
    //        ColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
    //        ShowMessageOk("012", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
    //        return false;
    //    }
    //}

    //Checking process name
    if (isEmptyOrWhiteSpace($("#txtProcessName").val())) {
        ColorButtonBorder("btnEnterProcess", "error-border");
        check = false;
    } else {
        RemoveClass("btnOpTime", "error-border");
    }

    //START MOD) SON - 2019.03.1.0 - 09/Mar/2019

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

//Add new process
function AddNewProcess(objOpDetail, lstMachine, lstTool, lstOpnt) {
    var addStatus;
    $.ajax({
        url: "/Ops/AddNewProcess",
        async: false,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail, lstOpMachine: lstMachine, lstOpTool: lstTool, lstOpnt: lstOpnt }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                addStatus = true;
                ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add, ObjMessageType.Info);

            } else {
                addStatus = false;
                ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);

            }

        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, ObjMessageType.Error, errorThrown.message);

            addStatus = false;
        }
    });
    return addStatus;
}

function UpdateOpDetail(objOpDetail, lstMachine, lstTool, lstOpnt) {

    var addStatus;

    $.ajax({
        url: "/Ops/UpdateOpDetail",
        async: false,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail, lstMachine: lstMachine, lstTool: lstTool, lstOpnt: lstOpnt }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                addStatus = true;
                ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, ObjMessageType.Info);

            } else {
                addStatus = false;
                ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);

            }
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);
            addStatus = false;
        }
    });

    return addStatus;
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
function CreateObjectOpDetail(objOpsMaster) {

    if ($.isEmptyObject(objOpsMaster)) {
        return null;
    }

    var opGroup;
    var machineType;
    var machineName;
    var moduleId;

    opGroup = $("#drpProcessGroup").val();
    machineType = $("#drpMachineDefault").val();
    machineName = $("#drpMachineDefault option:selected").text();
    moduleId = $("#drpModule").val();

    if (IsLayoutEvent = "1") {
        switch (LayoutGroupMode) {
            case ModuleTypeConst:
                moduleId = $.isEmptyObject(moduleId) ? null : moduleId;
                break;
            case OpGroupConst:
                opGroup = $.isEmptyObject(opGroup) ? null : opGroup;
                break;
            case MachineTypeConst:
                machineType = $.isEmptyObject(machineType) ? null : machineType;
                break;
        }
    }

    let iotType = "";
    if ($("#rdAssembly").is(':checked')){
        iotType = "SA";
    } else if ($("#rdFinalAssembly").is(':checked')){
        iotType = "FA";
    } else if ($("#rdQa").is(':checked')){
        iotType = "QA";
    } else if ($("#rdNormal").is(':checked')){
        iotType = "";
    }

    //ADD) SON (2019.08.29) - 30 August 2019 -  Get paiting type and material type
    let objPtDt = GetMaterialAndPaitingType();

    var objOpDetail = {
        Edition: objOpsMaster.Edition,
        StyleCode: objOpsMaster.StyleCode,
        StyleSize: objOpsMaster.StyleSize,
        StyleColorSerial: objOpsMaster.StyleColorSerial,
        RevNo: objOpsMaster.RevNo,
        OpRevNo: objOpsMaster.OpRevNo,
        OpSerial: $("#txtProcessNo").val(),
        OpNum: $("#txtProcessNumber").val(),
        OpGroup: opGroup,
        OpName: $("#txtProcessName").val(), //MOD) SON - 2019.03.1.0 - 09/Mar/2019 - Get operation name from textbox ( $("#hdOpNameTex").val() ) 
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
        ActionCode: $("#drpActionCode").val(), // VITHV
        Factory: $("#drpFactory").val(),
        StitchCount: $("#txtStitchCount").val(), //HA ADD
        IotType: iotType,
        //START ADD) SON (2019.08.29) - 30 August 2019 - Get paiting detail
        PaintingType: objPtDt.PaintingType,
        MaterialType: objPtDt.MaterialType,
        DryingTime: $("#drpDryingTime").val(),
        Temperature: $("#drpTemperature").val(),
        CoolingTime: $("#drpCoolingTime").val()
        //END ADD) SON (2019.08.29)
    };

    return objOpDetail;
}

function LoadObjectOpDetailModal(objOpDetail) {

    $("#hdStyleCode").val(objOpDetail.StyleCode);
    $("#hdStyleColor").val(objOpDetail.StyleColorSerial);
    $("#hdStyleSize").val(objOpDetail.StyleSize);
    $("#hdStyleRevNo").val(objOpDetail.RevNo);
    $("#hdOpRevNo").val(objOpDetail.OpRevNo);
    $("#txtProcessNo").val(ZeroPad(objOpDetail.OpSerial, 3));
    $("#txtProcessNumber").val(objOpDetail.OpNum);
    $("#drpProcessGroup").val(objOpDetail.OpGroup).trigger("change");
    $("#txtProcessTime").val(objOpDetail.OpTime);
    $("#hdOpNameTex").val(objOpDetail.OpName);

    //ADD) SON - 2019.03.1.0 - 09/Mar/2019 - assign process name
    $("#txtProcessName").val(objOpDetail.OpName);

    //Get machine and tool
    var lstTool = GetListOpToolLinking(objOpDetail);

    //Get process name detail 
    //var arrProcessName = [];
    GetListProcessNameDetail(objOpDetail.Edition, objOpDetail.StyleCode, objOpDetail.StyleSize, objOpDetail.StyleColorSerial, objOpDetail.RevNo, objOpDetail.OpRevNo, objOpDetail.OpSerial, objOpDetail.LanguageId, function (lstOpnts) {

        //$.each(lstOpnts, function (index, value) {
        //    arrProcessName.push(String(value.OpNameId));
        //});
        //$('#drpOpName').multiselect('select', arrProcessName, true);

        //ListOldProcessName = lstOpnts;
        //ArrSelectedOpname = ListOldProcessName;

        //Reload OpNameInput gridview.
        //ReloadJqGridLocal(TableInputOpNameName, ListOldProcessName);

        //START ADD) SON - 2019.03.1.0 - 08/Mar/2019
        //Reload OpNameInput gridview.
        ReloadJqGridLocal("tbOpTime", lstOpnts);
        //END ADD) SON - 2019.03.1.0 - 08/Mar/2019

    });

    var arrCategory = [];
    var arrTool = [];
    var arrMc = [];

    var arrSelectedTool = [];
    var arrSelectedMachine = [];

    $.each(lstTool, function (index, value) {
        //Get tool and machine
        if (value.Machine === "0") {
            var objTool = { ItemCode: value.ItemCode, ItemName: value.ItemName };
            arrTool.push(objTool);
            arrSelectedTool.push(value.ItemCode);
        } else {
            var objMachine = { ItemCode: value.ItemCode, ItemName: value.ItemName };
            arrMc.push(objMachine);
            arrSelectedMachine.push(value.ItemCode);
        }

        //Get list category
        //START MOD) SON (2019.08.30) - 30 August 2019
        //if (!IsInArray(value.CategId, arrCategory)) {
        //    arrCategory.push(value.CategId.trim());
        //}

        //Check category
        if (value.CategId !== null) {
            if (!IsInArray(value.CategId, arrCategory)) {
                arrCategory.push(value.CategId.trim());
            }
        }       
        //END MOD) SON (2019.08.30) - 30 August 2019
    });

    //Add machine type to selected array - old machine
    if ($.isEmptyObject(lstTool) && !$.isEmptyObject(objOpDetail.MachineType)) {
        var objOldMachine = { ItemCode: objOpDetail.MachineType, ItemName: objOpDetail.MachineName };
        arrMc.push(objOldMachine);
        arrSelectedMachine.push(objOpDetail.MachineType);
    }

    $("#drpJobCode").val(objOpDetail.JobType).trigger("change");
    $("#drpActionCode").val(objOpDetail.ActionCode).trigger("change");
    $("#drpFactory").val(objOpDetail.Factory).trigger("change"); //VITHV
    $("#drpModule").val(objOpDetail.ModuleId).trigger("change");

    $('#drpCategoryMachine').multiselect('select', arrCategory, true);
    $('#drpCategoryTool').multiselect('select', arrCategory, true);

    ArrSelectedTool = arrSelectedTool;
    ArrSelectedMachine = arrSelectedMachine;

    //LoadOpNameByGetTemplate(); //MOD - SON) 23/Oct/2020 - Don't use template

    //Get list of tool and machine base on list of category
    arrCategory = $.isEmptyObject(arrCategory) ? ["CSW", "SEW"] : arrCategory;
    GetToolDataAndFillToDropdownList("0", arrCategory);
    GetToolDataAndFillToDropdownList("1", arrCategory);

    FillDataToDropDownlist("drpMachineDefault", arrMc, "ItemCode", "ItemName");
    $("#drpMachineDefault").val(objOpDetail.MachineType).trigger("change");

    //Load main tool
    FillDataToDropDownlist("drpToolMain", arrTool, "ItemCode", "ItemName");
    $("#drpToolMain").val(objOpDetail.ToolId).trigger("change");

    $("#txtOfferPrice").val(objOpDetail.OfferOpPrice);
    $("#txtMachineCount").val(objOpDetail.MachineCount);
    $("#txtRemarks").val(objOpDetail.Remarks);
    $("#txtMaxTime").val(objOpDetail.MaxTime);
    $("#txtWorker").val(objOpDetail.ManCount);
    $("#txtStitchCount").val(objOpDetail.StitchCount); // HA ADD

    $("#txtBenchmarkTime").val(objOpDetail.BenchmarkTime);
    $("#txtProcessCost").val(objOpDetail.OpPrice);
    $("#chkHotSpot").prop('checked', StringToBoolean(objOpDetail.HotSpot));
    $("#chkOutsourcing").prop('checked', StringToBoolean(objOpDetail.OpsState));

    let iotType = objOpDetail.IotType;
    let assembly = false;
    let finalAssembly = false;
    let qa = false;
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
    $("#hdImageName").val(objOpDetail.ImageName);
    $("#hdVideoName").val(objOpDetail.VideoFile);

    $("#txtRemarks").val(objOpDetail.Remarks);

    var imageName = objOpDetail.ImageName;
    var imgPath = "/img/no-image.png";
    if (!isEmpty(imageName)) {
        imgPath = objOpDetail.ImageLink;
    }
    $("#imgPreview").attr("src", imgPath);

    //Load video
    var videoFilename = objOpDetail.VideoFile;
    var posterPath = "/img/no-video.png";
    let srtSrc = "";
    if (!isEmpty(videoFilename)) {
        srtSrc = objOpDetail.VideoOpLink;
        //fol = "";        
        //var srtSrc = document.location.origin + "/api/MediaPlay/Play?fol=" + fol + "" + "&f=" + videoFilename;
        posterPath = "";
    }
    $("#vidPreview").attr("src", srtSrc);
    $("#vidPreview").attr("poster", posterPath);

    //START ADD) SON (2019.08.29) - 31 August 2019 - load data painting detail on modal
    LoadPaintingDetailDataOnModal(objOpDetail);
    //END ADD) SON (2019.08.29) - 31 August 2019
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

//START ADD) SON (2019.08.29) - 29 August 2019
function ClearDataOnPaitingDetailModal() {
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

//Select radio button of material type and painting type
function SelectRadioMaterialAndPaitingType(matType, paiType) {
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

//Get data from object operation detail and load it on modal
function LoadPaintingDetailDataOnModal(objOpDetail) {
    let matType = objOpDetail.MaterialType;
    let paiType = objOpDetail.PaintingType;

    //If material type and painting type is null then return, do not load data.
    if (isEmpty(matType) || isEmpty(paiType)) return;

    //Select radio button of material type and painting type.
    SelectRadioMaterialAndPaitingType(matType, paiType);

    //Get paiting type range of termperature, drying time and cooling time.
    GetPaitingTimeRange(null, null, function (resList) {
        ListPaintingTime = resList;

        //Get painting time range include temperature, drying time and cooling time.
        GetPaitingTimeRangeBaseOnSelection();

        //Select 
        //Temperature time
        $("#drpTemperature").val(objOpDetail.Temperature).trigger('change');

        //Drying time
        $("#drpDryingTime").val(objOpDetail.DryingTime).trigger('change');

        //Cooling time
        $("#drpCoolingTime").val(objOpDetail.CoolingTime).trigger('change');
    });
}
//START ADD) SON (2019.08.29) - 31 August 2019

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

    //Check video type
    //var ext = this.value.match(/\.(.+)$/)[1].toLowerCase(); //get extention of file
    //switch (ext) {
    //    case 'mp4':
    //        break;
    //    default:
    //        alert('This is not an allowed file type.');
    //        this.value = '';
    //        return;
    //}

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
    /*
    $("#drpModule").change(function () {
        //No need load process name if event is update.
        if (StatusUpdateProcess === 1) return;

        var opMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));

        //Get opname by module.
        var langId = MapLanguageToFlag(opMaster.Language);
        var moduleId = $(this).val();
        var actionGroup = $("#drpJobCode").val();
        GetOpNameByCode(langId, moduleId, actionGroup);
    });
    */
    $("#drpModule, #drpActionCode").change(function () {

        //START MOD) SON - 2019.03.1.0 - 11/Mar/2019 - comment event change module
        //if (StatusUpdateProcess === 1) { return }; //Load all process name.
        ////Clear data process name
        //$("#hdOpNameTex").val("");

        //var opMaster = GetSelectedOneRowData(gridOpsTableId);
        //var langId = MapLanguageToFlag(opMaster.Language);
        //var moduleId = $("#drpModule").val();
        //var actionCode = $("#drpActionCode").val();
        //var config = ObjectConfigAjaxPost("/Ops/GetOpNameByCode", true, JSON.stringify({ languageId: langId, moduleId: moduleId, actionCode: actionCode, buyer: opMaster.Buyer }));
        //AjaxPostCommon(config, function (response) {
        //    FillDataToDropdownOpName(response);
        //});
        //END MOD) SON - 2019.03.1.0 - 11/Mar/2019
    });
}

function ClickButtonOpTime() {
    $("#btnOpTime").click(function () {
        //START MOD) SON - 2019.03.1.0 - 11/Mar/2019 - comment event click "Sub Process" button
        //setTimeout(function () {
        //    ShowModal(ModalInputOpTimeName);
        //    //Compare current process name array with process name in database.
        //    var processNameVal = [];
        //    $.each(ListOldProcessName, function (index, value) {
        //        processNameVal.push(String(value.OpNameId));
        //    });
        //    CompareProcessName = Compare2Arrays($('#drpOpName').val(), processNameVal);
        //    if (CompareProcessName) {
        //        //Reload data for load data the second time
        //        ReloadJqGridLocal(TableInputOpNameName, ListOldProcessName);
        //    } else {
        //        //Reload data for load data the second time
        //        ReloadJqGridLocal(TableInputOpNameName, ArrSelectedOpname);
        //    }

        //}, 100);
        //END MOD) SON - 2019.03.1.0 - 11/Mar/2019
    });
}

function ClickLoadAllProcessName() {
    $("#btnLoadAllProcessName").click(function () {
        var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
        //Load process name.
        var opsLanId = MapLanguageToFlag(objOpsMaster.Language);
        GetOpName2(opsLanId);
    });
}

function ClickButtonOk() {
    $("#btnGetOptime").click(function () {

        var gridData = $(TableInputOpNameId).getGridParam('data');
        var strOpNameId = "";
        var strOptime = "";
        var totalProcessTime = 0;
        //var IsMain = "0";
        for (var i = 0; i < gridData.length; i++) {
            var opTime = $("#txtOpTime_" + (i + 1)).val();

            //var isMain =  $("#rdIsMain_" + (i + 1)).is(":checked");
            //IsMain = ConvertBooleanToString01(isMain);

            if (isEmptyOrWhiteSpace(opTime)) {
                ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, "(Row " + (i + 1) + ")");
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

            var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
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
                    //START ADD) HA
                    , MachineType: ''
                    , MachineCount: ''
                    , Remarks: ''
                    , MaxTime: ''
                    , ManCount: ''
                    , JobType: ''
                    , ToolId: ''
                    , ActionCode: ''
                    //END ADD) HA
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

//START ADD) SON (2019.08.29) - 29 August 2019
function EventClickPaitingButton() {
    $("#btnShowPaiting").click(function () {

        if (ListPaintingTime === null) {
            //Get paiting type time range
            GetPaitingTimeRange(null, null, function (resList) {
                ListPaintingTime = resList;
            });
        }        
        
        ShowModal("mdlPaitingDetail");
    });

    $("#btnPaitingDtCancle, #btnClosePaitingDtMdl").click(function () {
        ShowConfirmYesNoMessage("001", SmsFunction.Generic, MessageType.Warning, MessageContext.IgnoreChanges, function () {
            //Clear data on paiting modal
            ClearDataOnPaitingDetailModal();

            //Hide paiting modal detail
            HideModal("mdlPaitingDetail");
        }, function () { }, "");       
    });

    $("#rdNormalMaterial, #rdHeatSenMaterial, #rdPromoter, #rdPrimer, #rdPaint").change(function () {
        GetPaitingTimeRangeBaseOnSelection();
    });
       
}

//Get material type and paiting type on modal paiting detail.
function GetMaterialAndPaitingType() {
    let matType = null;
    let paiType = null;

    if ($("#rdNormalMaterial").is(':checked')) {
        matType = MATERIALTYPE.Normal;
    }

    if ($("#rdHeatSenMaterial").is(':checked')) {
        matType = MATERIALTYPE.HeatSensitive;
    }

    if ($("#rdPromoter").is(':checked')) {
        paiType = PAINTINGTYE.Promoter;
    }

    if ($("#rdPrimer").is(':checked')) {
        paiType = PAINTINGTYE.Primer;
    }

    if ($("#rdPaint").is(':checked')) {
        paiType = PAINTINGTYE.Paint;
    }

    let obj = { MaterialType: matType, PaintingType: paiType };

    return obj;
    
}

function GetPaitingTimeRangeBaseOnSelection() {
    let matType = null;
    let paiType = null;

    //Get material type and painting type on modal paiting detail.
    let obj = GetMaterialAndPaitingType();

    matType = obj.MaterialType;
    paiType = obj.PaintingType;

    var objPt = $.grep(ListPaintingTime, function (pt) {
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

    let temperature = CreatePaitingRangeVaule(objPt[0].MinTemperature, objPt[0].MaxTemperature);
    let dryingTime = CreatePaitingRangeVaule(objPt[0].MinDryingTime, objPt[0].MaxDryingTime);
    let coolingTime = CreatePaitingRangeVaule(objPt[0].MinCoolingTime, objPt[0].MaxCoolingTime);

    $("#lblTemperatureRange").text(objPt[0].MinTemperature + " - " + objPt[0].MaxTemperature);
    $("#lblDryingTimeRange").text(objPt[0].MinDryingTime + " - " + objPt[0].MaxDryingTime);
    $("#lblCoolingTimeRange").text(objPt[0].MinCoolingTime + " - " + objPt[0].MaxCoolingTime);

    FillDataToDropDownlist("drpTemperature", temperature, "RangeValue", "RangeText");
    FillDataToDropDownlist("drpDryingTime", dryingTime, "RangeValue", "RangeText");
    FillDataToDropDownlist("drpCoolingTime", coolingTime, "RangeValue", "RangeText");

}

/**
 * Create paiting time rang base on Min and Max time
 * @param {any} min The min range
 * @param {any} max The max range
 * @returns {any} range of time.
 */
function CreatePaitingRangeVaule(min, max) {
    let rangeTime = [];

    //If min is 0 then get max value only
    if (min === 0) {
        let obj = { RangeText: max, RangeValue: max };
        rangeTime.push(obj);

        return rangeTime;
    }

    for (let i = min; i <= max; i++) {
        let obj = { RangeText: i, RangeValue: i };
        rangeTime.push(obj);
    }

    return rangeTime;
}
//START ADD) SON (2019.08.29) - 30 August 2019

// #region Upload machine file

function UploadFileAlert(objResult) {
    if (objResult.Result === Success) {

        var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
        //Show alert.
        ShowMessageOk("002", SmsFunction.Upload, MessageType.Success, MessageContext.Update, ObjMessageType.Info);

        //Close modal
        HideModal("modalUploadFileOpDetail");

        //Reload data ops detail gridview.
        ReloadJqGrid(gridOpsDetailName, objOpsMaster);
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
                var opMaster = GetSelectedOneRowData(gridOpsTableId);
                var lstNewOpdt = respone.Content;
                var arrLstOpdt = [];
                $.each(lstNewOpdt, function (i, val) {
                    val.X = $.isEmptyObject(val.X) ? null : val.X.split('.')[1];
                    val.Y = $.isEmptyObject(val.Y) ? null : val.Y.split('.')[1];
                    var node = CreateObjectForLayout(opMaster, val);
                    arrLstOpdt.push(node);
                });
                callBack(arrLstOpdt[0]);
                HideModal("mdlCloneProcess");
                //Update summary
                UpdateSummary();
                //Reload ops master gridview.      
                opMaster.Edition = $("#drpOpsMasterEdition").val();
                //ReloadJqGrid(gridOpsTableName, opMaster);
                ReloadJqGrid2LoCal(gridOpsTableName, opMaster);
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

//START ADD: HA
function ButtonClickEnterProcess() {
    $("#btnEnterProcess").click(function () {
        ShowModal("mdlProcess");
    });
}

function LoadAllProcessName() {
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    //Load process name.
    var opsLanId = MapLanguageToFlag(objOpsMaster.Language);
    var data = { languageId: opsLanId };
    ReloadJqGrid2LoCal("tbAllOpName", data);
}

function jqGridProcessName(languageId) {
    jQuery("#tbAllOpName").jqGrid({
        url: '/Ops/GetOpName',
        datatype: "json",
        postData: {
            languageId: languageId
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
        loadonce: true,
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
    let lstObjOpName = jQuery("#tbAllOpName").getGridParam('data');

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
    //var objOpTime = {OpnSerial: opnSerial, OpName: objSeldProcess.OpName, OpNameId: objSeldProcess.OpNameId}
    //jQuery("#tbOpTime").jqGrid('addRowData', 1, objOpTime);

    //if (gridSelPro.length = 0) {
    //    ArrSelectedOpname = [];
    //}

    let objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    let opSerial = $("#txtProcessNo").val();

    let opNameId = objSeldProcess.OpNameId;
    let objOpNameDt = {
        StyleCode: objOpsMaster.StyleCode
        , StyleSize: objOpsMaster.StyleSize
        , StyleColorSerial: objOpsMaster.StyleColorSerial
        , RevNo: objOpsMaster.RevNo
        , OpRevNo: objOpsMaster.OpRevNo
        , OpSerial: opSerial
        , Edition: objOpsMaster.Edition
        , OpNameId: opNameId
        , OpName: objSeldProcess.OpName
        , OpTime: ''
        , OpnSerial: opnSerial
    };

    gridSelPro.push(objOpNameDt);

    //ArrSelectedOpname.push(objOpNameDt);

    ReloadJqGridLocal("tbOpTime", gridSelPro);
}

function ButtonClickProcessDelete(rowid) {

    //var lstObjOpName = jQuery("#tbOpTime").getGridParam('data');
    //var objOpName = jQuery("#tbOpTime").jqGrid('getRowData', rowid);

    //Delete a row on gridview base row id
    jQuery("#tbOpTime").jqGrid('delRowData', rowid);

    //Get current list selected process after delete row.
    let gridSelPro = jQuery("#tbOpTime").getGridParam('data');

    //Set operation name serial again
    $.each(gridSelPro, function (idx, obj) {
        obj.OpnSerial = idx + 1;
    });

    //Assign selected process name list again.
    //ArrSelectedOpname = gridSelPro;

    //Reload grid selected process
    ReloadJqGridLocal("tbOpTime", gridSelPro);

}

function JqGridSelectedProcess(opNameData) {
    jQuery("#tbOpTime").jqGrid({
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
            { name: 'StitchCount', index: 'StitchCount', hidden: true} //ADD) SON - 25 December 2019  
        ],
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
        },
        gridComplete: function () {
            setTimeout(function () {
                updatePagerIcons();
            }, 0);

        }
    });

    function addTextbox(cellvalue, options, rowObject) {
        return "<input type='text' id='txtOpTimeTestMdl' class='form-control' maxlength='3' onkeypress='return isNumber(event)' >";
    }
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

//END ADD: HA