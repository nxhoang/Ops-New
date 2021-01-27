var Megabyte = "MB";
var Kilobyte = "Kb";
var Success = "success";
var Fail = "fail";
var Warning = "warning";
var True = "1";
var Ops = "OPS";
var Obl = "OPM";
var Sms = "SMS";
var srcImg = "http://118.69.170.24:8005/OPS/ToolImages/";
var TimeClose = 3000;

var LimitSize = 4;

var MessageTypeError = "error";
var MessageTypeAlert = "alert";
var MessageTypeConfirm = "confirm";
var MessageTypeInfo = "info";
var MessageTypePrompt = "prompt";

//#region Objects
var ObjMessageType = {
    Error: "error",
    Alert: "alert",
    Confirm: "confirm",
    Info: "info",
    Prompt: "prompt"
};

var StatusResult = {
    Success: "success",
    Fail: "fail",
    Warning: "warning"
};

var ReportAction = {
    Success: 1,
    Duplicate: -1,
    RoleFail: -2,
    Error: -10,
    TimeOut: -20
};
// type fof db T_OP_SMSG
var eventData = {
    "Add": 'Add', "Update": 'Update', "Delete": "Delete",
    "Confirm": "Confirm", "Save": "Save", "Upload": "Upload",
    Remove: "Remove"
};
var typeData = {
    "Success": 'Success', "Warning": 'Warning', "Error": "Error", "Confirmation": "Confirmation", "Save": "Save", "NotNull": "NotNull", "RoleFail": "RoleFail"
};
// type for msgbox
var Type = {
    Success: "infor",
    Error: "error"
}

var MachineType = {
    tool: 0,
    machine: 1
};
//#endregion

//Show message box
function ShowMessage(mesTitle, mesContent, type) {
    $.msgBox({
        title: mesTitle,
        content: mesContent,
        type: type,
        autoClose: false
    });
}

function ShowAlert(mesTitle, mesContent, type) {
    ShowAlertByTime(mesTitle, mesContent, type, TimeClose)
}

function ShowAlertByTime(mesTitle, mesContent, type, timeOut) {
    $.msgBox({
        title: mesTitle,
        content: mesContent,
        showButtons: false,
        type: type,
        autoClose: true,
        timeOut: timeOut
    });
}

function example3() {
    $.msgBox({
        title: "Are You Sure",
        content: "Would you like a cup of coffee?",
        type: "confirm",
        buttons: [{ value: "Yes" }, { value: "No" }, { value: "Cancel" }],
        success: function (result) {
            if (result === "Yes") {
                alert("One cup of coffee coming right up!");
            } else {
                alert("aaaa");
            }
        }
    });
}

function ShowConfirmMessage(confContent, yesFunc, noFunc, caFunc) {
    //var conf = false;
    $.msgBox({
        title: "Are You Sure",
        content: confContent,
        type: "confirm",
        buttons: [{ value: "Yes" }, { value: "No" }, { value: "Cancel" }],
        success: function (result) {
            if (result === "Yes") {
                if (yesFunc !== null) {
                    yesFunc();
                }
            } else if (result === "No") {
                if (noFunc !== null) {
                    noFunc();
                }
            } else {
                if (caFunc !== null) {
                    caFunc();
                }
            }
        }
    });
    //return conf;
}

function ShowConfirmYesNo(title, confContent, yesFunc, noFunc) {
    $.msgBox({
        title: title,
        content: confContent,
        type: "confirm",
        buttons: [{ value: "Yes" }, { value: "No" }],
        success: function (result) {
            if (result === "Yes") {
                if (yesFunc !== null) {
                    yesFunc();
                }
            } else {
                if (noFunc !== null) {
                    noFunc();
                }
            }
        }
    });
    //return conf;
}

function ConfirmYesNo(tile, content, yesHandler, noHandler) {
    /// <summary>
    /// Shows the yes/no confirmation.
    /// </summary>
    /// <param name="tile">The tile.</param>
    /// <param name="content">The content.</param>
    /// <param name="yesHandler">The yes handler.</param>
    /// <param name="noHandler">The no handler.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    return $.msgBox({
        title: tile,
        content: content,
        type: "confirm",
        buttons: [{ value: "Yes" }, { value: "No" }],
        success: function (result) {
            if (result === "Yes") {
                if (typeof yesHandler !== "undefined") {
                    yesHandler();
                }
            } else {
                if (typeof noHandler !== "undefined") {
                    noHandler();
                }
            }
        }
    });
}

