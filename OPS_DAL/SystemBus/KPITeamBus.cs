using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

using OPS_DAL.SystemEntities;
using OPS_DAL.DAL;

namespace OPS_DAL.SystemBus
{
    public class KPITeamBus
    {
        public static List<Corporation> GetKPITeamList(string DBType)
        {
            List<Corporation> Corporations = new List<Corporation>();

            const string sql =
                         " SELECT T_CM_MCMT.S_CODE ,  T_CM_MCMT.CODE_NAME  " +
                         " FROM T_CM_MCMT " +
                         " WHERE M_CODE = 'Team' " +
                         " ORDER BY T_CM_MCMT.S_CODE " +
                         "";

            switch (DBType.ToLower())
            {
                case "oracle":
                    {
                        Corporations = OracleDbManager.GetObjects<Corporation>(sql, CommandType.Text, null);
                    }
                    break;

                case "mysql":
                    {
                        Corporations = MySqlDBManager.GetObjects<Corporation>(sql, CommandType.Text, null);
                    }
                    break;
            }

            return Corporations;
        }

        public static bool AddKPISetting(string pType, string selected_System, string selected_Corporation, string selected_KPITeam, string KPISeniorData,
            string selected_Buyer, string selected_Factory, string KPIManagerData, string KPILocalMgrData,
            string selected_Menu, string KPIPrimaryData, string KPIStaffData, out string Message)
        {
            /**
             * Validation Rule
             *      KPISeniorData (Director) ; KPIManagerData ; KPILocalMgrData ; KPIPrimaryData  << MUST Exist on HRM_VN/HRM_In/USMT with Status = "C"
             *      KPIStaffData  << MUST Exist on USMT with Status = "OK" (note: check the New EmpID and Old EmpID) 
             */


            /**
             * pType : {ByUI ; ByTemplate}
             * pType == "ByTemplate"
             *      >> Convert { selected_KPITeam ; selected_Corporation ; selected_Menu } from Text to ID
             */

            Message = "";
            //bool IsActiveEmp = true;
            try
            {
                //Do converting
                if (pType == "ByTemplate")
                {
                    //Team (it's NOT SaleTeam)
                    var arrTeam = GetKPITeamList("oracle");
                    var filterTeam = arrTeam.Find(x => x.CODE_NAME == selected_KPITeam);

                    if (filterTeam != null)
                        selected_KPITeam = filterTeam.S_CODE;

                    //Menu
                    var arrselected_Menu = selected_Menu.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    var arrSystemMenu = OPS_DAL.SystemBus.MenuBus.GetSystemMenuList("oracle");
                    var filterSystemMenu = arrSystemMenu.Find(x => (x.SYSTEM_ID == arrselected_Menu[0].Trim() && x.MENU_NAME == arrselected_Menu[1].Trim()));

                    if (filterSystemMenu != null)
                        selected_Menu = filterSystemMenu.MENU_ID;
                }


                string KPI_MASTERID = "";

                using (OracleConnection OracleConn = new OracleConnection(OPS_Utils.ConstantGeneric.ConnectionStrMes))
                {
                    OracleConn.Open();

                    //** MASTER     T_KP_KPIM
                    //OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(
                    //    " SELECT * " +
                    //    " FROM T_KP_KPIM " +
                    //    " WHERE SYSTEM_ID = :SYSTEMID " +
                    //    " AND TEAM = :TEAM " +
                    //    " AND CORPORATION = :CORPORATION " +
                    //    " AND  DIRECTOR = :DIRECTOR  ", OracleConn);

                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(
                            " SELECT * " +
                            " FROM T_KP_KPIM " +
                            " WHERE SYSTEM_ID = :SYSTEMID " +
                            " AND TEAM = :TEAM " +
                            " AND CORPORATION = :CORPORATION " +
                            " ", OracleConn);

                    oracleDataAdapter.SelectCommand.Parameters.Add("SYSTEMID", selected_System);
                    oracleDataAdapter.SelectCommand.Parameters.Add("TEAM", selected_KPITeam);
                    oracleDataAdapter.SelectCommand.Parameters.Add("CORPORATION", selected_Corporation);
                    //oracleDataAdapter.SelectCommand.Parameters.Add("DIRECTOR", KPISeniorData);
                    DataTable dt = new DataTable();
                    oracleDataAdapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        //Add Record into T_KP_KPIM
                        //GET MasterID
                        KPI_MASTERID = NewMasterID();

                        DataRow drNew = dt.NewRow();
                        drNew["MASTERID"] = KPI_MASTERID;
                        drNew["SYSTEM_ID"] = selected_System;
                        drNew["TEAM"] = selected_KPITeam;
                        drNew["CORPORATION"] = selected_Corporation;

                        if (!String.IsNullOrEmpty(KPISeniorData))
                        {
                            drNew["DIRECTOR"] = KPISeniorData;

                            if (!IsActiveEmp(KPISeniorData, "HRM"))
                                drNew["EXPIRY_DATE"] = DateTime.Today.AddDays(-1);
                            else
                                drNew["START_DATE"] = DateTime.Now;
                        }

                        dt.Rows.Add(drNew);

                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(dt);
                        oracleCommandBuilder.Dispose();

                        //Return the KPI_MASTERID for Adding Detail table ""
                    }
                    else
                        KPI_MASTERID = dt.Rows[0]["MASTERID"].ToString();

                    dt.Clear();
                    dt.Dispose();
                    oracleDataAdapter.SelectCommand.Parameters.Clear();
                    oracleDataAdapter.Dispose();


                    //** DETAIL     T_KP_KPIP
                    int KPIP_SEQNO = 0;

                    oracleDataAdapter = new OracleDataAdapter(
                        " SELECT * FROM T_KP_KPIP " +
                        " WHERE MASTERID = :MASTERID " +
                        " AND NVL(BUYER, 'PK_') = :BUYER " +
                        " AND NVL(FACTORY, 'PK_') = :FACTORY " +
                        " AND NVL(MENU, 'PK_') = :MENU ", OracleConn);
                    oracleDataAdapter.SelectCommand.Parameters.Add("MASTERID", String.IsNullOrEmpty(KPI_MASTERID) ? "PK_" : KPI_MASTERID);
                    oracleDataAdapter.SelectCommand.Parameters.Add("BUYER", String.IsNullOrEmpty(selected_Buyer) ? "PK_" : selected_Buyer);
                    oracleDataAdapter.SelectCommand.Parameters.Add("FACTORY", String.IsNullOrEmpty(selected_Factory) ? "PK_" : selected_Factory);
                    oracleDataAdapter.SelectCommand.Parameters.Add("MENU", String.IsNullOrEmpty(selected_Menu) ? "PK_" : selected_Menu);
                    dt = new DataTable();
                    oracleDataAdapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        //Add Record into T_KP_KPIP
                        KPIP_SEQNO = NextKPIP_SEQNO(KPI_MASTERID);

                        DataRow drNew = dt.NewRow();
                        drNew["MASTERID"] = KPI_MASTERID;
                        drNew["SEQNO"] = KPIP_SEQNO;
                        drNew["BUYER"] = selected_Buyer;
                        drNew["FACTORY"] = selected_Factory;
                        drNew["MENU"] = selected_Menu;

                        dt.Rows.Add(drNew);

                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(dt);
                        oracleCommandBuilder.Dispose();
                    }
                    else
                        KPIP_SEQNO = Convert.ToInt32(dt.Rows[0]["SEQNO"].ToString());

                    dt.Clear();
                    dt.Dispose();
                    oracleDataAdapter.SelectCommand.Parameters.Clear();
                    oracleDataAdapter.Dispose();


                    //** USERS      T_KP_KPIR   
                    //string[] arrKPIManagerData = KPIManagerData.Split(';');
                    var KPIManagerDataList = KPIManagerData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    //string[] arrKPILocalMgrData = KPILocalMgrData.Split(';');
                    var KPILocalMgrDataList = KPILocalMgrData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    //string[] arrKPIPrimaryData = KPIPrimaryData.Split(';');
                    var KPIPrimaryDataList = KPIPrimaryData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                    //string[] arrKPIStaffData = KPIStaffData.Split(';');
                    var KPIStaffDataList = KPIStaffData.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();


                    oracleDataAdapter = new OracleDataAdapter(
                        " SELECT * " +
                        " FROM T_KP_KPIR " +
                        " WHERE MASTERID = :MASTERID " +
                        " AND KPIP_SEQNO = :KPIPSEQNO " +
                        " ORDER BY POSITION ", OracleConn);
                    oracleDataAdapter.SelectCommand.Parameters.Add("MASTERID", KPI_MASTERID);
                    oracleDataAdapter.SelectCommand.Parameters.Add("KPIPSEQNO", KPIP_SEQNO);
                    dt = new DataTable();
                    oracleDataAdapter.Fill(dt);

                    int intI = -1;
                    if (dt.Rows.Count > 0)
                    {
                        //Remove the Select User if they already EXIST 
                        foreach (DataRow dr in dt.Rows)
                        {
                            string USERID = dr["USERID"].ToString();
                            string POSITION = dr["POSITION"].ToString();

                            //Exclude the EXISTING User
                            switch (POSITION)
                            {
                                case "Manager":
                                    for (int i = KPIManagerDataList.Count - 1; i >= 0; i--)
                                    {
                                        if (USERID == KPIManagerDataList[i])
                                            KPIManagerDataList.RemoveAt(i);
                                    }
                                    break;

                                case "LocalManager":
                                    for (int i = KPILocalMgrDataList.Count - 1; i >= 0; i--)
                                    {
                                        if (USERID == KPILocalMgrDataList[i])
                                            KPILocalMgrDataList.RemoveAt(i);
                                    }
                                    break;

                                case "Primary":
                                    for (int i = KPIPrimaryDataList.Count - 1; i >= 0; i--)
                                    {
                                        if (USERID == KPIPrimaryDataList[i])
                                            KPIPrimaryDataList.RemoveAt(i);
                                    }
                                    break;

                                case "Incharge":
                                    for (int i = KPIStaffDataList.Count - 1; i >= 0; i--)
                                    {
                                        if (USERID == KPIStaffDataList[i])
                                            KPIStaffDataList.RemoveAt(i);
                                    }
                                    break;
                            }
                        }
                    }

                    //Add the Rest 
                    intI = GetNextSeqNo(KPI_MASTERID, KPIP_SEQNO);
                    DataRow drNew_;

                    KPIManagerDataList = KPIManagerDataList.Distinct().ToList();
                    for (int i = KPIManagerDataList.Count - 1; i >= 0; i--)
                    {
                        if (!String.IsNullOrEmpty(KPIManagerDataList[i]))
                        {
                            drNew_ = dt.NewRow();
                            intI += 1;

                            drNew_["MASTERID"] = KPI_MASTERID;
                            drNew_["KPIP_SEQNO"] = KPIP_SEQNO;
                            drNew_["SEQNO"] = intI;
                            drNew_["USERID"] = KPIManagerDataList[i];
                            drNew_["USE_YN"] = "Y";

                            if (!IsActiveEmp(KPIManagerDataList[i], "HRM"))
                                drNew_["EXPIRY_DATE"] = DateTime.Today.AddDays(-1);
                            else
                                drNew_["START_DATE"] = DateTime.Now;

                            drNew_["POSITION"] = "Manager";

                            dt.Rows.Add(drNew_);
                        }

                    }


                    KPILocalMgrDataList = KPILocalMgrDataList.Distinct().ToList();
                    for (int i = KPILocalMgrDataList.Count - 1; i >= 0; i--)
                    {
                        if (!String.IsNullOrEmpty(KPILocalMgrDataList[i]))
                        {
                            drNew_ = dt.NewRow();
                            intI += 1;

                            drNew_["MASTERID"] = KPI_MASTERID;
                            drNew_["KPIP_SEQNO"] = KPIP_SEQNO;
                            drNew_["SEQNO"] = intI;
                            drNew_["USERID"] = KPILocalMgrDataList[i];
                            drNew_["USE_YN"] = "Y";

                            if (!IsActiveEmp(KPILocalMgrDataList[i], "HRM"))
                                drNew_["EXPIRY_DATE"] = DateTime.Today.AddDays(-1);
                            else
                                drNew_["START_DATE"] = DateTime.Now;

                            drNew_["POSITION"] = "LocalManager";

                            dt.Rows.Add(drNew_);
                        }
                    }


                    KPIPrimaryDataList = KPIPrimaryDataList.Distinct().ToList();
                    for (int i = KPIPrimaryDataList.Count - 1; i >= 0; i--)
                    {
                        if (!String.IsNullOrEmpty(KPIPrimaryDataList[i]))
                        {
                            drNew_ = dt.NewRow();
                            intI += 1;

                            drNew_["MASTERID"] = KPI_MASTERID;
                            drNew_["KPIP_SEQNO"] = KPIP_SEQNO;
                            drNew_["SEQNO"] = intI;
                            drNew_["USERID"] = KPIPrimaryDataList[i];
                            drNew_["USE_YN"] = "Y";

                            if (!IsActiveEmp(KPIPrimaryDataList[i], "HRM"))
                                drNew_["EXPIRY_DATE"] = DateTime.Today.AddDays(-1);
                            else
                                drNew_["START_DATE"] = DateTime.Now;

                            drNew_["POSITION"] = "Primary";

                            dt.Rows.Add(drNew_);
                        }
                    }


                    KPIStaffDataList = KPIStaffDataList.Distinct().ToList();
                    for (int i = KPIStaffDataList.Count - 1; i >= 0; i--)
                    {
                        if (!String.IsNullOrEmpty(KPIStaffDataList[i]))
                        {
                            drNew_ = dt.NewRow();
                            intI += 1;

                            drNew_["MASTERID"] = KPI_MASTERID;
                            drNew_["KPIP_SEQNO"] = KPIP_SEQNO;
                            drNew_["SEQNO"] = intI;
                            drNew_["USERID"] = KPIStaffDataList[i];
                            drNew_["USE_YN"] = "Y";

                            if (!IsActiveEmp(KPIStaffDataList[i], "USMT"))
                                drNew_["EXPIRY_DATE"] = DateTime.Today.AddDays(-1);
                            else
                                drNew_["START_DATE"] = DateTime.Now;

                            drNew_["POSITION"] = "Incharge";

                            dt.Rows.Add(drNew_);
                        }
                    }

                    OracleCommandBuilder oracleCommandBuilder_ = new OracleCommandBuilder(oracleDataAdapter);
                    oracleDataAdapter.Update(dt);
                    oracleCommandBuilder_.Dispose();

                    dt.Clear();
                    dt.Dispose();
                    oracleDataAdapter.SelectCommand.Parameters.Clear();
                    oracleDataAdapter.Dispose();

                    OracleConn.Close();
                    OracleConn.Dispose();
                }

