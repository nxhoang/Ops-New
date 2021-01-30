var StatusUpdateProcess = 1;
var ProcessModal = "mdlProcessDetail";
var LayoutPage;
var DisplayColor;
var LayoutTopY;
var LayoutLeftX;
var LayoutGroupMode;
var IsLayoutEvent;
var ModuleTypeConst = "ModuleType";
var OpGroupConst = "OpGroup";
var MachineTypeConst = "MachineType";

const _iconNoImage = '/img/ops/icon/no-image-icon.svg';
const _opGrade = 'OpGrade';
const _opGroupLevel = {
    Level_0: 0,
    Level_1: 1,
    Level_2: 2
};
const _opnSerial = {
    One: 1,
    Two: 2,
    Three: 3,
    Four: 4,
    Five: 5
}
var _addOpName = 1;
var _addPickUpOp = 1;
var _listOpnt = [];
var _listImgVideoOpnt = []; //Don't keep file data in _listOpnt because this list will be send to server when saving or updating
const _stitchingOpNameId = '3486';
var _listTempOpNameDt = [];
var _listTempLinkedBom = [];
var _listTempLinkedPattern = [];
var _listExpandedRowIdTbLinkedItem = [];
var _listExpandedRowIdTbBom = [];
var _listExpandedRowIdOpNameDt = [];
var _listExpandedRowIdSubOpNameDt = [];
var _isDragable = true;
var multiTitle = "Multiple selected";
var htmlMuilty = "<div id='dragItemID' style='z-index:9999999' class='DraggAllItem'><span class='rowdragg'>" + multiTitle + "</span></div>";
var _opep = false;

$(() => {
    eventClickButtonProcessDetail();

    eventOnChangeSelection();

    eventSelectRadioButton();

    eventSelectFileChange();

    //loadDataOnOpDetailModal();

    //let objOpmt = JSON.parse(localStorage.getItem(OpsMasterInfo));
    //if (!$.isEmptyObject(objOpmt))
    //    bindDataToJqGridBom(objOpmt.StyleCode, objOpmt.StyleSize, objOpmt.StyleColorSerial, objOpmt.RevNo, objOpmt.OpRevNo, objOpmt.Edition, false);

    //bindDataToJqGridLinkedItem('', '', '', '', '', '0', '', false);

    //init temporary grid view linking bom and pattern
    //bindDataToJqGridBomAndPatternLinking();

    ////init grid process name detail
    //bindDataToJqGridOpNameDetail('', '', '', '', '', '', '', '', false);

    //set width for select2 dropdown
    setSelect2DropdownBelowWidthList(['drpGroupShow', 'drpPickUpGrade', 'drpDisposeGrade', 'drpClassificationSub', 'drpSubClassSub', 'drpSubSubSub'], 300);
});

//#region funtions
const loadDataOnOpDetailModal = () => {
    //Get Classification
    getClassification();
    //Get grade
    getGrade();
}

const generateRefOpName = () => {
    //Set value for reference OpName  
    const pickUp = $('#drpPickUpGrade option:selected').text();
    const opName1 = $('#txtOpName1').val();
    const opName2 = $('#txtOpName2').val();
    const opName3 = $('#txtOpName3').val();
    const opName4 = $('#txtOpName4').val();
    const opName5 = $('#txtOpName5').val();
    const dispose = $('#drpDisposeGrade option:selected').text();

    let refOpName = '';
    if (!isEmpty(pickUp)) refOpName += pickUp;
    refOpName += !isEmpty(opName1) && !isEmpty(pickUp) ? ' | ' + opName1 : opName1;
    if (!isEmpty(opName2)) refOpName += ' | ' + opName2;
    if (!isEmpty(opName3)) refOpName += ' | ' + opName3;
    if (!isEmpty(opName4)) refOpName += ' | ' + opName4;
    if (!isEmpty(opName5)) refOpName += ' | ' + opName5;
    if (!isEmpty(dispose)) refOpName += ' | ' + dispose;

    $('#txtRefOpName').val(refOpName);
}

const generateOpName = (opNameTextboxId, opNameHiddenId) => {
    //If sub sub is not empty then get this selection make process name
    if (!isEmpty($(`#drpSubSubSub option:selected`).text())) {
        $(`${opNameTextboxId}`).val($(`#drpSubSubSub option:selected`).text());
        $(`${opNameHiddenId}`).val($(`#drpSubSubSub`).val());
        return;
    }

    //If sub class is not empty then get this selection make process name
    if (!isEmpty($(`#drpSubClassSub option:selected`).text())) {
        $(`${opNameTextboxId}`).val($(`#drpSubClassSub option:selected`).text());
        $(`${opNameHiddenId}`).val($(`#drpSubClassSub`).val());
        //Set value for reference OpName
        $('#txtRefOpName').val($(`#drpSubClassSub option:selected`).text());
        return;
    }

    //If classification is not empty then get this selection make process name
    if (!isEmpty($(`#drpClassificationSub option:selected`).text())) {
        $(`${opNameTextboxId}`).val($(`#drpClassificationSub option:selected`).text());
        $(`${opNameHiddenId}`).val($(`#drpClassificationSub`).val());
        //Set value for reference OpName
        $('#txtRefOpName').val($(`#drpClassificationSub option:selected`).text());
        return;
    }
}

const getOperationIcon = (opnSerial) => {
    var config = {
        url: "/PlanManagement/GetOperationIconLink",
        postData: JSON.stringify({ opNameId: $(`#drpClassificationSub`).val() }),
        async: true
    };
    AjaxPostCommon(config, function (response) {
        let iconLink = '~/img/ops/icon/no-image-icon.svg';
        if (response.IsSuccess) {
            iconLink = response.Result.IconLink;
        }
        $(`#imgOpIcon${opnSerial}`).attr("src", iconLink);
    });
}

const generateDefiningMultiOperation = () => {
    let pickUp = $('#drpPickUpGrade option:selected').text();
    let opName1 = $('#txtOpName1').val();
    let opName2 = $('#txtOpName2').val();
    let opName3 = $('#txtOpName3').val();
    let opName4 = $('#txtOpName4').val();
    let opName5 = $('#txtOpName5').val();
    let dispose = $('#drpDisposeGrade option:selected').text();

    //count operation
    let countOp = 0;

    if (!isEmpty(pickUp)) {
        pickUp = pickUp.split("-")[0];
        //Show next arrow if opname 1 is not emtpy
        if (!isEmpty(opName1) || !isEmpty(opName2) || !isEmpty(opName3) || !isEmpty(dispose)) $('#divArrow1').show();
        //Show process
        $('#lblPickUp').text(pickUp);
        $('#divPickUp').show();
        $('#divOpPickup').show();
        countOp++;
    } else {
        //Hide process
        $('#lblPickUp').val('');
        $('#divPickUp').hide();
        $('#divArrow1').hide();
        $('#divOpPickup').hide();
    }

    if (!isEmpty(opName1)) {
        opName1 = opName1.split(" ")[0];
        //if opname 2 is not empty then show arrow
        if (!isEmpty(opName2) || !isEmpty(opName3) || !isEmpty(opName4) || !isEmpty(opName5) || !isEmpty(dispose)) $('#divArrow2').show();
        //Show process
        $('#lblOpeartion1').text(opName1);
        $('#divOpeartion1').show();
        $('#divOp1').show();
        countOp++;
    } else {
        //Hide process
        $('#lblOpeartion1').text('');
        $('#divOpeartion1').hide();
        $('#divArrow2').hide();
        $('#divOp1').hide();
    }

    if (!isEmpty(opName2)) {
        opName2 = opName2.split(" ")[0];
        //if opname 3 is not empty then show arrow
        if (!isEmpty(opName3) || !isEmpty(opName4) || !isEmpty(opName5) || !isEmpty(dispose)) $('#divArrow3').show();
        //Show process
        $('#lblOpeartion2').text(opName2);
        $('#divOpeartion2').show();
        $('#divOp2').show();
        countOp++;
    } else {
        //Hide process
        $('#lblOpeartion2').text('');
        $('#divOpeartion2').hide();
        $('#divArrow3').hide();
        $('#divOp2').hide();
    }

    if (!isEmpty(opName3)) {
        opName3 = opName3.split(" ")[0];
        //if dispose is not empty then show arrow
        if (!isEmpty(opName4) || !isEmpty(opName5) || !isEmpty(dispose)) $('#divArrow4').show();
        //Show process
        $('#lblOpeartion3').text(opName3);
        $('#divOpeartion3').show();
        $('#divOp3').show();
        countOp++;
    } else {
        //Hide process
        $('#lblOpeartion3').text('');
        $('#divOpeartion3').hide();
        $('#divArrow4').hide();
        $('#divOp3').hide();
    }

    if (!isEmpty(opName4)) {
        opName4 = opName4.split(" ")[0];
        //if dispose is not empty then show arrow
        if (!isEmpty(opName5) || !isEmpty(dispose)) $('#divArrow5').show();
        //Show process
        $('#lblOpeartion4').text(opName4);
        $('#divOpeartion4').show();
        $('#divOp4').show();
        countOp++;
    } else {
        //Hide process
        $('#lblOpeartion4').text('');
        $('#divOpeartion4').hide();
        $('#divArrow5').hide();
        $('#divOp4').hide();
    }

    if (!isEmpty(opName5)) {
        opName5 = opName5.split(" ")[0];
        //if dispose is not empty then show arrow
        if (!isEmpty(dispose)) $('#divArrow6').show();
        //Show process
        $('#lblOpeartion5').text(opName5);
        $('#divOpeartion5').show();
        $('#divOp5').show();
        $('#divOpDispose').show();
        countOp++;
    } else {
        //Hide process
        $('#lblOpeartion5').text('');
        $('#divOpeartion5').hide();
        $('#divArrow6').hide();
        $('#divOp5').hide();
        $('#divOpDispose').hide();
    }

    if (!isEmpty(dispose)) {
        dispose = dispose.split("-")[0];
        //Show process
        $('#lblDispose').text(dispose);
        $('#divDispose').show();
        $('#divOpDispose').show();
        countOp++;
    } else {
        //Hide process
        $('#lblDispose').text('');
        $('#divDispose').hide();
        $('#divOpDispose').hide();
    }

    //set scale for div contain define multi operation
    //if count operation is less than 5 then do set zoom
    if (countOp > 5) {
        let zoomDiv = 1;
        switch (countOp) {
            case 5:
                zoomDiv = 0.9;
                break;
            case 6:
                zoomDiv = 0.8;
                break;
            case 7:
                zoomDiv = 0.7;
                break;
            default:
        }

        $("#divDefMulOperation").css("zoom", zoomDiv);
    }
}

const generateObjectFileOpnt = (opnSerial, isImage, fileName, fileData) => {
    //Generate object file opnt to keep image and video
    return { OpnSerial: opnSerial, IsImage: isImage, FileName: fileName, FileData: fileData };
}

const clearImageAndVideoOnOpdtModal = () => {
    //clear video
    $("#flProcessVideoSub").val("").clone(true);
    $("#vidPreviewSub").attr('src', '')
    var $source = $('#videoPreviewSub');
    $source[0].src = "";
    $source.parent()[0].load();
    $("#lblVideoNameSub").text("");

    //clear image
    $("#flProcessImageSub").val("").clone(true);
    $("#imgPreviewSub").attr("src", "../img/no-image.png");
    $("#lblImageNameSub").text("");
}

const clearDataOnProcessDtModal = () => {
    _addOpName = 1;
    _listOpnt = [];
    _listImgVideoOpnt = [];
    //Left side of modal
    //Clear process name
    $('#txtOpName1').val('');
    $('#txtOpName2').val('');
    $('#txtOpName3').val('');
    $('#txtOpName4').val('');
    $('#txtOpName5').val('');
    //Clear hidden field opname id
    $('#hdOpNameId1').val('');
    $('#hdOpNameId2').val('');
    $('#hdOpNameId3').val('');
    $('#hdOpNameId4').val('');
    $('#hdOpNameId5').val('');

    //clear hidden field icon name
    $('#hdIconName1').val('');
    $('#hdIconName2').val('');
    $('#hdIconName3').val('');
    $('#hdIconName4').val('');
    $('#hdIconName5').val('');

    $('#txtProcessNo').val('');
    $('#txtProcessNumber').val('');
    $('#drpModule').val('').trigger('change');

    //Clear machine
    $('#drpMachineOpName1').val('').trigger('change');
    $('#drpMachineOpName2').val('').trigger('change');
    $('#drpMachineOpName3').val('').trigger('change');
    $('#drpMachineOpName4').val('').trigger('change');
    $('#drpMachineOpName5').val('').trigger('change');

    //Clear grade
    $('#drpPickUpGrade').val('').trigger('change');
    $('#drpDisposeGrade').val('').trigger('change');
    $('#drpPickUpGsdCode').val('').trigger('change');
    $('#drpDisposeGsdCode').val('').trigger('change');

    $('#divOpName2').hide();
    $('#divOpName3').hide();
    $('#divOpName4').hide();
    $('#divOpName5').hide();

    //Right side of modal
    $('#txtProcessCost').val('');
    $('#txtOfferPrice').val('');
    $('#txtCostingGroup').val('');
    $('#drpProcessGroup').val('').trigger('change');
    //Out Sourcing and hotSpot
    $('#chkOutsourcing').prop('checked', false);
    $('#chkHotSpot').prop('checked', false);
    //Machine count worker and process time and max time
    $('#txtMachineCount').val('0');
    $('#txtWorker').val('0');
    $('#txtProcessTime').val('0');
    $('#txtMaxTime').val('0');

    removeImagePreview('#flProcessImage', '#imgPreview', '#lblImageName');
    removeVideoPreview('#flProcessVideo', '#vidPreview', '#videoPreview', '#lblVideoName');

    //hide all play buttons
    $('#btnPlayVideoOp1').hide();
    $('#btnPlayVideoOp2').hide();
    $('#btnPlayVideoOp3').hide();
    $('#btnPlayVideoOp4').hide();
    $('#btnPlayVideoOp5').hide();

    //Clear remarks
    $('#txtRemarks').val('');
    //Clear Iot tracking
    $("input[name=assemblyradio]").prop("checked", false);
    //main process
    $("input[name=mainProcess]").prop("checked", false);
    //select the first one make main process
    $("#rdMainProcess1").prop("checked", true);

    //Hide defining multi operation
    $('#divPickUp').hide();
    $('#divOpeartion1').hide();
    $('#divOpeartion2').hide();
    $('#divOpeartion3').hide();
    $('#divOpeartion4').hide();
    $('#divOpeartion5').hide();
    $('#divDispose').hide();

    $('#divArrow1').hide();
    $('#divArrow2').hide();
    $('#divArrow3').hide();
    $('#divArrow4').hide();
    $('#divArrow5').hide();
    $('#divArrow6').hide();

    //remove error csss
    StyleValidateAddNewProcess();
}

