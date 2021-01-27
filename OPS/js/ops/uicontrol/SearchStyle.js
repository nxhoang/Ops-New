//interface 
//GetKeyCodeStyle(x,x,x,x);
//GetStyleMaster(search);
//==================var
var myJqgrid = $("#tb_Grid");
var tableName = "tb_Grid";
var tableNavName = "tb_Nav_JqGrid";
var dbclick = 1;

//var SearchText = ''; //HA ADD

arrColname = {
    STYLECODE: "Style Code",
    STYLENAME: "Style Name",
    BuyerName: "Buyer Name",
    BUYERSTYLECODE: "Buyer Style Code",
    BUYERSTYLENAME: "Buyer Style Name",
    STYLESIZE: "Style Size",
    STYLECOLORSERIAL: "Color",
    REVNO: "Revno",
    STATUS: "Status",
    REGISTRYDATE: "Register Date",
    REGISTER_NAME: "Register",
    AD_CONFIRM: "IO Confirm",
    AD_DEV_SALES: "Value"
};

arrButtonName = {
    edittext: "Edit",
    addtext: "Add",
    deltext: "Delete",
    searchtext: "Search",
    refreshtext: "Refresh"
};

arrButtonAction = {
    all: "All"
};

//function ======================================================
//====ready function

function OpenReadySearch() {
    getBuyerByAOQty();
    //AppendBuyer();
    RegisterDateTime();
    OpenCard();
    CreateGrid();
    HiglightCheck();
    //START ADD - SON
    EventPressEnterSearchStyle();
    //END ADD - SON

    //START ADD - SON) 22/Sep/2020
    BindDataToJqGridOpPlan(null, null, null, null, null);
    EventSelectOpEdiOnStyleModal();
    //END ADD - SON) 22/Sep/2020
}

function OpenCard() {
    $("#btnCAD,#txtSearch").click(function () {
        ShowModal("SearchStyleModal");
    });

    $('#searchStyle').keyup(function (e) {
        if (e.keyCode === 13) {
            var value = $(this).val();
            ShowSearch(value);
        } else if (e.which === 27) {
            $(this).val("");
        }
    });

    $("#txtSearch2").click(function () {
        var value = $('#searchStyle').val();
        ShowSearch(value);

        //START ADD: HA
        if (typeof (Storage) !== 'undefined') {
            sessionStorage.setItem('searchStyleCode', value);
        }
        //END ADD: HA
    });

    /*
    $('#btnCAD').click(function () {
        $('.cad-panel').toggleClass('is-visible');
        if ($('[ID*=HiddCADPanel_Visible]').val() == "0") {
            $('[ID*=HiddCADPanel_Visible]').val("1");
        }
        else {
            $('[ID*=HiddCADPanel_Visible]').val("0");
        }
    });
    $('.cad-close').on('click', function (event) {
        $('.cad-panel').toggleClass('is-visible');
        $('[ID*=HiddCADPanel_Visible]').val("0");
    });
    */
}

function ShowSearch(value) {
    ShowModal("SearchStyleModal");
    $("#cbbSearch").val('').trigger("change");
    $("#txtDate").val('');
    $("#txtDate").val('');
    $("#txtEndDate").val('');
    $("#aoNumber").val('');
    if (!$.isEmptyObject(value)) {
        $("#txtFind").val(value);
        $("#btnSearch").trigger("click");
    }
}

function AppendBuyer() {
    var arrBuyer = GetArrayBuyer();
    $("#cbbSearch").empty();
    var option = '';
    option += "<option></option>"; //add empty data
    for (var i = 0; i < arrBuyer.length; i++) {
        option += '<option value="' + arrBuyer[i]["BuyerCode"] + '">' + arrBuyer[i]["BuyerCode"] + " - " + arrBuyer[i]["BuyerName"] + "</option>";
    }
    $('#cbbSearch').append(option);

    //Format dropdownlist to selection
    Selection2('cbbSearch');
    // FillDataToDropDownlist("cbbSearch", arrBuyer, "BuyerCode", "BuyerName");
}

//START ADD - SON) 7/Oct/2020 - Get list buyer base on AO qty
var getBuyerByAOQty = () => {
    let config = ObjectConfigAjaxPost("/UIControl/GetBuyerByAOqty", true, null);
    AjaxGetCommon(config, (respone) => {
        FillDataToDropDownlist("cbbSearch", respone, "SubCode", "CodeName");
    });
}
//START ADD - SON) 7/Oct/2020

