using OPS_DAL.MesEntities;
using PKERP.MES.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MES.Repositories
{
    public interface IIohtRepository
    {
        Task<Ioht> GetLastEntryByDateAsync(string mac, DateTime dt);
    }
}