function MsgInform(title, content, type, showButtons, autoClose) {
    /// <summary>
    /// Message the inform.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="content">The content.</param>
    /// <param name="type">The type.</param>
    /// <param name="showButtons">The show buttons.</param>
    /// <param name="autoClose">The automatic close.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang

    $.msgBox({
        title: title,
        content: content,
        type: type,
        showButtons: showButtons,
        opacity: 0.9,
        autoClose: autoClose
    });
}

//Preview image
function readURL(inputFile, imgId) {
    if (inputFile.files && inputFile.files[0]) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $(imgId).attr('src', e.target.result);
        }

        reader.readAsDataURL(inputFile.files[0]);
    }
}

//Convert bytes to Megabyte
function ConvertByteToExpectedType(bytes, type) {
    var size;
    if (type === Kilobyte) {
        size = (bytes / 1024);
    } else {
        size = (bytes / 1024) / 1024;
    }

    return size.toFixed(2);
}

//Disable button
function DisabledButton(id, value) {
    $('#' + id).attr('disabled', value);
}

function HideButton(id, value) {
    if (value === true) {
        $('#' + id).hide();
    } else {
        $('#' + id).show();
    }
}

function DragFormCenter(id) {
    var winH = $(window).height();
    var winW = $(window).width();
    //Set the popup window to center
    $(id).css('top', winH / 2 - $(id).height() / 2);
    $(id).css('left', winW / 2 - $(id).width() / 2);
}

// get paramate
function getUrlParameter(sParam) {
    var sPageUrl = decodeURIComponent(window.location.search.substring(1)),
        sUrlVariables = sPageUrl.split('&'),
        sParameterName,
        i;
    for (i = 0; i < sUrlVariables.length; i++) {
        sParameterName = sUrlVariables[i].split("=");

        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
    return sParam;
};

//Only allow enter numeric
function isNumber(evt) {
    evt = (evt) ? evt : window.event;
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode > 31 && (charCode < 48 || charCode > 57)) {
        return false;
    }
    return true;
}

//Allow enter decimal point.
function isDecimalNumber(evt) {
    var charCode = (evt.which) ? evt.which : evt.keyCode;
    if (charCode !== 46 && charCode > 31
        && (charCode < 48 || charCode > 57))
        return false;

    return true;
}

//Set multiple select for selection
function MultipleSelect(idDropdownlist) {
    $("#" + idDropdownlist).multiselect({
        includeSelectAllOption: true,
        enableCaseInsensitiveFiltering: true,
        buttonWidth: '100%',
        maxHeight: 300,
        buttonClass: 'btn-multiple-select'
    });
}

//Fill data to dropdownlist
function FillDataToMultipleSelect(idDropdownlist, arrDataSource, valueField, textFiled) {
    $('#' + idDropdownlist).multiselect('destroy');
    $('#' + idDropdownlist).empty();
    var option = '';
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i][valueField] + '">' + arrDataSource[i][textFiled] + '</option>';
    }
    $('#' + idDropdownlist).append(option);

    //Format dropdownlist to selection
    MultipleSelect(idDropdownlist);
}

//Set dropdown list select2
function Selection2(idDropdownlist) {

    $("#" + idDropdownlist).select2({
        allowClear: true,
        width: '100%',
        height: '34px'
        //data: data
    });
}

//Fill data to dropdownlist
function FillDataToDropDownlist(idDropdownlist, arrDataSource, valueField, textFiled) {
    $("#" + idDropdownlist).empty();
    var option = '';
    option += "<option></option>"; //add empty data
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i][valueField] + '">' + arrDataSource[i][textFiled] + "</option>";
    }
    $('#' + idDropdownlist).append(option);

    //Format dropdownlist to selection
    Selection2(idDropdownlist);
}

const fillDataToDropdownlistAsync = (idDropdownlist, arrDataSource, valueField, textFiled) => new Promise((resolve, reject) => {
    $("#" + idDropdownlist).empty();
    var option = '';
    option += "<option></option>"; //add empty data
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i][valueField] + '">' + arrDataSource[i][textFiled] + "</option>";
    }
    $('#' + idDropdownlist).append(option);

    //Format dropdownlist to selection
    Selection2(idDropdownlist);
    resolve(true);
});

