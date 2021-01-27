var MachineGrid = "MachineGrid";
var newSrc = null;
var UserRole = null;
function CreateMcGrid(gId, machine, titleGrid) {
    var showGsd = machine == MachineType.machine ? false : true;
    $("#" + MachineGrid).jqGrid({
        caption: titleGrid,
        datatype: "json",
        pager: "#MachineGrid_Nav",
        sortname: "ItemCode",
        sortorder: "DESC",
        height: 350,
        shrinkToFit: false,
        width: null,
        rownumbers: true,
        pgbuttons: false,
        viewrecords: false,
        pginput: false,
        rowNum: 10000000,
        //==========================================
        url: "/OpsMasterData/GetOtmtsMc",
        postData: {
            gId: gId, machine: machine
        },
        //mtype: 'POST',
        colModel: [
            //START ADD - SON) 8/Oct/2020
            {
                name: "GroupLevel_0", index: "GroupLevel_0", label: "OpGroup"
                , editable: true, formoptions: { rowpos: 1/*8*/, colpos: 1 }, edittype: 'select', hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#divOpGroup").html());
                        $(elem).attr("id", "GroupLevel_0");
                        $(elem).attr("name", "GroupLevel_0");
                        $(elem).attr("onchange", "eventOnChangeOpGroup(this.value)");
                    }
                }
            },      
            {
                name: "MachineGroup", index: "MachineGroup", label: "OpGroup"
                , editable: true, formoptions: { rowpos: 1, colpos: 1 }, edittype: 'select', hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editoptions: {
                    maxlength: 15,
                    dataInit: function (elem) {
                        $(elem).html($("#divMachineGroup").html());
                        $(elem).attr("id", "MachineGroup");
                        $(elem).attr("name", "MachineGroup");
                    }
                }
            },
            {
                name: "GroupLevel_1", index: "GroupLevel_1", label: "OpDetail"
                , editable: true, formoptions: { rowpos: 1/*8*/, colpos: 2 }, edittype: 'select', hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editoptions: {
                    dataInit: function (elem) {
                        //$(elem).html($("#drpOpDetail").html());
                        $(elem).attr("id", "GroupLevel_1");
                        $(elem).attr("name", "GroupLevel_1");
                        $(elem).attr("onchange", "eventOnChangeSubOpGroup(this.value)");
                    }
                }
            },
            {
                name: "GroupLevel_2", index: "GroupLevel_2", label: "Op Sub Group"
                , editable: true, formoptions: { rowpos: 1/*8*/, colpos: 3 }, edittype: 'select', hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editoptions: {
                    dataInit: function (elem) {
                        //$(elem).html($("#drpOpSubGroup").html());
                        $(elem).attr("id", "GroupLevel_2");
                        $(elem).attr("name", "GroupLevel_2");
                    }
                }
            },
            //END ADD - SON) 8/Oct/2020
            {
                name: "CategId", index: "CategId", label: arrMc.CategId, width: 100
                , editable: true, formoptions: { rowpos: 2/*1*/, colpos: 1 }, edittype: 'select', hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbCategory").html());
                        $(elem).attr("id", "CategId");
                        $(elem).attr("name", "CategId");
                    }
                }
            },
            {
                name: "Category", index: "Category", label: arrMc.Category, width: 100, editable: false, search: false, hidden: true
                , formatter: function (cellvalue, options, rowObject) {
                    return rowObject.CategId + " - " + rowObject.Category;
                }
            },
            {
                name: "ItemCode", index: "ItemCode", label: arrMc.ItemCode, editable: true
                , formoptions: { rowpos: 4/*3*/, colpos: 1 }, hidden: false, editrules: { edithidden: true }, hidedlg: false
                , search: true, editoptions: { readonly: "readonly" }
            },
            {
                name: "ItemName", index: "ItemName", label: arrMc.ItemName, editable: true
                , formoptions: { rowpos: 5/*4*/, colpos: 1 }
                , search: true, editoptions: { maxlength: 300 }, editrules: { custom: true, custom_func: emptyCheck }
            },
            {
                name: "Sos", index: "Sos", label: arrMc.Sos, editable: true, hidden: true, editrules: { edithidden: true }, hidedlg: false
                , formoptions: { rowpos: 2 /*1*/, colpos: 2 }
                , edittype: 'select'
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbSuppliers").html());
                        $(elem).attr("id", "Sos");
                        $(elem).attr("name", "Sos");
                    }
                }
            },
            { name: "FullName", index: "FullName", label: arrMc.Sos },
            { name: "Model", index: "Model", label: arrMc.Model, editable: true, formoptions: { rowpos: 3/*2*/, colpos: 1 }, editoptions: { maxlength: 30 } },
            {
                name: "Buyer", index: "Buyer", label: arrMc.Buyer, hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editable: true, formoptions: { rowpos: 4/*3*/, colpos: 2 }
                , edittype: 'select'
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbBuyer").html());
                        $(elem).attr("id", "Buyer");
                        $(elem).attr("name", "Buyer");
                    }
                }
            },
            {
                name: "BuyerName", index: "BuyerName", label: arrMc.Buyer,
                formatter: function (cellvalue, options, rowObject) {
                    if (rowObject.Buyer) {
                        return rowObject.Buyer + " - " + rowObject.BuyerName;
                    } else {
                        return '';
                    }
                }
            },
            {
                name: "Cost", index: "Cost", label: arrMc.Cost, editable: true
                , formoptions: { rowpos: 5 /*4*/, colpos: 2 }, search: false
                , editoptions: {
                    maxlength: 15,
                    dataInit: function (element) {
                        $(element).keypress(function (e) {
                            return isDecimalNumber(e);
                        });
                    }
                }
            },
            {
                name: "Unit", index: "Unit", label: arrMc.Unit, editable: true
                , edittype: 'select'
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbUnitId").html());
                        $(elem).attr("id", "Unit");
                        $(elem).attr("name", "Unit");
                    }
                }
                , formoptions: { rowpos: 5/*4*/, colpos: 3 }
            },
            {
                name: "Remarks", index: "Remarks", label: arrMc.Remarks, editable: true
                , formoptions: { rowpos: 2/*1*/, colpos: 4 }
            },
            {
                name: "Ranking", index: "Ranking", label: arrMc.Ranking
                , editable: true, formoptions: { rowpos: 3/*2*/, colpos: 4 }, edittype: 'select'
                , editoptions: { value: { 1: '1', 2: '2', 3: "3", 4: "4", 5: "5" }, search: true }
            },
            { name: "Purpose", index: "Purpose", label: arrMc.Purpose, edittype: "textarea", editable: true, formoptions: { rowpos: 6 /*5*/, colpos: 1 }, editoptions: { maxlength: 300 } },
            { name: "Process", index: "Process", label: arrMc.Process, editable: true, edittype: "textarea", formoptions: { rowpos: 7 /*6*/, colpos: 1 }, editoptions: { maxlength: 300 } },
            { name: "GsdRefId", index: "GsdRefId", label: arrMc.GsdRefId, editable: true, hidden: showGsd, formoptions: { rowpos: 4 /*3*/, colpos: 4 }, editoptions: { maxlength: 30, hidden: true } },
            { name: "Status", index: "Status", label: arrMc.Status, editable: true, formoptions: { rowpos: 4 /*3*/, colpos: 4 }, editoptions: { readonly: "readonly" } },
            { name: "Brand", index: "Brand", label: arrMc.Brand, editable: false, formoptions: { rowpos: 3 /*2*/, colpos: 2 } },
            { name: "ProcessName", index: "ProcessName", label: 'Process Name', editable: false, formoptions: { rowpos: 3 /*2*/, colpos: 2 } }, //ADD - SON) 28/Feb/2020
            {//START ADD - SON) 28/Feb/2020
                name: "ProcessCode", index: "ProcessCode", label: "Process Code", width: 100
                , editable: true, formoptions: { rowpos: 8 /*7*/, colpos: 1 }, edittype: 'select', hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#drpProcess").html());
                        $(elem).attr("id", "ProcessCode");
                        $(elem).attr("name", "ProcessCode");
                    }
                }
            },//END ADD - SON) 28/Feb/2020            
            {
                name: "BrandId", index: "BrandId", label: arrMc.Brand, width: 100
                , editable: true, formoptions: { rowpos: 3/*2*/, colpos: 2 }, edittype: 'select', hidden: true, editrules: { edithidden: true }, hidedlg: false
                , editoptions: {
                    dataInit: function (elem) {
                        $(elem).html($("#cbBrand").html());
                        $(elem).attr("id", "BrandId");
                        $(elem).attr("name", "BrandId");
                    }
                }
            },
            {
                name: "ImagePath", index: "ImagePath", label: arrMc.ImagePath, align: "center", formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return "<img id='" + id +
                            "'src='" + cellvalue + "' alt='" + cellvalue + "' class='imgpattern'  onerror='imgError(this);'/>";
                    return "";
                }, edittype: 'file',
                editoptions: {
                    enctype: "multipart/form-data",
                    accept: "image/*"
                },
                editable: true, search: false, formoptions: { rowpos: 5/*4*/, colpos: 4 }
            }
        ],
        ondblClickRow: function () {
            //if (!CheckRoleEdit()) {
            //    $("#edit_MachineGrid").removeClass('ui-state-disabled');
            //    $("#edit_" + MachineGrid).trigger("click");
            //    $("#edit_MachineGrid").addClass('ui-state-disabled');
            //} else {
            //    $("#edit_" + MachineGrid).trigger("click");
            //}
            $("#edit_" + MachineGrid).trigger("click");
        },
        gridComplete: function () {
        }

    }).jqGrid('navGrid', '#MachineGrid_Nav', {
        cloneToTop: true,
        add: true,
        edit: true,
        del: true,
        search: false,
        refresh: false
    },
        {
            //edit
            url: '/OpsMasterData/UpdateMachine?type=' + machine,
            editCaption: arrAcction.editTitle, bSubmit: arrAcction.submintBtn, bCancel: arrAcction.cancelBtn,
            recreateForm: true, closeAfterEdit: true, closeOnEscape: true, viewPagerButtons: true,
            beforeShowForm: function (form) {
                var formRows = form.find(".FormData");
                MergerRow(formRows, true);
            },
            afterShowForm: function (form) {
                SetWidthForFormEditing("tr_ItemCode", MachineGrid);
                DragFormCenter("#editmod" + MachineGrid);
                SelectedCombobox();
                ChangeImageBtn();
                EditCssForm("TblGrid_" + MachineGrid);
                AddClassForAcction("#sData", "#cData");
                $("#edithdMachineGrid,.ui-jqdialog-titlebar-close").mousedown(function () {
                    CoseAllSelect2();
                });
                if (!CheckRoleEdit()) {
                    SetReadOnlyForm("#editmod" + MachineGrid, true, "#ImagePath,#sData");
                    $('#Purpose,#Process,#Buyer,#Unit,#Ranking,#CategId, #ProcessCode, #GroupLevel_0, #GroupLevel_1, #GroupLevel_2').css("background-color", "rgb(245, 245, 245)");
                }
                var textTitle = $("#editmod" + MachineGrid).find("span.ui-jqdialog-title").text();
                textTitle = textTitle + ": " + $("#ItemName").val();
                $("#editmod" + MachineGrid).find("span.ui-jqdialog-title").text(textTitle);
            },
            beforeSubmit: function (postdata, formid) {
                //more validations
                if (!Validate()) {
                    $("#FormError").find("td.ui-state-error").html(value);
                    $("#select2-CategId-container").parent().addClass("error");
                    var smsg = GetMsgAsin('001', "OPS", "OPM", SmsFunction.BeforeChange, MessageType.Warning, MessageContext.IgnoreChanges, language);
                    var value = ReplaceStr(smsg.value, "Category");
                    return [false, value];
                }
                var itName = $("#ItemName").val();
                if ($.isEmptyObject(itName)) {
                    $("#ItemName").addClass("error");
                }
                // Save image
                var fileUpload = $("#ImagePath").get(0);

                if (fileUpload.files.length !== 0) {
                    var ItemCode = $("#ItemCode").val();
                    UploadFile(fileUpload, ItemCode);
                }
                return [true, '']; // no error
            },
            onclickSubmit: function (options) {
                if ($.isEmptyObject(newSrc)) {
                    newSrc = $("#imgMachine").attr("alt");
                }
                options.url = '/OpsMasterData/UpdateMachine?type=' + machine + "&newSrc=" + newSrc;
                newSrc = null;
            }
        },
        {
            //add
            addCaption: arrAcction.addTitle, bSubmit: arrAcction.addBtn, bCancel: arrAcction.cancelBtn,
            url: '/OpsMasterData/AddMachine?type=' + machine,
            recreateForm: true, closeAfterAdd: true, closeOnEscape: true,
            beforeShowForm: function (form) {
                var formRows = form.find(".FormData");
                MergerRow(formRows);
            },
            afterShowForm: function (form) {
                SetWidthForFormEditing("tr_ItemCode", MachineGrid);
                DragFormCenter("#editmod" + MachineGrid);
                SetSelection2();
                AddClassForAcction("#sData", "#cData");
                var opToolConfig = {
                    url: "/OpsMasterData/GetAutomaticCode",
                    postData: JSON.stringify({ isMachine: machine })
                };
                AjaxPostCommon(opToolConfig, function (response) {
                    $("#ItemCode").val(response);
                });
                $("#edithdMachineGrid,.ui-jqdialog-titlebar-close").mousedown(function () {
                    CoseAllSelect2();
                });
                ChangeImageBtn();
                EditCssForm("TblGrid_" + MachineGrid);
            },
            beforeSubmit: function (postdata, formid) {
                if (!Validate()) {
                    $("#FormError").find("td.ui-state-error").html(value);
                    $("#select2-CategId-container").parent().addClass("error");
                    var smsg = GetMsgAsin('001', "OPS", "OPM", SmsFunction.BeforeChange, MessageType.Warning, MessageContext.IgnoreChanges, language);
                    var value = ReplaceStr(smsg.value, "Category");
                    return [false, value];
                }
                var itName = $("#ItemName").val();
                if ($.isEmptyObject(itName)) {
                    $("#ItemName").addClass("error");
                }
                // Save image
                var fileUpload = $("#ImagePath").get(0);

                if (fileUpload.files.length !== 0) {
                    var ItemCode = $("#ItemCode").val();
                    UploadFile(fileUpload, ItemCode);
                }
                return [true, '']; // no error
            },
            onclickSubmit: function (options) {
                if ($.isEmptyObject(newSrc)) {
                    newSrc = $("#imgMachine").attr("alt");
                }
                if (!$.isEmptyObject(newSrc)) {
                    options.url = '/OpsMasterData/AddMachine?type=' + machine + "&newSrc=" + newSrc;
                    newSrc = null;
                }
            }
        },
        {
            //del
            url: '/OpsMasterData/DeleteMachine',
            recreateForm: true, closeAfterEdit: true, closeOnEscape: true,
            afterShowForm: function (form) {
                DragFormCenter("#delmod" + MachineGrid);
                EditCssForm();
                AddClassForAcction("#dData", "#eData");
            },
            beforeSubmit: function () {
                var selRowId = $("#" + MachineGrid).jqGrid("getGridParam", "selrow");
                var itemcode = $("#" + MachineGrid).jqGrid("getCell", selRowId, "ItemCode");
                if (CheckReferenceKey(itemcode)) {
                    var name = MachineType.tool === machine ? "Machine" : "Tools";
                    var smsg = GetMsgAsin("001", SystemIdOps, "OTM", SmsFunction.Delete, MessageType.Error, MessageContext.Error, language);
                    smsg = ReplaceStr(smsg.value, name);
                    return [false, smsg];
                } else {
                    return [true];
                }
            },
            onclickSubmit: function (options) {
                var selRowId = $("#" + MachineGrid).jqGrid("getGridParam", "selrow");
                var itemcode = $("#" + MachineGrid).jqGrid("getCell", selRowId, "ItemCode");
                options.url = "/OpsMasterData/DeleteMachine?itemcode=" + itemcode;
            }
        }
    );
    SearchFilter($("#" + MachineGrid));
    //if (!CheckRoleEdit()) {
    //    $("#edit_MachineGrid").addClass('ui-state-disabled');
    //}
    if (!CheckRoleAdd()) {
        $("#add_MachineGrid").addClass('ui-state-disabled');
    }
    if (!CheckRoleDel()) {
        $("#del_MachineGrid").addClass('ui-state-disabled');
    }

    //START ADD) SON (2020.01.08) - 08 January 2020
    $(window).on('resize', function () {
        let height = $(window).height() - 350;
        $('.ui-jqgrid-bdiv').height(height);

        $("#" + MachineGrid).setGridHeight(height);
    }).trigger('resize');
    //END ADD) SON (2020.01.08) - 08 January 2020
}

