
function eventClickButton() {
    //#region tab Module Revision
    $("#btnSaveLinkedModule").click(function () {
        var config = ObjectConfigAjaxPost("/Ops/InsertLinkedMboms", false, JSON.stringify({ listLnkBoms: getLinkedMbomsOnGrid() }));
        AjaxPostCommon(config, function (respone) {
            if (respone === Success) {
                ShowMessage('Save Module Revision', 'Saved', MessageTypeInfo);
            } else {
                ShowMessage('Save Module Revision', 'Cannot save', MessageTypeError);
            }
        });
    });
    //#endregion

    //#region tab Style Detail
    //START ADD - SON) 29/Oct/2020 - Add event more detail
    $('#achStyleDetail').click(() => {
        _activeTab = 1;
        const selOpmt = GetSelectedOneRowData(gridOpsTableId);
        //Load style information
        GetStyleMaster(selOpmt.StyleCode);

        //Change Efficiency and SAH
        getDataForStyleDetailTab(selOpmt)

        //START MOD - SON - 2021.01.15) 16/Jan/2021
        //Check role for style detail tab
        checkRoleBaseOnActiveTab(1, selOpmt.Edition, '');
        //END MOD - SON - 2021.01.15) 16/Jan/2021
    });

    $('#btnMoreStlDt').click(() => {
        ShowModal('mdlStyleDetail');

        //Load style detail.
    });
    //START ADD - SON) 29/Oct/2020
    //#endregion
}

// #region Regiser New Ops

// #region Modal Copy / Register new operation plan

//Event find list style follow Buyer and Style code
function ClickButonFilter() {
    $("#btnFilter").click(function () {

        var buyer = $("#drpBuyer").val();
        var startDate = "";
        var endDate = "";
        var searchText = $("#txtStyleFilter").val();
        var aoNo = $("#txtAoNumber").val();
        var data = { buyer: buyer, start: startDate, end: endDate, search: searchText, aoNumber: aoNo };
        ReloadJqGrid(NameTableStyleModal, data);

        //Clear grid operation plan on modal
        ReloadJqGrid2LoCal(NameTableOpsModal, []);

    });
}

//Neutral color
function ClickCheckboxRegisterNeutralColor() {

    $('#chkRegisterNeutralColor').change(function () {
        if ($(this).is(":checked")) {
            $("#txtColor").val("000");
            //styleColorSerial = NeutralColor;
        } else {
            $("#txtColor").val($("#hdStyleColor").val());
        }

    });
}

function SelectRadioCopySelectPlan() {
    $("#rdCopySelectPlan").click(function () {
        CheckCopyPatterntBomToolLinking(false);
        DisableCopyPatterntBomToolLinking(false);
        HideButtonTranslateLanguage(false);

        $("#chkRegisterNeutralColor").prop("disabled", false);
        CheckCopyPatterntBomToolLinking(true);
        window.ListOpsDetail = [];

        //Disable button upload csv file
        $("#btnUploadCsv").prop("disabled", true);

        var selOpmt = GetSelectedOneRowData(IdTableOpsModal);
        if (selOpmt != null) {
            //Get value from hiden field
            var processCount = selOpmt.OpCount;
            var machineCount = selOpmt.MachineCount;
            var workerCount = selOpmt.ManCount;
            var time = selOpmt.OpTime;

            //Set value of ops detail on modal
            SetValueOpDetailModal(processCount, machineCount, workerCount, time);
        }

        CheckTargetEdition();

    });
}

function SelectRadioRegisterEmptyPlan() {
    $("#rdRegisterEmptyPlan").click(function () {
        //Uncheck copy pattern, boom and tool linking.
        CheckCopyPatterntBomToolLinking(false);
        DisableCopyPatterntBomToolLinking(true);
        HideButtonTranslateLanguage(true);

        $("#chkRegisterNeutralColor").prop("disabled", false);
        window.ListOpsDetail = [];

        //Disable button upload csv file
        $("#btnUploadCsv").prop("disabled", true);

        //Set empty value for ops detail
        SetValueOpDetailModal("", "", "", "");

    });
}

function SelectRadioImportFileCsv() {
    $("#rdImportCsv").click(function () {
        //Uncheck copy pattern, boom and tool linking.
        CheckCopyPatterntBomToolLinking(false);
        DisableCopyPatterntBomToolLinking(true);
        HideButtonTranslateLanguage(true);

        $("#chkRegisterNeutralColor").prop("disabled", false);

        //Disable button upload csv file
        $("#btnUploadCsv").prop("disabled", false);

        SetValueOpDetailModal("", "", "", "");
    });
}

//Show modal import gsd file
function ClickButonImportCsvFile() {

    $("#btnUploadCsv").click(function () {
        $("#flImportCsv").val("");
        $("#dvImportFileError").hide();
        $("#dvImportFileError").empty();
        $("#btnAccept").prop('disabled', false);

        ShowModal(ImportCsvModal);

        //Get lis of process name
        //GetOpNameListForSelect2();

    });
}

function SelectLanguageAddNewOP() {
    $("#drpLanguageOpMaster").change(function () {
        ReloadJqGridLocal(GridOpsTableImportName, []);
    });
}

//Translate language ops detail.
function ClickButtonTranslateProcessName() {
    $("#btnTranslateProcessName").click(function () {
        var selOpmt = GetSelectedOneRowData(IdTableOpsModal);
        var lanIdOps = $("#drpLanguageOpMaster").val();
        var data = {
            styleCode: selOpmt.StyleCode,
            styleSize: selOpmt.StyleSize,
            styleColorSerial: selOpmt.StyleColorSerial,
            revNo: selOpmt.RevNo,
            opRevNo: selOpmt.OpRevNo,
            edition: selOpmt.Edition,
            languageId: lanIdOps
        };
        ReloadJqGrid(TableProcessName, data);

        //Set caption
        var tranLan = MapValueToNameLanguage(lanIdOps);
        jQuery(TableProcessNameId).jqGrid('setCaption', " Translate Process Name to " + tranLan);
        jQuery(TableProcessNameId).jqGrid('setLabel', 'OpNameLan', "Process Name in " + tranLan);

        ShowModal("mdlProcessName");
    });
}

