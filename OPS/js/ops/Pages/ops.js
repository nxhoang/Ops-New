var _activeTab = 2; //Default acitve tab is 2

var gridOpsDetailId = "#gridOpsDetail";
var gridOpsDetailName = "gridOpsDetail";

var IdTableStyleModal = "#gridTableStyleModal";
var IdPaperStyleModal = "#gridPaperStyleModal";
var NameTableStyleModal = "gridTableStyleModal";
var NamePaperStyleModal = "gridPaperStyleModal";

var IdTableOpsModal = "#gridTableOpsModal";
var IdPaperOpsModal = "#gridPagerOpsModal";
var NameTableOpsModal = "gridTableOpsModal";
var NamePaperOpsModal = "gridPagerOpsModal";

var NameModalOpName = "mdlOpName";
var IdModalOpName = "#mdlOpName";

var GridOpsTableImportId = "#gridOpsTableImport";
var GridOpsTableImportName = "gridOpsTableImport";

var TableUpdateProcessTimeName = "tblUpdateProcessTime";
var TableUpdateProcessTimeId = "#tblUpdateProcessTime";

var TableProcessNameId = "#tbProcessNameTranslate";
var TableProcessName = "tbProcessNameTranslate";

var TableFileId = "#tblFiles";
var TableFileName = "tblFiles";

var TableFilesDownloadId = "#tblFilesDownload";
var TableFilesDownloadName = "tblFilesDownload";

var TableVideosId = "#tblVideo";
var TableVideosName = "tblVideo";

//Module
var TableModuleId = "#tbModule";
var TableModuleName = "tbModule";
var DivPagerModuleId = "#dvModulePager";
var DivPagerModuleName = "dvModulePager";

var TableModuleRevisionId = "#tbModuleRevision"
var TableModuleRevisionName = "tbModuleRevision"
var DivPagerModuleRevisionId = "#divModuleRevisionPager"
var DivPagerModuleRevisionName = "divModuleRevisionPager"

var DivMenuOpsRegistration = "dvOpsRegistration";
var DivMenuOpsDetail = "divOpPlan";

//Column Csv file
var GroupStartCol = 0;
var GroupNameCol = 1;
var ProcessNameCol = 1;
var ProcessTime3Col = 9;
var MachineTypeCol = 12;

//Recognize prosess data
var BlockStart = "-----";
var BlockEnd = "=====";
var GroupStart = "-";
var FileEnd = "*";

//Application language.
var ApplicationLanguage;

//Store list of proccess from csv file
var ListOpsDetail = [];
var AcceptData = [];

//Keep selected tool and machine in temporary 
var ArrSelectedTool = [];
var ArrSelectedMachine = [];
var ArrSelectedOpname = [];
var ArrListOpTime = {};

//Dms File
var StyleFile = "StyleFile";
var CadFile = "CAD File";
var MarkerFile = "Marker File";
var PrintingFile = "Printing";
var EmbroideryFile = "Embroidery Design";
var OthersFile = "Others";
//var StyleMasterInfo = "StyleMasterInfo"; //Ha add //MOD - SON) 29/Oct/2020 - SON comment due to this variable was declared in ops-common.js

//Keep data ops detail for update/delete.
var SeletedObjOpsDetail;

//Keep data for reload jqgrid Files PDM linking
var ObjFilesPdmLinking;

//Keep User Role
var UserRoleOpm;
var UserRoleFom;
var UserRoleMes;
var UserRoleCurrent;
var UserRoleStm;
var UserRoleBal;
var UserRoleMdl;

//Modal
var OpsModal = "opsModal";
var ImportCsvModal = "modalImportCsv";

var ArrOpsGroup = [];
var ArrOutsourceGroup = [];

//Icon of files
var ObjIcons = [];

var ISSHOWNONESTDNAME = false;

// #region Gridview column name

arrColNameStyle = {
    STYLECODE: 'Style Code',
    STYLENAME: 'Style Name',
    BUYERSTYLECODE: 'Buyer',
    BUYERSTYLENAME: 'Buyer Name',
    STYLESIZE: 'Style Size',
    STYLECOLORSERIAL: 'Color',
    REVNO: 'Revno',
    STATUS: 'Status',
    REGISTRYDATE: 'Register Date',
    REGISTER_NAME: 'Register',
    AD_CONFIRM: 'AD_CONFIRM',
    AD_DEV_SALES: 'Value'
};

arrOpsImport = {
    OPGROUP: 'Operation Group',
    OPNUM: 'OpNum',
    OPNAME: 'OPS Process Name',
    OPNAMEREF: 'GSD Process Name',
    OPTIME: 'Opeartion Time',
    MACHINETYPE: 'Machine Type'
};

arrColNameGridModule = {
    MODULEID: 'Module ID',
    MODULENAME: 'Module Name',
    ITEMNAME: 'Name',
    SUBGROUP: 'Sub Group'
};

arrColNameFiles = {
    LINKING: 'Linking',
    FILETYPE: 'File Type',
    FILENAME: 'File Name',
    AMENDNO: 'Amend No',
    REMARKS: 'Remarks',
    STYLECOLORWAYS: 'Color',
    REVNO: 'RevNo',
};

// #endregion

// #region Check user role

function SetRoleForControl(action, objRole, controlId, opsConfirm, edition) {

    var blAdd = false;
    var blUpd = false;
    var blDel = false;
    var blConf = false;
    var blExp = false;

    if ($.isEmptyObject(objRole) || opsConfirm === ConfirmCheck) {
        blAdd = false; blUpd = false; blDel = false; blConf = false; blExp = false;
    } else {
        blAdd = StringToBoolean(objRole.IsAdd);
        blUpd = StringToBoolean(objRole.IsUpdate);
        blDel = StringToBoolean(objRole.IsDelete);
        blConf = StringToBoolean(objRole.IsConfirm);
        blExp = StringToBoolean(objRole.IsExport);
    }

    switch (action) {
        case ReadOnly:
            DisabledButton(controlId, true);
            break;
        case AddRole:
            DisabledButton(controlId, !blAdd);
            break;
        case UpdateRole:
            DisabledButton(controlId, !blUpd);
            break;
        case DeleteRole:
            DisabledButton(controlId, !blDel);
            break;
        case ConfirmRole:
            DisabledButton(controlId, !blConf);
            break;
        case ExportRole:
            DisabledButton(controlId, !blExp);
            break;
        default:
    }
}

function CheckRoleStyleManagement(edition, opsConfirm) {
    //If user role style management is empty then get it from database.
    if ($.isEmptyObject(UserRoleStm)) {
        UserRoleStm = GetUserRoleInfo(SystemIdOps, MenuIdStm);
    }

    SetRoleForControl(UpdateRole, UserRoleStm, "btnUploadImageStyle", "", edition);
    SetRoleForControl(UpdateRole, UserRoleStm, "flImageDetail", "", edition);
}

function CheckRoleOperationManagement(edition, opsConfirm) {

    var opRole;
    //If edition is PDM or OPS then get OPM role 
    //Otherwise if edition is AOM then get FOM role
    if (edition === editionPdm || edition === editionOps) {
        opRole = UserRoleOpm;
    } else if (edition === editionAom) {
        opRole = UserRoleFom;
    } else if (edition === editionMes) {
        opRole = UserRoleMes;
    } else {
        SetMenuActionMode("divOpPlan", ReadOnly, opRole);
        //Set role on menu registration ops
        SetMenuActionMode("dvOpsRegistration", AddOnly, opRole);

        return;
    }

    if (opsConfirm === ConfirmCheck) {
        //Set role on menu registry process
        SetMenuActionMode("divOpPlan", ReadOnly, opRole);
        //Set role on menu registration ops
        SetMenuActionMode("dvOpsRegistration", AddOnly, opRole);
    } else {
        //START ADD - SON) 9/Sep/2020 - check add permission
        if (UserRoleOpm.IsAdd === "1" || UserRoleFom.IsAdd === "1" || UserRoleMes.IsAdd === "1") {
            opRole.IsAdd = '1';
        }
        //END ADD - SON) 9/Sep/2020

        //Set role on menu registry process
        SetMenuActionMode("divOpPlan", Init, opRole);
        //Set role on menu registration ops
        SetMenuActionMode("dvOpsRegistration", Init, opRole);
    }

    //Enable add button for operation plan with Factory edition
    if ($.isEmptyObject(opRole.OwnerId) && edition !== editionMes) {
        opRole = UserRoleOpm;
        if (opRole.OwnerId === null) {
            opRole = UserRoleFom;
        }
        SetMenuActionMode("dvOpsRegistration", AddOnly, opRole);
    }

    UserRoleCurrent = opRole;

    //SetRoleForControl(UpdateRole, UserRole, "btnExportCsvOpDetail", opsConfirm, edition);
    SetRoleForControl(UpdateRole, opRole, "btnSaveOpName", opsConfirm, edition);
}

function CheckRoleLineBalancing(edition, opsConfirm) {
    //If user role style management is empty then get it from database.
    if ($.isEmptyObject(UserRoleBal) || typeof UserRoleBal === 'undefined') {
        UserRoleBal = GetUserRoleInfo(SystemIdOps, MenuIdTbl);
    }

    SetRoleForControl(UpdateRole, UserRoleBal, "btnSaveBalancingTime", opsConfirm, edition);
    SetRoleForControl(ExportRole, UserRoleBal, "btnExportBalToExcel", false, edition);

}

function CheckRoleModuleManagement() {
    //If user role style management is empty then get it from database.
    if ($.isEmptyObject(UserRoleMdl)) {
        UserRoleMdl = GetUserRoleInfo(SystemIdOps, MenuIdMdl);
    }

    SetRoleForControl(AddRole, UserRoleMdl, "btnAddModule", "", "");

    if (UserRoleMdl.IsAdd !== "1") {
        //Disable dropdownlist.
        DisableModuleControl(true);
    }

}

function CheckUserRoleOps(edition, opsConfirm) {
    //Check role for style
    CheckRoleStyleManagement(edition, opsConfirm);
    //Check role for operation plan (Registry operation plans and processes)
    CheckRoleOperationManagement(edition, opsConfirm);
    //Check role for line balancing
    CheckRoleLineBalancing(edition, opsConfirm);

}

// #endregion

// #region Init data

//Init data for modal register new ops.
function InitDataForAddNewOpsModal() {
    var arrBuyer = GetArrayBuyer();
    FillDataToDropDownlist("drpBuyer", arrBuyer, "BuyerCode", "BuyerName");

    var arrTarget = CreateObjectOpsEdition();
    FillDataToDropDownlist("drpTargetEdition", arrTarget, "EditionCode", "EditionName");

    let arrFactoryOpmt = getFactories();
    FillDataToDropDownlist("drpFactoryOpmt", arrFactoryOpmt, "FactoryId", "FactoryName");

    BindDataToJqGridOpsModal("", "", "", "---", "---", "---");
    BindDataToJqGridStyleModal("-1", "", "", "-1", "-1");
    BindDataToJqGridProcessName("", "", "", "", "", "");

    CheckTargetEdition();

    //Clear data for importing csv file.
    AcceptData = [];
    $(GridOpsTableImportId).jqGrid('GridUnload');
    //Bind empty data for load the first time.
    BindDataToJqGridOpsImportModal([]);
    //Reload data for load data the second time
    ReloadJqGridLocal(GridOpsTableImportName, []);

    $("#rdSelectProcessName").prop('checked', false);
    $("#rdAddNewProcess").prop('checked', false);

    DisableOptionRegEmptyAndImpFile();
}

//Init data for the first time load page
function InitPage() {

    //Get user role, include role for operation management and factory role
    UserRoleOpm = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionPdm));
    UserRoleFom = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionAom));
    //START MOD) SON - 14/Jun/2019 - Get MES role like Factory role
    //UserRoleMes = GetUserRoleInfo(SystemIdMes, GetMenuIdByEdition(editionMes));
    UserRoleMes = GetUserRoleInfo(SystemIdOps, GetMenuIdByEdition(editionAom));
    //END MOD) SON - 14/Jun/2019

    //Get style master from local storage
    var objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));

    //Load style master information
    if (!$.isEmptyObject(objStyleMaster)) {

        //Set style key code master for hidden field
        $("#hdStyleCode").val(objStyleMaster.StyleCode);
        $("#hdStyleSize").val(objStyleMaster.StyleSize);
        $("#hdStyleColor").val(objStyleMaster.StyleColorSerial);
        $("#hdStyleRevNo").val(objStyleMaster.RevNo);

        //Get style master information
        //GetStyleMaster(objStyleMaster.StyleCode); //MOD - SON) 29/Oct/2020 - Don't get style information when initing page
    }

    //Get language for web page
    ApplicationLanguage = isEmpty(ApplicationLanguage) ? English : localStorage.getItem(LanguageId);

    //Init gridview operation master and detail
    BindDataToJqGridOpsDetail("", "", "", "", "", "", "");

    //Init dropdown list module. - must init before disable or enable multiple select
    MultipleSelect("drpModuleLevel");

    //Load grid ops master.
    LoadOpsMasterGrid();

    //Visible menu button of Operation plan master
    VisibleMenuButton("dvOpsRegistration", true, false, false, true, true, false);

    //Visible menu button of Operation plan detail
    VisibleMenuButton("divOpPlan", true, false, true, false, true, false);

    $("#drpLanguageOpMaster").msDropDown();
    $("#drpLanguages").msDropdown();
    Selection2("drpGroupShow");

    //Load operation pland detail follow operation plan master from local Storage
    var objOpsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));

    if ($.isEmptyObject(objOpsMaster)) {
        //Check user permission.
        CheckUserRoleOps("", "");
    }

    UserRoleCurrent = UserRoleOpm;
    if (UserRoleCurrent.OwnerId === null) {
        UserRoleCurrent = UserRoleFom;
    }
    $('#dvOpsRegistration .btnAdd').prop('disabled', !StringToBoolean(UserRoleCurrent.IsAdd));
    targetEditionOnchanged();

    //Get style files
    ObjIcons = GetStyleFiles();

    //START ADD) SON - 2019.03.1.0 - 09/Mar/2019
    loadTooltipForProcessNameTextbox();
    //START ADD) SON - 2019.03.1.0 - 09/Mar/2019

}

// #endregion