const clearDataOnProcessDtSubModal = () => {

    $('#txtToolMainSub').val('');
    $('#txtToolTypeSub').val('');
    $('#txtToolItemSub').val('');

    $("#drpSubSubSub").val('').change();
    $("#drpSubClassSub").val('').change();
    $("#drpClassificationSub").val('').change();

    $('#txtMachineCountSub').val('0');
    $('#txtWorkerSub').val('0');
    $('#txtProcessTimeSub').val('0');
    $('#txtMaxTimeSub').val('0');

    //clear stitching
    $('#txtStitchingLength').val('');
    $('#txtStitchesPerInch').val('');
    $('#divStitching').hide();

    clearImageAndVideoOnOpdtModal();

    //Remarks
    $('#txtRemarksSub').val('');

    //Clear Iot tracking
    $("input[name=assemblyradioSub]").prop("checked", false);
}

const clearDataOnAddingProcessModal = () => {

    $('#txtMachineCountMain').val('0');
    $('#txtWorkerMain').val('0');
    $('#txtProcessTimeMain').val('0');
    $('#txtMaxTimeMain').val('0');

    $("#drpSubSub").val('').change();
    $("#drpSubClass").val('').change();
    $("#drpClassification").val('').change();
}

const reloadSubProcessDtModal = (opnSerial) => {
    //Find process in list if it does not exist then do nothing.
    const opnt = _listOpnt.find(x => x.OpnSerial === opnSerial);
    if (!$.isEmptyObject(opnt)) {
        getOpNameLevel(opnt.OpNameId, (opnm) => {
            if (opnm.GroupLevel === _opGroupLevel.Level_0.toString()) {
                $("#drpClassificationSub").val(opnm.OpNameId).change();
                //reload stitching
                if ($("#drpClassificationSub").val() === _stitchingOpNameId) {
                    $('#txtStitchingLength').val(opnt.StitchingLength);
                    $('#txtStitchesPerInch').val(opnt.StitchesPerInch);
                    $('#divStitching').show();
                }
            } else if (opnm.GroupLevel === _opGroupLevel.Level_1.toString()) {
                $("#drpClassificationSub").val(opnm.OpNameId2).change();
                $("#drpSubClassSub").val(opnm.OpNameId).change();
            } else if (opnm.GroupLevel === _opGroupLevel.Level_2.toString()) {
                $("#drpClassificationSub").val(opnm.OpNameId3).change();
                $("#drpSubClassSub").val(opnm.OpNameId2).change();
                $("#drpSubSubSub").val(opnm.OpNameId).change();
            }
        });

        $('#txtMachineCountSub').val(opnt.MachineCount);
        $('#txtWorkerSub').val(opnt.ManCount);
        $('#txtProcessTimeSub').val(opnt.OpTime);
        $('#txtMaxTimeSub').val(opnt.MaxTime);

        //Remarks
        $('#txtRemarksSub').val(opnt.Remarks);
        //IoT tracking
        $(`input[name=assemblyradioSub][value='${opnt.IotType}']`).prop("checked", true);

        //find image and video in temporary list
        const opntImage = _listImgVideoOpnt.find(x => x.OpnSerial === opnSerial && x.IsImage === true);
        const opntVideo = _listImgVideoOpnt.find(x => x.OpnSerial === opnSerial && x.IsImage === false);
        if (StatusUpdateProcess === 1) {
            //if status is update then load image and video for previewing from http link
            //if new image is uploaded then preview new image
            if (!$.isEmptyObject(opntImage)) previewImage(opntImage.FileData, '#imgPreviewSub', '#lblImageNameSub');
            else previewImageFromHttpLink(opnt.ImageName, opnt.ImageLink, '#imgPreviewSub');

            //If new video is uploaded then preview new video otherwise preview video from http link.
            if (!$.isEmptyObject(opntVideo)) previewVideo(opntVideo.FileData, '#vidPreviewSub', '#videoPreviewSub', '#lblVideoNameSub');
            else previewVideoFromHttpLink(opnt.VideoFile, opnt.VideoLink, '#vidPreviewSub');

        } else {
            //If status is adding new process then preview image and video from temporary variable _listImgVideoOpnt
            if (!$.isEmptyObject(opntImage)) previewImage(opntImage.FileData, '#imgPreviewSub', '#lblImageNameSub');
            if (!$.isEmptyObject(opntVideo)) previewVideo(opntVideo.FileData, '#vidPreviewSub', '#videoPreviewSub', '#lblVideoNameSub');
        }
    }
}

const clearDataOnProcessDtConceptModal = () => {
    //Clear proess name
    $('#txtOpNameConcept').val('');
    $('#hdOpNameIdConcept').val('');
    //Clear process type, sub class and sub sub
    $('#drpClassificationConcept').val('').trigger('change');
    $('#drpSubClassConcept').val('').trigger('change');
    $('#drpSubSubConcept').val('').trigger('change');
    //Clear tool
    $('#txtToolMainConcept').val('');
    $('#txtToolTypeConcept').val('');
    $('#txtToolItemConcept').val('');
    //Clear time
    $('#txtMachineCountConcept').val('');
    $('#txtWorkerConcept').val('');
    $('#txtProcessTimeConcept').val('');
    $('#txtMaxTimeConcept').val('');
    //Clear image and video
    $('#flProcessImageConcept').val('');
    $("#imgPreviewConcept").attr("src", '/img/no-image.png');
    //video
    $('#flProcessVideoConcept').val('');
    $("#vidPreviewConcept").attr("src", '');
    $("#vidPreviewConcept").attr("poster", '/img/no-video.png');
    //Remarks
    $('#txtRemarksConcept').val('');
    //Clear Iot tracking
    $("input[name=assemblyradioConcept]").prop("checked", false);
}

const classificationChange = (dropdownMachineId) => {
    //Get group machine
    getGroupMachine($('#drpClassificationSub').val(), dropdownMachineId);
    //Get list sub class
    getSubClass('drpSubClassSub', _opGroupLevel.Level_1, $('#drpClassificationSub').val());
}

const subClassChange = (dropdownMachineId) => {
    getSubSub(_opGroupLevel.Level_2, $('#drpSubClassSub').val(), 'drpSubSubSub')
    //Get machine
    getMachine($('#drpSubClassSub').val(), dropdownMachineId)
}

const updateSubProcessDetail = () => {
    //update machine for each process and main process
    _listOpnt.forEach(opnt => {
        //update machine type
        opnt.MachineType = $(`#drpMachineOpName${opnt.OpnSerial}`).val();
        //update main process
        let isChecked = $(`#rdMainProcess${opnt.OpnSerial}`).is(":checked");
        opnt.MainProcess = isChecked ? '1' : '0';
    });
}

const updateOrCreateOpntObject = (opmt, opSerial, opnSerial, opNameId, machineCount, manCount, opTime, maxTime, machineType, remarks, iotType, mainProcess, stitchingLength, stitchesPerInch, imageName, videoFile, imageData, videoData, groupLevel_0, groupLevel_1, groupLevel_2) => {
    let opnt = _listOpnt.find(x => x.OpnSerial === opnSerial);
    //if process name detail is empty then adding it to the list otherwise update object process name detail
    if ($.isEmptyObject(opnt)) {
        opnt = createObjectProcessNameDetail(
            opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, opSerial
            , opNameId, opnSerial, machineCount, manCount, opTime, maxTime, machineType, remarks, iotType, mainProcess, imageName, videoFile, stitchingLength, stitchesPerInch, groupLevel_0, groupLevel_1, groupLevel_2);
        _listOpnt.push(opnt);
    } else {
        //update process name detail
        opnt.OpNameId = opNameId;
        opnt.MachineCount = machineCount;
        opnt.ManCount = manCount;
        opnt.OpTime = opTime;
        opnt.MaxTime = maxTime;
        opnt.Remarks = remarks;
        opnt.IotType = iotType;
        opnt.MainProcess = mainProcess;
        opnt.ImageName = imageName;
        opnt.VideoFile = videoFile;
        opnt.StitchingLength = stitchingLength;
        opnt.StitchesPerInch = stitchesPerInch;
        opnt.GroupLevel_0 = groupLevel_0;
        opnt.GroupLevel_1 = groupLevel_1;
        opnt.GroupLevel_2 = groupLevel_2;
    }

    //create or update object file opnt
    let imageOpnt = _listImgVideoOpnt.find(x => x.OpnSerial === opnSerial && x.IsImage === true);
    if ($.isEmptyObject(imageOpnt)) {
        //keep image and video to temporary list
        if (!$.isEmptyObject(imageData)) _listImgVideoOpnt.push(generateObjectFileOpnt(opnSerial, true, imageData.name, imageData));
    } else {
        if (!$.isEmptyObject(imageData)) {
            //If image is not empty then update image information
            fileOpnt.IsImage = true;
            fileOpnt.FileName = imageData.name;
            fileOpnt.FileData = imageData;
        } else {
            //remove image object if it is removed
            _listImgVideoOpnt = _listImgVideoOpnt.filter(function (listFilesOpnt) {
                return listFilesOpnt.IsImage !== true && listFilesOpnt.OpnSerial !== opnSerial;
            });
        }
    }

    let videoOpnt = _listImgVideoOpnt.find(x => x.OpnSerial === opnSerial && x.IsImage === false);
    if ($.isEmptyObject(videoOpnt)) {
        //keep image and video to temporary list
        if (!$.isEmptyObject(videoData)) {
            _listImgVideoOpnt.push(generateObjectFileOpnt(opnSerial, false, videoData.name, videoData));
        }
    } else {
        if (!$.isEmptyObject(videoData)) {
            //If video is not empty then updating
            fileOpnt.IsImage = false;
            fileOpnt.FileName = videoData.name;
            fileOpnt.FileData = videoData;
        }
    }
}

const updateObjOpNameDt = (opnSerial) => {
    if (opnSerial === _opnSerial.One) {
        generateProcessNameDetail(opnSerial, '#txtOpName1', '#hdOpNameId1', '#rdMainProcess1', '#divOpName2');
    } else if (opnSerial === _opnSerial.Two) {
        generateProcessNameDetail(opnSerial, '#txtOpName2', '#hdOpNameId2', '#rdMainProcess2', '#divOpName3');
    } else if (opnSerial === _opnSerial.Three) {
        generateProcessNameDetail(opnSerial, '#txtOpName3', '#hdOpNameId3', '#rdMainProcess3', '#divOpName4');
    } else if (opnSerial === _opnSerial.Four) {
        generateProcessNameDetail(opnSerial, '#txtOpName4', '#hdOpNameId4', '#rdMainProcess4', '#divOpName5');
    } else {
        generateProcessNameDetail(opnSerial, '#txtOpName5', '#hdOpNameId5', '#rdMainProcess5', '');
    }

    updateMachineWorkerOpTime()
    getOperationIcon(opnSerial);
}

