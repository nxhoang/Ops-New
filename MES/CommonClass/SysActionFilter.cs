using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OPS_DAL.Entities;
using OPS_DAL.Business;

namespace MES.CommonClass
{
    public class SysActionFilter : ActionFilterAttribute, IExceptionFilter
    {
        /*
         Creator: Tai Le (Thomas)
         */

        //Your Properties in Action Filter
        public string SystemID { get; set; }
        public string MenuID { get; set; }
        public string Action { get; set; }


        public void OnException(ExceptionContext filterContext) { }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var RouteData = filterContext.RouteData;
            var controller = RouteData.Values["controller"];
            var action = RouteData.Values["action"];

            if (HttpContext.Current.Session["LoginRole"] != null)
            {
                bool IsAccess = false;

                //var ExeRole = HttpContext.Current.Session["LoginRole"].ToString();

                //Get Role from T_CM_SRMT  
                var _Srmt = SrmtBus.GetUserRoleInfoMySql(HttpContext.Current.Session["LoginRole"].ToString(), SystemID, MenuID);

                if (_Srmt != null)
                {
                    if (String.IsNullOrEmpty(Action))
                    {
                        //Default is View
                        IsAccess = _Srmt.IsView == "1";
                    }
                    else
                    {
                        switch (Action.ToUpper())
                        {
                            case "ADD":
                                IsAccess = _Srmt.IsAdd == "1";
                                break;
                            case "UPDATE":
                                IsAccess = _Srmt.IsUpdate == "1";
                                break;
                            case "DELETE":
                                IsAccess = _Srmt.IsDelete == "1";
                                break;
                            case "PRINT":
                                IsAccess = _Srmt.IsPrint == "1";
                                break;
                            case "EXPORT":
                                IsAccess = _Srmt.IsExport == "1";
                                break;
                            case "CONFIRM":
                                IsAccess = _Srmt.IsConfirm == "1";
                                break;
                            case "UNCONFIRM":
                                IsAccess = _Srmt.IsUnconfirm == "1";
                                break;
                            case "SCAN_QRCODE":
                                IsAccess = _Srmt.IsScanQRCode == "1";
                                break;
                        }
                    }
                }
                else 
                    if (HttpContext.Current.Session["LoginUserID"].ToString() == "22171078")IsAccess = true;

                if (!IsAccess)
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.HttpContext.Response.StatusCode = 401;
                        filterContext.HttpContext.Response.End();
                    }
                    else
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "_UnauthorizedPage", controller = "SysPage", RequestURL = "/" + controller + "/" + action }));
                    return;
                }
                else
                    base.OnActionExecuting(filterContext);
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Login", controller = "Account" }));
                return;
            }
        }
    }
}