function CoseAllSelect2() {
    $("#CategId,#Sos,#BrandId, #ProcessCode, #GroupLevel_1, #GroupLevel_2").select2("close"); //MOD - SON) 28/Feb/2020 - add drpProcessCode
}

emptyCheck = function (value, colname) {
    if (value) {
        $("#ItemName").removeClass("error");
        return [true];
    } else {
        $("#ItemName").addClass("error");
        return [false, "Please input name"];
    }
}

function CheckReferenceKey(itemcode) {
    var result;
    var opToolConfig = {
        url: "/OpsMasterData/CheckReferenceKey",
        async: false,
        postData: JSON.stringify({ itemcode: itemcode })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        result = response;
    });
    if (result === 1)
        return true;
    return false;
}

function Validate() {
    var value = $("#CategId").val();
    if (value) {
        return true;
    }
    return false;
}

function SelectRowMachine(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition) {
    $("#" + MachineGrid).jqGrid("setGridParam", {
        postData: {
            gId: $("#cbMachine").val()
        }
    }).trigger("reloadGrid");
}

function SetWidthForFormEditing(id, gridName) {
    var curentW = $('#' + id).width();
    $('#editmod' + gridName + '').css('width', curentW + 100);
}

//
function MergerRow(formRows, isEdit) {
    $("#Purpose").parent().attr("colspan", 6);
    $("#Process").parent().attr("colspan", 6);
    $("#Sos").parent().attr("colspan", 3);
    $("#BrandId").parent().attr("colspan", 3);

    //START ADD - SON) 8/Oct/2020
    $("#GroupLevel_1").parent().attr("colspan", 3);
    //END ADD - SON) 8/Oct/2020

    // hide row
    $("#tr_CategId").children("td.CaptionTD").eq(2).hide();
    $("#tr_CategId").children("td.DataTD").eq(2).hide();
    $("#tr_Model").children("td.CaptionTD").eq(2).hide();
    $("#tr_Model").children("td.DataTD").eq(2).hide();
    $("#Buyer").parent().attr("colspan", 3);
    $("#tr_ItemCode").children("td.CaptionTD").eq(2).hide();
    $("#tr_ItemCode").children("td.DataTD").eq(2).hide();
    //merger row for textarea.
    $("#tr_Purpose").children("td.DataTD").eq(1).attr("rowspan", "2");
    $("#tr_Purpose").children("td.DataTD").eq(2).hide();
    $("#tr_Purpose").children("td.DataTD").eq(3).hide();
    $("#tr_Purpose").children("td.DataTD").eq(4).hide();
    $("#tr_Purpose").children("td.CaptionTD").eq(1).hide();
    $("#tr_Purpose").children("td.CaptionTD").eq(2).hide();
    $("#tr_Purpose").children("td.CaptionTD").eq(3).hide();

    $("#tr_Process").children("td.DataTD").eq(1).hide();
    $("#tr_Process").children("td.DataTD").eq(2).hide();
    $("#tr_Process").children("td.DataTD").eq(3).hide();
    $("#tr_Process").children("td.DataTD").eq(4).hide();
    $("#tr_Process").children("td.CaptionTD").eq(1).hide();
    $("#tr_Process").children("td.CaptionTD").eq(2).hide();
    $("#tr_Process").children("td.CaptionTD").eq(3).hide();

    var url = "";
    if (isEdit) {
        var selRowId = $("#" + MachineGrid).jqGrid('getGridParam', 'selrow');
        url = $("#" + MachineGrid).find("tr[id='" + selRowId + "']")
            .find('td[aria-describedby="' + MachineGrid + '_ImagePath"]').find("img").attr("src");
    }
    $("#tr_Purpose").children("td.DataTD").eq(1).html('');
    $("#tr_Purpose").children("td.DataTD").eq(1).append("<div class='imgMachine'><img id= 'imgMachine' alt='' src ='" + url + "'  onerror='imgError(this);'/><div>");

}