function BindDataToDdl(id, dataSource, value, text) {
    const dr = $(`#${id}`);
    dr.empty();
    var option = '';
    for (var i = 0; i < dataSource.length; i++) {
        option += `<option value="${dataSource[i][value]}">${dataSource[i][text]}</option>`;
    }
    dr.append(option);

    //Format dropdownlist as selection
    Selection2(id);
}

function FillDataToDropDownlist2(idDropdownlist, arrDataSource, valueField, textFiled) {
    $("#" + idDropdownlist).empty();
    var option = '';
    for (var i = 0; i < arrDataSource.length; i++) {
        option += '<option value="' + arrDataSource[i][valueField] + '">' + arrDataSource[i][textFiled] + '</option>';
    }
    $('#' + idDropdownlist).append(option);

    //Format dropdownlist to selection
    Selection2(idDropdownlist);
}

//Check string is null or empty
function isEmpty(str) {
    return (!str || 0 === str.length);
}

function isEmptyOrWhiteSpace(str) {
    if (!str || 0 === str.length) return true;
    else return str.trim().length === 0;
}

function checkUndefined(params) {
    if (params === "" || params === "undefined" || typeof params === "undefined") {
        return true;
    }
    return false;
}

function AjaxPost(urlVal, params, successCallback) {
    $.ajax({
        async: false,
        type: "POST",
        url: urlVal,
        data: params,
        contentType: "application/json; charset=utf-8",
        dataType: "text",
        success: successCallback,
        error: function (xhr, status, error) {
            //alert("thrownError=  " + error);
            alert("xhr.responseText= " + xhr.responseText);
            //alert("thrownError= " + error);
        }
    });
}

//Change minute to second
function ChangeMinute2Second(minutes) {
    try {
        var seconds = minutes * 60;
        return Math.round(seconds);
    }
    catch (err) {
        return Fail;
    }

}

//Convert csv to 2d array
function csvToArray(csv) {
    var rows = csv.split("\n");
    return rows.map(function (row) {
        return row.split(",");
    });
};

//Check arrary list is null or empty
function ArrayListIsNull(lstArr) {
    if (typeof lstArr != "undefined" && lstArr != null && lstArr.length > 0) {
        return false;
    } else {
        return true;
    }
}

//File
function ExceedAllowedLimitSize(files, limitSize) {
    var fileSize = ConvertByteToExpectedType(files[0].size, Megabyte);
    if (fileSize > limitSize) return true;
    return false;
}

//Check has file or not
function HasFile(files) {
    //Check file length
    if (files.length === 0) {
        return false;
    }
    return true;
}

//Check string to make sure that string is not null. 
//Return emtpy if string is null.
function ConvertString(str) {
    if (isEmptyOrWhiteSpace(str)) return "";
    if (!isNaN(str)) return str.toString().trim();
    return str.trim();
}

//Convert string to number
function ConvertStringToNumber(str) {
    if (isEmptyOrWhiteSpace(str)) return 0;
    if (!isNaN(str)) return Number(str);
    return 0;
}

//Convert string to boolean.
function StringToBoolean(string) {
    if (isEmptyOrWhiteSpace(string)) return false;

    switch (string.toLowerCase().trim()) {
        case "true": case "yes": case "1": case "Y": return true;
        case "false": case "no": case "0": case "": case null: return false;
        default: return Boolean(string);
    }
}

function ConvertBooleanToString01(boolean) {

    switch (boolean) {
        case true: return "1";
        case false: return "0";
        default: return "0";
    }
}

//Check value is in array or not -  return > -1, the value is exist in array
function IsInArray(value, array) {
    return array.indexOf(value) > -1;
}

//Convert array to object.
function ArrayToObject(arr) {
    var rv = {};
    for (var i = 0; i < arr.length; ++i)
        rv[i] = arr[i];
    return rv;
}

function ZeroPad(num, size) {
    var s = num + "";
    while (s.length < size) s = "0" + s;
    return s;
}

