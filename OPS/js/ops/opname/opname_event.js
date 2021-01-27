//#region event
const eventClickButtonOpNamePage = () => {
    $('#btnSelectMachine').click(() => {
        let selMachine = GetSelectedOneRowData('#tbMachine');
        updateOpNameMachineId($('#hdOpNameId').val(), selMachine.ItemCode);

        if (_selectMchOpSub) {
            //Update machine for Operation Sub Group
            //Collapse and expand subgrid
            $('#tbOpName').collapseSubGridRow($('#hdGridRowId').val());
            $('#tbOpName').expandSubGridRow($('#hdGridRowId').val());
        } else {
            //Update machine for Operation Detail
            //Collapse and expand subgrid
            $($('#hdSubGridId').val()).collapseSubGridRow($('#hdGridRowId').val());
            $($('#hdSubGridId').val()).expandSubGridRow($('#hdGridRowId').val());
        }
        //Hide modal
        HideModal('mdlSelectMachine');
    });

    $('#btnSelectMchGroup').click(() => {
        let selMachine = GetSelectedOneRowData('#tbMachineGroup');
        updateOpNameMachineGroup(_opNameIdOpType, selMachine.MchGroupId);

        //reload grid opname
        ReloadJqGrid('tbOpName', {
            groupLevel: '0',
            parentId: ''
        });

        //Hide modal
        HideModal('mdlSelectMachineGroup');
    });

    $('#btnUploadIcon').click(() => {
        uploadOpNameIcon();
    });
}
//#enregion