                Message = "KPI Setting Added";
                return true;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                return false;
            }
        }

        public static bool IsActiveEmp(string UserID, string pType)
        {
            try
            {
                //Get HRM_VN
                var HRMVNList = OPS_DAL.Business.UsmtBus.GetHRMVN();
                //Get HRM_Indo
                var HRMInList = OPS_DAL.Business.UsmtBus.GetHRMIndo();
                //Get UMST
                var USMT = OPS_DAL.Business.UsmtBus.GetFullUserList();

                bool IsActiveEmp = true;


                if (pType == "USMT")
                {
                    var KPISeniorDataEmp = USMT.Find(x => x.UserName == UserID);
                    if (KPISeniorDataEmp != null)
                    {
                        if (KPISeniorDataEmp.Status != "OK")
                            IsActiveEmp = false;
                    }
                    else
                        IsActiveEmp = false;
                }
                else if (pType == "HRM")
                {
                    //HRM_VN
                    var KPISeniorDataEmp = HRMVNList.Find(x => x.UserName == UserID);
                    if (KPISeniorDataEmp != null)
                    {
                        if (KPISeniorDataEmp.Status != "C")
                            IsActiveEmp = false;
                    }
                    else
                    {
                        //HRM_Indo
                        KPISeniorDataEmp = HRMInList.Find(x => x.UserName == UserID);
                        if (KPISeniorDataEmp != null)
                        {
                            if (KPISeniorDataEmp.Status != "C")
                                IsActiveEmp = false;
                        }
                        else
                        {
                            //USMT
                            KPISeniorDataEmp = USMT.Find(x => x.UserName == UserID);
                            if (KPISeniorDataEmp != null)
                            {
                                if (KPISeniorDataEmp.Status != "OK")
                                    IsActiveEmp = false;
                            }
                            else
                                IsActiveEmp = false;
                        }
                    }

                }


                return IsActiveEmp;
            }
            catch
            {
                return false;
            }
        }

        private static int GetNextSeqNo(string KPI_MASTERID, int KPIP_SEQNO)
        {
            string strSQL = " Select SEQNO From T_KP_KPIR Where MASTERID = :MASTERID AND KPIP_SEQNO = :KPIP_SEQNO Order By SEQNO DESC ";
            string SEQNO = "0";
            //DataTable dt = new DataTable();
            List<OracleParameter> oracleParameters = new List<OracleParameter>
            {
                new OracleParameter("MASTERID", KPI_MASTERID ),
                new OracleParameter("KPIP_SEQNO", KPIP_SEQNO )
            };

            var dt = OracleDbManager.Query(strSQL, oracleParameters.ToArray(), OPS_Utils.ConstantGeneric.ConnectionStrMes);

            int NextSeqNo = 1;
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0][0] != null)
                    if (!String.IsNullOrEmpty(dt.Rows[0][0].ToString()))
                        SEQNO = dt.Rows[0][0].ToString();

                NextSeqNo = Convert.ToInt32(SEQNO) + 1;
                dt.Dispose();
            }

            return NextSeqNo;
        }

        private static int NextKPIP_SEQNO(string MASTERID)
        {
            string strSQL = "Select SEQNO From T_KP_KPIP Where MASTERID = :MASTERID Order By SEQNO DESC ";
            string SEQNO = "0";
            //DataTable dt = new DataTable();
            List<OracleParameter> oracleParameters = new List<OracleParameter>
            {
                new OracleParameter("MASTERID", MASTERID )
            };

            var dt = OracleDbManager.Query(strSQL, oracleParameters.ToArray(), OPS_Utils.ConstantGeneric.ConnectionStrMes);

            int NextSeqNo = 1;
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0][0] != null)
                    if (!String.IsNullOrEmpty(dt.Rows[0][0].ToString()))
                        SEQNO = dt.Rows[0][0].ToString();

                NextSeqNo = Convert.ToInt32(SEQNO) + 1;
                dt.Dispose();
            }

            return NextSeqNo;
        }

        public static string NewMasterID()
        {
            /* Master ID Format:  KPI + ###.###.### (9 digits) */

            string strSQL = "Select MASTERID From T_KP_KPIM Order By MASTERID DESC ";
            string strMASTERID = "";
            DataTable dt = new DataTable();
            OracleDbManager.Query(strSQL, null, OPS_Utils.ConstantGeneric.ConnectionStrMes);

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0][0] != null)
                    if (!String.IsNullOrEmpty(dt.Rows[0][0].ToString()))
                        strMASTERID = dt.Rows[0][0].ToString();

                dt.Dispose();
            }

            if (!String.IsNullOrEmpty(strMASTERID))
                strMASTERID = strMASTERID.Remove(0, 3);
            else
                strMASTERID = "0";

            int NextID = Convert.ToInt32(strMASTERID) + 1;
            strMASTERID = OPS_Utils.CommonMethod.GetRight("000000000" + NextID.ToString(), 9);

            return "KPI" + strMASTERID;
        }

        public static List<OPS_DAL.SystemEntities.KPISetting> GetKPISettings(string pMasterID)
        {
            string strSQL =
                @" SELECT T_KP_KPIM.MASTERID 
                    , T_KP_KPIM.SYSTEM_ID 
                    , SYST.CODE_NAME AS SYSTEM_NAME 
                    , T_KP_KPIM.TEAM 
                    , TEAM.CODE_NAME AS TEAM_NAME 
                    , T_KP_KPIM.CORPORATION 
                    , CORP.CODE_NAME AS CORP_NAME 
                    , T_KP_KPIM.DIRECTOR 
                    , DIRE.NAME AS DIRE_NAME 
                    , T_KP_KPIP.SEQNO  AS KPIP_SEQNO
                    , T_KP_KPIP.BUYER  
                    , BUYE.CODE_NAME AS BUYER_NAME  
                    , T_KP_KPIP.FACTORY
                    , T_CM_FCMT.NAME AS FACTORY_NAME
                    , T_KP_KPIP.MENU  
                    , T_CM_MENU.MENU_NAME  
                    , T_KP_KPIR.SEQNO 
                    , T_KP_KPIR.USERID
                    , INCS.NAME AS UserName
                    , T_KP_KPIR.USE_YN
                    , T_KP_KPIR.START_DATE
                    , T_KP_KPIR.EXPIRY_DATE
                    , T_KP_KPIR.POSITION
                    FROM PKMES.T_KP_KPIM 
                    LEFT JOIN PKMES.T_KP_KPIP ON 
	                    T_KP_KPIM.MASTERID = T_KP_KPIP.MASTERID  
                    LEFT JOIN PKMES.T_KP_KPIR ON 
	                    T_KP_KPIP.MASTERID = T_KP_KPIR.MASTERID 
	                    AND T_KP_KPIP.SEQNO = T_KP_KPIR.KPIP_SEQNO  
                    LEFT JOIN PKERP.T_CM_MCMT SYST ON 
	                    T_KP_KPIM.SYSTEM_ID  = SYST.S_CODE 
	                    AND SYST.M_CODE = 'System' 
                    LEFT JOIN PKERP.T_CM_MCMT TEAM ON 
	                    T_KP_KPIM.TEAM  = TEAM.S_CODE 
	                    AND TEAM.M_CODE = 'Team' 
                    LEFT JOIN PKERP.T_CM_MCMT CORP ON 
	                    T_KP_KPIM.CORPORATION  = CORP.S_CODE 
	                    AND CORP.M_CODE = 'Corporation' 
                    LEFT JOIN PKERP.T_CM_USMT DIRE ON 
	                    T_KP_KPIM.DIRECTOR = DIRE.USERID  
                    LEFT JOIN PKERP.T_CM_MCMT BUYE ON 
	                    T_KP_KPIP.BUYER  = BUYE.S_CODE 
	                    AND BUYE.M_CODE = 'Buyer' 
                    LEFT JOIN PKERP.T_CM_FCMT ON 
	                    T_KP_KPIP.FACTORY = T_CM_FCMT.FACTORY 
                    LEFT JOIN PKERP.T_CM_MENU ON 
	                    T_KP_KPIM.SYSTEM_ID  = T_CM_MENU.System_ID 
	                    AND T_KP_KPIP.MENU    = T_CM_MENU.MENU_ID  
                    LEFT JOIN PKERP.T_CM_USMT INCS ON 
	                    T_KP_KPIR.USERID = INCS.USERID 
                    WHERE T_KP_KPIM.MASTERID like  :MASTERID
                   ";
            List<OracleParameter> parameters = new List<OracleParameter>();

            if (String.IsNullOrEmpty(pMasterID))
                parameters.Add(new OracleParameter("MASTERID", "%"));
            else
                parameters.Add(new OracleParameter("MASTERID", pMasterID));
             
            var _KPISettings = OracleDbManager.GetObjects<OPS_DAL.SystemEntities.KPISetting>(strSQL, parameters.ToArray()); 
            return _KPISettings; 
        }
    }
}
