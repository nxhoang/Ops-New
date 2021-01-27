//#region Variable
var FileTypeList = {
    Cad: "CAD File",
    Embroidery: "Embroidery Design",
    Printing: "Printing",
    Marker: "Marker File",
    Jig: "Jig",
    Other: "Others",
};

var CountryList = {
    Indonesia: "IDA",
    Myanmar: "MMR",
    Ethiopia: "ETH",
    VietNam: "VNM"
};
//#endregion

function initPage() {
    //$(document).ajaxStart($.blockUI).ajaxStop($.unblockUI);

    //Get list of Buyer
    GetMasterCodesErp("drpBuyer", BuyerMasterCode, StatusOkMasterCode);

    $("#drpBuyer").val("LLB").trigger('change');
    $("#txtAoNumber").val("AD-LLB-5554");

    //syncFile();

    selectAllFileType();
}