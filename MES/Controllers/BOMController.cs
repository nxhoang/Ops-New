using OPS_DAL.Business;
using OPS_DAL.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MES.Controllers
{
    public class BOMController : Controller
    {
        // GET: BOM
        public ActionResult BOM()
        {
            ViewBag.PageTitle = "BOM";

            return View();
        }

        public JsonResult GetBOM(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            //If style code, size, color, revno is null or empty then return list of empty BOM
            if(string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo))
            {
                return Json(new List<Bomt>(), JsonRequestBehavior.AllowGet);
            }

            var listPatterns = PatternBus.GetPaternsMySql(styleCode, styleSize, styleColorSerial, revNo);

            var listBOM = BomtBus.GetBOMDetailMySQL(styleCode, styleSize, styleColorSerial, revNo);

            //Check item has patterns or not
            foreach (var bom in listBOM)
            {
                var hasPt = listPatterns.Where(x => x.ItemCode == bom.ItemCode && x.ItemColorSerial == bom.ItemColorSerial).Any();
                if (hasPt)
                {
                    bom.HasPattern = "Y";
                }                
            }

            return Json(listBOM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetModule(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo))
            {
                return Json(new List<MBom>(), JsonRequestBehavior.AllowGet);
            }

            //Get list module by style code
            var listModules = SamtBus.GetByStyleCode(styleCode);

            //Get list item which linked to module
            var listMBOM = MBomBus.GetListMBOMMySQL(styleCode, styleSize, styleColorSerial, revNo);
            
            //Check item has patterns or not
            foreach (var module in listModules)
            {
                var hasItem = listMBOM.Where(x => x.ModuleItemCode == module.ModuleId).Any();
                if (hasItem)
                {
                    module.HasItem = "Y";
                }
            }

            return Json(listModules, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMBOM(string styleCode, string styleSize, string styleColorSerial, string revNo, string moduleId)
        {
            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(moduleId))
            {
                return Json(new List<MBom>(), JsonRequestBehavior.AllowGet);
            }

            //Get list module bom by style code, size, color and revision
            var listAllMBOM = MBomBus.GetListMBOMMySQL(styleCode, styleSize, styleColorSerial, revNo);
            //Get list module bom by module id
            var listMBOM = listAllMBOM.Where(x => x.ModuleItemCode == moduleId).ToList();

            //Get list item which linked to module
            var listPatterns = PatternBus.GetPaternsMySql(styleCode, styleSize, styleColorSerial, revNo);

            //Check item has patterns or not
            foreach (var mbom in listMBOM)
            {
                //Check mbom whether has patterns or not
                var hasPattern = listPatterns.Where(x => x.ItemCode == mbom.ItemCode && x.ItemColorSerial == mbom.ItemColorSerial).Any();
                if (hasPattern)
                {
                    mbom.HasPattern = "Y";
                }
            }

            return Json(listMBOM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPatterns(string styleCode, string styleSize, string styleColorSerial, string revNo, string itemCode, string itemColorSerial)
        {
            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) 
                || string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(itemCode) || string.IsNullOrEmpty(itemColorSerial))
            {
                return Json(new List<Pattern>(), JsonRequestBehavior.AllowGet);
            }

            var listPatterns = PatternBus.GetPaternsMySql(styleCode, styleSize, styleColorSerial, revNo, itemCode, itemColorSerial);

            //Get cad file name from BOMH
            var bomh = BomhBus.GetBOMHeaderMySQL(styleCode, styleSize, styleColorSerial, revNo);
            var ftpInf = FtpInfoBus.GetHostInfo();
            var ftpPath = ftpInf.FtpLink + ftpInf.FtpFolder + "/";
                       
            if (!string.IsNullOrEmpty(bomh.CADFILE)){
                //Get file name without extension
                var cadFileName = System.IO.Path.GetFileNameWithoutExtension(bomh.CADFILE);

                //Create pattern path
                var patternPath = styleCode.Substring(0, 3) + "/" + styleCode + "/" + cadFileName.Substring(0, 16) + "/" + cadFileName;

                var ftpPatternPath = ftpPath + patternPath + "/";

                foreach (var pt in listPatterns)
                {
                    pt.Url = ftpPatternPath + pt.PieceUnique + ".png";
                }
            }

            
            return Json(listPatterns, JsonRequestBehavior.AllowGet);
        }
    }
}