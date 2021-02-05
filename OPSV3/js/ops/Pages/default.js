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

function ConvertDate(date) {
    var fullDate = new Date(date.match(/\d+/)[0] * 1);
    var twoDigitMonth = fullDate.getMonth() + 1 + "";
    if (twoDigitMonth.length === 1)
        twoDigitMonth = "0" + twoDigitMonth;
    var twoDigitDate = fullDate.getDate() + "";
    if (twoDigitDate.length === 1)
        twoDigitDate = "0" + twoDigitDate;
    var hours = fullDate.getHours() + "";
    if (hours.length === 1)
        hours = "0" + hours;
    var minutes = fullDate.getMinutes() + "";
    if (minutes.length === 1)
        minutes = "0" + minutes;
    var seconds = fullDate.getSeconds() + "";
    if (seconds.length === 1)
        seconds = "0" + seconds;
    var currentDate = twoDigitDate + "/" + twoDigitMonth + "/" + fullDate.getFullYear() + " - " + hours + ":" + minutes + ":" + seconds;
    return currentDate;
}

function CreateGridStyle() {
    $("#tbStyle").jqGrid({
        scroll: false,
        viewrecords: true,
        scrollrows: true,
        shrinkToFit: false,
        width: null,
        height: 368,
        gridview: true,
        //==========================================
        url: "/Default/GetNewStyle",
        //caption: "New development requests",
        hiddengrid: true,
        datatype: "json",
        //mtype: 'POST',
        colModel: [
            { name: "StyleCode", index: "StyleCode", label: arrColname.STYLECODE, align: "center", width: 80, sortable: false },
            { name: "StyleSize", index: "StyleSize", label: arrColname.STYLESIZE, align: "center", width: 80, sortable: false },
            { name: "StyleColorWays", index: "StyleColorWays", label: arrColname.STYLECOLORSERIAL, width: 80, sortable: false },
            { name: "RevNo", index: "RevNo", label: arrColname.REVNO, align: "center", width: 60, sortable: false },
            { name: "StyleName", index: "StyleName", label: arrColname.STYLENAME, sortable: false, width: 130 },
            { name: "", index: "", label: "Plan", width: 80, sortable: false, formatter: LinkOps, align: "center" },
            { name: "", index: "", label: "Layout", width: 90, sortable: false, formatter: LinkLayout, align: "center" },
            { name: "", index: "", label: "Engineering Files", width: 120, sortable: false, formatter: Linking, align: "center" },
            { name: "Buyer", index: "Buyer", label: arrColname.BUYERSTYLECODE, align: "center", hidden: true },
            { name: "RegistryDate", index: "RegistryDate", label: arrColname.REGISTRYDATE, align: "center", hidden: true },
            { name: "StyleColorSerial", index: "StyleColorSerial", hidden: true },
            { name: "BuyerStyleCode", index: "BuyerStyleCode", hidden: true },
            { name: "StyleGroup", index: "StyleGroup", hidden: true },
            { name: 'SubGroup', index: 'SubGroup', hidden: true },
            { name: 'SubSubGroup', index: 'SubSubGroup', hidden: true }
        ]
    });
}

