using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class DfmtBus
    {
        #region MySql
        /// <summary>
        /// Get list defect code by category
        /// </summary>
        /// <param name="defectCat"></param>
        /// <returns></returns>
        public static List<Dfmt> GetListDefectCodeByCategory(string defectCat)
        {
            string strSql = @" select distinct dt.defectcat, dt.defectcode, dm.defectdesc, dm.vietnamese, dm.bahasa, dm.burmese, dm.amharic
                                    , case when dfm.PKDEFECTCODE is null then 'N' else 'Y' end hasBuyerDefect                                
                                from t_cm_dfDt dt 
                                    join t_cm_dfmt dm on dm.defectcode = dt.defectcode 
                                    left join t_cm_dfmp dfm on dfm.PKDEFECTCODE = dt.defectcode and dfm.CATEGORYID = dt.defectcat
                                    left join t_cm_bdmt bdm on bdm.BUYERDEFECTCODE = dfm.BUYERDEFECTCODE and bdm.BUYER = dfm.BUYER
                                where 1 = 1 ";

            var defectCatCon = @" and dt.defectcat = ?P_DEFECTCAT; ";

            var param = new List<MySqlParameter>();

            if (!string.IsNullOrEmpty(defectCat))
            {
                strSql += defectCatCon;
                param.Add(new MySqlParameter("P_DEFECTCAT", defectCat));
            }

            var listDfcd = MySqlDBManager.GetObjects<Dfmt>(strSql, CommandType.Text, param.ToArray());

            return listDfcd;
        }

        #endregion

    }
}
