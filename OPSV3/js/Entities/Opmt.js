class Opmt extends StyleMaster {
    constructor(edition, edition2, styleCode, styleColorSerial, styleSize, revNo, opRevNo, language, groupMode, processWidth,
        processHeight, layoutFontSize, canvasHeight, factory, remarks) {
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