function SetSelection2() {
    //Selection2("BrandId");
    $("#BrandId,#Buyer,#CategId,#Sos, #ProcessCode, #GroupLevel_1, #GroupLevel_2").select2({
        allowClear: true,
        width: '300px',
        height: '34px',
        placeholder: 'select..'
    });
}

function SelectedCombobox() {
    SetSelection2();
    var selRowId = $("#" + MachineGrid).jqGrid('getGridParam', 'selrow');
    var dataRow = $("#" + MachineGrid).jqGrid("getRowData", selRowId);
    var CategId = dataRow.CategId.trim();
    var category = dataRow.Category;
    var suplier = dataRow.Sos;
    var fullname = dataRow.FullName;
    var Buyer = dataRow.Buyer;
    var buyerName = dataRow.BuyerName;
    var Unit = dataRow.Unit;
    $("#Unit").val(Unit);
    $("#CategId").val(CategId);
    $("#Sos").val(suplier);
    $("#Buyer").val(Buyer);
    $("#BrandId").val(dataRow.BrandId);
    $("#select2-BrandId-container").attr("title", dataRow.Brand).html(dataRow.Brand);
    $("#select2-Buyer-container").attr("title", buyerName).html(buyerName);
    $("#select2-CategId-container").attr("title", category).html(category);
    $("#select2-Sos-container").attr("title", category).html(fullname);

    //START ADD - SON) 28/Feb/2020
    $("#ProcessCode").val(dataRow.ProcessCode).trigger('change');
    //END ADD - SON) 28/Feb/2020

    //START ADD - SON) 8/Oct/2020
    //$("#GroupLevel_0").val(dataRow.GroupLevel_0).trigger('change');
    $("#GroupLevel_0").val(dataRow.GroupLevel_0);
    eventOnChangeOpGroup(dataRow.GroupLevel_0);
    $("#MachineGroup").val(dataRow.MachineGroup);
    $("#GroupLevel_1").val(dataRow.GroupLevel_1).trigger('change');
    $("#GroupLevel_2").val(dataRow.GroupLevel_2).trigger('change');
    //END ADD - SON) 8/Oct/2020
}

