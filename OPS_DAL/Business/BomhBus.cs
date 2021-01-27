using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Business
{
    public class BomhBus
    {
        /// <summary>
        /// Get BOM header
        /// </summary>
        /// <param name="stlCode"></param>
        /// <param name="stlSize"></param>
        /// <param name="stlColorSerial"></param>
        /// <param name="stlRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Bomh GetBOMHeader(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            var strSql = @"  SELECT * FROM T_SD_BOMH WHERE STYLECODE = :P_STYLECODE AND STYLESIZE = :P_STYLESIZE AND STYLECOLORSERIAL = :P_STYLECOLORSERIAL AND REVNO = :P_REVNO ";

            var oraPrams = new OpsParams(stlCode, stlSize, stlColorSerial, stlRevNo);

            return OracleDbManager.GetObjects<Bomh>(strSql, oraPrams.ToArray()).FirstOrDefault();
        }

        #region MySQL

        /// <summary>
        /// Get BOMH from MySQL
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        public static Bomh GetBOMHeaderMySQL(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            var strSql = @"  SELECT * FROM T_SD_BOMH WHERE STYLECODE = ?P_STYLECODE AND STYLESIZE = ?P_STYLESIZE AND STYLECOLORSERIAL = ?P_STYLECOLORSERIAL AND REVNO = ?P_REVNO ";

            var parMySql = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", styleCode),
                new MySqlParameter("P_STYLESIZE", styleSize),
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial),
                new MySqlParameter("P_REVNO", revNo)
            };


            return MySqlDBManager.GetAll<Bomh>(strSql, CommandType.Text, parMySql.ToArray()).FirstOrDefault();
        }

        /// <summary>
        /// Insert bomh to MySQL
        /// </summary>
        /// <param name="bomh"></param>
        /// <returns></returns>
        public static bool InsertBOMHToMESMySql(Bomh bomh, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @"INSERT INTO T_SD_BOMH(STYLECODE, STYLESIZE, STYLECOLORSERIAL, REVNO, CADCOLORSERIAL
                                , CARREVNO, CADFILE, CONFDATE, VENDORCODE, CREATEDDATE, MODIFIDATE
                                , NESTINGORDERSTATUS, NESTINGORDERGUID, NESTINGORDERSENTTIME, NESTINGORDERRECDTIME
                                , NESTINGTYPE, NESTINGERROR, NESTINGFILETYPE, NESTINGSENDER, MBOM_NESTINSTATUS
                                , MBOM_NESTINGGUID, MBOM_NESTINSENTTIME, MBOM_NESTINGRECDTIME, MBOM_NESTINGERROR, MBOM_NESTINGSENDER) 
                              VALUES(?P_STYLECODE, ?P_STYLESIZE, ?P_STYLECOLORSERIAL, ?P_REVNO, ?P_CADCOLORSERIAL
                                , ?P_CARREVNO, ?P_CADFILE, ?P_CONFDATE, ?P_VENDORCODE, ?P_CREATEDDATE, ?P_MODIFIDATE
                                , ?P_NESTINGORDERSTATUS, ?P_NESTINGORDERGUID, ?P_NESTINGORDERSENTTIME, ?P_NESTINGORDERRECDTIME
                                , ?P_NESTINGTYPE, ?P_NESTINGERROR, ?P_NESTINGFILETYPE, ?P_NESTINGSENDER, ?P_MBOM_NESTINSTATUS
                                , ?P_MBOM_NESTINGGUID, ?P_MBOM_NESTINSENTTIME, ?P_MBOM_NESTINGRECDTIME, ?P_MBOM_NESTINGERROR, ?P_MBOM_NESTINGSENDER); ";

            var param = new List<MySqlParameter>
            {
                new MySqlParameter("P_STYLECODE", bomh.STYLECODE),
                new MySqlParameter("P_STYLESIZE", bomh.STYLESIZE),
                new MySqlParameter("P_STYLECOLORSERIAL", bomh.STYLECOLORSERIAL),
                new MySqlParameter("P_REVNO", bomh.REVNO),
                new MySqlParameter("P_CADCOLORSERIAL", bomh.CADCOLORSERIAL),
                new MySqlParameter("P_CARREVNO", bomh.CARREVNO),
                new MySqlParameter("P_CADFILE", bomh.CADFILE),
                new MySqlParameter("P_CONFDATE", bomh.CONFDATE),
                new MySqlParameter("P_VENDORCODE", bomh.VENDORCODE),
                new MySqlParameter("P_CREATEDDATE", bomh.CREATEDDATE),
                new MySqlParameter("P_MODIFIDATE", bomh.MODIFIDATE),
                new MySqlParameter("P_NESTINGORDERSTATUS", bomh.NESTINGORDERSTATUS),
                new MySqlParameter("P_NESTINGORDERGUID", bomh.NESTINGORDERGUID),
                new MySqlParameter("P_NESTINGORDERSENTTIME", bomh.NESTINGORDERSENTTIME),
                new MySqlParameter("P_NESTINGORDERRECDTIME", bomh.NESTINGORDERRECDTIME),
                new MySqlParameter("P_NESTINGTYPE", bomh.NESTINGTYPE),
                new MySqlParameter("P_NESTINGERROR", bomh.NESTINGERROR),
                new MySqlParameter("P_NESTINGFILETYPE", bomh.NESTINGFILETYPE),
                new MySqlParameter("P_NESTINGSENDER", bomh.NESTINGSENDER),
                new MySqlParameter("P_MBOM_NESTINSTATUS", bomh.MBOM_NESTINSTATUS),
                new MySqlParameter("P_MBOM_NESTINGGUID", bomh.MBOM_NESTINGGUID),
                new MySqlParameter("P_MBOM_NESTINSENTTIME", bomh.MBOM_NESTINSENTTIME),
                new MySqlParameter("P_MBOM_NESTINGRECDTIME", bomh.MBOM_NESTINGRECDTIME),
                new MySqlParameter("P_MBOM_NESTINGERROR", bomh.MBOM_NESTINGERROR),
                new MySqlParameter("P_MBOM_NESTINGSENDER", bomh.MBOM_NESTINGSENDER)
            };

            var blIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, param.ToArray(), CommandType.Text, myTrans, myConnection);

            return blIns != null;
        }

        /// <summary>
        /// Insert BOM information
        /// </summary>
        /// <param name="bomh"></param>
        /// <param name="listBomt"></param>
        /// <param name="listPattern"></param>
        /// <param name="listMbom"></param>
        /// <returns></returns>
        public static bool InsertBOMToMESMySql(Bomh bomh, List<Bomt> listBomt, List<Pattern> listPattern, List<MBom> listMbom)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    if (InsertBOMHToMESMySql(bomh, myTrans, myConnection))
                    {
                        //Create temporary list bomt to keep item which was inserted to DB
                        var listItemTemp = new List<Icmt>();
                        var listSOSTemp = new List<string>();

                        //Insert list of BOMT
                        foreach (var bomt in listBomt)
                        {
                            BomtBus.InsertBOMTToMESMySql(bomt, myTrans, myConnection);

                            //If item code does not exist in MySQL then copy it                            
                            //Check item in BOM whether was inserted or not
                            var itemCodeTemp = listItemTemp.Where(x => x.ItemCode == bomt.ItemCode);
                            if (!itemCodeTemp.Any())
                            {
                                if (IcmtBus.GetItemCodeMySql(bomt.ItemCode) == null)
                                {
                                    //Copy item code, item color
                                    var itemCode = IcmtBus.GetItemCode(bomt.ItemCode);
                                    //Insert item code to MySQL DB
                                    IcmtBus.InsertItemCodeMySQL(itemCode, myTrans, myConnection);

                                    //Keep item in temporary list
                                    listItemTemp.Add(itemCode);
                                }
                            }

                            //Insert SOS information
                            //Check sos whether inserted or not
                            var tempSos = listSOSTemp.Where(x => x == bomt.Sos);
                            if (!tempSos.Any())
                            {
                                if (!string.IsNullOrEmpty(bomt.Sos))
                                {
                                    if (SsCmBus.GetSOSMySQL(bomt.Sos) == null)
                                    {
                                        //Get sos information from Oracle
                                        var sos = SsCmBus.GetSOS(bomt.Sos);
                                        //Insert sos information to MySQL
                                        SsCmBus.InsertSOSMySQL(sos, myTrans, myConnection);
                                    }
                                }
                                //Add SOS code to  temporary list.
                                listSOSTemp.Add(bomt.Sos);
                            }
                        }

                        //Insert  list of PTMT
                        foreach (var ptmt in listPattern)
                        {
                            PatternBus.InsertPatternToMESMySql(ptmt, myTrans, myConnection);
                        }

                        //Insert list of mbom
                        foreach (var mbom in listMbom)
                        {
                            MBomBus.InsertMBOMToMESMySql(mbom, myTrans, myConnection);
                        }
                    }

                    myTrans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    myTrans.Rollback();
                    throw;
                }
            }
        }
        #endregion

    }
}