const generateProcessNameDetail = (opnSerial, txtOpnameId, hdOpnameId, rdMainProcessId, divOpNameId) => {
    let opmt = GetSelectedOneRowData(gridOpsTableId);
    let iotType = $("input[name='assemblyradioSub']:checked").val();
    //if iot type is null or undifined, NaN, empty string(''), 0 or false then return empty
    if ($.isEmptyObject(iotType)) iotType = '';

    const machineCount = parseInt($('#txtMachineCountSub').val());
    const manCount = parseInt($('#txtWorkerSub').val());
    const opTime = parseInt($('#txtProcessTimeSub').val());
    const maxTime = parseInt($('#txtMaxTimeSub').val());
    const stitchingLength = parseInt($('#txtStitchingLength').val());
    const stitchesPerInch = parseInt($('#txtStitchesPerInch').val());
    const opSerial = $('#txtProcessNo').val();
    const remarks = $('#txtRemarksSub').val();
    const machineType = '';

    const imageFile = $("[id*=flProcessImageSub]").get(0).files;
    const videoFile = $("[id*=flProcessVideoSub]").get(0).files;

    const imageData = imageFile.length === 0 ? null : imageFile[0];
    const videoData = videoFile.length === 0 ? null : videoFile[0];

    const groupLevel_0 = $('#drpClassificationSub').val();
    const groupLevel_1 = $('#drpSubClassSub').val();
    const groupLevel_2 = $('#drpSubSubSub').val();

    //keep icon name in hidden field
    $(`#hdIconName${opnSerial}`).val(`${groupLevel_0}.svg`);

    //Generate process name
    generateOpName(txtOpnameId, hdOpnameId);
    //Display process name 2
    $(divOpNameId).show();
    //Generate reference opname
    generateRefOpName();

    //get main process
    let mainProcess = $(rdMainProcessId).is(":checked") ? '1' : '0';

    //update opnt if it exist or create and add it to the list if it does not exist
    updateOrCreateOpntObject(opmt, opSerial, opnSerial, $(hdOpnameId).val(), machineCount, manCount, opTime, maxTime, machineType, remarks, iotType, mainProcess, stitchingLength, stitchesPerInch, '', '', imageData, videoData, groupLevel_0, groupLevel_1, groupLevel_2);

    generateDefiningMultiOperation();
}

const updateMachineWorkerOpTime = () => {
    let totalMachine = 0;
    let totalWorker = 0;
    let totalOpTime = 0;
    let totalMaxTime = 0;
    //Calculate total optime, machine, worker and max time.
    _listOpnt.forEach(opnt => {
        totalMachine += opnt.MachineCount;
        totalWorker += opnt.ManCount;
        totalOpTime += opnt.OpTime;
        totalMaxTime += opnt.MaxTime;
    });

    $('#txtMachineCount').val(totalMachine);
    $('#txtWorker').val(totalWorker);
    $('#txtProcessTime').val(totalOpTime);
    $('#txtMaxTime').val(totalMaxTime);
}

const reselectOpdtMachine = (dropdownId, machineObj) => {
    $(dropdownId)
        .find('option')
        .remove()
        .end()
        .append($('<option>', {
            value: machineObj.MachineType,
            text: machineObj.MachineName
        }))
        .val(machineObj.MachineType)
        ;
}

const checkDataSubProcessDetail = () => {

    if (isEmpty($('#drpClassificationSub').val()) && isEmpty($('#drpSubClassSub').val()) && isEmpty($('#drpSubSubSub').val())) {
        ShowMessage('Process Detail', 'Please select classification', MessageType.Warning);
        return false;
    }

    if (isEmpty($('#txtMachineCountSub').val())) {
        ShowMessage('Process Detail', 'Please input machine count.', MessageType.Warning);
        return false;
    }
    if (isEmpty($('#txtWorkerSub').val())) {
        ShowMessage('Process Detail', 'Please input worker.', MessageType.Warning);
        return false;
    }
    if (isEmpty($('#txtProcessTimeSub').val())) {
        ShowMessage('Process Detail', 'Please input process time.', MessageType.Warning);
        return false;
    }
    if (isEmpty($('#txtMaxTimeSub').val())) {
        ShowMessage('Process Detail', 'Please input max time.', MessageType.Warning);
        return false;
    }

    return true;
}

const loadDataForSubProcess = (objOpnt) => {
    const isMainProcess = StringToBoolean(objOpnt.MainProcess);
    //show sub process icon
    let iconLink = isEmpty(objOpnt.IconLink) ? _iconNoImage : objOpnt.IconLink;
    $(`#imgOpIcon${objOpnt.OpnSerial}`).attr("src", iconLink);

    if (isEmpty(objOpnt.VideoFile)) {
        $(`#btnPlayVideoOp${objOpnt.OpnSerial}`).hide();
    } else {
        $(`#btnPlayVideoOp${objOpnt.OpnSerial}`).show();
    }

    //get icon name
    $(`#hdIconName${objOpnt.OpnSerial}`).val(`${objOpnt.GroupLevel_0}.svg`);  
  
    $(`#hdOpNameId${objOpnt.OpnSerial}`).val(objOpnt.OpNameId);
    $(`#txtOpName${objOpnt.OpnSerial}`).val(objOpnt.OpNameCode);
    $(`#rdMainProcess${objOpnt.OpnSerial}`).prop("checked", isMainProcess);

    //reload machine
    reselectOpdtMachine(`#drpMachineOpName${objOpnt.OpnSerial}`, { MachineType: objOpnt.MachineType, MachineName: objOpnt.MachineName });
}

//#region preview image and video
const readURLData = (inputData, imgPreviewId) => {
    if (inputData) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $(imgPreviewId).attr('src', e.target.result);
        }

        reader.readAsDataURL(inputData); // convert to base64 string
    }
}

const previewVideo = (fileData, VideoPreviewId, sourceVideoPreviewId, lblVideoNameId) => {
    //Clear source of video preview
    $(VideoPreviewId).removeAttr("src");
    $(VideoPreviewId).removeAttr("poster");

    //Preview video before upload
    var $source = $(sourceVideoPreviewId);
    $source[0].src = URL.createObjectURL(fileData);
    $source.parent()[0].load();

    //Get file name
    var fileName = fileData.name;
    var fileSize = ConvertByteToExpectedType(fileData.size, "MB");
    $(lblVideoNameId).text(fileName + " ( " + fileSize + " MB )");
}

const previewImage = (imageData, imgPreviewId, lblImageNameId) => {
    //const imageData = fileUploadEle.files[0];
    const fileName = imageData.name;
    const fileSize = ConvertByteToExpectedType(imageData.size, Megabyte);
    if (fileSize > 4) {
        ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
        return;
    }

    $(lblImageNameId).text(fileName + " ( " + fileSize + " MB )");

    //Preview image before upload to FTP
    //readURL(fileUploadEle, imgPreviewId);
    readURLData(imageData, imgPreviewId);
}

const previewImageFromHttpLink = (imageName, imageHttpLink, imgPreviewId) => {
    //Preview image from http link
    let imgPath = "/img/no-image.png";
    if (!isEmpty(imageName)) {
        imgPath = imageHttpLink;
    }
    $(imgPreviewId).attr("src", imgPath);
}

const previewVideoFromHttpLink = (videoName, videoHttpLink, videoPreviewId) => {
    //Preview video from http link
    let posterPath = "/img/no-video.png";
    let srtSrc = "";
    if (!isEmpty(videoName)) {
        srtSrc = videoHttpLink;
        posterPath = "";
    }
    $(videoPreviewId).attr("src", srtSrc);
    $(videoPreviewId).attr("poster", posterPath);
}

const removeImagePreview = (inputId, imgPreviewId, lblImageNameId) => {
    $(inputId).val("").clone(true);
    $(imgPreviewId).attr("src", "../img/no-image.png");
    $(lblImageNameId).text("");
}

const removeVideoPreview = (inputId, videoPreviewId, sourceVideoPreviewId, lblVideoNameId) => {
    $(inputId).val("").clone(true);
    $(videoPreviewId).attr('src', '')

    var $source = $(sourceVideoPreviewId);
    $source[0].src = "";
    $source.parent()[0].load();

    $(lblVideoNameId).text("");

    videoPreview
}

const generateSysFileNameSubProcess = (extension, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, opnSerial, edition) => {
    if (isEmpty(styleCode) || isEmpty(styleSize) || isEmpty(styleColorSerial) || isEmpty(revNo) || isEmpty(opRevNo)
        || isEmpty(opSerial) || isEmpty(opnSerial) || isEmpty(edition)) return '';

    return `${styleCode}${styleSize}${styleColorSerial}${revNo}${opRevNo}${opSerial}${opnSerial}${edition}.${extension}`;
}

const playVideoSubProcess = (opnSerial) => {
    const opnt = _listOpnt.find(x => x.OpnSerial === opnSerial);
    if (!$.isEmptyObject(opnt)) {
        //Adding video link to textbox
        const l = opnt.VideoLink.includes("pkfile") ? `${window.location.host}\\${opnt.VideoLink}` : opnt.VideoLink;
        //console.log(l);
        $("#txtVideoLink").val(l);
        $("#vdoOpsDetail").attr("src", opnt.VideoLink);
        ShowModal("mdlPlayVideo");
    } else {
        ShowMessage('Play Video', 'There is no video to play', MessageType.Warning);
    }
}
//#endregion

//#region bom & patterns linking
const checkTheNumberPatternQtyLinking = () => {

}

const getOpSerial = () => {
    let opSerial = StatusUpdateProcess === 1 ? GetSelectedOneRowData('#gridOpsDetail').OpSerial : $('#txtProcessNo').val();
    return opSerial;
}

const linkingBomAndPattern = (listLinkedItem, listLinkedPattern) => {
    var config = {
        url: "/PlanManagement/LinkingBomAndPattern",
        postData: JSON.stringify({ listLinkedItem: listLinkedItem, listLinkedPattern: listLinkedPattern }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            ShowMessage("Linking Bom & Pattern", response.Result, MessageType.Success);
        } else {
            console.log("Error save BOM & Patterns: " + response.Log);
            ShowMessage("Linking Bom & Pattern", response.Log, MessageType.Error);
        }
    });
}

const deleteBomPatternLinking = (rowData, isItem = true) => {
    var config = {
        url: "/PlanManagement/DeleteBomPatternLinking",
        postData: JSON.stringify({ prot: rowData, isItem: isItem }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            //let selOpmt = GetSelectedOneRowData('#gridOpsTable');
            //let opSerial = getOpSerial();
            //reloadGridLinkedItem(selOpmt.StyleCode, selOpmt.StyleSize, selOpmt.StyleColorSerial, selOpmt.RevNo, selOpmt.OpRevNo, opSerial, selOpmt.Edition, true);
            reloadLinkedItemAndBom(true);
            ShowMessage("Delete Bom & Pattern Linking", response.Result, MessageType.Success);
        } else {
            console.log("Delete Bom & Pattern Linking: " + response.Log);
            ShowMessage("Delete Bom & Pattern Linking", response.Log, MessageType.Error);
        }
    });
}

const checkLinkedPattern = (listPattern) => {
    let isLinked = true
    var config = {
        url: "/PlanManagement/CheckLinkedPattern",
        postData: JSON.stringify({ listPattern: listPattern }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            isLinked = true;
        } else {
            console.log("Error Linking Bom & Pattern: " + response.Log);
            ShowMessage("Linking Bom & Pattern", response.Log, MessageType.Error);
            isLinked = false;
        }
    });

    return isLinked;
}

const getExpandedRowIdsManySubGridJqGrid = (jqGridId) => {
    //get expanded row ids for grid has many sub grid in itself
    let listRowIds = [];
    //get all table rows
    let tableRows = $(`${jqGridId}`).find('> tbody > tr');
    tableRows.each(function (idx, row) {
        if (row.firstChild.className.indexOf('sgexpanded') !== -1) {
            let rowId = $(this).attr('id');
            listRowIds.push(rowId);
        }
    });

    return listRowIds;
}

const getExpandedRowIdsJqGrid = (jqGridId) => {
    let listRowIds = [];
    $(`${jqGridId} tr:has(.sgexpanded)`).each(function () {
        let rowId = $(this).attr('id');
        listRowIds.push(rowId);
    });

    return listRowIds;
}

const getExpandedRowIdsSubGridJqGrid = (jqGridId, listExpandedRowIdsParents) => {
    let listExpanedRowIdSubGrid = [];
    listExpandedRowIdsParents.forEach(parentsRowId => {
        let subGridId = `${jqGridId}_${parentsRowId}_t`;
        let subExpandedRowIds = getExpandedRowIdsManySubGridJqGrid(subGridId);
        listExpanedRowIdSubGrid.push({ SubGridId: subGridId, ListExpendedRowIds: subExpandedRowIds });
    });

    return listExpanedRowIdSubGrid;
}

const expandRowJqgrid = (jqGridId, listRowIds) => {
    for (var j = 0; j < listRowIds.length; j++) {
        $(jqGridId).jqGrid('expandSubGridRow', listRowIds[j]);
    }
}

const expandRowSubJqgrid = (subGridIds) => {
    subGridIds.forEach(subGrid => {
        subGrid.ListExpendedRowIds.forEach(rowId => {
            $(subGrid.SubGridId).jqGrid('expandSubGridRow', rowId);
        });
    });
}

const reloadLinkedItemAndBom = (isLinking) => {
    //get list current expanded row
    _listExpandedRowIdTbLinkedItem = getExpandedRowIdsJqGrid('#tbLinkedItem');
    _listExpandedRowIdTbBom = getExpandedRowIdsJqGrid('#tbBom');

    let opSerial = getOpSerial();
    let selOpmt = GetSelectedOneRowData('#gridOpsTable');
    //reload grid linked item
    reloadGridLinkedItem(selOpmt.StyleCode, selOpmt.StyleSize, selOpmt.StyleColorSerial, selOpmt.RevNo, selOpmt.OpRevNo, opSerial, selOpmt.Edition, isLinking);
    //reload grid bom
    reloadGridBom(selOpmt.StyleCode, selOpmt.StyleSize, selOpmt.StyleColorSerial, selOpmt.RevNo, selOpmt.OpRevNo, selOpmt.Edition, isLinking);
    //reload linked bom
    //reloadGridLinkedBom(selOpmt.StyleCode, selOpmt.StyleSize, selOpmt.StyleColorSerial, selOpmt.RevNo, selOpmt.OpRevNo, opSerial, selOpmt.Edition, localStorage.getItem(LanguageId), isLinking);
}

