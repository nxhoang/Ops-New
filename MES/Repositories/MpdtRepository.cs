using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MySql.Data.MySqlClient;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using PKERP.MES.Domain.Interface.Dto;

namespace MES.Repositories
{
    public class MpdtRepository : IMpdtRepository
    {
        /// <summary>
        /// Get list of MES package by line
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="line"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public async Task<IEnumerable<Mpdt>> GetMesPackagesByLineAsync(string factory, string line, DateTime dt)
        {
            var dtText = dt.ToString("yyyyMMdd");
            var result = MySqlDBManager.GetAll<Mpdt>($@"SELECT DISTINCT T_MX_MPDT.*
                                                        FROM T_MX_MPDT
                                                        WHERE 
                                                        T_MX_MPDT.FACTORY = @factory
                                                        AND T_MX_MPDT.PLNSTARTDATE = @dt
                                                        AND T_MX_MPDT.LINESERIAL = @line
                                                        ", System.Data.CommandType.Text, new MySqlParameter[] {
                new MySqlParameter("factory", factory),
                new MySqlParameter("dt", dtText),
                new MySqlParameter("line", line)
            });

            return await Task.FromResult<IEnumerable<Mpdt>>(result);
        }

        public async Task<IEnumerable<Mpdt>> GetMesPackagesByDateAsync(string factory, DateTime dt)
        {
            var dtText = dt.ToString("yyyyMMdd");
            var result = MySqlDBManager.GetAll<Mpdt>($@"SELECT DISTINCT T_MX_MPDT.*
                                                        FROM T_MX_MPDT
                                                        WHERE 
                                                        T_MX_MPDT.FACTORY = @factory
                                                        AND T_MX_MPDT.PLNSTARTDATE = @dt
                                                        ", System.Data.CommandType.Text, new MySqlParameter[] {
                new MySqlParameter("factory", factory),
                new MySqlParameter("dt", dtText),
            });

