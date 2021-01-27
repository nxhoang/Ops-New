//#region Variables
var editionOps = "O";
var editionPdm = "P";
var editionAom = "A";
var editionMes = "M";
var OpGroup = "OPGroup";
var OutsourceGroup = "OPSState";
var OpType = "OPType";
var OpTool = "OPTool";
var StyleGroup = "StyleGroup";
var StyleSubGroup = "StyleSubGroup";
var StyleSubSubGroup = "StyleSubSubGroup";
var MachineRange = "MachineRange";
var MachineCodeDesc = "Machine";
var MainPart = "MainPart";
var Factory = "Factory";
var MachineFile = "Machine";
var MachineFileDesc = "Machine File";
var JigFile = "Jig";
var StyleFile = "StyleFile";
var ImageType = "Image";
var VideoType = "Video";
var Yes = "Yes";
var No = "No";
var NeutralColor = "000";
var RoleTrue = "1";
var RoleFalse = "0";
var ConfirmCheck = "Y";

//Role on web page.
var ReadOnly = "R";
var Init = "I";
var AddOnly = "AO";
var New = "N";
var Update = "U";
var EditOnly = "EO";

var AddRole = "A";
var UpdateRole = "U";
var DeleteRole = "D";
var ConfirmRole = "C";
var ExportRole = "E";

var MessageTypeError = "error";
var MessageTypeAlert = "alert";
var MessageTypeConfirm = "confirm";
var MessageTypeInfo = "info";
var MessageTypePrompt = "prompt";

//Language.
var VietNam = "vi";
var English = "en";
var Indonesia = "id";
var Myanmar = "mm";
var DefaultLanguage = "df";

//Source file upload
var SourceUpload = "U";
var SourcePlm = "P";
var SourcePkVideo = "V";

//Keep data master.
var OpsMasterInfo = "OpsMasterInfo";
var StyleMasterInfo = "StyleMasterInfo";
var StyleSearchInfo = "StyleSearchInfo";
var OpsMasterListInfo = "OpsMasterList";
var LanguageId = "LanguageId";
var OpMasterLanguage = "OpMasterLanId";

//Column grid op detail.
var ColOpName = "OpName";
var ColOpNameLan = "OpNameLan";

//Factory ID
var FactoryRoleId = "5";

//Name of gridview style
var GridStyleName = "tb_Grid";
var GridStyleId = "#tb_Grid";

var gridOpsTableName = "gridOpsTable";
var gridOpsTableId = "#gridOpsTable";
var gridOpsPaperName = "gridOpsPaper";
var gridOpsPaperId = "#gridOpsPaper";

//Gridview input optime
var TableInputOpNameId = "#tbInputOptime";
var TableInputOpNameName = "tbInputOptime";

//Modal input operation time.
var ModalInputOpTimeName = "mdlInputOpTime";

//System and menu id
var SystemIdOps = "OPS";
var SystemIdMes = "MES";
var MenuIdMes = "LRG";
var MenuIdOpm = "OPM";
var MenuIdAom = "FOM";
var MenuIdStm = "STM";
var MenuIdTbl = "TBL";
var MenuIdMdl = "MDL";
var MenuIdSms = "SMS";

//Type of machine file
var ArrMachineFileType = ["dxf", "vdt", "ptg", "dat", "sew"];
var ArrJigFileType = ["dxf", "jpg", "png"];
var ArrVideoType = ["mp4"];

// These variables to check before leave opslayout screen
var isChange = false;
var CanSave = false;

// #region Message

var SmsFunction = {
    Add: "Add",
    Confirm: "Confirm",
    Update: "Update",
    Delete: "Delete",
    Generic: 'Generic',
    BeforeChange: 'BeforeChange',
    Check: 'Check',
    Link: 'Link',
    Import: 'Import',
    Exprort: 'Export',
    Upload: 'Upload'
};

var MessageType = {
    Confirm: "Confirm",
    Error: "Error",
    Success: "Success",
    Warning: "Warning"
};

var MessageContext = {
    IgnoreChanges: "012",
    Add: "022",
    AddConfirm: "031",
    Confirmed: "024",
    Confirm: "034",
    Save: "021",
    SaveConfirm: "031",
    Generic: "Generic",
    Communication: "003",
    Error: "005",
    Update: "023",
    UpdateConfirm: "033",
    InvalidData: "004",
    Delete: "026",
    DeleteConfirm: "036",
    Database: "002",
    NoData: "013"
};

// #endregion

//Block UI CSS
var ObjectBlockUICss = {
    message: "<h3>Uploading...</h3>",
    css: { backgroundColor: '#f00', color: '#fff', zIndex: 20000 }
}

var ObjectBlockUICssWaiting = {
    message: "<h3>Please wait...</h3>",
    css: { backgroundColor: '#f00', color: '#fff', zIndex: 20000 }
}
//#endregion

// #region Jqgrid column name

arrOpsColname = {
    EDITION: 'Edition',
    STYLECODE: 'Style Code',
    STYLESIZE: 'Style Size',
    STYLECOLORSERIAL: 'Color',
    REVNO: 'Revision',
    OPREVNO: 'Op Revision',
    OPTIME: 'Op Time',
    OPPRICE: 'Op Price',
    MACHINECOUNT: 'Machine count',
    CONFIRMCHK: 'Status',
    OPCOUNT: 'Op Count',
    MANCOUNT: 'Workers',
    FACTORY: 'Factory',
    LASTUPDATEDATE: 'Date update',
    REMARKS: 'Remarks',
    BUYERSTYLECODE: 'Buyer Code',
    BUYERSTYLENAME: 'Buyer Name'
};

