using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace OPS_DAL.MesEntities
{
    /// <summary>
    /// Mapping to t_hr_empm table
    /// </summary>
    /// Author: Nguyen Xuan Hoang
    public class Employee : OpdtKey
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Factory { get; set; }
        public string CorporationCode { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Skill { get; set; }
        public string SkillLevel { get; set; }
        public DateTime? JoinedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? LastWorkedDate { get; set; }
        public string Status { get; set; }
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }

        public string FullImageUrl
        {
            get
            {
                if (ConfigurationManager.AppSettings["WorkerImageUrl"] == null) return "";
                var url = $"{ConfigurationManager.AppSettings["WorkerImageUrl"]}{CorporationCode}/{ImageName}";
                return url;
            }
        }
    }

    public class ApiDeserializeObj<T>
    {
        public List<T> items { get; set; }
    }

    /// <summary>This object to get data from API</summary>
    /// Author: Nguyen Xuan Hoang
    public class eUserInformation
    {
        public string DEPARTMENT { get; set; }
        public string DEPTCODE { get; set; }
        public string FACTORY { get; set; }
        public string FULL_PATH { get; set; }
        public string GENDER { get; set; }
        public string JOBTYPE { get; set; }
        public string LINE { get; set; }
        public string POSITION { get; set; }
        public string PROCESSTYPE { get; set; }
        public string RESPONSIBILITYNM { get; set; }
        public string SKILL_LEVEL { get; set; }
        public string SYS_EMPID { get; set; }
        public string USER_ID { get; set; }
        public string USER_NAME { get; set; }
    }

    /// <summary>This object matches user info from API</summary>
    public class eEmployeeBasicInfo
    {
        public string DEPARTMENT { get; set; }
        public string DEPTCODE { get; set; }
        public string EMPID { get; set; }
        public string EMPNAME { get; set; }
        public string FACTORY { get; set; }
        public string FULL_PATH { get; set; }
        public string GENDER { get; set; }
        public string IMAGE { get; set; }
        public string JOBTYPE { get; set; }
        public string LEVEL { get; set; }
        public string LINE { get; set; }
        public string POSITION { get; set; }
        public string PROCESSTYPE { get; set; }
        public string RESPONSIBILITYNM { get; set; }
        public string SKILL_LEVEL { get; set; }
        public string SKILL_TYPE { get; set; }
        public string SYS_EMPID { get; set; }
        public string TEAM { get; set; }
    }

    public class eEmployeesKillsImgUrl
    {
        public string DEPTCODE { get; set; }
        public string EMPID { get; set; }
        public string EMPNAME { get; set; }
        public string FULL_PATH { get; set; }
        public string IMG_URL { get; set; }
        public string JOBCODE { get; set; }
        public string JOBTYPE_CODE { get; set; }
        public string JOBTYPE_NAME { get; set; }
        public string PROCESSTYPE_CODE { get; set; }
        public string PROCESSTYPE_NAME { get; set; }
        public string SKILL_CODE { get; set; }
        public string SKILL_NAME { get; set; }
        public string UID_FOR_MES { get; set; }
    }

    public class eDepartmentUnit
    {
        public string CORPORATION_NAME { get; set; }
        public string CORPORATION_CODE { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public string DEPARTMENT_CODE { get; set; }
        public string TEAM_NAME { get; set; }
        public string TEAM_CODE { get; set; }
        public string LINE_NAME { get; set; }
        public string LINE_CODE { get; set; }
    }

    public class eEmployeeDailyWorking
    {
        public string EMPLOYEEID { get; set; }
        public string FULLNAME { get; set; }
        public string ENDTIME130 { get; set; }
        public string CHECKIN { get; set; }
        public string CHECKOUT { get; set; }
        public string DEDUCTDAY { get; set; }
        public string DEDUCTTIMEIN { get; set; }
        public string DEDUCTTIMEOUT { get; set; }
        public string HISLOGDATA { get; set; }
        public string MINUTESLATE { get; set; }
        public string MINUTESSOON { get; set; }
        public string MINU_DEDUCT { get; set; }
        public string NORMAL_150_IN { get; set; }
        public string NORMAL_150_OUT { get; set; }
        public string NORMAL_150_TOTAL { get; set; }
        public string STARTTIME130 { get; set; }
        public string WORKHOUR130 { get; set; }
        public string WORKHOUR390 { get; set; }
        public string WORKKIND { get; set; }
        public string WORKSHIFT { get; set; }
        public string WORKSHIFTCODE { get; set; }
    }

    public class AttEmpGroupDept
    {
        public string Factory { get; set; }
        public string DeptCode { get; set; }
        public List<eEmployeeDailyWorking> Employees { get; set; }

        public AttEmpGroupDept(string factory, string deptCode, List<eEmployeeDailyWorking> employees)
        {
            Factory = factory;
            DeptCode = deptCode;
            Employees = employees;
        }
    }
}
