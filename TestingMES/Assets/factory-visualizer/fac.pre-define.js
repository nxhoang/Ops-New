//#region Constants
const UriApiDcmt = `${document.location.origin}/api/ApiDcmt`,
    UriApiFactory = `${document.location.origin}/api/ApiFactory`,
    UriApiLine = `${document.location.origin}/api/ApiLine`,
    UriApiOracleLine = `${document.location.origin}/api/ApiOracleLine`,
    UriApiCstp = `${document.location.origin}/api/ApiCstp`,
    UriApiTableSpace = `${document.location.origin}/api/ApiTableSpace`,
    UriApiOracleTbsp = `${document.location.origin}/api/ApiOracleTpsp`,
    FactoryGrid = "#tbFactory", FactoryPager = "#tbFactoryPager",
    LineAmtopGrid = "#tbAmtopLine", LineAmtopPager = "#tbAmtopLinePager",
    LineMesGrid = "#tbMesLine", LineMesPager = "#tbMesLinePager",
    SeatSize = 35, CbCorporation = "#selCorporation", DesDataBase = { MySqlDb: 1, OracleDb: 2 },
    ToolImageSrc = "http://118.69.170.24:8005/OPS/ToolImages/",
    FactoryCtrl = "/Factory/", OraFacSaveChanges = `${FactoryCtrl}OracleSaveChanges`,
    MsqlFacSaveChanges = `${FactoryCtrl}MySqlSaveChanges`,
    MsqlFacSaveSeatDetails = `${FactoryCtrl}MySqlSaveSeatDetail`,
    OraFacSaveSeatDetails = `${FactoryCtrl}OracleSaveSeatDetail`,
    DivLineActionId = "divLineAction",
    DivLineAction = "#divLineAction", TableShape = { Rectangle: "Rectangle", Circle: "Circle", Square: "Square" },
    TableTypes = { TableC8: "TableC8", TableOO: "TableOO" }, ModalTableName = 'modalTable', ModalTableId = `#${ModalTableName}`,
    CbLineId = "#selLine", TbLocation = "0 0", TabLiOpLayout = "liOpLayout", TabLiOpLineMapping = "liOpLineMapping",
    TabDivMesOpLayout = "mesOpLayout", TabDivOpLineMapping = "opLineMappingTab",
    CtrlOpsLayout = "OpsLayout", UpdateOpmtMxPackageUri = `/${CtrlOpsLayout}/UpdateOpmtMxPackage`,
    CtrlOps = "Ops", OraCreateOp = `/${CtrlOps}/OraCreateOperationPlan`, OraDeleteOpmtUri = `/${CtrlOps}/OraDeleteOpmt`,
    OraGetMaxOpRevNoUri = `/${CtrlOps}/OraGetMaxOpRevNo`, OraAddOpdt = `/${CtrlOps}/OraAddOpdt`,
    OraSyncOp = `/${CtrlOps}/SyncOperationPlan`, LayoutCbLineId = "#cbLine", UrlGetOpdts = `/${CtrlOpsLayout}/GetOpdts`;
//#endregion

//#region Variables
var SubmitMode, SubmitLineMode, NavMode = { Create: "Cr", Update: "U" }, LastSelectedFactoryId,
    SelectedDatabase = DesDataBase.MySqlDb;
//#endregion

//#region Classes
class TableSpace {
    constructor(tableId, tbName, tbLocation, tbAngle, virtualWidth, virtualLength, totalSeat, seatType, seatDistance,
        bgColorTable, workers, factory, lineSerial, tbCategory, tbActualWidth, tbActualLength, tbRate) {
        this.TableId = tableId;
        this.Factory = factory;
        this.LineSerial = lineSerial;
        this.TableName = tbName;
        this.TbCategory = tbCategory;
        this.Angle = tbAngle;
        this.TbLocation = tbLocation;
        this.SeatTotal = totalSeat;
        this.SeatDistance = seatDistance;
        this.BackgroundColor = bgColorTable;
        this.Workers = workers;
        this.SeatType = seatType;
        this.VirtualWidth = virtualWidth;
        this.VirtualLength = virtualLength;
        this.ActualWidth = tbActualWidth;
        this.ActualLength = tbActualLength;
        this.Rate = tbRate;
    }
}

