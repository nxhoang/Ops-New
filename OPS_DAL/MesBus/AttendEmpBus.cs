using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class AttendEmpBus
    {
        private readonly MySqlDBManager MySqlDBManager = new MySqlDBManager();

        public bool Insert(eEmployeeDailyWorking eEmp, string attDate)
        {
            var qr = "INSERT INTO mes.t_hr_attemp " +
                     "(`id`,`factory`, `deptcode`,`employeeid`,`attdate`,`checkin`,`checkout`,`workkind`,`workshift`,`workshiftcode`)" +
                  $"VALUES (0, '{eEmp.EMPLOYEEID}','{attDate}','{eEmp.EMPLOYEEID}','{attDate}','{eEmp.CHECKIN}','{eEmp.CHECKOUT}','{eEmp.WORKKIND}','{eEmp.WORKSHIFT}','{eEmp.WORKSHIFTCODE}';)";

            var result = MySqlDBManager.InsertQuery(qr, CommandType.Text, null);
            return result > 0;
        }

        public async Task<List<AttendEmp>> GetByFactoryDeptDate(string factory, string deptCode, string attDate, bool isPresent)
        {
            var qCheckIn = isPresent ? @"AND ae.checkin IS NOT NULL AND TRIM(ae.checkin) != ''" : "";
            var query = $@"SELECT 
                                    ae.id,
                                    ae.factory,
                                    ae.deptcode,
                                    d.department,
                                    ae.employeeid as EmployeeCode,
                                    ae.fullname as Name,
                                    ae.attdate,
                                    ae.checkin,
                                    ae.checkout,
                                    ae.workkind,
                                    ae.workshift,
                                    ae.workshiftcode
                                FROM
                                    mes.t_hr_attemp ae
                                JOIN
                                    mes.t_hr_dept d ON ae.deptcode = d.deptcode
                                WHERE
                                    ae.factory = '{factory}' AND ae.deptcode = '{deptCode}'
                                        AND ae.attdate = '{attDate}' {qCheckIn};";

            var attendEmps = await MySqlDBManager.GetAllAsync<AttendEmp>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return attendEmps;
        }

        public async Task<List<AttendEmp>> GetByFactoryDeptDate(string factory, string[] deptCode, string attDate, bool isPresent)
        {
            var teams = CommonMethod.GetWhereCondition(deptCode);

            var qCheckIn = isPresent ? @"AND ae.checkin IS NOT NULL AND TRIM(ae.checkin) != ''" : "";
            var query = $@"SELECT 
                                    ae.id,
                                    ae.factory,
                                    ae.deptcode,
                                    d.department,
                                    ae.employeeid as EmployeeCode,
                                    ae.fullname as Name,
                                    ae.attdate,
                                    ae.checkin,
                                    ae.checkout,
                                    ae.workkind,
                                    ae.workshift,
                                    ae.workshiftcode,
                                    e.Position,
                                    e.ImageName
                                FROM
                                    mes.t_hr_attemp ae
                                JOIN
                                    mes.t_hr_dept d ON ae.deptcode = d.deptcode
                                join mes.t_hr_empm e on ae.employeeid = e.EmployeeCode
                                WHERE
                                    ae.factory = '{factory}' AND ae.deptcode in ({teams})
                                        AND ae.attdate = '{attDate}' {qCheckIn};";

            var attendEmps = await MySqlDBManager.GetAllAsync<AttendEmp>(ConstantGeneric.ConnectionStrMesMySql, query,
                CommandType.Text, null);

            return attendEmps;
        }

        /// <summary>
        /// Inserting list of attendance employees to db
        /// </summary>
        /// <param name="eEmps">attendance employees</param>
        /// <param name="attDate">attendance date</param>
        /// <returns></returns>
        public async Task<bool> BulkInsert(List<eEmployeeDailyWorking> eEmps, string factory, string deptCode, string attDate)
        {
            string qValue = "";

            for (var i = 0; i < eEmps.Count; i++)
            {
                qValue += $"(0, '{factory}','{deptCode}','{eEmps[i].EMPLOYEEID}','{eEmps[i].FULLNAME}','{attDate}'," +
                          $"'{eEmps[i].CHECKIN}','{eEmps[i].CHECKOUT}','{eEmps[i].WORKKIND}','{eEmps[i].WORKSHIFT}'," +
                          $"'{eEmps[i].WORKSHIFTCODE}')";
                if (i != eEmps.Count - 1) qValue += ",";
            }
            string q = $@"INSERT INTO `mes`.`t_hr_attemp`
                        (`id`,`factory`,`deptcode`,`employeeid`,`fullname`,`attdate`,`checkin`,`checkout`,`workkind`,`workshift`,`workshiftcode`)
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
    }
}
