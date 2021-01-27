using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace OPS.Models
{
    public class UploadFtp
    {
        public bool UploadFileToFtpServer(HttpFileCollection files, string ftpFullPathFile, FtpInfo ftp)
        {
            for (int i = 0; i < files.Count; i++)
            {
                HttpPostedFile file = files[i];
                //Upload file to FTP server. 
                byte[] fileBytes;
                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    fileBytes = binaryReader.ReadBytes(file.ContentLength);
                }

                //var ftpFile = "ftp://203.113.151.204/BETAPDM/ADI/ADI0422/ADI0422RGL003003/ADI0422RGL003003005002.png";
                
                // Get the object used to communicate with the server.  
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpFullPathFile);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.  .
                request.Credentials = new NetworkCredential(ftp.Username, ftp.Password);

                // Copy the contents of the file to the request stream.  
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
            return true;
        }
    }

    public class FtpInfoFunc
    {
        internal static bool FtpUpload(byte[] fileBytes, string ftpurl, string user, string pass)
        {
            if (fileBytes.Length == 0) return true;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpurl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
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

        internal static bool FtpUpload(string urlFile, string ftpurl)
        {
            FtpInfo f = new FtpInfo();
            byte[] fileBytes = File.ReadAllBytes(urlFile);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpurl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(f.Username, f.Password);
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
            return true;
        }
        
        /// <summary>
        /// Upload a file to Ftp
        /// </summary>
        /// <param name="filename">Url file</param>
        /// <param name="ftpurl">fpt url</param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        internal static bool FtpUpload(string filename, string ftpurl, string user, string pass)
        {
            byte[] fileBytes = File.ReadAllBytes(filename);
            //Create FTP Request.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpurl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            //Enter FTP Server credentials.
            request.Credentials = new NetworkCredential(user, pass);
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
            return true;
        }

        internal static bool CreateFtpFolder(string subFolders)
        {
            FtpInfo f = new FtpInfo();
            string url = f.FtpRoot + subFolders;
            FtpWebRequest reqFTP = null;
            Stream ftpStream = null;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(url);
            reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
            reqFTP.UseBinary = true;
            reqFTP.Credentials = new NetworkCredential(f.Username, f.Password);
            FtpWebResponse response1 = (FtpWebResponse)reqFTP.GetResponse();
            ftpStream = response1.GetResponseStream();
            ftpStream.Close();
            response1.Close();
            return true;
        }

        /// <summary>
        /// Create a list Folders from list Subfolders
        /// </summary>
        /// <param name="root">url ftp</param>
        /// <param name="subFolders">list Folder will create</param>
        /// <param name="username"></param>
        /// <param name="pass"></param>
        internal static void CreateFtpFolder(string root, string subFolders, string username, string pass)
        {
            String[] folders = subFolders.Split('/');
            string des = root;
            //create subfolder follow CAD properties
            for (int i = 0; i <= folders.Length - 1; i++)
            {
                des = des + "/" + folders[i];
                try
                {
                    //make folder
                    FtpWebRequest reqFtp = null;
                    Stream ftpStream = null;
                    reqFtp = (FtpWebRequest)FtpWebRequest.Create(des);
                    reqFtp.Method = WebRequestMethods.Ftp.MakeDirectory;
                    reqFtp.UseBinary = true;
                    reqFtp.Credentials = new NetworkCredential(username, pass);
                    FtpWebResponse response1 = (FtpWebResponse)reqFtp.GetResponse();
                    ftpStream = response1.GetResponseStream();
                    ftpStream.Close();
                    response1.Close();
                }
                catch
                {
                    continue;
                }
            }
        }

        internal static bool DeleteFtpfile(string source, string username, string password)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(source);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
            return true;
        }

        internal static bool DeleteAllFile_FTP(string source, string username, string password)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(source);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string names = reader.ReadToEnd();
            reader.Close();
            response.Close();
            string[] files = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            bool ret = true;
            for (int i = 0; i <= files.Length - 1; i++)
            {
                string delFile = source + "/" + files[i];
                ret = DeleteFtpfile(delFile, username, password);
            }
            return ret;
        }

        internal static List<string> GetAllFileName_FTP(string source, string username, string password)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(source);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string names = reader.ReadToEnd();
            reader.Close();
            response.Close();
            string[] files = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return files.ToList();
        }

        internal static Stream GetFtpFile(string strUrl, string username, string pass)
        {
            FtpWebResponse objResponse = null;
            FtpWebRequest objRequest = (FtpWebRequest)WebRequest.Create(strUrl);
            objRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            objRequest.Credentials = new NetworkCredential(username, pass);
            objResponse = (FtpWebResponse)objRequest.GetResponse();
            return objResponse.GetResponseStream();
        }

        internal static bool DeleteDir(string remoteFolder, string username, string pass)
        {
            List<String> allfile = GetAllFileName_FTP(remoteFolder, username, pass);
            foreach (string filename in allfile)
            {
                DeleteFtpfile(remoteFolder + "/" + filename, username, pass);
            }
            /* Create an FTP Request */
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(remoteFolder);
            ftpRequest.Credentials = new NetworkCredential(username, pass);
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
            FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            ftpResponse.Close();
            return true;
        }
    }
}