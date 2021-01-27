using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.DgsEntities
{
    public class Schl
    {
        public string PlnDate { get; set; }
        public string MchnId { get; set; }
        public decimal DlySeq { get; set; }
        public string FctrCd { get; set; }
        public string LineCd { get; set; }
        public string StylNm { get; set; }
        public string PrcsNm { get; set; }
        public string SeatSeq { get; set; }
        public decimal TrgtQty { get; set; }
        public string OprtEmpNo { get; set; }
        public string CrtDttm { get; set; }

        //Adding properties
        public decimal Pending { get; set; }
        public decimal Data { get; set; }

    }
}