function CreateGridAction(sysId, funcId, user) {
    var tbId = "#tbOpsMt";
    var title = "Recent operation plans worked on by ";
    //var collap = false;
    var collap = true;
    if (funcId === "LIN") {
        tbId = "#tbOpsLink";
        title = "Recent details linking done by ";
        collap = true;
    }
    title = title + user;
    $(tbId).jqGrid({
        scroll: false,
        viewrecords: true,
        scrollrows: true,
        shrinkToFit: false,
        //caption: title,
        hiddengrid: collap,
        width: null,
        height: 368,
        gridview: true,
        //==========================================
        url: "/Default/GetLogByLogin",
        postData: {
            sysId: sysId, funcId: funcId
        },
        datatype: "json",
        //mtype: 'POST',
        colModel: [
            { name: "StyleCode", index: "StyleCode", label: arrColname.STYLECODE, width: 90, sortable: false, align: "center" },
            { name: "StyleSize", index: "StyleSize", label: arrColname.STYLESIZE, width: 90, sortable: false, align: "center" },
            { name: "StyleColorSerial", index: "StyleColorSerial", label: arrColname.STYLECOLORSERIAL, align: "center", width: 70, sortable: false },
            { name: "RevNo", index: "RevNo", label: "Revision", align: "center", width: 80, sortable: false },
            { name: "OpRevNo", index: "OpRevNo", label: "Op Rev", align: "center", width: 80, sortable: false },
            { name: "TransactionTime", index: "TransactionTime", formatter: formaterTime, label: "Action Date", align: "center", width: 100, sortable: false },
            { name: "Remark", index: "Remark", label: "  Action", align: "left", width: 140, sortable: false },
            { name: "Edition", index: "Edition", hidden: true },
            { name: "FunctionId", index: "FunctionId", hidden: true },
            { name: "OperationId", index: "OperationId", hidden: true },
            { name: "BuyerStyleCode", index: "BuyerStyleCode", hidden: true },
            { name: "Buyer", index: "Buyer", hidden: true },
            { name: "StyleGroup", index: "StyleGroup", hidden: true },
            { name: 'SubGroup', index: 'SubGroup', hidden: true },
            { name: 'SubSubGroup', index: 'SubSubGroup', hidden: true },
            { name: 'Buyer', index: 'Buyer', hidden: true }//ADD - SON) 1/Feb/2021
        ],
        onSelectRow: function (rowid) {
            var row = $(tbId).getRowData(rowid);
            if (row.Remark !== "Delete process.") {
                //localStorage.removeItem(OpsMasterInfo);
                localStorage.setItem(StyleMasterInfo, JSON.stringify(row));
                row.IsDetail = 1;
                localStorage.setItem(OpsMasterInfo, JSON.stringify(row));
                switch (funcId) {
                    case "RES":
                        var functionId = row.FunctionId;
                        if (functionId === "LAY") {
                            window.location.href = "/OpLayout/OpLayout";
                        } else {                            
                            //window.location.href = "/Ops/Ops"; //MOD - SON) 26/Dec/2020
                            window.location.href = "/PlanManagement/PlanManagement";
                        }
                        break;
                    case "LIN":
                        window.location.href = "/OpsLink/index";
                        break;
                    case "LAY":
                        window.location.href = "/OpLayout/OpLayout";
                        break;
                }
            }

        }
    });
}

function formaterTime(cellvalue, options, rowObject) {
    var rs = ConvertDate(cellvalue);
    return rs;
}

function LinkOps(cellvalue, options, rowObject) {
    var id = options.rowId;
    var html = "<a class ='btn btn-xs btn-primary btn-x' title ='Plan' onclick=\"LinkToPage('" + id + "','ops')\" >Plan</a>";
    return html;
}
function LinkLayout(cellvalue, options, rowObject) {
    var id = options.rowId;
    var html = "<a class ='btn btn-xs btn-primary btn-x' title ='Layout' onclick=\"LinkToPage('" + id + "','layout')\" >Layout</a>";
    return html;
}
function Linking(cellvalue, options, rowObject) {
    var id = options.rowId;
    var html = "<a class ='btn btn-xs btn-primary btn-x' title ='Engineering Files' onclick=\"LinkToPage('" + id + "','linking')\" >Engg' File</a>";
    return html;
}

function LinkToPage(id, page) {
    var row = $("#tbStyle").getRowData(id);
    localStorage.removeItem(OpsMasterInfo);
    localStorage.setItem(StyleMasterInfo, JSON.stringify(row));
    switch (page) {
        case "ops":
            //window.location.href = "/Ops/ops"; //MOD - SON) 26/Dec/2020
            window.location.href = "/PlanManagement/PlanManagement";
            break;
        case "layout":
            window.location.href = "/OpLayout/OpLayout";
            break;
        case "linking":
            window.location.href = "/OpsLink/index";
            break;
        default:
            //window.location.href = "/Ops/ops"; //MOD - SON) 26/Dec/2020
            window.location.href = "/PlanManagement/PlanManagement";
            break;
    }            
}
//user infor ==============================================
function GetRoleByID(RoleID, uName) {
    var opToolConfig = {
        url: "/Account/GetRoleByID",
        postData: JSON.stringify({ RoleID: RoleID })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        $("#uprofile").text(uName + " (logged in as role " + RoleID + " - " + response.ROLEDESC + ")");
        return response;
    });
}

function GetUserInfo() {
    var opToolConfig = {
        url: "/Account/GetUserInfo",
        postData: {}
    };
    AjaxPostCommon(opToolConfig, function (response) {
        AppentInfor(response);
        return response;
    });
}

function GetLastLogin(sysId, funcId) {
    var opToolConfig = {
        url: "/Default/GetActlByLog",
        postData: JSON.stringify({ sysId: sysId, funcId: funcId })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        $("#lastLogin").text(ConvertDate(response.TransactionTime));
        return response;
    });
}

var src = 'http://203.113.151.204:8080/BETAPDM/User/';

