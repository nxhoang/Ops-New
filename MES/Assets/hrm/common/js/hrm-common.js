var CurrentEmpsInEmpSel = [];

// Inserting options to employee select control
function InsertOptToEmpSel(data) {
    if (data) CurrentEmpsInEmpSel = data;
    $(jqTbEmp).jqGrid("clearGridData");

    const ops = [];
    let index = 0;
    for (var e of data) {
        $(jqTbEmp).jqGrid('addRowData', index += 1, e);

        let genderIcon;
        switch (e.Gender) {
        case "Male":
            genderIcon = ' ♂️ M';
            break;
        case "Female":
            genderIcon = ' ♀ F';
            break;
        default:
            genderIcon = '';
            break;
        }

        const op = {
            label: `${e.EmployeeCode}-${e.Name} ${genderIcon}`,
            title: `${e.EmployeeCode}-${e.Name} ${genderIcon}`,
            value: e.EmployeeCode
        };
        ops.push(op);
    }

    $(`#${SelEmp}`).multiselect('dataprovider', ops);
}

function InitMultiSelect(select, onChange, onSelectAll) {
    ///<summary>
    /// Initialize multi-select control
    /// <param name="select">select id</param>
    /// <param name="onChange">onchange event</param>
    /// <param name="onSelectAll">select all event</param>
    ///</summary >

    $(`#${select.selId}`).multiselect({
        includeSelectAllOption: true,
        enableFiltering: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: select.buttonWidth,
        maxHeight: select.maxHeight,
        onChange: (option, checked) => {
            onChange(checked, option[0].value);
        },
        onSelectAll: (isCheck) => {
            onSelectAll(isCheck);
        }
    });
}