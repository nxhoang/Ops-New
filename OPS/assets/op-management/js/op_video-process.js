//#region Properties
const InputUploadVideo = "inputUploadVideo",
    DivDropVideo = "divDropVideo",
    LbUploadVideo = "lbUploadVideo",
    VdProcess = "vdProcess",
    JqVdProcess = "#vdProcess",
    DivVideoTag = "divVideoTag",
    DivLoadVideo = "divLoadVideo",
    BtnCloseVdPreview = "btnCloseVdPreview",
    AVdPlay = "aVdPlay",
    SpTotalTime = "spTotalTime",
    SpCurentTime = "spCurentTime",
    DivCurrentSlider = "divCurrentSlider",
    DivSliderWrap = "divSliderWrap",
    SliderLeft = "sliderLeft",
    SliderRight = "sliderRight",
    SpSplitBar = "spSplitBar",
    MsgBox = "msgBox",
    BtnSplit = "btnSplit",
    BtnUploadVideo = "btnUploadVideo",
    BtnSaveVideo = "btnSaveVideo",
    DivProcessVideo = "divProcessVideo",
    DivVideoSide = "divVideoSide",
    DivPartVideoSide = "divPartVideoSide",
    DivBarPart = "divBarPart",
    StartH = "startH",
    StartM = "startM",
    StartS = "startS",
    EndH = "endH",
    EndM = "endM",
    EndS = "endS",
    TxtCropSecond = "txtCropSecond",
    SpMsgContent = "spMsgContent",
    DivVideoRightList = "divVideoRightList",
    DivGlobe = "divGlobe",
    BtnShowProcessVideoModal = "btnShowProcessVideoModal",
    BtnHideOpVideoModal = "btnHideOpVideoModal",
    BtnCloseVdpModal = "btnCloseVdpModal",
    DivRightVideoList = "divRightVideoList",
    JsDivRightVideoList = "#divRightVideoList",
    JsVdpModal = "#vdpModal",
    BtnCloseRightVdList = "btnCloseRightVdList",
    UrlUploadChunkVideos = "/OpsLayout/UploadVideoChunkFile",
    UrlSplitOpdtVideos = "/OpsLayout/SplitOpdtVideos",
    BarColour = ["#FF0000", "#800000", "#6e6e04", "#2a0873", "#0e750e", "#096666"],
    AjaxWaitMes = "<h3>Please wait...</h3>"; // blockUI message;
var _isSlide = false,
    _isSlideLeft = false,
    _isSlideRight = false,
    _sliderWrap,
    _sliderLeft,
    _sliderRight,
    _spSplitBar,
    _txtCropSec,
    _msgBox,
    _isMsgBoxShow = false,
    _pxPerOneSecond,
    _vidTag,
    _circleSlider,
    _startH,
    _startM,
    _startS,
    _endH,
    _endM,
    _endS,
    _btnSplit,
    _isStartTimeChange = false,
    _isEndTimeChange = false,
    _currentVideoSrc,
    _partVideo = 0,
    _videoOpdts = [],
    colorIndex = 0,
    _limitedCropping = false,
    HideOpVideoModal = false;
//#endregion

//#region Ready
(() => {
    _sliderLeft = document.getElementById(SliderLeft);
    _sliderRight = document.getElementById(SliderRight);
    _spSplitBar = document.getElementById(SpSplitBar);
    _msgBox = document.getElementById(MsgBox);
    _vidTag = document.getElementById(VdProcess);
    _circleSlider = document.getElementById(DivCurrentSlider);
    _startH = document.getElementById(StartH);
    _startM = document.getElementById(StartM);
    _startS = document.getElementById(StartS);
    _endH = document.getElementById(EndH);
    _endM = document.getElementById(EndM);
    _endS = document.getElementById(EndS);
    _btnSplit = document.getElementById(BtnSplit);
    _txtCropSec = document.getElementById(TxtCropSecond);

    document.getElementById(BtnCloseVdPreview).addEventListener("click", closeVdPreview);
    document.getElementById(AVdPlay).addEventListener("click", playVideo);
    document.getElementById(InputUploadVideo).addEventListener("change", onChangedInputVf);
    document.getElementById(LbUploadVideo).addEventListener("dragover", dragOverDivVideo);
    document.getElementById(LbUploadVideo).addEventListener("drop", dropDivVideo);
    _startH.addEventListener('change', changeTimeValue);
    _startM.addEventListener('change', changeTimeValue);
    _startS.addEventListener('change', changeTimeValue);
    _endH.addEventListener('change', changeTimeValue);
    _endM.addEventListener('change', changeTimeValue);
    _endS.addEventListener('change', changeTimeValue);
    _btnSplit.addEventListener('click', cropVideo);

    _circleSlider.addEventListener("mousedown", mouseDownCurrentSlider);
    _sliderLeft.addEventListener("mousedown", mouseDownSliderLeft);
    _sliderRight.addEventListener("mousedown", mouseDownSliderRight);
    //document.getElementById(DivCurrentSlider).addEventListener("mouseup", mouseUpCurrentSlider);
    window.addEventListener('mousemove', mouseMove);
    window.addEventListener('mouseup', mouseUp);

    $('#divDropVideo').on("dragenter dragstart dragend dragleave dragover drag drop", (e) => {
        e.preventDefault();
    });

    document.getElementById(VdProcess).ontimeupdate = (e) => { ontimeUpdate(e); };

    _sliderWrap = document.getElementById(DivSliderWrap);

    // Clicking 'Save' events
    document.getElementById(BtnUploadVideo).addEventListener("click", () => { SaveProcessVideo(); });
    //document.getElementById(BtnSaveVideo).addEventListener("click", () => { SaveProcessVideo(); });

    $(JsDivRightVideoList).hide(); // Hiding right short video list by default.

    // Hiding 'Operation Video' modal
    document.getElementById(BtnHideOpVideoModal).addEventListener("click", () => {
        $(JsVdpModal).hide("slide", { direction: "right" }, 1000);
        HideOpVideoModal = true;

        setTimeout(() => {
            //document.getElementById(DivRightVideoList).style.display = "flex";

            $(JsDivRightVideoList).show("slide", { direction: "right" }, 1000);
        }, 1000);

        const appElement = document.querySelector("[ng-app=opsApp]"),
            $scope = angular.element(appElement).scope();

        console.log(_videoOpdts);

        $scope.$apply(() => {
            $scope.OpVideo = _videoOpdts;
        });
    });

    document.getElementById(BtnCloseVdpModal).addEventListener("click", () => {
        HideOpVideoModal = false;
    });

    //document.getElementById(BtnCloseRightVdList).addEventListener("click", () => {
    //    console.log("Story...");
    //    document.getElementById(DivRightVideoList).style.display = "none";
    //});

    //$("#vdpModal").on("hidden.bs.modal", () => {
    //    console.log("hello world...");
    //    //document.getElementById(DivRightVideoList).style.display = "none";
    //    $(JsDivRightVideoList).show("slide", { direction: "left" }, 1000);
    //});
})();
//#endregion

