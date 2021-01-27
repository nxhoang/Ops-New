using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class DfmpBus
    {
        #region get data from MySql
        /// <summary>
        /// Get list buyer defect code
        /// </summary>
        /// <param name="pkDefectCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<Dfmp> GetBuyerDefectMySql(string pkDefectCode)
        {
            string strSql = @"SELECT dfm.*
		                            , DMT.DEFECTDESC
		                            , mcm.code_name as buyername
		                            , bdm.BUYERDEFECTDESC
                            FROM T_CM_DFMP dfm 	
	                            LEFT JOIN t_cm_dfdt DFD ON DFD.DEFECTCODE = DFM.PKDEFECTCODE AND DFD.DEFECTCAT = DFM.CATEGORYID
                                LEFT JOIN T_CM_DFMT DMT ON DMT.DEFECTCODE = DFD.DEFECTCODE    
	                            LEFT join t_cm_mcmt mcm on mcm.S_CODE = dfm.buyer and m_code = 'Buyer' 
                                LEFT join t_cm_bdmt bdm on bdm.BUYERDEFECTCODE = dfm.buyerdefectcode and bdm.BUYER = dfm.buyer
                            where PKDEFECTCODE = ?P_PKDEFECTCODE;";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_PKDEFECTCODE", pkDefectCode)
            };

            var listBuyerDef = MySqlDBManager.GetObjects<Dfmp>(strSql, CommandType.Text, oraParams.ToArray());

            return listBuyerDef;
        }
        #endregion
    }
}
