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
        
#region MES
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
        public async Task<Mcmp> GetMachineIotByMachineIoTAsync(string IOT_MAC, string MACHINE_ID)
        {
            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(
                _mySqlConn,
                $"Select * From t_mx_mcmp " +
                $"Where IOT_MAC = '{IOT_MAC}' and MACHINE_ID = '{MACHINE_ID}' ",
                System.Data.CommandType.Text,
                null);
            if (res != null && res.Count > 0)
                return res[0];
            else return null;
        }
        public async Task<bool> DeleteMachineIotByIdAsync(Int64 Id)
        {
            return await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, $"Delete from t_mx_mcmp Where ID = {Id} ", System.Data.CommandType.Text, null);
        }
        public async Task<string> InsertMachineIotAsync(Mcmp mcmp, string userid)
        {
            try
            {
                await _mySqlDBManager.ExecuteNonQueryAsync(
                    conn: _mySqlConn,
                    commandText: $" Insert Into t_mx_mcmp(MACHINE_ID , IOT_MAC , UPDATE_TIME, IOT_DEVICE_TYPE, IOT_POSITION ) " +
                        $" Values ('{mcmp.MACHINE_ID}' , '{mcmp.IOT_MAC}' , sysdate() ,  '{mcmp.IOT_DEVICE_TYPE}', '{mcmp.IOT_POSITION}') ",
                    commandType: System.Data.CommandType.Text,
                    parameters: null);
                return "OK";
            }
            catch (Exception ex)
            {
                return $"Fail: {ex.Message}";
            }
        }
        public async Task<string> UpdateMachineIotMappingAsync(Mcmp mcmp, string userid)
        {
            try
            {
                var existMapping = await GetMachineIotByMachineIoTAsync(mcmp.IOT_MAC, mcmp.MACHINE_ID);
                if (existMapping == null)
                {
                    var resByMachine = await GetMachineIotByMachineAsync(mcmp.MACHINE_ID);
                    if (resByMachine != null) await DeleteMachineIotByIdAsync(resByMachine.ID);

                    var resByMAC = await GetMachineIotByIoTAsync(mcmp.IOT_MAC);
                    if (resByMAC != null) await DeleteMachineIotByIdAsync(resByMAC.ID);

                    var DgsMcmp = await GetDGSIotStatusAsync(mcmp.IOT_MAC);

                    if (DgsMcmp != null)
                        mcmp.IOT_DEVICE_TYPE = DgsMcmp.IOT_DEVICE_TYPE;
                    var res = await InsertMachineIotAsync(mcmp, userid);

                    await UpdateOpdtmc(mcmp);
                    return res;
                }
                else {
                    existMapping.IOT_POSITION = mcmp.IOT_POSITION;
                    await UpdateMachineIotAsync(existMapping);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<bool> UpdateMachineIotAsync(Mcmp mcmp)
        {
            try
            {
                var query = $@"
Update t_mx_mcmp 
Set 
IOT_POSITION = '{mcmp.IOT_POSITION}'
, UPDATE_TIME = sysdate() 
Where ID = {mcmp.ID}
";
                await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, query, System.Data.CommandType.Text, null);
                return true;
            }
            catch { return false; }
        }
        public async Task<bool> UpdateOpdtmc(Mcmp mcmp) {
            try
            {
                var query = $@"
Select * 
From t_mx_opdt_mc 
Where ( ( MCID = '{mcmp.MACHINE_ID}' AND IOT_MODULE_MAC != '{mcmp.IOT_MAC}' ) OR ( MCID != '{mcmp.MACHINE_ID}' AND IOT_MODULE_MAC = '{mcmp.IOT_MAC}' ) )
And Date(MC_PAIR_TIME) = current_date() 
";
                var res =await _mySqlDBManager.GetAllAsync<dynamic>(_mySqlConn, query, System.Data.CommandType.Text, null);

                if (res != null && res.Count > 0)
                {
                    query = $@"
Update t_mx_opdt_mc 
Set 
MCID = '{mcmp.MACHINE_ID}' 
, IOT_MODULE_MAC = '{mcmp.IOT_MAC}'
Where ID in  ({res.Select(e=>e.ID)})
";
                    await _mySqlDBManager.ExecuteNonQueryAsync(_mySqlConn, query, System.Data.CommandType.Text, null);
                }

                return true;
            }
            catch {
                return false;
            }
        }
#endregion


#region DGS
        public async Task<Mcmp> GetDGSIotStatusAsync(string Mac)
        {
            if (String.IsNullOrEmpty(Mac)) return null;

            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(
                conn: _DGSmySqlConn,
                commandText: 
                    $" Select t_dg_iot_status.MacAddress as Machine_ID , t_dg_sppl_info.SpplName as IOT_DEVICE_TYPE " + 
                    $" From pkdgs_db.t_dg_iot_status " + 
                    $" Left join t_dg_sppl_info ON t_dg_iot_status.IotSpplCode = t_dg_sppl_info.SpplCode " + 
                    $" Where MacAddress = '{Mac}' ",
                commandType: System.Data.CommandType.Text,
                parameters: null);

            if (res != null && res.Count > 0)
                return res[0];
            else return null;
        }
        public async Task<Mcmp> GetDGSMachineIotByMachineAsync(string MACHINE_ID) {
            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(
                conn: _DGSmySqlConn,
                commandText: $"Select Seq as ID From t_dg_mchn_iot_mapping Where MachineId = '{MACHINE_ID}' ",
                commandType: System.Data.CommandType.Text,
                parameters: null);
            if (res != null && res.Count > 0)
                return res[0];
            else return null;
        }
        public async Task<Mcmp> GetDGSMachineIotByIoTAsync(string IOT_MAC) {
            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(
                    conn: _DGSmySqlConn,
                    commandText: $"Select Seq as ID From t_dg_mchn_iot_mapping Where MacAddress = '{IOT_MAC}' ",
                    commandType: System.Data.CommandType.Text,
                    parameters: null);
            if (res != null && res.Count > 0)
                return res[0];
            else return null;
        }
        public async Task<Mcmp> GetDGSMachineIotByMachineIoTAsync(string IOT_MAC, string MACHINE_ID)
        {
            var res = await _mySqlDBManager.GetAllAsync<Mcmp>(
                    conn: _DGSmySqlConn,
                    commandText: 
                        $"Select Seq as ID " +
                        $"From t_dg_mchn_iot_mapping " +
                        $"Where MacAddress = '{IOT_MAC}' and MachineId = '{MACHINE_ID}' ",
                    commandType: System.Data.CommandType.Text,
                    parameters: null);
            if (res != null && res.Count > 0)
                return res[0];
            else return null;
        }
        public async Task<bool> DeleteDGSMachineIotByIdAsync(Int64 Id)
        {
            return await _mySqlDBManager.ExecuteNonQueryAsync(
                conn: _DGSmySqlConn,
                commandText: $"Delete from t_dg_mchn_iot_mapping Where Seq = {Id} ",
                commandType: System.Data.CommandType.Text,
                parameters: null);
        }
        public async Task<string> InsertDGSMachineIotAsync(Mcmp mcmp)
        {
            try
            {
                await _mySqlDBManager.ExecuteNonQueryAsync(
                    conn: _DGSmySqlConn,
                    commandText: $" Insert Into t_dg_mchn_iot_mapping(MachineId , MacAddress , CrtDttm , `position`) " +
                        $" Values ( '{mcmp.MACHINE_ID}' , '{mcmp.IOT_MAC}' , sysdate() , '{mcmp.IOT_POSITION}' ) ",
                    commandType: System.Data.CommandType.Text,
                    parameters: null);
                return "OK";
            }
            catch (Exception ex)
            {
                return $"Fail: {ex.Message}";
            }
        }
        public async Task<string> UpdateDGSMachineIotMappingAsync(Mcmp mcmp, string userid)
        {
            try
            {
                var existMapping = await GetDGSMachineIotByMachineIoTAsync(mcmp.IOT_MAC, mcmp.MACHINE_ID);
                if (existMapping == null)
                {
                    var resByMachine = await GetDGSMachineIotByMachineAsync(mcmp.MACHINE_ID);
                    if (resByMachine != null) await DeleteDGSMachineIotByIdAsync(resByMachine.ID);

                    var resByMAC = await GetDGSMachineIotByIoTAsync(mcmp.IOT_MAC);
                    if (resByMAC != null) await DeleteDGSMachineIotByIdAsync(resByMAC.ID);

                    var DgsMcmp = await GetDGSIotStatusAsync(mcmp.IOT_MAC);

                    if (DgsMcmp != null)
                        mcmp.IOT_DEVICE_TYPE = DgsMcmp.IOT_DEVICE_TYPE;
                    else
                    {
                        //Insert into pkdgs_db.t_dg_iot_status
                        await _mySqlDBManager.ExecuteNonQueryAsync(
                            conn: _DGSmySqlConn,
                            commandText: $" Insert Into t_dg_iot_status(MacAddress , UDT_DTTM_UTC ) " +
                                $" Values ('{mcmp.IOT_MAC}' ,  utc_timestamp() ) ",
                            commandType: System.Data.CommandType.Text,
                            parameters: null);
                    }
                    return await InsertMachineIotAsync(mcmp, userid);
                }
                else {
                    existMapping.IOT_POSITION = mcmp.IOT_POSITION;
                    await UpdateDGSMachineIotAsync(existMapping, userid);
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<bool> UpdateDGSMachineIotAsync(Mcmp mcmp, string userid)
        {
            try
            {
                var query = $@"
Update t_dg_mchn_iot_mapping 
Set 
`position` = '{mcmp.IOT_POSITION}'
, CrtDttm = sysdate() 
, updt_usrid = '{userid}' 
Where Seq = {mcmp.ID}
";
                await _mySqlDBManager.ExecuteNonQueryAsync(_DGSmySqlConn, query, System.Data.CommandType.Text, null);
                return true;
            }
            catch { return false; }
        }
        #endregion
    }
}