//Save a copy or an empty ops
function ClickButtonRegisterOps() {
    $("#btnRegisterOps").click(function () {

        AddNewOps();

        ////Get object Operation Plan to copy content
        //var objOpsKeysCopy = GetObjOpsKeyCodeCopyModal();

        //var language = MapFlagValueToLanguage($("#drpLanguageOpMaster").val());
        ////The number of copies Operation Plan processes
        //var opCount = $("#txtProcess").val();

        ////START ADD) SON - 25/Feb/2019: Count processes with standard name
        //if ($("#rdCopySelectPlan").is(":checked")) {

        //    //START ADD) SON - 10/Apr/2019 - check selected operation plan which needs to copy
        //    //Check object of opeartion plan which need to copy
        //    let selCopiedOp = GetObjOpsKeyCodeCopyModal();
        //    if (!CheckOperationPlanMasterKeyIsValid(selCopiedOp)) return;
        //    //END ADD) SON - 10/Apr/2019

        //    var countPro = countProcessesWithStandardName(objOpsKeysCopy.Edition, language, objOpsKeysCopy.StyleCode, objOpsKeysCopy.StyleSize, objOpsKeysCopy.StyleColorSerial, objOpsKeysCopy.RevNo, objOpsKeysCopy.OpRevNo);
        //    if (countPro < opCount) {
        //        ShowConfirmYesNoMessage("003", SmsFunction.Confirm, MessageType.Confirm, MessageContext.Confirm, function () {

        //            AddNewOps();

        //        }, function () { }, countPro);
        //    } else { //Copy Operation Plan normally
        //        AddNewOps();
        //    }

        //} else { //Register an empty Operation Plan or Import processes from GSD file

        //    AddNewOps();
        //}

        ////END ADD) SON - 25/Feb/2019

    });
}

// #region Modal import gsd file

//Click accept import file.
function ClickButtonAcceptImportFile() {
    $("#btnAccept").click(function () {

        var gridData = $(GridOpsTableImportId).getGridParam('data');

        if ($("#rdAddNewProcess").prop("checked")) {
            //Check process name in database.
            CheckProcessNameIsStandard(gridData, function callBackFunc(result) {
                if (result.Result === Fail) {
                    ShowMessageOk("001", SmsFunction.Import, MessageType.Error, MessageContext.Error, ObjMessageType.Error, result.Content);
                    AcceptData = [];
                    return;
                } else {
                    AcceptData = result.Content;
                }

            });
        } else {
            for (var i = 0; i < gridData.length; i++) {

                if (isEmptyOrWhiteSpace(gridData[i].OpName)) {
                    //var dropdown = jQuery('#' + (i + 1) + '_OpName')[0];
                    //if (dropdown !== null) {
                    //    var selectedOption = dropdown.options[dropdown.selectedIndex];
                    //    if (!$.isEmptyObject(selectedOption)) {
                    //        var selectedText = selectedOption.text;
                    //        var selectedVal = selectedOption.value;

                    //        gridData[i].OpName = selectedText;
                    //        gridData[i].OpNameId = selectedVal;
                    //    } else {
                    //        ShowMessageOk("001", SmsFunction.Import, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, (i + 1));

                    //        return;
                    //    }
                    //}

                    var dropdown = jQuery('#' + (i + 1) + '_OpName')[0];
                    if (dropdown !== null) {
                        if (dropdown.length > 1) {
                            var arrSelectedText = "";
                            var arrSelectedVal = "";

                            for (var j = 0; j < dropdown.length; j++) {
                                arrSelectedText += dropdown[j].text
                                arrSelectedVal += dropdown[j].value
                                if (j + 1 !== dropdown.length) {
                                    arrSelectedText += " | ";
                                    arrSelectedVal += "|";
                                }
                            }
                            gridData[i].OpName = arrSelectedText;
                            gridData[i].ArrOpNameId = arrSelectedVal;
                        } else {
                            var selectedOption = dropdown.options[dropdown.selectedIndex];
                            if (!$.isEmptyObject(selectedOption)) {

                                var selectedText = selectedOption.text;
                                var selectedVal = selectedOption.value;

                                gridData[i].OpName = selectedText;
                                gridData[i].ArrOpNameId = selectedVal;
                                //gridData[i].OpNameId = selectedVal;
                            } else {
                                ShowMessageOk("001", SmsFunction.Import, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, (i + 1));

                                return;
                            }

                        }
                    }
                }
            }
            AcceptData = gridData;
        }

        if (!ArrayListIsNull(AcceptData)) {
            HideModal(ImportCsvModal);
        }
    });
}

function CheckProcessNameIsStandard(lstOpdt, callBackFunc) {
    var config = ObjectConfigAjaxPost("/Ops/CheckProcessNameIsStandard", false, JSON.stringify({ lstOpdt: lstOpdt }));
    AjaxPostCommon(config, function (respone) {
        callBackFunc(respone);
    });
}

// #endregion

// #endregion

// #region Menu Register new Ops

//Registration Ops
function ClickButtonAddOpsMaster() {
    $("#" + DivMenuOpsRegistration + " .btnAdd").click(function () {

        var styleKeyCode = GetStyleMasterKeyCode();
        if ($.isEmptyObject(styleKeyCode)) {
            ShowMessageOk("003", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert);

            return;
        }

        ClearDataOnOpsModal();

        InitDataForAddNewOpsModal();

        $("#btnUploadCsv").prop('disabled', true);

        $('#chkRegisterNeutralColor').prop("checked", false);

        //Get max op revision when click style
        var edition = $("#drpTargetEdition").val();
        var maxOpRev = GetMaxOpRevision(edition,
            styleKeyCode.StyleCode,
            styleKeyCode.StyleSize,
            styleKeyCode.StyleColorSerial,
            styleKeyCode.RevNo);
        $("#txtOpRevision").val(maxOpRev);
        $("#hdOpRevNoMax").val(maxOpRev);

        SetValueForStyleModal(styleKeyCode.StyleCode,
            styleKeyCode.StyleSize,
            styleKeyCode.StyleColorSerial,
            styleKeyCode.RevNo,
            maxOpRev);

        ShowModal(OpsModal);
    });
}