const reloadDataGridviewTbBomPatternLinking = () => {
    //get selected row in sub gridview
    _listTempLinkedPattern = getSelectedRowsSubGrid('#tbBom');
    //get selected row in gridview
    let selectedRowBom = GetSelectedMultipleRowsData('#tbBom');

    //get edition from selected op master
    const selOpMt = GetSelectedOneRowData('#gridOpsTable');
    let opSerial = getOpSerial();

    const gridData = jQuery('#tbBom').jqGrid("getRowData");
    //set selectedRowBom with empty array if it is null
    if (selectedRowBom === null) {
        selectedRowBom = [];
    }
    else {
        //update has pattern to N, and set pattern serial is 000
        selectedRowBom.forEach((bom) => {
            bom.HasPattern = 'N';
            bom.PatternSerial = '000';
            bom.OpType = "I";
            bom.OpRevNo = selOpMt.OpRevNo;
            bom.Edition = selOpMt.Edition;
            bom.OpSerial = opSerial;
        });
    }

    //get item of linked patterns
    _listTempLinkedPattern.forEach((pt) => {
        //add total piece qty
        pt.TotalPieceQty = pt.PieceQty;
        pt.OpRevNo = selOpMt.OpRevNo;
        pt.Edition = selOpMt.Edition;
        pt.OpType = "I";
        pt.OpSerial = opSerial;
        //find patterns of pattern
        let item = selectedRowBom.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
        //if pattern doesn't have parents then get it from BOM
        if (item === null || typeof (item) === "undefined") {
            let parentsItem = gridData.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
            parentsItem.HasPattern = 'Y';
            selectedRowBom.push(parentsItem);
        } else {
            item.HasPattern = 'Y';
        }
    });

    if (!checkLinkedPattern(_listTempLinkedPattern)) return;

    ShowModal('mdlBomPatternLinking');
    //reload bom and pattern linking modal
    ReloadJqGridLocal('tbBomPatternLinking', selectedRowBom);
}

const reloadDataGridviewTbLinkedItemAfterLinking = () => {
    let opSerial = getOpSerial();
    let listLinkedPattern = getSubGridJqGrid('#tbBomPatternLinking');
    let listLinkedItem = GetAllRowsDataJqGrid('#tbBomPatternLinking');

    //update opserial for item and pattern linking
    listLinkedItem.forEach(item => { item.OpSerial = opSerial; });
    listLinkedPattern.forEach(pt => { pt.OpSerial = opSerial });
    //checking pattern quantity whether greater than remain quantity or not
    let isValidPieceQty = true;
    let pieceName = '';
    listLinkedPattern.forEach(pt => {
        if (pt.PieceQty > pt.PieceQtyRest) {
            isValidPieceQty = false;
            pieceName = pt.Piece;
            return false;
        }
    });
    if (!isValidPieceQty) {
        ShowMessage("Linking Bom & Pattern", `Please input a valid quantity for pattern: ${pieceName}`, MessageType.Error);
        return;
    }

    linkingBomAndPattern(listLinkedItem, listLinkedPattern);

    HideModal('mdlBomPatternLinking');
    reloadLinkedItemAndBom(true);
}

const saveBomAndPatternsLinking = () => {
    const opSerial = getOpSerial();
    const selOpmt = GetSelectedOneRowData('#gridOpsTable');

    var config = {
        url: "/PlanManagement/SaveLinkingBomPattern",
        postData: JSON.stringify({
            styleCode: selOpmt.StyleCode,
            styleSize: selOpmt.StyleSize,
            styleColorSerial: selOpmt.StyleColorSerial,
            revNo: selOpmt.RevNo,
            opRevNo: selOpmt.OpRevNo,
            opSerial: opSerial,
            edition: selOpmt.Edition
        }),
        async: false
    };
    AjaxPostCommon(config, function (response) {
        if (response.IsSuccess) {
            reloadLinkedItemAndBom(false);
            ShowMessage("Save Bom & Pattern", response.Result, MessageType.Success);
        } else {
            console.log("Error save BOM & Patterns: " + response.Log);
            ShowMessage("Save Bom & Pattern", response.Log, MessageType.Error);
        }
    });
}
//#endregion

//#endregion

//#region bind data to grid linking patterns 
const bindDataToJqGridBom = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, isLinking) => {
    jQuery("#tbBom").jqGrid({
        url: '/PlanManagement/GetBomt',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            opRevNo: opRevNo,
            edition: edition,
            isLinking: isLinking
        },
        datatype: "json",
        height: 400,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        //gridview: true,
        multiselect: true,
        caption: "BOM & Patterns",
        colModel: [
            { name: 'MainItemCode', index: 'MainItemCode', label: "Main Item", width: 150, classes: 'pointer' },
            { name: 'MainItemName', index: 'MainItemName', label: "Main Item Name", width: 150, classes: 'pointer', hidden: true },
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 250, classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'ItemColorWays', index: 'ItemColorWays', label: "Item Color", width: 150, classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", width: 100, align: 'center', classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Cons.Unit", width: 70, align: 'center', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'Qty', index: 'Qty', label: "Qty", width: 50, align: 'center', classes: 'pointer', search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'RegistryDate', index: 'RegistryDate', label: "Registry Date", width: 100, align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" }, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }
        ],
        afterInsertRow: function (rowid, rowdata) {
            $("#" + rowid, "#tbBom").addClass("Itemrow-draggable");
        },
        gridComplete: function () {
            let ids = jQuery("#tbBom").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbBom").jqGrid("getRowData", i);
                if (rowdata.HasPattern !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbBom_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbBom_subgrid]").unbind('click');
                }
            }

            //expand row
            expandRowJqgrid('#tbBom', _listExpandedRowIdTbBom);

            DragRow();
        },
        onSelectRow: function (rowid) {
            //Get row data
            //var rowdata = $(tableMesPackageId).jqGrid("getRowData", rowid);
        },
        subGrid: true,
        subGridOptions: {
            plusicon: "ui-icon-plus",
            minusicon: "ui-icon-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: true
        },
        subGridRowExpanded: subGridviewPattern
    });

    //enable filter tool bar
    jQuery("#tbBom").jqGrid("filterToolbar", {
        stringResult: true, searchOnEnter: false,
        defaultSearch: "cn", ignoreCase: true, enableCstringResult: true, autoencode: false
    });
}

const subGridviewPattern = (subgridDivId, rowId) => {
    var rowData = $('#tbBom').jqGrid('getRowData', rowId);
    var subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/PlanManagement/GetPatterns',
        postData: {
            styleCode: rowData.StyleCode,
            styleSize: rowData.StyleSize,
            styleColorSerial: rowData.StyleColorSerial,
            revNo: rowData.RevNo,
            itemCode: rowData.ItemCode,
            itemColorSerial: rowData.ItemColorSerial,
            mainItemCode: rowData.MainItemCode,
            mainItemColorSerial: rowData.MainItemColorSerial
        },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        //gridview: true,
        multiselect: true,
        colModel: [
            //{ name: "Status", index: "Status", label: "Status" },
            { name: 'Status', index: 'Status', label: "Status", formatter: formatterPatternStatus },
            {
                label: " ",
                name: "Url",
                index: "Url",
                align: "center",
                width: 120,
                formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return `<img id='${id}' class='imgpattern' onclick = showPatternImage('${cellvalue}'); src='${cellvalue}' onerror='imgError(this);'/>`;
                    return "";
                }
            },
            { name: "Piece", index: "Piece", label: "Piece", width: 300 },
            { name: "Width", index: "Width", label: "Width", align: "center", width: 50 },
            { name: "Height", index: "Height", label: "Height", align: "center", width: 50 },
            { name: "EndWise", index: "EndWise", label: "End Wise", align: "center", width: 80 },
            { name: "PieceQty", index: "PieceQty", label: "Qty", width: 50 },
            { name: "PieceQtyRest", index: "PieceQtyRest", label: "Remain Qty", width: 100 },
            { name: "UnitConsumption", index: "UnitConsumption", label: "Unit.Cons", width: 80 },
            { name: "ConsumpUnit", index: "ConsumpUnit", label: "Cons.Unit", width: 80 },
            { name: "PatternSerial", index: "PatternSerial", label: "Serial", width: 80 },
            { name: "PieceUnique", index: "PieceUnique", label: "Unique", align: "center", width: 80 },
            { name: "ItemCode", index: "ItemCode", hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "MainItemCode", index: "MainItemCode", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true }
        ],
        afterInsertRow: function (rowid, rowdata) {
            if (rowdata.Status !== 'Linked' || (rowdata.Status === 'Linked' && rowdata.PieceQtyRest !== 0)) {
                $("#" + subgridTableId).find('tr[id="' + rowid + '"]').addClass("Child-draggable");
            } else {
                $("#jqg_" + subgridTableId + "_" + rowid).prop("checked", false).attr("disabled", true);
                $("#jqg_" + subgridTableId + "_" + rowid).parent().parent().find('td[aria-describedby="' + subgridTableId + '_Status"]').addClass("Linked");
            }
        },
        beforeSelectRow: function (rowid, e) {
            let rowData = $("#" + subgridTableId).jqGrid("getRowData", rowid);
            if (rowData.PieceQtyRest === "0") return false;
            return true;
        },
        gridComplete: function () {
            DraggChil();
        }
    });
}

const bindDataToJqGridLinkedItem = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, isLinking) => {
    jQuery("#tbLinkedItem").jqGrid({
        url: '/PlanManagement/GetLinkedItems',
        postData: {
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            opRevNo: opRevNo,
            opSerial: opSerial,
            edition: edition,
            isLinking: isLinking
        },
        datatype: "json",
        height: 150,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        gridview: true,
        //multiselect: true,
        caption: "Linked Items",
        colModel: [
            { name: 'DeleteItemEle', index: 'DeleteItemEle', label: " ", width: 50, align: "center", formatter: formatterDeleteItem },
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer' },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 250, classes: 'pointer' },
            { name: 'ItemColorWays', index: 'ItemColorWays', label: "Item Color", width: 150, classes: 'pointer' },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", width: 100, align: 'center', classes: 'pointer' },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Cons.Unit", width: 70, align: 'center' },
            { name: 'MainItemCode', index: 'MainItemCode', hidden: true },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'PatternSerial', index: 'PatternSerial', hidden: true },
            { name: 'OpType', index: 'OpType', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }
        ],
        gridComplete: function () {
            let ids = jQuery("#tbLinkedItem").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbLinkedItem").jqGrid("getRowData", i);
                if (rowdata.HasPattern !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbLinkedItem_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbLinkedItem_subgrid]").unbind('click');
                }
            }

            //expand row
            expandRowJqgrid('#tbLinkedItem', _listExpandedRowIdTbLinkedItem)
        },
        loadComplete: function () {

        },
        onSelectRow: function (rowid) {
            //Get row data
            //var rowdata = $(tableMesPackageId).jqGrid("getRowData", rowid);
        },
        subGrid: true,
        subGridOptions: {
            plusicon: "ui-icon-plus",
            minusicon: "ui-icon-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: true
        },
        subGridRowExpanded: subGridviewLinkedPattern
    });
}

const formatterDeleteItem = (cellValue, option, rowData) => {
    const { OpSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial } = rowData;
    return `<a id='ach_${option.gid}_${option.rowId}' onclick='deleteBomPatternLinking(${JSON.stringify({ OpSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial })}, ${true})'>X</a>`;
}

const formatterDeletePattern = (cellValue, option, rowData) => {
    const { OpSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial } = rowData;
    return `<a id='ach_${option.gid}_${option.rowId}' onclick='deleteBomPatternLinking(${JSON.stringify({ OpSerial, ItemCode, ItemColorSerial, MainItemCode, MainItemColorSerial, OpType, Edition, PatternSerial })}, ${false})'>X</a>`;
}

const formatterPatternStatus = (cellValue, option, rowData) => {
    //let bgCellColor = rowData.Status === 'Linked'? 'yellow':'none';
    let bgCellColor = rowData.PieceQtyRest === 0 ? 'yellow' : 'none';
    return `<span style="background-color: ${bgCellColor};margin-right:-2px; margin-left:-2px;">${cellValue}</span>`;
}

