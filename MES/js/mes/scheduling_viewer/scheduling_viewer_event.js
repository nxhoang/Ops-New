
const eventClickButtonSchdulingViewer = () => {
    $('#btnSearchMesPkg').click(() => {
        var arrDateRange = $("#txtDateRange").val().split('-');

        var d1 = arrDateRange[0].trim();
        var d2 = arrDateRange[1].trim();

        var fac = $("#drpFactory").val();

        //Check date range
        if (isEmpty(d1) || isEmpty(d2)) {
            ShowMessage("Search style", "Please select date range", ObjMessageType.Info);
            return;
        }

        if (isEmpty(fac)) {
            ShowMessage("Search style", "Please select factory", ObjMessageType.Info);
            return;
        }

        loadMESPackage();

    });
}