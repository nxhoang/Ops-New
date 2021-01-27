using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using OPS_DAL.DAL;
using OPS_DAL.MesEntities;
using PKERP.MES.Domain.Interface.Dto;

namespace MES.Repositories
{
    public class IohtRepository : IIohtRepository
    {
        public async Task<Ioht> GetLastEntryByDateAsync(string mac, DateTime dt)
        {
            var dtText = dt.ToString("yyyyMMdd");
            var result = MySqlDBManager.GetAll<Ioht>($@"SELECT *
                                                        FROM T_MX_IOHT
                                                        WHERE
                                                        T_MX_IOHT.MAC = @mac
                                                        AND Date(T_MX_MPDT.EVENT_DATE) = Date(@dt)
                                                        ", System.Data.CommandType.Text, new MySql.Data.MySqlClient.MySqlParameter[] {
                new MySql.Data.MySqlClient.MySqlParameter("mac", mac),
                new MySql.Data.MySqlClient.MySqlParameter("dt", dt),
            });

            return await Task.FromResult<Ioht>(result.FirstOrDefault());
        }

    }
}