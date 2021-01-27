using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class CstpBus
    {
        private readonly MySqlDBManager _MySqlDBManager = new MySqlDBManager();
        //private readonly string _mySqlConn = ConstantGeneric.ConnectionStrMesMySql;

        public static List<Cstp> GetAll()
        {
            var cstp = MySqlDBManager.GetAll<Cstp>("SP_MES_GETALL_CSTP", CommandType.StoredProcedure,
                new MySqlParameter[] { });

            return cstp;
        }

        /// <summary>
        /// Get cstp by corporation code and factory
        /// </summary>
        /// <param name="corpCode"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Cstp GetCstpByCorpAndFactory(string corpCode, string factory)
        {

            string strSql = @" SELECT * FROM T_CM_CSTP WHERE CORPCODE = ?P_CORPCODE AND FACTORY = ?P_FACTORY ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_CORPCODE", corpCode),
                new MySqlParameter("P_FACTORY", factory)
            };

            var cstp = MySqlDBManager.GetObjects<Cstp>(strSql, CommandType.Text, param.ToArray()).FirstOrDefault();

            return cstp;
        }

        /// <summary>
        /// Get coporate seting by server number
        /// </summary>
        /// <param name="serverNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Cstp GetCstpByServerNo(string serverNo)
        {

            string strSql = @" SELECT * FROM T_CM_CSTP WHERE SERVERNO = ?P_SERVERNO ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_SERVERNO", serverNo)
            };

            var cstp = MySqlDBManager.GetObjects<Cstp>(strSql, CommandType.Text, param.ToArray()).FirstOrDefault();

            return cstp;
        }

        /// <summary>
        /// Get first cstp object
        /// </summary>
        /// <returns></returns>
        /// Author: ndphuong
        public static async Task<Cstp> GetFirstRecordAsync()
        {
            var result = MySqlDBManager.GetAll<Cstp>($@"SELECT *
                                                        FROM t_cm_cstp
                                                        ", System.Data.CommandType.Text, null);

            return await Task.FromResult<Cstp>(result.FirstOrDefault());
        }

        public async Task<Cstp> GetByServerNo(string serverNo)
        {
            string strSql = @" SELECT * FROM T_CM_CSTP WHERE SERVERNO = ?P_SERVERNO ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_SERVERNO", serverNo)
            };

            var cstp = await _MySqlDBManager.GetAllAsync<Cstp>(ConstantGeneric.ConnectionStrMesMySql, strSql, CommandType.Text, param.ToArray());

            return cstp.FirstOrDefault();
        }
    }
}
