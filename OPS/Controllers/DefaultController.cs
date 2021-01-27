using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace OPS.Controllers
{
    public class DefaultController : Controller
    {
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        // GET: Default
        public ActionResult Default()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        public ActionResult GetLogByLogin(string sysId, string funcId)
        {
            try
            {
                var actls = ActlBus.GetActlByLogin(sysId, funcId, UserInf.UserName);
                actls = FilterActl(actls);
                return Json(actls, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                var x = ex.ToString();
                return null;
            }
        }

        public ActionResult GetNewStyle()
        {
            try
            {
                DormBus d = new DormBus();
                var nstyles = d.GetNewsStyles();
                return Json(nstyles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var x = ex.ToString();
                return null;
            }
        }

        //Vithv
        public ActionResult GetActlByLog(string sysId, string funcId)
        {
            var users = (Usmt)Session["LoginUser"] ?? new Usmt();
            var actl = ActlBus.GetActlByLog(sysId, funcId, users.UserName);
            //START ADD: HA
            if(actl == null)
            {
                actl = new Actl();
            }
            //END ADD: HA
            return Json(actl, JsonRequestBehavior.AllowGet);
        }


        List<Actl> FilterActl(List<Actl> actls)
        {
            List<Actl> up = new List<Actl>();
            List<Actl> ad = new List<Actl>();
            List<Actl> del = new List<Actl>();
            List<Actl> newlist = new List<Actl>();
            int i = 0;
            foreach (var item in actls)
            {
                // get max item update.
                var maxItem = GetMaxActl(item, actls);
                if(item.TransactionTime >= maxItem.TransactionTime)
                {
                    newlist.Add(item);
                    i++;
                }
                if (i == 15)
                {
                    break;
                }
            }
            return newlist;
        }

        Actl GetMaxActl(Actl actl, List<Actl> actls)
        {
            var result = actls.Where(d => d.RefNo == actl.RefNo).OrderByDescending(d => d.TransactionTime).FirstOrDefault();
            return result;
        }
    }
}