//Load combobox
function AppendMcmtByCode(type) {
    var opToolConfig = {
        url: "/OpsLink/GetMasterCodeByCode",
        postData: JSON.stringify({ isMachine: type })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        //FillDataToDropDownlist("cbCategId", response, "SubCode", "CodeName");
        $("#cbCategId").empty();
        var option = '';
        option += "<option></option>"; //add empty data
        for (var i = 0; i < response.length; i++) {
            option += '<option value="' + response[i]["SubCode"] + '">' + response[i]["SubCode"] + " - " + response[i]["CodeName"] + "</option>";
        }
        $('#cbCategId').append(option);
        //Format dropdownlist to selection
        Selection2("cbCategId");
    });
}

function AppendSuppiers(type) {
    var opToolConfig = {
        url: "/OpsMasterData/GetSuppiers",
        postData: JSON.stringify({ isMachine: type })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        FillDataToDropDownlist("cbSosId", response, "Sos", "FullName");
    });
}

function AppendBrand() {
    var opToolConfig = {
        url: "/OpsMasterData/GetBrand",
        postData: {}
    };
    AjaxPostCommon(opToolConfig, function (response) {
        FillDataToDropDownlist("cbBrandId", response, "SubCode", "CodeName");
    });
}

function AppendUnit(mCode) {
    var opToolConfig = {
        url: "/Ops/GetMasterCode",
        postData: JSON.stringify({ mCode: mCode })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        //FillDataToDropDownlist("cbUnitId", response, "SubCode", "SubCode");
        $("#cbUnitId").empty();
        var option = '';
        option += "<option value=''>Select</option>";
        for (var i = 0; i < response.length; i++) {
            option += '<option value="' + response[i]["SubCode"] + '">' + response[i]["SubCode"] + "</option>";
        }
        $('#cbUnitId').append(option);
    });
}

