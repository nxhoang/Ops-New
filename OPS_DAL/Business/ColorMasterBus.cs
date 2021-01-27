using System.Collections.Generic;
using OPS_DAL.DAL;
using OPS_DAL.Entities;

namespace OPS_DAL.Business
{
    public class ColorMasterBus
    {
        /// <summary>
        /// SON ADD
        /// </summary>
        /// <returns>A list color master</returns>
        public static List<ColorMaster> GetColorMaster()
        {
            var sqlSelectColor = " SELECT COLORCODE, COLORDESC FROM T_CM_CCMT ";

            var lstColorMaster = OracleDbManager.GetObjects<ColorMaster>(sqlSelectColor, null);
            return lstColorMaster;
        }
    }
}
