const SelFactory = "selFactory", SelDept = "selDept", SelPos = "selPos", SelSkill = "selSkill",
    SelSection = "selSection", DivSelSection = "divSelSection", SelEmp = "selEmp",
    RdMale = "rdMale", RdFemale = "rdFemale",
    UriApiCstp = `${document.location.origin}/api/ApiCstp`,
    UriApiFactory = `${document.location.origin}/api/ApiFactory`,
    UrlGetDeptCodesByFactory = "/MesLineAllocation/GetDeptCodesByFactory", UrlGetSections = "",
    UrlGetEmployees = "/Employee/GetEmployees",
    AjaxWaitMes = "<h3>Please wait...</h3>", SearchEmpForm = "searchEmpForm";
var selectedFactoryCodes = [], currentFactories = [], allSections = [], FilteredSections = [], SelectedSections = [],
    selectedEmpPos = [], currentCorp = null, currentEmps = [], FilteredEmpsByPos = [];

function genderClickEvent(e) {
    ///<summary>
    /// Clicking gender checkbox event
    /// <param name="e">current element</param>
    ///</summary >

    const isMaleChecked = document.getElementById(RdMale).checked,
        isFemaleChecked = document.getElementById(RdFemale).checked;

    if (isMaleChecked && isFemaleChecked || !isMaleChecked && !isFemaleChecked) {
        window.InsertOptToEmpSel(currentEmps);
    } else {
        const emps = currentEmps.filter(x => x.Gender === e.currentTarget.value);
        window.InsertOptToEmpSel(emps);
    }
}

function getEmployees(corp, depts, deptNames, positions) {
    ///<summary>
    /// Getting list of employees by department code(known as section or line)
    /// sections: list of selected sections
    ///</summary>

    const config = new AjaxShortHandConfig(AjaxLoadMdMes, UrlGetEmployees, { corp, depts, deptNames, positions });
    AjaxPostShortHand(config, (res) => {
        if (res.IsSuccess && res.Result) {
            currentEmps = res.Result;
            window.InsertOptToEmpSel(res.Result);

            $(jqTbEmp).jqGrid("clearGridData").jqGrid("setGridParam", { data: res.Result, datatype: "local" }).trigger("reloadGrid");
            //if (res.Result.length > 0 && !res.Result[0].ImageName) {
                //document.getElementById(BtnSyncData).style.display = res.Result.length > 0 ? "none" : "block";
                //window.SyncEmpImg();
            //}
        } else {
            console.log(res.Log);
        }
    });
}

function getEmpPos() {
    const posArr = [
        { Code: 0, Name: "None-Position" },
        { Code: 1, Name: "Assistant Manager" },
        { Code: 2, Name: "Assistant Staff" },
        { Code: 3, Name: "Chief Manager" },
        { Code: 4, Name: "Manager" },
        { Code: 5, Name: "Normal Staff" },
        { Code: 6, Name: "Normal Worker" },
        { Code: 7, Name: "Professional Staff" },
        { Code: 8, Name: "Simple Worker" },
        { Code: 9, Name: "Supervisor" }];

    return posArr;
}

function getCorporations(uri) {
    ///<summary>
    /// List of corporations is storage in t_cm_cstp table that configure for each country.
    ///</summary >

    $.getJSON(uri).done((data) => {
        console.log("dedicated");
        console.log(data);

        if (data && data.length > 0 && data[0].CorpCode) {
            currentCorp = data[0].HrmCorpCode;
            getFactoriesByCorporation(UriApiFactory, data[0].CorpCode, 1);
        } else {
            console.log("There is not Corporation");
            console.log(data);
        }
    });
}

function insertOptToPosSel(data) {
    const ops = [];
    for (var e of data) {
        const op = {
            label: e.Name,
            title: e.Name,
            value: e.Name
        };
        ops.push(op);
    }

    $(`#${SelPos}`).multiselect('dataprovider', ops);
}

function insertOptToFacSel(data) {
    const ops = [];
    for (var e of data) {
        const op = {
            label: e.FactoryName,
            title: e.FactoryId,
            value: e.FactoryName
        };
        ops.push(op);
    }

    $(`#${SelFactory}`).multiselect('dataprovider', ops);
}

function getFactoriesByCorporation(uri, corporation, desDatabase) {
    const l = `${uri}/?corporation=${corporation}&tenantId=${desDatabase}`;
    $.getJSON(l).done(function (res) {
        if (res) {
            insertOptToFacSel(res);
            currentFactories = res;
        } else {
            console.log("There is no factory.");
        }
    });
}

function submitSearchEmpForm(e) {
    ///<summary>Clicking OK button to filter employee</summary>

    e.preventDefault();

    console.log("Clicking OK button...");

    $(jqTbEmp).jqGrid("clearGridData");

    $(`#${SelEmp} option:selected`).map((index, item) => {
        const emp = window.CurrentEmpsInEmpSel.find(v => v.EmployeeCode === item.value);
        $(jqTbEmp).jqGrid('addRowData', index + 1, emp);
        return emp;
    });
}

function getDeptCodesByFactory() {
    ///<summary>Getting departments (known as section or line) by current factory
    /// factory: current factory that selected at top dropdown list for searching package
    ///</summary >

    $.blockUI({ message: AjaxWaitMes });

    $.post(UrlGetDeptCodesByFactory).done((res) => {
        //console.log("getDeptCodesByFactory");
        //console.log(res);
        if (res.IsSuccess && res.Result) {
            allSections = res.Result;

            //document.getElementById(BtnSyncData).style.display = allSections.length > 0 ? "none" : "block";
        } else {
            console.error(res.Log);
        }
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always(() => {
            $.unblockUI();
        });
}

function getSections() {
    $.blockUI({ message: AjaxWaitMes });

    $.getJSON(UrlGetSections).done((res) => {
        if (res.IsSuccess && res.Result) console.log(res.Result);
    })
        .fail((err) => {
            console.log(`${err.statusText} ${err.status}`);
        })
        .always(() => {
            $.unblockUI();
        });
}

// Inserting options to section select control
function insertOptToSectionSel(data) {
    const ops = [];
    for (var e of data) {
        const op = {
            label: e.Section,
            title: e.FullName,
            value: e.DeptCode
        };
        ops.push(op);
    }

    $(`#${SelSection}`).multiselect('dataprovider', ops);
}