//START ADD - SON) 28/Feb/2020 - get process code
function GetProcessCode(mCode) {
    let ajaxConfig = {
        url: "/Ops/GetMasterCode",
        postData: JSON.stringify({ mCode: mCode })
    };
    AjaxPostCommon(ajaxConfig, function (response) {
        $("#drpProcessCode").empty();
        let option = '';
        option += "<option value=''>Process Code</option>";
        for (let i = 0; i < response.length; i++) {
            option += '<option value="' + response[i]["SubCode"] + '">' + response[i]["CodeName"] + "</option>";
        }
        $('#drpProcessCode').append(option);

    });
}
//END ADD - SON) 28/Feb/2020

//START ADD - SON) 8/Oct/2020
function generateDropdownOpGroup(dropdownId, opGroupData) {
    $(dropdownId).empty();
    var option = '';
    option += "<option></option>"; //add empty data
    for (var i = 0; i < opGroupData.length; i++) {
        option += `<option value="${opGroupData[i]["OpNameId"]}">${opGroupData[i]["English"]}</option>`;
    }
    $(dropdownId).append(option);
}

function generateDropdownMachineGroup(dropdownId, opGroupData) {
    $(dropdownId).empty();
    var option = '';
    option += "<option value=''>Machine group...</option>";
    for (var i = 0; i < opGroupData.length; i++) {
        option += `<option value="${opGroupData[i]["MchGroupId"]}">${opGroupData[i]["MchGroupName"]}</option>`;
    }
    $(dropdownId).append(option);
}

