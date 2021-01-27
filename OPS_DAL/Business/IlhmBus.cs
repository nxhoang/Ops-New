using OPS_DAL.DAL;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class IlhmBus
    {
        public static List<Ilhm> GetItemMainLevel()
        {
            string sql = @"SELECT MAINLEVEL, LEVELNO_01, LEVELNO_02, LEVELNO_03, LEVELNO_04, LEVELNO_05, LEVELNO_06, LEVELNO_07 
                            LEVELNO_08, LEVELNO_09, MAINLEVEL_NAME MAINLEVELNAME, NEWTYPE, LEVELNO_10 FROM T_00_ILHM";
            var lstItemMailLevel = OracleDbManager.GetObjects<Ilhm>(sql, null);

            return lstItemMailLevel;
        }
    }
}
