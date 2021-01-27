using System;
using System.Web;
using System.Web.Mvc;

namespace OPS.GenericClass
{
 
    public class SessionAuthorizeAttribute: ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework before the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// Author: Son Nguyen Cao
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;

            if (HttpContext.Current.Session["LoginUser"] == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}