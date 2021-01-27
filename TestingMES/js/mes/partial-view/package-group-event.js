//Event click on button search production package
function eventClickBtnSearchPkgGroup() {
    $("#btnSearchExePackage").click(function () {

        var factoryId = $("#drpFactory").val();
        if (!factoryId) {
            ShowMessage("Show Group Package", "Please select Factory", ObjMessageType.Info);
            return;
        }

        var arrDateRange = $('#txtDateRange').val().split('-');
        var startDate = $.trim(arrDateRange[0].replace(/\//g, ''));
        var endDate = $.trim(arrDateRange[1].replace(/\//g, ''));
        var buyer = $("#drpBuyer").val();
        var buyerInfo = $("#txtBuyerInfo").val();
        var aoNo = $("#txtAONo").val();

        reloadGridPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo);

        eventSearchPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo);
        
    });
}

function reloadGridPackageGroup(factory, plnStartDate, plnEndDate, buyer, buyerInfo, aoNo) {

    var params = { factory: factory, plnStartDate: plnStartDate, plnEndDate: plnEndDate, buyer: buyer, buyerInfo: buyerInfo, aoNo: aoNo };
    ReloadJqGrid2LoCal(tableGroupPackageName, params);
}
