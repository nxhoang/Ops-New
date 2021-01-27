using OPS_DAL.MesEntities;
using PKERP.MES.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MES.Repositories
{
    public interface IMpdtRepository
    {
        Task<IEnumerable<Mpdt>> GetMesPackagesByLineAsync(string factory, string line, DateTime dt);
        Task<IEnumerable<Mpdt>> GetMesPackagesByDateAsync(string factory, DateTime dt);
        Task<LineDashboardDataDto> GetLineDashBoardDtoAsync(string mxpackage);
        Task<IEnumerable<Mpdt>> GetMesPackagesByDateAsync(string factory, string date);
        Task<IEnumerable<OpdtMc>> GetIotOfMesPackage(string mesPkg);
        Task<IEnumerable<OpdtMc>> GetInfoChartByMesDate(string mesPkg, string date);
        Task<IEnumerable<Defect>> GetDefectMXPackage(string mxpackage);
        Task<IEnumerable<Mpdt>> GetEndLineSpection(string factory, string date);
        Task<IEnumerable<Defe>> GetTotalDefectAsync(string factory, string lineId, string startDate, string endDate, string package);
        Task<IEnumerable<Defe>> GetDetailDefectAsync(string defectCat, string startDate, string endDate, string lineId, string package);
        Task<IEnumerable<LineEntity>> GetLineByFactoryAsync(string factoryId);
        Task<IEnumerable<Mpdt>> GetPackageByFactoryLineAsync(string factoryId, string lineId, string startDate, string endDate);
        Task<IEnumerable<OpdtMc>> GetLobRateByMesPackage(string mesPkg);
        Task<IEnumerable<OpdtMc>>GetOperationPlan(string mesPkg);


    }
}