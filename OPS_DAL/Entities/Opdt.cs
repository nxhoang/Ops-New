using System;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Operation detail maps to T_OP_OPDT(OPS Edition), T_SD_OPDT(PDM Edition), T_MT_OPDT(AOM) and T_MX_OPDT(MES Edition)
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    /// <seealso cref="StyleMaster" />
    public class Opdt : StyleMaster
    {
        #region Properties

        public string OpRevNo { get; set; }
        public int OpSerial { get; set; }
        public string EmployeeCode { get; set; }
        public int OpType { get; set; }
        public string OpNum { get; set; }
        public string OpGroup { get; set; }
        public string OpName { get; set; }

        // For specific languages
        public string GbOpName { get; set; }
        public string VnOpName { get; set; }
        public string IdOpName { get; set; }
        public string MmOpName { get; set; }
        public string KrOpName { get; set; }
        public string EtOpName { get; set; }
        
        public string Factory { get; set; }
        public string MachineType { get; set; }
        public string MachineName { get; set; }
        public string ThreadColor { get; set; }
        public string OpDesc { get; set; }
        public int OpTime { get; set; }
        public decimal OpPrice { get; set; }
        public decimal OfferOpPrice { get; set; }
        public int MachineCount { get; set; }
        public string Remarks { get; set; }
        public int MaxTime { get; set; }
        public float ManCount { get; set; }
        public string FileName { get; set; }
        public string MaxTop { get; set; }
        public string NextOp { get; set; }
        public string OutSourced { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string UiTop
        {
            get
            {
                var arrY = Y?.Split('.');
                if (arrY != null && arrY.Length <= 1)
                {
                    return Y;
                }
                var top = arrY?[1];
                return top;
            }
            set => Y = value;
        }
        public string UiLeft
        {
            get
            {
                var arrX = X?.Split('.');
                if (arrX != null && arrX.Length <= 1)
                {
                    return X;
                }
                var left = arrX?[1];
                return left;
            }
            set => X = value;
        }
        public string ImageName { get; set; }
        public string DisplayColor { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Edition { get; set; }
        public decimal Page { get; set; }
        public string GroupColor { get; set; }
        public string RegisterId { get; set; }
        public DateTime? RegistryDate { get; set; }
        public string ConfirmedId { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public decimal TargetOpPrice { get; set; }
        public string VideoFile { get; set; }
        public string JobType { get; set; }
        public string ModuleId { get; set; }
        public string OpNameRef { get; set; }
        public decimal OpNameId { get; set; }
        public string ArrOpNameId { get; set; }
        public string HotSpot { get; set; }
        public string ToolId { get; set; }
        public string ToolName { get; set; }
        public decimal ToolCount { get; set; }
        public string OpsState { get; set; }
        public int OpTimeBalancing { get; set; }
        public string HasFile { get; set; }
        public string HasManyFiles { get; set; }
        public string FileNameOpfl { get; set; }
        public string SewingFile { get; set; }
        public string BenchmarkTime { get; set; }
        public string LaborType { get; set; }
        public string ComponentId { get; set; }
        public string OpGroupName { get; set; }
        public decimal MaxOpSerial { get; set; }
        public string NewPrevNo { get; set; }
        public string LanguageId { get; set; }
        public string OpNameLan { get; set; }
        public int OpTimeMax { get; set; }
        public string ModuleName { get; set; }
        public decimal TackTime { get; set; }
        public decimal TotalMachines { get; set; }
        public string FactoryName { get; set; }
        public decimal TotalManCount { get; set; }
        public string ActionProcess { get; set; }
        public string ActionCode { get; set; }
        public string OrgFileName { get; set; }
        public string VideoOpLink { get; set; }
        public string ImageLink { get; set; }
        public decimal StitchCount { get; set; }
        public decimal SeatNo { get; set; }
        public decimal TableId { get; set; }
        public decimal LineSerial { get; set; }
        public string McId { get; set; }
        public DateTime McPairDate { get; set; }
        public int AssemblyMdl { get; set; } //MOD) SON - 07 Septemper 2019 - change AssemblyDl to AssemblyMdl
        public int FinalAssembly { get; set; }
        public string IotType { get; set; }
        public string NoneStd { get; set; }
        public int Achieve { get; set; }
        public string MxPackage { get; set; }
        //START ADD) SON (2019.08.29) - 30 August 2019 - add properties for painting detail
        public string PaintingType { get; set; }
        public string MaterialType { get; set; }
        public decimal DryingTime { get; set; }
        public decimal Temperature { get; set; }
        public decimal CoolingTime { get; set; }
        public string PickUp { get; set; }
        public string Dispose { get; set; }
        public string MaterialTypeName { get; set; }
        public string PaintingTypeName { get; set; }
        //END ADD) SON (2019.08.29) - 05 Septemper 2019
        public decimal Efficiency { get; set; } //ADD - SON) 29/Oct/2020

        public decimal XPos { get; set; }
        public decimal YPos { get; set; }

        public string Codes { get; set; }
        public string IconNames { get; set; }
        public string MainProcessArr { get; set; }
        public string IconName { get; set; }

        #endregion

        #region Constructors

        public Opdt() { }

        public Opdt(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo,
            string opRevNo, int opSerial, string opNum, string opGroup, string opName, string factory, string machineType,
            string toolId, int opTime, int maxTime, string benchmarkTime, int machineCount, string remarks, string jobType,
            float manCount, string moduleId, decimal opPrice, decimal offerOpPrice, string outSourced, string hotSpot,
            string x, string y, decimal page, string displayColor) :
            base(styleCode, styleSize, styleColorSerial, revNo)
        {
            Edition = edition;
            OpRevNo = opRevNo;
            OpSerial = opSerial;
            OpNum = opNum;
            OpGroup = opGroup;
            OpName = opName;
            Factory = factory;
            MachineType = machineType;
            ToolId = toolId;
            OpTime = opTime;
            MaxTime = maxTime;
            BenchmarkTime = benchmarkTime;
            MachineCount = machineCount;
            Remarks = remarks;
            JobType = jobType;
            ManCount = manCount;
            ModuleId = moduleId;
            OpPrice = opPrice;
            OfferOpPrice = offerOpPrice;
            OutSourced = outSourced;
            HotSpot = hotSpot;
            X = x;
            Y = y;
            Page = page;
            DisplayColor = displayColor;
        }

        public Opdt(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo,
            string opRevNo, int opSerial, string videoFile)
        {
            Edition = edition;
            OpRevNo = opRevNo;
            OpSerial = opSerial;
            VideoFile = videoFile;
        }

        #endregion
    }
}
