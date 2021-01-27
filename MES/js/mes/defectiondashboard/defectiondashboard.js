//variables
var mqttConfig;
var mqttClient;
var choosedPackage = "";
var dataSource = "direct";
var choosedPackageInfo = undefined;
var choosedDatePicker = undefined;

function initPage() {

    initDefectionTree();

    //Get list of Buyer
    GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);

    ////init chooseDate control, using pikaday
    choosedDatePicker = new Pikaday({
        field: document.getElementById("chooseDate"),
        format: "YYYY-MM-DD",
        onSelect: function () {
            // this.setDate('2015-01-01')
            console.log(this.getDate());
        }
    });

    //set current date
    setChoosedDate(new Date());

    //
    eventDateControlClick();

    eventSearchBtnControlClick();

    //
    eventDateSelected();

    //Get list of Buyer
    //GetMasterCodes("drpBuyer", BuyerMasterCode, StatusOkMasterCode);
    //GetMasterCodes("drpBuyerMdl", BuyerMasterCode, StatusOkMasterCode);

    //Event selected factory
    eventFactorySelected();

    //when select package
    eventPackageSelected();

     //get selected data source
    dataSource = $("#drpDataSource").val();

    //when select datasource
    eventDataSourceSelected();


    //Init grid view style modal.
    // bindDataToJqGridStyleModal(null, null, null, null, null);

    //Set factory base on factory of user role.
    $("#drpFactoryMdl")
        .val($("#hdFactoryUser").val())
        .trigger("change");
    $("#drpFactory")
        .val($("#hdFactoryUser").val())
        .trigger("change");

    //set default date, not work yet
    //var today = moment().format('YYYY-MM-DD');
    //document.getElementById("chooseDate").value = today;
}


function initDefectionTree() {
    $("#jstree_demo_div").fancytree({
        titlesTabbable: true,
        extensions: ["table"],
        table: {
            checkboxColumnIdx: 0,    // render the checkboxes into the this column index (default: nodeColumnIdx)
            indentation: 16,         // indent every node level by 16px
            nodeColumnIdx: 0        // render node expander, icon, and title to this column (default: #0)
        },
        source: [],
        renderColumns: function (event, data) {
            var node = data.node,
                $tdList = $(node.tr).find(">td");

            // Make the title cell span the remaining columns if it's a folder:
            if (node.isFolder()) {
                $tdList.eq(2)
                    .prop("colspan", 3)
                    .nextAll().remove();
                return;
            }
            // (Column #0 is rendered by fancytree by adding the checkbox)

            // Column #1 should contain the index as plain text, e.g. '2.7.1'
            console.log('lala');
            console.info(node);
            //$tdList.eq(1)
            //    .text(node.getIndexHier())
            //    .addClass("alignRight");


            //$tdList.eq(1).text(node.data.AONO);


            var tergetText = node.data.TARGET ? node.data.TARGET : "";
            var totalDefectText = node.data.TOTAL_DEFECT ? node.data.TOTAL_DEFECT : "";

            $tdList.eq(1).text(node.data.AONO);
            $tdList.eq(2).text(node.data.STYLETEXT);
            $tdList.eq(3).text(node.data.BUYERSTYLETEXT);

            $tdList.eq(4).text(node.data.STYLECOLORSERIAL);
            $tdList.eq(5).text(node.data.REVNO);
            $tdList.eq(6).text(node.data.BUYER);
            $tdList.eq(7).text(node.data.LINENAME);
            //$tdList.eq(5).html("<span style='float:mfright'>" + node.data.TARGET + "</span>");
            $tdList.eq(8).html("<span style='text-align:center; display:block'>" + tergetText + "</span>");

            //$tdList.eq(6).text(node.data.TOTAL_DEFECT).addClass("alignRight");
            $tdList.eq(9).html("<span style='text-align:center; display:block'>" + totalDefectText + "</span>");

            if (node.data.DEFECT_ITEM_ID > 0 && (node.data.IMAGE_COUNT > 0 || node.data.AUDIO_COUNT > 0 || node.data.VIDEO_COUNT > 0)) {
                $tdList.eq(10).html('<button type="button" class="btn btn-link" onclick="showMediaClick(' + node.data.DEFECT_ITEM_ID + ')"><i class="fas fa-photo-video"></i></button>');
            }
        }
    });
}

