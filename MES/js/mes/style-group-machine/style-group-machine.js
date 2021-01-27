function initStyleGroupMachinePage() {
    eventButtonClick();

    eventDropdownlistChange();

    GetMasterCodeOracle("drpStyleGroup", "StyleGroup", null, null, null);
    GetMasterCodeOracle("drpSubGroup", null, null, null, null);
    GetMasterCodeOracle("drpSubSubGroup", null, null, null, null);

    GetCategoryMachineTool("drpCategory", "1");

    multilSelect("drpMachine");
    multilSelect("drpMainMachine");

    bindDataToJqGridStyleGroupMachine(null, null, null);
}

//Bind data to grid group packages
function bindDataToJqGridStyleGroupMachine(styleGroup, subGroup, subSubGroup) {

    jQuery("#tbMachineGroup").jqGrid({
        url: '/StyleGroupMachine/GetStyleGroupMachines',
        postData: {
            styleGroup: styleGroup, subGroup: subGroup, subSubGroup: subSubGroup
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'MAINMACHINE', index: 'MAINMACHINE', width: 100, label: "Main Machine", align: 'center' }, //, formatter: formatMainMachine
            { name: 'STYLEGROUPNAME', index: 'STYLEGROUPNAME', width: 160, label: "Style Group", classes: 'pointer' },
            { name: 'SUBGROUPNAME', index: 'SUBGROUPNAME', width: 160, label: "Sub Group", align: 'left', classes: 'pointer' },
            { name: 'SUBSUBGROUPNAME', index: 'SUBSUBGROUPNAME', width: 160, label: "Sub Sub Group", align: 'left', classes: 'pointer' },
            { name: 'MACHINEID', index: 'MACHINEID', width: 120, label: "MachineId", align: 'left', classes: 'pointer' },
            { name: 'MACHINENAME', index: 'MACHINENAME', width: 160, label: "Machine Name", align: 'left', classes: 'pointer' },
            { name: 'REGISTERNAME', index: 'REGISTERNAME', width: 160, label: "Register", align: 'left', classes: 'pointer' },
            { name: 'REGISTRYDATE', index: 'REGISTRYDATE', width: 120, label: "Registry Date", align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "m-d-Y", newformat: "Y-m-d" } },           
            { name: 'STYLEGROUP', index: 'STYLEGROUP', hidden: true },
            { name: 'SUBGROUP', index: 'SUBGROUP', hidden: true },
            { name: 'SUBGROUP', index: 'SUBGROUP', hidden: true }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        pager: "#divMachineGroup",
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        width: null,
        shrinkToFit: false,
        loadonce: true,
        gridComplete: function () {

        },
        loaderror: function (xhr, status, err) {
            alert("error - get group package: " + err);
        },
        onSelectRow: function (rowid) {
            const dataRow = $("#tbMachineGroup").jqGrid("getRowData", rowid);

        },
        loadcomplete: function () {

        }
        
    });

    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    //$(window).on('resize.jqGrid', function () {
    //    $("#tbMachineGroup").jqGrid('setGridWidth', $("#content").width());
    //});
    
    //function formatMainMachine(cellvalue, options, rowObject) {
    //    return rowObject.MAINMACHINE;
    //}
}

function reloadJqGridStyleGroupMachine() {
    let styleGroup = $("#drpStyleGroup").val();
    let subGroup = $("#drpSubGroup").val();
    let subSubGroup = $("#drpSubSubGroup").val();

    var params = { styleGroup: styleGroup, subGroup: subGroup, subSubGroup: subSubGroup };
    ReloadJqGrid2LoCal("tbMachineGroup", params);
}

function fillDataToMultiSelect(arrDataSource, dropdownlistId) {
    $('#' + dropdownlistId).multiselect('destroy');
    $('#' + dropdownlistId).empty();

    var option = '';
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i]["ItemCode"] + '">' + arrDataSource[i]["ItemName"] + '</option>';
    }

    $('#' + dropdownlistId).append(option);

    multilSelect(dropdownlistId);

}

function fillDataToDropdownMachine(arrDataSource, idDropdownlistMachine) {
    //drpMachine
    $('#' + idDropdownlistMachine).multiselect('destroy');
    $('#' + idDropdownlistMachine).empty();
    var a = $('#' + idDropdownlistMachine).val();
    var option = '';
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i]["ItemCode"] + '">' + arrDataSource[i]["ItemName"] + '</option>';
    }

    $('#' + idDropdownlistMachine).append(option);
    
    multilSelectDrpMachine("drpMachine");

}

function multilSelectDrpMachine(dropdownlistId) {
    $("#" + dropdownlistId).multiselect({
        enableCaseInsensitiveFiltering: true,
        buttonWidth: '100%',
        maxHeight: 300,
        buttonClass: 'btn-multiple-select',
        onDropdownHidden: function () {
            let arrMachineVal = $("#" + dropdownlistId).val();
            
            if (!ArrayListIsNull(arrMachineVal)) {
                let arrMc = [];
                $.each(arrMachineVal, function (idx, machineId) {
                    let mcName = $("#" + dropdownlistId + " option[value='" + machineId + "']").text();
                    arrMc.push({
                        ItemCode: machineId,
                        ItemName: mcName
                    });
                });

                fillDataToMultiSelect(arrMc, "drpMainMachine");
                
            }

        }
    });
}

function multilSelect(dropdownlistId) {
    $("#" + dropdownlistId).multiselect({
        //includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: '100%',
        maxHeight: 300,
        buttonClass: 'btn-multiple-select'
    });
}