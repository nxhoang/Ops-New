using Newtonsoft.Json;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class EmployeeController : Controller
    {
        private readonly EmployeeBus _EmployeeBus = new EmployeeBus();
        private readonly DeptTeamBus _deptTeamBus = new DeptTeamBus();
        private readonly CstpBus _CstpBus = new CstpBus();
        //private readonly FcmtBus _Fcmt = new FcmtBus();

        public ActionResult Index()
        {
            ViewBag.TestMess = "Message from Controller";
            return View();
        }

        public async Task<List<eEmployeeBasicInfo>> GetEmployeesByTeamApi(string corporation, string department, string team)
        {
            var getEmployeesApi = ConfigurationManager.AppSettings["GetEmployeesByTeamApi"];
            var restClient = new RestClient($"{getEmployeesApi}?factory={corporation}&department={department}&team={team}");
            var request = new RestRequest(@"", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

            if (!response.IsSuccessful) return new List<eEmployeeBasicInfo>();
            var tempEmployees = JsonConvert.DeserializeObject<ApiDeserializeObj<eEmployeeBasicInfo>>(response.Content);

            return tempEmployees.items;
        }

        public async Task<JsonResult> BulkInsertEmployeeByDept()
        {
            try
            {
                // Getting local server info (cstp)
                if (string.IsNullOrEmpty(ConstantGeneric.ServerNo))
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "ServerNo is not config so can not get CSTP."
                    });
                }
                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);

                //var fcmts = _Fcmt.GetByCorporation(cstp.CorpCode);
                //var hrmCorpCodes = fcmts.Result.GroupBy(x => x.HrmCorpCode);

                // Getting list of department teams as production teams
                var deptTeams = await _deptTeamBus.GetByDeptName("Production");
                if (deptTeams == null || deptTeams.Count == 0)
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "There are not any departments in DB, consider synchronize from API."
                    });
                }

                var users = new List<eEmployeeBasicInfo>();
                var empBus = new EmployeeBus();
                //var result = false;

                foreach (var deptTeam in deptTeams)
                {
                    var tempUsers = await GetEmployeesByTeamApi(cstp.HrmCorpCode, deptTeam.DeptCode,
                        deptTeam.TeamCode);
                    users.AddRange(tempUsers);
                    if (tempUsers.Count > 0)
                    {
                        // Delete list of employees
                        var delEmpsResult = await empBus.DeleteEmployees(tempUsers);

                        // Insert list of employees
                        if (delEmpsResult)
                        {
                            await empBus.BulkInsertEmployee(tempUsers, cstp.HrmCorpCode, deptTeam.TeamCode);
                        }
                        else
                        {
                            return Json(new { IsSuccess = false, Log = "Could not delete any employees." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                if (users.Count == 0)
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "There are not any employees from API."
                    });
                }

                return Json(new { IsSuccess = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetEmployees(string corp, List<DeptEntity> depts, string[] deptNames,
            string[] positions)
        {
            try
            {
                var emps = await _EmployeeBus.GetEmployees(corp, depts, deptNames, positions);

                return Json(new TaskResult<List<Employee>> { IsSuccess = true, Result = emps });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<Employee>> { IsSuccess = false, Log = e.Message });
            }
        }

        public async Task<JsonResult> GetDeptTeams()
        {
            try
            {
                var deptTeams = await _deptTeamBus.GetByDeptName("Production");

                return Json(new TaskResult<List<DeptTeam>> { IsSuccess = true, Result = deptTeams });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<DeptTeam>> { IsSuccess = false, Log = e.Message });
            }
        }

        public async Task<JsonResult> SyncEmpImgByCondition(List<DeptEntity> depts, string[] deptNames,
            string[] positions)
        {
            try
            {
                // Getting local server info (cstp)
                if (string.IsNullOrEmpty(ConstantGeneric.ServerNo))
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "ServerNo is not config so can not get CSTP."
                    });
                }

                // Checking configuration folder of employee image
                if (string.IsNullOrEmpty(ConstantGeneric.EmpImageFolder))
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "Not config employee folder yet in Web.config."
                    });
                }

                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);
                var emps = await _EmployeeBus.GetEmployees(cstp.HrmCorpCode, depts, deptNames, positions);
                var downloadedResults = new List<TaskResult<string>>();

                if (emps != null && emps.Count > 0)
                {
                    var empImageFolder = Server.MapPath(ConstantGeneric.EmpImageFolder);
                    var fullPathImg = Path.Combine(empImageFolder, cstp.HrmCorpCode);

                    if (!Directory.Exists(fullPathImg)) Directory.CreateDirectory(fullPathImg);

                    foreach (var employee in emps)
                    {
                        downloadedResults.AddRange(await SaveEmpImage(employee, fullPathImg));
                    }
                }

                return Json(new TaskResult<List<TaskResult<string>>> { IsSuccess = true, Result = downloadedResults });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<Employee>> { IsSuccess = false, Log = e.Message });
            }
        }

        public async Task<JsonResult> SyncEmpImg()
        {
            try
            {
                // Getting local server info (cstp)
                if (string.IsNullOrEmpty(ConstantGeneric.ServerNo))
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "ServerNo is not config so can not get CSTP."
                    });
                }

                // Checking configuration folder of employee image
                if (string.IsNullOrEmpty(ConstantGeneric.EmpImageFolder))
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "Not config employee folder yet in Web.config."
                    });
                }

                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);
                var emps = await _EmployeeBus.GetEmployees(null, null, null, null);
                var downloadedResults = new List<TaskResult<string>>();

                if (emps != null && emps.Count > 0)
                {
                    var empImageFolder = Server.MapPath(ConstantGeneric.EmpImageFolder);
                    var fullPathImg = Path.Combine(empImageFolder, cstp.HrmCorpCode);

                    if (!Directory.Exists(fullPathImg)) Directory.CreateDirectory(fullPathImg);

                    foreach (var employee in emps.Where(employee => string.IsNullOrEmpty(employee.ImageName)))
                    {
                        downloadedResults.AddRange(await SaveEmpImage(employee, fullPathImg));
                    }
                }

                return Json(new TaskResult<List<TaskResult<string>>> { IsSuccess = true, Result = downloadedResults });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<Employee>> { IsSuccess = false, Log = e.Message });
            }
        }

        public async Task<List<TaskResult<string>>> SaveEmpImage(Employee employee, string empImageFolder)
        {
            //var isDownloadedImage = false;
            var rs = new List<TaskResult<string>>();

            //if (employee.ImageUrl == "118.69.83.197:8011") return rs;
            //if (employee.ImageUrl.ToLower(CultureInfo.CurrentCulture).Contains("png")) imgName = $"{employee.EmployeeCode}.png";
            //if (employee.ImageUrl.ToLower(CultureInfo.CurrentCulture).Contains("jpg")) imgName = $"{employee.EmployeeCode}.jpg";
           
            if (employee.ImageUrl != null)
            {
                var request = WebRequest.Create($"http://{employee.ImageUrl}");
                request.Method = "HEAD";
                try
                {
                    var imgExt = Path.GetExtension(employee.ImageUrl);
                    var imgName = $"{employee.EmployeeCode}{imgExt}";
                    var fullPathImg = Path.Combine(empImageFolder, imgName);

                    request.GetResponse();
                    var rsSaveImg = await SaveImage(fullPathImg, $"http://{employee.ImageUrl}");
                    if (!string.IsNullOrEmpty(imgName)) await _EmployeeBus.UpdateImageName(employee.EmployeeCode, imgName);
                    rsSaveImg.Result = employee.EmployeeCode;

                    rs.Add(rsSaveImg);
                }
                catch (Exception ex)
                {
                    // If the image could not be loaded, updating imagename = null as order to load as default images.
                    // ImageName = employee.Gender == "Male" ? "male-user.png" : "female-user.png";
                    await _EmployeeBus.UpdateImageName(employee.EmployeeCode, null);

                    rs.Add(new TaskResult<string> { IsSuccess = false, Log = ex.Message, Result = employee.EmployeeCode });
                }
            }
            else
            {
                rs.Add(new TaskResult<string> { IsSuccess = false, Log = "No API image URL.", Result = employee.EmployeeCode });
            }

            //rs = isDownloadedImage ? new TaskResult<string> { IsSuccess = true, Log = $"Success: {employee.EmployeeCode}" } :
            //    new TaskResult<string> { IsSuccess = false, Log = $"Failed: {employee.EmployeeCode}" };

            return rs;
        }

        public async Task<TaskResult<string>> SaveImage(string fileName, string imgUrl)
        {
            try
            {
                var client = new WebClient();
                var stream = await client.OpenReadTaskAsync(imgUrl);
                if (stream != null)
                {
                    var bitmap = new Bitmap(stream);
                    bitmap.Save(fileName);
                    stream.Flush();
                    stream.Close();
                }

                client.Dispose();

                return new TaskResult<string> { IsSuccess = true };
            }
            catch (Exception e)
            {
                return new TaskResult<string> { IsSuccess = false, Log = e.Message };
            }
        }

        public async Task<JsonResult> GetEmpNoImage()
        {
            try
            {
                var emps = await _EmployeeBus.GetEmpNoImage();

                return Json(new TaskResult<int> { IsSuccess = true, Result = emps.Count });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<Employee>> { IsSuccess = false, Log = e.Message });
            }
        }

        public async Task<List<eEmployeesKillsImgUrl>> GetEmpNfcIdApi(string corporation)
        {
            var getGetEmpNfcIdApi = ConfigurationManager.AppSettings["GetEmpNfcIdApi"];
            var restClient = new RestClient($"{getGetEmpNfcIdApi}?corpcode={corporation}");
            var request = new RestRequest(@"", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

            if (!response.IsSuccessful) return new List<eEmployeesKillsImgUrl>();
            var employees = JsonConvert.DeserializeObject<ApiDeserializeObj<eEmployeesKillsImgUrl>>(response.Content);

            return employees.items;
        }

        public async Task<JsonResult> SynchronizeEmpNfcId()
        {
            try
            {
                // Getting local server info (cstp)
                if (string.IsNullOrEmpty(ConstantGeneric.ServerNo))
                {
                    return Json(new TaskResult<eEmployeeBasicInfo>
                    {
                        IsSuccess = false,
                        Log = "ServerNo is not config so can not get CSTP."
                    });
                }
                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);

                if (cstp.HrmCorpCode == null)
                {
                    return Json(new TaskResult<List<eEmployeesKillsImgUrl>> { IsSuccess = false, Log = "There is not corporation code is mapped to API corporation code." });
                }

                var emps = await GetEmpNfcIdApi(cstp.HrmCorpCode);
                var updateNfcIdResult = await _EmployeeBus.UpdateNfcId(emps);

                //return Json(new TaskResult<List<eEmployeesKillsImgUrl>> { IsSuccess = true, Result = emps }, JsonRequestBehavior.AllowGet);
                return Json(new TaskResult<List<eEmployeesKillsImgUrl>> { IsSuccess = updateNfcIdResult, Result = emps.Count > 1000 ? emps.GetRange(0, 1000) : emps });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<eEmployeesKillsImgUrl>> { IsSuccess = false, Log = e.Message });
                //return Json(new TaskResult<List<eEmployeesKillsImgUrl>> { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}