arrColNameOpsDetail = {
    STYLECODE: "Style Code",
    SYTLESIZE: "Style Size",
    STYLECOLORSERIAL: "Color",
    REVNO: "RevNo",
    OPREVNO: "Op No",
    OPSERIAL: "Op Serial",
    OPNUM: "OpNum",
    OPNAME: "Operation Name",
    OPTIME: "Operation Time",
    OPPRICE: "Cost",
    FACTORY: "Factory",
    MANCOUNT: "Workers",
    MACHINETYPE: "MachineType",
    MACHINECOUNT: "Machines",
    OFFEROPPRICE: "Offer Price",
    MAXTIME: "Max Time",
    THREADCOLOR: "Thread Color",
    BENCHMARKTIME: "Benchmark Time",
    OPGROUPNAME: "Group Name",
    MACHINENAME: "Machine Type",
    MACHINEFILE: "Machine File",
    VIDEO: "Video",
    UPLOADFILE: "Upload file",
    DOWNLOADFILE: "View files",
    PLAYVIDEO: "Play Video",
    OPNAMETRANSLATE: "Operation Name Translate",
    MODULENAME: "Module Name",
};

// #endregion

// #region Mapping edtion

function MappingEdition(edition) {
    switch (edition) {
        case "P":
            edition = "PDM";
            break;
        case "O":
            edition = "OPS";
            break;
        case "A":
            edition = "AOM";
    }
    return edition;
}

// #endregion

// #region Language maping functions

function MapLanguageToFlag(lang) {
    switch (lang) {
        case "V":
            lang = "vn";
            break;
        case "I":
            lang = "id";
            break;
        case "E":
            lang = "gb";
            break;
        case "M":
            lang = "mm";
            break;
        case "T":
            lang = "et";
            break;
        case "K":
            lang = "ko";
            break;
        default:
            lang = "df";
    }

    return lang;
}

function MapFlagValueToLanguage(value) {
    switch (value) {
        case "vn":
            value = "V";
            break;
        case "id":
            value = "I";
            break;
        case "gb":
            value = "E";
            break;
        case "mm":
            value = "M";
            break;
        case "et":
            value = "T";
            break;
        case "ko":
            value = "K";
            break;
        default:
            value = "";
    }

    return value;
}

function MapOperationPlanEditioin(edition) {
    switch (edition) {
        case "P":
            edition = "PDM";
            break;
        case "O":
            edition = "OPS";
            break;
        case "A":
            edition = "AOM";
            break;
        case "M":
            edition = "MES";
            break;
        default:
            edition = "";
    }

    return edition;
}

function MapValueToNameLanguage(value) {
    switch (value) {
        case "vn":
        case "V":
            value = "VietNam";
            break;
        case "id":
        case "I":
            value = "Indonesia";
            break;
        case "gb":
        case "E":
            value = "English";
            break;
        case "mm":
        case "M":
            value = "Myanmar";
            break;
        default:
            value = "";
    }

    return value;
}

// #endregion

// #region Modal and Menu

function HideModal(modalId) {
    $('#' + modalId).modal('hide');
}

function ShowModal(modalName) {
    $("#" + modalName).modal("show");
    $('.modal-dialog').draggable({
        handle: ".modal-header ",
        cursor: "move"
    });
}

function VisibleMenuButton(divId, blAdd, blSave, blEdit, blConfirm, blDelete, blCancel) {
    if (blAdd) $('#' + divId + ' .btnAdd').show(); else $('#' + divId + ' .btnAdd').hide();
    if (blSave) $('#' + divId + ' .btnSave').show(); else $('#' + divId + ' .btnSave').hide();
    if (blEdit) $('#' + divId + ' .btnEdit').show(); else $('#' + divId + ' .btnEdit').hide();
    if (blConfirm) $('#' + divId + ' .btnConfirm').show(); else $('#' + divId + ' .btnConfirm').hide();
    if (blDelete) $('#' + divId + ' .btnDelete').show(); else $('#' + divId + ' .btnDelete').hide();
    if (blCancel) $('#' + divId + ' .btnCancel').show(); else $('#' + divId + ' .btnCancel').hide();
}

function DisableMenuButton(divId, blAdd, blSave, blEdit, blConfirm, blDelete, blCancel) {
    $('#' + divId + ' .btnAdd').prop('disabled', blAdd);
    $('#' + divId + ' .btnSave').prop('disabled', blSave);
    $('#' + divId + ' .btnEdit').prop('disabled', blEdit);
    $('#' + divId + ' .btnConfirm').prop('disabled', blConfirm);
    $('#' + divId + ' .btnDelete').prop('disabled', blDelete);
    $('#' + divId + ' .btnCancel').prop('disabled', blCancel);
}

// #endregion

// #region Common checking functions

function CheckStyleInfoIsValid(styleCode, styleSize, styleColor, revNo) {
    if (isEmpty(styleCode) || isEmpty(styleSize) || isEmpty(styleColor) || isEmpty(revNo)) {
        ShowMessage("Style key code", "Please check style code, style size, style color and revison!", MessageTypeAlert);
        return false;
    }

    return true;
}

