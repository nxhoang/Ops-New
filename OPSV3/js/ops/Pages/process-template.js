
var UserRole = null;

//Get ActionCode
function GetActionCodeTemplate(OpType) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: OpType }));
    AjaxPostCommon(config, function (respone) {

        var arrActioncode = [];
        var obj = { id: '999', name: 'ALL' };
        arrActioncode.push(obj);

        for (var i = 0; i < respone.length; i++) {
            var object = { id: '', name: '' };
            object.id = respone[i].SubCode;
            object.name = respone[i].CodeName;
            arrActioncode.push(object);
        }
        FillDataToDropDownlist("drpActionCodeTemplate", arrActioncode, "id", "name");
    });
}

function LoadTemplateByActionCode() {
    $("#drpActionCodeTemplate").change(function () {
        var value = $("#drpActionCodeTemplate").val();

        var data = { actionCode: value };
        ReloadJqGrid2LoCal("tbActionCodeTemplate", data);
    });
}

function GetSelectProcessName() {
    var languageId = '@HttpContext.Current.Request.Cookies["language"].Value';
    SelectProcessName(languageId);
}

function SelectProcessName(languageId) {
    $('#selectProcessName').select2({
        minimumInputLength: 1,
        placeholder: "Search process ...",
        ajax: {
            url: '/Ops/SearchProcessName',
            delay: 250,
            data: function (params) {
                return {
                    q: params.term, // search term
                    languageId: languageId,
                };
            },
            processResults: function (data) {
                var dataMod = [];
                if (!$.isEmptyObject(data.items)) {

                    var langId = '@HttpContext.Current.Request.Cookies["language"].Value';

                    $.each(data.items, function (index, item) {
                        var language;
                        switch (langId) {
                            case 'vi':
                                language = item.Vietnam;
                                break;
                            case 'id':
                                language = item.Indonesia;
                                break;
                            case 'mm':
                                language = item.Myanmar;
                                break;
                            case 'et':
                                language = item.Ethiopia;
                                break;
                            default:
                                language = item.English;
                                break;
                        }
                        var newItem = { id: item.OpNameId, text: language };
                        dataMod.push(newItem);
                    });
                }
                return { results: dataMod };
            },
            multiple: true,
            allowClear: true
        },
        editrules: { edithidden: true, required: true }
    });
}

function EventBtnClick() {

    //Add Process Name
    $("#btnAddProcessName").click(function () {
        var rowId = $("#tbActionCodeTemplate").jqGrid('getGridParam', 'selrow');

        if (rowId) {
            var rowData = $("#tbActionCodeTemplate").getRowData(rowId);

            var actioncode = rowData.ActionCodeID;
            var tempid = rowData.TempId;

            var lstSelectProcess = $("#selectProcessName").val();

            if ($.isEmptyObject(lstSelectProcess)) {
                ShowMessageOk("013", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert, "Process");
                return null;
            }
            else {
                var lstProcess = [];

                for (var i = 0; i < lstSelectProcess.length; i++) {
                    var obj = { ActionCode: actioncode, TempId: tempid, OpNameId: lstSelectProcess[i] };
                    lstProcess.push(obj);
                }
                //}

                AddProcessToTemplate(lstProcess);

                var language = '@HttpContext.Current.Request.Cookies["language"].Value';
                var data = { opActionCode: actioncode, opTempId: tempid, opLanguage: language };
                ReloadJqGrid2LoCal("tbProcessName", data);

                $('#selectProcessName').empty().trigger('change');
            }
        }
        else ShowMessageOk("013", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert, "Template Actioncode");
    });

    //Add new template
    $("#btnAddNewTemplate").click(function () {

        var newTempName = $("#txtNewTemplate").val();
        var data = $('#tbAllTemplate').jqGrid('getGridParam', 'data');
        var maxID = 0;

        for (var i = 0; i < data.length; i++) {
            if (data[i].TempId >= maxID) {
                maxID = data[i].TempId;
            }

            if (newTempName == data[i].TempName) {
                return ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, MessageTypeAlert);
            }
        }
        var newId = maxID + 1;
        var newTemplate = { TempId: newId, TempName: newTempName };

        AddNewTemplate(newTemplate);
        $("#tbAllTemplate").addRowData("newId", newTemplate);
        $("#txtNewTemplate").val('');
    });

    //Add template to ActionCode
    $('#btnAddTemplateActioncode').click(function () {

        var lstSelected = GetSelectedMultipleRowsData("#tbAllTemplate");
        var actioncode = $("#drpActionCodeTemplate").val();
        var lstTemplate = [];

        if (actioncode && actioncode != '999') {
            for (var i = 0; i < lstSelected.length; i++) {
                var obj = { ActionCode: actioncode, TempId: lstSelected[i].TempId };
                lstTemplate.push(obj);
            }

            AddTemplateToActioncode(lstTemplate);
            var data = { ActionCode: actioncode };
            ReloadJqGrid2LoCal("tbActionCodeTemplate", data);

            HideModal("mdlAddTemplate");
        }

        else ShowMessageOk("013", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert, "Action Code");

    });
}

