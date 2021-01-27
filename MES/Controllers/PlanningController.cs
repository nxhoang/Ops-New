using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;

namespace MES.Controllers
{
    [SessionTimeout]
    public class PlanningController : Controller
    {
        public string MenuPlnId => ConstantGeneric.MesPplMenuId;
        public string SystemMesId => ConstantGeneric.MesSystemId;
        public Usmt UserInf => (Usmt)Session["LoginUser"];
        public Srmt Role => SrmtBus.GetUserRoleInfoMySql(UserInf.RoleID, SystemMesId, MenuPlnId);


        // GET: Planning
        public ActionResult Planning()
        {
            ViewBag.PageTitle = "Production Plan";
            return View();
        }

        public JsonResult GetProductionPackage(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            try
            {
                if (string.IsNullOrEmpty(factoryId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(buyer))
                {
                    return Json(new List<Vepp>(), JsonRequestBehavior.AllowGet);
                }

                //Get production package through view
                var lstVeep = VeppBus.GetProductionPackage(factoryId, startDate, endDate, buyer, styleInfo, aoNo);

                return Json(lstVeep, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetProductionPackageByQco(string qcoFactory, string qcoYear, string qcoWeekNo, string buyer, string aoNo, string styleInf)
        {
            try
            {
                if (string.IsNullOrEmpty(qcoFactory) || string.IsNullOrEmpty(qcoYear) || string.IsNullOrEmpty(qcoWeekNo))
                {
                    return Json(new List<QueueEntity>(), JsonRequestBehavior.AllowGet);
                }

                //Get production package through view
                var listQco = QueueBus.GetProductionPackageByQco(qcoFactory, qcoYear, qcoWeekNo, buyer, aoNo, styleInf);

                var startDate = DateTime.Now.ToString("yyyyMMdd");
                var endDate = DateTime.Now.AddDays(30).ToString("yyyyMMdd");
                var listPpkg = MpmtBus.GetProductionPackageGroupByFactoryMES(qcoFactory, startDate, endDate, qcoFactory, null, null, null);
                foreach (var qco in listQco)
                {
                    //find and update remain if qco exist in list production package
                    var ppkg = listPpkg.Find(x => x.PPackage == qco.PrdPkg);
                    if (ppkg != null)
                    {
                        qco.RemainQty = ppkg.RemainQty;
                    }
                }

                //Check remain Qty
                //foreach (var qco in listQco)
                //{
                //    var mpmt = MpmtBus.GetPackageGroupByProductionPackage(qcoFactory, qco.PrdPkg);
                //    if (mpmt != null) qco.RemainQty = mpmt.RemainQty;
                //}

                return Json(listQco, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult IsProductionPackageAvailable(string factoryId, string prdPkg)
        {
            try
            {
                var mpmt = MpmtBus.GetPackageGroupByProductionPackage(factoryId, prdPkg);
                if (mpmt != null)
                {
                    if (mpmt.RemainQty > 0)
                    {
                        return Json("1", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("0", JsonRequestBehavior.AllowGet);
                    }
                }

                return Json("1", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetMESPackage(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            try
            {
                if (string.IsNullOrEmpty(mesFac) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                {
                    return Json(new List<Mpmt>(), JsonRequestBehavior.AllowGet);
                }

                //Get MES package 
                //var listMesPkg = MpdtBus.GetMesPackagesByFactory(mesFac, startDate, endDate, ppFactory, aoNo, buyer,styleInf );

                var listMpmt = MpmtBus.GetPackageGroupByFactory(mesFac, startDate, endDate, ppFactory, aoNo, buyer, styleInf);
                foreach (var mpmt in listMpmt)
                {
                    //mpmt.ListMpdt = MpdtBus.GetMesPackagesByPackageGroup(mpmt.PackageGroup);
                    mpmt.ListMpdt = MpdtBus.GetMesPackagesByPackageGroup(mpmt.PackageGroup).FindAll(x=>int.Parse(x.PlnStartDate) >= int.Parse(startDate));
                    mpmt.ListPpkg = PpkgBus.GetProPackages(mpmt.PackageGroup);
                }

                return Json(listMpmt, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetMesPackageGroup(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            try
            {
                if (string.IsNullOrEmpty(mesFac) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(aoNo))
                {
                    return Json(new List<Mpmt>(), JsonRequestBehavior.AllowGet);
                }

                //Get MES package 
                var listMesPkg = MpdtBus.GetMesPackagesByFactory(mesFac, startDate, endDate, ppFactory, aoNo, buyer, styleInf);

                return Json(listMesPkg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetLinesByProPackage(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            try
            {
                if (string.IsNullOrEmpty(factoryId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(buyer))
                {
                    return Json(new List<Vepp>(), JsonRequestBehavior.AllowGet);
                }

                //Get production package through view
                var lstVeep = VeppBus.GetFactoryLines(factoryId, startDate, endDate, buyer, styleInfo, aoNo);

                return Json(lstVeep, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetLinesByFactoryIdMySql(string factoryId)
        {
            try
            {
                if (string.IsNullOrEmpty(factoryId))
                {
                    return Json(new List<LineEntity>(), JsonRequestBehavior.AllowGet);
                }
                //Get production package through view
                var lines = LineBus.GetLinesByFactoryId(factoryId);

                return Json(lines, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetMESLinesByFactoryMySql(string factory)
        {
            try
            {
                if (string.IsNullOrEmpty(factory))
                {
                    return Json(new List<LineEntity>(), JsonRequestBehavior.AllowGet);
                }
                //Get production package through view
                var lines = LineBus.GetMESLineByFactory(factory);

                return Json(lines, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetMaxMesSeqPkgGroup(string packageGroupId)
        {
            try
            {
                if (string.IsNullOrEmpty(packageGroupId))
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }

                //Get max seq base on package group 
                var seq = MpdtBus.GetMaxMesSeqPackageGroup(packageGroupId);

                return Json(seq, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetMaxPackageGroup(string factoryId, string yymm)
        {
            try
            {
                if (string.IsNullOrEmpty(factoryId) || string.IsNullOrEmpty(yymm))
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }

                //Get max seq base on package group 
                var maxPkgGroup = MpmtBus.GetMaxPackageGroup(factoryId, yymm);

                return Json(maxPkgGroup, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetListPackageGroupByAoNoFactory(string mesFac, string startDate, string endDate, string ppFactory, string aoNo, string buyer, string styleInf)
        {
            try
            {
                if (string.IsNullOrEmpty(mesFac) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(ppFactory))
                {
                    return Json(new List<Ppkg>(), JsonRequestBehavior.AllowGet);
                }

                //Get max seq base on package group  GetProductionPackageByFactory
                var listPp = PpkgBus.GetProductionPackageByFactory(mesFac, startDate, endDate, ppFactory, aoNo, buyer, styleInf);

                //Get status of package group to check whether production package distribued all or not
                var listPkgGroup = MpmtBus.GetPackageGroupByFactory(mesFac, startDate, endDate, ppFactory, aoNo, buyer, styleInf);

                foreach (var mpmt in listPkgGroup)
                {
                    foreach (var pkg in listPp)
                    {
                        if (mpmt.PackageGroup == pkg.PackageGroup)
                        {
                            pkg.DistributeStatus = mpmt.DistributeStatus;
                            //START ADD) SON - 13 Decmber 2019 - Set remain quantity
                            pkg.RemainQty = mpmt.RemainQty;
                            //END ADD) SON - 13 Decmber 2019
                        }
                    }
                }

                return Json(listPp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult SaveMesPackage(List<Mpmt> lstMpmt, List<Ppkg> lstPpkg, List<Mpdt> lstMpdt)
        {
            try
            {
                //Check insert role of user.
                if (Role == null || Role.IsAdd != "1") return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Insert package group and mes pacakges
                var resIns = MpdtBus.InsertPackageGroupAndMesPackages(lstMpmt, lstPpkg, lstMpdt);

                //var resIns = true;
                var strRes = resIns ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //CMP: Create MES packages
                var numMesPkg = "0";
                if (lstMpdt != null) numMesPkg = lstMpdt.Count.ToString();
                InsertActionLog(resIns, "CMP", ConstantGeneric.ActionCreate, lstMpmt == null ? "" : lstMpmt[0].PackageGroup, "Create MES packages (" + numMesPkg + ")");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //CMP: Create MES packages
                InsertActionLog(false, "CMP", ConstantGeneric.ActionCreate, lstMpmt == null ? "" : lstMpmt[0].PackageGroup, ex.Message);

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult UpdateMesPackage(List<Mpmt> lstMpmt, List<Ppkg> lstPpkg, List<Mpdt> lstMpdt, List<Mpdt> lstMesPkgUpd, List<Mpdt> listMesPkdDel, List<Mpmt> listMpmt)
        {
            try
            {
                //Check insert role of user.
                if (Role == null || Role.IsUpdate != "1") return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                var resUdp = MpdtBus.UpdateMesPackage(lstMpmt, lstPpkg, lstMpdt, lstMesPkgUpd, listMesPkdDel, listMpmt);
                if (resUdp)
                {
                    //Delete no used package group
                    var listNoUsedPkgGroup = MpmtBus.GetListNoUsedPackageGroup();
                    if (listNoUsedPkgGroup != null && listNoUsedPkgGroup.Count != 0)
                    {
                        MpmtBus.DeleteListPackageGroupById(listNoUsedPkgGroup);
                    }
                }

                var strRes = resUdp == true ? ConstantGeneric.Success : ConstantGeneric.Fail;
                var refNo = lstMpmt == null ? "" : lstMpmt[0].PackageGroup.ToString();
                var newPkgGrp = lstMpmt == null ? "0" : lstMpmt.Count.ToString();
                var updPkgGrp = listMpmt == null ? "0" : listMpmt.Count.ToString();

                //UMX: Update MES package
                InsertActionLog(resUdp, "UMX", ConstantGeneric.ActionUpdate, refNo, "Update package group (adding new:" + newPkgGrp + " - update old: " + updPkgGrp + ")");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //UMX: Update MES package
                InsertActionLog(false, "UMX", ConstantGeneric.ActionUpdate, ex.Message, "Update package group");

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult SavePackageGroups(List<Mpmt> lstMpmt)
        {
            try
            {
                //Check insert role of user.
                if (Role == null || Role.IsAdd != "1") return Json(ConstantGeneric.NoAuthority, JsonRequestBehavior.AllowGet);

                //Insert list of package groups
                var resIns = MpmtBus.InsertListPackageGroups(lstMpmt);
                var strRes = resIns == true ? ConstantGeneric.Success : ConstantGeneric.Fail;

                //CPG: Create package group
                InsertActionLog(resIns, "CPG", ConstantGeneric.ActionCreate, lstMpmt == null ? "" : lstMpmt[0].PackageGroup, "Create package group (" + lstMpmt.Count() + ")");

                return Json(strRes, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //CPG: Create package group - Failure when creating package group
                InsertActionLog(false, "CPG", ConstantGeneric.ActionCreate, lstMpmt == null ? "" : lstMpmt[0].PackageGroup, ex.Message);

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetDailyTargetFromOPS(string factory, string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            try
            {
                var optime = OpTimeBus.GetStyleOpTime(factory, styleCode, styleSize, styleColorSerial, revNo);
                var dailyTarget = optime == null ? 0 : optime.HourlyTarget;
                return Json(dailyTarget, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //public JsonResult GetCombinationByCorpAndFactory(string corpCode, string factory)
        public JsonResult GetCombinationByCorpAndFactory()
        {
            try
            {
                //var cstp = CstpBus.GetCstpByCorpAndFactory(corpCode, factory);
                var cstp = CstpBus.GetCstpByServerNo(ConstantGeneric.ServerNo);
                return Json(cstp == null ? "N" : cstp.CombinePackage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> CopyStyleInfomationAsync(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            // Start all operations.
            var tasks = new[]
            {
                Task.Run(() => CopyStyleInfomation(styleCode, styleSize, styleColorSerial, revNo))
            };

            // Asynchronously wait for them all to complete.
            // Uncomment below line to not forget the results
            //  var results = await Task.WhenAll(tasks);

            // Return empty string for fire and forget.
            //return View(string.Empty);

            return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CopyStyleInfomation(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            try
            {
                //Get style information in MES MySql
                var mesStyle = StmtBus.GetByStyleCode(styleCode);
                if (mesStyle == null)
                {
                    //Check Style whether exist in MySQL or not
                    var orgStyle = StmtBus.GetStyleInfoByCode(styleCode);
                    //Get list style size
                    var listStlSize = SsmtBus.GetStyleSizeMasterByCode(styleCode);
                    //Get list style color
                    var listStlColors = ScmtBus.GetStyleColorByStyleCode(styleCode);
                    StmtBus.InsertStyleToMESMySql(orgStyle, listStlColors, listStlSize);

                    //Get style image file from MES mySQL
                    var listSfdt = SfdtBus.GetListStyleFileMySql(styleCode);
                    //If there is no style file detail in MySQL then get style file detail from Oracle and intert to MySQL
                    if (listSfdt.Count == 0)
                    {
                        //Get style file detail from ERP(Oracle)
                        var listOrgSfdt = SfdtBus.GetStyleFileByStyleCode(styleCode);
                        if (listOrgSfdt.Count > 0)
                        {
                            //Insert list style file detail to MySQL
                            SfdtBus.InsertListStyleFileMySql(listOrgSfdt);
                        }
                    }
                }

                //Get style in DORM table from Oracle
                var dorm = DormBus.GetDormByCode(styleCode, styleSize, styleColorSerial, revNo);
                if (dorm != null)
                {
                    //Check dorm in MySQL, if it is empty then insert DORM from ERP
                    if (DormBus.GetDormMySQL(styleCode, styleSize, styleColorSerial, revNo) == null)
                    {
                        //Insert DORM
                        DormBus.InsertDORMToMESMySql(dorm);
                    }

                    //Check list of module from MySQL
                    if (SamtBus.GetByStyleCode(styleCode).Count == 0)
                    {
                        //Get list module from ERP
                        var listSamt = SamtBus.GetModulesByCode(styleCode);
                        if (listSamt.Count > 0)
                        {
                            //Insert list of module to MES my SQL
                            SamtBus.InsertListSAMTToMESMySql(listSamt);
                        }
                    }

                    //Get list BOMT
                    var listBomt = BomtBus.GetBOMDetail(styleCode, styleSize, styleColorSerial, revNo);

                    //Get BOMH from MES MySQL, if bomh is empty then get bomh information from ERR and insert to MySQL
                    if (BomhBus.GetBOMHeaderMySQL(styleCode, styleSize, styleColorSerial, revNo) == null)
                    {
                        //Get BOM header from ERP
                        var bomh = BomhBus.GetBOMHeader(styleCode, styleSize, styleColorSerial, revNo);
                        if (bomh != null)
                        {
                            //Get Pattern master
                            var listPtmt = PatternBus.GetPatternByStyleCode(styleCode, styleSize, styleColorSerial, revNo);
                            //Get list moudle BOM
                            var listMbom = MBomBus.GetMBOMByStyleCode(styleCode, styleSize, styleColorSerial, revNo);

                            //Insert BOM information to MES MySQL
                            BomhBus.InsertBOMToMESMySql(bomh, listBomt, listPtmt, listMbom);
                        }
                    }

                    //Copy item color to MySQL
                    CopyItemColorToMySQL(listBomt);
                }

                //Copy files from FTP and locate it on local drive
                CopyFilesFromFTP(styleCode, styleSize, styleColorSerial, revNo);
                //CopyFilesFromFTP("TNF1055", "RGL", "001", "001");

                return Json(ConstantGeneric.Success, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //Copy item color to MySQL DB
        private void CopyItemColorToMySQL(List<Bomt> listBomt)
        {
            //Create list item code which added color.
            var listItemCode = new List<string>();
            foreach (var bomt in listBomt)
            {
                if (!listItemCode.Where(x => x == bomt.ItemCode).Any())
                {
                    //Get list of item color from Oracle.
                    var listItemColorOra = IccmBus.GetListItemCocolor(bomt.ItemCode, null);

                    //Get list of item color in MySQL
                    var listItemColorMySql = IccmBus.GetListItemColorMySql(bomt.ItemCode, null);

                    var newListIccm = new List<Iccm>();
                    foreach (var iccm in listItemColorOra)
                    {
                        //Find item color existed in MySQL or not
                        var iccmMySql = listItemColorMySql.Where(x => x.ItemCode == iccm.ItemCode && x.ItemColorSerial == iccm.ItemColorSerial).FirstOrDefault();
                        if (iccmMySql == null)
                        {
                            //If item color does not exist in MySql DB then adding it to list in order to insert to MySQL
                            newListIccm.Add(iccm);
                        }
                    }

                    //Insert list item color to MySql
                    foreach (var iccm in newListIccm)
                    {
                        IccmBus.InsertItemColorMySQL(iccm);
                        listItemCode.Add(iccm.ItemCode);
                    }
                }
            }
        }

        protected void CopyMaterialImages(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            //Get BOMT
            var listMaterial = BomtBus.GetBOMDetail(styleCode, styleSize, styleColorSerial, revNo);
            var subFol = ConstantGeneric.MaterialFolder + @"\";
            var pkPDMFol = ConstantGeneric.PKMESFolder;
            var localFol = pkPDMFol + subFol;
            //Get FTP information to download file
            var ftp = FtpInfoBus.GetFtpInfo();
            //Keep list of item code in BOMT don't have image
            var listItemImg = new List<string>();
            foreach (var itemCode in listMaterial)
            {
                var jpgImg = itemCode.ItemCode + ".jpg";
                var pngImg = itemCode.ItemCode + ".png";
                if (!System.IO.File.Exists(localFol + jpgImg))
                {
                    listItemImg.Add(jpgImg);
                }

                if (!System.IO.File.Exists(localFol + pngImg))
                {
                    listItemImg.Add(pngImg);
                }
            }

            foreach (var img in listItemImg)
            {
                var ftpFilePath = ftp.FtpRoot + subFol + img;
                if (!ExistFileOnServer(ftpFilePath, ftp.FtpUser, ftp.FtpPass))
                {
                    SaveFileToLocalDrive(localFol, img, ftp.FtpRoot + subFol, ftp.FtpUser, ftp.FtpPass);
                }
            }
        }

        protected void CopyFilesFromFTP(string styleCode, string styleSize, string styleColorSerial, string revNo)
        {
            //Buyer Code
            var buyer = styleCode.Substring(0, 3);
            //Style information: Style Code, Size, Color and Revision
            var styleFolder = styleCode + styleSize + styleColorSerial + revNo;
            var neuStyleFol = styleCode + styleSize + "000" + revNo;
            //Sub folder of style
            var ftpFolSub = buyer + "/" + styleCode + "/" + styleFolder + "/";
            //Local folder path
            var locPath = ConstantGeneric.PKMESFolder;

            //Get FTP information to download file
            var ftp = FtpInfoBus.GetFtpInfo();

            //Check If local folder does not exist then copy files from FTP (BUYER/STYLECODE/STYLE_INFORMATION)
            if (!Directory.Exists(locPath + ftpFolSub))
            {
                DownloadFileFromPDM(locPath, ftp.FtpRoot, ftpFolSub, ftp.FtpUser, ftp.FtpPass);
            }

            //Check neutral local folder(BUYER/STYLECODE/NEUTRAL_STYLE_INFORMATION)         
            var ftpNeuFolSub = buyer + "/" + styleCode + "/" + neuStyleFol + "/";
            if (!Directory.Exists(locPath + ftpNeuFolSub))
            {
                //START ADD) SON (2019.11.04) - 04 November 2019 - Copy files
                DownloadFileFromPDM(locPath, ftp.FtpRoot, ftpNeuFolSub, ftp.FtpUser, ftp.FtpPass);
                //END ADD) SON (2019.11.04) - 04 November 2019 - Copy files
            }

            //Check local style folder (BUYER/STYLE_INFORMATION)
            var ftpStlSubFol = buyer + "/" + styleFolder + "/";
            if (!Directory.Exists(locPath + ftpStlSubFol))
            {
                DownloadFileFromPDM(locPath, ftp.FtpRoot, ftpStlSubFol, ftp.FtpUser, ftp.FtpPass);
            }

            //Check local neutral style folder (BUYER/NEUTRAL_STYLE_INFORMATION)
            var ftpNeuStlSubFol = buyer + "/" + neuStyleFol + "/";
            if (!Directory.Exists(locPath + ftpNeuStlSubFol))
            {
                DownloadFileFromPDM(locPath, ftp.FtpRoot, ftpNeuStlSubFol, ftp.FtpUser, ftp.FtpPass);
            }

            //Download style image (STYLE/BUYER/STYLECODE/IMAGES)           
            var ftpFolSubStlImg = "style/" + buyer + "/" + styleCode + "/" + "Images/";
            var locStyleImgPath = locPath + ftpFolSubStlImg;
            if (!Directory.Exists(locStyleImgPath))
            {
                //Download style image.
                DownloadFileFromPDM(locPath, ftp.FtpRoot, ftpFolSubStlImg, ftp.FtpUser, ftp.FtpPass);
            }
        }

        protected void DownloadFileFromPDM(string locPath, string ftpFolRoot, string ftpFolSub, string ftpUserName, string ftpPass)
        {
            try
            {
                var ftpFol = ftpFolRoot + ftpFolSub;

                locPath += ftpFolSub;

                var listFleAndFol = GetFilesInFtpDirectory(ftpFol, ftpUserName, ftpPass);
                if (listFleAndFol != null)
                {
                    foreach (var item in listFleAndFol)
                    {
                        if (item.IsFile)
                        {
                            //Save file to local drive
                            SaveFileToLocalDrive(locPath, item.Name, ftpFol, ftpUserName, ftpPass);
                        }
                        else
                        {
                            string subFtpFol = item.Name + @"\";
                            //Recursive folder
                            DownloadFileFromPDM(locPath, ftpFol, subFtpFol, ftpUserName, ftpPass);
                        }
                    }
                }

            }
            catch (WebException ex)
            {
                throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
            }
        }

        public void SaveFileToLocalDrive(string locFolPath, string fileName, string ftpFolder, string ftpUser, string ftpPass)
        {
            try
            {
                //If folder does not exist then creating it.
                if (!Directory.Exists(locFolPath))
                {
                    Directory.CreateDirectory(locFolPath);
                }

                //Create FTP Request.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFolder + fileName);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(ftpUser, ftpPass);
                request.UsePassive = true;
                request.UseBinary = true;
                request.EnableSsl = false;

                //Fetch the Response and read it into a MemoryStream object.
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (Stream fileStream = new FileStream(locFolPath + fileName, FileMode.CreateNew))
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

        }

        public static IEnumerable<FileDto> GetFilesInFtpDirectory(string url, string ftpUserName, string ftpPass)
        {
            try
            {

                //Create FTP Request.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(ftpUserName, ftpPass);
                request.UsePassive = true;
                request.UseBinary = true;
                request.EnableSsl = false;

                //Fetch the Response and read it using StreamReader.
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                List<string> entries = new List<string>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    //Read the Response as String and split using New Line character.
                    entries = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                response.Close();


                var listFiles = new List<FileDto>();

                //Loop and add details of each File to the DataTable.
                foreach (string entry in entries)
                {
                    string[] splits = entry.Split(new string[] { " ", }, StringSplitOptions.RemoveEmptyEntries);

                    //Determine whether entry is for File or Directory.
                    bool isFile = splits[2].Trim() != "<DIR>";
                    //bool isDirectory = splits[2].Substring(0, 1) == "d";

                    //Get file name in case it has white space.
                    var fileName = splits[3].Trim();
                    if (splits.Count() > 4)
                    {
                        for (int i = 4; i < splits.Count(); i++)
                        {
                            fileName += " " + splits[i];
                        }
                    }

                    var file = new FileDto
                    {
                        IsFile = isFile,
                        Name = fileName,
                        Date = string.Join(" ", splits[0], splits[1])
                    };

                    //If entry is for File, add details to DataTable.
                    if (isFile)
                    {
                        //Get file information
                        file.Size = decimal.Parse(splits[2]) / 1024;
                    }

                    listFiles.Add(file);
                }

                return listFiles;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return null;
            }
        }

        private bool ExistFileOnServer(string ftpFilePath, string ftpUserName, string ftpPassword)
        {
            var request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
            request.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return false;
            }
            return false;
        }

        private void InsertActionLog(bool actStatus, string functionId, string operationId, string refNo, string remark)
        {
            var isSuccess = actStatus ? "1" : "0";

            ActlBus.AddTransactionLog(UserInf.UserName, UserInf.RoleID, functionId, operationId, isSuccess, ConstantGeneric.MesPplMenuId, ConstantGeneric.MesSystemId, refNo, remark);

        }

        public ActionResult GetStyleSummary()
        {
            //Get package group parameter from url
            string pkgGroup = Url.RequestContext.HttpContext.Request["PackageGroup"] ?? "";

            //Get style summary by package group
            var mpmt = MpmtBus.GetStyleInfoByPkgGroup(pkgGroup);

            //Return to partial view
            return PartialView("~/Views/PartialView/PkgGroupStyleSummary.cshtml", mpmt);
        }

        public JsonResult GetWorkingTime(string factoryId, string startDate, string endDate)
        {
            //Get year and month of start date and end date
            DateTime sDate = DateTime.ParseExact(startDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime eDate = DateTime.ParseExact(endDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            var startYear = sDate.Year.ToString();
            var endYear = eDate.Year.ToString();

            var startMonth = sDate.Month.ToString("D2");
            var endMonth = eDate.Month.ToString("D2");
                        
            var listWrkTime = new List<Fwts>();

            //If start year and end year are same
            if(startYear == endYear)
            {
                if(startMonth == endMonth)
                {
                    listWrkTime = FwtsBus.GetWorkingTimeOraMES(factoryId, startYear, startMonth, null);
                }
                else
                {
                    for (int i = sDate.Month; i <= eDate.Month; i++)
                    {
                        var listWrkTimeMonth = FwtsBus.GetWorkingTimeOraMES(factoryId, startYear, i.ToString("D2"), null);

                        listWrkTime.AddRange(listWrkTimeMonth);
                    }
                }
            }
            else
            {
                //Get working time of start year from start month to December
                for (int i = sDate.Month; i <= 12; i++)
                {
                    var listWrkTimeMonth = FwtsBus.GetWorkingTimeOraMES(factoryId, startYear, i.ToString("D2"), null);

                    listWrkTime.AddRange(listWrkTimeMonth);
                }

                //Get working time of end year from January to end month
                for (int i = 1; i <= eDate.Month; i++)
                {
                    var listWrkTimeMonth = FwtsBus.GetWorkingTimeOraMES(factoryId, endYear, i.ToString("D2"), null);

                    listWrkTime.AddRange(listWrkTimeMonth);
                }
            }

            //if(startYear == endYear && startMonth == endMonth)
            //{
            //    listWrkTime = FwtsBus.GetWorkingTimeOraMES(factoryId, startYear, startMonth, null);
            //}
            //else
            //{
            //    var listSrtWrkTime = FwtsBus.GetWorkingTimeOraMES(factoryId, startYear, startMonth, null);
            //    var listEndWrkTime = FwtsBus.GetWorkingTimeOraMES(factoryId, endYear, endMonth, null);

            //    listSrtWrkTime.AddRange(listEndWrkTime);

            //    listWrkTime = listSrtWrkTime;
            //}

            return Json(listWrkTime, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListMESPackages(string productionPkgId)
        {
            var listMpdt = new List<Mpdt>();
            //Get list 
            var ppkg = PpkgBus.GetProPackageById(productionPkgId);
            if(ppkg != null)
            {
                //Get list MES package by package group
                listMpdt = MpdtBus.GetMesPackages(ppkg.PackageGroup, string.Empty);
            }

            return Json(listMpdt, JsonRequestBehavior.AllowGet);
        }

        public JsonResult IsScannedMesPackage(string factory, string startDate, string mesPkg)
        {
            var listScannedOp = OpdtMcBus.GetScannedProcess(factory, startDate, mesPkg);

            if(listScannedOp.Count > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        #region function for module tab
        //START ADD - SON) 24/Nov/2020
        public JsonResult GetAomtopsPackagesModule(string factoryId, string startDate, string endDate, string buyer, string styleInfo, string aoNo)
        {
            try
            {
                if (string.IsNullOrEmpty(factoryId) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate) || string.IsNullOrEmpty(buyer))
                {
                    return Json(new List<Vepp>(), JsonRequestBehavior.AllowGet);
                }

                //Get production package through view
                var lstVeep = VeppBus.GetAomtopsPackagesModule(factoryId, startDate, endDate, buyer, styleInfo, aoNo);

                return Json(lstVeep, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetMesPackagesModule(string mesFac, string startDate, string endDate, string aoNo, string buyer, string styleInf)
        {
            try
            {
                if (string.IsNullOrEmpty(mesFac) || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
                {
                    return Json(new List<Mpmt>(), JsonRequestBehavior.AllowGet);
                }

                //Get list of Mes packages
                var listMesPkg = MpdtBus.GetMesPackagesModule(mesFac, startDate, endDate, buyer, styleInf, aoNo);

                return Json(listMesPkg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        public JsonResult GetModulesByStyleCode(string styleCode)
        {
            try
            {                
                //Get list of module by style code
                var listModules = SamtBus.GetModulesByCode(styleCode);

                return Json(listModules, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                throw;
            }
        }
        //END ADD - SON) 24/Nov/2020
        #endregion
    }
}