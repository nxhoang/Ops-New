function initPage() {
    //Get list of factory
    GetFactories("drpFactory", null);
    GetFactories("drpFactoryMdl", null);

    //Get list of Buyer
    GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);
    GetMasterCodes("drpBuyerMdl", BuyerMasterCode, StatusOkMasterCode);

    //Event selected factory
    eventFactorySelected();
    
    eventClickSearchStyle();

    //Init grid view style modal.
    bindDataToJqGridStyleModal(null, null, null, null, null);

    //Set factory base on factory of user role.
    $("#drpFactoryMdl").val($("#hdFactoryUser").val()).trigger('change'); 
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change'); 

    //initValueForTesting();

        
}

function initValueForTesting() {
   // $("#txtAoNumber").val("AD-KUU-0073"); for testing  server
    $("#txtAoNumber").val("AD-REI-0037"); 
    $("#txtBuyerInfo").val("REI0208"); 
    //$("#drpFactoryMdl").val('P2C1').trigger('change');

    $("#drpLine").val("P2C1_ASA003").trigger('change'); 

    var hdFac = $("#hdFactoryUser").val();
    console.log(hdFac);

}

//#region Variable

var tblStyleAoId = "#tbStyleAo";
var tblStyleAoName = "tbStyleAo";
var papStyleAoId = "#divStyleAo";
var papStyleAoName = "divStyleAo";

var curStyleInfo;

//#endregion

//#region Bindata to gridview

//Bind data to grid group packages
function bindDataToJqGridStyleModal(aoNo, buyer, factory, buyerInfo) {    
    jQuery(tblStyleAoId).jqGrid({
        url: '/DgsTarget/GetAoStyles',
        postData: {
            aoNo: aoNo, buyer: buyer, factory: factory, buyerInfo: buyerInfo
        },
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'AdNo', index: 'AdNo', width: 120, label: "AD No", classes: 'pointer' },
            { name: 'BuyerStyleName', index: 'BuyerStyleName', width: 150, label: "Buyer", classes: 'pointer' },
            { name: 'StyleCode', index: 'StyleCode', width: 100, label: "Style Code", align: 'center', classes: 'pointer' },
            { name: 'StyleSize', index: 'StyleSize', width: 70, label: "Style Size", align: 'center', classes: 'pointer' },
            { name: 'StyleColorWays', index: 'StyleColorWays', width: 200, label: "Color", align: 'left', classes: 'pointer' },
            { name: 'RevNo', index: 'RevNo', width: 70, label: "Rev No", align: 'center', classes: 'pointer' },
            { name: 'DeliveryDate', index: 'DeliveryDate', width: 120, label: "Delivery Date", align: 'center', classes: 'pointer', formatter: 'date', formatoptions: { srcformat: 'd/m/Y', newformat: 'd/m/Y' } },
            { name: 'Destination', index: 'Destination', width: 70, label: "Destination", align: 'center', classes: 'pointer' },
            { name: 'AdQty', index: 'AdQty', width: 70, label: "Ad Qty", align: 'center', classes: 'pointer' },
            { name: 'Factory', index: 'Factory', width: 100, label: "Factory", align: 'center', classes: 'pointer' },
            { name: 'StatusName', index: 'StatusName', width: 200, label: "Status", align: 'left', classes: 'pointer' },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true }
        ],
        rowNum: 10,
        rowList: [10, 20, 30],
        pager: papStyleAoId,
        sortname: 'id',
        toolbarfilter: true,
        viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        gridComplete: function () {

        },
        loaderror: function (xhr, status, err) {
            alert("error - get group package: " + err);
        },
        onSelectRow: function (rowid) {
            var rowdata = $(tblStyleAoId).jqGrid("getRowData", rowid);

            curStyleInfo = rowdata.StyleCode + "." + rowdata.StyleSize + rowdata.StyleColorSerial + rowdata.RevNo;
         
            //var fullDate = new Date();
            ////convert month to 2 digits
            //var twoDigitMonth = ((fullDate.getMonth().length + 1) === 1) ? (fullDate.getMonth() + 1) : '0' + (fullDate.getMonth() + 1);

            //var currentDate = fullDate.getFullYear() + "-" + twoDigitMonth + "-" + fullDate.getDate();

            var curDate = getCurrentDate();

            curDate = "2019-01-30";

            var factory = $("#drpFactoryMdl").val();

            var lineCd = $("#drpLineMdl").val();

            console.log(curStyleInfo + ' - ' + curDate + ' - ' + factory);
            
            // '2019-01-17' , 'P2C1', 'REI0207.SMA002001', 'PG90'
            //currentDate = "2019-01-17";
            //factory = "P2C1";
            //curStyleInfo = "REI0207.SMA002001";

            GetProductionTarget(curDate, factory, lineCd, curStyleInfo, function (schl) {

                $("#lblTarget").text(schl.TrgtQty);
                $("#lblAchieve").text(schl.Data);
                $("#lblPending").text(schl.Pending);
                
                console.log(schl.TrgtQty + ' - ' + schl.Data + ' - ' + schl.Pending);
            });
            
        },
        loadcomplete: function () {

        },
        //autowidth: true,
        width: null,
        shrinkToFit: false,
        scroll: false,

    });


    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

}

//#endregion

//#region Functions
function GetProductionTarget(plnDate, factory, lineCd, stylInf, callBackFunc) {
    var config = ObjectConfigAjaxPost("../DgsTarget/GetProductionTarget", false
        , JSON.stringify({ plnDate: plnDate, factory: factory, lineCd: lineCd, stylInf: stylInf }));
    AjaxPostCommon(config, function (schl) {
        callBackFunc(schl);
    });
}

function GetLinesByFactoryDgs(controlId, factory, plnDate) {
    /**
     * GEt lines by factory from DGS
     */
    var config = ObjectConfigAjaxPost("../DgsTarget/GetLinesByFactoryDgs", false
        , JSON.stringify({ factory: factory, plnDate: plnDate }));
    AjaxPostCommon(config, function (lstSchl) {
        //$.each(lstSchl, function (index, value) {
        //    value.LineCd = value.SubCode + " - " + value.CodeName;
        //});
        FillDataToDropDownlist(controlId, lstSchl, "LineCd", "LineCd");
    });
}

function getCurrentDate() {
    /**
     * Get current date 
     */
    var fullDate = new Date();
    //convert month to 2 digits
    var twoDigitMonth = ((fullDate.getMonth().length + 1) === 1) ? (fullDate.getMonth() + 1) : '0' + (fullDate.getMonth() + 1);

    var currentDate = fullDate.getFullYear() + "-" + twoDigitMonth + "-" + fullDate.getDate();

    return currentDate;
}

//#endregion
