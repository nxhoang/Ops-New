//variables
var mqttConfig;
var mqttClient;
var choosedPackage = "";
var dataSource = "direct";
var choosedPackageInfo = undefined;

function initPage() {

    //init chooseDate control, using Jquery-ui
    $("#chooseDate").datepicker({
        dateFormat: "dd/mm/yy"
    });

    //Get list of Buyer
    //GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);
    //GetMasterCodes("drpBuyerMdl", BuyerMasterCode, StatusOkMasterCode);

    //Event selected factory
    eventFactorySelected();

    //when select package
    eventPackageSelected();

     //get selected data source
    dataSource = $("#drpDataSource").val();

    //when select datasource
    eventDataSourceSelected();

    //
    eventDateSelected();

    //
    eventDateControlClick();

    //Init grid view style modal.
    // bindDataToJqGridStyleModal(null, null, null, null, null);

    //Set factory base on factory of user role.
    $("#drpFactoryMdl")
        .val($("#hdFactoryUser").val())
        .trigger("change");
    $("#drpFactory")
        .val($("#hdFactoryUser").val())
        .trigger("change");

    //set default date, not work yet
    //var today = moment().format('YYYY-MM-DD');
    //document.getElementById("chooseDate").value = today;

    //set current date
    setChoosedDate(new Date());


    setInterval(function () {
        RefreshData()
    }, 30000)
}

function RefreshData() {
    console.info(choosedPackage);
    if (!choosedPackage) return;

    GetPackageInfoAndTryConnectToMqtt();
}


function GetPackageInfoAndTryConnectToMqtt() {
    //get data first
    var config = ObjectConfigAjaxPost(
    "../MesManagement/GetPackageDto?mxpackage=" + choosedPackage + "&datasource=" + dataSource,
    false
   );
    AjaxGetCommon(config, function (response) {
        console.info(response);

        choosedPackageInfo = response.Result;

        //display latest data
        PopulatePackageInfoToDashboard(choosedPackageInfo);
    });
}

function PopulatePackageInfoToDashboard(package) {

    $("#lblFactory").text(package.FACTORY);
    $("#lblLine").text(package.LINENO);
    $("#lblPackage").text(package.MXPACKAGE);

    $("#lblTarget").text(package.MXTARGET);
    $("#lblAchieve").text(package.COMPLETED);
    $("#lblPending").text(package.REMAIN);
}



//#region Variable
var tblStyleAoId = "#tbStyleAo";
var tblStyleAoName = "tbStyleAo";
var papStyleAoId = "#divStyleAo";
var papStyleAoName = "divStyleAo";

var curStyleInfo;

//#endregion

//#region Functions

function resetPackage() {
    $("#lblFactory").text("---");
    $("#lblLine").text("---");
    $("#lblPackage").text("---");

    $("#lblTarget").text(0);
    $("#lblAchieve").text(0);
    $("#lblPending").text(0);



    choosedPackageInfo = undefined;
    choosedPackage = ""
}


function GetProductionTarget(plnDate, factory, lineCd, stylInf, callBackFunc) {
    var config = ObjectConfigAjaxPost(
        "../DgsTarget/GetProductionTarget",
        false,
        JSON.stringify({
            plnDate: plnDate,
            factory: factory,
            lineCd: lineCd,
            stylInf: stylInf
        })
    );
    AjaxPostCommon(config, function (schl) {
        callBackFunc(schl);
    });
}

function ReloadPackageDataSource() {
    var fac = $("#drpFactory").val();
    var curDate = getCurrentDate();
    var choosedDate = getChoosedDate();

    if (!choosedDate)
        choosedDate = curDate;

    GetPackageByFactoryAndDate(fac, choosedDate);
}

function GetPackageByFactoryAndDate(factory, plnDate) {

    console.info(factory);
    console.info(plnDate);

    if(!factory) return; //user must select the factory

    /**
     * GEt lines by factory from DGS
     */
    var config = ObjectConfigAjaxPost(
        "../linedashboard/GetMesPackagesByDate",
        false,
        JSON.stringify({ factory: factory, dt: plnDate })
    );
    AjaxPostCommon(config, function (lstPackage) {
        //$.each(lstSchl, function (index, value) {
        //    value.LineCd = value.SubCode + " - " + value.CodeName;

        console.log(lstPackage);


        var innerHtml = '';

        if (lstPackage && lstPackage.Data && lstPackage.Data.length > 0) {
            innerHtml = innerHtml.concat('<option>---</option>');
            $.each(lstPackage.Data, function (index, p) {
                innerHtml = innerHtml.concat('<option value="' + p.MxPackage + '">' + p.MxPackage + '</option>');
            });
        }

        $("#drpPackage").html(innerHtml);

    });
}

function getChoosedDate() {
    var currentDate = $("#chooseDate").datepicker("getDate");
    return currentDate;
    //return $("#chooseDate").val();
}

function setChoosedDate(date) {
    $("#chooseDate").datepicker("setDate", date);
}

function getCurrentDate() {
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
    var yyyy = today.getFullYear();

    today = yyyy + '/' + mm + '/' + dd;

    return today;


    // /**
    //  * Get current date
    //  */
    // var fullDate = new Date();
    // //convert month to 2 digits
    // var twoDigitMonth =
    //     fullDate.getMonth().length + 1 === 1
    //         ? fullDate.getMonth() + 1
    //         : "0" + (fullDate.getMonth() + 1);

    // var currentDate =
    //     fullDate.getFullYear() + "-" + twoDigitMonth + "-" + fullDate.getDate();

    // return currentDate;
}


//#endregion
