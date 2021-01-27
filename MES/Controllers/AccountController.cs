using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using OPS_Utils;
using OPS_DAL.MesBus;

namespace MES.Controllers
{
    public class AccountController: Controller
    {
        string LOGINFUNC = "LOG";
        string LOGINOPER = "I";
        //string LOGOUTOPER = "O";
        string SUCESS = "1";
        string FAILURE = "0";
        string SYSID = "MES";

        // GET: Account
        public ActionResult Index()
        {
            if (Session["LoginUser"] != null)
            {
                return RedirectToAction("Default", "Dashboard");
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            Usmt UserMdl = new Usmt();
            if (Request.Cookies["Login"] != null)
            {
                try
                {
                    UserMdl.UserName = Request.Cookies["Login"].Values["UserID"];
                    UserMdl.Password = Request.Cookies["Login"].Values["Password"];
                    UserMdl.RoleID = Request.Cookies["Login"].Values["RoleID"];
                    UserMdl.RememberMe = Request.Cookies["Login"].Values["RememberMe"] == "true" ? true : false;
                }
                catch
                {
                    return View(UserMdl);
                }
            }
            if (Session["LoginUser"] != null)
            {
                return RedirectToAction("Default", "Dashboard");
            }
            return View(UserMdl);



        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Login(Usmt model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Check user login information
            if (UsmtBus.isValidUserMySql(model))
            //if (UsmtBus.isValidUser(model))
            {
                FormsAuthentication.SetAuthCookie(model.UserName, false);
                Session["LoginUser"] = model;
                Session["LoginName"] = model.Name;
                Session["LoginRole"] = model.RoleID;

                //2019-07-16 Tai Le (Thomas): Add for quick access
                Session["LoginUserID"] = model.UserName;

                /* 2019-01-30:: Tai Le (Thomas) add 2 Sessions: LoginFactory; LoginCRCode */
                Session["LoginFactory"] = OPS_DAL.Business.UrmtBus.GetInformationByID(model.UserName, model.RoleID)[0].FACTORY;
                Session["LoginCRCode"] = OPS_DAL.Business.UrmtBus.GetInformationByID(model.UserName, model.RoleID)[0].CRCODE;

                /* 2019-08-20 put the Session["ver"]*/
                Session["ver"] = ApplicationVersionControl.BuildVersion();


                /*2020-09-08 Tai Le(Thomas): get Corporation from Web.config / ServerNo */
                if (System.Configuration.ConfigurationManager.AppSettings["ServerNo"] != null)
                {
                    var ServerNo = System.Configuration.ConfigurationManager.AppSettings["ServerNo"];
                    if (!String.IsNullOrEmpty(ServerNo))
                    {
                        //Get cstp based on [ServerNo]
                        var cstp = CstpBus.GetCstpByServerNo(ServerNo);
                        if (cstp != null)
                        {
                            if (!String.IsNullOrEmpty(cstp.CorpCode))
                                //Assign CorpCode
                                Session["CorpCode"] = cstp.CorpCode;
                        } 
                    } 
                }
                /*::END     2020-09-08 Tai Le(Thomas)*/

                try
                {
                    //Record login
                    var users = model;
                    Actl actl = new Actl
                    {
                        UserId = users.UserName,
                        RoleId = users.RoleID,
                        FunctionId = LOGINFUNC,
                        OperationId = LOGINOPER,
                        RefNo = users.UserName + users.RoleID,
                        Success = SUCESS,
                        TransactionTime = DateTime.Now,
                        Remark = "Login success",
                        SystemId = SYSID
                    };
                    ActlBus.InsertLog(actl);
                }
                catch
                {
                    //countinues;
                }

                //Set cookie
                if (model.RememberMe)
                {
                    HttpCookie cookie = new HttpCookie("Login");
                    cookie.Values.Add("UserID", model.UserName);
                    cookie.Values.Add("Password", model.Password);
                    cookie.Values.Add("RoleID", model.RoleID);
                    cookie.Values.Add("RememberMe", model.RememberMe == true ? "true" : "false");
                    cookie.Expires = DateTime.Now.AddDays(15);
                    Response.Cookies.Add(cookie);
                }
                else
                { 
                    if (Request.Cookies["Login"] != null)
                    {
                        var c = new HttpCookie("Login")
                        {
                            Expires = DateTime.Now.AddDays(-1)
                        };
                        Response.Cookies.Add(c);
                    }
                }

                //Check return Url
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Default", "Dashboard");
                }

            }
            else
            {
                //Record log if user login failure
                try
                {
                    var users = model;
                    Actl actl = new Actl
                    {
                        UserId = users.UserName,
                        RoleId = users.RoleID,
                        FunctionId = LOGINFUNC,
                        OperationId = LOGINOPER,
                        RefNo = users.UserName + users.RoleID,
                        Success = FAILURE,
                        TransactionTime = DateTime.Now,
                        Remark = "Login failure",
                        SystemId = SYSID
                    };
                    ActlBus.InsertLog(actl);
                }
                catch (Exception)
                {
                    //countinues;
                }
            }

            ViewBag.Message = "UserName or password is invalid";
            return View(model);

        }

        [AllowAnonymous]
        public ActionResult GetListRole(string userID)
        {
            try
            {
                //return Json(UrmtBus.GetListRole(userID), JsonRequestBehavior.AllowGet);
                return Json(UrmtBus.GetListRoleMySql(userID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

        }

        //START ADD) 25 Jan 2019
        public JsonResult GetRoleInfoByUserRoleId()
        {
            var user = (Usmt)Session["LoginUser"];
            var urmt = UrmtBus.GetListRoleMySql(user.UserName).Where(u => u.ROLEID == user.RoleID).FirstOrDefault();
            return Json(urmt, JsonRequestBehavior.AllowGet);
        }
        //END ADD) 25 Jan 2019

        [AllowAnonymous]
        public ActionResult GetRoleByID(string RoleID)
        {
            return Json(UrmtBus.GetRoleByID(RoleID), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();

            return RedirectToAction("Login", "Account");
        }
    }
}