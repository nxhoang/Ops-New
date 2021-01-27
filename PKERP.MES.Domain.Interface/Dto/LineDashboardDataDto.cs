using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKERP.MES.Domain.Interface.Dto
{
    public class LineDashboardDataDto
    {
        public string mxpackage { get; set; }
        public int target { get; set; }
        public int completed { get; set; }

        //estimated completed at this time
        public decimal estcompleted { get; set; }
        public int remain { get; set; }
        public string final_assembly_machine_mac { get; set; }

        //planning
        public DateTime plan_start_date { get; set; }
        public DateTime plan_end_date { get; set; }

        //actual
        public DateTime actual_start_date { get; set; }
        public DateTime actual_end_date { get; set; }
    }
}