function CheckOperationPlanMasterKeyIsValid(objOpKey) {

    if ($.isEmptyObject(objOpKey)) {
        ShowMessageOk("003", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

        return false;
    }

    if (!CheckStyleInfoIsValid(objOpKey.StyleCode, objOpKey.StyleSize, objOpKey.StyleColorSerial, objOpKey.RevNo)) {
        return false;
    }

    if (isEmpty(objOpKey.OpRevNo)) {
        ShowMessage("Op master key", "Operation no is empty!", MessageTypeAlert);
        return false;
    }

    return true;
}

function CheckOpsInfo(objOps) {
    if (!CheckOperationPlanMasterKeyIsValid(objOps)) return false;

    if (isEmpty(objOps.OpTime)) {
        ShowMessage("Ops info", "Operation time is empty!", MessageTypeAlert);
        return false;
    }

    if (isEmpty(objOps.MachineCount)) {
        ShowMessage("Ops info", "Machine count is empty!", MessageTypeAlert);
        return false;
    }

    if (isEmpty(objOps.OpCount)) {
        ShowMessage("Ops info", "Operation count is empty!", MessageTypeAlert);
        return false;
    }

    if (isEmpty(objOps.ManCount)) {
        ShowMessage("Ops info", "Worker number is empty!", MessageTypeAlert);
        return false;
    }

    return true;
}

//Check style is empty or not
function IsStyleEmpty(styleCode, styleSize, styleColor, revNo) {
    if (isEmpty(styleCode) || isEmpty(styleSize) || isEmpty(styleColor) || isEmpty(revNo)) return false;
    return true;
}

function IsSameStyle(oldStyle, newStyle) {
    if (oldStyle.StyleCode === newStyle.StyleCode) return true;
    if (oldStyle.StyleSize === newStyle.StyleSize) return true;
    if (oldStyle.StyleColorSeiral === newStyle.StyleColorSeiral) return true;
    if (oldStyle.RevNo === newStyle.RevNo) return true;
    return false;
}
// #endregion

// #region Init data

function LoadOpsMasterGrid() {
    //Get style master from local storage
    var objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));
    if ($.isEmptyObject(objStyleMaster)) {
        BindDataToJqGridOps("", "", "", "", "");
        jQuery(gridOpsTableId).jqGrid('setCaption', "");
    } else {
        BindDataToJqGridOps(objStyleMaster.StyleCode, objStyleMaster.StyleSize, objStyleMaster.StyleColorSerial, objStyleMaster.RevNo, "");

        jQuery(gridOpsTableId).jqGrid('setCaption', " Style Code: " + objStyleMaster.StyleCode + " | Size: " + objStyleMaster.StyleSize
            + " | Color: " + objStyleMaster.StyleColorSerial + " | Revision: " + objStyleMaster.RevNo);

    }
}

//Create list languages
function CreateListLanguages() {
    //Create list language
    var arrLang = [
        { LanguageId: "vn", LanguageName: "VietNamese" },
        { LanguageId: "gb", LanguageName: "English" },
        { LanguageId: "in", LanguageName: "Indonesia" },
        { LanguageId: "mm", LanguageName: "Myanmar" }
    ];

    return arrLang;
}

//Load languages to dropdownlist.
function LoadLanguageToDropdownlist(drpName) {
    var arrLang = CreateListLanguages();
    FillDataToDropDownlist(drpName, arrLang, "LanguageId", "LanguageName");
    $("#" + drpName).val("EN").trigger("change");
}

function ClearCookie() {
    //Clear cookie
    window.localStorage.removeItem(StyleSearchInfo);
    window.localStorage.removeItem(StyleMasterInfo);
    window.localStorage.removeItem(OpsMasterInfo);
}

// #endregion

//#region JqGrid
arrButtonName = {
    edittext: 'Edit',
    addtext: 'Add',
    deltext: 'Delete',
    searchtext: 'Search',
    refreshtext: 'Refresh'
};
arrPopup = {
    captionEdit: 'Edit',
    submitEdit: 'Summit',
    cancel: 'Cancel'
};
arrTitle = {
    textAnd: 'And',
    textOr: 'Or',
    coppyRow: 'Coppy'
};
arrSession = {
    deleted: 'true',
    user: 'Admin'
};
arrButtonAction = {
    all: 'All',
    save: 'Save',
    deleted: 'delete'
};
function SetPaging(gridTableId, navPageName) {
    var rowtext = $('#pg_' + navPageName).find('.ui-pg-selbox  option:selected').text();
    var rownum = $('#pg_' + navPageName).find('.ui-pg-selbox').val();
    if (rowtext === arrButtonAction.all) {
        gridTableId.jqGrid('setGridParam', { scroll: 1, page: 1, rowNum: 20, scrollrows: false });
        $("#" + navPageName + "_center table tbody tr td").css('display', 'none');
        $("#" + navPageName + "_center table tbody tr td").last().show();
    }
    else {
        gridTableId.jqGrid('setGridParam', { scroll: false, page: 1, rowNum: rownum, scrollrows: true });
        $(".ui-jqgrid-bdiv").children().css('height', 'auto');
        $("#" + navPageName + "_center table tbody tr td").show();
    }
    gridTableId.trigger('reloadGrid');
}
function SearchFilter(grid) {
    grid.jqGrid("filterToolbar", {
        stringResult: true, searchOnEnter: false,
        defaultSearch: "cn", ignoreCase: true, enableCstringResult: true, autoencode: false
    });
}
//#endregion JqGrid

// #region Jqgrid functions

//Reload Jq Grid 
function ReloadJqGrid(tableJqGridName, data) {
    $("#" + tableJqGridName).jqGrid('setGridParam', {
        dataType: "json",
        postData: data
    }).trigger('reloadGrid');
}

