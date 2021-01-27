using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace OPS_DAL.Business
{
    public class StmtBus
    {
        #region MySQL database
        /// <summary>
        /// Get style information by mx package
        /// </summary>
        /// <param name="mxPackage"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Stmt GetStyleInfoByMxPackage(string mxPackage)
        {
            var strSql = @"select pdt.MXPACKAGE, pdt.MXTARGET, pdt.MX_IOT_COMPLETED 
		                        , opm.STYLECODE, opm.STYLESIZE, opm.STYLECOLORSERIAL, opm.REVNO    
                                , stm.BUYERSTYLECODE, stm.BUYERSTYLENAME
                                , scm.STYLECOLORWAYS        
                                , concat('http://203.113.151.204:8080/PKPDM/style/',stm.BUYER, '/', stm.STYLECODE, '/Images/', sfd.FILENAME ) ImageLink
                        from t_mx_mpdt pdt
	                        left join t_mx_opmt opm on opm.MXPACKAGE = pdt.MXPACKAGE
                            left join t_00_stmt stm on stm.stylecode = opm.STYLECODE
                            left join t_00_scmt scm on scm.STYLECODE = opm.stylecode and scm.STYLECOLORSERIAL = opm.STYLECOLORSERIAL
                            left join t_00_sfdt sfd on sfd.STYLECODE = opm.stylecode and sfd.IS_MAIN = 'Y'
                        where pdt.MXPACKAGE = ?P_MXPACKAGE;";

            var param = new List<MySqlParameter> { new MySqlParameter("P_MXPACKAGE", mxPackage) };
            var listStmt = MySqlDBManager.GetObjectsConvertType<Stmt>(strSql, CommandType.Text, param.ToArray());

            return listStmt.FirstOrDefault();
        }

        /// <summary>
        /// Get style information by style key: styleCode, styleSize, styleColorSerial and revNo
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static Stmt GetStyleInfoByStyleKey(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            var strSql = @"select DOR.STYLECODE, DOR.STYLESIZE, DOR.STYLECOLORSERIAL, DOR.REVNO 
	                        , stm.STYLENAME, stm.BUYERSTYLECODE, stm.BUYERSTYLENAME, scm.STYLECOLORWAYS
                            , concat('http://203.113.151.204:8080/PKPDM/style/',stm.BUYER, '/', stm.STYLECODE, '/Images/', sfd.FILENAME ) ImageLink
                        from t_sd_dorm dor 
	                        LEFT JOIN T_00_STMT STM ON STM.STYLECODE = DOR.STYLECODE
                            LEFT JOIN T_00_SCMT SCM ON SCM.STYLECODE = DOR.STYLECODE AND SCM.STYLECOLORSERIAL = DOR.STYLECOLORSERIAL
                            left join t_00_sfdt sfd on sfd.STYLECODE = DOR.stylecode and sfd.IS_MAIN = 'Y'
                        where dor.STYLECODE = ?P_STYLECODE and dor.STYLESIZE = ?P_STYLESIZE and dor.STYLECOLORSERIAL = ?P_STYLECOLORSERIAL and REVNO = ?P_REVNO;";

            var param = new List<MySqlParameter> { 
                new MySqlParameter("P_STYLECODE", styleCode), 
                new MySqlParameter("P_STYLESIZE", styleSize), 
                new MySqlParameter("P_STYLECOLORSERIAL", styleColorSerial), 
                new MySqlParameter("P_REVNO", revNo), 
            };
            var listStmt = MySqlDBManager.GetObjectsConvertType<Stmt>(strSql, CommandType.Text, param.ToArray());

            return listStmt.FirstOrDefault();
        }

        /// <summary>
        /// Gets the by style code.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 03-Jul-19
        public static Stmt GetByStyleCode(string styleCode)
        {
            var prs = new List<MySqlParameter> { new MySqlParameter("P_STYLECODE", styleCode) };
            var stmts = MySqlDBManager.GetAll<Stmt>("SP_MES_GETBYSTYLECODE_STMT", CommandType.StoredProcedure,
                prs.ToArray());

            return stmts.FirstOrDefault();
        }

        public static bool InsertStyleMasterToMESMySql(Stmt stm, MySqlTransaction myTrans, MySqlConnection myConnection)
        {
            string strSql = @" insert into t_00_stmt(stylecode, buyer, old_stylecode, stylename, buyerstylecode, buyerstylename, stylegroup, subgroup, subsubgroup, sizeunit, width
			                    , endwise, height, weightunit, weight, packingunit, packingqty, packingsizeunit, packingwidth, packingendwise, packingheight
			                    , status, currcode, unitprice, factory, registrydate, lastmodidate, styleregister, designer, itemmanager, qtyassumer, opplanner
                                ,  technician, itemdman, confirmdate, seasoncode, samplestage, picture, model, functions, volume, collecname, pattern, volumeunit, fun_atch, outsourcecheck, sono)
                            values(?p_stylecode, ?p_buyer, ?p_old_stylecode, ?p_stylename, ?p_buyerstylecode, ?p_buyerstylename, ?p_stylegroup, ?p_subgroup, ?p_subsubgroup, ?p_sizeunit, ?p_width
			                            , ?p_endwise, ?p_height, ?p_weightunit, ?p_weight, ?p_packingunit, ?p_packingqty, ?p_packingsizeunit, ?p_packingwidth, ?p_packingendwise, ?p_packingheight
			                            , ?p_status, ?p_currcode, ?p_unitprice, ?p_factory, ?p_registrydate, ?p_lastmodidate, ?p_styleregister,?p_designer, ?p_itemmanager, ?p_qtyassumer, ?p_opplanner
                                        ,  ?p_technician, ?p_itemdman, ?p_confirmdate, ?p_seasoncode, ?p_samplestage, ?p_picture, ?p_model, ?p_functions, ?p_volume, ?p_collecname, ?p_pattern, ?p_volumeunit, ?p_fun_atch, ?p_outsourcecheck, ?p_sono);
                             ";

            var par = new List<MySqlParameter>
            {
                new MySqlParameter("p_stylecode", stm.StyleCode),
                new MySqlParameter("p_buyer", stm.Buyer),
                new MySqlParameter("p_old_stylecode", stm.Old_StyleCode),
                new MySqlParameter("p_stylename", stm.StyleName),
                new MySqlParameter("p_buyerstylecode", stm.BuyerStyleCode),
                new MySqlParameter("p_buyerstylename", stm.BuyerStyleName),
                new MySqlParameter("p_stylegroup", stm.StyleGroup),
                new MySqlParameter("p_subgroup", stm.SubGroup),
                new MySqlParameter("p_subsubgroup", stm.SubSubGroup),
                new MySqlParameter("p_sizeunit", stm.SizeUnit),
                new MySqlParameter("p_width", stm.Width),
                new MySqlParameter("p_endwise", stm.EndWise),
                new MySqlParameter("p_height", stm.Height),
                new MySqlParameter("p_weightunit", stm.WeightUnit),
                new MySqlParameter("p_weight", stm.Weight),
                new MySqlParameter("p_packingunit", stm.PackingUnit),
                new MySqlParameter("p_packingqty", stm.PackingQty),
                new MySqlParameter("p_packingsizeunit", stm.PackingSizeUnit),
                new MySqlParameter("p_packingwidth", stm.PackingWidth),
                new MySqlParameter("p_packingendwise", stm.PackingEndWise),
                new MySqlParameter("p_packingheight", stm.PackingHeight),
                new MySqlParameter("p_status", stm.Status),
                new MySqlParameter("p_currcode", stm.CurrCode),
                new MySqlParameter("p_unitprice", stm.UnitPrice),
                new MySqlParameter("p_factory", stm.Factory),
                new MySqlParameter("p_registrydate", stm.RegistryDate),
                new MySqlParameter("p_lastmodidate", stm.LastModiDate),
                new MySqlParameter("p_styleregister", stm.StyleRegister),
                new MySqlParameter("p_designer", stm.Designer),
                new MySqlParameter("p_itemmanager", stm.ItemManager),
                new MySqlParameter("p_qtyassumer", stm.QtyAssumer),
                new MySqlParameter("p_opplanner", stm.OpPlanner),
                new MySqlParameter("p_technician", stm.Technician),
                new MySqlParameter("p_itemdman", stm.ItemDMan),
                new MySqlParameter("p_confirmdate", stm.ConfirmDate),
                new MySqlParameter("p_seasoncode", stm.SeasonCode),
                new MySqlParameter("p_samplestage", stm.SampleStage),
                new MySqlParameter("p_picture", stm.Picture),
                new MySqlParameter("p_model", stm.Model),
                new MySqlParameter("p_functions", stm.Functions),
                new MySqlParameter("p_volume", stm.Volume),
                new MySqlParameter("p_collecname", stm.CollectName),
                new MySqlParameter("p_pattern", stm.Pattern),
                new MySqlParameter("p_volumeunit", stm.VolumeUnit),
                new MySqlParameter("p_fun_atch", stm.Fun_Atch),
                new MySqlParameter("p_outsourcecheck", stm.OutSourceCheck),
                new MySqlParameter("p_sono", stm.Sono)
            };

            var resIns = MySqlDBManager.ExecuteQueryWithTrans(strSql, par.ToArray(), CommandType.Text, myTrans, myConnection);

            return resIns != null;
        }

        /// <summary>
        /// Insert style informamtion to MySQL
        /// </summary>
        /// <param name="stmt"></param>
        /// <param name="listScmt"></param>
        /// <param name="listSsmt"></param>
        /// <returns></returns>
        public static bool InsertStyleToMESMySql(Stmt stmt, List<Scmt> listScmt, List<Ssmt> listSsmt)
        {
            using (var myConnection = new MySqlConnection(ConstantGeneric.ConnectionStrMesMySql))
            {
                myConnection.Open();
                var myTrans = myConnection.BeginTransaction();
                try
                {
                    if (InsertStyleMasterToMESMySql(stmt, myTrans, myConnection))
                    {
                        //Insert list of BOMT
                        foreach (var scmt in listScmt)
                        {
                            ScmtBus.InsertStyleColorToMESMySql(scmt, myTrans, myConnection);
                        }

                        //Insert  list of PTMT
                        foreach (var ssmt in listSsmt)
                        {
                            SsmtBus.InsertStyleSizeToMESMySql(ssmt, myTrans, myConnection);;
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

        #region Oracle database

        /// <summary>
        /// Gets the style master by style code.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Stmt GetStyleMasterByStyleCode(string styleCode)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
                cursor
            };
            var lstStyleMaster = OracleDbManager.GetObjects<Stmt>("SP_OPS_GETSTYLEMASTER_STMT", CommandType.StoredProcedure, oracleParams.ToArray());
            return lstStyleMaster.Count > 0 ? lstStyleMaster.FirstOrDefault() : new Stmt();
        }

        /// <summary>
        /// Get style information to copy
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static Stmt GetStyleInfoByCode(string styleCode)
        {
            var strSql = " SELECT * FROM T_00_STMT WHERE STYLECODE = :P_STYLECODE";

            List<OracleParameter> oraParams = new List<OracleParameter>
            {
                new OracleParameter("P_STYLECODE", styleCode),
            };

            var styleInf = OracleDbManager.GetObjects<Stmt>(strSql, CommandType.Text, oraParams.ToArray());

            return styleInf.Count > 0 ? styleInf.FirstOrDefault() : new Stmt();
        }

        /// <summary>
        /// Updates the name of the style picture.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="pictureName">Name of the picture.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateStylePictureName(string styleCode, string pictureName)
        {

            using (var connection = new OracleConnection(ConstantGeneric.ConnectionStr))
            {
                connection.Open();
                var trans = connection.BeginTransaction();
                try
                {
                    List<OracleParameter> oracleParams = new List<OracleParameter>
                    {
                        new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16){Direction = ParameterDirection.Output},
                        new OracleParameter("P_STYLECODE", styleCode),
                        new OracleParameter("P_PICTURE", pictureName)
                    };
                    var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEPICTURESTYLE_STMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, connection);

                    trans.Commit();
                    return resUpdate != null && int.Parse(resUpdate.ToString()) != 0;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public static bool UpdateStylePictureName(string styleCode, string pictureName, OracleConnection oraConn, OracleTransaction trans)
        {

            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16){Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_PICTURE", pictureName)
            };
            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATEPICTURESTYLE_STMT", oracleParams.ToArray(), CommandType.StoredProcedure, trans, oraConn);

            return resUpdate != null && int.Parse(resUpdate.ToString()) != 0;

        }

        /// <summary>
        /// Update style group, sub group, sub sub group
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleGroup"></param>
        /// <param name="subGroup"></param>
        /// <param name="subSubGroup"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UpdateStyleGroup(string styleCode, string styleGroup, string subGroup, string subSubGroup)
        {
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_AFFECTEDROWS", OracleDbType.Int16){Direction = ParameterDirection.Output},
                new OracleParameter("P_STYLECODE", styleCode),
                new OracleParameter("P_STYLEGROUP", styleGroup),
                new OracleParameter("P_SUBGROUP", subGroup),
                new OracleParameter("P_SUBSUBGROUP", subSubGroup)
            };

            var resUpdate = OracleDbManager.ExecuteQuery("SP_OPS_UPDATESTYLEGROUP_STMT", oracleParams.ToArray(), CommandType.StoredProcedure);

            return resUpdate != null;
        }

        #region Style

        /// <summary>
        /// Create style image link
        /// </summary>
        /// <param name="pictureName"></param>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static string CreateStyleImageLink(string pictureName, string styleCode)
        {
            var pictureLink = string.Empty;
            if (string.IsNullOrEmpty(pictureName)) return pictureLink;

            var buyer = styleCode.Substring(0, 3);
            var subDir = buyer + "/" + styleCode + "/" + ConstantGeneric.FtpStyleImageFolder + "/" + pictureName;
            //Get http link for show style image
            var ftpInfo = FtpInfoBus.GetFtpInfo(ConstantGeneric.FtpAppTypePlmHost);
            var httpImage = ftpInfo.FtpLink + ftpInfo.FtpFolder + "/" + ConstantGeneric.FtpStyleFolder + "/" + subDir;

            pictureLink = httpImage;

            return pictureLink;

        }
        #endregion

        #endregion
    }
}
