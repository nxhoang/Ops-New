using OPS_DAL.APIEntities;
using OPS_DAL.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIBus
{
    public class BomtAPIBus
    {
        /// <summary>
        /// Get bom detail
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        public static List<BomtAPI> GetBOMDetail(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            var strSql = @" SELECT BOM.*, ICM.ITEMNAME, ICC.ITEMCOLORWAYS, ICM2.ITEMNAME AS MAINITEMNAME, ICC2.ITEMCOLORWAYS AS MAINITEMCOLORWAYS 
                            FROM T_SD_BOMT BOM 
                                LEFT JOIN T_00_ICMT ICM ON ICM.ITEMCODE = BOM.ITEMCODE
                                LEFT JOIN T_00_ICCM ICC ON ICC.ITEMCODE = BOM.ITEMCODE AND ICC.ITEMCOLORSERIAL = BOM.ITEMCOLORSERIAL
                                LEFT JOIN T_00_ICMT ICM2 ON ICM2.ITEMCODE = BOM.MAINITEMCODE
                                LEFT JOIN T_00_ICCM ICC2 ON ICC2.ITEMCODE = BOM.MAINITEMCODE AND ICC2.ITEMCOLORSERIAL = BOM.MAINITEMCOLORSERIAL 
                            WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO ";

            var oraPrams = new OpsParams(stlCode.ToUpper(), stlSize.ToUpper(), stlColorSerial, stlRevNo);

            return OracleDbManager.GetObjects<BomtAPI>(strSql, oraPrams.ToArray());
        }
    }
}