function ChangeGroupingJqGrid(gridId, groupBy) {
    //Author: Son Nguyen Cao  
    switch (groupBy) {
        case "GN":
            //Show process according by group name
            $(gridId).jqGrid('groupingGroupBy', 'OpGroupName', {
                groupOrder: ['asc'],
                groupText: ["Group Name: {0} - {1} Item(s)"]
            });
            break;
        case "GM":
            //Show process according by group module
            $(gridId).jqGrid('groupingGroupBy', 'ModuleName', {
                groupOrder: ['asc'],
                groupText: ["Module Name: {0} - {1} Item(s)"]
            });
            break;
        case "MC":
            //Show process according by group MachineName
            $(gridId).jqGrid('groupingGroupBy', 'MachineName', {
                groupOrder: ['asc'],
                groupText: ["Machine Name: {0} - {1} Item(s)"]
            });
            break;
        default:
            $(gridId).jqGrid('groupingRemove');
            break;
    }
    //MachineName
}

//Show column op name for gridview load op detail.
function ShowOpNameColumGridOpDetail(gvId, selLanId, opsLanId) {
    //Check reference operation name is empty or not. If the name is empty then show original process name.
    var gridData = jQuery(gvId).jqGrid("getRowData");
    var isNullOpName = false;
    for (var key in gridData) {
        if (gridData.hasOwnProperty(key)) {
            if (isEmpty(gridData[key].OpNameLan)) {
                isNullOpName = true;
                break;
            }
        }
    }

    if (selLanId === DefaultLanguage || selLanId === opsLanId || isNullOpName === true) {
        jQuery(gvId).hideCol(ColOpNameLan);
        jQuery(gvId).showCol(ColOpName);
    } else {
        jQuery(gvId).hideCol(ColOpName);
        jQuery(gvId).showCol(ColOpNameLan);
    }
}

// #endregion

// #region Common functions get data

//Get user role information
function GetUserRoleInfo(sysId, menuId) {
    var role;
    $.ajax({
        url: "/Ops/GetUserRoleInfo",
        async: false, //run sequence
        type: "POST",
        data: JSON.stringify({ sysId: sysId, menuId: menuId }),
        dataType: "json",
        contentType: "application/json",
        success: function (resRole) {
            role = resRole;
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessage("Get user role", "Cannot get user role info!\n" + errorThrown.message, MessageTypeError);
            role = {};
        }
    });
    return role;
}

//Get style master information
function GetStyleMasterByStyleCode(styleCode, fncCallBack) {
    //var styleMaster;
    $.ajax({
        url: "/Ops/GetStyleMasterByStyleCode",
        //async: false, //run sequence
        type: "POST",
        data: JSON.stringify({ styleCode: styleCode }),
        dataType: "json",
        contentType: "application/json",
        success: function (resStyleMaster) {
            fncCallBack(resStyleMaster);
            //styleMaster = resStyleMaster;
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessage("Get style master", "Cannot get style master info!\n" + errorThrown.message, MessageTypeError);
            styleMaster = {};
        }
    });
    //return styleMaster;
}

//Get Master Code
function GetMasterCode(mCode) {
    var arrMasterCode;
    $.ajax({
        url: "/Ops/GetMasterCode",
        async: false,
        type: "POST",
        data: JSON.stringify({ mCode: mCode }),
        dataType: "json",
        contentType: "application/json",
        success: function (arrRes) {
            arrMasterCode = arrRes;
        },
        error: function () {
            arrMasterCode = [];
        }
    });

    return arrMasterCode;
}

//Get operation name by language
function GetOpName(languageId) {
    var arrOpName;
    $.ajax({
        url: "/Ops/GetOpName",
        async: false,
        type: "POST",
        data: JSON.stringify({ languageId: languageId }),
        dataType: "json",
        contentType: "application/json",
        success: function (arrRes) {
            arrOpName = arrRes;
        },
        error: function () {
            arrOpName = [];
        }
    });

    return arrOpName;
}

function GetMasterCodeDes(mCode, mDes) {
    var arrMasterCode;
    $.ajax({
        url: "/Ops/GetMasterCodeDes",
        async: false, //run sequence
        type: "POST",
        data: JSON.stringify({ mCode: mCode, mDes: mDes }),
        dataType: "json",
        contentType: "application/json",
        success: function (arrRes) {
            arrMasterCode = arrRes;
        },
        error: function () {
            arrMasterCode = [];
        }
    });

    return arrMasterCode;
}

//Get color master
function GetMasterColor() {
    var arrColor;
    $.ajax({
        url: "/Ops/GetMasterColor",
        async: false,
        type: "POST",
        data: {},
        dataType: "json",
        contentType: "application/json",
        success: function (arrRes) {
            arrColor = arrRes;
        },
        error: function () {
            arrColor = [];
        }
    });

    return arrColor;
}

//Get style files
function GetStyleFiles() {
    var arrStyleFile;
    $.ajax({
        url: "/Ops/GetStyleFiles",
        async: false,
        type: "POST",
        data: JSON.stringify({}),
        dataType: "json",
        contentType: "application/json",
        success: function (arrRes) {
            arrStyleFile = arrRes;
        },
        statusCode: {
            500: (jqxhr, textStatus, error) => {
                console.log(error);
            },
            404: (jqxhr, textStatus, error) => {
                console.log(error);
            }
        }
    });

    return arrStyleFile;
}

//Create object config for Ajax.
function ObjectConfigAjaxPost(url, asynchronous, postData) {
    var config = {
        url: url,
        async: asynchronous,
        postData: postData
    };
    return config;
}

//Get an array tool Master
function GetArrayMachineMaster(isTool, lstCategoryId) {
    var arrToolMaster;
    var config = ObjectConfigAjaxPost("/Ops/GetOpMachineMaster", false, JSON.stringify({ isTool: isTool, lstCategoryId: lstCategoryId }));
    AjaxPostCommon(config, function (respone) {
        arrToolMaster = respone;
    });

    return arrToolMaster;
}