function GetStyleMaster(styleCode) {

    ////Check role
    //CheckRoleModuleManagement();

    GetStyleMasterByStyleCode(styleCode, function (styleMaster) {
        $("#txtStyleCodeDetail").val(styleMaster.StyleCode);
        $("#txtStyleNameDetail").val(styleMaster.StyleName);
        $("#txtOldStyleCodeDetail").val(styleMaster.OldStyleCode);
        $("#txtStyleGroupDetail").val(styleMaster.StyleGroupName);
        $("#txtStyleSubGroupDetail").val(styleMaster.StyleSubGroupName);
        $("#txtStyleSubSubGroupDetail").val(styleMaster.StyleSubSubGroupName);
        $("#txtBuyerDetail").val(styleMaster.Buyer);
        $("#txtBuyerCodeDetail").val(styleMaster.BuyerStyleCode);

        $("#txtStyleSize").val(styleMaster.StyleSize);
        $("#txtPriceDetail").val(styleMaster.UnitPrice);
        $("#txtCurrencyDetail").val(styleMaster.CurrCode);
        $("#txtInputUnitDetail").val(styleMaster.VolumeUnit);
        $("#txtInputVolumeDetail").val(styleMaster.Volume);
        $("#txtFactoryDetail").val(styleMaster.Factory);
        $("#txtBuyerNameDetail").val(styleMaster.BuyerStyleName);

        $("#txtStatus").val(styleMaster.Status);
        $("#txtRegisterDetail").val(styleMaster.StyleRegister);
        $("#txtPatternDesignerDetail").val(styleMaster.Designer);
        $("#txtItemManagerDetail").val(styleMaster.ItemManager);
        $("#txtItemDevmanagerDetail").val(styleMaster.ItemDMan);
        $("#txtTechnicianDetail").val(styleMaster.Technician);
        $("#txtConfirmedDetail").val(styleMaster.ConfirmedDate);
        $("#txtRegistryDateDetail").val(styleMaster.RegistryDate);

        //START ADD - SON) 29/Oct/2020
        $("#txtStyleCodeStlDt").val(styleMaster.StyleCode);
        $("#txtStyleNameStlDt").val(styleMaster.StyleName);
        $("#txtStyleCodeBuyerStlDt").val(styleMaster.BuyerStyleCode);
        $("#txtStyleNameBuyerStlDt").val(styleMaster.BuyerStyleName);
        $("#txtRegistrarStlDt").val(styleMaster.StyleRegister);
        $("#txtRegistryDateStlDt").val(styleMaster.RegistryDate);
        $("#txtEfficencyStlDte").val();
        $("#txtSAHStlDt").val();
        //END ADD - SON) 29/Oct/2020

        var imgPath;
        if (!isEmpty(styleMaster.ImageLink)) {
            imgPath = styleMaster.ImageLink;
        } else {
            imgPath = "/img/no-image.png";
        }

        $("#imgPreviewDetail").attr("src", imgPath);
    });
}

function Tooltip() {
    $('[data-toggle="tooltip"]').tooltip();
}

// #region Event row click gridview Ops master

//Define for Opsmaster
function OpsMasterFunction(row) {

    $("#hdConfChkOpsMaster").val(row.ConfirmChk);

    var editionRow = !isEmpty(row.Edition) ? row.Edition.slice(0, 1).toUpperCase() : "";

    CheckUserRoleOps(editionRow, row.ConfirmChk);

    GetDataBaseOnActiveTab(row);

    //Keep Operation plan master revision 
    $("#hdOpRevNo").val(row.OpRevNo);
    $("#hdOpDetailEdition").val(editionRow);

    //Keep and show language of ops master
    $("#hdOpsLanguage").val(row.Language);

    //Select language.
    var opsLanId = MapLanguageToFlag(row.Language);
    SetValueForLanguage("drpLanguages", opsLanId);

    //jQuery(gridOpsDetailId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
    //    + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + " | Edition: " + row.Edition2 + " | Op Revision: " + row.OpRevNo);

    jQuery(gridOpsDetailId).jqGrid('setCaption', row.StyleCode + ' - ' + row.BuyerStyleName + " | " + row.StyleSize
        + ' | ' + row.StyleColorWays + " | REV " + row.RevNo + " | OPREV - " + row.OpRevNo
        + ' | Edition - ' + row.Edition2);
}

function GetGroupTypeVaule(groupType) {
    var group;
    switch (groupType) {
        case "OpGroup":
            group = "GN";
            break;
        case "ModuleType":
            group = "GM";
            break;
        case "MachineType":
            group = "MC";
            break;
        default:
            group = "GN";
            break;
    }

    return group;
}

function GetDataForOperationTab(dataRow) {
    var styleCodeRow = dataRow.StyleCode;
    var styleSizeRow = dataRow.StyleSize;
    var styleColorRow = !isEmpty(dataRow.StyleColorWays) ? dataRow.StyleColorWays.slice(0, 3).toUpperCase() : "";
    var styleRevNoRow = dataRow.RevNo;
    var styleOpRevNoRow = dataRow.OpRevNo;
    var editionRow = !isEmpty(dataRow.Edition) ? dataRow.Edition.slice(0, 1).toUpperCase() : "";
    var confChk = dataRow.ConfirmChk;
    var langId = dataRow.Language;

    var data = {
        styleCode: styleCodeRow,
        styleSize: styleSizeRow,
        styleColor: styleColorRow,
        revNo: styleRevNoRow,
        opRevNo: styleOpRevNoRow,
        edition: editionRow,
        languageId: MapLanguageToFlag(langId)
    };

    //Note: must load data the first time (the first time loading empty data), 
    //after that can ReloadJqGrid function. This function can work.
    ReloadJqGrid(gridOpsDetailName, data);// Move to bottom

    //Hide column operation name language
    ShowOpNameColumGridOpDetail(gridOpsDetailId, dataRow.Language, dataRow.Language);

    //Clear caption gridview process
    //jQuery(gridOpsDetailId).jqGrid('setCaption', " Style Code: " + styleCodeRow + " | Size: " + styleSizeRow
    //    + " | Color: " + styleColorRow + " | Revision: " + styleRevNoRow + "|  Edtion: " + dataRow.Edition2 + " | Op Revision: " + styleOpRevNoRow);

    jQuery(gridOpsDetailId).jqGrid('setCaption', dataRow.StyleCode + ' - ' + dataRow.BuyerStyleName + " | " + dataRow.StyleSize
        + ' | ' + dataRow.StyleColorWays + " | REV " + dataRow.RevNo + " | OPREV - " + dataRow.OpRevNo
        + ' | Edition - ' + dataRow.Edition2);

    SetValueForLanguage("drpLanguages", MapLanguageToFlag(langId));

    //Hide button save operatoin name.
    $("#btnSaveOpName").hide();

    //Load download modal jqgrid
    BindDataToJqGridDownloadMachineFiles("", "", "", "", "", "", "", "");
    //Init file gridview.
    BindDataToJqGridMachineFile("", "", "", "", "", "", "", "", "", "");
    //Init video link gridview
    BindataToGridVideos("", "", "", "");

    //Group process detail.
    if ($("#drpGroupShow").val() !== dataRow.GroupMode) {
        $("#drpGroupShow").val(GetGroupTypeVaule(dataRow.GroupMode)).trigger("change"); //Grid process load again
    }
}

function GetDataForLineBalancingTab() {

    //Generate char of process
    GenerationFusionChart($("#drpChartType").val(), $("#drpShowChart").val());

    //Set text for button save balancing time
    $("#btnSaveBalancingTime").text("").append("<span class='fa fa-edit'></span> Edit");
}

//START ADD - SON) 29/Oct/2020
const getDataForStyleDetailTab = (selOpmt) => {

    const sah = selOpmt.ManCount * selOpmt.OpTime;
    $('#txtSAHStlDt').val(sah);

    var config = ObjectConfigAjaxPost("/Ops/GetEfficiency", true, JSON.stringify({
        edition: selOpmt.Edition, styleCode: selOpmt.StyleCode, styleSize: selOpmt.StyleSize, styleColorSerial: selOpmt.StyleColorSerial, revNo: selOpmt.RevNo, opRevNo: selOpmt.OpRevNo
    }));
    AjaxPostCommon(config, function (respone) {
        $('#txtEfficencyStlDt').val(respone.Efficiency);
    });
}
//END ADD - SON) 29/Oct/2020

function GetDataBaseOnActiveTab(dataRow) {
    var tabStyleDetail = $("#achStyleDetail").parent().attr('class');
    var tabOperation = $("#achOperationPlan").parent().attr('class');
    var tabLineBalancing = $("#achLineBalancing").parent().attr('class');

    var activeSta = "active";
    if (tabStyleDetail === activeSta) {

        getDataForStyleDetailTab(dataRow);

    } else if (tabOperation === activeSta) {

        GetDataForOperationTab(dataRow);

    } else if (tabLineBalancing === activeSta) {

        GetDataForLineBalancingTab();

    }
}

// #endregion

// #region Bind data to gridview

// #region Tab Operation Plan

