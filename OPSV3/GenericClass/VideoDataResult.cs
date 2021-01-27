using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;

namespace OPS.GenericClass
{
    public class VideoDataResult: ActionResult
    {
        /// <summary>
        /// The below method will respond with the Video file
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteResult(ControllerContext context)
        {

            var strVideoFilePath = HostingEnvironment.MapPath("~/assets/ops-fileupload/videos/process/UNI0037/UNI0037LRG001001001140.mp4");

            context.HttpContext.Response.AddHeader("Content-Disposition", "attachment; filename=Test2.mp4");

            if (strVideoFilePath == null) return;
            var objFile = new FileInfo(strVideoFilePath);

            var stream = objFile.OpenRead();
            var objBytes = new byte[stream.Length];
            stream.Read(objBytes, 0, (int)objFile.Length);
            context.HttpContext.Response.BinaryWrite(objBytes);
        }
    }
}