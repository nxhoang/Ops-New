function eventButtonClick() {
    $("#btnSearch").click(function () {

        reloadJqGridStyleGroupMachine();
    });
        
    $("#btnAdd").click(function () {

        if (!isValidData()) return;

        let listMainMc = $("#drpMainMachine").val();

        let listMachine = $("#drpMachine").val();
        let styleGroup = $("#drpStyleGroup").val();
        let subGroup = $("#drpSubGroup").val();
        let subSubGroup = $("#drpSubSubGroup").val();
        let loginId = $("#hdUserId").val();
        let listSgmc = [];

        subGroup = isEmpty(subGroup) ? "000" : subGroup;
        subSubGroup = isEmpty(subSubGroup) ? "000" : subSubGroup;
            
        $.each(listMachine, function (idx, machineId) {

            //Check main machine
            let isMainMc = 0;
            $.each(listMainMc, function (idx, mcId) {
                if (mcId === machineId) {
                    isMainMc = 1;
                    return false;
                }
            });

            let smgc = {
                STYLEGROUP: styleGroup,
                SUBGROUP: subGroup,
                SUBSUBGROUP: subSubGroup,
                MACHINEID: machineId,
                MAINMACHINE: isMainMc,
                REGISTERID: loginId
            };

            listSgmc.push(smgc);
        });

        var config = ObjectConfigAjaxPost("../StyleGroupMachine/AddStyleGroupMachines", true, JSON.stringify({ listSgmc: listSgmc }));
        AjaxPostCommon(config, function (respone) {
            if (respone.IsSuccess) {
                ShowMessage("Adding machine", respone.Result, ObjMessageType.Info);
                reloadJqGridStyleGroupMachine();
            } else {
                ShowMessage("Adding machine", respone.Log, ObjMessageType.Alert);
            }

        });
    });
}

//Check data is valid before insert to database
function isValidData() {
    if (isEmpty($("#drpStyleGroup").val())) {
        ShowMessage("Adding machine", "Please select Style Group", ObjMessageType.Alert);
        return false;
    }

    if (ArrayListIsNull($("#drpMachine").val())) {
        ShowMessage("Adding machine", "Please select machine", ObjMessageType.Alert);
        return false;
    }

    return true;
}

function eventDropdownlistChange() {

    //Dropdownlist style group change.
    $("#drpStyleGroup").change(function () {
        let stlGroup = this.value;
        GetMasterCodeOracle("drpSubGroup", "StyleSubGroup", null, stlGroup, null);

        $("#drpSubSubGroup").empty();

        //$("#drpSubSubGroup").val("").trigger('change');
    });

    //Dropdownlist sub group change
    $("#drpSubGroup").change(function () {
        let stlGroup = $("#drpStyleGroup").val();
        let subSubGroup = this.value;
        GetMasterCodeOracle("drpSubSubGroup", "StyleSubSubGroup", null, stlGroup, subSubGroup);
    });

    //Dropdownlist category change
    $("#drpCategory").change(function () {
        let categoryId = this.value;
        var config = ObjectConfigAjaxPost("../StyleGroupMachine/GetMachines", true, JSON.stringify({ categoryId: categoryId }));
        AjaxPostCommon(config, function (respone) {
           
            fillDataToDropdownMachine(respone.Result, "drpMachine");

        });
    });
}