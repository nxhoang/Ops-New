using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using System.Collections.Generic;
using System.Data;

namespace OPS_DAL.MesBus
{
    public class MXOpdtBus
    {
        public static List<MXOpdt> GetMXPkdOPSum(string MXPackage) {
            string query = "";
             
            query = $@"
select t_mx_opdt.OPSERIAL, IFNULL(t_mx_opdt.NEXTOP, '') as NEXTOP , IFNULL(t_mx_opdt.OPGROUP,'N/A') as OPGROUP, t_mx_opmt.MXPACKAGE, 
t_mx_mpmt.STYLECODE , t_mx_mpmt.STYLESIZE , t_mx_mpmt.STYLECOLORSERIAL , t_mx_mpmt.REVNO , t_mx_mpdt.MXTARGET ,
IFNULL(v_mpmt_opdt_iot.IOTCounter,0) IOTCounter, 
IFNULL(T_CM_MCMT.CODE_NAME , 'N/A') as OPGROUPNAME ,
LEAD( IFNULL(t_mx_opdt.OPGROUP,'N/A') ) OVER (
        ORDER BY IFNULL(t_mx_opdt.OPGROUP,'N/A'), t_mx_opdt.OPSERIAL
    ) NEXTOPGROUP, 
t_mx_opdt.OPSERIAL , t_mx_opdt.OPNUM , t_mx_opdt.OPNAME , t_mx_opdt.NEXTOP ,  t_mx_opdt.DISPLAYCOLOR , t_mx_opdt.EmployeeCode,
(select SUM( t_mx_opdt_mc.LAST_IOT_DATA ) from t_mx_opdt_mc where     
    t_mx_opdt_mc.STYLECODE = t_mx_opdt.STYLECODE and
    t_mx_opdt_mc.STYLESIZE = t_mx_opdt.STYLESIZE and
    t_mx_opdt_mc.STYLECOLORSERIAL = t_mx_opdt.STYLECOLORSERIAL and
    t_mx_opdt_mc.REVNO = t_mx_opdt.REVNO and
    t_mx_opdt_mc.OPREVNO = t_mx_opdt.OPREVNO and 
    t_mx_opdt_mc.OPSERIAL = t_mx_opdt.OPSERIAL ) as OPIOTCOMPQTY ,
(select SUM( t_mx_opdt_mc.LAST_IOT_DATA_DGS ) from t_mx_opdt_mc where     
    t_mx_opdt_mc.STYLECODE = t_mx_opdt.STYLECODE and
    t_mx_opdt_mc.STYLESIZE = t_mx_opdt.STYLESIZE and
    t_mx_opdt_mc.STYLECOLORSERIAL = t_mx_opdt.STYLECOLORSERIAL and
    t_mx_opdt_mc.REVNO = t_mx_opdt.REVNO and
    t_mx_opdt_mc.OPREVNO = t_mx_opdt.OPREVNO and 
    t_mx_opdt_mc.OPSERIAL = t_mx_opdt.OPSERIAL ) as OPDGSCOMPQTY ,  
(select Max( t_mx_opdt_mc.LAST_IOT_DATA_RECEIVE_TIME_DGS ) from t_mx_opdt_mc where     
    t_mx_opdt_mc.STYLECODE = t_mx_opdt.STYLECODE and
    t_mx_opdt_mc.STYLESIZE = t_mx_opdt.STYLESIZE and
    t_mx_opdt_mc.STYLECOLORSERIAL = t_mx_opdt.STYLECOLORSERIAL and
    t_mx_opdt_mc.REVNO = t_mx_opdt.REVNO and
    t_mx_opdt_mc.OPREVNO = t_mx_opdt.OPREVNO and 
    t_mx_opdt_mc.OPSERIAL = t_mx_opdt.OPSERIAL ) as DGSLASTDATE ,  
(select MAX( t_mx_opdt_mc.LAST_IOT_DATA_RECEIVE_TIME ) from t_mx_opdt_mc where     
    t_mx_opdt_mc.STYLECODE = t_mx_opdt.STYLECODE and
    t_mx_opdt_mc.STYLESIZE = t_mx_opdt.STYLESIZE and
    t_mx_opdt_mc.STYLECOLORSERIAL = t_mx_opdt.STYLECOLORSERIAL and
    t_mx_opdt_mc.REVNO = t_mx_opdt.REVNO and
    t_mx_opdt_mc.OPREVNO = t_mx_opdt.OPREVNO and 
    t_mx_opdt_mc.OPSERIAL = t_mx_opdt.OPSERIAL ) as IOTLASTDATE
from t_mx_mpmt
join t_mx_mpdt on t_mx_mpdt.PACKAGEGROUP = t_mx_mpmt.PACKAGEGROUP
join t_mx_opmt on t_mx_opmt.MXPACKAGE = t_mx_mpdt.MXPACKAGE 
join t_mx_opdt on 
    t_mx_opmt.STYLECODE = t_mx_opdt.STYLECODE and
    t_mx_opmt.STYLESIZE = t_mx_opdt.STYLESIZE and
    t_mx_opmt.STYLECOLORSERIAL = t_mx_opdt.STYLECOLORSERIAL and
    t_mx_opmt.REVNO = t_mx_opdt.REVNO and
    t_mx_opmt.OPREVNO = t_mx_opdt.OPREVNO 
left join ( Select MXPACKAGE , OPREVNO , OPSERIAL , COUNT( IOT_MODULE_MAC ) as IOTCounter From v_mpmt_opdt_iot Where MXPACKAGE = '{MXPackage}'  Group By MXPACKAGE , OPREVNO , OPSERIAL ) v_mpmt_opdt_iot ON 
    t_mx_mpdt.MXPACKAGE  = v_mpmt_opdt_iot.MXPACKAGE AND 
    t_mx_opdt.OPREVNO = v_mpmt_opdt_iot.OPREVNO AND 
    t_mx_opdt.OPSERIAL= v_mpmt_opdt_iot.OPSERIAL
left join T_CM_MCMT on t_mx_opdt.OPGROUP = T_CM_MCMT.S_CODE and  T_CM_MCMT.M_CODE ='OPGroup' 
where 1=1  
and t_mx_mpdt.MXPACKAGE = '{MXPackage}'
";
            return MySqlDBManager.GetObjectsConvertType<MXOpdt>(query, CommandType.Text, null, 1200);
        }
    }
}
