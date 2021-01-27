using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using OPS.GenericClass;
using System.Web;
using System;
using System.Globalization;

namespace OPS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            RegisterGlobalFilters(GlobalFilters.Filters);
        }
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                string culture = "en";//
                var httpCookie = Request.Cookies["language"];
                if (httpCookie != null)
                {
                    culture = httpCookie.Value;
                }
                else
                {
                    HttpCookie language = new HttpCookie("language");
                    language.Value = culture;
                    language.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Add(language);
                }
                if (culture == "df")
                {
                    culture = "";
                }
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            }
            catch
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("");
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            }
            
        }
        /// <summary>
        /// Registers the global filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// Author: Son Nguyen Cao
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
        }
    }
}
