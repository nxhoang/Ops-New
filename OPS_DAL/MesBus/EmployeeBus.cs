using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class EmployeeBus
    {
        private readonly MySqlDBManager _MySqlDBManager = new MySqlDBManager();

        /// <summary>
        /// Inserting list of employees
        /// </summary>
        /// <param name="users">List of employees</param>
        /// <returns>true/false</returns>
        public async Task<bool> BulkInsertEmployee(List<eUserInformation> users)
        {
            string qValue = "";

            for (int i = 0; i < users.Count; i++)
            {
                qValue += $"('{users[i].USER_ID}','{users[i].USER_NAME}','{users[i].GENDER}','{users[i].FACTORY}'," +
                          $"'{users[i].DEPTCODE}', '{users[i].DEPARTMENT}','{users[i].POSITION}','','{users[i].SKILL_LEVEL}'," +
                          "null,null,null,'','')";
                if (i != users.Count - 1) qValue += ",";
            }

            string q = $@"INSERT INTO `mes`.`t_hr_empm`
                        (`EmployeeCode`,
                        `Name`,
                        `Gender`,
                        `Factory`,
                        `DeptCode`,
                        `Department`,
                        `Position`,
                        `Skill`,
                        `SkillLevel`,
                        `JoinedDate`,
                        `UpdatedDate`,
                        `LastWorkedDate`,
                        `Status`,
                        `ImageUrl`)
                        VALUES {qValue};";
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                using (MySqlCommand myCmd = new MySqlCommand(q, connection))
                {
                    myCmd.CommandType = CommandType.Text;
                    await myCmd.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        public async Task<bool> BulkInsertEmployee(List<eEmployeeBasicInfo> users, string corp, string team)
        {
            string qValue = "";

            for (int i = 0; i < users.Count; i++)
            {
                var empName = users[i].EMPNAME.Replace("'", "''");
                var extImg = Path.GetExtension(users[i].IMAGE);
                //var imgName = $"{users[i].EMPID}{extImg}";

                qValue += $"('{users[i].EMPID}','{empName}','{users[i].GENDER}','{users[i].FACTORY}','{corp}'," +
                          $"'{users[i].DEPTCODE}', '{users[i].DEPARTMENT}','{users[i].POSITION}','','{users[i].SKILL_LEVEL}'," +
                          $"null,null,null,'','{users[i].IMAGE}','{users[i].JOBTYPE}','{users[i].LEVEL}','{users[i].LINE}'," +
                          $"'{users[i].PROCESSTYPE}','{users[i].RESPONSIBILITYNM}','{team}')";
                if (i != users.Count - 1) qValue += ",";
            }

            string q = $@"INSERT INTO `mes`.`t_hr_empm`
                        (`EmployeeCode`,
                        `Name`,
                        `Gender`,
                        `Factory`,
                        `CorporationCode`,
                        `DeptCode`,
                        `Department`,
                        `Position`,
                        `Skill`,
                        `SkillLevel`,
                        `JoinedDate`,
                        `UpdatedDate`,
                        `LastWorkedDate`,
                        `Status`,
                        `ImageUrl`,                        
                        `JobType`,
                        `Level`,
                        `Line`,
                        `ProcessType`,
                        `Responsibility`,
                        `Team`)
                        VALUES {qValue};";
            var rs = await _MySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, q, CommandType.Text);

            return rs;
        }

        public async Task<bool> DeleteEmployees(List<eEmployeeBasicInfo> tempUsers)
        {
            if (tempUsers.Count <= 0) return false;
            var empIds = CommonMethod.GetWhereCondition(tempUsers, "t_hr_empm", "EmployeeCode", "EMPID");
            var q = $"delete from `mes`.`t_hr_empm` where {empIds};";
            var rs = await _MySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, q, CommandType.Text);

            return rs;
        }

        public async Task<List<Employee>> GetEmployees(string corp, List<DeptEntity> depts, string[] deptNames,
            string[] positions)
        {
            var corpCon = string.IsNullOrEmpty(corp) ? "1=1" : $"(emp.CorporationCode IN ('{corp}'))";
            var deptCodes = CommonMethod.GetWhereCondition(depts, "emp", "DeptCode");
            var dns = CommonMethod.GetWhereCondition(deptNames);
            var poss = CommonMethod.GetWhereCondition(positions);
            var query = $@"SELECT 
                                    *
                                FROM
                                    mes.t_hr_empm emp
                                WHERE
                                    {corpCon}
                                        AND {deptCodes}
                                        AND ({dns})
                                        AND ({poss});";

            var emps = await _MySqlDBManager.GetAllAsync<Employee>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return emps;
        }


        /// <summary>
        /// Getting list of employees
        /// </summary>
        /// <param name="corp"></param>
        /// <param name="dept"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns></returns>
        public async Task<List<Employee>> GetOpEmps(string corp, string dept, string team, string styleCode,
            string styleSize, string styleColorSerial, string revNo, string opRevNo, string mxPackage)
        {
            var query = $@"SELECT 
                                    empm.EmployeeCode,
                                    empm.Name,
                                    empm.Gender,
                                    empm.Factory,
                                    empm.CorporationCode,
                                    empm.Department,
                                    empm.Position,
                                    empm.Skill,
                                    empm.SkillLevel,
                                    empm.JoinedDate,
                                    empm.UpdatedDate,
                                    empm.LastWorkedDate,
                                    empm.Status,
                                    empm.ImageUrl,
                                    empm.ImageName,
                                    p.stylecode,
                                    p.stylesize,
                                    p.stylecolorserial,
                                    p.revno,
                                    p.oprevno,
                                    p.opserial,
                                    p.mxpackage
                                FROM
                                    mes.t_hr_empm empm
                                        LEFT JOIN
                                    (SELECT 
                                        opdt.EmployeeCode,
                                            opdt.stylecode,
                                            opdt.stylesize,
                                            opdt.stylecolorserial,
                                            opdt.revno,
                                            opdt.oprevno,
                                            opdt.opserial,
                                            opmt.mxpackage
                                    FROM
                                        mes.t_mx_opdt opdt
                                    JOIN mes.t_mx_opmt opmt ON opdt.stylecode = opmt.stylecode
                                        AND opdt.stylesize = opmt.stylesize
                                        AND opdt.stylecolorserial = opmt.stylecolorserial
                                        AND opdt.revno = opmt.revno
                                        AND opdt.oprevno = opmt.oprevno
                                    WHERE
                                        opdt.stylecode = '{styleCode}'
                                            AND opdt.stylesize = '{styleSize}'
                                            AND opdt.stylecolorserial = '{styleColorSerial}'
                                            AND opdt.revno = '{revNo}'
                                            AND opdt.oprevno = '{opRevNo}'
                                            AND opmt.mxpackage = '{mxPackage}') AS p ON empm.EmployeeCode = p.EmployeeCode
                                WHERE
                                    empm.corporationcode = '{corp}'
                                        AND empm.department = '{dept}'
                                        AND empm.deptcode = '{team}';";

            var emps = await _MySqlDBManager.GetAllAsync<Employee>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return emps;
        }

        public async Task<List<Employee>> GetOpEmps(string corp, string dept, string[] teams, string styleCode,
            string styleSize, string styleColorSerial, string revNo, string opRevNo, string mxPackage)
        {
            var teamCodes = CommonMethod.GetWhereCondition(teams);
            var tc = teamCodes == "1=1" ? teamCodes : $"deptcode in ({teamCodes})";

                var query = $@"SELECT 
                                    empm.EmployeeCode,
                                    empm.Name,
                                    empm.Gender,
                                    empm.Factory,
                                    empm.CorporationCode,
                                    empm.Department,
                                    empm.Position,
                                    empm.Skill,
                                    empm.SkillLevel,
                                    empm.JoinedDate,
                                    empm.UpdatedDate,
                                    empm.LastWorkedDate,
                                    empm.Status,
                                    empm.ImageUrl,
                                    empm.ImageName,
                                    p.stylecode,
                                    p.stylesize,
                                    p.stylecolorserial,
                                    p.revno,
                                    p.oprevno,
                                    p.opserial,
                                    p.mxpackage
                                FROM
                                    mes.t_hr_empm empm
                                        LEFT JOIN
                                    (SELECT 
                                        opdt.EmployeeCode,
                                            opdt.stylecode,
                                            opdt.stylesize,
                                            opdt.stylecolorserial,
                                            opdt.revno,
                                            opdt.oprevno,
                                            opdt.opserial,
                                            opmt.mxpackage
                                    FROM
                                        mes.t_mx_opdt opdt
                                    JOIN mes.t_mx_opmt opmt ON opdt.stylecode = opmt.stylecode
                                        AND opdt.stylesize = opmt.stylesize
                                        AND opdt.stylecolorserial = opmt.stylecolorserial
                                        AND opdt.revno = opmt.revno
                                        AND opdt.oprevno = opmt.oprevno
                                    WHERE
                                        opdt.stylecode = '{styleCode}'
                                            AND opdt.stylesize = '{styleSize}'
                                            AND opdt.stylecolorserial = '{styleColorSerial}'
                                            AND opdt.revno = '{revNo}'
                                            AND opdt.oprevno = '{opRevNo}'
                                            AND opmt.mxpackage = '{mxPackage}') AS p ON empm.EmployeeCode = p.EmployeeCode
                                WHERE
                                    empm.corporationcode = '{corp}'
                                        AND empm.department = '{dept}'
                                        AND {tc};";

            var emps = await _MySqlDBManager.GetAllAsync<Employee>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return emps;
        }

        public async Task<bool> UpdateImageName(string employeeCode, string imageName)
        {
            //var img = imageName == null ? imageName : $@"'{imageName}'";
            var query = $@"UPDATE `mes`.`t_hr_empm`
                                SET `ImageName` = '{imageName}'
                                WHERE `EmployeeCode` = '{employeeCode}';";

            var rsUpdate = await _MySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text);

            return rsUpdate;
        }

        public async Task<List<Employee>> GetEmpNoImage()
        {
            var query = @"SELECT 
                            *
                        FROM
                            mes.t_hr_empm
                        WHERE
                            imagename IS NULL
                                || TRIM(imagename) = '';";

            var emps = await _MySqlDBManager.GetAllAsync<Employee>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return emps;
        }

        public async Task<bool> UpdateNfcId(List<eEmployeesKillsImgUrl> emps)
        {
            string condStr = "";
            string empCodesStr = "";
            for (int i = 0; i < emps.Count; i++)
            {
                condStr += $"WHEN '{emps[i].EMPID}' THEN '{emps[i].UID_FOR_MES}' ";
                empCodesStr += i == emps.Count - 1 ? $"'{emps[i].EMPID}'" : $"'{emps[i].EMPID}',";
            }

            string q = $@"UPDATE `mes`.`t_hr_empm` 
                        SET 
                            `NfcId` = (CASE EmployeeCode
                                {condStr}
                            END)
                        WHERE
                            `EmployeeCode` IN ({empCodesStr});";

            var rsUpdate = await _MySqlDBManager.ExecuteNonQueryAsync(ConstantGeneric.ConnectionStrMesMySql, q,
                CommandType.Text);

            return rsUpdate;
        }
    }
}
