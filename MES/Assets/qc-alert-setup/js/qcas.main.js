const UriApiCstp = `${document.location.origin}/api/ApiCstp`, BtnAddUser = "btnAddUser", DivSelBuyer = "divSelBuyer",
    DivSelFactory = "divSelFactory";

(() => {
    getCorporations(UriApiCstp);

    window.InitMultiSelect({ selId: SelFactory, buttonWidth: 300, buttonHeight: 300 }, (checked, checkedOpt) => {
        //if (checked) {
        //} else {
        //}
    }, (isCheck) => {
        //const searchInput = document.querySelectorAll(`#${SelFactory} input`)[0];

        //if (searchInput && searchInput.value && searchInput.value !== "") {
        //    // Searching
        //    console.log(searchInput.value);
        //} else {
        //    if (isCheck) {
        //    } else {
        //    }
        //}
    });

    window.InitMultiSelect({ selId: SelBuyer, buttonWidth: 300, buttonHeight: 300 }, (checked, checkedOpt) => {

    }, (isCheck) => {

    });

    GetBuyer("Buyer", "OK");
    //InitPkUserJqGrid("shit");
    //TestDa("testing");
    //InitJqGridUser();
    InitPkUserJqGrid();
    AddUser();
})();

function getSelectedBuyer() {
    const selectedBuyers = document.getElementById(DivSelBuyer).querySelectorAll("li.active"),
        buyers = [];

    selectedBuyers.forEach((e) => {
        const opt = e.getElementsByTagName("input")[0];
        buyers.push({
            FactoryId: opt.value,
            FactoryName: opt.labels[0].textContent
        });
    });

    return buyers;
}

function getSelectedFactory() {
    const selectedFactories = document.getElementById(DivSelFactory).querySelectorAll("li.active"),
        factories = [];

    selectedFactories.forEach((e) => {
        const opt = e.getElementsByTagName("input")[0];
        factories.push({
            FactoryId: opt.value,
            FactoryName: opt.labels[0].textContent
        });
    });

    return factories;
}

function AddUser() {
    document.getElementById(BtnAddUser).addEventListener('click', (e) => {
        //console.log(getSelectedBuyer());

        console.log(getSelectedFactory());

        //$(jqTbUsmtUser).jqGrid("setGridParam", { postData: { "factories": getSelectedBuyer() } });
        const config = new AjaxConfig("/QCAlertSetup/GetPkUserByFactories", true, JSON.stringify({ "factories": getSelectedFactory() }));

        AjaxPostCommon(config, (res) => {
            console.log(res);
        });

        //$(jqTbUsmtUser).jqGrid("clearGridData");

        //$(`#${SelEmp} option:selected`).map((index, item) => {
        //    const emp = window.CurrentEmpsInEmpSel.find(v => v.EmployeeCode === item.value);
        //    $(jqTbEmp).jqGrid('addRowData', index + 1, emp);
        //    return emp;
        //});
    });
}