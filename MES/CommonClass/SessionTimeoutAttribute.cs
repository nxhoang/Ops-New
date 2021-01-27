using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MES
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
                    var controlerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    var actionName = filterContext.ActionDescriptor.ActionName;
                    var _returnUrl = "/" + controlerName + "/" + actionName;
                    //filterContext.RequestContext.HttpContext.Response.Redirect("~/Account/Login?ReturnUrl=" + returnUrl);
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Login", controller = "Account", ReturnUrl = _returnUrl }) );
                }
                return;
            }
            else
                base.OnActionExecuting(filterContext); 
        }
    }
}