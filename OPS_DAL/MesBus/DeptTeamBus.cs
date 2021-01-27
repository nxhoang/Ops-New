using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class DeptTeamBus
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

                cond = $"team in {factoryCodes} AND section IS NOT NULL AND TRIM(section) != '';";
            }

            var query = $@"SELECT 
                                    *
                                FROM
                                    mes.t_hr_dept
                                WHERE {cond}";

            var depts = await _MySqlDBManager.GetAllAsync<DeptEntity>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return depts;
        }

        public async Task<bool> BulkInsert(List<eDepartmentUnit> eDepartments, List<Fcmt> factories)
        {
            string qValue = "";

            for (int i = 0; i < eDepartments.Count; i++)
            {
                if (!string.IsNullOrEmpty(eDepartments[i].TEAM_CODE))
                {
                    var pkFactoryName = "";
                    var pkFactoryCode = "";
                    var tempFactory = factories.Where(x => x.Name.Replace("-", "") == eDepartments[i].TEAM_NAME || x.Name == eDepartments[i].TEAM_NAME);
                    if (tempFactory.Any())
                    {
                        pkFactoryName = tempFactory.FirstOrDefault().Name;
                        pkFactoryCode = tempFactory.FirstOrDefault().Factory;
                    }

                    qValue += $"('{eDepartments[i].CORPORATION_CODE}','{eDepartments[i].CORPORATION_NAME}'," +
                              $"'{eDepartments[i].DEPARTMENT_CODE}','{eDepartments[i].DEPARTMENT_NAME}'," +
                              $"'{eDepartments[i].LINE_CODE}','{eDepartments[i].LINE_NAME}', '{eDepartments[i].TEAM_CODE}'," +
                              $"'{eDepartments[i].TEAM_NAME}', '{pkFactoryName}', '{pkFactoryCode}')";
                    if (i != eDepartments.Count - 1) qValue += ",";
                }
            }

            if (qValue.Substring(qValue.Length - 1) == ",")
            {
                qValue = qValue.Substring(0, qValue.Length - 1);
            }

            string q = $@"INSERT INTO `mes`.`t_hr_deptteam`
                        (`CorpCode`,
                        `CorpName`,
                        `DeptCode`,
                        `DeptName`,
                        `LineCode`,
                        `LineName`,
                        `TeamCode`,
                        `TeamName`,
                        `PkName`,
                        `PkTeamCode`)
                        VALUES {qValue};";

            var rs = await _MySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, q, CommandType.Text);

            return rs;
        }

        public async Task<List<DeptTeam>> GetByCorp(string corpCode)
        {
            string q = $@"SELECT dt.`CorpCode`,
                dt.`CorpName`,
                dt.`DeptCode`,
                dt.`DeptName`,
                dt.`LineCode`,
                dt.`LineName`,
                dt.`TeamCode`,
                dt.`TeamName`
            FROM `mes`.`t_hr_deptteam` dt
            WHERE CorpCode = {corpCode}";

            var deptTeams = await _MySqlDBManager.GetAllAsync<DeptTeam>(ConstantGeneric.ConnectionStrMesMySql,
                q, CommandType.Text, null);

            return deptTeams;
        }

        public async Task<List<DeptTeam>> GetByDeptName(string deptName)
        {
            string q = $@"SELECT dt.`CorpCode`,
                dt.`CorpName`,
                dt.`DeptCode`,
                dt.`DeptName`,
                dt.`LineCode`,
                dt.`LineName`,
                dt.`TeamCode`,
                dt.`TeamName`
            FROM `mes`.`t_hr_deptteam` dt
            WHERE  TRIM(teamcode) != '' and deptname = '{deptName}'";

            var deptTeams = await _MySqlDBManager.GetAllAsync<DeptTeam>(ConstantGeneric.ConnectionStrMesMySql,
                q, CommandType.Text, null);

            return deptTeams;
        }

        public async Task<bool> DeleteByTeamCodes(List<eDepartmentUnit> teamCodes)
        {
            //if (teamCodes.Length <= 0) return false;
            //string qValue = CommonMethod.GetWhereCondition(teamCodes);
            //string q = $@"DELETE FROM mes.t_hr_deptteam 
            //            WHERE
            //                teamcode IN ({qValue});";

            //var rs = await _MySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, q, CommandType.Text);

            //return rs;

            if (teamCodes.Count <= 0) return false;
            var teamCodesCondition = CommonMethod.GetWhereCondition(teamCodes, "t_hr_deptteam", "TeamCode", "TEAM_CODE");
            var q = $"delete from `mes`.`t_hr_deptteam` where {teamCodesCondition};";
            var rs = await _MySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, q, CommandType.Text);

            return rs;
        }
    }
}
