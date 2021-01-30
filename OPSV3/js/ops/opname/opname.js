var _groupLevel = {
    Level_0: '0',
    Level_1: '1',
    Level_2: '2'
};

var _selectMchOpSub = true;
var _opNameIdOpType = '';
var _uploadIconOp = 0;
let _operationsRole = null;

$(() => {
    initScriptOpNamePage();
});

const initScriptOpNamePage = () => {
    GetUserRoleInfoAsync(SystemIdOps, 'OPN', userRole => {
        console.log('userRole', userRole);
        _operationsRole = userRole;
        bindDataOnGridOpName(_groupLevel.Level_0, '');
        bindDataOnGridMachineCategories('');
        bindDataOnGridMachine('', '', '');
    });

    eventClickButtonOpNamePage();
}

//#region bind data on gridview
const bindDataOnGridOpName = (groupLevel, parentId) => {
    jQuery("#tbOpName").jqGrid({
        url: '/OpName/GetOpNames',
        postData: {
            groupLevel: groupLevel,
            parentId: parentId
        },
        datatype: "json",
        gridview: true,
        colModel: [
            { name: "Code", index: "Code", label: 'Code', width: 70, align: 'center' },
            { label: ' ', align: 'center', width: 70, formatter: showOpNameIconOperationLevel0 },
            { name: "OpNameId", index: "OpNameId", label: 'OPNAME ID', hidden: true },
            { name: "English", index: "English", label: 'Operation Type', width: 1000 },
            { name: 'UploadIcon', index: 'UploadIcon', label: ' ', width: 100, align: 'center', formatter: uploadOpIcon },
            { name: "IconLink", index: "IconLink", hidden: true },
            { name: "GroupLevel", index: "GroupLevel", hidden: true },
            { name: "ParentId", index: "ParentId", hidden: true },
            { name: "HasChild", index: "HasChild", hidden: true }
        ],
        rowNum: 100,
        //autowidth: true,
        shrinkToFit: false,
        width: null,
        height: 350,
        onSelectRow: function (rowid) {
            //Hide machine category gridview
            $('#divMachineCate').show();
            //get selected row data
            let dtRow = $('#tbOpName').jqGrid("getRowData", rowid);

            //Reload machine category gridview
            ReloadJqGrid('tbMachineCat', { opNameId: dtRow.OpNameId });
        },
        gridComplete: function () {
            let ids = jQuery("#tbOpName").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbOpName").jqGrid("getRowData", i);
                if (rowdata.HasChild === "N") {
                    //Hide plus icon if item has no pattern
                    $(`tr[id=${i}]>td[aria-describedby$=tbOpName_subgrid]`).html("&nbsp;");

                    //Disable click event on the first column
                    $(`tr[id=${i}]>td[aria-describedby$=tbOpName_subgrid]`).unbind('click');
                }
            }
        },
        subGrid: true,
        subGridRowExpanded: getOpSubGroups,
    });

    checkOperationPageRole();

    //Resize girdview
    $(window).on('resize', function () {
        var wd = $(window).height() - 200;
        $('#tbOpName').setGridHeight(wd);
    }).trigger('resize');

}

const checkOperationPageRole = () => {
    if (_operationsRole.IsUpdate === '1') {
        jQuery("#tbOpName").showCol("UploadIcon");
    } else {
        jQuery("#tbOpName").hideCol("UploadIcon");
    }
}

