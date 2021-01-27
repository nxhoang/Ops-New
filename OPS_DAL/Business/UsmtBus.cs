using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business

{
    public class UsmtBus
    {
        private readonly string _mySqlConn = ConstantGeneric.ConnectionStrMesMySql;
        private readonly MySqlDBManager _MySqlDBManager = new MySqlDBManager();

        #region Tai Le (Thomas) part
        public static List<Usmt> GetUserList(string pKeyword, string DBType)
        {
            // 2019-05-31, Tai Le: (Thomas) 

            /** Usmt Type:
             * 'I' << Indonesia
             * 'K' << Korea 
             * 'B' << Buyer
             * 'C' << China
             * 'A' 
             * 'O' <<Outsource
             * 'S'<< Supplier
             * 'E' <<Employee
             */

            List<Usmt> UserList = new List<Usmt>();

            switch (DBType.ToLower())
            {
                case "oracle":
                    {
                        var dt = new DataTable();
                        dt.Columns.Add("UserName", typeof(String));
                        dt.Columns.Add("Name", typeof(String));

                        using (OracleConnection oracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStr))
                        {
                            oracleConn.Open();

                            //USMT
                            string sql =
                                    " Select UserID AS UserName, UserID || ' - ' || Name as Name " +
                                    " From T_CM_USMT " +
                                    " Where Status = 'OK' " +
                                    " AND Type IN ('E' , 'I') ";

                            //if (!String.IsNullOrEmpty(pKeyword))
                            sql = sql + " AND ( UserID = :Keyword OR UPPER(Name) like '%' || :Keyword || '%'  ) ";

                            OracleCommand oracleCommand = new OracleCommand(sql, oracleConn);
                            //if (!String.IsNullOrEmpty(pKeyword))
                            oracleCommand.Parameters.Add("Keyword", pKeyword.ToUpper());

                            OracleDataReader Reader = oracleCommand.ExecuteReader();

                            if (Reader.HasRows)
                                while (Reader.Read())
                                {
                                    DataRow dr_New = dt.NewRow();

                                    dr_New["UserName"] = Reader["UserName"];
                                    dr_New["Name"] = Reader["Name"];

                                    dt.Rows.Add(dr_New);
                                }

                            Reader.Dispose();
                            oracleCommand.Parameters.Clear();
                            oracleCommand.Dispose();

                            ///////////////////////////////////////////////
                            //HRM VN 
                            sql =
                                " Select EMPID as UserName , EMPID || ' - ' || EMPNAME as Name " +
                                " From T_HR_EMP_MASTER@HRMVNDB  " +
                                " Where 1=1  ";

                            //if (!String.IsNullOrEmpty(pKeyword))
                            sql = sql + " AND (EMPID = :Keyword OR UPPER(EMPNAME) like '%' || :Keyword || '%'   ) ";

                            //oracleCommand = new OracleCommand(sql, oracleConn);

                            ////if (!String.IsNullOrEmpty(pKeyword))
                            //    oracleCommand.Parameters.Add("Keyword", pKeyword.ToUpper());

                            //Reader = oracleCommand.ExecuteReader();

                            //if (Reader.HasRows)
                            //    while (Reader.Read())
                            //    {
                            //        DataRow dr_New = dt.NewRow();

                            //        dr_New["UserName"] = Reader["UserName"];
                            //        dr_New["Name"] = Reader["Name"];

                            //        dt.Rows.Add(dr_New);
                            //    }

                            //Reader.Dispose();
                            //oracleCommand.Parameters.Clear();
                            //oracleCommand.Dispose();


                            ///////////////////////////////////////////////
                            //HRM Indo
                            sql =
                                " Select EMPID as UserName , EMPID || ' - ' || EMPNAME as Name " +
                                " From T_HR_EMP_MASTER@HRMIDDB  " +
                                " Where 1=1  ";

                            //if (!String.IsNullOrEmpty(pKeyword))
                            sql = sql + " AND (EMPID = :Keyword OR UPPER(EMPNAME) like '%' || :Keyword || '%'   ) ";

                            //oracleCommand = new OracleCommand(sql, oracleConn);

                            ////if (!String.IsNullOrEmpty(pKeyword))
                            //    oracleCommand.Parameters.Add("Keyword", pKeyword.ToUpper());

                            //Reader = oracleCommand.ExecuteReader();

                            //if (Reader.HasRows)
                            //    while (Reader.Read())
                            //    {
                            //        DataRow dr_New = dt.NewRow();

                            //        dr_New["UserName"] = Reader["UserName"];
                            //        dr_New["Name"] = Reader["Name"];

                            //        dt.Rows.Add(dr_New);
                            //    }

                            //Reader.Dispose();
                            //oracleCommand.Parameters.Clear();
                            //oracleCommand.Dispose();

                            oracleConn.Close();
                            oracleConn.Dispose();
                        }

                        var FinalDT = OracleDbManager.RemoveDuplicateRows(dt, "UserName");
                        UserList = OracleDbManager.GetObjectsFromDataTable<Usmt>(FinalDT);

                    }
                    break;

                case "mysql":
                    {
                        string sql =
                            " Select UserID AS UserName, UserID || ' - ' || Name as Name " +
                            " From T_CM_USMT " +
                            " Where Status = 'OK' " +
                            " AND Type IN ('E' , 'I') ";

                        if (!String.IsNullOrEmpty(pKeyword))
                            sql = sql + " AND UserID = @Keyword OR UPPER(Name) like '%' || @Keyword || '%'  ";

                        sql = sql + " Order by UserID ";

                        List<MySqlParameter> Parameters = new List<MySqlParameter>();

                        if (!String.IsNullOrEmpty(pKeyword))
                            Parameters.Add(new MySqlParameter("@Keyword", pKeyword.ToUpper()));

                        UserList = MySqlDBManager.GetObjects<Usmt>(sql, CommandType.Text, Parameters.ToArray());
                    }
                    break;
            }

            return UserList;
        }

        public static List<Usmt> GetHRMEmployeeList(string pKeyword, string DBType)
        {
            // 2019-05-31, Tai Le: (Thomas) 

            /** Usmt Type:
             * 'I' << Indonesia
             * 'K' << Korea 
             * 'B' << Buyer
             * 'C' << China
             * 'A' 
             * 'O' <<Outsource
             * 'S'<< Supplier
             * 'E' <<Employee
             */

            List<Usmt> UserList = new List<Usmt>();

            switch (DBType.ToLower())
            {
                case "oracle":
                    {
                        var dt = new DataTable();
                        dt.Columns.Add("UserName", typeof(String));
                        dt.Columns.Add("Name", typeof(String));

                        using (OracleConnection oracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStr))
                        {
                            oracleConn.Open();

                            //USMT
                            string sql =
                                    " Select UserID AS UserName, UserID || ' - ' || Name as Name " +
                                    " From T_CM_USMT " +
                                    " Where Status = 'OK' " +
                                    " AND Type IN ('E' , 'I') ";

                            sql = sql + " AND ( UserID = :Keyword OR UPPER(Name) like '%' || :Keyword || '%'  ) ";

                            OracleCommand oracleCommand = new OracleCommand(sql, oracleConn);
                            oracleCommand.Parameters.Add("Keyword", pKeyword.ToUpper());

                            OracleDataReader Reader = oracleCommand.ExecuteReader();

                            //if (Reader.HasRows)
                            //    while (Reader.Read())
                            //    {
                            //        DataRow dr_New = dt.NewRow(); 
                            //        dr_New["UserName"] = Reader["UserName"];
                            //        dr_New["Name"] = Reader["Name"]; 
                            //        dt.Rows.Add(dr_New);
                            //    }

                            Reader.Dispose();
                            oracleCommand.Parameters.Clear();
                            oracleCommand.Dispose();

                            ///////////////////////////////////////////////
                            //HRM VN 
                            sql =
                                " Select EMPID as UserName , EMPID || ' - ' || EMPNAME as Name " +
                                " From T_HR_EMP_MASTER@HRMVNDB  " +
                                " Where   Status = 'C'  ";

                            sql = sql + " AND (EMPID = :Keyword OR UPPER(EMPNAME) like '%' || :Keyword || '%'   ) ";

                            oracleCommand = new OracleCommand(sql, oracleConn);
                            oracleCommand.Parameters.Add("Keyword", pKeyword.ToUpper());

                            Reader = oracleCommand.ExecuteReader();

                            if (Reader.HasRows)
                                while (Reader.Read())
                                {
                                    DataRow dr_New = dt.NewRow();
                                    dr_New["UserName"] = Reader["UserName"];
                                    dr_New["Name"] = Reader["Name"];
                                    dt.Rows.Add(dr_New);
                                }

                            Reader.Dispose();
                            oracleCommand.Parameters.Clear();
                            oracleCommand.Dispose();


                            ///////////////////////////////////////////////
                            //HRM Indo
                            sql =
                                " Select EMPID as UserName , EMPID || ' - ' || EMPNAME as Name " +
                                " From T_HR_EMP_MASTER@HRMIDDB  " +
                                " Where   Status = 'C'  ";

                            sql = sql + " AND (EMPID = :Keyword OR UPPER(EMPNAME) like '%' || :Keyword || '%'   ) ";

                            oracleCommand = new OracleCommand(sql, oracleConn);
                            oracleCommand.Parameters.Add("Keyword", pKeyword.ToUpper());

                            Reader = oracleCommand.ExecuteReader();

                            if (Reader.HasRows)
                                while (Reader.Read())
                                {
                                    DataRow dr_New = dt.NewRow();
                                    dr_New["UserName"] = Reader["UserName"];
                                    dr_New["Name"] = Reader["Name"];
                                    dt.Rows.Add(dr_New);
                                }

                            Reader.Dispose();
                            oracleCommand.Parameters.Clear();
                            oracleCommand.Dispose();

                            //Close Connection::
                            oracleConn.Close();
                            oracleConn.Dispose();
                        }
                        var FinalDT = OracleDbManager.RemoveDuplicateRows(dt, "UserName");
                        UserList = OracleDbManager.GetObjectsFromDataTable<Usmt>(FinalDT);
                    }
                    break;

                case "mysql":
                    {
                        //string sql =
                        //    " Select UserID AS UserName, UserID || ' - ' || Name as Name " +
                        //    " From T_CM_USMT " +
                        //    " Where Status = 'OK' " +
                        //    " AND Type IN ('E' , 'I') "; 
                        //if (!String.IsNullOrEmpty(pKeyword))
                        //    sql = sql + " AND UserID = @Keyword OR UPPER(Name) like '%' || @Keyword || '%'  "; 
                        //sql = sql + " Order by UserID "; 
                        //List<MySqlParameter> Parameters = new List<MySqlParameter>(); 
                        //if (!String.IsNullOrEmpty(pKeyword))
                        //    Parameters.Add(new MySqlParameter("@Keyword", pKeyword.ToUpper()));   
                        //UserList = MySqlDBManager.GetObjects<Usmt>(sql, CommandType.Text, Parameters.ToArray());
                    }
                    break;
            }

            return UserList;
        }

        public static List<Usmt> GetHRMVN()
        {
            // 2019-05-31, Tai Le: (Thomas)  
            List<Usmt> UserList = new List<Usmt>();

            var dt = new DataTable();
            dt.Columns.Add("UserName", typeof(String));
            dt.Columns.Add("Name", typeof(String));
            dt.Columns.Add("Status", typeof(String));

            using (OracleConnection oracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStr))
            {
                oracleConn.Open();

                ///////////////////////////////////////////////
                //HRM VN 
                string sql =
                    " Select EMPID as UserName , EMPID || ' - ' || EMPNAME as Name , Status " +
                    " From T_HR_EMP_MASTER@HRMVNDB  " +
                    " Where  1=1 ";

                //sql = sql + " AND (EMPID = :Keyword OR UPPER(EMPNAME) like '%' || :Keyword || '%'   ) ";

                OracleCommand oracleCommand = new OracleCommand(sql, oracleConn);
                //oracleCommand.Parameters.Add("Keyword", pKeyword.ToUpper());

                var Reader = oracleCommand.ExecuteReader();

                if (Reader.HasRows)
                    while (Reader.Read())
                    {
                        DataRow dr_New = dt.NewRow();
                        dr_New["UserName"] = Reader["UserName"];
                        dr_New["Name"] = Reader["Name"];
                        dr_New["Status"] = Reader["Status"];
                        dt.Rows.Add(dr_New);
                    }

                Reader.Dispose();
                oracleCommand.Parameters.Clear();
                oracleCommand.Dispose();


                //Close Connection::
                oracleConn.Close();
                oracleConn.Dispose();
            }

            var FinalDT = OracleDbManager.RemoveDuplicateRows(dt, "UserName");
            UserList = OracleDbManager.GetObjectsFromDataTable<Usmt>(FinalDT);

            return UserList;
        }

        public static List<Usmt> GetHRMIndo()
        {
            // 2019-05-31, Tai Le: (Thomas)  
            List<Usmt> UserList = new List<Usmt>();

            var dt = new DataTable();
            dt.Columns.Add("UserName", typeof(String));
            dt.Columns.Add("Name", typeof(String));
            dt.Columns.Add("Status", typeof(String));

            using (OracleConnection oracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStr))
            {
                oracleConn.Open();
                ///////////////////////////////////////////////
                //HRM Indo
                string sql =
                    " Select EMPID as UserName , EMPID || ' - ' || EMPNAME as Name, Status " +
                    " From T_HR_EMP_MASTER@HRMIDDB  " +
                    " Where 1=1 ";

                OracleCommand oracleCommand = new OracleCommand(sql, oracleConn);

                var Reader = oracleCommand.ExecuteReader();

                if (Reader.HasRows)
                    while (Reader.Read())
                    {
                        DataRow dr_New = dt.NewRow();
                        dr_New["UserName"] = Reader["UserName"];
                        dr_New["Name"] = Reader["Name"];
                        dr_New["Status"] = Reader["Status"];
                        dt.Rows.Add(dr_New);
                    }

                Reader.Dispose();
                oracleCommand.Parameters.Clear();
                oracleCommand.Dispose();

                //Close Connection::
                oracleConn.Close();
                oracleConn.Dispose();
            }

            var FinalDT = OracleDbManager.RemoveDuplicateRows(dt, "UserName");
            UserList = OracleDbManager.GetObjectsFromDataTable<Usmt>(FinalDT);

            return UserList;
        }

        public static List<Usmt> GetFullUserList()
        {
            // 2019-05-31, Tai Le: (Thomas) 

            /** Usmt Type:
             * 'I' << Indonesia
             * 'K' << Korea 
             * 'B' << Buyer
             * 'C' << China
             * 'A' 
             * 'O' <<Outsource
             * 'S'<< Supplier
             * 'E' <<Employee
             */

            List<Usmt> UserList = new List<Usmt>();

            var dt = new DataTable();
            dt.Columns.Add("UserName", typeof(String));
            dt.Columns.Add("Name", typeof(String));

            using (OracleConnection oracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStr))
            {
                oracleConn.Open();

                //USMT
                string sql =
                        " Select UserID AS UserName, UserID || ' - ' || Name as Name " +
                        " From T_CM_USMT " +
                        " Where Type IN ('E' , 'I') ";

                OracleCommand oracleCommand = new OracleCommand(sql, oracleConn);
                OracleDataReader Reader = oracleCommand.ExecuteReader();

                if (Reader.HasRows)
                    while (Reader.Read())
                    {
                        DataRow dr_New = dt.NewRow();

                        dr_New["UserName"] = Reader["UserName"];
                        dr_New["Name"] = Reader["Name"];

                        dt.Rows.Add(dr_New);
                    }

                Reader.Dispose();
                oracleCommand.Parameters.Clear();
                oracleCommand.Dispose();

                oracleConn.Close();
                oracleConn.Dispose();
            }

            var FinalDT = OracleDbManager.RemoveDuplicateRows(dt, "UserName");
            UserList = OracleDbManager.GetObjectsFromDataTable<Usmt>(FinalDT);

            return UserList;
        }

        //2020-08-05 Tai Le(Thomas)
        public static bool NewUser(Usmt objUsmt)
        {
            string query = "";
            try
            {
                var ExistedUser = MysqlGetUserInfoByUserID(objUsmt.UserName);

                if (ExistedUser == null)
                {
                    ///T_CM_USMT
                    query = @"
Insert into T_CM_USMT ( `USERID` , `NAME` , `PASSWD` ,  `STATUS` , `REGISTRYDATE` ) 
Values ( @PUSERID , @PNAME , @PPASSWD ,  @PSTATUS , @PREGISTRYDATE)
"; 
                    List<MySqlParameter> paras = new List<MySqlParameter> {
                        new MySqlParameter("PUSERID",objUsmt.UserName),
                        new MySqlParameter("PNAME",objUsmt.Name),
                        new MySqlParameter("PPASSWD",objUsmt.Password),
                        new MySqlParameter("PSTATUS","OK")
                    };

                    MySqlDBManager.ExecuteQuery(query, CommandType.Text, paras.ToArray());
                    paras.Clear();

                    ///T_CM_URMT
                    query = @"
Insert into T_CM_URMT(`USERID` , `ROLEID` )
Values( @PUSERID, @PROLEID)
";
                    for (int I = 0; I < objUsmt.Roles.Length; I++)
                    {

                        paras.Add(new MySqlParameter("PUSERID", objUsmt.UserName));
                        paras.Add(new MySqlParameter("PROLEID", objUsmt.Roles[I]));
                        MySqlDBManager.ExecuteQuery(query, CommandType.Text, paras.ToArray());
                        paras.Clear();
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
                return false;
            }
        }
        public static Usmt MysqlGetUserInfoByUserID(string username)
        {
            var strSql = "SELECT USERID, NAME FROM T_CM_USMT WHERE USERID = @P_USERID ";
            var Params = new List<MySqlParameter>()
            {
                new MySqlParameter("P_USERID", username)
            };

            var usmt = MySqlDBManager.GetObjects<Usmt>(strSql, CommandType.Text, Params.ToArray()).FirstOrDefault();

            return usmt;
        }

        #endregion


        public static Boolean isValidUser(Usmt user)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT");
            sb.AppendLine(" A.USERID,A.NAME, A.EXPIRYDATE "); //SON ADD A.EXPIRYDATE
            sb.AppendLine(" FROM T_CM_USMT A ");
            sb.AppendLine(" WHERE A.USERID = '" + user.UserName + "'");
            sb.AppendLine(" AND  A.PASSWD = '" + user.Password + "'");
            sb.AppendLine(" AND  A.STATUS = 'OK'"); //SON ADD A.STATUS

            var result = OracleDbManager.GetObjects<Usmt>(sb.ToString(), null);

            if (result.ToList().Count == 0)
                return false;

            //START ADD) SON - 2018/12/06 - Check expiry date of user account
            var usmt = result.FirstOrDefault();
            if (!string.IsNullOrEmpty(usmt.ExpiryDate.ToString()) && usmt.ExpiryDate < DateTime.Now)
                return false;
            //END ADD) SON - 2018/12/06 - Check expiry date of user account

            user.Name = result.FirstOrDefault().Name;

            return true;
        }

        public static Boolean isValidUserMySql(Usmt user)
        {
            var sb = new StringBuilder();
            sb.AppendLine(" SELECT ");
            sb.AppendLine(" A.USERID,A.NAME, A.EXPIRYDATE "); //SON ADD A.EXPIRYDATE
            sb.AppendLine(" FROM T_CM_USMT A ");
            sb.AppendLine(" WHERE A.USERID = '" + user.UserName + "'");
            sb.AppendLine(" AND  A.PASSWD = '" + user.Password + "'");
            sb.AppendLine(" AND  A.STATUS = 'OK'"); //SON ADD A.STATUS

            //var result = OracleDbManager.GetObjects<Usmt>(sb.ToString(), null);
            var result = MySqlDBManager.GetObjects<Usmt>(sb.ToString(), CommandType.Text, null);

            if (result.ToList().Count == 0)
                return false;

            //START ADD) SON - 2018/12/06 - Check expiry date of user account
            var usmt = result.FirstOrDefault();
            if (!string.IsNullOrEmpty(usmt.ExpiryDate.ToString()) && usmt.ExpiryDate < DateTime.Now)
                return false;
            //END ADD) SON - 2018/12/06 - Check expiry date of user account

            user.Name = result.FirstOrDefault().Name;

            return true;
        }

        /// <summary>
        /// Update email
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="oraConn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static Boolean UpdateEmail(string username, string email)
        {
            var strSql = "UPDATE T_CM_USMT SET EMAIL = :P_EMAIL WHERE USERID = :P_USERID ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_EMAIL", email),
                new OracleParameter("P_USERID", username)
            };

            var result = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text);

            return result != null;
        }

        /// <summary>
        /// Get user information by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Usmt GetUserInfoByUsername(string username)
        {
            var strSql = "SELECT USERID, NAME, TEL, EMAIL, SEX FROM T_CM_USMT WHERE USERID = :P_USERID ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_USERID", username)
            };

            var usmt = OracleDbManager.GetObjects<Usmt>(strSql, CommandType.Text, oracleParams.ToArray()).FirstOrDefault();

            return usmt;
        }


        /// <summary>
        /// Get user information by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>usmt</returns>
        /// Author: VitHV
        public static Usmt GetUserInfo(string username)
        {
            var strSql = "SELECT USERID, NAME, TEL, EMAIL, SEX FROM T_CM_USMT WHERE USERID = :P_USERID ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_USERID", username)
            };

            var usmt = OracleDbManager.GetObjects<Usmt>(strSql, CommandType.Text, oracleParams.ToArray()).FirstOrDefault();

            return usmt;
        }

        /// <summary>
        /// Get user information by username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pass">password</param>
        /// <returns>usmt</returns>
        /// Author: VitHV
        public static bool CheckPass(string username, string pass)
        {
            var strSql = "SELECT USERID, PASSWD Password FROM T_CM_USMT WHERE USERID = :P_USERID ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_USERID", username)
            };

            var usmt = OracleDbManager.GetObjects<Usmt>(strSql, CommandType.Text, oracleParams.ToArray()).FirstOrDefault();

            return usmt.Password == pass;
        }

        /// <summary>
        /// Change password user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pass">password</param>
        /// <returns>success is true else is false</returns>
        /// Author: VitHV
        public static bool ChangePass(string username, string pass)
        {
            var strSql = "UPDATE T_CM_USMT SET PASSWD = :P_PASSWD  WHERE USERID = :P_USERID ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_PASSWD", pass),
                new OracleParameter("P_USERID", username)
            };

            var rs = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text);

            return rs.ToString() == "1";
        }

        /// <summary>
        /// Change information by username
        /// </summary>
        /// <param name="username"></param>
        /// <param name="Name">Name</param>
        /// <param name="Email">Email</param>
        /// <param name="Tel">Tel</param>
        /// <param name="Sex">Sex</param>
        /// <returns>success is true else is false</returns>
        /// Author: VitHV
        public static bool ChangeUserInfor(string username, string Name, string Email, string Tel, string Sex)
        {
            var strSql = @"UPDATE T_CM_USMT SET NAME = :P_NAME,EMAIL= :P_EMAIL, TEL= :P_TEL, SEX= :P_SEX
                 WHERE USERID = :P_USERID ";
            var oracleParams = new List<OracleParameter>()
            {
                new OracleParameter("P_NAME", Name),
                new OracleParameter("P_EMAIL", Email),
                new OracleParameter("P_TEL", Tel),
                new OracleParameter("P_SEX", Sex),
                new OracleParameter("P_USERID", username)
            };

            var rs = OracleDbManager.ExecuteQuery(strSql, oracleParams.ToArray(), CommandType.Text);

            return rs.ToString() == "1";
        }

        /// <summary>
        /// Get list of users by list factories.
        /// </summary>
        /// <param name="factories">List of factories</param>
        /// <returns></returns>
        public async Task<List<Usmt>> GetByFactories(List<FactoryEntity> factories)
        {
            string conQuery = "";
            string factoriesStr = "";

            if (factories != null && factories.Count > 0)
            {
                for (int i = 0; i < factories.Count; i++)
                {
                    factoriesStr += i == factories.Count - 1 ? $"'{factories[i].FactoryId}'" : $"'{factories[i].FactoryId}',";
                }

                conQuery = $"WHERE factory IN({factoriesStr})";
            }

            string query = $@"SELECT 
                                *
                            FROM
                                mes.t_cm_usmt {conQuery};";

            var usmts = await _MySqlDBManager.GetAllAsync<Usmt>(_mySqlConn, query, CommandType.Text, null);

            return usmts;
        }
    }
}