//Get list Operation Group
function getOperationGroup(groupLevel, parentId) {
    var opToolConfig = {
        url: "/OpsMasterData/GetOperationGroup",
        postData: JSON.stringify({ groupLevel: groupLevel, parentId: parentId }),
        async: false
    };
    AjaxPostCommon(opToolConfig, function (response) {
        generateDropdownOpGroup('#drpOpGroup', response);
    });
}

function getMachineGroup(opNameId) {
    var opToolConfig = {
        url: "/OpName/GetMachineCategories",
        postData: JSON.stringify({ opNameId: opNameId}),
        async: false
    };
    AjaxPostCommon(opToolConfig, function (response) {
        generateDropdownMachineGroup('#MachineGroup', response);
    });
}

//Get list Opeartion Detail for dropdownlist on modal
function getOperationDetail(groupLevel, parentId) {
    var opToolConfig = {
        url: "/OpsMasterData/GetOperationGroup",
        postData: JSON.stringify({ groupLevel: groupLevel, parentId: parentId }),
        async: false
    };
    AjaxPostCommon(opToolConfig, function (response) {
        generateDropdownOpGroup('#GroupLevel_1', response);
    });
}

//Get list Opeartion Sub Group for dropdownlist on modal
function getOperationSubGroup(groupLevel, parentId) {
    var opToolConfig = {
        url: "/OpsMasterData/GetOperationGroup",
        postData: JSON.stringify({ groupLevel: groupLevel, parentId: parentId }),
        async: false
    };
    AjaxPostCommon(opToolConfig, function (response) {
        generateDropdownOpGroup('#GroupLevel_2', response);
    });
}