function AppentInfor(user) {
    //$("#lblName").text(user.Name);
    $("#lblTel").text(user.Tel);
    $("#lblEmail").text(user.Email);
    var substr = $("#hdUsername").val();

    var url = src + substr + '.jpg';
    $("#testAvata").attr("src", url);
    setTimeout(function () {
        var newSrc = $("#testAvata").attr("src");
        $("#uAvata").attr("src", newSrc);
    }, 200)
}

function ChangeImage() {
    UploadFile();
}

function imgChangeError(image) {
    var substr = $("#hdUsername").val();
    var url = src + substr + '.png';
    image.onerror = "";
    image.src = url;
    return true;
}

function ConvertSex(x) {
    if (x === "F")
        return "Female";
    if (x === "M") return "Male";
    return "Not Sure";
}
//Change infomation==============
function ChangeImageBtn() {
    $("#fUrl").change(function (evt) {
        
        var fileUpload = $(this).get(0).files;
        var extn = fileUpload[0].name.split('.').pop();
        var newFileName = $("#hdUsername").val() + "." + extn;
        // var files = fileUpload.files;
        readURL(this, "#uAvata");
        ////////////
        var files = $("#fUrl").get(0);
        var formData = new FormData();
        //readURL(files, "#imgMachine");
        var fileimage = files.files;
        for (let i = 0; i < fileimage.length; i++) {
            formData.append(fileimage[i].name, fileimage[i]);
        }
        $.ajax({
            type: "POST",
            url: "/Upload/UploadImageFPT",
            data: formData,
            contentType: false,
            processData: false,
            success: function (result) {
                return true;
            },
            error: function (ex) {
                console.log("err");
            }
        });

    });
}
function ChangeInformation() {
    ShowModal("UpdateUserInfor");
    var src = $("#uAvata").attr("src");
    $("#imgAvata").attr("src",src);
    $("#txtName").val($("#lblName").text());
    $("#txtTel").val($("#lblTel").text());
    $("#Email").val($("#lblEmail").text());
    var x = $("#lblSex").text();
    $("#cbSex").val(x.charAt(0));
}

function SaveInfor() {
    var Name = $("#txtName").val();
    var Tel = $("#txtTel").val();
    var Email = $("#Email").val();
    var Sex = $("#cbSex").val();
    //var Url = $("#").val();
    if (Sex === "N") {
        Sex = "";
    }
    ShowHideValidateUpdate("", false);
    if ($.isEmptyObject(Name)) {
        ShowHideValidateUpdate("Please input name", true);
        $("#txtName").addClass("error");
        return false;
    }
    if (!$.isEmptyObject(Email) && !validateEmail(Email)) {
        ShowHideValidateUpdate("Email invalid", true);
        $("#Email").addClass("error");
        return false;
    }
    var opToolConfig = {
        url: "/Account/ChangeUserInfo",
        postData: JSON.stringify({ Name: Name, Email: Email, Tel: Tel, Sex: Sex })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        if (response) {
            $("#lblName").text(Name);
            $("#lblTel").text(Tel);
            $("#lblSex").text(ConvertSex(Sex));
            var src = $("#imgAvata").attr("src");
            $("#uAvata").attr("src", src);
            UploadFile();
            HideModal("UpdateUserInfor");
        }
    });
}

function ShowHideValidateUpdate(value, show) {
    if (show) {
        $("#showUpdateInfo").text(value);
        $("#showUpdateInfo").show();
    } else {
        $("#showUpdateInfo").hide();
        $("#txtName").removeClass("error");
        $("#Email").removeClass("error");
    }
}

