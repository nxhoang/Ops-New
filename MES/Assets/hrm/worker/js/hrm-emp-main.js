const BtnSyncData = "btnSyncData";

(() => {
    GetDeptTeam();
    GetEmpNoImage();

    document.getElementById(BtnSyncData).addEventListener('click', () => {
        window.BulkInsertDeptTeam();
    });

    document.getElementById(BtnSyncUserImg).addEventListener('click', () => {
        window.SyncEmpImg();
    });

    document.getElementById(BtnSyncUserImgByCondition).addEventListener('click', () => {
        $.blockUI(AjaxWaitMes);

        const depts = [], deptNames = [], positions = [];

        for (var j of $(`#${SelSection} option:selected`)) {
            depts.push({ DeptCode: j.value});
        }

        for (var k of $(`#${SelPos} option:selected`)) {
            positions.push(k.value);
        }

        console.log(depts);
        console.log(deptNames);
        console.log(positions);

        if (depts.length > 0) {
            window.SyncEmpImgByCond({ depts, deptNames, positions });
        } else {
            MsgInform("Error", "Please select factory and team.", "error", true, true);
            $.unblockUI();
        }
    });

    getCorporations(UriApiCstp);

    // Loading all of factories
    getDeptCodesByFactory();

    window.InitMultiSelect({ selId: SelFactory, buttonWidth: 300, buttonHeight: 300 }, (checked, checkedOpt) => {
        // Ex: factory is PKS2-A at K-Tech side and PKS2A at ERP side so need to remove hyphen
        //const selectedFactory = checkedOpt.replace(/-/g, '');

        console.log(checkedOpt);
        //console.log(allSections);

        if (checked) {
            if (checkedOpt) {
                //const sections = allSections.filter(x => x.PkName === checkedOpt);
                console.log(allSections.filter(x => x.PkName === checkedOpt));
                window.FilteredSections = window.FilteredSections.concat(allSections.filter(x => x.PkName === checkedOpt));
            }
        } else {
            window.FilteredSections = window.FilteredSections.filter(x => x.PkName !== checkedOpt);
        }
        insertOptToSectionSel(window.FilteredSections);
    }, (isCheck) => {
        const searchInput = document.querySelectorAll(`#${SelFactory} input`)[0];

        if (searchInput && searchInput.value && searchInput.value !== "") {
            // Searching
            console.log(searchInput.value);
        } else {
            if (isCheck) {
                insertOptToSectionSel(allSections);
            } else {
                insertOptToSectionSel([]);
            }
        }
    });

    // Section dropdown list
    window.InitMultiSelect({ selId: SelSection, buttonWidth: 300, buttonHeight: 300 }, (checked, checkedOpt) => {
        if (checked) {
            if (checkedOpt) {
                const selSections = window.FilteredSections.filter(x => x.DeptCode === checkedOpt);
                window.SelectedSections = window.SelectedSections.concat(selSections);
            }
        } else {
            window.SelectedSections = window.SelectedSections.filter(x => x.DeptCode !== checkedOpt);
        }

        console.log(window.SelectedSections);

        getEmployees(currentCorp, window.SelectedSections);
    }, (isCheck) => {
        const searchInput = document.querySelectorAll(`#${DivSelSection} input`)[0];

        if (searchInput && searchInput.value && searchInput.value !== "") {
            console.log(searchInput.value);
        } else {
            if (isCheck) {
                getEmployees(currentCorp, window.FilteredSections);
            } else {
                window.InsertOptToEmpSel([]);
            }
        }
    });

    window.InitMultiSelect({ selId: SelPos, buttonWidth: 300, buttonHeight: 300 }, (checked, checkedOpt) => {
        if (checked) {
            if (checkedOpt) {
                const emps = checkedOpt === "None-Position" ? currentEmps.filter(x => x.Position.trim() === "") :
                    currentEmps.filter(x => x.Position === checkedOpt);

                window.FilteredEmpsByPos = window.FilteredEmpsByPos.concat(emps);
            }
        } else {
            if (checkedOpt === "None-Position") {
                window.FilteredEmpsByPos = window.FilteredEmpsByPos.filter(x => x.Position.trim() !== "");
            } else {
                window.FilteredEmpsByPos = window.FilteredEmpsByPos.filter(x => x.Position !== checkedOpt);
            }
        }
        window.InsertOptToEmpSel(window.FilteredEmpsByPos);
    }, (isCheck) => {
        console.log(isCheck);

        const searchInput = document.querySelectorAll(`#${SelPos} input`)[0];

        if (searchInput && searchInput.value && searchInput.value !== "") {
            // Searching
            console.log(searchInput.value);
        } else {
            window.InsertOptToEmpSel(currentEmps);
        }
    });

    window.InitMultiSelect({ selId: SelSkill, buttonWidth: 300, buttonHeight: 300 }, (checked, checkedOpt) => {
        console.log(checkedOpt);
    }, () => { });

    console.log(SelEmp);

    // Employee dropdown list
    window.InitMultiSelect({ selId: SelEmp, buttonWidth: 300, buttonHeight: 300 }, (checked, checkedOpt) => {
        console.log(checkedOpt);
    }, () => { });

    insertOptToPosSel(getEmpPos());
    document.getElementById(SearchEmpForm).addEventListener("submit", submitSearchEmpForm);
    document.getElementById(RdMale).addEventListener("change", genderClickEvent);
    document.getElementById(RdFemale).addEventListener("change", genderClickEvent);

    InitJqGrid();
    CloseEmpPopup();

    document.getElementById(BtnSyncEmpNfcId).addEventListener("click", SyncEmpNfcId);
})();