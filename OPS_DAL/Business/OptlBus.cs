using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace OPS_DAL.Business
{
    public class OptlBus
    {
        #region General

        public static List<Optl> GetByStyle(string styleCode, string styleSize, string styleColor, string revNo,
            string opRevNo, string edition, int sourceDb)
        {
            switch (sourceDb)
            {
                case 1:
                    return GetByStyle(styleCode, styleSize, styleColor, revNo, opRevNo, edition);
                case 2:
                    return GetListToolLinking(styleCode, styleSize, styleColor, revNo, opRevNo, edition);
                default:
                    return null;
            }
        }

        #endregion

        #region Oracle database 

        /// <summary>
        /// Adds the tool linking.
        /// </summary>
        /// <param name="opToolLinking">The op tool linking.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <param name="trans">The trans.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool AddToolLinking_New(Optl opToolLinking, OracleConnection oraConn, OracleTransaction trans)
        {
            var tblName = CommonMethod.GetTableNameOptlByEdition(opToolLinking.Edition);
            string strSql = $@"INSERT INTO {tblName} (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, OPREVNO, OPSERIAL, ITEMCODE, MACHINE, EDITION, MAINTOOL )
                                VALUES (:STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :OPREVNO, :OPSERIAL, :ITEMCODE, :MACHINE, :EDITION, :MAINTOOL ) ";

            var prams = new List<OracleParameter>() {
                new OracleParameter("STYLECODE", opToolLinking.StyleCode),
                new OracleParameter("STYLESIZE", opToolLinking.StyleSize),
                new OracleParameter("STYLECOLORSERIAL", opToolLinking.StyleColorSerial),
                new OracleParameter("REVNO", opToolLinking.RevNo),
                new OracleParameter("OPREVNO", opToolLinking.OpRevNo),
                new OracleParameter("OPSERIAL", opToolLinking.OpSerial),
                new OracleParameter("ITEMCODE", opToolLinking.ItemCode),
                new OracleParameter("MACHINE", opToolLinking.Machine),
                new OracleParameter("EDITION", opToolLinking.Edition),
                new OracleParameter("MAINTOOL", opToolLinking.MainTool)
            };

            var result = OracleDbManager.ExecuteQuery(strSql, prams.ToArray(), CommandType.Text, trans, oraConn);

            return result != null;
        }

        public static bool AddToolLinking(Optl opToolLinking, OracleConnection oraConn, OracleTransaction trans)
        {
            //START MOD) SON - 11/Jun/2019
            //var sb = new StringBuilder();
            //sb.AppendLine(" INSERT INTO ");
            //sb.AppendLine(" T_OP_OPTL (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, OPREVNO, OPSERIAL, ITEMCODE, MACHINE, EDITION, MAINTOOL ) ");
            //sb.AppendLine(" VALUES (:STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :OPREVNO, :OPSERIAL, :ITEMCODE, :MACHINE, :EDITION, :MAINTOOL ) ");

            var tblName = CommonMethod.GetTableNameOptlByEdition(opToolLinking.Edition);
            string strSql = $"INSERT INTO {tblName} (STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, OPREVNO, OPSERIAL, ITEMCODE, MACHINE, EDITION, MAINTOOL )" +
                            $" VALUES (:STYLECODE, :STYLESIZE, :STYLECOLORSERIAL, :REVNO, :OPREVNO, :OPSERIAL, :ITEMCODE, :MACHINE, :EDITION, :MAINTOOL ) ";
            //END MOD) SON - 11/Jun/2019

            var prams = new OracleParameter[10];
            prams[0] = new OracleParameter("STYLECODE", opToolLinking.StyleCode);
            prams[1] = new OracleParameter("STYLESIZE", opToolLinking.StyleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", opToolLinking.StyleColorSerial);
            prams[3] = new OracleParameter("REVNO", opToolLinking.RevNo);
            prams[4] = new OracleParameter("OPREVNO", opToolLinking.OpRevNo);
            prams[5] = new OracleParameter("OPSERIAL", opToolLinking.OpSerial);
            prams[6] = new OracleParameter("ITEMCODE", opToolLinking.ItemCode);
            prams[7] = new OracleParameter("MACHINE", opToolLinking.Machine);
            prams[8] = new OracleParameter("EDITION", opToolLinking.Edition);
            prams[9] = new OracleParameter("MAINTOOL", opToolLinking.MainTool);

            var result = OracleDbManager.ExecuteQuery(strSql, prams, CommandType.Text, trans, oraConn);

            return result != null;
        }

        /// <summary>
        ///     Deletes the ops tool linking.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <param name="trans">The trans.</param>
        /// <param name="oraConn">The ora connection.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteOpsToolLinking(Opdt opDetail, OracleTransaction trans, OracleConnection oraConn)
        {
            var oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_EDITION", opDetail.Edition),
                new OracleParameter("P_STYLECODE", opDetail.StyleCode),
                new OracleParameter("P_STYLESIZE", opDetail.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opDetail.StyleColorSerial),
                new OracleParameter("P_REVNO", opDetail.RevNo),
                new OracleParameter("P_OPREVNO", opDetail.OpRevNo),
                new OracleParameter("P_OPSERIAL", opDetail.OpSerial)
            };

            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELTOOLLINKING_OPTL", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null; //&& int.Parse(resDel.ToString()) != 0;
        }

        /// <summary>
        /// Delete tool by operation maser code.
        /// </summary>
        /// <param name="opmt"></param>
        /// <param name="trans"></param>
        /// <param name="oraConn"></param>
        /// <returns></returns>
        public static bool DeleteOpsToolByOpmt(Opmt opmt, OracleTransaction trans, OracleConnection oraConn)
        {
            //START MOD) SON - 11/Jun/2019 - add edition to delete tool linking by operation master key
            //var oracleParams = new OpsOracleParams(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            var oracleParams = new OpsOracleParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);
            //END MOD) SON - 11/Jun/2019
            oracleParams.Insert(0, new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16) { Direction = ParameterDirection.Output });
            var resDel = OracleDbManager.ExecuteQuery("SP_OPS_DELETETOOLBYOPMT_OPTL", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resDel != null;// && int.Parse(resDel.ToString()) != 0;

        }

        /// <summary>
        /// Add Main
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// author: VitHV
        public static bool AddMain(Optl tool, bool istool)
        {
            RemoveMain(tool, istool);

            //START MOD) SON - 2/Jul/2019 - get list tool by edition
            //var sql = @" UPDATE T_OP_OPTL SET MAINTOOL = '1' 
            //             WHERE  STYLECODE = :STYLECODE
            //                    AND STYLESIZE = :STYLESIZE
            //                    AND STYLECOLORSERIAL = :STYLECOLORSERIAL
            //                    AND REVNO = :REVNO 
            //                    AND OPREVNO = :OPREVNO
            //                    AND OPSERIAL = :OPSERIAL
            //                    AND ITEMCODE = :ITEMCODE AND EDITION = :EDITION ";

            var tblName = CommonMethod.GetTableNameOptlByEdition(tool.Edition);

            var sql = $" UPDATE {tblName} SET MAINTOOL = '1' WHERE  STYLECODE = :STYLECODE AND STYLESIZE = :STYLESIZE ";
            sql += @" AND STYLECOLORSERIAL = :STYLECOLORSERIAL
                                AND REVNO = :REVNO 
                                AND OPREVNO = :OPREVNO
                                AND OPSERIAL = :OPSERIAL
                                AND ITEMCODE = :ITEMCODE AND EDITION = :EDITION ";
            //START MOD) SON - 2/Jul/2019

            var prams = new OpsParams(tool.StyleCode, tool.StyleSize, tool.StyleColorSerial
                         , tool.RevNo, tool.OpRevNo, tool.OpSerial, tool.ItemCode)
            { new OracleParameter("EDITION", tool.Edition)};
            prams.ReplacePbyEmpty();
            var result = OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return result != null;
        }

        /// <summary>
        /// Remove all main of opdt Main
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// author: VitHV
        static bool RemoveMain(Optl tool, bool isTool)
        {
            string strwhere = "";
            if (isTool)
            {
                strwhere = " AND (MACHINE IS NULL OR MACHINE = '0' )";
            }
            else
            {
                strwhere = " AND MACHINE = '1' ";
            }
            //START MOD) SON - 2/Jul/2019 - check table tool linking by edition
            //var sql = @" UPDATE T_OP_OPTL SET MAINTOOL = '0' 
            //             WHERE  STYLECODE = :STYLECODE
            //                    AND STYLESIZE = :STYLESIZE
            //                    AND STYLECOLORSERIAL = :STYLECOLORSERIAL
            //                    AND REVNO = :REVNO 
            //                    AND OPREVNO = :OPREVNO
            //                    AND OPSERIAL = :OPSERIAL
            //                    AND EDITION = :EDITION " + strwhere;

            var tblName = CommonMethod.GetTableNameOptlByEdition(tool.Edition);

            var sql = $" UPDATE {tblName} SET MAINTOOL = '0' WHERE STYLECODE = :STYLECODE AND STYLESIZE = :STYLESIZE";
            sql += @" AND STYLECOLORSERIAL = :STYLECOLORSERIAL
                                AND REVNO = :REVNO 
                                AND OPREVNO = :OPREVNO
                                AND OPSERIAL = :OPSERIAL
                                AND EDITION = :EDITION " + strwhere;
            //START MOD) SON - 2/Jul/2019

            var prams = new OpsParams(tool.StyleCode, tool.StyleSize, tool.StyleColorSerial
                         , tool.RevNo, tool.OpRevNo, tool.OpSerial)
            { new OracleParameter("EDITION", tool.Edition)};
            prams.ReplacePbyEmpty();
            var result = OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return result != null;
        }

        /// <summary>
        /// Remove ToolLinking
        /// </summary>
        /// <param name="opToolLinking"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        /// author: VitHV
        public static bool RemoveToolLinking(Optl tool, OracleConnection oraConn, OracleTransaction trans)
        {
            var tblName = CommonMethod.GetTableNameOptlByEdition(tool.Edition);

            var sb = new StringBuilder();
            //START MOD) SON - 2/Jul/2019 - check table tool linking before delete
            //sb.AppendLine(" DELETE ");
            //sb.AppendLine(@" T_OP_OPTL WHERE STYLECODE = :STYLECODE
            //                                 AND STYLESIZE = :STYLESIZE
            //                                 AND STYLECOLORSERIAL = :STYLECOLORSERIAL
            //                                 AND REVNO = :REVNO 
            //                                 AND OPREVNO = :OPREVNO
            //                                 AND OPSERIAL = :OPSERIAL
            //                                 AND ITEMCODE = :ITEMCODE AND (EDITION = :EDITION OR EDITION IS NULL) ");

            sb.AppendLine(" DELETE ");
            sb.AppendLine($" {tblName} WHERE STYLECODE = :STYLECODE ");
            sb.AppendLine(@" AND STYLESIZE = :STYLESIZE 
                                             AND STYLECOLORSERIAL = :STYLECOLORSERIAL
                                             AND REVNO = :REVNO 
                                             AND OPREVNO = :OPREVNO
                                             AND OPSERIAL = :OPSERIAL
                                             AND ITEMCODE = :ITEMCODE AND (EDITION = :EDITION OR EDITION IS NULL) ");

            //END MOD) SON - 2/Jul/2019
            var prams = new OpsParams(tool.StyleCode, tool.StyleSize, tool.StyleColorSerial
                         , tool.RevNo, tool.OpRevNo, tool.OpSerial, tool.ItemCode)
            { new OracleParameter("EDITION", tool.Edition) };
            prams.ReplacePbyEmpty();
            var result = OracleDbManager.ExecuteQuery(sb.ToString(), prams.ToArray(), CommandType.Text, trans, oraConn);
            return result != null;
        }

        /// <summary>
        /// Adds the new List Prot proccess.
        /// </summary>
        /// <param name="optl">The optl.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool AddNewTools(List<Optl> optl)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add list tool Prot
                    foreach (var item in optl)
                    {
                        AddToolLinking(item, connection, trans);
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Removes the proccess.
        /// </summary>
        /// <param name="optl">The optl.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static bool RemoveTools(List<Optl> optl)
        {
            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    //Add list tool Prot
                    foreach (var item in optl)
                    {
                        RemoveToolLinking(item, connection, trans);
                    }
                    trans.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    trans.Rollback();
                    return false;
                }

            }
        }

        /// <summary>
        /// Author: Son Nguyen Cao
        /// Date: 2 Aug 2017
        /// Get list Prot
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns>List Prot</returns>
        public static List<Optl> GetListToolLinking(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            var tblNameOptl = CommonMethod.GetTableNameOptlByEdition(edition); //ADD) SON - 19/Jun/2019

            var sb = new StringBuilder();
            sb.AppendLine(" SELECT ");
            sb.AppendLine("     OPT.STYLECODE, OPT.STYLESIZE, OPT.STYLECOLORSERIAL, OPT.REVNO, OPT.OPREVNO ");
            sb.AppendLine("      , OPT.OPSERIAL, OPT.ITEMCODE, OPT.MACHINE, OPT.EDITION, OPT.MAINTOOL ");
            sb.AppendLine(" FROM ");
            sb.AppendLine($"     {tblNameOptl} OPT  "); //MOD) SON - 19/Jun/2019
            sb.AppendLine(" WHERE ");
            sb.AppendLine("      OPT.STYLECODE = :STYLECODE AND OPT.STYLESIZE = :STYLESIZE ");
            sb.AppendLine("      AND OPT.STYLECOLORSERIAL = :STYLECOLORSERIAL AND OPT.REVNO = :REVNO ");
            sb.AppendLine("      AND OPT.OPREVNO = :OPREVNO AND OPT.EDITION = :EDITION ");

            OracleParameter[] prams = new OracleParameter[6];
            prams[0] = new OracleParameter("STYLECODE", styleCode);
            prams[1] = new OracleParameter("STYLESIZE", styleSize);
            prams[2] = new OracleParameter("STYLECOLORSERIAL", styleColor);
            prams[3] = new OracleParameter("REVNO", revNo);
            prams[4] = new OracleParameter("OPREVNO", opRevNo);
            prams[5] = new OracleParameter("EDITION", edition);

            var lstTool = OracleDbManager.GetObjects<Optl>(sb.ToString(), prams);

            return lstTool;
        }

        //START ADD) 05/Mar/2019: get list of tool and machines of process with standard name
        /// <summary>
        /// Get list of tool and machine of process which has standard name.
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Nguyen Cao Son
        public static List<Optl> GetListToolLinkingWithStandardName(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string edition)
        {
            //string strSql = @"SELECT DISTINCT TL.STYLECODE, TL.STYLESIZE, TL.STYLECOLORSERIAL, TL.REVNO, TL.OPREVNO,TL.EDITION, TL.OPSERIAL, TL.ITEMCODE, TL.MACHINE, TL.EDITION, TL.MAINTOOL
            //                    FROM T_OP_OPTL TL 
            //                        JOIN T_OP_OPNT NT ON NT.STYLECODE = TL.STYLECODE AND NT.STYLESIZE = TL.STYLESIZE AND NT.STYLECOLORSERIAL = TL.STYLECOLORSERIAL 
            //                                AND NT.REVNO = NT.REVNO AND NT.OPREVNO = TL.OPREVNO AND NT.EDITION = TL.EDITION AND TL.OPSERIAL = NT.OPSERIAL 
            //                WHERE TL.STYLECODE = :P_STYLECODE AND TL.STYLESIZE = :P_STYLESIZE  AND TL.STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND TL.REVNO = :P_REVNO AND TL.OPREVNO = :P_OPREVNO AND TL.EDITION = :P_EDITION ";

            var tblNameOptl = CommonMethod.GetTableNameOptlByEdition(edition);

            var tblNameOpnt = CommonMethod.GetTableNameOPNTByEdition(edition);

            string strSql = $"SELECT DISTINCT TL.STYLECODE, TL.STYLESIZE, TL.STYLECOLORSERIAL, TL.REVNO, TL.OPREVNO,TL.EDITION, TL.OPSERIAL, TL.ITEMCODE, TL.MACHINE, TL.EDITION, TL.MAINTOOL " +
                            $" FROM {tblNameOptl} TL " +
                                    $" JOIN {tblNameOpnt} NT ON NT.STYLECODE = TL.STYLECODE AND NT.STYLESIZE = TL.STYLESIZE AND NT.STYLECOLORSERIAL = TL.STYLECOLORSERIAL " +
                                     $" AND NT.REVNO = NT.REVNO AND NT.OPREVNO = TL.OPREVNO AND NT.EDITION = TL.EDITION AND TL.OPSERIAL = NT.OPSERIAL " +
                            $"WHERE TL.STYLECODE = :P_STYLECODE AND TL.STYLESIZE = :P_STYLESIZE  AND TL.STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND TL.REVNO = :P_REVNO AND TL.OPREVNO = :P_OPREVNO AND TL.EDITION = :P_EDITION ";


            OracleParameter[] prams = new OracleParameter[6];
            prams[0] = new OracleParameter("P_STYLECODE", styleCode);
            prams[1] = new OracleParameter("P_STYLESIZE", styleSize);
            prams[2] = new OracleParameter("P_STYLECOLORSERIAL", styleColor);
            prams[3] = new OracleParameter("P_REVNO", revNo);
            prams[4] = new OracleParameter("P_OPREVNO", opRevNo);
            prams[5] = new OracleParameter("P_EDITION", edition);

            var lstTool = OracleDbManager.GetObjects<Optl>(strSql.ToString(), prams);

            return lstTool;

        }

        /// <summary>
        /// Author: VITHV
        /// Date: 2 Aug 2017
        /// Get list Prot
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="isMachine">1 Machine, 0 tools.</param>
        /// <returns>List Prot</returns>
        public static List<Optl> GetListOptl(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, int isMachine)
        {
            string strWhere = "";
            if (isMachine == 1)
            {
                strWhere = " AND OPT.MACHINE = '1'";
            }
            else
            {
                strWhere = " AND (OPT.MACHINE = '0' OR OPT.MACHINE IS NULL) ";
            }
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT ");
            sb.AppendLine("     OPT.STYLECODE, OPT.STYLESIZE, OPT.STYLECOLORSERIAL, OPT.REVNO, OPT.OPREVNO ");
            sb.AppendLine("      , OPT.OPSERIAL, OPT.ITEMCODE, OPT.MACHINE, OPT.EDITION ");
            sb.AppendLine(" FROM ");
            sb.AppendLine("     T_OP_OPTL OPT  ");
            sb.AppendLine(" WHERE ");
            sb.AppendLine("      OPT.STYLECODE = :STYLECODE AND OPT.STYLESIZE = :STYLESIZE ");
            sb.AppendLine(@"      AND OPT.STYLECOLORSERIAL = :STYLECOLORSERIAL AND OPT.REVNO = :REVNO 
                                  AND OPT.OPREVNO = :OPREVNO " + strWhere);
            var prams = new OpsParams(styleCode, styleSize, styleColor, revNo, opRevNo);
            prams.ReplacePbyEmpty();
            var lstTool = OracleDbManager.GetObjects<Optl>(sb.ToString(), prams.ToArray());

            return lstTool;

        }

        /// <summary>
        /// Get list tool linking by edition
        /// </summary>
        /// <param name="eidtion"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColor"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="isMachine"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Optl> GetListOptlByEdition(string edition, string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, int isMachine)
        {
            var tblToolLinkingName = CommonMethod.GetTableNameOptlByEdition(edition);

            string strWhere = "";
            if (isMachine == 1)
            {
                strWhere = " AND OPT.MACHINE = '1'";
            }
            else
            {
                strWhere = " AND (OPT.MACHINE = '0' OR OPT.MACHINE IS NULL) ";
            }
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT ");
            sb.AppendLine("     OPT.STYLECODE, OPT.STYLESIZE, OPT.STYLECOLORSERIAL, OPT.REVNO, OPT.OPREVNO ");
            sb.AppendLine("      , OPT.OPSERIAL, OPT.ITEMCODE, OPT.MACHINE, OPT.EDITION ");
            sb.AppendLine(" FROM ");
            sb.AppendLine($"     {tblToolLinkingName} OPT  ");
            sb.AppendLine(" WHERE ");
            sb.AppendLine("      OPT.STYLECODE = :STYLECODE AND OPT.STYLESIZE = :STYLESIZE ");
            sb.AppendLine(@"      AND OPT.STYLECOLORSERIAL = :STYLECOLORSERIAL AND OPT.REVNO = :REVNO 
                                  AND OPT.OPREVNO = :OPREVNO AND OPT.EDITION =  :EDITION " + strWhere);
            var prams = new OpsParams(styleCode, styleSize, styleColor, revNo, opRevNo)
            {
                new OracleParameter("EDITION", edition)
            };

            prams.ReplacePbyEmpty();

            var lstTool = OracleDbManager.GetObjects<Optl>(sb.ToString(), prams.ToArray());

            return lstTool;

        }
        /// <summary>
        /// Gets the tool linking by code.
        /// </summary>
        /// <param name="opTool">The op detail.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Optl> GetToolLinkingByCode(Optl opTool)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {

                new OracleParameter("P_EDITION", opTool.Edition), //ADD) SON - 12/Jun/2019 - add condition edition to get tool linking
                new OracleParameter("P_STYLECODE", opTool.StyleCode),
                new OracleParameter("P_STYLESIZE", opTool.StyleSize),
                new OracleParameter("P_STYLECOLORSERIAL", opTool.StyleColorSerial),
                new OracleParameter("P_REVNO", opTool.RevNo),
                new OracleParameter("P_OPREVNO", opTool.OpRevNo),
                new OracleParameter("P_OPSERIAL", opTool.OpSerial),
                new OracleParameter("P_ITEMCODE", opTool.ItemCode),
                cursor
            };

            //var lstTool = OracleDbManager.GetObjects<Optl>("SP_OPS_GETTOOLBYCODE_OPTL", CommandType.StoredProcedure, oracleParams.ToArray());
            var lstTool = OracleDbManager.GetObjects<Optl>("SP_OPS_GETTOOLLINKING_OPTL", CommandType.StoredProcedure, oracleParams.ToArray());
            return lstTool;
        }

        //START ADD: HA
        public static List<Optl> GetMachineJquery(string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo, string edition)
        {
            var strSql = @"SELECT OTMT.IMAGEPATH, OTMT.ITEMCODE, OTMT.ITEMNAME, OTMT.CATEGID, MCMT.CODE_NAME AS MACHINETYPE FROM T_OP_OPTL OPTL 
                    LEFT JOIN T_OP_OTMT OTMT ON OTMT.ITEMCODE = OPTL.ITEMCODE
                    LEFT JOIN T_CM_MCMT MCMT ON trim(OTMT.CATEGID) = trim(MCMT.S_CODE)
                    WHERE OTMT.ITEMCODE LIKE 'M%' 
                    AND MCMT.M_CODE IN('NonSewingMc','SewingMc')
                    AND OPTL.STYLECODE = :P_STYLECODE 
                    AND OPTL.STYLESIZE = :P_STYLESIZE
                    AND OPTL.STYLECOLORSERIAL = :P_STYLECOLORSERIAL 
                    AND OPTL.REVNO = :P_REVNO 
                    AND OPTL.OPREVNO = :P_OPREVNO 
                    AND OPTL.EDITION = :P_EDITION";

            var oracleParams = new List<OracleParameter> {

                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLESIZE", styleSize),
                new OracleParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new OracleParameter("P_REVNO", revNo),
                new OracleParameter("P_OPREVNO", opRevNo),
                new OracleParameter("P_EDITION", edition),
            };

            return OracleDbManager.GetObjects<Optl>(strSql, oracleParams.ToArray());
        }
        //END ADD: HA

        #endregion

        #region MySql database

        /// <summary>
        /// Gets the by style.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">Color of the style.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="edition">The edition.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Optl> GetByStyle(string styleCode, string styleSize, string styleColor, string revNo,
            string opRevNo, string edition)
        {
            var mySqlParams = new OpsMySqlParams(edition, styleCode, styleSize, styleColor, revNo, opRevNo);
            var optls = MySqlDBManager.GetAll<Optl>("SP_MES_GETBYSTYLE_OPTL", CommandType.StoredProcedure,
                mySqlParams.ToArray());

            return optls;
        }

        public static List<Optl> GetTools(Optl optl)
        {
            var mySqlParams = new OpsMySqlParams(optl.StyleCode, optl.StyleSize, optl.StyleColorSerial, optl.RevNo,
                optl.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", optl.OpSerial),
                new MySqlParameter("P_ITEMCODE", optl.ItemCode)
            };
            var optls = MySqlDBManager.GetAll<Optl>("SP_MES_GETTOOLS_OPTL", CommandType.StoredProcedure,
                mySqlParams.ToArray());

            return optls;
        }

        /// <summary>
        /// Inserts the tool.
        /// </summary>
        /// <param name="optl">The optl.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool InsertTool(Optl optl, MySqlConnection connection, MySqlTransaction transaction)
        {
            var ps = new OpsMySqlParams(optl.StyleCode, optl.StyleSize, optl.StyleColorSerial, optl.RevNo, optl.OpRevNo)
            {
                new MySqlParameter("P_OPSERIAL", optl.OpSerial),
                new MySqlParameter("P_ITEMCODE", optl.ItemCode),
                new MySqlParameter("P_MACHINE", optl.Machine),
                new MySqlParameter("P_EDITION", optl.Edition),
                new MySqlParameter("P_MAINTOOL", optl.MainTool),
                new MySqlParameter("P_AFFECTEDROWS", MySqlDbType.Int16) { Direction = ParameterDirection.Output }
            };

            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_INSERT_OPTL", ps.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }

        /// <summary>
        /// Deletes the optl.
        /// </summary>
        /// <param name="opdt">The opdt.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool DeleteOptl(Opdt opdt, MySqlTransaction transaction, MySqlConnection connection)
        {
            var pr = new OpsMySqlParams(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo,
                opdt.OpRevNo) { new MySqlParameter("P_OPSERIAL", opdt.OpSerial) };

            var affectedRow = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_DELETE_OPTL", pr.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return affectedRow != null;
        }

        /// <summary>
        /// Deletes the by opmt.
        /// </summary>
        /// <param name="opmt">The opmt.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 20-Jun-19
        public static bool DeleteByOpmt(Opmt opmt, MySqlTransaction transaction, MySqlConnection connection)
        {
            var prs = new OpsMySqlParams(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                opmt.RevNo, opmt.OpRevNo);

            var result = MySqlDBManager.ExecuteQueryWithTrans("SP_MES_DELETEBYOPMT_OPTL", prs.ToArray(),
                CommandType.StoredProcedure, transaction, connection);

            return result != null;
        }
        #endregion
    }
}
