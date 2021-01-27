using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OPS_DAL.Business
{
    public class OpflBus
    {
        #region Oracle database

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opfile"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static List<FileSd> GetOperationFiles(Opfl opfile)
        {
            var oracleParams = new OpsOracleParams(opfile.StyleCode, opfile.StyleSize, opfile.StyleColorSerial, opfile.RevNo, opfile.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opfile.OpSerial),
                new OracleParameter("P_EDITION", opfile.Edition),
                new OracleParameter("P_UPLOADCODE", opfile.UploadCode),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };

            var lstOpFile = OracleDbManager.GetObjects<FileSd>("SP_OPS_GETOPFILES_OPFL", CommandType.StoredProcedure, oracleParams.ToArray());

            return lstOpFile;
        }

        /// <summary>
        /// Get list of file by operation plan
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Opfl> GetOperationFilesByPlan(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {

            string strSql = @" SELECT * 
                               FROM T_OP_OPFL 
                               WHERE STYLECODE  = :P_STYLECODE and STYLESIZE = :P_STYLESIZE 
                                        AND (STYLECOLORSERIAL = :P_STYLECOLORSERIAL OR STYLECOLORSERIAL = '000') 
                                        AND (REVNO = '000' OR REVNO  = :P_REVNO) AND OPREVNO = :P_OPREVNO AND EDITION = :P_EDITION  ";

            var oracleParams = new OpsOracleParams(styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_EDITION", edition)
            };

            var lstOpFile = OracleDbManager.GetObjects<Opfl>(strSql, CommandType.Text, oracleParams.ToArray());

            return lstOpFile;
        }

        /// <summary>
        /// Get max amendno of operation file.
        /// </summary>
        /// <param name="opfile"></param>
        /// <returns></returns>
        public static string GetMaxAmendNoOpFile(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string edition, string uploadCode)
        {
            var oracleParams = new OpsOracleParams(styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_OPSERIAL", opSerial),
                new OracleParameter("P_EDITION", edition),
                new OracleParameter("P_UPLOADCODE", uploadCode),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction = ParameterDirection.Output}
            };

            var opfl = OracleDbManager.GetObjects<Opfl>("SP_OPS_GETMAXAMENDNO_OPFL", CommandType.StoredProcedure, oracleParams.ToArray()).FirstOrDefault();
            var maxAmendNo = "001";
            if (opfl.AmendNo != null)
            {
                maxAmendNo = (int.Parse(opfl.AmendNo) + 1).ToString("D3");
            }

            return maxAmendNo;
        }


        /// <summary>
        /// Insert process file.
        /// </summary>
        /// <param name="opfile"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertProcessFile(Opfl opfile)
        {
            var oracleParams = new OpsOracleParams(opfile.StyleCode, opfile.StyleSize, opfile.StyleColorSerial, opfile.RevNo, opfile.OpRevNo)
            {
                new OracleParameter("P_OPSERIAL", opfile.OpSerial),
                new OracleParameter("P_EDITION", opfile.Edition),
                new OracleParameter("P_UPLOADCODE", opfile.UploadCode),
                new OracleParameter("P_AMENDNO", opfile.AmendNo),
                new OracleParameter("P_REGISTER", opfile.Register),
                new OracleParameter("P_SYSFILENAME", opfile.SysFileName),
                new OracleParameter("P_ORGFILENAME", opfile.OrgFileName),
                new OracleParameter("P_SOURCEFILE", opfile.SourceFile),
                new OracleParameter("P_REFLINK", opfile.RefLink)
            };
            oracleParams.Insert(0, new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) { Direction = ParameterDirection.Output });

            var resInsert = OracleDbManager.ExecuteQuery("SP_OPS_INSERTFILE_OPFL", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resInsert != null;
        }

        /// <summary>
        /// Delete files linking
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <param name="edition"></param>
        /// <param name="uploadCode"></param>
        /// <param name="amendNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteFilesLinking(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string edition, string uploadCode, string amendNo)
        {
            var oracleParams = new OpsOracleParams(styleCode, styleSize, styleColorSerial, revNo, opRevNo)
            {
                new OracleParameter("P_OPSERIAL", opSerial),
                new OracleParameter("P_EDITION", edition),
                new OracleParameter("P_UPLOADCODE", uploadCode),
                new OracleParameter("P_AMENDNO", amendNo),
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) {Direction = ParameterDirection.Output}
            };

            var resInsert = OracleDbManager.ExecuteQuery("SP_OPS_DELETEFILESLINKING_OPFL", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resInsert != null;
        }

        /// <summary>
        /// Gets the opfls by operation plan.
        /// </summary>
        /// <param name="opmt">The operation master.</param>
        /// <returns>List of Opfls</returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Opfl> GetOpflsByOp(Opmt opmt)
        {
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);
            oracleParams.AddCursor();

            var opfls = OracleDbManager.GetObjects<Opfl>("SP_OPS_GETBYOP_OPFL", CommandType.StoredProcedure,
                oracleParams.ToArray());
            return opfls;
        }

        #endregion

        #region MySql database

        /// <summary>
        /// Gets the by operation master.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 05-Jul-19
        public static List<Opfl> GetByOp(Opmt opmt)
        {
            var prs = new OpsMySqlParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);
            var opfls = MySqlDBManager.GetAll<Opfl>("SP_MES_GETBYOP_OPFL", CommandType.StoredProcedure,
                prs.ToArray());

            return opfls;
        }

        #endregion
    }
}