//Bind data to Ops detail gridview
function BindDataToJqGridOpsDetail(styleCode, styleSize, styleColor, revNo, opRevNo, edition, lanId) {

    jQuery(gridOpsDetailId).jqGrid({
        url: '/OPS/GetOpDetail',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColor: styleColor,
            revNo: revNo,
            opRevNo: opRevNo,
            edition: edition,
            languageId: lanId
        },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        gridview: true,
        multiselect: true,
        caption: "OPS Detail",
        colModel: [
            { name: 'HotSpot', index: 'HotSpot', label: " ", width: 25, classes: 'pointer', formatter: markHotSpot },
            { name: 'OpGroupName', index: 'OpGroupName', label: arrColNameOpsDetail.OPGROUPNAME, hidden: true, classes: 'pointer' },
            { name: 'ModuleName', index: 'ModuleName', label: arrColNameOpsDetail.MODULENAME, hidden: false, classes: 'pointer' },
            { name: 'OpNum', index: 'OpNum', width: 70, label: arrColNameOpsDetail.OPNUM, align: 'left', classes: 'pointer' },
            { name: 'OpName', index: 'OpName', width: 250, label: arrColNameOpsDetail.OPNAME, classes: 'pointer' },
            { name: 'OpNameLan', index: 'OpNameLan', width: 250, label: arrColNameOpsDetail.OPNAME, hidden: true, classes: 'pointer' },
            { name: 'OpTime', index: 'OpTime', width: 130, label: arrColNameOpsDetail.OPTIME, align: 'center', classes: 'pointer' },
            { name: 'OpPrice', index: 'OpPrice', width: 80, label: arrColNameOpsDetail.OPPRICE, align: 'center', classes: 'pointer', hidden: true }, //Ha add
            { name: 'Factory', index: 'Factory', width: 90, label: arrColNameOpsDetail.FACTORY, align: 'center', hidden: true, classes: 'pointer' },
            { name: 'FactoryName', index: 'FactoryName', width: 120, label: arrColNameOpsDetail.FACTORY, align: 'left', classes: 'pointer' },

            { name: 'ManCount', index: 'ManCount', width: 70, label: arrColNameOpsDetail.MANCOUNT, align: 'center', classes: 'pointer' },
            { name: 'MachineName', index: 'MachineName', width: 120, label: arrColNameOpsDetail.MACHINENAME, align: 'left', classes: 'pointer' },
            { name: 'MachineCount', index: 'MachineCount', width: 80, label: arrColNameOpsDetail.MACHINECOUNT, align: 'center', classes: 'pointer' },
            { name: 'OfferOpPrice', index: 'OfferOpPrice', width: 100, label: arrColNameOpsDetail.OFFEROPPRICE, align: 'center', classes: 'pointer', hidden: true }, //Ha add
            { name: 'MaxTime', index: 'MaxTime', width: 75, label: arrColNameOpsDetail.MAXTIME, align: 'center', classes: 'pointer' },
            { name: 'StitchCount', index: 'StitchCount', hidden: true }, // HA ADD
            {
                name: 'OrgFileName', index: 'OrgFileName', width: 70, label: 'Jig Image', align: 'center', classes: 'pointer'
                , formatter: function (cellvalue, options) {
                    if (cellvalue)
                        return "<img style='width:60px;height:20px' src='" + cellvalue + "' onclick=ShowImageDetail('" + options.rowId + "'); />";
                    return "";
                }
            },
            { name: 'BenchmarkTime', index: 'BenchmarkTime', width: 120, label: arrColNameOpsDetail.BENCHMARKTIME, align: 'center', classes: 'pointer' },
            { name: 'Remarks', index: 'Remark', width: 250, label: "Remarks", classes: 'pointer' },
            { width: 100, label: arrColNameOpsDetail.UPLOADFILE, align: 'center', formatter: uploadMachineFile, classes: 'pointer' },
            { name: 'PlayVideo', index: 'PlayVideo', width: 80, label: arrColNameOpsDetail.PLAYVIDEO, align: 'center', formatter: playVideo, classes: 'pointer' },
            { width: 100, label: arrColNameOpsDetail.DOWNLOADFILE, align: 'left', formatter: downloadMachineFile, classes: 'pointer' },
            { name: 'VideoFile', index: 'VideoFile', width: 150, label: arrColNameOpsDetail.VIDEO, align: 'center', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', width: 100, label: arrColNameOpsDetail.STYLECODE, hidden: true },
            { name: 'StyleSize', index: 'StyleSize', width: 100, label: arrColNameOpsDetail.STYLESIZE, hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSeiral', width: 100, label: arrColNameOpsDetail.STYLECOLORSERIAL, hidden: true },
            { name: 'RevNo', index: 'RevNo', width: 100, label: arrColNameOpsDetail.REVNO, hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', width: 100, label: arrColNameOpsDetail.OPREVNO, hidden: true },
            { name: 'OpSerial', index: 'OpSerial', width: 100, label: arrColNameOpsDetail.OPSERIAL, hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'HotSpot', index: 'HotSpot', hidden: true },
            { name: 'OpTimeBalancing', index: 'OpTimeBalancing', hidden: true },
            { name: 'ToolId', index: 'ToolId', hidden: true },
            { name: 'MachineType', index: 'MachineType', hidden: true },
            { name: 'OpGroup', index: 'OpGroup', hidden: true },
            { name: 'ModuleId', index: 'ModuleId', hidden: true },
            { name: 'ActionCode', index: 'ActionCode', hidden: true },
            { name: 'VideoOpLink', index: 'VideoOpLink', hidden: true },
            { name: 'ImageLink', index: 'ImageLink', hidden: true },
            { name: 'HasFile', index: 'HasFile', hidden: true },
            { name: 'HasManyFiles', index: 'HasManyFiles', hidden: true },
            { name: 'FileNameOpfl', index: 'FileNameOpfl', hidden: true },
            { name: 'NoneStd', index: 'NoneStd', hidden: true } //ADD) SON - 21/Jun/2019 - remark process is standard name

        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Ops Detail", err.message, MessageTypeError);
        },
        grouping: true,
        groupingView: {
            groupField: ['OpGroupName'],
            groupColumnShow: [false],
            groupText: ["Group Name: {0} - {1} Item(s)"],
            groupCollapse: false,
            plusicon: "ace-icon fa fa-plus",
            minusicon: "ace-icon fa fa-minus"
        },
        onSelectRow: function (rowid) {
            var row = $(gridOpsDetailId).jqGrid("getRowData", rowid);
            var oldProName = row.OpName;
            //jQuery(gridOpsDetailId).jqGrid('setCaption', "Style Code: " + row.StyleCode + " | Size: " + row.StyleSize
            //    + " | Color: " + row.StyleColorSerial + " | Revision: " + row.RevNo + "|  Edtion: " + MapOperationPlanEditioin(row.Edition)
            //    + " | Op Revision: " + row.OpRevNo + " | Op Serial: " + ZeroPad(row.OpSerial, 3));

            let selOpmtRow = GetSelectedOneRowData(gridOpsTableId);
            jQuery(gridOpsDetailId).jqGrid('setCaption', selOpmtRow.StyleCode + ' - ' + selOpmtRow.BuyerStyleName + " | " + selOpmtRow.StyleSize
                + ' | ' + selOpmtRow.StyleColorWays + " | REV " + selOpmtRow.RevNo + " | OPREV - " + selOpmtRow.OpRevNo
                + ' | OpSerial - ' + ZeroPad(row.OpSerial, 3) + ' | Edition - ' + MapOperationPlanEditioin(row.Edition));
        },
        loadComplete: function () {
            //var rows = $("tr", gridOpsDetailId).length;
            //for (var i = 0; i < rows; i++) {
            //    var rowData = jQuery(this).getRowData(i); 
            //    if (rowData.HotSpot === "1") {
            //        jQuery(gridOpsDetailId).jqGrid("setRowData", i, false, "row-color");
            //    }
            //}
        },
        ondblClickRow: function (rowid, abc) {
            //jQuery(this).jqGrid('editGridRow', rowid);
            ShowModal(ProcessModal);
            var rowData = jQuery(gridOpsDetailId).jqGrid("getRowData", rowid);
            var objOpsDetail = GetObjectOpsDetail(rowData);

            //Get selected operation plan

            var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
            objOpsDetail.LanguageId = objOpsMaster.Language;

            StatusUpdateProcess = 3; //Set view mode.

            InitDataForProcessModal();

            //MOD) SON - 2019.03.1.0 - 11/Mar/2019 - remove timeout due to master data already was loaded when click "Opeartion Plan" tab.
            //setTimeout(function () {
            LoadObjectOpDetailModal(objOpsDetail);
            //}, 100);

            $('#btnUpdateProcess').hide();
            $('#btnSaveProcess').hide();

            //alert(rowData);
        },
        ajaxGridOptions: { async: false },
        rowattr: function (rd) {
            if (ISSHOWNONESTDNAME) {
                if (rd.NoneStd === "Y") {
                    return { "style": "background-color:#FFFF99;" };
                }
            }

        }
    });

    function markHotSpot(cellvalue, options, rowObject) {
        if (cellvalue === "1") {
            return "<i class='fa fa-flag' style='color: red'></i> ";
        }
        return "";
    }

    function uploadMachineFile(cellvalue, options, rowObject) {
        //START MOD) SON - 16 Jan 2019 - do not check confirmed status
        //var confChk = $("#hdConfChkOpsMaster").val();
        //var btnDisabled = (UserRoleCurrent.IsUpdate !== "1" || confChk === ConfirmCheck) ? 'disabled' : "";

        //return "<button type='button' class='btn btn-primary btn-modal' " + btnDisabled + " onclick='UploadFileOpDetail(" + JSON.stringify(rowObject) + ")'>" +
        //    "<i class='glyphicon glyphicon-upload'></i> </button>";

        //var confChk = $("#hdConfChkOpsMaster").val();
        let btnDisabled = 'disabled';
        switch (rowObject.Edition) {
            case editionPdm:
                btnDisabled = UserRoleOpm.IsUpdate !== "1" ? 'disabled' : "";
                break;
            case editionAom:
                btnDisabled = UserRoleFom.IsUpdate !== "1" ? 'disabled' : "";
                break;
            case editionMes:
                btnDisabled = UserRoleMes.IsUpdate !== "1" ? 'disabled' : "";
                break;
            default:
                btnDisabled = UserRoleCurrent.IsUpdate !== "1" ? 'disabled' : "";
        }

        //var btnDisabled = UserRoleCurrent.IsUpdate !== "1" ? 'disabled' : "";

        return "<button type='button' class='btn btn-primary btn-modal' " + btnDisabled + " onclick='UploadFileOpDetail(" + JSON.stringify(rowObject) + ")'>" +
            "<i class='glyphicon glyphicon-upload'></i> </button>";
        //END MOD) SON
    }

    function downloadMachineFile(cellvalue, options, rowObject) {
        //START MOD) SON - hide button view files if process does not have file to download
        if (rowObject.HasFile === "Y") {
            var btnDisabled = UserRoleCurrent.IsExport !== "1" ? 'disabled' : "";

            //Set icon for file download
            var startImg = "<img ";
            var srcImg = " onerror='imgErrorIcon(this);' src='../img/icons/";
            var endImg = " height='30'/>";
            var path = "<img src='../img/icons/csv.png' height='15'/>";
            var icon = "";
            var iconPath = "";

            var strExt = "";
            if (rowObject.HasManyFiles === "Y") {
                //If process has many files then get default icon is files icon
                icon = "files.png";
                srcImg += icon + "'";
                strExt = "...";
            } else {
                //Get file name
                var fileName = rowObject.FileNameOpfl;
                var extFile = fileName.split('.').pop().toUpperCase();
                $.each(ObjIcons, function (idx, value) {
                    if (extFile === value.CodeDetail2) {
                        icon = value.CodeDetail;
                        strExt = extFile.toLowerCase();
                    }
                });

                srcImg += icon + "'";

            }

            iconPath = startImg + srcImg + endImg;

            return "<button type='button' style='background-color:transparent; border-color:transparent;' " + btnDisabled + " onclick='DownloadFileOpDetail(" + JSON.stringify(rowObject) + ")'>" +
                iconPath + "</button>" + strExt;

            //return "<button type='button' class='btn btn-primary btn-modal' " + btnDisabled + " onclick='DownloadFileOpDetail(" + JSON.stringify(rowObject) + ")'>" +
            //    "<i class='glyphicon glyphicon-download'></i>" + "</button>";
        } else {
            return "";
        }


    }

    function playVideo(cellvalue, options, rowObject) {
        if (isEmpty(rowObject.VideoFile)) return "";
        //return "<button type='button' class='btn btn-info btn-modal' onclick='PlayVideo(" + JSON.stringify(rowObject.VideoOpLink) + ")'>" +

        return "<button type='button' class='btn btn-info btn-modal' onclick='PlayVideo(" + JSON.stringify(rowObject) + ")'>" +
            "<i class='glyphicon glyphicon-play'></i>" + "</button>";
    }

    jQuery(gridOpsDetailId).jqGrid('bindKeys');

    //Hide checkbox all
    var myGrid = $(gridOpsDetailId);
    $("#cb_" + myGrid[0].id).hide();

    //Resize girdview
    //$(window).on('resize', function () {
    //    var wd = $("#gridPagerOpsDetail").width();
    //    $(gridOpsDetailId).setGridWidth(wd);
    //}).trigger('resize');

}

//Get list of file from dms.
function BindDataToJqGridMachineFile(styleCode, styleSize, styleColorSerial, revNo, uploadCode, styleFile, styleFileDesc, opRevNo, opSerial, edition) {

    jQuery(TableFileId).jqGrid({
        url: '/OPS/GetFiles',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            uploadCode: uploadCode,
            styleFile: styleFile,
            styleFileDesc: styleFileDesc,
            opRevNo: opRevNo,
            opSerial: opSerial,
            edition: edition
        },
        datatype: "json",
        height: 250,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: true,
        gridview: true,
        multiselect: true,
        caption: "List Of Files From Dms",
        colModel: [
            { label: arrColNameFiles.LINKING, width: 70, align: 'center', formatter: linkedFiles },
            { name: 'FileNote', index: 'FileNote', label: arrColNameFiles.FILETYPE, width: 200 },
            { name: 'FileName', index: 'FileName', label: arrColNameFiles.FILENAME, width: 350 },
            { name: 'StyleColorWays', index: 'StyleColorWays', label: arrColNameFiles.STYLECOLORWAYS, align: 'left', width: 150 },
            { name: 'RevNo', index: 'RevNo', label: arrColNameFiles.REVNO, align: 'center', width: 100 },
            { name: 'AmendNo', index: 'AmendNo', label: arrColNameFiles.AMENDNO, align: 'center', width: 100 },
            { name: 'Remarks', index: 'Remarks', width: 250, label: arrColNameFiles.REMARKS, align: 'center' },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'RemoteFile', index: 'RemoteFile', hidden: true },
            { name: 'UploadCode', index: 'UploadCode', hidden: true },
            { name: 'Linked', index: 'Linked', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Files", err.message, MessageTypeError);
        },
        onSelectRow: function (rowid) {
            //var row = $(TableFileId).jqGrid("getRowData", rowid);
            //$("#jqg_tblFiles_" + rowid).prop("disabled", true);
            //if (row.UploadedFile === "1") {
            //    $("#jqg_tblFiles_" + rowid).prop("disabled", true);
            //}
            ////alert(row.UploadedFile);
        },
        loadComplete: function () {

        },
        beforeSelectRow: function (rowid, e) {
            var row = $(TableFileId).jqGrid("getRowData", rowid);
            if (row.Linked === "1") {
                return false;
            }
            return true;
        },
        onSelectAll: function (id, status) {
            var gridFile = $(TableFileId);
            for (i = 0; i < id.length; i++) {
                var linked = gridFile.getCell(id[i], "Linked");
                if (linked === "1") {
                    $("#jqg_tblFiles_" + (i + 1)).prop("disabled", true);
                    $("#jqg_tblFiles_" + (i + 1)).prop("checked", false);
                }
            }
        },
        gridComplete: function () {
            var gridFile = $(TableFileId);
            var rows = gridFile.getDataIDs();
            for (var i = 0; i < rows.length; i++) {
                var linked = gridFile.getCell(rows[i], "Linked");
                if (linked === "1") {
                    $("#jqg_tblFiles_" + (i + 1)).prop("disabled", true);
                }
            }
        }
    });

    function linkedFiles(cellvalue, options, rowObject) {
        if (rowObject.Linked === "1") {
            return '<span style="color: forestgreen">Linked</span>';
        }
        return "";
    }

    jQuery(TableFileId).jqGrid('setLabel', arrColNameFiles.LINKING, '', { 'text-align': 'right' });
}

//Get list of file from dms.
function BindDataToJqGridDownloadMachineFiles(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, uploadCode) {

    jQuery(TableFilesDownloadId).jqGrid({
        url: '/OPS/GetOpFiles',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            opRevNo: opRevNo,
            opSerial: opSerial,
            edition: edition,
            uploadCode: uploadCode
        },
        datatype: "json",
        height: 250,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: true,
        gridview: true,
        caption: "List Of Files",
        colModel: [
            { label: "Remove", width: 70, align: 'center', formatter: removeLinking },
            { label: "Download", width: 120, align: 'center', formatter: downloadFile },
            { label: " ", width: 100, align: 'center', formatter: loadIcon },
            { name: 'FileNote', index: 'FileNote', label: arrColNameFiles.FILETYPE, width: 200 },
            { name: 'FileName', index: 'FileName', label: arrColNameFiles.FILENAME, width: 350, formatter: hyperVideoLink },
            { name: 'AmendNo', index: 'AmendNo', label: arrColNameFiles.AMENDNO, align: 'center', width: 100 },
            { name: 'StyleColorWays', index: 'StyleColorWays', label: arrColNameFiles.STYLECOLORWAYS, align: 'left', width: 150 },
            { name: 'RevNo', index: 'RevNo', label: arrColNameFiles.REVNO, align: 'center', width: 100 },
            { name: 'Remarks', index: 'Remarks', width: 250, label: arrColNameFiles.REMARKS, align: 'center' },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'RemoteFile', index: 'RemoteFile', hidden: true },
            { name: 'UploadCode', index: 'UploadCode', hidden: true },
            { name: 'SourceFile', index: 'SourceFile', hidden: true },
            { name: 'RefLink', index: 'RefLink', hidden: true },
            { name: 'Used', index: 'Used', hidden: true } //SON ADD) 2019/Jan/11
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Operation Files", err.message, MessageTypeError);
        },
        onSelectRow: function (rowid) {

        },
        loadComplete: function () {

        },
        gridComplete: function () {

        }
    });

    //START ADD) SON - 2018/Dec/25
    function loadIcon(cellvalue, options, rowObject) {

        //Set icon for file download
        var startImg = "<img ";
        var srcImg = "onerror='imgErrorIcon(this);' src='../img/icons/";
        var endImg = " height='30'/>";
        var icon = "";
        var iconPath = "";

        var fileName = rowObject.FileName;
        var extFile = fileName.split('.').pop().toUpperCase();
        $.each(ObjIcons, function (idx, value) {
            if (extFile === value.CodeDetail2) {
                icon = value.CodeDetail;
            }
        });
        //icon = ObjIcons[extFile] == null ? "file.png" : ObjIcons[extFile];
        srcImg += icon + "'";


        iconPath = startImg + srcImg + endImg;

        return iconPath;

        //return "<button type='button' class='btn btn-primary btn-modal' " + btnDisabled + " onclick='DownloadFileOpDetail(" + JSON.stringify(rowObject) + ")'>" +
        //    "<i class='glyphicon glyphicon-download'></i>" + "</button>";

    }
    //END ADD) SON - 2018/Dec/25

    function hyperVideoLink(cellvalue, options, rowObject) {
        if (rowObject.SourceFile === SourcePkVideo)
            return '<a target="_blank" id="achVideoLink_' + options.rowId + '"  href="' + rowObject.RefLink + '"> ' + cellvalue + '</a>';

        return cellvalue;
    }

    function downloadFile(cellvalue, options, rowObject) {
        if (rowObject.SourceFile === SourcePkVideo)
            return "";

        //SON ADD) 2019/Jan/11
        if (rowObject.Used === "N")
            return "No Used";

        return "<button type='button' class='btn btn-primary btn-modal' onclick='DownloadFile(" + JSON.stringify(rowObject) + ")'>" +
            "<i class='glyphicon glyphicon-download'></i> Download</button>";
    }

    function removeLinking(cellvalue, options, rowObject) {
        //Check operation master confirmed
        var confChk = $("#hdConfChkOpsMaster").val();
        if (confChk === ConfirmCheck) return '<span style="color: forestgreen">Confirmed</span>';

        //Check linked file or process file (stored in FTP server).
        if (rowObject.SourceFile === SourceUpload)
            return "";

        return "<button type='button' class='btn btn-danger btn-modal' onclick='RemoveLinkedFiles(" + JSON.stringify(rowObject) + ")'>" +
            "<i class='glyphicon glyphicon-remove'></i> </button>";
    }
}

