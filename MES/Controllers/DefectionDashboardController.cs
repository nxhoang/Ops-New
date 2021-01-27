using MES.Hubs;
using MES.Repositories;
using Newtonsoft.Json;
using OPS_DAL.MesBus;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using PKERP.MES.Domain.Interface.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class DefectionDashboardController : Controller
    {
        private readonly IotDataHub _dataHub;

        private readonly IMpdtRepository _mpdtRepo;
        private readonly IIohtRepository _iohtRepo;

        public DefectionDashboardController(IotDataHub dataHub, IMpdtRepository mpdtRepo, IIohtRepository iohtRepo)
        {
            _dataHub = dataHub;
            _mpdtRepo = mpdtRepo;
            _iohtRepo = iohtRepo;
        }

        // GET: DgsTarget
        public ActionResult Index()
        {
            ViewBag.PageTitle = "Defect Report";
            var lstFactories = OPS_DAL.MesBus.FcmtBus.GetFactories(string.Empty);
            ViewBag.LstFactories = lstFactories;
            var cstp = CstpBus.GetFirstRecordAsync().Result;
            ViewBag.Cstp = cstp;

            var lstBuyerCode = OPS_DAL.MesBus.McmtBus.GetMasterCodeByStauts("Buyer", "OK");
            ViewBag.LstBuyerCodes = lstBuyerCode;

            return View();
        }

        public async Task<ActionResult> Media(int defectItemId)
        {
            ViewBag.PageTitle = "Defect Report";

            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];
            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"defects/media", Method.GET);

            request.AddQueryParameter("defectitemid", defectItemId.ToString());

            var response = await restClient.ExecuteTaskAsync(request);

            var mediaResult = JsonConvert.DeserializeObject<TaskResult<List<DefectMediaDto>>>(response.Content);

            ViewBag.LstImage = mediaResult.Result.Where(m => m.MEDIA_TYPE == "IMAGE").ToList();
            ViewBag.LstAudio = mediaResult.Result.Where(m => m.MEDIA_TYPE == "AUDIO").ToList();
            ViewBag.LstVideo = mediaResult.Result.Where(m => m.MEDIA_TYPE == "VIDEO").ToList();

            return PartialView("_Media");
        }

        public async Task<ContentResult> GetMedia(int defectitemId, string mediatype)
        {
            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];

            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"defects/media", Method.GET);

            request.AddQueryParameter("defectitemId", defectitemId.ToString());
            request.AddQueryParameter("mediatype", mediatype);

            var response = await restClient.ExecuteTaskAsync(request);

            return new ContentResult { Content = response.Content, ContentType = "application/json" };
        }

        public async Task<ContentResult> GetMediaInfo(string objectkey)
        {
            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];

            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"defects/media/{objectkey}", Method.GET);

            var response = await restClient.ExecuteTaskAsync(request);
             
            var result = JsonConvert.DeserializeObject<TaskResult<DefectMediaDto>>(response.Content);
             
            //calc url
            if(result.Result != null)
            {
                string BaseURL = $"http://{HttpContext.Request.Url.Host}:8888/";
                result.Result.URL = $@"{BaseURL}objectstorages/mes/{objectkey}";
                //Origin
                //result.Result.URL = $@"{mesApiLink}/objectstorages/mes/{objectkey}";
            }

            return new ContentResult { Content = JsonConvert.SerializeObject(result), ContentType = "application/json" };
        }

        public async Task<FileResult> Export(string factory, string dt, string buyer, string aoNo)
        {
            var dtValue = DateTime.ParseExact(dt, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];

            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"defects/export", Method.GET);

            request.AddQueryParameter("fromdate", dtValue.ToString("yyyyMMdd"));
            request.AddQueryParameter("todate", dtValue.ToString("yyyyMMdd"));

            if (string.IsNullOrWhiteSpace(factory) == false)
                request.AddQueryParameter("factory", factory);

            if (string.IsNullOrWhiteSpace(buyer) == false)
                request.AddQueryParameter("buyer", buyer);

            if (string.IsNullOrWhiteSpace(aoNo) == false)
                request.AddQueryParameter("aono", aoNo);


            var response = await restClient.ExecuteTaskAsync(request);



            var taskResult = JsonConvert.DeserializeObject<TaskResult<string>>(response.Content);

            var base64text = taskResult.Result;
            var stream = StringHelper.ConvertBase64ToStream(base64text);

            var fileName = $@"defectreport.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", fileName);
        }

        /// <summary>
        /// This function get mes packages by factory and date. It query all package in specify date
        /// Using for production line dashboard
        /// </summary>
        /// <param name="factory">Factory No</param>
        /// <param name="dt">Specify date</param>
        /// <returns></returns>
        public async Task<ContentResult> GetDefectTreeData(string factory, DateTime dt, string buyer, string aoNo)
        {
            var mesApiLink = ConfigurationManager.AppSettings["MESApiLink"];

            var restClient = new RestClient(mesApiLink);
            var request = new RestRequest($@"defects/tree", Method.GET);
            
            request.AddQueryParameter("fromdate", dt.ToString("yyyyMMdd"));
            request.AddQueryParameter("todate", dt.ToString("yyyyMMdd"));

            if (string.IsNullOrWhiteSpace(factory) == false)
                request.AddQueryParameter("factory", factory);

            if (string.IsNullOrWhiteSpace(buyer) == false)
                request.AddQueryParameter("buyer", buyer);

            if (string.IsNullOrWhiteSpace(aoNo) == false)
                request.AddQueryParameter("aono", aoNo);


            var response = await restClient.ExecuteTaskAsync(request);

            return new ContentResult { Content = response.Content, ContentType = "application/json" };
        }


    }
}