function validateEmail(email) {
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function UploadFile() {
    var files = $("#fUrl").get(0);
    var formData = new FormData();
    //readURL(files, "#imgMachine");
    var fileimage = files.files;
    for (let i = 0; i < fileimage.length; i++) {
        formData.append(fileimage[i].name, fileimage[i]);
    }
    $.ajax({
        type: "POST",
        url: "/Upload/UploadImageFPT",
        data: formData,
        contentType: false,
        processData: false,
        success: function (result) {
            //ShowImageUpload(result);
            return true;
        },
        error: function (ex) {
            console.log("err");
        }
    });
}

//change password====================
function ChangePassword() {
    ShowModal("ChangePass");
}


function ChangePass() {
    var oldpass = $("#txtOlpass").val();
    var passWord = $("#txtNewpassword").val();
    var confirmPassWord = $("#txtConfirmpassword").val();
    ShowHideValidate(false);
    if (passWord !== confirmPassWord) {
        ShowHideValidate("Password and confirm password is not math", true);
        $("#txtNewpassword").addClass("error");
        $("#txtConfirmpassword").addClass("error");
    }
    
    var opToolConfig = {
        url: "/Account/CheckPass",
        postData: JSON.stringify({ Password: oldpass })
    };
    AjaxPostCommon(opToolConfig, function (response) {
        if (response === true) {
            var appConfig = {
                url: "/Account/ChangePass",
                postData: JSON.stringify({ Password: passWord })
            };
            AjaxPostCommon(appConfig, function (response) {
                if (response) {
                    // login
                    window.location.href = "/Account/Login";
                }
            });
        } else {
            ShowHideValidate("Password is not match", true);
            $("#txtOlpass").addClass("error");
        }
    });
}

function ShowHideValidate(value, show) {
    if (show) {
        $("#showChangePass").text(value);
        $("#validateChangePass").show();
    } else {
        $("#validateChangePass").hide();
        $("#txtNewpassword").removeClass("error");
        $("#txtOlpass").removeClass("error");
    }
}

//START ADD - SON) 9/Oct/2020
function binDataToHistoryPlanGrid(buyer, styleInf, recentDay) {

    jQuery('#tbHistoryPlan').jqGrid({
        pager: 'divHistoryPlanPager',
        page: 1,
        rowNum: 40,
        rowList: [40, 60, 80, 20],
        scroll: false,
        viewrecords: true,
        scrollrows: true,
        shrinkToFit: false,
        width: null,
        gridview: true,
        height: 300,
        url: "/UIControl/SearchRecentPlan",
        datatype: "json",
        postData: {
            buyer: buyer, styleInf: styleInf, recentDay: recentDay
        },
        colModel: [
            { name: "Edition2", index: "Edition2", label: 'Edition', align: "center", width: 100 },
            { name: "StyleCode", index: "StyleCode", label: 'STYLE CODE', align: "center", width: 100 },
            { name: "StyleSize", index: "StyleSize", label: 'STYLE SIZE', align: "center", width: 80 },
            { name: "StyleColorWays", index: "StyleColorWays", label: 'COLOR', width: 150 },
            { name: "RevNo", index: "RevNo", label: 'REVNO', align: "center", width: 80 },
            { name: "OpRevNo", index: "OpRevNo", label: 'OP REVNO', align: "center", width: 80 },
            { name: "StyleName", index: "StyleName", label: 'STYLENAME', width: 150 },
            { name: "BuyerStyleCode", index: "BuyerStyleCode", label: 'BUYER STYLE CODE', width: 150 },
            { name: "BuyerStyleName", index: "BuyerStyleName", label: 'BUYER STYLE NAME', width: 150 },
            { name: "FactoryName", index: "FactoryName", label: 'FACTORY', width: 150 },
            { name: "LastUpdateTime", index: "LastUpdateTime", label: 'LAST UPDATE', formatter: 'date', formatoptions: { newformat: 'd-M-Y H:m:s' } },
            { name: "RegistryDate", index: "RegistryDate", label: 'REGISTRY DATE', formatter: 'date', formatoptions: { newformat: 'd-M-Y H:m:s' } },
            { name: "RegisterName", index: "RegisterName", label: 'REGISTER', align: "left" },
            { name: "RegisterId", index: "RegisterId", hidden: true },
            { name: "StyleColorSerial", index: "StyleColorSerial", hidden: true },
            { name: "Edition", index: "Edition", hidden: true },
            { name: 'Buyer', index: 'Buyer', hidden: true }//ADD - SON) 1/Feb/2021
        ],
        ondblClickRow: function (rowid) {
            var row = $('#tbHistoryPlan').jqGrid("getRowData", rowid);
            //Save ops master key to local storage
            localStorage.setItem(StyleMasterInfo, JSON.stringify(row));
            //Keep Operation Plan in local storage
            localStorage.setItem(OpsMasterInfo, JSON.stringify(row));
            //Navigate to plan registry page
            //window.location.href = "/Ops/Ops"; //MOD - SON) 26/Dec/2020
            window.location.href = "/PlanManagement/PlanManagement";
        },
        onSelectRow: function (rowid) {
            setTimeout(function () {
                //get selected row data
                let row = $('#tbHistoryPlan').jqGrid("getRowData", rowid);
                //Save style key to local storage
                localStorage.setItem(StyleMasterInfo, JSON.stringify(row));
                //Save ops master key to local storage
                localStorage.setItem(OpsMasterInfo, JSON.stringify(row));

            }, 300);
        },
        loadComplete: function () {
            updatePagerIcons();
        }
    });
}

//END ADD - SON) 9/Oct/2020