//#region Variables
var tbWorkingSheetId = "tbWorkingSheet";
//#endregion
 
function initWorkingSheetPage() {
    //init gridview working sheet
    bindDataToJqGridWorkingSheet(null, null);

    eventButtonsSync();  

    document.getElementById('txtYearMonthSync').DatePickerX.init({
        format: 'yyyy/mm'
    });

    //Fill list weekno to dropdownlist
    FillDataToDropDownlist("drpWeekNoSync", CreateWeekNo(), "WeekNo", "WeekName");

    //Get list factory
    getListFactories(function (facList) {
        console.log(facList);
        FillDataToMultipleSelect("txtFactorySync", facList, "FACTORY", "NAME");
    });
}

function getListFactories(callBack) {
    var config = ObjectConfigAjaxPost("/FactorySortingParameter/GetFactoryList", false
        , JSON.stringify({ }));
    AjaxPostCommon(config, function (facList) {
        callBack(facList);
    });
}

function getFactoryWorkersNumber(factory, yyMM) {
    
    var config = ObjectConfigAjaxPost("../Factory/GetFactoryWorkersNumber", false
        , JSON.stringify({ factory: factory, yyMM: yyMM}));
    AjaxPostCommon(config, function (syncRes) {
        if (syncRes.IsSuccess && syncRes.Data !== null) {
            $("#txtTotalWorker").val(syncRes.Data.WORKER);
            $("#txtTotalSewer").val(syncRes.Data.SEWER);
        } else {
            $("#txtTotalWorker").val("");
            $("#txtTotalSewer").val("");
            ShowMessage("Get the number of workers in factory", syncRes.Message, ObjMessageType.Error);
        }
    });
}

//#region Bindata to gridview
function bindDataToJqGridWorkingSheet(factory, startDate) {

    jQuery("#" + tbWorkingSheetId).jqGrid({
        url: '/Factory/GetWorkingSheet',
        postData: {
            factory: factory, startDate: startDate
        },
        datatype: "json",
        //height: 'auto',
        colModel: [
            { name: '', index: '', width: 50, label: "Check", hidden: true },
            { name: 'LineNo', index: 'LineNo', width: 70, label: "Line", classes: 'pointer', hidden: true  },
            { name: 'LineName', index: 'LineName', width: 100, label: "Line Name", classes: 'pointer', hidden: true   },
            { name: 'ObjectName', index: 'ObjectName', width: 120, label: " ", classes: 'pointer' },
            { name: 'D1', index: 'D1', width: 35, label: "1" },
            { name: 'D2', index: 'D2', width: 35, label: "2" },
            { name: 'D3', index: 'D3', width: 35, label: "3" },
            { name: 'D4', index: 'D4', width: 35, label: "4" },
            { name: 'D5', index: 'D5', width: 35, label: "5" },
            { name: 'D6', index: 'D6', width: 35, label: "6" },
            { name: 'D7', index: 'D7', width: 35, label: "7" },
            { name: 'D8', index: 'D8', width: 35, label: "8" },
            { name: 'D9', index: 'D9', width: 35, label: "9" },
            { name: 'D10', index: 'D10', width: 35, label: "10" },       
            { name: 'D11', index: 'D11', width: 35, label: "11" },            
            { name: 'D12', index: 'D12', width: 35, label: "12" },            
            { name: 'D13', index: 'D13', width: 35, label: "13" },            
            { name: 'D14', index: 'D14', width: 35, label: "14" },            
            { name: 'D15', index: 'D15', width: 35, label: "15" },            
            { name: 'D16', index: 'D16', width: 35, label: "16" },            
            { name: 'D17', index: 'D17', width: 35, label: "17" },            
            { name: 'D18', index: 'D18', width: 35, label: "18" },            
            { name: 'D19', index: 'D19', width: 35, label: "19" },            
            { name: 'D20', index: 'D20', width: 35, label: "20" },           
            { name: 'D21', index: 'D21', width: 35, label: "21" },           
            { name: 'D22', index: 'D22', width: 35, label: "22" },           
            { name: 'D23', index: 'D23', width: 35, label: "23" },           
            { name: 'D24', index: 'D24', width: 35, label: "24" },           
            { name: 'D25', index: 'D25', width: 35, label: "25" },           
            { name: 'D26', index: 'D26', width: 35, label: "26" },           
            { name: 'D27', index: 'D27', width: 35, label: "27" },           
            { name: 'D28', index: 'D28', width: 35, label: "28" },           
            { name: 'D29', index: 'D29', width: 35, label: "29" },           
            { name: 'D30', index: 'D30', width: 35, label: "30" },           
            { name: 'D31', index: 'D31', width: 35, label: "31" }
        ],
        rowNum: 1000,
        height: 200,
        //rowList: [10, 20, 30],
        //pager: paperGroupPackageId,
        //sortname: 'id',
        //toolbarfilter: true,
        //viewrecords: true,
        sortorder: "asc",
        loadonce: true,
        width: null,
        shrinkToFit: false,
        gridComplete: function () {

        },
        loaderror: function (xhr, status, err) {
            alert("error - get working sheet: " + err);
        },      
        onSelectRow: function (rowid) {
            //const dataRow = $("#" + tbWorkingSheetId).jqGrid("getRowData", rowid);
        },
        loadcomplete: function () {
           
        },
        autowidth: true
    });


    /* Add tooltips */
    $('.navtable .ui-pg-button').tooltip({
        container: 'body'
    });

    //Custom jqgrid css
    customJqGridCss();

    $(window).on('resize.jqGrid', function () {
        //$("#" + tbWorkingSheetId).jqGrid('setGridWidth', $("#wid-id-fwts").width() - 32); 
    });    
}
//#endregion