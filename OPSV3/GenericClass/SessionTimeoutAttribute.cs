using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPS
{
    public class SessionTimeoutAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["LoginUser"] == null)
            {
                var url = new UrlHelper(filterContext.Controller.ControllerContext.RequestContext).Action("Login", "Account");

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.HttpContext.Response.End();
                }
                else
                {
                    filterContext.RequestContext.HttpContext.Response.Redirect("~/Account/Login");
                }
            }

        }
    }
}