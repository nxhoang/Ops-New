
function eventClickButtonWIP() {
    $("#btnExcelWorkingProcess").click(function () {

        //Get current mes package
        let objMesPackage = GetSelectedOneRowData("#" + tableMesPackageId);

        let mesPackage = objMesPackage.MxPackage;

        //Check mes package is empty or white space
        if (isEmptyOrWhiteSpace(mesPackage)) {
            ShowMessage("Export working process", "Please select MES package", ObjMessageType.Alert);
            return;
        }

        //Get type of WIP
        let getType = $("#rdOpGroupWIP").is(':checked') === true ? WIPTYPE.OpGroup : WIPTYPE.Module;

        var config = ObjectConfigAjaxPost(
            "../ExcelMes/ExportWorkingProcess"
            , false
            , JSON.stringify({
                mesPackage: mesPackage, getType: getType
            })
        );

        AjaxPostCommon(config, function (resIns) {
            if (resIns.fileName !== "") {
                //Download excel file if export it successfully
                window.location.href = `../ExcelMes/Download/?file=${resIns.fileName}`;
            } else {
                ShowMessage("Export work in process", resIns.errorMessage, ObjMessageType.Error);
            }
        });
    });

    $("#rdModuleWIP").change(function () {
        //Get WIP by module
        //Get selected row data
        let selMesPkg = GetSelectedOneRowData("#" + tableMesPackageId);
        let getType = $(this).is(':checked') === true ? WIPTYPE.Module : WIPTYPE.OpGroup;

        //Reload work in process by mes package
        let params = { mesPkg: selMesPkg.MxPackage, getType: getType };
        ReloadJqGrid2LoCal(tableWIPId, params);
    });

    $("#rdOpGroupWIP").change(function () {
        //Get WIP by OP Group
        //Get selected row data
        let selMesPkg = GetSelectedOneRowData("#" + tableMesPackageId);
        let getType = $(this).is(':checked') === true ? WIPTYPE.OpGroup : WIPTYPE.Module;

        //Reload work in process by mes package
        let params = { mesPkg: selMesPkg.MxPackage, getType: getType };
        ReloadJqGrid2LoCal(tableWIPId, params);
    });
}