function getOpSubGroups(subgrid_id, row_id) {
    let opGroupDtRow = jQuery('#tbOpName').jqGrid('getRowData', row_id);

    let subgrid_table_id, pager_id;
    subgrid_table_id = subgrid_id + "_t";
    pager_id = "p_" + subgrid_table_id;
    //keep subgrid and row id
    $('#hdGridRowId').val(row_id);
    $('#hdSubGridId').val('#' + subgrid_table_id);
    $("#" + subgrid_id).html("<table id='" + subgrid_table_id + "' class='scroll td-opsub'></table><div id='" + pager_id + "' class='scroll'></div>");
    jQuery("#" + subgrid_table_id).jqGrid({
        url: '/OpName/GetOpNames',
        postData: {
            groupLevel: _groupLevel.Level_1,
            parentId: opGroupDtRow.OpNameId
        },
        datatype: "json",
        gridview: true,
        colModel: [
            { name: "Code", index: "Code", label: 'Code', width: 60 },
            { name: "English", index: "English", label: 'Operation Sub', width: 500 },
            { name: "ItemName", index: "ItemName", label: 'Machine', width: 350 },
            { name: "SelectMachine", index: "SelectMachine", label: ' ', width: 60, align: 'center', formatter: selectMachineOpSub },
            { label: ' ', width: 100, align: 'center', formatter: showOpNameIcon },
            { name: "UploadOpIcon", index: "UploadOpIcon", label: ' ', width: 60, align: 'center', formatter: uploadOpIcon },
            { name: "IconLink", index: "IconLink", hidden: true },
            { name: "MachineId", index: "MachineId", hidden: true },
            { name: "GroupLevel", index: "GroupLevel", hidden: true },
            { name: "ParentId", index: "ParentId", hidden: true }, /*OpNameId of GroupLevel_0*/
            { name: "HasChild", index: "HasChild", hidden: true },
            { name: "OpNameId", index: "OpNameId", label: 'OPNAME ID', hidden: true }
        ],
        rowNum: 100,
        height: 'auto',
        //autowidth: true,
        shrinkToFit: false,
        width: null,
        onSelectRow: function (rowid) {
            //get selected row data
            let dtRow = $('#' + subgrid_table_id).jqGrid("getRowData", rowid);
            //Set OpnameId in hidden field
            $('#hdOpNameId').val(dtRow.OpNameId);
            //Hide machine category gridview
            $('#divMachineCate').hide();
            //Clear machine category
            ReloadJqGrid('tbMachineCat', { opNameId: '' });
        },
        gridComplete: function () {
            let ids = jQuery("#" + subgrid_table_id).jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#" + subgrid_table_id).jqGrid("getRowData", i);
                if (rowdata.HasChild === "N") {
                    //Hide plus icon if item has no pattern
                    $(`tr[id=${i}]>td[aria-describedby$=${subgrid_table_id}_subgrid]`).html("&nbsp;");

                    //Disable click event on the first column
                    $(`tr[id=${i}]>td[aria-describedby$=${subgrid_table_id}_subgrid]`).unbind('click');
                }
            }
        },
        subGrid: true,
        subGridRowExpanded: getOpDetails,
    });

    checkRoleForGridLevel1(subgrid_table_id);
}

const checkRoleForGridLevel1 = (gridId) => {
    if (_operationsRole.IsUpdate === '1') {
        jQuery(`#${gridId}`).showCol("UploadOpIcon");
        jQuery(`#${gridId}`).showCol("SelectMachine");
    } else {
        jQuery(`#${gridId}`).hideCol("UploadOpIcon");
        jQuery(`#${gridId}`).hideCol("SelectMachine");
    }
}

function getOpDetails(subgrid_id, row_id) {

    var opDetailDtRow = $(this).jqGrid('getRowData', row_id);

    let subgrid_table_id, pager_id;
    subgrid_table_id = subgrid_id + "_t";
    pager_id = "p_" + subgrid_table_id;
    $("#" + subgrid_id).html("<table id='" + subgrid_table_id + "' class='scroll td-opsubsub'></table><div id='" + pager_id + "' class='scroll'></div>");
    jQuery("#" + subgrid_table_id).jqGrid({
        url: '/OpName/GetOpNamesOpDetail',
        postData: {
            groupLevel: _groupLevel.Level_2,
            parentId: opDetailDtRow.OpNameId
        },
        datatype: "json",
        gridview: true,
        colModel: [
            { name: "Code", index: "Code", label: 'Code', width: 60 },
            { name: "English", index: "English", label: 'Operation Detail', width: 500 },
            { name: "Vietnam", index: "Vietnam", label: 'Operation Detail', hidden: true },
            { name: "Indonesia", index: "Indonesia", label: 'Operation Detail', hidden: true },
            { name: "Myanmar", index: "Myanmar", label: 'Operation Detail', hidden: true },
            { name: "Ethiopia", index: "Ethiopia", label: 'Operation Detail', hidden: true },
            { name: "OpNameId", index: "OpNameId", label: 'OPNAME ID', hidden: true },
            { name: "ItemName", index: "ItemName", label: 'Machine', width: 350 },
            { name: "SelectMachine", index: "SelectMachine", label: ' ', width: 60, align: 'center', formatter: selectMachineOpDetail },
            { name: "GroupLevel", index: "GroupLevel", hidden: true },
            { name: "ParentId", index: "ParentId", hidden: true }, /*OpNameId of GroupLevel_1*/
            { name: "MachineId", index: "MachineId", hidden: true },
            { name: "GroupLevel_0", index: "GroupLevel_0", hidden: true }
        ],
        rowNum: 100,
        height: '100%',
        autowidth: true,
        onSelectRow: function (rowid) {
            //get selected row data
            let dtRow = $('#' + subgrid_table_id).jqGrid("getRowData", rowid);
            //Set OpnameId in hidden field
            $('#hdOpNameId').val(dtRow.OpNameId);
            //Hide machine category gridview
            $('#divMachineCate').hide();
            //Clear machine category
            ReloadJqGrid('tbMachineCat', { opNameId: '' });
        }
    });

    checkRoleForGridLevel2(subgrid_table_id);
}

