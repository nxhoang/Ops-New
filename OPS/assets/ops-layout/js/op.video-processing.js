//#region Properties
const InputUploadVideo1 = "inputUploadVideo1",
    DivDropVideo1 = "divDropVideo1",
    LbUploadVideo1 = "lbUploadVideo1",
    VdProcess1 = "vdProcess1",
    JqVdProcess1 = "#vdProcess1",
    DivVideoTag1 = "divVideoTag1",
    DivLoadVideo1 = "divLoadVideo1",
    BtnCloseVdPreview1 = "btnCloseVdPreview1",
    AVdPlay1 = "aVdPlay1",
    SpTotalTime1 = "spTotalTime1",
    SpCurentTime1 = "spCurentTime1",
    DivCurrentSlider1 = "divCurrentSlider1",
    DivSliderWrap1 = "divSliderWrap1",
    SliderLeft1 = "sliderLeft1",
    SliderRight1 = "sliderRight1",
    SpSplitBar1 = "spSplitBar1",
    MsgBox1 = "msgBox1",
    BtnSplit1 = "btnSplit1",
    BtnUploadVideo1 = "btnUploadVideo1",
    BtnSaveVideo1 = "btnSaveVideo1",
    DivProcessVideo1 = "divProcessVideo1",
    DivVideoSide1 = "divVideoSide1",
    DivPartVideoSide1 = "divPartVideoSide1",
    DivBarPart1 = "divBarPart1",
    StartH1 = "startH1",
    StartM1 = "startM1",
    StartS1 = "startS1",
    EndH1 = "endH1",
    EndM1 = "endM1",
    EndS1 = "endS1",
    TxtCropSecond1 = "txtCropSecond1",
    SpMsgContent1 = "spMsgContent1",
    DivVideoRightList1 = "divVideoRightList1",
    DivGlobe1 = "divGlobe",
    BtnShowProcessVideoModal1 = "btnShowProcessVideoModal1",
    //BtnHideOpVideoModal1 = "btnHideOpVideoModal1",
    //BtnCloseVdpModal1 = "btnCloseVdpModal1",
    BtnCloseNvpModal = "btnCloseNvpModal",
    DivRightVideoList1 = "divRightVideoList1",
    JsDivRightVideoList1 = "#divRightVideoList1",
    ModalFixedVp = "modalFixedVp",
    JsModalFixedVp = $("#modalFixedVp"),
    JsModalFixedVp2 = $("#modalFixedVp2"),
    BtnCloseRightVdList1 = "btnCloseRightVdList1",
    UrlUploadChunkVideos1 = "/OpLayout/UploadVideoChunkFile",
    UrlSplitOpdtVideos1 = "/OpLayout/SplitOpdtVideos",
    BarColour1 = ["#FF0000", "#800000", "#6e6e04", "#2a0873", "#0e750e", "#096666"],
    DivVideoThumbnail = "divVideoThumbnail",
    BtnMinimizeNvpModal = "btnMinimizeNvpModal",
    AjaxWaitMes1 = "<h3>Please wait...</h3>"; // blockUI message;
var _modalFixedVp,
    _inputUploadVideo1,
    _isSlide1 = false,
    _isSlideLeft1 = false,
    _isSlideRight1 = false,
    _sliderWrap1,
    _sliderLeft1,
    _sliderRight1,
    _spSplitBar1,
    _txtCropSec1,
    _msgBox1,
    _isMsgBoxShow1 = false,
    _pxPerOneSecond1,
    _vidTag1,
    _circleSlider1,
    _startH1,
    _startM1,
    _startS1,
    _endH1,
    _endM1,
    _endS1,
    _btnSplit1,
    _isStartTimeChange1 = false,
    _isEndTimeChange1 = false,
    _currentVideoSrc1,
    _partVideo1 = 0,
    _videoOpdts1 = [],
    _splitVideo1 = [],
    colorIndex1 = 0,
    _limitedCropping1 = false,
    _divVideoRightList1,
    _divVideoThumbnail,
    _btnMinimizeNvpModal,
    _appElement,
    HideOpVideoModal1 = false;
//#endregion

