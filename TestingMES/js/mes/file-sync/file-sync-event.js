
/**
 * Synchornize file
 * @param {any} buyer
 * @param {any} ao
 * @param {any} style
 */
function syncFile() {
    $("#btnSysnFile").click(function () {

        //Get list of files type which want to synchornize
        let listFileSync = [];
        let listCountrySync = [];

        let buyer = $("#drpBuyer").val();
        let ao = $("#txtAoNumber").val();
        let style = $("#txtStyleInfo").val();

        if (isEmpty(buyer) || isEmpty(ao)) {
            ShowMessage("Synchronize files", "Please select buyer and enter AO.", ObjMessageType.Info);
            return;
        }

        let overwrite = $("#chkCad").is(":checked") === true ? "1" : "0";

        //Check list of file type
        if ($("#chkCad").is(":checked")) {
            listFileSync.push(FileTypeList.Cad);
        }

        if ($("#chkMarker").is(":checked")) {
            listFileSync.push(FileTypeList.Marker);
        }

        if ($("#chkPrinting").is(":checked")) {
            listFileSync.push(FileTypeList.Printing);
        }

        if ($("#chkEmbroidery").is(":checked")) {
            listFileSync.push(FileTypeList.Embroidery);
        }

        if ($("#chkOther").is(":checked")) {
            listFileSync.push(FileTypeList.Other);
        }

        if ($("#chkJig").is(":checked")) {
            listFileSync.push(FileTypeList.Jig);
        }

        //Get country list
        if ($("#chkIndonesia").is(":checked")) {
            listCountrySync.push(CountryList.Indonesia);
        }

        if ($("#chkEthiopia").is(":checked")) {
            listCountrySync.push(CountryList.Ethiopia);
        }

        if ($("#chkMyanmar").is(":checked")) {
            listCountrySync.push(CountryList.Myanmar);
        }

        //Check country list and file type list
        if (listCountrySync.length === 0 || listFileSync.length === 0) {
            ShowMessage("Synchronize files", "Please select country or file type.", ObjMessageType.Info);
            return;
        }

        $.blockUI();
        setTimeout(function () {
            //var config = ObjectConfigAjaxPost("../FileSync/SyncFile", false
            //    , JSON.stringify({ buyer: buyer, ao: ao, style: style, overwrite: overwrite, countryList: listCountrySync, fileTypeList: listFileSync }));
            //AjaxPostCommon(config, function (res) {
            //    $.unblockUI();
            //    alert(res);
            //});

            $.ajax({
                xhr: function () {
                    var xhr = new window.XMLHttpRequest();

                    xhr.upload.addEventListener("progress", function (evt) {
                        if (evt.lengthComputable) {
                            var percentComplete = evt.loaded / evt.total;
                            percentComplete = parseInt(percentComplete * 100);
                            console.log(percentComplete);

                            if (percentComplete === 100) {

                            }

                        }
                    }, false);

                    return xhr;
                },
                url: "../FileSync/SyncFile",
                type: "POST",
                data: JSON.stringify({ buyer: buyer, ao: ao, style: style, overwrite: overwrite, countryList: listCountrySync, fileTypeList: listFileSync }),
                contentType: "application/json",
                dataType: "json",
                success: function (result) {
                    
                    $.unblockUI();
                    alert(result);
                }
            });

        }, 50);
    });
}

function selectAllFileType() {
    $("#chkSelectAll").change(function () {
        if ($(this).is(":checked")) {
            $("#chkCad").prop('checked', true);
            $("#chkMarker").prop('checked', true);
            $("#chkPrinting").prop('checked', true);
            $("#chkJig").prop('checked', true);
            $("#chkEmbroidery").prop('checked', true);
            $("#chkOther").prop('checked', true);
        } else {
            $("#chkCad").prop('checked', false);
            $("#chkMarker").prop('checked', false);
            $("#chkPrinting").prop('checked', false);
            $("#chkJig").prop('checked', false);
            $("#chkEmbroidery").prop('checked', false);
            $("#chkOther").prop('checked', false);
        }
       
    });
}