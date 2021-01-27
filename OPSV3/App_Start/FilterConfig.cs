using System.Web;
using System.Web.Mvc;

namespace OPSV3
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
