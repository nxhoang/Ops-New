function getSelectedRadioByName(n) {
    const i = document.getElementsByName(n);
    for (var t = 0, r = i.length; t < r; t++)
        if (i[t].checked)
            return i[t];
    return null
}
function calculateTableWidth() {
    const t = document.getElementById("seatTotal")
        , i = document.getElementById("seatDis");
    if (t.value === "") {
        MsgInform("Inform", "Please input total seat", "error", !1, !0);
        return
    }
    if (i.value === "") {
        MsgInform("Inform", "Please input seat distance", "error", !1, !0);
        return
    }
    const r = parseInt(t.value)
        , n = getSelectedRadioByName("seatType");
    if (n === undefined || n === null) {
        MsgInform("Inform", "Please select Seat Type", "error", !1, !0);
        return
    }
    const u = CalculateTableWidth(r, i.value, n.value);
    document.getElementById("txtDisplayWidth").value = parseInt(u);
    document.getElementById("txtDisplayWidth").classList.remove("fac-vs__required-border")
}
function CalculateTableWidth(n, t, i) {
    if (n && t && i) {
        const u = n * SeatSize + t * (n - 1) + SeatSize;
        let r = Math.floor(u / 2);
        switch (i) {
            case "1":
            case "2":
                return n % 2 == 1 ? r + SeatSize : r + SeatSize / 2;
            case "9":
            case "10":
            case "11":
            case "12":
                return n % 2 == 1 ? r + SeatSize : r + SeatSize + SeatSize / 2;
            default:
                return u
        }
    } else
        return 0
}
function validateNumber(n) {
    const t = n.value;
    return t && n.value !== "" && Number.isInteger(parseInt(t)) && parseInt(t) > 0 ? (n.classList.remove("fac-vs__required-border"),
        !0) : (n.classList.add("fac-vs__required-border"),
            !1)
}
function validateInput(n) {
    return n.value && n.value !== "" ? (n.classList.remove("required-border"),
        !0) : (n.classList.add("required-border"),
            !1)
}
function showTableFormModal() {
    if (SubmitMode = NavMode.Create,
        ChangeMode(),
        LastSelectedFactoryId === null || LastSelectedFactoryId === undefined) {
        MsgInform("Inform", "Please select a factory", "error", !1, !0);
        return
    }
    const n = $(LineMesGrid).jqGrid("getGridParam", "selrow");
    if (n === null || n === undefined) {
        MsgInform("Inform", "Please select a Line", "error", !1, !0);
        return
    }
    const t = $(LineMesGrid).jqGrid("getRowData", n);
    document.getElementById("txtSeletedLine").value = t.LineName;
    ModalTable.modal("show")
}
function ConvertCmToPx(n, t) {
    return n && t ? t * 96 / 2.54 / n : null
}
function toggleProcessDiv() {
    $("#selOpView").slideToggle("slow");
    $("#facWorkerDiv").animate({
        width: "toggle"
    }, "slow");
    $("#iChevron").toggleClass("glyphicon-chevron-right glyphicon-chevron-left")
}
const UriApiDcmt = `${document.location.origin}/api/ApiDcmt`
    , UriApiFactory = `${document.location.origin}/api/ApiFactory`
    , UriApiLine = `${document.location.origin}/api/ApiLine`
    , UriApiOracleLine = `${document.location.origin}/api/ApiOracleLine`
    , UriApiCstp = `${document.location.origin}/api/ApiCstp`
    , UriApiTableSpace = `${document.location.origin}/api/ApiTableSpace`
    , UriApiOracleTbsp = `${document.location.origin}/api/ApiOracleTpsp`
    , FactoryGrid = "#tbFactory"
    , FactoryPager = "#tbFactoryPager"
    , LineAmtopGrid = "#tbAmtopLine"
    , LineAmtopPager = "#tbAmtopLinePager"
    , LineMesGrid = "#tbMesLine"
    , LineMesPager = "#tbMesLinePager"
    , SeatSize = 35
    , CbCorporation = "#selCorporation"
    , DesDataBase = {
        MySqlDb: 1,
        OracleDb: 2
    }
    , ToolImageSrc = "http://118.69.170.24:8005/OPS/ToolImages/"
    , FactoryCtrl = "/Factory/"
    , OraFacSaveChanges = `${FactoryCtrl}OracleSaveChanges`
    , MsqlFacSaveChanges = `${FactoryCtrl}MySqlSaveChanges`
    , MsqlFacSaveSeatDetails = `${FactoryCtrl}MySqlSaveSeatDetail`
    , SaveOpstChanges = `${FactoryCtrl}SaveChangeOpst`
    , FacSaveLink = `${FactoryCtrl}SaveLink`
    , FacGetLinksByMxPackage = `${FactoryCtrl}GetLinksByMxPackage`
    , FacDeleteLink = `${FactoryCtrl}DeleteLink`
    , FacSaveOpst = `${FactoryCtrl}SaveOpst`
    , OraFacSaveSeatDetails = `${FactoryCtrl}OracleSaveSeatDetail`
    , DivLineActionId = "divLineAction"
    , GetOpstsByMxPackageUrl = `${FactoryCtrl}GetOpstsByMxPackage`
    , GetOpsmsByMxPackageUrl = `${FactoryCtrl}GetOpsmsByMxPackage`
    , SaveOpsmsUrl = `${FactoryCtrl}SaveOpsms`
    , DivLineAction = "#divLineAction"
    , TableShape = {
        Rectangle: "Rectangle",
        Circle: "Circle",
        Square: "Square"
    }
    , TableTypes = {
        TableC8: "TableC8",
        TableOO: "TableOO"
    }
    , ModalTableName = "modalTable"
    , ModalTableId = `#${ModalTableName}`
    , CbLineId = "#selLine"
    , TbLocation = "0 0"
    , TabLiOpLayout = "liOpLayout"
    , TabLiOpLineMapping = "liOpLineMapping"
    , TabLiEmployeeOpMapping = "liEmployeeOpMapping"
    , TabOpEmployeeMapping = "opEmployeeMappingTab"
    , TabDivMesOpLayout = "mesOpLayout"
    , TabDivOpLineMapping = "opLineMappingTab"
    , CtrlOpsLayout = "OpsLayout"
    , UpdateOpmtMxPackageUri = `/${CtrlOpsLayout}/UpdateOpmtMxPackage`
    , CtrlOps = "Ops"
    , OraCreateOp = `/${CtrlOps}/OraCreateOperationPlan`
    , OraDeleteOpmtUri = `/${CtrlOps}/OraDeleteOpmt`
    , OraGetMaxOpRevNoUri = `/${CtrlOps}/OraGetMaxOpRevNo`
    , OraAddOpdt = `/${CtrlOps}/OraAddOpdt`
    , OraSyncOp = `/${CtrlOps}/SyncOperationPlan`
    , LayoutCbLineId = "#cbLine"
    , UrlGetOpdts = `/${CtrlOpsLayout}/GetOp`
    , FlowType = {
        Process: 0,
        Table: 1
    }
    , DeleteLineFlowCb = "cbDeleteLineFlow"
    , FacDeleteTimeBar = `${FactoryCtrl}DeleteOpst`
    , TimelineType = {
        Line: 0,
        Module: 1
    }
    , MdWidth = 130
    , MdHeight = 40
    , MdAngle = 0
    , SaveOpsmsChanges = `${FactoryCtrl}SaveChangeOpsms`
    , BtnSaveWorkerMap = "btnSaveOpEmpChanges";
