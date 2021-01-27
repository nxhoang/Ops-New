using OPS_Utils;
using System.Web;

namespace OPS.GenericClass
{
    /// <summary>
    /// Summary description for DownloadFile
    /// </summary>
    public class DownloadFile : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            HttpRequest request = HttpContext.Current.Request;
            string fileName = request.QueryString["fileName"];
            string expFilePath = HttpContext.Current.Server.MapPath(ConstantGeneric.ExportingFolder + fileName);

            if (!System.IO.File.Exists(expFilePath))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Cannot find the file you are downloading.");
            }
            else
            {
                HttpResponse response = HttpContext.Current.Response;

                response.ClearContent();
                response.Clear();
                response.ContentType = "text/plain";
                response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + ";");
                response.TransmitFile(expFilePath);
                response.Flush();
                response.End();
            }
                       
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}