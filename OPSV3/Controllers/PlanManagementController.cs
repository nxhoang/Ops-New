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

        public JsonResult GetBomt(GridSettings gridRequest, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, bool isLinking)
        {
            if (!gridRequest.isSearch)
            {
                if (isLinking)
                {
                    var listBOM = (List<Bomt>)Session["listBom"];
                    return Json(listBOM, JsonRequestBehavior.AllowGet);
                }
                else
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

                    //keep list BOMT in session
                    Session["listBom"] = listBOM;
                    Session["listPattern"] = listPatterns;
                    //Session["listLinkedPattern"] = listLinkedPattern;
                    return Json(listBOM, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var listBOM = (List<Bomt>)Session["listBom"];
                var filterBomt = FilterListWithGridRequest(gridRequest, listBOM);
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
            //var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
            var listLinkedPattern = (List<Prot>)Session["listLinkedPatterns"];
            foreach (var pt in listPatterns)
            {
                var linkedPt = listLinkedPattern.Find(x => x.ItemCode == pt.ItemCode && x.ItemColorSerial == pt.ItemColorSerial && x.MainItemCode == pt.MainItemCode && x.MainItemColorSerial == pt.MainItemColorSerial && x.PatternSerial == pt.PatternSerial);
                if (linkedPt != null)
                {
                    pt.Status = "Linked";
                    pt.PieceQtyRest = (short)linkedPt.PieceQtyRest;
                }
            }

            return Json(listPatterns, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLinkedItems(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string edition, bool isLinking)
        {
            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(edition))
            {
                return Json(new List<Prot>(), JsonRequestBehavior.AllowGet);
            }

            var listLinkedPtByOpSerial = new List<Prot>();
            if (isLinking)
            {
                var listCurrentLinkedItem = (List<Prot>)Session["listLinkedItem"];
                var listCurrentLinkedPattern = (List<Prot>)Session["listLinkedPattern"];

                //get list linked item by opserial
                var listLinkedItemByOpSerial = listCurrentLinkedItem.FindAll(x => x.OpSerial == opSerial).ToList();
                //check item whether has pattern or not.
                foreach (var item in listLinkedItemByOpSerial)
                {
                    //find pattern in current list. If it existed then set property HasPattern is Y
                    var existedLinkedPattern = listCurrentLinkedPattern.Find(x => x.ItemCode == item.ItemCode && x.ItemColorSerial == item.ItemColorSerial && x.MainItemCode == item.MainItemCode && x.MainItemColorSerial == item.MainItemColorSerial && x.OpSerial == item.OpSerial);
                    if (existedLinkedPattern != null) item.HasPattern = "Y";
                    else item.HasPattern = "N";
                }
                return Json(listLinkedItemByOpSerial, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //get list linked pattern
                var listLinkedPattern = ProtBus.GetListLinkedPattern(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                //get linked item with process
                var listProts = ProtBus.GetListLinkedItem(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                var listProtFull = ProtBus.GetListProt(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                //update pattern serial for item
                foreach (var prot in listProts)
                {
                    //get linked item with pattern serial is 000
                    var item = listProtFull.Find(x => x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpSerial == prot.OpSerial && x.PatternSerial == "000");
                    if (item != null)
                    {
                        prot.PatternSerial = item.PatternSerial;
                        prot.OpType = item.OpType;
                    }
                }
                Session["listLinkedItem"] = listProts;
                Session["listLinkedPattern"] = listLinkedPattern;
                //filter list linked item by op serial
                listLinkedPtByOpSerial = listProts.FindAll(x => x.OpSerial == opSerial);
                listLinkedPattern = listLinkedPattern.FindAll(x => x.OpSerial == opSerial);

                foreach (var prot in listLinkedPtByOpSerial)
                {
                    var linkedPattern = listLinkedPattern.Find(x => x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpSerial == prot.OpSerial);
                    if (linkedPattern != null)
                    {
                        prot.HasPattern = "Y";
                    }
                }
            }
            return Json(listLinkedPtByOpSerial, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLinkedPatterns(string objProt)
        {
            var prot = JsonConvert.DeserializeObject<Prot>(objProt);
            var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
            listLinkedPattern = listLinkedPattern.FindAll(x => x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpSerial == prot.OpSerial);

            return Json(listLinkedPattern, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LinkingBomAndPattern(List<Prot> listLinkedItem, List<Prot> listLinkedPattern)
        {
            //get current linked Items and Patterns from session.
            var listCurrentLinkedItem = (List<Prot>)Session["listLinkedItem"];
            var listCurrentLinkedPattern = (List<Prot>)Session["listLinkedPattern"];

            foreach (var item in listLinkedItem ?? new List<Prot>())
            {
                //check item whether exist in current list or not. If it doesn't exit then adding it to current list.
                Prot existedItem = listCurrentLinkedItem.Find(x => x.ItemCode == item.ItemCode && x.ItemColorSerial == item.ItemColorSerial && x.MainItemCode == item.MainItemCode && x.MainItemColorSerial == item.MainItemColorSerial && x.OpSerial == item.OpSerial);
                if (existedItem == null)
                {
                    listCurrentLinkedItem.Add(item);
                }
            }
            foreach (var pt in listLinkedPattern ?? new List<Prot>())
            {
                // pt.RemainPieceQty
                Prot existedPt = listCurrentLinkedPattern.Find(x => x.ItemCode == pt.ItemCode && x.ItemColorSerial == pt.ItemColorSerial && x.MainItemCode == pt.MainItemCode && x.MainItemColorSerial == pt.MainItemColorSerial && x.OpSerial == pt.OpSerial && x.PatternSerial == pt.PatternSerial);
                if (existedPt == null)
                {
                    pt.PieceQtyRest -= pt.PieceQty;
                    listCurrentLinkedPattern.Add(pt);
                }
            }
            //update session list linked item and patterns with the new list
            Session["listLinkedItem"] = listCurrentLinkedItem;
            Session["listLinkedPattern"] = listCurrentLinkedPattern;

            return Json(new { IsSuccess = true, Result = "Linked" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveLinkingBomPattern(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string edition)
        {
            try
            {
                var listLinkedItem = (List<Prot>)Session["listLinkedItem"];
                var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
                //get list linked item and pattern by opserial
                listLinkedItem = listLinkedItem.FindAll(x => x.OpSerial == opSerial);
                listLinkedPattern = listLinkedPattern.FindAll(x => x.OpSerial == opSerial);

                //get list linked item with pattern serial equal 000 then combine it with the list linked pattern
                var listProt = listLinkedItem.FindAll(x => x.PatternSerial == "000");
                if (listProt != null) listProt.AddRange(listLinkedPattern);
                else listProt = listLinkedPattern;
                //add pattern linkin gby opSerial
                ProtBus.AddPatternLinkingByOpSerial(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, listProt);

                return Json(new { IsSuccess = true, Result = "Saved" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, Log = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteBomPatternLinking(Prot prot, bool isItem)
        {
            //get list linked item and patterns from session
            var listLinkedItem = (List<Prot>)Session["listLinkedItem"];
            var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
            //if isItem is true then remove item and all patterns in this item otherwise remove pattern linking only
            if (isItem)
            {
                listLinkedItem.RemoveAll(x => x.OpSerial == prot.OpSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && (x.PatternSerial == "000" || string.IsNullOrEmpty(x.PatternSerial)) && x.OpType == prot.OpType);
                listLinkedPattern.RemoveAll(x => x.OpSerial == prot.OpSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpType == prot.OpType);
                Session["listLinkedItem"] = listLinkedItem;
                Session["listLinkedPattern"] = listLinkedPattern;
            }
            else
            {
                //remove pattern from current list               
                listLinkedPattern.RemoveAll(x => x.OpSerial == prot.OpSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.PatternSerial == prot.PatternSerial && x.OpType == prot.OpType);

                //update session list linked item and patterns with the new list
                Session["listLinkedPattern"] = listLinkedPattern;
            }
            return Json(new { IsSuccess = true, Result = "Deleted" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckLinkedPattern(List<Prot> listPattern)
        {
            //get list linked pattern
            var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
            foreach (var pt in listPattern ?? new List<Prot>())
            {
                var linkedPt = listLinkedPattern.Find(x => x.OpSerial == pt.OpSerial && x.ItemCode == pt.ItemCode && x.ItemColorSerial == pt.ItemColorSerial && x.MainItemCode == pt.MainItemCode && x.MainItemColorSerial == pt.MainItemColorSerial && x.PatternSerial == pt.PatternSerial);
                if (linkedPt != null)
                {
                    return Json(new { IsSuccess = false, Log = $"Pattern {linkedPt.Piece} was linked" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { IsSuccess = true, Result = "Ok" }, JsonRequestBehavior.AllowGet);
        }

        #region linking bom and patterns to sub processes (Op Name detail)
        public JsonResult GetListOpNameDetail(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string edition, string languageId, bool isLinking)
        {
            //if style information is null then return an emtpy list.
            if (string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(edition)) return Json(new List<Opnt>(), JsonRequestBehavior.AllowGet);
            if (!isLinking)
            {
                //get linked bom and pattern and push it to session
                GetLinkedBomPushToSession(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                //Get list process name detail
                var lstOpnts = OpntBus.GetOpNameDetails(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, "", languageId);
                Session["listOpNameDt"] = lstOpnts;
                if (lstOpnts != null)
                {
                    //get linked item with process
                    var listProts = ProtBus.GetListLinkedBom(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                    foreach (var opn in lstOpnts)
                    {
                        var existedBom = listProts.Find(x => x.OpSerial == opn.OpSerial && x.OpnSerial == opn.OpnSerial);
                        if (existedBom != null) opn.HasBom = "Y";
                    }

                    //update has pattern property
                    return Json(lstOpnts, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var listOpnDt = (List<Opnt>)Session["listOpNameDt"];
                var listLinkedBom = (List<Prot>)Session["listLinkedBom"];
                foreach (var opnt in listOpnDt)
                {
                    var opntHasBom = listLinkedBom.Find(x => x.OpSerial == opnt.OpSerial && x.OpnSerial == opnt.OpnSerial);
                    if (opntHasBom != null) opnt.HasBom = "Y";
                    else opnt.HasBom = "N";
                }

                Session["listOpNameDt"] = listOpnDt;
                return Json(listOpnDt, JsonRequestBehavior.AllowGet);
            }


            return Json(new List<Opnt>(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLinkedBom(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, int opnSerial, string edition, bool isLinking)
        {
            //If style code, size, color, revno is null or empty then return list of empty BOM
            if (string.IsNullOrEmpty(opRevNo) || opnSerial == 0 || string.IsNullOrEmpty(edition))
            {
                return Json(new List<Prot>(), JsonRequestBehavior.AllowGet);
            }

            var listLinkedPtByOpSerial = new List<Prot>();
            if (isLinking)
            {
                var listCurrentLinkedBom = (List<Prot>)Session["listLinkedBom"];
                var listCurrentLinkedPattern = (List<Prot>)Session["listLinkedPatterns"];

                //get list linked item by opserial
                var listLinkedItemByOpSerial = listCurrentLinkedBom.FindAll(x => x.OpSerial == opSerial && x.OpnSerial == opnSerial).ToList();
                //check item whether has pattern or not.
                foreach (var item in listLinkedItemByOpSerial)
                {
                    //find pattern in current list. If it existed then set property HasPattern is Y
                    var existedLinkedPattern = listCurrentLinkedPattern.Find(x => x.OpSerial == item.OpSerial && x.OpnSerial == item.OpnSerial && x.ItemCode == item.ItemCode && x.ItemColorSerial == item.ItemColorSerial && x.MainItemCode == item.MainItemCode && x.MainItemColorSerial == item.MainItemColorSerial && x.OpSerial == item.OpSerial);
                    if (existedLinkedPattern != null) item.HasPattern = "Y";
                    else item.HasPattern = "N";
                }
                return Json(listLinkedItemByOpSerial, JsonRequestBehavior.AllowGet);
            }

            return Json(listLinkedPtByOpSerial, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLinkedPatternsOpnDt(string objProt)
        {
            var prot = JsonConvert.DeserializeObject<Prot>(objProt);
            //var listLinkedPattern = (List<Prot>)Session["listLinkedPattern"];
            var listLinkedPattern = (List<Prot>)Session["listLinkedPatterns"];
            listLinkedPattern = listLinkedPattern.FindAll(x => x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpSerial == prot.OpSerial && x.OpnSerial == prot.OpnSerial);

            return Json(listLinkedPattern, JsonRequestBehavior.AllowGet);
        }

        private void GetLinkedBomPushToSession(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {
            //get list linked pattern
            var listLinkedPattern = ProtBus.GetListLinkedPattern(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
            //get linked bom with process
            var listProts = ProtBus.GetListLinkedBom(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
            //get all linked bom and patterns from t_sd_prot table
            var listProtFull = ProtBus.GetListProt(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
            //update pattern serial for item
            foreach (var prot in listProts)
            {
                //get linked item with pattern serial is 000
                var item = listProtFull.Find(x => x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpSerial == prot.OpSerial && x.PatternSerial == "000");
                if (item != null)
                {
                    prot.PatternSerial = item.PatternSerial;
                    prot.OpType = item.OpType;
                }
            }
            Session["listLinkedBom"] = listProts;
            Session["listLinkedPatterns"] = listLinkedPattern;
        }

        public JsonResult CheckLinkedPatternByOpnSerial(List<Prot> listPattern)
        {
            //get list linked pattern
            var listLinkedPattern = (List<Prot>)Session["listLinkedPatterns"];
            foreach (var pt in listPattern ?? new List<Prot>())
            {
                var linkedPt = listLinkedPattern.Find(x => x.OpSerial == pt.OpSerial && x.OpnSerial == pt.OpnSerial && x.ItemCode == pt.ItemCode && x.ItemColorSerial == pt.ItemColorSerial && x.MainItemCode == pt.MainItemCode && x.MainItemColorSerial == pt.MainItemColorSerial && x.PatternSerial == pt.PatternSerial);
                if (linkedPt != null)
                {
                    return Json(new { IsSuccess = false, Log = $"Pattern {linkedPt.Piece} was linked" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { IsSuccess = true, Result = "Ok" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LinkingBomAndPatternOpnDt(List<Prot> listLinkedItem, List<Prot> listLinkedPattern)
        {
            //get current linked Items and Patterns from session.
            var listCurrentLinkedItem = (List<Prot>)Session["listLinkedBom"];
            var listCurrentLinkedPattern = (List<Prot>)Session["listLinkedPatterns"];

            foreach (var item in listLinkedItem ?? new List<Prot>())
            {
                //check item whether exist in current list or not. If it doesn't exit then adding it to current list.
                Prot existedItem = listCurrentLinkedItem.Find(x => x.ItemCode == item.ItemCode && x.ItemColorSerial == item.ItemColorSerial && x.MainItemCode == item.MainItemCode && x.MainItemColorSerial == item.MainItemColorSerial && x.OpSerial == item.OpSerial && x.OpnSerial == item.OpnSerial);
                if (existedItem == null)
                {
                    listCurrentLinkedItem.Add(item);
                }
            }
            foreach (var pt in listLinkedPattern ?? new List<Prot>())
            {
                Prot existedPt = listCurrentLinkedPattern.Find(x => x.ItemCode == pt.ItemCode && x.ItemColorSerial == pt.ItemColorSerial && x.MainItemCode == pt.MainItemCode && x.MainItemColorSerial == pt.MainItemColorSerial && x.OpSerial == pt.OpSerial && x.OpnSerial == pt.OpnSerial && x.PatternSerial == pt.PatternSerial);
                if (existedPt == null)
                {
                    pt.PieceQtyRest -= pt.PieceQty;
                    listCurrentLinkedPattern.Add(pt);

                    //update pattern quantity for all pattern serial
                    var listExistedPts = listCurrentLinkedPattern.FindAll(x => x.ItemCode == pt.ItemCode && x.ItemColorSerial == pt.ItemColorSerial && x.MainItemCode == pt.MainItemCode && x.MainItemColorSerial == pt.MainItemColorSerial && x.PatternSerial == pt.PatternSerial);
                    foreach (var exstPt in listExistedPts ?? new List<Prot>())
                    {
                        exstPt.PieceQtyRest = pt.PieceQtyRest;
                    }
                }
                
            }
            //update session list linked item and patterns with the new list
            Session["listLinkedBom"] = listCurrentLinkedItem;
            Session["listLinkedPatterns"] = listCurrentLinkedPattern;

            return Json(new { IsSuccess = true, Result = "Linked" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveLinkingBomPatternOpnDt(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, int opSerial, string edition)
        {
            try
            {
                var listLinkedItem = (List<Prot>)Session["listLinkedBom"];
                var listLinkedPattern = (List<Prot>)Session["listLinkedPatterns"];
                //get list linked item and pattern by opserial
                listLinkedItem = listLinkedItem.FindAll(x => x.OpSerial == opSerial);
                listLinkedPattern = listLinkedPattern.FindAll(x => x.OpSerial == opSerial);

                //get list linked item with pattern serial equal 000 then combine it with the list linked pattern
                var listProt = listLinkedItem.FindAll(x => x.PatternSerial == "000");
                if (listProt != null) listProt.AddRange(listLinkedPattern);
                else listProt = listLinkedPattern;
                //add pattern linkin gby opSerial
                ProtBus.AddPatternLinkingByOpSerial(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, listProt);

                return Json(new { IsSuccess = true, Result = "Saved" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false, Log = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteBomPatternLinkingOpnDt(Prot prot, bool isItem)
        {            
            //get list linked item and patterns from session
            var listLinkedItem = (List<Prot>)Session["listLinkedBom"];
            var listLinkedPattern = (List<Prot>)Session["listLinkedPatterns"];
            //if isItem is true then remove item and all patterns in this item otherwise remove pattern linking only
            if (isItem)
            {
                //find all remove pattern
                var listDeletePatterns = listLinkedPattern.FindAll(x => x.OpSerial == prot.OpSerial && x.OpnSerial == prot.OpnSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpType == prot.OpType);
                //update remain piece quantity in list linked patterns
                foreach (var delPt in listDeletePatterns)
                {
                    var listLinkedPts = listLinkedPattern.FindAll(x => x.ItemCode == delPt.ItemCode && x.ItemColorSerial == delPt.ItemColorSerial && x.MainItemCode == delPt.MainItemCode && x.MainItemColorSerial == delPt.MainItemColorSerial && x.PatternSerial == delPt.PatternSerial);
                    foreach (var linkedPt in listLinkedPts ?? new List<Prot>())
                    {
                        linkedPt.PieceQtyRest += delPt.PieceQty;
                    }
                }

                listLinkedItem.RemoveAll(x => x.OpSerial == prot.OpSerial && x.OpnSerial == prot.OpnSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && (x.PatternSerial == "000" || string.IsNullOrEmpty(x.PatternSerial)) && x.OpType == prot.OpType);
                listLinkedPattern.RemoveAll(x => x.OpSerial == prot.OpSerial && x.OpnSerial == prot.OpnSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.OpType == prot.OpType);

                Session["listLinkedBom"] = listLinkedItem;
                Session["listLinkedPatterns"] = listLinkedPattern;
            }
            else
            {
                //get delete piece quantity if delete pattern
                var deletedPieceQty = prot.PieceQty;

                //remove pattern from current list               
                listLinkedPattern.RemoveAll(x => x.OpSerial == prot.OpSerial && x.OpnSerial == prot.OpnSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.PatternSerial == prot.PatternSerial && x.OpType == prot.OpType);
                //update remain piece quantity               
                //var linkedPattern = listLinkedItem.Find(x => x.OpSerial == prot.OpSerial && x.OpnSerial == prot.OpnSerial && x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.PatternSerial == prot.PatternSerial && x.OpType == prot.OpType);
                var linkedPattern = listLinkedPattern.Find(x => x.ItemCode == prot.ItemCode && x.ItemColorSerial == prot.ItemColorSerial && x.MainItemCode == prot.MainItemCode && x.MainItemColorSerial == prot.MainItemColorSerial && x.PatternSerial == prot.PatternSerial && x.OpType == prot.OpType);
                if (linkedPattern != null) linkedPattern.PieceQtyRest += deletedPieceQty;

                //update session list linked item and patterns with the new list
                Session["listLinkedPatterns"] = listLinkedPattern;
            }
            return Json(new { IsSuccess = true, Result = "Deleted" }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}