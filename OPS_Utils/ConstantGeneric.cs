using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace OPS_Utils
{
    public enum EnumDataSource
    {
        [Description("PkMes")]
        PkMes,
        [Description("OdpConnStr")]
        OdpConnStr
    }

    public class ConstantGeneric
    {
        public static readonly string ConnectionStr = ConfigurationManager.ConnectionStrings["OdpConnStr"].ConnectionString;
        public static readonly string ConnectionStrMes = ConfigurationManager.ConnectionStrings["PkMes"].ConnectionString;
        //public static readonly string ConnectionStrLive = ConfigurationManager.ConnectionStrings["OdpConnStrLive"].ConnectionString;
        public static readonly string ConnectionStrMesMySql = ConfigurationManager.ConnectionStrings["MySqlMesConnection"].ConnectionString;
        public static readonly string ConnectionStrDgsMySql = ConfigurationManager.ConnectionStrings["MySqlDgsConnection"].ConnectionString;

        //Server No
        public static readonly string ServerNo = ConfigurationManager.AppSettings["ServerNo"] != null ? ConfigurationManager.AppSettings["ServerNo"] : "";

        public const string EditionPdm = "P";
        public const string EditionOps = "O";
        public const string EditionAom = "A";
        public const string EditionMes = "M";
        public const string GroupOp = "OPGroup";
        public const string MachineType = "MachineType";
        public const string Factory = "Factory";
        public const string OpType = "OPType";
        public const string OpTool = "OPTool";
        public static readonly string OpsTempFolder = ConfigurationManager.AppSettings["TemporaryFolder"] != null ? ConfigurationManager.AppSettings["TemporaryFolder"] : "";
        public static readonly string EmpImageFolder = ConfigurationManager.AppSettings["EmpImageFolder"] != null ? ConfigurationManager.AppSettings["EmpImageFolder"] : "";
        public static readonly string ProcessImageDirectory = ConfigurationManager.AppSettings["ProcessImageDirectory"] != null ? ConfigurationManager.AppSettings["ProcessImageDirectory"] : "";
        public static readonly string OpsVideoProcessPath = ConfigurationManager.AppSettings["OpsVideoProcessPath"] != null ? ConfigurationManager.AppSettings["OpsVideoProcessPath"] : "";
        public static readonly string OperationFilePath = ConfigurationManager.AppSettings["OperationFilePath"] != null ? ConfigurationManager.AppSettings["OperationFilePath"] : "";
        public static readonly string OpsStyleImagePath = ConfigurationManager.AppSettings["OpsStyleImagePath"] != null ? ConfigurationManager.AppSettings["OpsStyleImagePath"] : "";
        public static readonly string MachinePathImg = ConfigurationManager.AppSettings["OperationToolsImageDirectory"] != null ? ConfigurationManager.AppSettings["OperationToolsImageDirectory"] : "";
        public static readonly string VideoServerLink = ConfigurationManager.AppSettings["VideoServerLink"] != null ? ConfigurationManager.AppSettings["VideoServerLink"] : "";
        public static readonly string LineBalacingTemplateFolder = ConfigurationManager.AppSettings["LineBalacingTemplateFolder"] != null ? ConfigurationManager.AppSettings["LineBalacingTemplateFolder"] : "";
        public static readonly string ExportingFolder = ConfigurationManager.AppSettings["ExportingFolder"] != null ? ConfigurationManager.AppSettings["ExportingFolder"] : "";
        public static readonly string PkFileFolder = ConfigurationManager.AppSettings["pkfilefolder"] != null ? ConfigurationManager.AppSettings["pkfilefolder"] : "";
        
        //Process Icons
        public static readonly string ProcessIcon = ConfigurationManager.AppSettings["ProcessIcon"] != null ? ConfigurationManager.AppSettings["ProcessIcon"] : "";
        public static readonly string ProcessIconHost = ConfigurationManager.AppSettings["ProcessIconHost"] != null ? ConfigurationManager.AppSettings["ProcessIconHost"] : "";
        public static readonly string LayoutIconPath = ConfigurationManager.AppSettings["LayoutIconPath"] != null ? ConfigurationManager.AppSettings["LayoutIconPath"] : "";
        
        //Pattern image link
        public static readonly string PatternImageLink = ConfigurationManager.AppSettings["PatternImageLink"] != null ? ConfigurationManager.AppSettings["PatternImageLink"] : "";

        //PKPDM folder
        public static readonly string PKMESFolder = ConfigurationManager.AppSettings["PKMESFolder"] != null ? ConfigurationManager.AppSettings["PKMESFolder"] : "";
        public const string MaterialFolder = "Material";

        //Process
        public static readonly string PorcessImageHostDirectory = ConfigurationManager.AppSettings["PorcessImageHostDirectory"] != null ? ConfigurationManager.AppSettings["PorcessImageHostDirectory"] : "";
        public static readonly string ImageHttpLink = ConfigurationManager.AppSettings["PorcessImageHostDirectory"] != null ? ConfigurationManager.AppSettings["PorcessImageHostDirectory"] : "";

        //FTP for operation video server 
        public static readonly string FtpOpVideoUser = ConfigurationManager.AppSettings["FtpOpVideoUser"] != null ? ConfigurationManager.AppSettings["FtpOpVideoUser"] : "";
        public static readonly string FtpOpVideoPassword = ConfigurationManager.AppSettings["FtpOpVideoPassword"] != null ? ConfigurationManager.AppSettings["FtpOpVideoPassword"] : "";
        public static readonly string FtpOpVideoDirectory = ConfigurationManager.AppSettings["FtpOpVideoDirectory"] != null ? ConfigurationManager.AppSettings["FtpOpVideoDirectory"] : "";
        public static readonly string VideoProcessHttpLink = ConfigurationManager.AppSettings["VideoProcessHttpLink"] != null ? ConfigurationManager.AppSettings["VideoProcessHttpLink"] : "";

        //Email information
        public static readonly string EmailAddress = ConfigurationManager.AppSettings["EmailAddress"] != null ? ConfigurationManager.AppSettings["EmailAddress"] : "";
        public static readonly string EmailPassword = ConfigurationManager.AppSettings["EmailPassword"] != null ? ConfigurationManager.AppSettings["EmailPassword"] : "";
        public static readonly string SmtpClient = ConfigurationManager.AppSettings["SmtpClient"] != null ? ConfigurationManager.AppSettings["SmtpClient"] : "";
        public static readonly string Port = ConfigurationManager.AppSettings["Port"] != null ? ConfigurationManager.AppSettings["Port"] : "";
        public static readonly int OraTimeout = int.Parse(ConfigurationManager.AppSettings["OraTimeout"] != null ? ConfigurationManager.AppSettings["OraTimeout"] : "30");

        public const string Success = "success";
        public const string Fail = "fail";
        public const string ImageType = "Image";
        public const string VideoType = "Video";
        public const string MachineFile = "Machine";
        public const string JigFile = "Jig";
        public const string StyleFile = "StyleFile";
        public const string True = "True";
        public const string False = "False";
        public const string Yes = "Yes";
        public const string No = "No";
        public const string RoleTrue = "1";
        public const string RoleFalse = "0";
        public const string FactoryId = "5";
        public const string NoAuthority = "noauthority";
        public const string FinishGoods = "PG90"; //SON ADD) 18 Jan 2019


        public const string AlertPermission = "You do not have permission to do this action.";
        public const string AlertEditoinMes = "You do not have permission to do anything on edition MES.";
        public const string AlertOpsConfirmed = "This Ops have comfirmed!";

        public const string TableSdOpmt = "T_SD_OPMT";
        public const string TableSdOpdt = "T_SD_OPDT";

        public const string TableOpOpmt = "T_OP_OPMT";
        public const string TableOpOpdt = "T_OP_OPDT";

        public const string TableMtOpmt = "T_MT_OPMT";
        public const string TableMtOpdt = "T_MT_OPDT";

        public const string TableMxOpmt = "PKMES.T_MX_OPMT";
        public const string TableMxOpdt = "PKMES.T_MX_OPDT";


        public const string TableOptlErp = "T_OP_OPTL";
        public const string TableOptlMes = "PKMES.T_OP_OPTL";

        public const string TableProtErp = "T_SD_PROT";
        public const string TableProtMes = "PKMES.T_SD_PROT";

        public const string TableOpntErp = "T_OP_OPNT";
        public const string TableOpntMes = "PKMES.T_OP_OPNT";

        public const string OpsSystemId = "OPS";
        public const string MesSystemId = "MES";
        public const string MesMenu = "LRG";
        public const string OpManagementMenuId = "OPM";
        public const string FactoryMenu = "FOM";
        public const string OpStyleMenuId = "STM";
        public const string OpBalanceMenuId = "TBL";
        public const string ModuleMenuId = "MDL";
        public const string LayoutMenuId = "LAY";
        public const string NoMenuId = "NON";
        public const string ProcessTemplateMenuId = "PTL";

        //MES menu
        public const string MesMenuId = "MMA";
        public const string MesFlsMenuId = "FLS";
        public const string MesPplMenuId = "PPL"; //Menu Production Plan
        public const string MesWTSMenuId = "WTS"; //Menu Working time sheet
        public const string MesMachineDBId = "MDB"; //Machine Dashboard
        public const string MesPkgReadinessId = "PKR"; //Package readiness

        //Screen ID
        public const string ScreenLayout = "LAY";
        public const string ScreenRegistry = "RES";
        public const string ScreenLinking = "LIN";
        public const string ScreenToolMaster = "TMT";
        public const string ScreenProcessTemplate = "PTL";

        //link
        public const string OpLinkMenuId = "OPM";
        public const string ManageToolId = "OTM";
        public const string OpSmSgMenuId = "SMS";

        //Ftp information
        public const string FtpAppTypePkMain = "PKPMAIN";
        public const string FtpAppTypePlmHost = "PLMHOST";
        public const string FtpAppTypeOpsHost = "OPSHOST";
        public const string FtpStyleFolder = "style";
        public const string FtpStyleImageFolder = "Images";

        public const string ConfirmCheck = "Y";

        //Screen Id
        public const string OpsScreen = "OPS";

        //Event Id
        public const string EventAdd = "A";
        public const string EventDelete = "D";
        public const string EventEdit = "E";
        public const string EventGetData = "G";
        public const string EventConfirm = "C";

        //Action CRUD
        public const string ActionCreate = "C";
        public const string ActionRead = "R";
        public const string ActionUpdate = "U";
        public const string ActionDelete = "D";

        //Language 
        public const string VietNam = "VN";
        public const string English = "GB";
        public const string Korea = "KR";
        public const string Indonesia = "ID";

        //ACTIVE
        public const string Active = "Y";
        public const string NoneActive = "N";
        //
        public const string IniTialpattern = "000";

        //Array type of machine file.       
        public static readonly string[] ArrMachineFileType = { "dxf", "vdt", "ptg", "dat", "sew" };
        public static readonly string[] ArrJigType = { "dxf", "jpg", "png" };

        //Main level final assembly
        public const string MainLevelFinalAssembly = "FGS";
        public const string MainLevelSubAssembly = "SUB";
        public const string LevelNo01 = "00001";
        public const string LevelNo = "01";
        public const string LevelNo2 = "02";

        //Source file
        public const string SourceUpload = "U";
        public const string SourcePlm = "P";
        public const string SourceVideoPk = "V";

        //mcmt
        public const string SubCodePkVideo = "060";
        public const string SubCodeOtherFile = "058";
        public const string SubCodeSewqFile = "061";
        public const string SubCodeSunStarFile = "062";

        //Style color
        public const string NeutralColorSerial = "000";
        public const string NeutralColorWays = "NEUTRAL COLOR";

        //Default MES package
        public const string MxPackageDefault = "M_XXX-XXXX_X_XXXXXXXXXXXXXXXX_XX";

        public static string CurrentMxPackage; // To storage current mxpackage at server side
        public const string ROPDT = "ROPDT"; // Get max operation plan revision
        public const string COPDT = "COPDT"; // Create operation detail
        public const string UOPDT = "UOPDT"; // Update operation detail
        public const string ROPNTS = "ROPNT"; // Get list of operation name details
        public const string ROPMT = "ROPMT"; // Get operation master
        public const string RMAXOPREV = "RMAXOPREV"; // Get max operation plan revision
    }
    public enum Machine
    {
        Tool = 0,
        Machine = 1
    }

    public enum ApiCorporationCode
    {
        PKS1 = 100001,
        PKS2 = 100002,
        PKS3 = 100003,
        JSC = 100004,
        PKBT = 100005,
        PKLA = 100006,
        PKMT = 100007,
        PKIG = 100008,
        PKIS = 100009,
        PKEA = 100011,
        PKMA = 100012
    }

    public class ReportAction
    {
        public const int Success = 1;
        public const int Duplicate = -1;
        public const int RoleFail = -2;
        public const int Error = -10;
        public const int TimeOut = -20;
    }

    public class ResultContent
    {
        public string Result { get; set; }
        public object Content { get; set; }
    }

    public class FactoryDgs
    {
        public string FactoryId { get; set; }
        public string FactoryName { get; set; }

        public FactoryDgs(string facId, string facName)
        {
            FactoryId = facId;
            FactoryName = facName;
        }

        public static IEnumerable<FactoryDgs> ListFactoryDgs() {
            var listFacDgs = new List<FactoryDgs>
            {
                new FactoryDgs("P2A1", "FAC 2A"),
                new FactoryDgs("P2B1", "FAC 2B"),
                new FactoryDgs("P2C1", "FAC 2C"),
                new FactoryDgs("P2D1", "FAC 2D")
            };

            return listFacDgs;
        }

    }
}
