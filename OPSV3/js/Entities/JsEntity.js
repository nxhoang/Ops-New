class StyleMaster {
    constructor(styleCode, styleColorSerial, styleSize, revNo) {
        this.StyleCode = styleCode;
        this.StyleColorSerial = styleColorSerial;
        this.StyleSize = styleSize;
        this.RevNo = revNo;
    }
}

class Opnt extends StyleMaster {
    constructor(edition, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, opnSerial, opNameId,
        firstGroupLevel, secondGroupLevel, thirdGroupLevel, imageName, iotType, machineCount, machineType,
        mainProcess, manCount, maxTime, opTime, stitchesPerInch, stitchingLength, videoFile, remarks) {
        super(styleCode, styleColorSerial, styleSize, revNo);
        this.Edition = edition;
        this.OpRevNo = opRevNo;
        this.OpSerial = opSerial;
        this.OpnSerial = opnSerial;
        this.OpNameId = opNameId;
        this.GroupLevel_0 = firstGroupLevel;
        this.GroupLevel_1 = secondGroupLevel;
        this.GroupLevel_2 = thirdGroupLevel;
        this.ImageName = imageName;
        this.IotType = iotType;
        this.MachineCount = machineCount;
        this.MachineType = machineType;
        this.MainProcess = mainProcess;
        this.ManCount = manCount;
        this.MaxTime = maxTime;
        this.OpTime = opTime;
        this.OpnSerial = opnSerial;
        this.Remarks = remarks;
        this.StitchesPerInch = stitchesPerInch;
        this.StitchingLength = stitchingLength;
        this.VideoFile = videoFile;
    }
}

class Opnm {
    constructor(opNameId, english, vietnam, indonesia, myanmar, korea, ethiopia, remark, groupLevel, parentId, code,
        machineGroup, machineId, iconName) {
        this.OpNameId = opNameId;
        this.English = english;
        this.Vietnam = vietnam;
        this.Indonesia = indonesia;
        this.Myanmar = myanmar;
        this.Korea = korea;
        this.Ethiopia = ethiopia;
        this.Remark = remark;
        this.GroupLevel = groupLevel;
        this.ParentId = parentId;
        this.Code = code;
        this.MachineGroup = machineGroup;
        this.MachineId = machineId;
        this.IconName = iconName;
    }
}

class Opmt extends StyleMaster {
    constructor(edition, edition2, styleCode, styleColorSerial, styleSize, revNo, opRevNo, language, groupMode, processWidth,
        processHeight, layoutFontSize, canvasHeight, factory, remarks, reason) {
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
        this.Reason = reason;
    }
}

class Opdt extends Opmt {
    constructor(edition, edition2, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, opName, opGroup, machineType,
        moduleId, nextOp, page, x, y, displayColor, opNum) {
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
        this.OpNum = opNum;
    }
}