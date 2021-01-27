using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.MesBus
{
    public class LineBus
    {
        #region Oracle db

        /// <summary>
        /// Gets the mtop lines by factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 06-Jul-19
        public static List<LineEntity> GetMtopLinesByFactory(string factory)
        {
            var prs = new List<OracleParameter>
            {
                new OracleParameter("p_factory", factory),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction=ParameterDirection.Output}
            };
            var lines = OracleDbManager.GetObjects<LineEntity>("SP_MES_GETBYFACTORY_FATLIN", CommandType.StoredProcedure,
                prs.ToArray(), ConstantGeneric.ConnectionStr, ConstantGeneric.OraTimeout);

            return lines;
        }

        /// <summary>
        /// Adds the line.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 06-Jul-19
        public static LineEntity OracleAddLine(LineEntity item)
        {
            var paramList = new List<OracleParameter>
            {
                new OracleParameter("p_lineserial", item.LineSerial),
                new OracleParameter("p_factory", item.Factory),
                new OracleParameter("p_linename", item.LineName),
                new OracleParameter("p_lineno", item.LineNo),
                new OracleParameter("p_backgroundcolor", item.BackgroundColor),
                new OracleParameter("p_inuse", item.InUse),
                new OracleParameter("p_workers", item.LineMan)
            };
            var lineSerial = OracleDbManager.ExecuteQuery("sp_mes_insert_line", paramList.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);

            return lineSerial == null ? null : item;
        }

        /// <summary>
        /// Gets the mes lines by factory.
        /// </summary>
        /// <param name="fac">The factory.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 06-Jul-19
        public static IEnumerable<LineEntity> GetMesLinesByFactory(string fac)
        {
            var oracleParams = new List<OracleParameter> {
                new OracleParameter("p_factory", fac),
                new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor){Direction=ParameterDirection.Output}
            };
            var lines = OracleDbManager.GetObjects<LineEntity>("sp_mes_getbyfactory_line",
                CommandType.StoredProcedure, oracleParams.ToArray(), EnumDataSource.PkMes);

            return lines;
        }

        /// <summary>
        /// Updates the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 06-Jul-19
        public static bool OracleUpdateLine(LineEntity line)
        {
            var arParam = new OracleParameter("p_affectedrows", OracleDbType.Int16)
            {
                Direction = ParameterDirection.Output
            };
            var oracleParams = new List<OracleParameter>
            {
                arParam,
                new OracleParameter("p_lineserial", line.LineSerial),
                new OracleParameter("p_factory", line.Factory),
                new OracleParameter("p_linename", line.LineName),
                new OracleParameter("p_lineno", line.LineNo),
                new OracleParameter("p_backgroundcolor", line.BackgroundColor),
                new OracleParameter("p_inuse", line.InUse),
                new OracleParameter("p_workers", line.LineMan)
            };
            var result = OracleDbManager.ExecuteQuery("sp_mes_update_line", oracleParams.ToArray(),
                CommandType.StoredProcedure, ConstantGeneric.ConnectionStrMes);
            var affectedRow = int.Parse(result.ToString());

            return affectedRow > 0;
        }

        #endregion

        #region MySQL db

        /// <summary>
        /// Gets the mes lines by factory.
        /// </summary>
        /// <param name="fac">The factory.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 10-Jul-19
        public static IEnumerable<LineEntity> MySqlGetMesLinesByFactory(string fac)
        {
            var prs = new List<MySqlParameter> { new MySqlParameter("p_factory", fac) };
            var lines = MySqlDBManager.GetAll<LineEntity>("SP_MES_GETBYFACTORY_LINE", CommandType.StoredProcedure,
                prs.ToArray());

            return lines;
        }

        /// <summary>
        /// Adds line.
        /// </summary>
        /// <param name="item">The line item.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 08-Jul-19
        public static LineEntity MySqlAddLine(LineEntity item)
        {
            var lineSerialParam = new MySqlParameter("p_lineserial", OracleDbType.Decimal)
            {
                Direction = ParameterDirection.Output
            };
            var paramList = new List<MySqlParameter>
            {
                lineSerialParam,
                new MySqlParameter("p_factory", item.Factory),
                new MySqlParameter("p_linename", item.LineName),
                new MySqlParameter("p_mtopline", item.LineNo),
                new MySqlParameter("p_backgroundcolor", item.BackgroundColor),
                new MySqlParameter("p_inuse", item.InUse),
                new MySqlParameter("p_workers", item.LineMan)
            };
            var lineSerial = MySqlDBManager.ExecuteNonQuery("SP_MES_INSERT_LINE", CommandType.StoredProcedure,
                paramList.ToArray());

            if (lineSerial == null) return null;

            // Updating LastUpdated date column to t_cm_fcmt
            FcmtBus.UpdateLastUpdated(item.Factory);

            var id = int.Parse(lineSerial.ToString());
            item.LineSerial = id;

            return item;
        }

        /// <summary>
        /// Updates the line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        /// Created Date: 08-Jul-19
        public static bool MySqlUpdateLine(LineEntity line)
        {
            var prs = new List<MySqlParameter>
            {
                new MySqlParameter("p_lineserial", line.LineSerial),
                new MySqlParameter("p_factory", line.Factory),
                new MySqlParameter("p_linename", line.LineName),
                new MySqlParameter("p_mtopline", line.LineNo),
                new MySqlParameter("p_backgroundcolor", line.BackgroundColor),
                new MySqlParameter("p_inuse", line.InUse),
                new MySqlParameter("p_workers", line.LineMan)
            };
            var result = MySqlDBManager.ExecuteNonQuery("SP_MES_UPDATE_LINE", CommandType.StoredProcedure, prs.ToArray());
            if (result != null)
            {
                // Updating LastUpdated date column to t_cm_fcmt
                FcmtBus.UpdateLastUpdated(line.Factory);
            }

            return result != null;
        }

        /// <summary>
        /// Get list of lines by factory
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<LineEntity> GetMESLineByFactory(string factory)
        {
            string strSql = @"select CONCAT(LINESERIAL, '#',COALESCE(mtopline,''))  as LineCombination,   IFNULL(linename, 'Undefined') as linename  from t_cm_line where inuse = '1' and  factory = ?P_FACTORY; ";
            
            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factory)
            };

            var listLine = MySqlDBManager.GetObjects<LineEntity>(strSql, CommandType.Text, oraParams.ToArray());

            return listLine;
        }

        /// <summary>
        /// Get the number mapping seats with operation plan.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="plnStartDate"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<LineEntity> GetMappingSeats(string factoryId, string plnStartDate)
        {
            string strSql = @"select li.linename, li.workers, t1.* from t_cm_line li left join	(
                            select count(opt.seatno) mappingseats, opt.LINESERIAL, mpt.PLNSTARTDATE, mpt.FACTORY  , max(mpt.CREATEDATE) createdate 
                            from t_mx_opmt opm join t_mx_opdt opt on opt.stylecode = opm.STYLECODE 
		                            and opt.stylecolorserial = opm.STYLECOLORSERIAL
		                            and opt.stylesize = opm.STYLESIZE and opt.REVNO = opm.REVNO and opt.OPREVNO = opm.OPREVNO
	                            join t_mx_mpdt mpt on mpt.MXPACKAGE = opm.MXPACKAGE
                            where mpt.FACTORY = ?P_FACTORY and mpt.PLNSTARTDATE = ?P_PLNSTARTDATE AND OPT.SEATNO != 0
                            group by  opt.LINESERIAL, mpt.PLNSTARTDATE, mpt.FACTORY 
                            ) t1 on li.lineserial = t1.LINESERIAL 
                            where li.factory = ?P_FACTORYLINE 
                            order by t1.plnstartdate desc
                                    , CAST((case when regexp_substr(li.linename, '(\\d)(\\d)') is null 
									        then regexp_substr(li.linename, '(\\d)') 
									        else regexp_substr(li.linename, '(\\d)(\\d)') end) AS UNSIGNED)
                                    , li.LINENAME;";
            
            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factoryId),
                new MySqlParameter("P_PLNSTARTDATE", plnStartDate),
                new MySqlParameter("P_FACTORYLINE", factoryId)
            };

            var listLine = MySqlDBManager.GetAll<LineEntity>(strSql, CommandType.Text, oraParams.ToArray());

            return listLine;
        }

        /// <summary>
        /// Get connected iot
        /// </summary>
        /// <param name="factoryId"></param>
        /// <param name="plnStartDate"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<LineEntity> GetConnectedIot(string factoryId, string plnStartDate)
        {
            string strSql1 = @"select count(opt.mcid) connectedIot, max(opt.LAST_IOT_DATA_RECEIVE_TIME) last_iot_data_receive_time, mpt.PLNSTARTDATE, mpt.FACTORY , mpt.MXPACKAGE, mpt.LINESERIAL 
                            from t_mx_opmt opm 
	                            join t_mx_opdt_mc opt on 
		                            opt.stylecode = opm.STYLECODE and opt.stylesize = opm.STYLESIZE 
		                            and opt.stylecolorserial = opm.STYLECOLORSERIAL  
		                            and opt.REVNO = opm.REVNO and opt.OPREVNO = opm.OPREVNO 
	                            join t_mx_mpdt mpt on 
		                            mpt.MXPACKAGE = opm.MXPACKAGE 
                            where  mpt.FACTORY = ?P_FACTORY and mpt.PLNSTARTDATE = ?P_PLNSTARTDATE and opt.LAST_IOT_DATA_RECEIVE_TIME is not null
                            group by  mpt.PLNSTARTDATE, mpt.FACTORY , mpt.MXPACKAGE, mpt.LINESERIAL ;";

            string strSql = @"select count(opd.LINESERIAL) connectedIot, opd.LINESERIAL, li.LINENAME
                            , max(t1.last_iot_data_receive_time) last_iot_data_receive_time 
                            , t1.mxpackage, t1.plnstartdate, t1.factory
                            from (
                                    select  opt.last_iot_data_receive_time
                                    , mpt.PLNSTARTDATE, mpt.FACTORY , mpt.MXPACKAGE, mpt.LINESERIAL , opt.STYLECODE, opt.STYLESIZE, opt.STYLECOLORSERIAL, opt.REVNO, opt.OPREVNO, opt.OPSERIAL
                                    from t_mx_opmt opm 
	                                    join t_mx_opdt_mc opt on 
		                                    opt.stylecode = opm.STYLECODE and opt.stylesize = opm.STYLESIZE 
		                                    and opt.stylecolorserial = opm.STYLECOLORSERIAL  
		                                    and opt.REVNO = opm.REVNO and opt.OPREVNO = opm.OPREVNO 	
	                                    join t_mx_mpdt mpt on 
		                                    mpt.MXPACKAGE = opm.MXPACKAGE 
                                    where mpt.FACTORY = ?P_FACTORY and  mpt.PLNSTARTDATE = ?P_PLNSTARTDATE
                            )t1 join t_mx_opdt opd on opd.stylecode = t1.stylecode and opd.STYLESIZE = t1.stylesize and opd.STYLECOLORSERIAL = t1.stylecolorserial 
			                            and opd.revno = t1.revno 
			                            and opd.OPREVNO = t1.oprevno and opd.OPSERIAL = t1.opserial
                            join t_cm_line li on li.LINESERIAL = opd.LINESERIAL
                            where	li.FACTORY = ?P_FACTORYLINE
                            group by opd.LINESERIAL, t1.mxpackage, t1.plnstartdate, t1.factory
                            order by t1.last_iot_data_receive_time desc;";

            var oraParams = new List<MySqlParameter>
            {
                new MySqlParameter("P_FACTORY", factoryId),
                new MySqlParameter("P_PLNSTARTDATE", plnStartDate),
                new MySqlParameter("P_FACTORYLINE", factoryId),
            };

            var listLine = MySqlDBManager.GetAll<LineEntity>(strSql, CommandType.Text, oraParams.ToArray());

            return listLine;
        }

        /// <summary>
        /// Get line by factory id in MySql
        /// </summary>
        /// <param name="factoryId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static List<LineEntity> GetLinesByFactoryId(string factoryId)
        {
            var queryStr = @"select LINESERIAL, linename, factory, inuse, BACKGROUNDCOLOR, MTOPLINE lineno, workers
                                from t_cm_line where factory = ?p_factory AND(INUSE = '1'  or mtopline in (select distinct lineno from t_mx_mpdt ))
                                order by MTOPLINE; ";

            var oracleParams = new List<MySqlParameter> { new MySqlParameter("p_factory", factoryId) };
            var lines = MySqlDBManager.GetAll<LineEntity>(queryStr, CommandType.Text, oracleParams.ToArray());

            return lines;
        }
        #endregion
    }
}
