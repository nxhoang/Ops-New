class StyleMaster {
    constructor(styleCode, styleColorSerial, styleSize, revNo) {
        this.StyleCode = styleCode;
        this.StyleColorSerial = styleColorSerial;
        this.StyleSize = styleSize;
        this.RevNo = revNo;
    }
}

class OpMaster extends StyleMaster {
    constructor(styleCode, styleColorSerial, styleSize, revNo, opRevNo) {
        super(styleCode, styleColorSerial, styleSize, revNo);
        this.OpRevNo = opRevNo;
    }
}

class Opmt extends StyleMaster {
    constructor(edition, edition2, styleCode, styleColorSerial, styleSize, revNo, opRevNo, language, groupMode,
    processWidth, processHeight, layoutFontSize, canvasHeight, factory, remarks) {
        super(styleCode, styleColorSerial, styleSize, revNo);
        this.Edition = edition;
        this.Edition2 = edition2;
        this.OpRevNo = opRevNo;
        this.Language = language;
        this.GroupMode = groupMode;
        this.ProcessWidth = processWidth;
        this.ProcessHeight = processHeight;
        this.LayoutFontSize = layoutFontSize;
        this.CanvasHeight = canvasHeight;
        this.Factory = factory;
        this.Remarks = remarks;
    }
}

class Opdt extends Opmt {
    constructor(edition, edition2, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, opName, opGroup,
    machineType, moduleId, nextOp, page, x, y, displayColor) {
        super(edition, edition2, styleCode, styleColorSerial, styleSize, revNo, opRevNo);
        this.OpSerial = opSerial;
        this.OpName = opName;
        this.OpGroup = opGroup;
        this.MachineType = machineType;
        this.ModuleId = moduleId;
        this.NextOp = nextOp;
        this.Page = page;
        this.X = x;
        this.Y = y;
        this.DisplayColor = displayColor;
    }
}

class WorkerOpdt extends OpMaster {
    constructor(styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, employeeCode, xPos, yPos) {
        super(styleCode, styleColorSerial, styleSize, revNo, opRevNo);
        this.OpSerial = opSerial;
        this.EmployeeCode = employeeCode;
        this.XPos = xPos;
        this.YPos = yPos;
    }
}

class RectObj {
    constructor(type, x, y, width, height, borderColor, backgroundColor, textColor, isDragging, isShow, data) {
        this.Type = type;
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
        this.BorderColor = borderColor;
        this.BackgroundColor = backgroundColor;
        this.TextColor = textColor;
        this.IsDragging = isDragging;
        this.IsShow = isShow;
        this.Data = data;
    }
}