function ClickButtonConfirmOpsMaster() {
    $("#" + DivMenuOpsRegistration + " .btnConfirm").click(function () {
        var opMaster = GetKeyCodeOpsMaster();
        if ($.isEmptyObject(opMaster)) {
            ShowMessageOk("004", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }

        ShowConfirmYesNoMessage("001", SmsFunction.Confirm, MessageType.Confirm, MessageContext.Confirm, function () {

            ConfirmOpsMaster(opMaster, function (resConf) {
                if (resConf === Success) {
                    ShowMessageOk("001", SmsFunction.Confirm, MessageType.Success, MessageContext.Confirm, ObjMessageType.Info);

                    //Reload data ops detail gridview.
                    opMaster.Edition = $("#drpOpsMasterEdition").val();
                    //ReloadJqGrid(gridOpsTableName, opMaster);
                    ReloadJqGrid2LoCal(gridOpsTableName, opMaster);
                } else {
                    ShowMessageOk("001", SmsFunction.Confirm, MessageType.Error, MessageContext.Error, ObjMessageType.Error, resConf);
                }
            });
        }, function () { });

    });
}

function ClickButtonDeleteOpsMaster() {
    $("#" + DivMenuOpsRegistration + " .btnDelete").click(function () {
        var opMaster = GetKeyCodeOpsMaster();
        if ($.isEmptyObject(opMaster)) {
            ShowMessageOk("004", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

            return;
        }

        ShowConfirmYesNoMessage("001", SmsFunction.Delete, MessageType.Confirm, MessageContext.DeleteConfirm, function () {
            var resDel = DeleteOpsMaster(opMaster);
            if (resDel === Success) {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info);

                //Reload data ops detail gridview.
                opMaster.Edition = $("#drpOpsMasterEdition").val();
                //ReloadJqGrid(gridOpsTableName, opMaster);
                ReloadJqGrid2LoCal(gridOpsTableName, opMaster);
                ReloadJqGrid(gridOpsDetailName, {});
            } else {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Error, resDel);
            }
        }, function () { }, "\"Operation Plan\"");

    });

    $("#btnDisableOp").click(function () {
        var opMaster = GetKeyCodeOpsMaster();
        if ($.isEmptyObject(opMaster)) {
            ShowMessageOk("004", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

            return;
        }

        ShowConfirmYesNoMessage("001", SmsFunction.Update, MessageType.Confirm, MessageContext.Update, function () {
            var resUpdate = UpdateIsUsedOp(opMaster);
            if (resUpdate === Success) {
                ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, ObjMessageType.Info);

                //Reload data ops detail gridview.
                opMaster.Edition = $("#drpOpsMasterEdition").val();
                ReloadJqGrid2LoCal(gridOpsTableName, opMaster);
                ReloadJqGrid(gridOpsDetailName, {});
            } else {
                ShowMessageOk("001", SmsFunction.Update, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Error, resUpdate);
            }
        }, function () { }, "\"Operation Plan\"");
    });
}

// #endregion

// #endregion

// #region Event tab Operation Plan

function ClickOperationPlanTab() {

    $("#achOperationPlan").click(function () {
        //Set active tab
        _activeTab = 2;

        BindDataToJqGridInputOpTimeModal([]);
        var dataRow = GetSelectedOneRowData(gridOpsTableId);
        GetDataForOperationTab(dataRow);

        //Init grid process template
        BindDataToJqGridProcessNameTemplate("", "");

        //CheckMachineOpsTable(); //HA ADD //MOD - SON - 2021.01.15) 16/Jan/2021

        //Move this function to function ready in Ops.cshtml 
        //START ADD) SON - 2019.03.1.0 - 07/Mar/2019: Init master data for process modal
        //initMasterDataProcessModal();
        //END ADD) SON - 2019.03.1.0 - 07/Mar/2019

        // Add func change color Oanh 22Jan2021
        setBackgroundColorJqGridModal();
    });
}

//Export operation plan detail to csv
function ClickButtonExportOpDetailToCsv() {
    $("#btnExportCsvOpDetail").click(function () {
        var d = new Date();
        var fileName = d.getFullYear().toString() + d.getMonth().toString() + d.getDay().toString() + d.getHours().toString() + d.getMinutes().toString() + d.getSeconds().toString() + "-ProcessDetail.csv";
        //downloadCSV({ filename: fileName });

        DowloadProcessToExcel();
    });
}

function SelectLanguageChange2() {
    $("#drpLanguages").change(function () {

        var selLanId = $("#drpLanguages").val();
        var oDropdown = $("#drpLanguages").msDropdown().data("dd");
        var index = oDropdown.get("selectedIndex");

        //var objOpsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));
        var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
        if (!$.isEmptyObject(objOpsMaster)) {
            var dataKey = {
                styleCode: objOpsMaster.StyleCode,
                styleSize: objOpsMaster.StyleSize,
                styleColor: objOpsMaster.StyleColorSerial,
                revNo: objOpsMaster.RevNo,
                opRevNo: objOpsMaster.OpRevNo,
                edition: objOpsMaster.Edition,
                languageId: selLanId
            };
            ReloadJqGrid(gridOpsDetailName, dataKey);

            //Show button save operatoin name.
            var opsLanId = MapLanguageToFlag($("#hdOpsLanguage").val());

            //Show colum following the language.
            ShowOpNameColumGridOpDetail(gridOpsDetailId, selLanId, opsLanId);

            if (selLanId === opsLanId || selLanId === DefaultLanguage) {
                $("#btnSaveOpName").hide();
            } else {
                $("#btnSaveOpName").show();
            }

        }

    });
}

function SelectChangeGroupShow() {
    $("#drpGroupShow").change(function () {
        var showVal = $(this).val();
        ChangeGroupingJqGrid(gridOpsDetailId, showVal);
    });
}

function ShowNoneStandardName() {
    $("#btnShowNoneStdName").click(function () {

        ISSHOWNONESTDNAME = true;

        var dataRow = GetSelectedOneRowData(gridOpsTableId);

        var data = {
            styleCode: dataRow.StyleCode,
            styleSize: dataRow.StyleSize,
            styleColor: dataRow.StyleColorSerial,
            revNo: dataRow.RevNo,
            opRevNo: dataRow.OpRevNo,
            edition: dataRow.Edition,
            languageId: MapLanguageToFlag(dataRow.Language)
        };

        //Note: must load data the first time (the first time loading empty data), 
        //after that can ReloadJqGrid function. This function can work.
        ReloadJqGrid(gridOpsDetailName, data);// Move to bottom

        ISSHOWNONESTDNAME = false;
    });
}
//#region Menu button registration Ops Detail

function BtnAddClick() {
    $("#" + DivMenuOpsDetail + " .btnAdd").click(function () {

        //InitDataAddNewProcess();

        clearDataOnProcessDtModal();
        ShowModal('mdlProcessDetail');

        StatusUpdateProcess = 0;
        GetMaxOpSerial();
        InitDataForProcessModal();
        $('#btnUpdateProcess').hide();
        $('#btnSaveProcess').show();

        //START ADD - SON) 15/Jan/2021
        $('#btnSaveLinkingBomPattern').show();
        //END ADD - SON) 15/Jan/2021

    });
}

function BtnEditClick() {
    $("#" + DivMenuOpsDetail + " .btnEdit").click(function () {

        if (!CheckNumberSelectedRow(gridOpsDetailId)) return;

        var selectedRowData = GetSelectedOneRowData(gridOpsDetailId);
        var objOpsDetail = GetObjectOpsDetail(selectedRowData);
        objOpsDetail.LanguageId = $("#drpLanguages").val();
        InitDataUpdateProcess(objOpsDetail);
    });
}

