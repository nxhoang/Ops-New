
const eventClickButtonProcessDetail = () => {
    //show process detail modal
    $('#btnAddProcessDetail').click(() => {
        clearDataOnProcessDtModal();
        ShowModal('mdlProcessDetail');

        StatusUpdateProcess = 0;
        GetMaxOpSerial();
        InitDataForProcessModal();
        $('#btnUpdateProcess').hide();
        $('#btnSaveProcess').show();
    });

    //add process name 1
    $('#btnAddOpName1').click(() => {

        _addOpName = 1;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');

        //reload data of process
        reloadSubProcessDtModal(_opnSerial.One);

    });

    //add process name 2
    $('#btnAddOpName2').click(() => {

        _addOpName = 2;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');

        //reload data of process
        reloadSubProcessDtModal(_opnSerial.Two);

    });

    //add process name 3
    $('#btnAddOpName3').click(() => {
        _addOpName = 3;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');

        //reload data of process
        reloadSubProcessDtModal(_opnSerial.Three);
    });

    //add process name 3
    $('#btnAddOpName4').click(() => {
        _addOpName = 4;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');

        //reload data of process
        reloadSubProcessDtModal(_opnSerial.Four);
    });

    //add process name 3
    $('#btnAddOpName5').click(() => {
        _addOpName = 5;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');

        //reload data of process
        reloadSubProcessDtModal(_opnSerial.Five);
    });

    //event click Ok button on modal select process name
    $('#btnOkOpName').click(() => {
        //Get operation master from selected row.
        let opmt = GetSelectedOneRowData(gridOpsTableId);
        const machineCount = $('#txtMachineCountMain').val();
        const manCount = $('#txtWorkerMain').val();
        const opTime = $('#txtProcessTimeMain').val();
        const maxTime = $('#txtMaxTimeMain').val();

        if (_addOpName === 1) {
            //Generate process name
            generateOpName('#txtOpName1', '#hdOpNameId1');
            //Display process name 2
            $('#divOpName2').show();
            //hide modal select process name
            HideModal('mdlSelectOpName');
            //Generate reference opname
            generateRefOpName();

            const opnt = createObjectProcessNameDetail(
                opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, ''
                , $('#hdOpNameId1').val(), _opnSerial.One, machineCount, manCount, opTime, maxTime, '', '', '', '', '');
            _listOpnt.push(opnt);

            generateDefiningMultiOperation();
        } else if (_addOpName === 2) {
            //Generate process name
            generateOpName('#txtOpName2', '#hdOpNameId2');
            //Display process name 2
            $('#divOpName3').show();
            //hide modal select process name
            HideModal('mdlSelectOpName');
            //Generate reference opname
            generateRefOpName();

            const opnt = createObjectProcessNameDetail(
                opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, ''
                , $('#hdOpNameId2').val(), _opnSerial.Two, machineCount, manCount, opTime, maxTime, '', '', '', '', '');
            _listOpnt.push(opnt);

            generateDefiningMultiOperation();
        } else {
            //Generate process name
            generateOpName('#txtOpName3', '#hdOpNameId3');
            //hide modal select process name
            HideModal('mdlSelectOpName');
            //Generate reference opname
            generateRefOpName();

            const opnt = createObjectProcessNameDetail(
                opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, ''
                , $('#hdOpNameId3').val(), _opnSerial.Three, machineCount, manCount, opTime, maxTime, '', '', '', '', '');
            _listOpnt.push(opnt);

            generateDefiningMultiOperation();
        }
    });

    //process detail process 2
    $('#btnDetail1').click(() => {
        _addOpName = 1;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');
    });

    //process detail process 2
    $('#btnDetail2').click(() => {
        _addOpName = 2;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');
    });

    //process detail process 3
    $('#btnDetail3').click(() => {
        _addOpName = 3;
        clearDataOnProcessDtSubModal();
        ShowModal('mdlProcessDetailSub');
    });

    $('#btnRemoveVideoSub').click(() => {

        //check whether video is uploaded or not. If it exists then remove it from current video list and reload video from http link
        let uploadVideo = _listImgVideoOpnt.find(x => x.IsImage === false && x.OpnSerial === _addOpName);
        if (!$.isEmptyObject(uploadVideo) || $("[id*=flProcessVideoSub]").get(0).files.length > 0) {
            //remove video object if it is removed
            _listImgVideoOpnt = _listImgVideoOpnt.filter(function (listFilesOpnt) {
                return listFilesOpnt.IsImage === false && listFilesOpnt.OpnSerial !== _addOpName;
            });
            //remove video preview
            removeVideoPreview('#flProcessVideoSub', '#vidPreviewSub', '#videoPreviewSub', '#lblVideoNameSub');

            if (StatusUpdateProcess === 1) {
                //load video from http link
                let opnt = _listOpnt.find(x => x.OpnSerial === _addOpName);
                if (!$.isEmptyObject(opnt) && !$.isEmptyObject(opnt.VideoFile)) {
                    previewVideoFromHttpLink(opnt.VideoFile, opnt.VideoLink, '#vidPreviewSub');
                }
            }
        } else {
            //if status is update then preview video from http link
            if (StatusUpdateProcess === 1) {
                deleteImageOrVideoSubProcess(_addOpName, false);
            }
        }
    });

    $('#btnRemoveImgSub').click(() => {
        const opnt = _listOpnt.find(x => x.OpnSerial === _addOpName);
        //Get image from temporary data
        const opntImg = _listImgVideoOpnt.find(x => x.IsImage && x.OpnSerial === _addOpName);
        //If image is not null then remove it from temporary list
        if (!$.isEmptyObject(opntImg)) {
            //remove image object if it is removed
            _listImgVideoOpnt = _listImgVideoOpnt.filter(function (listFilesOpnt) {
                return listFilesOpnt.IsImage === false && listFilesOpnt.OpnSerial !== _addOpName;
            });
            //remove image preview
            removeImagePreview('#flProcessImageSub', '#imgPreviewSub', '#lblImageNameSub');

            //if status is update then load image preview from http link
            if (StatusUpdateProcess === 1) previewImageFromHttpLink(opnt.ImageName, opnt.ImageLink, '#imgPreviewSub');
        } else {
            //if status update process is 1 then update image name of opnt.
            if (StatusUpdateProcess === 1) {
                //update image name to empty

                //remove image preview after update image name to empty
                removeImagePreview('#flProcessImageSub', '#imgPreviewSub', '#lblImageNameSub');
            }
        }
    });

    //Saving sub process detail 1, 2 and 3
    $('#btnOkOpDetailSub').click(() => {
        //Check data before save to temporary
        if (!checkDataSubProcessDetail()) return;

        if (_addOpName === 1) {
            //Update object process name detail 2
            updateObjOpNameDt(_opnSerial.One);
            HideModal('mdlProcessDetailSub');
        } else if (_addOpName === 2) {
            //Update object process name detail 2
            updateObjOpNameDt(_opnSerial.Two);
            HideModal('mdlProcessDetailSub');
        } else if (_addOpName === 3) {
            //Update object process name detail 3
            updateObjOpNameDt(_opnSerial.Three);
            HideModal('mdlProcessDetailSub');
        } else if (_addOpName === 4) {
            //Update object process name detail 3
            updateObjOpNameDt(_opnSerial.Four);
            HideModal('mdlProcessDetailSub');
        } else {
            //Update object process name detail 3
            updateObjOpNameDt(_opnSerial.Five);
            HideModal('mdlProcessDetailSub');
        }
    });

    //#region pickup and dispose
    //Click button pick up detail
    $('#btnPickUpDetail').click(() => {
        clearDataOnProcessDtConceptModal();
        ShowModal('mdlProcessDetailConcept');
    });

    //Click button pick up detail
    $('#btnDisposeDetail').click(() => {
        clearDataOnProcessDtConceptModal();
        ShowModal('mdlProcessDetailConcept');
    });

    $('#btnSaveOpDetailConcept').click(() => {
        //Get operation master from selected row.
        let opmt = GetSelectedOneRowData(gridOpsTableId);

        if (_addPickUpOp === 1) {
            //Create object process name detail for pickup 
            const opnt = createObjOpNameDtPickUpAndDispose(opmt, '4')
            opnt.OpNameId = $('#hdOpNameIdConcept').val();
            HideModal('mdlProcessDetailConcept');
        } else {
            //Create object process name detail for dispose 
            const opnt = createObjOpNameDtPickUpAndDispose(opmt, '5')
            opnt.OpNameId = $('#hdOpNameIdConcept').val();
            HideModal('mdlProcessDetailConcept');
        }
    });
    //#endregion

    $('#btnPattern').click(() => {
        let opSerial = StatusUpdateProcess === 1 ? GetSelectedOneRowData('#gridOpsDetail').OpSerial : 1;
        let selOpmt = GetSelectedOneRowData('#gridOpsTable');
        reloadGridLinkedItem(selOpmt.StyleCode, selOpmt.StyleSize, selOpmt.StyleColorSerial, selOpmt.RevNo, selOpmt.OpRevNo, opSerial, selOpmt.Edition, false, {}, {});
        ShowModal('mdlPatternLinking');
    });

    $("#btnLinkItem").click(() => {
        //get selected row in sub gridview
        _listTempLinkedPattern = getSelectedRowsSubGrid('#tbBom');
        //get selected row in gridview
        let selectedRowBom = GetSelectedMultipleRowsData('#tbBom');
        
        const gridData = jQuery('#tbBom').jqGrid("getRowData");

        //set selectedRowBom with empty array if it is null
        if (selectedRowBom === null) {
            selectedRowBom = [];
        }
        else {
            //update has pattern to N
            selectedRowBom.forEach((bom) => { bom.HasPattern = 'N'; });
        }

        //get item of linked patterns
        _listTempLinkedPattern.forEach((pt) => {            

            //find patterns of pattern
            let item = selectedRowBom.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
            //if pattern doesn't have parents then get it from BOM
            if (item === null || typeof (item) === "undefined") {
                let parentsItem = gridData.find(x => x.ItemCode === pt.ItemCode && x.ItemColorSerial === pt.ItemColorSerial && x.MainItemCode === pt.MainItemCode && x.MainItemColorSerial === pt.MainItemColorSerial);
                parentsItem.HasPattern = 'Y';
                selectedRowBom.push(parentsItem);
            }
        });

        ShowModal('mdlBomPatternLinking');
        //reload bom and pattern linking modal
        ReloadJqGridLocal('tbBomPatternLinking', selectedRowBom);
    });
}

