using Autofac;
using Autofac.Integration.Mvc; 
using MES.Hubs;
using MES.Repositories;
using PKBugClient;
using PKERP.MES.Services.Implement.Services;
using PKERP.MES.Services.Interface.Services;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MES
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            //BugClientFactory.CreateBugClient().SendBugAsync("MES", "ApplicationStart", "Application started").Wait();

            RegisterAutofac();

            //Register API
            GlobalConfiguration.Configure(WebApiConfig.Register);
             

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles); 
        }

        private void RegisterAutofac()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterSource(new ViewRegistrationSource());

            // manual registration of types;
            //builder.RegisterType<LoggerService>.As<ILoggerService>();
            //builder.RegisterType<UnitOfWork>.As<IUnitOfWork>();
            //builder.RegisterType<ApplicationDbContext>();

            // For property injection using Autofac
            // builder.RegisterType<QuoteService>().PropertiesAutowired();

            builder.RegisterType<IotDataHub>();
            builder.RegisterType<ConfigService>().As<IConfigService>();
            builder.RegisterType<MpdtRepository>().As<IMpdtRepository>();
            builder.RegisterType<IohtRepository>().As<IIohtRepository>();

            //2020-03-12 Tai Le(Thomas)  
            builder.RegisterType<OPS_DAL.CuttingPlanService.CutTicketService>().As<OPS_DAL.CuttingPlanService.ICutTicketService>();
            //::END     2020-03-12 Tai Le(Thomas)  

            var container = builder.Build();

            //DependencyResolver.SetResolver(container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        protected void Application_Error()
        {
            var error = HttpContext.Current.Error;
            if(error != null)
            {
                //2020-03-12 Tai Le(Thomas): disable feature "throw bug to Telegram"
                //Post bug
                //BugClientFactory.CreateBugClient().SendBugAsync("MES", "Unknow", error.Message, error.StackTrace);
            }


            //Dùng để tham khảo

            //if (httpContext != null)
            //{
            //    RequestContext requestContext = ((MvcHandler)httpContext.CurrentHandler).RequestContext;
            //    /* When the request is ajax the system can automatically handle a mistake with a JSON response. 
            //       Then overwrites the default response */
            //    if (requestContext.HttpContext.Request.IsAjaxRequest())
            //    {
            //        httpContext.Response.Clear();
            //        string controllerName = requestContext.RouteData.GetRequiredString("controller");
            //        IControllerFactory factory = ControllerBuilder.Current.GetControllerFactory();
            //        IController controller = factory.CreateController(requestContext, controllerName);
            //        ControllerContext controllerContext = new ControllerContext(requestContext, (ControllerBase)controller);

            //        JsonResult jsonResult = new JsonResult
            //        {
            //            Data = new { success = false, serverError = "500" },
            //            JsonRequestBehavior = JsonRequestBehavior.AllowGet
            //        };
            //        jsonResult.ExecuteResult(controllerContext);
            //        httpContext.Response.End();
            //    }
            //    else
            //    {
            //        httpContext.Response.Redirect("~/Error");
            //    }
            //}
        }
    }
}