class LineEntity {
    constructor(lineSerial, lineName, factory, lineNo, totalTables, lineMan, backgroundColor, inUse) {
        this.LineSerial = lineSerial;
        this.LineName = lineName;
        this.Factory = factory;
        this.LineNo = lineNo;
        this.TotalTables = totalTables;
        this.LineMan = lineMan;
        this.BackgroundColor = backgroundColor;
        this.InUse = inUse;
    }
}

class MesOpdt extends StyleMaster {
    constructor(edition, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, lineSerial, tableId,
        seatNo) {
        super(styleCode, styleColorSerial, styleSize, revNo);
        this.Edition = edition;
        this.OpRevNo = opRevNo;
        this.OpSerial = opSerial;
        this.LineSerial = lineSerial;
        this.TableId = tableId;
        this.SeatNo = seatNo;
    }
}
//#endregion

//#region Functions
function getSelectedRadioByName(rdName) {
    const radios = document.getElementsByName(rdName);

    for (var i = 0, length = radios.length; i < length; i++) {
        if (radios[i].checked) {
            return radios[i];
        }
    }

    return null;
}

function calculateTableWidth() {
    const txtTotalSeat = document.getElementById("seatTotal");
    const txtSeatDis = document.getElementById("seatDis");

    if (txtTotalSeat.value === "") {
        MsgInform("Inform", "Please input total seat", "error", false, true);
        return;
    }
    if (txtSeatDis.value === "") {
        MsgInform("Inform", "Please input seat distance", "error", false, true);
        return;
    }

    const totalSeat = parseInt(txtTotalSeat.value);
    const seatType = getSelectedRadioByName("seatType");

    if (seatType === undefined || seatType === null) {
        MsgInform("Inform", "Please select Seat Type", "error", false, true);
        return;
    }
    const tbWidthPx = CalculateTableWidth(totalSeat, txtSeatDis.value, seatType.value);
    document.getElementById("txtDisplayWidth").value = parseInt(tbWidthPx);
    document.getElementById("txtDisplayWidth").classList.remove("fac-vs__required-border");
}

function CalculateTableWidth(totalSeat, seatDis, seatType) {
    if (totalSeat && seatDis && seatType) {
        const tableWidth = (totalSeat * SeatSize) + (seatDis * (totalSeat - 1)) + SeatSize;
        let haftWidth = Math.floor(tableWidth / 2);
        let tbWidth;

        switch (seatType) {
            case "1":
            case "2":
                tbWidth = totalSeat % 2 === 1 ? haftWidth + SeatSize : haftWidth + (SeatSize / 2);
                return tbWidth;
            case "9":
            case "10":
            case "11":
            case "12":
                tbWidth = totalSeat % 2 === 1 ? haftWidth + SeatSize : haftWidth + SeatSize + (SeatSize / 2);
                return tbWidth;
            default:
                return tableWidth;
        }
    } else {
        return 0;
    }
}

function validateNumber(e) {
    const v = e.value;
    if (v && e.value !== "" && Number.isInteger(parseInt(v)) && parseInt(v) > 0) {
        e.classList.remove("fac-vs__required-border");
        return true;
    } else {
        e.classList.add("fac-vs__required-border");
        return false;
    }
}

function validateInput(e) {
    if (e.value && e.value !== "") {
        e.classList.remove("required-border");
        return true;
    } else {
        e.classList.add("required-border");
        return false;
    }
}

function showTableFormModal() {
    SubmitMode = NavMode.Create;
    ChangeMode();
    if (LastSelectedFactoryId === null || LastSelectedFactoryId === undefined) {
        MsgInform("Inform", "Please select a factory", "error", false, true);
        return;
    }

    const selectedLineId = $(LineMesGrid).jqGrid('getGridParam', 'selrow');
    if (selectedLineId === null || selectedLineId === undefined) {
        MsgInform("Inform", "Please select a Line", "error", false, true);
        return;
    }

    const selectedLineRow = $(LineMesGrid).jqGrid("getRowData", selectedLineId);
    document.getElementById("txtSeletedLine").value = selectedLineRow.LineName;
    ModalTable.modal('show');
}

function ConvertCmToPx(rate, cm) {
    if (rate && cm) {
        const px = cm * 96 / 2.54 / rate;
        return px;
    } else {
        return null;
    }
}

function toggleProcessDiv() {
    $("#facWorkerDiv").toggle();
    $("#selOpView").toggle();
}
//#endregion Functions