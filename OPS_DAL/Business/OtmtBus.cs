using OPS_DAL.DAL;
using OPS_DAL.Entities;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System;
using MySql.Data.MySqlClient;
using OPS_Utils;

namespace OPS_DAL.Business
{
    public class OtmtBus
    {
        #region Properties

        static int _machine = (int)Machine.Machine;
        static int _tool = (int)Machine.Tool;

        #endregion

        #region Oracle database

        /// <summary>
        /// Get list of machine base on optype, opsub and opDetail.
        /// </summary>
        /// <param name="opType"></param>
        /// <param name="opSub"></param>
        /// <param name="opDetail"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Otmt> GetMachines(string opType, string opSub, string opDetail)
        {
            var oraParams = new List<OracleParameter>() {
                new OracleParameter("P_GROUPLEVEL_0", opType)
            };

            string conOpSub = string.Empty;
            string conOpDetail = string.Empty;
            //if group level 1 is not null then add group level 1 condition in where clause
            if (!string.IsNullOrEmpty(opSub))
            {
                conOpSub = " AND GROUPLEVEL_1 = :P_GROUPLEVEL_1 ";
                oraParams.Add(new OracleParameter("P_GROUPLEVEL_1", opSub));
            }

            //if group level 2 is not null then add group level 2 codition in where clause
            if (!string.IsNullOrEmpty(opDetail))
            {
                conOpDetail = " AND GROUPLEVEL_2 = :P_GROUPLEVEL_2  ";
                oraParams.Add(new OracleParameter("P_GROUPLEVEL_2", opDetail));
            }

            string strSql = $@"SELECT OT.ITEMCODE, OT.ITEMNAME, OT.SOS, OT.MODEL, MC.CODE_NAME AS BRANDNAME, OT.CATEGID, CAT.CODE_NAME AS CATEGORYNAME
                                        , OT.GROUPLEVEL_0, OT.GROUPLEVEL_1, OT.GROUPLEVEL_2
                                        , GL0.ENGLISH GROUPLEVEL_0_NAME, GL1.ENGLISH GROUPLEVEL_1_NAME, GL2.ENGLISH GROUPLEVEL_2_NAME, MC.MCHGROUPNAME
                                FROM T_OP_OTMT OT 
                                    LEFT JOIN T_CM_MCMT MC ON MC.S_CODE = OT.BRANDID AND M_CODE = 'MachineBrand'
                                    LEFT JOIN T_CM_MCMT CAT ON CAT.S_CODE = TRIM(OT.CATEGID) AND CAT.CODE_DESC = 'Machine'
                                    LEFT JOIN T_OP_OPNM GL0 ON GL0.OPNAMEID = OT.GROUPLEVEL_0
                                    LEFT JOIN T_OP_OPNM GL1 ON GL1.OPNAMEID = OT.GROUPLEVEL_1
                                    LEFT JOIN T_OP_OPNM GL2 ON GL2.OPNAMEID = OT.GROUPLEVEL_2
                                    LEFT JOIN T_OP_MCCA MC ON MC.OPNAMEID = OT.GROUPLEVEL_0 AND MC.MCHGROUPID = OT.MACHINEGROUP
                                WHERE GROUPLEVEL_0 = :P_GROUPLEVEL_0 {conOpSub} {conOpDetail}";

            //Get list of machine in database
            var listMachines = OracleDbManager.GetObjects<Otmt>(strSql, oraParams.ToArray());

            return listMachines;
        }

        /// <summary>
        /// Gets the op machine.
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Otmt> GetOpMachine(List<string> categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any()) return new List<Otmt>();

            var strCat = "'" + categoryIds[0] + "'";
            strCat = categoryIds.Where((t, j) => j > 0).Aggregate(strCat, (current, t) => current + (", '" + t + "'"));

            var sb = new StringBuilder();
            sb.AppendLine(" SELECT * FROM T_OP_OTMT WHERE TRIM(CATEGID) in (" + strCat + ") AND SUBSTR(ITEMCODE, 0, 1) = 'M' AND ( ACTIVE = '" + ConstantGeneric.Active + "' OR ACTIVE IS NULL) ");
            var lstOperationTool = OracleDbManager.GetObjects<Otmt>(sb.ToString(), null);
            return lstOperationTool;
        }

        /// <summary>
        /// Gets the list of op machines.
        /// </summary>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static List<Otmt> GetOpMachines()
        {
            var sb = $"SELECT * FROM T_OP_OTMT WHERE SUBSTR(ITEMCODE, 0, 1) = 'M' AND ACTIVE = '{ConstantGeneric.Active}'";
            var otmts = OracleDbManager.GetObjects<Otmt>(sb, null);

            return otmts;
        }