function RegisterDateTime() {
    $("#txtDate, #txtEndDate").datepicker({
        format: "yyyy/mm/dd",
        todayHighlight: true,
        autoclose: true
    });
}

function SearchClick() {
    var buyer = $("#cbbSearch").val();
    var start = $("#txtDate").val();
    var end = $("#txtEndDate").val();
    var search = $("#txtFind").val();
    var aoNumber = $("#aoNumber").val();
    var searchType = $("#drpSearchType").val(); //ADD - SON) 7/Oct/2020
    myJqgrid.jqGrid("setGridParam", {
        postData: {
            buyer: buyer, start: start, end: end, search: search, aoNumber: aoNumber, searchType: searchType
        }
    }).trigger("reloadGrid");

    //START ADD - SON

    //Get style master inforamtion
    GetStyleMaster(search.toUpperCase());

    //Set style search information to local storage.
    var objSearch = {
        buyer: buyer,
        start: start,
        end: end,
        search: search,
        aoNumber: aoNumber
    };
    localStorage.setItem(StyleSearchInfo, JSON.stringify(objSearch));

    //END ADD - SON

    //START ADD: HA
    var value = $('#txtFind').val();
    $('#searchStyle').val(value);
    sessionStorage.setItem('searchStyleCode', value);
    //END ADD: HA
}

//START ADD - SON
function EventPressEnterSearchStyle() {
    $("#txtFind").keypress(function (e) {
        var key = e.which;
        if (key === 13) {
            //Press enter key
            $("#btnSearch").click();
        }
    });
}
//END ADD - SON

//======================
function CreateGrid() {
    var buyer = "----";//$("#cbbSearch").val();
    var start = "----";//$("#txtDate").val();
    var end = "----";//$("#txtEndDate").val();
    var search = "----";//$("#txtFind").val();
    var aoNumber = "----";//$("#aoNumber").val();
    let searchType = ''; //ADD - SON) 7/Oct/2020
    myJqgrid.jqGrid({
        pager: tableNavName,
        sortname: "STYLECODE",
        sortorder: "DESC",
        page: 1,
        rowNum: 40,
        rowList: [40, 60, 80, 20],
        scroll: false,
        viewrecords: true,
        scrollrows: true,
        shrinkToFit: false,
        width: null,
        gridview: true,
        height: 180,
        //==========================================
        url: "/UIControl/SearchList",
        //caption: "List style",
        datatype: "json",
        postData: {
            buyer: buyer, start: start, end: end, search: search, aoNumber: aoNumber, searchType: searchType
        },
        //mtype: 'POST',
        colModel: [
            { name: "StyleCode", index: "StyleCode", label: arrColname.STYLECODE, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 100 },
            { name: "StyleName", index: "StyleName", label: arrColname.STYLENAME, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "BuyerName", index: "BuyerName", label: arrColname.BuyerName, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "BuyerStyleCode", index: "BuyerStyleCode", label: arrColname.BUYERSTYLECODE, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 100 },
            { name: "BuyerStyleName", index: "BuyerStyleName", label: arrColname.BUYERSTYLENAME, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "StyleSize", index: "StyleSize", label: arrColname.STYLESIZE, search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 80 },
            { name: "StyleColorSerial", index: "StyleColorSerial", label: arrColname.STYLECOLORSERIAL, hidden: true },
            {
                name: "StyleColorWays", index: "StyleColorWays", label: arrColname.STYLECOLORSERIAL, search: true, width: 120
                //formatter: function (cellvalue, options, rowobject) {
                //    if (cellvalue)
                //        return rowobject.StyleColorSerial + "-" + cellvalue;
                //    return "";
                //}
            },
            { name: "RevNo", index: "RevNo", label: arrColname.REVNO, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 80 },
            { name: "StaTus", index: "StaTus", label: arrColname.STATUS, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "RegistryDate", index: "RegistryDate", label: arrColname.REGISTRYDATE, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "Register", index: "Register", label: arrColname.REGISTER_NAME, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "AdConfirm", index: "AdConfirm", label: arrColname.AD_CONFIRM, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] }, width: 140 },
            { name: "AdDevSale", index: "AdDevSale", label: arrColname.AD_DEV_SALES, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            { name: "Have", index: "Have", hidden: true },
            { name: "AdDevSale", index: "AdDevSale", label: arrColname.AD_DEV_SALES, align: "center", search: true, searchoptions: { sopt: ["cn", "eq", "ne"] } },
            //START ADD - SON
            { name: 'StyleGroup', index: 'StyleGroup', hidden: true },
            { name: 'SubGroup', index: 'SubGroup', hidden: true },
            { name: 'SubSubGroup', index: 'SubSubGroup', hidden: true },
            { name: 'Buyer', index: 'Buyer', hidden: true }
            //END ADD - SON
        ],
        ondblClickRow: function (rowid) {
            //START ADD) SON - 11 December 2019 - If current page is default page then navigate to Plan Management page
            //  Get current page url using JavaScript
            var currentPageUrl = "";
            if (typeof this.href === "undefined") {
                currentPageUrl = document.location.toString().toLowerCase();
            }
            else {
                currentPageUrl = this.href.toString().toLowerCase();
            }

            //Check current page whether is default page or not
            if (currentPageUrl.indexOf("/default/default") !== -1) {

                localStorage.removeItem(OpsMasterInfo);

                var row = $(myJqgrid).jqGrid("getRowData", rowid);
                //Save ops master key to local storage
                localStorage.setItem(StyleMasterInfo, JSON.stringify(row));

                //If current page is not default then navigate to plan management page
                window.location.href = "/Ops/Ops";
            }
            //END ADD) SON - 11 December 2019

            dbclick = 2;
            //SelectRowSearch(rowid);
            HideModal("SearchStyleModal");
            //$('#SearchStyleModal').modal('toggle');
        },
        onSelectRow: function (rowid) {
            // setTimeout because no way to prevent click when you double click
            // this mean: 1 double = 2 click 
            setTimeout(function () {
                if (dbclick === 1) {
                    //console.log("1");
                    SelectRowSearch(rowid);

                    dbclick = 1;
                } else {
                    dbclick = 1;
                }

            }, 300);
        },
        loadComplete: function () {
            updatePagerIcons();
            if ($('#showHiglight').is(":checked")) {
                ShowHiglight();
            }
        },
        onPaging: function (pgButton) {
            if (pgButton === "records") {
                SetPaging(myJqgrid, tableNavName);
            }
        },
        ajaxGridOptions: { async: true } //MOD - SON) 23/Oct/2020 - Change async to true
    }).jqGrid("navGrid", "#" + tableNavName, {
        cloneToTop: true,
        edit: false,
        add: false,
        del: false,
        search: false,
        searchicon: "ace-icon fa fa-search orange",
        searchtext: arrButtonName.searchtext,
        refresh: true, refreshicon: "ace-icon fa fa-refresh green", refreshtext: arrButtonName.refreshtext
    });
    $("#" + tableNavName).find("option[value=20]").text(arrButtonAction.all);
    //merge header 2 column button Save Delete
    SearchFilter(myJqgrid);
}