const eventOnChangeSelection = () => {
    //On change classification
    $('#drpClassificationSub').change(() => {
        if (_addOpName === 1) {
            classificationChange('drpMachineOpName1');
        } else if (_addOpName === 2) {
            classificationChange('drpMachineOpName2');
        } else {
            classificationChange('drpMachineOpName3');
        }

        //Show stitching length and stitches per inch: Stitching(3486)
        if ($('#drpClassificationSub').val() === _stitchingOpNameId) {
            $('#divStitching').show();
        } else {
            $('#divStitching').hide();
        }
        //clear stitching
        $('#txtStitchingLength').val('');
        $('#txtStitchesPerInch').val('');
    });

    //On change sub class
    $('#drpSubClassSub').change(() => {
        if (_addOpName === 1) {
            subClassChange('drpMachineOpName1');
        } else if (_addOpName === 2) {
            subClassChange('drpMachineOpName2');
        } else {
            subClassChange('drpMachineOpName3');
        }
    });

    //On change Sub Sub
    $('#drpSubSubSub').change(() => {
        if (_addOpName === 1) {
            //Get machine
            getMachine($('#drpSubSubSub').val(), 'drpMachineOpName1');
        } else if (_addOpName === 2) {
            //Get machine
            getMachine($('#drpSubSubSub').val(), 'drpMachineOpName2');
        } else {
            //Get machine
            getMachine($('#drpSubSubSub').val(), 'drpMachineOpName3');
        }
    });

    //Event on change opdetail concept
    //On change classification
    $('#drpClassificationConcept').change(() => {
        //Get group machine
        getGroupMachine($('#drpClassificationConcept').val(), `drpMachineConcept`);
        //Get list sub class
        getSubClass('drpSubClassConcept', _opGroupLevel.Level_1, $('#drpClassificationConcept').val());

        //Generate process name
        generateOpName('#txtOpNameConcept', '#hdOpNameIdConcept');
    });

    //On change sub class
    $('#drpSubClassConcept').change(() => {
        getSubSub(_opGroupLevel.Level_2, $('#drpSubClassConcept').val(), 'drpSubSubConcept');
        //Get machine
        getMachine($('#drpSubClassConcept').val(), "drpMachineConcept");

        //Generate process name
        generateOpName('#txtOpNameConcept', '#hdOpNameIdConcept');
    });

    //On change Sub Sub
    $('#drpSubSubConcept').change(() => {
        //Get machine
        getMachine($('#drpSubSubConcept').val(), "drpMachineConcept");

        //Generate process name
        generateOpName('#txtOpNameConcept', '#hdOpNameIdConcept');
    });

    //event on change pick up and dispose
    $('#drpPickUpGrade, #drpDisposeGrade').change(() => {
        generateRefOpName();

        generateDefiningMultiOperation();
    });
}

const eventSelectRadioButton = () => {
    $('input[type=radio][name=mainProcess]').change(function () {
        if (this.value === '1') {
            //upate main process for process name detail
            let opnt = _listOpnt.find(x => x.OpnSerial === _opnSerial.One);
            if (!$.isEmptyObject(opnt)) opnt.MainProcess = '1';
        }
        else if (this.value === '2') {
            //upate main process for process name detail
            let opnt = _listOpnt.find(x => x.OpnSerial === _opnSerial.Two);
            if (!$.isEmptyObject(opnt)) opnt.MainProcess = '1';
        } else if (this.value === '3') {
            //upate main process for process name detail
            let opnt = _listOpnt.find(x => x.OpnSerial === _opnSerial.Three);
            if (!$.isEmptyObject(opnt)) opnt.MainProcess = '1';
        }
    });
}

const eventSelectFileChange = () => {
    $("#flProcessImageSub").change(function (evt) {
        previewImage(evt.target.files[0], '#imgPreviewSub', '#lblImageNameSub');
    });

    $("#flProcessVideoSub").change(function (evt) {
        previewVideo(this.files[0], '#vidPreviewSub', '#videoPreviewSub', '#lblVideoNameSub');
    });
}