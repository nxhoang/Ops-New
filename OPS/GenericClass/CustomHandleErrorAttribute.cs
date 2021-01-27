using System;
using System.Web.Mvc;
using OPS_DAL.Business;

namespace OPS.GenericClass
{
    public class CustomHandleErrorAttribute: HandleErrorAttribute
    {
        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The action-filter context.</param>
        /// Author: Son Nguyen Cao
        public override void OnException(ExceptionContext filterContext)
        {
            try
            {
                // Do not continue if exception already handled
                if (filterContext.ExceptionHandled) return;

                Exception ex = filterContext.Exception;
                // Log Exception ex in database
                var strExc = ex.Message.Substring(0, ex.Message.IndexOf(';'));
                var arrExc = strExc.Split('-');
                if (arrExc.Length < 3) return;
                ErrlBus.InserExceptionLog(ex, arrExc[0], arrExc[1], arrExc[2], "OPS");

                // Notify  admin team

                filterContext.ExceptionHandled = true;

                filterContext.ExceptionHandled = true;

                filterContext.Result = new ViewResult()
                {
                    ViewName = "Error.cshtml"
                };
            }
            catch
            {
                // ignored
            }
        }
    }
}