//#region Processing video
function SaveProcessVideo() {
    const validVideos = _videoOpdts.filter(x => x.OpSerial && x.OpSerial !== "0");
    console.log(validVideos);
    if (validVideos && validVideos.length > 0) {
        UploadMainVideoToServer();
    } else {
        MsgInform("Information", "There is no selected process.", "error", false, true);
    }
}

function SplitVideoOp(inputFilePath, connectionId) {
    const validVideos = _videoOpdts.filter(x => x.OpSerial && x.OpSerial !== "0"),
        config = new AjaxConfig(UrlSplitOpdtVideos, true, JSON.stringify({ opdtVideos: validVideos, inputFilePath, connectionId }));

    return AjaxPostCommon(config, (res) => {
        console.log(res);
        if (res.IsSuccess === false) {
            MsgInform("Error", "An error occured, please try again or contact admin.", "error", false, true);
        } else {
            for (var r of res) {
                const n = toolkit.getNode(r.Result.OpSerial);

                // Updating video icon and video link of process layout.
                n.data.ShowButtonPlayVideo = true;
                toolkit.updateNode(n);
            }
            document.getElementById(DivGlobe).style.display = "none";
        }
    });
}

function SendCroppedOpdtVideoInfo(inputFilePath, connectionId) {
    //$.blockUI({ message: "<h3>Splitting video...</h3>" });
    const validVideos = _videoOpdts.filter(x => x.OpSerial && x.OpSerial !== "0"),
        clientId = uuidv4();

    return $.post(UrlSplitOpdtVideos, { opdtVideos: validVideos, inputFilePath, connectionId }).
        done((res) => {
            console.log(res);
            if (res.IsSuccess === false) {
                MsgInform("Error", "An error occured, please try again or contact admin.", "error", false, true);
            } else {
                //let msgContent = "📘 Splitting result<br>";
                for (var r of res) {
                    const /*status = r.IsSuccess ? "Success" : "Failure",*/
                        //videoFullPath = r.Result.VideoFullPath ? r.Result.VideoFullPath : "",
                        n = toolkit.getNode(r.Result.OpSerial);

                    //msgContent += `🔷 Process: ${r.Result.DisplayName}<br>➡ Status: ${status}<br>➡ Video link: ${
                    //    videoFullPath}<br>`;

                    // Updating video icon and video link of process layout.
                    n.data.ShowButtonPlayVideo = true;
                    toolkit.updateNode(n);
                }

                //console.log(window.toolkit);

                //MsgInform("Information", msgContent, "info", false, false);
                //document.getElementById(SpMsgContent).innerHTML = msgContent;
                //$('#modalMessage').modal("show");
                document.getElementById(DivGlobe).style.display = "none";
            }
            //$.unblockUI();
        }).fail((xhr, status, err) => {
            HandleException(xhr, status, err);
        });
}

function UploadMainVideoToServer() {
    const guid = uuidv4(),
        fileUpload = $('#inputUploadVideo')[0].files;

    // Checking file length
    if (fileUpload.length === 0) {
        //$.unblockUI();
        //document.getElementById("div-loading").style.display = "none";
        console.log("No file to upload");
    } else {
        const files = fileUpload[0], chunkFile = [], bufferChunkSize = 3 * (1024 * 1024),
            currentOpmt = GetSelectedOneRowData(gridOpsTableId);
        let fileStreamPos = 0, endPos = bufferChunkSize;
        const fileSize = files.size;

        // Adding to the FileChunk array until we get to the end of the file
        while (fileStreamPos < fileSize) {
            // "slice" the file from the starting position/offset, to the required length
            chunkFile.push(files.slice(fileStreamPos, endPos));
            fileStreamPos = endPos; // jump by the amount read
            endPos = fileStreamPos + bufferChunkSize; // set next chunk length
        }

        // Getting total number of "files" we will be sending
        const totalParts = chunkFile.length, reqPromises = [];
        let partCount = 0, partRes = [], videoServerUrl;

        $.blockUI({
            fadeIn: 1000,
            message: "<h3>Uploading video to server...</h3>",
            onBlock: () => {
                // Looping through, pulling the first item from the array each time and sending it
                while (chunkFile.length > 0) {
                    partCount++;

                    const chunk = chunkFile.shift(),
                        filePartName = files.name + ".part_" + partCount + "." + totalParts,
                        dataForm = new FormData();

                    dataForm.append(files.name, chunk);
                    dataForm.append("FileName", files.name);
                    dataForm.append("FilePartName", filePartName);
                    dataForm.append("guid", guid);
                    dataForm.append("Edition", currentOpmt.Edition2);
                    dataForm.append("StyleCode", currentOpmt.StyleCode);
                    dataForm.append("StyleSize", currentOpmt.StyleSize);
                    dataForm.append("StyleColorSerial", currentOpmt.StyleColorSerial);
                    dataForm.append("RevNo", currentOpmt.RevNo);
                    dataForm.append("OpRevNo", currentOpmt.OpRevNo);

                    const reqUpload = UploadChunkFile(dataForm, (res) => {
                        console.log(res);
                        partRes.push(res);

                        if (res && res.IsSuccess && res.Result) {
                            console.log("Done uploading video and file is merged.");
                            console.log(res.Result);

                            videoServerUrl = res.Result;

                            // Now sending list of range of time and opdts to split video.
                            //SendCroppedOpdtVideoInfo(res.Result);
                        }
                    });

                    reqPromises.push(reqUpload);
                }

                $.when.apply($, reqPromises).done(() => {
                    $.unblockUI();
                    //document.getElementById("div-loading").style.display = "none";

                    console.log("All threads completed");
                    MsgInform("Information", "Splitting video...\n We will notify once finish.", "info", false, true);
                    //console.log(partRes);

                    document.getElementById(DivGlobe).style.display = "block";

                    // Now sending list of range of time and opdts to split video.
                    if (videoServerUrl) {
                        const chat = $.connection.opsVideoHub;

                        console.log($.connection);
                        console.log(chat);

                        // Client-side sendMessage function that will be called from the server-side.
                        chat.client.sendMessage = (message, count) => {
                            console.log(message);
                            console.log(`Number of video: ${count}`);

                            const vdpAlerts = document.getElementById("vdpAlerts"),
                                alertTag = createAlertTag(message);

                            vdpAlerts.appendChild(alertTag);
                            setTimeout(() => { alertTag.remove(); }, 5000);
                        };

                        // Starting the connection.
                        $.connection.hub.start().done(() => {
                            console.log("SignalR is started.");
                            chat.server.getConnectionId().done((connectionId) => {
                                SplitVideoOp(videoServerUrl, connectionId);
                            });
                        });
                    }
                });
            }
        });
    }
}