        /// <summary>
        /// Gets the op tool.
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Otmt> GetOpTool(List<string> categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any()) return new List<Otmt>();

            var strCat = "'" + categoryIds[0] + "'";
            strCat = categoryIds.Where((t, j) => j > 0).Aggregate(strCat, (current, t) => current + (", '" + t + "'"));

            //START MOD) SON (2019.11.20) - 21 November 2019 - add brand name in item name            
            //string strSql = " SELECT * FROM T_OP_OTMT WHERE TRIM(CATEGID) IN (" + strCat + ") AND SUBSTR(ITEMCODE, 0, 1) != 'M' AND ( ACTIVE = '" + ConstantGeneric.Active + "' OR ACTIVE IS NULL) ";
            string strSql = @" SELECT ITEMCODE    
                                     , CASE
                                            WHEN MACHINE = 1 THEN OT.ITEMNAME
                                          ELSE  CASE WHEN OT.BRANDID IS NULL THEN OT.ITEMNAME ELSE CM.CODE_NAME || ' - ' || OT.ITEMNAME END
                                       END ITEMNAME
                                    , SOS, BUYER, BRAND, CATEGORY, IMAGEPATH, MODEL, COST
                                    , REMARKS, RANKING, REGISTRAR, CATEGID, PURPOSE, PROCESS, MACHINE, GSDREFID, ACTIVE, UNIT, BRANDID
                                FROM T_OP_OTMT OT
                                    LEFT JOIN T_CM_MCMT CM ON CM.S_CODE = OT.BRANDID AND M_CODE = 'MachineBrand' WHERE TRIM(CATEGID) IN (" + strCat + ") AND SUBSTR(ITEMCODE, 0, 1) != 'M' AND ( ACTIVE = '" + ConstantGeneric.Active + "' OR ACTIVE IS NULL) ";
            //END MOD) SON (2019.11.20) - 21 November 2019 - add brand name in item name

            var prams = new OracleParameter[1];
            prams[0] = new OracleParameter("CATEGID", strCat);
            var operationTools = OracleDbManager.GetObjects<Otmt>(strSql, CommandType.Text, null);

