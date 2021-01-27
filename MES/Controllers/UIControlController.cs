using MES.Models;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace MES.Controllers
{
    public class UiControlController : Controller
    {
        readonly DormBus _style;
        readonly BuyerBus _buyer;
        public UiControlController()
        {
            _style = new DormBus();
            _buyer = new BuyerBus();
        }

        // GET: UIControl
        public ActionResult Index(string b, string s, string size, string serial)
        {
            return View();
        }
        public ActionResult Test()
        {
            return View();
        }
        public List<Dorm> SearchStyle(int pageIndex, int pageSize, string buyerCode, string start, string end,string searchText, string aoNumber)
        {
            if(!string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                end = DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            }else if(string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                start = "1900/01/01";
            }
            return _style.SearchStyles(pageIndex, pageSize, buyerCode, start, end, searchText, aoNumber, string.Empty);
        }

        public ActionResult GetList(GridSettings gridRequest)
        {
            List<Dorm> lisData = SearchStyle(gridRequest.pageIndex, gridRequest.pageSize, "", "", "","","");
            decimal totalRecords = 0;
            var firstOrDefault = lisData.FirstOrDefault();

            if (firstOrDefault != null)
            {
                totalRecords = firstOrDefault.Total;
            }
            var lisDataQ = lisData.AsQueryable().OrderBy(gridRequest.sortColumn, gridRequest.sortOrder);
            int pageIndex = gridRequest.pageIndex;
            int pageSize = gridRequest.pageSize;
            int modes = totalRecords % pageSize == 0 ? 0 : 1;
            var intTotalPage = totalRecords > 0 ? (totalRecords / pageSize) + modes : 1;
            var result = new
            {
                total = intTotalPage,
                page = pageIndex,
                records = totalRecords,
                rows = (from item in (lisDataQ)
                        select new
                        {
                            id = item.StyleCode + item.StyleSize + item.RevNo,
                            cell = new[] {
                                    item.StyleCode,
                                    item.StyleName,
                                    item.BuyerStyleCode,
                                    item.BuyerStyleName,
                                    item.StyleSize,
                                    item.StyleColorSerial,
                                    item.RevNo,
                                    item.StaTus,
                                    item.RegistryDate,
                                    item.RegisterName,
                                    item.AdConfirm,
                                    item.AdDevSale
                                }
                        }).ToArray()
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SearchList(GridSettings gridRequest, string buyer="", string start="", string end="", string search="", string aoNumber="")
        {
            try
            {
                decimal totalRecords = 0;
                List<Dorm> lisData = SearchStyle(gridRequest.pageIndex, gridRequest.pageSize, buyer, start, end, search, aoNumber);
                var lisDataQ = lisData.AsQueryable();
                if (null != gridRequest.where && gridRequest.where.rules.Length > 0)
                {
                    string strWhere = LinqExtensionsMethod.FilterNullExpression(gridRequest);
                    if (string.IsNullOrEmpty(strWhere) == false)
                    {
                        var data = lisDataQ.Where(strWhere).ToList();
                        lisDataQ = data.AsQueryable();
                    }
                    strWhere = LinqExtensionsMethod.GetAllStringFiltersExpression(gridRequest);
                    if (string.IsNullOrEmpty(strWhere) == false)
                    {
                        lisDataQ = lisDataQ.Where(strWhere);
                    }
                }
                if (lisData.Count > 0)
                {
                    totalRecords = lisData.ElementAt(0).Total;
                    //StyleCode, StyleSize, StyleColorSerial, RevNo
                    lisDataQ = lisDataQ?.OrderBy("StyleCode", "desc").ThenBy("StyleSize", "desc").ThenBy("StyleColorWays", "asc")
                        .ThenBy("RevNo", "desc").ThenBy(gridRequest.sortColumn, gridRequest.sortOrder);
                }
                int pageIndex = gridRequest.pageIndex;
                int pageSize = gridRequest.pageSize;
                int modes = totalRecords % pageSize == 0 ? 0 : 1;
                var intTotalPage = totalRecords > 0 ? (totalRecords / pageSize) + modes : 1;
                if (lisDataQ != null)
                {
                    var result = new
                    {
                        total = intTotalPage,
                        page = pageIndex,
                        records = totalRecords,
                        rows = lisDataQ.ToArray()
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
           
        }

        public ActionResult ListStyle()
        {
            return View();
        }

        public ActionResult ListBuyer()
        {
            var buyer = _buyer.GetBuyer();
            return Json(buyer, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _SearchStyle()
        {
            return PartialView();
        }
    }
}