var SubmitMode, SubmitLineMode, NavMode = {
    Create: "Cr",
    Update: "U"
}, LastSelectedFactoryId, SelectedDatabase = DesDataBase.MySqlDb;
class TableSpace {
    constructor(n, t, i, r, u, f, e, o, s, h, c, l, a, v, y, p, w) {
        this.TableId = n;
        this.Factory = l;
        this.LineSerial = a;
        this.TableName = t;
        this.TbCategory = v;
        this.Angle = r;
        this.TbLocation = i;
        this.SeatTotal = e;
        this.SeatDistance = s;
        this.BackgroundColor = h;
        this.Workers = c;
        this.SeatType = o;
        this.VirtualWidth = u;
        this.VirtualLength = f;
        this.ActualWidth = y;
        this.ActualLength = p;
        this.Rate = w
    }
}
class LineEntity {
    constructor(n, t, i, r, u, f, e, o) {
        this.LineSerial = n;
        this.LineName = t;
        this.Factory = i;
        this.LineNo = r;
        this.TotalTables = u;
        this.LineMan = f;
        this.BackgroundColor = e;
        this.InUse = o
    }
}
class MesOpdt extends StyleMaster {
    constructor(n, t, i, r, u, f, e, o, s, h) {
        super(t, i, r, u);
        this.Edition = n;
        this.OpRevNo = f;
        this.OpSerial = e;
        this.LineSerial = o;
        this.TableId = s;
        this.SeatNo = h
    }
}
class Opls {
    constructor(n, t, i) {
        this.MxPackage = n;
        this.FromTable = t;
        this.ToTable = i
    }
}
class Opst {
    constructor(n, t, i, r, u, f, e, o, s, h, c, l, a, v, y, p) {
        this.Key = n;
        this.TimeLineId = t;
        this.LineSerial = i;
        this.TableId = r;
        this.MxPackage = u;
        this.OpTime = f;
        this.StartTime = e;
        this.EndTime = o;
        this.TableWidth = s;
        this.Height = h;
        this.Location = c;
        this.Angle = l;
        this.Length = a;
        this.Color = v;
        this.Type = y;
        this.GroupId = p
    }
}
class Opsm {
    constructor(n, t, i, r, u, f, e, o, s, h, c, l, a) {
        this.key = n;
        this.Id = t;
        this.Name = i;
        this.GroupId = r;
        this.MxPackage = u;
        this.Factory = f;
        this.OpTime = e;
        this.Location = o;
        this.Width = s;
        this.Height = h;
        this.Angle = c;
        this.BackgroundColor = l;
        this.NextModule = a
    }
}
