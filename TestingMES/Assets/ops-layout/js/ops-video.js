
// #region Upload style image

function UploadImageStyle(fncCallBack) {
    var fileUpload = $("[id*=flImageDetail]").get(0);
    var files = fileUpload.files;
    var data = new FormData();
    var styleCode = $("#txtStyleCodeDetail").val();

    //Check style code is empty or not
    if (isEmpty(styleCode)) {
        ShowMessageOk("003", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, ObjMessageType.Error);

        return;
    }

    //Check file length
    if (files.length !== 0) {
        data.append("StyleCode", styleCode);
        for (var i = 0; i < files.length; i++) {
            data.append(files[i].name, files[i]);
        }
    } else {
        ShowMessageOk("004", SmsFunction.Upload, MessageType.Error, MessageContext.InvalidData);
        return;
    }

    $.ajax({
        url: "/Ops/UploadImageStyleMaster",
        async: false,
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            fncCallBack(result);
        },
        error: function (err) {
            ShowAjaxError(err, "/Ops/UploadImageStyleMaster");
        }
    });
}

// #endregion

// #region Upload files and videos of process.

function UploadOpDetailFile(fncCallBack) {

    var files = $("#flOpDetail")[0].files;

    var data = new FormData();
    for (var i = 0; i < files.length; i++) {
        data.append(files[i].name, files[i]);
    }

    //Get selected operation plan
    var objOpMaster = GetSelectedOneRowData(gridOpsTableId);

    //Get process key code from selected process.
    data.append("StyleCode", objOpMaster.StyleCode);
    data.append("StyleSize", objOpMaster.StyleSize);
    data.append("StyleColor", objOpMaster.StyleColorSerial);
    data.append("StyleRevNo", objOpMaster.RevNo);
    data.append("OpRevNo", objOpMaster.OpRevNo);
    data.append("OpSerial", $("#hdOpSerialOpDetail").val());
    data.append("Edition", objOpMaster.Edition);

    var uploadCode = $("#drpJigFileType").val().split('-')[0];;
    data.append("UploadCode", uploadCode);

    $.ajax({
        url: "/Ops/UploadOpDetailFile",
        //async: false,
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (objResult) {
            fncCallBack(JSON.parse(objResult));
        },
        error: function (err) {
            ShowAjaxError(err, "/Ops/UploadOpDetailFile");
        }
    });

}

function UploadJigFile(uploadType, callBackFnc) {

    var files = $("#flOpDetail")[0].files;

    var data = new FormData();
    for (var i = 0; i < files.length; i++) {
        data.append(files[i].name, files[i]);
    }

    //Get selected operation plan
    var objOpMaster = GetSelectedOneRowData(gridOpsTableId);

    //Get process key code from selected process.
    data.append("StyleCode", objOpMaster.StyleCode);
    data.append("StyleSize", objOpMaster.StyleSize);
    data.append("StyleColor", objOpMaster.StyleColorSerial);
    data.append("StyleRevNo", objOpMaster.RevNo);
    data.append("OpRevNo", objOpMaster.OpRevNo);
    data.append("OpSerial", $("#hdOpSerialOpDetail").val());
    data.append("Edition", objOpMaster.Edition);

    var uploadCode = $("#drpJigFileType").val().split('-')[0];;
    data.append("UploadCode", uploadCode);

    data.append("UploadType", uploadType);

    $.ajax({
        url: "/Ops/UploadJigFile",
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (objResult) {
            callBackFnc(objResult);
        },
        error: function (err) {
            ShowAjaxError(err, "/Ops/UploadJigFile");
        }
    });

}

function UploadVideoOpDetail() {
    var fileUpload = $('#flOpDetail')[0].files;

    if (!CheckDataBeforeUploadOpDetailFile(fileUpload)) return Fail;

    var files = fileUpload[0];
    var statusUpload = Fail;

    // create array to store the buffer chunks
    var chunkFile = [];

    // set up other initial vars
    var bufferChunkSize = 10 * (1024 * 1024);

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
        UploadChunkFileOpDetail(chunk, filePartName, function (result) {
            statusUpload = result;
        });
    }

    return statusUpload;
}

function UploadChunkFileOpDetail(chunk, fileName, fncCallBack) {

    var data = new FormData();
    data.append(fileName, chunk);
    data.append("FileName", fileName);

    //Get selected operation plan
    var objOpMaster = GetSelectedOneRowData(gridOpsTableId);

    //Get process key code from selected process.
    data.append("StyleCode", objOpMaster.StyleCode);
    data.append("StyleSize", objOpMaster.StyleSize);
    data.append("StyleColor", objOpMaster.StyleColorSerial);
    data.append("StyleRevNo", objOpMaster.RevNo);
    data.append("OpRevNo", objOpMaster.OpRevNo);
    data.append("OpSerial", $("#hdOpSerialOpDetail").val());
    data.append("Edition", objOpMaster.Edition);
    data.append("FileUploadType", $("#hdFileTypeOpDetail").val());

    $.ajax({
        url: "/Ops/UploadVideoOpDetail",
        async: false,
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            fncCallBack(result);
        },
        error: function (err) {
            ShowAjaxError(err, "/Ops/UploadVideoOpDetail");
        }
    });
}

