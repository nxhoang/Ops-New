class Opdt extends Opmt {
    constructor(edition, edition2, styleCode, styleColorSerial, styleSize, revNo, opRevNo, opSerial, opName, opGroup, machineType,
        moduleId, nextOp, page, x, y, displayColor) {
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