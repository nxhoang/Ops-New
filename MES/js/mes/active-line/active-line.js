const ACTIVE_IMG_NAME = "iconfinder_bullet_green_35779.png";
const DEACTIVE_IMG_NAME = "iconfinder_bullet_red_35785.png";

function initActiveLineMesPage() {
    initDate();

    eventClickButton();
    eventOnChange();

    //#region Mes Active Line
    //Get list of factory
    GetFactories("drpFactory", null);
    
    $("#drpFactory").val($("#hdFactoryUser").val()).trigger('change');

    Selection2("drpDisplayOption");

    Selection2("drpShowBy");

    $("#lblDisplayOption, #divDisplayOption").hide();
    //#endregion

    //#region Active Line data receive
    GetFactories("drpFactory2", null);
    $("#drpFactory2").val($("#hdFactoryUser").val()).trigger('change');
    Selection2("drpReceiveData");
    //#endregion
}

function initDate() {
    $("#txtDate").daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        "setDate": new Date(),
        locale: {
            format: 'YYYY/MM/DD'
        }

    });
}

function GetMappingSeats(factoryId, plnStartDate, callbackFunc) {
    var config = ObjectConfigAjaxPost(
        "../ActiveLineMes/GetMappingSeats"
        , false
        , JSON.stringify({
            factoryId: factoryId, plnStartDate: plnStartDate
        })
    );
    AjaxPostCommon(config, function (resData) {
        let listLines;
        if (resData.IsSuccess) {
            listLines = resData.Data;
        } else {
            listMpdt = null;
        }
        callbackFunc(listLines);
    });
}

function GetMesPackagesById(factoryId, scheDate, callbackFunc) {
    var config = ObjectConfigAjaxPost(
        "../ActiveLineMes/GetMesPackagesByDate"
        , false
        , JSON.stringify({
            factoryId: factoryId, scheDate: scheDate
        })
    );
    AjaxPostCommon(config, function (resData) {
        let listMpdt;
        if (resData.IsSuccess) {
            listMpdt = resData.Data;
        } else {
            listMpdt = null;
        }
        callbackFunc(listMpdt);
    });
}

function createLine(lineName, imgName, mxTarget, mxIotCompleted, mxPackage = '') {

    let remainQty = 0;
    let completePer = 0;
    let divProgressBar = `<div class="progress-bar bg-color-greenLight" role="progressbar" style="width: ${completePer}%">`;

    if (mxTarget > 0) {
        remainQty = mxTarget - mxIotCompleted;
        if (remainQty < 0) remainQty = 0;

        completePer = mxIotCompleted / mxTarget * 100;
        if (completePer > 100) completePer = 100;
        completePer = Math.round(completePer);


        divProgressBar = `<div class="progress-bar bg-color-greenLight" role="progressbar" style="width: ${completePer}%" title="${mxPackage}" >${mxIotCompleted} / ${mxTarget} (${completePer}%)`;
    }

    let drLn = `<div class="row"> \
                    <div class="col-sm-1" > \
                        <img src="/img/mes/active-line/${imgName}" /> \
                        <label class="control-label">${lineName}</label> \
                     </div> \
                    <div class="col-sm-10"> \
                        <div class="progress"> \
                            ${divProgressBar}
                            </div> \
                        </div> \
                    </div> \
                </div >`;

    return drLn;
}

function createMappingSeat(lineName, imgName, connectedMch, totalSeats, mappingSeats) {

    //Calculate percentage of mapping seat
    let perMapSea = Math.round(mappingSeats / totalSeats * 100);
    if (perMapSea > 100) perMapSea = 100;

    let perRemainSea = 100 - perMapSea;

    let remainSeats = totalSeats - mappingSeats;
    if (remainSeats < 0) remainSeats = 0;

    let drmapLin = ` <div class="row"> \
                      <div class="col-sm-1" > \
                        <img src="/img/mes/active-line/${imgName}" /> \
                        <label class="control-label">${lineName}</label> \
                     </div> \
                     <div class="col-sm-10"> \
                        <div class="col-sm-1" style="padding-left:0; padding-right:0"> \
                            <div class="progress"> \
                            <div class="progress-bar bg-color-yellow" role="progressbar" data-tooltip="Connected IoT (${connectedMch})" style="width: 100%">${connectedMch}</div> \
                        </div> \
                            </div> \
                            <div class="col-sm-11" style="padding-left:0;"> \
                                <div class="progress"> \
                                    <div class="progress-bar bg-color-greenLight" data-tooltip="Mapped Seats (${perMapSea}%)" style="width: ${perMapSea}%">${mappingSeats}</div> \
                                    <div class="progress-bar bg-color-blueLight" data-tooltip="Remain Seats (${perRemainSea}%)" style="width: ${perRemainSea}%">${remainSeats}</div> \
                                </div> \
                            </div> \
                        </div> \
                    </div>`;

    return drmapLin;
}

function GetLinesByFactoryId(factoryId) {
    var config = ObjectConfigAjaxPost(
        "../ActiveLineMes/GetLinesByFactoryId"
        , false
        , JSON.stringify({
            factoryId: factoryId
        })
    );
    AjaxPostCommon(config, function (listLine) {
        //console.log(listLine);
        return listLine;
    });
}

function GetListMesPkg(factoryId, plnStartDate, isLast30Min, callbackFunc) {
    var config = ObjectConfigAjaxPost(
        "../ActiveLineMes/GetListMesPkg"
        , false
        , JSON.stringify({
            factoryId: factoryId, yyyyMMdd: plnStartDate, isLast30Min: isLast30Min
        })
    );
    AjaxPostCommon(config, function (resData) {
        let listMxPkg;
        if (resData.IsSuccess) {
            listMxPkg = resData.Data;
        } else {
            listMxPkg = null;
        }
        callbackFunc(listMxPkg);
    });
}