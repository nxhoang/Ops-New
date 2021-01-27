function exportPng() {
    // xu ly css.
    AddCss();
    PreviewPrinter();
    setTimeout(function () {
        RemoveCss();
    }, 500);
};

$("#btnDownload").on('click', function () {
    DownLoadImage();
});

function AddCss() {
    $("a").addClass("printer");
    $(".jtk-ops-canvas").addClass(".jtk-ops-canvas-printer");
    $("#jsPlumb_2_1,.opSum ").addClass("printer-all");
    $(".controls, #miniview, .jtk-surface-pan, .delete, .group-title button").addClass("printer-all2");
    $(".op__control-div, .op__paging, .opmt-table, .opsum-btn-close, #opsmodal,#selected-flag,#miniview,.fifth-controls-input-page, .delete-connection,.ops-node-change-page,.connect,.jtk-surface-pan-left, .jtk-surface-pan-right,.jtk-surface-pan-top, .jtk-surface-pan-bottom").addClass("printer-btn");
    $("#jsPlumb_2_1").find("svg").each(function () {
        var style = $(this).attr("style");
        $(this).wrap("<spand style='" + style + "'></spand>");
        $(this).removeAttr("style");
    });
}
function RemoveCss() {
    $("a").removeClass("printer");
    $(".jtk-ops-canvas").removeClass("jtk-ops-canvas-printer");
    $("#jsPlumb_2_1,.opSum ").removeClass("printer-all");
    $(".controls, #miniview, .jtk-surface-pan, .delete, .group-title button").removeClass("printer-all2");
    $(".op__control-div, .op__paging, .opmt-table, .opsum-btn-close, #opsmodal,#selected-flag,#miniview,.fifth-controls-input-page, .delete-connection,.ops-node-change-page,.connect,.jtk-surface-pan-left, .jtk-surface-pan-right,.jtk-surface-pan-top, .jtk-surface-pan-bottom").removeClass("printer-btn");
    $("#jsPlumb_2_1").find("svg").each(function () {
        css = $(this).parent().attr("style");
        $(this).attr('style', css);
        $(this).unwrap();
    });
}
var getCanvas;
function PreviewPrinter_old() {
    $("#previewImage").html('');
    html2canvas(document.getElementById("jsPlumb_2_1")).then(function (canvas) {
        document.getElementById("previewImage").appendChild(canvas);
        getCanvas = canvas;
    });
    ShowModal("printView");
}

function DownLoadImage() {
    var imgageData = $("#previewImage").find("img").attr("src");
    var newData = imgageData.replace(/^data:image\/png/, "data:application/octet-stream");
    $("#btnDownload").attr("download", "layout-Ops.png").attr("href", newData);
}

function DownLoadImage_old() {
    var imgageData = getCanvas.toDataURL("image/png");
    // Now browser starts downloading it instead of just showing it
    var newData = imgageData.replace(/^data:image\/png/, "data:application/octet-stream");
    $("#btnDownload").attr("download", "layout-Ops.png").attr("href", newData);
}

function ShowImgTools(thisurl) {
    $("#imgPreviewDt").modal("show");
    var url = thisurl.attr("src");
    $("#imgDetailDt").attr("src", url);
}

//------New---------------------------
function PreviewPrinter() {
    var node = document.getElementsByClassName('jtk-ops-canvas')[0];

    domtoimage.toPng(node).then(function (dataUrl) {
        getCanvas = dataUrl;
        $("#previewImage").html("<img src=" + dataUrl + ">");
    }).catch(function (error) {
        console.error('oops, something went wrong!', error);
    });
    ShowModal("printView");
}