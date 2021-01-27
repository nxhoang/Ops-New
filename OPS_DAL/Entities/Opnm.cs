using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to T_OP_OPNM table
    /// </summary>
    /// Author: Son Nguyen Cao
    public class Opnm
    {
        public decimal OpNameId { get; set; }
        public string English { get; set; }
        public string Vietnam { get; set; }
        public string Indonesia { get; set; }
        public string Myanmar { get; set; }
        public string Ethiopia { get; set; }
        public string ProcessName { get; set; } // ADD - HA NGUYEN
        public string GroupLevel { get; set; } 
        public decimal? ParentId { get; set; } 
        public string Code { get; set; } 
        public string MachineGroup { get; set; } 
        public string MachineId { get; set; } 
        public string IconName { get; set; } 
        public string HasChild { get; set; } 
        public decimal GroupLevel_0 { get; set; } 
        public decimal GroupLevel_1 { get; set; } 
        public decimal GroupLevel_2 { get; set; } 
        //Machine name
        public string ItemName { get; set; } 
        public string MchGroupName { get; set; } 
        public string IconLink { 
            get {
                if (!string.IsNullOrEmpty(IconName))  
                    return ConstantGeneric.ProcessIconHost + IconName;
                return IconName;
            }
            set {
                IconLink = value;
            } 
        }

        public int OpNameId2 { get; set; }
        public int OpNameId3 { get; set; }

    }
}