function GetMasterCode2(masterCode, subCode, codeDesc) {
    var arrMasterCode;
    $.ajax({
        url: "/Ops/GetMasterCode2",
        async: false,
        type: "POST",
        data: JSON.stringify({ masterCode: masterCode, subCode: subCode, codeDesc: codeDesc }),
        dataType: "json",
        contentType: "application/json",
        success: function (arrRes) {
            arrMasterCode = arrRes;
        },
        error: function () {
            arrMasterCode = [];
        }
    });

    return arrMasterCode;
}

//Get an array tool Master
function GetArrayColorMaster() {
    var arrColorMaster;
    $.ajax({
        url: "/Ops/GetOpColorMaster",
        async: false,
        type: "POST",
        data: {},
        dataType: "json",
        contentType: "application/json",
        success: function (arrColor) {
            arrColorMaster = arrColor;
        },
        error: function () {
            arrColorMaster = [];
        }
    });

    return arrColorMaster;
}

//Get an array Buyer
function GetArrayBuyer() {
    var arrBuyer;
    $.ajax({
        url: "/Ops/GetBuyer",
        async: false,
        type: "POST",
        data: {},
        dataType: "json",
        contentType: "application/json",
        success: function (arrResColor) {
            arrBuyer = arrResColor;
        },
        error: function () {
            arrBuyer = [];
        }
    });

    return arrBuyer;
}

//Get max operation plan revision
function GetMaxOpRevision(data) {
    var maxOpRev;
    $.ajax({
        url: "/Ops/GetMaxOpRevision",
        async: false,
        type: "POST",
        data: JSON.stringify(data),
        dataType: "json",
        contentType: "application/json",
        success: function (resMaxOpRev) {
            maxOpRev = resMaxOpRev;
        },
        error: function () {
            maxOpRev = "";
            //alert(error.responseText);
        }
    });

    return maxOpRev;
}

function GetMaxOpSerialByCode(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition) {
    var maxOpSerial = null;
    var opDetail = {
        edition: edition, styleCode: styleCode,
        styleSize: styleSize, styleColorSerial: styleColorSerial,
        revNo: revNo, opRevNo: opRevNo
    };

    $.ajax({
        url: "/Ops/GetMaxOpSerial",
        type: "POST",
        async: false,
        data: JSON.stringify(opDetail),
        dataType: "json",
        contentType: "application/json",
        success: function (maxSerial) {
            maxOpSerial = maxSerial;
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessage("Get Max Operation Serial", "Cannot get max Operation Plan serial!\n" + errorThrown.message, MessageTypeError);
        }
    });

    return maxOpSerial;
}

//Get max Operation Plan Serial
function GetMaxOpSerial() {
    let styleCode = "", styleSize = "", styleColorSerial = "", revNo = "", opRevNo = "";
    if (!$.isEmptyObject(CurrentOpmt)) {
        styleCode = CurrentOpmt.StyleCode;
        styleSize = CurrentOpmt.StyleSize;
        styleColorSerial = CurrentOpmt.StyleColorSerial;
        revNo = CurrentOpmt.RevNo;
        opRevNo = CurrentOpmt.OpRevNo;
    }
    const opDetail = {
        edition: "M", styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo,
        opRevNo: opRevNo
    };

    $.ajax({
        url: "/Ops/GetMaxOpSerial",
        async: false,
        type: "POST",
        data: JSON.stringify(opDetail),
        dataType: "json",
        contentType: "application/json",
        success: function (maxSerial) {
            $("#txtProcessNo").val(maxSerial);
            $("#txtProcessNumber").val(Number(maxSerial));
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessage("Get Max Operation Serial", "Cannot get max Operation Plan serial!\n" + errorThrown.message,
                MessageTypeError);
            $("txtProcessNo").val("");
        }
    });
}

//Get object ops detail by code
function GetObjectOpsDetail(objOpDetail, callBack) {
    $.ajax({
        url: "/Ops/GetOpDetailByCode",
        async: true,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            callBack(res);
        },
        error: function (jqXhr, status, err) {
            //ShowMessage("Get ops detail", "Cannot get ops detail!\n" + err.messages, MessageTypeError);
            console.log(jqXhr);
        }
    });
}

//Get key code of style overwrite function GetKeyCodeStyle in SearchStyle.js
function GetKeyCodeStyle(styleCode, styleSize, styleColor, revNo) {

    CreateGridOpsDetail(styleCode, styleSize, styleColor, revNo);

    //Reload girdview Ops   
    var data = CreateObjStyleKeyCode(styleCode, styleSize, styleColor, revNo);
    //ReloadJqGrid(gridOpsTableName, data);
    ReloadJqGrid2LoCal(gridOpsTableName, data);
    jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + styleCode + " | Size: " + styleSize
        + " | Color: " + styleColor + " | Revision: " + revNo);

}

//Check factory role
function IsFactoryRole(objUserRole) {
    //Check empty role
    if ($.isEmptyObject(objUserRole)) {
        return false;
    }
    var factoryId = isEmpty(objUserRole.OwnerId) ? "" : objUserRole.OwnerId.slice(0, 1);
    return factoryId === FactoryRoleId;
}

//Set value from drowdownlist language - operation detail
function SetValueForLanguage(idDropdowlist, lanId) {
    const oDropdown = $("#" + idDropdowlist).msDropdown().data("dd");
    if (oDropdown) {
        let index = 0;
        for (let i = 0; i < oDropdown.length; i++) {
            if (oDropdown.options[i].value === lanId) {
                index = i;
                break;
            }
        }
        oDropdown.set("selectedIndex", index);
    }
}

// #endregion

