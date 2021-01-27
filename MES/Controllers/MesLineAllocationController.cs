using Newtonsoft.Json;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using OPS_DAL.MesBus;
using OPS_DAL.MesEntities;
using OPS_Utils;
using PKERP.Base.Domain.Interface.Dto;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MES.Controllers
{
    [SessionTimeout]
    public class MesLineAllocationController : Controller
    {
        private readonly CstpBus _CstpBus = new CstpBus();
        private readonly OPS_DAL.MesBus.FcmtBus _Fcmt = new OPS_DAL.MesBus.FcmtBus();
        // private Cstp _cstp;

        public ActionResult MesLineAllocation()
        {
            ViewBag.PageTitle = "Operation Plan";
           // _cstp = _CstpBus.GetByServerNo(ConstantGeneric.ServerNo).Result;

            return View();
        }

        /// <summary>
        /// Get list of attendance employees
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <returns>items as object includes list of employees</returns>
        public async Task<JsonResult> GetAttendEmpApi(string deptCode, string attDate)
        {
            var getEmployeesApi = ConfigurationManager.AppSettings["GetAttendEmpsApi"];
            var restClient = new RestClient($"{getEmployeesApi}?Deptcode={deptCode}&workdate={attDate}");
            var request = new RestRequest(@"", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

            if (!response.IsSuccessful) return Json(new { IsSuccess = false, Log = response.ErrorMessage }, JsonRequestBehavior.AllowGet);
            var employees = JsonConvert.DeserializeObject<List<eEmployeeDailyWorking>>(response.Content);
            return Json(new { IsSuccess = true, Result = employees }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Getting directly list of attendance employee from K-Tech API
        /// (K-Tech named corporation as factory)
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <returns>string as json</returns>
        public async Task<string> GetAttendEmpsApi()
        {
            var getAttendEmpApi = ConfigurationManager.AppSettings["GetAttendEmpsApi"];
            var restClient = new RestClient(getAttendEmpApi);
            var request = new RestRequest(@"", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

            return response.IsSuccessful ? response.Content : null;
        }

        /// <summary>
        /// Get list of employees from K-Tech API
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <returns>List of employees</returns>
        public async Task<List<eUserInformation>> GetEmployeesFromApi()
        {
            var employees = new List<eUserInformation>();
            var getEmployeesApi = ConfigurationManager.AppSettings["GetEmployeesApi"];
            var restClient = new RestClient(getEmployeesApi);
            var request = new RestRequest(@"", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

            if (response.IsSuccessful)
            {
                employees = JsonConvert.DeserializeObject<List<eUserInformation>>(response.Content);
            }

            return employees;
        }

        /// <summary>
        /// Get list of department teams by corporation
        /// (K-Tech named corporation as factory)
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <returns>string as json</returns>
        public async Task<string> GetDeptTeamByCorpApi(string corp)
        {
            if (string.IsNullOrEmpty(corp)) return null;

            var getDeptTeamApi = ConfigurationManager.AppSettings["GetDeptTeamByCorpApi"];
            var restClient = new RestClient($"{getDeptTeamApi}?factory={corp}");
            var request = new RestRequest(@"", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

            return response.IsSuccessful ? response.Content : null;
        }

        /// <summary>
        /// Get list of corporations from K-Tech API
        /// (K-Tech named corporation as factory)
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <returns>string as json</returns>
        public async Task<string> GetCorpsApi()
        {
            var getCorpApi = ConfigurationManager.AppSettings["GetCorpsApi"];
            var restClient = new RestClient(getCorpApi);
            var request = new RestRequest(@"", Method.GET);
            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

            return response.IsSuccessful ? response.Content : null;
        }

        /// <summary>
        /// Inserting list of employees
        /// Author: Nguyen Xuan Hoang
        /// </summary>
        /// <returns>true/false</returns>
        public async Task<JsonResult> BulkInsertEmployee()
        {
            try
            {
                var users = await GetEmployeesFromApi();
                var empBus = new EmployeeBus();
                var result = await empBus.BulkInsertEmployee(users);

                return Json(new { IsSuccess = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Getting list of employees
        /// </summary>
        /// <param name="corp"></param>
        /// <param name="dept"></param>
        /// <param name="styleCode"></param>
        /// <param name="styleSize"></param>
        /// <param name="styleColorSerial"></param>
        /// <param name="revNo"></param>
        /// <param name="opRevNo"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetOpEmps(string dept, string team, string styleCode, string styleSize,
            string styleColorSerial, string revNo, string opRevNo, string mxPackage)
        {
            try
            {
                if (string.IsNullOrEmpty(ConstantGeneric.ServerNo))
                {
                    return Json(new TaskResult<Cstp>
                    {
                        IsSuccess = false, Log = "ServerNo is not config so can not get CSTP."
                    });
                }
                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);
                var empBus = new EmployeeBus();
                var employees = await empBus.GetOpEmps(cstp.HrmCorpCode, dept, team, styleCode,
                    styleSize, styleColorSerial, revNo, opRevNo, mxPackage);

                return Json(new { IsSuccess = true, Result = employees }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetOpEmpsByTeams(string dept, string[] teams, Opmt opmt)
        {
            try
            {
                if (string.IsNullOrEmpty(ConstantGeneric.ServerNo))
                {
                    return Json(new TaskResult<Cstp>
                    {
                        IsSuccess = false,
                        Log = "ServerNo is not config so can not get CSTP."
                    });
                }
                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);
                var empBus = new EmployeeBus();
                var employees = await empBus.GetOpEmps(cstp.HrmCorpCode, dept, teams, opmt.StyleCode,
                    opmt.StyleSize, opmt.StyleColorSerial, opmt.RevNo, opmt.OpRevNo, opmt.MxPackage);

                return Json(new { IsSuccess = true, Result = employees }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Getting department code (section) by factory
        /// </summary>
        /// <param name="factories">the filtered factories</param>
        /// <returns></returns>
        public async Task<JsonResult> GetDeptCodesByFactory(List<string> factories)
        {
            try
            {
                var deptBus = new DeptBus();
                //var factories = new List<string>();
                //factories.Add(factory);
                var depts = await deptBus.GetByFactory(factories);

                return Json(new { IsSuccess = true, Result = depts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Inserting list of attendance employees
        /// </summary>
        /// <param name="factory">the factory</param>
        /// <param name="deptCode">the department code</param>
        /// <param name="attDate">attendance date</param>
        /// <param name="isPresent">is present (come to work)</param>
        /// <returns></returns>
        public async Task<JsonResult> BulkInsertAttEmp(string factory, string[] selectedTeams, string attDate, bool isPresent)
        {
            try
            {
                var attEmpBus = new AttendEmpBus();
                var employees = new ApiDeserializeObj<eEmployeeDailyWorking>
                {
                    items = new List<eEmployeeDailyWorking>()
                };

                // 1. Checking existing in db, If there are some records, loading to client-side
                var attendEmps = await attEmpBus.GetByFactoryDeptDate(factory, selectedTeams, attDate, isPresent);
                if (attendEmps != null && attendEmps.Count > 0) return Json(new { IsSuccess = true, Result = attendEmps }, JsonRequestBehavior.AllowGet);

                // 2. Loading list of attendance employees from API by factory, department code and attendance date
                var getEmployeesApi = ConfigurationManager.AppSettings["GetAttendEmpsApi"];

                foreach (var selectedTeam in selectedTeams)
                {
                    var restClient = new RestClient($"{getEmployeesApi}?Deptcode={selectedTeam}&workdate={attDate}");
                    var request = new RestRequest(@"", Method.GET);
                    var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

                    if (response.IsSuccessful)
                    {
                        var tempEmps = JsonConvert.DeserializeObject<ApiDeserializeObj<eEmployeeDailyWorking>>(response.Content);

                        // 3. Inserting list of attendance employees to database.
                        if(tempEmps.items.Count > 0) await attEmpBus.BulkInsert(tempEmps.items, factory, selectedTeam, attDate).ConfigureAwait(true);

                        employees.items.AddRange(tempEmps.items);
                    }
                }

                if (employees.items.Count == 0) Json(new FailedTaskResult<List<eEmployeeDailyWorking>>("No worker found"), JsonRequestBehavior.AllowGet);

                //if (response.IsSuccessful)
                //{
                //    employees = JsonConvert.DeserializeObject<ApiDeserializeObj<eEmployeeDailyWorking>>(response.Content);
                //    if (employees.items.Count == 0) Json(new FailedTaskResult<List<eEmployeeDailyWorking>>("No worker found"), JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    var errorMes = string.IsNullOrEmpty(response.ErrorMessage) ? response.Content : response.ErrorMessage;

                //    return Json(new { IsSuccess = false, Log = errorMes }, JsonRequestBehavior.AllowGet);
                //}



                // 4. Loading list of attendance employees after bulk inserted
                var afterwardAttendEmps = await attEmpBus.GetByFactoryDeptDate(factory, selectedTeams, attDate, isPresent);
                if (afterwardAttendEmps != null && afterwardAttendEmps.Count > 0) return Json(new { IsSuccess = true, Result = afterwardAttendEmps }, JsonRequestBehavior.AllowGet);

                //if (result)
                //{
                //    var afterwardAttendEmps = await attEmpBus.GetByFactoryDeptDate(factory, deptCode, attDate, isPresent);
                //    if (afterwardAttendEmps != null && afterwardAttendEmps.Count > 0) return Json(new { IsSuccess = true, Result = afterwardAttendEmps }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new { IsSuccess = false, Log = "Could not bulk insert list of attendance employees that was loaded from API." }, JsonRequestBehavior.AllowGet);
                //}

                return Json(new { IsSuccess = false, Result = new List<AttendEmp>() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> SaveOpEmpChanges(List<Opdt> opdts)
        {

            try
            {
                if (opdts == null || opdts.Count == 0) return Json(new TaskResult<List<Opdt>> { IsSuccess = false, Log = "There are no processes to save" });

                var opdtBus = new OpdtBus();
                var updateOpEmpRs = await opdtBus.SaveOpEmpChanges(opdts);

                return Json(new TaskResult<List<Opdt>> { IsSuccess = updateOpEmpRs });
            }
            catch (Exception e)
            {
                return Json(new TaskResult<List<Opdt>> { IsSuccess = false, Log = e.Message });
            }
        }

        public async Task<JsonResult> BulkInsertDeptTeam()
        {
            try
            {
                var deptTeamBus = new DeptTeamBus();
                ApiDeserializeObj<eDepartmentUnit> departments = new ApiDeserializeObj<eDepartmentUnit>
                {
                    items = new List<eDepartmentUnit>()
                };

                // 1. Checking existing in db, If there are some records, loading to client-side
                //var deptTeams = await deptTeamBus.GetByDeptName("Production");
                //if (deptTeams != null && deptTeams.Count > 0)
                //{
                //    return Json(new { IsSuccess = true, Result = deptTeams }, JsonRequestBehavior.AllowGet);
                //}

                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);
                //cstp.CorpCode = "1006"; Joon Saigon and PK3 are one
                var fcmts = await _Fcmt.GetHrmCorpsByPkCorp(cstp.CorpCode);
                //var hrmCorpCodes = fcmts.Result.GroupBy(x => x.HrmCorpCode).ToList();

                // 2. Loading list of department teams from API by corporation.

                foreach (var hrmCorpCode in fcmts)
                {
                    if (!string.IsNullOrEmpty(hrmCorpCode.HrmCorpCode))
                    {
                        var getDeptTeamsApi = ConfigurationManager.AppSettings["GetDeptTeamByCorpApi"];
                        var restClient = new RestClient($"{getDeptTeamsApi}?factory={hrmCorpCode.HrmCorpCode}");
                        var request = new RestRequest(@"", Method.GET);
                        var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(true);

                        if (response.IsSuccessful)
                        {
                            var depts = JsonConvert.DeserializeObject<ApiDeserializeObj<eDepartmentUnit>>(response.Content);
                            departments.items.AddRange(depts.items);
                        }
                        else
                        {
                            var errorMes = string.IsNullOrEmpty(response.ErrorMessage) ? response.Content : response.ErrorMessage;

                            return Json(new { IsSuccess = false, Log = errorMes }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                // If there are no team, return to failed message.
                if (departments.items.Count == 0)
                {
                    return Json(new FailedTaskResult<List<eDepartmentUnit>>("No department team found"), JsonRequestBehavior.AllowGet);
                }

                // 3. Inserting list of departments to database.
                var delResult = await deptTeamBus.DeleteByTeamCodes(departments.items);

                if (delResult)
                {
                    // Loading list of factories then update PkName and PkTeamCode.
                    var factories = await _Fcmt.GetByCorporation(cstp.CorpCode);
                    //foreach (var factory in factories)
                    //{
                    //    // Updating PkName, PkTeamCode
                    //    var pkFactoryName = factory.Name.Replace("-", "");
                    //    var tempTeam = departments.items.Where(x => x.TEAM_NAME == pkFactoryName);
                        
                    //}

                    var result = await deptTeamBus.BulkInsert(departments.items, factories).ConfigureAwait(true);

                    // 4. Loading list of departments after bulk inserting
                    if (result)
                    {
                        var afterwardTeams = await deptTeamBus.GetByDeptName("Production");
                        if (afterwardTeams != null && afterwardTeams.Count > 0)
                        {
                            return Json(new { IsSuccess = true, Result = afterwardTeams }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            IsSuccess = false,
                            Log = "Could not bulk insert list of department team that was loaded from API."
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { IsSuccess = false, Result = departments.items, Log = "Could not delete teams before insert new teams." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { IsSuccess = false, Result = departments.items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> GetEmpImageUrlConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["WorkerImageUrl"]))
                {
                    return Json(new { IsSuccess = false, Log="Not config employee image path yet." }, JsonRequestBehavior.AllowGet);
                }
                var cstp = await _CstpBus.GetByServerNo(ConstantGeneric.ServerNo);
                if (cstp == null || string.IsNullOrEmpty(cstp.CorpCode))
                {
                    return Json(new { IsSuccess = false, Log = "Not found corporation in cstp table." }, JsonRequestBehavior.AllowGet);
                }

                var url = $"{ConfigurationManager.AppSettings["WorkerImageUrl"]}{cstp.CorpCode}";
                return Json(new { IsSuccess = true, Result = url }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { IsSuccess = false, Log = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}