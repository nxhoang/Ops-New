//variables
var mqttConfig;
var mqttClient;
var choosedPackage = "";
var dataSource = "direct";
var assemblyInfo = undefined;
var tbAssemblyDashboardId = "#tbAssemblyDashboard";

function initPage() {

    //init chooseDate control, using Jquery-ui
    $("#chooseDate").datepicker({
        dateFormat: "dd/mm/yy"
    });

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


    bindDataToJqGridAssembly();

    setInterval(function () {
        RefreshData()
    }, 30000)

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
}

function RefreshData() {
    console.info(choosedPackage);
    if (!choosedPackage) return;

    GetPackageInfoAndTryConnectToMqtt();
}

function GetPackageInfoAndTryConnectToMqtt() {
    //get data first
    var config = ObjectConfigAjaxPost(
    "../MesManagement/GetAssemblySummaryAsync?mxpackage=" + choosedPackage + "&datasource=" + dataSource,
    false
    );

    AjaxGetCommon(config, function (response) {
        console.info(response);

        assemblyInfo = response.Result;

        //display latest data
        PopulatePackageInfoToDashboard(assemblyInfo);

        //connect to mqtt
        //mqttClient.subscribe(choosedPackageInfo.MQTT_TOPIC_NAME);
        //response.Result.
    });
}

function PopulatePackageInfoToDashboard(assemblyInfo) {
    $("#lblFactory").text(assemblyInfo.Package.FACTORY);
    $("#lblLine").text(assemblyInfo.Package.LINENO);
    $("#lblPackage").text(assemblyInfo.Package.MXPACKAGE);


    var assemblyDashboardGrid = $(tbAssemblyDashboardId)[0]
    console.log(assemblyDashboardGrid);
    console.log(assemblyInfo);

    assemblyDashboardGrid.addJSONData(assemblyInfo.LstAssembly);
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

    var assemblyDashboardGrid = $(tbAssemblyDashboardId)[0]

    assemblyDashboardGrid.addJSONData([]);
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
    if(!factory) return; //user must select the factory
    
    /**
     * Get lines by factory from DGS
     */
    var config = ObjectConfigAjaxPost(
        "../linedashboard/GetMesPackagesByDate",
        false,
        JSON.stringify({ factory: factory, dt: plnDate })
    );
    AjaxPostCommon(config, function (lstPackage) {

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

//Bind data to grid group packages
function bindDataToJqGridAssembly() {
    jQuery(tbAssemblyDashboardId).jqGrid({
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: 'GroupName', index: 'GroupName', width: 200, label: "MODULE", align: 'left' },
            { name: 'Target', index: 'Target', width: 150, label: "TARGET", align: 'center' },
            { name: 'Achieved', index: 'Achieved', width: 150, label: "ACHIEVED", align: 'center' },
            { name: 'Remaining', index: 'Remaining', width: 150, label: "PENDING", align: 'center' }
        ],
        rowNum: 10,
        autowidth: true,
        //width: '100%',
        shrinkToFit: true,
        scroll: false,
    });



    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

}