function UploadChunkFile(dataForm, callBack) {
    const request = $.ajax({
        url: UrlUploadChunkVideos,
        async: true,
        type: "POST",
        data: dataForm,
        contentType: false,
        processData: false,
        success: (result) => {
            callBack(result);
        },
        beforeSend: () => {
            //$.blockUI({ message: "<h3>Uploading video...</h3>" });
            //document.getElementById("div-loading").style.display = "block";
        },
        complete: () => {
            //$.unblockUI();
        },
        error: (err) => {
            console.log(err);
        }
    });

    return request;
}

function createProcessDropdownList1(data, partNo) {
    const selProcess = document.createElement("select"),
        empOpt = document.createElement("option");
    empOpt.setAttribute("value", 0);
    empOpt.innerHTML = "Select process!";
    selProcess.appendChild(empOpt);
    selProcess.id = `selP${partNo}`;

    for (var p of data) {
        const opt = document.createElement("option"),
            opName = p.OpNameLan ? p.OpNameLan : p.OpName,
            opNum = p.OpNum ? `[${p.OpNum}]` : "[]";
        opt.setAttribute("value", p.OpSerial);
        opt.innerHTML = `${opNum} ${opName}`;
        selProcess.appendChild(opt);
    }

    return selProcess;
}

function createRightVideoList(videoOpdt) {
    const row = document.createElement("div"),
        col9 = document.createElement("div"),
        col3 = document.createElement("div"),
        p = document.createElement("div"),
        pNo = document.createElement("div"),
        d = document.createElement("div"),
        divTotalTime = document.createElement("div"),
        spTotalTime = document.createElement("span"),
        vd = document.createElement("video"),
        btnRemove = document.createElement("button"),
        spRemove = document.createElement("span"),
        vdRow = document.createElement("div"),
        divSelect = document.createElement("div"),
        btnInsertProcess = document.createElement("button");

    row.classList.add("row", "vdp-modal--each-vd");
    divSelect.classList.add("row");
    spTotalTime.classList.add("op__total-time");

    const drpProcess = createProcessDropdownList(window.CurrentOpdts, videoOpdt.PartNo),
        spSelectedTextId = `sp${videoOpdt.PartNo}`;
    divSelect.appendChild(drpProcess);

    $(drpProcess).multiselect({
        enableFiltering: true,
        enableCaseInsensitiveFiltering: true,
        maxHeight: 300,
        buttonWidth: '200px',
        onChange: (selectedOption) => {
            //console.log(selectedOption[0]);
            const selectedOpdt = _videoOpdts.find(x => x.OpSerial === selectedOption[0].value.toString());

            if (selectedOpdt) {
                MsgInform("Alert", `This process was selected for video number: ${selectedOpdt.PartNo}`, "error", true, true);
                //this.multiselect('deselectAll', true);
                if (selectedOpdt.OpSerial) {
                    console.log(selectedOpdt.OpSerial);
                    //this.multiselect('select', selectedOpdt.OpSerial, true);
                    //document.getElementById(spSelectedTextId).innerHTML = selectedOption[0].text;
                } else {
                    //this.multiselect('select', 0, true);
                    document.getElementById(spSelectedTextId).innerHTML = "Select process!";
                    document.getElementById(spSelectedTextId).style.cssText = "color: red !important";
                }
                //console.error(`This processes was selected for video: ${selectedOpdt.PartNo}`);
            } else {
                videoOpdt.OpSerial = selectedOption[0].value.toString();
                videoOpdt.DisplayName = selectedOption[0].text;

                document.getElementById(spSelectedTextId).innerHTML = selectedOption[0].text;

                if (selectedOption[0].value.toString() === "0") {
                    document.getElementById(spSelectedTextId).style.cssText = "color: red !important";
                } else {
                    document.getElementById(spSelectedTextId).style.cssText = "color: #000 !important";
                }
                console.log(selectedOption[0].value);
            }
        },
        templates: {
            button: `<span id=${spSelectedTextId} class="multiselect dropdown-toggle vdp__drp-text" data-toggle="dropdown">Select process!</span>`
        }
    });

    row.appendChild(vdRow);
    row.appendChild(divSelect);

    btnInsertProcess.classList.add("glyphicon", "glyphicon-plus-sign", "op__btn--add-process");
    //row.appendChild(btnInsertProcess);

    vd.classList.add("op-vdp__video");
    col9.classList.add("col-md-9");
    col3.classList.add("col-md-3");
    p.classList.add("row");
    d.classList.add("row");
    divTotalTime.classList.add("row");

    divTotalTime.innerHTML = "Total time: ";
    spTotalTime.innerHTML = new Date(videoOpdt.TotalTime * 1000).toISOString().substr(11, 8);
    divTotalTime.appendChild(spTotalTime);

    btnRemove.classList.add("vdp__btn-close-part-video");
    //spRemove.innerHTML = "❌";

    d.innerHTML = `${videoOpdt.StartTime} - ${videoOpdt.EndTime}`;
    pNo.innerHTML = `${videoOpdt.PartNo}`;
    pNo.classList.add("op__part-no");
    p.appendChild(pNo);

    btnRemove.addEventListener("click", () => {
        ConfirmYesNo("Confirmation", "Are you sure to cancel this video?", () => {
            row.remove();
            const vdRows = document.getElementsByClassName("vdp-modal--each-vd");
            if (vdRows && vdRows.length === 0) _partVideo = 0;

            // Removing item from list selected video-processes.
            const pos = _videoOpdts.map((e) => e.PartNo).indexOf(videoOpdt.PartNo);
            if (pos > -1) _videoOpdts.splice(pos, 1);

            // Removing part bar by part number.
            const divPart = document.getElementById(`divPart${videoOpdt.PartNo}`);
            if (divPart) divPart.remove();
        }, () => { });
    });

    if (_currentVideoSrc) vd.setAttribute("src", _currentVideoSrc);
    vd.setAttribute("controls", "controls");
    vd.currentTime = convertToSecond(videoOpdt.StartTime);
    vd.ontimeupdate = () => {
        if (vd.currentTime >= convertToSecond(videoOpdt.EndTime)) vd.pause();
    };

    col9.appendChild(vd);
    col3.appendChild(p);
    col3.appendChild(d);
    col3.appendChild(divTotalTime);

    btnRemove.appendChild(spRemove);
    vdRow.appendChild(btnRemove);
    vdRow.appendChild(col9);
    vdRow.appendChild(col3);

    return row;
}