//#region Ready
(() => {
    _sliderLeft1 = document.getElementById(SliderLeft1);
    _sliderRight1 = document.getElementById(SliderRight1);
    _spSplitBar1 = document.getElementById(SpSplitBar1);
    _msgBox1 = document.getElementById(MsgBox1);
    _vidTag1 = document.getElementById(VdProcess1);
    _circleSlider1 = document.getElementById(DivCurrentSlider1);
    _startH1 = document.getElementById(StartH1);
    _startM1 = document.getElementById(StartM1);
    _startS1 = document.getElementById(StartS1);
    _endH1 = document.getElementById(EndH1);
    _endM1 = document.getElementById(EndM1);
    _endS1 = document.getElementById(EndS1);
    _btnSplit1 = document.getElementById(BtnSplit1);
    _txtCropSec1 = document.getElementById(TxtCropSecond1);
    _inputUploadVideo1 = document.getElementById("inputUploadVideo1");
    _modalFixedVp = document.getElementById(ModalFixedVp);
    _divVideoRightList1 = document.getElementById(DivVideoRightList1);
    _divVideoThumbnail = document.getElementById(DivVideoThumbnail);
    _btnMinimizeNvpModal = document.getElementById(BtnMinimizeNvpModal);

    document.getElementById(BtnCloseVdPreview1).addEventListener("click", closeVdPreview1);
    document.getElementById(AVdPlay1).addEventListener("click", playVideo1);

    _appElement = document.querySelector("[ng-app=opsApp]");

    document.getElementById(BtnCloseNvpModal).addEventListener("click", () => {
        //_modalFixedVp.style.display = "none";
        //console.log("Not work");
        JsModalFixedVp.slideUp("slow");
        angular.element(appElement).scope().$apply(() => {
            angular.element(appElement).scope().FixedModal.IsShow = !angular.element(appElement).scope().FixedModal.IsShow;
        });
    });
    _inputUploadVideo1.addEventListener("change", onChangedInputVf1);
    _btnMinimizeNvpModal.addEventListener("click", () => {
        JsModalFixedVp.slideUp("slow");
        console.log("Displaying the modal.");
        $("#modalFixedVp2").slideDown("slow");
        //$("#modalFixedVp2").show();
        angular.element(_appElement).scope().$apply(() => {
            console.log("Current list of split-videos.");
            console.log(_splitVideo1);
            angular.element(_appElement).scope().OpVideo1 = _splitVideo1;
        });
    });

    //document.getElementById("inputUploadVideo1").addEventListener("change", () => {
    //    console.log("testing...");
    //});

    //console.log(document.getElementById(InputUploadVideo1));

    //document.getElementById(LbUploadVideo1).addEventListener("dragover", dragOverDivVideo1);
    //document.getElementById(LbUploadVideo1).addEventListener("drop", dropDivVideo1);
    _startH1.addEventListener('change', changeTimeValue1);
    _startM1.addEventListener('change', changeTimeValue1);
    _startS1.addEventListener('change', changeTimeValue1);
    _endH1.addEventListener('change', changeTimeValue1);
    _endM1.addEventListener('change', changeTimeValue1);
    _endS1.addEventListener('change', changeTimeValue1);
    _btnSplit1.addEventListener('click', cropVideo1);

    _circleSlider1.addEventListener("mousedown", mouseDownCurrentSlider1);
    _sliderLeft1.addEventListener("mousedown", mouseDownSliderLeft1);
    _sliderRight1.addEventListener("mousedown", mouseDownSliderRight1);
    //document.getElementById(DivCurrentSlider).addEventListener("mouseup", mouseUpCurrentSlider);
    window.addEventListener('mousemove', mouseMove1);
    window.addEventListener('mouseup', mouseUp1);

    //$('#divDropVideo1').on("dragenter dragstart dragend dragleave dragover drag drop", (e) => {
    //    e.preventDefault();
    //});

    document.getElementById(VdProcess1).ontimeupdate = (e) => { ontimeUpdate1(e); };

    _sliderWrap1 = document.getElementById(DivSliderWrap1);

    //// Clicking 'Save' events
    //document.getElementById(BtnUploadVideo1).addEventListener("click", () => { SaveProcessVideo1(); });
    ////document.getElementById(BtnSaveVideo).addEventListener("click", () => { SaveProcessVideo(); });

    //$(JsDivRightVideoList1).hide(); // Hiding right short video list by default.

    // Hiding 'Operation Video' modal
    //document.getElementById(BtnHideOpVideoModal1).addEventListener("click", () => {
    //    $(JsVdpModal1).hide("slide", { direction: "right" }, 1000);
    //    HideOpVideoModal1 = true;

    //    setTimeout(() => {
    //        //document.getElementById(DivRightVideoList).style.display = "flex";

    //        $(JsDivRightVideoList1).show("slide", { direction: "right" }, 1000);
    //    }, 1000);

    //    const appElement = document.querySelector("[ng-app=opsApp]"),
    //        $scope = angular.element(appElement).scope();

    //    console.log(_videoOpdts1);

    //    $scope.$apply(() => {
    //        $scope.OpVideo1 = _videoOpdts1;
    //    });
    //});

    //document.getElementById(BtnCloseVdpModal1).addEventListener("click", () => {
    //    HideOpVideoModal1 = false;
    //});

    console.log("Weakness!!!");
})();
//#endregion

//#region Processing video
function SaveProcessVideo1() {
    const validVideos = _videoOpdts1.filter(x => x.OpSerial && x.OpSerial !== "0");
    console.log(validVideos);
    if (validVideos && validVideos.length > 0) {
        UploadMainVideoToServer1();
    } else {
        MsgInform("Information", "There are not any processes that are assigned to videos.", "error", false, true);
    }
}

function SplitVideoOp1(inputFilePath, connectionId) {
    const validVideos = _videoOpdts1.filter(x => x.OpSerial && x.OpSerial !== "0"),
        config = new AjaxConfig(UrlSplitOpdtVideos1, true, JSON.stringify({ opdtVideos: validVideos, inputFilePath, connectionId }));

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
            document.getElementById(DivGlobe1).style.display = "none";
        }
    });
}

function SendCroppedOpdtVideoInfo1(inputFilePath, connectionId) {
    //$.blockUI({ message: "<h3>Splitting video...</h3>" });
    const validVideos = _videoOpdts1.filter(x => x.OpSerial && x.OpSerial !== "0");

    return $.post(UrlSplitOpdtVideos1, { opdtVideos: validVideos, inputFilePath, connectionId }).
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
                document.getElementById(DivGlobe1).style.display = "none";
            }
            //$.unblockUI();
        }).fail((xhr, status, err) => {
            HandleException(xhr, status, err);
        });
}

