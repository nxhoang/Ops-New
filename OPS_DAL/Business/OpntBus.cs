using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace OPS_DAL.Business
{
    public class OpntBus
    {
        #region Properties

        private readonly MySqlDBManager _mySqlDBManager = new MySqlDBManager();

        #endregion

        #region General

        public static List<Opnt> GetByOpdtAndLang(string edition, string styleCode, string styleSize, string styleColor,
            string revNo, string opRevNo, string opSerial, string opNameId, string languageId, int sourceDb)
        {
            switch (sourceDb)
            {
                case 1:
                    return GetByOpdtAndLang(styleCode, styleSize, styleColor, revNo, opRevNo, opRevNo, opSerial, opNameId,
                        languageId);
                case 2:
                    return GetOpNameDetails(edition, styleCode, styleSize, styleColor, revNo, opRevNo, opSerial, opNameId, languageId);
                default:
                    return null;
            }
        }

        #endregion

        #region Oracle database

        /// <summary>
        /// Get list opeartion name detail
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <param name="opNameId"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<Opnt> GetOpNameDetails(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string opNameId, string languageId)
        {
            var oracleParams = new OpsOracleParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_OPSERIAL", opSerial),
                new OracleParameter("P_OPNAMEID", opNameId),
                new OracleParameter("P_LANGUAGEID", languageId)
            };
            oracleParams.AddCursor();

            var lstOpnts = OracleDbManager.GetObjects<Opnt>("SP_OPS_GETOPNAMEDETAILS_OPNT", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstOpnts;
        }

        /// <summary>
        /// This function is cloned from GetOpNameDetails for asynchronous purpose.
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <param name="opNameId"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task<List<Opnt>> GetOpnts(string edition, string styleCode, string styleSize, string styleColorSerial, 
            string revNo, string opRevNo, string opSerial, string opNameId, string languageId)
        {
            var oracleParams = new OpsOracleParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_OPSERIAL", opSerial),
                new OracleParameter("P_OPNAMEID", opNameId),
                new OracleParameter("P_LANGUAGEID", languageId)
            };
            oracleParams.AddCursor();

            var opnts = await OracleDbManager.GetAllAsync<Opnt>(ConstantGeneric.ConnectionStr,"SP_OPS_GETOPNAMEDETAILS_OPNT",
                CommandType.StoredProcedure, oracleParams.ToArray());

            return opnts;
        }

        /// <summary>
        /// Insert operation name detail.
        /// </summary>
        /// <param name="opnt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool InsertOpNameDetail(Opnt opnt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opnt.Edition, opnt.StyleCode, opnt.StyleSize, opnt.StyleColorSerial, opnt.RevNo, opnt.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opnt.OpSerial),
                new OracleParameter("P_OPNAMEID", opnt.OpNameId),
                new OracleParameter("P_OPTIME", opnt.OpTime),
                new OracleParameter("P_OPNSERIAL", opnt.OpnSerial),

                //Ha add
                new OracleParameter("P_MACHINETYPE", opnt.MachineType),
                new OracleParameter("P_MACHINECOUNT", opnt.MachineCount),
                new OracleParameter("P_REMARKS", opnt.Remarks),
                new OracleParameter("P_MAXTIME", opnt.MaxTime),
                new OracleParameter("P_MANCOUNT", opnt.ManCount),
                new OracleParameter("P_JOBTYPE", opnt.JobType),
                new OracleParameter("P_TOOLID", opnt.ToolId),
                new OracleParameter("P_ACTIONCODE", opnt.ActionCode),
                new OracleParameter("P_STITCHCOUNT", opnt.StitchCount),//ADD) SON - 25 December 2019
              
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) { Direction = ParameterDirection.Output }
            
            };

            var resIns = OracleDbManager.ExecuteQuery("SP_OPS_INSERT_OPNT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resIns != null;
        }

        public static bool InsertOpNameDetail_New(Opnt opnt, OracleConnection oraConn, OracleTransaction trans)
        {
            string opntTb = (opnt.Edition == "M" || opnt.Edition == "MES") ? "PKMES.t_op_opnt" : "t_op_opnt";

            string strSql = $@"INSERT INTO {opntTb} ( 
                                    stylecode, stylesize, stylecolorserial, revno, oprevno, opserial, opnameid, edition, optime, opnserial
                                    , machinetype, machinecount, remarks, maxtime, mancount, imagename, videofile, jobtype, toolid, actioncode
                                    , stitchcount, mainprocess, stitchinglength, stitchesperinch, iottype, grouplevel_0, grouplevel_1, grouplevel_2 ) 
                            VALUES ( 
                                    :p_stylecode, :p_stylesize, :p_stylecolorserial, :p_revno, :p_oprevno, :p_opserial, :p_opnameid, :p_edition, :p_optime, :p_opnserial
                                    , :p_machinetype, :p_machinecount, :p_remarks, :p_maxtime, :p_mancount, :p_imagename, :p_videofile, :p_jobtype, :p_toolid, :p_actioncode
                                    , :p_stitchcount, :p_mainprocess, :p_stitchinglength, :p_stitchesperinch, :p_iottype, :p_grouplevel_0, :p_grouplevel_1, :p_grouplevel_2)";

            var oracleParams = new List<OracleParameter>()
            {
                 new OracleParameter("P_STYLECODE", opnt.StyleCode),
                new OracleParameter("P_STYLESIZE", opnt.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opnt.StyleColorSerial),
                new OracleParameter("P_REVNO", opnt.RevNo),
                new OracleParameter("P_OPREVNO", opnt.OpRevNo),
                new OracleParameter("P_OPSERIAL", opnt.OpSerial),
                new OracleParameter("P_OPNAMEID", opnt.OpNameId),
                new OracleParameter("P_EDITION", opnt.Edition),
                new OracleParameter("P_OPTIME", opnt.OpTime),
                new OracleParameter("P_OPNSERIAL", opnt.OpnSerial),
                new OracleParameter("P_MACHINETYPE", opnt.MachineType),
                new OracleParameter("P_MACHINECOUNT", opnt.MachineCount),
                new OracleParameter("P_REMARKS", opnt.Remarks),
                new OracleParameter("P_MAXTIME", opnt.MaxTime),
                new OracleParameter("P_MANCOUNT", opnt.ManCount),
                new OracleParameter("P_IMAGENAME", opnt.ImageName),
                new OracleParameter("P_VIDEOFILE", opnt.VideoFile),
                new OracleParameter("P_JOBTYPE", opnt.JobType),
                new OracleParameter("P_TOOLID", opnt.ToolId),
                new OracleParameter("P_ACTIONCODE", opnt.ActionCode),
                new OracleParameter("P_STITCHCOUNT", opnt.StitchCount),
                new OracleParameter("P_MAINPROCESS", opnt.MainProcess),
                new OracleParameter("P_STITCHINGLENGTH", opnt.StitchingLength),
                new OracleParameter("P_STITCHESPERINCH", opnt.StitchesPerInch),
                new OracleParameter("P_IOTTYPE", opnt.IotType),
                new OracleParameter("P_GROUPLEVEL_0", opnt.GroupLevel_0),
                new OracleParameter("P_GROUPLEVEL_1", opnt.GroupLevel_1),
                new OracleParameter("P_GROUPLEVEL_2", opnt.GroupLevel_2)
            };

            var resIns = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text, trans, oraConn);

            return resIns != null;
        }

        /// <summary>
        /// Delete operation name detail
        /// </summary>
        /// <param name="opnt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteOpNameDetail(Opdt opdt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opdt.OpSerial),
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) { Direction = ParameterDirection.Output }
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELETEBYOPDT_OPNT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null;
        }

        /// <summary>
        /// Delete process name detail by operatio master
        /// </summary>
        /// <param name="opmt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool DeleteOpNameDetailByOpmt(Opmt opmt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            oracleParams.Insert(0, new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) { Direction = ParameterDirection.Output });
            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELETEBYOPMT_OPNT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null; // && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// Update process time.
        /// </summary>
        /// <param name="opnt"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool UpdateProcessTime(Opnt opnt, OracleConnection oraConn, OracleTransaction trans)
        {
            var oracleParams = new OpsOracleParams(opnt.Edition, opnt.StyleCode, opnt.StyleSize, opnt.StyleColorSerial, opnt.RevNo, opnt.OpRevNo, opnt.OpSerial) {
                new OracleParameter("P_OPNAMEID", opnt.OpNameId),
                new OracleParameter("P_OPTIME", opnt.OpTime)
            };
            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEPROCESSTIME_OPNT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// Update list of process time.
        /// </summary>
        /// <param name="lstOpnt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool UpdateListProcessTime(List<Opnt> lstOpnt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {

                    foreach (var opnt in lstOpnt)
                    {
                        if (!UpdateProcessTime(opnt, connection, trans))
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
        /// 
        /// </summary>
        /// <returns></returns>
        /// Author: Ha Nguyen Thi Ngoc
        public static List<Opnt> GetListMachineType()
        {
            string strSql = $"select t_op_otmt.itemname MachineType from t_op_otmt join t_op_opnt on t_op_opnt.machinetype = t_op_otmt.itemcode";

            var lstOpnt = OracleDbManager.GetObjects<Opnt>(strSql, null);

            return lstOpnt;
        }

        /// <summary>
        /// Insert list of process name detail
        /// </summary>
        /// <param name="lstOpnt"></param>
        /// <returns></returns>
        public static bool InsertListOpnt(List<Opnt> lstOpnt)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {

                    foreach (var opnt in lstOpnt)
                    {
                        if (!InsertOpNameDetail(opnt, connection, trans))
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
        ///Update image name 
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <param name="opNameId"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateImageOrVideoName(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string opNameId, string edition, bool isImage)
        {
            //Check primary key before updating
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(opRevNo) ||
                string.IsNullOrEmpty(opSerial) || string.IsNullOrEmpty(opNameId) || string.IsNullOrEmpty(edition)) return false;

            string fileName = isImage ? "imagename" : "videofile";

            string strSql = $@"update t_op_opnt set {fileName} = '' where stylecode = :P_STYLECODE and stylesize = :P_STYLESIZE and stylecolorserial = :P_STYLECOLORSERIAL and revno = :P_REVNO 
                                        and oprevno = :P_OPREVNO and opserial = :P_OPSERIAL and opnameid = :P_OPNAMEID and edition = :P_EIDTION";

            var oracleParams = new List<OracleParameter>() {
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo),
                new OracleParameter("P_OPSERIAL", opSerial),
                new OracleParameter("P_OPNAMEID", opNameId),
                new OracleParameter("P_EIDTION", edition)
            };
            var resUdp = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text);

            return resUdp != null && int.Parse(resUdp.ToString()) != 0;
        }
        #endregion

        #region MySql database

        /// <summary>
        /// Gets the by opdt and language.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial">The op serial.</param>
        /// <param name="opNameId">The op name identifier.</param>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Opnt> GetByOpdtAndLang(string edition, string styleCode, string styleSize,
            string styleColorSerial, string revNo, string opRevNo, string opSerial, string opNameId, string languageId)
        {
            var mySqlParams = new OpsMySqlParams(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial)
            {
                new MySqlParameter("P_OPNAMEID", opNameId),
                new MySqlParameter("P_LANGUAGEID", languageId)
            };

            var opnts = MySqlDBManager.GetAll<Opnt>("SP_MES_GETBYOPDTANDLANG_OPNT", CommandType.StoredProcedure,
                mySqlParams.ToArray());

            return opnts;
        }

        /// <summary>
        /// Inserts the opnt.
        /// </summary>
        /// <param name="opnt">The opnt.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool InsertOpnt(Opnt opnt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var ps = new OpsMySqlParams(opnt.Edition, opnt.StyleCode, opnt.StyleSize, opnt.StyleColorSerial,
                opnt.RevNo, opnt.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opnt.OpSerial),
                new MySqlParameter("P_OPNAMEID", opnt.OpNameId),
                new MySqlParameter("P_OPTIME", opnt.OpTime),
                new MySqlParameter("P_OPNSERIAL", opnt.OpnSerial),
                new MySqlParameter("P_MACHINETYPE", opnt.MachineType),
                new MySqlParameter("P_MACHINECOUNT", opnt.MachineCount),
                new MySqlParameter("P_REMARKS", opnt.Remarks),
                new MySqlParameter("P_MAXTIME", opnt.MaxTime),
                new MySqlParameter("P_MANCOUNT", opnt.ManCount),
                new MySqlParameter("P_JOBTYPE", opnt.JobType),
                new MySqlParameter("P_TOOLID", opnt.ToolId),
                new MySqlParameter("P_ACTIONCODE", opnt.ActionCode),
                new MySqlParameter("P_AFFECTEDROWS", MySqlDbType.Int16) { Direction = ParameterDirection.Output }
            };

            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_INSERT_OPNT", ps.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        /// <summary>
        /// Inserts list of process names.
        /// This function is based on InsertOpnt.
        /// </summary>
        /// <param name="opnts">The list of opnts.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public async Task<bool> BulkInsertOpntAsync(List<Opnt> opnts)
        {
            var qValue = "";

            for (var i = 0; i < opnts.Count; i++)
            {
                qValue += $"('{opnts[i].StyleCode}','{opnts[i].StyleSize}','{opnts[i].StyleColorSerial}'," +
                          $"'{opnts[i].RevNo}','{opnts[i].OpRevNo}','{opnts[i].OpSerial}','{opnts[i].OpNameId}'," +
                          $"'{opnts[i].Edition}','{opnts[i].OpTime}','{opnts[i].OpnSerial}','{opnts[i].MachineType}'," +
                          $"'{opnts[i].MachineCount}','{opnts[i].Remarks}','{opnts[i].MaxTime}','{opnts[i].ManCount}'," +
                          $"'{opnts[i].ImageName}','{opnts[i].VideoFile}','{opnts[i].JobType}','{opnts[i].ToolId}'," +
                          $"'{opnts[i].ActionCode}')";

                if (i != opnts.Count - 1) qValue += ",";
            }

            var q = $@"INSERT INTO `mes`.`t_op_opnt`
                            (`STYLECODE`,
                            `STYLESIZE`,
                            `STYLECOLORSERIAL`,
                            `REVNO`,
                            `OPREVNO`,
                            `OPSERIAL`,
                            `OPNAMEID`,
                            `EDITION`,
                            `OPTIME`,
                            `OPNSERIAL`,
                            `MACHINETYPE`,                            
                            `MACHINECOUNT`,
                            `REMARKS`,
                            `MAXTIME`,
                            `MANCOUNT`,                           
                            `IMAGENAME`, 
                            `VIDEOFILE`,
                            `JOBTYPE`,
                            `TOOLID`,
                            `ACTIONCODE`)
                        VALUES {qValue};";
            var rs = await _mySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, q, CommandType.Text);

            return rs;
        }

        /// <summary>
        /// Deletes the operation name.
        /// </summary>
        /// <param name="opdt">The operation detail.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool DeleteOpnt(Opdt opdt, MySqlConnection oraConn, MySqlTransaction trans)
        {
            var oracleParams = new OpsMySqlParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                opdt.RevNo, opdt.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", opdt.OpSerial)
            };

            var affectedRow = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_DELETEBYOPDT_OPNT", oracleParams.ToArray(),
                CommandType.StoredProcedure, trans, oraConn);

            return affectedRow != null;
        }

        public static bool DeleteByOpmt(Opmt opmt, MySqlConnection connection, MySqlTransaction transaction)
        {
            var prs = new OpsMySqlParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);
            var resDel = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_DELETEBYOPMT_OPNT", prs.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return resDel != null;
        }

        /// <summary>
        /// Gets the by operation master.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 25-Jul-19
        public static List<Opnt> GetByOpmt(Opmt opmt)
        {
            var mySqlParams = new OpsMySqlParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo,
                opmt.OpRevNo);
            var opnts = MySqlDBManager.GetAll<Opnt>("SP_MES_GETBYOPMT_OPNT", CommandType.StoredProcedure,
                mySqlParams.ToArray());

            return opnts;
        }

        #endregion
    }
}