function BtnDeleteClick() {
    $("#" + DivMenuOpsDetail + " .btnDelete").click(function () {

        var myGrid = $(gridOpsDetailId);
        var selRowIds = myGrid.jqGrid('getGridParam', 'selarrrow');
        if (selRowIds.length === 0) {
            ShowMessageOk("005", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert);

            return;
        }

        ShowConfirmYesNoMessage("001", SmsFunction.Delete, MessageType.Confirm, MessageContext.DeleteConfirm, function () {
            //Get list op detail to delete
            var lstOpDetail = [];

            //var edition = $("#hdOpDetailEdition").val();
            for (var i = 0, n = selRowIds.length; i < n; i++) {
                var rowData = myGrid.jqGrid("getRowData", selRowIds[i]);
                //rowData.Edition = edition;
                lstOpDetail.push(rowData);
            }

            //Check object op detail is null or not
            if (!$.isEmptyObject(lstOpDetail)) {
                //Delete Ops Detail
                var resDel = DeleteOpsDetail(lstOpDetail);
                if (resDel === Success) {
                    var opMaster = GetSelectedOneRowData(gridOpsTableId);
                    //Reload data ops detail gridview.
                    //ReloadJqGrid(gridOpsDetailName, opMaster);

                    opMaster.edition = $("#drpOpsMasterEdition").val();
                    //ReloadJqGrid(gridOpsTableName, opMaster);
                    ReloadJqGrid2LoCal(gridOpsTableName, opMaster);

                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info);

                } else {
                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Error, MessageContext.Error, ObjMessageType.Error, resDel);
                }
            } else {
                ShowMessageOk("006", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            }
        }, function () { }, "\"Process\"");

    });
}

//#endregion

// #region Save and change process name

//Event click button Save operation name
function ClickButtonSaveOpName() {
    $("#btnSaveOpName").click(function () {
        ShowConfirmYesNoMessage("001", SmsFunction.Update, MessageType.Confirm, MessageContext.UpdateConfirm, function () {
            //Get all row data on gridview ops detail.
            var gridData = jQuery(gridOpsDetailId).jqGrid("getRowData");
            var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
            var languageId = MapFlagValueToLanguage($("#drpLanguages").val());
            if (languageId === objOpsMaster.language) {

                ShowMessageOk("007", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            } else {
                if (UpdateOpName(objOpsMaster, gridData, languageId)) {
                    //Hiden button save process name
                    $("#btnSaveOpName").hide();

                    //Reload grid ops master.               
                    var data = {
                        styleCode: objOpsMaster.StyleCode,
                        styleSize: objOpsMaster.StyleSize,
                        styleColor: objOpsMaster.StyleColorSerial,
                        revNo: objOpsMaster.RevNo,
                        edition: $("#drpOpsMasterEdition").val()
                    };
                    //Reload grid ops detail on modal
                    //ReloadJqGrid(gridOpsTableName, data);
                    ReloadJqGrid2LoCal(gridOpsTableName, data);
                }
            }
        }, function () { });

    });
}

// #endregion

// #region Event on upload machine file modal
function ClickButtonUploadOpDetailFile() {
    $("#btnUploadFileOpDetail").click(function () {

        var files = $("#flOpDetail")[0].files;

        //Check vaild file
        if (!CheckDataBeforeUploadOpDetailFile(files)) { return };

        //block UI
        $.blockUI(ObjectBlockUICss);

        setTimeout(function () {
            //Get selected row on jqgrid opdetail.
            var objOpsDetail = SeletedObjOpsDetail;
            var resUpload;
            if ($("#rdUploadMachineFileOpDetail").is(':checked')) {
                //Upload machine file

                //If file type is "Other file" then do not need to check machien file.
                var strExt = $("#drpJigFileType").val().split('-')[1];
                if (strExt !== "ALL" && strExt !== "NON") {
                    if (!CheckTypeOfMachineFile(files)) {
                        ShowMessageOk("001", SmsFunction.Check, MessageType.Error, MessageContext.Error, ObjMessageType.Error, ArrMachineFileType.toString());

                        $.unblockUI();
                        return;
                    }
                }

                UploadJigFile(MachineFile, function callBackFnc(objResult) {
                    objResult = JSON.parse(objResult);
                    $("#flOpDetail").val("");
                    if (objResult === Success) {
                        //Reload grid process detail.
                        //Get data from local storage.
                        var opMaster = GetSelectedOneRowData(gridOpsTableId);
                        //Reload data ops detail gridview.
                        ReloadJqGrid(gridOpsDetailName, opMaster);

                        ShowMessageOk("002", SmsFunction.Upload, MessageType.Success, MessageContext.Update, ObjMessageType.Info, objResult);
                    } else {
                        ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error, objResult);
                    }

                    //unblock UI
                    $.unblockUI();
                });

            } else if ($("#rdUploadVideoOpDetail").is(':checked')) {
                //Upload video
                if (!CheckTypeOfVideo(files)) {
                    ShowMessageOk("008", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, ArrVideoType.toString());
                    $.unblockUI();
                    return;
                }

                //Set file type to hiden field
                $("#hdFileTypeOpDetail").val(VideoType);

                let resUpload = UploadVideoOpDetail();
                if (resUpload !== Fail) {
                    objOpsDetail.VideoFile = resUpload;
                    UpdateFilenameUpload(VideoType, objOpsDetail, resUpload, function (objResult) {
                        UploadFileAlert(objResult);
                    });
                } else {
                    //Upload fail
                    ShowMessage("Upload video", resUpload, ObjMessageType.Error);
                }

            } else {
                //upload jig file.
                if (!CheckIsJigFile(files)) {
                    ShowMessageOk("008", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, ArrJigFileType.toString());
                    $.unblockUI();

                    return;
                }

                UploadJigFile(JigFile, function callBackFnc(objResult) {
                    objResult = JSON.parse(objResult);
                    if (objResult === Success) {
                        objResult = "Uploaded file.";
                        $("#flOpDetail").val("");

                        //Reload grid process detail.
                        //Get data from local storage.
                        var opMaster = GetSelectedOneRowData(gridOpsTableId);
                        //Reload data ops detail gridview.
                        ReloadJqGrid(gridOpsDetailName, opMaster);
                        ShowMessageOk("002", SmsFunction.Upload, MessageType.Success, MessageContext.Update, ObjMessageType.Info);

                    } else {
                        ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error, objResult);

                    }

                    //unblock UI
                    $.unblockUI();
                });

            }

            //unblock UI
            $.unblockUI();
        }, 100);
    });
}

//Upload Operation Plan Detail
function SelectRadioUploadVideoOpDetail() {
    $("#rdUploadVideoOpDetail").click(function () {
        $("#flOpDetail").attr("accept", "video/*");
        $("#dvPreviewOpFile").show();
        $("#divUploadJigFile").hide();
        $("#divRefVideoLink").hide();
        $("#flOpDetail").show();
        RemoveOpDetailFilePreview();

        //Hide video links
        $("#divVideoList").hide();

        $("#btnUploadFileOpDetail").show();
        $("#btnRemoveFileOpDetail").show();
    });
}