function showMediaClick(defectItemId) {
    $.ajax({
        type: 'GET',
        url: '/DefectionDashboard/Media?defectitemid=' + defectItemId,
        success: function (data) {
            Swal.fire({
                title: '<strong>Defect Media</strong>',
                //icon: 'info',
                html: data,
                width: '95%',
                showCloseButton: true,
                showConfirmButton: false,
                showCancelButton: false,
                focusConfirm: false,
            })

            initMediaGrid("#tbImage", "IMAGE")
            initMediaGrid("#tbVideo", "VIDEO")
            initMediaGrid("#tbAudio", "AUDIO")


            loadGridData("#tbImage", defectItemId, "IMAGE");
            loadGridData("#tbVideo", defectItemId, "VIDEO");
            loadGridData("#tbAudio", defectItemId, "AUDIO");
        }
    });

   
}

function initMediaGrid(tableId, title) {
    jQuery(tableId).jqGrid({
        datatype: "json",
        height: 'auto',
        colModel: [
            { name: '#', index: 'ItemIndex', width: 50, label: "#", align: 'left', sortable: false },
            { name: 'OBJECTKEY', index: 'OBJECTKEY', width: 300, label: title, key: true, align: 'center', sortable: false },
            { name: 'CreatedDate', index: 'CREATED_DATE', width: 150, label: "Created", align: 'right', sortable: false }
        ],
        rowNum: 10,
        autowidth: true,
        //width: '100%',
        shrinkToFit: true,
        scroll: true,
        onSelectRow: function (ids) {
            var grid = $(tableId)[0]
            var objectkey = jQuery(tableId).jqGrid('getGridParam', "selrow");
            console.info(objectkey);

            //get media info
            var config = ObjectConfigAjaxPost(
                "../DefectionDashboard/GetMediaInfo?objectkey=" + objectkey,
                false
            );

            AjaxGetCommon(config, function (response) {
                console.info(response);
                mediaInfo = response.Result;

                console.info("mediaInfo");
                console.info(mediaInfo);

                viewMedia(mediaInfo);
                //display latest data
                //PopulatePackageInfoToDashboard(choosedPackageInfo);
            });
        }
    });
}

function viewMedia(mediaInfo) {
    if (mediaInfo.MEDIA_TYPE == "IMAGE") {
        setVisibleForImagePlayer();
        setMediaSource("#imagePlayer", mediaInfo.URL);
    } else if (mediaInfo.MEDIA_TYPE == "VIDEO") {
        setVisibleForVideoPlayer();
        setMediaSource("#videoPlayer", mediaInfo.URL);
    } else if (mediaInfo.MEDIA_TYPE == "AUDIO") {
        setVisibleForAudioPlayer();
        setMediaSource("#audioPlayer", mediaInfo.URL);
    }
}

function setMediaSource(elementId, url) {
    $(elementId).attr("src", url);
}


function setVisibleForImagePlayer() {
    $("#imageDiv").show();
    $("#videoDiv").hide();
    $("#audioDiv").hide();
}

function setVisibleForVideoPlayer() {
    $("#imageDiv").hide();
    $("#videoDiv").show();
    $("#audioDiv").hide();
}

function setVisibleForAudioPlayer() {
    $("#imageDiv").hide();
    $("#videoDiv").hide();
    $("#audioDiv").show();
}

function loadGridData(tableId, defectItemId, mediaType) {
    //get data first
    var config = ObjectConfigAjaxPost(
        "../DefectionDashboard/GetMedia?defectitemid=" + defectItemId + "&mediatype=" + mediaType,
        false
    );

    AjaxGetCommon(config, function (response) {
        console.info(response);

        var lst = response.Result;

        console.info("data");
        console.info(lst);

        //display latest data
        var grid = $(tableId)[0]

        grid.addJSONData(lst);
    });
}

function RefreshData() {
    console.info(choosedPackage);
    if (!choosedPackage) return;

    GetPackageInfoAndTryConnectToMqtt();
}


function GetPackageInfoAndTryConnectToMqtt() {
    //get package first
    var config = ObjectConfigAjaxPost(
        "../MesManagement/GetPackageDto?mxpackage=" + choosedPackage + "&datasource=" + dataSource,
        false
    );

    AjaxGetCommon(config, function (response) {
        console.info(response);

        choosedPackageInfo = response.Result;

        //display latest data
        PopulatePackageInfoToDashboard(choosedPackageInfo);
    });


    //get data first
    var config = ObjectConfigAjaxPost(
    "../MesManagement/GetDefectionTree?mxpackage=" + choosedPackage,
    false
   );
    AjaxGetCommon(config, function (response) {
        console.info(response);

        treeData = response.Result;

        //display latest data
        PopulateDefectInfoToDashboard(treeData);
    });
}