function imgErrorIcon(image) {
    image.onerror = "";
    image.src = "../img/icons/file.png";
    return true;
}

function BindataToGridVideos(styleCode, styleSize, styleColorSerial, revNo) {
    jQuery(TableVideosId).jqGrid({
        url: '/OPS/GetVideosLink',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo
        },
        datatype: "json",
        height: 250,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: true,
        gridview: true,
        caption: "List Of Videos",
        colModel: [
            { label: "Copy", width: 70, align: 'center', search: false, formatter: copyVideoLink },
            { name: 'FileName', index: 'FileName', label: arrColNameFiles.FILENAME, width: 200 },
            { name: 'VideoLink', index: 'VideoLink', label: arrColNameFiles.FILETYPE, width: 550, formatter: hyperVideoLink }
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Videos", err.message, MessageTypeError);
        },
        onSelectRow: function (rowid) {

        },
        loadComplete: function () {

        },
        gridComplete: function () {

        },
        ajaxGridOptions: { async: true } //ADD - SON) 29/Oct/2020 - set async is true
    });

    function copyVideoLink(cellvalue, options, rowObject) {
        return "<button type='button' class='btn btn-primary btn-modal' onclick='CopyVideoLink(" + JSON.stringify(options.rowId) + ")'>" +
            "<i class='glyphicon glyphicon-copy'></i> Copy</button>";
    }

    function hyperVideoLink(cellvalue, options, rowObject) {
        return '<a target="_blank" id="achVideoLink_' + options.rowId + '"  href="' + cellvalue + '"> ' + cellvalue + '</a>';
    }

    SearchFilter(jQuery(TableVideosId));
}

function CopyVideoLink(videoLink) {
    var aux = document.createElement("input");
    aux.setAttribute("value", document.getElementById("achVideoLink_" + videoLink).innerHTML);
    aux.defaultValue = aux.defaultValue.replace(/&amp;/g, '&');
    document.body.appendChild(aux);
    aux.select();
    document.execCommand("copy");
    document.body.removeChild(aux);
}
// #endregion

//#region Tab Module Revision
function BindDataToJqGridModuleRevision(styleCode, styleSize, styleColorSerial, revNo) {
    jQuery(TableModuleRevisionId).jqGrid({
        url: '/OPS/GetModulesMbom',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo
        },
        datatype: "json",
        height: 'auto',
        width: null,
        shrinkToFit: false,
        scroll: false,
        deepempty: true,
        ignoreCase: true,
        viewrecords: true,
        rowNum: 20,
        rowList: [20, 30, 40],
        pager: DivPagerModuleRevisionId,
        gridview: true,
        caption: "Module Revison",
        colModel: [
            { name: 'ModuleItemCode', index: 'ModuleItemCode', width: 150, label: arrColNameGridModule.MODULEID, classes: 'pointer' },
            { name: 'ModuleName', index: 'ModuleName', width: 400, label: arrColNameGridModule.MODULENAME, classes: 'pointer' },
            { name: 'Linked', index: 'Linked', width: 100, label: 'Linked', align: 'center', formatter: createLinkedCheckbox },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Module revision", err.message, MessageTypeError);
        },
        onPaging: function (pgButton) { },
        onSelectRow: function (rowid) { },
        gridComplete: function () {
            setTimeout(function () {
                updatePagerIcons();
            }, 0);
        },
        loadComplete: function (gridData) {
            //if (gridData.length > 0 || typeof gridData.length !== 'undefined') {
            //    $.each(gridData, function (idx, rowData) {
            //        let rowId = idx + 1;
            //        if (rowData.Linked === '1') {
            //            //Set link checked
            //            $(`#chkLinked_${rowId}`).prop('checked', true);
            //        } else {
            //            $(`#chkLinked_${rowId}`).prop('checked', false);
            //        }                    
            //    });
            //}
        }
    });

    function createLinkedCheckbox(cellValue, option) {
        let linked = cellValue === '1' ? 'checked' : '';
        let chkLinked = `<input type="checkbox" id="chkLinked_${option.rowId}" ${linked}>`;
        return chkLinked;
    }
}
//#endregion

// #region Tab Module

function BindDataToJqGridModule(styleCode, moduleItemCode) {
    jQuery(TableModuleId).jqGrid({
        url: '/OPS/GetModules',
        postData: {
            styleCode: styleCode,
            moduleItemCode: moduleItemCode
        },
        datatype: "json",
        height: 'auto',
        width: null,
        shrinkToFit: false,
        scroll: false,
        deepempty: true,
        ignoreCase: true,
        viewrecords: true,
        rowNum: 20,
        rowList: [20, 30, 40],
        pager: DivPagerModuleId,
        gridview: true,
        caption: "Module",
        colModel: [
            { name: 'ModuleId', index: 'ModuleId', width: 150, label: arrColNameGridModule.MODULEID, classes: 'pointer', hidden: true },
            { name: 'ModuleName', index: 'ModuleName', width: 400, label: arrColNameGridModule.MODULENAME, classes: 'pointer', hidden: true },
            { name: 'PartId', index: 'PartId', width: 100, label: "ID", classes: 'pointer' },
            { name: 'ItemName', index: 'ItemName', width: 400, label: arrColNameGridModule.ITEMNAME, classes: 'pointer' },
            //{ name: 'SubGroup', index: 'SubGroup', width: 120, label: arrColNameGridModule.SUBGROUP, align: 'center', formatter: createDropdownSubGroup },
            { name: 'FinalAssembly', index: 'FinalAssembly', width: 200, label: arrColNameGridModule.FINALASSEMBLY, classes: 'pointer' },
            {
                name: 'PartComment', width: 300, label: "Custom name", classes: 'pointer',
                formatter: function (cellValue, option) {
                    cellValue = cellValue == null ? "" : cellValue;
                    if (UserRoleMdl.IsUpdate) {
                        return '<input type="text" name="txtPartComment" id="txtPartComment_' + option.rowId + '" value="' + cellValue + '"/>';
                    }

                    return '<input type="text" name="txtPartComment" id="txtPartComment_' + option.rowId + '" value="' + cellValue + '" readonly/>';
                }
            },
            { name: 'StyleCode', index: 'StyleCode', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Module", err.message, MessageTypeError);
        },
        onPaging: function (pgButton) {
            if (pgButton === "records") {

                SetPaging($(TableModuleId), DivPagerModuleName);
            }
        },
        onSelectRow: function (rowid) {
            //var row = $(TableModuleId).jqGrid("getRowData", rowid);

            //Save ops master key to local storage
            //localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

        },
        gridComplete: function () {
            setTimeout(function () {
                updatePagerIcons();
            }, 0);

        },
        loadComplete: function (gridData) {
            //if (gridData.length > 0 || typeof gridData.length !== 'undefined') {
            //    $.each(gridData, function (idx, rowData) {
            //        let rowId = idx + 1;
            //        //console.log(rowId);
            //        $(`#drpSubGroupModule_${rowId}`).val(rowData.SubGroup);
            //    });
            //}
        }
    });

    //navButtons
    jQuery(TableModuleId).jqGrid('navGrid', DivPagerModuleId, {
        //option
        view: false,
        viewicon: 'ace-icon fa fa-search-plus grey',
        add: false,
        edit: false,
        del: true,
        search: false,
        searchicon: 'ace-icon fa fa-search orange',
        refresh: true,
        refreshicon: 'ace-icon fa fa-refresh green'
    },
        {/*edit options*/ },
        {/*add options*/ },
        {
            //del options
            url: "/Ops/DeleteModule"
            , delData: {
                opKey: function () {
                    var sel_id = $(TableModuleId).jqGrid('getGridParam', 'selrow');
                    var dtRow = jQuery(TableModuleId).getRowData(sel_id);
                    //Redefine Part Commnet because it return html
                    dtRow.PartComment = "";
                    dtRow.SubGroup = "";
                    var rowData = JSON.stringify(dtRow);

                    return rowData;
                }
            }
            , afterComplete: function (response) {
                var res = JSON.parse(response.responseText);
                if (res === Success) {
                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info, "");
                } else {
                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Error, res);
                }
            }
        },
        { /*search options*/ }
    );

    $("#pg_" + DivPagerModuleName + " option[value=40]").text(arrButtonAction.all);

    //function createDropdownSubGroup(cellValue, option) {
    //    let selSubGroup = `<select id="drpSubGroupModule_${option.rowId}"><option value="0">Unlinked</option><option value="1">Linked</option></select>`;

    //    return selSubGroup;
    //}
}

// #endregion

// #region Modal register new ops

