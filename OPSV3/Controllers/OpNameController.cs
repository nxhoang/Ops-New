using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OPS.Controllers
{
    [SessionTimeout]
    public class OpNameController : Controller
    {
        // GET: OpName
        public ActionResult OpName()
        {
            ViewBag.PageTitle = "Operation Name";

            return View();
        }

        public JsonResult GetOpNames(string groupLevel, string parentId)
        {
            //Set parent Id is -1 if group level is difference with 0 and parent id is empty
            if (groupLevel != "0") parentId = string.IsNullOrEmpty(parentId) ? "-1" : parentId;

            return Json(OpnmBus.GetOpNames(groupLevel, parentId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpNamesOpDetail(string groupLevel, string parentId)
        {
            return Json(OpnmBus.GetOpNamesOpDetail(groupLevel, parentId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMachines(string opType, string opSub, string opDetail)
        {
            //If opSub and opDetail are empty then get group machine only otherwise get specific machine
            if (string.IsNullOrEmpty(opSub) && string.IsNullOrEmpty(opDetail))
            {
                //Get group machines
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            //Get list of machines.
            var listMachines = OtmtBus.GetMachines(opType, opSub, opDetail);

            //If opDetail is not empty then combine with list of machines at level 1
            if (!string.IsNullOrEmpty(opDetail))
            {
                var listMachineLv1 = OtmtBus.GetMachines(opType, opSub, "").Where(mc => mc.GroupLevel_2 == null).ToList();
                listMachines.AddRange(listMachineLv1);
            }

            return Json(listMachines, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetMachineCategories(string opNameId)
        {
            if (string.IsNullOrEmpty(opNameId)) return Json(new List<Mcca>(), JsonRequestBehavior.AllowGet);

            //Get list of machines.
            var listMchCate = MccaBus.GetMachineCategories(opNameId);

            return Json(listMchCate, JsonRequestBehavior.AllowGet);

        }

        public JsonResult UpdateOpNameMachine(string opNameId, string machineId)
        {
            if (string.IsNullOrEmpty(opNameId) || string.IsNullOrEmpty(machineId)) return Json(new FailedTaskResult<string>("OpName Id or Machine Id cannot be emtpy"), JsonRequestBehavior.AllowGet);

            //update machine id
            OpnmBus.UpdateOpNameMachine(opNameId, machineId);

            return Json(new SuccessTaskResult<string>("Updated"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateOpNameMachineGroup(string opNameId, string machineGroup)
        {
            if (string.IsNullOrEmpty(opNameId) || string.IsNullOrEmpty(machineGroup)) return Json(new FailedTaskResult<string>("OpName Id or Machine Group cannot be emtpy"), JsonRequestBehavior.AllowGet);

            //update machine id
            OpnmBus.UpdateOpNameMachineGroup(opNameId, machineGroup);

            return Json(new SuccessTaskResult<string>("Updated"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadOpNameIcon()
        {
            try
            {
                var data = Request.Form;
                string opNameId = data["OpNameId"];
                //Check OpNameId whether is empty or not
                if (string.IsNullOrEmpty(opNameId)) return Json(new FailedTaskResult<string>("OpNameId cannot be empty"), JsonRequestBehavior.AllowGet);
                
                //Get file from request
                var fileContent = Request.Files[0];
                //Get file name
                var fileName = fileContent.FileName;
                string iconName = $"{opNameId}.svg";
                //Generate file path to save
                //string iconPath = Server.MapPath("~/img/opnameicon/") + iconName;
                string iconPath = ConstantGeneric.ProcessIcon + iconName;
                //Delete file if exist
                //if (System.IO.File.Exists(iconPath)) System.IO.File.Delete(iconPath);                             
                //Saving file
                fileContent.SaveAs(iconPath);

                //Update icon name
                OpnmBus.UpdateOpNameIconName(opNameId, iconName);
                return Json(new SuccessTaskResult<string>("Updated"), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new FailedTaskResult<string>(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        #region Make icon and save it
        private bool SaveIconFromHttpPostedFile(HttpPostedFileBase fileContent, string iconPath)
        {
            try
            {
                //Get stream from file content
                Stream streamImg = fileContent.InputStream;
                //Get image from stream
                Image image = Image.FromStream(streamImg);
                //Make icon
                Icon icon = MakeIcon(image, 64, false);

                // Create a stream
                FileStream fs = new FileStream(iconPath, FileMode.OpenOrCreate);
                // Save the icon
                icon.Save(fs);
                // Close the filestream
                fs.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Converts an image into an icon.
        /// </summary>
        /// <param name="img">The image that shall become an icon
        /// <param name="size">The width and height of the icon. Standard
        /// sizes are 16x16, 32x32, 48x48, 64x64.
        /// <param name="keepAspectRatio">Whether the image should be squashed into a
        /// square or whether whitespace should be put around it.
        /// <returns>An icon!!</returns>
        private Icon MakeIcon(Image img, int size, bool keepAspectRatio)
        {
            Bitmap square = new Bitmap(size, size); // create new bitmap
            Graphics g = Graphics.FromImage(square); // allow drawing to it

            int x, y, w, h; // dimensions for new image

            if (!keepAspectRatio || img.Height == img.Width)
            {
                // just fill the square
                x = y = 0; // set x and y to 0
                w = h = size; // set width and height to size
            }
            else
            {
                // work out the aspect ratio
                float r = (float)img.Width / (float)img.Height;

                // set dimensions accordingly to fit inside size^2 square
                if (r > 1)
                { // w is bigger, so divide h by r
                    w = size;
                    h = (int)((float)size / r);
                    x = 0; y = (size - h) / 2; // center the image
                }
                else
                { // h is bigger, so multiply w by r
                    w = (int)((float)size * r);
                    h = size;
                    y = 0; x = (size - w) / 2; // center the image
                }
            }

            // make the image shrink nicely by using HighQualityBicubic mode
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, x, y, w, h); // draw image with specified dimensions
            g.Flush(); // make sure all drawing operations complete before we get the icon

            // following line would work directly on any image, but then
            // it wouldn't look as nice.
            return Icon.FromHandle(square.GetHicon());
        }
        #endregion
    }
}