using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Urmt
    {
        public string USERID { get; set; }
        public string ROLEID { get; set; }
        public string ROLEDESC { get; set; }
        public string FACTORY { get; set; } //SON ADD) 25 Jan 2019
        
        public string CRCODE { get; set; } //TAI LE ADD) 30 Jan 2019  
    }
}
