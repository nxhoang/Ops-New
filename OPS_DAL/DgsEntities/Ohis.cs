using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPS_DAL.DgsEntities
{
    /// <summary>
    /// table t_dg_iot_output_history
    /// </summary>
    public class Ohis
    {
        //public int OUT_SEQ { get; set; }
        //public string PLN_DATE { get; set; }
        //public string MAC_ADDR { get; set; }
        //public string PROCESS_ID { get; set; }
        //public string TYPE { get; set; }
        //public int DATA { get; set; }
        //public float CUR_CYCLE { get; set; }
        //public float SUM_CYCLE { get; set; }
        //public string MES_STS { get; set; }
        //public string CRT_DTTM { get; set; }
        //public string CRT_DTTM_SV { get; set; }
        //public string MCHN_ID { get; set; }

        public long Seq { get; set; }
        public string MacAddress { get; set; }
        public string ExcDttm { get; set; }
        public string ProcessId { get; set; }
        public string Type { get; set; }
        public int Data { get; set; }
        public int CycleTime { get; set; }
        public int SumCycle { get; set; }
        public string MachineId { get; set; }
        public string Factory { get; set; }

    }
}
