using OPS.GenericClass;
using OPS.Models;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace OPS.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Note: Show data by Session is not welcom, but in here is advantage !!!
    /// </summary>
    /// <seealso cref="T:System.Web.Mvc.Controller" />
    public class OpsLinkController : Controller
    {

        #region variable
        private readonly PatternBus _pattern;
        private readonly BomtBus _bom;
        string iniTialpattern = ConstantGeneric.IniTialpattern;
        string pattern = "Pattern";
        string statusNotLink = "Not yet Linked";
        string statusLink = "Linked";
        int _tool => (int)Machine.Tool;
        int _machine => (int)Machine.Machine;
        string status = "1";
        bool rsult = false;
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfo(UserInf.RoleID, SystemOpsId, ConstantGeneric.OpLinkMenuId);
        string screenId = ConstantGeneric.ScreenLinking;
        public string SystemOpsId => ConstantGeneric.OpsSystemId;


        public OpsLinkController()
        {
            _pattern = new PatternBus();
            _bom = new BomtBus();
        }
        #endregion variable

        #region Link bomt and pattern

        public ActionResult Index()
        {
            if (Session["LoginUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            Session.Remove("prots");
            Session.Remove("protsUp");
            Session.Remove("current");
            Session.Remove("currentt");
            Session.Remove("optls");
            Session.Remove("machine");
            Session.Remove("currentm");
            Session.Remove("Main");
            Session.Remove("Mainr");
            return View();
        }

        public ActionResult Test()
        {
            var uploadPath = Server.MapPath(ConfigurationManager.AppSettings["ApplicationInsightsWebTracking"]);
            return View();
        }

        public JsonResult GetPatternLink(string styleCode, string styleSize, string styleColorSerial, string revNo,
            string itemcode, string itemColorSerial, string mainItemCode, string mainItemColorSerial)
        {
            try
            {
                var pattern = _pattern.GetPatternByBom(styleCode, styleSize, styleColorSerial, revNo, itemcode,
                    itemColorSerial, mainItemCode, mainItemColorSerial);
                ConvertPattern(pattern);
                return Json(pattern.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetBom(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            try
            {
                var bom = _bom.GetBom(styleCode, styleSize, styleColorSerial, revNo);
                return Json(bom.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new List<Bomt>().ToArray(), JsonRequestBehavior.AllowGet);
            }
        }

        //=====deatail
        public JsonResult GetOpDetail(GridSettings gridRequest, string styleCode, string styleSize, string styleColor,
            string revNo, string opRevNo, string edition, string languageId)
        {
            try
            {
                if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(styleSize)
                    || string.IsNullOrEmpty(styleColor) || string.IsNullOrEmpty(revNo)
                    || string.IsNullOrEmpty(opRevNo) || string.IsNullOrEmpty(edition))
                    return Json(new List<Opdt>(), JsonRequestBehavior.AllowGet);
                //pravite
                var prots = (List<Prot>)Session["prots"];
                if (prots == null)
                {
                    prots = ProtBus.GetProt(styleCode, styleSize, styleColor, revNo, opRevNo, edition);
                    if (prots.Count > 0)
                    {
                        Session["prots"] = prots;
                    }
                }
                var p = new Prot
                {
                    StyleCode = styleCode,
                    StyleSize = styleSize,
                    StyleColorSerial = styleColor,
                    RevNo = revNo,
                    OpRevNo = opRevNo,
                    Edition = edition
                };
                Session["current"] = p;
                //end pravite
                languageId = string.IsNullOrEmpty(languageId) ? "" : languageId;
                var opsDetail = OpdtBus.GetOpDetailByLanguage2(styleCode, styleSize, styleColor, revNo, opRevNo, edition, languageId);
                var opsDetailQ = opsDetail.AsQueryable();
                if (null != gridRequest.where && gridRequest.where.rules.Length > 0)
                {
                    string strWhere = LinqExtensionsMethod.FilterNullExpression(gridRequest);
                    opsDetailQ = string.IsNullOrEmpty(strWhere) ? opsDetailQ : opsDetailQ.Where(strWhere);

                    strWhere = LinqExtensionsMethod.GetAllStringFiltersExpression(gridRequest);
                    opsDetailQ = string.IsNullOrEmpty(strWhere) ? opsDetailQ : opsDetailQ.Where(strWhere);

                    opsDetail = opsDetailQ.ToList();
                }

                //opsDetailQ = opsDetailQ?.OrderBy(gridRequest.sortColumn, gridRequest.sortOrder);
                // pravite
                GetContainChildList(opsDetail);
                //end pravite
                if (gridRequest.sortColumn.Split(' ')[0] == "ModuleName")
                {
                    //var lstOpDetail = (from t in opsDetail select t).OrderBy(x => x.ModuleName);
                    //Sorting process detail
                    var lstOpDetail = OpdtBus.SortListProcess(opsDetail, true);
                    return Json(lstOpDetail, JsonRequestBehavior.AllowGet);
                }

                //Sorting process by group
                opsDetail = OpdtBus.SortListProcess(opsDetail, false);

                return Json(opsDetail, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exc)
            {
                ErrlBus.InserExceptionLog(exc, UserInf.UserName, screenId, ConstantGeneric.EventGetData, SystemOpsId);
                return Json(exc.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetBomByOp(string styleCode, string styleSize, string styleColor
           , string revNo, string opRevNo, int opSerial, string edition)
        {
            try
            {
                var prots = (List<Prot>)Session["prots"] ?? ProtBus.GetProt(styleCode, styleSize,
                    styleColor, revNo, opRevNo, edition);
                prots = prots.Where(d => d.OpSerial == opSerial).ToList();
                var newProts = GetBomtParent(prots);
                return Json(newProts.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetPatternByBom(string styleCode, string styleSize, string styleColor
           , string revNo, string opRevNo, int opSerial, string edition, string mainItemColorSerial,
            string itemCode, string itemColorSerial, string mainItemCode)
        {
            try
            {
                var prots = (List<Prot>)Session["prots"] ?? ProtBus.GetProt(styleCode, styleSize,
                    styleColor, revNo, opRevNo, edition);
                prots = prots.Where(d => d.OpSerial == opSerial && d.MainItemColorSerial == mainItemColorSerial
                        && d.PatternSerial != iniTialpattern && d.ItemCode == itemCode
                        && d.ItemColorSerial == itemColorSerial && d.MainItemCode == mainItemCode).ToList();
                return Json(prots.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetProtBomPattern(string styleCode, string styleSize, string styleColor
           , string revNo, string opRevNo, int opSerial, string edition)
        {
            try
            {
                var prots = (List<Prot>)Session["prots"] ?? ProtBus.GetProt(styleCode, styleSize,
                    styleColor, revNo, opRevNo, edition);
                prots = prots.Where(d => d.OpSerial == opSerial).ToList();
                return Json(prots.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        #endregion parttern linking

        #region Tools linking

        public JsonResult GetOpDetailTools(string styleCode, string styleSize, string styleColor,
            string revNo, string opRevNo, string edition, string languageId)
        {
            try
            {
                edition = edition.Substring(0, 1);
                var opsDetail = OpdtBus.GetOpDetailByLanguage2(styleCode, styleSize, styleColor, revNo, opRevNo, edition, languageId);
                var tools = (List<Optl>)Session["optls"];
                if (tools == null)
                {
                    //START MOD) SON - 2/Jul/2019 - change function get list  tool linking
                    //tools = OptlBus.GetListOptl(styleCode, styleSize, styleColor, revNo, opRevNo, _tool);
                    tools = OptlBus.GetListOptlByEdition(edition, styleCode, styleSize, styleColor, revNo, opRevNo, _tool);
                    //END MOD) SON - 2/Jul/2019
                    GetPathTools(tools, _tool);
                    if (tools.Count > 0)
                    {
                        Session["optls"] = tools;
                    }
                }
                var o = new Optl
                {
                    StyleCode = styleCode,
                    StyleSize = styleSize,
                    StyleColorSerial = styleColor,
                    RevNo = revNo,
                    OpRevNo = opRevNo,
                    Edition = edition //ADD) SON - 1/Jul/2019
                };
                Session["currentt"] = o;
                GetContainChildDetails(opsDetail, _tool);
                return Json(opsDetail.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetOpDetailMachine(string styleCode, string styleSize, string styleColor,
            string revNo, string opRevNo, string edition, string languageId)
        {
            try
            {
                edition = edition.Substring(0, 1);
                var opsDetail = OpdtBus.GetOpDetailByLanguage2(styleCode, styleSize, styleColor, revNo
                    , opRevNo, edition, languageId);
                var machine = (List<Optl>)Session["machine"];
                if (machine == null)
                {
                    //START MOD) SON - 2/Jul/2019 - change function get list  tool linking
                    //machine = OptlBus.GetListOptl(styleCode, styleSize, styleColor, revNo, opRevNo, _machine);
                    machine = OptlBus.GetListOptlByEdition(edition, styleCode, styleSize, styleColor, revNo, opRevNo, _machine);
                    //END MOD) SON - 2/Jul/2019

                    GetPathTools(machine, _machine);
                    if (machine.Count > 0)
                    {
                        Session["machine"] = machine;
                    }
                }
                var o = new Optl
                {
                    StyleCode = styleCode,
                    StyleSize = styleSize,
                    StyleColorSerial = styleColor,
                    RevNo = revNo,
                    OpRevNo = opRevNo,
                    Edition = edition //ADD) SON - 2/Jul/2019
                };
                Session["currentm"] = o;
                GetContainChildDetails(opsDetail, 1);
                return Json(opsDetail.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetOtmts(string gId)
        {
            try
            {
                var otmts = OtmtBus.GetOtmtsByCateGid(gId, 0);
                //ConvertTools(otmts, 0);
                return Json(otmts.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetOtmtsMc(string gId)
        {
            try
            {
                var otmts = OtmtBus.GetOtmtsByCateGid(gId, 1);
                //ConvertTools(otmts, 1);
                return Json(otmts.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }

        }

        public JsonResult GetOptl(string styleCode, string styleSize, string styleColor
            , string revNo, string opRevNo, int opSerial, string edition)
        {
            try
            {

                //START MOD) SON - 2/Jul/2019 - Get list of Optl by edition
                //var oplts = (List<Optl>)Session["optls"] ?? OptlBus.GetListOptl(styleCode, styleSize, styleColor, revNo, opRevNo, _tool);
                var oplts = (List<Optl>)Session["optls"] ?? OptlBus.GetListOptlByEdition(edition, styleCode, styleSize, styleColor, revNo, opRevNo, _tool);
                //END MOD) SON - 2/Jul/2019
                oplts = oplts.Where(d => d.OpSerial == opSerial && 
                (d.Edition == edition || d.Edition == "ALL")).ToList();
                if (Session["optls"] == null)
                {
                    GetPathTools(oplts, _tool);
                }
                return Json(oplts.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetOptlMc(string styleCode, string styleSize, string styleColor
            , string revNo, string opRevNo, int opSerial, string edition)
        {
            try
            {
                //START MOD) SON - 2/Jul/2019 - Get list of Optl by edition
                //var oplts = (List<Optl>)Session["machine"] ?? OptlBus.GetListOptl(styleCode, styleSize, styleColor, revNo, opRevNo, _machine);
                var oplts = (List<Optl>)Session["machine"] ?? OptlBus.GetListOptlByEdition(edition, styleCode, styleSize, styleColor, revNo, opRevNo, _machine);
                //END MOD) SON - 2/Jul/2019

                oplts = oplts.Where(d => d.OpSerial == opSerial && (d.Edition == edition || d.Edition == "ALL")).ToList();
                if (Session["machine"] == null)
                {
                    GetPathTools(oplts, _machine);
                }
                return Json(oplts.ToArray(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public JsonResult GetMasterCodeByCode(int isMachine)
        {
            try
            {
                var arrMasterCode = OtmtBus.GetCategroy(isMachine).ToArray();
                return Json(arrMasterCode, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }

        }
        #endregion

        #region Save and Update bomt and pattern
        public int Save()
        {
            try
            {
                int rs;
                var protsNew = (List<Prot>)Session["prots"];
                var prUp = (List<Prot>)Session["protsUp"];
                if (protsNew == null && prUp == null) return 2;
                var p = (Prot)Session["current"];
                var protsCurrent = ProtBus.GetProt(p.StyleCode, p.StyleSize, p.StyleColorSerial, p.RevNo
                    , p.OpRevNo, p.Edition);
                var protsS = GetListSave(protsCurrent, protsNew);
                var protsD = GetListSave(protsNew, protsCurrent);

                if (protsD != null && protsD.Count > 0)
                {
                    rsult = ProtBus.RemoveListProt(protsD);
                    status = CommonUtility.ConvertBoolToString01(rsult);
                    CommonUtility.InsertLogActivity(protsD[0], UserInf, SystemOpsId, screenId, ConstantGeneric.EventDelete, "Delete Pattern Linking.", status);

                    rs = 1;
                }
                else
                    rs = 2;
                if (prUp != null && prUp.Count > 0)
                {
                    rsult = ProtBus.UpdateProt(prUp);
                    status = CommonUtility.ConvertBoolToString01(rsult);
                    CommonUtility.InsertLogActivity(prUp[0], UserInf, SystemOpsId, screenId, ConstantGeneric.EventEdit, "Update Pattern Linking.", status);
                    rs = 1;
                }
                Session.Remove("protsUp");
                Session.Remove("prots");
                Session.Remove("current");
                if (protsS.Count == 0) return rs;
                rsult = ProtBus.AddListProt(protsS);
                status = CommonUtility.ConvertBoolToString01(rsult);
                CommonUtility.InsertLogActivity(protsS[0], UserInf, SystemOpsId, screenId, ConstantGeneric.EventAdd, "Add Pattern Linking.", status);
                return 1;
            }
            catch (Exception ex)
            {
                Session.Remove("protsUp");
                Session.Remove("prots");
                Session.Remove("current");
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int SaveTools()
        {
            try
            {
                int rs;
                SaveToOpdt(true);
                var toolsNew = (List<Optl>)Session["optls"];
                if (toolsNew == null) return 2;
                var p = (Optl)Session["currentt"];
                //START MOD) SON - 2/Jul/2019 - change function get list tool linking
                //var toolsCurrent = OptlBus.GetListOptl(p.StyleCode, p.StyleSize, p.StyleColorSerial, p.RevNo, p.OpRevNo, _tool);
                var toolsCurrent = OptlBus.GetListOptlByEdition(p.Edition, p.StyleCode, p.StyleSize, p.StyleColorSerial, p.RevNo, p.OpRevNo, _tool);
                //END MOD) SON - 2/Jul/2019
                var protsS = GetListSave(toolsCurrent, toolsNew);
                var protsD = GetListSave(toolsNew, toolsCurrent);
                if (protsD.Count > 0)
                {
                    rsult = OptlBus.RemoveTools(protsD);
                    status = CommonUtility.ConvertBoolToString01(rsult);
                    CommonUtility.InsertLogActivity(protsD[0], UserInf, SystemOpsId, screenId, ConstantGeneric.EventDelete, "Delete Tools linking.", status);
                    rs = 1;
                }
                else
                    rs = 2;
                Session.Remove("optls");
                Session.Remove("currentt");
                if (protsS.Count == 0) return rs;
                rsult = OptlBus.AddNewTools(protsS);
                status = CommonUtility.ConvertBoolToString01(rsult);
                CommonUtility.InsertLogActivity(protsS[0], UserInf, SystemOpsId, screenId, ConstantGeneric.EventAdd, "Add Tools linking.", status);
                return 1;
            }
            catch (Exception ex)
            {
                Session.Remove("optls");
                Session.Remove("currentt");
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int SaveMachine()
        {
            try
            {
                int rs;
                SaveToOpdt(false);
                var toolsNew = (List<Optl>)Session["machine"];
                if (toolsNew == null) return 2;
                var p = (Optl)Session["currentm"];
                //START MOD) SON - 2/Jul/2019 - change function get list tool linking
                //var toolsCurrent = OptlBus.GetListOptl(p.StyleCode, p.StyleSize, p.StyleColorSerial, p.RevNo, p.OpRevNo, _machine);
                var toolsCurrent = OptlBus.GetListOptlByEdition(p.Edition, p.StyleCode, p.StyleSize, p.StyleColorSerial, p.RevNo, p.OpRevNo, _machine);
                //END MOD) SON - 2/Jul/2019

                var protsS = GetListSave(toolsCurrent, toolsNew);
                var protsD = GetListSave(toolsNew, toolsCurrent);
                if (protsD.Count > 0)
                {
                    rsult = OptlBus.RemoveTools(protsD);
                    status = CommonUtility.ConvertBoolToString01(rsult);
                    CommonUtility.InsertLogActivity(protsD[0], UserInf, SystemOpsId, screenId, ConstantGeneric.EventAdd, "Remove machine linking", status);
                    rs = 1;
                }
                else
                    rs = 2;
                Session.Remove("machine");
                Session.Remove("currentm");
                if (protsS.Count == 0) return rs;
                rsult = OptlBus.AddNewTools(protsS);
                status = CommonUtility.ConvertBoolToString01(rsult);
                CommonUtility.InsertLogActivity(protsS[0], UserInf, SystemOpsId, screenId, ConstantGeneric.EventAdd, "Add machine linking", status);
                return 1;
            }
            catch (Exception ex)
            {
                Session.Remove("machine");
                Session.Remove("currentm");
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        void SaveToOpdt(bool isTool)
        {
            try
            {
                var lmain = (List<Optl>)Session["Main"];
                if (lmain != null && lmain.Count > 0)
                {
                    foreach (var item in lmain)
                    {
                        OpdtBus.UpdateOpDetail(item, isTool);
                        OptlBus.AddMain(item, isTool);
                    }
                    Session.Remove("Main");
                }
                var lremove = (List<Optl>)Session["Mainr"];
                if (lremove != null && lremove.Count() >= 0)
                {
                    foreach (var item in lremove)
                    {
                        OpdtBus.UpdateOpDetail(item, isTool);
                        //OptlBus.RemoveMain(item);
                    }
                    Session.Remove("Mainr");
                }

            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
                //return;
            }
        }        

        public int AddSessionProts(Prot[] prots)
        {
            if (prots == null) return 0;
            try
            {
                var protData = (List<Prot>)Session["prots"] ?? new List<Prot>();
                var protUp = (List<Prot>)Session["protsUp"] ?? new List<Prot>();
                int count = 0;
                foreach (var item in prots)
                {
                    var protInlist = GetItemInList(item, protData);
                    if (item.PatternSerial != iniTialpattern)
                    {
                        // if existed prot, update piece not add
                        if (protInlist.Count > 0)
                        {
                            var protIn = protInlist.FirstOrDefault();
                            protIn.PieceQty = (short)(item.PieceQty + protIn.PieceQty);
                            protIn.BomOrPattern = protIn.OpType;
                            protUp.Add(protIn);
                        }
                        else
                        {
                            protData.Add(item);
                        }
                        count++;
                    }
                    else
                    {
                        // prot is bomt, if prot hasbeen esixted not add
                        if (protInlist.Count > 0)
                        {
                            continue;
                        }
                        else
                        {
                            protData.Add(item);
                            count++;
                        }
                    }
                }
                //protData.AddRange(prots.ToList());
                Session["protsUp"] = protUp;
                Session["prots"] = protData;
                return count;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int AddSessionOptl(Optl[] tools)
        {
            if (tools == null) return 0;
            var lnew = new List<Optl>();
            try
            {
                Optl main = null;
                var optlData = (List<Optl>)Session["optls"] ?? new List<Optl>();
                foreach (var item in tools)
                {
                    if (!CheckItemInList(item, optlData))
                    {
                        lnew.Add(item);
                    }
                    if (item.MainTool == "1")
                    {
                        main = item;
                    }
                }
                optlData.AddRange(lnew.ToList());
                Session["optls"] = optlData;
                AddMainToSession(main);
                return lnew.Count;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int AddSessionMc(Optl[] tools)
        {
            if (tools == null) return 0;
            var lnew = new List<Optl>();
            try
            {
                Optl main = null;
                var optlData = (List<Optl>)Session["machine"] ?? new List<Optl>();
                foreach (var item in tools)
                {
                    if (!CheckItemInList(item, optlData))
                    {
                        lnew.Add(item);
                    }
                    if (item.MainTool == "1")
                    {
                        main = item;
                    }
                }
                optlData.AddRange(lnew.ToList());
                Session["machine"] = optlData;
                AddMainToSession(main);
                return lnew.Count;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        void AddMainToSession(Optl m)
        {
            if (m == null) return;
            var lmain = (List<Optl>)Session["Main"] ?? new List<Optl>();
            foreach (var child in lmain)
            {
                if (CompareMain(m, child))
                {
                    lmain.Remove(child);
                    break;
                }
            }
            var lmainr = (List<Optl>)Session["Mainr"] ?? new List<Optl>();
            foreach (var item in lmainr)
            {
                if (CompareMain(m, item))
                {
                    lmainr.Remove(item);
                    break;
                }
            }
            lmain.Add(m);
            Session["Main"] = lmain;
            Session["Mainr"] = lmainr;
        }

        void AddMainr(Optl m)
        {
            if (m == null) return;
            var lmainr = (List<Optl>)Session["Mainr"] ?? new List<Optl>();
            foreach (var child in lmainr)
            {
                // if existed remove
                if (CompareMain(m, child))
                {
                    lmainr.Remove(child);
                    break;
                }
            }
            // check it in main add, if existed remove
            var lmain = (List<Optl>)Session["Main"] ?? new List<Optl>();
            foreach (var item in lmain)
            {
                if (CompareMain(m, item))
                {
                    lmain.Remove(item);
                    break;
                }
            }
            lmainr.Add(m);
            Session["Mainr"] = lmainr;
            Session["Main"] = lmain;

        }

        bool CompareMain(Optl m, Optl child)
        {
            return (child.StyleCode == m.StyleCode && child.StyleSize == m.StyleSize
                    && child.StyleColorSerial == m.StyleColorSerial
                    && child.RevNo == m.RevNo && child.OpRevNo == m.OpRevNo && child.OpSerial == m.OpSerial);
        }

        public int SaveMainToSession(Optl optl)
        {
            try
            {
                AddMainToSession(optl);
                return ReportAction.Success;
            }
            catch
            {
                return ReportAction.Error;
            }
        }

        public int AddMainRemove(Optl optl)
        {
            try
            {
                AddMainr(optl);
                return ReportAction.Success;
            }
            catch
            {
                return ReportAction.Error;
            }
        }

        public int RemoveSessionProts(Prot[] prots)
        {
            if (prots == null) return 0;
            //1. if new check in ladd if have you'll remove if dont have add to lremove
            //bool isnew = true;
            var protData = (List<Prot>)Session["prots"];
            foreach (var item in prots)
            {
                if (CheckItemInList(item, protData))
                {
                    Remove(item, protData);
                }
            }
            Session["prots"] = protData;
            return prots.Length;
        }

        public int RemoveSessionTools(Optl[] tools)
        {
            if (tools == null) return 0;
            var protData = (List<Optl>)Session["optls"];
            foreach (var item in tools)
            {
                if (CheckItemInList(item, protData))
                {
                    Remove(item, protData);
                }
            }
            Session["optls"] = protData;
            return tools.Length;
        }

        public int RemoveSessionMc(Optl[] tools)
        {
            if (tools == null) return 0;
            var toolsCurent = (List<Optl>)Session["machine"];
            foreach (var item in tools)
            {
                if (CheckItemInList(item, toolsCurent))
                {
                    Remove(item, toolsCurent);
                }
            }
            Session["machine"] = toolsCurent;
            return tools.Length;
        }

        public int UpdateSessionProts(Prot prot)
        {
            if (prot == null) return 0;
            try
            {
                int checkCanlink = CheckePiece(prot);
                if (checkCanlink != 0)
                {
                    return checkCanlink;
                }
                var protData = (List<Prot>)Session["prots"] ?? new List<Prot>();
                var protUp = (List<Prot>)Session["protsUp"] ?? new List<Prot>();
                protUp.Add(prot);
                foreach (var pr in protData)
                {
                    if (!EquaItem(prot, pr)) continue;
                    pr.ConsumpUnit = prot.ConsumpUnit;
                    pr.UnitConsumption = prot.UnitConsumption;
                    pr.PieceQty = prot.PieceQty;
                    pr.OpType = prot.BomOrPattern;
                }
                Session["protsUp"] = protUp;
                Session["prots"] = protData;
                return 0;
            }
            catch (Exception ex)
            {
                var msg = CommonUtility.CreateMessageException(ex.Message, UserInf.UserName, SystemOpsId, ConstantGeneric.EventGetData);
                throw new Exception(msg);
            }
        }

        public int RemoveAll(string styleCode, string styleSize, string styleColor,
            string revNo, string opRevNo, string edition, string type)
        {
            Session.Remove("protsUp");
            Session.Remove("prots");
            Session.Remove("current");
            Session.Remove("currentt");
            Session.Remove("optls");
            Session.Remove("machine");
            Session.Remove("currentm");
            Session.Remove("Main");
            Session.Remove("Mainr");
            if (type == "1")
            {
                var prots = ProtBus.GetProt(styleCode, styleSize, styleColor, revNo, opRevNo, edition);
                if (prots.Count > 0)
                {
                    Session["prots"] = prots;
                }
            }
            if (type == "2")
            {
                //START MOD) SON - 2/Jul/2019 - Get list tool by Edition
                //var tools = OptlBus.GetListOptl(styleCode, styleSize, styleColor, revNo, opRevNo, _tool);
                var tools = OptlBus.GetListOptlByEdition(edition, styleCode, styleSize, styleColor, revNo, opRevNo, _tool);
                //END MOD) SON - 2/Jul/2019
                GetPathTools(tools, _tool);
                if (tools.Count > 0)
                {
                    Session["optls"] = tools;
                }
            }
            if (type == "3")
            {
                //START MOD) SON - 2/Jul/2019 - Get list tool by Edition
                //var machine = OptlBus.GetListOptl(styleCode, styleSize, styleColor, revNo, opRevNo, _machine);
                var machine = OptlBus.GetListOptlByEdition(edition, styleCode, styleSize, styleColor, revNo, opRevNo, _machine);
                //END MOD) SON - 2/Jul/2019
                GetPathTools(machine, _machine);
                if (machine.Count > 1)
                {
                    Session["machine"] = machine;
                }
            }
            return 1;
        }

        List<Prot> GetListSave(List<Prot> lcurent, List<Prot> lnew)
        {
            return lnew.Where(item => !CheckItemInList(item, lcurent)).ToList();
        }

        List<Optl> GetListSave(List<Optl> lcurent, List<Optl> lnew)
        {
            return lnew.Where(item => !CheckItemInList(item, lcurent)).ToList();
        }

        private bool CheckLinked(List<Optl> l, Optl[] tools)
        {
            if (tools == null)
            {
                return false;
            }
            foreach (var item in tools)
            {
                if (CheckItemInList(item, l))
                {
                    return true;
                }
            }
            return false;
        }

        private static void Remove(Prot item, List<Prot> lprots)
        {
            var lnew = new List<Prot>();
            foreach (var p in lprots)
            {
                if (EquaItem(p, item))
                {
                    lnew.Add(p);
                    break;
                }
            }
            foreach (var x in lnew)
            {
                lprots.Remove(x);
            }
        }

        private static void Remove(Optl item, List<Optl> l)
        {
            var lnew = new List<Optl>();
            foreach (var p in l)
            {
                if (EqualItem(p, item))
                {
                    lnew.Add(p);
                    break;
                }
            }
            foreach (var x in lnew)
            {
                l.Remove(x);
            }
        }

        private static bool CheckItemInList(Prot item, List<Prot> lprots)
        {
            if (lprots == null) return false;
            if (item.Edition == "ALL" && item.PatternSerial == "000")
            {
                return lprots.Any(a => item.StyleCode == a.StyleCode
            && item.StyleSize == a.StyleSize
            && item.StyleColorSerial == a.StyleColorSerial
            && item.RevNo == a.RevNo
            && item.ItemCode == a.ItemCode
            && item.ItemColorSerial == a.ItemColorSerial
            && item.MainItemCode == a.MainItemCode
            && item.MainItemColorSerial == a.MainItemColorSerial
            && item.PatternSerial == a.PatternSerial
            && item.OpRevNo == a.OpRevNo
            && item.OpSerial == a.OpSerial
            && item.Edition == a.Edition
            && item.CurPatternSerial == a.CurPatternSerial);
            }
            else
            {
                return lprots.Any(a => item.StyleCode == a.StyleCode
        && item.StyleSize == a.StyleSize
        && item.StyleColorSerial == a.StyleColorSerial
        && item.RevNo == a.RevNo
        && item.ItemCode == a.ItemCode
        && item.ItemColorSerial == a.ItemColorSerial
        && item.MainItemCode == a.MainItemCode
        && item.MainItemColorSerial == a.MainItemColorSerial
        && item.PatternSerial == a.PatternSerial
        && item.OpRevNo == a.OpRevNo
        && item.OpSerial == a.OpSerial
        && item.Edition == a.Edition);
            }
        }

        private static List<Prot> GetItemInList(Prot item, List<Prot> lprots)
        {
            if (lprots == null) return new List<Prot>();
            return lprots.Where(a => item.StyleCode == a.StyleCode
            && item.StyleSize == a.StyleSize
            && item.StyleColorSerial == a.StyleColorSerial
            && item.RevNo == a.RevNo
            && item.ItemCode == a.ItemCode
            && item.ItemColorSerial == a.ItemColorSerial
            && item.MainItemCode == a.MainItemCode
            && item.MainItemColorSerial == a.MainItemColorSerial
            && item.PatternSerial == a.PatternSerial
            && item.OpRevNo == a.OpRevNo
            && item.OpSerial == a.OpSerial).ToList();
            //&& item.OpType == a.OpType);
        }

        private static bool CheckItemInList(Optl item, List<Optl> l)
        {
            if (l == null) return false;
            return l.Any(a => item.StyleCode == a.StyleCode
                                   && item.StyleSize == a.StyleSize
                                   && item.StyleColorSerial == a.StyleColorSerial
                                   && item.RevNo == a.RevNo
                                   && item.ItemCode == a.ItemCode
                                   && item.OpRevNo == a.OpRevNo
                                   && item.OpSerial == a.OpSerial
                                   && item.Edition == a.Edition);
        }

        private static bool CheckBomtInProt(Bomt item, List<Prot> p)
        {
            if (p == null) return false;
            return p.Any(a => item.StyleCode == a.StyleCode
            && item.StyleSize == a.StyleSize
            && item.StyleColorSerial == a.StyleColorSerial
            && item.RevNo == a.RevNo
            && item.ItemCode == a.ItemCode
            && item.ItemColorSerial == a.ItemColorSerial
            && item.MainItemCode == a.MainItemCode
            && item.MainItemColorSerial == a.MainItemColorSerial);
            //&& item.OpType == a.OpType);
        }

        private static bool CheckOtmtInOptl(Otmt ot, List<Optl> tools)
        {
            if (tools == null) return false;
            return tools.Any(p => p.ItemCode == ot.ItemCode);
        }

        private List<Prot> GetPatternInProt(Pattern item, List<Prot> p)
        {
            if (p == null) return new List<Prot>();
            return p.Where(a => item.StyleCode == a.StyleCode
            && item.StyleSize == a.StyleSize
            && item.StyleColorSerial == a.StyleColorSerial
            && item.RevNo == a.RevNo
            && item.ItemCode == a.ItemCode
            && item.ItemColorSerial == a.ItemColorSerial
            && item.MainItemCode == a.MainItemCode
            && item.MainItemColorSerial == a.MainItemColorSerial
            && item.PatternSerial == a.PatternSerial).ToList();
        }

        private List<Prot> GetProtInProt(Prot item, List<Prot> p)
        {
            if (p == null) return new List<Prot>();
            return p.Where(a => item.StyleCode == a.StyleCode
            && item.StyleSize == a.StyleSize
            && item.StyleColorSerial == a.StyleColorSerial
            && item.RevNo == a.RevNo
            && item.ItemCode == a.ItemCode
            && item.ItemColorSerial == a.ItemColorSerial
            && item.MainItemCode == a.MainItemCode
            && item.MainItemColorSerial == a.MainItemColorSerial
            && item.PatternSerial == a.PatternSerial).ToList();
        }

        private static bool EquaItem(Prot item, Prot a)
        {
            return item.StyleCode == a.StyleCode
            && item.StyleSize == a.StyleSize
            && item.StyleColorSerial == a.StyleColorSerial
            && item.RevNo == a.RevNo
            && item.ItemCode == a.ItemCode
            && item.ItemColorSerial == a.ItemColorSerial
            && item.MainItemCode == a.MainItemCode
            && item.MainItemColorSerial == a.MainItemColorSerial
            && item.PatternSerial == a.PatternSerial
            && item.OpRevNo == a.OpRevNo
            && item.OpSerial == a.OpSerial
            && item.OpType == a.OpType
            && item.Edition == a.Edition;
        }

        private static bool EquaItem2(Prot item, Prot a)
        {
            return item.StyleCode == a.StyleCode
            && item.StyleSize == a.StyleSize
            && item.StyleColorSerial == a.StyleColorSerial
            && item.RevNo == a.RevNo
            && item.ItemCode == a.ItemCode
            && item.ItemColorSerial == a.ItemColorSerial
            && item.MainItemCode == a.MainItemCode
            && item.MainItemColorSerial == a.MainItemColorSerial
            && item.PatternSerial == a.PatternSerial
            && item.OpRevNo == a.OpRevNo
            && item.OpSerial == a.OpSerial
            && item.Edition == a.Edition
           ;
        }

        private static bool EqualItem(Optl item, Optl a)
        {
            return item.StyleCode == a.StyleCode
                   && item.StyleSize == a.StyleSize
                   && item.StyleColorSerial == a.StyleColorSerial
                   && item.RevNo == a.RevNo
                   && item.ItemCode == a.ItemCode
                   && item.OpRevNo == a.OpRevNo
                   && item.OpSerial == a.OpSerial
                   && item.Edition == a.Edition;
        }

        private void GetContainChildList(List<Opdt> l)
        {
            var prots = (List<Prot>)Session["prots"];
            if (prots == null) return;
            OpdtBus.GetDetailConvert(l, prots, "Have");
        }

        private void GetContainChildDetails(List<Opdt> l, int isMachine)
        {
            List<Optl> tools = null;
            if (isMachine == 1)
            {
                tools = (List<Optl>)Session["machine"];
            }
            else
            {
                tools = (List<Optl>)Session["optls"];
            }
            if (tools == null) return;
            GetDetailConvertTools(l, tools, "Have");
        }

        List<Opdt> GetDetailConvertTools(List<Opdt> l, List<Optl> tools, string title)
        {
            var lmain = (List<Optl>)Session["Main"];
            foreach (var item in tools)
            {
                foreach (var child in l)
                {
                    if (item.StyleCode == child.StyleCode
                        && item.StyleSize == child.StyleSize
                        && item.StyleColorSerial == child.StyleColorSerial
                        && item.RevNo == child.RevNo
                        && item.OpRevNo == child.OpRevNo
                        && item.OpSerial == child.OpSerial)
                    {
                        child.NewPrevNo = title;
                    }
                    if (lmain != null)
                    {
                        foreach (var m in lmain)
                        {
                            if (child.StyleCode == m.StyleCode && child.StyleSize == m.StyleSize && child.StyleColorSerial == m.StyleColorSerial
                                && child.RevNo == m.RevNo && child.OpRevNo == m.OpRevNo && child.OpSerial == m.OpSerial)
                            {
                                child.ToolId = m.ItemCode;
                                child.MachineType = m.ItemCode;
                                child.MachineName = m.ItemName;
                            }
                        }
                    }
                }

            }
            return l;
        }

        private Prot CheckProtInBomt(Prot p, List<Prot> prots)
        {
            foreach (var a in prots)
            {
                if (a.StyleCode == p.StyleCode
                    && a.StyleSize == p.StyleSize
                    && a.StyleColorSerial == p.StyleColorSerial
                    && a.RevNo == p.RevNo
                    && a.ItemCode == p.ItemCode
                    && a.ItemColorSerial == p.ItemColorSerial
                    && a.MainItemCode == p.MainItemCode
                    && a.MainItemColorSerial == p.MainItemColorSerial)
                {
                    a.BomOrPattern = "";
                    return a;
                }
            }
            return null;
        }

        private List<Prot> GetBomtParent(List<Prot> prots)
        {
            if (prots == null) return null;
            List<Prot> newprots = new List<Prot>();
            foreach (var item in prots)
            {
                newprots.Add((Prot)item.Clone());
            }
            //return newprots;
            var prBom = newprots.Where(d => d.PatternSerial == iniTialpattern).ToList();
            var prPattern = newprots.Where(d => d.PatternSerial != iniTialpattern);
            foreach (var b in prPattern)
            {
                b.BomOrPattern = pattern;
                var bomt = CheckProtInBomt(b, prBom);
                if (bomt != null)
                {
                    bomt.BomOrPattern = pattern;
                }
                else
                {
                    prBom.Add(b);
                }
            }
            return prBom;
        }

        private List<Prot> ConvertProt(List<Prot> prots)
        {
            //remember just for input, out put will processing later
            if (prots == null) return prots;
            List<Prot> newprots = new List<Prot>();
            foreach (var item in prots)
            {
                newprots.Add((Prot)item.Clone());
            }
            //return newprots;
            var prPattern = newprots.Where(d => d.PatternSerial != iniTialpattern);
            var prBom = newprots.Where(d => d.PatternSerial == iniTialpattern);
            foreach (var b in prPattern)
            {
                b.BomOrPattern = "Pattern";
                b.PatternSerial = "";
                foreach (var m in prBom)
                {
                    m.PatternSerial = iniTialpattern;
                    // m will be parrent and has been linked 
                    if (m.StyleCode == b.StyleCode &&
                        m.StyleSize == b.StyleSize &&
                        m.StyleColorSerial == b.StyleColorSerial &&
                        m.RevNo == b.RevNo &&
                        m.ItemCode == b.ItemCode &&
                        m.ItemColorSerial == b.ItemColorSerial &&
                        m.MainItemCode == b.MainItemCode &&
                        m.MainItemColorSerial == b.MainItemColorSerial &&
                        m.OpRevNo == b.OpRevNo &&
                        m.OpSerial == b.OpSerial)
                    {
                        m.PatternSerial = "";
                        m.BomOrPattern = "Pattern";
                    }
                }
            }
            return newprots;
        }

        private void ConvertBomtToBomtStatus(List<Bomt> bomt)
        {
            var protData = (List<Prot>)Session["prots"];
            if (protData == null) return;
            protData = protData.Where(d => d.PatternSerial == iniTialpattern).ToList();
            foreach (var b in bomt)
            {
                b.Status = CheckBomtInProt(b, protData) ? statusLink : statusNotLink;
            }
        }

        private void GetPathTools(List<Optl> opmt, int isMachine)
        {
            var uploadPath = GetPath();
            foreach (var op in opmt)
            {
                var otmt = OtmtBus.GetOtmtsByItemCode(op.ItemCode, isMachine);
                if (otmt != null)
                {
                    op.ImagePath = uploadPath + otmt.ImagePath;
                    op.ItemName = otmt.ItemName;
                    op.CategId = otmt.CategId;
                    op.Category = otmt.Category;
                }
                else
                {
                    // MachineType in t_cm_mcmt is otmt in old project
                    var arrMasterCode = McmtBus.GetMasterCode("MachineType");
                    var mcmt = arrMasterCode.Where(d => d.SubCode == op.ItemCode).FirstOrDefault();
                    if (mcmt != null)
                    {
                        op.ItemName = mcmt.CodeName;
                        op.CategId = mcmt.SubCode;
                        op.Category = mcmt.CodeDesc;
                    }
                }

            }
        }

        private string GetPath()
        {
            return OtmtBus.GetPathImg();
        }
        /*
                private void ConvertTools(List<Otmt> opmt, int isMachine)
                {
                    List<Optl> tools;// (List<Optl>)Session["optls"];
                    if (isMachine == 0)
                    {
                        tools = (List<Optl>)Session["optls"];
                    }
                    else
                    {
                        tools = (List<Optl>)Session["machine"];
                    }
                    //foreach (var op in opmt)
                    //{
                    //    var path = GetPath();
                    //    op.ImagePath = path + op.ImagePath;
                    //}
                }
                */
        private void ConvertPattern(List<Pattern> pt)
        {
            var protData = (List<Prot>)Session["prots"];
            if (protData == null) return;
            protData = protData.Where(d => d.PatternSerial != iniTialpattern).ToList();
            foreach (var p in pt)
            {

                var prot = GetPatternInProt(p, protData);
                if (prot.Count > 0)
                {
                    var total = prot.Sum(d => d.PieceQty);
                    p.PieceQtyRest = (short)(p.PieceQty - total);
                    if (p.PieceQtyRest > 0)
                    {
                        p.Status = statusNotLink;
                    }
                    else
                    {
                        p.Status = statusLink;
                    }
                }
                else
                {
                    p.PieceQtyRest = p.PieceQty;
                    p.Status = statusNotLink;
                }
            }
        }

        private int CheckePiece(Prot prot)
        {
            var currentQty = prot.PieceQty;
            // get current Pattern
            Pattern pattern = _pattern.GetPattern(prot.StyleCode, prot.StyleSize, prot.StyleColorSerial
                , prot.RevNo, prot.ItemCode, prot.ItemColorSerial, prot.PatternSerial
                , prot.MainItemCode, prot.MainItemColorSerial).FirstOrDefault();
            // get total PieceQty not in current prot
            var pieceExitsted = 0;
            var protData = (List<Prot>)Session["prots"];
            if (protData == null) return 0;
            protData = protData.Where(d => d.PatternSerial != iniTialpattern).ToList();
            var prots = GetProtInProt(prot, protData);
            foreach (var pr in prots)
            {
                if (!EquaItem2(prot, pr))
                {
                    pieceExitsted += pr.PieceQty;
                }
            }
            int qty = (pattern?.PieceQty == null) ? 0 : (int)pattern?.PieceQty;

            if (qty - (pieceExitsted + currentQty) >= 0)
            {
                // it OK
                return 0;
            }
            else
            {
                // not OK, validate that if it can not be more than qty - pieceExitsted
                return (qty - pieceExitsted);
            }
        }
        #endregion

    }
}