using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MES.Controllers
{
    public class MasterDataController : Controller
    {
        //Get category of machine or tool
        public JsonResult GetCategoryMachineTool(string isMachine)
        {
            try
            {
                var lstCategory = OPS_DAL.Business.McmtBus.GetCategorysMachineTool(isMachine);
                return Json(lstCategory, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Get master code with description and detail
        public JsonResult GetMasterCodeOracle(string masterCode, string subCode, string codeDesc, string codeDetail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(masterCode))
                {
                    Json(new List<Mcmt>(), JsonRequestBehavior.AllowGet);
                }

                var arrMasterCode = OPS_DAL.Business.McmtBus.GetMasterCode3(masterCode, subCode, codeDesc, codeDetail).ToArray();
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //// GET: MasterData
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public JsonResult GetMasterCodes(string mCode, string codeStatus)
        {
            try
            {
                var lstMtCode = OPS_DAL.Business.McmtBus.GetMasterCodeByStauts(mCode, codeStatus);

                return Json(lstMtCode);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// Get master code from mysql database
        /// </summary>
        /// <param name="mCode"></param>
        /// <param name="codeStatus"></param>
        /// <returns></returns>
        public JsonResult GetMasterCodesMySql(string mCode, string codeStatus)
        {
            try
            {
                var lstMtCode = OPS_DAL.MesBus.McmtBus.GetMasterCodeByStauts(mCode, codeStatus);

                return Json(lstMtCode);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        //Author: Son Nguyen Cao
        public JsonResult GetFactoriesMySql(string factoryId)
        {
            try
            {
                var lstFactory = OPS_DAL.MesBus.FcmtBus.GetFactories(factoryId);

                return Json(lstFactory);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Author: Son Nguyen Cao
        public JsonResult GetFactoriesByCorporation(string corporationCode)
        {
            try
            {
                /*2020-09-08 Tai Le(Thomas): Add*/
                if (String.IsNullOrEmpty(corporationCode)) {
                    if (Session["CorpCode"] != null)
                        corporationCode = Session["CorpCode"].ToString();
                }
                /*::END     2020-09-08 Tai Le(Thomas)*/

                var lstFactory = OPS_DAL.MesBus.FcmtBus.GetFactoriesByCorporation(corporationCode);

                return Json(lstFactory);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        
        public JsonResult GetCompanyListCoporation()
        {
            try
            {

                var lstDcmt = DcmtBus.GetCompanyListCoporation();

                return Json(lstDcmt);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
               
        #region MySQL
        //Author: Son Nguyen Cao
        public JsonResult GetLineByFactoryMySql(string factoryId)
        {
            try
            {
                var listLine = OPS_DAL.MesBus.LineBus.MySqlGetMesLinesByFactory(factoryId);

                return Json(listLine, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get list module by style code from MySQL
        /// </summary>
        /// <param name="styleCode"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public JsonResult GetListModulesByStyleCode(string styleCode)
        {
            if (string.IsNullOrEmpty(styleCode))
                return Json(new List<Samt>(), JsonRequestBehavior.AllowGet);

            var listModules = SamtBus.GetByStyleCode(styleCode);

            return Json(listModules, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get role from MySQL
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="systemId"></param>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public JsonResult GetRoleMySql(string roleId, string systemId, string menuId)
        {
            try
            {
                var role = SrmtBus.GetUserRoleInfoMySql(roleId, systemId, menuId);

                return Json(role, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new Mcmt(), JsonRequestBehavior.AllowGet);
                throw;
            }
        }
        #endregion
    }

}