// #region Upload file with process bar.

function uploadFile(uploadType) {
    var files = $("#flOpDetail")[0].files;

    var data = new FormData();
    for (var i = 0; i < files.length; i++) {
        data.append(files[i].name, files[i]);
    }

    //Get selected operation plan
    var objOpMaster = GetSelectedOneRowData(gridOpsTableId);

    //Get process key code from selected process.
    data.append("StyleCode", objOpMaster.StyleCode);
    data.append("StyleSize", objOpMaster.StyleSize);
    data.append("StyleColor", objOpMaster.StyleColorSerial);
    data.append("StyleRevNo", objOpMaster.RevNo);
    data.append("OpRevNo", objOpMaster.OpRevNo);
    data.append("OpSerial", $("#hdOpSerialOpDetail").val());
    data.append("Edition", objOpMaster.Edition);

    var uploadCode = $("#drpJigFileType").val().split('-')[0];;
    data.append("UploadCode", uploadCode);

    data.append("UploadType", uploadType);

    ajax = new XMLHttpRequest();
    ajax.upload.addEventListener("progress", progressHandler, false);
    ajax.addEventListener("load", completeHandler, false);
    ajax.open("POST", "/Ops/UploadJigFile");
    ajax.send(data);
}

function progressHandler(event) {
    var percent = (event.loaded / event.total) * 100;
    var completePer = percent + "%";
    $('#divProcessBar').css('width', completePer);
    $('#divProcessBar').append("Completed " + completePer);
}

function completeHandler() {
    $('#divProcessBar').css('width', "100%");
    $('#divProcessBar').append("Completed");
}
// #endregion

// #endregion

// #region Upload Image and video of process on modal.

function UploadImageProcess(opMaster, callBack) {
    const fileUpload = $("[id*=flProcessImage]").get(0);
    const files = fileUpload.files;

    //Check file length
    if (files.length === 0) {
        callBack(3);
    } else {
        const data = new FormData();
        for (let i = 0; i < files.length; i++) {
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
            url: "/Ops/UploadImageProcess",
            async: true,
            type: "POST",
            data: data,
            contentType: false,
            processData: false,
            success: function (result) {
                callBack(result);
            },
            beforeSend: () => {
                $.blockUI({ message: "<h3>Uploading image...</h3>" });
            },
            complete: () => {
                //$.unblockUI();
            },
            error: function () {
                $("[id*=hfFileName]").val("");
                callBack(Fail);
            }
        });
    }
}

function UploadVideoProcess(opmt, callBack) {
    const fileUpload = $('#flProcessVideo')[0].files;

    // Check file length
    if (fileUpload.length === 0) {
        callBack(3);
    } else {
        const files = fileUpload[0], chunkFile = [], bufferChunkSize = 3 * (1024 * 1024);
        let fileStreamPos = 0, endPos = bufferChunkSize;
        const fileSize = files.size;

        // Add to the FileChunk array until we get to the end of the file
        while (fileStreamPos < fileSize) {
            // "slice" the file from the starting position/offset, to the required length
            chunkFile.push(files.slice(fileStreamPos, endPos));
            fileStreamPos = endPos; // jump by the amount read
            endPos = fileStreamPos + bufferChunkSize; // set next chunk length
        }

        // Get total number of "files" we will be sending
        const totalParts = chunkFile.length, reqPromises = [];
        let partCount = 0, partRes = [];

        // Loop through, pulling the first item from the array each time and sending it
        while (chunkFile.length > 0) {
            const chunk = chunkFile.shift();
            partCount++;

            // file name convention
            const filePartName = files.name + ".part_" + partCount + "." + totalParts;
            const reqUpload = UploadChunkFile(chunk, filePartName, opmt, (res) => {
                console.log(res);
                if (res && res.length > 0 && res !== "fail") {
                    partRes.push(res);
                }
            });

            reqPromises.push(reqUpload);
        }

        $.when.apply($, reqPromises).done((a) => {
            console.log(a);
            callBack(partRes);
        });
    }
}

function UploadChunkFile(chunk, fileName, opmt, callBack) {
    const data = new FormData();

    data.append(fileName, chunk);
    data.append("FileName", fileName);
    data.append("StyleCode", opmt.StyleCode);
    data.append("StyleSize", opmt.StyleSize);
    data.append("StyleColor", opmt.StyleColorSerial);
    data.append("StyleRevNo", opmt.RevNo);
    data.append("OpRevNo", opmt.OpRevNo);
    data.append("Edition", opmt.Edition);
    data.append("OpSerial", $("#txtProcessNo").val());

    const request = $.ajax({
        url: "/Ops/UploadVideoProcess",
        async: true,
        type: "POST",
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            callBack(result);
        },
        beforeSend: () => {
            $.blockUI({ message: "<h3>Uploading video...</h3>" });
        },
        complete: () => {
            //$.unblockUI();
        },
        error: function () {
            callBack(Fail);
        }
    });

    return request;
}

