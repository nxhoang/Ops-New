using MES.Hubs;
using MES.Repositories;
using OPS_DAL.DgsBus;
using OPS_DAL.DgsEntities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class LineDashboardController : Controller
    {
        private readonly IotDataHub _dataHub;

        private readonly IMpdtRepository _mpdtRepo;
        private readonly IIohtRepository _iohtRepo;

        public LineDashboardController(IotDataHub dataHub, IMpdtRepository mpdtRepo, IIohtRepository iohtRepo)
        {
            _dataHub = dataHub;
            _mpdtRepo = mpdtRepo;
            _iohtRepo = iohtRepo;
        }

        // GET: DgsTarget
        public ActionResult Index()
        {
            ViewBag.PageTitle = "Production Line Dashboard";
            var lstFactories = OPS_DAL.MesBus.FcmtBus.GetFactories(string.Empty);
            ViewBag.LstFactories = lstFactories;
            var cstp = CstpBus.GetFirstRecordAsync().Result;
            ViewBag.Cstp = cstp;

            return View();
        }

        public JsonResult GetAoStyles(string aoNo, string buyer, string factory, string buyerInfo, IEnumerable<Mcmt> mcmt)
        {
            try
            {
                if (string.IsNullOrEmpty(aoNo) && string.IsNullOrEmpty(buyer) && string.IsNullOrEmpty(factory))
                {
                    return Json(new List<Adsm>(), JsonRequestBehavior.AllowGet);
                }

                var lstAdms = AdsmBus.GetAoStyleList(aoNo, buyer, factory, buyerInfo);

                return Json(lstAdms, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetProductionTarget(string plnDate, string factory, string lineCd, string stylInf)
        {
            try
            {
                if (string.IsNullOrEmpty(plnDate) || string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(stylInf) || string.IsNullOrEmpty(lineCd))
                {
                    return Json(new List<Schl>(), JsonRequestBehavior.AllowGet);
                }

                //Get production target
                var schl = SchlBus.ProductionTarget(plnDate, factory, lineCd, stylInf, ConstantGeneric.FinishGoods).FirstOrDefault();

                if (schl == null) return Json(new List<Schl>(), JsonRequestBehavior.AllowGet);

                return Json(schl, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetLinesByFactoryDgs(string factory, string plnDate)
        {
            try
            {
                if (string.IsNullOrEmpty(plnDate) && string.IsNullOrEmpty(factory))
                {
                    return Json(new List<Schl>(), JsonRequestBehavior.AllowGet);
                }

                var lstSchl = SchlBus.GetLinesByFactoryDgs(factory, plnDate, ConstantGeneric.FinishGoods);

                return Json(lstSchl, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }


        /// <summary>
        /// This function get mes packages by factory and date. It query all package in specify date
        /// Using for production line dashboard
        /// </summary>
        /// <param name="factory">Factory No</param>
        /// <param name="dt">Specify date</param>
        /// <returns></returns>
        public async Task<JsonResult> GetMesPackagesByDate(string factory, DateTime dt)
        {
            if (string.IsNullOrWhiteSpace(factory))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            var lstMpdt = await _mpdtRepo.GetMesPackagesByDateAsync(factory, dt);

            return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(lstMpdt), JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetLineDashBoardData(string mxpackage)
        {
            var dto = await _mpdtRepo.GetLineDashBoardDtoAsync(mxpackage);
            if (dto == null)
                return null;

            //get current completed value
            if (string.IsNullOrWhiteSpace(dto.final_assembly_machine_mac) == false)
            {
                var lastEntryInDate = await _iohtRepo.GetLastEntryByDateAsync(dto.mxpackage, dto.actual_start_date);

                //completed
                if (lastEntryInDate != null)
                    dto.completed = (int)lastEntryInDate.DATA;

                //remain
                dto.remain = dto.target - dto.completed;

                //calc estimate
                if (dto.plan_end_date != null && dto.plan_start_date != null)
                {
                    var totalMinutes = (dto.plan_end_date - dto.plan_start_date).TotalMinutes;
                    var currentMinutes = (DateTime.UtcNow - dto.plan_start_date).TotalMinutes;

                    if (totalMinutes > 0)
                    {
                        //tam đoạn luận
                        dto.estcompleted = (decimal)Math.Round(dto.target * currentMinutes / totalMinutes, 0);
                    }
                }
            }

            return Json(dto, JsonRequestBehavior.AllowGet);
        }

        #region SON Funtions - Line Chart Dashboard
        public ActionResult LineChartDashboard()
        {
            //ViewBag.PageTitle = "Line Chart Dashboard";
            var lstFactories = OPS_DAL.MesBus.FcmtBus.GetFactories(string.Empty);
            ViewBag.LstFactories = lstFactories;

            return View("~/Views/LineDashboard/LineChartDashboard.cshtml");
        }

        public JsonResult GetListActiveMesPkg(string factoryId, string yyyyMMdd)
        {
            if (string.IsNullOrWhiteSpace(factoryId) || string.IsNullOrWhiteSpace(yyyyMMdd))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory and date cannot empty"), JsonRequestBehavior.AllowGet);
            //Get list of mes package
            var listMesPkg = MpdtBus.GetListMesPkg(factoryId, yyyyMMdd);

            //Get list line which have mes package
            var newListMesPkg = listMesPkg.Where(p => !string.IsNullOrEmpty(p.MxPackage)).ToList();

            return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(newListMesPkg), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMxPackageInfo(string mxPackage)
        {
            if (string.IsNullOrWhiteSpace(mxPackage))
                return Json(new FailedTaskResult<List<OPS_DAL.Entities.Stmt>>("MxPackage cannot be empty"), JsonRequestBehavior.AllowGet);

            //Get style key in mes package
            var stlKey = mxPackage.Split('_')[3];
            var styleCode = stlKey.Substring(0, 7);
            var styleSize = stlKey.Substring(7, 3);
            var styleColorSerial = stlKey.Substring(10, 3);
            var revNo = stlKey.Substring(13, 3);
            string aoNo = "AD-" + mxPackage.Split('_')[1];

            //Get style information by mes package
            var styleInf = OPS_DAL.Business.StmtBus.GetStyleInfoByStyleKey(styleCode, styleSize, styleColorSerial, revNo);

            //Set AO number
            styleInf.AONo = aoNo;

            return Json(new SuccessTaskResult<OPS_DAL.Entities.Stmt>(styleInf), JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetMesPackagesByLine(string factory, string line, DateTime dt)
        {
            if (string.IsNullOrWhiteSpace(factory))
                return Json(new FailedTaskResult<List<Mpdt>>("Factory cannot empty"), JsonRequestBehavior.AllowGet);

            //If line is empty then get mes package by factory
            var lstMpdt = string.IsNullOrEmpty(line) ? await _mpdtRepo.GetMesPackagesByDateAsync(factory, dt)
                : await _mpdtRepo.GetMesPackagesByLineAsync(factory, line, dt);

            return Json(new SuccessTaskResult<IEnumerable<Mpdt>>(lstMpdt), JsonRequestBehavior.AllowGet);
        }
        #endregion

        //code by Dinh Van 2021-01-21
        public ActionResult OperationPlanDashboard()
        {
            return View();
        }
    }
}