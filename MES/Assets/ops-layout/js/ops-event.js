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

    });
}

//Neutral color
function ClickCbRegNeuColor() {
    $('#chkRegisterNeutralColor').change(function () {
        if ($(this).is(":checked")) {
            $("#txtColor").val("000");
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
        if (selOpmt) {
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

//Save a copied or an empty ops
function ClickButtonRegisterOps() {
    $("#btnRegisterOps").click(function () {
        const opmt = GetSelectedOneRowData("#gridTableOpsModal");

        if (opmt && !$.isEmptyObject(opmt)) {
            const language = MapFlagValueToLanguage($("#drpLanguageOpMaster").val());

            //The number of copies Operation Plan processes
            const opCount = parseInt($("#txtProcess").val());
            const isCopyOp = document.getElementById("rdCopySelectPlan").checked;

            //START ADD) SON - 25/Feb/2019: Count processes with standard name
            if (isCopyOp) {
                const countPro = countProcessesWithStandardName(opmt.Edition, language, opmt.StyleCode, opmt.StyleSize,
                    opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
                if (countPro < opCount) {
                    ShowConfirmYesNoMessage("003", SmsFunction.Confirm, MessageType.Confirm, MessageContext.Confirm, function () {
                        AddNewOps();
                    }, function () { }, countPro + " process(es) with standard name in " + opCount);
                } else { //Copy Operation Plan normally
                    AddNewOps();
                }
            } else { //Register an empty Operation Plan or Import processes from GSD file
                //AddNewOps();
                MsgInform("Information", "Only allow copying Operation Plan from center data.", "error", false, true);
            }
            //END ADD) SON - 25/Feb/2019
        }
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
    if ($.isEmptyObject(CurrentStyle)) {
        ShowMessageOk("003", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert);
        return;
    }
    ClearDataOnOpsModal();
    $('#chkRegisterNeutralColor').prop("checked", false);
    const msqlMaxOpRev = GetMaxOpRevision(CurrentStyle);
    const pkMesMaxOpRevNo = PkMesGetMaxOpRevNo(window.CurrentStyle);
    const maxOpRev = parseInt(msqlMaxOpRev) > parseInt(pkMesMaxOpRevNo) ? msqlMaxOpRev : pkMesMaxOpRevNo;
    $("#txtOpRevision").val(maxOpRev);
    $("#hdOpRevNoMax").val(maxOpRev);
    SetValueForStyleModal(CurrentStyle.StyleCode, CurrentStyle.StyleSize, CurrentStyle.StyleColorSerial, CurrentStyle.RevNo,
        maxOpRev);
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

function OraDeleteOpmt(opmt, callBack) {
    $.post(OraDeleteOpmtUri, { opmt }).done((response) => {
        callBack(response);
    });
}

function SyncOperationPlan(opmt) {
    OraDeleteOpmt(opmt, (response) => {
        if (response === '"success"') {
            const desOpmt = {
                StyleCode: opmt.StyleCode,
                StyleSize: opmt.StyleSize,
                StyleColorSerial: opmt.StyleColorSerial,
                RevNo: opmt.RevNo,
                OpRevNo: opmt.OpRevNo,
                Edition: opmt.Edition,
                Language: opmt.Language,
                Factory: opmt.Factory,
                MxPackage: opmt.MxPackage
            };
            //const postData = {
            //    editionReg: "M", desOpmt, sourceOpmt: opmt, isCopyTool: true, isCopyOp: true,
            //    isCopyBomPattern: false
            //};

            SyncOp(desOpmt);
            //InsertOpmtToPkMes(postData);
        }
    });
}

function SyncOp(opmt) {
    $.post(OraSyncOp, { opmt }).done((response) => {
        console.log(response);
        if (response.result) {
            console.log("Synced operation plan successfully.");
        } else {
            console.log("Failed syncing operation plan.");
        }
    });
}

var CurrentMpmt;
function ClickButtonDeleteOpsMaster() {
    $(`#${BtnDeleteOpmt}`).click(function () {
        if ($.isEmptyObject(window.CurrentOpmt)) {
            ShowMessageOk("004", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }
        const isOpenMpmt = window.CurrentMesPackage.MxStatus;
        if (isOpenMpmt !== "RO") {
            MsgInform("Error", "Cannot delete this Operation Plan because the package hasn't been opened anymore.", false, true);
            return;
        }

        CurrentMesPackage.Edition = "M";
        ShowConfirmYesNoMessage("001", SmsFunction.Delete, MessageType.Confirm, MessageContext.DeleteConfirm, () => {
            const resDel = DeleteOpsMaster(window.CurrentOpmt);
            if (resDel === true) {
                ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info);
                $(`#${BtnCreateOpmt}`).show();
                $(`#${BtnDeleteOpmt}`).hide();
                //document.getElementById("wid-op-layout").classList.add("hide");
                //document.getElementById("wid-id-mtop-tap").classList.add("hide");
                $("#wid-op-layout").hide();
                $("#wid-id-mtop-tap").hide();

                OraDeleteOpmt(window.CurrentOpmt, (response) => { console.log(response); });
            }
        }, function () { }, "\"Operation Plan\"");
    });
}

// #endregion

// #endregion

// #region Event tab Operation Plan

function InitializeOpdtModal() {
    BindDataToJqGridInputOpTimeModal([]);
    initMasterDataProcessModal();
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
        if (fileSize > 4) {
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
        GetDataForLineBalancingTab();
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

// #endregion