function UploadMainVideoToServer1() {
    const guid = uuidv4(),
        fileUpload = $('#inputUploadVideo1')[0].files;

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
            message: "<h3>Uploading video to the server...</h3>",
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

                    const reqUpload = UploadChunkFile1(dataForm, (res) => {
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

                    document.getElementById(DivGlobe1).style.display = "block";

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
                                alertTag = createAlertTag1(message);

                            vdpAlerts.appendChild(alertTag);
                            setTimeout(() => { alertTag.remove(); }, 5000);
                        };

                        // Starting the connection.
                        $.connection.hub.start().done(() => {
                            console.log("SignalR is started.");
                            chat.server.getConnectionId().done((connectionId) => {
                                SplitVideoOp1(videoServerUrl, connectionId);
                            });
                        });
                    }
                });
            }
        });
    }
}

function UploadChunkFile1(dataForm, callBack) {
    const request = $.ajax({
        url: UrlUploadChunkVideos1,
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

function createRightVideoList1(videoOpdt) {
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

    row.classList.add("row", "n-vdp-modal--each-vd");
    divSelect.classList.add("row");
    spTotalTime.classList.add("n-op__total-time");

    const drpProcess = createProcessDropdownList1(window.CurrentOpdts, videoOpdt.PartNo),
        spSelectedTextId = `sp${videoOpdt.PartNo}`;
    divSelect.appendChild(drpProcess);

    $(drpProcess).multiselect({
        enableFiltering: true,
        enableCaseInsensitiveFiltering: true,
        maxHeight: 300,
        buttonWidth: '200px',
        onChange: (selectedOption) => {
            const selectedOpdt = _videoOpdts1.find(x => x.OpSerial === selectedOption[0].value.toString());

            if (selectedOpdt) {
                MsgInform("Alert", `This process was selected for video number: ${selectedOpdt.PartNo}`, "error", true, true);
                if (selectedOpdt.OpSerial) {
                    console.log(selectedOpdt.OpSerial);
                } else {
                    document.getElementById(spSelectedTextId).innerHTML = "Select process!";
                    document.getElementById(spSelectedTextId).style.cssText = "color: red !important";
                }
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
            button: `<span id=${spSelectedTextId} class="multiselect dropdown-toggle n-vdp__drp-text" data-toggle="dropdown">Select process!</span>`
        }
    });

    row.appendChild(vdRow);
    row.appendChild(divSelect);

    btnInsertProcess.classList.add("glyphicon", "glyphicon-plus-sign", "n-op__btn--add-process");

    vd.classList.add("n-op-vdp__video");
    col9.classList.add("col-md-9");
    col3.classList.add("col-md-3");
    p.classList.add("row");
    d.classList.add("row");
    divTotalTime.classList.add("row");

    divTotalTime.innerHTML = "Total time: ";
    spTotalTime.innerHTML = new Date(videoOpdt.TotalTime * 1000).toISOString().substr(11, 8);
    divTotalTime.appendChild(spTotalTime);

    btnRemove.classList.add("n-vdp__btn-close-part-video");
    //spRemove.innerHTML = "❌";

    d.innerHTML = `${videoOpdt.StartTime} - ${videoOpdt.EndTime}`;
    pNo.innerHTML = `${videoOpdt.PartNo}`;
    pNo.classList.add("n-op__part-no");
    p.appendChild(pNo);

    btnRemove.addEventListener("click", () => {
        ConfirmYesNo("Confirmation", "Are you sure to remove this video?", () => {
            row.remove();
            const vdRows = document.getElementsByClassName("vdp-modal--each-vd");
            if (vdRows && vdRows.length === 0) _partVideo1 = 0;

            // Removing item from list selected video-processes.
            const pos = _videoOpdts1.map((e) => e.PartNo).indexOf(videoOpdt.PartNo);
            if (pos > -1) {
                _videoOpdts1.splice(pos, 1);
                _splitVideo1.splice(pos, 1);
            }

            // Removing part bar by part number.
            const divPart = document.getElementById(`divPart${videoOpdt.PartNo}`);
            if (divPart) divPart.remove();

            // Removing video thumbnail image
            const imgPart = document.getElementById(`imgPart${videoOpdt.PartNo}`);
            if (imgPart) imgPart.remove();
        }, () => { });
    });

    if (_currentVideoSrc1) vd.setAttribute("src", _currentVideoSrc1);
    vd.setAttribute("controls", "controls");
    vd.currentTime = convertToSecond1(videoOpdt.StartTime);
    vd.ontimeupdate = () => {
        if (vd.currentTime >= convertToSecond1(videoOpdt.EndTime)) vd.pause();
    };

    col9.appendChild(vd);
    col3.appendChild(p);
    col3.appendChild(d);
    col3.appendChild(divTotalTime);

    btnRemove.appendChild(spRemove);
    col9.appendChild(btnRemove);
    vdRow.appendChild(col9);
    vdRow.appendChild(col3);

    return row;
}

function moveSlidersToNewRangeTime1(videoOpdt) {
    console.log("Moving start/end sliders to new positions by default second.");

    const endedSliderLeft = _sliderRight1.style.left,
        endedSliderLeftFloat = parseFloat(endedSliderLeft.replace(/%/gi, ""));

    //if (!_limitedCropping) {
    // Calculating position of ending slider by default time(s).
    const barLeft = _sliderLeft1.style.left,
        //startedSliderLeft = parseFloat(barLeft.replace(/%/gi, "")),
        //startedDuration = _vidTag1.duration / 100 * startedSliderLeft,
        defaultSecond = _txtCropSec1.value,
        lastEndedDuration = _vidTag1.duration / 100 * endedSliderLeftFloat;
    let newEndedDuration = parseInt(defaultSecond) + lastEndedDuration,
        newEndedPercent = newEndedDuration / _vidTag1.duration * 100;

    if (endedSliderLeftFloat < 100) {
        if (parseInt(defaultSecond) < 1) {
            MsgInform("Error", "Invalid splitting second!", "error", false, true);
        } else {
            console.log(_spSplitBar1.style.width);
            _sliderLeft1.style.left = endedSliderLeft;

            if (newEndedDuration > _vidTag1.duration) newEndedDuration = _vidTag1.duration;
            // Moving current time of video to starting slider.
            _vidTag1.currentTime = lastEndedDuration;
            //console.log(lastEndedDuration);

            //const splittingBarLength = spSplitBar1.style.width;
            //console.log(splittingBarLength);

            if (newEndedPercent > 100) newEndedPercent = 100;

            // Positioning controls.
            _sliderRight1.style.left = `${newEndedPercent}%`;
            spSplitBar1.style.left = endedSliderLeft;
            _circleSlider1.style.left = endedSliderLeft;

            // Assigning time to textboxes.
            const startTimeStr = new Date(lastEndedDuration * 1000).toISOString(),
                endTimeStr = new Date(newEndedDuration * 1000).toISOString();
            _startH1.value = startTimeStr.substr(11, 2);
            _startM1.value = startTimeStr.substr(14, 2);
            _startS1.value = startTimeStr.substr(17, 2);
            _endH1.value = endTimeStr.substr(11, 2);
            _endM1.value = endTimeStr.substr(14, 2);
            _endS1.value = endTimeStr.substr(17, 2);

            const startedDur = _vidTag1.duration / 100 * parseFloat(barLeft.replace(/%/gi, ""));
            let splitControl = new SplitControl(videoOpdt.Edition, videoOpdt.StyleCode, videoOpdt.StyleColorSerial,
                videoOpdt.StyleSize, videoOpdt.RevNo, videoOpdt.OpRevNo, videoOpdt.OpSerial, videoOpdt.StartTime,
                videoOpdt.EndTime, videoOpdt.PartNo, videoOpdt.DisplayName, videoOpdt.TotalTime, videoOpdt.TotalTimeStr,
                videoOpdt.BarBgColor, barLeft, endedSliderLeft, spSplitBar1.style.width, startedDur);

            console.log(splitControl);
            _splitVideo1.push(splitControl);

            // Adding part bar.
            document.getElementById(DivBarPart1).appendChild(addPartBar1(splitControl));

            // Adding video thumbnail image to div
            console.log("Adding video thumbnail image to div");
            //const videoData = { borderColor: BarColour1[colorIndex1 - 1], left: splitControl.StartedSplittingLeft, partNo: videoOpdt.PartNo };
            _divVideoThumbnail.appendChild(takeVideoThumbnail(splitControl));

            // Assigning width to splitting bar.
            const sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
                sliderRightBcr = _sliderRight1.getBoundingClientRect(),
                divSliderWrapWidth = _sliderWrap1.clientWidth,
                splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
            spSplitBar1.style.width = `${splitBarPercent}%`;

            //videoOpdt.TotalTime = lastEndedDuration - startedDuration;
            console.log("Create object then add to right div.");
            console.log(videoOpdt);

            // Displaying right list video
            _divVideoRightList1.style.display = "block";

            // Adding cropped video to right list area.
            document.getElementById(DivVideoRightList1).appendChild(createRightVideoList1(videoOpdt));
        }
    } else {
        //MsgInform("Information", "Limited time", "error", false, true);
        _limitedCropping1 = true;

        const startedDur = _vidTag1.duration / 100 * parseFloat(barLeft.replace(/%/gi, ""));
        let splitControl = new SplitControl(videoOpdt.Edition, videoOpdt.StyleCode, videoOpdt.StyleColorSerial,
            videoOpdt.StyleSize, videoOpdt.RevNo, videoOpdt.OpRevNo, videoOpdt.OpSerial, videoOpdt.StartTime,
            videoOpdt.EndTime, videoOpdt.PartNo, videoOpdt.DisplayName, videoOpdt.TotalTime, videoOpdt.TotalTimeStr,
            videoOpdt.BarBgColor, barLeft, endedSliderLeft, spSplitBar1.style.width, startedDur);

        _splitVideo1.push(splitControl);

        // Adding part bar.
        document.getElementById(DivBarPart1).appendChild(addPartBar1(splitControl));

        // Adding video thumbnail image to div
        console.log("Adding video thumbnail image to div");
        //const videoData = { borderColor: BarColour1[colorIndex1 - 1], left: splitControl.StartedSplittingLeft, partNo: videoOpdt.PartNo };
        _divVideoThumbnail.appendChild(takeVideoThumbnail(splitControl));

        // Total time of split video.
        //videoOpdt.TotalTime = lastEndedDuration - startedDuration;

        // Adding cropped video to right list area.
        document.getElementById(DivVideoRightList1).appendChild(createRightVideoList1(videoOpdt));
    }
    //} else {
    //    MsgInform("Information", "Limited time", "error", false, true);
    //}
}

function moveSplittingControls1(startedSplittingLeft, endedSplittingLeft, splittingBarWidth, startedDuration, startTimeStr, endTimeStr) {
    console.log(startedSplittingLeft);

    // Assigning position to started splitting.
    _sliderLeft1.style.left = startedSplittingLeft;

    // Moving current time of video to starting slider.
    _vidTag1.currentTime = startedDuration;

    // Positioning controls.
    _sliderRight1.style.left = endedSplittingLeft;
    spSplitBar1.style.left = startedSplittingLeft;
    //_circleSlider1.style.left = startedSplittingLeft;

    // Assigning width to slitting bar.
    spSplitBar1.style.width = splittingBarWidth;

    // Assigning time to textboxes.
    _startH1.value = startTimeStr.substr(0, 2);
    _startM1.value = startTimeStr.substr(3, 2);
    _startS1.value = startTimeStr.substr(6, 2);
    _endH1.value = endTimeStr.substr(0, 2);
    _endM1.value = endTimeStr.substr(3, 2);
    _endS1.value = endTimeStr.substr(6, 2);
}

function addPartBar1(splitControl) {
    const divPartBar = document.createElement("div");
    divPartBar.id = `divPart${splitControl.PartNo}`;

    // Part bar clicking event.
    divPartBar.addEventListener("click", () => {
        moveSplittingControls1(splitControl.StartedSplittingLeft, splitControl.EndedSplittingLeft, splitControl.SplittingBarWidth,
            splitControl.StartedDuration, splitControl.StartTime, splitControl.EndTime);
    });

    //if (colorIndex1 === BarColour1.length) colorIndex1 = 0;
    //divPartBar.style.backgroundColor = BarColour1[colorIndex1];
    //colorIndex1++;
    divPartBar.style.backgroundColor = splitControl.BarBgColor;

    divPartBar.style.left = splitControl.StartedSplittingLeft;
    divPartBar.style.width = splitControl.SplittingBarWidth;
    divPartBar.style.cursor = "pointer";
    divPartBar.innerHTML = splitControl.PartNo;
    return divPartBar;
}

function cropVideo1() {
    if (validateStartEndTime1(true)) {
        console.log("Crop video...");
        _partVideo1++;
        const totalSecond = parseInt(_endH1.value) * 3600 + parseInt(_endM1.value) * 60 + parseInt(_endS1.value) - (parseInt(_startH1.value) * 3600 + parseInt(_startM1.value) * 60 + parseInt(_startS1.value)),
            currentOpmt = GetSelectedOneRowData(gridOpsTableId);

        console.log(totalSecond);
        if (colorIndex1 === BarColour1.length) colorIndex1 = 0;
        const barBgColor = BarColour1[colorIndex1];
        colorIndex1++;

        let videoOpdt = new VideoOpdt(currentOpmt.Edition, currentOpmt.StyleCode, currentOpmt.StyleColorSerial,
            currentOpmt.StyleSize, currentOpmt.RevNo, currentOpmt.OpRevNo, null,
            `${_startH1.value}:${_startM1.value}:${_startS1.value}`, `${_endH1.value}:${_endM1.value}:${_endS1.value}`,
            _partVideo1, null, totalSecond, new Date(totalSecond * 1000).toISOString().substr(11, 8), barBgColor);

        _videoOpdts1.push(videoOpdt);

        // Moving sliders and adding cropped video to right
        moveSlidersToNewRangeTime1(videoOpdt);
    }
}
//#endregion

//#region Generic methods
function takeVideoThumbnail(data) {
    const canvas = document.createElement("canvas"),
        scale = 1,
        img = document.createElement("img"),
        imgHeight = 60;

    img.id = `imgPart${data.PartNo}`;
    img.style.border = `3px solid ${data.BarBgColor}`;
    img.style.left = data.StartedSplittingLeft;

    const imgWidth = _vidTag1.videoWidth * imgHeight / _vidTag1.videoHeight;

    //canvas.width = _vidTag1.videoWidth * scale;
    //canvas.height = _vidTag1.videoHeight * scale;
    canvas.width = imgWidth * scale;
    canvas.height = imgHeight * scale;

    canvas.getContext('2d').drawImage(_vidTag1, 0, 0, canvas.width, canvas.height);

    img.src = canvas.toDataURL();
    data.ThumbnailImage = canvas.toDataURL();

    return img;
}

function dragStartVideo1(e) {
    //console.log(e.currentTarget);
    console.log(e.currentTarget.alt);
    //event.dataTransfer.setData("VideoNo", e.currentTarget.innerHTML);

    // Because the target is image and video number is assigned to alt attribute.
    event.dataTransfer.setData("VideoNo", e.currentTarget.alt);

    //const n = toolkit.getNode(e.currentTarget.innerHTML),
    //    currentVideo = $(`[data-jtk-node-id="${e.currentTarget.innerHTML}"]`);

    //console.log(currentVideo);
    //console.log(n);
    //console.log(n.data);

    //alert("dragStartVideo...");
}

function allowDropVideo1(event) {
    event.preventDefault();
    console.log("Allowing dropping video");
}

function dropVideo1(event) {
    event.preventDefault();
    const videoNo = event.dataTransfer.getData("VideoNo"),
        dragVideo = _videoOpdts1.find(x => x.PartNo.toString() === videoNo),
        opSerial = event.currentTarget.id.substr(4),
        selectedOpdt = _videoOpdts1.find(x => x.OpSerial === opSerial),
        pOpName = document.getElementById(`pOpName${opSerial}`);

    if (selectedOpdt) {
        MsgInform("Alert", `This process was selected for video number: ${selectedOpdt.PartNo}`, "error", true, true);
    } else {
        dragVideo.OpSerial = opSerial;
        dragVideo.DisplayName = pOpName.innerHTML;

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
            alertTag = createInformTag1(`&nbsp;Assigned video number '${videoNo}' to process: ${pOpName.innerHTML}`);

        vdpAlerts.appendChild(alertTag);
        setTimeout(() => { alertTag.remove(); }, 5000);
    }
}

function droppableProcessNodes1() {
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

function createInformTag1(msg) {
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

function createAlertTag1(msg) {
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

function validateStartEndTime1(isShowMsg) {
    if (startTimeToSecond1() >= endTimeToSecond1()) {
        if (isShowMsg) MsgInform("Error", "Ending time must be greater than starting time.", "error", false, true);
        return false;
    }
    if (startTimeToSecond1() >= _vidTag1.duration) {
        if (isShowMsg) MsgInform("Error", "Invalid starting time.", "error", false, true);
        return false;
    }
    if (endTimeToSecond1() >= _vidTag1.duration) {
        if (isShowMsg) MsgInform("Error", "Invalid ending time.", "error", false, true);
        return false;
    }

    return true;
}

function startTimeToSecond1() {
    return parseInt(_startH1.value) * 3600 + parseInt(_startM1.value) * 60 + parseInt(_startS1.value);
}

function endTimeToSecond1() {
    return parseInt(_endH1.value * 3600) + parseInt(_endM1.value) * 60 + parseInt(_endS1.value);
}

function convertToSecond1(timeStr) {
    const h = timeStr.substr(0, 2),
        m = timeStr.substr(3, 2),
        s = timeStr.substr(6, 2);

    return parseInt(h) * 3600 + parseInt(m) * 60 + parseInt(s);
}
//#endregion Generic methods

//#region Start, End time
function changeTimeValue1(e) {
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
        if (validateStartEndTime1(false)) {
            _isStartTimeChange1 = true;

            //console.log(startTimeToSecond1());

            _vidTag1.currentTime = startTimeToSecond1();
        }
    }

    if (e.currentTarget.id === EndH || e.currentTarget.id === EndM || e.currentTarget.id === EndS) {
        if (validateStartEndTime1(false)) {
            _isEndTimeChange1 = true;

            //console.log(endTimeToSecond1());
            _vidTag1.currentTime = endTimeToSecond1();
        }
    }
}
//#endregion Start, End time

//#region Sliding tags events
function mouseDownSliderLeft1() {
    console.log("Allow moving left slider");
    _isSlideLeft1 = true;
    _isSlide1 = true;
}

function mouseDownSliderRight1() {
    console.log("Allow moving right slider");
    _isSlideRight1 = true;
    _isSlide1 = true;
}

function oneSecondToPx1(width) {
    const totalTime = document.getElementById(VdProcess1).duration,
        px = width / totalTime;

    return px;
}

function getCurrentTime1(sliderLeft) {
    const sliderWrapBcr = _sliderWrap1.getBoundingClientRect(),
        percentLeft = sliderLeft / sliderWrapBcr.width * 100,
        totalTime = document.getElementById(VdProcess1).duration,
        currentTime = percentLeft.toFixed(2) / 100 * totalTime;

    return currentTime;
}

function mouseMove1(e) {
    const sliderWrapBcr = _sliderWrap1.getBoundingClientRect(),
        circleSliderBcr = _circleSlider1.getBoundingClientRect();

    //console.log("Moving event");

    //console.log(circleSliderBcr.width / 2);

    if (e.clientX >= sliderWrapBcr.left - circleSliderBcr.width / 2
        && e.clientX <= sliderWrapBcr.width + sliderWrapBcr.left + circleSliderBcr.width / 2) {
        const x = e.clientX - sliderWrapBcr.left,
            l = x / sliderWrapBcr.width * 100,
            sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            sliderRightBcr = _sliderRight1.getBoundingClientRect();

        if (_isSlide1 && !_isSlideLeft1 && !_isSlideRight1) {
            //circleSlider = document.getElementById(DivCurrentSlider);
            _circleSlider1.style.left = `${l}%`;
        }

        if (_isSlideLeft1) {
            //const x = e.clientX - sliderWrapBcr.left,
            //    sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            //    sliderRightBcr = _sliderRight1.getBoundingClientRect();
            //l = x / sliderWrapBcr.width * 100;

            if (sliderRightBcr.x + sliderRightBcr.width / 2 - e.clientX > oneSecondToPx1(sliderWrapBcr.width)) {
                //console.log(sliderLeftBcr);
                //console.log(_sliderRight1.getBoundingClientRect());
                //console.log(e.pageX);
                //console.log(e.clientX);

                _isMsgBoxShow1 = false;
                _msgBox1.style.display = "none";

                _sliderLeft1.style.left = `${l}%`;
                spSplitBar1.style.left = `${l}%`;

                // Scaling splitting bar
                const divSliderWrapWidth = _sliderWrap1.clientWidth,
                    splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
                spSplitBar1.style.width = `${splitBarPercent}%`;

                // Positioning circle slider
                _circleSlider1.style.left = `${l}%`;

                //console.log("Moving starting slider");
            } else {
                _isMsgBoxShow1 = true;
                _msgBox1.style.display = "block";
            }
        }

        if (_isSlideRight1) {
            //const x = e.clientX - sliderWrapBcr.left,
            //    sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            //    sliderRightBcr = _sliderRight1.getBoundingClientRect();
            //l = x / sliderWrapBcr.width * 100;

            if (e.clientX - (sliderLeftBcr.x + sliderLeftBcr.width / 2) > oneSecondToPx1(sliderWrapBcr.width)) {
                //console.log(e.clientX - sliderLeftBcr.x);

                _isMsgBoxShow1 = true;
                _msgBox1.style.display = "none";
                _sliderRight1.style.left = `${l}%`;

                // Scaling splitting bar
                //spSplitBar1.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
                const divSliderWrapWidth = _sliderWrap1.clientWidth,
                    splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
                spSplitBar1.style.width = `${splitBarPercent}%`;

                // Positioning circle slider
                _circleSlider1.style.left = `${l}%`;

                //console.log("Moving end slider");
            } else {
                _msgBox1.style.display = "block";
            }
        }

        if (_isSlide1) {
            console.log("moving current slider");
            const percentLeft = _circleSlider1.style.left.replace(/%/gi, ""),
                curTime = percentLeft / 100 * _vidTag1.duration;

            //_isSlide1 = false;

            document.getElementById(VdProcess1).currentTime = curTime;
        }
    }
}

function mouseUp1() {
    if (_isSlide1) {
        //const percentLeft = _circleSlider1.style.left.replace(/%/gi, ""),
        //    curTime = percentLeft / 100 * _vidTag1.duration;

        _isSlide1 = false;

        //document.getElementById(VdProcess1).currentTime = curTime;
    }

    if (_isSlideLeft1) _isSlideLeft1 = false;
    if (_isSlideRight1) _isSlideRight1 = false;
}

function mouseDownCurrentSlider1() {
    _isSlide1 = true;
}
//#endregion Sliding tags events

//#region Video tag events
function ontimeUpdate1(e) {
    const currentTimeStr = new Date(e.currentTarget.currentTime * 1000).toISOString(),
        timePercent = e.currentTarget.currentTime / e.currentTarget.duration * 100;

    document.getElementById(SpCurentTime1).innerHTML = currentTimeStr.substr(11, 8);
    _circleSlider1.style.left = `${timePercent}%`;

    console.log("timePercent");
    console.log(timePercent);

    if (_isSlideLeft1) {
        //_isSlideLeft1 = false;
        _sliderLeft1.style.left = `${timePercent}%`;
        spSplitBar1.style.left = `${timePercent}%`;
        _startH1.value = currentTimeStr.substr(11, 2);
        _startM1.value = currentTimeStr.substr(14, 2);
        _startS1.value = currentTimeStr.substr(17, 2);

        const sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            sliderRightBcr = _sliderRight1.getBoundingClientRect();

        //spSplitBar1.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap1.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        spSplitBar1.style.width = `${splitBarPercent}%`;
    }
    if (_isSlideRight1) {
        //_isSlideRight1 = false;
        _sliderRight1.style.left = `${timePercent}%`;
        _endH1.value = currentTimeStr.substr(11, 2);
        _endM1.value = currentTimeStr.substr(14, 2);
        _endS1.value = currentTimeStr.substr(17, 2);

        const sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            sliderRightBcr = _sliderRight1.getBoundingClientRect();

        spSplitBar1.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap1.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        spSplitBar1.style.width = `${splitBarPercent}%`;
    }

    if (_isStartTimeChange1) {
        _isStartTimeChange1 = false;
        _sliderLeft1.style.left = `${timePercent}%`;
        spSplitBar1.style.left = `${timePercent}%`;

        const sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            sliderRightBcr = _sliderRight1.getBoundingClientRect();

        //spSplitBar1.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap1.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        spSplitBar1.style.width = `${splitBarPercent}%`;
    }
    if (_isEndTimeChange1) {
        _isEndTimeChange1 = false;
        _sliderRight1.style.left = `${timePercent}%`;

        const sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            sliderRightBcr = _sliderRight1.getBoundingClientRect();

        //spSplitBar1.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap1.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        spSplitBar1.style.width = `${splitBarPercent}%`;
    }
}

function playVideo1(e) {
    e.currentTarget.classList.toggle("n-video-pause");
    const vid = document.getElementById(VdProcess1);
    if (vid.paused) {
        vid.play();
    } else {
        vid.pause();
    }
}

function ClearAllVideoInfo1() {
    console.log("Clearing all video information.");

    // Clearing list of process videos information.
    _videoOpdts1 = [];
    _splitVideo1 = [];

    _partVideo1 = 0;
    colorIndex1 = 0;
    //_limitedCropping = false;

    document.getElementById(VdProcess1).src = "";
    document.getElementById(DivVideoTag1).style.cssText = "display:none !important";
    document.getElementById(DivLoadVideo1).style.cssText = "display:flex !important";
    document.getElementById(InputUploadVideo1).value = "";
    document.getElementById(DivVideoRightList1).innerHTML = "";
    _divVideoThumbnail.innerHTML = ""; // Clearing video thumbnail image.

    // Hiding process video button
    document.getElementById(DivProcessVideo1).style.cssText = "display:none !important";

    // Clearing part div
    document.getElementById(DivBarPart1).innerHTML = "";

    // Hiding right list video
    _divVideoRightList1.style.display = "none";
}

function closeVdPreview1() {
    ConfirmYesNo("Confirmation", "Are you sure to cancel this video?", () => {
        ClearAllVideoInfo1();
    }, () => { });
}

function dragOverDivVideo1(e) {
    //e.preventDefault();
    e.target.style.backgroundColor = "dodgerblue";
    e.target.style.borderColor = "#fff";
    e.target.style.color = "white";
    e.dataTransfer.setData("text/plain", e.target.id);
    console.log(e.currentTarget.files);
}

function dropDivVideo1(e) {
    //e.preventDefault();

    const data = e.dataTransfer.getData("text/plain");
    console.log(data);
    console.log(e.currentTarget.files);
}

function setDefaultSplitTime1(dur) {
    const startTime = new Date(dur / 100 * _sliderLeft1.style.left.replace(/%/gi, "") * 1000).toISOString(),
        endTime = new Date(dur / 100 * _sliderRight1.style.left.replace(/%/gi, "") * 1000).toISOString();

    _startH1.value = startTime.substr(11, 2);
    _startM1.value = startTime.substr(14, 2);
    _startS1.value = startTime.substr(17, 2);
    _endH1.value = endTime.substr(11, 2);
    _endM1.value = endTime.substr(14, 2);
    _endS1.value = endTime.substr(17, 2);
}

function posSplitControlsByDefaultSecond1() {
    const defaultSecond = _txtCropSec1.value;

    if (parseInt(defaultSecond) < 1) {
        MsgInform("Error", "Invalid default second!", "error", false, true);
    } else {
        let percentTime = defaultSecond / _vidTag1.duration * 100;
        console.log(percentTime);
        percentTime = percentTime > 100 ? 100 : percentTime;
        console.log(percentTime);

        _sliderLeft1.style.left = "0%";
        spSplitBar1.style.left = "0%";
        _circleSlider1.style.left = "0%";
        _sliderRight1.style.left = `${percentTime}%`;
        //spSplitBar1.style.left = `${percentTime}%`;

        const sliderLeftBcr = _sliderLeft1.getBoundingClientRect(),
            sliderRightBcr = _sliderRight1.getBoundingClientRect();

        //spSplitBar1.style.width = `${sliderRightBcr.x - sliderLeftBcr.x}px`;
        // Scaling splitting bar
        const divSliderWrapWidth = _sliderWrap1.clientWidth,
            splitBarPercent = (sliderRightBcr.x - sliderLeftBcr.x) / divSliderWrapWidth * 100;
        spSplitBar1.style.width = `${splitBarPercent}%`;

        console.log(divSliderWrapWidth);
        console.log(splitBarPercent);
    }
}

function onChangedInputVf1(e) {
    ///<summary>On changed input video file</summary>

    console.log("Video file has been changed.");

    // Resetting video icon playing.
    const playIcon = document.getElementById(AVdPlay1);
    playIcon.classList.remove("n-video-pause");
    playIcon.classList.add("n-video-preview");

    console.log(e.currentTarget.files);

    if (e.currentTarget.files.length === 0) return;
    const vdUrl = URL.createObjectURL(e.currentTarget.files[0]),
        video = document.getElementById(VdProcess1);

    video.src = vdUrl;
    _currentVideoSrc1 = URL.createObjectURL(e.currentTarget.files[0]);

    video.onloadeddata = () => {
        //$.unblockUI();
        console.log(video.duration);

        if (video.duration > 86400) {
            console.log("The video is too long.");
        } else {
            //angular.element(_appElement).scope().$apply(() => {
            //    angular.element(_appElement).scope().SplitVideoUrl = vdUrl;
            //});

            const totalTime = new Date(video.duration * 1000).toISOString().substr(11, 8);
            document.getElementById(SpTotalTime1).innerHTML = totalTime;
            console.log(totalTime);

            document.getElementById(DivPartVideoSide1).style.height = document.getElementById(DivVideoSide1).clientHeight + "px";

            // Displaying video processing button
            document.getElementById(DivProcessVideo1).style.cssText = "display:flex !important";

            // Positioning ending splitting control by default splitting time (second).
            posSplitControlsByDefaultSecond1();

            setDefaultSplitTime1(video.duration);
        }
    };
    console.log(e.currentTarget.files[0]);

    document.getElementById(DivVideoTag1).style.cssText = "display:block !important";
    document.getElementById(DivLoadVideo1).style.cssText = "display:none !important";
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
        displayName, totalTime, totalTimeStr, barBgColor) {
        super(styleCode, styleColorSerial, styleSize, revNo, opRevNo);
        this.Edition = edition;
        this.OpSerial = opSerial;
        this.StartTime = startTime;
        this.EndTime = endTime;
        this.PartNo = partNo;
        this.DisplayName = displayName;
        this.TotalTime = totalTime;
        this.TotalTimeStr = totalTimeStr;
        this.BarBgColor = barBgColor;
    }
}

class SplitControl extends VideoOpdt {
    constructor(edition, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, startTime, endTime, partNo,
        displayName, totalTime, totalTimeStr, barBgColor, startedSplittingLeft, endedSplittingLeft, splittingBarWidth,
        startedDuration, thumbnailImage) {
        super(edition, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, startTime, endTime, partNo,
            displayName, totalTime, totalTimeStr, barBgColor);
        this.StartedSplittingLeft = startedSplittingLeft;
        this.EndedSplittingLeft = endedSplittingLeft;
        this.SplittingBarWidth = splittingBarWidth;
        this.StartedDuration = startedDuration;
        this.ThumbnailImage = thumbnailImage;
    }
}
//#endregion Classes