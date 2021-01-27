
//Must overwrite function ClickRowOpsCon(row);

var gridOpsTableNameCon = "gridOpsTableCon";
var gridOpsTableIdCon = "#gridOpsTableCon";

var gridOpsPaperNameCon = "gridOpsPaperCon";
var gridOpsPaperIdCon = "#gridOpsPaperCon";

arrOpsColNameCon = {
    EDITION: "Edition",
    STYLECODE: "Style Code",
    STYLESIZE: "Style Size",
    STYLECOLORSERIAL: "Color",
    REVNO: "Revision",
    OPREVNO: "OP Revision",
    OPTIME: "Ops Time",
    OPPRICE: "OP Price",
    MACHINECOUNT: "Machine count",
    CONFIRMCHK: "Status",
    OPCOUNT: "OP Count",
    MANCOUNT: "Workers",
    LASTUPDATEDATE: "Date update",
    REMARKS: "Remarks"
};

//Bind data to Ops gridview
function BindDataToJqGridOpsCon(styleCode, styleSize, styleColor, revNo) {
    jQuery(gridOpsTableIdCon).jqGrid({
        url: '/OpsMaster/GetOpMaster',
        postData: {
            styleCode: styleCode, styleSize: styleSize
            , styleColor: styleColor, revNo: revNo
        },
        datatype: "json",
        height: 120,
        scroll: false,
        width: null,
        shrinkToFit: false,
        deepempty: true,
        ignoreCase: true,
        viewrecords: true,
        rowNum: 10,
        rowList: [10, 20, 30, 40],
        pager: gridOpsPaperNameCon,
        gridview: true,
        caption: "OPS",
        colModel: [
            {
                name: 'Edition', index: 'Edition', width: 90, label: arrOpsColNameCon.EDITION
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'StyleCode', index: 'StyleCode', width: 110, label: arrOpsColNameCon.STYLECODE
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'StyleSize', index: 'StyleSize', width: 100, label: arrOpsColNameCon.STYLESIZE, search: true
                , editable: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'StyleColorWays', index: 'StyleColorWays', width: 200, label: arrOpsColNameCon.STYLECOLORSERIAL
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'RevNo', index: 'RevNo', width: 100, label: arrOpsColNameCon.REVNO, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'OpRevNo', index: 'OpRevNo', width: 115, label: arrOpsColNameCon.OPREVNO, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'OpTime', index: 'OpTime', width: 100, label: arrOpsColNameCon.OPTIME, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'OpPrice', index: 'OpPrice', width: 90, label: arrOpsColNameCon.OPPRICE, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'MachineCount', index: 'MachineCount', width: 135, label: arrOpsColNameCon.MACHINECOUNT, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'ConfirmChk', index: 'ConfirmChk', width: 75, label: arrOpsColNameCon.CONFIRMCHK, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'OpCount', index: 'OpCount', width: 95, label: arrOpsColNameCon.OPCOUNT, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'ManCount', index: 'ManCount', width: 90, label: arrOpsColNameCon.MANCOUNT, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'LastUpdateTime', index: 'LastUpdateTime', width: 250, label: arrOpsColNameCon.LASTUPDATEDATE, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'Remarks', index: 'Remarks', width: 250, label: arrOpsColNameCon.REMARKS, align: 'center'
                , editable: true, search: true, searchoptions: { sopt: ['cn', 'eq', 'ne'] }
            },
            {
                name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true 
            }

        ],
        loadError: function (xhr) {
            ShowMessage("", xhr.responseText, Fail);
        },
        loadComplete: function () {
            setTimeout(function () {
                updatePagerIcons();
            }, 0);
        },
        onPaging: function (pgButton) {
            if (pgButton === "records") {

                SetPaging($(gridOpsTableIdCon), gridOpsPaperNameCon);
            }
        },
        onSelectRow: function (rowid) {
            var row = $(gridOpsTableIdCon).jqGrid("getRowData", rowid);
            EventClickRow(row);

            //Save ops master key to local storage
            localStorage.setItem(OpsMasterInfo, JSON.stringify(row));
        },
        gridComplete: function () {
            setTimeout(function () {
                window.updatePagerIcons();
            }, 0);
           
            var opsMaster = JSON.parse(localStorage.getItem(OpsMasterInfo));

            if (!$.isEmptyObject(opsMaster)) {
                
                var rows = jQuery(gridOpsTableIdCon).getDataIDs();
                for (var i = 0; i < rows.length; i++) {
                    var row = jQuery(gridOpsTableIdCon).getRowData(rows[i]);
                    if (row.StyleCode === opsMaster.StyleCode &&
                        row.StyleSize === opsMaster.StyleSize &&
                        row.StyleColorSerial === opsMaster.StyleColorSerial &&
                        row.RevNo === opsMaster.RevNo &&
                        row.OpRevNo === opsMaster.OpRevNo &&
                        row.Edition === opsMaster.Edition) {
                        $(gridOpsTableIdCon).jqGrid("setSelection", rows[i], true);
                     
                        break;
                    }
                }
            }
        }
    });

    //navButtons
    jQuery(gridOpsTableIdCon).jqGrid('navGrid', gridOpsPaperNameCon, {
        //navbar options
        view: true,
        viewicon: 'ace-icon fa fa-search-plus grey',
        edit: false,
        del: false,
        search: true,
        searchicon: 'ace-icon fa fa-search orange',
        refresh: true,
        refreshicon: 'ace-icon fa fa-refresh green'
    });

    $("#pg_" + gridOpsPaperNameCon + " option[value=40]").text(arrButtonAction.all);

    // Bind the navigation and set the onEnter event
    jQuery(gridOpsTableIdCon).jqGrid('bindKeys');

}

//Event when clicking on row of grid ops master.
function EventClickRow(row) {
    //Save ops master key to local storage
    localStorage.setItem("OpsMasterInfo", JSON.stringify(row));

    var styleColor = !isEmpty(row.StyleColorWays) ? row.StyleColorWays.slice(0, 3).toUpperCase() : "";
    //var editionRow = !isEmpty(row.Edition) ? row.Edition.slice(0, 1).toUpperCase() : "";
  
    jQuery(gridOpsTableIdCon).jqGrid('setCaption', " OPS - Style: " + row.StyleCode + " | Size: " + row.StyleSize
        + " | Color: " + styleColor + " | Revision: " + row.RevNo);

    window.ClickRowOpsCon(row);
}