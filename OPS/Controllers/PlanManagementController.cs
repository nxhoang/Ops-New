using Newtonsoft.Json;
using OPS.GenericClass;
using OPS.Models;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OPS.Controllers
{
    [SessionTimeout]
    public class PlanManagementController : Controller
    {
        public string MenuOpsId => ConstantGeneric.OpManagementMenuId;
        public string MenuFomId => ConstantGeneric.FactoryMenu;
        public string SystemOpsId => ConstantGeneric.OpsSystemId;
        public string SystemMesId => ConstantGeneric.MesSystemId;
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, MenuOpsId);
        public Srmt RoleFom => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, MenuFomId);
        public Srmt RoleMes => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemMesId, ConstantGeneric.MesMenu);

        // GET: PlanManagement
        public ActionResult PlanManagement()
        {
            return View();
        }

        public JsonResult GetOperationGroup(string groupLevel, string parentId)
        {
            //Set parent Id is -1 if group level is difference with 0 and parent id is empty
            if (groupLevel != "0") parentId = string.IsNullOrEmpty(parentId) ? "-1" : parentId;

            return Json(OpnmBus.GetOperationGroup(groupLevel, parentId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpGrade(string mCode)
        {
            try
            {
                var lstMtCode = McmtBus.GetMasterCode(mCode).Where(m => m.CodeStatus == "OK");
                foreach (var mc in lstMtCode)
                {
                    mc.CodeName = mc.CodeDesc + " - " + mc.CodeName;
                }

                return Json(lstMtCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public JsonResult GetOpNameHasMachine(string opNameId)
        {
            try
            {
                var opName = OpnmBus.GetOpName(opNameId);
                if (opName == null) return Json(new List<Opnm>(), JsonRequestBehavior.AllowGet);
                //Add object opname to list to return to client
                return Json(new List<Opnm>() { opName }.Where(nm => nm.MachineId != null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public JsonResult GetOpNameLevel(string opNameId)
        {
            try
            {
                var opnm = OpnmBus.GetOpNameLevel(opNameId);

                return Json(opnm, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        public string AddNewProcess_New(Opdt opDetail, List<Optl> lstOpMachine, List<Optl> lstOpTool, List<Opnt> lstOpnt)
        {
            var listTempImagePath = new List<string>();
            var listTempVideoPath = new List<string>();
            var addNewResult = string.Empty;
            try
            {
                var srcTempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);

                //START ADD - SON) 13/Nov/2020 - Get image and video name of process name detail    
                //Add image temporary path to the list
                if (!string.IsNullOrEmpty(opDetail.ImageName)) listTempImagePath.Add(srcTempFolder + opDetail.ImageName);
                //Add video temporary path to the list
                if (!string.IsNullOrEmpty(opDetail.VideoFile)) listTempVideoPath.Add(srcTempFolder + opDetail.VideoFile);

                foreach (var opnt in lstOpnt)
                {
                    //Add image temporary path to the list
                    if (!string.IsNullOrEmpty(opnt.ImageName)) listTempImagePath.Add(srcTempFolder + opnt.ImageName);
                    //Add video temporary path to the list
                    if (!string.IsNullOrEmpty(opnt.VideoFile)) listTempVideoPath.Add(srcTempFolder + opnt.VideoFile);
                }
                //START ADD - SON) 13/Nov/2020

                //Add process
                var blResultAdd = OpdtBus.AddNewProccess_New(opDetail, lstOpMachine, lstOpTool, lstOpnt);
                if (blResultAdd)
                {
                    //Count processes, machines, wokers and calculate optime. 
                    var objOpmt = CommonUtility.CountOperationPlan(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo);
                    //Update operation master.
                    OpmtBus.UpdateOpMaster(objOpmt);

                    foreach (var tempImagePath in listTempImagePath)
                    {
                        //Move image file from temporary folder after adding new process into database
                        if (System.IO.File.Exists(tempImagePath))
                        {
                            //var proImgDir = Server.MapPath(ConstantGeneric.ProcessImageDirectory);
                            var proImgDir = ConstantGeneric.ProcessImageDirectory;
                            MoveFile(Path.GetFileName(tempImagePath), srcTempFolder, proImgDir);
                        }
                    }

                    foreach (var tempVideoPath in listTempVideoPath)
                    {
                        //Upload video to FTP server
                        if (System.IO.File.Exists(tempVideoPath))
                        {
                            if (System.IO.File.Exists(tempVideoPath))
                            {
                                var ftpInfo = new FtpInfo()
                                {
                                    //ftp://203.113.151.204/BETAPDM/operationvideos/
                                    FtpRoot = ConstantGeneric.FtpOpVideoDirectory,
                                    FtpUser = ConstantGeneric.FtpOpVideoUser,
                                    FtpPass = ConstantGeneric.FtpOpVideoPassword,
                                };

                                var fileName = Path.GetFileName(tempVideoPath);// opDetail.VideoFile;

                                //Create folder on FTP server.
                                FtpInfoBus.CreateFTPDirectory(ftpInfo, "");
                                //Create FTP file path string to upload Jig file to FTP
                                var ftpFilePath = ftpInfo.FtpRoot + fileName;
                                var statusUpload = FtpInfoBus.UploadFileToFtpFromSource(tempVideoPath, ftpFilePath, ftpInfo);
                            }
                        }
                    }
                    //Record log add new process.      
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(opDetail, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new process.", status);
                }

                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Cannot add new process!");

            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                addNewResult = CommonUtility.ObjectToJson(exc.Message);
            }
            finally
            {
                //Delete image and video path
                listTempVideoPath.AddRange(listTempImagePath);
                foreach (var tempPath in listTempVideoPath)
                {
                    DeleteLocalProcessFile(tempPath);
                }
            }

            return addNewResult;
        }

        [HttpPost]
        public string UpdateOpDetail_New(Opdt opDetail, List<Optl> lstMachine, List<Optl> lstTool, List<Opnt> lstOpnt)
        {

            var listTempImagePath = new List<string>();
            var listTempVideoPath = new List<string>();
            var updateResult = string.Empty;
            try
            {
                var srcTempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);

                //START ADD - SON) 13/Nov/2020 - Get image and video name of process name detail    
                //Add image temporary path to the list
                if (!string.IsNullOrEmpty(opDetail.ImageName)) listTempImagePath.Add(srcTempFolder + opDetail.ImageName);
                //Add video temporary path to the list
                if (!string.IsNullOrEmpty(opDetail.VideoFile)) listTempVideoPath.Add(srcTempFolder + opDetail.VideoFile);

                foreach (var opnt in lstOpnt)
                {
                    //Add image temporary path to the list
                    if (!string.IsNullOrEmpty(opnt.ImageName)) listTempImagePath.Add(srcTempFolder + opnt.ImageName);
                    //Add video temporary path to the list
                    if (!string.IsNullOrEmpty(opnt.VideoFile)) listTempVideoPath.Add(srcTempFolder + opnt.VideoFile);
                }
                //START ADD - SON) 13/Nov/2020

                //update process
                var blResultUpdate = OpdtBus.UpdateOpDetail_New(opDetail, lstMachine, lstTool, lstOpnt);

                //Record log update process.      
                var status = CommonUtility.ConvertBoolToString01(blResultUpdate);
                CommonUtility.InsertLogActivity(opDetail, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventEdit, "Update process.", status);

                if (blResultUpdate)
                {
                    //Count processes, machines, wokers and calculate optime. 
                    var objOpmt = CommonUtility.CountOperationPlan(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo);
                    //Update operation master.
                    OpmtBus.UpdateOpMaster(objOpmt);

                    foreach (var tempImagePath in listTempImagePath)
                    {
                        //Move temporary image.
                        if (System.IO.File.Exists(tempImagePath))
                        {
                            //Move image file from temporary folder after adding new process into database
                            //var proImgDir = Server.MapPath(ConstantGeneric.ProcessImageDirectory);
                            var proImgDir = ConstantGeneric.ProcessImageDirectory;
                            MoveFile(Path.GetFileName(tempImagePath), srcTempFolder, proImgDir);
                        }
                    }

                    foreach (var tempVideoPath in listTempVideoPath)
                    {
                        //Move temporary video.
                        if (System.IO.File.Exists(tempVideoPath))
                        {

                            if (System.IO.File.Exists(tempVideoPath))
                            {
                                var ftpInfo = new FtpInfo()
                                {
                                    //ftp://203.113.151.204/BETAPDM/operationvideos/
                                    FtpRoot = ConstantGeneric.FtpOpVideoDirectory,
                                    FtpUser = ConstantGeneric.FtpOpVideoUser,
                                    FtpPass = ConstantGeneric.FtpOpVideoPassword,
                                };

                                //var fileName = opDetail.VideoFile;
                                var fileName = Path.GetFileName(tempVideoPath);

                                //Create folder on FTP server.
                                FtpInfoBus.CreateFTPDirectory(ftpInfo, "");
                                //Create FTP file path string to upload Jig file to FTP
                                var ftpFilePath = ftpInfo.FtpRoot + fileName;
                                FtpInfoBus.UploadFileToFtpFromSource(tempVideoPath, ftpFilePath, ftpInfo);

                            }
                        }
                    }
                }

                return blResultUpdate ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Cannot upate process!");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                updateResult = CommonUtility.ObjectToJson(exc.Message);
            }
            finally
            {
                //Delete image and video path
                listTempVideoPath.AddRange(listTempImagePath);
                foreach (var tempPath in listTempVideoPath)
                {
                    DeleteLocalProcessFile(tempPath);
                }
            }

            return updateResult;
        }

        /// <summary>
        /// Moves the file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="srcFolder">The source folder.</param>
        /// <param name="desFolder">The DES folder.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static void MoveFile(string filename, string srcFolder, string desFolder)
        {
            var srcPathFile = srcFolder + filename;
            var desPathFile = desFolder + filename;

            if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(srcFolder) || string.IsNullOrEmpty(desFolder)) return;

            //Create parent folder if parent folder is not exist.
            if (!Directory.Exists(desFolder)) Directory.CreateDirectory(desFolder);

            // Ensure that the target does not exist and source file copy must be exist
            if (System.IO.File.Exists(desPathFile) && System.IO.File.Exists(srcPathFile))
            {
                System.IO.File.Delete(desPathFile);
            }

            //Check source file is exist before uploading
            if (System.IO.File.Exists(srcPathFile) && !System.IO.File.Exists(desPathFile))
            {
                //Move image file
                System.IO.File.Move(srcPathFile, desPathFile);
            }
        }

        [HttpPost]
        public void DeleteLocalProcessFile(string localFilePath)
        {
            try
            {
                if (System.IO.File.Exists(localFilePath))
                {
                    System.IO.File.Delete(localFilePath);
                }
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
            }

        }

        //
        [HttpPost]
        public string UploadVideoOpdt_New()
        {
            var data = Request.Form;
            var fileName = data["FileName"];
            var styleCode = data["StyleCode"];
            var styleSize = data["StyleSize"];
            var styleColor = data["StyleColor"];
            var styleRevNo = data["StyleRevNo"];
            var opRevNo = data["OpRevNo"];
            var edition = data["Edition"];
            var opSerial = data["OpSerial"];

            var userRole = GetUserRoleByEdition(edition);

            //Check role
            if (!CommonMethod.CheckRole(userRole.IsAdd)) return ConstantGeneric.Fail;

            string sysVideoName = ConstantGeneric.Fail;
            foreach (string file in Request.Files)
            {
                var fileDataContent = Request.Files[file];

                if (fileDataContent == null || fileDataContent.ContentLength <= 0) continue;
                // take the input stream, and save it to a temp folder using the original file.part name posted
                var stream = fileDataContent.InputStream;

                var sysFileName = styleCode + styleSize + styleColor + styleRevNo + opRevNo + opSerial + edition;

                var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);
                //Create directory for storing video.
                if (!CommonMethod.CreateFolder(tempFolder)) return ConstantGeneric.Fail;

                string path = Path.Combine(tempFolder, fileName);
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
                    ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                    return exc.Message;
                }
            }
            return Path.GetFileName(sysVideoName);
        }

        [HttpPost]
        public string UploadImageOpdt_New()
        {
            var sysFileName = ConstantGeneric.Fail;
            try
            {
                var data = Request.Form;
                var styleCode = data["StyleCode"];
                var styleSize = data["StyleSize"];
                var styleColor = data["StyleColor"];
                var styleRevNo = data["StyleRevNo"];
                var opRevNo = data["OpRevNo"];
                var edition = data["Edition"];
                var opSerial = data["OpSerial"];

                var userRole = GetUserRoleByEdition(edition);

                //Check role
                if (!CommonMethod.CheckRole(userRole.IsUpdate)) return ConstantGeneric.Fail;

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];

                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var fileName = fileContent.FileName;
                        var extFile = Path.GetExtension(fileName)?.ToLower();
                        var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);
                        //Create directory for storing image.
                        if (!CommonMethod.CreateFolder(tempFolder)) return ConstantGeneric.Fail;

                        //Create system image name
                        sysFileName = CreateSystemFileName_New(styleCode, styleSize, styleColor, styleRevNo, opRevNo, opSerial, extFile, edition);

                        if (!string.IsNullOrEmpty(sysFileName))
                        {
                            var pathSysFile = Path.Combine(tempFolder, sysFileName);
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
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return exc.Message;
            }

            return sysFileName;
        }

        private static string CreateSystemFileName_New(string styleCode, string styleSize, string styleColor, string styleRev, string opRevNo, string opSerial, string extFile, string edition)
        {
            //Check extention of file
            if (string.IsNullOrEmpty(extFile)) return "";

            //Check Operation Plan serial
            if (string.IsNullOrEmpty(opSerial)) return "";

            //Check selection Operation Plan master
            if (!CommonUtility.CheckKeyCodeOpMaster(styleCode, styleSize, styleColor, styleRev, opRevNo)) return "";

            //Create file name.
            //var fileName = styleCode + styleSize + styleColor + styleRev + opRevNo + opSerial + extFile;
            var fileName = styleCode + styleSize + styleColor + styleRev + opRevNo + opSerial + edition + extFile;

            return fileName;
        }

        [HttpPost]
        public JsonResult UploadImageSubProcess()
        {
            try
            {
                var data = Request.Form;
                string sysFileName = data["SysFileName"];

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];

                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var fileName = fileContent.FileName;
                        var extFile = Path.GetExtension(fileName)?.ToLower();
                        var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);
                        //Create directory for storing image.
                        if (!CommonMethod.CreateFolder(tempFolder)) return Json(new { IsSuccess = false, Log = "Cannot create folder to store image" }, JsonRequestBehavior.AllowGet);

                        if (!string.IsNullOrEmpty(sysFileName))
                        {
                            var pathSysFile = Path.Combine(tempFolder, sysFileName);
                            //Delete file if it exist
                            if (System.IO.File.Exists(pathSysFile)) System.IO.File.Delete(pathSysFile);
                            //Saving file
                            fileContent.SaveAs(pathSysFile);
                            //Return after saving file
                            return Json(new { IsSuccess = true, Result = sysFileName }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { IsSuccess = false, Log = "System file name is empty" }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new { IsSuccess = false, Log = "File is empty" }, JsonRequestBehavior.AllowGet); ;
                    }
                }

                return Json(new { IsSuccess = false, Result = sysFileName }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return Json(new { IsSuccess = false, Log = exc.Message }, JsonRequestBehavior.AllowGet); ;
            }
        }

        [HttpPost]
        public string UploadVideoSubProcess_New()
        {
            var data = Request.Form;
            var fileName = data["FileName"];
            var sysFileName = data["SysFileName"];

            string sysVideoName = ConstantGeneric.Fail;
            foreach (string file in Request.Files)
            {
                var fileDataContent = Request.Files[file];

                if (fileDataContent == null || fileDataContent.ContentLength <= 0) continue;
                // take the input stream, and save it to a temp folder using the original file.part name posted
                var stream = fileDataContent.InputStream;

                //var sysFileName = styleCode + styleSize + styleColor + styleRevNo + opRevNo + opSerial + opnSerial + edition; //ADD - SON) 18/Nov/2020 - add edition to system name

                var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);
                //Create directory for storing video.
                if (!CommonMethod.CreateFolder(tempFolder)) return ConstantGeneric.Fail;

                string path = Path.Combine(tempFolder, fileName);
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
                    sysVideoName = ut.MergeFile(path, Path.GetFileNameWithoutExtension(sysFileName));
                }
                catch (Exception exc)
                {
                    ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                    return exc.Message;
                }
            }
            return Path.GetFileName(sysVideoName);
        }

        public JsonResult DeleteImageOrVideoSubProcess(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string opNameId, string edition, bool isImage)
        {
            try
            {
                var refCode = styleCode + styleSize + styleColorSerial + revNo + opRevNo + opSerial + opNameId + edition;
                bool blResUpd = false;
                //if is image is true then update image name otherwise update video name
                blResUpd = OpntBus.UpdateImageOrVideoName(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, opNameId, edition, isImage);

                // Record log add new operation master.
                var status = CommonUtility.ConvertBoolToString01(blResUpd);
                CommonUtility.InsertLogActivity(refCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventDelete, "Update image name of sub process.", status);

                return blResUpd ? Json(new { IsSuccess = blResUpd, Result = "Updated" }, JsonRequestBehavior.AllowGet) :
                    Json(new { IsSuccess = blResUpd, Log = "Cannot update image" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(new { IsSuccess = false, Log = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetOperationIconLink(string opNameId)
        {
            var opName = OpnmBus.GetOpName(opNameId);

            return Json(new { IsSuccess = true, Result = opName }, JsonRequestBehavior.AllowGet);

        }
        #region User Role
        //Get User role by edtion
        private Srmt GetUserRoleByEdition(string edition)
        {
            switch (edition)
            {
                case ConstantGeneric.EditionAom:
                    return RoleFom;
                case ConstantGeneric.EditionMes:
                    return RoleMes;
                case ConstantGeneric.EditionOps:
                case ConstantGeneric.EditionPdm:
                    return Role;
                default:
                    return new Srmt();
            }
        }
        #endregion

        public JsonResult GetBomt(GridSettings gridRequest, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {
            if (!gridRequest.isSearch)
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
                var listLinkedPattern = ProtBus.GetListLinkedPattern(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                //keep list BOMT in session
                Session["ListBomtLinking"] = listBOM;
                Session["listLinkedPattern"] = listLinkedPattern;
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
            else
            {
                var listBOM = (List<Bomt>)Session["ListBomtLinking"];
                var filterBomt = FilterListWithGridRequest<Bomt>(gridRequest, listBOM);
                return Json(filterBomt, JsonRequestBehavior.AllowGet);
            }
        }

        private List<T> FilterListWithGridRequest<T>(GridSettings gridRequest, List<T> listData)
        {
            var lisDataQ = listData.AsQueryable();
            if (null != gridRequest.where && gridRequest.where.rules.Length > 0)
            {
                string strWhere = LinqExtensionsMethod.FilterNullExpression(gridRequest);
                if (string.IsNullOrEmpty(strWhere) == false)
                {
                    var data = lisDataQ.Where(strWhere).ToList();
                    lisDataQ = data.AsQueryable();
                }
                strWhere = LinqExtensionsMethod.GetAllStringFiltersExpression(gridRequest);
                if (string.IsNullOrEmpty(strWhere) == false)
                {
                    lisDataQ = lisDataQ.Where(strWhere);
                }
            }

            return lisDataQ.ToList();
        }

        public JsonResult GetPatterns(string styleCode, string styleSize, string styleColorSerial, string revNo, string itemCode, string itemColorSerial, string mainItemCode, string mainItemColorSerial)
        {
            //get list patterns by style
            var pattern = new PatternBus();
            var listPatterns = pattern.GetPatternByBom(styleCode, styleSize, styleColorSerial, revNo, itemCode, itemColorSerial, mainItemCode, mainItemColorSerial);
            var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
            foreach (var pt in listPatterns)
            {
                var linkedPt = listLinkedPattern.Find(x => x.ItemCode == pt.ItemCode && x.ItemColorSerial == pt.ItemColorSerial && x.MainItemCode == pt.MainItemCode && x.MainItemColorSerial == pt.MainItemColorSerial && x.PatternSerial == pt.PatternSerial);
                if(linkedPt != null)
                {
                    pt.Status = "Linked";
                }
            }

            return Json(listPatterns, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult GetLinkedItems(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string edition, bool isLinking)
        public JsonResult GetLinkedItems(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string edition, bool isLinking, string bomt, string ptmt)
        {
            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(edition))
            {
                return Json(new List<Prot>(), JsonRequestBehavior.AllowGet);
            }

            var listProts = new List<Prot>();
            if (isLinking)
            {
                var objBomt = JsonConvert.DeserializeObject<Bomt>(bomt);
                var objPtmt = JsonConvert.DeserializeObject<Bomt>(ptmt);
                listProts = (List<Prot>)Session["ListPortsByOpSerial"];
            }
            else
            {
                //get linked item with process
                listProts = ProtBus.GetListLinkedItem(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                var listLinkedPattern = ProtBus.GetListLinkedPattern(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                //filter list linked item by op serial
                listProts = listProts.FindAll(x => x.OpSerial == opSerial);
                listLinkedPattern = listLinkedPattern.FindAll(x => x.OpSerial == opSerial);
                Session["listLinkedItem"] = listProts;
                Session["listLinkedPattern"] = listLinkedPattern.FindAll(x => x.OpSerial == opSerial);

                foreach (var prot in listProts)
                {
                    var linkedPattern = listLinkedPattern.Find(x => x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial);
                    if(linkedPattern != null)
                    {
                        prot.HasPattern = "Y";
                    }
                }
            }
            return Json(listProts, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLinkedPatterns(string objProt)
        {
            var prot = JsonConvert.DeserializeObject<Prot>(objProt);
            var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
            listLinkedPattern = listLinkedPattern.FindAll(x => x.OpSerial == prot.OpSerial);

            return Json(listLinkedPattern, JsonRequestBehavior.AllowGet);
        }
    }
}