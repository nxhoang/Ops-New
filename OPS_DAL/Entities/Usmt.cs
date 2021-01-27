using System;
using System.ComponentModel.DataAnnotations;

namespace OPS_DAL.Entities
{
    public class Usmt
    {
        [Required(ErrorMessage = "Please enter user name.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public string RoleID { get; set; }

        public string Name { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Sex { get; set; }
        public string Url { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; }

        //2020-08-05 Tai Le(Thomas)
        public string[] Roles { get; set; }
    }
}