// #region Common Functions
//Calculate maxtime
function CalculateMaxTime(opTime, manCount, machineCount) {
    var maxTime = 0;
    if (ConvertStringToNumber(manCount) < 1 || ConvertStringToNumber(machineCount) === 0 || (ConvertStringToNumber(manCount) % 1) !== 0) return maxTime;
    else {
        maxTime = Math.round(opTime / manCount);
    }

    return maxTime;
}

function countProcessesWithStandardName(edition, languageId, styleCode, styleSize, styleColorSerial, revNo, opRevNo) {
    var config = ObjectConfigAjaxPost("/Ops/GetNumberOfProcesses", false, JSON.stringify({
        edition: edition,
        languageId: languageId, styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo,
        opRevNo: opRevNo
    }));
    var countPro = 0;
    AjaxPostCommon(config, function (res) {
        countPro = res;
    });

    return countPro;
}
// #endregion

// #region Create Object

//Create Ops object Edition
function CreateObjectOpsEdition() {
    var arrTarget = [];
    arrTarget.push(
        { EditionCode: "P", EditionName: "OPS" },
        { EditionCode: "A", EditionName: "AOM" },
        { EditionCode: "M", EditionName: "MES" }
    );

    return arrTarget;
}

//Create Object Style
function CreateObjStyleKeyCode(styleCode, styleSize, styleColor, revNo) {
    var data = { styleCode: styleCode, styleSize: styleSize, styleColor: styleColor, revNo: revNo };
    return data;
}

//Create object to fill csv file data
function CreateObjectProcess(opGroup, opId, opNum, opName, opTime) {
    var objProcess = {
        OpGroup: opGroup, OpId: opId, OpNum: opNum, OpName: opName, OpTime: opTime
    };

    return objProcess;
}

//Create object ops
function CreateObjectOps(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opTime, machineCount, opCount, manCount) {
    var objOps = {
        StyleCode: styleCode,
        StyleSize: styleSize,
        StyleColorSerial: styleColorSerial,
        RevNo: revNo,
        OpRevNo: opRevNo,
        OpTime: opTime,
        MachineCount: machineCount,
        OpCount: opCount,
        ManCount: manCount
    };

    return objOps;
}

//Create object ops
function CreateObjectOpsDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, opNum, opGroup, opName, opTime, edition, opNameRef, machineName) {
    var objOps = {
        StyleCode: styleCode,
        StyleSize: styleSize,
        StyleColorSerial: styleColorSerial,
        RevNo: revNo,
        OpRevNo: opRevNo,
        OpSerial: opSerial,
        OpNum: opNum,
        OpGroup: opGroup,
        OpName: opName,
        OpTime: opTime,
        Edition: edition,
        OpNameRef: opNameRef,
        MachineType: machineName,
        MachineName: machineName
    };

    return objOps;
}

// #endregion

// #region User role

function GetMenuIdByEdition(edition) {
    var menuId = "";
    switch (edition) {
        case editionOps:
        case editionPdm:
            menuId = MenuIdOpm;
            break;
        case editionAom:
            menuId = MenuIdAom;
            break;
        case editionMes:
            menuId = MenuIdMes;
            break;
        default:
            break;
    }

    return menuId;
}

// #endregion

// #region Operation master

function SelectEditionOpsMaster() {
    $("#drpOpsMasterEdition").change(function () {
        //Get search style info
        var stl = JSON.parse(localStorage.getItem(StyleMasterInfo));
        //Reload girdview Ops
        var data = CreateObjStyleKeyCode(stl.StyleCode, stl.StyleSize, stl.StyleColorSerial, stl.RevNo);
        data.edition = $(this).val();
        //ReloadJqGrid(gridOpsTableName, data);
        ReloadJqGrid2LoCal(gridOpsTableName, data);

    });

}

function AddEditionDropdownToOpsHeader() {
    jQuery(gridOpsTableId).jqGrid('setLabel', 'Edition2',
        "<select id= 'drpOpsMasterEdition' >" +
        "<option value=''>All</option >" +
        "<option value='P'>PDM</option >" +
        "<option value='O'>OPS</option >" +
        "<option value='A'>AOMTOPS</option>" +
        "<option value='M'>MES</option>" +
        "</select> ");

    //console.log("AddEditionDropdownToOpsHeader");
}