const subGridviewLinkedPattern = (subgridDivId, rowId) => {
    //get current selected linked item row data
    let rowData = $('#tbLinkedItem').jqGrid('getRowData', rowId);
    //clear delete element
    rowData.DeleteItemEle = '';
    let subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        url: '/PlanManagement/GetLinkedPatterns',
        postData: { objProt: JSON.stringify(rowData) },
        datatype: "json",
        height: 300,
        width: null,
        shrinkToFit: false,
        viewrecords: false,
        rowNum: -1, //Show all rows
        rownumbers: false,
        gridview: true,
        //multiselect: true,
        colModel: [
            { name: 'DeletePatternEle', index: 'DeletePatternEle', label: " ", width: 50, align: "center", formatter: formatterDeletePattern },
            {
                label: "",
                name: "Url",
                index: "Url",
                align: "center",
                width: 120,
                formatter: function (cellvalue, options) {
                    var id = options.rowId;
                    if (cellvalue)
                        return `<img id='${id}' class='imgpattern' onclick = showPatternImage('${cellvalue}'); src='${cellvalue}' onerror='imgError(this);'/>`;
                    return "";
                }
            },
            { name: "Piece", index: "Piece", label: "Piece", width: 300 },
            { name: "Width", index: "Width", label: "Width", align: "center", width: 50 },
            { name: "Height", index: "Height", label: "Height", align: "center", width: 50 },
            { name: "EndWise", index: "EndWise", label: "End Wise", align: "center", width: 80 },
            { name: "PieceQty", index: "PieceQty", label: "Qty", width: 50 },
            { name: "ConsumpUnit", index: "ConsumpUnit", label: "Cons.Unit", width: 80 },
            { name: "PatternSerial", index: "PatternSerial", label: "Serial", width: 80 },
            { name: "PieceUnique", index: "PieceUnique", label: "Unique", align: "center", width: 80 },
            { name: "ItemCode", index: "ItemCode", hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "MainItemCode", index: "MainItemCode", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'OpType', index: 'OpType', hidden: true }
        ]
    });
}

const bindDataToJqGridBomAndPatternLinking = () => {
    jQuery("#tbBomPatternLinking").jqGrid({
        datatype: "local",
        loadonce: true,
        height: 250,
        width: null,
        shrinkToFit: false,
        rowNum: 10000,
        colModel: [
            { name: 'MainItemCode', index: 'MainItemCode', label: "Main Item Code", width: 150, classes: 'pointer' },
            { name: 'MainItemName', index: 'MainItemName', label: "Main Item Name", width: 150, classes: 'pointer', hidden: true },
            { name: 'ItemCode', index: 'ItemCode', label: "Item Code", classes: 'pointer' },
            { name: 'ItemName', index: 'ItemName', label: "Item Name", width: 300, classes: 'pointer' },
            { name: 'ItemColorWays', index: 'ItemColorWays', label: "Item Color", width: 300, classes: 'pointer', },
            { name: 'UnitConsumption', index: 'UnitConsumption', label: "Purchase Cons", width: 150, align: 'center', classes: 'pointer' },
            { name: 'ConsumpUnit', index: 'ConsumpUnit', label: "Cons.Unit", width: 150, align: 'center' },
            { name: 'Qty', index: 'Qty', label: "Qty", width: 50, align: 'center', classes: 'pointer', hidden: true },
            { name: 'RegistryDate', index: 'RegistryDate', label: "Registry Date", width: 100, align: 'center', classes: 'pointer', formatter: "date", formatoptions: { srcformat: "Y-m-d H:i:s", newformat: "d-m-Y H:i:s" }, hidden: true },
            { name: 'MainItemColorSerial', index: 'MainItemColorSerial', hidden: true },
            { name: 'ItemColorSerial', index: 'ItemColorSerial', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'OpnSerial', index: 'OpnSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'OpType', index: 'OpType', hidden: true },
            { name: 'PatternSerial', index: 'PatternSerial', hidden: true },
            { name: 'HasPattern', index: 'HasPattern', hidden: true }
        ],
        gridComplete: function () {
            let ids = jQuery("#tbBomPatternLinking").jqGrid('getDataIDs');
            for (let i = 1; i <= ids.length; i++) {
                let rowdata = $("#tbBomPatternLinking").jqGrid("getRowData", i);
                if (rowdata.HasPattern !== "Y") {
                    //Hide plus icon if item has no pattern
                    $("tr[id=" + i + "]>td[aria-describedby$=tbBomPatternLinking_subgrid]").html("&nbsp;");

                    //Disable click event on the first column
                    $("tr[id=" + i + "]>td[aria-describedby$=tbBomPatternLinking_subgrid]").unbind('click');
                } else {
                    //click on plus icon to expand subgrid
                    $("tr[id=" + i + "]>td[aria-describedby$=tbBomPatternLinking_subgrid]").click();
                }
            }
        },
        subGrid: true,
        subGridOptions: {
            plusicon: "ui-icon-plus",
            minusicon: "ui-icon-minus",
            openicon: "ui-icon-carat-1-sw",
            expandOnLoad: false,
            selectOnExpand: false,
            reloadOnExpand: false
        },
        subGridRowExpanded: subGridBomPatternLinking
    });
}

const subGridBomPatternLinking = (subgridDivId, rowId) => {
    var rowData = $('#tbBomPatternLinking').jqGrid('getRowData', rowId);
    var subgridTableId = subgridDivId + "_t";
    $("#" + subgridDivId).html("<table id='" + subgridTableId + "' class='scroll'></table>");
    $("#" + subgridTableId).jqGrid({
        datatype: "local",
        data: getPatternsInTemporary(rowData),
        height: 250,
        width: null,
        shrinkToFit: false,
        rowNum: 10000,
        colModel: [
            { label: "", name: "Url", index: "Url", align: "center", width: 120 },
            { name: "Piece", index: "Piece", label: "Piece", width: 300 },
            { name: "Width", index: "Width", label: "Width", align: "center", width: 50 },
            { name: "Height", index: "Height", label: "Height", align: "center", width: 50 },
            { name: "EndWise", index: "EndWise", label: "End Wise", align: "center", width: 80 },
            { name: "PieceQty", index: "PieceQty", label: "Piece Qty", width: 100 },
            {
                name: "PieceQtyRestEle", index: "PieceQtyRestEle", label: "Qty", width: 150,
                formatter: function (cellValue, option, rowData) {
                    return '<input type="text" id="' + option.gid + '_txtPieceQty_' + option.rowId + '" class="form-control" maxlength="3" ' +
                        'onkeypress = "return isNumber(event)" value= "' + rowData.PieceQtyRest + '" />';
                }
            },
            { name: "PieceQtyRest", index: "PieceQtyRest", label: "Remain Qty", width: 100 },
            { name: "UnitConsumption", index: "UnitConsumption", label: "Unit.Cons", width: 80 },
            { name: "ConsumpUnit", index: "ConsumpUnit", label: "Cons.Unit", width: 80 },
            { name: "PatternSerial", index: "PatternSerial", label: "Serial", width: 80 },
            { name: "PieceUnique", index: "PieceUnique", label: "Unique", align: "center", width: 80 },
            { name: "PieceQtyTxtId", index: "PieceQtyTxtId", hidden: true, formatter: function (cellValue, option) { return `${option.gid}_txtPieceQty_${option.rowId}` } },
            { name: "TotalPieceQty", index: "TotalPieceQty", hidden: true },
            { name: "ItemCode", index: "ItemCode", hidden: true },
            { name: "ItemColorSerial", index: "ItemColorSerial", hidden: true },
            { name: "MainItemCode", index: "MainItemCode", hidden: true },
            { name: "MainItemColorSerial", index: "MainItemColorSerial", hidden: true },
            { name: 'StyleCode', index: 'StyleCode', hidden: true },
            { name: 'StyleSize', index: 'StyleSize', hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'RevNo', index: 'RevNo', hidden: true },
            { name: 'OpRevNo', index: 'OpRevNo', hidden: true },
            { name: 'OpSerial', index: 'OpSerial', hidden: true },
            { name: 'OpnSerial', index: 'OpnSerial', hidden: true },
            { name: 'Edition', index: 'Edition', hidden: true },
            { name: 'OpType', index: 'OpType', hidden: true }
        ]
    });
};

const getPatternsInTemporary = (rowData) => {
    //get linked pattern in temporary list
    return _listTempLinkedPattern.filter(x => x.ItemCode === rowData.ItemCode && x.ItemColorSerial === rowData.ItemColorSerial && x.MainItemCode === rowData.MainItemCode && x.MainItemColorSerial === rowData.MainItemColorSerial)
}

function showPatternImage(url) {
    //$("#modalImage").modal("show");
    //$("#modalShowimage").attr("src", url);
    //setTimeout(function () {
    //    $("#modalImage").modal("hide");
    //}, 6000);
}

const reloadGridLinkedItem = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, isLinking) => {
    //reload gridview BOM
    var data = {
        styleCode: styleCode,
        styleSize: styleSize,
        styleColorSerial: styleColorSerial,
        revNo: revNo,
        opRevNo: opRevNo,
        opSerial: opSerial,
        edition: edition,
        isLinking: isLinking
    };
    ReloadJqGrid2LoCal("tbLinkedItem", data);
}

const reloadGridBom = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, isLinking) => {
    //reload gridview BOM
    var data = {
        styleCode: styleCode,
        styleSize: styleSize,
        styleColor: styleColorSerial,
        revNo: revNo,
        opRevNo: opRevNo,
        edition: edition,
        isLinking: isLinking
    };
    ReloadJqGrid2LoCal("tbBom", data);

}
//#endregion

//#region get data from screen
const createObjectProcessNameDetail = (edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, opNameId, opnSerial, machineCount, manCount, opTime, maxTime, machineType, remarks, iotType, mainProcess, imageName, videoFile, stitchingLength, stitchesPerInch, groupLevel_0, groupLevel_1, groupLevel_2) => {
    return opnt = {
        StyleCode: styleCode,
        StyleSize: styleSize,
        StyleColorSerial: styleColorSerial,
        RevNo: revNo,
        OpRevNo: opRevNo,
        OpSerial: opSerial,
        OpNameId: opNameId,
        Edition: edition,
        OpnSerial: opnSerial,
        MachineCount: machineCount,
        ManCount: manCount,
        OpTime: opTime,
        MaxTime: maxTime,
        MachineType: machineType,
        Remarks: remarks,
        IotType: iotType,
        MainProcess: mainProcess,
        ImageName: imageName,
        VideoFile: videoFile,
        StitchingLength: stitchingLength,
        StitchesPerInch: stitchesPerInch,
        GroupLevel_0: groupLevel_0,
        GroupLevel_1: groupLevel_1,
        GroupLevel_2: groupLevel_2
    };
}

//Create object process name detail for process pickup and dispose
const createObjOpNameDtPickUpAndDispose = (opmt, opnSerial) => {
    let iotType = $("input[name='assemblyradioConcept']:checked").val();
    //if iot type is null or undifined, NaN, empty string(''), 0 or false then return empty
    if ($.isEmptyObject(iotType)) iotType = '';
    return createObjectProcessNameDetail(
        opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, ''
        , $('#hdOpNameIdConcept').val(), opnSerial, $('#txtMachineCountConcept').val(), $('#txtWorkerConcept').val(), $('#txtProcessTimeConcept').val()
        , $('#txtMaxTimeConcept').val(), $('#drpMachineConcept').val(), $('#txtRemarksConcept').val(), iotType, '', '', ''
    );
}

const getMachinesList = (opMaster) => {
    let styleCode = opMaster.StyleCode;
    let styleSize = opMaster.StyleSize;
    let styleColorSerial = opMaster.StyleColorSerial;
    let revNo = opMaster.RevNo;
    let opRevNo = opMaster.OpRevNo;
    let opSerial = $('#txtProcessNo').val();
    let edition = opMaster.Edition;
    let isMachine = '1';

    const machine1 = $('#drpMachineOpName1').val();
    const machine2 = $('#drpMachineOpName2').val();
    const machine3 = $('#drpMachineOpName3').val();
    //Declare empty list machine
    let listMachines = [];
    _listOpnt.forEach(opnt => {
        if (opnt.OpnSerial === _opnSerial.One && !$.isEmptyObject(machine1)) {
            //create machine object
            let optl = generateMachineToolObject(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, machine1, isMachine, '1', edition, _opnSerial.One);
            listMachines.push(optl);
        } else if (opnt.OpnSerial === _opnSerial.Two && !$.isEmptyObject(machine2)) {
            //create machine object
            let optl = generateMachineToolObject(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, machine2, isMachine, '0', edition, _opnSerial.Two);
            listMachines.push(optl);
        } else if (opnt.OpnSerial === _opnSerial.Three && !$.isEmptyObject(machine3)) {
            //create machine object
            let optl = generateMachineToolObject(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, machine3, isMachine, '0', edition, _opnSerial.Three);
            listMachines.push(optl);
        }
    });

    return listMachines;
}

const generateMachineToolObject = (styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, itemCode, isMachine, isMainMachine, edition, opnSerial) => {
    var objMachine = {
        StyleCode: styleCode,
        StyleSize: styleSize,
        StyleColorSerial: styleColorSerial,
        RevNo: revNo,
        OpRevNo: opRevNo,
        OpSerial: opSerial,
        ItemCode: itemCode,
        Machine: isMachine,
        MainTool: isMainMachine,
        Edition: edition,
        OpnSerial: opnSerial
    };

    return objMachine;
}
//#endregion

//#region get data from API

const getMachine = (opNameId, mchDropdownId) => {
    var config = {
        url: "/PlanManagement/GetOpNameHasMachine",
        postData: JSON.stringify({ opNameId: opNameId }),
        async: true
    };
    AjaxPostCommon(config, function (response) {
        fillDataToDropdownlistAsync(mchDropdownId, response, "MachineId", "ItemName").then(() => {
            if (response.length > 0) {
                $('#' + mchDropdownId).val(response[0].MachineId).trigger('change');
            }
        });
    });
}