//Bind style data to Style gridview on Ops modal
function BindDataToJqGridStyleModal(buyer, start, end, searchText, aoNumber) {
    jQuery(IdTableStyleModal).jqGrid({
        url: "/UIControl/SearchList",
        postData: {
            buyer: buyer, start: start, end: end, search: searchText, aoNumber
        },
        sortname: "REGISTRYDATE",
        sortorder: "DESC",
        datatype: "json",
        height: 150,
        width: null,
        shrinkToFit: false,
        scroll: false,
        ignoreCase: true,
        viewrecords: true,
        rowNum: 10,
        rowList: [10, 20, 30, 40],
        pager: IdPaperStyleModal,
        gridview: true,
        colModel: [
            { name: 'StyleCode', index: 'StyleCode', width: 90, label: arrColNameStyle.STYLECODE, editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'StyleSize', index: 'StyleSize', width: 70, label: arrColNameStyle.STYLESIZE, search: true, editable: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', width: 180, label: arrColNameStyle.STYLECOLORSERIAL, editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }, formatter: colorWaysFormat },
            { name: 'RevNo', index: 'RevNo', width: 50, label: arrColNameStyle.REVNO, editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'StyleName', index: 'StyleName', width: 90, label: arrColNameStyle.STYLENAME, editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'BuyerStyleCode', index: 'BuyerStyleCode', width: 70, label: arrColNameStyle.BUYERSTYLECODE, editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'BuyerStyleName', index: 'BuyerStyleName', width: 100, label: arrColNameStyle.BUYERSTYLENAME, editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'StaTus', index: 'StaTus', width: 50, label: arrColNameStyle.STATUS, align: 'center', editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'RegistryDate', index: 'RegistryDate', label: arrColNameStyle.REGISTRYDATE, align: 'center', editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'Register', index: 'Register', label: arrColNameStyle.REGISTER_NAME, align: 'center', editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'AdConfirm', index: 'AdConfirm', label: arrColNameStyle.AD_CONFIRM, align: 'center', editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } },
            { name: 'AdDevSale', index: 'AdDevSale', label: arrColNameStyle.AD_DEV_SALES, align: 'center', editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] } }
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Ops Master", err.message, MessageTypeError);
        },
        loadComplete: function () {
            //var table = this;
            setTimeout(function () {
                updatePagerIcons();
            }, 0);

        },
        onSortCol: function (index, idxcol) {
            if (this.p.lastsort >= 0 && this.p.lastsort !== idxcol
                && this.p.colModel[this.p.lastsort].sortable !== false) {
                $(this.grid.headers[this.p.lastsort].el).find(">div.ui-jqgrid-sortable>span.s-ico").hide();
                $(this.grid.headers[this.p.lastsort].el).removeClass('ui-state-highlight');
            }
            $(this.grid.headers[idxcol].el).addClass('ui-state-highlight');
        },
        onSelectRow: function (rowid) {

            //Get value on row of grid
            var row = $(IdTableStyleModal).jqGrid("getRowData", rowid);
            var styleCode = row.StyleCode;
            var styleSize = row.StyleSize;
            var styleColor = !isEmpty(row.StyleColorSerial) ? row.StyleColorSerial.slice(0, 3).toUpperCase() : ""; //row.StyleColorSerial.substr(0,3);
            var styleRevNo = row.RevNo;

            //Create object
            var data = {
                styleCode: styleCode,
                styleSize: styleSize,
                styleColor: styleColor,
                revNo: styleRevNo
            }

            //Reload grid ops detail on modal
            ReloadJqGrid2LoCal(NameTableOpsModal, data);
            //ReloadJqGrid(NameTableOpsModal, data);

        },
        onPaging: function (pgButton) {
            if (pgButton === "records") {
                SetPaging($(IdTableStyleModal), NamePaperStyleModal);
            }
        }
    });

    //navButtons
    jQuery(IdTableStyleModal).jqGrid('navGrid', IdPaperStyleModal, {
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

    jQuery(IdTableStyleModal).jqGrid('bindKeys');

    $("option[value=40]").text(arrButtonAction.all);

    function colorWaysFormat(cellvalue, options, rowObject) {
        //return cellvalue + ' - ' + rowObject.StyleColorWays;
        return rowObject.StyleColorWays; //ADD - SON) 25/Sep/2020
    }
}

//Bind ops detail to Ops gridview on Ops modal
function BindDataToJqGridOpsModal(styleCode, styleSize, styleColor, revNo) {

    jQuery(IdTableOpsModal).jqGrid({
        //url: '/OPS/GetOpMaster',
        url: '/OPS/GetOpMasterByStyle', //ADD - SON) 25/Sep/2020
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColor: styleColor,
            revNo: revNo
        },
        datatype: "json",
        sortname: 'Edition',
        sortorder: 'DESC',
        loadonce: true,
        height: 150,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: 10,
        rowList: [10, 20, 30, 40],
        pager: IdPaperOpsModal,
        gridview: true,
        colModel: [
            { name: 'Edition2', index: 'Edition2', width: 60, label: arrOpsColname.EDITION },
            { name: 'StyleCode', index: 'StyleCode', width: 100, label: arrOpsColname.STYLECODE },
            { name: 'StyleSize', index: 'StyleSize', width: 80, label: arrOpsColname.STYLESIZE },
            { name: 'StyleColorWays', index: 'StyleColorWays', width: 140, label: arrOpsColname.STYLECOLORSERIAL },
            { name: 'RevNo', index: 'RevNo', width: 80, label: arrOpsColname.REVNO, align: 'center' },
            { name: 'OpRevNo', index: 'OpRevNo', width: 90, label: arrOpsColname.OPREVNO, align: 'center' },
            { name: 'OpTime', index: 'OpTime', width: 80, label: arrOpsColname.OPTIME, align: 'center' },
            { name: 'ManCount', index: 'ManCount', width: 80, label: arrOpsColname.MANCOUNT, align: 'center' },
            { name: 'MachineCount', index: 'MachineCount', width: 110, label: arrOpsColname.MACHINECOUNT, align: 'center' },
            { name: 'OpCount', index: 'OpCount', width: 95, label: arrOpsColname.OPCOUNT, align: 'center' },
            { name: 'ConfirmChk', index: 'ConfirmChk', width: 100, label: arrOpsColname.CONFIRMCHK, align: 'center' },
            { name: 'OpPrice', index: 'OpPrice', width: 90, label: arrOpsColname.OPPRICE, align: 'center' },
            { name: 'LastUpdateTime', index: 'LastUpdateTime', width: 250, label: arrOpsColname.LASTUPDATEDATE, align: 'center', formatter: convertDateToString },
            { name: 'Remarks', index: 'Remarks', width: 250, label: arrOpsColname.REMARKS, align: 'center' },
            { name: 'Factory', index: 'Factory', width: 150, label: "Factory", align: 'center' }, //ADD) SON - 28/Jun/2019
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'Language', index: 'Language', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true }

        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Ops Master", err.message, MessageTypeError);
        },
        onSortCol: function (index, idxcol) {
            if (this.p.lastsort >= 0 && this.p.lastsort !== idxcol
                && this.p.colModel[this.p.lastsort].sortable !== false) {
                $(this.grid.headers[this.p.lastsort].el).find(">div.ui-jqgrid-sortable>span.s-ico").hide();
                $(this.grid.headers[this.p.lastsort].el).removeClass('ui-state-highlight');
            }
            $(this.grid.headers[idxcol].el).addClass('ui-state-highlight');
        },
        onSelectRow: function (rowid) {
            var row = $(IdTableOpsModal).jqGrid("getRowData", rowid);

            var processCount = row.OpCount;
            var machineCount = row.MachineCount;
            var workerCount = row.ManCount;
            var time = row.OpTime;

            //Choose "Copy the select plan" and
            //check copy patter and Bom, copy tool linking
            $("#rdCopySelectPlan").prop("checked", true);
            CheckCopyPatterntBomToolLinking(true);
            HideButtonTranslateLanguage(false);

            if (row.StyleCode === $("#txtStyleCode").val() && row.StyleSize === $("#txtStyleSize").val()
                && row.StyleColorSerial === $("#txtColor").val() && row.RevNo === $("#txtRevNo").val()) {
                $("#chkCopyBOMPattern").prop("checked", true);
                $("#chkCopyBOMPattern").prop("disabled", false);
            } else {
                $("#chkCopyBOMPattern").prop("checked", false);
                $("#chkCopyBOMPattern").prop("disabled", true);
            }

            SetValueOpDetailModal(processCount, machineCount, workerCount, time);

            //Select language
            SetValueForLanguage("drpLanguageOpMaster", MapLanguageToFlag(row.Language));

        },
        ajaxRowOptions: { async: true }//ADD - SON) 1/Oct/2020
    });

    //navButtons
    jQuery(IdTableOpsModal).jqGrid("navGrid", IdPaperOpsModal, {
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

    jQuery(IdTableOpsModal).jqGrid('bindKeys');

    $("option[value=40]").text(arrButtonAction.all);

    function convertDateToString(cellValue, options, rowObject) {
        if (!$.isEmptyObject(rowObject.LastUpdateTime)) {
            var newDate = eval(("new " + rowObject.LastUpdateTime).replace(/\//g, ""));
            return newDate;
        }
        return "";
    }

}

function BinDataToJqGridUpdateProcessTime(data) {

    jQuery(TableUpdateProcessTimeId).jqGrid({
        datatype: "local",
        height: 450,
        rownumbers: true,
        rowNum: 10000,
        colModel: [
            { name: 'OpGroup', index: 'OpGroup', width: 50, label: arrOpsImport.OPGROUP, align: 'left' },
            { name: 'OpNameRef', index: 'OpNameRef', width: 250, label: arrOpsImport.OPNAMEREF, align: 'left', hidden: false },
            { name: 'OpTime', index: 'OpTime', width: 40, label: arrOpsImport.OPTIME, align: 'center' },
            { name: 'MachineType', index: 'MachineType', width: 70, label: arrOpsImport.MACHINETYPE, align: 'center' },
            { name: 'OpNameId', index: 'OpNameId', hidden: true }
        ],
        afterInsertRow: function (rowid, rowData) {

        },
        loadError: function (xhr, status, err) {
            ShowMessage("Get Ops Master", err.message, MessageTypeError);
        }
    });

    $(window).bind("resize", function () {
        var wdContainer = $(".modal-content-ops").width();

        var w = (window.innerWidth * 83.5) / 100;
        if (wdContainer <= 0) {
            wdContainer = w;
        }
        $(TableUpdateProcessTimeId).setGridWidth(wdContainer);
    }).trigger('resize');

    for (var i = 0; i <= data.length; i++)
        jQuery(TableUpdateProcessTimeId).jqGrid('addRowData', i + 1, data[i]);
}

var ArrayProcessNameSelect2 = [];
function GetOpNameListForSelect2() {

    var languageId = $("#drpLanguageOpMaster").val();

    var config = ObjectConfigAjaxPost("/Ops/GetOpName", false, JSON.stringify({ languageId: languageId }));
    AjaxPostCommon(config, function (respone) {
        lstProcessName = respone;

        //var count = 0;
        $.each(lstProcessName, function (index, objProName) {
            var obj = { id: objProName.OpNameId, text: objProName.OpName }
            ArrayProcessNameSelect2.push(obj);
        });
    });

}

//Get data from excel and bind to table
function BindDataToJqGridOpsImportModal(mydata) {
    //var lastsel;
    jQuery(GridOpsTableImportId).jqGrid({
        datatype: "local",
        height: 450,
        rownumbers: true,
        rowNum: 10000,
        //width: null,
        //shrinkToFit: false,
        //gridview: false,
        colModel: [
            //{ name: 'OpGroup', index: 'OpGroup', width: 50, label: arrOpsImport.OPGROUP, align: 'left' },
            { name: 'ModuleName', index: 'ModuleName', width: 50, label: "Module", align: 'left' }, //ADD - SON)18/Jul/2020
            {
                name: 'OpName', index: 'OpName', width: 160, label: arrOpsImport.OPNAME
                , editable: true, edittype: 'select', editoptions: {
                    value: [], //loadOpNameList(),                    
                    dataInit: function (elem) {
                        //$(elem).select2({ placeholder: "Choose process name", data: ArrayProcessNameSelect2, allowClear: true, width: '100%', height: '34px' });
                        $(elem).select2({
                            ajax: {
                                url: '/Ops/SearchProcessName',
                                delay: 500,
                                data: function (params) {
                                    return {
                                        q: params.term, //search term
                                        languageId: $("#drpLanguageOpMaster").val()
                                    };
                                },
                                processResults: function (data) {
                                    //data = ArrayProcessNameSelect2;
                                    var dataMod = [];
                                    if (!$.isEmptyObject(data.items)) {
                                        var langId = $("#drpLanguageOpMaster").val();

                                        $.each(data.items, function (index, item) {
                                            var language;
                                            switch (langId) {
                                                case 'vn':
                                                    language = item.Vietnam
                                                    break;
                                                case 'id':
                                                    language = item.Indonesia
                                                    break;
                                                case 'mm':
                                                    language = item.Myanmar
                                                    break;
                                                case 'et':
                                                    language = item.Ethiopia
                                                    break;
                                                default:
                                                    language = item.English
                                                    break;
                                            }
                                            var newItem = { id: item.OpNameId, text: language };
                                            dataMod.push(newItem);
                                        });
                                    }

                                    return {
                                        //results: data
                                        results: dataMod
                                    };
                                },
                                allowClear: true,
                                minimumInputLength: 2,
                                //dropdownAutoWidth: true
                                //width: 'resolve'
                            }
                        });

                    },
                    multiple: "multiple"
                }
                , editrules: { edithidden: true, required: true }
            },
            //{ name: 'OpName', index: 'OpName', width: 250, label: arrOpsImport.OPNAME, align: 'left' },
            { name: 'OpNameRef', index: 'OpNameRef', width: 250, label: arrOpsImport.OPNAMEREF, align: 'left', hidden: false },
            { name: 'OpTime', index: 'OpTime', width: 40, label: arrOpsImport.OPTIME, align: 'center' },
            { name: 'MachineName', index: 'MachineName', width: 70, label: arrOpsImport.MACHINETYPE, align: 'center' },
            { name: 'MachineType', index: 'MachineType', width: 70, label: arrOpsImport.MACHINETYPE, align: 'center', hidden: true },
            { name: 'OpNameId', index: 'OpNameId', hidden: true }
            //{ name: 'ArrOpNameId', index: 'ArrOpNameId', hidden: true }
        ],
        afterInsertRow: function (rowid, rowData) {
            jQuery(GridOpsTableImportId).jqGrid('editRow', rowid, true);
        },
        loadError: function (xhr, status, err) {
            ShowMessage("Get Ops Master", err.message, MessageTypeError);
        }
    });

    $(window).bind("resize", function () {
        var wdContainer = $(".modal-content-ops").width();

        var w = (window.innerWidth * 83.5) / 100;
        if (wdContainer <= 0) {
            wdContainer = w;
        }
        $(GridOpsTableImportId).setGridWidth(wdContainer);
    }).trigger('resize');

    for (var i = 0; i <= mydata.length; i++)
        jQuery(GridOpsTableImportId).jqGrid('addRowData', i + 1, mydata[i]);

    function loadOpGroup() {

        var arrOpGroup = GetMasterCode(OpGroup);
        var code;
        var name;
        var objGroup = "'':'',";
        for (var i = 0; i < arrOpGroup.length; i++) {
            code = arrOpGroup[i]["SubCode"];
            name = arrOpGroup[i]["CodeName"];
            if (i > 0) {
                objGroup += ",";
            }
            objGroup += "'" + code + "':'" + name + "'";
        }
        var obj = eval('({' + objGroup + '})');
        return obj;
    }

    function loadOpNameList() {
        var obj;
        var arrOpNameList = [];
        var languageId = $("#drpLanguageOpMaster").val();

        var config = ObjectConfigAjaxPost("/Ops/GetOpName", false, JSON.stringify({ languageId: languageId }));
        AjaxPostCommon(config, function (respone) {
            arrOpNameList = respone;
            var code;
            var name;
            var objOpName = "'':'',";
            for (var i = 0; i < arrOpNameList.length; i++) {
                code = arrOpNameList[i]["OpNameId"];
                name = arrOpNameList[i]["OpName"];
                if (i > 0) {
                    objOpName += ",";
                }
                objOpName += "'" + code + "':'" + name + "'";
            }
            obj = eval('({' + objOpName + '})');

        });

        return obj;
    }
}

//Bind data to Ops detail gridview
function BindDataToJqGridProcessName(styleCode, styleSize, styleColor, revNo, opRevNo, edition, lanId) {
    jQuery(TableProcessNameId).jqGrid({
        url: '/OPS/GetOpDetail',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColor: styleColor,
            revNo: revNo,
            opRevNo: opRevNo,
            edition: edition,
            languageId: lanId
        },
        datatype: "json",
        height: 250,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: true,
        gridview: true,
        caption: "OPS Detail",
        colModel: [
            { name: 'OpName', index: 'OpName', width: 550, label: arrColNameOpsDetail.OPNAME, classes: 'pointer' },
            { name: 'OpNameLan', index: 'OpNameLan', width: 550, label: arrColNameOpsDetail.OPNAMETRANSLATE, classes: 'pointer' },
            { name: 'StyleCode', index: 'StyleCode', label: arrColNameOpsDetail.STYLECODE, hidden: true },
            { name: 'StyleSize', index: 'StyleSize', label: arrColNameOpsDetail.STYLESIZE, hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSeiral', label: arrColNameOpsDetail.STYLECOLORSERIAL, hidden: true },
            { name: 'RevNo', index: 'RevNo', label: arrColNameOpsDetail.REVNO, hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', label: arrColNameOpsDetail.OPREVNO, hidden: true },
            { name: 'OpSerial', index: 'OpSerial', label: arrColNameOpsDetail.OPSERIAL, hidden: true }
            //{ name: 'RefOpName', index: 'RefOpName', hidden: true }
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Ops Detail", err.message, MessageTypeError);
        },
        onSelectRow: function (rowid) {
            var row = $(TableProcessNameId).jqGrid("getRowData", rowid);

            var styleCodeRow = row.StyleCode;
            var styleSizeRow = row.StyleSize;
            var styleColorSerialRow = row.StyleColorSerial;
            var revNoRow = row.RevNo;
            var opRevNoRow = row.OpRevNo;
            var opSerialRow = row.OpSerial;

            jQuery(gridOpsDetailId).jqGrid('setCaption',
                "Style Code: " + styleCodeRow + " | Size: " + styleSizeRow +
                " | Color: " + styleColorSerialRow + " | Revision: " + revNoRow +
                " | Op Revisioin: " + opRevNoRow + " | Op Serial: " + opSerialRow
            );

        }
    });
}

// #endregion

// #endregion

// #region Ops Master

// #region Function register Ops master

function BeforeSelectRowGridOpMaster() {
    return true;
}

//Get master style key code from hiden field
function GetStyleMasterKeyCode() {
    var styleCode;
    var styleSize;
    var styleColorSerial;
    var revNo;
    var objKeyCode = null;

    //Get style master infor
    var objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));
    if (!$.isEmptyObject(objStyleMaster)) {
        styleCode = objStyleMaster.StyleCode;
        styleSize = objStyleMaster.StyleSize;
        styleColorSerial = objStyleMaster.StyleColorSerial;
        revNo = objStyleMaster.RevNo;

        objKeyCode = {
            StyleCode: styleCode,
            StyleSize: styleSize,
            StyleColorSerial: styleColorSerial,
            RevNo: revNo
        }
    }

    return objKeyCode;
}

//Get key code op master from hidden field
function GetKeyCodeOpsMaster() {
    //Get selected datarow
    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    if ($.isEmptyObject(objOpsMaster)) return null;

    var styleCode = objOpsMaster.StyleCode;
    var styleSize = objOpsMaster.StyleSize;
    var styleColorSerial = objOpsMaster.StyleColorSerial;
    var revNo = objOpsMaster.RevNo;
    var opRevNo = objOpsMaster.OpRevNo;
    var edition = objOpsMaster.Edition;
    var confirmedId = $("#hdUsername").val();

    var keyCodeOpMaster = {
        StyleCode: styleCode,
        StyleSize: styleSize,
        styleColorSerial: styleColorSerial,
        RevNo: revNo,
        OpRevNo: opRevNo,
        Edition: edition,
        ConfirmedID: confirmedId
    };

    return keyCodeOpMaster;
}

//Clear data on modal register new ops
function ClearDataOnOpsModal() {

    SetValueOpDetailModal("", "", "", "");

    CheckCopyPatterntBomToolLinking(true);
    DisableCopyPatterntBomToolLinking(false);
    HideButtonTranslateLanguage(false);

    $("#rdCopySelectPlan").prop('checked', true);

    $("#drpTargetEdition").val("").trigger('change');

    //Clear data on gridview style modal
    var data = { buyer: "-1", start: "", end: "", search: "-1", aoNumber: "-1" };
    ReloadJqGrid(NameTableStyleModal, data);

    ListOpsDetail = [];

    //Clear data on gridview ops modal
    //ReloadJqGrid(NameTableOpsModal, []);

    //Clear grid operation plan on modal
    ReloadJqGrid2LoCal(NameTableOpsModal, []);

    ReloadJqGridLocal(GridOpsTableImportName, []);
}

//Confirm Ops master
function ConfirmOpsMaster(objOpsMaster, callBack) {

    $.ajax({
        url: "/Ops/ConfirmOpsMaster",
        async: true,
        type: "POST",
        data: JSON.stringify({ opMaster: objOpsMaster }),
        dataType: "json",
        contentType: "application/json",
        success: function (resConf) {
            callBack(resConf);
        },
        error: function (jqXhr, status, err) {
            strResult = err.message;
        }
    });

}

//Delete Ops master
function DeleteOpsMaster(objOpMaster) {
    var strResult;
    $.ajax({
        url: "/Ops/DeleteOpsMaster",
        async: false,
        type: "POST",
        data: JSON.stringify({ opMaster: objOpMaster }),
        dataType: "json",
        contentType: "application/json",
        success: function (resDel) {
            if (resDel === Success) {
                strResult = Success;
            } else {
                strResult = resDel;
            }
        },
        error: function (jqXhr, status, err) {
            strResult = err.message;
        }
    });
    return strResult;
}

function AlertEditionSelectionForImporting(selectedEdi) {
    let opGridData = GetAllRowsDataJqGrid(IdTableOpsModal);

    let result = false;
    let ediMesage = '';
    let listEdi = [];
    //Get list of Edition from list of operation plan.
    $.each(opGridData, function (idx, opPlan) {
        if (!listEdi.includes(opPlan.Edition)) {
            listEdi.push(opPlan.Edition);
        }
    });

    const ediName = MappingEdition(selectedEdi);

    //Check edition of selected plan with priority edition (MES, AOMTOP, PDM, OPS)
    if (listEdi.includes(editionMes)) {
        if (selectedEdi === editionMes) {
            result = true;
        } else {
            ediMesage = `This style seems to have an MES edition plan available. It is highly recommended to use MES plan over ${ediName} edition . Are you sure to copy from a ${ediName} edition?`;
            result = false;
        }
    } else if (listEdi.includes(editionAom)) {
        if (selectedEdi === editionAom) {
            result = true;
        } else {
            ediMesage = `This style seems to have an AOMTOPS edition plan. It is highly recommended to use AOMTOPS plan over ${ediName} edition plan. are you sure to import a ${ediName} edition?`;
            result = false;
        }
    } else if (listEdi.includes(editionPdm)) {
        if (selectedEdi === editionPdm) {
            result = true;
        } else {
            ediMesage = `This style seems to have an PDM edition plan. It is highly recommended to use PDM plan over ${ediName} edition plan. are you sure to import a ${ediName} edition?`;
            result = false;
        }
    }

    return { Result: result, EdiMessage: ediMesage };
}

//Add new Ops master
function AddNewOps() {
    //Check target edition
    const targetEdition = $("#drpTargetEdition").val();

    if (isEmpty(targetEdition)) {

        ShowMessageOk("002", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

        return;
    }

    //If edition is MES or AOMTOPS then check factory, user must select factory.
    if (targetEdition === 'M' || targetEdition === 'A') {
        const factory = $("#drpFactoryOpmt").val();
        if (isEmpty(factory)) {
            ShowMessageOk("015", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

            return;
        }
    }

    var url = "/Ops/AddNewOps";
    var copyToolLinking = $("#chkCopyToolLinking").is(":checked");
    var copySelectPlan = $("#rdCopySelectPlan").is(":checked");
    var registerEmptyPlan = $("#rdRegisterEmptyPlan").is(":checked");
    var importFile = $("#rdImportCsv").is(":checked");

    let copyBOMPattern = $("#chkCopyBOMPattern").is(":checked");

    var objOps = GetObjectOpsMasterOnModal();
    var objOpsKeysCopy = GetObjOpsKeyCodeCopyModal();
    var lstOpsDetail;

    //START ADD - SON) 26/Sep/2020 - Show alert when importing operation plan by edition
    if (!$.isEmptyObject(objOpsKeysCopy) && copySelectPlan === true) {
        let objMes = AlertEditionSelectionForImporting(objOpsKeysCopy.Edition);
        if (!objMes.Result) {
            let cf = confirm(objMes.EdiMessage);
            if (!cf) return;
        };
    }
    //END ADD - SON) 26/Sep/2020

    //Check Operation plan master keys code
    if (!CheckOperationPlanMasterKeyIsValid(objOps)) return;

    if (importFile) {
        lstOpsDetail = AcceptData;

        //list operation plan detail from csv file
        if (ArrayListIsNull(lstOpsDetail)) {
            ShowMessageOk("002", SmsFunction.Add, MessageType.Error, MessageContext.InvalidData);
            return;
        } else {
            $.each(lstOpsDetail, function (index, value) {
                //Change style color serial
                lstOpsDetail[index].StyleColorSerial = objOps.StyleColorSerial;
            });
        }
    } else if (copySelectPlan) {
        var selOpMaster = GetSelectedOneRowData(IdTableOpsModal);
        //var selRowIds = $().jqGrid("getGridParam", "selarrrow"); //SON MOD - Comment on 22 Feb 2019
        if ($.isEmptyObject(selOpMaster.StyleCode) || $.isEmptyObject(selOpMaster.StyleSize)
            || $.isEmptyObject(selOpMaster.StyleColorSerial) || $.isEmptyObject(selOpMaster.RevNo) || $.isEmptyObject(selOpMaster.OpRevNo)) {
            ShowMessageOk("003", SmsFunction.Add, MessageType.Error, MessageContext.InvalidData);
            return;
        }

        //Check operation plans master
        if (!CheckOpsInfo(objOps)) return; //SON MOD - 22 Feb 2019: CheckOpsInfo(objOps);        

    }

    var param = JSON.stringify({
        editionReg: targetEdition,
        opMaster: objOps,
        opMasterCopy: objOpsKeysCopy,
        lstOpDetail: lstOpsDetail,
        copyToolLinking: copyToolLinking,
        copySelectPlan: copySelectPlan,
        registerEmptyPlan: registerEmptyPlan,
        importFile: importFile,
        copyBOMPattern: copyBOMPattern
    });

    AjaxPost(url, param, ResultAddNewOps);
}

//Resutl after add new Ops master
function ResultAddNewOps(result) {
    result = JSON.parse(result);
    if (result === Success) {
        //Clear data on ops modal
        ClearDataOnOpsModal();

        //Clear style info on ops modal
        SetValueForStyleModal("", "", "", "", "");
        ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add);

        //Hide Ops modal
        HideModal(OpsModal);

        //Reload girdview Ops           
        var data = GetStyleMasterKeyCode();
        data.Edition = $("#drpOpsMasterEdition").val();
        //ReloadJqGrid(gridOpsTableName, data);
        ReloadJqGrid2LoCal(gridOpsTableName, data);
    } else {
        ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, MessageType.Warning, result);
    }

}

//Check target edition when show modal register new ops.
function CheckTargetEdition() {
    var editionsToAdd = [];

    if (UserRoleFom && UserRoleFom.OwnerId) {
        editionsToAdd.push({ EditionCode: "A", EditionName: "AOMTOPS" });

        //START ADD - SON) 19/Jun/2020
        $('#drpFactoryOpmt').prop('disabled', false);
        //END ADD - SON) 19/Jun/2020
    }
    if (UserRoleOpm && UserRoleOpm.OwnerId) {
        editionsToAdd.push({ EditionCode: "P", EditionName: "OPS" });

        //START ADD - SON) 19/Jun/2020
        $('#drpFactoryOpmt').prop('disabled', true);
        //END ADD - SON) 19/Jun/2020
    }
    if (UserRoleMes && UserRoleMes.OwnerId) {
        editionsToAdd.push({ EditionCode: "M", EditionName: "MES" });

        //START ADD - SON) 19/Jun/2020
        $('#drpFactoryOpmt').prop('disabled', false);
        //END ADD - SON) 19/Jun/2020
    }

    BindDataToDdl("drpTargetEdition", editionsToAdd, "EditionCode", "EditionName");
}

//Update is used operation plan
function UpdateIsUsedOp(objOpMaster) {
    var strResult;
    $.ajax({
        url: "/Ops/UpdateIsUsedOp",
        async: false,
        type: "POST",
        data: JSON.stringify({ opMaster: objOpMaster }),
        dataType: "json",
        contentType: "application/json",
        success: function (resDel) {
            if (resDel === Success) {
                strResult = Success;
            } else {
                strResult = resDel;
            }
        },
        error: function (jqXhr, status, err) {
            strResult = err.message;
        }
    });
    return strResult;
}
// #endregion

//#region Target edition changed event
function targetEditionOnchanged() {
    $('#drpTargetEdition').on('change', function () {
        var edition = $("#drpTargetEdition").val();
        var style = GetStyleMasterKeyCode();
        var maxOpRev = GetMaxOpRevision(edition, style.StyleCode, style.StyleSize, style.StyleColorSerial, style.RevNo);
        $("#txtOpRevision").val(maxOpRev);
    });
}
//endregion Target edition changed event

// #region Ops master modal

//Disable option register empty and import GSD file when user login with factory role
function DisableOptionRegEmptyAndImpFile() {
    if ((UserRoleFom && UserRoleFom.OwnerId) || UserRoleMes && UserRoleMes.OwnerId) {
        $("#rdRegisterEmptyPlan").prop("disabled", true);
        $("#rdImportCsv").prop("disabled", true);
    }
}

function EventEnterOnTextboxStyle() {
    $("#txtStyleFilter").keypress(function (e) {
        var key = e.which;
        if (key === 13) {
            //Enter key code
            $("#btnFilter").click();
        }
    });
}

//Check Checkbox
function CheckCopyPatterntBomToolLinking(checkStatus) {
    $('#chkCopyToolLinking').prop('checked', checkStatus);
}

//Disable checkbox
function DisableCopyPatterntBomToolLinking(disableStatus) {
    $("#chkCopyToolLinking").prop("disabled", disableStatus);
}

//Hide or show button translate language.
function HideButtonTranslateLanguage(value) {
    HideButton("btnTranslateProcessName", value);
}

//Handle file when selecing
function handleFileSelect(evt) {

    if (!$("#rdSelectProcessName").prop("checked") && !$("#rdAddNewProcess").prop("checked")) {
        ShowMessageOk("004", SmsFunction.Add, MessageType.Error, MessageContext.InvalidData);
        $("#flImportCsv").val("");
        return;
    }

    var files = evt.target.files;
    var file = files[0];
    $("#dvImportFileError").empty();
    LoadDataFromCsvFile(file);
}

//Load data from csv file to gridview.
function LoadDataFromCsvFile(file) {
    //block UI
    $.blockUI(ObjectBlockUICssWaiting);

    var reader = new FileReader();
    reader.readAsText(file);

    reader.onload = function (event) {
        var csv = event.target.result;
        csv = csv.replace(/["']/g, '');
        var data = $.csv.toArrays(csv);

        var objNewOp = GetObjectOpsMasterOnModal();
        var objResult = CreateObjectProcessFromCsvData(objNewOp.StyleCode, objNewOp.StyleSize, objNewOp.StyleColorSerial, objNewOp.RevNo, objNewOp.OpRevNo, objNewOp.Edition, data);

        var err = objResult.Error;
        if (!isEmpty(err)) {
            if (!isEmpty(objResult.Error)) {
                err = "Operation time is not correct at row: " + err;
                ListOpsDetail = [];
                ShowErrorLoadingCsv(true, err);
            } else {
                ListOpsDetail = lstProccess;
                ShowErrorLoadingCsv(false, "");
            }
        }

        //Reload data gridview import data.
        ReloadJqGridLocal(GridOpsTableImportName, objResult.ListProcess);
        objResult = null;
        $.unblockUI();
    };
}

function CreateObjectProcessFromCsvData(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, csvData) {

    var opGroup = "";
    var opSerial = 0;
    var opNameId;
    var opNameRef;
    var opTime;
    var machineType;
    var err = "";
    var objProcess;
    var lstProccess = [];
    let moduleName = '';
    for (var i = 1; i < csvData.length; i++) {

        //Check the end of file
        if (!isEmpty(csvData[i][GroupStartCol])) {
            if (csvData[i][GroupStartCol] === GroupStart && csvData[i + 1][GroupStartCol] === GroupStart
                && csvData[i + 2][GroupStartCol] === GroupStart) {
                break;
            }
        }

        //Check start process group
        if (csvData[i][GroupStartCol] === GroupStart) {
            //START MOD - SON) 18/Jul/2020
            ////Get operation plan group name
            //opGroup = csvData[i][GroupNameCol];
            //Get module name
            moduleName = csvData[i][GroupNameCol];
            //END MOD - SON) 18/Jul/2020
            continue;
        }

        //Check start or end of block of group
        if (csvData[i][GroupStartCol] === BlockStart || csvData[i][GroupStartCol] === BlockEnd) {
            continue;
        }

        opSerial += 1;
        opNameRef = csvData[i][ProcessNameCol];

        //START ADD - SON) 18/Jul/2020
        //Check the end of operationname
        let posCol = 0;
        while (!opNameRef.includes("##") && posCol < 51) {
            posCol++;
            opNameRef += ',' + csvData[i][ProcessNameCol + posCol];
        }

        //If cannot find then end of process name then get data from the first column
        if (posCol > 50) {
            posCol = 0;
            opNameRef = csvData[i][ProcessNameCol];
        }

        //Remove ## from operation name
        opNameRef = opNameRef.replace("##", '')
        //END ADD - SON) 18/Jul/2020

        machineName = csvData[i][MachineTypeCol];

        opTime = ChangeMinute2Second(csvData[i][ProcessTime3Col]);
        if (isNaN(opTime)) {
            err += opSerial + ", ";
        }

        //Create object process.
        objProcess = CreateObjectOpsDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, "", opGroup, "", opTime, edition, opNameRef, machineName, moduleName);
        lstProccess.push(objProcess);
    }

    var objResult = { Error: err, ListProcess: lstProccess };

    return objResult;
}

function ShowErrorLoadingCsv(show, err) {
    if (show === true) {
        $("#dvImportFileError").append(err);
        $("#dvImportFileError").show();
    } else {
        $("#dvImportFileError").append(err);
        $("#dvImportFileError").hide();
    }

    $("#btnAccept").prop('disabled', show);
}

function SetValueOpDetailModal(processCount, machineCount, workerCount, time) {
    $("#txtProcess").val(processCount);
    $("#txtMachine").val(machineCount);
    $("#txtWorkerModal").val(workerCount);
    $("#txtTime").val(time);
}

//Set value style on ops modal
function SetValueForStyleModal(styleCode, styleSize, styleColor, styleRevNo, opRevNo) {
    $("#txtStyleCode").val(styleCode);
    $("#txtStyleSize").val(styleSize);
    $("#txtColor").val(styleColor);
    $("#txtRevNo").val(styleRevNo);
    $("#txtOpRevision").val(opRevNo);
}

//Get operation plan master key on modal add new ops
function GetObjectOpsMasterKeyOnModal() {
    var styleCode = $("#txtStyleCode").val();
    var styleSize = $("#txtStyleSize").val();
    var styleColorSerial = $("#txtColor").val();
    var revNo = $("#txtRevNo").val();
    var opRevNo = $("#txtOpRevision").val();

    var objOps = { StyleCode: styleCode, StyleSize: styleSize, StyleColorSerial: styleColorSerial, RevNo: revNo, OpRevNo: opRevNo };
    return objOps;
}

//Get Object Ops information on modal
function GetObjectOpsMasterOnModal() {
    var styleCode = $("#txtStyleCode").val();
    var styleSize = $("#txtStyleSize").val();
    var styleColorSerial = $("#txtColor").val();
    var revNo = $("#txtRevNo").val();
    var opRevNo = $("#txtOpRevision").val();
    var opTime = $("#txtTime").val();
    var machineCount = $("#txtMachine").val();
    var opCount = $("#txtProcess").val();
    var manCount = $("#txtWorkerModal").val();
    var edition = $("#drpTargetEdition").val();
    var language = MapFlagValueToLanguage($("#drpLanguageOpMaster").val());
    var factory = $("#drpFactoryOpmt").val(); //ADD) SON - 1/Jul/2019

    var objOps = {
        StyleCode: styleCode,
        StyleSize: styleSize,
        StyleColorSerial: styleColorSerial,
        RevNo: revNo,
        OpRevNo: opRevNo,
        OpTime: opTime,
        MachineCount: machineCount,
        OpCount: opCount,
        ManCount: manCount,
        Edition: edition,
        Language: language,
        Factory: factory //ADD) SON - 1/Jul/2019
    };
    return objOps;
}

//Get operation plan key code for copy pattern, bom and tool linking
function GetObjOpsKeyCodeCopyModal() {
    var selOpmt = GetSelectedOneRowData(IdTableOpsModal);
    var objOpsKeys = null;
    if (!$.isEmptyObject(selOpmt)) {
        objOpsKeys = {
            StyleCode: selOpmt.StyleCode,
            StyleSize: selOpmt.StyleSize,
            StyleColorSerial: selOpmt.StyleColorSerial,
            RevNo: selOpmt.RevNo,
            OpRevNo: selOpmt.OpRevNo,
            Edition: selOpmt.Edition
        };
    }

    return objOpsKeys;
}


// #endregion

// #endregion

// #region Style Detail

function ClickButtonRemovePreviewStyleImage() {
    $("#btnRemoveImgDetail").click(function () {
        RemovePreviewStyleImage();
    });
}

// #endregion

// #region Ops detail

// #region Function Ops Detail

function CreateGridOpsDetail(styleCode, styleSize, styleColor, revNo) {
    //Set style key to hiden field
    $("#hdStyleCode").val(styleCode);
    $("#hdStyleSize").val(styleSize);
    $("#hdStyleColor").val(styleColor);
    $("#hdStyleRevNo").val(revNo);

    //Get style master information
    //GetStyleMaster(styleCode);//MOD - SON) 29/Oct/2020 - Don't get style information when initing page

    //Clear ops detail.
    ReloadJqGrid(gridOpsDetailName, []);

    //Load style group, sub group, sub sub group for adding module
    //LoadDataForAddingModule(styleCode); //MOD - SON) 29/Oct/2020 - Don't load module

    //START ADD) HA - 12/Oct/2018
    //Reload grid Video Link
    var data = { stylecode: styleCode };
    ReloadJqGrid2LoCal("tbVideoLink", data);

    //END ADD) HA - 12/Oct/2018
}

//Delete ops detail
function DeleteOpsDetail(lstOpDetail) {
    var strResult;
    $.ajax({
        url: "/Ops/DeleteOpsDetail",
        async: false,
        type: "POST",
        data: JSON.stringify({ lstOpDetail: lstOpDetail }),
        dataType: "json",
        contentType: "application/json",
        success: function (resDel) {
            if (resDel === Success) {
                strResult = Success;
            } else {
                strResult = resDel;
            }
        },
        error: function (jqXhr, status, err) {
            strResult = err.message;
        }
    });
    return strResult;
}

//Check type of machine file.
function CheckTypeOfMachineFile(files) {
    var res = true;
    for (var i = 0; i < files.length; i++) {
        var fileExt = GetExtensionFileName(files[i].name);

        res = IsInArray(fileExt, ArrMachineFileType) ? true : false;
        break;
    }

    return res;
}

//Check extension of file with an extension
function CheckValidFileExtension(strExt, files) {
    var res = true;
    for (var i = 0; i < files.length; i++) {
        var fileExt = GetExtensionFileName(files[i].name);

        res = fileExt === strExt.replace(".", "").toLowerCase() ? true : false;
        if (!res) { break; }
    }

    return res;
}

//Check Jig type file.
function CheckIsJigFile(files) {
    var res = true;
    for (var i = 0; i < files.length; i++) {
        var fileExt = GetExtensionFileName(files[i].name);

        res = IsInArray(fileExt, ArrJigFileType) ? true : false;
        break;
    }

    return res;
}

function CheckDataBeforeUploadOpDetailFile(files) {
    if (!HasFile(files)) {
        ShowMessageOk("004", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData);
        return false;
    }

    if ($("#rdUploadMachineFileOpDetail").is(':checked')) {
        if ($.isEmptyObject($("#drpJigFileType").val())) {
            ShowMessageOk("002", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert);

            return false;
        } else {
            if (ExceedAllowedLimitSize(files, LimitSize)) {
                ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData);

                return false;
            }

            let strExt = $("#drpJigFileType").val().split('-')[1];
            if (strExt !== "ALL" && strExt !== "NON") {
                if (!CheckValidFileExtension(strExt, files)) {
                    ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert, strExt);

                    return false;
                }
            }
        }
    }

    if ($("#rdUploadJigFile").is(':checked')) {
        if ($.isEmptyObject($("#drpJigFileType").val())) {
            ShowMessageOk("002", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert);

            return false;
        } else {

            //Check file size in limited
            if (ExceedAllowedLimitSize(files, LimitSize)) {
                ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData);

                return false;
            }

            let strExt = $("#drpJigFileType").val().split('-')[1];

            if (!CheckValidFileExtension(strExt, files)) {
                ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, strExt);

                return false;
            }
        }

    }

    return true;
}

