using System;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.File;


namespace MES.CommonClass
{
	public class AutologArribute : ActionFilterAttribute, IExceptionFilter
	{
        /*
		 * Creator: Tai Le (Thomas)
         */

        public AutologArribute()
        {
            Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.File(
					new CompactJsonFormatter()
					, System.IO.Path.Combine(HostingEnvironment.MapPath("~"), $"Logs/log-{DateTime.Today:yyyyMMdd}.json")
					)
				.Enrich.FromLogContext()
				.CreateLogger();
        }

        public void OnException(ExceptionContext filterContext)
		{
			var RouteData = filterContext.RouteData;
			var msg = $"Controller[{RouteData.Values["controller"]}], action[{RouteData.Values["action"]}], Error Desc: {filterContext.Exception}";
			Log.Error(msg);

			Log.CloseAndFlush();
		}


		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			//Log.Logger = new LoggerConfiguration()
			//.MinimumLevel.Verbose()
			//.WriteTo.File(
			//	new CompactJsonFormatter()
			//	, System.IO.Path.Combine(HostingEnvironment.MapPath("~"), $"Logs/log-{DateTime.Today:yyyyMMdd}.json")
			//	)
			//.Enrich.FromLogContext()
			//.CreateLogger();


			//	Called by the ASP.NET MVC framework before the action method executes.
			var RouteData = filterContext.RouteData;
			var controller = RouteData.Values["controller"];
			var action = RouteData.Values["action"];

			//	Loop for QueryString-Key
			var valuesStr = new StringBuilder();
			if (filterContext.RouteData != null && filterContext.RouteData.Values != null)
				foreach (var v in filterContext.RouteData.Values)
					valuesStr.AppendFormat("/{0}", v.Value);

			//	Append the rawURL
			valuesStr.AppendFormat("/{0}", filterContext.HttpContext.Request.RawUrl);

			//Append Request.Form
			if (filterContext.HttpContext.Request.Form != null
				&& filterContext.HttpContext.Request.Form.AllKeys.Length > 0)
				foreach (var key in filterContext.HttpContext.Request.Form.AllKeys)
					valuesStr.AppendFormat("/{0}", $"key:{key},value:{filterContext.HttpContext.Request.Form[key]}");

			var msg = $"OnActionExecuting(): Controller[{RouteData.Values["controller"]}], action[{RouteData.Values["action"]}], parameters[{valuesStr}]";
			Log.Information(msg);
			 
			base.OnActionExecuting(filterContext);
		}
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			//	Called by the ASP.NET MVC framework after the action method executes.  
			base.OnActionExecuted(filterContext);
		}


		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			//	Called by the ASP.NET MVC framework after the action result executes. 
			base.OnResultExecuting(filterContext);
		}
		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			//	Called by the ASP.NET MVC framework before the action result executes. 

			//Log.CloseAndFlush();
			base.OnResultExecuted(filterContext);
		}
	}
}