function moveSlidersToNewRangeTime(videoOpdt) {
    const endedSliderLeft = _sliderRight.style.left,
        endedSliderLeftFloat = parseFloat(endedSliderLeft.replace(/%/gi, ""));

    //if (!_limitedCropping) {
    // Calculating position of ending slider by default time(s).
    const barLeft = _sliderLeft.style.left,
        //startedSliderLeft = parseFloat(barLeft.replace(/%/gi, "")),
        //startedDuration = _vidTag.duration / 100 * startedSliderLeft,
        defaultSecond = _txtCropSec.value,
        lastEndedDuration = _vidTag.duration / 100 * endedSliderLeftFloat;
    let newEndedDuration = parseInt(defaultSecond) + lastEndedDuration,
        newEndedPercent = newEndedDuration / _vidTag.duration * 100;

    if (endedSliderLeftFloat < 100) {
        if (parseInt(defaultSecond) < 1) {
            MsgInform("Error", "Invalid splitting second!", "error", false, true);
        } else {
            console.log(_spSplitBar.style.width);
            _sliderLeft.style.left = endedSliderLeft;

            if (newEndedDuration > _vidTag.duration) newEndedDuration = _vidTag.duration;
            // Moving current time of video to starting slider.
            _vidTag.currentTime = lastEndedDuration;
            //console.log(lastEndedDuration);

            //const splittingBarLength = _spSplitBar.style.width;
            //console.log(splittingBarLength);

            if (newEndedPercent > 100) newEndedPercent = 100;

            // Positioning controls.
            _sliderRight.style.left = `${newEndedPercent}%`;
            _spSplitBar.style.left = endedSliderLeft;
            _circleSlider.style.left = endedSliderLeft;

            // Assigning time to textboxes.
            const startTimeStr = new Date(lastEndedDuration * 1000).toISOString(),
                endTimeStr = new Date(newEndedDuration * 1000).toISOString();
            _startH.value = startTimeStr.substr(11, 2);
            _startM.value = startTimeStr.substr(14, 2);
            _startS.value = startTimeStr.substr(17, 2);
            _endH.value = endTimeStr.substr(11, 2);
            _endM.value = endTimeStr.substr(14, 2);
            _endS.value = endTimeStr.substr(17, 2);

            const startedDur = _vidTag.duration / 100 * parseFloat(barLeft.replace(/%/gi, "")),
                splitControl = new SplitControl(videoOpdt.Edition, videoOpdt.StyleCode, videoOpdt.StyleColorSerial,
                    videoOpdt.StyleSize, videoOpdt.RevNo, videoOpdt.OpRevNo, videoOpdt.OpSerial, videoOpdt.StartTime,
                    videoOpdt.EndTime, videoOpdt.PartNo, videoOpdt.DisplayName, barLeft, endedSliderLeft,
                    _spSplitBar.style.width, startedDur);

            console.log(splitControl);

            // Adding part bar.
            document.getElementById(DivBarPart).appendChild(addPartBar(splitControl));

            // Assigning width to slitting bar.
            const sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
                sliderRightBcr = _sliderRight.getBoundingClientRect(),
                divSliderWrapWidth = _sliderWrap.clientWidth,
                splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
            _spSplitBar.style.width = `${splitBarPercent}%`;

            //videoOpdt.TotalTime = lastEndedDuration - startedDuration;
            console.log(videoOpdt);

            // Adding cropped video to right list area.
            document.getElementById(DivVideoRightList).appendChild(createRightVideoList(videoOpdt));
        }
    } else {
        //MsgInform("Information", "Limited time", "error", false, true);
        _limitedCropping = true;

        const startedDur = _vidTag.duration / 100 * parseFloat(barLeft.replace(/%/gi, "")),
            splitControl = new SplitControl(videoOpdt.Edition, videoOpdt.StyleCode, videoOpdt.StyleColorSerial,
                videoOpdt.StyleSize, videoOpdt.RevNo, videoOpdt.OpRevNo, videoOpdt.OpSerial, videoOpdt.StartTime,
                videoOpdt.EndTime, videoOpdt.PartNo, videoOpdt.DisplayName, barLeft, endedSliderLeft,
                _spSplitBar.style.width, startedDur);

        // Adding part bar.
        document.getElementById(DivBarPart).appendChild(addPartBar(splitControl));

        // Total time of split video.
        //videoOpdt.TotalTime = lastEndedDuration - startedDuration;

        // Adding cropped video to right list area.
        document.getElementById(DivVideoRightList).appendChild(createRightVideoList(videoOpdt));
    }
    //} else {
    //    MsgInform("Information", "Limited time", "error", false, true);
    //}
}