function PopulatePackageInfoToDashboard(package) {
    $("#lblFactory").text(package.FACTORY);
    $("#lblLine").text(package.LINENO);
    $("#lblPackage").text(package.MXPACKAGE);
}

function PopulateDefectInfoToDashboard(treeData) {
    console.info(treeData);
}



//#region Variable
var tblStyleAoId = "#tbStyleAo";
var tblStyleAoName = "tbStyleAo";
var papStyleAoId = "#divStyleAo";
var papStyleAoName = "divStyleAo";

var curStyleInfo;

//#endregion

//#region Functions

function resetPackage() {
    $("#lblFactory").text("---");
    $("#lblLine").text("---");
    $("#lblPackage").text("---");

    $("#lblTarget").text(0);
    $("#lblAchieve").text(0);
    $("#lblPending").text(0);

    choosedPackageInfo = undefined;
    choosedPackage = ""
}


function GetProductionTarget(plnDate, factory, lineCd, stylInf, callBackFunc) {
    var config = ObjectConfigAjaxPost(
        "../DgsTarget/GetProductionTarget",
        false,
        JSON.stringify({
            plnDate: plnDate,
            factory: factory,
            lineCd: lineCd,
            stylInf: stylInf
        })
    );
    AjaxPostCommon(config, function (schl) {
        callBackFunc(schl);
    });
}

function ReloadPackageDataSource() {
    //var fac = $("#drpFactory").val();
    var fac = document.getElementById("drpFactory").value;
    var curDate = getCurrentDate();
    var choosedDate = getChoosedDate();
    var buyer = document.getElementById("drpBuyer").value;
    var aono = document.getElementById("txtAONo").value;

    if (!choosedDate)
        choosedDate = curDate;

    GetPackageByFactoryAndDate(fac, choosedDate, buyer, aono);
}


function Export() {
    var fac = document.getElementById("drpFactory").value;
    var curDate = getCurrentDate();
    var choosedDate = getChoosedDate();
    var buyer = document.getElementById( "drpBuyer").value;
    var aono = document.getElementById("txtAONo").value;

    if (!choosedDate)
        choosedDate = curDate;


    var dateText = moment(choosedDate).format('YYYY-MM-DD');

    var url = "/defectiondashboard/export?factory=" + fac + "&dt=" + dateText + "&buyer=" + buyer + "&aono=" + aono;
    console.info(url);
    window.location = url;
}
function GetPackageByFactoryAndDate(factory, plnDate, buyer, aono) {
    console.info(factory);
    console.info(plnDate);
    console.info(buyer);
    console.info(aono);

    axios.get('../defectiondashboard/GetDefectTreeData', {
            params: {
            factory: factory,
            dt: plnDate,
            buyer: buyer,
            aono: aono
            }
        })
        .then(function (response) {
            //resultElement.innerHTML = generateSuccessHTMLOutput(response);

            console.info(response);

            var tree = $("#jstree_demo_div").fancytree("getTree");

            tree.reload(response.data.Result);
        })
        .catch(function (error) {
            console.info(error)
            resultElement.innerHTML = generateErrorHTMLOutput(error);
        });
}

function getChoosedDate() {
    //var currentDate = $("#chooseDate").datepicker("getDate");
    //return currentDate;
    //return $("#chooseDate").val();

    return choosedDatePicker.getDate();
}

function setChoosedDate(date) {
    //$("#chooseDate").datepicker("setDate", date);
    choosedDatePicker.setDate(date);
}

function getCurrentDate() {
    /**
     * Get current date
     */
    var fullDate = new Date();
    //convert month to 2 digits
    var twoDigitMonth =
        fullDate.getMonth().length + 1 === 1
            ? fullDate.getMonth() + 1
            : "0" + (fullDate.getMonth() + 1);

    var currentDate =
        fullDate.getFullYear() + "-" + twoDigitMonth + "-" + fullDate.getDate();

    return currentDate;
}
/*
     * Add days to the date
     */
function AddDays(date, days) {
    var result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
}



//#endregion
