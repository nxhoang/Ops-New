using MES.Models;
using MES.Repositories;
using Newtonsoft.Json;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_DAL.MtopBus;
using OPS_DAL.MtopEntities;
using OPS_DAL.QCOBus;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class FactoryController : Controller
    {
        private static readonly TableSpaceRepository TableSpaceRepository = new TableSpaceRepository();

        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt FlsRole => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, ConstantGeneric.MesSystemId,
            ConstantGeneric.MesFlsMenuId);

        public JsonResult GetUserRole()
        {
            try
            {
                return Json(new { FlsRole }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Factory
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult OracleSaveChanges(List<TableSpaceEntity> tbsps)
        {
            try
            {
                if (tbsps == null) return Json(false, JsonRequestBehavior.AllowGet);

                var result = FactoryBus.OracleSaveChanges(tbsps);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MySqlSaveChanges(List<TableSpaceEntity> tbsps)
        {
            try
            {
                if (tbsps == null) return Json(false, JsonRequestBehavior.AllowGet);
                var result = FactoryBus.MySqlSaveChanges(tbsps);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult OracleSaveSeatDetail(List<Opdt> opdts)
        {
            try
            {
                if (opdts == null) return Json(false, JsonRequestBehavior.AllowGet);

                var result = FactoryBus.OracleSaveSeatDetail(opdts);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MySqlSaveSeatDetail(List<Opdt> opdts)
        {
            try
            {
                if (opdts == null) return Json(false, JsonRequestBehavior.AllowGet);

                var result = FactoryBus.MySqlSaveSeatDetail(opdts);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveChangeOpst(List<Opst> opsts)
        {
            try
            {
                if (opsts == null)
                {
                    return Json(new { error = "No simulation timeline bar to save." }, JsonRequestBehavior.AllowGet);
                }

                var result = OpstBus.SaveChangeOpst(opsts);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveChangeOpsms(List<Opsm> opsms)
        {
            try
            {
                if (opsms == null)
                {
                    return Json(new { error = "No modules (OP groups) to save." }, JsonRequestBehavior.AllowGet);
                }

                var result = OpsmBus.SaveChangeOpsm(opsms);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveLink(Opls link)
        {
            try
            {
                if (link == null) return Json(false, JsonRequestBehavior.AllowGet);

                var result = OplsBus.SaveLinks(link);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Line-Timeline bar

        [HttpPost]
        public JsonResult SaveOpst(Opst opst)
        {
            try
            {
                if (opst == null) return Json(false, JsonRequestBehavior.AllowGet);

                var result = OpstBus.SaveOpst(opst);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteOpst(int id)
        {
            try
            {
                if (id == 0) return Json(false, JsonRequestBehavior.AllowGet);

                var result = OpstBus.DeleteOpst(id);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetOpstsByMxPackage(string mxPackage)
        {
            try
            {
                var opsts = OpstBus.GetOpstsByMxPackage(mxPackage);
                var jsonResult = Json(new { result = opsts }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Module-Timeline bar

        [HttpPost]
        public JsonResult SaveOpsms(List<Opsm> opsms)
        {
            try
            {
                if (opsms == null) return Json(false, JsonRequestBehavior.AllowGet);

                var result = OpsmBus.SaveOpsms(opsms);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetOpsmsByMxPackage(string mxPackage)
        {
            try
            {
                var opsts = OpsmBus.GetOpsmsByMxPackage(mxPackage);
                var jsonResult = Json(new { result = opsts }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        [HttpPost]
        public JsonResult DeleteLink(Opls link)
        {
            try
            {
                if (link == null) return Json(false, JsonRequestBehavior.AllowGet);

                var result = OplsBus.DeleteLink(link);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetLinksByMxPackage(string mxPackage)
        {
            try
            {
                if (string.IsNullOrEmpty(mxPackage))
                {
                    return Json(new { error = "MxPackage is null or empty." }, JsonRequestBehavior.AllowGet);
                }
                var links = OplsBus.GetLinksByMxPackage(mxPackage);
                var jsonResult = Json(new { result = links }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetTableSpaces(Opmt opmt)
        {
            try
            {
                var tpsps = TableSpaceRepository.MySqlGet(opmt.Factory);
                //var opdts = OpdtBus.GetOpDetailByCode()

                var jsonResult = Json(new { opmt }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MySqlDeleteTable(int tableId)
        {
            try
            {
                var result = TableSpaceBus.MySqlDeleteTable(tableId);
                var table = TableSpaceBus.GetTableById(tableId);
                if (result)
                {
                    // Updating LastUpdated date column to t_cm_fcmt
                    OPS_DAL.MesBus.FcmtBus.UpdateLastUpdated(table.Factory);
                }

                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult OracleDeleteTable(int tableId)
        {
            try
            {
                var result = TableSpaceBus.OracleDeleteTable(tableId);
                var jsonResult = Json(result, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult OracleUpdateSeat(TableSpaceEntity table, string action)
        {
            try
            {
                var result = TableSpaceBus.OracleUpdateSeat(table.TableId, table.SeatTotal, table.VirtualWidth);
                //var table = TableSpaceBus.GetTableById(tableId);
                if (action == "del") OpdtBus.OracleUpdateSeat(table);
                var jsonResult = Json(new { result }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MySqlUpdateSeat(TableSpaceEntity table, string action)
        {
            try
            {
                var result = TableSpaceBus.MySqlUpdateSeat(table.TableId, table.SeatTotal, table.VirtualWidth);
                //var table = TableSpaceBus.GetTableById(tableId);
                if (result)
                {
                    var tbsp = TableSpaceBus.GetTableById((int) table.TableId);
                    // Updating LastUpdated date column to t_cm_fcmt
                    OPS_DAL.MesBus.FcmtBus.UpdateLastUpdated(tbsp.Factory);
                }

                if (action == "del") OpdtBus.MySqlUpdateSeat(table);
                var jsonResult = Json(new { result, table }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MySqlUpdateTable(TableSpaceEntity table)
        {
            try
            {
                var result = TableSpaceBus.MySqlUpdate(table);
                if (result)
                {
                    var updateSeatResult = OpdtBus.MySqlUpdateSeat(table);
                    return Json(new { updateSeatResult }, JsonRequestBehavior.AllowGet);
                }
                var jsonResult = Json(new { result = false }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult OracleUpdateTable(TableSpaceEntity table)
        {
            try
            {
                var result = TableSpaceBus.OracleUpdate(table);
                if (result)
                {
                    var updateSeatResult = OpdtBus.OracleUpdateSeat(table);
                    return Json(new { updateSeatResult }, JsonRequestBehavior.AllowGet);
                }
                var jsonResult = Json(new { result = false }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MySqlUpdateLine(LineEntity line)
        {
            try
            {
                var result = LineBus.MySqlUpdateLine(line);
                var jsonResult = Json(new { result }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult OracleUpdateLine(LineEntity line)
        {
            try
            {
                var result = LineBus.OracleUpdateLine(line);
                var jsonResult = Json(new { result }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateFlsm(Flsm flsm, int tenantId)
        {
            try
            {
                var factory = FactoryBus.GetFlsmByFactory(flsm.FactoryId, tenantId);
                if (factory == null)
                {
                    var resultInsert = FactoryBus.InsertFlsm(flsm, tenantId);
                    return Json(resultInsert.FactoryId != null ? new { result = true } : new { result = false },
                        JsonRequestBehavior.AllowGet);
                }

                var result = FactoryBus.UpdateFlsm(flsm, tenantId);
                var jsonResult = Json(new { result }, JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        #region Methods by Tai Le (Thomas)
        public ActionResult WorkingSheetMng()
        {
            /* Created on: 2019-09-30
             * Creator: Tai Le (Thomas)
             */

            ViewBag.PageTitle = "<i class=\"fa fa-home\"></i>&nbsp;Factory";
            ViewBag.SubPageTitle = "&nbsp;<span>> Factory Working Sheet</span>";

            return View();
        }

        public ActionResult GetWorkingSheet(string factory, string startDate)
        {
            /* Created on: 2019-09-30
             * Creator: Tai Le (Thomas)
             * Purpose: Return the MES Working Sheet based on 
             *      Factory 
             *      Start Date
             */

            if (string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(startDate))
                return Json(new Lwtmw(), JsonRequestBehavior.AllowGet);

            //var listLines = GetListWorkingTimeSheetMtop(factory, startDate);
            //return Json(listLines, JsonRequestBehavior.AllowGet);

            //Get list working time from MES
            var listFwtsMes = FwtsBus.GetLineWorkingTimeFactoryOraMES(factory, startDate.Substring(0, 4), startDate.Substring(4, 2));

            //Create morning, afternoon and over time 
            var listLwtmw = CreateListWorkingTime(listFwtsMes);

            return Json(listLwtmw, JsonRequestBehavior.AllowGet);
        }

        private List<Lwtmw> GetListWorkingTimeSheetMtop(string factory, string startDate)
        {
            /*Author: Son Nguyen Cao*/

            var listLines = CalmstBus.GetLines(factory, startDate);
            if (listLines.Count > 0)
            {
                //Get first line No or any line
                var firstLine = listLines[1].LineNo;
                //Get list working time
                var listCalmst = CalmstBus.GetFacWorkingTimeSheet(factory, startDate);
                //Get list working time of the first line.
                var lineCalmst = listCalmst.Where(x => x.LINENO == firstLine);

                //Create working time object
                var newLwtmw = CreateWorkingTime(lineCalmst);
                //Add working time object at the top to keep working time
                listLines.Insert(0, newLwtmw);

                //Create object line working time machine / worker
                var lwtmwSum = new Lwtmw();

                //Get working machines
                var listWrkman = WrkmanBus.GetWorkingMachines(factory, startDate);
                //Get machine for each line and each date
                foreach (var lwtmwCur in listLines.Skip(1))
                {
                    var lstWrkmanLne = listWrkman.Where(x => x.LINENO == lwtmwCur.LineNo);
                    GetWorkingMachines(lstWrkmanLne, lwtmwCur, lwtmwSum);
                }

                //Add object total machine
                listLines.Add(lwtmwSum);
            }

            return listLines;
        }

        public JsonResult GetFactoryWorkersNumber(string factory, string yymm)
        {
            //Return null object if factory or year and month are empty
            if (string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(yymm))
                return Json(new SuccessTaskResult<FATWRKR>(new FATWRKR()), JsonRequestBehavior.AllowGet);

            var facwrkr = FATWRKRBus.GetFacWorkers(factory, yymm);

            return Json(new SuccessTaskResult<FATWRKR>(facwrkr), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SynchronizeWorkingSheet(string factory, string yyyyMM, string weekNo, string sourceSync)
        {
            if (String.IsNullOrEmpty(factory) || String.IsNullOrEmpty(yyyyMM))
                return Json(new { retResult = false, retMsg = "Factory[" + factory + "] and Month[" + yyyyMM + "] is required. Please check." }, JsonRequestBehavior.AllowGet);


            switch (sourceSync)
            {
                case "AOMTOPS":
                    {
                        var listFlws = FLWSBus.GetLineWorkingSheetFromMtop(factory, yyyyMM, weekNo);

                    }
                    break;
                case "HRM":
                    {
                    }
                    break;
            }

            return Json(new { retResult = true, retMsg = "Sync Working Sheet" }, JsonRequestBehavior.AllowGet);
            //var listFlwsMes = FLWSBus.GetLineWorkingSheetFromMES(factory, yyyyMM, weekNo); 
            //return Json(new SuccessTaskResult<List<FLWS>>(listFlws), JsonRequestBehavior.AllowGet);

        }

        public JsonResult SynchronizeWorkingTime(string factory, string yyyyMM)
        {
            /*Author: Son Nguyen Cao*/
            try
            {
                if (string.IsNullOrEmpty(factory) || string.IsNullOrEmpty(yyyyMM))
                    return Json(new { retResult = false, retMsg = "Factory[" + factory + "] and Month[" + yyyyMM + "] is required. Please check." }, JsonRequestBehavior.AllowGet);

                var yyyy = yyyyMM.Substring(0, 4);
                var mm = yyyyMM.Substring(5, 2);
                var arrFac = factory.Split(',');
                foreach (var fac in arrFac)
                {
                    //Synchronize factory worker                
                    var fawkMes = FawkBus.GetFactoryWorkerFromMES(fac, yyyy, mm);
                    //START MOD - SON) 7/Sep/2020
                    //if (fawkMes == null)
                    //{
                    //    //Get factor woker from Mtop then inserting to MES
                    //    var fawkMtop = FawkBus.GetFactoryWorkerFromMtop(fac, yyyyMM);
                    //    //Synchronize worker factory from Mtop
                    //    FawkBus.InsertFactoryWorkerToMES(fawkMtop);
                    //}

                    var fawkMtop = FawkBus.GetFactoryWorkerFromMtop(fac, yyyyMM);
                    if (fawkMes == null)
                    {
                        //Synchronize worker factory from Mtop
                        FawkBus.InsertFactoryWorkerToMES(fawkMtop);
                    }
                    else
                    {
                        //Update factory worker
                        if(fawkMtop != null)
                        {
                            FawkBus.UpdateFactoryWorkerToMES(fawkMtop);
                        }
                    }
                    //END MOD - SON) 7/Sep/2020

                    //Get list working time sheet from Mtop
                    var listFlwsMtop = FwtsBus.GetListWorkingTimeFromMtop(fac, yyyyMM);
                    //Get list working time from MES
                    var listFwtsMes = FwtsBus.GetLineWorkingTimeFactoryOraMES(fac, yyyyMM.Substring(0, 4), yyyyMM.Substring(5, 2));
                    //If factory working time sheet was synchronized already then update
                    if (listFwtsMes.Count() > 0)
                    {
                        //Update list factory working sheet
                        FwtsBus.UpdateListWorkingTimeOracle(listFlwsMtop);
                    }
                    else
                    {
                        if (listFlwsMtop.Count > 0)
                        {
                            //Insert list of working time to MES
                            FwtsBus.InsertListtWorkingTimeOracle(listFlwsMtop);
                        }
                    }
                }

                //SWT: synchronize working time
                InsertActionLog(true, "SWT", "C", yyyyMM, "Synchronize working time sheet from Mtop");

                ////Get list working time from MES
                //var listFwtsMes = FwtsBus.GetLineWorkingTimeFactoryOraMES(factory, yyyyMM.Substring(0, 4), yyyyMM.Substring(5, 2));
                ////If factory working time sheet was synchronized already then return
                //if (listFwtsMes.Count() > 0)
                //{
                //    //Return if working time sheet was synchronized
                //    return Json(new { retResult = false, retMsg = "Working time sheet was synchronized" }, JsonRequestBehavior.AllowGet);
                //}

                ////Get list working time sheet from Mtop
                //var listFlwsMtop = FwtsBus.GetListWorkingTimeFromMtop(factory, yyyyMM);
                //if (listFlwsMtop.Count > 0)
                //{                    
                //    //Insert list of working time to MES
                //    if (!FwtsBus.InsertListtWorkingTimeOracle(listFlwsMtop))
                //        return Json(new { retResult = false, retMsg = "Error while synchronizing working time" }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new { retResult = false, retMsg = "There is no data to synchronize" }, JsonRequestBehavior.AllowGet);
                //}

                //Return successful if working time sheet was synchronized
                return Json(new { retResult = true, retMsg = "Sync Working Sheet successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { retResult = false, retMsg = "Sync failure: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private void GetWorkingMachines(IEnumerable<Wrkman> listWrkmanLne, Lwtmw lwtmw, Lwtmw lwtmwSum)
        {
            foreach (var wrkman in listWrkmanLne)
            {
                switch (wrkman.PLNDAY)
                {
                    case "01":
                        lwtmw.D1 = wrkman.MANCNT;
                        lwtmwSum.D1 += wrkman.MANCNT;
                        break;
                    case "02":
                        lwtmw.D2 = wrkman.MANCNT;
                        lwtmwSum.D2 += wrkman.MANCNT;
                        break;
                    case "03":
                        lwtmw.D3 = wrkman.MANCNT;
                        lwtmwSum.D3 += wrkman.MANCNT;
                        break;
                    case "04":
                        lwtmw.D4 = wrkman.MANCNT;
                        lwtmwSum.D4 += wrkman.MANCNT;
                        break;
                    case "05":
                        lwtmw.D5 = wrkman.MANCNT;
                        lwtmwSum.D5 += wrkman.MANCNT;
                        break;
                    case "06":
                        lwtmw.D6 = wrkman.MANCNT;
                        lwtmwSum.D6 += wrkman.MANCNT;
                        break;
                    case "07":
                        lwtmw.D7 = wrkman.MANCNT;
                        lwtmwSum.D7 += wrkman.MANCNT;
                        break;
                    case "08":
                        lwtmw.D8 = wrkman.MANCNT;
                        lwtmwSum.D8 += wrkman.MANCNT;
                        break;
                    case "09":
                        lwtmw.D9 = wrkman.MANCNT;
                        lwtmwSum.D9 += wrkman.MANCNT;
                        break;
                    case "10":
                        lwtmw.D10 = wrkman.MANCNT;
                        lwtmwSum.D10 += wrkman.MANCNT;
                        break;
                    case "11":
                        lwtmw.D11 = wrkman.MANCNT;
                        lwtmwSum.D11 += wrkman.MANCNT;
                        break;
                    case "12":
                        lwtmw.D12 = wrkman.MANCNT;
                        lwtmwSum.D12 += wrkman.MANCNT;
                        break;
                    case "13":
                        lwtmw.D13 = wrkman.MANCNT;
                        lwtmwSum.D13 += wrkman.MANCNT;
                        break;
                    case "14":
                        lwtmw.D14 = wrkman.MANCNT;
                        lwtmwSum.D14 += wrkman.MANCNT;
                        break;
                    case "15":
                        lwtmw.D15 = wrkman.MANCNT;
                        lwtmwSum.D15 += wrkman.MANCNT;
                        break;
                    case "16":
                        lwtmw.D16 = wrkman.MANCNT;
                        lwtmwSum.D16 += wrkman.MANCNT;
                        break;
                    case "17":
                        lwtmw.D17 = wrkman.MANCNT;
                        lwtmwSum.D17 += wrkman.MANCNT;
                        break;
                    case "18":
                        lwtmw.D18 = wrkman.MANCNT;
                        lwtmwSum.D18 += wrkman.MANCNT;
                        break;
                    case "19":
                        lwtmw.D19 = wrkman.MANCNT;
                        lwtmwSum.D19 += wrkman.MANCNT;
                        break;
                    case "20":
                        lwtmw.D20 = wrkman.MANCNT;
                        lwtmwSum.D20 += wrkman.MANCNT;
                        break;
                    case "21":
                        lwtmw.D21 = wrkman.MANCNT;
                        lwtmwSum.D21 += wrkman.MANCNT;
                        break;
                    case "22":
                        lwtmw.D22 = wrkman.MANCNT;
                        lwtmwSum.D22 += wrkman.MANCNT;
                        break;
                    case "23":
                        lwtmw.D23 = wrkman.MANCNT;
                        lwtmwSum.D23 += wrkman.MANCNT;
                        break;
                    case "24":
                        lwtmw.D24 = wrkman.MANCNT;
                        lwtmwSum.D24 += wrkman.MANCNT;
                        break;
                    case "25":
                        lwtmw.D25 = wrkman.MANCNT;
                        lwtmwSum.D25 += wrkman.MANCNT;
                        break;
                    case "26":
                        lwtmw.D26 = wrkman.MANCNT;
                        lwtmwSum.D26 += wrkman.MANCNT;
                        break;
                    case "27":
                        lwtmw.D27 = wrkman.MANCNT;
                        lwtmwSum.D27 += wrkman.MANCNT;
                        break;
                    case "28":
                        lwtmw.D28 = wrkman.MANCNT;
                        lwtmwSum.D28 += wrkman.MANCNT;
                        break;
                    case "29":
                        lwtmw.D29 = wrkman.MANCNT;
                        lwtmwSum.D29 += wrkman.MANCNT;
                        break;
                    case "30":
                        lwtmw.D30 = wrkman.MANCNT;
                        lwtmwSum.D30 += wrkman.MANCNT;
                        break;
                    case "31":
                        lwtmw.D31 = wrkman.MANCNT;
                        lwtmwSum.D31 += wrkman.MANCNT;
                        break;
                    default:
                        break;
                }
            }

            lwtmw.ObjectName = "M/C";
            lwtmwSum.ObjectName = "TOTAL M/C";
        }

        //Create object working time
        private Lwtmw CreateWorkingTime(IEnumerable<Calmst> listCalmst)
        {
            var lwtmw = new Lwtmw();
            //Set working time to list
            foreach (var calmst in listCalmst)
            {
                switch (calmst.PLNDAY)
                {
                    case "01":
                        lwtmw.D1 = calmst.REDTME;
                        break;
                    case "02":
                        lwtmw.D2 = calmst.REDTME;
                        break;
                    case "03":
                        lwtmw.D3 = calmst.REDTME;
                        break;
                    case "04":
                        lwtmw.D4 = calmst.REDTME;
                        break;
                    case "05":
                        lwtmw.D5 = calmst.REDTME;
                        break;
                    case "06":
                        lwtmw.D6 = calmst.REDTME;
                        break;
                    case "07":
                        lwtmw.D7 = calmst.REDTME;
                        break;
                    case "08":
                        lwtmw.D8 = calmst.REDTME;
                        break;
                    case "09":
                        lwtmw.D9 = calmst.REDTME;
                        break;
                    case "10":
                        lwtmw.D10 = calmst.REDTME;
                        break;
                    case "11":
                        lwtmw.D11 = calmst.REDTME;
                        break;
                    case "12":
                        lwtmw.D12 = calmst.REDTME;
                        break;
                    case "13":
                        lwtmw.D13 = calmst.REDTME;
                        break;
                    case "14":
                        lwtmw.D14 = calmst.REDTME;
                        break;
                    case "15":
                        lwtmw.D15 = calmst.REDTME;
                        break;
                    case "16":
                        lwtmw.D16 = calmst.REDTME;
                        break;
                    case "17":
                        lwtmw.D17 = calmst.REDTME;
                        break;
                    case "18":
                        lwtmw.D18 = calmst.REDTME;
                        break;
                    case "19":
                        lwtmw.D19 = calmst.REDTME;
                        break;
                    case "20":
                        lwtmw.D20 = calmst.REDTME;
                        break;
                    case "21":
                        lwtmw.D21 = calmst.REDTME;
                        break;
                    case "22":
                        lwtmw.D22 = calmst.REDTME;
                        break;
                    case "23":
                        lwtmw.D23 = calmst.REDTME;
                        break;
                    case "24":
                        lwtmw.D24 = calmst.REDTME;
                        break;
                    case "25":
                        lwtmw.D25 = calmst.REDTME;
                        break;
                    case "26":
                        lwtmw.D26 = calmst.REDTME;
                        break;
                    case "27":
                        lwtmw.D27 = calmst.REDTME;
                        break;
                    case "28":
                        lwtmw.D28 = calmst.REDTME;
                        break;
                    case "29":
                        lwtmw.D29 = calmst.REDTME;
                        break;
                    case "30":
                        lwtmw.D30 = calmst.REDTME;
                        break;
                    case "31":
                        lwtmw.D31 = calmst.REDTME;
                        break;
                    default:
                        break;
                }
            }

            lwtmw.ObjectName = "WORKING TIME";

            return lwtmw;
        }

        private List<Lwtmw> CreateListWorkingTime(IEnumerable<Fwts> listFwts)
        {
            var listlwtmw = new List<Lwtmw>();

            var morningTime = new Lwtmw
            {
                ObjectName = "Morning Time"
            };

            var afternoonTime = new Lwtmw()
            {
                ObjectName = "Afternoon Time"
            };

            var overtime = new Lwtmw()
            {
                ObjectName = "Overtime"
            };

            var totalTime = new Lwtmw()
            {
                ObjectName = "Total Time"
            };

            //Set working time to list
            foreach (var fwts in listFwts)
            {
                switch (fwts.PLANDAY)
                {
                    case "01":
                        morningTime.D1 = fwts.MORNINGTIME;
                        afternoonTime.D1 = fwts.AFTERNOONTIME;
                        overtime.D1 = fwts.OVERTIME;
                        totalTime.D1 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "02":
                        morningTime.D2 = fwts.MORNINGTIME;
                        afternoonTime.D2 = fwts.AFTERNOONTIME;
                        overtime.D2 = fwts.OVERTIME;
                        totalTime.D2 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "03":
                        morningTime.D3 = fwts.MORNINGTIME;
                        afternoonTime.D3 = fwts.AFTERNOONTIME;
                        overtime.D3 = fwts.OVERTIME;
                        totalTime.D3 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "04":
                        morningTime.D4 = fwts.MORNINGTIME;
                        afternoonTime.D4 = fwts.AFTERNOONTIME;
                        overtime.D4 = fwts.OVERTIME;
                        totalTime.D4 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "05":
                        morningTime.D5 = fwts.MORNINGTIME;
                        afternoonTime.D5 = fwts.AFTERNOONTIME;
                        overtime.D5 = fwts.OVERTIME;
                        totalTime.D5 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "06":
                        morningTime.D6 = fwts.MORNINGTIME;
                        afternoonTime.D6 = fwts.AFTERNOONTIME;
                        overtime.D6 = fwts.OVERTIME;
                        totalTime.D6 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "07":
                        morningTime.D7 = fwts.MORNINGTIME;
                        afternoonTime.D7 = fwts.AFTERNOONTIME;
                        overtime.D7 = fwts.OVERTIME;
                        totalTime.D7 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "08":
                        morningTime.D8 = fwts.MORNINGTIME;
                        afternoonTime.D8 = fwts.AFTERNOONTIME;
                        overtime.D8 = fwts.OVERTIME;
                        totalTime.D8 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "09":
                        morningTime.D9 = fwts.MORNINGTIME;
                        afternoonTime.D9 = fwts.AFTERNOONTIME;
                        overtime.D9 = fwts.OVERTIME;
                        totalTime.D9 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "10":
                        morningTime.D10 = fwts.MORNINGTIME;
                        afternoonTime.D10 = fwts.AFTERNOONTIME;
                        overtime.D10 = fwts.OVERTIME;
                        totalTime.D10 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "11":
                        morningTime.D11 = fwts.MORNINGTIME;
                        afternoonTime.D11 = fwts.AFTERNOONTIME;
                        overtime.D11 = fwts.OVERTIME;
                        totalTime.D11 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "12":
                        morningTime.D12 = fwts.MORNINGTIME;
                        afternoonTime.D12 = fwts.AFTERNOONTIME;
                        overtime.D12 = fwts.OVERTIME;
                        totalTime.D12 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "13":
                        morningTime.D13 = fwts.MORNINGTIME;
                        afternoonTime.D13 = fwts.AFTERNOONTIME;
                        overtime.D13 = fwts.OVERTIME;
                        totalTime.D13 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "14":
                        morningTime.D14 = fwts.MORNINGTIME;
                        afternoonTime.D14 = fwts.AFTERNOONTIME;
                        overtime.D14 = fwts.OVERTIME;
                        totalTime.D14 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "15":
                        morningTime.D15 = fwts.MORNINGTIME;
                        afternoonTime.D15 = fwts.AFTERNOONTIME;
                        overtime.D15 = fwts.OVERTIME;
                        totalTime.D15 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "16":
                        morningTime.D16 = fwts.MORNINGTIME;
                        afternoonTime.D16 = fwts.AFTERNOONTIME;
                        overtime.D16 = fwts.OVERTIME;
                        totalTime.D16 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "17":
                        morningTime.D17 = fwts.MORNINGTIME;
                        afternoonTime.D17 = fwts.AFTERNOONTIME;
                        overtime.D17 = fwts.OVERTIME;
                        totalTime.D17 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "18":
                        morningTime.D18 = fwts.MORNINGTIME;
                        afternoonTime.D18 = fwts.AFTERNOONTIME;
                        overtime.D18 = fwts.OVERTIME;
                        totalTime.D18 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "19":
                        morningTime.D19 = fwts.MORNINGTIME;
                        afternoonTime.D19 = fwts.AFTERNOONTIME;
                        overtime.D1 = fwts.OVERTIME;
                        totalTime.D19 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "20":
                        morningTime.D20 = fwts.MORNINGTIME;
                        afternoonTime.D20 = fwts.AFTERNOONTIME;
                        overtime.D20 = fwts.OVERTIME;
                        totalTime.D20 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "21":
                        morningTime.D21 = fwts.MORNINGTIME;
                        afternoonTime.D21 = fwts.AFTERNOONTIME;
                        overtime.D21 = fwts.OVERTIME;
                        totalTime.D21 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "22":
                        morningTime.D22 = fwts.MORNINGTIME;
                        afternoonTime.D22 = fwts.AFTERNOONTIME;
                        overtime.D22 = fwts.OVERTIME;
                        totalTime.D22 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "23":
                        morningTime.D23 = fwts.MORNINGTIME;
                        afternoonTime.D23 = fwts.AFTERNOONTIME;
                        overtime.D23 = fwts.OVERTIME;
                        totalTime.D23 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "24":
                        morningTime.D24 = fwts.MORNINGTIME;
                        afternoonTime.D24 = fwts.AFTERNOONTIME;
                        overtime.D24 = fwts.OVERTIME;
                        totalTime.D24 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "25":
                        morningTime.D25 = fwts.MORNINGTIME;
                        afternoonTime.D25 = fwts.AFTERNOONTIME;
                        overtime.D25 = fwts.OVERTIME;
                        totalTime.D25 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "26":
                        morningTime.D26 = fwts.MORNINGTIME;
                        afternoonTime.D26 = fwts.AFTERNOONTIME;
                        overtime.D26 = fwts.OVERTIME;
                        totalTime.D26 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "27":
                        morningTime.D27 = fwts.MORNINGTIME;
                        afternoonTime.D27 = fwts.AFTERNOONTIME;
                        overtime.D27 = fwts.OVERTIME;
                        totalTime.D27 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "28":
                        morningTime.D28 = fwts.MORNINGTIME;
                        afternoonTime.D28 = fwts.AFTERNOONTIME;
                        overtime.D28 = fwts.OVERTIME;
                        totalTime.D28 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "29":
                        morningTime.D29 = fwts.MORNINGTIME;
                        afternoonTime.D29 = fwts.AFTERNOONTIME;
                        overtime.D29 = fwts.OVERTIME;
                        totalTime.D29 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "30":
                        morningTime.D30 = fwts.MORNINGTIME;
                        afternoonTime.D30 = fwts.AFTERNOONTIME;
                        overtime.D30 = fwts.OVERTIME;
                        totalTime.D30 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    case "31":
                        morningTime.D31 = fwts.MORNINGTIME;
                        afternoonTime.D31 = fwts.AFTERNOONTIME;
                        overtime.D31 = fwts.OVERTIME;
                        totalTime.D31 = fwts.MORNINGTIME + fwts.AFTERNOONTIME + fwts.OVERTIME;
                        break;
                    default:
                        break;
                }
            }

            listlwtmw.Add(morningTime);
            listlwtmw.Add(afternoonTime);
            listlwtmw.Add(overtime);
            listlwtmw.Add(totalTime);

            return listlwtmw;
        }

        public JsonResult ImportWorkingSheetFromMTOPS()
        {
            try
            {
                var jsonResult = Json(new { retResult = true, retMsg = "" },
                    JsonRequestBehavior.AllowGet);

                return jsonResult;
            }
            catch (Exception ex)
            {
                return Json(new { retResult = false, retMsg = "ImportWorkingSheetFromMTOPS ERROR: " + ex.Message },
                    JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult UpdateWeeklyCapacity(string modalWeekCapaFactory, string modalWeekCapaYear, string modalWeekCapaFromWeek, string modalWeekCapaToWeek)
        {
            var arrmodalWeekCapaFactory = modalWeekCapaFactory.Split(';');

            string Msg = "", AccumMsg = "";

            for (int _i = 0; _i < arrmodalWeekCapaFactory.Length; _i++)
            {
                var intmodalWeekCapaFromWeek = Convert.ToInt32(modalWeekCapaFromWeek);
                var intmodalWeekCapaToWeek = Convert.ToInt32(modalWeekCapaToWeek);

                for (int I = intmodalWeekCapaFromWeek; I <= intmodalWeekCapaToWeek; I++)
                {
                    /** Source from MTOPS */
                    //var strSQL = @"
                    //SELECT
                    //    A.* ,
                    //    WeekDays.WeekStartDate ,
                    //    WeekDays.WeekEndDate ,
                    //    B.WORKER ,
                    //    B.SEWER ,
                    //    A.MINWORKTIME * B.WORKER AS MAXCapacity ,
                    //    A.MINWORKTIME * B.SEWER AS SEWCapacity
                    //FROM
                    //    (
                    //    SELECT
                    //        FATOY ,
                    //        MIN(MOTHNO) MOTHNO,
                    //        WEEKNO ,
                    //        SUM(MINWORKTIME) MINWORKTIME ,
                    //        SUM(MAXWORKTIME) MAXWORKTIME
                    //    FROM
                    //        (
                    //        SELECT
                    //            FATOY ,
                    //            MOTHNO ,
                    //            WEEKNO ,
                    //            PLNDAY ,
                    //            MIN(REDTME) AS MINWORKTIME ,
                    //            MAX(REDTME) AS MAXWORKTIME
                    //        FROM
                    //            MT_CALMST_TBL@MTOPSDB A
                    //        WHERE
                    //            A.MOTHNO LIKE :YEAR || '%'
                    //            AND A.WEEKNO = :WEEKNO  
                    //            AND A.FATOY like :FATOY  
                    //            GROUP BY FATOY ,
                    //            MOTHNO ,
                    //            WEEKNO ,
                    //            PLNDAY )MAIN
                    //    GROUP BY
                    //        FATOY ,
                    //        WEEKNO ) A
                    //INNER JOIN MT_FATWRKR_TBL@MTOPSDB B ON
                    //    A.FATOY = B.FATOY
                    //    AND A.MOTHNO = B.MOTHNO
                    //INNER JOIN (
                    //    SELECT
                    //        A.FATOY ,
                    //        A.WEEKNO ,
                    //        MIN(A.MOTHNO || A.PLNDAY ) AS WeekStartDate ,
                    //        MAX(A.MOTHNO || A.PLNDAY ) AS WeekEndDate
                    //    FROM
                    //        MT_CALMST_TBL@MTOPSDB A
                    //    WHERE
                    //        A.MOTHNO LIKE :YEAR || '%'
                    //        AND A.WEEKNO = :WEEKNO
                    //    GROUP BY
                    //        A.FATOY ,
                    //        A.WEEKNO ) WeekDays ON
                    //    A.FATOY = WeekDays.FATOY
                    //    AND A.WEEKNO = WeekDays.WEEKNO 
                    //            ";

                    /** Source from PKMES.T_MX_FWTS */
                    //                    var strSQL = @"
                    //SELECT A.* , 
                    //    B.WeekStartDate , B.WeekEndDate , 
                    //    C.WORKER , C.SEWER , 
                    //    A.MINWORKTIME * C.WORKER AS MAXCapacity , 
                    //    A.MINWORKTIME * C.SEWER AS SEWCapacity
                    //FROM 
                    //( 
                    //    SELECT FACTORY, MIN(MONTHNO) MONTHNO, WEEKNO , SUM(MINWORKTIME) MINWORKTIME , SUM(MAXWORKTIME) MAXWORKTIME
                    //    FROM 
                    //    ( SELECT
                    //        FACTORY ,
                    //        PLANYEAR || PLANMONTH AS MONTHNO ,
                    //        WEEKNO ,
                    //        PLANDAY ,
                    //        MIN(MORNINGTIME + AFTERNOONTIME + OVERTIME) AS MINWORKTIME ,
                    //        MAX(MORNINGTIME + AFTERNOONTIME + OVERTIME) AS MAXWORKTIME
                    //    FROM
                    //        T_MX_FWTS A
                    //    WHERE
                    //        PLANYEAR || PLANMONTH  LIKE :YEAR || '%'
                    //        AND WEEKNO = :WEEKNO
                    //        AND FACTORY LIKE :FATOY
                    //    GROUP BY
                    //        FACTORY ,
                    //        PLANYEAR || PLANMONTH ,
                    //        WEEKNO ,
                    //        PLANDAY
                    //    ) SubA 
                    //    GROUP BY FACTORY,  WEEKNO 
                    //)A  
                    //INNER JOIN 
                    //( SELECT
                    //    FACTORY , 
                    //    PLANYEAR || PLANMONTH AS MONTHNO ,
                    //    WEEKNO , 
                    //    MIN(PLANYEAR || PLANMONTH  || PLANDAY) AS WeekStartDate ,
                    //    MAX(PLANYEAR || PLANMONTH  || PLANDAY) AS WeekEndDate
                    //FROM
                    //    T_MX_FWTS A  
                    //GROUP BY
                    //    FACTORY , 
                    //    PLANYEAR || PLANMONTH   ,
                    //    WEEKNO
                    //) B ON  
                    //A.FACTORY = B.FACTORY 
                    //AND A.MONTHNO = B.MONTHNO
                    //AND A.WEEKNO = B.WEEKNO 
                    //LEFT JOIN PKERP.VIEW_MTOPS_MONTHLYWORKER C ON 
                    //A.FACTORY = C.FACTORY 
                    //AND A.MONTHNO = C.MONTHNO 
                    //";

                    ///2020-06-29 Tai Le(Thomas):
                    ///remove Group By "PLANYEAR || PLANMONTH" inside subquery "B"
                    var strSQL = @"
SELECT A.* , 
    B.WeekStartDate , B.WeekEndDate , 
    C.WORKER , C.SEWER , 
    A.MINWORKTIME * C.WORKER AS MAXCapacity , 
    A.MINWORKTIME * C.SEWER AS SEWCapacity
FROM 
( 
    SELECT FACTORY, MIN(MONTHNO) MONTHNO, WEEKNO , SUM(MINWORKTIME) MINWORKTIME , SUM(MAXWORKTIME) MAXWORKTIME
    FROM 
    ( SELECT
        FACTORY ,
        PLANYEAR || PLANMONTH AS MONTHNO ,
        WEEKNO ,
        PLANDAY ,
        MIN(MORNINGTIME + AFTERNOONTIME + OVERTIME) AS MINWORKTIME ,
        MAX(MORNINGTIME + AFTERNOONTIME + OVERTIME) AS MAXWORKTIME
    FROM
        T_MX_FWTS A
    WHERE
        PLANYEAR || PLANMONTH  LIKE :YEAR || '%'
        AND WEEKNO = :WEEKNO
        AND FACTORY LIKE :FATOY
    GROUP BY
        FACTORY ,
        PLANYEAR || PLANMONTH ,
        WEEKNO ,
        PLANDAY
    ) SubA 
    GROUP BY FACTORY,  WEEKNO 
)A  
INNER JOIN 
( SELECT
    FACTORY , 
    MIN(PLANYEAR || PLANMONTH) AS MONTHNO ,
    WEEKNO , 
    MIN(PLANYEAR || PLANMONTH  || PLANDAY) AS WeekStartDate ,
    MAX(PLANYEAR || PLANMONTH  || PLANDAY) AS WeekEndDate
FROM
    T_MX_FWTS A  
GROUP BY
    FACTORY ,  
    WEEKNO
) B ON  
A.FACTORY = B.FACTORY 
AND A.MONTHNO = B.MONTHNO
AND A.WEEKNO = B.WEEKNO 
LEFT JOIN PKERP.VIEW_MTOPS_MONTHLYWORKER C ON 
A.FACTORY = C.FACTORY 
AND A.MONTHNO = C.MONTHNO 
";
                    if (String.IsNullOrEmpty(modalWeekCapaYear))
                    {
                        modalWeekCapaYear = DateTime.Today.Year.ToString();
                    }

                    List<OracleParameter> parameters = new List<OracleParameter>();
                    parameters.Add(new OracleParameter("YEAR", modalWeekCapaYear));
                    var strWeekNo = ("00" + I);
                    parameters.Add(new OracleParameter("WEEKNO", strWeekNo.Substring(strWeekNo.Length - 2, 2)));

                    //if (String.IsNullOrEmpty(arrmodalWeekCapaFactory[_i]))
                    //    parameters.Add(new OracleParameter("FATOY", "%"));
                    //else
                    parameters.Add(new OracleParameter("FATOY", arrmodalWeekCapaFactory[_i]));

                    var _dt = OPS_DAL.DAL.OracleDbManager.Query(strSQL, parameters.ToArray(), OPS_Utils.ConstantGeneric.ConnectionStrMes);

                    //string AccMsg = "", Msg = "";

                    if (_dt != null)
                        foreach (DataRow _dr in _dt.Rows)
                        {
                            var objFWCP = new OPS_DAL.QCOEntities.FWCP();
                            //objFWCP.FACTORY = _dr["FATOY"].ToString(); //pFactory;
                            objFWCP.FACTORY = _dr["FACTORY"].ToString(); //pFactory;
                            objFWCP.YEAR = Convert.ToInt32(modalWeekCapaYear);
                            objFWCP.WEEKNO = I;

                            objFWCP.STARTDATE = DateTime.ParseExact(_dr["WeekStartDate"].ToString(), "yyyyMMdd", new CultureInfo(""));
                            objFWCP.ENDDATE = DateTime.ParseExact(_dr["WeekEndDate"].ToString(), "yyyyMMdd", new CultureInfo(""));

                            objFWCP.TOTALWORKERS = Convert.ToInt32(_dr["WORKER"]); // Worker of Start Date
                            objFWCP.TOTALSEWER = Convert.ToInt32(_dr["SEWER"]);// Sewer of Start Date

                            objFWCP.CAPACITY = Convert.ToDouble(_dr["MAXCapacity"]);
                            objFWCP.SEWERCAPA = Convert.ToDouble(_dr["SEWCapacity"]);

                            objFWCP.TOTALMACHINES = 0;

                            objFWCP.CREATOR = UserInf.UserName;

                            objFWCP.TOTALWORKHOUR = Convert.ToDouble(_dr["MINWORKTIME"]);

                            var objFWCPBus = new OPS_DAL.QCOBus.FWCPBus();

                            Msg = "";
                            if (objFWCPBus.Update(OPS_Utils.ConstantGeneric.ConnectionStrMes, objFWCP, out Msg))
                                Msg = "<span style=\"color:green;\">" + Msg + "</span>";
                            else
                                Msg = "<span style=\"color:red;\">" + Msg + "</span>";

                            if (String.IsNullOrEmpty(AccumMsg))
                                AccumMsg = Msg;
                            else
                                AccumMsg = AccumMsg + "<BR/>" + Msg;
                        }
                }
            }
            return Json(new { retMsg = AccumMsg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult WeeklyCapacity()
        {
            /* Created on: 2019-09-30
             * Creator: Tai Le (Thomas)
             */
            ViewBag.PageTitle = "<i class=\"fa fa-home\"></i>&nbsp;Factory";
            ViewBag.SubPageTitle = "&nbsp;<span>> Weekly Capacity</span>";
            return View();
        }

        public string WeeklyCapacityGrid(GridSettings gridRequest)
        {
            try
            {
                string strSQL = "",
                    strSQLWhere = "";

                strSQL =
                    " SELECT ROW_NUMBER() OVER(ORDER BY T_CM_FWCP.FACTORY , T_CM_FWCP.YEAR , T_CM_FWCP.WEEKNO) AS RANKING , " +
                    " T_CM_FWCP.* " +
                    " FROM PKMES.T_CM_FWCP " +
                    " ";

                strSQLWhere = " ";

                var _Result = GridData.GetGridData(ConstantGeneric.ConnectionStrMes, strSQL, strSQLWhere, gridRequest);

                return _Result;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return JsonConvert.SerializeObject(new { retResult = false, dataRow = ex.Message });

            }
        }

        #endregion

        #region Record log
        private void InsertActionLog(bool actStatus, string functionId, string operationId, string refNo, string remark)
        {
            var isSuccess = actStatus ? "1" : "0";

            ActlBus.AddTransactionLog(UserInf.UserName, UserInf.RoleID, functionId, operationId, isSuccess, ConstantGeneric.MesWTSMenuId, ConstantGeneric.MesSystemId, refNo, remark);

        }
        #endregion
    }
}