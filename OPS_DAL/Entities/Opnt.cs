using OPS_Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.Entities
{
    /// <summary>
    /// Refer to t_op_opnt table
    /// </summary>
    /// Author: Son Nguyen Cao.
    public class Opnt:StyleMaster
    {
        public string OpRevNo { get; set; }
        public int OpSerial { get; set; }
        public decimal OpNameId { get; set; }
        public string Edition { get; set; }
        public int OpTime { get; set; }
        public int OpnSerial { get; set; }
        public string OpName { get; set; }
        public string OpNameCode { get; set; }

        //Ha
        public string MachineType { get; set; }
        public string MachineName { get; set; }
        public string ToolName { get; set; }
        public int MachineCount { get; set; }
        public string Remarks { get; set; }
        public int MaxTime { get; set; }
        public float ManCount { get; set; }
        public string ImageName { get; set; }
        public string VideoFile { get; set; }
        public string JobType { get; set; }
        public string ToolId { get; set; }
        public string ActionCode { get; set; }
        public decimal StitchCount { get; set; } //ADD) SON - 25 December 2019
        public string MainProcess { get; set; }
        public int StitchingLength { get; set; }//ADD) SON - 17/Nov/2020
        public int StitchesPerInch { get; set; }//ADD) SON - 17/Nov/2020
        public string IotType { get; set; }//ADD - SON) 23/Dec/2020
        //START ADD - SON) 28/Dec/2020
        public int? GroupLevel_0 { get; set; }
        public int? GroupLevel_1 { get; set; }
        public int? GroupLevel_2 { get; set; }
        //END ADD - SON) 28/Dec/2020
        public string ImageLink { get; set; }
        public string VideoLink
        {
            get { return ConstantGeneric.VideoProcessHttpLink + VideoFile; }
        }
        public string IconName { get; set; }//ADD - SON) 24/Dec/2020
        public string IconLink //ADD - SON) 24/Dec/2020
        {
            get
            {
                if (!string.IsNullOrEmpty(IconName))
                    return ConstantGeneric.ProcessIconHost + IconName;
                return IconName;
            }
        }
        public string HasBom { get; set; } //ADD - SON) 9/Jan/2021
    }
}
