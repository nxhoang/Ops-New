using Newtonsoft.Json;
using OPS.GenericClass;
using OPS.Models;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Excel = Microsoft.Office.Interop.Excel;

namespace OPS.Controllers
{
    [SessionTimeout]
    public class OpsController : Controller
    {

        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, MenuOpsId);
        public Srmt RoleFom => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, MenuFomId);
        public Srmt RoleMes => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemMesId, ConstantGeneric.MesMenu);
        public string MenuOpsId => ConstantGeneric.OpManagementMenuId;
        public string MenuFomId => ConstantGeneric.FactoryMenu;
        public string SystemOpsId => ConstantGeneric.OpsSystemId;
        public string SystemMesId => ConstantGeneric.MesSystemId;
        public string VideoServerLink => ConstantGeneric.VideoServerLink;
        public readonly OpntBus _OpntBus = new OpntBus();

        //GET: OPS
        public ActionResult Ops()
        {
            ViewBag.RegisterId = UserInf.UserName;
            ViewBag.UserLoginEmail = UserInf.Email;
            ViewBag.PageTitle = "Ops Registry";
            return View();
        }

        public ActionResult ProcessTemplate()
        {
            ViewBag.PageTitle = "Process Template";
            return View();
        }

        public ActionResult Test()
        {
            ViewBag.RegisterId = UserInf.UserName;
            ViewBag.UserLoginEmail = UserInf.Email;
            ViewBag.PageTitle = "Test";
            return View();
        }

        public ActionResult VideoLink()
        {
            ViewBag.PageTitle = "Video Link";
            return View();
        }
        //Author: Son Nguyen Cao
        //Get get user role information
        public JsonResult GetUserRoleInfo(string sysId, string menuId)
        {
            try
            {
                if (string.IsNullOrEmpty(sysId) || string.IsNullOrEmpty(menuId)) return Json(new Srmt());
                var rol = SrmtBus.GetUserRoleInfo(UserInf.RoleID, sysId, menuId);

                return rol == null ? Json(new Srmt()) : Json(rol);

            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Author: Son Nguyen Cao
        //Get style master by style code
        public JsonResult GetStyleMasterByStyleCode(string styleCode)
        {
            try
            {
                //Get user role info
                var styleMaster = StmtBus.GetStyleMasterByStyleCode(styleCode);
                styleMaster.ImageLink = StmtBus.CreateStyleImageLink(styleMaster.Picture, styleCode);

                return Json(styleMaster, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpStyleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetEfficiency(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            try
            {
                //Get user role info
                var opdt = OpdtBus.GetEfficiency(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);

                return Json(opdt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Get tool linking style code
        //Author: Son Nguyen Cao
        public JsonResult GetToolLinkingByCode(Optl opTool)
        {
            try
            {
                //Get list tool linking by key code
                var lstTool = OptlBus.GetToolLinkingByCode(opTool);
                return Json(lstTool, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        #region Method

        ///
        /// Checks the file exists or not.
        ///
        /// The URL of the remote file.
        /// True : If the file exits, False if file not exists
        public JsonResult RemoteFileExists(string url)
        {
            try
            {
                ////Creating the HttpWebRequest
                //HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                ////Setting the Request method HEAD, you can also use GET too.
                //request.Method = "HEAD";
                ////Getting the Web Response.
                //HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                ////Returns TRUE if the Status code == 200
                //response.Close();
                //if (response.StatusCode == HttpStatusCode.OK) return Json(ConstantGeneric.Success);

                WebRequest req = WebRequest.Create(url);

                WebResponse res = req.GetResponse();

                return Json(ConstantGeneric.Success);
            }
            catch
            {
                //Any exception will returns false.
                return Json(ConstantGeneric.Fail);
            }
        }

        //Get list of style files
        public JsonResult GetStyleFiles()
        {
            try
            {
                var lstStyleFile = McmtBus.GetStyleFiles();
                return Json(lstStyleFile, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        // Get item main level
        public JsonResult GetItemMainLevel()
        {
            try
            {
                var lstLevel = IlhmBus.GetItemMainLevel();
                return Json(lstLevel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opActionCode"></param>
        /// <param name="opTempId"></param>
        /// <param name="opLanguage"></param>
        /// <returns></returns>
        /// Author: Ha Nguyen
        public JsonResult GetProcessNameTable(string opActionCode, string opTempId, string opLanguage)
        {
            try
            {
                var lstTemplate = OptpBus.GetProcessNameTable(opActionCode, opTempId, opLanguage);
                return Json(lstTemplate, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        public JsonResult GetTempName(string actionCode)
        {
            try
            {
                var lstTempName = PrthBus.GetTempName(actionCode);
                return Json(lstTempName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get Actioncode table
        public JsonResult GetActionCodeTable(string actionCode)
        {
            try
            {
                var lstTemplate = ActpBus.GetActionCodeTable(actionCode);
                return Json(lstTemplate, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get All Template
        public JsonResult GetAllTemplate()
        {
            try
            {
                var lstTemplate = PrthBus.GetAllTemplate();
                return Json(lstTemplate, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get All Process Name
        public JsonResult GetAllProcessName(string languageId)
        {
            try
            {
                var lstTemplate = OpnmBus.GetAllProcessName(languageId);
                return Json(lstTemplate, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        public string AddNewTemplate(Prth objTemplate)
        {
            try
            {
                var blResultAdd = PrthBus.AddNewTemplate(objTemplate);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(objTemplate, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new process.", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Cannot add new process!");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        public string AddProcessToTemplate(List<Optp> lstProcess)
        {
            //if (lstProcess == null) { return CommonUtility.ObjectToJson("Please choose Process Name!"); };
            try
            {
                var lstExistOptp = OptpBus.GetProcessNameTable(lstProcess[0].ActionCode, lstProcess[0].TempId.ToString(), "vn");

                foreach (var item in lstProcess)
                {
                    var bl = lstExistOptp.Any(i => i.ActionCode == item.ActionCode && i.TempId == item.TempId && i.OpNameId == item.OpNameId);


                    if (bl)
                    {
                        var objOptp = (from optp in lstExistOptp
                                       where optp.ActionCode == item.ActionCode && optp.TempId == item.TempId && optp.OpNameId == item.OpNameId
                                       select optp).FirstOrDefault();

                        return CommonUtility.ObjectToJson("Processes " + objOptp.OpNameLan + " existed!");
                    }
                }

                var blResultAdd = OptpBus.AddProcessToTemplate(lstProcess);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(lstProcess, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new process.", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Cannot add new process!");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        public string AddTemplateToActioncode(List<Actp> lstTemplate)
        {
            try
            {
                var blResultAdd = ActpBus.AddTemplateToActioncode(lstTemplate);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(lstTemplate, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new process.", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Cannot add new process!");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        //Delete Template
        public JsonResult DeleteTemplate()
        {
            try
            {
                var mdlRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.ProcessTemplateMenuId);
                if (mdlRole == null || !CommonMethod.CheckRole(mdlRole.IsDelete))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                var jsonOpKey = Request.Form["opKey"];
                var objPrth = JsonConvert.DeserializeObject<List<string>>(jsonOpKey);
                string combindedString = string.Join(",", objPrth.ToArray());

                var resDelete = PrthBus.DeleteTemplate(objPrth);

                var status = CommonUtility.ConvertBoolToString01(resDelete);
                CommonUtility.InsertLogActivity(combindedString, UserInf, SystemOpsId, ConstantGeneric.ScreenProcessTemplate, ConstantGeneric.EventDelete, "Delete template.", status);

                if (resDelete)
                {
                    return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
                }

                return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Delete Template Actioncode
        public JsonResult DeleteTemplateActioncode()
        {
            try
            {
                var mdlRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.ProcessTemplateMenuId);
                if (mdlRole == null || !CommonMethod.CheckRole(mdlRole.IsDelete))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                var jsonOpKey = Request.Form["opKey"];
                var objActp = JsonConvert.DeserializeObject<Actp>(jsonOpKey);

                if (objActp == null) return Json("Please choose template actioncode", JsonRequestBehavior.AllowGet);

                var resDel = ActpBus.DeleteTemplateActioncode(objActp);

                var status = CommonUtility.ConvertBoolToString01(resDel);
                CommonUtility.InsertLogActivity(objActp, UserInf, SystemOpsId, ConstantGeneric.ScreenProcessTemplate, ConstantGeneric.EventDelete, "Delete actioncode", status);

                if (resDel)
                {
                    return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
                }

                return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Delete Template
        public JsonResult DeleteProcessTemplate()
        {
            try
            {
                var mdlRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.ProcessTemplateMenuId);
                if (mdlRole == null || !CommonMethod.CheckRole(mdlRole.IsDelete))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                //var lstOptp = new List<Optp>();
                var jsonOpKey = Request.Form["opKey"];
                var lstOptp = JsonConvert.DeserializeObject<List<Optp>>(jsonOpKey);

                var resDel = OptpBus.DeleteProcessTemplate(lstOptp);

                var status = CommonUtility.ConvertBoolToString01(resDel);
                CommonUtility.InsertLogActivity(lstOptp, UserInf, SystemOpsId, ConstantGeneric.ScreenProcessTemplate, ConstantGeneric.EventDelete, "Delete process template.", status);

                if (resDel)
                {
                    return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
                }

                return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get Machine
        public JsonResult GetMachineJquery(string styleCode, string styleSize, string styleColor,
            string revNo, string opRevNo, string edition)
        {
            try
            {
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColor) || string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(edition))
                {
                    return Json(new List<Optl>(), JsonRequestBehavior.AllowGet);
                }

                var lstMachines = OptlBus.GetMachineJquery(styleCode, styleSize, styleColor, revNo, opRevNo, edition);
                return Json(lstMachines, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get Video Link
        public JsonResult GetVideoLink(string corp, string dept, string fileName)
        {
            try
            {
                var videoLink = VideoServerLink + corp + dept + "&f=" + fileName;

                return Json(videoLink, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get Video Max CommentId
        public JsonResult GetMaxCommentId(string fileId)
        {
            try
            {
                var maxCommentId = FcmtBus.GetMaxCommentId(fileId);

                return Json(maxCommentId, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get Video Comments
        public JsonResult GetVideoComment(string fileId)
        {
            try
            {
                var lstComment = FcmtBus.GetVideoComment(fileId);
                return Json(lstComment, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get Comments
        public JsonResult GetCommentJquery(string fileId)
        {
            try
            {
                var lstComment = FcmdBus.GetVideoComment(fileId);
                return Json(lstComment, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get CommentId
        public JsonResult GetCommentId(string fileId)
        {
            try
            {
                var maxCommentId = FcmdBus.GetCommentId(fileId);

                return Json(maxCommentId, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Get CommentId
        public JsonResult GetTimeVideoById(string fileId, string commentId)
        {
            try
            {
                var timeVideo = FcmdBus.GetTimeVideoById(fileId, commentId);

                return Json(timeVideo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Ha Nguyen
        //Post Comment
        public string PostComment(Fcmd comment)
        {
            try
            {
                var blResultAdd = FcmdBus.AddVideoComment(comment);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(comment, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        //Edit Comment
        public string EditComment(Fcmd comment)
        {
            try
            {
                var blResultAdd = FcmdBus.EditComment(comment);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(comment, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        //Delete Comment
        public string DeleteComment(Fcmd comment)
        {
            try
            {
                var blResultAdd = FcmdBus.DeleteComment(comment);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(comment, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        //Upvote Comment
        public string UpvoteComment(Fcmd comment)
        {
            try
            {
                var blResultAdd = FcmdBus.UpvoteComment(comment);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(comment, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        //Upload Attachments Comment
        public string UploadAttachments(Fcmd comment)
        {
            try
            {
                var blResultAdd = FcmdBus.UploadAttachments(comment);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(comment, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Ha Nguyen
        //Add Video Comment
        public string AddVideoComment(Fcmt videoComment)
        {
            try
            {
                if (videoComment.CommentNote == null)
                {
                    return CommonUtility.ObjectToJson("Please type your comment!");
                }

                var blResultAdd = FcmtBus.AddVideoComment(videoComment);
                if (blResultAdd)
                {
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(videoComment, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add", status);
                }
                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
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

        /// <summary>
        /// Author: Son Cao Nguyen
        /// Date: 25 July 2017
        /// Get list of color master
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMasterColor()
        {
            try
            {
                var arrCode = ColorMasterBus.GetColorMaster().ToArray();
                return Json(arrCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Author: Son Nguyen Cao
        /// Date: 27 July 2017
        /// Get list buyer
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBuyer()
        {
            try
            {
                var objBuyer = new BuyerBus();
                var arrBuyer = objBuyer.GetBuyer().ToArray();
                return Json(arrBuyer, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Author: Son Nguyen Cao
        /// Date: 25 July 2017
        /// Gets the op tool master.
        /// </summary>
        /// <returns>An array Tool master</returns>
        public JsonResult GetOpMachineMaster(string isTool, List<string> lstCategoryId)
        {
            try
            {
                var arrTool = isTool == "1" ? OtmtBus.GetOpTool(lstCategoryId).ToArray() : OtmtBus.GetOpMachine(lstCategoryId).ToArray();

                return Json(arrTool, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetOpMachineMaster2(string isTool, List<string> lstCategoryId)
        {
            try
            {
                var arrTool = new Otmt[] { };
                if (isTool == "1")
                {
                    arrTool = OtmtBus.GetOpTool(lstCategoryId).ToArray();
                }
                else
                {
                    //Get list machine from otmt table
                    var lstMachine = OtmtBus.GetOpMachine(lstCategoryId);
                    //Get list of machine from mcmt table
                    var lstMachine2 = McmtBus.GetMasterCode(ConstantGeneric.MachineType);
                    List<Otmt> newMachine2 = new List<Otmt>();
                    foreach (var mc2 in lstMachine2)
                    {
                        var obj = new Otmt { ItemCode = mc2.SubCode, ItemName = mc2.CodeName + " (Không chọn - No Use) " }; //SON ADD) (2019.08.30) - remark old machine load from t_cm_mcmt table 
                        newMachine2.Add(obj);
                    }
                    //join 2 list of machine
                    lstMachine.AddRange(newMachine2);

                    arrTool = lstMachine.ToArray();
                }

                return Json(arrTool, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Author: Son Nguyen Cao
        /// Date: 25 July 2017
        /// Gets the op color master.
        /// </summary>
        /// <returns>An array color master</returns>
        public JsonResult GetOpColorMaster()
        {
            try
            {
                var arrColorMaster = ColorMasterBus.GetColorMaster().ToArray();
                return Json(arrColorMaster, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetVideo()
        {
            return new VideoDataResult();
        }

        //Get max ops revision
        //Author: Son Nguyen Cao
        [HttpPost]
        public string GetMaxOpRevision(string edition, string styleCode, string styleSize, string styleColor, string revNo)
        {
            try
            {
                var maxOpSeial = OpmtBus.GetMaxOpRevision(edition, styleCode, styleSize, styleColor, revNo, false);
                return maxOpSeial == 0 ? "" : CommonUtility.ObjectToJson(maxOpSeial.ToString("D3"));
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return exc.Message;
            }

        }

        /// <summary>
        /// Author: Son Cao Nguyen
        /// Date: 25 July 2017
        /// Get list of master code
        /// </summary>
        /// <param name="mCode">The m code.</param>
        /// <returns></returns>
        public JsonResult GetMasterCode(string mCode)
        {
            try
            {
                var arrMasterCode = McmtBus.GetMasterCode(mCode).ToArray();
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="masterCode"></param>
        /// <param name="subCode"></param>
        /// <param name="codeDesc"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public JsonResult GetMasterCode2(string masterCode, string subCode, string codeDesc)
        {
            try
            {
                var arrMasterCode = McmtBus.GetMasterCode2(masterCode, subCode, codeDesc).ToArray();
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Author: VitHV
        /// Date: 13 March 2018
        /// Get list of factory ops
        /// </summary>
        /// <returns></returns>
        public JsonResult GetFactory()
        {
            try
            {
                var factories = FactoryBus.GetByTypeAndStatus("P", "OK").ToArray(); // P is for Pungkook's factories
                return Json(factories, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Son Nguyen Cao
        public JsonResult GetCategorysMachineTool(string isMachine)
        {
            try
            {
                var lstCategory = McmtBus.GetCategorysMachineTool(isMachine);
                return Json(lstCategory, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Atuhor: Son Nguyen Cao
        public JsonResult GetMasterCode3(string masterCode, string subCode, string codeDesc, string codeDetail)
        {
            try
            {
                var arrMasterCode = McmtBus.GetMasterCode3(masterCode, subCode, codeDesc, codeDetail).ToArray();
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Gets the name of the op.
        /// </summary>
        /// <param name="languageId">The language identifier.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// <exception cref="System.Exception"></exception>
        public JsonResult GetOpName(string languageId)
        {
            try
            {
                if (string.IsNullOrEmpty(languageId)) return Json(new List<string>(), JsonRequestBehavior.AllowGet);

                var arrOpName = OperationNameBus.GetOpName(languageId);
                return Json(arrOpName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //START ADD) HA 
        //Get Video By Stylecode
        public JsonResult GetVideoByStylecode(string stylecode)
        {
            try
            {
                //if (string.IsNullOrEmpty(stylecode)) return Json(new List<string>(), JsonRequestBehavior.AllowGet);
                var arrVideo = UfmtBus.GetVideoByStylecode(stylecode);

                //Get video link
                for (var i = 0; i < arrVideo.Count; i++)
                {
                    var videoLink = VideoServerLink + arrVideo[i].Corporation + arrVideo[i].Department + "&f=" + arrVideo[i].FileNameSys;
                    arrVideo[i].VideoLink = videoLink;
                }
                return Json(arrVideo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Search Process Name
        public JsonResult GetProcessName(string languageId, string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(languageId)) return Json(new List<string>(), JsonRequestBehavior.AllowGet);

                Session["ssOpName"] = null;
                var arrOpName = new List<OperationName>();

                if (Session["ssOpName"] == null)
                {
                    arrOpName = OperationNameBus.GetOpName(languageId);
                }

                else
                {
                    arrOpName = Session["ssOpName"] as List<OperationName>;
                }

                var result = arrOpName.Where(p => p.OpName.Contains(searchTerm)).Select(p => new { p.OpNameId, p.OpName });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Get Weekly Report
        public JsonResult GetWeeklyReport(string startDate, string finishDate)
        {
            try
            {
                var arr = ActpBus.GetWeeklyReport(startDate, finishDate);
                return Json(arr, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }
        //END ADD) HA 

        //Author: Son Nguyen Cao
        public JsonResult GetOpNameByCode(string languageId, string moduleId, string actionCode, string buyer)
        {
            try
            {
                var arrOpName = OperationNameBus.GetOpNameByCode(languageId, moduleId, actionCode, buyer);
                return Json(arrOpName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        // VitHV
        public JsonResult GetMasterCodeDes(string mCode, string mDes)
        {
            try
            {
                var arrMasterCode = McmtBus.GetMasterCode(mCode, mDes).ToArray();
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Checks the edition aom.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool CheckEditionMes(string edition)
        {
            return edition == ConstantGeneric.EditionMes && !string.IsNullOrEmpty(edition);
        }

        #endregion

        #region Module

        //Author: Son Nguyen Cao
        public JsonResult GetItemLevel(string mainLevel, string levelNo)
        {
            try
            {
                var lstItemLevel = IclmBus.GetItemLevel(mainLevel, levelNo);
                return Json(lstItemLevel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Author: Son Nguyen Cao
        public JsonResult GetModulesLevel(Mrul mrul)
        {
            try
            {
                //START MOD) SON (2019.09.19) - 20 September 2019 - get all module if style doesn't follow any rules
                var lstModulesLevel = new List<Mrul>();

                //Get list module by style group and style sub group
                var listMdlByGroup = MrulBus.GetModulesStlGroupAndSubGroup(mrul.StyleGroup, mrul.StyleSubGroup);

                //If the is no module then get all modules
                lstModulesLevel = listMdlByGroup.Count == 0 ?
                    MrulBus.GetModules(ConstantGeneric.MainLevelSubAssembly, ConstantGeneric.LevelNo) : MrulBus.GetModulesLevel(mrul);

                return Json(lstModulesLevel, JsonRequestBehavior.AllowGet);

                //var lstModulesLevel = MrulBus.GetModulesLevel(mrul);
                //return Json(lstModulesLevel, JsonRequestBehavior.AllowGet);

                //END MOD) SON (2019.09.19) - 20 September 2019 
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetModulesPart()
        {
            try
            {
                var listModulePart = IclmBus.GetItemLevel(ConstantGeneric.MainLevelSubAssembly, "02");
                return Json(listModulePart, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Author: Son Nguyen Cao
        public JsonResult GetModulesListByStyleCode(string styleCode)
        {
            try
            {
                var lstModules = SamtBus.GetModulesByCode(styleCode, string.Empty);
                return Json(lstModules, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Son Nguyen Cao
        public Icmt GetItemCode(string mainLevel, string levelNo, string buyer)
        {
            try
            {
                return IcmtBus.GetItemCode(mainLevel, levelNo, buyer);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return new Icmt();
            }

        }

        /// <summary>
        /// Add item master when adding new module.
        /// </summary>
        /// <param name="buyer"></param>
        /// <returns></returns>
        public bool AddItemMaster(string buyer)
        {
            //Get list of item code with main level are SUB and FGS
            var lstSubLevel = IcmtBus.GetListIcmt(ConstantGeneric.MainLevelSubAssembly, buyer);
            var lstFgsLevel = IcmtBus.GetListIcmt(ConstantGeneric.MainLevelFinalAssembly, buyer);

            //Combine 2 objects icmt
            var lstIcmt = lstSubLevel.Union(lstFgsLevel);

            //Get item level by main level and level no
            var lstIclmSub = IclmBus.GetItemLevel(ConstantGeneric.MainLevelSubAssembly, ConstantGeneric.LevelNo);
            var lstIclmFgs = IclmBus.GetItemLevel(ConstantGeneric.MainLevelFinalAssembly, ConstantGeneric.LevelNo);
            var lstIclm = lstIclmSub.Union(lstIclmFgs);

            //Get item level which hasn't added yet in item master table.
            var newListIclm = lstIclm.Where(iclm => !lstIcmt.Any(icmt => icmt.LevelNo01 == iclm.LevelCode && icmt.MainLevel == iclm.MainLevel));

            if (newListIclm.Count() == 0) return true;

            //Get max item code of main level Sub and Finish Goods Stock
            var maxItemCodeSub = IcmtBus.GetMaxItemCodeModule(ConstantGeneric.MainLevelSubAssembly, buyer);
            var maxItemCodeFgs = IcmtBus.GetMaxItemCodeModule(ConstantGeneric.MainLevelFinalAssembly, buyer);
            var orderItemCodeSub = int.Parse(maxItemCodeSub.Substring(6));
            var ordermaxItemCodeFgs = int.Parse(maxItemCodeFgs.Substring(6));

            var newListIcmt = new List<Icmt>();
            foreach (var iclm in newListIclm)
            {
                var newItemCode = string.Empty;
                //Create new item code
                if (iclm.MainLevel == ConstantGeneric.MainLevelSubAssembly)
                {
                    orderItemCodeSub += 1;
                    newItemCode = iclm.MainLevel + buyer + orderItemCodeSub.ToString("D7");
                }
                else
                {
                    ordermaxItemCodeFgs += 1;
                    newItemCode = iclm.MainLevel + buyer + ordermaxItemCodeFgs.ToString("D7");
                }

                //Check ItemCode is empty or not
                if (string.IsNullOrEmpty(newItemCode)) return false;

                //Create new Item to insert to database.
                var newIcmt = new Icmt
                {
                    ItemCode = newItemCode,
                    ItemName = iclm.LevelDesc,
                    MainLevel = iclm.MainLevel,
                    LevelNo01 = iclm.LevelCode,
                    ItemRegister = UserInf.UserName,
                    RegistryDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Buyer = buyer
                };
                newListIcmt.Add(newIcmt);
            }

            //Insert list of item code.
            return IcmtBus.InsertItemCodeList(newListIcmt);
        }

        public bool AddItemMasterModulePart(string buyer)
        {
            //Get list of item code with main level are SUB and FGS
            var lstSubLevel = IcmtBus.GetListIcmt(ConstantGeneric.MainLevelSubAssembly, buyer);

            //Get item level by main level and level no 02
            var lstIclmSub = IclmBus.GetItemLevel(ConstantGeneric.MainLevelSubAssembly, "02");

            //Get item level which hasn't added yet in item master table.
            var newListIclm = lstIclmSub.Where(iclm => !lstSubLevel.Any(icmt => icmt.LevelNo02 == iclm.LevelCode && icmt.MainLevel == iclm.MainLevel));

            if (newListIclm.Count() == 0) return true;

            //Get max item code of main level Sub and Finish Goods Stock
            var maxItemCodeSub = IcmtBus.GetMaxItemCode(ConstantGeneric.MainLevelSubAssembly, buyer);

            var orderItemCodeSub = int.Parse(maxItemCodeSub.Substring(6));

            //set item code from 101 for part module
            if (orderItemCodeSub < 101) orderItemCodeSub = 100;

            var newListIcmt = new List<Icmt>();
            foreach (var iclm in newListIclm)
            {
                var newItemCode = string.Empty;

                orderItemCodeSub += 1;
                newItemCode = iclm.MainLevel + buyer + orderItemCodeSub.ToString("D7");

                //Create new Item to insert to database.
                var newIcmt = new Icmt
                {
                    ItemCode = newItemCode,
                    ItemName = iclm.LevelDesc,
                    MainLevel = iclm.MainLevel,
                    LevelNo02 = iclm.LevelCode, //MOD) SON - 13/Jun/2019 - change level to keep data
                    ItemRegister = UserInf.UserName,
                    RegistryDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Buyer = buyer
                };
                newListIcmt.Add(newIcmt);
            }

            //Insert list of item code.
            return IcmtBus.InsertItemCodeList(newListIcmt);
        }

        //Author: Son Nguyen Cao
        public JsonResult AddModule(Samt objModule, string mainLevel, List<string> lstModuleLevleCode, string buyer)
        {
            try
            {
                //Chekc role.
                var mdlRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.ModuleMenuId);
                if (mdlRole == null || !CommonMethod.CheckRole(mdlRole.IsAdd))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                var styleCode = objModule.StyleCode;

                //Check register id and stylecode
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(objModule.Registrar))
                    return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

                //Add item master before add new module
                if (!AddItemMaster(buyer)) return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

                var lstItemMaster = new List<Icmt>();

                //Check Final Assembly
                var finalItem = GetItemCode(ConstantGeneric.MainLevelFinalAssembly, ConstantGeneric.LevelNo01, buyer);
                if (finalItem != null) lstItemMaster.Add(finalItem);

                //Get item master according to main level, module level code and buyer               
                foreach (var mdCode in lstModuleLevleCode)
                {
                    var item = IcmtBus.GetItemCode(mainLevel, mdCode, buyer);
                    if (item != null)
                    {
                        lstItemMaster.Add(item);
                    }
                }

                if (lstItemMaster == null) return Json("Cannot find item master in system.", JsonRequestBehavior.AllowGet);

                //Get list module by style code.
                var lstModules = SamtBus.GetModulesByCode(styleCode, string.Empty);

                var maxPartId = SamtBus.GetMaxPartIdBuyer(styleCode);//ADD) SON - 1.1.1 - 30/Mar/2019

                //START ADD - SON) 1/Feb/2021 - get list module color
                var listModuleColors = OpColorBus.GetColour();
                //END ADD - SON) 1/Feb/2021

                var lstTempModules = new List<Samt>();
                foreach (var item in lstItemMaster)
                {
                    //Check module was exists or not.
                    var samt = lstModules.Where(x => x.ModuleId == item.ItemCode).FirstOrDefault();
                    if (samt == null)
                    {
                        //START ADD - SON) 1/Feb/2021 - get list module color
                        //Get module color
                        var moduleColor = listModuleColors.Find(x => x.Module == item.LevelNo01)?.HexaValue;
                        //END ADD - SON) 1/Feb/2021

                        maxPartId++;//ADD) SON - 1.1.1 - 30/Mar/2019
                        var finalAssembly = item.MainLevel == ConstantGeneric.MainLevelFinalAssembly ? "1" : objModule.FinalAssembly;
                        var samtTemp = new Samt()
                        {
                            StyleCode = objModule.StyleCode,
                            ModuleId = item.ItemCode,
                            ModuleName = item.ItemName,
                            Registrar = objModule.Registrar,
                            RegistryDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                            FinalAssembly = finalAssembly,
                            PartId = maxPartId.ToString("D2"),//ADD) SON - 1.1.1 - 30/Mar/2019
                            Color = moduleColor //ADD - SON) 1/Feb/2021
                        };

                        lstTempModules.Add(samtTemp);
                    }
                }

                if (lstTempModules.Count == 0) return Json("There are no modules to add.", JsonRequestBehavior.AllowGet);

                var resAdd = lstTempModules.Count > 0 ? SamtBus.InsertModulesList_New(lstTempModules) : true;

                //Record log add new module           
                var status = CommonUtility.ConvertBoolToString01(resAdd);
                CommonUtility.InsertLogActivity(styleCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new module.", status);

                return resAdd ? Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet) : Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AddModulePart(Samt objModule, string mainLevel, List<string> lstModulePart, string buyer)
        {
            try
            {
                //Chekc role.
                var mdlRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.ModuleMenuId);
                if (mdlRole == null || !CommonMethod.CheckRole(mdlRole.IsAdd))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                var styleCode = objModule.StyleCode;

                //Check register id and stylecode
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(objModule.Registrar))
                    return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

                //Add item master before add new module
                if (!AddItemMasterModulePart(buyer)) return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

                var lstItemMaster = new List<Icmt>();

                //Get item master according to main level, module level code and buyer               
                foreach (var mdCode in lstModulePart)
                {
                    var item = IcmtBus.GetItemCodeByLevel(mainLevel, mdCode, ConstantGeneric.LevelNo2, buyer);
                    if (item != null)
                    {
                        lstItemMaster.Add(item);
                    }
                }

                if (lstItemMaster == null) return Json("Cannot find item master in system.", JsonRequestBehavior.AllowGet);

                //Get list module by style code.
                var lstModules = SamtBus.GetModulesByCode(styleCode, string.Empty);

                var lstTempModules = new List<Samt>();

                var maxPartId = SamtBus.GetMaxPartIdBuyer(styleCode);

                foreach (var item in lstItemMaster)
                {
                    //Check module was exists or not.
                    var samt = lstModules.Where(x => x.ModuleId == item.ItemCode).FirstOrDefault();
                    if (samt == null)
                    {
                        maxPartId++;
                        var samtTemp = new Samt()
                        {
                            StyleCode = objModule.StyleCode,
                            ModuleId = item.ItemCode,
                            ModuleName = item.ItemName,
                            Registrar = objModule.Registrar,
                            RegistryDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                            FinalAssembly = "0",
                            PartId = maxPartId.ToString("D2")
                        };

                        lstTempModules.Add(samtTemp);
                    }
                }

                if (lstTempModules.Count == 0) return Json("There are no Parts to to be added.", JsonRequestBehavior.AllowGet);

                var resAdd = lstTempModules.Count > 0 ? SamtBus.InsertModulesList_New(lstTempModules) : true;

                //Record log add new module           
                var status = CommonUtility.ConvertBoolToString01(resAdd);
                CommonUtility.InsertLogActivity(styleCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new module.", status);

                return resAdd ? Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet) : Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Son Nguyen Cao
        public JsonResult GetModules(GridSettings gridRequest, string styleCode)
        {
            try
            {
                if (string.IsNullOrEmpty(styleCode)) return Json(new Samt(), JsonRequestBehavior.AllowGet);

                int pageIndex = gridRequest.pageIndex;
                int pageSize = gridRequest.pageSize;

                var lstModules = SamtBus.GetModules(styleCode, pageIndex, pageSize);
                return lstModules != null ? Json(lstModules, JsonRequestBehavior.AllowGet) : Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateModuleComment(List<Samt> listModule)
        {
            //Check permission
            var mdlRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.ModuleMenuId);
            if (mdlRole == null || !CommonMethod.CheckRole(mdlRole.IsUpdate))
                return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

            //Update list of module comment
            var resUpd = SamtBus.UpdateModulesCommentList(listModule);

            return resUpd == true ? Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet) : Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
        }

        //Author: Son Nguyen Cao
        public JsonResult DeleteModule()
        {
            try
            {
                //Check permission
                var mdlRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.ModuleMenuId);
                if (mdlRole == null || !CommonMethod.CheckRole(mdlRole.IsDelete))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                var lstSamt = new List<Samt>();
                var jsonOpKey = Request.Form["opKey"];
                var objSamt = JsonConvert.DeserializeObject<Samt>(jsonOpKey);
                lstSamt.Add(objSamt);

                if (lstSamt == null) return Json("Modules is empty.", JsonRequestBehavior.AllowGet);

                //Check module code was exists in operation detail or not
                foreach (var samt in lstSamt)
                {
                    //START ADD) SON - 1.1.1 - 30/Mar/2019 - check module existed in module BOM

                    var listMBOM = MBomBus.GetModulesByModuleId(samt.StyleCode, samt.ModuleId);
                    if (listMBOM.Count != 0) return Json("Cannot delete because this Module/Part existed Module BOM.", JsonRequestBehavior.AllowGet);

                    //START ADD) SON - 1.1.1 - 30/Mar/2019 

                    var lstOpdts = OpdtBus.GetListOpdtsByModuleCode(samt.ModuleId, samt.StyleCode);
                    if (lstOpdts.Count != 0)
                        return Json("Cannot delete because this Module/Part was existing in operation plan.", JsonRequestBehavior.AllowGet);
                }

                //Delete module
                var resDel = SamtBus.DeleteModulesList(lstSamt);

                //Record log delete module.      
                var status = CommonUtility.ConvertBoolToString01(resDel);
                CommonUtility.InsertLogActivity(lstSamt[0].StyleCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventDelete, "Delete module.", status);

                if (resDel)
                {
                    return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
                }

                return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Son Nguyen Cao
        public JsonResult GetListOpdtsByModuleCode(string mdCode, string styleCode)
        {
            try
            {
                var lstOpdts = OpdtBus.GetListOpdtsByModuleCode(mdCode, styleCode);
                return lstOpdts != null ? Json(lstOpdts, JsonRequestBehavior.AllowGet) : Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// This function is cloned from GetListProcessNameDetail function
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetOpnts(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo,
            string opRevNo, string opSerial, string languageId)
        {
            try
            {
                var opnts = await _OpntBus.GetOpnts(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo,
                    opSerial, "", languageId);

                return opnts != null ? Json(new TaskResult<object> { IsSuccess = true, Result = new { opnts, ConstantGeneric.ImageHttpLink } }) :
                    Json(new TaskResult<object> { IsSuccess = false, Log = "opnts is null." });
            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(new TaskResult<object> { IsSuccess = false, Log = ex.Message });
            }
        }

        //Author: Son Nguyen Cao.
        public JsonResult GetListProcessNameDetail(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string languageId)
        {
            try
            {
                var lstOpnts = OpntBus.GetOpNameDetails(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, "", languageId);
                return lstOpnts != null ? Json(lstOpnts, JsonRequestBehavior.AllowGet) : Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SearchProcessName(string q, string page, string languageId)
        {
            try
            {
                if (string.IsNullOrEmpty(q) || string.IsNullOrWhiteSpace(q)) return Json(new { }, JsonRequestBehavior.AllowGet);

                var lstOpnm = OpnmBus.SearchProcessName(q, languageId);

                return lstOpnm != null ? Json(new { items = lstOpnm }, JsonRequestBehavior.AllowGet) : Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //START ADD) HA NGUYEN

        public JsonResult SearchMachineName(string q, string page)
        {
            try
            {
                //if (string.IsNullOrEmpty(q)) return null;
                if (string.IsNullOrEmpty(q) || string.IsNullOrWhiteSpace(q)) return Json(new { }, JsonRequestBehavior.AllowGet);

                var lstOtmt = OtmtBus.SearchMachineName(q);

                return lstOtmt != null ? Json(new { items = lstOtmt }, JsonRequestBehavior.AllowGet) : Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SearchToolId(string q, string page)
        {
            try
            {
                //if (string.IsNullOrEmpty(q)) return null;
                if (string.IsNullOrEmpty(q) || string.IsNullOrWhiteSpace(q)) return Json(new { }, JsonRequestBehavior.AllowGet);

                var lstOtmt = OtmtBus.SearchToolId(q);

                return lstOtmt != null ? Json(new { items = lstOtmt }, JsonRequestBehavior.AllowGet) : Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        //END ADD) HA NGUYEN
        #endregion

        #region Module Revision
        public JsonResult GetModulesMbom(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            try
            {
                //Get list modules which linked with material
                var listMdlMbom = MBomBus.GetModulesMbom(styleCode, styleSize, styleColorSerial, revNo);

                return Json(listMdlMbom, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult InsertLinkedMboms(List<Mboh> listLnkBoms)
        {
            try
            {
                //Generate reference no
                var refNo = string.Empty;
                if (listLnkBoms == null || listLnkBoms.Count == 0) return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

                refNo = listLnkBoms[0].STYLECODE + listLnkBoms[0].STYLESIZE + listLnkBoms[0].STYLECOLORSERIAL + listLnkBoms[0].REVNO + listLnkBoms[0].MODULEID;

                //Get list mbom header
                var listOriginalMboh = MbohBus.GetMbomsHeader(listLnkBoms[0].STYLECODE, listLnkBoms[0].STYLESIZE, listLnkBoms[0].STYLECOLORSERIAL, listLnkBoms[0].REVNO);
                //Create temporary list to add new item if this item doesn't exist in original list
                var tempMboh = new List<Mboh>(); tempMboh.AddRange(listOriginalMboh);
                //Update linked into 0 for each item
                foreach (var item in tempMboh)
                {
                    item.LINKED = "0";
                }

                foreach (var orgMboh in listOriginalMboh)
                {
                    foreach (var newMboh in listLnkBoms)
                    {
                        //Find new mboh in the original list
                        var mboh = tempMboh.Find(x => x.MODULEID == newMboh.MODULEID);
                        if (mboh != null)
                        {
                            //Update linked status
                            mboh.LINKED = newMboh.LINKED;
                        }
                        else
                        {
                            //Update outsource status for new item
                            newMboh.OUTSOURCE = "0";
                            //If Module does not exist then adding to the new list in order to insert
                            tempMboh.Add(newMboh);
                        }
                    }
                }

                //Get new list Mbom header with linked or outsource status is 1 to insert
                var newListMboh = tempMboh.Where(x => x.LINKED == "1" || x.OUTSOURCE == "1").ToList();

                //Delete and insert new list mbom header
                var resIns = MbohBus.DeleteAndInsertMBOMHeader(newListMboh);

                var status = CommonUtility.ConvertBoolToString01(resIns);
                CommonUtility.InsertLogActivity(refNo, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add module revision.", status);

                return resIns ? Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet) : Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.ModuleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Operation Plan Detail

        //Author: Son Nguyen Cao
        public JsonResult UpdateProcessTime(List<Opnt> lstOpnt)
        {

            //Update process time.
            //if (OpntBus.UpdateListProcessTime(lstOpdt))
            //{
            //    return Json("Updated process time", JsonRequestBehavior.AllowGet);
            //}

            //return Json("Something wrong during update process time.", JsonRequestBehavior.AllowGet);

            return Json("This function is not available now.", JsonRequestBehavior.AllowGet);

        }

        //Author: Son Nguyen Cao.
        public JsonResult CheckProcessNameIsStandard(List<Opdt> lstOpdt)
        {
            try
            {
                int i = 1;
                foreach (var opdt in lstOpdt)
                {
                    var orgOpName = opdt.OpNameRef;
                    orgOpName = orgOpName.Substring(orgOpName.LastIndexOf('#') + 1);

                    //Get process name id
                    var arrOpNameId = string.Empty;
                    var strOpName = orgOpName.Replace('+', ',');
                    var arrOpName = strOpName.Split('|');

                    foreach (var opName in arrOpName)
                    {
                        var subOpName = opName.Trim();
                        var objOpnm2 = string.IsNullOrEmpty(subOpName) ? null : OpnmBus.GetOpNameId(subOpName);
                        if (objOpnm2 != null)
                        {
                            if (!string.IsNullOrEmpty(arrOpNameId)) arrOpNameId += "|";
                            arrOpNameId += objOpnm2.OpNameId;
                        }
                        else
                        {
                            return Json(CommonUtility.GetResultContent(ConstantGeneric.Fail, "Process name at row " + i + " does not follow the standard."), JsonRequestBehavior.AllowGet);
                        }
                    }
                    opdt.ArrOpNameId = arrOpNameId;
                    opdt.OpName = strOpName;

                    i++;


                    //var objOpnm = string.IsNullOrEmpty(opdt.OpNameRef) ? null : OpnmBus.GetOpNameId(opdt.OpNameRef.Trim());
                    //if (objOpnm != null)
                    //{
                    //    opdt.OpNameId = objOpnm.OpNameId;
                    //}
                    //else
                    //{
                    //    return Json(CommonUtility.GetResultContent(ConstantGeneric.Fail, "Process name at row " + i + " does not follow the standard."), JsonRequestBehavior.AllowGet);
                    //}
                    //i++;
                }
                return Json(CommonUtility.GetResultContent(ConstantGeneric.Success, lstOpdt), JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Checks the role edition confirmed.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="opMaster">The op master.</param>
        /// <param name="opDetail"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static string CheckRoleEditionConfirmed(string actionRole, Opmt opMaster, Opdt opDetail)
        {
            // Check role
            if (!CommonMethod.CheckRole(actionRole))
                return CommonUtility.ObjectToJson(ConstantGeneric.AlertPermission);

            if (opMaster == null)
            {
                //Check ops confirmed
                if (CommonUtility.CheckOpsMasterConfirmed(opDetail))
                    return CommonUtility.ObjectToJson(ConstantGeneric.AlertOpsConfirmed);
            }
            else
            {
                //Check ops confirmed
                if (CommonUtility.CheckOpsMasterConfirmed(opMaster))
                    return CommonUtility.ObjectToJson(ConstantGeneric.AlertOpsConfirmed);
            }

            return ConstantGeneric.False;
        }

        /// <summary>
        /// Checks the op detail is valid.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private string CheckOpDetailIsValid(Opdt opDetail)
        {
            try
            {
                if (string.IsNullOrEmpty(opDetail.OpSerial.ToString()))
                    return "Operation serial is empty!";

                if (string.IsNullOrEmpty(opDetail.OpName))
                    return "Operation name is empty!";

                //if (string.IsNullOrEmpty(opDetail.JobType))
                //    return "Action process is empty!";

                //if (string.IsNullOrEmpty(opDetail.MachineType))
                //    return "Default machine is empty!";

                //if (string.IsNullOrEmpty(opDetail.OpGroup))
                //    return "Process group is empty!";

                //if (string.IsNullOrEmpty(opDetail.MdCode))
                //    return "Module is empty!";

                if (string.IsNullOrEmpty(opDetail.MaxTime.ToString()))
                    return "Maxtime is empty!";

                if (string.IsNullOrEmpty(opDetail.OpTime.ToString()))
                    return "Process time is empty!";
            }
            catch (Exception exc)
            {
                return exc.Message;
            }

            return ConstantGeneric.True;
        }

        //Get max opserial
        //Author: Son Nguyen Cao
        [HttpPost]
        public string GetMaxOpSerial(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            try
            {
                if (string.IsNullOrEmpty(edition))
                    return CommonUtility.ObjectToJson(ConstantGeneric.Fail);

                var maxOpSeial = OpdtBus.GetMaxOpSerial(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);
                return maxOpSeial == 0 ? "000" : CommonUtility.ObjectToJson(maxOpSeial.ToString("D3"));
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.NoMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return exc.Message;
            }

        }

        // Author: Son Nguyen Cao
        public JsonResult GetOpDetail(GridSettings gridRequest, string styleCode, string styleSize, string styleColor,
            string revNo, string opRevNo, string edition, string languageId)
        {
            try
            {
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize)
                    || string.IsNullOrEmpty(styleColor) || string.IsNullOrEmpty(revNo)
                    || string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(edition))
                    return Json(new List<Opdt>(), JsonRequestBehavior.AllowGet);

                languageId = string.IsNullOrEmpty(languageId) ? "" : languageId;
                var opsDetail = OpdtBus.GetOpDetailByLanguage(styleCode, styleSize, styleColor, revNo, opRevNo, edition, languageId);

                //START ADD) SON - Get list of files
                var lstFile = OpflBus.GetOperationFilesByPlan(styleCode, styleSize, styleColor, revNo, opRevNo, edition);
                foreach (var opdt in opsDetail)
                {
                    var bl = lstFile.Where(o => o.StyleCode == opdt.StyleCode && o.StyleSize == opdt.StyleSize
                                            && (o.StyleColorSerial == opdt.StyleColorSerial || o.StyleColorSerial == "000")
                                            && (o.RevNo == opdt.RevNo || o.RevNo == "000")
                                            && o.OpRevNo == opdt.OpRevNo && o.OpSerial == opdt.OpSerial
                                            && o.Edition == opdt.Edition);

                    if (bl.Count() > 0)
                    {
                        opdt.HasFile = "Y";
                        if (bl.Count() == 1)
                        {
                            opdt.FileNameOpfl = bl.FirstOrDefault().OrgFileName;
                        }
                        else
                        {
                            opdt.HasManyFiles = "Y";
                        }

                    }
                }
                //END ADD) SON 

                //Add process video link.
                var videoLink = ConstantGeneric.VideoProcessHttpLink;
                foreach (var opdt in opsDetail)
                {
                    if (!string.IsNullOrEmpty(opdt.VideoFile))
                    {
                        opdt.VideoOpLink = videoLink + opdt.VideoFile;
                    }

                    if (!string.IsNullOrEmpty(opdt.ImageName))
                    {
                        opdt.ImageLink = OpdtBus.CreateProcessImageLink(opdt.ImageName);
                    }
                }

                var opsDetailQ = opsDetail.AsQueryable();
                if (null != gridRequest.where && gridRequest.where.rules.Length > 0)
                {
                    string strWhere = LinqExtensionsMethod.FilterNullExpression(gridRequest);
                    opsDetailQ = string.IsNullOrEmpty(strWhere) ? opsDetailQ : opsDetailQ.Where(strWhere);

                    strWhere = LinqExtensionsMethod.GetAllStringFiltersExpression(gridRequest);
                    opsDetailQ = string.IsNullOrEmpty(strWhere) ? opsDetailQ : opsDetailQ.Where(strWhere);

                    opsDetail = opsDetailQ.ToList();
                }

                //opsDetailQ = opsDetailQ?.OrderBy(gridRequest.sortColumn, gridRequest.sortOrder);

                if (gridRequest.sortColumn.Split(' ')[0] == "ModuleName")
                {
                    //var lstOpDetail = (from t in opsDetail select t).OrderBy(x => x.ModuleName);
                    //Sorting process detail
                    var lstOpDetail = OpdtBus.SortListProcess(opsDetail, true);
                    return Json(lstOpDetail, JsonRequestBehavior.AllowGet);
                }

                //Sorting process by group
                opsDetail = OpdtBus.SortListProcess(opsDetail, false);

                return Json(opsDetail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Get object op detail by key code
        //Author: Son Nguyen Cao
        public JsonResult GetOpDetailByCode(Opdt opDetail)
        {
            try
            {
                //Check key code of process detail is null or not.
                if (string.IsNullOrEmpty(opDetail.StyleCode) || string.IsNullOrEmpty(opDetail.StyleSize)
                    || string.IsNullOrEmpty(opDetail.StyleColorSerial) || string.IsNullOrEmpty(opDetail.RevNo)
                    || string.IsNullOrEmpty(opDetail.OpSerial.ToString()) || string.IsNullOrEmpty(opDetail.Edition))
                    return Json(new Opdt(), JsonRequestBehavior.AllowGet);

                var opsDetail = OpdtBus.GetOpDetailByCode(opDetail).FirstOrDefault();

                //Add process video link.
                if (!string.IsNullOrEmpty(opsDetail?.VideoFile))
                {
                    if (opsDetail.VideoFile.Contains("pkfile"))
                    {
                        var videoLink = HttpContext.Request.Url?.Authority;
                        if (videoLink != null)
                        {
                            var l = opsDetail.VideoFile.Replace("~", "").Replace("/", "\\");
                            opsDetail.VideoOpLink = l;
                        }
                    }
                    else
                    {
                        var videoLink = ConstantGeneric.VideoProcessHttpLink;
                        opsDetail.VideoOpLink = videoLink + opsDetail.VideoFile;
                    }
                }

                //Create Image link
                if (!string.IsNullOrEmpty(opsDetail.ImageName))
                {
                    opsDetail.ImageLink = OpdtBus.CreateProcessImageLink(opsDetail.ImageName);
                }

                return Json(opsDetail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Add new process
        //Author: Son Nguyen Cao
        [HttpPost]
        public string AddNewProcess(Opdt opDetail, List<Optl> lstOpMachine, List<Optl> lstOpTool, List<Opnt> lstOpnt)
        {
            try
            {
                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(opDetail.Edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? SystemMesId : SystemOpsId;
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);
                if (objUserRole == null) return ConstantGeneric.AlertPermission;

                var checkConfrimed = CheckRoleEditionConfirmed(objUserRole.IsAdd, null, opDetail);
                if (checkConfrimed != ConstantGeneric.False)
                    return checkConfrimed;

                //Check operation plan.
                var checkValideOpDetail = CheckOpDetailIsValid(opDetail);
                if (checkValideOpDetail != ConstantGeneric.True)
                    return CommonUtility.ObjectToJson(checkValideOpDetail);

                var srcTempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);
                //var imgFileName = opDetail.ImageName;
                var tempImagePath = srcTempFolder + opDetail.ImageName;
                var tempVideoPath = srcTempFolder + opDetail.VideoFile;

                //Add process
                var blResultAdd = OpdtBus.AddNewProccess(opDetail, lstOpMachine, lstOpTool, lstOpnt);
                if (blResultAdd)
                {
                    //Count processes, machines, wokers and calculate optime. 
                    var objOpmt = CommonUtility.CountOperationPlan(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo);
                    //Update operation master.
                    OpmtBus.UpdateOpMaster(objOpmt);

                    //Move image file from temporary folder after adding new process into database
                    if (System.IO.File.Exists(tempImagePath))
                    {
                        //var proImgDir = Server.MapPath(ConstantGeneric.ProcessImageDirectory);
                        var proImgDir = ConstantGeneric.ProcessImageDirectory;
                        MoveFile(opDetail.ImageName, srcTempFolder, proImgDir);
                    }

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

                            var fileName = opDetail.VideoFile; ;

                            //Create folder on FTP server.
                            FtpInfoBus.CreateFTPDirectory(ftpInfo, "");
                            //Create FTP file path string to upload Jig file to FTP
                            var ftpFilePath = ftpInfo.FtpRoot + fileName;
                            var statusUpload = FtpInfoBus.UploadFileToFtpFromSource(tempVideoPath, ftpFilePath, ftpInfo);

                            //Delete file after upload to FTP server.
                            System.IO.File.Delete(tempVideoPath);
                        }
                    }

                    //Record log add new process.      
                    var status = CommonUtility.ConvertBoolToString01(blResultAdd);
                    CommonUtility.InsertLogActivity(opDetail, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new process.", status);
                }

                //Delete temporary image and video after upload to FTP server.
                DeleteLocalProcessFile(tempVideoPath);
                DeleteLocalProcessFile(tempVideoPath);

                return blResultAdd ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Cannot add new process!");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        [HttpPost]
        public string UpdateOpName(Opmt opMaster, List<Opdt> lstOpdt, string languageId)
        {
            try
            {
                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(opMaster.Edition);
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, menuId);

                //Check ops confirmed
                var checkConfrimed = CheckRoleEditionConfirmed(objUserRole.IsUpdate, opMaster, null);
                if (checkConfrimed != ConstantGeneric.False)
                    return checkConfrimed;

                //Check key code ops master
                if (!CommonMethod.CheckOpMasterKeyCodeValid(opMaster.StyleCode, opMaster.StyleSize, opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo))
                    return CommonUtility.ObjectToJson("Please check ops master key code!");

                if (lstOpdt != null)
                {
                    //Check key code op detail.
                    foreach (var opdt in lstOpdt)
                    {
                        if (!CommonMethod.CheckKeyCodeOpDetailValid(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo, opdt.OpSerial.ToString()))
                            return CommonUtility.ObjectToJson("Please check ops detail key code!");

                        //Check operation name is null or not
                        if (string.IsNullOrWhiteSpace(opdt.OpNameLan))
                        {
                            var mes = "Cannot change language becuase processes name do not use standard name.";
                            return CommonUtility.ObjectToJson(mes);
                        }
                    }
                }

                //Update operation name.
                var resUpdate = OpdtBus.UpdateOpName(opMaster, lstOpdt, languageId);
                if (resUpdate)
                {
                    //Record log update process name.      
                    var status = CommonUtility.ConvertBoolToString01(resUpdate);
                    CommonUtility.InsertLogActivity(opMaster, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Update operation plan language.", status);

                    return CommonUtility.ObjectToJson(ConstantGeneric.Success);
                }

                return CommonUtility.ObjectToJson(ConstantGeneric.Fail);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Update object ops detail list of tool linking
        //Author: Son Nguyen Cao
        public string UpdateOpDetail(Opdt opDetail, List<Optl> lstMachine, List<Optl> lstTool, List<Opnt> lstOpnt)
        {
            try
            {
                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(opDetail.Edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? SystemMesId : SystemOpsId;
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);

                if (objUserRole == null) return CommonUtility.ObjectToJson("Your role cannot update this edition");

                //Check ops confirmed
                var checkConfrimed = CheckRoleEditionConfirmed(objUserRole.IsUpdate, null, opDetail);
                if (checkConfrimed != ConstantGeneric.False)
                    return checkConfrimed;

                //Check op detail key code.
                if (!CommonMethod.CheckKeyCodeOpDetailValid(opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial,
                    opDetail.RevNo, opDetail.OpRevNo, opDetail.OpSerial.ToString()))
                    return CommonUtility.ObjectToJson("Please check ops detail key code!");

                var srcTempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);

                //var imgFileName = opDetail.ImageName;
                var tempImagePath = srcTempFolder + opDetail.ImageName;
                var tempVideoPath = srcTempFolder + opDetail.VideoFile;

                //update process
                var blResultUpdate = OpdtBus.UpdateOpDetail(opDetail, lstMachine, lstTool, lstOpnt);

                //Record log update process.      
                var status = CommonUtility.ConvertBoolToString01(blResultUpdate);
                CommonUtility.InsertLogActivity(opDetail, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventEdit, "Update process.", status);

                if (blResultUpdate)
                {
                    //Count processes, machines, wokers and calculate optime. 
                    var objOpmt = CommonUtility.CountOperationPlan(opDetail.Edition, opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial, opDetail.RevNo, opDetail.OpRevNo);
                    //Update operation master.
                    OpmtBus.UpdateOpMaster(objOpmt);

                    //Move temporary image.
                    if (System.IO.File.Exists(tempImagePath))
                    {
                        //Move image file from temporary folder after adding new process into database
                        //var proImgDir = Server.MapPath(ConstantGeneric.ProcessImageDirectory);
                        var proImgDir = ConstantGeneric.ProcessImageDirectory;
                        MoveFile(opDetail.ImageName, srcTempFolder, proImgDir);
                    }

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

                            var fileName = opDetail.VideoFile;

                            //Create folder on FTP server.
                            FtpInfoBus.CreateFTPDirectory(ftpInfo, "");
                            //Create FTP file path string to upload Jig file to FTP
                            var ftpFilePath = ftpInfo.FtpRoot + fileName;
                            FtpInfoBus.UploadFileToFtpFromSource(tempVideoPath, ftpFilePath, ftpInfo);

                        }
                    }
                }

                //Delete temporary image and video.
                DeleteLocalProcessFile(tempVideoPath);
                DeleteLocalProcessFile(tempImagePath);

                return blResultUpdate ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Cannot upate process!");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Delete file in local folder
        //Author: Son Nguyen Cao
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

        //Delete list of Ops detail
        //Author: Son Nguyen Cao
        [HttpPost]
        public string DeleteOpsDetail(List<Opdt> lstOpDetail)
        {
            try
            {
                var deletedOpdt = lstOpDetail[0];

                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(deletedOpdt.Edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? SystemMesId : SystemOpsId;
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);
                if (objUserRole == null) return ConstantGeneric.AlertPermission;

                var checkConfrimed = CheckRoleEditionConfirmed(objUserRole.IsDelete, null, deletedOpdt);

                if (checkConfrimed != ConstantGeneric.False)
                    return checkConfrimed;

                foreach (var opDetail in lstOpDetail)
                {
                    if (!CommonUtility.CheckKeyCodeOpDetail(opDetail.StyleCode, opDetail.StyleSize, opDetail.StyleColorSerial,
                        opDetail.StyleColorSerial, opDetail.OpRevNo, opDetail.OpSerial.ToString()))
                    {
                        return CommonUtility.ObjectToJson("Pleas check key code of operation plan detail.");
                    }
                }

                var resDelete = OpdtBus.DeleteOpsDetailAndToolLinking(lstOpDetail);

                // Record log delete process.
                var status = CommonUtility.ConvertBoolToString01(resDelete);
                CommonUtility.InsertLogActivity(lstOpDetail[0], UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventDelete, "Delete process.", status);

                if (!resDelete) return CommonUtility.ObjectToJson("Cannot delete process.");

                //Count processes, machines, wokers and calculate optime. 
                var objOpmt = CommonUtility.CountOperationPlan(deletedOpdt.Edition, deletedOpdt.StyleCode, deletedOpdt.StyleSize, deletedOpdt.StyleColorSerial, deletedOpdt.RevNo, deletedOpdt.OpRevNo);
                //Update operation master.
                OpmtBus.UpdateOpMaster(objOpmt);

                return CommonUtility.ObjectToJson(ConstantGeneric.Success);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        //Author: Son Nguyen Cao
        public JsonResult GetFiles(string styleCode, string styleSize, string styleColorSerial, string revNo, string uploadCode, string styleFile, string styleFileDesc, string opRevNo, string opSerial, string edition)
        {
            try
            {
                if (string.IsNullOrEmpty(styleFile) || string.IsNullOrEmpty(styleFileDesc))
                    return Json(new FileSd(), JsonRequestBehavior.AllowGet);

                var objFileSd = new FileSd()
                {
                    StyleCode = styleCode,
                    StyleSize = styleSize,
                    StyleColorSerial = styleColorSerial,
                    RevNo = revNo,
                    UploadCode = uploadCode
                };

                //Get list operation files - OPFL table for check linking
                var opfl = CreateObjectOpfl(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, uploadCode, "", "", "", "", "", "");
                var opFiles = OpflBus.GetOperationFiles(opfl);

                //Get file from PDM - FILE table
                var lstFiles = FileSdBus.GetFiles(objFileSd, styleFile, styleFileDesc);

                //Check uploaded files
                foreach (var file in lstFiles)
                {
                    foreach (var opFile in opFiles)
                    {
                        if (file.StyleCode == opFile.StyleCode && file.StyleSize == opFile.StyleSize && file.StyleColorSerial == opFile.StyleColorSerial
                            && file.RevNo == opFile.RevNo && file.UploadCode == opFile.UploadCode && file.AmendNo == opFile.AmendNo)
                        {
                            //Remark uploaded file.
                            file.Linked = "1";

                        }
                    }

                }

                return Json(lstFiles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Author: Son Nguyen Cao.
        public JsonResult GetOpFiles(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string edition, string uploadCode)
        {
            try
            {
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) ||
                    string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(opSerial) || string.IsNullOrEmpty(edition))
                    return Json("Key code of process file is empty.", JsonRequestBehavior.AllowGet); ;

                //Get list machine and jig files
                var opfl = CreateObjectOpfl(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, uploadCode, "", "", "", "", "", "");
                var opFiles = OpflBus.GetOperationFiles(opfl);

                return Json(opFiles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Author: Son Nguyen Cao
        public JsonResult GetProcessNameTemplate(string actionCode, string lanId)
        {
            try
            {
                if (string.IsNullOrEmpty(actionCode) || string.IsNullOrEmpty(lanId))
                    return Json(new List<Onam>(), JsonRequestBehavior.AllowGet);

                var lstProNameTemp = OnamBus.GetProcessTempalte(actionCode, lanId);

                return Json(lstProNameTemp, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Son Nguyen Cao
        public ActionResult DownLoadFiles()
        {
            try
            {
                FileSd sdFile = (FileSd)JsonConvert.DeserializeObject(Request["sdFile"], typeof(FileSd));
                Opdt objOpdt = (Opdt)JsonConvert.DeserializeObject(Request["objOpdt"], typeof(Opdt));

                //Get http link.
                var ftpInfo = FtpInfoBus.GetFtpInfo(ConstantGeneric.FtpAppTypePkMain);

                var styleCode = sdFile.StyleCode;
                var styleSize = sdFile.StyleSize;
                var styleColorSerial = sdFile.StyleColorSerial.PadLeft(3, '0');
                var revNo = sdFile.RevNo;
                var uploadCode = sdFile.UploadCode;
                var amendNo = sdFile.AmendNo;
                var sourceFile = sdFile.SourceFile;

                var opRevNo = objOpdt.OpRevNo;
                var edition = objOpdt.Edition;
                var opSerial = objOpdt.OpSerial.ToString("D3");
                var isOpFile = sdFile.IsOpFile;

                var subFolderFtp = string.Empty;
                var ftpUrl = string.Empty;
                if (isOpFile == "1" || sourceFile == ConstantGeneric.SourceUpload)
                {
                    subFolderFtp = CommonUtility.CreateStringSubFolder2(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition);

                    ftpUrl = ftpInfo.FtpLink + ftpInfo.FtpFolder + "/" + subFolderFtp + sdFile.RemoteFile;
                    //if (!CommonUtility.CheckExistFtpFile(ftpUrl, ftpInfo.FtpUser, ftpInfo.FtpPass))
                    //{
                    //    ftpUrl = ftpInfo.FtpLink + ftpInfo.FtpFolder + "/" + subFolderFtp + ConstantGeneric.JigFile + "/" + sdFile.FileName;
                    //}
                }
                else
                {
                    //START ADD) SON - 2019/Jan/11
                    //Update status USED for file after downding this file.
                    FileSdBus.UpdateUsedStatus(styleCode, styleSize, styleColorSerial, revNo, uploadCode, amendNo, "Y");
                    //START ADD) SON - 2019/Jan/11

                    subFolderFtp = CommonUtility.CreateSubFolderDmsFiles(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial);
                    ftpUrl = ftpInfo.FtpLink + ftpInfo.FtpFolder + "/" + subFolderFtp + sdFile.RemoteFile;
                }

                var objResult = new ResultContent();

                if (CommonUtility.CheckExistFtpFile(ftpUrl, ftpInfo.FtpUser, ftpInfo.FtpPass))
                {
                    DownloadFileFromFtp(ftpInfo, ftpUrl, sdFile.FileName);
                }

                return View();

            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);

                return View();
            }
        }

        /// <summary>
        /// Download file from FTP
        /// </summary>
        /// <param name="ftpInfo"></param>
        /// <param name="ftpUrl"></param>
        /// <param name="orgFileName"></param>
        /// Author: Son Nguyen Cao
        public void DownloadFileFromFtp(FtpInfo ftpInfo, string ftpUrl, string orgFileName)
        {
            try
            {

                // Get the object used to communicate with the server.  
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // This example assumes the FTP site uses anonymous logon.  
                request.Credentials = new NetworkCredential(ftpInfo.FtpUser, ftpInfo.FtpPass);

                request.UsePassive = true;
                request.UseBinary = true;
                request.EnableSsl = false;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    StreamReader reader = new StreamReader(responseStream);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        //Download the File.
                        response.GetResponseStream()?.CopyTo(stream);
                        Response.AddHeader("content-disposition", "attachment;filename=\"" + orgFileName + "\"");
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        Response.BinaryWrite(stream.ToArray());
                        Response.End();
                    }

                    reader.Close();
                }
                response.Close();

            }
            catch (WebException ex)
            {
                throw new Exception((ex.Response as FtpWebResponse)?.StatusDescription);
            }
        }

        #region Process File and Video
        //Author: Son Nguyen Cao
        public List<FileSd> GetFilesNameFromFtp(FtpInfo ftpInfo, string subFolderPath, string fileNote)
        {
            try
            {
                var ftpFolderPath = ftpInfo.FtpRoot + subFolderPath;
                //Create FTP Request.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFolderPath);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(ftpInfo.FtpUser, ftpInfo.FtpPass);
                request.UsePassive = true;
                request.UseBinary = true;
                request.EnableSsl = false;

                //Fetch the Response and read it using StreamReader.
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                List<string> entries = new List<string>();
                //var lstFilesName = new List<string>();
                var lstFilesName = new List<FileSd>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    //Read the Response as String and split using New Line character.
                    entries = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                response.Close();

                foreach (string entry in entries)
                {
                    // Windows FTP Server Response Format
                    // DateCreated    IsDirectory    Name
                    string data = entry;

                    // Remove date                   
                    data = data.Remove(0, 24);

                    // Remove <DIR>
                    string dir = data.Substring(0, 5);
                    bool isDirectory = dir.Equals("<dir>", StringComparison.InvariantCultureIgnoreCase);
                    data = data.Remove(0, 5);
                    data = data.Remove(0, 10);

                    // Parse name
                    string name = data;
                    if (!isDirectory)
                    {
                        var sdFile = new FileSd { FileNote = fileNote, FileName = name };
                        lstFilesName.Add(sdFile);
                    }
                }

                return lstFilesName;
            }
            catch (Exception)
            {
                return new List<FileSd>();
            }
        }

        //Author: Son Nguyen Cao
        [HttpPost]
        public string LinkFilesToPdm(List<FileSd> lstSdFile, Opdt objOpdt)
        {
            try
            {
                var ftpInfo = FtpInfoBus.GetFtpInfo();
                var opRevNo = objOpdt.OpRevNo;
                var opSerial = objOpdt.OpSerial.ToString("D3");
                var edition = objOpdt.Edition;
                var resLink = false;

                //Saving each file from Ftp to specifice folder.
                foreach (var sdFile in lstSdFile)
                {
                    var styleCode = sdFile.StyleCode;
                    var styleSize = sdFile.StyleSize;
                    var styleColorSerial = sdFile.StyleColorSerial.PadLeft(3, '0');
                    var revNo = sdFile.RevNo;
                    var uploadCode = sdFile.UploadCode;
                    var amendNo = sdFile.AmendNo;
                    var orgFileName = sdFile.FileName;
                    var sysFileName = sdFile.RemoteFile;

                    //Insert process file to database.
                    var opfl = CreateObjectOpfl(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, uploadCode, amendNo, UserInf.UserName, sysFileName, orgFileName, ConstantGeneric.SourcePlm, "");
                    resLink = OpflBus.InsertProcessFile(opfl);
                    if (!resLink)
                    {
                        return CommonUtility.ObjectToJson(ConstantGeneric.Fail);
                    }
                }

                // Record log link file to PDM.
                var status = CommonUtility.ConvertBoolToString01(resLink);
                CommonUtility.InsertLogActivity(objOpdt, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Link file from PDM.", status);

                return CommonUtility.ObjectToJson(ConstantGeneric.Success);
            }
            catch (Exception ex)
            {
                return CommonUtility.ObjectToJson(ex.Message);
            }
        }

        //Author: Son Nguyen Cao.
        public Opfl CreateObjectOpfl(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string edition, string uploadCode, string amendNo, string register, string sysFileName, string orgFileName, string sourceFile, string refLink)
        {
            var opfl = new Opfl()
            {
                StyleCode = styleCode,
                StyleSize = styleSize,
                StyleColorSerial = styleColorSerial,
                RevNo = revNo,
                OpRevNo = opRevNo,
                OpSerial = int.Parse(opSerial),
                Edition = edition,
                UploadCode = uploadCode,
                AmendNo = amendNo,
                Register = register,
                SysFileName = sysFileName,
                OrgFileName = orgFileName,
                SourceFile = sourceFile,
                RefLink = refLink
            };

            return opfl;
        }

        //Author Son Nguyen Cao
        public JsonResult RemoveFilesLinking(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string edition, string uploadCode, string amendNo)
        {
            try
            {
                //Check key code of process file.
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColorSerial) ||
                    string.IsNullOrEmpty(revNo) || string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(opSerial) ||
                    string.IsNullOrEmpty(edition) || string.IsNullOrEmpty(uploadCode) || string.IsNullOrEmpty(amendNo))
                    return Json("Key code of linked file is empty.", JsonRequestBehavior.AllowGet);

                var blresuslt = OpflBus.DeleteFilesLinking(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, uploadCode, amendNo);

                // Record log add new operation master.
                var status = CommonUtility.ConvertBoolToString01(blresuslt);
                var refCode = styleCode + styleSize + styleColorSerial + revNo + opRevNo + opSerial + edition + uploadCode + amendNo;
                CommonUtility.InsertLogActivity(refCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new operation plan.", status);

                return blresuslt ? Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet) : Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Author: Son Nguyen Cao.
        public JsonResult GetVideosLink(GridSettings gridRequest, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            try
            {
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize)
                    || string.IsNullOrEmpty(styleColorSerial) || string.IsNullOrEmpty(revNo))
                    return Json("Key code of style is empty.", JsonRequestBehavior.AllowGet);
                var videoServerLink = ConstantGeneric.VideoServerLink;

                var lstVideoLinks = UfmtBus.GetVideoLinks(styleCode, styleSize, styleColorSerial, revNo, videoServerLink);

                if (null != gridRequest.where && gridRequest.where.rules.Length > 0)
                {
                    var lstVideoLinksQ = lstVideoLinks.AsQueryable();

                    string strWhere = LinqExtensionsMethod.FilterNullExpression(gridRequest);
                    lstVideoLinksQ = string.IsNullOrEmpty(strWhere) ? lstVideoLinksQ : lstVideoLinksQ.Where(strWhere);

                    strWhere = LinqExtensionsMethod.GetAllStringFiltersExpression(gridRequest);
                    lstVideoLinksQ = string.IsNullOrEmpty(strWhere) ? lstVideoLinksQ : lstVideoLinksQ.Where(strWhere);

                    lstVideoLinks = lstVideoLinksQ.ToList();
                }

                return Json(lstVideoLinks, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Son Nguyen Cao.
        public JsonResult DeleteVideo(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial, string edition)
        {
            try
            {
                var opdt = new Opdt()
                {
                    StyleCode = styleCode,
                    StyleSize = styleSize,
                    StyleColorSerial = styleColorSerial,
                    RevNo = revNo,
                    OpRevNo = opRevNo,
                    OpSerial = int.Parse(opSerial),
                    Edition = edition
                };
                //Update process video file.
                var blresuslt = OpdtBus.UpdateVideoName(opdt);
                // Record log add new operation master.
                var status = CommonUtility.ConvertBoolToString01(blresuslt);
                var refCode = styleCode + styleSize + styleColorSerial + revNo + opRevNo + opSerial;
                CommonUtility.InsertLogActivity(refCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventDelete, "Delete process video.", status);

                return blresuslt ? Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet) : Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Add reference video link
        public JsonResult AddReferencVideoLink(string styleCode, string styleSize, string styleColor, string revNo, string opRevNo, string opSerial, string edition, string refLink, string refVideoName)
        {
            try
            {
                var uploadCode = ConstantGeneric.SubCodePkVideo;
                //Get max amend no
                var amendNo = OpflBus.GetMaxAmendNoOpFile(styleCode, styleSize, styleColor, revNo, opRevNo, opSerial, edition, uploadCode);

                //Insert process file to database.
                var opfl = CreateObjectOpfl(styleCode, styleSize, styleColor, revNo, opRevNo, opSerial, edition, uploadCode, amendNo, UserInf.UserName, "", refVideoName, ConstantGeneric.SourceVideoPk, refLink);
                var resLink = OpflBus.InsertProcessFile(opfl);

                // Record log add new operation master.
                var status = CommonUtility.ConvertBoolToString01(resLink);
                var refCode = styleCode + styleSize + styleColor + revNo + opRevNo + opSerial + uploadCode + amendNo;
                CommonUtility.InsertLogActivity(refCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add reference video link.", status);

                if (!resLink)
                {
                    return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
                }

                return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //START ADD) SON (2019.08.29) - 29 August 2019
        public JsonResult GetPaitingTimeRange(string paintingType, string materialType)
        {
            try
            {
                var listPaTime = PadtBus.GetPaintingTimeRange(paintingType, materialType);
                return Json(listPaTime, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        //START ADD) SON (2019.08.29) - 29 August 2019

        #endregion

        #endregion

        #region Line Balancing

        /// <summary>
        /// Export Line Balancing data to excel.
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public JsonResult ExportBalancingToExcel(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition, string languageId)
        {
            try
            {
                //Get list of process
                var lstOpdt = OpdtBus.GetOpDetailByLanguage(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);

                var startRow = 42;
                var endRow = 42;

                const string topLeft = "D41";
                string bottomRight = "";
                const string graphTitle = "Line Balancing";
                const string xAxis = "Process Name ";
                const string yAxis = "Process Time";
                var fileName = "line-balancing.xlsx";
                var tempExcelPath = Server.MapPath(ConstantGeneric.LineBalacingTemplateFolder) + "Line-Balancing.xlsx";
                var expExcel = Server.MapPath(ConstantGeneric.ExportingFolder) + fileName;

                //Check file is exists and delete it
                if (System.IO.File.Exists(expExcel))
                {
                    System.IO.File.Delete(expExcel);
                }

                //Creae an Excel application instance
                Excel.Application excelApp = new Excel.Application();

                //Create an Excel workbook instance and open it from the predefined location
                Excel.Workbook excelWorkBook = excelApp.Workbooks.Open(tempExcelPath);

                //Add a new worksheet to workbook with the Datatable name
                //Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                //excelWorkSheet.Name = "Line Balancing";

                Excel.Worksheet excelWorkSheet = excelWorkBook.Worksheets[1];

                //Set value for style
                excelWorkSheet.Cells[5, 4] = styleCode;
                excelWorkSheet.Cells[6, 4] = styleSize;
                excelWorkSheet.Cells[7, 4] = styleColorSerial;
                excelWorkSheet.Cells[8, 4] = revNo;
                excelWorkSheet.Cells[9, 4] = opRevNo;

                var lastRow = 0;
                //Create data for exporting excel
                for (int j = 0; j < lstOpdt.Count; j++) //Loop for row.
                {
                    excelWorkSheet.Cells[j + startRow, 2] = lstOpdt[j].OpSerial;
                    excelWorkSheet.Cells[j + startRow, 3] = lstOpdt[j].OpNum;
                    excelWorkSheet.Cells[j + startRow, 4] = lstOpdt[j].OpName;
                    excelWorkSheet.Cells[j + startRow, 5] = lstOpdt[j].OpTime;
                    excelWorkSheet.Cells[j + startRow, 6] = lstOpdt[j].OpPrice;
                    excelWorkSheet.Cells[j + startRow, 7] = lstOpdt[j].FactoryName;
                    excelWorkSheet.Cells[j + startRow, 8] = lstOpdt[j].ManCount;
                    excelWorkSheet.Cells[j + startRow, 9] = lstOpdt[j].MachineType;
                    excelWorkSheet.Cells[j + startRow, 10] = lstOpdt[j].MachineCount;
                    excelWorkSheet.Cells[j + startRow, 11] = lstOpdt[j].OfferOpPrice;
                    excelWorkSheet.Cells[j + startRow, 12] = lstOpdt[j].MaxTime;
                    excelWorkSheet.Cells[j + startRow, 13] = lstOpdt[j].BenchmarkTime;

                    lastRow = j;
                }
                endRow = startRow + lastRow;
                bottomRight = "E" + endRow; //End of row.

                // Add chart.
                var charts = excelWorkSheet.ChartObjects() as Excel.ChartObjects;
                var chartObject = charts.Add(10, 170, 900, 400) as Excel.ChartObject;

                var chart = chartObject.Chart;

                // Set chart range.
                var range = excelWorkSheet.get_Range(topLeft, bottomRight);
                chart.SetSourceData(range);

                // Set chart properties.
                //chart.ChartType = Excel.XlChartType.xlLine;
                chart.ChartType = Excel.XlChartType.xlColumnClustered;
                chart.ChartWizard(Source: range, Title: graphTitle, CategoryTitle: xAxis, ValueTitle: yAxis);

                //Color boder of cell
                var borderRange = excelWorkSheet.get_Range("B" + startRow, "M" + endRow);
                borderRange.Borders[Excel.XlBordersIndex.xlEdgeBottom].Color = Color.Black.ToArgb();
                borderRange.Borders[Excel.XlBordersIndex.xlEdgeLeft].Color = Color.Black.ToArgb();
                borderRange.Borders[Excel.XlBordersIndex.xlEdgeRight].Color = Color.Black.ToArgb();
                borderRange.Borders[Excel.XlBordersIndex.xlEdgeTop].Color = Color.Black.ToArgb();
                borderRange.Borders[Excel.XlBordersIndex.xlInsideHorizontal].Color = Color.Black.ToArgb();
                borderRange.Borders[Excel.XlBordersIndex.xlInsideVertical].Color = Color.Black.ToArgb();

                excelApp.DisplayAlerts = false; //Disable alert

                excelWorkBook.SaveAs(expExcel);
                excelWorkBook.Close();
                excelWorkBook = null;
                excelApp.Quit();
                excelApp = null;

                DeallocateObject(excelWorkSheet);
                DeallocateObject(excelWorkBook);
                DeallocateObject(excelApp);

                return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Release Object
        private static void DeallocateObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal
                   .ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Console.WriteLine("Exception Occurred while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        protected void DownLoadFile(string fileName)
        {
            try
            {
                string sPath = Server.MapPath(ConstantGeneric.ExportingFolder) + fileName;

                Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
                Response.TransmitFile(sPath);
                Response.End();
            }
            catch (Exception)
            {
            }
        }

        //Author: Son Nguyen Cao
        public string UpdateOptimeBalancing(List<Opdt> lstOpDetail)
        {
            try
            {
                var balRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.OpBalanceMenuId);
                //Check ops confirmed
                var checkConfrimed = CheckRoleEditionConfirmed(balRole.IsUpdate, null, lstOpDetail[0]);
                if (checkConfrimed != ConstantGeneric.False)
                    return checkConfrimed;

                if (lstOpDetail == null || !lstOpDetail.Any())
                    CommonUtility.ObjectToJson("List of operation detail is empty.");

                //Check key code ops master
                foreach (var opdt in lstOpDetail)
                {
                    if (!CommonMethod.CheckKeyCodeOpDetailValid(opdt.StyleCode, opdt.StyleSize, opdt.StyleColorSerial, opdt.RevNo, opdt.OpRevNo, opdt.OpSerial.ToString()))
                        return CommonUtility.ObjectToJson("Please check ops detail key code!");
                }
                //Update optime blancing
                var resUpdate = OpdtBus.UpdateOpTimeBalancing(lstOpDetail);

                //Record log update time balancing.      
                var status = CommonUtility.ConvertBoolToString01(resUpdate);
                CommonUtility.InsertLogActivity(lstOpDetail[0], UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventEdit, "Update line balancing.", status);

                if (resUpdate)
                {
                    return CommonUtility.ObjectToJson(ConstantGeneric.Success);
                }

                return CommonUtility.ObjectToJson(ConstantGeneric.Fail);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        #endregion

        #region Register new OPS

        /// <summary>
        /// Author: Son Cao Nguyen
        /// Date: 25 July 2017
        /// Get list of Opearation Plan master
        /// </summary>
        /// <param name="gridRequest">The grid request.</param>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <returns></returns>
        public JsonResult GetOpMaster(GridSettings gridRequest, string styleCode, string styleSize, string styleColor, string revNo, string edition)
        {
            try
            {
                //Return an empty list of opmt if keys code is empty.
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColor)
                    || string.IsNullOrEmpty(revNo))
                    return Json(new List<Opmt>(), JsonRequestBehavior.AllowGet);

                int pageIndex = gridRequest.pageIndex;
                int pageSize = gridRequest.pageSize;
                var lstOpmt = new List<Opmt>();
                //lstOpmt = string.IsNullOrWhiteSpace(edition)
                //    ? OpmtBus.GetOpMaster(styleCode, styleSize, styleColor, revNo, pageIndex, pageSize)
                //    : OpmtBus.GetOpMasterByEdition(styleCode, styleSize, styleColor, revNo, pageIndex, pageSize, edition);

                lstOpmt = string.IsNullOrWhiteSpace(edition)
                    ? OpmtBus.GetOpMaster2(styleCode, styleSize, styleColor, revNo)
                    : OpmtBus.GetOpMasterByEdition2(styleCode, styleSize, styleColor, revNo, edition);

                return Json(lstOpmt, JsonRequestBehavior.AllowGet);

                //var opMaster = lstOpmt;

                //decimal totalRecords = 0;
                //if (opMaster.Count > 0) { totalRecords = opMaster[0].TotalRecords; }

                //int modes = totalRecords % pageSize == 0 ? 0 : 1;
                //var totalPage = totalRecords > 0 ? (totalRecords / pageSize) + modes : 1;
                //var pagingOpMaster = new
                //{
                //    total = totalPage,
                //    page = pageIndex,
                //    records = totalRecords,
                //    rows = opMaster
                //};

                //return Json(pagingOpMaster, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetOpMasterByStyle(string styleCode, string styleSize, string styleColor, string revNo, string edition)
        {
            try
            {
                var lstOpmt = new List<Opmt>();

                //START ADD - SON) 30/Jan/2021
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize) || string.IsNullOrEmpty(styleColor) || string.IsNullOrEmpty(revNo))
                {
                    return Json(lstOpmt, JsonRequestBehavior.AllowGet);
                }
                //END ADD - SON) 30/Jan/2021

                lstOpmt = string.IsNullOrWhiteSpace(edition)
                    ? OpmtBus.GetOpMaster2(styleCode, styleSize, styleColor, revNo)
                    : OpmtBus.GetOpMasterByEdition2(styleCode, styleSize, styleColor, revNo, edition);

                //Sorting list operation plan
                var orderListOpmt = from s in lstOpmt
                                    orderby s.Sorting
                                    select s;

                //START ADD - SON) 30/Jan/2021 - check the last confirmation for MES operation plan
                var lastOpmtMes = orderListOpmt.Where(x => x.Edition == "M" && x.ConfirmChk == "Y").OrderByDescending(x => x.ConfirmedDate).FirstOrDefault();
                if (lastOpmtMes != null) lastOpmtMes.IsLastConfirmation = "Y";
                //END ADD - SON) 30/Jan/2021

                return Json(orderListOpmt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //Get object ops master by key code
        //Author: Son Nguyen Cao
        public JsonResult GetListOpsMasterByCode(Opmt opMaster)
        {
            try
            {
                var objOpMaster = OpmtBus.GetOpsMasterByCode(opMaster);
                if (objOpMaster != null) return Json(objOpMaster.FirstOrDefault(), JsonRequestBehavior.AllowGet);
                return Json(new Opmt(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //START ADD) SON - 25/Feb/2019: Get the number of copied processes with standard name
        public ActionResult GetNumberOfProcesses(string edition, string languageId, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            try
            {
                //Get list of process with standard name
                var lstOpdt = OpdtBus.GetOpDetailWithStandardName(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition, languageId);

                //Count the number of processes with standard name
                var countPro = lstOpdt != null ? lstOpdt.Count : 0;

                return Json(countPro, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        //END ADD) SON - 25/Feb/2019

        //Author: son Nguyen Cao
        public ActionResult AddNewOps(string editionReg, Opmt opMaster, Opmt opMasterCopy, List<Opdt> lstOpDetail,
                                        string copyToolLinking, string copySelectPlan, string registerEmptyPlan, string importFile, string copyBOMPattern)
        {
            try
            {
                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(opMaster.Edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? SystemMesId : SystemOpsId;
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);
                if (objUserRole == null) return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                //Check role
                if (!CommonMethod.CheckRole(objUserRole.IsAdd))
                    return Json(ConstantGeneric.AlertPermission, JsonRequestBehavior.AllowGet);

                var listProt = new List<Prot>();//ADD) SON - 5/Jun/2019
                List<Optl> lstTool = new List<Optl>();
                List<Opdt> lstOpDetailCopy = new List<Opdt>();
                var lstOpnt = new List<Opnt>();

                if (copySelectPlan == ConstantGeneric.True)
                {
                    //Get operation master for copying
                    var copyOpmt = OpmtBus.GetOpsMasterByCode(opMasterCopy).FirstOrDefault();
                    //Change Operation revision and edition to insert new one.
                    copyOpmt.StyleCode = opMaster.StyleCode;
                    copyOpmt.StyleSize = opMaster.StyleSize;
                    copyOpmt.StyleColorSerial = opMaster.StyleColorSerial;
                    copyOpmt.RevNo = opMaster.RevNo;
                    copyOpmt.OpRevNo = opMaster.OpRevNo;
                    copyOpmt.Edition = opMaster.Edition;
                    copyOpmt.Language = opMaster.Language;
                    copyOpmt.Factory = opMaster.Factory; //ADD) SON - 1/Jul/2019
                    copyOpmt.Reason = opMaster.Reason; //ADD - SON) 30/Jan/2021
                    copyOpmt.OpSource = opMaster.OpSource; //ADD - SON) 30/Jan/2021
                    opMaster = copyOpmt;

                    //START MOD) SON - 23/Feb/2019: Get list of processes with process standard name only
                    //Start check standar process name
                    //If process does not have process detail (opnt) then check process name if process name is standard name (opnm)
                    //then insert process name id ot opnt table
                    //Get all process by opmt code.
                    lstOpDetailCopy = OpdtBus.GetOpDetailByLanguage(opMasterCopy.StyleCode, opMasterCopy.StyleSize, opMasterCopy.StyleColorSerial,
                        opMasterCopy.RevNo, opMasterCopy.OpRevNo, opMasterCopy.Edition, opMaster.Language);

                    //Get list of process detail with standard name.
                    var listOpdtStdNameCopy = OpdtBus.GetOpDetailWithStandardName(opMasterCopy.StyleCode, opMasterCopy.StyleSize, opMasterCopy.StyleColorSerial,
                   opMasterCopy.RevNo, opMasterCopy.OpRevNo, opMasterCopy.Edition, opMaster.Language);

                    var listNSDProcess = new List<Opdt>();
                    var listNewOpnt = new List<Opnt>();
                    //Get list process name with none standard name
                    foreach (var opdtNtd in lstOpDetailCopy)
                    {
                        var exitsOpdt = listOpdtStdNameCopy.Any(opdt => opdt.StyleCode == opdtNtd.StyleCode && opdt.StyleSize == opdtNtd.StyleSize && opdt.StyleColorSerial == opdtNtd.StyleColorSerial && opdt.RevNo == opdtNtd.RevNo && opdt.OpRevNo == opdtNtd.OpRevNo && opdt.OpSerial == opdtNtd.OpSerial);
                        if (!exitsOpdt)
                        {
                            listNSDProcess.Add(opdtNtd);
                        }
                    }

                    //Find standard name for list of process None Standad and insert it to process detail table (opnt)
                    foreach (var opdtNtd in listNSDProcess)
                    {
                        //Get name from process name and split it into array
                        var arrOpName = opdtNtd.OpName.Split('|');
                        var opnSerial = 0; //Declare OpSearal for process name detail
                        foreach (var proName in arrOpName)
                        {
                            opnSerial++;
                            //Find process name in process name master to get id
                            var objOpnm = OpnmBus.GetOpNameId(proName);
                            if (objOpnm != null)
                            {
                                var opnt = new Opnt
                                {
                                    StyleCode = opdtNtd.StyleCode,
                                    StyleSize = opdtNtd.StyleSize,
                                    StyleColorSerial = opdtNtd.StyleColorSerial,
                                    RevNo = opdtNtd.RevNo,
                                    OpRevNo = opdtNtd.OpRevNo,
                                    OpSerial = opdtNtd.OpSerial,
                                    OpNameId = objOpnm.OpNameId,
                                    Edition = opdtNtd.Edition,
                                    OpTime = opnSerial == 1 ? opdtNtd.OpTime : 0,
                                    OpnSerial = opnSerial

                                };
                                //Add new process name detail to the list
                                listNewOpnt.Add(opnt);
                            }
                        }
                    }

                    if (listNewOpnt.Count > 0)
                    {
                        //Insert process name detail to database
                        if (!OpntBus.InsertListOpnt(listNewOpnt))
                        {
                            return Json("Cannot insert process name detail. ", JsonRequestBehavior.AllowGet);
                        }
                    }

                    //End check standar process name

                    //Get list process to copy.
                    //lstOpDetailCopy = OpdtBus.GetOpDetailByLanguage(opMasterCopy.StyleCode, opMasterCopy.StyleSize, opMasterCopy.StyleColorSerial,
                    //    opMasterCopy.RevNo, opMasterCopy.OpRevNo, opMasterCopy.Edition, opMaster.Language);

                    // lstOpDetailCopy = OpdtBus.GetOpDetailWithStandardName(opMasterCopy.StyleCode, opMasterCopy.StyleSize, opMasterCopy.StyleColorSerial,
                    //opMasterCopy.RevNo, opMasterCopy.OpRevNo, opMasterCopy.Edition, opMaster.Language);

                    //Recalculate takt time and recount the number of processes, machines and men in operation plan
                    //if (opMaster.OpCount > lstOpDetailCopy.Count() || listNewOpnt.Count > 0)
                    //{
                    //    //Count the number of processes, machines and men.
                    //    var opmtCount = OpmtBus.CountProcessesWithStandardName(opMasterCopy.Edition, opMasterCopy.StyleCode, opMasterCopy.StyleSize, opMasterCopy.StyleColorSerial, opMasterCopy.RevNo, opMasterCopy.OpRevNo);
                    //    //Recalculate takt time
                    //    var taktTime = OpdtBus.GetTackTimeWithStandardName(opMasterCopy);

                    //    if (opmtCount != null)
                    //    {
                    //        opMaster.OpCount = int.Parse(opmtCount.SumOpCount.ToString());
                    //        opMaster.MachineCount = int.Parse(opmtCount.SumMachineCount.ToString());
                    //        opMaster.ManCount = int.Parse(Math.Ceiling((decimal)opmtCount.SumManCount).ToString());  //int.Parse(opmtCount.SumManCount.ToString());
                    //    }
                    //    else //There is no process with standard name.
                    //    {
                    //        opMaster.OpCount = 0;
                    //        opMaster.MachineCount = 0;
                    //        opMaster.ManCount = 0;
                    //    }

                    //    opMaster.OpTime = int.Parse(taktTime.ToString());

                    //}
                    //END MOD) SON - 23/Feb/2019

                    //Check copy tool linking - If operation plan with edition is MES then let copy tool none standard process name.
                    if (copyToolLinking == ConstantGeneric.True)
                    {

                        lstTool = OptlBus.GetListToolLinking(opMasterCopy.StyleCode, opMasterCopy.StyleSize,
                            opMasterCopy.StyleColorSerial, opMasterCopy.RevNo, opMasterCopy.OpRevNo, opMasterCopy.Edition);

                        //lstTool = OptlBus.GetListToolLinkingWithStandardName(opMasterCopy.StyleCode, opMasterCopy.StyleSize,
                        //opMasterCopy.StyleColorSerial, opMasterCopy.RevNo, opMasterCopy.OpRevNo, opMasterCopy.Edition);

                        //Change Ops master key in list copy tool linking
                        foreach (var tool in lstTool)
                        {
                            tool.StyleCode = opMaster.StyleCode;
                            tool.StyleColorSerial = opMaster.StyleColorSerial;
                            tool.StyleSize = opMaster.StyleSize;
                            tool.RevNo = opMaster.RevNo;
                            tool.OpRevNo = opMaster.OpRevNo;
                            tool.Edition = opMaster.Edition;
                        }
                    }

                    if (copyBOMPattern == ConstantGeneric.True)
                    {
                        //Check if key of copied style are same key of style master then copy PROT
                        if (opMasterCopy.StyleCode == opMaster.StyleCode && opMasterCopy.StyleSize == opMaster.StyleSize
                            && opMasterCopy.StyleColorSerial == opMaster.StyleColorSerial && opMasterCopy.RevNo == opMaster.RevNo)
                        {
                            //START ADD) SON - 5/Jun/2019 - Get linked pattern 
                            listProt = ProtBus.GetListPatternLinked(opMasterCopy.StyleCode, opMasterCopy.StyleSize,
                                opMasterCopy.StyleColorSerial, opMasterCopy.RevNo, opMasterCopy.OpRevNo, opMasterCopy.Edition);

                            //Change operation plan key 
                            foreach (var prot in listProt)
                            {
                                prot.StyleCode = opMaster.StyleCode;
                                prot.StyleColorSerial = opMaster.StyleColorSerial;
                                prot.StyleSize = opMaster.StyleSize;
                                prot.RevNo = opMaster.RevNo;
                                prot.OpRevNo = opMaster.OpRevNo;
                                prot.Edition = opMaster.Edition;
                            }
                            //END ADD) SON - 5/Jun/2019
                        }

                    }

                    //Get list opeartion name detai for coping
                    lstOpnt = OpntBus.GetOpNameDetails(opMasterCopy.Edition, opMasterCopy.StyleCode, opMasterCopy.StyleSize, opMasterCopy.StyleColorSerial, opMasterCopy.RevNo, opMasterCopy.OpRevNo, "", "", "");

                    //Change operation plan master key code
                    foreach (var opnt in lstOpnt)
                    {
                        opnt.StyleCode = opMaster.StyleCode;
                        opnt.StyleSize = opMaster.StyleSize;
                        opnt.StyleColorSerial = opMaster.StyleColorSerial;
                        opnt.RevNo = opMaster.RevNo;
                        opnt.OpRevNo = opMaster.OpRevNo;
                        opnt.Edition = editionReg;
                    }

                    //Change Ops key code in list Opdetail.
                    foreach (var opdt in lstOpDetailCopy)
                    {
                        //set opname = opname language
                        if (!string.IsNullOrWhiteSpace(opdt.OpNameLan))
                        {
                            opdt.OpName = opdt.OpNameLan;
                        }

                        opdt.StyleCode = opMaster.StyleCode;
                        opdt.StyleSize = opMaster.StyleSize;
                        opdt.StyleColorSerial = opMaster.StyleColorSerial;
                        opdt.RevNo = opMaster.RevNo;
                        opdt.OpRevNo = opMaster.OpRevNo;
                        opdt.Edition = editionReg;
                    }
                }
                else if (importFile == ConstantGeneric.True)
                {
                    //START ADD - SON) 18/Jul/2020
                    //Get list modules
                    var listModules = SamtBus.GetModules(opMaster.StyleCode);
                    //END ADD - SON) 18/Jul/2020

                    //Get group code base on group name (code name in t_cm_mcmt table)
                    foreach (var opdt in lstOpDetail)
                    {
                        //START ADD - 18/Jul/2020
                        //Find new module by module name
                        var module = listModules.Find(x => x.ModuleName == opdt.ModuleName?.Trim() && x.IsNew == "Y");
                        if (module == null)
                        {
                            //If new module does not exist then get old module
                            module = listModules.Find(x => x.ModuleName == opdt.ModuleName?.Trim() && x.IsNew == "N");
                        }
                        //If module is null then let moduleId is empty
                        var moduleId = module != null ? module.ModuleId : string.Empty;
                        //Assign module Id to operation detail
                        opdt.ModuleId = moduleId;
                        //END ADD - 18/Jul/2020

                        //Time balancing
                        opdt.OpTimeBalancing = opdt.OpTime;

                        //START MOD - SON) 18/Jul/2020 - don't use process group                     
                        ////Map process group
                        //var masterCode = string.IsNullOrEmpty(opdt.OpGroup) ? null : McmtBus.GetSubCodeByCodeName(opdt.OpGroup.Trim());
                        //var subCode = masterCode == null ? string.Empty : masterCode.SubCode;
                        //opdt.OpGroup = subCode;
                        //END MOD - SON) 18/Jul/2020

                        //Map machine
                        var objOtmt = string.IsNullOrEmpty(opdt.MachineName) ? null : OtmtBus.GetMachineId(opdt.MachineName.Trim());
                        var machineId = objOtmt == null ? string.Empty : objOtmt.ItemCode;
                        opdt.MachineType = machineId;

                        //Map process name.

                        var arrOpNameId = opdt.ArrOpNameId.Split('|');
                        var opnSerial = 1;
                        foreach (var opNameId in arrOpNameId)
                        {
                            var opTime = opnSerial == 1 ? opdt.OpTime : 0;
                            var id = int.Parse(opNameId);
                            if (id != 0)
                            {
                                //Get list process name detail.
                                var opnt = new Opnt
                                {
                                    StyleCode = opdt.StyleCode,
                                    StyleSize = opdt.StyleSize,
                                    StyleColorSerial = opdt.StyleColorSerial,
                                    RevNo = opdt.RevNo,
                                    OpRevNo = opdt.OpRevNo,
                                    OpSerial = opdt.OpSerial,
                                    OpNameId = id,
                                    Edition = opdt.Edition,
                                    OpnSerial = opnSerial,
                                    OpTime = opTime//opdt.OpTime
                                };
                                lstOpnt.Add(opnt);
                                opnSerial++;
                            }
                        }

                    }

                    lstOpDetailCopy = lstOpDetail;
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
                opMaster.RegisterId = UserInf.UserName;

                if (opMaster.Edition == ConstantGeneric.EditionMes)
                {
                    opMaster.MxPackage = ConstantGeneric.MxPackageDefault;
                }

                //START MOD) SON - 04/Jun/2019 - Check if edition is MES then save operation plan to MES schema
                var resAdd = OpmtBus.AddNewOpsMasterDetail_New(editionReg, opMaster, lstOpDetailCopy, lstOpnt, lstTool, copyToolLinking, copySelectPlan, registerEmptyPlan, importFile, listProt);

                //END MOD) SON - 04/Jun/2019
                //Count process, machine... if add process by importing csv file.
                if (importFile == ConstantGeneric.True)
                {
                    //Count processes, machines, wokers and calculate optime. 
                    var objOpmt = CommonUtility.CountOperationPlan(opMaster.Edition, opMaster.StyleCode, opMaster.StyleSize, opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo);
                    //Update operation master.
                    OpmtBus.UpdateOpMaster(objOpmt);
                }

                // Record log add new operation master.
                var status = CommonUtility.ConvertBoolToString01(resAdd);
                CommonUtility.InsertLogActivity(opMaster, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventAdd, "Add new operation plan.", status);

                return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventAdd, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Delete object ops master
        //Author: Son Nguyen Cao
        [HttpPost]
        public string DeleteOpsMaster(Opmt opMaster)
        {
            try
            {
                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(opMaster.Edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? SystemMesId : SystemOpsId;
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);
                if (objUserRole == null) return CommonUtility.ObjectToJson(ConstantGeneric.AlertPermission);

                var checkConfrimed = CheckRoleEditionConfirmed(objUserRole.IsDelete, opMaster, null);
                if (checkConfrimed != ConstantGeneric.False)
                    return checkConfrimed;

                //Get ops detail
                var opDetail = new Opdt
                {
                    StyleCode = opMaster.StyleCode,
                    StyleSize = opMaster.StyleSize,
                    StyleColorSerial = opMaster.StyleColorSerial,
                    RevNo = opMaster.RevNo,
                    OpRevNo = opMaster.OpRevNo,
                    Edition = opMaster.Edition
                };

                //Check key code
                if (!CommonUtility.CheckKeyCodeOpMaster(opMaster.StyleCode, opMaster.StyleSize,
                    opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo))
                    return CommonUtility.ObjectToJson("Please check opeation plan master key code.");

                //Get list op detail base on op master key code
                //var lstOpdt = OpdtBus.GetOpDetailByLanguage(opMaster.StyleCode, opMaster.StyleSize, opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo, opMaster.Edition, opMaster.Language);

                //START MOD) SON - 5/Jun/2019 - Delete 
                //Delete ops master;
                var resDelete = OpmtBus.DeleteOpsMasterDetailToolLinking(opMaster);
                //END MOD) SON - 5/Jun/2019 - Delete 

                // Record log delete operation master.
                var status = CommonUtility.ConvertBoolToString01(resDelete);
                CommonUtility.InsertLogActivity(opMaster, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventDelete, "Delete operation plan.", status);

                return !resDelete ? CommonUtility.ObjectToJson(ConstantGeneric.Fail) : CommonUtility.ObjectToJson(ConstantGeneric.Success);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }

        }

        //Confirm object ops master
        //Author: Son Nguyen Cao
        [HttpPost]
        public string ConfirmOpsMaster(Opmt opMaster)
        {
            try
            {
                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(opMaster.Edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? SystemMesId : SystemOpsId;
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);
                if (objUserRole == null) return CommonUtility.ObjectToJson(ConstantGeneric.AlertPermission);

                var checkConfrimed = CheckRoleEditionConfirmed(objUserRole.IsConfirm, opMaster, null);
                if (checkConfrimed != ConstantGeneric.False)
                    return checkConfrimed;

                //Check key code
                if (!CommonUtility.CheckKeyCodeOpMaster(opMaster.StyleCode, opMaster.StyleSize,
                    opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo))
                    return "Please check opeation plan master key code.";


                //START ADD) SON - 21/Jun/2019 check standard process
                var listNStd = OpdtBus.GetListProcesWithNoneStandardName(opMaster);
                if (listNStd.Count > 0) return CommonUtility.ObjectToJson("Opeartion Plan is using none standard name.");
                //END check standard process


                var resConf = OpmtBus.ConfirmOpsMasterAndDetail(opMaster);

                // Record log add confirm operation master.
                var status = CommonUtility.ConvertBoolToString01(resConf);
                CommonUtility.InsertLogActivity(opMaster, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventConfirm, "Confirm operation plan.", status);

                return resConf ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson("Something was wrong while confirming operation plan.");
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventConfirm, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }
        }

        [HttpPost]
        public string UpdateIsUsedOp(Opmt opMaster)
        {
            try
            {
                //Get user role.
                var menuId = CommonUtility.GetMenuIdByEdition(opMaster.Edition);
                var systemId = menuId == ConstantGeneric.MesMenu ? SystemMesId : SystemOpsId;
                var objUserRole = SrmtBus.GetUserRoleInfo(UserInf.RoleID, systemId, menuId);
                if (objUserRole == null) return CommonUtility.ObjectToJson(ConstantGeneric.AlertPermission);

                //Check key code
                if (!CommonUtility.CheckKeyCodeOpMaster(opMaster.StyleCode, opMaster.StyleSize,
                    opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo))
                    return CommonUtility.ObjectToJson("Please check opeation plan master key code.");

                var resUpdate = OpmtBus.UpdateIsUsedStatus(opMaster.Edition, opMaster.StyleCode, opMaster.StyleSize, opMaster.StyleColorSerial, opMaster.RevNo, opMaster.OpRevNo);

                // Record log delete operation master.
                var status = CommonUtility.ConvertBoolToString01(resUpdate);
                CommonUtility.InsertLogActivity(opMaster, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventDelete, "Update is used operation plan.", status);

                return !resUpdate ? CommonUtility.ObjectToJson(ConstantGeneric.Fail) : CommonUtility.ObjectToJson(ConstantGeneric.Success);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventDelete, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(exc.Message);
            }

        }
        #endregion

        #region Upload Videos and Image

        //Upload image of process to local folder
        //Author: Son Nguyen Cao
        [HttpPost]
        public string UploadImageProcess()
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
                        sysFileName = CreateSystemFileName(styleCode, styleSize, styleColor, styleRevNo, opRevNo, opSerial, extFile);

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

        /// <summary>
        /// Author: Son Nguyen Cao
        /// Date: 25 July 2017
        /// Uploads the video process.
        /// </summary>
        /// <returns>A string success or error</returns>
        [HttpPost]
        public string UploadVideoProcess()
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

                var sysFileName = styleCode + styleSize + styleColor + styleRevNo + opRevNo + opSerial;

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

        /// <summary>
        /// Author: Son Cao Nguyen
        /// Date: 25 July 2017
        /// Creates the name of the system file.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">Color of the style.</param>
        /// <param name="styleRev">The style rev.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial">The op serial.</param>
        /// <param name="extFile">The ext file.</param>
        /// <returns></returns>
        private static string CreateSystemFileName(string styleCode, string styleSize, string styleColor, string styleRev, string opRevNo, string opSerial, string extFile)
        {
            //Check extention of file
            if (string.IsNullOrEmpty(extFile)) return "";

            //Check Operation Plan serial
            if (string.IsNullOrEmpty(opSerial)) return "";

            //Check selection Operation Plan master
            if (!CommonUtility.CheckKeyCodeOpMaster(styleCode, styleSize, styleColor, styleRev, opRevNo)) return "";

            //Create file name.
            var fileName = styleCode + styleSize + styleColor + styleRev + opRevNo + opSerial + extFile;

            return fileName;
        }

        #endregion

        #region Upload Image Style Master

        /// <summary>
        /// Read string and write to an array.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static byte[] ReadStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Upload file to FTP directly
        /// </summary>
        /// <param name="fileContent"></param>
        /// <param name="ftp"></param>
        /// Author: Son Nguyen Cao.
        public void UploadFilesToFtp(HttpPostedFileBase fileContent, FtpInfo ftp, string subFtpDir, string fileName)
        {
            try
            {
                byte[] fileBytes = null;

                //Read the FileName and convert it to Byte array.
                //string orgFileName = fileContent.FileName;
                //Stream reader using for reading text.
                //using (StreamReader fileStream = new StreamReader(fileContent.InputStream))
                //{                    
                //    fileBytes = Encoding.UTF8.GetBytes(fileStream.ReadToEnd());
                //    fileStream.Close();
                //}

                fileBytes = ReadStream(fileContent.InputStream);

                //Create FTP Request.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp.FtpRoot + subFtpDir + fileName);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(ftp.FtpUser, ftp.FtpPass);
                request.ContentLength = fileBytes.Length;
                request.UsePassive = true;
                request.UseBinary = true;
                request.ServicePoint.ConnectionLimit = fileBytes.Length;
                request.EnableSsl = false;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                    requestStream.Close();
                }

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                response.Close();

            }
            catch (WebException ex)
            {
                throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
            }
        }

        //Author: Son Nguyen Cao
        //Upload image style master
        [HttpPost]
        public string UploadImageStyleMaster()
        {
            string pathFile = string.Empty;
            try
            {
                //Check role
                var roleStyle = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.OpStyleMenuId);
                if (!CommonMethod.CheckRole(roleStyle.IsAdd)) return CommonUtility.ObjectToJson(ConstantGeneric.AlertPermission);

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    var data = Request.Form;
                    var styleCode = data["StyleCode"];

                    if (fileContent == null || fileContent.ContentLength <= 0) continue;
                    var fileName = fileContent.FileName;
                    var extFile = Path.GetExtension(fileName)?.ToLower();
                    var subFolder = styleCode.Substring(0, 3) + "/" + styleCode + "/" + ConstantGeneric.FtpStyleImageFolder + "/";
                    //var pathFolder = Server.MapPath(ConstantGeneric.OpsStyleImagePath) + subFolder;
                    var pathFolder = ConstantGeneric.OpsStyleImagePath;

                    var maxSerial = SfdtBus.GetMaxSerialStyleCode(styleCode);

                    //Create new filename
                    var newFileName = styleCode + maxSerial + extFile;

                    pathFile = pathFolder + newFileName;

                    //Check file was exist or not.
                    //pathFile = CommonMethod.CheckAndCreateNewPath(pathFile);
                    //newFileName = Path.GetFileName(pathFile);

                    if (string.IsNullOrEmpty(newFileName) || string.IsNullOrEmpty(pathFile))
                        return "File name or path of file is empty!";

                    //Create folder
                    if (!Directory.Exists(pathFolder)) Directory.CreateDirectory(pathFolder);

                    //Save file
                    fileContent.SaveAs(pathFile);

                    //Update style picture name
                    //var updateStatus = StmtBus.UpdateStylePictureName(styleCode, newFileName);

                    //Crete new style file detail for insert
                    var sfdtIns = new Sfdt()
                    {
                        StyleCode = styleCode,
                        Serial = maxSerial,
                        FileName = newFileName,
                        Description = "OPS",
                        IsMain = "Y" //Set is_main stauts is Y
                    };

                    //Get style file with the max serial
                    var sfdtUpd = new Sfdt();
                    if (maxSerial - 1 > 0)
                    {
                        sfdtUpd = SfdtBus.GetStyleFile(styleCode, (maxSerial - 1).ToString());
                        sfdtUpd.IsMain = "N"; //Change is_main status to N
                    }

                    //Update picture name for style, Insert record and update the new main picture.
                    var updateStatus = SfdtBus.InsertUpdateStyleFileDetail(sfdtIns, sfdtUpd);

                    // Record log add upload image of style.
                    var status = CommonUtility.ConvertBoolToString01(updateStatus);
                    CommonUtility.InsertLogActivity(styleCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventEdit, "Upload style image.", status);

                    if (updateStatus)
                    {
                        //Upload Style Image to Ftp
                        var ftpInfo = FtpInfoBus.GetFtpInfo();
                        var ftpFilePath = ftpInfo.FtpRoot + ConstantGeneric.FtpStyleFolder + "/" + subFolder + newFileName;
                        //Create folder on FTP server to save Style image.
                        FtpInfoBus.CreateFTPDirectory(ftpInfo, ConstantGeneric.FtpStyleFolder + "/" + subFolder);
                        if (!FtpInfoBus.UploadFileToFtpFromSource(pathFile, ftpFilePath, ftpInfo))
                            System.IO.File.Delete(pathFile);
                    }
                    else
                    {
                        System.IO.File.Delete(pathFile);
                        return "Cannot upload style image.";
                    }
                }

                return ConstantGeneric.Success;
            }
            catch (Exception exc)
            {
                if (!string.IsNullOrEmpty(pathFile)) System.IO.File.Delete(pathFile);
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return exc.Message;
            }

        }

        //Author: Son Nguyen Cao
        public JsonResult UpdateStyleGroup(string styleCode, string styleGroup, string subGroup, string subSubGroup)
        {
            try
            {
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleGroup) || string.IsNullOrEmpty(subGroup))
                    return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

                var objStyle = StmtBus.GetStyleMasterByStyleCode(styleCode);
                if (objStyle != null)
                {
                    if (string.IsNullOrEmpty(objStyle.StyleGroup) || string.IsNullOrEmpty(objStyle.SubGroup) || string.IsNullOrEmpty(objStyle.SubSubGroup))
                    {
                        //Update style group
                        var resUpdate = StmtBus.UpdateStyleGroup(styleCode, styleGroup, subGroup, subSubGroup);

                        // Record log update style group.
                        var status = CommonUtility.ConvertBoolToString01(resUpdate);
                        CommonUtility.InsertLogActivity(styleCode, UserInf, SystemOpsId, ConstantGeneric.ScreenRegistry, ConstantGeneric.EventEdit, "Update style group.", status);

                        if (!resUpdate) return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);

                        return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
                    }

                    return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
                }

                return Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpStyleMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Upload file and video of Operation Plan detail

        //Upload machine file for op process
        //Author: Son Nguyen Cao
        [HttpPost]
        public string UploadOpDetailFile()
        {
            //Check role permission.
            var roleStyle = SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.OpManagementMenuId);
            if (!CommonMethod.CheckRole(roleStyle.IsUpdate)) return CommonUtility.ObjectToJson(ConstantGeneric.AlertPermission);

            var tempFilePath = string.Empty;
            //Get temporary folder
            var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);

            var lstFileName = new List<string>();
            //var newFileName = string.Empty;
            try
            {
                //Check role of user
                if (!CommonMethod.CheckRole(Role.IsUpdate)) return ConstantGeneric.AlertPermission;

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent == null || fileContent.ContentLength <= 0) continue;
                    var fileName = CommonUtility.GetValidFileName(fileContent.FileName);

                    var extFile = Path.GetExtension(fileName)?.ToLower().Substring(1);
                    if (!ConstantGeneric.ArrMachineFileType.Contains(extFile))
                        return CommonUtility.ObjectToJson(CommonUtility.GetResultContent(ConstantGeneric.Fail,
                            "You are not authorized to upload types of file: " + extFile));

                    //Create path file folder
                    tempFilePath = tempFolder + fileName;

                    //Check if the folder is not exits then create it and save file
                    if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
                    fileContent.SaveAs(tempFilePath);

                    lstFileName.Add(fileName);

                }

                return CommonUtility.ObjectToJson(CommonUtility.GetResultContent(ConstantGeneric.Success, lstFileName));
            }
            catch (Exception exc)
            {
                foreach (var fileName in lstFileName)
                {
                    var tempPath = tempFolder + fileName;
                    System.IO.File.Delete(tempPath);
                }

                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(CommonUtility.GetResultContent(ConstantGeneric.Fail, exc.Message));
            }
        }

        [HttpPost]
        public string UploadJigFile()
        {
            try
            {
                var data = Request.Form;
                var styleCode = data["StyleCode"];
                var styleSize = data["StyleSize"];
                var styleColorSerial = data["StyleColor"];
                var revNo = data["StyleRevNo"];
                var opRevNo = data["OpRevNo"];
                var opSerial = data["OpSerial"].PadLeft(3, '0');
                var edition = data["Edition"];
                var uploadCode = data["UploadCode"];
                var uploadType = data["UploadType"];

                //Check role permission.
                var roleStyle = GetUserRoleByEdition(edition);
                if (roleStyle == null || !CommonMethod.CheckRole(roleStyle.IsUpdate)) return CommonUtility.ObjectToJson(ConstantGeneric.AlertPermission);

                //Check process key code.
                if (!CommonUtility.CheckKeyCodeOpDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial))
                {
                    return CommonUtility.ObjectToJson("Pleas check key code of operation plan detail.");
                }

                foreach (string file in Request.Files)
                {
                    var fileContent = Request.Files[file];
                    if (fileContent == null || fileContent.ContentLength <= 0) continue;
                    var fileName = fileContent.FileName;
                    if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) continue;

                    fileName = CommonUtility.GetValidFileName(fileContent.FileName);

                    var amendNo = OpflBus.GetMaxAmendNoOpFile(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition, uploadCode);
                    var sysFileName = CommonUtility.CreatSystemFileName(uploadCode, amendNo, Path.GetExtension(fileName));

                    if (string.IsNullOrEmpty(sysFileName)) return CommonUtility.ObjectToJson("Cannot create system file name.");

                    string strExt = Path.GetExtension(fileName);
                    var extFile = string.Empty;
                    if (!string.IsNullOrEmpty(strExt))
                    {
                        extFile = strExt?.ToLower().Substring(1);
                    }
                    //var extFile = Path.GetExtension(fileName)?.ToLower().Substring(1);
                    if (uploadType == ConstantGeneric.MachineFile)
                    {
                        if (uploadCode != ConstantGeneric.SubCodeOtherFile && uploadCode != ConstantGeneric.SubCodeSewqFile && uploadCode != ConstantGeneric.SubCodeSunStarFile)
                        {
                            if (!ConstantGeneric.ArrMachineFileType.Contains(extFile))
                                return CommonUtility.ObjectToJson("You are not authorized to upload types of file: " + extFile);
                        }
                    }
                    else
                    {
                        if (!ConstantGeneric.ArrJigType.Contains(extFile))
                            return CommonUtility.ObjectToJson("You are not authorized to upload types of file: " + extFile);
                    }

                    //Insert file information to database.
                    var opfl = new Opfl
                    {
                        StyleCode = styleCode,
                        StyleSize = styleSize,
                        StyleColorSerial = styleColorSerial,
                        RevNo = revNo,
                        OpRevNo = opRevNo,
                        OpSerial = int.Parse(opSerial),
                        Edition = edition,
                        UploadCode = uploadCode,
                        AmendNo = amendNo,
                        Register = UserInf.UserName,
                        SysFileName = sysFileName,
                        OrgFileName = fileName,
                        SourceFile = ConstantGeneric.SourceUpload
                    };

                    if (!OpflBus.InsertProcessFile(opfl))
                    {
                        return CommonUtility.ObjectToJson("Cannot insert file information.");
                    }

                    //Creat string sub folder to store Jig file
                    var subFolder = CommonUtility.CreateStringSubFolder2(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial, edition);
                    if (string.IsNullOrEmpty(subFolder)) return CommonUtility.ObjectToJson("Cannot create sub folder.");

                    //Create server string folder
                    var pathFolder = Server.MapPath(ConstantGeneric.OperationFilePath) + subFolder;
                    //Create path file
                    var pathFile = pathFolder + sysFileName;

                    //Create folder on FTP server.
                    var ftpInfo2 = FtpInfoBus.GetFtpInfo();
                    FtpInfoBus.CreateFTPDirectory(ftpInfo2, subFolder);
                    UploadFilesToFtp(fileContent, ftpInfo2, subFolder, sysFileName);
                    return CommonUtility.ObjectToJson(ConstantGeneric.Success);

                }

                return CommonUtility.ObjectToJson(ConstantGeneric.Success);
            }
            catch (Exception ex)
            {
                ErrlBus.InserExceptionLog(ex, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventEdit, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(ex.Message);
            }
        }

        //Upload video for process
        //Author: Son Nguyen Cao
        [HttpPost]
        public string UploadVideoOpDetail()
        {

            var videoPath = ConstantGeneric.Fail;
            var data = Request.Form;
            var fileName = data["FileName"];
            var styleCode = data["StyleCode"];
            var styleSize = data["StyleSize"];
            var styleColor = data["StyleColor"];
            var styleRevNo = data["StyleRevNo"];
            var opRevNo = data["OpRevNo"];
            var opSerial = data["OpSerial"].PadLeft(3, '0');
            var edition = data["Edition"];

            var userRole = GetUserRoleByEdition(edition);

            //Check role
            if (userRole == null || !CommonMethod.CheckRole(userRole.IsAdd))
                return ConstantGeneric.Fail;
            //return ConstantGeneric.AlertPermission;

            if (!CommonUtility.CheckKeyCodeOpDetail(styleCode, styleSize, styleColor, styleRevNo, opRevNo, opSerial))
            {
                return "Pleas check key code of operation plan detail.";
            }

            foreach (string file in Request.Files)
            {
                var fileDataContent = Request.Files[file];
                if (fileDataContent == null || fileDataContent.ContentLength <= 0) continue;

                // take the input stream, and save it to a temp folder using the original file.part name posted
                var stream = fileDataContent.InputStream;

                var sysFileName = styleCode + styleSize + styleColor + styleRevNo + opRevNo + opSerial;

                var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);
                //Create folder if it is not exist.
                if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

                var path = Path.Combine(tempFolder, fileName);

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
                    videoPath = ut.MergeFile(path, sysFileName);
                }
                catch (Exception exc)
                {
                    ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                    return exc.Message;
                }
            }

            return Path.GetFileName(videoPath);
        }

        //Update file name of ops detail.
        //Author: Son Nguyen Cao
        [HttpPost]
        public string UpdateFileNameUpload(string fileType, Opdt opDetail, List<string> lstFileName)
        {
            try
            {
                //Get sub folder.
                var tempFolder = Server.MapPath(ConstantGeneric.OpsTempFolder);

                switch (fileType)
                {
                    case ConstantGeneric.VideoType:

                        var tempVideoPath = tempFolder + opDetail.VideoFile;
                        if (!OpdtBus.UpdateVideoName(opDetail))
                        {
                            //Delete file in temporary folder
                            if (System.IO.File.Exists(tempVideoPath))
                                System.IO.File.Delete(tempVideoPath);

                            return CommonUtility.ObjectToJson(CommonUtility.GetResultContent(ConstantGeneric.Fail, "Cannot update video name. "));

                        }
                        else
                        {

                            //Upload video to FTP server.            
                            //var ftpInfo = FtpInfoBus.GetFtpInfo();

                            var ftpInfo = new FtpInfo()
                            {
                                //ftp://203.113.151.204/BETAPDM/operationvideos/
                                FtpRoot = ConstantGeneric.FtpOpVideoDirectory,
                                FtpUser = ConstantGeneric.FtpOpVideoUser,
                                FtpPass = ConstantGeneric.FtpOpVideoPassword,
                            };

                            var fileName = opDetail.VideoFile;

                            //Create folder on FTP server.
                            FtpInfoBus.CreateFTPDirectory(ftpInfo, "");
                            //Create FTP file path string to upload Jig file to FTP
                            var ftpFilePath = ftpInfo.FtpRoot + fileName;
                            var statusUpload = FtpInfoBus.UploadFileToFtpFromSource(tempVideoPath, ftpFilePath, ftpInfo);

                            //Delete file after upload to FTP server.
                            System.IO.File.Delete(tempVideoPath);

                            if (!statusUpload) { return CommonUtility.ObjectToJson(CommonUtility.GetResultContent(ConstantGeneric.Fail, "Failure")); }

                        }
                        break;
                    case ConstantGeneric.MachineFile:
                        break;

                }

                return CommonUtility.ObjectToJson(CommonUtility.GetResultContent(ConstantGeneric.Success, "Updated"));
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, ConstantGeneric.OpManagementMenuId, ConstantGeneric.EventGetData, ConstantGeneric.OpsSystemId);
                return CommonUtility.ObjectToJson(CommonUtility.GetResultContent(ConstantGeneric.Fail, exc.Message));
            }
        }

        #endregion

        #region Update Email
        [HttpPost]
        public string UpdateEmail(string sEmail)
        {
            string username = UserInf.UserName;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(sEmail)) return CommonUtility.ObjectToJson(ConstantGeneric.Fail);

            return UsmtBus.UpdateEmail(username, sEmail) ? CommonUtility.ObjectToJson(ConstantGeneric.Success) : CommonUtility.ObjectToJson(ConstantGeneric.Fail);
        }

        //Author: Son Nguyen Cao
        public JsonResult SendEmailProcess(string toAddress, string ccAddress, string subject, string content)
        {
            try
            {
                //Send email about process video
                var staSendEma = CommonUtility.SendEmailProcess(toAddress, ccAddress, subject, content);

                return staSendEma ? Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet) : Json(ConstantGeneric.Fail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Author: Son Nguyen Cao
        public JsonResult GetUserInfoByUsername(string userName)
        {
            try
            {
                //Get user information
                var usmt = UsmtBus.GetUserInfoByUsername(userName);
                return usmt != null ? Json(usmt.Email, JsonRequestBehavior.AllowGet) : Json(string.Empty, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(string.Empty, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

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
    }
}