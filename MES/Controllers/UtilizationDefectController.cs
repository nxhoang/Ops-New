using MES.Repositories;
using OPS_DAL.MesEntities;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class UtilizationDefectController : Controller
    {
        private readonly IMpdtRepository _mpdtRepo;
        public UtilizationDefectController(IMpdtRepository mpdtRepo)
        {
            _mpdtRepo = mpdtRepo;
        }
        // GET: UtilizationDefect
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Employee()
        {
            return View();
        }

        public async Task<JsonResult> GetDefectTreeMXPackage(string mxpackage)
        {
            if (string.IsNullOrWhiteSpace(mxpackage))
                return Json(new FailedTaskResult<List<Defect>>("MES package cannot empty"), JsonRequestBehavior.AllowGet);
            try
            {
                var item = await _mpdtRepo.GetDefectMXPackage(mxpackage);
                return Json(new SuccessTaskResult<IEnumerable<Defect>>(item), JsonRequestBehavior.AllowGet);
            } catch(Exception ex)
            {
                var mes = ex.Message;
            }

            return null;
            
        }

        public ActionResult Inspection()
        {
            return View();
        }

        public async Task<JsonResult> GetListEndLineSpection(string factory, string date)
        {
            if (string.IsNullOrWhiteSpace(factory))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            try
            {
                var item = await _mpdtRepo.GetEndLineSpection(factory, date);
                return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(item), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
            }

            return null;

        }

        public ActionResult MainGroupDefect()
        {
            return View();
        }

        public async Task<JsonResult> GetListTotalDefect(string factory, string lineId, string startDate, string endDate, string package)
        {
            if (string.IsNullOrWhiteSpace(factory))
                return Json(new FailedTaskResult<List<Defe>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            try
            {
                var item = await _mpdtRepo.GetTotalDefectAsync(factory, lineId, startDate, endDate, package);
                return Json(new SuccessTaskResult<IEnumerable<Defe>>(item), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
            }

            return null;

        }

        public async Task<JsonResult> GetDetailDefect(string defectCat, string startDate, string endDate, string lineId, string package)
        {
            if (string.IsNullOrWhiteSpace(defectCat))
                return Json(new FailedTaskResult<List<Defe>>("DefectCat cannot empty"), JsonRequestBehavior.AllowGet);

            try
            {
                var item = await _mpdtRepo.GetDetailDefectAsync(defectCat, startDate, endDate, lineId, package);
                return Json(new SuccessTaskResult<IEnumerable<Defe>>(item), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
            }

            return null;

        }

        public async Task<JsonResult> GetLineByFactory(string factoryId)
        {
            if (string.IsNullOrWhiteSpace(factoryId))
                return Json(new FailedTaskResult<List<LineEntity>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            try
            {
                var item = await _mpdtRepo.GetLineByFactoryAsync(factoryId);
                return Json(new SuccessTaskResult<IEnumerable<LineEntity>>(item), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
            }

            return null;

        }

        public async Task<JsonResult> GetPackageByFactoryLine(string factoryId, string lineId, string startDate, string endDate)
        {
            if (string.IsNullOrWhiteSpace(factoryId))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            try
            {
                var item = await _mpdtRepo.GetPackageByFactoryLineAsync(factoryId, lineId, startDate, endDate);
                return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(item), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var mes = ex.Message;
            }

            return null;

        }





    }
}