const getGroupMachine = (opNameId, mchDropdownId) => {
    var config = {
        url: "/OpName/GetMachineCategories",
        postData: JSON.stringify({ opNameId: opNameId }),
        async: true
    };
    AjaxPostCommon(config, function (response) {
        FillDataToDropDownlist(mchDropdownId, response, "MchGroupId", "MchGroupName");
    });
}

const getClassification = () => {
    var config = {
        url: "/PlanManagement/GetOperationGroup",
        postData: JSON.stringify({ groupLevel: _opGroupLevel.Level_0, parentId: '' }),
        async: true
    };
    AjaxPostCommon(config, function (response) {
        FillDataToDropDownlist("drpClassificationSub", response, "OpNameId", "English");
    });
}

const getSubClass = (idDropdownSubClass, groupLevel, classificationVal) => {
    var opToolConfig = {
        url: "/PlanManagement/GetOperationGroup",
        postData: JSON.stringify({ groupLevel: groupLevel, parentId: classificationVal }),
        async: false
    };
    AjaxPostCommon(opToolConfig, function (response) {
        FillDataToDropDownlist(idDropdownSubClass, response, "OpNameId", "English");
    });
}

const getSubSub = (groupLevel, parentId, idDropdownSubSub) => {
    var opToolConfig = {
        url: "/PlanManagement/GetOperationGroup",
        postData: JSON.stringify({ groupLevel: groupLevel, parentId: parentId }),
        async: false
    };
    AjaxPostCommon(opToolConfig, function (response) {
        FillDataToDropDownlist(idDropdownSubSub, response, "OpNameId", "English");
    });
}

const getOpNameLevel = (opNameId, callBack) => {
    var config = {
        url: "/PlanManagement/GetOpNameLevel",
        postData: JSON.stringify({ opNameId: opNameId }),
        async: true
    };
    AjaxPostCommon(config, function (response) {
        callBack(response);
    });
}

const getGrade = () => {
    //const listMCode = GetMasterCode(_opGrade);
    var opToolConfig = {
        url: "/PlanManagement/GetOpGrade",
        postData: JSON.stringify({ mCode: _opGrade }),
        async: false
    };
    AjaxPostCommon(opToolConfig, function (response) {
        FillDataToDropDownlist("drpPickUpGrade", response, "SubCode", "CodeName");
        FillDataToDropDownlist("drpDisposeGrade", response, "SubCode", "CodeName");
    });
}

//#endregion 

//#region post data through API
const uploadSubProcessImage = (fileData, opnt) => {
    //Check file length
    if (fileData.length === 0) return Success;

    const extFile = GetExtensionFileName(fileData.name);

    //Generate system file name
    const sysFileName = generateSysFileNameSubProcess(extFile, opnt.StyleCode, opnt.StyleSize, opnt.StyleColorSerial, opnt.RevNo, opnt.OpRevNo, ZeroPad(opnt.OpSerial, 3), opnt.OpnSerial, opnt.Edition);

    var data = new FormData();
    data.append(fileData.name, fileData);
    data.append("SysFileName", sysFileName);

    var uploadImage = new Promise((resolve, reject) => {
        $.ajax({
            url: "/PlanManagement/UploadImageSubProcess",
            async: false,
            type: "POST",
            data: data,
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.IsSuccess) resolve(result.Result);
                else reject(result.Log);
            },
            error: function () {
                reject(result.Log);
            }
        });
    });

    return uploadImage;
}

const upoadListImageOpnt = async () => {

    console.log('start upload image opnt');
    //upload image sub process
    let statusUploadImgSubPro = true;
    //Filter list image
    const opntImage = _listImgVideoOpnt.filter(x => x.IsImage === true);
    await opntImage.forEach(async (opntFile) => {
        let opnt = _listOpnt.find(x => x.OpnSerial === opntFile.OpnSerial);

        uploadSubProcessImage(opntFile.FileData, opnt).then(result => {
            opnt.ImageName = result;
            statusUploadImgSubPro = true;
            console.log('uploaded: ' + result);
        }).catch(log => {
            statusUploadImgSubPro = false;
            console.log(`Upload image sub process ${log}`);
            ShowMessage('Add New Process', log, MessageType.Error);
            return false;
        });
    });

    console.log('uploaded image opnt');

    return new Promise(resolve => { resolve(statusUploadImgSubPro) });
}

const uploadListVideoOpnt = async (opmt) => {
    console.log('start upload video sub process');
    //upload video sub process.
    //Get list video
    let resUploadVideo = Fail;
    const opntVideos = _listImgVideoOpnt.filter(x => x.IsImage === false);
    await opntVideos.forEach(video => {
        let opnt = _listOpnt.find(x => x.OpnSerial === video.OpnSerial);
        //Generate system file name
        const sysFileName = generateSysFileNameSubProcess(GetExtensionFileName(video.FileData.name), opnt.StyleCode, opnt.StyleSize, opnt.StyleColorSerial, opnt.RevNo, opnt.OpRevNo, ZeroPad(opnt.OpSerial, 3), opnt.OpnSerial, opnt.Edition);

        resUploadVideo = UploadVideoOpnt_New(video.FileData, sysFileName, opmt)
        if (resUploadVideo === Fail) {
            return Fail;
        }
        else {
            console.log('uploaded: ' + resUploadVideo);
            opnt.VideoFile = resUploadVideo;
        }
    });
    console.log('end upload video sub process');
}

const deleteImageOrVideoSubProcess = (opnSerial, isImage) => {
    const opnt = _listOpnt.find(x => x.OpnSerial === opnSerial);
    if (!$.isEmptyObject(opnt) && !$.isEmptyObject(opnt.VideoFile)) {
        ShowConfirmYesNoMessage("001", SmsFunction.Delete, MessageType.Confirm, MessageContext.DeleteConfirm, function () {
            //Update video name.
            var config = ObjectConfigAjaxPost("/PlanManagement/DeleteImageOrVideoSubProcess", false, JSON.stringify({
                styleCode: opnt.StyleCode,
                styleSize: opnt.StyleSize,
                styleColorSerial: opnt.StyleColorSerial,
                revNo: opnt.RevNo,
                opRevNo: opnt.OpRevNo,
                opSerial: opnt.OpSerial,
                opNameId: opnt.OpNameId,
                edition: opnt.Edition,
                isImage: isImage
            }));
            AjaxPostCommon(config, function (respone) {
                if (respone.IsSuccess) {
                    //remove video preview
                    removeVideoPreview('#flProcessVideoSub', '#vidPreviewSub', '#videoPreviewSub', '#lblVideoNameSub');

                    //Reload operation master gird
                    var opMaster = GetSelectedOneRowData(gridOpsTableId);
                    opMaster.Edition = $("#drpOpsMasterEdition").val();
                    ReloadJqGrid2LoCal(gridOpsTableName, opMaster);

                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete, ObjMessageType.Info, respone.Result);
                } else {
                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges, ObjMessageType.Error, respone.Log);
                }
            });
        }, function () { }, "video");
    }
}
//#endregion

//#region copy funtions from process-partial-view.js

//#region event click on button
function ImageVideoFileEvents() {
    //Remove image on process modal
    $("#btnRemoveImg").click(function () {
        RemovePreviewImage();
    });

    //update video name on process modal
    $("#btnRemoveVideo").click(function () {
        DeleteProcessVideo();
    });

    //event on change video
    $(document).on("change", "#flProcessVideo", function (evt) {
        //Clear source of video preview
        $("#vidPreview").removeAttr("src");
        $("#vidPreview").removeAttr("poster");

        //Preview video before upload
        var $source = $('#videoPreview');
        $source[0].src = URL.createObjectURL(this.files[0]);
        $source.parent()[0].load();

        //Get file name
        var fileName = evt.target.files[0].name;
        var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, "MB");
        $("#lblVideoName").text(fileName + " ( " + fileSize + " MB )");
    });

    //event change when selecting image on process modal
    $("#flProcessImage").change(function (evt) {

        var fileName = evt.target.files[0].name;
        var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, Megabyte);
        if (fileSize > 4) {
            ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }

        $("#lblImageName").text(fileName + " ( " + fileSize + " MB )");

        //Preview image before upload to FTP
        readURL(this, "#imgPreview");
    });

    //clear video on modal "upload machine file"
    $("#btnRemoveFileOpDetail").click(function () {
        RemoveOpDetailFilePreview();
    });
}

const eventClickOnButtonOld = () => {
    //Save new process
    $("#btnSaveProcess").click(function () {
        //IsLayoutEvent = "0";
        //SaveNewProcess(FunctionCallBackSavingProcess);

        IsLayoutEvent = "0";
        if (StatusUpdateProcess === 1) {
            UpdateProcess_New(FunctionCallBackUpdateProcess);
        } else {
            SaveNewProcess_New(FunctionCallBackSavingProcess);
        }
    });

    //Update process
    //$("#btnUpdateProcess").click(function () {
    //    IsLayoutEvent = "0";
    //    UpdateProcess(FunctionCallBackUpdateProcess);
    //});

    //Remove image on process modal
    $("#btnRemoveImg").click(function () {
        RemovePreviewImage();
    });

    //update video name on process modal
    $("#btnRemoveVideo").click(function () {
        DeleteProcessVideo();
    });

    //event on change video
    $(document).on("change", "#flProcessVideo", function (evt) {
        //Clear source of video preview
        $("#vidPreview").removeAttr("src");
        $("#vidPreview").removeAttr("poster");

        //Preview video before upload
        var $source = $('#videoPreview');
        $source[0].src = URL.createObjectURL(this.files[0]);
        $source.parent()[0].load();

        //Get file name
        var fileName = evt.target.files[0].name;
        var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, "MB");
        $("#lblVideoName").text(fileName + " ( " + fileSize + " MB )");
    });

    //event change when selecting image on process modal
    $("#flProcessImage").change(function (evt) {

        var fileName = evt.target.files[0].name;
        var fileSize = ConvertByteToExpectedType(evt.target.files[0].size, Megabyte);
        if (fileSize > 4) {
            ShowMessageOk("003", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);
            return;
        }

        $("#lblImageName").text(fileName + " ( " + fileSize + " MB )");

        //Preview image before upload to FTP
        readURL(this, "#imgPreview");
    });

    //clear video on modal "upload machine file"
    $("#btnRemoveFileOpDetail").click(function () {
        RemoveOpDetailFilePreview();
    });
}

const eventOnTextBoxOld = () => {
    //event key up on textbox machine count
    $("#txtMachineCount").keyup(function () {
        var maxTime = CalculateMaxTime($("#txtProcessTime").val(), $("#txtWorker").val(), $("#txtMachineCount").val());
        $("#txtMaxTime").val(maxTime);
    });

    //event key up on textbox worker
    $("#txtWorker").keyup(function () {
        var maxTime = CalculateMaxTime($("#txtProcessTime").val(), $("#txtWorker").val(), $("#txtMachineCount").val());
        $("#txtMaxTime").val(maxTime);
    });
}
//#endregion

//#region get data through API
function GetModulesByStyleCode(styleCode) {
    var config = ObjectConfigAjaxPost("/Ops/GetModulesListByStyleCode", false, JSON.stringify({ styleCode: styleCode }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpModule", respone, "ModuleId", "ModuleName");
    });
}

function GetMasterCodeOpGroup(OpGroup) {
    var config = ObjectConfigAjaxPost("/Ops/GetMasterCode", false, JSON.stringify({ mCode: OpGroup }));
    AjaxPostCommon(config, function (respone) {
        FillDataToDropDownlist("drpProcessGroup", respone, "SubCode", "CodeName");
    });
}

function GetListProcessNameDetail(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, languageId, callBack) {
    var config = ObjectConfigAjaxPost("/Ops/GetListProcessNameDetail", true,
        JSON.stringify({
            edition: edition,
            styleCode: styleCode,
            styleSize: styleSize,
            styleColorSerial: styleColorSerial,
            revNo: revNo,
            opRevNo: opRevNo,
            opSerial: opSerial,
            languageId: languageId
        }));
    AjaxPostCommon(config, function (respone) {
        callBack(respone);
    });
}

//Get list ops detail
function GetListOpToolLinking(opTool, callBack) {
    $.ajax({
        url: "/Ops/GetToolLinkingByCode",
        async: true,
        type: "POST",
        data: JSON.stringify({ opTool: opTool }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            callBack(res);
        },
        error: function (jqXhr, status, err) {
            ShowMessageOk("002", SmsFunction.Generic, MessageType.Error, MessageContext.Error, ObjMessageType.Error, err.message);
        }
    });
}
//#endregion

//#region post Data
function UploadVideoOpnt_New(files, sysFileName, opMaster) {

    var statusUpload;

    // create array to store the buffer chunks
    var chunkFile = [];

    // set up other initial vars
    var bufferChunkSize = 3 * (1024 * 1024);

    var fileStreamPos = 0;

    // set the initial chunk length
    var endPos = bufferChunkSize;

    var fileSize = files.size;
    // add to the FileChunk array until we get to the end of the file
    while (fileStreamPos < fileSize) {
        // "slice" the file from the starting position/offset, to  the required length
        chunkFile.push(files.slice(fileStreamPos, endPos));
        fileStreamPos = endPos; // jump by the amount read
        endPos = fileStreamPos + bufferChunkSize; // set next chunk length
    }
    // get total number of "files" we will be sending
    var totalParts = chunkFile.length;
    var partCount = 0;
    // loop through, pulling the first item from the array each time and sending it
    while (chunk = chunkFile.shift()) {
        partCount++;
        // file name convention
        var filePartName = files.name + ".part_" + partCount + "." + totalParts;
        statusUpload = UploadChunkFileOpnt_New(chunk, filePartName, sysFileName, opMaster);
    }

    if (statusUpload === Fail) {
        return Fail;
    } else {
        return statusUpload;
    }
}

