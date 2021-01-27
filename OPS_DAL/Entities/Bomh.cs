using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    public class Bomh
    {
        public string STYLECODE { get; set; }
        public string STYLESIZE { get; set; }
        public string STYLECOLORSERIAL { get; set; }
        public string REVNO { get; set; }
        public string CADCOLORSERIAL { get; set; }
        public string CARREVNO { get; set; }
        public string CADFILE { get; set; }
        public DateTime? CONFDATE { get; set; }
        public string VENDORCODE { get; set; }
        public DateTime? CREATEDDATE { get; set; }
        public DateTime? MODIFIDATE { get; set; }
        public string NESTINGORDERSTATUS { get; set; }
        public string NESTINGORDERGUID { get; set; }
        public DateTime? NESTINGORDERSENTTIME { get; set; }
        public DateTime? NESTINGORDERRECDTIME { get; set; }
        public string NESTINGTYPE { get; set; }
        public string NESTINGERROR { get; set; }
        public decimal? NESTINGFILETYPE { get; set; }
        public string NESTINGSENDER { get; set; }
        public string MBOM_NESTINSTATUS { get; set; }
        public string MBOM_NESTINGGUID { get; set; }
        public DateTime? MBOM_NESTINSENTTIME { get; set; }
        public DateTime? MBOM_NESTINGRECDTIME { get; set; }
        public string MBOM_NESTINGERROR { get; set; }
        public string MBOM_NESTINGSENDER { get; set; }

    }
}
