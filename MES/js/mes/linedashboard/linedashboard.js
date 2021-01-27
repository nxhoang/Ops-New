//variables
var mqttConfig;
var mqttClient;
var choosedPackage = "";
var dataSource = "direct";
var choosedPackageInfo = undefined;
var ReportBy = "FA";
var ReportByDesc = "By Final Assembly";

function initPage() {

    //init chooseDate control, using Jquery-ui
    //$("#chooseDate").datepicker({
    //    dateFormat: "dd/mm/yy"
    //});

    $("#chooseDate").daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1901,
        maxYear: parseInt(moment().format('YYYY'), 10)
    }, function (start, end, label) {
        
    });

    //Event selected factory
    eventFactorySelected();

    //when select package
    eventPackageSelected();

     //get selected data source
    dataSource = $("#drpDataSource").val();

    //when select datasource
    eventDataSourceSelected();

    //2020-12-16 Tai Le(Thomas): dropdown ReportBy
    eventReportBySelected();

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

    //START ADD - SON) 5/Sep/2020
    GetLineByFactoryId('drpLine', $("#hdFactoryUser").val());
    eventLineSelection();
    //END ADD - SON) 5/Sep/2020

    setInterval(function () {
        RefreshData()
    }, 30000)

    Selection2("drpFactory");
    Selection2("drpPackage");
    Selection2("drpDataSource");
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

    if (ReportBy === "FA") {
        //Based on Final Assembly
        $("#lblAchieve").text(package.COMPLETED);
        $("#lblPending").text(package.REMAIN);
        $("#lblDefect").text(package.TOTAL_DEFECT);
    } else if (ReportBy === "QA") {
        //Based on End_line QC
        let pendingQty = 0;
        if (package.ELQC_Total > (package.MXTARGET + package.ELQC_Defects))
            pendingQty = package.ELQC_Total - (package.MXTARGET + package.ELQC_Defects);
        $("#lblAchieve").text(package.ELQC_Total);
        $("#lblPending").text(pendingQty);
        $("#lblDefect").text(package.ELQC_Defects);
    }
}

//START ADD - SON) 5/Sep/2020
function GetStyleInformation(mxPackage) {
    //get data first
    var config = ObjectConfigAjaxPost(
        "../LineDashboard/GetMxPackageInfo", false,
        JSON.stringify({ mxPackage: mxPackage })
    );
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            //Load style information on line dashboard screen
            ShowStyleInfoOnLineDashboard(response.Result);
        } else {
            console.info("Failuer: " + response.Log +". MxPackage: " + mxPackage);
        }        
    });
}

function ShowStyleInfoOnLineDashboard(stlInf) {

    var div = document.getElementById("tableLbl");
    div.innerHTML = "";

    var tbl = document.createElement("TABLE");
    tbl.setAttribute("id", "myTable");
    div.appendChild(tbl);

    var table = document.getElementById("myTable");
    var row = table.insertRow(0);
    var row1 = table.insertRow(1);
    var row2 = table.insertRow(2);
    var row3 = table.insertRow(3);

    var cell1 = row.insertCell(0);
    var cell2 = row1.insertCell(0);
    var cell3 = row2.insertCell(0);
    var cell4 = row3.insertCell(0);

    cell1.innerHTML = '<i class="fa fa-asterisk"></i>' + stlInf.StyleCode + ' - ' + stlInf.StyleName;
    cell2.innerHTML = stlInf.StyleColorSerial + ' - ' + stlInf.StyleColorways;
    cell3.innerHTML = stlInf.BuyerStyleName;
    cell4.innerHTML = stlInf.AONo;
    
    //$("#lblStyleName").html(`<label class="stl-inf-value" id="lblStyleName"><i class="fa fa-asterisk"></i> ${stlInf.StyleCode} - ${stlInf.StyleName}</label>`);
    //$('#lblStyleColor').html(`<label class="stl-inf-value" id="lblStyleName"><i class="fa fa-asterisk"></i> ${stlInf.StyleColorSerial} - ${stlInf.StyleColorways}</label>`);
    //$('#lblBuyerStyleName').html(`<label class="stl-inf-value" id="lblStyleName"><i class="fa fa-asterisk"></i> ${stlInf.BuyerStyleName}</label>`);
    //$('#lblAONumber').html(`<label class="stl-inf-value" id="lblStyleName"><i class="fa fa-asterisk"></i> ${stlInf.AONo}</label>`);

    //$('#lblStyleName').text(stlInf.StyleCode + ' - ' + stlInf.StyleName);
    //$('#lblStyleColor').text(stlInf.StyleColorSerial  + ' - ' +  stlInf.StyleColorways); 
    //$('#lblBuyerStyleName').text(stlInf.BuyerStyleName);
    //$('#lblAONumber').text(stlInf.AONo);

    //Set source for image
    $("#imgStyleImage").attr("src", stlInf.ImageLink);
}

//END ADD - SON) 5/Sep/2020

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

    //START MOD - SON) 5/Sep/2020
    //GetPackageByFactoryAndDate(fac, choosedDate);
    let line = $('#drpLine').val();
    if (!line) {
        GetPackageByFactoryAndDate(fac, choosedDate);
    } else {
        GetPackagesByLine(fac, line, choosedDate);
    }
    //END MOD - SON) 5/Sep/2020
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

        if (lstPackage && lstPackage.Result && lstPackage.Result.length > 0) {
            innerHtml = innerHtml.concat('<option>---</option>');
            $.each(lstPackage.Result, function (index, p) {
                innerHtml = innerHtml.concat('<option value="' + p.MxPackage + '">' + p.MxPackage + '</option>');
            });
        }

        $("#drpPackage").html(innerHtml);

    });
}

function getChoosedDate() {
    //START MOD - SON) 7/Sep/2020    
    //var currentDate = $("#chooseDate").datepicker("getDate");
    //return currentDate;

    let currentDate = $('#chooseDate').data('daterangepicker').startDate._d;
    return currentDate;
    //END MOD - SON) 7/Sep/2020
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

//START ADD - SON) 5/Sep/2020
function GetPackagesByLine(factory, line, plnDate) {

    if (!factory) return; //user must select the factory

    var config = ObjectConfigAjaxPost(
        "../linedashboard/GetMesPackagesByLine",
        false,
        JSON.stringify({ factory: factory, line: line, dt: plnDate })
    );
    AjaxPostCommon(config, function (lstPackage) {
     
        var innerHtml = '';

        if (lstPackage && lstPackage.Result && lstPackage.Result.length > 0) {
            innerHtml = innerHtml.concat('<option>---</option>');
            $.each(lstPackage.Result, function (index, p) {
                innerHtml = innerHtml.concat('<option value="' + p.MxPackage + '">' + p.MxPackage + '</option>');
            });
        }

        $("#drpPackage").html(innerHtml);

    });
}
//END ADD - SON) 5/Sep/2020
//#endregion