function UpdateOpName(opmt, lstOpdt, languageId) {
    var resUpdate = false;
    $.ajax({
        url: "/Ops/UpdateOpName",
        async: false,
        type: "POST",
        data: JSON.stringify({ opMaster: opmt, lstOpdt: lstOpdt, languageId: languageId }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {

            if (res === Success) {
                resUpdate = true;
                ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, MessageTypeInfo);

            } else {
                ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, MessageTypeError, res);
            }

        },
        error: function (jqXhr, status, errorThrown) {
            resUpdate = false;
            ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, MessageTypeError, errorThrown.message);
        }
    });
    return resUpdate;
}

//Get list ops detail
function GetListOpsDetail(objOpMaster) {
    var lst;
    $.ajax({
        url: "/Ops/GetOpDetail",
        async: false,
        type: "POST",
        data: JSON.stringify(objOpMaster),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            lst = res;
        },
        error: function (jqXhr, status, err) {
            ShowMessage("Get list ops detail", "Cannot get list ops detail!\n" + err.message, MessageTypeError);
        }
    });

    return lst;
}

function CheckNumberSelectedRow(gridId) {
    var myGrid = $(gridId);
    var selRowIds = myGrid.jqGrid('getGridParam', 'selarrrow');
    if (selRowIds.length > 1 || selRowIds.length === 0) {
        ShowMessageOk("001", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

        return false;
    }
    return true;
}

//Mr. Vit add
function ShowImageDetail(id) {
    var url = $("#gridOpsDetail").find("tr[id='" + id + "']").
        find('td[aria-describedby="gridOpsDetail_OrgFileName"]').find("img").attr("src");
    $("#imgPreviewDt").modal("show");
    $("#imgDetailDt").attr("src", url);
}
// #endregion

// #region Ops Detail modal

function FunctionCallBackSavingProcess(addStatus) {
    if (addStatus) {
        HideModal(ProcessModal);

        //Get ops master from loacal storage.
        var objOpsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));

        //Reload data ops detail gridview.
        ReloadJqGrid(gridOpsDetailName, objOpsMaster);

        //Reload ops master gridview.
        objOpsMaster.Edition = $("#drpOpsMasterEdition").val();
        //ReloadJqGrid(gridOpsTableName, objOpsMaster);
        ReloadJqGrid2LoCal(gridOpsTableName, objOpsMaster);
    }
}