//#region OpsLayout functions
function formatDate(date) {
    /// <summary>
    /// Formats the date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    var monthNames = [
        "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];

    var day = date.getDate();
    var monthIndex = date.getMonth();
    var year = date.getFullYear();
    var hours = date.getHours() > 12 ? date.getHours() - 12 : date.getHours();
    var minutes = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
    var seconds = date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();
    var amPm = date.getHours() >= 12 ? "PM" : "AM";
    var time = hours + ":" + minutes + ":" + seconds + " " + amPm;

    return day + "-" + monthNames[monthIndex] + "-" + year + " " + time;
}

function IsImageFile(header) {
    /// <summary>
    /// Determines whether [is image file] [the specified header].
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    switch (header) {
        case "89504e47":
            //type = "image/png";
            return true;
        case "47494638":
            //type = "image/gif";
            return true;
        case "ffd8ffe0":
        case "ffd8ffe1":
        case "ffd8ffe2":
            //type = "image/jpeg";
            return true;
        default:
            // not accept the others
            return false;
    }
}

function IsVideoFile(header) {
    /// <summary>
    /// Determines whether [is video file] [the specified header].
    /// </summary>
    /// <param name="header">The header.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    switch (header) {
        case "0001c":
        case "00020":
            //type = "video/mp4";
            return true;
        case "00014":
        case "00018":
            //type = "video/mov";
            return true;
        default:
            // not accept the others
            return false;
    }
}

function GetFileHeader(arrayBuffer) {
    /// <summary>
    /// Gets the file header.
    /// </summary>
    /// <param name="arrayBuffer">The array buffer.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    var arr = (new Uint8Array(arrayBuffer)).subarray(0, 4);
    var header = "";
    for (var i = 0; i < arr.length; i++) {
        header += arr[i].toString(16);
    }

    return header;
}

function VisibleAllChildren(tag, visibleClass, isVisible) {
    /// <summary>
    /// Visibles all children of an element.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="visibleClass">The visible class.</param>
    /// <param name="isVisible">The is visible.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    tag.addClass(visibleClass);
    //let tags = tag.getElementsByTagName("*");
    //let total = tags.length;

    //if (isVisible) {
    //    for (let k = 0; k < total; k++) {
    //        $(tags[k]).addClass(visibleClass);
    //    }
    //} else {
    //    for (let j = 0; j < total; j++) {
    //        $(tags[j]).removeClass(visibleClass);
    //    }
    //}

    tag.each(function () {
        var $this = $(this);
        if (isVisible) {
            $this.addClass(visibleClass);
        } else {
            $this.removeClass(visibleClass);
        }
    });
}

function CheckImageURL(url) {
    return (url.match(/\.(jpeg|jpg|gif|png)$/) != null);
}

function GetFtpLink(ftpInfo, opmt) {
    const buyerCode = opmt.StyleCode.substring(0, 3);
    const link = `${ftpInfo.FtpLink}/${ftpInfo.FtpFolder}/${buyerCode}/${opmt.StyleCode}/${opmt.StyleCode}${opmt.StyleSize}` +
        `${opmt.StyleColorSerial}${opmt.RevNo}/${opmt.Edition2}/${opmt.OpRevNo}`;

    return link;
}
//#endregion

//#endregion

// #region Color boder
function ColorBorderDropdownlistMultiSelect(idName, classCss) {
    $("#" + idName).next("div.btn-group").find("button").addClass(classCss);
}

function ColorBorderDropdownlistSelect2(idName, classCss) {
    $("#" + idName).next("span").find("span.select2-selection--single").addClass(classCss);
}

function RemoveColorBorderDropdownlistSelect2(idName, classCss) {
    $("#" + idName).next("span").find("span.select2-selection--single").removeClass(classCss);
}

function RemoveColorBorderDropdownlistMultiSelect(idName, classCss) {
    $("#" + idName).next("div.btn-group").find("button").removeClass(classCss);
}

function RemoveClass(idName, classCss) {
    $("#" + idName).removeClass(classCss);
}

function ColorTextbox(idName, classCss) {
    $("#" + idName).addClass(classCss);
}

function ColorButtonBorder(idName, classCss) {
    $("#" + idName).addClass(classCss);
}
// #endregion