//Add template to actioncode
function AddTemplateToActioncode(lstTemplate) {
    var addStatus;
    $.ajax({
        url: "/Ops/AddTemplateToActioncode",
        async: false,
        type: "POST",
        data: JSON.stringify({ lstTemplate: lstTemplate }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                addStatus = true;
                ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add, ObjMessageType.Info);

            } else {
                addStatus = false;
                ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, "Template Actioncode exist!");
            }
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessageOk("013", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert, "Action Code");
            addStatus = false;
            return null;
        }
    });
    return addStatus;
}

//Add process to template actioncode
function AddProcessToTemplate(lstProcess) {
    var addStatus;
    $.ajax({
        url: "/Ops/AddProcessToTemplate",
        async: false,
        type: "POST",
        data: JSON.stringify({ lstProcess: lstProcess }),
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

//Add new template
function AddNewTemplate(objTemplate) {
    var addStatus;
    $.ajax({
        url: "/Ops/AddNewTemplate",
        async: false,
        type: "POST",
        data: JSON.stringify({ objTemplate: objTemplate }),
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

//Fill data to Template table
function jqGridTemplate(actionCode) {
    jQuery("#tbActionCodeTemplate").jqGrid({
        url: '/OPS/GetActionCodeTable',
        datatype: "json",
        postData: {
            actionCode: actionCode,
        },
        width: null,
        shrinkToFit: false,
        height: 250,
        colModel: [
            { name: 'ActionCode', index: 'ActionCode', label: 'ActionCode', width: 200 },
            { name: 'TempName', index: 'TempName', label: 'Template Name', width: 400 },
            { name: 'TempId', index: 'TempId', hidden: true },
            { name: 'ActionCodeID', index: 'ActionCodeID', hidden: true },
        ],
        rowList: [10, 20, 30],
        pager: '#pagerTemplate',
        viewrecords: true,
        loadonce: true,
        multiselect: false,
        sortorder: "asc",
        sortname: 'ActionCode',
        caption: "ActionCode Template",
        gridview: true,

        onSelectRow: function (id) {
            var rowData = $("#tbActionCodeTemplate").jqGrid("getRowData", id);
            var actionCodeId = rowData.ActionCodeID;
            var tempId = rowData.TempId;

            var language = '@HttpContext.Current.Request.Cookies["language"].Value';
            var data = { opActionCode: actionCodeId, opTempId: tempId, opLanguage: language };
            ReloadJqGrid2LoCal("tbProcessName", data);
        },

    }).jqGrid('navGrid', '#pagerTemplate', {
        view: false,
        viewicon: 'ace-icon fa fa-search-plus grey',
        add: true,
        addicon: 'ace-icon fa fa-plus blue',
        edit: false,
        del: true,
        search: false,
        searchicon: 'ace-icon fa fa-search orange',
        refresh: false,
        refreshicon: 'ace-icon fa fa-refresh green',
    },

    {/*edit*/ },

    { //add
        beforeShowForm: function () {
            $("#tbAllTemplate").jqGrid("resetSelection");
            ShowModal("mdlAddTemplate");
            
        },
        afterShowForm: function () {
            $("#editmodtbActionCodeTemplate").css("display", "none");
            $(".ui-widget-overlay").addClass("displayClass");
        }
    },

    { //delete
        url: "/Ops/DeleteTemplateActioncode"
        , delData: {
            opKey: function () {

                var rowId = $("#tbActionCodeTemplate").jqGrid('getGridParam', 'selrow');
                var rowData = $("#tbActionCodeTemplate").getRowData(rowId);
                var template = JSON.stringify({ actioncode: rowData.ActionCodeID, tempid: rowData.TempId });
                return template;
            }
        }
        , afterComplete: function (response) {
            var res = JSON.parse(response.responseText);
            if (res === Success) {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info, "");
            } else {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Error, res);
            }
            ReloadJqGrid2LoCal("tbProcessName", null);
        }
    });
}

//Fill data to ProcessName table
function jqGridProcessNameTemplate(actioncode, tempid, languageId) {
    jQuery("#tbProcessName").jqGrid({
        url: '/OPS/GetProcessNameTable',
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
            { name: 'OpNameId', index: 'OpNameId', label: 'ID', width: 200 },
            { name: 'OpNameLan', index: 'OpNameLan', label: 'Process Name', width: 400 },
            { name: 'TempId', index: 'TempId', hidden: true },
            { name: 'ActionCode', index: 'ActionCode', hidden: true },
        ],
        rowList: [10, 20, 30],
        pager: '#pagerProcessName',
        sortname: 'OpNameLan',
        viewrecords: true,
        loadonce: true,
        multiselect: true,
        sortorder: "asc",
        caption: "Process Template",
        gridview: true,
        //autowidth: true,
    }).jqGrid('navGrid', '#pagerProcessName', {
        view: false,
        viewicon: 'ace-icon fa fa-search-plus grey',
        add: false,
        addicon: 'ace-icon fa fa-plus blue',
        edit: false,
        del: true,
        search: false,
        searchicon: 'ace-icon fa fa-search orange',
        refresh: false,
        refreshicon: 'ace-icon fa fa-refresh green'
    },

    { /*edit*/ },

    { /*add*/ },

    { //delete
        url: "/Ops/DeleteProcessTemplate"
        , delData: {
            opKey: function () {
                var lstProcess = JSON.stringify(GetSelectedMultipleRowsData("#tbProcessName"));
                return lstProcess;
            }
        }
        , afterComplete: function (response) {
            var res = JSON.parse(response.responseText);
            if (res === Success) {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info, "");
            } else {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Error, res);
            }
            
        }
    });
}

//Get all Template
function jqGridAddTemplate() {
    jQuery("#tbAllTemplate").jqGrid({
        url: '/OPS/GetAllTemplate',
        datatype: "json",
        postData: {
        },
        width: null,
        shrinkToFit: false,
        height: 250,
        colModel: [
            { name: 'TempId', index: 'TempId', label: 'Template ID', width: 300, add: true },
            { name: 'TempName', index: 'TempName', label: 'Template Name', width: 500, add: true }
        ],
        rowList: [10, 20, 30],
        pager: '#pagerAllTemplate',
        sortname: 'TempId',
        viewrecords: true,
        loadonce: true,
        multiselect: true,
        sortorder: "asc",
        caption: "All Template",
        gridview: true,
        autowidth: false,
        //editable: true,
        gridComplete: function () {
            setTimeout(function () {
                window.updatePagerIcons();
            }, 0);
        }
    }).jqGrid('navGrid', '#pagerAllTemplate', {
        view: false,
        viewicon: 'ace-icon fa fa-search-plus grey',
        add: false,
        addicon: 'ace-icon fa fa-plus blue',
        edit: false,
        del: true,
        search: false,
        searchicon: 'ace-icon fa fa-search orange',
        refresh: false,
        refreshicon: 'ace-icon fa fa-refresh green'
    },
    { /*edit*/ },

    { //add
    },
    { //delete
        url: "/Ops/DeleteTemplate"
        , delData: {
            opKey: function () {
                var lstTemplateSelect = [];
                var selRowIds = $("#tbAllTemplate").jqGrid("getGridParam", "selarrrow");

                for (var i = 0; i < selRowIds.length; i++) {
                    var rowData = $("#tbAllTemplate").getRowData(selRowIds[i]);
                    lstTemplateSelect.push(rowData.TempId);
                }
                var lstTemplate = JSON.stringify(lstTemplateSelect);
                return lstTemplate;
            }
        }
        , afterComplete: function (response) {
            var res = JSON.parse(response.responseText);
            if (res === Success) {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info, "");
            } else {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Error, res);
            }
            var actioncode = $("#drpActionCodeTemplate").val();
            var data = { ActionCode: actioncode };
            ReloadJqGrid2LoCal("tbActionCodeTemplate", data);
            ReloadJqGrid2LoCal("tbProcessName", null);
        }
    });
}

function GetRoleProcessTemplate() {
    if ($.isEmptyObject(UserRole)) {

        UserRole = GetUserRoleInfo("OPS", "PTL");

        if (UserRole.IsDelete != '1') {
            $("span").removeClass("ui-icon ui-icon-trash");
        }
    }
    return UserRole;
}
