var SmSgGrid = "SmSgGrid";
var UserRole = null;
var arrAcction = {
    editTitle: "Edit Smsg",
    submintBtn: "Update",
    cancelBtn: "Close",
    addTitle: "Add Smsg",
    addBtn: "Add"
};
function CreateSmSgGrid(gId) {
    $("#" + SmSgGrid).jqGrid({
        datatype: "json",
        caption:"Message Management",
        pager: "#SmSgGrid_Nav",
        height: 350,
        page: 1,
        rowNum: 40,
        rowList: [40, 60, 80, 20],
        scroll: false,
        viewrecords: true,
        scrollrows: true,
        shrinkToFit: false,
        width: null,
        gridview: true,
        sortname: "UpdateDate",
        sortorder: "DESC",
        url: "/SmSg/GetListSmSg",
        colModel: [
            {
                name: "ContextSerial", index: "ContextSerial", label: "Serial", hidden: false, editable: false, width: 90, search: false
            },
            {
                name: "SystemId", index: "SystemId", label: arrSmSg.SystemId, width: 100, editable: true, search: true
                , searchoptions: { sopt: ["cn", "eq", "ne"] }
                , formoptions: { rowpos: 1, colpos: 1 }, edittype: 'select'
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbSystemId").html());
                        $(elem).attr("id", "SystemId");
                        $(elem).attr("name", "SystemId");
                    }
                }
            },
            {
                name: "MenuId", index: "MenuId", label: arrSmSg.MenuId, width: 100, editable: true, search: true
                , searchoptions: { sopt: ["cn", "eq", "ne"] }
                , formoptions: { rowpos: 2, colpos: 1 }, edittype: 'select'
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbMenuId").html());
                        $(elem).attr("id", "MenuId");
                        $(elem).attr("name", "MenuId");
                    }
                }
            },
            {
                name: "Function", index: "Function", label: arrSmSg.Function + "<spand class='requered'> (*)</spand>"
                , width: 100, editable: true, search: true
                , searchoptions: { sopt: ["cn", "eq", "ne"] }, editoptions: {maxlength: 200}
                , formoptions: { rowpos: 3, colpos: 1 }, editrules: { required: true }
            },
            {
                name: "MessageType", index: "MessageType", label: arrSmSg.Type, width: 200, editable: true, search: true
                , searchoptions: { sopt: ["cn", "eq", "ne"] }
                , formoptions: { rowpos: 4, colpos: 1 }, edittype: 'select', editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbTypeId").html());
                        $(elem).attr("id", "MessageType");
                        $(elem).attr("name", "MessageType");
                    }
                }
            },
            {
                name: "ContextDesc", index: "ContextDesc", label: arrSmSg.MessageContext
                , width: 200, editable: false, search: true
                , searchoptions: { sopt: ["cn", "eq", "ne"] }
            },
            {
                name: "MessageContext", index: "MessageContext", label: arrSmSg.MessageContext, width: 200, editable: true, hidden: true
                , searchoptions: { sopt: ["cn", "eq", "ne"] }
                , formoptions: { rowpos: 5, colpos: 1 }, edittype: 'select', editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbContextId").html());
                        $(elem).attr("id", "MessageContext");
                        $(elem).attr("name", "MessageContext");
                    }
                }, editrules: { edithidden: true }
            },
            {
                name: "Title", index: "Title", label: arrSmSg.Title + "<spand class='requered'> (*)</spand>", width: 200, editable: true, search: true, editrules: { required: true}
                , formoptions: {rowpos: 6, colpos: 1 }, searchoptions: { sopt: ["cn", "eq", "ne"], editoptions: {maxlength: 200}}
            },
            {
                name: "English", index: "English", label: arrSmSg.English, width: 200, editable: true, search: true
                , formoptions: { rowpos: 1, colpos: 2 }, searchoptions: { sopt: ["cn", "eq", "ne"], editoptions: { maxlength: 200 } }
            },
             {
                 name: "Vietnamese", index: "Vietnamese", label: arrSmSg.Vietnamese, width: 200, editable: true, search: true
                , formoptions: { rowpos: 2, colpos: 2 }, searchoptions: { sopt: ["cn", "eq", "ne"], editoptions: { maxlength: 200 } }
             },
            {
                name: "Korean", index: "Korean", label: arrSmSg.Korean, width: 200, editable: true, search: true
                , formoptions: { rowpos: 3, colpos: 2 }, searchoptions: { sopt: ["cn", "eq", "ne"], editoptions: { maxlength: 200 } }
            },
            {
                name: "Indonesian", index: "Indonesian", label: arrSmSg.Indonesian, width: 200, editable: true, search: true
                , formoptions: { rowpos: 4, colpos: 2 }, searchoptions: { sopt: ["cn", "eq", "ne"] }
            },
            {
                name: "Myanmar", index: "Myanmar", label: arrSmSg.Myanmar, width: 100, editable: true, search: true
                , formoptions: { rowpos: 5, colpos: 2 }, searchoptions: { sopt: ["cn", "eq", "ne"], editoptions: { maxlength: 200 } }
            },
            {
                name: "Amharic", index: "Amharic", label: arrSmSg.Amharic, width: 100, editable: true, search: true
                , formoptions: { rowpos: 6, colpos: 2 }, searchoptions: { sopt: ["cn", "eq", "ne"], editoptions: { maxlength: 200 } }
            },
            { name: "UpdateDate", index: "UpdateDate", hidden: true }
        ],
        loadComplete: function () {
            updatePagerIcons();
        },
        ondblClickRow: function (rowid) {
            $("#edit_" + SmSgGrid).trigger("click");
        },
        onPaging: function (pgButton) {
            if (pgButton === "records") {
                SetPaging(SmSgGrid, "SmSgGrid_Nav");
            }
        }
    }).jqGrid('navGrid', '#SmSgGrid_Nav', {
        cloneToTop: true,
        add: CheckRoleAdd(),
        edit: true,
        del: CheckRoleDel()
    },
    {
        //edit
        url: '/SmSg/UpdateSmSg',
        recreateForm: true, closeAfterEdit: true, closeOnEscape: true, viewPagerButtons: true,
        editCaption: arrAcction.editTitle, bSubmit: arrAcction.submintBtn, bCancel: arrAcction.cancelBtn,
        afterShowForm: function (form) {
            // read only for key
            $('#SystemId,#MenuId,#Function,#MessageType,#MessageContext').prop("disabled", true);
            //==============bgcolor
            $('#SystemId,#MenuId,#Function,#MessageType,#MessageContext').css("background-color", "#eee");
            //#background-color: beige;
            SetWidthForFormEditing("tr_SystemId", SmSgGrid, 80);
            DragFormCenter("#editmod" + SmSgGrid);
            EditCssForm("TblGrid_" + SmSgGrid);
            AddClassForAcction("#sData", "#cData");
            if (!CheckRoleEdit()) {
                SetReadOnlyForm("#editmod" + SmSgGrid, true, "#sData");
            } else {
                ChangeSystem();
                var rowid = $("#"+SmSgGrid).jqGrid("getGridParam", "selrow");
                var messageType = $("#"+SmSgGrid).jqGrid("getCell", rowid, "MessageType");
                ChangeType(messageType);
            }
            SelectedCombobox();
            $("#Function").attr("list", "cbFunctionId");
        },
        onclickSubmit: function (options, postdata) {
            var selRowId = $("#" + SmSgGrid).jqGrid("getGridParam", "selrow");
            var data = $("#" + SmSgGrid).jqGrid("getRowData", selRowId);
            options.url = "/SmSg/UpdateSmSg?ContextSerial=" + data.ContextSerial;
        },
        afterComplete: function (response, postdata) {
            var rowid = postdata.id;
            $("#" + SmSgGrid).jqGrid('setSelection', rowid, false);
            return [true];
        }
        
    },
    {
        //add
        addCaption: arrAcction.addTitle, bSubmit: arrAcction.addBtn, bCancel: arrAcction.cancelBtn,
        url: '/SmSg/AddSmSg',
        recreateForm: true, closeAfterAdd: true, closeOnEscape: true,
        beforeShowForm: function (form) {
            var formRows = form.find(".FormData");
        },
        afterShowForm: function (form) {
            SetWidthForFormEditing("tr_SystemId", SmSgGrid, 80);
            DragFormCenter("#editmod" + SmSgGrid);
            EditCssForm("TblGrid_" + SmSgGrid);
            AddClassForAcction("#sData", "#cData");
            $("#Function").attr("list", "cbFunctionId");
            ChangeSystem();
            ChangeType();
        },
        afterComplete: function (response, postdata) {
            var rowid = postdata.id;
            $("#" + SmSgGrid).jqGrid('setSelection', rowid, false);
            return [true];
        }
    },
    {
        //del
        url: '/SmSg/DelSmSg',
        recreateForm: true, closeAfterEdit: true, closeOnEscape: true,
        afterShowForm: function (form) {
            DragFormCenter("#delmod" + SmSgGrid);
            AddClassForAcction("#dData", "#eData");
        },
        onclickSubmit: function (options, postdata) {
            var selRowId = $("#" + SmSgGrid).jqGrid("getGridParam", "selrow");
            var data = $("#" + SmSgGrid).jqGrid("getRowData", selRowId);
            options.url = "/SmSg/DelSmSg?ContextSerial=" + data.ContextSerial + "&SystemId=" + data.SystemId + "&MenuId=" + data.MenuId +
                          "&Function=" + data.Function + "&MessageType=" + data.MessageType + "&MessageContext=" + data.MessageContext;
        }
    }
    );
    SearchFilter($("#" + SmSgGrid));
}