function SelectVideoFromServer() {
    $("#rdGetVideoFromServer").click(function () {
        $("#divVideoList").show();

        //Hide upload video, upload machine file, linking file DMS
        $("#divUploadJigFile").hide();
        $("#divRefVideoLink").hide();
        $("#flOpDetail").hide();
        $("#dvPreviewOpFile").hide();
        RemoveOpDetailFilePreview();

        $("#btnUploadFileOpDetail").hide();
        $("#btnRemoveFileOpDetail").hide();

        //Get selected row on jqgrid opdetail.
        var objOpsDetail = SeletedObjOpsDetail;
        $("#txtStyleCodeVideo").val(objOpsDetail.StyleCode);
        $("#txtStyleSizeVideo").val(objOpsDetail.StyleSize);
        $("#txtStyleColorVideo").val(objOpsDetail.StyleColorSerial);
        $("#txtStyleRevNoVideo").val(objOpsDetail.RevNo);

        var postData = {
            styleCode: objOpsDetail.StyleCode,
            styleSize: objOpsDetail.StyleSize,
            styleColorSerial: objOpsDetail.StyleColorSerial,
            revNo: objOpsDetail.RevNo
        };
        ReloadJqGrid(TableVideosName, postData);

        /*End Oanh add change color 20Jan2021*/
        setBackgroundColorJqGridModal();
    });
}

function SelectRadioUploadMachineFileOpDetail() {
    $("#rdUploadMachineFileOpDetail").click(function () {

        //Load jig file types.
        GetMasterCodeWithMSD(StyleFile, "", MachineFileDesc, function (respone) {
            $.each(respone, function (index, value) {
                //Combine Sub Code and Code Detail 2 (file extension)
                value.SubCode = value.SubCode + "-" + value.CodeDetail2;
            });
            FillDataToDropDownlist("drpJigFileType", respone, "SubCode", "CodeName");
        });

        //$("#flOpDetail").attr("accept", ".dxf, .vdt, .ptg, .dat, .sew");
        $("#flOpDetail").removeAttr("accept");
        $("#divUploadJigFile").show();
        $("#divRefVideoLink").hide();
        $("#flOpDetail").show();
        $("#dvPreviewOpFile").hide();
        RemoveOpDetailFilePreview();

        $("#btnUploadFileOpDetail").show();
        $("#btnRemoveFileOpDetail").hide();

        //Hide video links
        $("#divVideoList").hide();
    });
}

function SelectShowModalGetFileFromDms() {
    $("#rdGetFileFromDms").click(function () {
        var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
        $("#txtStyleCodeUploading").val(objOpsMaster.StyleCode);
        $("#txtStyleSizeUploading").val(objOpsMaster.StyleSize);
        $("#txtStyleColorUploading").val(objOpsMaster.StyleColorSerial);
        $("#txtRevisionUploading").val(objOpsMaster.RevNo);

        $("#txtStyleCodeUploading").attr("disabled", "disabled");
        $("#txtStyleSizeUploading").attr("disabled", "disabled");
        $("#txtStyleColorUploading").attr("disabled", "disabled");
        $("#txtRevisionUploading").attr("disabled", "disabled");

        //Clear object files pdm linking.
        ObjFilesPdmLinking = {};
        ReloadJqGrid(TableFileName, []);

        //Load type uploadding
        Selection2("drpTypeFileUploading");
        Selection2("drpFileTypeUploading");

        //Get file type.
        var typeUploadText = $("#drpTypeFileUploading option:selected").text();
        GetMasterCodeFiles(StyleFile, "", typeUploadText);

        //Load types of file
        ShowModal("mdlUploadMachineFile");

        $("#divUploadJigFile").hide();
        $("#divRefVideoLink").hide();
        $("#flOpDetail").hide();
        $("#dvPreviewOpFile").hide();
        RemoveOpDetailFilePreview();
        $("#btnUploadFileOpDetail").hide();
        $("#btnRemoveFileOpDetail").hide();
        $("#rdGetFileFromDms").prop('checked', false);

        //Hide video links
        $("#divVideoList").hide();
    });
}

function SelectRadioUploadJigFile() {
    $("#rdUploadJigFile").click(function () {
        //Load jig file types.
        GetMasterCodeWithMSD(StyleFile, "", JigFile, function (respone) {
            $.each(respone, function (index, value) {
                //Combine Sub Code and Code Detail 2 (file extension)
                value.SubCode = value.SubCode + "-" + value.CodeDetail2;
            });
            FillDataToDropDownlist("drpJigFileType", respone, "SubCode", "CodeName");
        });

        $("#flOpDetail").attr("accept", ".dxf,image/*");

        //Hide upload video, upload machine file, linking file DMS
        $("#divUploadJigFile").show();
        $("#divRefVideoLink").hide();
        $("#flOpDetail").show();
        $("#dvPreviewOpFile").hide();
        RemoveOpDetailFilePreview();
        $("#divVideoList").hide();
        $("#btnUploadFileOpDetail").show();
        $("#btnRemoveFileOpDetail").hide();
    });
}

function SelectRadioAddRefVideoLink() {
    $("#rdAddRefVideoLink").click(function () {
        //Hide upload video, upload machine file, linking file DMS
        $("#divUploadJigFile").hide();
        $("#divRefVideoLink").show();
        $("#flOpDetail").hide();
        $("#dvPreviewOpFile").hide();
        RemoveOpDetailFilePreview();
        $("#divVideoList").hide();
        $("#btnUploadFileOpDetail").hide();
        $("#btnRemoveFileOpDetail").hide();
    });
}

function SelectFileOpDetailUploadChange() {
    $("#flOpDetail").change(function (evt) {
        //var fileName = evt.target.files[0].name;
        //If user upload file then check file machine size 
        if (!$("#rdUploadVideoOpDetail").is(':checked')) {
            var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, Megabyte);
            if (fileSize > 4) {
                ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

                return;
            }
        } else {
            //Preview video before upload
            var $source = $('#opDetailVideoPreview');
            $source[0].src = URL.createObjectURL(this.files[0]);
            $source.parent()[0].load();
        }

    });
}

function SelectTypeUploading() {
    $("#drpTypeFileUploading").change(function () {
        var typeUploadText = $("#drpTypeFileUploading option:selected").text();
        GetMasterCodeFiles(StyleFile, "", typeUploadText);
    });
}

function CreateObjectFile(styleCode, styleSize, styleColorSerial, revNo, uploadCode) {
    var objFile = {
        StyleCode: styleCode,
        StyleSize: styleSize,
        StyleColorSerial: styleColorSerial,
        RevNo: revNo,
        UploadCode: uploadCode
    };

    return objFile;
}

function ClickButtonFindDmsFile() {
    $("#btnFindDmsFile").click(function () {

        var proRowData = SeletedObjOpsDetail;

        var styleCode = $("#txtStyleCodeUploading").val();
        var styleSize = $("#txtStyleSizeUploading").val();
        var styleColorSerial = $("#txtStyleColorUploading").val();
        var revNo = $("#txtRevisionUploading").val();
        var uploadCode = $("#drpFileTypeUploading").val();
        var styleFileDesc = $("#drpTypeFileUploading option:selected").text();
        var opRevNo = proRowData.OpRevNo;
        var opSerial = proRowData.OpSerial;
        var edition = proRowData.Edition;

        var postData = {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            uploadCode: uploadCode,
            styleFile: StyleFile,
            styleFileDesc: styleFileDesc,
            opRevNo: opRevNo,
            opSerial: opSerial,
            edition: edition
        };

        //Use for reload jqgrid after linking.
        ObjFilesPdmLinking = postData;
        ReloadJqGrid(TableFileName, postData);

    });
}

