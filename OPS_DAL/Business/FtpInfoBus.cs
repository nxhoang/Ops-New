using OPS_DAL.DAL;
using OPS_DAL.Entities;
using OPS_Utils;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace OPS_DAL.Business
{
    public class FtpInfoBus
    {
        public static FtpInfo GetHostInfo()
        {
            return GetFtpInfo("PLMHOST");
        }

        public static string GetSubUrl()
        {
            try
            {
                var inf = GetHostInfo();
                return inf.FtpLink + inf.FtpFolder + "/";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the FTP information.
        /// </summary>
        /// <param name="appType">Type of the application.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static FtpInfo GetFtpInfo(string appType)
        {
            var cursor = new OracleParameter("OUT_CURSOR", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
            List<OracleParameter> oracleParams = new List<OracleParameter>
            {
                new OracleParameter("P_APPTYPE", appType),
                cursor
            };
            var ftpInfo = OracleDbManager.GetObjects<FtpInfo>("SP_PLM_GETBYAPPTYPE_PFTP", CommandType.StoredProcedure, oracleParams.ToArray());

            return ftpInfo.FirstOrDefault();
        }

        /// <summary>
        /// Gets the FTP information.
        /// </summary>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static FtpInfo GetFtpInfo()
        {
            try
            {
                var objFtp = GetFtpInfo(ConstantGeneric.FtpAppTypePkMain);
                if (objFtp == null) return null;
                objFtp.FtpRoot = objFtp.FtpLink + objFtp.FtpFolder + "/";

                return objFtp;
            }
            catch
            {
                return null;
            }
        }

        public bool UploadFTP(byte[] fileBytes, string ftpurl, string user, string pass)
        {
            try
            {
                if (fileBytes.Length == 0) return true;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpurl);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                //Enter FTP Server credentials.
                request.Credentials = new NetworkCredential(user, pass);
                request.ContentLength = fileBytes.Length;
                request.UsePassive = true;
                request.UseBinary = false;
                request.ServicePoint.ConnectionLimit = fileBytes.Length;
                request.EnableSsl = false;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                    requestStream.Close();
                }

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
                return true;

            }
            catch (Exception exc)
            {
                var x = exc.ToString();
                return false;
            }

        }

        /// <summary>
        /// Uploads the file to FTP server.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <param name="ftpFullPathFile">The FTP full path file.</param>
        /// <param name="ftp">The FTP.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public bool UploadFileToFtpFromClient(HttpFileCollection files, string ftpFullPathFile, FtpInfo ftp)
        {
            try
            {
                for (var i = 0; i < files.Count; i++)
                {

                    HttpPostedFile file = files[i];

                    //Upload file to FTP server. 
                    byte[] fileBytes;

                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        fileBytes = binaryReader.ReadBytes(file.ContentLength);
                    }

                    //var ftpFile = "ftp://203.113.151.204/BETAPDM/ADI/ADI0422/ADI0422RGL003003/ADI0422RGL003003005002.png";
                    //Create FTP Request.
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFullPathFile);
                    request.Method = WebRequestMethods.Ftp.UploadFile;

                    //Enter FTP Server credentials.
                    request.Credentials = new NetworkCredential(ftp.FtpUser, ftp.FtpPass);
                    request.ContentLength = fileBytes.Length;
                    request.UsePassive = true;
                    request.UseBinary = true;
                    request.ServicePoint.ConnectionLimit = fileBytes.Length;
                    request.EnableSsl = false;

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(fileBytes, 0, fileBytes.Length);
                        requestStream.Close();
                    }

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();

                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uploads the file to FTP from source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="ftpFullPath">The FTP full path.</param>
        /// <param name="ftp">The FTP.</param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static bool UploadFileToFtpFromSource(string source, string ftpFullPath, FtpInfo ftp)
        {
            try
            {
                //string filename = Path.GetFileName(source);
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullPath);
                ftpRequest.Credentials = new NetworkCredential(ftp.FtpUser, ftp.FtpPass);

                ftpRequest.KeepAlive = true;
                ftpRequest.UseBinary = true;
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                FileStream fs = File.OpenRead(source);
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();

                Stream ftpstream = ftpRequest.GetRequestStream();
                ftpstream.Write(buffer, 0, buffer.Length);
                ftpstream.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task UploadFileToFtpServer(string source, string ftpFullPath, FtpInfo ftpInfo)
        {
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFullPath);
            ftpRequest.Credentials = new NetworkCredential(ftpInfo.FtpUser, ftpInfo.FtpPass);

            ftpRequest.KeepAlive = true;
            ftpRequest.UseBinary = true;
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            FileStream fs = File.OpenRead(source);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();

            Stream ftpStream = ftpRequest.GetRequestStream();
            await ftpStream.WriteAsync(buffer, 0, buffer.Length);
            ftpStream.Close();
        }

        /// <summary>
        /// Deletes the FTP file.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// Author: Nguyen Xuan Hoang
        public static bool DeleteFtpFile(string source, string username, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(source);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Create FTP directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// Author: Son Nguyen Cao
        public static void CreateFTPDirectory(FtpInfo ftpInfo, string subFolder)
        {
            string[] subDirs = subFolder.Split('/');
            string currentDir = ftpInfo.FtpRoot;
            foreach (string subDir in subDirs)
            {
                currentDir = currentDir + "/" + subDir;
                try
                {
                    //create the directory
                    FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(new Uri(currentDir));
                    requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                    requestDir.Credentials = new NetworkCredential(ftpInfo.FtpUser, ftpInfo.FtpPass);
                    requestDir.UsePassive = true;
                    requestDir.UseBinary = true;
                    requestDir.KeepAlive = false;
                    FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                    Stream ftpStream = response.GetResponseStream();

                    ftpStream.Close();
                    response.Close();

                }
                catch (WebException ex)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        response.Close();
                    }
                    else
                    {
                        response.Close();
                    }
                }
            }

        }

    }
}
