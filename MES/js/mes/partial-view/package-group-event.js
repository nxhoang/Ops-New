//Event click on button search production package
function eventClickBtnSearchPkgGroup() {
    $("#btnSearchExePackage").click(function () {

        let factoryId = $("#drpFactory").val();

        console.log(`selected factory: ${factoryId}`);

        if (!factoryId) {
            ShowMessage("Show Group Package", "Please select Factory", ObjMessageType.Info);
            return;
        }

        let arrDateRange = $('#txtDateRange').val().split('-');
        let startDate = $.trim(arrDateRange[0].replace(/\//g, ''));
        let endDate = $.trim(arrDateRange[1].replace(/\//g, ''));
        let buyer = $("#drpBuyer").val();
        let buyerInfo = $("#txtBuyerInfo").val();
        let aoNo = $("#txtAONo").val();

        let filterStartDate = $("#drpFilterBy").val();
        
        reloadGridPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo, filterStartDate);

        eventSearchPackageGroup(factoryId, startDate, endDate, buyer, buyerInfo, aoNo);  

    });
}

function reloadGridPackageGroup(factory, plnStartDate, plnEndDate, buyer, buyerInfo, aoNo, filterStartDate) {

    var params = { factory: factory, plnStartDate: plnStartDate, plnEndDate: plnEndDate, buyer: buyer, buyerInfo: buyerInfo, aoNo: aoNo, filterStartDate: filterStartDate };
    ReloadJqGrid2LoCal(tableGroupPackageName, params);
}