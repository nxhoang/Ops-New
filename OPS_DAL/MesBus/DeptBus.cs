using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class DeptBus
    {
        private readonly MySqlDBManager _MySqlDBManager = new MySqlDBManager();

        /// <summary>
        /// Getting list of departments by factory
        /// </summary>
        /// <param name="factories">list of factories</param>
        /// <returns>list of departments</returns>
        public async Task<List<DeptEntity>> GetByFactory(List<string> factories)
        {
            string cond;
            if (factories == null)
            {
                cond = "section IS NOT NULL AND TRIM(section) != '';";
            }
            else
            {
                var factoryCodes = "";
                for (var i = 0; i < factories.Count; i++)
                {
                    if (i == 0)
                    {
                        factoryCodes += $"('{factories[i]}'";
                    }
                    else
                    {
                        factoryCodes += $",'{factories[i]}'";
                    }
                    if (i == factories.Count - 1) factoryCodes = $"{factoryCodes})";
                }

                cond = $"dt.pkteamcode in {factoryCodes} AND section IS NOT NULL AND TRIM(section) != '';";
            }

            var query = $@"SELECT 
                                    d.*, dt.pkname
                                FROM
                                    mes.t_hr_dept d
                                left JOIN
                                    mes.t_hr_deptteam dt ON d.team = dt.teamname
                                WHERE {cond}";

            var depts = await _MySqlDBManager.GetAllAsync<DeptEntity>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return depts;
        }
    }
}
