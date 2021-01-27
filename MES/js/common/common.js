var Megabyte = "MB";
var Kilobyte = "Kb";
var Success = "success";
var Failure = "failure";
var Fail = "fail";
var Warning = "warning";
var NoAuthority = "noauthority";
var True = "1";

var TimeClose = 3000;

var LimitSize = 4;

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
    Failure: "failure",
    Warning: "warning"
};

var ReportAction = {
    Success: 1,
    Duplicate: -1,
    RoleFail: -2,
    Error: -10,
    TimeOut: -20
};

//#endregion

//#region Show message box
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

function ShowConfirmMessageWithReturnData(mesTitle, mesContent, callback) {
    var ret;
    $.msgBox({
        title: (mesTitle) ? mesTitle : 'Confirm',
        content: mesContent,
        type: "confirm", /* alert ; info ; error ; confirm ; prompt ; <else> considered as alert */
        autoClose: false,
        buttons: [{ value: "Yes", Cssclass: "" }, { value: "No" }],
        success: function (result) {
            if (result == "Yes") {
                ret = true;
            }
            if (result == "No") {
                ret = false;
            }
        },
        afterClose: function () {
            if (typeof callback == 'function') {
                callback.call(this, ret);
            };
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

function ConfirmYesNo(title, content, yesHandler, noHandler) {
    /// <summary>
    /// Shows the yes/no confirmation.
    /// </summary>
    /// <param name="tile">The title.</param>
    /// <param name="content">The content.</param>
    /// <param name="yesHandler">The yes handler.</param>
    /// <param name="noHandler">The no handler.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    return $.msgBox({
        title: title,
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

//#endregion

//#region modal
function ShowModalDragable(modalId) {
    $("#" + modalId).modal('show');
    //Enable drag modal
    $("#" + modalId).draggable({
        handle: ".modal-header"
    });
}
//#endregion

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
            alert("xhr.responseText= " + xhr.responseText);
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
        return Failure;
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
        case "true": case "yes": case "1": return true;
        case "false": case "no": case "0": case null: return false;
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

//#region Jqgrid

//Reload Jq Grid 
function ReloadJqGrid(tableJqGridName, data) {
    $("#" + tableJqGridName).jqGrid('setGridParam', {
        dataType: "json",
        postData: data
    }).trigger('reloadGrid');
}

//Reload using for loadonce = true;
function ReloadJqGrid2LoCal(tableJqGridName, dataPost) {
    $("#" + tableJqGridName).setGridParam({ datatype: 'json' });
    $("#" + tableJqGridName).jqGrid('setGridParam', {
        serializeGridData: function (postData) {
            //return JSON.stringify({ strSearchingText: "C00423" });
            return dataPost;
        }
        //postData: data
    }).trigger('reloadGrid');
}

function ReloadJqGridLocal(tableJqGridName, data) {
    $("#" + tableJqGridName).jqGrid('clearGridData');
    $("#" + tableJqGridName).jqGrid('setGridParam', {
        dataType: "local",
        data: data
    }).trigger('reloadGrid');
}

// #region Get selected row

//Get seleted row data on jqgrid opdetail (process)
function GetSelectedMultipleRowsData(gridId) {
    var myGrid = $(gridId);
    var selRowIds = myGrid.jqGrid('getGridParam', 'selarrrow');
    var lstSelectedRow = [];

    if ($.isEmptyObject(selRowIds)) return null;

    //var edition = $("#hdOpDetailEdition").val();
    for (var i = 0, n = selRowIds.length; i < n; i++) {
        var rowData = myGrid.jqGrid("getRowData", selRowIds[i]);
        lstSelectedRow.push(rowData);
    }
    return lstSelectedRow;
}

//Selected one row on jqgrid
function GetSelectedOneRowData(gridId) {
    var myGrid = $(gridId);
    var selRowId = myGrid.jqGrid("getGridParam", "selrow");
    var rowData = myGrid.jqGrid("getRowData", selRowId);

    return rowData;
}

//Get all rows on jqgrid
function GetAllRowsDataJqGrid(jqGridId) {
    var gridData = $(jqGridId).getGridParam('data');
    return gridData;
}

//Get all rows on jqgrid
function GetAllRowsDataJqGrid2(jqGridId) {
    var gridData = jQuery(gridOpsDetailId).jqGrid("getRowData");
    return gridData;
}
// #endregion

//#endregion

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

//#region Common Ajax 

//Create object config for Ajax.
function ObjectConfigAjaxPost(url, asynchronous, postData) {
    var config = {
        url: url,
        async: asynchronous,
        postData: postData
    };
    return config;
}

class AjaxConfig {
    constructor(url, async, postData) {
        this.url = url;
        this.async = async;
        this.postData = postData;
    }
}

class AjaxShortHandConfig {
    constructor(message, url, data) {
        this.Message = message;
        this.Url = url;
        this.Data = data;
    }
}

function AjaxPostShortHand(config, callBack) {
    ///<summary>$.post ajax jquery post shorthand</summary>
    ///<param name="config">Including Message (blockUI), Url, Data</param>

    $.blockUI({ message: config.Message });
    return $.post(config.Url, config.Data).done((response) => {
        let result;
        if (response.error === null || response.error === undefined) {
            result = response;
        } else {
            result = false;
            console.log(response.error);
        }

        callBack(result);
    }).always(() => { $.unblockUI(); });
}

function AjaxPostCommon(config, callback) {
    /// <summary>
    /// Ajax post - This is common function to post data to server.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang

    const request = $.ajax({
        cache: false,
        type: "POST",
        async: config.async,
        url: config.url,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: config.postData
    }).done(function (response) {
        callback(response);
    }).fail(function (xhr) {
        ShowAjaxError(xhr, config.url);
    });

    return request;
}

function GenericAjaxPost(config) {
    /// <summary>
    /// Ajax post - This is common function to post data to server.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang

    const request = $.ajax({
        cache: false,
        type: "POST",
        async: config.async,
        url: config.url,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: config.postData
    });

    return request;
}

function HandleException(xhr, status, err) {
    switch (xhr.status) {
        case 500:
            MsgInform("Inform", "Could not load data. Please contact admin.", MessageTypeError, true, true);
            console.log(xhr.responseJSON);
            break;
        case 404:
            MsgInform("Inform", "Could not connect to center data. Please check the connection.", MessageTypeError, true, true);
            console.log(xhr.responseJSON);
            break;
        default:
            MsgInform("Inform", "An error occurred. Please contact admin.", MessageTypeError, true, true);
            console.log(err);
            break;
    }
}

function AjaxGetCommon(config, callback) {
    /// <summary>
    /// Ajax post - This is common function to post data to server.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="callback">The callback.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang

    const request = $.ajax({
        cache: false,
        type: "GET",
        url: config.url,
        async: config.async
    }).done(function (response) {
        callback(response);
    }).fail(function (xhr) {
        ShowAjaxError(xhr, config.url);
    });

    return request;
}

function ShowAjaxError(error, url) {
    /// <summary>
    /// Shows the ajax error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="url">The URL.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang

    if (error.status !== undefined) {
        window.ShowMessage("Error " + error.status, error.statusText + "<br>" + url, window.ObjMessageType.Error);
    } else {
        window.ShowMessage(error.name, error.message, window.ObjMessageType.Error);
    }
}

//#endregion

//#region Get Date
function getCurrentDate(intDay = 0) {
    var Temp = new Date();

    var today = new Date();
    today.setDate(Temp.getDate() + intDay);


    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();
    if (dd < 10) {
        dd = '0' + dd;
    }
    if (mm < 10) {
        mm = '0' + mm;
    }
    today = yyyy + '/' + mm + '/' + dd;
    return today;
}

function formatDateYYYYMMDD() {
    Date.prototype.yyyymmddhyphen = function () {
        var mm = this.getMonth() + 1; // getMonth() is zero-based
        var dd = this.getDate();

        return [this.getFullYear(),
        (mm > 9 ? '' : '0') + mm,
        (dd > 9 ? '' : '0') + dd
        ].join('-');
    };
}
//#endregion

//#region Color
function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}
//#endregion

//#region
function getFormData($form, pFormatType = 'json') {
    var unindexed_array = $form.serializeArray();
    /* Reference: https://stackoverflow.com/questions/3029870/jquery-serialize-does-not-register-checkboxes/7108685#7108685
     * Because serializeArray() ignores unset checkboxes and radio buttons: */
    unindexed_array = unindexed_array.concat(
        $form.find('input[type=checkbox]').map(
            function () {
                return { "name": this.name, "value": $(this).prop('checked') }
            }).get()
    );

    var indexed_json = {};

    $.map(unindexed_array, function (n, i) {
        indexed_json[n['name']] = n['value'];
    });

    if (pFormatType == 'array')
        return unindexed_array;
    else
        return indexed_json;
}
//#endregion

//#region Datetime
/* For a given date, get the ISO week number
 *
 * Based on information at:
 *
 *    http://www.merlyn.demon.co.uk/weekcalc.htm#WNR
 *
 * Algorithm is to find nearest thursday, it's year
 * is the year of the week number. Then get weeks
 * between that date and the first day of that year.
 *
 * Note that dates in one year can be weeks of previous
 * or next year, overlap is up to 3 days.
 *
 * e.g. 2014/12/29 is Monday in week  1 of 2015
 *      2012/1/1   is Sunday in week 52 of 2011
 */
function getWeekNumber(d) {
    // Copy date so don't modify original
    d = new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()));
    // Set to nearest Thursday: current date + 4 - current day number
    // Make Sunday's day number 7
    d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay() || 7));
    // Get first day of year
    var yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
    // Calculate full weeks to nearest Thursday
    var weekNo = Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
    // Return array of year and week number
    return [d.getUTCFullYear(), weekNo];
}
//#endregion

//#region BlockUI
var BLOCKUI_CSS = {
    border: 'none',
    padding: '15px',
    backgroundColor: '#000',
    '-webkit-border-radius': '10px',
    '-moz-border-radius': '10px',
    opacity: .5,
    color: '#fff'
};

const AjaxDelMessage = "<h3>Deleting...</h3>",
    AjaxLoadMdMes = "<h3>Loading data...</h3>",
    AjaxSaveMdMes = "<h3>Syncing data...</h3>",
    AjaxSavingMes = "<h3>Saving data...</h3>";
//#endregion

//#region Cookie
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toGMTString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) === 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
//#endregion

//#region Image
function LoadImg(url) {
    return new Promise(function (resolve, reject) {
        const img = new Image();
        img.onload = function () {
            resolve(img);
        };
        img.onerror = function (e) {
            reject(e);
        };
        img.src = url;
    });
}
//#endregion Image