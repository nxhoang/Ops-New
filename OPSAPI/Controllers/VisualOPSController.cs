using OPS_DAL.APIBus;
using OPS_DAL.APIEntities;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace OPSAPI.Controllers
{
    public class VisualOPSController : ApiController
    {

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/pattern")]
        public IHttpActionResult GetBOM(string styleCode, string size, string color, string revision)
        {
            if (string.IsNullOrWhiteSpace(styleCode) || string.IsNullOrWhiteSpace(size) || string.IsNullOrWhiteSpace(color) || string.IsNullOrWhiteSpace(revision))
            {
                return BadRequest("Please input style code, size, color and revision");
            }

            var listBomtAPI = BomtAPIBus.GetBOMDetail(styleCode, size, color, revision);

            if (listBomtAPI.Count == 0) return NotFound();


            foreach (var bomt in listBomtAPI)
            {
                var listPatern = PatternAPIBus.GetPatternByBom(bomt.StyleCode, bomt.StyleSize, bomt.StyleColorSerial, bomt.RevNo, bomt.ItemCode, bomt.ItemColorSerial, bomt.MainItemCode, bomt.MainItemColorSerial);
                if (listPatern.Count != 0)
                {
                    bomt.Patterns = listPatern;

                }
            }

            return Ok(listBomtAPI);
        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/style")]
        public IHttpActionResult GetStyle(int pageIndex, int pageSize, string buyer, string styleCode = null, string size = null, string color = null)
        {
            if (pageIndex == 0 || pageSize == 0)
            {
                return BadRequest("Please input page index and page size");
            }

            //Make sure buyer code is not empty
            if (string.IsNullOrWhiteSpace(buyer))
            {
                return BadRequest("Please input buyer");
            }

            //Get list of style
            var listDorm = DormAPIBus.GetStyles(pageIndex, pageSize, buyer, styleCode, size, color);

            if (listDorm.Count == 0) return NotFound();

            var totalRecords = decimal.ToInt32(listDorm.FirstOrDefault().Total);

            int modes = totalRecords % pageSize == 0 ? 0 : 1;
            int intTotalPage = totalRecords > 0 ? (totalRecords / pageSize) + modes : 1;
            var styles = new
            {
                total = intTotalPage,
                page = pageIndex,
                records = totalRecords,
                rows = from item in listDorm
                       select new
                       {
                           item.StyleCode,
                           item.StyleName,
                           item.BuyerStyleCode,
                           item.BuyerStyleName,
                           item.StyleSize,
                           item.StyleColorSerial,
                           item.RevNo,
                           item.Status,
                           item.RegistryDate,
                           item.RegisterName,
                           item.AdConfirm,
                           item.AdDevSale
                       }
            };

            return Ok(styles);

        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/machine-tool")]
        public IHttpActionResult GetMachineTool(string category = null, string id = null)
        {
            if (string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Please input category or id");
            }

            //Get parameter from url
            var idParam = Request.GetQueryNameValuePairs().Where(m => m.Key == "id").SingleOrDefault().Value;
            var catParam = Request.GetQueryNameValuePairs().Where(m => m.Key == "category").SingleOrDefault().Value;
            //If id and category are not null return bad request
            if (!string.IsNullOrWhiteSpace(idParam) && !string.IsNullOrWhiteSpace(catParam))
            {
                return BadRequest("Please input id or category");
            }

            if (!string.IsNullOrWhiteSpace(idParam))
            {
                //Get machine or tool by id
                var item = OtmtAPIBus.GetMachineToolById(id);
                if (item == null) return NotFound();

                return Ok(item);
            }
            else if (!string.IsNullOrWhiteSpace(catParam))
            {
                var mchList = OtmtAPIBus.GetMachineTools(category);
                if (mchList.Count == 0) return NotFound();

                return Ok(mchList);
            }
            else
            {
                return BadRequest("Please input category or id");
            }
        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/machine-tool-category")]
        public IHttpActionResult GetMachineToolCategries(string type)
        {
            if (type.ToLower() == "machine")
            {
                //Get machine category
                var listMchCat = GetMachineCategories();
                return Ok(listMchCat);
            }
            else if (type.ToLower() == "tool")
            {
                //Get tool category
                var listToolCat = GetToolCategories();
                return Ok(listToolCat);
            }
            else
            {
                return BadRequest("Please input type of category");
            }
        }

        private List<object> GetMachineCategories()
        {
            var nonSewMchList = McmtBus.GetMasterCodeByStauts("NonSewingMc", null);
            var sewMchList = McmtBus.GetMasterCodeByStauts("SewingMc", null);

            var mchList = nonSewMchList.Union(sewMchList);

            var newMchList = new List<object>();
            foreach (var tool in mchList)
            {
                var mcmt = new
                {
                    CategoryID = tool.SubCode,
                    CategoryName = tool.CodeName
                };

                newMchList.Add(mcmt);
            }

            return newMchList;
        }

        private List<object> GetToolCategories()
        {
            var toolList = McmtBus.GetMasterCodeByStauts("OPTool", null);

            var newToolList = new List<object>();
            foreach (var tool in toolList)
            {
                var mcmt = new
                {
                    CategoryID = tool.SubCode,
                    CategoryName = tool.CodeName
                };

                newToolList.Add(mcmt);
            }

            return newToolList;
        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/buyer")]
        public IHttpActionResult GetBuyers()
        {
            var buyerList = McmtBus.GetMasterCodeByStauts("Buyer", "OK");

            var newBuyerList = new List<object>();
            foreach (var buyer in buyerList)
            {
                var byr = new
                {
                    BuyerCode = buyer.SubCode,
                    BuyerName = buyer.CodeName
                };

                newBuyerList.Add(byr);
            }

            return Ok(newBuyerList);
        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/job-type")]
        public IHttpActionResult GetJobType()
        {
            var jobTypeList = McmtBus.GetMasterCodeByStauts("OPType", null);

            var newjobTypeList = new List<object>();
            foreach (var jobType in jobTypeList)
            {
                var jb = new
                {
                    JobTypeCode = jobType.SubCode,
                    JobTypeName = jobType.CodeName
                };

                newjobTypeList.Add(jb);
            }

            return Ok(newjobTypeList);
        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/action-code")]
        public IHttpActionResult GetActionCode()
        {
            var actionCodeList = McmtBus.GetMasterCodeByStauts("ActionCode", null);

            var newActionCodeList = new List<object>();
            foreach (var actionCode in actionCodeList)
            {
                var ac = new
                {
                    ActionCode = actionCode.SubCode,
                    ActionName = actionCode.CodeName
                };

                newActionCodeList.Add(ac);
            }

            return Ok(newActionCodeList);
        }

        #region Operation Plan
        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/operation-plan")]
        public IHttpActionResult GetOperationPlan(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            try
            {
                //Get PDM edition (get the latest operation plan)
                var listOpdt = OpdtAPIBus.GetOperationPlanDetail(styleCode, styleSize, styleColorSerial, revNo, null, "P");
                if (listOpdt.Count() > 0)
                {
                    var opRevNo = listOpdt[0].OpRevNo;

                    //Get list of sub process
                    foreach (var opdt in listOpdt)
                    {
                        //GEt sub process by Operation Plan Serial with default language is VietNamese
                        var listSubPro = OpntAPIBus.GetSubProcess("P", styleCode, styleSize, styleColorSerial, revNo, opRevNo, opdt.OpSerial, null, "V");

                        opdt.ListSubProcess = listSubPro;
                    }

                    //Get operation master
                    var listOpmt = OpmtAPIBus.GetOpMaster(styleCode, styleSize, styleColorSerial, revNo, opRevNo, "P");

                    if (listOpmt.Count() > 0)
                    {
                        listOpmt.FirstOrDefault().ListProcess = listOpdt;

                        return Ok(listOpmt);
                    }
                }

                return Ok(listOpdt);
            }
            catch
            {
                return Ok(new List<OpdtAPI>());
            }

        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/operation-plan")]
        public IHttpActionResult GetOperationPlan(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            try
            {
                //Get PDM edition
                var listOpdt = OpdtAPIBus.GetOperationPlanDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, "P");
                if (listOpdt.Count() > 0)
                {
                    //Get list of sub process
                    foreach (var opdt in listOpdt)
                    {
                        //GEt sub process by Operation Plan Serial with default language is VietNamese
                        var listSubPro = OpntAPIBus.GetSubProcess("P", styleCode, styleSize, styleColorSerial, revNo, opRevNo, opdt.OpSerial, null, "V");

                        opdt.ListSubProcess = listSubPro;
                    }

                    var listOpmt = OpmtAPIBus.GetOpMaster(styleCode, styleSize, styleColorSerial, revNo, opRevNo, "P");

                    if (listOpmt.Count() > 0)
                    {
                        listOpmt.FirstOrDefault().ListProcess = listOpdt;

                        return Ok(listOpmt);
                    }
                }
                return Ok(listOpdt);
            }
            catch
            {
                return Ok(new List<OpdtAPI>());
            }
        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/operation-plan")]
        public IHttpActionResult GetOperationPlan(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string edition)
        {
            try
            {
                //Get PDM edition
                var listOpdt = OpdtAPIBus.GetOperationPlanDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);
                if (listOpdt.Count() > 0)
                {
                    //Get list of sub process
                    foreach (var opdt in listOpdt)
                    {
                        //GEt sub process by Operation Plan Serial with default language is VietNamese
                        var listSubPro = OpntAPIBus.GetSubProcess(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo, opdt.OpSerial, null, "V");

                        opdt.ListSubProcess = listSubPro;
                    }

                    var listOpmt = OpmtAPIBus.GetOpMaster(styleCode, styleSize, styleColorSerial, revNo, opRevNo, edition);

                    if (listOpmt.Count() > 0)
                    {
                        listOpmt.FirstOrDefault().ListProcess = listOpdt;

                        return Ok(listOpmt);
                    }
                }

                return Ok(listOpdt);
            }
            catch
            {
                return Ok(new List<OpdtAPI>());
            }
        }

        [Authorize(Roles = "User, 5551")]
        [HttpGet]
        [Route("api/visualops/module")]
        public IHttpActionResult GetModules(string styleCode, string getAll)
        {
            try
            {
                //Get style information to check valid style code
                var stmt = StmtBus.GetStyleInfoByCode(styleCode);
                if (stmt.StyleCode == null) return Ok(new List<SamtAPI>());

                var buyerCode = styleCode.Substring(0, 3);

                //Check module exist in icmt table or not
                CheckAndInsertItemCodeModule(buyerCode);

                var listModule = new List<SamtAPI>();
                if (getAll == "1")
                {
                    //Check and insert new module to samt table
                    var listInsertMdl = CheckAndInsertNewModule(styleCode);

                    //Get list modules and return
                    listModule = SamtAPIBus.GetNewModules(styleCode);

                    foreach (var mdl in listModule)
                    {
                        var newMdl = listInsertMdl.Find(x => x.ModuleId == mdl.MODULEID);
                        mdl.USED = newMdl == null ? "Y" : "N";
                    }
                }
                else if (getAll == "0")
                {
                    //Get list modules and return
                    listModule = SamtAPIBus.GetNewModules(styleCode);
                }
                else
                {
                    return Ok(new List<SamtAPI>());
                }
                               
                return Ok(listModule);
            }
            catch
            {
                return Ok(new List<SamtAPI>());
            }
        }

        /// <summary>
        /// Insert module item code
        /// </summary>
        /// <param name="buyerCode"></param>
        private void CheckAndInsertItemCodeModule(string buyerCode)
        {
            //Get list of modules by buyer code
            var listModule = IcmtAPIBus.GetItemCodeModules(buyerCode);

            //If module is empty then inserting and get again
            if (listModule.Count == 0)
            {
                //Get maximun item code
                var maxItemCode = IcmtBus.GetMaxItemCodeModule("SUB", buyerCode);
                var itemCodeSerial = int.Parse(maxItemCode.Substring(6));

                var newListIcmt = new List<Icmt>();

                //Get list item level master
                var listIclm = IclmBus.GetItemLevel("SUB", "01");
                foreach (var iclm in listIclm)
                {
                    //Create new item code
                    itemCodeSerial += 1;
                    var newItemCode = "SUB" + buyerCode + itemCodeSerial.ToString("D7");

                    //Create new Item to insert to database.
                    var newIcmt = new Icmt
                    {
                        ItemCode = newItemCode,
                        ItemName = iclm.LevelDesc,
                        MainLevel = iclm.MainLevel,
                        LevelNo01 = iclm.LevelCode,
                        ItemRegister = "22170865",
                        RegistryDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        Buyer = buyerCode
                    };

                    newListIcmt.Add(newIcmt);
                }

                //Insert list of item code.
                IcmtBus.InsertItemCodeList(newListIcmt);
            }
        }

        /// <summary>
        /// Check and insert new module to samt table
        /// </summary>
        /// <param name="styleCode"></param>
        private List<Samt> CheckAndInsertNewModule(string styleCode)
        {
            //Get new list module which do not insert yet.
            var newListModule = SamtBus.GetNewModules(styleCode);
            if (newListModule.Count > 0)
            {
                foreach (var mdl in newListModule)
                {
                    mdl.StyleCode = styleCode;
                    mdl.Registrar = "22170865";
                    mdl.RegistryDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    mdl.FinalAssembly = "0";
                }
                //Insert new list module to samt table.
                SamtBus.InsertModulesList(newListModule);
            }

            return newListModule;
        }
        #endregion
    }
}