            return await Task.FromResult<IEnumerable<Mpdt>>(result);
        }

        /// <summary>
        /// Get line dashboard information
        /// </summary>
        /// <param name="mxpackage">mes package</param>
        /// <returns></returns>
        public async Task<LineDashboardDataDto> GetLineDashBoardDtoAsync(string mxpackage)
        {
            var result = MySqlDBManager.GetAll<LineDashboardDataDto>($@"SELECT
                                                        T_MX_MPDT.MXPACKAGE,
                                                        T_MX_MPDT.MXTARGET AS target,
                                                        T_MX_MPDT.PLNSTARTDATE AS plan_start_date,
                                                        T_MX_MPDT.PLNENDDATE AS plan_end_date,
                                                        T_MX_MPDT.PLNACTSTARTDATE AS actual_start_date,
                                                        T_MX_MPDT.PLNACTENDDATE AS actual_end_date,
                                                        T_TP_MCHN_DTL.MAC AS final_assembly_machine_mac
                                                        FROM T_MX_MPDT
                                                        LEFT JOIN T_MX_OPMT ON T_MX_MPDT.MXPACKAGE = T_MX_OPMT.MXPACKAGE
                                                        LEFT JOIN T_MX_OPDT
                                                            ON T_MX_OPMT.STYLECODE = T_MX_OPDT.STYLECODE
                                                            AND T_MX_OPMT.STYLESIZE = T_MX_OPDT.STYLESIZE
                                                            AND T_MX_OPMT.STYLECOLORSERIAL = T_MX_OPDT.STYLECOLORSERIAL
                                                            AND T_MX_OPMT.REVNO = t_mx_opdt.REVNO
                                                            AND T_MX_OPMT.OPREVNO = t_mx_opdt.OPREVNO
                                                        LEFT JOIN T_TP_MCHN_DTL 
                                                            ON T_MX_OPDT.MCID = T_TP_MCHN_DTL.MCHN_DTL_CD
                                                        WHERE
                                                        T_MX_MPDT.MXPACKAGE = @mxpackage
                                                        AND IFNULL(FINALASSEMBLY, 1) = 1
                                                        LIMIT 1", System.Data.CommandType.Text, new MySqlParameter[] {
                new MySqlParameter("mxpackage", mxpackage)
            });

            return await Task.FromResult<LineDashboardDataDto>(result.FirstOrDefault());
        }

        /*----- Code by Dinh Van 2020-09-22 -----*/

        //Get mes package by date and factory
        public async Task<IEnumerable<Mpdt>> GetMesPackagesByDateAsync(string factory, string date)
        {
            //var result = MySqlDBManager.GetAll<Mpdt>($@"SELECT DISTINCT T_MX_MPDT.*,
            //                                            v.STYLECODE,
            //                                            v.STYLESIZE,
            //                                            v.STYLECOLORSERIAL,
            //                                            v.STYLECOLORWAYS,
            //                                            v.STYLENAME,
            //                                            v.REVNO,
            //                                            v.LINENAME,
            //                                            v.MXTARGET,
            //                                            v.BUYERSTYLENAME,
            //                                            v.BUYERSTYLECODE,
            //                                            x.AONO
            //                                            FROM T_MX_MPDT
            //                                            JOIN view_mpdt_opdt_mc v ON T_MX_MPDT.MXPACKAGE = v.MXPACKAGE
            //                                            JOIN v_mesgroup_ao x ON x.PACKAGEGROUP = v.PACKAGEGROUP
            //                                            WHERE 
            //                                            T_MX_MPDT.FACTORY = @factory
            //                                            AND T_MX_MPDT.PLNSTARTDATE = @dt
            //                                            ", System.Data.CommandType.Text, new MySqlParameter[] {
            //    new MySqlParameter("factory", factory),
            //    new MySqlParameter("dt", date),
            //});
            //update 20210127  
            var result = MySqlDBManager.GetAll<Mpdt>($@"
                                                        select * from (
                                                         Select T_MX_MPDT.MXPackage , 
                                                        v.STYLECODE,
                                                        v.STYLESIZE,
                                                        v.STYLECOLORSERIAL,
                                                        T_00_scmt.STYLECOLORWAYS,
                                                        T_00_stmt.STYLENAME,
                                                        v.REVNO,
                                                        t_cm_line.LINENAME,
                                                        T_MX_MPDT.MXTARGET,
                                                        T_00_stmt.BUYERSTYLENAME,
                                                        T_00_stmt.BUYERSTYLECODE,
                                                        x.AONO
                                                        FROM T_MX_MPDT 
                                                        join T_MX_MPMT v ON v.PACKAGEGROUP = T_MX_MPDT.PACKAGEGROUP
                                                        JOIN v_mesgroup_ao x ON x.PACKAGEGROUP = T_MX_MPDT.PACKAGEGROUP
                                                        left join T_00_stmt on v.STYLECODE = T_00_stmt.StyleCode 
                                                        left join T_00_scmt on v.STYLECODE = T_00_scmt.StyleCode And v.STYLECOLORSERIAL = T_00_scmt.STYLECOLORSERIAL 
                                                        join t_cm_line on T_MX_MPDT.LineSerial = t_cm_line.LINESERIAL 
                                                        WHERE 
                                                        T_MX_MPDT.FACTORY = @factory
                                                        AND T_MX_MPDT.PLNSTARTDATE = @dt
                                                        ) t1 
                                                        join (
                                                        select T_MX_opmt.MXPackage
                                                        from T_MX_MPDT
                                                        join T_MX_opmt on T_MX_opmt.MXPackage = T_MX_MPDT.MXPackage
                                                        Join t_mx_opdt on
                                                        T_MX_opmt.StyleCode = t_mx_opdt.StyleCode AND
                                                        T_MX_opmt.StyleSize = t_mx_opdt.StyleSize AND
                                                        T_MX_opmt.StyleColorSerial = t_mx_opdt.StyleColorSerial AND
                                                        T_MX_opmt.RevNo = t_mx_opdt.RevNo AND
                                                        T_MX_opmt.OPREVNO = t_mx_opdt.OPREVNO
                                                        JOIN t_mx_opdt_mc ON
                                                        t_mx_opdt.StyleCode = t_mx_opdt_mc.StyleCode AND
                                                        t_mx_opdt.StyleSize = t_mx_opdt_mc.StyleSize AND
                                                        t_mx_opdt.StyleColorSerial = t_mx_opdt_mc.StyleColorSerial AND
                                                        t_mx_opdt.RevNo = t_mx_opdt_mc.RevNo AND
                                                        t_mx_opdt.OPREVNO = t_mx_opdt_mc.OPREVNO AND
                                                        t_mx_opdt.OPSERIAL = t_mx_opdt_mc.OPSERIAL
                                                        Where T_MX_MPDT.PLNSTARTDATE = @dt and T_MX_MPDT.Factory = @factory
                                                        Group By T_MX_opmt.MXPackage
                                                        ) t2
                                                        on t1.mxpackage = t2.mxpackage
                                                        ", System.Data.CommandType.Text, new MySqlParameter[] {
                new MySqlParameter("factory", factory),
                new MySqlParameter("dt", date),
            });

            return await Task.FromResult<IEnumerable<Mpdt>>(result);
        }

        //Get Iot of mes package
        public async Task<IEnumerable<OpdtMc>> GetIotOfMesPackage(string mesPkg )
        {
            //var result = MySqlDBManager.GetObjects<OpdtMc>($@"select view_mpdt_opdt_mc.*, 
            //                                                    count(IOT_MODULE_MAC) as VAN_COUNT, 
            //                                                    t_hr_empm.CorporationCode, 
            //                                                    t_hr_empm.ImageName, 
            //                                                    t_hr_empm.Name as EmployeeName 
            //                                                    from view_mpdt_opdt_mc left 
            //                                                    join t_hr_empm on view_mpdt_opdt_mc.EMPID = t_hr_empm.EmployeeCode 
            //                                                    where MxPackage = '{mesPkg}' 
            //                                                    group by IOT_MODULE_MAC",
            //                                               System.Data.CommandType.Text,
            //                                               null);

            var result = MySqlDBManager.GetObjects<OpdtMc>($@"select t1.*, 
                                                                t2.CorporationCode, 
                                                                t2.ImageName, 
                                                                t2.Name as EmployeeName 
                                                                from (
	                                                                select * from view_mpdt_opdt_mc where MxPackage = '{mesPkg}' group by IOT_MODULE_MAC LIMIT 0, 100
                                                                ) t1 left join  t_hr_empm t2 on t1.EMPID = t2.EmployeeCode;",
                                                           System.Data.CommandType.Text,
                                                           null);
            return await Task.FromResult<IEnumerable<OpdtMc>>(result);
        }

        // Get info detail chart by mes package and date
        public async Task<IEnumerable<OpdtMc>> GetInfoChartByMesDate(string mesPkg, string date)
        {
            var result = MySqlDBManager.GetObjects<OpdtMc>($@"select * from view_mpdt_opdt_mc where MxPackage = '{mesPkg}' and MC_PAIR_TIME like '{date}%' group by MxPackage",
                                                           System.Data.CommandType.Text,
                                                           null);
            return await Task.FromResult<IEnumerable<OpdtMc>>(result);
        }

        /*----- Code by Dinh Van 2020-09-22 -----*/

        /*----- Code by Dinh Van 2020-11-04 -----*/
        //Get defect of mes package
        public async Task<IEnumerable<Defect>> GetDefectMXPackage(string mxpackage)
        {
            var result = MySqlDBManager.GetAll<Defect>($@"select t_mx_mpdt.MXPACKAGE, tb_aono.AONO, CONCAT(CONCAT(t_00_stmt.STYLECODE , '-'), t_00_stmt.BUYERSTYLENAME) as STYLETEXT,
                                                                CONCAT(CONCAT(t_00_stmt.BUYERSTYLECODE , '-'), t_00_stmt.BUYERSTYLENAME) as BUYERSTYLETEXT,
                                                                t_00_stmt.BUYER,
                                                                t_mx_opmt.STYLECOLORSERIAL,
                                                                t_mx_opmt.REVNO,
                                                                t_cm_line.LINENAME,
                                                                t_mx_mpdt.MXTARGET,
                                                                t_mx_mpdt.TOTAL_DEFECT
                                                                from t_mx_mpdt 
                                                                join t_mx_opmt on t_mx_opmt.MXPACKAGE = t_mx_mpdt.MXPACKAGE
                                                                join t_00_stmt on t_00_stmt.STYLECODE = t_mx_opmt.STYLECODE
                                                                join t_cm_line on t_mx_mpdt.LINESERIAL = t_cm_line.LINESERIAL
                                                                JOIN (
	                                                                SELECT 
                                                                        t_mx_ppkg.PACKAGEGROUP AS PACKAGEGROUP,
                                                                        t_mx_ppkg.AONO AS AONO
                                                                    FROM t_mx_ppkg
                                                                   GROUP BY t_mx_ppkg.PACKAGEGROUP , t_mx_ppkg.AONO 
                                                                ) as tb_aono on t_mx_mpdt.PACKAGEGROUP = tb_aono.PACKAGEGROUP
                                                                WHERE t_mx_mpdt.MXPACKAGE = @mxpackage", System.Data.CommandType.Text, new MySqlParameter[] {
                new MySqlParameter("mxpackage", mxpackage),
            });

            return await Task.FromResult<IEnumerable<Defect>>(result);
        }
        /*----- Code by Dinh Van 2020-11-04 -----*/

        /*----- Code by Dinh Van 2020-12-07 -----*/
        //Get list dashboard end line spection
        public async Task<IEnumerable<Mpdt>> GetEndLineSpection(string factory, string date)
        {
            var result = MySqlDBManager.GetAll<Mpdt>($@"select mp.MXPACKAGE, 
                                                                mp.PLNSTARTDATE, 
                                                                mp.FACTORY, 
                                                                mp.MXTARGET,
                                                                mp.LINESERIAL, 
                                                                mp.TOTAL_DEFECT as TotalDefect,
                                                                 GREATEST(mp.MX_IOT_COMPLETED, mp.MX_IOT_COMPLETED_DGS) as OUTPUT,
                                                                 ln.LINENAME
                                                                from t_mx_mpdt mp
                                                                join t_cm_line ln on ln.LINESERIAL = mp.LINESERIAL
                                                            where mp.FACTORY = @factory AND mp.PLNSTARTDATE = @dt", System.Data.CommandType.Text, new MySqlParameter[] {
                                                                            new MySqlParameter("factory", factory),
                                                                            new MySqlParameter("dt", date),
                                                                        });

            return await Task.FromResult<IEnumerable<Mpdt>>(result);
        }

        /*----- Start code by Dinh Van 2021-01-05 -----*/
        public async Task<IEnumerable<Defe>> GetTotalDefectAsync(string factory, string lineId, string startDate, string endDate, string package)
        {
            var param = new List<MySqlParameter>
            {
                new MySqlParameter("factory", factory)
            };

            string whereLineId = string.Empty;
            string whereStartDate = string.Empty;
            string whereEndDate = string.Empty;
            string wherePackage = string.Empty;
            if (!string.IsNullOrEmpty(lineId))
            {
                param.Add(new MySqlParameter("lineId", lineId));
                whereLineId = "AND t_mx_mpdt.LINESERIAL = @lineId";
            }
            if (!string.IsNullOrEmpty(startDate))
            {
                param.Add(new MySqlParameter("startDate", startDate));
                whereStartDate = "AND t_mx_mpdt.PLNSTARTDATE >= @startDate";
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                param.Add(new MySqlParameter("endDate", endDate));
                whereEndDate = "AND t_mx_mpdt.PLNSTARTDATE <= @endDate";
            }

            if (!string.IsNullOrEmpty(package))
            {
                param.Add(new MySqlParameter("package", package));
                wherePackage = "AND t_mx_mpdt.MXPACKage = @package";
            }

            var result = MySqlDBManager.GetAll<Defe>($@"SELECT t_mx_opdf.DEFECTCAT , 
                                                                t_cm_mcmt.CODE_NAME as DEFECTNAME , 
                                                                SUM( t_mx_opdf.QTY) DEFQTY, 
                                                                CONCAT(t_mx_opdf.DEFECTCAT, '-', t_cm_mcmt.CODE_NAME) AS DEFECTCATNAME
                                                        FROM t_mx_opdf
                                                        LEFT JOIN t_mx_mpdt on t_mx_opdf.mxPackage = t_mx_mpdt.mxPackage
                                                        LEFT JOIN t_mx_opmt ON t_mx_opdf.mxPackage = t_mx_opmt.mxPackage
                                                        LEFT JOIN t_mx_opdt on t_mx_opmt.StyleCode = t_mx_opdt.StyleCode
                                                        AND t_mx_opmt.StyleSize = t_mx_opdt.StyleSize
                                                        AND t_mx_opmt.StyleColorSerial = t_mx_opdt.StyleColorSerial
                                                        AND t_mx_opmt.RevNo = t_mx_opdt.RevNo
                                                        AND t_mx_opmt.OPRevNo = t_mx_opdt.OPRevNo
                                                        AND t_mx_opdf.OPSERIAL = t_mx_opdt.OPSERIAL
                                                        LEFT JOIN t_cm_mcmt on t_mx_opdf.DEFECTCAT = t_cm_mcmt.S_CODE and t_cm_mcmt.M_CODE = 'DefectCat'
                                                        WHERE t_mx_mpdt.FACTORY = @factory
                                                        {whereLineId} {whereStartDate} {whereEndDate} {wherePackage}
                                                        Group By t_mx_opdf.DEFECTCAT , t_cm_mcmt.CODE_NAME", System.Data.CommandType.Text, param.ToArray());

            return await Task.FromResult<IEnumerable<Defe>>(result);
        }

        public async Task<IEnumerable<Defe>> GetDetailDefectAsync(string defectCat, string startDate, string endDate, string lineId, string package)
        {
            var param = new List<MySqlParameter> {
                new MySqlParameter("defectcat", defectCat),
                new MySqlParameter("startDate", startDate),
                new MySqlParameter("endDate", endDate),
            };
            string whereLineId = string.Empty;
            string wherePackage = string.Empty;
            if (!string.IsNullOrEmpty(lineId))
            {
                param.Add(new MySqlParameter("lineId", lineId));
                whereLineId = "AND t_mx_mpdt.LINESERIAL = @lineId";
            }

            if (!string.IsNullOrEmpty(package))
            {
                param.Add(new MySqlParameter("package", package));
                wherePackage = "AND t_mx_mpdt.MXPACKage = @package";
            }

            var result = MySqlDBManager.GetAll<Defe>($@"SELECT t_mx_opdf.DEFECTCODE , t_cm_dfmt.DEFECTDESC , SUM( t_mx_opdf.QTY) DefQty
                                                        FROM t_mx_opdf
                                                        LEFT JOIN t_mx_mpdt ON t_mx_opdf.mxPackage = t_mx_mpdt.mxPackage
                                                        LEFT JOIN t_mx_opmt ON t_mx_opdf.mxPackage = t_mx_opmt.mxPackage
                                                        LEFT JOIN t_mx_opdt ON t_mx_opmt.StyleCode = t_mx_opdt.StyleCode
                                                        AND t_mx_opmt.StyleSize = t_mx_opdt.StyleSize
                                                        AND t_mx_opmt.StyleColorSerial = t_mx_opdt.StyleColorSerial
                                                        AND t_mx_opmt.RevNo = t_mx_opdt.RevNo
                                                        AND t_mx_opmt.OPRevNo = t_mx_opdt.OPRevNo
                                                        AND t_mx_opdf.OPSERIAL = t_mx_opdt.OPSERIAL
                                                        LEFT JOIN t_cm_mcmt on t_mx_opdf.DEFECTCAT = t_cm_mcmt.S_CODE and t_cm_mcmt.M_CODE = 'DefectCat'
                                                        LEFT JOIN t_cm_dfmt on t_mx_opdf.DEFECTCODE = t_cm_dfmt.DEFECTCODE
                                                        WHERE t_mx_opdf.DEFECTCAT = @defectcat AND t_mx_mpdt.PLNSTARTDATE >= @startDate AND t_mx_mpdt.PLNSTARTDATE <= @endDate {whereLineId} {wherePackage}
                                                        GROUP BY t_mx_opdf.DEFECTCODE , t_cm_dfmt.DEFECTDESC", System.Data.CommandType.Text, param.ToArray());

            return await Task.FromResult<IEnumerable<Defe>>(result);
        }

        public async Task<IEnumerable<LineEntity>> GetLineByFactoryAsync(string factoryId)
        {
            var result = MySqlDBManager.GetAll<LineEntity>($@"SELECT LINESERIAL , LINENAME, FACTORY
                                                        FROM t_cm_line
                                                        WHERE t_cm_line.FACTORY = @factory", System.Data.CommandType.Text, new MySqlParameter[] {
                                                                        new MySqlParameter("factory", factoryId)
                                                                    });

            return await Task.FromResult<IEnumerable<LineEntity>>(result);
        }

        public async Task<IEnumerable<Mpdt>> GetPackageByFactoryLineAsync(string factoryId, string lineId, string startDate, string endDate)
        {
            var result = MySqlDBManager.GetAll<Mpdt>($@"SELECT MXPACKAGE
                                                        FROM t_mx_mpdt
                                                        WHERE FACTORY = @factory 
                                                               AND LINESERIAL = @line 
                                                               AND @startDate <= PLNSTARTDATE 
                                                               AND PLNSTARTDATE <= @endDate", System.Data.CommandType.Text, new MySqlParameter[] {
                                                                        new MySqlParameter("factory", factoryId),
                                                                        new MySqlParameter("line", lineId),
                                                                        new MySqlParameter("startDate", startDate),
                                                                        new MySqlParameter("endDate", endDate)
                                                                    });

            return await Task.FromResult<IEnumerable<Mpdt>>(result);
        }

        /*----- End code by Dinh Van 2021-01-05 -----*/

        /*----- Start code by Dinh Van 2021-01-06 -----*/
        public async Task<IEnumerable<OpdtMc>> GetLobRateByMesPackage(string mesPkg)
        {
            var result = MySqlDBManager.GetObjects<OpdtMc>($@"Select t_mx_mpdt.MXPackage,
                                                                MAX(IFNULL(t_mx_opdt.OPTIME,0)) LongestOPTime,
                                                                SUM(IFNULL(t_mx_opdt.OPTIME,0)) TotalOPTime,
                                                                COUNT(t_mx_opdt.OPSERIAL) NumOfOP,
                                                                SUM(IFNULL(t_mx_opdt.OPTIME,0)) / (MAX(IFNULL(t_mx_opdt.OPTIME,0)) * COUNT(t_mx_opdt.OPSERIAL)) * 100 as LOB
                                                            from t_mx_mpdt
                                                                join t_mx_opmt on t_mx_mpdt.MXPackage = t_mx_opmt.MXPackage
                                                                join t_mx_opdt on
                                                                t_mx_opmt.STYLECODE = t_mx_opdt.STYLECODE and
                                                                t_mx_opmt.STYLESize = t_mx_opdt.STYLESize and
                                                                t_mx_opmt.STYLECOlorSerial = t_mx_opdt.STYLECOlorSerial and
                                                                t_mx_opmt.RevNo = t_mx_opdt.RevNo and
                                                                t_mx_opmt.OPREVNO = t_mx_opdt.OPREVNO
                                                            Where t_mx_mpdt.MXPackage= '{mesPkg}'
                                                            Group By t_mx_mpdt.MXPackage",
                                                           System.Data.CommandType.Text,
                                                           null);
            return await Task.FromResult<IEnumerable<OpdtMc>>(result);
        }
        /*----- End code by Dinh Van 2021-01-06 -----*/

        /*----- Start code by Dinh Van 2021-01-22 -----*/
        public async Task<IEnumerable<OpdtMc>> GetOperationPlan(string mesPkg)
        {
            var result = MySqlDBManager.GetObjects<OpdtMc>($@"select dt.OPSERIAL, 
                                                                mt.MXPACKAGE, 
                                                                dt.LAST_IOT_DATA, 
                                                                dt.LAST_IOT_DATA_DGS, 
                                                                dt.OPNAME, 
                                                                dt.OPGROUP, 
                                                                pt.MXTARGET, 
                                                                pt.PLNSTARTDATE, 
                                                                dt.OPGROUP, 
                                                                dt.IOTTYPE, 
                                                                dt.DISPLAYCOLOR
                                                            from t_mx_opdt dt
                                                            join t_mx_opmt mt on dt.STYLECODE = mt.STYLECODE 
                                                                and dt.STYLESIZE = mt.STYLESIZE 
                                                                and dt.STYLECOLORSERIAL = mt.STYLECOLORSERIAL 
                                                                and dt.REVNO = mt.REVNO 
                                                                and dt.OPREVNO = mt.OPREVNO
                                                            join t_mx_mpdt pt on mt.MXPACKAGE = pt.MXPACKAGE
                                                            where mt.MXPACKAGE = '{mesPkg}'
                                                            order by dt.OPSERIAL;",
                                                           System.Data.CommandType.Text,
                                                           null);
            return await Task.FromResult<IEnumerable<OpdtMc>>(result);
        }
        /*----- End code by Dinh Van 2021-01-22 -----*/
    }
}