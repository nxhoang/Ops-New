using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.APIEntities
{
    public class UsmtAPI
    {
        public string USERNAME { get; set; }
        public string USERID { get; set; }
        public string PASSWD { get; set; }        
        public string ROLEID { get; set; }
        public string NAME { get; set; }
        public string TEL { get; set; }
        public string EMAIL { get; set; }
        public string COMPCODE { get; set; }
        public string DEPTCODE { get; set; }
        public string SEX { get; set; }
        public string STATUS { get; set; }
        public string TYPE { get; set; }
        public DateTime? EXPIRYDATE { get; set; }
    }
}