function moveSplittingControls(startedSplittingLeft, endedSplittingLeft, splittingBarWidth, startedDuration, startTimeStr, endTimeStr) {
    console.log(startedSplittingLeft);

    // Assigning position to started splitting.
    _sliderLeft.style.left = startedSplittingLeft;

    // Moving current time of video to starting slider.
    _vidTag.currentTime = startedDuration;

    // Positioning controls.
    _sliderRight.style.left = endedSplittingLeft;
    _spSplitBar.style.left = startedSplittingLeft;
    //_circleSlider.style.left = startedSplittingLeft;

    // Assigning width to slitting bar.
    _spSplitBar.style.width = splittingBarWidth;

    // Assigning time to textboxes.
    _startH.value = startTimeStr.substr(0, 2);
    _startM.value = startTimeStr.substr(3, 2);
    _startS.value = startTimeStr.substr(6, 2);
    _endH.value = endTimeStr.substr(0, 2);
    _endM.value = endTimeStr.substr(3, 2);
    _endS.value = endTimeStr.substr(6, 2);
}

function addPartBar(splitControl) {
    const divPartBar = document.createElement("div");
    divPartBar.id = `divPart${splitControl.PartNo}`;

    // Part bar clicking event.
    divPartBar.addEventListener("click", () => {
        moveSplittingControls(splitControl.StartedSplittingLeft, splitControl.EndedSplittingLeft, splitControl.SplittingBarWidth,
            splitControl.StartedDuration, splitControl.StartTime, splitControl.EndTime);
    });

    if (colorIndex === BarColour.length) colorIndex = 0;
    divPartBar.style.backgroundColor = BarColour[colorIndex];
    colorIndex++;

    divPartBar.style.left = splitControl.StartedSplittingLeft;
    divPartBar.style.width = splitControl.SplittingBarWidth;
    divPartBar.style.cursor = "pointer";
    divPartBar.innerHTML = splitControl.PartNo;
    return divPartBar;
}

function cropVideo() {
    if (validateStartEndTime(true)) {
        console.log("Crop video...");
        _partVideo++;
        const totalSecond = parseInt(_endH.value) * 3600 + parseInt(_endM.value) * 60 + parseInt(_endS.value) - (parseInt(_startH.value) * 3600 + parseInt(_startM.value) * 60 + parseInt(_startS.value)),
            currentOpmt = GetSelectedOneRowData(gridOpsTableId);

        console.log(totalSecond);

        let videoOpdt = new VideoOpdt(currentOpmt.Edition, currentOpmt.StyleCode, currentOpmt.StyleColorSerial,
            currentOpmt.StyleSize, currentOpmt.RevNo, currentOpmt.OpRevNo, null,
            `${_startH.value}:${_startM.value}:${_startS.value}`, `${_endH.value}:${_endM.value}:${_endS.value}`,
            _partVideo, null, totalSecond, new Date(totalSecond * 1000).toISOString().substr(11, 8));

        _videoOpdts.push(videoOpdt);

        // Moving sliders and adding cropped video to right
        moveSlidersToNewRangeTime(videoOpdt);
    }
}
//#endregion

//#region Generic methods
function dragStartVideo(e) {
    console.log(e.currentTarget.innerHTML);
    event.dataTransfer.setData("VideoNo", e.currentTarget.innerHTML);

    //const n = toolkit.getNode(e.currentTarget.innerHTML),
    //    currentVideo = $(`[data-jtk-node-id="${e.currentTarget.innerHTML}"]`);

    //console.log(currentVideo);
    //console.log(n);
    //console.log(n.data);

    //alert("dragStartVideo...");
}

function allowDropVideo(event) {
    event.preventDefault();
    console.log("Allowing dropping video");
}

function dropVideo(event) {
    event.preventDefault();
    const videoNo = event.dataTransfer.getData("VideoNo"),
        dragVideo = _videoOpdts.find(x => x.PartNo.toString() === videoNo),
        opSerial = event.currentTarget.id.substr(4),
        selectedOpdt = _videoOpdts.find(x => x.OpSerial === opSerial),
        spOpName = document.getElementById(`spOpName${opSerial}`);

    if (selectedOpdt) {
        MsgInform("Alert", `This process was selected for video number: ${selectedOpdt.PartNo}`, "error", true, true);
    } else {
        dragVideo.OpSerial = opSerial;
        dragVideo.DisplayName = spOpName.innerHTML;

        console.log(`Video number: ${videoNo}`);
        console.log(`OpSerial: ${opSerial}`);
        console.log($(`#selP${videoNo}`));

        //$(`#selP${videoNo}`).multiselect('refresh');
        $(`#selP${videoNo}`).multiselect('deselectAll', false);
        $(`#selP${videoNo}`).multiselect('select', [opSerial.toString()]);

        //alert(`Selected ${[opSerial.toString()]}.`);
        //$(`#selP${videoNo}`).multiselect('updateButtonText');

        $(`#selP${videoNo}`).multiselect('refresh');

        const vdpAlerts = document.getElementById("vdpAlerts"),
            alertTag = createInformTag(`&nbsp;Assigned video number '${videoNo}' to process: ${spOpName.innerHTML}`);

        vdpAlerts.appendChild(alertTag);
        setTimeout(() => { alertTag.remove(); }, 5000);
    }
}

function droppableProcessNodes() {
    const processNodes = toolkit.getNodes();
    for (var pn of processNodes) {
        const processNode = $(`[data-jtk-node-id="${pn.data.id}"]`);
        if (processNode[0]) {
            //$(processNode[0]).droppable({
            //    drop: (event, ui) => {
            //        console.log(event);
            //        console.log(ui);
            //    }
            //});
            $(processNode[0]).ondrop = (e) => {
                e.preventDefault();
                console.log("drop a video...");
            };

            $(processNode[0]).ondragover = (e) => {
                e.stopPropagation();
                e.preventDefault();
            };
        } else {
            console.log(processNode);
        }
    }
}