function FunctionCallBackUpdateProcess(updateStatus) {
    if (updateStatus) {
        HideModal(ProcessModal);

        //Get ops master from loacal storage.
        var objOpsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));

        //Reload data ops detail gridview.
        ReloadJqGrid(gridOpsDetailName, objOpsMaster);

        //Reload ops master gridview.               
        ReloadJqGrid2LoCal(gridOpsTableName, objOpsMaster);
    }
}

// #region Load data to dropdownlist

function GetMasterCodeFiles(masterCode, subCode, codeDesc) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode2", true, JSON.stringify({ masterCode: masterCode, subCode: subCode, codeDesc: codeDesc }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpFileTypeUploading", respone, "SubCode", "CodeName");
    });
}

// #endregion

// #endregion

// #region Download file

function UploadFileOpDetail(rowObject) {
    SeletedObjOpsDetail = rowObject;

    ShowModal("modalUploadFileOpDetail");
    $("#hdOpSerialOpDetail").val(rowObject.OpSerial);
    $("#hdFileTypeOpDetail").val(VideoType);

    $("#rdUploadVideoOpDetail").prop('checked', true);
    $("#rdUploadMachineFileOpDetail").prop('checked', false);
    $("#rdGetFileFromDms").prop('checked', false);

    $("#divUploadJigFile").hide();
    $("#divRefVideoLink").hide()
    $("#flOpDetail").attr("accept", "video/*");
    $("#dvPreviewOpFile").show();
    $("#flOpDetail").show();
    RemoveOpDetailFilePreview();
    $("#btnUploadFileOpDetail").show();
    $("#btnRemoveFileOpDetail").show();

    $("#divVideoList").hide();
}

function DownloadFileOpDetail(rowObject) {
    SeletedObjOpsDetail = rowObject;

    var postData = {
        styleCode: rowObject.StyleCode,
        styleSize: rowObject.StyleSize,
        styleColorSerial: rowObject.StyleColorSerial,
        revNo: rowObject.RevNo,
        opRevNo: rowObject.OpRevNo,
        opSerial: ZeroPad(rowObject.OpSerial, 3),
        edition: rowObject.Edition,
        uploadCode: ""
    };
    ReloadJqGrid(TableFilesDownloadName, postData);

    //Show modal to download file.
    ShowModal("mdlDownloadFileOpDetail");
}

function DownloadFile(rowObject) {

    var objFile = {
        StyleCode: rowObject.StyleCode,
        StyleSize: rowObject.StyleSize,
        StyleColorSerial: ZeroPad(rowObject.StyleColorSerial, 3),
        RevNo: rowObject.RevNo,
        UploadCode: rowObject.UploadCode,
        AmendNo: rowObject.AmendNo,
        FileName: rowObject.FileName,
        RemoteFile: rowObject.RemoteFile,
        IsOpFile: rowObject.IsOpFile,
        SourceFile: rowObject.SourceFile
    };

    var objOpdt = {
        OpRevNo: SeletedObjOpsDetail.OpRevNo,
        Edition: SeletedObjOpsDetail.Edition,
        OpSerial: ZeroPad(SeletedObjOpsDetail.OpSerial, 3)
    };

    window.location = '/Ops/DownLoadFiles?sdFile=' + JSON.stringify(objFile) + "&objOpdt=" + JSON.stringify(objOpdt);

}

//Remove files linking.
function RemoveLinkedFiles(rowObject) {

    ShowConfirmMessage("Do you want to remove this linked file?", function () {

        var jsonStr = JSON.stringify({
            styleCode: rowObject.StyleCode,
            styleSize: rowObject.StyleSize,
            styleColorSerial: rowObject.StyleColorSerial,
            revNo: rowObject.RevNo,
            uploadCode: rowObject.UploadCode,
            amendNo: rowObject.AmendNo,
            opRevNo: SeletedObjOpsDetail.OpRevNo,
            edition: SeletedObjOpsDetail.Edition,
            opSerial: SeletedObjOpsDetail.OpSerial
        });

        var postData = {
            styleCode: rowObject.StyleCode,
            styleSize: rowObject.StyleSize,
            styleColorSerial: SeletedObjOpsDetail.StyleColorSerial,
            revNo: rowObject.RevNo,
            OpRevNo: SeletedObjOpsDetail.OpRevNo,
            Edition: SeletedObjOpsDetail.Edition,
            OpSerial: ZeroPad(SeletedObjOpsDetail.OpSerial, 3),
            uploadCode: ""
        };

        var config = ObjectConfigAjaxPost("/Ops/RemoveFilesLinking", true, jsonStr);
        AjaxPostCommon(config, function (respone) {
            if (respone === Success) {
                ShowMessageOk("001", SmsFunction.Link, MessageType.Success, MessageContext.Delete, ObjMessageType.Info);

                //Reload jqgrid files linking.
                ReloadJqGrid(TableFilesDownloadName, postData);

                //Reload grid process detail.
                //Get data from local storage.
                var opMaster = GetSelectedOneRowData(gridOpsTableId);
                //Reload data ops detail gridview.
                ReloadJqGrid(gridOpsDetailName, opMaster);
            } else {
                ShowMessageOk("001", SmsFunction.Link, MessageType.Error, MessageContext.Error, ObjMessageType.Error, respone);
            }
        });
    }, function () { }, function () { });
}

