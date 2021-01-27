using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using iTextSharp.text;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using PKERP.Base.Domain.Interface.Dto;

namespace MES.Controllers
{
    [SessionTimeout]
    public class QCAlertSetupController : Controller
    {
        private readonly UsmtBus _UsmtBus = new UsmtBus();
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetPkUserByFactories(List<FactoryEntity> factories)
        {
            try
            {
                var facs = await _UsmtBus.GetByFactories(factories);
                return Json(new TaskResult<List<Usmt>> { IsSuccess = true, Result = facs });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<Usmt>> { IsSuccess = false, Log = e.Message });
            }
        }

        //public JsonResult GetPkUserByFactories(string gId)
        //{
        //    try
        //    {
        //        List<FactoryEntity> factories = new List<FactoryEntity>();
        //        var facs = _UsmtBus.GetByFactories(factories);
        //        return Json(facs, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { IsSuccess = false, Log = e.Message });
        //    }
        //}
    }
}