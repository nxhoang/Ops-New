using OPS_DAL.DAL;
using OPS_DAL.DgsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.DgsBus
{
    public static class IotMachineTimeBus
    {
        public static List<IotMachineTime> GetDGSIotMachineTime(string inputDate) {
            return MySqlDBManager.GetObjects<IotMachineTime>($"select MacAddress, ExcDttm, PowerTime, MotoTime, ActTime from t_dg_iot_event_machine_time where ExcDttm like '{inputDate}%' ", System.Data.CommandType.Text, null, OPS_Utils.ConstantGeneric.ConnectionStrDgsMySql);
        }
    }
}