function createInformTag(msg) {
    const d = document.createElement("div"),
        btn = document.createElement("button"),
        spClose = document.createElement("span"),
        spIcon = document.createElement("span"),
        spMsg2 = document.createElement("span");

    d.classList.add("alert", "alert-success", "alert-dismissible", "fadeIn");
    d.width = "100%";
    d.style.borderWidth = 0;
    d.setAttribute("role", "alert");

    btn.setAttribute("type", "button");
    btn.setAttribute("data-dismiss", "alert");
    btn.setAttribute("aria-label", "Close");
    btn.classList.add("close");
    btn.style.top = "-10px !important";

    spClose.setAttribute("aria-hidden", "true");
    spClose.innerHTML = "&times;";
    btn.appendChild(spClose);
    d.appendChild(btn);

    spIcon.classList.add("glyphicon", "glyphicon-ok");
    d.appendChild(spIcon);

    spMsg2.innerHTML = msg;
    d.appendChild(spMsg2);

    return d;
}

function createAlertTag(msg) {
    const d = document.createElement("div"),
        btn = document.createElement("button"),
        spClose = document.createElement("span"),
        spIcon = document.createElement("span"),
        str = document.createElement("strong"),
        br1 = document.createElement('br'),
        spMsg1 = document.createElement("span"),
        br2 = document.createElement('br'),
        spMsg2 = document.createElement("span");

    d.classList.add("alert", "alert-success", "alert-dismissible", "fadeIn");
    d.width = "100%";
    d.style.borderWidth = 0;
    d.setAttribute("role", "alert");

    btn.setAttribute("type", "button");
    btn.setAttribute("data-dismiss", "alert");
    btn.setAttribute("aria-label", "Close");
    btn.classList.add("close");
    btn.style.top = "-10px !important";

    spClose.setAttribute("aria-hidden", "true");
    spClose.innerHTML = "&times;";
    btn.appendChild(spClose);
    d.appendChild(btn);

    spIcon.classList.add("glyphicon", "glyphicon-ok");
    d.appendChild(spIcon);

    str.innerHTML = "&nbsp; Successfully!";
    d.appendChild(str);
    d.appendChild(br1);

    spMsg1.innerHTML = "Saved video for process";
    d.appendChild(spMsg1);
    d.appendChild(br2);

    spMsg2.innerHTML = msg;
    d.appendChild(spMsg2);

    return d;
}

function validateStartEndTime(isShowMsg) {
    if (startTimeToSecond() >= endTimeToSecond()) {
        if (isShowMsg) MsgInform("Error", "Ending time must be greater than starting time.", "error", false, true);
        return false;
    }
    if (startTimeToSecond() >= _vidTag.duration) {
        if (isShowMsg) MsgInform("Error", "Invalid starting time.", "error", false, true);
        return false;
    }
    if (endTimeToSecond() >= _vidTag.duration) {
        if (isShowMsg) MsgInform("Error", "Invalid ending time.", "error", false, true);
        return false;
    }

    return true;
}

function startTimeToSecond() {
    return parseInt(_startH.value) * 3600 + parseInt(_startM.value) * 60 + parseInt(_startS.value);
}

function endTimeToSecond() {
    return parseInt(_endH.value * 3600) + parseInt(_endM.value) * 60 + parseInt(_endS.value);
}

function convertToSecond(timeStr) {
    const h = timeStr.substr(0, 2),
        m = timeStr.substr(3, 2),
        s = timeStr.substr(6, 2);

    return parseInt(h) * 3600 + parseInt(m) * 60 + parseInt(s);
}
//#endregion Generic methods

//#region Start, End time
function changeTimeValue(e) {
    const t = parseInt(e.currentTarget.value);
    //switch (t) {
    //    case t < 0:
    //        e.currentTarget.value = `0${t.toString().replace(/-/gi, "")}`;
    //        break;
    //    case t >= 0 && t < 60:
    //        e.currentTarget.value = t.toString().length === 1 ? `0${t}` : t;
    //        break;
    //    case t >= 60:
    //        e.currentTarget.value = "59";
    //        break;
    //    default:
    //        e.currentTarget.value = "00";
    //        break;
    //}

    if (t) {
        if (t < 0) {
            e.currentTarget.value = `0${t.toString().replace(/-/gi, "")}`;
        } else {
            if (e.currentTarget.id === StartH || e.currentTarget.id === EndH) {
                if (t < 24) {
                    e.currentTarget.value = t.toString().length === 1 ? `0${t}` : t;
                } else {
                    e.currentTarget.value = "23";
                }
            } else {
                if (t < 60) {
                    e.currentTarget.value = t.toString().length === 1 ? `0${t}` : t;
                } else {
                    e.currentTarget.value = "59";
                }
            }
        }
    } else {
        e.currentTarget.value = "00";
    }

    if (e.currentTarget.id === StartH || e.currentTarget.id === StartM || e.currentTarget.id === StartS) {
        if (validateStartEndTime(false)) {
            _isStartTimeChange = true;

            //console.log(startTimeToSecond());

            _vidTag.currentTime = startTimeToSecond();
        }
    }

    if (e.currentTarget.id === EndH || e.currentTarget.id === EndM || e.currentTarget.id === EndS) {
        if (validateStartEndTime(false)) {
            _isEndTimeChange = true;

            //console.log(endTimeToSecond());
            _vidTag.currentTime = endTimeToSecond();
        }
    }
}
//#endregion Start, End time

//#region Sliding tags events
function mouseDownSliderLeft() {
    console.log("Allow moving left slider");
    _isSlideLeft = true;
    _isSlide = true;
}

function mouseDownSliderRight() {
    console.log("Allow moving right slider");
    _isSlideRight = true;
    _isSlide = true;
}

function oneSecondToPx(width) {
    const totalTime = document.getElementById(VdProcess).duration,
        px = width / totalTime;

    return px;
}

function getCurrentTime(sliderLeft) {
    const sliderWrapBcr = _sliderWrap.getBoundingClientRect(),
        percentLeft = sliderLeft / sliderWrapBcr.width * 100,
        totalTime = document.getElementById(VdProcess).duration,
        currentTime = percentLeft.toFixed(2) / 100 * totalTime;

    return currentTime;
}