            return operationTools;
        }

        /// <summary>
        /// GetSqlByMachine.
        /// </summary>
        /// <param name="tb">The table.</param>
        /// <param name="isMachine">1 Machine, 0 tools.</param>
        /// <returns></returns>
        /// Author: VitHV
        static string GetSqlByMachine(string tb, int machine)
        {
            return machine == _machine ? " AND " + tb + ".MACHINE = '" + _machine + "' " :
                " AND (" + tb + ".MACHINE IS NULL OR " + tb + ".MACHINE = '" + _tool + "') ";
        }
        /// <summary>
        /// Gets the otmts by cate gid.
        /// </summary>
        /// <param name="gId">The g identifier.</param>
        /// <param name="isMachine">1 Machine, 0 tools.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static List<Otmt> GetOtmtsByCateGid(string gId, int isMachine)
        {
            string strBy = GetSqlByMachine("OT", isMachine);
            string strWhere = isMachine == _machine ? strBy + "AND MC.M_CODE IN ('NonSewingMc','SewingMc')" :
                " " + strBy + " AND MC.M_CODE IN ('OPTool')";
            string subImagePath = "";
            try
            {
                subImagePath = GetPathImg();
            }
            catch
            {
                subImagePath = "";
            }
            //MOD) SON (2019.11.20) - 20 November 2019 - remove OT.ITEMNAME and change by case when
            string sql = @" SELECT OT.ITEMCODE                           
                            , CASE 
                                WHEN MACHINE = 1 THEN   OT.ITEMNAME 
                                ELSE  CASE WHEN OT.BRANDID IS NULL THEN OT.ITEMNAME ELSE C.CODE_NAME || ' - ' || OT.ITEMNAME END
                             END ITEMNAME
                           ,'" + subImagePath + @"' || OT.IMAGEPATH IMAGEPATH
                           , OT.CATEGID
                           ,C.CODE_NAME BRAND
                           , D.CODE_NAME PROCESSNAME -- ADD - SON) 28/Feb/2020
                           , OT.PROCESSCODE -- ADD - SON) 28/Feb/2020
                           ,OT.BRANDID
                           ,OT.PURPOSE
                           ,OT.PROCESS
                           ,OT.REMARKS
                           ,OT.MODEL
                           ,OT.SOS
                           ,S.FULLNAME
                           ,OT.BUYER
                           ,B.CODE_NAME BuyerName
                           ,OT.RANKING
                           ,OT.REGISTRAR
                           ,OT.GSDREFID
                           ,OT.COST
                           ,OT.UNIT
                            , OT.GROUPLEVEL_0
                            , OT.GROUPLEVEL_1
                            , OT.GROUPLEVEL_2
                            , OT.MACHINEGROUP
                           ,MC.CODE_NAME  Category
                           ,CASE WHEN OT.ACTIVE ='Y' THEN 'In Use' ELSE 'Not In Use' END STATUS
                    FROM T_OP_OTMT OT JOIN T_CM_MCMT  MC ON TRIM(OT.CATEGID) = MC.S_CODE 
                                   LEFT JOIN 
                                    (
                                         SELECT * FROM T_CM_MCMT 
                                         where m_code='Buyer' and s_code<>'000'
                                    )B ON OT.BUYER = B.S_CODE  
                                   LEFT JOIN 
                                    (
                                         SELECT * FROM T_CM_MCMT 
                                         where m_code='MachineBrand' 
                                    )C ON OT.BRANDID = C.S_CODE  
                                    LEFT JOIN -- START ADD - SON) 28/Feb/2020
                                    (
                                         SELECT s_code, code_name FROM T_CM_MCMT 
                                         where m_code='ProcessCode' 
                                    )D ON OT.PROCESSCODE = D.S_CODE  -- END ADD - SON) 28/Feb/2020 
                                    LEFT JOIN (SELECT SOS , FULLNAME FROM T_CM_SSCM WHERE ACCCHK = 'Y' AND STATUS = 'OK')
                                    S ON S.SOS = OT.SOS
                    WHERE TRIM(OT.CATEGID) =: CATEGID " + strWhere;
            var prams = new OracleParameter[1];
            prams[0] = new OracleParameter("CATEGID", gId);
            var lstOperationTool = OracleDbManager.GetObjects<Otmt>(sql, prams);
            return lstOperationTool;
        }

        /// <summary>
        /// Get automatic Code
        /// </summary>
        /// <param name="isMachine"></param>
        /// <returns></returns>
        public static string GetAutomaticCode(int isMachine)
        {
            string strBy = GetSqlByMachine("OT", isMachine);
            string sql = @"SELECT SUBSTR(ITEMCODE, 1, 1) || LPAD((TO_NUMBER(SUBSTR(ITEMCODE,2)) + 1), 12, '0') ITEMCODE
                            from(
                                    SELECT ITEMCODE FROM T_OP_OTMT OT WHERE 1=1 " + strBy + @" and rownum = 1
                                    ORDER BY ITEMCODE DESC
                              ) ";

            //START MOD) SON - 2019/Jan/11
            //string sql = @"SELECT SUBSTR(ITEMCODE, 1, 1) || LPAD((TO_NUMBER(SUBSTR(ITEMCODE,2)) + 1), 12, '0') ITEMCODE
            //                from( SELECT MAX(ITEMCODE) ITEMCODE FROM T_OP_OTMT OT WHERE 1=1 " + strBy + ") ";
            //END MOD) SON - 2019/Jan/11

            var otmt = OracleDbManager.GetObjects<Otmt>(sql, null).FirstOrDefault();
            if (otmt == null)
            {
                if (isMachine == (int)Machine.Machine)
                    return "M000000000001";
                else
                {
                    return "0000000000001";
                }
            }
            return otmt.ItemCode;
        }

        /// <summary>
        /// Get PathImg Code
        /// </summary>
        /// <returns> pathImg</returns>
        /// Author: VitHV
        public static string GetPathImg()
        {
            FtpInfo f = FtpInfoBus.GetFtpInfo("OPSToolHttp");
            return f.FtpLink + "/";
        }

        /// <summary>
        /// Gets the otmts by item code.
        /// </summary>
        /// <param name="itemCode">The item code.</param>
        /// <param name="isMachine">The is machine.</param>
        /// <returns></returns>
        /// Author: VitHV
        public static Otmt GetOtmtsByItemCode(string itemCode, int isMachine)
        {
            string strBy = GetSqlByMachine("OT", isMachine);
            string strWhere = isMachine == _machine ? strBy + "AND MC.M_CODE IN ('NonSewingMc','SewingMc')" :
                " " + strBy + " AND MC.M_CODE IN ('OPTool')";
            //MOD) SON (2019.11.20) - 20 November 2019 - remove ,OT.ITEMNAME  and change by case when
            string sql = @"  SELECT OT.ITEMCODE   
                               , CASE
                                    WHEN MACHINE = 1 THEN OT.ITEMNAME
                                  ELSE  CASE WHEN OT.BRANDID IS NULL THEN OT.ITEMNAME ELSE BR.CODE_NAME || ' - ' || OT.ITEMNAME END
                                END ITEMNAME
                               ,OT.IMAGEPATH
                               ,OT.CATEGID
                               ,OT.BRAND
                               ,OT.PURPOSE
                               ,OT.PROCESS
                               ,OT.REMARKS
                               ,OT.MODEL
                               ,OT.SOS
                               ,OT.BUYER
                               ,OT.RANKING
                               ,OT.REGISTRAR
                               ,OT.GSDREFID
                               ,OT.COST
                               ,OT.UNIT
                               ,MC.CODE_NAME  Category
                        FROM T_OP_OTMT OT
                        JOIN T_CM_MCMT  MC ON TRIM(OT.CATEGID) = MC.S_CODE
                        LEFT JOIN T_CM_MCMT BR ON OT.BRANDID = BR.S_CODE AND BR.M_CODE = 'MachineBrand'
                        WHERE ItemCode =: ItemCode " + strWhere;
            var prams = new OracleParameter[1];
            prams[0] = new OracleParameter("ItemCode", itemCode);
            var lstOperationTool = OracleDbManager.GetObjects<Otmt>(sql, prams);

            return lstOperationTool.FirstOrDefault();
        }

        /// <summary>
        /// GetCategroy
        /// <param name="isMachine">1 Machine, 0 tools.</param>
        /// </summary>
        /// Author: VitHV
        public static List<Mcmt> GetCategroy(int isMachine)
        {
            string strWhere = isMachine == 1 ? " WHERE M_CODE IN('NonSewingMc','SewingMc') "
                : " WHERE M_CODE IN('OPTool') ";
            //string sql = " SELECT DISTINCT CATEGID FROM T_OP_OTMT " + strWhere;
            string sql = "SELECT S_CODE SubCode, CODE_NAME CodeName FROM T_CM_MCMT " + strWhere;
            return OracleDbManager.GetObjects<Mcmt>(sql, null);
        }

        /// <summary>
        /// Check linked
        /// </summary>
        /// <param name="itemcode"></param>
        /// Author: VitHV
        public static bool CheckReferenceKey(string itemcode)
        {
            string sql = "SELECT ITEMCODE FROM T_OP_OPTL L WHERE L.ITEMCODE = :ITEMCODE AND ROWNUM <2";
            var prams = new OracleParameter[1];
            prams[0] = new OracleParameter("ITEMCODE", itemcode);
            var rs = OracleDbManager.GetObjects<Mcmt>(sql, prams);
            return rs.Count() > 0;
        }

        /// <summary>
        /// Update otmts.
        /// </summary>
        /// <param name="otmt">The otmt.</param>
        /// <returns></returns>
        /// Author: Vithv
        public static string Update(Otmt otmt)
        {

            string sql = @"  UPDATE T_OP_OTMT OT
                                    SET OT.ITEMNAME = :ITEMNAME,
                                    OT.SOS = :SOS,
                                    OT.BUYER = :BUYER,
                                    --OT.BRAND = :BRAND,
                                    OT.BRANDID = :BRANDID,
                                    OT.CATEGORY = :CATEGORY,
                                    OT.IMAGEPATH = :IMAGEPATH,
                                    OT.MODEL = :MODEL,
                                    OT.COST = :COST,
                                    OT.UNIT = :UNIT,
                                    OT.REMARKS = :REMARKS,
                                    OT.RANKING = :RANKING,
                                    OT.REGISTRAR = :REGISTRAR,
                                    OT.CATEGID =:CATEGID,
                                    OT.PURPOSE = :PURPOSE,
                                    OT.PROCESS =:PROCESS,
                                    OT.MACHINE = :MACHINE,
                                    OT.GSDREFID = :GSDREFID,
                                    OT.PROCESSCODE = :P_PROCESSCODE,
                                    OT.GROUPLEVEL_0 = :P_GROUPLEVEL_0,
                                    OT.GROUPLEVEL_1 = :P_GROUPLEVEL_1,
                                    OT.GROUPLEVEL_2 = :P_GROUPLEVEL_2,
                                    OT.MACHINEGROUP = :P_MACHINEGROUP
                                WHERE OT.ITEMCODE = :ITEMCODE";
            var prams = new List<OracleParameter>
            {
                new OracleParameter("ITEMNAME", otmt.ItemName),
                new OracleParameter("SOS", otmt.Sos),
                new OracleParameter("BUYER", otmt.Buyer),
                //new OracleParameter("BRAND", otmt.Brand),
                new OracleParameter("BRANDID", otmt.BrandId),
                new OracleParameter("CATEGORY", otmt.Category),
                new OracleParameter("IMAGEPATH", otmt.ImagePath),
                new OracleParameter("MODEL", otmt.Model),
                new OracleParameter("COST", otmt.Cost),
                new OracleParameter("UNIT", otmt.Unit),
                new OracleParameter("REMARKS", otmt.Remarks),
                new OracleParameter("RANKING", otmt.Ranking),
                new OracleParameter("REGISTRAR", otmt.Registrar),
                new OracleParameter("CATEGID", otmt.CategId),
                new OracleParameter("PURPOSE", otmt.Purpose),
                new OracleParameter("PROCESS", otmt.Process),
                new OracleParameter("MACHINE", otmt.Machine),
                new OracleParameter("GSDREFID",otmt.GsdRefId),
                new OracleParameter("P_PROCESSCODE",otmt.ProcessCode),
                new OracleParameter("P_GROUPLEVEL_0",otmt.GroupLevel_0), //ADD - SON) 8/Oct/2020
                new OracleParameter("P_GROUPLEVEL_1",otmt.GroupLevel_1), //ADD - SON) 8/Oct/2020
                new OracleParameter("P_GROUPLEVEL_2",otmt.GroupLevel_2), //ADD - SON) 8/Oct/2020
                new OracleParameter("P_MACHINEGROUP",otmt.MachineGroup), //ADD - SON) 16/Oct/2020
                new OracleParameter("ITEMCODE",otmt.ItemCode)
            };
            var lstOperationTool = OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return lstOperationTool.ToString();
        }

        /// <summary>
        /// AddMachine otmts.
        /// </summary>
        /// <param name="otmt">The otmt.</param>
        /// <returns></returns>
        /// Author: Vithv
        public static string AddMachine(Otmt otmt)
        {

            string sql = @"  INSERT INTO T_OP_OTMT(ITEMCODE,ITEMNAME,SOS,BUYER,BRANDID,CATEGORY,IMAGEPATH,MODEL,COST,UNIT,REMARKS,RANKING,REGISTRAR,CATEGID,PURPOSE,PROCESS,MACHINE,GSDREFID,ACTIVE, PROCESSCODE, GROUPLEVEL_0, GROUPLEVEL_1, GROUPLEVEL_2, MACHINEGROUP)
                             VALUES(:ITEMCODE,:ITEMNAME,:SOS,:BUYER,:BRANDID,:CATEGORY,:IMAGEPATH,:MODEL,:COST,:UNIT,:REMARKS,:RANKING,:REGISTRAR,:CATEGID,:PURPOSE,:PROCESS,:MACHINE,:GSDREFID,'" + ConstantGeneric.Active + "', :P_PROCESSCODE, :P_GROUPLEVEL_0, :P_GROUPLEVEL_1, :P_GROUPLEVEL_2, :P_MACHINEGROUP)"; //ADD - SON) 28/Feb/2020 - add process code
            var prams = new List<OracleParameter>
            {
                new OracleParameter("ITEMCODE",otmt.ItemCode),
                new OracleParameter("ITEMNAME", otmt.ItemName),
                new OracleParameter("SOS", otmt.Sos),
                new OracleParameter("BUYER", otmt.Buyer),
                //new OracleParameter("BRAND", otmt.Brand),
                new OracleParameter("BRANDID", otmt.BrandId),
                new OracleParameter("CATEGORY", otmt.Category),
                new OracleParameter("IMAGEPATH", otmt.ImagePath),
                new OracleParameter("MODEL", otmt.Model),
                new OracleParameter("COST", otmt.Cost),
                new OracleParameter("UNIT", otmt.Unit),
                new OracleParameter("REMARKS", otmt.Remarks),
                new OracleParameter("RANKING", otmt.Ranking),
                new OracleParameter("REGISTRAR", otmt.Registrar),
                new OracleParameter("CATEGID", otmt.CategId),
                new OracleParameter("PURPOSE", otmt.Purpose),
                new OracleParameter("PROCESS", otmt.Process),
                new OracleParameter("MACHINE", otmt.Machine),
                new OracleParameter("GSDREFID",otmt.GsdRefId),
                new OracleParameter("P_PROCESSCODE",otmt.ProcessCode),//ADD - SON) 28/Feb/2020 - add process code
                //START ADD - SON) 9/Oct/2020
                new OracleParameter("P_GROUPLEVEL_0",otmt.GroupLevel_0),
                new OracleParameter("P_GROUPLEVEL_1",otmt.GroupLevel_1),
                new OracleParameter("P_GROUPLEVEL_2",otmt.GroupLevel_2),
                new OracleParameter("P_MACHINEGROUP",otmt.MachineGroup),
                //END ADD - SON) 9/Oct/2020

            };
            var lstOperationTool = OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
            return lstOperationTool.ToString();
        }

        /// <summary>
        /// DeleteMachine
        /// </summary>
        /// <param name="itemcode"></param>
        /// Author: VitHV
        public static void DeleteMachine(string itemcode)
        {
            string sql = "UPDATE T_OP_OTMT SET ACTIVE='" + ConstantGeneric.NoneActive + "'  WHERE ITEMCODE= :ITEMCODE";
            var prams = new OracleParameter[1];
            prams[0] = new OracleParameter("ITEMCODE", itemcode);
            OracleDbManager.ExecuteQuery(sql, prams.ToArray(), CommandType.Text);
        }

        /// <summary>
        /// Get machine id by machine name
        /// </summary>
        /// <param name="machineName"></param>
        /// <returns></returns>
        /// AuthorL: Son Nguyen Cao
        public static Otmt GetMachineId(string machineName)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_MACHINENAME", machineName),
                cursor
            };
            var machine = OracleDbManager.GetObjects<Otmt>("SP_OPS_GETMACHINEID_OTMT", CommandType.StoredProcedure, oracleParams.ToArray());
            return machine.FirstOrDefault();
        }

        //START ADD) HA NGUYEN

        // Search Machine      
        public static List<Otmt> SearchMachineName(string searchName)
        {
            string strSql = $"SELECT * from t_op_otmt where ITEMCODE like 'M%' and lower (ITEMNAME) like lower ('%{searchName}%')";

            var lstOtmt = OracleDbManager.GetObjects<Otmt>(strSql, null);

            return lstOtmt;
        }

        //Search Tool Id
        public static List<Otmt> SearchToolId(string searchName)
        {
            string strSql = $"SELECT * from t_op_otmt where ITEMCODE not like 'M%' and lower (ITEMNAME) like lower ('%{searchName}%')";

            var lstOtmt = OracleDbManager.GetObjects<Otmt>(strSql, null);

            return lstOtmt;
        }
        //END ADD) HA NGUYEN

        #endregion

        #region MySQL database

        public static Otmt GetMachineByName(string machineName)
        {
            var pL = new List<MySqlParameter> { new MySqlParameter("P_MACHINENAME", machineName) };
            var otmt = MySqlDBManager.GetAll<Otmt>("SP_MES_GETMACHINEBYNAME_OTMT", CommandType.StoredProcedure,
                pL.ToArray());

            return otmt.FirstOrDefault();
        }

        /// <summary>
        /// Gets the by categ ids.
        /// </summary>
        /// <param name="categIds">The categ ids.</param>
        /// <param name="oper">The operator '!=' for getting tools and '=' for machines .</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 26-Jun-19
        public static List<Otmt> GetByCategIds(List<string> categIds, string oper)
        {
            if (categIds == null || !categIds.Any()) return new List<Otmt>();

            var categid = "'" + categIds[0] + "'";
            categid = categIds.Where((t, j) => j > 0).Aggregate(categid, (current, t) => current + ", '" + t + "'");

            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("P_CATEGID", categid),
                new MySqlParameter("P_OPERATOR", oper)
            };
            var tools = MySqlDBManager.GetAll<Otmt>("SP_MES_GETBYCATEGIDS_OTMT", CommandType.StoredProcedure,
                prs.ToArray());

            return tools;
        }

        public static List<Otmt> MySqlGetOpMachines()
        {
            var sb = $"SELECT * FROM mes.T_OP_OTMT WHERE SUBSTR(ITEMCODE, 0, 1) = 'M' AND ACTIVE = '{ConstantGeneric.Active}'";
            var otmts = MySqlDBManager.GetAll<Otmt>(sb, CommandType.Text, null);

            return otmts;
        }
        #endregion
    }
}
