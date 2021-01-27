using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.MesBus
{
    public class OpstBus
    {
        /// <summary>
        /// Saves the operation simulation time bar.
        /// </summary>
        /// <param name="opst">The opst.</param>
        /// <returns>id of opst</returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 21-Nov-19
        public static long SaveOpst(Opst opst)
        {
            var v = $"({0},{opst.LineSerial},{opst.TableId},'{opst.MxPackage}',{opst.OpTime},'{opst.StartTime}'," +
                    $"'{opst.EndTime}',{opst.TableWidth},{opst.Height},'{opst.Location}',{opst.Angle},{opst.Length},'{opst.Color}', '{opst.GroupId}')";
            var q = "INSERT INTO `mes`.`t_mx_opst` (`timelineid`,`lineserial`,`tableid`,`mxpackage`,`optime`,`starttime`," +
                    $"`endtime`,`tablewidth`,`height`,`location`,`angle`,`length`,`color`,`groupid`) VALUES {v};";

            var timeLineId = MySqlDBManager.InsertQuery(q, CommandType.Text, null);

            return timeLineId;
        }

        /// <summary>
        /// Gets time line (opst) by mxPackage.
        /// </summary>
        /// <param name="mxPackage">The mxpackage.</param>
        /// <returns>List of opsts</returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 22-Nov-19
        public static List<Opst> GetOpstsByMxPackage(string mxPackage)
        {
            var q = "select * from mes.t_mx_opst tl WHERE mxpackage = ?p_mxpackage;";
            var ps = new List<MySqlParameter> { new MySqlParameter("p_mxpackage", mxPackage) };
            var opsts = MySqlDBManager.GetAll<Opst>(q, CommandType.Text, ps.ToArray());

            return opsts;
        }

        /// <summary>
        /// Deletes the opst - timeline bar.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 26-Nov-19
        public static bool DeleteOpst(int id)
        {
            var q = "DELETE FROM mes.t_mx_opst WHERE TimeLineId = ?p_TimeLineId;";
            var ps = new List<MySqlParameter> { new MySqlParameter("p_TimeLineId", id) };
            var effectedRow = MySqlDBManager.ExecuteQuery(q, CommandType.Text, ps.ToArray());

            return int.Parse(effectedRow.ToString()) > 0;
        }

        public static bool SaveChangeOpst(List<Opst> opsts)
        {
            using (var connection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    string timeLineIds = "", lineSerialData = "", tableData = "", mxPackageData = "", opTimeData = "",
                        startTimeData = "", endTimeData = "", widthData = "", heightData = "", locData = "", angleData = "",
                        lengthData = "";

                    for (int i = 0; i < opsts.Count; i++)
                    {
                        lineSerialData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].LineSerial}' ";
                        tableData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].TableId}' ";
                        mxPackageData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].MxPackage}' ";
                        opTimeData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].OpTime}' ";
                        startTimeData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].StartTime}' ";
                        endTimeData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].EndTime}' ";
                        widthData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].TableWidth}' ";
                        heightData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].Height}' ";
                        locData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].Location}' ";
                        angleData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].Angle}' ";
                        lengthData += $"WHEN {opsts[i].TimeLineId} THEN '{opsts[i].Length}' ";

                        if (i != opsts.Count - 1)
                        {
                            timeLineIds += $"'{opsts[i].TimeLineId}',";
                        }
                        else
                        {
                            timeLineIds += $"'{opsts[i].TimeLineId}'";
                        }
                    }
                    string q = "UPDATE mes.t_mx_opst SET " +
                               $"lineSerial = (CASE timeLineId {lineSerialData} END), " +
                               $"tableid = (CASE timeLineId {tableData} END)," +
                               $"mxpackage = (CASE timeLineId {mxPackageData} END)," +
                               $"opTime = (CASE timeLineId {opTimeData} END)," +
                               $"startTime = (CASE timeLineId {startTimeData} END)," +
                               $"endTime = (CASE timeLineId {endTimeData} END)," +
                               $"tablewidth = (CASE timeLineId {widthData} END)," +
                               $"height = (CASE timeLineId {heightData} END)," +
                               $"location = (CASE timeLineId {locData} END)," +
                               $"angle = (CASE timeLineId {angleData} END)," +
                               $"length = (CASE timeLineId {lengthData} END)" +
                               $"WHERE timeLineId IN({timeLineIds});";
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