function ClickButtonUploadFileFromDms() {
    $("#btnUploadFileFromDms").click(function () {

        //Get list of files name from gridview.
        var objOpdt = SeletedObjOpsDetail;

        var fileGrid = $(TableFileId);
        var selRowIds = fileGrid.jqGrid('getGridParam', 'selarrrow');
        var lstSdFile = [];

        if (selRowIds.length === 0) {
            ShowMessageOk("001", SmsFunction.Link, MessageType.Error, MessageContext.InvalidData);

            return;
        }

        for (var i = 0, n = selRowIds.length; i < n; i++) {
            var rowData = fileGrid.jqGrid("getRowData", selRowIds[i]);
            lstSdFile.push(rowData);
        }

        LinkFilesToPdm(lstSdFile, objOpdt);
    });
}

function ClickButtonAddRefVideoLink() {
    $("#btnAddRefVideoLink").click(function () {

        var refLink = $("#txtRefVideoLink").val();

        //var url = "http://video.pungkookvn.com:8888/api/Media/Play?fol=10082001&f=47X8w2sbzGUmc2T.mp4";
        var isExist = checkUrlExist(refLink);
        if (!isExist) {
            ShowMessageOk("005", SmsFunction.Add, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }

        if (refLink.indexOf("http://video.pungkookvn.com:8888") === -1) {
            ShowMessageOk("006", SmsFunction.Add, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }

        //Get selected operation plan
        var objOpMaster = GetSelectedOneRowData(gridOpsTableId);

        //Get process key code from selected process.
        var styleCode = objOpMaster.StyleCode;
        var styleSize = objOpMaster.StyleSize;
        var styleColor = objOpMaster.StyleColorSerial;
        var revNo = objOpMaster.RevNo;
        var opRevNo = objOpMaster.OpRevNo;
        var opSerial = $("#hdOpSerialOpDetail").val();
        var edition = objOpMaster.Edition;
        //var refLink = $("#txtRefVideoLink").val();
        var refVideoName = $("#txtRefVideoName").val();

        if ($.isEmptyObject(refLink) || $.isEmptyObject(refVideoName)) {
            ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Info, "Reference video link.");
            return;
        }

        var config = ObjectConfigAjaxPost("/Ops/AddReferencVideoLink", true, JSON.stringify({
            styleCode: styleCode
            , styleSize: styleSize
            , styleColor: styleColor
            , revNo: revNo
            , opRevNo: opRevNo
            , opSerial: opSerial
            , edition: edition
            , refLink: refLink
            , refVideoName: refVideoName
        }));

        AjaxPostCommon(config, function (respone) {
            if (respone === Success) {
                ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Save, ObjMessageType.Info);
                $("#txtRefVideoName").val("");
                $("#txtRefVideoLink").val("");

                //Reload grid process detail.
                //Get data from local storage.
                var opMaster = GetSelectedOneRowData(gridOpsTableId);
                //Reload data ops detail gridview.
                ReloadJqGrid(gridOpsDetailName, opMaster);
            } else {
                ShowMessageOk("001", SmsFunction.Link, MessageType.Error, MessageContext.Database, ObjMessageType.Error, respone);
            }
        });
    });
}
// #endregion

// #region Update process name

//Update process time from csv file.
function ClickButtonUpdateOpTime() {
    $("#btnUpdateProcessTime").click(function () {
        ShowMessage("Update process time", "This function is not available now.", MessageTypeInfo);
        //BinDataToJqGridUpdateProcessTime([]);
        //ShowModal("mdlUpdateProcessTime");
    });
}

function ClickButtonUpdateProcessTime() {
    $("#btnUpdateOpTime").click(function () {
        var proDataUpdate;
        var gridData = $(TableUpdateProcessTimeId).getGridParam('data');
        //Check process name in database.
        CheckProcessNameIsStandard(gridData, function callBackFunc(result) {
            if (result.Result === Fail) {
                ShowMessageOk("001", SmsFunction.Import, MessageType.Error, MessageContext.Error, ObjMessageType.Error, result.Content);

                return;
            } else {
                proDataUpdate = result.Content;
                //$.each(proDataUpdate, function (index, value) {
                //    console.log(value.OpNameId);
                //});
            }

        });
        //Update process time.
        UpdateProcesTime(proDataUpdate, function (resUpdate) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, ObjMessageType.Info);

        });
    });
}

//Update list of process time.
function UpdateProcesTime(lstOpnt, callBackFunc) {
    var config = ObjectConfigAjaxPost("/Ops/UpdateProcessTime", false, JSON.stringify({ lstOpnt: lstOpnt }));
    AjaxPostCommon(config, function (respone) {
        callBackFunc(respone);
    });
}

function SelectProcessCsvFile() {
    $('#flImportProcessCsv').bind('change', function (evt) {
        var files = evt.target.files;
        var file = files[0];

        var reader = new FileReader();
        reader.readAsText(file);

        reader.onload = function (event) {
            var csv = event.target.result;
            var data = $.csv.toArrays(csv);
            var objOpMaster = GetSelectedOneRowData(gridOpsTableId);
            var objResult = CreateObjectProcessFromCsvData(objOpMaster.StyleCode, objOpMaster.StyleSize, objOpMaster.StyleColorSerial, objOpMaster.RevNo, objOpMaster.OpRevNo, objOpMaster.Edition, data);

            var err = objResult.Error;
            if (!isEmpty(err)) {
                err = "Operation time is not correct at row: " + err;
                $("#divUpdateProcessTimeError").append(err);
                $("#divUpdateProcessTimeError").show();

                $("#btnUpdateOpTime").prop('disabled', true);
            } else {
                $("#divUpdateProcessTimeError").append("");
                $("#divUpdateProcessTimeError").hide();

                $("#btnUpdateOpTime").prop('disabled', false);
            }

            ReloadJqGridLocal(TableUpdateProcessTimeName, objResult.ListProcess);

        }
    });
}

// #endregion

// #endregion

// #region Event tab Style Detail

function ClickButtonUploadImageStyle() {
    $("#btnUploadImageStyle").click(function () {
        UploadImageStyle(function (resUpload) {
            if (resUpload === Success) {
                //Remove file from control
                $("#flImageDetail").val("").clone(true);
                ShowMessageOk("001", SmsFunction.Upload, MessageType.Success, MessageContext.Update, ObjMessageType.Info);

            } else {
                ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error);
            }
        });

    });
}