const checkRoleForGridLevel2 = (gridId) => {
    if (_operationsRole.IsUpdate === '1') {
        jQuery(`#${gridId}`).showCol("SelectMachine");
    } else {
        jQuery(`#${gridId}`).hideCol("SelectMachine");
    }
}

const bindDataOnGridMachineCategories = (opNameId) => {
    jQuery('#tbMachineCat').jqGrid({
        rowNum: 1000,
        autowidth: true,
        height: 200,
        url: "/OpName/GetMachineCategories",
        datatype: "json",
        postData: {
            opNameId: opNameId
        },
        colModel: [
            { name: "MchGroupName", index: "MchGroupName", label: 'Group Name' },
            { name: "MchGroupId", index: "MchGroupId", hidden: true },
            { name: "OpNameId", index: "OpNameId", hidden: true }
        ]
    });
}

const bindDataOnGridMachine = (opType, opSub, opDetail) => {
    jQuery('#tbMachine').jqGrid({
        rowNum: 1000,
        //autowidth: true,
        width: null,
        shrinkToFit: false,
        height: 300,
        url: "/OpName/GetMachines",
        datatype: "json",
        postData: {
            opType: opType,
            opSub: opSub,
            opDetail: opDetail
        },
        colModel: [
            { name: "ItemCode", index: "ItemCode", label: 'Machine Code', width: 150 },
            { name: "ItemName", index: "ItemName", label: 'Machine Name', width: 250 },
            { name: "Model", index: "Model", label: 'Model', width: 150 },
            { name: "BrandName", index: "BrandName", label: 'Brand', width: 150 },
            { name: "CategoryName", index: "CategoryName", label: 'Category', width: 150 },
            { name: "GroupLevel_0_Name", index: "GroupLevel_0_Name", label: 'OpType', width: 150 },
            { name: "GroupLevel_1_Name", index: "GroupLevel_1_Name", label: 'OpSub', width: 150 },
            { name: "GroupLevel_2_Name", index: "GroupLevel_2_Name", label: 'OpDetail', width: 150 },
            { name: "MchGroupName", index: "MchGroupName", label: 'Group Machine', width: 150 },
            { name: "CategId", index: "CategId", hidden: true },
            { name: "BrandId", index: "BrandId", hidden: true },
            { name: "GroupLevel_0", index: "GroupLevel_0", hidden: true },
            { name: "GroupLevel_1", index: "GroupLevel_1", hidden: true },
            { name: "GroupLevel_2", index: "GroupLevel_2", hidden: true },
            { name: "MachineGroup", index: "MachineGroup", hidden: true }
        ]
    });
}

const bindDataOnGridMachineGroup = (opNameId) => {
    jQuery('#tbMachineGroup').jqGrid({
        rowNum: 1000,
        width: null,
        shrinkToFit: false,
        height: 200,
        url: "/OpName/GetMachineCategories",
        datatype: "json",
        postData: {
            opNameId: opNameId
        },
        colModel: [
            { name: "MchGroupName", index: "MchGroupName", label: 'Group Name', width: 300 },
            { name: "MchGroupId", index: "MchGroupId", hidden: true },
            { name: "OpNameId", index: "OpNameId", hidden: true }
        ]
    });
}
//#endregion

//#region formatter
function selectMachineGroup(cellvalue, options, rowObject) {
    return `<i class='icon-note' style='cursor: pointer; color: #42a5f5' onclick='showModalMachineGroup(${rowObject.OpNameId})'></i>`;
}

function selectMachineOpSub(cellvalue, options, rowObject) {
    return `<i class='icon-note' style='cursor: pointer; color: #42a5f5' onclick='showModalMachineOpSub(${rowObject.ParentId}, ${rowObject.OpNameId})'></i>`;
}

function uploadOpIcon(cellvalue, options, rowObject) {
    //If grid id is table operation name then set upload icon is 0
    _uploadIconOp = options.gid === 'tbOpName' ? 0 : 1;
    return `<i class='icon-cloud-upload' style='cursor: pointer; color: #42a5f5' onclick='showModalUploadOpIcon(${rowObject.OpNameId})'></i>`;
}