function UploadChunkFileOpnt_New(chunk, fileName, sysFileName, opMaster) {

    var resultUploadChunk;

    var data = new FormData();

    data.append(fileName, chunk);
    data.append("FileName", fileName);
    data.append("SysFileName", sysFileName);

    $.ajax({
        url: "/PlanManagement/UploadVideoSubProcess_New",
        async: false, //run sequence
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            if (result !== Fail) {
                resultUploadChunk = result;
            } else {
                resultUploadChunk = Fail;
            }
        },
        error: function () {
            resultUploadChunk = Fail;
        }
    });

    return resultUploadChunk;
}

function UploadVideoOpdt_New(opMaster) {
    var fileUpload = $('#flProcessVideo')[0].files;

    //Check file length
    if (fileUpload.length === 0) {
        return Success;
    }

    var files = fileUpload[0];
    var statusUpload;

    // create array to store the buffer chunks
    var chunkFile = [];

    // set up other initial vars
    var bufferChunkSize = 3 * (1024 * 1024);

    var fileStreamPos = 0;

    // set the initial chunk length
    var endPos = bufferChunkSize;

    var fileSize = files.size;
    // add to the FileChunk array until we get to the end of the file
    while (fileStreamPos < fileSize) {
        // "slice" the file from the starting position/offset, to  the required length
        chunkFile.push(files.slice(fileStreamPos, endPos));
        fileStreamPos = endPos; // jump by the amount read
        endPos = fileStreamPos + bufferChunkSize; // set next chunk length
    }
    // get total number of "files" we will be sending
    var totalParts = chunkFile.length;
    var partCount = 0;
    // loop through, pulling the first item from the array each time and sending it
    while (chunk = chunkFile.shift()) {
        partCount++;
        // file name convention
        var filePartName = files.name + ".part_" + partCount + "." + totalParts;
        statusUpload = UploadChunkFileOpdt_New(chunk, filePartName, opMaster);
    }

    if (statusUpload === Fail) {
        return Fail;
    } else {
        return statusUpload;
    }
}

function UploadChunkFileOpdt_New(chunk, fileName, opMaster) {

    var resultUploadChunk;

    var data = new FormData();

    data.append(fileName, chunk);
    data.append("FileName", fileName);
    data.append("StyleCode", opMaster.StyleCode);
    data.append("StyleSize", opMaster.StyleSize);
    data.append("StyleColor", opMaster.StyleColorSerial);
    data.append("StyleRevNo", opMaster.RevNo);
    data.append("OpRevNo", opMaster.OpRevNo);
    data.append("Edition", opMaster.Edition);
    data.append("OpSerial", $("#txtProcessNo").val());

    $.ajax({
        url: "/PlanManagement/UploadVideoOpdt_New",
        async: false, //run sequence
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            if (result !== Fail) {
                resultUploadChunk = result;
            } else {
                resultUploadChunk = Fail;
            }
        },
        error: function () {
            resultUploadChunk = Fail;
        }
    });

    return resultUploadChunk;

}

function UploadImageOpdt_New(opMaster) {
    var fileUpload = $("[id*=flProcessImage]").get(0);
    var files = fileUpload.files;
    var uploadStatus = Fail;

    //Check file length
    if (files.length === 0) {
        return Success;
    }

    //var fileSize = ConvertByteToExpectedType(files[0].size, Megabyte);

    var data = new FormData();
    for (var i = 0; i < files.length; i++) {

        data.append(files[i].name, files[i]);
        data.append("StyleCode", opMaster.StyleCode);
        data.append("StyleSize", opMaster.StyleSize);
        data.append("StyleColor", opMaster.StyleColorSerial);
        data.append("StyleRevNo", opMaster.RevNo);
        data.append("OpRevNo", opMaster.OpRevNo);
        data.append("Edition", opMaster.Edition);
        data.append("OpSerial", $("#txtProcessNo").val());
    }

    $.ajax({
        url: "/PlanManagement/UploadImageOpdt_New",
        async: false, //run sequence
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            if (result !== Fail) {
                uploadStatus = result;
            } else {
                uploadStatus = Fail;
            }
        },
        error: function () {
            $("[id*=hfFileName]").val("");
            uploadStatus = Fail;
        }
    });

    return uploadStatus;
}
//#endregion

//#region get data on screen
function CreateObjectOpDetail(objOpsMaster) {

    if ($.isEmptyObject(objOpsMaster)) {
        return null;
    }

    let moduleId = $("#drpModule").val();
    let opGroup = $("#drpProcessGroup").val();
    let machineType = '';
    let machineName = '';
    let iconname = '';
    //get main process
    const mainProcess = _listOpnt.find(x => x.MainProcess === '1');
    if (typeof (mainProcess) !== 'undefined') {
        machineType = $(`#drpMachineOpName${mainProcess.OpnSerial}`).val();
        machineName = $(`#drpMachineOpName${mainProcess.OpnSerial} option:selected`).text();
        iconname = $(`#hdIconName${mainProcess.OpnSerial}`).val();
    }

    if (IsLayoutEvent === "1") {
        switch (LayoutGroupMode) {
            case ModuleTypeConst:
                moduleId = $.isEmptyObject(moduleId) ? null : moduleId;
                break;
            case OpGroupConst:
                opGroup = $.isEmptyObject(opGroup) ? null : opGroup;
                break;
            case MachineTypeConst:
                machineType = $.isEmptyObject(machineType) ? null : machineType;
                break;
        }
    }

    let iotType = $("input[name='assemblyradio']:checked").val();
    //if iot type is null or undifined, NaN, empty string(''), 0 or false then return empty
    if ($.isEmptyObject(iotType)) iotType = '';

    let objOpDetail = {
        Edition: objOpsMaster.Edition,
        StyleCode: objOpsMaster.StyleCode,
        StyleSize: objOpsMaster.StyleSize,
        StyleColorSerial: objOpsMaster.StyleColorSerial,
        RevNo: objOpsMaster.RevNo,
        OpRevNo: objOpsMaster.OpRevNo,
        OpSerial: $("#txtProcessNo").val(),
        //process number and module
        OpNum: $("#txtProcessNumber").val(),
        ModuleId: moduleId,
        //Old process name
        RefOpName: $("#txtRefOpName").val(),
        OpName: $("#txtRefOpName").val(),
        MachineType: machineType,
        MachineName: machineName,
        //proess cost
        OpPrice: $("#txtProcessCost").val(),
        OfferOpPrice: $("#txtOfferPrice").val(),
        //CostingGroup: $("#txtCostingGroup").val(),
        OpGroup: opGroup,
        //outsourcing and hotspot
        OpsState: $("#chkOutsourcing").is(":checked") === true ? "1" : "0",
        HotSpot: $("#chkHotSpot").is(":checked") === true ? "1" : "0",
        //process time
        MachineCount: $("#txtMachineCount").val(),
        ManCount: $("#txtWorker").val(),
        OpTime: $("#txtProcessTime").val(),
        MaxTime: $("#txtMaxTime").val(),
        //image and video
        ImageName: $("#flProcessImage").val().split('\\').pop(),
        VideoFile: $("#flProcessVideo").val().split('\\').pop(),
        //remarks
        Remarks: $("#txtRemarks").val(),
        //ToolId: $("#drpToolMain").val(),
        OpTimeMax: objOpsMaster.OpTime,
        Page: isEmpty(LayoutPage) === true ? "" : LayoutPage,
        DisplayColor: isEmpty(DisplayColor) === true ? "" : DisplayColor,
        OpTimeBalancing: $("#txtProcessTime").val(),
        //IotType: iotType
        IotType: iotType,
        PickUp: $("#drpPickUpGrade").val(),
        Dispose: $("#drpDisposeGrade").val(),
        IconName: iconname
    };

    return objOpDetail;
}

//#endregion

//#region checking data
function CheckDataAddNewProccess() {
    let check = true;

    //Get style master from local storage
    let objOpsMaster = GetSelectedOneRowData(gridOpsTableId);
    if ($.isEmptyObject(objOpsMaster)) {
        check = false;
    }

    if (isEmpty($("#txtProcessNo").val())) {
        ColorTextbox("txtProcessNo", "error-border");
        check = false;
    } else {
        RemoveClass("txtProcessNo", "error-border");
    }

    if (isEmpty($("#txtMachineCount").val())) {
        ColorTextbox("txtMachineCount", "error-border");
        check = false;
    } else {
        RemoveClass("txtMachineCount", "error-border");
    }

    if (isEmpty($("#txtWorker").val())) {
        ColorTextbox("txtWorker", "error-border");
        check = false;
    } else {
        RemoveClass("txtWorker", "error-border");
    }

    if (isEmpty($("#txtMaxTime").val())) {
        ColorTextbox("txtMaxTime", "error-border");
        check = false;
    } else {
        RemoveClass("txtMaxTime", "error-border");
    }

    if (isEmpty($("#txtProcessTime").val())) {
        ColorTextbox("txtProcessTime", "error-border");
        check = false;
    } else {
        RemoveClass("txtProcessTime", "error-border");
    }

    //Checking process name
    if (isEmptyOrWhiteSpace($("#txtOpName1").val())) {
        ColorTextbox("txtOpName1", "error-border");
        check = false;
    } else {
        RemoveClass("txtOpName1", "error-border");
    }

    if (HasFile($("#flProcessVideo")[0].files)) {
        if (!CheckTypeOfVideo($("#flProcessVideo")[0].files)) {
            ShowMessage("Upload video", "Please upload MP4 file only", MessageType.Error);
            return false;
        }
    }

    const mainProcess = _listOpnt.find(x => x.MainProcess === '1');
    if (typeof (mainProcess) === 'undefined') {
        ShowMessage("Save Process", "Please select main process", MessageType.Warning);
        return false;
    }

    if (check === false) {
        ShowMessage("Save Process", "Please check data input", MessageType.Warning);
    }
    return check;
}
//#endregion

//#region functions
/**
 * Init master data for process modal
 */
function initMasterDataProcessModal() {
    //Get process group
    GetMasterCodeOpGroup(OpGroup);
}

function loadTooltipForProcessNameTextbox() {
    $("#txtRefOpName").mouseenter(function () {
        let proName = $(this).val();
        $(this).attr('title', proName);
    });
}

function InitDataForProcessModal() {
    //START ADD - SON - 2021.01.15) 18/Jan/2021
    //Get data Classification and grade
    loadDataOnOpDetailModal();

    //init grid process name detail
    bindDataToJqGridOpNameDetail('', '', '', '', '', '', '', '', false);

    let objOpmt = JSON.parse(localStorage.getItem(OpsMasterInfo));
    if (!$.isEmptyObject(objOpmt))
        bindDataToJqGridBom(objOpmt.StyleCode, objOpmt.StyleSize, objOpmt.StyleColorSerial, objOpmt.RevNo, objOpmt.OpRevNo, objOpmt.Edition, false);

    //init temporary grid view linking bom and pattern
    bindDataToJqGridBomAndPatternLinking();

    //get module by style code
    GetModulesByStyleCode(objOpmt.StyleCode);

    //Get process group
    GetMasterCodeOpGroup(OpGroup);
    //END ADD - SON - 2021.01.15) 18/Jan/2021

    if (StatusUpdateProcess === 1) {
        $("#btnRemoveVideo").prop("disabled", false);
        //$("#btnEnterProcess").prop("disabled", false);
    } else if (StatusUpdateProcess === 0) {
        $("#btnRemoveVideo").prop("disabled", true);
        //$("#btnEnterProcess").prop("disabled", false);
    } else {
        //StatusUpdateProcess = 3: view mode
        //$("#btnEnterProcess").prop("disabled", true);
    }

    //Check on ops layout screen
    if (IsLayoutEvent === "1") {
        switch (LayoutGroupMode) {
            case ModuleTypeConst:
                $("#drpModule").prop("disabled", true);
                break;
            case OpGroupConst:
                $("#drpProcessGroup").prop("disabled", true);
                break;
            case MachineTypeConst:
                $("#drpMachineDefault").prop("disabled", true);
                break;
        }
    }
}

function InitDataUpdateProcess(objOpsDetail) {
    //Set status update process is 1.
    StatusUpdateProcess = 1;

    ClearDataAddNewProccess();

    ShowModal(ProcessModal);

    //Get menu id by edition.
    var currentOpmt = GetSelectedOneRowData(gridOpsTableId);

    InitDataForProcessModal();
    LoadObjectOpDetailModal(objOpsDetail);

    //setTimeout(function () {
    //    LoadObjectOpDetailModal(objOpsDetail);
    //}, 1000);

    let userRole = null;
    switch (currentOpmt.Edition) {
        case editionPdm:
        case editionOps:
            userRole = UserRoleOpm;
            break;
        case editionAom:
        case editionMes:
            userRole = UserRoleFom;
            break;
        default:
    }

    if (!$.isEmptyObject(userRole) && userRole.IsUpdate === '1') {
        $('#btnSaveProcess').show();
        //START ADD - SON) 15/Jan/2021
        $('#btnSaveLinkingBomPattern').show();
        //END ADD - SON) 15/Jan/2021
    } else {
        $('#btnSaveProcess').hide();
        //START ADD - SON) 15/Jan/2021
        $('#btnSaveLinkingBomPattern').hide();
        //END ADD - SON) 15/Jan/2021
    }
}

