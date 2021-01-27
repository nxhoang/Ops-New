using DocumentFormat.OpenXml.Wordprocessing;
using MES.Repositories;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using PKERP.MES.Services.Interface.Services;
using PKQCO;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class MesManagementController : Controller
    {
        public string MenuMesId => ConstantGeneric.MesMenuId;
        public string SystemMesId => ConstantGeneric.MesSystemId;
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, SystemMesId, MenuMesId);

        private readonly IMpdtRepository _mpdtRepo;
        private readonly IConfigService _configService;

        public MesManagementController(IMpdtRepository mpdtRepo, IConfigService configService)
        {
            _mpdtRepo = mpdtRepo;
            _configService = configService;
        }

        // GET: MesManagement
        public ActionResult MesManagement()
        {
            ViewBag.PageTitle = "MES Management";
            return View();
        }

        public ActionResult FactorySummary()
        {
            ViewBag.PageTitle = "Corporate Dashboard";
            return View();
        }

        public JsonResult GetGroupPackages(string factory, string plnStartDate, string plnEndDate, string buyer, string buyerInfo, string aoNo, string filterStartDate)
        {
            try
            {
                if (string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(plnStartDate) || string.IsNullOrEmpty(plnEndDate))
                {
                    return Json(new List<Mpmt>(), JsonRequestBehavior.AllowGet);
                }

                var lstMpmt = MpmtBus.GetGroupPackages(factory, plnStartDate, plnEndDate, buyer, buyerInfo, aoNo);

                if (filterStartDate == "1")
                {
                    DateTime plnStartDate2 = DateTime.ParseExact(plnStartDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                    lstMpmt = lstMpmt.FindAll(x => DateTime.ParseExact(x.MesPlnStartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= plnStartDate2);
                }

                return Json(lstMpmt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMesPackages(string packageGroup, string seqNo)
        {
            try
            {
                if (string.IsNullOrEmpty(packageGroup))
                {
                    return Json(new List<Mpdt>(), JsonRequestBehavior.AllowGet);
                }

                var lstMpdt = MpdtBus.GetMesPackages(packageGroup, null);

                return Json(lstMpdt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetProPackages(string packageGroup)
        {
            try
            {
                if (string.IsNullOrEmpty(packageGroup))
                {
                    return Json(new List<Ppkg>(), JsonRequestBehavior.AllowGet);
                }

                var lstPpkg = PpkgBus.GetProPackages(packageGroup);

                //2019-06-18 Tai Le(Thomas) 
                foreach (Ppkg item in lstPpkg)
                {
                    QCOQueue objQCOQue = PCMQCOCalculation.GetLatestQCO(ConstantGeneric.ConnectionStr, item.AoNo, item.Factory, item.StyleCode, item.StyleSize, item.StyleColorSerial, item.RevNo, item.PPackage);

                    //Assign the Latest QCO Information
                    item.NORMALIZEDPERCENT = objQCOQue.NORMALIZEDPERCENT;
                    item.LATESTQCOTIME = objQCOQue.LATESTQCOTIME.ToString();

                    /*2019-07-22 Tai Le (Thomas) */
                    item.QCOYear = objQCOQue.QCOYEAR;
                    item.QCOWeekNo = objQCOQue.QCOWEEKNO;
                    item.LINENO = objQCOQue.LINENO;
                    item.QCORANK = objQCOQue.QCORANK; //2020-07-31 Tai Le(Thomas)

                    //START ADD) SON - 12/Aug/2019 - Get remain production package quantity
                    var ppkg = PpdpBus.GetPPRemainQuantity(item.Factory, item.PPackage);
                    if (ppkg != null)
                    {
                        item.RemainQty = ppkg.PPRemainQty;
                    }
                    //END ADD) SON - 12/Aug/2019
                }

                return Json(lstPpkg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                return Json(Msg, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult InsertReadniessCheckList(Mpcl mpcl)
        {
            try
            {
                //Check Role
                //Check Mes role
                if (Role == null || Role.IsAdd != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Set confirmer
                mpcl.Confirmer = UserInf.UserName;

                //Insert readiness check list to database
                //var blIns = MpclBus.InsertReadinessCheckList(mpcl);
                var blIns = MpclBus.InsertReadinessCheckList(mpcl);

                var strRes = blIns == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: MCL (Create Readiness check list)
                InsertActionLog(blIns, "CCL", ConstantGeneric.ActionCreate, mpcl.MxPackage, "Insert produciton readiness");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult InsertListProReadinessCheckList(string mxPackage, List<Mpcl> listMpcl)
        {
            try
            {
                //Check Mes role
                if (Role == null || Role.IsAdd != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Insert list production readiness check list
                var blIns = MpclBus.InsertListProReadiness(mxPackage, listMpcl);

                var strRes = blIns == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: MCL (Create Readiness check list)
                InsertActionLog(blIns, "CCL", ConstantGeneric.ActionCreate, mxPackage, "Insert list produciton readiness");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Insert log: MCL (Create Readiness check list)
                InsertActionLog(false, "CCL", ConstantGeneric.ActionCreate, mxPackage, "Insert list produciton readiness");

                return Json(ex, JsonRequestBehavior.AllowGet);
            }

        }

        private void InsertActionLog(bool actStatus, string functionId, string operationId, string refNo, string remark)
        {
            var isSuccess = actStatus ? "1" : "0";

            ActlBus.AddTransactionLog(UserInf.UserName, UserInf.RoleID, functionId, operationId, isSuccess, ConstantGeneric.MesPplMenuId, ConstantGeneric.MesSystemId, refNo, remark);

        }

        public JsonResult InsertGroupReadniessCheckList(Mgcl mgcl)
        {
            try
            {
                //Check Role
                //Check Mes role
                if (Role == null || Role.IsAdd != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Set confirmer
                mgcl.Confirmer = UserInf.UserName;

                //Insert readiness check list to database
                //var blIns = MpclBus.InsertReadinessCheckList(mpcl);
                var blIns = MgclBus.InsertReadinessCheckList(mgcl);

                var strRes = blIns == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteReadniessCheckList(string mxPackage)
        {
            try
            {
                //Check Mes role
                if (Role == null || Role.IsDelete != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Insert readiness check list to database
                var blDel = MpclBus.DeleteMesCheckList(mxPackage);

                var strRes = blDel == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: DCL (Delete check list)
                InsertActionLog(blDel, "DCL", ConstantGeneric.ActionDelete, mxPackage, "Delete readiness check list");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteProReadniessCheckList(string mxPackage, string checkListId)
        {
            try
            {
                //Check Mes role
                if (Role == null || Role.IsDelete != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Insert readiness check list to database
                var blDel = MpclBus.DeleteMesCheckList(mxPackage, checkListId);

                var strRes = blDel == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: DCL (Delete check list) - duplicate primary key if click on 2 checkboxes within 1 second
                //InsertActionLog(blDel, "DCL", ConstantGeneric.ActionDelete, mxPackage, "Delete readiness check list");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                InsertActionLog(false, "DCL", ConstantGeneric.ActionDelete, mxPackage, ex.Message);

                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetProductionReadiness(string mxPackage)
        {
            try
            {
                if (string.IsNullOrEmpty(mxPackage))
                {
                    return Json(new List<Mpcl>(), JsonRequestBehavior.AllowGet);
                }

                var lstMpcl = MpclBus.GetProductionReadinessList(mxPackage);

                return Json(lstMpcl, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetMaterialReadiness(string packageGroup)
        {
            try
            {
                if (string.IsNullOrEmpty(packageGroup))
                {
                    return Json(new List<Mgcl>(), JsonRequestBehavior.AllowGet);
                }
                //Get list of material readiness by package group
                var lstMgcl = await MgclBus.GetMaterialReadiness(packageGroup);

                return Json(new SuccessTaskResult<IEnumerable<Mgcl>>(lstMgcl), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new FailedTaskResult<IEnumerable<Mgcl>>(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdatePackageGroupStatus(string packageGroup, string strStatus)
        {
            try
            {
                //Check Mes role
                if (Role == null || Role.IsUpdate != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Insert readiness check list to database
                var blUpdate = MpmtBus.UpdatePackageGroupStatus(packageGroup, strStatus);

                var strRes = blUpdate == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: UPG (Update package group)
                InsertActionLog(blUpdate, "UPG", ConstantGeneric.ActionUpdate, packageGroup, "Update status of Package Group");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateMesStartPlan(string packageGroup, string seqNo)
        {
            try
            {
                //Check Mes role
                if (Role == null || Role.IsUpdate != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Update date start plan
                var blUpdate = MpdtBus.UpdateMesStartPlan(packageGroup, seqNo);

                var strRes = blUpdate == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: UMX (Update MES package)
                InsertActionLog(blUpdate, "UMX", ConstantGeneric.ActionUpdate, packageGroup + seqNo, "Update actual start plan of MES package");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        // Get calculated Mes Package from mxpackage
        public async Task<ContentResult> GetPackageDto(string mxpackage, string datasource)
        {
            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];

            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"packages/{mxpackage}/{datasource}", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request);

            return new ContentResult { Content = response.Content, ContentType = "application/json" };
        }

        // Get defection tree
        public async Task<ContentResult> GetDefectionTree(string mxpackage)
        {
            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];

            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"defects/tree?mxpackage={mxpackage}", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request);

            return new ContentResult { Content = response.Content, ContentType = "application/json" };
        }

        public async Task<JsonResult> GetMqttConfiguration()
        {
            var server = await _configService.GetStringConfigValueAsync("mqtt.tcpserver");
            var username = await _configService.GetStringConfigValueAsync("mqtt.username");
            var password = await _configService.GetStringConfigValueAsync("mqtt.password");
            var port = await _configService.GetIntConfigValueAsync("mqtt.port");
            var wsport = await _configService.GetIntConfigValueAsync("mqtt.wsport");

            return Json(new SuccessTaskResult<MqttConfig>(new MqttConfig()
            {
                Server = server,
                Port = port,
                WsPort = wsport,
                UserName = username,
                Password = password,
            }), JsonRequestBehavior.AllowGet);
        }

        // Get calculated Assembly Package from mxpackage
        public async Task<ContentResult> GetAssemblySummaryAsync(string mxpackage, string datasource)
        {
            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];

            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"packages/assemblydashboard/{mxpackage}/{datasource}", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request);

            return new ContentResult { Content = response.Content, ContentType = "application/json" };
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

        public JsonResult UpdateMesPackageStatus(string packageGroup, string seqNo, string mesStatus, string confirmedId)
        {
            try
            {
                //Check insert role of user.
                if (Role == null || Role.IsUpdate != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Update MES package status
                var resUpd = MpdtBus.UpdateMESPackageStatus(packageGroup, seqNo, mesStatus, confirmedId);
                var strRes = resUpd == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: UPG (Update package status)
                InsertActionLog(resUpd, "UPS", ConstantGeneric.ActionUpdate, packageGroup, "Update package status");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult CompleteProductionPackageGroup(string packageGroup, string pkgGroupStatus, string completedId)
        {
            try
            {
                //Check insert role of user.
                if (Role == null || Role.IsUpdate != "1")
                    return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Update MES package status
                var resUpd = MpmtBus.CompletePackageGroup(packageGroup, pkgGroupStatus, completedId);
                var strRes = resUpd == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //Insert log: UPG (Update Package Group)
                InsertActionLog(resUpd, "UPG", ConstantGeneric.ActionUpdate, packageGroup, "Update Package Group status");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult CopyStyleInfo(string stlCode, string stlSize, string stlColorSerial, string stlRevNo)
        {
            //Get style master (t_00_stmt)
            var stl = StmtBus.GetStyleInfoByCode(stlCode);

            //Get list style size
            var listStlSize = SsmtBus.GetStyleSizeMasterByCode(stlCode);

            //Get list style color
            var listStlColors = ScmtBus.GetStyleColorByStyleCode(stlCode);

            //Get style in DORM table
            var dorm = DormBus.GetDormByCode(stlCode, stlSize, stlColorSerial, stlRevNo);

            //Get BOM header
            var bomH = BomhBus.GetBOMHeader(stlCode, stlSize, stlColorSerial, stlRevNo);

            //Get BOM detail
            var listBomt = BomtBus.GetBOMDetail(stlCode, stlSize, stlColorSerial, stlRevNo);

            //Get Pattern master
            var listPtmt = PatternBus.GetPatternByStyleCode(stlCode, stlSize, stlColorSerial, stlRevNo);

            //Get list moudle BOM
            var listMbom = MBomBus.GetMBOMByStyleCode(stlCode, stlSize, stlColorSerial, stlRevNo);

            return Json("Copied", JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetMesOperationPlan(string styleCode, string styleSize, string styleColorSerial, string revNo, string mxPackage)
        {
            var opmt = new Opmt { StyleCode = styleCode, StyleSize = styleSize, StyleColorSerial = styleColorSerial, RevNo = revNo, MxPackage = mxPackage };
            var listOpmt = await OpmtBus.GetByMxPackageAsyncMySql(opmt);

            return Json(new SuccessTaskResult<Opmt>(listOpmt), JsonRequestBehavior.AllowGet);
        }

        //Get list  BOM Patterns
        public async Task<JsonResult> GetMesBOMPatternMySql(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string eidtion)
        {
            try
            {
                var prot = new Prot { StyleCode = styleCode, StyleSize = styleSize, StyleColorSerial = styleColorSerial, RevNo = revNo, OpRevNo = opRevNo, Edition = eidtion };
                IEnumerable<Prot> listProt = await ProtBus.GetProtsAsyncMySql(prot);

                return Json(new SuccessTaskResult<IEnumerable<Prot>>(listProt), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new FailedTaskResult<List<Prot>>(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }

        //Author: Son Nguyen Cao
        public JsonResult GetLineDetailByMESPkg(string mesPackage)
        {
            //If mes package is null or empty then return the empty list
            if (string.IsNullOrEmpty(mesPackage))
                return Json(new List<Lndt>(), JsonRequestBehavior.AllowGet);

            //Get list line detail by mes package
            var listLndt = LndtBus.GetLinesDetailByMesPkgMySql(mesPackage);

            return Json(listLndt, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddLineDetail(Lndt lineDt)
        {
            try
            {
                //Insert line detail to database
                var resIns = LndtBus.InsertLineDetailMySql(lineDt);

                //Record action
                //Insert log: Creating Line Detail
                InsertActionLog(resIns, "CLDT", ConstantGeneric.ActionCreate, lineDt.MxPackage + " - " + lineDt.LineSerial, "Adding line detail");

                if (resIns)
                    return Json(new SuccessTaskResult<string>("Added line detail"), JsonRequestBehavior.AllowGet);

                return Json(new FailedTaskResult<string>("Adding line detail fail"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Insert log: Creating Line Detail
                InsertActionLog(false, "CLDT", ConstantGeneric.ActionCreate, lineDt.MxPackage + " - " + lineDt.LineSerial, "Adding line detail fail");

                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateMaunualPkgQuantity(string packageGroup, int seqNo, string mxPackage, int mxQty)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(packageGroup) || string.IsNullOrWhiteSpace(mxPackage))
                {
                    return Json(new FailedTaskResult<string>("Package group or MxPackage is empty"), JsonRequestBehavior.AllowGet);
                }

                if (seqNo == 0)
                {
                    return Json(new FailedTaskResult<string>("SeqNo is 0"), JsonRequestBehavior.AllowGet);
                }

                var resUpd = MpdtBus.UpdateManualMxPkgQuantity(packageGroup, seqNo, mxPackage, mxQty);

                //Insert log: Updating Manual MxPackage quantity
                InsertActionLog(resUpd, "UMPQ", ConstantGeneric.ActionCreate, packageGroup + " - " + seqNo + " - " + mxPackage, "Updating manual mxPackage quantity");


                if (resUpd)
                    return Json(new SuccessTaskResult<string>("Updated quantity"), JsonRequestBehavior.AllowGet);

                return Json(new FailedTaskResult<string>("Updating quantity fail"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                //Insert log: Updating Manual MxPackage quantity
                InsertActionLog(false, "UMPQ", ConstantGeneric.ActionCreate, packageGroup + " - " + seqNo + " - " + mxPackage, "Updating manual mxPackage quantity");

                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteLineDetail(string mxPackage, string lineSerial)
        {
            try
            {
                //Insert line detail to database
                var resIns = LndtBus.DeleteLineDetailMySql(mxPackage, lineSerial);

                //Record action
                //Insert log: Deleting Line Detail 
                InsertActionLog(resIns, "DLDT", ConstantGeneric.ActionDelete, mxPackage + " - " + lineSerial, "Deleting line detail");

                if (resIns)
                    return Json(new SuccessTaskResult<string>("Deleted line detail"), JsonRequestBehavior.AllowGet);

                return Json(new FailedTaskResult<string>("Deleting line detail fail"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Insert log: Deleting Line Detail
                InsertActionLog(false, "DLDT", ConstantGeneric.ActionCreate, mxPackage + " - " + lineSerial, "Deleting line detail fail");

                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }

        //public JsonResult CheckMaterialReadinessCheckList(Mpcl mpcl)
        //{
        //    /* Tai Le(Thomas): 2019-06-17: Handle Check New Material Readiness */
        //    try
        //    {
        //        //Check Role
        //        //Check Mes role
        //        if (Role == null || Role.IsAdd != "1")
        //            return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

        //        //Set confirmer
        //        //mpcl.Confirmer = UserInf.UserName;

        //        //Insert readiness check list to database
        //        //var blIns = MpclBus.InsertReadinessCheckList(mpcl);
        //        //var blIns = MpclBus.InsertReadinessCheckList(mpcl); 
        //        //var strRes = blIns == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

        //        /* DataString Format: 
        //         *  Factory + ";" + AoNo + ";" + StyleCode + ";" + StyleSize + ";" + StyleColorSerial + ";" + RevNo + ";" + PPackage; 
        //         */
        //        string[] DataString = mpcl.MxPackage.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        //        string QCOResult_1 = "", QCOResult_2;

        //        PCMLib.PCMQCOCalculation pcmQCOCalculation = new PCMLib.PCMQCOCalculation();
        //        pcmQCOCalculation.mEnviroment = "";

        //        //Fire the QCO Calculation for Single Selected Package 
        //        QCOResult_1 = pcmQCOCalculation.QCOCalculation(OPS_Utils.ConstantGeneric.ConnectionStr, DataString[0], UserInf.UserName, Role.OwnerId,String.Empty, true, DataString[1], DataString[2], DataString[3], DataString[4], DataString[5], DataString[6], out QCOResult_2);

        //        var strRes = QCOResult_1 == String.Empty ? ConstantGeneric.Success : ConstantGeneric.Fail;

        //        return Json(strRes, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex, JsonRequestBehavior.AllowGet);
        //    }
        //}

        #region MES production log        
        public JsonResult SaveMesProductionLog(Prlg proLog)
        {
            try
            {
                //Check existed production log. If it existed then update, otherwise insert it to database.
                var existProLog = PrlgBus.GetMesProductionLog(proLog.MXPACKAGE);
                if(existProLog != null)
                {
                    //Update production log
                    var resUdp = PrlgBus.UpdateMesProductionLog(proLog);
                    
                    //Record action
                    InsertActionLog(resUdp, "SaveMesProductionLog", ConstantGeneric.ActionUpdate, proLog.MXPACKAGE, "Update MES production log");

                    return Json(new SuccessTaskResult<string>("Saved"), JsonRequestBehavior.AllowGet);
                }

                //Insert mes production log
                var resIns = PrlgBus.InsertMesProductionLog(proLog);

                //Record action
                InsertActionLog(resIns, "SaveMesProductionLog", ConstantGeneric.ActionCreate, proLog.MXPACKAGE, "Save MES production log");

                if (resIns) return Json(new SuccessTaskResult<string>("Saved"), JsonRequestBehavior.AllowGet);

                return Json(new FailedTaskResult<string>("Failure during saving"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Insert log: Deleting Line Detail
                InsertActionLog(false, "InsertMesProductionLog", ConstantGeneric.ActionCreate, proLog.MXPACKAGE, ex.Message);

                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetMesProductionLog(string mxPackage)
        {
            try
            {
                //Get mes production log
                var proLog = PrlgBus.GetMesProductionLog(mxPackage);

                return Json(new SuccessTaskResult<Prlg>(proLog), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //Insert log: Deleting Line Detail
                InsertActionLog(false, "GetMesProductionLog", ConstantGeneric.ActionRead, mxPackage, ex.Message);

                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetProducitonLogFromOps(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            try
            {
                //Get MES operation plan target quantity from
                var opmt = OpmtBus.GetOpTargetQuantity(styleCode, styleSize, styleColorSerial, revNo, ConstantGeneric.TableMxOpmt, ConstantGeneric.TableMxOpdt);

                if(opmt == null)
                {
                    //Get AOMTOP opeartion plan target quantity
                    opmt = OpmtBus.GetOpTargetQuantity(styleCode, styleSize, styleColorSerial, revNo, ConstantGeneric.TableMtOpmt, ConstantGeneric.TableMtOpdt);
                
                    if(opmt == null)
                    {
                        //Get PDM opeartion plan target quantity
                        opmt = OpmtBus.GetOpTargetQuantity(styleCode, styleSize, styleColorSerial, revNo, ConstantGeneric.TableSdOpmt, ConstantGeneric.TableSdOpdt);

                    }
                }

                return Json(new SuccessTaskResult<Opmt>(opmt), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }

        }
        #endregion
    }
}