function LinkFilesToPdm(lstSdFile, objOpdt) {
    var config = ObjectConfigAjaxPost("/Ops/LinkFilesToPdm", true, JSON.stringify({ lstSdFile: lstSdFile, objOpdt: objOpdt }));
    AjaxPostCommon(config, function (respone) {
        if (respone === Success) {
            ShowMessageOk("001", SmsFunction.Link, MessageType.Success, MessageContext.Add, ObjMessageType.Info);

            //Reload jqgrid files linking.
            ReloadJqGrid(TableFileName, ObjFilesPdmLinking);

            //Reload grid process detail.
            //Get data from local storage.
            var opMaster = GetSelectedOneRowData(gridOpsTableId);
            //Reload data ops detail gridview.
            ReloadJqGrid(gridOpsDetailName, opMaster);
        } else {
            ShowMessageOk("001", SmsFunction.Link, MessageType.Error, MessageContext.Database, ObjMessageType.Error, respone);
        }
    });
}

// #endregion

// #region Upload Jig File

//Get master code with 3 parameters: master code, sub code and code description.
function GetMasterCodeWithMSD(masterCode, subCode, codeDesc, callBackFunc) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode2", true, JSON.stringify({ masterCode: masterCode, subCode: subCode, codeDesc: codeDesc }));
    AjaxPostCommon(config, function (respone) {
        callBackFunc(respone);
    });
}

// #endregion

// #endregion

// #region Balancing

function GetChartType() {
    var chartType = $("#drpChartType").val();

    //Get text of button edit.
    var buttonText = $("#btnSaveBalancingTime").text().trim();
    if (buttonText === "Save") {
        chartType = chartType === "line" ? "dragline" : "dragcolumn2d";
    }

    return chartType;
}

// #endregion

//#region Module Revision
const getLinkedMbomsOnGrid = () => {
    //Get all rows module data
    var linkedMboms = jQuery(TableModuleRevisionId).jqGrid("getRowData");
    $.each(linkedMboms, function (idx, lnkBom) {
        let linked = $('#chkLinked_' + (idx + 1)).is(":checked");
        //Set linked is one
        lnkBom.Linked = linked ? '1' : '0';
        lnkBom.ModuleId = lnkBom.ModuleItemCode;
    });

    return linkedMboms;
}
//#endregion

// #region Module

function CreateObjectMrul() {
    var objMrul = {
        StyleGroup: $("#drpStyleGroup").val(),
        StyleSubGroup: $("#drpStyleSubGroup").val(),
        StyleSubSubGroup: $("#drpStyleSubSubGroup").val(),
        MachineRangeCode: $("#drpMachineRange").val()
    };

    return objMrul;
}

function GetMachineRangeMaster(mCode) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", true, JSON.stringify({ mCode: mCode }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpMachineRange", respone, "SubCode", "CodeName");
    });
}

function GetStyleGroupMaster(mCode) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", false, JSON.stringify({ mCode: mCode }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpStyleGroup", respone, "SubCode", "CodeName");
    });
}

//Get style sub sub group
function GetStyleSubGroupMaster(mCode, subCode, codeDesc) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode2", false, JSON.stringify({ masterCode: mCode, subCode: subCode, codeDesc: codeDesc }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpStyleSubGroup", respone, "SubCode", "CodeName");
    });
}

function GetStyleSubSubGroupMaster(mCode, subCode, codeDesc, codeDetail) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode3", false, JSON.stringify({ masterCode: mCode, subCode: subCode, codeDesc: codeDesc, codeDetail: codeDetail }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpStyleSubSubGroup", respone, "SubCode", "CodeName");
    });
}

function GetModulesLevel(objMrul) {
    var config = ObjectConfigAjaxPost("/Ops/GetModulesLevel", true, JSON.stringify({ mrul: objMrul }));
    AjaxPostCommon(config, function (respone) {
        FillDataToMultipleSelect("drpModuleLevel", respone, "ModuleLevelCode", "LevelDesc");

        var arrModules = [];
        $('#drpModuleLevel option').each(function () {
            var val = $(this).attr('value').split("-");
            if (val[1] === "M") {
                arrModules.push($(this).attr('value'));
            }
        });

        $('#drpModuleLevel').multiselect('select', arrModules, true);

        DisableOptionMultipleSelect("dvModuleLevel", arrModules);
    });
}

function GetModulePart() {
    var config = ObjectConfigAjaxPost("/Ops/GetModulesPart", true, JSON.stringify({}));
    AjaxPostCommon(config, function (respone) {
        FillDataToMultipleSelect("drpModulePart", respone, "LevelCode", "LevelDesc");
    });
}

function DisableOptionMultipleSelect(idDivContainMultiSelect, arrValue) {

    if ($.isEmptyObject(arrValue)) return;

    $.each(arrValue, function (index, value) {
        var val = value.split("-");
        if (val[1] === "M") {
            var input = $('#' + idDivContainMultiSelect + ' input[value="' + value + '"]');
            var option = $('#' + idDivContainMultiSelect + ' option[value="' + value + '"]');

            input.prop('disabled', true);
            option.prop('disabled', true);

            input.parent('label').parent('a').parent('li').addClass('disabled');
        }
    });
}

function AddModule(module, mainLevel, arrModuleLevel, buyer, funcCallBackAddModule) {
    var config = ObjectConfigAjaxPost("/Ops/AddModule", true, JSON.stringify({ objModule: module, mainLevel: mainLevel, lstModuleLevleCode: arrModuleLevel, buyer: buyer }));
    AjaxPostCommon(config, function (respone) {
        if (respone === Success) {
            //Reload gridview.
            var postData = {
                styleCode: module.StyleCode,
                moduleItemCode: module.ModuleItemCode
            };

            //Update style group
            funcCallBackAddModule(respone);

            ReloadJqGrid(TableModuleName, postData);
            ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add, ObjMessageType.Info);

        } else {
            ShowMessageOk("002", SmsFunction.Add, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Warning);
        }

    });
}

function AddModulePart(module, mainLevel, arrModulePart, buyer, funcCallBackAddModule) {
    var config = ObjectConfigAjaxPost("/Ops/AddModulePart", true, JSON.stringify({ objModule: module, mainLevel: mainLevel, lstModulePart: arrModulePart, buyer: buyer }));
    AjaxPostCommon(config, function (respone) {
        if (respone === Success) {
            //Reload gridview.
            var postData = {
                styleCode: module.StyleCode,
                moduleItemCode: module.ModuleItemCode
            };

            //Update style group
            funcCallBackAddModule(respone);

            ReloadJqGrid(TableModuleName, postData);
            ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add, ObjMessageType.Info);

        } else {
            ShowMessageOk("002", SmsFunction.Add, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Warning);
        }

    });
}

//Update style group
function UpdateStyleGroup(styleCode, styleGroup, subGroup, subSubGroup) {
    var config = ObjectConfigAjaxPost("/Ops/UpdateStyleGroup", true, JSON.stringify({ styleCode: styleCode, styleGroup: styleGroup, subGroup: subGroup, subSubGroup: subSubGroup }));
    AjaxPostCommon(config, function (respone) {
        //Process update
    });
}

function DisableModuleControl(isDisable) {
    $("#drpStyleGroup").prop("disabled", isDisable);
    $("#drpStyleSubGroup").prop("disabled", isDisable);
    $("#drpStyleSubSubGroup").prop("disabled", isDisable);
    $("#drpMachineRange").prop("disabled", isDisable);

    //START MOD) SON - 04/Apr/2019
    //if (isDisable) {
    //    $("#drpModuleLevel").multiselect('disable');
    //} else {
    //    $("#drpModuleLevel").multiselect('enable');
    //}

    if (isDisable) {
        $("#drpModuleLevel").multiselect('disable');
        $("#btnAddPart").hide();
        $("#btnSaveComment").hide();
    } else {
        $("#drpModuleLevel").multiselect('enable');
        $("#btnAddPart").show();
        $("#btnSaveComment").show();
    }
    //END MOD) SON - 04/Apr/2019
}

function LoadDataForAddingModule(styleCode) {

    ////Reload module    
    //var postData = {
    //    styleCode: styleCode,
    //    moduleItemCode: ""
    //};
    //ReloadJqGrid(TableModuleName, postData);

    BindDataToJqGridModule(styleCode, "");

    if (UserRoleMdl.IsAdd !== "1") return;

    //Get all style group.
    GetStyleGroupMaster(StyleGroup);

    MultipleSelect("drpModuleLevel");

    GetStyleMasterByStyleCode(styleCode, function (styleMaster) {

        if (!isEmpty(styleMaster.StyleGroup) && styleMaster.StyleGroup !== 'NON') {
            setTimeout(function () {
                $("#drpStyleGroup").val(styleMaster.StyleGroup).trigger('change');
                $("#drpStyleGroup").prop("disabled", true);
            }, 150);
        } else {
            $("#drpStyleGroup").prop("disabled", false);
        }

        if (!isEmpty(styleMaster.SubGroup) && styleMaster.SubGroup !== 'NON') {

            var codeDes = $("#drpStyleGroup").val();
            GetStyleSubGroupMaster(StyleSubGroup, "", codeDes);

            setTimeout(function () {
                $("#drpStyleSubGroup").val(styleMaster.SubGroup).trigger('change');
                $("#drpStyleSubGroup").prop("disabled", true);
            }, 500);
        } else {
            FillDataToDropDownlist("drpStyleSubGroup", [], "SubCode", "CodeName");
            $("#drpStyleSubGroup").prop("disabled", false);
        }

        if (!isEmpty(styleMaster.SubSubGroup) && styleMaster.SubSubGroup !== 'NON') {
            var codeDes2 = $("#drpStyleGroup").val();
            var codeDetail = $("#drpStyleSubGroup").val();
            GetStyleSubSubGroupMaster(StyleSubSubGroup, "", codeDes2, codeDetail);

            setTimeout(function () {
                $("#drpStyleSubSubGroup").val(styleMaster.SubSubGroup).trigger('change');
                $("#drpStyleSubSubGroup").prop("disabled", true);
            }, 950);

        } else {
            FillDataToDropDownlist("drpStyleSubSubGroup", [], "SubCode", "CodeName");
            $("#drpStyleSubSubGroup").prop("disabled", false);
        }
    });

    GetMachineRangeMaster(MachineRange);
}
// #endregion

//START ADD: HA
//Video link
function CheckVideoOpsTable() {
    var objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));

    if ($.isEmptyObject(objStyleMaster)) {
        $("#btnShowVideo").prop('disabled', true);
    } else {
        var styleCode = objStyleMaster.StyleCode;
        jqGridVideoLink(styleCode);
    }
}

function BtnClickShowVideo() {
    $("#btnShowVideo").click(function () {
        ShowModal("mdlShowVideo");
    });

    $("#btnMachine").click(function () {
        ShowModal("mdlShowMachine");
    });

    $("#btnExportMachine").click(function () {
        var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);

        if ($.isEmptyObject(objOpsMaster)) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Warning, MessageContext.NoData, ObjMessageType.Alert);

            return;
        }

        window.open(`/ExportExcel/ExportMachineListToExcel/?styleCode=${objOpsMaster.StyleCode}&styleSize=${objOpsMaster.StyleSize}&styleColorSerial=${objOpsMaster.StyleColorSerial}&revNo=${objOpsMaster.RevNo}&opRevNo=${objOpsMaster.OpRevNo}&edition=${objOpsMaster.Edition}`);
    });
}

function jqGridVideoLink(stylecode) {
    jQuery("#tbVideoLink").jqGrid({
        url: '/Ops/GetVideoByStylecode',
        datatype: "json",
        postData: {
            stylecode: stylecode,
        },
        height: 250,
        colModel: [
            { name: 'FileName', index: 'FileName', label: 'Video Name', width: 170 },
            {
                name: '', label: 'Video Link', width: 700,
                formatter: function (cellvalue, option, rowObject) {
                    // return '<a href=" ' + rowObject.VideoLink + '" target="_blank">' + rowObject.VideoLink + '</a>';
                    return '<a href="/Ops/VideoLink?corporation=' + rowObject.Corporation + '&department=' + rowObject.Department +
                        '&fileNameSys=' + rowObject.FileNameSys + '&fileId=' + rowObject.FileId + '" target="_blank">' + rowObject.VideoLink + ' </a>';
                }
            },
        ],
        loadComplete: function (data) {
            if ($.isEmptyObject(data)) {
                $("#btnShowVideo").prop('disabled', true);
            }
            else {
                $("#btnShowVideo").prop('disabled', false);
            }
        },
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
        },
        width: null,
        shrinkToFit: false,
        caption: "List of Video Link",
        gridview: true,
    });
}

//Machine function
function JqGridMachine(styleCode, styleSize, styleColor, revNo, opRevNo, edition) {
    jQuery("#tbMachine").jqGrid({
        url: '/Ops/GetMachineJquery',
        datatype: "json",
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColor: styleColor,
            revNo: revNo,
            opRevNo: opRevNo,
            edition: edition,
        },
        height: 250,
        colModel: [
            {
                name: 'ImagePath', index: 'ImagePath', label: 'ImagePath', width: 100, align: 'center', formatter: function (cellvalue, options, rowobject) {
                    return "<img src='http://118.69.170.24:8005/OPS/ToolImages/" + cellvalue + "' width='60' height='20' onerror='imgError(this);' onclick=ShowImageDetailMachine('" + cellvalue + "')>";
                }
            },
            { name: 'ItemCode', index: 'ItemCode', label: 'ItemCode', width: 230 },
            { name: 'ItemName', index: 'ItemName', label: 'ItemName', width: 230 },
            {
                name: 'CategId', index: 'CategId', label: 'MachineType', width: 230, formatter: function (cellvalue, options, rowobject) {
                    return cellvalue + " - " + rowobject.MachineType;
                }
            },
        ],
        loadComplete: function (data) {
            if ($.isEmptyObject(data)) {
                $("#btnMachine").prop('disabled', true);
            }
            else {
                $("#btnMachine").prop('disabled', false);
            }

            var grid = $("#tbMachine"),
                ids = grid.getDataIDs();

            for (var i = 0; i < ids.length; i++) {
                grid.setRowData(ids[i], false, { height: 20 });
            }
        },
        loadError: function (xhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
        },
        width: null,
        shrinkToFit: false,
        caption: "List of Machine",
        gridview: true,
    });
}

function ShowImageDetailMachine(machine) {
    var url = "http://118.69.170.24:8005/OPS/ToolImages/" + machine;
    $("#imgPreviewDt").modal("show");
    $("#imgDetailDt").attr("src", url);
}
//END ADD: HA

//Check url exist or not
function checkUrlExist(url) {
    var config = ObjectConfigAjaxPost("/Ops/RemoteFileExists", false, JSON.stringify({ url: url }));
    var res = false;
    AjaxPostCommon(config, function (respone) {
        if (respone === Success) {
            res = true;
        } else {
            res = false;
        }
    });

    return res;
}
