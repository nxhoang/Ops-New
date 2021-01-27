using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Oracle.ManagedDataAccess.Client;

namespace OPS_DAL.QCOEntities
{
    public class FWES
    {
        public string FACTORY { get; set; }
        public int YEAR { get; set; }
        public string WEEKNO { get; set; }
        public decimal EFFICIENCYPERCEN { get; set; }
        public string CONFIRMYN { get; set; }
        public string CONFIRMBY { get; set; }
        public DateTime? CONFIRMDATE { get; set; }
    }

    public class FWESBus
    {
        public static bool SaveFWES(string pConnString, FWES objFWES)
        {
            if (objFWES == null)
                return false;

            try
            {
                var strSQL =
                    " SELECT * " +
                    " FROM PKMES.T_CM_FWES " +
                    " WHERE FACTORY = :FACTORY " +
                    " AND YEAR = :YEAR " +
                    " AND WEEKNO = :WEEKNO ";

                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", objFWES.FACTORY));
                parameters.Add(new OracleParameter("YEAR", objFWES.YEAR));
                parameters.Add(new OracleParameter("WEEKNO", WeekToString(objFWES.WEEKNO)));

                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, pConnString);
                oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());

                DataTable dt = new DataTable();
                oracleDataAdapter.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        var dr = dt.Rows[0];

                        if (dr["CONFIRMYN"].ToString() == "N")
                            dr["EFFICIENCYPERCEN"] = objFWES.EFFICIENCYPERCEN;
                    }
                    else
                    {
                        var drNew = dt.NewRow();

                        drNew["FACTORY"] = objFWES.FACTORY;
                        drNew["YEAR"] = objFWES.YEAR;
                        drNew["WEEKNO"] = WeekToString(objFWES.WEEKNO);
                        drNew["EFFICIENCYPERCEN"] = objFWES.EFFICIENCYPERCEN;

                        drNew["CONFIRMYN"] = "N";

                        dt.Rows.Add(drNew);
                    }
                }

                //Update back to DB
                OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                oracleDataAdapter.Update(dt);
                //dt.AcceptChanges();
                oracleCommandBuilder.Dispose();


                //Clear objects to prevent Memory Leak
                parameters.Clear();
                dt.Dispose();
                oracleDataAdapter.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }

        public static bool ConfirmFWES(string pConnString, FWES objFWES)
        {
            if (objFWES == null)
                return false;

            try
            {
                var strSQL =
                    " SELECT * " +
                    " FROM PKMES.T_CM_FWES " +
                    " WHERE FACTORY = :FACTORY " +
                    " AND YEAR = :YEAR " +
                    " AND WEEKNO = :WEEKNO ";

                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", objFWES.FACTORY));
                parameters.Add(new OracleParameter("YEAR", objFWES.YEAR));
                parameters.Add(new OracleParameter("WEEKNO", WeekToString(objFWES.WEEKNO)));

                OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, pConnString);
                oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());

                DataTable dt = new DataTable();
                oracleDataAdapter.Fill(dt);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        var dr = dt.Rows[0];

                        if (dr["CONFIRMYN"].ToString() == "N")
                            dr["CONFIRMYN"] = "Y";


                        //Update back to DB
                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(dt); 
                        oracleCommandBuilder.Dispose();
                    } 
                }
                 
                //Clear objects to prevent Memory Leak
                parameters.Clear();
                dt.Dispose();
                oracleDataAdapter.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return false;
            }
        }

        public static async Task<bool> RemoveFWES(string pConnString, FWES objFWES) {
            bool blRes = true;
            try {
                var strSQL = 
                    $" Select * " +
                    $" From PKMES.T_QC_QUEUE " +
                    $" Where rownum<=2 " +
                    $" And QCOFactory = '{objFWES.FACTORY}' " +
                    $" And WEEKCAPA = '{objFWES.YEAR}' || ' / ' || '{objFWES.WEEKNO}' ";
                var dt = OPS_DAL.DAL.OracleDbManager.Query(strSQL, null, OPS_Utils.ConstantGeneric.ConnectionStrMes);

                if (dt != null)
                {
                    if (dt.Rows.Count == 0)
                    {
                        //OK to remove
                        await OPS_DAL.DAL.OracleDbManager.ExecuteQueryAsync(
                            $"Delete T_CM_FWES Where FACTORY = '{objFWES.FACTORY}' And YEAR = {objFWES.YEAR} And WEEKNO = '{objFWES.WEEKNO}' "
                            , null
                            , CommandType.Text
                            , OPS_Utils.ConstantGeneric.ConnectionStrMes);
                    }
                    else
                    {
                        //Be in used already
                        blRes = false;
                    }
                    dt.Dispose();
                }

            }
            catch (Exception ex) {
                var msg = ex.Message;
                blRes= false;
            }

            return blRes;
        }
      
        public static string WeekToString(string pWeekNo)
        {
            if (pWeekNo.Contains("W"))
                return pWeekNo;

            if (Convert.ToInt32(pWeekNo) < 10)
                return "W0" + Convert.ToInt32(pWeekNo).ToString();
            else
                return "W" + Convert.ToInt32(pWeekNo).ToString();
        }
    }
}
