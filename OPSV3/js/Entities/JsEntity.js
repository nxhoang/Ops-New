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
        mainProcess, manCount, maxTime, opTime, remarks, stitchesPerInch, stitchingLength, videoFile) {
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
        this.Remarks = remarks;
        this.StitchesPerInch = stitchesPerInch;
        this.StitchingLength = stitchingLength;
        this.VideoFile = videoFile;
    }
}