function mouseMove(e) {
    const sliderWrapBcr = _sliderWrap.getBoundingClientRect(),
        circleSliderBcr = _circleSlider.getBoundingClientRect();

    //console.log("Moving event");

    //console.log(circleSliderBcr.width / 2);

    if (e.clientX >= sliderWrapBcr.left - circleSliderBcr.width / 2
        && e.clientX <= sliderWrapBcr.width + sliderWrapBcr.left + circleSliderBcr.width / 2) {
        const x = e.clientX - sliderWrapBcr.left,
            l = x / sliderWrapBcr.width * 100,
            sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            sliderRightBcr = _sliderRight.getBoundingClientRect();

        if (_isSlide && !_isSlideLeft && !_isSlideRight) {
            //circleSlider = document.getElementById(DivCurrentSlider);
            _circleSlider.style.left = `${l}%`;
        }

        if (_isSlideLeft) {
            //const x = e.clientX - sliderWrapBcr.left,
            //    sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            //    sliderRightBcr = _sliderRight.getBoundingClientRect();
            //l = x / sliderWrapBcr.width * 100;

            if (sliderRightBcr.x + sliderRightBcr.width / 2 - e.clientX > oneSecondToPx(sliderWrapBcr.width)) {
                //console.log(sliderLeftBcr);
                //console.log(_sliderRight.getBoundingClientRect());
                //console.log(e.pageX);
                //console.log(e.clientX);

                _isMsgBoxShow = false;
                _msgBox.style.display = "none";

                _sliderLeft.style.left = `${l}%`;
                _spSplitBar.style.left = `${l}%`;

                // Scaling splitting bar
                const divSliderWrapWidth = _sliderWrap.clientWidth,
                    splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
                _spSplitBar.style.width = `${splitBarPercent}%`;

                // Positioning circle slider
                _circleSlider.style.left = `${l}%`;

                //console.log("Moving starting slider");
            } else {
                _isMsgBoxShow = true;
                _msgBox.style.display = "block";
            }
        }

        if (_isSlideRight) {
            //const x = e.clientX - sliderWrapBcr.left,
            //    sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            //    sliderRightBcr = _sliderRight.getBoundingClientRect();
            //l = x / sliderWrapBcr.width * 100;

            if (e.clientX - (sliderLeftBcr.x + sliderLeftBcr.width / 2) > oneSecondToPx(sliderWrapBcr.width)) {
                //console.log(e.clientX - sliderLeftBcr.x);

                _isMsgBoxShow = true;
                _msgBox.style.display = "none";
                _sliderRight.style.left = `${l}%`;

                // Scaling splitting bar
                //_spSplitBar.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
                const divSliderWrapWidth = _sliderWrap.clientWidth,
                    splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
                _spSplitBar.style.width = `${splitBarPercent}%`;

                // Positioning circle slider
                _circleSlider.style.left = `${l}%`;

                //console.log("Moving end slider");
            } else {
                _msgBox.style.display = "block";
            }
        }

        if (_isSlide) {
            console.log("moving current slider");
            const percentLeft = _circleSlider.style.left.replace(/%/gi, ""),
                curTime = percentLeft / 100 * _vidTag.duration;

            //_isSlide = false;

            document.getElementById(VdProcess).currentTime = curTime;
        }
    }
}

function mouseUp() {
    if (_isSlide) {
        //const percentLeft = _circleSlider.style.left.replace(/%/gi, ""),
        //    curTime = percentLeft / 100 * _vidTag.duration;

        _isSlide = false;

        //document.getElementById(VdProcess).currentTime = curTime;
    }

    if (_isSlideLeft) _isSlideLeft = false;
    if (_isSlideRight) _isSlideRight = false;
}

function mouseDownCurrentSlider() {
    _isSlide = true;
}
//#endregion Sliding tags events

//#region Video tag events
function ontimeUpdate(e) {
    const currentTimeStr = new Date(e.currentTarget.currentTime * 1000).toISOString(),
        timePercent = e.currentTarget.currentTime / e.currentTarget.duration * 100;

    document.getElementById(SpCurentTime).innerHTML = currentTimeStr.substr(11, 8);
    _circleSlider.style.left = `${timePercent}%`;

    console.log("timePercent");
    console.log(timePercent);

    if (_isSlideLeft) {
        //_isSlideLeft = false;
        _sliderLeft.style.left = `${timePercent}%`;
        _spSplitBar.style.left = `${timePercent}%`;
        _startH.value = currentTimeStr.substr(11, 2);
        _startM.value = currentTimeStr.substr(14, 2);
        _startS.value = currentTimeStr.substr(17, 2);

        const sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            sliderRightBcr = _sliderRight.getBoundingClientRect();

        //_spSplitBar.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        _spSplitBar.style.width = `${splitBarPercent}%`;
    }
    if (_isSlideRight) {
        //_isSlideRight = false;
        _sliderRight.style.left = `${timePercent}%`;
        _endH.value = currentTimeStr.substr(11, 2);
        _endM.value = currentTimeStr.substr(14, 2);
        _endS.value = currentTimeStr.substr(17, 2);

        const sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            sliderRightBcr = _sliderRight.getBoundingClientRect();

        _spSplitBar.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        _spSplitBar.style.width = `${splitBarPercent}%`;
    }

    if (_isStartTimeChange) {
        _isStartTimeChange = false;
        _sliderLeft.style.left = `${timePercent}%`;
        _spSplitBar.style.left = `${timePercent}%`;

        const sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            sliderRightBcr = _sliderRight.getBoundingClientRect();

        //_spSplitBar.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        _spSplitBar.style.width = `${splitBarPercent}%`;
    }
    if (_isEndTimeChange) {
        _isEndTimeChange = false;
        _sliderRight.style.left = `${timePercent}%`;

        const sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            sliderRightBcr = _sliderRight.getBoundingClientRect();

        //_spSplitBar.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        _spSplitBar.style.width = `${splitBarPercent}%`;
    }
}

function playVideo(e) {
    e.currentTarget.classList.toggle("video-pause");
    const vid = document.getElementById(VdProcess);
    if (vid.paused) {
        vid.play();
    } else {
        vid.pause();
    }
}