function AddErrorForKey() {
    $("#SystemId,#MenuId,#Function,#MessageType,#MessageContext").addClass("error");
}

function SetWidthForFormEditing(id, gridName, w) {
    var curentW = $('#' + id).width();
    $('#editmod' + gridName + '').css('width', curentW + w);
}
function SelectedCombobox() {
    var selRowId = $("#" + SmSgGrid).jqGrid('getGridParam', 'selrow');
    var dataRow = $("#" + SmSgGrid).jqGrid("getRowData", selRowId);
    var SystemId = dataRow.SystemId;
    var MenuId = dataRow.MenuId;
    var type = dataRow.MessageType;
    var context = dataRow.MessageContext;
    var Function = dataRow.Function;
    $("#SystemId").val(SystemId);
    $("#MenuId").val(MenuId);
    //$("#Function").val(Function);
    $("#MessageType").val(type);
    $("#MessageContext").val(context);
}

// role
function GetRole() {
    if ($.isEmptyObject(UserRole)) {
        UserRole = GetUserRoleInfo("OPS", "SMS");
    }
    return UserRole;
}

function CheckRoleAdd() {
    var role = GetRole();
    if (role.IsAdd === "1") {
        return true;
    }
    return false;
}

function CheckRoleEdit() {
    var role = GetRole();
    if (role.IsUpdate === "1") {
        return true;
    }
    return false;
}

function CheckRoleDel() {
    var role = GetRole();
    if (role.IsDelete === "1") {
        return true;
    }
    return false;
}