//Bind data to Ops gridview
function BindDataToJqGridOps(styleCode, styleSize, styleColor, revNo, edition) {
    jQuery(gridOpsTableId).jqGrid({
        url: '/OPS/GetOpMaster',
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColor: styleColor, revNo: revNo, edition: edition
        },
        datatype: "json",
        height: 'auto',
        width: null,
        shrinkToFit: false,
        scroll: false,
        deepempty: true,
        ignoreCase: true,
        viewrecords: true,
        rowNum: 10,
        rowList: [10, 20, 30, 40],
        pager: gridOpsPaperName,
        gridview: true,
        caption: "OPS",
        colModel: [
            { name: 'Edition2', index: 'Edition2', width: 110, label: arrOpsColname.EDITION, align: 'center', classes: 'pointer', sortable: false },
            { name: 'StyleCode', index: 'StyleCode', width: 90, label: arrOpsColname.STYLECODE, classes: 'pointer' },
            { name: 'StyleColorWays', index: 'StyleColorWays', width: 200, label: arrOpsColname.STYLECOLORSERIAL, classes: 'pointer' },
            { name: 'BuyerStyleCode', index: 'BuyerStyleCode', width: 120, label: arrOpsColname.BUYERSTYLECODE, classes: 'pointer' },
            { name: 'BuyerStyleName', index: 'BuyerStyleName', width: 250, label: arrOpsColname.BUYERSTYLENAME, classes: 'pointer' },
            { name: 'StyleSize', index: 'StyleSize', width: 90, label: arrOpsColname.STYLESIZE, classes: 'pointer' },
            { name: 'RevNo', index: 'RevNo', width: 90, label: arrOpsColname.REVNO, align: 'center', classes: 'pointer' },
            { name: 'OpRevNo', index: 'OpRevNo', width: 90, label: arrOpsColname.OPREVNO, align: 'center', classes: 'pointer' },
            { name: 'OpTime', index: 'OpTime', width: 90, label: arrOpsColname.OPTIME, align: 'center', classes: 'pointer' },
            { name: 'OpPrice', index: 'OpPrice', width: 90, label: arrOpsColname.OPPRICE, align: 'center', classes: 'pointer', hidden: true },
            { name: 'MachineCount', index: 'MachineCount', width: 115, label: arrOpsColname.MACHINECOUNT, align: 'center', classes: 'pointer' },
            { width: 60, label: arrOpsColname.CONFIRMCHK, align: 'center', classes: 'pointer', formatter: showIconConfirmed },
            { name: 'OpCount', index: 'OpCount', width: 90, label: arrOpsColname.OPCOUNT, align: 'center', classes: 'pointer' },
            { name: 'ManCount', index: 'ManCount', width: 90, label: arrOpsColname.MANCOUNT, align: 'center', classes: 'pointer' },
            { name: 'Factory', index: 'Factory', width: 90, label: arrOpsColname.FACTORY, align: 'center', classes: 'pointer' },
            { name: 'LastUpdateTime', index: 'LastUpdateTime', width: 150, label: arrOpsColname.LASTUPDATEDATE, align: 'left', classes: 'pointer', formatter: convertDateToString },
            { name: 'Remarks', index: 'Remarks', width: 250, label: arrOpsColname.REMARKS, align: 'left', classes: 'pointer' },
            { name: 'Edition', index: 'Edition', width: 90, label: arrOpsColname.EDITION, hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'Language', index: 'Language', hidden: true },
            { name: 'ProcessWidth', index: 'ProcessWidth', hidden: true },
            { name: 'ProcessHeight', index: 'ProcessHeight', hidden: true },
            { name: 'GroupMode', index: 'GroupMode', hidden: true },
            { name: 'CanvasHeight', index: 'CanvasHeight', hidden: true },
            { name: 'Buyer', index: 'Buyer', hidden: true },
            { name: 'LayoutFontSize', index: 'LayoutFontSize', hidden: true },
            { name: 'StyleGroup', index: 'StyleGroup', hidden: true },
            { name: 'SubGroup', index: 'SubGroup', hidden: true },
            { name: 'SubSubGroup', index: 'SubSubGroup', hidden: true },
            { name: 'ConfirmChk', index: 'ConfirmChk', hidden: true },
            { name: 'RegisterId', index: 'RegisterId', hidden: true },
            { name: 'ConfirmedId', index: 'ConfirmedId', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            //window.location.reload();
            ShowMessage("Get Ops Master", err.message, MessageTypeError);
            //ShowMessage("", xhr.responseText, Fail);
        },
        //loadComplete: function () {
        //    setTimeout(function () {
        //        updatePagerIcons();
        //    }, 0);
        //},
        onPaging: function (pgButton) {
            if (isChange && CanSave) {
                var smsg = GetMsgAsin("001", SystemIdOps, MenuIdAom, SmsFunction.Confirm, MessageType.Confirm, MessageContext.Confirm, language);
                if (!confirm(smsg.value)) {
                    return 'stop';
                } else {
                    isChange = false;
                }
            }
            if (pgButton === "records") {
                SetPaging($(gridOpsTableId), gridOpsPaperName);
            }
        },
        onSelectRow: function (rowid) {
            const opmt = $(gridOpsTableId).jqGrid("getRowData", rowid);
            //row.RowId = rowid;
            //OpsMasterFunction(row);

            jQuery(gridOpsTableId).jqGrid('setCaption', `OPS - Style: ${opmt.StyleCode} | Size: ${opmt.StyleSize} | Color: ${opmt.StyleColorSerial} | Revision: ${opmt.RevNo} | Op Revision: ${opmt.OpRevNo}`);

            ////Save ops master key to local storage
            //localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

            CheckMachineOpsTable(); // HA ADD
            console.log(`selected opmt: `);
            console.log(opmt);
        },
        ajaxGridOptions: { async: false },
        loadonce: true,
        gridComplete: function () {
            setTimeout(function () {
                window.updatePagerIcons();
            }, 0);

            var opsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));
            var rows = jQuery(gridOpsTableId).getDataIDs();

            if (!$.isEmptyObject(opsMaster)) {
                var iselected = false;
                for (var i = 0; i < rows.length; i++) {
                    var row = jQuery(gridOpsTableId).getRowData(rows[i]);
                    if (row.StyleCode === opsMaster.StyleCode &&
                        row.StyleSize === opsMaster.StyleSize &&
                        row.StyleColorSerial === opsMaster.StyleColorSerial &&
                        row.RevNo === opsMaster.RevNo &&
                        row.OpRevNo === opsMaster.OpRevNo &&
                        row.Edition === opsMaster.Edition) {
                        $(gridOpsTableId).jqGrid("setSelection", rows[i], true);

                        OpsMasterFunction(row);
                        jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
                            + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Op Revision: " + row.OpRevNo);

                        //Save ops master key to local storage
                        localStorage.setItem(OpsMasterInfo, JSON.stringify(row));
                        iselected = true;
                        break;
                    }
                }
                if (!iselected) {
                    if (!$.isEmptyObject(rows)) {
                        $(gridOpsTableId).jqGrid("setSelection", rows[0], true);
                        var row = jQuery(gridOpsTableId).getRowData(rows[0]);
                        OpsMasterFunction(row);
                        localStorage.setItem(OpsMasterInfo, JSON.stringify(row));
                    }
                }
            } else {
                if (!$.isEmptyObject(rows)) {
                    $(gridOpsTableId).jqGrid("setSelection", rows[0], true);
                    var row = jQuery(gridOpsTableId).getRowData(rows[0]);
                    OpsMasterFunction(row);
                    localStorage.setItem(OpsMasterInfo, JSON.stringify(row));
                }
            }
        },
        loadComplete: function () { },
        beforeSelectRow: function (rowid, e) {
            if (isChange && CanSave) {
                if (BeforeSelectRowGridOpMaster()) {
                    isChange = false;
                    var row = $(gridOpsTableId).jqGrid("getRowData", rowid);
                    OpsMasterFunction(row);

                    jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
                        + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Op Revision: " + row.OpRevNo);

                    //Save ops master key to local storage
                    localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

                    return true;
                }
            } else {
                var row = $(gridOpsTableId).jqGrid("getRowData", rowid);
                OpsMasterFunction(row);

                jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
                    + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Op Revision: " + row.OpRevNo);

                //Save ops master key to local storage
                localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

                return true;
            }
        },
    });

    AddEditionDropdownToOpsHeader();
    //SelectEditionOpsMaster();

    //navButtons
    jQuery(gridOpsTableId).jqGrid('navGrid', gridOpsPaperName, {
        //navbar options
        view: true,
        viewicon: 'ace-icon fa fa-search-plus grey',
        edit: false,
        del: false,
        search: true,
        searchicon: 'ace-icon fa fa-search orange',
        refresh: true,
        refreshicon: 'ace-icon fa fa-refresh green'
    });

    $("#pg_" + gridOpsPaperName + " option[value=40]").text(arrButtonAction.all);

    // Bind the navigation and set the onEnter event
    jQuery(gridOpsTableId).jqGrid('bindKeys');

    function showIconConfirmed(cellValue, options, rowObject) {
        if (rowObject.ConfirmChk === ConfirmCheck) {
            return "<label><i class='fa fa-lock'></i></label>";
        }
        return "";
    }

    function convertDateToString(cellValue, options, rowObject) {
        if (!$.isEmptyObject(rowObject.LastUpdateTime)) {
            var newDate = eval(("new " + rowObject.LastUpdateTime).replace(/\//g, ""))
            return newDate;
        }
        return "";
    }

    //Set height of grid operation plan.
    $(gridOpsTableId).setGridHeight(200);

    $(gridOpsTableId).jqGrid("filterToolbar", {
        stringResult: true, searchOnEnter: false,
        defaultSearch: "cn", ignoreCase: true, enableCstringResult: true, autoencode: false
    });
}

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
// #endregion

//#region Machine for ops registry
//START ADD) HA - 14 Jan 2019
function CheckMachineOpsTable() {
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    if ($.isEmptyObject(objOpsMaster)) return null;

    var styleCode = objOpsMaster.StyleCode;
    var styleSize = objOpsMaster.StyleSize;
    var styleColorSerial = objOpsMaster.StyleColorSerial;
    var revNo = objOpsMaster.RevNo;
    var opRevNo = objOpsMaster.OpRevNo;
    var edition = objOpsMaster.Edition;

    var data = { styleCode: styleCode, styleSize: styleSize, styleColor: styleColorSerial, revNo: revNo, opRevNo: opRevNo, edition: edition };
    //JqGridMachine(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition)
    ReloadJqGrid("tbMachine", data);
}
//END ADD) HA - 14 Jan 2019
//#endregion

// #region Message

function ShowMessageOk(contextSerial, func, mesType, mesContext, typeShowMes, mesReplace) {
    const language = "en";
    GetMsg(contextSerial, SystemIdOps, MenuIdOpm, func, mesType, mesContext, language, function (result) {
        var strMes = ReplaceStr(result.value, mesReplace);
        ShowMessage(result.title, strMes, typeShowMes);
    });
}

function ShowConfirmYesNoMessage(ContextSerial, event, mesType, mesContext, yesFunc, noFunc, mesReplace) {
    const language = "en";
    GetMsg(ContextSerial, SystemIdOps, MenuIdOpm, event, mesType, mesContext, language, function (result) {
        var strMes = ReplaceStr(result.value, mesReplace);
        ConfirmYesNo(result.title, strMes, yesFunc, noFunc);
    });
}

// #endregion

//#region Cookie
function GetUserIdFromCookie() {
    const decodedCookie = decodeURIComponent(document.cookie);
    const ca = decodedCookie.split('&');
    const userId = ca[0].split('=')[2];
    return userId;
}
//#endregion Cookie

// #region Play Video
function PauseProcessVideo() {
    $('#vdoOpsDetail').get(0).pause();
}

function PlayVideo(rowObj) {
    $("#vdoOpsDetail").attr("src", rowObj.VideoOpLink);

    //Get process information (primary key)
    var proInf = "Style Code: " + rowObj.StyleCode + " | Size: " + rowObj.StyleSize + " | Color: " + rowObj.StyleColorSerial
        + " | RevNo: " + rowObj.RevNo + " | Op Revision: " + rowObj.OpRevNo + " | Op Serial: " + rowObj.OpSerial;
    var proName = "Group name: " + rowObj.OpGroupName + " | Process name: " + rowObj.OpName;

    $("#lblProcessName").html(proName);
    $("#lblProcessInfo").html(proInf);
    ShowModal("mdlPlayVideo");
}
// #endregion