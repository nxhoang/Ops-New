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

let _initialPage = true; //ADD - SON - 2021.01.15) 18/Jan/2021

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

var IsSelectRow = false;
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
    BUYERSTYLENAME: 'Buyer Name',
    MXPACKAGE: 'MxPackage',
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

//#region Jquery Document Ready
//Init event select edition on operation master jqgrid.
$(document).ready(function () {
    SelectEditionOpsMaster();
});
//#endregion

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
            edition = "AOMTOPS";
            break;
        default:
            edition = 'MES';
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
    // Oanh add function change color 19Jan2021
    setBackgroundColorJqGridModal();
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

        //jQuery(gridOpsTableId).jqGrid('setCaption', " Style Code: " + objStyleMaster.StyleCode + " | Size: " + objStyleMaster.StyleSize
        //    + " | Color: " + objStyleMaster.StyleColorSerial + " | Revision: " + objStyleMaster.RevNo);

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

// #region Jqgrid functions

//Reload Jq Grid 
function ReloadJqGrid(tableJqGridName, data) {
    $("#" + tableJqGridName).jqGrid('setGridParam', {
        dataType: "json",
        postData: data
    }).trigger('reloadGrid');
}

//Reload using for loadonce = true;
function ReloadJqGrid2LoCal(tableJqGridName, data) {
    $("#" + tableJqGridName).setGridParam({ datatype: 'json' });
    $("#" + tableJqGridName).jqGrid('setGridParam', {
        //dataType: "json",
        postData: data
    }).trigger('reloadGrid');
}