function SelectStyleImageChange() {
    $("#flImageDetail").change(function (evt) {
        //Check file size of style
        var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, Megabyte);
        if (fileSize > 20) {
            ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData);
            return;
        }
        //Preview image before upload to FTP
        readURL(this, "#imgPreviewDetail");
    });
}

// #endregion

// #region Event tab Line Banlancing

function ClickTabBalancing() {

    $("#achLineBalancing").click(function () {
        //Set active tab
        _activeTab = 3;
        GetDataForLineBalancingTab();

        //START ADD - SON - 2021.01.15) 16/Jan/2021 - check role on line balacing tab
        const selOpmt = GetSelectedOneRowData(gridOpsTableId);
        checkRoleBaseOnActiveTab(3, selOpmt.Edition, selOpmt.ConfirmChk);
        //END ADD - SON - 2021.01.15) 16/Jan/2021
    });
}

function SelectChartType() {
    $("#drpChartType").change(function () {

        var chartType = GetChartType();

        var datasource = CreateDatasource();
        $("#chart-container").updateFusionCharts({ dataSource: datasource, type: chartType });

    });
}

function SelectShowChart() {
    $("#drpShowChart").change(function () {

        var chartType = GetChartType();

        var datasource = CreateDatasource();

        $("#chart-container").updateFusionCharts({ dataSource: datasource, type: chartType });

        //var revenueChart = FusionCharts(chartName);
        //revenueChart.chartType(chartType)

    });
}

function ClickCheckboxShowVaule() {
    $('#chkShowValue').change(function () {
        var charProp = ChartProperties();
        charProp.showValues = $("#chkShowValue").is(":checked") ? "1" : "0";

        var datasource = CreateDataSourceProperies(charProp, tempCategories, tempDataChart);
        $("#chart-container").updateFusionCharts({ dataSource: datasource, dataFormat: 'json' });

        //var revenueChart = FusionCharts("chartProcess");
        //revenueChart.setChartAttribute('usePlotGradientColor', 1)
    });
}

function SelectShowTime() {
    $("#drpShowTime").change(function () {
        var chartType = GetChartType();

        var datasource = CreateDatasource();

        $("#chart-container").updateFusionCharts({ dataSource: datasource, type: chartType });
        if ($(this).val() === "1") {
            $("#btnSaveBalancingTime").text("").append("<span class='fa fa-edit'></span> Edit");
        }

    });
}

function SaveBlancingTime() {
    $("#btnSaveBalancingTime").click(function () {
        //$("#drpShowTime").val("2").trigger("change");

        //Check test is Edit or Save
        var buttonText = $(this).text().trim();

        if (buttonText === "Edit") {
            $(this).text("").append("<span class='fa fa-floppy-o'></span> Save");
            $("#drpShowTime").val("2").trigger("change");

        } else if (buttonText === "Save") {
            var revenueChart = FusionCharts(chartName);
            if ($.isEmptyObject(revenueChart.args.dataSource.dataset[0].data)) {
                ShowMessageOk("001", SmsFunction.Update, MessageType.Warning, MessageContext.NoData, ObjMessageType.Alert);

                return;
            }
            ShowConfirmYesNoMessage("001", SmsFunction.Update, MessageType.Confirm, MessageContext.UpdateConfirm, function () {
                if (revenueChart !== undefined) {

                    // get chart json data
                    var jsonData = revenueChart.getJSONData();

                    //Update optime balancing
                    //Get ops master from loacal storage.
                    var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
                    if ($.isEmptyObject(objOpsMaster) || $.isEmptyObject(jsonData)) {
                        return null;
                    }

                    var arrOpDetail = [];
                    if ($.isEmptyObject(jsonData)) return;

                    for (var i = 0; i < jsonData.dataset[0].data.length; i++) {
                        var objOpDetail = {
                            Edition: objOpsMaster.Edition,
                            StyleCode: objOpsMaster.StyleCode,
                            StyleSize: objOpsMaster.StyleSize,
                            StyleColorSerial: objOpsMaster.StyleColorSerial,
                            RevNo: objOpsMaster.RevNo,
                            OpRevNo: objOpsMaster.OpRevNo,
                            OpSerial: jsonData.dataset[0].data[i].opserial,
                            OpTimeBalancing: jsonData.dataset[0].data[i].value
                        };

                        arrOpDetail.push(objOpDetail);
                    }

                    UpdateOpTimeBalancing(arrOpDetail);

                    //Reload gridview ops detail
                    var data = {
                        styleCode: objOpsMaster.StyleCode,
                        styleSize: objOpsMaster.StyleSize,
                        styleColor: objOpsMaster.StyleColorSerial,
                        revNo: objOpsMaster.RevNo,
                        opRevNo: objOpsMaster.OpRevNo,
                        edition: objOpsMaster.Edition,
                        languageId: objOpsMaster.Language
                    };
                    ReloadJqGrid(gridOpsDetailName, data);
                }
            }, function () { });

            $(this).text("").append("<span class='fa fa-edit'></span> Edit");
        }
    });
}

function ClickButtonExportBalancingToExcel() {
    $("#btnExportBalToExcel").click(function () {
        var objOpMaster = GetSelectedOneRowData(gridOpsTableId);
        var revenueChart = FusionCharts(chartName);
        if ($.isEmptyObject(revenueChart.args.dataSource.dataset[0].data) || $.isEmptyObject(objOpMaster)) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Warning, MessageContext.NoData, ObjMessageType.Alert);

            return;
        }

        window.open(`/ExportExcel/ExportBalancingToExcel/?styleCode=${objOpMaster.StyleCode}&styleSize=${objOpMaster.StyleSize}&styleColorSerial=${objOpMaster.StyleColorSerial}&revNo=${objOpMaster.RevNo}&opRevNo=${objOpMaster.OpRevNo}&edition=${objOpMaster.Edition}&languageId=${objOpMaster.Language}`);

    });
}

// #endregion

// #region Event tab Module

function ClickLayoutTab() {
    //$("#achLayout").click(function () {
    //    window.location = '/OpsLayout/OpsLayout';
    //});
}