//START ADD - SON) 22/Sep/2020
function BindDataToJqGridOpPlan(styleCode, styleSize, styleColor, revNo, edition) {
    jQuery("#tbOpPlan").jqGrid({
        url: '/OPS/GetOpMasterByStyle',
        postData: {
            styleCode: styleCode, styleSize: styleSize, styleColor: styleColor, revNo: revNo, edition: edition
        },
        datatype: "json",
        width: null,
        height: 130,
        shrinkToFit: false,
        scroll: false,
        deepempty: true,
        ignoreCase: true,
        viewrecords: true,
        rowNum: 40,
        rowList: [10, 20, 30, 40],
        pager: '#divOpPlanPager',
        gridview: true,
        //caption: "Operation Plan",
        colModel: [
            { name: 'Edition2', index: 'Edition2', width: 110, label: arrOpsColname.EDITION, align: 'center', classes: 'pointer', sortable: false },
            { name: 'StyleCode', index: 'StyleCode', width: 90, label: arrOpsColname.STYLECODE, classes: 'pointer' },
            { name: 'StyleColorWays', index: 'StyleColorWays', width: 200, label: arrOpsColname.STYLECOLORSERIAL, classes: 'pointer' },
            { name: 'BuyerStyleCode', index: 'BuyerStyleCode', width: 120, label: arrOpsColname.BUYERSTYLECODE, classes: 'pointer' },
            { name: 'BuyerStyleName', index: 'BuyerStyleName', width: 250, label: arrOpsColname.BUYERSTYLENAME, classes: 'pointer' },
            { name: 'StyleSize', index: 'StyleSize', width: 90, label: arrOpsColname.STYLESIZE, classes: 'pointer' },
            { name: 'RevNo', index: 'RevNo', width: 90, label: arrOpsColname.REVNO, align: 'center', classes: 'pointer' },
            { name: 'OpRevNo', index: 'OpRevNo', width: 90, label: arrOpsColname.OPREVNO, align: 'center', classes: 'pointer' },
            { name: 'OpTime', index: 'OpTime', width: 90, label: arrOpsColname.OPTIME, align: 'center', classes: 'pointer' },
            { name: 'TotalOpTime', index: 'TotalOpTime', width: 90, label: "Total Time", align: 'center', classes: 'pointer' },
            { name: 'OpPrice', index: 'OpPrice', width: 90, label: arrOpsColname.OPPRICE, align: 'center', classes: 'pointer', hidden: true },
            { name: 'MachineCount', index: 'MachineCount', width: 115, label: arrOpsColname.MACHINECOUNT, align: 'center', classes: 'pointer' },
            { width: 60, label: arrOpsColname.CONFIRMCHK, align: 'center', classes: 'pointer', formatter: showIconConfirmed },
            { name: 'OpCount', index: 'OpCount', width: 90, label: arrOpsColname.OPCOUNT, align: 'center', classes: 'pointer' },
            { name: 'ManCount', index: 'ManCount', width: 90, label: arrOpsColname.MANCOUNT, align: 'center', classes: 'pointer' },
            { name: 'Factory', index: 'Factory', width: 90, label: arrOpsColname.FACTORY, align: 'center', classes: 'pointer' },
            //{ name: 'LastUpdateTime', index: 'LastUpdateTime', width: 150, label: arrOpsColname.LASTUPDATEDATE, align: 'left', classes: 'pointer', formatter: convertDateToString },
            { name: 'LastUpdateTime', index: 'LastUpdateTime', width: 150, label: arrOpsColname.LASTUPDATEDATE, align: 'left', classes: 'pointer', formatter: 'date', formatoptions: { newformat: 'Y-M-d H:m:s' } },
            { name: 'Remarks', index: 'Remarks', width: 250, label: arrOpsColname.REMARKS, align: 'left', classes: 'pointer' },
            { name: 'MxPackage', index: 'MxPackage', width: 250, label: arrOpsColname.MXPACKAGE, align: 'left', classes: 'pointer' }, //ADD) SON - 1/Jul/2019
            { name: 'Edition', index: 'Edition', width: 90, hidden: true },
            { name: 'StyleColorSerial', index: 'StyleColorSerial', hidden: true },
            { name: 'Language', index: 'Language', hidden: true },
            { name: 'ProcessWidth', index: 'ProcessWidth', hidden: true },
            { name: 'ProcessHeight', index: 'ProcessHeight', hidden: true },
            { name: 'GroupMode', index: 'GroupMode', hidden: true },
            { name: 'CanvasHeight', index: 'CanvasHeight', hidden: true },
            { name: 'Buyer', index: 'Buyer', hidden: true },
            { name: 'LayoutFontSize', index: 'LayoutFontSize', hidden: true },
            { name: 'StyleGroup', index: 'StyleGroup', hidden: true },
            { name: 'SubGroup', index: 'SubGroup', hidden: true },
            { name: 'SubSubGroup', index: 'SubSubGroup', hidden: true },
            { name: 'ConfirmChk', index: 'ConfirmChk', hidden: true },
            { name: 'RegisterId', index: 'RegisterId', hidden: true },
            { name: 'ConfirmedId', index: 'ConfirmedId', hidden: true },
            { name: 'GridKey', index: 'GridKey', key: true, hidden: true, formatter: createGridOpmtKey }//ADD - SON) 4/Sep/2020
        ],
        loadError: function (xhr, status, err) {
            ShowMessage("Get Operation Plan Master", err.message, MessageTypeError);
        },
        onPaging: function (pgButton) { },
        ondblClickRow: function (rowid) {            
            //  Get current page url using JavaScript
            var currentPageUrl = "";
            if (typeof this.href === "undefined") {
                currentPageUrl = document.location.toString().toLowerCase();
            }
            else {
                currentPageUrl = this.href.toString().toLowerCase();
            }

            let row = $('#tbOpPlan').jqGrid("getRowData", rowid);
            //Save ops master key to local storage
            localStorage.setItem(StyleMasterInfo, JSON.stringify(row));

            //Check current page whether is default page or not
            if (currentPageUrl.indexOf("/default/default") !== -1) {

                //If current page is not default then navigate to plan management page
                window.location.href = "/Ops/Ops";

                localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

            }

            dbclick = 2;

            HideModal("SearchStyleModal");
        },
        onSelectRow: function (rowid) {

            setTimeout(function () {
                if (dbclick === 1) {
                    //SelectRowRecentPlan(rowid);

                    //get selected row data
                    let row = $('#tbOpPlan').jqGrid("getRowData", rowid);
                    //Save ops master key to local storage
                    localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

                    SetSelectionOpmtRow(row.GridKey);

                    dbclick = 1;
                } else {
                    dbclick = 1;
                }
            }, 300);

        },
        ajaxGridOptions: { async: true }, //MOD - SON) 23/Oct/2020 - Change async from false to true
        loadonce: true,
        gridComplete: function () {
            setTimeout(function () {
                window.updatePagerIcons();
            }, 0);
        },
        loadComplete: function () { },
        beforeSelectRow: function (rowid, e) { return true; },
    });

    AddEditionDropdownOnGridOpPlanHeader();
    
    function showIconConfirmed(cellValue, options, rowObject) {
        if (rowObject.ConfirmChk === ConfirmCheck) {
            return "<label><i class='fa fa-lock'></i></label>";
        }
        return "";
    }

    //START ADD - SON) 4/Sep/2020
    function createGridOpmtKey(cellValue, options, rowObject) {
        return rowObject.Edition + rowObject.StyleCode + rowObject.StyleSize + rowObject.StyleColorSerial + rowObject.RevNo + rowObject.OpRevNo;
    }
    //END ADD - SON) 4/Sep/2020

    function convertDateToString(cellValue, options, rowObject) {
        if (!$.isEmptyObject(rowObject.LastUpdateTime)) {
            var newDate = eval(("new " + rowObject.LastUpdateTime).replace(/\//g, ""))
            return newDate;
        }
        return "";
    }
}

function EventSelectOpEdiOnStyleModal() {
    $("#drpEdiStyleMdl").change(function () {
        //Get search style info
        var stl = JSON.parse(localStorage.getItem(StyleMasterInfo));
        //Reload girdview Ops
        var data = CreateObjStyleKeyCode(stl.StyleCode, stl.StyleSize, stl.StyleColorSerial, stl.RevNo);
        data.edition = $(this).val();
        //ReloadJqGrid(gridOpsTableName, data);
        ReloadJqGrid2LoCal("tbOpPlan", data);
    });
}

function AddEditionDropdownOnGridOpPlanHeader() {
    jQuery('#tbOpPlan').jqGrid('setLabel', 'Edition2',
        "<select id= 'drpEdiStyleMdl' style='height: 27px;'>" +
        "<option value=''>All</option >" +
        "<option value='P'>PDM</option >" +
        "<option value='O'>OPS</option >" +
        "<option value='A'>AOMTOPS</option>" +
        "<option value='M'>MES</option>" +
        "</select> ");
}
//START ADD - SON) 22/Sep/2020

function SelectRowSearch(rowid) {

    //START ADD - SON
    localStorage.removeItem(OpsMasterInfo);

    var row = $(myJqgrid).jqGrid("getRowData", rowid);
    //Save ops master key to local storage
    localStorage.setItem(StyleMasterInfo, JSON.stringify(row));

    //END ADD - SON
    GetKeyCodeStyle(row.StyleCode, row.StyleSize, row.StyleColorSerial, row.RevNo);
    // window.location.href = url + "?styleCode=" + styleCode + "&styleSize=" + styleSize + "&serial=" + styleColorSerial + '&revNo=' + revNo;

    //START ADD - SON) 22/Sep/2020 - Reload operation plan grid on search style modal
    var data = CreateObjStyleKeyCode(row.StyleCode, row.StyleSize, row.StyleColorSerial, row.RevNo);
    ReloadJqGrid2LoCal("tbOpPlan", data);
    //END ADD - SON) 22/Sep/2020
}

function SetAlignCenterPopups(strPopupName) {
    var dlgDiv = $("#" + strPopupName + myJqgrid[0].id);
    DragFormCenter(dlgDiv);
}
function HiglightCheck() {
    $("#showHiglight").change(function () {
        if (this.checked) {
            ShowHiglight();
        } else {
            HideHiglight();
        }
    });
}

function ShowHiglight() {
    $('td[aria-describedby="tb_Grid_Have"]').each(function () {
        if ($(this).html() === "YES") {
            $(this).parent().addClass("highlighthave");
        } else {
            $(this).parent().removeClass("highlighthave");
        }
    });
}
function HideHiglight() {
    $("#tb_Grid tr").removeClass("highlighthave");
}