function ReloadJqGridLocal(tableJqGridName, data) {
    $("#" + tableJqGridName).jqGrid('clearGridData');
    $("#" + tableJqGridName).jqGrid('setGridParam', {
        dataType: "local",
        data: data
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

function GetUserRoleInfoAsync(sysId, menuId, callBack) {
    $.ajax({
        url: "/Ops/GetUserRoleInfo",
        async: true,
        type: "POST",
        data: JSON.stringify({ sysId: sysId, menuId: menuId }),
        dataType: "json",
        contentType: "application/json",
        success: function (resRole) {
            callBack(resRole);
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessage("Get user role", "Cannot get user role info!\n" + errorThrown.message, MessageTypeError);
            console.log(errorThrown);
        }
    });
}

//START ADD - SON) 18/Jan/2021
const getOpsUserRole = () => {
    //Get user role for operation plan has PDM edition
    GetUserRoleInfoAsync(SystemIdOps, GetMenuIdByEdition(editionPdm), (res) => {
        localStorage.setItem('UserRoleOpm', JSON.stringify(res));
    });

    //Get user role for operation plan has AOM and MES edition
    GetUserRoleInfoAsync(SystemIdOps, GetMenuIdByEdition(editionAom), (res) => {
        localStorage.setItem('UserRoleFom', JSON.stringify(res));
    });
}
//END ADD - SON) 18/Jan/2021

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
        error: function () {
            arrStyleFile = [];
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
function GetMaxOpRevision(edition, styleCode, styleSize, styleColor, revNo) {
    var data = { edition: edition, styleCode: styleCode, styleSize: styleSize, styleColor: styleColor, revNo: revNo };
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
async function GetMaxOpSerialAsync() {
    console.log("Get max of OpSerial.");

    let styleCode = "", styleSize = "", styleColorSerial = "", revNo = "", opRevNo = "", edition = "", result;

    try {
        // Get ops master from the grid.
        const objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

        if (!$.isEmptyObject(objOpsMaster)) {
            styleCode = objOpsMaster.StyleCode;
            styleSize = objOpsMaster.StyleSize;
            styleColorSerial = objOpsMaster.StyleColorSerial;
            revNo = objOpsMaster.RevNo;
            opRevNo = objOpsMaster.OpRevNo;
            edition = objOpsMaster.Edition;
        }
        const opdt = { edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo };

        result = await $.ajax({
            url: "/Ops/GetMaxOpSerial",
            async: true,
            type: "POST",
            data: JSON.stringify(opdt),
            dataType: "json",
            contentType: "application/json",
            error: (jqXhr, status, errorThrown) => {
                console.log(jqXhr);
                console.log(status);

                ShowMessage("Get Max Operation Serial", "Cannot get max Operation Plan serial!\n" + errorThrown.message, MessageTypeError);
                $("txtProcessNo").val("");
            }
        });

        return result;
    } catch (e) {
        console.error(e);
    }
}

function AsyncGetMaxOpSerial(callBack) {
    console.log("Get max of OpSerial.");

    let styleCode = "", styleSize = "", styleColorSerial = "", revNo = "", opRevNo = "", edition = "";

    //Get ops master from
    const objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

    if (!$.isEmptyObject(objOpsMaster)) {
        styleCode = objOpsMaster.StyleCode;
        styleSize = objOpsMaster.StyleSize;
        styleColorSerial = objOpsMaster.StyleColorSerial;
        revNo = objOpsMaster.RevNo;
        opRevNo = objOpsMaster.OpRevNo;
        edition = objOpsMaster.Edition;
    }
    const opdt = { edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo };

    $.ajax({
        url: "/Ops/GetMaxOpSerial",
        async: true,
        type: "POST",
        data: JSON.stringify(opdt),
        dataType: "json",
        contentType: "application/json",
        success: (maxSerial) => {
            callBack(maxSerial);
        },
        error: (jqXhr, status, errorThrown) => {
            console.log(jqXhr);
            console.log(status);

            ShowMessage("Get Max Operation Serial", "Cannot get max Operation Plan serial!\n" + errorThrown.message, MessageTypeError);
            $("txtProcessNo").val("");
        }
    });
}

function GetMaxOpSerial() {
    console.log("Get max of OpSerial.");

    var styleCode = "";
    var styleSize = "";
    var styleColorSerial = "";
    var revNo = "";
    var opRevNo = "";
    var edition = "";

    //Get ops master from 
    //var objOpsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

    if (!$.isEmptyObject(objOpsMaster)) {
        styleCode = objOpsMaster.StyleCode;
        styleSize = objOpsMaster.StyleSize;
        styleColorSerial = objOpsMaster.StyleColorSerial;
        revNo = objOpsMaster.RevNo;
        opRevNo = objOpsMaster.OpRevNo;
        edition = objOpsMaster.Edition;
    }

    var opDetail = {
        edition: edition, styleCode: styleCode, styleSize: styleSize
        , styleColorSerial: styleColorSerial,
        revNo: revNo, opRevNo: opRevNo
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

            console.log("Assign OpSerial to textbox");
            $("#txtProcessNumber").val(Number(maxSerial));
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessage("Get Max Operation Serial", "Cannot get max Operation Plan serial!\n" + errorThrown.message, MessageTypeError);
            $("txtProcessNo").val("");
        }
    });
}

//Get object ops detail by code
function GetObjectOpsDetail(objOpDetail) {
    var resObjOpDetail;
    $.ajax({
        url: "/Ops/GetOpDetailByCode",
        async: false,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            resObjOpDetail = res;
        },
        error: function (jqXhr, status, err) {
            ShowMessage("Get ops detail", "Cannot get ops detail!\n" + err.messages, MessageTypeError);
        }
    });

    return resObjOpDetail;
}

function GetOpdtAsync(objOpDetail, callBack) {
    $.ajax({
        url: "/Ops/GetOpDetailByCode",
        async: true,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail }),
        dataType: "json",
        contentType: "application/json",
        success: (res) => {
            callBack(res);
        },
        error: function (jqXhr, status, err) {
            ShowMessage("Get ops detail", "Cannot get ops detail!\n" + err.messages, MessageTypeError);
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
    //jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + styleCode + " | Size: " + styleSize
    //    + " | Color: " + styleColor + " | Revision: " + revNo);

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
    var oDropdown = $("#" + idDropdowlist).msDropdown().data("dd");
    var index = 0;
    if (oDropdown != null) {
        for (var i = 0; i < oDropdown.length; i++) {
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

/**
 * * Count processes with standard name
 * @param {any} edition
 * @param {any} languageId
 * @param {any} styleCode
 * @param {any} styleSize
 * @param {any} styleColorSerial
 * @param {any} revNo
 * @param {any} opRevNo
 */
function countProcessesWithStandardName(edition, languageId, styleCode, styleSize, styleColorSerial, revNo, opRevNo) {
    var config = ObjectConfigAjaxPost("/Ops/GetNumberOfProcesses", false, JSON.stringify({ edition: edition, languageId: languageId, styleCode: styleCode, styleSize: styleSize, styleColorSerial: styleColorSerial, revNo: revNo, opRevNo: opRevNo }));
    var countPro = 0;
    AjaxPostCommon(config, function (res) {
        countPro = res;
    });

    return countPro;
}

/**
 * Get list of factory
 */
function getFactories() {
    let listFactories = [];
    var config = ObjectConfigAjaxPost("/Ops/GetFactory", false, JSON.stringify({}));
    AjaxPostCommon(config, function (res) {
        listFactories = res;
    });

    return listFactories;
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
function CreateObjectOpsDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, opNum, opGroup, opName, opTime, edition, opNameRef, machineName, moduleName) {
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
        MachineName: machineName,
        ModuleName: moduleName //ADD - SON) 18/Jul/2020
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
            { name: 'TotalOpTime', index: 'TotalOpTime', width: 90, label: "Total Time", align: 'center', classes: 'pointer' },
            { name: 'OpPrice', index: 'OpPrice', width: 90, label: arrOpsColname.OPPRICE, align: 'center', classes: 'pointer', hidden: true },
            { name: 'MachineCount', index: 'MachineCount', width: 115, label: arrOpsColname.MACHINECOUNT, align: 'center', classes: 'pointer' },
            { width: 60, label: arrOpsColname.CONFIRMCHK, align: 'center', classes: 'pointer', formatter: showIconConfirmed },
            { name: 'OpCount', index: 'OpCount', width: 90, label: arrOpsColname.OPCOUNT, align: 'center', classes: 'pointer' },
            { name: 'ManCount', index: 'ManCount', width: 90, label: arrOpsColname.MANCOUNT, align: 'center', classes: 'pointer' },
            { name: 'Factory', index: 'Factory', width: 90, label: arrOpsColname.FACTORY, align: 'center', classes: 'pointer' },
            { name: 'LastUpdateTime', index: 'LastUpdateTime', width: 150, label: arrOpsColname.LASTUPDATEDATE, align: 'left', classes: 'pointer', formatter: convertDateToString },
            { name: 'Remarks', index: 'Remarks', width: 250, label: arrOpsColname.REMARKS, align: 'left', classes: 'pointer' },
            { name: 'MxPackage', index: 'MxPackage', width: 250, label: arrOpsColname.MXPACKAGE, align: 'left', classes: 'pointer' }, //ADD) SON - 1/Jul/2019
            { name: 'Edition', index: 'Edition', width: 90, hidden: true },
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
            { name: 'ConfirmedId', index: 'ConfirmedId', hidden: true },
            { name: 'GridKey', index: 'GridKey', key: true, hidden: true, formatter: createGridOpmtKey }//ADD - SON) 4/Sep/2020
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
            //var row = $(gridOpsTableId).jqGrid("getRowData", rowid);
            //row.RowId = rowid;
            //OpsMasterFunction(row);

            //jQuery(gridOpsTableId).jqGrid('setCaption', " OPS - Style: " + row.StyleCode + " | Size: " + row.StyleSize
            //    + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Op Revision: " + row.OpRevNo);

            ////Save ops master key to local storage
            //localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

            //CheckMachineOpsTable(); // HA ADD //MOD - SON - 2021.01.15) 16/Jan/2021

            //START ADD) SON - 2019.03.1.0 - 12/Mar/2019
            //let row = $(gridOpsTableId).jqGrid("getRowData", rowid);
            //GetModulesByStyleCode(row.StyleCode);
            //END ADD) SON - 2019.03.1.0 - 12/Mar/2019

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
                        //jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
                        //    + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Op Revision: " + row.OpRevNo);

                        jQuery(gridOpsTableId).jqGrid('setCaption', row.StyleCode + ' - ' + row.BuyerStyleName + " | " + row.StyleSize
                            + ' | ' + row.StyleColorWays + " | REV - " + row.RevNo + " | OPREV - " + row.OpRevNo);

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
                    var let = $(gridOpsTableId).jqGrid("getRowData", rowid);
                    OpsMasterFunction(row);

                    //jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
                    //    + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Op Revision: " + row.OpRevNo);

                    jQuery(gridOpsTableId).jqGrid('setCaption', row.StyleCode + ' - ' + row.BuyerStyleName + " | " + row.StyleSize
                        + ' | ' + row.StyleColorWays + " | REV - " + row.RevNo + " | OPREV - " + row.OpRevNo);

                    //Save ops master key to local storage
                    localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

                    return true;
                }
            } else {
                IsSelectRow = true;
                var row = $(gridOpsTableId).jqGrid("getRowData", rowid);
                OpsMasterFunction(row);

                //jQuery(gridOpsTableId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
                //    + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Op Revision: " + row.OpRevNo);

                jQuery(gridOpsTableId).jqGrid('setCaption', row.StyleCode + ' - ' + row.BuyerStyleName + " | " + row.StyleSize
                    + ' | ' + row.StyleColorWays + " | REV - " + row.RevNo + " | OPREV - " + row.OpRevNo);

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

    //START ADD - SON) 4/Sep/2020
    function createGridOpmtKey(cellValue, options, rowObject) {
        return rowObject.Edition + rowObject.StyleCode + rowObject.StyleSize + rowObject.StyleColorSerial + rowObject.RevNo + rowObject.OpRevNo;
    }
    //END ADD - SON) 4/Sep/2020

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

// #endregion

//#region Common Ajax

class AjaxConfig {
    constructor(url, async, postData) {
        this.url = url;
        this.async = async;
        this.postData = postData;
    }
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

async function PostAjaxAsync(config) {
    let result;
    try {
        result = await $.ajax({
            type: "POST",
            async: config.async,
            url: config.url,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: config.postData
        });

        return result;
    } catch (error) {
        console.error(error);
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

function AjaxUploadFile(config, callBack) {
    /// <summary>
    /// Ajax function to upload file.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="callBack">The call back.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang

    const request = $.ajax({
        type: "POST",
        async: config.async,
        url: config.url,
        data: config.postData,
        dataType: "json",
        contentType: false,
        processData: false
    }).done(function (response) {
        callBack(response);
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

function GetMasterCodes(url, mCode, callBack) {
    /// <summary>
    /// Gets list of objects from master codes table.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="mCode">The master code.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang

    $.post(url, { mCode: mCode }).then(function (response) {
        callBack(response);
    }, function (error) {
        window.ShowAjaxError(error, url);
    });
}
//#endregion

// #region Jqgrid

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

//Get all rows on jqgrid - MOD - SON) 4/Jan/2021 - get row with customize data
function GetAllRowsDataJqGrid2(jqGridId) {
    //var gridData = jQuery(gridOpsDetailId).jqGrid("getRowData");
    var gridData = jQuery(jqGridId).jqGrid("getRowData");
    return gridData;
}

const getSelectedRowsSubGrid = (jqGridId) => {
    let listSelectedRow = [];
    let gridBom = $(jqGridId)[0], rows = gridBom.rows, cRows = rows.length, iRow, row, trClasses;
    for (iRow = 0; iRow < cRows; iRow++) {
        row = rows[iRow]; // row.id is the rowid
        trClasses = row.className.split(' ');

        if ($.inArray('ui-subgrid', trClasses) > -1) {
            // the row contains subgrid (only if subGrid:true are used)
            var subgridTable = $(row).find("table.ui-jqgrid-btable:first");
            let subGridTableId = subgridTable[0].id;
            //Get selected row in sub gridview
            let selectedRowSubGrid = GetSelectedMultipleRowsData("#" + subGridTableId);
            if (selectedRowSubGrid !== null) {
                //push selected row to temporary variable
                selectedRowSubGrid.forEach((selPt) => {
                    listSelectedRow.push(selPt);
                });
            }
        }
    }

    return listSelectedRow;
}
// #endregion

// #region Play Video
//START ADD) SON (2019.09.20) - 20 September 2019 - add event copy video link
function CopyVideoLinkProcess() {
    /* Get the text field */
    var copyText = document.getElementById("txtVideoLink");

    /* Select the text field */
    copyText.select();
    copyText.setSelectionRange(0, 99999); /*For mobile devices*/

    /* Copy the text inside the text field */
    document.execCommand("copy");

    var tooltip = document.getElementById("spToolTipCopyLink");
    tooltip.innerHTML = "Copied";

    /* Alert the copied text */
    ///alert("Copied the text: " + copyText.value);
}

function outCopyProcessLink() {
    var tooltip = document.getElementById("spToolTipCopyLink");
    tooltip.innerHTML = "Copy to clipboard";
}
//END ADD) SON (2019.09.20)- 20 September 2019

function PauseProcessVideo() {
    $('#vdoOpsDetail').get(0).pause();
}

function PlayVideo(rowObj) {

    //START ADD) SON (2019.09.20) - 20 September 2019 - Adding video link to textbox
    //const videosLink = rowObj.VideoOpLink.replace("~", "").replace("/", "\\");
    const l = rowObj.VideoOpLink.includes("pkfile") ? `${window.location.host}\\${rowObj.VideoOpLink}` : rowObj.VideoOpLink;
    console.log(l);

    $("#txtVideoLink").val(l);
    //END ADD) SON (2019.09.20)

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

// #region Validate email

function UpdateEmail() {
    $("#btnUpdateEmail").click(function (e) {
        var sEmail = $('#txtEmail').val();
        if ($.trim(sEmail).length == 0) {
            ShowMessage("Update Email", "Please enter valid email address", MessageType.Warning);
            e.preventDefault();
        } else {
            if (ValidateEmail(sEmail)) {
                var config = ObjectConfigAjaxPost("/Ops/UpdateEmail", true, JSON.stringify({ sEmail: sEmail }));
                AjaxPostCommon(config, function (respone) {
                    if (respone === Success) {
                        ShowMessage("Update Email", "Updated email", MessageType.Success);
                    } else {
                        ShowMessage("Update Email", "Cannot update email", MessageType.Error);
                    }
                });
            }
            else {
                ShowMessage("Update Email", "Invalid Email Address", MessageType.Warning);
                e.preventDefault();
            }
        }
    });
}

function GetUserInfomation(userName, callBackFunc) {
    //Get user information
    var config = ObjectConfigAjaxPost("/Account/GetUserInforByUserName", true, JSON.stringify({ userName: userName }));
    AjaxPostCommon(config, function (respone) {
        if (respone.status === Success) {
            callBackFunc(respone.usmt);
        } else {
            callBackFunc("");
        }
    });
}

function ShowModalUpdateEmail() {
    var userName = $("#hdUsername").val();
    if ($.isEmptyObject(userName)) {
        ShowMessage("Update Email", "Please check username.", MessageType.Warning);
        return;
    }

    GetUserInfomation(userName, function (objUsmt) {
        $("#txtEmail").val(objUsmt.Email);
    });

    ShowModal("mdlUpdateEmail");
}

//Show modal to compose email
function ShowModalEmailProcessVideo() {
    $("#btnSendEmailProcessVideo").click(function () {

        var userName = $("#hdUsername").val();
        GetUserInfomation(userName, function (objUsmt) {
            if (!$.isEmptyObject(objUsmt)) $("#txtCcAddress").val(objUsmt.Email);
        });

        ShowModal("mdlEmailProcess");
    });
}

function SendEmailProcess() {
    $("#btnSendEmail").click(function () {
        var toAddress = $("#txtToAddress").val();
        var ccAddress = $("#txtCcAddress").val();
        var subject = $("#txtSubject").val();
        var contenEmail = $("#txtEmailContent").val();

        if ($.isEmptyObject(toAddress) || $.isEmptyObject(subject) || $.isEmptyObject(contenEmail)) {
            ShowMessage("Send Email", "Please enter \"To Address, Subject and Content\" of email.", MessageType.Success);
            return;
        }

        var content = $("#lblProcessInfo").text() + "<br/>" + $("#lblProcessName").text() + "<br/><br/>";
        content += contenEmail;
        //Sending email
        SendEmail(toAddress, ccAddress, subject, content);

    });
}

//Send email function
function SendEmail(toAddress, ccAddress, subject, content) {
    var config = ObjectConfigAjaxPost("/Ops/SendEmailProcess", true
        , JSON.stringify({
            toAddress: toAddress, ccAddress: ccAddress, subject: subject, content: content
        }));
    AjaxPostCommon(config, function (respone) {
        if (respone === Success) {
            $("#txtToAddress").val("");
            $("#txtCcAddress").val("");
            $("#txtEmailContent").val("");
            ShowMessage("Send Email", "Email send", MessageType.Success);
        } else {
            ShowMessage("Send Email", "Cannot send email. " + respone, MessageType.Error);
        }
    });
}

// #endregion

// #region Message

function ShowMessageOk(contextSerial, func, mesType, mesContext, typeShowMes, mesReplace) {
    GetMsg(contextSerial, SystemIdOps, MenuIdOpm, func, mesType, mesContext, language, function (result) {
        var strMes = ReplaceStr(result.value, mesReplace);
        ShowMessage(result.title, strMes, typeShowMes);
    });
}

function ShowConfirmYesNoMessage(ContextSerial, event, mesType, mesContext, yesFunc, noFunc, mesReplace) {
    GetMsg(ContextSerial, SystemIdOps, MenuIdOpm, event, mesType, mesContext, language, function (result) {
        var strMes = ReplaceStr(result.value, mesReplace);
        ConfirmYesNo(result.title, strMes, yesFunc, noFunc);
    });
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

//#region Generate GUID
function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
//#endregion
//#endregion

//#region event click button
//START ADD - SON) 3/Sep/2020
//Event click on button in layout screen - 3/Sep/2020
function EventClickButtonLayout() {
    $('#achRecentPlan, #achRecentPlan2').click(function () {
        ShowModal('mdlSearchRecentPlan');
        FillDataToDropdownlistLayout();

        BindDataToGridRecentPlan(null, null, null);
    });

    $('#btnSearchRecentPlan').click(function () {
        //Create object
        let searchObj = {
            buyer: $('#drpBuyerRecentPlanMdl').val(),
            styleInf: $('#txtStyleInfRecentPlanMdl').val(),
            recentDay: $('#drpRecentTimePlanMdl').val()
        }

        //Reload grid recent operation plan
        ReloadJqGrid2LoCal("tbRecentPlan", searchObj);
    });
}

function FillDataToDropdownlistLayout() {
    var arrBuyer = RenameBuyer(GetArrayBuyer());
    FillDataToDropDownlist("drpBuyerRecentPlanMdl", arrBuyer, "BuyerCode", "BuyerName");

    Selection2("drpRecentTimePlanMdl");
}

function RenameBuyer(arrBuyer) {
    $.each(arrBuyer, function (idx, value) {
        value.BuyerName = value.BuyerCode + ' - ' + value.BuyerName;
    });

    return arrBuyer;
}

function BindDataToGridRecentPlan(buyer, styleInf, recentDay) {

    jQuery('#tbRecentPlan').jqGrid({
        pager: 'divRecentPlanPager',
        page: 1,
        rowNum: 40,
        rowList: [40, 60, 80, 20],
        scroll: false,
        viewrecords: true,
        scrollrows: true,
        shrinkToFit: false,
        width: null,
        gridview: true,
        height: 300,
        url: "/UIControl/SearchRecentPlan",
        datatype: "json",
        postData: {
            buyer: buyer, styleInf: styleInf, recentDay: recentDay
        },
        colModel: [
            { name: "Edition2", index: "Edition2", label: 'Edition', align: "center", width: 100 },
            { name: "StyleCode", index: "StyleCode", label: 'STYLE CODE', align: "center", width: 100 },
            { name: "StyleSize", index: "StyleSize", label: 'STYLE SIZE', align: "center", width: 80 },
            { name: "StyleColorWays", index: "StyleColorWays", label: 'COLOR', width: 150 },
            { name: "RevNo", index: "RevNo", label: 'REVNO', align: "center", width: 80 },
            { name: "OpRevNo", index: "OpRevNo", label: 'OP REVNO', align: "center", width: 80 },
            { name: "StyleName", index: "StyleName", label: 'STYLENAME', width: 150 },
            { name: "BuyerStyleCode", index: "BuyerStyleCode", label: 'BUYER STYLE CODE', width: 150 },
            { name: "BuyerStyleName", index: "BuyerStyleName", label: 'BUYER STYLE NAME', width: 150 },
            { name: "FactoryName", index: "FactoryName", label: 'FACTORY', width: 150 },
            { name: "LastUpdateTime", index: "LastUpdateTime", label: 'LAST UPDATE', formatter: 'date', formatoptions: { newformat: 'd-M-Y H:m:s' } },
            { name: "RegistryDate", index: "RegistryDate", label: 'REGISTRY DATE', formatter: 'date', formatoptions: { newformat: 'd-M-Y H:m:s' } },
            { name: "RegisterName", index: "RegisterName", label: 'REGISTER', align: "left" },
            { name: "RegisterId", index: "RegisterId", hidden: true },
            { name: "StyleColorSerial", index: "StyleColorSerial", hidden: true },
            { name: "Edition", index: "Edition", hidden: true },
            { name: 'GridKey', index: 'GridKey', key: true, hidden: true, formatter: createGridOpmtKey }//ADD - SON) 4/Sep/2020
        ],
        ondblClickRow: function (rowid) {
            //  Get current page url using JavaScript
            var currentPageUrl = "";
            if (typeof this.href === "undefined") {
                currentPageUrl = document.location.toString().toLowerCase();
            }
            else {
                currentPageUrl = this.href.toString().toLowerCase();
            }

            //Check current page whether is default page or not
            if (currentPageUrl.indexOf("/default/default") !== -1) {

                var row = $('#tbRecentPlan').jqGrid("getRowData", rowid);
                //Save ops master key to local storage
                localStorage.setItem(StyleMasterInfo, JSON.stringify(row));

                //If current page is not default then navigate to plan management page
                //window.location.href = "/Ops/Ops";
                window.location.href = "/PlanManagement/PlanManagement";

                localStorage.setItem(OpsMasterInfo, JSON.stringify(row));
            }

            dbclick = 2;
            HideModal("mdlSearchRecentPlan");
        },
        onSelectRow: function (rowid) {
            setTimeout(function () {
                if (dbclick === 1) {
                    SelectRowRecentPlan(rowid);
                    dbclick = 1;
                } else {
                    dbclick = 1;
                }

                //get selected row data
                let row = $('#tbRecentPlan').jqGrid("getRowData", rowid);

                SetSelectionOpmtRow(row.GridKey);
            }, 300);
        },
        loadComplete: function () {
            updatePagerIcons();
        },
        onPaging: function (pgButton) {
            //if (pgButton === "records") {
            //    SetPaging(myJqgrid, tableNavName);
            //}
        }
    }).jqGrid("navGrid", "#divRecentPlanPager", {
        cloneToTop: true,
        edit: false,
        add: false,
        del: false,
        search: false,
        searchicon: "ace-icon fa fa-search orange",
        searchtext: 'Search',
        refresh: true, refreshicon: "ace-icon fa fa-refresh green", refreshtext: 'Refresh'
    });

    //START ADD - SON) 4/Sep/2020
    function createGridOpmtKey(cellValue, options, rowObject) {
        return rowObject.Edition + rowObject.StyleCode + rowObject.StyleSize + rowObject.StyleColorSerial + rowObject.RevNo + rowObject.OpRevNo;
    }
    //END ADD - SON) 4/Sep/2020
}

function SelectRowRecentPlan(rowid) {
    //START ADD - SON
    localStorage.removeItem(OpsMasterInfo);

    var row = $('#tbRecentPlan').jqGrid("getRowData", rowid);
    //Save ops master key to local storage
    localStorage.setItem(StyleMasterInfo, JSON.stringify(row));

    //END ADD - SON
    GetKeyCodeStyle(row.StyleCode, row.StyleSize, row.StyleColorSerial, row.RevNo);
}

function SetSelectionOpmtRow(gridKey) {
    //Get all rows data on grid opmt
    let gridData = GetAllRowsDataJqGrid(gridOpsTableId);
    //If grid data is empty then return
    if ($.isEmptyObject(gridData)) return;

    //Find grid key to set selection
    $.each(gridData, function (idx, opmt) {
        let opmtKey = opmt.Edition + opmt.StyleCode + opmt.StyleSize + opmt.StyleColorSerial + opmt.RevNo + opmt.OpRevNo;
        if (opmtKey === gridKey) {
            //jQuery(gridOpsTableId).jqGrid('setSelection', gridKey);
            $(gridOpsTableId).jqGrid("setSelection", idx + 1, true);
            return false;
        }
    });
}
//END ADD - SON) 3/Sep/2020
//#endregion 