function ClickTabModule() {
    $("#achModule").click(function () {
        //Set active tab
        _activeTab = 4;

        //START MOD - SON - 2021.01.15) 16/Jan/2021
        //Check role for module tab
        //CheckRoleModuleManagement(); //ADD - SON) 29/Oct/2020
        checkRoleBaseOnActiveTab(4, null, null);
        //END MOD - SON - 2021.01.15) 16/Jan/2021

        //Get search style info
        var objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));
        var styleCode;

        if (!$.isEmptyObject(objStyleMaster)) {
            styleCode = objStyleMaster.StyleCode;

            LoadDataForAddingModule(styleCode);
        } else {
            ShowMessageOk("003", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

            return;
        }

        //Get list module part
        GetModulePart();
        // Oanh add function change color 19Jan2021
        setTimeout(() => {
            setBackgroundColorJqGridModal();        
        }, 0);
    });

    $("#achModuleRevision").click(function () {
        //Set active tab
        _activeTab = 5;
        //Get style master from local storage
        let objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));
        BindDataToJqGridModuleRevision(objStyleMaster.StyleCode, objStyleMaster.StyleSize, objStyleMaster.StyleColorSerial, objStyleMaster.RevNo);
        ////Reload module  revision    
        //let postData = {
        //    styleCode: objStyleMaster.StyleCode,
        //    styleSize: objStyleMaster.StyleSize,
        //    styleColorSerial: objStyleMaster.StyleColorSerial,
        //    revNo: objStyleMaster.RevNo
        //};
        //ReloadJqGrid(TableModuleRevisionName, postData);

        // Oanh add function change color 19Jan2021
        setBackgroundColorJqGridModal();

    });
}

function SelectStyleGroup() {
    $("#drpStyleGroup").change(function () {
        var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
        if (isEmpty(objOpsMaster.SubGroup)) {
            var subCode = '';
            var codeDes = $(this).val();
            GetStyleSubGroupMaster(StyleSubGroup, subCode, codeDes);
        }
    });
}

function SelectStyleSubGroup() {
    $("#drpStyleSubGroup").change(function () {
        var objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
        if (isEmpty(objOpsMaster.SubSubGroup)) {
            var subCode = '';
            var codeDes = $("#drpStyleGroup").val();
            var codeDetail = $(this).val();
            GetStyleSubSubGroupMaster(StyleSubSubGroup, subCode, codeDes, codeDetail);

            var objMrul = CreateObjectMrul();
            GetModulesLevel(objMrul);
        }
    });
}

function SelectStyleSubSubGroup() {
    $("#drpStyleSubSubGroup").change(function () {
        var objMrul = CreateObjectMrul();
        GetModulesLevel(objMrul);
    });
}

function SelectMachineRange() {
    $("#drpMachineRange").change(function () {
        var objMrul = CreateObjectMrul();
        GetModulesLevel(objMrul);
    });
}

function ClickButtonAddModule() {
    $("#btnAddModule").click(function () {

        var mainLevel = "SUB";
        var arrModuleLevel = [];

        $('#drpModuleLevel option:selected').each(function () {
            arrModuleLevel.push($(this).val().split("-")[0]);
        });

        if ($.isEmptyObject(arrModuleLevel)) {
            ShowMessageOk("009", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

            return;
        }

        //Get style master from local storage
        var objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));

        if ($.isEmptyObject(objStyleMaster.Buyer) || $.isEmptyObject(objStyleMaster.StyleCode)) {
            ShowMessageOk("003", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return
        }

        ShowConfirmYesNoMessage("002", SmsFunction.Add, MessageType.Confirm, MessageContext.Confirm, function () {
            var objModule = {
                StyleCode: objStyleMaster.StyleCode,
                ModuleItemCode: "",
                ModuleDesc: "",
                Registrar: $("#hdUsername").val(),
                FinalAssembly: "0",
                Confirmed: ""
            };

            AddModule(objModule, mainLevel, arrModuleLevel, objStyleMaster.Buyer, function (resAdd) {
                if (resAdd === Success) {
                    //update style group.
                    if (isEmpty(objStyleMaster.StyleGroup) || isEmpty(objStyleMaster.SubGroup) || isEmpty(objStyleMaster.SubSubGroup)) {
                        var styleGroup = $("#drpStyleGroup").val();
                        var styleSubGroup = $("#drpStyleSubGroup").val();
                        var styleSubSubGroup = $("#drpStyleSubSubGroup").val();
                        UpdateStyleGroup(objStyleMaster.StyleCode, styleGroup, styleSubGroup, styleSubSubGroup);
                    }
                }
            });
        }, function () { }, "\"Module\"");

    });
}

function ClickButtonAddModulePart() {
    $("#btnAddModulePart").click(function () {
        let mainLevel = "SUB";
        let arrModulePart = $("#drpModulePart").val();

        if ($.isEmptyObject(arrModulePart)) {
            ShowMessageOk("013", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error, "\"Part\"");
            return;
        }

        //Get style master from local storage
        var objStyleMaster = JSON.parse(localStorage.getItem(StyleMasterInfo));
        if ($.isEmptyObject(objStyleMaster.Buyer) || $.isEmptyObject(objStyleMaster.StyleCode)) {
            ShowMessageOk("003", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return
        }

        ShowConfirmYesNoMessage("002", SmsFunction.Add, MessageType.Confirm, MessageContext.Confirm, function () {
            var objModule = {
                StyleCode: objStyleMaster.StyleCode,
                ModuleItemCode: "",
                ModuleDesc: "",
                Registrar: $("#hdUsername").val(),
                FinalAssembly: "0",
                Confirmed: ""
            };

            AddModulePart(objModule, mainLevel, arrModulePart, objStyleMaster.Buyer, function (resAdd) {
                //if (resAdd === Success) {

                //}
            });
        }, function () { }, "Part(s)");

    });
}

function SelectModuleType() {
    $("#rdStandardModule, #rdDefineModule").change(function () {
        if ($("#rdStandardModule").is(":checked")) {
            $("#divStandardModule").show();
            $("#divDefineModule").hide();
        } else {
            $("#divDefineModule").show();
            $("#divStandardModule").hide();
        }
    });
}

function ClickButtonAddParts() {
    $("#btnAddPart").click(function () {
        if ($("#divPartManagement").css("display") === "none") {
            $("#divPartManagement").show();
        } else {
            $("#divPartManagement").hide();
        }
    });
}

function ClickButtonSaveComment() {
    $("#btnSaveComment").click(function () {
        ShowConfirmYesNoMessage("001", SmsFunction.Add, MessageType.Confirm, MessageContext.Confirm, function () {

            //Get module information on grid
            let gridData = jQuery(TableModuleId).jqGrid("getRowData");
            let i = 1;
            //Get part comment on gridview
            $.each(gridData, function (idx, value) {
                value.PartComment = $("#txtPartComment_" + i).val();
                //value.SubGroup = $("#drpSubGroupModule_" + i).val(); //ADD - SON) 8/Sep/2020
                value.Color = $(`#drpModuleColor_${i}`).val()
                i++;
            });

            //Save list of module comment
            updateComment(gridData, function (res) {
                if (res === Success) {
                    ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, ObjMessageType.Info);
                } else {
                    ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error);
                }
            });

        }, function () { });
    });
}

//Update module comment.
function updateComment(listModule, callBackFnc) {
    var config = ObjectConfigAjaxPost("/Ops/UpdateModuleComment", false, JSON.stringify({ listModule: listModule }));
    var res = false;
    AjaxPostCommon(config, function (respone) {
        callBackFnc(respone);
    });

    return res;
}

// #endregion
