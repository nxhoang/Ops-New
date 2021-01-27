using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace OPS.GenericClass
{
    public class CommonUtility
    {
        /// <summary>
        /// Author: Son Nguyen Cao
        /// Date: 27 July 2017
        /// Convert object to json string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            try
            {
                if (obj == null) return String.Empty;
                var serializer = new JavaScriptSerializer();
                return serializer.Serialize(obj);
            }
            catch (Exception)
            {
                return String.Empty;
            }

        }

        /// <summary>
        /// Remove specific characters
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string GetValidFileName(string fileName)
        {
            // remove any invalid character from the filename.
            //String ret = Regex.Replace(fileName.Trim(), "[^A-Za-z0-9_. ]+", "");
            string ret = Regex.Replace(fileName.Trim(), "[`~!@#$%^&*()+|{}'\":;?/><,]", "");            
            return ret;
            //return ret.Replace(" ", String.Empty);
        }

        /// <summary>
        /// Checks the key code op master.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckKeyCodeOpMaster(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            if (string.IsNullOrEmpty(styleCode)) return false;
            if (string.IsNullOrEmpty(styleSize)) return false;
            if (string.IsNullOrEmpty(styleColorSerial)) return false;
            if (string.IsNullOrEmpty(revNo)) return false;
            return !string.IsNullOrEmpty(opRevNo);
        }

        /// <summary>
        /// Checks the key code op detail.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColorSerial">The style color serial.</param>
        /// <param name="revNo">The rev no.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial">The op serial.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckKeyCodeOpDetail(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial)
        {
            if (!CheckKeyCodeOpMaster(styleCode, styleSize, styleColorSerial, revNo, opRevNo)) return false;

            if (string.IsNullOrEmpty(opSerial)) return false;

            return true;
        }

        /// <summary>
        /// Checks the ops master confirmed.
        /// </summary>
        /// <param name="opDetail">The op detail.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckOpsMasterConfirmed(Opdt opDetail)
        {
            //Get Op master to check confirm
            var opMaster = new Opmt
            {
                StyleCode = opDetail.StyleCode,
                StyleSize = opDetail.StyleSize,
                StyleColorSerial = opDetail.StyleColorSerial,
                RevNo = opDetail.RevNo,
                OpRevNo = opDetail.OpRevNo,
                Edition = opDetail.Edition
            };
            var resOpMaster = OpmtBus.GetOpsMasterByCode(opMaster).FirstOrDefault();
            return resOpMaster != null && resOpMaster.ConfirmChk == ConstantGeneric.ConfirmCheck || resOpMaster == null;
        }

        /// <summary>
        /// Checks the ops master confirmed.
        /// </summary>
        /// <param name="opMaster">The op master.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool CheckOpsMasterConfirmed(Opmt opMaster)
        {
            var resOpMaster = OpmtBus.GetOpsMasterByCode(opMaster).FirstOrDefault();
            return resOpMaster != null && resOpMaster.ConfirmChk == ConstantGeneric.ConfirmCheck || resOpMaster == null;
        }

        /// <summary>
        /// Creates the message exception.
        /// </summary>
        /// <param name="msgExc">The MSG exc.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="sysId">The system identifier.</param>
        /// <param name="funcId">The function identifier.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string CreateMessageException(string msgExc, string userId, string sysId, string funcId)
        {
            return userId + "-" + sysId + "-" + funcId + ";" + msgExc;
        }

        /// <summary>
        /// Inserts the log.
        /// </summary>
        /// <param name="act">The act.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        /// <exception cref="System.Exception"></exception>
        public static bool InsertLog(Actl act)
        {
            try
            {
                return ActlBus.InsertLog(act);
            }
            catch (Exception)
            {
                return false;
                //throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Inserts the log activity with object - Redundant
        /// </summary>
        /// <param name="objOps">Object has key code are StyleCode, StyleSize, StylelColorSerial, RevNo or OpRevNo</param>
        /// <param name="user">The user.</param>
        /// <param name="systemId">The system identifier.</param>
        /// <param name="functionId">The function identifier.</param>
        /// <param name="remark">The remark.</param>
        /// Author: Son Nguyen Cao
        public static void InsertLogActivity(object objOps, Usmt user, string systemId, string functionId, string remark)
        {
            try
            {
                var styleCode = "";
                var styleSize = "";
                var styleColorSerial = "";
                var revNo = "";
                var opRevNo = "";

                foreach (var prop in objOps.GetType().GetProperties())
                {
                    switch (prop.Name)
                    {
                        case "StyleCode":
                            styleCode = prop.GetValue(objOps, null).ToString();
                            break;
                        case "StyleSize":
                            styleSize = prop.GetValue(objOps, null).ToString();
                            break;
                        case "StyleColorSerial":
                            styleColorSerial = prop.GetValue(objOps, null).ToString();
                            break;
                        case "RevNo":
                            revNo = prop.GetValue(objOps, null).ToString();
                            break;
                        case "OpRevNo":
                            opRevNo = prop.GetValue(objOps, null).ToString();
                            break;
                    }
                }

                var refNo = styleCode + styleSize + styleColorSerial + revNo + opRevNo;
                var act = new Actl
                {
                    UserId = user.UserName,
                    FunctionId = functionId,
                    OperationId = systemId,
                    RefNo = refNo,
                    RoleId = user.RoleID,
                    Success = "1",
                    Remark = remark,
                    SystemId = systemId
                };
                ActlBus.InsertLog(act);
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        /// <summary>
        /// Insert log with status
        /// </summary>
        /// <param name="objOps"></param>
        /// <param name="user"></param>
        /// <param name="systemId"></param>
        /// <param name="functionId"></param>
        /// <param name="remark"></param>
        /// <param name="status"></param>
        public static void InsertLogActivity(object objOps, Usmt user, string systemId, string functionId, string operationId, string remark, string status)
        {
            try
            {
                var styleCode = "";
                var styleSize = "";
                var styleColorSerial = "";
                var revNo = "";
                var opRevNo = "";
                var edition = "";
                foreach (var prop in objOps.GetType().GetProperties())
                {
                    switch (prop.Name)
                    {
                        case "StyleCode":
                            styleCode = prop.GetValue(objOps, null).ToString();
                            break;
                        case "StyleSize":
                            styleSize = prop.GetValue(objOps, null).ToString();
                            break;
                        case "StyleColorSerial":
                            styleColorSerial = prop.GetValue(objOps, null).ToString();
                            break;
                        case "RevNo":
                            revNo = prop.GetValue(objOps, null).ToString();
                            break;
                        case "OpRevNo":
                            opRevNo = prop.GetValue(objOps, null).ToString();
                            break;
                        case "Edition":
                            edition = prop.GetValue(objOps, null).ToString();
                            break;
                    }
                }

                var refNo = styleCode + styleSize + styleColorSerial + revNo + opRevNo + edition;
                var act = new Actl
                {
                    UserId = user.UserName,
                    FunctionId = functionId,
                    OperationId = operationId,
                    RefNo = refNo,
                    RoleId = user.RoleID,
                    Success = status,
                    Remark = remark,
                    SystemId = systemId
                };
                ActlBus.InsertLog(act);
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        /// <summary>
        /// Inster log without status - Redundant
        /// </summary>
        /// <param name="strRefNo"></param>
        /// <param name="user"></param>
        /// <param name="systemId"></param>
        /// <param name="functionId"></param>
        /// <param name="remark"></param>
        /// Author: Son Nguyen Cao
        public static void InsertLogActivity(string strRefNo, Usmt user, string systemId, string functionId, string remark)
        {
            try
            {
                var refNo = strRefNo;
                var act = new Actl
                {
                    UserId = user.UserName,
                    FunctionId = functionId,
                    OperationId = systemId,
                    RefNo = refNo,
                    RoleId = user.RoleID,
                    Success = "1",
                    Remark = remark,
                    SystemId = systemId
                };
                ActlBus.InsertLog(act);
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        /// <summary>
        /// Insert log with reference no is a string
        /// </summary>
        /// <param name="strRefNo"></param>
        /// <param name="user"></param>
        /// <param name="systemId"></param>
        /// <param name="functionId"></param>
        /// <param name="remark"></param>
        /// <param name="status"></param>
        /// Author: Son Nguyen Cao.
        public static void InsertLogActivity(string strRefNo, Usmt user, string systemId, string functionId, string operationId, string remark, string status)
        {
            try
            {
                var refNo = strRefNo;

                var act = new Actl
                {
                    UserId = user.UserName,
                    RoleId = user.RoleID,
                    FunctionId = functionId,
                    OperationId = operationId,
                    RefNo = refNo,                   
                    Success = status,
                    Remark = remark,
                    SystemId = systemId
                };
                ActlBus.InsertLog(act);
            }
            catch (Exception)
            {
                //Ignore
            }
        }

        /// <summary>
        /// Creates the name of the system file.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">Color of the style.</param>
        /// <param name="styleRev">The style rev.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial">The op serial.</param>
        /// <param name="extFile">The ext file.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        internal static string CreateSystemFileName(string styleCode, string styleSize, string styleColor, string styleRev,
            string opRevNo, string opSerial, string extFile)
        {
            //Check extention of file
            if (string.IsNullOrEmpty(extFile)) return "";

            //Check Operation Plan serial
            if (string.IsNullOrEmpty(opSerial)) return "";

            //Check selection Operation Plan master
            if (!CheckKeyCodeOpMaster(styleCode, styleSize, styleColor, styleRev, opRevNo)) return "";

            //Create file name.
            var fileName = styleCode + styleSize + styleColor + styleRev + opRevNo + opSerial + extFile;

            return fileName;
        }

        /// <summary>
        /// Creates the string sub folder.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">Color of the style.</param>
        /// <param name="styleRev">The style rev.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial">The op serial.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        internal static string CreateSubFolder(string styleCode, string styleSize, string styleColor, string styleRev,
            string opRevNo, string opSerial)
        {
            //Check selection style
            if (!CheckKeyCodeOpDetail(styleCode, styleSize, styleColor, styleRev, opRevNo, opSerial)) return "";

            //Create string sub folder from Operation Plan master
            var subFolder = $"{styleCode.Substring(0, 3)}/{styleCode}/{styleCode}{styleSize}{styleColor}{styleRev}";

            return subFolder;
        }
        
        /// <summary>
        /// Create reuslt content object
        /// </summary>
        /// <param name="result"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static ResultContent GetResultContent(string result, object content)
        {
            var resCont = new ResultContent
            {
                Result = result,
                Content = content
            };

            return resCont;
        }

        /// <summary>
        /// Creates the string sub folder.
        /// </summary>
        /// <param name="styleCode">The style code.</param>
        /// <param name="styleSize">Size of the style.</param>
        /// <param name="styleColor">Color of the style.</param>
        /// <param name="styleRev">The style rev.</param>
        /// <param name="opRevNo">The op rev no.</param>
        /// <param name="opSerial">The op serial.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string CreateStringSubFolder(string styleCode, string styleSize, string styleColor, string styleRev, string opRevNo, string opSerial, string edition)
        {
            //Check selection style
            if (!CheckKeyCodeOpDetail(styleCode, styleSize, styleColor, styleRev, opRevNo, opSerial)) return "";

            //Create string sub folder from Operation Plan master
            var subFolder = styleCode.Substring(0, 3) + "/" + styleCode + "/" +
                            styleCode + styleSize + styleColor + styleRev + "/" +
                            edition + "/" + opRevNo + "/" + opSerial + "/";

            return subFolder;
        }

        public static string CreateStringSubFolder2(string styleCode, string styleSize, string styleColor, string styleRev, string opRevNo, string opSerial, string edition)
        {
            //Check selection style
            if (!CheckKeyCodeOpDetail(styleCode, styleSize, styleColor, styleRev, opRevNo, opSerial)) return "";

            //Create string sub folder from Operation Plan master
            var subFolder = styleCode.Substring(0, 3) + "/" + styleCode + "/" +
                            styleCode + styleSize + styleColor + styleRev + "/" +
                            MapOperationEdition(edition) + "/" + opRevNo + "/" + opSerial + "/";

            return subFolder;
        }

        /// <summary>
        /// Create sub folder ftp (dms)
        /// </summary>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <param name="opSerial"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static string CreateSubFolderDmsFiles(string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo, string opSerial)
        {
            //Check selection style
            if (!CommonUtility.CheckKeyCodeOpDetail(styleCode, styleSize, styleColorSerial, revNo, opRevNo, opSerial)) return "";

            string subFolderFtp = styleCode.Substring(0, 3) + "/" + styleCode + "/" + styleCode + styleSize + styleColorSerial + revNo + "/";

            return subFolderFtp;
        }

        /// <summary>
        /// Create system file name for jig and machine file.
        /// </summary>
        /// <param name="uploadCode"></param>
        /// <param name="amendNo"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string CreatSystemFileName(string uploadCode, string amendNo, string fileExt)
        {
            //Check selection style
            if (string.IsNullOrEmpty(uploadCode) || string.IsNullOrEmpty(amendNo)) return "";

            string sysFileName = uploadCode + amendNo + fileExt.ToLower();

            return sysFileName;
        }

        /// <summary>
        /// SON ADD 3 May 2017 - check file exist on ftp or not.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool CheckExistFtpFile(string url, string userName, string password)
        {
            var isExist = false;
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential(userName, password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                isExist = true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    isExist = false;
                }

            }
            return isExist;
        }

        /// <summary>
        /// Download file from FTP to local server.
        /// </summary>
        /// <param name="fptUrl"></param>
        /// <param name="savingFolder"></param>
        /// Author: Son Nguyen Cao
        public static void DownloadFileFromFtpToSepcificFolder(string fptUrl, string savingFolderPath, FtpInfo ftp)
        {
            try
            {
                //Create FTP Request.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fptUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(ftp.FtpUser, ftp.FtpPass);
                request.UsePassive = true;
                request.UseBinary = true;
                request.EnableSsl = false;

                //Fetch the Response and read it into a MemoryStream object.
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (Stream fileStream = new FileStream(savingFolderPath, FileMode.CreateNew))
                    {
                        responseStream.CopyTo(fileStream);
                    }
                }
            }
            catch (WebException ex)
            {
                throw new Exception((ex.Response as FtpWebResponse).StatusDescription);
            }
        }

        /// <summary>
        /// Count machines, workers, process and calculate operation time.
        /// </summary>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns></returns>
        //Author: Son Nguyen Cao
        public static Opmt CountOperationPlan(string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {

            //Get operation master by id
            var opmtId = new Opmt() { Edition = edition, StyleCode = styleCode, StyleSize = styleSize, StyleColorSerial = styleColorSerial, RevNo = revNo, OpRevNo = opRevNo };
            var objOpmt = OpmtBus.GetOpsMasterByCode(opmtId).FirstOrDefault();

            //Count operation plan.
            var opmtCount = OpmtBus.CountOpDetail(edition, styleCode, styleSize, styleColorSerial, revNo, opRevNo);
            if (opmtCount == null)
            {
                objOpmt.MachineCount = 0;
                objOpmt.ManCount = 0;
                objOpmt.OpCount = 0;
                objOpmt.OpTime = 0;
            }
            else
            {
                //Calculate tack time.                             
                var tackTime = (int)OpdtBus.GetTackTime(objOpmt);
                if (objOpmt.OpTime > tackTime) { objOpmt.OpTime = tackTime; }
                objOpmt.MachineCount = (int)opmtCount.SumMachineCount;
                objOpmt.ManCount = int.Parse(Math.Ceiling((decimal)opmtCount.SumManCount).ToString()); //(int)opmtCount.SumManCount; //MOD) SON
                objOpmt.OpCount = (int)opmtCount.SumOpCount;
                objOpmt.OpTime = tackTime;
            }

            return objOpmt;

        }

        public static async Task UpdateOpmt(Opmt opmt)
        {
            var objOpmt = CountOperationPlan(opmt.Edition, opmt.StyleCode, opmt.StyleSize,
                opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo);

            //Update operation master.
            await Task.Run(() => OpmtBus.UpdateOpMaster(objOpmt));
        }

        /// <summary>
        /// Get Menu Id base on edition
        /// </summary>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string GetMenuIdByEdition(string edition)
        {
            var menuId = string.Empty;
            edition = edition?.Substring(0, 1);
            switch (edition)
            {
                case ConstantGeneric.EditionPdm:
                case ConstantGeneric.EditionOps:
                    menuId = ConstantGeneric.OpManagementMenuId;
                    break;
                case ConstantGeneric.EditionAom:
                    menuId = ConstantGeneric.FactoryMenu;
                    break;
                case ConstantGeneric.EditionMes:
                    menuId = ConstantGeneric.FactoryMenu; //ConstantGeneric.MesMenu; //MOD) SON - 14/Jun/2019
                    break;
            }

            return menuId;
        }

        /// <summary>
        /// Check url is exist or not
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static bool IsExistUrl(string url)
        {
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (response != null) response.Close();
            }

            return true;
        }

        /// <summary>
        /// Checks the edition aom.
        /// </summary>
        /// <param name="edition">The edition.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        private static bool CheckEditionMes(string edition)
        {
            return edition == ConstantGeneric.EditionMes && !string.IsNullOrEmpty(edition);
        }

        /// <summary>
        /// Convert boolane to string 1 or 0
        /// </summary>
        /// <param name="bl"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static string ConvertBoolToString01(bool bl)
        {
            var str = bl ? 1 : 0;
            return str.ToString();
        }

        /// <summary>
        /// Check edition and confirmation status of operation plan. 
        /// </summary>
        /// <param name="actionRole"></param>
        /// <param name="edition"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao.
        public static string CheckRoleEditionConfirmed(string actionRole, string edition, string styleCode, string styleSize, string styleColorSerial, string revNo, string opRevNo)
        {
            // Check role
            if (!CommonMethod.CheckRole(actionRole))
                return ObjectToJson(ConstantGeneric.AlertPermission);

            var opMaster = new Opmt()
            {
                StyleCode = styleCode,
                StyleSize = styleSize,
                StyleColorSerial = styleColorSerial,
                RevNo = revNo,
                OpRevNo = opRevNo,
                Edition = edition
            };
            
            //Check edition
            if (CheckEditionMes(edition))
                return ObjectToJson(ConstantGeneric.AlertEditoinMes);
          
            //Check ops confirmed
            if (CheckOpsMasterConfirmed(opMaster))
                return ObjectToJson(ConstantGeneric.AlertOpsConfirmed);
           
            return ConstantGeneric.False;
        }

        /// <summary>
        /// Map operation edition
        /// </summary>
        /// <param name="edition"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static string MapOperationEdition(string edition)
        {
            string resEdi = string.Empty;
            switch (edition)
            {
                case "P":
                case "PDM":
                    resEdi = "PDM";
                    break;
                case "O":
                case "OPS":
                    resEdi = "OPS";
                    break;
                case "A":
                case "AOM":
                    resEdi = "AOM";
                    break;
                default:
                    break;
            }
            return resEdi;
        }

        #region Send Email
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="ccAddress"></param>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool SendEmailProcess(string toAddress, string ccAddress, string subject, string content)
        {
            try
            {                
                string headContent = "Dear Mr/Ms <br/><br/>";
                string footerContent = "<br/><br/> <b>This is the automatic email, please do not reply. Thanks</b>";
                var emaContent = headContent + content + footerContent;
                var msg = new MailMessage( ConstantGeneric.EmailAddress, toAddress, subject, emaContent);
                msg.CC.Add(ccAddress);
                msg.IsBodyHtml = true;
                var smtpClient = new SmtpClient(ConstantGeneric.SmtpClient, int.Parse(ConstantGeneric.Port))
                {
                    UseDefaultCredentials = true,
                    Credentials = new NetworkCredential(ConstantGeneric.EmailAddress, ConstantGeneric.EmailPassword),
                    EnableSsl = false
                };
                smtpClient.Send(msg);
               
                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}