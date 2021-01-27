using OPS_DAL.DgsBus;
using OPS_DAL.DgsEntities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MES.Controllers
{
    [SessionTimeout]
    public class DgsTargetController : Controller
    {
        // GET: DgsTarget
        public ActionResult DgsTarget()
        {
            ViewBag.PageTitle = "Production Line Dashboard";
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

        public JsonResult GetLinesByFactory(string factory)
        {
            try
            {
                if ( string.IsNullOrEmpty(factory))
                {
                    return Json(new List<LineEntity>(), JsonRequestBehavior.AllowGet);
                }

                var listLine = LineBus.GetLinesByFactoryId(factory);

                return Json(listLine, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }
        
        public JsonResult GetMesPkgTarget(string plnStartDate, string factory, string lineSerial, string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            try
            {
                //Check data whether is empty or not
                if (string.IsNullOrEmpty(plnStartDate) || string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(styleCode) 
                    || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) 
                    || string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(lineSerial))
                {
                    return Json(new Mpdt(), JsonRequestBehavior.AllowGet);
                }

                //Get mes package information
                var mesPkg = MpdtBus.GetMesPackage(plnStartDate, factory, lineSerial, styleCode, styleSize, styleColorSerial, revNo);
               
                if (mesPkg == null) return Json(new Mpdt(), JsonRequestBehavior.AllowGet);

                return Json(mesPkg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }
    }
}