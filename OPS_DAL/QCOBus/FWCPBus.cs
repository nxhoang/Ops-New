using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Oracle.ManagedDataAccess.Client;

namespace OPS_DAL.QCOBus
{
    public class FWCPBus
    {
        public bool Update(string pConnectionString, QCOEntities.FWCP objFWCP, out string pMessage)
        {
            pMessage = "";
            bool isResult = true;
            string Term = "";

            try
            {
                var strSQL = @" 
                    Select * 
                    From PKMES.T_CM_FWCP 
                    Where FACTORY = :FACTORY
                    AND YEAR = :YEAR 
                    AND  WEEKNO = :WEEKNO 
                ";

                List<OracleParameter> parameters = new List<OracleParameter>();
                parameters.Add(new OracleParameter("FACTORY", objFWCP.FACTORY));
                parameters.Add(new OracleParameter("YEAR", objFWCP.YEAR));
                parameters.Add(new OracleParameter("WEEKNO", objFWCP.WEEKNO));

                using (OracleConnection oracleConn = new OracleConnection(pConnectionString))
                {
                    oracleConn.Open();

                    OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(strSQL, oracleConn);
                    oracleDataAdapter.SelectCommand.Parameters.AddRange(parameters.ToArray());

                    var _dt = new DataTable();
                    oracleDataAdapter.Fill(_dt);

                    if (_dt != null)
                    {
                        if (_dt.Rows.Count == 0)
                        {
                            Term = "Added";
                            //Add New
                            var _drNew = _dt.NewRow();

                            _drNew["FACTORY"] = objFWCP.FACTORY;
                            _drNew["YEAR"] = objFWCP.YEAR;
                            _drNew["WEEKNO"] = objFWCP.WEEKNO;

                            _drNew["TOTALWORKERS"] = objFWCP.TOTALWORKERS;
                            _drNew["CAPACITY"] = objFWCP.CAPACITY;

                            _drNew["TOTALMACHINES"] = objFWCP.TOTALMACHINES;

                            _drNew["TOTALSEWER"] = objFWCP.TOTALSEWER;
                            _drNew["SEWERCAPA"] = objFWCP.SEWERCAPA;

                            _drNew["STARTDATE"] = objFWCP.STARTDATE;
                            _drNew["ENDDATE"] = objFWCP.ENDDATE;

                            _drNew["CREATOR"] = objFWCP.CREATOR;
                            _drNew["CREATETIME"] = DateTime.Now;

                            _drNew["TOTALWORKHOUR"] = objFWCP.TOTALWORKHOUR; //2019-10-24 Tai Le (Thomas)

                            _dt.Rows.Add(_drNew);
                        }
                        else
                        {
                            Term = "Updated";
                            //Update 
                            var _drNew = _dt.Rows[0];

                            _drNew["TOTALWORKERS"] = objFWCP.TOTALWORKERS;
                            _drNew["CAPACITY"] = objFWCP.CAPACITY;

                            _drNew["TOTALMACHINES"] = objFWCP.TOTALMACHINES;

                            _drNew["TOTALSEWER"] = objFWCP.TOTALSEWER;
                            _drNew["SEWERCAPA"] = objFWCP.SEWERCAPA;

                            _drNew["CREATOR"] = objFWCP.CREATOR;
                            _drNew["CREATETIME"] = DateTime.Now;

                            _drNew["TOTALWORKHOUR"] = objFWCP.TOTALWORKHOUR; //2019-10-24 Tai Le (Thomas)


                            _drNew["CREATOR"] = objFWCP.CREATOR;
                            _drNew["CREATETIME"] = DateTime.Now;

                            ///2020-06-29 Tai Le(Thomas)
                            ///Error happens 2020-W27: wrong the StartDate / End Date
                            _drNew["STARTDATE"] = objFWCP.STARTDATE;
                            _drNew["ENDDATE"] = objFWCP.ENDDATE; 
                        }

                        OracleCommandBuilder oracleCommandBuilder = new OracleCommandBuilder(oracleDataAdapter);
                        oracleDataAdapter.Update(_dt);
                        oracleCommandBuilder.Dispose();

                        _dt.Dispose();
                        oracleDataAdapter.Dispose();
                    }
                    oracleConn.Close();
                }
                pMessage = $"Weekly Capacity of Factory[{objFWCP.FACTORY}], Year[{ objFWCP.YEAR}], WeekNo[{ objFWCP.WEEKNO }] {Term}.";
                //return true;
            }
            catch (Exception ex)
            {
                pMessage = "Factory[" + objFWCP.FACTORY + "], Year[" + objFWCP.YEAR + "], WeekNo[" + objFWCP.WEEKNO + "] FAIL: "+ ex.Message;
                isResult = false;
            }

            return isResult;
        }

        public static string WeekNoToString(int pWeekNo)
        { 
            if (pWeekNo < 10)
                return "0" + pWeekNo.ToString();
            else
                return pWeekNo.ToString();
        }

        public static int StringWeekNoToINT(string pWeekNo)
        {
            /* pWeekNo Format as  W01 , ... , W10 
             * or pWeekNo Format as text like 01 , ... , 10  
             */
            if (pWeekNo.Contains("W"))
                return Convert.ToInt32(pWeekNo.Substring(1, pWeekNo.Length - 1));
            else
                return Convert.ToInt32(pWeekNo);
        }
    }
}