function selectMachineOpDetail(cellvalue, options, rowObject) {
    return `<i class='icon-note' style='cursor: pointer; color: #42a5f5' onclick='showModalMachineOpDetail(${rowObject.GroupLevel_0}, ${rowObject.ParentId}, ${rowObject.OpNameId})'></i>`;
}

function showOpNameIconOperationLevel0(cellvalue, options, rowObject) {
    if (!$.isEmptyObject(rowObject.IconName)) {
        return `<img style="width:45px; height:45px;" src="${rowObject.IconLink}"/>`;
    }

    return '';
}

function showOpNameIcon(cellvalue, options, rowObject) {
    if (!$.isEmptyObject(rowObject.IconName)) {
        return `<img style="width:60px; height:30px;" src="${rowObject.IconLink}"/>`;
    }

    return '';
}
//#endregion

//#region show modal
const showModalMachineGroup = opNameId => {
    _opNameIdOpType = opNameId;
    //Reload machine group gridview
    ReloadJqGrid('tbMachineGroup', { opNameId: opNameId });
    //show modal
    ShowModal('mdlSelectMachineGroup');
}

const showModalMachineOpSub = (groupLevel_0, groupLevel_1) => {
    _selectMchOpSub = true;
    //Keep current machine id
    $('#hdMachineId').val(groupLevel_1);
    //reload gridview machine
    ReloadJqGrid('tbMachine', {
        opType: groupLevel_0,
        opSub: groupLevel_1,
        opDetail: ''
    });

    //reload machine list base on 
    ShowModal('mdlSelectMachine');
}

const showModalUploadOpIcon = (opNameId) => {
    //Keep current opname id
    $('#hdOpNameIdUploadIcon').val(opNameId);
    //reload machine list base on 
    ShowModal('mdlUploadIcon');
}

const showModalMachineOpDetail = (groupLevel_0, groupLevel_1, groupLevel_2) => {
    _selectMchOpSub = false;
    //Keep current machine id
    $('#hdMachineId').val(groupLevel_2);
    //reload gridview machine
    ReloadJqGrid('tbMachine', {
        opType: groupLevel_0,
        opSub: groupLevel_1,
        opDetail: groupLevel_2
    });

    //reload machine list base on 
    ShowModal('mdlSelectMachine');
}
//#endregion

//#region funtions
const updateOpNameMachineId = (opNameId, machineId) => {
    var config = ObjectConfigAjaxPost("/OpName/UpdateOpNameMachine", true, JSON.stringify({ opNameId: opNameId, machineId: machineId }));

    AjaxPostCommon(config, function (respone) {
        if (respone.IsSuccess) {
            ShowMessage('Update Machine', respone.Result, MessageType.Success);
        } else {
            ShowMessage('Update Machine', respone.Log, MessageType.Error);
        }
    });
}

const updateOpNameMachineGroup = (opNameId, machineGroup) => {
    var config = ObjectConfigAjaxPost("/OpName/UpdateOpNameMachineGroup", true, JSON.stringify({ opNameId: opNameId, machineGroup: machineGroup }));
    AjaxPostCommon(config, function (respone) {
        if (respone.IsSuccess) {
            ShowMessage('Update Machine', respone.Result, MessageType.Success);
        } else {
            ShowMessage('Update Machine', respone.Log, MessageType.Error);
        }
    });
}

const uploadOpNameIcon = () => {

    var files = $("#filOpIcon")[0].files;

    var data = new FormData();
    data.append(files[0].name, files[0]);
    data.append('OpNameId', $('#hdOpNameIdUploadIcon').val());

    $.ajax({
        url: "/OpName/UploadOpNameIcon",
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.IsSuccess) {
                //Clear input file field
                $('#filOpIcon').val('');
                HideModal('mdlUploadIcon');

                if (_uploadIconOp === 0) {
                    //Reload grid process name
                    ReloadJqGrid('tbOpName', {
                        groupLevel: _groupLevel.Level_0,
                        parentId: ''
                    })
                } else {
                    //Reload sub grid
                    //Collapse and expand subgrid
                    $('#tbOpName').collapseSubGridRow($('#hdGridRowId').val());
                    $('#tbOpName').expandSubGridRow($('#hdGridRowId').val());
                }

                ShowMessage("Upload Icon", response.Result, MessageType.Success);
            } else {
                ShowMessage("Upload Icon", response.Log, MessageType.Error);
            }
        },
        error: function (err) {
            ShowAjaxError(err, "/OpName/UploadOpNameIcon");
        }
    });
}
//#endregion