function ClearDataAddNewProccess() {
    var LayoutPage = null;
    var DisplayColor = null;
    var LayoutTopY = null;
    var LayoutLeftX = null;

    clearDataOnProcessDtModal();
}

function LoadObjectOpDetailModal(objOpDetail) {

    $("#hdStyleCode").val(objOpDetail.StyleCode);
    $("#hdStyleColor").val(objOpDetail.StyleColorSerial);
    $("#hdStyleSize").val(objOpDetail.StyleSize);
    $("#hdStyleRevNo").val(objOpDetail.RevNo);
    $("#hdOpRevNo").val(objOpDetail.OpRevNo);

    $("#drpPickUpGrade").val(objOpDetail.PickUp).trigger("change");
    $("#drpDisposeGrade").val(objOpDetail.Dispose).trigger("change");

    $("#drpModule").val(objOpDetail.ModuleId).trigger("change");
    $("#txtProcessNo").val(ZeroPad(objOpDetail.OpSerial, 3));
    $("#txtProcessNumber").val(objOpDetail.OpNum);
    $("#txtRefOpName").val(objOpDetail.OpName);

    $("#txtProcessCost").val(objOpDetail.OpPrice);
    $("#txtOfferPrice").val(objOpDetail.OfferOpPrice);
    $("#drpProcessGroup").val(objOpDetail.OpGroup).trigger("change");

    $("#txtMachineCount").val(objOpDetail.MachineCount);
    $("#txtWorker").val(objOpDetail.ManCount);
    $("#txtProcessTime").val(objOpDetail.OpTime);
    $("#txtMaxTime").val(objOpDetail.MaxTime);

    $("#txtRemarks").val(objOpDetail.Remarks);

    $("#chkHotSpot").prop('checked', StringToBoolean(objOpDetail.HotSpot));
    $("#chkOutsourcing").prop('checked', StringToBoolean(objOpDetail.OpsState));

    //Get process name detail 
    GetListProcessNameDetail(objOpDetail.Edition, objOpDetail.StyleCode, objOpDetail.StyleSize, objOpDetail.StyleColorSerial, objOpDetail.RevNo, objOpDetail.OpRevNo, objOpDetail.OpSerial, objOpDetail.LanguageId, function (lstOpnts) {
        //set list opnt to golable variable
        _listOpnt = lstOpnts;
        //Set hidden opname id
        _listOpnt.forEach(opnt => {
            let mainProcess = opnt.MainProcess === "1" ? true : false;
            switch (opnt.OpnSerial) {
                case _opnSerial.One:
                    $('#divOpName2').show();
                    loadDataForSubProcess(opnt);
                    break;
                case _opnSerial.Two:
                    $('#divOpName3').show();                   
                    loadDataForSubProcess(opnt);
                    break;
                case _opnSerial.Three:
                    $('#divOpName4').show();
                    loadDataForSubProcess(opnt);
                    break;
                case _opnSerial.Four:
                    $('#divOpName5').show();
                    loadDataForSubProcess(opnt);
                    break;
                case _opnSerial.Five:
                    loadDataForSubProcess(opnt);
                    break;
                default:
                    break;
            }
        });

        generateDefiningMultiOperation();
    });

    //Get machine
    //GetListOpToolLinking(objOpDetail, (listMachine) => {
    //    listMachine.forEach(mc => {
    //        switch (mc.OpnSerial) {
    //            case _opnSerial.One:
    //                reselectOpdtMachine('#drpMachineOpName1', mc);
    //                break;
    //            case _opnSerial.Two:
    //                reselectOpdtMachine('#drpMachineOpName2', mc);
    //                break;
    //            case _opnSerial.Three:
    //                reselectOpdtMachine('#drpMachineOpName3', mc);
    //                break;
    //            case _opnSerial.Four:
    //                reselectOpdtMachine('#drpMachineOpName4', mc);
    //                break;
    //            case _opnSerial.Five:
    //                reselectOpdtMachine('#drpMachineOpName5', mc);
    //                break;
    //            default:
    //        }
    //    });
    //});

    //Get image name and video name
    $("#hdImageName").val(objOpDetail.ImageName);
    $("#hdVideoName").val(objOpDetail.VideoFile);

    previewImageFromHttpLink(objOpDetail.ImageName, objOpDetail.ImageLink, '#imgPreview');
    previewVideoFromHttpLink(objOpDetail.VideoFile, objOpDetail.VideoOpLink, '#vidPreview');

    //IoT tracking
    $(`input[name=assemblyradio][value='${objOpDetail.IotType}']`).prop("checked", true);

}

function StyleValidateAddNewProcess() {
    //Remove class normal-error
    //RemoveColorBorderDropdownlistMultiSelect("drpOpName", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpProcessGroup", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpJobCode", "error-border");
    RemoveColorBorderDropdownlistSelect2("drpMachineDefault", "error-border");
    RemoveClass("txtMachineCount", "error-border");
    RemoveClass("txtWorker", "error-border");
    RemoveClass("txtMaxTime", "error-border");
    RemoveClass("txtProcessTime", "error-border");
    //RemoveClass("btnOpTime", "error-border");
    RemoveClass("btnEnterProcess", "error-border");
}

function BindDataToJqGridInputOpTimeModal(opNameData) { }

function BindDataToJqGridProcessNameTemplate() { }
//#endregion

//#region saving process
//async function SaveNewProcess(callBackFunc, oploCallBack) {
const SaveNewProcess_New = async (callBackFunc, oploCallBack) => {

    //Update machine type for process 1, 2, 3, 4 and 5
    updateSubProcessDetail();

    if (CheckDataAddNewProccess()) {

        //Get data from local storage.
        let opMaster = GetSelectedOneRowData(gridOpsTableId);

        if ($.isEmptyObject(opMaster)) {
            ShowMessageOk("004", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

            return;
        }

        let objOpDetail = CreateObjectOpDetail(opMaster);

        $.blockUI(ObjectBlockUICss);

        setTimeout(async function () {
            //const statusUploadImgSubPro = await upoadListImageOpnt();
            await upoadListImageOpnt().then((statusUploadImgSubPro) => {
                //upload list image of sub process
                if (!statusUploadImgSubPro) {
                    $.unblockUI();
                    console.log('cannot upload image of sub process');
                    //ShowMessage('Add new process', 'Cannot upload image of sub process', MessageType.Error);
                    return;
                }
            });

            let statusUploadVideoSupPro = await uploadListVideoOpnt(opMaster);
            if (statusUploadVideoSupPro === Fail) {
                $.unblockUI();
                console.log('cannot upload video of opnt');
                //ShowMessage('Add new process', 'Cannot upload video of sub process');
                return;
            }

            console.log('start saving process');

            //Set uploading status is true for testing insert new process detail
            let statusUploadImg = UploadImageOpdt_New(opMaster);
            let statusUploadVideo = UploadVideoOpdt_New(opMaster);

            if (statusUploadImg !== Fail && statusUploadVideo !== Fail) {

                //let lstMachine = CreateObjectMachine();
                //let lstTool = CreateObjectTool();
                //let lstMachine = getMachinesList(opMaster);
                let lstMachine = [];
                let lstTool = [];

                //Set image name and video name.
                if (statusUploadImg !== Success) objOpDetail.ImageName = statusUploadImg;
                if (statusUploadVideo !== Success) objOpDetail.VideoFile = statusUploadVideo;

                //console.log(lstMachine);

                let blAddPro = AddNewProcess_New(objOpDetail, lstMachine, lstTool, _listOpnt);

                if (blAddPro === true) {
                    if (callBackFunc === null) {
                        HideModal(ProcessModal);
                        objOpDetail.X = LayoutLeftX;
                        objOpDetail.Y = LayoutTopY;
                        //var node = CreateObjectForLayout(opMaster, objOpDetail);
                        //// map objOpDetail to node
                        //oploCallBack(node);

                        CreateLayoutProcess(objOpDetail, (node) => {
                            oploCallBack(node);

                            //Reload operation master gird
                            opMaster.Edition = $("#drpOpsMasterEdition").val();
                            ReloadJqGrid2LoCal(gridOpsTableName, opMaster);
                        });
                    } else {
                        callBackFunc(blAddPro);
                    }
                }
            } else {
                ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error);
            }

            //unblock UI
            $.unblockUI();
        }, 100);
    }
}

function UpdateProcess_New(callBackFunc, oploCallBack) {

    //Update machine type and main process for sub process
    updateSubProcessDetail();

    //Check data before update
    if (!CheckDataAddNewProccess()) return;

    //Get data from local storage.
    var opMaster = GetSelectedOneRowData(gridOpsTableId);

    $.blockUI(ObjectBlockUICss);
    setTimeout(async function () {

        //const statusUploadImgSubPro = await upoadListImageOpnt();
        await upoadListImageOpnt().then((statusUploadImgSubPro) => {
            //upload list image of sub process
            if (!statusUploadImgSubPro) { $.unblockUI(); console.log('cannot upload image of opnt'); return; }
        });

        let statusUploadVideoSupPro = await uploadListVideoOpnt(opMaster);
        if (statusUploadVideoSupPro === Fail) {
            $.unblockUI();
            console.log('cannot upload video of opnt');
            return;
        }

        console.log('start upload process image');

        var statusUploadImg = UploadImageOpdt_New(opMaster);
        var statusUploadVideo = UploadVideoOpdt_New(opMaster);
        var objOpDetail = CreateObjectOpDetail(opMaster);

        if (statusUploadImg !== Fail && statusUploadVideo !== Fail) {
            //Set old name of file
            objOpDetail.ImageName = $("#hdImageName").val();
            objOpDetail.VideoFile = $("#hdVideoName").val();

            //Set image name and video name.
            if (statusUploadImg !== Success) objOpDetail.ImageName = statusUploadImg;
            if (statusUploadVideo !== Success) objOpDetail.VideoFile = statusUploadVideo;

            //var lstMachine = CreateObjectMachine();
            //var lstTool = CreateObjectTool();
            //let lstMachine = getMachinesList(opMaster);
            let lstMachine = [];
            let lstTool = [];

            //Update process detail
            var blUpdate = UpdateOpDetail_New(objOpDetail, lstMachine, lstTool, _listOpnt);
            if (blUpdate === false) {
                //callBackFunc(blUpdate, objOpDetail);
            } else {
                if (callBackFunc === null) {

                    HideModal(ProcessModal);
                    objOpDetail.X = LayoutLeftX;
                    objOpDetail.Y = LayoutTopY;
                    //var node = CreateObjectForLayout(opMaster, objOpDetail);
                    //// map objOpDetail to node
                    //oploCallBack(node);

                    CreateLayoutProcess(objOpDetail, (node) => {
                        oploCallBack(node);

                        //Reload operation master gird
                        opMaster.Edition = $("#drpOpsMasterEdition").val();
                        //ReloadJqGrid(gridOpsTableName, opMaster);
                        ReloadJqGrid2LoCal(gridOpsTableName, opMaster);
                    });
                } else {
                    callBackFunc(blUpdate);
                }
            }
        } else {
            ShowMessageOk("001", SmsFunction.Upload, MessageType.Error, MessageContext.Error, ObjMessageType.Error);
        }
        //unblock UI
        $.unblockUI();
    }, 100);
}

//Add new process
function AddNewProcess_New(objOpDetail, lstMachine, lstTool, lstOpnt) {
    var addStatus;
    $.ajax({
        url: "/PlanManagement/AddNewProcess_New",
        async: false,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail, lstOpMachine: lstMachine, lstOpTool: lstTool, lstOpnt: lstOpnt }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                addStatus = true;
                ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add, ObjMessageType.Info);

            } else {
                addStatus = false;
                ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);

            }

        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessageOk("001", SmsFunction.Add, MessageType.Error, MessageContext.Error, ObjMessageType.Error, errorThrown.message);

            addStatus = false;
        }
    });
    return addStatus;
}

function UpdateOpDetail_New(objOpDetail, lstMachine, lstTool, lstOpnt) {

    var addStatus;

    $.ajax({
        url: "/PlanManagement/UpdateOpDetail_New",
        async: false,
        type: "POST",
        data: JSON.stringify({ opDetail: objOpDetail, lstMachine: lstMachine, lstTool: lstTool, lstOpnt: lstOpnt }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                addStatus = true;
                ShowMessageOk("001", SmsFunction.Update, MessageType.Success, MessageContext.Update, ObjMessageType.Info);
            } else {
                addStatus = false;
                ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error, res);
            }
        },
        error: function (jqXhr, status, errorThrown) {
            ShowMessageOk("001", SmsFunction.Update, MessageType.Error, MessageContext.Error, ObjMessageType.Error, status + ': ' + errorThrown);
            addStatus = false;
        }
    });

    return addStatus;
}
//#endregion

//#endregion

