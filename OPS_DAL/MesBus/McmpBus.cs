using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.MesBus
{
    public class McmpBus
    {
        private readonly MySqlDBManager _mySqlDBManager = new MySqlDBManager();
        private readonly string _mySqlConn = ConstantGeneric.ConnectionStrMesMySql;
        private readonly string _DGSmySqlConn = ConstantGeneric.ConnectionStrDgsMySql;

        public async Task<Mcmp> GetMachineIotByMachineAsync(string MACHINE_ID)
        {
            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(_mySqlConn, $"Select * From t_mx_mcmp Where MACHINE_ID = '{MACHINE_ID}' ", System.Data.CommandType.Text, null);
            if (res !=null && res.Count > 0)
                return res[0];
            else return null;
        }
        public async Task<Mcmp> GetMachineIotByIoTAsync(string IOT_MAC)
        {
            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(_mySqlConn, $"Select * From t_mx_mcmp Where IOT_MAC = '{IOT_MAC}' ", System.Data.CommandType.Text, null);
            if (res != null && res.Count > 0)
                return res[0];
            else return null;
        }
        public async Task<bool> DeleteMachineIotByIdAsync(Int64 Id)
        {
            return await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, $"Delete from t_mx_mcmp Where ID = {Id} ", System.Data.CommandType.Text, null);
        }
        public async Task<string> InsertMachineIotAsync(Mcmp mcmp)
        {
            try
            {
                await _mySqlDBManager.ExecuteNonQueryAsync(
                    conn: _mySqlConn,
                    commandText: $" Insert Into t_mx_mcmp(MACHINE_ID , IOT_MAC , UPDATE_TIME,  IOT_DEVICE_TYPE) " +
                        $" Values ('{mcmp.MACHINE_ID}' , '{mcmp.IOT_MAC}' , sysdate() ,  '{mcmp.IOT_DEVICE_TYPE}') ",
                    commandType: System.Data.CommandType.Text,
                    parameters: null);
                return "OK";
            }
            catch (Exception ex)
            {
                return $"Fail: {ex.Message}";
            }
        }
        public async Task<string> UpdateMachineIotMapping(Mcmp mcmp)
        {
            try
            {
                var resByMachine = await GetMachineIotByMachineAsync(mcmp.MACHINE_ID);
                if (resByMachine != null) await DeleteMachineIotByIdAsync(resByMachine.ID);

                var resByMAC = await GetMachineIotByIoTAsync(mcmp.IOT_MAC);
                if (resByMAC != null) await DeleteMachineIotByIdAsync(resByMAC.ID);

                var DgsMcmp = await GetMachineDGSAsync(mcmp.IOT_MAC);

                if (DgsMcmp != null)
                    mcmp.IOT_DEVICE_TYPE = DgsMcmp.IOT_DEVICE_TYPE;
                return await InsertMachineIotAsync(mcmp);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #region DGS
        public async Task<Mcmp> GetMachineDGSAsync(string Mac)
        {
            if (String.IsNullOrEmpty(Mac)) return null;

            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(_DGSmySqlConn,
                $" Select t_dg_iot_status.MacAddress as Machine_ID , t_dg_sppl_info.SpplName as IOT_DEVICE_TYPE " +
                $" From pkdgs_db.t_dg_iot_status " +
                $" Left join t_dg_sppl_info ON t_dg_iot_status.IotSpplCode = t_dg_sppl_info.SpplCode " +
                $" Where MacAddress = '{Mac}' ",
                System.Data.CommandType.Text, null);

            if (res != null && res.Count > 0)
                return res[0];
            else return null;
        }
        #endregion
    }
}
