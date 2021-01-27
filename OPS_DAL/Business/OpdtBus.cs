using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class OpdtBus
    {
        #region Properties

        private readonly MySqlDBManager _mySqlDBManager = new MySqlDBManager();
        private readonly OracleDI _oracleDI = new OracleDI();
        private readonly string _mySqlConn = ConstantGeneric.ConnectionStrMesMySql;

        #endregion

        #region General

        /// <summary>
        /// Sorting list of proccess by opnum
        /// </summary>
        /// <param name="listOpDetail"></param>
        /// <param name="sortByModule"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opdt> SortListProcess(List<Opdt> listOpDetail, bool sortByModule)
        {
            var newListOpdt = new List<Opdt>();

            if (sortByModule)
            {
                //Create temporary module id list
                List<string> listModuleId = new List<string>();

                foreach (var opdt in listOpDetail)
                {
                    //Find module in the list
                    string opdtModuleId = opdt.ModuleId ?? string.Empty;
                    var existModule = listModuleId.Any(x => x.Equals(opdtModuleId));
                    if (!existModule)
                    {
                        //Add module id to temprary list
                        listModuleId.Add(opdtModuleId);
                        //find list process with the same module id
                        var listProcesByModule = listOpDetail.Where(x => x.ModuleId == (opdtModuleId == string.Empty ? null : opdtModuleId));
                        //Add list process to the new list
                        newListOpdt.AddRange(listProcesByModule);
                    }
                }
            }
            else
            {
                //Create temporary module id list
                List<string> listOpGroup = new List<string>();

                foreach (var opdt in listOpDetail)
                {
                    //Find module in the list
                    string opdtOpGroupId = opdt.OpGroup ?? string.Empty;
                    var existOpGroup = listOpGroup.Any(x => x.Equals(opdtOpGroupId));
                    if (!existOpGroup)
                    {
                        //Add module id to temprary list
                        listOpGroup.Add(opdtOpGroupId);
                        //find list process with the same module id
                        var listProcesByOpGroup = listOpDetail.Where(x => x.OpGroup == (opdtOpGroupId == string.Empty ? null : opdtOpGroupId));
                        //Add list process to the new list
                        newListOpdt.AddRange(listProcesByOpGroup);
                    }
                }
            }

            return newListOpdt;
        }

        public static List<Opdt> GetByLanguage(string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo, string edition, string languageId, int sourceDb)
        {
            switch (sourceDb)
            {
                case 1:
                    return GetByLanguage(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);
                case 2:
                    return GetOpDetailByLanguage(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);
                default:
                    return null;
            }
        }

        #endregion

        #region Oracle database

        #region Methods

        /// <summary>
        /// Get Efficiency
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Opdt GetEfficiency(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            string tblName = CommonMethod.GetTableNameDetailByEdition(edition);
            string strSql = $@"select ROUND(MIN(OPT.OPTIME) /MAX(OPT.OPTIME),2) EFFICIENCY, OPT.STYLECODE, OPT.STYLESIZE, OPT.STYLECOLORSERIAL, OPT.REVNO, OPT.OPREVNO 
                                FROM {tblName} OPT 
                                where stylecode = :p_stylecode and stylesize = :p_stylesize and stylecolorserial = :p_stylecolorserial and revno = :p_revno and oprevno = :p_oprevno
                                GROUP BY OPT.STYLECODE, OPT.STYLESIZE, OPT.STYLECOLORSERIAL, OPT.REVNO, OPT.OPREVNO";
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("p_stylecode", styleCode),
                new OracleParameter("p_stylesize", styleSize),
                new OracleParameter("p_stylecolorserial", styleColorSerial),
                new OracleParameter("p_revno", revNo),
                new OracleParameter("p_oprevno", opRevNo)

            };
            var opdt = OracleDbManager.GetObjectsByType<Opdt>(strSql, CommandType.Text, oracleParams.ToArray());

            return opdt.FirstOrDefault();
        }

        /// <summary>
        /// Gets the opdt by primary keys.
        /// </summary>
        /// <param name="opdt">The operation detail.</param>
        /// <returns>Operation detail</returns>
        /// Author: Nguyen Xuan Hoang
        public static Opdt GetOpdtByPks(Opdt opdt)
        {
            var oracleParams = new OpsOracleParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                opdt.RevNo, opdt.OpRevNo, opdt.OpSerial);
            oracleParams.AddCursor();

            var opdtResult = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETBYPRIMARYKEYS_OPDT",
                CommandType.StoredProcedure, oracleParams.ToArray()).FirstOrDefault();
            return opdtResult;
        }

        public async Task<Opdt> GetOpdt(Opdt opdt)
        {
            var oracleParams = new OpsOracleParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                opdt.RevNo, opdt.OpRevNo, opdt.OpSerial);
            oracleParams.AddCursor();

            var opdtResult = await _oracleDI.GetAllAsync<Opdt>("sp_ops_getwithlang_opdt110",
                CommandType.StoredProcedure, oracleParams.ToArray(), ConstantGeneric.ConnectionStr);

            return opdtResult.FirstOrDefault();
        }

        /// <summary>
        /// Gets the op detail by language.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opdt> GetOpDetailByLanguage(string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo, string edition, string languageId)
        {
            var subUrl = FtpInfoBus.GetSubUrl();
            var oracleParams = new OpsOracleParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower()),
                new OracleParameter("P_URL", subUrl)
            };
            oracleParams.AddCursor();

            var lstOpDetail = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETOPDTBYLANG_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray());

            return lstOpDetail;
        }

        /// <summary>
        /// Get list of process name with standard name.
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Opdt> GetOpDetailWithStandardName(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            var oracleParams = new OpsOracleParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower())
            };
            oracleParams.AddCursor();

            var lstOpDetail = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETOPDTWITHSTANDARDNAME", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstOpDetail;
        }

        /// <summary>
        /// Gets the op detail by language.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static List<Opdt> GetOpDetailByLanguage2(string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo, string edition, string languageId)
        {
            var oracleParams = new OpsOracleParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_LANGUAGEID", languageId?.ToLower())
            };
            oracleParams.AddCursor();

            var lstOpDetail = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETOPDTBYLANG2_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray());

            return lstOpDetail;
        }

        /// <summary>
        ///     Gets the detail by op.
        /// </summary>
        /// <param name="l"></param>
        /// <param name="lprots"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        /// Author: VitHV
        /// <exception cref="System.NotImplementedException"></exception>
        public static List<Opdt> GetDetailConvert(List<Opdt> l, List<Prot> lprots, string title)
        {
            foreach (var item in lprots)
                foreach (var child in l)
                    if (item.StyleCode == child.StyleCode
                        && item.StyleSize == child.StyleSize
                        && item.StyleColorSerial == child.StyleColorSerial
                        && item.RevNo == child.RevNo
                        && item.OpRevNo == child.OpRevNo
                        && item.OpSerial == child.OpSerial)
                        child.NewPrevNo = title;
            return l;
        }

        /// <summary>
        ///     Author: Son Nguyen Cao
        ///     Date: 28 July 2017
        ///     Get max operation plan serial
        /// </summary>
        /// <param name="edtion"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="styleRevision"></param>
        /// <param name="opRevision"></param>
        /// <returns>Max Operation Plan serial</returns>
        public static int GetMaxOpSerial(string edtion, string styleCode, string styleSize, string styleColorSerial, string styleRevision, string opRevision)
        {
            var sb = new StringBuilder();
            var tableName = CommonMethod.GetTableNameDetailByEdition(edtion);
            var strSql = $" SELECT  MAX(OPD.OPSERIAL) MaxOpSerial " +
                         $" FROM {tableName} OPD " +
                         " WHERE " +
                         "         OPD.STYLECODE = :STYLECODE AND OPD.STYLESIZE = :STYLESIZE " +
                         "         AND OPD.STYLECOLORSERIAL = :STYLECOLORSERIAL AND OPD.REVNO = :REVNO " +
                         "         AND OPD.OPREVNO = :OPREVNO ";

            var prams = new OracleParameter[5];
            prams[0] = new OracleParameter("STYLECODE", styleCode);
            prams[1] = new OracleParameter("STYLESIZE", styleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", styleColorSerial);
            prams[3] = new OracleParameter("REVNO", styleRevision);
            prams[4] = new OracleParameter("OPREVNO", opRevision);

            var objOpdt = OracleDbManager.GetObjects<Opdt>(strSql, prams).FirstOrDefault();

            if (objOpdt != null) return decimal.ToInt32(objOpdt.MaxOpSerial) + 1;
            return 1;
        }

        /// <summary>
        /// Get tack time
        /// </summary>
        /// <param name="objOpmt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static decimal GetTackTime(Opmt objOpmt)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", objOpmt.StyleCode),
                new OracleParameter("P_STYLESIZE", objOpmt.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", objOpmt.StyleColorSerial),
                new OracleParameter("P_REVNO", objOpmt.RevNo),
                new OracleParameter("P_OPREVNO", objOpmt.OpRevNo),
                new OracleParameter("P_EDITION", objOpmt.Edition?.Substring(0,1)),
                cursor
            };

            var objOpdt = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETTACKTIME_OPDT", CommandType.StoredProcedure, oracleParams.ToArray()).FirstOrDefault();
            return objOpdt != null ? objOpdt.TackTime : 0;
        }

        /// <summary>
        /// Get takt time of processes which use standard process name. 
        /// </summary>
        /// <param name="objOpmt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static decimal GetTackTimeWithStandardName(Opmt objOpmt)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", objOpmt.StyleCode),
                new OracleParameter("P_STYLESIZE", objOpmt.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", objOpmt.StyleColorSerial),
                new OracleParameter("P_REVNO", objOpmt.RevNo),
                new OracleParameter("P_OPREVNO", objOpmt.OpRevNo),
                new OracleParameter("P_EDITION", objOpmt.Edition?.Substring(0,1)),
                cursor
            };

            var objOpdt = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETTACKTIMESTDNAME_OPDT", CommandType.StoredProcedure, oracleParams.ToArray()).FirstOrDefault();
            return objOpdt != null ? objOpdt.TackTime : 0;
        }

        public static bool CreateOpdt(Opdt opdt, OracleConnection oraConn, OracleTransaction trans)
        {
            //Check the lenght of opname.
            if (opdt.OpName?.Length > 200) opdt.OpName = opdt.OpName.Substring(0, 199);

            var tblName = CommonMethod.GetTableNameDetailByEdition(opdt.Edition);
            //ADD) SON - 22/Jun/2019 - add iot type
            var strSql1 = $"INSERT INTO {tblName} " +
                                @" (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, OPREVNO, OPSERIAL, OPNUM, OPGROUP, OPNAME, FACTORY 
                                    , MACHINETYPE, OPDESC, OPTIME, OPPRICE, OFFEROPPRICE, MACHINECOUNT, REMARKS , MAXTIME, MANCOUNT, FILENAME
                                    , NEXTOP, OUTSOURCED, X, Y, IMAGENAME, DISPLAYCOLOR, PAGE , GROUPCOLOR, VIDEOFILE, JOBTYPE
                                    , MODULEID, HOTSPOT, TOOLID, BENCHMARKTIME, ACTIONCODE, IOTTYPE, seatno, tableid, lineserial)  
                            VALUES (:STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :OPREVNO, :OPSERIAL, :OPNUM, :OPGROUP, :OPNAME, :FACTORY
                                    , :MACHINETYPE, :OPDESC, :OPTIME, :OPPRICE, :OFFEROPPRICE, :MACHINECOUNT, :REMARKS , :MAXTIME, :MANCOUNT, :FILENAME
                                    , :NEXTOP, :OUTSOURCED, :X, :Y, :IMAGENAME, :DISPLAYCOLOR, :PAGE, :GROUPCOLOR, :VIDEOFILE, :JOBTYPE
                                    , :MODULEID, :HOTSPOT, :TOOLID, :BENCHMARKTIME, :ACTIONCODE, :IOTTYPE, :seatno, :tableid, :lineserial) ";

            var oracleParams = new OpsOracleParams(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opdt.OpSerial),
                new OracleParameter("OPNUM", opdt.OpNum),
                new OracleParameter("OPGROUP", opdt.OpGroup),
                new OracleParameter("OPNAME", opdt.OpName),
                new OracleParameter("FACTORY", opdt.Factory),
                new OracleParameter("MACHINETYPE", opdt.MachineType),
                new OracleParameter("OPDESC", opdt.OpDesc),
                new OracleParameter("OPTIME", opdt.OpTime),
                new OracleParameter("OPPRICE", opdt.OpPrice),
                new OracleParameter("OFFEROPPRICE", opdt.OfferOpPrice),
                new OracleParameter("MACHINECOUNT", opdt.MachineCount),
                new OracleParameter("REMARKS", opdt.Remarks),
                new OracleParameter("MAXTIME", opdt.MaxTime),
                new OracleParameter("MANCOUNT", opdt.ManCount),
                new OracleParameter("FILENAME", opdt.FileName),
                new OracleParameter("NEXTOP", opdt.NextOp),
                new OracleParameter("OUTSOURCED", opdt.OutSourced),
                new OracleParameter("X", opdt.X),
                new OracleParameter("Y", opdt.Y),
                new OracleParameter("IMAGENAME", opdt.ImageName),
                new OracleParameter("DISPLAYCOLOR", opdt.DisplayColor),
                new OracleParameter("PAGE", opdt.Page),
                new OracleParameter("GROUPCOLOR", opdt.GroupColor),
                new OracleParameter("VIDEOFILE", opdt.VideoFile),
                new OracleParameter("JOBTYPE", opdt.JobType),
                new OracleParameter("MODULEID", opdt.ModuleId),
                new OracleParameter("HOTSPOT", opdt.HotSpot),
                new OracleParameter("TOOLID", opdt.ToolId ),
                new OracleParameter("BENCHMARKTIME", opdt.BenchmarkTime),
                new OracleParameter("ACTIONCODE", opdt.ActionCode),
                new OracleParameter("IOTTYPE", opdt.IotType),
                new OracleParameter("SeatNo", opdt.SeatNo),
                new OracleParameter("TableId", opdt.TableId),
                new OracleParameter("LineSerial", opdt.LineSerial)
            };

            var result = OracleDbManager.ExecuteQuery(strSql1, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        /// <summary>
        ///     Adds the op detail.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddOpDetail(Opdt opDetail, OracleConnection oraConn, OracleTransaction trans)
        {
            //Check the lenght of opname.
            if (opDetail.OpName?.Length > 200) opDetail.OpName = opDetail.OpName.Substring(0, 199);

            var tableName1 = CommonMethod.GetTableNameDetailByEdition(opDetail.Edition);

            var strSql1 = @"INSERT INTO " + tableName1 +
                                @"(STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, OPREVNO, OPSERIAL, OPNUM, OPGROUP, OPNAME, FACTORY 
                                    , MACHINETYPE, OPDESC, OPTIME, OPPRICE, OFFEROPPRICE, MACHINECOUNT, REMARKS , MAXTIME, MANCOUNT, FILENAME
                                    , NEXTOP, OUTSOURCED, X, Y, IMAGENAME, DISPLAYCOLOR, PAGE , GROUPCOLOR, VIDEOFILE, JOBTYPE
                                    , MODULEID, HOTSPOT, TOOLID, OPSSTATE, OPTIMEBALANCING, BENCHMARKTIME, ACTIONCODE, STITCHCOUNT, IOTTYPE
                                    , PAINTINGTYPE, MATERIALTYPE, DRYINGTIME, TEMPERATURE, COOLINGTIME)  
                            VALUES (:STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :OPREVNO, :OPSERIAL, :OPNUM, :OPGROUP, :OPNAME, :FACTORY
                                    , :MACHINETYPE, :OPDESC, :OPTIME, :OPPRICE, :OFFEROPPRICE, :MACHINECOUNT, :REMARKS , :MAXTIME, :MANCOUNT, :FILENAME
                                    , :NEXTOP, :OUTSOURCED, :X, :Y, :IMAGENAME, :DISPLAYCOLOR, :PAGE, :GROUPCOLOR, :VIDEOFILE, :JOBTYPE
                                    , :MODULEID, :HOTSPOT, :TOOLID, :OPSSTATE, :OPTIMEBALANCING, :BENCHMARKTIME, :ACTIONCODE, :STITCHCOUNT, :IOTTYPE
                                    , :PAINTINGTYPE, :MATERIALTYPE, :DRYINGTIME, :TEMPERATURE, :COOLINGTIME) ";

            var oracleParams = new OpsOracleParams(opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("OPNUM", opDetail.OpNum),
                new OracleParameter("OPGROUP", opDetail.OpGroup),
                new OracleParameter("OPNAME", opDetail.OpName),
                new OracleParameter("FACTORY", opDetail.Factory),
                new OracleParameter("MACHINETYPE", opDetail.MachineType),
                new OracleParameter("OPDESC", opDetail.OpDesc),
                new OracleParameter("OPTIME", opDetail.OpTime),
                new OracleParameter("OPPRICE", opDetail.OpPrice),
                new OracleParameter("OFFEROPPRICE", opDetail.OfferOpPrice),
                new OracleParameter("MACHINECOUNT", opDetail.MachineCount),
                new OracleParameter("REMARKS", opDetail.Remarks),
                new OracleParameter("MAXTIME", opDetail.MaxTime),
                new OracleParameter("MANCOUNT", opDetail.ManCount),
                new OracleParameter("FILENAME", opDetail.FileName),
                new OracleParameter("NEXTOP", opDetail.NextOp),
                new OracleParameter("OUTSOURCED", opDetail.OutSourced),
                new OracleParameter("X", opDetail.X),
                new OracleParameter("Y", opDetail.Y),
                new OracleParameter("IMAGENAME", opDetail.ImageName),
                new OracleParameter("DISPLAYCOLOR", opDetail.DisplayColor),
                new OracleParameter("PAGE", opDetail.Page),
                new OracleParameter("GROUPCOLOR", opDetail.GroupColor),
                new OracleParameter("VIDEOFILE", opDetail.VideoFile),
                new OracleParameter("JOBTYPE", opDetail.JobType),
                new OracleParameter("MODULEID", opDetail.ModuleId),
                new OracleParameter("HOTSPOT", opDetail.HotSpot),
                new OracleParameter("TOOLID", opDetail.ToolId ),
                new OracleParameter("OPSSTATE", opDetail.OpsState),
                new OracleParameter("OPTIMEBALANCING", opDetail.OpTimeBalancing),
                new OracleParameter("BENCHMARKTIME", opDetail.BenchmarkTime),
                new OracleParameter("ACTIONCODE", opDetail.ActionCode), // vithv
                new OracleParameter("STITCHCOUNT", opDetail.StitchCount), // HA ADD
                new OracleParameter("IOTTYPE", opDetail.IotType),
                //START ADD) SON (2019.08.29) - 30 August 2019 - adding paiting detail
                new OracleParameter("PAINTINGTYPE", opDetail.PaintingType),
                new OracleParameter("MATERIALTYPE", opDetail.MaterialType),
                new OracleParameter("DRYINGTIME", opDetail.DryingTime),
                new OracleParameter("TEMPERATURE", opDetail.Temperature),
                new OracleParameter("COOLINGTIME", opDetail.CoolingTime),                
                //END ADD) SON (2019.08.29) - 30 August 2019
            };

            var result = OracleDbManager.ExecuteQuery(strSql1, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;

        }

        /// <summary>.
        /// Adds the op detail.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddOpDetail_New(Opdt opDetail, OracleConnection oraConn, OracleTransaction trans)
        {
            //Check the lenght of opname.
            if (opDetail.OpName?.Length > 200) opDetail.OpName = opDetail.OpName.Substring(0, 199);

            var tableName1 = CommonMethod.GetTableNameDetailByEdition(opDetail.Edition);

            var strSql1 = @"INSERT INTO " + tableName1 +
                                @"(STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, OPREVNO, OPSERIAL, OPNUM, OPGROUP, OPNAME, FACTORY 
                                    , MACHINETYPE, OPDESC, OPTIME, OPPRICE, OFFEROPPRICE, MACHINECOUNT, REMARKS , MAXTIME, MANCOUNT, FILENAME
                                    , NEXTOP, OUTSOURCED, X, Y, IMAGENAME, DISPLAYCOLOR, PAGE , GROUPCOLOR, VIDEOFILE, JOBTYPE
                                    , MODULEID, HOTSPOT, TOOLID, OPSSTATE, OPTIMEBALANCING, BENCHMARKTIME, ACTIONCODE, STITCHCOUNT, IOTTYPE
                                    , PAINTINGTYPE, MATERIALTYPE, DRYINGTIME, TEMPERATURE, COOLINGTIME, PICKUP, DISPOSE, ICONNAME)  
                            VALUES (:STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :OPREVNO, :OPSERIAL, :OPNUM, :OPGROUP, :OPNAME, :FACTORY
                                    , :MACHINETYPE, :OPDESC, :OPTIME, :OPPRICE, :OFFEROPPRICE, :MACHINECOUNT, :REMARKS , :MAXTIME, :MANCOUNT, :FILENAME
                                    , :NEXTOP, :OUTSOURCED, :X, :Y, :IMAGENAME, :DISPLAYCOLOR, :PAGE, :GROUPCOLOR, :VIDEOFILE, :JOBTYPE
                                    , :MODULEID, :HOTSPOT, :TOOLID, :OPSSTATE, :OPTIMEBALANCING, :BENCHMARKTIME, :ACTIONCODE, :STITCHCOUNT, :IOTTYPE
                                    , :PAINTINGTYPE, :MATERIALTYPE, :DRYINGTIME, :TEMPERATURE, :COOLINGTIME, :PICKUP, :DISPOSE, :ICONNAME) ";

            var oracleParams = new OpsOracleParams(opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("OPNUM", opDetail.OpNum),
                new OracleParameter("OPGROUP", opDetail.OpGroup),
                new OracleParameter("OPNAME", opDetail.OpName),
                new OracleParameter("FACTORY", opDetail.Factory),
                new OracleParameter("MACHINETYPE", opDetail.MachineType),
                new OracleParameter("OPDESC", opDetail.OpDesc),
                new OracleParameter("OPTIME", opDetail.OpTime),
                new OracleParameter("OPPRICE", opDetail.OpPrice),
                new OracleParameter("OFFEROPPRICE", opDetail.OfferOpPrice),
                new OracleParameter("MACHINECOUNT", opDetail.MachineCount),
                new OracleParameter("REMARKS", opDetail.Remarks),
                new OracleParameter("MAXTIME", opDetail.MaxTime),
                new OracleParameter("MANCOUNT", opDetail.ManCount),
                new OracleParameter("FILENAME", opDetail.FileName),
                new OracleParameter("NEXTOP", opDetail.NextOp),
                new OracleParameter("OUTSOURCED", opDetail.OutSourced),
                new OracleParameter("X", opDetail.X),
                new OracleParameter("Y", opDetail.Y),
                new OracleParameter("IMAGENAME", opDetail.ImageName),
                new OracleParameter("DISPLAYCOLOR", opDetail.DisplayColor),
                new OracleParameter("PAGE", opDetail.Page),
                new OracleParameter("GROUPCOLOR", opDetail.GroupColor),
                new OracleParameter("VIDEOFILE", opDetail.VideoFile),
                new OracleParameter("JOBTYPE", opDetail.JobType),
                new OracleParameter("MODULEID", opDetail.ModuleId),
                new OracleParameter("HOTSPOT", opDetail.HotSpot),
                new OracleParameter("TOOLID", opDetail.ToolId ),
                new OracleParameter("OPSSTATE", opDetail.OpsState),
                new OracleParameter("OPTIMEBALANCING", opDetail.OpTimeBalancing),
                new OracleParameter("BENCHMARKTIME", opDetail.BenchmarkTime),
                new OracleParameter("ACTIONCODE", opDetail.ActionCode), // vithv
                new OracleParameter("STITCHCOUNT", opDetail.StitchCount), // HA ADD
                new OracleParameter("IOTTYPE", opDetail.IotType),
                //START ADD) SON (2019.08.29) - 30 August 2019 - adding paiting detail
                new OracleParameter("PAINTINGTYPE", opDetail.PaintingType),
                new OracleParameter("MATERIALTYPE", opDetail.MaterialType),
                new OracleParameter("DRYINGTIME", opDetail.DryingTime),
                new OracleParameter("TEMPERATURE", opDetail.Temperature),
                new OracleParameter("COOLINGTIME", opDetail.CoolingTime),                
                //END ADD) SON (2019.08.29) - 30 August 2019
                new OracleParameter("PICKUP", opDetail.PickUp),
                new OracleParameter("DISPOSE", opDetail.Dispose),
                new OracleParameter("ICONNAME", opDetail.IconName) //ADD - SON) 25/Jan/2021
            };

            var result = OracleDbManager.ExecuteQuery(strSql1, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;

        }

        /// <summary>
        /// Updates the op detail.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateObjectOpDetail(Opdt opDetail, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_EDITION", opDetail.Edition),
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("P_OPNUM", opDetail.OpNum),
                new OracleParameter("P_OPGROUP", opDetail.OpGroup),
                new OracleParameter("P_OPNAME", opDetail.OpName),
                new OracleParameter("P_MACHINETYPE", opDetail.MachineType),
                new OracleParameter("P_OPTIME", opDetail.OpTime),
                new OracleParameter("P_OPPRICE", opDetail.OpPrice),
                new OracleParameter("P_OFFEROPPRICE", opDetail.OfferOpPrice),
                new OracleParameter("P_MACHINECOUNT", opDetail.MachineCount),
                new OracleParameter("P_REMARKS", opDetail.Remarks),
                new OracleParameter("P_MAXTIME", opDetail.MaxTime),
                new OracleParameter("P_MANCOUNT", opDetail.ManCount),
                new OracleParameter("P_IMAGENAME", opDetail.ImageName),
                new OracleParameter("P_VIDEOFILE", opDetail.VideoFile),
                new OracleParameter("P_JOBTYPE", opDetail.JobType),
                new OracleParameter("P_BENCHMARKTIME", opDetail.BenchmarkTime),
                new OracleParameter("P_MODULEID", opDetail.ModuleId),
                new OracleParameter("P_HOTSPOT", opDetail.HotSpot),
                new OracleParameter("P_TOOLID", opDetail.ToolId),
                new OracleParameter("P_OPSSTATE", opDetail.OpsState),
                new OracleParameter("P_OPTIMEBALANCING", opDetail.OpTimeBalancing),
                new OracleParameter("P_ACTIONCODE", opDetail.ActionCode), // VITHV
                new OracleParameter("P_FACTORY", opDetail.Factory), // VITHV
                new OracleParameter("P_STITCHCOUNT", opDetail.StitchCount), // HA ADD
                new OracleParameter("P_IOTTYPE", opDetail.IotType),
                 //START ADD) SON (2019.08.29) - 30 August 2019 - adding paiting detail
                new OracleParameter("PAINTINGTYPE", opDetail.PaintingType),
                new OracleParameter("MATERIALTYPE", opDetail.MaterialType),
                new OracleParameter("DRYINGTIME", opDetail.DryingTime),
                new OracleParameter("TEMPERATURE", opDetail.Temperature),
                new OracleParameter("COOLINGTIME", opDetail.CoolingTime),                
                //END ADD) SON (2019.08.29) - 30 August 2019
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEOPDETAIL_OPDT", oracleParams.ToArray(),
                CommandType.StoredProcedure, trans, oraConn);

            return resUpdate != null && int.Parse(resUpdate.ToString()) != 0;
        }

        public static bool UpdateObjectOpDetail_New(Opdt opDetail, OracleConnection oraConn, OracleTransaction trans)
        {
            try
            {
                string opdtTb = CommonMethod.GetTableNameDetailByEdition(opDetail.Edition);

                string strSql = $@"UPDATE {opdtTb}
                                SET opnum = :p_opnum, opgroup = :p_opgroup, opname = :p_opname, machinetype = :p_machinetype, optime = :p_optime
			                    , opprice = :p_opprice, offeropprice = :p_offeropprice, machinecount = :p_machinecount, remarks = :p_remarks, maxtime = :p_maxtime
			                    , mancount = :p_mancount, imagename = :p_imagename, videofile = :p_videofile, jobtype = :p_jobtype, benchmarktime = :p_benchmarktime
			                    , moduleid = :p_moduleid, hotspot = :p_hotspot, toolid = :p_toolid, opsstate = :p_opsstate, optimebalancing = :p_optimebalancing
			                    , actioncode = :p_actioncode, factory = :p_factory, stitchcount = :p_stitchcount, iottype = :p_iottype, paintingtype = :p_paintingtype
			                    , materialtype = :p_materiatype, dryingtime = :p_dryingtime, temperature = :p_temperature, coolingtime = :p_coolingtime, pickup = :p_pickup
			                    , dispose = :p_dispose, iconname = :p_iconname			
                            WHERE
                                stylecode = :p_stylecode
                                AND   stylesize = :p_stylesize
                                AND   stylecolorserial = :p_stylecolorserial
                                AND   revno = :p_revno
                                AND   oprevno = :p_oprevno
                                AND   opserial = :p_opserial";
                var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_OPNUM", opDetail.OpNum),
                new OracleParameter("P_OPGROUP", opDetail.OpGroup),
                new OracleParameter("P_OPNAME", opDetail.OpName),
                new OracleParameter("P_MACHINETYPE", opDetail.MachineType),
                new OracleParameter("P_OPTIME", opDetail.OpTime),
                new OracleParameter("P_OPPRICE", opDetail.OpPrice),
                new OracleParameter("P_OFFEROPPRICE", opDetail.OfferOpPrice),
                new OracleParameter("P_MACHINECOUNT", opDetail.MachineCount),
                new OracleParameter("P_REMARKS", opDetail.Remarks),
                new OracleParameter("P_MAXTIME", opDetail.MaxTime),
                new OracleParameter("P_MANCOUNT", opDetail.ManCount),
                new OracleParameter("P_IMAGENAME", opDetail.ImageName),
                new OracleParameter("P_VIDEOFILE", opDetail.VideoFile),
                new OracleParameter("P_JOBTYPE", opDetail.JobType),
                new OracleParameter("P_BENCHMARKTIME", opDetail.BenchmarkTime),
                new OracleParameter("P_MODULEID", opDetail.ModuleId),
                new OracleParameter("P_HOTSPOT", opDetail.HotSpot),
                new OracleParameter("P_TOOLID", opDetail.ToolId),
                new OracleParameter("P_OPSSTATE", opDetail.OpsState),
                new OracleParameter("P_OPTIMEBALANCING", opDetail.OpTimeBalancing),
                new OracleParameter("P_ACTIONCODE", opDetail.ActionCode),
                new OracleParameter("P_FACTORY", opDetail.Factory),
                new OracleParameter("P_STITCHCOUNT", opDetail.StitchCount),
                new OracleParameter("P_IOTTYPE", opDetail.IotType),
                new OracleParameter("PAINTINGTYPE", opDetail.PaintingType),
                new OracleParameter("MATERIALTYPE", opDetail.MaterialType),
                new OracleParameter("DRYINGTIME", opDetail.DryingTime),
                new OracleParameter("TEMPERATURE", opDetail.Temperature),
                new OracleParameter("COOLINGTIME", opDetail.CoolingTime),
                new OracleParameter("PICKUP", opDetail.PickUp),
                new OracleParameter("DISPOSE", opDetail.Dispose),
                new OracleParameter("p_iconname", opDetail.IconName), //ADD - SON) 25/Jan/2021
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial)
            };

                var resUpdate = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

                return resUpdate != null && int.Parse(resUpdate.ToString()) != 0;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Updates the op detail.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="lstMachine"></param>
        /// <param name="lstTool"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpDetail(Opdt opDetail, List<Optl> lstMachine, List<Optl> lstTool, List<Opnt> lstOpnt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (UpdateObjectOpDetail(opDetail, connection, trans))
                    {
                        //Update optime of ops master.
                        if (opDetail.OpTimeMax < opDetail.TackTime)
                        {
                            OpmtBus.UpdateOpTimeMaster(opDetail, connection, trans);
                        }

                        //Deleting and inserting operation name detail.
                        if (OpntBus.DeleteOpNameDetail(opDetail, connection, trans))
                        {
                            if (lstOpnt != null)
                            {
                                foreach (var opnt in lstOpnt)
                                {
                                    OpntBus.InsertOpNameDetail(opnt, connection, trans);
                                }
                            }
                        }

                        //Delete toolinking.
                        if (OptlBus.DeleteOpsToolLinking(opDetail, trans, connection))
                        {
                            //Insert toolinking.
                            if (lstMachine != null)
                            {
                                foreach (var tl in lstMachine)
                                {
                                    OptlBus.AddToolLinking(tl, connection, trans);
                                }
                            }

                            //Add list tool
                            if (lstTool != null)
                            {
                                foreach (var opTool in lstTool)
                                    OptlBus.AddToolLinking(opTool, connection, trans);
                            }
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }
                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool UpdateOpDetail_New(Opdt opDetail, List<Optl> lstMachine, List<Optl> lstTool, List<Opnt> lstOpnt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (UpdateObjectOpDetail_New(opDetail, connection, trans))
                    {
                        //Update optime of ops master.
                        if (opDetail.OpTimeMax < opDetail.TackTime)
                        {
                            OpmtBus.UpdateOpTimeMaster(opDetail, connection, trans);
                        }

                        //Deleting and inserting operation name detail.
                        if (OpntBus.DeleteOpNameDetail(opDetail, connection, trans))
                        {
                            if (lstOpnt != null)
                            {
                                foreach (var opnt in lstOpnt)
                                {
                                    OpntBus.InsertOpNameDetail_New(opnt, connection, trans);
                                }
                            }
                        }

                        //Delete toolinking.
                        if (OptlBus.DeleteOpsToolLinking(opDetail, trans, connection))
                        {
                            //Insert toolinking.
                            if (lstMachine != null)
                            {
                                foreach (var tl in lstMachine)
                                {
                                    OptlBus.AddToolLinking_New(tl, connection, trans);
                                }
                            }

                            //Add list tool
                            if (lstTool != null)
                            {
                                foreach (var opTool in lstTool)
                                    OptlBus.AddToolLinking_New(opTool, connection, trans);
                            }
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }
                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///Author: Son Nguyen Cao 
        /// </summary>
        /// <param name="opDetail"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static bool UpdateOpNameByOpDetail(Opdt opDetail, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("P_OPNAME", opDetail.OpNameLan)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEOPNAME_OPDT", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resUpdate != null;
        }

        /// <summary>
        /// Updates the seat.
        /// </summary>
        /// <param name="tableSpace">The table space.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool OracleUpdateSeat(TableSpaceEntity tableSpace)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("p_affectedrows", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("p_tableid", tableSpace.TableId),
                new OracleParameter("p_lineserial", tableSpace.LineSerial),
                new OracleParameter("p_seattotal", tableSpace.SeatTotal)
            };
            var resUpdate = OracleDbManager.ExecuteQuery("SP_MES_UPDATESEAT_OPDT", oracleParams.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);

            return resUpdate != null && int.Parse(resUpdate.ToString()) != 0;
        }

        /// <summary>
        /// Update next operation (next op)
        /// </summary>
        /// <param name="opdt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool UpdateOpNextOp(Opdt opdt, OracleConnection oraConn, OracleTransaction trans)
        {

            var oracleParams = new OpsOracleParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opdt.OpSerial),
                new OracleParameter("P_OPNEXTOP", opdt.NextOp)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATENEXTOP_OPDT", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resUpdate != null;
        }

        /// <summary>
        /// Update list next op.
        /// </summary>
        /// <param name="lstOpdt"></param>
        /// <returns></returns>
        public static bool UpdateListOpNextOp(List<Opdt> lstOpdt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var opdt in lstOpdt)
                    {
                        if (!UpdateOpNextOp(opdt, connection, trans))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Update ops mater language
        /// </summary>
        /// <param name="opMaster"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpLanguage(Opmt opMaster, string languageId, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opMaster.Edition, opMaster.StyleCode, opMaster.StyleSize, opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo)
            {
                new OracleParameter("P_OPLANGUAGE", languageId)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEOPLANGUAGE_OPMT", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resUpdate != null;
        }

        /// <summary>
        /// Author: Son Nguyen Cao
        /// </summary>
        /// <param name="lstOpDetail"></param>
        /// <returns></returns>
        public static bool UpdateOpName(Opmt opMaster, List<Opdt> lstOpDetail, string languageId)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (UpdateOpLanguage(opMaster, languageId, connection, trans))
                    {
                        if (lstOpDetail != null)
                        {
                            foreach (var opDetail in lstOpDetail)
                            {
                                if (!UpdateOpNameByOpDetail(opDetail, connection, trans))
                                {
                                    trans.Rollback();
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///     Adds the new proccess.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="lstMachine">The op tool linking.</param>
        /// <param name="lstTool"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddNewProccess(Opdt opDetail, List<Optl> lstMachine, List<Optl> lstTool, List<Opnt> lstOpnt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add operation detail
                    if (AddOpDetail(opDetail, connection, trans))
                    {
                        if (lstOpnt != null)
                        {
                            //Add list operation name detail
                            foreach (var opnt in lstOpnt)
                            {
                                OpntBus.InsertOpNameDetail(opnt, connection, trans);
                            }
                        }

                        if (lstMachine != null)
                        {
                            //Add list machine
                            foreach (var opMachine in lstMachine)
                            {
                                OptlBus.AddToolLinking(opMachine, connection, trans);
                            }
                        }

                        if (lstTool != null)
                        {
                            //Add list tool
                            foreach (var opTool in lstTool)
                            {
                                OptlBus.AddToolLinking(opTool, connection, trans);
                            }
                        }

                        trans.Commit();
                        return true;
                    }

                    trans.Rollback();
                    return false;

                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>.
        /// Adds the new proccess.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="lstMachine">The op tool linking.</param>
        /// <param name="lstTool"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddNewProccess_New(Opdt opDetail, List<Optl> lstMachine, List<Optl> lstTool, List<Opnt> lstOpnt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add operation detail
                    if (AddOpDetail_New(opDetail, connection, trans))
                    {
                        if (lstOpnt != null)
                        {
                            //Add list operation name detail
                            foreach (var opnt in lstOpnt)
                            {
                                OpntBus.InsertOpNameDetail_New(opnt, connection, trans);
                            }
                        }

                        if (lstMachine != null)
                        {
                            //Add list machine
                            foreach (var opMachine in lstMachine)
                            {
                                OptlBus.AddToolLinking_New(opMachine, connection, trans);
                            }
                        }

                        if (lstTool != null)
                        {
                            //Add list tool
                            foreach (var opTool in lstTool)
                            {
                                OptlBus.AddToolLinking_New(opTool, connection, trans);
                            }
                        }

                        trans.Commit();
                        return true;
                    }

                    trans.Rollback();
                    return false;

                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///     Updates the name of the file.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateFileName(Opdt opDetail)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_EDITION", opDetail.Edition),
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("P_FILENAME", opDetail.FileName)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEFILENAME_OPDT", oracleParams.ToArray(),
                CommandType.StoredProcedure);

            return resUpdate != null && int.Parse(resUpdate.ToString()) != 0;
        }

        /// <summary>
        ///     Updates the name of the video.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateVideoName(Opdt opDetail)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_EDITION", opDetail.Edition),
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("P_VIDEONAME", opDetail.VideoFile)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEVIDEONAME_OPDT", oracleParams.ToArray(),
                CommandType.StoredProcedure);

            return resUpdate != null && int.Parse(resUpdate.ToString()) != 0;
        }

        /// <summary>
        ///     Deletes the ops detail.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="trans">The trans.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteOpsDetail(Opdt opDetail, OracleTransaction trans, OracleConnection oraConn)
        {

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("P_EDITION", opDetail.Edition.Substring(0, 1))
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELETEOPSDETAIL_OPDT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// Delete process list by operation master.
        /// </summary>
        /// <param name="opmt"></param>
        /// <param name="trans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        public static bool DeleteOpDetailByOpmt(Opmt opmt, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            oracleParams.Insert(0, new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) { Direction = ParameterDirection.Output });

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELOPDETAILBYOPMT_OPDT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null; // && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        ///     Deletes the ops detail and tool linking.
        /// </summary>
        /// <param name="lstOpDetail">The list op detail.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteOpsDetailAndToolLinking(List<Opdt> lstOpDetail)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    var frsOpdt = lstOpDetail[0];
                    var lstOpdt = GetOpDetailByLanguage(frsOpdt.StyleCode, frsOpdt.StyleSize, frsOpdt.StyleColorSerial, frsOpdt.RevNo, frsOpdt.OpRevNo, frsOpdt.Edition, "");

                    foreach (var opDetail in lstOpDetail)
                    {

                        if (!(OptlBus.DeleteOpsToolLinking(opDetail, trans, connection)
                             && OpntBus.DeleteOpNameDetail(opDetail, connection, trans)
                             && ProtBus.DeletePatternBomByOpdt(opDetail, trans, connection)))
                        {
                            trans.Rollback();
                            return false;
                        }

                        //Delete ops detail
                        if (DeleteOpsDetail(opDetail, trans, connection))
                        {
                            //Update next process.
                            var lstNextOp = lstOpdt.Where(s => s.NextOp == opDetail.OpSerial.ToString());
                            foreach (var nextOp in lstNextOp)
                            {
                                nextOp.NextOp = "";
                                UpdateOpNextOp(nextOp, connection, trans);
                            }
                            continue;
                        }

                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete MES operation plan detail
        /// </summary>
        /// <param name="lstOpDetail"></param>
        /// <returns></returns>
        public static bool DeleteMESOperationPlanDetail(List<Opdt> lstOpDetail)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStrMes))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    var frsOpdt = lstOpDetail[0];
                    var lstOpdt = GetOpDetailByLanguage(frsOpdt.StyleCode, frsOpdt.StyleSize, frsOpdt.StyleColorSerial, frsOpdt.RevNo, frsOpdt.OpRevNo, frsOpdt.Edition, "");

                    foreach (var opDetail in lstOpDetail)
                    {

                        if (!(OptlBus.DeleteOpsToolLinking(opDetail, trans, connection)
                             && OpntBus.DeleteOpNameDetail(opDetail, connection, trans)
                             && ProtBus.DeletePatternBomByOpdt(opDetail, trans, connection)))
                        {
                            trans.Rollback();
                            return false;
                        }

                        //Delete ops detail
                        if (DeleteOpsDetail(opDetail, trans, connection))
                        {
                            //Update next process.
                            var lstNextOp = lstOpdt.Where(s => s.NextOp == opDetail.OpSerial.ToString());
                            foreach (var nextOp in lstNextOp)
                            {
                                nextOp.NextOp = "";
                                UpdateOpNextOp(nextOp, connection, trans);
                            }
                            continue;
                        }

                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///     Gets the op detail by code.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opdt> GetOpDetailByCode(Opdt opDetail)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("P_EDITION", opDetail.Edition),
                new OracleParameter("P_MASTERCODE1", ConstantGeneric.GroupOp),
                new OracleParameter("P_MASTERCODE2", ConstantGeneric.MachineType),
                cursor
            };
            var lstStyleMaster = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETOPSDETAILBYCODE_OPDT",
                CommandType.StoredProcedure, oracleParams.ToArray());
            return lstStyleMaster;
        }

        /// <summary>
        /// Confirms the ops detail.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="trans">The trans.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool ConfirmOpsDetail(Opdt opDetail, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial),
                new OracleParameter("P_EDITION", opDetail.Edition.Substring(0, 1)),
                new OracleParameter("P_CONFIRMID", opDetail.ConfirmedId)
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_CONFIRMOPSDETAIL", oracleParams.ToArray(),
                CommandType.StoredProcedure, trans, oraConn);

            return resDel != null && int.Parse(resDel.ToString()) != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opDetail"></param>
        /// <returns></returns>
        /// Author: Son Cao Nguyen
        private static bool UpdateOpTimeBalancing(Opdt opDetail, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial.ToString()),
                new OracleParameter("P_OPTIMEBALANCING", opDetail.OpTimeBalancing.ToString())
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEOPTIMEBAL_OPDT", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resUpdate != null;
        }

        /// <summary>
        /// Update optime balancing
        /// </summary>
        /// <param name="lstOpDetail"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateOpTimeBalancing(List<Opdt> lstOpDetail)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    foreach (var opdt in lstOpDetail)
                    {
                        if (!UpdateOpTimeBalancing(opdt, connection, trans))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Update T_SD_OPDT//T_OP_OPDT//T_MT_OPDT for column MACHINETYPE.
        /// </summary>
        /// <param name="op">The Optl.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool UpdateOpDetail(Optl op, bool isTool)
        {
            string table = CommonMethod.GetTableNameDetailByEdition(op.Edition);
            var sb = new StringBuilder();
            string colUpdate = " SET TOOLID = '" + op.ItemCode + "' ";
            if (!isTool)
            {
                colUpdate = " SET MACHINETYPE = '" + op.ItemCode + "' ";
            }
            sb.AppendLine(@" UPDATE " + table + colUpdate);
            sb.AppendLine(@" WHERE STYLECODE =:STYLECODE 
                             AND STYLESIZE =:STYLESIZE 
                             AND STYLECOLORSERIAL =:STYLECOLORSERIAL
                             AND REVNO =:REVNO 
                             AND OPREVNO =:OPREVNO 
                             AND OPSERIAL =:OPSERIAL");
            var prams = new OpsParams(op.StyleCode, op.StyleSize, op.StyleColorSerial, op.RevNo, op.OpRevNo, op.OpSerial);
            prams.ReplacePbyEmpty();
            var result = OracleDbManager.ExecuteQuery(sb.ToString(), prams.ToArray(), CommandType.Text);
            return result != null;
        }

        /// <summary>
        /// Get list opdts by module code
        /// </summary>
        /// <param name="mdCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opdt> GetListOpdtsByModuleCode(string mdCode, string styleCode)
        {
            var strSql = @" SELECT MODULEID FROM T_SD_OPDT where MODULEID = :P_MODULEID AND STYLECODE = :P_STYLECODE
                            UNION
                            SELECT MODULEID FROM T_OP_OPDT WHERE MODULEID = :P_MODULEID AND STYLECODE = :P_STYLECODE
                            UNION
                            SELECT MODULEID FROM T_mt_OPDT where MODULEID = :P_MODULEID AND STYLECODE = :P_STYLECODE
                            UNION
                            SELECT MODULEID FROM PKMES.T_MX_OPDT where MODULEID = :P_MODULEID AND STYLECODE = :P_STYLECODE ";

            var oracleParams = new List<OracleParameter> {
                new OracleParameter("P_MODULEID", mdCode),
                new OracleParameter("P_STYLECODE", styleCode)
            };

            return OracleDbManager.GetObjects<Opdt>(strSql, oracleParams.ToArray());

        }

        #endregion

        #region Ops Layout
        /// <summary>
        /// Adds the new process.
        /// </summary>
        /// <param name="opdt">The operation detail.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object AddNewProcess(Opdt opdt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();

                var lastOpSerial = GetMaxOpSerial(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo);
                opdt.OpSerial = lastOpSerial;

                var trans = connection.BeginTransaction();
                var result = AddOpDetail(opdt, connection, trans);

                trans.Commit();

                return result ? opdt : null;
            }
        }

        #region For new UI

        public async Task<List<Opdt>> GetOpdts(Opmt opmt)
        {
            //var subUrl = FtpInfoBus.GetSubUrl();
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            oracleParams.AddCursor();

            var opdts = await _oracleDI.GetAllAsync<Opdt>("sp_ops_get_opdt_v11", CommandType.StoredProcedure,
                oracleParams.ToArray(), ConstantGeneric.ConnectionStr);

            return opdts;
        }

        public async Task<object> OracleGetByOpGroup(Opmt opsMaster, int page)
        {
            // Get list of operation details
            var opdts = await GetOpdts(opsMaster);
            var totalPage = opdts.Max(x => x.Page);

            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() : opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => grp.First()).ToList();

            // Get all of group from master table
            var groupCodes = McmtBus.GetMasterCode(ConstantGeneric.GroupOp);

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.SubCode equals opdt.OpGroup
                         select new
                         {
                             SubCode = opdt.OpGroup,
                             g.CodeName,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0],
                             opdt.Page
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = groupCodes, totalPage };

            return result;
        }

        public async Task<object> OracleGetByModuleType(Opmt opsMaster, int page)
        {
            // Get list of operation details
            var opdts = await GetOpdts(opsMaster);
            var totalPage = opdts.Max(x => x.Page);

            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() : opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.ModuleId).Select(grp => grp.First()).ToList();

            // Get all of group from master table
            var groupCodes = SamtBus.GetModulesByCode(opsMaster.StyleCode);

            // Group for adding new 
            var groupsToAdd = from g in groupCodes
                              select new
                              {
                                  SubCode = g.ModuleId,
                                  CodeName = g.ModuleName
                              };

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.ModuleId equals opdt.ModuleId
                         select new
                         {
                             SubCode = opdt.ModuleId,
                             CodeName = g.ModuleName,
                             PartComment = g.PartComment,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0]
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd, totalPage };

            return result;
        }

        public async Task<object> OracleGetByMachineType(Opmt opsMaster, int page)
        {
            // Get list of operation details
            var opdts = await GetOpdts(opsMaster);
            var totalPage = opdts.Max(x => x.Page);

            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() : opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes
            var edges = from o in opdts
                        where o.NextOp != null
                        select new
                        {
                            source = o.OpSerial.ToString(),
                            target = o.NextOp
                        };

            // Get list of machines
            var mcmtMachines = McmtBus.GetMasterCode(ConstantGeneric.MachineType);
            var otmtMachines = OtmtBus.GetOpMachines();
            var tempMachines = from mc in mcmtMachines
                               select new Otmt
                               {
                                   ItemCode = mc.SubCode,
                                   ItemName = mc.CodeName
                               };

            // Machines were stored in 2 tables so we need to aggregate them
            var totalMachines = (from mc in otmtMachines
                                 select new
                                 {
                                     SubCode = mc.ItemCode,
                                     CodeName = mc.ItemName
                                 }).Union(from mc in tempMachines
                                          select new
                                          {
                                              SubCode = mc.ItemCode,
                                              CodeName = mc.ItemName
                                          }).ToArray();

            var allMachineGroups = opdts.GroupBy(x => x.MachineType).Select(grp => grp.First());

            var groupMachines = (from g in allMachineGroups
                                 join m in totalMachines
                                 on g.MachineType equals m.SubCode
                                 select new
                                 {
                                     SubCode = g.MachineType,
                                     m.CodeName,
                                     X = g.X?.Split('.')[0],
                                     Y = g.Y?.Split('.')[0]
                                 }).OrderBy(x => x.SubCode);

            var result = new { groups = groupMachines, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = totalMachines, totalPage };

            return result;
        }

        #endregion

        /// <summary>
        /// Gets the type of the opdt by machine.
        /// </summary>
        /// <param name="opsMaster">The ops master.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="page">selected page</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object GetOpdtByModuleType(Opmt opsMaster, string languageId, int page)
        {
            // Get list of operation details
            var opdts = GetOpDetailByLanguage(opsMaster.StyleCode, opsMaster.StyleSize, opsMaster.StyleColorSerial,
                opsMaster.RevNo, opsMaster.OpRevNo, opsMaster.Edition, languageId);
            var totalPage = opdts.Max(x => x.Page);

            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() : opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.ModuleId).Select(grp => grp.First()).ToList();

            // Get all of group from master table
            var groupCodes = SamtBus.GetModulesByCode(opsMaster.StyleCode);

            // Group for adding new 
            var groupsToAdd = from g in groupCodes
                              select new
                              {
                                  SubCode = g.ModuleId,
                                  CodeName = g.ModuleName
                              };

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.ModuleId equals opdt.ModuleId
                         select new
                         {
                             SubCode = opdt.ModuleId,
                             CodeName = g.ModuleName,
                             PartComment = g.PartComment,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0]
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd, totalPage };

            return result;
        }

        /// <summary>
        /// Gets the type of the opdt by machine.
        /// </summary>
        /// <param name="opsMaster">The ops master.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="page"></param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object GetOpdtByMachineType(Opmt opsMaster, string languageId, int page)
        {
            // Get list of operation details
            var opdts = GetOpDetailByLanguage(opsMaster.StyleCode, opsMaster.StyleSize, opsMaster.StyleColorSerial,
                opsMaster.RevNo, opsMaster.OpRevNo, opsMaster.Edition, languageId);
            var totalPage = opdts.Max(x => x.Page);

            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() : opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes
            var edges = from o in opdts
                        where o.NextOp != null
                        select new
                        {
                            source = o.OpSerial.ToString(),
                            target = o.NextOp
                        };

            // Get list of machines
            var mcmtMachines = McmtBus.GetMasterCode(ConstantGeneric.MachineType);
            var otmtMachines = OtmtBus.GetOpMachines();
            var tempMachines = from mc in mcmtMachines
                               select new Otmt
                               {
                                   ItemCode = mc.SubCode,
                                   ItemName = mc.CodeName
                               };

            // Machines were stored in 2 tables so we need to aggregate them
            var totalMachines = (from mc in otmtMachines
                                 select new
                                 {
                                     SubCode = mc.ItemCode,
                                     CodeName = mc.ItemName
                                 }).Union(from mc in tempMachines
                                          select new
                                          {
                                              SubCode = mc.ItemCode,
                                              CodeName = mc.ItemName
                                          }).ToArray();

            var allMachineGroups = opdts.GroupBy(x => x.MachineType).Select(grp => grp.First());

            var groupMachines = (from g in allMachineGroups
                                 join m in totalMachines
                                 on g.MachineType equals m.SubCode
                                 select new
                                 {
                                     SubCode = g.MachineType,
                                     m.CodeName,
                                     X = g.X?.Split('.')[0],
                                     Y = g.Y?.Split('.')[0]
                                 }).OrderBy(x => x.SubCode);

            var result = new { groups = groupMachines, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = totalMachines, totalPage };

            return result;
        }

        /// <summary>
        /// Gets the opdt by op group.
        /// </summary>
        /// <param name="opsMaster">The ops master.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="page">Number page of layout.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object GetOpdtByOpGroup(Opmt opsMaster, string languageId, int page)
        {
            // Get list of operation details
            var opdts = GetOpDetailByLanguage(opsMaster.StyleCode, opsMaster.StyleSize, opsMaster.StyleColorSerial,
                opsMaster.RevNo, opsMaster.OpRevNo, opsMaster.Edition, languageId);
            var totalPage = opdts.Max(x => x.Page);

            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() : opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => grp.First()).ToList();

            // Get all of group from master table
            var groupCodes = McmtBus.GetMasterCode(ConstantGeneric.GroupOp);

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.SubCode equals opdt.OpGroup
                         select new
                         {
                             SubCode = opdt.OpGroup,
                             g.CodeName,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0],
                             opdt.Page
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = groupCodes, totalPage };

            return result;
        }

        /// <summary>
        /// Gets the edges (line flows).
        /// </summary>
        /// <param name="opdts">The list of opdts.</param>
        /// <returns>List of edges</returns>
        /// Author: Nguyen Xuan Hoang
        private static object GetEdges(List<Opdt> opdts)
        {
            // Get all connection of processes
            var tempEdges = from o in opdts
                            where o.NextOp != null
                            select o;
            var edges = new List<object>();

            // Do not get edges if NextOp is not in the list (because OpSerial is NextOp)
            foreach (var e in tempEdges)
            {
                var rs = false;
                foreach (var o in opdts)
                {
                    if (o.OpSerial.ToString() == e.NextOp)
                    {
                        rs = true;
                    }
                }

                if (rs)
                {
                    var edge = new
                    {
                        source = e.OpSerial.ToString(),
                        target = e.NextOp
                    };
                    edges.Add(edge);
                }
            }

            return edges;
        }

        /// <summary>
        /// Updates the list of operation detail.
        /// </summary>
        /// <param name="opdtList">The opdt list.</param>
        /// <param name="opmt">Operation master</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateOpdts(List<Opdt> opdtList, Opmt opmt)
        {
            using (var oracleConnection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                oracleConnection.Open();
                var oracleTransaction = oracleConnection.BeginTransaction();

                try
                {
                    // Save opmt firstly
                    var affectedRowPara = new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var opmtOracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize,
                        opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
                    opmtOracleParams.AddRange(new List<OracleParameter>
                    {
                        new OracleParameter("P_LANGUAGE", opmt.Language),
                        new OracleParameter("P_GROUPMODE", opmt.GroupMode),
                        new OracleParameter("P_FACTORY", opmt.Factory),
                        new OracleParameter("P_PROCESSWIDTH", opmt.ProcessWidth),
                        new OracleParameter("P_PROCESSHEIGHT", opmt.ProcessHeight),
                        new OracleParameter("P_LAYOUTFONTSIZE", opmt.LayoutFontSize),
                        new OracleParameter("P_CANVASHEIGHT", opmt.CanvasHeight),
                        new OracleParameter("P_REMARKS", opmt.Remarks)
                    });

                    opmtOracleParams.Insert(0, affectedRowPara);

                    OracleDbManager.ExecuteQuery("SP_OPS_UPDATE_OPMT", opmtOracleParams.ToArray(), CommandType.StoredProcedure, oracleTransaction, oracleConnection);

                    var oracleParams = new OpsOracleParams(opdtList.Select(x => x.Edition).ToArray(), opdtList.Select(x => x.StyleCode).ToArray(),
                        opdtList.Select(x => x.StyleSize).ToArray(), opdtList.Select(x => x.StyleColorSerial).ToArray(),
                        opdtList.Select(x => x.RevNo).ToArray(), opdtList.Select(x => x.OpRevNo).ToArray());

                    var oracleParas = new List<OracleParameter>
                    {
                        new OracleParameter("P_OPSERIAL", opdtList.Select(x=>x.OpSerial).ToArray()),
                        new OracleParameter("P_OPGROUP", opdtList.Select(x=>x.OpGroup).ToArray()),
                        new OracleParameter("P_OPNAME", opdtList.Select(x=>x.OpName).ToArray()),
                        new OracleParameter("P_MACHINETYPE", opdtList.Select(x=>x.MachineType).ToArray()),
                        new OracleParameter("P_MODULEID", opdtList.Select(x=>x.ModuleId).ToArray()),
                        new OracleParameter("P_NEXTOP", opdtList.Select(x=>x.NextOp).ToArray()),
                        new OracleParameter("P_X", opdtList.Select(x=>x.X).ToArray()),
                        new OracleParameter("P_Y", opdtList.Select(x=>x.Y).ToArray()),
                        new OracleParameter("P_DISPLAYCOLOR", opdtList.Select(x=>x.DisplayColor).ToArray()),
                        new OracleParameter("P_PAGE", opdtList.Select(x=>x.Page).ToArray())
                    };

                    oracleParams.AddRange(oracleParas);
                    oracleParams.Insert(0, affectedRowPara);

                    // Save OPDT then
                    var result = OracleDbManager.ExecuteQuery("SP_OPS_UPDATE_OPDT", oracleParams.ToArray(), CommandType.StoredProcedure,
                        opdtList.Count, oracleTransaction, oracleConnection);

                    oracleTransaction.Commit();

                    return result is Array affectedRows && affectedRows.Length > 0;
                }
                catch
                {
                    oracleTransaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the opdt and tool linking.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <returns>True(success) or False(fail)</returns>
        /// Author: Nguyen Xuan Hoang
        public static bool DeleteOpdtAndToolLinking(Opdt opdt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                var hasDelToolLinking = OptlBus.DeleteOpsToolLinking(opdt, trans, connection);
                //START ADD - Son Nguyen Cao
                var hasDelOpName = OpntBus.DeleteOpNameDetail(opdt, connection, trans);
                var hasDelPattern = ProtBus.DeletePatternBomByOpdt(opdt, trans, connection);
                //END ADD - Son Nguyen Cao
                if (hasDelToolLinking && hasDelOpName && hasDelPattern)
                {
                    //Delete ops detail
                    var hasDeleteOpdt = DeleteOpsDetail(opdt, trans, connection);
                    if (hasDeleteOpdt)
                    {
                        trans.Commit();
                        return true;
                    }

                    trans.Rollback();
                    return false;
                }

                trans.Rollback();
                return false;
            }
        }

        /// <summary>
        /// Clones the single process.
        /// </summary>
        /// <param name="opdt">List of operation detail.</param>
        /// <param name="opnts">List of operation name.</param>
        /// <param name="optls">List of operation tool linking</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool CloneSingleProcess(Opdt opdt, List<Opnt> opnts, List<Optl> optls)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        AddOpDetail(opdt, connection, trans);

                        //Add list of operation name details
                        foreach (var opnt in opnts)
                        {
                            opnt.OpSerial = opdt.OpSerial;
                            OpntBus.InsertOpNameDetail(opnt, connection, trans);
                        }

                        // Change Ops master key in list copy tool linking
                        foreach (var tool in optls)
                        {
                            tool.Edition = opdt.Edition;
                            tool.OpSerial = opdt.OpSerial;
                            OptlBus.AddToolLinking(tool, connection, trans);
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdatePage(List<Opdt> opdts)
        {
            var tb = CommonMethod.GetTableNameDetailByEdition(opdts[0].Edition);
            string prcPage = "", opSerials = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                prcPage += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].Page}' ";
                opSerials += i != opdts.Count - 1 ? $"'{opdts[i].OpSerial}'," : $"'{opdts[i].OpSerial}'";
            }

            var q = $"UPDATE {tb} SET " +
                    $"Page = (CASE opserial {prcPage} END) " +
                    $"WHERE OpSerial IN({opSerials}) " +
                    $"AND stylecode = '{opdts[0].StyleCode}' " +
                    $"AND stylesize = '{opdts[0].StyleSize}' " +
                    $"AND stylecolorserial = '{opdts[0].StyleColorSerial}' " +
                    $"AND revno = '{opdts[0].RevNo}' " +
                    $"AND oprevno = '{opdts[0].OpRevNo}'";

            var rs = await OracleDbManager.ExecQueryAsync(q, null, CommandType.Text, ConstantGeneric.ConnectionStr);
            return rs;
        }

        public async Task<bool> UpdateModule(List<Opdt> opdts)
        {
            var tb = CommonMethod.GetTableNameDetailByEdition(opdts[0].Edition);
            string moduleData = "", opSerials = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                moduleData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].ModuleId}' ";
                opSerials += i != opdts.Count - 1 ? $"'{opdts[i].OpSerial}'," : $"'{opdts[i].OpSerial}'";
            }

            var q = $"UPDATE {tb} SET " +
                    $"ModuleId = (CASE opserial {moduleData} END) " +
                    $"WHERE OpSerial IN({opSerials}) " +
                    $"AND stylecode = '{opdts[0].StyleCode}' " +
                    $"AND stylesize = '{opdts[0].StyleSize}' " +
                    $"AND stylecolorserial = '{opdts[0].StyleColorSerial}' " +
                    $"AND revno = '{opdts[0].RevNo}' " +
                    $"AND oprevno = '{opdts[0].OpRevNo}'";

            var rs = await OracleDbManager.ExecQueryAsync(q, null, CommandType.Text, ConstantGeneric.ConnectionStr);
            return rs;
        }

        public async Task<bool> UpdateVideoFile(VideoOpdt opdt)
        {
            var tb = CommonMethod.GetTableNameDetailByEdition(opdt.Edition);

            var q = $@"UPDATE {tb} SET
                            VideoFile = :P_VideoFile
                            WHERE 
                            Stylecode = :P_StyleCode
                            AND Stylesize = :P_StyleSize 
                            AND Stylecolorserial = :P_StyleColorSerial
                            AND RevNo = :P_RevNo
                            AND OpRevNo = :P_OpRevNo
                            AND OpSerial = :P_OpSerial";

            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_VideoFile", opdt.VideoFile),
                new OracleParameter("P_StyleCode", opdt.StyleCode),
                new OracleParameter("P_StyleSize", opdt.StyleSize),
                new OracleParameter("P_StyleColorSerial", opdt.StyleColorSerial),
                new OracleParameter("P_RevNo", opdt.RevNo),
                new OracleParameter("P_OpRevNo", opdt.OpRevNo),
                new OracleParameter("P_OpSerial", opdt.OpSerial)
            };

            var rs = await OracleDbManager.ExecQueryAsync(q, oracleParams.ToArray(), CommandType.Text, ConstantGeneric.ConnectionStr);

            return rs;
        }
        #endregion

        #region Clone Process
        /// <summary>
        /// Copy list of process and tool linking.
        /// </summary>
        /// <param name="lstOpdt"></param>
        /// <param name="lstOptl"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool CloneProcess(List<Opdt> lstOpdt, List<Opnt> lstOpnt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add copied processes.
                    foreach (var opdt in lstOpdt)
                    {
                        AddOpDetail(opdt, connection, trans);
                    }

                    //Add list operation name detail
                    foreach (var opnt in lstOpnt)
                    {
                        OpntBus.InsertOpNameDetail(opnt, connection, trans);
                    }

                    //Add list tool
                    //foreach (var optl in lstOptl)
                    //{
                    //    OptlBus.AddToolLinking(optl, connection, trans);
                    //}

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion

        #region Opdt Summaries
        /// <summary>
        /// Summarizes the processes.
        /// </summary>
        /// <param name="opmt">The opdt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object SummarizeProcesses(Opmt opmt)
        {
            var outCursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            var oracleParams = new OpsOracleParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo,
                opmt.OpRevNo) { outCursor };
            var cmdText = "";
            var edition = opmt.Edition;

            CommonMethod.MapEditionToTable("SP_OPS_SUMMARIZE", "OPDT", ref edition, ref cmdText);

            var buyerCode = opmt.StyleCode.Substring(0, 3);
            var buyers = McmtBus.GetMasterCode("Buyer").Where(x => x.SubCode == buyerCode);
            var styleSizes = McmtBus.GetMasterCode("StyleSize").Where(x => x.SubCode == opmt.StyleSize);
            var styleColors = StyleColorBus.GetStyleColorEntities(opmt.StyleCode, opmt.StyleColorSerial);
            var processSummary = OracleDbManager.GetObjects<ProcessSummary>(cmdText, CommandType.StoredProcedure,
                oracleParams.ToArray()).FirstOrDefault();
            var styleMaster = StmtBus.GetStyleMasterByStyleCode(opmt.StyleCode);
            //var styleImage = $"/assets/ops-fileupload/style/{buyerCode}/{opmt.StyleCode}/Images/{styleMaster.Picture}";
            var styleImage = StmtBus.CreateStyleImageLink(styleMaster.Picture, opmt.StyleCode); //Son Add
            var style = new
            {
                styleCode = opmt.StyleCode,
                styleMaster.BuyerStyleCode,
                buyerName = buyers.FirstOrDefault()?.CodeName,
                styleSize = styleSizes.FirstOrDefault()?.CodeName,
                styleColor = styleColors.StyleColorWays,
                revNo = opmt.RevNo,
                opRevNo = opmt.OpRevNo,
                edition,
                picture = styleImage
            };

            return new { style, processSummary };
        }

        /// <summary>
        /// Summarizes the opdt by machine.
        /// </summary>
        /// <param name="opmt">The operation details.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Opdt> SummarizeOpdtByMachine(Opmt opmt)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);

            oracleParams.AddCursor();
            var result = OracleDbManager.GetObjects<Opdt>("SP_OPS_SUMMARIZEBYMACHINE_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray());

            return result;
        }

        /// <summary>
        /// Summarizes the opdt by machine.
        /// </summary>
        /// <param name="opmt">The operation details.</param>
        /// <returns></returns>
        /// Author: VITHV
        public static List<Otmt> SummarizeOpdtByTools(Opmt opmt)
        {
            var oracleParams = new OpsOracleParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo,
                opmt.OpRevNo);
            oracleParams.AddCursor();

            return OracleDbManager.GetObjects<Otmt>("SP_OPS_SUMMARIZEBYTOOLS_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray());
        }

        /// <summary>
        /// Summarizes the opdt by all machine has been linked.
        /// </summary>
        /// <param name="opmt">The operation details.</param>
        /// <returns></returns>
        /// Author: VITHV
        public static List<Otmt> SummarizeOpdtAllMachine(Opmt opmt)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);
            oracleParams.AddCursor();

            return OracleDbManager.GetObjects<Otmt>("SP_OPS_SUMMARIZEMACHINES_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray());
        }

        /// <summary>
        /// Summarizes the opdt by machine.
        /// </summary>
        /// <param name="opmt">The operation details.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static List<Opdt> SummarizeOpdtWorker(Opmt opmt)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);

            oracleParams.AddCursor();

            return OracleDbManager.GetObjects<Opdt>("SP_OPS_SUMMARIZEBYWORKER_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray());
        }

        /// <summary>
        /// Summarizes the jig file.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <param name="language">The language.</param>
        /// <param name="sumJigMode">The sum jig mode.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object SummarizeJigFile(Opmt opmt, string language, string sumJigMode)
        {
            // Get list of operation details
            var opdts = GetOpDetailByLanguage(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo, opmt.Edition, language);

            // Get Opfl
            var opfls = OpflBus.GetOpflsByOp(opmt);

            // Get uploadcode (s_code) for opfl
            var mcmts = McmtBus.GetMasterCode(ConstantGeneric.StyleFile, ConstantGeneric.JigFile);

            // Get list of opfl by UploadCode in mcmt table
            var opfs = from o in opfls
                       where (from m in mcmts
                              select m.SubCode).Contains(o.UploadCode)
                       select o;

            object result;
            switch (sumJigMode)
            {
                case "ModuleName":
                    result = SummarizeJigFileByModule(opmt.StyleCode, opdts, opfs.ToList());
                    break;
                case "OpGroup":
                    result = SummarizeJigFileByOpGroup(opdts, opfs.ToList());
                    break;
                default:
                    result = SummarizeJigFileByDefault(opdts, opfs.ToList(), "");
                    break;
            }

            return result;
        }

        /// <summary>
        /// Summarizes the jig file by module.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="opdts">The opdts.</param>
        /// <param name="opfs">The opfs.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        private static object SummarizeJigFileByModule(string styleCode, List<Opdt> opdts, List<Opfl> opfs)
        {
            // Get all of module
            var groupCodes = SamtBus.GetModulesByCode(styleCode);

            var sumModule = (from o in opdts
                             join f in opfs on new
                             { o.StyleCode, o.StyleSize, o.StyleColorSerial, o.RevNo, o.OpRevNo, o.OpSerial, o.Edition }
                             equals
                             new { f.StyleCode, f.StyleSize, f.StyleColorSerial, f.RevNo, f.OpRevNo, f.OpSerial, f.Edition }
                             join m in groupCodes on o.ModuleId equals m.ModuleId
                             where !string.IsNullOrEmpty(f.OrgFileName)
                             select new
                             {
                                 OpSerial = o.OpSerial.ToString("D3"),
                                 o.OpNum,
                                 SumModeId = m.ModuleId,
                                 GroupName = m.ModuleName,
                                 o.OpName,
                                 o.OpNameLan,
                                 f.SysFileName
                             }).OrderBy(x => x.SumModeId).ThenBy(o => o.OpNum);

            var result = sumModule.ToArray().Length == 0 ? SummarizeJigFileByDefault(opdts, opfs, "No Module found") :
                sumModule;

            return result;
        }

        /// <summary>
        /// Summarizes the jig file by op group.
        /// </summary>
        /// <param name="opdts">The opdts.</param>
        /// <param name="opfs">The opfs.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        private static object SummarizeJigFileByOpGroup(List<Opdt> opdts, List<Opfl> opfs)
        {
            // Get all of group from master table
            var groupCodes = McmtBus.GetMasterCode(ConstantGeneric.GroupOp);

            var sumOpGroup = (from o in opdts
                              join f in opfs on new
                              { o.StyleCode, o.StyleSize, o.StyleColorSerial, o.RevNo, o.OpRevNo, o.OpSerial, o.Edition }
                              equals
                              new { f.StyleCode, f.StyleSize, f.StyleColorSerial, f.RevNo, f.OpRevNo, f.OpSerial, f.Edition }
                              join m in groupCodes on o.OpGroup equals m.SubCode
                              where !string.IsNullOrEmpty(f.OrgFileName)
                              select new
                              {
                                  OpSerial = o.OpSerial.ToString("D3"),
                                  o.OpNum,
                                  SumModeId = o.OpGroup,
                                  GroupName = m.CodeName,
                                  o.OpName,
                                  o.OpNameLan,
                                  f.SysFileName
                              }).OrderBy(x => x.SumModeId).ThenBy(o => o.OpNum);

            var result = sumOpGroup.ToArray().Length == 0 ? SummarizeJigFileByDefault(opdts, opfs, "No OpGroup found") :
                sumOpGroup;

            return result;
        }

        /// <summary>
        /// Summarizes the jig file by default.
        /// </summary>
        /// <param name="opdts">The list of opdts.</param>
        /// <param name="opfs">The list of operation files.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        private static object SummarizeJigFileByDefault(List<Opdt> opdts, List<Opfl> opfs, string groupName)
        {
            var result = (from o in opdts
                          join f in opfs on new
                          { o.StyleCode, o.StyleSize, o.StyleColorSerial, o.RevNo, o.OpRevNo, o.OpSerial, o.Edition }
                          equals
                          new { f.StyleCode, f.StyleSize, f.StyleColorSerial, f.RevNo, f.OpRevNo, f.OpSerial, f.Edition }
                          where !string.IsNullOrEmpty(f.OrgFileName)
                          select new
                          {
                              OpSerial = o.OpSerial.ToString("D3"),
                              o.OpNum,
                              SumModeId = "null000",
                              GroupName = groupName,
                              o.OpName,
                              o.OpNameLan,
                              f.SysFileName
                          }).OrderBy(x => x.SumModeId).ThenBy(o => o.OpNum);

            return result;
        }
        #endregion

        #region Create Process image link
        /// <summary>
        /// Create process image link
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public static string CreateProcessImageLink(string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return string.Empty;

            //Get http link for show process image
            var ftpInfo = FtpInfoBus.GetFtpInfo(ConstantGeneric.FtpAppTypeOpsHost);
            var httpImage = ftpInfo.FtpLink + ConstantGeneric.PorcessImageHostDirectory + imageName;

            return httpImage;
        }

        /// <summary>
        /// Get list of process with none standard name
        /// </summary>
        /// <param name="opMaster"></param>
        /// <returns></returns>
        public static List<Opdt> GetListProcesWithNoneStandardName(Opmt opMaster)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", opMaster.StyleCode),
                new OracleParameter("P_STYLESIZE", opMaster.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opMaster.StyleColorSerial),
                new OracleParameter("P_REVNO", opMaster.RevNo),
                new OracleParameter("P_OPREVNO", opMaster.OpRevNo),
                new OracleParameter("P_EDITION", opMaster.Edition),
                cursor
            };
            var lstStyleMaster = OracleDbManager.GetObjects<Opdt>("SP_OPS_GETPRONONESTDNAMES_OPDT",
                CommandType.StoredProcedure, oracleParams.ToArray());
            return lstStyleMaster;

        }
        #endregion

        #endregion

        #region MySQL database

        /// <summary>
        /// Gets the specified opdt.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static Opdt Get(Opdt opdt)
        {
            var prs = new OpsMySqlParams(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                opdt.RevNo, opdt.OpRevNo) { new MySqlParameter("P_OPSERIAL", opdt.OpSerial) };

            var opdtResult = MySqlDBManager.GetAll<Opdt>("SP_MES_GET_OPDT", CommandType.StoredProcedure,
                prs.ToArray()).FirstOrDefault();

            return opdtResult;
        }

        /// <summary>
        /// Gets the by language.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Opdt> GetByLanguage(string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo, string edition, string languageId)
        {
            var subUrl = FtpInfoBus.GetSubUrl();
            var mySqlParams = new OpsMySqlParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new MySqlParameter("P_LANGUAGEID", languageId?.ToLower()),
                new MySqlParameter("P_URL", subUrl)
            };
            var opdts = MySqlDBManager.GetAll<Opdt>("SP_MES_GETBYLANG_OPDT", CommandType.StoredProcedure,
                mySqlParams.ToArray());

            return opdts;
        }

        public async Task<List<Opdt>> GetWithLanguage(string mxpackage, string styleCode, string styleSize,
            string styleColorSerial, string revNo, string edition)
        {
            var mySqlParams = new OpsMySqlParams(styleCode, styleSize, styleColorSerial, revNo)
            {
                new MySqlParameter("p_mxpackage", mxpackage),
                new MySqlParameter("P_EDITION", edition)
            };
            var opdts = await _mySqlDBManager.GetAllAsync<Opdt>(_mySqlConn, "SP_MES_GETWITHLANG_OPDT", CommandType.StoredProcedure,
                mySqlParams.ToArray());

            return opdts;
        }

        /// <summary>
        /// Gets the maximum op serial.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">Color of the style.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static int GetMaxOpSerial(string styleCode, string styleSize, string styleColor, string revNo,
            string opRevNo)
        {
            var mySqlParams = new OpsMySqlParams(styleCode, styleSize, styleColor, revNo, opRevNo);
            var opdt = MySqlDBManager.GetAll<Opdt>("SP_MES_GETMAXOPSERIAL_OPDT", CommandType.StoredProcedure,
                mySqlParams.ToArray()).FirstOrDefault();

            if (opdt != null) return decimal.ToInt32(opdt.MaxOpSerial) + 1;
            return 1;
        }

        /// <summary>
        /// Updates the op next op.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateOpNextOp(Opdt opdt, MySqlConnection connection, MySqlTransaction transaction)
        {

            var ps = new OpsMySqlParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                opdt.RevNo, opdt.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opdt.OpSerial),
                new MySqlParameter("P_OPNEXTOP", opdt.NextOp)
            };

            var resUpdate = MySqlDBManager.ExecuteQueryWithTrans("SP_OPS_UPDATENEXTOP_OPDT", ps.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return resUpdate != null;
        }

        /// <summary>
        /// Gets the by op group.
        /// </summary>
        /// <param name="opsMaster">The ops master.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object GetByOpGroup(Opmt opsMaster, string languageId)
        {
            // Get list of operation details
            var opdts = GetByLanguage(opsMaster.StyleCode, opsMaster.StyleSize, opsMaster.StyleColorSerial,
                opsMaster.RevNo, opsMaster.OpRevNo, opsMaster.Edition, languageId);

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of groups
            //var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => grp.First()).ToList();
            var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => new Opdt
            {
                OpGroup = grp.First().OpGroup,
                X = grp.First().X,
                Y = grp.First().Y,
                DisplayColor = grp.First().DisplayColor,
                Page = grp.First().Page,
                OpTime = grp.Sum(o => o.OpTime)
            }).ToList();

            // Get all of groups from master table
            var groupCodes = McmtBus.GetByMasterCode(ConstantGeneric.GroupOp);

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.SubCode equals opdt.OpGroup
                         select new
                         {
                             SubCode = opdt.OpGroup,
                             g.CodeName,
                             key = g.SubCode,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0],
                             BackgroundColor = opdt.DisplayColor == null ? "#FFFFFF" : $"#{opdt.DisplayColor?.Substring(3)}",
                             opdt.Page,
                             opdt.OpTime
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = groupCodes };

            return result;
        }

        /// <summary>
        /// Gets the type of the opdt by machine.
        /// </summary>
        /// <param name="opsMaster">The ops master.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="page">Current page</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object GetByModuleType(Opmt opsMaster, string languageId, int page)
        {
            // Get list of operation details
            var opdts = GetByLanguage(opsMaster.StyleCode, opsMaster.StyleSize, opsMaster.StyleColorSerial,
                opsMaster.RevNo, opsMaster.OpRevNo, opsMaster.Edition, languageId);
            var totalPage = opdts.Max(x => x.Page);
            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() :
                opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.ModuleId).Select(grp => grp.First()).ToList();

            // Get all of group from master table
            var groupCodes = SamtBus.GetByStyleCode(opsMaster.StyleCode);

            // Group for adding new 
            var groupsToAdd = from g in groupCodes
                              select new
                              {
                                  SubCode = g.ModuleId,
                                  CodeName = g.ModuleName
                              };

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.ModuleId equals opdt.ModuleId
                         select new
                         {
                             SubCode = opdt.ModuleId,
                             CodeName = g.ModuleName,
                             PartComment = g.PartComment,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0]
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd, totalPage };

            return result;
        }

        public async Task<object> GetByModuleType(Opmt opsMaster, int page)
        {
            // Get list of operation details
            var opdts = await GetWithLanguage(opsMaster.MxPackage, opsMaster.StyleCode, opsMaster.StyleSize,
                opsMaster.StyleColorSerial, opsMaster.RevNo, opsMaster.Edition);
            var totalPage = opdts.Max(x => x.Page);
            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() :
                opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.ModuleId).Select(grp => grp.First()).ToList();

            // Get all of group from master table
            var groupCodes = SamtBus.GetByStyleCode(opsMaster.StyleCode);

            // Group for adding new 
            var groupsToAdd = from g in groupCodes
                              select new
                              {
                                  SubCode = g.ModuleId,
                                  CodeName = g.ModuleName
                              };

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.ModuleId equals opdt.ModuleId
                         select new
                         {
                             SubCode = opdt.ModuleId,
                             CodeName = g.ModuleName,
                             PartComment = g.PartComment,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0]
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd, totalPage };

            return result;
        }

        /// <summary>
        /// Gets the type of the opdt by machine.
        /// </summary>
        /// <param name="opsMaster">The ops master.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <param name="page">The current page</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object GetByMachineType(Opmt opsMaster, string languageId, int page)
        {
            // Get list of operation details
            var opdts = GetByLanguage(opsMaster.StyleCode, opsMaster.StyleSize, opsMaster.StyleColorSerial,
                opsMaster.RevNo, opsMaster.OpRevNo, opsMaster.Edition, languageId);
            var totalPage = opdts.Max(x => x.Page);
            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() :
                opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Get list of machines
            var mcmtMachines = McmtBus.GetByMasterCode(ConstantGeneric.MachineType);
            var otmtMachines = OtmtBus.MySqlGetOpMachines();
            var tempMachines = from mc in mcmtMachines
                               select new Otmt
                               {
                                   ItemCode = mc.SubCode,
                                   ItemName = mc.CodeName
                               };

            // Machines were stored in 2 tables so we need to aggregate them
            var totalMachines = (from mc in otmtMachines
                                 select new
                                 {
                                     SubCode = mc.ItemCode,
                                     CodeName = mc.ItemName
                                 }).Union(from mc in tempMachines
                                          select new
                                          {
                                              SubCode = mc.ItemCode,
                                              CodeName = mc.ItemName
                                          }).ToArray();

            var allMachineGroups = opdts.GroupBy(x => x.MachineType).Select(grp => grp.First());

            var groupMachines = (from g in allMachineGroups
                                 join m in totalMachines
                                 on g.MachineType equals m.SubCode
                                 select new
                                 {
                                     SubCode = g.MachineType,
                                     m.CodeName,
                                     X = g.X?.Split('.')[0],
                                     Y = g.Y?.Split('.')[0]
                                 }).OrderBy(x => x.SubCode);

            var result = new { groups = groupMachines, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = totalMachines, totalPage };

            return result;
        }

        public async Task<object> GetByMachineType(Opmt opsMaster, int page)
        {
            // Get list of operation details
            var opdts = await GetWithLanguage(opsMaster.MxPackage, opsMaster.StyleCode, opsMaster.StyleSize,
                opsMaster.StyleColorSerial, opsMaster.RevNo, opsMaster.Edition);
            var totalPage = opdts.Max(x => x.Page);
            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() :
                opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Get list of machines
            var mcmtMachines = McmtBus.GetByMasterCode(ConstantGeneric.MachineType);
            var otmtMachines = OtmtBus.MySqlGetOpMachines();
            var tempMachines = from mc in mcmtMachines
                               select new Otmt
                               {
                                   ItemCode = mc.SubCode,
                                   ItemName = mc.CodeName
                               };

            // Machines were stored in 2 tables so we need to aggregate them
            var totalMachines = (from mc in otmtMachines
                                 select new
                                 {
                                     SubCode = mc.ItemCode,
                                     CodeName = mc.ItemName
                                 }).Union(from mc in tempMachines
                                          select new
                                          {
                                              SubCode = mc.ItemCode,
                                              CodeName = mc.ItemName
                                          }).ToArray();

            var allMachineGroups = opdts.GroupBy(x => x.MachineType).Select(grp => grp.First());

            var groupMachines = (from g in allMachineGroups
                                 join m in totalMachines
                                 on g.MachineType equals m.SubCode
                                 select new
                                 {
                                     SubCode = g.MachineType,
                                     m.CodeName,
                                     X = g.X?.Split('.')[0],
                                     Y = g.Y?.Split('.')[0]
                                 }).OrderBy(x => x.SubCode);

            var result = new { groups = groupMachines, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = totalMachines, totalPage };

            return result;
        }

        public static object GetByOpGroup(Opmt opsMaster, string languageId, int page)
        {
            // Get list of operation details
            var opdts = GetByLanguage(opsMaster.StyleCode, opsMaster.StyleSize, opsMaster.StyleColorSerial,
                opsMaster.RevNo, opsMaster.OpRevNo, opsMaster.Edition, languageId);

            var totalPage = opdts.Max(x => x.Page);

            opdts = page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() :
                opdts.Where(x => x.Page == page).ToList();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => grp.First()).ToList();

            // Get all of group from master table
            var groupCodes = McmtBus.GetMasterCode(ConstantGeneric.GroupOp);

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.SubCode equals opdt.OpGroup
                         select new
                         {
                             SubCode = opdt.OpGroup,
                             g.CodeName,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0],
                             opdt.Page
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = groupCodes, totalPage };

            return result;
        }

        /// <summary>
        /// Loading list of processes for seating to table.
        /// </summary>
        /// <param name="opmt"></param>
        /// <returns></returns>
        public async Task<object> GetByOpGroup(Opmt opmt)
        {
            // Get list of operation details
            var opdts = await GetWithLanguage(opmt.MxPackage, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.Edition);

            // Distinct groups, we have all of group
            var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => new Opdt
            {
                OpGroup = grp.First().OpGroup,
                X = grp.First().X,
                Y = grp.First().Y,
                DisplayColor = grp.First().DisplayColor,
                Page = grp.First().Page,
                OpTime = grp.Sum(o => o.OpTime)
            }).ToList();

            // Get all of groups from master table
            var groupCodes = McmtBus.GetByMasterCode(ConstantGeneric.GroupOp);

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.SubCode equals opdt.OpGroup
                         select new
                         {
                             SubCode = opdt.OpGroup,
                             g.CodeName,
                             key = g.SubCode,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0],
                             BackgroundColor = opdt.DisplayColor == null ? "#FFFFFF" : $"#{opdt.DisplayColor?.Substring(3)}",
                             opdt.Page,
                             opdt.OpTime
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial) };

            return result;
        }

        public async Task<object> GetByOpGroup(Opmt opsMaster, int page)
        {
            // Get list of operation details
            var opdts = await GetWithLanguage(opsMaster.MxPackage, opsMaster.StyleCode, opsMaster.StyleSize,
                opsMaster.StyleColorSerial, opsMaster.RevNo, opsMaster.Edition);

            //if (opdts == null || opdts.Count == 0)
            //{
            //    return new { IsSuccess = false, Log = "Existing opmt in db but no process (opdt)" };
            //}

            var totalPage = opdts == null || opdts.Count == 0 ? 1 : opdts.Max(x => x.Page);

            opdts = opdts != null && opdts.Count != 0 ? page == 1 ? opdts.Where(x => x.Page == 1 || x.Page == 0).ToList() :
                opdts.Where(x => x.Page == page).ToList() : new List<Opdt>();

            // Get all connection of processes           
            var edges = GetEdges(opdts);

            // Distinct groups, we have all of group
            //var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => grp.First()).ToList();
            var allGroup = opdts.GroupBy(x => x.OpGroup).Select(grp => new Opdt
            {
                OpGroup = grp.First().OpGroup,
                X = grp.First().X,
                Y = grp.First().Y,
                DisplayColor = grp.First().DisplayColor,
                Page = grp.First().Page,
                OpTime = grp.Sum(o => o.OpTime)
            }).ToList();

            // Get all of group from master table
            var groupCodes = McmtBus.GetMasterCode(ConstantGeneric.GroupOp);

            // All of groups for all process
            var groups = from g in groupCodes
                         join opdt in allGroup on g.SubCode equals opdt.OpGroup
                         select new
                         {
                             SubCode = opdt.OpGroup,
                             g.CodeName,
                             key = g.SubCode,
                             X = opdt.X?.Split('.')[0],
                             Y = opdt.Y?.Split('.')[0],
                             BackgroundColor = opdt.DisplayColor == null ? "#FFFFFF" : $"#{opdt.DisplayColor?.Substring(3)}",
                             opdt.Page,
                             opdt.OpTime
                         };

            var result = new { groups, nodes = opdts.OrderBy(x => x.OpSerial), edges, groupsToAdd = groupCodes, totalPage };

            return result;
        }


        /// <summary>
        /// Inserts the opdt.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>true/false</returns>
        /// Author: Nguyen Xuan Hoang
        public static bool InsertOpdt(Opdt opdt, MySqlConnection connection, MySqlTransaction transaction)
        {
            //Check the lenght of opname.
            if (opdt.OpName?.Length > 200) opdt.OpName = opdt.OpName.Substring(0, 199);

            var mySqlParams = new OpsMySqlParams(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo,
                opdt.OpRevNo)
                        {
                            new MySqlParameter("P_OPSERIAL", opdt.OpSerial),
                            new MySqlParameter("P_OPNUM", opdt.OpNum),
                            new MySqlParameter("P_OPGROUP", opdt.OpGroup),
                            new MySqlParameter("P_OPNAME", opdt.OpName),
                            new MySqlParameter("P_FACTORY", opdt.Factory),
                            new MySqlParameter("P_MACHINETYPE", opdt.MachineType),
                            new MySqlParameter("P_THREADCOLOR", opdt.ThreadColor),
                            new MySqlParameter("P_OPDESC", opdt.OpDesc),
                            new MySqlParameter("P_OPTIME", opdt.OpTime),
                            new MySqlParameter("P_OPPRICE", opdt.OpPrice),
                            new MySqlParameter("P_OFFEROPPRICE", opdt.OfferOpPrice),
                            new MySqlParameter("P_MACHINECOUNT", opdt.MachineCount),
                            new MySqlParameter("P_REMARKS", opdt.Remarks),
                            new MySqlParameter("P_MAXTIME", opdt.MaxTime),
                            new MySqlParameter("P_MANCOUNT", opdt.ManCount),
                            new MySqlParameter("P_FILENAME", opdt.FileName),
                            new MySqlParameter("P_NEXTOP", opdt.NextOp),
                            new MySqlParameter("P_OUTSOURCED", opdt.OutSourced),
                            new MySqlParameter("P_X", opdt.X),
                            new MySqlParameter("P_Y", opdt.Y),
                            new MySqlParameter("P_IMAGENAME", opdt.ImageName),
                            new MySqlParameter("P_DISPLAYCOLOR", opdt.DisplayColor),
                            new MySqlParameter("P_PAGE", opdt.Page),
                            new MySqlParameter("P_GROUPCOLOR", opdt.GroupColor),
                            new MySqlParameter("P_VIDEOFILE", opdt.VideoFile),
                            new MySqlParameter("P_JOBTYPE", opdt.JobType),
                            new MySqlParameter("P_SEATNO", opdt.SeatNo),
                            new MySqlParameter("P_SEWINGFILE", opdt.SewingFile),
                            new MySqlParameter("P_BENCHMARKTIME", opdt.BenchmarkTime),
                            new MySqlParameter("P_LABORTYPE", opdt.LaborType),
                            new MySqlParameter("P_COMPONENTID", opdt.ComponentId),
                            new MySqlParameter("P_ACTIONCODE", opdt.ActionCode),
                            new MySqlParameter("P_MODULEID", opdt.ModuleId),
                            new MySqlParameter("P_HOTSPOT", opdt.HotSpot),
                            new MySqlParameter("P_TOOLID", opdt.ToolId),
                            new MySqlParameter("P_STITCHCOUNT", opdt.StitchCount),
                            new MySqlParameter("P_OPTIMEBALANCING", opdt.OpTimeBalancing),
                            new MySqlParameter("P_OPSSTATE", opdt.OpsState),
                            new MySqlParameter("P_TABLEID", opdt.TableId),
                            new MySqlParameter("P_LINESERIAL", opdt.LineSerial),
                            new MySqlParameter("P_MCID", opdt.McId),
                            new MySqlParameter("P_MC_PAIR_DATE", opdt.McPairDate),
                            new MySqlParameter("P_ASSEMBLYMDL", opdt.AssemblyMdl),
                            new MySqlParameter("P_FINALASSEMBLY", opdt.FinalAssembly),
                            new MySqlParameter("P_IOTTYPE", opdt.IotType),
                            new MySqlParameter("P_PAINTINGTYPE", opdt.PaintingType),
                            new MySqlParameter("P_MATERIALTYPE", opdt.MaterialType),
                            new MySqlParameter("P_DRYINGTIME", opdt.DryingTime),
                            new MySqlParameter("P_TEMPERATURE", opdt.Temperature),
                            new MySqlParameter("P_COOLINGTIME", opdt.CoolingTime)
                        };

            var insertResult = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_INSERT_OPDT", mySqlParams.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return insertResult != null;
        }

        /// <summary>
        /// Inserts list of operation detail.
        /// </summary>
        /// <param name="opdts">The opdt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns>true/false</returns>
        /// Author: Nguyen Xuan Hoang
        public async Task<bool> BulkInsertOpdtAsync(List<Opdt> opdts, MySqlConnection connection, MySqlTransaction transaction)
        {
            var qValue = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                var opName = opdts[i].OpName?.Length > 200 ? opdts[i].OpName.Substring(0, 199) : opdts[i].OpName;
                var mcPairDate = $"{opdts[i].McPairDate.Year}-{opdts[i].McPairDate.Month}-{opdts[i].McPairDate.Day}";

                qValue += $"('{opdts[i].StyleCode}','{opdts[i].StyleSize}','{opdts[i].StyleColorSerial}','{opdts[i].RevNo}'," +
                          $"'{opdts[i].OpRevNo}','{opdts[i].OpSerial}','{opdts[i].OpNum}','{opdts[i].OpGroup}','{opName}'," +
                          $"'{opdts[i].Factory}','{opdts[i].MachineType}','{opdts[i].ThreadColor}','{opdts[i].OpDesc}'," +
                          $"'{opdts[i].OpTime}','{opdts[i].OpPrice}','{opdts[i].OfferOpPrice}','{opdts[i].MachineCount}'," +
                          $"'{opdts[i].Remarks}','{opdts[i].MaxTime}','{opdts[i].ManCount}','{opdts[i].FileName}'," +
                          $"'{opdts[i].NextOp}','{opdts[i].OutSourced}','{opdts[i].X}','{opdts[i].Y}','{opdts[i].ImageName}'," +
                          $"'{opdts[i].DisplayColor}','{opdts[i].Page}','{opdts[i].GroupColor}','{opdts[i].VideoFile}'," +
                          $"'{opdts[i].JobType}','{opdts[i].SeatNo}','{opdts[i].SewingFile}','{opdts[i].BenchmarkTime}'," +
                          $"'{opdts[i].LaborType}','{opdts[i].ComponentId}','{opdts[i].ActionCode}','{opdts[i].ModuleId}'," +
                          $"'{opdts[i].HotSpot}','{opdts[i].ToolId}','{opdts[i].StitchCount}','{opdts[i].OpTimeBalancing}'," +
                          $"'{opdts[i].OpsState}','{opdts[i].TableId}','{opdts[i].LineSerial}','{opdts[i].McId}','{mcPairDate}'," +
                          $"{opdts[i].AssemblyMdl},{opdts[i].FinalAssembly},'{opdts[i].IotType}','{opdts[i].PaintingType}'," +
                          $"'{opdts[i].MaterialType}','{opdts[i].DryingTime}','{opdts[i].Temperature}','{opdts[i].CoolingTime}')";

                if (i != opdts.Count - 1) qValue += ",";
            }

            var q = $@"INSERT INTO `mes`.`t_mx_opdt`
                            (`STYLECODE`,
                            `STYLESIZE`,
                            `STYLECOLORSERIAL`,
                            `REVNO`,
                            `OPREVNO`,
                            `OPSERIAL`,
                            `OPNUM`,
                            `OPGROUP`,
                            `OPNAME`,
                            `FACTORY`,
                            `MACHINETYPE`,
                            `THREADCOLOR`,
                            `OPDESC`,
                            `OPTIME`,
                            `OPPRICE`,
                            `OFFEROPPRICE`,
                            `MACHINECOUNT`,
                            `REMARKS`,
                            `MAXTIME`,
                            `MANCOUNT`,
                            `FILENAME`,
                            `NEXTOP`,
                            `OUTSOURCED`,
                            `X`,
                            `Y`,
                            `IMAGENAME`,
                            `DISPLAYCOLOR`,
                            `PAGE`,
                            `GROUPCOLOR`,
                            `VIDEOFILE`,
                            `JOBTYPE`,
                            `SEATNO`,
                            `SEWINGFILE`,
                            `BENCHMARKTIME`,
                            `LABORTYPE`,
                            `COMPONENTID`,
                            `ACTIONCODE`,
                            `MODULEID`,
                            `HOTSPOT`,
                            `TOOLID`,
                            `STITCHCOUNT`,
                            `OPTIMEBALANCING`,
                            `OPSSTATE`,
                            `TABLEID`,
                            `LINESERIAL`,
                            `MCID`,
                            `MC_PAIR_DATE`,
                            `ASSEMBLYMDL`,
                            `FINALASSEMBLY`,
                            `IOTTYPE`,
                            `PAINTINGTYPE`,
                            `MATERIALTYPE`,
                            `DRYINGTIME`,
                            `TEMPERATURE`,
                            `COOLINGTIME`)
                        VALUES {qValue};";
            var rs = await _mySqlDBManager.ExecuteWithTransAsync(q, null, CommandType.Text, transaction, connection);

            return rs != null;
        }

        /// <summary>
        /// Deletes the opdt.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool DeleteOpdt(Opdt opdt, MySqlTransaction transaction, MySqlConnection connection)
        {
            var prs = new OpsMySqlParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo,
                opdt.OpRevNo) { new MySqlParameter("P_OPSERIAL", opdt.OpSerial) };
            var rs = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_DELETE_OPDT", prs.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return rs != null;
        }

        public static bool InsertProccess(Opdt opdt, List<Optl> machines, List<Optl> tools, List<Opnt> opnts)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add operation detail
                    if (InsertOpdt(opdt, connection, trans))
                    {
                        //Add list operation name detail
                        foreach (var opnt in opnts)
                        {
                            OpntBus.InsertOpnt(opnt, connection, trans);
                        }

                        //Add list machine
                        foreach (var opMachine in machines)
                        {
                            OptlBus.InsertTool(opMachine, connection, trans);
                        }

                        //Add list tool
                        foreach (var opTool in tools)
                        {
                            OptlBus.InsertTool(opTool, connection, trans);
                        }

                        trans.Commit();
                        return true;
                    }

                    trans.Rollback();
                    return false;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the opdts and tool.
        /// </summary>
        /// <param name="opdts">The opdts.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool DeleteOpdtsAndTool(List<Opdt> opdts)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    var lstOpdt = GetByLanguage(opdts[0].StyleCode, opdts[0].StyleSize, opdts[0].StyleColorSerial,
                        opdts[0].RevNo, opdts[0].OpRevNo, opdts[0].Edition, "");

                    foreach (var opDetail in opdts)
                    {
                        if (!(OptlBus.DeleteOptl(opDetail, trans, connection) && OpntBus.DeleteOpnt(opDetail, connection, trans)))
                        {
                            trans.Rollback();
                            return false;
                        }

                        //Delete ops detail
                        if (DeleteOpdt(opDetail, trans, connection))
                        {
                            //Update next process.
                            var lstNextOp = lstOpdt.Where(s => s.NextOp == opDetail.OpSerial.ToString());
                            foreach (var nextOp in lstNextOp)
                            {
                                nextOp.NextOp = "";
                                UpdateOpNextOp(nextOp, connection, trans);
                            }
                            continue;
                        }

                        trans.Rollback();
                        return false;
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the opdt and tool.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool DeleteOpdtAndTool(Opdt opdt)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                var hasDelToolLinking = OptlBus.DeleteOptl(opdt, trans, connection);
                var hasDelOpName = OpntBus.DeleteOpnt(opdt, connection, trans);
                //var hasDelPattern = ProtBus.DeleteByOpdt(opdt, trans, connection);
                if (hasDelToolLinking && hasDelOpName)
                {
                    //Delete ops detail
                    var hasDeleteOpdt = DeleteOpdt(opdt, trans, connection);
                    if (hasDeleteOpdt)
                    {
                        trans.Commit();
                        return true;
                    }

                    trans.Rollback();
                    return false;
                }

                trans.Rollback();
                return false;
            }
        }

        /// <summary>
        /// Updates the opdt.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateOpdt(Opdt opdt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var prs = new OpsMySqlParams(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opdt.OpSerial),
                new MySqlParameter("P_OPNUM", opdt.OpNum),
                new MySqlParameter("P_OPGROUP", opdt.OpGroup),
                new MySqlParameter("P_OPNAME", opdt.OpName),
                new MySqlParameter("P_FACTORY", opdt.Factory),
                new MySqlParameter("P_MACHINETYPE", opdt.MachineType),
                new MySqlParameter("P_THREADCOLOR", opdt.ThreadColor),
                new MySqlParameter("P_OPDESC", opdt.OpDesc),
                new MySqlParameter("P_OPTIME", opdt.OpTime),
                new MySqlParameter("P_OPPRICE", opdt.OpPrice),
                new MySqlParameter("P_OFFEROPPRICE", opdt.OfferOpPrice),
                new MySqlParameter("P_MACHINECOUNT", opdt.MachineCount),
                new MySqlParameter("P_REMARKS", opdt.Remarks),
                new MySqlParameter("P_MAXTIME", opdt.MaxTime),
                new MySqlParameter("P_MANCOUNT", opdt.ManCount),
                new MySqlParameter("P_FILENAME", opdt.FileName),
                new MySqlParameter("P_NEXTOP", opdt.NextOp),
                new MySqlParameter("P_OUTSOURCED", opdt.OutSourced),
                new MySqlParameter("P_X", opdt.X),
                new MySqlParameter("P_Y", opdt.Y),
                new MySqlParameter("P_IMAGENAME", opdt.ImageName),
                new MySqlParameter("P_DISPLAYCOLOR", opdt.DisplayColor),
                new MySqlParameter("P_PAGE", opdt.Page),
                new MySqlParameter("P_GROUPCOLOR", opdt.GroupColor),
                new MySqlParameter("P_VIDEOFILE", opdt.VideoFile),
                new MySqlParameter("P_JOBTYPE", opdt.JobType),
                new MySqlParameter("P_SEATNO", opdt.SeatNo),
                new MySqlParameter("P_SEWINGFILE", opdt.SewingFile),
                new MySqlParameter("P_BENCHMARKTIME", opdt.BenchmarkTime),
                new MySqlParameter("P_LABORTYPE", opdt.LaborType),
                new MySqlParameter("P_COMPONENTID", opdt.ComponentId),
                new MySqlParameter("P_ACTIONCODE", opdt.ActionCode),
                new MySqlParameter("P_MODULEID", opdt.ModuleId),
                new MySqlParameter("P_HOTSPOT", opdt.HotSpot),
                new MySqlParameter("P_TOOLID", opdt.ToolId),
                new MySqlParameter("P_STITCHCOUNT", opdt.StitchCount),
                new MySqlParameter("P_OPTIMEBALANCING", opdt.OpTimeBalancing),
                new MySqlParameter("P_OPSSTATE", opdt.OpsState),
                new MySqlParameter("P_TABLEID", opdt.TableId),
                new MySqlParameter("P_LINESERIAL", opdt.LineSerial),
                new MySqlParameter("P_MCID", opdt.McId),
                new MySqlParameter("P_MC_PAIR_DATE", opdt.McPairDate),
                new MySqlParameter("P_ASSEMBLYMDL", opdt.AssemblyMdl),
                new MySqlParameter("P_FINALASSEMBLY", opdt.FinalAssembly),
                new MySqlParameter("P_IOTTYPE", opdt.IotType),
                new MySqlParameter("P_PAINTINGTYPE", opdt.PaintingType),
                new MySqlParameter("P_MATERIALTYPE", opdt.MaterialType),
                new MySqlParameter("P_DRYINGTIME", opdt.DryingTime),
                new MySqlParameter("P_TEMPERATURE", opdt.Temperature),
                new MySqlParameter("P_COOLINGTIME", opdt.CoolingTime)
            };

            var resUpdate = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_UPDATE_OPDT", prs.ToArray(),
                    CommandType.StoredProcedure, transaction, connection);

            return resUpdate != null;
        }

        /// <summary>
        /// Updates the opdt.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="optls">The optls.</param>
        /// <param name="tools">The tools.</param>
        /// <param name="opnts">The opnts.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateOpdt(Opdt opdt, List<Optl> optls, List<Optl> tools, List<Opnt> opnts)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    if (UpdateOpdt(opdt, connection, trans))
                    {
                        //Update optime of ops master.
                        if (opdt.OpTimeMax < opdt.TackTime)
                        {
                            OpmtBus.UpdateOpTime(opdt, connection, trans);
                        }

                        //Deleting and inserting operation name detail.
                        if (opnts != null)
                        {
                            if (OpntBus.DeleteOpnt(opdt, connection, trans))
                            {
                                foreach (var opnt in opnts)
                                {
                                    OpntBus.InsertOpnt(opnt, connection, trans);
                                }
                            }
                        }

                        //Delete toolinking.
                        if (OptlBus.DeleteOptl(opdt, trans, connection))
                        {
                            //Insert toolinking.
                            if (optls != null)
                            {
                                foreach (var tl in optls)
                                {
                                    OptlBus.InsertTool(tl, connection, trans);
                                }
                            }
                            //Add list tool
                            if (tools != null)
                            {
                                foreach (var opTool in tools)
                                {
                                    OptlBus.InsertTool(opTool, connection, trans);
                                }
                            }
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return false;
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the by code.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Opdt> GetByCode(Opdt opdt)
        {
            var prs = new OpsMySqlParams(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opdt.OpSerial),
                new MySqlParameter("P_MASTERCODE1", ConstantGeneric.GroupOp),
                new MySqlParameter("P_ITEMCODE", opdt.MachineType),
                new MySqlParameter("P_OPGROUP", opdt.OpGroup)
            };

            var opdts = MySqlDBManager.GetAll<Opdt>("SP_MES_GETBYCODE_OPDT", CommandType.StoredProcedure,
                prs.ToArray());

            return opdts;
        }

        /// <summary>
        /// Updates the layout.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateLayout(Opdt opdt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var prs = new OpsMySqlParams(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo,
                opdt.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opdt.OpSerial),
                new MySqlParameter("P_OPGROUP", opdt.OpGroup),
                new MySqlParameter("P_OPNAME", opdt.OpName),
                new MySqlParameter("P_MACHINETYPE", opdt.MachineType),
                new MySqlParameter("P_MODULEID", opdt.ModuleId),
                new MySqlParameter("P_NEXTOP", opdt.NextOp),
                new MySqlParameter("P_X", opdt.X),
                new MySqlParameter("P_Y", opdt.Y),
                new MySqlParameter("P_DISPLAYCOLOR", opdt.DisplayColor),
                new MySqlParameter("P_PAGE", opdt.Page)
            };

            // Save OPDT then
            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_OPS_UPDATELAYOUT_OPDT", prs.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        /// <summary>
        /// Updates the layout opdts.
        /// </summary>
        /// <param name="opdtList">The opdt list.</param>
        /// <param name="opmt">The opmt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool UpdateLayoutOpdts(List<Opdt> opdtList, Opmt opmt)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    // Save opmt firstly
                    var updatedOpmtResult = OpmtBus.UpdateLayout(opmt, connection, transaction);
                    if (!updatedOpmtResult) return false;

                    UpdateLayout(opdtList, connection, transaction);
                    transaction.Commit();

                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Updates the layout (list of processes).
        /// </summary>
        /// <param name="opdts">The list of operation details (processes).</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 18-Sep-19
        public static bool UpdateLayout(List<Opdt> opdts, MySqlConnection connection, MySqlTransaction transaction)
        {
            string opSerials = "", opNameData = "", opGroupData = "", machineTypeData = "", moduleIdData = "",
                nextOpData = "", xData = "", yData = "", displayColorData = "", pageData = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                opNameData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].OpName}' ";
                opGroupData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].OpGroup}' ";
                machineTypeData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].MachineType}' ";
                moduleIdData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].ModuleId}' ";
                nextOpData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].NextOp}' ";
                xData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].X}' ";
                yData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].Y}' ";
                displayColorData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].DisplayColor}' ";
                pageData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].Page}' ";
                if (i != opdts.Count - 1)
                {
                    opSerials += $"'{opdts[i].OpSerial}',";
                }
                else
                {
                    opSerials += $"'{opdts[i].OpSerial}'";
                }
            }
            var q = $"UPDATE mes.t_mx_opdt SET opname = (CASE opserial {opNameData} END), " +
                    $"opgroup = (CASE opserial {opGroupData} END), machinetype = (CASE opserial {machineTypeData} END), " +
                    $"moduleid = (CASE opserial {moduleIdData} END), x = (CASE opserial {xData} END), " +
                    $"y = (CASE opserial {yData} END), nextop = (CASE opserial {nextOpData} END), " +
                    $"displaycolor = (CASE opserial {displayColorData} END), page = (CASE opserial {pageData} END) " +
                    $"WHERE OpSerial IN({opSerials}) AND stylecode = '{opdts[0].StyleCode}' AND stylesize = '{opdts[0].StyleSize}' " +
                    $"AND stylecolorserial = '{opdts[0].StyleColorSerial}' AND revno = '{opdts[0].RevNo}' " +
                    $"AND oprevno = '{opdts[0].OpRevNo}'";
            using (var myCmd = new MySqlCommand(q, connection, transaction))
            {
                myCmd.CommandType = CommandType.Text;
                myCmd.ExecuteNonQuery();
            }

            return true;
        }

        /// <summary>
        /// Clones the specified opdt.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="opnts">The opnts.</param>
        /// <param name="optls">The optls.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool Clone(Opdt opdt, List<Opnt> opnts, List<Optl> optls)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (var trans = connection.BeginTransaction())
                {
                    try
                    {
                        InsertOpdt(opdt, connection, trans);

                        //Add list of operation name details
                        foreach (var opnt in opnts)
                        {
                            opnt.OpSerial = opdt.OpSerial;
                            OpntBus.InsertOpnt(opnt, connection, trans);
                        }

                        // Change Ops master key in list copy tool linking
                        foreach (var tool in optls)
                        {
                            tool.Edition = opdt.Edition;
                            tool.OpSerial = opdt.OpSerial;
                            OptlBus.InsertTool(tool, connection, trans);
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public static bool DeleteByOpmt(Opmt opmt, MySqlTransaction transaction, MySqlConnection connection)
        {
            var prs = new OpsMySqlParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);
            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_DELETEBYOPMT_OPDT", prs.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        #region Summaries Operation Layout

        /// <summary>
        /// Summarize processes from MySql db.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static object MySqlSummarizeProcesses(Opmt opmt)
        {
            var prs = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            var buyerCode = opmt.StyleCode.Substring(0, 3);
            var buyers = McmtBus.GetByMasterCode("Buyer").Where(x => x.SubCode == buyerCode);
            var styleSizes = McmtBus.GetByMasterCode("StyleSize").Where(x => x.SubCode == opmt.StyleSize);
            var styleColors = StyleColorBus.GetStyleColor(opmt.StyleCode, opmt.StyleColorSerial);
            var processSummary = MySqlDBManager.GetAll<ProcessSummary>("SP_MES_SUMMARIZE_OPDT", CommandType.StoredProcedure,
                prs.ToArray()).FirstOrDefault();
            var styleMaster = StmtBus.GetByStyleCode(opmt.StyleCode);
            var styleImage = StmtBus.CreateStyleImageLink(styleMaster?.Picture, opmt.StyleCode);
            var style = new
            {
                styleCode = opmt.StyleCode,
                styleMaster?.BuyerStyleCode,
                buyerName = buyers.FirstOrDefault()?.CodeName,
                styleSize = styleSizes.FirstOrDefault()?.CodeName,
                styleColor = styleColors?.StyleColorWays,
                revNo = opmt.RevNo,
                opRevNo = opmt.OpRevNo,
                edition = opmt.Edition,
                picture = styleImage
            };

            return new { style, processSummary };
        }

        /// <summary>
        /// Summarize opdt worker from MySql db.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Opdt> MySqlSummarizeOpdtWorker(Opmt opmt)
        {
            var oracleParams = new OpsMySqlParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);

            return MySqlDBManager.GetAll<Opdt>("SP_MES_SUMMARIZEBYWORKER_OPDT", CommandType.StoredProcedure,
                oracleParams.ToArray());
        }

        /// <summary>
        /// Summarize opdt by machine from MySql db.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Opdt> MySqlSummarizeOpdtByMachine(Opmt opmt)
        {
            var prs = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            var result = MySqlDBManager.GetAll<Opdt>("SP_MES_SUMMARIZEBYMACHINE_OPDT", CommandType.StoredProcedure,
                prs.ToArray());

            return result;
        }

        /// <summary>
        /// Summarize opdt all machine from MySql db.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Otmt> MySqlSummarizeOpdtAllMachine(Opmt opmt)
        {
            var prs = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            var result = MySqlDBManager.GetAll<Otmt>("SP_MES_SUMMARIZEMACHINES_OPDT", CommandType.StoredProcedure,
                prs.ToArray());

            return result;
        }

        /// <summary>
        /// Summarize opdt by tools from MySql db.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Otmt> MySqlSummarizeOpdtByTools(Opmt opmt)
        {
            var prs = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            var result = MySqlDBManager.GetAll<Otmt>("SP_MES_SUMMARIZEBYTOOLS_OPDT", CommandType.StoredProcedure,
                prs.ToArray());

            return result;
        }

        /// <summary>
        /// Summarize opdt by BOM from MySql db.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Prot> MySqlSummarizeBomByProcess(Opmt opmt, string language)
        {
            var prs = new OpsMySqlParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo) { new MySqlParameter("P_LANGUAGEID", language) };
            var result = MySqlDBManager.GetAll<Prot>("SP_MES_SUMMARIZEBOM_PROT", CommandType.StoredProcedure,
                prs.ToArray());

            return result;
        }

        /// <summary>
        /// Summarizes the jig file by module.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="opdts">The opdts.</param>
        /// <param name="opfs">The opfs.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        private static object MySqlSummarizeJigFileByModule(string styleCode, List<Opdt> opdts, List<Opfl> opfs)
        {
            // Get all of module
            var groupCodes = SamtBus.GetByStyleCode(styleCode);

            var sumModule = (from o in opdts
                             join f in opfs on new
                             { o.StyleCode, o.StyleSize, o.StyleColorSerial, o.RevNo, o.OpRevNo, o.OpSerial, o.Edition }
                             equals
                             new { f.StyleCode, f.StyleSize, f.StyleColorSerial, f.RevNo, f.OpRevNo, f.OpSerial, f.Edition }
                             join m in groupCodes on o.ModuleId equals m.ModuleId
                             where !string.IsNullOrEmpty(f.OrgFileName)
                             select new
                             {
                                 OpSerial = o.OpSerial.ToString("D3"),
                                 o.OpNum,
                                 SumModeId = m.ModuleId,
                                 GroupName = m.ModuleName,
                                 o.OpName,
                                 o.OpNameLan,
                                 f.SysFileName
                             }).OrderBy(x => x.SumModeId).ThenBy(o => o.OpNum);

            var result = sumModule.ToArray().Length == 0 ? SummarizeJigFileByDefault(opdts, opfs, "No Module found") :
                sumModule;

            return result;
        }

        /// <summary>
        /// Summarizes the jig file by op group.
        /// </summary>
        /// <param name="opdts">The opdts.</param>
        /// <param name="opfs">The opfs.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        private static object MySqlSummarizeJigFileByOpGroup(List<Opdt> opdts, List<Opfl> opfs)
        {
            // Get all of group from master table
            var groupCodes = McmtBus.GetByMasterCode(ConstantGeneric.GroupOp);

            var sumOpGroup = (from o in opdts
                              join f in opfs on new
                              { o.StyleCode, o.StyleSize, o.StyleColorSerial, o.RevNo, o.OpRevNo, o.OpSerial, o.Edition }
                              equals
                              new { f.StyleCode, f.StyleSize, f.StyleColorSerial, f.RevNo, f.OpRevNo, f.OpSerial, f.Edition }
                              join m in groupCodes on o.OpGroup equals m.SubCode
                              where !string.IsNullOrEmpty(f.OrgFileName)
                              select new
                              {
                                  OpSerial = o.OpSerial.ToString("D3"),
                                  o.OpNum,
                                  SumModeId = o.OpGroup,
                                  GroupName = m.CodeName,
                                  o.OpName,
                                  o.OpNameLan,
                                  f.SysFileName
                              }).OrderBy(x => x.SumModeId).ThenBy(o => o.OpNum);

            var result = sumOpGroup.ToArray().Length == 0 ? SummarizeJigFileByDefault(opdts, opfs, "No OpGroup found") :
                sumOpGroup;

            return result;
        }

        /// <summary>
        /// Summarizes the jig file by default.
        /// </summary>
        /// <param name="opdts">The list of opdts.</param>
        /// <param name="opfs">The list of operation files.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        private static object MySqlSummarizeJigFileByDefault(List<Opdt> opdts, List<Opfl> opfs, string groupName)
        {
            var result = (from o in opdts
                          join f in opfs on new
                          { o.StyleCode, o.StyleSize, o.StyleColorSerial, o.RevNo, o.OpRevNo, o.OpSerial, o.Edition }
                          equals
                          new { f.StyleCode, f.StyleSize, f.StyleColorSerial, f.RevNo, f.OpRevNo, f.OpSerial, f.Edition }
                          where !string.IsNullOrEmpty(f.OrgFileName)
                          select new
                          {
                              OpSerial = o.OpSerial.ToString("D3"),
                              o.OpNum,
                              SumModeId = "null000",
                              GroupName = groupName,
                              o.OpName,
                              o.OpNameLan,
                              f.SysFileName
                          }).OrderBy(x => x.SumModeId).ThenBy(o => o.OpNum);

            return result;
        }

        public static object MySqlSummarizeJigFile(Opmt opmt, string language, string sumJigMode)
        {
            // Get list of operation details
            var opdts = GetByLanguage(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo,
                opmt.Edition, language);

            // Get Opfl
            var opfls = OpflBus.GetByOp(opmt);

            // Get uploadcode (s_code) for opfl
            //var mcmts = McmtBus.GetMasterCode(ConstantGeneric.StyleFile, ConstantGeneric.JigFile);
            var mcmts = McmtBus.GetByMasterCode(ConstantGeneric.StyleFile).Where(x => x.CodeDesc.Contains(ConstantGeneric.JigFile));

            // Get list of opfl by UploadCode in mcmt table
            var opfs = from o in opfls
                       where (from m in mcmts
                              select m.SubCode).Contains(o.UploadCode)
                       select o;

            object result;
            switch (sumJigMode)
            {
                case "ModuleName":
                    result = MySqlSummarizeJigFileByModule(opmt.StyleCode, opdts, opfs.ToList());
                    break;
                case "OpGroup":
                    result = MySqlSummarizeJigFileByOpGroup(opdts, opfs.ToList());
                    break;
                default:
                    result = MySqlSummarizeJigFileByDefault(opdts, opfs.ToList(), "");
                    break;
            }

            return result;
        }
        #endregion

        /// <summary>
        /// MySQL update seat.
        /// </summary>
        /// <param name="tableSpace">The table space.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool MySqlUpdateSeat(TableSpaceEntity tableSpace)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("p_tableid", tableSpace.TableId),
                new MySqlParameter("p_lineserial", tableSpace.LineSerial),
                new MySqlParameter("p_seattotal", tableSpace.SeatTotal)
            };
            var result = MySqlDBManager.ExecuteNonQuery("SP_MES_UPDATESEAT_OPDT", CommandType.StoredProcedure, prs.ToArray());

            return result != null && int.Parse(result.ToString()) != 0;
        }

        /// <summary>
        /// MySQL save seat.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static object MySqlSaveSeat(Opdt opdt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var prs = new OpsMySqlParams(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opdt.OpSerial),
                new MySqlParameter("P_LineSerial", opdt.LineSerial),
                new MySqlParameter("P_TableId", opdt.TableId),
                new MySqlParameter("P_SeatNo", opdt.SeatNo)
            };
            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_SAVESEAT_OPDT", prs.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        public async Task<bool> SaveOpEmpChanges(List<Opdt> opdts)
        {
            string empCodeData = "", xPosData = "", yPosData = "", opSerials = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                empCodeData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].EmployeeCode}' ";
                xPosData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].XPos}' ";
                yPosData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].YPos}' ";

                if (i != opdts.Count - 1)
                {
                    opSerials += $"'{opdts[i].OpSerial}',";
                }
                else
                {
                    opSerials += $"'{opdts[i].OpSerial}'";
                }
            }
            var q = "UPDATE mes.t_mx_opdt SET " +
                       $"EmployeeCode = (CASE opserial {empCodeData} END), " +
                       $"XPos = (CASE opserial {xPosData} END), " +
                       $"YPos = (CASE opserial {yPosData} END) " +
                       $"WHERE OpSerial IN({opSerials}) " +
                       $"AND stylecode = '{opdts[0].StyleCode}' " +
                       $"AND stylesize = '{opdts[0].StyleSize}' " +
                       $"AND stylecolorserial = '{opdts[0].StyleColorSerial}' " +
                       $"AND revno = '{opdts[0].RevNo}' " +
                       $"AND oprevno = '{opdts[0].OpRevNo}';";

            var rs = await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, q, CommandType.Text);

            return rs;
        }

        public async Task<bool> MySqlUpdateModule(List<Opdt> opdts)
        {
            string moduleData = "", opSerials = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                moduleData += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].ModuleId}' ";
                opSerials += i != opdts.Count - 1 ? $"'{opdts[i].OpSerial}'," : $"'{opdts[i].OpSerial}'";
            }

            var q = "UPDATE mes.t_mx_opdt SET " +
                    $"ModuleId = (CASE opserial {moduleData} END) " +
                    $"WHERE OpSerial IN({opSerials}) " +
                    $"AND stylecode = '{opdts[0].StyleCode}' " +
                    $"AND stylesize = '{opdts[0].StyleSize}' " +
                    $"AND stylecolorserial = '{opdts[0].StyleColorSerial}' " +
                    $"AND revno = '{opdts[0].RevNo}' " +
                    $"AND oprevno = '{opdts[0].OpRevNo}';";

            var rs = await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, q, CommandType.Text);
            return rs;
        }

        public async Task<bool> MySqlUpdatePage(List<Opdt> opdts)
        {
            string prcPage = "", opSerials = "";

            for (var i = 0; i < opdts.Count; i++)
            {
                prcPage += $"WHEN {opdts[i].OpSerial} THEN '{opdts[i].Page}' ";
                opSerials += i != opdts.Count - 1 ? $"'{opdts[i].OpSerial}'," : $"'{opdts[i].OpSerial}'";
            }

            var q = "UPDATE mes.t_mx_opdt SET " +
                    $"Page = (CASE opserial {prcPage} END) " +
                    $"WHERE OpSerial IN({opSerials}) " +
                    $"AND stylecode = '{opdts[0].StyleCode}' " +
                    $"AND stylesize = '{opdts[0].StyleSize}' " +
                    $"AND stylecolorserial = '{opdts[0].StyleColorSerial}' " +
                    $"AND revno = '{opdts[0].RevNo}' " +
                    $"AND oprevno = '{opdts[0].OpRevNo}';";

            var rs = await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, q, CommandType.Text);
            return rs;
        }

        public async Task<bool> MySqlUpdateVideoFile(VideoOpdt opdt)
        {
            var q = @"UPDATE MES.T_MX_OPDT SET
                    VideoFile = ?P_VideoFile
                    WHERE 
                    Stylecode = ?P_StyleCode
                    AND Stylesize = ?P_StyleSize 
                    AND Stylecolorserial = ?P_StyleColorSerial
                    AND RevNo = ?P_RevNo
                    AND OpRevNo = ?P_OpRevNo
                    AND OpSerial = ?P_OpSerial";

            var oracleParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_VideoFile", opdt.VideoFile),
                new MySqlParameter("P_StyleCode", opdt.StyleCode),
                new MySqlParameter("P_StyleSize", opdt.StyleSize),
                new MySqlParameter("P_StyleColorSerial", opdt.StyleColorSerial),
                new MySqlParameter("P_RevNo", opdt.RevNo),
                new MySqlParameter("P_OpRevNo", opdt.OpRevNo),
                new MySqlParameter("P_OpSerial", opdt.OpSerial)
            };

            //var rs = await OracleDbManager.ExecQueryAsync(q, oracleParams.ToArray(), CommandType.Text, ConstantGeneric.ConnectionStr);
            var rs = await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, q, CommandType.Text, oracleParams.ToArray());

            return rs;
        }
        #endregion

        public async Task<List<Opdt>> GetVideos(Opdt opdt)
        {
            var oracleParams = new OpsOracleParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                opdt.RevNo, opdt.OpRevNo, opdt.OpSerial);
            oracleParams.AddCursor();

            var opdts = await _oracleDI.GetAllAsync<Opdt>("sp_ops_getvideos_opdt",
                CommandType.StoredProcedure, oracleParams.ToArray(), ConstantGeneric.ConnectionStr);

            return opdts;
        }
    }
}