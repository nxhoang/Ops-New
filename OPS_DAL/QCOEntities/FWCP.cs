using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.QCOEntities
{
    public class FWCP
    {
        /* create Time: 2019-10-01
         * creator:     Tai Le (Thomas)
         * Class Name: Factory Weekly Capacity
         */

        public string FACTORY { get; set; }
        public int YEAR { get; set; }
        public int WEEKNO { get; set; }

        public int TOTALWORKERS { get; set; }
        
        public double CAPACITY { get; set; }

        public int TOTALMACHINES { get; set; }

        public DateTime STARTDATE { get; set; }
        public DateTime ENDDATE { get; set; }

        public int TOTALSEWER { get; set; }
        public double SEWERCAPA { get; set; }

        public double TOTALWORKHOUR { get; set; } //2019-10-24

        public string CREATOR { get; set; }
        
        public DateTime CREATETIME { get; set; }


        public FWCP() { }

        protected FWCP(FWCP copy)
        {
            this.FACTORY = copy.FACTORY;
            this.YEAR = copy.YEAR;
            this.WEEKNO = copy.WEEKNO;

            this.TOTALWORKERS = copy.TOTALWORKERS;
            this.CAPACITY = copy.CAPACITY;
            this.TOTALMACHINES = copy.TOTALMACHINES;

            this.STARTDATE = copy.STARTDATE;
            this.ENDDATE = copy.ENDDATE;

            this.TOTALSEWER = copy.TOTALSEWER;
            this.SEWERCAPA = copy.SEWERCAPA;
        }
    }
}
