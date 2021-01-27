using OPS.GenericClass;
using OPS.Models;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OPS.Controllers
{
    public class TaskResult<T>
    {
        public bool IsSuccess { get; set; }
        public int Code { get; set; }
        public string Log { get; set; }
        public T Result { get; set; }
        public T Data => Result;
    }

    [SessionAuthorize]
    public class OpsLayoutController : Controller
    {
        #region Properties
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt RnDRole => SrmtBus.GetUserRoleInfo(UserInf.RoleID, ConstantGeneric.OpsSystemId, MenuOpsId);
        public Srmt FactoryRole => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, FactoryEdition);
        public Srmt MesRole => SrmtBus.GetUserRoleInfo(UserInf.RoleID, ConstantGeneric.MesSystemId, ConstantGeneric.MesMenu);
        public string FactoryEdition => ConstantGeneric.FactoryMenu;
        public string MenuOpsId => ConstantGeneric.OpManagementMenuId;
        public string SystemOpsId => ConstantGeneric.OpsSystemId;
        private readonly OpdtBus _OpdtBus = new OpdtBus();
        private readonly FtpInfoBus _FtpInfoBus = new FtpInfoBus();
        private readonly OpsVideoHub _OpsVideoHub = new OpsVideoHub();
        //private IHubContext<OpsVideoHub> _rulesHubContext;
        #endregion

        #region General functions

        //public OpsLayoutController(IHubContext<OpsVideoHub> rulesHubContext)
        //{
        //    _rulesHubContext = rulesHubContext;
        //}


        [HttpPost]
        public JsonResult GetModulesByCode(string styleCode)
        {
            try
            {
                var modules = SamtBus.GetModulesByCode(styleCode);

                return Json(new { modules }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult OpsLayout()
        {
            //OpsVideoHub.SendMessage("initializing and preparing", 2);

            ViewBag.PageTitle = "Ops Layout";
            return View();
        }

        public JsonResult GetUserRole()
        {
            try
            {
                return Json(new { rdRole = RnDRole, facRole = FactoryRole, mesRole = MesRole }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetOpName(string languageId)
        {
            try
            {
                var opNames = OperationNameBus.GetOpName(languageId);
                return Json(new { opNames }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMasterCodes(string mCode)
        {
            try
            {
                var arrMasterCode = McmtBus.GetMasterCode(mCode);
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMaxOpSerial(Opdt opdt)
        {
            try
            {
                var lastOpSerial = OpdtBus.GetMaxOpSerial(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo);
                return Json(lastOpSerial, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetMachineCategories(string masterCode, string subCode, string codeDesc)
        {
            try
            {
                var arrMasterCode = McmtBus.GetMasterCode2(masterCode, subCode, codeDesc);
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetOpMachineMasters(bool isTool, List<string> categoryIds)
        {
            try
            {
                var arrTool = isTool ? OtmtBus.GetOpTool(categoryIds) : OtmtBus.GetOpMachine(categoryIds);

                return Json(arrTool, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetFactoryByTypeAndStatus()
        {
            try
            {
                var factories = FactoryBus.GetByTypeAndStatus("P", "OK"); // P is for Pungkook's factories

                return Json(new { result = factories }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get color by theme
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public JsonResult GetColorByTheme(string theme)
        {
            try
            {
                var lstColor = OpColorBus.GetColorByTheme(theme);
                return Json(lstColor, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetColour()
        {
            try
            {
                var colour = OpColorBus.GetColour();
                return Json(colour, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region CRUD Process 
        [HttpPost]
        public JsonResult GetOpdts(Opmt opsMaster, string groupMode, string languageId, int page)
        {
            try
            {
                object opdts;
                var opmt = OpmtBus.GetOpsMasterByCode(opsMaster).FirstOrDefault();

                if (opmt == null) return Json(new
                {
                    opmt = (Opmt)null,
                    opdts = (object)new
                    {
                        groups = new ArrayList(),
                        nodes = new ArrayList(),
                        edges = new ArrayList(),
                        groupsToAdd = new ArrayList()
                    }
                }, JsonRequestBehavior.AllowGet);

                switch (groupMode)
                {
                    case "MachineType":
                        opdts = OpdtBus.GetOpdtByMachineType(opsMaster, languageId, page);
                        break;
                    case "ModuleType":
                        opdts = OpdtBus.GetOpdtByModuleType(opsMaster, languageId, page);
                        break;
                    default:
                        opdts = OpdtBus.GetOpdtByOpGroup(opsMaster, languageId, page);
                        break;
                }

                var jsonResult = Json(new { opmt, opdts }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult AddNewProcess(Opdt opdt, List<Optl> machines, List<Optl> tools)
        {
            try
            {
                var process = OpdtBus.AddNewProcess(opdt);
                return Json(new { process }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventAdd);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteProcess(Opdt opdt)
        {
            try
            {
                //START ADD - SON
                //Check user role before delete process.
                var edition = opdt.Edition?.Substring(0, 1);
                var menuId = CommonUtility.GetMenuIdByEdition(edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? ConstantGeneric.MesSystemId : SystemOpsId;
                var userRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);

                if (!CommonMethod.CheckRole(userRole.IsDelete))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                var result = OpdtBus.DeleteOpdtAndToolLinking(opdt);

                //Count processes, machines, wokers and calculate optime. 
                var objOpmt = CommonUtility.CountOperationPlan(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo);

                // Record log delete process.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opdt, UserInf, SystemOpsId, ConstantGeneric.LayoutMenuId, ConstantGeneric.EventDelete, "Delete process.", status);

                //Update operation master.
                OpmtBus.UpdateOpMaster(objOpmt);
                //END ADD - SON

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventDelete);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CloneSingleProcess(Opdt opdt)
        {
            try
            {
                var maxOpSerial = OpdtBus.GetMaxOpSerial(opdt.Edition, opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                    opdt.RevNo, opdt.OpRevNo);
                var copiedOpnts = OpntBus.GetOpNameDetails(opdt.Edition, opdt.StyleCode, opdt.StyleSize,
                    opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo, opdt.OpSerial.ToString(), "", "");
                var p = OpdtBus.GetOpdtByPks(opdt);
                var opNum = string.IsNullOrEmpty(p.OpNum) ? ".1" : $"{p.OpNum}.1";
                var copiedOpdt = new Opdt(opdt.Edition, p.StyleCode, p.StyleSize, p.StyleColorSerial, p.RevNo, p.OpRevNo,
                    maxOpSerial, opNum, opdt.OpGroup, p.OpName, p.Factory, opdt.MachineType, p.ToolId, p.OpTime, p.MaxTime,
                    p.BenchmarkTime, p.MachineCount, p.Remarks, p.JobType, p.ManCount, opdt.ModuleId, p.OpPrice,
                    p.OfferOpPrice, p.OutSourced, p.HotSpot, opdt.X, opdt.Y, opdt.Page, opdt.DisplayColor);

                //START MOD - SON - Add edition when get list of tool linking
                //var optls = OptlBus.GetListToolLinking(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo,
                //            opdt.OpRevNo).Where(x => x.OpSerial == opdt.OpSerial).ToList();
                var optls = OptlBus.GetListToolLinking(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo,
                            opdt.OpRevNo, opdt.Edition).Where(x => x.OpSerial == opdt.OpSerial).ToList();
                //END MOD - SON

                var cloneResult = OpdtBus.CloneSingleProcess(copiedOpdt, copiedOpnts, optls);

                // Record log copy new operation.
                var status = CommonUtility.ConvertBoolToString01(cloneResult);
                CommonUtility.InsertLogActivity(opdt, UserInf, SystemOpsId, ConstantGeneric.LayoutMenuId,
                    ConstantGeneric.EventAdd, "Clone process", status);

                if (!cloneResult) return Json(new { result = false }, JsonRequestBehavior.AllowGet);

                //Count processes, machines, wokers and calculate optime. 
                var opmt = CommonUtility.CountOperationPlan(opdt.Edition, opdt.StyleCode, opdt.StyleSize,
                    opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo);

                //Update operation master.
                OpmtBus.UpdateOpMaster(opmt);

                return Json(new { result = copiedOpdt }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventAdd);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Saving options
        [HttpPost]
        public JsonResult SaveData(List<Opdt> opdts, Opmt opmt)
        {
            try
            {
                if (opdts == null) return null;

                var result = OpdtBus.UpdateOpdts(opdts, opmt);

                // Record logs.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opmt, UserInf, SystemOpsId, ConstantGeneric.LayoutMenuId, ConstantGeneric.EventEdit, "Save data.", status);

                // Update MachineCount, ManCount, OpCount, OpTime = tackTime;
                Task unused = CommonUtility.UpdateOpmt(opmt);

                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventEdit);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Save as new revision: 1 create new Opmt with same old Opmt(CONFIRMCHK = null and OpRevNo automatically increase) 
        /// --------------------  2 Save all of opdts with OpRev is from new Opmt
        /// --------------------  3 Save all toolinking that an opdt has it
        /// </summary>
        /// <param name="opmt">Operation master</param>
        /// <param name="opdts">List of operation details</param>
        /// <returns></returns>
        ///  Author: VitHV
        public ActionResult CopyToNewOps(Opmt opmt, List<Opdt> opdts)
        {
            try
            {
                // Update field opmt
                opmt.RegisterId = UserInf.UserName;
                var result = OpmtBus.CopyOpsMaster(opmt, opdts);

                // Record log copy new operation.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opmt, UserInf, SystemOpsId, ConstantGeneric.LayoutMenuId, ConstantGeneric.EventAdd, "Copy to new process.", status);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventAdd);
                return Json(new { error = "An error occurred, please contact admin." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ConfirmOpmt(Opmt opmt)
        {
            try
            {
                //START ADD) SON - 21/Jun/2019 check standard process
                var listNStd = OpdtBus.GetListProcesWithNoneStandardName(opmt);
                if (listNStd.Count > 0) return Json(new { error = "Opeartion Plan is using none standard name." }, JsonRequestBehavior.AllowGet);
                //END check standard process

                opmt.ConfirmedId = UserInf.UserName; //SON ADD
                var result = OpmtBus.ConfirmOpsMasterAndDetail(opmt);

                // Record log copy new operation.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opmt, UserInf, SystemOpsId, ConstantGeneric.LayoutMenuId, ConstantGeneric.EventConfirm, "Confirm operation master.", status);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventConfirm);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Save options

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstOpdt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public JsonResult UpdateNextOp(List<Opdt> lstOpdt)
        {
            try
            {
                //If there is no list of operation plan then do not update next op
                if (lstOpdt == null) return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);

                if (OpdtBus.UpdateListOpNextOp(lstOpdt))
                    return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);

                return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventEdit);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Clone Process
        //Author: Son Nguyen Cao.
        public JsonResult CloneProcess(GridSettings gridRequest, List<Opdt> lstOpDetail)
        {
            try
            {
                var lstOpDetailCopy = new List<Opdt>();
                var lstToolCopy = new List<Optl>();
                var lstOpntCopy = new List<Opnt>();

                var styleCode = lstOpDetail[0].StyleCode;
                var styleSize = lstOpDetail[0].StyleSize;
                var styleColorSerial = lstOpDetail[0].StyleColorSerial;
                var revNo = lstOpDetail[0].RevNo;
                var opRevNo = lstOpDetail[0].OpRevNo;
                var edition = lstOpDetail[0].Edition;

                var role = edition == ConstantGeneric.EditionAom ? FactoryRole : RnDRole;

                //Check ops confirmed
                var checkConfrimed = CommonUtility.CheckRoleEditionConfirmed(role.IsAdd, edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);
                if (checkConfrimed != ConstantGeneric.False)
                    return Json(CommonUtility.GetResultContent(ConstantGeneric.Fail, checkConfrimed), JsonRequestBehavior.AllowGet);

                var maxOpSerial = OpdtBus.GetMaxOpSerial(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);
                //Copy list of process and tool linking.
                foreach (var opdt in lstOpDetail)
                {
                    //Get list opeartion name detail.
                    var lstOpnt = OpntBus.GetOpNameDetails(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opdt.OpSerial.ToString(), "", "");
                    foreach (var opnt in lstOpnt)
                    {
                        opnt.OpSerial = maxOpSerial;
                    }
                    lstOpntCopy.AddRange(lstOpnt);

                    //Get list of process to copy
                    var copyOpdt = OpdtBus.GetOpDetailByCode(opdt).FirstOrDefault();
                    if (copyOpdt != null)
                    {
                        var copyNewOpdt = new Opdt
                        {
                            StyleCode = copyOpdt.StyleCode,
                            StyleSize = copyOpdt.StyleSize,
                            StyleColorSerial = copyOpdt.StyleColorSerial,
                            RevNo = copyOpdt.RevNo,
                            OpRevNo = copyOpdt.OpRevNo,
                            OpSerial = maxOpSerial,
                            OpName = copyOpdt.OpName,
                            OpTime = copyOpdt.OpTime,
                            MachineType = copyOpdt.MachineType,
                            JobType = copyOpdt.JobType,
                            ManCount = copyOpdt.ManCount,
                            ModuleId = copyOpdt.ModuleId,
                            Edition = copyOpdt.Edition
                        };

                        lstOpDetailCopy.Add(copyNewOpdt);
                        maxOpSerial += 1;
                    }
                    else
                    {
                        return Json(CommonUtility.GetResultContent(ConstantGeneric.Fail, "Cannot find process to copy."));
                    }
                }

                //Clone process.
                var cloneSta = OpdtBus.CloneProcess(lstOpDetailCopy, lstOpntCopy);

                // Record log copy new operation.
                var status = CommonUtility.ConvertBoolToString01(cloneSta);
                CommonUtility.InsertLogActivity(lstOpDetailCopy[0], UserInf, SystemOpsId, ConstantGeneric.LayoutMenuId, ConstantGeneric.EventAdd, "Clone process.", status);

                if (!cloneSta)
                    return Json(CommonUtility.GetResultContent(ConstantGeneric.Fail, "Cannot add clone process."), JsonRequestBehavior.AllowGet);

                //Count processes, machines, wokers and calculate optime. 
                var objOpmt = CommonUtility.CountOperationPlan(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);
                //Update operation master.
                OpmtBus.UpdateOpMaster(objOpmt);

                var objResult = CommonUtility.GetResultContent(ConstantGeneric.Success, lstOpDetailCopy);

                return Json(objResult, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return Json(CommonUtility.GetResultContent(ConstantGeneric.Fail, exc.Message), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public async Task<JsonResult> UpdatePage(List<Opdt> opdts)
        {
            try
            {
                if (opdts == null || opdts.Count == 0)
                {
                    return Json(new { error = "No process to update." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _OpdtBus.UpdatePage(opdts);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventDelete);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> UpdateModule(List<Opdt> opdts)
        {
            try
            {
                if (opdts == null || opdts.Count == 0)
                {
                    return Json(new { error = "No process to update." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _OpdtBus.UpdateModule(opdts);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventDelete);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Image and video files handler
        [HttpPost]
        public string UploadImageProcess()
        {
            var sysFileName = ConstantGeneric.Fail;
            try
            {
                //Check role
                if (RnDRole.IsAdd != ConstantGeneric.RoleTrue) return ConstantGeneric.Fail;

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    var data = Request.Form;
                    var styleCode = data["StyleCode"];
                    var styleSize = data["StyleSize"];
                    var styleColor = data["StyleColor"];
                    var styleRevNo = data["StyleRevNo"];
                    var opRevNo = data["OpRevNo"];
                    var opSerial = data["OpSerial"];

                    if (fileContent == null || fileContent.ContentLength <= 0) continue;

                    var fileName = fileContent.FileName;
                    var extFile = Path.GetExtension(fileName)?.ToLower();
                    var subFolder = CommonUtility.CreateSubFolder(styleCode, styleSize, styleColor, styleRevNo, opRevNo,
                       opSerial);
                    var imgFolder = $"{Server.MapPath(ConstantGeneric.OperationFilePath)}/{subFolder}";

                    //Create directory for storing image.
                    if (!CommonMethod.CreateFolder(imgFolder)) return ConstantGeneric.Fail;

                    //Create system image name
                    sysFileName = CommonUtility.CreateSystemFileName(styleCode, styleSize, styleColor, styleRevNo, opRevNo, opSerial, extFile);

                    if (!string.IsNullOrEmpty(sysFileName))
                    {
                        var pathSysFile = Path.Combine(imgFolder, sysFileName);
                        if (System.IO.File.Exists(pathSysFile))
                        {
                            System.IO.File.Delete(pathSysFile);
                        }
                        fileContent.SaveAs(pathSysFile);
                    }
                    else
                    {
                        return ConstantGeneric.Fail;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventAdd);

                return ConstantGeneric.Fail;
            }

            return sysFileName;
        }

        [HttpPost]
        public ActionResult UploadVideoProcess()
        {
            //Check role
            if (RnDRole.IsAdd != ConstantGeneric.RoleTrue) return Json(ConstantGeneric.Fail);

            string sysVideoName = ConstantGeneric.Fail;
            foreach (string file in Request.Files)
            {
                var fileDataContent = Request.Files[file];

                if (fileDataContent == null || fileDataContent.ContentLength <= 0) continue;

                // take the input stream, and save it to a temp folder using the original file.part name posted
                var stream = fileDataContent.InputStream;
                var data = Request.Form;
                var fileName = data["FileName"];
                var styleCode = data["StyleCode"];
                var styleSize = data["StyleSize"];
                var styleColor = data["StyleColor"];
                var styleRevNo = data["StyleRevNo"];
                var opRevNo = data["OpRevNo"];
                var opSerial = data["OpSerial"];

                //Create folder and system video filename
                var subFolder = CommonUtility.CreateSubFolder(styleCode, styleSize, styleColor, styleRevNo, opRevNo,
                      opSerial);
                var videoFolder = $"{Server.MapPath(ConstantGeneric.OpsVideoProcessPath)}{subFolder}";
                var sysFileName = $"{styleCode}{styleSize}{styleColor}{styleRevNo}{opRevNo}{opSerial}";

                //Create directory for storing video.
                if (!CommonMethod.CreateFolder(videoFolder)) return Json(ConstantGeneric.Fail);

                string path = Path.Combine(videoFolder, fileName);
                try
                {
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    using (var fileStream = System.IO.File.Create(path))
                    {
                        stream.CopyTo(fileStream);
                    }

                    // Once the file part is saved, see if we have enough to merge it
                    // if last it will do
                    WebUtils ut = new WebUtils();
                    sysVideoName = ut.MergeFile(path, sysFileName);
                }
                catch (Exception exc)
                {
                    CommonUtility.CreateMessageException(exc.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventAdd);
                    return Json(ConstantGeneric.Fail);
                }
            }

            return Json(Path.GetFileName(sysVideoName));
        }

        [HttpPost]
        public async Task<JsonResult> UploadVideoChunkFile()
        {
            if (string.IsNullOrEmpty(ConstantGeneric.PkFileFolder))
            {
                return Json(new TaskResult<string> { IsSuccess = false, Log = "Not config temp folder to upload file in web.config." });
            }

            var data = Request.Form;
            var filePartName = data["FilePartName"];
            var guid = data["guid"];
            foreach (string file in Request.Files)
            {
                var fileDataContent = Request.Files[file];
                if (fileDataContent == null || fileDataContent.ContentLength <= 0) continue;

                // take the input stream, and save it to a temp folder using the original file.part name posted
                var stream = fileDataContent.InputStream;

                var pkFileFolder = Server.MapPath(ConstantGeneric.PkFileFolder);
                var tempUploadFolder = Path.Combine(pkFileFolder, "temp", guid);
                var videoExtension = Path.GetExtension(file);
                if (videoExtension == null) continue;
                var sysFilePath = Path.Combine(pkFileFolder, "upload", $"{guid}{videoExtension}"); 
                var videoFolder = Path.Combine(pkFileFolder, "upload");

                //Create folder if it is not exist.
                if (!Directory.Exists(tempUploadFolder)) Directory.CreateDirectory(tempUploadFolder);
                if (!Directory.Exists(videoFolder)) Directory.CreateDirectory(videoFolder);

                var pathPartFile = Path.Combine(tempUploadFolder, filePartName);

                try
                {
                    if (System.IO.File.Exists(pathPartFile)) System.IO.File.Delete(pathPartFile);
                    using (var fileStream = System.IO.File.Create(pathPartFile))
                    {
                        stream.CopyTo(fileStream);
                    }

                    // Once the file part is saved, see if we have enough to merge it
                    var pkFileProcessing = new PkFileProcessing();
                    var videoPath = await pkFileProcessing.OpsMergeFile(pathPartFile, sysFilePath);

                    return Json(string.IsNullOrEmpty(videoPath) ? new TaskResult<string> { IsSuccess = true, Log = $"Uploaded file: {filePartName}" } : new TaskResult<string> { IsSuccess = true, Result = videoPath });
                }
                catch (Exception ex)
                {
                    ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId,
                        ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);

                    return Json(new TaskResult<string> { IsSuccess = false, Log = ex.Message });
                }
            }

            return Json(new TaskResult<string> { IsSuccess = false, Log = "No file" });
        }

        [HttpPost]
        public async Task<JsonResult> SplitOpdtVideos(List<VideoOpdt> opdtVideos, string inputFilePath, string connectionId)
        {
            try
            {
                var resultList = new List<TaskResult<VideoOpdt>>();
                var ffmpegFilePath = Server.MapPath("~/assets/ffmpeg/bin/ffmpeg.exe");
                var combineStyle = $"{opdtVideos[0].StyleCode}{opdtVideos[0].StyleSize}{opdtVideos[0].StyleColorSerial}{opdtVideos[0].RevNo}{opdtVideos[0].OpRevNo}";
                var styleFolder = Path.Combine("ops", opdtVideos[0].Edition, $"{combineStyle}");
                var pkFileFolder = Server.MapPath(ConstantGeneric.PkFileFolder);
                var videoExtension = Path.GetExtension(inputFilePath);
                var outputFolder = Path.Combine(pkFileFolder, styleFolder);

                // Getting FPT information.
                var ftpInfo = new FtpInfo
                {
                    FtpRoot = ConstantGeneric.FtpOpVideoDirectory,
                    FtpUser = ConstantGeneric.FtpOpVideoUser,
                    FtpPass = ConstantGeneric.FtpOpVideoPassword
                };
                //Create folder on FTP server.
                FtpInfoBus.CreateFTPDirectory(ftpInfo, "");
                var videoNo = 0;

                foreach (var opdtVideo in opdtVideos)
                {
                    var outputFileName = $"{combineStyle}{opdtVideo.OpSerial}{videoExtension}";

                    //If output folder is not exist, creating it
                    if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

                    var outputFilePath = Path.Combine(outputFolder, outputFileName);
                    if (System.IO.File.Exists(outputFilePath)) System.IO.File.Delete(outputFilePath);

                    var strCmdText = $@"-i ""{inputFilePath}"" -ss {opdtVideo.StartTime} -to {opdtVideo.EndTime} -async 1 ""{outputFilePath}""";
                    var process = System.Diagnostics.Process.Start(ffmpegFilePath, strCmdText);
                    process?.WaitForExit();

                    // Copying file to FTP Server
                    //await CopyFileToFtpServer(outputFilePath, outputFileName);
                    var ftpFilePath = $"{ftpInfo.FtpRoot}{outputFileName}";
                    await _FtpInfoBus.UploadFileToFtpServer(outputFilePath, ftpFilePath, ftpInfo);

                    if (System.IO.File.Exists(outputFilePath)) System.IO.File.Delete(outputFilePath);

                    // Updating video file of process
                    //var videoFile = Path.Combine(ConstantGeneric.PkFileFolder, styleFolder, outputFileName);
                    opdtVideo.VideoFile = outputFileName;
                    opdtVideo.VideoFullPath = $"{ConstantGeneric.VideoProcessHttpLink}{outputFileName}";

                    var updateVideoFile = await _OpdtBus.UpdateVideoFile(opdtVideo);

                    videoNo++;

                    OpsVideoHub.SendMessage(opdtVideo.DisplayName, videoNo, connectionId);

                    resultList.Add(new TaskResult<VideoOpdt> { IsSuccess = updateVideoFile, Result = opdtVideo, Log = "Updated video file" });
                }

                return Json(resultList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new TaskResult<List<VideoOpdt>> { IsSuccess = false, Log = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteProcessFile(string fileName, string fileType, Opdt opdt)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    //Create sub-folder
                    string pathSysFile = string.Empty;
                    var subFolder = CommonUtility.CreateSubFolder(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                        opdt.RevNo, opdt.OpRevNo, opdt.OpSerial.ToString());
                    if (string.IsNullOrEmpty(subFolder)) return Json("Can't find folder to delete.");

                    switch (fileType)
                    {
                        case ConstantGeneric.ImageType:
                            pathSysFile = $"{Server.MapPath(ConstantGeneric.OperationFilePath)}{subFolder}/{fileName}";
                            break;
                        case ConstantGeneric.VideoType:
                            pathSysFile = $"{Server.MapPath(ConstantGeneric.OpsVideoProcessPath)}{subFolder}/{fileName}";
                            break;
                        case ConstantGeneric.MachineType:
                            pathSysFile = Server.MapPath(ConstantGeneric.OperationFilePath) + subFolder + fileName;
                            break;
                    }

                    if (!System.IO.File.Exists(pathSysFile)) return Json(ConstantGeneric.Fail);
                    System.IO.File.Delete(pathSysFile);

                    return Json(ConstantGeneric.Success);
                }
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventDelete);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }

            return Json(ConstantGeneric.Fail);
        }

        [HttpPost]
        public ActionResult MergeOpsFile(string fileName, Opdt opdt)
        {
            WebUtils ut = new WebUtils();
            var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);
            var sysFileName = $"{opdt.StyleCode}{opdt.StyleSize}{opdt.StyleColorSerial}{opdt.RevNo}{opdt.OpRevNo}{opdt.OpSerial}";
            string path = Path.Combine(tempFolder, fileName);
            var sysVideoName = ut.MergeFile(path, sysFileName);

            return Json(Path.GetFileName(sysVideoName));
        }
        #endregion

        #region Process Summaries 
        [HttpPost]
        public JsonResult SummarizeProcesses(Opmt opmt)
        {
            try
            {
                var result = OpdtBus.SummarizeProcesses(opmt);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtByMachine(Opmt opmt)
        {
            try
            {
                var result = OpdtBus.SummarizeOpdtByMachine(opmt);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtByTools(Opmt opmt)
        {
            try
            {
                //START MOD) SON - 2/Jul/2019 - Get list tool linking by Edition
                // Get list of tool linking (OPTL)
                //var processes = OptlBus.GetListOptl(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, 0);
                var processes = OptlBus.GetListOptlByEdition(opmt.Edition, opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, 0);
                //START MOD) SON - 2/Jul/2019
                var tools = OpdtBus.SummarizeOpdtByTools(opmt);
                var jsonResult = Json(new { processes, tools }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtAllMachine(Opmt opmt)
        {
            try
            {
                // Get list of tool linking (OPTL)
                var machine = OpdtBus.SummarizeOpdtAllMachine(opmt);
                var jsonResult = Json(new { machine }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtByWorker(Opmt opmt)
        {
            try
            {
                var workers = OpdtBus.SummarizeOpdtWorker(opmt);
                var jsonResult = Json(new { workers }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get list of patterns
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public JsonResult GetPatterns(string edition, string styleCode, string styleSize, string styleColorSerial,
            string revNo, string opRevNo, string languageId)
        {
            try
            {
                var lstProts = ProtBus.GetListProts(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, languageId);

                foreach (var prot in lstProts)
                {
                    if (string.IsNullOrEmpty(prot.OpNameLan))
                    {
                        prot.OpNameLan = prot.OpName;
                    }
                }
                //Buyer/StyleCode/StyleCode + StyleSize + CadColorSerial + RevNo/CadFileName/Thumbnail/ PiceUnique.png
                //Buyer/StyleCode/StyleCode + StyleSize + CadColorSerial + RevNo/CadFileName/PiceUnique.png
                //ftp://203.113.151.204/PKPDM/UNI/UNI0036/UNI0036LRG000002/UNI0036LRG000002002012/Thumbnail/U00003.png
                var ftpHost = FtpInfoBus.GetFtpInfo(ConstantGeneric.FtpAppTypePlmHost);
                foreach (var prot in lstProts)
                {
                    if (!string.IsNullOrEmpty(prot.CadColorSerial) && !string.IsNullOrEmpty(prot.CadFile))
                    {
                        //Get server host
                        var serverHost = ftpHost.FtpLink + ftpHost.FtpFolder + "/";
                        var thumbnailFol = "Thumbnail/";
                        //Create sub folder.
                        var subFolder = styleCode.Substring(0, 3) + "/" + styleCode + "/" + styleCode + styleSize + prot.CadColorSerial + revNo
                                        + "/" + prot.CadFile.Substring(0, prot.CadFile.IndexOf(".")) + "/";
                        var subFolThumb = subFolder + thumbnailFol;
                        var cadFilePng = prot.PieceUnique + ".png";
                        var cadFileSvg = prot.PieceUnique + ".svg";
                        //Get thumbnail file to load on gridview.
                        var thumbLink = serverHost + subFolThumb + cadFilePng;
                        //Get SVG file to show detail.
                        var svgLink = serverHost + subFolder + cadFileSvg;
                        if (!CommonUtility.IsExistUrl(svgLink)) svgLink = serverHost + subFolder + cadFilePng;
                        prot.Url = svgLink;
                        prot.UrlThumbnail = thumbLink;
                    }

                }

                return Json(lstProts, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeBomByProcess(Opmt opmt, string language)
        {
            try
            {
                var prots = ProtBus.SummarizeBomByProcess(opmt, language);

                return Json(new { prots }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeJigFile(Opmt opmt, string language, string sumJigMode)
        {
            try
            {
                var opdts = OpdtBus.SummarizeJigFile(opmt, language, sumJigMode);
                var ftpInfo = FtpInfoBus.GetFtpInfo("PLMHOST");

                return Json(new { result = opdts, ftpInfo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