//VitHV
function SetReadOnlyForm(fomid, isHide, btn) {
    $(fomid).find("input").attr('readonly', isHide);
    $(fomid).find("textarea").attr('readonly', isHide);
    $(fomid).find("select").prop('disabled', isHide);
    if (isHide) $(btn).hide();
}

function AddClassForAcction(btnAct, btnClose) {
    $(btnAct).addClass("btn-primary");
    $(btnClose).addClass("btn-primary");
}

// edit form grid to standar Ops
function EditCssForm(tbid) {
    if ($('.body-dialog-jq').length) {
        $(".formdata").unwrap();
        $(".formdata").unwrap();
    }
    $("#" + tbid).wrap("<div class='body-dialog-jq'><div class='dialog-content-jq'></div></div>");
}

//Get extension of filename.
function GetExtensionFileName(filename) {
    var fileExt = filename.split('.').pop();
    fileExt = isEmpty(fileExt) ? "" : fileExt.toLowerCase();

    return fileExt;
}

//Comparison 2 arrays
function Compare2Arrays(array1, array2) {

    if ($.isEmptyObject(array1) || $.isEmptyObject(array2)) return false;

    var isSame = array1.length == array2.length && array1.every(function (element, index) {
        return element === array2[index];
    });

    return isSame;
}

function ExistsImage(image_url) {

    var http = new XMLHttpRequest();

    http.open('HEAD', image_url, false);
    http.send();

    return http.status != 404;

}

// #region Validate Email
function ValidateEmail(sEmail) {
    var filter = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
    if (filter.test(sEmail)) {
        return true;
    }
    else {
        return false;
    }
}
// #endregion
// Vithv
function imgError(image) {
    image.onerror = "";
    image.src = "../img/no-image.png";
    return true;
}

//#region Datetime
function SecondToTime(sec) {
    ///<summary>
    /// Converting second to time object
    /// <param name="sec">second</param>
    ///</summary >

    const days = Math.floor(sec / (3600 * 24)),
        hours = Math.floor((sec - days * 3600 * 24) / 3600),
        minutes = Math.floor((sec - (days * 3600 * 24 + hours * 3600)) / 60),
        seconds = Math.round(sec - (days * 3600 * 24 + hours * 3600 + minutes * 60)),
        h = hours < 10 ? `0${hours}` : hours,
        m = minutes < 10 ? `0${minutes}` : minutes,
        s = seconds < 10 ? `0${seconds}` : seconds,
        t = days > 0 ? { d: d, h: h, m: m, s: s } : { h: h, m: m, s: s };

    return t;
}

function RoundToPrecision(x, precision) {
    const y = +x + (precision === undefined ? 0.5 : precision / 2);
    return y - y % (precision === undefined ? 1 : +precision);
}
//#endregion Datetime

//#region BlockUI
const AjaxDelMessage = "<h3>Deleting...</h3>",
    AjaxLoadMdMes = "<h3>Loading data...</h3>",
    AjaxSaveMdMes = "<h3>Syncing data...</h3>",
    AjaxSavingMes = "<h3>Saving data...</h3>";
AjaxWaitingMes = "<h3>Please wait...</h3>";
//#endregion

//#region Jquery Ajax Common
function AjaxJqueryUploadFile(config, callBack) {
    const request = $.ajax({
            url: config.Url,
            type: "POST",
            data: config.Data,
            cache: false,
            contentType: false,
            processData: false,
            beforeSend: config.BeforeSend,
            statusCode: {
                404: function () {
                    console.log("page not found");
                }
            }
        })
        .done(function (response) {
            callBack(JSON.parse(response));
        })
        .fail(function (xhr) {
            console.log(config.Url + "\n" + xhr.statusText);
        })
        .always(config.Always);

    return request;
}
//#endregion Jquery Ajax Common

//#region select2
const setSelect2DropdownBelowWidth = (dropdownId, dropWidth) => {
    $(`#${dropdownId}`).on('select2:open', function (e) {
        $(`#select2-${dropdownId}-results`).parent().parent().addClass(`select2-dropdown-width-${dropWidth}`);
    });
}

const setSelect2DropdownBelowWidthList = (listDropdownId, dropWidth) => {
    listDropdownId.forEach(dropId => {
        setSelect2DropdownBelowWidth(dropId, dropWidth);
    });
}
//#endregion