function ClearAllVideoInfo() {
    // Clearing list of process videos information.
    _videoOpdts = [];
    _partVideo = 0;
    colorIndex = 0;
    //_limitedCropping = false;

    document.getElementById(VdProcess).src = "";
    document.getElementById(DivVideoTag).style.cssText = "display:none !important";
    document.getElementById(DivLoadVideo).style.cssText = "display:block !important";
    document.getElementById(InputUploadVideo).value = "";

    document.getElementById(DivVideoRightList).innerHTML = "";

    // Hiding process video button
    document.getElementById(DivProcessVideo).style.cssText = "display:none !important";

    // Clearing part div
    document.getElementById(DivBarPart).innerHTML = "";
}

function closeVdPreview() {
    ConfirmYesNo("Confirmation", "Are you sure to cancel this video?", () => {
        ClearAllVideoInfo();
    }, () => { });
}

function dragOverDivVideo(e) {
    //e.preventDefault();
    e.target.style.backgroundColor = "dodgerblue";
    e.target.style.borderColor = "#fff";
    e.target.style.color = "white";
    e.dataTransfer.setData("text/plain", e.target.id);
    console.log(e.currentTarget.files);
}

function dropDivVideo(e) {
    //e.preventDefault();

    const data = e.dataTransfer.getData("text/plain");
    console.log(data);
    console.log(e.currentTarget.files);
}

function setDefaultSplitTime(dur) {
    const startTime = new Date(dur / 100 * _sliderLeft.style.left.replace(/%/gi, "") * 1000).toISOString(),
        endTime = new Date(dur / 100 * _sliderRight.style.left.replace(/%/gi, "") * 1000).toISOString();

    _startH.value = startTime.substr(11, 2);
    _startM.value = startTime.substr(14, 2);
    _startS.value = startTime.substr(17, 2);
    _endH.value = endTime.substr(11, 2);
    _endM.value = endTime.substr(14, 2);
    _endS.value = endTime.substr(17, 2);
}

function posSplitControlsByDefaultSecond() {
    const defaultSecond = _txtCropSec.value;

    if (parseInt(defaultSecond) < 1) {
        MsgInform("Error", "Invalid default second!", "error", false, true);
    } else {
        let percentTime = defaultSecond / _vidTag.duration * 100;
        console.log(percentTime);
        percentTime = percentTime > 100 ? 100 : percentTime;
        console.log(percentTime);

        _sliderLeft.style.left = "0%";
        _spSplitBar.style.left = "0%";
        _circleSlider.style.left = "0%";
        _sliderRight.style.left = `${percentTime}%`;
        //_spSplitBar.style.left = `${percentTime}%`;

        const sliderLeftBcr = _sliderLeft.getBoundingClientRect(),
            sliderRightBcr = _sliderRight.getBoundingClientRect();

        //_spSplitBar.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        _spSplitBar.style.width = `${splitBarPercent}%`;

        console.log(divSliderWrapWidth);
        console.log(splitBarPercent);
    }
}

function onChangedInputVf(e) {
    ///<summary>On changed input video file</summary>

    // Draggable process nodes.
    //droppableProcessNodes();

    // Resetting video icon playing.
    const playIcon = document.getElementById(AVdPlay);
    playIcon.classList.remove("video-pause");
    playIcon.classList.add("video-preview");

    console.log(e.currentTarget.files);

    if (e.currentTarget.files.length === 0) return;
    const vdUrl = URL.createObjectURL(e.currentTarget.files[0]),
        video = document.getElementById(VdProcess);

    video.src = vdUrl;
    _currentVideoSrc = URL.createObjectURL(e.currentTarget.files[0]);

    //$.blockUI({ message: AjaxWaitMes });
    //$.blockUI(window.BlockUI);

    //fetch(vdUrl).then((response) => {
    //    return response.arrayBuffer();
    //}).then((videoData) => {
    //    console.log(videoData);
    //    _videoArrayBuffer = videoData;
    //    $.unblockUI();
    //});

    video.onloadeddata = () => {
        //$.unblockUI();
        console.log(video.duration);

        if (video.duration > 86400) {
            console.log("The video is too long.");
        } else {
            const totalTime = new Date(video.duration * 1000).toISOString().substr(11, 8);
            document.getElementById(SpTotalTime).innerHTML = totalTime;
            console.log(totalTime);

            document.getElementById(DivPartVideoSide).style.height = document.getElementById(DivVideoSide).clientHeight + "px";

            // Displaying video processing button
            document.getElementById(DivProcessVideo).style.cssText = "display:block !important";

            // Positioning ending splitting control by default splitting time (second).
            posSplitControlsByDefaultSecond();

            setDefaultSplitTime(video.duration);
        }
    };
    console.log(e.currentTarget.files[0]);

    document.getElementById(DivVideoTag).style.cssText = "display:block !important";
    document.getElementById(DivLoadVideo).style.cssText = "display:none !important";
}
//#endregion Video tag events

//#region Classes
class OpMaster extends StyleMaster {
    constructor(styleCode, styleColorSerial, styleSize, revNo, opRevNo) {
        super(styleCode, styleColorSerial, styleSize, revNo);
        this.OpRevNo = opRevNo;
    }
}

class VideoOpdt extends OpMaster {
    constructor(edition, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, startTime, endTime, partNo,
        displayName, totalTime, totalTimeStr) {
        super(styleCode, styleColorSerial, styleSize, revNo, opRevNo);
        this.Edition = edition;
        this.OpSerial = opSerial;
        this.StartTime = startTime;
        this.EndTime = endTime;
        this.PartNo = partNo;
        this.DisplayName = displayName;
        this.TotalTime = totalTime;
        this.TotalTimeStr = totalTimeStr;
    }
}

class SplitControl extends VideoOpdt {
    constructor(edition, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, startTime, endTime, partNo,
        displayName, startedSplittingLeft, endedSplittingLeft, splittingBarWidth, startedDuration) {
        super(edition, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, startTime, endTime, partNo, displayName);
        this.StartedSplittingLeft = startedSplittingLeft;
        this.EndedSplittingLeft = endedSplittingLeft;
        this.SplittingBarWidth = splittingBarWidth;
        this.StartedDuration = startedDuration;
    }
}
//#endregion Classes