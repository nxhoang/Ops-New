using OPS.Models;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPSV3.Controllers
{
    public class DataAuditToolController : Controller
    {
        // GET: DataAuditTool
        public ActionResult DataAuditTool()
        {
            return View();
        }

        public JsonResult GetBomt(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {

            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo))
            {
                return Json(new List<Bomt>(), JsonRequestBehavior.AllowGet);
            }
            //get list patterns by style
            var listPatterns = PatternBus.GetPatternByStyleCode(styleCode, styleSize, styleColorSerial, revNo);
            //get list bom detail by style
            var listBOM = BomtBus.GetBOMDetail(styleCode, styleSize, styleColorSerial, revNo);

            //Check item has patterns or not
            foreach (var bom in listBOM)
            {
                //Check item has pattern or not
                var hasPt = listPatterns.Where(x => x.ItemCode == bom.ItemCode && x.ItemColorSerial == bom.ItemColorSerial && bom.MainItemCode == x.MainItemCode && bom.MainItemColorSerial == x.MainItemColorSerial).Any();
                if (hasPt)
                {
                    bom.HasPattern = "Y";
                }
            }

            return Json(listBOM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPatterns(string styleCode, string styleSize, string styleColorSerial, string revNo, string itemCode, string itemColorSerial, string mainItemCode, string mainItemColorSerial)
        {
            //get list patterns by style
            var pattern = new PatternBus();
            var listPatterns = pattern.GetPatternByBom(styleCode, styleSize, styleColorSerial, revNo, itemCode, itemColorSerial, mainItemCode, mainItemColorSerial);

            return Json(listPatterns, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetModules(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {

            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(styleCode))
            {
                return Json(new List<MBom>(), JsonRequestBehavior.AllowGet);
            }
            //get list patterns by style
            var listMbom = MBomBus.GetMBOMByStyleCode(styleCode, styleSize, styleColorSerial, revNo);
            //get list bom detail by style
            var listModule = SamtBus.GetModulesByCode(styleCode);

            //Check item has patterns or not
            foreach (var mdl in listModule)
            {
                //Check item has pattern or not
                var countItem = listMbom.Where(x => x.ModuleItemCode == mdl.ModuleId).Count();
                mdl.ItemCount = countItem;
                if (countItem > 0)
                {
                    mdl.HasItem = "Y";
                }
            }

            return Json(listModule, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMBom(string styleCode, string styleSize, string styleColorSerial, string revNo, string moduleId)
        {
            //get list mbom
            var listMbom = MBomBus.GetMBOMByStyleCode(styleCode, styleSize, styleColorSerial, revNo);
            //filter list mbom by module
            var listMbomByModule = listMbom.Where(x => x.ModuleItemCode == moduleId);

            //get all patterns MBOM
            var listPatterns = MptnBus.GetPatternsMbom(styleCode, styleSize, styleColorSerial, revNo);
            //filter patterns by module
            var listPatternsByModule = listPatterns.Where(x => x.MODULEITEMCODE == moduleId);

            //check mbom whether has pattern or not
            foreach (var mbom in listMbomByModule)
            {
                var hasPt = listMbomByModule.Where(x => x.ItemCode == mbom.ItemCode && x.ItemColorSerial == mbom.ItemColorSerial && x.MainItemCode == mbom.MainItemCode && x.MainItemColorSerial == mbom.MainItemColorSerial).Any();
                if (hasPt)
                {
                    mbom.HasPattern = "Y";
                }
            }

            return Json(listMbomByModule, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMbomPatterns(string styleCode, string styleSize, string styleColorSerial, string revNo, string moduleId, string itemCode, string itemColorSerial, string mainItemCode, string mainItemColorSerial)
        {
            //get all patterns MBOM
            var listPatterns = MptnBus.GetPatternsMbom(styleCode, styleSize, styleColorSerial, revNo);
            //filter patterns by module, item code, item color serial, main item code, main item color serial
            var listPatternsFilter = listPatterns.Where(x => x.MODULEITEMCODE == moduleId && x.ITEMCODE ==itemCode && x.ITEMCOLORSERIAL == itemColorSerial && x.MAINITEMCODE == mainItemCode && x.MAINITEMCOLORSERIAL == mainItemColorSerial);

            return Json(listPatternsFilter, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SearchStyle(GridSettings gridRequest, string buyer, string startDate, string endDate, string aoNumber, string styleInfo)
        {
            //if initial page then return null
            if(styleInfo == "----") return Json(null, JsonRequestBehavior.AllowGet);

            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                endDate = DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            }
            else if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                startDate = "1900/01/01";
            }
            var dorm = new DormBus();
            var listStyle = dorm.SearchStyles(gridRequest.pageIndex, gridRequest.pageSize, buyer, startDate, endDate, styleInfo, aoNumber, "");
            if (listStyle.Count > 0)
            {
                int totalRecords = (int)listStyle.ElementAt(0).Total;
                int pageIndex = gridRequest.pageIndex;
                int pageSize = gridRequest.pageSize;
                int modes = totalRecords % pageSize == 0 ? 0 : 1;
                var intTotalPage = totalRecords > 0 ? (totalRecords / pageSize) + modes : 1;
                var result = new
                {
                    total = intTotalPage,
                    page = pageIndex,
                    records = totalRecords,
                    rows = listStyle.ToArray()
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(listStyle, JsonRequestBehavior.AllowGet);
        }
    }
}