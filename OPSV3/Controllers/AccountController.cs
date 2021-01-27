using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using OPS_Utils;

namespace OPS.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        string log = "LOG";
        string login = "I";
        string logOut = "O";
        string remakLogin = "Log in";
        string remakLogOut = "Log out";
        string remakLoginF = "Log in fail";
        string success = "1";
        string fail = "0";
        string sysId = "OPS";

        [AllowAnonymous]
        public ActionResult Index()
        {
            if (Session["LoginUser"] != null)
            {
                return RedirectToAction("default", "default");
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
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
                return RedirectToAction("default", "default");
            }
            return View(UserMdl);
 
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Usmt model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (UsmtBus.isValidUser(model))
            {
                FormsAuthentication.SetAuthCookie(model.UserName,false);
                Session["LoginUser"] = model;
                try
                {
                    var users = model;
                    Actl actl = new Actl();
                    actl.UserId = users.UserName;
                    actl.RoleId = users.RoleID;
                    actl.FunctionId = log;
                    actl.OperationId = login;
                    actl.RefNo = users.UserName + users.RoleID;
                    actl.Success = success;
                    actl.TransactionTime = DateTime.Now;
                    actl.Remark = remakLogin;
                    actl.SystemId = sysId;
                    ActlBus.InsertLog(actl);
                }
                catch
                {
                    //countinues;
                }
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
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")&& !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("default", "default");
                }
                
            }
            try
            {
                var users = model;
                Actl actl = new Actl();
                actl.UserId = users.UserName;
                actl.RoleId = users.RoleID;
                actl.FunctionId = log;
                actl.OperationId = login;
                actl.RefNo = users.UserName + users.RoleID;
                actl.Success = fail;
                actl.TransactionTime = DateTime.Now;
                actl.Remark = remakLoginF;
                actl.SystemId = sysId;
                ActlBus.InsertLog(actl);
            }
            catch
            {
                //countinues;
            }
            ViewBag.Message = "UserName or password is invalid";
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult GetListRole(string userID)
        {
            return Json(UrmtBus.GetListRole(userID), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult GetRoleByID(string RoleID)
        {
            return Json(UrmtBus.GetRoleByID(RoleID), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Logout()
        {
            //log
            try
            {
                var users = (Usmt)Session["LoginUser"] ?? new Usmt();
                Actl actl = new Actl();
                actl.UserId = users.UserName;
                actl.RoleId = users.RoleID;
                actl.FunctionId = log;
                actl.OperationId = logOut;
                actl.RefNo = users.UserName + users.RoleID;
                actl.Success = success;
                actl.TransactionTime = DateTime.Now;
                actl.Remark = remakLogOut;
                actl.SystemId = sysId;
                ActlBus.InsertLog(actl);
            }
            catch
            {
                //countinues;
            }
            Session.Abandon();
            FormsAuthentication.SignOut();
            
            return RedirectToAction("Login","Account");
        }

        #region User information

        //Author: Son Nguyen Cao
        public ActionResult GetUserInforByUserName(string userName)
        {
            try
            {
                //Get user information by username
                var usmt = UsmtBus.GetUserInfoByUsername(userName);

                return usmt != null ? Json(new {status = ConstantGeneric.Success, usmt = usmt }, JsonRequestBehavior.AllowGet) : Json(new { status = ConstantGeneric.Fail, usmt = string.Empty }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { status = ConstantGeneric.Fail, usmt = string.Empty }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #region Change information
        //Vithv
        public ActionResult GetUserInfo()
        {
            if (Session["LoginUser"] == null)
            {
                return null;
            }
            else
            {
                var u = (Usmt)Session["LoginUser"];
                var userid = u.UserName;
                var newu = UsmtBus.GetUserInfo(userid);
                return Json(newu, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ChangeUserInfo(string Name, string Email, string Tel, string Sex)
        {
            if (Session["LoginUser"] == null)
            {
                return null;
            }
            else
            {
                var u = (Usmt)Session["LoginUser"];
                var userid = u.UserName;
                var newu = UsmtBus.ChangeUserInfor(userid, Name, Email, Tel, Sex);
                return Json(newu, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckPass(string Password)
        {
            if (Session["LoginUser"] == null)
            {
                return null;
            }
            else
            {
                var u = (Usmt)Session["LoginUser"];
                var userid = u.UserName;
                bool check = UsmtBus.CheckPass(userid, Password);
                return Json(check, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult ChangePass(string Password)
        {
            if (Session["LoginUser"] == null)
            {
                return null;
            }
            else
            {
                var u = (Usmt)Session["LoginUser"];
                var userid = u.UserName;
                bool check = UsmtBus.ChangePass(userid, Password);
                if (check)
                {
                    Session.Abandon();
                    FormsAuthentication.SignOut();
                }
                return Json(check, JsonRequestBehavior.AllowGet);

            }
        }

        
        #endregion change information
    }
}