using OPS_DAL.APIBus;
using OPS_DAL.APIEntities;
using OPS_DAL.Business;
using OPS_DAL.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace OPSAPI.Models
{
    public class UserMasterRepository
    {
        //public UserMaster ValidateUser(string username, string password)
        //{
        //    var apiUser = ConfigurationManager.AppSettings["APIUser"];
        //    var apiPassword = ConfigurationManager.AppSettings["APIPassword"];
        //    var apiRole = ConfigurationManager.AppSettings["APIRole"];
        //    var apiEmail = ConfigurationManager.AppSettings["APIEmail"];

        //    if (username.ToLower() == apiUser.ToLower() && password == apiPassword)
        //        return new UserMaster { UserName = apiUser, UserPassword = apiPassword, UserRoles = apiRole, UserEmailID = apiEmail };
        //    else return null;
        //}

        public UsmtAPI ValidateUser(string username, string password)
        {
            var apiUser = ConfigurationManager.AppSettings["APIUser"];
            var apiPassword = ConfigurationManager.AppSettings["APIPassword"];
            var apiRole = ConfigurationManager.AppSettings["APIRole"];
            var apiEmail = ConfigurationManager.AppSettings["APIEmail"];

            //If user authentication is difference with default api user then check user in database
            //var usmt = new UsmtAPI();
            if (apiUser.ToLower() != username.ToLower())
            {
                var usmt = UsmtAPIBus.GetUserInfo(username.ToUpper());

                //If user information is null
                if (usmt == null) return null;

                //If wrong password
                if (usmt.PASSWD != password) return null;

                //Get user role
                var role = UrmtBus.GetListRole(username.ToUpper()).Find(x=>x.ROLEID == "5551");

                //set role on user information
                usmt.ROLEID = role.ROLEID;

                return usmt;
            }
            else
            {
                if (username.ToLower() == apiUser.ToLower() && password == apiPassword)
                    return new UsmtAPI { USERID = apiUser, PASSWD = apiPassword, ROLEID = apiRole, EMAIL = apiEmail };

                return null;
            }
        }
    }

    //public class UserMaster
    //{
    //    public string UserName { get; set; }
    //    public string UserPassword { get; set; }
    //    public string UserRoles { get; set; }
    //    public string UserEmailID { get; set; }
    //}
}