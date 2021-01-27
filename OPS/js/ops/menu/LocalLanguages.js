function ChangeLocalLang(arrgridName) {
    $("#drpLanguages").change(function () {
        var selLanId = $("#drpLanguages").val();
        var objOpsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));
        if (!$.isEmptyObject(objOpsMaster)) {
            var dataKey = {
                styleCode: objOpsMaster.StyleCode,
                styleSize: objOpsMaster.StyleSize,
                styleColor: objOpsMaster.StyleColorSeiral,
                revNo: objOpsMaster.RevNo,
                opRevNo: objOpsMaster.OpRevNo,
                edition: objOpsMaster.Edition,
                languageId: selLanId
            };
            arrgridName.forEach(function (a) {
                ReloadJqGrid(a, dataKey);
            });
            //orther function inteface
        }
    });
}
