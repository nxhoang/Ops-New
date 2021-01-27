using MES.GenericClass;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using McmtBus = OPS_DAL.Business.McmtBus;

namespace MES.Controllers
{
    [SessionTimeout]
    public class OpsLayoutController : Controller
    {
        #region Properties
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt RnDRole => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, SystemMesId, MenuOpsId);
        public Srmt FactoryRole => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, SystemMesId, FactoryEdition);
        public Srmt MesRole => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, ConstantGeneric.MesSystemId, ConstantGeneric.MesMenu);
        public string FactoryEdition => ConstantGeneric.FactoryMenu;
        public string MenuOpsId => ConstantGeneric.OpManagementMenuId;
        public string SystemMesId => ConstantGeneric.MesSystemId;
        private readonly OpdtBus _OpdtBus = new OpdtBus();
        private readonly FtpInfoBus _FtpInfoBus = new FtpInfoBus();
        private readonly OpmtBus _OpmtBus = new OpmtBus();
        private readonly OpntBus _OpntBus = new OpntBus();
        

        #endregion

        #region General functions
        [HttpPost]
        public JsonResult GetModulesByCode(string styleCode)
        {
            try
            {
                var modules = SamtBus.GetByStyleCode(styleCode);

                return Json(new { modules }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventGetData);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetUserRole()
        {
            try
            {
                return Json(new { rdRole = RnDRole, facRole = FactoryRole, mesRole = MesRole }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventGetData);
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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventGetData);
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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetColorByTheme(string theme)
        {
            try
            {
                var colour = OpColorBus.GetByTheme(theme);
                return Json(colour, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetMpdtByMxPackage(string mxPackage)
        {
            try
            {
                var mpdt = MpdtBus.GetByMxPackage(mxPackage);
                return Json(new { result = mpdt }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Operation plan

        public async Task<ActionResult> RegisterOp(string editionReg, Opmt desOpmt, Opmt sourceOpmt, bool isCopyTool, bool isCopyOp,
            bool isImportFile, int sourceDb)
        {
            try
            {
                var opmt = OpmtBus.MySqlGetByMxPackage(desOpmt).FirstOrDefault();
                if (opmt != null)
                {
                    return Json("Existing operation plan of this package. Please reload layout.", JsonRequestBehavior.AllowGet);
                }

                // Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(desOpmt.Edition);
                //var systemId = SystemMesId;
                var objUserRole = SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, SystemMesId, menuId);
                if (objUserRole == null) return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                // Check role
                if (!CommonMethod.CheckRole(objUserRole.IsAdd))
                {
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);
                }

                var tools = new List<Optl>();
                var copiedOpdts = new List<Opdt>();
                var opnts = new List<Opnt>();

                if (isCopyOp)
                {

                    //Get operation master for copying
                    var copiedOpmt = OpmtBus.GetBySourceDb(sourceOpmt, sourceDb);

                    //Change Operation revision and edition to insert new one.
                    if (copiedOpmt != null)
                    {
                        copiedOpmt.StyleCode = desOpmt.StyleCode;
                        copiedOpmt.StyleSize = desOpmt.StyleSize;
                        copiedOpmt.StyleColorSerial = desOpmt.StyleColorSerial;
                        copiedOpmt.RevNo = desOpmt.RevNo;
                        copiedOpmt.OpRevNo = desOpmt.OpRevNo;
                        copiedOpmt.Edition = desOpmt.Edition;
                        copiedOpmt.Language = desOpmt.Language;
                        copiedOpmt.MxPackage = desOpmt.MxPackage;
                        desOpmt = copiedOpmt;
                    }
                    else
                    {
                        return Json("Could not find the source of selected operation master in database.", JsonRequestBehavior.AllowGet);
                    }

                    //Get list process to copy.
                    copiedOpdts = OpdtBus.GetByLanguage(sourceOpmt.StyleCode, sourceOpmt.StyleSize, sourceOpmt.StyleColorSerial,
                        sourceOpmt.RevNo, sourceOpmt.OpRevNo, sourceOpmt.Edition, desOpmt.Language, sourceDb);

                    //Check copying tool linking
                    if (isCopyTool)
                    {
                        tools = OptlBus.GetByStyle(sourceOpmt.StyleCode, sourceOpmt.StyleSize, sourceOpmt.StyleColorSerial,
                            sourceOpmt.RevNo, sourceOpmt.OpRevNo, sourceOpmt.Edition, sourceDb);

                        //Change Ops master key in list copy tool linking
                        foreach (var tool in tools)
                        {
                            tool.StyleCode = desOpmt.StyleCode;
                            tool.StyleColorSerial = desOpmt.StyleColorSerial;
                            tool.StyleSize = desOpmt.StyleSize;
                            tool.RevNo = desOpmt.RevNo;
                            tool.OpRevNo = desOpmt.OpRevNo;
                            tool.Edition = desOpmt.Edition;
                        }
                    }

                    //Get list of operation name detail for copying.
                    opnts = OpntBus.GetByOpdtAndLang(sourceOpmt.Edition, sourceOpmt.StyleCode, sourceOpmt.StyleSize,
                        sourceOpmt.StyleColorSerial, sourceOpmt.RevNo, sourceOpmt.OpRevNo, "", "", "", sourceDb);

                    //Change operation plan master key code
                    foreach (var opnt in opnts)
                    {
                        opnt.StyleCode = desOpmt.StyleCode;
                        opnt.StyleSize = desOpmt.StyleSize;
                        opnt.StyleColorSerial = desOpmt.StyleColorSerial;
                        opnt.RevNo = desOpmt.RevNo;
                        opnt.OpRevNo = desOpmt.OpRevNo;
                        opnt.Edition = editionReg;
                    }

                    //Change Ops key code in list Opdetail.
                    foreach (var opdt in copiedOpdts)
                    {
                        //set opname = opname language
                        if (!string.IsNullOrWhiteSpace(opdt.OpNameLan)) opdt.OpName = opdt.OpNameLan;

                        opdt.StyleCode = desOpmt.StyleCode;
                        opdt.StyleSize = desOpmt.StyleSize;
                        opdt.StyleColorSerial = desOpmt.StyleColorSerial;
                        opdt.RevNo = desOpmt.RevNo;
                        opdt.OpRevNo = desOpmt.OpRevNo;
                        opdt.Edition = editionReg;
                    }
                }
                else
                {
                    if (editionReg == ConstantGeneric.EditionAom)
                    {
                        string strAlert = "You cannot register new empty plan for Aom Edition";
                        return Json(strAlert, JsonRequestBehavior.AllowGet);
                    }
                }

                //Assign userid and registry date for opsmaster
                desOpmt.RegisterId = UserInf.UserName;

                var resAdd = await _OpmtBus.RegisterOp(desOpmt, copiedOpdts, opnts, tools, isCopyTool, isCopyOp);

                // Because of error 'Connection must be valid and open to rollback transaction',
                // moved this function out of RegisterOp function
                if (resAdd) await _OpntBus.BulkInsertOpntAsync(opnts);

                // Record log add new operation master.
                var status = CommonUtility.ConvertBoolToString01(resAdd);
                CommonUtility.InsertLogActivity(desOpmt, UserInf, SystemMesId, ConstantGeneric.ScreenRegistry,
                    ConstantGeneric.EventAdd, "Add new operation plan.", status);

                //return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
                return Json(new TaskResult<ActionResult> { Log = "Added", IsSuccess = resAdd }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.MesSystemId);
                return Json(new TaskResult<ActionResult> { Log = ex.Message, IsSuccess = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region CRUD Process
        public async Task<JsonResult> GetOp(Opmt opsMaster, string groupMode, string languageId)
        {
            try
            {
                object opdts;
                var opmt = OpmtBus.MySqlGetByMxPackage(opsMaster).FirstOrDefault();

                if (opmt == null) return Json(new
                {
                    opmt = (Opmt)null,
                    opdts = new
                    {
                        groups = new ArrayList(),
                        nodes = new ArrayList(),
                        edges = new ArrayList(),
                        groupsToAdd = new ArrayList()
                    }
                }, JsonRequestBehavior.AllowGet);

                opmt.Edition = "M";
                ConstantGeneric.CurrentMxPackage = opmt.MxPackage;
                opdts = await _OpdtBus.GetByOpGroup(opmt);

                var jsonResult = Json(new { opmt, opdts }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetOpdts(Opmt opsMaster, string groupMode, string languageId, int page)
        {
            try
            {
                object opdts;
                var opmt = OpmtBus.MySqlGetByMxPackage(opsMaster).FirstOrDefault();

                if (opmt == null) return Json(new
                {
                    opmt = (Opmt)null,
                    opdts = new
                    {
                        groups = new ArrayList(),
                        nodes = new ArrayList(),
                        edges = new ArrayList(),
                        groupsToAdd = new ArrayList()
                    }
                }, JsonRequestBehavior.AllowGet);

                opsMaster.Edition = "M";
                ConstantGeneric.CurrentMxPackage = opsMaster.MxPackage;

                if (groupMode == "FirstLoad")
                {
                    groupMode = string.IsNullOrEmpty(opmt.GroupMode) ? "OpGroup" : opmt.GroupMode;
                }

                switch (groupMode)
                {
                    case "MachineType":
                        opdts = await _OpdtBus.GetByMachineType(opsMaster, page);
                        break;
                    case "ModuleType":
                        opdts = await _OpdtBus.GetByModuleType(opsMaster, page);
                        break;
                    default:
                        opdts = await _OpdtBus.GetByOpGroup(opsMaster, page);
                        break;
                }

                var jsonResult = Json(new { opmt, opdts }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventAdd);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteProcess(Opdt opdt)
        {
            try
            {
                //Check user role before delete process.
                var edition = opdt.Edition?.Substring(0, 1);
                var menuId = CommonUtility.GetMenuIdByEdition(edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? ConstantGeneric.MesSystemId : SystemMesId;
                var userRole = SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, systemId, menuId);

                if (!CommonMethod.CheckRole(userRole.IsDelete))
                {
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);
                }

                var result = OpdtBus.DeleteOpdtAndTool(opdt);

                //Count processes, machines, wokers and calculate optime. 
                var opmt = OpmtBus.CountOperationPlan(opdt.Edition, opdt.StyleCode, opdt.StyleSize,
                    opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo);

                // Record log delete process.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opdt, UserInf, SystemMesId, ConstantGeneric.LayoutMenuId,
                    ConstantGeneric.EventDelete, "Delete process.", status);

                //Update operation master.
                OpmtBus.UpdateOpmt(opmt);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventDelete);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CloneSingleProcess(Opdt opdt)
        {
            try
            {
                var maxOpSerial = OpdtBus.GetMaxOpSerial(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial,
                    opdt.RevNo, opdt.OpRevNo);
                var copiedOpnts = OpntBus.GetByOpdtAndLang(opdt.Edition, opdt.StyleCode, opdt.StyleSize,
                    opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo, opdt.OpSerial.ToString(), null, "");
                var p = OpdtBus.Get(opdt);
                var opNum = string.IsNullOrEmpty(p.OpNum) ? ".1" : $"{p.OpNum}.1";
                var copiedOpdt = new Opdt(opdt.Edition, p.StyleCode, p.StyleSize, p.StyleColorSerial, p.RevNo, p.OpRevNo,
                    maxOpSerial, opNum, opdt.OpGroup, p.OpName, p.Factory, opdt.MachineType, p.ToolId, p.OpTime, p.MaxTime,
                    p.BenchmarkTime, p.MachineCount, p.Remarks, p.JobType, p.ManCount, opdt.ModuleId, p.OpPrice,
                    p.OfferOpPrice, p.OutSourced, p.HotSpot, opdt.X, opdt.Y, opdt.Page, opdt.DisplayColor);
                var optls = OptlBus.GetByStyle(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo,
                    opdt.OpRevNo, opdt.Edition).Where(x => x.OpSerial == opdt.OpSerial).ToList();
                var cloneResult = OpdtBus.Clone(copiedOpdt, copiedOpnts, optls);

                // Record log copy new operation.
                var status = CommonUtility.ConvertBoolToString01(cloneResult);
                CommonUtility.InsertLogActivity(opdt, UserInf, SystemMesId, ConstantGeneric.LayoutMenuId,
                    ConstantGeneric.EventAdd, "Clone process", status);

                if (!cloneResult) return Json(new { result = false }, JsonRequestBehavior.AllowGet);

                //Count processes, machines, wokers and calculate optime. 
                var opmt = OpmtBus.CountOperationPlan(opdt.Edition, opdt.StyleCode, opdt.StyleSize,
                    opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo);

                //Update operation master.
                OpmtBus.UpdateOpmt(opmt);

                return Json(new { result = copiedOpdt }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventAdd);
                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateOpmtMxPackage(Opmt opmt)
        {
            try
            {
                var result = OpmtBus.UpdateOpmtMxPackage(opmt);
                return Json(new { result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.ActionUpdate);
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

                var result = OpdtBus.UpdateLayoutOpdts(opdts, opmt);

                // Record logs.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opmt, UserInf, SystemMesId, ConstantGeneric.LayoutMenuId,
                    ConstantGeneric.EventEdit, "Save data.", status);

                // Update MachineCount, ManCount, OpCount, OpTime = tackTime;
                Task unused = CommonUtility.UpdateOpmt(opmt);

                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventEdit);
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CopyToNewOps(Opmt opmt, List<Opdt> opdts)
        {
            try
            {
                // Update field opmt
                opmt.RegisterId = UserInf.UserName;
                var result = OpmtBus.CopyOpsMaster(opmt, opdts);

                // Record log copy new operation.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opmt, UserInf, SystemMesId, ConstantGeneric.LayoutMenuId, ConstantGeneric.EventAdd, "Copy to new process.", status);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventAdd);
                return Json(new { error = "An error occurred, please contact admin." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ConfirmOpmt(Opmt opmt)
        {
            try
            {
                opmt.ConfirmedId = UserInf.UserName;
                opmt.ConfirmChk = "Y";
                var result = OpmtBus.ConfirmOperationPlan(opmt);

                // Record log copy new operation.
                var status = CommonUtility.ConvertBoolToString01(result);
                CommonUtility.InsertLogActivity(opmt, UserInf, SystemMesId, ConstantGeneric.LayoutMenuId,
                    ConstantGeneric.EventConfirm, "Confirm operation master.", status);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventEdit);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> UpdatePage(List<Opdt> opdts)
        {
            try
            {
                if (opdts == null || opdts.Count == 0)
                {
                    return Json(new { error = "No process to update." }, JsonRequestBehavior.AllowGet);
                }

                var result = await _OpdtBus.MySqlUpdatePage(opdts);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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

                var result = await _OpdtBus.MySqlUpdateModule(opdts);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventAdd);

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
                    CommonUtility.CreateMessageException(exc.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventAdd);
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
                //var _rulesHubContext = new OpsVideoHub();

                var resultList = new List<TaskResult<VideoOpdt>>();
                var ffmpegFilePath = Server.MapPath("~/Assets/ffmpeg/bin/ffmpeg.exe");
                var combineStyle = $"{opdtVideos[0].StyleCode}{opdtVideos[0].StyleSize}{opdtVideos[0].StyleColorSerial}{opdtVideos[0].RevNo}{opdtVideos[0].OpRevNo}";
                var styleFolder = Path.Combine("ops", "M", $"{combineStyle}");
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

                    var updateVideoFile = await _OpdtBus.MySqlUpdateVideoFile(opdtVideo);

                    videoNo++;
                    //var connId = _OpsVideoHub.GetConnectionId();

                    OpsVideoHub.SendMessage(opdtVideo.DisplayName, videoNo, connectionId);
                    //_OpsVideoHub.SendMessage("This is message from SignalR", videoNo, clientId);
                    //_rulesHubContext.Clients.Client(connectionId).sendMessage("This is message from SignalR", videoNo.ToString());

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
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                var result = OpdtBus.MySqlSummarizeProcesses(opmt);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtByMachine(Opmt opmt)
        {
            try
            {
                var result = OpdtBus.MySqlSummarizeOpdtByMachine(opmt);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtByTools(Opmt opmt)
        {
            try
            {
                var processes = OptlBus.GetByStyle(opmt.StyleCode, opmt.StyleSize, opmt.StyleColorSerial,
                    opmt.RevNo, opmt.OpRevNo, opmt.Edition).Where(x => x.Machine == "0" || x.Machine is null);
                var tools = OpdtBus.MySqlSummarizeOpdtByTools(opmt);
                var jsonResult = Json(new { processes, tools }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtAllMachine(Opmt opmt)
        {
            try
            {
                // Get list of linking tools (OPTL)
                var machine = OpdtBus.MySqlSummarizeOpdtAllMachine(opmt);
                var jsonResult = Json(new { machine }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeOpdtByWorker(Opmt opmt)
        {
            try
            {
                var workers = OpdtBus.MySqlSummarizeOpdtWorker(opmt);
                //var workers = OpdtBus.SummarizeOpdtWorker(opmt);
                var jsonResult = Json(new { workers }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
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
                //var lstProts = ProtBus.GetListProts(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, languageId);
                var prots = ProtBus.GetProts(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, languageId);

                foreach (var prot in prots)
                {
                    if (string.IsNullOrEmpty(prot.OpNameLan))
                    {
                        prot.OpNameLan = prot.OpName;
                    }
                }
                var ftpHost = FtpInfoBus.GetFtpInfo(ConstantGeneric.FtpAppTypePlmHost);
                foreach (var prot in prots)
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

                return Json(prots, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId, ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeBomByProcess(Opmt opmt, string language)
        {
            try
            {
                //var prots = ProtBus.SummarizeBomByProcess(opmt, language);
                var prots = OpdtBus.MySqlSummarizeBomByProcess(opmt, language);

                return Json(new { prots }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SummarizeJigFile(Opmt opmt, string language, string sumJigMode)
        {
            try
            {
                //var opdts = OpdtBus.SummarizeJigFile(opmt, language, sumJigMode);
                var opdts = OpdtBus.MySqlSummarizeJigFile(opmt, language, sumJigMode);

                var ftpInfo = FtpInfoBus.GetFtpInfo("PLMHOST");

                return Json(new { result = opdts, ftpInfo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemMesId,
                    ConstantGeneric.EventGetData);

                return Json(new { error = msg }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
