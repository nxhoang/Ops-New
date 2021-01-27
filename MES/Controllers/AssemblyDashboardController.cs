using MES.Hubs;
using MES.Repositories;
using OPS_DAL.MesBus;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class AssemblyDashboardController : Controller
    {
        private readonly IotDataHub _dataHub;

        private readonly IMpdtRepository _mpdtRepo;
        private readonly IIohtRepository _iohtRepo;

        public AssemblyDashboardController(IotDataHub dataHub, IMpdtRepository mpdtRepo, IIohtRepository iohtRepo)
        {
            _dataHub = dataHub;
            _mpdtRepo = mpdtRepo;
            _iohtRepo = iohtRepo;
        }

        // GET: DgsTarget
        public ActionResult Index()
        {
            ViewBag.PageTitle = "Assembly Dashboard";
            var lstFactories = OPS_DAL.MesBus.FcmtBus.GetFactories(string.Empty);
            ViewBag.LstFactories = lstFactories;
            var cstp = CstpBus.GetFirstRecordAsync().Result;
            ViewBag.Cstp = cstp;

            return View();
        }
    }
}