// #endregion

// #region Functions for image and videos

function DeleteProcessVideo(opdt) {
    if (opdt && opdt.VideoFile && !$.isEmptyObject(opdt.VideoFile)) {
        ShowConfirmYesNoMessage("001", SmsFunction.Delete, MessageType.Confirm, MessageContext.DeleteConfirm, () => {
            const config = ObjectConfigAjaxPost("/Ops/DeleteVideo", false, JSON.stringify({
                styleCode: opdt.StyleCode,
                styleSize: opdt.StyleSize,
                styleColorSerial: opdt.StyleColorSerial,
                revNo: opdt.RevNo,
                opRevNo: opdt.OpRevNo,
                opSerial: opdt.OpSerial,
                edition: opdt.Edition
            }));
            AjaxPostCommon(config, (respone) => {
                if (respone === Success) {
                    //Set hidden file is empty
                    $("#hdVideoName").val(""); RemovePreviewVideo();
                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Success, MessageContext.Delete,
                        ObjMessageType.Info, "");
                } else {
                    ShowMessageOk("001", SmsFunction.Delete, MessageType.Warning, MessageContext.IgnoreChanges,
                        ObjMessageType.Error, respone);
                }
            });
        }, () => { }, "video");
    }
}

function RemovePreviewVideo() {
    $("#flProcessVideo").val("").clone(true);
    $("#vidPreview").attr('src', '');

    var $source = $('#videoPreview');
    $source[0].src = "";
    $source.parent()[0].load();

    $("#lblVideoName").text("");
}

function RemovePreviewImage() {
    $("#flProcessImage").val("").clone(true);
    $("#imgPreview").attr("src", "../img/no-image.png");
    $("#lblImageName").text("");
}

function RemovePreviewStyleImage() {
    $("#flImageDetail").val("").clone(true);
    $("#imgPreviewDetail").attr("src", "../img/no-image.png");
}

function RemoveOpDetailFilePreview() {
    $("#flOpDetail").val("").clone(true);

    var $source = $('#opDetailVideoPreview');
    $source[0].src = "";
    $source.parent()[0].load();
}

function DeleteFileOpDetail(fileType, opDetail) {
    var delStatus;
    var fileName;

    if (fileType === VideoType) {
        fileName = opDetail.VideoFile;
    } else {
        fileName = opDetail.ImageName;
    }

    $.ajax({
        url: "/Ops/DeleteLocalProcessFile",
        async: false, //run sequence
        type: "POST",
        data: JSON.stringify({ fileName: fileName, fileType: fileType, opDetail: opDetail }),
        dataType: "json",
        contentType: "application/json",
        success: function (resDel) {
            delStatus = resDel;
        },
        error: function (jqXhr, status, errorThrown) {
            delStatus = false;
        }
    });

    return delStatus;
}

// #endregion

//#region Upload file common functions
function SetDataToUploadFile(file, opdt) {
    /// <summary>
    /// Sets the data to upload imgage.
    /// </summary>
    /// <param name="files">The files.</param>
    /// <param name="opdt">The opdt.</param>
    /// <returns></returns>
    /// Author: Nguyen Xuan Hoang
    var data = new FormData();

    data.append(file.name, file);
    data.append("StyleCode", opdt.StyleCode);
    data.append("StyleSize", opdt.StyleSize);
    data.append("StyleColor", opdt.StyleColorSerial);
    data.append("StyleRevNo", opdt.RevNo);
    data.append("OpRevNo", opdt.OpRevNo);
    data.append("OpSerial", opdt.OpSerial);

    return data;
}
//#endregion

//#region Upload video functions
function UploadProcessVideo(file, uploadChunkFile) {
    var promises = [];
    var chunkFile = [];
    var bufferChunkSize = 3 * (1024 * 1024);
    var fileStreamPos = 0;
    var endPos = bufferChunkSize;
    var fileSize = file.size;

    // Devide the video file by bufferChunkSize then push into chunkFile array
    while (fileStreamPos < fileSize) {
        chunkFile.push(file.slice(fileStreamPos, endPos));
        fileStreamPos = endPos;
        endPos = fileStreamPos + bufferChunkSize;
    }

    // Total part of the video file
    var totalParts = chunkFile.length;
    var partCount = 0;

    // Upload parts of the video file one by one
    while (chunkFile.length > 0) {
        var chunk = chunkFile.shift();
        partCount++;
        var filePartName = file.name + ".part_" + partCount + "." + totalParts;
        var request = uploadChunkFile(chunk, filePartName, chunkFile.length);

        promises.push(request);
    }

    return promises;
}
//#endregion

//Check type of machine file.
function CheckTypeOfVideo(files) {
    var res = true;
    for (var i = 0; i < files.length; i++) {
        var fileExt = GetExtensionFileName(files[i].name);

        res = IsInArray(fileExt, ArrVideoType) ? true : false;
        break;
    }

    return res;
}