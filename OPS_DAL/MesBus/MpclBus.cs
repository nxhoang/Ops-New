using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class MpclBus
    {
        /// <summary>
        /// Insert check list with transaction
        /// </summary>
        /// <param name="mpcl"></param>
        /// <param name="myTrans"></param>
        /// <param name="myCon"></param>
        /// <returns></returns>
        private static bool InsertProReadinessCheckList(Mpcl mpcl, MySqlTransaction myTrans, MySqlConnection myCon)
        {
            string strSql = @" INSERT INTO T_MX_MPCL 
                                (MXPACKAGE, CHECKLISTID, CONFIRMER, CONFIRMTIME)
                                VALUES (?P_MXPACKAGE, ?P_CHECKLISTID, ?P_CONFIRMER, SYSDATE()) ";

            var par = new List<MySqlParameter>() {
                new MySqlParameter("P_MXPACKAGE", mpcl.MxPackage),
                new MySqlParameter("P_CHECKLISTID", mpcl.CheckListId),
                new MySqlParameter("P_CONFIRMER", mpcl.Confirmer)
            };

            var result = MySqlDBManager.ExecuteQueryWithTrans(strSql, par.ToArray(), CommandType.Text, myTrans, myCon);

            return result != null;
        }

        /// <summary>
        /// Delete check list by mx package with transaction
        /// </summary>
        /// <param name="mxPackage"></param>
        /// <param name="myTrans"></param>
        /// <param name="myCon"></param>
        /// <returns></returns>
        private static bool DeleteProReadinessCheckList(string mxPackage, MySqlTransaction myTrans, MySqlConnection myCon)
        {
            string strSql = @" DELETE FROM T_MX_MPCL WHERE MXPACKAGE = ?P_MXPACKAGE ";

            var oraParams = new List<MySqlParameter>() {
                new MySqlParameter("P_MXPACKAGE", mxPackage)
            };

            var result = MySqlDBManager.ExecuteQueryWithTrans(strSql, oraParams.ToArray(), CommandType.Text, myTrans, myCon);

            return result != null;
        }

        /// <summary>
        /// Insert list check list
        /// </summary>
        /// <param name="listMpcl"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertListProReadiness(string mxPackage, List<Mpcl> listMpcl)
        {
            //Check check list before insert
            if (listMpcl == null || listMpcl.Count == 0) {
                return DeleteMesCheckList(mxPackage);                
            };
        
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    //Delete old check list and then insert the new check list
                    if ((DeleteProReadinessCheckList(mxPackage, myTrans, myConnection)))
                    {
                        foreach (var mpcl in listMpcl)
                        {
                            InsertProReadinessCheckList(mpcl, myTrans, myConnection);
                        }                        
                    }
                    else
                    {
                        myTrans.Rollback();
                        return false;
                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    myTrans.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// Insert check list readiness
        /// </summary>
        /// <param name="mpcl"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool InsertReadinessCheckList(Mpcl mpcl)
        {
            string strSql = @" INSERT INTO T_MX_MPCL 
                                (MXPACKAGE, CHECKLISTID, CONFIRMER, CONFIRMTIME)
                                VALUES (?P_MXPACKAGE, ?P_CHECKLISTID, ?P_CONFIRMER, SYSDATE()) ";

            var par = new List<MySqlParameter>() {
                new MySqlParameter("P_MXPACKAGE", mpcl.MxPackage),
                new MySqlParameter("P_CHECKLISTID", mpcl.CheckListId),
                new MySqlParameter("P_CONFIRMER", mpcl.Confirmer)
            };

            //var result = OracleDbManager.ExecuteQuery(strSql, oraParams.ToArray(), CommandType.Text);
            var result = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, par.ToArray());

            return result != null;
        }

        /// <summary>
        /// Get list of production readiness by mes package
        /// </summary>
        /// <param name="mxPackage"></param>
        /// <returns></returns>
        public static List<Mpcl> GetProductionReadinessList(string mxPackage)
        {
            string strSql = @" SELECT MPC.MXPACKAGE, MPC.CHECKLISTID, MPC.CONFIRMER, MPC.CONFIRMTIME
                                FROM T_MX_MPCL MPC
                                WHERE MPC.MXPACKAGE = ?P_MXPACKAGE ";

            var oraParams = new List<MySqlParameter>() {
                new MySqlParameter("P_MXPACKAGE", mxPackage)
            };

            //var lstFcmt = OracleDbManager.GetObjects<Mpcl>(strSql, CommandType.Text, oraParams.ToArray());
            var lstFcmt = MySqlDBManager.GetObjects<Mpcl>(strSql, CommandType.Text, oraParams.ToArray());

            return lstFcmt;
        }

        /// <summary>
        /// Delete check list of production readiness
        /// </summary>
        /// <param name="mxPackage"></param>
        /// <returns></returns>
        public static bool DeleteMesCheckList(string mxPackage)
        {
            string strSql = @" DELETE FROM T_MX_MPCL WHERE MXPACKAGE = ?P_MXPACKAGE ";

            var oraParams = new List<MySqlParameter>() {
                new MySqlParameter("P_MXPACKAGE", mxPackage)
            };
            
            var result = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, oraParams.ToArray());

            return result != null;
        }

        /// <summary>
        /// Delete check list with check list id
        /// </summary>
        /// <param name="mxPackage"></param>
        /// <param name="checkListId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool DeleteMesCheckList(string mxPackage, string checkListId)
        {
            string strSql = @" DELETE FROM T_MX_MPCL WHERE MXPACKAGE = ?P_MXPACKAGE AND CHECKLISTID = ?P_CHECKLISTID ";

            var oraParams = new List<MySqlParameter>() {
                new MySqlParameter("P_MXPACKAGE", mxPackage),
                new MySqlParameter("P_CHECKLISTID", checkListId)
            };

            var result = MySqlDBManager.ExecuteQuery(strSql, CommandType.Text, oraParams.ToArray());

            return result != null;
        }
    }
}