const eventOnChangeOpGroup = (parentId) => {
    //Get list of OpDetail base on OpGroup
    getOperationDetail("1", parentId);

    //Clear data in dropdown list OpDetail and OpSubGroup
    $('#GroupLevel_2').empty();

    getMachineGroup(parentId);
}

const eventOnChangeSubOpGroup = (parentId) => {
    //Get list of OpDetail base on OpDetail
    getOperationSubGroup("2", parentId);
}
//END ADD - SON) 8/Oct/2020

function AppendBuyerTocb() {
    var arrBuyer = GetArrayBuyer();
    $("#cbBuyerId").empty();
    var option = '';
    option += "<option></option>"; //add empty data
    for (var i = 0; i < arrBuyer.length; i++) {
        option += '<option value="' + arrBuyer[i]["BuyerCode"] + '">' + arrBuyer[i]["BuyerCode"] + " - " + arrBuyer[i]["BuyerName"] + "</option>";
    }
    $('#cbBuyerId').append(option);
}

function ChangeType(type) {
    $("#cbCategId").change(function () {
        $("#" + MachineGrid).jqGrid("setGridParam",
            {
                postData: {
                    gId: $(this).val(), machine: type
                }
            }).trigger("reloadGrid");
    });
}
// role

function GetRole() {
    if ($.isEmptyObject(UserRole)) {
        UserRole = GetUserRoleInfo("OPS", "OTM");
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

//============================================
//upload file
function ChangeImageBtn() {
    $("#ImagePath").change(function (evt) {

        //START ADD: HA
        var imageSize = $(this)[0].files[0].size; //(byte)
        if (imageSize >= 102400) {
            ShowMessage("Image", "Please choose file with the size less than 100kb ", ObjMessageType.Info);
            $("#ImagePath").val('');
        }
        //END ADD: HA

        var fileUpload = $(this).get(0).files;
        var extn = fileUpload[0].name.split('.').pop();
        var newFileName = $("#ItemCode").val() + "." + extn;
        $("#imgMachine").attr("alt", newFileName)
        // var files = fileUpload.files;
        //Preview image before upload to FTP
        readURL(this, "#imgMachine");

        //UploadFile(fileUpload);
    });
}

function UploadFile(files, id) {
    var formData = new FormData();
    //readURL(files, "#imgMachine");
    var fileimage = files.files;
    for (let i = 0; i < fileimage.length; i++) {
        formData.append(fileimage[i].name, fileimage[i]);
    }
    $.ajax({
        type: "POST",
        url: "/Upload/UploadImage?type=1&id=" + id,
        data: formData,
        contentType: false,
        processData: false,
        success: function (result) {
            ShowImageUpload(result);
        },
        error: function (ex) {
            console.log("err");
        }
    });
}

function ShowImageUpload(url) {
    var fullUrl = srcImg + url;
    $("#imgMachine").attr("src", fullUrl);
    newSrc = url;
}
