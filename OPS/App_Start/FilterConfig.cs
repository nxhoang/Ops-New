using System.Web.Mvc;

namespace OPS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //Thai added for system authorization - apply for all controller
            filters.Add(new AuthorizeAttribute());

            filters.Add(new HandleErrorAttribute());
        }
    }
}
