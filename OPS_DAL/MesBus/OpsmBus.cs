using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.MesBus
{
    public class OpsmBus
    {
        public static long SaveOpsm(Opsm opst)
        {
            var v = $"({0},{opst.Name},{opst.GroupId},'{opst.MxPackage}',{opst.OpTime},'{opst.Location}',{opst.Width}," +
                    $"{opst.Height},{opst.Angle})";
            var q = "INSERT INTO `mes`.`t_mx_opst` (`Id`,`Name`,`GroupId`,`MxPackage`,`OpTime`,`Location`," +
                    $"`Width`,`Height`,`Angle`) VALUES {v};";

            var timeLineId = MySqlDBManager.InsertQuery(q, CommandType.Text, null);

            return timeLineId;
        }

        /// <summary>
        /// Gets list of simulation-modules by mxpackage.
        /// </summary>
        /// <param name="mxPackage">The mxpackage.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 21-Dec-19
        public static List<Opsm> GetOpsmsByMxPackage(string mxPackage)
        {
            var q = "select o.GroupId as 'key', o.* from mes.t_mx_opsm o WHERE mxpackage = ?p_mxpackage;";
            var ps = new List<MySqlParameter> { new MySqlParameter("p_mxpackage", mxPackage) };
            var opsms = MySqlDBManager.GetAll<Opsm>(q, CommandType.Text, ps.ToArray());

            return opsms;
        }

        public static bool SaveChangeOpsm(List<Opsm> opsms)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    string idData = "", nameData = "", groupIdData = "", mxPackageData = "", factoryData = "",
                        optimeData = "", locationData = "", widthData = "", heightData = "", angleData = "",
                        bgColorData = "", nextModuleData = "";

                    for (int i = 0; i < opsms.Count; i++)
                    {
                        idData += i != opsms.Count - 1 ? $"{opsms[i].Id}," : $"{opsms[i].Id}";
                        nameData += $"WHEN {opsms[i].Id} THEN '{opsms[i].Name}' ";
                        groupIdData += $"WHEN {opsms[i].Id} THEN '{opsms[i].GroupId}' ";
                        mxPackageData += $"WHEN {opsms[i].Id} THEN '{opsms[i].MxPackage}' ";
                        factoryData += $"WHEN {opsms[i].Id} THEN '{opsms[i].Factory}' ";
                        optimeData += $"WHEN {opsms[i].Id} THEN {opsms[i].OpTime} ";
                        locationData += $"WHEN {opsms[i].Id} THEN '{opsms[i].Location}' ";
                        widthData += $"WHEN {opsms[i].Id} THEN '{opsms[i].Width}' ";
                        heightData += $"WHEN {opsms[i].Id} THEN '{opsms[i].Height}' ";
                        angleData += $"WHEN {opsms[i].Id} THEN '{opsms[i].Angle}' ";
                        bgColorData += $"WHEN {opsms[i].Id} THEN '{opsms[i].BackgroundColor}' ";
                        nextModuleData += $"WHEN {opsms[i].Id} THEN '{opsms[i].NextModule}' ";
                    }
                    string q = "UPDATE mes.t_mx_opsm SET " +
                               $"name = (CASE Id {nameData} END)," +
                               $"groupid = (CASE Id {groupIdData} END)," +
                               $"mxpackage = (CASE Id {mxPackageData} END)," +
                               $"factory = (CASE Id {factoryData} END)," +
                               $"optime = (CASE Id {optimeData} END)," +
                               $"location = (CASE Id {locationData} END)," +
                               $"width = (CASE Id {widthData} END)," +
                               $"height = (CASE Id {heightData} END)," +
                               $"angle = (CASE Id {angleData} END)," +
                               $"backgroundcolor = (CASE Id {bgColorData} END)," +
                               $"nextmodule = (CASE Id {nextModuleData} END)" +
                               $"WHERE Id IN({idData});";
                    using (var myCmd = new MySqlCommand(q, connection, transaction))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static bool SaveOpsms(List<Opsm> opsms)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    var v = "";
                    for (var i = 0; i < opsms.Count; i++)
                    {
                        var vTemp = $"(0,'{opsms[i].Name}','{opsms[i].GroupId}','{opsms[i].MxPackage}'," +
                                    $"'{opsms[i].Factory}',{opsms[i].OpTime},'{opsms[i].Location}',{opsms[i].Width}," +
                                    $"{opsms[i].Height},{opsms[i].Angle},'{opsms[i].BackgroundColor}','{opsms[i].NextModule}')";
                        v += i == opsms.Count - 1 ? $"{vTemp} " : $"{vTemp}, ";
                    }

                    var q = "Insert into t_mx_opsm(`id`,`name`,`groupid`,`mxpackage`,`factory`,`optime`,`location`,`width`," +
                            $"`height`,`angle`,`backgroundcolor`,`nextmodule`) values{v};";

                    using (var myCmd = new MySqlCommand(q, connection, transaction))
                    {
                        myCmd.CommandType = CommandType.Text;
                        myCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
